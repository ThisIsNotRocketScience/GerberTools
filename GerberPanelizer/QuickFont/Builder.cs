using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.IO;

namespace QuickFont
{

    /// <summary>
    /// Class for building a Quick Font, given a Font
    /// and a configuration object.
    /// </summary>
    class Builder
    {

        private string charSet;
        private QFontBuilderConfiguration config;
        private Font font;

        public Builder(Font font, QFontBuilderConfiguration config)
        {
            this.charSet = config.charSet;
            this.config = config;
            this.font = font;
            
        }



        private static Dictionary<char, QFontGlyph> CreateCharGlyphMapping(QFontGlyph[] glyphs)
        {
            var dict = new Dictionary<char, QFontGlyph>();
            for (int i = 0; i < glyphs.Length; i++)
                dict.Add(glyphs[i].character, glyphs[i]);

            return dict;
        }


        //these do not affect the actual width of glyphs (we measure widths pixel-perfectly ourselves), but is used to detect whether a font is monospaced
        private List<SizeF> GetGlyphSizes(Font font)
        {
            Bitmap bmp = new Bitmap(512, 512, PixelFormat.Format24bppRgb);
            Graphics graph = Graphics.FromImage(bmp);
            List<SizeF> sizes = new List<SizeF>();

            for (int i = 0; i < charSet.Length; i++)
            {
                var charSize = graph.MeasureString("" + charSet[i], font);
                sizes.Add(new SizeF(charSize.Width, charSize.Height));
            }

            graph.Dispose();
            bmp.Dispose();

            return sizes;
        }

        private SizeF GetMaxGlyphSize(List<SizeF> sizes)
        {
            SizeF maxSize = new SizeF(0f, 0f);
            for (int i = 0; i < charSet.Length; i++)
            {
                if (sizes[i].Width > maxSize.Width)
                    maxSize.Width = sizes[i].Width;

                if (sizes[i].Height > maxSize.Height)
                    maxSize.Height = sizes[i].Height;
            }

            return maxSize;
        }

        private SizeF GetMinGlyphSize(List<SizeF> sizes)
        {
            SizeF minSize = new SizeF(float.MaxValue, float.MaxValue);
            for (int i = 0; i < charSet.Length; i++)
            {
                if (sizes[i].Width < minSize.Width)
                    minSize.Width = sizes[i].Width;

                if (sizes[i].Height < minSize.Height)
                    minSize.Height = sizes[i].Height;
            }

            return minSize;
        }


        /// <summary>
        /// Returns true if all glyph widths are within 5% of each other
        /// </summary>
        /// <param name="sizes"></param>
        /// <returns></returns>
        private bool IsMonospaced(List<SizeF> sizes)
        {
            var min = GetMinGlyphSize(sizes);
            var max = GetMaxGlyphSize(sizes);

            if (max.Width - min.Width < max.Width * 0.05f)
                return true;

            return false;
        }

        /*
        private SizeF GetMaxGlyphSize(Font font)
        {
            Bitmap bmp = new Bitmap(256, 256, PixelFormat.Format24bppRgb);
            Graphics graph = Graphics.FromImage(bmp);

            SizeF maxSize = new SizeF(0f, 0f);
            for (int i = 0; i < charSet.Length; i++)
            {
                var charSize = graph.MeasureString("" + charSet[i], font);

                if (charSize.Width > maxSize.Width)
                    maxSize.Width = charSize.Width;

                if (charSize.Height > maxSize.Height)
                    maxSize.Height = charSize.Height;
            }

            graph.Dispose();
            bmp.Dispose();

            return maxSize;
        }*/

