using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
//using EagleXML;
using System.Drawing.Drawing2D;
using GerberLibrary.EagleXML;
using GerberLibrary.Core;
using GerberLibrary.Core.Primitives;

namespace GerberLibrary
{
    namespace Eagle
    {

        public class ThingInNet
        {
            public string NetName;
        }

        public class RectThing
        {
            public double X;
            public double Y;
            public double W;
            public double H;
            public string Name;
        }

        public class RotationSpec
        {
            public double Degrees;
            public bool Mirrored;
            public bool AllowSpin;

            public static RotationSpec Parse(string inp)
            {
                double degrees = 0;
                bool mirror = inp.IndexOf('M') >= 0;
                bool spin = inp.IndexOf('S') >= 0;
                if (inp.IndexOf('R') >= 0)
                {
                    int idx = 1 + (spin ? 1 : 0) + (mirror ? 1 : 0);
                    degrees = double.Parse(inp.Substring(idx));
                }
                return new RotationSpec() { Degrees = degrees, AllowSpin = spin, Mirrored = mirror };
            }

            public PointF Transform(double x, double y, double offx = 0, double offy = 0)
            {
                double deg = Degrees;
                //  if (rot.Mirrored) deg = -deg;
                double P = (-deg * Math.PI * 2.0) / 360;

                return new PointF(
                    (float)((Mirrored ? -1.0 : 1.0) * (x * Math.Cos(P) + y * Math.Sin(P)) + offx),
                    (float)(-x * Math.Sin(P) + y * Math.Cos(P) + offy)
                    );


            }
        }

        public class SchematicThing : RectThing
        {

        }

        public class BoardThing : RectThing
        {
        }

        public class Wire : ThingInNet
        {
            public string Name;
            public double x1;
            public double y1;
            public double x2;
            public double y2;

        }

        public class Net
        {
            public string Name;
            public List<ThingInNet> Things = new List<ThingInNet>();
        }

        public class DisplayString
        {
            public string Text;
            public double x;
            public double y;
            public double size;
            public textAlign alignment;
        }

        public enum LayerType
        {
            Unknown,
            Copper,
            SolderMask,
            Silk,
            Docu,
            Outline,
            Drill
        }

        public class NetSpec : IEquatable<NetSpec>
        {
            public string Name;
            public double Clearance = 0.2;
            public double MinTraceWidth = 7 * 25.4f / 1000.0f;
            public bool AutoPour;

            public bool Equals(NetSpec other)
            {
                if (Name != other.Name) return false;
                if (Clearance != other.Clearance) return false;
                if (MinTraceWidth != other.MinTraceWidth) return false;
                if (AutoPour != other.AutoPour) return false;
                return true;
            }
        }

        public class LayerSpec : IEquatable<LayerSpec>
        {
            public string name;
            public int layerID;
            public BoardSide Side;
            public LayerType type;
            public static LayerSpec TopCopper = new LayerSpec() { name = "TOPCOPPER", Side = BoardSide.Top, type = LayerType.Copper };
            public static LayerSpec BottomCopper = new LayerSpec() { name = "BOTTOMCOPPER", Side = BoardSide.Top, type = LayerType.Copper };
            public static LayerSpec BothCopper = new LayerSpec() { name = "BOTHCOPPER", Side = BoardSide.Both, type = LayerType.Copper };
            public static LayerSpec NullLayer = new LayerSpec() { name = "Null", layerID = -1, Side = BoardSide.Unknown, type = LayerType.Unknown };

            internal static LayerSpec ParseLayer(string v, List<LayerSpec> layers, LayerType DefaultType = LayerType.Unknown)
            {
                if (v == null || v.Length == 0) return NullLayer;
                foreach (var l in layers)
                {
                    if (l.name == v) return l;
                }
                if (v == "TOPCOPPER")
                {
                    layers.Add(TopCopper);
                    return TopCopper;
                }
                if (v == "BOTTOMCOPPER")
                {
                    layers.Add(BottomCopper);
                    return BottomCopper;
                }
                if (v == "BOTHCOPPER")
                {
                    layers.Add(BothCopper);
                    return BothCopper;
                }
                int parsed = 0;
                int.TryParse(v, out parsed);

                LayerType LT = DefaultType;
                BoardSide Side = BoardSide.Top;
                if (Enum.TryParse<LayerType>(v, out LT))
                {
                    //  Console.WriteLine("found type: {0}", v);
                    if ((int)LT == 1) { LT = LayerType.Copper; Side = BoardSide.Top; };
                    if ((int)LT == 16) { LT = LayerType.Copper; Side = BoardSide.Bottom; };
                    if ((int)LT == 18) { LT = LayerType.Copper; Side = BoardSide.Top; };
                    if ((int)LT == 17) { LT = LayerType.Copper; Side = BoardSide.Top; };
                }

                var LS = new LayerSpec() { name = v, layerID = parsed, type = LT, Side = Side };
                layers.Add(LS);
                return LS;
            }

            public bool Equals(LayerSpec other)
            {
                if (other.name == name && other.layerID == layerID && other.Side == Side && other.type == type) return true; return false;
            }
        }

        public class BoardSpec
        {
            public int CopperLayers;
            public List<LayerSpec> Layers = new List<LayerSpec>();
            public List<SchematicNet> Nets = new List<SchematicNet>();

            public double DefaultClearance = (25.4 / 1000.0) * 8.0;
        }

        public class Shape
        {
            public LayerSpec Layer;
            public LayerSpec GetLayer() { return Layer != null ? Layer : LayerSpec.NullLayer; }
            public String Name = "";
            public SchematicNet Net;

            virtual public List<List<PointF>> GetPolygons() { return new List<List<PointF>>(); }

            public List<PointF> DebugPoints = new List<PointF>();
            public List<DisplayString> DisplayStrings = new List<DisplayString>();

            public List<PointF> GetDebugPoints() { return DebugPoints; }

        }

        public class Drill : Shape
        {

            public Drill(double _x, double _y, double _diameter)
            {
                x = _x;
                y = _y;
                diameter = _diameter;
            }
            public double x;
            public double y;

            public double diameter = 0;

            public override List<List<PointF>> GetPolygons()
            {
                double radius = diameter / 2.0;
                var T = new List<PointF>((int)Math.Ceiling(radius)); ;
                int segs = (int)Math.Max(40, radius * 10);
                for (int i = 0; i < segs; i++)
                {
                    float p = (float)((i * Math.PI * 2.0) / (segs * 1.0));
                    T.Add(new PointF((float)(x + Math.Sin(p) * radius), (float)(y + Math.Cos(p) * radius)));
                }
                return new List<List<PointF>>() { T };

            }
        }

        public class Circle : Shape
        {
            public double x;
            public double y;
            public double strokewidth;

            public Circle(double _x, double _y, double _radius)
            {
                x = _x;
                y = _y;
                radius = _radius;

            }

            public double radius;

            public override List<List<PointF>> GetPolygons()
            {
                if (strokewidth == 0)
                {
                    var T = new List<PointF>((int)Math.Ceiling(radius)); ;
                    int segs = (int)Math.Max(40, radius * 10);
                    for (int i = 0; i < segs; i++)
                    {
                        float p = (float)((i * Math.PI * 2.0) / (segs * 1.0));
                        T.Add(new PointF((float)(x + Math.Sin(p) * radius), (float)(y + Math.Cos(p) * radius)));
                    }
                    return new List<List<PointF>>() { T };
                }
                else
                {
                    var T = new List<PointF>((int)Math.Ceiling(radius)); ;
                    var T2 = new List<PointF>((int)Math.Ceiling(radius)); ;
                    int segs = (int)Math.Max(40, radius * 10);
                    double r1 = radius + strokewidth / 2;
                    double r2 = radius - strokewidth / 2;

                    for (int i = 0; i < segs; i++)
                    {
                        float p = (float)((i * Math.PI * 2.0) / (segs * 1.0));
                        T.Add(new PointF((float)(x + Math.Sin(p) * r1), (float)(y + Math.Cos(p) * r1)));
                        T2.Add(new PointF((float)(x + Math.Sin(p) * r2), (float)(y + Math.Cos(p) * r2)));
                    }
                    return new List<List<PointF>>() { T, T2 };
                }


            }

        }

