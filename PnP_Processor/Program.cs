using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PnP_Processor
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        enum Argument
        {
            None,
            ZipFile,
            BomFile,
            PnpFile,
            FlipMode,
            __Count,
            Stock
        };

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

                AttachConsole(ATTACH_PARENT_PROCESS);
                Argument NextArgument = Argument.None;
                PnPProcDoc.FlipMode Mode = PnPProcDoc.FlipMode.NoFlip;
                PnPProcDoc d = new PnPProcDoc();
                foreach(var a in args)
                {


                    switch(NextArgument)
                    {
                        case Argument.ZipFile: d.gerberzip = a;NextArgument = Argument.None; break;
                        case Argument.FlipMode: d.FlipBoard= PnPProcDoc.DecodeFlip(a); NextArgument = Argument.None; break;
                        case Argument.PnpFile: d.pnp = a; NextArgument = Argument.None; break;
                        case Argument.BomFile: d.bom = a; NextArgument = Argument.None; break;
                        case Argument.Stock: d.stock = a; NextArgument = Argument.None; break;
                        case Argument.None:
                            switch(a)
                            {
                                case "-zip": NextArgument = Argument.ZipFile;break;
                                case "-bom": NextArgument = Argument.BomFile;break;
                                case "-pnp": NextArgument = Argument.PnpFile;break;
                                case "-mode": NextArgument = Argument.FlipMode;break;
                                case "-stock": NextArgument = Argument.Stock;break;
                                case "-help":
                                case "/?":
                                case "-?":
                                case "--help":
                                case "--?":
                                    Console.WriteLine("PnP_Processor <-stock stockfile> <-zip zipfile> <-pnp pnpfile> <-bom bomfile> <-mode none|diagonal|horizontal>");
                                    break;

                            }
                            break;
                    }
                } 
                int LastIDX = 0;

                d.StartLoad();
                while (d.loaded == false)
                {
                    while (LastIDX < d.Log.Count())
                    {
                        Console.WriteLine(d.Log[LastIDX]);
                        LastIDX++;
                    }
                    
                    System.Threading.Thread.Sleep(4);
                }
                Console.WriteLine("done loading and processing!");
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                Application.Exit();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PnPMain(args));
        }
    }
}
