using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace GerberLibrary.Core.Primitives
{
    public enum GerberApertureShape
    {
        Circle,
        Rectangle,
        OBround,
        Polygon,
        Undefined,
        Macro,
        Outline,
        Compound,
        Empty
    }


    public class GerberApertureType
    {
        public List<string> Issues = new List<string>();
        public List<GerberApertureType> Parts = new List<GerberApertureType>();
        public static int MacroPostFix = 1;
        public GerberApertureShape ShapeType = GerberApertureShape.Undefined;
        public override string ToString()
        {
            return String.Format("D{0}: {1} ({2})", ID, Enum.GetName(typeof(GerberApertureShape), ShapeType), SourceLine);
        }
        public string BuildGerber(GerberNumberFormat format, double RotationAngle = 0)
        {
            string res = String.Format("%ADD{0}", ID.ToString("D2"));
            int parmcount = 0;
            switch (ShapeType)
            {
                case GerberApertureShape.Macro:

                    res += MacroName;
                    if (MacroParamList.Count > 0) res += ",";
                    foreach (var a in MacroParamList)
                    {
                        parmcount++;
                        if (parmcount >= 2) res += "X";
                        res += Gerber.ToFloatingPointString(format._ScaleMMToFile(a)).Replace(',', '.');
                    }
                    break;
                case GerberApertureShape.Circle:
                    {
                        if (CircleRadius > 3)
                        {
                            //Console.WriteLine("test");
                        }
                        res += "C," + Gerber.ToFloatingPointString(format._ScaleMMToFile(CircleRadius * 2)).Replace(',', '.');
                    }
                    break;
                case GerberApertureShape.Rectangle:
                    if (RotationAngle == 0 || (int)RotationAngle == 180)
                    {
                        res += "R," + Gerber.ToFloatingPointString(format._ScaleMMToFile(RectWidth)).Replace(',', '.') + "X" + Gerber.ToFloatingPointString(format._ScaleMMToFile(RectHeight)).Replace(',', '.');
                    }
                    else
                    {
                        if ((int)RotationAngle == 90 || (int)RotationAngle == (-90) || (int)RotationAngle == (270))
                        {
                            res += "R," + Gerber.ToFloatingPointString(format._ScaleMMToFile(RectHeight)).Replace(',', '.') + "X" + Gerber.ToFloatingPointString(format._ScaleMMToFile(RectWidth)).Replace(',', '.');

                        }
                        else
                        {
                            string macroname = "REC" + (MacroPostFix++).ToString();

                            PolyLine Rect = new PolyLine();
                            Rect.MakeRectangle(RectWidth, RectHeight);
                            Rect.RotateDegrees(RotationAngle);
                            res = Gerber.BuildOutlineApertureMacro(macroname, Rect.Vertices, format) + res;

                            res += macroname;
                            if (Gerber.ShowProgress) Console.WriteLine("generated rotated rect: ");
                            if (Gerber.ShowProgress) Console.WriteLine(res);
                        }
                    }
                    break;
                case GerberApertureShape.Polygon:
                    if (RotationAngle == 0)
                    {
                        res += "P," + Gerber.ToFloatingPointString(format._ScaleMMToFile(NGonRadius * 2)).Replace(',', '.') + "X" + NGonSides.ToString() + "X" + Gerber.ToFloatingPointString(NGonRotation).Replace(',', '.');
                    }
                    else
                    {
                        double newangle = NGonRotation + RotationAngle;
                        while (newangle < 0) newangle += 360;
                        while (newangle > 360) newangle -= 360;
                        res += "P," + Gerber.ToFloatingPointString(format._ScaleMMToFile(NGonRadius * 2)).Replace(',', '.') + "X" + NGonSides.ToString() + "X" + Gerber.ToFloatingPointString(newangle).Replace(',', '.');
                        if (Gerber.ShowProgress) Console.WriteLine("generated rotated NGon: ");
                        if (Gerber.ShowProgress) Console.WriteLine(res);
                    }
                    break;

                case GerberApertureShape.OBround:
                    if (RotationAngle == 0 || Math.Abs(Math.Abs(RotationAngle) - 180) < 0.01)
                    {
                        res += "O," + Gerber.ToFloatingPointString(format._ScaleMMToFile(RectWidth)).Replace(',', '.') + "X" + Gerber.ToFloatingPointString(format._ScaleMMToFile(RectHeight)).Replace(',', '.');
                    }
                    else
                    {
                        if (Math.Abs(Math.Abs(RotationAngle) - 90) < 0.01)
                        {
                            res += "O," + Gerber.ToFloatingPointString(format._ScaleMMToFile(RectHeight)).Replace(',', '.') + "X" + Gerber.ToFloatingPointString(format._ScaleMMToFile(RectWidth)).Replace(',', '.');

                        }
                        else
                        {
                            string macroname = "OBR" + (MacroPostFix++).ToString();


                            PolyLine Obround = new PolyLine();

                            Obround.SetObround(RectWidth, RectHeight);
                            Obround.RotateDegrees(RotationAngle);

                            res = Gerber.BuildOutlineApertureMacro(macroname, Obround.Vertices, format) + res;
                            res += macroname;


                            if (Gerber.ShowProgress) Console.WriteLine("generated rotated obround: ");
                            if (Gerber.ShowProgress) Console.WriteLine(res);
                        }
                    }
                    break;
                case GerberApertureShape.Compound:
                    {
                        string macroname = "COMP" + (MacroPostFix++).ToString();
                        string macrores = Gerber.WriteMacroStart(macroname);
                        foreach (var P in Parts)
                        {
                            PolyLine Comp = new PolyLine();

                            foreach (var a in P.Shape.Vertices)

                            {
                                Comp.Add(a.X, a.Y);
                            }
                            Comp.Close();
                            Comp.RotateDegrees(RotationAngle);

                            macrores += Gerber.WriteMacroPartVertices(Comp.Vertices, format);
                        }

                        macrores += Gerber.WriteMacroEnd();
                        res = macrores + res;
                        res += macroname;


                        if (Parts.Count > 1)
                        {
                            Console.WriteLine("Number of parts: {0} but only 1 written", Parts.Count);
                            Console.WriteLine(res);
                        }

                        // res += MacroName;
                        if (MacroParamList.Count > 0) res += ",";
                        foreach (var a in MacroParamList)
                        {
                            parmcount++;
                            if (parmcount >= 2) res += "X";
                            res += Gerber.ToFloatingPointString(a).Replace(',', '.');
                        }


                        if (Gerber.ShowProgress) Console.WriteLine("generated rotated compound macro shape: ");
                        if (Gerber.ShowProgress) Console.WriteLine(res);
                    }
                    break;

                default:
                    Console.WriteLine("I don't know how to generate the source for this aperture yet.");
                    break;
            }

            res += "*%";

            return res;
        }

        public GerberApertureType()
        {
            Shape = new PolyLine();
            SetRectangle(1, 1);
            ID = 0;
        }

        public PolyLine Shape;
        public int ID;
        public string SourceLine;
        public string MacroName;
        public double NGonDiameter;
        public int NGonXoff;
        public int NGonYoff;
        public List<double> MacroParamList;
        public double CircleRadius;
        private double RectWidth;
        private double RectHeight;
        public void SetCircle(double radius, double xoff = 0, double yoff = 0)
        {
            CircleRadius = Math.Max(0, radius);
            if (CircleRadius == 0)
            {
                Shape.Vertices.Clear();
            }
            else
            {
                NGon((int)Math.Floor(10 * Math.Max(2.0, CircleRadius)), CircleRadius, xoff, yoff);
            };

            ShapeType = GerberApertureShape.Circle;

        }

        public void SetRectangle(double width, double height)
        {
            RectWidth = width;
            RectHeight = height;
            ShapeType = GerberApertureShape.Rectangle;
            Shape.Vertices.Clear();
            double W = (double)width / 2;
            double H = (double)height / 2;
            Shape.Add(W, -H);
            Shape.Add(-W, -H);
            Shape.Add(-W, H);
            Shape.Add(W, H);

        }

        //                public PolyLine BuildFromLine(double x1, double y1, double x2, double y2)
        //              {
        //                return new PolyLine();
        //          }

        public double NGonRadius = 0;
        public int NGonSides = 0;
        public double NGonRotation = 0;
        public bool ZeroWidth = false;

        public void NGon(int sides, double radius, double xoff = 0, double yoff = 0, double rotation = 0)
        {
            NGonRadius = radius;
            NGonSides = sides;
            NGonRotation = rotation;
            ShapeType = GerberApertureShape.Polygon;
            double padd = -rotation * Math.PI * 2.0 / 360;
            Shape.Vertices.Clear();
            for (int i = 0; i < sides; i++)
            {
                double P = i / (double)sides * Math.PI * 2.0 + padd;
                Shape.Add((double)(xoff + Math.Sin(P) * radius), (double)(yoff + Math.Cos(P) * radius));
            }
        }

        public bool isLeft(PointD a, PointD b, PointD c)
        {
            return ((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X)) > 0;
        }


        public void FindExtendIndices(PointD A, PointD B, out int one, out int two)
        {
            double leftdistance = 0;
            double rightdistance = 0;
            int leftindex = 0;
            int rightindex = 0;

            for (int i = 0; i < Shape.Count(); i++)
            {
                PointD C = new PointD(Shape.Vertices[i].X + A.X, Shape.Vertices[i].Y + A.Y);

                //bool side = isLeft(A, B, C);
                double dist = (double)PointD.CrossProduct(A, B, C);
                if (dist > 0)
                {
                    if (rightdistance < dist)
                    {
                        rightdistance = dist;
                        rightindex = i;
                    };
                }
                else
                {
                    dist = -dist;
                    if (leftdistance < dist)
                    {
                        leftdistance = dist;
                        leftindex = i;
                    };
                }
            }

            two = leftindex;
            one = rightindex;
        }

        public string BuildSelectGerber()
        {
            return String.Format("D{0}*", ID.ToString("D2"));
        }

        internal void SetCustom(List<PointD> OutlineVertices)
        {
            ShapeType = GerberApertureShape.Outline;
            Shape.Vertices.Clear();
            foreach (var a in OutlineVertices)
            {
                Shape.Vertices.Add(a);
            }
            // throw new NotImplementedException();
        }

        internal void SetObround(double W, double H)
        {
            RectWidth = W;
            RectHeight = H;

            ShapeType = GerberApertureShape.OBround;

            Shape.SetObround(W, H);

        }

        public List<PolyLine> CreatePolyLineSet(double X, double Y)
        {
            List<PolyLine> Res = new List<PolyLine>();
            if (Parts.Count > 0)
            {
                List<PolyLine> ResPre = new List<PolyLine>();

                foreach (var a in Parts)
                {
                    ResPre.AddRange(a.CreatePolyLineSet(X, Y));
                }
                Polygons Combined = new Polygons();

                Polygons clips = new Polygons();


                foreach (var c in ResPre)
                {
                    //                    c.CheckIfHole();
                    //Clipper cp = new Clipper();
                    //cp.AddPolygons(Combined, PolyType.ptSubject);
                    //clips.Add(c.toPolygon());
                    //cp.AddPolygons(clips, PolyType.ptClip);
                    //cp.Execute(ClipType.ctUnion, Combined, PolyFillType.pftNonZero, PolyFillType.pftEvenOdd);

                    Combined.Add(c.toPolygon());

                }



                foreach (var p in Combined)
                {
                    PolyLine PL = new PolyLine();
                    PL.fromPolygon(p);
                    Res.Add(PL);
                }
            }
            else
            {
                if (Shape.Count() > 0)
                {
                    var PL = new PolyLine();
                    for (int i = 0; i < Shape.Count(); i++)
                    {
                        PL.Add(X + Shape.Vertices[i].X, Y + Shape.Vertices[i].Y);
                    }
                    PL.Add(X + Shape.Vertices[0].X, Y + Shape.Vertices[0].Y);
                    PL.Close();
                    Res.Add(PL);
                }
                else
                {
                    if (Gerber.ShowProgress) Console.WriteLine("creating empty shape?? {0} {1}", MacroName, ShapeType);
                }
            }
            return Res;
        }

        internal void SetRotatedRectangle(double Width, double Height, double Rotation, double Xoff, double Yoff)
        {
            Shape.Vertices.Clear();
            Shape.MakeRectangle(Width, Height);
            Shape.Translate(Xoff, Yoff);
            Shape.RotateDegrees(Rotation);

        }

        internal void SetLineSegment(PointD A, PointD B, double Width, double Rotation)
        {
            var Dir = B - A;
            Dir.Normalize();
            var N = Dir.Rotate(90) * Width * 0.5;

            Shape.Vertices.Clear();

            Shape.Add(A.X - N.X, A.Y - N.Y);
            Shape.Add(A.X + N.X, A.Y + N.Y);
            Shape.Add(B.X + N.X, B.Y + N.Y);
            Shape.Add(B.X - N.X, B.Y - N.Y);

            Shape.Close();

            Shape.RotateDegrees(Rotation);
        }
        internal void SetThermal(double Xoff, double Yoff, double OuterDiameter, double InnerDiameter, double GapWidth, double Rotation)
        {
            Shape.Vertices.Clear();
            Console.WriteLine("TODO! Generate Thermal geometry for further processing!! ");
        }
        internal void SetMoire(double Xoff, double Yoff, double OuterDiameter, double Width, double RingGap, int MaxRings, double CrossHairThickness, double CrossHairLength, double Rotation)
        {
            Shape.Vertices.Clear();
            double Rad = OuterDiameter / 2 - Width / 2;
            int Rings = 0;
            if (CrossHairThickness > 0 && CrossHairLength > 0)
            {
                GerberApertureType L1 = new GerberApertureType();
                L1.SetLineSegment(new PointD(Xoff - CrossHairLength / 2, Yoff), new PointD(Xoff + CrossHairLength / 2, Yoff), CrossHairThickness, Rotation);
                Parts.Add(L1);
                GerberApertureType L2 = new GerberApertureType();
                L2.SetLineSegment(new PointD(Xoff, Yoff - CrossHairLength / 2), new PointD(Xoff, Yoff + CrossHairLength / 2), CrossHairThickness, Rotation);
                Parts.Add(L2);
            }
            while (Rad > 0 && Rings < MaxRings)
            {
                GerberApertureType AT = new GerberApertureType();
                AT.SetRing(Xoff, Yoff, Rad, Width);
                Parts.Add(AT);

                Rad -= RingGap + Width;
                Rings++;
            }

        }

        private void SetRing(double Xoff, double Yoff, double p, double Width)
        {
            GerberApertureType GA = new GerberApertureType();
            GA.NGon((int)Math.Floor(10 * Math.Max(2.0, p)), 1, 0, 0);
            Shape.Vertices.Clear();

            for (int i = 0; i < GA.Shape.Vertices.Count(); i++)
            {
                Shape.Vertices.Add(new PointD(Xoff + GA.Shape.Vertices[i].X * (p - (Width / 2)), Yoff + GA.Shape.Vertices[i].Y * (p - (Width / 2))));
            }
            Shape.Vertices.Add(new PointD(Xoff + GA.Shape.Vertices[0].X * (p + (Width / 2)), Yoff + GA.Shape.Vertices[0].Y * (p + (Width / 2))));
            for (int i = GA.Shape.Vertices.Count() - 1; i >= 0; i--)
            {
                Shape.Vertices.Add(new PointD(Xoff + GA.Shape.Vertices[i].X * (p + (Width / 2)), Yoff + GA.Shape.Vertices[i].Y * (p + (Width / 2))));
            }
            Shape.Vertices.Add(new PointD(Xoff + GA.Shape.Vertices[GA.Shape.Vertices.Count() - 1].X * (p - (Width / 2)), Yoff + GA.Shape.Vertices[GA.Shape.Vertices.Count() - 1].Y * (p - (Width / 2))));
            Shape.Close();
        }
    }

}
