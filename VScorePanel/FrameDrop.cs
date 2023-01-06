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
using GerberLibrary.Core.Primitives;

namespace VScorePanel
{
    public partial class FrameDrop : Form
    {
        public FrameDrop()
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
                           
                            GerberLibrary.GerberImageCreator GIC = new GerberLibrary.GerberImageCreator();
                            var A = Directory.GetFiles(S);
                            List<String> SensibleFile = new List<string>();
                            foreach(var a in A)
                            {
                                GerberLibrary.Core.BoardSide Si;
                                GerberLibrary.Core.BoardLayer La;
                                Gerber.DetermineBoardSideAndLayer(a, out Si, out La);
                                
                                if (Si == GerberLibrary.Core.BoardSide.Both && La == GerberLibrary.Core.BoardLayer.Outline) SensibleFile.Add(a);
                                if (Si == GerberLibrary.Core.BoardSide.Both && La == GerberLibrary.Core.BoardLayer.Mill) SensibleFile.Add(a);
                                if (La == GerberLibrary.Core.BoardLayer.Copper) SensibleFile.Add(a);
                            }
                            GIC.AddBoardsToSet(SensibleFile.ToList(), new StandardConsoleLog());

                            GerberFrameWriter.FrameSettings FS = new GerberFrameWriter.FrameSettings();
                            List<String> OutputLines = new List<string>();

                            OutputLines.Add(String.Format("Panel for {0}", Path.GetDirectoryName(S)));
                            Bounds PanelBounds = new Bounds();
                            double w = GIC.BoundingBox.Width();
                            double h = GIC.BoundingBox.Height();
                            double bx = GIC.BoundingBox.TopLeft.X;
                            double by = GIC.BoundingBox.TopLeft.Y;

                            GerberPanel Pnl = new GerberPanel();
                            Pnl.TheSet.ClipToOutlines = false;
                            Pnl.AddGerberFolder(new StandardConsoleLog(), S);
                            double ginbetween = (double)numericUpDown1.Value;

