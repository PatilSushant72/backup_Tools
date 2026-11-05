
// ============================================================
// YOUR EXISTING LOGIC — NOT REMOVED
// ============================================================
//static void RunAutomation()
//{
//    Directory.CreateDirectory(logRoot);
//    sessionLogFile = Path.Combine(logRoot, $"CheckInLog_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
//    File.WriteAllText(sessionLogFile, "Component,Project,DateTime\n");
//    foreach (var containerFolder in Directory.GetDirectories(extractRoot))
//    {
//        var level1 = Directory.GetDirectories(containerFolder);
//        string projectContainer = level1.Length == 1 ? level1[0] : containerFolder;

//        var projectFolders = Directory
//            .GetDirectories(projectContainer)
//            .Select(f => new { Path = f, Time = GetProjectTimestamp(f) })
//            .OrderBy(x => x.Time)
//            .Select(x => x.Path)
//            .ToArray();

//        ProcessProjectContainer(containerFolder, projectFolders);
//    }
//    SendLogReportMail(sessionLogFile);
//}
//static void RunAutomation()
//{
//    Directory.CreateDirectory(logRoot);

//    sessionLogFile = System.IO.Path.Combine(
//        logRoot,
//        $"CheckInLog_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
//    );

//    File.WriteAllText(sessionLogFile, "Component,Project,DateTime\n");

//    foreach (var containerFolder in Directory.GetDirectories(extractRoot))
//    {
//        Console.WriteLine($"\n📦 Processing container: {containerFolder}");

//        var level1Dirs = Directory.GetDirectories(containerFolder);

//        string projectContainer =
//            level1Dirs.Length == 1 ? level1Dirs[0] : containerFolder;

//        // --------------------------------------------------
//        // 🔑 KEY FIX: DO NOT FILTER – TAKE ALL DIRECTORIES
//        // --------------------------------------------------
//        var projectFolders = Directory.GetDirectories(projectContainer);

//        // --------------------------------------------------
//        // 🔑 IF NO SUB-FOLDERS → CONTAINER ITSELF IS PROJECT
//        // --------------------------------------------------
//        if (projectFolders.Length == 0)
//        {
//            Console.WriteLine("ℹ No project subfolders found. Using container as project.");
//            projectFolders = new[] { projectContainer };
//        }

//        // OPTIONAL: order only if timestamp exists
//        projectFolders = projectFolders
//            .Select(p => new { Path = p, Time = GetProjectTimestamp(p) })
//            .OrderBy(x => x.Time)
//            .Select(x => x.Path)
//            .ToArray();

//        Console.WriteLine($"✅ Projects to process: {projectFolders.Length}");

//        ProcessProjectContainer(containerFolder, projectFolders);
//    }

