using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using System.Drawing.Drawing2D;

using GerberLibrary.EagleXML;

namespace GerberLibrary
{
    namespace Eagle
    {
        public class ShapeInstance
        {
            public ShapeContainer TheShape;
            public bool TopSide = true;
            public PointF Position = new PointF(0, 0);
            public double Rotation = 0;

            public Bounds Bounds;
            internal PointF Centroid;

            public EagleLoader.DevicePlacement Placement;
        }

        public class Layer
        {
        }

        public class Bounds
        {
            public double minx;
            public double miny;
            public double maxx;
            public double maxy;
            public int fitpoints = 0;
            public void Fit(double x, double y)
            {
                if (fitpoints == 0)
                {
                    minx = maxx = x;
                    miny = maxy = y;
                }
                else
                {
                    if (x < minx) minx = x; else if (x > maxx) maxx = x;
                    if (y < miny) miny = y; else if (y > maxy) maxy = y;
                }
                fitpoints++;
            }

            public double Width()
            {
                return maxx - minx;
            }

            public void Clear()
            {
                fitpoints = 0;
            }

            public double Height()
            {
                return maxy - miny;
            }


        }

        public class BoardRenderer
        {
            public static Color SilkColor = Color.White;
            public static Color SMTColor = Color.DarkGray;
            public static Color PADColor = Color.DarkGreen;
            public static Color CopperColor = Color.Gold;
            public static Color SymbolsColor = Color.DarkRed;
            public static Bounds GetShapeBounds(ShapeContainer a)
            {
                Bounds R = new Bounds();
                foreach (var s in a.Shapes)
                {
                    var Ps = s.GetPolygons();
                    foreach (var P in Ps)
                    {
                        foreach (var v in P)
                        {
                            R.Fit(v.X, v.Y);
                        }
                    }
                }
                return R;

            }

            public static Color DrillColor = Color.Black;
            public static Color DocuColor = Color.FromArgb(200, 255, 255, 0);

            Dictionary<LayerSpec, Color> Colors = new Dictionary<LayerSpec, Color>();
            Dictionary<LayerSpec, int> LayerPriority = new Dictionary<LayerSpec, int>();
            Dictionary<LayerSpec, List<DisplayString>> StringsPerLayer = new Dictionary<LayerSpec, List<DisplayString>>();
            List<PointF> debugpoints = new List<PointF>();
            Dictionary<LayerSpec, int> Layers = new Dictionary<LayerSpec, int>();
            Dictionary<LayerSpec, List<List<Tuple<SchematicNet, List<PointF>>>>> ShapeLists = new Dictionary<LayerSpec, List<List<Tuple<SchematicNet, List<PointF>>>>>();

            public GerberLibrary.Bounds Bounds = new GerberLibrary.Bounds();
            public float GetScale(double MaxW, double MaxH)
            {
                double aspect = MaxW / MaxH;
                double dispw = Bounds.Width();
                double disph = Bounds.Height();
                double scale = 1.0;
                double partaspect = dispw / disph;

                if (partaspect > aspect)
                {
                    scale = (MaxW) / (dispw + 3);
                }
                else
                {
                    scale = (MaxH) / (disph + 3);
                }

                return (float)scale;
            }

            public Matrix Getmatrix(double MaxW, double MaxH, double scalefac = 1.0, double xoff = 0, double yoff = 0)
            {

                double minx = Bounds.TopLeft.X;
                double maxx = Bounds.BottomRight.X;
                double miny = Bounds.TopLeft.Y;
                double maxy = Bounds.BottomRight.Y;

                double aspect = MaxW / MaxH;
                double dispw = maxx - minx;
                double disph = maxy - miny;
                double scale = 1.0;
                double partaspect = dispw / disph;

                if (partaspect > aspect)
                {
                    scale = (MaxW) / (dispw + 3);
                }
                else
                {
                    scale = (MaxH) / (disph + 3);
                }

                Graphics g = Graphics.FromImage(new Bitmap(1, 1));
                g.TranslateTransform((float)((MaxW) / 2), (float)((MaxH) / 2));
                g.ScaleTransform((float)scale, (float)-scale);
                g.TranslateTransform((float)(-(maxx + minx) / 2), (float)((-(maxy + miny) / 2)));

                g.TranslateTransform((float)xoff, (float)yoff);
                g.ScaleTransform((float)scalefac, (float)scalefac);

                return g.Transform.Clone();
            }

