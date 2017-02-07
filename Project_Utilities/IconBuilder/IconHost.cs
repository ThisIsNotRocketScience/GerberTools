using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IconBuilder
{
    public partial class IconHost : Form
    {
        public IconHost()
        {
            InitializeComponent();

            List<String> Icons = new List<string>() { "PNL", "GBR", ".49\"", "3Dg", "Boot", "POKE" };
            int c = 0;
            int LW = 0;
            int LH = 0;

            
            foreach(var a in Icons)
            {
                var F = new IconFrame(a, Color.Yellow);
                F.Text = "Icon: " + a;
                F.Show();
                F.Bounds = new Rectangle(((c)%4) * F.Width, ((c++)/4)*F.Height , F.Width, F.Height);
                LW = F.Width;
                LH = F.Height;
            }
            this.SetBounds(((c) % 4) * LW, ((c++) / 4) * LH, LW, LH);

        }
    }
}
