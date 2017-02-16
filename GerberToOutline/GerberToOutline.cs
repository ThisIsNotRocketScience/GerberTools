using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GerberLibrary;
using GerberLibrary.Core;
using System.IO;
using System.Drawing;

namespace GerberToOutline
{
    class GerberToOutline
    {
        static void Main(string[] args)
        {
            if (args.Count() < 2)
            {
                Console.WriteLine("Usage: GerberToOutline.exe <infile> <outfile>");
                return;
            }

            string infile = args[0];
            string outfile = args[1];

            var FileType = Gerber.FindFileType(infile);

            if (FileType == BoardFileType.Unsupported)
            {
                if (Gerber.ExtremelyVerbose) Console.WriteLine("Warning: {1}: files with extension {0} are not supported!", Path.GetExtension(infile), Path.GetFileName(infile));
                return;
            }


            ParsedGerber PLS;
            GerberParserState State = new GerberParserState() { PreCombinePolygons = false };

            if (FileType == BoardFileType.Drill)
            {
                if (Gerber.ExtremelyVerbose) Console.WriteLine("Log: Drill file: {0}", infile);
                PLS = PolyLineSet.LoadExcellonDrillFile(infile, false, 1.0);
                // ExcellonFile EF = new ExcellonFile();
                // EF.Load(a);
            }
            else

            {
                bool forcezerowidth = false;
                bool precombinepolygons = false;
                if (Gerber.ExtremelyVerbose) Console.WriteLine("Log: Gerber file: {0}", infile);
                BoardSide Side = BoardSide.Unknown;
                BoardLayer Layer = BoardLayer.Unknown;
                Gerber.DetermineBoardSideAndLayer(infile, out Side, out Layer);
                if (Layer == BoardLayer.Outline || Layer == BoardLayer.Mill)
                {
                    forcezerowidth = true;
                    precombinepolygons = true;
                }
                State.PreCombinePolygons = precombinepolygons;

                PLS = PolyLineSet.LoadGerberFile(infile, forcezerowidth, false, State);
                PLS.Side = State.Side;
                PLS.Layer = State.Layer;
                if (Layer == BoardLayer.Outline)
                {
                    PLS.FixPolygonWindings();
                }

            }
            SVGGraphicsInterface SG = new SVGGraphicsInterface(PLS.BoundingBox.Width()*100, PLS.BoundingBox.Height()*100);
            SG.ScaleTransform(10, 10);
            SG.TranslateTransform((float)-PLS.BoundingBox.TopLeft.X, (float)-PLS.BoundingBox.TopLeft.Y);
            DrawToInterface(PLS, SG);
            SG.Save(outfile);
            System.Diagnostics.Process.Start(outfile);
            Console.WriteLine("press any key to continue..");
            Console.ReadKey();

        }

        private static void DrawToInterface(ParsedGerber PLS, SVGGraphicsInterface SG)
        {
            Random R = new Random();

            int i2 = 0;
            foreach (var a in PLS.DisplayShapes)
            {
                i2++;
                Pen P = new Pen(Color.FromArgb((byte)R.Next(), (byte)R.Next(), (byte)R.Next()), 0.1f);
                for (int i = 0; i < a.Vertices.Count; i++)
                {
                    var v1 = a.Vertices[i];
                    var v2 = a.Vertices[(i + 1) % a.Vertices.Count];
                   
                    SG.DrawLine(P, 54.2f + (float)v1.X + i2*12.4f, (float)v1.Y,54.2f+ (float)v2.X + i2*12.4f , (float)v2.Y);
                    SG.DrawLine(P, (float)v1.X , (float)v1.Y, (float)v2.X , (float)v2.Y);

                }

            }
        }
    }
}
