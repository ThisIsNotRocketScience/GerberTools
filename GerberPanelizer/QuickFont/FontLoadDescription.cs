using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;


namespace QuickFont
{

    enum FontLoadMethod { FontObject, FontFile, QFontFile };
        

    /// <summary>
    /// Describes how a font was loaded so that it can be reloaded
    /// </summary>
    class FontLoadDescription
    {
        public FontLoadMethod Method { get; private set; }
        public String Path { get; private set; }
        public float Size { get; private set;}
        public FontStyle Style {get; private set; }
        public QFontBuilderConfiguration BuilderConfig {get; private set;}
        public float DownSampleFactor {get;private set;}
        public QFontLoaderConfiguration LoaderConfig {get; private set;}

        public FontLoadDescription(String Path, float DownSampleFactor, QFontLoaderConfiguration LoaderConfig){
            Method = FontLoadMethod.QFontFile;

            this.Path = Path;
            this.DownSampleFactor = DownSampleFactor;
            this.LoaderConfig = LoaderConfig;
        }

        public FontLoadDescription(String Path, float Size, FontStyle Style, QFontBuilderConfiguration BuilderConfig){
            Method = FontLoadMethod.FontFile;

            this.Path = Path;
            this.Size = Size;
            this.Style = Style;
            this.BuilderConfig = BuilderConfig;
        }

        public FontLoadDescription(Font font, QFontBuilderConfiguration config){
            Method = FontLoadMethod.FontObject;
            //we don't reload fonts loaded direct from a font object...
        }


    }
}
