using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeIcon
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() < 3)
            {
                Console.WriteLine("MakeIcon <filename> <label> <number>");
                return;
            }
            string filename = args[0];
            string lbl = args[1];
            double O = 0;
            double.TryParse(args[2], out O);

            Artwork.TINRSArtWorkRenderer.SaveMultiIcon(filename, lbl, (float)O); 
        }
    }
}
