using GerberLibrary;
using GerberLibrary.Core;
using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntennaBuilder
{
    class NFCAntennaBuilder
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
            NFCAntennaBuilder NA = new NFCAntennaBuilder();
            NA.WriteAntennaBoard(width, height, mounthole, rounding, clearance);
        }

        GerberArtWriter GAW = new GerberArtWriter();

        GerberArtWriter GAWTop = new GerberArtWriter();
        GerberArtWriter GAWBottom = new GerberArtWriter();

        GerberArtWriter MaskTop = new GerberArtWriter();
        GerberArtWriter MaskBottom = new GerberArtWriter();
        GerberArtWriter SilkTop = new GerberArtWriter();
        GerberArtWriter SilkBottom = new GerberArtWriter();


        public void AddAntennaVia(double x, double y)
        {
            GAWBottom.AddFlash(new PointD(x,y), 0.6);
            GAWTop.AddFlash(new PointD(x,y), 0.6);
            ViaDrill.Drills.Add(new PointD(x,y));
        }

        ExcellonTool ViaDrill = new ExcellonTool();
        double antennatracewidth = 1.75;
            

        public void WriteAntennaBoard(double width, double height, double mountholediameter, double cornerrounding, double mountholeclearance, double edgeclearance = 1.26, bool WriteCombinedImage = true, bool WriteImages = false)
        {
            int polyid = 0;
            FontSet FS = FontSet.Load("Font.xml");

            List<String> FilesGenerated = new List<string>();
            double LabelHeight = 1.2;
            double PadWidth = 1.8;
            double DrillWidth = 1.0;

            string basename = String.Format("Generated_This_is_not_RocketScience_Antenna_{0}x{1}cm", width / 10.0f, height / 10.0f);

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

            ViaDrill.Radius = 0.6 / 2.0;

            EF.Tools[11] = ViaDrill;


            ExcellonTool ProtoHoleDrill = new ExcellonTool();
            ProtoHoleDrill.Radius = DrillWidth / 2.0;

            PointD pC1 = new PointD(mountholeclearance, height/2 - 2.54);
            PointD pC2 = new PointD(mountholeclearance, height / 2 );
            PointD pC3 = new PointD(mountholeclearance, height / 2 + 2.54);

            ProtoHoleDrill.Drills.Add(pC1);
            ProtoHoleDrill.Drills.Add(pC2);
            ProtoHoleDrill.Drills.Add(pC3);

            MaskBottom.AddFlash(pC1, 1);
            MaskBottom.AddFlash(pC2, 1);
            MaskBottom.AddFlash(pC3, 1);
            MaskTop.AddFlash(pC1, 1);
            MaskTop.AddFlash(pC2, 1);
            MaskTop.AddFlash(pC3, 1);

            PolyLine PL = new PolyLine(polyid++);

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

            GAW.AddPolyLine(PL, 0);
            GAW.Write(OutlineFile);
           
            double AX = C1.X +  mountholeclearance;
            double AY = C1.Y +  mountholeclearance;
            double gap = 5;
            
            
            double W = width - AX * 2;
            double H = height -AY*2 - gap;

            int rounds = 6;
            
            antennatracewidth = 0.3/2;
            
            double spacing = 0.8;
            
            for (int i = 0; i < rounds; i++)
            {
                double W2 = W - i * spacing  * 2;
                double H2 = H - i * spacing *  2;
                
                double W3 = W - (i+1) * spacing * 2;
                double H3 = H - (i+1) * spacing * 2;

                double AX2 = AX + i * spacing ;
                double AY2 = AY + i * spacing ;
        
                double AX3 = AX + (i+1) * spacing;
                double AY3 = AY + (i+1) * spacing;
                
                PolyLine PL2 = new PolyLine(polyid++);

                PL2.Add(AX2, AY2 + H2/2);
                PL2.Add(AX2, AY2);
                PL2.Add(AX2 + W2, AY2);
                PL2.Add(AX2 + W2, AY2 + H2/2);
                AddAntennaTop(PL2);

                
                PolyLine PL3 = new PolyLine(polyid++);

                PL3.Add(AX2,AY2 + gap + H2 - ( H2 / 2) );
                PL3.Add(AX2, AY2 + gap + H2);
                PL3.Add(AX2 + W2, AY2 + gap + H2);
                PL3.Add(AX2 + W2, AY2 + gap + H2 - (H2 / 2));
                AddAntennaTop(PL3);
                double gs = 2;
                
                if (i % 2 == 1)
                {
                    if (i < rounds - 1)
                    {
                        PolyLine PL4 = new PolyLine(polyid++);
                        PL4.Add(AX2, AY2 + gap + H2 - (H2 / 2));
                        PL4.Add(AX2, AY2 + gap - gs + H2 - (H2 / 2));
                        PL4.Add(AX3, AY3 + gs + (H3 / 2));
                        PL4.Add(AX3, AY3 + (H3 / 2));
                        AddAntennaTop(PL4);
                        
                        PolyLine PL5 = new PolyLine(polyid++);
                        PL5.Add(AX3, AY3 + gap + H3 - (H3 / 2));
                        PL5.Add(AX2, AY2 + (H2 / 2));
                        AddAntennaBottom(PL5);

                        AddAntennaVia(AX3, AY3 + gap + H3 - (H3 / 2));
                        AddAntennaVia(AX2, AY2 + (H2 / 2));



                    }
                    else
                    {
                        PolyLine PL4 = new PolyLine(polyid++);
                        PL4.Add(AX2, AY2 + gap + H2  - (H2 / 2));
                        PL4.Add(AX2, AY2 + (H2 / 2));
                        AddAntennaTop(PL4);
                        
                        AddAntennaVia(AX2, AY2 + H2/2 +  gap / 2);

                        PolyLine PL5 = new PolyLine(polyid++);
                        PL5.Add(AX2, AY2 + H2 / 2 + gap / 2);
                        PL5.Add(AX2, AY2 + H2 / 2 - gs);
                        PL5.Add(AX - spacing , AY2 + H2 / 2 - gs);
                        PL5.Add(AX - spacing, AY2 + H2 / 2 + gap/2);

                        AddAntennaVia(AX - spacing, AY2 + H2 / 2 + gap / 2);

                        AddAntennaBottom(PL5);
                    } 
                }
                else
                {
                    PolyLine PL4 = new PolyLine(polyid++);
                    PL4.Add(AX2 + W2 , AY2 + gap + H2 - (H2 / 2));
                    PL4.Add(AX2 + W2 , AY2 + gap - gs + H2 - (H2 / 2));
                    PL4.Add(AX3 + W3, AY3 + gs + (H3 / 2));
                    PL4.Add(AX3 + W3, AY3 + (H3 / 2));
                    AddAntennaTop(PL4);

                    PolyLine PL5 = new PolyLine(polyid++);
                    PL5.Add(AX3 + W3, AY3 + gap + H3 - (H3 / 2));
                    PL5.Add(AX2 + W2, AY2 + (H2 / 2));

                    AddAntennaBottom(PL5);

                    PolyLine PL6 = new PolyLine(polyid++);
                    PL6.Add(AX, AY + H / 2);
                    PL6.Add(pC1.X, pC1.Y);
                    AddAntennaTop(PL6);

                    PolyLine PL7 = new PolyLine(polyid++);
                    PL7.Add(AX, AY + H / 2 + gap);
                    PL7.Add(pC3.X, pC3.Y);
                    AddAntennaTop(PL7);
                    
                    PolyLine PL8 = new PolyLine(polyid++);
                    PL8.Add(AX - spacing, AY2 + H2 / 2 + gap / 2);
                    PL8.Add(pC2.X, pC2.Y);
                    AddAntennaTop(PL8);


                    AddAntennaVia(AX3 + W3, AY3 + gap + H3 - (H3 / 2));
                    AddAntennaVia(AX2 + W2, AY2 + (H2 / 2));



                }

            }


            string R ="NFC antenna: " +  AntennaLen.ToString("N2") + " mm";
           // MaskTop.DrawString(new PointD(width / 2, 5), FS, R, LabelHeight, 0.05, StringAlign.CenterCenter);
           // MaskBottom.DrawString(new PointD(width / 2, 5), FS, R, LabelHeight, 0.05, StringAlign.CenterCenter, true);
            SilkTop.DrawString(new PointD(width / 2, 5), FS, R, LabelHeight, 0.05, StringAlign.CenterCenter);
            SilkBottom.DrawString(new PointD(width/2, 5), FS, R, LabelHeight, 0.05, StringAlign.CenterCenter, true);
              
            
            GAWTop.Write(CopperTop);

            GAWBottom.Write(CopperBottom);
            SilkBottom.Write(SilkFileBottom);
            SilkTop.Write(SilkFileTop);

            MaskTop.Write(SolderMaskTop);
            MaskBottom.Write(SolderMaskBottom);
            EF.Tools[12] = ProtoHoleDrill;
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
                GIC.AddBoardsToSet(FilesGenerated, new StandardConsoleLog());
                GIC.WriteImageFiles(basename + "_render", 200, false);
            }
            if (WriteImages)
            {
                Gerber.SaveGerberFileToImage(new StandardConsoleLog(),OutlineFile, OutlineFile + "_render.png", 1000, Color.Black, Color.White);
                Gerber.SaveGerberFileToImage(new StandardConsoleLog(),CopperBottom, CopperBottom + "_render.png", 1000, Color.Black, Color.White);
                Gerber.SaveGerberFileToImage(new StandardConsoleLog(),CopperTop, CopperTop + "_render.png", 1000, Color.Black, Color.White);
                Gerber.SaveGerberFileToImage(new StandardConsoleLog(),DrillFile, DrillFile + "_render.png", 1000, Color.Black, Color.White);
                Gerber.SaveGerberFileToImage(new StandardConsoleLog(),SilkFileTop, SilkFileTop + "_render.png", 1000, Color.Black, Color.White);
                Gerber.SaveGerberFileToImage(new StandardConsoleLog(),SilkFileBottom, SilkFileBottom + "_render.png", 1000, Color.Black, Color.White);
            }

        }

        private void AddAntennaTop(PolyLine PL2)
        {
            GAWTop.AddPolyLine(PL2, antennatracewidth);
            AddLength(PL2);

        }

        private void AddAntennaBottom(PolyLine PL3)
        {
            GAWBottom.AddPolyLine(PL3, antennatracewidth);
            AddLength(PL3);

        }
        double AntennaLen = 0;
        private void AddLength(PolyLine PL3)
        {

            for (int i = 0; i < PL3.Vertices.Count - 1; i++)
            {
                var A = PL3.Vertices[i];
                var B = PL3.Vertices[i + 1];

                double DX = A.X - B.X;
                double DY = A.Y - B.Y;

                AntennaLen += Math.Sqrt(DX * DX + DY * DY);

            }
        }
    }
}
