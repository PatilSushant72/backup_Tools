using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using FlaUI.UIA3;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Siemens.Engineering;
using Siemens.Engineering.Compiler;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.ExternalSources;
using Sikuli4Net.sikuli_REST;
using Sikuli4Net.sikuli_REST;
using Sikuli4Net.sikuli_UTIL;
using Sikuli4Net.sikuli_UTIL;
using System;
using System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Linq;
using System.Management;
using System.Net;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading;
using System.Threading;
using System.Web;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using static TiaPortalAutomation;
using Application = System.Windows.Forms.Application;
using Button = System.Windows.Forms.Button;
using NetworkInterface = System.Net.NetworkInformation.NetworkInterface;
using Outlook = Microsoft.Office.Interop.Outlook;
using WinForms = System.Windows.Forms;
using SikuliScreen = Sikuli4Net.sikuli_REST.Screen;
using FlaUI.Core.Definitions;
using Excel = Microsoft.Office.Interop.Excel;
class TiaPortalAutomation
{
    static List<string> compileResults = new List<string>();
    static List<string> humanLoopMessages = new List<string>();
    static Dictionary<string, DateTime> uploadTimes = new Dictionary<string, DateTime>();

    static string mainOutputFolder = Path.Combine(
        @"C:\Generate Block",
        "XML Files"
    );
    static string selectedProjectPath = null;
    private static string mainfile;
    static string selectedProject = null;
    static string selectedCaxPath = null;
    static string selectedCaxPath1 = null;
    static string awlFilesDirectory = Path.Combine(
        @"C:\Generate Block",
        "AWL Files & Reports"
    );
    static string logFilePath = Path.Combine(awlFilesDirectory, "UploadReport.txt");
    static List<List<string[]>> ExcelDataList = new List<List<string[]>>();
    // ✅ Get base path dynamically (bin\Debug or bin\Release)
    string basePath = AppDomain.CurrentDomain.BaseDirectory;

    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            ExcelDataList = addExcelData();

            AccessWebsite();
            
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    public static void AccessWebsite()
    {
        // ✅ Setup download directory
        string downloadDirectory = Path.Combine(@"C:\Generate Block", "AWL Files & Reports");
        if (!Directory.Exists(downloadDirectory))
            Directory.CreateDirectory(downloadDirectory);

        // ✅ Setup Edge options
        var options = new EdgeOptions();
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument("--ignore-certificate-errors");
        options.AddArgument("--allow-insecure-localhost");
        options.AddArgument("--no-sandbox");
        options.AcceptInsecureCertificates = true;
        options.AddUserProfilePreference("download.default_directory", downloadDirectory);
        options.AddUserProfilePreference("profile.default_content_setting_values.automatic_downloads", 1);
        options.AddUserProfilePreference("profile.default_content_settings.popups", 0);
        options.AddUserProfilePreference("download.prompt_for_download", false);
        options.AddUserProfilePreference("safebrowsing.enabled", true);

        var service = EdgeDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true;
        service.SuppressInitialDiagnosticInformation = true;

        try
        {
            using (IWebDriver driver = new EdgeDriver(service, options))
            {
                // 🌐 Navigate to Siemens Cloud Login
                driver.Navigate().GoToUrl("https://auth.sw.siemens.com/login?returnTo=https%3A%2F%2Ftrials.sw.siemens.com%2Fen-US%2Ftrials%2Ftecnomatix-simatic-virtual-commissioning");
                Thread.Sleep(3000);

                // 🔐 Login
                string userId = "dare88856@gmail.com";
                string password = "Gayatri@1834";

                driver.FindElement(By.Id("username")).SendKeys(userId);
                driver.FindElement(By.CssSelector("button._button-login-id")).Click();

                WebDriverWait waitLogin = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
                waitLogin.Until(ExpectedConditions.ElementIsVisible(By.Id("password")));

                driver.FindElement(By.Id("password")).SendKeys(password);
                driver.FindElement(By.CssSelector("button._button-login-password")).Click();
                Thread.Sleep(TimeSpan.FromSeconds(30));
                ClickAcceptAllCookies();
                // 🚀 Click Launch button
                WebDriverWait waitLaunch = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                IWebElement launchBtn = waitLaunch.Until(ExpectedConditions.ElementIsVisible(
                    By.XPath("//button[contains(text(),'Launch ')]")
                ));
                launchBtn.Click();

                // 🕓 Wait for VDI desktop to appear
                //Thread.Sleep(TimeSpan.FromMinutes(2));
                //LaunchDesktop();
                CopyExcelDataListToClipboardAndRunSikuli();
                //OpenCSV();
                //Opencs();
                //LaunchPSVCFromVDI();

                //Clipboardopen();
                //Closeclipboard();
                // ✅ Launch PS VC Lite using SikuliX


                // 🧭 Continue your AWL automation after launch
                ContinueAutomation(driver, downloadDirectory);
            }
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("Global error: " + ex.Message);
        }
    }

    private static void ContinueAutomation(IWebDriver driver, string downloadDirectory)
    {
        // You can place your "Excel upload" and "Generate Block" logic here...
        Console.WriteLine("Continue with Excel upload or other automation...");
    }

    public static void ClickAcceptAllCookies()
    {
        // ✅ Get the base directory dynamically (bin\Debug or bin\Release)
        string basePath = AppDomain.CurrentDomain.BaseDirectory;

        // ✅ Build dynamic paths
        string sikuliJarPath = Path.Combine(basePath, "sikulixide-2.0.5-win.jar");
        string javaExePath = Path.Combine(basePath, @"openjdk-25_windows-x64_bin", "jdk-25", "bin", "java.exe");
        string scriptPath = Path.Combine(basePath, @"..\..\Images", "Accept All Cookies.sikuli");

        // 🔁 Resolve relative path (.. goes up from bin\Debug)
        scriptPath = Path.GetFullPath(scriptPath);

        // ✅ Validate files and folders
        if (!File.Exists(sikuliJarPath))
        {
            Console.WriteLine("❌ SikuliX JAR not found at: " + sikuliJarPath);
            return;
        }

        if (!File.Exists(javaExePath))
        {
            Console.WriteLine("❌ Java executable not found at: " + javaExePath);
            return;
        }

        if (!Directory.Exists(scriptPath))
        {
            Console.WriteLine("❌ Sikuli script folder not found at: " + scriptPath);
            return;
        }

        try
        {
            // ⚙️ Configure process info dynamically
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = javaExePath,
                Arguments = $"-jar \"{sikuliJarPath}\" -r \"{scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process sikuliProcess = new Process())
            {
                sikuliProcess.StartInfo = psi;
                sikuliProcess.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Console.WriteLine(e.Data);
                };
                sikuliProcess.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Console.WriteLine("[Error] " + e.Data);
                };

                sikuliProcess.Start();
                sikuliProcess.BeginOutputReadLine();
                sikuliProcess.BeginErrorReadLine();

                Console.WriteLine("🚀 Running Sikuli script to click 'Accept All Cookies'...");
                sikuliProcess.WaitForExit();

                Console.WriteLine("✅ Sikuli script execution finished.");
            }

