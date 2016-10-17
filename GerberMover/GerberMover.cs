using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GerberLibrary;
using System.IO;
using System.Drawing;
using GerberLibrary.Core;

namespace GerberMover
{
    class GerberMover
    {
        static void Main(string[] args)
        {

            

            if (args.Count() < 2)
            {
                Console.WriteLine("usage: ");
                Console.WriteLine("GerberMover <inputfile> <outputfile> <X> <Y> <CX> <CY> <Angle>");
                Console.WriteLine("<inputfile>: file to load - this can be either gerber or excellon file");
                Console.WriteLine("<outputfile>: file to write - this will be the same format as the input file");
                Console.WriteLine("<X> <Y>: amount of millimeter to move the input - this will be the offset of the rotation center point too");
                Console.WriteLine("<CX> <CY> <Angle>: center of rotation and angle. Millimeters and degrees"); 
                Console.WriteLine("Only <inputfile> and <outputfile> are needed, the rest is optional.");

                return;
            }

            if (File.Exists(args[0]) == false)
            {
                Console.WriteLine("File {0} not found!", args[0]);
            }
            else
            {
                double dx = 0;
                double dy = 0;
                double cx = 0;
                double cy = 0;
                double angle = 0;
                if (args.Count() > 2) double.TryParse(args[2], out dx);
                if (args.Count() > 3) double.TryParse(args[3], out dy);
                if (args.Count() > 4) double.TryParse(args[4], out cx);
                if (args.Count() > 5) double.TryParse(args[5], out cy);
                if (args.Count() > 6) double.TryParse(args[6], out angle);

                var T = Gerber.FindFileType(args[0]);
                if (T == BoardFileType.Drill)
                {
                    ExcellonFile EF = new ExcellonFile();
                    EF.Load(args[0]);
                    EF.Write(args[1], dx, dy, cx, cy, angle);
                }
                else
                {
                    BoardSide Side;
                    BoardLayer Layer;
                    Gerber.DetermineBoardSideAndLayer(args[0], out Side, out Layer);

                    GerberTransposer GT = new GerberTransposer();
                    GT.Transform(args[0], args[1], dx, dy, cx, cy, angle);

                    Gerber.SaveGerberFileToImage(args[1], args[1]+ "_render.png", 200, Color.Black, Color.White);

                }
            }
            //Console.WriteLine("Press any key to continue..");
            Console.ReadKey();
        }
    }
}
