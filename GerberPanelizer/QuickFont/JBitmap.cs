using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;  

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Audio;
using OpenTK.Input;
using OpenTK.Platform;

using OpenTK.Graphics.OpenGL;

namespace QuickFont
{

    /// <summary>
    /// A class for basic bitmap manipulation: clearing bitmaps, blitting from one bitmap to another etc.
    /// 
    /// Generally methods in this class are quite slow and are only intended for pre-rendering.
    /// 
    /// TODO : replace loops with block copies
    /// </summary>
    public partial class JBitmap
    {
        public enum JBitmapPixelFormat {Format8bpp=1, Format24bppBGR=3, Format32bppBGRA=4 }


        /// <summary>
        /// Gets the corresponding C# pixel format
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        private static System.Drawing.Imaging.PixelFormat GetPixFormat(JBitmapPixelFormat format) {
            switch (format){

                case JBitmapPixelFormat.Format8bpp:
                    return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                
                case JBitmapPixelFormat.Format24bppBGR:
                    return System.Drawing.Imaging.PixelFormat.Format24bppRgb;

                case JBitmapPixelFormat.Format32bppBGRA:
                    return System.Drawing.Imaging.PixelFormat.Format32bppArgb;

                default:
                    return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
            }



        }


        public Bitmap bitmap;
        public BitmapData bitmapData;
        public JBitmapPixelFormat format;


        public int h
        {
            get { return bitmapData.Height; }
        }

        public int w
        {
            get { return bitmapData.Width; }
        }



        public JBitmap(int w, int h, JBitmapPixelFormat _format)
        {

            format = _format;
            bitmap = new Bitmap(w, h, GetPixFormat(format));
            bitmapData = bitmap.LockBits(new Rectangle(0, 0, w, h),ImageLockMode.ReadWrite, GetPixFormat(format));
        }



        public JBitmap(String fileName, JBitmapPixelFormat _format)
        {
            format = _format;
            bitmap = new Bitmap(fileName);
            bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, GetPixFormat(format));
        }




        public JBitmap CreateCopy()
        {
            JBitmap newBitmap = new JBitmap(w, h, format);
            Blit(newBitmap, w, h);

            return newBitmap;

        }


 

        public void Clear(byte val)
        {
            unsafe
            {
                byte* sourcePtr = (byte*)(bitmapData.Scan0);

                for (int i = 0; i < bitmapData.Height; i++)
                {
                    for (int j = 0; j < bitmapData.Width; j++)
                    {
                       (*sourcePtr) = val;
                        sourcePtr++;
                    }
                    sourcePtr += bitmapData.Stride - bitmapData.Width * 1; //move to the end of the line (past unused space)
                }
            }
        }

        public void Clear24(byte r, byte g, byte b)
        {
            unsafe
            {
                byte* sourcePtr = (byte*)(bitmapData.Scan0);

                for (int i = 0; i < bitmapData.Height; i++)
                {
                    for (int j = 0; j < bitmapData.Width; j++)
                    {
                        *(sourcePtr) = b;
                        *(sourcePtr + 1) = g;
                        *(sourcePtr + 2) = r;

                        sourcePtr += 3;
                    }
                    sourcePtr += bitmapData.Stride - bitmapData.Width * 3; //move to the end of the line (past unused space)
                }
            }
        }






