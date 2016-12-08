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

namespace GerberViewer
{
    public partial class LayerDisplay : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public LoadedStuff Document;
        public BoardSide DisplaySide;
        public LoadedStuff.DisplayGerber DispGerb;
        Bitmap Cache;
        private GerberViewerMainForm MainForm;

        public LayerDisplay(LoadedStuff doc, BoardSide Side, GerberViewerMainForm _Owner)
        {
            MainForm = _Owner;

            DisplaySide = Side;
            Document = doc;
            InitializeComponent();
            CloseButton = false;
            CloseButtonVisible = false;
        }

        public LayerDisplay(LoadedStuff doc, LoadedStuff.DisplayGerber Gerb, GerberViewerMainForm _Owner)
        {
            MainForm = _Owner;
            CloseButton = false;

            CloseButtonVisible = false;

            DispGerb = Gerb;
            DisplaySide = Gerb.File.Side;
            Document = doc;
            InitializeComponent();
        }




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
                    pictureBox1.Invalidate();
            }
            else
            {
              //  Console.WriteLine("Skipping ");
            }
        }



        private void DrawGerber(Graphics G, ParsedGerber file, float S, Color C, bool dotted = false)
        {


            Pen P = new Pen(C, 1.0f / S);
            if (dotted) P.DashPattern = new float[2] { 2, 2 };
            SolidBrush B = new SolidBrush(C);
            GraphicsPath GP = new GraphicsPath();
            var Out = file.IsOutline();

            foreach (var a in file.DisplayShapes)
            {

                if (a.Vertices.Count > 1)
                {

                    PointF[] Points = new PointF[a.Vertices.Count];
                    for (int i = 0; i < a.Vertices.Count; i++)
                    {
                        Points[i] = a.Vertices[i].ToF();
                    }
                    GP.AddPolygon(Points);
                    if (Out == false)
                    {
                        G.FillPath(B, GP);
                        GP = new GraphicsPath();
                    }
                }
            }
            if (Out)
            {
                G.DrawPath(P, GP);
            }

        }

        internal void ClearCache()
        {
            Cache = null;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var G2 = e.Graphics;
            PolyLineSet.Bounds Bounds = new PolyLineSet.Bounds();
            foreach (var a in Document.Gerbers.OrderBy(x => x.sortindex))
            {
                Bounds.AddBox(a.File.BoundingBox);
            }

            if (Cache == null)
            {
                Cache = new Bitmap(Width, Height);
                Graphics G = Graphics.FromImage(Cache);
                GerberImageCreator.ApplyAASettings(G);
                G.Clear(Document.Colors.BackgroundColor);
                if (Document.Gerbers.Count > 0)
                {

                    float S = 1;
                    if (DisplaySide == BoardSide.Bottom)
                    {
                        S = Bounds.GenerateTransform(G, Width, Height, 4, false);
                    }
                    else
                    {
                        S = Bounds.GenerateTransform(G, Width, Height, 4, true);

                    }
                    if (DispGerb == null)
                    {
                        if (DisplaySide == BoardSide.Bottom)
                        {
                            foreach (var a in Document.Gerbers.OrderByDescending(x => x.sortindex))
                            {
                                if (a.File.Layer != BoardLayer.Drill)
                                {
                                    var C = a.Color;
                                    if (a.File.Side == BoardSide.Top) C = MathHelpers.Interpolate(C, Document.Colors.BackgroundColor, 0.4f);
                                    DrawGerber(G, a.File, S, C);
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

                                    DrawGerber(G, a.File, S, C);
                                }
                            }
                        }

                        foreach (var a in Document.Gerbers.OrderBy(x => x.sortindex))
                        {
                            if (a.File.Layer == BoardLayer.Drill)
                            {
                                DrawGerber(G, a.File, S, a.Color);
                            }
                        }
                    }
                    else
                    {
                        foreach (var a in Document.Gerbers.OrderBy(x => x.sortindex))
                        {
                            if (a.File.Layer == BoardLayer.Outline || a.File.Layer == BoardLayer.Mill)
                            {
                                DrawGerber(G, a.File, S, Color.FromArgb(20, 255, 255, 255), true);
                            }
                        }
                        DrawGerber(G, DispGerb.File, S, Color.White);
                    }
                }
            }
            G2.DrawImage(Cache, 0, 0);

            GerberImageCreator.ApplyAASettings(G2);

            {
                if (Document.CrossHairActive)
                {
                    if (Document.Gerbers.Count > 0)
                    {
                        float S = 1;
                        if (DisplaySide == BoardSide.Bottom)
                        {
                            S = Bounds.GenerateTransform(G2, Width, Height, 4, false);
                        }
                        else
                        {
                            S = Bounds.GenerateTransform(G2, Width, Height, 4, true);

                        }

                        Pen P = new Pen(Color.FromArgb(255,255,200), 1.0f / S);

                        P.DashPattern = new float[2] { 2, 2 };

                        G2.DrawLine(P, (float)Bounds.TopLeft.X - 1000, Document.MouseY, (float)Bounds.BottomRight.X+1000, Document.MouseY);
                        G2.DrawLine(P, (float)Document.MouseX, (float)Bounds.TopLeft.Y-1000, (float)Document.MouseX, (float)Bounds.BottomRight.Y+1000);
                      


                        DrawUpsideDown(G2, String.Format("{0:N2}", Document.MouseX), S, 12, Color.Yellow, 5 / S + (float)Document.MouseX, (float)Bounds.TopLeft.Y);


                                         }
                }
            }

        }

        private void DrawUpsideDown(Graphics G2, string v1, float s, float f, Color C, float x, float y, bool flipx = true)
        {

            Font F = new Font("Arial", f);

            System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat();
            // drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            SizeF sf = G2.MeasureString(v1, F, this.Height, drawFormat);
            Bitmap bmp = new Bitmap((int)sf.Width, (int)sf.Height);
            using (Graphics bmpGraphics = Graphics.FromImage(bmp))
            {
                bmpGraphics.DrawString(v1, F, new SolidBrush(C), 0, 0, drawFormat);
            }

            bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
            if (flipx) bmp.RotateFlip(RotateFlipType.RotateNoneFlipX); else x += sf.Width / s;
            G2.DrawImage(bmp,new RectangleF(x,y,sf.Width / s, sf.Height/s),new RectangleF(0,0,bmp.Width, bmp.Height ), GraphicsUnit.Pixel);


//            G2.DrawString(String.Format("{0:N2}", Document.MouseX), F, new SolidBrush(Color.Yellow), 5 / S + (float)Document.MouseX, (float)Bounds.TopLeft.Y);


            
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            Cache = null;
            pictureBox1.Invalidate();
        }


        void SetXY(int x, int y)
        {
            if (Document.Gerbers.Count == 0) return;
            if (Cache == null) return;

            PolyLineSet.Bounds Bounds = new PolyLineSet.Bounds();


            foreach (var a in Document.Gerbers.OrderBy(xx => xx.sortindex))
            {
                Bounds.AddBox(a.File.BoundingBox);
            }
            float S = 1;
            Graphics G = Graphics.FromImage(Cache);

            if (DisplaySide == BoardSide.Bottom)
            {
                S = Bounds.GenerateTransform(G, Width, Height, 4, false);
            }
            else
            {
                S = Bounds.GenerateTransform(G, Width, Height, 4, true);

            }
            var M = G.Transform.Clone();
            M.Invert();
            PointF[] P = new PointF[1] { new PointF(x, y) };
            M.TransformPoints(P);

            MainForm.SetMouseCoord(P[0].X, P[0].Y);

        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            pictureBox1.Invalidate();
            SetXY(e.X, e.Y);
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            MainForm.MouseOut();
            pictureBox1.Invalidate();
        }
    }
}
