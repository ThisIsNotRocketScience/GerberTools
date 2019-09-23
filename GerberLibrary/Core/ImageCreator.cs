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

namespace GerberLibrary
{
    public class GerberImageCreator
    {
        public static bool AA = true;
        public Bounds BoundingBox = new Bounds();
        public List<String> Errors = new List<string>();
        public double scale = 25.0d / 25.4d; // dpi
        private BoardRenderColorSet ActiveColorSet = new BoardRenderColorSet();
        Dictionary<string, MemoryStream> Streams = new Dictionary<string, MemoryStream>();
        public Dictionary<string, double> DrillFileScale = new Dictionary<string, double>();


        bool hasgko = false;
        
        List<ParsedGerber> PLSs = new List<ParsedGerber>();

        public ParsedGerber GetGerberByName(string name)
        {
            foreach(var a in PLSs)
            {
                if (a.Name == name) return a;
            }
            return null;
        }

        public void ClipBoard(string infile, string outputfile, ProgressLog log)
        {
            var toclip = GetGerberByName(infile);
            if (toclip== null)
            {
                log.AddString(String.Format("file {0} not loaded - not clipping!", infile));
            }
            
           

            var ols = (from a in PLSs where (a.Side == BoardSide.Both && (a.Layer == BoardLayer.Outline || a.Layer == BoardLayer.Mill) && a.Generated == false) select a).ToList();

            if (IsInPolygons(toclip, ols) == true)
            {
                File.Copy(infile, outputfile);
                log.AddString("file not clipped - just copied");
                return;
            }

            GerberArtWriter GAW = new GerberArtWriter();
            int lineID = 0; 
            foreach (var a in toclip.DisplayShapes)
            {
                ClipperLib.Clipper CP = new ClipperLib.Clipper();
                foreach(var ol in ols)
                {
                    var R = ol.FindLargestPolygon();

                    var poly = R.Item2.toPolygon();

                    if (ClipperLib.Clipper.Orientation(poly) == false)
                    {
                        //Console.WriteLine("pos");
                    }
                    else
                    {
                        poly.Reverse();
                        //Console.WriteLine("neg");
                    }

                        CP.AddPolygon(poly, ClipperLib.PolyType.ptClip);
                }
                CP.AddPolygon(a.toPolygon(), ClipperLib.PolyType.ptSubject);
                List<List<ClipperLib.IntPoint>> solution = new List<List<ClipperLib.IntPoint>>();
                
                CP.Execute(ClipperLib.ClipType.ctIntersection, solution);
                foreach(var p in solution)
                {
                    PolyLine P = new PolyLine(lineID++);
                    P.fromPolygon(p);
                    GAW.AddPolygon(P);
                }

            }

            GAW.Write(outputfile);
        }

        private bool IsInPolygons(ParsedGerber toclip, List<ParsedGerber> ols)
        {
            foreach (var o in ols)
            {
                foreach (var oo in o.DisplayShapes)
                {
                    foreach (var p in toclip.DisplayShapes)
                    {
                        foreach (var pt in p.Vertices)
                        {
                            if (Helpers.IsInPolygon(oo.Vertices, pt) == false)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;

        }

        public int Count()
        {
            return PLSs.Count;
        }

        public static void ApplyAASettings(Graphics G)
        {
            if (AA)
            {
                G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                G.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            }
            else
            {
                G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                G.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            }
        }

        public static Color Darker(Color color, double Fac)
        {
            float correctionFactor = 1.0f - (float)Fac;
            float red = (color.R) * correctionFactor;
            float green = (color.G) * correctionFactor;
            float blue = (color.B) * correctionFactor;
            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }

        public static Color Lighter(Color color, double Fac)
        {
            float correctionFactor = (float)Fac;
            float red = (255 - color.R) * correctionFactor + color.R;
            float green = (255 - color.G) * correctionFactor + color.G;
            float blue = (255 - color.B) * correctionFactor + color.B;
            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }

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
                                Gerber.DetermineBoardSideAndLayer (e.FileName, out aSide, out aLayer);
                                if (aLayer == BoardLayer.Outline) hasgko = true;

                                //     AddFileStream(MS, e.FileName, drillscaler);
                            }
                        }
                    }
                }
                else
                {
                    
                    Gerber.DetermineBoardSideAndLayer(a, out aSide, out aLayer);
                }
                if (aLayer == BoardLayer.Outline) hasgko = true;
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
                    try
                    {
                        MemoryStream MS2 = new MemoryStream();
                        FileStream FS = File.OpenRead(a);
                        FS.CopyTo(MS2);
                        MS2.Seek(0, SeekOrigin.Begin);
                        AddFileToSet(MS2, a, Logger);
                    }catch(Exception E)
                    {
                        Logger.AddString(String.Format("Failed to add file! {0}", a));
                    }
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
        
        public ParsedGerber AddBoardToSet(string _originalfilename, bool forcezerowidth = false, bool precombinepolygons = false, double drillscaler = 1.0)
        {
            if (Streams.ContainsKey(_originalfilename))
            {
                return AddBoardToSet(Streams[_originalfilename], _originalfilename, forcezerowidth, precombinepolygons, drillscaler);
            }
            return null;
        }


          public ParsedGerber AddBoardToSet(MemoryStream MS, string _originalfilename, bool forcezerowidth = false, bool precombinepolygons = false, double drillscaler = 1.0)
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
                    if (Gerber.ExtremelyVerbose) Console.WriteLine("Warning: {1}: files with extension {0} are not supported!", Path.GetExtension( _originalfilename), Path.GetFileName(_originalfilename));
                    return null;
                }


                ParsedGerber PLS;
                GerberParserState State = new GerberParserState() { PreCombinePolygons = precombinepolygons };

