using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using OfficeOpenXml;
using System.Windows;

namespace FolderScan
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // ✅ EPPlus 7.x – correct & supported
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            base.OnStartup(e);
        }
    }
}


//using System.Windows;

//namespace FolderScan
//{
//    public partial class App : Application
//    {
//        protected override void OnStartup(StartupEventArgs e)
//        {
//            base.OnStartup(e);

//            // 🔐 LICENSE CHECK AT EXE START
//            if (!LicenseBootstrap.EnsureLicense())
//            {
//                Shutdown();
//                return;
//            }

//            // ✅ License OK → Load Main Window
//            MainWindow window = new MainWindow();
//            window.Show();
//        }
//    }
//}
