using GerberLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GerberLibrary;
using GerberLibrary.Core.Primitives;

namespace VScorePanel
{
    public partial class VScore : Form
    {
        public VScore()
        {
            InitializeComponent();
        }

        private void VScore_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {

                string[] D = e.Data.GetData(DataFormats.FileDrop) as string[];

                BackColor = Color.Green;

                foreach (string S in D)
                {
                    if (Directory.Exists(S))
                    {
                        try
                        {
                            var OutputFolder = Path.Combine(S, "../" + Path.GetFileNameWithoutExtension(S) + "_GROOVY");
                            Directory.CreateDirectory(OutputFolder);
                            GerberLibrary.GerberImageCreator GIC = new GerberLibrary.GerberImageCreator();
                            var A = Directory.GetFiles(S);

                            GIC.AddBoardsToSet(A.ToList(), new StandardConsoleLog());

                            GerberFrameWriter.FrameSettings FS = new GerberFrameWriter.FrameSettings();
                            
                            FS.FrameTitle = FrameTitle.Text; ;
                            FS.RenderSample = false;
                            FS.margin = 0;
                            FS.topEdge = FS.leftEdge = (double)framebox.Value;
                            FS.roundedInnerCorners = 0;
                            FS.roundedOuterCorners = 0;
                            FS.RenderSample = false;
                            FS.RenderDirectionArrow = false;
                            FS.DirectionArrowSide = GerberLibrary.Core.BoardSide.Top;
                            //FS.DefaultFiducials = true;
                            //FS.FiducialSide = BoardSide.Both;
                            FS.HorizontalTabs = false;
                            FS.VerticalTabs = false;
                            FS.InsideEdgeMode = GerberFrameWriter.FrameSettings.InsideMode.NoEdge;

                            Bounds PanelBounds = new Bounds();
                            double w = GIC.BoundingBox.Width();
                            double h = GIC.BoundingBox.Height();
                            double bx = GIC.BoundingBox.TopLeft.X;
                            double by = GIC.BoundingBox.TopLeft.Y;

                            GerberPanel Pnl = new GerberPanel();
                            Pnl.TheSet.ClipToOutlines = false;

                            Pnl.AddGerberFolder(new StandardConsoleLog(), S);

                            for (int x = 0;x<(int)xbox.Value;x++)
                            {
                                for(int y =0;y< (int)ybox.Value; y++)
                                {
                                    var I = Pnl.AddInstance(S, new PointD(x * w, y * h));
                                    I.IgnoreOutline = true;

                                    PanelBounds.FitPoint(bx + ((x + 0) * w), by + ((y + 0) * h));
                                    PanelBounds.FitPoint(bx + ((x + 1) * w), by + ((y + 0) * h));
                                    PanelBounds.FitPoint(bx + ((x + 1) * w), by + ((y + 1) * h));
                                    PanelBounds.FitPoint(bx + ((x + 0) * w), by + ((y + 1) * h));
                                }
                            }
                            FS.PositionAround(PanelBounds);


                            GerberOutlineWriter GOW = new GerberOutlineWriter();
                            for (int x = 0; x < (int)xbox.Value+1; x++)
                            {
                                PolyLine PL = new PolyLine();
                                PL.Add(bx+(x ) * w, PanelBounds.TopLeft.Y - (float)framebox.Value - 10);
                                PL.Add(bx+(x ) * w, PanelBounds.BottomRight.Y + (float)framebox.Value + 10);
                                GOW.AddPolyLine(PL);

                            }
                            for (int y = 0; y < (int)ybox.Value + 1; y++)
                            {
                                PolyLine PL = new PolyLine();
                                PL.Add(PanelBounds.TopLeft.X - (float)framebox.Value - 10,by+ (y ) * h);
                                PL.Add(PanelBounds.BottomRight.X + (float)framebox.Value + 10,by+ (y ) * h);
                                GOW.AddPolyLine(PL);

                            }

                            GOW.Write(Path.Combine(OutputFolder, "VGrooves.gbr"));
                            String FrameFolder = Path.Combine(OutputFolder, "frame");
                            Directory.CreateDirectory(FrameFolder);
                            Directory.CreateDirectory(Path.Combine(OutputFolder, "merged"));
                            GerberFrameWriter.WriteSideEdgeFrame(null, FS, Path.Combine(FrameFolder, "panelframe"), null);

                            Pnl.AddGerberFolder(new StandardConsoleLog(), FrameFolder);
                            Pnl.AddInstance(FrameFolder, new PointD(0, 0));
                            Pnl.UpdateShape(new StandardConsoleLog());
                            Pnl.SaveGerbersToFolder(Path.GetFileNameWithoutExtension(S)+"_Panel", Path.Combine(OutputFolder, "merged"), new StandardConsoleLog());


                            CountDown = 10;
                        }
                        catch (Exception)
                        {
                            BackColor = Color.Red;
                        }
                    }


                }
            }
        }

        private void VScore_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }

        }
        int CountDown = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
                if (CountDown > 0)
                {
                    CountDown--;
                }
                else
                {
                    BackColor = Color.Gray;
                }
            
        }
    }
}