//    SendLogReportMail(sessionLogFile);
//}
using OpenQA.Selenium;
using OpenQA.Selenium;
using OpenQA.Selenium.BiDi;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Support.UI;
using System;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO;
using System.IO;
using System.IO.Compression;
using System.IO.Compression;
using System.IO.Compression;
using System.Linq;
using System.Linq;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Linq;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace FolderScan
{
    public static class AutomationRunner
    {
        public static ProgressForm ProgressUI;
        // ================= ORIGINAL CONFIG =================
        static string extractRoot = @"C:\OneDriveDownload\Extracted";
        static string vdClientArchiveRoot = @"C:\vdClientArchive";
        //static string logRoot = @"C:\CheckInLogs";
        static string logRoot = Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory,
    "CheckInLogs"
);

        static string sessionLogFile;
        static string oneDriveBackupRoot =
    @"C:\Users\P01090879\OneDrive - Wipro PARI (1)\BackUpAllCheckin";

        public static void Run(string URL, string srcPath, string logrootfilepath)
        {
            RunAutomation(URL, srcPath, logrootfilepath);

        }
        static void RunAutomation(string URL, string logfilepath, string l)
        {
            //Directory.CreateDirectory(logRoot);

            //sessionLogFile = System.IO.Path.Combine(
            //    logRoot,
            //    $"CheckInLog_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            //);
            sessionLogFile = l;
            //File.WriteAllText(sessionLogFile, "Component Name,Component Path,Project,Octoplant Version,Date,Time\n");

            foreach (var containerFolder in Directory.GetDirectories(extractRoot))
            {
                Console.WriteLine($"\n📦 Processing container: {containerFolder}");

                //var level1Dirs = Directory.GetDirectories(containerFolder);
                //string projectContainer = level1Dirs.Length == 1 ? level1Dirs[0] : containerFolder;

                var projectFolders = Directory.GetDirectories(containerFolder);

                if (projectFolders.Length == 0)
                    projectFolders = new[] { containerFolder };

                //projectFolders = projectFolders
                //    .Select(p => new { Path = p, Time = GetProjectTimestamp(p) })
                //    .OrderBy(x => x.Time)
                //    .Select(x => x.Path)
                //    .ToArray();

                //Console.WriteLine($"✅ Projects to process: {projectFolders.Length}");

                ProcessProjectContainer(containerFolder, projectFolders, URL, logfilepath);

            }

        }


        static DateTime GetProjectTimestamp(string folderPath)
        {
            string name = Path.GetFileName(folderPath);

            // Expected: ProjectName_yyyyMMdd_HHmmss
            var match = System.Text.RegularExpressions.Regex.Match(
                name,
                @"_(\d{8})_(\d{6})$"
            );

            if (!match.Success)
            {
                Console.WriteLine($"⚠ Timestamp not found in folder: {name}");
                return DateTime.MaxValue;
            }

            string stamp = match.Groups[1].Value + match.Groups[2].Value;

            if (DateTime.TryParseExact(
                stamp,
                "yyyyMMddHHmmss",
                null,
                System.Globalization.DateTimeStyles.None,
                out DateTime dt))
            {
                return dt;
            }

            return DateTime.MaxValue;
        }
        //////////////static void ProcessProjectContainer(string containerFolder, string[] projectFolders)
        //////////////{
        //////////////    Console.WriteLine($"\n📂 Processing OneDrive Folder: {containerFolder}");

        //////////////    string componentName = Path.GetFileName(containerFolder);

        //////////////    foreach (var projectRoot in projectFolders)
        //////////////    {
        //////////////        string projectName = Path.GetFileName(projectRoot);
        //////////////        Console.WriteLine($"➡ Processing project: {projectName}");

        //////////////        // =====================================================
        //////////////        // RESOLVE DIRR + COMMENT
        //////////////        // =====================================================
        //////////////        var (dirR, comment, change) = ResolveDirR(containerFolder);

        //////////////        Console.WriteLine($"🚀 CHECKOUT DIRR: {dirR}");

        //////////////        // =====================================================
        //////////////        // CHECKOUT
        //////////////        // =====================================================
        //////////////        RunCmd(
        //////////////            @"C:\Program Files (x86)\vdogClient\VDogAutoCheckOut.exe",
        //////////////            $"/RD:C:\\vdClientArchive /AT:C /DirR:\"{dirR}\" /Account:s03007223 /Domain:wipropari /Password:Wipropari@30072"
        //////////////        );

        //////////////        // =====================================================
        //////////////        // OCTOPLANT BASE PATH (FROM DIRR)
        //////////////        // =====================================================
        //////////////        string octoBasePath = Path.Combine(
        //////////////            vdClientArchiveRoot,
        //////////////            dirR.TrimStart('\\')
        //////////////        );

        //////////////        // CLEAN only contents
        //////////////        SafeDeleteDirectoryContents(octoBasePath);

        //////////////        // =====================================================
        //////////////        // MOVE ONLY ProjectData FOLDER
        //////////////        // =====================================================
        //////////////        string sourceProjectData = Path.Combine(containerFolder, "ProjectData");

        //////////////        if (!Directory.Exists(sourceProjectData))
        //////////////        {
        //////////////            Console.WriteLine($"⚠ ProjectData not found in {containerFolder}");
        //////////////            continue;
        //////////////        }

        //////////////        string destProjectData = Path.Combine(octoBasePath, "ProjectData");

        //////////////        if (Directory.Exists(destProjectData))
        //////////////            Directory.Delete(destProjectData, true);

        //////////////        Directory.Move(sourceProjectData, destProjectData);

        //////////////        // =====================================================
        //////////////        // CHECK-IN (SAME DIRR + ProjectData)
        //////////////        // =====================================================
        //////////////        //string checkInDirR = dirR.TrimEnd('\\') + "\\ProjectData";
        //////////////        string checkInDirR = dirR.TrimEnd('\\') ;

        //////////////        Console.WriteLine("➡ CHECK-IN DIRR = " + checkInDirR);
        //////////////        RunCheckIn(checkInDirR, comment, change);

        //////////////        Console.WriteLine($"✔ ProjectData checked in successfully");

        //////////////        //string componentName = Path.GetFileName(dirR);
        //////////////        AppendCheckInLog(dirR, componentName);

        //////////////        // =====================================================
        //////////////        // CLEANUP
        //////////////        // =====================================================
        //////////////        Console.WriteLine("🧹 Deleting Octoplant folder...");
        //////////////        if (Directory.Exists(octoBasePath))
        //////////////            Directory.Delete(octoBasePath, true);

        //////////////    }
        //////////////}
        /// <summary>
        /// 


        static string hVer = "";
        /// </summary>
        /// <param name="containerFolder"></param>
        /// <param name="projectFolders"></param>
        /// <param name="URL"></param>
        static void ProcessProjectContainer(string containerFolder, string[] projectFolders, string URL, string logfilePath)
        {
            Console.WriteLine($"\n📂 Processing OneDrive Folder: {containerFolder}");

            string componentName = Path.GetFileName(containerFolder);

            foreach (var projectRoot in projectFolders)
            {

                hVer = "";
                if (projectRoot.Contains("Old Backup"))
                    continue;
                string urlFilePath = Path.Combine(containerFolder, "URL.txt");
                URL = File.ReadAllText(urlFilePath).Trim();

                string projectName = Path.GetFileName(projectRoot);
                Console.WriteLine($"➡ Processing project: {projectName}");

                //// 🔥 POPUP: STARTED
                //ProgressUI?.UpdateStatus(
                //    dirR,
                //    componentName,

                //    "Check-In Started"
                //);
                // =====================================================
                // RESOLVE DIRR + COMMENT
                // =====================================================
                var (dirR, comment, change, noComment, comments) = ResolveDirR(containerFolder, URL);
                string name = Path.GetFileName(dirR);
                try
                {
                    // 🔥 POPUP: STARTED
                    ProgressUI?.UpdateStatus(
                        dirR,
                        componentName,

                        "Check-In Started"
                    );
                    string log = $"{dirR} - {componentName} - {componentName} - Check-In Started - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                    File.AppendAllText(logfilePath, log + Environment.NewLine);
                    Console.WriteLine($"🚀 CHECKOUT DIRR: {dirR}");
                    //                if (Directory.Exists(projectRoot) &&
                    //Directory.GetFiles(projectRoot, "*.zip", SearchOption.TopDirectoryOnly).Any())
                    //                {
                    //                    AppendCheckInLog(name, componentName, dirR, "NA", "Check In Fail : Folder Contains .zip file " );
                    //                    return;
                    //                }
                    //                else if (Directory.Exists(projectRoot) &&
                    //Directory.GetFiles(projectRoot, "*.zap*", SearchOption.TopDirectoryOnly).Any())
                    //                {
                    //                    AppendCheckInLog(name, componentName, dirR, "NA", "Check In Fail : Folder Contains .zip file ");
                    //                    return;
                    //                }

                    //if (Directory.Exists(projectRoot) && Directory.EnumerateFiles(projectRoot,"*.zip",SearchOption.AllDirectories).Any())
                    //{
                    //    AppendCheckInLog(name,componentName,dirR,"NA","Check In Fail : Folder or subfolder contains .zip file");

                    //    return;
                    //}

                    if (Directory.Exists(projectRoot) && Directory.EnumerateFiles(projectRoot, "*.zap*", SearchOption.AllDirectories).Any())
                    {
                        AppendCheckInLog(name, componentName, dirR, "NA", "Check In Fail : Folder contains .zap file");
                        return;
                    }
                    // =====================================================
                    // CHECKOUT
                    // =====================================================
                    //MessageBox.Show(dirR);
                    //RunCmd(
                    //    @"C:\Program Files (x86)\vdogClient\VDogAutoCheckOut.exe",
                    //    $"/RD:C:\\vdClientArchive /AT:C /DirR:\"{dirR}\" /Account:p03007387 /Domain:wipropari /Password:#P1R2asadwa@ /WithBackups:Y /NumberOfArchives:1"
                    //);

                    RunCmd(
    @"C:\Program Files (x86)\vdogClient\VDogAutoCheckOut.exe",
    $"/RD:\"C:\\vdClientArchive\" " +
    "/AT:C " +
    $"/DirR:\"{dirR}\" " +
    "/Account:p03007387 " +
    "/Password:#P1R2asadwa$ " +
    "/Domain:wipropari " +
    "/WithBackups:Y " +
    "/NumberOfArchives:1"
);
                    //RunCmd(
                    //    @"C:\Program Files (x86)\vdogClient\VDogAutoCheckOut.exe",
                    //    $"/RD:\"C:\\vdClientArchive\" " +
                    //    "/AT:C " +
                    //    $"/DirR:\"{dirR}\" " +
                    //    "/Account:s03007223 " +
                    //    "/Password:Wipropari@30072 " +
                    //    "/Domain:wipropari " +
                    //    "/WithBackups:Y " +
                    //    "/NumberOfArchives:1"
                    //);

                    // =====================================================
                    // OCTOPLANT BASE PATH
                    // =====================================================

                    string octoBasePath = Path.Combine(
                        vdClientArchiveRoot,
                        dirR.TrimStart('\\')
                    );

                    SafeDeleteDirectoryContents(octoBasePath);
                    if (!Directory.Exists(octoBasePath))
                    {
                        //AppendCheckInLog(name, componentName, dirR, "Check In Fail");
                        throw new Exception("Check Out Fail");
                    }
                    bool hasMerOrGtx = false;   // ✅ DECLARED HERE (important)

                    string projectDataDir = Path.Combine(containerFolder, "ProjectData");

                    if (Directory.Exists(projectDataDir))
                    {
                        hasMerOrGtx = Directory
                            .EnumerateFiles(projectDataDir, "*.*", SearchOption.AllDirectories)
                            .Any(f =>
                                f.EndsWith(".mer", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".gtx", StringComparison.OrdinalIgnoreCase));

                        if (hasMerOrGtx)
                            Console.WriteLine("⚠ .mer / .GTX found in ProjectData folder");
                    }




                    // =====================================================
                    // MOVE ProjectData
                    // =====================================================
                    string sourceProjectData = Path.Combine(containerFolder, "ProjectData");

                    if (!Directory.Exists(sourceProjectData))
                        throw new Exception("ProjectData folder not found");

                    //string destProjectData = Path.Combine(octoBasePath, "ProjectData");
                    string destProjectData = Path.Combine(octoBasePath);
                    if (Directory.Exists(destProjectData))
                        Directory.Delete(destProjectData, true);

                    Directory.Move(sourceProjectData, destProjectData);

                    string checkInDirR = dirR.TrimEnd('\\');

                    Console.WriteLine("➡ CHECK-IN DIRR = " + checkInDirR);
                    //string error = RunCheckIn(checkInDirR, comment, change, logfilePath);
                    string error = RunCheckIn(
     checkInDirR,
     comment,
     change,
     logfilePath,
     hasMerOrGtx
 );



                    Console.WriteLine($"✔ ProjectData checked in successfully");
                    //File.WriteAllText(sessionLogFile, "Component Name,Project,Component Path,DateTime\n");
                    string currentDir = Directory.GetCurrentDirectory();
                    string iniPath = Path.Combine(currentDir, "AutoCheckIn.ini");
                    string hVersion = ReadHVersion(iniPath);
                    hVer = hVersion;

                    string currentDir2 = Directory.GetCurrentDirectory();
                    string iniPath2 = Path.Combine(currentDir2, "AutoCheckIn.ini");
                    bool nodiff = false;
                    UpdateIniCredentials(iniPath2);
                    if (hVersion == "NA" && string.IsNullOrEmpty(error))
                    {
                        hVer = "No Diff";
                        nodiff = true;
                        ProgressUI?.UpdateStatus(
                        dirR,
                        componentName,

                        "No Difference Found ");
                        log = $"{dirR} - {componentName} - {componentName} - \"No Difference Found - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                        File.AppendAllText(logfilePath, log + Environment.NewLine);
                        AppendCheckInLog(name, componentName, dirR, hVersion, "No Difference Found");
                    }
                    else if (hVersion == "NA")
                    {
                        ProgressUI?.UpdateStatus(
                        dirR,
                        componentName,

                        "Check In Fail : Check Log for details : " + logfilePath
                    );
                        log = $"{dirR} - {componentName} - {componentName} - \"Check In Fail : - {error}: - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                        File.AppendAllText(logfilePath, log + Environment.NewLine);
                        AppendCheckInLog(name, componentName, dirR, hVersion, "Check In Fail : Check Log for details : " + logfilePath);
                    }
                    else
                    {
                        ProgressUI?.UpdateStatus(
                        dirR,
                        componentName,

                        "Check-In Completed Successfully"
                    );
                        log = $"{dirR} - {componentName} - {componentName} - Check-In Completed Successfully - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                        File.AppendAllText(logfilePath, log + Environment.NewLine);
                        AppendCheckInLog(name, componentName, dirR, hVersion, "Check-In Completed Successfully: " + logfilePath);
                    }
                    //if (noComment)
                    //{
                    //    AppendCheckInLog(name, componentName, dirR, hVersion, "Check In Fail : Check Log for details : " + logfilePath);
                    //}
                    //else if (nodiff)
                    //{
                    //    AppendCheckInLog(name, componentName, dirR, hVersion, "No Difference Found");
                    //}
                    //else
                    //{
                    //    AppendCheckInLog(name, componentName, dirR, hVersion, "Check In Fail : Check Log for details : " + logfilePath);
                    //}

                    // =====================================================
                    // CLEANUP
                    // =====================================================
                    Console.WriteLine("🧹 Deleting Octoplant folder...");
                    if (Directory.Exists(octoBasePath))
                        Directory.Delete(octoBasePath, true);
                }
                catch (Exception ex)
                {
                    // ❌ POPUP: FAILED
                    ProgressUI?.UpdateStatus(
                        dirR,
                        componentName,

                        "❌ Check-In Failed"
                    );
                    string log = $"{dirR} - {componentName} - {componentName} - ❌ Check-In Failed - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    File.AppendAllText(logfilePath, log + Environment.NewLine);

                    Console.WriteLine(
                        $"❌ Error processing project {componentName}: {ex.Message}"
                    );

                    // continue with next project safely
                    //continue;
                }
            }
        }
        static void UpdateIniCredentials(string iniPath)
        {
            if (!File.Exists(iniPath))
                return;

            var lines = File.ReadAllLines(iniPath);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("Account=", StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] = "Account=";   // empty
                }
                else if (lines[i].StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] = "Password=";  // empty
                }
            }

            File.WriteAllLines(iniPath, lines);
        }
        public static string returnVersion()
        {
            return hVer;
        }
        // ============================================================
        // DIRR + COMMENT RESOLUTION (UPDATED LOGIC)
        // ============================================================
        static (string dirR, string comment, string change, bool noComment, string comments) ResolveDirR(string containerFolder, string URL)
        {
            bool noComment = false;
            string dirR = "";
            string comment = "";
            string change = "";
            string comments = "";
            string name = "";
            // --------------------------------------------------------
            // 1️⃣ DIRR: ONLY FROM .vdog-sdn
            // --------------------------------------------------------
            string vdogFile = Directory
                .GetFiles(containerFolder, "*.vdog-sdn", SearchOption.TopDirectoryOnly)
                .FirstOrDefault();

            if (vdogFile != null)
            {
                try
                {
                    XDocument xml = XDocument.Load(vdogFile);
                    dirR = xml.Descendants("DirR")
                              .FirstOrDefault()?.Value?.Trim();

                    name = xml.Descendants("Name")
                              .FirstOrDefault()?.Value?.Trim();

                    dirR = dirR + "\\" + name;
                    if (!string.IsNullOrWhiteSpace(dirR))
                    {
                        Console.WriteLine("➡ DIRR from .vdog-sdn = " + dirR);
                    }
                    else
                    {
                        Console.WriteLine("⚠ DIRR tag found but empty in .vdog-sdn");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("⚠ Error reading .vdog-sdn: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("⚠ No .vdog-sdn file found");
            }

            // --------------------------------------------------------
            // 2️⃣ COMMENT + CHANGE
            // COMMENT  -> SupplierComment.txt
            // CHANGE   -> SupplierChangeReason.txt
            // --------------------------------------------------------

            string reasonFile = Path.Combine(containerFolder, "SupplierChangeReason.txt");
            string commentFile = Path.Combine(containerFolder, "SupplierComment.txt");


            // CHANGE (from SupplierChangeReason.txt)
            if (File.Exists(reasonFile))
            {
                change = File.ReadAllText(reasonFile).Trim();
                Console.WriteLine("➡ CHANGE = " + change);
            }

            // COMMENT (from SupplierComment.txt)
            if (File.Exists(commentFile))
            {
                comment = File.ReadAllText(commentFile).Trim();
                Console.WriteLine("➡ COMMENT = " + comment);
            }

            // --------------------------------------------------------
            // 3️⃣ FALLBACK HANDLING
            // --------------------------------------------------------
            if (string.IsNullOrWhiteSpace(change))
            {
                change = "Updated Site Running Back Up";
            }

            //if (string.IsNullOrWhiteSpace(comment))
            //{
            //    noComment = true;
            //    string projCode = dirR.Split('\\')[2];
            //    projCode = projCode.Split('_')[0];
            //    comment = projCode +"_Site Person";//"Updated Site Running Back Up";
            //    comments = comment;
            //}


            if (string.IsNullOrWhiteSpace(comment) || comment.Contains("Updated by: Name of person"))
            {
                noComment = true;
                comment = URL;//GetOneDriveLinkFromPopup();
                //MessageBox.Show(URL);
                if (string.IsNullOrWhiteSpace(comment))
                {
                    MessageBox.Show("OneDrive link is required.");
                    //return;
                }

                var info = OpenLinkAndReadProjectData(comment);

                if (info == null)
                {
                    MessageBox.Show("Comment not found.");
                    //return;
                }
                MessageBox.Show(info.ModifiedDate + info.ModifiedBy);
                // ✅ FINAL COMMENT FORMAT
                comment = $"{info.ModifiedDate}, Modified by: {info.ModifiedBy}";
                comments = comment;
            }

            Console.WriteLine("➡ FINAL CHANGE = " + change);
            Console.WriteLine("➡ FINAL COMMENT = " + comment);

            return (dirR, comment, change, noComment, comments);

        }
        //public static ProjectDataInfo OpenLinkAndReadProjectData(string url)
        //{
        //    var options = new EdgeOptions();
        //    options.AddArgument("start-maximized");

        //    using (var driver = new EdgeDriver(options))
        //    {
        //        driver.Navigate().GoToUrl(url);
        //        Thread.Sleep(40000);

        //        var projectItem = driver.FindElements(By.XPath(
        //            "//span[text()='ProjectData.zip' or @aria-label='ProjectData.zip']"
        //        )).FirstOrDefault();

        //        if (projectItem == null)
        //            return null;

        //        var row = projectItem.FindElement(
        //            By.XPath("./ancestor::div[@role='row']")
        //        );

        //        string rowText = row.Text;

        //        return new ProjectDataInfo
        //        {
        //            ModifiedDate = ExtractDate(rowText),
        //            ModifiedBy = ExtractModifiedBy(rowText)
        //        };
        //    }
        //}
        //public static ProjectDataInfo OpenLinkAndReadProjectData(string url)
        //{
        //    try
        //    {
        //        var options = new EdgeOptions();
        //        options.AddArgument("--headless=new");
        //        options.AddArgument("--disable-gpu");
        //        options.AddArgument("--window-size=1920,1080");
        //        options.AddArgument("--no-sandbox");
        //        options.AddArgument("--disable-dev-shm-usage");

        //        using (var driver = new EdgeDriver(options))
        //        {
        //            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));

        //            driver.Navigate().GoToUrl(url);

        //            // ---------- LOGIN ----------
        //            var passwordBox = wait.Until(d =>
        //                d.FindElement(By.Id("i0118"))
        //            );

        //            passwordBox.Clear();
        //            passwordBox.SendKeys("C@ntRu1#3gbC");

        //            var nextButton = wait.Until(d =>
        //                d.FindElement(By.Id("idSIButton9"))
        //            );

        //            SafeClick(driver, nextButton);

        //            // ---------- WAIT FOR GRID ----------
        //            wait.Until(d =>
        //                d.FindElements(By.XPath("//div[@role='grid']")).Any()
        //            );

        //            // ---------- SCROLL GRID ----------
        //            var grid = driver.FindElement(By.XPath("//div[@role='grid']"));
        //            ((IJavaScriptExecutor)driver).ExecuteScript(
        //                "arguments[0].scrollTop = arguments[0].scrollHeight", grid
        //            );

        //            Thread.Sleep(1500);

        //            // ---------- FIND ProjectData ----------
        //            var projectItem = driver.FindElements(By.XPath(
        //                "//span[contains(@aria-label,'ProjectData') or normalize-space(text())='ProjectData']"
        //            )).FirstOrDefault();

        //            if (projectItem == null)
        //                return null;

        //            var row = projectItem.FindElement(
        //                By.XPath("ancestor::div[contains(@role,'row')]")
        //            );

        //            var cells = row.FindElements(By.XPath(".//div[@role='gridcell']"));

        //            return new ProjectDataInfo
        //            {
        //                ModifiedDate = cells.ElementAtOrDefault(3)?.Text ?? "Not found",
        //                ModifiedBy = cells.ElementAtOrDefault(4)?.Text ?? "Not found"
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //            string projectBasePath = AppDomain.CurrentDomain.BaseDirectory;
        //            string logDir = Path.Combine(projectBasePath, "Logs");
        //            string logFile = Path.Combine(logDir, "SeleniumErrors.log");

        //            Directory.CreateDirectory(logDir);

        //            File.AppendAllText(
        //                logFile,
        //                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {url} | {ex}\n"
        //            );



        //        return null; // ✔ process continues
        //    }
        //}
        //private static void SafeClick(IWebDriver driver, IWebElement element)
        //{
        //    try
        //    {
        //        element.Click();
        //    }
        //    catch
        //    {
        //        ((IJavaScriptExecutor)driver)
        //            .ExecuteScript("arguments[0].click();", element);
        //    }
        //}

        public static ProjectDataInfo OpenLinkAndReadProjectData(string url)
        {
            //var options = new EdgeOptions();
            //options.AddArgument("--start-maximized");
            var options = new EdgeOptions();// 1114

            // ================= HEADLESS CONFIG =================
            options.AddArgument("--headless=new");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            // 🔥 ADD THIS: suppress EdgeDriver console window
            var service = EdgeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            service.SuppressInitialDiagnosticInformation = true;
            try
            {
                //    using (var driver = new EdgeDriver(options))
                //{
                //    driver.Navigate().GoToUrl(url);
                using (var driver = new EdgeDriver(service, options))
                {
                    driver.Navigate().GoToUrl(url);
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));

                    // Ask user for password
                    string password = "C@ntRu1#3gbC";// PromptForPassword();
                    if (string.IsNullOrEmpty(password))
                        throw new Exception("Password input cancelled by user.");

                    // Wait for password input
                    var passwordBox = wait.Until(d =>
                        d.FindElement(By.Id("i0118"))
                    );

                    passwordBox.Clear();
                    passwordBox.SendKeys(password);

                    // Click Next / Sign In
                    var nextButton = wait.Until(d =>
                        d.FindElement(By.Id("idSIButton9"))
                    );
                    //nextButton.Click();
                    SafeClick(driver, nextButton);

                    //HandleMfaIfPresent(driver, wait);

                    // Wait for OneDrive page
                    //wait.Until(d => d.PageSource.Contains("ProjectData.zip"));

                    //var projectItem = driver.FindElements(By.XPath(
                    //    "//span[text()='ProjectData.zip' or @aria-label='ProjectData.zip']"
                    //)).FirstOrDefault();

                    //if (projectItem == null)
                    //    return null;

                    //var row = projectItem.FindElement(By.XPath("./ancestor::div[@role='row']"));
                    //string rowText = row.Text;

                    //return new ProjectDataInfo
                    //{
                    //    ModifiedDate = ExtractDate(rowText),
                    //    ModifiedBy = ExtractModifiedBy(rowText)
                    //};
                    wait.Until(d =>
                d.FindElements(By.XPath("//div[@role='grid']")).Any()
            );

                    // 2️⃣ Scroll grid (important for headless)
                    var grid = driver.FindElement(By.XPath("//div[@role='grid']"));
                    ((IJavaScriptExecutor)driver).ExecuteScript(
                        "arguments[0].scrollTop = arguments[0].scrollHeight", grid
                    );
                    Thread.Sleep(1500);

                    // 3️⃣ FIND ProjectData.zip  ✅ DECLARED HERE
                    var projectItem = driver.FindElements(By.XPath(
                        "//span[contains(@aria-label,'ProjectData.zip') " +
                        "or contains(@title,'ProjectData.zip') " +
                        "or normalize-space(text())='ProjectData.zip']"
                    )).FirstOrDefault();

                    // 4️⃣ NULL CHECK
                    if (projectItem == null)
                        return null;
                    // 🔒 Ensure interactable
                    ((IJavaScriptExecutor)driver)
                        .ExecuteScript("arguments[0].scrollIntoView(true);", projectItem);

                    Thread.Sleep(500);
                    // 5️⃣ FIND ROW (NOW VALID)
                    var row = projectItem.FindElement(
                        By.XPath("ancestor::div[contains(@role,'row')]")
                    );

                    // 6️⃣ GET CELLS
                    var cells = row.FindElements(By.XPath(".//div[@role='gridcell']"));

                    // 7️⃣ FULL MODIFIED DATE (attribute-based)
                    string modifiedDate =
                           cells.ElementAtOrDefault(3)?.GetAttribute("aria-label")
                        ?? cells.ElementAtOrDefault(3)?.GetAttribute("title")
                        ?? cells.ElementAtOrDefault(3)?.Text
                        ?? "Not found";

                    // 8️⃣ MODIFIED BY
                    string modifiedBy =
                           cells.ElementAtOrDefault(4)?.GetAttribute("aria-label")
                        ?? cells.ElementAtOrDefault(4)?.Text
                        ?? "Not found";

                    // 9️⃣ RETURN RESULT
                    return new ProjectDataInfo
                    {
                        ModifiedDate = modifiedDate,
                        ModifiedBy = modifiedBy
                    };
                }
            }
            catch (WebDriverException)
            {
                // 🔥 Selenium instability in headless mode – swallow & continue
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static void SafeClick(IWebDriver driver, IWebElement element)
        {
            try
            {
                element.Click();
            }
            catch
            {
                ((IJavaScriptExecutor)driver)
                    .ExecuteScript("arguments[0].click();", element);
            }
        }

        private static void HandleMfaIfPresent(IWebDriver driver, WebDriverWait wait)
        {
            // Wait briefly to see if MFA page appears
            try
            {
                wait.Until(d =>
                    d.Url.Contains("login.microsoftonline.com") &&
                    d.PageSource.Contains("Approve sign in request")
                );
            }
            catch
            {
                // MFA not shown
                return;
            }

            // Extract approval number from page text (e.g. 79)
            string approvalNumber = "Check your app";

            try
            {
                string pageText = driver.FindElement(By.TagName("body")).Text;

                // Look for a standalone 2-digit number
                var match = Regex.Match(pageText, @"\b\d{2}\b");

                if (match.Success)
                    approvalNumber = match.Value;
            }
            catch
            {
                // Ignore extraction failures
            }

            // Show your custom popup
            ShowMfaPopup(approvalNumber);

            // Wait until authentication completes (redirect happens)
            wait.Until(d =>
                !d.Url.Contains("login.microsoftonline.com")
            );
        }
        private static void ShowMfaPopup(string number)
        {
            MessageBox.Show(
                $"Microsoft sign-in approval required.\n\n" +
                $"Open your Outlook / Authenticator app and approve the request.\n\n" +
                $"Approval number: {number}",
                "MFA Approval Required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        public static string PromptForPassword()
        {
            using (Form form = new Form())
            {
                form.Text = "Microsoft Login";
                form.Width = 360;
                form.Height = 150;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                Label label = new Label()
                {
                    Text = "Enter Microsoft account password:",
                    Left = 10,
                    Top = 15,
                    AutoSize = true
                };

                TextBox textBox = new TextBox()
                {
                    Left = 10,
                    Top = 40,
                    Width = 320,
                    UseSystemPasswordChar = true
                };

                Button okButton = new Button()
                {
                    Text = "OK",
                    Left = 230,
                    Width = 100,
                    Top = 75,
                    DialogResult = DialogResult.OK
                };

                form.Controls.Add(label);
                form.Controls.Add(textBox);
                form.Controls.Add(okButton);
                form.AcceptButton = okButton;

                return form.ShowDialog() == DialogResult.OK
                    ? textBox.Text
                    : null;
            }
        }
        private static string PrepareEdgeAutomationProfile()
        {
            string sourceProfile =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"Microsoft\Edge\User Data"
                );

            string targetProfile = @"C:\Temp\EdgeAutomationProfile";

            if (!Directory.Exists(targetProfile))
            {
                CopyDirectory(sourceProfile, targetProfile);
            }

            return targetProfile;
        }

        private static void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string targetFile = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, targetFile, true);
            }

            foreach (string directory in Directory.GetDirectories(sourceDir))
            {
                string targetSubDir = Path.Combine(targetDir, Path.GetFileName(directory));
                CopyDirectory(directory, targetSubDir);
            }
        }
        //public static ProjectDataInfo OpenLinkAndReadProjectData(string url)
        //{
        //    string automationProfile = PrepareEdgeAutomationProfile();

        //    var options = new EdgeOptions();
        //    options.AddArgument($"--user-data-dir={automationProfile}");
        //    options.AddArgument("--start-maximized");
        //    options.AddArgument("--disable-dev-shm-usage");
        //    options.AddArgument("--no-sandbox");

        //    using (var driver = new EdgeDriver(options))
        //    {
        //        driver.Navigate().GoToUrl(url);

        //        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        //        wait.Until(d => d.PageSource.Contains("ProjectData.zip"));

        //        var projectItem = driver.FindElements(By.XPath(
        //            "//span[text()='ProjectData.zip' or @aria-label='ProjectData.zip']"
        //        )).FirstOrDefault();

        //        if (projectItem == null)
        //            return null;

        //        var row = projectItem.FindElement(
        //            By.XPath("./ancestor::div[@role='row']")
        //        );

        //        string rowText = row.Text;

        //        return new ProjectDataInfo
        //        {
        //            ModifiedDate = ExtractDate(rowText),
        //            ModifiedBy = ExtractModifiedBy(rowText)
        //        };
        //    }
        //}
        private static string ExtractDate(string text)
        {
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                // Absolute date with time
                if (Regex.IsMatch(line, @"\d{1,2}/\d{1,2}/\d{4}.*?(AM|PM)", RegexOptions.IgnoreCase))
                    return line.Trim();

                // Month Day
                if (Regex.IsMatch(line, @"^[A-Za-z]+\s+\d{1,2}$"))
                    return line.Trim();

                // Relative time (STRICT)
                if (Regex.IsMatch(line, @"^\d+\s+(minutes?|hours?|days?)\s+ago$", RegexOptions.IgnoreCase))
                    return line.Trim();

                // Yesterday
                if (Regex.IsMatch(line, @"^yesterday$", RegexOptions.IgnoreCase))
                    return line.Trim();

                //return lines[1];
            }

            return "Not found";
        }
        private static string ExtractModifiedBy(string text)
        {
            var lines = text
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l =>
                    !string.IsNullOrWhiteSpace(l) &&
                    !l.Equals("ProjectData.zip", StringComparison.OrdinalIgnoreCase) &&
                    !Regex.IsMatch(l, @"\d") &&          // remove dates & sizes
                    !l.EndsWith("MB", StringComparison.OrdinalIgnoreCase) &&
                    !l.Equals("Private", StringComparison.OrdinalIgnoreCase)
                )
                .ToList();

            // After filtering, the remaining line is the person name
            return lines.Count > 0 ? lines[0] : "Not found";
        }
        public class ProjectDataInfo
        {
            public string ModifiedDate { get; set; }
            public string ModifiedBy { get; set; }
        }
        //////////////////// ============================================================
        //////////////////// DIRR + COMMENT RESOLUTION
        //////////////////// ============================================================
        //////////////////static (string dirR, string comment) ResolveDirR(string containerFolder)
        //////////////////{
        //////////////////    string dirR = "";
        //////////////////    string comment = "Updated Project";

        //////////////////    // --------------------------------------------------------
        //////////////////    // 1️⃣ FIRST: .vdog-sdn
        //////////////////    // --------------------------------------------------------
        //////////////////    string vdogFile = Directory
        //////////////////        .GetFiles(containerFolder, "*.vdog-sdn", SearchOption.TopDirectoryOnly)
        //////////////////        .FirstOrDefault();

        //////////////////    if (vdogFile != null)
        //////////////////    {
        //////////////////        try
        //////////////////        {
        //////////////////            XDocument xml = XDocument.Load(vdogFile);
        //////////////////            dirR = xml.Descendants("DirR")
        //////////////////                      .FirstOrDefault()?.Value?.Trim();

        //////////////////            if (!string.IsNullOrWhiteSpace(dirR))
        //////////////////            {
        //////////////////                Console.WriteLine("➡ DIRR from .vdog-sdn = " + dirR);
        //////////////////                return (dirR, comment);
        //////////////////            }
        //////////////////        }
        //////////////////        catch (Exception ex)
        //////////////////        {
        //////////////////            Console.WriteLine("⚠ Error reading .vdog-sdn: " + ex.Message);
        //////////////////        }
        //////////////////    }

        //////////////////    // --------------------------------------------------------
        //////////////////    // 2️⃣ SECOND: README.txt
        //////////////////    // --------------------------------------------------------
        //////////////////    string readmePath = System.IO.Path.Combine(containerFolder, "README.txt");

        //////////////////    if (File.Exists(readmePath))
        //////////////////    {
        //////////////////        foreach (var line in File.ReadAllLines(readmePath))
        //////////////////        {
        //////////////////            if (line.StartsWith("DIRR=", StringComparison.OrdinalIgnoreCase))
        //////////////////                dirR = line.Substring(5).Trim();

        //////////////////            if (line.StartsWith("COMMENT=", StringComparison.OrdinalIgnoreCase))
        //////////////////                comment = line.Substring(8).Trim();
        //////////////////        }

        //////////////////        if (!string.IsNullOrWhiteSpace(dirR))
        //////////////////        {
        //////////////////            Console.WriteLine("➡ DIRR from README = " + dirR);
        //////////////////            Console.WriteLine("➡ COMMENT from README = " + comment);
        //////////////////            return (dirR, comment);
        //////////////////        }
        //////////////////    }

        //////////////////    // --------------------------------------------------------
        //////////////////    // 3️⃣ LAST FALLBACK: Folder name
        //////////////////    // --------------------------------------------------------
        //////////////////    string folderName = Path.GetFileName(containerFolder);
        //////////////////    dirR = "\\" + folderName.Replace("+", "\\");

        //////////////////    Console.WriteLine("➡ DIRR fallback (folder name) = " + dirR);

        //////////////////    return (dirR, comment);
        //////////////////}
        // ============================================================
        // CHECK-IN (INI BASED)
        // ============================================================
        //static string RunCheckIn(string dirR, string comment, string change, string logfilePath)
        //{
        //    string output = "";
        //    string plainPassword = "#P1R2asadwa@"; // already decrypted
        //    string iniFile = $@"[User]
        //    Account={"p03007387"}
        //    Domain={"wipropari"}
        //    Password={plainPassword}
        //    [Component]
        //    Dir={dirR}
        //    [Version]
        //    Enabled=Y
        //    WithoutComparison=N
        //    Comment={comment}
        //    ChangeReason={change}";

        //////        static string RunCheckIn(string dirR, string comment, string change, string logfilePath)
        //////        {
        //////            string name = Path.GetFileName(dirR);
        //////            string withoutComparison = "WithoutComparison=N"; // default

        //////            string[] noComparisonNames =
        //////            {
        //////    "Sinumerik ONE",
        //////    "HMIPro CS",
        //////    "PartProgram",
        //////    "Transline"
        //////};

        //////            if (noComparisonNames.Any(n =>
        //////                    name.Equals(n, StringComparison.OrdinalIgnoreCase)))
        //////            {
        //////                withoutComparison = "WithoutComparison=Y";
        //////                Console.WriteLine($"⚠ WithoutComparison enabled for folder: {name}");
        //////            }


        //////            string output = "";
        //////            string plainPassword = "#P1R2asadwa@"; // already decrypted
        //////            string iniFile = $@"[User]
        //////            Account={"p03007387"}
        //////            Domain={"wipropari"}
        //////            Password={plainPassword}
        //////            [Component]
        //////            Dir={dirR}
        //////            [Version]
        //////            Enabled=Y
        //////            {withoutComparison}
        //////            Comment={comment}
        //////            ChangeReason={change}";
        //            string plainPassword = "Wipropari@30072"; // already decrypted
        //            string iniFile = $@"[User]
        //Account={"s03007223"}
        //Domain={"wipropari"}
        //Password={plainPassword}
        //[Component]
        //Dir={dirR}
        //[Version]
        //Enabled=Y
        //WithoutComparison=N
        //Comment={comment}
        //ChangeReason={change}";
        //static string RunCheckIn(string dirR, string comment, string change, string logfilePath)
        static string RunCheckIn(
      string dirR,
      string comment,
      string change,
      string logfilePath,
      bool forceWithoutComparison
  )



        {
            string name = Path.GetFileName(dirR);
            string withoutComparison = "WithoutComparison=N"; // default

            string[] noComparisonNames =
            {
        "Sinumerik ONE",
        "HMIPro CS",
        "PartProgram",
        "Transline"
    };

            // -------------------------------
            // Condition 1: Folder name match
            // -------------------------------
            if (noComparisonNames.Any(n =>
                    name.Equals(n, StringComparison.OrdinalIgnoreCase)))
            {
                withoutComparison = "WithoutComparison=Y";
                Console.WriteLine($"⚠ WithoutComparison enabled (folder rule): {name}");
            }

            // ------------------------------------------------
            // Condition 2: ProjectData.zip contains .mer / .GTX
            // ------------------------------------------------
            // ------------------------------------------------
            // Condition 2: ProjectData.zip contains .mer / .GTX
            // ------------------------------------------------


            // NEW rule (folder content based)
            if (forceWithoutComparison)
            {
                withoutComparison = "WithoutComparison=Y";
                Console.WriteLine("⚠ WithoutComparison enabled (ProjectData contains .mer/.GTX)");
            }

            string output = "";
            string plainPassword = "#P1R2asadwa$"; // already decrypted

            string iniFile = $@"[User]
Account=p03007387
Domain=wipropari
Password={plainPassword}
[Component]
Dir={dirR}
[Version]
Enabled=Y
{withoutComparison}
Comment={comment}
ChangeReason={change}";
            string currentDir = Directory.GetCurrentDirectory();
            string iniPath = Path.Combine(currentDir, "AutoCheckIn.ini");
            File.WriteAllText(iniPath, iniFile);

            using (var process = new Process())
            {
                process.StartInfo.FileName = @"C:\Program Files (x86)\vdogClient\VDogAutoCheckIn.exe";
                process.StartInfo.Arguments = "/at:C /rd:C:\\vdClientArchive /CFile:" + iniPath;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                //Thread.Sleep(10000);

                output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                File.AppendAllText(logfilePath, output + " - " + DateTime.Now);
                if (!string.IsNullOrEmpty(error))
                {
                    MessageBox.Show("Error: " + error, "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    string checkInFolder = Path.Combine(currentDir, "CheckIn_Error");
                    if (!Directory.Exists(checkInFolder))
                        Directory.CreateDirectory(checkInFolder);

                    string errorFilePath = Path.Combine(checkInFolder, $"Error_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

                }

                // ---------- AutoCheckOut ----------


            }

            return output;
        }
        static void ExtractZipOverwrite(string zip, string target)
        {
            using (var archive = ZipFile.OpenRead(zip))
            {
                foreach (var e in archive.Entries)
                {
                    string path = System.IO.Path.Combine(target, e.FullName);
                    if (string.IsNullOrEmpty(e.Name))
                    {
                        Directory.CreateDirectory(path);
                        continue;
                    }
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    e.ExtractToFile(path, true);
                }
            }
        }

        static void SafeDeleteDirectoryContents(string path)
        {
            if (!Directory.Exists(path)) return;

            foreach (var d in Directory.GetDirectories(path))
                try { Directory.Delete(d, true); } catch { }

            foreach (var f in Directory.GetFiles(path))
                try { File.Delete(f); } catch { }
        }

        static void WaitForDownload(string folder, TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();
            long lastSize = -1;

            while (sw.Elapsed < timeout)
            {
                var zip = Directory.GetFiles(folder, "*.zip").FirstOrDefault();
                var cr = Directory.GetFiles(folder, "*.crdownload").FirstOrDefault();

                if (cr != null) { Thread.Sleep(1000); continue; }

                if (zip != null)
                {
                    long size = new FileInfo(zip).Length;
                    if (size == lastSize) return;
                    lastSize = size;
                }

                Thread.Sleep(1000);
            }

            throw new Exception("Download timeout");
        }

        static void RunCmd(string exe, string args)
        {
            Console.WriteLine($">>> {exe} {args}");

            var psi = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var p = Process.Start(psi))
            {
                string o = p.StandardOutput.ReadToEnd();
                string e = p.StandardError.ReadToEnd();
                p.WaitForExit();

                if (!string.IsNullOrEmpty(o)) Console.WriteLine(o);
                if (!string.IsNullOrEmpty(e)) Console.WriteLine("ERR: " + e);
            }
        }
        static string ReadHVersion(string iniPath)
        {
            if (!File.Exists(iniPath))
                return "NA";

            foreach (var line in File.ReadAllLines(iniPath))
            {
                if (line.Trim().StartsWith("HVersion=", StringComparison.OrdinalIgnoreCase))
                {
                    return line.Split('=')[1].Trim();
                }
            }

            return "NA";
        }

        //Working
        //static void AppendCheckInLog(string name, string component, string project, string hVersion, string comment)
        //{


        //    if (!string.IsNullOrWhiteSpace(project) && project.StartsWith("\\"))
        //    {

        //        project = project.Substring(1);

        //    }

        //    string date = DateTime.Now.ToString("dd-MMM-yyyy"); // 19-Feb-2018

        //    string time = DateTime.Now.ToString("HH:mm:ss");


        //    if (hVersion == "NA")
        //    {

        //        //string line = $"{name},{project},{result},{hVersion},{DateTime.Now:yyyy-MM-dd},{DateTime.Now:HH:mm:ss}, {comment}\n";

        //        string line = $"{DateTime.Now:dd-MMM-yyyy},{DateTime.Now:HH:mm:ss},{hVersion},{project},{name},{comment}\n";

        //        File.AppendAllText(sessionLogFile, line);

        //    }
        //    else
        //    {

        //        string line = $"{DateTime.Now:dd-MMM-yyyy},{DateTime.Now:HH:mm:ss},{hVersion},{project},{name}\n";

        //        File.AppendAllText(sessionLogFile, line);

        //    }

        //}
        static void AppendCheckInLog(string name, string component, string project, string hVersion, string comment)
        {
            if (!string.IsNullOrWhiteSpace(project) && project.StartsWith("\\"))
            {
                project = project.Substring(1);
            }

            string date = "=\"" + DateTime.Now.ToString("dd-MMM-yyyy") + "\"";
            //string date = DateTime.Now.ToString("dd-MMM-yyyy");   // 13-Jan-2026
            string time = DateTime.Now.ToString("hh:mm:ss tt");   // 02:35:10 PM

            if (hVersion == "NA")
            {
                string line = $"{date},{time},{hVersion},{project},{name},{comment}\n";
                File.AppendAllText(sessionLogFile, line);
            }
            else
            {
                string line = $"{date},{time},{hVersion},{project},{name}\n";
                File.AppendAllText(sessionLogFile, line);
            }
        }
        // ============================================================Working Code
        //static void AppendCheckInLog(string name, string component, string project, string hVersion, string comment)
        //{
        //    if (!string.IsNullOrWhiteSpace(component) && component.StartsWith("\\"))
        //    {
        //        component = component.Substring(1);
        //    }

        //    // 🔹 Required date & time formats
        //    string date = DateTime.Now.ToString("dd-MMM-yyyy"); // 19-Feb-2018
        //    string time = DateTime.Now.ToString("HH:mm:ss");

        //    //        string result = project
        //    //.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries)
        //    //.Skip(1)          // skip "P2Proj2021_22"
        //    //.FirstOrDefault();
        //    if (hVersion == "NA")
        //    {
        //        //string line = $"{name},{project},{result},{hVersion},{DateTime.Now:yyyy-MM-dd},{DateTime.Now:HH:mm:ss}, {comment}\n";
        //        string line = $"{DateTime.Now:yyyy-MM-dd},{DateTime.Now:HH:mm:ss},{hVersion},{project},{name},{comment}\n";
        //        File.AppendAllText(sessionLogFile, line);
        //    }
        //    else
        //    {
        //        string line = $"{DateTime.Now:yyyy-MM-dd},{DateTime.Now:HH:mm:ss},{hVersion},{project},{name}\n";
        //        File.AppendAllText(sessionLogFile, line);
        //    }
        //}
        //static void AppendCheckInLog(string name, string component, string project, string hVersion, string URL, string comments)
        //{
        //    //if (hVersion == "NA")
        //    //{
        //    //    hVersion = "Error For Check In";
        //    //}
        //    string line = $"{name},{project},{component},{hVersion},{DateTime.Now:yyyy-MM-dd HH:mm:ss}, {"Default Comment Added : " + comments}, {URL}\n";
        //    File.AppendAllText(sessionLogFile, line);
        //}

        // ============================================================
        //static void SendLogReportMail(string csvPath)
        //{
        //    try
        //    {
        //        Outlook.Application outlook = new Outlook.Application();
        //        Outlook.MailItem mail =
        //            (Outlook.MailItem)outlook.CreateItem(Outlook.OlItemType.olMailItem);

        //        mail.Subject = "Octoplant Check-In Report";
        //        mail.Body =
        //            "Dear Team,\n\nPlease find attached the Octoplant Check-In report.\n\nRegards,\nAutomation";

        //        mail.To = "all member in csv";
        //        mail.Attachments.Add(csvPath);
        //        mail.DeleteAfterSubmit = true;
        //        mail.Send();
        //    }
        //    catch { }
        //}
        static void SendLogReportMail(string csvPath)
        {
            try
            {
                Outlook.Application outlook = new Outlook.Application();
                Outlook.MailItem mail =
                    (Outlook.MailItem)outlook.CreateItem(Outlook.OlItemType.olMailItem);

                mail.Subject = "Octoplant Check-In Report";
                mail.Body =
                    "Dear Team,\n\nPlease find attached the Octoplant Check-In report.\n\nRegards,\nAutomation";

                // 🔥 ADD: Read recipients from CSV
                //string recipientsCsv = @"C:\MailSend\Recipients.csv";   // <-- your path
                string recipientsCsv = Path.Combine(
                  AppDomain.CurrentDomain.BaseDirectory,
                  "MailSend",
                  "Recipients.csv"
              );
                string toList = ReadEmailIdsFromCsv(recipientsCsv);

                if (!string.IsNullOrWhiteSpace(toList))
                    mail.To = toList;

                // ✔ already correct
                mail.Attachments.Add(csvPath);

                mail.DeleteAfterSubmit = true;
                mail.Send();
            }
            //catch (Exception ex)
            //{
            //    File.AppendAllText(@"C:\MailSend\MailError.log", ex.ToString());
            //}
            catch (Exception ex)
            {
                string mailSendDir = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "MailSend");

                Directory.CreateDirectory(mailSendDir);

                File.AppendAllText(
                    Path.Combine(mailSendDir, "MailError.log"),
                    DateTime.Now + " : " + ex + Environment.NewLine
                );
            }

        }
        static string ReadEmailIdsFromCsv(string csvPath)
        {
            if (!File.Exists(csvPath))
                return string.Empty;

            var emails = File.ReadAllLines(csvPath)
                .Skip(1) // ✅ Skip header row
                .Select(line => line.Trim().Trim('"')) // remove quotes if any
                .Select(line => line.Replace("\uFEFF", "")) // ✅ remove BOM
                .Where(line =>
                    !string.IsNullOrWhiteSpace(line) &&
                    line.Contains("@") &&
                    line.Contains("."))
                .Distinct()
                .ToList();

            return string.Join(";", emails); // Outlook requires ;
        }

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
                        System.IO.Path.Combine(doneFolder, Path.GetFileName(projectPath));

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

        class CheckInLogEntry
        {
            public string Component;
            public string Project;
            public DateTime Time;
        }
    }
}
// ============================================================
// PROCESS ONE ONEDRIVE CONTAINER
// ============================================================
//static void ProcessProjectContainer(string containerFolder, string[] projectFolders)
//{
//    Console.WriteLine($"\n📂 Processing OneDrive Folder: {containerFolder}");