        //The initial bitmap is simply a long thin strip of all glyphs in a row
        private Bitmap CreateInitialBitmap(Font font, SizeF maxSize, int initialMargin, out QFontGlyph[] glyphs, TextGenerationRenderHint renderHint)
        {
            glyphs = new QFontGlyph[charSet.Length];

            int spacing = (int)Math.Ceiling(maxSize.Width) + 2 * initialMargin;
            Bitmap bmp = new Bitmap(spacing * charSet.Length, (int)Math.Ceiling(maxSize.Height) + 2 * initialMargin, PixelFormat.Format24bppRgb);
            Graphics graph = Graphics.FromImage(bmp);

            switch(renderHint){
                case TextGenerationRenderHint.SizeDependent: 
                    graph.TextRenderingHint = font.Size <= 12.0f  ? TextRenderingHint.ClearTypeGridFit : TextRenderingHint.AntiAlias; 
                    break;
                case TextGenerationRenderHint.AntiAlias: 
                    graph.TextRenderingHint = TextRenderingHint.AntiAlias; 
                    break;
                case TextGenerationRenderHint.AntiAliasGridFit: 
                    graph.TextRenderingHint = TextRenderingHint.AntiAliasGridFit; 
                    break;
                case TextGenerationRenderHint.ClearTypeGridFit:
                    graph.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    break;
                case TextGenerationRenderHint.SystemDefault:
                    graph.TextRenderingHint = TextRenderingHint.SystemDefault;
                    break;
            }

  
                

            int xOffset = initialMargin;
            for (int i = 0; i < charSet.Length; i++)
            {
                graph.DrawString("" + charSet[i], font, Brushes.White, xOffset, initialMargin);
                var charSize = graph.MeasureString("" + charSet[i], font);
                glyphs[i] = new QFontGlyph(0, new Rectangle(xOffset - initialMargin, 0, (int)charSize.Width + initialMargin * 2, (int)charSize.Height + initialMargin * 2), 0, charSet[i]);
                xOffset += (int)charSize.Width + initialMargin * 2;
            }

            graph.Flush();
            graph.Dispose();

            return bmp;
        }

       



        private delegate bool EmptyDel(BitmapData data, int x, int y);



        private static void RetargetGlyphRectangleInwards(BitmapData bitmapData, QFontGlyph glyph, bool setYOffset, byte alphaTolerance)
        {
            int startX, endX;
            int startY, endY;

            var rect = glyph.rect;

            EmptyDel emptyPix;

            if (bitmapData.PixelFormat == PixelFormat.Format32bppArgb)
                emptyPix = delegate(BitmapData data, int x, int y) { return QBitmap.EmptyAlphaPixel(data, x, y, alphaTolerance); };
            else
                emptyPix = delegate(BitmapData data, int x, int y) { return QBitmap.EmptyPixel(data, x, y); };


            unsafe
            {

                for (startX = rect.X; startX < bitmapData.Width; startX++)
                    for (int j = rect.Y; j < rect.Y + rect.Height; j++)
                        if (!emptyPix(bitmapData, startX, j))
                            goto Done1;
                Done1:

                for (endX = rect.X + rect.Width; endX >= 0; endX--)
                    for (int j = rect.Y; j < rect.Y + rect.Height; j++)
                        if (!emptyPix(bitmapData, endX, j))
                            goto Done2;
                Done2:

                for (startY = rect.Y; startY < bitmapData.Height; startY++)
                    for (int i = startX; i < endX; i++)
                        if (!emptyPix(bitmapData, i, startY))
                            goto Done3;
                            
                Done3:

                for (endY = rect.Y + rect.Height; endY >= 0; endY--)
                    for (int i = startX; i < endX; i++)
                        if (!emptyPix(bitmapData, i, endY))
                            goto Done4;
                Done4:;


            }

            if (endY < startY)
                startY = endY = rect.Y;

            if (endX < startX)
                startX = endX = rect.X;

            glyph.rect = new Rectangle(startX, startY, endX - startX + 1, endY - startY + 1);

            if (setYOffset)
                glyph.yOffset = glyph.rect.Y;

        }




