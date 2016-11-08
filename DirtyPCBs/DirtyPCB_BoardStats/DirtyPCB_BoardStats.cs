using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ionic.Zip;
using System.Web.Script.Serialization;

namespace DirtyPCB_BoardStats
{
    class DirtyPCB_BoardStats
    {
        static void Main(string[] args)
        {
            if (args.Count()<1)
            {
                Console.WriteLine("Usage: DirtyPCB_BoardStats.exe <infile>|<infolder>|<inzip>");
                return;
            }

            Stats TheStats = new Stats();
            if (Directory.Exists(args[0]))
            {
                GerberLibrary.GerberImageCreator GIC = new GerberLibrary.GerberImageCreator();
                
                foreach(var L in Directory.GetFiles(args[0]).ToList())
                {
                    TheStats.AddFile(L);
                }
                
            }
            else
            if (File.Exists(args[0]))
            {
                if (Path.GetExtension(args[0]).ToLower() == ".zip")
                {
                    using (ZipFile zip1 = ZipFile.Read(args[0]))
                    {
                        foreach (ZipEntry e in zip1)
                        {
                            MemoryStream MS = new MemoryStream();
                            if (e.IsDirectory == false)
                            {
                                e.Extract(MS);
                                MS.Seek(0, SeekOrigin.Begin);
                                TheStats.AddFile( MS, e.FileName);
                            }
                        }
                    }
                }
                else
                {
                    TheStats.AddFile(args[0]);
                }

            }
            TheStats.Complete();

            var json = new JavaScriptSerializer().Serialize(TheStats);
            Console.WriteLine(json);
            

        }

        public class Stats
        {
            public int DrillCount;
            public double Width;
            public double Height;
            private GerberLibrary.PolyLineSet.Bounds Box = new GerberLibrary.PolyLineSet.Bounds();


            public void AddFile(MemoryStream L, string filename)
            {
                L.Seek(0, SeekOrigin.Begin);

                var T = GerberLibrary.Gerber.FindFileTypeFromStream(new StreamReader(L), filename);
                switch (T)
                {
                    case GerberLibrary.Core.BoardFileType.Drill:
                        {
                            GerberLibrary.ExcellonFile EF = new GerberLibrary.ExcellonFile();
                            L.Seek(0, SeekOrigin.Begin);

                            EF.Load(new StreamReader(L));
                            DrillCount += EF.TotalDrillCount();
                        }
                        break;
                    case GerberLibrary.Core.BoardFileType.Gerber:
                        {
                            GerberLibrary.Core.BoardSide Side;
                            GerberLibrary.Core.BoardLayer Layer;
                            GerberLibrary.Gerber.DetermineBoardSideAndLayer(filename, out Side, out Layer);
                            if (Layer == GerberLibrary.Core.BoardLayer.Outline || Layer == GerberLibrary.Core.BoardLayer.Mill)
                            {
                                L.Seek(0, SeekOrigin.Begin);
                                var G = GerberLibrary.PolyLineSet.LoadGerberFileFromStream(new StreamReader(L), filename);
                                Box.AddBox(G.BoundingBox);
                            }
                        }
                        break;
                }


            }

            public void AddFile(string L)
            {
                var T = GerberLibrary.Gerber.FindFileType(L);
                switch (T)
                {
                    case GerberLibrary.Core.BoardFileType.Drill:
                        {
                            GerberLibrary.ExcellonFile EF = new GerberLibrary.ExcellonFile();
                            EF.Load(L);
                            DrillCount += EF.TotalDrillCount();
                        }
                        break;
                    case GerberLibrary.Core.BoardFileType.Gerber:
                        {
                            GerberLibrary.Core.BoardSide Side;
                            GerberLibrary.Core.BoardLayer Layer;
                            GerberLibrary.Gerber.DetermineBoardSideAndLayer(L, out Side, out Layer);
                            if (Layer == GerberLibrary.Core.BoardLayer.Outline || Layer == GerberLibrary.Core.BoardLayer.Mill)
                            {
                                var G = GerberLibrary.PolyLineSet.LoadGerberFile(L);
                                Box.AddBox(G.BoundingBox);
                            }
                        }
                        break;
                }


            }

            public void Complete()
            {
                Width = Box.Width();
                Height = Box.Height();   
            }
        }
        
    }
}
