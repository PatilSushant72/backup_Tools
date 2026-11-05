//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.IO.Compression;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Forms;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
//using MessageBox = System.Windows.MessageBox;
//using System.IO.Compression;
//namespace FolderScan
//{
//    /// <summary>                            
//    /// Interaction logic for MainWindow.xaml
//    /// </summary>                           
//    public partial class MainWindow : Window
//    {
//        List<string> customeOption1 = new List<string>() { "As is", "Empty" };
//        List<string> customeOption2 = new List<string>() { "Yes", "No" };
//        string downloadRoot = @"C:\OneDriveDownload\Extracted";
//        static string oneDriveBackupRoot =
//    @"C:\Users\P01090879\OneDrive - Wipro PARI (1)\BackUpAllCheckin";

//        public MainWindow()
//        {
//            InitializeComponent();
//            //cmbUpdateSupplierChangeReason.ItemsSource=customeOption1;               
//            //cmbUpdateSupplierChangeReason.SelectedValue = customeOption1[1];        
//            //cmbUpdateSupplierComment.ItemsSource = customeOption1;                  
//            //cmbUpdateSupplierComment.SelectedValue = customeOption1[1];             
//            //cmbUpdateVersionSupplierDeliveryNote.ItemsSource = customeOption2;      
//            //cmbUpdateVersionSupplierDeliveryNote.SelectedValue = customeOption2[0]; 
//            //cmbUpdateProjectData.ItemsSource = customeOption2;                      
//            //cmbUpdateProjectData.SelectedValue = customeOption2[0];                 
//            //cmbUpdateReadme.ItemsSource = customeOption2;                           
//            //cmbUpdateReadme.SelectedValue = customeOption2[0];                      

//        }

//        //private void Browse_Click(object sender, RoutedEventArgs e)
//        //{
//        //    var folderSearch = new FolderBrowserDialog();

//        //    if (System.Windows.Forms.DialogResult.OK == folderSearch.ShowDialog())
//        //    {
//        //        Path.Text = folderSearch.SelectedPath;
//        //    }
//        //}
//        //string CSVName;
//        //string ExtraFileName;
//        //long sizeInKb;
//        private void Browse_Click(object sender, RoutedEventArgs e)
//        {
//            var folderSearch = new FolderBrowserDialog();

//            if (System.Windows.Forms.DialogResult.OK == folderSearch.ShowDialog())
//            {
//                Path.Text = folderSearch.SelectedPath;
//            }
//        }
//        string CSVName;
//        string ExtraFileName;
//        long sizeInKb;

//        //private async void btnGenerateCSV_Click(object sender, RoutedEventArgs e)
//        //{
//        //    try
//        //    {
//        //        strSize = Size.Text;
//        //        sizeInKb = (strSize.All(char.IsDigit) ? Convert.ToInt64(strSize) : 24) * 1000;
//        //        Size.Text = strSize.All(char.IsDigit) ? strSize : "24";
//        //        CSVName = @"C:\Temp\";
//        //        if (!Directory.Exists(CSVName))
//        //        {
//        //            Directory.CreateDirectory(CSVName);
//        //        }

//        //        if (!Directory.Exists(Path.Text))
//        //        {
//        //            MessageBox.Show("Please check folder path", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//        //            return;
//        //        }
//        //        CSVName = System.IO.Path.Combine(CSVName, "ProjectFolderList" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss") + ".csv");
//        //        ExtraFileName = System.IO.Path.Combine(@"C:\Temp\", "Files" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss") + ".csv");

//        //        File.AppendAllText(CSVName, "Folder List" + "," + "Date" + "," + "Time" + "\n");
//        //        File.AppendAllText(ExtraFileName, "Folder Name,File Name" + "\n");
//        //        RunningFolder.Content = Path.Text;
//        //        btnGenerateCSV.IsEnabled = false;
//        //        List<Task> lstTsk = new List<Task>();
//        //        foreach (var item in Directory.GetDirectories(Path.Text))
//        //        {
//        //            RunningFolder.Content = item;
//        //            var tsk = Task.Run(async () =>
//        //            {
//        //                ScanDirectory(item);
//        //            });
//        //            //await tsk;                                                                                                        
//        //            lstTsk.Add(tsk);
//        //        }
//        //        foreach (var item in lstTsk)
//        //        {
//        //            await item;
//        //        }
//        //        MessageBox.Show("CSV Created Sucessfully");
//        //        btnGenerateCSV.IsEnabled = true;
//        //        // =================================================
//        //        // ============ ADD + COMBINE (FIXED) ==============
//        //        // =================================================

//        //        var projectPaths = ReadFolderListFromCsv(CSVName);

//        //        if (Directory.Exists(downloadRoot))
//        //            Directory.Delete(downloadRoot, true);

//        //        Directory.CreateDirectory(downloadRoot);

//        //        foreach (var srcPath in projectPaths)
//        //        {
//        //            if (!Directory.Exists(srcPath))
//        //                continue;

//        //            string dest = System.IO.Path.Combine(
//        //                downloadRoot,
//        //                new DirectoryInfo(srcPath).Name + "_" +
//        //                DateTime.Now.ToString("yyyyMMdd_HHmmss"));

//        //            CopyDirectory(srcPath, dest);



//        //        }
//        private async void btnGenerateCSV_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                string strSize = Size.Text;
//                sizeInKb = (strSize.All(char.IsDigit) ? Convert.ToInt64(strSize) : 24) * 1000;
//                Size.Text = strSize.All(char.IsDigit) ? strSize : "24";

//                string tempRoot = @"C:\Temp\";
//                if (!Directory.Exists(tempRoot))
//                    Directory.CreateDirectory(tempRoot);

//                if (!Directory.Exists(Path.Text))
//                {
//                    MessageBox.Show("Please check folder path", "Error",
//                        MessageBoxButton.OK, MessageBoxImage.Error);
//                    return;
//                }

//                CSVName = System.IO.Path.Combine(
//                    tempRoot,
//                    "ProjectFolderList_" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss") + ".csv");

//                ExtraFileName = System.IO.Path.Combine(
//                    tempRoot,
//                    "Files_" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss") + ".csv");

//                // CSV Headers (MATCH YOUR SCREENSHOT)
//                File.WriteAllText(CSVName, "Folder List,Date,Time\n");
//                File.WriteAllText(ExtraFileName, "Folder Name,File Name\n");

//                RunningFolder.Content = Path.Text;
//                btnGenerateCSV.IsEnabled = false;

//                List<Task> lstTsk = new List<Task>();

//                foreach (var item in Directory.GetDirectories(Path.Text))
//                {
//                    var folder = item;

//                    lstTsk.Add(Task.Run(() =>
//                    {
//                        ScanDirectory(folder);   // YOUR EXISTING LOGIC
//                    }));
//                }

//                await Task.WhenAll(lstTsk);

//                MessageBox.Show("CSV Created Successfully");
//                btnGenerateCSV.IsEnabled = true;

//                // =================================================
//                // ============ COPY + EXTRACT =====================
//                // =================================================

//                var projectEntries = ReadFolderListFromCsv(CSVName);

//                if (Directory.Exists(downloadRoot))
//                    Directory.Delete(downloadRoot, true);

//                Directory.CreateDirectory(downloadRoot);

//                foreach (var entry in projectEntries)
//                {
//                    if (!Directory.Exists(entry.Path))
//                        continue;

//                    string dest = System.IO.Path.Combine(
//                        downloadRoot,
//                        new DirectoryInfo(entry.Path).Name + "_" +
//                        entry.Time.ToString("yyyyMMdd_HHmmss"));

//                    CopyDirectory(entry.Path, dest);

//                    // 🔽 Extract ZIP files if present
//                    ExtractZipFilesInFolder(dest);
//                }

//        // 👉 SECOND EXE LOGIC (NOW SAME EXE)
//        AutomationRunner.Run();
//                 // 🔥 RUN AUTOMATION IN BACKGROUND(IMPORTANT)
//                Task.Run(() =>
//                {
//                    AutomationRunner.Run();
//                });
//                // Read CSV paths (already exists in your code)
//                List<string> csvProjectPaths = ReadFolderListFromCsv1(CSVName);

//                // --- your existing check-in logic here ---
//                // project by project check-in
//                // no changes
//                // ----------------------------------------

//                // 🔥 AFTER ALL CHECK-INS COMPLETED
//                MoveCsvProjectsToOneDriveBackup(csvProjectPaths);
//                // =================================================
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(ex.Message);
//            }
//            finally
//            {
//                btnGenerateCSV.IsEnabled = true;
//            }
//        }


//        // =========================================================
//        // ADD METHODS (NO REMOVAL)
//        // =========================================================
//        List<string> ReadFolderListFromCsv1(string csvPath)
//        {
//            return File.ReadAllLines(csvPath)
//                .Skip(1)
//                .Where(l => !string.IsNullOrWhiteSpace(l))
//                .Select(l => l.Split(',')[0].Trim('"'))
//                .Distinct()
//                .ToList();
//        }
//        List<(string Path, DateTime Time)> ReadFolderListFromCsv(string csvPath)
//        {
//            return File.ReadAllLines(csvPath)
//                .Skip(1) // skip header
//                .Where(l => !string.IsNullOrWhiteSpace(l))
//                .Select(l =>
//                {
//                    var cols = l.Split(',');

//                    string folderPath = cols[0].Trim();
//                    string datePart = cols[1].Trim();
//                    string timePart = cols[2].Trim();

//                    DateTime dateTimeFromCsv =
//                        DateTime.ParseExact(
//                            $"{datePart} {timePart}",
//                            "dd/MM/yyyy HH:mm:ss",
//                            System.Globalization.CultureInfo.InvariantCulture);

//                    return (folderPath, dateTimeFromCsv);
//                })
//                .ToList();
//        }


//        void ExtractZipFilesInFolder(string folderPath)
//        {
//            var zipFiles = Directory.GetFiles(folderPath, "*.zip", SearchOption.AllDirectories);

//            foreach (var zip in zipFiles)
//            {
//                try
//                {
//                    string parentDir = System.IO.Path.GetDirectoryName(zip);

//                    // Folder name = ZIP file name (without .zip)
//                    string extractDir = System.IO.Path.Combine(
//                        parentDir,
//                        System.IO.Path.GetFileNameWithoutExtension(zip));

//                    // Create extraction folder
//                    if (!Directory.Exists(extractDir))
//                        Directory.CreateDirectory(extractDir);

//                    // Extract inside its own folder
//                    ZipFile.ExtractToDirectory(zip, extractDir);
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine(ex.Message);
//                }
//            }
//        }
//        void ScanDirectory1(string folderPath)
//        {
//            lock (this)
//            {
//                File.AppendAllText(
//                    CSVName,
//                    $"{folderPath},{DateTime.Now:dd/MM/yyyy},{DateTime.Now:HH:mm:ss}\n");
//            }

//            // rest of your existing ScanDirectory logic
//        }

//        void CopyDirectory(string source, string target)
//        {
//            Directory.CreateDirectory(target);

//            foreach (var f in Directory.GetFiles(source))
//                File.Copy(f, System.IO.Path.Combine(target, System.IO.Path.GetFileName(f)), true);

//            foreach (var d in Directory.GetDirectories(source))
//                CopyDirectory(d, System.IO.Path.Combine(target, System.IO.Path.GetFileName(d)));
//            MessageBox.Show("Project Added in Download Folder");
//        }


//        //string strSize = "24";
//        //List<string> filesNames = new List<string>() { "ProjectData.zip", "Readme.txt", "SupplierChangeReason.txt", "SupplierComment.txt", "SupplierDeliveryNote.vdog-sdn" };
//        //public void ScanDirectory(string directoryInfo)
//        //{
//        //    try
//        //    {
//        //        lock (directoryInfo)
//        //        {
//        //            foreach (var file in Directory.GetFiles(directoryInfo))
//        //            {
//        //                FileInfo fileInfo = new FileInfo(file);
//        //                if (fileInfo.Length >= sizeInKb && fileInfo.Name.Contains("ProjectData.zip"))
//        //                {
//        //                    lock (CSVName)
//        //                    {
//        //                        string line = $"\"{fileInfo.DirectoryName}\",{fileInfo.Directory.CreationTime:yyyy-MM-dd},{fileInfo.Directory.CreationTime:HH:mm:ss}{Environment.NewLine}";
//        //                        //File.AppendAllText(CSVName, "\"" + fileInfo.DirectoryName + "\"," + fileInfo.CreationTime.Date+ "," + fileInfo.CreationTime.TimeOfDay + Environment.NewLine);
//        //                        File.AppendAllText(CSVName, line);
//        //                        //File.AppendAllText(CSVName, "\""+fileInfo.DirectoryName+"\"" + "\n");                                            
//        //                        //return;                                                                                                        
//        //                    }

//        //                }
//        //                if (filesNames.FindIndex(f => f.ToLower() == fileInfo.Name.ToLower()) < 0)
//        //                {
//        //                    lock (ExtraFileName)
//        //                    {
//        //                        File.AppendAllText(ExtraFileName, "\"" + fileInfo.DirectoryName + "\"" + "," + "\"" + fileInfo.Name + "\"" + "\n");
//        //                        //return;                                                                                                        
//        //                    }
//        //                }
//        //                if (fileInfo.Name.Contains("ProjectData.zip"))
//        //                {
//        //                    foreach (var dfile in Directory.GetDirectories(directoryInfo))
//        //                    {
//        //                        DirectoryInfo Master = new DirectoryInfo(directoryInfo);
//        //                        DirectoryInfo dfileInfo = new DirectoryInfo(dfile);
//        //                        lock (ExtraFileName)
//        //                        {
//        //                            File.AppendAllText(ExtraFileName, "\"" + Master.FullName + "\"" + "," + "\"" + dfileInfo.Name + "\"" + "\n");
//        //                            //return;                                                                                                    
//        //                        }
//        //                    }
//        //                }
//        //            }
//        //            foreach (var item in Directory.GetDirectories(directoryInfo))
//        //            {
//        //                lock (RunningFolder)
//        //                {
//        //                    Action invokeAction = new Action(() => { RunningFolder.Content = item; });
//        //                    RunningFolder.Dispatcher.Invoke(invokeAction);
//        //                }
//        //                ScanDirectory(item);
//        //            }
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        MessageBox.Show(ex.Message);
//        //    }

//        //}
//        string strSize = "24";
//        List<string> filesNames = new List<string>()
//{
//    "ProjectData.zip",
//    "Readme.txt",
//    "SupplierChangeReason.txt",
//    "SupplierComment.txt",
//    "SupplierDeliveryNote.vdog-sdn"
//};

//        public void ScanDirectory(string directoryInfo)
//        {
//            try
//            {
//                // ================================
//                // ADDITION: CSV entry per folder
//                // ================================
//                lock (CSVName)
//                {
//                    File.AppendAllText(
//                        CSVName,
//                        $"{directoryInfo},{DateTime.Now:dd/MM/yyyy},{DateTime.Now:HH:mm:ss}{Environment.NewLine}");
//                }

//                lock (directoryInfo)
//                {
//                    foreach (var file in Directory.GetFiles(directoryInfo))
//                    {
//                        FileInfo fileInfo = new FileInfo(file);

//                        // ================================
//                        // Existing ZIP size logic (UNCHANGED)
//                        // ================================
//                        if (fileInfo.Length >= sizeInKb && fileInfo.Name.Contains("ProjectData.zip"))
//                        {
//                            lock (CSVName)
//                            {
//                                string line =
//                                    $"\"{fileInfo.DirectoryName}\"," +
//                                    $"{fileInfo.Directory.CreationTime:dd/MM/yyyy}," +
//                                    $"{fileInfo.Directory.CreationTime:HH:mm:ss}" +
//                                    Environment.NewLine;

//                                File.AppendAllText(CSVName, line);
//                            }
//                        }

//                        // ================================
//                        // Extra files logging (UNCHANGED)
//                        // ================================
//                        if (filesNames.FindIndex(f => f.Equals(fileInfo.Name, StringComparison.OrdinalIgnoreCase)) < 0)
//                        {
//                            lock (ExtraFileName)
//                            {
//                                File.AppendAllText(
//                                    ExtraFileName,
//                                    $"\"{fileInfo.DirectoryName}\",\"{fileInfo.Name}\"{Environment.NewLine}");
//                            }
//                        }