        public class Rectangle : Shape
        {
            public Rectangle(double _x1, double _y1, double _x2, double _y2, RotationSpec Rot)
            {
                x1 = _x1;
                y1 = _y1;
                x2 = _x2;
                y2 = _y2;
                rot = Rot;
            }
            public double x1;
            public double y1;
            public double x2;
            public double y2;
            public RotationSpec rot;

            public float CornerRadius = 0;

            public override List<List<PointF>> GetPolygons()
            {
                if (CornerRadius == 0)
                {
                    double W = x2 - x1;
                    double H = y2 - y1;
                    double midx = x1 + W / 2;
                    double midy = y1 + H / 2;

                    return new List<List<PointF>>() { new List<PointF>()
            {
                ShapeContainer.ToRotatedPointF(-W/2, -H/2, rot, midx, midy),
                ShapeContainer.ToRotatedPointF( W/2, -H/2, rot, midx, midy),
                ShapeContainer.ToRotatedPointF( W/2,  H/2, rot, midx, midy),
                ShapeContainer.ToRotatedPointF(-W/2,  H/2, rot, midx, midy)
            } };
                }
                else
                {
                    List<List<PointF>> P = new List<List<PointF>>();
                    var p = new List<PointF>();

                    double W = x2 - x1;
                    double H = y2 - y1;
                    double midx = x1 + W / 2;
                    double midy = y1 + H / 2;

                    double xx1 = -W / 2 + CornerRadius;
                    double xx2 = W / 2 - CornerRadius;
                    double yy1 = -H / 2 + CornerRadius;
                    double yy2 = H / 2 - CornerRadius;

                    double x = xx1;
                    double y = yy1;

                    // p.Add(ShapeContainer.ToRotatedPointF(x,y, rot, midx, midy));
                    for (int i = 0; i < 10; i++)
                    {
                        double r = (i * Math.PI / 2.0) / 10.0f;
                        double sr = Math.Sin(r) * CornerRadius;
                        double cr = Math.Cos(r) * CornerRadius;
                        p.Add(ShapeContainer.ToRotatedPointF(x - cr, y - sr, rot, midx, midy));

                    }

                    x = xx2;
                    y = yy1;


                    for (int i = 0; i < 10; i++)
                    {
                        double r = (i * Math.PI / 2.0) / 10.0f;
                        double sr = Math.Sin(r) * CornerRadius;
                        double cr = Math.Cos(r) * CornerRadius;
                        p.Add(ShapeContainer.ToRotatedPointF(x + sr, y - cr, rot, midx, midy));

                    }
                    x = xx2;
                    y = yy2;

                    for (int i = 0; i < 10; i++)
                    {
                        double r = (i * Math.PI / 2.0) / 10.0f;
                        double sr = Math.Sin(r) * CornerRadius;
                        double cr = Math.Cos(r) * CornerRadius;
                        p.Add(ShapeContainer.ToRotatedPointF(x + cr, y + sr, rot, midx, midy));

                    }
                    x = xx1;
                    y = yy2;

                    for (int i = 0; i < 10; i++)
                    {
                        double r = (i * Math.PI / 2.0) / 10.0f;
                        double sr = Math.Sin(r) * CornerRadius;
                        double cr = Math.Cos(r) * CornerRadius;
                        p.Add(ShapeContainer.ToRotatedPointF(x - sr, y + cr, rot, midx, midy));

                    }


                    P.Add(p);
                    return P;

                }
            }
        }

        public class PadShape : Shape
        {
            public PadShape(double _x, double _y, RotationSpec _rot, EagleXML.padShape _type, double _diameter)
            {
                x = _x;
                y = _y;
                rot = _rot;
                type = _type;
                diameter = _diameter;
                //Console.WriteLine(type);
            }

            public double diameter;
            public double x;
            public double y;
            public RotationSpec rot;
            public EagleXML.padShape type;

            public override List<List<PointF>> GetPolygons()
            {

                double px1 = -diameter / 2.0;
                double px2 = +diameter / 2.0;
                double py1 = -diameter / 2.0;
                double py2 = +diameter / 2.0;

                switch (type)
                {
                    case EagleXML.padShape.@long:
                        {
                            List<PointF> R = new List<PointF>();
                            for (int i = 0; i < 20; i++)
                            {
                                double P = (i) * Math.PI * 2.0 / 40.0;
                                R.Add(ShapeContainer.ToRotatedPointF(diameter * .5 + Math.Sin(P) * diameter * 0.5, Math.Cos(P) * diameter * 0.5, rot, x, y));
                            }
                            for (int i = 20; i < 40; i++)
                            {
                                double P = (i) * Math.PI * 2.0 / 40.0;
                                R.Add(ShapeContainer.ToRotatedPointF(-diameter * .5 + Math.Sin(P) * diameter * 0.5, Math.Cos(P) * diameter * 0.5, rot, x, y));
                            }
                            return new List<List<PointF>>() { R };
                        }
                    case EagleXML.padShape.offset:
                        {
                            List<PointF> R = new List<PointF>();
                            for (int i = 0; i < 20; i++)
                            {
                                double P = (i) * Math.PI * 2.0 / 40.0;
                                R.Add(ShapeContainer.ToRotatedPointF(Math.Sin(P) * diameter * 0.5, Math.Cos(P) * diameter * 0.5, rot, x, y));
                            }
                            for (int i = 20; i < 40; i++)
                            {
                                double P = (i) * Math.PI * 2.0 / 40.0;
                                R.Add(ShapeContainer.ToRotatedPointF(-diameter + Math.Sin(P) * diameter * 0.5, Math.Cos(P) * diameter * 0.5, rot, x, y));
                            }
                            return new List<List<PointF>>() { R };
                        }
                    case padShape.octagon:
                        {
                            List<PointF> R = new List<PointF>();
                            double adjdiameter = diameter * 1.077;
                            for (int i = 0; i < 8; i++)
                            {
                                double P = (i) * Math.PI * 2.0 / 8 + Math.PI / 8.0;
                                R.Add(ShapeContainer.ToRotatedPointF(Math.Sin(P) * adjdiameter * 0.5, Math.Cos(P) * adjdiameter * 0.5, rot, x, y));
                            }

                            return new List<List<PointF>>() { R };
                        }
                        ;
                    case padShape.square:
                        {
                            List<PointF> R = new List<PointF>();
                            double adjdiameter = diameter * 1.414;
                            for (int i = 0; i < 4; i++)
                            {
                                double P = (i) * Math.PI * 2.0 / 4 + Math.PI / 4.0;
                                R.Add(ShapeContainer.ToRotatedPointF(Math.Sin(P) * adjdiameter * 0.5, Math.Cos(P) * adjdiameter * 0.5, rot, x, y));
                            }

                            return new List<List<PointF>>() { R };
                        }


                    case EagleXML.padShape.round:
                    default:
                        {
                            List<PointF> R = new List<PointF>();
                            for (int i = 0; i < 40; i++)
                            {
                                double P = (-i) * Math.PI * 2.0 / 40.0;
                                R.Add(ShapeContainer.ToRotatedPointF(Math.Sin(P) * diameter * 0.5, Math.Cos(P) * diameter * 0.5, rot, x, y));
                            }

                            return new List<List<PointF>>() { R };
                        }

                }

            }

        }

        public class Polygon : Shape
        {

            public Polygon(List<PointF> _Vertices, double _width, double _spacing)
            {
                Vertices = _Vertices;
                width = _width;
                spacing = _spacing;
            }
            public double width;
            public double spacing;
            List<PointF> Vertices = new List<PointF>();

            public override List<List<PointF>> GetPolygons()
            {
                return new List<List<PointF>>() { Vertices };

            }
        }