                if (FileType == BoardFileType.Drill)
                {
                    if (Gerber.ExtremelyVerbose) Console.WriteLine("Log: Drill file: {0}", _originalfilename);
                    PLS = PolyLineSet.LoadExcellonDrillFileFromStream(new StreamReader(MS), _originalfilename, false, drillscaler);
                    MS.Seek(0, SeekOrigin.Begin);
                    PLS.Side = BoardSide.Both;
                    PLS.Layer = BoardLayer.Drill;
                    // ExcellonFile EF = new ExcellonFile();
                    // EF.Load(a);
                    
                }
                else
                {
                    if (Gerber.ExtremelyVerbose) Console.WriteLine("Log: Gerber file: {0}", _originalfilename);
                    BoardSide Side = BoardSide.Unknown;
                    BoardLayer Layer = BoardLayer.Unknown;
                    Gerber.DetermineBoardSideAndLayer(_originalfilename, out Side, out Layer);
                    if (Layer == BoardLayer.Outline || Layer == BoardLayer.Mill)
                    {
                        forcezerowidth = true;
                        precombinepolygons = true;
                    }
                    State.PreCombinePolygons = precombinepolygons;
                    if (Layer == BoardLayer.Silk)
                    {
                        State.IgnoreZeroWidth = true;
                    }
                    PLS = PolyLineSet.LoadGerberFileFromStream(new StreamReader(MS), _originalfilename, forcezerowidth, false, State);
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

        public System.Drawing.Drawing2D.Matrix BuildMatrix(int w, int h)
        {
            Bitmap B = new Bitmap(1, 1);
            Graphics G = Graphics.FromImage(B);
            System.Drawing.Drawing2D.Matrix T = new System.Drawing.Drawing2D.Matrix();
            var OutlineBoundingBox = GetOutlineBoundingBox();

            G.TranslateTransform(0, h);
            G.ScaleTransform(1, -1);
            G.TranslateTransform(1, 1);
            G.ScaleTransform((float)scale, (float)scale);
            G.TranslateTransform((float)-OutlineBoundingBox.TopLeft.X, (float)-OutlineBoundingBox.TopLeft.Y);
            T = G.Transform.Clone();
            return T;

        }

        public void CreateBoxOutline()
        {
            PolyLine Box = new PolyLine(PolyLine.PolyIDs.Outline);
            Box.MakeRectangle(BoundingBox.Width(), BoundingBox.Height());
            Box.Translate(BoundingBox.TopLeft.X + BoundingBox.Width() / 2.0, BoundingBox.TopLeft.Y + BoundingBox.Height() / 2.0);
            Box.Hole = false;
            //Box.Close();
           // Box.Vertices.Reverse();
            ParsedGerber PLS = new ParsedGerber();
            PLS.Name = "Generated BoundingBox";
            PLS.Generated = true;
            PLS.DisplayShapes.Add(Box);
            PLS.OutlineShapes.Add(Box);
            PLS.Shapes.Add(Box);
            PLS.Layer = BoardLayer.Outline;
            PLS.Side = BoardSide.Both;
      //      PLS.FixPolygonWindings();
            PLS.CalcPathBounds();
            PLSs.Add(PLS);
        }

        public void DrawAllFiles(string v1, double dpi, ProgressLog Logger = null)
        {

            scale = dpi / 25.4d; // dpi
            var OutlineBoundingBox = GetOutlineBoundingBox();
            double bw = Math.Abs(OutlineBoundingBox.BottomRight.X - OutlineBoundingBox.TopLeft.X);
            double bh = Math.Abs(OutlineBoundingBox.BottomRight.Y - OutlineBoundingBox.TopLeft.Y);
            int width = (int)((bw * scale));
            int height = (int)((bh * scale));

            int w = width + 3;
            int h = height + 3;

            System.Drawing.Drawing2D.Matrix TransformCopy = null;
            Graphics G = Graphics.FromImage(new Bitmap(1, 1));

            G.TranslateTransform(0, h);
            G.ScaleTransform(1, -1);

            G.TranslateTransform(1, 1);
            G.ScaleTransform((float)scale, (float)scale);
            G.TranslateTransform((float)-OutlineBoundingBox.TopLeft.X, (float)-OutlineBoundingBox.TopLeft.Y);
            TransformCopy = G.Transform.Clone();






            foreach (var L in PLSs)
            {

                string FileName = v1 + "_" + L.Layer.ToString() + "_" + L.Side.ToString() + ".png";
                if (Logger != null) Logger.AddString(String.Format("Rendering {0}-{1}", L.Layer.ToString(), L.Side.ToString()));

                Bitmap B2 = new Bitmap(w + 3, h + 3, PixelFormat.Format32bppArgb);

                Graphics G2 = Graphics.FromImage(B2);
                ApplyAASettings(G2);
                var G3 = new GraphicsGraphicsInterface(G2);
                G3.Clear(Color.White);
                G3.Transform = TransformCopy.Clone();

                Pen P = new Pen(Color.Black, 1.0f / (float)(scale));
                int Shapes = 0;

                Shapes += DrawLayerToGraphics(Color.Black, true, G3, P, L, false);

                B2.Save(FileName);
            }

        }

        public string DrawToFile(string basefilename, BoardSide CurrentLayer, double dpi = 400, bool showimage = true, ProgressLog Logger = null)
        {
            Bitmap B = DrawBoard(dpi, CurrentLayer, ActiveColorSet, basefilename,  Logger);
            string filename = basefilename + "_Combined_" + CurrentLayer.ToString() + ".png";

            B.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
            if (showimage) System.Diagnostics.Process.Start(basefilename + "_Combined_" + CurrentLayer.ToString() + ".png");
            return filename;
        }

        public Bitmap RenderToBitmap(int w, int h, System.Drawing.Drawing2D.Matrix Transform, Color foregroundcolor, Color backgroundcolor, ParsedGerber PLS, bool fill, bool forcefill = false)
        {
            Bitmap B2;

            try
            {
                B2 = new Bitmap(w + 3, h + 3, PixelFormat.Format32bppArgb);
            }
            catch (Exception)
            {
                Console.WriteLine("Error: Failed to create image of size {0}x{1}", w, h);
                return null;
            }

            Graphics G2 = Graphics.FromImage(B2);

            ApplyAASettings(G2);
            var G3 = new GraphicsGraphicsInterface(G2);
            G3.Clear(backgroundcolor);
            G3.Transform = Transform.Clone();

            Pen P = new Pen(foregroundcolor, 1.0f / (float)(scale));
            int Shapes = 0;
            Shapes += DrawLayerToGraphics(foregroundcolor, fill, G3, P, PLS, forcefill);
            //            if (Shapes == 0) return null;
            return B2;
        }

        public void SetColors(BoardRenderColorSet colors)
        {
            ActiveColorSet = colors;
        }

        public void WriteImageFiles(string TargetFileBaseName, double dpi = 200, bool showimage = true, ProgressLog Logger = null)
        {
            if (Logger != null) Logger.AddString("Build top layer image");
            DrawToFile(TargetFileBaseName, BoardSide.Top, dpi, showimage, Logger);
            if (Logger != null) Logger.AddString("Build bottom layer image");
            DrawToFile(TargetFileBaseName, BoardSide.Bottom, dpi, showimage, Logger);
        }

        private void AddFileToSet(string aname, ProgressLog Logger, double drillscaler = 1.0)
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

        private void AddFileToSet(MemoryStream MS, string aname, ProgressLog Logger, double drillscaler = 1.0)
        {

            Streams[aname] = MS;

            ///string[] filesplit = a.Split('.');

            bool zerowidth = false;
            bool precombine = false;

            BoardSide aSide;
            BoardLayer aLayer;
            Gerber.DetermineBoardSideAndLayer(aname, out aSide, out aLayer);

            if (aLayer == BoardLayer.Outline || (aLayer == BoardLayer.Mill && hasgko == false))
            {
                zerowidth = true;
                precombine = true;
            }
            MS.Seek(0, SeekOrigin.Begin);
            AddBoardToSet(MS, aname, zerowidth, precombine, drillscaler);
        }

        private void ApplyBumpMapping(Bitmap _Target, Bitmap _Bump, int w, int h)
        {
            LockBitmap Target = new LockBitmap(_Target);
            Target.LockBits();
            LockBitmap BumpMap = new LockBitmap(_Bump);

            BumpMap.LockBits();

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int idx = (y * BumpMap.Width + x) * 4;
                    Color TargetPixel = Target.GetPixelIDX(idx);
                    Color B1 = BumpMap.GetPixelIDX(idx);

                    if (true)//B1.A > 0)
                    {
                        Color B2 = B1;
                        Color B4 = B1;
                        if (x < w - 1)
                        {
                            B2 = BumpMap.GetPixel(x + 1, y);
                            B4 = B2;
                        }
                        Color B3 = B1;
                        if (y < h - 1) B3 = BumpMap.GetPixel(x, y + 1);
                        if (y < h - 1 && x < w - 1)
                        {
                            B4 = BumpMap.GetPixel(x + 1, y + 1);
                        }
                        float dx1 = (B1.GetBrightness() - B2.GetBrightness());
                        float dx2 = (B3.GetBrightness() - B4.GetBrightness());
                        float dy1 = (B1.GetBrightness() - B3.GetBrightness());
                        float dy2 = (B2.GetBrightness() - B4.GetBrightness());
                        float dx = dx1 + dx2;
                        float dy = dy1 + dy2;

                        if (dx == 0 && dy == 0)
                        {
                            //skip
                        }
                        else
                        {
                            double ang = Math.Atan2(dy, dx);
                            double dist = Math.Sqrt(dx * dx + dy * dy);
                            double L = Math.Sin(ang - 1.4);
                            if (L > 0) Target.SetPixelIDX(idx, Lighter(TargetPixel, L * 0.04));
                            else
                            {
                                Target.SetPixelIDX(idx, Darker(TargetPixel, Math.Abs(L * 0.04)));
                            }
                        }

                    }
                }
            }

