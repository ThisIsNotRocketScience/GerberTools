using ClipperLib;
using GerberLibrary;
using GerberLibrary.Core;
using GerberLibrary.Core.Primitives;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace GerberCombinerBuilder
{
    public partial class GerberPanelize  : Form
    {
        GerberPanelizerParent ParentFrame;

        Thread ExportThread;
        String ExportFolder;
        Progress ProgressDialog;
        bool MouseCapture = false;
        PointD DragStartCoord = new PointD();
        PointD DragInstanceOriginalPosition = new PointD();
        PointD ContextStartCoord = new PointD();

        private float DrawingScale;
        public double Zoom = 1;
        public PointD CenterPoint = new PointD(0, 0);

        enum SnapMode
        {
            Mil100,
            Mil50,
            MM1,
            MM05,
            Off
        }

        SnapMode CurrentSnapMode = SnapMode.Off;
        public string LoadedFile = "";
        private AngledThing HoverShape = null;
        public GerberPanel ThePanel = new GerberPanel();
        Treeview TV;
        public AngledThing SelectedInstance;
        InstanceDialog ID;


        public GerberPanelize(GerberPanelizerParent Host, Treeview tv, InstanceDialog id)
        {
            ParentFrame = Host;
            Gerber.ArcQualityScaleFactor = 0.1;
            InitializeComponent();
            UpdateAutoProcessButton();
            UpdateSnapBox(SnapMode.MM1);
            //   AddGerberFolder(@"C:\Projects\Circuits\50pcs 5x5cm green 1.6mm - goaoma");
            //   TheSet.Tabs.Add(new BreakTab() { Radius = 5, Angle = 10, Center = new PointF(50,50) });
            TV = tv;
            ID = id;
            TV.BuildTree(this, ThePanel.TheSet);
            DrawingScale = Math.Min(glControl1.Width, glControl1.Height) / 110.0f;

            ZoomToFit();
            BuildTitle();
            //   LoadFile(@"C:\Projects\Circuits\panelsets\RackPanel-Frame\innerframetest.gerberset");

        }
        void BuildTitle()
        {
            this.Text = "Set: " + BaseName;
        }

        //private void pictureBox1_Paint(object sender, PaintEventArgs e)
        //{
        //    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
        //    e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        //    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        //    Aspect = pictureBox1.Width / pictureBox1.Height;
        //    DrawingScale = Math.Min(pictureBox1.Width, pictureBox1.Height) / 110.0f;

        //    e.Graphics.TranslateTransform(0, pictureBox1.Height);

        //    e.Graphics.ScaleTransform(DrawingScale, -DrawingScale);
        //    e.Graphics.TranslateTransform(5, 5);


        //    ThePanel.DrawBoardBitmap(1.0f / DrawingScale, new GraphicsGraphicsInterface(e.Graphics), pictureBox1.Width, pictureBox1.Height, SelectedInstance);
        //    // e.Graphics.DrawImage(BoardBitmap, 0, 0, pictureBox1.Width, pictureBox1.Height);

        //}

        PointD MouseToMM(PointD Mouse)
        {
            PointF P = Mouse.ToF();
            P.X -= glControl1.Width / 2;
            P.Y -= glControl1.Height / 2;
            P.Y *= -1;

            // Console.Write("| {0},{1} ", (int)P.X, (int)P.Y);
            P.X /= (float)Zoom;
            P.Y /= (float)Zoom;
            //  Console.Write("| {0},{1} ", (int)P.X, (int)P.Y);
            P.X += (float)CenterPoint.X;
            P.Y += (float)CenterPoint.Y;

            //  Console.WriteLine("| {0},{1}", (int)P.X, (int)P.Y);
            return new PointD(P);
            P.X = ((P.X * 2) / glControl1.Width - 1.0f);
            P.Y = ((P.Y * 2) / glControl1.Height - 1.0f);
            P.Y *= -1;




            P.X /= DrawingScale / (0.5f * glControl1.Width);
            P.Y /= DrawingScale / (0.5f * glControl1.Height);


            P.X += 50;
            P.Y += 50;

            return new PointD(P);


        }



        internal void SetSelectedInstance(AngledThing gerberInstance)
        {
            SelectedInstance = gerberInstance;
            ID.UpdateBoxes(this);
            Redraw(false);
        }


        private string BaseName = "Untitled";
        private bool ShapeMarkedForUpdate = true;
        private bool ForceShapeUpdate = false;
        internal void Redraw(bool refreshshape = true, bool force = false)
        {
            if (SuspendRedraw) return;
            if (force) ForceShapeUpdate = true;
            if (refreshshape)
            {
                ShapeMarkedForUpdate = true;
                ProcessButton.Enabled = true;

            }
            glControl1.Invalidate();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            UpdateScrollers();
            Redraw(false);
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filename = openFileDialog1.FileName;

                LoadFile(filename);
            }
        }

        public void LoadFile(string filename)
        {
            LoadedFile = filename;
            ThePanel.LoadFile(filename);
            ThePanel.UpdateShape(); 
            TV.BuildTree(this, ThePanel.TheSet);
            ZoomToFit();

            Redraw(false);

            BaseName = Path.GetFileNameWithoutExtension(filename);
            BuildTitle();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string FileName = saveFileDialog1.FileName;

                SaveFile(FileName);
            }
        }


        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectedInstance = null;
            HoverShape = null;
            ThePanel = new GerberPanel();
            TV.BuildTree(this, ThePanel.TheSet);
            Redraw(true);
        }


        private void AddTab(PointD center)
        {

            var BT = ThePanel.AddTab(MouseToMM(center));

            TV.BuildTree(this, ThePanel.TheSet);
            SetSelectedInstance(BT);
            Redraw(true);
        }




        public void exportAllGerbersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ExportFolder = folderBrowserDialog2.SelectedPath;
                ProgressDialog = new Progress(this);
                ProgressDialog.Show();
                Enabled = false;
                ParentFrame.Enabled = false;
                ExportThread = new Thread(new ThreadStart(ExportThreadFunc));
                ExportThread.Start();

            }
        }

        public void ExportThreadFunc()
        {
            ThePanel.SaveGerbersToFolder(BaseName, ExportFolder, ProgressDialog);
        }


        internal void ProcessDone()
        {
            this.Enabled = true;
            ParentFrame.Enabled = true;

            ProgressDialog.Close();
            ProgressDialog.Dispose();
            ProgressDialog = null;
        }

        private void GerberPanelize_Activated(object sender, EventArgs e)
        {
            ID.UpdateBoxes(this);
            TV.BuildTree(this, ThePanel.TheSet);
            ParentFrame.ActivatePanelizer(this);
        }

        private void GerberPanelize_FormClosed(object sender, FormClosedEventArgs e)
        {
            ParentFrame.RemovePanelizer();
        }

        private void GerberPanelize_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
        }

        internal void AddInstance(string path, PointD coord)
        {

            SetSelectedInstance(ThePanel.AddInstance(path, MouseToMM(coord)));
            TV.BuildTree(this, ThePanel.TheSet);
            Redraw(true);
        }

        internal void RemoveInstance(AngledThing angledThing)
        {
            if (SelectedInstance == angledThing)
            {

                SetSelectedInstance(null);
            }

            if (HoverShape == angledThing)
            {
                HoverShape = null;
            }

            ThePanel.RemoveInstance(angledThing);
            TV.BuildTree(this, ThePanel.TheSet);
            Redraw(true);
        }

        internal void SaveFile(string FileName)
        {
            ThePanel.SaveFile(FileName);
            BaseName = Path.GetFileNameWithoutExtension(FileName);
            BuildTitle();
            LoadedFile = FileName;
        }

        private void DoMouseDown(MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {

                SelectedInstance = ThePanel.FindOutlineUnderPoint(MouseToMM(new PointD(e.X, e.Y)));
                if (SelectedInstance != null)
                {
                    MouseCapture = true;
                    DragStartCoord = new PointD(e.X, e.Y);
                    DragInstanceOriginalPosition = new PointD(SelectedInstance.Center);
                }
                SetSelectedInstance(SelectedInstance);
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {

                SelectedInstance = ThePanel.FindOutlineUnderPoint(MouseToMM(new PointD(e.X, e.Y)));

                ContextStartCoord = new PointD(e.X, e.Y);

                if (SelectedInstance != null)
                {
                    contextMenuStrip2.Show(this, e.Location);
                }
                else
                {
                    addInstanceToolStripMenuItem.DropDownItems.Clear();
                    foreach (var a in ThePanel.TheSet.LoadedOutlines)
                    {
                        addInstanceToolStripMenuItem.DropDownItems.Add(a, null, addinstance);
                    }
                    contextMenuStrip1.Show(this, e.Location);
                }
                SetSelectedInstance(SelectedInstance);
            }
        }

        private void addinstance(object sender, EventArgs e)
        {
            ToolStripDropDownItem TSDDI = sender as ToolStripDropDownItem;
            AddInstance(TSDDI.Text, ContextStartCoord);
            //Console.WriteLine(sender.GetType().ToString());
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            DoMouseUp(e);
        }

        private void DoMouseUp(MouseEventArgs e)
        {
            if (MouseCapture)
            {
                ID.UpdateBoxes(this);

                MouseCapture = false;
                PointD Delta = new PointD(e.X, e.Y) - DragStartCoord;
                if (Delta.Length() == 0)
                {
                    if (SelectedInstance != null)
                    {
                        SelectedInstance.Center = DragInstanceOriginalPosition.ToF();
                        
                    }
                    Redraw(false);
                }
                else
                {
                    if (SelectedInstance != null)
                    {
                        var GI = SelectedInstance as GerberInstance;
                        if (GI != null)
                        {
                            GI.RebuildTransformed(ThePanel.GerberOutlines[GI.GerberPath], ThePanel.TheSet.ExtraTabDrillDistance);

                        }
                    }
                    Redraw(true);
                }
            }
        }

        PointD LastMouseMove = new PointD(0, 0);
        public bool SuspendRedraw = false;

        private void DoMouseMove(MouseEventArgs e)
        {
            LastMouseMove = new PointD(e.X, e.Y);
            if (MouseCapture && SelectedInstance != null)
            {
                PointD Delta = new PointD(e.X, e.Y) - DragStartCoord;
                Delta.X /= Zoom;
                Delta.Y /= -Zoom;
                SelectedInstance.Center = Snap(DragInstanceOriginalPosition + Delta).ToF();
                //       SelectedInstance.Center.Y = (float)(DragInstanceOriginalPosition.Y + Delta.Y);
                Redraw(false);
            }
            else
            {

                var newHoverShape = ThePanel.FindOutlineUnderPoint(MouseToMM(new PointD(e.X, e.Y)));
                if (newHoverShape != HoverShape)
                {
                    HoverShape = newHoverShape;
                    Redraw(false);
                }
            }
        }

        public double SnapDistance()
        {

            switch (CurrentSnapMode)
            {
                case SnapMode.MM1: return 1;
                case SnapMode.MM05: return 0.5;
                case SnapMode.Mil50: return 50 * (25.4 / 1000.0);
                case SnapMode.Mil100: return 100 * (25.4 / 1000.0);
            };
            return -1;

        }

        public PointD Snap(PointD inp)
        {
            if (CurrentSnapMode == SnapMode.Off) return inp;
            double multdiv = 1;

            switch (CurrentSnapMode)
            {
                case SnapMode.MM1: break;
                case SnapMode.MM05: multdiv = 2; break;
                case SnapMode.Mil50: multdiv = 0.254 / 2.0; break;
                case SnapMode.Mil100: multdiv = 0.254; break;
            };

            PointD Res = new PointD();
            Res.X = Math.Floor(inp.X * multdiv) / multdiv;
            Res.Y = Math.Floor(inp.Y * multdiv) / multdiv;
            return Res;

        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedInstance != null) RemoveInstance(SelectedInstance);
        }

        private void addBreakTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddTab(ContextStartCoord);
        }



        private void milToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            UpdateSnapBox(SnapMode.Mil50);
        }

        private void UpdateSnapBox(SnapMode Mode)
        {
            string name = "none";
            switch (Mode)
            {
                case SnapMode.Mil100: name = "100 mil"; break;
                case SnapMode.Mil50: name = "50 mil"; break;
                case SnapMode.MM1: name = "1mm"; break;
                case SnapMode.MM05: name = "0.5mm"; break;
                case SnapMode.Off: name = "off"; break;
            }
            CurrentSnapMode = Mode;
            Redraw(false);

            toolStripDropDownButton1.Text = "Snap: " + name;
        }

        private void milToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateSnapBox(SnapMode.Mil100);

        }

        private void mmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateSnapBox(SnapMode.MM1);

        }

        private void mmToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            UpdateSnapBox(SnapMode.MM05);

        }

        private void offToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SnapOff();
        }

        private void SnapOff()
        {
            UpdateSnapBox(SnapMode.Off);
        }

        public void ZoomToFit()
        {
            if (ThePanel.TheSet.Width > 0 && ThePanel.TheSet.Height > 0 && glControl1.Width > 0 && glControl1.Height > 0)
            {
                double A1 = (ThePanel.TheSet.Width + 8) / (ThePanel.TheSet.Height + 8);
                double A2 = glControl1.Width / glControl1.Height;

                Zoom = Math.Min(glControl1.Width / (ThePanel.TheSet.Width + 8), glControl1.Height / (ThePanel.TheSet.Height + 8));

                CenterPoint.X = ThePanel.TheSet.Width / 2;
                CenterPoint.Y = ThePanel.TheSet.Height / 2;
            }
            else
            {
                Zoom = 1;
                CenterPoint = new PointD(0, 0);
            }

            UpdateScrollers();
        }

        private void UpdateScrollers()
        {
            double hratio = glControl1.Width / (ThePanel.TheSet.Width * Zoom);
            double vratio = glControl1.Height / (ThePanel.TheSet.Height * Zoom);

            if (hratio > 1)
            {
                hScrollBar1.Visible = false;
            }
            else
            {
                double scrollablemm = (1 - hratio) * (ThePanel.TheSet.Width + 6);
                // Console.WriteLine("{0} mm in X", scrollablemm);
                hScrollBar1.Maximum = (int)Math.Ceiling(scrollablemm);
                hScrollBar1.LargeChange = 1;
                hScrollBar1.Minimum = 0;
                hScrollBar1.Value = 0;
                hScrollBar1.Update();
                hScrollBar1.Visible = true;
            }


            if (vratio > 1)
            {
                vScrollBar1.Visible = false;
            }
            else
            {
                double scrollablemm = (1 - vratio) * (ThePanel.TheSet.Height + 6);
                //  Console.WriteLine("{0} mm in Y", scrollablemm);
                vScrollBar1.LargeChange = 1;

                vScrollBar1.Visible = true;
                vScrollBar1.Maximum = (int)Math.Ceiling(scrollablemm);
                vScrollBar1.Minimum = 0;
                vScrollBar1.Value = 0;
            }
        }

        public bool AutoUpdate = false;

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (ShapeMarkedForUpdate && (AutoUpdate || ForceShapeUpdate))
            {
                //Console.WriteLine("updating shape..");
                ThePanel.UpdateShape(); // check if needed?
                ShapeMarkedForUpdate = false;
            }

            glControl1.MakeCurrent();
            DrawingScale = Math.Min(glControl1.Width, glControl1.Height) / (float)(Math.Max(ThePanel.TheSet.Height, ThePanel.TheSet.Width) + 10);
            GraphicsInterface GI = (GraphicsInterface)new GLGraphicsInterface(0, 0, glControl1.Width, glControl1.Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-glControl1.Width / 2, glControl1.Width / 2, glControl1.Height / 2, -glControl1.Height / 2, -100, 100);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GI.ScaleTransform((float)Zoom, -(float)Zoom);
            GI.TranslateTransform(-(float)(CenterPoint.X), -(float)(CenterPoint.Y));

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

            
            ThePanel.DrawBoardBitmap(1.0f, GI, glControl1.Width, glControl1.Height, SelectedInstance, HoverShape, SnapDistance());
            glControl1.SwapBuffers();
        }

        private void glControl1_MouseDown(object sender, MouseEventArgs e)
        {
            DrawingScale = Math.Min(glControl1.Width, glControl1.Height) / 110.0f;
            DoMouseDown(e);
        }

        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            DrawingScale = Math.Min(glControl1.Width, glControl1.Height) / 110.0f;
            DoMouseMove(e);
        }

        private void glControl1_MouseUp(object sender, MouseEventArgs e)
        {
            DrawingScale = Math.Min(glControl1.Width, glControl1.Height) / 110.0f;
            DoMouseUp(e);
        }

       
        private void exportBoardImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedInstance == null) return;
            if (SelectedInstance.GetType() == typeof(GerberInstance))
            {
                string path = (SelectedInstance as GerberInstance).GerberPath;
                try
                {
                    System.Windows.Forms.SaveFileDialog OFD = new System.Windows.Forms.SaveFileDialog();

                    OFD.DefaultExt = "";
                    if (OFD.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                    Console.WriteLine("path selected: {0}", path);

                    GerberImageCreator GIC = new GerberImageCreator();

                    foreach (var a in Directory.GetFiles(path, "*.*"))
                    {
                        GIC.AddBoardToSet(a);
                    }

                    GIC.WriteImageFiles(OFD.FileName);

                }
                catch (Exception)
                {
                }
            }
        }

        public void glControl1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                Point DropPointPre = glControl1.PointToClient(new Point(e.X, e.Y));
                var DropPoint = MouseToMM(new PointD(DropPointPre.X, DropPointPre.Y));


                string[] D = e.Data.GetData(DataFormats.FileDrop) as string[];
                foreach (string S in D)
                {
                    if (Directory.Exists(S) || (File.Exists(S) && (Path.GetExtension(S).ToLower() == ".zip" || Path.GetExtension(S).ToLower() == "zip")))
                    {
                        Console.WriteLine("Adding dropped folder: {0}", S);
                        var R = ThePanel.AddGerberFolder(S);
                        foreach (var s in R)
                        {
                            GerberInstance GI = new GerberInstance() { GerberPath = s };
                            GI.Center = DropPoint.ToF();
                            ThePanel.TheSet.Instances.Add(GI);
                            SelectedInstance = GI;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Dropped item {0} is not a folder! ignoring!", S);
                    }
                }
                TV.BuildTree(this, ThePanel.TheSet);
                Redraw(true, true);
            }
        }

        private void glControl1_DragEnter(object sender, DragEventArgs e)
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


        private void naiveRectanglePackerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ThePanel.RectanglePack();
            Redraw(true);

        }

        private void maxRectsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ThePanel.MaxRectPack();
            Redraw(true);

        }

        private void panelPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PanelProperties PP = new PanelProperties(ThePanel);
            PP.ShowDialog();
            Redraw(true);
        }

        private void deleteAllBreaktabsWithErrorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ThePanel.RemoveAllTabs(true);
            Redraw(true);

        }

        private void insertBoardJoinToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            AddTab(new PointD(0, 0));
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ThePanel.BuildAutoTabs();// GenerateTabLocations();
            Redraw(true);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            ThePanel.RemoveAllTabs(false);
            Redraw(true);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            ThePanel.RemoveAllTabs(true);
            Redraw(true);
        }

        private void addGerberFolderToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var R = ThePanel.AddGerberFolder(folderBrowserDialog1.SelectedPath);
                foreach (var s in R)
                {
                    GerberInstance GI = new GerberInstance() { GerberPath = s };
                    ThePanel.TheSet.Instances.Add(GI);
                    SelectedInstance = GI;
                }
                TV.BuildTree(this, ThePanel.TheSet);
                Redraw(true);
            }
        }

        private void mergeOverlappingBreaktabsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ThePanel.MergeOverlappingTabs();
            Redraw(true);
        }

        private void doItAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ThePanel.RemoveAllTabs();
            ThePanel.GenerateTabLocations();
            ThePanel.RemoveAllTabs(true);
            ThePanel.MergeOverlappingTabs();

            Redraw(true);
        }

        private void zoomToFitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ZoomToFit();
            Redraw(false);
        }

        private void scale11ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Zoom1to1();
            Redraw(false);
        }

        private void Zoom1to1()
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                try
                {
                    var dx = g.DpiX;
                    var dy = g.DpiY;
                    Zoom = 1 * dx / 25.4;
                    CenterPoint.X = ThePanel.TheSet.Width / 2;
                    CenterPoint.Y = ThePanel.TheSet.Height / 2;
                }
                catch (Exception)
                {
                    Zoom = 1;
                    CenterPoint = new PointD(0, 0);
                }
            }
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            //Console.Write("newval: {0}", vScrollBar1.Value);

            CenterPoint.Y = -vScrollBar1.Value + (vScrollBar1.Maximum / 2) + ThePanel.TheSet.Height / 2;
            Redraw(false);

        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            //  Console.Write("newval: {0}", hScrollBar1.Value);
            CenterPoint.X = hScrollBar1.Value - (hScrollBar1.Maximum / 2) + ThePanel.TheSet.Width / 2;
            Redraw(false);
        }

        private void generateSilkscreenLayerOffsetArtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedInstance == null) return;
            if (SelectedInstance.GetType() == typeof(GerberInstance))
            {
                string path = (SelectedInstance as GerberInstance).GerberPath;
                GerberLibrary.ArtWork.Functions.CreateArtLayersForFolder(path, GerberLibrary.ArtWork.ArtLayerStyle.CheckerField);
            }
        }

        private void glControl1_KeyDown(object sender, KeyEventArgs e)
        {            
            switch (e.KeyCode)
            {
                case Keys.T:
                    AddTab(LastMouseMove);
                    break;

                case Keys.Delete:
                    if (SelectedInstance != null)
                    {
                        ThePanel.RemoveInstance(SelectedInstance);
                        Redraw(true);
                    }
                    break;

                case Keys.Add:
                case Keys.Oemplus:
                    Zoom *= 1.05;
                    UpdateScrollers();
                    Redraw(false);
                    break;

                case Keys.OemMinus:
                case Keys.Subtract:
                    Zoom *= 0.95;
                    UpdateScrollers();
                    Redraw(false);
                    break;

                default:
                    if (SelectedInstance != null)
                    {
                        switch (e.KeyCode)
                        {
                            case Keys.Up:
                                SelectedInstance.Center.Y -= (float)SnapDistance();
                                Redraw(true);
                                break;
                            case Keys.Down:
                                SelectedInstance.Center.Y += (float)SnapDistance();
                                Redraw(true);
                                break;
                            case Keys.Left:
                                SelectedInstance.Center.X -= (float)SnapDistance();
                                Redraw(true);
                                break;
                            case Keys.Right:
                                SelectedInstance.Center.X += (float)SnapDistance();
                                Redraw(true);
                                break;
                        }
                    }
                    break;
            }
        }

        private void generateArtOffsetCurvesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedInstance == null) return;
            if (SelectedInstance.GetType() == typeof(GerberInstance))
            {
                string path = (SelectedInstance as GerberInstance).GerberPath;
                GerberLibrary.ArtWork.Functions.CreateArtLayersForFolder(path, GerberLibrary.ArtWork.ArtLayerStyle.OffsetCurves_GoldfishBoard);
            }
        }

        private void generateArtFieldLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedInstance == null) return;
            if (SelectedInstance.GetType() == typeof(GerberInstance))
            {
                string path = (SelectedInstance as GerberInstance).GerberPath;
                GerberLibrary.ArtWork.Functions.CreateArtLayersForFolder(path, GerberLibrary.ArtWork.ArtLayerStyle.FlowField);
            }

        }

        private void addInstanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ZoomToFit();
        }

        private void generateArtReactedBlobsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedInstance == null) return;
            if (SelectedInstance.GetType() == typeof(GerberInstance))
            {
                string path = (SelectedInstance as GerberInstance).GerberPath;
                GerberLibrary.ArtWork.Functions.CreateArtLayersForFolder(path, GerberLibrary.ArtWork.ArtLayerStyle.ReactDiffuse);
            }
        }

        private void generateArtPrototypeStripToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedInstance == null) return;
            if (SelectedInstance.GetType() == typeof(GerberInstance))
            {
                string path = (SelectedInstance as GerberInstance).GerberPath;
                GerberLibrary.ArtWork.Functions.CreateArtLayersForFolder(path, GerberLibrary.ArtWork.ArtLayerStyle.PrototypeEdge);
            }
        }

       

        private void ProcessButton_Click_1(object sender, EventArgs e)
        {
            if (ShapeMarkedForUpdate)
            {
                ThePanel.UpdateShape(); ShapeMarkedForUpdate = false;
                Redraw(false);
                ProcessButton.Enabled = false;
            }
        }

        private void GerberPanelize_Resize(object sender, EventArgs e)
        {
            ZoomToFit();
        }
        

        void UpdateAutoProcessButton()
        {
            if (AutoUpdate)
            {
                AutoUpdate = false;
                AutoProcess.BackColor = Color.FromKnownColor(KnownColor.Control);
                if (ShapeMarkedForUpdate)
                {
                    ProcessButton.Enabled = true;
                }
            }
            else
            {
                AutoUpdate = true;
                if (ShapeMarkedForUpdate)
                {
                    ThePanel.UpdateShape(); ShapeMarkedForUpdate = false;
                    Redraw(false);


                }
                ProcessButton.Enabled = false;
                AutoProcess.BackColor = Color.Gold;
            }
        }

        private void AutoProcess_Click(object sender, EventArgs e)
        {
            UpdateAutoProcessButton();
        }
    }
}
