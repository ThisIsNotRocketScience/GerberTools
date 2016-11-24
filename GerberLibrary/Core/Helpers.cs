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
    public static class Helpers
    {
        public static List<PointD> Sanitize(List<PointD> inp)
        {
            List<PointD> R = new List<PointD>();
            PointD last = new PointD(double.NaN, double.NaN);
            foreach (var v in inp)
            {
                if (v != last)
                {
                    R.Add(v);
                    last = v.Copy();
                }
            }
            return R;
        }

        private class PathDefWithClosed
        {
            public bool Closed = false;
            public List<PointD> Points = new List<PointD>();
            public override string ToString()
            {
                string r = "";
                foreach (var a in Points)
                    r += a.ToString() + "  ";
                return string.Format("closed: {0} verts: {1}", Closed, r);
            }
        }

        public static List<List<PointD>> LineSegmentsToPolygons(List<List<PointD>> input, bool joinclosest = true)
        {
            List<PathDefWithClosed> Paths = new List<PathDefWithClosed>();
            List<List<PointD>> FirstSweep = new List<List<PointD>>();
            List<List<PointD>> LeftoverLines = new List<List<PointD>>();
            if (input.Count == 0) return LeftoverLines;
            try
            {
                foreach (var p in input)
                {
                    if (p.Count > 0)
                    {

                        FirstSweep.Add(Sanitize(p));
                    }

                }
                LeftoverLines.Add(FirstSweep[0]);
                for (int i = 1; i < FirstSweep.Count; i++)
                {
                    if (FirstSweep[i].First() == LeftoverLines.Last().Last())
                    {
                        LeftoverLines.Last().AddRange(FirstSweep[i].Skip(1));
                    }
                    else
                    {
                        if (FirstSweep[i].Last() == LeftoverLines.Last().Last())
                        {
                            FirstSweep[i].Reverse();
                            LeftoverLines.Last().AddRange(FirstSweep[i].Skip(1));
                        }
                        else
                        {

                            LeftoverLines.Add(FirstSweep[i]);
                        }
                    }
                }
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
                #region oldcode
                //foreach (var P in Paths)
                //{
                //    if (added == false && P.Closed == false)
                //    {
                //        if (P.Points[0] == a.First())
                //        {
                //            if (P.Points.Last() == a.Last())
                //            {
                //                P.Closed = true;
                //            }
                //            else
                //            {
                //                P.Points.Insert(0, a.First());
                //            }
                //            added = true;
                //        }
                //        else
                //            if (P.Points[0] == a.Last())
                //            {
                //                P.Points.Insert(0, a.First());
                //                added = true;
                //            }
                //            else
                //                if (P.Points.Last() == a.First())
                //                {
                //                    P.Points.Add(a.Last());
                //                    added = true;
                //                }
                //                else
                //                    if (P.Points.Last() == a.Last())
                //                    {
                //                        P.Points.Add(a.First());
                //                        added = true;
                //                    }
                //                    else
                //                    {
                //                    }
                //    }
                //}
                #endregion
                if (added == false)
                {
                    PathDefWithClosed P = new PathDefWithClosed();
                    foreach (var v in a)
                    {
                        P.Points.Add(v);
                    }
                    //P.Points.Add(a.Last());
                    if (a.First() == a.Last())
                    {
                        int matching = 0;
                        for (int i = 0; i < a.Count / 2;i++)
                        {
                            if (a[i] == a[a.Count - 1 - i]) matching++;
                        }
                        if (matching > 1)
                        {
                            P.Points.Clear();
                            for(int i =0;i<(a.Count+1)/2;i++)
                            {
                                P.Points.Add(a[i]);
                            }
                            
                        }
                        else
                        {
//                            Console.WriteLine("closed path with {0} points during stage 2: {1} reversematched", a.Count(), matching);
                            P.Closed = true;
                        }
                    }
                    Paths.Add(P);
                }
            }

            int Merges = 1;
            while (Merges > 0)
            {
                Merges = FindNextMerge(Paths);
            }
            
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
            List<List<PointD>> Results = new List<List<PointD>>();
            foreach (var p in Paths)
            {
                Results.Add(Sanitize(p.Points));
            }
            return Results;
        }


        enum SideEnum
        {
            Start,
            End
        }
        class SegmentEndContainer: QuadTreeItem
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

        private static int FindNextMerge(List<PathDefWithClosed> Paths)
        {

            QuadTreeNode Root = new QuadTreeNode();
            Bounds B = new Bounds();
            for (int i = 0; i < Paths.Count; i++)
            {
                if (Paths[i].Closed == false)
                {
                    B.FitPoint(Paths[i].Points.First());
                    B.FitPoint(Paths[i].Points.Last());
                }
                
            }

            Root.xstart = B.TopLeft.X-1;
            Root.xend= B.BottomRight.X+1;
            Root.ystart = B.TopLeft.Y-1;
            Root.yend = B.BottomRight.Y+1;

            for (int i = 0; i < Paths.Count; i++)
            {
                if (Paths[i].Closed == false)
                {
                    Root.Insert(new SegmentEndContainer() { PathID = i, Point = Paths[i].Points.First(), Side = SideEnum.Start }, 4);
                    Root.Insert(new SegmentEndContainer() { PathID = i, Point = Paths[i].Points.Last(), Side = SideEnum.End }, 4);
                }
            }
            RectangleF R = new RectangleF();

            R.Width = 10;
            R.Height = 10;

            for (int i = 0; i < Paths.Count; i++)
            {
               // Console.WriteLine("checking path {0}", i);
                var P = Paths[i];
                if (P.Closed == false)
                {
                    var PF = P.Points.First();
                    //Console.WriteLine("checking firstvert {0}", PF);

                    R.X = (float)(P.Points.First().X - 5);
                    R.Y = (float)(P.Points.First().Y -5);
                    int startmatch = -1;
                    int endmatch = -1;
                    Root.CallBackInside(R, delegate (QuadTreeItem QI) 
                        {
                            var S = QI as SegmentEndContainer;
                            if (S.PathID == i) return true;
                           // Console.WriteLine(" against {0}", S.Point);
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

                    if (startmatch >-1 || endmatch > -1)
                    {
                        if (endmatch>-1)
                        {
                            Paths[endmatch].Points.Remove(Paths[endmatch].Points.Last());
                            Paths[endmatch].Points.AddRange(Paths[i].Points);
                            if (Paths[endmatch].Points.First() == Paths[endmatch].Points.Last())
                            {
                                //Console.WriteLine("closed path with {0} points during stage 3a", Paths[endmatch].Points.Count());
                                Paths[endmatch].Closed = true;
                            }
                            Paths.Remove(Paths[i]);
                            //Console.WriteLine(" a");
                            return 1;
                        }
                        if (startmatch >-1)
                        {
                            Paths[i].Points.Reverse();
                            Paths[i].Points.Remove(Paths[i].Points.Last());
                            Paths[i].Points.AddRange(Paths[startmatch].Points);
                            if (Paths[i].Points.First() == Paths[i].Points.Last())
                            {
                            //    Console.WriteLine("closed path with {0} points during stage 3b", Paths[i].Points.Count());
                                Paths[i].Closed = true;
                            }
                            Paths.Remove(Paths[startmatch]);
                          //  Console.WriteLine(" b");

                            return 1;
                        }
                        
                    }

                     PF = P.Points.Last();
                   // Console.WriteLine("checking lastvert {0}", PF);
                    R.X = (float)(P.Points.First().X - 5);
                    R.Y = (float)(P.Points.First().Y - 5);
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
                            Paths[i].Points.Reverse();
                            Paths[endmatch].Points.Remove(Paths[endmatch].Points.Last());
                            Paths[endmatch].Points.AddRange(Paths[i].Points);
                            if (Paths[endmatch].Points.First() == Paths[endmatch].Points.Last())
                            {
                              //  Console.WriteLine("closed path with {0} points during stage 3c", Paths[endmatch].Points.Count());
                                Paths[endmatch].Closed = true;
                            }
                            Paths.Remove(Paths[i]);
                            //Console.WriteLine(" c");

                            return 1;
                        }
                        if (startmatch > -1)
                        {
                            Paths[i].Points.Remove(Paths[i].Points.Last());
                            Paths[i].Points.AddRange(Paths[startmatch].Points);
                            if (Paths[i].Points.First() == Paths[i].Points.Last())
                            {
                                Console.WriteLine("closed path with {0} points during stage 3d", Paths[i].Points.Count());
                                Paths[i].Closed = true;
                            }
                            Paths.Remove(Paths[startmatch]);
                            //Console.WriteLine(" d");

                            return 1;
                        }
                       
                    }

                }
            }
            
            return 0;
            for (int i = 0; i < Paths.Count; i++)
            {
                for (int j = i + 1; j < Paths.Count; j++)
                {
                    if (Paths[i].Closed == false && Paths[j].Closed == false)
                    {
                        if (Paths[j].Points.First() == Paths[i].Points.Last())
                        {
                            Paths[i].Points.Remove(Paths[i].Points.Last());
                            Paths[i].Points.AddRange(Paths[j].Points);
                            if (Paths[i].Points.First() == Paths[i].Points.Last()) Paths[i].Closed = true;
                            Paths.Remove(Paths[j]);
                            return 1;
                        }
                        else
                        {
                            if (Paths[i].Points.First() == Paths[j].Points.Last())
                            {
                                Paths[j].Points.Remove(Paths[j].Points.Last());
                                Paths[j].Points.AddRange(Paths[i].Points);
                                if (Paths[j].Points.First() == Paths[j].Points.Last()) Paths[j].Closed = true;
                                Paths.Remove(Paths[i]);
                                return 1;

                            }
                            else
                            {
                                if (Paths[i].Points.First() == Paths[j].Points.First())
                                {
                                    Paths[i].Points.Reverse();
                                    Paths[i].Points.Remove(Paths[i].Points.Last());
                                    Paths[i].Points.AddRange(Paths[j].Points);
                                    if (Paths[i].Points.First() == Paths[i].Points.Last()) Paths[i].Closed = true;
                                    Paths.Remove(Paths[j]);
                                    return 1;

                                }
                                else
                                    if (Paths[i].Points.Last() == Paths[j].Points.Last())
                                {
                                    Paths[i].Points.Reverse();
                                    Paths[j].Points.Remove(Paths[j].Points.Last());
                                    Paths[j].Points.AddRange(Paths[i].Points);
                                    if (Paths[j].Points.First() == Paths[j].Points.Last()) Paths[j].Closed = true;
                                    Paths.Remove(Paths[i]);
                                    return 1;

                                }
                            }
                        }
                    }
                }
            }

            return 0;
        }

        private static int FindNextClosest(List<PathDefWithClosed> Paths)
        {
            QuadTreeNode Root = new QuadTreeNode();
            Bounds B = new Bounds();
            for (int i = 0; i < Paths.Count; i++)
            {
                if (Paths[i].Closed == false)
                {
                    B.FitPoint(Paths[i].Points.First());
                    B.FitPoint(Paths[i].Points.Last());
                }

            }

            Root.xstart = B.TopLeft.X;
            Root.xend = B.BottomRight.X;
            Root.ystart = B.TopLeft.Y;
            Root.yend = B.BottomRight.Y;

            for (int i = 0; i < Paths.Count; i++)
            {
                if (Paths[i].Closed == false)
                {
                    Root.Insert(new SegmentEndContainer() { PathID = i, Point = Paths[i].Points.First(), Side = SideEnum.Start }, 4);
                    Root.Insert(new SegmentEndContainer() { PathID = i, Point = Paths[i].Points.Last(), Side = SideEnum.End }, 4);
                }
            }
            RectangleF R = new RectangleF();

            R.Width = 30;
            R.Height = 30;

            for (int i = 0; i < Paths.Count; i++)
            {
                var P = Paths[i];
                if (P.Closed == false)
                {
                    var PF = P.Points.First();
                    R.X = (float)(P.Points.First().X - 15);
                    R.Y = (float)(P.Points.First().Y - 15);
                    int startmatch = -1;
                    int endmatch = -1;
                    double closestdistance = B.Width() + B.Height();
                    Root.CallBackInside(R, delegate (QuadTreeItem QI)
                    {
                        var S = QI as SegmentEndContainer;
                        if (S.PathID == i) return true;
                        var D = PointD.Distance(S.Point, PF);
                        if (D<1.0 &&  D < closestdistance )
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
                            Paths[endmatch].Points.Remove(Paths[endmatch].Points.Last());
                            Paths[endmatch].Points.AddRange(Paths[i].Points);
                            if (Paths[endmatch].Points.First() == Paths[endmatch].Points.Last())
                            {
                                Console.WriteLine("closed path with {0} points during stage 4a", Paths[endmatch].Points.Count());
                                Paths[endmatch].Closed = true;
                            }
                            Paths.Remove(Paths[i]);
                            Console.WriteLine(" 4a");
                            return 1;
                        }
                        if (startmatch > -1)
                        {
                            Paths[i].Points.Reverse();
                            Paths[i].Points.Remove(Paths[i].Points.Last());
                            Paths[i].Points.AddRange(Paths[startmatch].Points);
                            if (Paths[i].Points.First() == Paths[i].Points.Last())
                            {
                                Console.WriteLine("closed path with {0} points during stage 4b", Paths[i].Points.Count());
                                Paths[i].Closed = true;
                            }
                            Paths.Remove(Paths[startmatch]);
                            Console.WriteLine(" 4b");

                            return 1;
                        }

                    }

                    PF = P.Points.Last();
                    R.X = (float)(P.Points.First().X - 0.5);
                    R.Y = (float)(P.Points.First().Y - 0.5);
                    startmatch = -1;
                    endmatch = -1;
                    closestdistance = B.Width() + B.Height();
                    Root.CallBackInside(R, delegate (QuadTreeItem QI)
                    {
                        var S = QI as SegmentEndContainer;
                        if (S.PathID == i) return true;
                        var D = PointD.Distance(S.Point, PF);
                        if (D <1.0 && D < closestdistance)
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
                            Paths[i].Points.Reverse();
                            Paths[endmatch].Points.Remove(Paths[endmatch].Points.Last());
                            Paths[endmatch].Points.AddRange(Paths[i].Points);
                            if (Paths[endmatch].Points.First() == Paths[endmatch].Points.Last())
                            {
                                Console.WriteLine("closed path with {0} points during stage 4c", Paths[endmatch].Points.Count());
                                Paths[endmatch].Closed = true;
                            }
                            Paths.Remove(Paths[i]);
                            Console.WriteLine(" 4c");

                            return 1;
                        }
                        if (startmatch > -1)
                        {
                            Paths[i].Points.Remove(Paths[i].Points.Last());
                            Paths[i].Points.AddRange(Paths[startmatch].Points);
                            if (Paths[i].Points.First() == Paths[i].Points.Last())
                            {
                                Console.WriteLine("closed path with {0} points during stage 4d", Paths[i].Points.Count());
                                Paths[i].Closed = true;
                            }
                            Paths.Remove(Paths[startmatch]);
                            Console.WriteLine(" 4d");

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

        public static void DrawMMGrid(GraphicsInterface G, float PW, float width, float height)
        {

            Pen P = new Pen(System.Drawing.ColorTranslator.FromHtml("#c4e5ff"), PW * 0.5f);
            for (float X = 0; X <= width; X += 1)
            {
                G.DrawLine(P, X, 0, X, height);
            }
            for (float X = 0; X <= height; X += 1)
            {
                G.DrawLine(P, 0.0f, X, width, X);
            }
            P = new Pen(System.Drawing.ColorTranslator.FromHtml("#bad7ed"), PW);
            for (float X = 0; X <= width; X += 10)
            {
                G.DrawLine(P, X, 0, X, height);
            }
            for (float X = 0; X <= height; X += 10)
            {
                G.DrawLine(P, 0.0f, X, width, X);
            }
        }
    }

}
