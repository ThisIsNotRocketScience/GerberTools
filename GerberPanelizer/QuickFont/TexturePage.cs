using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using OpenTK;


namespace QuickFont
{
    internal class TexturePage : IDisposable
    {
        int gLTexID;
        int width;
        int height;

        public int GLTexID { get { return gLTexID; } }
        public int Width { get { return width; } }
        public int Height { get { return height; } }


        public TexturePage(string filePath)
        {
            var bitmap = new QBitmap(filePath);
            CreateTexture(bitmap.bitmapData);
            bitmap.Free();
        }

        public TexturePage(BitmapData dataSource)
        {
            CreateTexture(dataSource);
        }


        private void CreateTexture(BitmapData dataSource)
        {

            width = dataSource.Width;
            height = dataSource.Height;

            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.GenTextures(1, out gLTexID);
            GL.BindTexture(TextureTarget.Texture2D, gLTexID);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, dataSource.Scan0);
         //   GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }



        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        //Note: finalizer NOT included. .Net runs finalizer on a separate thread, 
        //which means that it does not know about the OpenGL context.
        /*
        ~TexturePage() 
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }*/

        // The bulk of the clean-up code is implemented in Dispose(bool)
        private bool deletedTexture = false;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing) 
            {
                //dispose managed resources here - if there were any!
            }

            //free managed resources here (if there were any!)
            if (!deletedTexture)
            {
                GL.DeleteTexture(gLTexID);
                deletedTexture = true;
            }
        }

        #endregion


    }
}
