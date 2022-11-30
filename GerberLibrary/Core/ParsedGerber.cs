using ClipperLib;
using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using TriangleNet.Geometry;
using static GerberLibrary.PolyLineSet;
using ClipperPolygon = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;


using TriangleNet.IO;
using TriangleNet.Meshing;


namespace GerberLibrary.Core
{
    public class ParsedGerber
    {
        public override string ToString()
        {
            return String.Format("{0} {1}: {2}", Side, Layer, Path.GetFileName(Name));
        }
        public PointD TranslationSinceLoad = new PointD();
        public GerberParserState State;
        public double PathLength()
        {
            double len = 0;
            for (int i = 0; i < Shapes.Count - 1; i++)
            {
                PolyLine A = Shapes[i];
                PolyLine B = Shapes[i + 1];
                len += (double)Math.Sqrt(Helpers.DistanceSq(A.end(), B.start()));
            }
            return len;
        }

        public Dictionary<string, GerberApertureType> GetApertureHashTable()
        {
            Dictionary<string, GerberApertureType> R = new Dictionary<string, GerberApertureType>();

            foreach(var a in State.Apertures)
            {
                R[a.Value.GetApertureHash()] = a.Value;
            }
            return R;
        }

        // brute force path optimization. This should be rewritten with some nice treesearching algo.. but this is fast enough to "not matter" for now.

        public void Optimize()
        {
            double BeforeTotal = PathLength();
            Random R = new Random();
            for (int i = 0; i < 100000; i++)
            {
                int idx1 = R.Next(Shapes.Count() - 3);
                int idx2 = R.Next(Shapes.Count() - 3);
                if (idx1 != idx2)
                {
                    if (idx2 < idx1)
                    {
                        int t = idx2;
                        idx2 = idx1;
                        idx1 = t;
                    }
                    if (idx2 > idx1)
                    {
                        int idx1b = idx1 + 1;
                        int idx2b = idx2 + 1;
                        int idx1c = idx1b + 1;
                        int idx2c = idx2b + 1;
                        PolyLine Aa = Shapes[idx1];
                        PolyLine Ab = Shapes[idx1b];
                        PolyLine Ac = Shapes[idx1c];

                        PolyLine Ba = Shapes[idx2];
                        PolyLine Bb = Shapes[idx2b];
                        PolyLine Bc = Shapes[idx2c];

                        double before = Helpers.DistanceSq(Aa.end(), Ab.start()) +
                                       Helpers.DistanceSq(Ab.end(), Ac.start())
                                       +
                                       Helpers.DistanceSq(Ba.end(), Bb.start()) +
                                       Helpers.DistanceSq(Bb.end(), Bc.start())

                            ;
                        double after = Helpers.DistanceSq(Ba.end(), Ab.start()) +
                                       Helpers.DistanceSq(Ab.end(), Bc.start())
                                       +
                                       Helpers.DistanceSq(Aa.end(), Bb.start()) +
                                       Helpers.DistanceSq(Bb.end(), Ac.start())

                            ;
                        if (after < before)
                        {
                            Shapes[idx1b] = Bb;
                            Shapes[idx2b] = Ab;
                        }
                    }
                }
            }
            double AfterTotal = PathLength();
            //Console.WriteLine("Optimized path - before: {0}, after: {1}", BeforeTotal, AfterTotal);
        }

        internal void FlipXY()
        {
            foreach (var a in DisplayShapes)
            {
                a.FlipXY();
            }

            foreach (var a in OutlineShapes)
            {
                a.FlipXY(); 
            }
            foreach (var a in Shapes)
            {
                a.FlipXY(); 
            }
            foreach (var a in ZerosizePoints)
            {
                var T = a.X;
                a.X = a.Y;
                a.Y = T;
            }

        }