//    string componentName = Path.GetFileName(containerFolder);

//    foreach (var projectRoot in projectFolders)
//    {
//        string projectName = Path.GetFileName(projectRoot);

//        Console.WriteLine($"➡ Processing project (by upload time): {projectName}");

//        // =====================================================
//        // RESOLVE DIRR ONCE (USED FOR BOTH CHECKOUT & CHECK-IN)
//        // =====================================================
//        var (dirR, comment) = ResolveDirR(projectRoot);

//        Console.WriteLine($"🚀 CHECKOUT DIRR: {dirR}");

//        RunCmd(
//            @"C:\Program Files (x86)\vdogClient\VDogAutoCheckOut.exe",
//            $"/RD:C:\\vdClientArchive /AT:C /DirR:\"{dirR}\" /Account: /Domain:wipropari /Password:"
//        );

//        // =====================================================
//        // OCTOPLANT FOLDER BASED ON RESOLVED DIRR
//        // =====================================================
//        string octoFolderPath = System.IO.Path.Combine(
//            vdClientArchiveRoot,
//            dirR.TrimStart('\\')
//        );

//        // CLEAN Octoplant folder contents ONLY
//        SafeDeleteDirectoryContents(octoFolderPath);

//        // DESTINATION = Octoplant checkout folder + project name
//        string dest = System.IO.Path.Combine(octoFolderPath, projectName);