        private static void RetargetGlyphRectangleOutwards(BitmapData bitmapData, QFontGlyph glyph, bool setYOffset, byte alphaTolerance)
        {
            int startX,endX;
            int startY,endY;

            var rect = glyph.rect;

            EmptyDel emptyPix;

            if (bitmapData.PixelFormat == PixelFormat.Format32bppArgb)
                emptyPix = delegate(BitmapData data, int x, int y) { return QBitmap.EmptyAlphaPixel(data, x, y, alphaTolerance); };
            else
                emptyPix = delegate(BitmapData data, int x, int y) { return QBitmap.EmptyPixel(data, x, y); };


            unsafe
            {
                
                for (startX = rect.X; startX >= 0; startX--)
                {
                    bool foundPix = false;
                    for (int j = rect.Y; j <= rect.Y + rect.Height; j++)
                    {
                        if (!emptyPix(bitmapData, startX, j))
                        {
                            foundPix = true;
                            break;
                        }
                    }

                    if (!foundPix)
                    {
                        startX++;
                        break;
                    }
                }


                for (endX = rect.X + rect.Width; endX < bitmapData.Width; endX++)
                {
                    bool foundPix = false;
                    for (int j = rect.Y; j <= rect.Y + rect.Height; j++)
                    {
                        if (!emptyPix(bitmapData, endX, j))
                        {
                            foundPix = true;
                            break; 
                        }
                    }

                    if (!foundPix)
                    {
                        endX--;
                        break;
                    }
                }



                for (startY = rect.Y; startY >= 0; startY--)
                {
                    bool foundPix = false;
                    for (int i = startX; i <= endX; i++)
                    {
                        if (!emptyPix(bitmapData, i, startY))
                        {
                            foundPix = true;
                            break;
                        }
                    }

                    if (!foundPix)
                    {
                        startY++;
                        break;
                    }
                }



                for (endY = rect.Y + rect.Height; endY < bitmapData.Height; endY++)
                {
                    bool foundPix = false;
                    for (int i = startX; i <= endX; i++)
                    {
                        if (!emptyPix(bitmapData, i, endY))
                        {
                            foundPix = true;
                            break;
                        }
                    }

                    if (!foundPix)
                    {
                        endY--;
                        break;
                    }
                }



            }



            glyph.rect = new Rectangle(startX, startY, endX - startX + 1, endY - startY + 1);

            if (setYOffset)
                glyph.yOffset = glyph.rect.Y;

        }




