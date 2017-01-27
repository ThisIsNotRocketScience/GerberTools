using ClipperLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;


namespace GerberLibrary.Core.Primitives
{
    public class PolyLine
    {
        public PolyLine()
        {
        }
        public List<PointD> Vertices = new List<PointD>();


        public PolyLine Copy()
        {
            PolyLine PL = new PolyLine();
            foreach (var a in Vertices)
            {
                PL.Add(a.X, a.Y);
            }
            return PL;
        }

        public void AddArc(double cx, double cy, double width, double height, double startangle, double endangle)
        {

            for (double p = startangle; p <= endangle; p += (endangle - startangle) / 200.0)
            {
                Add(cx + Math.Cos(p) * width / 2.0, cy + Math.Sin(p) * height / 2.0);
            }
        }
        public void ArcTo(PointD Target, PointD Center, Core.InterpolationMode Dir = Core.InterpolationMode.ClockWise)
        {

            double LastX = 0;
            double LastY = 0;
            if (Vertices.Count() > 0)
            {
                LastX = Vertices.Last().X;
                LastY = Vertices.Last().Y;
            }
            var NewVert = Gerber.CreateCurvePoints(LastX, LastY, Target.X, Target.Y, Center.X, Center.Y, Dir, GerberQuadrantMode.Multi);
            Vertices.AddRange(NewVert);
        }

        void Add(PointD Target)
        {
            Vertices.Add(Target);
        }

        public bool Hole = false;
        public void CheckIfHole()
        {
            if (Clipper.Orientation(this.toPolygon()))
            {
                Hole = false;
            }
            else
            {
                Hole = true;
            }
        }

        public bool Draw = true;
        public PointD start() { return Vertices[0]; }
        public PointD end() { return Vertices[Vertices.Count - 1]; }
        public int Count() { return Vertices.Count; }

        public void Add(double x, double y)
        {
            //    if (MyTool != null)
            //   {
            //       Vertices.Add(new PointD(x + MyTool.XOffset, y + MyTool.YOffset));
            //   }
            //   else
            {
                Vertices.Add(new PointD(x, y));
            }
        }

        public void Close()
        {
            if (Vertices.Count == 0) return;
            PointD S = start();
            PointD E = end();
            if (E.X != S.X || E.Y != S.Y)
            {
                //   if (MyTool != null)
                //   {
                //       Add(S.X - MyTool.XOffset, S.Y - MyTool.YOffset);
                //   }
                //   else
                {
                    Add(S.X, S.Y);

                }
            }

        }

        public Polygon toPolygon()
        {
            Polygon P = new Polygon();
            for (int i = 0; i < Count(); i++)
            {
                P.Add(new IntPoint((long)(Vertices[i].X * 100000.0f), (long)(Vertices[i].Y * 100000.0f)));
            }
            return P;

        }

        public void fromPolygon(Polygon list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Add(list[i].X / 100000.0f, list[i].Y / 100000.0f);
            }
            Close();
        }

        public Color MyColor = Color.DarkGreen;
        public bool Thin = false;
        public bool ClearanceMode;
        public Color GetColor()
        {
            return MyColor;
        }

        //  public Tool MyTool;

        public void FillTransformed(PolyLine c, PointD trans, float angle)
        {
            double cx = Math.Cos(angle * Math.PI * 2 / 360.0);
            double sx = Math.Sin(angle * Math.PI * 2 / 360.0);


            Vertices.Clear();
            foreach (var a in c.Vertices)
            {
                Vertices.Add(new PointD(cx * a.X - sx * a.Y + trans.X, sx * a.X + cx * a.Y + trans.Y));
            }
            //  throw new NotImplementedException();
        }

