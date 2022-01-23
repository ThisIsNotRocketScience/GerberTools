﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using GerberLibrary.Core.Primitives;
using GerberLibrary.Core;

namespace GerberLibrary
{
    public static class GerberMerger
    {
        public static void MergeAll(List<string> Files, string output, ProgressLog Log)
        {
            int Check = Log.PushActivity("Gerber MergeAll");
            if (Files.Count > 1)
            {
                MultiMerge(Files[0], Files.Skip(1).ToList(), output, Log);
                Log.PopActivity(Check);
                return;
            }
            if (Files.Count < 2)
            {
                if (Files.Count == 1)
                {
                    Log.AddString("Merging 1 file is copying... doing so...");
                    if (File.Exists(output)) File.Delete(output);
                    File.Copy(Files[0], output);
                }
                else
                {
                    Log.AddString("Need files to do anything??");
                }
                Log.PopActivity(Check);
                return;
            }


            List<string> TempFiles = new List<string>();
            if (true)
            {

                List<String> LeftOverFiles = new List<string>();
                foreach (var a in Files)
                {
                    LeftOverFiles.Add(a);
                }

                while (LeftOverFiles.Count > 1)
                {
                    Log.AddString(String.Format("{0} files left in mergequeue", LeftOverFiles.Count));
                    string File1 = LeftOverFiles[0];
                    string File2 = LeftOverFiles[1];
                    LeftOverFiles.Remove(File1);
                    LeftOverFiles.Remove(File2);
                    string NewFile = Path.GetTempFileName();

                    Merge(File1, File2, NewFile, Log);
                    TempFiles.Add(NewFile);
                    LeftOverFiles.Add(NewFile);
                }

                if (File.Exists(output)) File.Delete(output);
                File.Move(LeftOverFiles[0], output);
            }
            else
            {

                string LastFile = Files[0];
                for (int i = 1; i < Files.Count - 1; i++)
                {
                    string NewFile = Path.GetTempFileName();
                    TempFiles.Add(NewFile);
                    Merge(LastFile, Files[i], NewFile, Log);
                    LastFile = NewFile;
                }
                if (File.Exists(output)) File.Delete(output);
                Merge(LastFile, Files.Last(), output, Log);
            }
            Log.AddString("Removing merge tempfiles");
            foreach (string s in TempFiles)
            {
                try
                {
                    File.Delete(s);
                }
                catch (Exception) { }
            }
            Log.PopActivity(Check);
        }

