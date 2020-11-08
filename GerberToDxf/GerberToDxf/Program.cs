using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GerberLibrary;
using netDxf;
using GerberLibrary.Core;

namespace GerberToDXF
{
    class Program
    {

        static void ConvertFile(string from, string to, bool displayshapes, bool outlineshapes)
        {

            ParsedGerber PLS = PolyLineSet.LoadGerberFile(new StandardConsoleLog(), from,true, State: new GerberParserState() {  PreCombinePolygons = true });



            DxfDocument dxf = new DxfDocument();
            // add your entities here


            if (outlineshapes) foreach (var a in PLS.OutlineShapes)
                {

                    List<netDxf.Entities.PolylineVertex> Vertices = new List<netDxf.Entities.PolylineVertex>();
                    foreach (var v in a.Vertices)
                    {
                        Vertices.Add(new netDxf.Entities.PolylineVertex(v.X, v.Y, 0));
                    }
                    netDxf.Entities.Polyline pl = new netDxf.Entities.Polyline(Vertices, true);
                    pl.Color = new AciColor(System.Drawing.Color.Blue);
                    dxf.AddEntity(pl);

                }
            if (displayshapes) foreach (var a in PLS.DisplayShapes)
                {
                    List<netDxf.Entities.PolylineVertex> Vertices = new List<netDxf.Entities.PolylineVertex>();
                    foreach (var v in a.Vertices)
                    {
                        Vertices.Add(new netDxf.Entities.PolylineVertex(v.X, v.Y, 0));
                    }
                    netDxf.Entities.Polyline pl = new netDxf.Entities.Polyline(Vertices, true);
                    pl.Color = new AciColor(System.Drawing.Color.Green);
                    dxf.AddEntity(pl);
                }
            if (false) foreach (var a in PLS.Shapes)
                {

                    List<netDxf.Entities.PolylineVertex> Vertices = new List<netDxf.Entities.PolylineVertex>();
                    foreach (var v in a.Vertices)
                    {
                        Vertices.Add(new netDxf.Entities.PolylineVertex(v.X, v.Y, 0));
                    }
                    netDxf.Entities.Polyline pl = new netDxf.Entities.Polyline(Vertices, true);
                    pl.Color = new AciColor(System.Drawing.Color.Red);
                    dxf.AddEntity(pl);
                }

            // save to file
            dxf.Save(to);

        }
        static void Main(string[] args)
        {
            if (args.Count() < 2)
            {
                //  Console.WriteLine("Arguments: <infolder>   converts *.pnl in folder to *.pnl.dxf");
                Console.WriteLine("Arguments: <infile> <outfile> <-nooutline> <-nodisplay>  converts infile to outfile");

                return;
            }

            bool outlineshapes = true;
            bool displayshapes = true;
            for (int i = 2; i < args.Count(); i++)
            {
                if (args[i] == "-nooutline") outlineshapes = false;
                if (args[i] == "-nodisplay") displayshapes = false;
            }
            if (System.IO.File.Exists(args[0]) == false)
            {
                if (System.IO.Directory.Exists(args[0]))
                {
                    var V = System.IO.Directory.GetFiles(args[0]);
                    foreach (var a in V)
                    {
                        if (a.ToLower().EndsWith(".dxf") == false && a.ToLower().EndsWith(".ai") == false && a.ToLower().EndsWith(".gpi") == false)
                        {
                            ConvertFile(a, a + ".dxf", displayshapes, outlineshapes);
                        }
                    }
                    return;
                }
                else
                {
                    Console.WriteLine("file/folder not found: {0}", args[0]);
                    return;
                }
            }

            ConvertFile(args[0], args[1], displayshapes, outlineshapes);


        }
    }
}
