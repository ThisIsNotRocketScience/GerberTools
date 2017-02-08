using System;
using System.Collections.Generic;
using System.Drawing;
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
            string inputfolder = ".";

            if (args.Length > 0)
            {
                inputfolder = args[0];
            }

            
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
            var Generated = ScanFolder(inputfolder, outputfolder);
            {
                int c = 0;
                foreach (var a in Generated.OrderBy(x => x.Label.ToLower()))
                {
                    File.Delete(a.Outputfilename);
                    Artwork.TINRSArtWorkRenderer.SaveMultiIcon(a.Outputfilename, a.Label, (float)c/(float)Generated.Count);
                    if (outputfolder.Length > 0)
                    {
                        string fileName = a.Label + ".ico";
                        foreach (char cc in System.IO.Path.GetInvalidFileNameChars())
                        {
                            fileName = fileName.Replace(cc, '_');
                        }

                        Artwork.TINRSArtWorkRenderer.SaveMultiIcon(Path.Combine(outputfolder, fileName), a.Label, (float)c / (float)Generated.Count);
                    }
                    c++;
                }
            }

            if (outputfolder.Length > 0)
            {
                int iconsize = 128;
                int iconspacing = 10;
                int Size = (int)Math.Ceiling(Math.Sqrt(Generated.Count));
                Bitmap B = new Bitmap((iconsize + iconspacing)* Size, (iconsize + iconspacing) * Size);
                Graphics T = Graphics.FromImage(B);
                T.Clear(Color.Black);
                int c = 0;

                foreach(var a in Generated.OrderBy(x => x.Label.ToLower()))
                {
                    Bitmap C = new Bitmap(iconsize, iconsize);
                    Graphics.FromImage(C).Clear(Color.Transparent);
                    Artwork.TINRSArtWorkRenderer.DrawIcon(iconsize, iconsize, Graphics.FromImage(C), a.Label,(float)c/(float)Generated.Count );
                    T.DrawImage(C, (c % Size) * (iconsize + iconspacing) + iconspacing / 2, (c / Size) * (iconsize + iconspacing) + iconspacing / 2);
                    c++;
                }

                B.Save(Path.Combine(outputfolder, "IconSheet.png"));
            }            
            
        }
        public class IcoEntry
        {
            public string Label;
            public string Outputfilename;
            
        }
        private static List<IcoEntry> ScanFolder(string inputfolder, string outputfolder)
        {
            List<IcoEntry> Res = new List<IcoEntry>();
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


                                string Label = l.Substring(i1 + 1, i2 - i1 - 1);
                                Console.WriteLine("Creating icon for \"{0}\"", Label);
                                Res.Add(new IcoEntry() { Label = Label, Outputfilename = a });


                            }
                        }
                    }
                }
            }
            foreach(var d in Directory.GetDirectories(inputfolder))
            {
                Res.AddRange(ScanFolder(d, outputfolder));
            }
            return Res;
        }
    }
}
