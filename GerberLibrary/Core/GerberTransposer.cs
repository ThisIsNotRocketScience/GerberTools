using GerberLibrary.Core;
using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberLibrary
{
    public class GerberTransposer
    {
        public GerberTransposer()
        {
            GerberSplitter GS = new GerberSplitter();
            GerberNumberFormat Format = new GerberNumberFormat();
            Format.DigitsAfter = 3;
            Format.DigitsBefore = 6;

            GS.Split("X123456789Y123456789", Format);

            if (Gerber.ShowProgress)
            {
                Console.WriteLine(GS.Rebuild(Format));
                Console.WriteLine("hmm");
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourcefile"></param>
        /// <param name="destfile"></param>
        /// <param name="DX">MM</param>
        /// <param name="DY">MM</param>
        /// <param name="Angle">Degrees</param>
        public static void Transform(string sourcefile, string destfile, double DX, double DY, double DXp, double DYp, double AngleInDeg = 0) 
        {
            List<String> lines = new List<string>();
            List<String> outlines = new List<string>();


            bool WriteMove = false;
            int moveswritten = 0;
            double Angle = AngleInDeg * (Math.PI * 2.0) / 360.0;
            double CA = Math.Cos(Angle);
            double SA = Math.Sin(Angle);
            using (StreamReader sr = new StreamReader(sourcefile))
            {
                while (sr.EndOfStream == false)
                {
                    String line = sr.ReadLine();
                    if (line.Length > 0)
                    {
                        lines.Add(line);
                    }
                }                
            }
            lines = PolyLineSet.SanitizeInputLines(lines);

            if (Gerber.WriteSanitized) Gerber.WriteAllLines(sourcefile + ".sanitized.txt", lines);
         //   PolyLineSet Parsed = new PolyLineSet("parsed gerber");
            ParsedGerber Parsed = PolyLineSet.ParseGerber274x(lines, true, false, new GerberParserState() { GenerateGeometry = false });

            if (Gerber.ShowProgress)
            {
                Console.WriteLine("found apertures: ");
                foreach (var a in Parsed.State.Apertures)
                {
                    Console.WriteLine(a.Value.ToString());
                }
            }
            
            

            GerberNumberFormat CoordinateFormat = new GerberNumberFormat();
            CoordinateFormat.SetImperialMode();
         //   CoordinateFormat = Parsed.State.CoordinateFormat;

            int cur = 0;
            bool formatparsed = false;
            while (cur < lines.Count && formatparsed == false)
            {
                if (lines[cur].Length >= 2 && lines[cur].Substring(0, 2) == "%F")
                {
                     CoordinateFormat.Parse(lines[cur]);
                    formatparsed = true;
                }
                cur++;
            }
            
          //  double coordmultiplier = 1.0;
            double LastX = 0;
            double LastY = 0;
            for(int i =0 ;i<lines.Count;i++)
            {

                GerberSplitter GS = new GerberSplitter();
                string FinalLine = lines[i].Replace("%", "").Replace("*", "").Trim();

                bool DumpToOutput = false;
                bool metaopen = false;
                if (lines[i][0] == '%')
                {
                    DumpToOutput = true;
                    metaopen = true;

                }
                else
                {
                    GS.Split(lines[i], CoordinateFormat);
                }


                switch (FinalLine)
                {
                    case "G71": CoordinateFormat.SetMetricMode(); break;
                    case "G70": CoordinateFormat.SetImperialMode(); break;

                    case "MOIN":
                        {
                            CoordinateFormat.SetImperialMode();
                            //CoordinateFormat.Multiplier  = 25.4f;
                        }
                        break;
                    case "MOMM":
                        {
                            CoordinateFormat.SetMetricMode();
                            //CoordinateFormat.Multiplier = 1.0f;
                        }
                        break;
                }
                if (lines[i].Length > 3 && lines[i].Substring(0, 3) == "%AM")
                {
                    string name = lines[i].Substring(3).Split('*')[0];

                    var M = Parsed.State.ApertureMacros[name];
                    M.Written = true;
                    var gerb = M.BuildGerber(CoordinateFormat, AngleInDeg).Split('\n');
                    foreach (var l in gerb)
                    {
                        if (l.Trim().Length > 0)
                        {
                            outlines.Add(l.Trim());
                        }
                    }
                //    outlines.Add(lines[i]);
                  //  if (lines[i][lines[i].Length - 1] != '%')
                  ///  {
                     //   i++;
                        while (lines[i][lines[i].Length - 1] != '%')
                        {
                       //     outlines.Add(lines[i]);
                            i++;
                        }
//                       outlines.Add(lines[i]);
               //     }
                }
                else
                
                if (lines[i].Length > 3 &&lines[i].Substring(0, 3) == "%AD")
                {
                    GCodeCommand GCC = new GCodeCommand();
                    GCC.Decode(lines[i], CoordinateFormat);
                    if (GCC.numbercommands.Count < 1)
                    {
                        Console.WriteLine("Skipping bad aperture definition: {0}", lines[i]);
                    }
                    else
                    {
                        int ATID = (int)GCC.numbercommands[0];
                        var Aperture = Parsed.State.Apertures[ATID];
                        if (Gerber.ShowProgress) Console.WriteLine("found " + Aperture.ToString());
                        string gerb = Aperture.BuildGerber(CoordinateFormat, AngleInDeg);

                        if ((Aperture.ShapeType == GerberApertureShape.Compound || Aperture.ShapeType == GerberApertureShape.Macro) && Parsed.State.ApertureMacros[Aperture.MacroName].Written == false)
                        {
                            Console.WriteLine("Macro type defined - skipping");
                        }
                        else
                        {
                            outlines.Add(gerb);
                        }

                        //                   outlines.Add(lines[i]);
                        if (lines[i][lines[i].Length - 1] != '%')
                        {
                            i++;
                            while (lines[i] != "%")
                            {
                                //                         outlines.Add(lines[i]);
                                i++;
                            }
                            //                   outlines.Add(lines[i]);
                        }
                    }
                }
                else
                {
                    bool PureD = false;
                    if (GS.Has("G"))
                    {
                        int GCode = (int)GS.Get("G");
                        switch (GCode)
                        {
                            case 4: DumpToOutput = true; break;
                            case 90: CoordinateFormat.Relativemode = false; break;
                            case 91: CoordinateFormat.Relativemode = true; break;
                            case 71: CoordinateFormat.Multiplier = 1.0d; break;
                            case 70: CoordinateFormat.Multiplier = 25.4d; break;
                        }
                    }
                    if (DumpToOutput)
                    {
                        outlines.Add(lines[i]);
                        if (lines[i].Contains("LNData"))
                        {
                            Console.WriteLine(" heh");
                        }
                        if (lines[i][0] == '%')
                        {
                            int starti = i;
                            if (lines[i].Length == 1) i++;
                            while (lines[i][lines[i].Length - 1] != '%')
                            {
                                if (i > starti) outlines.Add(lines[i]);
                                i++;
                            }
                            if (i>starti) outlines.Add(lines[i]); 
                        }
                    }
                    else
                    {
                        bool translate = true;
                        if (CoordinateFormat.Relativemode) translate = false;
                        if (GS.Has("X")== false && GS.Has("Y")==false && (GS.Has("D")&& GS.Get("D" )<10 ))
                        {
                            PureD = true;
                            int Dcode = (int)GS.Get("D");
                            if (Dcode == 1 || Dcode == 3)
                            {
                                if (moveswritten == 0)
                                {
                                    WriteMove = true;
                                }
                            }
                            moveswritten++;
                            Console.WriteLine(" Pure D Code: {0}", lines[i]);
                        }
                        else
                        if (GS.Has("X") || GS.Has("Y") || (GS.Has("D") && GS.Get("D") < 10))
                        {
                            int Dcode = (int)GS.Get("D");
                            if (Dcode == 1 || Dcode == 3)
                            {
                                if (moveswritten == 0)
                                {
                                    WriteMove = true;
                                }
                            }
                            moveswritten++;
                            double X = LastX;
                            if (GS.Has("X")) X = GS.Get("X");
                            double Y = LastY;
                            if (GS.Has("Y")) Y = GS.Get("Y");
                            LastX = X;
                            LastY = Y;
                           GetTransformedCoord(DX, DY, DXp, DYp, Angle, CA, SA, CoordinateFormat, translate, ref X, ref Y);
                            if ((GS.Has("I") || GS.Has("J"))  && Angle != 0)
                            {
                             //   int g = (int)GS.Get("G");
                              //  if (g == 2 || g == 3)
                                {
                                    double I = 0;
                                    double J = 0;
                                    bool arc = false;
                                    if (GS.Has("I")) { I = GS.Get("I"); arc = true; };
                                    if (GS.Has("J")) {J = GS.Get("J"); arc = true; };
                                    if (arc)
                                    {
                                        double nJ = J * CA + I * SA;
                                        double nI = -J * SA + I * CA;
                                        I = nI;
                                        J = nJ;
                                        //  GS.Set("I", Math.Abs(I));
                                        //  GS.Set("J", Math.Abs(J));
                                        GS.Set("I", I);
                                        GS.Set("J", J);
                                    }
                                }
                            }
                            GS.Set("X", X);
                            GS.Set("Y", Y);
                        }
                     
                        if (WriteMove)
                        {
                            GerberSplitter GS2 = new GerberSplitter();
                            GS2.Set("D", 2);
                                double X0 = 0;
                            double Y0 = 0;
                                GetTransformedCoord(DX, DY, DXp, DYp, Angle, CA, SA, CoordinateFormat, translate, ref X0, ref Y0);
                                GS2.Set("X", X0);
                                GS2.Set("Y", Y0);
                                WriteMove = false;
                            outlines.Add(GS2.Rebuild(CoordinateFormat));
                        }
                        outlines.Add(GS.Rebuild(CoordinateFormat));
                        if (PureD)
                        {
                            Console.WriteLine("pureD");
                        }

                    }
                }

            }
            try
            {
                List<String> PostProcLines = new List<string>();
                foreach (var a in outlines)
                {
                    if (a == "%")
                    {
                        PostProcLines[PostProcLines.Count - 1] += "%";
                    }
                    else
                    {
                        PostProcLines.Add(a);
                    }
                }
                Gerber.WriteAllLines(destfile, PolyLineSet.SanitizeInputLines( PostProcLines));
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
        }

        public static void GetTransformedCoord(double DX, double DY, double DXp, double DYp, double Angle, double CA, double SA, GerberNumberFormat CoordinateFormat, bool translate, ref double X, ref double Y)
        {
            if (translate)
            {
                X = (X * CoordinateFormat.Multiplier + DXp) / CoordinateFormat.Multiplier;
                Y = (Y * CoordinateFormat.Multiplier + DYp) / CoordinateFormat.Multiplier;
            }
            if (Angle != 0)
            {
                double nX = X * CA - Y * SA;
                double nY = X * SA + Y * CA;
                X = nX;
                Y = nY;
            }
            if (translate)
            {
                X = (X * CoordinateFormat.Multiplier + DX) / CoordinateFormat.Multiplier;
                Y = (Y * CoordinateFormat.Multiplier + DY) / CoordinateFormat.Multiplier;
            }
        }


    }
}
