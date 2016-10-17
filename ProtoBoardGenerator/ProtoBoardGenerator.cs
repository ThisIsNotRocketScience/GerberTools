using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GerberLibrary;
using System.IO;
using System.Drawing;
using GerberLibrary.Core.Primitives;
using GerberLibrary.Core;

namespace ProtoBoardGenerator
{
    class ProtoBoardGenerator
    {
        static void Main(string[] args)
        {
            if (args.Count() < 5)
            {
                Console.WriteLine("usage: ProtoBoardGenerator.exe <width> <height> <mountholediameter> <cornerrounding> <mountholeclearance> ");
                return;    
            }

            double width = double.Parse(args[0]);
            double height = double.Parse(args[1]);
            double mounthole = double.Parse(args[2]);
            double rounding = double.Parse(args[3]);
            double clearance = double.Parse(args[4]);
            //double width = double.Parse(args[0]);

            WriteProtoBoard(width, height, mounthole, rounding, clearance);
            WriteProtoBoardFlowerStyle(width, height, mounthole, rounding, clearance);
        }

        public static List<String> WriteProtoBoardFlowerStyle(double width, double height, double mountholediameter, double cornerrounding, double mountholeclearance, double edgeclearance = 1.26, bool WriteCombinedImage = true, bool WriteImages = false)
        {
            FontSet FS = FontSet.Load("Font.xml");

            List<String> FilesGenerated = new List<string>();
            double LabelHeight = 1.2;
            double PadWidth = 1.8;
            double DrillWidth = 1.0;

            string basename = String.Format("Generated_This_is_not_RocketScience_Flower_prototoboard_{0}x{1}cm", width / 10.0f, height / 10.0f);

            try
            {
                Directory.CreateDirectory(basename);
            }
            catch (Exception)
            {

            }
            basename = Path.Combine(basename, String.Format("proto_{0}x{1}cm", width / 10.0f, height / 10.0f));
            string OutlineFile = basename + ".gko";
            string SilkFileTop = basename + ".gto";
            string SilkFileBottom = basename + ".gbo";
            string SolderMaskBottom = basename + ".gbs";
            string SolderMaskTop = basename + ".gts";
            string CopperBottom = basename + ".gbl";
            string CopperTop = basename + ".gtl";
            string DrillFile = basename + ".txt";


            int XE = (int)Math.Floor((width - Math.Max(edgeclearance, LabelHeight) * 2) / 2.54);
            int YE = (int)Math.Floor((height - Math.Max(edgeclearance, LabelHeight) * 2) / 2.54);

            PrepareGridSpace(XE, YE, true, true, 3, 2);

            string[][] Grid = new string[XE + 1][];
            for (int x = 0; x <= XE; x++)
            {
                Grid[x] = new string[YE + 1];
                for (int y = 0; y <= YE; y++)
                {
                    Grid[x][y] = GridSpaceType(x, y);
                    if (y == YE) Grid[x][y] = "None";
                    if (x == XE) Grid[x][y] = "None";
                }
            }

            double XO = (width / 2) - (XE / 2.0) * 2.54;
            double YO = (height / 2) - (YE / 2.0) * 2.54;

            Console.WriteLine("{0} rows and {1} columns", YE, XE);


            ExcellonFile EF = new ExcellonFile();
            PointD C1 = new PointD(0, 0);
            PointD C2 = new PointD(width, 0);
            PointD C3 = new PointD(width, height);
            PointD C4 = new PointD(0, height);
            if (mountholediameter > 0)
            {
                ExcellonTool MountHoleDrill = new ExcellonTool();
                MountHoleDrill.Radius = mountholediameter / 2.0;


                C1 = new PointD(mountholeclearance, mountholeclearance);
                C2 = new PointD(width - mountholeclearance, mountholeclearance);
                C3 = new PointD(width - mountholeclearance, height - mountholeclearance);
                C4 = new PointD(mountholeclearance, height - mountholeclearance);
                MountHoleDrill.Drills.Add(C1);
                MountHoleDrill.Drills.Add(C2);
                MountHoleDrill.Drills.Add(C3);
                MountHoleDrill.Drills.Add(C4);

                EF.Tools[10] = MountHoleDrill;
            }
            ExcellonTool ProtoHoleDrill = new ExcellonTool();
            ProtoHoleDrill.Radius = DrillWidth / 2.0;



            PolyLine PL = new PolyLine();

            PL.Add(cornerrounding, 0);
            PL.Add(width - cornerrounding, 0);
            PL.ArcTo(new PointD(width, cornerrounding), new PointD(0, cornerrounding), InterpolationMode.CounterClockwise);
            PL.Add(width, height - cornerrounding);
            PL.ArcTo(new PointD(width - cornerrounding, height), new PointD(-cornerrounding, 0), InterpolationMode.CounterClockwise);
            PL.Add(cornerrounding, height);
            PL.ArcTo(new PointD(0, height - cornerrounding), new PointD(0, -cornerrounding), InterpolationMode.CounterClockwise);
            PL.Add(0, cornerrounding);
            PL.ArcTo(new PointD(cornerrounding, 0), new PointD(cornerrounding, 0), InterpolationMode.CounterClockwise);
            PL.Close();

            GerberArtWriter GAW = new GerberArtWriter();
            GAW.AddPolyLine(PL, 0);
            GAW.Write(OutlineFile);

            GerberArtWriter GAWTop = new GerberArtWriter();
            GerberArtWriter GAWBottom = new GerberArtWriter();

            GerberArtWriter MaskTop = new GerberArtWriter();
            GerberArtWriter MaskBottom = new GerberArtWriter();
            GerberArtWriter SilkTop = new GerberArtWriter();
            GerberArtWriter SilkBottom = new GerberArtWriter();

            for (int x = 0; x < XE; x++)
            {
                double xc = XO + x * 2.54 + 2.54 / 2;
                for (int y = 0; y < YE; y++)
                {
                    double yc = YO + y * 2.54 + 2.54 / 2;

                    PointD P = new PointD(xc, yc);
                    if ((P - C1).Length() < edgeclearance + mountholediameter / 2 + 1.27) Grid[x][y] = "None";
                    if ((P - C2).Length() < edgeclearance + mountholediameter / 2 + 1.27) Grid[x][y] = "None";
                    if ((P - C3).Length() < edgeclearance + mountholediameter / 2 + 1.27) Grid[x][y] = "None";
                    if ((P - C4).Length() < edgeclearance + mountholediameter / 2 + 1.27) Grid[x][y] = "None";

                    if (Grid[x][y] != "None")
                    {
                        GAWTop.AddFlash(new PointD(xc, yc), DrillWidth*1.3 / 2.0);
                        GAWBottom.AddFlash(new PointD(xc, yc), DrillWidth * 1.3 / 2.0);
                        MaskTop.AddFlash(new PointD(xc, yc), DrillWidth * 1.3 / 2.0);
                        MaskBottom.AddFlash(new PointD(xc, yc), DrillWidth * 1.3 / 2.0);
                        ProtoHoleDrill.Drills.Add(new PointD(xc, yc));
                    }
                }
            }

            //            SilkTop.DrawString(new PointD(5, 0), FS, "O0O", 5, 0.05, StringAlign.BottomCenter);
            //           SilkTop.DrawString(new PointD(20, 0), FS, "O0O", 5, 0.05, StringAlign.CenterCenter);
            //         SilkTop.DrawString(new PointD(40, 0), FS, "OMO", 5, 0.05, StringAlign.TopCenter);

            for (int x = 0; x < XE; x++)
            {
                double xc = XO + x * 2.54 + 2.54 / 2;
                double yc = YO + (0) * 2.54 ;
                double yc1 = YO + (YE) * 2.54 + 2.54 ;
                char T = (char)('A' + (char)((x) % 26));

                if (Grid[x][0] != "None")
                {
                    SilkTop.DrawString(new PointD(xc, yc), FS, T.ToString(), LabelHeight, 0.05, StringAlign.CenterCenter);
                    SilkBottom.DrawString(new PointD(xc, yc), FS, T.ToString(), LabelHeight, 0.05, StringAlign.CenterCenter, true);
                }

                if (Grid[x][YE - 1] != "None")
                {
                    SilkTop.DrawString(new PointD(xc, yc1), FS, T.ToString(), LabelHeight, 0.05, StringAlign.CenterCenter);
                    SilkBottom.DrawString(new PointD(xc, yc1), FS, T.ToString(), LabelHeight, 0.05, StringAlign.CenterCenter, true);
                }
            }
            for (int y = 0; y < YE; y++)
            {
                double xc = XO + (0) * 2.54 - 2.54 / 2;
                double xc1 = XO + (XE + 1) * 2.54 - 2.54 / 2;
                double yc = YO + (y) * 2.54 + 2.54;

                if (Grid[0][y] != "None")
                {
                    SilkTop.DrawString(new PointD(xc, yc), FS, (YE - y).ToString(), LabelHeight, 0.05, StringAlign.CenterCenter);
                    SilkBottom.DrawString(new PointD(xc, yc), FS, (YE - y).ToString(), LabelHeight, 0.05, StringAlign.CenterCenter, true);
                }

                if (Grid[XE - 1][y] != "None")
                {
                    SilkTop.DrawString(new PointD(xc1, yc), FS, (YE - y).ToString(), LabelHeight, 0.05, StringAlign.CenterCenter);
                    SilkBottom.DrawString(new PointD(xc1, yc), FS, (YE - y).ToString(), LabelHeight, 0.05, StringAlign.CenterCenter, true);

                }
            }

            PolyLine PL2 = new PolyLine();
            double rad = 0.6;
            for (int i = 0; i < 20; i++)
            {
                double P = i * Math.PI / 19.0;
                PL2.Add(Math.Sin(P) * rad, Math.Cos(P) * rad);
            }
            PL2.Close();
            PL2.Translate(-1.1, 0);


            for (int x = -1; x < XE + 1; x++)
            {

                for (int y = -1; y < YE + 1; y++)
                {

                    string s1 = "None";
                    if (y >= 0 && y < YE && x < XE) s1 = Grid[x + 1][y];
                    string s2 = "None";
                    if (x >= 0 && x < XE && y < YE) s2 = Grid[x][y + 1];
                    string s3 = "None";
                    if (x >= 0 && x < XE && y >= 0 && y < YE) s3 = Grid[x][y];
                    double xc = XO + x * 2.54 + 2.54 / 2;
                    double yc = YO + y * 2.54 + 2.54 / 2;
                    double xc1 = XO + (x + 1) * 2.54 + 2.54 / 2;
                    double yc1 = YO + (y + 1) * 2.54 + 2.54 / 2;

                    if (s1 != "None" && s3 != "None")
                    {

                        PolyLine PLA = PL2.Copy();
                        PLA.Translate(xc1, yc);
                        PolyLine PLC = PL2.Copy();
                        PLC.RotateDegrees(180);
                        PLC.Translate(xc, yc);
                       
                        GAWTop.AddPolygon(PLA);
                        MaskTop.AddPolygon(PLA);
                        GAWTop.AddPolygon(PLC);
                        MaskTop.AddPolygon(PLC);
                        GAWBottom.AddPolygon(PLA);
                        MaskBottom.AddPolygon(PLA);
                        GAWBottom.AddPolygon(PLC);
                        MaskBottom.AddPolygon(PLC);
                    }

                    if (s3 != "None" && s2 != "None")
                    {
                        PolyLine PLB = PL2.Copy();
                        PLB.RotateDegrees(90);
                        PLB.Translate(xc, yc1);
                        PolyLine PLD = PL2.Copy();
                        PLD.RotateDegrees(270);
                        PLD.Translate(xc, yc);

                        GAWTop.AddPolygon(PLB);
                        MaskTop.AddPolygon(PLB);
                        GAWTop.AddPolygon(PLD);
                        MaskTop.AddPolygon(PLD);
                        GAWBottom.AddPolygon(PLB);
                        MaskBottom.AddPolygon(PLB);
                        GAWBottom.AddPolygon(PLD);
                        MaskBottom.AddPolygon(PLD);
                        //       GAWTop.AddPolygon(PLD);
                        //     MaskTop.AddPolygon(PLD);
                    }
                }
            }

            GAWTop.Write(CopperTop);

            GAWBottom.Write(CopperBottom);
            SilkBottom.Write(SilkFileBottom);
            SilkTop.Write(SilkFileTop);

            MaskTop.Write(SolderMaskTop);
            MaskBottom.Write(SolderMaskBottom);
            EF.Tools[11] = ProtoHoleDrill;
            EF.Write(DrillFile, 0, 0, 0, 0);

            FilesGenerated.Add(CopperBottom);
            FilesGenerated.Add(CopperTop);
            FilesGenerated.Add(OutlineFile);
            FilesGenerated.Add(DrillFile);
            FilesGenerated.Add(SilkFileBottom);
            FilesGenerated.Add(SilkFileTop);
            FilesGenerated.Add(SolderMaskBottom);
            FilesGenerated.Add(SolderMaskTop);


            if (WriteCombinedImage)
            {
                GerberImageCreator GIC = new GerberImageCreator();
                GIC.AddBoardsToSet(FilesGenerated);
                GIC.WriteImageFiles(basename + "_render", 200, false);
            }
            if (WriteImages)
            {
                Gerber.SaveGerberFileToImage(OutlineFile, OutlineFile + "_render.png", 1000, Color.Black, Color.White);
                Gerber.SaveGerberFileToImage(CopperBottom, CopperBottom + "_render.png", 1000, Color.Black, Color.White);
                Gerber.SaveGerberFileToImage(CopperTop, CopperTop + "_render.png", 1000, Color.Black, Color.White);
                Gerber.SaveGerberFileToImage(DrillFile, DrillFile + "_render.png", 1000, Color.Black, Color.White);
                Gerber.SaveGerberFileToImage(SilkFileTop, SilkFileTop + "_render.png", 1000, Color.Black, Color.White);
                Gerber.SaveGerberFileToImage(SilkFileBottom, SilkFileBottom + "_render.png", 1000, Color.Black, Color.White);
            }

            return FilesGenerated;
        }



