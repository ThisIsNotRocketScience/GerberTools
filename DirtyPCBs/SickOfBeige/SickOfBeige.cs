using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SickOfBeige
{
    class SickOfBeige: GerberLibrary.ProgressLog
    {
        enum Arguments
        {
            Offset,
            HoleDiameter,
            None
        }

        static void Main(string[] args)
        {

            double offset = 3.0f;
            double holediameter = 3.2f;


            if (args.Count() < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("SickOfBeige.exe  [--offset {mm}]");
                Console.WriteLine("\t[--holediameter {mm}]");
                Console.WriteLine("\tinput_path");
                Console.WriteLine("\toutput_path");

                return;
            }

            Arguments NextArg = Arguments.None;
            for (int i = 0; i < args.Count() - 2; i++)
            {
                switch (NextArg)
                {
                    case Arguments.Offset: offset = Double.Parse(args[i]); NextArg = Arguments.None; break;
                    case Arguments.HoleDiameter: holediameter = Double.Parse(args[i]); NextArg = Arguments.None; break;
                    case Arguments.None:
                        switch (args[i].ToLower())
                        {
                            case "--offset": NextArg = Arguments.Offset; break;
                            case "--holediameter": NextArg = Arguments.HoleDiameter; break;
                        }
                        break;
                }
            }

            var InputFolder = args[args.Count() - 2];
            var OutputFolder = args[args.Count() - 1];


            GerberLibrary.Core.SickOfBeige Box = new GerberLibrary.Core.SickOfBeige();
            List<string> Files = new List<string>() { InputFolder };
            Box.AddBoardsToSet(Files, true, new SickOfBeige());
            Box.MinimalDXFSave(OutputFolder, offset, holediameter);
            foreach(var a in Box.Errors)
            {
                Console.WriteLine("ERROR: {0}", a);
            }
        }

        public void AddString(string text, float progress = -1F)
        {
            
        }
    }
}
