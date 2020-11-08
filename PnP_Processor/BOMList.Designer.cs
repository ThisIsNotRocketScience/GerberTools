namespace PnP_Processor
{
    partial class BOMList
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.BOM = new System.Windows.Forms.ListBox();
            this.pnplist = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.BOM);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pnplist);
            this.splitContainer1.Size = new System.Drawing.Size(533, 292);
            this.splitContainer1.SplitterDistance = 172;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 0;
            // 
            // BOM
            // 
            this.BOM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BOM.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.BOM.FormattingEnabled = true;
            this.BOM.ItemHeight = 20;
            this.BOM.Location = new System.Drawing.Point(0, 0);
            this.BOM.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.BOM.Name = "BOM";
            this.BOM.Size = new System.Drawing.Size(529, 168);
            this.BOM.TabIndex = 0;
            this.BOM.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.BOM_DrawItem);
            this.BOM.SelectedIndexChanged += new System.EventHandler(this.BOM_SelectedIndexChanged);
            // 
            // pnplist
            // 
            this.pnplist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnplist.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.pnplist.FormattingEnabled = true;
            this.pnplist.ItemHeight = 20;
            this.pnplist.Location = new System.Drawing.Point(0, 0);
            this.pnplist.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pnplist.MultiColumn = true;
            this.pnplist.Name = "pnplist";
            this.pnplist.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.pnplist.Size = new System.Drawing.Size(529, 113);
            this.pnplist.TabIndex = 1;
            this.pnplist.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.pnplist_DrawItem);
            this.pnplist.SelectedIndexChanged += new System.EventHandler(this.pnplist_SelectedIndexChanged);
            // 
            // BOMList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(533, 292);
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "BOMList";
            this.Text = "BOMList";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox BOM;
        private System.Windows.Forms.ListBox pnplist;
    }
}