using GerberLibrary;
using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DirtyLocaleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //CultureInfo ci = new CultureInfo("en-US");
            //Thread.CurrentThread.CurrentCulture = ci;
            //Thread.CurrentThread.CurrentUICulture = ci;
            
            GerberSplitter GS = new GerberSplitter();
            GerberNumberFormat GNF = new GerberNumberFormat();
            GNF.DigitsBefore = 2;
            GNF.DigitsAfter = 4;
            GNF.OmitLeading = true;

            GS.Split("X1.4917Y-2.1589", GNF, true);
            foreach(var a in GS.Pairs)
            {
                Console.WriteLine("{0}:{1}", a.Key, a.Value.Number);
            }
            Console.ReadKey();
        }
    }
}
