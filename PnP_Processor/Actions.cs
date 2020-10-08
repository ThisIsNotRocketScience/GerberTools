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

        public void SetReg(string name, string value)
        {
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("TiNRS_PnP_Proc");
            key.SetValue(name, value);
            key.Close();           
        }

        public PnPMain pnp;
        bool blocksave = false;
        public Actions(PnPMain parent)
        {
            InitializeComponent();
            blocksave = true;

            stockbox.Text = GetReg("StockFile");
            silkbox.Text = GetReg("SilkFile");
            pnpbox.Text = GetReg("PnPFile");
            bombox.Text = GetReg("BOMFile");
            outlinebox.Text = GetReg("OutlineFile");
            gerberzipbox.Text = GetReg("GerberZip");

            blocksave = false;
            pnp = parent;
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

        private void selectoutline_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = GetReg("lastfolder");
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                outlinebox.Text = openFileDialog1.FileName;
                SaveRegs();
            }

        }
        public void SaveRegs()
        {
            if (blocksave) return;
            SetReg("lastfolder", Path.GetDirectoryName(openFileDialog1.FileName));
            SetReg("StockFile", stockbox.Text);
            SetReg("SilkFile", silkbox.Text);
            SetReg("PnPFile", pnpbox.Text);
            SetReg("BOMFile", bombox.Text);
            SetReg("OutlineFile", outlinebox.Text);
            SetReg("GerberZip", gerberzipbox.Text);
        }


        private void selectsilk_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = GetReg("lastfolder");
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                silkbox.Text = openFileDialog1.FileName;
                SaveRegs();

            }
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
                silk = silkbox.Text,
                pnp = pnpbox.Text,
                bom = bombox.Text,
                outline = outlinebox.Text,
                gerberzip = gerberzipbox.Text,
                loaded = false
            };
           
            pnp.AddDoc(d);

        }
    }
}
