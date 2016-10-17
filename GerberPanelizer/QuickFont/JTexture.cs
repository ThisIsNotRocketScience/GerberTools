using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Input;
using OpenTK.Platform;




namespace QuickFont
{





    public class JTexture
    {

        private bool isSubTexture;

        private float _h;
        public float h
        {
            get { return _h; }
        }

        private float _w;
        public float w
        {
            get { return _w; }
        }

        private int _GLTexID;
        public int GLTexID
        {
            get { return _GLTexID; }
        }

        private bool _hasAlpha;
        public bool hasAlpha
        {
            get { return _hasAlpha; }
        }

        private bool _useAlpha;
        public bool useAlpha
        {
            get { return _useAlpha; }
            //can only use alpha if the texture has alpha (but can turn alpha off 
            //when drawing even if the texture has alpha) 
            set { _useAlpha = value && _hasAlpha; }
        }



        private Vector2 bottomLeft;
        private Vector2 topRight;

        public JTexture()
        {
            _h = _w = 0;
            _GLTexID = -1;
            _useAlpha = false;
            _hasAlpha = false;
        }


        public void SetDrawSize(float width, float height)
        {
            _w = width;
            _h = height;
        }


        #region Drawing

        public void Draw(Vector2 pos)
        {
            Draw(pos,1f);

        }
        public void Draw(Vector2 pos, float scale)
        {
            Draw(pos, new Vector2(scale,scale));
        }


        public void Draw(Vector2 pos, Vector2 scale)
        {
            GL.PushMatrix();
            GL.Translate(pos.X, pos.Y, 0f);
            GL.Scale(scale.X, scale.Y, 1f);
                Draw();
            GL.PopMatrix();

        }





        /// <summary>
        /// Draw using JTexture blending settings (ordinary ones).
        /// </summary>
        public void Draw()
        {
           
            if (_GLTexID == -1)
            {
                return;  //no texture loaded yet
            }

            
            if (_useAlpha)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }
            else
            {
                GL.Disable(EnableCap.Blend);
            }

            DrawBasic();
        
        }


        /// <summary>
        /// Draw using JTexture blending settings (ordinary ones).
        /// </summary>
        public void DrawCentred()
        {
            if (_GLTexID == -1)
            {
                return;  //no texture loaded yet
            }


            if (_useAlpha)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }
            else
            {
                GL.Disable(EnableCap.Blend);
            }