        public class SMDShape : Shape
        {

            public SMDShape(double _x, double _y, RotationSpec _rot, double _roundness, double _dx, double _dy, string name, LayerSpec layer, EagleXML.smdCream cream)
            {
                x = _x;
                y = _y;
                rot = _rot;
                dx = _dx;
                dy = _dy;
                Name = name;
                Cream = cream;
                Roundness = _roundness;
                Layer = layer;
            }
            public EagleXML.smdCream Cream;
            public double x;
            public double y;
            public RotationSpec rot;
            public double dx;
            public double dy;
            public double Roundness;


            public override List<List<PointF>> GetPolygons()
            {
                double hx = dx / 2.0;
                double hy = dy / 2.0;

                if (Roundness > 0)
                {
                    GerberLibrary.Core.Primitives.PolyLine P = new GerberLibrary.Core.Primitives.PolyLine(PolyLine.PolyIDs.Temp);
                    PointD TopLeft = new PointD(-hx, -hy);
                    PointD BottomRight = new PointD(hx, hy);
                    double mm = Math.Min((BottomRight.Y - TopLeft.Y), (BottomRight.X - TopLeft.X)) * (Roundness) / 200.0f;

                    P.MakeRoundedRect(TopLeft, BottomRight, mm);
                    List<PointF> R = new List<PointF>();

                    foreach (var a in P.Vertices)
                    {
                        R.Add(ShapeContainer.ToRotatedPointF(a.X, a.Y, rot, x, y));
                    }

                    return new List<List<PointF>>() { R };

                }
                else
                {
                    return new List<List<PointF>>()
                { new List<PointF>()
                    {
                           ShapeContainer.ToRotatedPointF(-hx,-hy,rot,x,y),
                           ShapeContainer.ToRotatedPointF(hx,-hy,rot,x,y),
                           ShapeContainer.ToRotatedPointF(hx,hy,rot,x,y),
                           ShapeContainer.ToRotatedPointF(-hx,hy,rot,x,y),
                    }
                };
                }
            }
        }

        public class WireShape : Shape
        {
            public WireShape(double _x1, double _y1, double _x2, double _y2, double _width, EagleXML.wireCap _cap, double _curve)
            {
                x1 = _x1;
                y1 = _y1;
                x2 = _x2;
                y2 = _y2;
                Curve = _curve;
                width = _width;
                cap = _cap;
            }

            public double width;
            public EagleXML.wireCap cap;
            public double Curve;
            public double x1;
            public double y1;
            public double x2;
            public double y2;

            void AddDebugLine(double ax, double ay, double bx, double by, int cnt = 20)
            {
                for (int i = 1; i < cnt; i++)
                {
                    float p = i * (1.0f / (float)cnt); ;
                    double dx1 = ax + p * (bx - ax);
                    double dy1 = ay + p * (by - ay);
                    DebugPoints.Add(new PointF((float)dx1, (float)dy1));
                }
            }

            public override List<List<PointF>> GetPolygons()
            {
                DebugPoints.Clear();

                List<PointF> R = new List<PointF>();
                double _dx = x2 - x1;
                double _dy = y2 - y1;
                double radrot = Math.Atan2(-_dy, -_dx);

                double L = Math.Sqrt(_dx * _dx + _dy * _dy);
                // Console.WriteLine("curve: {0}", Curve);
                if (Curve == 0)
                {

                    double h = width / 2.0;
                    double dhx = (-_dy / L) * h;
                    double dhy = (_dx / L) * h;

                    AddCap(R, x1, y1, h, radrot);
                    R.Add(new PointF((float)(x2 - dhx), (float)(y2 - dhy)));
                    R.Add(new PointF((float)(x1 - dhx), (float)(y1 - dhy)));
                    AddCap2(R, x2, y2, h, radrot);
                    R.Add(new PointF((float)(x1 + dhx), (float)(y1 + dhy)));
                    R.Add(new PointF((float)(x2 + dhx), (float)(y2 + dhy)));


                }
                else
                {
                    //  return R;
                    bool lefthand = false;
                    double lx = x1;
                    double ly = y1;
                    double endx = x2;
                    double endy = y2;
                    double curve = Curve;
                    if (curve < 0)
                    {
                        lefthand = true;

                        double T = lx;
                        lx = endx;
                        endx = T;
                        T = ly;
                        ly = endy;
                        endy = T;
                        curve *= -1;
                    }
                    else
                    {

                    }

                    List<PointF> Outer = new List<PointF>();
                    List<PointF> Inner = new List<PointF>();


                    double theta = curve * Math.PI * 2.0 / 360.0;
                    double sintheta = Math.Sin(theta / 2.0);
                    double tantheta = Math.Tan(theta / 2.0);

                    double dx = endx - lx;
                    double dy = endy - ly;

                    double O = Math.Sqrt(dx * dx + dy * dy);
                    double A = ((O / 2) / tantheta);
                    double S = (O / 2) / sintheta;

                    double sx = dy / O;
                    double sy = -dx / O;
                    double cx = lx - sx * A + dx / 2;
                    double cy = ly - sy * A + dy / 2;

                    if (lefthand)
                    {
                        cx = lx - sx * A + dx / 2;
                        cy = ly - sy * A + dy / 2;

                    }

                    // DebugPoints.Add(new PointF((float)cx, (float)cy));
                    //AddDebugLine(cx, cy, x1, y1);
                    //AddDebugLine(cx, cy, x2, y2);


                    double ldx = lx - cx;
                    double ldy = ly - cy;
                    double edx = endx - cx;
                    double edy = endy - cy;

                    double sh = Math.Atan2(ldy, ldx);
                    double esh = Math.Atan2(edy, edx);

                    double dsh = esh - sh;

                    //AddDebugLine(cx, cy, cx + Math.Cos(sh),  cy + Math.Sin(sh));
                    //AddDebugLine(cx, cy, cx + Math.Cos(esh), cy + Math.Sin(esh));



                    // A *= 2;
                    int curvelength = (int)Math.Ceiling((Math.Abs(curve) * 2));
                    double HW = width / 2.0;
                    for (int i = 0; i < curvelength; i++)
                    {
                        double P = sh + (curve * i * Math.PI * 2) / (curvelength * 360.0);

                        Outer.Add(new PointF((float)(Math.Cos(P) * (S + HW) + cx), (float)(Math.Sin(P) * (S + HW) + cy)));
                        Inner.Add(new PointF((float)(Math.Cos(P) * (S - HW) + cx), (float)(Math.Sin(P) * (S - HW) + cy)));
                    }



                    R.AddRange(Outer);
                    if (lefthand)
                    {
                        AddCap2(R, x1, y1, HW, esh + (-Math.PI / 2), false, cap);
                    }
                    else
                    {
                        AddCap(R, x2, y2, HW, esh - (-Math.PI / 2), lefthand, cap);
                    }
                    Inner.Reverse();
                    R.AddRange(Inner);
                    if (lefthand)
                    {
                        AddCap(R, x2, y2, HW, sh - Math.PI / 2, false, cap);
                    }
                    else
                    {
                        AddCap2(R, x1, y1, HW, sh + Math.PI / 2, lefthand, cap);
                    }


                }

                return new List<List<PointF>>() { R };
            }

            private void AddCap(List<PointF> R, double cx, double cy, double h, double rot, bool reverse = false, EagleXML.wireCap cap = wireCap.round)
            {
                List<PointF> set = new List<PointF>();
                if (cap == wireCap.round)
                {
                    for (int i = 0; i < 40; i++)
                    {
                        double P = i * Math.PI / 40.0f;
                        set.Add(new PointF((float)(cx + Math.Sin(P + rot) * h), (float)(cy - Math.Cos(P + rot) * h)));
                    }
                    if (reverse)
                    {
                        set.Reverse();
                    }
                }
                R.AddRange(set);
            }

