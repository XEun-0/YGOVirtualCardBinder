using DigitalCardBinder.Mk3.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DigitalCardBinder.Mk3
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        static Form ss;
        static Form main;
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ss = new SplashScreen();
            var splashThread = new Thread(new ThreadStart(
                () => Application.Run(ss)));
            splashThread.SetApartmentState(ApartmentState.STA);
            splashThread.Start();

            main = new Body();
            main.Load += Load_Completed;
            Application.Run(main);
        }

        public static void Load_Completed(object sender, EventArgs e)
        {
            if (ss != null && !ss.Disposing && !ss.IsDisposed)
                ss.Invoke(new Action(() => ss.Close()));
            main.TopMost = true;
            main.Activate();
            main.TopMost = false;
        }
    }
}
