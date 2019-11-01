namespace TINRS_ArtWorkGenerator
{
    partial class TinrsArtWork
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TinrsArtWork));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadMaskToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveArtworkbmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveArtworksvgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveArtworkgerberToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blankMaskToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.openBitmapDialog = new System.Windows.Forms.OpenFileDialog();
            this.svgsaveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.bmpsaveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.settingssaveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.settingsopenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.gerbersaveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.createSleeveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(735, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadMaskToolStripMenuItem,
            this.saveArtworkbmpToolStripMenuItem,
            this.saveArtworksvgToolStripMenuItem,
            this.saveArtworkgerberToolStripMenuItem,
            this.saveSettingsToolStripMenuItem,
            this.loadSettingsToolStripMenuItem,
            this.blankMaskToolStripMenuItem,
            this.createSleeveToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // loadMaskToolStripMenuItem
            // 
            this.loadMaskToolStripMenuItem.Name = "loadMaskToolStripMenuItem";
            this.loadMaskToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.loadMaskToolStripMenuItem.Text = "&Load Mask";
            this.loadMaskToolStripMenuItem.Click += new System.EventHandler(this.loadMaskToolStripMenuItem_Click);
            // 
            // saveArtworkbmpToolStripMenuItem
            // 
            this.saveArtworkbmpToolStripMenuItem.Name = "saveArtworkbmpToolStripMenuItem";
            this.saveArtworkbmpToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.saveArtworkbmpToolStripMenuItem.Text = "&Save Artwork (png)";
            this.saveArtworkbmpToolStripMenuItem.Click += new System.EventHandler(this.saveArtworkbmpToolStripMenuItem_Click);
            // 
            // saveArtworksvgToolStripMenuItem
            // 
            this.saveArtworksvgToolStripMenuItem.Name = "saveArtworksvgToolStripMenuItem";
            this.saveArtworksvgToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.saveArtworksvgToolStripMenuItem.Text = "Save Artwork (svg)";
            this.saveArtworksvgToolStripMenuItem.Click += new System.EventHandler(this.saveArtworksvgToolStripMenuItem_Click);
            // 
            // saveArtworkgerberToolStripMenuItem
            // 
            this.saveArtworkgerberToolStripMenuItem.Name = "saveArtworkgerberToolStripMenuItem";
            this.saveArtworkgerberToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.saveArtworkgerberToolStripMenuItem.Text = "&Save Artwork (gerber)";
            this.saveArtworkgerberToolStripMenuItem.Click += new System.EventHandler(this.saveArtworkgerberToolStripMenuItem_Click);
            // 
            // saveSettingsToolStripMenuItem
            // 
            this.saveSettingsToolStripMenuItem.Name = "saveSettingsToolStripMenuItem";
            this.saveSettingsToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.saveSettingsToolStripMenuItem.Text = "Save Settings";
            this.saveSettingsToolStripMenuItem.Click += new System.EventHandler(this.saveSettingsToolStripMenuItem_Click);
            // 
            // loadSettingsToolStripMenuItem
            // 
            this.loadSettingsToolStripMenuItem.Name = "loadSettingsToolStripMenuItem";
            this.loadSettingsToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.loadSettingsToolStripMenuItem.Text = "Load Settings";
            this.loadSettingsToolStripMenuItem.Click += new System.EventHandler(this.loadSettingsToolStripMenuItem_Click);
            // 
            // blankMaskToolStripMenuItem
            // 
            this.blankMaskToolStripMenuItem.Name = "blankMaskToolStripMenuItem";
            this.blankMaskToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.blankMaskToolStripMenuItem.Text = "Blank Mask";
            this.blankMaskToolStripMenuItem.Click += new System.EventHandler(this.blankMaskToolStripMenuItem_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 24);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(735, 468);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseUp);
            this.pictureBox1.Resize += new System.EventHandler(this.pictureBox1_Resize);
            // 
            // openBitmapDialog
            // 
            this.openBitmapDialog.DefaultExt = "png";
            this.openBitmapDialog.Filter = "PNG files|*.png";
            this.openBitmapDialog.Title = "Select Image file";
            // 
            // svgsaveFileDialog1
            // 
            this.svgsaveFileDialog1.DefaultExt = "svg";
            this.svgsaveFileDialog1.Filter = "SVG files|*.svg";
            // 
            // bmpsaveFileDialog1
            // 
            this.bmpsaveFileDialog1.DefaultExt = "png";
            this.bmpsaveFileDialog1.Filter = "PNG files|*.png";
            // 
            // settingssaveFileDialog1
            // 
            this.settingssaveFileDialog1.DefaultExt = "xml";
            this.settingssaveFileDialog1.Filter = "XML files|*.xml";
            // 
            // settingsopenFileDialog1
            // 
            this.settingsopenFileDialog1.DefaultExt = "xml";
            this.settingsopenFileDialog1.Filter = "XML files|*.xml";
            // 
            // gerbersaveFileDialog1
            // 
            this.gerbersaveFileDialog1.DefaultExt = "gbr";
            this.gerbersaveFileDialog1.Filter = "GERBER files|*.gbr";
            this.gerbersaveFileDialog1.Title = "Select Gerber File.";
            // 
            // createSleeveToolStripMenuItem
            // 
            this.createSleeveToolStripMenuItem.Name = "createSleeveToolStripMenuItem";
            this.createSleeveToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.createSleeveToolStripMenuItem.Text = "Create Sleeve";
            this.createSleeveToolStripMenuItem.Click += new System.EventHandler(this.createSleeveToolStripMenuItem_Click);
            // 
            // TinrsArtWork
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(735, 492);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "TinrsArtWork";
            this.Text = "TINRS Artwork Tool";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.TinrsArtWork_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.TinrsArtWork_DragEnter);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadMaskToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveArtworkbmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveArtworkgerberToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.OpenFileDialog openBitmapDialog;
        private System.Windows.Forms.ToolStripMenuItem saveSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveArtworksvgToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog svgsaveFileDialog1;
        private System.Windows.Forms.SaveFileDialog bmpsaveFileDialog1;
        private System.Windows.Forms.SaveFileDialog settingssaveFileDialog1;
        private System.Windows.Forms.OpenFileDialog settingsopenFileDialog1;
        private System.Windows.Forms.SaveFileDialog gerbersaveFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem blankMaskToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createSleeveToolStripMenuItem;
    }
}