            private void AddCap2(List<PointF> R, double cx, double cy, double h, double rot, bool reverse = false, EagleXML.wireCap cap = wireCap.round)
            {
                List<PointF> set = new List<PointF>();
                if (cap == wireCap.round)
                {

                    for (int i = 0; i < 40; i++)
                    {
                        double P = i * Math.PI / 40.0f;
                        set.Add(new PointF((float)(cx - Math.Sin(P + rot) * h), (float)(cy + Math.Cos(P + rot) * h)));

                    }

                    if (reverse)
                    {
                        set.Reverse();
                    }
                }
                R.AddRange(set);
            }
        }


        public class ShapeContainer
        {
            public static PointF ToRotatedPointF(double x, double y, double rot, double offx = 0, double offy = 0)
            {
                double P = (rot * Math.PI * 2.0) / 360;

                return new PointF(
                    (float)(x * Math.Cos(P) + y * Math.Sin(P) + offx),
                    (float)(-x * Math.Sin(P) + y * Math.Cos(P) + offy)
                   );
            }

            public static PointF ToRotatedPointF(double x, double y, RotationSpec rot, double offx = 0, double offy = 0)
            {
                double deg = rot.Degrees;
                //  if (rot.Mirrored) deg = -deg;
                double P = (-deg * Math.PI * 2.0) / 360;

                return new PointF(
                    (float)(x * Math.Cos(P) + y * Math.Sin(P) + offx),
                    (float)(-x * Math.Sin(P) + y * Math.Cos(P) + offy)
                   );
            }

            public string Name;
            public List<Shape> Shapes = new List<Shape>();
            public Dictionary<string, List<Shape>> NamedShapes = new Dictionary<string, List<Shape>>();

            internal void AddHole(double x, double y, double drillwidth, LayerSpec layer, string shapename)
            {
                AddShape(new Drill(x, y, drillwidth) { Layer = layer }, shapename);

            }

            public void AddShape(Shape shape, string shapename)
            {
                Shapes.Add(shape);
                if (shapename != null) shape.Name = shapename; else shape.Name = "";
                if (shapename != null && shapename.Length > 0)
                {
                    if (NamedShapes.ContainsKey(shapename) == false)
                    {
                        NamedShapes[shapename] = new List<Shape>();
                    }
                    NamedShapes[shapename].Add(shape);
                }
            }

            internal void AddCircle(double x, double y, double radius, double strokewidth, LayerSpec layer, string shapename)
            {
                AddShape(new Circle(x, y, radius) { strokewidth = strokewidth, Layer = layer }, shapename);
            }

            internal void AddPad(string name, double x, double y, RotationSpec rot, EagleXML.padShape padshape, string p6, EagleXML.padThermals thermals, double drillwidth, string diameter, string p9, LayerSpec layer, BoardSpec BS, string shapename)
            {
                double dia = EagleLoader.ParseDouble(diameter);
                if (diameter == "auto" || diameter == "0")
                {
                    dia = drillwidth * 1.5;

                    dia = Math.Min(drillwidth + 25.0 / 25.40, dia);
                    dia = Math.Max(drillwidth + 10.9 / 25.40, dia);
                }

                AddShape(new Drill(x, y, drillwidth) { Layer = LayerSpec.ParseLayer("DRILL", BS.Layers) }, shapename);
                //           AddShape(new PadShape(x, y, rot, padshape, dia) { Layer = LayerSpec.ParseLayer("TOPCOPPER", BS.Layers, LayerType.Copper) }, shapename);
                AddShape(new PadShape(x, y, rot, padshape, dia) { Layer = LayerSpec.ParseLayer("BOTHCOPPER", BS.Layers, LayerType.Copper) }, shapename);
                AddShape(new PadShape(x, y, rot, padshape, dia) { Layer = LayerSpec.ParseLayer("TOPSOLDERMASK", BS.Layers) }, shapename);
                AddShape(new PadShape(x, y, rot, padshape, dia) { Layer = LayerSpec.ParseLayer("BOTTOMSOLDERMASK", BS.Layers) }, shapename);
            }

            internal void AddRectangle(LayerSpec layer, double x1, double y1, double x2, double y2, RotationSpec Rot, string shapename)
            {
                AddShape(new Rectangle(x1, y1, x2, y2, Rot) { Layer = layer }, shapename);
            }

            internal void AddPoly_O(LayerSpec layer, EagleXML.vertex[] vertex, double spacing, double width, string shapename)
            {
                List<PointF> Verts = new List<PointF>();
                foreach (var v in vertex)
                {
                    Verts.Add(new PointF((float)EagleLoader.ParseDouble(v.x), (float)EagleLoader.ParseDouble(v.y)));
                }
                AddShape(new Polygon(Verts, width, spacing) { Layer = layer }, shapename);
            }

            internal void AddWire(double x1, double y1, double x2, double y2, double width, LayerSpec layer, EagleXML.wireCap cap, double curve, string shapename)
            {
                AddShape(new WireShape(x1, y1, x2, y2, width, cap, curve) { Layer = layer }, shapename);

            }

            public void AddPoly(LayerSpec layer, EagleXML.vertex[] vertex, double spacing, double width, string shapename)
            {

                List<PointF> Verts = new List<PointF>();
                double nextcurve = 0;
                double lx = 0;
                double ly = 0;
                for (int j = 0; j <= vertex.Count(); j++)
                {
                    var v = vertex[j % vertex.Count()];

                    if (nextcurve != 0)
                    {
                        bool lefthand = false;

                        double endx = EagleLoader.ParseDouble(v.x);
                        double endy = EagleLoader.ParseDouble(v.y);

                        if (nextcurve < 0)
                        {
                            lefthand = true;

                            double T = lx;
                            lx = endx;
                            endx = T;
                            T = ly;
                            ly = endy;
                            endy = T;
                            nextcurve *= -1;
                        }
                        else
                        {

                        }



                        double theta = nextcurve * Math.PI * 2.0 / 360.0;
                        double sintheta = Math.Sin(theta / 2.0);
                        double tantheta = Math.Tan(theta / 2.0);

                        double dx = endx - lx;
                        double dy = endy - ly;

                        double O = Math.Sqrt(dx * dx + dy * dy);
                        double A = ((O / 2) / tantheta);
                        double S = (O / 2) / sintheta;

                        double sx = dy / O;
                        double sy = -dx / O;
                        double cx = lx - sx * A + dx / 2;
                        double cy = ly - sy * A + dy / 2;

                        if (lefthand)
                        {
                            cx = lx - sx * A + dx / 2;
                            cy = ly - sy * A + dy / 2;

                        }


                        // DebugPoints.Add(new PointF((float)cx, (float)cy));
                        //AddDebugLine(cx, cy, x1, y1);
                        //AddDebugLine(cx, cy, x2, y2);


                        double ldx = lx - cx;
                        double ldy = ly - cy;
                        double edx = endx - cx;
                        double edy = endy - cy;

                        double sh = Math.Atan2(ldy, ldx);
                        double esh = Math.Atan2(edy, edx);

                        double dsh = esh - sh;

                        //AddDebugLine(cx, cy, cx + Math.Cos(sh),  cy + Math.Sin(sh));
                        //AddDebugLine(cx, cy, cx + Math.Cos(esh), cy + Math.Sin(esh));



                        // A *= 2;
                        int curvelength = (int)Math.Ceiling((Math.Abs(nextcurve) * 20));
                        double HW = width / 2.0;
                        List<PointF> Series = new List<PointF>();

                        for (int i = 0; i < curvelength; i++)
                        {
                            double P = sh + (nextcurve * i * Math.PI * 2) / (curvelength * 360.0);

                            Series.Add(new PointF((float)(Math.Cos(P) * (S) + cx), (float)(Math.Sin(P) * (S) + cy)));
                            //                        Inner.Add(new PointF((float)(Math.Cos(P) * (S - HW) + cx), (float)(Math.Sin(P) * (S - HW) + cy)));
                        }
                        if (lefthand) Series.Reverse();
                        Verts.AddRange(Series);







                        if (lefthand == false)
                        {
                            lx = endx;
                            ly = endy;
                        }

                        Verts.Add(new PointF((float)lx, (float)ly));
                    }
                    else
                    {
                        lx = EagleLoader.ParseDouble(v.x);
                        ly = EagleLoader.ParseDouble(v.y);
                        if (j < vertex.Count())
                        {
                            Verts.Add(new PointF((float)lx, (float)ly));
                        }
                    }
                    nextcurve = EagleLoader.ParseDouble(v.curve);

                }
                AddShape(new Polygon(Verts, width, spacing) { Layer = layer }, shapename);
            }

