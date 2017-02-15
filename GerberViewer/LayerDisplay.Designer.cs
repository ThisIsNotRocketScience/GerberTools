namespace GerberViewer
{
    partial class LayerDisplay
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LayerDisplay));
            this.SuspendLayout();
            // 
            // LayerDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 417);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LayerDisplay";
            this.Text = "LayerDisplay";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LayerDisplay_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.LayerDisplay_KeyPress);
            this.Resize += new System.EventHandler(this.LayerDisplay_Resize);
            this.ResumeLayout(false);

        }

        #endregion
    }
}