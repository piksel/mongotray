using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace dbHandler
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
            var f = new FormConsole();
            f.ShowInTaskbar = false;
            f.WindowState = FormWindowState.Minimized;
            f.Visible = false;
            Application.Run(f);
        }
    }
}
