using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GerberLibrary
{
    public enum StringAlign
    {
        TopCenter,
        TopLeft,
        TopRight,
        BottomCenter,
        BottomLeft,
        BottomRight,
        CenterCenter,
        CenterLeft,
        CenterRight
    }

    [Serializable]
    public class XY
    {
        public double X;
        public double Y;

        public override string ToString()
        {
            CultureInfo CI = CultureInfo.InvariantCulture;

            return "(" + X.ToString("N3",CI) + ", " + Y.ToString("N3", CI)+")" ;
        }
    }

    [Serializable]
    public class LineSet
    {
        public List<List<XY>> lines = new List<List<XY>>();
        void AddLine(List<XY> L)
        {
            lines.Add(L);
        }
        public char TheChar;
        public double Advance;
        public double DistancesFromHorizontalBaselineToBlackBoxBottom;
    }

    [Serializable]
    public class FontSet
    {
        public double Height;
        public List<LineSet> TheChars = new List<LineSet>();
        public double BaseLine;
        public double CapsHeight;
        public double ReportedHeight;

        public static FontSet Load(string p)
        {
            if (File.Exists(p) == false)
            {
                string local = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(p));
                if (File.Exists(local))
                {
                    p = local;
                }
                else
                {
                    return null;
                }
            }
            XmlSerializer serializer = new XmlSerializer(typeof(FontSet));

            // Declare an object variable of the type to be deserialized.
            FontSet i;

            using (Stream reader = new FileStream(p, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Call the Deserialize method to restore the object's state.
                i = (FontSet)serializer.Deserialize(reader);
            }
            return i;
        }

        public void Write(string p)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(FontSet));
            TextWriter writer = new StreamWriter(p);
            serializer.Serialize(writer, this);
            writer.Close();
        
        }
        public double StringWidth(string text, double size= 1.0)
        {
            double W = 0;
            double Scaler = size / CapsHeight; ;

            for (int i = 0; i < text.Length; i++)
            {
                char t = text[i];
                var R = GetChar(t);
                if (R != null)
                {
                    W += R.Advance * Scaler;
                }
                else
                {
                    W += size;
                }
            }
            return W;
        }
        public LineSet GetChar(char t)
        {
            foreach(var ls in TheChars)
            {
                if (ls.TheChar == t) return ls;
            }
            return null;
        }
    }
}
