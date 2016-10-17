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

namespace GerberLibrary
{
    public class GerberImageCreator
    {
       public double scale = 25.0f / 25.4f; // dpi
       List<ParsedGerber> PLSs = new List<ParsedGerber>();
       // public PointD TopLeft = new PointD(100000, 100000);
        //public PointD BottomRight = new PointD(-100000, -100000);
       public PolyLineSet.Bounds BoundingBox = new PolyLineSet.Bounds();

        public static bool AA = true;

        public void AddBoardToSet(string a, bool forcezerowidth = false, bool precombinepolygons = false)
        {
            string[] filesplit = a.Split('.');
            string ext = filesplit[filesplit.Count() - 1].ToLower();

            var FileType = Gerber.FindFileType(a);

            if (FileType == BoardFileType.Unsupported)
            {
                //                Console.WriteLine("{1}: files with extension {0} are not supported!", ext, Path.GetFileName(a));
                return;

            }


            //    try
            //   {
            ParsedGerber PLS;
            GerberParserState State = new GerberParserState() { 
             PreCombinePolygons = precombinepolygons
            };
            
            if (FileType == BoardFileType.Drill)
            {
                PLS = PolyLineSet.LoadExcellonDrillFile(a);
                // ExcellonFile EF = new ExcellonFile();
                // EF.Load(a);
            }
            else
            {
                BoardSide Side = BoardSide.Unknown;
                BoardLayer Layer = BoardLayer.Unknown;
                Gerber.DetermineBoardSideAndLayer(a, out Side, out Layer);
                if (Layer == BoardLayer.Outline)
                {
                    forcezerowidth = true;
                    precombinepolygons = true;
                }
                State.PreCombinePolygons = precombinepolygons;

                PLS = PolyLineSet.LoadGerberFile(a, forcezerowidth, false, State);
                PLS.Side = State.Side;
                PLS.Layer = State.Layer;
                if (Layer == BoardLayer.Outline)
                {
                    PLS.FixPolygonWindings();
                }
            }

            PLS.CalcPathBounds();
            BoundingBox.AddBox(PLS.BoundingBox);
            
            Console.WriteLine("Progress: Loaded {0}: {1:N1} x {2:N1} mm", Path.GetFileName(a), PLS.BoundingBox.BottomRight.X - PLS.BoundingBox.TopLeft.X, PLS.BoundingBox.BottomRight.Y - PLS.BoundingBox.TopLeft.Y);
            PLSs.Add(PLS);
            //     }
            //     catch (Exception)
            //    {
            //   }
        }

        public void WriteImageFiles(string TargetFileBaseName, double dpi = 200, bool showimage = true, ProgressLog Logger = null)
        {
            if (Logger != null) Logger.AddString("Build top layer image");
            DrawToFile(TargetFileBaseName, BoardSide.Top, dpi, showimage, Logger);
            if (Logger != null) Logger.AddString("Build bottom layer image");
            DrawToFile(TargetFileBaseName, BoardSide.Bottom, dpi, showimage, Logger);
        }

        Bitmap DrawBoard(double dpi, BoardSide CurrentLayer, string basefilename = null, ProgressLog Logger = null)
        {
            scale = dpi / 25.4f; // dpi

            double bw = Math.Abs(BoundingBox.BottomRight.X - BoundingBox.TopLeft.X);
            double bh = Math.Abs(BoundingBox.BottomRight.Y - BoundingBox.TopLeft.Y);
            int width = (int)((bw * scale));
            int height = (int)((bh * scale));

            //if (width > scale * 100) width =  (int)(scale * 100);
            //if (height > scale * 100) height = (int)(scale * 100);

            int w = width + 3;
            int h = height + 3;
            Bitmap _Final = new Bitmap(w, h);
            Bitmap _BoardPlate = new Bitmap(w, h);
            Bitmap _SilkMask = new Bitmap(w, h);

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
                G.TranslateTransform((float)-BoundingBox.TopLeft.X, (float)-BoundingBox.TopLeft.Y);
                TransformCopy = G.Transform.Clone();

            }