        public static void MultiMerge(string file1, List<string> filestomergein, string output, ProgressLog Log)
        {
            int check = Log.PushActivity("Gerber MultiMerge");
            if (File.Exists(file1) == false)
            {
                Log.AddString(String.Format("{0} not found! stopping process!", Path.GetFileName(file1)));
                Log.PopActivity(check);
                return;
            }
            List<ParsedGerber> OtherFiles = new List<ParsedGerber>();

            int MaxDigitsBefore = 0;
            int MaxDigitsAfter = 0;
            foreach (var otherfile in filestomergein)
            {
                if (File.Exists(otherfile) == false)
                {
                    Log.AddString(String.Format("{0} not found! stopping process!", Path.GetFileName(otherfile)));
                    Log.PopActivity(check);
                    return;
                }

                List<string> OtherLines = File.ReadAllLines(otherfile).ToList();
                OtherLines = PolyLineSet.SanitizeInputLines(OtherLines);
                ParsedGerber OtherFileParsed = PolyLineSet.ParseGerber274x(Log, OtherLines, true, false, new GerberParserState() { PreCombinePolygons = false, GenerateGeometry = false });
                OtherFiles.Add(OtherFileParsed);
                OtherFileParsed.OriginalLines = OtherLines;
                MaxDigitsAfter = Math.Max(OtherFileParsed.State.CoordinateFormat.DigitsAfter, MaxDigitsAfter);
                MaxDigitsBefore = Math.Max(OtherFileParsed.State.CoordinateFormat.DigitsBefore, MaxDigitsBefore);

            }
            List<string> File1Lines = File.ReadAllLines(file1).ToList();

            File1Lines = PolyLineSet.SanitizeInputLines(File1Lines);
            ParsedGerber File1Parsed = PolyLineSet.ParseGerber274x(Log, File1Lines, true, false, new GerberParserState() { PreCombinePolygons = false, GenerateGeometry = false });

            List<string> OutputLines = new List<string>();
            GerberNumberFormat GNF = new GerberNumberFormat();
            GNF.DigitsBefore = Math.Max(File1Parsed.State.CoordinateFormat.DigitsBefore, MaxDigitsBefore);
            GNF.DigitsAfter = Math.Max(File1Parsed.State.CoordinateFormat.DigitsAfter, MaxDigitsAfter);

            GNF.SetImperialMode();

            //OutputLines.Add(String.Format("G04 merged from {0} and {1}", Path.GetFileNameWithoutExtension(file1), Path.GetFileNameWithoutExtension(file2)));
            OutputLines.Add(GNF.BuildMetricImperialFormatLine());
            OutputLines.Add("%OFA0B0*%");
            OutputLines.Add(GNF.BuildGerberFormatLine());
            OutputLines.Add("%IPPOS*%");
            OutputLines.Add("%LPD*%");

            Dictionary<string, string> MacroDict = new Dictionary<string, string>();

            foreach (var a in File1Parsed.State.ApertureMacros)
            {
                //a.Value.Name = a.Value.Name + "_FILE0";
                OutputLines.Add(a.Value.BuildGerber(GNF, 0, "_FILE0").Trim());
                MacroDict[a.Value.Name ] = a.Value.Name;
            }
            int FileN = 1;
            foreach (var fileparsed in OtherFiles)
            {

                string FileSuffix = "_FILE" + FileN.ToString();
                FileN++;
                foreach (var a in fileparsed.State.ApertureMacros)
                {
                    /*a.Value.Name = a.Value.Name + FileSuffix;

                    int off = 0;
                    string name = string.Format("{0}", a.Value.Name);
                    while (MacroDict.Values.Contains(name))
                    {
                        off++;
                        name = string.Format("{0}_{1}", a.Value.Name, off);
                    }*/
                    MacroDict[a.Value.Name] = a.Value.Name;
                    OutputLines.Add(a.Value.BuildGerber(GNF, 0, FileSuffix).Trim());
                }
            }

            CheckAllApertures(File1Parsed, File1Lines, Log);

            foreach (var a in File1Parsed.State.Apertures)
            {
                //if (a.Value.ShapeType == GerberApertureShape.Macro)
                //{
                    //a.Value.MacroName = a.Value.MacroName + "_FILE0";
                //}
                OutputLines.Add(a.Value.BuildGerber(GNF, "_FILE0"));
            }

            int ApertureOffset = 0;
            if (File1Parsed.State.Apertures.Count > 0) ApertureOffset = File1Parsed.State.Apertures.Keys.Max() + 1;


            int LastID = ApertureOffset + 10;
            FileN = 1;
            foreach (var fileparsed in OtherFiles)
            {
                CheckAllApertures(fileparsed, fileparsed.OriginalLines, Log);
                string FileSuffix = "_FILE" + FileN.ToString();
                FileN++;

                foreach (var a in fileparsed.State.Apertures)
                {

                    if (a.Value.ShapeType == GerberApertureShape.Macro )
                    {
                        a.Value.MacroName = a.Value.MacroName + FileSuffix;
                //        a.Value.MacroName = MacroDict[a.Value.MacroName];
                    }
                    a.Value.ID = LastID++;
                    OutputLines.Add(a.Value.BuildGerber(GNF, FileSuffix));
                }
            }
            // stuff goes here.
            //OutputLines.Add(String.Format("G04 :starting {0}", Path.GetFileNameWithoutExtension(file1)));
            string CurrentPolarity = "LPD";
            bool Repeating = false;
            double X = 0;
            double Y = 0;
            for (int i = 0; i < File1Lines.Count; i++)
            {
                string CurrentLine = File1Lines[i];
                if (CurrentLine.Length == 0) continue;

                try
                {
                    GCodeCommand GCC = new GCodeCommand();
                    GCC.Decode(CurrentLine, GNF);

                    switch (CurrentLine[0])
                    {
                        case '%':
                            {
                                string FinalLine = "" + CurrentLine;
                                FinalLine = FinalLine.Replace("%", "").Replace("*", "").Trim();
                                switch (FinalLine)
                                {
                                    case "LPD":
                                        if (CurrentPolarity != "LPD") { OutputLines.Add("%LPD*%"); CurrentPolarity = "LPD"; }

                                        break;
                                    case "LPC":
                                        if (CurrentPolarity != "LPC") { OutputLines.Add("%LPC*%"); CurrentPolarity = "LPC"; }
                                        break;
                                    default:
                                        if (CurrentLine.StartsWith("%SR"))
                                        {
                                            OutputLines.Add(CurrentLine);
                                           // Console.WriteLine("StepRepeatYay!", CurrentLine);
                                            if (GCC.numbercommands.Count > 0)
                                            {
                                                int x = (int)GCC.numbercommands[0];
                                                int y = (int)GCC.numbercommands[1];
                                                if (x > 1 && y > 1)
                                                {
                                                    Repeating = true;
                                                }
                                                else
                                                {
                                                    Repeating = false;
                                                }
                                            }
                                            else
                                            {
                                                Repeating = false;
                                            }
                                        }
                                        else
                                        {
                                            //Console.WriteLine("eating % commands... {0}", CurrentLine);
                                            if (CurrentLine.Length > 1 && CurrentLine[CurrentLine.Length - 1] != '%')
                                            {
                                                while (CurrentLine[CurrentLine.Length - 1] != '%')
                                                {
                                                    i++;
                                                    CurrentLine = File1Lines[i];
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                            break;
                        case 'G': // machine status commands and interpolations?
                            {
                                GerberSplitter GS = new GerberSplitter();
                                GS.Split(GCC.originalline, File1Parsed.State.CoordinateFormat);
                                if ((int)GS.Get("G") == 54)
                                {
                                    if (GS.Has("D") && GS.Get("D") > 3)
                                    {

                                        OutputLines.Add(CurrentLine);
                                    }
                                }
                                else
                                    if ((int)GCC.numbercommands[0] == 3 || (int)GCC.numbercommands[0] == 2 || (int)GCC.numbercommands[0] == 1)
                                {

                                    //GS.Split(GCC.originalline, File1Parsed.State.CoordinateFormat);
                                    GS.ScaleToMM(File1Parsed.State.CoordinateFormat);

                                    if (File1Parsed.State.CoordinateFormat.Relativemode)
                                    {
                                        if (GS.Has("X"))
                                        {
                                            X += GS.Get("X");
                                            GS.Set("X", X);
                                        }
                                        if (GS.Has("Y"))
                                        {
                                            Y += GS.Get("Y");
                                            GS.Set("Y", Y);
                                        }
                                    }

                                    GS.ScaleToFile(GNF);


                                    OutputLines.Add(GS.Rebuild(GNF));
                                    //   Log.AddString("Unsupported arc command found!");
                                    //  MessageBox.Show("Unsupported arc type found during merge! File will NOT be exported correctly!", "Error during export", MessageBoxButtons.OK, MessageBoxIcon.Error);


                                    //         Log.AddString("Unsupported arc command found!");
                                    //       MessageBox.Show("Unsupported arc type found during merge! File will NOT be exported correctly!", "Error during export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                else
                                {
                                    if (GS.Has("G") && (GS.Get("G") == 71 || GS.Get("G") == 70))
                                    {
                                        if (GS.Get("G") == 71)
                                        {
                                            File1Parsed.State.CoordinateFormat.SetMetricMode();
                                        }
                                        if (GS.Get("G") == 70)
                                        {
                                            File1Parsed.State.CoordinateFormat.SetImperialMode();
                                        }
                                        // skip changing metric mode!
                                        Log.AddString(String.Format("ignoring metric/imperial gcode: G{0}", GS.Get("G")));

                                    }
                                    else
                                    {
                                        if (GS.Get("G") == 4)
                                        {
                                            Log.AddString(String.Format("skipping comment: {0}", CurrentLine));
                                        }
                                        else
                                        {
                                            OutputLines.Add(CurrentLine);
                                        }

                                        //OutputLines.Add(CurrentLine);
                                    }
                                    //tputLines.Add(CurrentLine);
                                }

                            }
                            break;
                        default: // move commands -> transform 
                            {
                                GerberSplitter GS = new GerberSplitter();
                                if (File1Parsed.State.CoordinateFormat.Relativemode == false)
                                {
                                    X = 0;
                                    Y = 0;
                                }
                                GS.Split(GCC.originalline, File1Parsed.State.CoordinateFormat);
                                GS.ApplyMirroring(File1Parsed.State);
                                if (GS.Has("D") || GS.Has("X") || GS.Has("Y"))
                                {
                                    if (GS.Get("D") > 3)
                                    {
                                        OutputLines.Add(CurrentLine);
                                    }
                                    else
                                    {
                                        if (GS.Has("X") == false && GS.Has("Y") == false)
                                        {
                                            OutputLines.Add(CurrentLine);
                                        }
                                        else
                                        {
                                            string OutLine = ";";
                                            if (GS.Has("X") && GS.Has("Y"))
                                            {

                                                X += File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("X"));
                                                Y += File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("Y"));

                                                OutLine = String.Format("X{0}Y{1}", GNF.Format(GNF._ScaleMMToFile(X)), GNF.Format(GNF._ScaleMMToFile(Y)));
                                            }
                                            else
                                            {
                                                if (GS.Has("X"))
                                                {
                                                    X += File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("X"));
                                                    OutLine = String.Format("X{0}", GNF.Format(GNF._ScaleMMToFile(X)));
                                                }
                                                else
                                                {
                                                    if (GS.Has("Y"))
                                                    {
                                                        Y += File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("Y"));
                                                        OutLine = String.Format("Y{0}", GNF.Format(GNF._ScaleMMToFile(Y)));
                                                    }
                                                    else
                                                    {
                                                        OutLine = "";
                                                    }

                                                }
                                            }


                                            if (GS.Has("I"))
                                            {

                                                var I = File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("I"));
                                                OutLine += String.Format("I{0}", GNF.Format(GNF._ScaleMMToFile(I)));

                                            }
                                            if (GS.Has("J"))
                                            {
                                                var J = File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("J"));
                                                OutLine += String.Format("J{0}", GNF.Format(GNF._ScaleMMToFile(J)));
                                            }


                                            if (GS.Has("D"))
                                            {
                                                OutLine += String.Format("D{0}", ((int)GS.Get("D")).ToString("D2"));
                                            }
                                            OutLine += "*";
                                            OutputLines.Add(OutLine);
                                        }

                                    }

                                    //                                OutputLines.Add(String.Format("X{0}Y{1}D{2}*", GNF.Format(GNF.ScaleMMToFile(X)), GNF.Format(GNF.ScaleMMToFile(Y)), ((int)GS.Get("D")).ToString("D2")));
                                }
                            }

                            break;
                        case 'M': // skip
                            break;
                    }
                }
                catch (Exception)
                {

                }
            }
            // TODO: set polarity back to "neutral" - or should each file do this at the start anyway? 
        
            if (Repeating)
            {
                OutputLines.Add("G04 previous file was still repeating.. stopping that*");
                OutputLines.Add("%SR*%");
                Repeating = false;
            }
            if (Gerber.ShowProgress)
            {
                Log.AddString(String.Format("File 1 format: {0}", File1Parsed.State.CoordinateFormat));
            }

            foreach (var otherfile in OtherFiles)
            {
                X = 0;
                Y = 0;
                if (Gerber.ShowProgress)
                {
                    Log.AddString(String.Format("File 2 format: {0}", otherfile.State.CoordinateFormat));
                }
                OutputLines.Add("G04 next file*");
                if (CurrentPolarity != "LPD") { OutputLines.Add("%LPD*%"); CurrentPolarity = "LPD"; }

                //OutputLines.Add( String.Format("G04 :starting {0}", Path.GetFileNameWithoutExtension(file2)));
                for (int i = 0; i < otherfile.OriginalLines.Count; i++)
                {
                    string CurrentLine = otherfile.OriginalLines[i];
                    if (CurrentLine.Length == 0) continue;
                    try
                    {
                        GCodeCommand GCC = new GCodeCommand();
                        GCC.Decode(CurrentLine, GNF);
                        switch (CurrentLine[0])
                        {
                            case '%':
                                {
                                    string FinalLine = ("" + CurrentLine).Replace("%", "").Replace("*", "").Trim();
                                    switch (FinalLine)
                                    {
                                        case "LPD":
                                            if (CurrentPolarity != "LPD") { OutputLines.Add("%LPD*%"); CurrentPolarity = "LPD"; }
                                            break;
                                        case "LPC":
                                            if (CurrentPolarity != "LPC") { OutputLines.Add("%LPC*%"); CurrentPolarity = "LPC"; }
                                            break;

                                        default:
                                            if (CurrentLine.StartsWith("%SR"))
                                            {
                                                OutputLines.Add(CurrentLine);
                                                //Console.WriteLine("StepRepeatYay!", CurrentLine);
                                                if (GCC.numbercommands.Count > 0)
                                                {
                                                    int x = (int)GCC.numbercommands[0];
                                                    int y = (int)GCC.numbercommands[1];
                                                    if (x > 1 && y > 1)
                                                    {
                                                        Repeating = true;
                                                    }
                                                    else
                                                    {
                                                        Repeating = false;
                                                    }
                                                }
                                                else
                                                {
                                                    Repeating = false;
                                                }
                                            }
                                            else
                                            {
                                                //Console.WriteLine("Eating gerber % commands here too! {0}", FinalLine);
                                                if (CurrentLine.Length > 1 && CurrentLine[CurrentLine.Length - 1] != '%')
                                                {
                                                    while (CurrentLine[CurrentLine.Length - 1] != '%')
                                                    {
                                                        i++;
                                                        CurrentLine = otherfile.OriginalLines[i];
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                    break;
                                }
                            case 'G': // machine status commands and interpolations?
                                {
                                    GerberSplitter GS = new GerberSplitter();
                                    GS.Split(GCC.originalline, otherfile.State.CoordinateFormat);
                                    GS.ApplyMirroring(otherfile.State);

                                    if ((int)GS.Get("G") == 54)
                                    {
                                        if (GS.Has("D") && GS.Get("D") > 3)
                                        {
                                            OutputLines.Add(otherfile.State.Apertures[(int)GS.Get("D")].BuildSelectGerber());
                                            //OutputLines.Add(CurrentLine);
                                        }
                                    }
                                    else

                                        switch ((int)GS.Get("G"))
                                        {
                                            case 1:
                                            case 2:
                                            case 3:
                                                {
                                                    //   double X = 0;
                                                    //     double Y = 0;
                                                    //GS.Split(GCC.originalline, otherfile.State.CoordinateFormat);
                                                    GS.ScaleToMM(otherfile.State.CoordinateFormat);
                                                    if (otherfile.State.CoordinateFormat.Relativemode)
                                                    {
                                                        if (GS.Has("X"))
                                                        {
                                                            X += GS.Get("X");
                                                            GS.Set("X", X);
                                                        }
                                                        if (GS.Has("Y"))
                                                        {
                                                            Y += GS.Get("Y");
                                                            GS.Set("Y", Y);
                                                        }
                                                    }
                                                    GS.ScaleToFile(GNF);
                                                    //         if (GS.Has("X")) GS.Set("X", File2Parsed.CoordinateFormat.ScaleFileToMM(GS.Get("X")));
                                                    //        if (GS.Has("Y")) GS.Set("Y", File2Parsed.CoordinateFormat.ScaleFileToMM(GS.Get("Y")));
                                                    //       if (GS.Has("I")) GS.Set("I", File2Parsed.CoordinateFormat.ScaleFileToMM(GS.Get("I")));
                                                    //      if (GS.Has("J")) GS.Set("J", File2Parsed.CoordinateFormat.ScaleFileToMM(GS.Get("J")));
                                                    OutputLines.Add(GS.Rebuild(GNF));
                                                    //   Log.AddString("Unsupported arc command found!");
                                                    //  MessageBox.Show("Unsupported arc type found during merge! File will NOT be exported correctly!", "Error during export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                                break;
                                            default:



                                                switch ((int)GS.Get("G"))
                                                {
                                                    case 70:
                                                        OutputLines.Add("G04 skipping 70*");
                                                        otherfile.State.CoordinateFormat.SetImperialMode();

                                                        break;
                                                    case 71:
                                                        OutputLines.Add("G04 skipping 71*");
                                                        otherfile.State.CoordinateFormat.SetMetricMode();

                                                        break;
                                                    default:
                                                        OutputLines.Add(CurrentLine);
                                                        break;
                                                }
                                                break;

                                        }
                                }
                                break;
                            default: // move commands -> transform 
                                {
                                    GerberSplitter GS = new GerberSplitter();
                                    if (otherfile.State.CoordinateFormat.Relativemode == false)
                                    {
                                        X = 0;
                                        Y = 0;
                                    }
                                    GS.Split(GCC.originalline, otherfile.State.CoordinateFormat);
                                    GS.ApplyMirroring(otherfile.State);

                                    if (GS.Has("D") || GS.Has("X") || GS.Has("Y"))
                                    {
                                        if (GS.Get("D") > 3)
                                        {
                                            OutputLines.Add(otherfile.State.Apertures[(int)GCC.numbercommands[0]].BuildSelectGerber());
                                        }
                                        else
                                        {
                                            if (GS.Has("X") == false && GS.Has("Y") == false)
                                            {
                                                OutputLines.Add(CurrentLine);
                                            }
                                            else
                                            {
                                                string OutLine = ";";
                                                if (GS.Has("X") && GS.Has("Y"))
                                                {

                                                    X += otherfile.State.CoordinateFormat.ScaleFileToMM(GS.Get("X"));
                                                    Y += otherfile.State.CoordinateFormat.ScaleFileToMM(GS.Get("Y"));

                                                    OutLine = String.Format("X{0}Y{1}", GNF.Format(GNF._ScaleMMToFile(X)), GNF.Format(GNF._ScaleMMToFile(Y)));
                                                }
                                                else
                                                {
                                                    if (GS.Has("X"))
                                                    {
                                                        X += otherfile.State.CoordinateFormat.ScaleFileToMM(GS.Get("X"));
                                                        OutLine = String.Format("X{0}", GNF.Format(GNF._ScaleMMToFile(X)));
                                                    }
                                                    else
                                                    {
                                                        if (GS.Has("Y"))
                                                        {
                                                            Y += otherfile.State.CoordinateFormat.ScaleFileToMM(GS.Get("Y"));
                                                            OutLine = String.Format("Y{0}", GNF.Format(GNF._ScaleMMToFile(Y)));
                                                        }
                                                        else
                                                        {
                                                            OutLine = "";
                                                        }

                                                    }
                                                }


                                                if (GS.Has("I"))
                                                {

                                                    var I = otherfile.State.CoordinateFormat.ScaleFileToMM(GS.Get("I"));
                                                    OutLine += String.Format("I{0}", GNF.Format(GNF._ScaleMMToFile(I)));

                                                }
                                                if (GS.Has("J"))
                                                {
                                                    var J = otherfile.State.CoordinateFormat.ScaleFileToMM(GS.Get("J"));
                                                    OutLine += String.Format("J{0}", GNF.Format(GNF._ScaleMMToFile(J)));
                                                }


                                                if (GS.Has("D"))
                                                {
                                                    OutLine += String.Format("D{0}", ((int)GS.Get("D")).ToString("D2"));
                                                }
                                                OutLine += "*";
                                                OutputLines.Add(OutLine);


                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (GS.Has("G") && (GS.Get("G") == 71 || GS.Get("G") == 70))
                                        {
                                            if (GS.Get("G") == 71)
                                            {
                                                otherfile.State.CoordinateFormat.SetMetricMode();
                                            }
                                            if (GS.Get("G") == 70)
                                            {
                                                otherfile.State.CoordinateFormat.SetImperialMode();
                                            }

                                            Log.AddString(String.Format("ignoring metric/imperial gcode: G{0}", GS.Get("G")));
                                            // skip changing metric mode!
                                        }
                                        else
                                        {
                                            if (GS.Get("G") == 4)
                                            {
                                                Log.AddString(String.Format("skipping comment: {0}", CurrentLine));
                                            }
                                            else
                                            {
                                                OutputLines.Add(CurrentLine);
                                            }

                                        }
                                    }
                                }
                                break;
                            case 'M': // skip
                                break;
                        }
                    }
                    catch (Exception) { }
                }

                if (Repeating)
                {
                    OutputLines.Add("G04 previous file was still repeating.. stopping that*");
                    OutputLines.Add("%SR*%");
                    Repeating = false;
                }

            }


            OutputLines.Add(Gerber.EOF);
            Gerber.WriteAllLines(output, PolyLineSet.SanitizeInputLines(OutputLines));

            Log.PopActivity(check);


        }

        internal static void MergeAllByFileType(List<string> allFiles, string targetfolder, string combinedfilename, ProgressLog Logger)
        {
            Dictionary<string, List<string>> FilesPerExt = new Dictionary<string, List<string>>();
            Dictionary<string, BoardFileType> FileTypePerExt = new Dictionary<string, BoardFileType>();
            foreach (var s in allFiles)
            {
                string ext = Path.GetExtension(s).ToLower(); ;
                if (ext == "xln") ext = "txt";
                if (ext == "drl") ext = "txt";
                BoardLayer layer;
                BoardSide Side;

                Gerber.DetermineBoardSideAndLayer(s, out Side, out layer);
                ext = String.Format(".{0}_{1}", layer, Side);

                if (FilesPerExt.ContainsKey(ext) == false)
                {
                    FilesPerExt[ext] = new List<string>();
                }

                FileTypePerExt[ext] = Gerber.FindFileType(s);
                FilesPerExt[ext].Add(s);
            }
            int count = 0;
            foreach (var a in FilesPerExt)
            {
                count++;
                Logger.AddString("merging *" + a.Key.ToLower(), ((float)count / (float)FilesPerExt.Keys.Count) * 0.5f + 0.3f);
                switch (FileTypePerExt[a.Key])
                {
                    case BoardFileType.Drill:
                        {
                            string Filename = Path.Combine(targetfolder, combinedfilename + a.Key);
                            //FinalFiles.Add(Filename);
                            ExcellonFile.MergeAll(a.Value, Filename, Logger);
                        }
                        break;
                    case BoardFileType.Gerber:
                        {
                            if (a.Key.ToLower() != ".gko")
                            {
                                string Filename = Path.Combine(targetfolder, combinedfilename + a.Key);
                                //    FinalFiles.Add(Filename);
                                GerberMerger.MergeAll(a.Value, Filename, Logger);
                            }
                        }
                        break;
                }
            }

        }

        private static void CheckAllApertures(ParsedGerber file1Parsed, List<string> File1Lines, ProgressLog Log)
        {

            for (int i = 0; i < File1Lines.Count; i++)
            {
                string CurrentLine = File1Lines[i];
                if (CurrentLine.Length == 0) continue;

                try
                {
                    GCodeCommand GCC = new GCodeCommand();
                    GCC.Decode(CurrentLine, file1Parsed.State.CoordinateFormat);

                    switch (CurrentLine[0])
                    {
                        case 'G': // machine status commands and interpolations?
                            {
                                GerberSplitter GS = new GerberSplitter();
                                GS.Split(GCC.originalline, file1Parsed.State.CoordinateFormat);
                                if ((int)GS.Get("G") == 54)
                                {
                                    if (GS.Has("D") && GS.Get("D") > 3)
                                    {
                                        int ApertureID = (int)GS.Get("D");
                                        if (file1Parsed.State.Apertures.ContainsKey(ApertureID) == false)
                                        {
                                            Log.AddString(String.Format("ERROR: Undefined aperture D{1} found in line {0}", i, ApertureID));
                                            var GAT = new GerberApertureType() { ID = ApertureID }; ;
                                            GAT.SetCircle(0);
                                            file1Parsed.State.Apertures[ApertureID] = GAT;
                                        }

                                    }

                                }
                            }
                            break;
                        case 'D':
                            {
                                GerberSplitter GS = new GerberSplitter();
                                GS.Split(GCC.originalline, file1Parsed.State.CoordinateFormat);
                                if (GS.Has("D") && GS.Get("D") > 3)
                                {
                                    int ApertureID = (int)GS.Get("D");
                                    if (file1Parsed.State.Apertures.ContainsKey(ApertureID) == false)
                                    {
                                        var GAT = new GerberApertureType() { ID = ApertureID }; ;
                                        GAT.SetCircle(0);
                                        file1Parsed.State.Apertures[ApertureID] = GAT;
                                    }
                                }
                            }
                            break;
                    }
                }
                catch (Exception) { }
            }
        }

        public static void Merge(string file1, string file2, string output, ProgressLog Log)
        {
            Log.PushActivity("Gerber Merge");
            if (File.Exists(file1) == false)
            {
                Log.AddString(String.Format("{0} not found! stopping process!", Path.GetFileName(file1)));
                Log.PopActivity();
                return;
            }
            if (File.Exists(file2) == false)
            {
                Log.AddString(String.Format("{0} not found! stopping process!", Path.GetFileName(file2)));
                Log.PopActivity();
                return;
            }

            Log.AddString(String.Format("Merging {0} with {1} in to {2}", Path.GetFileName(file1), Path.GetFileName(file2), Path.GetFileName(output)));

            List<string> File1Lines = File.ReadAllLines(file1).ToList();
            File1Lines = PolyLineSet.SanitizeInputLines(File1Lines);
            ParsedGerber File1Parsed = PolyLineSet.ParseGerber274x(Log, File1Lines, true, false, new GerberParserState() { PreCombinePolygons = false, GenerateGeometry = false });

            List<string> File2Lines = File.ReadAllLines(file2).ToList();
            File2Lines = PolyLineSet.SanitizeInputLines(File2Lines);
            ParsedGerber File2Parsed = PolyLineSet.ParseGerber274x(Log, File2Lines, true, false, new GerberParserState() { PreCombinePolygons = false, GenerateGeometry = false });

            CheckAllApertures(File1Parsed, File1Lines, Log);
            CheckAllApertures(File2Parsed, File2Lines, Log);

            int ApertureOffset = 0;
            if (File1Parsed.State.Apertures.Count > 0) ApertureOffset = File1Parsed.State.Apertures.Keys.Max() + 1;

            List<string> OutputLines = new List<string>();
            GerberNumberFormat GNF = new GerberNumberFormat();
            GNF.DigitsBefore = Math.Max(File1Parsed.State.CoordinateFormat.DigitsBefore, File2Parsed.State.CoordinateFormat.DigitsBefore);
            GNF.DigitsAfter = Math.Max(File1Parsed.State.CoordinateFormat.DigitsAfter, File2Parsed.State.CoordinateFormat.DigitsAfter);
            if (File1Parsed.State.CoordinateFormat.CurrentNumberScale == GerberNumberFormat.NumberScale.Metric
                || File2Parsed.State.CoordinateFormat.CurrentNumberScale == GerberNumberFormat.NumberScale.Metric
                )
            {
                GNF.SetMetricMode();
            }
            else
            {
                GNF.SetImperialMode();
            }

            GNF.SetImperialMode();

            //OutputLines.Add(String.Format("G04 merged from {0} and {1}", Path.GetFileNameWithoutExtension(file1), Path.GetFileNameWithoutExtension(file2)));
            OutputLines.Add(GNF.BuildMetricImperialFormatLine());
            OutputLines.Add("%OFA0B0*%");
            OutputLines.Add(GNF.BuildGerberFormatLine());
            OutputLines.Add("%IPPOS*%");
            OutputLines.Add("%LPD*%");

            Dictionary<string, string> MacroDict = new Dictionary<string, string>();


            foreach (var a in File1Parsed.State.ApertureMacros)
            {
                OutputLines.Add(a.Value.BuildGerber(GNF, 0, "_FILE0").Trim());
                MacroDict[a.Value.Name + "_FILE0"] = a.Value.Name;
            }

            foreach (var a in File2Parsed.State.ApertureMacros)
            {
                MacroDict[a.Value.Name + "_FILE1"] = a.Value.Name;
                OutputLines.Add(a.Value.BuildGerber(GNF, 0,"_FILE1").Trim());
            }

            foreach (var a in File1Parsed.State.Apertures)
            {
                OutputLines.Add(a.Value.BuildGerber(GNF, "_FILE0"));
            }

            foreach (var a in File2Parsed.State.Apertures)
            {
                if (a.Value.ShapeType == GerberApertureShape.Macro || a.Value.ShapeType == GerberApertureShape.Compound)
                {
//                    a.Value.MacroName = MacroDict[a.Value.MacroName];
                }
                a.Value.ID += ApertureOffset;
                OutputLines.Add(a.Value.BuildGerber(GNF, "_FILE1"));
            }
            // stuff goes here.
            //OutputLines.Add(String.Format("G04 :starting {0}", Path.GetFileNameWithoutExtension(file1)));
            string CurrentPolarity = "LPD";
            bool Repeating = false;
            double X = 0;
            double Y = 0;
            for (int i = 0; i < File1Lines.Count; i++)
            {
                string CurrentLine = File1Lines[i];
                if (CurrentLine.Length == 0) continue;

                try
                {
                    GCodeCommand GCC = new GCodeCommand();
                    GCC.Decode(CurrentLine, GNF);

                    switch (CurrentLine[0])
                    {
                        case '%':
                            {
                                string FinalLine = "" + CurrentLine;
                                FinalLine = FinalLine.Replace("%", "").Replace("*", "").Trim();
                                switch (FinalLine)
                                {
                                    case "LPD":
                                        if (CurrentPolarity != CurrentLine) { OutputLines.Add("%LPD*%"); CurrentPolarity = CurrentLine; }

                                        break;
                                    case "LPC":
                                        if (CurrentPolarity != CurrentLine) { OutputLines.Add("%LPC*%"); CurrentPolarity = CurrentLine; }
                                        break;
                                    default:
                                        if (CurrentLine.StartsWith("%SR"))
                                        {
                                            OutputLines.Add(CurrentLine);
                                            //Console.WriteLine("StepRepeatYay!", CurrentLine);
                                            if (GCC.numbercommands.Count > 0)
                                            {
                                                int x = (int)GCC.numbercommands[0];
                                                int y = (int)GCC.numbercommands[1];
                                                if (x > 1 && y > 1)
                                                {
                                                    Repeating = true;
                                                }
                                                else
                                                {
                                                    Repeating = false;
                                                }
                                            }
                                            else
                                            {
                                                Repeating = false;
                                            }


                                        }
                                        else
                                        {
                                            //Console.WriteLine("eating % commands! {0}", CurrentLine);
                                            if (CurrentLine.Length > 1 && CurrentLine[CurrentLine.Length - 1] != '%')
                                            {
                                                while (CurrentLine[CurrentLine.Length - 1] != '%')
                                                {
                                                    i++;
                                                    CurrentLine = File1Lines[i];
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                            break;
                        case 'G': // machine status commands and interpolations?
                            {
                                GerberSplitter GS = new GerberSplitter();
                                GS.Split(GCC.originalline, File1Parsed.State.CoordinateFormat);
                                if ((int)GS.Get("G") == 54)
                                {
                                    if (GS.Has("D") && GS.Get("D") > 3)
                                    {
                                        OutputLines.Add(CurrentLine);
                                    }
                                }
                                else
                                    if ((int)GCC.numbercommands[0] == 3 || (int)GCC.numbercommands[0] == 2 || (int)GCC.numbercommands[0] == 1)
                                {

                                    //GS.Split(GCC.originalline, File1Parsed.State.CoordinateFormat);
                                    GS.ScaleToMM(File1Parsed.State.CoordinateFormat);
                                    GS.ScaleToFile(GNF);


                                    OutputLines.Add(GS.Rebuild(GNF));
                                    //   Log.AddString("Unsupported arc command found!");
                                    //  MessageBox.Show("Unsupported arc type found during merge! File will NOT be exported correctly!", "Error during export", MessageBoxButtons.OK, MessageBoxIcon.Error);


                                    //         Log.AddString("Unsupported arc command found!");
                                    //       MessageBox.Show("Unsupported arc type found during merge! File will NOT be exported correctly!", "Error during export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                else
                                {
                                    if (GS.Has("G") && (GS.Get("G") == 71 || GS.Get("G") == 70))
                                    {
                                        if (GS.Get("G") == 71)
                                        {
                                            File1Parsed.State.CoordinateFormat.SetMetricMode();
                                        }
                                        if (GS.Get("G") == 70)
                                        {
                                            File1Parsed.State.CoordinateFormat.SetImperialMode();
                                        }
                                        // skip changing metric mode!
                                        Log.AddString(String.Format("ignoring metric/imperial gcode: G{0}", GS.Get("G")));

                                    }
                                    else
                                    {
                                        if (GS.Get("G") == 4)
                                        {
                                            Log.AddString(String.Format("skipping comment: {0}", CurrentLine));
                                        }
                                        else
                                        {
                                            OutputLines.Add(CurrentLine);
                                        }

                                        //OutputLines.Add(CurrentLine);
                                    }
                                    //tputLines.Add(CurrentLine);
                                }

                            }
                            break;
                        default: // move commands -> transform 
                            {
                                GerberSplitter GS = new GerberSplitter();
                                if (File1Parsed.State.CoordinateFormat.Relativemode == false)
                                {
                                    X = 0;
                                    Y = 0;
                                }
                                GS.Split(GCC.originalline, File1Parsed.State.CoordinateFormat);

                                if (GS.Has("D") || GS.Has("X") || GS.Has("Y"))
                                {
                                    if (GS.Get("D") > 3)
                                    {
                                        OutputLines.Add(CurrentLine);
                                    }
                                    else
                                    {
                                        if (GS.Has("X") == false && GS.Has("Y") == false)
                                        {
                                            OutputLines.Add(CurrentLine);
                                        }
                                        else
                                        {
                                            string OutLine = ";";
                                            if (GS.Has("X") && GS.Has("Y"))
                                            {

                                                X += File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("X"));
                                                Y += File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("Y"));

                                                OutLine = String.Format("X{0}Y{1}", GNF.Format(GNF._ScaleMMToFile(X)), GNF.Format(GNF._ScaleMMToFile(Y)));
                                            }
                                            else
                                            {
                                                if (GS.Has("X"))
                                                {
                                                    X += File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("X"));
                                                    OutLine = String.Format("X{0}", GNF.Format(GNF._ScaleMMToFile(X)));
                                                }
                                                else
                                                {
                                                    if (GS.Has("Y"))
                                                    {
                                                        Y += File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("Y"));
                                                        OutLine = String.Format("Y{0}", GNF.Format(GNF._ScaleMMToFile(Y)));
                                                    }
                                                    else
                                                    {
                                                        OutLine = "";
                                                    }

                                                }
                                            }

                                            if (GS.Has("I"))
                                            {

                                                var I = File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("I"));
                                                OutLine += String.Format("I{0}", GNF.Format(GNF._ScaleMMToFile(I)));

                                            }
                                            if (GS.Has("J"))
                                            {
                                                var J = File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("J"));
                                                OutLine += String.Format("J{0}", GNF.Format(GNF._ScaleMMToFile(J)));
                                            }
                                            if (GS.Has("D"))
                                            {
                                                OutLine += String.Format("D{0}", ((int)GS.Get("D")).ToString("D2"));
                                            }
                                            OutLine += "*";
                                            OutputLines.Add(OutLine);
                                        }

                                    }

                                    //                                OutputLines.Add(String.Format("X{0}Y{1}D{2}*", GNF.Format(GNF.ScaleMMToFile(X)), GNF.Format(GNF.ScaleMMToFile(Y)), ((int)GS.Get("D")).ToString("D2")));
                                }
                            }

                            break;
                        case 'M': // skip
                            break;
                    }
                }
                catch (Exception)
                {

                }
            }
            // TODO: set polarity back to "neutral" - or should each file do this at the start anyway? 

            if (Repeating)
            {
                OutputLines.Add("G04 previous file was still repeating.. stopping that*");
                OutputLines.Add("%SR*%");
                Repeating = false;
            }

            if (Gerber.ShowProgress)
            {
                Log.AddString(String.Format("File 1 format: {0}", File1Parsed.State.CoordinateFormat));
                Log.AddString(String.Format("File 2 format: {0}", File2Parsed.State.CoordinateFormat));
            }
            OutputLines.Add("G04 next file*");

            if (CurrentPolarity != "LPD") { OutputLines.Add("%LPD*%"); CurrentPolarity = "LPD"; }
            X = 0;Y = 0;
            //OutputLines.Add( String.Format("G04 :starting {0}", Path.GetFileNameWithoutExtension(file2)));
            for (int i = 0; i < File2Lines.Count; i++)
            {
                string CurrentLine = File2Lines[i];
                if (CurrentLine.Length == 0) continue;
                try
                {
                    GCodeCommand GCC = new GCodeCommand();
                    GCC.Decode(CurrentLine, GNF);
                    switch (CurrentLine[0])
                    {
                        case '%':
                            {
                                string FinalLine = ("" + CurrentLine).Replace("%", "").Replace("*", "").Trim();
                                switch (FinalLine)
                                {
                                    case "LPD":
                                        if (CurrentPolarity != "LPD") { OutputLines.Add("%LPD*%"); CurrentPolarity = "LPD"; }

                                        break;
                                    case "LPC":
                                        if (CurrentPolarity != "LPC") { OutputLines.Add("%LPC*%"); CurrentPolarity = "LPC"; }


                                        break;

                                    default:
                                        if (CurrentLine.Length > 1 && CurrentLine[CurrentLine.Length - 1] != '%')
                                        {
                                            while (CurrentLine[CurrentLine.Length - 1] != '%')
                                            {
                                                i++;
                                                CurrentLine = File2Lines[i];
                                            }
                                        }
                                        break;
                                }
                                break;
                            }
                        case 'G': // machine status commands and interpolations?
                            {
                                GerberSplitter GS = new GerberSplitter();
                                GS.Split(GCC.originalline, File2Parsed.State.CoordinateFormat);
                                if ((int)GS.Get("G") == 54)
                                {
                                    if (GS.Has("D") && GS.Get("D") > 3)
                                    {
                                        OutputLines.Add(File2Parsed.State.Apertures[(int)GS.Get("D")].BuildSelectGerber());
                                        //OutputLines.Add(CurrentLine);
                                    }
                                }
                                else

                                    switch ((int)GS.Get("G"))
                                    {
                                        case 1:
                                        case 2:
                                        case 3:
                                            {
                                                //double X = 0;
                                                //     double Y = 0;
                                                // GS.Split(GCC.originalline, File2Parsed.State.CoordinateFormat);
                                                GS.ScaleToMM(File2Parsed.State.CoordinateFormat);
                                                GS.ScaleToFile(GNF);
                                                //         if (GS.Has("X")) GS.Set("X", File2Parsed.CoordinateFormat.ScaleFileToMM(GS.Get("X")));
                                                //        if (GS.Has("Y")) GS.Set("Y", File2Parsed.CoordinateFormat.ScaleFileToMM(GS.Get("Y")));
                                                //       if (GS.Has("I")) GS.Set("I", File2Parsed.CoordinateFormat.ScaleFileToMM(GS.Get("I")));
                                                //      if (GS.Has("J")) GS.Set("J", File2Parsed.CoordinateFormat.ScaleFileToMM(GS.Get("J")));
                                                OutputLines.Add(GS.Rebuild(GNF));
                                                //   Log.AddString("Unsupported arc command found!");
                                                //  MessageBox.Show("Unsupported arc type found during merge! File will NOT be exported correctly!", "Error during export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            }
                                            break;
                                        default:



                                            switch ((int)GS.Get("G"))
                                            {
                                                case 70:
                                                    OutputLines.Add("G04 skipping 70*");
                                                    File2Parsed.State.CoordinateFormat.SetImperialMode();

                                                    break;
                                                case 71:
                                                    OutputLines.Add("G04 skipping 71*");
                                                    File2Parsed.State.CoordinateFormat.SetMetricMode();

                                                    break;
                                                default:
                                                    OutputLines.Add(CurrentLine);
                                                    break;
                                            }
                                            break;

                                    }
                            }
                            break;
                        default: // move commands -> transform 
                            {
                                GerberSplitter GS = new GerberSplitter();
                                if (File2Parsed.State.CoordinateFormat.Relativemode == false)
                                {
                                    X = 0;
                                    Y = 0;
                                }
                                GS.Split(GCC.originalline, File2Parsed.State.CoordinateFormat);


                                if (GS.Has("D") || GS.Has("X") || GS.Has("Y"))
                                {
                                    if (GS.Get("D") > 3)
                                    {
                                        OutputLines.Add(File2Parsed.State.Apertures[(int)GCC.numbercommands[0]].BuildSelectGerber());
                                    }
                                    else
                                    {
                                        if (GS.Has("X") == false && GS.Has("Y") == false)
                                        {
                                            OutputLines.Add(CurrentLine);
                                        }
                                        else
                                        {
                                            string OutLine = ";";
                                            if (GS.Has("X") && GS.Has("Y"))
                                            {

                                                X += File2Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("X"));
                                                Y += File2Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("Y"));

                                                OutLine = String.Format("X{0}Y{1}", GNF.Format(GNF._ScaleMMToFile(X)), GNF.Format(GNF._ScaleMMToFile(Y)));
                                            }
                                            else
                                            {
                                                if (GS.Has("X"))
                                                {
                                                    X += File2Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("X"));
                                                    OutLine = String.Format("X{0}", GNF.Format(GNF._ScaleMMToFile(X)));
                                                }
                                                else
                                                {
                                                    if (GS.Has("Y"))
                                                    {
                                                        Y += File2Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("Y"));
                                                        OutLine = String.Format("Y{0}", GNF.Format(GNF._ScaleMMToFile(Y)));
                                                    }
                                                    else
                                                    {
                                                        OutLine = "";
                                                    }

                                                }
                                            }


                                            if (GS.Has("I"))
                                            {

                                                var I = File2Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("I"));
                                                OutLine += String.Format("I{0}", GNF.Format(GNF._ScaleMMToFile(I)));

                                            }
                                            if (GS.Has("J"))
                                            {
                                                var J = File2Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("J"));
                                                OutLine += String.Format("J{0}", GNF.Format(GNF._ScaleMMToFile(J)));
                                            }

                                            if (GS.Has("D"))
                                            {
                                                OutLine += String.Format("D{0}", ((int)GS.Get("D")).ToString("D2"));
                                            }
                                            OutLine += "*";
                                            OutputLines.Add(OutLine);


                                        }
                                    }
                                }
                                else
                                {
                                    if (GS.Has("G") && (GS.Get("G") == 71 || GS.Get("G") == 70))
                                    {
                                        if (GS.Get("G") == 71)
                                        {
                                            File2Parsed.State.CoordinateFormat.SetMetricMode();
                                        }
                                        if (GS.Get("G") == 70)
                                        {
                                            File2Parsed.State.CoordinateFormat.SetImperialMode();
                                        }

                                        Log.AddString(String.Format("ignoring metric/imperial gcode: G{0}", GS.Get("G")));
                                        // skip changing metric mode!
                                    }
                                    else
                                    {
                                        if (GS.Get("G") == 4)
                                        {
                                            Log.AddString(String.Format("skipping comment: {0}", CurrentLine));
                                        }
                                        else
                                        {
                                            OutputLines.Add(CurrentLine);
                                        }

                                    }
                                }
                            }
                            break;
                        case 'M': // skip
                            break;
                    }
                }
                catch (Exception) { }
            }

            if (Repeating)
            {
                OutputLines.Add("G04 previous file was still repeating.. stopping that*");
                OutputLines.Add("%SR*%");
                Repeating = false;
            }

            OutputLines.Add(Gerber.EOF);
            Gerber.WriteAllLines(output, PolyLineSet.SanitizeInputLines(OutputLines));

            Log.PopActivity();
        }


        public static void WriteContainedOnly(string inputfile, PolyLine Boundary, string outputfilename, ProgressLog Log)
        {
            Log.PushActivity("Gerber Clipper");
            if (File.Exists(inputfile) == false)
            {
                Log.AddString(String.Format("{0} not found! stopping process!", Path.GetFileName(inputfile)));
                Log.PopActivity();
                return;
            }


            Log.AddString(String.Format("Clipping {0} to {1}", Path.GetFileName(inputfile), Path.GetFileName(outputfilename)));
            List<string> File1Lines = File.ReadAllLines(inputfile).ToList();

            File1Lines = PolyLineSet.SanitizeInputLines(File1Lines);
            ParsedGerber File1Parsed = PolyLineSet.ParseGerber274x(Log, File1Lines, true, false, new GerberParserState() { PreCombinePolygons = false, GenerateGeometry = false });

            CheckAllApertures(File1Parsed, File1Lines, Log);

            int ApertureOffset = 0;
            if (File1Parsed.State.Apertures.Count > 0) ApertureOffset = File1Parsed.State.Apertures.Keys.Max() + 1;

            List<string> OutputLines = new List<string>();
            GerberNumberFormat GNF = new GerberNumberFormat();
            GNF.DigitsBefore = File1Parsed.State.CoordinateFormat.DigitsBefore;
            GNF.DigitsAfter = File1Parsed.State.CoordinateFormat.DigitsAfter;
            if (File1Parsed.State.CoordinateFormat.CurrentNumberScale == GerberNumberFormat.NumberScale.Metric)
            {
                GNF.SetMetricMode();
            }
            else
            {
                GNF.SetImperialMode();
            }

            GNF.SetImperialMode();

            //OutputLines.Add(String.Format("G04 merged from {0} and {1}", Path.GetFileNameWithoutExtension(file1), Path.GetFileNameWithoutExtension(file2)));
            OutputLines.Add(GNF.BuildMetricImperialFormatLine());
            OutputLines.Add("%OFA0B0*%");
            OutputLines.Add(GNF.BuildGerberFormatLine());
            OutputLines.Add("%IPPOS*%");
            OutputLines.Add("%LPD*%");

            Dictionary<string, string> MacroDict = new Dictionary<string, string>();

            foreach (var a in File1Parsed.State.ApertureMacros)
            {
                OutputLines.Add(a.Value.BuildGerber(GNF, 0,"_FILE0").Trim());
                MacroDict[a.Value.Name + "_FILE0"] = a.Value.Name;
            }


            foreach (var a in File1Parsed.State.Apertures)
            {
                OutputLines.Add(a.Value.BuildGerber(GNF,"_FILE0"));
            }

            // stuff goes here.
            //OutputLines.Add(String.Format("G04 :starting {0}", Path.GetFileNameWithoutExtension(file1)));
            string CurrentPolarity = "LPD";
            double X = 0;
            double Y = 0;
            for (int i = 0; i < File1Lines.Count; i++)
            {
                string CurrentLine = File1Lines[i];
                if (CurrentLine.Length == 0) continue;

                try
                {
                    GCodeCommand GCC = new GCodeCommand();
                    GCC.Decode(CurrentLine, GNF);

                    switch (CurrentLine[0])
                    {
                        case '%':
                            {
                                string FinalLine = "" + CurrentLine;
                                FinalLine = FinalLine.Replace("%", "").Replace("*", "").Trim();
                                switch (FinalLine)
                                {
                                    case "LPD":
                                        if (CurrentPolarity != CurrentLine) { OutputLines.Add("%LPD*%"); CurrentPolarity = CurrentLine; }

                                        break;
                                    case "LPC":
                                        if (CurrentPolarity != CurrentLine) { OutputLines.Add("%LPC*%"); CurrentPolarity = CurrentLine; }
                                        break;
                                    default:
                                        if (CurrentLine.Length > 1 && CurrentLine[CurrentLine.Length - 1] != '%')
                                        {
                                            while (CurrentLine[CurrentLine.Length - 1] != '%')
                                            {
                                                i++;
                                                CurrentLine = File1Lines[i];
                                            }
                                        }
                                        break;
                                }
                            }
                            break;
                        case 'G': // machine status commands and interpolations?
                            {
                                GerberSplitter GS = new GerberSplitter();
                                GS.Split(GCC.originalline, File1Parsed.State.CoordinateFormat);
                                if ((int)GS.Get("G") == 54)
                                {
                                    if (GS.Has("D") && GS.Get("D") > 3)
                                    {
                                        OutputLines.Add(CurrentLine);
                                    }
                                }
                                else
                                    if ((int)GCC.numbercommands[0] == 3 || (int)GCC.numbercommands[0] == 2 || (int)GCC.numbercommands[0] == 1)
                                {

                                    //GS.Split(GCC.originalline, File1Parsed.State.CoordinateFormat);
                                    GS.ScaleToMM(File1Parsed.State.CoordinateFormat);
                                    GS.ScaleToFile(GNF);

                                    bool skipline = false;
                                    if (GS.Has("X") && GS.Has("Y"))
                                    {

                                        X += File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("X"));
                                        Y += File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("Y"));

                                        if (Boundary.PointInPoly(new PointD(X, Y)) == false)
                                        {
                                            skipline = true;
                                        }

                                        //   OutLine = String.Format("X{0}Y{1}", GNF.Format(GNF._ScaleMMToFile(X)), GNF.Format(GNF._ScaleMMToFile(Y)));
                                    }
                                    if (!skipline) OutputLines.Add(GS.Rebuild(GNF));
                                    //   Log.AddString("Unsupported arc command found!");
                                    //  MessageBox.Show("Unsupported arc type found during merge! File will NOT be exported correctly!", "Error during export", MessageBoxButtons.OK, MessageBoxIcon.Error);


                                    //         Log.AddString("Unsupported arc command found!");
                                    //       MessageBox.Show("Unsupported arc type found during merge! File will NOT be exported correctly!", "Error during export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                else
                                {
                                    if (GS.Has("G") && (GS.Get("G") == 71 || GS.Get("G") == 70))
                                    {
                                        if (GS.Get("G") == 71)
                                        {
                                            File1Parsed.State.CoordinateFormat.SetMetricMode();
                                        }
                                        if (GS.Get("G") == 70)
                                        {
                                            File1Parsed.State.CoordinateFormat.SetImperialMode();
                                        }
                                        // skip changing metric mode!
                                        Log.AddString(String.Format("ignoring metric/imperial gcode: G{0}", GS.Get("G")));

                                    }
                                    else
                                    {
                                        if (GS.Get("G") == 4)
                                        {
                                            //Console.WriteLine("skipping comment: {0}", CurrentLine);
                                        }
                                        else
                                        {
                                            OutputLines.Add(CurrentLine);
                                        }

                                        //OutputLines.Add(CurrentLine);
                                    }
                                    //tputLines.Add(CurrentLine);
                                }

                            }
                            break;
                        default: // move commands -> transform 
                            {
                                GerberSplitter GS = new GerberSplitter();
                                if (File1Parsed.State.CoordinateFormat.Relativemode == false)
                                {
                                    X = 0;
                                    Y = 0;
                                }
                                GS.Split(GCC.originalline, File1Parsed.State.CoordinateFormat);

                                if (GS.Has("D") || GS.Has("X") || GS.Has("Y"))
                                {
                                    if (GS.Get("D") > 3)
                                    {
                                        OutputLines.Add(CurrentLine);
                                    }
                                    else
                                    {
                                        if (GS.Has("X") == false && GS.Has("Y") == false)
                                        {

                                            OutputLines.Add(CurrentLine);
                                        }
                                        else
                                        {
                                            string OutLine = ";";
                                            bool skipline = false;
                                            if (GS.Has("X") && GS.Has("Y"))
                                            {

                                                X += File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("X"));
                                                Y += File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("Y"));

                                                if (Boundary.PointInPoly(new PointD(X, Y)) == false)
                                                {
                                                    skipline = true;
                                                }

                                                OutLine = String.Format("X{0}Y{1}", GNF.Format(GNF._ScaleMMToFile(X)), GNF.Format(GNF._ScaleMMToFile(Y)));
                                            }
                                            else
                                            {
                                                if (GS.Has("X"))
                                                {
                                                    X += File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("X"));
                                                    OutLine = String.Format("X{0}", GNF.Format(GNF._ScaleMMToFile(X)));
                                                }
                                                else
                                                {
                                                    if (GS.Has("Y"))
                                                    {
                                                        Y += File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("Y"));
                                                        OutLine = String.Format("Y{0}", GNF.Format(GNF._ScaleMMToFile(Y)));
                                                    }
                                                    else
                                                    {
                                                        OutLine = "";
                                                    }

                                                }
                                            }

                                            if (GS.Has("I"))
                                            {

                                                var I = File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("I"));
                                                OutLine += String.Format("I{0}", GNF.Format(GNF._ScaleMMToFile(I)));

                                            }
                                            if (GS.Has("J"))
                                            {
                                                var J = File1Parsed.State.CoordinateFormat.ScaleFileToMM(GS.Get("J"));
                                                OutLine += String.Format("J{0}", GNF.Format(GNF._ScaleMMToFile(J)));
                                            }
                                            if (GS.Has("D"))
                                            {
                                                OutLine += String.Format("D{0}", ((int)GS.Get("D")).ToString("D2"));
                                            }
                                            OutLine += "*";
                                            if (skipline == false) OutputLines.Add(OutLine);
                                        }

                                    }

                                    //                                OutputLines.Add(String.Format("X{0}Y{1}D{2}*", GNF.Format(GNF.ScaleMMToFile(X)), GNF.Format(GNF.ScaleMMToFile(Y)), ((int)GS.Get("D")).ToString("D2")));
                                }
                            }

                            break;
                        case 'M': // skip
                            break;
                    }
                }
                catch (Exception)
                {

                }
            }
            // TODO: set polarity back to "neutral" - or should each file do this at the start anyway? 
            if (Gerber.ShowProgress)
            {
                Log.AddString(String.Format("File 1 format: {0}", File1Parsed.State.CoordinateFormat));

            }
            OutputLines.Add("G04 next file*");

            if (CurrentPolarity != "LPD") { OutputLines.Add("%LPD*%"); CurrentPolarity = "LPD"; }

            //OutputLines.Add( String.Format("G04 :starting {0}", Path.GetFileNameWithoutExtension(file2)));



            OutputLines.Add(Gerber.EOF);
            Gerber.WriteAllLines(outputfilename, PolyLineSet.SanitizeInputLines(OutputLines));

            Log.PopActivity();
        }
    }
}
