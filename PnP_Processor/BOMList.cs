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
    public partial class BOMList :   WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public PnPMain pnp;
        public BOMList(PnPMain main)
        {
            pnp = main;
            InitializeComponent();
        }

        internal void UpdateList()
        {
            pnplist.Items.Clear();
            BOM.Items.Clear();
            if (pnp.ActiveDoc == null) return;
            var D = pnp.ActiveDoc;
            if (D.loaded == false)
            {
                pnplist.Enabled = false;
                BOM.Enabled = false;
                return;
            }
            pnplist.Enabled = true;
            BOM.Enabled = true;
            foreach(var a in D.B.DeviceTree)
            {
                foreach(var b in a.Value)
                {
                    BOM.Items.Add(b.Value.Combined());
                    foreach(var c in b.Value.RefDes)
                    {
                        pnplist.Items.Add(c.NameOnBoard);
                    }
                }
            }
        }
    }
}
