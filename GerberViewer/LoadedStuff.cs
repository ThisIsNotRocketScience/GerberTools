using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GerberLibrary;
using GerberLibrary.Core;

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
        }
        public GerberLibrary.BoardRenderColorSet Colors = new GerberLibrary.BoardRenderColorSet();

        public List<DisplayGerber> Gerbers = new List<DisplayGerber>();

        public void AddFile(string filename, double drillscaler = 1.0)
        {
            string[] filesplit = filename.Split('.');
            string ext = filesplit[filesplit.Count() - 1].ToLower();

            var FileType = Gerber.FindFileType(filename);

            if (FileType == BoardFileType.Unsupported)
            {
                return;
            }

            ParsedGerber PLS;


            GerberParserState State = new GerberParserState() { PreCombinePolygons = false };
            if (FileType == BoardFileType.Drill)
            {
                if (Gerber.ExtremelyVerbose) Console.WriteLine("Log: Drill file: {0}", filename);
                PLS = PolyLineSet.LoadExcellonDrillFile(filename, false, drillscaler);
                // ExcellonFile EF = new ExcellonFile();
                // EF.Load(a);
            }
            else
            {

                bool forcezerowidth = false;
                bool precombinepolygons = false;
                BoardSide Side = BoardSide.Unknown;
                BoardLayer Layer = BoardLayer.Unknown;
                Gerber.DetermineBoardSideAndLayer(filename, out Side, out Layer);
                if (Layer == BoardLayer.Outline)
                {
                    forcezerowidth = true;
                    precombinepolygons = true;
                }
                State.PreCombinePolygons = precombinepolygons;


                PLS = PolyLineSet.LoadGerberFile(filename, forcezerowidth, false, State);
                PLS.Side = Side;
                PLS.Layer = Layer;
            }

            Gerbers.Add(new DisplayGerber() { File = PLS, visible = true, sortindex = Gerber.GetDefaultSortOrder(PLS.Side, PLS.Layer), Color = Colors.GetDefaultColor(PLS.Layer, PLS.Side)});
        }
    }
}
