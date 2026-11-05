//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Windows.Forms;

//namespace ProjectWiseMailApp
//{
//    internal static class Program
//    {
//        /// <summary>
//        /// The main entry point for the application.
//        /// </summary>
//        [STAThread]
//        static void Main()
//        {
//            Application.EnableVisualStyles();
//            Application.SetCompatibleTextRenderingDefault(false);
//            Application.Run(new Form1());
//        }
//    }
//}
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace ProjectWiseMailApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // =========================================
            // 🔐 LICENSE CHECK (FIRST STEP)
            // =========================================
            var license = new LicenseManager();
            if (!license.EnsureLicense())
            {
                MessageBox.Show("License validation failed.", "License Error");
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            
        }
    }
}