//        if (Directory.Exists(dest))
//            Directory.Delete(dest, true);

//        // MOVE ONLY THIS PROJECT
//        Directory.Move(projectRoot, dest);

//        // =====================================================
//        // CHECK-IN USING SAME BASE DIRR
//        // =====================================================
//        string fullDirR = dirR.TrimEnd('\\') + "\\" + projectName;

//        Console.WriteLine("➡ CHECK-IN DIRR = " + fullDirR);

//        RunCheckIn(fullDirR, comment);

//        Console.WriteLine($"✔ Project {projectName} checked in successfully");

//        AppendCheckInLog(componentName, projectName);

//        // -------------------------------------------------
//        // DELETE OCTOPLANT FOLDER AFTER EACH PROJECT
//        // -------------------------------------------------
//        Console.WriteLine("🧹 Deleting Octoplant folder...");
//        if (Directory.Exists(octoFolderPath))
//            Directory.Delete(octoFolderPath, true);
//    }
//}

//SafeDeleteDirectoryContents(downloadDir);
//Directory.CreateDirectory(downloadDir);

//SafeDeleteDirectoryContents(extractRoot);
//Directory.CreateDirectory(extractRoot);

//// STEP 1: DOWNLOAD ALL ONEDRIVE ZIPS
//foreach (var url in oneDriveUrls)
//{
//    DownloadOneDriveZip(url, downloadDir);
//}