            DrawBasicCentred();

        }








        public void DrawBasicCentred(Vector2 pos, Vector2 scale,float ang)
        {
            GL.PushMatrix();

                GL.Translate(pos.X, pos.Y, 0f);
                GL.Scale(scale.X, scale.Y, 1f);
                GL.Rotate(ang, 0f, 0f, 1f);
                GL.Translate(-w * 0.5f, -h * 0.5f, 0f);

                DrawBasic();
            GL.PopMatrix();
        }



        public void DrawBasicCentred()
        {
            GL.PushMatrix();
                GL.Translate(-w * 0.5f, -h * 0.5f, 0f);
                DrawBasic();
            GL.PopMatrix();
        }


        public void DrawBasic(Vector2 pos)
        {
            DrawBasic(pos, 1f);
        }

        public void DrawBasic(Vector2 pos, float scale)
        {
            DrawBasic(pos, new Vector2(scale, scale));
        }

        public void DrawBasic(Vector2 pos, Vector2 scale)
        {
            GL.PushMatrix();
                GL.Translate(pos.X, pos.Y, 0f);
                GL.Scale(scale.X, scale.Y, 1f);
                DrawBasic();
            GL.PopMatrix();
        }

        /// <summary>
        /// Draws the texture without changing any blending settings. Use this 
        /// when you wish to set the blending settings yourself.
        /// </summary>
        public void DrawBasic()
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, _GLTexID);

            GL.Begin(BeginMode.Quads);
                GL.TexCoord2(bottomLeft); GL.Vertex2(0f, _h);
                GL.TexCoord2(bottomLeft.X, topRight.Y); GL.Vertex2(0, 0);
                GL.TexCoord2(topRight); GL.Vertex2(_w, 0f);
                GL.TexCoord2(topRight.X, bottomLeft.Y); GL.Vertex2(_w, _h);
            GL.End();
        }





        #endregion



        public void Free()
        {

            if (_GLTexID != -1)
            {
                //only free root textures
                if (!isSubTexture) 
                    GL.DeleteTexture(_GLTexID);
            }

        }




        /// <summary>
        /// Construct a JTexture when the GLTexture already exists. texW and texH are the actual
        /// dimensions of the given opengl texture (usually pots). Width and height are always
        /// less than or equal to texW and texH respectively, and are the part of the texture to
        /// be used starting in the bottom left corner.
        /// </summary>
        /// <param name="GLTex"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="alpha"></param>
        public JTexture(int GLTex, float width, float height, bool alpha)
        {
            isSubTexture = false;
            _GLTexID = GLTex;

            _w = width;
            _h = height;
            _hasAlpha = _useAlpha = alpha;

            bottomLeft = new Vector2(0f, 0f);
            topRight = new Vector2(1f, 1f);

        }



        /// <summary>
        /// Constructs a JTexture from a parent texture
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="topLeftX"></param>
        /// <param name="topLeftY"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public JTexture(JTexture parent, float topLeftX, float topLeftY, float width, float height, bool alpha)
        {
            if (topLeftX < 0 || topLeftY < 0 || topLeftX + width > parent.w || topLeftY + height > parent.h)
            {
                throw new ArgumentException("Attempted to define a sub JTexture outside the bounds of the parent");
            }


            isSubTexture = true;
            _GLTexID = parent.GLTexID;
            _hasAlpha = parent.hasAlpha;
            useAlpha = alpha;

            _w = width;
            _h = height;

            float parentTextureW = parent.topRight.X - parent.bottomLeft.X;
            float parentTextureH = parent.topRight.Y - parent.bottomLeft.Y;

            bottomLeft.X = parent.bottomLeft.X + (float)topLeftX * parentTextureW / parent.w;
            topRight.X = parent.bottomLeft.X + (float)(topLeftX + width) * parentTextureW / parent.w;

            bottomLeft.Y = parent.bottomLeft.Y + (1 - (float)(topLeftY + height) / parent.h) * parentTextureH;
            topRight.Y = parent.bottomLeft.Y + (1 - (float)topLeftY / parent.h) * parentTextureH;

        }





        public JTexture(String fileName, bool alpha)
            : this(fileName, alpha, false)
        {
        }




        public JTexture(String fileName, bool alpha, bool padToPowerOfTwo)
        {
            Bitmap bitmapSource = new Bitmap(fileName);

            
            var format = alpha ? System.Drawing.Imaging.PixelFormat.Format32bppArgb : System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            BitmapData dataSource = bitmapSource.LockBits(new Rectangle(0, 0, bitmapSource.Width, bitmapSource.Height),ImageLockMode.ReadOnly, format);

            JTextureConstructorHelper(dataSource, alpha, padToPowerOfTwo);
            
            
            bitmapSource.UnlockBits(dataSource);
        }


        public JTexture(JBitmap jbitmap, bool alpha)
        {
            JTextureConstructorHelper(jbitmap.bitmapData, alpha, false);
        }


        public JTexture(JBitmap jbitmap, bool alpha, bool padToPowerOfTwo)
        {
            JTextureConstructorHelper(jbitmap.bitmapData, alpha, padToPowerOfTwo);
        }



        private void JTextureConstructorHelper(BitmapData dataSource, bool alpha, bool padToPowerOfTwo)
        {

            int texture;
            int bpp;

            Bitmap bitmapTarget;

            _w  = dataSource.Width;
            _h  = dataSource.Height;
            _hasAlpha = _useAlpha = alpha;

           
            isSubTexture = false;

            GL.Enable(EnableCap.Texture2D);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.GenTextures(1, out texture);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

            System.Drawing.Imaging.PixelFormat format1;
            PixelInternalFormat format2;
            OpenTK.Graphics.OpenGL.PixelFormat format3;

            if (alpha)
            {
                format1 = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                format2 = PixelInternalFormat.Rgba;
                format3 = OpenTK.Graphics.OpenGL.PixelFormat.Bgra;
                bpp = 4;
            }
            else
            {
                format1 = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                format2 = PixelInternalFormat.Three;
                format3 = OpenTK.Graphics.OpenGL.PixelFormat.Bgr;
                bpp = 3;
            }

            int targetW, targetH;

            if (!padToPowerOfTwo)
            {
                targetW = (int)_w;
                targetH = (int)_h;
            }
            else
            {
                targetW = JMath.pot((int)_w);
                targetH = JMath.pot((int)_h);

           
            }

            bitmapTarget = new Bitmap(targetW, targetH, format1);
            bottomLeft = new Vector2(0f, 0f);
            topRight = new Vector2((float)_w / targetW, (float)_h / targetH);




            BitmapData dataTarget = bitmapTarget.LockBits(new Rectangle(0, 0, bitmapTarget.Width, bitmapTarget.Height),
                ImageLockMode.ReadWrite, format1);


            //copy source data into the target data, flipping it vertically in the process

            unsafe
            {
                byte* sourcePtr = (byte*)(dataSource.Scan0);
                byte* targetPtr = (byte*)(dataTarget.Scan0);

                targetPtr += dataTarget.Stride * (dataSource.Height - 1); //target moves to start of last line

                for (int i = 0; i < dataSource.Height; i++)
                {
                    for (int j = 0; j < dataSource.Width; j++)
                    {
                        // write the logic implementation here

                        for (int k = 0; k < bpp; k++)
                        {
                            (*targetPtr) = (*sourcePtr);

                            sourcePtr++;
                            targetPtr++;
                        }
                    }
                    sourcePtr += dataSource.Stride - dataSource.Width * bpp; //move to the end of the line (past unused space)
                    targetPtr += dataTarget.Stride - dataSource.Width * bpp; //move to the end of the line (past unused space)

                    targetPtr -= dataTarget.Stride * 2; //move up a line
                }

            }



            GL.TexImage2D(TextureTarget.Texture2D, 0, format2, dataTarget.Width, dataTarget.Height, 0,
                format3, PixelType.UnsignedByte, dataTarget.Scan0);



            bitmapTarget.UnlockBits(dataTarget);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);


            _GLTexID = texture;

        }


        



    }
}
