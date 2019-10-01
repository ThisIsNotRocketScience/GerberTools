using GerberLibrary.Core.Algorithms;
using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GerberLibrary.PolyLineSet;

namespace GerberLibrary.Core
{
    public class Line
    {
        public float x1;
        public float y1;
        public float x2;
        public float y2;

        public void Draw(Graphics g,Color C, float W  = 1.0f)
        {
            g.DrawLine(new Pen(C, W), x1, y1, x2, y2);
        }

        public static float SumLengths(List<Line> outline)
        {
            return outline.Sum(x => x.Length());            
        }

        private float Length()
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }


    public static class Helpers
    {
        public static PathDefWithClosed Sanitize(PathDefWithClosed inp)
        {
            PathDefWithClosed R = new PathDefWithClosed();
            R.Width = inp.Width;
            R.Closed = inp.Closed;

            PointD last = new PointD(double.NaN, double.NaN);

            foreach (var v in inp.Vertices)
            {
                if (v != last)
                {
                    R.Vertices.Add(v);
                    last = v.Copy();
                }
            }
            return R;
        }

        public static List<double> E12 = new List<double>() { 1.0, 1.2, 1.5, 1.8, 2.2, 2.7, 3.3, 3.9, 4.7, 5.6, 6.8, 8.2 };
        public static List<double> E24 = new List<double>() { 1.0, 1.1, 1.2, 1.3, 1.5, 1.6, 1.8, 2.0, 2.2, 2.4, 2.7, 3.0, 3.3, 3.6, 3.9, 4.3, 4.7, 5.1, 5.6, 6.2, 6.8, 7.5, 8.2, 9.1 };
        public static List<double> E48 = new List<double>() { 1.00, 1.05, 1.10, 1.15, 1.21, 1.27, 1.33, 1.40, 1.47, 1.54, 1.62, 1.69, 1.78, 1.87, 1.96, 2.05, 2.15, 2.26, 2.37, 2.49, 2.61, 2.74, 2.87, 3.01, 3.16, 3.32, 3.48, 3.65, 3.83, 4.02, 4.22, 4.42, 4.64, 4.87, 5.11, 5.36, 5.62, 5.90, 6.19, 6.49, 6.81, 7.15, 7.50, 7.87, 8.25, 8.66, 9.09, 9.53 };
        public static List<double> E96 = new List<double>() { 1.00, 1.02, 1.05, 1.07, 1.10, 1.13, 1.15, 1.18, 1.21, 1.24, 1.27, 1.30, 1.33, 1.37, 1.40, 1.43, 1.47, 1.50, 1.54, 1.58, 1.62, 1.65, 1.69, 1.74, 1.78, 1.82, 1.87, 1.91, 1.96, 2.00, 2.05, 2.10, 2.15, 2.21, 2.26, 2.32, 2.37, 2.43, 2.49, 2.55, 2.61, 2.67, 2.74, 2.80, 2.87, 2.94, 3.01, 3.09, 3.16, 3.24, 3.32, 3.40, 3.48, 3.57, 3.65, 3.74, 3.83, 3.92, 4.02, 4.12, 4.22, 4.32, 4.42, 4.53, 4.64, 4.75, 4.87, 4.99, 5.11, 5.23, 5.36, 5.49, 5.62, 5.76, 5.90, 6.04, 6.19, 6.34, 6.49, 6.65, 6.81, 6.98, 7.15, 7.32, 7.50, 7.68, 7.87, 8.06, 8.25, 8.45, 8.66, 8.87, 9.09, 9.31, 9.53, 9.76 };
        public static List<double> ResistorRanges = new List<double>() { 1, 10, 100, 1000, 10000, 100000, 1000000 };
        public static List<double> CapacitorRanges = new List<double>() { 0.000000000001, 0.00000000001, 0.0000000001, 0.000000001, 0.00000001, 0.0000001, 0.000001, 0.00001, 0.0001, 0.001, 0.01, 0.1 };

        public static string MakeNiceUnitString(double V, Units U)
        {
            string UnitName = Enum.GetName(typeof(Units), U);
            if (U == GerberLibrary.Core.Units.None) UnitName = "";
            if (V == 0) return "0" + UnitName;
            int M = 0;
            int M2 = 0;
            double V2 = Math.Abs(V);
            double V3 = V2;
            while (V2 >= 10)
            {
                V2 /= 10;
                M++;
                if (M % 3 == 0) { M2++; V3 /= 1000.0f; };
            }

            if (V2 < 1)
            {
                while (V3 < 1)
                {
                    V2 *= 10;
                    M--;
                    if (M % 3 == 0) { M2--; V3 *= 1000.0f; };
                }
            }
            M = M2;
            List<string> Units = new List<string>() { "p", "n", "u", "m", "", "k", "M", "G", "T" };
            M += 4;
            if (M >= 0 && M < Units.Count)
            {
                UnitName = Units[M] + UnitName;
            }
            return V3.ToString().Replace(',', '.') + " " + UnitName;

        }
        public static void Transform(double dx, double dy, double cx, double cy, double angle, ref double X, ref double Y)
        {

            GerberNumberFormat GNF = new GerberNumberFormat();

            double na = angle * (Math.PI * 2.0) / 360.0; ;
            double SA = Math.Sin(na);
            double CA = Math.Cos(na);
            GNF.Multiplier = 1;
            GerberTransposer.GetTransformedCoord(dx, dy, cx, cy, angle, CA, SA, GNF, true, ref X, ref Y);
            double adx = dx;
            double ady = dy;
        }


