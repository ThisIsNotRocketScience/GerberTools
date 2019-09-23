using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberLibrary.Core.Primitives
{
    public class GerberNumberFormat
    {
        public int DigitsBefore;
        public int DigitsAfter;
        public bool OmitLeading; // otherwise trailing;
        public bool Relativemode;

        public override string ToString()
        {
            return String.Format("before: {0}, after: {1}, omitleading: {2}, relativemode: {3}, scale: {4}", DigitsBefore, DigitsAfter, OmitLeading, Relativemode, CurrentNumberScale);

        }

        public double Decode(string Numbers, bool hasdecimalpoint)
        {

            double R = 0;
            bool invert = false;
            if (Numbers[0] == '-')
            {
                invert = true;
                Numbers = Numbers.Substring(1);
            }
            if (Numbers.IndexOf('.') > -1 && hasdecimalpoint == true)
            {
                double D = 0;
                Double.TryParse(Numbers, NumberStyles.Any, CultureInfo.InvariantCulture, out D);
                if (invert) D = -D;
                return D;
            }
            while (Numbers.Length < DigitsAfter + DigitsBefore)
            {
                if (OmitLeading)
                {
                    Numbers = "0" + Numbers;
                }
                else
                {
                    Numbers = Numbers + "0";
                }
            }

            for (int i = 0; i < Numbers.Length; i++)
            {
                R = R + int.Parse(Numbers[i].ToString());
                R = R * 10;
            }

            R = R / Math.Pow(10, DigitsAfter + 1);

            if (invert) return -R;
            return R;
        }

        public void Parse(string p)
        {
            GCodeCommand GCC = new GCodeCommand();
            GCC.Decode(p, new GerberNumberFormat());
            GerberSplitter GS = new GerberSplitter();
            GS.Split(p, new GerberNumberFormat());
            if (true)//GCC.charcommands[2] == 'S')
            {
                if (p.IndexOf('L') > p.IndexOf('T'))
                {
                    OmitLeading = true;
                }
                else
                {
                    OmitLeading = false;
                    // omit trailing zeroes
                }
                if (p.IndexOf('A') > p.IndexOf('I'))
                {
                    Relativemode = false;
                }
                else
                {
                    Relativemode = true;
                }

                int X1 = (int)GS.GetByLastChar("X");

                int X2 = (int)GS.GetByLastChar("Y");


                if (X1 != X2)
                {
                    Console.WriteLine("Format has different precisions for X and Y coordinates - not supported yet.");
                }

                DigitsBefore = X1 / 10;
                DigitsAfter = X1 % 10; ;
                if (Gerber.ShowProgress)
                {
                    Console.WriteLine("Gerber number format: {0} before, {1} after. Omitleading: {2}", DigitsBefore, DigitsAfter, OmitLeading);
                }
            }

        }

        public string Format(double p)
        {
            Int64 R = (Int64)(p * Math.Pow(10, DigitsAfter));
            return R.ToString("D" + (DigitsAfter + DigitsBefore).ToString());
        }

        public string BuildGerberFormatLine()
        {
            return string.Format("%FSLAX{0}{1}Y{0}{1}*%", DigitsBefore.ToString("D1"), DigitsAfter.ToString("D1"));

        }

        // public void DecodeGerber(string GerberLine)
        // {
        // }

        public double Multiplier = 25.4;

        public double ScaleFileToMM(double Val)
        {
            return Val * Multiplier;
        }

        public double _ScaleMMToFile(double Val)
        {
            return Val / Multiplier;
        }

        public enum NumberScale
        {
            Metric,
            Imperial
        }

        public NumberScale CurrentNumberScale = NumberScale.Imperial;

        public void SetImperialMode()
        {
            CurrentNumberScale = NumberScale.Imperial;
            Multiplier = 25.4d;
        }

        public void SetMetricMode()
        {
            CurrentNumberScale = NumberScale.Metric;
            Multiplier = 1.0d;
        }

        public string BuildMetricImperialFormatLine()
        {
            if (CurrentNumberScale == NumberScale.Imperial) return Gerber.INCH;
            return Gerber.MM;
        }



        public GerberQuadrantMode CurrentQuadrantMode = GerberQuadrantMode.Single;

        internal void SetSingleQuadrantMode()
        {
            CurrentQuadrantMode = GerberQuadrantMode.Single;
            if (Gerber.ShowProgress) Console.WriteLine("QuadrantMode: Single");

        }

        internal void SetMultiQuadrantMode()
        {
            CurrentQuadrantMode = GerberQuadrantMode.Multi;
            if (Gerber.ShowProgress) Console.WriteLine("QuadrantMode: Multi");
        }
    }

}
