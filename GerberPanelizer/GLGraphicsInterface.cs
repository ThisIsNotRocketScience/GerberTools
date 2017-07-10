using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GerberLibrary;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
//using System.Drawing.Drawing2D;
using GerberLibrary.Core.Primitives;
using GerberLibrary.Core;
using System.Drawing.Drawing2D;


using TriangleNet.Geometry;
using TriangleNet.IO;
using TriangleNet.Meshing;


namespace GerberCombinerBuilder
{
    public class GLGraphicsInterface : GraphicsInterface
    {
        RectangleF ClipRect;

        public GLGraphicsInterface(float x, float y, float w, float h)
        {
            ClipRect = new RectangleF(x, y, w, h);
        }

        public void Clear(Color color)
        {
            GL.ClearColor(color);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        private System.Drawing.Drawing2D.Matrix trans = new System.Drawing.Drawing2D.Matrix();
        public static Matrix4 Mat4FromMat(System.Drawing.Drawing2D.Matrix inp)
        {
            Matrix4 M =
         Matrix4.Identity;
            M.M11 = inp.Elements[0];
            M.M12 = inp.Elements[1];
            M.M21 = inp.Elements[2];
            M.M22 = inp.Elements[3];
            M[3, 0] = inp.OffsetX;
            M[3, 1] = inp.OffsetY;

            return M;

        }
        public System.Drawing.Drawing2D.Matrix Transform
        {
            get
            {
                return trans;
            }
            set
            {
                trans = value;
                Matrix4 M = Mat4FromMat(trans);
                GL.LoadIdentity();
                GL.MultMatrix(ref M);

            }
        }

        public RectangleF ClipBounds
        {
            get { return ClipRect; }
        }

        public void DrawImage(Bitmap MMGrid, float p1, float p2, float p3, float p4)
        {
            return;
        }

        public System.Drawing.Drawing2D.InterpolationMode InterpolationMode
        {
            get
            {
                return System.Drawing.Drawing2D.InterpolationMode.High;
            }
            set
            {

            }
        }


        public void DrawLines(Pen P, PointF[] Points)
        {
            GL.LineWidth(P.Width);
            GL.Color4(P.Color.R, P.Color.G, P.Color.B, P.Color.A);
            GL.Begin(BeginMode.LineStrip);
            for (int i = 0; i < Points.Count(); i++)
            {
                GL.Vertex2(Points[i].X, Points[i].Y);
            }
            GL.End();

        }

        public void DrawLine(Pen P, float x1, float y1, float x2, float y2)
        {
            GL.LineWidth(P.Width);
            GL.Color4(P.Color.R, P.Color.G, P.Color.B, P.Color.A);
            GL.Begin(BeginMode.Lines);
            GL.Vertex2(x1, y1);
            GL.Vertex2(x2, y2);
            GL.End();
        }


        void DoTransform(System.Drawing.Drawing2D.Matrix M)
        {

            trans.Multiply(M);

            Transform = trans;
        }
        void DoTransform(Matrix4 M)
        {
        }

        public void RotateTransform(float p)
        {
            System.Drawing.Drawing2D.Matrix M2 = new System.Drawing.Drawing2D.Matrix();
            M2.Rotate(p);
            DoTransform(M2);
        }


        public void ScaleTransform(float sx, float sy)
        {
            System.Drawing.Drawing2D.Matrix M2 = new System.Drawing.Drawing2D.Matrix();
            M2.Scale(sx, sy);
            DoTransform(M2);
        }
        public void TranslateTransform(float p1, float p2)
        {

            System.Drawing.Drawing2D.Matrix M2 = new System.Drawing.Drawing2D.Matrix();
            M2.Translate(p1, p2);

            DoTransform(M2);
        }

        public static System.Drawing.Drawing2D.Matrix MatFromMat4(Matrix4 M42)
        {
            System.Drawing.Drawing2D.Matrix R = new System.Drawing.Drawing2D.Matrix(M42.M11, M42.M12, M42.M21, M42.M22, M42[3, 0], M42[3, 1]);
            return R;
        }


        public bool IsFast
        {
            get
            {
                return true;
            }
            set
            {
                // return false;
            }
        }

        public CompositingMode CompositingMode
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Dotted
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public static QuickFont.QFont Font;
        public void DrawString(PointD pos, string text, double scale, bool center, float r = 0.2f, float g = 0.2f, float b = 0.2f, float a = 1.0f)
        {
            CheckFont();
            GL.PushMatrix();
            GL.Translate(pos.X, pos.Y, 0);
            GL.Scale(scale / 40.0, -scale / 40.0, scale / 40.0);
            Font.Options.Colour = new OpenTK.Graphics.Color4(r, g, b, a);
            if (center)
            {
                Font.Print(text, QuickFont.QFontAlignment.Centre, new OpenTK.Vector2(0, -20));

            }
            else
            {
                Font.Print(text, QuickFont.QFontAlignment.Left, new OpenTK.Vector2(0, -20));
            }
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Blend);
            GL.PopMatrix();
        }