        public static Line TransFormLine(Line v, double dx, double dy, float cx, float cy, double angle)
        {
            double x1 = v.x1;
            double x2 = v.x2;
            double y1 = v.y1;
            double y2 = v.y2;
            Transform(dx, dy, cx, cy, angle, ref x1, ref y1);
            Transform(dx, dy, cx, cy, angle, ref x2, ref y2);

            return new Line() { x1 = (float)x1, y1 = (float)y1, x2 = (float)x2, y2 = (float)y2 };

        }

        public static List<PathDefWithClosed> LineSegmentsToPolygons(List<PathDefWithClosed> input, bool joinclosest = true)
        {
            // return input;
            List<PathDefWithClosed> Paths = new List<PathDefWithClosed>();
            List<PathDefWithClosed> FirstSweep = new List<PathDefWithClosed>();
            List<PathDefWithClosed> LeftoverLines = new List<PathDefWithClosed>();
            if (input.Count == 0) return LeftoverLines;
            try
            {
                foreach (var p in input)
                {
                    if (p.Vertices.Count() > 0)
                    {

                        FirstSweep.Add(Sanitize(p));
                    }

                }
                FirstSweep = StripOverlaps(FirstSweep);


                LeftoverLines.Add(FirstSweep[0]);
                for (int i = 1; i < FirstSweep.Count; i++)
                {
                    var LastLeft = LeftoverLines.Last();
                    var LastLeftP = LastLeft.Vertices.Last();

                    if (FirstSweep[i].Vertices.First() == LastLeftP)
                    {
                        LastLeft.Vertices.AddRange(FirstSweep[i].Vertices.Skip(1));
                    }
                    else
                    {
                        if (FirstSweep[i].Vertices.Last() == LastLeftP)
                        {
                            FirstSweep[i].Vertices.Reverse();
                            LastLeft.Vertices.AddRange(FirstSweep[i].Vertices.Skip(1));
                        }
                        else
                        {
                            LeftoverLines.Add(FirstSweep[i]);
                        }
                    }
                }
                LeftoverLines = StripOverlaps(LeftoverLines);
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }

            while (LeftoverLines.Count > 0)
            {
                bool added = false;

                var a = LeftoverLines.Last();
                LeftoverLines.Remove(a);

                if (added == false)
                {
                    PathDefWithClosed P = new PathDefWithClosed();
                    P.Width = a.Width;

                    foreach (var v in a.Vertices)
                    {
                        P.Vertices.Add(v);
                    }
                    //P.Points.Add(a.Last());
                    if (a.Vertices.First() == a.Vertices.Last())
                    {


                        int matching = 0;
                        for (int i = 0; i < a.Vertices.Count / 2; i++)
                        {
                            if (a.Vertices[i] == a.Vertices[a.Vertices.Count - 1 - i]) matching++;
                        }
                        if (matching > 1)
                        {
                            P.Vertices.Clear();
                            List<PointD> DebugN = new List<PointD>();
                            List<int> Kinks = new List<int>();
                            List<int> KinkEnd = new List<int>();
                            
                            Kinks.Add(0);
                            for (int i = 1; i < a.Vertices.Count; i++)
                            {
                                var N1 = MathHelpers.Normal(a.Vertices[(i-1)], a.Vertices[i]);
                                var N2 = MathHelpers.Normal(a.Vertices[i], a.Vertices[(i + 1) % a.Vertices.Count()]);
                                if (N1.Dot(N2) < 0.8)
                                {
                                    DebugN.Add(a.Vertices[i]);
                                    DebugN.Add(N1 * 4.0 + a.Vertices[i]);
                                    Kinks.Add(i);
                                    KinkEnd.Add(i);

                                }
                            }
                            KinkEnd.Add(a.Vertices.Count-1);

                            double maxD = 0;
                            int maxSeg = 0;
                            for(int i =0;i<Kinks.Count;i++)
                            {
                                double D = 0;
                                for (int j = Kinks[i]; j < KinkEnd[i]; j++)
                                {
                                    D += (a.Vertices[j] - a.Vertices[j + 1]).Length();
                                }
                                if (D>maxD)
                                {
                                    maxD = D;
                                    maxSeg = i;
                                }
                            }



                            for (int i = Kinks[maxSeg]; i < KinkEnd[maxSeg]; i++)
                            {
                                P.Vertices.Add(a.Vertices[i]);
                            }

                            bool dumpdebugline = false;
                            if (dumpdebugline)
                            {
                                Bounds Bbox = new Bounds();

                                foreach (var v in a.Vertices)
                                {
                                    Bbox.FitPoint(v);
                                    Bbox.FitPoint(new PointD(v.X + 10, v.Y + 10));
                                    Bbox.FitPoint(new PointD(v.X - 10, v.Y - 10));
                                }
                                float Scaler = 50.0f;
                                Bitmap OutB = new Bitmap((int)(Bbox.Width() * Scaler), (int)(Bbox.Height() * Scaler));

                                Graphics G = Graphics.FromImage(OutB);
                                G.Clear(Color.White);
                                GraphicsGraphicsInterface GGI = new GraphicsGraphicsInterface(G);

                                GGI.TranslateTransform((float)-Bbox.TopLeft.X * Scaler, (float)-Bbox.TopLeft.Y * Scaler);
                                GGI.DrawLines(new Pen(Color.Red, 0.1f), (from i in a.Vertices select (i * Scaler).ToF()).ToArray());
                                for (int i = 1; i < a.Vertices.Count; i++)
                                {

                                    var N = MathHelpers.Normal(a.Vertices[i - 1], a.Vertices[i]);

                                    GGI.DrawString(i.ToString(), new Font("arial", 14), new SolidBrush(Color.FromArgb((i + 128) % 256, i % 256, i % 256)), (float)a.Vertices[i].X * Scaler + (float)N.X * 20.0f, (float)a.Vertices[i].Y * Scaler + (float)N.Y * 20.0f, new StringFormat());
                                }
                                for (int i = 0; i < DebugN.Count; i += 2)
                                {
                                    GGI.DrawLine(new Pen(Color.DarkGreen, 1.0f), (DebugN[i] * Scaler).ToF(), (DebugN[i + 1] * Scaler).ToF());
                                }
                                OutB.Save("testout.png");
                            }

                        }
                        else
                        {
                            a.Vertices.Remove(a.Vertices.Last());
                            //Console.WriteLine("closed path with {0} points during stage 2: {1} reversematched", a.Vertices.Count(), matching);
                            P.Closed = true;
                        }
                    }
                    Paths.Add(P);
                }
            }
            Paths = StripOverlaps(Paths);
            //  return Paths;


            int Merges = 1;
            int startat = 0;
            int lasthigh = 0;
            while (Merges > 0)
            {
                startat = lasthigh;
                Merges = FindNextMerge(Paths, out lasthigh, startat);
            }
            //return Paths;

            int ClosedCount = (from i in Paths where i.Closed == true select i).Count();
            if (ClosedCount < Paths.Count)
            {
                //Console.WriteLine("{0}/{1} paths open - finding closest connections", Paths.Count - ClosedCount, Paths.Count);
                if (joinclosest)
                {

                    Merges = 1;
                    while (Merges > 0)
                    {
                        Merges = FindNextClosest(Paths);
                    }

                    int NewClosedCount = (from i in Paths where i.Closed == true select i).Count();

                    Console.WriteLine("remaining open: {0}", NewClosedCount);
                    /*
                    var OpenPaths = (from i in Paths where i.Closed == false select i).ToArray();
                    //  foreach (var p in OpenPaths)
                    //  {
                    //     Paths.Remove(p);
                    //  }

                    while (OpenPaths.Count() > 1)
                    {
                        var P = OpenPaths[0];
                        var V = P.Points.Last();
                        double ClosestDistance = 1000000;
                        int closestindex = 0;
                        bool forward = true;

                        double OwnDistance = (P.Points.First() - P.Points.Last()).Length();
                        ClosestDistance = OwnDistance;
                        for (int i = 1; i < OpenPaths.Count(); i++)
                        {
                            double dx1 = OpenPaths[i].Points.First().X - V.X;
                            double dy1 = OpenPaths[i].Points.First().Y - V.Y;

                            double dx2 = OpenPaths[i].Points.Last().X - V.X;
                            double dy2 = OpenPaths[i].Points.Last().Y - V.Y;

                            float disttofirst = (float)Math.Sqrt(dx1 * dx1 + dy1 * dy1);
                            float disttolast = (float)Math.Sqrt(dx2 * dx2 + dy2 * dy2);

                            if (disttofirst < ClosestDistance)
                            {
                                forward = true;
                                closestindex = i;
                                ClosestDistance = disttofirst;
                            }

                            if (disttolast < ClosestDistance)
                            {
                                forward = false;
                                closestindex = i;
                                ClosestDistance = disttolast;
                            }

                        }
                        if (OwnDistance == ClosestDistance)
                        {
                            P.Closed = true;
                            //      Paths.Add(P);
                        }
                        else
                        {
                            if (forward)
                            {
                                var PJ = OpenPaths[closestindex];
                                P.Points.AddRange(PJ.Points);
                                Paths.Remove(PJ);
                            }
                            else
                            {
                                var PJ = OpenPaths[closestindex];
                                PJ.Points.Reverse();
                                P.Points.AddRange(PJ.Points);
                                Paths.Remove(PJ);
                            }
                            if (P.Points.First() == P.Points.Last())
                            {
                                P.Closed = true;
                            }
                        }


                        OpenPaths = (from i in Paths where i.Closed == false select i).ToArray();

                    }*/
                }
            }
            List<PathDefWithClosed> Results = new List<PathDefWithClosed>();
            foreach (var p in Paths)
            {
                Results.Add(Sanitize(p));
            }
            return Results;
        }

