using GerberLibrary;
using GerberLibrary.Core;
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

namespace PnP_Processor
{
    public partial class Actions :   WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public string GetReg(string name)
        {
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("TiNRS_PnP_Proc");
            var res = key.GetValue(name) as string;
            key.Close();
            return (res!=null)?res:"";
        }

        public bool GetRegBool(string name)
        {
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("TiNRS_PnP_Proc");
            var res = key.GetValue(name) as string;
            key.Close();
            return (res != null) ? (res=="true"):false;
        }
        public void SetReg(string name, string value)
        {
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("TiNRS_PnP_Proc");
            key.SetValue(name, value);
            key.Close();           
        }

        public void SetRegBool(string name, bool value)
        {
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("TiNRS_PnP_Proc");
            key.SetValue(name, value ? "true" : "false");
            key.Close();
        }


        public PnPMain pnp;
        bool blocksave = false;
        public Actions(PnPMain parent)
        {
            InitializeComponent();
            blocksave = true;

            stockbox.Text = GetReg("StockFile");
            pnpbox.Text = GetReg("PnPFile");
            bombox.Text = GetReg("BOMFile");
            gerberzipbox.Text = GetReg("GerberZip");
            topsilk.Checked = GetRegBool("topsilkvisible");
            bottomsilk.Checked = GetRegBool("bottomsilkvisible");
            string fmode = GetReg("flipmode");
            int idx = FlipModeBox.Items.IndexOf(fmode);
            if (idx <= 0) idx = 0;
            FlipModeBox.SelectedIndex = idx;

            blocksave = false;
            pnp = parent;
            pnp.flipboard = MakeFlipMode(FlipModeBox.SelectedItem);
            pnp.topsilkvisible = topsilk.Checked;
            pnp.bottomsilkvisible = bottomsilk.Checked;
        }

        private PnPProcDoc.FlipMode MakeFlipMode(object selectedItem)
        {
            var s = selectedItem as string;
            if (s == null) return PnPProcDoc.FlipMode.NoFlip;
            switch(s) 
            {
                case "None":
                    return PnPProcDoc.FlipMode.NoFlip;
                case "Diagonal":
                    return PnPProcDoc.FlipMode.FlipDiagonal;
                case "Horizontal":
                    return PnPProcDoc.FlipMode.FlipHorizontal;
            }
            return PnPProcDoc.FlipMode.NoFlip;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = GetReg("lastfolder");
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                bombox.Text = openFileDialog1.FileName;
                SaveRegs();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // rotate 90
            pnp.Rotate();
        }

        internal void RefreshDisplay()
        {
            pictureBox2.Invalidate();
            pictureBox1.Invalidate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // switch sides
            pnp.Flip();
        }