//// STEP 2: EXTRACT ALL ZIP FILES
//foreach (var zip in Directory.GetFiles(downloadDir, "*.zip"))
//{
//    ExtractZipOverwrite(zip, extractRoot);
//    File.Delete(zip);
//}

// STEP 3: PROCESS EACH EXTRACTED ONEDRIVE FOLDER

//CreateOneDriveCheckInDoneFolder();
// Read CSV paths (already exists in your code)
//List<string> csvProjectPaths = ReadFolderListFromCsv(CSVName);

// --- your existing check-in logic here ---
// project by project check-in
// no changes
// ----------------------------------------

// 🔥 AFTER ALL CHECK-INS COMPLETED
//MoveCsvProjectsToOneDriveBackup(csvProjectPaths);




//////////////        // ============================================================
//////////////        // PROCESS ONE ONEDRIVE CONTAINER
//////////////        // ============================================================
//////////////        static void ProcessProjectContainer(string containerFolder, string[] projectFolders)
//////////////        {
//////////////            Console.WriteLine($"\n📂 Processing OneDrive Folder: {containerFolder}");

//////////////            // Checkout DIRR comes from container name
//////////////            string componentName = Path.GetFileName(containerFolder);
//////////////            string containerName = Path.GetFileName(containerFolder);
//////////////            string checkoutDirR = "\\" + containerName.Replace("+", "\\");

