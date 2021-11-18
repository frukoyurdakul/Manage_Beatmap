using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Globalization;

namespace Manage_Beatmap
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Manage_Beatmap());
        }

        public static string GetDecimalSeparator()
        {
            return Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        }

        public static string GetOriginalDecimalSeparator()
        {
            return ".";
        }
    }
}