                            for (int x = 0; x < (int)xbox.Value; x++)
                            {
                                for (int y = 0; y < (int)ybox.Value; y++)
                                {

                                    var I = Pnl.AddInstance(S, new PointD(x * (w+ginbetween), y * (h+ginbetween)));

                                    OutputLines.Add(String.Format("Adding board instance: {0}mm x {1}mm offset", x * (w + ginbetween), y * (h + ginbetween)));

                                    if (theMode == PanelMode.Groovy)  I.IgnoreOutline = true;

                                    PanelBounds.FitPoint(bx + ((x + 0) * (w + ginbetween)), by + ((y + 0) *(h+ ginbetween)));
                                    PanelBounds.FitPoint(bx + (w+(x + 0) * (w + ginbetween)), by + ((y + 0) *(h+ ginbetween)));
                                    PanelBounds.FitPoint(bx + (w+(x + 0) * (w + ginbetween)), by + (h+(y +0) *(h+ ginbetween)));
                                    PanelBounds.FitPoint(bx + ((x + 0) * (w + ginbetween)), by + (h+(y + 0) *(h + ginbetween)));
                                }
                            }
                            if(theMode == PanelMode.Tabby)
                            {
                                OutputLines.Add("Panel created as Tabby board");
                                OutputLines.Add(String.Format("Space inbetween: {0}mm", ginbetween));


                                FS.FrameTitle = FrameTitle.Text; ;
                                FS.RenderSample = false;
                                FS.margin = ginbetween;
                                FS.topEdge = FS.leftEdge = (double)framebox.Value;
                                OutputLines.Add(String.Format("Frame width: {0}mm", FS.leftEdge));
                                FS.roundedInnerCorners = 0;
                                FS.roundedOuterCorners = 0;
                                FS.RenderSample = false;
                                FS.DirectionArrowSide = GerberLibrary.Core.BoardSide.Top;

                                FS.RenderDirectionArrow = true;
                                //FS.DefaultFiducials = true;
                                //FS.FiducialSide = BoardSide.Both;
                                FS.HorizontalTabs = false;
                                FS.VerticalTabs = false;
                                FS.InsideEdgeMode = GerberFrameWriter.FrameSettings.InsideMode.RegularEdge;
                                FS.PositionAround(PanelBounds);

                                string sS = String.Format("Frame size: {0} mm x {1} mm", FS.innerWidth + FS.margin * 2 + FS.leftEdge, FS.innerHeight + FS.margin * 2 + FS.topEdge);
                                OutputLines.Add(sS);
                                var OutputFolder = Path.Combine(S, "../" + Path.GetFileNameWithoutExtension(S) + "_TABBY");
                                Directory.CreateDirectory(OutputFolder);

                                String FrameFolder = Path.Combine(OutputFolder, "frame");
                                Directory.CreateDirectory(FrameFolder);

                                
                                //Directory.CreateDirectory(Path.Combine(OutputFolder, "merged"));
                                Directory.CreateDirectory(Path.Combine(OutputFolder, "merged"));
                                GerberFrameWriter.WriteSideEdgeFrame(null, FS, Path.Combine(FrameFolder, "panelframe"), null);
                                Pnl.AddGerberFolder(new StandardConsoleLog(), FrameFolder);
                                Pnl.AddInstance(FrameFolder, new PointD(0, 0));
                                Pnl.TheSet.MergeFileTypes = true;

                                int tabcount = 2;
                                double tx = (w - (tabcount * ginbetween*2))/ (tabcount + 1.0f);
                                double ty = (h - (tabcount * ginbetween * 2)) / (tabcount + 1.0f);
                                double txs = tx+ ginbetween;
                                double tys = ty + ginbetween;
                                tx += ginbetween * 2;
                                ty += ginbetween * 2;

                                for (int x = 0; x < (int)xbox.Value+1; x++)
                                {
                                    for (int y = 0; y < (int)ybox.Value+1; y++)
                                    {

                                        for (int i = 0; i < tabcount; i++)
                                        {

                                            if (x < (int)xbox.Value)
                                            {
                                                var BT1 = Pnl.AddTab(new PointD(bx + (x + 0) * (w + ginbetween) + tx * (i) + txs, by + ((y + 0) * (h + ginbetween)) - ginbetween / 2));
                                                BT1.Radius = (float)ginbetween;
                                                OutputLines.Add(String.Format( "tab at ({0},{1}) diameter {2}", BT1.Center.X, BT1.Center.Y, BT1.Radius * 2.0f));
                                            }
                                            if (y < (int)ybox.Value)
                                            {
                                                var BT2 = Pnl.AddTab(new PointD(bx + (x + 0) * (w + ginbetween) - ginbetween / 2, by + ((y + 0) * (h + ginbetween)) + ty * (i) + tys));
                                                BT2.Radius = (float)ginbetween;
                                                OutputLines.Add(String.Format("tab at ({0},{1}) diameter {2}", BT2.Center.X, BT2.Center.Y, BT2.Radius * 2.0f));
                                            }
                                        }

                                    }
                                }


                                
                                Pnl.UpdateShape(new StandardConsoleLog());
                                Pnl.SaveGerbersToFolder(Path.GetFileNameWithoutExtension(S) + "_Panel", Path.Combine(OutputFolder, "merged"), new StandardConsoleLog());
                                File.WriteAllLines(Path.Combine(OutputFolder, "PanelReport.txt"), OutputLines);
                                Pnl.SaveFile(Path.Combine(OutputFolder, "Panel.gerberset"));

                                List<String> FilesInFolder = Directory.GetFiles(Path.Combine(OutputFolder, "merged"), "*.frontpanelholes").ToList();
                                if (FilesInFolder.Count > 0)
                                {
                                    String JigFolder = Path.Combine(OutputFolder, "jig");
                                    Directory.CreateDirectory(JigFolder);
                                    File.Copy(FilesInFolder[0], Path.Combine(JigFolder, "jig.gml"), true);
                                    FS.InsideEdgeMode = GerberFrameWriter.FrameSettings.InsideMode.NoEdge;                                    
                                    GerberFrameWriter.WriteSideEdgeFrame(null, FS, Path.Combine(JigFolder,"jig"), null);
                                    GerberMerger.Merge(Path.Combine(JigFolder, "jig.gml"), Path.Combine(JigFolder, "jig.gko"), Path.Combine(JigFolder, "jig.gko2"), new StandardConsoleLog());
                                    File.Delete(Path.Combine(JigFolder, "jig.gko"));
                                    File.Delete(Path.Combine(JigFolder, "jig.gml"));
                                    File.Move(Path.Combine(JigFolder, "jig.gko2"), Path.Combine(JigFolder, "jig.gko"));

                                }

                            }

