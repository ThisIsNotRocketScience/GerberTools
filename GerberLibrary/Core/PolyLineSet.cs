using GerberLibrary.Core.Primitives;
using GerberLibrary.Core;
using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClipperLib;
using System.Drawing;
using System.Text.RegularExpressions;
using System.IO;

namespace GerberLibrary
{
    public class PathDefWithClosed
    {
        public bool Closed = false;
        public List<PointD> Vertices = new List<PointD>();
        public double Width;

        public override string ToString()
        {
            string r = "";
            foreach (var a in Vertices)
                r += a.ToString() + "  ";
            return string.Format("closed: {0} verts: ({1} {2}) ({3} {4})", Closed, Vertices[0], Vertices[1], Vertices[Vertices.Count()-2], Vertices[Vertices.Count() - 1] );
        }
    }
    public class GerberParserState
    {
        public Dictionary<string, GerberApertureMacro> ApertureMacros = new Dictionary<string, GerberApertureMacro>();
        public Dictionary<int, GerberApertureType> Apertures = new Dictionary<int, GerberApertureType>();
        public PolyLine Boundary;
        public bool ClearanceMode = false;
        public GerberNumberFormat CoordinateFormat = new GerberNumberFormat();
        public GerberApertureType CurrentAperture = new GerberApertureType() { };
        public int CurrentLineIndex = 0;
        public bool IgnoreZeroWidth = false;
        public int LastD;
        public int LastShapeID = 0;
        // blank default
        public double LastX = 0;

        public double LastY = 0;
        public BoardLayer Layer;
        public double MinimumApertureRadius = -1;
        public InterpolationMode MoveInterpolation = InterpolationMode.Linear;
        public List<PolyLine> NewShapes = new List<PolyLine>();
        public List<PolyLine> NewThinShapes = new List<PolyLine>();
        public bool PolygonMode = false;
        public List<PointD> PolygonPoints = new List<PointD>();
        public bool PreCombinePolygons = true;
        public bool Repeater;
        public int RepeatStartShapeIdx;
        public int RepeatStartThinShapeIdx;
        public int RepeatXCount;
        public double RepeatXOff;
        public int RepeatYCount;
        public double RepeatYOff;
        public string SanitizedFile = "";
        public BoardSide Side;
        public PolyLine ThinLine;
        internal bool GenerateGeometry = true;

        public double FlashRotation = 0;
        public double FlashScale = 1.0;
        public MirrorMode FlashMirror = MirrorMode.NoMirror;
        public List<PointD> ZerosizePoints = new List<PointD>();

        public enum MirrorMode
        {
            X,
            Y,
            XY, 
            NoMirror
        }
    }

    public partial class PolyLineSet
    {
        public string Name;



        static string laststatus = "";

        static int laststatuscount = 0;

        static int laststatusidx = 0;

        public PolyLineSet(string inname)
        {
            Name = inname;
        }

        /// <summary>
        /// Find optimal locations for breaktabs -> returns list of position + normal tuples
        /// </summary>
        /// <returns></returns>
        public static List<Tuple<PointD, PointD>> FindOptimalBreaktTabLocations(ParsedGerber Gerb)
        {
            List<Tuple<PointD, PointD>> Res = new List<Tuple<PointD, PointD>>();
            List<SegmentWithNormalAndDistance> Segs = new List<SegmentWithNormalAndDistance>();

            if (Gerb.OutlineShapes.Count == 0 || Gerb.OutlineShapes[0].Vertices.Count == 0) return Res;

            var First = Gerb.OutlineShapes[0].Vertices[0];
            PointD Max = new PointD(First.X, First.Y);
            PointD Min = new PointD(First.X, First.Y);

            foreach (var a in Gerb.OutlineShapes)
            {

                if (Clipper.Orientation(a.toPolygon()) == true)
                {
                    PointD Prev = a.Vertices[a.Vertices.Count - 1];
                    // generate normals and find extends
                    for (int V = 0; V < a.Vertices.Count; V++)
                    {
                        SegmentWithNormalAndDistance S = new SegmentWithNormalAndDistance();
                        S.A = Prev;
                        S.B = a.Vertices[V];

                        if (S.A != S.B)
                        {
                            S.CalculateNormal();

                            if (a.Vertices[V].X < Min.X) Min.X = a.Vertices[V].X;
                            if (a.Vertices[V].Y < Min.Y) Min.Y = a.Vertices[V].Y;
                            if (a.Vertices[V].X > Max.X) Max.X = a.Vertices[V].X;
                            if (a.Vertices[V].Y > Max.Y) Max.Y = a.Vertices[V].Y;

                            Segs.Add(S);
                        }
                        Prev = a.Vertices[V];
                    }
                }
            }
            PointD Center = (Min + Max) * 0.5;


            List<List<SegmentWithNormalAndDistance>> Buckets = new List<List<SegmentWithNormalAndDistance>>(); ;
            Buckets.Add(new List<SegmentWithNormalAndDistance>());
            Buckets.Add(new List<SegmentWithNormalAndDistance>());
            Buckets.Add(new List<SegmentWithNormalAndDistance>());
            Buckets.Add(new List<SegmentWithNormalAndDistance>());
            Buckets.Add(new List<SegmentWithNormalAndDistance>());
            Buckets.Add(new List<SegmentWithNormalAndDistance>());
            Buckets.Add(new List<SegmentWithNormalAndDistance>());
            Buckets.Add(new List<SegmentWithNormalAndDistance>());

            foreach (var a in Segs)
            {
                int Bucket = (int)Math.Floor((a.Angle * 8.0 / 360.0) + 0.5);
                while (Bucket < 0) Bucket += 8;
                while (Bucket >= 8) Bucket -= 8;
                double bucketangle = Bucket * (360 / 8.0);

                //                if (Bucket == 0)
                {
                    a.CalculateDistance(Center, bucketangle);
                    //       Console.WriteLine("{0}: {1} {2} {3}", a.Angle.ToString("N1"), a.Length.ToString("N1"), a.DistanceToCenter.ToString("N1"), a.ProjectedDistanceToCenter.ToString("N1"));
                    Buckets[Bucket].Add(a);
                }
            }

            List<SegmentWithNormalAndDistance> BucketItems = new List<SegmentWithNormalAndDistance>();

            for (int i = 0; i < 8; i++)
            {
                var L = Buckets[i];
                double bucketangle = i * 360.0 / 8.0;
                var sorted = from ii in L orderby (ii.ProjectedDistanceToCenter / ii.Length) descending select ii;
                if (sorted.Count() > 0)
                {
                    var f = sorted.First();
                    BucketItems.Add(f);
                    var t = new Tuple<PointD, PointD>(f.M, f.N);
                    //       Console.WriteLine("bucket {0}: {1} angle: {2}", i, f, bucketangle);
                }
                else
                {
                    BucketItems.Add(null);
                }
            }

            double evenscore = 0;
            double oddscore = 0;
            for (int i = 0; i < 8; i++)
            {
                double score = 0;
                if (BucketItems[i] != null)
                {
                    score += Math.Abs(BucketItems[i].Length * BucketItems[i].ProjectedDistanceToCenter);
                }
                Console.WriteLine("{0}: {1}", i, score);
                if (i % 2 == 0) evenscore += score; else oddscore += score;
            }

            int off = 0;
            if (oddscore > evenscore) off = 1;
            for (int i = off; i < 8; i += 2)
            {
                var f = BucketItems[i];


                int Items = (int)Math.Ceiling(f.Length / 20);
                PointD delta = (f.B - f.A) * (1.0 / Items);
                for (int j = 0; j < Items; j++)
                {
                    var t = new Tuple<PointD, PointD>(f.A + delta * 0.5 + delta * (double)j, f.N);
                    Res.Add(t);
                }

            }
            return Res;
        }

