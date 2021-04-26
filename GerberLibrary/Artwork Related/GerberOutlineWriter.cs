using GerberLibrary;
using GerberLibrary.Core;
using GerberLibrary.Core.Primitives;
//using QiHe.CodeLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
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

        public Dictionary<BoardSide, Dictionary<BoardLayer, List<GerberArtWriter>>> AllLayers = new Dictionary<BoardSide, Dictionary<BoardLayer, List<GerberArtWriter>>>();
        public PCBWriterSet()
        {
            AllLayers[BoardSide.Both] = new Dictionary<BoardLayer, List<GerberArtWriter>>();
            AllLayers[BoardSide.Bottom] = new Dictionary<BoardLayer, List<GerberArtWriter>>();
            AllLayers[BoardSide.Top] = new Dictionary<BoardLayer, List<GerberArtWriter>>();
            AllLayers[BoardSide.Both][BoardLayer.Outline] = new List<GerberArtWriter>() { Outline };
            AllLayers[BoardSide.Both][BoardLayer.Silk] = new List<GerberArtWriter>() { TopSilk, BottomSilk };
            AllLayers[BoardSide.Both][BoardLayer.Copper] = new List<GerberArtWriter>() { TopCopper, BottomCopper };
            AllLayers[BoardSide.Both][BoardLayer.SolderMask] = new List<GerberArtWriter>() { TopSolderMask, BottomSolderMask };

            AllLayers[BoardSide.Top][BoardLayer.Silk] = new List<GerberArtWriter>() { TopSilk };
            AllLayers[BoardSide.Top][BoardLayer.Copper] = new List<GerberArtWriter>() { TopCopper };
            AllLayers[BoardSide.Top][BoardLayer.SolderMask] = new List<GerberArtWriter>() { TopSolderMask };
            AllLayers[BoardSide.Bottom][BoardLayer.Silk] = new List<GerberArtWriter>() { BottomSilk };
            AllLayers[BoardSide.Bottom][BoardLayer.Copper] = new List<GerberArtWriter>() { BottomCopper };
            AllLayers[BoardSide.Bottom][BoardLayer.SolderMask] = new List<GerberArtWriter>() { BottomSolderMask };

        }
        public List<RectangleD> ArtExclusions = new List<RectangleD>();
        public List<PolyLine> ArtInclusions = new List<PolyLine>();

        public void AddBoardHole(PointD pos, double diameter)
        {
            PolyLine PL = new PolyLine();
            PL.MakeCircle(diameter / 2.0, 20, pos.X, pos.Y);
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
                            P.MakeRectangle(0.7, 0.7, x + B.TopLeft.X + 0.5, y + B.TopLeft.Y + 0.5);
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
        public List<string> Write(string targetfolder, string basename,BOM inFiducialBom, PointD offset = null  )
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

            BOM FiducialBom;
            if (inFiducialBom != null) FiducialBom = inFiducialBom;  else FiducialBom = new BOM();

            BOMNumberSet set = new BOMNumberSet();
            int fd = 1;

            foreach(var a in Fiducials)
            {
                FiducialBom.AddBOMItemExt("FIDUCIAL_" + a.Style.ToString(), "FIDUCIAL_" + a.Style.ToString(), a.Style.ToString(), "__FD" + (fd.ToString()), set, "Frame_" + basename, a.Pos.X + offset.X, a.Pos.Y+offset.Y, 0, a.Side);
            }
            FiducialBom.WriteJLCCSV(targetfolder, basename + "_fiducials", false);
            
            return Files;
        }

        public void AddOutline(PolyLine pL)
        {
            Outline.AddPolyLine(pL, 0);
        }
        
        public enum FiducialStyle
        {
            Round,
            Square
        }

        class FiducialPlacement
        {
            public PointD Pos;
            public FiducialStyle Style;
            public BoardSide Side;
        }

        List<FiducialPlacement> Fiducials = new List<FiducialPlacement>();

        public void Fiducial(PointD pos, double copperdiameter = 1.0, double maskdiameter = 2.0, BoardSide fiducialSide = BoardSide.Top, FiducialStyle style = FiducialStyle.Square)
        {
            var coppers = GetGAW(fiducialSide, BoardLayer.Copper);
            var mask = GetGAW(fiducialSide, BoardLayer.SolderMask);
            var silk = GetGAW(fiducialSide, BoardLayer.Silk);
            Fiducials.Add(new FiducialPlacement() { Pos = pos, Style = style, Side = fiducialSide });
            switch (style)
            {

                case FiducialStyle.Round:
                    {
                        PolyLine Circle = new PolyLine();
                        Circle.MakeCircle((maskdiameter + 1) / 2, 20, pos.X, pos.Y);
                        foreach (var c in coppers) c.AddFlash(pos, copperdiameter / 2.0);
                        foreach (var sm in mask) sm.AddFlash(pos, maskdiameter / 2.0);
                        foreach (var s in silk) s.AddPolyLine(Circle, 0.1);
                    }
                    break;
                case FiducialStyle.Square:
                    {
                        PolyLine SmallSquare = new PolyLine();
                        SmallSquare.MakeRectangle(copperdiameter, copperdiameter, pos.X, pos.Y);
                        PolyLine BiggerSquare = new PolyLine();
                        BiggerSquare.MakeRectangle(maskdiameter, maskdiameter, pos.X, pos.Y);
                        PolyLine BiggestSquare = new PolyLine();
                        BiggestSquare.MakeRectangle(maskdiameter + 1, maskdiameter + 1, pos.X, pos.Y);

                        foreach (var c in coppers) c.AddPolygon(SmallSquare);
                        foreach (var sm in mask) sm.AddPolygon(BiggerSquare);
                        foreach (var s in silk) s.AddPolyLine(BiggestSquare, 0.1);
                    }
                    break;
            }


        }

        public static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public RectangleD Identicon(PointD center, double size, string thingtohash, bool top = true)
        {
            size *= 2;
            var H = GetHash(thingtohash);

            Int32 r = 0;
            for (int i = 0; i < Math.Min(4, H.Length); i++)
            {
                r += (int)H[i] << (int)(i * 8);
            }

            for (int ix = 0; ix < 4; ix++)
            {
                for (int iy = 0; iy < 4; iy++)
                {
                    int b = ix + iy * 4;
                    bool fill = ((r >> b) & 0x1) > 0 ? true : false;
                    if (fill)
                    {
                        PolyLine P1 = new PolyLine();
                        P1.MakePRectangle(size / 8, size / 8, (ix - 3.5) * size / 8 + center.X, (iy - 3.5) * size / 8 + center.Y);
                        TopSilk.AddPolyLine(P1, 0.01);
                    }
                }
            }

            return new RectangleD() { X = center.X - size / 2, Y = center.Y - size / 2, Height = size, Width = size };
        }
        public void Label(FontSet fnt, PointD pos, string txt, double size, StringAlign alignment = StringAlign.CenterCenter, double strokewidth = 0.1, bool silk = true, bool copper = false, double angle = 0, bool backside = false, bool identiconstart = false, bool identiconend = false)
        {

            var therect = new RectangleD(TopSilk.MeasureString(pos, fnt, txt, size, strokewidth, alignment, backside, angle));

            if (!backside)
            {

                ArtExclusions.Add(new RectangleD(TopSilk.MeasureString(pos, fnt, txt, size, strokewidth, alignment, backside, angle)));

                //                ArtExclusions.Add(Identicon(new PointD(therect.X - size * 0.7, therect.Y + therect.Height / 2), size, txt, true));



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

        internal void Arrow(PointD pointD1, PointD pointD2, BoardSide side, BoardLayer layer = BoardLayer.Silk)
        {
            foreach (var L in GetGAW(side, layer))
            {
                L.Arrow(pointD1, pointD2);
            }
        }

        private List<GerberArtWriter> GetGAW(BoardSide side, BoardLayer layer)
        {
            List<GerberArtWriter> R = new List<GerberArtWriter>();
            if (AllLayers.ContainsKey(side))
            {
                var CurrentLayer = AllLayers[side];
                if (CurrentLayer.ContainsKey(layer))
                {
                    foreach (var l in CurrentLayer[layer])
                    {
                        R.Add(l);
                    }

                }
            }

            return R;
        }
    }

    public class GerberFrameWriter
    {
        public class FrameSettings
        {

            public enum InsideMode
            {
                NoEdge,
                RegularEdge,
                FormFitting
            }

            public InsideMode InsideEdgeMode =  InsideMode.RegularEdge;
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
                PositionAround(B);
            }

            public void PositionAround(Bounds B)
            {
                offset = new PointD(B.TopLeft.X - leftEdge - margin, B.TopLeft.Y - topEdge - margin);
                innerWidth = B.Width() + margin * 2;
                innerHeight = B.Height() + margin * 2;
            }

            public bool RenderDirectionArrow = true;
            public bool DefaultFiducials = true;
            public BoardSide DirectionArrowSide = BoardSide.Top; // only top or bottom

            public bool RenderSample = false;
            public string FrameTitle = "";
            public double innerWidth;
            public double innerHeight;
            public PointD offset = new PointD(0, 0);


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

            public double MinMountHoleSpacingMM = 10;

            public double MaxMountHoleSpacingMM = 40;
            public BoardSide FiducialSide = BoardSide.Top;
            public bool VerticalTabs = true;
            public bool HorizontalTabs = false;
        }

        public static void MergeFrameIntoGerberSet(string FrameFolder, string OutlineFolder, string OutputFolder, FrameSettings FS, ProgressLog log, string basename)
        {


            log.PushActivity("MergeFrame");
           // log.AddString(".....");
            if (Directory.Exists(FrameFolder) == false) log.AddString(String.Format("Framefolder {0} does not exist?", FrameFolder));
            if (Directory.Exists(OutlineFolder) == false) log.AddString(String.Format("OutlineFolder {0} does not exist?", OutlineFolder));
            if (Directory.Exists(OutputFolder) == false) log.AddString(String.Format("OutputFolder {0} does not exist?", OutputFolder));
            GerberPanel PNL = new GerberPanel();
            PNL.AddGerberFolder(log, FrameFolder);
            PNL.AddGerberFolder(log, OutlineFolder);
            PNL.TheSet.ClipToOutlines = false;



            var FrameInstance = PNL.AddInstance(FrameFolder, new PointD(0, 0));
            var OutlineInstance = PNL.AddInstance(OutlineFolder, new PointD(0, 0));

            PNL.UpdateShape(log);

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
                        if (Math.Abs(D.X) < 0.5 && Math.Abs(D.Y) >= FS.mmbetweentabs && FS.VerticalTabs)
                        {
                            // perfect vertical! 
                            log.AddString(String.Format("vertical found: {0} -> {1}", p1, p2));

                            double dy = p2.Y - p1.Y;

                            double x = 0;
                            double rad = 0;
                            bool rightside = (dy > 0);
                            if (ClockWise) rightside = !rightside;

                            if (FS.InsideEdgeMode == FrameSettings.InsideMode.RegularEdge)
                            {
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
                            }

                            if (FS.InsideEdgeMode == FrameSettings.InsideMode.FormFitting)
                            {
                                if (rightside)
                                {
                                    x = p1.X + (FS.margin / 2) ;
                                    rad = FS.margin;
                                }
                                else
                                {
                                    x =  p1.X - (FS.margin / 2);
                                    rad = FS.margin;
                                }
                            }


                            int tabs = (int)Math.Floor(Math.Abs(dy) / FS.mmbetweentabs);
                            for (int j = 0; j < tabs; j++)
                            {
                                double y = p1.Y + (dy / (float)tabs) * (j + 0.5);
                                var BR = PNL.AddTab(new PointD(x, y));
                                log.AddString(String.Format("tab at {0} - radius {1}", BR.Center, rad));
                                BR.Radius = (float)rad;
                            }
                        }

                        if (Math.Abs(D.Y) < 0.5 && Math.Abs(D.X) >= FS.mmbetweentabs && FS.HorizontalTabs)
                        {
                            // perfect vertical! 
                            log.AddString(String.Format("horizontal found: {0} -> {1}", p1, p2));

                            double dx = p2.X - p1.X;

                            double y = 0;
                            double rad = 0;
                            bool rightside = (dx < 0);
                            if (ClockWise) rightside = !rightside;

                            if (FS.InsideEdgeMode == FrameSettings.InsideMode.RegularEdge)
                            {
                                if (rightside)
                                {
                                    y = (p1.Y + (BB.BottomRight.Y + FS.margin)) / 2;
                                    rad = Math.Abs((p1.Y - (BB.BottomRight.Y + FS.margin))) / 2.0 + FS.margin;
                                }
                                else
                                {
                                    y = (p1.Y + (BB.TopLeft.Y - FS.margin)) / 2;
                                    rad = Math.Abs((p1.Y - (BB.TopLeft.Y - FS.margin))) / 2.0 + FS.margin;
                                }
                            }

                            if (FS.InsideEdgeMode == FrameSettings.InsideMode.FormFitting)
                            {
                                if (rightside)
                                {
                                    y = p1.Y + (FS.margin / 2);
                                    rad = FS.margin;
                                }
                                else
                                {
                                    y = p1.Y - (FS.margin / 2);
                                    rad = FS.margin;
                                }
                            }


                            int tabs = (int)Math.Floor(Math.Abs(dx) / FS.mmbetweentabs);
                            for (int j = 0; j < tabs; j++)
                            {
                                double x = p1.X + (dx / (float)tabs) * (j + 0.5);
                                var BR = PNL.AddTab(new PointD(x, y));
                                log.AddString(String.Format("tab at {0} - radius {1}", BR.Center, rad));
                                BR.Radius = (float)rad;
                            }
                        }
                    }
                }
            }


            PNL.UpdateShape(log);
            log.AddString("postupdateshape");
            try
            {
                Directory.CreateDirectory(OutputFolder);
                var PNLFiles = PNL.SaveGerbersToFolder("MergedFrame", OutputFolder, log, false, true, false, true, basename);
            }
            catch(Exception E)
            {
                log.AddString("save gerbers to folder Exceptions: " + E.ToString());
            }
            try
            {
                if (FS.RenderSample)
                {
                    GerberImageCreator GIC = new GerberImageCreator();
                    GIC.AddBoardsToSet(Directory.GetFiles(OutputFolder).ToList(), new SilentLog());
                    GIC.WriteImageFiles(basename, 200, true, false, true, null);
                }
            }
            catch (Exception E)
            {
                log.AddString("GIC Exceptions: " + E.ToString());
            }

            log.PopActivity();

            //            PNL.SaveOutlineTo(OutputFolder, "mergedframeblended");
            return;



            var FrameFiles = Directory.GetFiles(FrameFolder);
            var OutlineFiles = Directory.GetFiles(OutlineFolder);
            List<String> AllFiles = new List<string>();
            AllFiles.AddRange(FrameFiles);
            foreach (var a in OutlineFiles)
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
            GerberMerger.MergeAllByFileType(AllFiles, OutputFolder, "MergedFrame", log);

        }


        public static List<string> WriteSideEdgeFrame(PolyLine pl, FrameSettings FS, string basefile, BOM output = null)
        {
            List<string> Files = new List<string>();

            try
            {

                int polyid = 0;
                string fname = System.IO.Path.GetFileName(basefile);
                string fnamenoext = System.IO.Path.GetFileNameWithoutExtension(basefile);
                string OutName = Path.Combine(System.IO.Path.GetDirectoryName(basefile), fnamenoext);
                Console.WriteLine("writing frame files to {0}", OutName);
             //   Bounds B = new Bounds();
               // B.AddPolyLine(pl);
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



                // board outline
                PolyLine PL = new PolyLine(polyid++);
                PL.MakeRoundedRect(new PointD(0, 0), new PointD(OuterWidth, OuterHeight), (double)FS.roundedOuterCorners);
                PCB.AddOutline(PL);

                if (FS.InsideEdgeMode == FrameSettings.InsideMode.RegularEdge )
                {
                    PolyLine PL2 = new PolyLine(polyid++);
                    PL2.MakeRoundedRect(new PointD(LE, TE), new PointD(InnerWidth + LE, InnerHeight + TE), (double)FS.roundedInnerCorners);
                    PCB.AddOutline(PL2);
                }

                if (FS.InsideEdgeMode == FrameSettings.InsideMode.FormFitting && pl !=null)
                {
                    PolyLine PP = pl.Copy();
                    PP.Translate(-FS.offset.X, -FS.offset.Y);
                    List<PolyLine> PL2 = PP.Offset(FS.margin, polyid++);
                   foreach(var l in PL2) PCB.AddOutline(l);

                    polyid += PL2.Count;


                }

                #region fiducials

                List<PointD> Fiducials = new List<PointD>();

                foreach (var P in FS.fiducialsListData)
                {
                    PointD fiducialPoint = P.pos;
                    PCB.Fiducial(fiducialPoint, P.CopperDiameter, P.MaskDiameter, P.Top ? BoardSide.Top : BoardSide.Bottom);
                }

                #endregion

                string FrameTitle = FS.FrameTitle;

                string FrameTopTitle = FrameTitle + " - Top";
                string FrameBottomTitle = FrameTitle + " - Bottom";
                double verticaltitleclearance = 0;
                double horizontaltitleclearance = 0;
                if (FrameTitle.Length > 0)
                {
                    FontSet fnt = FontSet.Load("Font.xml");
                    horizontaltitleclearance = fnt.StringWidth(FrameBottomTitle, TE / 4) + mountholediameter * 2 + FS.topEdge;
                    verticaltitleclearance = fnt.StringWidth(FrameBottomTitle, TE / 4) + mountholediameter * 2 + FS.topEdge;

                    PCB.Label(fnt, new PointD(OuterWidth / 2.0, OuterHeight - TE / 4.0), FrameTopTitle, TE / 4, StringAlign.CenterCenter, 0.1, true, true, 0);
                    PCB.Label(fnt, new PointD(OuterWidth - LE / 4, OuterHeight / 2), FrameTopTitle, TE / 4.0, StringAlign.CenterCenter, 0.1, true, true, -90);
                    PCB.Label(fnt, new PointD(LE / 4, OuterHeight / 2), FrameTopTitle, TE / 4.0, StringAlign.CenterCenter, 0.1, true, true, 90);
                    PCB.Label(fnt, new PointD(OuterWidth / 2.0, TE - TE / 4.0), FrameTopTitle, TE / 4.0, StringAlign.CenterCenter, 0.1, true, true, 0);


                    PCB.Label(fnt, new PointD(OuterWidth / 2.0, OuterHeight - TE / 4.0), FrameBottomTitle, TE / 4, StringAlign.CenterCenter, 0.1, true, true, 0, true, true, true);
                    PCB.Label(fnt, new PointD(OuterWidth - LE / 4, OuterHeight / 2), FrameBottomTitle, TE / 4.0, StringAlign.CenterCenter, 0.1, true, true, -90, true);
                    PCB.Label(fnt, new PointD(LE / 4, OuterHeight / 2), FrameBottomTitle, TE / 4.0, StringAlign.CenterCenter, 0.1, true, true, 90, true);
                    PCB.Label(fnt, new PointD(OuterWidth / 2.0, TE - TE / 4.0), FrameBottomTitle, TE / 4.0, StringAlign.CenterCenter, 0.1, true, true, 0, true);
                }

                if (FS.addHoles)
                {
                    double side = LE / 2.0;
                    double top = TE / 2.0;

                    PCB.Drill(new PointD(side, top), mountholediameter, 1);
                    PCB.Drill(new PointD(OuterWidth - side, top), mountholediameter, 1);
                    PCB.Drill(new PointD(OuterWidth - side, OuterHeight - top), mountholediameter, 1);
                    PCB.Drill(new PointD(side, OuterHeight - top), mountholediameter, 1);

                    double dx = (OuterWidth - side) - side;
                    double dy = (OuterHeight - top) - top;

                    dx -= horizontaltitleclearance;
                    dy -= verticaltitleclearance;
                    dx /= 2;
                    dy /= 2;

                    int horiz = (int)Math.Ceiling((dx / 2) / FS.MaxMountHoleSpacingMM);
                    int vert = (int)Math.Ceiling((dy / 2) / FS.MaxMountHoleSpacingMM);

                    dx /= (float)horiz;
                    dy /= (float)vert;

                    if (dx < FS.MinMountHoleSpacingMM) horiz = 0;
                    if (dy < FS.MinMountHoleSpacingMM) vert = 0;

                    for (int i = 1; i <= horiz; i++)
                    {
                        PCB.Drill(new PointD(side + (dx) * i, top), mountholediameter, 1);
                        PCB.Drill(new PointD((OuterWidth - side) - (dx) * i, top), mountholediameter, 1);
                        PCB.Drill(new PointD(side + (dx) * i, OuterHeight - top), mountholediameter, 1);
                        PCB.Drill(new PointD((OuterWidth - side) - (dx) * i, OuterHeight - top), mountholediameter, 1);
                    }


                    for (int i = 1; i <= vert; i++)
                    {
                        PCB.Drill(new PointD(side, top + (dy) * i), mountholediameter, 1);
                        PCB.Drill(new PointD(side, (OuterHeight - top) - (dy) * i), mountholediameter, 1);
                        PCB.Drill(new PointD((OuterWidth - side), top + (dy) * i), mountholediameter, 1);
                        PCB.Drill(new PointD((OuterWidth - side), (OuterHeight - top) - (dy) * i), mountholediameter, 1);
                    }
                }
                if (FS.DefaultFiducials)
                {
                    double side = LE / 2.0;
                    double top = TE / 2.0;

                    PCB.Fiducial(new PointD(side + mountholediameter * 2, top), 1, 2, FS.FiducialSide, PCBWriterSet.FiducialStyle.Square);
                    PCB.Fiducial(new PointD(side + mountholediameter * 3, top), 1, 2, FS.FiducialSide, PCBWriterSet.FiducialStyle.Round);


                    PCB.Fiducial(new PointD(OuterWidth- (side + mountholediameter * 2), top), 1, 2, FS.FiducialSide, PCBWriterSet.FiducialStyle.Square);
                    PCB.Fiducial(new PointD(OuterWidth - (side + mountholediameter * 3), top), 1, 2, FS.FiducialSide, PCBWriterSet.FiducialStyle.Round);


                    PCB.Fiducial(new PointD(side + mountholediameter * 2, OuterHeight - top), 1, 2, FS.FiducialSide, PCBWriterSet.FiducialStyle.Round);
                    PCB.Fiducial(new PointD(side + mountholediameter * 3, OuterHeight - top), 1, 2, FS.FiducialSide, PCBWriterSet.FiducialStyle.Square);

                }
                if (FS.RenderDirectionArrow)
                {
                    double side = LE / 2.0;
                    double top = TE / 2.0;
                    double dy = (OuterHeight - top) - top;

                    dy -= verticaltitleclearance;
                    dy /= 2;
                    int vert = (int)Math.Ceiling((dy / 2) / FS.MaxMountHoleSpacingMM);
                    dy /= (float)vert;

                    PCB.Arrow(new PointD(side, (OuterHeight - top) - (dy) * 1 + mountholediameter * 1.5), new PointD(side, (OuterHeight - top) - (dy) * 0 - mountholediameter * 1.5), FS.DirectionArrowSide);
                    PCB.Arrow(new PointD(OuterWidth - side, (OuterHeight - top) - (dy) * 1 + mountholediameter * 1.5), new PointD(OuterWidth - side, (OuterHeight - top) - (dy) * 0 - mountholediameter * 1.5), FS.DirectionArrowSide);

                    PCB.Arrow(new PointD(side, (top) + (dy) * 0 + mountholediameter * 1.5), new PointD(side, (top) + (dy) * 1 - mountholediameter * 1.5), FS.DirectionArrowSide);
                    PCB.Arrow(new PointD(OuterWidth - side, (top) + (dy) * 0 + mountholediameter * 1.5), new PointD(OuterWidth - side, (top) + (dy) * 1 - mountholediameter * 1.5), FS.DirectionArrowSide);
                }

                PolyLine Left = new PolyLine();
                PolyLine Right = new PolyLine();
                PolyLine Top = new PolyLine();
                PolyLine Bottom = new PolyLine();


                Top.MakePRectangle(OuterWidth + 1, FS.topEdge + 1, -1, -1);
                Bottom.MakePRectangle(OuterWidth, FS.topEdge, 0, OuterHeight - FS.topEdge);
                Left.MakePRectangle(FS.leftEdge + 1, OuterHeight + 1, -1, -1);
                Right.MakePRectangle(FS.leftEdge, OuterHeight, OuterWidth - FS.leftEdge, 0);
                PCB.ArtInclusions.Add(Left);
                PCB.ArtInclusions.Add(Right);
                PCB.ArtInclusions.Add(Top);
                PCB.ArtInclusions.Add(Bottom);



                //PCB.CellularArt();


                Files.AddRange(PCB.Write(Path.GetDirectoryName(basefile), Path.GetFileNameWithoutExtension(basefile), output, FS.offset)); ;



                if (FS.RenderSample)
                {
                    GerberImageCreator GIC = new GerberImageCreator();
                    GIC.AddBoardsToSet(Files, new SilentLog());
                    GIC.WriteImageFiles(OutName, 200, true, false, true, null);
                }
            }
            catch (Exception e)
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