//////////////            string octoFolderPath = Path.Combine(
//////////////                vdClientArchiveRoot,
//////////////                checkoutDirR.TrimStart('\\')
//////////////            );

//////////////            Console.WriteLine($"🚀 CHECKOUT ONCE: {checkoutDirR}");

//////////////            RunCmd(
//////////////                @"C:\Program Files (x86)\vdogClient\VDogAutoCheckOut.exe",
//////////////                $"/RD:C:\\vdClientArchive /AT:C /DirR:\"{checkoutDirR}\" /Account: /Domain:wipropari /Password:"
//////////////            );


//////////////            foreach (var projectRoot in projectFolders)
//////////////            {
//////////////                string projectName = Path.GetFileName(projectRoot);

//////////////                Console.WriteLine($"➡ Processing project (by upload time): {projectName}");
//////////////                var (projectDirR, comment) = ResolveDirR(projectRoot);
//////////////                // CLEAN Octoplant folder contents ONLY
//////////////                SafeDeleteDirectoryContents(octoFolderPath);

//////////////                // DESTINATION = Octoplant checkout folder + project name
//////////////                string dest = Path.Combine(octoFolderPath, projectName);

//////////////                if (Directory.Exists(dest))
//////////////                    Directory.Delete(dest, true);

//////////////                // MOVE ONLY THIS PROJECT
//////////////                Directory.Move(projectRoot, dest);

