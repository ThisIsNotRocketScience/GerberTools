using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberLibrary.Core.Primitives
{
    public class GerberApertureMacroPart
    {
        public enum ApertureMacroTypes
        {
            Circle = 1,
            Line = 2,
            Outline = 4,
            Polygon = 5,
            Moire = 6,
            Line_2 = 20,
            CenterLine = 21,
            Equation = 999,
            LowerLeftLine = 22,
            Thermal = 7
        };


        public string Name;

        public ApertureMacroTypes Type = ApertureMacroTypes.Polygon;
        double Diameter;
        int Sides;
        double Rotation;
        double Xoff;
        double Yoff;
        double Width;
        double Height;

        public class ApertureMacroParam
        {
            public string ExpressionSource;

            public ApertureMacroParam(bool copy)
            {

            }
            public ApertureMacroParam(string format, int lastboundin, GerberNumberFormat GNF, out int lastboundout)
            {
                lastboundout = lastboundin;
                string[] parts = format.Split('X');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].Contains("$"))
                    {
                        ExpressionSource = format;

                        boundparam = lastboundin + 1;
                        lastboundout = lastboundin + 1;
                    }
                    else
                    {
                        double R;
                        if (Gerber.TryParseDouble(parts[i], out R))
                        {
                            value = R;
                            scaledvalue = GNF.ScaleFileToMM(R);
                        }
                        else
                        {
                            Console.WriteLine("failed to parse {0}", format);
                        }
                    }

                }
            }
            public double value = 0;
            public int boundparam = -1;
            public double scaledvalue = 0;

            public double BuildValue(List<double> paramlist)
            {
                if (boundparam > -1)
                {
                    string srccopy = ExpressionSource.Replace("$1", "V1"); ;
                    for (int i = 2; i < paramlist.Count() + 1; i++)
                    {
                        srccopy = srccopy.Replace("$" + i.ToString(), "V" + i.ToString());
                    }
                    srccopy = srccopy.Replace("X", " * ");
                    // srccopy = srccopy.Replace('.', ',');
                    MacroExpressionEvaluator E = new MacroExpressionEvaluator();

                    for (int i = 0; i < paramlist.Count(); i++)
                    {
                        E.Set("V" + (i + 1).ToString(), paramlist[i]);

                    }
                    try
                    {
                        var R = E.Evaluate(srccopy);
                        return R;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error in macro expression syntax: {0}", ExpressionSource);

                        return 0;
                    }
                    //Console.WriteLine("bound param out of range: idx = {0}, {1} total.", boundparam, paramlist.Count());

                    // return value;
                }
                else
                {
                    return value;
                }
            }


            public string BuildGerber(GerberNumberFormat GNF, double rotation, bool isrotationparam, bool isscaled)
            {
                double getval = value;
                if (isscaled) getval = GNF._ScaleMMToFile(scaledvalue);
                if (boundparam > -1)
                {

                    if (rotation != 0 && isrotationparam)
                    {
                        return String.Format("{1}+${0}", boundparam + 1, Gerber.ToFloatingPointString(rotation).Replace(',', '.'));
                    }
                    else
                    {
                        return String.Format("${0}", boundparam + 1);
                    }
                }
                else
                {
                    if (isrotationparam)
                    {
                        return Gerber.ToFloatingPointString((getval + rotation)).Replace(',', '.');
                    }
                    else
                    {
                        return Gerber.ToFloatingPointString(getval).Replace(',', '.');
                    }
                }
            }

            internal string BuildGerberInt(GerberNumberFormat GNF, double rotationdegrees, bool p)
            {
                return ((int)value).ToString();
            }
        }
        public string EquationTarget;
        public String EquationSource;
        List<ApertureMacroParam> Params = new List<ApertureMacroParam>();
        List<ApertureMacroParam> RotateParams(ApertureMacroParam A, ApertureMacroParam B, double rotation)
        {
            List<ApertureMacroParam> Res = new List<ApertureMacroParam>();

            ApertureMacroParam X = new ApertureMacroParam(true) { boundparam = A.boundparam, scaledvalue = A.scaledvalue, value = A.value };
            ApertureMacroParam Y = new ApertureMacroParam(true) { boundparam = B.boundparam, scaledvalue = B.scaledvalue, value = B.value };

            PolyLine R = new PolyLine(PolyLine.PolyIDs.Temp);
            R.Add(X.value, Y.value);
            R.Add(X.scaledvalue, Y.scaledvalue);
            R.RotateDegrees(rotation);

            X.value = R.Vertices[0].X;
            Y.value = R.Vertices[0].Y;
            X.scaledvalue = R.Vertices[1].X;
            Y.scaledvalue = R.Vertices[1].Y;

            Res.Add(X);
            Res.Add(Y);
            return Res;
        }

        public string BuildGerber(GerberNumberFormat GNF, double rotationdegrees)
        {
            string res = "";
            switch (Type)
            {
                case ApertureMacroTypes.Equation:
                    res += String.Format("{0}={1}", EquationTarget, EquationSource);
                    break;
                case ApertureMacroTypes.Line:
                case ApertureMacroTypes.Line_2:
                    {
                        var Rot = RotateParams(Params[3], Params[4], rotationdegrees);

                        res += String.Format("20,1,{0},{1},{2},{3},{4},{5}",
                            //Params[1].BuildGerber(rotationdegrees, false),
                            Params[2].BuildGerber(GNF, rotationdegrees, false, true),
                            Params[3].BuildGerber(GNF, rotationdegrees, false, true),
                            Params[4].BuildGerber(GNF, rotationdegrees, false, true),
                            Params[5].BuildGerber(GNF, rotationdegrees, false, true),
                            Params[6].BuildGerber(GNF, rotationdegrees, false, true),
                            Params[7].BuildGerber(GNF, rotationdegrees, true, false)
                            );
                        break;
                    }

                default:
                    Console.WriteLine("Sorry - I cant make gerber for macro parts of type {0} yet", Type);
                    break;
                case ApertureMacroTypes.Outline:
                    res += String.Format("4,1,{0}," + Gerber.LineEnding, OutlineVertices.Count - 1);
                    if (rotationdegrees != 0)
                    {
                        PolyLine P = new PolyLine(PolyLine.PolyIDs.Aperture);
                        for (int i = 0; i < OutlineVertices.Count; i++)
                        {
                            PointD B = OutlineVertices[i].Get(Params);
                            P.Add(B.X, B.Y);
                        }
                        P.RotateDegrees(rotationdegrees);
                        for (int i = 0; i < P.Vertices.Count(); i++)
                        {

                            res += String.Format("{0},{1}," + Gerber.LineEnding, Gerber.ToFloatingPointString(GNF._ScaleMMToFile(P.Vertices[i].X)).Replace(',', '.'), Gerber.ToFloatingPointString(GNF._ScaleMMToFile(P.Vertices[i].Y)).Replace(',', '.'));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < OutlineVertices.Count; i++)
                        {
                            PointD B = OutlineVertices[i].Get(Params);

                            res += String.Format("{0},{1}," + Gerber.LineEnding, Gerber.ToFloatingPointString(GNF._ScaleMMToFile(B.X)).Replace(',', '.'), Gerber.ToFloatingPointString(GNF._ScaleMMToFile(B.Y)).Replace(',', '.'));
                        }
                    }
                    res += "0";
                    break;
                case ApertureMacroTypes.Polygon:
                    {
                        var Rot = RotateParams(Params[3], Params[4], rotationdegrees);

                        res += String.Format("5,1,{0},{1},{2},{3},{4}",
                            //Params[1].BuildGerber(rotationdegrees, false),
                            Params[2].BuildGerberInt(GNF, rotationdegrees, false),
                            Rot[0].BuildGerber(GNF, rotationdegrees, false, true),
                            Rot[1].BuildGerber(GNF, rotationdegrees, false, true),
                            Params[5].BuildGerber(GNF, rotationdegrees, false, false),
                            Params[6].BuildGerber(GNF, rotationdegrees, true, false)
                            );
                        break;
                    }
                case ApertureMacroTypes.Thermal:
                    {
                        var Rot = RotateParams(Params[1], Params[2], rotationdegrees);

                        res += String.Format("7,1,{0},{1},{2},{3},{4},{5}",
                            Rot[0].BuildGerber(GNF, rotationdegrees, false, true),
                            Rot[1].BuildGerber(GNF, rotationdegrees, false, true),
                            Params[3].BuildGerber(GNF, rotationdegrees, false, false),
                            Params[4].BuildGerber(GNF, rotationdegrees, false, false),
                            Params[5].BuildGerber(GNF, rotationdegrees, false, false),
                            Params[6].BuildGerber(GNF, rotationdegrees, true, false)
                            );
                        break;
                    }

                case ApertureMacroTypes.CenterLine:
                    {

                        var Rot = RotateParams(Params[4], Params[5], rotationdegrees);
                        res += String.Format("21,1,{0},{1},{2},{3},{4}",
                           //Params[1].BuildGerber(rotationdegrees, false),
                           Params[2].BuildGerber(GNF, rotationdegrees, false, true),
                           Params[3].BuildGerber(GNF, rotationdegrees, false, true),
                          Rot[0].BuildGerber(GNF, rotationdegrees, false, true),
                           Rot[1].BuildGerber(GNF, rotationdegrees, false, true),
                           Params[6].BuildGerber(GNF, rotationdegrees, true, false)
                           );
                    }
                    break;
                case ApertureMacroTypes.LowerLeftLine:
                    {

                        var Rot = RotateParams(Params[4], Params[5], rotationdegrees);
                        res += String.Format("21,1,{0},{1},{2},{3},{4}",
                           //Params[1].BuildGerber(rotationdegrees, false),
                           Params[2].BuildGerber(GNF, rotationdegrees, false, true),
                           Params[3].BuildGerber(GNF, rotationdegrees, false, true),
                          Rot[0].BuildGerber(GNF, rotationdegrees, false, true),
                           Rot[1].BuildGerber(GNF, rotationdegrees, false, true),
                           Params[6].BuildGerber(GNF, rotationdegrees, true, false)
                           );
                    }
                    break;
                case ApertureMacroTypes.Circle:
                    //res += "circ";
                    {
                        var Rot = RotateParams(Params[3], Params[4], rotationdegrees);

                        res += String.Format("1,1,{0},{1},{2}",
                           //Params[1].BuildGerber(rotationdegrees, false),
                           Params[2].BuildGerber(GNF, rotationdegrees, false, true),
                           Rot[0].BuildGerber(GNF, rotationdegrees, false, true),
                           Rot[1].BuildGerber(GNF, rotationdegrees, false, true)
                           );
                    }
                    break;
            }
            foreach (var a in Params)
            {

            }
            res += "*" + Gerber.LineEnding;
            return res;
        }

        public void Decode(string Format, GerberNumberFormat GNF)
        {
            string[] parts = Format.Split(',');
            Params.Clear();
            int lastboundparameter = -1;
            for (int i = 0; i < parts.Length; i++)
            {
                Params.Add(new ApertureMacroParam(parts[i], lastboundparameter, GNF, out lastboundparameter));
            }

            Type = (ApertureMacroTypes)(int)Params[0].value;
        }

        public GerberApertureType BuildAperture(List<double> paramlist, GerberNumberFormat GNF)
        {
            //List<double> paramlist = _paramlist.ToList();
            GerberApertureType AT = new GerberApertureType();
            AT.MacroParamList = paramlist.ToList();
            AT.Shape.Vertices.Clear();

            switch (Type)
            {
                case ApertureMacroTypes.Equation:

                    int T = int.Parse(EquationTarget.Substring(1));
                    while (paramlist.Count() < T) paramlist.Add(0);
                    {
                        string srccopy = EquationSource.Replace("$1", "V1");
                        for (int i = 2; i <= paramlist.Count(); i++)
                        {
                            srccopy = srccopy.Replace("$" + i.ToString(), "V" + i.ToString());

                        }
                        srccopy = srccopy.Replace("X", "*");
                        MacroExpressionEvaluator E = new MacroExpressionEvaluator();
                        for (int i = 0; i < paramlist.Count(); i++)
                        {
                            E.Set("V" + (i + 1).ToString(), paramlist[i]);

                        }
                        while (paramlist.Count() < T) paramlist.Add(0);

                        paramlist[T - 1] = E.Evaluate(srccopy); ;
                        if (Gerber.ShowProgress)
                        {
                            Console.Write("equation {0}={1} -> {2}. Paramlist: ", EquationTarget, EquationSource, paramlist[T - 1]);
                            int c = 1;
                            foreach (var a in paramlist)
                            {
                                Console.Write(" {0}:{1},", c, a);
                                c++;
                            }
                            Console.WriteLine();
                        }
                        AT.MacroParamList = paramlist.ToList();
                    }

                    break;
                case ApertureMacroTypes.Polygon:
                    {
                        if (Gerber.ShowProgress) Console.WriteLine("Making an aperture for polygon. {0} params. {1} in paramlist", Params.Count, paramlist.Count());

                        Sides = (int)Params[2].BuildValue(paramlist);
                        var diamvalue = Params[5].BuildValue(paramlist);
                        Diameter = GNF.ScaleFileToMM(diamvalue);
                        Rotation = Params[6].BuildValue(paramlist);
                        Xoff = GNF.ScaleFileToMM(Params[3].BuildValue(paramlist));
                        Yoff = GNF.ScaleFileToMM(Params[4].BuildValue(paramlist));
                        AT.NGon(Sides, Diameter / 2, Xoff, Yoff, Rotation);
                    }
                    break;
                case ApertureMacroTypes.Outline:
                    if (Gerber.ShowProgress) Console.WriteLine("Making an aperture for outline. {0} params. {1} in paramlist", Params.Count, paramlist.Count());
                    OutlineVerticesPostProc = new List<PointD>();
                    foreach(var a in OutlineVertices)
                    {
                        OutlineVerticesPostProc.Add(a.Get(Params));
                    }
                    AT.SetCustom(OutlineVerticesPostProc);
                    break;
                case ApertureMacroTypes.Circle:
                    if (Gerber.ShowProgress) Console.WriteLine("Making an aperture for circle. {0} params. {1} in paramlist", Params.Count, paramlist.Count());
                    Diameter = GNF.ScaleFileToMM(Params[2].BuildValue(paramlist));
                    Xoff = GNF.ScaleFileToMM(Params[3].BuildValue(paramlist));
                    Yoff = GNF.ScaleFileToMM(Params[4].BuildValue(paramlist));
                    AT.SetCircle(Diameter / 2, Xoff, Yoff);

                    break;

                case ApertureMacroTypes.CenterLine:
                    {
                        if (Gerber.ShowProgress) Console.WriteLine("Making an aperture for centerline. {0} params. {1} in paramlist", Params.Count, paramlist.Count());
                        {
                            //1 Exposure off/on (0/1))
                            //2 Rectangle width, a decimal ≥ 0.
                            //3 Rectangle height, a decimal ≥ 0.
                            //4 A decimal defining the X coordinate of center point.
                            //5 A decimal defining the Y coordinate of center point.
                            //6 A decimal defining the rotation angle around the origin (rotation is notaround the center of the object)
                        }
                        Width = GNF.ScaleFileToMM(Params[2].BuildValue(paramlist));
                        Height = GNF.ScaleFileToMM(Params[3].BuildValue(paramlist));
                        Xoff = GNF.ScaleFileToMM(Params[4].BuildValue(paramlist));
                        Yoff = GNF.ScaleFileToMM(Params[5].BuildValue(paramlist));
                        Rotation = Params[6].BuildValue(paramlist);
                        AT.SetRotatedRectangle(Width, Height, Rotation, Xoff, Yoff);
                        //AT.ShapeType = GerberApertureShape.CenterLine;
                    }
                    break;
                case ApertureMacroTypes.LowerLeftLine:
                    {
                        if (Gerber.ShowProgress) Console.WriteLine("Making an aperture for lowerleftline. {0} params. {1} in paramlist", Params.Count, paramlist.Count());
                        {
                            // 1 Exposure off/on (0/1))
                            // 2 Rectangle width, a decimal ≥ 0.
                            // 3 Rectangle height, a decimal ≥ 0.
                            // 4 A decimal defining the X coordinate of lower left point.
                            // 5 A decimal defining the Y coordinate of lower left point.
                            // 6 A decimal defining the rotation angle around the origin (rotation is not around the center of the object)
                        }

                        Width = GNF.ScaleFileToMM(Params[2].BuildValue(paramlist));
                        Height = GNF.ScaleFileToMM(Params[3].BuildValue(paramlist));
                        Xoff = GNF.ScaleFileToMM(Params[4].BuildValue(paramlist));
                        Yoff = GNF.ScaleFileToMM(Params[5].BuildValue(paramlist));
                        Rotation = Params[6].BuildValue(paramlist);
                        AT.SetRotatedRectangle(Width, Height, Rotation, Xoff + Width / 2, Yoff + Height / 2);
                    }
                    break;
                case ApertureMacroTypes.Thermal:
                    if (Gerber.ShowProgress) Console.WriteLine("Making an aperture for moire. {0} params. {1} in paramlist", Params.Count, paramlist.Count());

                    Xoff = GNF.ScaleFileToMM(Params[1].BuildValue(paramlist));
                    Yoff = GNF.ScaleFileToMM(Params[2].BuildValue(paramlist));
                    OuterDiameter = GNF.ScaleFileToMM(Params[3].BuildValue(paramlist));
                    InnerDiameter = GNF.ScaleFileToMM(Params[4].BuildValue(paramlist));
                    GapWidth = GNF.ScaleFileToMM(Params[5].BuildValue(paramlist));
                    Rotation = Params[6].BuildValue(paramlist);

                    AT.SetThermal(Xoff, Yoff, OuterDiameter, InnerDiameter, GapWidth, Rotation);

                    //1 A decimal defining the X coordinate of center point
                    //2 A decimal defining the Y coordinate of center point
                    //3 Outer diameter, must be a decimal and > inner diameter
                    //4 Inner diameter, must be a decimal and ≥ 0
                    //5 Gap thickness, must be a decimal < (outer diameter)/√2
                    //6 A decimal defining the rotation angle around the origin. Rotation is
                    //only allowed if the center point is on the origin. If the rotation angle is
                    //zero the gaps are on the X and Y axes through the center.



                    break;
                case ApertureMacroTypes.Moire:

                    if (Gerber.ShowProgress) Console.WriteLine("Making an aperture for moire. {0} params. {1} in paramlist", Params.Count, paramlist.Count());

                    Xoff = GNF.ScaleFileToMM(Params[1].BuildValue(paramlist));
                    Yoff = GNF.ScaleFileToMM(Params[2].BuildValue(paramlist));
                    OuterDiameter = GNF.ScaleFileToMM(Params[3].BuildValue(paramlist));
                    Width = GNF.ScaleFileToMM(Params[4].BuildValue(paramlist));
                    RingGap = GNF.ScaleFileToMM(Params[5].BuildValue(paramlist));
                    MaxRings = (int)Params[6].BuildValue(paramlist);
                    CrossHairThickness = GNF.ScaleFileToMM(Params[7].BuildValue(paramlist));
                    CrossHairLength = GNF.ScaleFileToMM(Params[8].BuildValue(paramlist));
                    Rotation = GNF.ScaleFileToMM(Params[9].BuildValue(paramlist));

                    AT.SetMoire(Xoff, Yoff, OuterDiameter, Width, RingGap, MaxRings, CrossHairThickness, CrossHairLength, Rotation);
                    //1 A decimal defining the X coordinate of center point.
                    //2 A decimal defining the Y coordinate of center point.
                    //3 A decimal defining the outer diameter of outer concentric ring
                    //4 A decimal defining the ring thickness
                    //5 A decimal defining the gap between rings
                    //6 Maximum number of rings
                    //7 A decimal defining the cross hair thickness
                    //8 A decimal defining the cross hair length
                    //9 A decimal defining the rotation angle around the origin. Rotation is only allowed if the center point is on the origin.

                    break;


                case ApertureMacroTypes.Line_2:
                case ApertureMacroTypes.Line:
                    {
                        if (Gerber.ShowProgress) Console.WriteLine("Making an aperture for line. {0} params. {1} in paramlist", Params.Count, paramlist.Count());
                        {
                            //1 Exposure off/on (0/1)
                            //2 Line width, a decimal ≥ 0.
                            //3 A decimal defining the X coordinate of start point.
                            //4 A decimal defining the Y coordinate of start point.
                            //5 A decimal defining the X coordinate of end point.
                            //6 A decimal defining the Y coordinate of end point.
                            //7 A decimal defining the rotation angle around the origin (rotation is not around the center of the object)
                        }
                        Width = GNF.ScaleFileToMM(Params[2].BuildValue(paramlist));
                        Xoff = GNF.ScaleFileToMM(Params[3].BuildValue(paramlist));
                        Yoff = GNF.ScaleFileToMM(Params[4].BuildValue(paramlist));
                        Xend = GNF.ScaleFileToMM(Params[5].BuildValue(paramlist));
                        Yend = GNF.ScaleFileToMM(Params[6].BuildValue(paramlist));
                        Rotation = Params[7].BuildValue(paramlist);




                        AT.SetLineSegment(new PointD(Xoff, Yoff), new PointD(Xend, Yend), Width, Rotation);
                    }
                    break;
                default:
                    Console.WriteLine("I don't know how to make an aperture for macro type {0}!", Type);
                    break;
            }

            AT.ShapeType = GerberApertureShape.Macro;
            AT.MacroName = Name;

            return AT;
        }
        public class OutlineParameterPoint
        {
            public PointD Point = new PointD();
            public int xparamID = -1;
            public int yparamID = -1;
            public bool xParamBound = false;
            public bool yParamBound = false;

            internal PointD Get(List<ApertureMacroParam> theparams)
            {
                if (xParamBound == false && yParamBound == false) return Point;
                PointD R = new PointD(Point.X, Point.Y);
                if (xParamBound) R.X = theparams[xparamID].value;
                if (yParamBound) R.Y = theparams[yparamID].value;
                return R;
            }
        }
        public List<OutlineParameterPoint> OutlineVertices;
        public List<PointD> OutlineVerticesPostProc;
        private double Xend;
        private double Yend;
        private double OuterDiameter;
        private double InnerDiameter;
        private double GapWidth;
        private double RingGap;
        private int MaxRings;
        private double CrossHairThickness;
        private double CrossHairLength;

        public void DecodeOutline(string line, GerberNumberFormat GNF)
        {
            OutlineVertices = new List< OutlineParameterPoint>();
            string[] v = line.Split(',');

            //  if (Gerber.Verbose) Console.WriteLine("decoding {0}", lines[currentline]);
            int vertices = Int32.Parse(v[2]) + 1;
            if (Gerber.ShowProgress) Console.WriteLine("{0} vertices", vertices);
            int idx = 2;
            int i = 0;
            if ((v.Length - 3 - vertices * 2) < 0) vertices--;
            double X = 0;
            double Y = 0;
            while (i < vertices && v[idx + 1].Contains("*") == false)
            //            for (int i = 0; i < vertices; i++)
            {
                bool xparambound = false;
                bool yparambound = false;
                int xid = -1;
                int yid = -1;

                idx++;
                //  if (Gerber.Verbose) Console.Write("{1}| reading X from {0}: ", idx, i);            
                if (v[idx].Contains("$"))
                {
                    Console.WriteLine("{0}", v[idx]);
                    xparambound = true;
                    xid = int.Parse(v[idx].Substring(1));
                }
                else
                {

                     X = Gerber.ParseDouble(v[idx]);
                }
                //  if (Gerber.Verbose) Console.WriteLine(" {0} ", X);
                idx++;
                // if (Gerber.Verbose) Console.Write("{1}| reading Y from {0}: ", idx,i);            
                if (v[idx].Contains("$"))
                {
                    Console.WriteLine("{0}", v[idx]);
                    yparambound = true;
                    yid = int.Parse(v[idx].Substring(1));

                }
                else
                {
                    Y = Gerber.ParseDouble(v[idx]);
                }
                //if (Gerber.Verbose) Console.WriteLine(" {0} ", Y);

                X = GNF.ScaleFileToMM(X);
                Y = GNF.ScaleFileToMM(Y);
                OutlineVertices.Add(new OutlineParameterPoint() { Point = new PointD(X, Y) , xParamBound =xparambound, yparamID = yid, xparamID = xid, yParamBound = yparambound});
                i++;

            }

            //       throw new NotImplementedException();
        }

        internal void DecodeCircle(string p, GerberNumberFormat GNF)
        {
            Decode(p, GNF);
            // 1 Exposure off/on (0/1)
            // 2 Diameter, a decimal ≥ 0.
            // 3 A decimal defining the X coordinate of center position.
            // 4 A decimal defining the Y coordinate of center position.


        }

        internal void DecodeCenterLine(string p, GerberNumberFormat GNF)
        {
            Decode(p, GNF);

            //1 Exposure off/on (0/1))
            //2 Rectangle width, a decimal ≥ 0.
            //3 Rectangle height, a decimal ≥ 0.
            //4 A decimal defining the X coordinate of center point.
            //5 A decimal defining the Y coordinate of center point.
            //6 A decimal defining the rotation angle around the origin (rotation is notaround the center of the object)
        }

        internal void DecodeLine(string p, GerberNumberFormat GNF)
        {
            Decode(p, GNF);

            //1 Exposure off/on (0/1)
            //2 Line width, a decimal ≥ 0.
            //3 A decimal defining the X coordinate of start point.
            //4 A decimal defining the Y coordinate of start point.
            //5 A decimal defining the X coordinate of end point.
            //6 A decimal defining the Y coordinate of end point.
            //7 A decimal defining the rotation angle around the origin (rotation is not around the center of the object)
        }

        internal void DecodeLowerLeftLine(string a, GerberNumberFormat GNF)
        {
            Decode(a, GNF);
            // 1 Exposure off/on (0/1))
            // 2 Rectangle width, a decimal ≥ 0.
            // 3 Rectangle height, a decimal ≥ 0.
            // 4 A decimal defining the X coordinate of lower left point.
            // 5 A decimal defining the Y coordinate of lower left point.
            // 6 A decimal defining the rotation angle around the origin (rotation is not around the center of the object)
        }
    }

}
