using GerberLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ClipperLib;

using System.Drawing;
using System.Drawing.Drawing2D;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using GerberLibrary.Core;
using GerberLibrary.Core.Primitives;
using static GerberLibrary.PolyLineSet;

namespace GerberLibrary
{
    namespace ArtWork
    {
        public enum BoxAttachment
        {
            None,
            Left,
            Right,
            Top,
            Bottom,
            TopLeft,
            BottomLeft,
            TopRight,
            BottomRight
        }
        public enum ArtLayerStyle
        {
            OffsetCurves_GoldfishBoard,
            CheckerField,
            FlowField,
            Flower,
            ReactDiffuse,
            PrototypeEdge
        }

        public class Functions
        {
            public static void CreateArtLayersForFolder(string foldername, ArtLayerStyle Style)
            {
                if (Directory.Exists(foldername) == false) return;

                string outlinefile = "";
                string topfile = "";
                string bottomfile = "";
                string bottomsilkfile = "";
                string topsilkfile = "";

                foreach (string s in Directory.GetFiles(foldername))
                {
                    BoardSide Side;
                    BoardLayer Layer;
                    if (s.ToLower().Contains("artlayer") == false)
                    {
                        Gerber.DetermineBoardSideAndLayer(s, out Side, out Layer);
                        if (Layer == BoardLayer.Outline)
                        {
                            outlinefile = s;
                        }
                        if (Layer == BoardLayer.SolderMask)
                        {
                            if (Side == BoardSide.Top)
                            {
                                topfile = s;
                            }
                            else
                            {
                                bottomfile = s;
                            }
                        }
                        if (Layer == BoardLayer.Silk)
                        {
                            if (Side == BoardSide.Top)
                            {
                                topsilkfile = s;
                            }
                            else
                            {
                                bottomsilkfile = s;
                            }
                        }
                    }
                }

                if (outlinefile.Length > 0)
                {
                    if (bottomfile.Length > 0) WriteArtLayerFiles(outlinefile, bottomfile, bottomsilkfile, Style);
                    if (topfile.Length > 0 && Style != ArtLayerStyle.PrototypeEdge) WriteArtLayerFiles(outlinefile, topfile, topsilkfile, Style);
                }
            }

            public static void WriteArtLayerFiles(string outline, string paste, string silk, ArtLayerStyle Style)
            {
                BoardSide Side;
                BoardLayer Layer;
                Gerber.DetermineBoardSideAndLayer(paste, out Side, out Layer);
                DoArtLayer(outline, paste, Path.Combine(Path.GetDirectoryName(outline), Path.GetFileNameWithoutExtension(outline) + "_artlayer" + ((Side == BoardSide.Top) ? ".gto" : ".gbo")), Style, silk);
            }

            static void DoArtLayer(string outline, string soldermask, string target, ArtLayerStyle Style, string silk = "")
            {

                switch (Style)
                {
                    case ArtLayerStyle.PrototypeEdge:
                        ArtGenerator_ProtoEdge(outline, soldermask, target, silk);
                        break;
                    case ArtLayerStyle.OffsetCurves_GoldfishBoard:
                        ArtGenerator_OffsetCurves(outline, soldermask, target, silk);
                        break;
                    case ArtLayerStyle.FlowField:
                        ArtGenerator_FlowField(outline, soldermask, target, silk);
                        break;
                    case ArtLayerStyle.CheckerField:
                        ArtGenerator_CheckerField(outline, soldermask, target, silk);
                        break;
                    case ArtLayerStyle.Flower:
                        ArtGenerator_Flower(outline, soldermask, target, silk);
                        break;
                    case ArtLayerStyle.ReactDiffuse:
                        ArtGenerator_ReactDiffuse(outline, soldermask, target, silk);
                        break;

                }

            }

            private static void ArtGenerator_ProtoEdge(string outline, string soldermask, string target, string silk)
            {
                Polygons CombinedSoldermask = new Polygons();
                Console.WriteLine("combining outlines..");

                ParsedGerber Outline = PolyLineSet.LoadGerberFile(outline);
                //Outline.FixPolygonWindings();


                Bitmap B = (Bitmap)Image.FromFile("protoedge.png");
                double Res = 200.0 / 25.4;
                B.RotateFlip(RotateFlipType.RotateNoneFlipY);

                GerberLibrary.ArtWork.Functions.WriteBitmapToGerber(target + "_protoedge.gbo", Outline, Res, B, -128, false, true);

            }


            private static void FindPixel(int[,] inarray, out int x, out int y)
            {
                for (int xx = 0; xx < inarray.GetLength(0); xx++)
                {
                    for (int yy = 0; yy < inarray.GetLength(1); yy++)
                    {
                        if (inarray[xx, yy] == 1)
                        {
                            x = xx;
                            y = yy;
                            return;
                        }
                    }
                }
                x = -1;
                y = -1;
            }
            private static int CheckPixel(int[,] inarray, int x, int y)
            {
                if (x < 0 || x >= inarray.GetLength(0)) return -1;
                if (y < 0 || y >= inarray.GetLength(1)) return -1;

                if (inarray[x, y] == 1) return 1;
                return 0;
            }

            public static ParsedGerber BuildMazeFromBitmap(string filename)
            {
                ParsedGerber Gerb = new ParsedGerber();
                Gerb.Shapes.Clear();
                Bitmap B = (Bitmap)Image.FromFile(filename);
                double aspect = (double)B.Width / (double)B.Height;

                double Scale = 20.0f / (double)B.Width;
                if (B.Height > B.Width)
                {
                    Scale = 20.0f / (double)B.Height;
                }
                int[,] M = new int[150, 150];
                int xx = 0;
                int PixelCount = 0;
                for (int x = 0; x < B.Width; x += B.Width / 50)
                {
                    int yy = 0;
                    for (int y = 0; y < B.Height; y += B.Height / 50)
                    {
                        Color C = B.GetPixel(x, y);
                        if (C.GetBrightness() < 0.2)
                        {
                            M[xx, yy] = 1;
                            PixelCount++;
                        }
                        else
                        {
                            M[xx, yy] = 0;

                        }
                        yy++;
                    }
                    xx++;
                }
                //  Up();
                Random R = new Random();
                double ScaleB = 25.0f / 50.0f;
                while (PixelCount > 0)
                {
                    PolyLine S = new PolyLine(-6);
                    int sx = 0;
                    int sy = 0;
                    FindPixel(M, out sx, out sy);
                    S.Vertices.Add(new PointD(sx * ScaleB, -sy * ScaleB / aspect + 50));
                    M[sx, sy] = 0;
                    PixelCount--;
                    //aMove(sx * ScaleB , -sy * ScaleB / aspect + 50);
                    //Down();
                    List<Point> Checks = new List<Point>();
                    if (CheckPixel(M, sx - 1, sy) == 1) Checks.Add(new Point(sx - 1, sy));
                    if (CheckPixel(M, sx + 1, sy) == 1) Checks.Add(new Point(sx + 1, sy));
                    if (CheckPixel(M, sx, sy - 1) == 1) Checks.Add(new Point(sx, sy - 1));
                    if (CheckPixel(M, sx, sy + 1) == 1) Checks.Add(new Point(sx, sy + 1));
                    int len = 0;
                    while (Checks.Count > 0 && len < 20)
                    {
                        len++;
                        int idx = R.Next(Checks.Count);

                        sx = Checks[idx].X;
                        sy = Checks[idx].Y;
                        M[sx, sy] = 0;
                        PixelCount--;
                        S.Vertices.Add(new PointD(sx * ScaleB, -sy * ScaleB / aspect + 50));
                        //aMove(sx * ScaleB, -sy * ScaleB / aspect + 50);

                        Checks.Clear();
                        if (CheckPixel(M, sx - 1, sy) == 1) Checks.Add(new Point(sx - 1, sy));
                        if (CheckPixel(M, sx + 1, sy) == 1) Checks.Add(new Point(sx + 1, sy));
                        if (CheckPixel(M, sx, sy - 1) == 1) Checks.Add(new Point(sx, sy - 1));
                        if (CheckPixel(M, sx, sy + 1) == 1) Checks.Add(new Point(sx, sy + 1));


                    }
                    Gerb.Shapes.Add(S);
                }
                Gerb.Optimize();

                return Gerb;
            }



            public abstract class BounceInterface
            {
                public abstract void RD_Bounce(RD_Elem[] FieldA, RD_Elem[] FieldB, int iW, int iH, float feed, float kill, double[] distancefield, float du = 0.2097f, float dv = 0.105f);


                public abstract void BounceN(int p, RD_Elem[] FieldA, RD_Elem[] FieldB, int iW, int iH, float feedrate, float killrate, double[] DistanceFieldBlur);

            }
            public static BounceInterface TheBounceInterface = null;
            public class RD_Elem
            {
                public float R;
                public float G;
            }

