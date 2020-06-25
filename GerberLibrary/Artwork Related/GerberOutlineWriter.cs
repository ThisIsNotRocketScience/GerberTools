using GerberLibrary;
using GerberLibrary.Core;
using GerberLibrary.Core.Primitives;
using QiHe.CodeLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

namespace GerberLibrary
{
    public class PCBWriterSet
    {
        public GerberArtWriter Outline = new GerberArtWriter();
        public GerberArtWriter TopSilk = new GerberArtWriter();
        public GerberArtWriter TopCopper = new GerberArtWriter();
        public GerberArtWriter TopSolderMask = new GerberArtWriter();

        public GerberArtWriter BottomSilk = new GerberArtWriter();
        public GerberArtWriter BottomCopper = new GerberArtWriter();
        public GerberArtWriter BottomSolderMask = new GerberArtWriter();

        public List<RectangleD> ArtExclusions = new List<RectangleD>();
        public List<PolyLine> ArtInclusions = new List<PolyLine>();

        public void AddBoardHole(PointD pos, double diameter)
        {
            PolyLine PL = new PolyLine();
            PL.MakeCircle(diameter/2.0, 20,pos.X, pos.Y);
            Outline.AddPolyLine(PL, 0);
        }

        public void CellularArt()
        {
            Bounds B = Outline.GetBounds();

            for (int y = 0; y < B.Height(); y += 1)
            {
                for (int x = 0; x < B.Width(); x += 1)
                {
                    if (InArt(x + B.TopLeft.X + 0.5, y + B.TopLeft.Y + 0.5, 0.5))
                    {
                        if (((x + y) % 2) == 0)
                        {
                            PolyLine P = new PolyLine();
                            P.MakeRectangle(0.7, 0.7, x + B.TopLeft.X + 0.5, y + B.TopLeft.Y+0.5);
                            TopSilk.AddPolygon(P);
                        }


                    }
                }
            }
        }

        private bool InArt(double x, double y, double surrounds = 0)
        {
            if (surrounds > 0)
            {
                if (InArt(x - surrounds, y - surrounds) == false) return false;
                if (InArt(x + surrounds, y - surrounds) == false) return false;
                if (InArt(x - surrounds, y + surrounds) == false) return false;
                if (InArt(x + surrounds, y + surrounds) == false) return false;


            }

            foreach (var r in ArtExclusions)
            {
                if (r.ContainsPoint(x, y)) return false;
            }
            bool contained = false;
            foreach (var a in ArtInclusions)
            {
                if (a.ContainsPoint(x, y))
                {
                    contained = true;
                }
            }
            return contained;
        }

        public Dictionary<double, List<PointD>> DrillHoles = new Dictionary<double, List<PointD>>();

        public void Drill(PointD pos, double Diameter, double soldermaskclearance = 0)
        {
            if (DrillHoles.ContainsKey(Diameter) == false)
            {
                DrillHoles[Diameter] = new List<PointD>();
            }
            DrillHoles[Diameter].Add(pos);


            if (soldermaskclearance > 0)
            {
                TopSolderMask.AddFlash(pos, Diameter / 2.0 + soldermaskclearance);
                BottomSolderMask.AddFlash(pos, Diameter / 2.0 + soldermaskclearance);
            }


        }
        public List<string> Write(string targetfolder, string basename, PointD offset = null)
        {
            if (offset == null)
            {
                offset = new PointD(0, 0);
            }

            List<string> Files = new List<string>();
            ExcellonFile EF = new ExcellonFile();
            int drillcount = 0;
            foreach (var h in DrillHoles)
            {


                ExcellonTool DrillBit = new ExcellonTool();
                DrillBit.Radius = h.Key / 2.0f;

                foreach (var a in h.Value)
                {
                    DrillBit.Drills.Add(a);
                }
                EF.Tools[10 + drillcount] = DrillBit;
                drillcount++;

            }
            string DrillFile = Path.Combine(targetfolder, basename + ".txt");

            EF.Write(DrillFile, offset.X, offset.Y, 0, 0);
            Files.Add(DrillFile);

            string OutName = Path.Combine(targetfolder, basename);

            Outline.Write(OutName + ".gko", offset); Files.Add(OutName + ".gko");
            TopSilk.Write(OutName + ".gto", offset); Files.Add(OutName + ".gto");
            TopCopper.Write(OutName + ".gtl", offset); Files.Add(OutName + ".gtl");

            TopSolderMask.Write(OutName + ".gts", offset); Files.Add(OutName + ".gts");

            BottomSilk.Write(OutName + ".gbo", offset); Files.Add(OutName + ".gbo");
            BottomCopper.Write(OutName + ".gbl", offset); Files.Add(OutName + ".gbl");
            BottomSolderMask.Write(OutName + ".gbs", offset); Files.Add(OutName + ".gbs");


            return Files;

        }

