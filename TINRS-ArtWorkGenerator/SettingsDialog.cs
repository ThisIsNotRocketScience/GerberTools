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
using Artwork;

namespace TINRS_ArtWorkGenerator
{
    public partial class SettingsDialog : Form
    {
        Settings SettingsTarget;
        bool initdone = false;
        private Action TheCallback;
        private Action<string> TheProcessCallback;

        public void UpdateFromSettings()
        {
            radioQuadTree.Checked = SettingsTarget.Mode == Settings.ArtMode.QuadTree ? true : false;
            radioSubstitution.Checked = SettingsTarget.Mode == Settings.ArtMode.Tiling ? true : false;
            delaunayRadio.Checked = SettingsTarget.Mode == Settings.ArtMode.Delaunay ? true : false;



            maxdepthbox.Value = SettingsTarget.MaxSubDiv;
            rotationbox.Value = (decimal)SettingsTarget.DegreesOff;
            scalesmallerbox.Value = (int)SettingsTarget.scalesmaller;

            scalesmallerlevelbox.Value = (int)SettingsTarget.scalesmallerlevel;
            string TileName = Enum.GetName(typeof(Artwork.Tiling.TilingType), SettingsTarget.TileType);
            TileTypes.SelectedItem = TileName;
            Thresholdlevelbox.Value = SettingsTarget.Threshold;
            invertoutput.Checked = SettingsTarget.InvertOutput;
            invertsource.Checked = SettingsTarget.InvertSource;
            alwaysSubdivide.Checked = SettingsTarget.alwayssubdivide;
            symmetry.Checked = SettingsTarget.Symmetry;
            Xscaling.Value = SettingsTarget.xscalesmallerlevel;
            SuperSymmetry.Checked = SettingsTarget.SuperSymmetry;
            generalScale.Value = (int)(SettingsTarget.scalesmallerfactor * 1000.0f);
            xscalecenterperc.Value = SettingsTarget.xscalecenter;
            distanceToMaskRange.Value  = (int)(SettingsTarget.distanceToMaskRange * 1000.0f);
            distanceToMaskScale.Value = (int)(SettingsTarget.distanceToMaskScale * 1000.0f);
            marcelplating.Checked = SettingsTarget.MarcelPlating;
            ballradius.Value = (int)(SettingsTarget.BallRadius * 1.0f);
            gap.Value = (int)(SettingsTarget.Gap * 1.0f);
            Rounding.Value = (int)(SettingsTarget.Rounding * 1.0f);

        }

        public SettingsDialog(Settings Target, Action callback, Action<string> proccall)
        {
            TheProcessCallback = proccall;
            SettingsTarget = Target;
            InitializeComponent();
            UpdateCheckbox.Checked = true;

            TheCallback = callback;

            foreach (var a in Enum.GetNames(typeof(Artwork.Tiling.TilingType)))
            {
                TileTypes.Items.Add(a);
            };

            UpdateFromSettings();

            initdone = true;
        }


        private void maxdepthbox_ValueChanged(object sender, EventArgs e)
        {
            SettingsTarget.MaxSubDiv = (int)maxdepthbox.Value;
            DoUpdate();
        }
        void DoUpdate()
        {
            if (UpdateCheckbox.Checked) TheCallback();
        }
        private void rotationbox_ValueChanged(object sender, EventArgs e)
        {
            SettingsTarget.DegreesOff = (float)rotationbox.Value;
            DoUpdate();
        }

        private void radioSubstitution_CheckedChanged(object sender, EventArgs e)
        {
            UpdateModeRadio();
        }

        void UpdateModeRadio()
        {
            if (radioQuadTree.Checked) SettingsTarget.Mode = Settings.ArtMode.QuadTree;
            if (radioSubstitution.Checked) SettingsTarget.Mode = Settings.ArtMode.Tiling;
            if (delaunayRadio.Checked) SettingsTarget.Mode = Settings.ArtMode.Delaunay;

            DoUpdate();
        }

        private void radioQuadTree_CheckedChanged(object sender, EventArgs e)
        {
            UpdateModeRadio();
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            TheCallback();
        }