            public void DrawInstances(GerberLibrary.Core.GraphicsInterface g, double MaxW, double MaxH, bool rendergrid, Color gridcolor, double scalefac = 1.0, double xoff = 0, double yoff = 0)
            {
                double minx = Bounds.TopLeft.X;
                double maxx = Bounds.BottomRight.X;
                double miny = Bounds.TopLeft.Y;
                double maxy = Bounds.BottomRight.Y;


                double aspect = MaxW / MaxH;
                double dispw = maxx - minx;
                double disph = maxy - miny;
                double scale = 1.0;
                double partaspect = dispw / disph;

                if (partaspect > aspect)
                {
                    scale = (MaxW) / (dispw + 3);
                }
                else
                {
                    scale = (MaxH) / (disph + 3);
                }

                if (dispw == 0 || disph == 0) return;
                var originaltransform = g.Transform.Clone();


                g.TranslateTransform((float)((MaxW) / 2), (float)((MaxH) / 2));
                g.ScaleTransform((float)scale, (float)-scale);


                g.TranslateTransform((float)(-(maxx + minx) / 2), (float)((-(maxy + miny) / 2)));

                g.TranslateTransform((float)xoff, (float)yoff);
                g.ScaleTransform((float)scalefac, (float)scalefac);


                if (rendergrid)
                {
                    Pen Thick = new Pen(gridcolor, (float)(2.0 / scale));
                    Pen Thin = new Pen(gridcolor, (float)(1.0 / scale));
                    for (int x = (int)minx - 10; x < (int)maxx + 10; x++)
                    {
                        g.DrawLine((x % 5 == 0) ? Thick : Thin, x, (float)miny - 10, x, (float)maxy + 10);
                    }
                    for (int y = (int)miny - 10; y < (int)maxy + 10; y++)
                    {
                        g.DrawLine((y % 5 == 0) ? Thick : Thin, (float)minx - 10, (float)y, (float)maxx + 10, (float)y);
                    }
                }
                StringFormat SF = new StringFormat();



                foreach (var l in (from i in Layers orderby i.Value select i.Key))
                {
                    var L = ShapeLists[l];

                    Color C = Color.FromArgb(100, 0, 10, 255);
                    if (Colors.ContainsKey(l))
                    {
                        var c = Colors[l];
                        C = Color.FromArgb(200, c);
                    }
                    foreach (var p in L)
                    {
                        if (p.Count > 0)
                        {
                            //    g.DrawPolygon(new Pen(C, (float)(1.0 / scale)), p.ToArray());
                            GraphicsPath GP = new GraphicsPath();
                            foreach (var P in p)
                            {
                                GP.AddPolygon(P.Item2.ToArray());
                            }
                            g.FillPath(C, GP);
                            //g.FillPolygon(new SolidBrush(C), p.ToArray());
                        }
                    }
                    foreach (var S in StringsPerLayer[l])
                    {

                        switch (S.alignment)
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
                        g.DrawString(S.Text, new Font("Arial", (float)S.size), new SolidBrush(C), (float)S.x, (float)S.y, SF);
                    }

                }
                SF.Alignment = StringAlignment.Center;
                SF.LineAlignment = StringAlignment.Center;

                foreach (var a in debugpoints)
                {
                    g.DrawRectangle(Color.GreenYellow, a.X - .01f, a.Y - .01f, .02f, .02f, 0.01f);
                }

                g.Transform = originaltransform;
                g.TranslateTransform((float)((MaxW) / 2), (float)((MaxH) / 2));
                g.ScaleTransform((float)scale, (float)scale);
                g.TranslateTransform((float)(-(maxx + minx) / 2), (float)((-(maxy + miny) / 2)));

                g.InterpolationMode = InterpolationMode.High;


                if (false)
                    foreach (var l in (from i in Layers orderby i.Value select i.Key))
                    {
                        var L = ShapeLists[l];

                        foreach (var p in L)
                        {
                            foreach (var P in p)
                            {
                                if (P.Item1 != null && P.Item1.Name != null)
                                {
                                    var PS = PolyCenter(P.Item2);

                                    GraphicsPath PATH = new GraphicsPath();
                                    PATH.AddString(
                                        P.Item1.Name,             // text to draw
                                        FontFamily.GenericSansSerif,  // or any other font family
                                        (int)FontStyle.Regular,      // font style (bold, italic, etc.)
                                        72 * 10.0f / (72 * (float)scale),       // em size
                                        new PointF(PS.X, (float)maxy - (PS.Y - (float)miny)),              // location where to draw text
                                        SF);          // set options here (e.g. center alignment)
                                    g.DrawPath(Color.Black, PATH, 2.0f / (float)scale);
                                    g.FillPath(Color.White, PATH);

                                    //g.DrawString(P.Item1.Name, new Font("Arial", 0.2f), Brushes.Black,, SF);


                                }
                            }
                        }
                    }
            }