        public void AddOutline(PolyLine pL)
        {
            Outline.AddPolyLine(pL, 0);
        }

        public void Fiducial(PointD pos, double copperdiameter = 1.0, double maskdiameter = 2.0, bool top = true)
        {
            if (top)
            {
                TopCopper.AddFlash(pos, copperdiameter / 2.0);
                TopSolderMask.AddFlash(pos, maskdiameter / 2.0);
            }
            else
            {
                BottomCopper.AddFlash(pos, copperdiameter / 2.0);
                BottomSolderMask.AddFlash(pos, maskdiameter / 2.0);
            }

        }

        public void Label(FontSet fnt, PointD pos, string txt, double size, StringAlign alignment = StringAlign.CenterCenter, double strokewidth = 0.1, bool silk = true, bool copper = false, double angle = 0, bool backside = false)
        {

            if (!backside)
            {
                ArtExclusions.Add(new RectangleD(TopSilk.MeasureString(pos, fnt, txt, size, strokewidth, alignment, backside, angle)));
                if (silk) TopSilk.DrawString(pos, fnt, txt, size, strokewidth, alignment, backside, angle);
                if (copper) TopCopper.DrawString(pos, fnt, txt, size, strokewidth, alignment, backside, angle);
            }
            else
            {
                ArtExclusions.Add(new RectangleD(BottomSilk.MeasureString(pos, fnt, txt, size, strokewidth, alignment, backside, angle)));
                if (silk) BottomSilk.DrawString(pos, fnt, txt, size, strokewidth, alignment, backside, angle);
                if (copper) BottomCopper.DrawString(pos, fnt, txt, size, strokewidth, alignment, backside, angle);
            }
        }
    }

    public class GerberFrameWriter
    {
        public class FrameSettings
        {
            public class Fiducial
            {
                public PointD pos = new PointD();
                public bool Top = true;
                public double CopperDiameter = 1;
                public double MaskDiameter = 2;

            }

            public void PositionAround(PolyLine pL)
            {
                var B = pL.GetBounds();
                offset = new PointD(B.TopLeft.X - leftEdge - margin, B.TopLeft.Y - topEdge - margin);
                innerWidth = B.Width() + margin * 2;
                innerHeight = B.Height() + margin * 2;

            }

            public bool RenderSample = false;
            public string FrameTitle = "";
            public double innerWidth;
            public double innerHeight;
            public PointD offset = new PointD(0,0);

            public double topEdge = 5;
            public double leftEdge = 5;
            public double holeDiameter = 3.2;
            public bool addHoles = true;
            public double roundedInnerCorners = 1.0;
            public double roundedOuterCorners = 2.0;
            public List<Fiducial> fiducialsListData = new List<Fiducial>();
            public double margin = 3;

            public int sideholes = 6;
            public double mmbetweentabs = 40;
        }

