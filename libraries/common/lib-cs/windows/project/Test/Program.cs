using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpringCard.LibCs;
using SpringCard.LibCs.Windows;

namespace Test
{
    static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SystemConsole.Show();
            Logger.TraceLevel = Logger.Level.All;
            Logger.ConsoleLevel = Logger.Level.All;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
