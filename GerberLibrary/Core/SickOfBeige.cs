using GerberLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GerberLibrary.Core;
using GerberLibrary.Core.Primitives;
using Ionic.Zip;
using ClipperLib;
using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace GerberLibrary.Core
{
    public class GerberSet
    {
        public Bounds BoundingBox = new Bounds();
        public List<String> Errors = new List<string>();

        public bool HasLoadedOutline = false;
        public List<ParsedGerber> PLSs = new List<ParsedGerber>();
        public List<ExcellonFile> Excellons = new List<ExcellonFile>();
        public int Count()
        {
            return PLSs.Count;
        }
        public Dictionary<string, MemoryStream> Streams = new Dictionary<string, MemoryStream>();

        public void AddBoardsToSet(List<string> FileList, bool fixgroup = true, ProgressLog Logger = null)
        {
            foreach (var a in FileList)
            {
                BoardSide aSide = BoardSide.Unknown;
                BoardLayer aLayer = BoardLayer.Unknown;
                string ext = Path.GetExtension(a);
                if (ext == ".zip")
                {
                    using (ZipFile zip1 = ZipFile.Read(a))
                    {
                        foreach (ZipEntry e in zip1)
                        {
                            MemoryStream MS = new MemoryStream();
                            if (e.IsDirectory == false)
                            {
                                //                              e.Extract(MS);
                                //                                MS.Seek(0, SeekOrigin.Begin);
                                Gerber.DetermineBoardSideAndLayer(e.FileName, out aSide, out aLayer);
                                if (aLayer == BoardLayer.Outline) HasLoadedOutline = true;

                                //     AddFileStream(MS, e.FileName, drillscaler);
                            }
                        }
                    }
                }
                else
                {

                    Gerber.DetermineBoardSideAndLayer(a, out aSide, out aLayer);
                }
                if (aLayer == BoardLayer.Outline) HasLoadedOutline = true;
            }

            foreach (var a in FileList)
            {
                if (Logger != null) Logger.AddString(String.Format("Loading {0}", Path.GetFileName(a)));
                string ext = Path.GetExtension(a);
                if (ext == ".zip")
                {
                    using (ZipFile zip1 = ZipFile.Read(a))
                    {
                        foreach (ZipEntry e in zip1)
                        {
                            MemoryStream MS = new MemoryStream();
                            if (e.IsDirectory == false)
                            {
                                if (Logger != null) Logger.AddString(String.Format("Loading inside zip: {0}", Path.GetFileName(e.FileName)));

                                e.Extract(MS);
                                MS.Seek(0, SeekOrigin.Begin);
                                AddFileToSet(MS, e.FileName, Logger);
                            }
                        }
                    }
                }
                else
                {
                    MemoryStream MS2 = new MemoryStream();
                    FileStream FS = File.OpenRead(a);
                    FS.CopyTo(MS2);
                    MS2.Seek(0, SeekOrigin.Begin);
                    AddFileToSet(MS2, a, Logger);
                }
            }

            if (fixgroup)
            {
                if (Logger != null) Logger.AddString("Checking for common file format mistakes.");
                FixEagleDrillExportIssues(Logger);
                CheckRelativeBoundingBoxes(Logger);
                CheckForOutlineFiles(Logger);

                CheckRelativeBoundingBoxes(Logger);

            }
        }

        public ParsedGerber AddBoardToSet(ProgressLog log, string _originalfilename, bool forcezerowidth = false, bool precombinepolygons = false, double drillscaler = 1.0)
        {
            if (Streams.ContainsKey(_originalfilename))
            {
                return AddBoardToSet(log, Streams[_originalfilename], _originalfilename, forcezerowidth, precombinepolygons, drillscaler) ;
            }
            return null;
        }
        public ParsedGerber AddBoardToSet(ProgressLog log, MemoryStream MS, string _originalfilename, bool forcezerowidth = false, bool precombinepolygons = false, double drillscaler = 1.0)
        {
            Streams[_originalfilename] = MS;
            try
            {
                //   string[] filesplit = originalfilename.Split('.');
                //     string ext = filesplit[filesplit.Count() - 1].ToLower();

                var FileType = Gerber.FindFileTypeFromStream(new StreamReader(MS), _originalfilename);
                MS.Seek(0, SeekOrigin.Begin);

                if (FileType == BoardFileType.Unsupported)
                {
                    if (Gerber.ExtremelyVerbose) Console.WriteLine("Warning: {1}: files with extension {0} are not supported!", Path.GetExtension(_originalfilename), Path.GetFileName(_originalfilename));
                    return null;
                }


                ParsedGerber PLS;
                GerberParserState State = new GerberParserState() { PreCombinePolygons = precombinepolygons };

                if (FileType == BoardFileType.Drill)
                {
                    if (Gerber.ExtremelyVerbose) Console.WriteLine("Log: Drill file: {0}", _originalfilename);
                    PLS = PolyLineSet.LoadExcellonDrillFileFromStream(log, new StreamReader(MS), _originalfilename, false, drillscaler);
                    MS.Seek(0, SeekOrigin.Begin);

                    ExcellonFile EF = new ExcellonFile();
                    EF.Load(log, new StreamReader(MS), drillscaler);
                    Excellons.Add(EF);
                }
                else
                {
                    if (Gerber.ExtremelyVerbose) Console.WriteLine("Log: Gerber file: {0}", _originalfilename);
                    BoardSide Side = BoardSide.Unknown;
                    BoardLayer Layer = BoardLayer.Unknown;
                    Gerber.DetermineBoardSideAndLayer(_originalfilename, out Side, out Layer);
                    if (Layer == BoardLayer.Outline)
                    {
                        forcezerowidth = true;
                        precombinepolygons = true;
                    }
                    State.PreCombinePolygons = precombinepolygons;

                    PLS = PolyLineSet.LoadGerberFileFromStream(log, new StreamReader(MS), _originalfilename, forcezerowidth, false, State);
                    MS.Seek(0, SeekOrigin.Begin);

                    PLS.Side = State.Side;
                    PLS.Layer = State.Layer;
                    if (Layer == BoardLayer.Outline)
                    {
                        PLS.FixPolygonWindings();
                    }
                }

                PLS.CalcPathBounds();
                BoundingBox.AddBox(PLS.BoundingBox);

                Console.WriteLine("Progress: Loaded {0}: {1:N1} x {2:N1} mm", Path.GetFileName(_originalfilename), PLS.BoundingBox.BottomRight.X - PLS.BoundingBox.TopLeft.X, PLS.BoundingBox.BottomRight.Y - PLS.BoundingBox.TopLeft.Y);
                PLSs.Add(PLS);
                //     }
                //     catch (Exception)
                //    {
                //   }

                return PLS;
            }
            catch (Exception E)
            {
                while (E != null)
                {
                    Console.WriteLine("Exception adding board: {0}", E.Message);
                    E = E.InnerException;
                }
            }
            return null;
        }

        public void AddFileToSet(string aname, ProgressLog Logger, double drillscaler = 1.0)
        {
            if (Streams.ContainsKey(aname))
            {
                AddFileToSet(Streams[aname], aname, Logger, drillscaler);
            }
            else
            {
                Logger.AddString(String.Format("[ERROR] no stream for {0}!!!", aname));
            }
        }
        public void AddFileToSet(MemoryStream MS, string aname, ProgressLog Logger, double drillscaler = 1.0)
        {

            Streams[aname] = MS;

            ///string[] filesplit = a.Split('.');

            bool zerowidth = false;
            bool precombine = false;

            BoardSide aSide;
            BoardLayer aLayer;
            Gerber.DetermineBoardSideAndLayer(aname, out aSide, out aLayer);

            if (aLayer == BoardLayer.Outline || (aLayer == BoardLayer.Mill && HasLoadedOutline == false))
            {
                zerowidth = true;
                precombine = true;
            }
            AddBoardToSet(Logger, MS, aname, zerowidth, precombine, drillscaler);
        }
        public void CheckForOutlineFiles(ProgressLog Logger)
        {
            List<ParsedGerber> Outlines = new List<ParsedGerber>();
            List<ParsedGerber> Mills = new List<ParsedGerber>();
            List<ParsedGerber> Unknowns = new List<ParsedGerber>();
            foreach (var a in PLSs)
            {
                if (a.Side == BoardSide.Both && (a.Layer == BoardLayer.Outline))
                {
                    Outlines.Add(a);
                }
                if (a.Side == BoardSide.Both && (a.Layer == BoardLayer.Mill))
                {
                    Mills.Add(a);
                }
                if (a.Side == BoardSide.Unknown && a.Layer == BoardLayer.Unknown)
                {
                    Unknowns.Add(a);
                    Errors.Add(String.Format("Unknown file in set:{0}", Path.GetFileName(a.Name)));
                    if (Logger != null) Logger.AddString(String.Format("Unknown file in set:{0}", Path.GetFileName(a.Name)));
                }

            }

            if (Outlines.Count == 0)
            {
                if (Unknowns.Count == 0)
                {
                    Errors.Add(String.Format("No outline file found and all other files accounted for! "));
                    if (Logger != null) Logger.AddString(String.Format("No outline file found and all other files accounted for! "));

                    // if (Mills.Count == 1)
                    // {
                    //    Mills[0].Layer = BoardLayer.Outline;
                    //   Errors.Add(String.Format("Elevating mill file to outline!"));
                    //  if (Logger != null) Logger.AddString(String.Format("Elevating mill file to outline!"));
                    // }
                    // else
                    //                    if (!InventOutlineFromMill())
                    {
                        CreateBoxOutline();
                    }
                }
                else
                {
                    CreateBoxOutline();
                    return;

                    //InventOutline();
                    //return;
                    //foreach (var a in Unknowns)
                    //{
                    //    PLSs.Remove(a);
                    //    hasgko = true;
                    //    a.Layer = BoardLayer.Outline;
                    //    a.Side = BoardSide.Both;
                    //    Console.WriteLine("Note: Using {0} as outline file", Path.GetFileName(a.Name));

                    //    if (Logger != null) Logger.AddString(String.Format("Note: Using {0} as outline file", Path.GetFileName(a.Name)));

                    //    bool zerowidth = true;
                    //    bool precombine = true;

                    //    var b = AddBoardToSet(a.Name, zerowidth, precombine, 1.0);
                    //    b.Layer = BoardLayer.Outline;
                    //    b.Side = BoardSide.Both;

                    //}
                }
            }
        }

        public void CheckRelativeBoundingBoxes(ProgressLog Logger)
        {


            List<ParsedGerber> DrillFiles = new List<ParsedGerber>();
            List<ParsedGerber> DrillFilesToReload = new List<ParsedGerber>();
            Bounds BB = new Bounds();
            foreach (var a in PLSs)
            {
                if (a.Layer == BoardLayer.Drill)
                {
                    DrillFiles.Add(a);
                }
                else
                {
                    BB.AddBox(a.BoundingBox);
                }
            }

            foreach (var a in DrillFiles)
            {

                if (a.BoundingBox.Intersects(BB) == false)
                {
                    Errors.Add(String.Format("Drill file {0} does not seem to touch the main bounding box!", Path.GetFileName(a.Name)));
                    if (Logger != null) Logger.AddString(String.Format("Drill file {0} does not seem to touch the main bounding box!", Path.GetFileName(a.Name)));
                    PLSs.Remove(a);
                }
            }



            BoundingBox = new Bounds();
            foreach (var a in PLSs)
            {
                //   Console.WriteLine("Progress: Adding board {6} to box::{0:N2},{1:N2} - {2:N2},{3:N2} -> {4:N2},{5:N2}", a.BoundingBox.TopLeft.X, a.BoundingBox.TopLeft.Y, a.BoundingBox.BottomRight.X, a.BoundingBox.BottomRight.Y, a.BoundingBox.Width(), a.BoundingBox.Height(), Path.GetFileName(a.Name));


                //Console.WriteLine("adding box for {0}:{1},{2}", a.Name, a.BoundingBox.Width(), a.BoundingBox.Height());
                BoundingBox.AddBox(a.BoundingBox);
            }

        }
        public void FixEagleDrillExportIssues(ProgressLog Logger)
        {
            List<ParsedGerber> DrillFiles = new List<ParsedGerber>();
            List<Tuple<double, ParsedGerber>> DrillFilesToReload = new List<Tuple<double, ParsedGerber>>();
            Bounds BB = new Bounds();
            foreach (var a in PLSs)
            {
                if (a.Layer == BoardLayer.Drill)
                {
                    DrillFiles.Add(a);
                }
                else
                {
                    BB.AddBox(a.BoundingBox);
                }
            }

            foreach (var a in DrillFiles)
            {
                var b = a.BoundingBox;
                if (b.Width() > BB.Width() * 1.5 || b.Height() > BB.Height() * 1.5)
                {
                    var MaxRatio = Math.Max(b.Width() / BB.Width(), b.Height() / BB.Height());
                    if (Logger != null) Logger.AddString(String.Format("Note: Really large drillfile found({0})-fix your export scripts!", a.Name));
                    Console.WriteLine("Note: Really large drillfile found ({0})- fix your export scripts!", a.Name);
                    DrillFilesToReload.Add(new Tuple<double, ParsedGerber>(MaxRatio, a));
                }

            }
            foreach (var a in DrillFilesToReload)
            {
                PLSs.Remove(a.Item2);
                var scale = 1.0;
                if (Double.IsInfinity(a.Item1) || Double.IsNaN(a.Item1))
                {
                    Errors.Add("Drill file size reached infinity - ignoring it");
                    if (Logger != null) Logger.AddString("Drill file size reached infinity - ignoring it");
                }
                else
                {
                    var R = a.Item1;
                    while (R >= 1.5)
                    {
                        R /= 10;
                        scale /= 10;
                    }
                    AddFileToSet(a.Item2.Name, Logger, scale);
                }
            }

            BoundingBox = new Bounds();
            foreach (var a in PLSs)
            {
                //Console.WriteLine("Progress: Adding board {6} to box::{0:N2},{1:N2} - {2:N2},{3:N2} -> {4:N2},{5:N2}", a.BoundingBox.TopLeft.X, a.BoundingBox.TopLeft.Y, a.BoundingBox.BottomRight.X, a.BoundingBox.BottomRight.Y, a.BoundingBox.Width(), a.BoundingBox.Height(), Path.GetFileName( a.Name));


                //Console.WriteLine("adding box for {0}:{1},{2}", a.Name, a.BoundingBox.Width(), a.BoundingBox.Height());
                BoundingBox.AddBox(a.BoundingBox);
            }
        }
        public Bounds GetOutlineBoundingBox()
        {
            Bounds B = new Bounds();
            int i = 0;
            foreach (var a in PLSs)
            {
                if (a.Layer == BoardLayer.Mill || a.Layer == BoardLayer.Outline)
                {
                    B.AddBox(a.BoundingBox);
                    i++;
                }
            }
            if (i == 0) return BoundingBox;
            return B;
        }
        private bool InventOutline(ProgressLog log)
        {
            double largest = 0;
            ParsedGerber Largest = null;
            PolyLine Outline = null;

            foreach (var a in PLSs)
            {
                var P = a.FindLargestPolygon();
                if (P != null)
                {
                    if (P.Item1 > largest)
                    {
                        largest = P.Item1;
                        Largest = a;
                        Outline = P.Item2;
                    }
                }

            }

            if (largest < BoundingBox.Area() / 3.0) return false;
            bool zerowidth = true;
            bool precombine = true;

            log.AddString(String.Format("Note: Using {0} to extract outline file", Path.GetFileName(Largest.Name)));
            if (Largest.Layer == BoardLayer.Mill)
            {
                Largest.OutlineShapes.Remove(Outline);
                Largest.Shapes.Remove(Outline);
            }

            var b = AddBoardToSet(log, Largest.Name, zerowidth, precombine, 1.0);
            b.Layer = BoardLayer.Outline;
            b.Side = BoardSide.Both;
            b.DisplayShapes.Clear();
            b.OutlineShapes.Clear();
            b.Shapes.Clear();
            Outline.Close();
            b.Shapes.Add(Outline);
            b.OutlineShapes.Add(Outline);
            //b.DisplayShapes.Add(Outline);
            //b.BuildBoundary();
            b.FixPolygonWindings();
            b.CalcPathBounds();

            return true;
        }
        private bool InventOutlineFromMill(ProgressLog log)
        {
            double largest = 0;
            ParsedGerber Largest = null;
            PolyLine Outline = null;

            foreach (var a in PLSs.Where(x => x.Layer == BoardLayer.Mill))
            {
                var P = a.FindLargestPolygon();
                if (P != null)
                {
                    if (P.Item1 > largest)
                    {
                        largest = P.Item1;
                        Largest = a;
                        Outline = P.Item2;
                    }
                }

            }
            if (Largest == null) return false;
            // if (largest < BoundingBox.Area() / 3.0) return false;
            bool zerowidth = true;
            bool precombine = true;

            log.AddString (String.Format("Note: Using {0} to extract outline file", Path.GetFileName(Largest.Name)));

            var b = AddBoardToSet(log, Largest.Name, zerowidth, precombine, 1.0);
            b.Layer = BoardLayer.Outline;
            b.Side = BoardSide.Both;
            //b.DisplayShapes.Clear();
            //b.OutlineShapes.Clear();
            //b.Shapes.Clear();
            // Outline.Close();
            // b.Shapes.Add(Outline);
            // b.OutlineShapes.Add(Outline);
            //b.DisplayShapes.Add(Outline);
            //b.BuildBoundary();
            // b.FixPolygonWindings();
            // b.CalcPathBounds();

            return true;
        }

        public void CreateBoxOutline()
        {
            PolyLine Box = new PolyLine( PolyLine.PolyIDs.Outline);
            Box.MakeRectangle(BoundingBox.Width(), BoundingBox.Height());
            Box.Translate(BoundingBox.TopLeft.X + BoundingBox.Width() / 2.0, BoundingBox.TopLeft.Y + BoundingBox.Height() / 2.0);
            Box.Hole = false;
            //Box.Close();
            // Box.Vertices.Reverse();
            ParsedGerber PLS = new ParsedGerber();
            PLS.Name = "Generated BoundingBox";
            PLS.DisplayShapes.Add(Box);
            PLS.OutlineShapes.Add(Box);
            PLS.Shapes.Add(Box);
            PLS.Layer = BoardLayer.Outline;
            PLS.Side = BoardSide.Both;
            //      PLS.FixPolygonWindings();
            PLS.CalcPathBounds();
            PLSs.Add(PLS);
        }


    }

    public class SickOfBeige : GerberSet
    {
        public string MinimalDXFSave(string outputfile, double offset = 3.0, double holediameter = 3.2)
        {
            if (Directory.Exists(outputfile))
            {
                outputfile = Path.Combine(outputfile, "SickOfBeige");
            }

            PolyLine Biggest = null;
            double BiggestArea = 0;
            List<PointD> Holes = new List<PointD>();
            var holeradius = holediameter / 2.0;


            double Circumference = 2 * Math.PI * holeradius;

            foreach (var a in PLSs.Where(x => x.Layer == BoardLayer.Outline))
            {
                foreach (var b in a.OutlineShapes)
                {
                    var A = b.toPolygon();
                    double LRatio = (b.OutlineLength() / Circumference);
                    double Lperc = Math.Abs(LRatio - 1);
                    if (Lperc < 0.1)
                    {
                        if (b.Vertices.Count > 5)
                        {
                            var C = b.GetCentroid();
                            bool round = true;
                            foreach(var v in b.Vertices)
                            {
                                var L = (C - v).Length();
                                if (Math.Abs(L - holeradius)>  0.2)
                                {
                                    // not very round!
                                    round = false;
                                }
                            }
                            if (round)
                            {
                                Console.WriteLine("Hole detected in outline:{0} {1} {2} {3} {4} {5}", a.Layer, a.Side, C, LRatio, Lperc, b.Vertices.Count);
                                Holes.Add(C);
                            }
                        }
                        // might be hole!
                    }
                    var Area = Clipper.Area(A);
                    if (Area > BiggestArea)
                    {
                        Biggest = b;
                        BiggestArea = Area;
                    }
                }
            }

            Polygons Offsetted = new Polygons();

            List<String> Lines = new List<string>();

            Lines.Add("0");
            Lines.Add("SECTION");
            Lines.Add("2 ");
            Lines.Add("ENTITIES");


            if (Biggest != null)
            {
                Polygons clips = new Polygons();
                clips.Add(Biggest.toPolygon());
                Offsetted = Clipper.OffsetPolygons(clips, offset * 100000.0f, JoinType.jtRound);
                foreach (var poly in Offsetted)
                {
                    PolyLine P = new PolyLine(PolyLine.PolyIDs.Temp);

                    P.fromPolygon(poly);

                    for (int i = 0; i < P.Vertices.Count; i++)
                    {
                        var V1 = P.Vertices[i];
                        var V2 = P.Vertices[(i + 1) % P.Vertices.Count];

                        Lines.Add("0");
                        Lines.Add("LINE");
                        Lines.Add("8");
                        Lines.Add("Outline");
                        Lines.Add("10");
                        Lines.Add(V1.X.ToString().Replace(',', '.'));
                        Lines.Add("20");
                        Lines.Add(V1.Y.ToString().Replace(',', '.'));
                        Lines.Add("11");
                        Lines.Add(V2.X.ToString().Replace(',', '.'));
                        Lines.Add("21");
                        Lines.Add(V2.Y.ToString().Replace(',', '.'));
                    }
                }
            }
            else
            {
                Errors.Add("No longest outline found - not generating offset curve");
            }

            foreach (var a in Excellons)
            {
                foreach (var t in a.Tools)
                {
                    var R = t.Value.Radius;
                    if (Math.Abs(R * 2 - holediameter) < 0.05)
                    {
                        foreach (var h in t.Value.Drills)
                        {
                            Holes.Add(h);
                           
                        }
                    }
                }
            }


            foreach(var a in Holes)
            {
                for (int i = 0; i < 40; i++)
                {
                    double P = i * Math.PI * 2.0 / 40.0;
                    double P2 = (i + 1) * Math.PI * 2.0 / 40.0;
                    var C1 = Math.Cos(P) * holeradius;
                    var C2 = Math.Cos(P2) * holeradius;
                    var S1 = Math.Sin(P) * holeradius;
                    var S2 = Math.Sin(P2) * holeradius;
                    double x1 = a.X + C1;
                    double y1 = a.Y + S1;
                    double x2 = a.X + C2;
                    double y2 = a.Y + S2;

                    Lines.Add("0");
                    Lines.Add("LINE");
                    Lines.Add("8");
                    Lines.Add("Holes");
                    Lines.Add("10");
                    Lines.Add(x1.ToString().Replace(',', '.'));
                    Lines.Add("20");
                    Lines.Add(y1.ToString().Replace(',', '.'));
                    Lines.Add("11");
                    Lines.Add(x2.ToString().Replace(',', '.'));
                    Lines.Add("21");
                    Lines.Add(y2.ToString().Replace(',', '.'));

                }
            }

            Lines.Add("0");
            Lines.Add("ENDSEC");
            Lines.Add("0");
            Lines.Add("EOF");
            File.WriteAllLines(outputfile + ".dxf", Lines);
            float scalefac = 10;
            Console.WriteLine("Report: {0} holes created in case ({1} spacers and {1} screws needed!)", Holes.Count, Holes.Count * 2);
            {
                var BB = new GerberLibrary.Bounds();
                BB.AddPolygons(Offsetted);
                BB.AddPolyLine(Biggest);

                Bitmap B = new Bitmap((int)((BB.Width() ) * scalefac) + 6, (int)((BB.Height()) * scalefac) +6);
                Graphics G = Graphics.FromImage(B);
                G.Clear(Color.Transparent);
                G.Clear(Color.White);

                G.TranslateTransform(3,3);
                G.ScaleTransform(scalefac, scalefac);
                G.TranslateTransform((float)-(BB.TopLeft.X ), (float)-(BB.TopLeft.Y ));
                Pen pen = new Pen(Color.Black, 0.1f);
                Pen pen2 = new Pen(Color.FromArgb(160, 160, 160), 0.1f);
                pen2.DashPattern = new float[2] { 2, 2 };
                GerberImageCreator.ApplyAASettings(G);
                RectangleF R = new RectangleF(0, 0, (float)holediameter, (float)holediameter);

                foreach (var a in Holes)
                {
                    R.X = (float)a.X - (float)holeradius;
                    R.Y = (float)a.Y - (float)holeradius;
                    G.DrawEllipse(pen, R);
                }

                foreach (var poly in Offsetted)
                {
                    PolyLine Pl = new PolyLine(PolyLine.PolyIDs.Temp);

                    Pl.fromPolygon(poly);
                    var Points = new List<PointF>(Pl.Vertices.Count);
                    for (int i = 0; i < Pl.Vertices.Count; i++)
                    {
                        Points.Add(Pl.Vertices[i].ToF());

                    }
                    Points.Add(Pl.Vertices[0].ToF());
                    G.DrawLines(pen, Points.ToArray());
                }

                {
                    PolyLine Pl = Biggest;

                    var Points = new List<PointF>(Pl.Vertices.Count);

                    for (int i = 0; i < Pl.Vertices.Count; i++)
                    {
                        Points.Add(Pl.Vertices[i].ToF());
                    }

                    Points.Add(Pl.Vertices[0].ToF());
                    G.DrawLines(pen2, Points.ToArray());
                }

                var ImagePNG = outputfile + ".png";
                B.Save(ImagePNG);
                return ImagePNG;
            }
        }        
    }
}