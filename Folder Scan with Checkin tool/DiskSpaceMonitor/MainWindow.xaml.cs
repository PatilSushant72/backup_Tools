//using System.Text;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

//namespace DiskSpaceMonitor
//{
//    /// <summary>
//    /// Interaction logic for MainWindow.xaml
//    /// </summary>
//    public partial class MainWindow : Window
//    {
//        public MainWindow()
//        {
//            InitializeComponent();
//        }
//    }
//}
using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace DiskSpaceMonitor
{
    public partial class MainWindow : Window
    {
        private const long LIMIT_GB = 25;
        private const string DRIVE_NAME = "C";

        public MainWindow()
        {
            InitializeComponent();

            // Run after UI thread starts (even if window is hidden)
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    CheckDiskAndSendMail();
                }
                catch (Exception ex)
                {
                    try
                    {
                        string folderPath = @"C:\DiskManager";
                        string logFilePath = Path.Combine(folderPath, "DiskSpaceMonitor_Error.log");

                        // Create folder if not exists
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        File.AppendAllText(
                            logFilePath,
                            DateTime.Now + " - " + ex + Environment.NewLine
                        );
                    }
                    catch
                    {
                        // Avoid crash if logging itself fails
                    }
                }

                finally
                {
                    Application.Current.Shutdown();
                }
            }), DispatcherPriority.Background);
        }

        private void CheckDiskAndSendMail()
        {
            DriveInfo drive = new DriveInfo(DRIVE_NAME);

            if (!drive.IsReady)
                return;

            long freeGB = drive.AvailableFreeSpace / (1024 * 1024 * 1024);

            if (freeGB < LIMIT_GB)
            {
                SendOutlookMail(freeGB);
            }
        }

        private void SendOutlookMail(long freeGB)
        {
            Outlook.Application outlook = null;
            Outlook.MailItem mail = null;

            try
            {
                outlook = new Outlook.Application();

                mail = (Outlook.MailItem)outlook.CreateItem(
                    Outlook.OlItemType.olMailItem);

                mail.Subject = "⚠ C Drive Disk Space Warning";

                mail.Body =
                    "Automatic warning:\n\n" +
                    $"C drive free space is below {LIMIT_GB} GB.\n\n" +
                    $"Current free space: {freeGB} GB\n\n" +
                    "Please take action.";

                // Multiple recipients are correct using comma-separated values
                // Multiple recipients are correct using semicolon-separated values
                //mail.To = "sandipc@wipropari.com; prasad.w@wipropari.com; harshal.g@wipropari.com";
                mail.To = "priti.d.3@wipropari.com; sushant.p.1@wipropari.com";


                mail.Send();
            }
            finally
            {
                // Release COM objects properly
                if (mail != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(mail);

                if (outlook != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(outlook);
            }
        

    }
}
}