        private static List<PathDefWithClosed> StripOverlaps(List<PathDefWithClosed> Paths)
        {
            List<PathDefWithClosed> Res = new List<PathDefWithClosed>();
            QuadTreeNode Root = new QuadTreeNode();
            Bounds B = new Bounds();
            for (int i = 0; i < Paths.Count; i++)
            {
                if (Paths[i].Closed == false)
                {
                    foreach (var a in Paths[i].Vertices)
                    {
                        B.FitPoint(a);
                    }
                }
                else
                {
                    Res.Add(Paths[i]);
                }
               
            }

            Root.xstart = B.TopLeft.X - 10;
            Root.xend = B.BottomRight.X + 10;
            Root.ystart = B.TopLeft.Y - 10;
            Root.yend = B.BottomRight.Y + 10;
            RectangleF QueryRect = new RectangleF();
            QueryRect.Width = 3;
            QueryRect.Height = 3;
            List<PathDefWithClosed> ToDelete = new List<PathDefWithClosed>();
            for (int i = 0; i < Paths.Count; i++)
            {
                if (Paths[i].Closed == false)
                {
                    int nearcount = 0;
                    foreach (var a in Paths[i].Vertices)
                    {
                        QueryRect.X = (float)a.X - 1.5f;
                        QueryRect.Y = (float)a.Y - 1.5f;

                        Root.CallBackInside(QueryRect, delegate (QuadTreeItem QI)
                        {
                            var S = QI as SegmentEndContainer;
                            if (S.Point == a)
                            {
                                nearcount++;

                            }
                            else
                            {
                                if (PointD.Distance(a, S.Point) < 0.001)
                                {
                                    nearcount++;
                                }
                            }
                            return true;
                        }
                        );
                    }

                    int max = Math.Max(4, (Paths[i].Vertices.Count * 50) / 100);
              
                    if (nearcount <= max)
                    {
                        foreach (var a in Paths[i].Vertices)
                        {
                            Root.Insert(new SegmentEndContainer() { PathID = i, Point = a, Side = SideEnum.Start }, 8);

                        }
                        Res.Add(Paths[i]);
                    }
                    else
                    {
                        Res.Add(Paths[i]);
                        Console.WriteLine("{4}: {0} out of {1}/{2}/{3}", nearcount, max, Paths[i].Vertices.Count, (Paths[i].Vertices.Count * 90) / 100, i);
                        Console.WriteLine("{0}: skipped!", i);
                    }
                }
            }
            return Res;
        }