        public static ParsedGerber LoadExcellonDrillFileFromStream(StreamReader s, string origfilename, bool Precombine = false, double drillscaler = 1.0)
        {
            ParsedGerber Gerb = new ParsedGerber();
            Gerb.Name = origfilename;
            Gerb.Shapes.Clear();
            Gerb.DisplayShapes.Clear();
            GerberParserState State = new GerberParserState
            {
                Side = BoardSide.Both,
                PreCombinePolygons = Precombine,
                Layer = BoardLayer.Drill
            };

            ExcellonFile EF = new ExcellonFile();
            EF.Load(s, drillscaler);
            foreach (var T in EF.Tools)
            {
                var Tool = T.Value;

                int sides = Math.Max(10,(int)(Gerber.ArcQualityScaleFactor * Math.Max(2.0, Tool.Radius)));


                foreach (var Hole in Tool.Drills)
                {
                    PolyLine DispPL = new PolyLine(State.LastShapeID++);

                    for (int i = 0; i < sides; i++)
                    {
                        double PP = i * 6.283f / (double)sides;
                        DispPL.Add(Hole.X + (double)Math.Sin(PP) * (double)Tool.Radius, Hole.Y + (double)Math.Cos(PP) * (double)Tool.Radius);
                    }
                    DispPL.Close();
                    DispPL.MyColor = Color.DarkGreen;
                    Gerb.DisplayShapes.Add(DispPL);
                }

                foreach (var Slot in Tool.Slots)
                {
                    PolyLine DispPL = new PolyLine(State.LastShapeID++);

                    double dy = Slot.End.Y - Slot.Start.Y;
                    double dx = Slot.End.X - Slot.Start.X;

                    double offangl = -Math.Atan2(dy, dx) + 3.1415;

                    for (int i = 0; i < sides / 2; i++)
                    {
                        double PP = i * 6.283f / (double)sides;
                        PP += offangl;
                        DispPL.Add(Slot.Start.X + (double)Math.Sin(PP) * (double)Tool.Radius, Slot.Start.Y + (double)Math.Cos(PP) * (double)Tool.Radius);
                    }

                    for (int i = sides / 2; i < sides; i++)
                    {
                        double PP = i * 6.283f / (double)sides;
                        PP += offangl;
                        DispPL.Add(Slot.End.X + (double)Math.Sin(PP) * (double)Tool.Radius, Slot.End.Y + (double)Math.Cos(PP) * (double)Tool.Radius);
                    }

                    DispPL.Close();
                    DispPL.MyColor = Color.DarkGreen;
                    Gerb.DisplayShapes.Add(DispPL);
                }
            }
            Gerb.Side = State.Side;
            Gerb.Layer = State.Layer;
            Gerb.State = State;

            return Gerb;

        }

        public static ParsedGerber LoadExcellonDrillFile(string drillfile, bool Precombine = false, double drillscaler = 1.0)
        {
            ParsedGerber Gerb = new ParsedGerber();
            Gerb.Name = drillfile;
            Gerb.Shapes.Clear();
            Gerb.DisplayShapes.Clear();
            GerberParserState State = new GerberParserState
            {
                Side = BoardSide.Both,
                PreCombinePolygons = Precombine,
                Layer = BoardLayer.Drill
            };

            ExcellonFile EF = new ExcellonFile();
            EF.Load(drillfile, drillscaler);
            foreach (var T in EF.Tools)
            {
                var Tool = T.Value;

                int sides = (int)(Gerber.ArcQualityScaleFactor * Math.Max(2.0, Tool.Radius));


                foreach (var Hole in Tool.Drills)
                {
                    PolyLine DispPL = new PolyLine(State.LastShapeID++);

                    for (int i = 0; i < sides; i++)
                    {
                        double PP = i * 6.283f / (double)sides;
                        DispPL.Add(Hole.X + (double)Math.Sin(PP) * (double)Tool.Radius, Hole.Y + (double)Math.Cos(PP) * (double)Tool.Radius);
                    }
                    DispPL.Close();
                    DispPL.MyColor = Color.DarkGreen;
                    Gerb.DisplayShapes.Add(DispPL);
                }

                foreach (var Slot in Tool.Slots)
                {
                    PolyLine DispPL = new PolyLine(State.LastShapeID++);

                    double dy = Slot.End.Y - Slot.Start.Y;
                    double dx = Slot.End.X - Slot.Start.X;

                    double offangl = -Math.Atan2(dy, dx) + 3.1415;

                    for (int i = 0; i < sides / 2; i++)
                    {
                        double PP = i * 6.283f / (double)sides;
                        PP += offangl;
                        DispPL.Add(Slot.Start.X + (double)Math.Sin(PP) * (double)Tool.Radius, Slot.Start.Y + (double)Math.Cos(PP) * (double)Tool.Radius);
                    }

                    for (int i = sides / 2; i < sides; i++)
                    {
                        double PP = i * 6.283f / (double)sides;
                        PP += offangl;
                        DispPL.Add(Slot.End.X + (double)Math.Sin(PP) * (double)Tool.Radius, Slot.End.Y + (double)Math.Cos(PP) * (double)Tool.Radius);
                    }

                    DispPL.Close();
                    DispPL.MyColor = Color.DarkGreen;
                    Gerb.DisplayShapes.Add(DispPL);
                }
            }
            Gerb.Side = State.Side;
            Gerb.Layer = State.Layer;
            Gerb.State = State;

            return Gerb;

        }

        public static ParsedGerber LoadGerberFile(string gerberfile, bool forcezerowidth = false, bool writesanitized = false, GerberParserState State = null)
        {
            if (State == null) State = new GerberParserState();

            Gerber.DetermineBoardSideAndLayer(gerberfile, out State.Side, out State.Layer);

            using (StreamReader sr = new StreamReader(gerberfile))
            {
                return ProcessStream(gerberfile, forcezerowidth, writesanitized, State, sr);
            }
        }

        public static ParsedGerber LoadGerberFileFromStream(StreamReader sr, string originalfilename, bool forcezerowidth = false, bool writesanitized = false, GerberParserState State = null)
        {
            if (State == null) State = new GerberParserState();

            Gerber.DetermineBoardSideAndLayer(originalfilename, out State.Side, out State.Layer);
            return ProcessStream(originalfilename, forcezerowidth, writesanitized, State, sr);

        }

