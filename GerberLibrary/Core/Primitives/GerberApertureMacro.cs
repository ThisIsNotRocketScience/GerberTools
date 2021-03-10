using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberLibrary.Core.Primitives
{
    public class GerberApertureMacro
    {
        public List<GerberApertureMacroPart> Parts = new List<GerberApertureMacroPart>();
        public string Name;

        /// <summary>
        /// Has this macro been defined in the file yet?
        /// </summary>
        public bool Written = false;


        public GerberApertureType BuildAperture(List<double> paramlist, GerberNumberFormat GNF)
        {
            
            GerberApertureType GAT = new GerberApertureType();
            GAT.MacroParamList = paramlist.ToList();
            GAT.ShapeType = GerberApertureShape.Compound;
            //GAT.ShapeType = GerberApertureShape.Macro;
            GAT.MacroName = Name;

            GAT.Shape.Vertices.Clear();
            if (Gerber.ShowProgress) Console.WriteLine("Building aperture for macro {1}  with {0} parts", Parts.Count, Name);
            for (int i = 0; i < Parts.Count(); i++)
            {
                var part = Parts[i];
                var Part = part.BuildAperture(paramlist, GNF);
                Part.Polarity = Parts[i].Polarity;
                GAT.Parts.Add(Part);

                if (Gerber.ShowProgress) Console.WriteLine("part with {0} vertices", Part.Shape.Count());


            }
            return GAT;

        }

        public string BuildGerber(GerberNumberFormat GNF, double p)
        {
            string res = "%AM" + Name + "*" + Gerber.LineEnding;
            foreach (var part in Parts)
            {
                res += part.BuildGerber(GNF, p);
            }
            res += "%" + Gerber.LineEnding;

            return res;
        }
    }
}