        /// <summary>
        /// Blits this bitmap onto the target - clips regions that are out of bounds.
        /// 
        /// Only supported for 8bbp
        /// </summary>
        /// <param name="target"></param>
        /// <param name="targetX"></param>
        /// <param name="targetY"></param>
        public void BlitOLD(JBitmap target, int px, int py){

            //currently only supported for 8 bpp source / target
            if (bitmapData.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed &&
                target.bitmapData.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {


                    int targetStartX, targetEndX;
                    int targetStartY, targetEndY;
                    int copyW, copyH;

                    targetStartX = Math.Max(px,0);
                    targetEndX = Math.Min(px + bitmapData.Width, target.bitmapData.Width);
                    
                    targetStartY = Math.Max(py,0);
                    targetEndY = Math.Min(py + bitmapData.Height, target.bitmapData.Height);

                    copyW = targetEndX - targetStartX;
                    copyH = targetEndY - targetStartY;

                    if(copyW < 0 ){
                        return;
                    }

                    if(copyH < 0){
                        return;
                    }

                    int sourceStartX = targetStartX - px;
                    int sourceStartY = targetStartY - py;


                unsafe
                {
                    byte* sourcePtr = (byte*)(bitmapData.Scan0);
                    byte* targetPtr = (byte*)(target.bitmapData.Scan0);


                    byte* targetY = targetPtr + targetStartY * target.bitmapData.Stride;
                    byte* sourceY = sourcePtr + sourceStartY * bitmapData.Stride;
                    for (int y = 0; y < copyH; y++, targetY += target.bitmapData.Stride, sourceY += bitmapData.Stride)
                    {

                        byte* targetOffset = targetY + targetStartX;
                        byte* sourceOffset = sourceY + sourceStartX;
                        for (int x = 0; x < copyW; x++, targetOffset++, sourceOffset++)
                        {
                            *(targetOffset) = *(sourceOffset);

                        }
                        
                    }
                }


            }



        }



        
        public void Blit(JBitmap target, int srcPx, int srcPy, int srcW, int srcH, int px, int py){

            if (target.format != format)
            {
                throw new Exception("Attempted to blit between bitmaps not of the same format");
            }

            int bpp = (int)format;

            int targetStartX, targetEndX;
            int targetStartY, targetEndY;
            int copyW, copyH;

            targetStartX = Math.Max(px, 0);
            targetEndX = Math.Min(px + srcW, target.bitmapData.Width);

            targetStartY = Math.Max(py, 0);
            targetEndY = Math.Min(py + srcH, target.bitmapData.Height);

            copyW = targetEndX - targetStartX;
            copyH = targetEndY - targetStartY;

            if (copyW < 0)
            {
                return;
            }

            if (copyH < 0)
            {
                return;
            }

            int sourceStartX = srcPx + targetStartX - px;
            int sourceStartY = srcPy + targetStartY - py;


            unsafe
            {
                byte* sourcePtr = (byte*)(bitmapData.Scan0);
                byte* targetPtr = (byte*)(target.bitmapData.Scan0);


                byte* targetY = targetPtr + targetStartY * target.bitmapData.Stride;
                byte* sourceY = sourcePtr + sourceStartY * bitmapData.Stride;
                for (int y = 0; y < copyH; y++, targetY += target.bitmapData.Stride, sourceY += bitmapData.Stride)
                {

                    byte* targetOffset = targetY + targetStartX * bpp;
                    byte* sourceOffset = sourceY + sourceStartX * bpp;
                    for (int x = 0; x < copyW * bpp; x++, targetOffset++, sourceOffset++)
                    {
                        *(targetOffset) = *(sourceOffset);

                    }

                }
            }
        }




        public void Blit(JBitmap target, int px, int py)
        {




            if (target.format != format)
            {
                throw new Exception("Attempted to blit between bitmaps not of the same format");
            }

            int bpp = (int)format;

            int targetStartX, targetEndX;
            int targetStartY, targetEndY;
            int copyW, copyH;

            targetStartX = Math.Max(px, 0);
            targetEndX = Math.Min(px + bitmapData.Width, target.bitmapData.Width);

            targetStartY = Math.Max(py, 0);
            targetEndY = Math.Min(py + bitmapData.Height, target.bitmapData.Height);

            copyW = targetEndX - targetStartX;
            copyH = targetEndY - targetStartY;

            if (copyW < 0)
            {
                return;
            }

            if (copyH < 0)
            {
                return;
            }

            int sourceStartX = targetStartX - px;
            int sourceStartY = targetStartY - py;


            unsafe
            {
                byte* sourcePtr = (byte*)(bitmapData.Scan0);
                byte* targetPtr = (byte*)(target.bitmapData.Scan0);


                byte* targetY = targetPtr + targetStartY * target.bitmapData.Stride;
                byte* sourceY = sourcePtr + sourceStartY * bitmapData.Stride;
                for (int y = 0; y < copyH; y++, targetY += target.bitmapData.Stride, sourceY += bitmapData.Stride)
                {

                    byte* targetOffset = targetY + targetStartX*bpp;
                    byte* sourceOffset = sourceY + sourceStartX*bpp;
                    for (int x = 0; x < copyW*bpp; x++, targetOffset++, sourceOffset++)
                    {
                        *(targetOffset) = *(sourceOffset);

                    }

                }
            }



        }


       


        /// <summary>
        /// Additively blits onto the target. 
        /// 
        /// Only supported for 8bbp.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="targetX"></param>
        /// <param name="targetY"></param>
        public void BlitAdditive(JBitmap target, int px, int py, float strength)
        {

            //currently only supported for 8 bpp source / target
            if (bitmapData.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed &&
                target.bitmapData.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {


                int targetStartX, targetEndX;
                int targetStartY, targetEndY;
                int copyW, copyH;

                targetStartX = Math.Max(px, 0);
                targetEndX = Math.Min(px + bitmapData.Width, target.bitmapData.Width);

                targetStartY = Math.Max(py, 0);
                targetEndY = Math.Min(py + bitmapData.Height, target.bitmapData.Height);

                copyW = targetEndX - targetStartX;
                copyH = targetEndY - targetStartY;

                if (copyW < 0)
                {
                    return;
                }

                if (copyH < 0)
                {
                    return;
                }

                int sourceStartX = targetStartX - px;
                int sourceStartY = targetStartY - py;


                unsafe
                {
                    byte* sourcePtr = (byte*)(bitmapData.Scan0);
                    byte* targetPtr = (byte*)(target.bitmapData.Scan0);
                    int sum;

                    byte* targetY = targetPtr + targetStartY * target.bitmapData.Stride;
                    byte* sourceY = sourcePtr + sourceStartY * bitmapData.Stride;
                    for (int y = 0; y < copyH; y++, targetY += target.bitmapData.Stride, sourceY += bitmapData.Stride)
                    {

                        byte* targetOffset = targetY + targetStartX;
                        byte* sourceOffset = sourceY + sourceStartX;
                        for (int x = 0; x < copyW; x++, targetOffset++, sourceOffset++)
                        {
                            sum = (int)*(targetOffset) + (int) (*(sourceOffset) * strength);

                            if (sum > 255)
                            {
                                *(targetOffset) = 255;
                            } else {
                                *(targetOffset) = (byte)sum;
                            }

                        }

                    }
                }


            }






        }



        /// <summary>
        /// Additively blits onto the target. 
        /// 
        /// Supports just 24bpp (source and target)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="targetX"></param>
        /// <param name="targetY"></param>
        public void BlitAdditive24(JBitmap target, int px, int py, float strength)
        {

            //currently only supported for 8 bpp source / target
            if (bitmapData.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb &&
                target.bitmapData.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            {


                int targetStartX, targetEndX;
                int targetStartY, targetEndY;
                int copyW, copyH;

                targetStartX = Math.Max(px, 0);
                targetEndX = Math.Min(px + bitmapData.Width, target.bitmapData.Width);

                targetStartY = Math.Max(py, 0);
                targetEndY = Math.Min(py + bitmapData.Height, target.bitmapData.Height);

                copyW = targetEndX - targetStartX;
                copyH = targetEndY - targetStartY;

                if (copyW < 0)
                {
                    return;
                }

                if (copyH < 0)
                {
                    return;
                }

                int sourceStartX = targetStartX - px;
                int sourceStartY = targetStartY - py;


                unsafe
                {
                    byte* sourcePtr = (byte*)(bitmapData.Scan0);
                    byte* targetPtr = (byte*)(target.bitmapData.Scan0);
                    int sum;

                    byte* targetY = targetPtr + targetStartY * target.bitmapData.Stride;
                    byte* sourceY = sourcePtr + sourceStartY * bitmapData.Stride;
                    for (int y = 0; y < copyH; y++, targetY += target.bitmapData.Stride, sourceY += bitmapData.Stride)
                    {

                        byte* targetOffset = targetY + targetStartX*3;
                        byte* sourceOffset = sourceY + sourceStartX*3;
                        for (int x = 0; x < copyW*3; x++, targetOffset++, sourceOffset++)
                        {
                            sum = (int)*(targetOffset) + (int)(*(sourceOffset) * strength);

                            if (sum > 255)
                            {
                                *(targetOffset) = 255;
                            }
                            else
                            {
                                *(targetOffset) = (byte)sum;
                            }

                        }

                    }
                }


            }

        }


        public void BlitMax(JBitmap target, int px, int py, float strength)
        {

            //currently only supported for 8 bpp source / target
            if (bitmapData.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed &&
                target.bitmapData.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {


                int targetStartX, targetEndX;
                int targetStartY, targetEndY;
                int copyW, copyH;

                targetStartX = Math.Max(px, 0);
                targetEndX = Math.Min(px + bitmapData.Width, target.bitmapData.Width);

                targetStartY = Math.Max(py, 0);
                targetEndY = Math.Min(py + bitmapData.Height, target.bitmapData.Height);

                copyW = targetEndX - targetStartX;
                copyH = targetEndY - targetStartY;

                if (copyW < 0)
                {
                    return;
                }

                if (copyH < 0)
                {
                    return;
                }

                int sourceStartX = targetStartX - px;
                int sourceStartY = targetStartY - py;


                unsafe
                {
                    byte* sourcePtr = (byte*)(bitmapData.Scan0);
                    byte* targetPtr = (byte*)(target.bitmapData.Scan0);
                    int max;

                    byte* targetY = targetPtr + targetStartY * target.bitmapData.Stride;
                    byte* sourceY = sourcePtr + sourceStartY * bitmapData.Stride;
                    for (int y = 0; y < copyH; y++, targetY += target.bitmapData.Stride, sourceY += bitmapData.Stride)
                    {

                        byte* targetOffset = targetY + targetStartX;
                        byte* sourceOffset = sourceY + sourceStartX;
                        for (int x = 0; x < copyW; x++, targetOffset++, sourceOffset++)
                        {
                            max = Math.Max((int)*(targetOffset), (int) (*(sourceOffset) * strength));

                            if (*(sourceOffset) * strength > *(targetOffset))
                            {
                                max = (int) (*(sourceOffset) * strength );

                                if (max > 255)
                                {
                                    *(targetOffset) = 255;
                                }
                                else
                                {
                                    *(targetOffset) = (byte)max;
                                }
                            }




                        }

                    }
                }


            }



        }



        /// <summary>
        /// Just unlocks the bits, so that the bitmap can no longer be blitted to / from.
        /// </summary>
        public void Free(){
            bitmap.UnlockBits(bitmapData);
            bitmap.Dispose();
        }

        /*
       * Resizes this bitmap to a smaller size using area-weighted averaging.
       * 
       */
        public void DownScale(int newWidth, int newHeight)
        {
            JBitmap newBitmap = new JBitmap(newWidth, newHeight, format);

            float xscale = (float)bitmapData.Width / newWidth;
            float yscale = (float)bitmapData.Height / newHeight;


            byte r = 0, g = 0, b = 0, a = 0;
            float summedR = 0f;
            float summedG = 0f;
            float summedB = 0f;
            float summedA = 0f;

            int left, right, top, bottom;  //the area of old pixels covered by the new bitmap


            float targetStartX, targetEndX;
            float targetStartY, targetEndY;

            float leftF, rightF, topF, bottomF; //edges of new pixel in old pixel coords
            float weight;
            float weightScale = xscale * yscale;
            float weightTotalTest = 0f;

            for (int m = 0; m < newHeight; m++)
            {
                for (int n = 0; n < newWidth; n++)
                {

                    leftF = n * xscale;
                    rightF = (n + 1) * xscale;

                    topF = m * yscale;
                    bottomF = (m + 1) * yscale;

                    left = (int)leftF;
                    right = (int)rightF;

                    top = (int)topF;
                    bottom = (int)bottomF;

                    if (left < 0) left = 0;
                    if (top < 0) top = 0;
                    if (right >= bitmapData.Width) right = bitmapData.Width - 1;
                    if (bottom >= bitmapData.Height) bottom = bitmapData.Height - 1;

                    summedR = 0f;
                    summedG = 0f;
                    summedB = 0f;
                    summedA = 0f;
                    weightTotalTest = 0f;

                    for (int j = top; j <= bottom; j++)
                    {
                        for (int i = left; i <= right; i++)
                        {
                            targetStartX = Math.Max(leftF, i);
                            targetEndX = Math.Min(rightF, i + 1);

                            targetStartY = Math.Max(topF, j);
                            targetEndY = Math.Min(bottomF, j + 1);

                            weight = (targetEndX - targetStartX) * (targetEndY - targetStartY);

                            GetPixelFast(i, j, ref r, ref g, ref b, ref a);

                            summedR += weight * r;
                            summedG += weight * g;
                            summedB += weight * b;
                            summedA += weight * a;

                            weightTotalTest += weight;

                        }
                    }

                    summedR /= weightScale;
                    summedG /= weightScale;
                    summedB /= weightScale;
                    summedA /= weightScale;

                    if (summedR < 0) summedR = 0f;
                    if (summedG < 0) summedG = 0f;
                    if (summedB < 0) summedB = 0f;
                    if (summedA < 0) summedA = 0f;

                    if (summedR >= 256) summedR = 255;
                    if (summedG >= 256) summedG = 255;
                    if (summedB >= 256) summedB = 255;
                    if (summedA >= 256) summedA = 255;

                    newBitmap.PutPixelFast(n, m, (byte)summedR, (byte)summedG, (byte)summedB, (byte)summedA);
                }

            }



            this.Free();

            this.bitmap = newBitmap.bitmap;
            this.bitmapData = newBitmap.bitmapData;
            this.format = newBitmap.format;


        }



        public byte GetPixel(int px, int py)
        {

            py = bitmapData.Height - py;

            if (px >= 0 && py >= 0 && px < bitmapData.Width && py < bitmapData.Height)
            {
                unsafe
                {
                    byte* addr = (byte*)(bitmapData.Scan0);
                    addr += bitmapData.Stride * py + px;
                    return *addr;

                }
            }
            else
            {
                return 0;
            }
        }


        public void GetPixel24(int px, int py, out byte r, out byte g, out byte b)
        {
            py = bitmapData.Height - py;

            if (px >= 0 && py >= 0 && px < bitmapData.Width && py < bitmapData.Height)
            {
                unsafe
                {
                    byte* addr = (byte*)(bitmapData.Scan0);
                    addr += bitmapData.Stride * py + px * 3;
                    b = *addr;
                    g = *(addr + 1);
                    r = *(addr + 2);
                }
            }
            else
            {
                r = 0;
                g = 0;
                b = 0;
            }

        }


        public void GetPixelFast(int px, int py, ref byte r, ref byte g, ref byte b, ref byte a)
        {
            unsafe
            {
                byte* addr = (byte*)(bitmapData.Scan0) + bitmapData.Stride * py + px * (int)format;

                if (format == JBitmapPixelFormat.Format8bpp)
                {
                    r = *addr;
                }
                else if (format == JBitmapPixelFormat.Format24bppBGR)
                {
                    b = *addr;
                    g = *(addr + 1);
                    r = *(addr + 2);
                }
                else if (format == JBitmapPixelFormat.Format32bppBGRA)
                {
                    b = *addr;
                    g = *(addr + 1);
                    r = *(addr + 2);
                    a = *(addr + 3);
                }
            }
        }


        public void GetPixelFast(int px, int py, ref byte r, ref byte g, ref byte b)
        {
            unsafe
            {
                byte* addr = (byte*)(bitmapData.Scan0) + bitmapData.Stride * py + px * (int)format;

                if (format == JBitmapPixelFormat.Format8bpp)
                {
                    r = *addr;
                }
                else if (format == JBitmapPixelFormat.Format24bppBGR)
                {
                    b = *addr;
                    g = *(addr + 1);
                    r = *(addr + 2);
                }
                else if (format == JBitmapPixelFormat.Format32bppBGRA)
                {
                    b = *addr;
                    g = *(addr + 1);
                    r = *(addr + 2);
                }
            }

        }


        /*
         * For 24-bit - just returns blue channel
         * For 32-bit - returns alpha channel
         */
        public void GetPixelFast(int px, int py, ref byte col)
        {
            unsafe
            {
                byte* addr = (byte*)(bitmapData.Scan0) + bitmapData.Stride * py + px * (int)format;

                if (format == JBitmapPixelFormat.Format8bpp)
                {
                    col = *addr;
                }
                else if (format == JBitmapPixelFormat.Format24bppBGR)
                {
                    col = *addr;
                }
                else if (format == JBitmapPixelFormat.Format32bppBGRA)
                {
                    col = *(addr + 3);
                }
            }

        }



        public void PutPixelFast(int px, int py, byte r, byte g, byte b, byte a)
        {
            unsafe
            {
                byte* addr = (byte*)(bitmapData.Scan0) + bitmapData.Stride * py + px * (int)format;

                if (format == JBitmapPixelFormat.Format8bpp)
                {
                    *addr = r;
                }
                else if (format == JBitmapPixelFormat.Format24bppBGR)
                {
                    *addr = b;
                    *(addr + 1) = g;
                    *(addr + 2) = r;
                }
                else if (format == JBitmapPixelFormat.Format32bppBGRA)
                {
                    *addr = b;
                    *(addr + 1) = g;
                    *(addr + 2) = r;
                    *(addr + 3) = a;
                }
            }
        }


        public void PutPixelFast(int px, int py, byte r, byte g, byte b)
        {
            unsafe
            {
                byte* addr = (byte*)(bitmapData.Scan0) + bitmapData.Stride * py + px * (int)format;

                if (format == JBitmapPixelFormat.Format8bpp)
                {
                    *addr = r;
                }
                else if (format == JBitmapPixelFormat.Format24bppBGR)
                {
                    *addr = b;
                    *(addr + 1) = g;
                    *(addr + 2) = r;
                }
                else if (format == JBitmapPixelFormat.Format32bppBGRA)
                {
                    *addr = b;
                    *(addr + 1) = g;
                    *(addr + 2) = r;
                }
            }

        }


        /*
         * For 24-bit - just sets blue channel
         * For 32-bit - sets alpha channel
         */
        public void PutPixelFast(int px, int py, byte col)
        {
            unsafe
            {
                byte* addr = (byte*)(bitmapData.Scan0) + bitmapData.Stride * py + px * (int)format;

                if (format == JBitmapPixelFormat.Format8bpp)
                {
                    *addr = col;
                }
                else if (format == JBitmapPixelFormat.Format24bppBGR)
                {
                    *addr = col;
                }
                else if (format == JBitmapPixelFormat.Format32bppBGRA)
                {
                    *(addr + 3) = col;
                }
            }

        }




    }


    public class JBitmapManager
    {
        List<JBitmap> bitmapList;

        public JBitmapManager()
        {
            bitmapList = new List<JBitmap>();
        }

        public JBitmap Add(JBitmap bmp)
        {
            bitmapList.Add(bmp);
            return bmp;
        }


        public void Remove(JBitmap bmpToRemove)
        {
            bitmapList.Remove(bmpToRemove);
            bmpToRemove.Free();
        }

        public void FreeAll()
        {
            foreach (JBitmap bmp in bitmapList)
            {
                bmp.Free();
            }
        }

    }








}
