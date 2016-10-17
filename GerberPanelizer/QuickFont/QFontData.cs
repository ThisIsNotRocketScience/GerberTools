using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace QuickFont
{
    class QFontData : IDisposable
    {

        /// <summary>
        /// Mapping from a pair of characters to a pixel offset
        /// </summary>
        public Dictionary<String, int> KerningPairs;

        /// <summary>
        /// List of texture pages
        /// </summary>
        public TexturePage[] Pages;

        /// <summary>
        /// Mapping from character to glyph index
        /// </summary>
        public Dictionary<char, QFontGlyph> CharSetMapping; 

        /// <summary>
        /// The average glyph width
        /// </summary>
        public float meanGlyphWidth;

        /// <summary>
        /// The maximum glyph height
        /// </summary>
        public int maxGlyphHeight;

        /// <summary>
        /// Null if no dropShadow is available
        /// </summary>
        public QFont dropShadow;

        /// <summary>
        /// Whether the original font (from ttf) was detected to be monospaced
        /// </summary>
        public bool naturallyMonospaced = false;

        /// <summary>
        /// The font scaling due to the font being transformed to the
        /// current viewport for consistent pixel-perfect size across
        /// any resolution
        /// </summary>
        public float scaleDueToTransformToViewport = 1.0f;

        public bool IsMonospacingActive(QFontRenderOptions options)
        {
            return (options.Monospacing == QFontMonospacing.Natural && naturallyMonospaced) || options.Monospacing == QFontMonospacing.Yes; 
        }



        public float GetMonoSpaceWidth(QFontRenderOptions options)
        {
            return (float)Math.Ceiling(1 + (1 + options.CharacterSpacing) * meanGlyphWidth);
        }




        public List<String> Serialize()
        {
            var data = new List<String>();


            data.Add("" + Pages.Length);
            data.Add("" + CharSetMapping.Count);

            foreach (var glyphChar in CharSetMapping)
            {
                var chr = glyphChar.Key;
                var glyph = glyphChar.Value;

                data.Add("" + chr + " " + 
                    glyph.page + " " +
                    glyph.rect.X + " " +
                    glyph.rect.Y + " " +
                    glyph.rect.Width + " " +
                    glyph.rect.Height + " " +
                    glyph.yOffset);
            }
            return data;
        }

        public void Deserialize(List<String> input, out int pageCount, out char[] charSet)
        {
            CharSetMapping = new Dictionary<char, QFontGlyph>();
            var charSetList = new List<char>();

            try
            {
                pageCount = int.Parse(input[0]);
                int glyphCount = int.Parse(input[1]);

                for (int i = 0; i < glyphCount; i++)
                {
                    var vals = input[2 + i].Split(' ');
                    var glyph = new QFontGlyph(int.Parse(vals[1]), new Rectangle(int.Parse(vals[2]), int.Parse(vals[3]), int.Parse(vals[4]), int.Parse(vals[5])), int.Parse(vals[6]), vals[0][0]);

                    CharSetMapping.Add(vals[0][0], glyph);
                    charSetList.Add(vals[0][0]);
                }


            }
            catch (Exception e)
            {
                throw new Exception("Failed to parse qfont file. Invalid format.",e);
            }

            charSet = charSetList.ToArray();

        }

        public void CalculateMeanWidth()
        {
            meanGlyphWidth = 0f;
            foreach (var glyph in CharSetMapping)
                meanGlyphWidth += glyph.Value.rect.Width;

            meanGlyphWidth /= CharSetMapping.Count;

        }


        public void CalculateMaxHeight()
        {
            maxGlyphHeight = 0;
            foreach (var glyph in CharSetMapping)
                maxGlyphHeight = Math.Max(glyph.Value.rect.Height, maxGlyphHeight);

        }


        /// <summary>
        /// Returns the kerning length correction for the character at the given index in the given string.
        /// Also, if the text is part of a textNode list, the nextNode is given so that the following 
        /// node can be checked incase of two adjacent word nodes.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="text"></param>
        /// <param name="textNode"></param>
        /// <returns></returns>
        public int GetKerningPairCorrection(int index, string text, TextNode textNode)
        {
            if (KerningPairs == null)
                return 0;


            var chars = new char[2];

            if (index + 1 == text.Length)
            {
                if (textNode != null && textNode.Next != null && textNode.Next.Type == TextNodeType.Word)
                    chars[1] = textNode.Next.Text[0];
                else
                    return 0;
            }
            else
            {
                chars[1] = text[index + 1];
            }

            chars[0] = text[index];

            String str = new String(chars);


            if (KerningPairs.ContainsKey(str))
                return KerningPairs[str];

            return 0;
            
        }


        public virtual void Dispose()
        {
            foreach (var page in Pages)
                page.Dispose();
        }



    }
}