            private PointF PolyCenter(List<PointF> points)
            {

                if (points.Count > 0)
                {
                    float x = 0;
                    float y = 0;
                    for (int i = 0; i < points.Count; i++)
                    {
                        x += points[i].X;
                        y += points[i].Y;
                    }
                    x /= (float)points.Count;
                    y /= (float)points.Count;
                    return new PointF(x, y);

                }
                return new PointF(0, 0);
            }

            public void PrepareInstances(List<ShapeInstance> Instances, BoardSpec BS, bool showtext = true)
            {
                Colors[LayerSpec.ParseLayer("PADS", BS.Layers, LayerType.Copper)] = PADColor;
                Colors[LayerSpec.ParseLayer("1", BS.Layers)] = CopperColor;
                Colors[LayerSpec.ParseLayer("TOPCOPPER", BS.Layers, LayerType.Copper)] = CopperColor;
                Colors[LayerSpec.ParseLayer("BOTHCOPPER", BS.Layers, LayerType.Copper)] = CopperColor;
                Colors[LayerSpec.ParseLayer("BOTTOMCOPPER", BS.Layers, LayerType.Copper)] = CopperColor;
                Colors[LayerSpec.ParseLayer("DRILL", BS.Layers)] = DrillColor;
                Colors[LayerSpec.ParseLayer("DRILLS", BS.Layers)] = DrillColor;
                /*Colors[LayerSpec.ParseLayer("16", BS.Layers)] = CopperColor;
                Colors[LayerSpec.ParseLayer("21", BS.Layers)] = SilkColor;
                Colors[LayerSpec.ParseLayer("51", BS.Layers)] = DocuColor;
                Colors[LayerSpec.ParseLayer("52", BS.Layers)] = DocuColor;
                Colors[LayerSpec.ParseLayer("22", BS.Layers)] = SilkColor;
                Colors[LayerSpec.ParseLayer("25", BS.Layers)] = SilkColor;
                Colors[LayerSpec.ParseLayer("26", BS.Layers)] = SilkColor;
                Colors[LayerSpec.ParseLayer("27", BS.Layers)] = SilkColor;
                Colors[LayerSpec.ParseLayer("28", BS.Layers)] = SilkColor;
                Colors[LayerSpec.ParseLayer("94", BS.Layers)] = SymbolsColor;*/
                foreach (var a in BS.Layers)
                {
                    switch (a.type)
                    {
                        case LayerType.Copper: Colors[a] = CopperColor; break;
                        case LayerType.Silk: Colors[a] = SilkColor; break;
                        case LayerType.Docu: Colors[a] = DocuColor; break;
                        case LayerType.Drill: Colors[a] = DrillColor; break;

                    }
                }

                LayerPriority[LayerSpec.ParseLayer("21", BS.Layers)] = 100;
                LayerPriority[LayerSpec.ParseLayer("22", BS.Layers)] = 100;
                LayerPriority[LayerSpec.ParseLayer("25", BS.Layers)] = 100;
                LayerPriority[LayerSpec.ParseLayer("26", BS.Layers)] = 100;
                LayerPriority[LayerSpec.ParseLayer("27", BS.Layers)] = 100;
                LayerPriority[LayerSpec.ParseLayer("28", BS.Layers)] = 100;
                LayerPriority[LayerSpec.ParseLayer("51", BS.Layers)] = 101;
                LayerPriority[LayerSpec.ParseLayer("52", BS.Layers)] = 101;
                LayerPriority[LayerSpec.ParseLayer("DRILL", BS.Layers)] = 200;
                LayerPriority[LayerSpec.ParseLayer("DRILLS", BS.Layers)] = 200;
                LayerPriority[LayerSpec.ParseLayer("TOPCOPPER", BS.Layers, LayerType.Copper)] = 90;
                LayerPriority[LayerSpec.ParseLayer("BOTTOMCOPPER", BS.Layers, LayerType.Copper)] = 90;
                LayerPriority[LayerSpec.ParseLayer("BOTHCOPPER", BS.Layers, LayerType.Copper)] = 90;

                int count = 0;
                int vertexcount = 0;
                foreach (var I in Instances)
                {
                    if (I.Bounds == null) I.Bounds = GetInstanceBounds(I);
                    var ThePackage = I.TheShape;
                    foreach (var a in ThePackage.Shapes)
                    {
                        LayerSpec layer = a.GetLayer();
                        if (Layers.ContainsKey(layer) == false)
                        {
                            Layers[layer] = 1;
                            if (LayerPriority.ContainsKey(layer)) Layers[layer] = LayerPriority[layer];
                            ShapeLists[layer] = new List<List<Tuple<SchematicNet, List<PointF>>>>();
                            count++;
                        }
                    }


                    Matrix R = new Matrix();
                    R.Translate((float)I.Position.X, (float)I.Position.Y);
                    if (I.TopSide == false)
                    {
                        R.Scale(-1, 1);
                    }
                    R.Rotate((float)I.Rotation);

                    foreach (var a in ThePackage.Shapes)
                    {

                        SchematicNet ShapeSignal = null;
                        if (I.Placement != null && I.Placement.Pins.ContainsKey(a.Name))
                        {
                            ShapeSignal = I.Placement.Pins[a.Name].signal;
                        }

                        var DBP = a.GetDebugPoints().ToArray();

                        var PolySet = new List<Tuple<SchematicNet, List<PointF>>>();
                        bool process = true;
                        if (a as TextShape != null && showtext == false)
                        { process = false; }
                        if (process)
                            foreach (var Ps in a.GetPolygons())
                            {
                                double minx = 0;
                                double miny = 0;
                                double maxx = 0;
                                double maxy = 0; ;

                                var P = Ps.ToArray();
                                for (int i = 0; i < P.Count(); i++)
                                {
                                    PointF[] rotP = { new PointF(P[i].X, P[i].Y) };
                                    R.TransformPoints(rotP);
                                    P[i].X = rotP[0].X;
                                    P[i].Y = rotP[0].Y;
                                    Bounds.FitPoint(P[i].X, P[i].Y);
                                    if (vertexcount == 0)
                                    {
                                        minx = maxx = P[i].X;
                                        miny = maxy = P[i].Y;
                                    }
                                    else
                                    {
                                        if (P[i].X < minx) minx = P[i].X; else if (P[i].X > maxx) maxx = P[i].X;
                                        if (P[i].Y < miny) miny = P[i].Y; else if (P[i].Y > maxy) maxy = P[i].Y;

                                    }

                                    vertexcount++;
                                }
                                I.Centroid = new PointF((float)(minx + maxx) / 2, (float)(miny + maxy) / 2);
                                PolySet.Add(new Tuple<SchematicNet, List<PointF>>(ShapeSignal, P.ToList()));
                            }
                        if (StringsPerLayer.ContainsKey(a.GetLayer()) == false) StringsPerLayer[a.GetLayer()] = new List<DisplayString>();
                        foreach (var ds in a.DisplayStrings)
                        {
                            StringsPerLayer[a.GetLayer()].Add(new DisplayString() { alignment = ds.alignment, size = ds.size, x = ds.x + I.Position.X, Text = ds.Text, y = ds.y + I.Position.Y });
                        }
                        ShapeLists[a.GetLayer()].Add(PolySet);
                        for (int i = 0; i < DBP.Count(); i++)
                        {
                            DBP[i].X += I.Position.X;
                            DBP[i].Y += I.Position.Y;

                        }
                        debugpoints.AddRange(DBP.ToList());

                    }
                }

                if (count == 0) return;


                foreach (var l in ShapeLists.Values)
                {
                    foreach (var k in l)
                    {
                        foreach (var q in k)
                        {
                            foreach (var p in q.Item2)
                            {
                                Bounds.FitPoint(p.X, p.Y);
                            }
                        }
                    }
                }


            }

            private Bounds GetInstanceBounds(ShapeInstance i)
            {
                return GetShapeBounds(i.TheShape);
            }
        }
    }
}