        public static List<String> WriteProtoBoard(double width, double height, double mountholediameter, double cornerrounding, double mountholeclearance, double edgeclearance = 1.26, bool WriteCombinedImage = true, bool WriteImages = false)
        {
            FontSet FS = FontSet.Load("Font.xml");

            List<String> FilesGenerated = new List<string>();
            double LabelHeight = 1.2;
            double PadWidth = 1.8;
            double DrillWidth = 1.0;

            string basename = String.Format("Generated_This_is_not_RocketScience_prototoboard_{0}x{1}cm", width / 10.0f, height / 10.0f);

            try
            {
                Directory.CreateDirectory(basename);
            }
            catch (Exception)
            {

            }
            basename = Path.Combine(basename, String.Format("proto_{0}x{1}cm", width / 10.0f, height / 10.0f));
            string OutlineFile = basename + ".gko";
            string SilkFileTop = basename + ".gto";
            string SilkFileBottom = basename + ".gbo";
            string SolderMaskBottom = basename + ".gbs";
            string SolderMaskTop = basename + ".gts";
            string CopperBottom = basename + ".gbl";
            string CopperTop = basename + ".gtl";
            string DrillFile = basename + ".txt";


            int XE = (int)Math.Floor((width - Math.Max(edgeclearance, LabelHeight) * 2) / 2.54);
            int YE = (int)Math.Floor((height - Math.Max(edgeclearance, LabelHeight) * 2) / 2.54);

            PrepareGridSpace(XE, YE, true, true, 3, 2);

            string[][] Grid = new string[XE + 1][];
            for (int x = 0; x <= XE; x++)
            {
                Grid[x] = new string[YE + 1];
                for (int y = 0; y <= YE; y++)
                {
                    Grid[x][y] = GridSpaceType(x, y);
                    if (y == YE) Grid[x][y] = "None";
                    if (x == XE) Grid[x][y] = "None";
                }
            }

            double XO = (width / 2) - (XE / 2.0) * 2.54;
            double YO = (height / 2) - (YE / 2.0) * 2.54;

            Console.WriteLine("{0} rows and {1} columns", YE, XE);


            ExcellonFile EF = new ExcellonFile();

            ExcellonTool MountHoleDrill = new ExcellonTool();
            MountHoleDrill.Radius = mountholediameter / 2.0;


            PointD C1 = new PointD(mountholeclearance, mountholeclearance);
            PointD C2 = new PointD(width - mountholeclearance, mountholeclearance);
            PointD C3 = new PointD(width - mountholeclearance, height - mountholeclearance);
            PointD C4 = new PointD(mountholeclearance, height - mountholeclearance);
            MountHoleDrill.Drills.Add(C1);
            MountHoleDrill.Drills.Add(C2);
            MountHoleDrill.Drills.Add(C3);
            MountHoleDrill.Drills.Add(C4);

            EF.Tools[10] = MountHoleDrill;
            ExcellonTool ProtoHoleDrill = new ExcellonTool();
            ProtoHoleDrill.Radius = DrillWidth/2.0;



            PolyLine PL = new PolyLine();

            PL.Add(cornerrounding, 0);
            PL.Add(width - cornerrounding, 0);
            PL.ArcTo(new PointD(width, cornerrounding), new PointD(0, cornerrounding), InterpolationMode.CounterClockwise);
            PL.Add(width, height - cornerrounding);
            PL.ArcTo(new PointD(width - cornerrounding, height), new PointD(-cornerrounding, 0), InterpolationMode.CounterClockwise);
            PL.Add(cornerrounding, height);
            PL.ArcTo(new PointD(0, height - cornerrounding), new PointD(0, -cornerrounding), InterpolationMode.CounterClockwise);
            PL.Add(0, cornerrounding);
            PL.ArcTo(new PointD(cornerrounding, 0), new PointD(cornerrounding, 0), InterpolationMode.CounterClockwise);
            PL.Close();

            GerberArtWriter GAW = new GerberArtWriter();
            GAW.AddPolyLine(PL, 0);
            GAW.Write(OutlineFile);

            GerberArtWriter GAWTop = new GerberArtWriter();
            GerberArtWriter GAWBottom = new GerberArtWriter();

            GerberArtWriter MaskTop = new GerberArtWriter();
            GerberArtWriter MaskBottom = new GerberArtWriter();
            GerberArtWriter SilkTop = new GerberArtWriter();
            GerberArtWriter SilkBottom = new GerberArtWriter();

            for (int x = 0; x < XE; x++)
            {
                double xc = XO + x * 2.54 + 2.54 / 2;
                for (int y = 0; y < YE; y++)
                {
                    double yc = YO + y * 2.54 + 2.54 / 2;

                    PointD P = new PointD(xc, yc);
                    if ((P - C1).Length() < edgeclearance+ mountholediameter / 2 + 1.27) Grid[x][y] = "None";
                    if ((P - C2).Length() < edgeclearance + mountholediameter / 2 + 1.27) Grid[x][y] = "None";
                    if ((P - C3).Length() < edgeclearance + mountholediameter / 2 + 1.27) Grid[x][y] = "None";
                    if ((P - C4).Length() < edgeclearance + mountholediameter / 2 + 1.27) Grid[x][y] = "None";

                    if (Grid[x][y] != "None")
                    {
                        GAWTop.AddFlash(new PointD(xc, yc), PadWidth/2.0);
                        GAWBottom.AddFlash(new PointD(xc, yc), PadWidth/2.0);
                        MaskTop.AddFlash(new PointD(xc, yc), PadWidth/2.0);
                        MaskBottom.AddFlash(new PointD(xc, yc), PadWidth/2.0);
                        ProtoHoleDrill.Drills.Add(new PointD(xc, yc));
                    }
                }
            }

            //            SilkTop.DrawString(new PointD(5, 0), FS, "O0O", 5, 0.05, StringAlign.BottomCenter);
            //           SilkTop.DrawString(new PointD(20, 0), FS, "O0O", 5, 0.05, StringAlign.CenterCenter);
            //         SilkTop.DrawString(new PointD(40, 0), FS, "OMO", 5, 0.05, StringAlign.TopCenter);

            for (int x = 0; x < XE; x++)
            {
                double xc = XO + x * 2.54 + 2.54 / 2;
                double yc = YO + (0) * 2.54;
                double yc1 = YO + (YE) * 2.54 + 2.54;
                char T = (char)('A' + (char)((x) % 26));

                if (Grid[x][0] != "None")
                {
                    SilkTop.DrawString(new PointD(xc, yc), FS, T.ToString(), LabelHeight, 0.05, StringAlign.CenterCenter);
                    SilkBottom.DrawString(new PointD(xc, yc), FS, T.ToString(), LabelHeight, 0.05, StringAlign.CenterCenter, true);
                }

                if (Grid[x][YE - 1] != "None")
                {
                    SilkTop.DrawString(new PointD(xc, yc1), FS, T.ToString(), LabelHeight, 0.05, StringAlign.CenterCenter);
                    SilkBottom.DrawString(new PointD(xc, yc1), FS, T.ToString(), LabelHeight, 0.05, StringAlign.CenterCenter, true);
                }
            }
            for (int y = 0; y < YE; y++)
            {
                double xc = XO + (0) * 2.54 - 2.54 / 2;
                double xc1 = XO + (XE + 1) * 2.54 - 2.54 / 2;
                double yc = YO + (y) * 2.54 + 2.54;

                if (Grid[0][y] != "None")
                {
                    SilkTop.DrawString(new PointD(xc, yc), FS, (YE - y).ToString(), LabelHeight, 0.05, StringAlign.CenterCenter);
                    SilkBottom.DrawString(new PointD(xc, yc), FS, (YE - y).ToString(), LabelHeight, 0.05, StringAlign.CenterCenter, true);
                }

                if (Grid[XE - 1][y] != "None")
                {
                    SilkTop.DrawString(new PointD(xc1, yc), FS, (YE - y).ToString(), LabelHeight, 0.05, StringAlign.CenterCenter);
                    SilkBottom.DrawString(new PointD(xc1, yc), FS, (YE - y).ToString(), LabelHeight, 0.05, StringAlign.CenterCenter, true);

                }
            }
            for (int x = -1; x < XE + 1; x++)
            {

                for (int y = -1; y < YE + 1; y++)
                {

                    string s1 = "None";
                    if (y >= 0 && y < YE && x < XE) s1 = Grid[x + 1][y];
                    string s2 = "None";
                    if (x >= 0 && x < XE && y < YE) s2 = Grid[x][y + 1];
                    string s3 = "None";
                    if (x >= 0 && x < XE && y >= 0 && y < YE) s3 = Grid[x][y];
                    double xc = XO + x * 2.54 + 2.54 / 2;
                    double yc = YO + y * 2.54 + 2.54 / 2;
                    double xc1 = XO + (x + 1) * 2.54 + 2.54 / 2;
                    double yc1 = YO + (y + 1) * 2.54 + 2.54 / 2;

                    if (s1 == s3)
                    {
                        if (s3 != "None")
                        {
                            PolyLine PL2 = new PolyLine();
                            PL2.Add(xc, yc);
                            PL2.Add(xc1, yc);
                            GAWTop.AddPolyLine(PL2, 0.5);
                        }
                    }
                    else
                    {
                        PolyLine PL2 = new PolyLine();
                        PL2.Add(xc + 2.54 / 2, yc - 2.54 / 2);
                        PL2.Add(xc + 2.54 / 2, yc1 - 2.54 / 2);
                        SilkTop.AddPolyLine(PL2, 0.1);
                        SilkBottom.AddPolyLine(PL2, 0.1);
                    }
                    ;
                    if (s3 == s2)
                    {
                        if (s3 != "None")
                        {
                            PolyLine PL2 = new PolyLine();
                            PL2.Add(xc, yc);
                            PL2.Add(xc, yc1);
                            GAWTop.AddPolyLine(PL2, 0.5);
                        }
                    }
                    else
                    {
                        PolyLine PL2 = new PolyLine();
                        PL2.Add(xc - 2.54 / 2, yc + 2.54 / 2);
                        PL2.Add(xc1 - 2.54 / 2, yc + 2.54 / 2);
                        SilkTop.AddPolyLine(PL2, 0.1);
                        SilkBottom.AddPolyLine(PL2, 0.1);
                    }
                }
            }

            GAWTop.Write(CopperTop);

            GAWBottom.Write(CopperBottom);
            SilkBottom.Write(SilkFileBottom);
            SilkTop.Write(SilkFileTop);

            MaskTop.Write(SolderMaskTop);
            MaskBottom.Write(SolderMaskBottom);
            EF.Tools[11] = ProtoHoleDrill;
            EF.Write(DrillFile, 0, 0, 0, 0);

            FilesGenerated.Add(CopperBottom);
            FilesGenerated.Add(CopperTop);
            FilesGenerated.Add(OutlineFile);
            FilesGenerated.Add(DrillFile);
            FilesGenerated.Add(SilkFileBottom);
            FilesGenerated.Add(SilkFileTop);
            FilesGenerated.Add(SolderMaskBottom);
            FilesGenerated.Add(SolderMaskTop);


            if (WriteCombinedImage)
            {
                GerberImageCreator GIC = new GerberImageCreator();
                GIC.AddBoardsToSet(FilesGenerated);
                GIC.WriteImageFiles(basename + "_render", 200, false);
            }
            if (WriteImages)
            {
                Gerber.SaveGerberFileToImage(OutlineFile, OutlineFile + "_render.png", 1000, Color.Black, Color.White);
                Gerber.SaveGerberFileToImage(CopperBottom, CopperBottom + "_render.png", 1000, Color.Black, Color.White);
                Gerber.SaveGerberFileToImage(CopperTop, CopperTop + "_render.png", 1000, Color.Black, Color.White);
                Gerber.SaveGerberFileToImage(DrillFile, DrillFile + "_render.png", 1000, Color.Black, Color.White);
                Gerber.SaveGerberFileToImage(SilkFileTop, SilkFileTop + "_render.png", 1000, Color.Black, Color.White);
                Gerber.SaveGerberFileToImage(SilkFileBottom, SilkFileBottom + "_render.png", 1000, Color.Black, Color.White);
            }

            return FilesGenerated;
        }