        enum SideEnum
        {
            Start,
            End
        }
        class SegmentEndContainer : QuadTreeItem
        {
            public PointD Point;
            public SideEnum Side;
            public int PathID;

            public double x
            {
                get
                {
                    return Point.X;
                }
            }

            public double y
            {
                get
                {
                    return Point.Y;
                }
            }
        }

        private static int FindNextMerge(List<PathDefWithClosed> Paths, out int highestnomatch, int startat = 0)
        {
            highestnomatch = 0;
            QuadTreeNode Root = new QuadTreeNode();
            Bounds B = new Bounds();
            for (int i = startat; i < Paths.Count; i++)
            {
                if (Paths[i].Closed == false)
                {
                    B.FitPoint(Paths[i].Vertices.First());
                    B.FitPoint(Paths[i].Vertices.Last());
                }

            }

            Root.xstart = B.TopLeft.X - 10;
            Root.xend = B.BottomRight.X + 10;
            Root.ystart = B.TopLeft.Y - 10;
            Root.yend = B.BottomRight.Y + 10;

            for (int i = startat; i < Paths.Count; i++)
            {
                if (Paths[i].Closed == false)
                {
                    Root.Insert(new SegmentEndContainer() { PathID = i, Point = Paths[i].Vertices.First(), Side = SideEnum.Start }, 4);
                    Root.Insert(new SegmentEndContainer() { PathID = i, Point = Paths[i].Vertices.Last(), Side = SideEnum.End }, 4);
                }
            }
            RectangleF R = new RectangleF();

            R.Width = 10;
            R.Height = 10;

            for (int i = startat; i < Paths.Count; i++)
            {

                //     Console.WriteLine("checking path {0}", i);
                var P = Paths[i];
                if (P.Closed == false)
                {
                    var PF = P.Vertices.First();
                    //  Console.WriteLine("checking firstvert {0}", PF);

                    R.X = (float)(P.Vertices.First().X - 5);
                    R.Y = (float)(P.Vertices.First().Y - 5);
                    int startmatch = -1;
                    int endmatch = -1;
                    Root.CallBackInside(R, delegate (QuadTreeItem QI)
                        {
                            var S = QI as SegmentEndContainer;
                            if (S.PathID == i) return true;
                            //   Console.WriteLine(" against {0}", S.Point);
                            if (S.Point == PF)
                            {
                                if (S.Side == SideEnum.Start)
                                {
                                    startmatch = S.PathID;
                                    //    Console.WriteLine(" matched start {0}" , startmatch);
                                }
                                else
                                {

                                    endmatch = S.PathID;
                                    //    Console.WriteLine(" matched end {0}", endmatch);
                                }

                            }
                            return true;
                        });

                    if (startmatch > -1 || endmatch > -1)
                    {
                        if (endmatch > -1)
                        {
                            Paths[endmatch].Vertices.Remove(Paths[endmatch].Vertices.Last());
                            Paths[endmatch].Vertices.AddRange(Paths[i].Vertices);
                            if (Paths[endmatch].Vertices.First() == Paths[endmatch].Vertices.Last())
                            {
                                Console.WriteLine("closed path with {0} points during stage 3a", Paths[endmatch].Vertices.Count());
                                Paths[endmatch].Closed = true;
                            }
                            Paths.Remove(Paths[i]);
                            //Console.WriteLine(" a");
                            return 1;
                        }
                        if (startmatch > -1)
                        {
                            Paths[i].Vertices.Reverse();
                            Paths[i].Vertices.Remove(Paths[i].Vertices.Last());
                            Paths[i].Vertices.AddRange(Paths[startmatch].Vertices);
                            if (Paths[i].Vertices.First() == Paths[i].Vertices.Last())
                            {
                                Console.WriteLine("closed path with {0} points during stage 3b", Paths[i].Vertices.Count());
                                Paths[i].Closed = true;
                            }
                            Paths.Remove(Paths[startmatch]);
                            //  Console.WriteLine(" b");

                            return 1;
                        }

                    }

                    PF = P.Vertices.Last();
                    // Console.WriteLine("checking lastvert {0}", PF);
                    R.X = (float)(P.Vertices.First().X - 5);
                    R.Y = (float)(P.Vertices.First().Y - 5);
                    startmatch = -1;
                    endmatch = -1;
                    Root.CallBackInside(R, delegate (QuadTreeItem QI)
                    {
                        var S = QI as SegmentEndContainer;
                        if (S.PathID == i) return true;
                        //  Console.WriteLine(" against {0}", S.Point);

                        if (S.Point == PF)
                        {
                            if (S.Side == SideEnum.Start)
                            {
                                startmatch = S.PathID;
                            }
                            else
                            {

                                endmatch = S.PathID;
                            }

                        }
                        return true;
                    });

                    if (startmatch > -1 || endmatch > -1)
                    {
                        if (endmatch > -1)
                        {
                            Paths[i].Vertices.Reverse();
                            Paths[endmatch].Vertices.Remove(Paths[endmatch].Vertices.Last());
                            Paths[endmatch].Vertices.AddRange(Paths[i].Vertices);
                            if (Paths[endmatch].Vertices.First() == Paths[endmatch].Vertices.Last())
                            {
                                Console.WriteLine("closed path with {0} points during stage 3c", Paths[endmatch].Vertices.Count());
                                Paths[endmatch].Closed = true;
                            }
                            Paths.Remove(Paths[i]);
                            //Console.WriteLine(" c");

                            return 1;
                        }
                        if (startmatch > -1)
                        {
                            Paths[i].Vertices.Remove(Paths[i].Vertices.Last());
                            Paths[i].Vertices.AddRange(Paths[startmatch].Vertices);
                            if (Paths[i].Vertices.First() == Paths[i].Vertices.Last())
                            {
                                Console.WriteLine("closed path with {0} points during stage 3d", Paths[i].Vertices.Count());
                                Paths[i].Closed = true;
                            }
                            Paths.Remove(Paths[startmatch]);
                            //Console.WriteLine(" d");

                            return 1;
                        }

                    }

                    highestnomatch = i;

                }
            }

            return 0;
            //for (int i = 0; i < Paths.Count; i++)
            //{
            //    for (int j = i + 1; j < Paths.Count; j++)
            //    {
            //        if (Paths[i].Closed == false && Paths[j].Closed == false)
            //        {
            //            if (Paths[j].Vertices.First() == Paths[i].Vertices.Last())
            //            {
            //                Paths[i].Vertices.Remove(Paths[i].Vertices.Last());
            //                Paths[i].Vertices.AddRange(Paths[j].Vertices);
            //                if (Paths[i].Vertices.First() == Paths[i].Vertices.Last()) Paths[i].Closed = true;
            //                Paths.Remove(Paths[j]);
            //                return 1;
            //            }
            //            else
            //            {
            //                if (Paths[i].Vertices.First() == Paths[j].Vertices.Last())
            //                {
            //                    Paths[j].Vertices.Remove(Paths[j].Vertices.Last());
            //                    Paths[j].Vertices.AddRange(Paths[i].Vertices);
            //                    if (Paths[j].Vertices.First() == Paths[j].Vertices.Last()) Paths[j].Closed = true;
            //                    Paths.Remove(Paths[i]);
            //                    return 1;

            //                }
            //                else
            //                {
            //                    if (Paths[i].Vertices.First() == Paths[j].Vertices.First())
            //                    {
            //                        Paths[i].Vertices.Reverse();
            //                        Paths[i].Vertices.Remove(Paths[i].Vertices.Last());
            //                        Paths[i].Vertices.AddRange(Paths[j].Vertices);
            //                        if (Paths[i].Vertices.First() == Paths[i].Vertices.Last()) Paths[i].Closed = true;
            //                        Paths.Remove(Paths[j]);
            //                        return 1;

            //                    }
            //                    else
            //                        if (Paths[i].Vertices.Last() == Paths[j].Vertices.Last())
            //                    {
            //                        Paths[i].Vertices.Reverse();
            //                        Paths[j].Vertices.Remove(Paths[j].Vertices.Last());
            //                        Paths[j].Vertices.AddRange(Paths[i].Vertices);
            //                        if (Paths[j].Vertices.First() == Paths[j].Vertices.Last()) Paths[j].Closed = true;
            //                        Paths.Remove(Paths[i]);
            //                        return 1;

            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            //return 0;
        }

