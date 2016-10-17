using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberLibrary.Core
{
    public interface GraphicsInterface
    {
        void Clear(Color color);

        System.Drawing.Drawing2D.Matrix Transform { get; set; }

        RectangleF ClipBounds { get; }

        void DrawImage(Bitmap MMGrid, float p1, float p2, float p3, float p4);

        System.Drawing.Drawing2D.InterpolationMode InterpolationMode { get; set; }

        void TranslateTransform(float p1, float p2);

        void DrawLines(Pen P, PointF[] Points);

        void DrawLine(Pen P, float p1, float p2, float p3, float p4);

        void RotateTransform(float p);

        void ScaleTransform(float sx, float sy);

        bool IsFast { get; set; }

        void DrawRectangle(Color color, float x, float y, float w, float h);

        void DrawString(PointD pos, string text, double scale, bool centered, float r = 0.2f, float g = 0.2f, float b = 0.2f, float a = 1.0f);

        PointD MeasureString(string p);

        void FillShape(SolidBrush BR, PolyLine Shape);
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

        public void DrawLine(Pen P, float p1, float p2, float p3, float p4)
        {
            G.DrawLine(P, p1, p2, p3, p4);
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


        public void DrawRectangle(Color color, float x, float y, float w, float h)
        {
            throw new NotImplementedException();
        }


        public void DrawString(PointD pos, string text, double scale, bool center, float r = 0.2f, float g = 0.2f, float b = 0.2f, float a = 1.0f)
        {
        }



        public PointD MeasureString(string p)
        {
            return new PointD(p.Length * 10, 10);
        }


        public void FillShape(SolidBrush BR, PolyLine Shape)
        {
            throw new NotImplementedException();
        }
    }

}
