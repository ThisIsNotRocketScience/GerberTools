using GerberLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JLCDrop
{
    static class Program
    {
        enum Argument
        {
            None, 
            FrameName, 
            FrameMM
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            CultureInfo ci = new CultureInfo("nl-NL");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            if (args.Count() > 0)
            {
                Argument NextArgument = Argument.None;
                List<String> SourceFiles = new List<string>();
                int framemm = 5;
                bool doframe = false;
                bool zipgerbers = true;
                string framename = DateTime.Now.ToShortDateString();


                foreach (var a in args)
                {
                    switch (NextArgument)
                    {
                        case Argument.FrameMM: framemm = int.Parse(a); NextArgument = Argument.FrameName; break;
                        case Argument.FrameName: framename = a; NextArgument = Argument.None; break;
                        case Argument.None:
                            switch (a)
                            {
                                case "-zip": zipgerbers = true; NextArgument = Argument.None; break;
                                case "-dontzip": zipgerbers = false;  NextArgument = Argument.None; break;
                                case "-frame": doframe= true; NextArgument = Argument.FrameMM; break;
                                case "-noframe": doframe= false; NextArgument = Argument.None; break;

                                case "-help":
                                case "/?":
                                case "-?":
                                case "--help":
                                case "--?":
                                    Console.WriteLine("JLCDrop <-zip/-dontzip> <-frame mm name/-noframe> eagleordiptracefiles..");
                                    break;
                                default:
                                    SourceFiles.Add(a);
                                    break;
                            }
                            break;
                    }
                }
                foreach (var a in SourceFiles)
                {

                    try
                    {
                        Factory.MakeBomAndPlacement(a);
                        if (zipgerbers) Factory.ZipGerbers(a, doframe, framemm, framename);

                    }
                    catch(Exception E)
                    {
                        Console.Write("Something went wrong for {0}: {1}", a, E);
                    }

                }
            }
            else
            {

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new JLCDropForm());
            }
        }
    }
}
