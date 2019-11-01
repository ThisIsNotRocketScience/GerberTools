namespace TINRS_ArtWorkGenerator
{
    partial class SettingsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDialog));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.delaunayRadio = new System.Windows.Forms.RadioButton();
            this.radioSubstitution = new System.Windows.Forms.RadioButton();
            this.radioQuadTree = new System.Windows.Forms.RadioButton();
            this.maxdepthbox = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.rotationbox = new System.Windows.Forms.NumericUpDown();
            this.UpdateCheckbox = new System.Windows.Forms.CheckBox();
            this.UpdateButton = new System.Windows.Forms.Button();
            this.TileTypes = new System.Windows.Forms.ListBox();
            this.invertsource = new System.Windows.Forms.CheckBox();
            this.invertoutput = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.Thresholdlevelbox = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.scalesmallerbox = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.scalesmallerlevelbox = new System.Windows.Forms.NumericUpDown();
            this.alwaysSubdivide = new System.Windows.Forms.CheckBox();
            this.symmetry = new System.Windows.Forms.CheckBox();
            this.SuperSymmetry = new System.Windows.Forms.CheckBox();
            this.xscalinglevel = new System.Windows.Forms.Label();
            this.Xscaling = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.xscalecenterperc = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.generalScale = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.distanceToMaskScale = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.distanceToMaskRange = new System.Windows.Forms.NumericUpDown();
            this.marcelplating = new System.Windows.Forms.CheckBox();
            this.ballradius = new System.Windows.Forms.NumericUpDown();
            this.gap = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.Rounding = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxdepthbox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rotationbox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Thresholdlevelbox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scalesmallerbox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scalesmallerlevelbox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Xscaling)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xscalecenterperc)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.generalScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.distanceToMaskScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.distanceToMaskRange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ballradius)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Rounding)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.delaunayRadio);
            this.groupBox1.Controls.Add(this.radioSubstitution);
            this.groupBox1.Controls.Add(this.radioQuadTree);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(200, 128);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "RenderMode";
            // 
            // delaunayRadio
            // 
            this.delaunayRadio.AutoSize = true;
            this.delaunayRadio.Location = new System.Drawing.Point(5, 86);
            this.delaunayRadio.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.delaunayRadio.Name = "delaunayRadio";
            this.delaunayRadio.Size = new System.Drawing.Size(165, 21);
            this.delaunayRadio.TabIndex = 2;
            this.delaunayRadio.TabStop = true;
            this.delaunayRadio.Text = "DelaunayConnectivity";
            this.delaunayRadio.UseVisualStyleBackColor = true;
            this.delaunayRadio.CheckedChanged += new System.EventHandler(this.delaunayRadio_CheckedChanged);
            // 
            // radioSubstitution
            // 
            this.radioSubstitution.AutoSize = true;
            this.radioSubstitution.Location = new System.Drawing.Point(5, 59);
            this.radioSubstitution.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.radioSubstitution.Name = "radioSubstitution";
            this.radioSubstitution.Size = new System.Drawing.Size(137, 21);
            this.radioSubstitution.TabIndex = 1;
            this.radioSubstitution.TabStop = true;
            this.radioSubstitution.Text = "SubstitutionTiling";
            this.radioSubstitution.UseVisualStyleBackColor = true;
            this.radioSubstitution.CheckedChanged += new System.EventHandler(this.radioSubstitution_CheckedChanged);
            // 
            // radioQuadTree
            // 
            this.radioQuadTree.AutoSize = true;
            this.radioQuadTree.Location = new System.Drawing.Point(5, 32);
            this.radioQuadTree.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.radioQuadTree.Name = "radioQuadTree";
            this.radioQuadTree.Size = new System.Drawing.Size(94, 21);
            this.radioQuadTree.TabIndex = 0;
            this.radioQuadTree.TabStop = true;
            this.radioQuadTree.Text = "QuadTree";
            this.radioQuadTree.UseVisualStyleBackColor = true;
            this.radioQuadTree.CheckedChanged += new System.EventHandler(this.radioQuadTree_CheckedChanged);
            // 
            // maxdepthbox
            // 
            this.maxdepthbox.Location = new System.Drawing.Point(137, 196);
            this.maxdepthbox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.maxdepthbox.Maximum = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.maxdepthbox.Name = "maxdepthbox";
            this.maxdepthbox.Size = new System.Drawing.Size(120, 22);
            this.maxdepthbox.TabIndex = 1;
            this.maxdepthbox.ValueChanged += new System.EventHandler(this.maxdepthbox_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(39, 196);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Max Depth";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(31, 224);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Degrees offset";
            // 
            // rotationbox
            // 
            this.rotationbox.Location = new System.Drawing.Point(137, 224);
            this.rotationbox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rotationbox.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.rotationbox.Name = "rotationbox";
            this.rotationbox.Size = new System.Drawing.Size(120, 22);
            this.rotationbox.TabIndex = 3;
            this.rotationbox.ValueChanged += new System.EventHandler(this.rotationbox_ValueChanged);
            // 
            // UpdateCheckbox
            // 
            this.UpdateCheckbox.AutoSize = true;
            this.UpdateCheckbox.Location = new System.Drawing.Point(397, 289);
            this.UpdateCheckbox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.UpdateCheckbox.Name = "UpdateCheckbox";
            this.UpdateCheckbox.Size = new System.Drawing.Size(105, 21);
            this.UpdateCheckbox.TabIndex = 5;
            this.UpdateCheckbox.Text = "AutoUpdate";
            this.UpdateCheckbox.UseVisualStyleBackColor = true;
            this.UpdateCheckbox.CheckedChanged += new System.EventHandler(this.UpdateCheckbox_CheckedChanged);
            // 
            // UpdateButton
            // 
            this.UpdateButton.Location = new System.Drawing.Point(317, 289);
            this.UpdateButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.UpdateButton.Name = "UpdateButton";
            this.UpdateButton.Size = new System.Drawing.Size(75, 23);
            this.UpdateButton.TabIndex = 6;
            this.UpdateButton.Text = "Update";
            this.UpdateButton.UseVisualStyleBackColor = true;
            this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
            // 
            // TileTypes
            // 
            this.TileTypes.FormattingEnabled = true;
            this.TileTypes.ItemHeight = 16;
            this.TileTypes.Location = new System.Drawing.Point(283, 21);
            this.TileTypes.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TileTypes.Name = "TileTypes";
            this.TileTypes.Size = new System.Drawing.Size(201, 212);
            this.TileTypes.TabIndex = 7;
            this.TileTypes.SelectedIndexChanged += new System.EventHandler(this.TileTypes_SelectedIndexChanged);
            // 
            // invertsource
            // 
            this.invertsource.AutoSize = true;
            this.invertsource.Location = new System.Drawing.Point(20, 378);
            this.invertsource.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.invertsource.Name = "invertsource";
            this.invertsource.Size = new System.Drawing.Size(114, 21);
            this.invertsource.TabIndex = 8;
            this.invertsource.Text = "Invert Source";
            this.invertsource.UseVisualStyleBackColor = true;
            this.invertsource.CheckedChanged += new System.EventHandler(this.invertsource_CheckedChanged);
            // 
            // invertoutput
            // 
            this.invertoutput.AutoSize = true;
            this.invertoutput.Location = new System.Drawing.Point(20, 405);
            this.invertoutput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.invertoutput.Name = "invertoutput";
            this.invertoutput.Size = new System.Drawing.Size(159, 21);
            this.invertoutput.TabIndex = 9;
            this.invertoutput.Text = "Invert Output Bitmap";
            this.invertoutput.UseVisualStyleBackColor = true;
            this.invertoutput.CheckedChanged += new System.EventHandler(this.invertoutput_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(32, 345);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 17);
            this.label3.TabIndex = 11;
            this.label3.Text = "Thresholdlevel";
            // 
            // Thresholdlevelbox
            // 
            this.Thresholdlevelbox.Location = new System.Drawing.Point(139, 345);
            this.Thresholdlevelbox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Thresholdlevelbox.Name = "Thresholdlevelbox";
            this.Thresholdlevelbox.Size = new System.Drawing.Size(120, 22);
            this.Thresholdlevelbox.TabIndex = 10;
            this.Thresholdlevelbox.ValueChanged += new System.EventHandler(this.Thresholdlevelbox_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(31, 265);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 17);
            this.label4.TabIndex = 13;
            this.label4.Text = "Scalesmaller";
            // 
            // scalesmallerbox
            // 
            this.scalesmallerbox.Location = new System.Drawing.Point(137, 265);
            this.scalesmallerbox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.scalesmallerbox.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.scalesmallerbox.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.scalesmallerbox.Name = "scalesmallerbox";
            this.scalesmallerbox.Size = new System.Drawing.Size(120, 22);
            this.scalesmallerbox.TabIndex = 12;
            this.scalesmallerbox.ValueChanged += new System.EventHandler(this.scalesmallerbox_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(32, 288);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 17);
            this.label5.TabIndex = 15;
            this.label5.Text = "Level";
            // 
            // scalesmallerlevelbox
            // 
            this.scalesmallerlevelbox.Location = new System.Drawing.Point(139, 288);
            this.scalesmallerlevelbox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.scalesmallerlevelbox.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.scalesmallerlevelbox.Name = "scalesmallerlevelbox";
            this.scalesmallerlevelbox.Size = new System.Drawing.Size(120, 22);
            this.scalesmallerlevelbox.TabIndex = 14;
            this.scalesmallerlevelbox.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.scalesmallerlevelbox.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // alwaysSubdivide
            // 
            this.alwaysSubdivide.AutoSize = true;
            this.alwaysSubdivide.Location = new System.Drawing.Point(20, 432);
            this.alwaysSubdivide.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.alwaysSubdivide.Name = "alwaysSubdivide";
            this.alwaysSubdivide.Size = new System.Drawing.Size(139, 21);
            this.alwaysSubdivide.TabIndex = 16;
            this.alwaysSubdivide.Text = "Always Subdivide";
            this.alwaysSubdivide.UseVisualStyleBackColor = true;
            this.alwaysSubdivide.CheckedChanged += new System.EventHandler(this.alwaysSubdivide_CheckedChanged);
            // 
            // symmetry
            // 
            this.symmetry.AutoSize = true;
            this.symmetry.Location = new System.Drawing.Point(20, 459);
            this.symmetry.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.symmetry.Name = "symmetry";
            this.symmetry.Size = new System.Drawing.Size(129, 21);
            this.symmetry.TabIndex = 17;
            this.symmetry.Text = "Symmetric Start";
            this.symmetry.UseVisualStyleBackColor = true;
            this.symmetry.CheckedChanged += new System.EventHandler(this.symmetry_CheckedChanged);
            // 
            // SuperSymmetry
            // 
            this.SuperSymmetry.AutoSize = true;
            this.SuperSymmetry.Location = new System.Drawing.Point(155, 459);
            this.SuperSymmetry.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.SuperSymmetry.Name = "SuperSymmetry";
            this.SuperSymmetry.Size = new System.Drawing.Size(130, 21);
            this.SuperSymmetry.TabIndex = 18;
            this.SuperSymmetry.Text = "SuperSymmetry";
            this.SuperSymmetry.UseVisualStyleBackColor = true;
            this.SuperSymmetry.CheckedChanged += new System.EventHandler(this.SuperSymmetry_CheckedChanged);
            // 
            // xscalinglevel
            // 
            this.xscalinglevel.AutoSize = true;
            this.xscalinglevel.Location = new System.Drawing.Point(29, 316);
            this.xscalinglevel.Name = "xscalinglevel";
            this.xscalinglevel.Size = new System.Drawing.Size(97, 17);
            this.xscalinglevel.TabIndex = 20;
            this.xscalinglevel.Text = "XScalingLevel";
            // 
            // Xscaling
            // 
            this.Xscaling.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.Xscaling.Location = new System.Drawing.Point(137, 316);
            this.Xscaling.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Xscaling.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.Xscaling.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            -2147483648});
            this.Xscaling.Name = "Xscaling";
            this.Xscaling.Size = new System.Drawing.Size(120, 22);
            this.Xscaling.TabIndex = 19;
            this.Xscaling.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.Xscaling.ValueChanged += new System.EventHandler(this.Xscaling_ValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(271, 345);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(134, 17);
            this.label6.TabIndex = 22;
            this.label6.Text = "XScalingCenterPerc";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // xscalecenterperc
            // 
            this.xscalecenterperc.Location = new System.Drawing.Point(272, 364);
            this.xscalecenterperc.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.xscalecenterperc.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.xscalecenterperc.Name = "xscalecenterperc";
            this.xscalecenterperc.Size = new System.Drawing.Size(120, 22);
            this.xscalecenterperc.TabIndex = 21;
            this.xscalecenterperc.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.xscalecenterperc.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged_1);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(281, 410);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(200, 17);
            this.label7.TabIndex = 24;
            this.label7.Text = "GeneralScale (1000 = neutral)";
            // 
            // generalScale
            // 
            this.generalScale.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.generalScale.Location = new System.Drawing.Point(283, 430);
            this.generalScale.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.generalScale.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.generalScale.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
            this.generalScale.Name = "generalScale";
            this.generalScale.Size = new System.Drawing.Size(120, 22);
            this.generalScale.TabIndex = 23;
            this.generalScale.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.generalScale.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged_2);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(31, 527);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(148, 17);
            this.label8.TabIndex = 26;
            this.label8.Text = "DistanceToMaskScale";
            // 
            // distanceToMaskScale
            // 
            this.distanceToMaskScale.Location = new System.Drawing.Point(189, 524);
            this.distanceToMaskScale.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.distanceToMaskScale.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.distanceToMaskScale.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.distanceToMaskScale.Name = "distanceToMaskScale";
            this.distanceToMaskScale.Size = new System.Drawing.Size(120, 22);
            this.distanceToMaskScale.TabIndex = 25;
            this.distanceToMaskScale.ValueChanged += new System.EventHandler(this.distanceToMaskScale_ValueChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(31, 560);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(155, 17);
            this.label9.TabIndex = 28;
            this.label9.Text = "DistanceToMaskRange";
            // 
            // distanceToMaskRange
            // 
            this.distanceToMaskRange.Location = new System.Drawing.Point(189, 558);
            this.distanceToMaskRange.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.distanceToMaskRange.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.distanceToMaskRange.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.distanceToMaskRange.Name = "distanceToMaskRange";
            this.distanceToMaskRange.Size = new System.Drawing.Size(120, 22);
            this.distanceToMaskRange.TabIndex = 27;
            this.distanceToMaskRange.ValueChanged += new System.EventHandler(this.distanceToMaskRange_ValueChanged);
            // 
            // marcelplating
            // 
            this.marcelplating.AutoSize = true;
            this.marcelplating.Location = new System.Drawing.Point(51, 618);
            this.marcelplating.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.marcelplating.Name = "marcelplating";
            this.marcelplating.Size = new System.Drawing.Size(111, 21);
            this.marcelplating.TabIndex = 29;
            this.marcelplating.Text = "MarcelPlates";
            this.marcelplating.UseVisualStyleBackColor = true;
            this.marcelplating.CheckedChanged += new System.EventHandler(this.marcelplating_CheckedChanged);
            // 
            // ballradius
            // 
            this.ballradius.Location = new System.Drawing.Point(272, 651);
            this.ballradius.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ballradius.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.ballradius.Name = "ballradius";
            this.ballradius.Size = new System.Drawing.Size(120, 22);
            this.ballradius.TabIndex = 31;
            this.ballradius.ValueChanged += new System.EventHandler(this.ballradius_ValueChanged);
            // 
            // gap
            // 
            this.gap.Location = new System.Drawing.Point(272, 618);
            this.gap.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gap.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.gap.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.gap.Name = "gap";
            this.gap.Size = new System.Drawing.Size(120, 22);
            this.gap.TabIndex = 30;
            this.gap.ValueChanged += new System.EventHandler(this.gap_ValueChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(223, 620);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(35, 17);
            this.label10.TabIndex = 32;
            this.label10.Text = "Gap";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(191, 654);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(75, 17);
            this.label11.TabIndex = 33;
            this.label11.Text = "BallRadius";
            // 
            // Rounding
            // 
            this.Rounding.Location = new System.Drawing.Point(275, 687);
            this.Rounding.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Rounding.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.Rounding.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.Rounding.Name = "Rounding";
            this.Rounding.Size = new System.Drawing.Size(120, 22);
            this.Rounding.TabIndex = 34;
            this.Rounding.ValueChanged += new System.EventHandler(this.Rounding_ValueChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(191, 689);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(69, 17);
            this.label12.TabIndex = 35;
            this.label12.Text = "Rounding";
            // 
            // SettingsDialog
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(509, 725);
            this.ControlBox = false;
            this.Controls.Add(this.label12);
            this.Controls.Add(this.Rounding);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.ballradius);
            this.Controls.Add(this.gap);
            this.Controls.Add(this.marcelplating);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.distanceToMaskRange);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.distanceToMaskScale);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.generalScale);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.xscalecenterperc);
            this.Controls.Add(this.xscalinglevel);
            this.Controls.Add(this.Xscaling);
            this.Controls.Add(this.SuperSymmetry);
            this.Controls.Add(this.symmetry);
            this.Controls.Add(this.alwaysSubdivide);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.scalesmallerlevelbox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.scalesmallerbox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Thresholdlevelbox);
            this.Controls.Add(this.invertoutput);
            this.Controls.Add(this.invertsource);
            this.Controls.Add(this.TileTypes);
            this.Controls.Add(this.UpdateButton);
            this.Controls.Add(this.UpdateCheckbox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.rotationbox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.maxdepthbox);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Settings";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.SettingsDialog_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.SettingsDialog_DragEnter);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxdepthbox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rotationbox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Thresholdlevelbox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.scalesmallerbox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.scalesmallerlevelbox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Xscaling)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xscalecenterperc)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.generalScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.distanceToMaskScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.distanceToMaskRange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ballradius)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gap)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Rounding)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioSubstitution;
        private System.Windows.Forms.RadioButton radioQuadTree;
        private System.Windows.Forms.NumericUpDown maxdepthbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown rotationbox;
        private System.Windows.Forms.CheckBox UpdateCheckbox;
        private System.Windows.Forms.Button UpdateButton;
        private System.Windows.Forms.ListBox TileTypes;
        private System.Windows.Forms.CheckBox invertsource;
        private System.Windows.Forms.CheckBox invertoutput;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown Thresholdlevelbox;
        private System.Windows.Forms.RadioButton delaunayRadio;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown scalesmallerbox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown scalesmallerlevelbox;
        private System.Windows.Forms.CheckBox alwaysSubdivide;
        private System.Windows.Forms.CheckBox symmetry;
        private System.Windows.Forms.CheckBox SuperSymmetry;
        private System.Windows.Forms.Label xscalinglevel;
        private System.Windows.Forms.NumericUpDown Xscaling;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown xscalecenterperc;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown generalScale;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown distanceToMaskScale;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown distanceToMaskRange;
        private System.Windows.Forms.CheckBox marcelplating;
        private System.Windows.Forms.NumericUpDown ballradius;
        private System.Windows.Forms.NumericUpDown gap;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown Rounding;
        private System.Windows.Forms.Label label12;
    }
}