            BumpMap.UnlockBits();
            Target.UnlockBits();


        }

        private void CarveOutlineAndMillInnerPolygonsFromImage(string basefilename, int w, int h, Graphics G, Bitmap _Target, System.Drawing.Drawing2D.Matrix TransformCopy)
        {
            var T = G.Transform.Clone();
            G.Transform = TransformCopy;
            var L = from i in PLSs where (i.Layer == BoardLayer.Outline || i.Layer == BoardLayer.Mill) && i.Side == BoardSide.Both select i;
            if (L.Count() == 0) return;

            List<PolyLine> ShapesList = new List<PolyLine>();
            Dictionary<int, PolyLine> InsidePolygons = new Dictionary<int, PolyLine>();
            foreach (var l in L)
            {
                l.FixPolygonWindings();
                ShapesList.AddRange(l.OutlineShapes);

            }

            List<int> AddedIds = new List<int>();
            // Helpers.LineSegmentsToPolygons()

            for (int i = 0; i < ShapesList.Count; i++)
            {
                if (ClipperLib.Clipper.Orientation(ShapesList[i].toPolygon()) == false)
                {
                    InsidePolygons[i] = ShapesList[i];
                }

                //double IArea = Helpers.PolygonSurfaceArea(ShapesList[i].Vertices);

                //for (int j = i + 1; j < ShapesList.Count; j++)
                //{
                //    double JArea = Helpers.PolygonSurfaceArea(ShapesList[j].Vertices);
                //    if (JArea < IArea)
                //    {
                //        if (Helpers.IsInPolygon(ShapesList[i].Vertices, ShapesList[j].Vertices[0]))
                //        {

                //            InsidePolygons[j] = ShapesList[j];
                //        }
                //    }
                //    else
                //    {
                //        if (Helpers.IsInPolygon(ShapesList[j].Vertices, ShapesList[i].Vertices[0]))
                //        {
                //            InsidePolygons[i] = ShapesList[i];
                //        }

                //    }
                //}
            }

            Bitmap B2 = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            Graphics G2 = Graphics.FromImage(B2);
            G2.Clear(Color.Black);
            G2.Transform = TransformCopy;
            ApplyAASettings(G2);


            //G.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;

            foreach (var a in InsidePolygons.Values)
            {
                List<PointF> Points = new List<PointF>();

                for (int j = 0; j < a.Count(); j++)
                {
                    var P1 = a.Vertices[j];
                    Points.Add(new PointF((float)((P1.X)), (float)((P1.Y))));
                }


                G2.FillPolygon(new SolidBrush(Color.Red), Points.ToArray());
            }
            if (Gerber.SaveIntermediateImages) B2.Save("11 outlines_redimages.png");


            LockBitmap Source = new LockBitmap(B2);
            LockBitmap Target = new LockBitmap(_Target);
            Source.LockBits();
            Target.LockBits();

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = (y * Target.Width + x) * 4;

                    var S = Source.GetPixelIDX(idx);
                    if (S.R > 0)
                    {
                        Target.SetPixelIDX(idx, Color.FromArgb(255 - S.R, Target.GetPixelIDX(idx)));
                    }
                }
            }
            Source.UnlockBits();
            Target.UnlockBits();

            if (Gerber.SaveIntermediateImages) _Target.Save("22 outlines_carved.png");


            // G.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            G.Transform = T;
            if (Gerber.ExtremelyVerbose) Console.WriteLine("polygons #: {0} total, {1} carved", ShapesList.Count, InsidePolygons.Count);


            //Bitmap B2 = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            // Graphics G2 = Graphics.FromImage(B2);
            G2.Clear(Color.Black);
            G2.Transform = TransformCopy;
            foreach (var a in ShapesList)
            {
                List<PointF> Points = new List<PointF>();

                for (int j = 0; j < a.Count(); j++)
                {
                    var P1 = a.Vertices[j];
                    Points.Add(new PointF((float)((P1.X)), (float)((P1.Y))));
                }

                G2.DrawPolygon(new Pen(Color.Transparent, 1.0f / (float)scale), Points.ToArray());
            }
            if (Gerber.SaveIntermediateImages) B2.Save("33 outlines_3aftershapelist.png");

            foreach (var a in InsidePolygons.Values)
            {
                List<PointF> Points = new List<PointF>();

                for (int j = 0; j < a.Count(); j++)
                {
                    var P1 = a.Vertices[j];
                    Points.Add(new PointF((float)((P1.X)), (float)((P1.Y))));
                }

                G2.DrawPolygon(new Pen(Color.FromArgb(200, 255, 255, 0), 1.0f / (float)scale), Points.ToArray());
            }

            if (Gerber.SaveIntermediateImages) B2.Save("44 outlines_4afterinside.png");

        }

        private void CheckForOutlineFiles(ProgressLog Logger)
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

        private void CheckRelativeBoundingBoxes(ProgressLog Logger)
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

        Bitmap DrawBoard(double dpi, BoardSide CurrentLayer, BoardRenderColorSet Colors, string basefilename = null, ProgressLog Logger = null, bool ForceWhite = false)
        {
            scale = dpi / 25.4d; // dpi
            var OutlineBoundingBox = GetOutlineBoundingBox();

            double bw = Math.Abs(OutlineBoundingBox.BottomRight.X - OutlineBoundingBox.TopLeft.X);
            double bh = Math.Abs(OutlineBoundingBox.BottomRight.Y - OutlineBoundingBox.TopLeft.Y);
            int width = (int)((bw * scale));
            int height = (int)((bh * scale));

            //if (width > scale * 100) width =  (int)(scale * 100);
            //if (height > scale * 100) height = (int)(scale * 100);

            int w = width + 3;
            int h = height + 3;
            Bitmap _Final = new Bitmap(w, h, PixelFormat.Format32bppArgb);

            Bitmap _BoardPlate = new Bitmap(w, h, PixelFormat.Format32bppArgb);

            Bitmap _SilkMask = new Bitmap(w, h, PixelFormat.Format32bppArgb);

            System.Drawing.Drawing2D.Matrix TransformCopy = null;
            //Bitmap B = new Bitmap(width + 3, height + 3);
            {
                Graphics G = Graphics.FromImage(_Final);
                ApplyAASettings(G);
                G.Clear(Color.Transparent);
                G.TranslateTransform(0, h);
                G.ScaleTransform(1, -1);

                if (CurrentLayer == BoardSide.Bottom)
                {
                    G.TranslateTransform(w, 0);
                    G.ScaleTransform(-1, 1);
                }

                G.TranslateTransform(1, 1);
                G.ScaleTransform((float)scale, (float)scale);
                G.TranslateTransform((float)-OutlineBoundingBox.TopLeft.X, (float)-OutlineBoundingBox.TopLeft.Y);
                TransformCopy = G.Transform.Clone();

            }

            if (Logger != null) Logger.AddString("Drawing outline files");
            Bitmap _OutlineBase = DrawIfExists(width, height, TransformCopy, Colors.BoardRenderBaseMaterialColor, BoardLayer.Outline, BoardSide.Both, basefilename, true, 1, true);
            if (Logger != null) Logger.AddString("Drawing mill files");
            Bitmap _OutlineMill = DrawIfExists(width, height, TransformCopy, Colors.BoardRenderBaseMaterialColor, BoardLayer.Mill, BoardSide.Both, basefilename, true, 1, true);



            if (Logger != null) Logger.AddString("Drawing copper files");
            Bitmap _Copper = DrawIfExists(width, height, TransformCopy, Color.FromArgb(80, 80, 0), BoardLayer.Copper, CurrentLayer, basefilename, true);

            if (Logger != null) Logger.AddString("Drawing silk files");
            Bitmap _Silk = DrawIfExists(width, height, TransformCopy, Colors.BoardRenderSilkColor, BoardLayer.Silk, CurrentLayer, basefilename, true, 0.2f);

            if (Logger != null) Logger.AddString("Drawing soldermask files");
            Bitmap _SolderMaskHoles = DrawIfExists(width, height, TransformCopy, Colors.BoardRenderPadColor, BoardLayer.SolderMask, CurrentLayer, basefilename, true, 0.2f);

            if (Logger != null) Logger.AddString("Drawing drill files");
            Bitmap _DrillHoles = DrawIfExists(width, height, TransformCopy, Color.Black, BoardLayer.Drill, BoardSide.Both, basefilename, true, 1.0f);

            if (Gerber.SaveIntermediateImages == true)
            {
                Console.WriteLine("Progress: Writing intermediate images:");
                if (_Copper != null) { _Copper.Save(CurrentLayer.ToString() + "_copper.png"); Console.WriteLine("Progress: Copper"); }
                if (_SolderMaskHoles != null) { _SolderMaskHoles.Save(CurrentLayer.ToString() + "_soldermaskholes.png"); Console.WriteLine("Progress: SolderMask"); }
                if (_DrillHoles != null) { _DrillHoles.Save(CurrentLayer.ToString() + "_drill.png"); Console.WriteLine("Progress: Drill"); }
                if (_OutlineBase != null) { _OutlineBase.Save(CurrentLayer.ToString() + "_base.png"); Console.WriteLine("Progress: Base"); }
                if (_OutlineMill != null) { _OutlineMill.Save(CurrentLayer.ToString() + "_mill.png"); Console.WriteLine("Progress: Mill"); }
            }

            //DrawIfExists(width, height, G, Color.Black, BoardLayer.Mill, BoardSide.Both, basefilename);
            //DrawIfExists(filename, width, height, G, Color.Blue, BoardLayer.Drill, BoardSide.Both, false);
            //DrawIfExists(width, height, G, Color.White, BoardLayer.Outline, BoardSide.Both, basefilename, false);
            //DrawIfExists(width, height, G, Color.White, BoardLayer.Mill, BoardSide.Both, basefilename, false);

            //LockBitmap OutlineBase = new LockBitmap(_OutlineBase);
            //LockBitmap OutlineMill = new LockBitmap(_OutlineMill);
            //LockBitmap Copper = new LockBitmap(_Copper);
            //LockBitmap SolderMaskHoles = new LockBitmap(_SolderMaskHoles);
            //LockBitmap Silk = new LockBitmap(_Silk);

            //LockBitmap Final = new LockBitmap(_Final);
            //LockBitmap SilkMask = new LockBitmap(_SilkMask);

            {
                {
                    Graphics G = Graphics.FromImage(_BoardPlate);

                    ApplyAASettings(G);
                    G.Clear(Color.Transparent);

                    if (_OutlineBase != null) G.DrawImage(_OutlineBase, new Rectangle(0, 0, w, h), 0, 0, w, h, GraphicsUnit.Pixel);
                    if (_OutlineMill != null) G.DrawImage(_OutlineMill, new Rectangle(0, 0, w, h), 0, 0, w, h, GraphicsUnit.Pixel);

                    if (Logger != null) Logger.AddString("Carving inner polygons from board");

                    if (Gerber.SaveIntermediateImages == true) _BoardPlate.Save("00 Outlines before carve.png");

                    CarveOutlineAndMillInnerPolygonsFromImage(basefilename, w, h, G, _BoardPlate, TransformCopy);
                    if (Gerber.SaveIntermediateImages == true) _BoardPlate.Save("66 OutlinesCarved.png");

                    G = Graphics.FromImage(_Final);
                    ApplyAASettings(G);
                    G.Clear(Color.Transparent);

                    if (Logger != null) Logger.AddString("Carving drills from board");

                    if (_DrillHoles != null)
                    {
                        LockBitmap DrillHoles = new LockBitmap(_DrillHoles);
                        LockBitmap BoardPlate = new LockBitmap(_BoardPlate);
                        BoardPlate.LockBits();
                        DrillHoles.LockBits();

                        for (int x = 0; x < w; x++)
                        {
                            for (int y = 0; y < h; y++)
                            {
                                int idx = (y * BoardPlate.Width + x) * 4;
                                var O = BoardPlate.GetPixelIDX(idx);
                                var Drill = DrillHoles.GetPixelIDX(idx);
                                Color newC = O;
                                if (Drill.A > 0)
                                {
                                    float OA = 1.0f - (Drill.A / 255.0f);
                                    float DA = O.A / 255.0f;
                                    newC = Color.FromArgb((byte)Math.Round((OA * DA) * 255.0f), O.R, O.G, O.B);
                                    BoardPlate.SetPixelIDX(idx, newC);
                                }
                            }
                        }
                        BoardPlate.UnlockBits();
                        DrillHoles.UnlockBits();
                    }
                    G.DrawImage(_BoardPlate, new Rectangle(0, 0, w, h), 0, 0, w, h, GraphicsUnit.Pixel);

                    if (Gerber.SaveIntermediateImages == true) _BoardPlate.Save("BoardPlateAfterDrills.png");
                }
                if (Logger != null) Logger.AddString("Layering copper on board");

                if (_Copper != null)
                {
                    LockBitmap Final = new LockBitmap(_Final);
                    LockBitmap Copper = new LockBitmap(_Copper);
                    Final.LockBits();
                    Copper.LockBits();
                    for (int x = 0; x < w; x++)
                    {
                        for (int y = 0; y < h; y++)
                        {
                            int idx = (y * Copper.Width + x) * 4;

                            var O = Final.GetPixelIDX(idx);
                            var C = Copper.GetPixelIDX(idx);

                            if (O.A > 0)
                            {
                                if (C.A > 0)
                                {
                                    Color CopperColor = Colors.BoardRenderPadColor;
                                    float A = (C.A / 255.0f);
                                    float IA = 1 - A;
                                    Color newDC = Color.FromArgb(O.A,
                                        (byte)Math.Round(CopperColor.R * A + O.R * IA),
                                        (byte)Math.Round(CopperColor.G * A + O.G * IA),
                                        (byte)Math.Round(CopperColor.B * A + O.B * IA));
                                    Final.SetPixelIDX(idx, newDC);
                                }
                            }
                            //G.DrawImage(Copper, new Rectangle(0, 0, w, h), 0, 0, w, h, GraphicsUnit.Pixel);
                        }
                    }
                    Copper.UnlockBits();
                    Final.UnlockBits();
                    if (Gerber.SaveIntermediateImages == true) _Final.Save("FinalAfterCopper.png");

                }
                {
                    Graphics G = Graphics.FromImage(_SilkMask);
                    ApplyAASettings(G); G.Clear(Color.Transparent);
                    G.DrawImage(_BoardPlate, new Rectangle(0, 0, w, h), 0, 0, w, h, GraphicsUnit.Pixel);
                }
                if (Logger != null) Logger.AddString("Applying soldermask to board");

                if (_SolderMaskHoles != null)
                {
                    LockBitmap SolderMaskHoles = new LockBitmap(_SolderMaskHoles);
                    SolderMaskHoles.LockBits();
                    LockBitmap Final = new LockBitmap(_Final);
                    Final.LockBits();
                    LockBitmap Copper = null;
                    if (_Copper != null)
                    {
                        Copper = new LockBitmap(_Copper);
                        Copper.LockBits();
                    }
                    LockBitmap SilkMask = new LockBitmap(_SilkMask);
                    SilkMask.LockBits();
                    LockBitmap BoardPlate = new LockBitmap(_BoardPlate);
                    BoardPlate.LockBits();

                    
                    for (int x = 0; x < w; x++)
                    {
                        for (int y = 0; y < h; y++)
                        {

                            int idx = (y * Final.Width + x) * 4;

                            var O = Final.GetPixelIDX(idx);
                            var Mask = SolderMaskHoles.GetPixelIDX(idx);


                            if (Mask.A > 0)
                            {
                                var OSM = SilkMask.GetPixelIDX(idx);

                                float OA = 1.0f - (Mask.A / 255.0f);
                                float DA = O.A / 255.0f;
                                Color newDC = Color.FromArgb((byte)Math.Round((OA * DA) * 255.0f), O.R, O.G, O.B);
                                SilkMask.SetPixelIDX(idx, newDC);
                            }


                            Color Cop = Color.Transparent;

                            if (Copper != null) Cop = Copper.GetPixelIDX(idx);

                            var BmP = BoardPlate.GetPixelIDX(idx);
                            if (Cop.A > 0 && Mask.A > 0)
                            {

                                Color SurfaceFinish = Colors.BoardRenderPadColor;
                                Color BaseColor = O;
                                float A = (Cop.A / 255.0f * Mask.A / 255.0f * (BmP.A / 255.0f)) * 0.85f;
                                float IA = 1.0f - A;
                                O = Color.FromArgb((byte)Math.Round(O.A * BmP.A / 255.0f),

                                    (byte)Math.Round(SurfaceFinish.R * A + O.R * IA),
                                    (byte)Math.Round(SurfaceFinish.G * A + O.G * IA),
                                    (byte)Math.Round(SurfaceFinish.B * A + O.B * IA));
                            }

                            Color newC = O;
                            float OA2 = (Mask.A / 255.0f) * 0.9f + 0.1f;
                            //float DA = O.A / 255.0f;

                            float IOA = 1.0f - OA2;

                            Color S = Colors.BoardRenderColor;

                            if (Cop.A > 0)
                            {
                                S = MathHelpers.Interpolate(S, Colors.BoardRenderTraceColor, Cop.A / 255);
                            }

                            newC = Color.FromArgb(O.A,
                                (byte)Math.Round(O.R * OA2 + S.R * IOA),
                                (byte)Math.Round(O.G * OA2 + S.G * IOA),
                                (byte)Math.Round(O.B * OA2 + S.B * IOA));

                            Final.SetPixelIDX(idx, newC);
                        }
                    }

                    SolderMaskHoles.UnlockBits();
                    Final.UnlockBits();
                    if (Copper != null)
                    {

                        Copper.UnlockBits();
                    }
                    SilkMask.UnlockBits();
                    BoardPlate.UnlockBits();

                    if (Gerber.SaveIntermediateImages == true) _Final.Save("FinalAfterSoldermask.png");

                }
                if (Logger != null) Logger.AddString("Applying silkscreen to board");

                if (_Silk != null)
                {
                    LockBitmap Final = new LockBitmap(_Final);
                    Final.LockBits();

                    LockBitmap Copper = null;
                    if (_Copper != null)
                    {
                        Copper = new LockBitmap(_Copper);
                        Copper.LockBits();
                    }
                    LockBitmap SilkMask = new LockBitmap(_SilkMask);
                    SilkMask.LockBits();
                    LockBitmap Silk = new LockBitmap(_Silk);
                    Silk.LockBits();
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            int idx = (y * Silk.Width + x) * 4;


                            var SilkPixel = Silk.GetPixelIDX(idx);
                            float AS = SilkPixel.A / 255.0f;
                            if (AS > 0)
                            {
                                var OutputPixel = Final.GetPixelIDX(idx);
                                var Mask = SilkMask.GetPixelIDX(idx);

                                //    float AO = O.A / 255.0f;
                                float AM = (Mask.A / 255.0f) * (1 - AS);
                                if (Mask.A < 255 && Copper != null)
                                {
                                    var CopperPixel = Copper.GetPixelIDX(idx);
                                    if (CopperPixel.A > 0)
                                    {
                                        AM = AM * (1 - (CopperPixel.A / 255.0f)) + 1 * (CopperPixel.A / 255.0f);
                                        //               AM = 1; 
                                    }
                                    //         O = Color.FromArgb(C.A, 200, 200);
                                }

                                float iAM = 1.0f - AM;
                                Color newC = Color.FromArgb(OutputPixel.A,
                                  (byte)Math.Round(OutputPixel.R * AM + SilkPixel.R * iAM),
                                  (byte)Math.Round(OutputPixel.G * AM + SilkPixel.G * iAM),
                                  (byte)Math.Round(OutputPixel.B * AM + SilkPixel.B * iAM));
                                Final.SetPixelIDX(idx, newC);
                            }
                        }
                    }
                    Final.UnlockBits();

                    if (Copper != null)
                    {
                        Copper.UnlockBits();
                    }
                    SilkMask.UnlockBits();
                    Silk.UnlockBits();

                    if (Gerber.SaveIntermediateImages == true) _Final.Save("FinalAfterSilk.png");

                }

                if (_Copper != null && Gerber.GerberRenderBumpMapOutput)
                {
                    ApplyBumpMapping(_Final, _Copper, w, h);
                }
                // if (OutlineBase != null) G.DrawImage(OutlineBase, new Rectangle(0, 0, w, h), 0, 0, w, h, GraphicsUnit.Pixel);                
            }

            if (ForceWhite)
            {
                LockBitmap Final = new LockBitmap(_Final);
                Final.LockBits();
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int idx = (y * Final.Width + x) * 4;

                        var C = Final.GetPixelIDX(idx);
                        if (C.A == 0) Final.SetPixelIDX(idx, Color.White);
                    }
                }
                Final.UnlockBits();

            }
            return _Final;

            //            return B;
        }
        private Bitmap DrawIfExists(int w, int h, System.Drawing.Drawing2D.Matrix TransformCopy, Color color, BoardLayer boardLayer, BoardSide boardSide, string filename = null, bool fill = true, float alpha = 0.83f, bool forcefill = false)
        {

            Bitmap B2 = RenderToBitmap(w, h, TransformCopy, color, boardLayer, boardSide, fill, forcefill);


            if (B2 != null)
            {
                if (filename != null)
                {
                    //  B2.Save(filename + "_Layer_" + boardSide.ToString() + "_" + boardLayer.ToString() + ".png");
                }

                //float[][] colorMatrixElements = 
                //{ 
                //           new float[] {1,  0,  0,  0, 0},        // red scaling factor of 2 
                //           new float[] {0,  1,  0,  0, 0},        // green scaling factor of 1 
                //           new float[] {0,  0,  1,  0, 0},        // blue scaling factor of 1 
                //           new float[] {0,  0,  0,  alpha, 0},        // alpha scaling factor of 1 
                //           new float[] {0, 0, 0, 0, 1}   // three translations of 0
                //}; 

                //ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
                //ImageAttributes imageAttributes = new ImageAttributes();
                //imageAttributes.SetColorMatrix(colorMatrix,ColorMatrixFlag.Default,ColorAdjustType.Bitmap);

                //var T = TransformCopy;
                //G.ResetTransform();
                //G.DrawImage(
                //   B2,
                //   new Rectangle(0, 0, w, h),  // destination rectangle 
                //   0, 0,        // upper-left corner of source rectangle 
                //   w,       // width of source rectangle
                //   h,      // height of source rectangle
                //   GraphicsUnit.Pixel, imageAttributes);

                //G.Transform = T;
            }

            return B2;
        }

        private int DrawLayerToGraphics(Color color, bool fill, GraphicsInterface G2, Pen P, ParsedGerber l, bool forcefill = false)
        {
            int RenderedShapes = 0;
            foreach (var Shape in l.DisplayShapes)
            {
                if (Shape.Vertices.Count > 1)
                {
                    //if (Shape.Thin == false)

                    DrawShape(G2, P, color, Shape, (fill && (Shape.Thin == false)) || forcefill, l.TranslationSinceLoad.X, l.TranslationSinceLoad.Y);
                    RenderedShapes++;
                }
            }
            return RenderedShapes;
        }

        private void DrawShape(GraphicsInterface G, Pen P, Color c, PolyLine Shape, bool fill, double dx, double dy)
        {

            List<PointF> Points = new List<PointF>();

            for (int j = 0; j < Shape.Count(); j++)
            {
                var P1 = Shape.Vertices[j];
                Points.Add(new PointF((float)((P1.X - dx)), (float)((P1.Y - dy))));
            }

            if (Points.Count() > 1)
            {
                if (fill)
                {
                    if (Points[0] == Points[Points.Count() - 1])
                    {
                        Points.Remove(Points.Last());
                    }
                    if (Shape.ClearanceMode)
                    {
                        G.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                        G.FillPolygon(new SolidBrush(Color.Transparent), Points.ToArray());
                        G.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                    }
                    else
                    {
                        G.FillPolygon(new SolidBrush(c), Points.ToArray());
                        G.DrawLines(new Pen(c, P.Width / 4), Points.ToArray());
                    }
                }
                else
                {
                    G.DrawLines(P, Points.ToArray());
                }
            }
        }

        private void FixEagleDrillExportIssues(ProgressLog Logger)
        {
            if (Gerber.SkipEagleDrillFix == true) { Logger.AddString("skipping eagle fix"); return; };
            List<ParsedGerber> DrillFiles = new List<ParsedGerber>();
            List<Tuple<double, ParsedGerber>> DrillFilesToReload = new List<Tuple<double, ParsedGerber>>();
            Bounds BB = new Bounds();
            foreach (var a in PLSs)
            {
                if (a.Layer == BoardLayer.Drill)
                {
                    DrillFiles.Add(a);
                    DrillFileScale[a.Name] = 1.0;
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

                    DrillFileScale[a.Item2.Name] = scale;
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


        private Bounds GetOutlineBoundingBox()
        {
            Bounds B = new Bounds();
            int i = 0;
            foreach(var a in PLSs)
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
        private bool InventOutline()
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

            Console.WriteLine("Note: Using {0} to extract outline file", Path.GetFileName(Largest.Name));
            if (Largest.Layer == BoardLayer.Mill)
            {
                Largest.OutlineShapes.Remove(Outline);
                Largest.Shapes.Remove(Outline);
            }

            var b = AddBoardToSet(Largest.Name, zerowidth, precombine, 1.0);
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

        private bool InventOutlineFromMill()
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

            Console.WriteLine("Note: Using {0} to extract outline file", Path.GetFileName(Largest.Name));

            var b = AddBoardToSet(Largest.Name, zerowidth, precombine, 1.0);
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

        private Bitmap RenderToBitmap(int w, int h, System.Drawing.Drawing2D.Matrix T, Color color, BoardLayer boardLayer, BoardSide boardSide, bool fill, bool forcefill = false)
        {
            var L = from i in PLSs where i.Layer == boardLayer && i.Side == boardSide select i;
            //if (L.Count() == 0) return null;
            Bitmap B2 = new Bitmap(w + 3, h + 3, PixelFormat.Format32bppArgb);
            Graphics G2 = Graphics.FromImage(B2);
            ApplyAASettings(G2);
            var G3 = new GraphicsGraphicsInterface(G2);
            G3.Clear(Color.FromArgb(0, 0, 0, 0));
            G3.Transform = T.Clone();
            Pen P = new Pen(color, 1.0f / (float)(scale));
            int Shapes = 0;
            foreach (var l in L)
            {
                Shapes += DrawLayerToGraphics(color, fill,  G3, P, l, forcefill);
            }
            if (Shapes == 0) return null;
            return B2;
        }

        public double GetDrillScaler(string f)
        {
            if (DrillFileScale.ContainsKey(f)) return DrillFileScale[f];
            return 1.0;
        }
    }

    public class LockBitmap
    {
        BitmapData bitmapData = null;
        IntPtr Iptr = IntPtr.Zero;
        Bitmap source = null;
        public LockBitmap(Bitmap source)
        {
            this.source = source;
        }

        public int Depth { get; private set; }
        public int Height { get; private set; }
        public byte[] Pixels { get; set; }
        public int Width { get; private set; }

        public Color GetPixelIDX(int idx)
        {
            byte b = Pixels[idx];
            byte g = Pixels[idx + 1];
            byte r = Pixels[idx + 2];
            byte a = Pixels[idx + 3]; // a
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Get the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Color GetPixel(int x, int y)
        {
            Color clr = Color.Empty;

            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (i > Pixels.Length - cCount)
                throw new IndexOutOfRangeException();

            if (Depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                byte a = Pixels[i + 3]; // a
                clr = Color.FromArgb(a, r, g, b);
            }
            if (Depth == 24) // For 24 bpp get Red, Green and Blue
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                clr = Color.FromArgb(r, g, b);
            }
            if (Depth == 8)
            // For 8 bpp get color value (Red, Green and Blue values are the same)
            {
                byte c = Pixels[i];
                clr = Color.FromArgb(c, c, c);
            }
            return clr;
        }

        /// <summary>
        /// Lock bitmap data
        /// </summary>
        public void LockBits()
        {
            try
            {
                // Get width and height of bitmap
                Width = source.Width;
                Height = source.Height;

                // get total locked pixels count
                int PixelCount = Width * Height;

                // Create rectangle to lock
                Rectangle rect = new Rectangle(0, 0, Width, Height);

                // get source bitmap pixel format size
                Depth = System.Drawing.Bitmap.GetPixelFormatSize(source.PixelFormat);

                // Check if bpp (Bits Per Pixel) is 8, 24, or 32
                if (Depth != 8 && Depth != 24 && Depth != 32)
                {
                    throw new ArgumentException("Error: Only 8, 24 and 32 bpp images are supported.");
                }
                //Console.WriteLine("Lockbits: {0} bit depth", Depth);
                // Lock bitmap and return bitmap data
                bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite,
                                             source.PixelFormat);

                // create byte array to copy pixel values
                int step = Depth / 8;
                Pixels = new byte[PixelCount * step];
                Iptr = bitmapData.Scan0;

                // Copy data from pointer to array
                Marshal.Copy(Iptr, Pixels, 0, Pixels.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                Console.WriteLine("Error: {0},{1}", source.Width, source.Height);
                throw ex;
            }
        }

        /// <summary>
        /// Set the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void SetPixel(int x, int y, Color color)
        {
            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (Depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
                Pixels[i + 3] = color.A;
            }
            if (Depth == 24) // For 24 bpp set Red, Green and Blue
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
            }
            if (Depth == 8)
            // For 8 bpp set color value (Red, Green and Blue values are the same)
            {
                Pixels[i] = color.B;
            }
        }

        /// <summary>
        /// Unlock bitmap data
        /// </summary>
        public void UnlockBits()
        {
            try
            {
                // Copy data from byte array to pointer
                Marshal.Copy(Pixels, 0, Iptr, Pixels.Length);

                // Unlock bitmap data
                source.UnlockBits(bitmapData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SetPixelIDX(int i, Color color)
        {
            Pixels[i] = color.B;
            Pixels[i + 1] = color.G;
            Pixels[i + 2] = color.R;
            Pixels[i + 3] = color.A;
        }
    }
}
