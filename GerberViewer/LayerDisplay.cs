using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GerberLibrary.Core;
using GerberLibrary;
using System.Drawing.Drawing2D;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GerberViewer
{
    public partial class LayerDisplay : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public LoadedStuff Document;
        public BoardSide DisplaySide;
        public LoadedStuff.DisplayGerber DispGerb;
        Bitmap Cache;
        GerberVBO VBOCache = new GerberVBO();
        bool VBOCacheDirty = true;
        private GerberViewerMainForm MainForm;
        ShaderProgram MainShader;

        public LayerDisplay(LoadedStuff doc, BoardSide Side, GerberViewerMainForm _Owner)
        {

            MainForm = _Owner;
            DisplaySide = Side;
            Document = doc;
            this.Load += LayerDisplay_Load;

            InitializeComponent();


            AddGLControl();
            CloseButton = false;
            CloseButtonVisible = false;
        }

        private void AddGLControl()
        {
            glcontrol1 = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 4));
            glcontrol1.Dock = DockStyle.Fill;
            this.glcontrol1.Size = new System.Drawing.Size(632, 295);
            this.glcontrol1.TabIndex = 1;
            this.glcontrol1.VSync = false;
            this.glcontrol1.KeyDown += LayerDisplay_KeyDown;
            this.glcontrol1.KeyPress += LayerDisplay_KeyPress;
            this.glcontrol1.MouseEnter += pictureBox1_MouseEnter;
            this.glcontrol1.MouseMove += pictureBox1_MouseMove;
            this.glcontrol1.MouseLeave += pictureBox1_MouseLeave;

            glcontrol1.Paint += Glcontrol1_Paint;

            this.Controls.Add(glcontrol1);

        }


        string vert =
@"
#version 330
 
in vec3 vPosition;
in vec4 vColor;
in vec3 vOff;

out vec4 color;

uniform mat4 trans;
uniform mat4 view;
uniform float linescale;
 
void main()
{
    float s = 1.0;
    if (vColor.a < 0) s = 2; 
    gl_Position = view  * trans * vec4(vPosition + vOff * linescale * s, 1.0);
    color = vColor;
}";


        string frag =
@"
#version 330
 
in vec4 color;
out vec4 outputColor;
 
float checker(in float u, in float v)
{
    float checkSize = 3;
    float fmodResult = mod(floor(checkSize * u) + floor(checkSize * v), 2.0);

    if (fmodResult < 1.0) 
    {
        return 1.0;
    } 
    else 
    {
        return 0.0;
    }
}

void main()
{
    if (color.a< 0)
    {
        outputColor = vec4(color.xyz, -color.a);
    }
    else
    {
        outputColor = color;
    }
}