        private static int FindNextClosest(List<PathDefWithClosed> Paths)
        {
            QuadTreeNode Root = new QuadTreeNode();
            Bounds B = new Bounds();
            for (int i = 0; i < Paths.Count; i++)
            {
                if (Paths[i].Closed == false)
                {
                    B.FitPoint(Paths[i].Vertices.First());
                    B.FitPoint(Paths[i].Vertices.Last());
                }
            }

            Root.xstart = B.TopLeft.X - 10;
            Root.xend = B.BottomRight.X + 10;
            Root.ystart = B.TopLeft.Y - 10;
            Root.yend = B.BottomRight.Y + 10;

            for (int i = 0; i < Paths.Count; i++)
            {
                if (Paths[i].Closed == false)
                {
                    Root.Insert(new SegmentEndContainer() { PathID = i, Point = Paths[i].Vertices.First(), Side = SideEnum.Start }, 5);
                    Root.Insert(new SegmentEndContainer() { PathID = i, Point = Paths[i].Vertices.Last(), Side = SideEnum.End }, 5);
                }
            }
            RectangleF R = new RectangleF();

            R.Width = 3;
            R.Height = 3;

            for (int i = 0; i < Paths.Count; i++)
            {
                var P = Paths[i];
                if (P.Closed == false && P.Vertices.Count > 1)
                {
                    var PF = P.Vertices[0];
                    var PF2 = P.Vertices[1];

                    var PathDir = PF - PF2;// MathHelpers.Difference(PF, PF2);
                    PathDir.Normalize();

                    R.X = (float)(P.Vertices.First().X - 1.5);
                    R.Y = (float)(P.Vertices.First().Y - 1.5);
                    int startmatch = -1;
                    int endmatch = -1;
                    double closestdistance = B.Width() + B.Height();
                    Root.CallBackInside(R, delegate (QuadTreeItem QI)
                    {
                        var S = QI as SegmentEndContainer;
                        if (S.PathID == i) return true;
                        if (P.Width != Paths[S.PathID].Width) return true;
                        PointD Dir2;
                        if (S.Side == SideEnum.Start)
                        {
                            var S2 = Paths[S.PathID].Vertices[1];
                            Dir2 = S2 - S.Point;
                            Dir2.Normalize();
                        }
                        else
                        {
                            var S2 = Paths[S.PathID].Vertices[Paths[S.PathID].Vertices.Count() - 2];
                            Dir2 = S2 - S.Point;
                            Dir2.Normalize();
                        }

                        double dotted = Dir2.Dot(PathDir);
                        var D = PointD.Distance(S.Point, PF);
                        if (D < 1.0)
                        {
                            //  D -= dotted * 3.0;
                            if (D < closestdistance)
                            {
                                closestdistance = D;
                                if (S.Side == SideEnum.Start)
                                {
                                    startmatch = S.PathID;
                                }
                                else
                                {
                                    endmatch = S.PathID;
                                }
                            }
                        }
                        return true;
                    });

                    if (startmatch > -1 || endmatch > -1)
                    {
                        if (endmatch > -1)
                        {
                            if (closestdistance > 0) Paths[endmatch].Vertices.Remove(Paths[endmatch].Vertices.Last());
                            Paths[endmatch].Vertices.AddRange(Paths[i].Vertices);
                            if (Paths[endmatch].Vertices.First() == Paths[endmatch].Vertices.Last())
                            {
                                Console.WriteLine("closed path with {0} points during stage 4a", Paths[endmatch].Vertices.Count());
                                Paths[endmatch].Closed = true;
                            }
                            Paths.Remove(Paths[i]);
                            // Console.WriteLine(" 4a");
                            return 1;
                        }
                        if (startmatch > -1)
                        {
                            Paths[i].Vertices.Reverse();
                            if (closestdistance > 0) Paths[i].Vertices.Remove(Paths[i].Vertices.Last());
                            Paths[i].Vertices.AddRange(Paths[startmatch].Vertices);
                            if (Paths[i].Vertices.First() == Paths[i].Vertices.Last())
                            {
                                Console.WriteLine("closed path with {0} points during stage 4b", Paths[i].Vertices.Count());
                                Paths[i].Closed = true;
                            }
                            Paths.Remove(Paths[startmatch]);
                            //Console.WriteLine(" 4b");

                            return 1;
                        }

                    }

                    PF = P.Vertices.Last();
                    R.X = (float)(P.Vertices.First().X - 1.5);
                    R.Y = (float)(P.Vertices.First().Y - 1.5);
                    startmatch = -1;
                    endmatch = -1;
                    closestdistance = B.Width() + B.Height();
                    Root.CallBackInside(R, delegate (QuadTreeItem QI)
                    {
                        var S = QI as SegmentEndContainer;
                        if (S.PathID == i) return true;
                        if (P.Width != Paths[S.PathID].Width) return true;

                        var D = PointD.Distance(S.Point, PF);
                        if (D < 1.0 && D < closestdistance)
                        {
                            closestdistance = D;
                            if (S.Side == SideEnum.Start)
                            {
                                startmatch = S.PathID;
                            }
                            else
                            {

                                endmatch = S.PathID;
                            }

                        }
                        return true;
                    });

                    if (startmatch > -1 || endmatch > -1)
                    {
                        if (endmatch > -1)
                        {
                            Paths[i].Vertices.Reverse();
                            if (closestdistance > 0) Paths[endmatch].Vertices.Remove(Paths[endmatch].Vertices.Last());
                            Paths[endmatch].Vertices.AddRange(Paths[i].Vertices);
                            if (Paths[endmatch].Vertices.First() == Paths[endmatch].Vertices.Last())
                            {
                                Console.WriteLine("closed path with {0} points during stage 4c", Paths[endmatch].Vertices.Count());
                                Paths[endmatch].Closed = true;
                            }
                            Paths.Remove(Paths[i]);
                            //  Console.WriteLine(" 4c");

                            return 1;
                        }
                        if (startmatch > -1)
                        {
                            if (closestdistance > 0) Paths[i].Vertices.Remove(Paths[i].Vertices.Last());
                            Paths[i].Vertices.AddRange(Paths[startmatch].Vertices);
                            if (Paths[i].Vertices.First() == Paths[i].Vertices.Last())
                            {
                                Console.WriteLine("closed path with {0} points during stage 4d", Paths[i].Vertices.Count());
                                Paths[i].Closed = true;
                            }
                            Paths.Remove(Paths[startmatch]);
                            // Console.WriteLine(" 4d");

                            return 1;
                        }

                    }

                }
            }

            return 0;

        }

