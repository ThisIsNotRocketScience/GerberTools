using GerberLibrary;
using GerberLibrary.Core;
using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberAnalyse
{
    class GerberAnalyse
    {
        static void Main(string[] args)
        {


            GerberNumberFormat GNF = new GerberNumberFormat();
            GNF.DigitsAfter = 3;
            GNF.DigitsBefore = 3;
            GNF.SetMetricMode();

            GerberSplitter GS = new GerberSplitter();
            GS.Split("X-1.15Y-1.9", GNF, true);

            foreach(var a in GS.Pairs)
            {
                Console.WriteLine("{0}:{1} {2}", a.Key, a.Value.Command, a.Value.Number);
            }
            Console.ReadKey();

            if (args.Count() == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("GerberAnalyse <inputfile> <-forcezerowidth> <-dim>");
                return;
            }
            ParsedGerber PLS;
            bool forcezerowidth = false;
            bool compact = false;

            for (int i = 1; i < args.Count(); i++)
            {
                if (args[i] == "-forcezerowidth") forcezerowidth = true;
                if (args[i] == "-dim") compact = true;
            }

            if (Gerber.FindFileType(args[0].ToLower())  == BoardFileType.Drill)
            {
                PLS = PolyLineSet.LoadExcellonDrillFile(args[0]);
                // ExcellonFile EF = new ExcellonFile();
                // EF.Load(a);
            }
            else
            {
               // PLS.PreCombinePolygons = false;
               // forcezerowidth = true;
                PLS = PolyLineSet.LoadGerberFile(args[0], forcezerowidth, false, new GerberParserState() {  PreCombinePolygons = false});
            }

            PLS.CalcPathBounds();

            if (compact) {

                CultureInfo CI = CultureInfo.InvariantCulture;

                Console.WriteLine("{0}x{1}(mm)", (PLS.BoundingBox.BottomRight.X - PLS.BoundingBox.TopLeft.X).ToString("N3", CI), (PLS.BoundingBox.BottomRight.Y - PLS.BoundingBox.TopLeft.Y).ToString("N3", CI));
                Console.WriteLine("{0}x{1}(imp)", ((PLS.BoundingBox.BottomRight.X - PLS.BoundingBox.TopLeft.X) / 25.4).ToString("N3", CI), ((PLS.BoundingBox.BottomRight.Y - PLS.BoundingBox.TopLeft.Y) / 25.4).ToString("N3", CI));
            }
            else
            {
                Console.WriteLine("Report for {0}:", args[0]);
                Console.WriteLine("Suspected file side: {0}, layertype: {1}", PLS.Side.ToString(), PLS.Layer.ToString());
                Console.WriteLine("DisplayShape #: {0}", PLS.DisplayShapes.Count);
                foreach (var o in PLS.DisplayShapes)
                {
                    Console.WriteLine("\tOutline {0} vertices thin:{1}", o.Vertices.Count, o.Thin);
                    foreach (var v in o.Vertices)
                    {
                        Console.WriteLine("\t\t{0}", v);
                    }

                }

                Console.WriteLine("OutlineShape #: {0}", PLS.OutlineShapes.Count);
                foreach (var o in PLS.OutlineShapes)
                {
                    Console.WriteLine("\tOutline {0} vertices thin:{1}", o.Vertices.Count, o.Thin);
                    foreach (var v in o.Vertices)
                    {
                        Console.WriteLine("\t\t{0}", v);
                    }
                }
                Console.WriteLine("Aperture #: {0}", PLS.State.Apertures.Count);
                foreach (var apt in PLS.State.Apertures)
                {
                    Console.Write("\tAperture D{0} ", apt.Key.ToString("D2"));
                    Console.Write("type: {0} ", apt.Value.ShapeType.ToString());
                    switch (apt.Value.ShapeType)
                    {
                        case GerberApertureShape.Circle:

                            Console.Write("diameter {0} ", apt.Value.CircleRadius * 2); break;
                    }
                    Console.WriteLine();
                }
                Console.WriteLine("Corners: ");
                Console.WriteLine(PLS.BoundingBox.TopLeft);
                Console.WriteLine(PLS.BoundingBox.BottomRight);
                Console.WriteLine("Size: {0}x{1} mm", PLS.BoundingBox.BottomRight.X - PLS.BoundingBox.TopLeft.X, PLS.BoundingBox.BottomRight.Y - PLS.BoundingBox.TopLeft.Y);
            }
          //  Console.WriteLine("Press any key to continue");
          //  Console.ReadKey();
        }
    }
}
