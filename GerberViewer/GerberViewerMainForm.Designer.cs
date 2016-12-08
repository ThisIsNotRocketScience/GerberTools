namespace GerberViewer
{
    partial class GerberViewerMainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GerberViewerMainForm));
            this.SuspendLayout();
            // 
            // GerberViewerMainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1068, 654);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.Name = "GerberViewerMainForm";
            this.Text = "TINRS Gerber Viewer";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.GerberViewerMainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.GerberViewerMainForm_DragEnter);
            this.ResumeLayout(false);

        }

        #endregion
    }
}