        public static double PolygonSurfaceArea(List<PointD> Polygon)
        {
            List<PointD> P = new List<PointD>();
            foreach (var p in Polygon) P.Add(p);
            double area = 0;
            if (Polygon[0] != Polygon[Polygon.Count - 1]) P.Add(Polygon[0]);

            for (int i = 1; i < P.Count - 1; ++i)
                area += P[i].X * (P[i + 1].Y - P[i - 1].Y);

            area /= 2;
            return area;
        }

        public static double DistanceSq(PointD A, PointD B)
        {
            double dx = B.X - A.X;
            double dy = B.Y - A.Y;
            return dx * dx + dy * dy;

        }

        public static PointD SegmentSegmentIntersect(PointD P0, PointD P1, PointD P2, PointD P3)
        {
            PointD S1 = P1 - P0;
            PointD S2 = P3 - P2;


            double s, t;
            s = (-S1.Y * (P0.X - P2.X) + S1.X * (P0.Y - P2.Y)) / (-S2.X * S1.Y + S1.X * S2.Y);
            t = (S2.X * (P0.Y - P2.Y) - S2.Y * (P0.X - P2.X)) / (-S2.X * S1.Y + S1.X * S2.Y);

            if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
            {
                double X = P0.X + (t * S1.X); ;
                double Y = P0.Y + (t * S1.Y);
                return new PointD(X, Y);
            }

            return null; // No collision

        }

