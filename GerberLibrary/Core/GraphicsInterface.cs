using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;

namespace GerberLibrary.Core
{

    public class Triangle
    {
        public PointD A;
        public PointD B;
        public PointD C;
    }


    public interface GraphicsInterface
    {
        void Clear(Color color);

        System.Drawing.Drawing2D.Matrix Transform { get; set; }

        RectangleF ClipBounds { get; }

        void FillTriangles(List<Triangle> triangles, Color C);
        void DrawImage(Bitmap MMGrid, float p1, float p2, float p3, float p4);

        System.Drawing.Drawing2D.InterpolationMode InterpolationMode { get; set; }
            
        void TranslateTransform(float p1, float p2);

        void DrawLines(Pen P, PointF[] Points);
        void DrawLine(Pen P, PointF p1, PointF p2);

        void DrawLine(Pen P, float p1, float p2, float p3, float p4);

        void RotateTransform(float p);

        void ScaleTransform(float sx, float sy);

        bool IsFast { get; set; }
        CompositingMode CompositingMode { get; set; }
        bool Dotted { get; set; }

        void DrawRectangle(Color color, float x, float y, float w, float h, float strokewidth = 1.0f);
        void FillRectangle(Color color, float x, float y, float w, float h);

        void DrawString(PointD pos, string text, double scale, bool centered, float r = 0.2f, float g = 0.2f, float b = 0.2f, float a = 1.0f);

        PointD MeasureString(string p);

        void FillShape(SolidBrush BR, PolyLine Shape);
        void FillPath(Color c, GraphicsPath gP);
        void DrawString(string text, Font font, SolidBrush solidBrush, float x, float y, StringFormat sF);
        void DrawPath(Color black, GraphicsPath pATH, float v);
        void FillPolygon(SolidBrush solidBrush, PointF[] pointF);
    }


    public class GraphicsGraphicsInterface : GraphicsInterface
    {
        Graphics G;

        public GraphicsGraphicsInterface(Graphics _G)
        {
            G = _G;
        }

        public void Clear(Color color)
        {
            G.Clear(color);
        }

        public System.Drawing.Drawing2D.Matrix Transform
        {
            get
            {
                return G.Transform;
            }
            set
            {
                G.Transform = value;
            }
        }

        public RectangleF ClipBounds
        {
            get
            {
                return G.ClipBounds;
            }

        }

        public void DrawImage(Bitmap Img, float p1, float p2, float p3, float p4)
        {
            G.DrawImage(Img, p1, p2, p3, p4);
        }

        public System.Drawing.Drawing2D.InterpolationMode InterpolationMode
        {
            get
            {
                return G.InterpolationMode;
            }
            set
            {
                G.InterpolationMode = value;
            }
        }

        public void TranslateTransform(float p1, float p2)
        {
            G.TranslateTransform(p1, p2);
        }

        public void DrawLines(Pen P, PointF[] Points)
        {
            G.DrawLines(P, Points);
        }

        public void DrawLine(Pen P, float x1, float y1, float x2, float y2)
        {
            G.DrawLine(P, x1, y1, x2, y2);
        }

        public void RotateTransform(float p)
        {
            G.RotateTransform(p);
        }


        public void ScaleTransform(float sx, float sy)
        {
            G.ScaleTransform(sx, sy);
        }


        public bool IsFast
        {
            get
            {
                return false;
            }
            set
            {

            }
        }

        public CompositingMode CompositingMode
        {
            get
            {
                return G.CompositingMode;
            }

            set
            {
                G.CompositingMode = value;
            }
        }

        bool _Dotted = false;
        public bool Dotted { get { return _Dotted; } set { _Dotted = value; }}

        public void DrawRectangle(Color color, float x, float y, float w, float h)
        {
            DrawLine(new Pen(color), x, y, x+w, y);
            DrawLine(new Pen(color), x+w, y, x+w, y+h);
            DrawLine(new Pen(color), x+w, y+h, x, y+h);
            DrawLine(new Pen(color), x, y+h, x, y);
        }

        public void DrawString(PointD pos, string text, double scale, bool center, float r = 0.2f, float g = 0.2f, float b = 0.2f, float a = 1.0f)
        {
            StringFormat SF = new StringFormat();
            if (center) SF.Alignment = StringAlignment.Center;
            SF.LineAlignment = StringAlignment.Far;
            G.DrawString(text, new Font("Arial", (float)scale * 13.0f), new SolidBrush(Color.FromArgb((byte)(a * 255.0), (byte)(r * 255.0), (byte)(g * 255.0), (byte)(b * 255.0))), new PointF((float)pos.X, (float)pos.Y), SF);
        }

        public PointD MeasureString(string p)
        {
            return new PointD(p.Length * 10, 10);
        }


        public void FillShape(SolidBrush BR, PolyLine Shape)
        {
            throw new NotImplementedException();
        }

        public void FillRectangle(Color color, float x, float y, float w, float h)
        {
            G.FillRectangle(new SolidBrush(color), x, y, w, h);
        }

        public void FillPath(Color c, GraphicsPath gP)
        {
            G.FillPath(new SolidBrush(c), gP);           
        }

        public void DrawLine(Pen P, PointF p1, PointF p2)
        {
            DrawLine(P, p1.X, p1.Y, p2.X, p2.Y);
        }

        public void DrawRectangle(Color color, float x, float y, float w, float h, float strokewidth = 1)
        {
            DrawLine(new Pen(color, strokewidth), x, y, x + w, y);
            DrawLine(new Pen(color, strokewidth), x+w, y, x + w, y+h);
            DrawLine(new Pen(color, strokewidth), x+w, y+h, x , y+h);
            DrawLine(new Pen(color, strokewidth), x, y+h, x , y );
        }

        public void DrawString(string text, Font font, SolidBrush solidBrush, float x, float y, StringFormat sF)
        {
            G.DrawString(text, font, solidBrush, new PointF(x, y));
        }

        public void DrawPath(Color black, GraphicsPath pATH, float v)
        {
            G.DrawPath(new Pen(black, v), pATH);
            
        }

        public void FillPolygon(SolidBrush solidBrush, PointF[] pointF)
        {
            G.FillPolygon(solidBrush, pointF);            
        }

        public void FillTriangles(List<Triangle> triangles, Color C)
        {
            throw new NotImplementedException();
        }
    }

}
