using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Progress : Form, GerberLibrary.ProgressLog
    {
        private List<string> Files;


        string SolderMaskColor;
        string SilkScreenColor;
        string CopperColor;
        public Progress(List<string> s, string _SolderMaskColor, string _SilkScreenColor, string _CopperColor)
        {
            SolderMaskColor = _SolderMaskColor;
            SilkScreenColor = _SilkScreenColor;
            CopperColor = _CopperColor;

            InitializeComponent();
            progressBar1.Value = 0;
            Files = s;



        }
        Thread T = null;
        internal void StartThread()
        {
            T = new Thread(new ThreadStart(doStuff));
            T.Start();
        }

        public void doStuff()
        {
            GerberLibrary.GerberImageCreator GIC = new GerberLibrary.GerberImageCreator();
            GerberLibrary.BoardRenderColorSet Colors = new GerberLibrary.BoardRenderColorSet();
            Colors.BoardRenderColor = GerberLibrary.Gerber.ParseColor(SolderMaskColor);
            Colors.BoardRenderSilkColor = GerberLibrary.Gerber.ParseColor(SilkScreenColor);
            Colors.BoardRenderPadColor = GerberLibrary.Gerber.ParseColor(CopperColor);
            SetProgress("Image generation started", -1);
            GIC.SetColors(Colors);

            GerberLibrary.Gerber.SaveIntermediateImages = true;


            bool fixgroup = true;
            if (Files.Count == 1) fixgroup = false;
            GIC.AddBoardsToSet(Files, fixgroup, this);

            if (GIC.Errors.Count > 0)
            {
                foreach (var a in GIC.Errors)
                {
                    Errors.Add(String.Format("Error: {0}", a));
                }
            }
            try
            {
                if (Files.Count > 1)
                {


                    GIC.WriteImageFiles(Path.GetDirectoryName(Files[0]) + ".png", 200, true, this);
                    //       GIC.DrawAllFiles(Path.GetDirectoryName(Files[0]) + "_Layer", 200, this);
                }
                else
                {
                    GIC.DrawAllFiles(Files[0] + "_Layer", 200, this);
                }
            }
            catch(Exception E)
            {
                Errors.Add("Some errors:");
                while(E!=null)
                {
                    Errors.Add(E.Message);
                    E = E.InnerException;
                }
            }
            SetProgress("Done!",-1);
            if (Errors.Count >0)
            {
                SetProgress("Encountered some problems during image generation:", -1);
            }
            foreach (var a in Errors)
            {
                SetProgress(a, -1);
            }

        }
        List<String> Errors = new List<string>();
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (T == null) return;
            if (T.ThreadState == ThreadState.Stopped && Errors.Count == 0)
            {
                this.Close();

            }
        }

        public void AddString(string text, float progress = -1F)
        {
            SetProgress(text, progress);

        }

        delegate void UpdateBar(string text, float progress);

        private void SetProgress(string text, float progress)
        {

            if (this.progressBar1.InvokeRequired)
            {
                UpdateBar d = new UpdateBar(SetProgress);
                this.Invoke(d, new object[] { text, progress });
            }
            else
            {
                if (progress > -1) progressBar1.Value = (int)((progress > 0 ? progress : 0) * 100.0);
                textBox1.Text = text + "\r\n" + textBox1.Text;
            }
        }


    }
}
