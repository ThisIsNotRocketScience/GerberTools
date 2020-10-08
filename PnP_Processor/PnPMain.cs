using GerberLibrary;
using GerberLibrary.Core;
using GerberLibrary.Core.Primitives;
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
using WeifenLuo.WinFormsUI.Docking;
using WeifenLuo.WinFormsUI.ThemeVS2015;


namespace PnP_Processor
{
    public partial class PnPMain : Form
    {
        private DockPanel dockPanel;

        BoardDisplay BoardDisp;
        BOMList TheBOMList;

        public PnPMain(string[] args)
        {
            InitializeComponent();

            this.dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();

            var theme = new VS2015BlueTheme();
            this.dockPanel.Theme = theme;

            this.dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Controls.Add(this.dockPanel);
            this.dockPanel.SetBounds(0, 0, Width, Height);

            TheBOMList = new BOMList(this);
            TheBOMList.Show(dockPanel, DockState.DockLeft);

            dockPanel.UpdateDockWindowZOrder(DockStyle.Left, true);
            
            Actions A1 = new Actions(this);
            A1.Show(dockPanel);

            BoardDisp = new BoardDisplay(this, false);
            BoardDisp.Show(A1.Pane, DockAlignment.Bottom, 0.7);
        }

        internal void Flip()
        {
            
        }

        internal void Rotate()
        {
            
        }
       
        List<PnPProcDoc> Docs = new List<PnPProcDoc>();

        public PnPProcDoc ActiveDoc = null;
        bool DocLoaded = false;
        internal void AddDoc(PnPProcDoc d)
        {
            Docs.Add(d);
            DocLoaded = false;
            d.StartLoad();
            ActiveDoc = d;
            TheBOMList.UpdateList();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (DocLoaded == false)
            {
                if (ActiveDoc!=null && ActiveDoc.loaded == true)
                {
                    DocLoaded = true;
                    TheBOMList.UpdateList();
                }
            }
        }

        public List<string> selectedrefdes = new List<string>();
        internal void UpdateBoard(List<string> refdeslist)
        {
            selectedrefdes = refdeslist;
            BoardDisp.RefreshPic();
        }
    }
}
