using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PnP_Processor
{
    public partial class Actions :   WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public PnPMain pnp;
        public Actions(PnPMain parent)
        {
            InitializeComponent();
            pnp = parent;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            // open folder
            pnp.Open();
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
    }
}