        public static void MergeFrameIntoGerberSet(string FrameFolder, string OutlineFolder, string OutputFolder, FrameSettings FS, ProgressLog log, string basename)
        {
            log.PushActivity("MergeFrame");
            GerberPanel PNL = new GerberPanel();
            PNL.AddGerberFolder(FrameFolder);
            PNL.AddGerberFolder(OutlineFolder);
            PNL.TheSet.ClipToOutlines = false;



            var FrameInstance = PNL.AddInstance(FrameFolder, new PointD(0, 0));
            var OutlineInstance = PNL.AddInstance(OutlineFolder, new PointD(0, 0));

            PNL.UpdateShape();

            var BB = OutlineInstance.BoundingBox;

            foreach (var s in OutlineInstance.TransformedOutlines)
            {
                bool ClockWise = s.ClockWise();
                
                if (s.Vertices.Count >= 2)
                {
                    for (int i = 0; i < s.Vertices.Count; i++)
                    {
                        PointD p1 = s.Vertices[i];
                        PointD p2 = s.Vertices[(i + 1) % s.Vertices.Count];

                        var D = p2 - p1;
                        if (Math.Abs(D.X)<0.5 && Math.Abs(D.Y) >= FS.mmbetweentabs)
                        {
                            // perfect vertical! 
                            log.AddString(String.Format("vertical found: {0} -> {1}", p1, p2));

                            double dy = p2.Y - p1.Y;

                            double x = 0;
                            double rad = 0;
                            bool rightside = (dy > 0);
                            if (ClockWise) rightside = !rightside;

                            if (rightside)
                            {
                                x = (p1.X + (BB.BottomRight.X + FS.margin)) / 2;
                                rad = Math.Abs((p1.X - (BB.BottomRight.X + FS.margin))) / 2.0 + FS.margin;
                            }
                            else
                            {
                                x = (p1.X + (BB.TopLeft.X - FS.margin)) / 2;
                                rad = Math.Abs((p1.X - (BB.TopLeft.X - FS.margin))) / 2.0 + FS.margin;
                            }


                            int tabs = (int)Math.Floor(Math.Abs(dy) / FS.mmbetweentabs);
                            for (int j = 0; j < tabs; j++)
                            {
                                double y = p1.Y + (dy/(float)tabs) * (j + 0.5);
                                var BR = PNL.AddTab(new PointD(x,y));
                                log.AddString(String.Format("tab at {0} - radius {1}", BR.Center, rad));
                                BR.Radius = (float)rad;
                            }
                        }
                    }
                }
            }
          

            PNL.UpdateShape();

            Directory.CreateDirectory(OutputFolder);
            PNL.SaveGerbersToFolder("MergedFrame", OutputFolder, log, true, false, true, basename);

            log.PopActivity();

//            PNL.SaveOutlineTo(OutputFolder, "mergedframeblended");
            return ;



            var FrameFiles = Directory.GetFiles(FrameFolder);
            var OutlineFiles = Directory.GetFiles(OutlineFolder);
            List<String> AllFiles = new List<string>();
            AllFiles.AddRange(FrameFiles);
            foreach(var a in OutlineFiles)
            {
                BoardLayer layer;
                BoardSide Side;

                Gerber.DetermineBoardSideAndLayer(a, out Side, out layer);

                if (layer != BoardLayer.Outline)
                {
                    AllFiles.Add(a);
                }

            }

          //  AllFiles.AddRange(OutlineFiles);
            GerberMerger.MergeAllByFileType(AllFiles, OutputFolder,"MergedFrame", log);

        }