            private static void ArtGenerator_ReactDiffuse(string outline, string soldermask, string target, string silk)
            {
                if (TheBounceInterface == null) TheBounceInterface = new BasicBounce();
                Console.WriteLine("Artlayer {0} started: CheckerField", target);
                ParsedGerber Outline ;
                ParsedGerber SolderMask; 
                Outline = PolyLineSet.LoadGerberFile(outline, true, false, new GerberParserState() {  PreCombinePolygons = true});
                Outline.FixPolygonWindings();

                SolderMask = PolyLineSet.LoadGerberFile(soldermask, false, false, new GerberParserState() { PreCombinePolygons = false, MinimumApertureRadius = 0.1});
                SolderMask.FixPolygonWindings();

                
                Polygons CombinedOutline = new Polygons();
                Polygons CombinedSoldermask = new Polygons();
                Console.WriteLine("combining outlines..");

                for (int i = 0; i < Outline.OutlineShapes.Count; i++)
                {
                    Polygons clips = new Polygons();

                    clips.Add(Outline.OutlineShapes[i].toPolygon());
                    Clipper cp = new Clipper();
                    cp.AddPolygons(CombinedOutline, PolyType.ptSubject);
                    cp.AddPolygons(clips, PolyType.ptClip);

                    cp.Execute(ClipType.ctXor, CombinedOutline, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
                }
                Console.WriteLine("removing paste curves..");
                if (true)
                    for (int i = 0; i < SolderMask.OutlineShapes.Count; i++)
                    {
                        Polygons clips = new Polygons();

                        clips.Add(SolderMask.OutlineShapes[i].toPolygon());
                        Clipper cp = new Clipper();
                        cp.AddPolygons(CombinedOutline, PolyType.ptSubject);
                        cp.AddPolygons(clips, PolyType.ptClip);

                        cp.Execute(ClipType.ctDifference, CombinedOutline, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
                    }




                Polygons CombinedSilk = new Polygons();
                Polygons OriginalCombinedSilk = new Polygons();

                if (silk.Length > 0)
                {
                    Console.WriteLine("removing silk curves..");
                    
                    ParsedGerber Silk = PolyLineSet.LoadGerberFile(silk, false, false, new GerberParserState() { PreCombinePolygons = false, MinimumApertureRadius = 0.1 });

                    //Silk.FixPolygonWindings();



                    Console.WriteLine("building big polygon..");
                    {

                        Polygons clips = new Polygons();

                        for (int i = 0; i < Silk.DisplayShapes.Count; i++)
                        {

                            clips.Add(Silk.DisplayShapes[i].toPolygon());
                        }
                        Clipper cp = new Clipper();
                        cp.AddPolygons(CombinedSilk, PolyType.ptSubject);
                        cp.AddPolygons(clips, PolyType.ptClip);

                        cp.Execute(ClipType.ctUnion, OriginalCombinedSilk, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
                    }
                    Console.WriteLine("offsetting big polygon..");

                    CombinedSilk = Clipper.OffsetPolygons(OriginalCombinedSilk, 1.0 * 100000.0f, JoinType.jtRound, 0.1 * 100000.0f);
                    {
                        Clipper cp = new Clipper();
                        cp.AddPolygons(CombinedOutline, PolyType.ptSubject);
                        cp.AddPolygons(CombinedSilk, PolyType.ptClip);

                        cp.Execute(ClipType.ctDifference, CombinedOutline, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

                    }

                }


                double W = Outline.BoundingBox.BottomRight.X - Outline.BoundingBox.TopLeft.X;
                double H = Outline.BoundingBox.BottomRight.Y - Outline.BoundingBox.TopLeft.Y;
                double Res = 200.0 / 25.4;
                int iW = (int)(W * Res) + 2;
                int iH = (int)(H * Res) + 2;
                int Range = 10;// Math.Min(iW, iH) / 2;
                Bitmap B = new Bitmap(iW, iH);
                Bitmap B2 = new Bitmap(iW, iH);
                Bitmap B3 = new Bitmap(iW, iH);
                Graphics G2 = Graphics.FromImage(B3);
                G2.Clear(Color.Black);
                double[] DistanceField = new double[iW * iH];
                double[] DistanceFieldBlur = new double[iW * iH];
                double[] AngleField = new double[iW * iH];
                bool DistMode = false;

                for (int i = 0; i < iW * iH; i++) { DistanceField[i] = DistMode ? 0 : 100000; AngleField[i] = 0; };
                Graphics G = Graphics.FromImage(B);
                G.Clear(Color.Black);
                Console.WriteLine("Rendering Base Bitmap");
                G.SmoothingMode = SmoothingMode.AntiAlias;

                foreach (var a in CombinedOutline)
                {
                    PolyLine P = new PolyLine(PolyLine.PolyIDs.ArtWork);
                    P.fromPolygon(a);
                    List<PointF> Pts = new List<PointF>();
                    foreach (var V in P.Vertices)
                    {
                        Pts.Add(new PointF((float)((V.X - Outline.BoundingBox.TopLeft.X) * Res) + 1, (float)((V.Y - Outline.BoundingBox.TopLeft.Y) * Res) + 1));
                    }
                    G.DrawPolygon(new Pen(Color.White, 4), Pts.ToArray());

                    P.CheckIfHole();
                    if (P.Hole)
                    {
                        G2.FillPolygon(new SolidBrush(Color.Red), Pts.ToArray());
                    }
                    else
                    {
                        G2.FillPolygon(new SolidBrush(Color.Red), Pts.ToArray());

                    }


                }

                foreach (var a in OriginalCombinedSilk)
                {
                    PolyLine P = new PolyLine(PolyLine.PolyIDs.ArtWork);
                    P.fromPolygon(a);
                    List<PointF> Pts = new List<PointF>();
                    foreach (var V in P.Vertices)
                    {
                        Pts.Add(new PointF((float)((V.X - Outline.BoundingBox.TopLeft.X) * Res) + 1, (float)((V.Y - Outline.BoundingBox.TopLeft.Y) * Res) + 1));
                    }
                    G.DrawPolygon(new Pen(Color.Red, 9), Pts.ToArray());

                    P.CheckIfHole();
                    if (P.Hole)
                    {
                        G2.FillPolygon(new SolidBrush(Color.Red), Pts.ToArray());
                    }
                    else
                    {
                        G2.FillPolygon(new SolidBrush(Color.Red), Pts.ToArray());

                    }

                }
                B.Save(target + "_renderbase.png");


                Console.WriteLine("Calculating Distance Field");

                {

                    for (int y = 0; y < iH; y++)
                    {
                        for (int x = 0; x < iW; x++)
                        {
                            var C = B.GetPixel(x, y);
                            if (C.R > 10)
                            {
                                for (int xx = Math.Max(x - Range, 0); xx < Math.Min(iW, x + Range); xx++)
                                {
                                    for (int yy = Math.Max(y - Range, 0); yy < Math.Min(iH, y + Range); yy++)
                                    {
                                        if (DistMode)
                                        {
                                            double dist = (xx - x) * (xx - x) + (yy - y) * (yy - y);


                                            DistanceField[xx + yy * iW] += 1.0 / (1.0 + dist * 0.1);
                                        }
                                        else
                                        {
                                            double dist = Math.Sqrt((xx - x) * (xx - x) + (yy - y) * (yy - y));
                                            if (dist < DistanceField[xx + yy * iW]) DistanceField[xx + yy * iW] = dist;



                                        }

                                    }
                                }
                            }
                        }
                    }
                }
                Console.WriteLine("Blurring Distance Field");

                double maxblurval = 0;
                double minblurval = 100000000000;
                {

                    for (int y = 0; y < iH; y++)
                    {
                        for (int x = 0; x < iW; x++)
                        {
                            double T = 0;
                            double amt = 0;
                            for (int xx = 0; xx < 1; xx++)
                            {
                                for (int yy = 0; yy < 1; yy++)
                                {
                                    double D = xx * xx + yy * yy;
                                    double M = Math.Exp(0);
                                    if (D > 0)
                                    {
                                        M = Math.Exp(-(xx * xx) * 0.01) * Math.Exp(-(yy * yy) * 0.01); ;

                                    }
                                    amt += M;
                                    T += DistanceField[((x + xx + iW) % iW) + ((y + yy + iH) % iH) * iW] * M;

                                }
                            }
                            double newval = T / amt;
                            DistanceFieldBlur[x + iW * y] = newval;
                            if (newval > maxblurval) maxblurval = newval;
                            if (newval < minblurval) minblurval = newval;


                        }
                    }
                }
                {
                    // G.Clear(Color.Black);
                    for (int x = 0; x < iW; x++)
                    {

                        for (int y = 0; y < iH; y++)
                        {
                            var a = DistanceFieldBlur[x + y * iW];
                            //a = Math.Sqrt(a);

                            byte R = (byte)((a - minblurval) * 255.0 / (maxblurval - minblurval));
                            Color C = Color.FromArgb(R, R, R);
                            DistanceFieldBlur[x + y * iW] = (DistanceFieldBlur[x + y * iW] - minblurval) / (maxblurval - minblurval);
                            B2.SetPixel(x, y, C);
                        }
                    }
                    B2.Save(target + "_renderDistance.png");
                }


                B.Save(target + "_renderbase_afterdistance.png");


                Console.WriteLine("Calculating DiffusePasses");

                {
                    int img = 0;
                    int forceiter = 0;
                    RD_Elem[] FieldA = new RD_Elem[iW * iH];
                    RD_Elem[] FieldB = new RD_Elem[iW * iH];
                    Random R = new Random(0);
                    for (int y = 0; y < iH; y++)
                    {
                        for (int x = 0; x < iW; x++)
                        {
                            FieldA[x + y * iW] = new RD_Elem();
                            FieldB[x + y * iW] = new RD_Elem();
                        }
                    }

                    for (int y = 0; y < iH; y++)
                    {
                        for (int x = 0; x < iW; x++)
                        {
                            var C = B.GetPixel(x, y);
                            var C0 = B.GetPixel((x + iW - 1) % iW, y);
                            var C1 = B.GetPixel((x + 1) % iW, y);
                            var C2 = B.GetPixel(x, (y - 1 + iH) % iH);
                            var C3 = B.GetPixel(x, (y + 1) % iH);



                            float Gg = (float)R.NextDouble() > 0.3 ? 0.9f : 0;
                            float Rr = (float)R.NextDouble() > 0.8 ? 0.9f : 0; ;
                            if (C.R + C1.R + C2.R + C3.R + C0.R < 200)
                            { Rr = 0.9f; };
                            SetF(FieldA, x, y, iW, iH, Gg, Rr);
                            SetF(FieldB, x, y, iW, iH, 0, 0);
                        }
                    }


                    float feedrate = 0.037f;
                    float killrate = 0.06f;
                    int totalbounce = 1000;
                    for (int i = 0; i < 1; i++)
                    {

                        Console.WriteLine("bounce {0}/{1}", i, totalbounce);

                        TheBounceInterface.BounceN(1000, FieldA, FieldB, iW, iH, feedrate, killrate, DistanceFieldBlur);

                        //TheBounceInterface.RD_Bounce(FieldA, FieldB, iW, iH, feedrate, killrate, DistanceFieldBlur);
                        //TheBounceInterface.RD_Bounce(FieldB, FieldA, iW, iH, feedrate, killrate, DistanceFieldBlur);




                        if (i % 100 == 0)
                        {
                            float minR = 1000;
                            float maxR = 0;
                            float minG = 1000;
                            float maxG = 0;
                            for (int y = 0; y < iH; y++)
                            {
                                for (int x = 0; x < iW; x++)
                                {
                                    var F = GetF(FieldA, x, y, iW, iH);
                                    if (F.R > maxR) maxR = F.R;
                                    if (F.R < minR) minR = F.R;
                                    if (F.G > maxG) maxG = F.G;
                                    if (F.G < minG) minG = F.G;
                                }
                            }

                            float scaleR = (float)(255.0f / Math.Max(0.0001f, (maxR - minR)));
                            float scaleG = (float)(255.0f / Math.Max(0.0001f, (maxR - minR)));
                            for (int y = 0; y < iH; y++)
                            {
                                for (int x = 0; x < iW; x++)
                                {
                                    var C = B.GetPixel(x, y);
                                    var C0 = B.GetPixel((x + iW - 1) % iW, y);
                                    var C1 = B.GetPixel((x + 1) % iW, y);
                                    var C2 = B.GetPixel(x, (y - 1 + iH) % iH);
                                    var C3 = B.GetPixel(x, (y + 1) % iH);



                                    var F = GetF(FieldA, x, y, iW, iH);

                                    byte c1 = (byte)(255 - ((F.R - minR) * scaleR));
                                    byte c2 = (byte)((F.G - minG) * scaleG);
                                    //   c1 =(byte)( c1 >= 200 ? 255 : 0);
                                    B2.SetPixel(x, y, Color.FromArgb(c1, c1, c1));

                                }
                            }
                            B2.Save(target + "_" + (img++).ToString() + ".png");

                        }
/*

                        if (false)//i>= 1500 && ((i % 400) == 0 ) && i< 2300)
                        {
                            forceiter++;
                            for (int y = 0; y < iH; y++)
                            {
                                for (int x = 0; x < iW; x++)
                                {

                                    var C = B.GetPixel(x, y);
                                    var C0 = B.GetPixel((x + iW - 1) % iW, y);
                                    var C1 = B.GetPixel((x + 1) % iW, y);
                                    var C2 = B.GetPixel(x, (y - 1 + iH) % iH);
                                    var C3 = B.GetPixel(x, (y + 1) % iH);

                                    var ggG = C.G + C1.G + C2.G + C3.G + C0.G;

                                    var rrR = C.R + C1.R + C2.R + C3.R + C0.R;
                                    //  double Gg = 0;
                                    if (ggG > 200)
                                    {
                                        var F = GetF(FieldA, x, y, iW, iH);

                                        var OG = F.G;
                                        var OR = F.R;

                                        F.G = OG * 0.2f + OR * 0.8f;
                                        F.R = OR * 0.2f + OG * 0.8f; ;
                                        //FieldA[x, y].G -= .002 - (forceiter * 0.0003); ;
                                    }
                                    else
                                    {
                                        if (rrR > 200)
                                        {
                                            var F = GetF(FieldA, x, y, iW, iH);

                                            var OG = F.G;
                                            var OR = F.R;

                                            F.G = OG * 0.1f + OR * 0.9f;
                                            F.R = OR * 0.1f + OG * 0.9f; ;
                                            //FieldA[x, y].G -= .002 - (forceiter * 0.0003); ;
                                        }
                                    }
                                }
                            }
                        }
                        */

                    }




                }

                Console.WriteLine("Converting to gerber..");
                WriteBitmapToGerber(target, Outline, Res, B2, 128);
                Console.WriteLine("Done");
            }

            private static void SetF(RD_Elem[] FieldB, int x, int y, int iW, int iH, float p1, float p2)
            {
                x = (x + iW) % iW;
                y = (y + iH) % iH;
                FieldB[x + y * iW].R = p1;
                FieldB[x + y * iW].G = p2;
            }

            static RD_Elem GetF(RD_Elem[] FieldA, int x, int y, int iW, int iH)
            {
                x = (x + iW) % iW;
                y = (y + iH) % iH;
                return FieldA[x + y * iW];

            }

            private static void ArtGenerator_Flower(string outline, string soldermask, string target, string silk)
            {
                Console.WriteLine("Artlayer {0} started: CheckerField", target);
                

                ParsedGerber Outline = PolyLineSet.LoadGerberFile(outline, true, false, new GerberParserState() { PreCombinePolygons = true});
                Outline.FixPolygonWindings();

                ParsedGerber SolderMask = PolyLineSet.LoadGerberFile(soldermask, false, false, new GerberParserState() {PreCombinePolygons = false, MinimumApertureRadius = 0.1 });
                SolderMask.FixPolygonWindings();

                //Silk.FixPolygonWindings();

                Polygons CombinedOutline = new Polygons();
                Polygons CombinedSoldermask = new Polygons();
                Console.WriteLine("combining outlines..");

                for (int i = 0; i < Outline.OutlineShapes.Count; i++)
                {
                    Polygons clips = new Polygons();

                    clips.Add(Outline.OutlineShapes[i].toPolygon());
                    Clipper cp = new Clipper();
                    cp.AddPolygons(CombinedOutline, PolyType.ptSubject);
                    cp.AddPolygons(clips, PolyType.ptClip);

                    cp.Execute(ClipType.ctXor, CombinedOutline, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
                }
                Console.WriteLine("removing paste curves..");
                if (true)
                    for (int i = 0; i < SolderMask.OutlineShapes.Count; i++)
                    {
                        Polygons clips = new Polygons();

                        clips.Add(SolderMask.OutlineShapes[i].toPolygon());
                        Clipper cp = new Clipper();
                        cp.AddPolygons(CombinedOutline, PolyType.ptSubject);
                        cp.AddPolygons(clips, PolyType.ptClip);

                        cp.Execute(ClipType.ctDifference, CombinedOutline, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
                    }




                if (silk.Length > 0)
                {
                    Console.WriteLine("removing silk curves..");
                    
                    ParsedGerber Silk = PolyLineSet.LoadGerberFile(silk, false, false, new GerberParserState() {PreCombinePolygons = false, MinimumApertureRadius = 0.1 });
                    //Silk.FixPolygonWindings();


                    Polygons CombinedSilk = new Polygons();

                    Console.WriteLine("building big polygon..");
                    {

                        Polygons clips = new Polygons();

                        for (int i = 0; i < Silk.DisplayShapes.Count; i++)
                        {

                            clips.Add(Silk.DisplayShapes[i].toPolygon());
                        }
                        Clipper cp = new Clipper();
                        cp.AddPolygons(CombinedSilk, PolyType.ptSubject);
                        cp.AddPolygons(clips, PolyType.ptClip);

                        cp.Execute(ClipType.ctUnion, CombinedSilk, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
                    }
                    Console.WriteLine("offsetting big polygon..");

                    CombinedSilk = Clipper.OffsetPolygons(CombinedSilk, 1.0 * 100000.0f, JoinType.jtRound, 0.1 * 100000.0f);
                    {
                        Clipper cp = new Clipper();
                        cp.AddPolygons(CombinedOutline, PolyType.ptSubject);
                        cp.AddPolygons(CombinedSilk, PolyType.ptClip);

                        cp.Execute(ClipType.ctDifference, CombinedOutline, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

                    }

                }


                double W = Outline.BoundingBox.BottomRight.X - Outline.BoundingBox.TopLeft.X;
                double H = Outline.BoundingBox.BottomRight.Y - Outline.BoundingBox.TopLeft.Y;
                double Res = 200.0 / 25.4;
                int iW = (int)(W * Res) + 10;
                int iH = (int)(H * Res) + 10;
                int Range = Math.Max(iW, iH);
                Bitmap B = new Bitmap(iW, iH);
                Bitmap B2 = new Bitmap(iW, iH);
                Bitmap B3 = new Bitmap(iW, iH);
                Graphics G2 = Graphics.FromImage(B3);
                G2.Clear(Color.Black);
                double[] DistanceField = new double[iW * iH];
                double[] DistanceFieldBlur = new double[iW * iH];
                double[] AngleField = new double[iW * iH];
                bool DistMode = true;

                for (int i = 0; i < iW * iH; i++) { DistanceField[i] = DistMode ? 0 : 100000; AngleField[i] = 0; };
                Graphics G = Graphics.FromImage(B);
                G.Clear(Color.Black);
                Console.WriteLine("Rendering Base Bitmap");
                // G.SmoothingMode = SmoothingMode.AntiAlias;
                foreach (var a in CombinedOutline)
                {
                    PolyLine P = new PolyLine(PolyLine.PolyIDs.ArtWork);
                    P.fromPolygon(a);
                    List<PointF> Pts = new List<PointF>();
                    foreach (var V in P.Vertices)
                    {
                        Pts.Add(new PointF((float)((V.X - Outline.BoundingBox.TopLeft.X) * Res) + 1, (float)((V.Y - Outline.BoundingBox.TopLeft.Y) * Res) + 1));
                    }
                    G.DrawPolygon(new Pen(Color.White, 1), Pts.ToArray());

                    P.CheckIfHole();
                    if (P.Hole)
                    {
                        G2.FillPolygon(new SolidBrush(Color.Green), Pts.ToArray());
                    }
                    else
                    {
                        G2.FillPolygon(new SolidBrush(Color.Red), Pts.ToArray());

                    }


                }
                B.Save(target + "_renderbase.png");
                Console.WriteLine("Calculating Distance Field");

                {
                    for (int x = 0; x < iW; x++)
                    {

                        for (int y = 0; y < iH; y++)
                        {
                            var C = B.GetPixel(x, y);
                            if (C.R > 10)
                            {
                                for (int xx = Math.Max(x - Range, 0); xx < Math.Min(iW, x + Range); xx++)
                                {
                                    for (int yy = Math.Max(y - Range, 0); yy < Math.Min(iH, y + Range); yy++)
                                    {
                                        if (DistMode)
                                        {
                                            double dist = (xx - x) * (xx - x) + (yy - y) * (yy - y);


                                            DistanceField[xx + yy * iW] += 1.0 / (1.0 + dist * 0.1);
                                        }
                                        else
                                        {
                                            double dist = Math.Sqrt((xx - x) * (xx - x) + (yy - y) * (yy - y));
                                            if (dist < DistanceField[xx + yy * iW]) DistanceField[xx + yy * iW] = dist;



                                        }

                                    }
                                }
                            }
                        }
                    }
                }
                Console.WriteLine("Blurring Distance Field");

                double maxblurval = 0;
                double minblurval = 100000000000;
                {
                    for (int x = 0; x < iW; x++)
                    {

                        for (int y = 0; y < iH; y++)
                        {
                            double T = 0;
                            double amt = 0;
                            for (int xx = 0; xx < 1; xx++)
                            {
                                for (int yy = 0; yy < 1; yy++)
                                {
                                    double D = xx * xx + yy * yy;
                                    double M = Math.Exp(0);
                                    if (D > 0)
                                    {
                                        M = Math.Exp(-(xx * xx) * 0.01) * Math.Exp(-(yy * yy) * 0.01); ;

                                    }
                                    amt += M;
                                    T += DistanceField[((x + xx + iW) % iW) + ((y + yy + iH) % iH) * iW] * M;

                                }
                            }
                            double newval = T / amt;
                            DistanceFieldBlur[x + iW * y] = newval;
                            if (newval > maxblurval) maxblurval = newval;
                            if (newval < minblurval) minblurval = newval;


                        }
                    }
                }
                {
                    G.Clear(Color.Black);
                    for (int x = 0; x < iW; x++)
                    {

                        for (int y = 0; y < iH; y++)
                        {
                            var a = DistanceFieldBlur[x + y * iW];
                            //a = Math.Sqrt(a);

                            byte R = (byte)((a - minblurval) * 255.0 / (maxblurval - minblurval));
                            Color C = Color.FromArgb(R / 2 + 127, R / 2 + 127, R / 2 + 127);
                            DistanceFieldBlur[x + y * iW] = (DistanceFieldBlur[x + y * iW] - minblurval) / (maxblurval - minblurval);
                            B.SetPixel(x, y, C);
                        }
                    }
                    B.Save(target + "_renderDistance.png");
                }

                Console.WriteLine("Calculating Flower Field and Artwork");
                GerberArtWriter GOW = new GerberArtWriter();
                {


                    //   G.Clear(Color.Black);
                    for (int x = 0; x < iW - 1; x++)
                    {

                        for (int y = 0; y < iH - 1; y++)
                        {
                            var a = DistanceFieldBlur[x + y * iW];
                            var b = DistanceFieldBlur[(x + 1) + y * iW];
                            var c = DistanceFieldBlur[x + (y + 1) * iW];

                            V3 AA = new V3(0, 0, Math.Sqrt(a) / (double)Range);
                            V3 BB = new V3(1, 0, Math.Sqrt(b) / (double)Range);
                            V3 CC = new V3(0, 1, Math.Sqrt(c) / (double)Range);

                            var D1 = BB - AA;
                            var D2 = CC - AA;
                            var C1 = V3.Cross(D1, D2);
                            // var DistT = Math.Sqrt(C1.X * C1.X + C1.Y * C1.Y + C1.Z * C1.Z);
                            var AngleRad = Math.Atan2(C1.Y, C1.X);

                            AngleField[x + (y * iW)] = AngleRad;
                        }
                    }
                    //  G.Clear(Color.Black);
                    Graphics GG = Graphics.FromImage(B2);
                    GG.Clear(Color.Gray);

                    FlowerThing FT = new FlowerThing();
                    FT.Build(DistanceFieldBlur, AngleField, iW, iH, CombinedOutline);
                    GG.DrawImage(B, 0, 0);
                    GG.SmoothingMode = SmoothingMode.AntiAlias;
                    GG.CompositingQuality = CompositingQuality.HighQuality;
                    GG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                    foreach (var a in FT.Items)
                    {
                        a.Draw(GG);
                    }

                    B2.Save(target + "_FlowerPrint.png");
                }

                GOW.Write(target);
                Console.WriteLine("Done");
            }

            public class FlowerThing
            {
                public class Item
                {
                    public virtual void Draw(Graphics G)
                    {

                    }
                }
                public class Leaf : Item
                {
                    public V3 Start;
                    public double Length;
                    public double Width;
                    public double Angle;

                    public override void Draw(Graphics G)
                    {
                        V3 begin = new V3(Start.X, Start.Y, 0);
                        V3 end = new V3(begin.X, begin.Y, 0);


                        V3 Anglev = new V3(Math.Sin(Angle), Math.Cos(Angle), 0);
                        V3 sideAnglev = new V3(Math.Sin(Angle + Math.PI / 2), Math.Cos(Angle + Math.PI / 2), 0);
                        begin -= Anglev * Length * 1.2;

                        end -= Anglev * Length * 0.4;


                        List<PointF> Points = new List<PointF>();
                        List<PointF> SymPoints = new List<PointF>();
                        List<V3> LeftSide = new List<V3>();
                        List<V3> RightSide = new List<V3>();

                        for (int i = 0; i < (int)Length; i++)
                        {
                            V3 mid = begin + Anglev * i;
                            V3 left = mid + sideAnglev * Math.Sin(Math.Pow(i / (double)Length, 1.1) * Math.PI) * Length / 5.0;
                            V3 right = mid - sideAnglev * Math.Sin(Math.Pow(i / (double)Length, 1.2) * Math.PI) * Length / 5.0;

                            LeftSide.Add(left);
                            RightSide.Add(right);
                        }

                        RightSide.Reverse();
                        LeftSide.AddRange(RightSide); ;

                        foreach (var P in LeftSide)
                        {
                            Points.Add(new PointF((float)P.X, (float)P.Y));
                        }

                        if (Points.Count > 0)
                        {
                            Pen Pp = new Pen(new SolidBrush(Color.Black), 1);
                            G.FillPolygon(new SolidBrush(Color.White), Points.ToArray());
                            G.DrawPolygon(Pp, Points.ToArray());
                            G.DrawLine(Pp, (float)begin.X, (float)begin.Y, (float)end.X, (float)end.Y);
                            G.DrawLine(Pp, (float)Start.X, (float)Start.Y, (float)end.X, (float)end.Y);
                        }
                    }
                }

                public class Stem : Item
                {
                    public V3 Start;
                    public V3 End;
                    public double Width;
                    public int Mode;
                    public override void Draw(Graphics G)
                    {
                        Color C = Color.Black;
                        Color C2 = Color.Black;
                        switch (Mode)
                        {
                            case 0: C = Color.Orange; break;
                            case 1: C = Color.Red; break;
                            case 2: C = Color.Blue; break;
                            case 3: C = Color.Green; break;
                            case 4: C = Color.Yellow; break;
                            case 6: C = Color.Purple; break;

                        }
                        Pen Pp = new Pen(new SolidBrush(C), (float)Width);
                        Pp.SetLineCap(LineCap.Round, LineCap.Round, DashCap.Round);
                        G.DrawLine(Pp, (float)Start.X, (float)Start.Y, (float)End.X, (float)End.Y);
                    }
                }

                public List<Item> Items = new List<Item>();

                double theWidth = 0;
                double theHeight = 0;
                int iWidth = 0;
                int iHeight = 0;
                double[] DistanceField;
                double[] AngleField;

                public void Build(double[] _DistanceFieldBlur, double[] _AngleField, int iW, int iH, Polygons Outline = null)
                {
                    iWidth = iW;
                    iHeight = iH;
                    AngleField = _AngleField;
                    DistanceField = _DistanceFieldBlur;
                    theWidth = iW;
                    theHeight = iH;
                    for (double x = theWidth * 0.1; x <= theWidth * 0.9; x += theWidth / 5)
                    {
                        for (double y = theHeight * 0.1; y <= theHeight * 0.9; y += theHeight / 5)
                        {

                            if (Outline != null)
                            {
                                bool inside = false;
                                foreach (var a in Outline)
                                {
                                    PolyLine PL = new PolyLine(PolyLine.PolyIDs.ArtWork);
                                    PL.fromPolygon(a);
                                    if (Helpers.IsInPolygon(PL.Vertices, new PointD(x, y))) inside = !inside;

                                } if (inside) Branch(x, y, Math.PI / 2, 80);
                            }
                            else
                            {
                                Branch(x, y, 0, 80);
                            }

                        }
                    }

                }

                Random R = new Random();

                void Branch(double x, double y, double initialdir, double count, int depth = 0)
                {
                    double LeafScale = 1.5;
                    List<Item> Stems = new List<Item>();
                    List<Item> Leafs = new List<Item>();
                    if (depth > 5) return;
                    V3 center = new V3(0, 0, 0);
                    double dir = initialdir;
                    double ddir = 0;

                    double MX = x;
                    double MY = y;
                    for (double i = 0; i < count; i += 0.6)
                    {


                        double W = theWidth;
                        double H = theHeight;
                        double nX = center.X + MX;
                        double nY = center.Y + MY;
                        int mode = 0;
                        double maxmarg = 0.95;
                        double minmarg = 0.05;
                        if (nX < W * minmarg || nX > W * maxmarg || nY < H * minmarg || nY > H * maxmarg)
                        {
                            double Hoek = Math.PI + Math.Atan2(H / 2 - nY, W / 2 - nX);
                            double DHoek = dir - Hoek;
                            //dir = Hoek;

                            //                            ddir = 0;
                            //                          mode = 1;
                            if (DHoek > Math.PI) DHoek -= Math.PI * 2;
                            if (DHoek < -Math.PI) DHoek += Math.PI * 2;
                            if (DHoek > Math.PI / 2) DHoek = Math.PI - DHoek;
                            if (DHoek < -Math.PI / 2) DHoek = -Math.PI - DHoek;



                            if (DHoek > Math.PI || DHoek < -Math.PI)
                            {
                                mode = 1;
                            }
                            else
                            {
                                if (DHoek > 0) ddir = 0.4; else ddir = -0.4;
                                mode = 1;
                            }
                            // curveback!
                        }
                        if (mode == 0 && DistanceField != null)
                        {
                            double Distance = DistanceField[(int)nX + (int)nY * iWidth];
                            if (Distance > .06)
                            {
                                //double Hoek = AngleField[(int)nX + (int)nY * iWidth]-Math.PI/2;
                                double Hoek = Math.PI + AngleField[(int)nX + (int)nY * iWidth];
                                double DHoek = dir - Hoek;
                                if (DHoek > Math.PI) DHoek -= Math.PI * 2;
                                if (DHoek < -Math.PI) DHoek += Math.PI * 2;
                                if (DHoek > Math.PI / 2) DHoek = Math.PI - DHoek;
                                if (DHoek < -Math.PI / 2) DHoek = -Math.PI - DHoek;
                                mode = 4;
                                if (DHoek > 0) ddir = 0.4; else ddir = -0.4;


                                //                                ddir = 0;
                                //                              mode = 4;
                            }

                        }
                        if (mode == 0)
                        {
                            if (R.NextDouble() < 0.2)
                            {
                                double Hoek = -Math.PI / 2;
                                double DHoek = dir - Hoek;
                                if (DHoek < 0) ddir = 0.2; else ddir = -0.2;
                                mode = 2;
                                // curveup!
                            }
                            else
                            {
                                if (R.NextDouble() < 0.2 && AngleField != null)
                                {
                                    double Hoek = AngleField[(int)nX + (int)nY * iWidth];
                                    double DHoek = dir - Hoek;
                                    if (DHoek > Math.PI) DHoek -= Math.PI * 2;
                                    if (DHoek < -Math.PI) DHoek += Math.PI * 2;
                                    if (DHoek > Math.PI / 2) DHoek = Math.PI - DHoek;
                                    if (DHoek < -Math.PI / 2) DHoek = -Math.PI - DHoek;
                                    mode = 3;
                                    if (DHoek > 0) ddir = 0.4; else ddir = -0.4;
                                }
                                else
                                {
                                    mode = 6;
                                    ddir += (R.NextDouble() - 0.5) * 1;
                                }

                            }
                            ddir *= 0.7;
                        }
                        dir += ddir;
                        //        if (dir > Math.PI * 2) dir -= Math.PI;
                        //       if (dir < - Math.PI * 2) dir += Math.PI;
                        double stemlen = 0.2 * (10 + 0.8 * (count - i));
                        if (mode == 1) stemlen *= 0.3;
                        if (mode == 4) stemlen *= 0.4;
                        V3 newcenter = center + (new V3(Math.Cos(dir), Math.Sin(dir), 0)) * stemlen;
                        float branchW = (float)((count - i) * 0.06);

                        Stems.Add(new Stem() { Mode = mode, Start = new V3((float)(MX + newcenter.X), (float)(MY + newcenter.Y), 0), End = new V3((float)(MX + center.X), (float)(MY + center.Y), 0), Width = branchW });


                        center = newcenter;

                        double A1 = -(dir + 0.8) + (R.NextDouble() - 0.5) * 0.5;
                        double A2 = Math.PI - (dir - 0.8) + (R.NextDouble() - 0.5) * 0.5;
                        if (mode != 1 && mode != 4)
                        {
                            if (R.NextDouble() > 0.8)
                            {
                                Leafs.Add(new Leaf() { Start = new V3(MX + center.X, MY + center.Y, 0), Angle = A1, Length = (10 + R.NextDouble() * 10) * LeafScale, Width = 0 });
                            }
                            if (R.NextDouble() > 0.8)
                            {
                                Leafs.Add(new Leaf() { Start = new V3(MX + center.X, MY + center.Y, 0), Angle = A2, Length = (10 + R.NextDouble() * 10) * LeafScale, Width = 0 });
                            }
                            if (R.NextDouble() > 0.999)
                            {
                                double minbranch = 30 * (Math.PI * 2.0 / 360.0);
                                double maxbranch = 30 * (Math.PI * 2.0 / 360.0);
                                double B = (maxbranch - minbranch) * R.NextDouble() + minbranch;
                                if (R.NextDouble() < 0.5) B = -B;
                                Branch(center.X + MX, center.Y + MY, dir + B, (count - i) * 0.8, depth + 1);
                            }
                        }
                        Items.AddRange(Stems);
                        Items.AddRange(Leafs);

                        //  Drawleaf(G, MX - center.X, MY - center.Y, A1, 60 + R.NextDouble() * 20);
                        //  Drawleaf(G, MX - center.X, MY - center.Y, A2, 60 + R.NextDouble() * 20);
                        //  Drawleaf(G, MX - center.X, MY + center.Y, A1, 60 + R.NextDouble() * 20);
                        // Drawleaf(G, MX - center.X, MY + center.Y, A2, 60 + R.NextDouble() * 20);
                    }

                }

            }

            private static void ArtGenerator_CheckerField(string outline, string soldermask, string target, string silk)
            {
                Console.WriteLine("Artlayer {0} started: CheckerField", target);                
                
                ParsedGerber Outline = PolyLineSet.LoadGerberFile(outline, true, false, new GerberParserState() { PreCombinePolygons = true });
                Outline.FixPolygonWindings();

                ParsedGerber SolderMask = PolyLineSet.LoadGerberFile(soldermask, false, false, new GerberParserState() { PreCombinePolygons = false, MinimumApertureRadius = 0.1});
                SolderMask.FixPolygonWindings();

                //Silk.FixPolygonWindings();

                Polygons CombinedOutline = new Polygons();
                Polygons CombinedSoldermask = new Polygons();
                Console.WriteLine("combining outlines..");

                for (int i = 0; i < Outline.OutlineShapes.Count; i++)
                {
                    Polygons clips = new Polygons();

                    clips.Add(Outline.OutlineShapes[i].toPolygon());
                    Clipper cp = new Clipper();
                    cp.AddPolygons(CombinedOutline, PolyType.ptSubject);
                    cp.AddPolygons(clips, PolyType.ptClip);

                    cp.Execute(ClipType.ctXor, CombinedOutline, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
                }
                Console.WriteLine("removing paste curves..");
                if (true)
                    for (int i = 0; i < SolderMask.OutlineShapes.Count; i++)
                    {
                        Polygons clips = new Polygons();

                        clips.Add(SolderMask.OutlineShapes[i].toPolygon());
                        Clipper cp = new Clipper();
                        cp.AddPolygons(CombinedOutline, PolyType.ptSubject);
                        cp.AddPolygons(clips, PolyType.ptClip);

                        cp.Execute(ClipType.ctDifference, CombinedOutline, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
                    }




                if (silk.Length > 0)
                {
                    Console.WriteLine("removing silk curves..");
                    
                    ParsedGerber Silk = PolyLineSet.LoadGerberFile(silk, false, false, new GerberParserState() { PreCombinePolygons = false, MinimumApertureRadius = 0.1 });
                    //Silk.FixPolygonWindings();


                    Polygons CombinedSilk = new Polygons();

                    Console.WriteLine("building big polygon..");
                    {

                        Polygons clips = new Polygons();

                        for (int i = 0; i < Silk.DisplayShapes.Count; i++)
                        {

                            clips.Add(Silk.DisplayShapes[i].toPolygon());
                        }
                        Clipper cp = new Clipper();
                        cp.AddPolygons(CombinedSilk, PolyType.ptSubject);
                        cp.AddPolygons(clips, PolyType.ptClip);

                        cp.Execute(ClipType.ctUnion, CombinedSilk, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
                    }
                    Console.WriteLine("offsetting big polygon..");

                    CombinedSilk = Clipper.OffsetPolygons(CombinedSilk, 1.0 * 100000.0f, JoinType.jtRound, 0.1 * 100000.0f);
                    {
                        Clipper cp = new Clipper();
                        cp.AddPolygons(CombinedOutline, PolyType.ptSubject);
                        cp.AddPolygons(CombinedSilk, PolyType.ptClip);

                        cp.Execute(ClipType.ctDifference, CombinedOutline, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

                    }

                }


                double W = Outline.BoundingBox.BottomRight.X - Outline.BoundingBox.TopLeft.X;
                double H = Outline.BoundingBox.BottomRight.Y - Outline.BoundingBox.TopLeft.Y;
                double Res = 200.0 / 25.4;
                int iW = (int)(W * Res) + 2;
                int iH = (int)(H * Res) + 2;
                int Range = Math.Max(iW, iH);
                Bitmap B = new Bitmap(iW, iH);
                Bitmap B2 = new Bitmap(iW, iH);
                Bitmap B3 = new Bitmap(iW, iH);
                Graphics G2 = Graphics.FromImage(B3);
                G2.Clear(Color.Black);
                double[] DistanceField = new double[iW * iH];
                double[] DistanceFieldBlur = new double[iW * iH];
                double[] AngleField = new double[iW * iH];
                bool DistMode = false;

                for (int i = 0; i < iW * iH; i++) { DistanceField[i] = DistMode ? 0 : 100000; AngleField[i] = 0; };
                Graphics G = Graphics.FromImage(B);
                G.Clear(Color.Black);
                Console.WriteLine("Rendering Base Bitmap");
                // G.SmoothingMode = SmoothingMode.AntiAlias;
                foreach (var a in CombinedOutline)
                {
                    PolyLine P = new PolyLine(PolyLine.PolyIDs.ArtWork);
                    P.fromPolygon(a);
                    List<PointF> Pts = new List<PointF>();
                    foreach (var V in P.Vertices)
                    {
                        Pts.Add(new PointF((float)((V.X - Outline.BoundingBox.TopLeft.X) * Res) + 1, (float)((V.Y - Outline.BoundingBox.TopLeft.Y) * Res) + 1));
                    }
                    G.DrawPolygon(new Pen(Color.White, 1), Pts.ToArray());

                    P.CheckIfHole();
                    if (P.Hole)
                    {
                        G2.FillPolygon(new SolidBrush(Color.Green), Pts.ToArray());
                    }
                    else
                    {
                        G2.FillPolygon(new SolidBrush(Color.Red), Pts.ToArray());

                    }


                }
                B.Save(target + "_renderbase.png");
                Console.WriteLine("Calculating Distance Field");

                {
                    for (int x = 0; x < iW; x++)
                    {

                        for (int y = 0; y < iH; y++)
                        {
                            var C = B.GetPixel(x, y);
                            if (C.R > 10)
                            {
                                for (int xx = Math.Max(x - Range, 0); xx < Math.Min(iW, x + Range); xx++)
                                {
                                    for (int yy = Math.Max(y - Range, 0); yy < Math.Min(iH, y + Range); yy++)
                                    {
                                        if (DistMode)
                                        {
                                            double dist = (xx - x) * (xx - x) + (yy - y) * (yy - y);


                                            DistanceField[xx + yy * iW] += 1.0 / (1.0 + dist * 0.1);
                                        }
                                        else
                                        {
                                            double dist = Math.Sqrt((xx - x) * (xx - x) + (yy - y) * (yy - y));
                                            if (dist < DistanceField[xx + yy * iW]) DistanceField[xx + yy * iW] = dist;



                                        }

                                    }
                                }
                            }
                        }
                    }
                }
                Console.WriteLine("Blurring Distance Field");

                double maxblurval = 0;
                double minblurval = 100000000000;
                {
                    for (int x = 0; x < iW; x++)
                    {

                        for (int y = 0; y < iH; y++)
                        {
                            double T = 0;
                            double amt = 0;
                            for (int xx = 0; xx < 1; xx++)
                            {
                                for (int yy = 0; yy < 1; yy++)
                                {
                                    double D = xx * xx + yy * yy;
                                    double M = Math.Exp(0);
                                    if (D > 0)
                                    {
                                        M = Math.Exp(-(xx * xx) * 0.01) * Math.Exp(-(yy * yy) * 0.01); ;

                                    }
                                    amt += M;
                                    T += DistanceField[((x + xx + iW) % iW) + ((y + yy + iH) % iH) * iW] * M;

                                }
                            }
                            double newval = T / amt;
                            DistanceFieldBlur[x + iW * y] = newval;
                            if (newval > maxblurval) maxblurval = newval;
                            if (newval < minblurval) minblurval = newval;


                        }
                    }
                }
                {
                    G.Clear(Color.Black);
                    for (int x = 0; x < iW; x++)
                    {

                        for (int y = 0; y < iH; y++)
                        {
                            var a = DistanceFieldBlur[x + y * iW];
                            //a = Math.Sqrt(a);
                            Color C = Color.FromArgb((byte)((a - minblurval) * 255.0 / (maxblurval - minblurval)), 0, 0);
                            B.SetPixel(x, y, C);
                        }
                    }
                    B.Save(target + "_renderDistance.png");
                }
                Console.WriteLine("Calculating Angle Field and Artwork");

                {
                    G.Clear(Color.Black);
                    for (int x = 0; x < iW - 1; x++)
                    {

                        for (int y = 0; y < iH - 1; y++)
                        {
                            var a = DistanceFieldBlur[x + y * iW];
                            var b = DistanceFieldBlur[(x + 1) + y * iW];
                            var c = DistanceFieldBlur[x + (y + 1) * iW];

                            V3 AA = new V3(0, 0, Math.Sqrt(a) / (double)Range);
                            V3 BB = new V3(1, 0, Math.Sqrt(b) / (double)Range);
                            V3 CC = new V3(0, 1, Math.Sqrt(c) / (double)Range);

                            var D1 = BB - AA;
                            var D2 = CC - AA;
                            var C1 = V3.Cross(D1, D2);

                            var AngleRad = Math.Atan2(C1.Y, C1.X);
                            var AngleDeg = AngleRad * 360.0 / (Math.PI * 2);
                            double S = Math.Sin(AngleRad * 7.0 + 0.2);

                            while (AngleDeg < 0) AngleDeg += 360;
                            byte vv = (byte)((S > 0.30) ? 255 : 0);

                            Color C = Color.FromArgb(255 - Math.Min(255, (int)(AngleDeg * (255.0 / 360.0))), 0, 0);
                            if (B3.GetPixel(x, y).R == 255)
                            {
                                Color C2 = Color.FromArgb(vv, vv, vv);
                                B2.SetPixel(x, y, C2);
                            }
                            else
                            {
                                B2.SetPixel(x, y, Color.Black);

                            }
                            B.SetPixel(x, y, C);
                        }
                    }

                    B.Save(target + "_renderAngle.png");
                    B2.Save(target + "_artwork.png");
                }

                Console.WriteLine("Converting to gerber..");
                WriteBitmapToGerber(target, Outline, Res, B2);
                Console.WriteLine("Done");


            }

            public static void WriteBitmapToGerber(string target, ParsedGerber Outline, double ResX, Bitmap B2, int threshold = 0, bool fittooutline = true, bool cliptooutline = false)
            {
                int iW = B2.Width;
                int iH = B2.Height;
                double ResY = ResX;

                if (fittooutline)
                {
                    ResX = 1.0 / ((Outline.BoundingBox.BottomRight.X - Outline.BoundingBox.TopLeft.X) / (B2.Width - 1));
                    ResY = 1.0 / ((Outline.BoundingBox.BottomRight.Y - Outline.BoundingBox.TopLeft.Y) / (B2.Height - 1.0));
                }
                else{
                    if (cliptooutline) {
                        var w = Outline.BoundingBox.Width();
                        var h = Outline.BoundingBox.Height();

                        iW = Math.Min(iW, (int)Math.Ceiling(w * ResX));
                        iH = Math.Min(iH, (int)Math.Ceiling(h * ResY));
                    }
                }
                bool invert = false;
                GerberArtWriter GOW = new GerberArtWriter();
                if (threshold < 0)
                {
                    threshold = -threshold;
                    invert = true;
                }
                int startlevel = 0;
                int endlevel;
                for (int y = 0; y < iH; y++)
                {
                    double sx = -1;
                    bool active = false;
                    for (int x = 0; x < iW; x++)
                    {
                        Color C = B2.GetPixel(x, y);
                        if ((invert == false && C.R > threshold) || (invert == true && C.R < threshold))
                        {
                            if (active == false)
                            {
                                sx = x;
                                startlevel = C.R;
                                active = true;
                            }
                        }
                        else
                        {
                            if (active)
                            {
                                endlevel = C.R;
                                active = false;
                                PolyLine pL = new PolyLine(PolyLine.PolyIDs.Bitmap);

                                double offL = ((startlevel - Math.Abs(threshold)) / 128.0) * 1.0 / ResX;
                                double offR = ((endlevel - Math.Abs(threshold)) / 128.0) * 1.0 / ResX;
                                //  offL *= -1;
                                //     offR *= -1;
                                pL.Add(sx / ResX + offL, ((double)y) / ResY);
                                pL.Add(((double)x) / ResX - offR, ((double)y) / ResY);
                                if (Outline != null) pL.Translate(Outline.BoundingBox.TopLeft.X, Outline.BoundingBox.TopLeft.Y);

                                pL.Translate(0, .50 / ResY);

                                GOW.AddPolyLine(pL, 1.0 / ResY);
                            }
                        }
                    }
                    if (active)
                    {
                        PolyLine pL = new PolyLine(PolyLine.PolyIDs.Bitmap);
                        double offL = ((startlevel - Math.Abs(threshold)) / 128.0) * .50 / ResX;

                        pL.Add(sx / ResX + offL, ((double)y) / ResY);
                        pL.Add((iW) / ResX, ((double)y) / ResY);
                        if (Outline != null) pL.Translate(Outline.BoundingBox.TopLeft.X, Outline.BoundingBox.TopLeft.Y);
                        pL.Translate(0, .50 / ResY);
                        GOW.AddPolyLine(pL, 1.0 / ResY);
                    }

                }

                GOW.Write(target, .50 / ResX, 1.0 / ResY);
            }


            private static void ArtGenerator_FlowField(string outline, string soldermask, string target, string silk)
            {
                Console.WriteLine("Artlayer {0} started: CheckerField", target);
                
                ParsedGerber Outline = PolyLineSet.LoadGerberFile(outline, true, false, new GerberParserState() { PreCombinePolygons = true});
                Outline.FixPolygonWindings();

                ParsedGerber SolderMask = PolyLineSet.LoadGerberFile(soldermask, false, false, new GerberParserState() {PreCombinePolygons = false, MinimumApertureRadius = 0.1 });
                SolderMask.FixPolygonWindings();

                //Silk.FixPolygonWindings();

                Polygons CombinedOutline = new Polygons();
                Polygons CombinedSoldermask = new Polygons();
                Console.WriteLine("combining outlines..");

                for (int i = 0; i < Outline.OutlineShapes.Count; i++)
                {
                    Polygons clips = new Polygons();

                    clips.Add(Outline.OutlineShapes[i].toPolygon());
                    Clipper cp = new Clipper();
                    cp.AddPolygons(CombinedOutline, PolyType.ptSubject);
                    cp.AddPolygons(clips, PolyType.ptClip);

                    cp.Execute(ClipType.ctXor, CombinedOutline, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
                }
                Console.WriteLine("removing paste curves..");
                if (true)
                    for (int i = 0; i < SolderMask.OutlineShapes.Count; i++)
                    {
                        Polygons clips = new Polygons();

                        clips.Add(SolderMask.OutlineShapes[i].toPolygon());
                        Clipper cp = new Clipper();
                        cp.AddPolygons(CombinedOutline, PolyType.ptSubject);
                        cp.AddPolygons(clips, PolyType.ptClip);

                        cp.Execute(ClipType.ctDifference, CombinedOutline, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
                    }




                if (silk.Length > 0)
                {
                    Console.WriteLine("removing silk curves..");
                    
                    ParsedGerber Silk = PolyLineSet.LoadGerberFile(silk, false, false, new GerberParserState() { PreCombinePolygons = false, MinimumApertureRadius = 0.1 });
                    //Silk.FixPolygonWindings();


                    Polygons CombinedSilk = new Polygons();

                    Console.WriteLine("building big polygon..");
                    {

                        Polygons clips = new Polygons();

                        for (int i = 0; i < Silk.DisplayShapes.Count; i++)
                        {

                            clips.Add(Silk.DisplayShapes[i].toPolygon());
                        }
                        Clipper cp = new Clipper();
                        cp.AddPolygons(CombinedSilk, PolyType.ptSubject);
                        cp.AddPolygons(clips, PolyType.ptClip);

                        cp.Execute(ClipType.ctUnion, CombinedSilk, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
                    }
                    Console.WriteLine("offsetting big polygon..");

                    CombinedSilk = Clipper.OffsetPolygons(CombinedSilk, 1.0 * 100000.0f, JoinType.jtRound, 0.1 * 100000.0f);
                    {
                        Clipper cp = new Clipper();
                        cp.AddPolygons(CombinedOutline, PolyType.ptSubject);
                        cp.AddPolygons(CombinedSilk, PolyType.ptClip);

                        cp.Execute(ClipType.ctDifference, CombinedOutline, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

                    }

                }


                double W = Outline.BoundingBox.BottomRight.X - Outline.BoundingBox.TopLeft.X;
                double H = Outline.BoundingBox.BottomRight.Y - Outline.BoundingBox.TopLeft.Y;
                double Res = 200.0 / 25.4;
                int iW = (int)(W * Res) + 2;
                int iH = (int)(H * Res) + 2;
                int Range = Math.Max(iW, iH);
                Bitmap B = new Bitmap(iW, iH);
                Bitmap B2 = new Bitmap(iW, iH);
                Bitmap B3 = new Bitmap(iW, iH);
                Graphics G2 = Graphics.FromImage(B3);
                G2.Clear(Color.Black);
                double[] DistanceField = new double[iW * iH];
                double[] DistanceFieldBlur = new double[iW * iH];
                double[] AngleField = new double[iW * iH];
                bool DistMode = true;

                for (int i = 0; i < iW * iH; i++) { DistanceField[i] = DistMode ? 0 : 100000; AngleField[i] = 0; };
                Graphics G = Graphics.FromImage(B);
                G.Clear(Color.Black);
                Console.WriteLine("Rendering Base Bitmap");
                // G.SmoothingMode = SmoothingMode.AntiAlias;
                foreach (var a in CombinedOutline)
                {
                    PolyLine P = new PolyLine(PolyLine.PolyIDs.ArtWork);
                    P.fromPolygon(a);
                    List<PointF> Pts = new List<PointF>();
                    foreach (var V in P.Vertices)
                    {
                        Pts.Add(new PointF((float)((V.X - Outline.BoundingBox.TopLeft.X) * Res) + 1, (float)((V.Y - Outline.BoundingBox.TopLeft.Y) * Res) + 1));
                    }
                    G.DrawPolygon(new Pen(Color.White, 1), Pts.ToArray());

                    P.CheckIfHole();
                    if (P.Hole)
                    {
                        G2.FillPolygon(new SolidBrush(Color.Green), Pts.ToArray());
                    }
                    else
                    {
                        G2.FillPolygon(new SolidBrush(Color.Red), Pts.ToArray());

                    }


                }
                B.Save(target + "_renderbase.png");
                Console.WriteLine("Calculating Distance Field");

                {
                    for (int x = 0; x < iW; x++)
                    {

                        for (int y = 0; y < iH; y++)
                        {
                            var C = B.GetPixel(x, y);
                            if (C.R > 10)
                            {
                                for (int xx = Math.Max(x - Range, 0); xx < Math.Min(iW, x + Range); xx++)
                                {
                                    for (int yy = Math.Max(y - Range, 0); yy < Math.Min(iH, y + Range); yy++)
                                    {
                                        if (DistMode)
                                        {
                                            double dist = (xx - x) * (xx - x) + (yy - y) * (yy - y);


                                            DistanceField[xx + yy * iW] += 1.0 / (1.0 + dist * 0.1);
                                        }
                                        else
                                        {
                                            double dist = Math.Sqrt((xx - x) * (xx - x) + (yy - y) * (yy - y));
                                            if (dist < DistanceField[xx + yy * iW]) DistanceField[xx + yy * iW] = dist;



                                        }

                                    }
                                }
                            }
                        }
                    }
                }
                Console.WriteLine("Blurring Distance Field");

                double maxblurval = 0;
                double minblurval = 100000000000;
                {
                    for (int x = 0; x < iW; x++)
                    {

                        for (int y = 0; y < iH; y++)
                        {
                            double T = 0;
                            double amt = 0;
                            for (int xx = 0; xx < 1; xx++)
                            {
                                for (int yy = 0; yy < 1; yy++)
                                {
                                    double D = xx * xx + yy * yy;
                                    double M = Math.Exp(0);
                                    if (D > 0)
                                    {
                                        M = Math.Exp(-(xx * xx) * 0.01) * Math.Exp(-(yy * yy) * 0.01); ;

                                    }
                                    amt += M;
                                    T += DistanceField[((x + xx + iW) % iW) + ((y + yy + iH) % iH) * iW] * M;

                                }
                            }
                            double newval = T / amt;
                            DistanceFieldBlur[x + iW * y] = newval;
                            if (newval > maxblurval) maxblurval = newval;
                            if (newval < minblurval) minblurval = newval;


                        }
                    }
                }
                {
                    G.Clear(Color.Black);
                    for (int x = 0; x < iW; x++)
                    {

                        for (int y = 0; y < iH; y++)
                        {
                            var a = DistanceFieldBlur[x + y * iW];
                            //a = Math.Sqrt(a);
                            Color C = Color.FromArgb((byte)((a - minblurval) * 255.0 / (maxblurval - minblurval)), 0, 0);
                            DistanceFieldBlur[x + y * iW] = (DistanceFieldBlur[x + y * iW] - minblurval) / (maxblurval - minblurval);
                            B.SetPixel(x, y, C);
                        }
                    }
                    B.Save(target + "_renderDistance.png");
                }
                Console.WriteLine("Calculating Angle Field and Artwork");
                GerberArtWriter GOW = new GerberArtWriter();

                {
                    G.Clear(Color.Black);
                    for (int x = 0; x < iW - 1; x += (int)(Res))
                    {

                        for (int y = 0; y < iH - 1; y += (int)(Res))
                        {
                            var a = DistanceFieldBlur[x + y * iW];
                            var b = DistanceFieldBlur[(x + 1) + y * iW];
                            var c = DistanceFieldBlur[x + (y + 1) * iW];

                            V3 AA = new V3(0, 0, Math.Sqrt(a) / (double)Range);
                            V3 BB = new V3(1, 0, Math.Sqrt(b) / (double)Range);
                            V3 CC = new V3(0, 1, Math.Sqrt(c) / (double)Range);

                            var D1 = BB - AA;
                            var D2 = CC - AA;
                            var C1 = V3.Cross(D1, D2);
                            var DistT = Math.Sqrt(C1.X * C1.X + C1.Y * C1.Y + C1.Z * C1.Z);
                            var AngleRad = Math.Atan2(C1.Y, C1.X);

                            var AngleDeg = AngleRad * 360.0 / (Math.PI * 2);

                            //     C1.X /= DistT;
                            //   C1.Y /= DistT;
                            var Dist = Math.Sqrt(C1.X * C1.X + C1.Y * C1.Y);

                            while (AngleDeg < 0) AngleDeg += 360;

                            Color C = Color.FromArgb(255 - Math.Min(255, (int)(AngleDeg * (255.0 / 360.0))), 0, 0);
                            if (B3.GetPixel(x, y).R == 255)
                            {

                                PolyLine pL = new PolyLine(PolyLine.PolyIDs.ArtWork);
                                //pL.Add((x-1) / Res, (y - 1) / Res);
                                double cx = Math.Sin(-AngleRad) * Res * 0.4 * 0.8 * (1 + Math.Pow(Dist, 0.1));
                                double cy = Math.Cos(-AngleRad) * Res * 0.4 * 0.8 * (1 + Math.Pow(Dist, 0.1));
                                pL.Add(((x - 1) + cx) / Res, ((y - 1) + cy) / Res);
                                pL.Add(((x - 1) - cx) / Res, ((y - 1) - cy) / Res);
                                pL.Translate(Outline.BoundingBox.TopLeft.X, Outline.BoundingBox.TopLeft.Y);

                                GOW.AddPolyLine(pL, 1.0 / Res);
                            }
                            else
                            {

                            }
                        }
                    }

                }



                GOW.Write(target);
                Console.WriteLine("Done");


            }

            private static void ArtGenerator_OffsetCurves(string outline, string soldermask, string target, string silk)
            {
                Console.WriteLine("Artlayer {0} started: OffsetCurves", target);
                
                ParsedGerber Outline = PolyLineSet.LoadGerberFile(outline, true, false, new GerberParserState() { PreCombinePolygons = true });
                Outline.FixPolygonWindings();

                ParsedGerber SolderMask = PolyLineSet.LoadGerberFile(soldermask, false, false, new GerberParserState() {PreCombinePolygons = false, MinimumApertureRadius = 0.1 });
                SolderMask.FixPolygonWindings();

                //Silk.FixPolygonWindings();

                Polygons CombinedOutline = new Polygons();
                Polygons CombinedSoldermask = new Polygons();
                Console.WriteLine("combining outlines..");

                for (int i = 0; i < Outline.OutlineShapes.Count; i++)
                {
                    Polygons clips = new Polygons();

                    clips.Add(Outline.OutlineShapes[i].toPolygon());
                    Clipper cp = new Clipper();
                    cp.AddPolygons(CombinedOutline, PolyType.ptSubject);
                    cp.AddPolygons(clips, PolyType.ptClip);

                    cp.Execute(ClipType.ctXor, CombinedOutline, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
                }
                Console.WriteLine("removing paste curves..");
                if (true)
                {
                    Polygons clips = new Polygons();
                    for (int i = 0; i < SolderMask.OutlineShapes.Count; i++)
                    {

                        clips.Add(SolderMask.OutlineShapes[i].toPolygon());
                    }
                    Clipper cp = new Clipper();
                    cp.AddPolygons(CombinedOutline, PolyType.ptSubject);
                    cp.AddPolygons(clips, PolyType.ptClip);

                    cp.Execute(ClipType.ctDifference, CombinedOutline, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
                }

                if (silk.Length > 0)
                {
                    Console.WriteLine("removing silk curves..");
                    
                    ParsedGerber Silk = PolyLineSet.LoadGerberFile(silk, false, false, new GerberParserState() {PreCombinePolygons = false, MinimumApertureRadius =0.1 });
                    //Silk.FixPolygonWindings();


                    Polygons CombinedSilk = new Polygons();

                    Console.WriteLine("building big polygon..");
                    {

                        Polygons clips = new Polygons();

                        for (int i = 0; i < Silk.DisplayShapes.Count; i++)
                        {

                            clips.Add(Silk.DisplayShapes[i].toPolygon());
                        }
                        Clipper cp = new Clipper();
                        cp.AddPolygons(CombinedSilk, PolyType.ptSubject);
                        cp.AddPolygons(clips, PolyType.ptClip);

                        cp.Execute(ClipType.ctUnion, CombinedSilk, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
                    }
                    Console.WriteLine("offsetting big polygon..");

                    CombinedSilk = Clipper.OffsetPolygons(CombinedSilk, 0.5 * 100000.0f, JoinType.jtRound, 0.1 * 100000.0f);
                    {
                        Clipper cp = new Clipper();
                        cp.AddPolygons(CombinedOutline, PolyType.ptSubject);
                        cp.AddPolygons(CombinedSilk, PolyType.ptClip);

                        cp.Execute(ClipType.ctDifference, CombinedOutline, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

                    }

                }

                Console.WriteLine("starting offsetcurves..");
                GerberArtWriter GOW = new GerberArtWriter();
                double add = 0.5;
                for (double i = 0.5; i < 15; i += add)
                {

                    //    if (add < (0.25)) add = (0.25 );
                    Console.WriteLine("offset {0}", i);

                    CombinedOutline = Clipper.OffsetPolygons(CombinedOutline, -add * 100000.0f, JoinType.jtRound, 0.1 * 100000.0f);
                    foreach (var p in CombinedOutline)
                    {
                        PolyLine P = new PolyLine(PolyLine.PolyIDs.ArtWork);
                        P.fromPolygon(p);
                        GOW.AddPolyLine(P, add / 4);
                    }
                    add += 0.1;

                }
                GOW.Write(target);
            }

        }

        [Serializable]

        public class TextLabel
        {
            public string Text = "Empty";
            public double FontSize = 2.54;
            public string Font = "Arial";
            public StringAlign Alignment = StringAlign.BottomLeft;
            public BoxAttachment Attachment = BoxAttachment.TopLeft;
            public double Rotation = 0;
            public XY Position = new XY();
            public double StrokeWidth = 0.05;
        }

        [Serializable]
        public class FontDef
        {
            public string Name = "font";
            public string FontFile = "font.xml";
        }

        [Serializable]
        public class GerberDef {
            public string FileName = "";
            public BoxAttachment Attachment = BoxAttachment.TopLeft;
            public XY Position = new XY();
        }

        [Serializable]
        public class ArtSet
        {
            public List<TextLabel> Labels = new List<TextLabel>();
            public List<Graphic> Graphics = new List<Graphic>();
            public List<FontDef> Fonts = new List<FontDef>();
            public List<GerberDef> GerberRenders = new List<GerberDef>();
            public void Write(string p)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ArtSet));
                TextWriter writer = new StreamWriter(p);
                serializer.Serialize(writer, this);
                writer.Close();
            }


            public static ArtSet Load(string p)
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ArtSet));

                    // Declare an object variable of the type to be deserialized.
                    ArtSet i;

                    using (Stream reader = new FileStream(p, FileMode.Open))
                    {
                        // Call the Deserialize method to restore the object's state.
                        i = (ArtSet)serializer.Deserialize(reader);

                    }
                    return i;
                }
                catch (Exception)
                {

                }
                return new ArtSet();
            }

            public Dictionary<string, FontSet> LoadFonts()
            {
                Dictionary<string, FontSet> R = new Dictionary<string, FontSet>();
                foreach (var a in Fonts)
                {
                    FontSet F = null;
                    try
                    {
                        F = FontSet.Load(a.FontFile);
                        R[a.Name.ToLower()] = F;
                    }
                    catch (Exception E)
                    {
                        Console.WriteLine("Font file failed to load: {0} {1}: {2}", a.Name, a.FontFile, E.Message);
                    }
                }
                return R;

            }
        }

        [Serializable]
        public class Graphic
        {
            //StringAlign Alignment = StringAlign.BottomLeft;
            //BoxAttachment Attachment = BoxAttachment.TopLeft;
            public double Rotation = 0;
            public XY Position = new XY();
        }
    }

    public class GerberArtWriter
    {
        Dictionary<double, List<PolyLine>> PolyLines = new Dictionary<double, List<PolyLine>>();
        Dictionary<double, int> ApertureLookup = new Dictionary<double, int>();
        List<PolyLine> Polygons = new List<PolyLine>();
        int LastID = 0;
        public void AddPolyLine(PolyLine a, double width = 1)
        {
      
            if (PolyLines.ContainsKey(width) == false)
            {
                PolyLines[width] = new List<PolyLine>();
            }
            PolyLines[width].Add(a);
        }

        public void AddFlash(PointD P, double width = 1)
        {
            PolyLine PL = new PolyLine(LastID++);
            PL.Vertices.Add(P);
            if (PolyLines.ContainsKey(width) == false)
            {
                PolyLines[width] = new List<PolyLine>();
            }
            PolyLines[width].Add(PL);
        }

        public void Write(string p, double rectx = -1, double recty = -1)
        {
            List<string> lines = new List<string>();

            GerberNumberFormat GNF = new GerberNumberFormat();
            GNF.DigitsBefore = 4;
            GNF.DigitsAfter = 4;
            GNF.SetImperialMode();

            lines.Add(Gerber.INCH);
            lines.Add("%OFA0B0*%");
            lines.Add(GNF.BuildGerberFormatLine());
            lines.Add("%IPPOS*%");
            lines.Add("%LPD*%");

            List<GerberApertureType> Apts = new List<GerberApertureType>();

            int aptindex = 0;
            foreach (var a in PolyLines.Keys)
            {

                GerberApertureType Apt = new GerberApertureType();
                if (rectx > 0)
                {
                    Apt.SetRectangle(rectx, recty,0);
                }
                else
                {
                    Apt.SetCircle(a);
                }
                Apt.ID = aptindex + 10;
                lines.Add(Apt.BuildGerber(GNF));
                Apts.Add(Apt);
                ApertureLookup[a] = aptindex;
                aptindex++;
            }

            foreach (var a in PolyLines)
            {
                lines.Add(Apts[ApertureLookup[a.Key]].BuildSelectGerber());
                foreach (var b in a.Value)
                {
                    if (b.Vertices.Count == 1)
                    {
                        lines.Add(Gerber.Flash(b.Vertices[0], GNF));
                    }
                    else
                    {
                        lines.Add(Gerber.MoveTo(b.Vertices[0], GNF));
                        for (int i = 1; i < b.Vertices.Count; i++)
                        {
                            lines.Add(Gerber.LineTo(b.Vertices[i], GNF));
                        }
                    }
                }
            }

            foreach (var a in Polygons)
            {
                if (a.Vertices.Count > 2)
                {
                    lines.Add(Gerber.StartRegion);
                    lines.Add(Gerber.MoveTo(a.Vertices[0], GNF));
                    lines.Add(Gerber.LinearInterpolation);
                    for (int i = 1; i < a.Vertices.Count; i++)
                    {
                        lines.Add(Gerber.LineTo(a.Vertices[i], GNF));
                    }
                    lines.Add(Gerber.StopRegion);
                }
            }

            lines.Add(Gerber.EOF);
            Gerber.WriteAllLines(p, lines);
        }

        public void DrawString(PointD Start, FontSet FS, string text, double size, double strokewidth, StringAlign SA, bool Reverse = false, double rotation = 0, bool Polygonized = false)
        {

            //text = "Test1'1234'0+!@\"#$@%#&*(";

            if (FS == null)
            {
                Console.WriteLine("DrawString called with no active fontset (\"{0}\")! Ignoring!", text);
                return;
            }
            //double x = Start.X;
            //double y = Start.Y;
            double x = 0;
            double y = -size;
            List<PolyLine> PreRotateTranslate = new List<PolyLine>();


            double Scaler = size / FS.CapsHeight;
            //  y += size;


            double W = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char t = text[i];
                var R = FS.GetChar(t);
                if (R != null)
                {
                    W += R.Advance * Scaler;
                }
                else
                {
                    W += size;
                }
            }
            if (Reverse)
            {
                //x += W;
                //W = -W;
            }

            switch (SA)
            {
                case StringAlign.BottomCenter:
                    if (Reverse) { x += W / 2; } else { x -= W / 2; };

                    break;
                case StringAlign.TopCenter:
                    y -= size;

                    if (Reverse) { x += W / 2; } else { x -= W / 2; };
                    break;
                case StringAlign.CenterCenter:
                    y -= size / 2;
                    if (Reverse) { x += W / 2; } else { x -= W / 2; };
                    break;

                case StringAlign.BottomRight:
                    y += size;
                    if (Reverse) { x += W; } else { x -= W; };
                    break;
                case StringAlign.TopRight:
                    if (Reverse) { x += W; } else { x -= W; };
                    break;
                case StringAlign.CenterRight:
                    y -= size / 2;
                    if (Reverse) { x += W; } else { x -= W; };
                    break;
                case StringAlign.BottomLeft:
                    y += size;
                    break;
                case StringAlign.TopLeft:
                    break;
                case StringAlign.CenterLeft:
                    y -= size / 2;
                    break;
            }

            for (int i = 0; i < text.Length; i++)
            {
                char t = text[i];
                var R = FS.GetChar(t);
                if (R != null)
                {
                    foreach (var l in R.lines)
                    {
                        PolyLine PL = new PolyLine(LastID++);
                        foreach (var v in l)
                        {
                            if (Reverse)
                            {
                                PL.Add(-v.X * Scaler + x, -(v.Y * Scaler) + y);
                            }
                            else
                            {
                                PL.Add(v.X * Scaler + x, -(v.Y * Scaler) + y);
                            }
                        }
                        PL.Close();
                        PreRotateTranslate.Add(PL);

                    }
                    if (Reverse)
                    {

                        x -= Scaler * R.Advance;
                    }
                    else
                    {
                        x += Scaler * R.Advance;

                    }
                }
                else
                {
                    if (Reverse)
                    {
                        x -= size;
                    }
                    else
                    {
                        x += size;
                    }
                }
            }


            double SX = Math.Sin(rotation * (Math.PI / 180.0));
            double CX = Math.Cos(rotation * (Math.PI / 180.0));

            foreach (var a in PreRotateTranslate)
            {
                PolyLine PL = new PolyLine(LastID++);
                foreach (var v in a.Vertices)
                {


                    double xx = CX * v.X - SX * v.Y;
                    double yy = SX * v.X + CX * v.Y;
                    PL.Add(xx + Start.X, yy + Start.Y);
                }
                if (Polygonized)
                {
                    AddPolygon(PL);
                }
                else
                {
                    AddPolyLine(PL, strokewidth);
                }
            }
        }

        public static PointD AttachPoint(PointD inp, PointD boxtopleft, PointD boxbottomright, GerberLibrary.ArtWork.BoxAttachment attachment)
        {
            PointD ret = new PointD(inp.X, inp.Y);
            switch (attachment)
            {
                case ArtWork.BoxAttachment.Top:
                    ret.Y = boxbottomright.Y - inp.Y;
                    break;
                case ArtWork.BoxAttachment.Bottom:
                    ret.Y = boxtopleft.Y + inp.Y;
                    break;
                case ArtWork.BoxAttachment.Left:
                    ret.X = boxtopleft.X + inp.X;
                    break;
                case ArtWork.BoxAttachment.Right:
                    ret.X = boxbottomright.X - inp.X;
                    break;

                case ArtWork.BoxAttachment.TopLeft:
                    ret.Y = boxbottomright.Y - inp.Y;
                    ret.X = boxtopleft.X + inp.X;
                    break;
                case ArtWork.BoxAttachment.BottomLeft:
                    ret.Y = boxtopleft.Y + inp.Y;
                    ret.X = boxtopleft.X + inp.X;

                    break;
                case ArtWork.BoxAttachment.TopRight:
                    ret.Y = boxbottomright.Y - inp.Y;
                    ret.X = boxbottomright.X - inp.X;
                    break;
                case ArtWork.BoxAttachment.BottomRight:
                    ret.Y = boxtopleft.Y + inp.Y;
                    ret.X = boxbottomright.X - inp.X;
                    break;
            }
            return ret;
        }

        Dictionary<string, FontSet> LocalFonts = new Dictionary<string, FontSet>();


        public void DrawBoardLabelSet(Dictionary<string, FontSet> FS, GerberLibrary.ArtWork.ArtSet AS, PointD boxtopleft, PointD boxbottomright)
        {
            Console.WriteLine("Writing artwork: dimension: {0} -> {1}", boxtopleft, boxbottomright);
            //if (FS.Count == 0) return;
            Console.WriteLine("Number of fonts: {0}", AS.Fonts.Count());
            foreach(var F in AS.Fonts)
            {
                try
                {
                    FontSet newFS = FontSet.Load(F.FontFile);
                    FS[F.Name.ToLower()] = newFS;
                    Console.WriteLine("Loaded font \"{0}\" from \"{1}\"", F.Name, F.FontFile);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Failed to load font {0}:{1} -> {2}", F.Name, F.FontFile, e.Message);
                }
                    
            }

            Console.WriteLine("Number of labels: {0}", AS.Labels.Count());
            
            foreach (var T in AS.Labels)
            {
                Console.WriteLine("Adding label {0} with font {1}, position {2}, alignment {3}, attachment {4}", T.Text, T.Font, T.Position, T.Alignment, T.Attachment);
                FontSet F = null;

                FS.TryGetValue(T.Font.ToLower(), out F);
                if (F == null)
                {
                    LocalFonts.TryGetValue(T.Font.ToLower(), out F);
                }

                if (F == null)
                {
                    if (FS.Count()>0)
                    {
                        F = FS.Values.First();
                    }
                }
                if (F == null)
                {
                    Console.WriteLine("Still no font found for {0}!", T.Font);
                }
                DrawString(AttachPoint(new PointD(T.Position.X, T.Position.Y), boxtopleft, boxbottomright, T.Attachment), F, T.Text, T.FontSize, T.StrokeWidth > 0? T.StrokeWidth: 0.05, T.Alignment, false, T.Rotation);

            }

        }

        public void AddPolygon(PolyLine PLA)
        {
            Polygons.Add(PLA);
        }
    }
}
