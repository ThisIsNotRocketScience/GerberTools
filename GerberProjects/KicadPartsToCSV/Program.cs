using GerberLibrary;
using GerberLibrary.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace KicadPartsToCSV
{
    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo ci = new CultureInfo("nl-NL");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: KicadPartsToCSV <kicadboardfile> [part name] [part name] - omit part names to see list of parts");
                return;
            }
            string inputfile = args[0];
            if (File.Exists(inputfile))
            {
                BOM.LoadRotationOffsets();
                string Name = Path.GetFileNameWithoutExtension(inputfile);
                string kicadschname = Path.Combine(Path.GetDirectoryName(inputfile), Name + ".kicad_sch");
                string kicadpcbname = Path.Combine(Path.GetDirectoryName(inputfile), Name + ".kicad_pcb");


                if (File.Exists(kicadschname) && File.Exists(kicadpcbname))
                {
                    // try
                    {
                        BOM TheBOM = new BOM();
                        TheBOM.LoadKicad(kicadschname, kicadpcbname, new StandardConsoleLog());

                        if (args.Length == 1)
                        {
                            string csvoutputname = Path.Combine(Path.GetDirectoryName(inputfile), Name + ".part_types.csv");
                            List<string> OutputFile2 = new List<string>();

                            foreach (var a in TheBOM.DeviceTree.Keys)
                            {
                                OutputFile2.Add(a);
                            }
                            File.WriteAllLines(csvoutputname, OutputFile2);
                        }
                        else
                        {
                            string csvoutputname = Path.Combine(Path.GetDirectoryName(inputfile), Name + ".part_locations.csv");

                            List<string> OutputFile2 = new List<string>();
                            OutputFile2.Add(String.Format("type;x;y"));

                            for(int j = 1;j<args.Length;j++)
                            {
                                if (TheBOM.DeviceTree.ContainsKey(args[j]))
                                {
                                    foreach (var a in TheBOM.DeviceTree[args[j]].Values)
                                    {
                                        foreach (var rd in a.RefDes)
                                        {
                                            OutputFile2.Add(String.Format("{2};{0};{1}", rd.x.ToString().Replace(",", "."), rd.y.ToString().Replace(",", "."),args[j]));
                                        }
                                    }
                                }

                            }
                            File.WriteAllLines(csvoutputname, OutputFile2);
                        }
                    }
                }
            }
        }
    }
}
