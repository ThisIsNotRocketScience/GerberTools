using GerberLibrary.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artwork
{
    public class SolidQuadTreeItem : QuadTreeItem
    {
        int _x;
        int _y;
        public int x
        {
            get
            {
                return _x;
            }

            set
            {
                _x = value;
            }
        }

        public int y
        {
            get
            {
                return _y;
            }

            set
            {
                _y = value;
            }
        }
    }


    public class TINRSArtWorkRenderer
    {

        // From: http://stackoverflow.com/a/11448060/368354
        public static void SaveAsIcon(List<Bitmap> SourceBitmaps, string FilePath)
        {
            FileStream FS = new FileStream(FilePath, FileMode.Create);
            // ICO header
            FS.WriteByte(0); FS.WriteByte(0); // reserved
            FS.WriteByte(1); FS.WriteByte(0); // type = icon
            FS.WriteByte((byte)SourceBitmaps.Count); FS.WriteByte(0); // number of images
            List<MemoryStream> Files = new List<MemoryStream>();
            List<long> SizeIdx = new List<long>();
            List<long> OffIdx = new List<long>();
            foreach (var b in SourceBitmaps)
            {
                // Image size
                // Set to 0 for 256 px width/height
                if (b.Width < 256) FS.WriteByte((byte)b.Width); else FS.WriteByte(0);
                if (b.Height < 256) FS.WriteByte((byte)b.Height); else FS.WriteByte(0);
                // Palette
                FS.WriteByte(0);
                // Reserved
                FS.WriteByte(0);
                // Number of color planes
                FS.WriteByte(1); FS.WriteByte(0);
                // Bits per pixel
                FS.WriteByte(32); FS.WriteByte(0);

                // Data size, will be written after the data
                SizeIdx.Add(FS.Length);
                FS.WriteByte(0);
                FS.WriteByte(0);
                FS.WriteByte(0);
                FS.WriteByte(0);

                OffIdx.Add(FS.Length);
                // Offset to image data
                FS.WriteByte(0);
                FS.WriteByte(0);
                FS.WriteByte(0);
                FS.WriteByte(0);

                MemoryStream MS = new MemoryStream();
                b.Save(MS, System.Drawing.Imaging.ImageFormat.Png);
                MS.Seek(0, SeekOrigin.Begin);
                Files.Add(MS);
            }

            long CurrentOff = FS.Length;
            for (int i = 0; i < Files.Count; i++)
            {
                var F = Files[i];
                var OffTgt = OffIdx[i];
                var LenTgt = SizeIdx[i];
                long Len = F.Length;
                FS.Seek(LenTgt, SeekOrigin.Begin);
                FS.WriteByte((byte)Len);
                FS.WriteByte((byte)(Len >> 8));
                FS.WriteByte((byte)(Len >> 16));
                FS.WriteByte((byte)(Len >> 24));

                FS.Seek(OffTgt, SeekOrigin.Begin);
                FS.WriteByte((byte)CurrentOff);
                FS.WriteByte((byte)(CurrentOff >> 8));
                FS.WriteByte((byte)(CurrentOff >> 16));
                FS.WriteByte((byte)(CurrentOff >> 24));

                FS.Seek(0, SeekOrigin.End);

                F.CopyTo(FS);

                CurrentOff = FS.Length;


            }

            FS.Close();
        }


        public DelaunayBuilder Delaunay = new DelaunayBuilder();
        public QuadTreeNode MaskTree;
        public QuadTreeNode ArtTree;
        public Tiling.TilingDefinition TD = new Tiling.TilingDefinition();
        public List<Tiling.Polygon> SubDivPoly = new List<Tiling.Polygon>();

        public static Font GetAdjustedFont(Graphics GraphicRef, string GraphicString, Font OriginalFont, float ContainerWidth, float MaxFontSize, float MinFontSize, bool SmallestOnFail)
        {
            // We utilize MeasureString which we get via a control instance           
            for (float AdjustedSize = MaxFontSize; AdjustedSize >= MinFontSize; AdjustedSize--)
            {
                Font TestFont = new Font(OriginalFont.Name, AdjustedSize, OriginalFont.Style);

                // Test the string with the new size
                SizeF AdjustedSizeNew = GraphicRef.MeasureString(GraphicString, TestFont);

                if (ContainerWidth > Convert.ToInt32(AdjustedSizeNew.Width))
                {
                    // Good font, return it
                    return TestFont;
                }
            }

            // If you get here there was no fontsize that worked
            // return MinimumSize or Original?
            if (SmallestOnFail)
            {
                return new Font(OriginalFont.Name, MinFontSize, OriginalFont.Style);
            }
            else
            {
                return OriginalFont;
            }
        }


        public static void DrawIcon(int w, int h, Graphics G, string Label, float huerange = -1)
        {
            Bitmap Output = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(Output);
            TINRSArtWorkRenderer Rend = new TINRSArtWorkRenderer();
            Settings TheSettings = Rend.GetHashSettings(Label, huerange);



            TheSettings.InvertSource = true;
            //TheSettings.MaxSubDiv = 4;

            Bitmap TileGfx = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics G3 = Graphics.FromImage(TileGfx);
            G3.Clear(Color.Transparent);
            float D = 2;
            float x1 = D / 2;
            float x2 = w - x1;
            float y1 = D / 2;
            float y2 = h - x1;


            Font F = new Font("Panton ExtraBold", h * 0.5f, FontStyle.Bold);
            F = GetAdjustedFont(g, Label, F, w * 0.9f, h * 0.9f, 3, false);
            var S = g.MeasureString(Label, F);
            var S2 = g.MeasureString("WO", F);
            S.Height = S2.Height;

            float BaseScale = w / 128.0f;

            GraphicsPath GP = new GraphicsPath();
            GP.AddString(Label, new FontFamily("Panton ExtraBold"), 0, F.Size * 1.333333f, new PointF(w / 2 - S.Width / 2, h / 2 - S.Height / 2), new StringFormat());

            Bitmap M = new Bitmap(w, h);
            Graphics G2 = Graphics.FromImage(M);

            G2.Clear(Color.White);
            G2.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            G2.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            G2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            G2.SmoothingMode = SmoothingMode.AntiAlias;

            RenderIconBackdrop(G2, Color.Black, TheSettings, x1, x2, y1, y2, 2, Rend);
            G2.DrawPath(new Pen(Color.Black, 5), GP);


            //            G2.DrawString(Letter.Text, F, new SolidBrush(Color.White), w / 2 - S.Width / 2, h / 2 - S.Height / 2);
            G2.DrawPath(new Pen(Color.White, Math.Max(2.0f, 6 * BaseScale)), GP);
            G2.FillPath(new SolidBrush(Color.White), GP);

            g.DrawPath(new Pen(TheSettings.BackGroundColor, 5), GP);

            Rend.BuildTree(M, TheSettings);
            Rend.BuildStuff(M, TheSettings);

            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            float BaseScale2 = 1.0f;

            // g.FillPath(new SolidBrush(Color.Teal), GP);
            TheSettings = Rend.GetHashSettings(Label, huerange);

            RenderIconBackdrop(g, TheSettings.BackGroundColor, TheSettings, x1, x2, y1, y2, 0, Rend);
            Rend.DrawTiling(TheSettings, M, G3, Color.FromArgb(40, Color.Black), Color.Black, Math.Max(3, 4.5f * BaseScale2), false);
            Rend.DrawTiling(TheSettings, M, G3, TheSettings.BackgroundHighlight, Color.Black, Math.Max(1.4f, 3 * BaseScale2), false);
            Rend.DrawTiling(TheSettings, M, G3, Color.FromArgb(100, 255, 255, 0), Color.Black, Math.Max(1.0f, 1.4f * BaseScale2), false);

            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    var C = Output.GetPixel(j, i);
                    if (C.A > 0)
                    {
                        var TP = TileGfx.GetPixel(j, i);
                        var C2 = GerberLibrary.MathHelpers.Interpolate(C, TileGfx.GetPixel(j, i), TP.A / 255.0f);
                        Output.SetPixel(j, i, Color.FromArgb(C.A, C2));
                    }

                }
            }

            g.DrawPath(new Pen(Color.FromArgb(60, Color.Black), 3), GP);
            g.FillPath(new SolidBrush(Color.FromArgb(255, 255, 255)), GP);


            G.DrawImage(Output, 0, 0);

        }

        private static void RenderIconBackdrop(Graphics g, Color C, Settings TheSettings, float x1, float x2, float y1, float y2, int offset, TINRSArtWorkRenderer R)
        {
            R.TD.Create(TheSettings.TileType);
            var M = R.TD.NormalizeSize();
            var T = R.TD.DivisionSet[0];

            float w = x2 - x1;
            float h = y2 - y1;

            for (float s = 0.6f; s < 2.0; s += 0.5f)
            {

                List<PointF> P = new List<PointF>();
                P.Add(new PointF((T.A.x - M.x) * w * s + w / 2, (T.A.y - M.y) * h * s + h / 2));
                P.Add(new PointF((T.B.x - M.x) * w * s + w / 2, (T.B.y - M.y) * h * s + h / 2));
                P.Add(new PointF((T.C.x - M.x) * w * s + w / 2, (T.C.y - M.y) * h * s + h / 2));

                Matrix Mm = new Matrix();
                Mm.RotateAt(360.0f * (float)TheSettings.Rand.NextDouble(), new PointF(w / 2, h / 2));
                var Pa = P.ToArray();
                Mm.TransformPoints(Pa);
                g.FillPolygon(new SolidBrush(C), Pa);
            }

            //            g.FillEllipse(new SolidBrush(C), new RectangleF(x1 + 7, y1 + 7 , x2 - x1, y2 - y1));
        }

        public static void SaveMultiIcon(string outputfile, string label, float huerange = -1)
        {
            //Bitmap B1 = new Bitmap(16, 16, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //Graphics.FromImage(B1).Clear(Color.Transparent); ;
            //B1.MakeTransparent(Color.Transparent);

            //Artwork.TINRSArtWorkRenderer.DrawIcon(B1.Width, B1.Height, Graphics.FromImage(B1), label, huerange);

            Bitmap B2 = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics.FromImage(B2).Clear(Color.Transparent); ;
            B2.MakeTransparent(Color.Transparent);
            Artwork.TINRSArtWorkRenderer.DrawIcon(B2.Width, B2.Height, Graphics.FromImage(B2), label, huerange);

            Bitmap B2b = new Bitmap(48, 48, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics.FromImage(B2b).Clear(Color.Transparent); ;
            B2b.MakeTransparent(Color.Transparent);
            Artwork.TINRSArtWorkRenderer.DrawIcon(B2b.Width, B2b.Height, Graphics.FromImage(B2b), label, huerange);


            Bitmap B3 = new Bitmap(64, 64, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics.FromImage(B3).Clear(Color.Transparent); ;
            B3.MakeTransparent(Color.Transparent);
            Artwork.TINRSArtWorkRenderer.DrawIcon(B3.Width, B3.Height, Graphics.FromImage(B3), label, huerange);


            Bitmap B4 = new Bitmap(128, 128, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics.FromImage(B4).Clear(Color.Transparent); ;
            B4.MakeTransparent(Color.Transparent);

            Artwork.TINRSArtWorkRenderer.DrawIcon(B4.Width, B4.Height, Graphics.FromImage(B4), label, huerange);

            List<Bitmap> IcoBMPs = new List<Bitmap>() { B2, B2b, B3, B4 };
            Artwork.TINRSArtWorkRenderer.SaveAsIcon(IcoBMPs, outputfile);
        }


        public void DrawTiling(Settings S, Bitmap MaskBitmap, Graphics G, Color FGColor, Color BGColor, float linewidth, bool Clear = true)
        {
            G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Color FG = FGColor;
            Color BG = BGColor;

            if (S.InvertOutput)
            {
                FG = BGColor;
                BG = FGColor;
            }
            if (S.Mode == Settings.ArtMode.QuadTree)
            {
                if (ArtTree != null)
                {
                    if (Clear) G.Clear(BG);
                    G.RotateTransform(S.DegreesOff);
                    // e.Graphics.TranslateTransform(0, -140);
                    ArtTree.DrawArt(G, FG);
                }
            }

            if (S.Mode == Settings.ArtMode.Delaunay)
            {
                Delaunay.Render(new GraphicsGraphicsInterface(G), FG, BG);
            }

            if (S.Mode == Settings.ArtMode.Tiling)
            {
                if (Clear) G.Clear(BG);
                PointF[] ThePoints = new PointF[3] { new PointF(), new PointF(), new PointF() };
                Pen P = new Pen(FG, linewidth);
                for (int j = 0; j < SubDivPoly.Count; j++)
                {
                    var a = SubDivPoly[j];
                    for (int i = 0; i < 3; i++)
                    {
                        ThePoints[i].X = (float)a.Vertices[i].x;
                        ThePoints[i].Y = (float)a.Vertices[i].y;
                    }
                    G.DrawPolygon(P, ThePoints);
                }
            }
        }

        public static Color GetHashColor(string text)
        {
            return MakeColor(HashHue(text));
        }

        public static Color GetHashHighlight(string text)
        {
            return MakeHighlight(HashHue(text));
        }

        public static Color MakeColor(double H)
        {
            int r, g, b;
            GerberLibrary.MathHelpers.HsvToRgb(H, 1.0, 0.5, out r, out g, out b);
            return Color.FromArgb(r, g, b);

        }
        public static Color MakeHighlight(double H)
        {
            int r, g, b;
            double DH = ((H + 60) % 120) - 60;
            H += DH * 0.4;
            GerberLibrary.MathHelpers.HsvToRgb(H, 1.0, 0.7, out r, out g, out b);
            return Color.FromArgb(r, g, b);
        }

        private static double HashHue(string text)
        {
            double H = 0;
            for (int i = 0; i < text.Length; i++)
            {
                H += ((text[i] - 'A') % 26) * (360.0 / 26);
                H = H % 360;
            }

            return H;
        }

        public Settings GetHashSettings(string text, float huerange = -1)
        {
            Settings S = new Settings();
            S.BackGroundColor = GetHashColor(text);
            S.BackgroundHighlight = GetHashHighlight(text);
            if (huerange > -1)
            {
                S.BackGroundColor = MakeColor(huerange * 360.0f);
                S.BackgroundHighlight = MakeHighlight(huerange * 360.0f);
            }
            S.TileType = Tiling.TilingType.Conway;
            S.MaxSubDiv = 6;
            for (int i = 0; i < text.Length; i++)
            {
                S.DegreesOff += (int)(text[i] * 112.123);
                S.DegreesOff = S.DegreesOff % 360;
            }
            S.Rand = new Random((int)(S.DegreesOff * 100.0));

            return S;
        }

        public void BuildTree(Bitmap Mask, Settings TheSettings)
        {
            int i = Math.Max(Mask.Width, Mask.Height);
            int R = 1;

            while (R < i) R *= 2;

            MaskTree = new QuadTreeNode() { xstart = 0, ystart = 0, xend = R, yend = R };

            float ThresholdLevel = TheSettings.Threshold * 0.01f;

            for (int x = 0; x < Mask.Width; x++)
            {
                for (int y = 0; y < Mask.Height; y++)
                {
                    var C = Mask.GetPixel(x, y);
                    bool doit = false;
                    if (TheSettings.InvertSource)
                    {
                        doit = C.GetBrightness() > ThresholdLevel;
                    }
                    else
                    {
                        doit = C.GetBrightness() < ThresholdLevel;
                    }
                    if (doit)
                    {
                        MaskTree.Insert(x, y, new SolidQuadTreeItem() { x = (int)x, y = (int)y }, 8);
                    }
                }
            }
        }

        public int BuildStuff(Bitmap Mask, Settings TheSettings)
        {
            int i = Math.Max(Mask.Width, Mask.Height);
            int R = 1;
            while (R < i) R *= 2;
            ArtTree = null;
            float ThresholdLevel = TheSettings.Threshold * 0.01f;
            switch (TheSettings.Mode)
            {
                case Settings.ArtMode.QuadTree:
                    {
                        DateTime rR = DateTime.Now;

                        ArtTree = new QuadTreeNode() { xstart = -1000, ystart = -1000, xend = R, yend = R };
                        float hoek = (float)((6.283 * TheSettings.DegreesOff) / 360.0);
                        for (int x = 0; x < Mask.Width; x++)
                        {
                            for (int y = 0; y < Mask.Height; y++)
                            {
                                var C = Mask.GetPixel(x, y);
                                bool doit = false;
                                if (TheSettings.InvertSource)
                                {
                                    doit = C.GetBrightness() > ThresholdLevel;
                                }
                                else
                                {
                                    doit = C.GetBrightness() < ThresholdLevel;
                                }
                                if (doit)
                                {
                                    double cx = Math.Cos(hoek) * x + Math.Sin(hoek) * y;
                                    double cy = Math.Sin(hoek) * -x + Math.Cos(hoek) * y;
                                    ArtTree.Insert((int)cx, (int)cy, new SolidQuadTreeItem() { x = (int)cx, y = (int)cy }, TheSettings.MaxSubDiv);
                                }
                            }
                        }
                        var Elapsed = DateTime.Now - rR;
                        return (int)Elapsed.TotalMilliseconds;
                    }

                case Settings.ArtMode.Delaunay:
                    {
                        DateTime rR = DateTime.Now;
                        ArtTree = new QuadTreeNode() { xstart = -1000, ystart = -1000, xend = R, yend = R };
                        float hoek = (float)((6.283 * TheSettings.DegreesOff) / 360.0);
                        for (int x = 0; x < Mask.Width; x++)
                        {
                            for (int y = 0; y < Mask.Height; y++)
                            {
                                var C = Mask.GetPixel(x, y);
                                bool doit = false;
                                if (TheSettings.InvertSource)
                                {
                                    doit = C.GetBrightness() > ThresholdLevel;
                                }
                                else
                                {
                                    doit = C.GetBrightness() < ThresholdLevel;
                                }
                                if (doit)
                                {
                                    double cx = Math.Cos(hoek) * x + Math.Sin(hoek) * y;
                                    double cy = Math.Sin(hoek) * -x + Math.Cos(hoek) * y;
                                    ArtTree.Insert((int)cx, (int)cy, new SolidQuadTreeItem() { x = (int)cx, y = (int)cy }, TheSettings.MaxSubDiv);
                                }
                            }
                        }
                        
                        Delaunay.Build(ArtTree, TheSettings.DegreesOff);

                        var Elapsed = DateTime.Now - rR;
                        return (int)Elapsed.TotalMilliseconds;
                    };

                case Settings.ArtMode.Tiling:
                    {
                        TD.Create(TheSettings.TileType);
                        var P = TD.CreateBaseTriangle(TheSettings.BaseTile, 1000);
                        var P2 = TD.CreateBaseTriangle(TheSettings.BaseTile, 1000);
                        P.Rotate(TheSettings.DegreesOff);
                        P.AlterToFit(Mask.Width, Mask.Height);
                        P2.Rotate(TheSettings.DegreesOff);
                        P2.AlterToFit(Mask.Width, Mask.Height);

                        if (TheSettings.Symmetry)
                        {
                            P.ShiftToEdge(Mask.Width / 2, Mask.Height / 2);
                            P2.ShiftToEdge(Mask.Width / 2, Mask.Height / 2);
                            P2.Flip(Mask.Width / 2, Mask.Height / 2);
                            if (TheSettings.SuperSymmetry)
                            {
                                P2.MirrorAround(Mask.Width / 2, Mask.Height / 2);
                            }
                        }

                        DateTime rR = DateTime.Now;
                        SubDivPoly = TD.SubdivideAdaptive(P, TheSettings.MaxSubDiv, MaskTree, TheSettings.alwayssubdivide);

                        if (TheSettings.Symmetry)
                        {
                            SubDivPoly.AddRange(TD.SubdivideAdaptive(P2, TheSettings.MaxSubDiv, MaskTree, TheSettings.alwayssubdivide));
                        }

                        if (TheSettings.xscalesmallerlevel != 0)
                        {
                            float midx = Mask.Width / 2.0f;
                            float width = Mask.Width;
                            float offs = TheSettings.xscalecenter * 0.01f * width;
                            foreach (var A in SubDivPoly)
                            {
                                var M = A.Mid();
                                float scaler = 1.0f - ((float)(M.x-offs) / width) * TheSettings.xscalesmallerlevel * 0.01f;
                                //scaler = Math.Max(0, Math.Min(1.0f, scaler));
                                A.ScaleDown(TheSettings.scalingMode, scaler);
                            }
                        }
                        if (TheSettings.scalesmallerfactor != 1.0f)
                        {
                            foreach (var A in SubDivPoly)
                            {
                                A.ScaleDown(Settings.TriangleScaleMode.Balanced, TheSettings.scalesmallerfactor);
                            }
                        }

                        if (TheSettings.scalesmaller != 0)
                        {
                            float scaler = Math.Abs(TheSettings.scalesmaller);
                            if (TheSettings.scalesmaller > 0)
                            {
                                scaler = scaler / 10.0f;
                            }
                            else
                            {
                                scaler = -scaler / 10.0f;
                            }
                            foreach (var A in SubDivPoly)
                            {
                              
                                if (A.depth - TheSettings.scalesmallerlevel <= 1)
                                {

                                }
                                else
                                {
                                    A.ScaleDown(TheSettings.scalingMode, (1 + scaler * (1.0f / (A.depth - TheSettings.scalesmallerlevel))));
                                   
                                }
                            }
                        }
                        var Elapsed = DateTime.Now - rR;
                        return (int)Elapsed.TotalMilliseconds;
                    }

            };
            return 0;
        }

    }

}