        private static void CheckFont()
        {
            if (Font == null)
            {
                Font = new QuickFont.QFont(new Font("Arial", 40));
            }
        }

        public PointD MeasureString(string p)
        {
            CheckFont();
            var R = Font.Measure(p);
            return new PointD(R.Width, R.Height);
        }

        public void FillShape(SolidBrush P, PolyLine Shape)
        {
            // GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            if (P.Color.A < 255)
            {
                GL.Enable(EnableCap.Blend);
            }
            var polygon = new Polygon();
            List<Vertex> V = new List<Vertex>();
            for (int i = 0; i < Shape.Count(); i++)
            {
                V.Add(new Vertex( Shape.Vertices[i].X, Shape.Vertices[i].Y));
            }
            polygon.AddContour(V);


            var options = new ConstraintOptions() { ConformingDelaunay = true };
            var quality = new QualityOptions() { MinimumAngle = 25 };

            var mesh = polygon.Triangulate(options, quality);


            GL.Begin(BeginMode.Triangles);
            GL.Color4(P.Color.R, P.Color.G, P.Color.B, P.Color.A);

            foreach(var t in mesh.Triangles)
            {

               var A =  t.GetVertex(0);
                var B = t.GetVertex(1);
                var C = t.GetVertex(2);
                GL.Vertex2(A.X, A.Y);
                GL.Vertex2(B.X, B.Y);
                GL.Vertex2(C.X, C.Y);
            }

            GL.End();
        }

        public void DrawLine(Pen P, PointF p1, PointF p2)
        {
            DrawLine(P, p1.X, p1.Y, p2.X, p2.Y);
        }

        public void DrawRectangle(Color color, float x, float y, float w, float h, float strokewidth = 1)
        {
            DrawLine(new Pen(color, strokewidth), x, y, x + w, y);
            DrawLine(new Pen(color, strokewidth), x + w, y, x + w, y + h);
            DrawLine(new Pen(color, strokewidth), x + w, y + h, x, y + h);
            DrawLine(new Pen(color, strokewidth), x, y + h, x, y);
        }

        public void FillRectangle(Color color, float x, float y, float w, float h)
        {


            GL.Color4(color);
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(x, y);
            GL.Vertex2(x + w, y);
            GL.Vertex2(x + w, y + h);
            GL.Vertex2(x, y + h);
            GL.End();

        
    }

        public void FillPath(Color c, GraphicsPath gP)
        {
            throw new NotImplementedException();
        }

        public void DrawString(string text, Font font, SolidBrush solidBrush, float x, float y, StringFormat sF)
        {
            throw new NotImplementedException();
        }

        public void DrawPath(Color black, GraphicsPath pATH, float v)
        {
            throw new NotImplementedException();
        }

        public void FillPolygon(SolidBrush solidBrush, PointF[] pointF)
        {
            throw new NotImplementedException();
        }

        public void FillTriangles(List<Triangle> triangles, Color C)
        {
            GL.Begin(BeginMode.Triangles);
            GL.Color4(C.R, C.G, C.B, C.A);
            foreach (var T in triangles)
            {
                foreach (var t in triangles)
                {                  
                    GL.Vertex2(t.A.X, t.A.Y);
                    GL.Vertex2(t.B.X, t.B.Y);
                    GL.Vertex2(t.C.X, t.C.Y);
                }
            }
            GL.End();
        }
    }
}
