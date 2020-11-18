using GerberLibrary.Core;
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
        public class BOMEntryItem
        {
            public BOMEntryItem(BOMEntry e)
            {
                entry = e;
            }
            public BOMEntry entry;
        }
        
        public class REFDesItem
        {
            public REFDesItem(BOMEntry.RefDesc rd, BOMEntry e)
            {
                refdes = rd;
                entry = e;
            }

            public BOMEntry entry;
            public BOMEntry.RefDesc refdes;
        }

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
            List<BOMEntryItem> be = new List<BOMEntryItem>();
            foreach(var a in D.B.DeviceTree)
            {
                foreach(var b in a.Value)
                {
                    be.Add(new BOMEntryItem(b.Value));
                    foreach(var c in b.Value.RefDes)
                    {
                        pnplist.Items.Add(new REFDesItem( c, b.Value));
                    }
                }
            }
            be = be.OrderBy(x => x.entry.Combined()).ToList();
            foreach(var a in be)
            {
                BOM.Items.Add(a);
            }            
        }

        private void pnplist_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();
            e.Graphics.DrawString((pnplist.Items[e.Index] as REFDesItem).refdes.OriginalName, new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular), Brushes.Black, e.Bounds);
        }

        private void BOM_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();
            if (e.Index < 0) return;

            var P = (BOM.Items[e.Index] as BOMEntryItem);

            var curcol = Helpers.RefractionNormalledMaxBrightnessAndSat((float)e.Index / (float)BOM.Items.Count);

            e.Graphics.FillRectangle(new SolidBrush(curcol), e.Bounds);
            if ((e.State & DrawItemState.Focus) != DrawItemState.Focus)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(150,Color.Black)), e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(50, Color.Black)), e.Bounds);

            }
            
//            e.Graphics.DrawLine(new Pen(Color.FromArgb(200, curcol), 2), e.Bounds.X, e.Bounds.Y + e.Bounds.Height-2, e.Bounds.X + e.Bounds.Width, e.Bounds.Y + e.Bounds.Height - 2);

            e.Graphics.DrawString(P.entry.Combined(), new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular), Brushes.White, e.Bounds);
        }

        bool SelectionInProcess = false;

        private void BOM_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectionInProcess) return;
            SelectionInProcess = true;
            pnplist.ClearSelected();

            if (BOM.SelectedItem == null) return;

            BOMEntryItem be = BOM.SelectedItem as BOMEntryItem;
            List<int> indices = new List<int>();
            List<string> refdeslist = new List<string>();
            foreach(var a in be.entry.RefDes)
            {
                for (int i =0;i<pnplist.Items.Count;i++)
                {
                    REFDesItem b = pnplist.Items[i] as REFDesItem;
                
                    if(b.refdes.NameOnBoard == a.NameOnBoard)
                    {
                        refdeslist.Add(a.NameOnBoard);
                        indices.Add(i);
                    }
                }
            }
            foreach (var i in indices) pnplist.SelectedItems.Add(pnplist.Items[i]);
            
            pnp.UpdateBoard(refdeslist);

            SelectionInProcess = false;
        }

        private void pnplist_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectionInProcess) return;

            

        }
    }
}