        public static List<string> WriteSideEdgeFrame(PolyLine pl, FrameSettings FS, string basefile, bool insideedge = true)
        {
            List<string> Files = new List<string>();

            try
            {

                int polyid = 0;
                string fname = System.IO.Path.GetFileName(basefile);
                string fnamenoext = System.IO.Path.GetFileNameWithoutExtension(basefile);
                string OutName = Path.Combine(System.IO.Path.GetDirectoryName(basefile),  fnamenoext);
                Console.WriteLine("writing frame files to {0}", OutName);
                Bounds B = new Bounds();
                B.AddPolyLine(pl);
//                FS.innerHeight = B.Height();
  //              FS.innerWidth = B.Width();

                
                double W = (double)FS.innerWidth;
                double H = (double)FS.innerHeight;
                double TE = (double)FS.topEdge;
                double LE = (double)FS.leftEdge;

                double OuterWidth = W + LE * 2.0;
                double OuterHeight = H + TE * 2.0;
                double InnerWidth = W;
                double InnerHeight = H;


                PCBWriterSet PCB = new PCBWriterSet();

                double mountholediameter = (double)FS.holeDiameter;

                if (FS.addHoles)
                {
                    double side = LE / 2.0;
                    double top = TE / 2.0;

                    PCB.Drill(new PointD(side, top), mountholediameter,1);
                    PCB.Drill(new PointD(OuterWidth - side, top), mountholediameter,1);
                    PCB.Drill(new PointD(OuterWidth - side, OuterHeight - top), mountholediameter,1);
                    PCB.Drill(new PointD(side, OuterHeight - top), mountholediameter,1);
                }

                // board outline
                PolyLine PL = new PolyLine(polyid++);
                PL.MakeRoundedRect(new PointD(0, 0), new PointD(OuterWidth, OuterHeight), (double)FS.roundedOuterCorners);
                PCB.AddOutline(PL);

                if (insideedge)
                {
                    PolyLine PL2 = new PolyLine(polyid++);
                    PL2.MakeRoundedRect(new PointD(LE, TE), new PointD(InnerWidth + LE, InnerHeight + TE), (double)FS.roundedInnerCorners);
                    PCB.AddOutline(PL2);
                }

                #region fiducials

                List<PointD> Fiducials = new List<PointD>();

                foreach (var P in FS.fiducialsListData)
                {
                    PointD fiducialPoint = P.pos;
                    PCB.Fiducial(fiducialPoint, P.CopperDiameter, P.MaskDiameter, P.Top);

                }

                #endregion

                string FrameTitle = FS.FrameTitle;

                if (FrameTitle.Length > 0)
                {
                    FontSet fnt = FontSet.Load("Font.xml");

                    PCB.Label(fnt, new PointD(OuterWidth / 2.0, OuterHeight - TE / 4.0), FrameTitle + " - top", TE/4, StringAlign.CenterCenter,0.1, true,true,0);
                    PCB.Label(fnt, new PointD(OuterWidth - LE / 4, OuterHeight / 2), FrameTitle + " - top", TE / 4.0, StringAlign.CenterCenter,0.1, true, true,-90);
                    PCB.Label(fnt, new PointD(LE / 4, OuterHeight / 2), FrameTitle + " - top", TE / 4.0, StringAlign.CenterCenter, 0.1, true,true, 90);
                    PCB.Label(fnt, new PointD(OuterWidth / 2.0, TE - TE / 4.0), FrameTitle + " - top", TE / 4.0, StringAlign.CenterCenter, 0.1, true,true,0);


                    PCB.Label(fnt, new PointD(OuterWidth / 2.0, OuterHeight - TE / 4.0), FrameTitle + " - bottom", TE / 4, StringAlign.CenterCenter, 0.1, true,true,0, true);
                    PCB.Label(fnt, new PointD(OuterWidth - LE / 4, OuterHeight / 2), FrameTitle + " - bottom", TE / 4.0, StringAlign.CenterCenter, 0.1, true, true, -90,true);
                    PCB.Label(fnt, new PointD(LE / 4, OuterHeight / 2), FrameTitle + " - bottom", TE / 4.0, StringAlign.CenterCenter, 0.1, true, true, 90,true);
                    PCB.Label(fnt, new PointD(OuterWidth / 2.0, TE - TE / 4.0), FrameTitle + " - bottom", TE / 4.0, StringAlign.CenterCenter, 0.1, true, true, 0,true);
                }

                PolyLine Left = new PolyLine();
                PolyLine Right = new PolyLine();
                PolyLine Top = new PolyLine();
                PolyLine Bottom = new PolyLine();


                Top.MakePRectangle(OuterWidth+1, FS.topEdge+1, -1, -1);
                Bottom.MakePRectangle(OuterWidth, FS.topEdge, 0, OuterHeight-FS.topEdge);
                Left.MakePRectangle(FS.leftEdge+1, OuterHeight+1, -1, -1);
                Right.MakePRectangle(FS.leftEdge, OuterHeight, OuterWidth-FS.leftEdge, 0);
                PCB.ArtInclusions.Add(Left);
                PCB.ArtInclusions.Add(Right);
                PCB.ArtInclusions.Add(Top);
                PCB.ArtInclusions.Add(Bottom);

                PCB.CellularArt();


                Files.AddRange(PCB.Write(Path.GetDirectoryName(basefile), Path.GetFileNameWithoutExtension(basefile),FS.offset)); ;



                if (FS.RenderSample)
                {
                    GerberImageCreator GIC = new GerberImageCreator();
                    GIC.AddBoardsToSet(Files, new SilentLog());
                    GIC.WriteImageFiles(OutName, 200, true, false, true, null);
                }
            }
            catch(Exception e)
            {
                Console.Write(" Exception while making frames:{0}", e.Message);
            }
            foreach (var s in Files)
            {
                Console.WriteLine("Writen edge file {0} succesfully", s);
            }
            return Files;
        }
    }

    public class GerberOutlineWriter
    {
        List<PolyLine> PolyLines = new List<PolyLine>();
        public void AddPolyLine(PolyLine a)
        {
            PolyLines.Add(a);
        }

        public void Write(string p, double inWidth = 0.0)
        {
            List<string> lines = new List<string>();

            GerberNumberFormat GNF = new GerberNumberFormat();
            GNF.DigitsBefore = 3;
            GNF.DigitsAfter = 6;
            GNF.SetImperialMode();

            lines.Add(Gerber.INCH);
            lines.Add("%OFA0B0*%");
            lines.Add(GNF.BuildGerberFormatLine());
            lines.Add("%IPPOS*%");
            lines.Add("%LPD*%");
            GerberApertureType Apt = new GerberApertureType();
            Apt.SetCircle(inWidth);
            Apt.ID = 10;
            lines.Add(Apt.BuildGerber(GNF));
            lines.Add(Apt.BuildSelectGerber());
            foreach (var a in PolyLines)
            {
                lines.Add(Gerber.MoveTo(a.Vertices[a.Vertices.Count - 1], GNF));
                for (int i = 0; i < a.Vertices.Count; i++)
                {
                    lines.Add(Gerber.LineTo(a.Vertices[i], GNF));
                }
            }
            lines.Add(Gerber.EOF);
            Gerber.WriteAllLines(p, lines);
        }
    }

}
