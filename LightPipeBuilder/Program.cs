using GerberLibrary;
using GerberLibrary.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightPipeBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo ci = new CultureInfo("nl-NL");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            
            foreach (var S in args)
            {
                if (File.Exists(S))
                {
                    try
                    {
                        List<string> OutputFile = new List<string>();

                        List<string> OutputFile2 = new List<string>();

                        OutputFile.Add("module pipe()");
                        OutputFile.Add("{");
                        OutputFile.Add("\ttranslate([0, 0, 2]) cylinder(h = 12,$fn = 40, d = 3);");
                        OutputFile.Add("\ttranslate([0, 0, 11]) cylinder(h = 1,$fn = 40, d = 4);");
                        OutputFile.Add("}");
                        OutputFile.Add("");

                        OutputFile.Add("module support()");
                        OutputFile.Add("{");
                        OutputFile.Add("\tcylinder(h = 13,$fn = 40, d = 2);");
                        OutputFile.Add("\ttranslate([0, 0, -2]) cylinder(h = 2,$fn = 40, d = 1);");
                        OutputFile.Add("}");
                        OutputFile.Add("");


                        BOM.LoadRotationOffsets();
                        string Name = Path.GetFileNameWithoutExtension(S);
                        string kicadschname = Path.Combine(Path.GetDirectoryName(S), Name + ".kicad_sch");
                        string kicadpcbname = Path.Combine(Path.GetDirectoryName(S), Name + ".kicad_pcb");
                        OutputFile2.Add(String.Format("type;x;y"));
                        if (File.Exists(kicadschname) && File.Exists(kicadpcbname))
                        {
                            // try
                            {
                                BOM TheBOM = new BOM();
                                TheBOM.LoadKicad(kicadschname, kicadpcbname, new StandardConsoleLog());
                                if (TheBOM.DeviceTree.ContainsKey("LED_0603_1608MetricLED_0603_1608Metric"))
                                {
                                    foreach(var a in TheBOM.DeviceTree["LED_0603_1608MetricLED_0603_1608Metric"].Values)
                                    {
                                        foreach(var rd in a.RefDes)
                                        {
                                            string PipeLine = String.Format("translate([{0}, {1}, 0]) pipe();", rd.x.ToString().Replace(",","."), rd.y.ToString().Replace(",", "."));
                                            OutputFile.Add(PipeLine);
                                            OutputFile2.Add(String.Format("pipe;{0};{1}", rd.x.ToString().Replace(",", "."), rd.y.ToString().Replace(",", ".")));
                                        }
                                    }
                                 }
                                if (TheBOM.DeviceTree.ContainsKey("LightpipeHoleLightpipeHole"))
                                {
                                    foreach (var a in TheBOM.DeviceTree["LightpipeHoleLightpipeHole"].Values)
                                    {
                                        foreach (var rd in a.RefDes)
                                        {
                                            string PipeLine = String.Format("translate([{0}, {1}, 0]) support();", rd.x.ToString().Replace(",", "."), rd.y.ToString().Replace(",", "."));
                                            OutputFile.Add(PipeLine);
                                            OutputFile2.Add(String.Format("pin;{0};{1}", rd.x.ToString().Replace(",", "."), rd.y.ToString().Replace(",", ".")));

                                        }
                                    }
                                }

                            }
                            File.WriteAllLines(Path.Combine(Path.GetDirectoryName(S), Name + ".scad"), OutputFile);
                            File.WriteAllLines(Path.Combine(Path.GetDirectoryName(S), Name + "_tabel.csv"), OutputFile2);
                        }

                    }
                    catch(Exception E)
                    {

                    }
                }
            }
        }

    }
}