        private static List<QBitmap> GenerateBitmapSheetsAndRepack(QFontGlyph[] sourceGlyphs, BitmapData[] sourceBitmaps, int destSheetWidth, int destSheetHeight, out QFontGlyph[] destGlyphs, int destMargin, bool usePowerOfTwo)
        {
            var pages = new List<QBitmap>();
            destGlyphs = new QFontGlyph[sourceGlyphs.Length];

            QBitmap currentPage = null;


            int maxY = 0;
            foreach (var glph in sourceGlyphs)
                maxY = Math.Max(glph.rect.Height, maxY);


            int finalPageIndex = 0;
            int finalPageRequiredWidth = 0;
            int finalPageRequiredHeight = 0;

            
            for (int k = 0; k < 2; k++)
            {
                bool pre = k == 0;  //first iteration is simply to determine the required size of the final page, so that we can crop it in advance


                int xPos = 0;
                int yPos = 0;
                int maxYInRow = 0;
                int totalTries = 0;

                for (int i = 0; i < sourceGlyphs.Length; i++)
                {


                    if(!pre && currentPage == null){

                        if (finalPageIndex == pages.Count)
                        {
                            int width = Math.Min(destSheetWidth, usePowerOfTwo ? PowerOfTwo(finalPageRequiredWidth) : finalPageRequiredWidth);
                            int height = Math.Min(destSheetHeight, usePowerOfTwo ? PowerOfTwo(finalPageRequiredHeight) : finalPageRequiredHeight);

                            currentPage = new QBitmap(new Bitmap(width, height, PixelFormat.Format32bppArgb));
                            currentPage.Clear32(255, 255, 255, 0); //clear to white, but totally transparent
                        }
                        else
                        {
                            currentPage = new QBitmap(new Bitmap(destSheetWidth, destSheetHeight, PixelFormat.Format32bppArgb));
                            currentPage.Clear32(255, 255, 255, 0); //clear to white, but totally transparent
                        }
                        pages.Add(currentPage);

                    }
                


                    totalTries++;

                    if (totalTries > 10 * sourceGlyphs.Length)
                        throw new Exception("Failed to fit font into texture pages");


                    var rect = sourceGlyphs[i].rect;

                    if (xPos + rect.Width + 2 * destMargin <= destSheetWidth && yPos + rect.Height + 2 * destMargin <= destSheetHeight)
                    {
                        if (!pre)
                        {
                            //add to page
                            if(sourceBitmaps[sourceGlyphs[i].page].PixelFormat == PixelFormat.Format32bppArgb)
                                QBitmap.Blit(sourceBitmaps[sourceGlyphs[i].page], currentPage.bitmapData, rect.X, rect.Y, rect.Width, rect.Height, xPos + destMargin, yPos + destMargin);
                            else
                                QBitmap.BlitMask(sourceBitmaps[sourceGlyphs[i].page], currentPage.bitmapData, rect.X, rect.Y, rect.Width, rect.Height, xPos + destMargin, yPos + destMargin);

                            destGlyphs[i] = new QFontGlyph(pages.Count - 1, new Rectangle(xPos + destMargin, yPos + destMargin, rect.Width, rect.Height), sourceGlyphs[i].yOffset, sourceGlyphs[i].character);
                        }
                        else
                        {
                            finalPageRequiredWidth = Math.Max(finalPageRequiredWidth, xPos + rect.Width + 2 * destMargin);
                            finalPageRequiredHeight = Math.Max(finalPageRequiredHeight, yPos + rect.Height + 2 * destMargin);
                        }


                        xPos += rect.Width + 2 * destMargin;
                        maxYInRow = Math.Max(maxYInRow, rect.Height);

                        continue;
                    }


                    if (xPos + rect.Width + 2 * destMargin > destSheetWidth)
                    {
                        i--;

                        yPos += maxYInRow + 2 * destMargin;
                        xPos = 0;

                        if (yPos + maxY + 2 * destMargin > destSheetHeight)
                        {
                            yPos = 0;

                            if (!pre)
                            {
                                currentPage = null;
                            }
                            else
                            {
                                finalPageRequiredWidth = 0;
                                finalPageRequiredHeight = 0;
                                finalPageIndex++;
                            }
                        }
                        continue;
                    }

                }

            }


            return pages;


        }



        public QFontData BuildFontData()
        {
            return BuildFontData(null);
        }

