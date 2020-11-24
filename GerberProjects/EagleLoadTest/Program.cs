using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GerberLibrary;
using GerberLibrary.Eagle;

namespace EagleLoadTest
{
    class Program
    {
        public class Led
        {
            public string Name;
            public double X;
            public double Y;

            public string net1 = "";
            public string net2 = "";

        }
     

        static void Main(string[] args)
        {

            CultureInfo ci = new CultureInfo("nl-NL");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            
            
            List<Led> Leds = new List<Led>();

            String sch = args[0];
            String brd = args[1];
            EagleLoader EB = new EagleLoader(brd);
            EagleLoader ES = new EagleLoader(sch);
            SchematicLoader ES2 = new SchematicLoader();
            ES2.LoadSchematic(EB, sch);
            foreach (var A in EB.DevicePlacements)
            {
                if (A.desc != "LEDBOARD")
                {
                    Leds.Add(new Led() { net1 = A.desc, X = A.x, Y = A.y });
                    Console.WriteLine("{0},{1}, {2}-{3}-{4}", A.x, A.y, A.desc, A.name, A.value);
                }
            }

            foreach (var b in ES2.Parts)
            {
                Console.WriteLine(b);
            }
            foreach (var b in ES2.PartPlacement)
            {
                Console.WriteLine(b);
            }
            foreach (var a in ES.Nets)
            {

                Console.WriteLine(a);
            }

            Console.ReadKey();

        }
    }
}
