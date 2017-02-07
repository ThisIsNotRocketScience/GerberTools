using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Artwork;

namespace IconBuilder
{
    public partial class IconFrame : Form
    {
        public Color BaseColor { get; private set; }

    
        public IconFrame(string Lbl, Color BaseColor)
        {
            this.BaseColor = BaseColor;
            InitializeComponent();
            Letter.Text = Lbl;
        }

        private void pb128_Paint(object sender, PaintEventArgs e)
        {
            TINRSArtWorkRenderer.DrawIcon(pb128.Width, pb128.Height, e.Graphics, Letter.Text);
        }


        private void pb96_Paint(object sender, PaintEventArgs e)
        {
            TINRSArtWorkRenderer.DrawIcon(pb96.Width, pb96.Height, e.Graphics, Letter.Text);
        }

        private void pb64_Paint(object sender, PaintEventArgs e)
        {
            TINRSArtWorkRenderer.DrawIcon(pb64.Width, pb64.Height, e.Graphics, Letter.Text);

        }

        private void pb48_Paint(object sender, PaintEventArgs e)
        {
            TINRSArtWorkRenderer.DrawIcon(pb48.Width, pb48.Height, e.Graphics, Letter.Text);

        }

        private void pb32_Paint(object sender, PaintEventArgs e)
        {
            TINRSArtWorkRenderer.DrawIcon(pb32.Width, pb32.Height, e.Graphics, Letter.Text);

        }

        private void Letter_TextChanged(object sender, EventArgs e)
        {
            pb32.Invalidate();
            pb48.Invalidate();
            pb64.Invalidate();
            pb96.Invalidate();
            pb128.Invalidate();
        }
    }
}