        public void MoveBack(PointD TopLeft)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i].X -= TopLeft.X;
                Vertices[i].Y -= TopLeft.Y;
            }
        }

        public void MakeSquare(double p)
        {
            Vertices.Clear();
            Vertices.Add(new PointD(-p, -p));
            Vertices.Add(new PointD(p, -p));
            Vertices.Add(new PointD(p, p));
            Vertices.Add(new PointD(-p, p));
            Vertices.Add(new PointD(-p, -p));
        }

        public void MakeCircle(double p, int C = 20)
        {
            Vertices.Clear();
            for (int i = 0; i < C + 1; i++)
            {
                double P = ((double)i / (double)(C)) * (double)Math.PI * 2;
                Vertices.Add(new PointD(Math.Sin(P) * p, Math.Cos(P) * p));
            }
        }

        internal void MakeRectangle(double w, double h)
        {
            Vertices.Clear();
            Vertices.Add(new PointD(-w / 2, -h / 2));
            Vertices.Add(new PointD(w / 2, -h / 2));
            Vertices.Add(new PointD(w / 2, h / 2));
            Vertices.Add(new PointD(-w / 2, h / 2));
            Vertices.Add(new PointD(-w / 2, -h / 2));
        }


        public void RotateDegrees(double AngleInDeg)
        {
            double Angle = AngleInDeg * (Math.PI * 2.0) / 360.0;
            double CA = Math.Cos(Angle);
            double SA = Math.Sin(Angle);

            for (int i = 0; i < Vertices.Count; i++)
            {
                double X = Vertices[i].X * CA - Vertices[i].Y * SA;
                double Y = Vertices[i].X * SA + Vertices[i].Y * CA;
                Vertices[i].X = X;
                Vertices[i].Y = Y;
            }
        }

        public void Translate(double Xoff, double Yoff)
        {
            foreach (var a in Vertices)
            {
                a.X += Xoff;
                a.Y += Yoff;
            }
        }

        public PointD GetCentroid()
        {
            double accumulatedArea = 0.0f;
            double centerX = 0.0f;
            double centerY = 0.0f;

            for (int i = 0, j = Vertices.Count - 1; i < Vertices.Count; j = i++)
            {
                double temp = Vertices[i].X * Vertices[j].Y - Vertices[j].X * Vertices[i].Y;
                accumulatedArea += temp;
                centerX += (Vertices[i].X + Vertices[j].X) * temp;
                centerY += (Vertices[i].Y + Vertices[j].Y) * temp;
            }

            if (accumulatedArea < 1E-7f)
                return null;

            accumulatedArea *= 3f;
            return new PointD(centerX / accumulatedArea, centerY / accumulatedArea);
        }

        public List<PointD> GetIntersections(PointD diffA, PointD cA)
        {
            List<PointD> res = new List<PointD>();
            for (int i = 0; i < Vertices.Count - 1; i++)
            {
                var D = Helpers.SegmentSegmentIntersect(diffA, cA, Vertices[i], Vertices[i + 1]);
                if (D != null) res.Add(D);
            }
            var D2 = Helpers.SegmentSegmentIntersect(diffA, cA, Vertices[Vertices.Count - 1], Vertices[0]);
            if (D2 != null) res.Add(D2);


            return res;
        }

        internal void SetObround(double W, double H)
        {

            Vertices.Clear();


            double MinSize = Math.Min(W, H);
            double Radius = MinSize / 2.0;
            double WAdd = (W - MinSize) / 2.0;
            double HAdd = (H - MinSize) / 2.0;

            int sides = (int)(Gerber.ArcQualityScaleFactor * Math.Max(2.0, Radius));

            for (int i = 0; i < sides; i++)
            {
                double P = (i / (double)(sides * 4)) * Math.PI * 2.0;
                double x = Math.Sin(P) * Radius + WAdd;
                double y = Math.Cos(P) * Radius + HAdd;
                Add(x, y);
            }

            for (int i = 0; i < sides; i++)
            {
                double P = (i / (double)(sides * 4)) * Math.PI * 2.0;
                P += Math.PI / 2;
                double x = Math.Sin(P) * Radius + WAdd;
                double y = Math.Cos(P) * Radius - HAdd;
                Add(x, y);
            }

            for (int i = 0; i < sides; i++)
            {
                double P = (i / (double)(sides * 4)) * Math.PI * 2.0;
                P += Math.PI;
                double x = Math.Sin(P) * Radius - WAdd;
                double y = Math.Cos(P) * Radius - HAdd;
                Add(x, y);
            }
            for (int i = 0; i < sides; i++)
            {
                double P = (i / (double)(sides * 4)) * Math.PI * 2.0;
                P += Math.PI + Math.PI / 2;
                double x = Math.Sin(P) * Radius - WAdd;
                double y = Math.Cos(P) * Radius + HAdd;
                Add(x, y);
            }
            Close();
        }

        public PointF[] ToPointsArray()
        {

            List<PointF> P = new List<PointF>();
            foreach (var v in Vertices)
            {
                P.Add(new PointF((float)v.X, (float)v.Y));
            }
            return P.ToArray();
        }

        internal void AddEllipse(double x, double y, double w, double h)
        {
            for (double p = 0; p <= Math.PI * 2; p += (Math.PI * 2) / 300.0)
            {
                Add(x + Math.Cos(p) * w / 2.0, y + Math.Sin(p) * h / 2.0);
            }
        }

        public void MakeRoundedRect(PointD TopLeft, PointD BottomRight, double BorderRadiusInMM)
        {
            if (BorderRadiusInMM == 0)
            {
                Vertices.Clear();
                Vertices.Add(TopLeft);
                Vertices.Add(new PointD(BottomRight.X, TopLeft.Y));
                Vertices.Add(BottomRight);
                Vertices.Add(new PointD(TopLeft.X, BottomRight.Y));
                Close();
            }
            else
            {
                double radcount = Math.Ceiling(Math.PI * BorderRadiusInMM * 2);
                Vertices.Clear();

                //Vertices.Add(TopLeft);
                var TopRight = new PointD(BottomRight.X, TopLeft.Y);
                var BottomLeft = new PointD(TopLeft.X, BottomRight.Y);

                var TopRight2 = new PointD(TopRight.X - BorderRadiusInMM, TopRight.Y + BorderRadiusInMM);
                var BottomRight2 = new PointD(BottomRight.X - BorderRadiusInMM, BottomRight.Y - BorderRadiusInMM);

                var TopLeft2 = new PointD(TopLeft.X + BorderRadiusInMM, TopLeft.Y + BorderRadiusInMM);
                var BottomLeft2 = new PointD(BottomLeft.X + BorderRadiusInMM, BottomLeft.Y - BorderRadiusInMM);


                for (int i = 0; i <= radcount; i++)
                {
                    double P = (i * Math.PI / 2.0) / (double)radcount;
                    Vertices.Add(new PointD(TopLeft2.X - Math.Cos(P) * BorderRadiusInMM, TopLeft2.Y - Math.Sin(P) * BorderRadiusInMM));
                }
                for (int i = 0; i <= radcount; i++)
                {
                    double P = (i * Math.PI / 2.0) / (double)radcount;
                    Vertices.Add(new PointD(TopRight2.X + Math.Sin(P) * BorderRadiusInMM, TopRight2.Y - Math.Cos(P) * BorderRadiusInMM));
                }
                for (int i = 0; i <= radcount; i++)
                {
                    double P = (i * Math.PI / 2.0) / (double)radcount;
                    Vertices.Add(new PointD(BottomRight2.X + Math.Cos(P) * BorderRadiusInMM, BottomRight2.Y + Math.Sin(P) * BorderRadiusInMM));
                }
                for (int i = 0; i <= radcount; i++)
                {
                    double P = (i * Math.PI / 2.0) / (double)radcount;
                    Vertices.Add(new PointD(BottomLeft2.X - Math.Sin(P) * BorderRadiusInMM, BottomLeft2.Y + Math.Cos(P) * BorderRadiusInMM));
                }


                Close();
            }
        }

        internal double OutlineLength()
        {
            double L = 0;
            for (int i = 0; i < Vertices.Count-1; i++)
            {
                L += (Vertices[i] - Vertices[i + 1]).Length();
            }
            return L;
        }
    }

}