        internal void FlipX()
        {
            foreach (var a in DisplayShapes)
            {
                a.FlipX();
            }

            foreach (var a in OutlineShapes)
            {
                a.FlipX();
            }

            foreach (var a in Shapes)
            {
                a.FlipX();
            }
            
            foreach (var a in ZerosizePoints)
            {
                a.X = -a.X;
            }
        }


        public void DefaultShape()
        {
            Shapes.Clear();
            PolyLine S1 = new PolyLine(State.LastShapeID++);
            S1.Add(0, 10);
            S1.Add(20, 10);
            S1.Add(20, 20);
            S1.Add(0, 20);
            S1.Add(0, 10);
            Shapes.Add(S1);
            PolyLine S2 = new PolyLine(State.LastShapeID++);

            for (int i = 0; i < 101; i++)
            {
                double p = (double)i * 0.06283f;
                S2.Add(10.0f + (double)Math.Sin(p) * 5.0f, 15.0f - (double)Math.Cos(p) * 5.0f);
            }
            Shapes.Add(S2);

            BuildBoundary();
        }

        internal void CopyFrom(ParsedGerber p)
        {
            Shapes = CopyPolylines(p.Shapes);
            DisplayShapes = CopyPolylines(p.DisplayShapes);
            OutlineShapes = CopyPolylines(p.OutlineShapes);
            ZerosizePoints = CopyPoints(p.ZerosizePoints);
        }

        private List<PointD> CopyPoints(List<PointD> zerosizePoints)
        {
            return (from i in zerosizePoints select new PointD(i.X, i.Y)).ToList();
        }

        private List<PolyLine> CopyPolylines(List<PolyLine> shapes)
        {
            return (from i in shapes select i.Copy()).ToList();
        }

        public List<PolyLine> Shapes = new List<PolyLine>();
        public List<PolyLine> DisplayShapes = new List<PolyLine>();
        public List<PolyLine> OutlineShapes = new List<PolyLine>();
        public List<PointD> ZerosizePoints = new List<PointD>();

        public Bounds BoundingBox = new Bounds();
        public BoardSide Side;
        public BoardLayer Layer;
        public string Name;

        //
        public List<string> OriginalLines = new List<string>();

        public bool IsOutline()
        {
            if (Layer == BoardLayer.Outline) return true;
            if (Side == BoardSide.Both) return true;
            return false;
        }

        public void CalcPathBounds()
        {
            BoundingBox.Reset();
            BoundingBox.AddPolyLines(Shapes);
            BoundingBox.AddPolyLines(DisplayShapes);
            BoundingBox.AddPolyLines(OutlineShapes);
            BoundingBox.FitPoint(ZerosizePoints);
        }

