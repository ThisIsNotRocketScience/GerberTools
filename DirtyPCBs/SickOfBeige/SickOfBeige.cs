using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SickOfBeige
{
    class SickOfBeige: GerberLibrary.ProgressLog
    {
        static void Main(string[] args)
        {
            GerberLibrary.Core.SickOfBeige Box = new GerberLibrary.Core.SickOfBeige();
            List<string> Files = new List<string>() { args[0]};
            Box.AddBoardsToSet(Files, true, new SickOfBeige());
            double offset = double.Parse(args[2]);
            Box.MinimalDXFSave(args[1], offset);
        }

        public void AddString(string text, float progress = -1F)
        {
            
        }
    }
}
