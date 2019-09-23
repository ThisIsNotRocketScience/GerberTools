using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GerberLibrary;
using GerberLibrary;
using GerberLibrary.Core.Primitives;

namespace ProductionFrame
{
    public partial class ProductionFrameForm : Form
    {
        public ProductionFrameForm()
        {
            InitializeComponent();
            innerWidth.Maximum = 6000;
            innerHeight.Maximum = 6000;

            fiducialsListData.Rows.Add("Top", (double)leftEdge.Value / 2.0, (double)innerHeight.Value + (double)topEdge.Value + (double)topEdge.Value / 2.0, 1.0, 3.0);
            fiducialsListData.Rows.Add("Top", (double)leftEdge.Value /2.0, (double)topEdge.Value / 2.0, 1.0, 3.0);
            fiducialsListData.Rows.Add("Top", (double)innerWidth.Value + (double)leftEdge.Value + (double)leftEdge.Value / 2.0, (double)topEdge.Value / 2.0, 1.0, 3.0);

            fiducialsListData.Rows.Add("Bottom", (double)leftEdge.Value / 2.0, (double)innerHeight.Value + (double)topEdge.Value + (double)topEdge.Value / 2.0, 1.0, 3.0);
            fiducialsListData.Rows.Add("Bottom", (double)leftEdge.Value / 2.0, (double)topEdge.Value / 2.0, 1.0, 3.0);
            fiducialsListData.Rows.Add("Bottom", (double)innerWidth.Value + (double)leftEdge.Value + (double)leftEdge.Value / 2.0, (double)topEdge.Value / 2.0, 1.0, 3.0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                int polyid = 0;
                string fname = System.IO.Path.GetFileName(saveFileDialog1.FileName);
                string fnamenoext = System.IO.Path.GetFileNameWithoutExtension(saveFileDialog1.FileName);
                string OutName = "";
                if (fname.Length != fnamenoext.Length)
                {
                    int extlen = fname.Length - fnamenoext.Length;
                    OutName = saveFileDialog1.FileName.Substring(0, saveFileDialog1.FileName.Length - extlen);
                }
                else
                {
                    OutName = saveFileDialog1.FileName;
                }


                List<string> Files = new List<string>();

                double W = (double)innerWidth.Value;
                double H = (double)innerHeight.Value;
                double TE = (double)topEdge.Value;
                double LE = (double)leftEdge.Value;

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
                double mountholediameter = (double)holeDiameter.Value;

                if (addHolesCheck.Checked)
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
                PL.MakeRoundedRect(new PointD(0, 0), new PointD(OuterWidth, OuterHeight), (double)roundedOuterCorners.Value);
                Outline.AddPolyLine(PL, 0);
                PolyLine PL2 = new PolyLine(polyid++);

                PL2.MakeRoundedRect(new PointD(LE, TE), new PointD(InnerWidth + LE, InnerHeight + TE), (double)roundedInnerCorners.Value);
                Outline.AddPolyLine(PL2, 0);

                #region fiducials

                List<PointD> Fiducials = new List<PointD>();

                foreach (DataGridViewRow dataRow in fiducialsListData.Rows)
                {
                    PointD fiducialPoint = new PointD();

                    if (dataRow.Cells != null && dataRow.Cells["XCoordinate"].Value != null && dataRow.Cells["YCoordinate"].Value != null)
                    {
                        fiducialPoint.X = double.Parse(dataRow.Cells["XCoordinate"].Value.ToString());
                        fiducialPoint.Y = double.Parse(dataRow.Cells["YCoordinate"].Value.ToString());

                        if (string.Equals(dataRow.Cells["fiducialLayer"].Value.ToString(), "Top"))
                        {
                            TopCopper.AddFlash(fiducialPoint, double.Parse(dataRow.Cells["fiducialCopperDiameter"].Value.ToString())/2.0);
                            TopSolderMask.AddFlash(fiducialPoint, double.Parse(dataRow.Cells["fiducialSolderMaskDiam"].Value.ToString())/2.0);
                        }
                        else
                        {
                            BottomCopper.AddFlash(fiducialPoint, double.Parse(dataRow.Cells["fiducialCopperDiameter"].Value.ToString())/2.0);
                            BottomSolderMask.AddFlash(fiducialPoint, double.Parse(dataRow.Cells["fiducialSolderMaskDiam"].Value.ToString())/2.0);
                        }
                    }

                }

                #endregion

                string FrameTitle = FrameTitleBox.Text;

                if (FrameTitle.Length > 0)
                {
                    FontSet FS = FontSet.Load("Font.xml");
                    TopSilk.DrawString(new PointD(OuterWidth / 2.0, OuterHeight - TE / 4.0), FS, FrameTitle + " - top", TE / 4.0, 0.1, StringAlign.CenterCenter);
                    TopCopper.DrawString(new PointD(OuterWidth / 2.0, OuterHeight - TE / 4.0), FS, FrameTitle + " - top", TE / 4.0, 0.1, StringAlign.CenterCenter);
                    BottomSilk.DrawString(new PointD(OuterWidth / 2.0, OuterHeight - TE / 4.0), FS, FrameTitle + " - bottom", TE / 4.0, 0.1, StringAlign.CenterCenter, true);
                    BottomCopper.DrawString(new PointD(OuterWidth / 2.0, OuterHeight - TE / 4.0), FS, FrameTitle + " - bottom", TE / 4.0, 0.1, StringAlign.CenterCenter, true);
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
                
                GIC.WriteImageFiles(OutName, 50 );
                // Gerber.SaveGerberFileToImage(OutName + ".gko", OutName + ".gko.png", 200, Color.Black, Color.White);
                // Gerber.SaveGerberFileToImage(OutName + ".gto", OutName + ".gto.png", 200, Color.Black, Color.White);

            }
        }

        private void addHolesCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (addHolesCheck.Checked)
            {
                holeDiameter.Enabled = true;
            }
            else
            {
                holeDiameter.Enabled = false;
            }
        }
    }
}