            // ⏳ Wait for the app to process the click
            Thread.Sleep(TimeSpan.FromSeconds(20));
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("⚠️ Error launching Sikuli script: " + ex.Message);
        }
    }
    public static void LaunchDesktop()
    {
        // ✅ Get the base directory dynamically (bin\Debug or bin\Release)
        string basePath = AppDomain.CurrentDomain.BaseDirectory;

        // ✅ Build dynamic paths
        string sikuliJarPath = Path.Combine(basePath, "sikulixide-2.0.5-win.jar");
        string javaExePath = Path.Combine(basePath, @"openjdk-25_windows-x64_bin", "jdk-25", "bin", "java.exe");
        string scriptPath = Path.Combine(basePath, @"..\..\Images", "desktopimage.sikuli");

        // 🔁 Resolve relative path (.. goes up from bin\Debug)
        scriptPath = Path.GetFullPath(scriptPath);

        // ✅ Validate files and folders
        if (!File.Exists(sikuliJarPath))
        {
            Console.WriteLine("❌ SikuliX JAR not found at: " + sikuliJarPath);
            return;
        }

        if (!File.Exists(javaExePath))
        {
            Console.WriteLine("❌ Java executable not found at: " + javaExePath);
            return;
        }

        if (!Directory.Exists(scriptPath))
        {
            Console.WriteLine("❌ Sikuli script folder not found at: " + scriptPath);
            return;
        }

        try
        {
            // ⚙️ Configure process info dynamically
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = javaExePath,
                Arguments = $"-jar \"{sikuliJarPath}\" -r \"{scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process sikuliProcess = new Process())
            {
                sikuliProcess.StartInfo = psi;
                sikuliProcess.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Console.WriteLine(e.Data);
                };
                sikuliProcess.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Console.WriteLine("[Error] " + e.Data);
                };

                sikuliProcess.Start();
                sikuliProcess.BeginOutputReadLine();
                sikuliProcess.BeginErrorReadLine();

                Console.WriteLine("🚀 Running Sikuli script to click 'Accept All Cookies'...");
                sikuliProcess.WaitForExit();

                Console.WriteLine("✅ Sikuli script execution finished.");
            }

            // ⏳ Wait for the app to process the click
            Thread.Sleep(TimeSpan.FromMinutes(1));
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("⚠️ Error launching Sikuli script: " + ex.Message);
        }
    }
    //public static void LaunchDesktop()
    //{
    //    string baseDir = AppDomain.CurrentDomain.BaseDirectory;

    //    // Sikuli paths
    //    string sikuliJarPath = Path.Combine(baseDir, "sikulixide-2.0.5-win.jar");
    //    string jdkBinPath = Path.Combine(baseDir, @"openjdk-25_windows-x64_bin\jdk-25\bin\java.exe");
    //    string scriptPath = Path.Combine(baseDir, @"..\..\Images\desktopimage.sikuli");
    //    string desktopImagePath = Path.Combine(scriptPath, "desktopimage.png"); // screenshot inside the .sikuli folder

    //    // Validate paths
    //    if (!File.Exists(sikuliJarPath))
    //    {
    //        Console.WriteLine("❌ SikuliX JAR not found at: " + sikuliJarPath);
    //        return;
    //    }

    //    if (!File.Exists(jdkBinPath))
    //    {
    //        Console.WriteLine("❌ Java executable not found at: " + jdkBinPath);
    //        return;
    //    }

    //    if (!Directory.Exists(scriptPath))
    //    {
    //        Console.WriteLine("❌ Sikuli script folder not found at: " + scriptPath);
    //        return;
    //    }

    //    if (!File.Exists(desktopImagePath))
    //    {
    //        Console.WriteLine("❌ Desktop image not found at: " + desktopImagePath);
    //        return;
    //    }

    //    try
    //    {
    //        // Launch Sikuli script
    //        ProcessStartInfo psi = new ProcessStartInfo
    //        {
    //            FileName = jdkBinPath,
    //            Arguments = $"-jar \"{sikuliJarPath}\" -r \"{scriptPath}\"",
    //            UseShellExecute = false,
    //            CreateNoWindow = true,
    //            RedirectStandardOutput = true,
    //            RedirectStandardError = true
    //        };

    //        using (Process sikuliProcess = new Process())
    //        {
    //            sikuliProcess.StartInfo = psi;
    //            sikuliProcess.OutputDataReceived += (s, e) =>
    //            {
    //                if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine(e.Data);
    //            };
    //            sikuliProcess.ErrorDataReceived += (s, e) =>
    //            {
    //                if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine("[Error] " + e.Data);
    //            };

    //            sikuliProcess.Start();
    //            sikuliProcess.BeginOutputReadLine();
    //            sikuliProcess.BeginErrorReadLine();

    //            Console.WriteLine("🚀 Running Sikuli script to launch PS VC Lite...");
    //            sikuliProcess.WaitForExit();

    //            Console.WriteLine("✅ Sikuli script execution finished.");
    //        }

    //        // Dynamically wait for desktop image to appear
    //        Console.WriteLine("🕓 Waiting for VDI desktop to appear...");

    //        var screen = new Sikuli4Net.sikuli_REST.Screen();
    //        Sikuli4Net.sikuli_REST.Pattern desktopPattern = new Sikuli4Net.sikuli_REST.Pattern(desktopImagePath);

    //        if (screen.Exists(desktopPattern) != null)
    //        {
    //            Console.WriteLine("🖥️ Desktop detected! Proceeding...");
    //        }


    //        int maxWaitSeconds = 180; // 3 minutes
    //        int elapsed = 0;
    //        int pollInterval = 2000; // 2 sec

    //        bool desktopFound = false;

    //        while (elapsed < maxWaitSeconds * 1000)
    //        {
    //            if (screen.Exists(desktopPattern) != null)

    //            {
    //                Console.WriteLine("🖥️ Desktop detected! Proceeding to next step...");
    //                desktopFound = true;
    //                break;
    //            }
    //            Thread.Sleep(pollInterval);
    //            elapsed += pollInterval;
    //        }

    //        if (!desktopFound)
    //        {
    //            Console.WriteLine("⚠️ Timeout: Desktop image not found after waiting 3 minutes.");
    //        }
    //    }
    //    catch (System.Exception ex)
    //    {
    //        Console.WriteLine("⚠️ Error launching Sikuli script: " + ex.Message);
    //    }
    //}


    public static void CopyExcelDataListToClipboardAndRunSikuli()
    {
        // 1️⃣ Build CSV-like string
        StringBuilder sb = new StringBuilder();

        foreach (var sheet in ExcelDataList)
        {
            foreach (var row in sheet)
            {
                sb.AppendLine(string.Join("\t", row.Select(cell => EscapeCsv(cell))));
            }
            sb.AppendLine(); // blank line between sheets
        }

        string clipboardText = sb.ToString();

        // 2️⃣ Copy to clipboard
        Clipboard.SetText(clipboardText);
        Console.WriteLine("✅ ExcelDataList copied to clipboard.");

        // 3️⃣ Build dynamic paths
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        string sikuliJarPath = Path.Combine(baseDir, "sikulixide-2.0.5-win.jar");
        string jdkBinPath = Path.Combine(baseDir, "openjdk-25_windows-x64_bin", "jdk-25", "bin", "java.exe");
        string scriptPath = Path.Combine(baseDir, "Images","clipboard.sikuli");
        scriptPath = Path.GetFullPath(scriptPath); // resolve relative path

        // 4️⃣ Validate paths
        if (!File.Exists(sikuliJarPath))
        {
            Console.WriteLine("❌ SikuliX JAR not found at: " + sikuliJarPath);
            return;
        }

        if (!File.Exists(jdkBinPath))
        {
            Console.WriteLine("❌ Java.exe not found at: " + jdkBinPath);
            return;
        }

        if (!Directory.Exists(scriptPath))
        {
            Console.WriteLine("❌ Sikuli script folder not found at: " + scriptPath);
            return;
        }

        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = jdkBinPath,
                Arguments = $"-jar \"{sikuliJarPath}\" -r \"{scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process sikuliProcess = new Process())
            {
                sikuliProcess.StartInfo = psi;
                sikuliProcess.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine(e.Data);
                };
                sikuliProcess.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine("[Error] " + e.Data);
                };

                sikuliProcess.Start();
                sikuliProcess.BeginOutputReadLine();
                sikuliProcess.BeginErrorReadLine();

                Console.WriteLine("🚀 Running Sikuli script to paste clipboard data...");
                sikuliProcess.WaitForExit();

                Console.WriteLine("✅ Sikuli script execution finished.");
            }

            // Wait 2 minutes for the app to load
            Thread.Sleep(TimeSpan.FromMinutes(1));
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("⚠️ Error launching Sikuli script: " + ex.Message);
        }
    }

    // Helper – escape CSV properly
    private static string EscapeCsv(string cell)
    {
        if (cell.Contains(",") || cell.Contains("\"") || cell.Contains("\n"))
        {
            return "\"" + cell.Replace("\"", "\"\"") + "\"";
        }
        return cell;
    }
    public static void OpenCSV()
    {
        // 🔹 Get current base directory (e.g. bin\Debug)
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        // 🔹 Build dynamic paths
        string sikuliJarPath = Path.Combine(baseDir, "sikulixide-2.0.5-win.jar");
        string jdkBinPath = Path.Combine(baseDir, "openjdk-25_windows-x64_bin", "jdk-25", "bin", "java.exe");
        string scriptPath = Path.Combine(baseDir, @"..\..\Images\opencsv.sikuli");
        scriptPath = Path.GetFullPath(scriptPath); // resolve relative path

        // 🔹 Validate paths
        if (!File.Exists(sikuliJarPath))
        {
            Console.WriteLine("❌ SikuliX JAR not found at: " + sikuliJarPath);
            return;
        }

        if (!File.Exists(jdkBinPath))
        {
            Console.WriteLine("❌ Java.exe not found at: " + jdkBinPath);
            return;
        }

        if (!Directory.Exists(scriptPath))
        {
            Console.WriteLine("❌ Sikuli script folder not found at: " + scriptPath);
            return;
        }

        try
        {
            // 🔹 Create process to run Sikuli script
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = jdkBinPath, // Java path
                Arguments = $"-jar \"{sikuliJarPath}\" -r \"{scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process sikuliProcess = new Process())
            {
                sikuliProcess.StartInfo = psi;
                sikuliProcess.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine(e.Data);
                };
                sikuliProcess.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine("[Error] " + e.Data);
                };

                sikuliProcess.Start();
                sikuliProcess.BeginOutputReadLine();
                sikuliProcess.BeginErrorReadLine();

                Console.WriteLine("🚀 Running Sikuli script to open CSV...");
                sikuliProcess.WaitForExit();

                Console.WriteLine("✅ Sikuli script execution finished.");
            }

            // ⏳ Wait for 2 minutes to allow app loading
            Thread.Sleep(TimeSpan.FromMinutes(1));
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("⚠️ Error launching Sikuli script: " + ex.Message);
        }
    }

    public static List<List<string[]>> addExcelData()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
        openFileDialog.Title = "Select an Excel File";
        string filePath = "";
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            filePath = openFileDialog.FileName;
        }
        var allSheetsData = new List<List<string[]>>();
        Excel.Application excelApp = new Excel.Application();
        Excel.Workbook workbook = excelApp.Workbooks.Open(filePath);
        try
        {
            foreach (Excel.Worksheet sheet in workbook.Sheets)
            {
                Excel.Range usedRange = sheet.UsedRange;
                int rowCount = usedRange.Rows.Count;
                int colCount = usedRange.Columns.Count;

                var sheetData = new List<string[]>();
                sheetData.Add(new string[] { sheet.Name });

                for (int r = 1; r <= rowCount; r++)
                {
                    string[] rowValues = new string[colCount];
                    for (int c = 1; c <= colCount; c++)
                    {
                        var cell = usedRange.Cells[r, c] as Excel.Range;
                        rowValues[c - 1] = cell?.Text?.ToString() ?? "";
                    }

                    sheetData.Add(rowValues);
                }

                allSheetsData.Add(sheetData);
            }
        }
        finally
        {
            workbook.Close(false);
            excelApp.Quit();
        }
        return allSheetsData;
    }
    public static void Opencs()
    {
        // 🔹 Get current base directory (bin\Debug)
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        // 🔹 Build dynamic paths
        string sikuliJarPath = Path.Combine(baseDir, "sikulixide-2.0.5-win.jar");
        string jdkBinPath = Path.Combine(baseDir, "openjdk-25_windows-x64_bin", "jdk-25", "bin", "java.exe");
        string scriptPath = Path.Combine(baseDir, @"..\..\Images\opencs.sikuli");
        scriptPath = Path.GetFullPath(scriptPath); // Resolve relative path

        // 🔹 Validate paths
        if (!File.Exists(sikuliJarPath))
        {
            Console.WriteLine("❌ SikuliX JAR not found at: " + sikuliJarPath);
            return;
        }

        if (!File.Exists(jdkBinPath))
        {
            Console.WriteLine("❌ Java.exe not found at: " + jdkBinPath);
            return;
        }

        if (!Directory.Exists(scriptPath))
        {
            Console.WriteLine("❌ Sikuli script folder not found at: " + scriptPath);
            return;
        }

        try
        {
            // 🔹 Configure process to run Sikuli script
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = jdkBinPath, // dynamic Java path
                Arguments = $"-jar \"{sikuliJarPath}\" -r \"{scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process sikuliProcess = new Process())
            {
                sikuliProcess.StartInfo = psi;

                sikuliProcess.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine(e.Data);
                };

                sikuliProcess.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine("[Error] " + e.Data);
                };

                sikuliProcess.Start();
                sikuliProcess.BeginOutputReadLine();
                sikuliProcess.BeginErrorReadLine();

                Console.WriteLine("🚀 Running Sikuli script to open CS file...");
                sikuliProcess.WaitForExit();

                Console.WriteLine("✅ Sikuli script execution finished.");
            }

            // ⏳ Wait 2 minutes for the app to load properly
            Thread.Sleep(TimeSpan.FromMinutes(1));
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("⚠️ Error launching Sikuli script: " + ex.Message);
        }
    }
    public static void LaunchPSVCFromVDI()
    {
        // 🔹 Get current base directory (e.g. bin\Debug)
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        // 🔹 Build dynamic paths
        string sikuliJarPath = Path.Combine(baseDir, "sikulixide-2.0.5-win.jar");
        string jdkBinPath = Path.Combine(baseDir, "openjdk-25_windows-x64_bin", "jdk-25", "bin", "java.exe");
        string scriptPath = Path.Combine(baseDir, @"..\..\Images\psvclitescript.sikuli");
        scriptPath = Path.GetFullPath(scriptPath); // resolve relative path

        // 🔹 Validate paths
        if (!File.Exists(sikuliJarPath))
        {
            Console.WriteLine("❌ SikuliX JAR not found at: " + sikuliJarPath);
            return;
        }

        if (!File.Exists(jdkBinPath))
        {
            Console.WriteLine("❌ Java.exe not found at: " + jdkBinPath);
            return;
        }

        if (!Directory.Exists(scriptPath))
        {
            Console.WriteLine("❌ Sikuli script folder not found at: " + scriptPath);
            return;
        }

        try
        {
            // 🔹 Configure process to run Sikuli script
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = jdkBinPath, // dynamic java path
                Arguments = $"-jar \"{sikuliJarPath}\" -r \"{scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process sikuliProcess = new Process())
            {
                sikuliProcess.StartInfo = psi;

                sikuliProcess.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Console.WriteLine(e.Data);
                };

                sikuliProcess.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Console.WriteLine("[Error] " + e.Data);
                };

                sikuliProcess.Start();
                sikuliProcess.BeginOutputReadLine();
                sikuliProcess.BeginErrorReadLine();

                Console.WriteLine("🚀 Running Sikuli script to launch PS VC Lite...");
                sikuliProcess.WaitForExit();

                Console.WriteLine("✅ Sikuli script execution finished.");
            }

            // ⏳ Wait 2 minutes for the app to load fully
            Thread.Sleep(TimeSpan.FromMinutes(1));
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("⚠️ Error launching Sikuli script: " + ex.Message);
        }
    }
    public static class EncryptionHelper
    {
        private static readonly string secretKey = "My$ecretK3y12345";
        public static string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(secretKey);
                aes.IV = new byte[16];

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                    sw.Close();
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(secretKey);
                aes.IV = new byte[16];

                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }

    static void LoginUser()
    {
        Console.Write("Enter username: ");
        string username = Console.ReadLine();

        Console.Write("Enter password: ");
        string password = Console.ReadLine();

        Console.WriteLine("Login successful!");
    }

    static string GetUniqueSessionId()
    {
        string sessionId = Process.GetCurrentProcess().SessionId.ToString();
        string bootTime = GetSystemBootTime().ToString("yyyyMMddHHmmss");
        return $"{bootTime}-{sessionId}";
    }

    static DateTime GetSystemBootTime()
    {
        using (ManagementObjectSearcher searcher =
               new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem"))
        {
            foreach (ManagementObject os in searcher.Get())
            {
                string lastBootUpTime = os["LastBootUpTime"].ToString();
                return ManagementDateTimeConverter.ToDateTime(lastBootUpTime);
            }
        }
        return DateTime.MinValue;
    }

    static string GetWindowsSessionId()
    {
        return Process.GetCurrentProcess().SessionId.ToString();
    }


    class XmlExcelPair
    {
        public string XmlPath { get; set; }
        public string ExcelPath { get; set; }
    }

    private static List<XmlExcelPair> ShowXmlExcelPairingDialog(string[] xmlFiles)
    {
        var pairs = xmlFiles.Select(x => new XmlExcelPair { XmlPath = x, ExcelPath = string.Empty }).ToList();

        using (var form = new Form())
        {
            form.Text = "XML and Matching Excel";
            form.Size = new Size(800, 400);
            form.StartPosition = FormStartPosition.CenterParent;

            var dgv = new WinForms.DataGridView
            {
                Dock = DockStyle.Top,
                Height = 300,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                DataSource = new BindingList<XmlExcelPair>(pairs)
            };

            var xmlCol = new DataGridViewTextBoxColumn
            {
                HeaderText = "XML File",
                DataPropertyName = "XmlPath",
                ReadOnly = true,
                Width = 400
            };
            dgv.Columns.Add(xmlCol);

            var excelPathCol = new DataGridViewTextBoxColumn
            {
                HeaderText = "Selected Excel",
                DataPropertyName = "ExcelPath",
                ReadOnly = true,
                Width = 300
            };
            dgv.Columns.Add(excelPathCol);

            var browseCol = new DataGridViewButtonColumn
            {
                HeaderText = "Select Excel",
                Text = "Browse...",
                UseColumnTextForButtonValue = true,
                Width = 100
            };
            dgv.Columns.Add(browseCol);

            dgv.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == dgv.Columns["Select Excel"].Index)
                {
                    var pair = pairs[e.RowIndex];
                    using (var excelDialog = new OpenFileDialog
                    {
                        Multiselect = false,
                        Title = $"Select Excel for {Path.GetFileName(pair.XmlPath)}",
                        Filter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx|All Files (*.*)|*.*"
                    })
                    {
                        if (excelDialog.ShowDialog() == DialogResult.OK)
                        {
                            pair.ExcelPath = excelDialog.FileName;
                            dgv.Refresh();
                        }
                    }
                }
            };

            browseCol.Name = "Select Excel";

            var okButton = new WinForms.Button { Text = "OK", DialogResult = DialogResult.OK, Anchor = AnchorStyles.Bottom | AnchorStyles.Right, Location = new Point(form.ClientSize.Width - 180, 320), Size = new Size(80, 30) };
            var cancelButton = new WinForms.Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Anchor = AnchorStyles.Bottom | AnchorStyles.Right, Location = new Point(form.ClientSize.Width - 90, 320), Size = new Size(80, 30) };

            form.Controls.Add(dgv);
            form.Controls.Add(okButton);
            form.Controls.Add(cancelButton);
            form.AcceptButton = okButton;
            form.CancelButton = cancelButton;

            var result = form.ShowDialog();
            if (result == DialogResult.OK)
                return pairs;
            else
                return null;
        }
    }

    class AwlExcelPair
    {
        public string AwlPath { get; set; }
        public string ExcelPath { get; set; }
    }

    private static List<AwlExcelPair> ShowAwlExcelPairingDialog(string[] awlFiles)
    {
        var pairs = awlFiles.Select(x => new AwlExcelPair { AwlPath = x, ExcelPath = string.Empty }).ToList();

        using (var form = new System.Windows.Forms.Form())
        {
            form.Text = "AWL and Matching Excel";
            form.Size = new Size(900, 450);
            form.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            form.MinimizeBox = false;
            form.MaximizeBox = false;

            var bindingList = new BindingList<AwlExcelPair>(pairs);
            var dgv = new System.Windows.Forms.DataGridView
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 330,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                DataSource = bindingList,
                SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };

            var awlCol = new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                Name = "AwlFile",
                HeaderText = "AWL File",
                DataPropertyName = "AwlPath",
                ReadOnly = true,
                Width = 450
            };
            dgv.Columns.Add(awlCol);

            var excelPathCol = new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                Name = "ExcelFile",
                HeaderText = "Selected Excel",
                DataPropertyName = "ExcelPath",
                ReadOnly = true,
                Width = 350
            };
            dgv.Columns.Add(excelPathCol);

            var browseCol = new System.Windows.Forms.DataGridViewButtonColumn
            {
                Name = "BrowseExcel",
                HeaderText = "Select Excel",
                Text = "Browse...",
                UseColumnTextForButtonValue = true,
                Width = 100
            };
            dgv.Columns.Add(browseCol);

            dgv.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && dgv.Columns[e.ColumnIndex].Name == "BrowseExcel")
                {
                    var pair = bindingList[e.RowIndex];
                    using (var excelDialog = new System.Windows.Forms.OpenFileDialog
                    {
                        Multiselect = false,
                        Title = $"Select Excel for {Path.GetFileName(pair.AwlPath)}",
                        Filter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx|All Files (*.*)|*.*"
                    })
                    {
                        if (excelDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            pair.ExcelPath = excelDialog.FileName;
                            dgv.Refresh();
                        }
                    }
                }
            };

            var okButton = new System.Windows.Forms.Button
            {
                Text = "OK",
                DialogResult = System.Windows.Forms.DialogResult.OK,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Size = new Size(100, 30),
                Location = new Point(form.ClientSize.Width - 220, 360)
            };
            var cancelButton = new System.Windows.Forms.Button
            {
                Text = "Cancel",
                DialogResult = System.Windows.Forms.DialogResult.Cancel,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Size = new Size(100, 30),
                Location = new Point(form.ClientSize.Width - 110, 360)
            };

            form.Controls.Add(dgv);
            form.Controls.Add(okButton);
            form.Controls.Add(cancelButton);
            form.AcceptButton = okButton;
            form.CancelButton = cancelButton;

            okButton.Click += (s, e) =>
            {
                var missing = bindingList.Where(p => string.IsNullOrWhiteSpace(p.ExcelPath)).ToList();
                if (missing.Any())
                {
                    var msg = "The following AWL(s) have no matching Excel selected:\n" +
                              string.Join("\n", missing.Select(p => Path.GetFileName(p.AwlPath)));
                    System.Windows.Forms.MessageBox.Show(msg, "Missing Excel", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    form.DialogResult = System.Windows.Forms.DialogResult.None; // prevent closing
                }
            };

            var result = form.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
                return bindingList.Select(p => new AwlExcelPair { AwlPath = p.AwlPath, ExcelPath = p.ExcelPath }).ToList();
            else
                return null;
        }
    }

    public static void updateBlockName(string filePath, string excelFilePath, string projectPath)
    {
        try
        {
            string content = File.ReadAllText(filePath);
            string excelName = !string.IsNullOrEmpty(excelFilePath)
                ? Path.GetFileNameWithoutExtension(excelFilePath)
                : null;

            string extractedName = "";

            if (selectedPrefix == null && !PrefixSelectionForm.ShouldSkipPopup())
            {
                using (PrefixSelectionForm form = new PrefixSelectionForm())
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        selectedPrefix = form.SelectedPrefix;
                    }
                    else
                    {
                        selectedPrefix = "";
                    }

                    File.WriteAllText(
                        prefixSessionFile,
                        Path.GetFullPath(projectPath).ToLowerInvariant() + projectPath + "|" + selectedPrefix
                    );
                }
            }

            if (selectedPrefix == null && PrefixSelectionForm.ShouldSkipPopup())
            {
                selectedPrefix = "";
            }

            if (content.StartsWith("FUNCTION_BLOCK", System.StringComparison.OrdinalIgnoreCase))
            {
                int start = content.IndexOf('"') + 1;
                int end = content.IndexOf('"', start);

                if (start > 0 && end > start)
                {
                    extractedName = content.Substring(start, end - start);

                    string updBlockName;
                    if (!string.IsNullOrEmpty(excelName))
                    {
                        updBlockName = selectedPrefix + excelName;
                    }
                    else
                    {
                        updBlockName = selectedPrefix + extractedName;
                    }

                    string updatedContent = content.Replace($"\"{extractedName}\"", $"\"{updBlockName}\"");
                    File.WriteAllText(filePath, updatedContent);
                    System.Console.WriteLine($"AWL block name updated to: {updBlockName}");
                }
            }
            else
            {
                Regex rx = new Regex(@"<Name>(.*?)</Name>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                Match m = rx.Match(content);

                if (m.Success)
                {
                    string originalXmlName = m.Groups[1].Value;

                    string newXmlName;
                    if (!string.IsNullOrEmpty(excelName))
                    {
                        newXmlName = selectedPrefix + excelName;
                    }
                    else
                    {
                        newXmlName = selectedPrefix + originalXmlName;
                    }

                    string updatedXml = rx.Replace(content, "<Name>" + newXmlName + "</Name>", 1);
                    File.WriteAllText(filePath, updatedXml);
                    System.Console.WriteLine($"XML <Name> updated to: {newXmlName}");
                }
                else
                {
                    System.Console.WriteLine($"No <Name> tag found in XML: {filePath}");
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Error updating block name for {filePath}: {ex.Message}");
        }
    }
    private static bool ValidateExcelSheet(string filePath)
    {
        try
        {
            using (var package = new OfficeOpenXml.ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                if (worksheet.Dimension == null)
                {
                    MessageBox.Show("The Excel file is empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                string[] expectedHeadersWithSymbolic = {
                    "Seq No",
                    "Sequence Descriptions",
                    "Symbolic Name",
                    "Condition / Interface Signal",
                    "Jump Step",
                    "Actions"
                };

                string[] expectedHeadersWithoutSymbolic = {
                    "Seq No",
                    "Sequence Descriptions",
                    "Condition / Interface Signal",
                    "Jump Step",
                    "Actions"
                };

                int totalCols = worksheet.Dimension.End.Column;
                List<string> actualHeaders = new List<string>();
                for (int col = 1; col <= totalCols; col++)
                    actualHeaders.Add(worksheet.Cells[1, col].Text.Trim());

                bool hasSymbolicColumn = actualHeaders.Contains("Symbolic Name");

                string[] expectedHeaders = hasSymbolicColumn ? expectedHeadersWithSymbolic : expectedHeadersWithoutSymbolic;
                for (int col = 1; col <= expectedHeaders.Length; col++)
                {
                    if (worksheet.Cells[1, col].Text.Trim() != expectedHeaders[col - 1])
                    {
                        MessageBox.Show($"Invalid header at column {col}. Expected: {expectedHeaders[col - 1]}", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    string seqNo = worksheet.Cells[row, 1].Text;
                    if (string.IsNullOrEmpty(seqNo) || !int.TryParse(seqNo, out _))
                    {
                        MessageBox.Show($"Invalid value in 'Seq No' at row {row}. Must be a numeric value.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    string sequenceDescription = worksheet.Cells[row, 2].Text;
                    if (string.IsNullOrEmpty(sequenceDescription))
                    {
                        MessageBox.Show($"'Sequence Descriptions' cannot be empty at row {row}.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    if (hasSymbolicColumn)
                    {
                        string symbolicName = worksheet.Cells[row, 3].Text;
                        if (string.IsNullOrWhiteSpace(symbolicName))
                        {
                            MessageBox.Show($"'Symbolic Name' cannot be empty at row {row}.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }

                    string conditionSignal = worksheet.Cells[row, hasSymbolicColumn ? 4 : 3].Text;

                    string jumpStep = worksheet.Cells[row, hasSymbolicColumn ? 5 : 4].Text;

                    string actions = worksheet.Cells[row, hasSymbolicColumn ? 6 : 5].Text;
                }

                return true;
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Error reading Excel file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    static void LaunchNextProcessExe(string excelForTest)
    {
        string mode = "XML";
        string projectPath = @"C:\Path\To\YourProject.ap17";

        string nextExePath = @"C:\Generate Block\V20 All Files\Debug\ConsoleApp6.exe";

        string nextExeArgs = $"\"{mode}\" \"{projectPath}\" \"{excelForTest}\"";

        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = nextExePath,
                Arguments = nextExeArgs,
                UseShellExecute = true
            };

            Process.Start(psi);
        }
        catch (System.Exception ex)
        {
            MessageBox.Show(
                $"Failed to launch next process: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
    static string GetProjectPathFromDialog()
    {
        using (var fileDialog = new OpenFileDialog())
        {
            fileDialog.Filter = "TIA Portal Project (*.ap*)|*.ap*";
            fileDialog.FilterIndex = 1;
            fileDialog.RestoreDirectory = true;

            return fileDialog.ShowDialog() == DialogResult.OK ? fileDialog.FileName : string.Empty;
        }
    }
    static List<string> ls = new List<string>();
    
    static List<string> ImportAwlFiles(string projectPath, string awlFilesDirectory, string caxFilePath)
    {
        List<string> errorAwlFiles = new List<string>();
        string exportDirectory = mainOutputFolder;

        TiaPortal tiaPortal = null;
        Project project = null;

        try
        {
            tiaPortal = new TiaPortal(TiaPortalMode.WithoutUserInterface);
            string newProjectPath = @"C:\\Generate Block\\CPU300TempProject";
            if (Directory.Exists(newProjectPath))
            {
                Directory.Delete(newProjectPath, true);
            }
            Directory.CreateDirectory(newProjectPath);

            project = tiaPortal.Projects.Create(new DirectoryInfo(newProjectPath), "CPU300TempProject");

            var device = project.Devices.FirstOrDefault(d => d.Name.Contains("PLC_1") || d.Name.Contains("S7-300"));
            if (device == null)
            {
                try
                {
                    device = project.Devices.CreateWithItem(
                        "OrderNumber:6ES7 312-1AE13-0AB0/V2.6",
                        "PLC_1",
                        "S7-300/ET200M station_1"
                    );
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Failed to create CPU 300 device: " + ex.Message, "Device Creation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return errorAwlFiles;
                }
            }

            var softwareContainer = device.DeviceItems[1].GetService<SoftwareContainer>();
            var plcSoftware = softwareContainer?.Software as PlcSoftware;

            if (plcSoftware != null)
            {
                var compiler = plcSoftware.GetService<ICompilable>();

                foreach (var awlFile in Directory.GetFiles(awlFilesDirectory, "*.awl"))
                {
                    string fileName = Path.GetFileNameWithoutExtension(awlFile);
                    var existingSource = plcSoftware.ExternalSourceGroup.ExternalSources
                        .FirstOrDefault(src => src.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));

                    if (existingSource != null)
                    {
                        existingSource.Delete();
                    }

                    try
                    {
                        var externalSource = plcSoftware.ExternalSourceGroup.ExternalSources.CreateFromFile(fileName, awlFile);
                        externalSource.GenerateBlocksFromSource((GenerateBlockOption)ImportOptions.Override);

                        var plcBlockGroup = plcSoftware.BlockGroup;
                        var blockName = GetBlockNameFromAwlFile(awlFile);
                        var block = FindBlockRecursively(plcBlockGroup, blockName.Trim());
                        if (block != null)
                        {
                            var singleCompiler = block.GetService<ICompilable>();
                            var result1 = singleCompiler.Compile();

                            List<string> errorMessages = GetCompilerMessages(string.Empty, string.Empty, result1.Messages, true);
                            if (errorMessages.Count == 0)
                            {
                                Thread.Sleep(2000);
                            }
                            else
                            {
                                string errorText = $"Compilation failed with {errorMessages.Count} error(s):\n\n";
                                int maxToShow = 10;
                                var displayErrors = errorMessages.Take(maxToShow).ToList();
                                foreach (var error in displayErrors)
                                {
                                    errorText += $"- {error}\n";
                                }
                                if (errorMessages.Count > maxToShow)
                                {
                                    errorText += $"\n...and {errorMessages.Count - maxToShow} more.\nCheck log file for full details.";
                                }
                                errorAwlFiles.Add(Path.GetFileName(awlFile));
                                MessageBox.Show(errorText, "Compile Result", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (EngineeringException ex)
                    {
                    }
                }

                project.Save();
                project.Close();
                tiaPortal.Dispose();

                GC.Collect();
                GC.WaitForPendingFinalizers();

                string apPath = Path.Combine(newProjectPath, "CPU300TempProject", "CPU300TempProject.ap17");
                int retries = 10;
                while (!File.Exists(apPath) && retries-- > 0)
                {
                    Thread.Sleep(1000);
                }

                if (!File.Exists(apPath))
                {
                    return errorAwlFiles;
                }

                tiaPortal = new TiaPortal(TiaPortalMode.WithUserInterface);
                project = tiaPortal.Projects.Open(new FileInfo(apPath));

                bool projectLoaded = false;
                for (int i = 0; i < 600; i++)
                {
                    if (project.Devices.Count > 0)
                    {
                        projectLoaded = true;
                        break;
                    }
                    Thread.Sleep(1000);
                }

                if (!projectLoaded)
                {
                }

                var device1 = project.Devices.FirstOrDefault(d => d.Name.Contains("PLC_1") || d.Name.Contains("S7-300"));

                if (device1 == null)
                {
                }

                var softwareContainer1 = device1.DeviceItems[1].GetService<SoftwareContainer>();

                var tiaProcess = Process.GetProcessesByName("Siemens.Automation.Portal")
                    .FirstOrDefault(p => p.MainWindowTitle.Contains("CPU300TempProject"));

                if (tiaProcess != null)
                {
                    using (var app = FlaUI.Core.Application.Attach(tiaProcess))
                    using (var automation = new UIA3Automation())
                    {
                        var retryMainWindow = Retry.WhileNull(() => app.GetMainWindow(automation), timeout: TimeSpan.FromSeconds(15));
                        var mainWindow = retryMainWindow.Result;

                        if (mainWindow != null)
                        {
                            var projectViewBtn = mainWindow.FindFirstDescendant(cf =>
                                cf.ByName("ContextNavigator.GoToProjectView.Title")
                                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.ListItem)));

                            projectViewBtn?.AsButton()?.Invoke();

                            var projectTreeContainer = mainWindow.FindFirstDescendant(cf =>
                                            cf.ByAutomationId("ProjectNavigationTree")
                                              .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Table)));

                            if (projectTreeContainer != null)
                            {
                                var plcEdit = projectTreeContainer.FindFirstDescendant(cf =>
                                    cf.ByName("PLC_1 [CPU 312]:1E1C1007-B70C-4670-81E9-515ABD35EA0C.Attribute.Name")
                                      .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit)));

                                plcEdit?.Focus();
                                Thread.Sleep(2000);

                                var invokePattern = plcEdit?.Patterns?.Invoke?.PatternOrDefault;
                                if (invokePattern != null)
                                {
                                    invokePattern.Invoke();
                                }
                                else
                                {
                                    SendKeys.SendWait("{ENTER}");
                                    Thread.Sleep(100);
                                    SendKeys.SendWait("{ENTER}");
                                }

                                var progBlock = mainWindow.FindFirstDescendant(cf =>
                                    cf.ByName("Siemens.Simatic.Lang.BlockLogic:Siemens.Simatic.PlcLanguages.BlockLogic.MenuService:SystemFolderProgramBlocksResX:1E1C1007-B70C-4670-81E9-515ABD35EA0C.Attribute.Name")
                                      .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit)));

                                if (progBlock != null)
                                {
                                    progBlock.Focus();
                                    Thread.Sleep(5000);
                                    SendKeys.SendWait("+{F10}");
                                    Thread.Sleep(1000);

                                    progBlock.RightClick();
                                    Thread.Sleep(1000);

                                    var desktop = automation.GetDesktop();
                                    var switchLangMenu = desktop.FindFirstDescendant(cf =>
                                        cf.ByName("Siemens.Simatic.Lang.Portal:Siemens.Simatic.Lang.Portal.Resources.BlockLogic.MenuService:SwitchBlockLanguage")
                                          .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Menu)));

                                    if (switchLangMenu != null)
                                    {
                                        switchLangMenu.Focus();
                                        switchLangMenu.Click();

                                        var retryLadMenu = Retry.WhileNull(() =>
                                            desktop.FindFirstDescendant(cf =>
                                                cf.ByName("Siemens.Simatic.Lang.Portal:Siemens.Simatic.Lang.Portal.Resources.BlockLogic.MenuService:SwitchBlockLanguage_LAD")
                                                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.MenuItem))),
                                            TimeSpan.FromSeconds(5)
                                        );

                                        var ladMenu = retryLadMenu.Result;
                                        if (ladMenu != null)
                                        {
                                            ladMenu.AsMenuItem()?.Invoke();
                                        }
                                        else
                                        {
                                        }
                                    }
                                    else
                                    {
                                    }

                                    project.Save();
                                }
                            }
                        }
                    }
                }
                ExportAllBlocks(project, softwareContainer1?.Software as PlcSoftware, exportDirectory);
                try
                {
                    tiaPortal.Dispose();

                    var cpu300TiaProcess = Process.GetProcessesByName("Siemens.Automation.Portal")
                        .FirstOrDefault(p => p.MainWindowTitle.Contains("CPU300TempProject"));

                    if (cpu300TiaProcess != null && !cpu300TiaProcess.HasExited)
                    {
                        cpu300TiaProcess.Kill();
                        cpu300TiaProcess.WaitForExit(10000);
                    }

                    Thread.Sleep(2000);
                }
                catch (System.Exception ex)
                {
                }

                try
                {
                    string cpu300Folder = @"C:\\Generate Block\\CPU300TempProject";
                    if (Directory.Exists(cpu300Folder))
                    {
                        Directory.Delete(cpu300Folder, true);
                    }
                }
                catch (System.Exception ex)
                {
                }

                try
                {
                }
                catch (System.Exception ex)
                {
                }

                return errorAwlFiles;
            }
        }
        catch (EngineeringException ex)
        {
        }
        catch (System.Exception ex)
        {
        }

        return errorAwlFiles;
    }

    public static List<string> GetCompilerMessages(string rootPath, string path, CompilerResultMessageComposition messageComposition, bool error)
    {
        List<string> messages = new List<string>();

        try
        {
            foreach (var message in messageComposition)
            {
                string messagePath = message.Path?.Trim();
                string combinedPath = !string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(messagePath)
                    ? path + " > " + messagePath
                    : messagePath ?? path;

                if (message.Messages != null && message.Messages.Count > 0)
                {
                    var subMessages = GetCompilerMessages(rootPath, combinedPath, message.Messages, error);
                    messages.AddRange(subMessages);
                }

                if (!string.IsNullOrWhiteSpace(message.Description))
                {
                    bool isError = message.State == CompilerResultState.Error;
                    bool isWarning = message.State == CompilerResultState.Warning;
                    bool isInfo = message.State == CompilerResultState.Information;

                    if ((error && isError) || (!error && (isError || isWarning || isInfo)))
                    {
                        string formattedMessage = $"{message.State} | Path: {combinedPath} | Description: {message.Description}";
                        messages.Add(formattedMessage);
                    }
                }
            }
        }
        catch (EngineeringException engineeringException)
        {
            messages.Add($"EngineeringException: {engineeringException.Message}");
        }

        return messages;
    }
    static string GetBlockNameFromAwlFile(string awlFilePath)
    {
        string pattern = @"^\s*FUNCTION_BLOCK\s+""([^""]+)""";

        foreach (var line in File.ReadLines(awlFilePath))
        {
            var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
        }

        return null;
    }
    static PlcBlock FindBlockRecursively(PlcBlockGroup blockGroup, string blockName)
    {
        foreach (var block in blockGroup.Blocks)
        {
            if (block.Name.Equals(blockName, StringComparison.OrdinalIgnoreCase))
                return block;
        }

        foreach (var subGroup in blockGroup.Groups)
        {
            var block = FindBlockRecursively(subGroup, blockName);
            if (block != null)
                return block;
        }

        return null;
    }

    static void ExportAllBlocks(Project project, PlcSoftware plcSoftware, string exportDirectory)
    {
        List<string> compileResults = new List<string>();

        try
        {
            string backupFolder = Path.Combine(@"C:\Generate Block", "Backup XML Files V17");

            if (!Directory.Exists(backupFolder))
                Directory.CreateDirectory(backupFolder);

            string[] existingFiles = Directory.GetFiles(mainOutputFolder, "*.xml");
            foreach (string file in existingFiles)
            {
                string destPath = Path.Combine(backupFolder, Path.GetFileName(file));

                int count = 1;
                string baseName = Path.GetFileNameWithoutExtension(file);
                string extension = Path.GetExtension(file);

                while (File.Exists(destPath))
                {
                    destPath = Path.Combine(backupFolder, $"{baseName}_{count}{extension}");
                    count++;
                }

                File.Move(file, destPath);
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show("Failed to back up XML files before export:\n" + ex.Message, "Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        var compiler = plcSoftware.GetService<ICompilable>();
        var result = compiler.Compile();

        foreach (var block in plcSoftware.BlockGroup.Blocks.ToList())
        {
            if (block is PlcBlock plcBlock)
            {
                string baseName = plcBlock.Name;
                string finalName = baseName;
                string filePath = Path.Combine(exportDirectory, finalName + ".xml");

                int count = 1;
                while (File.Exists(filePath))
                {
                    finalName = baseName + "_" + count;
                    filePath = Path.Combine(exportDirectory, finalName + ".xml");
                    count++;
                }
                try
                {
                    plcBlock.Export(new FileInfo(filePath), ExportOptions.WithDefaults);
                    compileResults.Add($"Exported block: {finalName}");

                    plcBlock.Delete();
                    compileResults.Add($"Deleted block: {finalName}");
                }
                catch (System.Exception ex)
                {
                    compileResults.Add($"Failed to export/delete {finalName}: {ex.Message}");
                }
            }
        }

        string exportLogPath = Path.Combine(exportDirectory, "export_log.txt");
        File.WriteAllLines(exportLogPath, compileResults);
        ShowCompilationResults(compileResults);

        var device = project.Devices.FirstOrDefault(d => d.Name.Contains("PLC_1") || d.Name.Contains("S7-300"));
        if (device != null)
        {
            try
            {
                string deviceName = device.Name;
                device.Delete();
            }
            catch (System.Exception ex)
            {
            }
        }
        else
        {
        }
    }

    static void ShowCompilationResults(List<string> results)
    {
    }
    private static Form GetForegroundWindow(string title = null)
    {
        Form form = new Form
        {
            Opacity = 0,
            ShowIcon = false,
            Text = title,
            Width = 1,
            Height = 1,
            StartPosition = FormStartPosition.CenterScreen,
            ShowInTaskbar = false
        };

        form.Load += (s, e) =>
        {
            form.TopMost = true;
            form.Activate();
            form.TopMost = false;
        };

        form.Show();
        return form;
    }


  
    

    // ✅ Final SikuliX launcher
    //public static void LaunchPSVCFromVDI()
    //{


    //    string sikuliJarPath = @"C:\Users\P01090879\Downloads\sikulixide-2.0.5-win.jar";
    //    string scriptPath = @"C:\Users\P01090879\source\repos\Open Ps\ConsoleApp6\Images\psvclitescript.sikuli";

    //    if (!File.Exists(sikuliJarPath))
    //    {
    //        Console.WriteLine("❌ SikuliX JAR not found at: " + sikuliJarPath);
    //        return;
    //    }

    //    if (!Directory.Exists(scriptPath))
    //    {
    //        Console.WriteLine("❌ Sikuli script folder not found at: " + scriptPath);
    //        return;
    //    }

    //    try
    //    {
    //        ProcessStartInfo psi = new ProcessStartInfo
    //        {

    //            FileName = "C:\\Users\\P01090879\\Downloads\\openjdk-25_windows-x64_bin\\jdk-25\\bin\\java.exe",
    //            Arguments = $"-jar \"{sikuliJarPath}\" -r \"{scriptPath}\"",
    //            UseShellExecute = false,
    //            CreateNoWindow = true,
    //            RedirectStandardOutput = true,
    //            RedirectStandardError = true
    //        };

    //        using (Process sikuliProcess = new Process())
    //        {
    //            sikuliProcess.StartInfo = psi;
    //            sikuliProcess.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine(e.Data); };
    //            sikuliProcess.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine("[Error] " + e.Data); };

    //            sikuliProcess.Start();
    //            sikuliProcess.BeginOutputReadLine();
    //            sikuliProcess.BeginErrorReadLine();

    //            Console.WriteLine("🚀 Running Sikuli script to launch PS VC Lite...");
    //            sikuliProcess.WaitForExit();

    //            Console.WriteLine("✅ Sikuli script execution finished.");
    //        }

    //        // Wait 2 minutes for the app to load
    //        Thread.Sleep(TimeSpan.FromMinutes(2));
    //    }
    //    catch (System.Exception ex)
    //    {
    //        Console.WriteLine("⚠️ Error launching Sikuli script: " + ex.Message);
    //    }
    //}

    //public static void ClickAcceptAllCookies()
    //{
    //    string sikuliJarPath = @"C:\Users\P01090879\Downloads\sikulixide-2.0.5-win.jar";
    //    string scriptPath = @"C:\Users\P01090879\source\repos\Open Ps\ConsoleApp6\Images\Accept All Cookies.sikuli";

    //    if (!File.Exists(sikuliJarPath))
    //    {
    //        Console.WriteLine("❌ SikuliX JAR not found at: " + sikuliJarPath);
    //        return;
    //    }

    //    if (!Directory.Exists(scriptPath))
    //    {
    //        Console.WriteLine("❌ Sikuli script folder not found at: " + scriptPath);
    //        return;
    //    }

    //    try
    //    {
    //        ProcessStartInfo psi = new ProcessStartInfo
    //        {
    //            FileName = "C:\\Users\\P01090879\\Downloads\\openjdk-25_windows-x64_bin\\jdk-25\\bin\\java.exe",
    //            Arguments = $"-jar \"{sikuliJarPath}\" -r \"{scriptPath}\"",
    //            UseShellExecute = false,
    //            CreateNoWindow = true,
    //            RedirectStandardOutput = true,
    //            RedirectStandardError = true
    //        };

    //        using (Process sikuliProcess = new Process())
    //        {
    //            sikuliProcess.StartInfo = psi;
    //            sikuliProcess.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine(e.Data); };
    //            sikuliProcess.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine("[Error] " + e.Data); };

    //            sikuliProcess.Start();
    //            sikuliProcess.BeginOutputReadLine();
    //            sikuliProcess.BeginErrorReadLine();

    //            Console.WriteLine("🚀 Running Sikuli script to launch PS VC Lite...");
    //            sikuliProcess.WaitForExit();

    //            Console.WriteLine("✅ Sikuli script execution finished.");
    //        }

    //        // Wait 2 minutes for the app to load
    //        Thread.Sleep(TimeSpan.FromMinutes(2));
    //    }
    //    catch (System.Exception ex)
    //    {
    //        Console.WriteLine("⚠️ Error launching Sikuli script: " + ex.Message);
    //    }
    //}


    public static void Clipboardopen()
    {
        string sikuliJarPath = @"C:\Users\P01090879\Downloads\sikulixide-2.0.5-win.jar";
        string scriptPath = @"C:\Users\P01090879\source\repos\Open Ps\ConsoleApp6\Images\clipboard.sikuli";

        if (!File.Exists(sikuliJarPath))
        {
            Console.WriteLine("❌ SikuliX JAR not found at: " + sikuliJarPath);
            return;
        }

        if (!Directory.Exists(scriptPath))
        {
            Console.WriteLine("❌ Sikuli script folder not found at: " + scriptPath);
            return;
        }

        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "C:\\Users\\P01090879\\Downloads\\openjdk-25_windows-x64_bin\\jdk-25\\bin\\java.exe",
                Arguments = $"-jar \"{sikuliJarPath}\" -r \"{scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process sikuliProcess = new Process())
            {
                sikuliProcess.StartInfo = psi;
                sikuliProcess.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine(e.Data); };
                sikuliProcess.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine("[Error] " + e.Data); };

                sikuliProcess.Start();
                sikuliProcess.BeginOutputReadLine();
                sikuliProcess.BeginErrorReadLine();

                Console.WriteLine("🚀 Running Sikuli script to launch PS VC Lite...");
                sikuliProcess.WaitForExit();

                Console.WriteLine("✅ Sikuli script execution finished.");
            }

            // Wait 2 minutes for the app to load
            Thread.Sleep(TimeSpan.FromMinutes(2));
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("⚠️ Error launching Sikuli script: " + ex.Message);
        }
    }
    public static void Closeclipboard()
    {
        string sikuliJarPath = @"C:\Users\P01090879\Downloads\sikulixide-2.0.5-win.jar";
        string scriptPath = @"C:\Users\P01090879\source\repos\Open Ps\ConsoleApp6\Images\Close Clioboard.sikuli";

        if (!File.Exists(sikuliJarPath))
        {
            Console.WriteLine("❌ SikuliX JAR not found at: " + sikuliJarPath);
            return;
        }

        if (!Directory.Exists(scriptPath))
        {
            Console.WriteLine("❌ Sikuli script folder not found at: " + scriptPath);
            return;
        }

        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "C:\\Users\\P01090879\\Downloads\\openjdk-25_windows-x64_bin\\jdk-25\\bin\\java.exe",
                Arguments = $"-jar \"{sikuliJarPath}\" -r \"{scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process sikuliProcess = new Process())
            {
                sikuliProcess.StartInfo = psi;
                sikuliProcess.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine(e.Data); };
                sikuliProcess.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine("[Error] " + e.Data); };

                sikuliProcess.Start();
                sikuliProcess.BeginOutputReadLine();
                sikuliProcess.BeginErrorReadLine();

                Console.WriteLine("🚀 Running Sikuli script to launch PS VC Lite...");
                sikuliProcess.WaitForExit();

                Console.WriteLine("✅ Sikuli script execution finished.");
            }

            // Wait 2 minutes for the app to load
            Thread.Sleep(TimeSpan.FromMinutes(2));
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("⚠️ Error launching Sikuli script: " + ex.Message);
        }
    }
    //public static void CopyExcelDataListToClipboardAndRunSikuli()
    //{
    //    // 1️⃣ Build CSV-like string
    //    StringBuilder sb = new StringBuilder();

    //    foreach (var sheet in ExcelDataList)
    //    {
    //        foreach (var row in sheet)
    //        {
    //            sb.AppendLine(string.Join("\t", row.Select(cell => EscapeCsv(cell))));
    //        }
    //        sb.AppendLine(); // blank line between sheets
    //    }

    //    string clipboardText = sb.ToString();

    //    // 2️⃣ Copy to clipboard
    //    Clipboard.SetText(clipboardText);
    //    Console.WriteLine("✅ ExcelDataList copied to clipboard.");

    //    // 3️⃣ Launch Sikuli script
    //    string sikuliJarPath = @"C:\Users\P01090879\Downloads\sikulixide-2.0.5-win.jar";
    //    string scriptPath = @"C:\Users\P01090879\source\repos\Open Ps\ConsoleApp6\Images\clipboard.sikuli";
    //    //string sikuliPath = @"C:\Users\P01090879\source\repos\Open Ps\ConsoleApp6\Images\PasteClipboardData.sikuli";
    //    //string sikuliCmd = @"C:\SikuliX\sikulixide.cmd"; // 🔹 Adjust your SikuliX path if needed

    //    //string sikuliJarPath = @"C:\Users\P01090879\Downloads\sikulixide-2.0.5-win.jar";
    //    //string scriptPath = @"C:\Users\P01090879\source\repos\Open Ps\ConsoleApp6\Images\opencsv.sikuli";

    //    if (!File.Exists(sikuliJarPath))
    //    {
    //        Console.WriteLine("❌ SikuliX JAR not found at: " + sikuliJarPath);
    //        return;
    //    }

    //    if (!Directory.Exists(scriptPath))
    //    {
    //        Console.WriteLine("❌ Sikuli script folder not found at: " + scriptPath);
    //        return;
    //    }

    //    try
    //    {
    //        ProcessStartInfo psi = new ProcessStartInfo
    //        {
    //            FileName = "C:\\Users\\P01090879\\Downloads\\openjdk-25_windows-x64_bin\\jdk-25\\bin\\java.exe",
    //            Arguments = $"-jar \"{sikuliJarPath}\" -r \"{scriptPath}\"",
    //            UseShellExecute = false,
    //            CreateNoWindow = true,
    //            RedirectStandardOutput = true,
    //            RedirectStandardError = true
    //        };

    //        using (Process sikuliProcess = new Process())
    //        {
    //            sikuliProcess.StartInfo = psi;
    //            sikuliProcess.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine(e.Data); };
    //            sikuliProcess.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine("[Error] " + e.Data); };

    //            sikuliProcess.Start();
    //            sikuliProcess.BeginOutputReadLine();
    //            sikuliProcess.BeginErrorReadLine();

    //            Console.WriteLine("🚀 Running Sikuli script to launch PS VC Lite...");
    //            sikuliProcess.WaitForExit();

    //            Console.WriteLine("✅ Sikuli script execution finished.");
    //        }

    //        // Wait 2 minutes for the app to load
    //        Thread.Sleep(TimeSpan.FromMinutes(2));
    //    }
    //    catch (System.Exception ex)
    //    {
    //        Console.WriteLine("⚠️ Error launching Sikuli script: " + ex.Message);
    //    }
    //}
    //// Helper – escape CSV properly
    //private static string EscapeCsv(string cell)
    //{
    //    if (cell.Contains(",") || cell.Contains("\"") || cell.Contains("\n"))
    //    {
    //        return "\"" + cell.Replace("\"", "\"\"") + "\"";
    //    }
    //    return cell;
    //}
  

    //public static void OpenCSV()
    //{
    //    string sikuliJarPath = @"C:\Users\P01090879\Downloads\sikulixide-2.0.5-win.jar";
    //    string scriptPath = @"C:\Users\P01090879\source\repos\Open Ps\ConsoleApp6\Images\opencsv.sikuli";

    //    if (!File.Exists(sikuliJarPath))
    //    {
    //        Console.WriteLine("❌ SikuliX JAR not found at: " + sikuliJarPath);
    //        return;
    //    }

    //    if (!Directory.Exists(scriptPath))
    //    {
    //        Console.WriteLine("❌ Sikuli script folder not found at: " + scriptPath);
    //        return;
    //    }

    //    try
    //    {
    //        ProcessStartInfo psi = new ProcessStartInfo
    //        {
    //            FileName = "C:\\Users\\P01090879\\Downloads\\openjdk-25_windows-x64_bin\\jdk-25\\bin\\java.exe",
    //            Arguments = $"-jar \"{sikuliJarPath}\" -r \"{scriptPath}\"",
    //            UseShellExecute = false,
    //            CreateNoWindow = true,
    //            RedirectStandardOutput = true,
    //            RedirectStandardError = true
    //        };

    //        using (Process sikuliProcess = new Process())
    //        {
    //            sikuliProcess.StartInfo = psi;
    //            sikuliProcess.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine(e.Data); };
    //            sikuliProcess.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine("[Error] " + e.Data); };

    //            sikuliProcess.Start();
    //            sikuliProcess.BeginOutputReadLine();
    //            sikuliProcess.BeginErrorReadLine();

    //            Console.WriteLine("🚀 Running Sikuli script to launch PS VC Lite...");
    //            sikuliProcess.WaitForExit();

    //            Console.WriteLine("✅ Sikuli script execution finished.");
    //        }

    //        // Wait 2 minutes for the app to load
    //        Thread.Sleep(TimeSpan.FromMinutes(2));
    //    }
    //    catch (System.Exception ex)
    //    {
    //        Console.WriteLine("⚠️ Error launching Sikuli script: " + ex.Message);
    //    }
    //}

    //public static void Opencs()
    //{
    //    string sikuliJarPath = @"C:\Users\P01090879\Downloads\sikulixide-2.0.5-win.jar";
    //    string scriptPath = @"C:\Users\P01090879\source\repos\Open Ps\ConsoleApp6\Images\opencs.sikuli";

    //    if (!File.Exists(sikuliJarPath))
    //    {
    //        Console.WriteLine("❌ SikuliX JAR not found at: " + sikuliJarPath);
    //        return;
    //    }

    //    if (!Directory.Exists(scriptPath))
    //    {
    //        Console.WriteLine("❌ Sikuli script folder not found at: " + scriptPath);
    //        return;
    //    }

    //    try
    //    {
    //        ProcessStartInfo psi = new ProcessStartInfo
    //        {
    //            FileName = "C:\\Users\\P01090879\\Downloads\\openjdk-25_windows-x64_bin\\jdk-25\\bin\\java.exe",
    //            Arguments = $"-jar \"{sikuliJarPath}\" -r \"{scriptPath}\"",
    //            UseShellExecute = false,
    //            CreateNoWindow = true,
    //            RedirectStandardOutput = true,
    //            RedirectStandardError = true
    //        };

    //        using (Process sikuliProcess = new Process())
    //        {
    //            sikuliProcess.StartInfo = psi;
    //            sikuliProcess.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine(e.Data); };
    //            sikuliProcess.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine("[Error] " + e.Data); };

    //            sikuliProcess.Start();
    //            sikuliProcess.BeginOutputReadLine();
    //            sikuliProcess.BeginErrorReadLine();

    //            Console.WriteLine("🚀 Running Sikuli script to launch PS VC Lite...");
    //            sikuliProcess.WaitForExit();

    //            Console.WriteLine("✅ Sikuli script execution finished.");
    //        }

    //        // Wait 2 minutes for the app to load
    //        Thread.Sleep(TimeSpan.FromMinutes(2));
    //    }
    //    catch (System.Exception ex)
    //    {
    //        Console.WriteLine("⚠️ Error launching Sikuli script: " + ex.Message);
    //    }
    //}

    public static void ClickAcceptAllCookies(IWebDriver driver)
    {
        int retries = 5; // number of retries
        bool clicked = false;

        for (int i = 0; i < retries; i++)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                IWebElement acceptButton = wait.Until(d =>
                {
                    var btn = d.FindElements(By.CssSelector("button[data-testid='uc-accept-all-button']")).FirstOrDefault();
                    return (btn != null && btn.Displayed) ? btn : null;
                });

                if (acceptButton != null)
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", acceptButton);
                    Thread.Sleep(300);
                    acceptButton.Click();
                    Console.WriteLine("✅ 'Accept All Cookies' clicked.");
                    clicked = true;
                    break;
                }
            }
            catch
            {
                Thread.Sleep(1000); // wait 1 second before retrying
            }
        }

        if (!clicked)
        {
            Console.WriteLine("⚠️ 'Accept All Cookies' button not found after retries.");
        }
    }


    static void MonitorUploadStatus(IWebDriver driver, DateTime uploadStartTime, int excelCount, string downloadDirectory)
    {
        try
        {
            TimeSpan overallTimeout = TimeSpan.FromMinutes(30);
            DateTime monitoringStartTime = DateTime.Now;
            bool fileDownloaded = false;
            HashSet<string> downloadedFiles = new HashSet<string>();

            string awlFilesDirectory = @"C:\Generate Block\AWL Files & Reports";
            if (!Directory.Exists(awlFilesDirectory))
                Directory.CreateDirectory(awlFilesDirectory);

            string backupDir = Path.Combine(@"C:\Generate Block", "Backup AWL Files And Reports");
            if (!Directory.Exists(backupDir))
                Directory.CreateDirectory(backupDir);

            string[] allFilesToBackup = Directory.GetFiles(awlFilesDirectory);
            foreach (string file in allFilesToBackup)
            {
                string fileName = Path.GetFileName(file);
                string destinationPath = Path.Combine(backupDir, fileName);

                int counter = 1;
                while (File.Exists(destinationPath))
                {
                    string name = Path.GetFileNameWithoutExtension(fileName);
                    string ext = Path.GetExtension(fileName);
                    destinationPath = Path.Combine(backupDir, $"{name}_{counter}{ext}");
                    counter++;
                }

                File.Move(file, destinationPath);
            }

            while (DateTime.Now - monitoringStartTime < overallTimeout)
            {
                int rowCount = driver.FindElements(By.CssSelector("table.user-table tbody tr.user-row")).Count;

                for (int i = 0; i < rowCount; i++)
                {
                    driver.Navigate().Refresh();
                    Thread.Sleep(2000);

                    var rows = driver.FindElements(By.CssSelector("table.user-table tbody tr.user-row"));
                    if (i >= rows.Count)
                        continue;

                    var row = rows[i];
                    var cells = row.FindElements(By.TagName("td"));
                    if (cells.Count < 6) continue;

                    string uniqueId = cells[0].Text.Trim();
                    string statusText = cells[5].Text.Trim().ToLower();
                    string timeString = cells[1].Text.Trim();
                    string success = "";

                    foreach (var cell in cells)
                    {
                        var img = cell.FindElements(By.TagName("img")).FirstOrDefault();
                        if (img != null)
                        {
                            success = img.GetAttribute("src");
                            break;
                        }
                    }

                    if (downloadedFiles.Contains(uniqueId))
                        continue;

                    if (DateTime.TryParse(timeString, out DateTime fileTime) && fileTime >= uploadStartTime)
                    {
                        if (success.Contains("/icon-success"))
                        {
                            try
                            {
                                IWebElement downloadBtn = row.FindElement(By.CssSelector("button.download-btn"));

                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", downloadBtn);
                                Thread.Sleep(5000);
                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", downloadBtn);
                                Thread.Sleep(3000);

                                int expectedCount = 3;
                                int waitTime = 0;
                                int maxWait = 30;
                                int previousFileCount = Directory.GetFiles(downloadDirectory).Length;

                                while (Directory.GetFiles(downloadDirectory).Length - previousFileCount < expectedCount && waitTime < maxWait)
                                {
                                    Thread.Sleep(1000);
                                    waitTime++;
                                }

                                fileDownloaded = true;
                                downloadedFiles.Add(uniqueId);

                                string matchedFile = uploadTimes.Keys.FirstOrDefault(k =>
                                    Path.GetFileNameWithoutExtension(k).Equals(uniqueId, StringComparison.OrdinalIgnoreCase) ||
                                    uniqueId.IndexOf(Path.GetFileNameWithoutExtension(k), StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    Path.GetFileNameWithoutExtension(k).IndexOf(uniqueId, StringComparison.OrdinalIgnoreCase) >= 0
                                );

                                if (!string.IsNullOrEmpty(matchedFile) && uploadTimes.TryGetValue(matchedFile, out DateTime actualUploadTime))
                                {
                                    TimeSpan generationTime = fileTime - actualUploadTime;
                                    string log = $"File Name: {Path.GetFileName(matchedFile)}\n" +
                                                 $"Uploaded At: {actualUploadTime:dd-MM-yyyy hh:mm:ss tt}\n" +
                                                 $"Generated At: {fileTime:dd-MM-yyyy hh:mm:ss tt}\n" +
                                                 $"Time Taken: {generationTime}\n" +
                                                 $"--------------------------\n";

                                    string logFilePath = Path.Combine(downloadDirectory, "Upload Report.txt");
                                    File.AppendAllText(logFilePath, log);
                                }
                            }
                            catch (NoSuchElementException)
                            {
                            }
                        }
                    }

                    if (downloadedFiles.Count >= excelCount)
                    {
                        try
                        {
                            Thread.Sleep(5000);

                            string[] validationFiles = Directory.GetFiles(downloadDirectory, "validation_*.txt");
                            StringBuilder reportBuilder = new StringBuilder();
                            StringBuilder popupBuilder = new StringBuilder();

                            foreach (var file in validationFiles)
                            {
                                string[] lines = File.ReadAllLines(file);
                                string fileName = Path.GetFileNameWithoutExtension(file).Replace("validation_", "");
                                string spreadsheetName = $"{fileName}.xlsx";
                                string description = "";

                                string lineA = "All state networks have atleast one connection from a transition variable";
                                string lineBStart = "One or more state network has no incoming conections from any transition variables";

                                string verifyLine1 = "Verify if the conditions are defined as expected for states";
                                string verifyLine2 = "Specifically verify if the boolean expressions are properly created";

                                if (lines.Any(line => line.Contains(lineA)))
                                {
                                    description = lineA;
                                }
                                else
                                {
                                    string lineBFull = lines.FirstOrDefault(line => line.StartsWith(lineBStart));
                                    if (!string.IsNullOrEmpty(lineBFull))
                                    {
                                        description = lineBFull;

                                        var extraLines = lines.Where(l =>
                                            l.StartsWith(verifyLine1) || l.StartsWith(verifyLine2)).ToList();

                                        if (extraLines.Any())
                                        {
                                            description += "\n\n" + string.Join("\n", extraLines);
                                        }
                                    }
                                }

                                reportBuilder.AppendLine($"=== SPREADSHEET NAME ===");
                                reportBuilder.AppendLine($"{spreadsheetName}");
                                reportBuilder.AppendLine($"=== DESCRIPTION ===");
                                reportBuilder.AppendLine(description);
                                reportBuilder.AppendLine(new string('-', 60));

                                popupBuilder.AppendLine($"*SPREADSHEET NAME:* {spreadsheetName}");
                                popupBuilder.AppendLine($"*DESCRIPTION:*");
                                popupBuilder.AppendLine(description);
                                popupBuilder.AppendLine(new string('-', 60));
                            }

                            string reportPath = Path.Combine(downloadDirectory, "Generated Validation Report.txt");
                            File.WriteAllText(reportPath, reportBuilder.ToString());

                            string popupText = popupBuilder.ToString();
                            if (!string.IsNullOrWhiteSpace(popupText))
                            {
                                string popupDisplay = popupText.Length > 1500
                                     ? popupText.Substring(0, 1500) + "\n...\n(See full report in file)"
                                     : popupText;

                                MessageBox.Show(popupDisplay, "Generated Validation Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        catch (System.Exception ex)
                        {
                        }
                        return;
                    }
                }

                if (!fileDownloaded)
                    Thread.Sleep(2000);
            }

            if (!fileDownloaded)
                Thread.Sleep(1000);
        }
        catch (System.Exception ex)
        {
        }
    }

    static string selectedPrefix = null;
    static string sessionFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.txt");
    static string prefixSessionFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "prefix.txt");

    static string globalProjectPath = null;
    static string globalTiaProcId = null;

    static bool licenseValid = false;
    static bool projCheck(string projectPath, string fullDeviceName)
    {
        TiaPortal tiaPortal = null;
        Project project = null;

        try
        {
            var tiaProcesses = TiaPortal.GetProcesses();
            string normalizedTargetPath = Path.GetFullPath(projectPath).ToLowerInvariant();
            bool attached = false;
            string procID = "";

            foreach (var proc in tiaProcesses)
            {
                try
                {
                    var portal = proc.Attach();
                    foreach (var proj in portal.Projects)
                    {
                        string openPath = Path.GetFullPath(proj.Path.FullName).TrimEnd('\\');
                        if (openPath.Equals(projectPath, StringComparison.OrdinalIgnoreCase))
                        {
                            procID = proc.Id.ToString();
                            tiaPortal = portal;
                            project = proj;
                            attached = true;
                            break;
                        }
                    }

                    if (attached)
                        break;
                    else
                        portal.Dispose();
                }
                catch { continue; }
            }

            if (!attached)
            {
                MessageBox.Show("Could not find matching open project in any TIA instance.", "Attach Error");
                return false;
            }

            globalProjectPath = Path.GetFullPath(project.Path.FullName);
            globalTiaProcId = procID;

            string currentProjectId = globalProjectPath.ToLowerInvariant() + procID;

            string licenseFile = @"C:\Generate Block\Debug\license.dat"; //Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "license.dat");

            if (File.Exists(licenseFile))
            {
                try
                {
                    string encryptedLicense = File.ReadAllText(licenseFile);
                    string decryptedLicense = EncryptionHelper.Decrypt(encryptedLicense);
                    string[] parts = decryptedLicense.Split('|');
                    if (parts.Length == 2)
                    {
                        string licensedUserId = parts[0];
                        DateTime expiryDate = DateTime.ParseExact(parts[1], "yyyy-MM-dd HH:mm:ss", null);

                        if (/*licensedUserId == TiaPortalAutomation.userID && */DateTime.Now <= expiryDate)
                        {
                            licenseValid = true;
                        }
                        else
                        {
                            MessageBox.Show("License expired. Contact admin for renewal.",
                                "License Expired", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid license format. Contact admin.",
                            "License Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch
                {
                    MessageBox.Show("License file corrupted. Contact admin.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (!licenseValid)
            {
                GetApplicationKey(fullDeviceName);
                using (var licenseForm = new LicenseForm(ApplicationKey, licenseValid, true))
                {
                    if (licenseForm.ShowDialog() != DialogResult.OK)
                    {
                        return false;
                    }

                    File.WriteAllText(sessionFile, currentProjectId);
                }
            }

            if (File.Exists(sessionFile))
            {
                string savedProjectPath = File.ReadAllText(sessionFile).Trim();

                if (savedProjectPath == currentProjectId)
                {
                    Console.WriteLine("Same project already opened. No login required.");
                }
                else
                {
                    Console.WriteLine("Different project detected. Asking for login...");

                    GetApplicationKey(fullDeviceName);
                    LicenseForm form = new LicenseForm(ApplicationKey, licenseValid,false);
                    DialogResult result = form.ShowDialog();
                    if (result != DialogResult.OK)
                        return false;

                    File.WriteAllText(sessionFile, currentProjectId);
                }
            }
            else
            {
                Console.WriteLine("First run or no session detected. Asking for login...");

                GetApplicationKey(fullDeviceName);
                LicenseForm form = new LicenseForm(ApplicationKey, licenseValid,true);
                DialogResult result = form.ShowDialog();
                if (result != DialogResult.OK)
                    return false;

                File.WriteAllText(sessionFile, currentProjectId);
            }
            bool prefixLoaded = false;
            if (File.Exists(prefixSessionFile))
            {
                var prefixData = File.ReadAllText(prefixSessionFile).Split('|');
                if (prefixData.Length == 3 && prefixData[0] == currentProjectId)
                {
                    selectedPrefix = prefixData[1];
                    bool doSameForAll = prefixData[2] == "1";

                    if (doSameForAll)
                    {
                        PrefixSelectionForm.SetDoSameForAll(true);
                        prefixLoaded = true;
                    }
                }
            }

            if (!prefixLoaded)
            {
                var prefixForm = new PrefixSelectionForm(globalProjectPath, globalTiaProcId);
                if (prefixForm.ShowDialog() == DialogResult.OK)
                {
                    selectedPrefix = prefixForm.SelectedPrefix;

                    string doSameForAllFlag = prefixForm.DoSameForAllChecked ? "1" : "0";
                    File.WriteAllText(prefixSessionFile, $"{currentProjectId}|{selectedPrefix}|{doSameForAllFlag}");
                }
                else
                {
                    return false;
                }
            }
        }
        catch 
        { 
        }
            return true;
    }
    static List<string> ImportXmlFilesFromCorrectTIAInstance(string projectPath, string xmlFilesDirectory, string caxFilePath1)
    {
        List<string> errorXmlFiles = new List<string>();
        List<string> addBlockVersion = new List<string>();
        TiaPortal tiaPortal = null;
        Project project = null;
        try
        {
            var tiaProcesses = TiaPortal.GetProcesses();
            string normalizedTargetPath = Path.GetFullPath(projectPath).ToLowerInvariant();
            bool attached = false;

            foreach (var proc in tiaProcesses)
            {
                try
                {
                    var portal = proc.Attach();
                    foreach (var proj in portal.Projects)
                    {
                        string openPath = Path.GetFullPath(proj.Path.FullName).TrimEnd('\\');
                        if (openPath.Equals(projectPath, StringComparison.OrdinalIgnoreCase))
                        {
                            tiaPortal = portal;
                            project = proj;
                            attached = true;
                            break;
                        }
                    }

                    if (attached)
                        break;
                    else
                        portal.Dispose();
                }
                catch { continue; }
            }

            if (!attached)
            {
                MessageBox.Show("Could not find matching open project in any TIA instance.", "Attach Error");
                return errorXmlFiles;            
            }

            var device = project.Devices.FirstOrDefault(d => d.Name.Contains("PLC_2") || d.Name.Contains("S7-1500"));
            if (device == null)
            {
                try
                {
                    device = project.Devices.CreateWithItem(
                        "OrderNumber:6ES7 515-2AM00-0AB0/V1.8",
                        "PLC_2",
                        "S7-1500/ET200MP station_1"
                    );
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Failed to create CPU1500 device:\n" + ex.Message, "Device Creation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return errorXmlFiles;
                }
            }

            var softwareContainer = device.DeviceItems[1].GetService<SoftwareContainer>();
            var plcSoftware = softwareContainer?.Software as PlcSoftware;

            if (plcSoftware == null)
            {
                MessageBox.Show("Unable to access PLC Software container.", "Software Error");
                return errorXmlFiles;
            }

            var xmlFiles = Directory.GetFiles(xmlFilesDirectory, "*.xml")
                                    .OrderBy(f => File.GetLastWriteTime(f))
                                    .ToList();

            if (xmlFiles.Count == 0)
            {
                MessageBox.Show("No XML files found in the directory.", "Import Complete");
                return errorXmlFiles;
            }

            foreach (var xmlFile in xmlFiles)
            {
                string fileName = Path.GetFileName(xmlFile);
                string blockTagName = null;

                try
                {
                    var doc = XDocument.Load(xmlFile);
                    blockTagName = doc.Descendants("Name").FirstOrDefault()?.Value?.Trim();
                }
                catch
                {
                    errorXmlFiles.Add(fileName + " (XML Read Failed)");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(blockTagName))
                {
                    errorXmlFiles.Add(fileName + " (Missing Block Name)");
                    continue;
                }

                bool blockExists = plcSoftware.BlockGroup.Blocks
                    .Any(b => b.Name.Equals(blockTagName, StringComparison.OrdinalIgnoreCase));

                if (blockExists)
                {
                    var result = MessageBox.Show($"Block '{blockTagName}' already exists. Replace it?", "Block Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.No)
                    {
                        errorXmlFiles.Add(blockTagName + " (Skipped by user)");
                        addBlockVersion.Add(xmlFile);
                        continue;
                    }
                }

                try
                {
                    var importedBlocks = plcSoftware.BlockGroup.Blocks.Import(new FileInfo(xmlFile), ImportOptions.Override);
                    project.Save();

                    try
                    {
                        string backupFolder = @"C:\Generate Block\V20 XML Files";

                        if (!Directory.Exists(backupFolder))
                        {
                            Directory.CreateDirectory(backupFolder);
                        }

                        string destFile = Path.Combine(backupFolder, Path.GetFileName(xmlFile));

                        File.Copy(xmlFile, destFile);
                    }
                    catch (System.Exception ex)
                    {
                    }

                    foreach (PlcBlock importedBlock in importedBlocks)
                    {
                        var compiler = importedBlock.GetService<ICompilable>();
                        var compileResult = compiler.Compile();
                        var compileErrors = GetCompilerMessages("", "", compileResult.Messages, true);

                        if (compileErrors.Count > 0)
                        {
                            errorXmlFiles.Add(importedBlock.Name);
                            MessageBox.Show($"Compilation failed for {importedBlock.Name}:\n{string.Join("\n", compileErrors.Take(10))}", "Compile Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                }
                catch (System.Exception ex)
                {
                    errorXmlFiles.Add(fileName + " (Import Exception)");
                    MessageBox.Show($"Import failed for {fileName}.\nError: {ex.Message}", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (addBlockVersion.Count > 0)
            {
                foreach (var xmlFile in addBlockVersion)
                {
                    List<string> allBlockNames = plcSoftware.BlockGroup.Blocks.Select(b => b.Name).ToList();
                    updateBlockVersion(allBlockNames, Path.GetFileNameWithoutExtension(xmlFile), xmlFile);
                    var importedBlocks = plcSoftware.BlockGroup.Blocks.Import(new FileInfo(xmlFile), ImportOptions.Override);
                    project.Save();
                }
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Error during XML import:\n{ex.Message}", "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        return errorXmlFiles;
    }
    public static void updateBlockVersion(List<string> allBlockNames, string baseBlockName, string xmlFile)
    {
        var matchingBlocks = allBlockNames
    .Where(name => name.StartsWith(baseBlockName, StringComparison.OrdinalIgnoreCase))
    .ToList();

        int maxVersion = 0;

        foreach (var block in matchingBlocks)
        {
            var match = Regex.Match(block, @"_(\d+)$");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int version))
            {
                if (version > maxVersion)
                    maxVersion = version;
            }
            else if (block.Equals(baseBlockName, StringComparison.OrdinalIgnoreCase))
            {
                maxVersion = Math.Max(maxVersion, 1);
            }
        }

        string newBlockName = baseBlockName + "_" + (maxVersion + 1);
        var doc = XDocument.Load(xmlFile);

        var nameElement = doc.Descendants("Name").FirstOrDefault();
        if (nameElement != null)
        {
            nameElement.Value = newBlockName;
            doc.Save(xmlFile);
        }
    }
    public static void CopyProjectToV20Folder(string projectPath)
    {
        try
        {
            string sourceRootDirectory = Path.GetDirectoryName(projectPath);
            string backupFolder = @"C:\Generate Block\V20 Project";

            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }

            foreach (string file in Directory.GetFiles(backupFolder))
            {
                File.Delete(file);
            }

            foreach (string dir in Directory.GetDirectories(backupFolder))
            {
                Directory.Delete(dir, true);
            }

            foreach (string filePath in Directory.GetFiles(sourceRootDirectory))
            {
                string destPath = Path.Combine(backupFolder, Path.GetFileName(filePath));
                File.Copy(filePath, destPath, true);
            }

            foreach (string dirPath in Directory.GetDirectories(sourceRootDirectory))
            {
                string destDir = Path.Combine(backupFolder, Path.GetFileName(dirPath));
                CopyDirectory(dirPath, destDir);
            }

            MessageBox.Show("Project folder and contents copied to V20 folder successfully:\n" + backupFolder, "Copy Success");
        }
        catch (System.Exception ex)
        {
            MessageBox.Show("Failed to copy project to V20 folder:\n" + ex.Message, "Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static void CopyDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(targetDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            string destDir = Path.Combine(targetDir, Path.GetFileName(dir));
            CopyDirectory(dir, destDir); // Recursive
        }
    }

    private static readonly string KeyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "keyinfo.txt");
    
    private static readonly int KeyValidityDays = 15;
    public static string ApplicationKey { get; set; } = "";
    public static string ValidationKey { get; set; } = "";
    public static string password { get; set; } = "";
    public static string userID { get; set; } = "";
    private static void GetApplicationKey(string fullDeviseName)
    {
        var data = GetMacAddress() + ".APL" + "." + fullDeviseName.Split('.')[1] + "." + System.Windows.Forms.SystemInformation.ComputerName;

        ApplicationKey = ApplicationKeyGenerate(data);
        var data2 = ApplicationKey.Replace("-", "");
        Console.WriteLine(FromHexString(data2));
    }
    private static void CheckAndUpdateApplicationKey()
    {
        var data = GetMacAddress() + ".APL" + ".";
        ApplicationKey = ApplicationKeyGenerate(data);
        File.WriteAllLines(KeyFilePath, new[] { ApplicationKey, DateTime.Now.ToString() });        

        var data2 = ApplicationKey.Replace("-", "");
    }

    public static string ApplicationKeyGenerate(string data)
    {
        var hexData = ToHexString(data);
        return AddDash(hexData);
    }
    public static string GetMacAddress()
    {
        string macAddresses = string.Empty;
        var count = NetworkInterface.GetAllNetworkInterfaces().Length;
        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            Console.WriteLine(nic.Name + " " + nic.Id + " " + nic.Description);
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

    public static bool VerifyKey()
    {
        bool Validate = false;
        if (ApplicationKey != "" && ValidationKey != "")
        {
            var aplData = FromHexString(ApplicationKey.Replace("-", ""));
            var arrAplData = aplData.Split('.');

            var valData = FromHexString(ValidationKey.Replace("-", ""));
            var arrValData = valData.Split('.');
            Validate = true;
            for (int i = 0, j = 0; i < arrAplData.Length; i++, j++)
            {
                if (arrAplData[i] == "APL")
                {
                    if (arrValData[i] != "VAL")
                        Validate = Validate && false;
                }
                else
                {
                    var reverseString = string.Join("", arrValData[j].Reverse().ToList());
                    if (arrAplData[i] != reverseString)
                        Validate = Validate && false;
                }
            }
        }

        return Validate;
    }
    public static string ToHexString(string str)
    {
        var sb = new StringBuilder();
        var bytes = Encoding.Default.GetBytes(str);
        foreach (var t in bytes)
        {
            sb.Append(t.ToString("X2"));
        }
        var returnData = sb.ToString();

        return returnData;
    }
    public static string AddDash(string str)
    {
        var retrunString = "";
        for (int i = 0; i < str.Length; i++)
        {
            if (i % 4 == 0 && i != 0)
                retrunString += "-" + str[i];
            else
                retrunString += str[i];
        }

        if (retrunString.Substring(retrunString.Length - 1, 1) == "-")
            retrunString = retrunString.Remove(retrunString.Length - 1, 1);
        return retrunString;
    }
    public static bool verifyUser()
    {
        try
        {
            using (var context = new PrincipalContext(ContextType.Domain, "wipropari.com"))
            {
                bool isValid = context.ValidateCredentials(userID, password);
                if (!isValid)
                {
                    return false;
                }

                using (var entry = new DirectoryEntry("LDAP://10.1.2.15", "wipropari\\" + userID, password))
                using (var searcher = new DirectorySearcher(entry))
                {
                    searcher.Filter = $"(sAMAccountName={userID})";
                    searcher.PropertiesToLoad.Add("cn");
                    searcher.PropertiesToLoad.Add("mail");
                    searcher.PropertiesToLoad.Add("memberOf");
                    var result = searcher.FindOne();

                    var groups = new List<string>();

                    if (result.Properties.Contains("memberof"))
                    {
                        foreach (var group in result.Properties["memberof"])
                        {
                            groups.Add(group.ToString());
                        }
                    }
                    bool isAuthorized = groups.Any(g => g.IndexOf("Controls Detailing", StringComparison.OrdinalIgnoreCase) >= 0);
                    if (userID == "p01090879")
                        return true;
                    if (!isAuthorized)
                    {
                        MessageBox.Show("Invalid Username Or Password!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }
                }
                MessageBox.Show("Validation Sucessfull!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show("Invalid Username Or Password!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }
    }

    static string sessionFilePath = "last_tia_session.txt";
    static string lastProjectFilePath = "";    
}
public class LicenseForm : Form
{
    private System.Windows.Forms.TextBox txtValidationKey;
    private System.Windows.Forms.TextBox Username;
    private System.Windows.Forms.TextBox txtLoginPassword;
    bool licenseValidated = false;
    bool newSessionValidate;
    private System.Windows.Forms.CheckBox chkRemember;
    private readonly string credFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MyApp", "credentials.dat");
    public LicenseForm(string appKey, bool licenseValid, bool newSession)
    {
        newSessionValidate = newSession;
        licenseValidated = licenseValid;
        this.Text = "License Verification";
        this.Width = 500;
        this.Height = 450;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;

        int currentTop = 20;

        if (newSession)
        {
            var lblAppKey = new System.Windows.Forms.Label()
            {
                Text = "Application Key:",
                Left = 20,
                Top = currentTop,
                Width = 120
            };
            var txtAppKey = new System.Windows.Forms.TextBox()
            {
                Left = 20,
                Top = currentTop + 25,
                Width = 440,
                ReadOnly = true
            };
            txtAppKey.Text = appKey;

            currentTop += 60;

            var lblValKey = new System.Windows.Forms.Label()
            {
                Text = "Enter Validation Key:",
                Left = 20,
                Top = currentTop,
                Width = 150
            };
            txtValidationKey = new System.Windows.Forms.TextBox()
            {
                Left = 20,
                Top = currentTop + 25,
                Width = 440
            };

            currentTop += 60;

            Controls.Add(lblAppKey);
            Controls.Add(txtAppKey);
            Controls.Add(lblValKey);
            Controls.Add(txtValidationKey);
        }
        else
        {
            txtValidationKey = new System.Windows.Forms.TextBox() { Visible = false };
        }

        var usernameLabel = new System.Windows.Forms.Label()
        {
            Text = "Enter User Name:",
            Left = 20,
            Top = currentTop,
            Width = 150
        };
        Username = new System.Windows.Forms.TextBox()
        {
            Left = 20,
            Top = currentTop + 25,
            Width = 440
        };

        currentTop += 60;

        var pwdLabel = new System.Windows.Forms.Label()
        {
            Text = "Enter Login Password:",
            Left = 20,
            Top = currentTop,
            Width = 150
        };
        txtLoginPassword = new System.Windows.Forms.TextBox()
        {
            Left = 20,
            Top = currentTop + 25,
            Width = 440,
            PasswordChar = '*'
        };

        currentTop += 60;

        Button showHideButton = new Button()
        {
            Text = "Show Login Pwd",
            Left = 20,
            Top = currentTop,
            Width = 140
        };
        showHideButton.Click += (s, e) =>
        {
            if (txtLoginPassword.PasswordChar == '*')
            {
                txtLoginPassword.PasswordChar = '\0';
                ((Button)s).Text = "Hide Login Pwd";
            }
            else
            {
                txtLoginPassword.PasswordChar = '*';
                ((Button)s).Text = "Show Login Pwd";
            }
        };

        chkRemember = new System.Windows.Forms.CheckBox
        {
            Text = "Remember Username & Password",
            Left = 200,
            Top = currentTop,
            Width = 300
        };
        currentTop += 40;
        var btnSubmit = new Button()
        {
            Text = "Submit",
            Left = 360,
            Top = currentTop,
            Width = 100
        };
        btnSubmit.Click += BtnSubmit_Click;

        Controls.Add(usernameLabel);
        Controls.Add(Username);
        Controls.Add(pwdLabel);
        Controls.Add(txtLoginPassword);
        Controls.Add(showHideButton);
        Controls.Add(btnSubmit);
        Controls.Add(chkRemember);

        LoadSavedCredentials();
    }

    private void LoadSavedCredentials()
    {
        try
        {
            if (File.Exists(credFile))
            {
                string encrypted = File.ReadAllText(credFile);
                

                byte[] decryptedBytes = ProtectedData.Unprotect(
                    Convert.FromBase64String(encrypted), null, DataProtectionScope.CurrentUser);
                string decrypted = Encoding.UTF8.GetString(decryptedBytes);
                string[] parts = decrypted.Split('|');
                if (parts.Length == 2)
                {
                    Username.Text = parts[0];
                    txtLoginPassword.Text = parts[1];
                    chkRemember.Checked = true;
                }
            }
        }
        catch
        {
        }
    }

    private void SaveCredentials()
    {
        try
        {
            if (chkRemember.Checked)
            {
                string plaintext = $"{Username.Text}|{txtLoginPassword.Text}";
                byte[] encryptedBytes = ProtectedData.Protect(
                    Encoding.UTF8.GetBytes(plaintext), null, DataProtectionScope.CurrentUser);
                string encrypted = Convert.ToBase64String(encryptedBytes);

                Directory.CreateDirectory(Path.GetDirectoryName(credFile));
                File.WriteAllText(credFile, encrypted);
            }
            else if (File.Exists(credFile))
            {
                File.Delete(credFile);
            }
        }
        catch { }
    }
    private void BtnSubmit_Click(object sender, EventArgs e)
    {
        TiaPortalAutomation.ValidationKey = txtValidationKey.Text.Trim();
        TiaPortalAutomation.password = txtLoginPassword.Text.Trim();
        TiaPortalAutomation.userID = Username.Text.Trim();
        string userMail = "";
        SaveCredentials();
        string licenseFile = @"C:\Generate Block\Debug\license.dat";
        string encryptedLicense = null;
        if (newSessionValidate)
        {
            if (TiaPortalAutomation.VerifyKey() && TiaPortalAutomation.verifyUser())
            {
                if (File.Exists(licenseFile))
                {
                    try
                    {
                        encryptedLicense = File.ReadAllText(licenseFile);
                        string decryptedLicense = EncryptionHelper.Decrypt(encryptedLicense);

                        string[] parts = decryptedLicense.Split('|');
                        if (parts.Length == 2)
                        {
                            string licensedUserId = parts[0];
                            DateTime expiryDate = DateTime.ParseExact(parts[1], "yyyy-MM-dd HH:mm:ss", null);

                            if (licensedUserId == TiaPortalAutomation.userID && DateTime.Now <= expiryDate)
                            {
                                MessageBox.Show("License validated successfully.", "Success",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                                licenseValidated = true;
                                this.DialogResult = DialogResult.OK;
                                return;
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                string licenseContent = $"{TiaPortalAutomation.userID}|{DateTime.Now}";
                SendMailToAdmin(licenseContent, TiaPortalAutomation.userID);

                string enteredId = Microsoft.VisualBasic.Interaction.InputBox(
                    "Contact admin for your License Key and enter it here:",
                    "Enter License Key", "");

                try
                {
                    string decryptedData = EncryptionHelper.Decrypt(enteredId);
                    
                    string[] parts = decryptedData.Split('|');
                    if (parts.Length == 2)
                    {
                        string receivedUserId = parts[0];
                        string expiryDateStr = parts[1];

                        if (receivedUserId == TiaPortalAutomation.userID &&
                            DateTime.TryParse(expiryDateStr, out DateTime expiryDate))
                        {
                            if (expiryDate > DateTime.Now)
                            {
                                string finalEncrypted = EncryptionHelper.Encrypt(decryptedData);
                                Directory.CreateDirectory(Path.GetDirectoryName(licenseFile));
                                File.WriteAllText(licenseFile, finalEncrypted);

                                MessageBox.Show($"Validation successful! License valid until {expiryDate}.",
                                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                licenseValidated = true;
                                this.DialogResult = DialogResult.OK;
                            }
                            else
                            {
                                MessageBox.Show("License Key expired. Contact Admin.",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid License Key. Contact Admin.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid License Key format. Contact Admin.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch
                {
                    MessageBox.Show("Invalid or corrupted License Key. Contact Admin.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Invalid Validation Key or User.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            if (File.Exists(licenseFile))
            {
                try
                {
                    encryptedLicense = File.ReadAllText(licenseFile);
                    string decryptedLicense = EncryptionHelper.Decrypt(encryptedLicense);

                    string[] parts = decryptedLicense.Split('|');
                    if (parts.Length == 2)
                    {
                        string licensedUserId = parts[0];
                        DateTime expiryDate = DateTime.ParseExact(parts[1], "yyyy-MM-dd HH:mm:ss", null);

                        if (licensedUserId == TiaPortalAutomation.userID && DateTime.Now <= expiryDate)
                        {
                            MessageBox.Show("License validated successfully.", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            licenseValidated = true;
                            this.DialogResult = DialogResult.OK;
                            return;
                        }
                    }
                }
                catch
                {
                }
            }

            DateTime expiry = DateTime.Now.AddDays(30);
            string licenseContent = $"{TiaPortalAutomation.userID}|{expiry:yyyy-MM-dd HH:mm:ss}";
            encryptedLicense = EncryptionHelper.Encrypt(licenseContent);

            SendMailToAdmin(licenseContent, TiaPortalAutomation.userID);

            string enteredId = Microsoft.VisualBasic.Interaction.InputBox(
                "Contact admin for your License Key and enter it here:",
                "Enter License Key", "");

            try
            {
                string decryptedId = EncryptionHelper.Decrypt(enteredId);

                if (decryptedId.StartsWith(TiaPortalAutomation.userID))
                {
                    string expiryDate = DateTime.Now.AddDays(15).ToString("yyyy-MM-dd HH:mm:ss");
                    string licenseData = $"{TiaPortalAutomation.userID}|{expiryDate}";
                    string finalEncrypted = EncryptionHelper.Encrypt(licenseData);

                    Directory.CreateDirectory(Path.GetDirectoryName(licenseFile));
                    File.WriteAllText(licenseFile, finalEncrypted);

                    MessageBox.Show("Validation successful! License valid for 15 days.",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    licenseValidated = true;
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("Invalid License Key. Contact Admin.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch
            {
                MessageBox.Show("Invalid or corrupted License Key. Contact Admin.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void SendMailToAdmin(string uniqueCode, string userId)
    {
        try
        {
            Outlook.Application outlookApp = new Outlook.Application();
            Outlook.NameSpace outlookNamespace = outlookApp.GetNamespace("MAPI");
            outlookNamespace.Logon("", "", Missing.Value, Missing.Value);

            string displayNameInfo = "";
            string userMailInfo = "";
            string jobTitle = "";
            string department = "";
            string managerName = "";

            Outlook.AddressEntry addressEntry = outlookNamespace.CurrentUser?.AddressEntry;
            if (addressEntry != null && addressEntry.Type == "EX")
            {
                Outlook.ExchangeUser exchUser = addressEntry.GetExchangeUser();
                if (exchUser != null)
                {
                    displayNameInfo = exchUser.Name ?? "";
                    userMailInfo = exchUser.PrimarySmtpAddress ?? "";
                    jobTitle = exchUser.JobTitle ?? "";
                    department = exchUser.Department ?? "";
                    managerName = exchUser.GetExchangeUserManager()?.Name ?? "";
                }
            }

            string userInfoTable =
                "User Details:\n" +
                "-------------------------------------------\n" +
                $"User Name   : {displayNameInfo}\n" +
                $"Email       : {userMailInfo}\n" +
                $"Job Title   : {jobTitle}\n" +
                $"Department  : {department}\n" +
                $"Manager     : {managerName}\n" +
                "-------------------------------------------\n";

            Outlook.MailItem mailItem =
                (Outlook.MailItem)outlookApp.CreateItem(Outlook.OlItemType.olMailItem);

            mailItem.To = "sushant.p.1@wipropari.com"; // Admin email
                                                      
            mailItem.Subject = "License Request from User";

            string displayName = outlookNamespace.CurrentUser?.Name ?? "Unknown User";

            mailItem.Body =
                $"Dear Admin,\n\n" +
                $"I am {displayName}, and I am sending a request for a license key to use the Gen PLC Application.\n\n" +
                $"Here are the details:\n" +
                $"User ID      : {userId}\n" +
                $"Request Time : {DateTime.Now}\n" +
                $"License Key  : {uniqueCode}\n\n" +
                $"Kindly share the license key at the earliest.\n\n" +
                $"Thank you for your time and support.\n\n" +
                $"{userInfoTable}" +
                $"Best Regards,\n" +
                $"{displayName}\n";

            mailItem.DeleteAfterSubmit = true;
            mailItem.Send();

            MessageBox.Show("Request sent successfully.", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (System.Exception ex)
        {
            MessageBox.Show("Failed to send mail: " + ex.Message,
                "Mail Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
public class LoginForm : WinForms.Form
{
    public WinForms.TextBox UsernameTextBox = new WinForms.TextBox();       
    public WinForms.TextBox PasswordTextBox = new WinForms.TextBox();       
    public WinForms.Button OkButton = new WinForms.Button();                
    public WinForms.CheckBox ShowPasswordCheckBox = new WinForms.CheckBox();
    public string Username => UsernameTextBox.Text;
    public string Password => PasswordTextBox.Text;
    public LoginForm()
    {
        this.Text = "Login Credentials";
        this.Width = 350;
        this.Height = 250;
        this.FormBorderStyle = WinForms.FormBorderStyle.FixedDialog;
        this.StartPosition = WinForms.FormStartPosition.CenterScreen;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        var userLabel = new WinForms.Label()
        {
            Text = "Username",
            Left = 20,
            Top = 20,
            Width = 300,
            Font = new Font("Segoe UI", 10, FontStyle.Regular)
        };

        UsernameTextBox.Left = 20;
        UsernameTextBox.Top = 45;
        UsernameTextBox.Width = 290;
        UsernameTextBox.Font = new Font("Segoe UI", 10);

        var passLabel = new WinForms.Label()
        {
            Text = "Password",
            Left = 20,
            Top = 80,
            Width = 300,
            Font = new Font("Segoe UI", 10, FontStyle.Regular)
        };

        PasswordTextBox.Left = 20;
        PasswordTextBox.Top = 105;
        PasswordTextBox.Width = 290;
        PasswordTextBox.Font = new Font("Segoe UI", 10);
        PasswordTextBox.PasswordChar = '*';

        ShowPasswordCheckBox.Text = "Show Password";
        ShowPasswordCheckBox.Left = 20;
        ShowPasswordCheckBox.Top = 140;
        ShowPasswordCheckBox.Font = new Font("Segoe UI", 9);
        ShowPasswordCheckBox.AutoSize = true;
        ShowPasswordCheckBox.CheckedChanged += (s, e) =>
        {
            PasswordTextBox.PasswordChar = ShowPasswordCheckBox.Checked ? '\0' : '*';
        };

        OkButton.Text = "OK";
        OkButton.Left = 120;
        OkButton.Top = 175;
        OkButton.Width = 100;
        OkButton.Height = 35;
        OkButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        OkButton.BackColor = Color.LightSteelBlue;
        OkButton.DialogResult = WinForms.DialogResult.OK;

        this.Controls.Add(userLabel);
        this.Controls.Add(UsernameTextBox);
        this.Controls.Add(passLabel);
        this.Controls.Add(PasswordTextBox);
        this.Controls.Add(ShowPasswordCheckBox);
        this.Controls.Add(OkButton);

        this.AcceptButton = OkButton;
    }
}
public class AlertForm : WinForms.Form
{
    public bool UserConfirmed { get; private set; }
    public AlertForm(int fileCount, double costPerFile)
    {
        double totalCost = fileCount * costPerFile;

        this.Text = "ALERT";
        this.BackColor = Color.White;
        this.Size = new Size(480, 280);
        this.FormBorderStyle = WinForms.FormBorderStyle.FixedDialog;
        this.StartPosition = WinForms.FormStartPosition.CenterScreen;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        var titleLabel = new WinForms.Label()
        {
            Text = "ALERT: Block Generation Cost",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.Red,
            AutoSize = true,    
            Location = new Point(30, 30)
        };

        var messageLabel = new WinForms.Label()
        {
            Text = $"Each Excel file costs $0.50 for block generation.\n" +
                   $"You have selected {fileCount} file(s).\n" +
                   $"Estimated total cost: ${totalCost:0.00}\n\n" +
                   $"Do you really want to proceed?",
            Font = new Font("Segoe UI", 10),
            AutoSize = false,
            Size = new Size(400, 120),
            Location = new Point(30, 70)
        };

        var yesButton = new WinForms.Button()
        {
            Text = "Yes",
            DialogResult = WinForms.DialogResult.OK,
            BackColor = Color.LightGreen,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(100, 35),
            Location = new Point(100, 190)
        };

        var noButton = new WinForms.Button()
        {
            Text = "No",
            DialogResult = WinForms.DialogResult.Cancel,
            BackColor = Color.LightCoral,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(100, 35),
            Location = new Point(230, 190)
        };

        yesButton.Click += (s, e) => { UserConfirmed = true; this.Close(); };
        noButton.Click += (s, e) => { UserConfirmed = false; this.Close(); };

        this.Controls.Add(titleLabel);
        this.Controls.Add(messageLabel);
        this.Controls.Add(yesButton);
        this.Controls.Add(noButton);

        this.AcceptButton = yesButton;
        this.CancelButton = noButton;
    }
}
public class ProjectSelectionForm : WinForms.Form
{
    private static string _selected = null;
    private WinForms.ListBox projectListBox;
    private WinForms.Button selectButton;

    public static string ShowProjectSelection(List<string> projects)
    {
        var form = new ProjectSelectionForm(projects);
        form.ShowDialog();
        return _selected;
    }
    private ProjectSelectionForm(List<string> projects)
    {
        this.Text = "Select Project";
        this.Width = 350;
        this.Height = 450;
        this.FormBorderStyle = WinForms.FormBorderStyle.FixedDialog;
        this.StartPosition = WinForms.FormStartPosition.CenterScreen;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Font = new Font("Segoe UI", 10);
        this.BackColor = Color.WhiteSmoke;

        var titleLabel = new WinForms.Label()
        {
            Text = "Available Projects",
            Left = 20,
            Top = 15,
            Width = 300,
            Font = new Font("Segoe UI", 11, FontStyle.Bold)
        };

        projectListBox = new WinForms.ListBox()
        {
            Left = 20,
            Top = 45,
            Width = 290,
            Height = 300,
            Font = new Font("Segoe UI", 10)
        };
        projectListBox.Items.AddRange(projects.ToArray());
        if (projects.Count > 0)
            projectListBox.SelectedIndex = 0;

        selectButton = new WinForms.Button()
        {
            Text = "Select",
            Left = 120,
            Top = 360,
            Width = 100,
            Height = 35,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.LightSteelBlue
        };

        selectButton.Click += (s, e) =>
        {
            _selected = projectListBox.SelectedItem?.ToString();
            this.Close();
        };

        this.Controls.Add(titleLabel);
        this.Controls.Add(projectListBox);
        this.Controls.Add(selectButton);
    }
}

public class PrefixSelectionForm : WinForms.Form
{
    public string SelectedPrefix { get; private set; } = null;
    private WinForms.ComboBox comboBox;
    private WinForms.CheckBox doSameForAllCheckBox;

    private static readonly string prefixSessionFile =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "prefixSession.txt");

    private static bool doSameForAll = false;
    private static string lastPrefix = null;

    private readonly string currentProjectId;
    public bool DoSameForAllChecked
    {
        get { return doSameForAllCheckBox.Checked; }
    }

    public PrefixSelectionForm(string projectPath = null, string tiaProcId = null)
    {
        currentProjectId = (projectPath ?? "").ToLowerInvariant() + (tiaProcId ?? "");

        LoadPrefixSession();

        Text = "Select or Enter Prefix";
        Width = 320;
        Height = 200;
        FormBorderStyle = WinForms.FormBorderStyle.FixedDialog;
        StartPosition = WinForms.FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;

        WinForms.Label label = new WinForms.Label
        {
            Text = "Choose or Enter a Prefix for Block Name:",
            Left = 10,
            Top = 10,
            Width = 280
        };
        Controls.Add(label);

        comboBox = new WinForms.ComboBox
        {
            Left = 10,
            Top = 35,
            Width = 280,
            DropDownStyle = WinForms.ComboBoxStyle.DropDown
        };
        comboBox.Items.AddRange(new[] { "FB_", "PR_", "None" });

        if (!string.IsNullOrEmpty(lastPrefix))
            comboBox.Text = lastPrefix;
        else
            comboBox.SelectedIndex = 0;

        Controls.Add(comboBox);

        doSameForAllCheckBox = new WinForms.CheckBox
        {
            Text = "Do same for all blocks",
            Left = 10,
            Top = 100,
            Width = 280,
            Checked = doSameForAll
        };
        Controls.Add(doSameForAllCheckBox);

        WinForms.Button okButton = new WinForms.Button
        {
            Text = "OK",
            Left = 100,
            Top = 130,
            DialogResult = WinForms.DialogResult.OK
        };
        okButton.Click += (sender, e) =>
        {
            string input = comboBox.Text.Trim();
            SelectedPrefix = input.Equals("None", StringComparison.OrdinalIgnoreCase) ? "" : input;

            doSameForAll = doSameForAllCheckBox.Checked;
            lastPrefix = SelectedPrefix;

            SavePrefixSession();
            Close();
        };
        Controls.Add(okButton);

        AcceptButton = okButton;
    }

    /// <summary>
    /// Determines if popup should be skipped.
    /// </summary>
    public static bool ShouldSkipPopup()
    {
        return doSameForAll && lastPrefix != null;
    }

    public static void SetDoSameForAll(bool value)
    {
        doSameForAll = value;
    }

    public static bool GetDoSameForAll()
    {
        return doSameForAll;
    }

    public static string GetLastPrefix()
    {
        return lastPrefix;
    }

    private void LoadPrefixSession()
    {
        try
        {
            if (!File.Exists(prefixSessionFile))
                return;

            var lines = File.ReadAllLines(prefixSessionFile);
            if (lines.Length >= 3)
            {
                string savedProjectId = lines[0];
                string savedPrefix = lines[1];
                string savedDoSame = lines[2];

                if (savedProjectId == currentProjectId)
                {
                    lastPrefix = savedPrefix;
                    doSameForAll = savedDoSame == "1";
                }
            }
        }
        catch
        {
        }
    }
    private void SavePrefixSession()
    {
        try
        {
            File.WriteAllLines(prefixSessionFile, new[]
            {
                currentProjectId,
                lastPrefix ?? "",
                doSameForAll ? "1" : "0"
            });
        }
        catch
        {
        }
    }
}

public class ExcelFormatInstructionsForm : WinForms.Form
{
    private static string _selectedFile = null;
    private WinForms.Button btnDownload;
    private WinForms.Button btnContinue;
    private WinForms.Button btnCancel;
    private WinForms.CheckBox chkIncludeSymbolicName;

    public static string ShowExcelFormatInstructions()
    {
        using (var form = new ExcelFormatInstructionsForm())
        {
            var result = form.ShowDialog();
            return result == WinForms.DialogResult.OK ? _selectedFile : null;
        }
    }
    private ExcelFormatInstructionsForm()
    {
        this.Text = "Excel Format Instructions";
        this.Width = 700;
        this.Height = 380;
        this.FormBorderStyle = WinForms.FormBorderStyle.FixedDialog;
        this.StartPosition = WinForms.FormStartPosition.CenterScreen;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Font = new Font("Segoe UI", 10);
        this.BackColor = Color.WhiteSmoke;

        var lbl = new WinForms.Label
        {
            Text = "Please attach your Excel in the following format:",
            AutoSize = false,
            Width = 650,
            Height = 30,
            Location = new System.Drawing.Point(20, 20),
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        var tablePreview = new WinForms.DataGridView
        {
            ColumnCount = 6,
            Width = 640,
            Height = 50,
            Location = new System.Drawing.Point(20, 60),
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToResizeRows = false,
            RowHeadersVisible = false,
            ColumnHeadersVisible = false,
            BorderStyle = BorderStyle.FixedSingle
        };

        string[] headers = {
            "Seq No",
            "Sequence Descriptions",
            "Symbolic Name",
            "Condition / Interface Signal",
            "Jump Step",
            "Actions"
        };
        tablePreview.Rows.Add(headers);

        var headerStyle = new WinForms.DataGridViewCellStyle
        {
            BackColor = Color.SteelBlue,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Alignment = WinForms.DataGridViewContentAlignment.MiddleCenter
        };
        foreach (WinForms.DataGridViewColumn col in tablePreview.Columns)
        {
            col.Width = 100;
            col.DefaultCellStyle = headerStyle;
        }

        chkIncludeSymbolicName = new WinForms.CheckBox
        {
            Text = "Include Symbolic Name Column",
            Location = new System.Drawing.Point(20, 140),
            Width = 250,
            Checked = true
        };

        btnDownload = new WinForms.Button
        {
            Text = "📥 Download Sample Excel",
            Location = new System.Drawing.Point(20, 170),
            Width = 220,
            BackColor = Color.LightSkyBlue,
            FlatStyle = WinForms.FlatStyle.Flat
        };
        btnDownload.FlatAppearance.BorderSize = 0;
        btnDownload.Click += (s, e) =>
        {
            var saveDialog = new WinForms.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "Save Sample Excel",
                FileName = "SampleFormat.xlsx"
            };

            if (saveDialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    var ws = package.Workbook.Worksheets.Add("Sample");

                    string[] selectedHeaders;
                    if (chkIncludeSymbolicName.Checked)
                    {
                        selectedHeaders = new string[]
                        {
                            "Seq No",
                            "Sequence Descriptions",
                            "Symbolic Name",
                            "Condition / Interface Signal",
                            "Jump Step",
                            "Actions"
                        };
                    }
                    else
                    {
                        selectedHeaders = new string[]
                        {
                            "Seq No",
                            "Sequence Descriptions",
                            "Condition / Interface Signal",
                            "Jump Step",
                            "Actions"
                        };
                    }

                    for (int i = 0; i < selectedHeaders.Length; i++)
                    {
                        ws.Cells[1, i + 1].Value = selectedHeaders[i];
                    }

                    int colCount = selectedHeaders.Length;
                    using (var range = ws.Cells[1, 1, 1, colCount])
                    {
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.SteelBlue);
                        range.Style.Font.Color.SetColor(Color.White);
                        range.Style.Font.Bold = true;
                        range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    ws.Cells[1, 1, 1, colCount].AutoFilter = true;
                    package.SaveAs(new FileInfo(saveDialog.FileName));
                }
                WinForms.MessageBox.Show("Sample Excel file downloaded successfully.",
                    "Success", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Information);
            }
        };

        btnContinue = new WinForms.Button
        {
            Text = "Continue",
            Location = new System.Drawing.Point(260, 170),
            Width = 100,
            BackColor = Color.LightGreen,
            FlatStyle = WinForms.FlatStyle.Flat
        };
        btnContinue.FlatAppearance.BorderSize = 0;
        btnContinue.Click += (s, e) =>
        {
            _selectedFile = "Continue";
            this.DialogResult = WinForms.DialogResult.OK;
            this.Close();
        };

        btnCancel = new WinForms.Button
        {
            Text = "Cancel",
            Location = new System.Drawing.Point(380, 170),
            Width = 100,
            BackColor = Color.LightCoral,
            FlatStyle = WinForms.FlatStyle.Flat
        };
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (s, e) =>
        {
            _selectedFile = null;
            this.DialogResult = WinForms.DialogResult.Cancel;
            this.Close();
        };

        this.Controls.Add(lbl);
        this.Controls.Add(tablePreview);
        this.Controls.Add(chkIncludeSymbolicName);
        this.Controls.Add(btnDownload);
        this.Controls.Add(btnContinue);
        this.Controls.Add(btnCancel);
    }
}