            if (Logger != null) Logger.AddString("Drawing outline files");
            Bitmap _OutlineBase = DrawIfExists(width, height, TransformCopy, Color.LightYellow, BoardLayer.Outline, BoardSide.Both, basefilename, true, 1, true);
            if (Logger != null) Logger.AddString("Drawing mill files");
            Bitmap _OutlineMill = DrawIfExists(width, height, TransformCopy, Color.LightYellow, BoardLayer.Mill, BoardSide.Both, basefilename, true, 1, true);



            if (Logger != null) Logger.AddString("Drawing copper files");

            Bitmap _Copper = DrawIfExists(width, height, TransformCopy, Color.FromArgb(80, 80, 0), BoardLayer.Copper, CurrentLayer, basefilename, true);
            if (Logger != null) Logger.AddString("Drawing silk files");
            Bitmap _Silk = DrawIfExists(width, height, TransformCopy, Gerber.BoardRenderSilkColor, BoardLayer.Silk, CurrentLayer, basefilename, true, 0.2f);
            if (Logger != null) Logger.AddString("Drawing soldermask files");

            Bitmap _SolderMaskHoles = DrawIfExists(width, height, TransformCopy, Gerber.BoardRenderPadColor, BoardLayer.SolderMask, CurrentLayer, basefilename, true, 0.2f);
            if (Logger != null) Logger.AddString("Drawing drill files");

            Bitmap _DrillHoles = DrawIfExists(width, height, TransformCopy, Color.Black, BoardLayer.Drill, BoardSide.Both, basefilename, true, 1.0f);

            //DrawIfExists(width, height, G, Color.Black, BoardLayer.Mill, BoardSide.Both, basefilename);
            //      DrawIfExists(filename, width, height, G, Color.Blue, BoardLayer.Drill, BoardSide.Both, false);
            //DrawIfExists(width, height, G, Color.White, BoardLayer.Outline, BoardSide.Both, basefilename, false);
            //DrawIfExists(width, height, G, Color.White, BoardLayer.Mill, BoardSide.Both, basefilename, false);

            //     LockBitmap OutlineBase = new LockBitmap(_OutlineBase);
            //     LockBitmap OutlineMill = new LockBitmap(_OutlineMill);
            //    LockBitmap Copper = new LockBitmap(_Copper);
            //   LockBitmap SolderMaskHoles = new LockBitmap(_SolderMaskHoles);
            //    LockBitmap Silk = new LockBitmap(_Silk);

            //    LockBitmap Final = new LockBitmap(_Final);
            //    LockBitmap SilkMask = new LockBitmap(_SilkMask);

            {
                {
                    Graphics G = Graphics.FromImage(_BoardPlate);

                    ApplyAASettings(G);
                    G.Clear(Color.Transparent);

                    if (_OutlineBase != null) G.DrawImage(_OutlineBase, new Rectangle(0, 0, w, h), 0, 0, w, h, GraphicsUnit.Pixel);
                    if (_OutlineMill != null) G.DrawImage(_OutlineMill, new Rectangle(0, 0, w, h), 0, 0, w, h, GraphicsUnit.Pixel);

                    if (Logger != null) Logger.AddString("Carving inner polygons from board");


                    CarveOutlineAndMillInnerPolygonsFromImage(basefilename, w, h, G, TransformCopy);
                    {
                        G = Graphics.FromImage(_Final);
                        ApplyAASettings(G);
                        G.Clear(Color.Transparent);
                    }
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
                                var O = BoardPlate.GetPixel(x, y);
                                var Drill = DrillHoles.GetPixel(x, y);
                                Color newC = O;
                                if (Drill.A > 0)
                                {
                                    float OA = 1.0f - (Drill.A / 255.0f);
                                    float DA = O.A / 255.0f;
                                    newC = Color.FromArgb((byte)Math.Round((OA * DA) * 255.0f), O.R, O.G, O.B);
                                    BoardPlate.SetPixel(x, y, newC);
                                }
                            }
                        }
                        BoardPlate.UnlockBits();
                        DrillHoles.UnlockBits();
                    }
                    G.DrawImage(_BoardPlate, new Rectangle(0, 0, w, h), 0, 0, w, h, GraphicsUnit.Pixel);
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
                            var O = Final.GetPixel(x, y);
                            var C = Copper.GetPixel(x, y);