        private void UpdateCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdateCheckbox.Checked && initdone) TheCallback();
        }

        private void TileTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            SettingsTarget.TileType = (Artwork.Tiling.TilingType)Enum.Parse(typeof(Artwork.Tiling.TilingType), TileTypes.Items[TileTypes.SelectedIndex].ToString());
            DoUpdate();
        }

        internal void SetTimeElapsed(double totalMilliseconds)
        {
            Text = String.Format("Settings - {0:N0} msec for last frame", totalMilliseconds);
        }

        private void Thresholdlevelbox_ValueChanged(object sender, EventArgs e)
        {
            SettingsTarget.Threshold = (int)Thresholdlevelbox.Value;
            SettingsTarget.ReloadMask = true;
            DoUpdate();
        }

        private void invertsource_CheckedChanged(object sender, EventArgs e)
        {
            SettingsTarget.InvertSource = invertsource.Checked;
            SettingsTarget.ReloadMask = true;

            DoUpdate();
        }

        private void invertoutput_CheckedChanged(object sender, EventArgs e)
        {
            SettingsTarget.InvertOutput = invertoutput.Checked;
            DoUpdate();
        }

        private void delaunayRadio_CheckedChanged(object sender, EventArgs e)
        {
            UpdateModeRadio();
        }

        private void SettingsDialog_DragEnter(object sender, DragEventArgs e)
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

        private void SettingsDialog_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {

                string[] D = e.Data.GetData(DataFormats.FileDrop) as string[];
                List<String> files = new List<string>();
                foreach (string S in D)
                {
                    if (File.Exists(S)) files.Add(S);
                }
                if (files.Count > 0)
                {
                    foreach (var a in files)
                    {
                        TheProcessCallback(a);
                    }
                }
            }
        }

        private void scalesmallerbox_ValueChanged(object sender, EventArgs e)
        {
            SettingsTarget.scalesmaller = (int)scalesmallerbox.Value;
            DoUpdate();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            SettingsTarget.scalesmallerlevel = (int) scalesmallerlevelbox.Value;
            DoUpdate();
        }

        private void alwaysSubdivide_CheckedChanged(object sender, EventArgs e)
        {
            SettingsTarget.alwayssubdivide = alwaysSubdivide.Checked;
            DoUpdate();
        }

        private void symmetry_CheckedChanged(object sender, EventArgs e)
        {
            SettingsTarget.Symmetry = symmetry.Checked;
            DoUpdate();
        }

        private void SuperSymmetry_CheckedChanged(object sender, EventArgs e)
        {
            SettingsTarget.SuperSymmetry = SuperSymmetry.Checked;
            DoUpdate();

        }

        private void Xscaling_ValueChanged(object sender, EventArgs e)
        {
            SettingsTarget.xscalesmallerlevel = (int)Xscaling.Value;
            DoUpdate();
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged_1(object sender, EventArgs e)
        {
            SettingsTarget.xscalecenter = (int)xscalecenterperc.Value;
            DoUpdate();
        }

        private void numericUpDown1_ValueChanged_2(object sender, EventArgs e)
        {
            SettingsTarget.scalesmallerfactor = (float)generalScale.Value * 0.001f;
            if (generalScale.Value == 1000) SettingsTarget.scalesmallerfactor = 1.0f;
            DoUpdate();
        }

        private void distanceToMaskScale_ValueChanged(object sender, EventArgs e)
        {
            SettingsTarget.distanceToMaskScale = (float)distanceToMaskScale.Value * 0.001f;
            if (distanceToMaskScale.Value == 1000) SettingsTarget.distanceToMaskScale = 1.0f;
            DoUpdate();
        }

        private void distanceToMaskRange_ValueChanged(object sender, EventArgs e)
        {
            SettingsTarget.distanceToMaskRange = (float)distanceToMaskRange.Value * 0.001f;
            if (distanceToMaskRange.Value == 1000) SettingsTarget.distanceToMaskRange= 1.0f;

            DoUpdate();
        }

        private void gap_ValueChanged(object sender, EventArgs e)
        {
            SettingsTarget.Gap = (float)gap.Value * 1.0f;
            DoUpdate();

        }

        private void ballradius_ValueChanged(object sender, EventArgs e)
        {
            SettingsTarget.BallRadius = (float)ballradius.Value * 1.0f;
            DoUpdate();
        }

        private void marcelplating_CheckedChanged(object sender, EventArgs e)
        {
            SettingsTarget.MarcelPlating = marcelplating.Checked;
            DoUpdate();
        }

        private void Rounding_ValueChanged(object sender, EventArgs e)
        {
            SettingsTarget.Rounding = (float)Rounding.Value * 1.0f; ;
            DoUpdate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SettingsTarget.DistanceMaskFile = openFileDialog1.FileName;
                SettingsTarget.ReloadMask = true;
                DoUpdate();
            }
        }
    }
}