//////////////                // FULL DIRR FOR CHECK-IN
//////////////                string fullDirR = checkoutDirR.TrimEnd('\\') + "\\" + projectName;

//////////////                Console.WriteLine("➡ CHECK-IN DIRR = " + fullDirR);

//////////////                RunCheckIn(fullDirR, comment);

//////////////                Console.WriteLine($"✔ Project {projectName} checked in successfully");


//////////////                AppendCheckInLog(componentName, projectName);

//////////////            }
//////////////            // -------------------------------------------------
//////////////            // DELETE OCTOPLANT FOLDER AFTER ALL PROJECTS
//////////////            // -------------------------------------------------
//////////////            Console.WriteLine("🧹 Deleting Octoplant folder...");
//////////////            if (Directory.Exists(octoFolderPath))
//////////////                Directory.Delete(octoFolderPath, true);
//////////////        }
//////////////        static (string dirR, string comment) ResolveDirR(string projectRoot)
//////////////        {
//////////////            string dirR = "";
//////////////            string comment = "Auto Check-in";

//////////////            // --------------------------------------------------------
//////////////            // 1️⃣ FIRST: .vdog-sdn (HIGHEST PRIORITY)
//////////////            // --------------------------------------------------------
//////////////            string vdogFile = Directory
//////////////                .GetFiles(projectRoot, "*.vdog-sdn", SearchOption.TopDirectoryOnly)
//////////////                .FirstOrDefault();