                            if (O.A > 0)
                            {
                                if (C.A > 0)
                                {
                                    Color CopperColor = Gerber.BoardRenderPadColor;
                                    float A = (C.A / 255.0f);
                                    float IA = 1 - A;
                                    Color newDC = Color.FromArgb(O.A,
                                        (byte)Math.Round(CopperColor.R * A + O.R * IA),
                                        (byte)Math.Round(CopperColor.G * A + O.G * IA),
                                        (byte)Math.Round(CopperColor.B * A + O.B * IA));
                                    Final.SetPixel(x, y, newDC);
                                }
                            }
                            //G.DrawImage(Copper, new Rectangle(0, 0, w, h), 0, 0, w, h, GraphicsUnit.Pixel);
                        }
                    }
                    Copper.UnlockBits();
                    Final.UnlockBits();
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


                            var O = Final.GetPixel(x, y);
                            var Mask = SolderMaskHoles.GetPixel(x, y);


                            if (Mask.A > 0)
                            {
                                var OSM = SilkMask.GetPixel(x, y);

                                float OA = 1.0f - (Mask.A / 255.0f);
                                float DA = O.A / 255.0f;
                                Color newDC = Color.FromArgb((byte)Math.Round((OA * DA) * 255.0f), O.R, O.G, O.B);
                                SilkMask.SetPixel(x, y, newDC);
                            }


                            Color Cop = Color.Transparent;

                            if (Copper != null) Cop = Copper.GetPixel(x, y);

                            var BmP = BoardPlate.GetPixel(x, y);
                            if (Cop.A > 0 && Mask.A > 0)
                            {

                                Color SurfaceFinish = Gerber.BoardRenderPadColor;
                                Color BaseColor = O;
                                float A = (Cop.A / 255.0f * Mask.A / 255.0f * (BmP.A / 255.0f)) * 0.85f;
                                float IA = 1.0f - A;
                                O = Color.FromArgb((byte)Math.Round(O.A * BmP.A / 255.0f),

                                    (byte)Math.Round(SurfaceFinish.R * A + O.R * IA),
                                    (byte)Math.Round(SurfaceFinish.G * A + O.G * IA),
                                    (byte)Math.Round(SurfaceFinish.B * A + O.B * IA));
                            }

                            Color newC = O;
                            float OA2 = (Mask.A / 255.0f) * 0.7f + 0.3f;
                            //float DA = O.A / 255.0f;

                            float IOA = 1.0f - OA2;

                            Color S = Gerber.BoardRenderColor;



                            newC = Color.FromArgb(O.A,
                                (byte)Math.Round(O.R * OA2 + S.R * IOA),
                                (byte)Math.Round(O.G * OA2 + S.G * IOA),
                                (byte)Math.Round(O.B * OA2 + S.B * IOA));

                            Final.SetPixel(x, y, newC);
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
                    for (int x = 0; x < w; x++)
                    {
                        for (int y = 0; y < h; y++)
                        {
                            var S = Silk.GetPixel(x, y);
                            float AS = S.A / 255.0f;
                            if (AS > 0)
                            {
                                var O = Final.GetPixel(x, y);
                                var Mask = SilkMask.GetPixel(x, y);

                                //    float AO = O.A / 255.0f;
                                float AM = (Mask.A / 255.0f) * (1 - AS);
                                if (Mask.A < 255 && Copper != null)
                                {
                                    var C = Copper.GetPixel(x, y);
                                    if (C.A > 0)
                                    {
                                        AM = AM * (1 - (C.A / 255.0f)) + 1 * (C.A / 255.0f);
                                        //               AM = 1; 
                                    }
                                    //         O = Color.FromArgb(C.A, 200, 200);
                                }

                                float iAM = 1.0f - AM;
                                Color newC = Color.FromArgb(O.A,
                                  (byte)Math.Round(O.R * AM + S.R * iAM),
                                  (byte)Math.Round(O.G * AM + S.G * iAM),
                                  (byte)Math.Round(O.B * AM + S.B * iAM));
                                Final.SetPixel(x, y, newC);
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
                }
                // if (OutlineBase != null) G.DrawImage(OutlineBase, new Rectangle(0, 0, w, h), 0, 0, w, h, GraphicsUnit.Pixel);                
            }

            return _Final;

            //            return B;
        }

        public void DrawAllFiles(string v1, double dpi, ProgressLog Logger = null)
        {

            scale = dpi / 25.4f; // dpi

            double bw = Math.Abs(BoundingBox.BottomRight.X - BoundingBox.TopLeft.X);
            double bh = Math.Abs(BoundingBox.BottomRight.Y - BoundingBox.TopLeft.Y);
            int width = (int)((bw * scale));
            int height = (int)((bh * scale));
       
            int w = width + 3;
            int h = height + 3;

            System.Drawing.Drawing2D.Matrix TransformCopy = null;
                Graphics G = Graphics.FromImage(new Bitmap(1,1));
                G.TranslateTransform(0, h);
                G.ScaleTransform(1, -1);

                G.TranslateTransform(1, 1);
                G.ScaleTransform((float)scale, (float)scale);
                G.TranslateTransform((float)-BoundingBox.TopLeft.X, (float)-BoundingBox.TopLeft.Y);
                TransformCopy = G.Transform.Clone();
        
        

           


            foreach (var L in PLSs)
            {
                //if (L.Count() == 0) return null;
                Bitmap B2 = new Bitmap(w + 3, h + 3);
                Graphics G2 = Graphics.FromImage(B2);
                ApplyAASettings(G2);
                G2.Clear(Color.White);
                G2.Transform = TransformCopy.Clone();

                Pen P = new Pen(Color.Black, 1.0f / (float)(scale));
                int Shapes = 0;
                
                Shapes += DrawLayerToGraphics(Color.Black, true, G2, P, L, false);

                string FileName = v1 + "_" + L.Layer.ToString() + "_" + L.Side.ToString() + ".png";
                if (Logger != null) Logger.AddString(String.Format("Rendering {0}-{1} to bitmap {2}",L.Layer.ToString(), L.Side.ToString(), Path.GetFileName(FileName)));
                
                B2.Save(FileName);
            }

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


        private void CarveOutlineAndMillInnerPolygonsFromImage(string basefilename, int w, int h, Graphics G, System.Drawing.Drawing2D.Matrix TransformCopy)
        {
            var T = G.Transform.Clone();
            G.Transform = TransformCopy;
            var L = from i in PLSs where i.Layer == BoardLayer.Outline || i.Layer == BoardLayer.Mill && i.Side == BoardSide.Both select i;
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
            G.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;

            foreach (var a in InsidePolygons.Values)
            {
                List<PointF> Points = new List<PointF>();

                for (int j = 0; j < a.Count(); j++)
                {
                    var P1 = a.Vertices[j];
                    Points.Add(new PointF((float)((P1.X)), (float)((P1.Y))));
                }


                G.FillPolygon(new SolidBrush(Color.Transparent), Points.ToArray());
            }
            G.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            G.Transform = T;
            if (Gerber.ExtremelyVerbose) Console.WriteLine("polygons #: {0} total, {1} carved", ShapesList.Count, InsidePolygons.Count);


            Bitmap B2 = new Bitmap(w, h);
            Graphics G2 = Graphics.FromImage(B2);
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


                G2.DrawPolygon(new Pen(Color.White, 1.0f / (float)scale), Points.ToArray());

            }
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

            if (Gerber.SaveOutlineImages)
            {
                B2.Save(basefilename + "_outlines.png");
                System.Diagnostics.Process.Start(basefilename + "_outlines.png");
            }

        }

        public string DrawToFile(string basefilename, BoardSide CurrentLayer, double dpi = 400, bool showimage = true, ProgressLog Logger = null)
        {
            Bitmap B = DrawBoard(dpi, CurrentLayer, basefilename, Logger);
            string filename = basefilename + "_Combined_" + CurrentLayer.ToString() + ".png";

            B.Save(filename);
            if (showimage) System.Diagnostics.Process.Start(basefilename + "_Combined_" + CurrentLayer.ToString() + ".png");
            return filename;
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

        public System.Drawing.Drawing2D.Matrix BuildMatrix(int w, int h)
        {
            Bitmap B = new Bitmap(1, 1);
            Graphics G = Graphics.FromImage(B);
            System.Drawing.Drawing2D.Matrix T = new System.Drawing.Drawing2D.Matrix();

            G.TranslateTransform(0, h);
            G.ScaleTransform(1, -1);
            G.TranslateTransform(1, 1);
            G.ScaleTransform((float)scale, (float)scale);
            G.TranslateTransform((float)-BoundingBox.TopLeft.X, (float)-BoundingBox.TopLeft.Y);
            T = G.Transform.Clone();
            return T;

        }

        public Bitmap RenderToBitmap(int w, int h, System.Drawing.Drawing2D.Matrix Transform, Color foregroundcolor,Color backgroundcolor, ParsedGerber PLS, bool fill, bool forcefill = false)
        {
            Bitmap B2;

            try
            {
                B2 = new Bitmap(w + 3, h + 3);
            }
            catch(Exception)
            {
                Console.WriteLine("Failed to create image of size {0}x{1}", w, h);
                return null;
            }

            Graphics G2 = Graphics.FromImage(B2);

            ApplyAASettings(G2);
            G2.Clear(backgroundcolor);
            G2.Transform = Transform.Clone();

            Pen P = new Pen(foregroundcolor, 1.0f / (float)(scale));
            int Shapes = 0;
            Shapes += DrawLayerToGraphics(foregroundcolor, fill, G2, P, PLS, forcefill);
//            if (Shapes == 0) return null;
                return B2;
        }

        private Bitmap RenderToBitmap(int w, int h, System.Drawing.Drawing2D.Matrix T, Color color, BoardLayer boardLayer, BoardSide boardSide, bool fill, bool forcefill = false)
        {
            var L = from i in PLSs where i.Layer == boardLayer && i.Side == boardSide select i;
            //if (L.Count() == 0) return null;
            Bitmap B2 = new Bitmap(w + 3, h + 3);
            Graphics G2 = Graphics.FromImage(B2);
            ApplyAASettings(G2);
            G2.Clear(Color.FromArgb(0, 0, 0, 0));
            G2.Transform = T.Clone();

            Pen P = new Pen(color, 1.0f / (float)(scale));
            int Shapes = 0;
            foreach (var l in L)
            {
                Shapes += DrawLayerToGraphics(color, fill, G2, P, l, forcefill);
            }
            if (Shapes == 0) return null;
            return B2;
        }

        private int DrawLayerToGraphics(Color color, bool fill, Graphics G2, Pen P, ParsedGerber l, bool forcefill = false)
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

        private void DrawShape(Graphics G, Pen P, Color c, PolyLine Shape, bool fill, double dx, double dy)
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

        public void AddBoardsToSet(List<string> FileList)
        {
            bool hasgko = false;
            foreach (var a in FileList)
            {
                BoardSide aSide;
                BoardLayer aLayer;
                Gerber.DetermineBoardSideAndLayer(a, out aSide,out aLayer);
                if (aLayer ==  BoardLayer.Outline) hasgko = true;
            }

            foreach (var a in FileList)
            {
                string[] filesplit = a.Split('.');

                string ext = filesplit[filesplit.Count() - 1].ToLower();
                bool zerowidth = false;
                bool precombine = false;

                BoardSide aSide;
                BoardLayer aLayer;
                Gerber.DetermineBoardSideAndLayer(a, out aSide, out aLayer);



                if (aLayer  ==  BoardLayer.Outline || (aLayer == BoardLayer.Mill  && hasgko == false))
                {
                    zerowidth = true;
                    precombine = true;
                }
                AddBoardToSet(a, zerowidth, precombine);
            }
        }
    }

    public class LockBitmap
    {
        Bitmap source = null;
        IntPtr Iptr = IntPtr.Zero;
        BitmapData bitmapData = null;

        public byte[] Pixels { get; set; }
        public int Depth { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public LockBitmap(Bitmap source)
        {
            this.source = source;
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
                    throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
                }

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
                Console.WriteLine(ex.Message);
                Console.WriteLine("{0},{1}", source.Width, source.Height);
                throw ex;
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
    }
}