        public static double HermiteInterpolate(double y0, double y1, double y2, double y3, double mu)
        {
            var mu2 = mu * mu;

            var a0 = -0.5f * y0 + 1.5f * y1 - 1.5f * y2 + 0.5f * y3;
            var a1 = y0 - 2.5f * y1 + 2f * y2 - 0.5f * y3;
            var a2 = -0.5f * y0 + 0.5f * y2;
            var a3 = y1;

            return (a0 * mu * mu2) + (a1 * mu2) + (a2 * mu) + a3;
        }

        public static PointD HermitePoint(PointD A, PointD B, PointD C, PointD D, double mu)
        {
            PointD R = new PointD();
            R.X = HermiteInterpolate(A.X, B.X, C.X, D.X, mu);
            R.Y = HermiteInterpolate(A.Y, B.Y, C.Y, D.Y, mu);

            return R;
        }

        public static PointD BezierPoint(PointD p1, PointD p2, PointD p3, PointD p4, double t)
        {
            PointD R = new PointD();
            double t1 = 1 - t;
            double t1_3 = t1 * t1 * t1;
            double t1_3a = (3 * t) * (t1 * t1);
            double t1_3b = (3 * (t * t)) * t1;
            double t1_3c = (t * t * t);


            double x = t1_3 * p1.X;
            x = x + t1_3a * p2.X;
            x = x + t1_3b * p3.X;
            x = x + t1_3c * p4.X;

            double y = t1_3 * p1.Y;
            y = y + t1_3a * p2.Y;
            y = y + t1_3b * p3.Y;
            y = y + t1_3c * p4.Y;

            R.X = x;
            R.Y = y;
            return R;
        }

        public static PointD Normalize(PointD inp)
        {
            PointD R = new PointD();

            double Dist = (double)Math.Sqrt(Helpers.DistanceSq(new PointD(0, 0), inp));
            if (Dist > 0)
            {
                R.X = inp.X / Dist;
                R.Y = inp.Y / Dist;
            }
            return R;
        }

