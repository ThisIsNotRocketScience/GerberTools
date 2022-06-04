using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BOMConsolidator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() <2)

            {
                return;
            }

            GerberLibrary.Core.BOM B = new GerberLibrary.Core.BOM();
            B.LoadJLC(args[0], args[1]);
            B.WriteJLCBom(Path.GetDirectoryName(args[0]), Path.GetFileNameWithoutExtension(args[0]) + "_newcondens");
        }
    }
}
