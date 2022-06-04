using GerberLibrary;
using GerberLibrary.Core;
using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JLCDrop
{
    public partial class JLCDropForm : Form
    {
        public static void SetInvariantCulture()
        {
            CultureInfo ci = new CultureInfo("nl-NL");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }


        public JLCDropForm()
        {
            InitializeComponent();
            SetInvariantCulture();
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {

                string[] D = e.Data.GetData(DataFormats.FileDrop) as string[];

                BackColor = Color.Green;

                foreach (string S in D)
                {
                    if (File.Exists(S))
                    {
                        try
                        {                            
                            BOM.LoadRotationOffsets();
                            Factory.MakeBomAndPlacement(S);
                            
                            Factory.ZipGerbers(S, MakeFrameBox.Checked, (int)FrameMM.Value, FrameName.Text);
                            CountDown = 10;
                        }
                        catch (Exception)
                        {
                            BackColor = Color.Red;
                        }
                    }

                    
                }
            }
        }

        int CountDown = 0;
    

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (CountDown > 0)
            {
                CountDown--;
            }
            else
            {
                BackColor = Color.Black;
            }
        }
    }
}
