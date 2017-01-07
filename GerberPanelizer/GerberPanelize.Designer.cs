namespace GerberCombinerBuilder
{
    partial class GerberPanelize
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GerberPanelize));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.autosortAlgo1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.naiveRectanglePackerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.maxRectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addGerberFolderToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.breaktabsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertBoardJoinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeOverlappingBreaktabsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.doItAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelPropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zoomToFitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scale11ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog2 = new System.Windows.Forms.FolderBrowserDialog();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addInstanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addBreakTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportBoardImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateSilkscreenLayerOffsetArtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateArtOffsetCurvesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateArtFieldLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateArtReactedBlobsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateArtPrototypeStripToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.milToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.milToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mmToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.mmToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.offToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.glControl1 = new OpenTK.GLControl();
            this.directoryEntry1 = new System.DirectoryServices.DirectoryEntry();
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
            this.menuStrip1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.AllowItemReorder = true;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autosortAlgo1ToolStripMenuItem,
            this.breaktabsToolStripMenuItem,
            this.panelPropertiesToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(632, 28);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.Visible = false;
            // 
            // autosortAlgo1ToolStripMenuItem
            // 
            this.autosortAlgo1ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.naiveRectanglePackerToolStripMenuItem,
            this.maxRectsToolStripMenuItem,
            this.addGerberFolderToolStripMenuItem1});
            this.autosortAlgo1ToolStripMenuItem.Name = "autosortAlgo1ToolStripMenuItem";
            this.autosortAlgo1ToolStripMenuItem.Size = new System.Drawing.Size(134, 24);
            this.autosortAlgo1ToolStripMenuItem.Text = "Board Placement";
            // 
            // naiveRectanglePackerToolStripMenuItem
            // 
            this.naiveRectanglePackerToolStripMenuItem.Name = "naiveRectanglePackerToolStripMenuItem";
            this.naiveRectanglePackerToolStripMenuItem.Size = new System.Drawing.Size(217, 26);
            this.naiveRectanglePackerToolStripMenuItem.Text = "Autopack: Naive";
            this.naiveRectanglePackerToolStripMenuItem.Click += new System.EventHandler(this.naiveRectanglePackerToolStripMenuItem_Click);
            // 
            // maxRectsToolStripMenuItem
            // 
            this.maxRectsToolStripMenuItem.Name = "maxRectsToolStripMenuItem";
            this.maxRectsToolStripMenuItem.Size = new System.Drawing.Size(217, 26);
            this.maxRectsToolStripMenuItem.Text = "Autopack: MaxRects";
            this.maxRectsToolStripMenuItem.Click += new System.EventHandler(this.maxRectsToolStripMenuItem_Click);
            // 
            // addGerberFolderToolStripMenuItem1
            // 
            this.addGerberFolderToolStripMenuItem1.Name = "addGerberFolderToolStripMenuItem1";
            this.addGerberFolderToolStripMenuItem1.Size = new System.Drawing.Size(217, 26);
            this.addGerberFolderToolStripMenuItem1.Text = "Add Gerber Folder";
            this.addGerberFolderToolStripMenuItem1.Click += new System.EventHandler(this.addGerberFolderToolStripMenuItem1_Click);
            // 
            // breaktabsToolStripMenuItem
            // 
            this.breaktabsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertBoardJoinToolStripMenuItem,
            this.toolStripMenuItem1,
            this.toolStripMenuItem2,
            this.toolStripMenuItem3,
            this.mergeOverlappingBreaktabsToolStripMenuItem,
            this.doItAllToolStripMenuItem});
            this.breaktabsToolStripMenuItem.Name = "breaktabsToolStripMenuItem";
            this.breaktabsToolStripMenuItem.Size = new System.Drawing.Size(86, 24);
            this.breaktabsToolStripMenuItem.Text = "Breaktabs";
            // 
            // insertBoardJoinToolStripMenuItem
            // 
            this.insertBoardJoinToolStripMenuItem.Name = "insertBoardJoinToolStripMenuItem";
            this.insertBoardJoinToolStripMenuItem.Size = new System.Drawing.Size(291, 26);
            this.insertBoardJoinToolStripMenuItem.Text = "Insert Breaktab";
            this.insertBoardJoinToolStripMenuItem.Click += new System.EventHandler(this.insertBoardJoinToolStripMenuItem_Click_1);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(291, 26);
            this.toolStripMenuItem1.Text = "Create Breaktabs";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(291, 26);
            this.toolStripMenuItem2.Text = "Delete all Breaktabs";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(291, 26);
            this.toolStripMenuItem3.Text = "Delete all Breaktabs with errors";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // mergeOverlappingBreaktabsToolStripMenuItem
            // 
            this.mergeOverlappingBreaktabsToolStripMenuItem.Name = "mergeOverlappingBreaktabsToolStripMenuItem";
            this.mergeOverlappingBreaktabsToolStripMenuItem.Size = new System.Drawing.Size(291, 26);
            this.mergeOverlappingBreaktabsToolStripMenuItem.Text = "Merge Overlapping Breaktabs";
            this.mergeOverlappingBreaktabsToolStripMenuItem.Click += new System.EventHandler(this.mergeOverlappingBreaktabsToolStripMenuItem_Click);
            // 
            // doItAllToolStripMenuItem
            // 
            this.doItAllToolStripMenuItem.Name = "doItAllToolStripMenuItem";
            this.doItAllToolStripMenuItem.Size = new System.Drawing.Size(291, 26);
            this.doItAllToolStripMenuItem.Text = "Do it all.";
            this.doItAllToolStripMenuItem.Click += new System.EventHandler(this.doItAllToolStripMenuItem_Click);
            // 
            // panelPropertiesToolStripMenuItem
            // 
            this.panelPropertiesToolStripMenuItem.Name = "panelPropertiesToolStripMenuItem";
            this.panelPropertiesToolStripMenuItem.Size = new System.Drawing.Size(127, 24);
            this.panelPropertiesToolStripMenuItem.Text = "Panel Properties";
            this.panelPropertiesToolStripMenuItem.Click += new System.EventHandler(this.panelPropertiesToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zoomToFitToolStripMenuItem,
            this.scale11ToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(53, 24);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // zoomToFitToolStripMenuItem
            // 
            this.zoomToFitToolStripMenuItem.Name = "zoomToFitToolStripMenuItem";
            this.zoomToFitToolStripMenuItem.Size = new System.Drawing.Size(160, 26);
            this.zoomToFitToolStripMenuItem.Text = "Zoom to fit";
            this.zoomToFitToolStripMenuItem.Click += new System.EventHandler(this.zoomToFitToolStripMenuItem_Click);
            // 
            // scale11ToolStripMenuItem
            // 
            this.scale11ToolStripMenuItem.Name = "scale11ToolStripMenuItem";
            this.scale11ToolStripMenuItem.Size = new System.Drawing.Size(160, 26);
            this.scale11ToolStripMenuItem.Text = "Scale 1:1";
            this.scale11ToolStripMenuItem.Click += new System.EventHandler(this.scale11ToolStripMenuItem_Click);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.ShowNewFolderButton = false;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addInstanceToolStripMenuItem,
            this.addBreakTabToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(176, 56);
            // 
            // addInstanceToolStripMenuItem
            // 
            this.addInstanceToolStripMenuItem.Name = "addInstanceToolStripMenuItem";
            this.addInstanceToolStripMenuItem.Size = new System.Drawing.Size(175, 26);
            this.addInstanceToolStripMenuItem.Text = "Add Instance";
            this.addInstanceToolStripMenuItem.Click += new System.EventHandler(this.addInstanceToolStripMenuItem_Click);
            // 
            // addBreakTabToolStripMenuItem
            // 
            this.addBreakTabToolStripMenuItem.Name = "addBreakTabToolStripMenuItem";
            this.addBreakTabToolStripMenuItem.Size = new System.Drawing.Size(175, 26);
            this.addBreakTabToolStripMenuItem.Text = "Add Breaktab";
            this.addBreakTabToolStripMenuItem.Click += new System.EventHandler(this.addBreakTabToolStripMenuItem_Click);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem,
            this.exportBoardImageToolStripMenuItem,
            this.generateSilkscreenLayerOffsetArtToolStripMenuItem,
            this.generateArtOffsetCurvesToolStripMenuItem,
            this.generateArtFieldLinesToolStripMenuItem,
            this.generateArtReactedBlobsToolStripMenuItem,
            this.generateArtPrototypeStripToolStripMenuItem});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(276, 186);
            this.contextMenuStrip2.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip2_Opening);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(275, 26);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // exportBoardImageToolStripMenuItem
            // 
            this.exportBoardImageToolStripMenuItem.Name = "exportBoardImageToolStripMenuItem";
            this.exportBoardImageToolStripMenuItem.Size = new System.Drawing.Size(275, 26);
            this.exportBoardImageToolStripMenuItem.Text = "Export Board Image";
            this.exportBoardImageToolStripMenuItem.Click += new System.EventHandler(this.exportBoardImageToolStripMenuItem_Click);
            // 
            // generateSilkscreenLayerOffsetArtToolStripMenuItem
            // 
            this.generateSilkscreenLayerOffsetArtToolStripMenuItem.Name = "generateSilkscreenLayerOffsetArtToolStripMenuItem";
            this.generateSilkscreenLayerOffsetArtToolStripMenuItem.Size = new System.Drawing.Size(275, 26);
            this.generateSilkscreenLayerOffsetArtToolStripMenuItem.Text = "Generate Art: CheckerField";
            this.generateSilkscreenLayerOffsetArtToolStripMenuItem.Click += new System.EventHandler(this.generateSilkscreenLayerOffsetArtToolStripMenuItem_Click);
            // 
            // generateArtOffsetCurvesToolStripMenuItem
            // 
            this.generateArtOffsetCurvesToolStripMenuItem.Name = "generateArtOffsetCurvesToolStripMenuItem";
            this.generateArtOffsetCurvesToolStripMenuItem.Size = new System.Drawing.Size(275, 26);
            this.generateArtOffsetCurvesToolStripMenuItem.Text = "Generate Art: OffsetCurves";
            this.generateArtOffsetCurvesToolStripMenuItem.Click += new System.EventHandler(this.generateArtOffsetCurvesToolStripMenuItem_Click);
            // 
            // generateArtFieldLinesToolStripMenuItem
            // 
            this.generateArtFieldLinesToolStripMenuItem.Name = "generateArtFieldLinesToolStripMenuItem";
            this.generateArtFieldLinesToolStripMenuItem.Size = new System.Drawing.Size(275, 26);
            this.generateArtFieldLinesToolStripMenuItem.Text = "Generate Art: FieldLines";
            this.generateArtFieldLinesToolStripMenuItem.Click += new System.EventHandler(this.generateArtFieldLinesToolStripMenuItem_Click);
            // 
            // generateArtReactedBlobsToolStripMenuItem
            // 
            this.generateArtReactedBlobsToolStripMenuItem.Name = "generateArtReactedBlobsToolStripMenuItem";
            this.generateArtReactedBlobsToolStripMenuItem.Size = new System.Drawing.Size(275, 26);
            this.generateArtReactedBlobsToolStripMenuItem.Text = "Generate Art: Reacted Blobs";
            this.generateArtReactedBlobsToolStripMenuItem.Click += new System.EventHandler(this.generateArtReactedBlobsToolStripMenuItem_Click);
            // 
            // generateArtPrototypeStripToolStripMenuItem
            // 
            this.generateArtPrototypeStripToolStripMenuItem.Name = "generateArtPrototypeStripToolStripMenuItem";
            this.generateArtPrototypeStripToolStripMenuItem.Size = new System.Drawing.Size(275, 26);
            this.generateArtPrototypeStripToolStripMenuItem.Text = "Generate Art: Prototype Strip";
            this.generateArtPrototypeStripToolStripMenuItem.Click += new System.EventHandler(this.generateArtPrototypeStripToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.AllowMerge = false;
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 295);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(19, 0, 1, 0);
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.statusStrip1.Size = new System.Drawing.Size(632, 26);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.milToolStripMenuItem1,
            this.milToolStripMenuItem,
            this.mmToolStripMenuItem1,
            this.mmToolStripMenuItem,
            this.offToolStripMenuItem});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(84, 24);
            this.toolStripDropDownButton1.Text = "Snap: Off";
            // 
            // milToolStripMenuItem1
            // 
            this.milToolStripMenuItem1.Name = "milToolStripMenuItem1";
            this.milToolStripMenuItem1.Size = new System.Drawing.Size(129, 26);
            this.milToolStripMenuItem1.Text = "50mil";
            this.milToolStripMenuItem1.Click += new System.EventHandler(this.milToolStripMenuItem1_Click);
            // 
            // milToolStripMenuItem
            // 
            this.milToolStripMenuItem.Name = "milToolStripMenuItem";
            this.milToolStripMenuItem.Size = new System.Drawing.Size(129, 26);
            this.milToolStripMenuItem.Text = "100mil";
            this.milToolStripMenuItem.Click += new System.EventHandler(this.milToolStripMenuItem_Click);
            // 
            // mmToolStripMenuItem1
            // 
            this.mmToolStripMenuItem1.Name = "mmToolStripMenuItem1";
            this.mmToolStripMenuItem1.Size = new System.Drawing.Size(129, 26);
            this.mmToolStripMenuItem1.Text = "0.5mm";
            this.mmToolStripMenuItem1.Click += new System.EventHandler(this.mmToolStripMenuItem1_Click);
            // 
            // mmToolStripMenuItem
            // 
            this.mmToolStripMenuItem.Name = "mmToolStripMenuItem";
            this.mmToolStripMenuItem.Size = new System.Drawing.Size(129, 26);
            this.mmToolStripMenuItem.Text = "1mm";
            this.mmToolStripMenuItem.Click += new System.EventHandler(this.mmToolStripMenuItem_Click);
            // 
            // offToolStripMenuItem
            // 
            this.offToolStripMenuItem.Name = "offToolStripMenuItem";
            this.offToolStripMenuItem.Size = new System.Drawing.Size(129, 26);
            this.offToolStripMenuItem.Text = "Off";
            this.offToolStripMenuItem.Click += new System.EventHandler(this.offToolStripMenuItem_Click);
            // 
            // glControl1
            // 
            this.glControl1.AllowDrop = true;
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl1.Location = new System.Drawing.Point(0, 28);
            this.glControl1.Margin = new System.Windows.Forms.Padding(5);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(632, 267);
            this.glControl1.TabIndex = 5;
            this.glControl1.VSync = false;
            this.glControl1.DragDrop += new System.Windows.Forms.DragEventHandler(this.glControl1_DragDrop);
            this.glControl1.DragEnter += new System.Windows.Forms.DragEventHandler(this.glControl1_DragEnter);
            this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);
            this.glControl1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.glControl1_KeyDown);
            this.glControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseDown);
            this.glControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseMove);
            this.glControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseUp);
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBar1.Location = new System.Drawing.Point(611, 28);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(21, 267);
            this.vScrollBar1.TabIndex = 6;
            this.vScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar1_Scroll);
            // 
            // hScrollBar1
            // 
            this.hScrollBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.hScrollBar1.Location = new System.Drawing.Point(0, 274);
            this.hScrollBar1.Name = "hScrollBar1";
            this.hScrollBar1.Size = new System.Drawing.Size(611, 21);
            this.hScrollBar1.TabIndex = 7;
            this.hScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollBar1_Scroll);
            // 
            // GerberPanelize
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(632, 321);
            this.ControlBox = false;
            this.Controls.Add(this.hScrollBar1);
            this.Controls.Add(this.vScrollBar1);
            this.Controls.Add(this.glControl1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "GerberPanelize";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "7";
            this.Activated += new System.EventHandler(this.GerberPanelize_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GerberPanelize_FormClosed);
            this.Load += new System.EventHandler(this.GerberPanelize_Load);
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem addInstanceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addBreakTabToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem milToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem milToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mmToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem mmToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem offToolStripMenuItem;
        private OpenTK.GLControl glControl1;
        private System.Windows.Forms.ToolStripMenuItem exportBoardImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autosortAlgo1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem naiveRectanglePackerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem maxRectsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem panelPropertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem breaktabsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertBoardJoinToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem addGerberFolderToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem mergeOverlappingBreaktabsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem doItAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zoomToFitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scale11ToolStripMenuItem;
        private System.DirectoryServices.DirectoryEntry directoryEntry1;
        private System.Windows.Forms.VScrollBar vScrollBar1;
        private System.Windows.Forms.HScrollBar hScrollBar1;
        private System.Windows.Forms.ToolStripMenuItem generateSilkscreenLayerOffsetArtToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateArtOffsetCurvesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateArtFieldLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateArtReactedBlobsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateArtPrototypeStripToolStripMenuItem;
    }
}