//                        // ================================
//                        // Sub-folder logging for ZIP (UNCHANGED)
//                        // ================================
//                        if (fileInfo.Name.Contains("ProjectData.zip"))
//                        {
//                            foreach (var dfile in Directory.GetDirectories(directoryInfo))
//                            {
//                                DirectoryInfo master = new DirectoryInfo(directoryInfo);
//                                DirectoryInfo dfileInfo = new DirectoryInfo(dfile);

//                                lock (ExtraFileName)
//                                {
//                                    File.AppendAllText(
//                                        ExtraFileName,
//                                        $"\"{master.FullName}\",\"{dfileInfo.Name}\"{Environment.NewLine}");
//                                }
//                            }
//                        }
//                    }

//                    // ================================
//                    // Recursive scan (UNCHANGED)
//                    // ================================
//                    foreach (var item in Directory.GetDirectories(directoryInfo))
//                    {
//                        lock (RunningFolder)
//                        {
//                            RunningFolder.Dispatcher.Invoke(() =>
//                            {
//                                RunningFolder.Content = item;
//                            });
//                        }

//                        ScanDirectory(item);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(ex.Message);
//            }
//        }

//        private void updateBrowse_Click(object sender, RoutedEventArgs e)
//        {
//            var folderSearch = new FolderBrowserDialog();

//            if (System.Windows.Forms.DialogResult.OK == folderSearch.ShowDialog())
//            {
//                //UpdatePath.Text = folderSearch.SelectedPath;                         
//            }
//        }
//        string cmbProjectData, cmbReadme, cmbSupplierC, cmbSupplierCR, cmbVersionSdn;
//        //private async void btnUpdateFolder_Click(object sender, RoutedEventArgs e)                               
//        //{                                                                                                        
//        //    strSize = Size.Text;                                                                                 
//        //    sizeInKb = (strSize.All(char.IsDigit) ? Convert.ToInt64(strSize) : 24) * 1000;                       
//        //    Size.Text = strSize.All(char.IsDigit) ? strSize : "24";                                              
//        //    if (!Directory.Exists(UpdatePath.Text))                                                              
//        //    {                                                                                                    
//        //        MessageBox.Show("Please check folder path", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//        //        return;                                                                                          
//        //    }                                                                                                    
//        //    UpdateRunningFolder.Content = UpdatePath.Text;                                                       
//        //    btnUpdateFolder.IsEnabled = false;                                                                   
//        //    cmbProjectData = cmbUpdateProjectData.Text;                                                          
//        //    cmbReadme = cmbUpdateReadme.Text;                                                                    
//        //    cmbSupplierC = cmbUpdateSupplierComment.Text;                                                        
//        //    cmbSupplierCR = cmbUpdateSupplierChangeReason.Text;                                                  
//        //    cmbVersionSdn = cmbUpdateVersionSupplierDeliveryNote.Text;                                           
//        //    List<Task> lstTsk = new List<Task>();                                                                
//        //    foreach (var item in Directory.GetDirectories(UpdatePath.Text))                                      
//        //    {                                                                                                    
//        //        UpdateRunningFolder.Content = item;                                                              
//        //        var tsk = Task.Run(async () =>                                                                   
//        //        {                                                                                                
//        //            ScanDirectoryUpdateFolder(item);                                                             
//        //        });                                                                                              
//        //        //await tsk;                                                                                     
//        //        lstTsk.Add(tsk);                                                                                 
//        //    }                                                                                                    
//        //    foreach (var item in lstTsk)                                                                         
//        //    {                                                                                                    
//        //        await item;                                                                                      
//        //    }                                                                                                    
//        //    MessageBox.Show("Folder Updated Sucessfully");                                                       
//        //    btnUpdateFolder.IsEnabled = true;                                                                    
//        //}                                                                                                        
//        // ============================================================
//        // FINAL BACKUP TO ONEDRIVE (AFTER ALL CHECKINS)
//        // ============================================================
//        // ============================================================
//        // MOVE ORIGINAL ONEDRIVE PROJECTS (FROM CSV PATHS)
//        // ============================================================
//        static void MoveCsvProjectsToOneDriveBackup(List<string> csvProjectPaths)
//        {
//            try
//            {
//                string backupRoot =
//                    @"C:\Users\P01090879\OneDrive - Wipro PARI (1)\BackUpAllCheckin";

//                Directory.CreateDirectory(backupRoot);

//                string doneFolder =
//                    System.IO.Path.Combine(
//                        backupRoot,
//                        $"CheckIn Done_{DateTime.Now:yyyyMMdd_HHmmss}");

//                Directory.CreateDirectory(doneFolder);

//                foreach (var projectPath in csvProjectPaths)
//                {
//                    if (!Directory.Exists(projectPath))
//                        continue;

//                    string targetPath =
//                        System.IO.Path.Combine(doneFolder, System.IO.Path.GetFileName(projectPath));

//                    // Safety: avoid overwrite
//                    if (Directory.Exists(targetPath))
//                    {
//                        targetPath += "_" +
//                            DateTime.Now.ToString("HHmmss");
//                    }

//                    Directory.Move(projectPath, targetPath);
//                }
//            }
//            catch (Exception ex)
//            {
//                File.AppendAllText(
//                    @"C:\Temp\OneDriveMoveError.log",
//                    ex + Environment.NewLine);
//            }
//        }


//        public void ScanDirectoryUpdateFolder(string directoryInfo)
//        {
//            try
//            {
//                lock (directoryInfo)
//                {
//                    foreach (var file in Directory.GetFiles(directoryInfo))
//                    {
//                        FileInfo fileInfo = new FileInfo(file);
//                        if (fileInfo.Length >= sizeInKb && fileInfo.Name.Contains("ProjectData.zip"))
//                        {
//                            lock (fileInfo)
//                            {
//                                foreach (var file2 in Directory.GetFiles(directoryInfo))
//                                {
//                                    FileInfo fileInfo2 = new FileInfo(file2);
//                                    if (fileInfo2.Name.ToLower().Contains(filesNames[0].ToLower()))
//                                    {
//                                        /////ProjectData.Zip                                                                                                
//                                        if (cmbProjectData == "Yes")
//                                        {
//                                            using (ZipArchive archive = ZipFile.Open(fileInfo2.FullName, ZipArchiveMode.Update))
//                                            {
//                                            GetEntries:
//                                                var entries = archive.Entries;
//                                                foreach (ZipArchiveEntry item in entries)
//                                                {
//                                                    item.Delete();
//                                                    goto GetEntries;
//                                                }
//                                            }

//                                        }
//                                    }
//                                    else if (fileInfo2.Name.ToLower().Contains(filesNames[1].ToLower()))
//                                    {
//                                        ////ReadMe.txt                                                                                                      
//                                        if (cmbReadme == "Yes")
//                                        {
//                                            var data = File.ReadAllText("Data.txt");
//                                            if (fileInfo2.Name == "readme.txt")
//                                            {
//                                                File.Delete(fileInfo2.FullName);
//                                                File.WriteAllText(System.IO.Path.Combine(fileInfo2.DirectoryName, "Readme.txt"), data);
//                                            }
//                                            else
//                                            {
//                                                File.WriteAllText(fileInfo2.DirectoryName, data);
//                                            }
//                                        }
//                                    }
//                                    else if (fileInfo2.Name.ToLower().Contains(filesNames[2].ToLower()))
//                                    {
//                                        /////SupplierChangeReason.txt                                                                                       
//                                        if (cmbSupplierCR == "Empty")
//                                        {
//                                            File.WriteAllText(fileInfo2.FullName, "");
//                                        }
//                                    }
//                                    else if (fileInfo2.Name.ToLower().Contains(filesNames[3].ToLower()))
//                                    {
//                                        /////SupplierCOmment.txt                                                                                            
//                                        if (cmbSupplierC == "Empty")
//                                        {
//                                            File.WriteAllText(fileInfo2.FullName, "dd_" + DateTime.Now.ToString("_MMM_yyyy") + ", Updated by: Name of person");
//                                        }
//                                    }
//                                    else if (fileInfo2.Name.ToLower().Contains(filesNames[4].ToLower()))
//                                    {
//                                        /////SupplierDeliveryNote.vdog-sdn                                                                                  
//                                        if (cmbVersionSdn == "Yes")
//                                        {
//                                            var data = File.ReadAllText(fileInfo2.FullName);
//                                            Console.WriteLine(data);
//                                            var FinIndex = data.IndexOf("Number");
//                                            var str = data.Substring(FinIndex + 7, 2);
//                                            var arr = str.Where(a => char.IsDigit(a));
//                                            str = string.Join("", arr);
//                                            var data2 = data.Replace(">" + str + "<", ">0<");
//                                            Console.WriteLine(data2);
//                                            File.WriteAllText(fileInfo2.FullName, data2);
//                                        }
//                                    }

//                                }
//                            }
//                        }

//                    }
//                    foreach (var item in Directory.GetDirectories(directoryInfo))
//                    {
//                        //lock (UpdateRunningFolder)                                                                                                          
//                        //{                                                                                                                                   
//                        //    Action invokeAction = new Action(() => { UpdateRunningFolder.Content = item; });                                                
//                        //    UpdateRunningFolder.Dispatcher.Invoke(invokeAction);                                                                            
//                        //}                                                                                                                                   
//                        ScanDirectoryUpdateFolder(item);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(ex.Message);
//            }

//        }
//    }
//}
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO;
using System.IO.Compression;
using System.IO.Compression;
using System.Linq;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows;   // WPF MessageBox
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using System.Windows.Forms;
using System.Xml.Linq;
using static FolderScan.AutomationRunner;
using MessageBox = System.Windows.MessageBox;
using NetworkInterface = System.Net.NetworkInformation.NetworkInterface;
using Outlook = Microsoft.Office.Interop.Outlook;

//using TextBox = System.Windows.Forms.TextBox;
//using Button = System.Windows.Forms.Button;
//using Label = System.Windows.Forms.Label;
//using System.Windows.Forms;

namespace FolderScan
{
    /// <summary>                            
    /// Interaction logic for MainWindow.xaml
    /// </summary>                           
    public partial class MainWindow : Window
    {
        List<string> customeOption1 = new List<string>() { "As is", "Empty" };
        List<string> customeOption2 = new List<string>() { "Yes", "No" };
        string downloadRoot = @"C:\OneDriveDownload\Extracted";
        static string oneDriveBackupRoot =
    @"C:\Users\P01090879\OneDrive - Wipro PARI (1)\BackUpAllCheckin";
        ProgressForm progressForm;

        public MainWindow()
        {
            InitializeComponent();

            // 🔐 LICENSE CHECK FIRST
            if (!EnsureLicenseAtStartup())
            {
                Application.Current.Shutdown();
                return;
            }

            // ---- rest of your existing UI init stays SAME ----


            // If no application key exists → generate
            if (string.IsNullOrEmpty(ApplicationKey))
            {
                string deviceName = Environment.MachineName + ".PC";
                this.GetApplicationKey(deviceName);
            }

            // If no validation key OR wrong validation key → show license form
            if (string.IsNullOrEmpty(ValidationKey) || !this.
VerifyKey())
            {
                LicenseForm lf = new LicenseForm(ApplicationKey);

                //if (lf.ShowDialog() == DialogResult.OK)
                bool? result = lf.ShowDialog();
                if (result == true)
                {
                    // OK pressed

                    ValidationKey = lf.EnteredValidationKey;

                    if (!this.VerifyKey())
                    {
                        MessageBox.Show("Invalid Validation Key.");
                        return;
                    }

                    // Save Data.dat
                    SaveKeyData();
                }
                else
                {
                    return; // user closed window
                }
            }
            InitializeComponent();

            //cmbUpdateSupplierChangeReason.ItemsSource=customeOption1;               
            //cmbUpdateSupplierChangeReason.SelectedValue = customeOption1[1];        
            //cmbUpdateSupplierComment.ItemsSource = customeOption1;                  
            //cmbUpdateSupplierComment.SelectedValue = customeOption1[1];             
            //cmbUpdateVersionSupplierDeliveryNote.ItemsSource = customeOption2;      
            //cmbUpdateVersionSupplierDeliveryNote.SelectedValue = customeOption2[0]; 
            //cmbUpdateProjectData.ItemsSource = customeOption2;                      
            //cmbUpdateProjectData.SelectedValue = customeOption2[0];                 
            //cmbUpdateReadme.ItemsSource = customeOption2;                           
            //cmbUpdateReadme.SelectedValue = customeOption2[0];                      

        }
        private static string GetExplorerExactSize(long bytes)
        {
            StringBuilder sb = new StringBuilder(32);
            NativeMethods.StrFormatByteSize(bytes, sb, sb.Capacity);
            return sb.ToString();
        }
        //private void Browse_Click(object sender, RoutedEventArgs e)
        //{
        //    var folderSearch = new FolderBrowserDialog();

        //    if (System.Windows.Forms.DialogResult.OK == folderSearch.ShowDialog())
        //    {
        //        Path.Text = folderSearch.SelectedPath;
        //    }
        //}
        //string CSVName;
        //string ExtraFileName;
        //long sizeInKb;


        private string _applicationKey;
        private static string DatFile = "Data.dat";
        private string _validationKey;

        private bool? _dialogResult;
        private bool _canCloseDialog;

        private bool _isRegister;

        public static string ApplicationKey = "";

        /// <summary>
        /// Selected version of the openness api
        /// </summary>
        public static string ValidationKey = "";
        public void ReadData()
        {
            KeyData data = null;

            if (File.Exists(DatFile))
            {
                try
                {
                    data = DeSerialize<KeyData>(DatFile);
                }
                catch
                {
                    MessageBox.Show("License file corrupted. Creating new license.");
                    CreateNewLicense();
                    return;
                }
            }
            else
            {
                CreateNewLicense();
                return;
            }

            ApplicationKey = data.ApplicationKey ?? "";
            ValidationKey = data.ValidationKey ?? "";

            // 🔐 VERIFY LICENSE
            if (!VerifyKey())
            {
                MessageBox.Show(
                    "License is invalid or expired.\nA new Application Key has been generated.\n\nPlease send it to Admin.",
                    "License Reset",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                CreateNewLicense();
            }
        }

        private void CreateNewLicense()
        {
            // Generate new Application Key
            GetApplicationKey(Environment.MachineName + ".DEVICE");

            // Clear validation key
            ValidationKey = "";

            // Save new license info
            SaveKeyData();

            var result = MessageBox.Show(
                $"New Application Key Generated:\n\n{ApplicationKey}\n\nSend this key to Admin for validation.",
                "Application Key",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            // ✅ Send mail AFTER clicking OK
            if (result == MessageBoxResult.OK)
            {
                 SendApplicationKeyMail(ApplicationKey);
            }
        }
        private void SendApplicationKeyMail(string applicationKey)
        {
            try
            {
                Outlook.Application outlook = new Outlook.Application();
                Outlook.MailItem mail =
                    (Outlook.MailItem)outlook.CreateItem(Outlook.OlItemType.olMailItem);
                string senderName = Environment.UserName;
                // TO
                mail.To = "priti.d.3@wipropari.com; sushant.p.1@wipropari.com";

                // CC
                mail.CC = "sainathhb@wipropari.com; sandipc@wipropari.com";

                mail.Subject = "Request for Validation Key";

                mail.Body =
                    "Hello Sir," + Environment.NewLine + Environment.NewLine +
                    "Kindly provide the validation key for the following application key:" +
                    Environment.NewLine + Environment.NewLine +
                    applicationKey + Environment.NewLine + Environment.NewLine +
                    "Thanks & regards," + Environment.NewLine +
                    senderName;

                // ✅ Opens Outlook mail window
                //mail.Display();

                // 🔴 If you want silent auto-send instead:
                mail.Send();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to send email.\n\n" + ex.Message,
                    "Mail Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        //working
        //private void CreateNewLicense()
        //{
        //    // Generate new Application Key
        //    GetApplicationKey(Environment.MachineName + ".DEVICE");

        //    // Clear validation key
        //    ValidationKey = "";

        //    // Save new license info
        //    SaveKeyData();

        //    //MessageBox.Show(
        //    //    $"New Application Key Generated:\n\n{ApplicationKey}\n\nSend this key to Admin for validation.",
        //    //    "Application Key",
        //    //    MessageBoxButton.OK,
        //    //    MessageBoxImage.Information
        //    //);
        //}
        //public void ReadData()
        //{
        //    KeyData data = null;

        //    if (File.Exists(DatFile))
        //    {
        //        try
        //        {
        //            data = (KeyData)Serialization.DeSerialize<KeyData>(DatFile);
        //        }
        //        catch
        //        {
        //            MessageBox.Show("License file corrupted. Creating new license.");
        //            data = new KeyData();
        //            SaveKeyData();
        //        }
        //    }
        //    else
        //    {
        //        // Create new file if missing
        //        data = new KeyData();
        //        SaveKeyData();
        //    }

        //    // Apply values
        //    ApplicationKey = data.ApplicationKey ?? "";
        //    ValidationKey = data.ValidationKey ?? "";

        //    // Check expiry (2 years)
        //    var expiry = data.RegisterDate.AddDays(15);
        //    if (DateTime.Now >= expiry)
        //    {
        //        MessageBox.Show("Your licence expired.\nPlease contact the administrator.");
        //        ValidationKey = "";
        //    }
        //}
        [Serializable]
        public class KeyData
        {
            public string ApplicationKey { get; set; } = "";
            public string ValidationKey { get; set; } = "";
            public DateTime RegisterDate { get; set; } = DateTime.Now;
        }
        public static void Serialize(string filePath, object data)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, data);
            }
        }
        public static T DeSerialize<T>(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                return (T)bf.Deserialize(fs);
            }
        }

