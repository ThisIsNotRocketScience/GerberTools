using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GerberCombinerBuilder
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //string myCulture = "en-US";
            //Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(myCulture);
            //Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(myCulture);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GerberPanelizerParent());
        }
    }
}
