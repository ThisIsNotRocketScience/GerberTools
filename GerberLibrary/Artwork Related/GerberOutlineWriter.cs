using GerberLibrary;
using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GerberLibrary
{
    public class GerberOutlineWriter
    {
        List<PolyLine> PolyLines = new List<PolyLine>();
        public void AddPolyLine(PolyLine a)
        {
            PolyLines.Add(a);
        }

        public void Write(string p, double inWidth = 0.0)
        {
            List<string> lines = new List<string>();

            GerberNumberFormat GNF = new GerberNumberFormat();
            GNF.DigitsBefore = 4;
            GNF.DigitsAfter = 4;
            GNF.SetImperialMode();

            lines.Add(Gerber.INCH);
            lines.Add("%OFA0B0*%");
            lines.Add(GNF.BuildGerberFormatLine());
            lines.Add("%IPPOS*%");
            lines.Add("%LPD*%");
            GerberApertureType Apt = new GerberApertureType();
            Apt.SetCircle(inWidth);
            Apt.ID = 10;
            lines.Add(Apt.BuildGerber(GNF));
            lines.Add(Apt.BuildSelectGerber());
            foreach (var a in PolyLines)
            {
                lines.Add(Gerber.MoveTo(a.Vertices[a.Vertices.Count-1], GNF));
                for (int i = 0; i < a.Vertices.Count; i++)
                {
                    lines.Add(Gerber.LineTo(a.Vertices[i], GNF));
                }
            }
            lines.Add(Gerber.EOF);
            Gerber.WriteAllLines(p, lines);
        }
    }

}