";

        private void LayerDisplay_Load(object sender, EventArgs e)
        {
            glLoaded = true;
            glcontrol1.MakeCurrent();
            MainShader = new ShaderProgram(vert, frag , false);
            glcontrol1.Invalidate();

        }

        private void Glcontrol1_Paint(object sender, PaintEventArgs e)
        {
            if (!glLoaded) return;
            Bounds Bounds = new Bounds();
            foreach (var a in Document.Gerbers.OrderBy(x => x.sortindex))
            {
                Bounds.AddBox(a.File.BoundingBox);
            }

            if (VBOCacheDirty)
            {
                VBOCache.Reset();
                DrawGerbersToGraphicsInterface(Bounds, VBOCache);
             //   VBOCache.DrawLine(new Pen(Color.White, 1), Bounds.TopLeft.ToF(), Bounds.BottomRight.ToF());
                VBOCache.BuildVBO();
                VBOCacheDirty = false;
            }

            GLGraphicsInterface GI = new GLGraphicsInterface(0, 0, Width, Height);
            glcontrol1.MakeCurrent();
            GL.MatrixMode(MatrixMode.Projection);

            GL.LoadIdentity();
            GL.Disable(EnableCap.CullFace);

            GL.Ortho(0, glcontrol1.Width, glcontrol1.Height, 0, -100, 100);
            GL.LineWidth(1.0f);
            //GL.Scale(0.01, 0.01, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Viewport(0, 0, glcontrol1.Width, glcontrol1.Height);

            Matrix4 View = Matrix4.CreateOrthographicOffCenter(0, glcontrol1.Width, glcontrol1.Height, 0, -100,100);
            //            GI.Clear(Color.Yellow);
            GI.Clear(Document.Colors.BackgroundColor);

            //            GGI.Clear(Document.Colors.BackgroundColor);
            float S = GetScaleAndBuildTransform(GI, Bounds);
            MainShader.Bind();
            var M = GI.GetGlMatrix();
            GL.Uniform1(MainShader.Uniforms["linescale"].address, 1.0f/S);
            GL.UniformMatrix4(MainShader.Uniforms["trans"].address, false, ref M);
            GL.UniformMatrix4(MainShader.Uniforms["view"].address, false, ref View);
             GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            VBOCache.RenderVBO(MainShader);
            MainShader.UnBind();
            //DrawGerbersToGraphicsInterface(Bounds, GI);


            {
                if (Document.CrossHairActive)
                {
                    if (Document.Gerbers.Count > 0)
                    {
                        float S2 = GetScaleAndBuildTransform(GI, Bounds);


                        Color DimensionColor = Color.FromArgb(255, 255, 200);
                        Pen P = new Pen(DimensionColor, 1.0f);

                        P.DashPattern = new float[2] { 2, 2 };

                        GI.DrawLine(P, (float)Bounds.TopLeft.X - 1000, Document.MouseY, (float)Bounds.BottomRight.X + 1000, Document.MouseY);
                        GI.DrawLine(P, (float)Document.MouseX, (float)Bounds.TopLeft.Y - 1000, (float)Document.MouseX, (float)Bounds.BottomRight.Y + 1000);


                        //DrawLabel(G2, String.Format("{0:N2}", Document.MouseX - Bounds.TopLeft.X), S, 12, DimensionColor, 5, 0, (float)Document.MouseX, (float)Bounds.TopLeft.Y, DisplaySide == BoardSide.Bottom);
                        //DrawLabel(G2, String.Format("{0:N2}", Document.MouseY - Bounds.TopLeft.Y), S, 12, DimensionColor, 0, -14, (float)Bounds.TopLeft.X, (float)Document.MouseY, DisplaySide == BoardSide.Bottom);
                        //DrawUpsideDown(G2, String.Format("{0:N2}", Document.MouseX), S, 12, Color.Yellow, 5 / S + (float)Document.MouseX, (float)Bounds.TopLeft.Y);


                    }
                }
            }

            glcontrol1.SwapBuffers();
        }

        public LayerDisplay(LoadedStuff doc, LoadedStuff.DisplayGerber Gerb, GerberViewerMainForm _Owner)
        {
            MainForm = _Owner;
            CloseButton = false;

            CloseButtonVisible = false;

            DispGerb = Gerb;
            DisplaySide = Gerb.File.Side;
            Document = doc;
            this.Load += LayerDisplay_Load;
            InitializeComponent();
            AddGLControl();

        }

        public float Zoomlevel = 1.0f;

        public PointF Offset = new PointF();
        private bool MouseHovering;
        private int lastY;
        private int lastX;
        private GLControl glcontrol1;
        private bool glLoaded = false;

        public void UpdateDocument(bool force = false)
        {
            bool DoInvalidate = force;
            // if (this.DockPanel.Visible) { DoInvalidate = true; Console.Write("dockpanel visible - "); }
            if (this.DockPanel.ActiveDocument == this) { DoInvalidate = true; };// Console.Write("dockpane = this - "); }
            // if (this.Pane.IsActivated) { DoInvalidate = true; Console.Write("Activated - "); }
            //    if (this.Pane.IsActivePane) { DoInvalidate = true; Console.Write("ActivePane - "); }
            if (DispGerb == null) DoInvalidate = true;

            if (DoInvalidate)
            {
                if (DispGerb != null)
                {
                    //          Console.WriteLine("invalidating {0}", DispGerb.File);
                }
                else
                {
                    //Console.WriteLine("invalidating {0}", DisplaySide);
                }
                if (glcontrol1 != null) glcontrol1.Invalidate();
                //pictureBox1.Invalidate();
            }
            else
            {
                //  Console.WriteLine("Skipping ");
            }
        }



        private void DrawGerber(GerberVBO G, ParsedGerber file, Color C, bool dotted = false)
        {

            Pen P = new Pen(C, 1.0f );
            if (dotted) P.DashPattern = new float[2] { 2, 2 };
            SolidBrush B = new SolidBrush(C);
            //GraphicsPath GP = new GraphicsPath();
            var Out = file.IsOutline();
            G.Dotted = dotted;
            int Vbefore = G.VertexCount();
            foreach (var a in file.DisplayShapes)
            {

                if (a.Vertices.Count > 1)
                {

                    PointF[] Points = new PointF[a.Vertices.Count];
                    for (int i = 0; i < a.Vertices.Count; i++)
                    {
                        Points[i] = a.Vertices[i].ToF();
                    }
                  //  GP.AddPolygon(Points);
                    if (Out == false)
                    {
                        G.FillPath(C, Points);
                    //    GP = new GraphicsPath();
                    }
                    else
                    {
                        G.DrawPath(C, Points,1.0f, true);
                        //G.DrawPath(C, Points, 1.0f);
                     //   GP = new GraphicsPath();

                    }
                }
            }
            if (Out)
            {
               
            }
            int Vafter= G.VertexCount();
            Console.WriteLine("Drawing file: {0} - {1} shapes, {2} vertices", file.Name, file.DisplayShapes.Count, Vafter - Vbefore);

            

        }

        internal void ClearCache(bool GeomChanged)
        {
            Cache = null;
            if (GeomChanged) VBOCacheDirty = true;
        }

        private void apictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var G2 = e.Graphics;
            Bounds Bounds = new Bounds();
            foreach (var a in Document.Gerbers.OrderBy(x => x.sortindex))
            {
                Bounds.AddBox(a.File.BoundingBox);
            }

            if (Cache == null)
            {
                Cache = new Bitmap(Width, Height);
                Graphics G = Graphics.FromImage(Cache);
                GerberImageCreator.ApplyAASettings(G);

                GraphicsGraphicsInterface GGI = new GraphicsGraphicsInterface(G);
                GGI.Clear(Document.Colors.BackgroundColor);

            //    DrawGerbersToGraphicsInterface(Bounds, GGI);
            }
            G2.DrawImage(Cache, 0, 0);

            GerberImageCreator.ApplyAASettings(G2);

            {
                if (Document.CrossHairActive)
                {
                    if (Document.Gerbers.Count > 0)
                    {
                        float S = GetScaleAndBuildTransform(G2, Bounds);


                        Color DimensionColor = Color.FromArgb(255, 255, 200);
                        Pen P = new Pen(DimensionColor, 1.0f / S);

                        P.DashPattern = new float[2] { 2, 2 };

                        G2.DrawLine(P, (float)Bounds.TopLeft.X - 1000, Document.MouseY, (float)Bounds.BottomRight.X + 1000, Document.MouseY);
                        G2.DrawLine(P, (float)Document.MouseX, (float)Bounds.TopLeft.Y - 1000, (float)Document.MouseX, (float)Bounds.BottomRight.Y + 1000);


                        DrawLabel(G2, String.Format("{0:N2}", Document.MouseX - Bounds.TopLeft.X), S, 12, DimensionColor, 5, 0, (float)Document.MouseX, (float)Bounds.TopLeft.Y, DisplaySide == BoardSide.Bottom);
                        DrawLabel(G2, String.Format("{0:N2}", Document.MouseY - Bounds.TopLeft.Y), S, 12, DimensionColor, 0, -14, (float)Bounds.TopLeft.X, (float)Document.MouseY, DisplaySide == BoardSide.Bottom);
                        //DrawUpsideDown(G2, String.Format("{0:N2}", Document.MouseX), S, 12, Color.Yellow, 5 / S + (float)Document.MouseX, (float)Bounds.TopLeft.Y);


                    }
                }
            }
        }

        private void DrawGerbersToGraphicsInterface(Bounds Bounds, GerberVBO GGI)
        {
            if (Document.Gerbers.Count > 0)
            {

                float S = GetScaleAndBuildTransform(GGI, Bounds);
                if (DispGerb == null)
                {
                    if (DisplaySide == BoardSide.Bottom)
                    {
                        foreach (var a in Document.Gerbers.OrderByDescending(x => x.sortindex))
                        {
                            if (a.File.Layer != BoardLayer.Drill)
                            {
                                var C =  Color.FromArgb(100, a.Color);
                                
                                if (a.File.Side == BoardSide.Top) C = MathHelpers.Interpolate(C, Document.Colors.BackgroundColor, 0.4f);
                                DrawGerber(GGI, a.File, C);
                            }
                        }
                    }
                    else
                    {
                        foreach (var a in Document.Gerbers.OrderBy(x => x.sortindex))
                        {
                            if (a.File.Layer != BoardLayer.Drill)
                            {
                                var C = a.Color;
                                if (a.File.Side == BoardSide.Bottom) C = MathHelpers.Interpolate(C, Document.Colors.BackgroundColor, 0.4f);

                                DrawGerber(GGI, a.File, C);
                            }
                        }
                    }

                    foreach (var a in Document.Gerbers.OrderBy(x => x.sortindex))
                    {
                        if (a.File.Layer == BoardLayer.Drill)
                        {
                            DrawGerber(GGI, a.File, a.Color);
                        }
                    }
                }
                else
                {
                    foreach (var a in Document.Gerbers.OrderBy(x => x.sortindex))
                    {
                        if (a.File.Layer == BoardLayer.Outline || a.File.Layer == BoardLayer.Mill)
                        {
                            DrawGerber(GGI, a.File, Color.FromArgb(20, 255, 255, 255), true);
                        }
                    }
                    DrawGerber(GGI, DispGerb.File, Color.White);
                }
            }
        }
        private float GetScaleAndBuildTransform(GraphicsInterface G2, Bounds Bounds)
        {
            Bitmap B = new Bitmap(1, 1);
            Graphics G = Graphics.FromImage(B);

            float S = GetScaleAndBuildTransform(G, Bounds);
            G2.Transform = G.Transform.Clone();

            return S;
        }


        private float GetScaleAndBuildTransform(Graphics G2, Bounds Bounds)
        {
            float S = 1;
            if (DisplaySide == BoardSide.Bottom)
            {
                S = Bounds.GenerateTransformWithScaleOffset(G2, Width, Height, 14, false, Zoomlevel, Offset);
            }
            else
            {
                S = Bounds.GenerateTransformWithScaleOffset(G2, Width, Height, 14, true, Zoomlevel, Offset);

            }

            return S;
        }

        private void DrawLabel(Graphics G, string TEXT, float S, float FontSize, Color C, int Xoff, int Yoff, float X, float Y, bool v5)
        {
            var T = G.Transform.Clone();

            G.TranslateTransform(X, Y);
            G.ScaleTransform((1 / S) * (v5 ? -1 : 1), -1 / S);

            G.DrawString(TEXT, new Font("Consolas", FontSize), new SolidBrush(C), Xoff, Yoff);

            G.Transform = T;

        }



        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            Cache = null;
            //pictureBox1.Invalidate();
            if (glcontrol1 != null) glcontrol1.Invalidate();
        }


        void SetXY(int x, int y)
        {
            lastX = x;
            lastY = y;
            if (Document.Gerbers.Count == 0) return;
           // if (Cache == null) return;

            Bounds Bounds = new Bounds();


            foreach (var a in Document.Gerbers.OrderBy(xx => xx.sortindex))
            {
                Bounds.AddBox(a.File.BoundingBox);
            }
            Graphics G = Graphics.FromImage(new Bitmap(1,1));

            float S = GetScaleAndBuildTransform(G, Bounds);
            var M = G.Transform.Clone();
            M.Invert();
            PointF[] P = new PointF[1] { new PointF(x, y) };
            M.TransformPoints(P);

            MainForm.SetMouseCoord(P[0].X, P[0].Y);

        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            MouseHovering = true;
            Document.CrossHairActive = true;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (glcontrol1 != null) glcontrol1.Invalidate();
            //pictureBox1.Invalidate();
            SetXY(e.X, e.Y);
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            MouseHovering = false;
            MainForm.MouseOut();
            if (glcontrol1 != null) glcontrol1.Invalidate();

            //pictureBox1.Invalidate();
        }

        private void LayerDisplay_KeyDown(object sender, KeyEventArgs e)
        {
            bool invalidate = false;
            switch (e.KeyCode)
            {
                case Keys.Add: Zoomlevel *= 1.1f; invalidate = true; break;
                case Keys.Subtract: Zoomlevel *= 0.8f; invalidate = true; break;
            }

            if (invalidate)
            {
                //pictureBox1.Invalidate();
                if (glcontrol1 != null) glcontrol1.Invalidate();

            }
        }

        private void LayerDisplay_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            bool invalidate = false;
            switch (e.KeyChar)
            {
                case '+':
                    {
                        float NewZoomlevel = Zoomlevel * 1.1f;
                        Offset.X -= (Document.MouseX * NewZoomlevel) - (Document.MouseX * Zoomlevel);
                        Offset.Y -= (Document.MouseY * NewZoomlevel) - (Document.MouseY * Zoomlevel);
                        invalidate = true;
                        Zoomlevel = NewZoomlevel;
                    }
                    break;

                case '-':
                    {
                        float NewZoomlevel = Zoomlevel * 0.9f;
                        Offset.X -= (Document.MouseX * NewZoomlevel) - (Document.MouseX * Zoomlevel);
                        Offset.Y -= (Document.MouseY * NewZoomlevel) - (Document.MouseY * Zoomlevel);
                        invalidate = true;
                        Zoomlevel = NewZoomlevel;
                    }
                    break;
                case 'f':
                case 'F':
                    Zoomlevel = 1.0f; Offset.X = 0; Offset.Y = 0;
                    invalidate = true;
                    break;
            }

            if (invalidate)
            {
                SetXY(lastX, lastY);
                ClearCache(false);
               // pictureBox1.Invalidate();
                if (glcontrol1 != null) glcontrol1.Invalidate();

            }


        }

        private void LayerDisplay_Resize(object sender, EventArgs e)
        {
            if (glcontrol1 != null) glcontrol1.Invalidate();
        }
    }
}