        public static ParsedGerber ParseGerber274x(List<String> inputlines, bool parseonly, bool forcezerowidth = false, GerberParserState State = null)
        {
            if (State == null) State = new GerberParserState();

            State.CurrentAperture.ShapeType = GerberApertureShape.Empty;
            State.Apertures.Clear();
            State.ApertureMacros.Clear();

            State.CoordinateFormat = new GerberNumberFormat();
            State.CoordinateFormat.SetImperialMode();

            List<String> lines = SanitizeInputLines(inputlines, State.SanitizedFile);

            ParsedGerber Gerb = new ParsedGerber
            {
                State = State
            };

            ParseGerber274_Lines(forcezerowidth, State, lines);

            if (parseonly) return Gerb;


            if (State.PreCombinePolygons)
            {
                Polygons solution = new Polygons();
                {


                    Polygons clips = new Polygons();
                    for (int i = 0; i < State.NewShapes.Count; i++)
                    {

                        clips.Add(State.NewShapes[i].toPolygon());
                    }
                    Clipper cp = new Clipper();
                    cp.AddPolygons(solution, PolyType.ptSubject);
                    cp.AddPolygons(clips, PolyType.ptClip);

                    cp.Execute(ClipType.ctUnion, solution, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

                }
                for (int i = 0; i < solution.Count; i++)
                {
                    PolyLine PL = new PolyLine(State.LastShapeID++);

                    PL.fromPolygon(solution[i]);
                    PL.MyColor = Color.FromArgb(255, 200, 128, 0);
                    Gerb.DisplayShapes.Add(PL);
                }
            }
            else
            {
                for (int i = 0; i < State.NewShapes.Count; i++)
                {

                    PolyLine PL = new PolyLine(State.LastShapeID++);
                    PL.ClearanceMode = State.NewShapes[i].ClearanceMode;
                    PL.fromPolygon(State.NewShapes[i].toPolygon());
                    Gerb.DisplayShapes.Add(PL);
                }
            }

            List<PathDefWithClosed> shapelist = new List<PathDefWithClosed>();
            for (int i = 0; i < State.NewThinShapes.Count; i++)
            {
                //     DisplayShapes.Add(NewThinShapes[i]);
                shapelist.Add(new PathDefWithClosed() { Vertices = State.NewThinShapes[i].Vertices, Width = State.NewThinShapes[i].Width });
            }

            var shapeslinked = Helpers.LineSegmentsToPolygons(shapelist);

            foreach (var a in shapeslinked)
            {
                PolyLine PL = new PolyLine(State.LastShapeID++);
                PL.Vertices = a.Vertices;
                PL.Thin = true;
                Gerb.DisplayShapes.Add(PL);
                Gerb.Shapes.Add(PL);
            }

            if (State.PreCombinePolygons)
            {
                //Console.WriteLine("Combining polygons - this may take some time..");
                Polygons solution2 = new Polygons();

                for (int i = 0; i < Gerb.DisplayShapes.Count; i++)
                {
                    Progress("Executing polygon merge", i, Gerb.DisplayShapes.Count);
                    Polygons clips = new Polygons();

                    clips.Add(Gerb.DisplayShapes[i].toPolygon());
                    Clipper cp = new Clipper();
                    cp.AddPolygons(solution2, PolyType.ptSubject);
                    cp.AddPolygons(clips, PolyType.ptClip);

                    cp.Execute(ClipType.ctUnion, solution2, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
                }

                //    DisplayShapes.Clear();
                for (int i = 0; i < solution2.Count; i++)
                {
                    Progress("Converting back to gerber", i, Gerb.DisplayShapes.Count);

                    PolyLine PL = new PolyLine(State.LastShapeID++);
                    PL.fromPolygon(solution2[i]);
                    PL.MyColor = Color.FromArgb(255, 200, 128, 0);
                    Gerb.OutlineShapes.Add(PL);
                }
            }
            else
            {

                for (int i = 0; i < Gerb.DisplayShapes.Count; i++)
                {
                    PolyLine PL = new PolyLine(State.LastShapeID++);
                    PL.fromPolygon(Gerb.DisplayShapes[i].toPolygon());
                    PL.ClearanceMode = Gerb.DisplayShapes[i].ClearanceMode;

                    Gerb.OutlineShapes.Add(Gerb.DisplayShapes[i]);

                }
            }
            Gerb.ZerosizePoints.AddRange(State.ZerosizePoints);
            Gerb.CalcPathBounds();
            Gerb.State = State;
            return Gerb;
        }

        public static List<string> SanitizeInputLines(List<String> inputlines, string SanitizedFile = "")
        {
            List<GerberBlock> Blocks = new List<GerberBlock>();
            bool HeaderActive = false;
            GerberBlock CurrentBlock = null;
            GerberBlock PreviousBlock = null;
            List<String> lines = new List<string>();
            string currentline = "";
            bool LastWasStar = false;

            CurrentBlock = new GerberBlock();
            CurrentBlock.Header = false;
            Blocks.Add(CurrentBlock);
            bool AddToPrevious = false;
            foreach (var ll in inputlines)
            {

                List<String> CadenceSplit = new List<string>();
                string T = ll.Trim();
                if (T.StartsWith("%") && T.EndsWith("%") && T.Contains("*") && (T.StartsWith("%AM") == false))
                {
                    T = T.Substring(1, T.Length - 2);
                    var s = T.Split('*');
                    if (s.Count() > 1)
                    {
                        foreach (var sl in s)
                        {
                            if (sl.Length > 0)
                            {
                                CadenceSplit.Add("%" + sl + "*%");
                            }

                        }
                        if (Gerber.ExtremelyVerbose)
                        {
                            if (CadenceSplit.Count > 1)
                            {
                                Console.WriteLine("Cadence format fixing:");
                                Console.WriteLine("original: {0}", ll);
                                Console.WriteLine("split:");
                                foreach (var sss in CadenceSplit)
                                {
                                    Console.WriteLine("     {0}", sss);
                                }
                            }
                        }
                    }
                    else
                    {
                        CadenceSplit.Add(ll);
                    }
                }
                else
                {
                    CadenceSplit.Add(ll);

                }

                foreach (var l in CadenceSplit)
                {

                    foreach (var c in l)
                    {
                        AddToPrevious = false;
                        GerberBlock Last = CurrentBlock;
                        if (c == '%')
                        {
                            if (HeaderActive == false)
                            {
                                HeaderActive = true;

                            }
                            else
                            {
                                if (CurrentBlock.Lines.Count == 0)
                                {
                                    CurrentBlock.Lines.Add("");
                                }
                                CurrentBlock.Lines[CurrentBlock.Lines.Count - 1] += '%';
                                AddToPrevious = true;
                                HeaderActive = false;
                            }
                            PreviousBlock = CurrentBlock;
                            CurrentBlock = null;
                        }
                        //     if (LastWasStar && c == '%')
                        //    {
                        //       Last.Lines[Last.Lines.Count -1] += '%';
                        //      lines[lines.Count - 1] += '%';
                        //  }
                        //  else
                        //  {

                        if (AddToPrevious == false)
                        {
                            if (CurrentBlock == null)
                            {
                                CurrentBlock = new GerberBlock();
                                CurrentBlock.Header = HeaderActive;
                                Blocks.Add(CurrentBlock);

                            }
                            currentline += c;
                            //  }
                            if (HeaderActive && c == '*')
                            {
                                LastWasStar = true;
                                CurrentBlock.Lines.Add(currentline);
                                lines.Add(currentline);
                                currentline = "";
                            }
                            else
                            {
                                if (c == '*')
                                {
                                    LastWasStar = true;
                                    CurrentBlock.Lines.Add(currentline);
                                    lines.Add(currentline);
                                    currentline = "";

                                }
                                else
                                {
                                    LastWasStar = false;

                                }
                            }
                        }

                    }
                    if (HeaderActive)
                    {

                    }
                    else
                    {
                        if (currentline.Length > 0)
                        {
                            if (CurrentBlock == null)
                            {
                                CurrentBlock = new GerberBlock();
                                CurrentBlock.Header = HeaderActive;
                                Blocks.Add(CurrentBlock);

                            }
                            CurrentBlock.Lines.Add(currentline);
                            lines.Add(currentline);
                            currentline = "";
                        }
                    }
                }
            }
            if (currentline.Length > 0)
            {
                CurrentBlock.Lines.Add(currentline);
                lines.Add(currentline);
            }

            List<String> reslines = new List<string>();
            //  foreach ( var a in lines)
            // {
            //     if (a.Length > 0) reslines.Add(a);

            // }

            foreach (var b in Blocks)
            {
                foreach (var a in b.Lines)
                {
                    string B = a.Trim().Replace(Gerber.LineEnding + Gerber.LineEnding, Gerber.LineEnding);
                    if (B.Trim().Length > 0)
                    {
                        if (b.Header)
                        {
                            //                        string FinalLine = a.Replace("%", "").Replace("*", "").Trim();

                            reslines.Add(a.Trim());
                        }
                        else
                        {
                            reslines.Add(a.Trim());
                        }
                    }
                }
            }
            DumpSanitizedFileToLog(SanitizedFile, Blocks, reslines);

            return reslines;
        }

        public static List<string> SanitizeInputLines_OLD(List<String> inputlines, string SanitizedFile = "")
        {
            List<GerberBlock> Blocks = new List<GerberBlock>();
            bool HeaderActive = false;
            GerberBlock CurrentBlock = null;

            List<String> lines = new List<string>();

            foreach (var l in inputlines)
            {
                string Line = l.Trim();
                while (Line.Length > 0)
                {
                    if (Line == "%")
                    {
                        lines.Add(Line);
                        if (HeaderActive)
                        {
                            HeaderActive = false;
                            CurrentBlock = null;
                        }
                        else
                        {
                            HeaderActive = true;
                        }
                        Line = "";
                    }
                    else
                        if (Line == "*")
                    {
                        Line = "";
                        // skip empty lines
                    }
                    else
                    {
                        if (Line.First() == '%' && Line.Last() == '%' && Line.Length > 2)
                        {
                            GerberBlock GB = new GerberBlock();
                            GB.Header = true;

                            string SL = Line.Substring(1, Line.Length - 2);
                            var SLSplit = SL.Split('*');
                            List<string> slres = new List<string>();
                            foreach (var part in SLSplit)
                            {
                                if (part.Trim().Length > 0)
                                {
                                    slres.Add(part.Trim() + "*");
                                    GB.Lines.Add(part.Trim());
                                }
                            }
                            if (slres.Count() > 0)
                            {
                                slres[0] = "%" + slres[0];
                                for (int i = 1; i < slres.Count; i++)
                                {
                                    if (char.IsDigit(slres[i][0]) == false)
                                    {
                                        slres[i] = "%" + slres[i];
                                        if (i < slres.Count - 1)
                                        {
                                            slres[i] = slres[i] + "%";

                                        }
                                    }
                                }
                                slres[slres.Count() - 1] += "%";
                                lines.AddRange(slres);
                            }
                            Blocks.Add(GB);
                            Line = "";

                        }
                        else
                        {
                            GerberBlock GB = new GerberBlock();
                            GB.Header = false;
                            int idx = Line.IndexOf('*');
                            int idx2 = Line.IndexOf("*%");
                            while (idx > -1)
                            {
                                int add = 1;
                                if (idx == idx2) add = 2;
                                if (add == 2 && Line.Length > 0 && Line.First() != '%') add = 1;
                                string SubLine = Line.Substring(0, idx + add);
                                lines.Add(SubLine.Trim());
                                GB.Lines.Add(SubLine.Trim());
                                Line = Line.Substring(idx + add).Trim();
                                if (Line.Length > 0 && Line.First() == '%' && Line.Last() == '%')
                                {
                                    Blocks.Add(GB);
                                    GB = new GerberBlock();
                                    GB.Header = true;

                                    string SL = Line;
                                    if (Line.Length > 2) SL = Line.Substring(1, Line.Length - 2);
                                    var SLSplit = SL.Split('*');
                                    List<string> slres = new List<string>();
                                    foreach (var part in SLSplit)
                                    {
                                        if (part.Trim().Length > 0)
                                        {
                                            slres.Add(part.Trim() + "*");
                                            GB.Lines.Add(part.Trim());
                                            lines.Add("%" + part.Trim() + "*%");

                                        }
                                    }
                                    Line = "";

                                    //    GB.Lines.Add(Line);
                                    idx = -1;
                                    idx2 = -2;
                                }
                                else
                                {
                                    idx = Line.IndexOf('*');
                                    idx2 = Line.IndexOf("*%");

                                }

                            }

                            if (Line.Length > 0)
                            {
                                lines.Add(Line);
                                GB.Lines.Add(Line);
                                Line = "";
                            }
                            Blocks.Add(GB);

                        }
                    }
                }
            }
            for (int i = 0; i < lines.Count(); i++)
            {
                lines[i] = lines[i].Replace(" ", "");
            }

            DumpSanitizedFileToLog(SanitizedFile, Blocks, lines);
            return lines;
        }

        public static void Normalize(ParsedGerber gerb)
        {
            gerb.Normalize();
        }

        public override string ToString()
        {
            return Name;
        }
        private static void AddExtrudedCurveSegment(ref double LastX, ref double LastY, List<PolyLine> NewShapes, GerberApertureType CurrentAperture, bool ClearanceMode, double X, double Y, int ShapeID, double rotation, double scale, GerberParserState.MirrorMode mirror)
        {
            PolyLine PL = new PolyLine(ShapeID);
            PL.ClearanceMode = ClearanceMode;

            // TODO: use CreatePolyLineSet and extrude that!
            var PolySet = CurrentAperture.CreatePolyLineSet(0, 0, ShapeID,rotation, scale, mirror);
            //       var Shapes = CurrentAperture.CreatePolyLineSet(0, 0);

            foreach (var currpoly in PolySet)
            {
                currpoly.ID = ShapeID;
                Polygons Combined = new Polygons();

                PolyLine A = new PolyLine( ShapeID);
                PolyLine B = new PolyLine(ShapeID);

                PointD start = new PointD(LastX, LastY);
                PointD end = new PointD(X, Y);
                PointD dir = end - start;

                dir.Normalize();
                //  dir.Rotate(180);
                dir = dir.Rotate(90);
                PointD LeftMost = new PointD();
                double maxdot = -10000000;
                double mindot = 10000000;
                PointD RightMost = new PointD();
                {
                    for (int i = 0; i < currpoly.Count(); i++)
                    {
                        PointD V = currpoly.Vertices[i];
                        //    PointD V2 = new PointD(V.X, V.Y);
                        // V2.Normalize();
                        double dot = V.Dot(dir);
                        //      Console.WriteLine("dot: {0}  {1}, {2}", dot.ToString("n2"), V, dir);
                        if (dot > maxdot)
                        {
                            RightMost = V; maxdot = dot;
                        }
                        if (dot < mindot)
                        {
                            LeftMost = V; mindot = dot;
                        }

                        A.Add(V.X + LastX, V.Y + LastY);
                        B.Add(V.X + X, V.Y + Y);
                    }

                    A.Close();
                    B.Close();

                    PolyLine C = new PolyLine(ShapeID); ;
                    C.Add(RightMost.X + LastX, RightMost.Y + LastY);
                    C.Add(LeftMost.X + LastX, LeftMost.Y + LastY);
                    C.Add(LeftMost.X + X, LeftMost.Y + Y);
                    C.Add(RightMost.X + X, RightMost.Y + Y);
                    C.Vertices.Reverse();
                    C.Close();

                    Clipper cp = new Clipper();
                    cp.AddPolygons(Combined, PolyType.ptSubject);
                    cp.AddPolygon(A.toPolygon(), PolyType.ptClip);
                    cp.Execute(ClipType.ctUnion, Combined, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

                    Clipper cp2 = new Clipper();
                    cp2.AddPolygons(Combined, PolyType.ptSubject);
                    cp2.AddPolygon(B.toPolygon(), PolyType.ptClip);
                    cp2.Execute(ClipType.ctUnion, Combined, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

                    Clipper cp3 = new Clipper();
                    cp3.AddPolygons(Combined, PolyType.ptSubject);
                    cp3.AddPolygon(C.toPolygon(), PolyType.ptClip);
                    cp3.Execute(ClipType.ctUnion, Combined, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

                    foreach (var a in Combined)
                    {
                        PolyLine PP = new PolyLine(ShapeID) ;
                        PP.fromPolygon(a);
                        PP.Close();
                        NewShapes.Add(PP);
                    }
                }
            }

            //    CurrentAperture.FindExtendIndices(new PointD(LastX, LastY), new PointD(X, Y), out one, out two);

            //    for (int i = two; i != one; i = (i + 1) % CurrentAperture.Shape.Count())
            //    {
            //        PL.Add(LastX + CurrentAperture.Shape.Vertices[i].X, LastY + CurrentAperture.Shape.Vertices[i].Y);
            //    }
            //    for (int i = one; i != two; i = (i + 1) % CurrentAperture.Shape.Count())
            //    {
            //        PL.Add(X + CurrentAperture.Shape.Vertices[i].X, Y + CurrentAperture.Shape.Vertices[i].Y);
            //    }

            //    PL.Close();


            ////    NewShapes.Add(PL);

            LastX = X;
            LastY = Y;
        }

        private static bool BasicLineCommands(string Line, GerberParserState State)
        {
            string FinalLine = Line.Replace("%", "").Replace("*", "").Trim();
            switch (FinalLine)
            {
                case "G90": State.CoordinateFormat.Relativemode = false; break;
                case "G91": State.CoordinateFormat.Relativemode = true; break;
                case "G71": State.CoordinateFormat.SetMetricMode(); break;
                case "G70": State.CoordinateFormat.SetImperialMode(); break;
                case "G74": State.CoordinateFormat.SetSingleQuadrantMode(); break;
                case "G75":
                    State.CoordinateFormat.SetMultiQuadrantMode();
                    break;
                case "G36":
                    State.PolygonMode = true;
                    State.PolygonPoints.Clear();
                    break;
                case "G37":
                    {
                        PolyLine PL = new PolyLine( State.LastShapeID++);
                        foreach (var a in State.PolygonPoints)
                        {
                            PL.Add(a.X, a.Y);
                        }
                        PL.Close();
                        State.NewShapes.Add(PL);
                        State.PolygonPoints.Clear();

                        State.PolygonMode = false;
                    }
                    break;
                case "LPC": State.ClearanceMode = true; break;
                case "LPD": State.ClearanceMode = false; break;
                case "MOIN": State.CoordinateFormat.SetImperialMode(); break;
                case "MOMM": State.CoordinateFormat.SetMetricMode(); break;
                case "G01":
                    State.MoveInterpolation = InterpolationMode.Linear;
                    break;
                case "G02":
                    State.MoveInterpolation = InterpolationMode.ClockWise;
                    break;
                case "G03":
                    State.MoveInterpolation = InterpolationMode.CounterClockwise;
                    break;

                default:
                    return false;
            }

            return true;
        }

        private static void DoRepeating(GerberParserState State)
        {
            int LastThin = State.NewThinShapes.Count();
            int LastShape = State.NewShapes.Count();
            for (int x = 0; x < State.RepeatXCount; x++)
            {
                for (int y = 0; y < State.RepeatYCount; y++)
                {
                    if (!(x == 0 && y == 0))
                    {
                        double xoff = State.RepeatXOff * x;
                        double yoff = State.RepeatYOff * y;
                        int LastShapeID = -1;
                        for (int i = State.RepeatStartThinShapeIdx; i < LastThin; i++)
                        {
                            var C = State.NewThinShapes[i];
                            if (LastShapeID != C.ID)
                            {
                                State.LastShapeID++;
                                LastShapeID = C.ID;
                            }
                            PolyLine P = new PolyLine(State.LastShapeID );
                            foreach (var a in C.Vertices)
                            {
                                P.Vertices.Add(new PointD(a.X + xoff, a.Y + yoff));
                            }
                            State.NewThinShapes.Add(P);
                        }
                        for (int i = State.RepeatStartShapeIdx; i < LastShape; i++)
                        {
                            var C = State.NewShapes[i];
                            if (LastShapeID != C.ID)
                            {
                                State.LastShapeID++;
                                LastShapeID = C.ID;
                            }
                            PolyLine P = new PolyLine(State.LastShapeID ); 
                            P.Width = C.Width;
                            foreach (var a in C.Vertices)
                            {
                                P.Vertices.Add(new PointD(a.X + xoff, a.Y + yoff));

                            }
                            State.NewShapes.Add(P);

                        }

                    }
                }
            }
            State.Repeater = false;
        }

        private static void DumpSanitizedFileToLog(string SanitizedFile, List<GerberBlock> Blocks, List<String> lines)
        {
            if (SanitizedFile.Length > 0)
            {
                Gerber.WriteAllLines(SanitizedFile, lines);
                List<string> l2 = new List<string>();
                foreach (var b in Blocks)
                {

                    foreach (var l in b.Lines)
                    {
                        if (b.Header == true)
                        {
                            l2.Add("HEAD " + l);
                        }
                        else
                        {
                            l2.Add("BODY " + l);
                        }
                    }

                }
                Gerber.WriteAllLines(SanitizedFile + ".txt", l2);
            }
        }

        private static void ParseGerber274_Lines(bool forcezerowidth, GerberParserState State, List<String> lines)
        {
            while (State.CurrentLineIndex < lines.Count)
            {
                GCodeCommand GCC = new GCodeCommand();
                GCC.Decode(lines[State.CurrentLineIndex], State.CoordinateFormat);
                string Line = lines[State.CurrentLineIndex];


                if (BasicLineCommands(Line, State) == false)
                {

                    if (GCC.charcommands.Count > 0 && GCC.errors == 0)
                    {
                        switch (GCC.charcommands[0])
                        {
                            case '%':
                                if (GCC.charcommands.Count > 1)
                                {
                                    switch (GCC.charcommands[1])
                                    {
                                        case 'L':
                                            {
                                                switch(GCC.charcommands[2])
                                                {
                                                    case 'R': // LR -> rotate; 
                                                        State.FlashRotation = GCC.numbercommands[0];
                                                        Console.WriteLine("rotation: {0}", GCC.numbercommands[0]);
                                                        break;
                                                    case 'S': // LS -> scale; 
                                                        State.FlashScale = GCC.numbercommands[0];
                                                        break;
                                                    case 'M': // LM -> mirror; 
                                                        switch(Line)
                                                        {
                                                            case "%LMX*%": State.FlashMirror = GerberParserState.MirrorMode.X; break;
                                                            case "%LMY*%": State.FlashMirror = GerberParserState.MirrorMode.Y; break;
                                                            case "%LMXY*%": State.FlashMirror = GerberParserState.MirrorMode.XY; break;
                                                            case "%LMN*%": State.FlashMirror = GerberParserState.MirrorMode.NoMirror; break;
                                                        }
                                                        break;
                                                }
                                            }
                                            break;
                                        case 'D':
                                            {
                                                Console.WriteLine(" D in %... ERROR but tolerated..");
                                                if (State.Apertures.TryGetValue((int)GCC.numbercommands[0], out State.CurrentAperture) == false)
                                                {
                                                    // Console.WriteLine("Failed to get aperture {0} ({1})", GCC.numbercommands[0], GCC.originalline);
                                                    State.CurrentAperture = new GerberApertureType();
                                                }
                                                else
                                                {
                                                    // Console.WriteLine("Switched to aperture {0}", GCC.numbercommands[0]);
                                                }


                                            }

                                            break;
                                        case 'S':
                                            if (GCC.charcommands[2] == 'R')
                                            {
                                                if (Gerber.ShowProgress) Console.WriteLine("Setting up step and repeat ");
                                                GerberSplitter GS2 = new GerberSplitter();
                                                GS2.Split(GCC.originalline, State.CoordinateFormat);
                                                if (GCC.numbercommands.Count == 0)
                                                {
                                                    DoRepeating(State);
                                                }
                                                else
                                                {


                                                    int Xcount = (int)GCC.numbercommands[0];
                                                    int Ycount = (int)GCC.numbercommands[1];
                                                    double Xoff = State.CoordinateFormat.ScaleFileToMM(GCC.numbercommands[2]);
                                                    double Yoff = State.CoordinateFormat.ScaleFileToMM(GCC.numbercommands[3]);
                                                    if (Xcount > 1 || Ycount > 1)
                                                    {
                                                        SetupRepeater(State, Xcount, Ycount, Xoff, Yoff);
                                                    }
                                                }
                                            }
                                            break;
                                        case 'F':
                                            State.CoordinateFormat.Parse(Line);

                                            break;
                                        case 'A':
                                            if (GCC.charcommands.Count > 2)
                                            {
                                                switch (GCC.charcommands[2])
                                                {
                                                    case 'D': // aperture definition
                                                        { 
                                                        {
                                                            // Console.WriteLine("Aperture definition: {0}", GCC.originalline);
                                                            GerberApertureType AT = new GerberApertureType();
                                                            AT.SourceLine = GCC.originalline;
                                                            if (GCC.numbercommands.Count < 1)
                                                            {
                                                                Console.WriteLine("Invalid aperture definition! No ID specified!");
                                                            }
                                                            else
                                                            {


                                                                int ATID = (int)GCC.numbercommands[0];
                                                                if (Gerber.ShowProgress) Console.Write("Aperture definition {0}:", ATID);
                                                                bool ismacro = false;

                                                                foreach (var a in State.ApertureMacros)
                                                                {
                                                                    if (GCC.originalline.Substring(6).Contains(a.Value.Name))
                                                                    {
                                                                        ismacro = true;
                                                                        if (Gerber.ShowProgress) Console.WriteLine(" macro");

                                                                    }
                                                                }
                                                                if (ismacro == false)
                                                                {
                                                                    switch (GCC.charcommands[4])
                                                                    {
                                                                        case 'C': // circle aperture

                                                                            if (Gerber.ShowProgress) Console.WriteLine(" circle: {0} (in mm: {1})", GCC.numbercommands[1], State.CoordinateFormat.ScaleFileToMM(GCC.numbercommands[1])); // command is diameter

                                                                            double radius = (State.CoordinateFormat.ScaleFileToMM(GCC.numbercommands[1]) / 2);
                                                                            if (radius < State.MinimumApertureRadius)
                                                                            {
                                                                                radius = State.MinimumApertureRadius;
                                                                                if (Gerber.ShowProgress) Console.WriteLine(" -- grew aperture radius to minimum radius: {0}", State.MinimumApertureRadius);
                                                                            }

                                                                            AT.SetCircle(radius); // hole ignored for now!
                                                                            // TODO: Add Hole Support
                                                                            if (AT.CircleRadius == 0)
                                                                            {
                                                                                AT.ZeroWidth = true;
                                                                                if (Gerber.ShowProgress) Console.WriteLine(" -- Zero width aperture found!");
                                                                            }
                                                                            break;
                                                                        case 'P': // polygon aperture
                                                                            {
                                                                                //   Console.WriteLine("      ngon: {0}", GCC.numbercommands[1]);
                                                                                if (Gerber.ShowProgress) Console.WriteLine("\tpolygon");
                                                                                AT.NGonDiameter = State.CoordinateFormat.ScaleFileToMM(GCC.numbercommands[1]);
                                                                                AT.NGonXoff = 0;
                                                                                AT.NGonYoff = 0;
                                                                                double Rotation = 0;
                                                                                if (GCC.numbercommands.Count > 3) Rotation = GCC.numbercommands[3];
                                                                                AT.NGon((int)GCC.numbercommands[2], State.CoordinateFormat.ScaleFileToMM(GCC.numbercommands[1]) / 2, 0, 0, Rotation); // hole ignored for now!
                                                                                // TODO: Add Hole Support
                                                                            }
                                                                            break;
                                                                        case 'R': // rectangle aperture
                                                                            {
                                                                                if (Gerber.ShowProgress) Console.WriteLine(" rectangle");
                                                                                double W = Math.Abs(State.CoordinateFormat.ScaleFileToMM(GCC.numbercommands[1]));
                                                                                double H = Math.Abs(State.CoordinateFormat.ScaleFileToMM(GCC.numbercommands[2]));
                                                                                //  Console.WriteLine("      rectangle: {0},{1} (in mm: {2},{3})",GCC.numbercommands[1],GCC.numbercommands[2], W,H);
                                                                                AT.SetRectangle(W, H,0); // hole ignored for now!
                                                                                // TODO: Add Hole Support
                                                                            }


                                                                            break;
                                                                        case 'O': // obround aperture
                                                                            {
                                                                                if (Gerber.ShowProgress) Console.WriteLine(" obround");

                                                                                double W = State.CoordinateFormat.ScaleFileToMM(GCC.numbercommands[1]);
                                                                                double H = State.CoordinateFormat.ScaleFileToMM(GCC.numbercommands[2]);
                                                                                AT.SetObround(W, H);
                                                                                if (GCC.numbercommands.Count() > 3)
                                                                                {
                                                                                    // TODO: Add Hole Support
                                                                                }
                                                                            }
                                                                            break;
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    {
                                                                        int idx = 4;
                                                                        while (char.IsDigit(lines[State.CurrentLineIndex][idx]) == true) idx++;
                                                                        string mname = lines[State.CurrentLineIndex].Substring(idx).Split('*')[0];
                                                                        string[] macrostrings = mname.Split(',');

                                                                        List<string> macroparamstrings = new List<string>();
                                                                        for (int i = 1; i < macrostrings.Count(); i++)
                                                                        {
                                                                            string a = macrostrings[i];
                                                                            string[] macrosubstrings = a.Split('X');
                                                                            foreach (var aa in macrosubstrings)
                                                                            {
                                                                                macroparamstrings.Add(aa.Trim());
                                                                            }

                                                                        }
                                                                        //   Console.WriteLine("macro aperture type: {0}", macrostrings[0]);
                                                                        double[] paramlist = new double[macroparamstrings.Count];
                                                                        for (int i = 0; i < macroparamstrings.Count; i++)
                                                                        {

                                                                            if (Gerber.TryParseDouble(macroparamstrings[i], out double R))
                                                                            {
                                                                                paramlist[i] = R;
                                                                            }
                                                                            else
                                                                            {
                                                                                paramlist[i] = 1;
                                                                            }
                                                                        }
                                                                        AT = State.ApertureMacros[macrostrings[0]].BuildAperture(paramlist.ToList(), State.CoordinateFormat);
                                                                        //  GCC.Print();
                                                                    }
                                                                }

                                                                AT.ID = ATID;
                                                                if (Gerber.ShowProgress)
                                                                {
                                                                    if (State.Apertures.ContainsKey(ATID))
                                                                    {
                                                                        Console.WriteLine("redefining aperture {0}", ATID);
                                                                    }
                                                                }
                                                                State.Apertures[ATID] = AT;

                                                            }
                                                        }
                                                        break;
                                                       }
                                                    case 'M': // aperture macro
                                                        { 
                                                        string name = GCC.originalline.Substring(3).Split('*')[0];
                                                        if (Gerber.ShowProgress) Console.WriteLine("Aperture macro: {0} ({1})", name, GCC.originalline);
                                                        GerberApertureMacro AM = new GerberApertureMacro();
                                                        AM.Name = name;

                                                        State.CurrentLineIndex++;
                                                        int macroline = State.CurrentLineIndex;
                                                        string macrostring = "";
                                                        List<string> MacroLines = new List<string>();
                                                        while (lines[macroline].Contains('%') == false)
                                                        {
                                                            MacroLines.Add(lines[macroline]);
                                                            macrostring += lines[macroline];
                                                            //     Console.WriteLine("macro def: {0}", lines[macroline]);
                                                            macroline++;
                                                        }

                                                        macrostring += lines[macroline];

                                                        //macroline++;
                                                        var macroparts = macrostring.Split('*');
                                                        List<string> trimmedparts = new List<string>();
                                                        foreach (var p in macroparts)
                                                        {
                                                            string pt = p.Trim();
                                                            if (pt.Length > 0)
                                                            {
                                                                if (pt[0] != '%')
                                                                {
                                                                    trimmedparts.Add(pt);
                                                                    var spl = pt.Split(',');

                                                                    // if (Gerber.Verbose) Console.WriteLine("macro part: {0}", spl[0]);
                                                                }
                                                            }
                                                        }
                                                        State.CurrentLineIndex = macroline;


                                                        //while (lines[currentline][lines[currentline].Length - 1] != '%')
                                                        foreach (var a in trimmedparts)
                                                        {
                                                            if (a[0] == '0')
                                                            {
                                                                // comment line
                                                            }
                                                            else
                                                            {


                                                                GCodeCommand GCC2 = new GCodeCommand();
                                                                GCC2.Decode(a, State.CoordinateFormat);
                                                                if (GCC2.numbercommands.Count() > 0)
                                                                    switch ((int)GCC2.numbercommands[0])
                                                                    {
                                                                        case 4: // outline
                                                                            {
                                                                                if (Gerber.ShowProgress) Console.WriteLine("\tMacro part: outline");
                                                                                GerberApertureMacroPart AMP = new GerberApertureMacroPart();
                                                                                AMP.Type = GerberApertureMacroPart.ApertureMacroTypes.Outline;
                                                                                AMP.DecodeOutline(a, State.CoordinateFormat);
                                                                                AM.Parts.Add(AMP);

                                                                            }
                                                                            break;
                                                                        case 5: // polygon
                                                                            {
                                                                                if (Gerber.ShowProgress) Console.WriteLine("\tMacro part: polygon");
                                                                                GerberApertureMacroPart AMP = new GerberApertureMacroPart();

                                                                                AMP.Decode(a, State.CoordinateFormat);
                                                                                AMP.Type = GerberApertureMacroPart.ApertureMacroTypes.Polygon;
                                                                                AM.Parts.Add(AMP);
                                                                                //ApertureMacros[name] = AM;
                                                                            }
                                                                            break;
                                                                        case 6: // MOIRE
                                                                            {
                                                                                if (Gerber.ShowProgress) Console.WriteLine("\tMacro part: moiré");
                                                                                GerberApertureMacroPart AMP = new GerberApertureMacroPart();

                                                                                AMP.Decode(a, State.CoordinateFormat);
                                                                                AMP.Type = GerberApertureMacroPart.ApertureMacroTypes.Moire;
                                                                                AM.Parts.Add(AMP);
                                                                                //ApertureMacros[name] = AM;
                                                                            }
                                                                            break;
                                                                        case 7: // THERMAL
                                                                            {
                                                                                if (Gerber.ShowProgress) Console.WriteLine("\tMacro part: thermal");
                                                                                GerberApertureMacroPart AMP = new GerberApertureMacroPart();

                                                                                AMP.Decode(a, State.CoordinateFormat);
                                                                                AMP.Type = GerberApertureMacroPart.ApertureMacroTypes.Thermal;
                                                                                AM.Parts.Add(AMP);
                                                                                //ApertureMacros[name] = AM;
                                                                            }
                                                                            break;
                                                                        case 1:
                                                                            {
                                                                                if (Gerber.ShowProgress) Console.WriteLine("\tMacro part: circle");

                                                                                GerberApertureMacroPart AMP = new GerberApertureMacroPart();

                                                                                AMP.DecodeCircle(a, State.CoordinateFormat);
                                                                                AMP.Type = GerberApertureMacroPart.ApertureMacroTypes.Circle;
                                                                                AM.Parts.Add(AMP);

                                                                            }
                                                                            break;

                                                                        case 20: // line
                                                                            {
                                                                                if (Gerber.ShowProgress) Console.WriteLine("\tMacro part: line");

                                                                                GerberApertureMacroPart AMP = new GerberApertureMacroPart();

                                                                                AMP.DecodeLine(a, State.CoordinateFormat);
                                                                                AMP.Type = GerberApertureMacroPart.ApertureMacroTypes.Line_2;
                                                                                AM.Parts.Add(AMP);

                                                                            }
                                                                            break;
                                                                        case 2: // line
                                                                            {
                                                                                if (Gerber.ShowProgress) Console.WriteLine("\tMacro part: line");

                                                                                GerberApertureMacroPart AMP = new GerberApertureMacroPart();

                                                                                AMP.DecodeLine(a, State.CoordinateFormat);
                                                                                AMP.Type = GerberApertureMacroPart.ApertureMacroTypes.Line;
                                                                                AM.Parts.Add(AMP);

                                                                            }
                                                                            break;
                                                                        case 21: // centerline
                                                                            {
                                                                                if (Gerber.ShowProgress) Console.WriteLine("\tMacro part: center line");

                                                                                GerberApertureMacroPart AMP = new GerberApertureMacroPart();

                                                                                AMP.DecodeCenterLine(a, State.CoordinateFormat);
                                                                                AMP.Type = GerberApertureMacroPart.ApertureMacroTypes.CenterLine;
                                                                                AM.Parts.Add(AMP);

                                                                            }
                                                                            break;

                                                                        case 22: // lowerlef3line
                                                                            {
                                                                                if (Gerber.ShowProgress) Console.WriteLine("\tMacro part: lower left line");

                                                                                GerberApertureMacroPart AMP = new GerberApertureMacroPart();

                                                                                AMP.DecodeLowerLeftLine(a, State.CoordinateFormat);
                                                                                AMP.Type = GerberApertureMacroPart.ApertureMacroTypes.LowerLeftLine;
                                                                                AM.Parts.Add(AMP);

                                                                            }
                                                                            break;
                                                                        default:
                                                                            {
                                                                                Regex R = new Regex(@"(?<normal>\s*(?<dec>\$\d+)\s*\=\s*(?<rightside>.*))");
                                                                                var M = R.Match(GCC2.originalline);

                                                                                if (M.Length > 0)
                                                                                {
                                                                                    Console.WriteLine("Found equation! {0}", GCC2.originalline);
                                                                                    GerberApertureMacroPart AMP = new GerberApertureMacroPart();
                                                                                    AMP.Type = GerberApertureMacroPart.ApertureMacroTypes.Equation;
                                                                                    AMP.EquationTarget = M.Groups["dec"].Value;
                                                                                    AMP.EquationSource = M.Groups["rightside"].Value;
                                                                                    AM.Parts.Add(AMP);
                                                                                }
                                                                                else

                                                                                    if (GCC2.numbercommands[0] == 0)
                                                                                {
                                                                                    Console.WriteLine("Macro comment? {0}", GCC2.originalline);
                                                                                }
                                                                                else
                                                                                {
                                                                                    Console.WriteLine("Unhandled macro part type: {0} in macro {1}: {2}", GCC2.originalline, AM.Name, GCC2.numbercommands[0]);
                                                                                    Console.WriteLine("\t{0}", a);
                                                                                }

                                                                            }
                                                                            break;
                                                                    }
                                                                else
                                                                {
                                                                    Regex R = new Regex(@"(?<normal>\s*(?<dec>\$\d+)\s*\=\s*(?<rightside>.*))");
                                                                    var M = R.Match(GCC2.originalline);

                                                                    if (M.Length > 0)
                                                                    {
                                                                        Console.WriteLine("Found equation! {0}", GCC2.originalline);
                                                                        GerberApertureMacroPart AMP = new GerberApertureMacroPart();
                                                                        AMP.Type = GerberApertureMacroPart.ApertureMacroTypes.Equation;
                                                                        AMP.EquationTarget = M.Groups["dec"].Value;
                                                                        AMP.EquationSource = M.Groups["rightside"].Value;
                                                                        AM.Parts.Add(AMP);
                                                                    }
                                                                }
                                                            }
                                                            //         currentline++;
                                                        }

                                                        State.ApertureMacros[name] = AM;

                                                        break;
                                                }

                                                    case 'B': // block aperture... here be dragons
                                                        {
                                                            if (GCC.numbercommands.Count < 1)
                                                            {
                                                                Console.WriteLine("Invalid aperture definition! No ID specified!");
                                                            }
                                                            else
                                                            {

                                                                int ATID = (int)GCC.numbercommands[0];
                                                                if (Gerber.ShowProgress) Console.Write("Block Aperture definition {0}:", ATID);
                                                                if (Gerber.ShowProgress) Console.Write("from {0}", State.CurrentLineIndex);
                                                                bool endfound = false;
                                                                int stack = 1;
                                                                State.CurrentLineIndex++;

                                                                List<String> SubGerberLines = new List<string>();

                                                                while (!endfound)
                                                                {
                                                                    SubGerberLines.Add(lines[State.CurrentLineIndex]);
                                                                    if (lines[State.CurrentLineIndex] != "%AB*%")
                                                                    {
                                                                        if (lines[State.CurrentLineIndex].StartsWith("%AB"))
                                                                        {
                                                                            stack++;
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        stack--;
                                                                        if (stack == 0) endfound = true;
                                                                    }
                                                                    State.CurrentLineIndex++;
                                                                }
                                                                if (Gerber.ShowProgress) Console.Write("to {0}", State.CurrentLineIndex);

                                                                GerberApertureType AT = new GerberApertureType();
                                                                AT.ShapeType = GerberApertureShape.GerberBlock;
                                                                AT.GerberLines = SubGerberLines;
                                                                AT.ID = ATID;
                                                                State.Apertures[ATID] = AT;
                                                            }
                                                        }
                                                        break;
                                                }
                                            }

                                            break;
                                    };
                                }

                                break;

                            default:
                                GerberSplitter GS = new GerberSplitter();
                                GS.Split(GCC.originalline, State.CoordinateFormat);
                                if (GS.Has("D") && GS.Get("D") >= 10)
                                {
                                    if (State.Apertures.TryGetValue((int)GS.Get("D"), out State.CurrentAperture) == false)
                                    {
                                        //Console.WriteLine("Failed to get aperture {0} ({1})", GCC.numbercommands[0], GCC.originalline);
                                        State.CurrentAperture = new GerberApertureType();
                                        State.CurrentAperture.ShapeType = GerberApertureShape.Empty;
                                    }
                                    else
                                    {
                                        // Console.WriteLine("Switched to aperture {0}", GCC.numbercommands[0]);
                                    }

                                }
                                else
                                {
                                    if (GS.Has("G") && GS.Get("G") < 4)
                                    {
                                        switch ((int)GS.Get("G"))
                                        {
                                            case 2:
                                                State.MoveInterpolation = InterpolationMode.ClockWise;
                                                break;
                                            case 3:
                                                State.MoveInterpolation = InterpolationMode.CounterClockwise;
                                                break;
                                            case 1:
                                                State.MoveInterpolation = InterpolationMode.Linear;
                                                break;
                                        }

                                    }

                                    if (GS.Has("X") || GS.Has("Y") || GS.Has("D"))
                                    {
                                        double X = State.LastX;
                                        double Y = State.LastY;
                                        double I = 0;
                                        double J = 0;
                                        if (State.CoordinateFormat.Relativemode)
                                        {
                                            X = 0;
                                            Y = 0;
                                        }
                                        if (State.MoveInterpolation != InterpolationMode.Linear)
                                        {
                                            if (GS.Has("I")) I = State.CoordinateFormat.ScaleFileToMM(GS.Get("I"));
                                            if (GS.Has("J")) J = State.CoordinateFormat.ScaleFileToMM(GS.Get("J"));
                                        }

                                        if (GS.Has("X"))
                                        {
                                            X = State.CoordinateFormat.ScaleFileToMM(GS.Get("X"));
                                        }

                                        if (GS.Has("Y"))
                                        {
                                            Y = State.CoordinateFormat.ScaleFileToMM(GS.Get("Y"));
                                        }

                                        if (State.CoordinateFormat.Relativemode)
                                        {
                                            X += State.LastX;
                                            Y += State.LastY;
                                        }
                                        int ActualD = State.LastD;
                                        if (GS.Has("D")) ActualD = (int)GS.Get("D");
                                        State.LastD = ActualD;
                                        if (Gerber.ExtremelyVerbose)
                                        {
                                            //      Console.WriteLine("{0} to {1} - {2} ({3} - {4})", State.MoveInterpolation, X, Y, I, J);
                                        }
                                        if (State.PolygonMode)
                                        {

                                            switch (ActualD)
                                            {
                                                case 1:
                                                    switch (State.MoveInterpolation)
                                                    {
                                                        case InterpolationMode.Linear:
                                                            State.PolygonPoints.Add(new PointD(X, Y));
                                                            break;
                                                        default:
                                                            //todo!
                                                            List<PointD> CurvePoints = Gerber.CreateCurvePoints(State.LastX, State.LastY, X, Y, I, J, State.MoveInterpolation, State.CoordinateFormat.CurrentQuadrantMode);
                                                            foreach (var D in CurvePoints)
                                                            {
                                                                State.PolygonPoints.Add(new PointD(D.X, D.Y));

                                                            }
                                                            break;
                                                    }

                                                    //Move
                                                    break;
                                                case 2:
                                                    if (State.PolygonPoints.Count > 0)
                                                    {
                                                        PolyLine PL = new PolyLine( State.LastShapeID++);
                                                        PL.ClearanceMode = State.ClearanceMode;
                                                        foreach (var a in State.PolygonPoints)
                                                        {
                                                            PL.Add(a.X, a.Y);
                                                        }
                                                        PL.Close();
                                                        State.NewShapes.Add(PL);
                                                        State.PolygonPoints.Clear();
                                                    }
                                                    State.PolygonPoints.Add(new PointD(X, Y));

                                                    //         PolygonMode = false;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            if (State.CurrentAperture.ZeroWidth && State.IgnoreZeroWidth || State.CurrentAperture.ShapeType == GerberApertureShape.Empty)
                                            {
                                                if (Gerber.ShowProgress)
                                                {
                                                    Console.WriteLine("ignoring moves with zero width or empty aperture");
                                                    State.ZerosizePoints.Add(new PointD(X, Y));
                                                    // adding to bounding box anyway!

                                                }
                                            }
                                            else
                                                switch (ActualD)
                                                {
                                                    case 1: // D01
                                                        // TODO: EXTRUDE A BOOLEAN UNION OF THE COMPOUND SHAPE   

                                                        if (State.CurrentAperture != null)
                                                        {
                                                            if (Gerber.ShowProgress && State.CurrentAperture.ShapeType == GerberApertureShape.Compound)
                                                            {
                                                                Console.WriteLine("Warning: compound aperture used for interpolated move! undefined behaviour for outline generation");

                                                            }
                                                            if (State.CurrentAperture.Shape.Count() > 0 && forcezerowidth == false)
                                                            {

                                                                switch (State.MoveInterpolation)
                                                                {
                                                                    case InterpolationMode.Linear:
                                                                        if (State.GenerateGeometry) AddExtrudedCurveSegment(ref State.LastX, ref State.LastY, State.NewShapes, State.CurrentAperture, State.ClearanceMode, X, Y,  State.LastShapeID, State.FlashRotation, State.FlashScale, State.FlashMirror);
                                                                        State.LastShapeID++;
                                                                        break;
                                                                    default:

                                                                        List<PointD> CurvePoints = Gerber.CreateCurvePoints(State.LastX, State.LastY, X, Y, I, J, State.MoveInterpolation, State.CoordinateFormat.CurrentQuadrantMode);
                                                                        foreach (var D in CurvePoints)
                                                                        {
                                                                            //   AddExtrudedCurveSegment(ref LastX, ref LastY, NewShapes, CurrentAperture, ClearanceMode, LastX + I, LastY + J);
                                                                            if (State.GenerateGeometry) AddExtrudedCurveSegment(ref State.LastX, ref State.LastY, State.NewShapes, State.CurrentAperture, State.ClearanceMode, D.X, D.Y, State.LastShapeID, State.FlashRotation, State.FlashScale, State.FlashMirror);
                                                                        }
                                                                        State.LastShapeID++;
                                                                        break;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (State.ThinLine == null)
                                                                {
                                                                    State.ThinLine = new PolyLine(State.LastShapeID ++);
                                                                    State.ThinLine.ClearanceMode = State.ClearanceMode;
                                                                    State.ThinLine.Width = State.CurrentAperture.CircleRadius;
                                                                    //Console.WriteLine("Start: {0:N2} , {1:N2} - {2}", State.LastX, State.LastY, Line);
                                                                    State.ThinLine.Add(State.LastX, State.LastY);
                                                                }
                                                                switch (State.MoveInterpolation)
                                                                {
                                                                    case InterpolationMode.Linear:

                                                                        State.ThinLine.Add(X, Y);
                                                                   //     Console.WriteLine("{0:N2} , {1:N2} - {2}", X,Y, Line);
                                                                        break;

                                                                    default:

                                                                        List<PointD> CurvePoints = Gerber.CreateCurvePoints(State.LastX, State.LastY, X, Y, I, J, State.MoveInterpolation, State.CoordinateFormat.CurrentQuadrantMode);
                                                                        foreach (var D in CurvePoints)
                                                                        {
                                                                            State.ThinLine.Add(D.X, D.Y);

                                                                        }
                                                                        break;
                                                                }         // PL.Close();

                                                            }

                                                        }
                                                        break;
                                                    //    CurrentPL.Add(X, Y); break; // move while drawing exposure
                                                    case 2: // D02 
                                                        {
                                                            // move only. 
                                                            if (State.ThinLine != null)
                                                            {
                                                                State.NewThinShapes.Add(State.ThinLine);
                                                                State.ThinLine = null;
                                                            }
                                                        }
                                                        break;
                                                    case 3: // stamp 1 aperture D03
                                                        {
                                                            if (State.CurrentAperture != null)
                                                            {
                                                                List<PolyLine> PL = State.CurrentAperture.CreatePolyLineSet(X, Y, State.LastShapeID++, State.FlashRotation, State.FlashScale, State.FlashMirror);
                                                                foreach (var p in PL)
                                                                {
                                                                    p.ClearanceMode = State.ClearanceMode;
                                                                    State.NewShapes.Add(p);

                                                                }

                                                            }

                                                        }
                                                        break;
                                                }
                                        }
                                        State.LastX = X;
                                        State.LastY = Y;
                                        break;
                                    }

                                    else
                                    {
                                        if (GCC.originalline.Contains("G04"))
                                        {
                                            // probably comment..
                                        }
                                        else
                                        {
                                            if (Gerber.ShowProgress) Console.WriteLine("...? {0}", GCC.originalline);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
                State.CurrentLineIndex++;
            }

            SetupRepeater(State, 1, 1, 0, 0);
            if (State.ThinLine != null)
            {
                State.NewThinShapes.Add(State.ThinLine);
                State.ThinLine = null;
            }
        }

        private static ParsedGerber ProcessStream(string gerberfile, bool forcezerowidth, bool writesanitized, GerberParserState State, StreamReader sr)
        {
            List<String> lines = new List<string>();
            while (sr.EndOfStream == false)
            {
                String line = sr.ReadLine();
                if (line.Length > 0)
                {
                    lines.Add(line);
                }
            }

            if (writesanitized)
            {
                State.SanitizedFile = gerberfile + ".sanitized.gerber";
            };

            var G = ParseGerber274x(lines, false, forcezerowidth, State); ;
            G.Name = gerberfile;
            return G;
        }

        private static void Progress(string name, int idx, int count)
        {
            if (laststatus != name || idx != laststatusidx || laststatuscount != count)
            {
                laststatus = name;
                laststatusidx = idx;
                laststatuscount = count;
                if (Gerber.ShowProgress) Console.WriteLine("{0}: {1}/{2}: {3}%", name, idx + 1, count, Math.Round(1000 * (double)(idx + 1) / (Math.Max(count, 1.0))) / 10.0);

            }
        }

        private static void SetupRepeater(GerberParserState State, int Xcount, int Ycount, double Xoff, double Yoff)
        {
            if (State.Repeater == true) DoRepeating(State);

            State.Repeater = (Xcount * Ycount > 1) ?  true: false;
            State.RepeatXCount = Xcount;
            State.RepeatYCount = Ycount;
            State.RepeatXOff = Xoff;
            State.RepeatYOff = Yoff;

            State.RepeatStartThinShapeIdx = State.NewThinShapes.Count();
            State.RepeatStartShapeIdx = State.NewShapes.Count();
        }
        
        public class GerberBlock
        {
            public bool Header;
            public List<string> Lines = new List<string>();
        }

        public class SegmentWithNormalAndDistance
        {
            public PointD A = new PointD();
            public double Angle;
            public PointD B = new PointD();
            public double DistanceToCenter;
            public double Length;
            public PointD M = new PointD(); public PointD N = new PointD();
            public double ProjectedDistanceToCenter;

            public void CalculateDistance(PointD center, double angle)
            {

                PointD N = new PointD(0, 1);
                N = N.Rotate(angle - 90);

                PointD dir = PointD.GetClosestPointOnLineSegment(A, B, center) - center;
                //  dir.Normalize();
                ProjectedDistanceToCenter = N.Dot(dir);

                DistanceToCenter = PointD.LineToPointDistance2D(A, B, center, true);
            }

            public void CalculateNormal()
            {

                N = (B - A).Rotate(90);
                M = (A + B) * 0.5;
                N.Normalize();
                Length = PointD.Distance(A, B);
                Angle = Math.Atan2(N.Y, N.X) * 360.0 / (Math.PI * 2);
            }
            public override string ToString()
            {
                return string.Format("Ang {0} Len {1} Dst {2} Cen {3}", Angle.ToString("N3"), Length.ToString("N3"), DistanceToCenter.ToString("N3"), M);
                //return base.ToString();
            }
        }
    };

}