        //public static string ApplicationKeyGenerate(string data)
        //{
        //    var hexData = ToHexString(data);
        //    return AddDash(hexData);
        //}
        //private void GetApplicationKey(string fullDeviseName)
        //{
        //    //var data = GetMacAddress() + ".APL" + "." + System.Windows.Forms.SystemInformation.ComputerName;

        //    var data = GetMacAddress() + ".APL" + "." + fullDeviseName.Split('.')[1] + "." + System.Windows.Forms.SystemInformation.ComputerName;

        //    ApplicationKey = ApplicationKeyGenerate(data);
        //    var data2 = ApplicationKey.Replace("-", "");
        //    //Console.WriteLine(FromHexString(data2));
        //}
        private void GetApplicationKey(string fullDeviseName)
        {
            string mac = GetMacAddress();

            // Expecting something like: DEVICE.PC or ABC.DEVICE
            string device = fullDeviseName.Split('.')[1];

            string pc = Environment.MachineName;
            string requestDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

            // 🔑 FINAL STANDARD FORMAT
            // MAC.APL.PC.DEVICE.PC.TIME.wipropari
            string data =
                $"{mac}.APL.{pc}.{device}.PC.{requestDateTime}.wipropari";

            ApplicationKey = AddDash(ToHexString(data));
        }


