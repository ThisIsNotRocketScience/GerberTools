using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberViewer
{
    class FBO : IDisposable
    {
        public int textureId = 0;
        private int fboId = 0;

        public int Width;
        public int Height;

        public FBO(int width, int height)
        {
            Width = width;
            Height = height;

            Init();
        }

        //semi pseudocode
        public void DrawToScreen(float xoffset = 0, float yoffset = 0)
        {
            if (textureId != -1)
            {
                GL.BindTexture(TextureTarget.Texture2D, textureId);

                GL.Begin(BeginMode.Quads);
                //todo : might also flip the texture since fbo's have right handed coordinate systems
                GL.TexCoord2(0.0, 0.0);
                GL.Vertex3(xoffset, yoffset, 0.0);

                GL.TexCoord2(0.0, 1.0);
                GL.Vertex3(xoffset, yoffset + Height, 0.0);

                GL.TexCoord2(1.0, 1.0);
                GL.Vertex3(xoffset + Width, yoffset + Height, 0.0);

                GL.TexCoord2(1.0, 0.0);
                GL.Vertex3(xoffset + Width, yoffset, 0.0);

                GL.End();

            }
        }

        private void Init()
        {
            // Generate the texture.
            textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            // Create a FBO and attach the texture.
            GL.Ext.GenFramebuffers(1, out fboId);
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, fboId);
            GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt,
                FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, textureId, 0);

            // Disable rendering into the FBO
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
        }


        // Track whether Dispose has been called.
        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Clean up what we allocated before exiting
                    if (textureId != 0)
                        GL.DeleteTextures(1, ref textureId);

                    if (fboId != 0)
                        GL.Ext.DeleteFramebuffers(1, ref fboId);

                    disposed = true;
                }
            }
        }

        public void BeginDrawing()
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, fboId);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.PushAttrib(AttribMask.ViewportBit);

            GL.Viewport(0, 0, Width, Height);
        }

        public void EndDrawing()
        {
            GL.PopAttrib();
            GL.Ext.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0); // disable rendering into the FBO
        }
    }

}