            internal void AddSMD(double x, double y, RotationSpec rot, double roundness, double dx, double dy, string name, LayerSpec layer, EagleXML.smdCream cream, string shapename)
            {
                AddShape(new SMDShape(x, y, rot, roundness, dx, dy, name, layer, cream), shapename);
            }

            internal void AddText(textFont font, LayerSpec LS, string value, double x, double y, double size, RotationSpec rotation, textAlign align, double distance, double ratio, string shapename)
            {
                AddShape(new TextShape(font, LS, value, x, y, size, rotation, align, distance, ratio), shapename);
            }
        }


        internal class TextShape : Shape
        {
            private textAlign align;
            private double distance;
            private textFont font;

            private double ratio;
            private double rotation;
            private double size;
            private string value;
            private double x;
            private double y;

            public TextShape(textFont font, LayerSpec LS, string value, double x, double y, double size, RotationSpec rotation, textAlign align, double distance, double ratio)
            {
                this.Layer = LS;
                this.font = font;
                this.value = value;
                this.x = x;
                this.y = y;
                this.size = size;
                this.rotation = rotation.Degrees;
                this.align = align;
                this.distance = distance;
                this.ratio = ratio;
            }

            public override List<List<PointF>> GetPolygons()
            {
                if (value == null) return new List<List<PointF>>();
                var R = new List<List<PointF>>(Math.Max(1, value.Length));
                StringFormat SF = new StringFormat();

                switch (align)
                {
                    case textAlign.center: SF.Alignment = StringAlignment.Center; SF.LineAlignment = StringAlignment.Center; break;
                    case textAlign.centerleft: SF.Alignment = StringAlignment.Near; SF.LineAlignment = StringAlignment.Center; break;
                    case textAlign.centerright: SF.Alignment = StringAlignment.Far; SF.LineAlignment = StringAlignment.Center; break;
                    case textAlign.topcenter: SF.Alignment = StringAlignment.Center; SF.LineAlignment = StringAlignment.Near; break;
                    case textAlign.topleft: SF.Alignment = StringAlignment.Near; SF.LineAlignment = StringAlignment.Near; break;
                    case textAlign.topright: SF.Alignment = StringAlignment.Far; SF.LineAlignment = StringAlignment.Near; break;
                    case textAlign.bottomcenter: SF.Alignment = StringAlignment.Center; SF.LineAlignment = StringAlignment.Far; break;
                    case textAlign.bottomleft: SF.Alignment = StringAlignment.Near; SF.LineAlignment = StringAlignment.Far; break;
                    case textAlign.bottomright: SF.Alignment = StringAlignment.Far; SF.LineAlignment = StringAlignment.Far; break;
                }
                GraphicsPath P = new GraphicsPath();
                P.AddString(
                        value,
                        FontFamily.GenericSansSerif,
                        (int)FontStyle.Regular,
                        (float)-size,
                        new PointF((float)0, (float)y),
                        SF
                );
                List<PointF> CurrentPath = new List<PointF>(100);
                for (int i = 0; i < P.PointCount; i++)
                {
                    switch (P.PathTypes[i])
                    {
                        case 0:
                            if (CurrentPath.Count > 0)
                            {
                                R.Add(CurrentPath);
                                CurrentPath = new List<PointF>(100);
                            }
                            break;
                        case 0x80:
                            {


                            }
                            break;
                    }
                    CurrentPath.Add(new PointF(-P.PathPoints[i].X + (float)x, P.PathPoints[i].Y));

                }
                R.Add(CurrentPath);

                return R;
            }
        }

        public class PinShape : Shape
        {

            public PinShape(double x, double y, RotationSpec rot, pinLength length, pinDirection direction, pinFunction function, pinVisible visible, string swaplevel, string pinname)
            {
                this.x = x;
                this.y = y;
                this.rot = rot;
                this.length = length;
                this.direction = direction;
                this.function = function;
                this.visible = visible;
                this.swaplevel = swaplevel;
                this.pinname = pinname;
                DisplayStrings.Add(new DisplayString() { alignment = textAlign.bottomright, x = x, y = y, size = 1, Text = pinname });

            }


            private double x;
            private double y;
            private RotationSpec rot;
            private pinLength length;
            private pinDirection direction;
            private pinFunction function;
            private pinVisible visible;
            private string swaplevel;
            private string pinname;

            public override List<List<PointF>> GetPolygons()
            {
                List<List<PointF>> R = new List<List<PointF>>();
                return R;
            }
        }
        public class Symbol : ShapeContainer
        {
            public override string ToString()
            {
                return String.Format("{0}: symbol with {1} pins", this.Name, Pins.Count);
            }
            public List<Pin> Pins = new List<Pin>();

            internal void AddPin(double x, double y, RotationSpec rot, pinLength length, pinDirection direction, pinFunction function, pinVisible visible, string swaplevel, string pinname)
            {
                AddShape(new PinShape(x, y, rot, length, direction, function, visible, swaplevel, pinname), pinname);
            }
        }

        public class Gate
        {
            public Symbol TheSymbol = null;
            public string Name;
            public string SymbolName;
        }
        public class DeviceSet
        {
            public List<Gate> Gates = new List<Gate>();
            public List<Device> Devices = new List<Device>();
            public string Name;
            internal void AddGate(string symname, string gatename)
            {
                Gates.Add(new Gate() { SymbolName = symname, Name = gatename });
            }


            internal Device GetDevice(string p)
            {
                if (Devices.Count == 1) return Devices[0];
                foreach (var d in Devices)
                {
                    if (d.Name == p) return d;
                }
                return Devices[0];
            }
        }
        public class GateConnection
        {
            public string GateName;
            public string Pad;
            public string Pin;
        }

        public class ShapeContainerSMD
        {

        }

        public class ShapeContainerPin
        {

        }

        public class ShapeContainerLine
        {

        }

        public class Device
        {
            public string Name;
            public ShapeContainer ThePackage = new ShapeContainer();
            public List<GateConnection> Connections = new List<GateConnection>();

            public List<ShapeContainerSMD> SMDs;
            public List<ShapeContainerPin> Pins;
            public List<ShapeContainerLine> Decals;
            //public List<>

            public string PackageName;
            public DeviceSet Parent;
        }

        public class Pad
        {
            public string Name;
        }

        public enum PinLength
        {
            Short, Medium, Long
        }

        public class Pin
        {
            public string Name;
            public double X;
            public double Y;
            public PinLength Length;
            public double Rotation;
            internal pinDirection PinDirection;
        }

        public class PinInstance
        {
            public string Name;
            public Pin ThePin;

        }

        public class GateInstance
        {
            public GateInstance(string _name, Symbol source)
            {
                Name = _name;
                SourceSymbol = source;
                foreach (var p in SourceSymbol.Pins)
                {
                    PinInstance PI = new PinInstance() { Name = p.Name, ThePin = p };
                }
            }
            public string Name;
            public Symbol SourceSymbol;
            public List<PinInstance> Pins = new List<PinInstance>();
        }

        public class PartInstance
        {
            public string Name;
            public List<GateInstance> Gates = new List<GateInstance>();
            public Device SourceDevice;

            public PartInstance(string name, Device sourceDevice)
            {
                // TODO: Complete member initialization
                Name = name;

                SourceDevice = sourceDevice;
                foreach (var g in SourceDevice.Parent.Gates)
                {
                    Gates.Add(new GateInstance(g.Name, g.TheSymbol));
                }
            }