//////////////            if (vdogFile != null)
//////////////            {
//////////////                try
//////////////                {
//////////////                    XDocument xml = XDocument.Load(vdogFile);
//////////////                    dirR = xml.Descendants("DirR")
//////////////                              .FirstOrDefault()?.Value?.Trim();

//////////////                    if (!string.IsNullOrWhiteSpace(dirR))
//////////////                    {
//////////////                        Console.WriteLine("➡ DIRR from .vdog-sdn = " + dirR);
//////////////                        return (dirR, comment);   // ✅ STOP HERE (NO README CHECK)
//////////////                    }
//////////////                }
//////////////                catch (Exception ex)
//////////////                {
//////////////                    Console.WriteLine("⚠ Error reading .vdog-sdn: " + ex.Message);
//////////////                }
//////////////            }

//////////////            // --------------------------------------------------------
//////////////            // 2️⃣ SECOND: README.txt (ONLY IF .vdog-sdn NOT PRESENT)
//////////////            // --------------------------------------------------------
//////////////            string readmePath = Path.Combine(projectRoot, "README.txt");

//////////////            if (File.Exists(readmePath))
//////////////            {
//////////////                foreach (var line in File.ReadAllLines(readmePath))
//////////////                {
//////////////                    if (line.StartsWith("DIRR=", StringComparison.OrdinalIgnoreCase))
//////////////                        dirR = line.Substring(5).Trim();

//////////////                    if (line.StartsWith("COMMENT=", StringComparison.OrdinalIgnoreCase))
//////////////                        comment = line.Substring(8).Trim();
//////////////                }

//////////////                if (!string.IsNullOrWhiteSpace(dirR))
//////////////                {
//////////////                    Console.WriteLine("➡ DIRR from README = " + dirR);
//////////////                    Console.WriteLine("➡ COMMENT from README = " + comment);
//////////////                    return (dirR, comment);
//////////////                }
//////////////            }

//////////////            // --------------------------------------------------------
//////////////            // 3️⃣ LAST FALLBACK: Folder name logic
//////////////            // --------------------------------------------------------
//////////////            string folderName = Path.GetFileName(projectRoot);
//////////////            dirR = "\\" + folderName.Replace("+", "\\");

//////////////            Console.WriteLine("➡ DIRR fallback (folder name) = " + dirR);

//////////////            return (dirR, comment);
//////////////        }

//////////////        // ============================================================
//////////////        // CHECK-IN (INI BASED)
//////////////        // ============================================================
//////////////        static void RunCheckIn(string dirR, string comment)
//////////////        {
//////////////            string ini =
//////////////    $@"[User]
//////////////Account=
//////////////Domain=wipropari
//////////////Password=

//////////////[Component]
//////////////Dir={dirR}

//////////////[Version]
//////////////Enabled=Y
//////////////WithoutComparison=N
//////////////Comment={comment}
//////////////ChangeReason={comment}";

//////////////            string iniPath = Path.Combine(
//////////////                Directory.GetCurrentDirectory(),
//////////////                "AutoCheckIn.ini"
//////////////            );

//////////////            File.WriteAllText(iniPath, ini, Encoding.UTF8);

//////////////            RunCmd(
//////////////                @"C:\Program Files (x86)\vdogClient\VDogAutoCheckIn.exe",
//////////////                $"/AT:C /RD:C:\\vdClientArchive /CFile:\"{iniPath}\""
//////////////            );
//////////////        }
//// ============================================================
//// DIRR + COMMENT RESOLUTION
//// ============================================================
//static (string dirR, string comment) ResolveDirR(string projectRoot)
//{
//    string dirR = "";
//    string comment = "Auto Check-in";

//    // README.txt
//    string readme = Path.Combine(projectRoot, "README.txt");
//    if (File.Exists(readme))
//    {
//        foreach (var line in File.ReadAllLines(readme))
//        {
//            if (line.StartsWith("DIRR=", StringComparison.OrdinalIgnoreCase))
//                dirR = line.Substring(5).Trim();

//            if (line.StartsWith("COMMENT=", StringComparison.OrdinalIgnoreCase))
//                comment = line.Substring(8).Trim();
//        }
//    }

//    // vdog-sdn
//    if (string.IsNullOrWhiteSpace(dirR))
//    {
//        var vdog = Directory.GetFiles(projectRoot, "*.vdog-sdn").FirstOrDefault();
//        if (vdog != null)
//        {
//            XDocument xml = XDocument.Load(vdog);
//            dirR = xml.Descendants("DirR").FirstOrDefault()?.Value;
//        }
//    }

//    // Folder fallback
//    if (string.IsNullOrWhiteSpace(dirR))
//    {
//        string folderName = Path.GetFileName(projectRoot);
//        dirR = "\\" + folderName.Replace("+", "\\");
//    }

//    Console.WriteLine($"➡ FINAL DIRR = {dirR}");
//    Console.WriteLine($"➡ COMMENT   = {comment}");

//    return (dirR, comment);
//}
// ============================================================
// DIRR + COMMENT RESOLUTION (FINAL AS PER REQUIREMENT)
// ============================================================


// ============================================================
// ONEDRIVE DOWNLOAD
// ============================================================
//static void DownloadOneDriveZip(string url, string downloadPath)
//{

//    var options = new EdgeOptions();

//     Headless mode
//    options.AddArgument("--headless=new");   // Edge 109+
//    options.AddArgument("--disable-gpu");
//    options.AddArgument("--window-size=1920,1080");

//     Downloads
//    options.AddUserProfilePreference("download.prompt_for_download", false);
//    options.AddUserProfilePreference("download.default_directory", downloadDir);
//    options.AddUserProfilePreference("profile.default_content_settings.popups", 0);
//    options.AddUserProfilePreference("safebrowsing.enabled", true);
//    var service = EdgeDriverService.CreateDefaultService();
//    service.HideCommandPromptWindow = true;
//    service.SuppressInitialDiagnosticInformation = true;

//    using (var driver = new EdgeDriver(service, options))
//    {

//        driver.Navigate().GoToUrl(url);

//         MUST wait longer in headless
//        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(90));

//        var downloadBtn = wait.Until(d =>
//            d.FindElements(By.CssSelector("button[data-id='download']")).FirstOrDefault()
//            ?? d.FindElements(By.CssSelector("i[data-icon-name='Download']")).FirstOrDefault()
//        );

//        if (downloadBtn == null)
//            throw new Exception("Download button not found (headless)");

//        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", downloadBtn);

//        WaitForDownload(downloadDir, TimeSpan.FromMinutes(10));


//    }
//}

// ============================================================
// HELPERS
// ============================================================


// ============================================================
//static void CreateOneDriveCheckInDoneFolder()
//{
//    string folderName = $"CheckIn Done_{DateTime.Now:yyyyMMdd_HHmmss}";
//    string target = Path.Combine(extractRoot, folderName);

//    Directory.CreateDirectory(target);

//    foreach (var dir in Directory.GetDirectories(extractRoot))
//    {
//        if (dir.Contains("CheckIn Done")) continue;

//        Directory.Move(dir, Path.Combine(target, Path.GetFileName(dir)));
//    }
//}
// ============================================================
// FINAL BACKUP TO ONEDRIVE (AFTER ALL CHECKINS)
// ============================================================
//static void MoveAllCheckedInProjectsToOneDriveBackup()
//{
//    try
//    {
//        if (!Directory.Exists(extractRoot))
//            return;

//        Directory.CreateDirectory(oneDriveBackupRoot);

//        string doneFolderName =
//            $"CheckIn Done_{DateTime.Now:yyyyMMdd_HHmmss}";

//        string doneFolderPath =
//            Path.Combine(oneDriveBackupRoot, doneFolderName);

//        Directory.CreateDirectory(doneFolderPath);

//        foreach (var projectDir in Directory.GetDirectories(extractRoot))
//        {
//            string targetPath =
//                Path.Combine(doneFolderPath, Path.GetFileName(projectDir));

//            // Safety: avoid overwrite
//            if (Directory.Exists(targetPath))
//            {
//                targetPath += "_" + Guid.NewGuid().ToString("N").Substring(0, 6);
//            }

//            Directory.Move(projectDir, targetPath);
//        }
//    }
//    catch (Exception ex)
//    {
//        File.AppendAllText(
//            @"C:\Temp\OneDriveBackupError.log",
//            ex + Environment.NewLine);
//    }
//}