        public static bool IsInPolygon(List<PointD> polygon, PointD testPoint, bool reverse = false)
        {
            if (reverse)
            {
                List<PointD> P = new List<PointD>();
                foreach (var p in polygon) { P.Add(p); };
                P.Reverse();
                return IsInPolygon(P, testPoint, false);
            }
            bool result = false;
            int j = polygon.Count() - 1;
            for (int i = 0; i < polygon.Count(); i++)
            {
                if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
                {
                    if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        public static bool IsInPolygona(List<PointD> poly, PointD p)
        {
            PointD p1, p2;


            bool inside = false;


            if (poly.Count < 3)
            {
                return inside;
            }


            var oldPoint = new PointD(
                poly[poly.Count - 1].X, poly[poly.Count - 1].Y);


            for (int i = 0; i < poly.Count; i++)
            {
                var newPoint = new PointD(poly[i].X, poly[i].Y);


                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;

                    p2 = newPoint;
                }

                else
                {
                    p1 = newPoint;

                    p2 = oldPoint;
                }


                if ((newPoint.X < p.X) == (p.X <= oldPoint.X)
                    && (p.Y - p1.Y) * (p2.X - p1.X)
                    < (p2.Y - p1.Y) * (p.X - p1.X))
                {
                    inside = !inside;
                }


                oldPoint = newPoint;
            }


            return inside;
        }

        public static PointD ClosestIntersection(float cx, float cy, float radius, PointD lineStart, PointD lineEnd)
        {
            PointD intersection1;
            PointD intersection2;
            int intersections = FindLineCircleIntersections(cx, cy, radius, lineStart, lineEnd, out intersection1, out intersection2);

            if (intersections == 1)
                return intersection1;//one intersection

            if (intersections == 2)
            {
                double dist1 = Distance(intersection1, lineStart);
                double dist2 = Distance(intersection2, lineStart);

                if (dist1 < dist2)
                    return intersection1;
                else
                    return intersection2;
            }

            return new PointD(0, 0);// no intersections at all
        }

        public static double Distance(PointD p1, PointD p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        public static int FindLineCircleIntersections(double cx, double cy, double radius, PointD point1, PointD point2, out PointD intersection1, out PointD intersection2)
        {
            double dx, dy, A, B, C, det, t;

            dx = point2.X - point1.X;
            dy = point2.Y - point1.Y;

            A = dx * dx + dy * dy;
            B = 2 * (dx * (point1.X - cx) + dy * (point1.Y - cy));
            C = (point1.X - cx) * (point1.X - cx) + (point1.Y - cy) * (point1.Y - cy) - radius * radius;

            det = B * B - 4 * A * C;
            if ((A <= 0.0000001) || (det < 0))
            {
                // No real solutions.
                intersection1 = new PointD(double.NaN, double.NaN);
                intersection2 = new PointD(double.NaN, double.NaN);
                return 0;
            }
            else if (det == 0)
            {
                // One solution.
                t = -B / (2 * A);
                if (t < 0 || t > 1)
                {
                    intersection1 = new PointD(double.NaN, double.NaN);
                    intersection2 = new PointD(double.NaN, double.NaN);
                    return 0;
                }
                intersection1 = new PointD(point1.X + t * dx, point1.Y + t * dy);
                intersection2 = new PointD(double.NaN, double.NaN);
                return 1;
            }
            else
            {
                // Two solutions.
                double t1 = (double)((-B + Math.Sqrt(det)) / (2 * A));
                if (t1 < 0 || t1 > 1.0)
                {
                    double t2 = (double)((-B - Math.Sqrt(det)) / (2 * A));
                    if (t2 < 0 || t2 > 1.0)
                    {
                        intersection1 = new PointD(double.NaN, double.NaN);
                        intersection2 = new PointD(double.NaN, double.NaN);
                        return 0;
                    }
                    else
                    {
                        intersection1 = new PointD(point1.X + t2 * dx, point1.Y + t2 * dy);
                        intersection2 = new PointD(double.NaN, double.NaN);
                        return 1;
                    }
                }
                else
                {
                    intersection1 = new PointD(point1.X + t1 * dx, point1.Y + t1 * dy);
                    double t2 = (double)((-B - Math.Sqrt(det)) / (2 * A));
                    if (t2 < 0 || t2 > 1.0)
                    {
                        intersection2 = new PointD(double.NaN, double.NaN);
                        return 1;
                    }
                    else
                    {
                        intersection2 = new PointD(point1.X + t2 * dx, point1.Y + t2 * dy);
                        return 2;
                    }
                }
            }
        }

        public static double AngleBetween(PointD a, PointD b)
        {
            return Math.Atan2(a.Y - b.Y, a.X - b.X);
        }

        public static void DrawMMGrid(GraphicsInterface G, float PW, float width, float height, float minorGrid, float majorGrid)
        {
            if (minorGrid < 0) return;

            Pen P = new Pen(System.Drawing.ColorTranslator.FromHtml("#c4e5ff"), PW * 0.5f);
            for (float X = 0; X <= width; X += minorGrid)
            {
                G.DrawLine(P, X, 0, X, height);
            }
            for (float X = 0; X <= height; X += minorGrid)
            {
                G.DrawLine(P, 0.0f, X, width, X);
            }
            P = new Pen(System.Drawing.ColorTranslator.FromHtml("#bad7ed"), PW);
            for (float X = 0; X <= width; X += majorGrid)
            {
                G.DrawLine(P, X, 0, X, height);
            }
            for (float X = 0; X <= height; X += majorGrid)
            {
                G.DrawLine(P, 0.0f, X, width, X);
            }
        }
    }

}