            public GateInstance GetGate(string gate)
            {
                foreach (var a in Gates)
                {
                    if (a.Name == gate) return a;
                }
                return null;
            }
        }

        public class PinRef
        {
            public string PartName;
            public string GateName;
            public string PinName;
            public PinInstance ThePin;
        }

        public class PadRef
        {
            public string DeviceName;
            public string PadName;
            internal EagleLoader.DevicePlacement DevicePlacement;
        }

        public class SchematicNet
        {
            public string Name;
            public List<PinRef> PinsInNet = new List<PinRef>();
            public List<PadRef> PadsInNet = new List<PadRef>();
            public NetSpec Settings = new NetSpec() { Name = "default", Clearance = 5 * 25.4 / 1000.0, AutoPour = false, MinTraceWidth = 9 * 25.4 / 1000.0 };

            public int IDX = 0;

            public override string ToString()
            {
                return String.Format("net {0}: {1} pins, {2} pads", Name, PinsInNet.Count(), PadsInNet.Count());
            }
        }

        public class GatePlacement
        {
            public string Gate;
            public double x;
            public double y;
            public double rot;
            public string part;

            public override string ToString()
            {
                return string.Format("{0}({4}): {1},{2} {3} ", Gate, x, y, rot, part);
            }
        }

        public class SchematicLoader
        {
            public List<PartInstance> Parts = new List<PartInstance>();
            public List<SchematicNet> Nets = new List<SchematicNet>();
            public List<GatePlacement> PartPlacement = new List<GatePlacement>();

            public void LoadSchematic(EagleLoader L, string filename)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(EagleXML.eagle));

                // Declare an object variable of the type to be deserialized.
                EagleXML.eagle i;

                using (Stream reader = new FileStream(filename, FileMode.Open))
                {
                    // Call the Deserialize method to restore the object's state.
                    i = (EagleXML.eagle)serializer.Deserialize(reader);
                }
                //   LoadedDoc = i;

                if (i.drawing != null && i.drawing.schematic != null && i.drawing.schematic.libraries != null)
                {
                    ParseSchematic(i.drawing.schematic, L);
                }
            }

            public static double DParse(string inp)
            {
                double outp = 0;
                if (inp == null) return 0;
                if (double.TryParse(inp.Replace('.', ','), out outp))
                {
                    return outp;
                }
                return outp;

            }

            private void ParseSchematic(EagleXML.schematic s, EagleLoader L)
            {
                if (s.parts != null)
                {
                    foreach (var p in s.parts.part)
                    {
                        var D = L.GetDevice(p.library + "_" + p.deviceset);
                        if (D != null)
                        {
                            Parts.Add(new PartInstance(p.name, D.GetDevice(p.device)));
                        }
                        else
                        {
                            Console.WriteLine("{2}:{0}:{1} not found", p.library, p.deviceset, p.name);
                        }
                    }
                }


                if (s.sheets != null) foreach (var a in s.sheets.sheet)
                    {
                        if (a.instances != null)
                            foreach (var i in a.instances.instance)
                            {
                                PartPlacement.Add(new GatePlacement() { Gate = i.gate, x = DParse(i.x), y = DParse(i.y), rot = DParse(i.rot), part = i.part });
                            }
                        if (a.nets != null)
                            foreach (var n in a.nets.net)
                            {
                                SchematicNet SN = GetNet(n.name, true);
                                if (n.segment != null) foreach (var p in n.segment)
                                    {
                                        if (p.pinref != null)
                                        {
                                            foreach (var pr in p.pinref)
                                            {
                                                SN.PinsInNet.Add(new PinRef() { GateName = pr.gate, PartName = pr.part, PinName = pr.pin });
                                            }
                                        }

                                    }
                            }

                    }

            }

            public PartInstance GetPart(string s)
            {
                foreach (var p in Parts)
                {
                    if (p.Name == s) return p;
                }
                return null;
            }