        public static List<string> GridRowBaseType = new List<string>();

        public static int Xextents = 0;
        public static int Yextents = 0;
        public static bool SideRails;
        public static void PrepareGridSpace(int XE, int YE, bool SIDERAILS = true, bool GNDANDVCCBETWEENAREAS = true, int avgblocklen = 4, int blocksperarea = 2)
        {
            GridRowBaseType.Clear();
            SideRails = SIDERAILS;
            Xextents = XE;
            Yextents = YE;
            int div = blocksperarea * avgblocklen;
            if (GNDANDVCCBETWEENAREAS) div += 2;
            int TotalAreas = (int)(Math.Floor((double)YE / (double)div));
            int leftover = YE - div * TotalAreas;

            int BlocksLeft = blocksperarea / 2;
            int ItemsLeftInBlock = avgblocklen;
            int BlocksLeftInArea = blocksperarea / 2;
            int BlocksHad = 0;
            while (GridRowBaseType.Count < YE)
            {
                GridRowBaseType.Add(String.Format("B{0}", BlocksHad));
                ItemsLeftInBlock--;
                if (ItemsLeftInBlock == 0)
                {
                    ItemsLeftInBlock = avgblocklen;
                    BlocksLeft--;
                    BlocksLeftInArea--;
                    BlocksHad++;
                    if (BlocksHad == TotalAreas * blocksperarea)
                    {
                        while (GridRowBaseType.Count < YE)
                        {
                            GridRowBaseType.Add(String.Format("B{0}", BlocksHad - 1));
                        }
                    }
                    if (BlocksLeftInArea == 0)
                    {
                        if (GNDANDVCCBETWEENAREAS)
                        {
                            GridRowBaseType.Add("GND");
                            GridRowBaseType.Add("VCC1");
                        }
                        BlocksLeftInArea = blocksperarea;
                    }
                }

            }
        }


        public static string GridSpaceType(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Xextents || y >= Yextents) return "None";
            if (SideRails)
            {
                if (x == 0) return "GND";
                if (x == Xextents - 1) return "VCC1";
                if (y < 3)
                {
                    if (x < 2) return "GND";
                    if (x == Xextents - 2) return "VCC1";
                    return String.Format("HDR1_{0}", x);
                }
            }

            string R = GridRowBaseType[y];
            if (R == "GND") return R;
            if (R == "VCC1") return R;
            return R + "_" + x.ToString();

        }
    }
}
