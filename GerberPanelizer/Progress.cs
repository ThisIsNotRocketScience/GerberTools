using GerberLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GerberCombinerBuilder
{
    public partial class Progress : Form, ProgressLog
    {
        public GerberPanelize MyPanelize;
        public Progress(GerberPanelize Parent)
        {
            InitializeComponent();
            MyPanelize = Parent;
        }
        Mutex LineLock = new Mutex();
        List<string> TextlinesToAdd = new List<string>();
        float fProgress = 0;

        public void AddLog(string text, float progress)
        {
            LineLock.WaitOne();
            TextlinesToAdd.Add(text);
            Console.WriteLine(text);
            fProgress = progress;
            LineLock.ReleaseMutex();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            LineLock.WaitOne();
            while (TextlinesToAdd.Count > 0)
            {
                textBox1.AppendText( TextlinesToAdd.First() + "\r\n");
                TextlinesToAdd.Remove(TextlinesToAdd.First());
            }
            LineLock.ReleaseMutex();
            progressBar1.Value = (int)Math.Floor(fProgress * 100) ;
            if (fProgress >= 1)
            {
                MyPanelize.ProcessDone();
                this.Close();
            }
        }

        public void AddString(string text, float progress)
        {
            if (progress == -1) progress = fProgress;
            AddLog(text, progress);
        }
    }
}