        public QFontData BuildFontData(string saveName)
        {
            if (config.ForcePowerOfTwo && config.SuperSampleLevels != PowerOfTwo(config.SuperSampleLevels))
            {
                throw new ArgumentOutOfRangeException("SuperSampleLevels must be a power of two when using ForcePowerOfTwo.");
            }

            if (config.SuperSampleLevels <= 0 || config.SuperSampleLevels > 8)
            {
                throw new ArgumentOutOfRangeException("SuperSampleLevels = [" + config.SuperSampleLevels + "] is an unsupported value. Please use values in the range [1,8]"); 
            }

            int margin = 2; //margin in initial bitmap (don't bother to make configurable - likely to cause confusion
            int pageWidth = config.PageWidth * config.SuperSampleLevels; //texture page width
            int pageHeight = config.PageHeight * config.SuperSampleLevels; //texture page height
            bool usePowerOfTwo = config.ForcePowerOfTwo;
            int glyphMargin = config.GlyphMargin * config.SuperSampleLevels;

            QFontGlyph[] initialGlyphs;
            var sizes = GetGlyphSizes(font);
            var maxSize = GetMaxGlyphSize(sizes);
            var initialBmp = CreateInitialBitmap(font, maxSize, margin, out initialGlyphs,config.TextGenerationRenderHint);
            var initialBitmapData = initialBmp.LockBits(new Rectangle(0, 0, initialBmp.Width, initialBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int minYOffset = int.MaxValue;
            foreach (var glyph in initialGlyphs){
                RetargetGlyphRectangleInwards(initialBitmapData, glyph, true, config.KerningConfig.alphaEmptyPixelTolerance);
                minYOffset = Math.Min(minYOffset,glyph.yOffset);
            }
            minYOffset--; //give one pixel of breathing room?

            foreach (var glyph in initialGlyphs)
                glyph.yOffset -= minYOffset;
           

            QFontGlyph[] glyphs; 
            var bitmapPages = GenerateBitmapSheetsAndRepack( initialGlyphs,new BitmapData[1] { initialBitmapData},pageWidth, pageHeight, out glyphs, glyphMargin, usePowerOfTwo);

            initialBmp.UnlockBits(initialBitmapData);
            initialBmp.Dispose();

            if (config.SuperSampleLevels != 1)
            {
                ScaleSheetsAndGlyphs(bitmapPages, glyphs, 1.0f / config.SuperSampleLevels);
                RetargetAllGlyphs(bitmapPages, glyphs,config.KerningConfig.alphaEmptyPixelTolerance);
            }


            //create list of texture pages
            var pages = new List<TexturePage>();
            foreach (var page in bitmapPages)
                pages.Add(new TexturePage(page.bitmapData));

            var fontData = new QFontData();
            fontData.CharSetMapping = CreateCharGlyphMapping(glyphs);
            fontData.Pages = pages.ToArray();
            fontData.CalculateMeanWidth();
            fontData.CalculateMaxHeight();
            fontData.KerningPairs = KerningCalculator.CalculateKerning(charSet.ToCharArray(), glyphs, bitmapPages,config.KerningConfig);
            fontData.naturallyMonospaced = IsMonospaced(sizes);

            if (saveName != null)
            {
                if (bitmapPages.Count == 1)
                    bitmapPages[0].bitmap.Save(saveName + ".png", System.Drawing.Imaging.ImageFormat.Png);
                else
                {
                    for (int i = 0; i < bitmapPages.Count; i++)
                        bitmapPages[i].bitmap.Save(saveName + "_sheet_" + i + ".png", System.Drawing.Imaging.ImageFormat.Png);
                }
            }


            if (config.ShadowConfig != null)
                fontData.dropShadow = BuildDropShadow(bitmapPages, glyphs, config.ShadowConfig, charSet.ToCharArray(),config.KerningConfig.alphaEmptyPixelTolerance);



            foreach (var page in bitmapPages)
                page.Free();


            //validate glyphs
            var intercept = FirstIntercept(fontData.CharSetMapping);
            if (intercept != null)
                throw new Exception("Failed to create glyph set. Glyphs '" + intercept[0] + "' and '" + intercept[1] + "' were overlapping. This is could be due to an error in the font, or a bug in Graphics.MeasureString().");
            



            return fontData;

        }



        private static QFont BuildDropShadow(List<QBitmap> sourceFontSheets, QFontGlyph[] sourceFontGlyphs, QFontShadowConfiguration shadowConfig, char[] charSet, byte alphaTolerance)
        {

            QFontGlyph[] newGlyphs;

            var sourceBitmapData = new List<BitmapData>();
            foreach(var sourceSheet in sourceFontSheets)
                sourceBitmapData.Add(sourceSheet.bitmapData);
            

            //GenerateBitmapSheetsAndRepack(QFontGlyph[] sourceGlyphs, BitmapData[] sourceBitmaps, int destSheetWidth, int destSheetHeight, out QFontGlyph[] destGlyphs, int destMargin, bool usePowerOfTwo)

            var bitmapSheets = GenerateBitmapSheetsAndRepack(sourceFontGlyphs, sourceBitmapData.ToArray(), shadowConfig.PageWidth, shadowConfig.PageHeight, out newGlyphs, shadowConfig.GlyphMargin + shadowConfig.blurRadius*3, shadowConfig.ForcePowerOfTwo);

            //scale up in case we wanted bigger/smaller shadows
            if (shadowConfig.Scale != 1.0f)
                ScaleSheetsAndGlyphs(bitmapSheets, newGlyphs, shadowConfig.Scale); //no point in retargeting yet, since we will do it after blur


            //blacken and blur
            foreach (var bitmapSheet in bitmapSheets)
            {
                bitmapSheet.Colour32(0, 0, 0);
                bitmapSheet.BlurAlpha(shadowConfig.blurRadius, shadowConfig.blurPasses);

            }

            


            //retarget after blur and scale
            RetargetAllGlyphs(bitmapSheets, newGlyphs, alphaTolerance);

            //create list of texture pages
            var newTextureSheets = new List<TexturePage>();
            foreach (var page in bitmapSheets)
                newTextureSheets.Add(new TexturePage(page.bitmapData));


            var fontData = new QFontData();
            fontData.CharSetMapping = new Dictionary<char, QFontGlyph>();
            for(int i = 0; i < charSet.Length; i++)
                fontData.CharSetMapping.Add(charSet[i],newGlyphs[i]);

            fontData.Pages = newTextureSheets.ToArray();
            fontData.CalculateMeanWidth();
            fontData.CalculateMaxHeight();


            foreach (var sheet in bitmapSheets)
                sheet.Free();

            
            return new QFont(fontData);
        }



        private static void ScaleSheetsAndGlyphs(List<QBitmap> pages, QFontGlyph[] glyphs, float scale)
        {
            foreach (var page in pages)
                page.DownScale32((int)(page.bitmap.Width * scale), (int)(page.bitmap.Height * scale));


            foreach (var glyph in glyphs)
            {
                glyph.rect = new Rectangle((int)(glyph.rect.X * scale), (int)(glyph.rect.Y * scale), (int)(glyph.rect.Width * scale), (int)(glyph.rect.Height * scale));
                glyph.yOffset = (int)(glyph.yOffset * scale);
  
            }
        }

        private static void RetargetAllGlyphs(List<QBitmap> pages, QFontGlyph[] glyphs, byte alphaTolerance)
        {
            foreach (var glyph in glyphs)
                RetargetGlyphRectangleOutwards(pages[glyph.page].bitmapData, glyph, false, alphaTolerance);
        }





        public static void SaveQFontDataToFile(QFontData data, string filePath)
        {
            var lines = data.Serialize();
            StreamWriter writer = new StreamWriter(filePath + ".qfont");
            foreach (var line in lines)
                writer.WriteLine(line);
            
            writer.Close();

        }



        public static void CreateBitmapPerGlyph(QFontGlyph[] sourceGlyphs, QBitmap[] sourceBitmaps, out QFontGlyph[]  destGlyphs, out QBitmap[] destBitmaps){
            destBitmaps = new QBitmap[sourceGlyphs.Length];
            destGlyphs = new QFontGlyph[sourceGlyphs.Length];
            for(int i = 0; i < sourceGlyphs.Length; i++){
                var sg = sourceGlyphs[i];
                destGlyphs[i] = new QFontGlyph(i,new Rectangle(0,0,sg.rect.Width,sg.rect.Height),sg.yOffset,sg.character);
                destBitmaps[i] = new QBitmap(new Bitmap(sg.rect.Width,sg.rect.Height,PixelFormat.Format32bppArgb));
                QBitmap.Blit(sourceBitmaps[sg.page].bitmapData,destBitmaps[i].bitmapData,sg.rect,0,0);
            }
        }


        public static QFontData LoadQFontDataFromFile(string filePath, float downSampleFactor, QFontLoaderConfiguration loaderConfig)
        {
            var lines = new List<String>();
            StreamReader reader = new StreamReader(filePath);
            string line;
            while((line = reader.ReadLine()) != null)
                lines.Add(line);
            reader.Close();

            var data = new QFontData();
            int pageCount = 0;
            char[] charSet;
            data.Deserialize(lines, out pageCount, out charSet);


            string namePrefix = filePath.Replace(".qfont","").Replace(" ", "");
            
            var bitmapPages = new List<QBitmap>();

            

            if (pageCount == 1)
            {
                bitmapPages.Add(new QBitmap(namePrefix + ".png"));
            }
            else
            {
                for (int i = 0; i < pageCount; i++)
                    bitmapPages.Add(new QBitmap(namePrefix + "_sheet_" + i));
            }


            foreach (var glyph in data.CharSetMapping.Values)
                RetargetGlyphRectangleOutwards(bitmapPages[glyph.page].bitmapData, glyph, false, loaderConfig.KerningConfig.alphaEmptyPixelTolerance);
 
            var intercept = FirstIntercept(data.CharSetMapping);
            if (intercept != null)
            {
                throw new Exception("Failed to load font from file. Glyphs '" + intercept[0] + "' and '" + intercept[1] + "' were overlapping. If you are texturing your font without locking pixel opacity, then consider using a larger glyph margin. This can be done by setting QFontBuilderConfiguration myQfontBuilderConfig.GlyphMargin, and passing it into CreateTextureFontFiles.");
            }

      
            if (downSampleFactor > 1.0f)
            {
                foreach (var page in bitmapPages)
                    page.DownScale32((int)(page.bitmap.Width * downSampleFactor), (int)(page.bitmap.Height * downSampleFactor));

                foreach (var glyph in data.CharSetMapping.Values)
                {

                    glyph.rect = new Rectangle((int)(glyph.rect.X * downSampleFactor),
                                                (int)(glyph.rect.Y * downSampleFactor),
                                                (int)(glyph.rect.Width * downSampleFactor),
                                                (int)(glyph.rect.Height * downSampleFactor));
                    glyph.yOffset = (int)(glyph.yOffset * downSampleFactor);
                }
            }
            else if (downSampleFactor < 1.0f )
            {
                // If we were simply to shrink the entire texture, then at some point we will make glyphs overlap, breaking the font.
                // For this reason it is necessary to copy every glyph to a separate bitmap, and then shrink each bitmap individually.
                QFontGlyph[] shrunkGlyphs;
                QBitmap[] shrunkBitmapsPerGlyph;
                CreateBitmapPerGlyph(Helper.ToArray(data.CharSetMapping.Values), bitmapPages.ToArray(), out shrunkGlyphs, out shrunkBitmapsPerGlyph);
                    
                //shrink each bitmap
                for (int i = 0; i < shrunkGlyphs.Length; i++)
                {   
                    var bmp = shrunkBitmapsPerGlyph[i];
                    bmp.DownScale32(Math.Max((int)(bmp.bitmap.Width * downSampleFactor),1), Math.Max((int)(bmp.bitmap.Height * downSampleFactor),1));
                    shrunkGlyphs[i].rect = new Rectangle(0, 0, bmp.bitmap.Width, bmp.bitmap.Height);
                    shrunkGlyphs[i].yOffset = (int)(shrunkGlyphs[i].yOffset * downSampleFactor);
                }

                var shrunkBitmapData = new BitmapData[shrunkBitmapsPerGlyph.Length];
                for(int i = 0; i < shrunkBitmapsPerGlyph.Length; i ++ ){
                    shrunkBitmapData[i] = shrunkBitmapsPerGlyph[i].bitmapData;
                }

                //use roughly the same number of pages as before..
                int newWidth = (int)(bitmapPages[0].bitmap.Width * (0.1f + downSampleFactor));
                int newHeight = (int)(bitmapPages[0].bitmap.Height * (0.1f + downSampleFactor));

                //free old bitmap pages since we are about to chuck them away
                for (int i = 0; i < pageCount; i++)
                    bitmapPages[i].Free();

                QFontGlyph[] shrunkRepackedGlyphs;
                bitmapPages = GenerateBitmapSheetsAndRepack(shrunkGlyphs, shrunkBitmapData, newWidth, newHeight, out shrunkRepackedGlyphs, 4, false);
                data.CharSetMapping = CreateCharGlyphMapping(shrunkRepackedGlyphs);

                foreach (var bmp in shrunkBitmapsPerGlyph)
                    bmp.Free();

                pageCount = bitmapPages.Count;
            }


            data.Pages = new TexturePage[pageCount];
            for(int i = 0; i < pageCount; i ++ )
                data.Pages[i] = new TexturePage(bitmapPages[i].bitmapData);


            if (downSampleFactor != 1.0f)
            {
                foreach (var glyph in data.CharSetMapping.Values)
                    RetargetGlyphRectangleOutwards(bitmapPages[glyph.page].bitmapData, glyph, false, loaderConfig.KerningConfig.alphaEmptyPixelTolerance);
 

                intercept = FirstIntercept(data.CharSetMapping);
                if (intercept != null)
                {
                    throw new Exception("Failed to load font from file. Glyphs '" + intercept[0] + "' and '" + intercept[1] + "' were overlapping. This occurred only after resizing your texture font, implying that there is a bug in QFont. ");
                }
            }



            var glyphList = new List<QFontGlyph>();

            foreach (var c in charSet)
                glyphList.Add(data.CharSetMapping[c]);

            if (loaderConfig.ShadowConfig != null)
                data.dropShadow = BuildDropShadow(bitmapPages, glyphList.ToArray(), loaderConfig.ShadowConfig, Helper.ToArray(charSet),loaderConfig.KerningConfig.alphaEmptyPixelTolerance);


            data.KerningPairs = KerningCalculator.CalculateKerning(Helper.ToArray(charSet), glyphList.ToArray(), bitmapPages, loaderConfig.KerningConfig);
            


            data.CalculateMeanWidth();
            data.CalculateMaxHeight();


            for (int i = 0; i < pageCount; i++)
                bitmapPages[i].Free();


            return data;
        }



        private static char[] FirstIntercept(Dictionary<char,QFontGlyph> charSet)
        {
            char[] keys = Helper.ToArray(charSet.Keys);

            for (int i = 0; i < keys.Length; i++)
            {
                for (int j = i + 1; j < keys.Length; j++)
                {
                    if (charSet[keys[i]].page == charSet[keys[j]].page && RectangleIntersect(charSet[keys[i]].rect, charSet[keys[j]].rect))
                    {
                        return new char[2] { keys[i], keys[j] };
                    }

                }
            }
            return null;
        }


        private static bool RectangleIntersect(Rectangle r1, Rectangle r2)
        {
            return (r1.X < r2.X + r2.Width && r1.X + r1.Width > r2.X &&
                    r1.Y < r2.Y + r2.Height && r1.Y + r1.Height > r2.Y);

        }




        /// <summary>
        /// Returns the power of 2 that is closest to x, but not smaller than x.
        /// </summary>
        private static int PowerOfTwo(int x)
        {
            int shifts = 0;
            uint val = (uint)x;

            if (x < 0)
                return 0;

            while (val > 0)
            {
                val = val >> 1;
                shifts++;
            }

            val = (uint)1 << (shifts - 1);
            if (val < x)
            {
                val = val << 1;
            }

            return (int)val;
        }


    }
}
