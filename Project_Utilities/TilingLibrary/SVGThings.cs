using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace TINRSART.SVG
{
    public class vec2
    {
        public override string ToString()
        {
            return String.Format("({0},{1})", x, y);
        }
        public vec2(double _x, double _y)
        {
            x = (float)_x;
            y = (float)_y;
        }
        public vec2()
        {
            x = y = 0;
        }
        public float x;
        public float y;

        public static vec2 operator +(vec2 c1, vec2 c2)
        {
            vec2 temp = new vec2();
            temp.x = c1.x + c2.x;
            temp.y = c1.y + c2.y;
            return temp;
        }
        public static vec2 operator -(vec2 c1, vec2 c2)
        {
            vec2 temp = new vec2();
            temp.x = c1.x - c2.x;
            temp.y = c1.y - c2.y;
            return temp;

        }
        public static vec2 operator *(vec2 c1, float c2)
        {
            vec2 temp = new vec2();
            temp.x = c1.x * c2;
            temp.y = c1.y * c2;
            return temp;

        }
        public double length()
        {
            return Math.Sqrt(x * x + y * y);
        }
        public void Normalize()
        {
            double L = length();
            x /= (float)L;
            y /= (float)L;
        }
        public vec2 Normalized()
        {
            return this * (float)(1 / length());
        }

        public double Angle()
        {
            return Math.Atan2(y, x);
        }
    }
    public enum Ordering
    {
        Base,
        SilkPrint,
        Carves,
        Holes,
        Standoffs
    }

    public class Polygon
    {

        public byte r, g, b, a;
        public List<vec2> Vertices = new List<vec2>();
        public int Type = 0;
        public int depth = 0;
        internal bool divided = false;
        public bool Closed = true;
        public bool Filled = true;
        public string Source = "";
        Polygon()
        {
            Source = "Unknown";
        }


        public Polygon(string src)
        {
            Source = src;
        }
        public Ordering ordering = Ordering.Base;
        public bool Stroke = false;
        public double StrokeWidth;
        public byte strokeR;
        public byte strokeG;
        public byte strokeB;
        public byte strokeA;

        public void Draw(Graphics G, float linewidth = 1.0f)
        {
            List<PointF> Points = new List<PointF>();
            foreach (var a in Vertices)
            {
                Points.Add(new PointF(a.x, a.y));

            }
            if (Filled)
            {
                G.FillPolygon(new SolidBrush(Color.FromArgb(a, r, g, b)), Points.ToArray());
            }
            else
            {
                if (Closed)
                {
                    G.DrawPolygon(new Pen(Color.FromArgb(a, r, g, b), linewidth), Points.ToArray());
                }
                else
                {
                    G.DrawLines(new Pen(Color.FromArgb(a, r, g, b), linewidth), Points.ToArray());
                }
            }
        }
        public List<PointF> GetPointList()
        {
            List<PointF> R = new List<PointF>();

            foreach (var a in Vertices)
            {
                R.Add(new PointF((float)a.x, (float)a.y));
            }
            return R;
        }

        public static List<Polygon> FromListOfLists(List<List<PointF>> list, string SourceName)
        {
            List<Polygon> R = new List<Polygon>();
            foreach (var L in list)
            {
                R.Add(Polygon.FromList(L, SourceName));
            }

            return R;
        }

        public static Polygon FromList(List<PointF> l, string SourceName)
        {
            Polygon P = new Polygon(SourceName);
            foreach (var v in l)
            {
                P.Vertices.Add(new vec2() { x = v.X, y = v.Y });
            }
            return P;
        }

        internal void Translate(float dx, float dy)
        {
            foreach (var v in Vertices)
            {
                v.x += dx;
                v.y += dy;
            }

        }

        internal void SetColor(Color c)
        {
            r = c.R;
            g = c.G;
            b = c.B;
            a = c.A;
        }

        internal void MakeRoundedRect(float x1, float y1, float W, float H, float R, int blockRoundingSegments)
        {
            Vertices.Clear();

            Vertices.Add(new vec2(x1 + R, y1));
            Vertices.Add(new vec2(x1 + W - R, y1));

            AddArc(x1 + W - R, y1 + R, R, blockRoundingSegments, Math.PI, Math.PI / 2);

            Vertices.Add(new vec2(x1 + W, y1 + R));
            Vertices.Add(new vec2(x1 + W, y1));

            AddArc(x1 + W - R, y1 + H - R, R, blockRoundingSegments, Math.PI / 2, 0);

            Vertices.Add(new vec2(x1 + W, y1 + H - R));
            Vertices.Add(new vec2(x1 + W - R, y1 + H));

            AddArc(x1 + R, y1 + H - R, R, blockRoundingSegments, 0, -Math.PI / 2);

            Vertices.Add(new vec2(x1 + R, y1 + H));
            Vertices.Add(new vec2(x1, y1 + H - R));

            AddArc(x1 + R, y1 + R, R, blockRoundingSegments, -Math.PI / 2, -Math.PI);

            Vertices.Add(new vec2(x1, y1 + R));


            Closed = true;

        }

        private void AddArc(float cx, float cy, float r, int blockRoundingSegments, double A1, double A2)
        {
            for (int i = 0; i <= blockRoundingSegments; i++)
            {
                double P = A1 + (A2 - A1) * i / (double)blockRoundingSegments;
                Vertices.Add(new vec2(cx + Math.Sin(P) * r, cy + Math.Cos(P) * r));
            }
        }

        public void MakeRect(double x1, double y1, double x2, double y2)
        {
            Vertices.Clear();

            Vertices.Add(new vec2(x1, y1));
            Vertices.Add(new vec2(x2, y1));
            Vertices.Add(new vec2(x2, y2));
            Vertices.Add(new vec2(x1, y2));
            Closed = true;
        }

        public void MakeCircle(double x, double y, double r, int blockRoundingSegments)
        {
            Vertices.Clear();
            for (int i = 0; i < blockRoundingSegments; i++)
            {
                double P = (i * Math.PI * 2) / blockRoundingSegments;
                Vertices.Add(new vec2(Math.Sin(P) * r + x, Math.Cos(P) * r + y));
            }
            Closed = true;

        }

        internal vec2 GetCenter()
        {
            vec2 R = new vec2();
            if (Vertices.Count == 0) return R;
            R.x = Vertices[0].x;
            R.y = Vertices[0].y;
            for (int i = 1; i < Vertices.Count; i++)
            {
                R.x += Vertices[i].x;
                R.y += Vertices[i].y;
            }
            float F = 1.0f / (float)Vertices.Count;
            R.x *= F;
            R.y *= F;

            return R;
        }


        public vec2 GetSize()
        {
            vec2 R = new vec2();
            if (Vertices.Count < 2) return R;

            vec2 min = new vec2(Vertices[0].x, Vertices[0].y);
            vec2 max = new vec2(Vertices[0].x, Vertices[0].y);

            for (int i = 1; i < Vertices.Count; i++)
            {
                if (Vertices[i].x > max.x) max.x = Vertices[i].x; else if (Vertices[i].x < min.x) min.x = Vertices[i].x;
                if (Vertices[i].y > max.y) max.y = Vertices[i].y; else if (Vertices[i].y < min.y) min.y = Vertices[i].y;
            }
            R.x = max.x - min.x;
            R.y = max.y - min.y;
            return R;
        }
    }
    public class TextInstance
    {
        public TextInstance()
        {

        }
        public TextInstance(string t, vec2 p, Color c, StringAlignment sa = StringAlignment.Center, float fs = 4, int l = 0)
        {
            layer = l;
            text = t;
            pos = p;
            color = c;
            Align = sa;
            fontsize = fs;
        }

        public Ordering ordering = Ordering.SilkPrint;
        public StringAlignment Align = StringAlignment.Near;
        public string text;
        public double fontsize = 12;
        public vec2 pos;
        public Color color;
        public int layer;
        internal string orig;
        internal string owner;

        public StringAlignment VerticalAlign = StringAlignment.Near;

        public override string ToString()
        {
            return text + String.Format(" {0},{1}", pos.x, pos.y);
        }
    }

    public class SVGWriter
    {

        public static string N(double val, string unit = "mm")
        {
            return val.ToString().Replace(',', '.') + unit;
        }

        public static string N2(double val, string unit = "mm")
        {
            return val.ToString().Replace(',', '.');
        }

        public static string XmlEscape(string unescaped)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerText = unescaped;
            return node.InnerXml;
        }


        public static void Write(string filename, float w, float h, List<Polygon> Polygons, double strokewidth, Color BG, List<TextInstance> texts, double fontsize = 4, string unit = "mm", float xoff = 0, float yoff = 0, int Layer = -1)
        {
            string bgcolorstring = String.Format("#{0:X2}{1:X2}{2:X2}", BG.R, BG.G, BG.B);

            List<string> OutLines = new List<string>();

            OutLines.Add("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\" >");
            OutLines.Add(String.Format("<svg version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" xml:space=\"preserve\" width=\"{0}\" height=\"{1}\">", N(w, unit), N(h, unit)));
            string fontstring = MakeFontstring("Panton", fontsize, StringAlignment.Center); ;
            OutLines.Add(String.Format(" <style> .small {{ {0} }}</style> ", fontstring));
            //OutLines.Add(String.Format("<rect width=\"100%\" height=\"100%\" fill=\"{0}\" />", bgcolorstring));
            OutLines.Add("<g transform=\"scale(2.8346456)\">");
            Dictionary<int, List<string>> groups = new Dictionary<int, List<string>>();
            Dictionary<int, List<string>> textgroups = new Dictionary<int, List<string>>();
            for (int i = 0; i < 256; i++)
            {
                groups[i] = new List<string>();
                textgroups[i] = new List<string>();
            }
            List<string> colors = new List<string>();// { "#606060", "#505050", "#404040", "#303030", "#202020", "#101010", "#080808", "#040404", "#020202", "#010101", "#000000", "#000000", "#000000", "#000000", "#000000", "#000000", "#000000", "#000000", "#000000" };

            foreach (var a in Polygons)
            {
                if (Layer < 0 || (int)a.ordering == Layer || a.ordering == Ordering.Base)
                    if (a.Vertices.Count > 0)
                    {
                        string colorstring = String.Format("#{0:X2}{1:X2}{2:X2}", a.r, a.g, a.b);
                        string commands = "";
                        commands += "M" + N2(a.Vertices[0].x + xoff, unit) + "," + N2(a.Vertices[0].y + yoff, unit);
                        for (int i = 1; i < a.Vertices.Count; i++)
                        {
                            commands += "L" + N2(a.Vertices[i].x + xoff, unit) + "," + N2(a.Vertices[i].y + yoff, unit);
                        }
                        if (a.Closed)
                        {
                            commands += "L" + N2(a.Vertices[0].x + xoff, unit) + "," + N2(a.Vertices[0].y + yoff, unit);
                            commands += "Z";
                        }
                        if (a.Filled)
                        {
                            string setup = String.Format("<path stroke=\"none\" fill=\"{2}\" stroke-width=\"{0}\" stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"{1}\"/>", N(strokewidth, unit), commands, colorstring);
                            groups[(int)a.ordering].Add(setup);
                        }
                        else
                        {
                            string setup = String.Format("<path fill=\"none\" stroke=\"{2}\" stroke-width=\"{0}\" stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"{1}\"/>", N(strokewidth, unit), commands, colorstring);
                            groups[(int)a.ordering].Add(setup);

                        }
                    }
            }
            foreach (var t in texts)
            {
                if (Layer < 0 || (int)t.ordering == Layer)
                {
                    var a = t.color;
                    string colorstring = String.Format("#{0:X2}{1:X2}{2:X2}", a.R, a.G, a.B);
                    string fontstring2 = MakeFontstring("Panton", t.fontsize, t.Align); ;

                    textgroups[t.layer].Add(string.Format("<text x=\"{0}\" y=\"{1}\" class=\"small\" style=\"{4}\" fill=\"{3}\">{2}</text>", N2(t.pos.x + xoff, unit), N2(t.pos.y + yoff, unit), XmlEscape(t.text), colorstring, fontstring2));
                }
            }

            foreach (var a in groups)
            {
                var L = a.Value;
                var T = textgroups[a.Key];
                if (L.Count > 0 || T.Count > 0)
                {
                    OutLines.Add("<g>");
                    foreach (var p in L) OutLines.Add(p);
                    OutLines.Add("<g>");
                    foreach (var p in T) OutLines.Add(p);
                    OutLines.Add("</g>");
                    OutLines.Add("</g>");
                }
            }
            OutLines.Add("<g>");

            OutLines.Add("</g>");

            OutLines.Add("</g>");
            OutLines.Add("</svg>");
            System.IO.File.WriteAllLines(filename, OutLines);
        }

        private static string MakeFontstring(string v, double fontsize, StringAlignment center, string unit = "mm")
        {
            return String.Format("font: {0} {2}; text-anchor:{1} ;font-weight:bold;", N(fontsize / 2.8346456f, unit), Alignment(center), v);
        }

        private static string Alignment(StringAlignment Align)
        {
            switch (Align)
            {
                case StringAlignment.Center: return "middle";
                case StringAlignment.Near: return "start";
                case StringAlignment.Far: return "end";
            }
            return "middle";
        }
    }


    public static class SVGThings
    {
        abstract class SVGNode
        {
            abstract public void Print(int indent);

            abstract public List<List<Polygon>> GetPolygons();

            abstract public void Transform(Matrix parentmatrix);

            public double ToMM(double dip)
            {
                return (dip / 72.0) * 25.4;
            }
        }

        abstract class SVGStyledNode : SVGNode
        {
            public double outline = 0;
            public Color OutlineColor = Color.Black;
            public Color FillColor = Color.Black;
            public bool FillEnabled = true;
            public Color StrokeColor = Color.Black;
            public double StrokeWidthInMM = 0;
            public bool StrokeEnabled = false;
            public ClipperLib.PolyFillType FillRule = ClipperLib.PolyFillType.pftEvenOdd;
            public static Color ParseColor(string color)
            {
                if (color == null)
                {
                    Console.WriteLine("Error: Null color! Defaulting to lime!");
                    return Color.Lime;
                }

                if (color.StartsWith("rgb"))
                {
                    color = color.Substring(4, color.Length - 5);
                    var S = color.Split(',');
                    Color R = Color.FromArgb(255, int.Parse(S[0]), int.Parse(S[1]), int.Parse(S[2]));
                    return R;
                }

                try
                {
                    return System.Drawing.ColorTranslator.FromHtml(color);
                }
                catch (Exception)
                {
                    // unknown colors end up here... no need to worry, just pass it on to the default color handler which returns 0,0,0 as error-color if it too cant find anything.
                }

                return Color.FromName(color);
            }

            public void DecodeStyle(string style)
            {
                var S = style.Split(';').ToList();
                foreach (var p in S)
                {
                    var a = p.Split(':');
                    if (a.Count() == 2)
                    {
                        switch (a[0])
                        {
                            case "stroke-width":
                                //     Console.WriteLine("strokewidth:   {0}", a[1]);
                                DecodeStrokeWidth(a[1]);

                                break;
                            case "fill-rule":
                                switch (a[1])
                                {
                                    case "nonzero":
                                        FillRule = ClipperLib.PolyFillType.pftNonZero;
                                        break;
                                    case "evenodd":
                                        FillRule = ClipperLib.PolyFillType.pftEvenOdd;
                                        break;
                                }
                                break;
                            case "stroke":
                                if (a[1] == "none")
                                {
                                    StrokeEnabled = false;
                                }
                                else
                                {
                                    StrokeEnabled = true;
                                    StrokeColor = ParseColor(a[1]);
                                }
                                break;
                                break;
                            case "fill":
                                if (a[1] == "none")
                                {
                                    FillEnabled = false;
                                }
                                else
                                {
                                    FillEnabled = true;
                                    FillColor = ParseColor(a[1]);
                                }
                                break;
                            default:
                                Console.WriteLine("unknown: " + p);
                                break;
                        }
                    }
                }

            }

            private void DecodeStrokeWidth(string v)
            {
                v = v.Trim();
                if (v.ToLower().EndsWith("px"))
                {
                    v = v.Substring(0, v.Length - 2).Trim();
                    try
                    {
                        double P = double.Parse(v);
                        StrokeWidthInMM = ToMM(P);
                        if (StrokeWidthInMM > 0) StrokeEnabled = true;
                    }
                    catch (Exception E)
                    {
                        Console.WriteLine(" failed to parse strokewidth {0}", v);
                    }


                }
            }

            public override void Print(int indent)
            {
                Console.WriteLine(new String(' ', indent * 3) + "style: {0},{1},{2}", outline, OutlineColor, FillColor);
            }
        }

        class SVGRect : SVGStyledNode
        {
            public double x, y, width, height;
            public override void Print(int indent)
            {
                Console.WriteLine(new String(' ', indent * 3) + "Rectangle x{0} y{1} w{2} h{3}", x, y, width, height);
                base.Print(indent + 1);
            }
            Polygon UnTransformed = new Polygon("untransformedSVG");
            Polygon Transformed = new Polygon("transformedSVG");

            public override List<List<Polygon>> GetPolygons()
            {
                if (FillEnabled == false) return new List<List<Polygon>>();
                var TransformedPolygons = new List<Polygon>() { Transformed };
                var L = new List<List<Polygon>>();
                L.Add(TransformedPolygons);


                foreach (var p in TransformedPolygons)
                {
                    p.r = FillColor.R;
                    p.g = FillColor.G;
                    p.b = FillColor.B;
                    p.a = FillColor.A;
                    p.strokeR = StrokeColor.R;
                    p.strokeG = StrokeColor.G;
                    p.strokeB = StrokeColor.B;
                    p.strokeA = StrokeColor.A;
                    p.Filled = FillEnabled;
                    p.Stroke = StrokeEnabled;
                    p.StrokeWidth = StrokeWidthInMM;
                }

                return L;
            }



            public override void Transform(Matrix parentmatrix)
            {
                Transformed = new Polygon("transformedSVG");
                foreach (var v in UnTransformed.Vertices)
                {
                    PointF[] P = new PointF[1] { new PointF((float)ToMM(v.x), (float)ToMM(v.y)) };
                    parentmatrix.TransformPoints(P);
                    Transformed.Vertices.Add(new vec2(P[0].X, P[0].Y));
                }

            }

            internal void Setup(string xx, string yy, string ww, string hh)
            {
                try
                {
                    x = double.Parse(xx.Replace('.', ','));
                    y = double.Parse(yy.Replace('.', ','));
                    width = double.Parse(ww.Replace('.', ','));
                    height = double.Parse(hh.Replace('.', ','));

                    UnTransformed.MakeRect(x, y, x + width, y + height);
                }
                catch (Exception)
                {

                }

            }
        }

        class SVGCirc : SVGStyledNode
        {
            public double x, y, radius;
            public override void Print(int indent)
            {
                Console.WriteLine(new String(' ', indent * 3) + "Circle  x{0} y{1} r{2} ", x, y, radius);
                base.Print(indent + 1);
            }
            Polygon UnTransformed = new Polygon("SVG untransformed Circle");
            Polygon Transformed = new Polygon("SVG transformed Circle");

            public override List<List<Polygon>> GetPolygons()
            {
                if (FillEnabled == false) return new List<List<Polygon>>();
                var TransformedPolygons = new List<Polygon>() { Transformed };
                var L = new List<List<Polygon>>();
                L.Add(TransformedPolygons);


                foreach (var p in TransformedPolygons)
                {
                    p.r = FillColor.R;
                    p.g = FillColor.G;
                    p.b = FillColor.B;
                    p.a = FillColor.A;
                    p.strokeR = StrokeColor.R;
                    p.strokeG = StrokeColor.G;
                    p.strokeB = StrokeColor.B;
                    p.strokeA = StrokeColor.A;
                    p.Filled = FillEnabled;
                    p.Stroke = StrokeEnabled;
                    p.StrokeWidth = StrokeWidthInMM;
                }

                return L;
            }



            public override void Transform(Matrix parentmatrix)
            {
                Transformed = new Polygon("TransformedSVGCircle");
                foreach (var v in UnTransformed.Vertices)
                {
                    PointF[] P = new PointF[1] { new PointF((float)ToMM(v.x), (float)ToMM(v.y)) };
                    parentmatrix.TransformPoints(P);
                    Transformed.Vertices.Add(new vec2(P[0].X, P[0].Y));
                }
            }

            internal void Setup(string xx, string yy, string radius)
            {
                try
                {
                    x = double.Parse(xx.Replace('.', ','));
                    y = double.Parse(yy.Replace('.', ','));
                    this.radius = double.Parse(radius.Replace('.', ','));

                    UnTransformed.MakeCircle(x, y, this.radius, 30 * 4);
                }
                catch (Exception)
                {

                }

            }
        }

        class SVGPathCollection : SVGStyledNode
        {
            List<Polygon> TransformedPolygons = new List<Polygon>();
            public override List<List<Polygon>> GetPolygons()
            {
                //                if (FillEnabled == false) return new List<List<Polygon>>();

                List<Polygon> CombinedPolygons = new List<Polygon>();


                Polygons solution = new Polygons();


                ClipperLib.Clipper cp = new ClipperLib.Clipper();


                int first = 0;
                foreach (var p in TransformedPolygons)
                {
                    Polygons clips = new Polygons();

                    List<ClipperLib.IntPoint> clipperpoly = new List<ClipperLib.IntPoint>();
                    foreach (var v in p.Vertices)
                    {
                        clipperpoly.Add(new ClipperLib.IntPoint((long)(v.x * 100000), (long)(v.y * 100000)));
                    }
                    clips.Add(clipperpoly);
                    //                    cp.AddPolygons(clips, (first != 0)?PolyType.ptClip:PolyType.ptSubject);
                    cp.AddPolygons(clips, ClipperLib.PolyType.ptSubject);
                    first++;
                }

                cp.Execute(ClipperLib.ClipType.ctUnion, solution, FillRule, FillRule);


                foreach (var l in solution)
                {
                    Polygon p = new Polygon("SVGPathCollection");

                    foreach (var v in l)
                    {
                        p.Vertices.Add(new vec2(v.X / 100000.0, v.Y / 100000.0));
                    }

                    p.r = FillColor.R;
                    p.g = FillColor.G;
                    p.b = FillColor.B;
                    // p.a = (byte)(FillColor.A/3);
                    // if (FillColor != Color.White) p.a = 0;
                    p.a = FillColor.A;
                    p.Filled = FillEnabled;
                    p.Stroke = StrokeEnabled;
                    p.StrokeWidth = StrokeWidthInMM;
                    p.strokeR = StrokeColor.R;
                    p.strokeG = StrokeColor.G;
                    p.strokeB = StrokeColor.B;
                    p.strokeA = StrokeColor.A;
                    CombinedPolygons.Add(p);
                }


                List<List<Polygon>> P = new List<List<Polygon>>() { CombinedPolygons };

                return P;
            }


            public override void Transform(Matrix parentmatrix)
            {
                TransformedPolygons.Clear();

                foreach (var p in Polygons)
                {
                    TransformedPolygons.Add(TransformPoly(p, parentmatrix));
                }
            }

            public Polygon TransformPoly(Polygon p, Matrix parentmatrix)
            {
                Polygon R = new Polygon("transformedSVG");
                foreach (var v in p.Vertices)
                {
                    PointF[] P = new PointF[1] { new PointF((float)v.x, (float)v.y) };
                    parentmatrix.TransformPoints(P);
                    R.Vertices.Add(new vec2(P[0].X, P[0].Y));
                }
                return R;
            }

            public override void Print(int indent)
            {
                Console.WriteLine(new String(' ', indent * 3) + "Pathcollection");
                base.Print(indent + 1);
                foreach (var p in Polygons)
                {
                    Console.WriteLine(new String(' ', indent * 3) + " poly with {0} vertices", p.Vertices.Count);
                }

            }

            enum TokenType
            {
                NoType,
                Command,
                Number
            }
            class Token
            {

                public void Print()
                {
                    if (Type == TokenType.Number)
                    {
                        Console.WriteLine("{0}", value);
                    }
                    if (Type == TokenType.Command)
                    {
                        Console.WriteLine("{0}", command);
                    }
                }
                public TokenType Type = TokenType.NoType;
                public void DoParse()
                {
                    value = Double.Parse(toparse.Replace('.', ','));
                }
                public bool AddChar(char C)
                {

                    switch (Type)
                    {
                        case TokenType.Command:
                            return false;
                        case TokenType.Number:
                            if (char.IsLetter(C))
                            {
                                if (C == '-' || C == '.' || C == ',')
                                {
                                    if (C != ',') toparse += C;
                                    return true;
                                }
                                DoParse();
                                return false;
                            }
                            else
                            {
                                if (C == ' ')
                                {
                                    DoParse();
                                    return false;
                                }
                                if (C == ',')
                                {
                                    DoParse();
                                    return false;
                                }
                                if (C != ',') toparse += C;

                            }
                            return true;
                        case TokenType.NoType:
                            {
                                if (C == ',') return true;
                                if (C == ' ') return true;
                                string commands = "MmcClLzZ";
                                if (commands.Contains(C))
                                {
                                    Type = TokenType.Command;
                                    command = C;
                                    return true;
                                }
                                else
                                {
                                    Type = TokenType.Number;
                                    AddChar(C);
                                }
                                return true;
                            }
                            break;

                    }
                    return false;

                }




                public string toparse = "";
                public double value;
                public char command;

            }
            List<Token> Tokens = new List<Token>();
            int CurrentToken = 0;
            Token NextToken()
            {
                if (CurrentToken == Tokens.Count) return null;
                return Tokens[CurrentToken++];
            }
            public void ParseSVGPath(string p)
            {
                Token Current = new Token();


                for (int i = 0; i < p.Length; i++)
                {
                    if (Current.AddChar(p[i]) == false)
                    {
                        Tokens.Add(Current);
                        Current = new Token();
                        Current.AddChar(p[i]);
                    }
                }


                if (Current.Type != TokenType.NoType)
                {
                    Current.AddChar(' ');
                    Tokens.Add(Current);
                }

                Token N = NextToken();
                while (N != null)
                {
                    switch (N.command)
                    {
                        case 'M': MoveToAbs(NextToken().value, NextToken().value); break;
                        case 'm': MoveToRel(NextToken().value, NextToken().value); break;
                        case 'L': LineToAbs(NextToken().value, NextToken().value); break;
                        case 'l':
                            LineToRel(NextToken().value, NextToken().value);
                            break;
                        case 'H':
                            HorizToAbs(NextToken().value);
                            break;
                        case 'h':
                            HorizToRel(NextToken().value);
                            break;
                        case 'V':
                            VertToAbs(NextToken().value);
                            break;
                        case 'v':
                            VertToRel(NextToken().value);
                            break;
                        case 'C':
                            CurveToAbs(NextToken().value, NextToken().value, NextToken().value, NextToken().value, NextToken().value, NextToken().value);
                            break;
                        case 'c':
                            CurveToRel(NextToken().value, NextToken().value, NextToken().value, NextToken().value, NextToken().value, NextToken().value);
                            break;
                        case 'Z':
                        case 'z':
                            ClosePoly();
                            break;
                    }
                    N = NextToken();
                }

                ClosePoly();
            }
            double lastx = 0;
            double lasty = 0;
            Polygon CurrentPolygon = new Polygon("untransformedSVG");
            void AddPoint(double x, double y)
            {
                lastx = x;
                lasty = y;


                x /= 72.0;
                y /= 72.0;
                x *= 25.4;
                y *= 25.4;

                CurrentPolygon.Vertices.Add(new vec2(x, y));
            }
            private void ClosePoly()
            {
                if (CurrentPolygon.Vertices.Count == 0) return;
                CurrentPolygon.Closed = true;
                Polygons.Add(CurrentPolygon);
                CurrentPolygon = new Polygon("untransformedSVG");
            }

            private void CurveToRel(double cp1x, double cp1y, double cp2x, double cp2y, double tx, double ty)
            {
                CurveToAbs(cp1x + lastx, cp1y + lasty, cp2x + lastx, cp2y + lasty, tx + lastx, ty + lasty);
            }


            private static double BezierX(double t,
                double x0, double x1, double x2, double x3)
            {
                return (
                    x0 * Math.Pow((1 - t), 3) +
                    x1 * 3 * t * Math.Pow((1 - t), 2) +
                    x2 * 3 * Math.Pow(t, 2) * (1 - t) +
                    x3 * Math.Pow(t, 3)
                );
            }
            private static double BezierY(double t, double y0, double y1, double y2, double y3)
            {
                return (
                    y0 * Math.Pow((1 - t), 3) +
                    y1 * 3 * t * Math.Pow((1 - t), 2) +
                    y2 * 3 * Math.Pow(t, 2) * (1 - t) +
                    y3 * Math.Pow(t, 3)
                );
            }

            private void CurveToAbs(double cp1x, double cp1y, double cp2x, double cp2y, double tx, double ty)
            {

                double sx = lastx;
                double sy = lasty;

                for (double t = 0.0f; t < 1.0; t += 1.0 / 20.0)
                {
                    AddPoint(BezierX(t, sx, cp1x, cp2x, tx), BezierY(t, sy, cp1y, cp2y, ty));
                }

                // Connect to the final point.
                AddPoint(BezierX(1, sx, cp1x, cp2x, tx), BezierY(1, sy, cp1y, cp2y, ty));
            }

            private void VertToRel(double y)
            {

                AddPoint(lastx, lasty + y);
            }

            private void VertToAbs(double y)
            {
                AddPoint(lastx, y);
            }

            private void HorizToRel(double x)
            {
                AddPoint(lastx + x, lasty);
            }

            private void HorizToAbs(double x)
            {
                AddPoint(x, lasty);
            }

            private void LineToRel(double x, double y)
            {
                AddPoint(lastx + x, lasty + y);

            }

            private void LineToAbs(double x, double y)
            {
                AddPoint(x, y);
            }

            private void MoveToRel(double x, double y)
            {
                ClosePoly();
                AddPoint(lastx + x, lasty + y);
            }

            private void MoveToAbs(double x, double y)
            {
                ClosePoly();
                AddPoint(x, y);
            }

            List<Polygon> Polygons = new List<Polygon>();
        }
        class SVGGroup : SVGNode
        {
            public Matrix mTransform = new Matrix();
            public Matrix TransformWithParent = new Matrix();
            public SVGGroup Parent = null;
            public List<SVGNode> Children = new List<SVGNode>();

            public SVGGroup()
            {
                mTransform.Reset();
            }

            public override void Print(int indent)
            {
                Console.WriteLine(new String(' ', indent * 3) + "Group");
                foreach (var c in Children)
                {
                    c.Print(indent + 1);
                }
            }

            internal void ParseTransform(string t)
            {
                t = t.Trim();
                if (t.StartsWith("matrix"))
                {
                    t = t.Substring(7, t.Length - 8);


                    var S = t.Split(',');
                    double[] L = new double[6];
                    for (int i = 0; i < 6; i++)
                    {
                        L[i] = double.Parse(S[i].Replace('.', ','));

                    }
                    mTransform = new Matrix((float)L[0]
                    , (float)L[2]
                    , (float)L[1]
                    , (float)L[3]
                    , (float)ToMM(L[4])
                    , (float)ToMM(L[5]));
                }

            }

            public override void Transform(Matrix parentmatrix)
            {
                TransformWithParent = mTransform.Clone();
                if (parentmatrix != null)
                {
                    //TransformWithParent.Multiply(parentmatrix);

                    TransformWithParent = parentmatrix.Clone();
                    TransformWithParent.Multiply(mTransform);
                }
                foreach (var c in Children)
                {
                    c.Transform(TransformWithParent);
                }
            }

            public override List<List<Polygon>> GetPolygons()
            {
                List<List<Polygon>> R = new List<List<Polygon>>();
                foreach (var c in Children)
                {

                    R.AddRange(c.GetPolygons());
                }
                return R;
            }
        }

        public static List<List<Polygon>> LoadSVGToPolies(string Filename, double xoff, double yoff)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;

            XmlReader reader = XmlReader.Create(Filename, settings);

            reader.MoveToContent();
            // Parse the file and display each of the nodes.
            SVGGroup RootGroup = new SVGGroup();
            SVGGroup CurrentGroup = RootGroup;
            bool clippath = false;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        //                        Console.WriteLine("<{0}>", reader.Name);
                        switch (reader.Name)
                        {

                            case "circle":

                                {
                                    SVGCirc R = new SVGCirc();
                                    var style = reader.GetAttribute("style");
                                    if (style != null) R.DecodeStyle(style);
                                    R.Setup(reader.GetAttribute("cx"), reader.GetAttribute("cy"), reader.GetAttribute("r"));
                                    //                                          < rect x = "336.794" y = "701.551" width = "1.889" height = "56.691" />

                                    if (clippath)
                                    {
                                        Console.WriteLine("circle ignored for clippath");
                                    }
                                    else
                                    {


                                        CurrentGroup.Children.Add(R);
                                    }
                                }
                                break;
                            case "rect":

                                {
                                    SVGRect R = new SVGRect();
                                    var style = reader.GetAttribute("style");
                                    if (style != null) R.DecodeStyle(style);
                                    R.Setup(reader.GetAttribute("x"), reader.GetAttribute("y"), reader.GetAttribute("width"), reader.GetAttribute("height"));
                                    //                                          < rect x = "336.794" y = "701.551" width = "1.889" height = "56.691" />

                                    if (clippath)
                                    {
                                        Console.WriteLine("rect ignored for clippath");
                                    }
                                    else
                                    {


                                        CurrentGroup.Children.Add(R);
                                    }
                                }
                                break;
                            case "clipPath":
                                clippath = true;
                                break;
                            case "g":
                                {
                                    SVGGroup ng = new SVGGroup();

                                    var T = reader.GetAttribute("transform");
                                    if (T != null)
                                    {
                                        ng.ParseTransform(T);
                                    }
                                    else
                                    {
                                        ng.mTransform.Reset();
                                    }


                                    CurrentGroup.Children.Add(ng);
                                    ng.Parent = CurrentGroup;
                                    CurrentGroup = ng;
                                }
                                break;
                            case "path":
                                {
                                    SVGPathCollection R = new SVGPathCollection();

                                    var P = reader.GetAttribute("d");
                                    if (P != null) R.ParseSVGPath(P);

                                    var style = reader.GetAttribute("style");
                                    if (style != null) R.DecodeStyle(style);

                                    if (clippath)
                                    {

                                    }
                                    else
                                    {



                                        CurrentGroup.Children.Add(R);
                                    }
                                }
                                break;
                        }
                        break;
                    case XmlNodeType.Text:
                        //         Console.WriteLine(reader.Value);
                        break;
                    case XmlNodeType.CDATA:
                        //       Console.WriteLine("<![CDATA[{0}]]>", reader.Value);
                        break;
                    case XmlNodeType.ProcessingInstruction:
                        //     Console.WriteLine("<?{0} {1}?>", reader.Name, reader.Value);
                        break;
                    case XmlNodeType.Comment:
                        //   Console.WriteLine("<!--{0}-->", reader.Value);
                        break;
                    case XmlNodeType.XmlDeclaration:
                        // Console.WriteLine("<?xml version='1.0'?>");
                        break;
                    case XmlNodeType.Document:
                        break;
                    case XmlNodeType.DocumentType:
                        //  Console.WriteLine("<!DOCTYPE {0} [{1}]", reader.Name, reader.Value);
                        break;
                    case XmlNodeType.EntityReference:
                        // Console.WriteLine(reader.Name);
                        break;
                    case XmlNodeType.EndElement:
                        switch (reader.Name)
                        {
                            case "clipPath":
                                clippath = false;
                                break;

                            case "g":
                                if (CurrentGroup.Parent != null)
                                {
                                    CurrentGroup = CurrentGroup.Parent;
                                }
                                break;
                        }

                        break;
                }
            }

            Matrix Trans = new Matrix();
            Trans.Translate((float)xoff, (float)yoff);
            RootGroup.Transform(Trans);
            //            RootGroup.Print(0);
            return RootGroup.GetPolygons();
        }

    }
}
