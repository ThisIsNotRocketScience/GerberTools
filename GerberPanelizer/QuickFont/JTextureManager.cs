using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuickFont
{

    /// <summary>
    /// A very simple class for managing a collection of textures.
    /// </summary>
    public class JTextureManager
    {
        List<JTexture> textureList;

        public JTextureManager()
        {
            textureList = new List<JTexture>();
        }

        public JTexture Add(JTexture texture)
        {
            textureList.Add(texture);
            return texture;
        }

        public void Remove(JTexture textureToRemove)
        {
            textureList.Remove(textureToRemove);
            textureToRemove.Free();
        }

        public void FreeAll()
        {
            foreach (JTexture tex in textureList)
            {
                tex.Free();
            }
        }

    }
}
