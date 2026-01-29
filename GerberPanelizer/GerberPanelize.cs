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
        bool Panning = false;
        PointD PanStartPoint = new PointD(0, 0);

        // Selection
        public List<AngledThing> SelectedInstances = new List<AngledThing>();
        private Dictionary<AngledThing, PointD> DragOriginalPositions = new Dictionary<AngledThing, PointD>();
        private bool IsBoxSelecting = false;
        private PointD BoxSelectStart = new PointD();
        private PointD BoxSelectCurrent = new PointD();

        private float DrawingScale;
        public double Zoom = 1;
        public double TargetZoom = 1;
        public PointD CenterPoint = new PointD(0, 0);

        // Undo/Redo
        public List<string> UndoStack = new List<string>();
        public List<string> RedoStack = new List<string>();
        private string PendingUndoState = null;

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
            Gerber.ArcQualityScaleFactor = 20;
            InitializeComponent();
            glControl1.MouseWheel += glControl1_MouseWheel;
            RotateLeftHover.Visible = false;
            RotateRightHover.Visible = false;

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

            PointD MMToMouse(PointD MM)
        {
            PointD P = new PointD(MM.X, MM.Y);
            P.X -= (float)CenterPoint.X;
            P.Y -= (float)CenterPoint.Y;

            P.X *= (float)Zoom;
            P.Y *= (float)Zoom;

            P.Y *= -1;

            P.X += glControl1.Width / 2;
            P.Y += glControl1.Height / 2;


            
            return P;


        }

        PointD MouseToMM(PointD Mouse)
        {
            PointD P = new PointD(Mouse.X, Mouse.Y);
            P.X -= glControl1.Width / 2;
            P.Y -= glControl1.Height / 2;
            P.Y *= -1;
            P.X /= (float)Zoom;
            P.Y /= (float)Zoom;
            P.X += (float)CenterPoint.X;
            P.Y += (float)CenterPoint.Y;

            return P;

        }

        public void UpdateHoverControls()
        {
            return;

            // todo -> redo these in pure GL.. windows controls interfere too much with the drawing resulting in glitches. 

            if (SelectedInstance != null)
            {
                GerberInstance GI = SelectedInstance as GerberInstance;
                if (GI == null)
                {
                    RotateLeftHover.Visible = false;
                    RotateRightHover.Visible = false;

                }
                else
                {
                    RotateLeftHover.Visible = true;
                    RotateRightHover.Visible = true;
                    var TL = MMToMouse(GI.BoundingBox.TopLeft);
                    var BR = MMToMouse(GI.BoundingBox.BottomRight);
                    int W = RotateLeftHover.Width;
                    int H = RotateLeftHover.Height;

                    RotateLeftHover.Top = Math.Max(0, Math.Min(glControl1.Height - H, (int)(TL.Y - RotateLeftHover.Height)));
                    RotateLeftHover.Left = Math.Max(0, Math.Min(glControl1.Width - W, (int)(TL.X - RotateLeftHover.Width)));
                    RotateRightHover.Top = Math.Max(0, Math.Min(glControl1.Height - H, (int)(TL.Y - RotateLeftHover.Height)));
                    RotateRightHover.Left = Math.Max(0, Math.Min(glControl1.Width - W, (int)(BR.X + 1)));
                }
            }
            else
            {
                RotateLeftHover.Visible = false;
                RotateRightHover.Visible = false;

            }
        }

        internal void SetSelectedInstance(AngledThing gerberInstance)
        {
            
            SelectedInstance = gerberInstance;

            UpdateHoverControls();

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
            ThePanel.LoadFile(new StandardConsoleLog(), filename);
            ThePanel.UpdateShape(new StandardConsoleLog()); 
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
            try
            {
                folderBrowserDialog2.SelectedPath = ThePanel.TheSet.LastExportFolder;
            }
            catch (Exception)
            {

            }
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

        public class ProgressForward : GerberLibrary.ProgressLog
        {
            public Progress parent;
            public ProgressForward(Progress p)
            {
                parent = p;
            }
            public override void AddString(string text, float progress = -1)
            {
                parent.AddString(text, progress);
            }
        }

        public void ExportThreadFunc()
        {

            ThePanel.SaveGerbersToFolder(BaseName, ExportFolder, new ProgressForward(ProgressDialog), ThePanel.TheSet.CopyOutlineToTopSilkscreen,true,true, true, BaseName+"_combined");
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
            CheckAndResizeCanvas();
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
            ThePanel.UpdateShape(new StandardConsoleLog());
            Redraw(true, true);
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
            if (e.Button == MouseButtons.Middle)
            {
                Panning = true;
                PanStartPoint = new PointD(e.X, e.Y);
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var clickedInstance = ThePanel.FindOutlineUnderPoint(MouseToMM(new PointD(e.X, e.Y)));
                
                if (clickedInstance != null)
                {
                    if (ModifierKeys.HasFlag(Keys.Control))
                    {
                        if (SelectedInstances.Contains(clickedInstance))
                        {
                            SelectedInstances.Remove(clickedInstance);
                            SelectedInstance = SelectedInstances.LastOrDefault(); // or null
                            MouseCapture = false; // Don't drag if deselecting
                        }
                        else
                        {
                            SelectedInstances.Add(clickedInstance);
                            SelectedInstance = clickedInstance;
                            MouseCapture = true; 
                        }
                    }
                    else
                    {
                        if (!SelectedInstances.Contains(clickedInstance))
                        {
                            SelectedInstances.Clear();
                            SelectedInstances.Add(clickedInstance);
                        }
                        // If it IS contained, we keep selection to allow dragging multiple
                        SelectedInstance = clickedInstance; 
                        MouseCapture = true;
                    }
                    
                    if (MouseCapture)
                    {
                        DragStartCoord = new PointD(e.X, e.Y);
                        DragOriginalPositions.Clear();
                        foreach(var inst in SelectedInstances)
                        {
                            DragOriginalPositions[inst] = new PointD(inst.Center.X, inst.Center.Y); 
                        }
                        DragInstanceOriginalPosition = SelectedInstance.Center; // Legacy support
                        
                        // Capture state for Undo if we actually move
                        try {
                            PendingUndoState = SerializeState();
                        } catch(Exception) { PendingUndoState = null; }
                    }
                    
                    SetSelectedInstance(SelectedInstance);
                }
                else // Clicked on empty space
                {
                    if (!ModifierKeys.HasFlag(Keys.Control))
                    {
                        SelectedInstances.Clear();
                        SetSelectedInstance(null);
                    }
                    IsBoxSelecting = true;
                    BoxSelectStart = new PointD(e.X, e.Y);
                    BoxSelectCurrent = new PointD(e.X, e.Y);
                }
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {

                SelectedInstance = ThePanel.FindOutlineUnderPoint(MouseToMM(new PointD(e.X, e.Y)));

                ContextStartCoord = new PointD(e.X, e.Y);

                if (SelectedInstance != null)
                {
                    if (!SelectedInstances.Contains(SelectedInstance))
                    {
                         SelectedInstances.Clear();
                         SelectedInstances.Add(SelectedInstance);
                         SetSelectedInstance(SelectedInstance);
                    }
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
            if (Panning && e.Button == MouseButtons.Middle)
            {
                Panning = false;
            }

            if (IsBoxSelecting && e.Button == MouseButtons.Left)
            {
                IsBoxSelecting = false;
                
                // Calculate box in MM
                PointD StartMM = MouseToMM(BoxSelectStart);
                PointD EndMM = MouseToMM(BoxSelectCurrent);
                double minX = Math.Min(StartMM.X, EndMM.X);
                double maxX = Math.Max(StartMM.X, EndMM.X);
                double minY = Math.Min(StartMM.Y, EndMM.Y);
                double maxY = Math.Max(StartMM.Y, EndMM.Y);
                Bounds selectionBounds = new Bounds();
                selectionBounds.FitPoint(minX, minY);
                selectionBounds.FitPoint(maxX, maxY);

                if (!ModifierKeys.HasFlag(Keys.Control))
                {
                    SelectedInstances.Clear();
                }

                foreach(var inst in ThePanel.TheSet.Instances)
                {
                    if (selectionBounds.Contains(inst.Center)) 
                    {
                        if (!SelectedInstances.Contains(inst)) SelectedInstances.Add(inst);
                    }
                }
                 foreach(var tab in ThePanel.TheSet.Tabs)
                {
                    if (selectionBounds.Contains(tab.Center)) 
                    {
                         if (!SelectedInstances.Contains(tab)) SelectedInstances.Add(tab);
                    }
                }
                
                SelectedInstance = SelectedInstances.LastOrDefault();
                SetSelectedInstance(SelectedInstance);
                Redraw(false);
                return; // Early return to skip Drag logic
            }

            if (MouseCapture)
            {
                MouseCapture = false;
                PointD Delta = new PointD(e.X, e.Y) - DragStartCoord;
                if (Delta.Length() == 0)
                {
                     // Clicked, not dragged. 
                     PendingUndoState = null; // discard
                     // Clicked, not dragged. Logic handled in MouseDown mostly.
                     // But if we were clicking on an already selected item without control, we might want to clear others?
                     // Standard behavior: 
                     // Click on selected item -> MouseDown: keeps others selected (in case of drag).
                     // MouseUp: if no drag, clear others?
                     if (!ModifierKeys.HasFlag(Keys.Control))
                     {
                         if(SelectedInstance != null && SelectedInstances.Count > 1) {
                             SelectedInstances.Clear();
                             SelectedInstances.Add(SelectedInstance);
                             SetSelectedInstance(SelectedInstance);
                         }
                     }
                      Redraw(false);

                }
                else
                {
                    // Dragged - Commit Undo
                    if (PendingUndoState != null)
                    {
                        UndoStack.Add(PendingUndoState);
                        if (UndoStack.Count > 20) UndoStack.RemoveAt(0);
                        RedoStack.Clear();
                        PendingUndoState = null;
                    }
                    
                    // Dragged
                    bool fullRedraw = false;
                    foreach(var inst in SelectedInstances)
                    {
                        var GI = inst as GerberInstance;
                        if (GI != null)
                        {
                            GI.RebuildTransformed(ThePanel.GerberOutlines[GI.GerberPath], ThePanel.TheSet.ExtraTabDrillDistance);
                            fullRedraw = true;
                        }
                        if (inst is BreakTab)
                        {
                            fullRedraw = true;
                        }
                    }
                    if (SelectedInstance != null) ID.UpdateBoxes(this);

                    Redraw(fullRedraw);
                }
            }
        }

        PointD LastMouseMove = new PointD(0, 0);
        public bool SuspendRedraw = false;

        private void DoMouseMove(MouseEventArgs e)
        {
            LastMouseMove = new PointD(e.X, e.Y);
            if (Panning)
            {
                PointD Current = new PointD(e.X, e.Y);
                PointD Delta = Current - PanStartPoint;

                CenterPoint.X -= Delta.X / Zoom;
                CenterPoint.Y += Delta.Y / Zoom;

                PanStartPoint = Current;
                Redraw(false);
                UpdateScrollers();
                return;
            }

            if (IsBoxSelecting)
            {
                BoxSelectCurrent = new PointD(e.X, e.Y);
                Redraw(false);
                return;
            }

            if (MouseCapture && SelectedInstances.Count > 0)
            {
                PointD Delta = new PointD(e.X, e.Y) - DragStartCoord;
                Delta.X /= Zoom;
                Delta.Y /= -Zoom;
                
                foreach(var inst in SelectedInstances)
                {
                    if (DragOriginalPositions.ContainsKey(inst))
                    {
                        inst.Center = Snap(DragOriginalPositions[inst] + Delta);
                    }
                }

                if (SelectedInstance != null) {
                     UpdateHoverControls();
                }
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
                TargetZoom = Zoom;

                CenterPoint.X = ThePanel.TheSet.Width / 2;
                CenterPoint.Y = ThePanel.TheSet.Height / 2;
            }
            else
            {
                Zoom = 1;
                TargetZoom = 1;
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
                ThePanel.UpdateShape(new StandardConsoleLog()); // check if needed?
                ShapeMarkedForUpdate = false;
                ForceShapeUpdate = false;
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
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

            
            ThePanel.DrawBoardBitmap(1.0f, GI, glControl1.Width, glControl1.Height, SelectedInstance, HoverShape, SnapDistance(), Zoom);
    
    // Highlight other selected instances
    foreach(var inst in SelectedInstances)
    {
        if (inst != SelectedInstance)
        {
             ThePanel.RenderInstance(GI, DrawingScale, Color.Black, inst, false, true, false, Zoom);
        }
    }

    if (IsBoxSelecting)
    {
         GL.MatrixMode(MatrixMode.Modelview);
         GL.LoadIdentity();
         // Coordinates are in screen space centered? No, Ortho was set to centered.
         // Ortho is (-w/2, w/2, h/2, -h/2)
         // Mouse coordinates are (0..w, 0..h) from top left?
         // WinForms mouse: 0,0 is top left.
         // GL Ortho: 0,0 is center. Y is up.
         // Need to map Mouse to GL.
         
         double x1 = BoxSelectStart.X - glControl1.Width / 2.0;
         double y1 = (BoxSelectStart.Y - glControl1.Height / 2.0);
         double x2 = BoxSelectCurrent.X - glControl1.Width / 2.0;
         double y2 = (BoxSelectCurrent.Y - glControl1.Height / 2.0);

        GL.Enable(EnableCap.Blend);
         GL.Color4(0.0f, 0.5f, 1.0f, 0.3f);
         GL.Begin(PrimitiveType.Quads);
         GL.Vertex2(x1, y1);
         GL.Vertex2(x2, y1);
         GL.Vertex2(x2, y2);
         GL.Vertex2(x1, y2);
         GL.End();

         GL.Color4(0.0f, 0.5f, 1.0f, 0.8f);
         GL.LineWidth(1.0f);
         GL.Begin(PrimitiveType.LineLoop);
         GL.Vertex2(x1, y1);
         GL.Vertex2(x2, y1);
         GL.Vertex2(x2, y2);
         GL.Vertex2(x1, y2);
         GL.End();
    }

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
                        GIC.AddBoardToSet(a, new StandardConsoleLog());
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
                    if (File.Exists(S) && (Path.GetExtension(S).ToLower() == ".gerberset" || Path.GetExtension(S).ToLower() == ".xml"))
                    {
                        LoadFile(S);
                        return;
                    }

                    if (Directory.Exists(S) || (File.Exists(S) && (Path.GetExtension(S).ToLower() == ".zip" || Path.GetExtension(S).ToLower() == "zip")))
                    {
                        Console.WriteLine("Adding dropped folder: {0}", S);
                        if (BaseName == "Untitled") BaseName = Path.GetFileNameWithoutExtension(S);
                        var R = ThePanel.AddGerberFolder(new StandardConsoleLog(), S);
                        foreach (var s in R)
                        {
                            GerberInstance GI = new GerberInstance() { GerberPath = s };
                            GI.Center = DropPoint;
                            ThePanel.TheSet.Instances.Add(GI);
                            SelectedInstance = GI;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Dropped item {0} is not a folder! ignoring!", S);
                    }
                }
                
                ThePanel.UpdateShape(new StandardConsoleLog());
                CheckAndResizeCanvas();
                TV.BuildTree(this, ThePanel.TheSet);
                ZoomToFit();
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
            ThePanel.BuildAutoTabs(new StandardConsoleLog());// GenerateTabLocations();
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
                var R = ThePanel.AddGerberFolder(new StandardConsoleLog(),folderBrowserDialog1.SelectedPath);
                foreach (var s in R)
                {
                    GerberInstance GI = new GerberInstance() { GerberPath = s };
                    ThePanel.TheSet.Instances.Add(GI);
                    SelectedInstance = GI;
                }
                CheckAndResizeCanvas();
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

        private void zoomInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ZoomIn();
        }

        public void ZoomIn()
        {
            TargetZoom *= 1.5;
            StartZoomAnimation(glControl1.Width / 2, glControl1.Height / 2);
        }

        private void zoomOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ZoomOut();
        }

        public void ZoomOut()
        {
            TargetZoom *= 0.6;
            StartZoomAnimation(glControl1.Width / 2, glControl1.Height / 2);
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
                GerberLibrary.ArtWork.Functions.CreateArtLayersForFolder(new StandardConsoleLog(), path, GerberLibrary.ArtWork.ArtLayerStyle.CheckerField);
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
                        RemoveInstance(SelectedInstance);
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

                case Keys.R:
                    if (HoverShape != null && HoverShape is BreakTab)
                    {
                        var BT = HoverShape as BreakTab;
                        BT.Radius -= 0.25f;
                        if (BT.Radius < 0.5f) BT.Radius = 0.5f;
                        Redraw(true);
                    }
                    else if (SelectedInstance != null && SelectedInstance is BreakTab)
                    {
                         var BT = SelectedInstance as BreakTab;
                        BT.Radius -= 0.25f;
                        if (BT.Radius < 0.5f) BT.Radius = 0.5f;
                        Redraw(true);
                    }
                    break;

                case Keys.Y:
                    if (HoverShape != null && HoverShape is BreakTab)
                    {
                         var BT = HoverShape as BreakTab;
                         BT.Radius += 0.25f;
                         Redraw(true);
                    }
                    else if (SelectedInstance != null && SelectedInstance is BreakTab)
                    {
                         var BT = SelectedInstance as BreakTab;
                         BT.Radius += 0.25f;
                         Redraw(true);
                    }
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
                GerberLibrary.ArtWork.Functions.CreateArtLayersForFolder(new StandardConsoleLog(), path, GerberLibrary.ArtWork.ArtLayerStyle.OffsetCurves_GoldfishBoard);
            }
        }

        private void generateArtFieldLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedInstance == null) return;
            if (SelectedInstance.GetType() == typeof(GerberInstance))
            {
                string path = (SelectedInstance as GerberInstance).GerberPath;
                GerberLibrary.ArtWork.Functions.CreateArtLayersForFolder(new StandardConsoleLog(), path, GerberLibrary.ArtWork.ArtLayerStyle.FlowField);
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
                GerberLibrary.ArtWork.Functions.CreateArtLayersForFolder(new StandardConsoleLog(), path, GerberLibrary.ArtWork.ArtLayerStyle.ReactDiffuse);
            }
        }

        private void generateArtPrototypeStripToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedInstance == null) return;
            if (SelectedInstance.GetType() == typeof(GerberInstance))
            {
                string path = (SelectedInstance as GerberInstance).GerberPath;
                GerberLibrary.ArtWork.Functions.CreateArtLayersForFolder(new StandardConsoleLog(), path, GerberLibrary.ArtWork.ArtLayerStyle.PrototypeEdge);
            }
        }

       

        private void ProcessButton_Click_1(object sender, EventArgs e)
        {
            if (ShapeMarkedForUpdate)
            {
                ThePanel.UpdateShape(new StandardConsoleLog()); ShapeMarkedForUpdate = false;
                Redraw(false);
                ProcessButton.Enabled = false;
            }
        }

        private void GerberPanelize_Resize(object sender, EventArgs e)
        {
            //ZoomToFit();
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
                    ThePanel.UpdateShape(new StandardConsoleLog()); ShapeMarkedForUpdate = false;
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

        private void CheckAndResizeCanvas()
        {
            double maxX = ThePanel.TheSet.Width;
            double maxY = ThePanel.TheSet.Height;
            bool changed = false;

            foreach (var i in ThePanel.TheSet.Instances)
            {
                var bb = i.BoundingBox;
                if (bb.BottomRight.X > maxX) { maxX = bb.BottomRight.X + 2; changed = true; }
                if (bb.TopLeft.X > maxX) { maxX = bb.TopLeft.X + 2; changed = true; }

                if (bb.BottomRight.Y > maxY) { maxY = bb.BottomRight.Y + 2; changed = true; }
                if (bb.TopLeft.Y > maxY) { maxY = bb.TopLeft.Y + 2; changed = true; }
            }

            if (changed)
            {
                ThePanel.TheSet.Width = maxX;
                ThePanel.TheSet.Height = maxY;
                UpdateScrollers();
                Redraw(true);
            }
        }

        private void RotateRightHover_Click(object sender, EventArgs e)
        {

        }

        private void RotateLeftHover_Click(object sender, EventArgs e)
        {

        }
        
        private void glControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                double det = e.Delta > 0 ? 1.5 : 0.6;
                TargetZoom *= det;
                StartZoomAnimation(e.X, e.Y);
            }
        }

        private void StartZoomAnimation(int mouseX, int mouseY)
        {
             ZoomAnimationCenter = new PointD(mouseX - glControl1.Width / 2, (mouseY - glControl1.Height / 2) * -1);
             ZoomAnimationTimer.Enabled = true;
        }

        // Clipboard
        public void CopySelection()
        {
            if (SelectedInstances.Count == 0) return;
            ClipboardData data = new ClipboardData();
            foreach(var inst in SelectedInstances)
            {
                if (inst is GerberInstance) data.Instances.Add(inst as GerberInstance);
                if (inst is BreakTab) data.Tabs.Add(inst as BreakTab);
            }
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ClipboardData));
                using (StringWriter writer = new StringWriter())
                {
                    serializer.Serialize(writer, data);
                    Clipboard.SetText(writer.ToString());
                }
            }
            catch(Exception e) { Console.WriteLine("Copy failed: " + e.Message); }
        }

        public void PasteSelection()
        {
            if (!Clipboard.ContainsText()) return;
            try
            {
                PushUndo();
                string xml = Clipboard.GetText();
                XmlSerializer serializer = new XmlSerializer(typeof(ClipboardData));
                using (StringReader reader = new StringReader(xml))
                {
                    ClipboardData data = (ClipboardData)serializer.Deserialize(reader);
                    SelectedInstances.Clear();
                    
                    foreach(var inst in data.Instances)
                    {
                        inst.Center.X += 5;
                        inst.Center.Y += 5;
                        // We need to re-link or re-load the outlines?
                        // GerberInstance has GerberPath
                        // We need to ensure loaded outlines has it?
                        // Actually, if we just add it to Instances, UpdateShape will handle it if LoadedOutlines has it.
                        // Assuming the file is still loaded.
                        if (!ThePanel.TheSet.LoadedOutlines.Contains(inst.GerberPath))
                        {
                            ThePanel.TheSet.LoadedOutlines.Add(inst.GerberPath);
                            // We might need to actually reload the file content if it's not in GerberOutlines?
                            // But ThePanel.TheSet.LoadedOutlines is just a list of strings.
                            // ThePanel.AddGerberFolder loads it.
                            // If we paste from same session, current loaded outlines is fine.
                            // If we paste from another session/instance, we might be missing the file?
                            // For now assume same session or path valid.
                        }
                        
                        ThePanel.TheSet.Instances.Add(inst);
                        SelectedInstances.Add(inst);
                    }
                    foreach(var tab in data.Tabs)
                    {
                        tab.Center.X += 5;
                        tab.Center.Y += 5;
                        ThePanel.TheSet.Tabs.Add(tab);
                        SelectedInstances.Add(tab);
                    }
                    SelectedInstance = SelectedInstances.LastOrDefault();
                    SetSelectedInstance(SelectedInstance);
                    ThePanel.UpdateShape(new StandardConsoleLog());
                    Redraw(true);
                }
            }
             catch(Exception e) { Console.WriteLine("Paste failed: " + e.Message); }
        }

        public void CutSelection()
        {
            CopySelection();
            DeleteSelection();
        }

        public void DeleteSelection()
        {
            if (SelectedInstances.Count == 0) return;
            PushUndo();
            foreach(var inst in SelectedInstances)
            {
                if (inst is GerberInstance) ThePanel.TheSet.Instances.Remove(inst as GerberInstance);
                if (inst is BreakTab) ThePanel.TheSet.Tabs.Remove(inst as BreakTab);
            }
            SelectedInstances.Clear();
            SelectedInstance = null;
            SetSelectedInstance(null);
            ThePanel.UpdateShape(new StandardConsoleLog());
            Redraw(true);
        }

        // Undo/Redo Logic
        public void PushUndo()
        {
            try {
                UndoStack.Add(SerializeState());
                if (UndoStack.Count > 20) UndoStack.RemoveAt(0);
                RedoStack.Clear();
            } catch { }
        }

        public void PerformUndo()
        {
            if (UndoStack.Count == 0) return;
            string state = UndoStack.Last();
            UndoStack.RemoveAt(UndoStack.Count - 1);
            
            RedoStack.Add(SerializeState());
            RestoreState(state);
            Redraw(true);
        }

        public void PerformRedo()
        {
            if (RedoStack.Count == 0) return;
            string state = RedoStack.Last();
            RedoStack.RemoveAt(RedoStack.Count - 1);
            
            UndoStack.Add(SerializeState());
            RestoreState(state);
            Redraw(true);
        }

        private string SerializeState()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GerberLayoutSet));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, ThePanel.TheSet);
                return writer.ToString();
            }
        }

        private void RestoreState(string state)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GerberLayoutSet));
            using (StringReader reader = new StringReader(state))
            {
                GerberLayoutSet set = (GerberLayoutSet)serializer.Deserialize(reader);
                ThePanel.TheSet = set;
                // We need to restore selected instances references?
                // IDs? They are new objects.
                SelectedInstances.Clear();
                SelectedInstance = null;
                SetSelectedInstance(null);
                ThePanel.UpdateShape(new StandardConsoleLog());
            }
        }

        public class ClipboardData
        {
            public List<GerberInstance> Instances = new List<GerberInstance>();
            public List<BreakTab> Tabs = new List<BreakTab>();
        }

        private void autofitCanvasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AutofitDialog D = new AutofitDialog(this);
            D.Show();
        }

        public void PerformAutofit(double margin, double moat)
        {
            PushUndo();
            if (ThePanel.TheSet.Instances.Count == 0) return;

            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            foreach (var inst in ThePanel.TheSet.Instances)
            {
                var bb = inst.BoundingBox;
                if (bb.TopLeft.X < minX) minX = bb.TopLeft.X;
                if (bb.BottomRight.X > maxX) maxX = bb.BottomRight.X;
                if (bb.TopLeft.Y < minY) minY = bb.TopLeft.Y;
                if (bb.BottomRight.Y > maxY) maxY = bb.BottomRight.Y;
            }

            // Also check tabs if they are stored separately in the set (though typically they are attached to instances or in a separate list)
            // Looking at GerberPanel.cs, Tabs are property of GerberInstance, but can also be added to TheSet?
            // Actually BreakTabs are added to TheSet.Tabs? No, TheSet is GerberPanelSet? 
            // In GerberPanel.cs: public BreakTab AddTab(PointD center) { ... TheSet.Tabs.Add(BT); ... }
            // So we must check ThePanel.TheSet.Tabs as well.

            foreach (var tab in ThePanel.TheSet.Tabs)
            {
                 // BreakTab has Center and Radius. Bounds are Center +/- Radius.
                 // Assuming BreakTab : AngledThing
                 
                 double r = tab.Radius;
                 if (tab.Center.X - r < minX) minX = tab.Center.X - r;
                 if (tab.Center.X + r > maxX) maxX = tab.Center.X + r;
                 if (tab.Center.Y - r < minY) minY = tab.Center.Y - r;
                 if (tab.Center.Y + r > maxY) maxY = tab.Center.Y + r;
            }

            double targetX = margin + moat;
            double targetY = margin + moat;
            double shiftX = targetX - minX;
            double shiftY = targetY - minY;

            foreach (var inst in ThePanel.TheSet.Instances)
            {
                inst.Center.X += (float)shiftX;
                inst.Center.Y += (float)shiftY;
            }
            
            foreach (var tab in ThePanel.TheSet.Tabs)
            {
                tab.Center.X += (float)shiftX;
                tab.Center.Y += (float)shiftY;
            }


            double contentWidth = maxX - minX;
            double contentHeight = maxY - minY;
            double totalPadding = 2 * (margin + moat);

            ThePanel.TheSet.Width = contentWidth + totalPadding;
            ThePanel.TheSet.Height = contentHeight + totalPadding;

            ThePanel.UpdateShape(new StandardConsoleLog());
            CheckAndResizeCanvas(); // Ensure consistency
            UpdateScrollers();
            CenterPoint = new PointD(ThePanel.TheSet.Width / 2, ThePanel.TheSet.Height / 2);
            ZoomToFit();
            Redraw(true, true);
        }

        PointD ZoomAnimationCenter;
        
        private void ZoomAnimationTimer_Tick(object sender, EventArgs e)
        {
            double diff = TargetZoom - Zoom;
            if (Math.Abs(diff) < 0.001)
            {
                Zoom = TargetZoom;
                ZoomAnimationTimer.Enabled = false;
            }
            else
            {
                var OldZoom = Zoom;
                Zoom += diff * 0.2;
                // var det = Zoom / LastScale;

                // The shift in centerpoint needed to keep the mouse fixed on the same world coordinate is:
                // Shift = MouseScreenRel * (1/OldZoom - 1/NewZoom) 

                double Factor = (1.0 / OldZoom - 1.0 / Zoom);
                CenterPoint.X += ZoomAnimationCenter.X * Factor;
                CenterPoint.Y += ZoomAnimationCenter.Y * Factor;
            }
            UpdateScrollers();
            Redraw(false);
        }
    }
}