        public void FixPolygonWindings()
        {
            Polygons solution2 = new Polygons();
            if (false)
            {
                for (int i = 0; i < DisplayShapes.Count; i++)
                {
                    Polygons clips = new Polygons();

                    clips.Add(DisplayShapes[i].toPolygon());
                    Clipper cp = new Clipper();
                    cp.AddPolygons(solution2, PolyType.ptSubject);
                    cp.AddPolygons(clips, PolyType.ptClip);

                    cp.Execute(ClipType.ctXor, solution2, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
                }
            }
            else
            {

                Polygons clips = new Polygons();
                for (int i = 0; i < DisplayShapes.Count; i++)
                {

                    clips.Add(DisplayShapes[i].toPolygon());
                }
                Clipper cp = new Clipper();
                cp.AddPolygons(solution2, PolyType.ptSubject);
                cp.AddPolygons(clips, PolyType.ptClip);

                cp.Execute(ClipType.ctXor, solution2, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
            }


            OutlineShapes.Clear();

            for (int i = 0; i < solution2.Count; i++)
            {
                int ID = i;
                if (State != null) ID = State.LastShapeID++;
                PolyLine PL = new PolyLine(ID);
                PL.fromPolygon(solution2[i]);

                // if (Clipper.Orientation(solution2[i]) == false)
                // {
                //    //     PL.Vertices.Reverse();
                // }

                PL.MyColor = Color.FromArgb(255, 200, 128, 0);
                OutlineShapes.Add(PL);
            }
        }



        internal PolyLine GetBoundingPolyLine()
        {
            PolyLine Boundary = new PolyLine(State.LastShapeID++);

            Boundary.Add(BoundingBox.TopLeft.X, BoundingBox.TopLeft.Y);
            Boundary.Add(BoundingBox.BottomRight.X, BoundingBox.TopLeft.Y);
            Boundary.Add(BoundingBox.BottomRight.X, BoundingBox.BottomRight.Y);
            Boundary.Add(BoundingBox.TopLeft.X, BoundingBox.BottomRight.Y);
            Boundary.Close();

            return Boundary;
        }

        public void Translate(PointD T)
        {
            TranslationSinceLoad.X += T.X;
            TranslationSinceLoad.Y += T.Y;

            foreach (var a in DisplayShapes)
            {
                a.Translate(T.X, T.Y);
            }

            foreach (var a in OutlineShapes)
            {
                a.Translate(T.X, T.Y);
            }
            foreach (var a in Shapes)
            {
                a.Translate(T.X, T.Y);
            }
            foreach (var a in ZerosizePoints)
            {
                a.X += T.X;
                a.Y += T.Y;
            }

        }
        public PointD Normalize()
        {

            PointD t = new PointD(-BoundingBox.TopLeft.X, -BoundingBox.TopLeft.Y);
            // return;
            TranslationSinceLoad.X -= BoundingBox.TopLeft.X;
            TranslationSinceLoad.Y -= BoundingBox.TopLeft.Y;

            foreach (var a in DisplayShapes)
            {
                a.MoveBack(BoundingBox.TopLeft);
            }

            foreach (var a in OutlineShapes)
            {
                a.MoveBack(BoundingBox.TopLeft);
            }
            return t;
        }

        public void BuildBoundary()
        {
            CalcPathBounds();
            DisplayShapes.Insert(0, GetBoundingPolyLine());

        }

        internal Tuple<double, PolyLine> FindLargestPolygon()
        {
            if (OutlineShapes.Count == 0) return null;

            List<Tuple<double, PolyLine>> Polies = new List<Tuple<double, PolyLine>>();

            foreach (var a in OutlineShapes)
            {
                var area = Core.Helpers.PolygonSurfaceArea(a.Vertices);
                var bounds = new Bounds();
                bounds.FitPoint(a.Vertices);
                Polies.Add(new Tuple<double, PolyLine>(bounds.Area(), a));
            }

            return (from a in Polies orderby a.Item1 descending select a).First();
        }

        bool shapecache = false;
        public List<Triangle> ShapeCacheTriangles = new List<Triangle>();
        public bool Generated = false;

        public void BuildShapeCache()
        {
            if (shapecache) return;
            shapecache = true;

            foreach (var a in OutlineShapes)
            {
                if (a.Hole == false)
                {
                    var polygon = new Polygon();
                    List<Vertex> V = new List<Vertex>();
                    for (int i = 0; i < a.Count(); i++)
                    {
                        V.Add(new Vertex(a.Vertices[i].X, a.Vertices[i].Y));
                    }

                    polygon.AddContour(V);

                    var options = new ConstraintOptions() { ConformingDelaunay = false};
                    var quality = new QualityOptions() { };
                    var mesh = polygon.Triangulate(options, quality);

                    foreach (var t in mesh.Triangles)
                    {
                        var A = t.GetVertex(0);
                        var B = t.GetVertex(1);
                        var C = t.GetVertex(2);
                        ShapeCacheTriangles.Add(new Triangle()
                        {
                            A = new PointD(A.X, A.Y),
                            B = new PointD(B.X, B.Y),
                            C = new PointD(C.X, C.Y)
                        });
                    }
                }
            }
        }
    }
}
