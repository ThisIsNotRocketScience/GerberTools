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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
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
                PolyLine PL = new PolyLine();
                PL.MakeRoundedRect(new PointD(0, 0), new PointD(OuterWidth, OuterHeight), (double)roundedOuterCorners.Value);
                Outline.AddPolyLine(PL, 0);
                PolyLine PL2 = new PolyLine();

                PL2.MakeRoundedRect(new PointD(LE, TE), new PointD(InnerWidth + LE, InnerHeight + TE), (double)roundedInnerCorners.Value);
                Outline.AddPolyLine(PL2, 0);

                #region fiducials

                List<PointD> Fiducials = new List<PointD>();
                Fiducials.Add(new PointD(LE * 2.0, TE / 2.0));                              // bottom left
                Fiducials.Add(new PointD(OuterWidth - LE * 2.0, TE / 2.0));                 // bottom right
                Fiducials.Add(new PointD(LE * 2.0, OuterHeight - TE / 2.0));                // top left

                // add the fourth fiducial only if no orientation safety is desired
                if (!cb_fiducials_orientationSafe.Checked)
                {
                    Fiducials.Add(new PointD(OuterWidth - LE * 2.0, OuterHeight - TE / 2.0));   // top right                    
                }
                                 
                foreach (var A in Fiducials)
                {
                    if (cb_fiductialsTopLayer.Checked)
                    {
                        TopCopper.AddFlash(A, 1.0);
                        TopSolderMask.AddFlash(A, 3.0);
                    }

                    if (cb_fiductialsBottomLayer.Checked)
                    {
                        BottomCopper.AddFlash(A, 1.0);
                        BottomSolderMask.AddFlash(A, 3.0);
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
    }
}
