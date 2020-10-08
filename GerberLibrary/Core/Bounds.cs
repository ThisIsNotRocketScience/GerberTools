using GerberLibrary.Core.Primitives;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace GerberLibrary
{

    public class Bounds
    {
        public PointD BottomRight = new PointD(0, 0);
        public bool Contains(PointD inp)
        {
            if (inp.X >= TopLeft.X && inp.X < BottomRight.X
                && inp.Y >= TopLeft.Y && inp.Y < BottomRight.Y) return true;
            return false;

        }
        public PointD TopLeft = new PointD(0, 0);

        public bool Valid = false;

        public void AddBox(Bounds bounds)
        {
            if (!bounds.Valid) return;

            FitPoint(bounds.TopLeft);
            FitPoint(bounds.BottomRight);

        }

        public void AddPolyLines(List<PolyLine> Shapes)
        {
            foreach (var a in Shapes)
            {
                AddPolyLine(a);

            }
        }

        public void AddPolyLine(PolyLine a, double expansion = 0)
        {
            if (expansion == 0)
            {
                foreach (var r in a.Vertices)
                {
                    FitPoint(new PointD(r.X, r.Y));
                }
            }
            else
            {
                foreach (var r in a.Vertices)
                {
                    FitPoint(new PointD(r.X+expansion, r.Y + expansion));
                    FitPoint(new PointD(r.X-expansion, r.Y- expansion));
                }
            }
        }

        public void FitPoint(double x, double y)
        {
            FitPoint(new PointD(x, y));
        }

        public void FitPoint(PointD P)
        {
            if (!Valid)
            {
                TopLeft.X = P.X;
                TopLeft.Y = P.Y;
                BottomRight.X = P.X;
                BottomRight.Y = P.Y;
                Valid = true;

            }
            if (P.X < TopLeft.X) TopLeft.X = P.X;
            if (P.Y < TopLeft.Y) TopLeft.Y = P.Y;
            if (P.X > BottomRight.X) BottomRight.X = P.X;
            if (P.Y > BottomRight.Y) BottomRight.Y = P.Y;
        }

        public float GenerateTransform(Graphics g, int width, int height, int margin, bool flipY = false)
        {
            if (Valid == false)
            {
                return 1;
            }
            float scale = Math.Min((width - margin * 2) / (float)Width(), (height - margin * 2) / (float)Height());
            g.TranslateTransform(width / 2, height / 2);
            g.ScaleTransform(scale, scale);
            if (flipY)
            {
                g.ScaleTransform(1, -1);

            }
            else
            {
                g.RotateTransform(180);
            }
            var M = Middle();
            g.TranslateTransform(-(float)M.X, -(float)M.Y);
            return scale;
        }

        public double Height()
        {
            if (!Valid) return 0;
            return BottomRight.Y - TopLeft.Y;
        }

        public bool Intersects(Bounds B)
        {
            var M1 = Middle();
            var M2 = B.Middle();

            return (Math.Abs(M1.X - M2.X) * 2 < (Width() + B.Width())) && (Math.Abs(M1.Y - M2.Y) * 2 < (Height() + B.Height()));
        }

        public PointD Middle()
        {
            return new PointD((TopLeft.X + BottomRight.X) * 0.5, (TopLeft.Y + BottomRight.Y) * 0.5);
        }

        public void Reset()
        {
            TopLeft.X = 10000;
            TopLeft.Y = 10000;
            BottomRight.X = -10000;
            BottomRight.Y = -10000;

            Valid = false;
        }

        public override string ToString()
        {
            return String.Format("({0:N2}, {1:N2}) - ({2:N2}, {3:N2}) -> {4:N2} x {5:N2} mm", TopLeft.X, TopLeft.Y, BottomRight.X, BottomRight.Y, Width(), Height());
        }

        public double Width()
        {
            if (!Valid) return 0;
            return BottomRight.X - TopLeft.X;
        }

        public double Area()
        {
            return Width() * Height();
        }

        internal void FitPoint(List<PointD> vertices)
        {
            foreach (var v in vertices)
            {
                FitPoint(v);
            }
        }

        public float GenerateTransformWithScaleOffset(Graphics g2, int width, int height, int margin, bool flipy, float scale, PointF offset)
        {
            var S = GenerateTransform(g2, width, height, margin, flipy);
            g2.ScaleTransform(scale, scale);
            g2.TranslateTransform(offset.X, offset.Y);
            S *= scale;

            return S;
        }

        public void AddPolygons(Polygons Polies)
        {
            foreach (var a in Polies)
            {
                AddPolygon(a);
            }
        }

        public void AddPolygon(Polygon a)
        {
            PolyLine P = new PolyLine(PolyLine.PolyIDs.Outline);
            P.fromPolygon(a);
            AddPolyLine(P);
        }

        public Bounds Grow(double v)
        {
            Bounds B = new Bounds();
            B.TopLeft = TopLeft - new PointD(v, v);
            B.BottomRight = BottomRight + new PointD(v, v);

            return B;
        }

    }

}
