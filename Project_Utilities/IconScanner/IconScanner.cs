using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconScanner
{
    class IconScanner
    {
        static void Main(string[] args)
        {

            if (args.Length < 1)
            {
                Console.WriteLine("Usage: IconScanner <inputfolder>");
                return;
            }

            string inputfolder = args[0];
            string outputfolder = "";

            if (args.Length > 1)
            {
                outputfolder = args[1];
                if (Directory.Exists(outputfolder) == false) Directory.CreateDirectory(outputfolder);
            }
            if (Directory.Exists(inputfolder) == false)
            {
                Console.WriteLine("Directory not found!");
                return;

            }
            ScanFolder(inputfolder, outputfolder);
            
            
        }

        private static void ScanFolder(string inputfolder, string outputfolder)
        {
            foreach (var a in Directory.GetFiles(inputfolder))
            {
                if (Path.GetExtension(a).ToLower() == ".ico" && a.ToLower().Contains("favicon") == true)
                {
                    var V = Path.GetDirectoryName(a);
                    var AsmInf = Path.Combine(V, "Properties\\AssemblyInfo.cs");
                    if (File.Exists(AsmInf))
                    {
                        var L = File.ReadAllLines(AsmInf);
                        foreach(var l in L)
                        {
                            if (l.Contains("AssemblyProduct"))
                            {
                                int i1 = l.IndexOf('\"');
                                int i2 = l.LastIndexOf('"');
                                File.Delete(a);


                                string Label = l.Substring(i1 + 1, i2 - i1 - 1);
                                Console.WriteLine("Creating icon for \"{0}\"", Label);

                                Artwork.TINRSArtWorkRenderer.SaveMultiIcon(a, Label);
                                if (outputfolder.Length > 0)
                                {
                                    string fileName = Label + ".ico";
                                    foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                                    {
                                        fileName = fileName.Replace(c, '_');
                                    }                                  

                                    Artwork.TINRSArtWorkRenderer.SaveMultiIcon(Path.Combine(outputfolder, fileName), Label);
                                }


                            }
                        }
                    }
                }
            }
            foreach(var d in Directory.GetDirectories(inputfolder))
            {
                ScanFolder(d, outputfolder);
            }
        }
    }
}
