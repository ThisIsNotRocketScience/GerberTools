using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;

namespace QuickFont
{
    public class QBitmap
    {
        public Bitmap bitmap;
        public BitmapData bitmapData;


        public QBitmap(string filePath)
        {
            LockBits(new Bitmap(filePath));
        }

        public QBitmap(Bitmap bitmap)
        {
            LockBits(bitmap);
        }


        private void LockBits(Bitmap bitmap)
        {
            this.bitmap = bitmap;
            bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
        }



        public void Clear32(byte r, byte g, byte b, byte a)
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
                        *(sourcePtr + 3) = a;

                        sourcePtr += 4;
                    }
                    sourcePtr += bitmapData.Stride - bitmapData.Width * 4; //move to the end of the line (past unused space)
                }
            }
        }




        /// <summary>
        /// Returns try if the given pixel is empty (i.e. black)
        /// </summary>
        public static unsafe bool EmptyPixel(BitmapData bitmapData, int px, int py)
        {

            byte* addr = (byte*)(bitmapData.Scan0) + bitmapData.Stride * py + px * 3;
            return (*addr == 0 && *(addr + 1) == 0 && *(addr + 2) == 0);

        }

        /// <summary>
        /// Returns try if the given pixel is empty (i.e. alpha is zero)
        /// </summary>
        public static unsafe bool EmptyAlphaPixel(BitmapData bitmapData, int px, int py, byte alphaEmptyPixelTolerance)
        {

            byte* addr = (byte*)(bitmapData.Scan0) + bitmapData.Stride * py + px * 4;
            return (*(addr + 3) <= alphaEmptyPixelTolerance);

        }

        /// <summary>
        /// Blits a block of a bitmap data from source to destination, using the luminance of the source to determine the 
        /// alpha of the target. Source must be 24-bit, target must be 32-bit.
        /// </summary>
        public static void BlitMask(BitmapData source, BitmapData target, int srcPx, int srcPy, int srcW, int srcH, int px, int py)
        {

            int sourceBpp = 3;
            int targetBpp = 4;

            int targetStartX, targetEndX;
            int targetStartY, targetEndY;
            int copyW, copyH;

            targetStartX = Math.Max(px, 0);
            targetEndX = Math.Min(px + srcW, target.Width);

            targetStartY = Math.Max(py, 0);
            targetEndY = Math.Min(py + srcH, target.Height);

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
                byte* sourcePtr = (byte*)(source.Scan0);
                byte* targetPtr = (byte*)(target.Scan0);


                byte* targetY = targetPtr + targetStartY * target.Stride;
                byte* sourceY = sourcePtr + sourceStartY * source.Stride;
                for (int y = 0; y < copyH; y++, targetY += target.Stride, sourceY += source.Stride)
                {

                    byte* targetOffset = targetY + targetStartX * targetBpp;
                    byte* sourceOffset = sourceY + sourceStartX * sourceBpp;
                    for (int x = 0; x < copyW; x++, targetOffset += targetBpp, sourceOffset += sourceBpp)
                    {
                        int lume = *(sourceOffset) + *(sourceOffset + 1) + *(sourceOffset + 2);

                        lume /= 3;

                        if (lume > 255)
                            lume = 255;

                        *(targetOffset + 3) = (byte)lume;

                    }

                }
            }
        }

        /// <summary>
        /// Blits from source to target. Both source and target must be 32-bit
        /// </summary>
        public static void Blit(BitmapData source, BitmapData target, Rectangle sourceRect, int px, int py)
        {
            Blit(source, target, sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height, px, py);
        }

        /// <summary>
        /// Blits from source to target. Both source and target must be 32-bit
        /// </summary>
        public static void Blit(BitmapData source, BitmapData target, int srcPx, int srcPy, int srcW, int srcH, int destX, int destY)
        {

            int bpp = 4;

            int targetStartX, targetEndX;
            int targetStartY, targetEndY;
            int copyW, copyH;

            targetStartX = Math.Max(destX, 0);
            targetEndX = Math.Min(destX + srcW, target.Width);

            targetStartY = Math.Max(destY, 0);
            targetEndY = Math.Min(destY + srcH, target.Height);

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

            int sourceStartX = srcPx + targetStartX - destX;
            int sourceStartY = srcPy + targetStartY - destY;


            unsafe
            {
                byte* sourcePtr = (byte*)(source.Scan0);
                byte* targetPtr = (byte*)(target.Scan0);


                byte* targetY = targetPtr + targetStartY * target.Stride;
                byte* sourceY = sourcePtr + sourceStartY * source.Stride;
                for (int y = 0; y < copyH; y++, targetY += target.Stride, sourceY += source.Stride)
                {

                    byte* targetOffset = targetY + targetStartX * bpp;
                    byte* sourceOffset = sourceY + sourceStartX * bpp;
                    for (int x = 0; x < copyW*bpp; x++, targetOffset ++, sourceOffset ++)
                        *(targetOffset) = *(sourceOffset);

                }
            }
        }
        

        public unsafe void PutPixel32(int px, int py, byte r, byte g, byte b, byte a)
        {
            byte* addr = (byte*)(bitmapData.Scan0) + bitmapData.Stride * py + px * 4;
       
            *addr = b;
            *(addr + 1) = g;
            *(addr + 2) = r;
            *(addr + 3) = a;
        }

        public unsafe void GetPixel32(int px, int py, ref byte r, ref byte g, ref byte b, ref byte a)
        {
            byte* addr = (byte*)(bitmapData.Scan0) + bitmapData.Stride * py + px * 4;
        
            b = *addr;
            g = *(addr + 1);
            r = *(addr + 2);
            a = *(addr + 3); 
        }


        public unsafe void PutAlpha32(int px, int py, byte a)
        {
            *((byte*)(bitmapData.Scan0) + bitmapData.Stride * py + px * 4 + 3) = a;
        }

        public unsafe void GetAlpha32(int px, int py, ref byte a)
        {
            a = *((byte*)(bitmapData.Scan0) + bitmapData.Stride * py + px * 4 + 3);
        }

        public void DownScale32(int newWidth, int newHeight)
        {
            

            QBitmap newBitmap = new QBitmap(new Bitmap(newWidth, newHeight, bitmap.PixelFormat));

            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                throw new Exception("DownsScale32 only works on 32 bit images");

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
            float totalColourWeight = 0f;

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
                    totalColourWeight = 0f;

                    for (int j = top; j <= bottom; j++)
                    {
                        for (int i = left; i <= right; i++)
                        {
                            targetStartX = Math.Max(leftF, i);
                            targetEndX = Math.Min(rightF, i + 1);

                            targetStartY = Math.Max(topF, j);
                            targetEndY = Math.Min(bottomF, j + 1);

                            weight = (targetEndX - targetStartX) * (targetEndY - targetStartY);

                            GetPixel32(i, j, ref r, ref g, ref b, ref a);

                            summedA += weight * a;

                            if (a != 0)
                            {
                                summedR += weight * r;
                                summedG += weight * g;
                                summedB += weight * b;
                                totalColourWeight += weight;
                            }

                        }
                    }

                    summedR /= totalColourWeight;
                    summedG /= totalColourWeight;
                    summedB /= totalColourWeight;
                    summedA /= weightScale;

                    if (summedR < 0) summedR = 0f;
                    if (summedG < 0) summedG = 0f;
                    if (summedB < 0) summedB = 0f;
                    if (summedA < 0) summedA = 0f;

                    if (summedR >= 256) summedR = 255;
                    if (summedG >= 256) summedG = 255;
                    if (summedB >= 256) summedB = 255;
                    if (summedA >= 256) summedA = 255;

                    newBitmap.PutPixel32(n, m, (byte)summedR, (byte)summedG, (byte)summedB, (byte)summedA);
                }

            }
            

            this.Free();
            
            this.bitmap = newBitmap.bitmap;
            this.bitmapData = newBitmap.bitmapData;
        }





        /// <summary>
        /// Sets colour without touching alpha values
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public void Colour32(byte r, byte g, byte b)
        {
            unsafe
            {
                byte* addr;
                for (int i = 0; i < bitmapData.Width; i++)
                {
                    for (int j = 0; j < bitmapData.Height; j++)
                    {
                        addr = (byte*)(bitmapData.Scan0) + bitmapData.Stride * j + i * 4;
                        *addr = b;
                        *(addr + 1) = g;
                        *(addr + 2) = r;
                    }
                }
            }
        }


        /*

        public void Blur(int radius, int passes)
        {
            QBitmap tmp = new QBitmap(new Bitmap(this.bitmap.Width, this.bitmap.Height, bitmap.PixelFormat));

            byte r=0,g=0,b=0,a=0;
            int summedR, summedG, summedB, summedA;
            int weight = 0;
            int xpos, ypos, x, y, kx, ky;


            for (int pass = 0; pass < passes; pass++)
            {

                //horizontal pass
                for (y = 0; y < bitmap.Height; y++)
                {
                    for (x = 0; x < bitmap.Width; x++)
                    {
                        summedR = summedG = summedB = summedA = weight = 0;
                        for (kx = -radius; kx <= radius; kx++)
                        {
                            xpos = x + kx;
                            if (xpos >= 0 && xpos < bitmap.Width)
                            {
                                GetPixel32(xpos, y, ref r, ref g, ref b, ref a);


                                summedR += r;
                                summedG += g;
                                summedB += b;
                                summedA += a;
                                weight++;
                            }

                        }

                        summedR /= weight;
                        summedG /= weight;
                        summedB /= weight;
                        summedA /= weight;

                        tmp.PutPixel32(x, y, (byte)summedR, (byte)summedG, (byte)summedB, (byte)summedA);
                    }
                }



                
                //vertical pass
                for (x = 0; x < bitmap.Width; ++x)
                {
                    for (y = 0; y < bitmap.Height; ++y)
                    {
                        summedR = summedG = summedB = summedA = weight = 0;
                        for (ky = -radius; ky <= radius; ky++)
                        {
                            ypos = y + ky;
                            if (ypos >= 0 && ypos < bitmap.Height)
                            {
                                tmp.GetPixel32(x, ypos, ref r, ref g, ref b, ref a);

                                summedR += r;
                                summedG += g;
                                summedB += b;
                                summedA += a;
                                weight++;
                            }
                        }

                        summedR /= weight;
                        summedG /= weight;
                        summedB /= weight;
                        summedA /= weight;

                        PutPixel32(x, y, (byte)summedR, (byte)summedG, (byte)summedB, (byte)summedA);

                    }
                } 

            }

            tmp.Free();

        }*/




        public void BlurAlpha(int radius, int passes)
        {
            QBitmap tmp = new QBitmap(new Bitmap(this.bitmap.Width, this.bitmap.Height, bitmap.PixelFormat));

            byte a = 0;
            int summedA;
            int weight = 0;
            int xpos, ypos, x, y, kx, ky;
            int width = bitmap.Width;
            int height = bitmap.Height;

            for (int pass = 0; pass < passes; pass++)
            {

                //horizontal pass
                for (y = 0; y < height; y++)
                {
                    for (x = 0; x < width; x++)
                    {
                        summedA = weight = 0;
                        for (kx = -radius; kx <= radius; kx++)
                        {
                            xpos = x + kx;
                            if (xpos >= 0 && xpos < width)
                            {
                                GetAlpha32(xpos, y, ref a);
                                summedA += a;
                                weight++;
                            }
                        }

                        summedA /= weight;
                        tmp.PutAlpha32(x, y, (byte)summedA);
                    }
                }




                //vertical pass
                for (x = 0; x <width; ++x)
                {
                    for (y = 0; y < height; ++y)
                    {
                        summedA = weight = 0;
                        for (ky = -radius; ky <= radius; ky++)
                        {
                            ypos = y + ky;
                            if (ypos >= 0 && ypos < height)
                            {
                                tmp.GetAlpha32(x, ypos,ref a);
                                summedA += a;
                                weight++;
                            }
                        }

                        summedA /= weight;

                        PutAlpha32(x, y, (byte)summedA);

                    }
                }

            }

            tmp.Free();

        }






        public void Free()
        {
            bitmap.UnlockBits(bitmapData);
            bitmap.Dispose();
        }

    }
}
