using GerberLibrary;
using GerberLibrary.Core;
using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberSplitter
{
    class Program
    {

        internal class log : ProgressLog
        {
            public void AddString(string text, float progress = -1F)
            {
                Console.WriteLine("   ***** {0}", text);
            }
        }

        static void Main(string[] args)
        {
            if (args.Count() >= 2)
            {
                string slicefile = args[0];
                List<string> gerbers = new List<string>();
                gerbers.AddRange(args.Skip(1));
                Slice(slicefile, gerbers);
            }
        }

        static void Slice(string slicefile, List<string> inputgerbers)
        { 
            List<PolyLine> SliceSet = new List<PolyLine>();
            
            var OutputFolder = Path.GetDirectoryName(slicefile) + @"\Output\" + Path.GetFileNameWithoutExtension(slicefile);

            ParsedGerber P = PolyLineSet.LoadGerberFile(slicefile);

            foreach (var l in P.Shapes)
            {
                /* Polygon Pp = new Polygon();
                Pp.Closed = true;
                var vertL = from i in l.Vertices select new vec2(i.X, i.Y);
                Pp.Vertices.AddRange(vertL);*/
                SliceSet.Add(l);
            }

            int slid = 1;

            foreach (var S in SliceSet)
            {
                Console.WriteLine("Slicing {0}/{1}", slid, SliceSet.Count); 
                var SliceOutputFolder = Path.GetDirectoryName(slicefile) + @"\Output\" + Path.GetFileNameWithoutExtension(slicefile) + "\\Slice" + slid.ToString();
                if (Directory.Exists(SliceOutputFolder) == false) Directory.CreateDirectory(SliceOutputFolder);

                foreach (var a in inputgerbers)
                {
                    try
                    {

                        var bf = GerberLibrary.Gerber.FindFileType(a);
                        if (bf == BoardFileType.Gerber)
                        {
                            BoardSide bs;
                            BoardLayer L;
                            GerberLibrary.Gerber.DetermineBoardSideAndLayer(a, out bs, out L);

                            GerberMerger.WriteContainedOnly(a, S, Path.Combine(SliceOutputFolder + "\\", Path.GetFileName(a)), new log());
                        }
                    }
                    catch (Exception) { };
                }
                slid++;
            }

        }
    }
}
