using GerberLibrary;
using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

namespace GerberLibrary
{

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

            public string FrameTitle;
            public double innerWidth;
            public double innerHeight;
            public double topEdge = 5;
            public double leftEdge = 5;
            public double holeDiameter = 3.2;
            public bool addHoles = true;
            public double roundedInnerCorners = 2.0;
            public double roundedOuterCorners = 4.0;
            public List<Fiducial> fiducialsListData = new List<Fiducial>();
        }
        public static void WriteSideEdgeFrame(PolyLine pl, FrameSettings FS, string basefile)
        {
            int polyid = 0;
            string fname = System.IO.Path.GetFileName(basefile);
            string fnamenoext = System.IO.Path.GetFileNameWithoutExtension(basefile);
            string OutName = "";

            Bounds B = new Bounds();
            B.AddPolyLine(pl);
            FS.innerHeight = B.Height();
            FS.innerWidth = B.Width();

            List<string> Files = new List<string>();

            double W = (double)FS.innerWidth;
            double H = (double)FS.innerHeight;
            double TE = (double)FS.topEdge;
            double LE = (double)FS.leftEdge;

            double OuterWidth = W + LE * 2.0;
            double OuterHeight = H + TE * 2.0;
            double InnerWidth = W;
            double InnerHeight = H;

            GerberArtWriter Outline = new GerberArtWriter();
            GerberArtWriter TopSilk = new GerberArtWriter();
            GerberArtWriter TopCopper = new GerberArtWriter();
            GerberArtWriter TopSolderMask = new GerberArtWriter();

            GerberArtWriter BottomSilk = new GerberArtWriter();
            GerberArtWriter BottomCopper = new GerberArtWriter();
            GerberArtWriter BottomSolderMask = new GerberArtWriter();


            List<PointD> DrillHoles = new List<PointD>();
            double mountholediameter = (double)FS.holeDiameter;

            if (FS.addHoles)
            {
                double side = LE / 2.0;
                double top = TE / 2.0;


                DrillHoles.Add(new PointD(side, top));
                DrillHoles.Add(new PointD(OuterWidth - side, top));
                DrillHoles.Add(new PointD(OuterWidth - side, OuterHeight - top));
                DrillHoles.Add(new PointD(side, OuterHeight - top));

                ExcellonFile EF = new ExcellonFile();

                ExcellonTool MountHoleDrill = new ExcellonTool();
                MountHoleDrill.Radius = mountholediameter / 2.0;

                foreach (var a in DrillHoles)
                {
                    MountHoleDrill.Drills.Add(a);
                }
                EF.Tools[10] = MountHoleDrill;
                EF.Write(OutName + ".txt", 0, 0, 0, 0);

                Files.Add(OutName + ".txt");
            }

            // board outline
            PolyLine PL = new PolyLine(polyid++);
            PL.MakeRoundedRect(new PointD(0, 0), new PointD(OuterWidth, OuterHeight), (double)FS.roundedOuterCorners);
            Outline.AddPolyLine(PL, 0);
            PolyLine PL2 = new PolyLine(polyid++);

            PL2.MakeRoundedRect(new PointD(LE, TE), new PointD(InnerWidth + LE, InnerHeight + TE), (double)FS.roundedInnerCorners);
            Outline.AddPolyLine(PL2, 0);

            #region fiducials

            List<PointD> Fiducials = new List<PointD>();

            foreach (var P in FS.fiducialsListData)
            {
                PointD fiducialPoint = P.pos;

                if (P.Top)
                {
                    TopCopper.AddFlash(P.pos, P.CopperDiameter / 2.0);
                    TopSolderMask.AddFlash(P.pos, P.MaskDiameter / 2.0);
                }
                else
                {
                    BottomCopper.AddFlash(P.pos, P.CopperDiameter / 2.0);
                    BottomSolderMask.AddFlash(P.pos, P.MaskDiameter / 2.0);
                }

            }

            #endregion

            string FrameTitle = FS.FrameTitle;

            if (FrameTitle.Length > 0)
            {
                FontSet fnt = FontSet.Load("Font.xml");
                TopSilk.DrawString(new PointD(OuterWidth / 2.0, OuterHeight - TE / 4.0), fnt, FrameTitle + " - top", TE / 4.0, 0.1, StringAlign.CenterCenter);
                TopCopper.DrawString(new PointD(OuterWidth / 2.0, OuterHeight - TE / 4.0), fnt, FrameTitle + " - top", TE / 4.0, 0.1, StringAlign.CenterCenter);
                BottomSilk.DrawString(new PointD(OuterWidth / 2.0, OuterHeight - TE / 4.0), fnt, FrameTitle + " - bottom", TE / 4.0, 0.1, StringAlign.CenterCenter, true);
                BottomCopper.DrawString(new PointD(OuterWidth / 2.0, OuterHeight - TE / 4.0), fnt, FrameTitle + " - bottom", TE / 4.0, 0.1, StringAlign.CenterCenter, true);
            }

            foreach (var a in DrillHoles)
            {
                TopSolderMask.AddFlash(a, mountholediameter / 2.0 + 1.0);
                BottomSolderMask.AddFlash(a, mountholediameter / 2.0 + 1.0);
            }

            Outline.Write(OutName + ".gko");
            TopSilk.Write(OutName + ".gto");
            TopCopper.Write(OutName + ".gtl");
            TopSolderMask.Write(OutName + ".gts");

            BottomSilk.Write(OutName + ".gbo");
            BottomCopper.Write(OutName + ".gbl");
            BottomSolderMask.Write(OutName + ".gbs");


            Files.Add(OutName + ".gko");
            Files.Add(OutName + ".gto");
            Files.Add(OutName + ".gtl");
            Files.Add(OutName + ".gts");
            Files.Add(OutName + ".gbo");
            Files.Add(OutName + ".gbl");
            Files.Add(OutName + ".gbs");
            GerberImageCreator GIC = new GerberImageCreator();
            GIC.AddBoardsToSet(Files);
            GIC.WriteImageFiles(OutName, 100, true, false, true, null);
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
