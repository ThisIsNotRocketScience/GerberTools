using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberLibrary
{
    public class ExcellonTool
    {
        public int ID;
        public double Radius;
        public List<PointD> Drills = new List<PointD>();

        public class SlotInfo
        {   
            public PointD Start = new PointD();
            public PointD End = new PointD();
        }
        public List<SlotInfo> Slots = new List<SlotInfo>();
    };

    public class ExcellonFile
    {
        public void Load(string filename, double drillscaler = 1.0)
        {
            var lines = File.ReadAllLines(filename);
            ParseExcellon(lines.ToList(), drillscaler);
        }

        public void Load(StreamReader stream, double drillscaler = 1.0)
        {
            List<string> lines = new List<string>();
            while (!stream.EndOfStream)
            {
                lines.Add(stream.ReadLine());
            }
            ParseExcellon(lines, drillscaler);
        }

        public static void MergeAll(List<string> Files, string output, ProgressLog Log)
        {
            if (Files.Count >= 2)
            {
                MultiMerge(Files[0], Files.Skip(1).ToList(), output, Log);
                return;

            }
            if (Files.Count < 2)
            {
                if (Files.Count == 1)
                {
                    Console.WriteLine("Merging 1 file is copying... doing so...");
                    if (File.Exists(output)) File.Delete(output);
                    File.Copy(Files[0], output);
                }
                else
                {
                    Console.WriteLine("Need files to do anything??");
                }
                return;
            }

            string LastFile = Files[0];
            List<string> TempFiles = new List<string>();
            for (int i = 1; i < Files.Count - 1; i++)
            {
                string NewFile = Path.GetTempFileName();
                TempFiles.Add(NewFile);
                Merge(LastFile, Files[i], NewFile, Log);
                LastFile = NewFile;
            }

            Merge(LastFile, Files.Last(), output, Log);
            Log.AddString("Removing merge tempfiles");

            foreach (string s in TempFiles)
            {
                File.Delete(s);
            }
        }

        private static void MultiMerge(string file1, List<string> otherfiles, string output, ProgressLog Log)
        {
            if (File.Exists(file1) == false)
            {
                Console.WriteLine("{0} not found! stopping process!", file1);
                return;
            }
            foreach (var otherfile in otherfiles)
            {
                if (File.Exists(otherfile) == false)
                {
                    Console.WriteLine("{0} not found! stopping process!", otherfile);
                    return;
                }
            }

            Console.WriteLine("*** Reading {0}:", file1);
            ExcellonFile File1Parsed = new ExcellonFile();
            File1Parsed.Load(file1);
            List<ExcellonFile> OtherFilesParsed = new List<ExcellonFile>();
            foreach (var otherfile in otherfiles)
            {

                Console.WriteLine("*** Reading {0}:", otherfile);
                ExcellonFile OtherFileParsed = new ExcellonFile();
                OtherFileParsed.Load(otherfile);
                OtherFilesParsed.Add(OtherFileParsed);
            }
            int MaxID = 0;
            foreach (var D in File1Parsed.Tools)
            {
                if (D.Value.ID > MaxID) MaxID = D.Value.ID + 1;
            }
            foreach (var F in OtherFilesParsed)
            {
                foreach (var D in F.Tools)
                {
                    File1Parsed.AddToolWithHoles(D.Value); ;
                    //                D.Value.ID += MaxID;
                    //              File1Parsed.Tools[D.Value.ID] = D.Value;
                }
            }
            File1Parsed.Write(output, 0, 0, 0, 0);
        }

        private void AddToolWithHoles(ExcellonTool d)
        {
            ExcellonTool T = FindMatchingTool(d);
            foreach(var a in d.Drills)
            {
                T.Drills.Add(new PointD(a.X, a.Y));
            }
            foreach(var s in d.Slots)
            {
                T.Slots.Add(new ExcellonTool.SlotInfo() { Start = new PointD(s.Start.X, s.Start.Y), End = new PointD(s.End.X, s.End.Y) });
            }
            
        }

        private ExcellonTool FindMatchingTool(ExcellonTool d)
        {
            int freeid = 10;
            foreach(var t in Tools)
            {
                if (d.Radius == t.Value.Radius) return t.Value;
                if (t.Key >= freeid) freeid = t.Key + 1;
            }
            var T = new ExcellonTool() { Radius = d.Radius , ID = freeid};
            
            Tools[T.ID] = T;

            return T;
        }

        public static void Merge(string file1, string file2, string outputfile, ProgressLog Log)
        {
            if (File.Exists(file1) == false)
            {
                Console.WriteLine("{0} not found! stopping process!", file1);
                return;
            }
            if (File.Exists(file2) == false)
            {
                Console.WriteLine("{0} not found! stopping process!", file2);
                return;
            }
            Log.AddString(String.Format("*** Merging {0} with {1}", file1, file2));

            Console.WriteLine("*** Reading {0}:", file1);
            ExcellonFile File1Parsed = new ExcellonFile();
            File1Parsed.Load(file1);
            Console.WriteLine("*** Reading {0}:", file2);
            ExcellonFile File2Parsed = new ExcellonFile();
            File2Parsed.Load(file2);

            int MaxID = 0;
            foreach (var D in File1Parsed.Tools)
            {
                if (D.Value.ID > MaxID) MaxID = D.Value.ID + 1;
            }

            foreach (var D in File2Parsed.Tools)
            {
                D.Value.ID += MaxID;
                File1Parsed.Tools[D.Value.ID] = D.Value;
            }

            File1Parsed.Write(outputfile, 0, 0, 0, 0);
        }

        public void Write(string filename, double DX, double DY, double DXp, double DYp, double AngleInDeg = 0)
        {
            double Angle = AngleInDeg * (Math.PI * 2.0) / 360.0;
            double CA = Math.Cos(Angle);
            double SA = Math.Sin(Angle);

            List<string> lines = new List<string>();
            lines.Add("%");
            lines.Add("M48");
            lines.Add("METRIC,000.000");
            //lines.Add("M71");
            foreach (var a in Tools)
            {
                lines.Add(String.Format("T{0}C{1}", a.Key.ToString("D2"), (a.Value.Radius * 2).ToString("N2").Replace(',', '.')));
            }
            lines.Add("%");
            GerberNumberFormat GNF = new GerberNumberFormat();
            GNF.SetMetricMode();
            GNF.OmitLeading = true;
            GNF.DigitsAfter = 3;
            GNF.DigitsBefore = 3;
            foreach (var a in Tools)
            {
                lines.Add(String.Format("T{0}", a.Key.ToString("D2")));
                double coordmultiplier = 1;

                foreach (var d in a.Value.Drills)
                {

                    double X = (d.X * coordmultiplier + DXp) / coordmultiplier;
                    double Y = (d.Y * coordmultiplier + DYp) / coordmultiplier;
                    if (Angle != 0)
                    {
                        double nX = X * CA - Y * SA;
                        double nY = X * SA + Y * CA;
                        X = nX;
                        Y = nY;
                    }
                    X = (X * coordmultiplier + DX) / coordmultiplier;
                    Y = (Y * coordmultiplier + DY) / coordmultiplier;

                    lines.Add(string.Format("X{0}Y{1}", GNF.Format(X), GNF.Format(Y).Replace(',', '.')));
                }

                foreach(var s in a.Value.Slots)
                {
                    double XS = (s.Start.X * coordmultiplier + DXp) / coordmultiplier;
                    double YS = (s.Start.Y * coordmultiplier + DYp) / coordmultiplier;
                    double XE = (s.End.X * coordmultiplier + DXp) / coordmultiplier;
                    double YE = (s.End.Y * coordmultiplier + DYp) / coordmultiplier;
                    if (Angle != 0)
                    {
                        double nX = XS * CA - YS * SA;
                        double nY = XS * SA + YS * CA;
                        XS = nX;
                        YS = nY;

                        double neX = XE * CA - YE * SA;
                        double neY = XE * SA + YE * CA;
                        XE = neX;
                        YE = neY;
                 
                    }
                    XS = (XS * coordmultiplier + DX) / coordmultiplier;
                    YS = (YS * coordmultiplier + DY) / coordmultiplier;
                    XE = (XE * coordmultiplier + DX) / coordmultiplier;
                    YE = (YE * coordmultiplier + DY) / coordmultiplier;

                    lines.Add(string.Format("X{0}Y{1}G85X{2}Y{3}", GNF.Format(XS), GNF.Format(YS).Replace(',', '.'),GNF.Format(XE), GNF.Format(YE).Replace(',', '.')));

                }


            }
            lines.Add("M30");
            Gerber.WriteAllLines(filename, lines);
        }
        public Dictionary<int, ExcellonTool> Tools = new Dictionary<int, ExcellonTool>();


        public int TotalDrillCount()
        {
            int T = 0;
            foreach(var Tool in Tools)
            {
                T += Tool.Value.Drills.Count;
            }
            return T;
        }

        bool ParseExcellon(List<string> lines, double drillscaler )
        {
            Tools.Clear();
            bool headerdone = false;
            int currentline = 0;
            ExcellonTool CurrentTool = null;
            GerberNumberFormat GNF = new GerberNumberFormat();
            GNF.DigitsBefore = 3;
            GNF.DigitsAfter = 3;
            GNF.OmitLeading = true;
            double Scaler = 1.0f;
            bool FormatSpecified = false;
            bool NumberSpecHad = false;
            double LastX = 0;
            double LastY = 0;
            while (currentline < lines.Count)
            {
                switch(lines[currentline])
                {
                    //  case "M70":  GNF.Multiplier = 25.4; break; // inch mode
                    case "INCH":
                        if (Gerber.ExtremelyVerbose) Console.WriteLine("Out of header INCH found!");
                        GNF.SetImperialMode();

                        break; // inch mode
                    case "METRIC":
                        if (Gerber.ExtremelyVerbose) Console.WriteLine("Out of header METRIC found!");

                        GNF.SetMetricMode();
                        break;
                    case "M72":
                        if (Gerber.ExtremelyVerbose) Console.WriteLine("Out of header M72 found!");
                        GNF.SetImperialMode();
                        break; // inch mode
                    case "M71":
                        if (Gerber.ExtremelyVerbose) Console.WriteLine("Out of header M71 found!");
                        GNF.SetMetricMode();
                        break; // metric mode

                }
                if (lines[currentline] == "M48")
                {
                    //Console.WriteLine("Excellon header starts at line {0}", currentline);
                    currentline++;
                    while ((lines[currentline] != "%" && lines[currentline] != "M95"))
                    {
                        headerdone = true;
                        //double InchMult = 1;// 0.010;
                        switch (lines[currentline])
                        {
                            //  case "M70":  GNF.Multiplier = 25.4; break; // inch mode
                            case "INCH":
                                GNF.SetImperialMode();

                                //Scaler = 0.01;
                                break; // inch mode
                            case "METRIC":
                                GNF.SetMetricMode();
                                break;
                            case "M72":
                                //GNF.Multiplier = 25.4 * InchMult;
                                GNF.SetImperialMode();
                                //  Scaler = 0.01;
                                break; // inch mode
                            case "M71":
                                //GNF.Multiplier = 1.0;
                                GNF.SetMetricMode();
                                break; // metric mode

                            default:
                                {
                                    var S = lines[currentline].Split(',');
                                    if (S[0].IndexOf("INCH") == 0 || S[0].IndexOf("METRIC") == 0)
                                    {
                                        if (S[0].IndexOf("INCH") ==0)
                                        {
                                            GNF.SetImperialMode();
                                        }
                                        else
                                        {
                                            GNF.SetMetricMode();

                                        }
                                        if (S.Count() > 1)
                                        {
                                            for (int i = 1; i < S.Count(); i++)
                                            {if (S[i][0] == '0')
                                            {
                                                Console.WriteLine("Number spec reading!: {0}", S[i]);
                                                var A = S[i].Split('.');
                                                if (A.Length == 2)
                                                {
                                                    GNF.DigitsBefore = A[0].Length;
                                                    GNF.DigitsAfter = A[1].Length;
                                                    NumberSpecHad = true;
                                                }
                                            }
                                                if (S[i] == "LZ")
                                                {
                                                    GNF.OmitLeading = false;
                                                }
                                                if (S[i] == "TZ")
                                                {
                                                    GNF.OmitLeading = true;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (lines[currentline][0] == ';')
                                        {
                                            if (Gerber.ShowProgress) Console.WriteLine(lines[currentline]);

                                            if (lines[currentline].Contains(";FILE_FORMAT="))
                                            {
                                                var N = lines[currentline].Substring(13).Split(':');
                                                GNF.DigitsBefore = int.Parse(N[0]);
                                                GNF.DigitsAfter = int.Parse(N[1]);
                                                FormatSpecified = true;
                                            }
                                        }
                                        else
                                        {
                                            GCodeCommand GCC = new GCodeCommand();
                                            GCC.Decode(lines[currentline], GNF);
                                            if (GCC.charcommands.Count > 0)
                                                switch (GCC.charcommands[0])
                                                {
                                                    case 'T':
                                                        {
                                                            ExcellonTool ET = new ExcellonTool();


                                                            ET.ID = (int)GCC.numbercommands[0];

                                                            ET.Radius = GNF.ScaleFileToMM(GCC.GetNumber('C')) / 2.0f;
                                                            Tools[ET.ID] = ET;
                                                        }
                                                        break;
                                                }

                                        }
                                    }
                                }
                                break;
                        }
                        currentline++;
                    }
                    //           Console.WriteLine("Excellon header stops at line {0}", currentline);
                    if (FormatSpecified == false && NumberSpecHad == false)
                    {
                        if (GNF.CurrentNumberScale == GerberNumberFormat.NumberScale.Imperial)
                        {
                          //  GNF.OmitLeading = true;
                            GNF.DigitsBefore = 2;
                            GNF.DigitsAfter = 4;
                        }
                        else
                        {
                            GNF.DigitsAfter = 3;
                            GNF.DigitsBefore = 3;
                        }
                    }
                }
                else
                {
                    if (headerdone)
                    {
                        GCodeCommand GCC = new GCodeCommand();
                        GCC.Decode(lines[currentline], GNF);
                        if (GCC.charcommands.Count > 0)
                        {
                            switch (GCC.charcommands[0])
                            {
                                case 'T':
                                    if ((int)GCC.numbercommands[0] > 0)
                                    {
                                        CurrentTool = Tools[(int)GCC.numbercommands[0]];
                                    }
                                    else
                                    {
                                        CurrentTool = null;
                                    }
                                    break;
                                case 'M':

                                default:
                                    {                                    
                                        GerberSplitter GS = new GerberSplitter();
                                        GS.Split(GCC.originalline, GNF, true);
                                        if (GS.Has("G") && GS.Get("G") == 85 && (GS.Has("X") || GS.Has("Y")))
                                        {
                                            GerberListSplitter GLS = new GerberListSplitter();
                                            GLS.Split(GCC.originalline, GNF, true);

                                            double x1 = LastX;
                                            double y1 = LastY;
                                            
                                            if (GLS.HasBefore("G", "X")) {x1 = GNF.ScaleFileToMM(GLS.GetBefore("G", "X") * Scaler);LastX = x1;}
                                            if (GLS.HasBefore("G", "Y")) {y1 = GNF.ScaleFileToMM(GLS.GetBefore("G", "Y") * Scaler); LastY = y1; }
                                            
                                            
                                            double x2 = LastX;
                                            double y2 = LastY;

                                            if (GLS.HasAfter("G", "X")) { x2 = GNF.ScaleFileToMM(GLS.GetAfter("G", "X") * Scaler); LastX = x2; }
                                            if (GLS.HasAfter("G", "Y")) { y2 = GNF.ScaleFileToMM(GLS.GetAfter("G", "Y") * Scaler); LastY = y2; }

                                            CurrentTool.Slots.Add(new ExcellonTool.SlotInfo() { Start = new PointD(x1 * drillscaler, y1 * drillscaler), End = new PointD(x2 * drillscaler, y2 * drillscaler) });

                                            LastX = x2;
                                            LastY = y2;
                                        }
                                        else if (GS.Has("G") && GS.Get("G") == 00 && (GS.Has("X") || GS.Has("Y")))
                                        {
                                            GerberListSplitter GLS = new GerberListSplitter();
                                            GLS.Split(GCC.originalline, GNF, true);

                                            double x1 = LastX;
                                            double y1 = LastY;

                                            if (GLS.HasAfter("G", "X")) { x1 = GNF.ScaleFileToMM(GLS.GetAfter("G", "X") * Scaler); LastX = x1; }
                                            if (GLS.HasAfter("G", "Y")) { y1 = GNF.ScaleFileToMM(GLS.GetAfter("G", "Y") * Scaler); LastY = y1; }

                                        }
                                        else if (GS.Has("G") && GS.Get("G") == 01 && (GS.Has("X") || GS.Has("Y")))
                                        {
                                            GerberListSplitter GLS = new GerberListSplitter();
                                            GLS.Split(GCC.originalline, GNF, true);

                                            double x1 = LastX;
                                            double y1 = LastY;
                                            double x2 = LastX;
                                            double y2 = LastY;

                                            if (GLS.HasAfter("G", "X")) { x2 = GNF.ScaleFileToMM(GLS.GetAfter("G", "X") * Scaler); LastX = x2; }
                                            if (GLS.HasAfter("G", "Y")) { y2 = GNF.ScaleFileToMM(GLS.GetAfter("G", "Y") * Scaler); LastY = y2; }

                                            CurrentTool.Slots.Add(new ExcellonTool.SlotInfo() { Start = new PointD(x1 * drillscaler, y1 * drillscaler), End = new PointD(x2 * drillscaler, y2 * drillscaler) });

                                            LastX = x2;
                                            LastY = y2;
                                        }
                                        else
                                        {
                                            if (GS.Has("X") || GS.Has("Y"))
                                            {
                                                double X = LastX;
                                                if (GS.Has("X")) X = GNF.ScaleFileToMM(GS.Get("X") * Scaler);
                                                double Y = LastY;
                                                if (GS.Has("Y")) Y = GNF.ScaleFileToMM(GS.Get("Y") * Scaler);
                                                CurrentTool.Drills.Add(new PointD(X * drillscaler, Y * drillscaler));
                                                LastX = X;
                                                LastY = Y;
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
                currentline++;
            }
            return headerdone;
        }


        public static void WriteContainedOnly(string inputfile, PolyLine Boundary, string outputfilename, ProgressLog Log)
        {
            if (File.Exists(inputfile) == false)
            {
                Console.WriteLine("{0} not found! stopping process!", Path.GetFileName(inputfile));
                return;
            }
            Log.AddString(String.Format("Clipping {0} to {1}", Path.GetFileName(inputfile), Path.GetFileName(outputfilename)));

            ExcellonFile EF = new ExcellonFile();
            EF.Load(inputfile);
            EF.WriteContained(Boundary, outputfilename, Log);

        }

        private void WriteContained(PolyLine boundary, string outputfilename, ProgressLog log)
        {
            ExcellonFile Out = new ExcellonFile();

            foreach(var T in Tools)
            {
                Out.Tools[T.Key] = new ExcellonTool() { ID = T.Value.ID, Radius = T.Value.Radius };
                foreach(var d in T.Value.Drills)
                {
                    if (boundary.PointInPoly(new PointD(d.X , d.Y)))
                      {
                        Out.Tools[T.Key].Drills.Add(d);
                    }
                }
                foreach (var d in T.Value.Slots)
                {
                    if (boundary.PointInPoly(d.Start) || boundary.PointInPoly(d.End))
                    {
                        Out.Tools[T.Key].Slots.Add(d);
                    }
                }
            }

            Out.Write(outputfilename, 0, 0, 0, 0);
        }
    }
}
