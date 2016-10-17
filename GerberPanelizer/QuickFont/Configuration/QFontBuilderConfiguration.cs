using System;
using System.Collections.Generic;
using System.Text;

namespace QuickFont
{


    public enum TextGenerationRenderHint
    {
        /// <summary>
        /// Use AntiAliasGridFit when rendering the ttf character set to create the QFont texture
        /// </summary>
        AntiAliasGridFit,
        /// <summary>
        /// Use AntiAlias when rendering the ttf character set to create the QFont texture
        /// </summary>
        AntiAlias,
        /// <summary>
        /// Use ClearTypeGridFit if the font is smaller than 12, otherwise use AntiAlias
        /// </summary>
        SizeDependent,
        /// <summary>
        /// Use ClearTypeGridFit when rendering the ttf character set to create the QFont texture
        /// </summary>
        ClearTypeGridFit,
        /// <summary>
        /// Use SystemDefault when rendering the ttf character set to create the QFont texture
        /// </summary>
        SystemDefault
    } 

    /// <summary>
    /// What settings to use when building the font
    /// </summary>
    public class QFontBuilderConfiguration : QFontConfiguration
    {

        /// <summary>
        /// Whether to use super sampling when building font texture pages
        /// 
        /// 
        /// </summary>
        public int SuperSampleLevels = 1;

        /// <summary>
        /// The standard width of texture pages (the page will
        /// automatically be cropped if there is extra space)
        /// </summary>
        public int PageWidth = 512;

        /// <summary>
        /// The standard height of texture pages (the page will
        /// automatically be cropped if there is extra space)
        /// </summary>
        public int PageHeight = 512;

        /// <summary>
        /// Whether to force texture pages to use a power of two.
        /// </summary>
        public bool ForcePowerOfTwo = true;

        /// <summary>
        /// The margin (on all sides) around glyphs when rendered to
        /// their texture page
        /// </summary>
        public int GlyphMargin = 2;
       
        /// <summary>
        /// Set of characters to support
        /// </summary>
        public string charSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.:,;'\"(!?)+-*/=_{}[]@~#\\<>|^%$£&";

        /// <summary>
        /// Which render hint to use when rendering the ttf character set to create the QFont texture
        /// </summary>
        public TextGenerationRenderHint TextGenerationRenderHint = TextGenerationRenderHint.SizeDependent;


        public QFontBuilderConfiguration() { }
        public QFontBuilderConfiguration(bool addDropShadow) : this(addDropShadow, false) { }
        public QFontBuilderConfiguration(bool addDropShadow, bool TransformToOrthogProjection)
        {
            if (addDropShadow)
                this.ShadowConfig = new QFontShadowConfiguration();
            this.TransformToCurrentOrthogProjection = TransformToOrthogProjection;
        }
    }
}