                            if (theMode == PanelMode.Groovy)
                            {

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
                                FS.PositionAround(PanelBounds);


                                var OutputFolder = Path.Combine(S, "../" + Path.GetFileNameWithoutExtension(S) + "_GROOVY");
                                Directory.CreateDirectory(OutputFolder);

                                GerberOutlineWriter GOW = new GerberOutlineWriter();
                                int xmax = (int)xbox.Value + 1;
                                int ymax = (int)ybox.Value + 1;
                                if (ginbetween > 0)
                                {
                                    xmax--;
                                    ymax--;
                                }

                                for (int x = 0; x < xmax; x++)
                                {
                                    PolyLine PL = new PolyLine();
                                    PL.Add(bx + (x) * (w + ginbetween), PanelBounds.TopLeft.Y - (float)framebox.Value - 10);
                                    PL.Add(bx + (x) * (w + ginbetween), PanelBounds.BottomRight.Y + (float)framebox.Value + 10);
                                    GOW.AddPolyLine(PL);

                                    if (ginbetween > 0)
                                    {
                                        PolyLine PL2 = new PolyLine();
                                        PL2.Add(bx + (x) * (w + ginbetween) + w, PanelBounds.TopLeft.Y - (float)framebox.Value - 10);
                                        PL2.Add(bx + (x) * (w + ginbetween) + w, PanelBounds.BottomRight.Y + (float)framebox.Value + 10);
                                        GOW.AddPolyLine(PL2);
                                    }
                                }

                                for (int y = 0; y < ymax; y++)
                                {
                                    PolyLine PL = new PolyLine();
                                    PL.Add(PanelBounds.TopLeft.X - (float)framebox.Value - 10, by + (y) * (h + ginbetween));
                                    PL.Add(PanelBounds.BottomRight.X + (float)framebox.Value + 10, by + (y) * (h + ginbetween));
                                    GOW.AddPolyLine(PL);

                                    if (ginbetween > 0)
                                    {
                                        PolyLine PL2 = new PolyLine();
                                        PL2.Add(PanelBounds.TopLeft.X - (float)framebox.Value - 10, by + (y) * (h + ginbetween) + h);
                                        PL2.Add(PanelBounds.BottomRight.X + (float)framebox.Value + 10, by + (y) * (h + ginbetween) + h);
                                        GOW.AddPolyLine(PL2);
                                    }
                                }
                                GOW.Write(Path.Combine(OutputFolder, "VGrooves.gbr"));

                                String FrameFolder = Path.Combine(OutputFolder, "frame");
                                Directory.CreateDirectory(FrameFolder);
                                Directory.CreateDirectory(Path.Combine(OutputFolder, "merged"));
                                GerberFrameWriter.WriteSideEdgeFrame(null, FS, Path.Combine(FrameFolder, "panelframe"), null);



                                Pnl.AddGerberFolder(new StandardConsoleLog(), FrameFolder);
                                Pnl.AddInstance(FrameFolder, new PointD(0, 0));
                                Pnl.UpdateShape(new StandardConsoleLog());
                                Pnl.SaveGerbersToFolder(Path.GetFileNameWithoutExtension(S) + "_Panel", Path.Combine(OutputFolder, "merged"), new StandardConsoleLog());

                            }

                            
                         


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

        public enum PanelMode{
            Groovy,
            Tabby
        }
        public PanelMode theMode = PanelMode.Groovy;
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(listBox1.SelectedIndex)
            {
                case 1:
                    theMode = PanelMode.Groovy;
                    break;
                case 0:
                    theMode = PanelMode.Tabby;
                    break;

            }
        }
    }
}
