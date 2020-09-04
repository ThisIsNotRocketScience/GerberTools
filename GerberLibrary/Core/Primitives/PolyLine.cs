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
        public enum PolyIDs
        {
            Outline = -20,
            Bitmap = -30,
            No = -2,
            Gallifrey = -50,
            ArtWork = -42,
            AutoTabs = -60,
            Negative = -70,
            Aperture = -900,
            ApertureConstr = -902,
            Temp = -1000,
            GFXTemp = -1001,
            OffsetLine = -21,
        }

        public int ID = -1;

        public PolyLine() : this(PolyIDs.Temp) { }

        public PolyLine(PolyIDs nID)
        {
            ID = (int)nID;
        }

        public PolyLine(int nID)
        {
            ID = nID;
        }


        public List<PointD> Vertices = new List<PointD>();


        public PolyLine Copy()
        {
            PolyLine PL = new PolyLine(ID);
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

        internal bool ClockWise()
        {


            bool isClockwise = false;
            double sum = 0;
            for (int i = 0; i < Vertices.Count; i++)
            {
                sum += (Vertices[(i + 1) % Vertices.Count].X - Vertices[i].X) * (Vertices[(i + 1) % Vertices.Count].Y + Vertices[i].Y);
            }
            isClockwise = (sum > 0) ? true : false;
            return isClockwise;


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
        public double Width;

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

        public void MakeCircle(double radius, int C = 20, double ox = 0, double oy = 0)
        {
            Vertices.Clear();
            for (int i = 0; i < C + 1; i++)
            {
                double P = ((double)i / (double)(C)) * (double)Math.PI * 2;
                Vertices.Add(new PointD(Math.Sin(P) * radius + ox, Math.Cos(P) * radius + oy));
            }
        }

        internal void MakeRectangle(double w, double h, double ox = 0, double oy = 0)
        {
            Vertices.Clear();
            Vertices.Add(new PointD(ox + -w / 2, oy + -h / 2));
            Vertices.Add(new PointD(ox + w / 2, oy + -h / 2));
            Vertices.Add(new PointD(ox + w / 2, oy + h / 2));
            Vertices.Add(new PointD(ox + -w / 2, oy + h / 2));
            Vertices.Add(new PointD(ox + -w / 2, oy + -h / 2));
        }

        internal void MakePRectangle(double w, double h, double ox = 0, double oy = 0)
        {
            Vertices.Clear();
            Vertices.Add(new PointD(ox + 0, oy + 0));
            Vertices.Add(new PointD(ox + w, oy + 0));
            Vertices.Add(new PointD(ox + w, oy + h));
            Vertices.Add(new PointD(ox + 0, oy + h));
            Vertices.Add(new PointD(ox + 0, oy + 0));
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
        public Bounds GetBounds()
        {
            Bounds B = new Bounds();
            foreach (var v in Vertices)
            {
                B.FitPoint(v);
            }
            return B;
        }


        public bool ContainsPoint(double x, double y)
        {
            return Helpers.IsInPolygon(Vertices, new PointD(x, y));
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

        public PointD GetMidPoint()
        {
            double centerX = 0.0f;
            double centerY = 0.0f;

            for (int i = 0; i < Vertices.Count; i++)
            {
                centerX += (Vertices[i].X);
                centerY += (Vertices[i].Y);
            }

            if (Vertices.Count == 0)
            {
                return new PointD(0, 0);
            }

            return new PointD(centerX / (float)Vertices.Count, centerY / (float)Vertices.Count);
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

        public static List<PolyLine> StrokeBox(PointD topLeft, PointD bottomRight, double v)
        {
            List<PolyLine> R = new List<PolyLine>();

            R.Add(Stroke(topLeft.X, topLeft.Y, topLeft.X, bottomRight.Y, v));
            R.Add(Stroke(bottomRight.X, topLeft.Y, bottomRight.X, bottomRight.Y, v));

            R.Add(Stroke(topLeft.X, topLeft.Y, bottomRight.X, topLeft.Y, v));
            R.Add(Stroke(topLeft.X, bottomRight.Y, bottomRight.X, bottomRight.Y, v));
            return R;
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

        public bool EntirelyInside(PolyLine b)
        {
            foreach (var v in Vertices)
            {
                if (b.PointInPoly(v) == false)
                {
                    return false;
                }
            }
            return true;
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
            for (int i = 0; i < Vertices.Count - 1; i++)
            {
                L += (Vertices[i] - Vertices[i + 1]).Length();
            }
            return L;
        }


        public bool PointInPoly(PointD pnt)
        {
            int i, j;
            int nvert = Vertices.Count;
            bool c = false;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (((Vertices[i].Y > pnt.Y) != (Vertices[j].Y > pnt.Y)) &&
                 (pnt.X < (Vertices[j].X - Vertices[i].X) * (pnt.Y - Vertices[i].Y) / (Vertices[j].Y - Vertices[i].Y) + Vertices[i].X))
                    c = !c;
            }
            return c;
        }

        public static PolyLine Stroke(double x1, double y1, double x2, double y2, double w)
        {
            PolyLine PL = new PolyLine(PolyIDs.GFXTemp);
            PointD A = new PointD(x1, y1);
            PointD B = new PointD(x2, y2);
            var C = B - A;

            PL.MakeRectangle(w, C.Length() + w);
            PL.Translate(0, C.Length() / 2);
            PL.RotateDegrees(Math.Atan2(C.Y, C.X) * 360 / (Math.PI * 2) - 90);
            PL.Translate(x1, y1);

            return PL;
        }


        public static List<PolyLine> MiniFontShapes(string v1, double _x, double _y, double size, double rotation = 0)
        {
            List<PolyLine> Res = new List<PolyLine>();
            double wunit = size * 0.55f;
            double hunit = -size;
            double stroke = size * 0.15;
            double w = 0;
            double x = 0;
            double y = 0;
            y -= hunit / 2;

            for (int i = 0; i < v1.Length; i++)
            {
                var c = v1[i];
                switch (c)
                {
                    case '1': w += 0.5 * wunit; break;
                    default: w += 1.5 * wunit; break;
                }
            }
            w -= 0.5 * wunit;
            x -= w / 2;

            for (int i = 0; i < v1.Length; i++)
            {
                var c = v1[i];
                switch (c)
                {
                    case '0':
                        Res.Add(PolyLine.Stroke(x, y, x, y + hunit, stroke));
                        Res.Add(PolyLine.Stroke(x + wunit, y, x + wunit, y + hunit, stroke));
                        Res.Add(PolyLine.Stroke(x, y, x + wunit, y, stroke));
                        Res.Add(PolyLine.Stroke(x, y + hunit, x + wunit, y + hunit, stroke));
                        x += 1.5 * wunit;
                        break;
                    case '1':
                        Res.Add(PolyLine.Stroke(x, y, x, y + hunit, stroke));
                        x += 0.5 * wunit;
                        break;
                    case '2':
                        Res.Add(PolyLine.Stroke(x, y, x + wunit, y, stroke));
                        Res.Add(PolyLine.Stroke(x, y + hunit / 2, x + wunit, y + hunit / 2, stroke));
                        Res.Add(PolyLine.Stroke(x, y + hunit, x + wunit, y + hunit, stroke));
                        Res.Add(PolyLine.Stroke(x + wunit, y, x + wunit, y + hunit / 2, stroke));
                        Res.Add(PolyLine.Stroke(x, y + hunit / 2, x, y + hunit, stroke));


                        x += 1.5 * wunit;
                        break;
                    case '3':
                        Res.Add(PolyLine.Stroke(x, y, x + wunit, y, stroke));
                        Res.Add(PolyLine.Stroke(x + wunit / 2, y + hunit / 2, x + wunit, y + hunit / 2, stroke));
                        Res.Add(PolyLine.Stroke(x, y + hunit, x + wunit, y + hunit, stroke));
                        Res.Add(PolyLine.Stroke(x + wunit, y, x + wunit, y + hunit, stroke));
                        x += 1.5 * wunit;
                        break;
                    case '4':
                        Res.Add(PolyLine.Stroke(x, y, x, y + hunit / 2, stroke));
                        Res.Add(PolyLine.Stroke(x, y + hunit / 2, x + wunit, y + hunit / 2, stroke));
                        Res.Add(PolyLine.Stroke(x + wunit, y, x + wunit, y + hunit, stroke));

                        x += 1.5 * wunit;
                        break;
                    case '5':
                        Res.Add(PolyLine.Stroke(x, y, x + wunit, y, stroke));
                        Res.Add(PolyLine.Stroke(x, y + hunit / 2, x + wunit, y + hunit / 2, stroke));
                        Res.Add(PolyLine.Stroke(x, y + hunit, x + wunit, y + hunit, stroke));
                        Res.Add(PolyLine.Stroke(x, y, x, y + hunit / 2, stroke));
                        Res.Add(PolyLine.Stroke(x + wunit, y + hunit / 2, x + wunit, y + hunit, stroke));

                        x += 1.5 * wunit;
                        break;
                    case '6':
                        Res.Add(PolyLine.Stroke(x, y, x, y + hunit, stroke));
                        Res.Add(PolyLine.Stroke(x + wunit, y + hunit / 2, x + wunit, y + hunit, stroke));
                        Res.Add(PolyLine.Stroke(x, y, x + wunit, y, stroke));
                        Res.Add(PolyLine.Stroke(x, y + hunit / 2, x + wunit, y + hunit / 2, stroke));
                        Res.Add(PolyLine.Stroke(x, y + hunit, x + wunit, y + hunit, stroke));
                        x += 1.5 * wunit;
                        break;
                    case '7':
                        Res.Add(PolyLine.Stroke(x, y, x + wunit, y, stroke));
                        Res.Add(PolyLine.Stroke(x + wunit, y, x + wunit, y + hunit, stroke));
                        x += 1.5 * wunit;
                        break;
                    case '8':
                        Res.Add(PolyLine.Stroke(x, y, x, y + hunit, stroke));
                        Res.Add(PolyLine.Stroke(x + wunit, y, x + wunit, y + hunit, stroke));
                        Res.Add(PolyLine.Stroke(x, y, x + wunit, y, stroke));
                        Res.Add(PolyLine.Stroke(x, y + hunit, x + wunit, y + hunit, stroke));
                        Res.Add(PolyLine.Stroke(x, y + hunit / 2, x + wunit, y + hunit / 2, stroke));

                        x += 1.5 * wunit;
                        break;
                    case '9':
                        Res.Add(PolyLine.Stroke(x, y + hunit / 2, x + wunit, y + hunit / 2, stroke));
                        Res.Add(PolyLine.Stroke(x, y, x, y + hunit / 2, stroke));

                        Res.Add(PolyLine.Stroke(x, y, x + wunit, y, stroke));

                        Res.Add(PolyLine.Stroke(x + wunit, y, x + wunit, y + hunit, stroke));
                        x += 1.5 * wunit;
                        break;
                }

            }

            Res.Add(PolyLine.Stroke(-w / 2, hunit * 0.5 - stroke * 2, w / 2, hunit * 0.5 - stroke * 2, stroke));
            foreach (var r in Res)
            {
                r.RotateDegrees(rotation);
                r.Translate(_x, _y);
            }

            return Res;



        }

        internal List<PolyLine> Offset(double margin, int v)
        {
            List<PolyLine> Res = new List<PolyLine>();

            
            Polygons clips = new Polygons();
            Polygon b = this.toPolygon();

            clips.Add(b);
            Polygons clips2 = Clipper.OffsetPolygons(clips, margin* 100000.0f, JoinType.jtMiter);

            foreach(var r in clips2)
            {
                PolyLine PLR = new PolyLine(v++);
                PLR.fromPolygon(r);
                Res.Add(PLR);
            }
            
            
            return Res;

        }
        public void MakeTriangle(PointD A, PointD B, PointD C)
        {
            Vertices.Clear();
            Vertices.Add(A);
            Vertices.Add(B);
            Vertices.Add(C);

        }
    }

}
