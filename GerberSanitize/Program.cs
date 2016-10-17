using GerberLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberSanitize
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() == 0)
            {
                Console.WriteLine("Usage: GernerSanitize <file1> <file2> <fileN>");
            }
            else
            {
                for (int i = 0; i < args.Count(); i++)
                {
                    PolyLineSet.SanitizeInputLines(File.ReadAllLines(args[i]).ToList(), args[i] + ".sanitized.txt");
                }
            }
        }
    }
}
