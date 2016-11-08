using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GerberLibrary.Core.Primitives;

namespace GerberLibrary
{

    public class SVGGraphicsInterface : Core.GraphicsInterface
    {
        public double Width;
        public double Height;

        public string GetColor(byte r, byte g, byte b)
        {
            return String.Format("#{0:X2}{1:X2}{2:X2}", r, g, b);
        }
        public string GetColor(Color C)
        {
            return GetColor(C.R, C.G, C.B);
        }

        public SVGGraphicsInterface(double v1, double v2)
        {
           Width = v1;
           Height = v2;
        }

        public SVGGraphicsInterface() : base()
        {
        }

        public RectangleF ClipBounds
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public InterpolationMode InterpolationMode
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                //throw new NotImplementedException();
            }
        }

        public bool IsFast
        {
            get
            {
                return false;
            }

            set
            {
                
            }
        }

        Matrix CurrentTransform = new Matrix();
        public Matrix Transform
        {
            get
            {
                return CurrentTransform;
            }

            set
            {
                CurrentTransform = value;
            }
        }

        public Color BackgroundColor = Color.White;
        public List<string> OutputLines = new List<string>();
        public void Clear(Color color)
        {
            BackgroundColor = color;
            //throw new NotImplementedException();
        }

        public void DrawImage(Bitmap MMGrid, float p1, float p2, float p3, float p4)
        {
          
        }

        public void DrawLine(Pen P, PointF p1, PointF p2)
        {

            DrawPolyline(P, new List<PointF>() { p1, p2 });

        }

        private void DrawPolyline(Pen p, List<PointF> TheList, bool closed = false)
        {
            string commands = "";

            PointF[] scaletrns = new PointF[1] { new PointF(0, p.Width) };

            CurrentTransform.TransformPoints(scaletrns);

            scaletrns[0].X -= CurrentTransform.Elements[4];
            scaletrns[0].Y -= CurrentTransform.Elements[5];

            double stroke = Math.Sqrt(scaletrns[0].X * scaletrns[0].X + scaletrns[0].Y* scaletrns[0].Y);
            var list = TheList.ToArray();
            CurrentTransform.TransformPoints(list);
                
            commands += "M" + list[0].X.ToString().Replace(',', '.') + "," + list[0].Y.ToString().Replace(',', '.');
            for (int i = 1; i < list.Count(); i++)
            {
                commands += "L" + list[i].X.ToString().Replace(',', '.') + "," + list[i].Y.ToString().Replace(',', '.');
            }
           // if (closed) commands += "L" + list[0].X.ToString().Replace(',', '.') + "," + list[0].Y.ToString().Replace(',', '.');
            if (closed) commands += "Z";

            
            string setup = String.Format("<path fill=\"none\" stroke=\"{2}\" stroke-width=\"{0}\" stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"{1}\"/>", stroke.ToString().Replace(',', '.'), commands, GetColor(p.Color));
            OutputLines.Add(setup);

            
        }

        private void FillPolyline(Color C, List<PointF> TheList, bool closed = false)
        {
            string commands = "";

            var list = TheList.ToArray();
            CurrentTransform.TransformPoints(list);

            commands += "M" + list[0].X.ToString().Replace(',', '.') + "," + list[0].Y.ToString().Replace(',', '.');
            for (int i = 1; i < list.Count(); i++)
            {
                commands += "L" + list[i].X.ToString().Replace(',', '.') + "," + list[i].Y.ToString().Replace(',', '.');
            }
            // if (closed) commands += "L" + list[0].X.ToString().Replace(',', '.') + "," + list[0].Y.ToString().Replace(',', '.');
            if (closed) commands += "Z";


            string setup = String.Format("<path fill=\"{1}\" d=\"{0}\"/>", commands, GetColor(C));
            OutputLines.Add(setup);


        }

        public void DrawLine(Pen P, float p1, float p2, float p3, float p4)
        {
            DrawLine(P, new PointF(p1, p2), new PointF(p3, p4));

        }

        public void DrawLines(Pen P, PointF[] Points)
        {

            DrawPolyline(P, Points.ToList());
            
        }

        public void DrawPath(Color black, GraphicsPath pATH, float v)
        {
           //pATH.
        }

        public void DrawRectangle(Color color, float x, float y, float w, float h)
        {
            DrawLine(new Pen(color), x, y, x + w, y);
            DrawLine(new Pen(color), x + w, y, x + w, y + h);
            DrawLine(new Pen(color), x + w, y + h, x, y + h);
            DrawLine(new Pen(color), x, y + h, x, y);
        }

        public void DrawRectangle(Color color, float x, float y, float w, float h, float strokewidth = 1)
        {
            DrawLine(new Pen(color, strokewidth), x, y, x+w,y);
            DrawLine(new Pen(color, strokewidth), x+w, y, x+w, y+h);
            DrawLine(new Pen(color, strokewidth), x+w, y+h, x,y+ h);
            DrawLine(new Pen(color, strokewidth), x, y+h, x, y);
        }

        public void DrawString(string text, Font font, SolidBrush solidBrush, float x, float y, StringFormat sF)
        {
            //throw new NotImplementedException();
        }

        public void DrawString(PointD pos, string text, double scale, bool centered, float r = 0.2F, float g = 0.2F, float b = 0.2F, float a = 1)
        {
          //  throw new NotImplementedException();
        }

        public void FillPath(Color c, GraphicsPath gP)
        {
            
            //DrawPolyline(new Pen(Color.Black, 0),
        }

        public void FillRectangle(Color color, float x, float y, float w, float h)
        {
          //  DrawPolyline(new Pen(Color.Black, 0),throw new NotImplementedException();
        }

        public void FillShape(SolidBrush BR, PolyLine Shape)
        {
//            throw new NotImplementedException();
        }

        public PointD MeasureString(string p)
        {
            return new PointD(0, 0);
  //          throw new NotImplementedException();
        }

        public void RotateTransform(float p)
        {
            CurrentTransform.Rotate(p);
            
        }

        public void ScaleTransform(float sx, float sy)
        {
            CurrentTransform.Scale(sx, sy);
            
        }

        public void TranslateTransform(float p1, float p2)
        {
            CurrentTransform.Translate(p1, p2);
        }

        public void Save(string fileName)
        {
            List<string> fileout = new List<string>();
            fileout.Add("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\" >");
            fileout.Add(String.Format("<svg version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" xml:space=\"preserve\" width=\"{0}\" height=\"{1}\">", Width, Height));
            fileout.AddRange(OutputLines);
            fileout.Add("</svg>");
            System.IO.File.WriteAllLines(fileName, fileout);

        }
    }

    class SVGWriter
    {

        
        public static void Write(string filename, int w, int h, List<GerberLibrary.Core.Primitives.PolyLine> Polygons, double strokewidth)
        {
            List<string> OutLines = new List<string>();

            OutLines.Add("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\" >");
            OutLines.Add(String.Format("<svg version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" xml:space=\"preserve\" width=\"{0}\" height=\"{1}\">", w, h));
            Dictionary<int, List<string>> groups = new Dictionary<int, List<string>>();
            for(int i =0;i<20;i++)
            {
                groups[i] = new List<string>();
            }

            List<string> colors = new List<string>();// { "#606060", "#505050", "#404040", "#303030", "#202020", "#101010", "#080808", "#040404", "#020202", "#010101", "#000000", "#000000", "#000000", "#000000", "#000000", "#000000", "#000000", "#000000", "#000000" };

            for (int i =0;i<45;i++)
            {
                byte r = (byte)(Math.Sin(i * 3.0) * 127 + 127);
                byte g = (byte)(Math.Sin(2+ i * 3.0) * 127 + 127);
                byte b = (byte)(Math.Sin(4 +i * 3.0) * 127 + 127);
                colors.Add(String.Format("#{0:X2}{1:X2}{2:X2}", r, g, b));
            }

            foreach (var a in Polygons)
            {
                string commands = "";
                commands += "M" + a.Vertices[0].X.ToString().Replace(',', '.') + "," + a.Vertices[0].Y.ToString().Replace(',', '.');
                for (int i = 1; i < a.Vertices.Count; i++)
                {
                    commands += "L" + a.Vertices[i].X.ToString().Replace(',', '.') + "," + a.Vertices[i].Y.ToString().Replace(',', '.');
                }
                commands += "L" + a.Vertices[0].X.ToString().Replace(',','.') + "," + a.Vertices[0].Y.ToString().Replace(',', '.');
                commands += "Z";
                string setup = String.Format("<path fill=\"none\" stroke=\"{2}\" stroke-width=\"{0}\" stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"{1}\"/>", strokewidth, commands, colors[0]);

                groups[0].Add(setup);
            }

            foreach(var a in groups)
            {
                var L = a.Value;
                if (L.Count > 0)
                {
                    OutLines.Add("<g>");
                    foreach (var p in L) OutLines.Add(p);
                    OutLines.Add("</g>");
                }
            }
            OutLines.Add("</svg>");
            System.IO.File.WriteAllLines(filename, OutLines);
        }
    }
}