        public static string GetMacAddress()
        {
            string macAddresses = string.Empty;
            var count = NetworkInterface.GetAllNetworkInterfaces().Length;
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                //Console.WriteLine(nic.Name + " " + nic.Id + " " + nic.Description);
            }
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.GetPhysicalAddress() != null && nic.Description.ToLower().Contains("realtek"))
                {
                    macAddresses += nic.GetPhysicalAddress().ToString();
                    break;
                }
            }

            return macAddresses;
        }
        public static string FromHexString(string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return Encoding.Default.GetString(bytes);
        }

        //public bool VerifyKey()
        //{
        //    bool validate = false;

        //    try
        //    {
        //        if (!string.IsNullOrEmpty(ApplicationKey) && !string.IsNullOrEmpty(ValidationKey))
        //        {
        //            var aplData = FromHexString(ApplicationKey.Replace("-", ""));
        //            var arrAplData = aplData.Split('.');

        //            var valData = FromHexString(ValidationKey.Replace("-", ""));
        //            var arrValData = valData.Split('.');

        //            validate = true;

        //            for (int i = 0, j = 0; i < arrAplData.Length; i++, j++)
        //            {
        //                if (arrAplData[i] == "APL")
        //                {
        //                    if (arrValData[i] != "VAL")
        //                        validate = false;
        //                }
        //                else
        //                {
        //                    var reverseString = string.Join("", arrValData[j].Reverse().ToList());
        //                    if (arrAplData[i] != reverseString)
        //                        validate = false;
        //                }

        //                if (!validate) break; // stop checking if invalid
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Invalid Validation Key");
        //        validate = false;
        //    }

        //    return validate;
        //}

        //public bool VerifyKey()
        //{
        //    if (string.IsNullOrEmpty(ValidationKey))
        //        return false;

        //    string raw = FromHexString(ValidationKey.Replace("-", ""));
        //    var parts = raw.Split('.');

        //    if (parts.Length < 6 || parts[1] != "VAL")
        //        return false;

        //    if (!DateTime.TryParseExact(
        //            parts[4],
        //            "yyyyMMddHHmmss",
        //            null,
        //            System.Globalization.DateTimeStyles.None,
        //            out DateTime expiry))
        //        return false;

        //    if (DateTime.Now > expiry)
        //    {
        //        MessageBox.Show("License expired. Contact administrator.");
        //        return false;
        //    }

        //    return true;
        //}
        static string expriry = "";
        static string generation = "";
        public bool VerifyKey()
        {

            if (string.IsNullOrWhiteSpace(ValidationKey))
                return false;

            string raw = FromHexString(ValidationKey.Replace("-", ""));
            string[] parts = raw.Split('.');

            string rawApplication = FromHexString(ApplicationKey.Replace("-", ""));
            string[] partsapplication = rawApplication.Split('.');
            string activepart = partsapplication[5];
            generation = activepart;
            // Expected format:
            // MAC.VAL.PC.DEVICE.PC.EXPIRY.wipropari
            if (parts.Length < 7 || !parts[1].Equals("VAL", StringComparison.OrdinalIgnoreCase))
                return false;

            string keyMac = parts[0];
            string keyPc = parts[2];
            string expiryPart = parts[5];
            expriry = expiryPart;
            // ------------------------------------------------
            // 🔐 VERIFY MACHINE BINDING
            // ------------------------------------------------
            string currentMac = GetMacAddress();
            string currentPc = Environment.MachineName;

            if (!string.Equals(keyMac, currentMac, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(keyPc, currentPc, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "License is not valid for this machine.",
                    "License Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }

            // ------------------------------------------------
            // ⏰ VERIFY EXPIRY
            // ------------------------------------------------
            if (!DateTime.TryParseExact(
                    expiryPart,
                    "yyyyMMddHHmmss",
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime expiry))
                return false;

            if (DateTime.Now > expiry)
            {
                MessageBox.Show(
                    "License expired. Contact administrator.",
                    "License Expired",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            return true;
        }


        //public static string ToHexString(string str)
        //{
        //    var sb = new StringBuilder();
        //    var bytes = Encoding.Default.GetBytes(str);
        //    foreach (var t in bytes)
        //    {
        //        sb.Append(t.ToString("X2"));
        //    }
        //    var returnData = sb.ToString();

        //    return returnData;
        //}
        public static string ToHexString(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] bytes = Encoding.UTF8.GetBytes(str);   // ✅ UTF8 (not Default)

            foreach (byte b in bytes)
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        //public static string AddDash(string str)
        //{
        //    var retrunString = "";
        //    for (int i = 0; i < str.Length; i++)
        //    {
        //        if (i % 4 == 0 && i != 0)
        //            retrunString += "-" + str[i];
        //        else
        //            retrunString += str[i];
        //    }

        //    if (retrunString.Substring(retrunString.Length - 1, 1) == "-")
        //        retrunString = retrunString.Remove(retrunString.Length - 1, 1);
        //    return retrunString;
        //}
        public static string AddDash(string str)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {
                if (i > 0 && i % 4 == 0)
                    sb.Append('-');

                sb.Append(str[i]);
            }

            return sb.ToString();
        }

        //public class Serialization
        //{
        //    public static bool Serialize<T>(string FileName, T cls)
        //    {
        //        try
        //        {
        //            BinaryFormatter bf = new BinaryFormatter();
        //            FileStream file = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None);
        //            using (file)
        //            {
        //                bf.Serialize(file, cls);
        //                return true;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            return false;
        //        }

        //    }

        //    public static object DeSerialize<T>(string FileName)
        //    {
        //        try
        //        {

        //            BinaryFormatter bf = new BinaryFormatter();
        //            FileStream file = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.None);
        //            using (file)
        //            {

        //                return bf.Deserialize(file);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            return null;
        //        }

        //    }


        public class Serialization
        {
            public static bool Serialize<T>(string fileName, T data)
            {
                try
                {
                    using (FileStream fs = new FileStream(
                        fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(fs, data);
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }

            public static T DeSerialize<T>(string fileName) where T : class
            {
                try
                {
                    using (FileStream fs = new FileStream(
                        fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        return bf.Deserialize(fs) as T;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }


        //public class KeyData
        //{
        //    public string ApplicationKey { get; set; } = "";
        //    public string ValidationKey { get; set; } = "";

        //    public DateTime RegisterDate { get; set; } = DateTime.Now;
        //}
        public class LicenseForm : Window
        {
            public string EnteredValidationKey { get; private set; } = "";

            private TextBox txtValidation;

            public LicenseForm(string applicationKey)
            {
                Title = "Key Validation";
                Width = 520;
                Height = 260;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                ResizeMode = ResizeMode.NoResize;

                Grid grid = new Grid
                {
                    Margin = new Thickness(20)
                };

                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Application Key
                grid.Children.Add(new TextBlock
                {
                    Text = "Application Key",
                    FontWeight = FontWeights.Bold
                });

                TextBox txtAppKey = new TextBox
                {
                    Text = applicationKey,
                    IsReadOnly = true,
                    Margin = new Thickness(0, 5, 0, 15)
                };
                Grid.SetRow(txtAppKey, 1);
                grid.Children.Add(txtAppKey);

                // Validation Key
                grid.Children.Add(new TextBlock
                {
                    Text = "Validation Key",
                    Margin = new Thickness(0, 10, 0, 0),
                    FontWeight = FontWeights.Bold
                });
                //Grid.SetRow(grid.Children[^1], 2);
                UIElement last = grid.Children[grid.Children.Count - 1];
                Grid.SetRow(last, 2);

                txtValidation = new TextBox
                {
                    Margin = new Thickness(0, 5, 0, 15)
                };
                Grid.SetRow(txtValidation, 3);
                grid.Children.Add(txtValidation);

                Button btnRegister = new Button
                {
                    Content = "Register",
                    Width = 100,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                btnRegister.Click += BtnRegister_Click;
                Grid.SetRow(btnRegister, 4);

                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.Children.Add(btnRegister);

                Content = grid;
            }

            private void BtnRegister_Click(object sender, RoutedEventArgs e)
            {
                EnteredValidationKey = txtValidation.Text.Trim();

                if (string.IsNullOrEmpty(EnteredValidationKey))
                {
                    MessageBox.Show(
                        "Please enter a Validation Key.",
                        "Validation Required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                DialogResult = true;   // ✅ WPF
                Close();
            }
        }

        //public class LicenseForm : Form
        //{
        //    public string EnteredValidationKey = "";

        //    private TextBox txtAppKey;
        //    private TextBox txtValidation;
        //    private Button btnRegister;

        //    public LicenseForm(string applicationKey)
        //    {
        //        Width = 500;
        //        Height = 250;
        //        Text = "Key Validation";
        //        FormBorderStyle = FormBorderStyle.FixedDialog;
        //        MaximizeBox = false;
        //        MinimizeBox = false;
        //        StartPosition = FormStartPosition.CenterScreen;

        //        // Application Key Label
        //        Label lblAppKey = new Label()
        //        {
        //            Text = "Application Key",
        //            Left = 20,
        //            Top = 30,
        //            Width = 120,
        //            Font = new Font("Segoe UI", 10)
        //        };

        //        txtAppKey = new TextBox()
        //        {
        //            Left = 150,
        //            Top = 25,
        //            Width = 300,
        //            ReadOnly = true,
        //            Text = applicationKey
        //        };

        //        // Validation Key Label
        //        Label lblValidation = new Label()
        //        {
        //            Text = "Validation Key",
        //            Left = 20,
        //            Top = 80,
        //            Width = 120,
        //            Font = new Font("Segoe UI", 10)
        //        };

        //        txtValidation = new TextBox()
        //        {
        //            Left = 150,
        //            Top = 75,
        //            Width = 300
        //        };

        //        // Register Button
        //        btnRegister = new Button()
        //        {
        //            Text = "Register",
        //            Left = 200,
        //            Top = 130,
        //            Width = 100,
        //            Enabled = true
        //        };
        //        btnRegister.Click += BtnRegister_Click;

        //        Controls.Add(lblAppKey);
        //        Controls.Add(txtAppKey);
        //        Controls.Add(lblValidation);
        //        Controls.Add(txtValidation);
        //        Controls.Add(btnRegister);
        //    }

        //    private void BtnRegister_Click(object sender, EventArgs e)
        //    {
        //        EnteredValidationKey = txtValidation.Text.Trim();

        //        if (string.IsNullOrEmpty(EnteredValidationKey))
        //        {
        //            MessageBox.Show("Please enter a Validation Key.", "Validation Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            return;
        //        }
        //        DialogResult = DialogResult.OK;
        //        Close();
        //    }
        //}
        public static void SaveKeyData()
        {
            KeyData data = new KeyData()
            {
                ApplicationKey = ApplicationKey ?? "",
                ValidationKey = ValidationKey ?? "",
                RegisterDate = DateTime.Now
            };

            Serialization.Serialize(DatFile, data);
        }
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = false,
                FileName = "Select Folder"
            };

            if (dialog.ShowDialog() == true)
            {
                Path.Text = System.IO.Path.GetDirectoryName(dialog.FileName);
            }
        }
        private const long DISK_LIMIT_GB = 25;
        private const string DRIVE_NAME = "C";

        private bool IsDiskSpaceBelowLimit(out long freeGB)
        {
            DriveInfo drive = new DriveInfo(DRIVE_NAME);
            freeGB = 0;

            if (!drive.IsReady)
                return true;

            freeGB = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
            return freeGB < DISK_LIMIT_GB;
        }

        private void SendLowDiskMail(long freeGB)
        {
            Outlook.Application outlook = null;
            Outlook.MailItem mail = null;

            try
            {
                outlook = new Outlook.Application();
                mail = (Outlook.MailItem)outlook.CreateItem(
                    Outlook.OlItemType.olMailItem);

                mail.Subject = "⚠ Low Disk Space – Process Stopped";

                mail.Body =
                    "Process was stopped automatically.\n\n" +
                    $"Free disk space on C drive is below {DISK_LIMIT_GB} GB.\n\n" +
                    $"Current free space: {freeGB} GB\n" +
                    $"Machine: {Environment.MachineName}\n" +
                    $"User: {Environment.UserName}\n\n" +
                    "Please clear disk space and try again.";

                mail.To =
                    "sandipc@wipropari.com; prasad.w@wipropari.com; harshal.g@wipropari.com";

                mail.CC =
                    "sainathhb@wipropari.com; priti.d.3@wipropari.com; sushant.p.1@wipropari.com";

                mail.Send();
            }
            finally
            {
                if (mail != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(mail);
                if (outlook != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(outlook);
            }
        }

        //private void Browse_Click(object sender, RoutedEventArgs e)
        //{
        //    var folderSearch = new FolderBrowserDialog();

        //    if (System.Windows.Forms.DialogResult.OK == folderSearch.ShowDialog())
        //    {
        //        Path.Text = folderSearch.SelectedPath;
        //    }
        //}
        string CSVName;
        string ExtraFileName;
        long sizeInKb;
        int checkInCount = 0;

        // ✅ ADD THIS HERE (outside MainWindow)
        internal static class NativeMethods
        {
            [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
            public static extern long StrFormatByteSize(
                long fileSize,
                StringBuilder buffer,
                int bufferSize
            );
        }
        private async void btnGenerateCSV_Click(object sender, RoutedEventArgs e)
        {
            // 🔴 DISK SPACE CHECK (FIRST LINE)
            if (IsDiskSpaceBelowLimit(out long freeGB))
            {
                MessageBox.Show(
                    $"C drive free space is low.\n\n" +
                    $"Available: {freeGB} GB\n" +
                    $"Required: {DISK_LIMIT_GB} GB\n\n" +
                    "Process will be stopped.",
                    "Low Disk Space",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

           SendLowDiskMail(freeGB);

                return; // 🔥 STOP HERE – NOTHING ELSE RUNS
            }
            try
            {
                checkInCount = 0;
                string URL = "";
                string selectedPath = "";
                string strSize = Size.Text;
                sizeInKb = (strSize.All(char.IsDigit) ? Convert.ToInt64(strSize) : 24) * 1000;
                Size.Text = strSize.All(char.IsDigit) ? strSize : "24";

                string tempRoot = @"C:\Temp\";
                if (!Directory.Exists(tempRoot))
                    Directory.CreateDirectory(tempRoot);

                if (!Directory.Exists(Path.Text))
                {
                    MessageBox.Show("Please check folder path", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                CSVName = System.IO.Path.Combine(
                    tempRoot,
                    "ProjectFolderList" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss") + ".csv");

                ExtraFileName = System.IO.Path.Combine(
                    tempRoot,
                    "Files" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss") + ".csv");

                // CSV Headers (MATCH YOUR SCREENSHOT) 
                File.WriteAllText(CSVName, "Folder List,Date,Time,Size,Comment,Old Backup Size\n");
                File.WriteAllText(ExtraFileName, "Folder Name,File Name\n");

                RunningFolder.Content = Path.Text;
                btnGenerateCSV.IsEnabled = false;

                List<Task> lstTsk = new List<Task>();
                string logRoot1 = System.IO.Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory,
    "CheckInLogs"
);
                string sessionLogFile = "";
                Directory.CreateDirectory(logRoot1);

                sessionLogFile = System.IO.Path.Combine(
                    logRoot1,
                    $"CheckInLog_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                );

                //foreach (var item in Directory.GetDirectories(Path.Text))
                //{
                //    var folder = item;

                //    lstTsk.Add(Task.Run(() =>
                //    {
                //        ScanDirectory(folder);
                //    }));
                //}
                foreach (var projectFolder in Directory.GetDirectories(Path.Text))
                {
                    //MessageBox.Show(projectFolder);
                    ScanDirectory(projectFolder);
                }

                await Task.WhenAll(lstTsk);
                //MessageBox.Show("CSV Created Sucessfully");
                btnGenerateCSV.IsEnabled = true;
                // =================================================
                // ============ ADD + COMBINE (FIXED) ==============
                // =================================================

                //var projectPaths = ReadFolderListFromCsv(CSVName);
                //int folderScanCount = projectPaths.Count();
                var projectPaths = ReadFolderListFromCsv(CSVName);

                int folderScanCount = projectPaths.Count;

                if (folderScanCount == 0)
                {
                    MessageBox.Show(
                        $"Generated CSV is blank or folder path not found.\n\nCSV Path:\n{CSVName}",
                        "No Folder Paths Found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );

                    return; // 🔥 STOP PROCESS COMPLETELY
                }


                if (Directory.Exists(downloadRoot))
                    Directory.Delete(downloadRoot, true);

                Directory.CreateDirectory(downloadRoot);

                //////////foreach (var srcPath in projectPaths)
                //////////{
                //////////    if (!Directory.Exists(srcPath))
                //////////        continue;

                //////////    string dest = System.IO.Path.Combine(
                //////////        downloadRoot,
                //////////        new DirectoryInfo(srcPath).Name + "_" +
                //////////        DateTime.Now.ToString("yyyyMMdd_HHmmss"));

                //////////    CopyDirectory(srcPath, dest);

                //////////    // 🔽 Extract ZIP files if present
                //////////    ExtractZipFilesInFolder(dest);
                //////////    break;
                //////////}

                // =================================================
                // ============ PROJECT COUNT SELECTION ============
                // =================================================

                // =================================================
                // ============ PROJECT COUNT FROM MAIN UI =========
                // =================================================

                // =================================================
                // ============ PROJECT COUNT FROM MAIN UI =========
                // =================================================

                string selectedValue = cmbProjectCount.Text?.Trim();

                bool isAllSelected = false;
                int projectsToCopy = 0;

                if (string.IsNullOrWhiteSpace(selectedValue))
                {
                    System.Windows.MessageBox.Show("Please select project count or ALL.");
                    return;
                }

                if (selectedValue.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                {
                    isAllSelected = true;
                    projectsToCopy = projectPaths.Count;
                }
                else if (int.TryParse(selectedValue, out int count) && count > 0)
                {
                    projectsToCopy = count;
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        "Please enter a valid number or ALL.",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                // =================================================
                // ============ COPY PROJECTS ======================
                // =================================================

                //int copied = 0;

                //foreach (var srcPath in projectPaths)
                //{
                //    if (!Directory.Exists(srcPath))
                //        continue;

                //    if (!popup.IsAllSelected && copied >= projectsToCopy)
                //        break;

                //    string dest = System.IO.Path.Combine(
                //        downloadRoot,
                //        new DirectoryInfo(srcPath).Name + "_" +
                //        DateTime.Now.ToString("yyyyMMdd_HHmmss"));

                //    CopyDirectory(srcPath, dest);
                //    ExtractZipFilesInFolder(dest);

                //    copied++;
                //}
                int copied = 0;
                List<string> paths = new List<string>();
                //foreach (var srcPath in projectPaths)
                //{
                //    //MessageBox.Show(srcPath);
                //    if (!Directory.Exists(srcPath))
                //        continue;

                //    if (!isAllSelected && copied >= projectsToCopy)
                //        break;

                //    string dest = System.IO.Path.Combine(
                //        downloadRoot,
                //        new DirectoryInfo(srcPath).Name);
                //        //+ "_" +
                //        //DateTime.Now.ToString("yyyyMMdd_HHmmss"));

                //    CopyDirectory(srcPath, dest);
                //    ExtractZipFilesInFolder(dest);
                //    paths.Add(srcPath);
                //    selectedPath = srcPath;
                //    // =============================
                //    // ONE DRIVE CLEANUP OPERATIONS
                //    // =============================
                //    //HandleOneDrivePostCopy(srcPath);

                //    copied++;
                //}
                string logRoot = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "CheckInLogs");

                Directory.CreateDirectory(logRoot);

                string filePath = System.IO.Path.Combine(
                    logRoot,
                    $"Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                );
                //File.WriteAllText(sessionLogFile, "Component Name,Component Path,Project,Octoplant Version,Date,Time\n");

                File.WriteAllText(sessionLogFile, "Date,Time,Octoplant Version,Component Path,Component Name\n");
                progressForm = new ProgressForm();
                AutomationRunner.ProgressUI = progressForm;
                progressForm.Show();
                foreach (var srcPath in projectPaths)
                {
                    FileInfo fileInfo = new FileInfo(srcPath);
                    string srcPath1 = fileInfo.DirectoryName;

                    if (!Directory.Exists(srcPath1))
                        continue;

                    if (!isAllSelected && copied >= projectsToCopy)
                        break;


                    string dest = System.IO.Path.Combine(
                        downloadRoot,
                        new DirectoryInfo(srcPath1).Name);

                    CopyDirectory(srcPath1, dest);
                    ExtractZipFilesInFolder(dest);
                    paths.Add(srcPath1);
                    selectedPath = srcPath1;

                    // ✅ WAIT here until background work finishes
                    await Task.Run(() =>
                    {
                        AutomationRunner.Run(URL, filePath, sessionLogFile);
                        DeleteExtractedFolder(downloadRoot);
                    });

                    // =============================
                    // Runs ONLY AFTER Task completes
                    // =============================
                    //if (File.Exists(CSVName))
                    //    File.Delete(CSVName);

                    //if (File.Exists(ExtraFileName))
                    //    File.Delete(ExtraFileName);

                    string verson = AutomationRunner.returnVersion();
                    MessageBox.Show(verson);
                    if (verson == "No Diff")
                    {
                        HandleOneDrivePostCopy(selectedPath);
                    }
                    else if (verson == "NA" || string.IsNullOrEmpty(verson))
                    {

                    }
                    else
                    {
                        checkInCount++;
                        HandleOneDrivePostCopy(selectedPath);
                    }
                    copied++;
                }
                //C:\Project\Folder_Scan_Updated\29-12-25\31-12-25\TodaysFolderScan - Copy\FolderScan\bin\Debug\CheckInLogs


                string latestLogFile = GetMostRecentFile(logRoot);
                UpdateLatestLogFile(latestLogFile, folderScanCount, checkInCount);
                updateTotalCount(logRoot, folderScanCount, checkInCount);
                try
                {

                    UpdateCombineFile(latestLogFile);
                }
                catch
                {

                }
                MessageBox.Show("Process Complete !");


  SendLogReportMail(latestLogFile, CSVName);
                // =================================================
                // 🔥 SHOW CHECK-IN PROGRESS POPUP
                // =================================================

                // 👉 SECOND EXE LOGIC (NOW SAME EXE)
                //AutomationRunner.Run();
                //         // 🔥 RUN AUTOMATION IN BACKGROUND(IMPORTANT)
                //Task.Run(() =>
                //{
                //    AutomationRunner.Run();
                //});
                //// Read CSV paths (already exists in your code)
                //List<string> csvProjectPaths = ReadFolderListFromCsv(CSVName);
                //Task.Run(() =>
                //{
                //    // 1️⃣ Run all automation (check-in etc.)
                //    AutomationRunner.Run();

                //    //// 2️⃣ Move ORIGINAL OneDrive folders (from CSV paths)
                //    //List<string> csvProjectPaths = ReadFolderListFromCsv(CSVName);
                //    //MoveCsvProjectsToOneDriveBackup(csvProjectPaths);

                //    // 3️⃣ DELETE Extracted folder (FINAL CLEANUP)
                //    DeleteExtractedFolder(downloadRoot);
                //});


                // --- your existing check-in logic here ---
                // project by project check-in
                // no changes
                // ----------------------------------------

                // 🔥 AFTER ALL CHECK-INS COMPLETED
                //MoveCsvProjectsToOneDriveBackup(csvProjectPaths);
                // =================================================
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
            }
        }
        string ExtractPipeline(string componentPath)
        {
            if (string.IsNullOrEmpty(componentPath))
                return null;

            // Take first folder
            string firstPart = componentPath.Split('\\')[0];

            // Example: P1Proj2023_24 → extract P1Proj
            // Logic: keep letters + first digit group until non-alphanumeric pattern changes
            if (firstPart.StartsWith("P", StringComparison.OrdinalIgnoreCase))
            {
                // Take letters and digits until second digit group ends
                // P1Proj2023_24 → P1Proj
                for (int i = 0; i < firstPart.Length; i++)
                {
                    if (char.IsDigit(firstPart[i]))
                    {
                        int j = i + 1;
                        while (j < firstPart.Length && char.IsDigit(firstPart[j]))
                            j++;

                        return firstPart.Substring(0, j); // P1Proj
                    }
                }
            }

            return null;
        }
        string siteUpdatePath = @"C:\CheckIn Report\AllComponent.xlsx";
        /* static string dataReceivedPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "CombineFile");*///@"C:\CheckIn Report\SiteUpdateCheckInLog.xlsx";
        string excelFilePath1 =/* System.IO.Path.Combine(dataReceivedPath, "SiteUpdateCheckInLog.xlsx");*/"C:\\wpdetailingcontrols onedrive\\OneDrive - Wipro PARI\\VD SiteUpdate\\SiteUpdateCheckInLog.xlsx";
        //string excelFilePath1 =/* System.IO.Path.Combine(dataReceivedPath, "SiteUpdateCheckInLog.xlsx");*/"C:\\CheckIn Report\\SiteUpdateCheckInLog1.xlsx";

        //void UpdateCombineFile(string latestLogFile)
        //{
        //    string combineExcelPath = excelFilePath1;

        //    if (!File.Exists(latestLogFile))
        //        throw new FileNotFoundException("CSV file not found.", latestLogFile);

        //    if (!File.Exists(combineExcelPath))
        //        throw new FileNotFoundException("Excel file not found.", combineExcelPath);

        //    ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        //    // Read CSV
        //    var csvLines = File.ReadAllLines(latestLogFile);
        //    if (csvLines.Length < 2)
        //        return; // no data

        //    string[] headers = csvLines[0].Split(',');

        //    int componentPathIndex = Array.FindIndex(headers, h => h.Trim().Equals("Component Path", StringComparison.OrdinalIgnoreCase));
        //    if (componentPathIndex == -1)
        //        throw new System.Exception("Component Path column not found in CSV.");

        //    // Parse CSV rows into grouped pipelines
        //    var pipelineGroups = new Dictionary<string, List<string[]>>();

        //    for (int i = 1; i < csvLines.Length; i++)
        //    {
        //        string[] row = csvLines[i].Split(',');

        //        if (row.Length <= componentPathIndex)
        //            continue;

        //        string componentPath = row[componentPathIndex].Trim();

        //        // Example: P1Proj2023_24\P23M5_FordTurkey\ST100\TIA Portal
        //        // Extract pipeline name: P1Proj, P2Proj, etc.
        //        string pipeline = ExtractPipeline(componentPath);

        //        if (string.IsNullOrEmpty(pipeline))
        //            continue;

        //        if (pipeline == "P1")
        //            pipeline = "P1Proj";


        //        if (pipeline == "P2")
        //            pipeline = "P2Proj";

        //        if (!pipelineGroups.ContainsKey(pipeline))
        //            pipelineGroups[pipeline] = new List<string[]>();

        //        pipelineGroups[pipeline].Add(row);
        //    }

        //    // Open Excel and append data to corresponding sheets
        //    using (var package = new ExcelPackage(new FileInfo(combineExcelPath)))
        //    {
        //        foreach (var group in pipelineGroups)
        //        {
        //            string pipelineName = group.Key;
        //            var rows = group.Value;

        //            var sheet = package.Workbook.Worksheets.FirstOrDefault(ws =>
        //                ws.Name.Equals(pipelineName, StringComparison.OrdinalIgnoreCase));

        //            if (sheet == null)
        //                continue; // Skip if sheet does not exist

        //            int startRow = sheet.Dimension?.End.Row + 1 ?? 2; // append after last row

        //            foreach (var rowData in rows)
        //            {
        //                for (int col = 0; col < 5; col++)
        //                {
        //                    sheet.Cells[startRow, col + 1].Value = rowData[col];
        //                }
        //                startRow++;
        //            }
        //        }

        //        package.Save();
        //    }
        //}
        void UpdateCombineFile(string latestLogFile)
        {
            string combineExcelPath = excelFilePath1;

            if (!File.Exists(latestLogFile))
                throw new FileNotFoundException("CSV file not found.", latestLogFile);

            if (!File.Exists(combineExcelPath))
                throw new FileNotFoundException("Excel file not found.", combineExcelPath);

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            // Read CSV
            var csvLines = File.ReadAllLines(latestLogFile);
            if (csvLines.Length < 2)
                return; // no data

            string[] headers = csvLines[0].Split(',');

            int componentPathIndex = Array.FindIndex(headers, h => h.Trim().Equals("Component Path", StringComparison.OrdinalIgnoreCase));
            if (componentPathIndex == -1)
                throw new System.Exception("Component Path column not found in CSV.");

            // Parse CSV rows into grouped pipelines
            var pipelineGroups = new Dictionary<string, List<string[]>>();

            for (int i = 1; i < csvLines.Length; i++)
            {
                string[] row = csvLines[i].Split(',');

                if (row.Length <= componentPathIndex)
                    continue;

                string componentPath = row[componentPathIndex].Trim();

                // Example: P1Proj2023_24\P23M5_FordTurkey\ST100\TIA Portal
                // Extract pipeline name: P1Proj, P2Proj, etc.
                string pipeline = ExtractPipeline(componentPath);

                if (string.IsNullOrEmpty(pipeline))
                    continue;

                if (pipeline == "P1")
                    pipeline = "P1Proj";


                if (pipeline == "P2")
                    pipeline = "P2Proj";

                if (!pipelineGroups.ContainsKey(pipeline))
                    pipelineGroups[pipeline] = new List<string[]>();

                pipelineGroups[pipeline].Add(row);
            }

            // Open Excel and append data to corresponding sheets
            using (var package = new ExcelPackage(new FileInfo(combineExcelPath)))
            {
                foreach (var group in pipelineGroups)
                {
                    string pipelineName = group.Key;
                    var rows = group.Value;

                    var sheet = package.Workbook.Worksheets.FirstOrDefault(ws =>
                        ws.Name.Equals(pipelineName, StringComparison.OrdinalIgnoreCase));

                    if (sheet == null)
                        continue; // Skip if sheet does not exist

                    int startRow = sheet.Dimension?.End.Row + 1 ?? 2; // append after last row

                    //foreach (var rowData in rows)
                    //{
                    //    for (int col = 0; col < 5; col++)
                    //    {
                    //        sheet.Cells[startRow, col + 1].Value = rowData[col];
                    //    }
                    //    startRow++;
                    //}
                    foreach (var rowData in rows)
                    {
                        for (int col = 0; col < 5; col++)
                        {
                            // DATE COLUMN (Assuming column 0 = Date)
                            if (col == 0)
                            {
                                string rawDate = rowData[col].Trim();

                                // Remove =" and "
                                if (rawDate.StartsWith("=\"") && rawDate.EndsWith("\""))
                                {
                                    rawDate = rawDate.Substring(2, rawDate.Length - 3);
                                }

                                if (DateTime.TryParseExact(
                                    rawDate,
                                    "dd-MMM-yyyy",
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    System.Globalization.DateTimeStyles.None,
                                    out DateTime parsedDate))
                                {
                                    sheet.Cells[startRow, col + 1].Value = parsedDate;
                                    sheet.Cells[startRow, col + 1].Style.Numberformat.Format = "dd-MMM-yyyy";
                                }
                                else
                                {
                                    sheet.Cells[startRow, col + 1].Value = rawDate;
                                }
                            }
                            else
                            {
                                sheet.Cells[startRow, col + 1].Value = rowData[col];
                            }
                        }
                        startRow++;
                    }

                }

                package.Save();
            }
        }

        void updateTotalCount(string logRoot, int valueF, int valueG)
        {
            try
            {
                DateTime compareTime = DateTime.ParseExact(
                    generation,
                    "yyyyMMddHHmmss",
                    CultureInfo.InvariantCulture
                );

                var resultFiles = Directory.GetFiles(logRoot, "*.csv")
                    .Where(f => File.GetCreationTime(f) > compareTime)
                    .ToList();

                int totalFolderScan = 0;
                int totalCheckIn = 0;

                if (resultFiles.Count > 0)
                {
                    foreach (var resultFile in resultFiles)
                    {
                        var (folderCount, checkInCount) = ReadCsvCounts(resultFile);

                        if (folderCount.HasValue)
                            totalFolderScan += folderCount.Value;

                        if (checkInCount.HasValue)
                            totalCheckIn += checkInCount.Value;
                    }
                }
                AppendDailyTotalsToLatestCsv(logRoot, totalFolderScan, totalCheckIn);
                // 👉 Update latest log file using calculated totals
                //UpdateLatestLogFile(logRoot, totalFolderScan, totalCheckIn);
            }
            catch (Exception ex)
            {
                Console.WriteLine("updateTotalCount Error: " + ex.Message);
            }
        }
        void AppendDailyTotalsToLatestCsv(string logRoot, int totalFolderScan, int totalCheckIn)
        {
            try
            {
                // Get the most recent CSV file
                string latestLogFile = Directory.GetFiles(logRoot, "*.csv")
                    .OrderByDescending(File.GetCreationTime)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(latestLogFile) || !File.Exists(latestLogFile))
                    return;

                var lines = File.ReadAllLines(latestLogFile).ToList();

                if (lines.Count == 0)
                    return;

                // --- Handle Header ---
                string header = lines[0];

                if (!header.Contains("Total Folder Scan Count For Day") &&
                    !header.Contains("Total Checkin Count For Day"))
                {
                    header += ",Total Folder Scan Count For Day,Total Checkin Count For Day";
                    lines[0] = header;
                }

                // --- Handle Data Rows ---
                for (int i = 1; i < lines.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                        continue;

                    lines[i] += "," + totalFolderScan + "," + totalCheckIn;
                    break;
                }

                File.WriteAllLines(latestLogFile, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine("AppendDailyTotalsToLatestCsv Error: " + ex.Message);
            }
        }
        void UpdateLatestLogFile(string logRoot, int valueF, int valueG)
        {
            //string line = $"{DateTime.Now:yyyy-MM-dd},{DateTime.Now:HH:mm:ss},{hVersion},{project},{name}\n";
            string latestLogFile = logRoot;// GetMostRecentFile(logRoot);

            if (string.IsNullOrEmpty(latestLogFile) || !File.Exists(latestLogFile))
                return;

            var lines = File.ReadAllLines(latestLogFile).ToList();

            if (lines.Count >= 2)
            {
                int count = lines[1].Split(',').Count();
                string firstline = lines[0];
                firstline += ",," + "Folder Scan Count," + "Check In Count";
                string secondLine = lines[1];
                if (count == 6)
                {
                    secondLine += "," + valueF + "," + valueG;
                }
                else
                {
                    secondLine += ",," + valueF + "," + valueG;
                }
                lines[0] = firstline;
                lines[1] = secondLine;
            }
            else if (lines.Count == 1)
            {
                string firstline = lines[0];
                firstline += ",," + "Folder Scan Count," + "Check In Count";
                string secondLine = "";
                secondLine += ",,,,,,," + valueF + "," + valueG;
                lines[0] = firstline;
                lines.Add(secondLine);
            }
            else if (lines.Count == 0)
            {
                string firstline = "";
                firstline += "Folder Scan Count," + "Check In Count";
                string secondLine = "";
                secondLine += valueF + "," + valueG;
                lines.Add(firstline);
                lines.Add(secondLine);
            }

            File.WriteAllLines(latestLogFile, lines);

        }
        static (int? FolderScanCount, int? CheckInCount) ReadCsvCounts(string filePath)
        {
            var lines = File.ReadAllLines(filePath);

            if (lines.Length < 2)
                return (null, null);

            var headers = lines[0].Split(',');

            int folderScanIndex = Array.FindIndex(headers,
                h => h.Trim().Equals("Folder Scan Count", StringComparison.OrdinalIgnoreCase));

            int checkInIndex = Array.FindIndex(headers,
                h => h.Trim().Equals("Check In Count", StringComparison.OrdinalIgnoreCase));

            if (folderScanIndex == -1 || checkInIndex == -1)
                return (null, null);

            var values = lines[1].Split(',');

            int? folderCount = null;
            int? checkInCount = null;

            if (values.Length > Math.Max(folderScanIndex, checkInIndex))
            {
                if (int.TryParse(values[folderScanIndex].Trim(), out int fCount))
                    folderCount = fCount;

                if (int.TryParse(values[checkInIndex].Trim(), out int cCount))
                    checkInCount = cCount;
            }

            return (folderCount, checkInCount);
        }

        string GetMostRecentFile(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return null;

            var directoryInfo = new DirectoryInfo(folderPath);

            FileInfo latestFile = directoryInfo
                .GetFiles("*.csv")              // filter if needed
                .OrderByDescending(f => f.CreationTime)
                .FirstOrDefault();

            return latestFile?.FullName;
        }
        static void SendLogReportMail(string csvPath, string folderScanFile)
        {
            try
            {
                Outlook.Application outlook = new Outlook.Application();
                Outlook.MailItem mail =
                    (Outlook.MailItem)outlook.CreateItem(Outlook.OlItemType.olMailItem);

                mail.Subject = "Octoplant Check-In Report";
                mail.Body =
                    "Dear Team,\n\nAttached lists details of components checked-in today, in Octoplant from respective folder of cloud service (OneDrive/Dropbox).\n\nThanks & regards,\nVD Team";

                //string recipientsCsv = @"C:\MailSend\Recipients.csv";
                string recipientsCsv = System.IO.Path.Combine(
                  AppDomain.CurrentDomain.BaseDirectory,
                  "MailSend",
                  "Recipients.csv"
              );
                //string toList = ReadEmailIdsFromCsv(recipientsCsv);
                string toList = "sandipc@wipropari.com; prasad.w@wipropari.com; harshal.g@wipropari.com";//ReadEmailIdsFromCsv(recipientsCsv, "Email");
                string ccList = "sainathhb@wipropari.com; priti.d.3@wipropari.com; sushant.p.1@wipropari.com";//ReadEmailIdsFromCsv(recipientsCsv, "CC");

                if (!string.IsNullOrWhiteSpace(toList))
                    mail.To = toList;
                if (!string.IsNullOrEmpty(ccList))
                    mail.CC = ccList;

                // ✔ already correct
                mail.Attachments.Add(csvPath);
                mail.Attachments.Add(folderScanFile);
                mail.DeleteAfterSubmit = false;
                mail.Send();
            }
            //catch (Exception ex)
            //{
            //    File.AppendAllText(@"C:\MailSend\MailError.log", ex.ToString());
            //}
            catch (Exception ex)
            {
                string mailSendDir = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "MailSend");

                Directory.CreateDirectory(mailSendDir);

                File.AppendAllText(
                    System.IO.Path.Combine(mailSendDir, "MailError.log"),
                    DateTime.Now + " : " + ex + Environment.NewLine
                );
            }

        }

        //static string ReadEmailIdsFromCsv(string csvPath)
        //{
        //    if (!File.Exists(csvPath))
        //        return string.Empty;

        //    var emails = File.ReadAllLines(csvPath)
        //        .Skip(1) // ✅ Skip header row
        //        .Select(line => line.Trim().Trim('"')) // remove quotes if any
        //        .Select(line => line.Replace("\uFEFF", "")) // ✅ remove BOM
        //        .Where(line =>
        //            !string.IsNullOrWhiteSpace(line) &&
        //            line.Contains("@") &&
        //            line.Contains("."))
        //        .Distinct()
        //        .ToList();

        //    return string.Join(";", emails); // Outlook requires ;
        //}

        static string ReadEmailIdsFromCsv(string csvPath, string columnName)
        {
            if (!File.Exists(csvPath))
                return string.Empty;

            var lines = File.ReadAllLines(csvPath);

            if (lines.Length < 2)
                return string.Empty;

            // Get header columns
            var headers = lines[0]
                .Replace("\uFEFF", "")
                .Split(',')
                .Select(h => h.Trim())
                .ToList();

            int colIndex = headers.FindIndex(h =>
                h.Equals(columnName, StringComparison.OrdinalIgnoreCase));

            if (colIndex == -1)
                return string.Empty;

            var emails = lines
                .Skip(1)
                .Select(line => line.Split(','))
                .Where(cols => cols.Length > colIndex)
                .Select(cols => cols[colIndex].Trim().Trim('"'))
                .Where(email =>
                    !string.IsNullOrWhiteSpace(email) &&
                    email.Contains("@") &&
                    email.Contains("."))
                .Distinct()
                .ToList();

            return string.Join(";", emails);
        }

        static string findURL(string Path)
        {
            //Path = "C:\\wpdetailingcontrols onedrive\\OneDrive - Wipro PARI\\VD SiteUpdate\\P2_Projects\\P2Proj2021_22\\PRT84 Stellantis\\TBU Line\\Zone 1\\OP1205_Z1_Conv";
            string oneDriveRoot = "C:\\wpdetailingcontrols onedrive\\OneDrive - Wipro PARI\\";//"C:\\Users\\Wpuser\\OneDrive - Wipro PARI";
            //Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
            //@"\OneDrive - Wipro PARI\";

            if (!Path.StartsWith(oneDriveRoot, StringComparison.OrdinalIgnoreCase))
                return Path;

            string extracted = Path.Substring(oneDriveRoot.Length)
                            .TrimStart(System.IO.Path.DirectorySeparatorChar);

            string result = EncodeOneDrivePath(extracted);
            string dummy = @"https://parirobotics-my.sharepoint.com/my?id=%2Fpersonal%2Fsushant%5Fp%5F1%5Fwipropari%5Fcom%2FDocuments";
            string dummy1 = @"https://parirobotics-my.sharepoint.com/personal/wpdetailingcontrols_wipropari_com/_layouts/15/onedrive.aspx?id=%2Fpersonal%2Fwpdetailingcontrols_wipropari_com%2FDocuments";
            string dummy2 = @"https://parirobotics-my.sharepoint.com/my?id=%2Fpersonal%2Fwpdetailingcontrols%5Fwipropari%5Fcom%2FDocuments";
            result = dummy2 + result;
            return result;
        }

        static string EncodeOneDrivePath(string input)
        {
            // 1. Convert Windows separators to URL separators
            string path = input.Replace("\\", "/");

            // 2. Standard URL encode
            string encoded = Uri.EscapeDataString(path);

            // 3. Force SharePoint-style encoding
            encoded = encoded.Replace("_", "%5F");

            // 4. SharePoint expects leading %2F
            return "%2F" + encoded;
        }


        //// ----------------------------------------
        //// 2️⃣ Move ProjectData.zip
        //// ----------------------------------------
        //string zipPath = System.IO.Path.Combine(projectPath, "ProjectData.zip");

        //if (File.Exists(zipPath))
        //{
        //    string targetZip = System.IO.Path.Combine(oldProjectFolder, "ProjectData.zip");

        //    // Safety: avoid overwrite
        //    if (File.Exists(targetZip))
        //    {
        //        targetZip = System.IO.Path.Combine(
        //            oldProjectFolder,
        //            $"ProjectData_{DateTime.Now:HHmmss}.zip");
        //    }

        //    File.Copy(zipPath, targetZip);
        //}
        static DateTime ExtractDateFromFileName(string fileName)
        {
            // Supports:
            // ProjectData_2026_02_23
            // ProjectData_2026-02-23
            // ProjectData_2026-02-23_1
            // ProjectData_2026_02_23_1

            var match = System.Text.RegularExpressions.Regex.Match(
                fileName,
                @"ProjectData_(\d{4})[-_](\d{2})[-_](\d{2})"
            );

            if (match.Success)
            {
                string dateStr = $"{match.Groups[1].Value}-{match.Groups[2].Value}-{match.Groups[3].Value}";

                if (DateTime.TryParse(dateStr, out DateTime dt))
                    return dt;
            }

            return DateTime.MinValue;
        }
        static void CleanupOldBackups(string folderPath, int keepLatest = 2)
        {
            if (!Directory.Exists(folderPath))
                return;

            var files = Directory.GetFiles(folderPath, "ProjectData_*.zip")
                .Select(f => new
                {
                    Path = f,
                    Date = ExtractDateFromFileName(System.IO.Path.GetFileNameWithoutExtension(f))
                })
                .Where(x => x.Date != DateTime.MinValue)
                .OrderByDescending(x => x.Date)
                .ToList();

            foreach (var file in files.Skip(keepLatest))
            {
                try
                {
                    File.Delete(file.Path);
                }
                catch { }
            }
        }


        static void HandleOneDrivePostCopy(string projectPath)
        {
            try
            {
                // ----------------------------------------
                // 1️⃣ Create OldProject folder
                // ----------------------------------------
                string oldProjectFolder = System.IO.Path.Combine(projectPath, "Old Backup");
                Directory.CreateDirectory(oldProjectFolder);
                // ----------------------------------------
                // 2️⃣ Copy ProjectData.zip
                // ----------------------------------------
                string zipPath = System.IO.Path.Combine(projectPath, "ProjectData.zip");

                if (File.Exists(zipPath))
                {
                    string dateSuffix = DateTime.Now.ToString("yyyy-MM-dd");

                    string targetZip = System.IO.Path.Combine(
                        oldProjectFolder,
                        $"ProjectData_{dateSuffix}.zip");

                    // Safety: if same date file already exists, add counter (no time)
                    int counter = 1;
                    while (File.Exists(targetZip))
                    {
                        targetZip = System.IO.Path.Combine(
                            oldProjectFolder,
                            $"ProjectData_{dateSuffix}_{counter}.zip");
                        counter++;
                    }

                    File.Copy(zipPath, targetZip);

                    // Keep newest 1 + older 1 = total 2
                    CleanupOldBackups(oldProjectFolder, 2);

                    // ----------------------------------------
                    // 🔹 CLEAR DATA INSIDE ProjectData.zip
                    // ----------------------------------------
                    using (var fs = new FileStream(zipPath, FileMode.Create))
                    using (var archive = new System.IO.Compression.ZipArchive(
                        fs,
                        System.IO.Compression.ZipArchiveMode.Create))
                    {
                        // Creating archive with no entries = empty zip
                    }
                }

                // ----------------------------------------
                // 3️⃣ Clear Supplier files (DO NOT DELETE)
                // ----------------------------------------
                string reasonFile = System.IO.Path.Combine(projectPath, "SupplierChangeReason.txt");
                string commentFile = System.IO.Path.Combine(projectPath, "SupplierComment.txt");

                if (File.Exists(reasonFile))
                    File.WriteAllText(reasonFile, "Site Updated Backup");

                if (File.Exists(commentFile))
                    File.WriteAllText(commentFile, "dd__Dec_2025, Updated by: Name of person");

                //if (File.Exists(reasonFile))
                //    File.WriteAllText(reasonFile, string.Empty);

                //if (File.Exists(commentFile))
                //    File.WriteAllText(commentFile, string.Empty);

                Console.WriteLine("✔ OneDrive cleanup completed for: " + projectPath);
            }
            catch (Exception ex)
            {
                File.AppendAllText(
                    @"C:\Temp\OneDrivePostCopyError.log",
                    $"[{DateTime.Now}] {projectPath} : {ex}{Environment.NewLine}");
            }
        }

        static void DeleteExtractedFolder(string extractRoot)
        {
            try
            {
                if (Directory.Exists(extractRoot))
                {
                    Directory.Delete(extractRoot, true);
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(
                    @"C:\Temp\ExtractedCleanupError.log",
                    ex + Environment.NewLine);
            }
        }
        void ExtractZipFilesInFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return;

            var zipFiles = Directory.GetFiles(folderPath, "*.zip", SearchOption.TopDirectoryOnly);

            foreach (var zip in zipFiles)
            {
                try
                {
                    // ✅ Only ProjectData.zip
                    if (!string.Equals(
                            System.IO.Path.GetFileName(zip),
                            "ProjectData.zip",
                            StringComparison.OrdinalIgnoreCase))
                        continue;

                    string parentDir = System.IO.Path.GetDirectoryName(zip);
                    string extractDir = System.IO.Path.Combine(
                        parentDir,
                        System.IO.Path.GetFileNameWithoutExtension(zip)
                    );

                    // ============================
                    // 1️⃣ Extract ProjectData.zip
                    // ============================
                    if (!Directory.Exists(extractDir))
                    {
                        Directory.CreateDirectory(extractDir);

                        using (var archive = ZipFile.OpenRead(zip))
                        {
                            foreach (var entry in archive.Entries)
                            {
                                string destPath = System.IO.Path.Combine(extractDir, entry.FullName);

                                // Directory
                                if (string.IsNullOrEmpty(entry.Name))
                                {
                                    Directory.CreateDirectory(destPath);
                                    continue;
                                }

                                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destPath));
                                entry.ExtractToFile(destPath, overwrite: true);
                            }
                        }
                    }

                    // ============================
                    // 2️⃣ Extract INNER zip files
                    // ============================
                    var innerZips = Directory.GetFiles(extractDir, "*.zip", SearchOption.TopDirectoryOnly);

                    foreach (var innerZip in innerZips)
                    {
                        try
                        {
                            using (var archive = ZipFile.OpenRead(innerZip))
                            {
                                foreach (var entry in archive.Entries)
                                {
                                    string destPath = System.IO.Path.Combine(extractDir, entry.FullName);

                                    if (string.IsNullOrEmpty(entry.Name))
                                    {
                                        Directory.CreateDirectory(destPath);
                                        continue;
                                    }

                                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destPath));
                                    entry.ExtractToFile(destPath, overwrite: true);
                                }
                            }

                            // ❌ Delete inner zip after clean extraction
                            File.Delete(innerZip);
                        }
                        catch (Exception exInner)
                        {
                            Console.WriteLine($"Inner zip error {innerZip}: {exInner.Message}");
                        }
                    }

                    // ❌ Delete ProjectData.zip
                    File.Delete(zip);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error extracting {zip}: {ex.Message}");
                }
            }

            // 🔁 Recursively process subfolders
            foreach (var dir in Directory.GetDirectories(folderPath))
            {
                ExtractZipFilesInFolder(dir);
            }
        }
        //void ExtractZipFilesInFolder(string folderPath)
        //{
        //    if (!Directory.Exists(folderPath))
        //        return;

        //    // Get zip files only from current folder
        //    var zipFiles = Directory.GetFiles(folderPath, "*.zip", SearchOption.TopDirectoryOnly);

        //    foreach (var zip in zipFiles)
        //    {
        //        try
        //        {
        //            string fileName = System.IO.Path.GetFileName(zip);
        //            string parentDir = System.IO.Path.GetDirectoryName(zip);

        //            // 🔹 CASE 1: Extract only ProjectData.zip anywhere
        //            if (string.Equals(fileName, "ProjectData.zip", StringComparison.OrdinalIgnoreCase))
        //            {
        //                string extractDir = System.IO.Path.Combine(parentDir, "ProjectData");

        //                if (!Directory.Exists(extractDir))
        //                {
        //                    Directory.CreateDirectory(extractDir);
        //                    ZipFile.ExtractToDirectory(zip, extractDir);
        //                }

        //                File.Delete(zip);
        //            }
        //            // 🔹 CASE 2: If we are INSIDE ProjectData folder, extract ALL zips
        //            else if (System.IO.Path.GetFileName(parentDir)
        //                         .Equals("ProjectData", StringComparison.OrdinalIgnoreCase))
        //            {
        //                string extractDir = System.IO.Path.Combine(
        //                    parentDir,
        //                    System.IO.Path.GetFileNameWithoutExtension(zip)
        //                );

        //                if (!Directory.Exists(extractDir))
        //                {
        //                    Directory.CreateDirectory(extractDir);
        //                    ZipFile.ExtractToDirectory(zip, extractDir);
        //                }

        //                File.Delete(zip);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Error extracting {zip}: {ex.Message}");
        //        }
        //    }

        //    // 🔁 Recursively process subfolders
        //    foreach (var dir in Directory.GetDirectories(folderPath))
        //    {
        //        ExtractZipFilesInFolder(dir);
        //    }
        //}
        //void ExtractZipFilesInFolder(string folderPath)
        //{
        //    if (!Directory.Exists(folderPath))
        //        return;

        //    // Get zip files only from current folder
        //    var zipFiles = Directory.GetFiles(folderPath, "*.zip", SearchOption.TopDirectoryOnly);

        //    foreach (var zip in zipFiles)
        //    {
        //        try
        //        {
        //            string parentDir = System.IO.Path.GetDirectoryName(zip);

        //            // Extract folder name = zip file name
        //            string extractDir = System.IO.Path.Combine(
        //                parentDir,
        //                System.IO.Path.GetFileNameWithoutExtension(zip)
        //            );

        //            // Extract only if not already extracted
        //            if (!Directory.Exists(extractDir))
        //            {
        //                Directory.CreateDirectory(extractDir);
        //                ZipFile.ExtractToDirectory(zip, extractDir);
        //            }

        //            // ✅ Delete zip AFTER successful extraction
        //            File.Delete(zip);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Error extracting {zip}: {ex.Message}");
        //        }
        //    }

        //    // Recursively process subfolders (including extracted ones)
        //    foreach (var dir in Directory.GetDirectories(folderPath))
        //    {
        //        ExtractZipFilesInFolder(dir);
        //    }
        //}
        //void ExtractZipFilesInFolder(string folderPath)
        //{
        //    var zipFiles = Directory.GetFiles(folderPath, "*.zip", SearchOption.AllDirectories);

        //    foreach (var zip in zipFiles)
        //    {
        //        try
        //        {
        //            string parentDir = System.IO.Path.GetDirectoryName(zip);

        //            // Folder name = ZIP file name (without .zip)
        //            string extractDir = System.IO.Path.Combine(parentDir, System.IO.Path.GetFileNameWithoutExtension(zip));

        //            // Create extraction folder
        //            if (!Directory.Exists(extractDir))
        //                Directory.CreateDirectory(extractDir);

        //            // Extract inside its own folder
        //            ZipFile.ExtractToDirectory(zip, extractDir);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }
        //    }
        //}
        void ScanDirectory1(string folderPath)
        {
            lock (this)
            {
                File.AppendAllText(
                    CSVName,
                    $"{folderPath},{DateTime.Now:dd/MM/yyyy},{DateTime.Now:HH:mm:ss}\n");
            }

        }
        private bool EnsureLicenseAtStartup()
        {
            ReadData();

            if (string.IsNullOrEmpty(ApplicationKey))
            {
                string deviceName = Environment.MachineName + ".PC";
                GetApplicationKey(deviceName);

                SaveKeyData();

                //MessageBox.Show(
                //    "Application Key generated.\n\nPlease send this key to the administrator to get a Validation Key:\n\n"
                //    + ApplicationKey,
                //    "Application Key Generated",
                //    MessageBoxButton.OK,
                //    MessageBoxImage.Information
                //);

                return false; // stop app until validation key is entered
            }

            // 3️⃣ Validate Validation Key (ADMIN GENERATED)
            if (string.IsNullOrEmpty(ValidationKey) || !VerifyKey())
            {
                LicenseForm lf = new LicenseForm(ApplicationKey);

                //if (lf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                bool? result = lf.ShowDialog();

                if (result == true)

                {
                    ValidationKey = lf.EnteredValidationKey;

                    if (!VerifyKey())
                    {
                        MessageBox.Show(
                            "Invalid or expired Validation Key.",
                            "License Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                        return false;
                    }

                    SaveKeyData();
                    return true;
                }
                else
                {
                    return false; // user cancelled
                }
            }

            // ✅ License OK
            return true;
        }

        void CopyDirectory(string source, string target)
        {
            Directory.CreateDirectory(target);

            foreach (var f in Directory.GetFiles(source))
            {
                File.Copy(f, System.IO.Path.Combine(target, System.IO.Path.GetFileName(f)), true);
            }
            foreach (var d in Directory.GetDirectories(source))
            {
                CopyDirectory(d, System.IO.Path.Combine(target, System.IO.Path.GetFileName(d)));
                //MessageBox.Show("Project Added in Download Folder");
            }
            string URL = findURL(source);
            //MessageBox.Show(URL);

            string urlFilePath = System.IO.Path.Combine(target, "URL.txt");
            if (File.Exists(urlFilePath))
                File.Delete(urlFilePath);

            if (!File.Exists(urlFilePath))
            {
                File.WriteAllText(urlFilePath, URL);
            }

        }
        //private string GetInnerZipComment(string projectFolder)
        //{
        //    string projectZipPath = System.IO.Path.Combine(projectFolder, "ProjectData.zip");

        //    if (!File.Exists(projectZipPath))
        //        return string.Empty;

        //    try
        //    {
        //        using (ZipArchive archive = ZipFile.OpenRead(projectZipPath))
        //        {
        //            var innerZip = archive.Entries
        //                .FirstOrDefault(e => e.FullName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

        //            if (innerZip != null)
        //            {
        //                return $"Contains {innerZip.Name} file in ProjectData.zip";
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        // Ignore corrupt zip / access issues
        //    }

        //    return string.Empty;
        //}
        private string GetInnerZipComment(string projectFolder)
        {
            //string projectZipPath = System.IO.Path.Combine(projectFolder, "ProjectData.zip");

            //if (!File.Exists(projectZipPath))
            //    return string.Empty;

            //try
            //{
            //    using (ZipArchive archive = ZipFile.OpenRead(projectZipPath))
            //    {
            //        var innerZip = archive.Entries
            //            .FirstOrDefault(e => e.FullName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

            //        if (innerZip != null)
            //        {
            //            //return $"Contains {innerZip.Name} file in ProjectData.zip";
            //            return $"Contains .zip file in ProjectData.zip";
            //        }
            //    }
            //}
            //catch
            //{
            //    // Ignore corrupt zip / access issues
            //}

            //return string.Empty;
            string projectZipPath = System.IO.Path.Combine(projectFolder, "ProjectData.zip");

            if (!File.Exists(projectZipPath))
                return string.Empty;

            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(projectZipPath))
                {
                    // ✅ Find root-level ZIP file
                    var rootZipEntry = archive.Entries.FirstOrDefault(e =>
                        e.FullName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) &&
                        !e.FullName.Contains("/") &&
                        !e.FullName.Contains("\\")
                    );

                    if (rootZipEntry != null)
                    {
                        // ✅ Exact filename as-is
                        return $"Contains {rootZipEntry.Name} file in ProjectData.zip";
                    }
                }
            }
            catch
            {
                // Ignore corrupt zip / access issues
            }

            return string.Empty;

        }
        // =========================================================
        // ADD METHODS (NO REMOVAL)
        // =========================================================
        //List<string> ReadFolderListFromCsv(string csvPath)
        //{
        //    return File.ReadAllLines(csvPath)
        //        .Skip(1)
        //        .Where(l => !string.IsNullOrWhiteSpace(l))
        //        .Select(l => l.Split(',')[0].Trim('"'))
        //        .Distinct()
        //        .ToList();
        //}
        List<string> ReadFolderListFromCsv(string csvPath)
        {
            if (!File.Exists(csvPath))
                return new List<string>();

            return File.ReadAllLines(csvPath)
                .Skip(1) // skip header
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Split(',')[0].Trim('"'))
                //.Where(p => Directory.Exists(p)) // ✅ OPTIONAL: only valid folders
                .Distinct()
                .ToList();
        }

        //void CopyDirectory(string source, string target)
        //{
        //    Directory.CreateDirectory(target);

        //    foreach (var f in Directory.GetFiles(source))
        //        File.Copy(f, System.IO.Path.Combine(target, System.IO.Path.GetFileName(f)), true);

        //    foreach (var d in Directory.GetDirectories(source))
        //        CopyDirectory(d, System.IO.Path.Combine(target, System.IO.Path.GetFileName(d)));
        //    MessageBox.Show("Project Added in Download Folder");
        //}
        string strSize = "24";

        List<string> filesNames = new List<string>()
{
    "ProjectData.zip",
    "Readme.txt",
    "SupplierChangeReason.txt",
    "SupplierComment.txt",
    "SupplierDeliveryNote.vdog-sdn"
};
        private static string GetReadableFileSize(long bytes)
        {
            const long KB = 1024;
            const long MB = KB * 1024;
            const long GB = MB * 1024;

            if (bytes >= GB)
                return $"{bytes / (double)GB:0.##} GB";
            if (bytes >= MB)
                return $"{bytes / (double)MB:0.##} MB";
            if (bytes >= KB)
                return $"{bytes / (double)KB:0.##} KB";

            return $"{bytes} Bytes";
        }
        private static string GetExplorerSizeFromBytes(long bytes)
        {
            long kb = (long)Math.Round(bytes / 1024.0, MidpointRounding.AwayFromZero);
            return $"{kb:N0} KB";

        }
        private static string FormatSize(long bytes)
        {
            double size = bytes;

            if (size >= 1024 * 1024 * 1024) // GB
                return $"{size / (1024 * 1024 * 1024):N2} GB";

            if (size >= 1024 * 1024) // MB
                return $"{size / (1024 * 1024):N2} MB";

            if (size >= 1024) // KB
                return $"{size / 1024:N0} KB";

            return $"{size:N0} Bytes";
        }
        private static long GetDirectorySizeBytesSafe(string folderPath)
        {
            long size = 0;

            try
            {
                foreach (var file in Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories))
                {
                    try { size += new FileInfo(file).Length; }
                    catch { /* skip unreadable file */ }
                }
            }
            catch
            {
                // skip unreadable folder / access denied
            }

            return size;
        }

        // Finds "Old Backup" folder by walking up from a start folder (in case ProjectData.zip is inside subfolders)
        private static string GetOldBackupSize(string projectFolderPath)
        {
            string oldBackupPath = System.IO.Path.Combine(projectFolderPath, "Old Backup");

            if (!Directory.Exists(oldBackupPath))
                return "0 KB";

            long totalBytes = 0;

            var stack = new Stack<string>();
            stack.Push(oldBackupPath);

            while (stack.Count > 0)
            {
                string currentDir = stack.Pop();

                // ✅ files
                try
                {
                    foreach (var file in Directory.EnumerateFiles(currentDir))
                    {
                        try
                        {
                            var fi = new FileInfo(file);

                            // Optional: if OneDrive file is "online-only", Length may be 0
                            // You can skip it or force download (not recommended).
                            totalBytes += fi.Length;
                        }
                        catch
                        {
                            // skip this file only
                        }
                    }
                }
                catch
                {
                    // skip this folder files
                }

                // ✅ subfolders
                try
                {
                    foreach (var dir in Directory.EnumerateDirectories(currentDir))
                    {
                        stack.Push(dir);
                    }
                }
                catch
                {
                    // skip subfolder listing
                }
            }

            //long kb = (long)Math.Round(totalBytes / 1024.0, MidpointRounding.AwayFromZero);
            //return $"{kb:N0} KB";
            return FormatSize(totalBytes);
        }
        public void ScanDirectory(string directoryInfo)
        {
            try
            {
                lock (directoryInfo)
                {
                    foreach (var file in Directory.GetFiles(directoryInfo))
                    {
                        FileInfo fileInfo = new FileInfo(file);

                        if (fileInfo.Length >= sizeInKb && fileInfo.Name.Contains("ProjectData.zip"))
                        {
                            lock (CSVName)
                            {
                                //////double fileSizeKB = Math.Round(fileInfo.Length / 1024.0, 2);

                                //////// ✅ PASS DIRECTORY, NOT FILE NAME
                                //////string comment = GetInnerZipComment(fileInfo.DirectoryName);

                                //////string line =
                                //////    $"\"{fileInfo.FullName}\"," +
                                //////    $"{fileInfo.LastWriteTime:yyyy-MM-dd}," +
                                //////    $"{fileInfo.LastWriteTime:HH:mm:ss}," +
                                //////    $"{fileSizeKB}," +
                                //////    $"{comment}" +
                                //////    Environment.NewLine;string explorerSize = GetExplorerKBExact(fileInfo.Length);

                                ///foreach (var file in Directory.GetFiles(directoryInfo))


                                // ✅ EXACT Windows Explorer size (KB / MB / GB as-is)
                                string explorerSize = GetExplorerKBExact(fileInfo.Length);

                                // ✅ NEW: Old Backup folder size (near the ProjectData.zip location)
                                // ✅ Get Old Backup size
                                string oldBackupSize = GetOldBackupSize(fileInfo.DirectoryName);
                                string comment = GetInnerZipComment(fileInfo.DirectoryName);

                                string line =
                                    $"\"{fileInfo.FullName}\"," +
                                    $"{fileInfo.LastWriteTime:yyyy-MM-dd}," +
                                    $"{fileInfo.LastWriteTime:HH:mm:ss}," +
                                    $"\"{explorerSize}\"," +
                                    $"{comment}," +
                                     $"\"{oldBackupSize}\"" +
                                    Environment.NewLine;





                                //string line = $"\"{fileInfo.FullName}\",{fileInfo.LastWriteTime:yyyy-MM-dd},{fileInfo.LastWriteTime:HH:mm:ss}{Environment.NewLine}";
                                //File.AppendAllText(CSVName, "\"" + fileInfo.DirectoryName + "\"," + fileInfo.CreationTime.Date+ "," + fileInfo.CreationTime.TimeOfDay + Environment.NewLine);
                                File.AppendAllText(CSVName, line);
                                //File.AppendAllText(CSVName, "\""+fileInfo.DirectoryName+"\"" + "\n");                                            
                                //return;                                                                                                        
                            }

                        }
                        //if (fileInfo.Length >= sizeInKb && fileInfo.Name.Equals("ProjectData.zip", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    lock (CSVName)
                        //    {
                        //        double fileSizeKB = Math.Round(fileInfo.Length / 1024.0, 2);

                        //        // ✅ PASS DIRECTORY, NOT FILE NAME
                        //        string comment = GetInnerZipComment(fileInfo.DirectoryName);

                        //        string line =
                        //            $"\"{fileInfo.FullName}\"," +
                        //            $"{fileInfo.LastWriteTime:yyyy-MM-dd}," +
                        //            $"{fileInfo.LastWriteTime:HH:mm:ss}," +
                        //            $"{fileSizeKB}," +
                        //            $"{comment}" +
                        //            Environment.NewLine;

                        //        // ✅ WRITE ONLY ONCE
                        //        File.AppendAllText(CSVName, line);

                        //    }
                        //}
                        if (filesNames.FindIndex(f => f.ToLower() == fileInfo.Name.ToLower()) < 0)
                        {
                            lock (ExtraFileName)
                            {
                                File.AppendAllText(ExtraFileName, "\"" + fileInfo.DirectoryName + "\"" + "," + "\"" + fileInfo.Name + "\"" + "\n");
                                //return;                                                                                                        
                            }
                        }
                        if (fileInfo.Name.Contains("ProjectData.zip"))
                        {
                            foreach (var dfile in Directory.GetDirectories(directoryInfo))
                            {
                                DirectoryInfo Master = new DirectoryInfo(directoryInfo);
                                DirectoryInfo dfileInfo = new DirectoryInfo(dfile);
                                lock (ExtraFileName)
                                {
                                    File.AppendAllText(ExtraFileName, "\"" + Master.FullName + "\"" + "," + "\"" + dfileInfo.Name + "\"" + "\n");
                                    //return;                                                                                                    
                                }
                            }
                        }
                    }
                    foreach (var item in Directory.GetDirectories(directoryInfo))
                    {
                        lock (RunningFolder)
                        {
                            Action invokeAction = new Action(() => { RunningFolder.Content = item; });
                            RunningFolder.Dispatcher.Invoke(invokeAction);
                        }
                        ScanDirectory(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private static string GetExplorerKBExact(long bytes)
        {
            long kb = (long)Math.Round(bytes / 1024.0, MidpointRounding.AwayFromZero);
            return $"{kb:N0} KB";
        }

        //recent working
        //public void ScanDirectory(string directoryPath)
        //{
        //    try
        //    {
        //        // ==========================================
        //        // RULE: ProjectData.zip MUST exist HERE ONLY
        //        // ==========================================
        //        string projectZip = System.IO.Path.Combine(directoryPath, "ProjectData.zip");

        //        if (File.Exists(projectZip))
        //        {
        //            FileInfo zipInfo = new FileInfo(projectZip);

        //            if (zipInfo.Length >= sizeInKb)
        //            {
        //                // ==============================
        //                // VALID PROJECT → ADD CSV
        //                // ==============================
        //                lock (CSVName)
        //                {
        //                    string line =
        //                        $"\"{zipInfo}\"," +
        //                        $"{zipInfo.Directory.CreationTime:yyyy-MM-dd}," +
        //                        $"{zipInfo.Directory.CreationTime:HH:mm:ss}{Environment.NewLine}";

        //                    File.AppendAllText(CSVName, line);
        //                }

        //                // ==============================
        //                // EXTRA FILES (Top level only)
        //                // ==============================
        //                foreach (var file in Directory.GetFiles(directoryPath, "*", SearchOption.TopDirectoryOnly))
        //                {
        //                    FileInfo fileInfo = new FileInfo(file);

        //                    if (!filesNames.Any(f =>
        //                            f.Equals(fileInfo.Name, StringComparison.OrdinalIgnoreCase)))
        //                    {
        //                        lock (ExtraFileName)
        //                        {
        //                            File.AppendAllText(
        //                                ExtraFileName,
        //                                $"\"{directoryPath}\",\"{fileInfo.Name}\"{Environment.NewLine}");
        //                        }
        //                    }
        //                }

        //                // ==============================
        //                // EXTRA SUBFOLDERS (Names only)
        //                // ==============================
        //                foreach (var subDir in Directory.GetDirectories(directoryPath))
        //                {
        //                    DirectoryInfo dInfo = new DirectoryInfo(subDir);

        //                    lock (ExtraFileName)
        //                    {
        //                        File.AppendAllText(
        //                            ExtraFileName,
        //                            $"\"{directoryPath}\",\"{dInfo.Name}\"{Environment.NewLine}");
        //                    }
        //                }
        //            }
        //        }

        //        // ==========================================
        //        // 🔁 RECURSION → Traverse child folders
        //        // ==========================================
        //        foreach (var subDir in Directory.GetDirectories(directoryPath))
        //        {
        //            ScanDirectory(subDir);
        //        }
        //    }
        //    catch (UnauthorizedAccessException)
        //    {
        //        // Skip folders without access
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}
        //Recent Working
        //public void ScanDirectory(string directoryInfo)
        //{
        //    try
        //    {
        //        // ==========================================
        //        // RULE: ProjectData.zip MUST exist HERE ONLY
        //        // ==========================================
        //        string projectZip = System.IO.Path.Combine(directoryInfo, "ProjectData.zip");

        //        if (!File.Exists(projectZip))
        //        {
        //            // ❌ Not a valid project → skip this folder
        //            return;
        //        }

        //        FileInfo zipInfo = new FileInfo(projectZip);

        //        if (zipInfo.Length < sizeInKb)
        //        {
        //            return;
        //        }

        //        // ==========================================
        //        // VALID PROJECT → ADD CSV
        //        // ==========================================
        //        lock (CSVName)
        //        {
        //            string line =
        //                $"\"{directoryInfo}\"," +
        //                $"{zipInfo.Directory.CreationTime:yyyy-MM-dd}," +
        //                $"{zipInfo.Directory.CreationTime:HH:mm:ss}{Environment.NewLine}";

        //            File.AppendAllText(CSVName, line);
        //        }

        //        // ==========================================
        //        // EXTRA FILES
        //        // ==========================================
        //        foreach (var file in Directory.GetFiles(directoryInfo, "*", SearchOption.TopDirectoryOnly))
        //        {
        //            FileInfo fileInfo = new FileInfo(file);

        //            if (!filesNames.Any(f =>
        //                    f.Equals(fileInfo.Name, StringComparison.OrdinalIgnoreCase)))
        //            {
        //                lock (ExtraFileName)
        //                {
        //                    File.AppendAllText(
        //                        ExtraFileName,
        //                        $"\"{directoryInfo}\",\"{fileInfo.Name}\"{Environment.NewLine}");
        //                }
        //            }
        //        }

        //        // ==========================================
        //        // EXTRA SUBFOLDERS (LOG NAMES ONLY)
        //        // ==========================================
        //        foreach (var subDir in Directory.GetDirectories(directoryInfo))
        //        {
        //            DirectoryInfo dInfo = new DirectoryInfo(subDir);

        //            lock (ExtraFileName)
        //            {
        //                File.AppendAllText(
        //                    ExtraFileName,
        //                    $"\"{directoryInfo}\",\"{dInfo.Name}\"{Environment.NewLine}");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}

        ////////public void ScanDirectory(string directoryInfo)
        ////////{
        ////////    try
        ////////    {
        ////////        lock (directoryInfo)
        ////////        {
        ////////            bool csvAdded = false; // ✅ ensure CSV added only once

        ////////            // ===============================
        ////////            // SCAN FILES IN CURRENT DIRECTORY
        ////////            // ===============================
        ////////            foreach (var file in Directory.GetFiles(directoryInfo, "*", SearchOption.TopDirectoryOnly))
        ////////            {
        ////////                FileInfo fileInfo = new FileInfo(file);

        ////////                // -------------------------------------------------
        ////////                // CSV ENTRY (ONLY IF ProjectData.zip IN SAME FOLDER)
        ////////                // -------------------------------------------------
        ////////                if (!csvAdded &&
        ////////                    fileInfo.Name.Equals("ProjectData.zip", StringComparison.OrdinalIgnoreCase) &&
        ////////                    fileInfo.Length >= sizeInKb)
        ////////                {
        ////////                    lock (CSVName)
        ////////                    {
        ////////                        string line =
        ////////                            $"\"{directoryInfo}\"," +
        ////////                            $"{fileInfo.Directory.CreationTime:yyyy-MM-dd}," +
        ////////                            $"{fileInfo.Directory.CreationTime:HH:mm:ss}{Environment.NewLine}";

        ////////                        File.AppendAllText(CSVName, line);
        ////////                    }

        ////////                    csvAdded = true;
        ////////                }

        ////////                // -----------------------------------------------
        ////////                // EXTRA FILES (FILES NOT IN ALLOWED LIST)
        ////////                // -----------------------------------------------
        ////////                if (!filesNames.Any(f =>
        ////////                        f.Equals(fileInfo.Name, StringComparison.OrdinalIgnoreCase)))
        ////////                {
        ////////                    lock (ExtraFileName)
        ////////                    {
        ////////                        File.AppendAllText(
        ////////                            ExtraFileName,
        ////////                            $"\"{directoryInfo}\",\"{fileInfo.Name}\"{Environment.NewLine}");
        ////////                    }
        ////////                }
        ////////            }

        ////////            // -------------------------------------------------
        ////////            // EXTRA FILES: SUBFOLDERS WHEN ProjectData.zip EXISTS
        ////////            // -------------------------------------------------
        ////////            if (File.Exists(System.IO.Path.Combine(directoryInfo, "ProjectData.zip")))
        ////////            {
        ////////                foreach (var subDir in Directory.GetDirectories(directoryInfo))
        ////////                {
        ////////                    DirectoryInfo dInfo = new DirectoryInfo(subDir);

        ////////                    lock (ExtraFileName)
        ////////                    {
        ////////                        File.AppendAllText(
        ////////                            ExtraFileName,
        ////////                            $"\"{directoryInfo}\",\"{dInfo.Name}\"{Environment.NewLine}");
        ////////                    }
        ////////                }
        ////////            }

        ////////            // ===============================
        ////////            // RECURSIVE DIRECTORY SCAN
        ////////            // ===============================
        ////////            foreach (var item in Directory.GetDirectories(directoryInfo))
        ////////            {
        ////////                lock (RunningFolder)
        ////////                {
        ////////                    RunningFolder.Dispatcher.Invoke(() =>
        ////////                    {
        ////////                        RunningFolder.Content = item;
        ////////                    });
        ////////                }

        ////////                ScanDirectory(item);
        ////////            }
        ////////        }
        ////////    }
        ////////    catch (Exception ex)
        ////////    {
        ////////        MessageBox.Show(ex.Message);
        ////////    }
        ////////}


        //////string strSize = "24";
        //////List<string> filesNames = new List<string>() { "ProjectData.zip", "Readme.txt", "SupplierChangeReason.txt", "SupplierComment.txt", "SupplierDeliveryNote.vdog-sdn" };
        //////public void ScanDirectory(string directoryInfo)
        //////{
        //////    try
        //////    {
        //////        lock (directoryInfo)
        //////        {
        //////            foreach (var file in Directory.GetFiles(directoryInfo))
        //////            {
        //////                FileInfo fileInfo = new FileInfo(file);
        //////                if (fileInfo.Length >= sizeInKb && fileInfo.Name.Contains("ProjectData.zip"))
        //////                {
        //////                    lock (CSVName)
        //////                    {
        //////                        string line = $"\"{fileInfo.DirectoryName}\",{fileInfo.Directory.CreationTime:yyyy-MM-dd},{fileInfo.Directory.CreationTime:HH:mm:ss}{Environment.NewLine}";
        //////                        //File.AppendAllText(CSVName, "\"" + fileInfo.DirectoryName + "\"," + fileInfo.CreationTime.Date+ "," + fileInfo.CreationTime.TimeOfDay + Environment.NewLine);
        //////                        File.AppendAllText(CSVName, line);
        //////                        //File.AppendAllText(CSVName, "\""+fileInfo.DirectoryName+"\"" + "\n");                                            
        //////                        //return;                                                                                                        
        //////                    }

        //////                }
        //////                if (filesNames.FindIndex(f => f.ToLower() == fileInfo.Name.ToLower()) < 0)
        //////                {
        //////                    lock (ExtraFileName)
        //////                    {
        //////                        File.AppendAllText(ExtraFileName, "\"" + fileInfo.DirectoryName + "\"" + "," + "\"" + fileInfo.Name + "\"" + "\n");
        //////                        //return;                                                                                                        
        //////                    }
        //////                }
        //////                if (fileInfo.Name.Contains("ProjectData.zip"))
        //////                {
        //////                    foreach (var dfile in Directory.GetDirectories(directoryInfo))
        //////                    {
        //////                        DirectoryInfo Master = new DirectoryInfo(directoryInfo);
        //////                        DirectoryInfo dfileInfo = new DirectoryInfo(dfile);
        //////                        lock (ExtraFileName)
        //////                        {
        //////                            File.AppendAllText(ExtraFileName, "\"" + Master.FullName + "\"" + "," + "\"" + dfileInfo.Name + "\"" + "\n");
        //////                            //return;                                                                                                    
        //////                        }
        //////                    }
        //////                }
        //////            }
        //////            foreach (var item in Directory.GetDirectories(directoryInfo))
        //////            {
        //////                lock (RunningFolder)
        //////                {
        //////                    Action invokeAction = new Action(() => { RunningFolder.Content = item; });
        //////                    RunningFolder.Dispatcher.Invoke(invokeAction);
        //////                }
        //////                ScanDirectory(item);
        //////            }
        //////        }
        //////    }
        //////    catch (Exception ex)
        //////    {
        //////        MessageBox.Show(ex.Message);
        //////    }

        //////}
        private void updateBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = false,
                FileName = "Select Folder"
            };

            if (dialog.ShowDialog() == true)
            {
                string folderPath = System.IO.Path.GetDirectoryName(dialog.FileName);

                // UpdatePath.Text = folderPath;   // uncomment when UpdatePath exists
            }
        }
        string cmbProjectData, cmbReadme, cmbSupplierC, cmbSupplierCR, cmbVersionSdn;

        private void cmbProjectCount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //private async void btnUpdateFolder_Click(object sender, RoutedEventArgs e)                               
        //{                                                                                                        
        //    strSize = Size.Text;                                                                                 
        //    sizeInKb = (strSize.All(char.IsDigit) ? Convert.ToInt64(strSize) : 24) * 1000;                       
        //    Size.Text = strSize.All(char.IsDigit) ? strSize : "24";                                              
        //    if (!Directory.Exists(UpdatePath.Text))                                                              
        //    {                                                                                                    
        //        MessageBox.Show("Please check folder path", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;                                                                                          
        //    }                                                                                                    
        //    UpdateRunningFolder.Content = UpdatePath.Text;                                                       
        //    btnUpdateFolder.IsEnabled = false;                                                                   
        //    cmbProjectData = cmbUpdateProjectData.Text;                                                          
        //    cmbReadme = cmbUpdateReadme.Text;                                                                    
        //    cmbSupplierC = cmbUpdateSupplierComment.Text;                                                        
        //    cmbSupplierCR = cmbUpdateSupplierChangeReason.Text;                                                  
        //    cmbVersionSdn = cmbUpdateVersionSupplierDeliveryNote.Text;                                           
        //    List<Task> lstTsk = new List<Task>();                                                                
        //    foreach (var item in Directory.GetDirectories(UpdatePath.Text))                                      
        //    {                                                                                                    
        //        UpdateRunningFolder.Content = item;                                                              
        //        var tsk = Task.Run(async () =>                                                                   
        //        {                                                                                                
        //            ScanDirectoryUpdateFolder(item);                                                             
        //        });                                                                                              
        //        //await tsk;                                                                                     
        //        lstTsk.Add(tsk);                                                                                 
        //    }                                                                                                    
        //    foreach (var item in lstTsk)                                                                         
        //    {                                                                                                    
        //        await item;                                                                                      
        //    }                                                                                                    
        //    MessageBox.Show("Folder Updated Sucessfully");                                                       
        //    btnUpdateFolder.IsEnabled = true;                                                                    
        //}                                                                                                        
        // ============================================================
        // FINAL BACKUP TO ONEDRIVE (AFTER ALL CHECKINS)
        // ============================================================
        // ============================================================
        // MOVE ORIGINAL ONEDRIVE PROJECTS (FROM CSV PATHS)
        // ============================================================
        static void MoveCsvProjectsToOneDriveBackup(List<string> csvProjectPaths)
        {
            try
            {
                string backupRoot =
                    @"C:\Users\P01090879\OneDrive - Wipro PARI (1)\BackUpAllCheckin";

                Directory.CreateDirectory(backupRoot);

                string doneFolder =
                    System.IO.Path.Combine(
                        backupRoot,
                        $"CheckIn Done_{DateTime.Now:yyyyMMdd_HHmmss}");

                Directory.CreateDirectory(doneFolder);

                foreach (var projectPath in csvProjectPaths)
                {
                    if (!Directory.Exists(projectPath))
                        continue;

                    string targetPath =
                        System.IO.Path.Combine(doneFolder, System.IO.Path.GetFileName(projectPath));

                    // Safety: avoid overwrite
                    if (Directory.Exists(targetPath))
                    {
                        targetPath += "_" +
                            DateTime.Now.ToString("HHmmss");
                    }

                    Directory.Move(projectPath, targetPath);
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(
                    @"C:\Temp\OneDriveMoveError.log",
                    ex + Environment.NewLine);
            }
        }


        public void ScanDirectoryUpdateFolder(string directoryInfo)
        {
            try
            {
                lock (directoryInfo)
                {
                    foreach (var file in Directory.GetFiles(directoryInfo))
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        if (fileInfo.Length >= sizeInKb && fileInfo.Name.Contains("ProjectData.zip"))
                        {
                            lock (fileInfo)
                            {
                                foreach (var file2 in Directory.GetFiles(directoryInfo))
                                {
                                    FileInfo fileInfo2 = new FileInfo(file2);
                                    if (fileInfo2.Name.ToLower().Contains(filesNames[0].ToLower()))
                                    {
                                        /////ProjectData.Zip                                                                                                
                                        if (cmbProjectData == "Yes")
                                        {
                                            using (ZipArchive archive = ZipFile.Open(fileInfo2.FullName, ZipArchiveMode.Update))
                                            {
                                            GetEntries:
                                                var entries = archive.Entries;
                                                foreach (ZipArchiveEntry item in entries)
                                                {
                                                    item.Delete();
                                                    goto GetEntries;
                                                }
                                            }

                                        }
                                    }
                                    else if (fileInfo2.Name.ToLower().Contains(filesNames[1].ToLower()))
                                    {
                                        ////ReadMe.txt                                                                                                      
                                        if (cmbReadme == "Yes")
                                        {
                                            var data = File.ReadAllText("Data.txt");
                                            if (fileInfo2.Name == "readme.txt")
                                            {
                                                File.Delete(fileInfo2.FullName);
                                                File.WriteAllText(System.IO.Path.Combine(fileInfo2.DirectoryName, "Readme.txt"), data);
                                            }
                                            else
                                            {
                                                File.WriteAllText(fileInfo2.DirectoryName, data);
                                            }
                                        }
                                    }
                                    else if (fileInfo2.Name.ToLower().Contains(filesNames[2].ToLower()))
                                    {
                                        /////SupplierChangeReason.txt                                                                                       
                                        if (cmbSupplierCR == "Empty")
                                        {
                                            File.WriteAllText(fileInfo2.FullName, "");
                                        }
                                    }
                                    else if (fileInfo2.Name.ToLower().Contains(filesNames[3].ToLower()))
                                    {
                                        /////SupplierCOmment.txt                                                                                            
                                        if (cmbSupplierC == "Empty")
                                        {
                                            File.WriteAllText(fileInfo2.FullName, "dd_" + DateTime.Now.ToString("_MMM_yyyy") + ", Updated by: Name of person");
                                        }
                                    }
                                    else if (fileInfo2.Name.ToLower().Contains(filesNames[4].ToLower()))
                                    {
                                        /////SupplierDeliveryNote.vdog-sdn                                                                                  
                                        if (cmbVersionSdn == "Yes")
                                        {
                                            var data = File.ReadAllText(fileInfo2.FullName);
                                            Console.WriteLine(data);
                                            var FinIndex = data.IndexOf("Number");
                                            var str = data.Substring(FinIndex + 7, 2);
                                            var arr = str.Where(a => char.IsDigit(a));
                                            str = string.Join("", arr);
                                            var data2 = data.Replace(">" + str + "<", ">0<");
                                            Console.WriteLine(data2);
                                            File.WriteAllText(fileInfo2.FullName, data2);
                                        }
                                    }

                                }
                            }
                        }

                    }
                    foreach (var item in Directory.GetDirectories(directoryInfo))
                    {
                        //lock (UpdateRunningFolder)                                                                                                          
                        //{                                                                                                                                   
                        //    Action invokeAction = new Action(() => { UpdateRunningFolder.Content = item; });                                                
                        //    UpdateRunningFolder.Dispatcher.Invoke(invokeAction);                                                                            
                        //}                                                                                                                                   
                        ScanDirectoryUpdateFolder(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}
//private async void btnGenerateCSV_Click(object sender, RoutedEventArgs e)
//{
//    try
//    {
//        strSize = Size.Text;
//        sizeInKb = (strSize.All(char.IsDigit) ? Convert.ToInt64(strSize) : 24) * 1000;
//        Size.Text = strSize.All(char.IsDigit) ? strSize : "24";
//        CSVName = @"C:\Temp\";
//        if (!Directory.Exists(CSVName))
//        {
//            Directory.CreateDirectory(CSVName);
//        }

//        if (!Directory.Exists(Path.Text))
//        {
//            MessageBox.Show("Please check folder path", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            return;
//        }
//        CSVName = System.IO.Path.Combine(CSVName, "ProjectFolderList" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss") + ".csv");
//        ExtraFileName = System.IO.Path.Combine(@"C:\Temp\", "Files" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss") + ".csv");

//        File.AppendAllText(CSVName, "Folder List" + "," + "Date" + "," + "Time" + "\n");
//        File.AppendAllText(ExtraFileName, "Folder Name,File Name" + "\n");
//        RunningFolder.Content = Path.Text;
//        btnGenerateCSV.IsEnabled = false;
//        List<Task> lstTsk = new List<Task>();
//        foreach (var item in Directory.GetDirectories(Path.Text))
//        {
//            RunningFolder.Content = item;
//            var tsk = Task.Run(async () =>
//            {
//                ScanDirectory(item);
//            });
//            //await tsk;                                                                                                        
//            lstTsk.Add(tsk);
//        }
//        foreach (var item in lstTsk)
//        {
//            await item;
//        }
//        MessageBox.Show("CSV Created Sucessfully");
//        btnGenerateCSV.IsEnabled = true;
//        // =================================================
//        // ============ ADD + COMBINE (FIXED) ==============
//        // =================================================

//        var projectPaths = ReadFolderListFromCsv(CSVName);

//        if (Directory.Exists(downloadRoot))
//            Directory.Delete(downloadRoot, true);

//        Directory.CreateDirectory(downloadRoot);

//        foreach (var srcPath in projectPaths)
//        {
//            if (!Directory.Exists(srcPath))
//                continue;

//            string dest = System.IO.Path.Combine(
//                downloadRoot,
//                new DirectoryInfo(srcPath).Name + "_" +
//                DateTime.Now.ToString("yyyyMMdd_HHmmss"));

//            CopyDirectory(srcPath, dest);



//        }


//// =========================================================
//// ADD METHODS (NO REMOVAL)
//// =========================================================
//List<string> ReadFolderListFromCsv1(string csvPath)
//{
//    return File.ReadAllLines(csvPath)
//        .Skip(1)
//        .Where(l => !string.IsNullOrWhiteSpace(l))
//        .Select(l => l.Split(',')[0].Trim('"'))
//        .Distinct()
//        .ToList();
//}
//List<(string Path, DateTime Time)> ReadFolderListFromCsv(string csvPath)
//{
//    return File.ReadAllLines(csvPath)
//        .Skip(1) // skip header
//        .Where(l => !string.IsNullOrWhiteSpace(l))
//        .Select(l =>
//        {
//            var cols = l.Split(',');

//            string folderPath = cols[0].Trim();
//            string datePart = cols[1].Trim();
//            string timePart = cols[2].Trim();

//            DateTime dateTimeFromCsv =
//                DateTime.ParseExact(
//                    $"{datePart} {timePart}",
//                    "dd/MM/yyyy HH:mm:ss",
//                    System.Globalization.CultureInfo.InvariantCulture);

//            return (folderPath, dateTimeFromCsv);
//        })
//        .ToList();
//}



//string strSize = "24";
//List<string> filesNames = new List<string>() { "ProjectData.zip", "Readme.txt", "SupplierChangeReason.txt", "SupplierComment.txt", "SupplierDeliveryNote.vdog-sdn" };
//public void ScanDirectory(string directoryInfo)
//{
//    try
//    {
//        lock (directoryInfo)
//        {
//            foreach (var file in Directory.GetFiles(directoryInfo))
//            {
//                FileInfo fileInfo = new FileInfo(file);
//                if (fileInfo.Length >= sizeInKb && fileInfo.Name.Contains("ProjectData.zip"))
//                {
//                    lock (CSVName)
//                    {
//                        string line = $"\"{fileInfo.DirectoryName}\",{fileInfo.Directory.CreationTime:yyyy-MM-dd},{fileInfo.Directory.CreationTime:HH:mm:ss}{Environment.NewLine}";
//                        //File.AppendAllText(CSVName, "\"" + fileInfo.DirectoryName + "\"," + fileInfo.CreationTime.Date+ "," + fileInfo.CreationTime.TimeOfDay + Environment.NewLine);
//                        File.AppendAllText(CSVName, line);
//                        //File.AppendAllText(CSVName, "\""+fileInfo.DirectoryName+"\"" + "\n");                                            
//                        //return;                                                                                                        
//                    }

//                }
//                if (filesNames.FindIndex(f => f.ToLower() == fileInfo.Name.ToLower()) < 0)
//                {
//                    lock (ExtraFileName)
//                    {
//                        File.AppendAllText(ExtraFileName, "\"" + fileInfo.DirectoryName + "\"" + "," + "\"" + fileInfo.Name + "\"" + "\n");
//                        //return;                                                                                                        
//                    }
//                }
//                if (fileInfo.Name.Contains("ProjectData.zip"))
//                {
//                    foreach (var dfile in Directory.GetDirectories(directoryInfo))
//                    {
//                        DirectoryInfo Master = new DirectoryInfo(directoryInfo);
//                        DirectoryInfo dfileInfo = new DirectoryInfo(dfile);
//                        lock (ExtraFileName)
//                        {
//                            File.AppendAllText(ExtraFileName, "\"" + Master.FullName + "\"" + "," + "\"" + dfileInfo.Name + "\"" + "\n");
//                            //return;                                                                                                    
//                        }
//                    }
//                }
//            }
//            foreach (var item in Directory.GetDirectories(directoryInfo))
//            {
//                lock (RunningFolder)
//                {
//                    Action invokeAction = new Action(() => { RunningFolder.Content = item; });
//                    RunningFolder.Dispatcher.Invoke(invokeAction);
//                }
//                ScanDirectory(item);
//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        MessageBox.Show(ex.Message);
//    }

//}
//        string strSize = "24";
//        List<string> filesNames = new List<string>()
//{
//    "ProjectData.zip",
//    "Readme.txt",
//    "SupplierChangeReason.txt",
//    "SupplierComment.txt",
//    "SupplierDeliveryNote.vdog-sdn"
//};

//        public void ScanDirectory(string directoryInfo)
//        {
//            try
//            {
//                // ================================
//                // ADDITION: CSV entry per folder
//                // ================================
//                lock (CSVName)
//                {
//                    File.AppendAllText(
//                        CSVName,
//                        $"{directoryInfo},{DateTime.Now:dd/MM/yyyy},{DateTime.Now:HH:mm:ss}{Environment.NewLine}");
//                }

//                lock (directoryInfo)
//                {
//                    foreach (var file in Directory.GetFiles(directoryInfo))
//                    {
//                        FileInfo fileInfo = new FileInfo(file);

//                        // ================================
//                        // Existing ZIP size logic (UNCHANGED)
//                        // ================================
//                        if (fileInfo.Length >= sizeInKb && fileInfo.Name.Contains("ProjectData.zip"))
//                        {
//                            lock (CSVName)
//                            {
//                                string line =
//                                    $"\"{fileInfo.DirectoryName}\"," +
//                                    $"{fileInfo.Directory.CreationTime:dd/MM/yyyy}," +
//                                    $"{fileInfo.Directory.CreationTime:HH:mm:ss}" +
//                                    Environment.NewLine;

//                                File.AppendAllText(CSVName, line);
//                            }
//                        }

//                        // ================================
//                        // Extra files logging (UNCHANGED)
//                        // ================================
//                        if (filesNames.FindIndex(f => f.Equals(fileInfo.Name, StringComparison.OrdinalIgnoreCase)) < 0)
//                        {
//                            lock (ExtraFileName)
//                            {
//                                File.AppendAllText(
//                                    ExtraFileName,
//                                    $"\"{fileInfo.DirectoryName}\",\"{fileInfo.Name}\"{Environment.NewLine}");
//                            }
//                        }

//                        // ================================
//                        // Sub-folder logging for ZIP (UNCHANGED)
//                        // ================================
//                        if (fileInfo.Name.Contains("ProjectData.zip"))
//                        {
//                            foreach (var dfile in Directory.GetDirectories(directoryInfo))
//                            {
//                                DirectoryInfo master = new DirectoryInfo(directoryInfo);
//                                DirectoryInfo dfileInfo = new DirectoryInfo(dfile);

//                                lock (ExtraFileName)
//                                {
//                                    File.AppendAllText(
//                                        ExtraFileName,
//                                        $"\"{master.FullName}\",\"{dfileInfo.Name}\"{Environment.NewLine}");
//                                }
//                            }
//                        }
//                    }

//                    // ================================
//                    // Recursive scan (UNCHANGED)
//                    // ================================
//                    foreach (var item in Directory.GetDirectories(directoryInfo))
//                    {
//                        lock (RunningFolder)
//                        {
//                            RunningFolder.Dispatcher.Invoke(() =>
//                            {
//                                RunningFolder.Content = item;
//                            });
//                        }

//                        ScanDirectory(item);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(ex.Message);
//            }
//        }

//Disclaimer: 'The information contained in this electronic message and any attachments to this message are intended for the exclusive use of the addressee(s) and may contain proprietary, confidential or privileged information. If you are not the intended recipient, you should not disseminate, distribute or copy this e-mail. Please notify the sender immediately and destroy all copies of this message and any attachments. WARNING: Computer viruses can be transmitted via email. The recipient should check this email and any attachments for the presence of viruses. The company accepts no liability for any damage caused by any virus transmitted by this email.'