        private void selectpnp_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = GetReg("lastfolder");
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pnpbox.Text = openFileDialog1.FileName;
                SaveRegs();
            }
        }

        private void selectstock_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = GetReg("lastfolder");
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                stockbox.Text = openFileDialog1.FileName;
                SaveRegs();
            }

        }

        public void SaveRegs()
        {
            if (blocksave) return;
            SetReg("lastfolder", Path.GetDirectoryName(openFileDialog1.FileName));
            SetReg("StockFile", stockbox.Text);
            SetReg("PnPFile", pnpbox.Text);
            SetReg("BOMFile", bombox.Text);
            SetReg("GerberZip", gerberzipbox.Text);
            SetReg("diprotate", diprotatefile.Text);
        }


        private void gerberzipbutton_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = GetReg("lastfolder");
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                gerberzipbox.Text = openFileDialog1.FileName;
                SaveRegs();

            }
        }

        private void gerberzipbox_TextChanged(object sender, EventArgs e)
        {
            SaveRegs();
        }

        private void silkbox_TextChanged(object sender, EventArgs e)
        {
            SaveRegs();
        }

        private void outlinebox_TextChanged(object sender, EventArgs e)
        {
            SaveRegs();
        }

        private void stockbox_TextChanged(object sender, EventArgs e)
        {
            SaveRegs();
        }

        private void pnpbox_TextChanged(object sender, EventArgs e)
        {
            SaveRegs();
        }

        private void bombox_TextChanged(object sender, EventArgs e)
        {
            SaveRegs();
        }

        private void ProcessButton_Click(object sender, EventArgs e)
        {
            var d = new PnPProcDoc()
            {
                stock = stockbox.Text,
                pnp = pnpbox.Text,
                bom = bombox.Text,               
                gerberzip = gerberzipbox.Text,
                loaded = false
            };
           
            pnp.AddDoc(d);

        }

        private void topsilk_CheckedChanged(object sender, EventArgs e)
        {
            if (blocksave) return;
            SetRegBool("topsilkvisible", topsilk.Checked);
            pnp.topsilkvisible = topsilk.Checked;

            pnp.UpdateBoard(null);
        }

        private void bottomsilk_CheckedChanged(object sender, EventArgs e)
        {
            if (blocksave) return;
            SetRegBool("bottomsilkvisible", bottomsilk.Checked);
            pnp.bottomsilkvisible = bottomsilk.Checked;

            pnp.UpdateBoard(null);

        }

      
        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            var G = e.Graphics;
            var P = pictureBox2;
            Bounds TheBox = new Bounds();
            TheBox.FitPoint(-10, -10);
            TheBox.FitPoint(10, 10);
            var Bb = TheBox;


            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            G.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            if (pnp.ActiveDoc == null) return;
            var D = pnp.ActiveDoc;
            float S = Helpers.SetupMatrixForExtends(G, P, Bb, 2);

            Pen Ppp = new Pen(Color.FromArgb(30, 0, 0, 255), 1.0f / S);
            Pen Ppp2 = new Pen(Color.FromArgb(50, 0, 0, 255), 1.0f / S);
            e.Graphics.Clear(Color.FromArgb(220, 220, 210));
            for (int i = -10; i <= 10; i++)
            {
                var theP = Ppp;
                if (i % 5 == 0)
                {
                    theP = Ppp2;
                }
                e.Graphics.DrawLine(theP, -10, i, 10, i);
                e.Graphics.DrawLine(theP, i, -10, i, 10);
            }



            if (pnp.selectedrefdes.Count > 0)
            {
                var BomEntry = D.B.GetBOMEntry(pnp.selectedrefdes[0]);
                if (BomEntry != null) BOM.RenderPackage(G, 0, 0, 0, BomEntry.PackageName, BoardSide.Top);

            }

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var G = e.Graphics;
            var P = pictureBox2;
            Bounds TheBox = new Bounds();
            TheBox.FitPoint(-10, -10);
            TheBox.FitPoint(10, 10);
            var Bb = TheBox;


            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            G.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            if (pnp.ActiveDoc == null) return;
            var D = pnp.ActiveDoc;
            float S = Helpers.SetupMatrixForExtends(G, P, Bb, 2);
            Pen Ppp = new Pen(Color.FromArgb(30, 0, 0, 255), 1.0f / S);
            Pen Ppp2 = new Pen(Color.FromArgb(50, 0, 0, 255), 1.0f / S);
            e.Graphics.Clear(Color.FromArgb(220,220,210));
            for (int i = -10; i <= 10; i++)
            {
                var theP = Ppp;
                if (i % 5 == 0)
                {
                    theP = Ppp2;
                }
                e.Graphics.DrawLine(theP, -10, i, 10, i);
                e.Graphics.DrawLine(theP, i, -10, i, 10);
            }

            if (pnp.selectedrefdes.Count > 0)
            {
                var BomEntry = D.B.GetBOMEntry(pnp.selectedrefdes[0]);
                BOM.RenderPackage(G, 0, 0, 0, BomEntry.PackageName, BoardSide.Top);
            }
        }

        private void pictureBox2_Resize(object sender, EventArgs e)
        {
            pictureBox2.Invalidate();
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void diprotatebutton_click(object sender, EventArgs e)
        {
            SaveRegs();
        }

        private void FlipModeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
                if (blocksave) return;
                SetReg("flipmode", FlipModeBox.SelectedItem as string);
                pnp.flipboard = MakeFlipMode(FlipModeBox.SelectedItem);
                pnp.RebuildPost();


        }
    }
}
