using System;
using System.Collections.Generic;
using System.Text;

namespace QuickFont
{
    public class QFontConfiguration
    {
        public QFontShadowConfiguration ShadowConfig = null;
        public QFontKerningConfiguration KerningConfig = new QFontKerningConfiguration();

        /// <summary>
        /// Render the font pixel-prefectly at a size in units of the current orthogonal projection, independent of the viewport pixel size.
        /// </summary>
        public bool TransformToCurrentOrthogProjection = false;

    }
}
