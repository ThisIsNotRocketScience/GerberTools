using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GerberLibrary;
using GerberLibrary.Core;

namespace OpampCalculator
{
    public partial class OpampCalculator : Form
    {
        public class ResistorComboItem
        {
            public double range;
            public double baseval;
            public override string ToString()
            {
                return Helpers.MakeNiceUnitString(range * baseval, Units.Ohm);

            }

            public double Get()
            {
                return range * baseval;
            }
        }
        public OpampCalculator()
        {
            InitializeComponent();

            foreach (var j in Helpers.ResistorRanges)
            {
                foreach (var i in Helpers.E24)
                {
                    FeedbackInputCombo.Items.Add(new ResistorComboItem() { baseval = i, range = j });
                }
            }


            List<double> Ecombo = new List<double>();
            Ecombo.AddRange(Helpers.E24);
            Ecombo.AddRange(Helpers.E48);
            //Ecombo.AddRange(Helpers.E96);
            Ecombo.Sort();
            Ecombo = Ecombo.Distinct().ToList();


            foreach (var j in Helpers.ResistorRanges)
            {

                foreach (var i in Ecombo)
                {
                    OffsetCombo.Items.Add(new ResistorComboItem() { baseval = i, range = j });
                    InputCombo.Items.Add(new ResistorComboItem() { baseval = i, range = j });
                    FeedbackCombo.Items.Add(new ResistorComboItem() { baseval = i, range = j });
                }
            }


            OffsetCombo.SelectedIndex = 0;
            InputCombo.SelectedIndex = 0;
            FeedbackCombo.SelectedIndex = 0;
            FeedbackInputCombo.SelectedIndex = 0;
            SetResistorComboTo(FeedbackInputCombo, 10000);
        }

        double GetNum(string T)
        {
            T = T.Trim().Replace(".", ",");
            try
            {
                return Double.Parse(T);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        void DoCalc()
        {
            double inMin = GetNum(InVoltMin.Text);
            double inMax = GetNum(InVoltMax.Text);
            double outMin = GetNum(OutVoltMin.Text);
            double outMax = GetNum(OutvoltMax.Text);
            double inputoffset= GetNum(OffsetVoltage.Text);
            double safemargin = GetNum(SafetyMargin.Text);

            double indiff = Math.Abs(inMax - inMin) + Math.Abs(safemargin * 2);
            double outdiff = Math.Abs(outMax - outMin);
            double scalar = indiff / outdiff;
            double offset = (inMin + inMax)/2 ;
            double outoffset = (outMin + outMax) / 2;
            var offsetshift = outoffset - offset;


            double Cselectedfeedbackresistor = ((ResistorComboItem)FeedbackInputCombo.SelectedItem).Get();

            var ResistorMatches = Helpers.GetE24FitForRatioAndSecond(scalar, Cselectedfeedbackresistor);
            var OffsetMatches = Helpers.GetE24FitForRatioAndSecond(Math.Abs(inputoffset / offsetshift), Cselectedfeedbackresistor);

            inputresistor = ResistorMatches[0].res1;
            feedbackresistor = ResistorMatches[0].res2;
            offsetresistor = OffsetMatches[0].res1;

            OffsetResistor.Text = Helpers.MakeNiceUnitString(offsetresistor, Units.Ohm);
            InputResistor.Text = Helpers.MakeNiceUnitString(inputresistor, Units.Ohm);
            FeedbackResistor.Text = Helpers.MakeNiceUnitString(feedbackresistor, Units.Ohm);

            Scale.Text = scalar.ToString();
            Offset.Text = offsetshift.ToString();

            double V1 = -(inMax / inputresistor + inputoffset / offsetresistor) * feedbackresistor;
            double V2 = -(inMin / inputresistor + inputoffset / offsetresistor) * feedbackresistor;
            OutVoltActualMax.Text = Math.Max(V2, V1).ToString("N4");
            OutVoltActualMin.Text = Math.Min(V2, V1).ToString("N4");


            double Cinputresistor = ((ResistorComboItem)InputCombo.SelectedItem).Get();
            double Coffsetresistor = ((ResistorComboItem)OffsetCombo.SelectedItem).Get();
            double Cfeedbackresistor = ((ResistorComboItem)FeedbackCombo.SelectedItem).Get();

            double cV1 = -(inMax / Cinputresistor + inputoffset / Coffsetresistor) * Cfeedbackresistor;
            double cV2 = -(inMin / Cinputresistor + inputoffset / Coffsetresistor) * Cfeedbackresistor;
            SelectedMaxBox.Text = Math.Max(cV2, cV1).ToString("N4");
            SelectedMinBox.Text = Math.Min(cV2, cV1).ToString("N4");

        }






        double inputresistor;
        double feedbackresistor;
         double offsetresistor;



        private void timer1_Tick(object sender, EventArgs e)
        {
            DoCalc();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SetResistorComboTo(FeedbackCombo, feedbackresistor);
            SetResistorComboTo(InputCombo, inputresistor);
            SetResistorComboTo(OffsetCombo, offsetresistor);

        }

        private void SetResistorComboTo(ComboBox Comb, double feedbackresistor)
        {
            int current = 0;
            foreach(ResistorComboItem a in Comb.Items)
            {
                if (Math.Abs(a.range * a.baseval - feedbackresistor)<0.1)
                {
                    Comb.SelectedIndex = current;
                    return;
                }
                current++;
            }

            current++;
        }
    }
}
