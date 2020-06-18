using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GerberLibrary;
using GerberLibrary.Core;
using Ionic.Zip;
using System.IO;

namespace GerberViewer
{
    public class LoadedStuff
    {
        public class DisplayGerber

        {
            public bool visible;
            public ParsedGerber File;
            public int sortindex;
            public Color Color;
            internal LayerDisplay Panel;
        }
        public GerberLibrary.BoardRenderColorSet Colors = new GerberLibrary.BoardRenderColorSet();

        public List<DisplayGerber> Gerbers = new List<DisplayGerber>();
        internal bool CrossHairActive;
        internal float MouseY;
        internal float MouseX;

        public void AddFileStream(ProgressLog log, MemoryStream S, string origfilename, double drillscaler = 1.0)
        {
            var FileType = Gerber.FindFileTypeFromStream(new StreamReader( S), origfilename);

            S.Seek(0, SeekOrigin.Begin);
           
            if (FileType == BoardFileType.Unsupported)
            {
                return;
            }

            ParsedGerber PLS;


            GerberParserState State = new GerberParserState() { PreCombinePolygons = false };
            if (FileType == BoardFileType.Drill)
            {
                if (Gerber.ExtremelyVerbose) Console.WriteLine("Log: Drill file: {0}", origfilename);
                PLS = PolyLineSet.LoadExcellonDrillFileFromStream(log, new StreamReader(S), origfilename, false, drillscaler);
                S.Seek(0, SeekOrigin.Begin);

                // ExcellonFile EF = new ExcellonFile();
                // EF.Load(a);
            }
            else
            {

                bool forcezerowidth = false;
                bool precombinepolygons = false;
                BoardSide Side = BoardSide.Unknown;
                BoardLayer Layer = BoardLayer.Unknown;
                Gerber.DetermineBoardSideAndLayer(origfilename, out Side, out Layer);
                if (Layer == BoardLayer.Outline || Layer == BoardLayer.Mill)
                {
                    forcezerowidth = true;
                    precombinepolygons = true;
                }
                State.PreCombinePolygons = precombinepolygons;
               

                PLS = PolyLineSet.LoadGerberFileFromStream(new StreamReader(S),origfilename, forcezerowidth, false, State);
                S.Seek(0, SeekOrigin.Begin);

                PLS.Side = Side;
                PLS.Layer = Layer;
            }

            Gerbers.Add(new DisplayGerber() { File = PLS, visible = true, sortindex = Gerber.GetDefaultSortOrder(PLS.Side, PLS.Layer), Color = Colors.GetDefaultColor(PLS.Layer, PLS.Side) });

        }

        public void AddFile(ProgressLog log, string filename, double drillscaler = 1.0)
        {

            string[] filesplit = filename.Split('.');
            string ext = filesplit[filesplit.Count() - 1].ToLower();
            if (ext == "zip")
            {
                using (ZipFile zip1 = ZipFile.Read(filename))
                {
                    foreach (ZipEntry e in zip1)
                    {
                        MemoryStream MS = new MemoryStream();
                        if (e.IsDirectory == false)
                        {
                            e.Extract(MS);
                            MS.Seek(0, SeekOrigin.Begin);
                            AddFileStream(log, MS, e.FileName, drillscaler);
                        }
                    }
                }
                return;

            }


            MemoryStream MS2 = new MemoryStream();
            FileStream FS = File.OpenRead(filename);
            FS.CopyTo(MS2);
            MS2.Seek(0, SeekOrigin.Begin);
            AddFileStream(log, MS2, filename, drillscaler);

        }
    }
}
