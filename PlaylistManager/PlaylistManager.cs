using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace YAPS
{
    static class PlaylistManager
    {
        public static bool play = true;
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}