            public SchematicNet GetNet(string s, bool createifneeded = false)
            {
                foreach (var p in Nets)
                {
                    if (p.Name == s) return p;
                }
                if (createifneeded)
                {
                    SchematicNet SN = new SchematicNet() { Name = s };
                    Nets.Add(SN);
                    return SN;
                }
                return null;
            }
        }


        public class EagleLoader
        {

            // matched up physical parts and device-gate schematic symbols
            public List<DeviceSet> DeviceSets = new List<DeviceSet>();

            // parts in the schematic
            public List<PartInstance> Parts = new List<PartInstance>();

            // signals
            public List<SchematicNet> Nets = new List<SchematicNet>();

            // placed devicesets on a board
            public List<DevicePlacement> DevicePlacements = new List<DevicePlacement>();
            public List<Wire> PlainWires = new List<Wire>();
            // board shapes
            public List<ShapeContainer> Packages = new List<ShapeContainer>();

            // schematic shapes
            public List<ShapeContainer> Symbols = new List<ShapeContainer>();

            public string LoadedLibrary = "";
            private EagleXML.eagle LoadedDoc;

            public BoardSpec BS = new BoardSpec();

            public EagleLoader(string filename)
            {
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(EagleXML.eagle));

                    // Declare an object variable of the type to be deserialized.
                    EagleXML.eagle i;

                    using (Stream reader = new FileStream(filename, FileMode.Open))
                    {
                        // Call the Deserialize method to restore the object's state.
                        i = (EagleXML.eagle)serializer.Deserialize(reader);
                    }
                    LoadedDoc = i;
                    if (i.drawing != null && i.drawing.library != null)
                    {
                        ParseLayers(i.drawing.layers, BS);
                        ParseLibrary(i.drawing.library, BS);
                    }

                    if (i.drawing != null)
                    {
                        ParseLayers(i.drawing.layers, BS);

                        if (i.drawing.board != null)
                        {
                            if (i.drawing.board.libraries.library != null)
                            {
                                foreach (var l in i.drawing.board.libraries.library)
                                {
                                    ParseLibrary(l, BS);
                                }
                            }
                            ParseBoard(i.drawing.board, BS);
                        }
                        if (i.drawing.schematic != null)
                        {
                            if (i.drawing.schematic.libraries.library != null)
                            {
                                foreach (var l in i.drawing.schematic.libraries.library)
                                {
                                    ParseLibrary(l, BS);
                                }
                            }

                            ParseSchematic(i.drawing.schematic, BS);
                        }

                    }

                }
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(EagleXML.eagle));

                    // Declare an object variable of the type to be deserialized.
                    EagleXML.eagle i;

                    using (Stream reader = new FileStream(filename, FileMode.Open))
                    {
                        // Call the Deserialize method to restore the object's state.
                        i = (EagleXML.eagle)serializer.Deserialize(reader);
                    }
                    //   LoadedDoc = i;

                    if (i.drawing != null && i.drawing.schematic != null && i.drawing.schematic.libraries != null)
                    {
                        foreach (var l in i.drawing.schematic.libraries.library)
                        {
                            ParseLibrary(l, BS);
                        }
                    }

                }

                FixSymbolLinks();
                LoadedLibrary = filename;
            }

            private void ParseSchematic(schematic schematic, BoardSpec bS)
            {
                DevicePlacements.Clear();
                if (schematic.parts != null && schematic.parts.part != null)
                {
                    foreach (var p in schematic.parts.part)
                    {
                        DevicePlacements.Add(new DevicePlacement()
                        {
                            library = p.library,
                            device = p.device,
                            name = p.name,
                            deviceset = p.deviceset
                        });
                    }

                }
                Dictionary<string, List<DevicePlacement>> Modules = new Dictionary<string, List<DevicePlacement>>();
                if (schematic.modules != null && schematic.modules.module != null)
                {
                    foreach (var m in schematic.modules.module)
                    {
                        List<DevicePlacement> DP = new List<DevicePlacement>();
                        if (m.parts != null && m.parts.part != null)
                        {
                            foreach (var p in m.parts.part)
                            {
                                DP.Add(new DevicePlacement()
                                {
                                    library = p.library,
                                    device = p.device,
                                    name = p.name,
                                    deviceset = p.deviceset
                                });
                            }
                        }
                        if (m.sheets != null && m.sheets.sheet != null)
                        {
                            foreach (var s in m.sheets.sheet)
                            {
                                if (s.nets != null && s.nets.net != null)
                                {
                                    foreach (var N in s.nets.net)
                                    {
                                        this.Nets.Add(new SchematicNet() {Name = N.name });
                                    }
                                }
                                if (s.moduleinsts != null && s.moduleinsts.moduleinst != null)
                                    foreach (var mi in s.moduleinsts.moduleinst)
                                    {
                                        var L = Modules[mi.module];
                                        foreach (var p in L)
                                        {
                                            DP.Add(new DevicePlacement()
                                            {
                                                library = p.library,
                                                device = p.device,
                                                name = mi.name + ":" + p.name,
                                                deviceset = p.deviceset
                                            });
                                        }
                                    }
                            }
                        }
                        Modules[m.name] = DP;
                    }
                }

                if (schematic.sheets != null && schematic.sheets.sheet != null)
                {
                    foreach (var s in schematic.sheets.sheet)
                    {
                        if (s.nets != null && s.nets.net != null)
                        {
                            foreach (var N in s.nets.net)
                            {

                                this.Nets.Add(new SchematicNet() {Name = N.name });
                                Console.WriteLine(N.name);
                                if (N.portref!=null) foreach(var p in N.portref)
                                {
                                    Console.WriteLine(p);
                                }
                                if (N.pinref!= null) foreach (var p in N.pinref)
                                {
                                    Console.WriteLine(p);
                                }
                                if (N.segment!=null) foreach(var seg in N.segment)
                                {
                                        if (seg.pinref !=null) foreach(var a in seg.pinref)
                                        {
                                                Console.WriteLine(a);
                                        }
                                 }
                            }
                        }
                        if (s.moduleinsts != null && s.moduleinsts.moduleinst != null)
                        {
                            foreach (var m in s.moduleinsts.moduleinst)
                            {
                                var L = Modules[m.module];
                                foreach (var p in L)
                                {
                                    DevicePlacements.Add(new DevicePlacement()
                                    {
                                        library = p.library,
                                        device = p.device,
                                        name = m.name + ":" + p.name,
                                        deviceset = p.deviceset
                                    });
                                }
                            }
                        }
                    }
                }


            }

            private void ParseLayers(layers layers, BoardSpec B)
            {
                if (layers == null) return;

                foreach (var l in layers.layer)
                {
                    LayerSpec L = LayerSpec.ParseLayer(l.number, B.Layers);
                    switch (l.name.ToLower())
                    {
                        case "top":
                            L.Side = BoardSide.Top;
                            L.type = LayerType.Copper;
                            break;
                        case "tnames":
                        case "tplace":
                            L.Side = BoardSide.Top;
                            L.type = LayerType.Silk;
                            break;
                        case "bdocu":
                            L.Side = BoardSide.Bottom;
                            L.type = LayerType.Docu;
                            break;
                        case "tdocu":
                            L.Side = BoardSide.Top;
                            L.type = LayerType.Docu;
                            break;
                        case "bnames":
                        case "bplace":
                            L.Side = BoardSide.Bottom;
                            L.type = LayerType.Silk;
                            break;
                        case "bottom":
                            L.Side = BoardSide.Bottom;
                            L.type = LayerType.Copper;
                            break;
                        case "pads":
                            L.type = LayerType.Copper;
                            L.Side = BoardSide.Both;
                            break;
                    }
                    //  BS.Layers.Add(L);
                }
            }

            public class DevicePlacementPin
            {
                public string name;
                public SchematicNet signal;
            }

            public class DevicePlacement
            {
                public override string ToString()
                {
                    return String.Format("{0} {1} {2} {3},{4}", device, package, library, x, y);
                }
                public double x;
                public double y;
                public string library;
                public string package;
                public string device;
                public RotationSpec rot;

                public Dictionary<string, DevicePlacementPin> Pins = new Dictionary<string, DevicePlacementPin>();
                public string desc;
                public string value;
                public string deviceset;
                public string name;
                public string sizedesc;

                internal void AddPin(string key)
                {
                    Pins[key] = new DevicePlacementPin() { name = key };
                }

                public void ConnectPinToSignal(string pin, SchematicNet Net)
                {
                    if (Pins.ContainsKey(pin)) Pins[pin].signal = Net;
                }

                public SchematicNet GetSignal(string name)
                {
                    if (Pins.ContainsKey(name)) return Pins[name].signal;
                    return null;
                }
            }

            private void ParseBoard(board board, BoardSpec BS)
            {
                DevicePlacements.Clear();
                if (board.elements != null && board.elements.element != null)
                {
                    foreach (var E in board.elements.element)
                    {
                        string descattrib = E.name;
                        string sizeattrib = "";
                        if (E.attribute != null)
                        {
                            foreach (var e in E.attribute)
                            {
                                if (e.name == "DESC") descattrib = e.value;
                                if (e.name == "SIZE") sizeattrib = e.value;
                            }
                        }
                        DevicePlacement DP = new DevicePlacement()
                        {
                            device = E.name,
                            package = E.package,
                            library = E.library,
                            value = E.value,
                            desc = descattrib,
                            sizedesc = sizeattrib,
                            x = ParseDouble(E.x),
                            y = ParseDouble(E.y),
                            rot = RotationSpec.Parse(E.rot)
                        };
                        FillDevice(DP);

                        DevicePlacements.Add(DP);
                    }
                }
                if (board.plain != null)
                {
                    if (board.plain.wire != null)
                        foreach (var w in board.plain.wire)
                        {
                            if (w.layer == "20")
                            {
                                PlainWires.Add(new Wire() { x1 = ParseDouble(w.x1), x2 = ParseDouble(w.x2), y1 = ParseDouble(w.y1), y2 = ParseDouble(w.y2) });
                                //                            Console.WriteLine("Dimension segment!");
                            }
                        }

                }
                if (board.signals != null && board.signals.signal != null)
                {
                    int i = 0;
                    foreach (var S in board.signals.signal)
                    {
                        SchematicNet SN = new SchematicNet() { Name = S.name, IDX = i++ };
                        // Console.WriteLine("signal: {0}", SN.Name);
                        if (S.contactref != null)
                        {
                            foreach (var p in S.contactref)
                            {
                                //   Console.WriteLine(" pin: {0}-{1}", p.element,p.pad);

                                var Dev = FindDevice(p.element);
                                Dev.ConnectPinToSignal(p.pad, SN);
                                SN.PadsInNet.Add(new PadRef() { DeviceName = p.element, PadName = p.pad, DevicePlacement = Dev });

                                var D = GetDevice(p.element);

                            }
                        }
                        BS.Nets.Add(SN);

                        Nets.Add(SN);
                    }
                }
                foreach (var DP in DevicePlacements)
                {
                }
            }
            private DevicePlacement FindDevice(string name)
            {
                foreach (var a in DevicePlacements)
                {
                    if (a.device == name) return a;
                }
                return null;
            }

            private void FillDevice(DevicePlacement dP)
            {
                var D = GetPackage(dP.package);

                foreach (var P in D.NamedShapes)
                {

                    dP.AddPin(P.Key);

                    // dP.Pads.Add(new DevicePlacementPad){ });
                    //                Console.WriteLine("D.name: {0}", D.Name);
                }
            }

            public SchematicNet GetSignal(string key)
            {
                foreach (var s in Nets)
                {
                    if (s.Name == key) return s;
                }
                return null;
            }

            private void FixSymbolLinks()
            {
                foreach (var DS in DeviceSets)
                {
                    foreach (var G in DS.Gates)
                    {
                        G.TheSymbol = GetSymbol(G.SymbolName);
                    }
                }
            }



            public static double ParseDouble(string inp)
            {
                if (inp == null) return 0;
                inp = inp.Trim();
                if (inp.Length == 0) return 0;
                inp = inp.Replace(".", ",");
                if (inp[0] == 'R') inp = inp.Substring(1);
                if (inp.Length > 2 && inp[0] == 'S' && inp[1] == 'R') inp = inp.Substring(2);
                if (inp.Length > 2 && inp[0] == 'M' && inp[1] == 'R') inp = inp.Substring(2);
                return double.Parse(inp);
            }

            private void ParseLibrary(EagleXML.library eagleDrawingLibrary, BoardSpec BS)
            {

                if (eagleDrawingLibrary.devicesets != null && eagleDrawingLibrary.devicesets.deviceset != null) foreach (var ds in eagleDrawingLibrary.devicesets.deviceset)
                    {
                        DeviceSet D = new DeviceSet();
                        D.Name = ds.name;
                        if (ds.gates.gate != null) foreach (var g in ds.gates.gate)
                            {
                                D.AddGate(g.symbol, g.name);
                            }
                        foreach (var d in ds.devices.device)
                        {
                            var dd = new Device() { Name = d.name, PackageName = d.package, Parent = D };
                            if (d.connects != null && d.connects.connect != null)
                            {
                                foreach (var c in d.connects.connect)
                                {
                                    dd.Connections.Add(new GateConnection() { GateName = c.gate, Pad = c.pad, Pin = c.pin });
                                }
                            }
                            D.Devices.Add(dd);

                        }
                        DeviceSets.Add(D);
                    }

                if (eagleDrawingLibrary.symbols != null) foreach (var p in eagleDrawingLibrary.symbols.symbol)
                    {
                        Symbol P = new Symbol() { Name = p.name };
                        if (p.text != null) foreach (var t in p.text)
                            {
                                P.AddText(t.font, LayerSpec.ParseLayer(t.layer, BS.Layers), t.Value, ParseDouble(t.x), ParseDouble(t.y), ParseDouble(t.size), RotationSpec.Parse(t.rot), t.align, ParseDouble(t.distance), ParseDouble(t.ratio), p.name);
                            }

                        if (p.circle != null) foreach (var a in p.circle)
                            {
                                P.AddCircle(ParseDouble(a.x), ParseDouble(a.y), ParseDouble(a.radius), ParseDouble(a.width), LayerSpec.ParseLayer(a.layer, BS.Layers), null);
                            }

                        if (p.rectangle != null) foreach (var r in p.rectangle)
                            {
                                P.AddRectangle(LayerSpec.ParseLayer(r.layer, BS.Layers), ParseDouble(r.x1), ParseDouble(r.y1), ParseDouble(r.x2), ParseDouble(r.y2), RotationSpec.Parse(r.rot), null);
                            }

                        if (p.polygon != null) foreach (var a in p.polygon)
                            {
                                P.AddPoly(LayerSpec.ParseLayer(a.layer, BS.Layers), a.vertex, ParseDouble(a.spacing), ParseDouble(a.width), null);
                            }

                        if (p.wire != null) foreach (var a in p.wire)
                            {
                                P.AddWire(ParseDouble(a.x1), ParseDouble(a.y1), ParseDouble(a.x2), ParseDouble(a.y2), ParseDouble(a.width), LayerSpec.ParseLayer(a.layer, BS.Layers), a.cap, ParseDouble(a.curve), null);
                            }

                        if (p.pin != null) foreach (var a in p.pin)
                            {
                                P.AddPin(ParseDouble(a.x), ParseDouble(a.y), RotationSpec.Parse(a.rot), a.length, a.direction, a.function, a.visible, a.swaplevel, a.name);
                            }
                        Symbols.Add(P);
                    }

                if (eagleDrawingLibrary.packages != null && eagleDrawingLibrary.packages.package != null)
                    foreach (var p in eagleDrawingLibrary.packages.package)
                    {
                        ShapeContainer P = new ShapeContainer() { Name = p.name };
                        P.Name = p.name;
                        if (p.text != null) foreach (var t in p.text)
                            {
                                P.AddText(t.font, LayerSpec.ParseLayer(t.layer, BS.Layers), t.Value, ParseDouble(t.x), ParseDouble(t.y), ParseDouble(t.size), RotationSpec.Parse(t.rot), t.align, ParseDouble(t.distance), ParseDouble(t.ratio), null);
                            }
                        if (p.circle != null) foreach (var a in p.circle)
                            {
                                P.AddCircle(ParseDouble(a.x), ParseDouble(a.y), ParseDouble(a.radius), ParseDouble(a.width), LayerSpec.ParseLayer(a.layer, BS.Layers), null);
                            }
                        if (p.hole != null) foreach (var a in p.hole)
                            {
                                P.AddHole(ParseDouble(a.x), ParseDouble(a.y), ParseDouble(a.drill), LayerSpec.ParseLayer("DRILL", BS.Layers), null);
                            }
                        if (p.rectangle != null) foreach (var r in p.rectangle)
                            {
                                P.AddRectangle(LayerSpec.ParseLayer(r.layer, BS.Layers), ParseDouble(r.x1), ParseDouble(r.y1), ParseDouble(r.x2), ParseDouble(r.y2), RotationSpec.Parse(r.rot), null);
                            }
                        if (p.pad != null) foreach (var a in p.pad)
                            {

                                P.AddPad(a.name, ParseDouble(a.x), ParseDouble(a.y), RotationSpec.Parse(a.rot), a.shape, "", a.thermals, ParseDouble(a.drill), a.diameter, "", LayerSpec.ParseLayer("PADS", BS.Layers), BS, a.name);
                            }
                        if (p.polygon != null) foreach (var a in p.polygon)
                            {

                                P.AddPoly(LayerSpec.ParseLayer(a.layer, BS.Layers), a.vertex, ParseDouble(a.spacing), ParseDouble(a.width), null);
                            }
                        if (p.wire != null) foreach (var a in p.wire)
                            {
                                P.AddWire(ParseDouble(a.x1), ParseDouble(a.y1), ParseDouble(a.x2), ParseDouble(a.y2), ParseDouble(a.width), LayerSpec.ParseLayer(a.layer, BS.Layers), a.cap, ParseDouble(a.curve), null);
                            }

                        if (p.smd != null) foreach (var a in p.smd)
                            {
                                //Console.WriteLine("Roundness: {0}", a.roundness);
                                P.AddSMD(ParseDouble(a.x), ParseDouble(a.y), RotationSpec.Parse(a.rot), ParseDouble(a.roundness), ParseDouble(a.dx), ParseDouble(a.dy), a.name, LayerSpec.ParseLayer(a.layer, BS.Layers), a.cream, a.name);
                            }

                        Packages.Add(P);
                    }

                if (eagleDrawingLibrary.symbols != null) foreach (var s in eagleDrawingLibrary.symbols.symbol)
                    {

                        Symbol S = GetSymbol(s.name);
                        bool news = false;
                        if (S == null)
                        {
                            news = true;
                            S = new Symbol();
                            S.Name = s.name;

                        }

                        if (s.pin != null)
                            foreach (var p in s.pin)
                            {
                                Pin P = new Pin() { PinDirection = p.direction, Name = p.name, X = ParseDouble(p.x), Y = ParseDouble(p.y) };
                                S.Pins.Add(P);
                            }

                        if (news) Symbols.Add(S);
                    }
            }

            public DeviceSet GetDevice(string p)
            {
                foreach (var d in DeviceSets)
                {
                    if (d.Name == p) return d;

                }
                return null;
            }

            public Symbol GetSymbol(string p)
            {
                foreach (var d in Symbols)
                {
                    if (d.Name == p) return d as Symbol;

                }
                return null;
            }

            public ShapeContainer GetPackage(string p)
            {
                foreach (var d in Packages)
                {
                    if (d.Name == p) return d as ShapeContainer;

                }
                return null;
            }

            public DevicePlacement FindPlacement(string device)
            {
                foreach (var d in DevicePlacements)
                {
                    if (d.name == device) return d;
                }
                return null;
            }
        }
    }
}