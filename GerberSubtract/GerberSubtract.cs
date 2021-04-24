using GerberLibrary;
using GerberLibrary.Core;
using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberSubtract
{
    class GerberSubtract
    {
        public static void Subtract(ProgressLog log, string inputfile, string subtractfile, string outputfile)
        {
            log.PushActivity("Subtracting gerbers");
            ParsedGerber inp = PolyLineSet.LoadGerberFile(new StandardConsoleLog(), inputfile,false, false, new GerberParserState() { GenerateGeometry = false });
            ParsedGerber subtr = PolyLineSet.LoadGerberFile(new StandardConsoleLog(), subtractfile, false, false, new GerberParserState() { GenerateGeometry = false });

            var inph = inp.GetApertureHashTable();
            var subh = subtr.GetApertureHashTable();
            Dictionary<string, Tuple<GerberApertureType, List<PointD>>> OverlapList = new Dictionary<string, Tuple<GerberApertureType, List<PointD>>>();
            foreach (var a in inph)
            {
                if (subh.ContainsKey(a.Key))
                {
                    Console.WriteLine("found matching aperture: {0} -> {1}", a.Value, subh[a.Key]);
                    OverlapList[a.Key] = new Tuple<GerberApertureType, List<PointD>>(a.Value, new List<PointD>());
                }
            }

            OverlapList = FillOverlapList(log, OverlapList, subtr, subtractfile);
            List<String> lines = new List<string>();
            List<String> outlines = new List<string>();

            int CurrentAperture = 10;
            bool WriteMove = false;
            int moveswritten = 0;
            using (StreamReader sr = new StreamReader(inputfile))
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

            ParsedGerber Parsed = inp;// PolyLineSet.ParseGerber274x(log, lines, true, false, new GerberParserState() { GenerateGeometry = false });

            if (Gerber.ShowProgress)
            {
                log.AddString("found apertures: ");
                foreach (var a in Parsed.State.Apertures)
                {
                    log.AddString(a.Value.ToString());
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
            for (int i = 0; i < lines.Count; i++)
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
                    var gerb = M.BuildGerber(CoordinateFormat, 0, "").Split('\n');
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

                if (lines[i].Length > 3 && lines[i].Substring(0, 3) == "%AD")
                {
                    GCodeCommand GCC = new GCodeCommand();
                    GCC.Decode(lines[i], CoordinateFormat);
                    if (GCC.numbercommands.Count < 1)
                    {
                        log.AddString(String.Format("Skipping bad aperture definition: {0}", lines[i]));
                    }
                    else
                    {
                        int ATID = (int)GCC.numbercommands[0];
                        var Aperture = Parsed.State.Apertures[ATID];
                        if (Gerber.ShowProgress) log.AddString(String.Format("found " + Aperture.ToString()));
                        string gerb = Aperture.BuildGerber(CoordinateFormat, "", 0);

                        if ((Aperture.ShapeType == GerberApertureShape.Compound || Aperture.ShapeType == GerberApertureShape.Macro) && Parsed.State.ApertureMacros[Aperture.MacroName].Written == false)
                        {
                            log.AddString(String.Format("Macro type defined - skipping"));
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
                            log.AddString(String.Format(" heh"));
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
                            if (i > starti) outlines.Add(lines[i]);
                        }
                    }
                    else
                    {
                        bool translate = true;
                        if (CoordinateFormat.Relativemode) translate = false;
                        if (GS.Has("X") == false && GS.Has("Y") == false && (GS.Has("D") && GS.Get("D") < 10))
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
                            //log.AddString(String.Format("Pure D Code: {0}", lines[i]));
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

                            GS.Set("X", X);
                            GS.Set("Y", Y);
                        }
                        if (GS.Get("D") >= 10)
                        {
                            CurrentAperture = (int)GS.Get("D");
                            // Select Aperture;
                        }
                        string hash = inp.State.Apertures[CurrentAperture].GetApertureHash();
                        bool writeline = true;
                        if (OverlapList.ContainsKey(hash) && ((int)GS.Get("D") == 3 || (int)GS.Get("D") == 1))
                        {
                            PointD n = new PointD(LastX, LastY);
                            if (OverlapList[hash].Item2.Contains(n))
                            {
//                                Console.WriteLine("eliminate this!");
                                GS.Set("D", 2);
                            }
//                            Console.WriteLine("Potential Skipping Segment Found!");
                        }
                        if (WriteMove)
                        {
                            GerberSplitter GS2 = new GerberSplitter();
                            GS2.Set("D", 2);
                            double X0 = 0;
                            double Y0 = 0;
                            //GetTransformedCoord(DX, DY, DXp, DYp, Angle, CA, SA, CoordinateFormat, translate, ref X0, ref Y0);
                            GS2.Set("X", X0);
                            GS2.Set("Y", Y0);
                            WriteMove = false;
                            outlines.Add(GS2.Rebuild(CoordinateFormat));
                        }
                        outlines.Add(GS.Rebuild(CoordinateFormat));
                        if (PureD)
                        {
                            //log.AddString(String.Format("pureD"));
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
                Gerber.WriteAllLines(outputfile, PolyLineSet.SanitizeInputLines(PostProcLines));
            }
            catch (Exception E)
            {
                log.AddString(String.Format(E.Message));
            }
            log.PopActivity();


        }

        private static Dictionary<string, Tuple<GerberApertureType, List<PointD>>> FillOverlapList(ProgressLog log, Dictionary<string, Tuple<GerberApertureType, List<PointD>>> overlapList, ParsedGerber subtr, string subtractfile)
        {
            log.PushActivity("Filling overlap list coords");
            List<String> lines = new List<string>();
            List<String> outlines = new List<string>();

            int CurrentAperture = 10;
            bool WriteMove = false;
            int moveswritten = 0;
            using (StreamReader sr = new StreamReader(subtractfile))
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

            //ParsedGerber Parsed = PolyLineSet.ParseGerber274x(log, lines, true, false, new GerberParserState() { GenerateGeometry = false });





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
            for (int i = 0; i < lines.Count; i++)
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

                    var M = subtr.State.ApertureMacros[name];
                    M.Written = true;
                    var gerb = M.BuildGerber(CoordinateFormat, 0, "").Split('\n');
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

                if (lines[i].Length > 3 && lines[i].Substring(0, 3) == "%AD")
                {
                    GCodeCommand GCC = new GCodeCommand();
                    GCC.Decode(lines[i], CoordinateFormat);
                    if (GCC.numbercommands.Count < 1)
                    {
                        log.AddString(String.Format("Skipping bad aperture definition: {0}", lines[i]));
                    }
                    else
                    {
                        int ATID = (int)GCC.numbercommands[0];
                        var Aperture = subtr.State.Apertures[ATID];
                        if (Gerber.ShowProgress) log.AddString(String.Format("found " + Aperture.ToString()));
                        string gerb = Aperture.BuildGerber(CoordinateFormat, "", 0);

                        if ((Aperture.ShapeType == GerberApertureShape.Compound || Aperture.ShapeType == GerberApertureShape.Macro) && subtr.State.ApertureMacros[Aperture.MacroName].Written == false)
                        {
                            log.AddString(String.Format("Macro type defined - skipping"));
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
                            log.AddString(String.Format(" heh"));
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
                            if (i > starti) outlines.Add(lines[i]);
                        }
                    }
                    else
                    {
                        bool translate = true;
                        if (CoordinateFormat.Relativemode) translate = false;
                        if (GS.Has("X") == false && GS.Has("Y") == false && (GS.Has("D") && GS.Get("D") < 10))
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
                            //log.AddString(String.Format("Pure D Code: {0}", lines[i]));
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

                            GS.Set("X", X);
                            GS.Set("Y", Y);
                        }
                        if (GS.Get("D") >= 10)
                        {
                            CurrentAperture = (int)GS.Get("D");
                            // Select Aperture;
                        }
                        string hash = subtr.State.Apertures[CurrentAperture].GetApertureHash();
                        if (overlapList.ContainsKey(hash))
                        {
                            overlapList[hash].Item2.Add(new PointD(LastX, LastY));
                        }
                        if (WriteMove)
                        {
                            GerberSplitter GS2 = new GerberSplitter();
                            GS2.Set("D", 2);
                            double X0 = 0;
                            double Y0 = 0;
                            if (overlapList.ContainsKey(hash))
                            {
                                overlapList[hash].Item2.Add(new PointD(0, 0));
                            }
                            //GetTransformedCoord(DX, DY, DXp, DYp, Angle, CA, SA, CoordinateFormat, translate, ref X0, ref Y0);
                            GS2.Set("X", X0);
                            GS2.Set("Y", Y0);
                            WriteMove = false;
                            //                          outlines.Add(GS2.Rebuild(CoordinateFormat));
                        }
                        //                        outlines.Add(GS.Rebuild(CoordinateFormat));
                        if (PureD)
                        {
                            //log.AddString(String.Format("pureD"));
                        }

                    }
                }

            }
            try
            {
                //            List<String> PostProcLines = new List<string>();
                //              foreach (var a in outlines)
                //                {
                //  if (a == "%")
                //    {
                //          PostProcLines[PostProcLines.Count - 1] += "%";
                //        }
                //          else
                //            {
                //                  PostProcLines.Add(a);
                //                }
                //              }
                //                Gerber.WriteAllLines(outputfile, PolyLineSet.SanitizeInputLines(PostProcLines));
            }
            catch (Exception E)
            {
                log.AddString(String.Format(E.Message));
            }
            log.PopActivity();


            return overlapList;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Warning! This is a very dangerous experimental piece of software! Always manually check your generated gerber afterwards!!!!!!");
            if (args.Length < 3)
            {
                Console.WriteLine("GerberSubtract <sourcefile> <subtractfile> <outputfile>");
                return;
            }
            string inputfile = args[0];
            string subtractfile = args[1];
            string outputfile = args[2];

            Subtract(new StandardConsoleLog(), inputfile, subtractfile, outputfile);
        }
    }
}
