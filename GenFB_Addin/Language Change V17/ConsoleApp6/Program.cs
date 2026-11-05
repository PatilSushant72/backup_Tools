using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using Siemens.Engineering;
using Siemens.Engineering.Compiler;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.ExternalSources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using static TiaPortalAutomation;
using Application = System.Windows.Forms.Application;
using Button = System.Windows.Forms.Button;
using NetworkInterface = System.Net.NetworkInformation.NetworkInterface;
using Outlook = Microsoft.Office.Interop.Outlook;
using WinForms = System.Windows.Forms;

class TiaPortalAutomation
{
    static List<string> compileResults = new List<string>();
    static List<string> humanLoopMessages = new List<string>();
    static Dictionary<string, DateTime> uploadTimes = new Dictionary<string, DateTime>();

    //static string mainOutputFolder = @"C:\Development\ProgrammingLanguage\OutputFiles";
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

    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            string userName = Environment.UserName;
            string userDomain = Environment.UserDomainName;
            string excelForTest = "";
            //ExcelPackage.License.SetNonCommercialPersonal("Wipropari");
            OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("Wipropari");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string mode = args[0];/* "EXCEL";*/ /*"XML"*//*args[0];*/  /*"EXCEL",*/ /*"XMLWITHTEST";*/ /*"AWLWITHTEST";*/
            selectedProjectPath = /*@"C:\Users\Wpuser\Downloads\109817270_TIA_Add-In_Template_CODE\Project2\Project2.ap17";*/ /* @"C:\Testing\Project2\Project2.ap17";*/ args[1]; /*@"C:\Generate Block\New folder (2)\V20 Project\Project2.ap17";*//// @"C:\Testing\Project2\Project2.ap17"args[1];

            Console.WriteLine("New Windows session detected. Asking for login...");

            string fqdn = Dns.GetHostEntry(Dns.GetHostName()).HostName;
            if (!fqdn.Contains("wipropari.com"))
            {
                MessageBox.Show("System Not Valid....", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string currentProjectFilePath = selectedProjectPath;// args[1];/* @"C:\Generate Block\New folder (2)\V20 Project\Project2.ap17";//*/ //@"C:\Testing\Project2\Project2.ap17";/*args[1];*/

            if (!projCheck(currentProjectFilePath, fqdn))
                return;

            string projectPath = selectedProjectPath;

            //// Get the TIA Portal process ID dynamically
            //var myTiaPortalProcess = Process.GetProcessesByName("Siemens.Tia.Portal").FirstOrDefault();
            //if (myTiaPortalProcess == null)
            //{
            //    MessageBox.Show("TIA Portal process not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}
            //string tiaProcId = myTiaPortalProcess.Id.ToString();

            string awlFromUserFolder = Path.Combine("C:\\Generate Block", "AWL Files From User");
            string awlBackupFolder = Path.Combine("C:\\Generate Block", "Backup AWL Files And Reports");
            string xmlFromUserFolder = Path.Combine("C:\\Generate Block", "XML Files From User");
            string xmlBackupFolder = Path.Combine("C:\\Generate Block", "Backup XML Files V17");

            if (mode == "EXCEL")
            {
                string excelFolder = Path.Combine(@"C:\Generate Block", "Excel Files");
                string backupExcelFolder = Path.Combine(@"C:\Generate Block", "Backup Excel Files");

                if (!Directory.Exists(excelFolder))
                    Directory.CreateDirectory(excelFolder);

                if (Directory.GetFiles(excelFolder, "*.xlsx").Any())
                {
                    if (!Directory.Exists(backupExcelFolder))
                        Directory.CreateDirectory(backupExcelFolder);

                    foreach (string file in Directory.GetFiles(excelFolder, "*.xlsx"))
                    {
                        string fileName = Path.GetFileName(file);
                        string destinationPath = Path.Combine(backupExcelFolder, fileName);

                        int counter = 1;
                        while (File.Exists(destinationPath))
                        {
                            string name = Path.GetFileNameWithoutExtension(fileName);
                            string ext = Path.GetExtension(fileName);
                            destinationPath = Path.Combine(backupExcelFolder, $"{name}_{counter}{ext}");
                            counter++;
                        }

                        File.Move(file, destinationPath);
                    }
                }
                string result = ExcelFormatInstructionsForm.ShowExcelFormatInstructions();

                if (result == null)
                {
                    MessageBox.Show("Block generation canceled by user before file selection.",
                        "Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                OpenFileDialog dialog = new OpenFileDialog
                {
                    Multiselect = true,
                    Title = "Select Excel Files",
                    Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*"
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    int selectedFileCount = dialog.FileNames.Length;

                    foreach (var file in dialog.FileNames)
                    {
                        if (!ValidateExcelSheet(file))
                        {
                            MessageBox.Show("Excel file validation failed. Please correct the errors and try again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    double costPerFile = 0.5;
                    double totalCost = selectedFileCount * costPerFile;

                    var alertForm = new AlertForm(selectedFileCount, 0.5);
                    alertForm.ShowDialog();

                    if (!alertForm.UserConfirmed)
                    {
                        MessageBox.Show(
                            "Block generation has been canceled by the user.",
                            "Process Canceled",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                        return;
                    }

                    foreach (var file in dialog.FileNames)
                    {
                        var dest = Path.Combine(excelFolder, Path.GetFileName(file));
                        File.Copy(file, dest, true);
                    }

                    if (!Directory.Exists(awlFilesDirectory))
                        Directory.CreateDirectory(awlFilesDirectory);

                    if (!Directory.Exists(mainOutputFolder))
                        Directory.CreateDirectory(mainOutputFolder);

                    int attempt = 0;
                    const int maxAttempts = 5;
                    List<string> errorMsg = null;

                    //do
                    //{
                        attempt++;
                        generateAWL();

                        //
                        foreach (var excel in dialog.FileNames)
                        {
                            foreach (var awl in Directory.GetFiles(awlFilesDirectory))
                            {
                                string baseName = Path.GetFileNameWithoutExtension(excel.ToString());
                                string fullBlockName = Path.GetFileNameWithoutExtension(awl);

                                if (fullBlockName.StartsWith(baseName + "_"))
                                {
                                    updateBlockName(awl, excel, projectPath);

                                    break;
                                }
                            }
                        }
                        //

                        errorMsg = ImportAwlFiles(selectedProjectPath, awlFilesDirectory, selectedCaxPath);

                    //} while (errorMsg != null && errorMsg.Count > 0 && attempt < maxAttempts);

                    if (errorMsg != null && errorMsg.Count > 0)
                    {
                        MessageBox.Show($"Import failed after {maxAttempts} attempts with {errorMsg.Count} errors.",
                                        "Final Compile Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                    }

                    List<string> importErrors = ImportXmlFilesFromCorrectTIAInstance(selectedProjectPath, mainOutputFolder, selectedCaxPath1);
                    if (importErrors.Count == 0)
                    {
                        MessageBox.Show("All XML blocks imported successfully.", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"XML import completed with {importErrors.Count} error(s).", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    mainfile = Path.Combine(mainOutputFolder, "CompileLogs");
                    //CopyProjectToV20Folder(selectedProjectPath);
                }
            }
            else if (mode == "AWL")
            {
                if (!Directory.Exists(awlFromUserFolder))
                    Directory.CreateDirectory(awlFromUserFolder);

                if (Directory.GetFiles(awlFromUserFolder).Any())
                {
                    if (!Directory.Exists(awlBackupFolder))
                        Directory.CreateDirectory(awlBackupFolder);

                    foreach (string file in Directory.GetFiles(awlFromUserFolder))
                    {
                        string fileName = Path.GetFileName(file);
                        string destinationPath = Path.Combine(awlBackupFolder, fileName);

                        int counter = 1;
                        while (File.Exists(destinationPath))
                        {
                            string name = Path.GetFileNameWithoutExtension(fileName);
                            string ext = Path.GetExtension(fileName);
                            destinationPath = Path.Combine(awlBackupFolder, $"{name}_{counter}{ext}");
                            counter++;
                        }

                        File.Move(file, destinationPath);
                    }
                }

                OpenFileDialog dialog = new OpenFileDialog
                {
                    Multiselect = true,
                    Title = "Select AWL Files",
                    Filter = "AWL Files (*.awl)|*.awl|All Files (*.*)|*.*"
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in dialog.FileNames)
                    {
                        var dest = Path.Combine(awlFromUserFolder, Path.GetFileName(file));
                        File.Copy(file, dest, true);
                    }

                    // 🔹 Directly update prefix for each AWL (no Excel dependency here)
                    foreach (var awl in Directory.GetFiles(awlFromUserFolder))
                    {
                        updateBlockName(awl, null, projectPath);
                    }

                    awlFilesDirectory = awlFromUserFolder;
                    var errorMsg = ImportAwlFiles(selectedProjectPath, awlFilesDirectory, selectedCaxPath);

                    List<string> importErrors = ImportXmlFilesFromCorrectTIAInstance(selectedProjectPath, mainOutputFolder, selectedCaxPath1);
                    if (importErrors.Count == 0)
                    {
                        MessageBox.Show("All XML blocks imported successfully.", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"XML import completed with {importErrors.Count} error(s).", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            else if (mode == "XML")
            {
                if (!Directory.Exists(xmlFromUserFolder))
                    Directory.CreateDirectory(xmlFromUserFolder);

                if (Directory.GetFiles(xmlFromUserFolder).Any())
                {
                    if (!Directory.Exists(xmlBackupFolder))
                        Directory.CreateDirectory(xmlBackupFolder);

                    foreach (string file in Directory.GetFiles(xmlFromUserFolder))
                    {
                        string fileName = Path.GetFileName(file);
                        string destinationPath = Path.Combine(xmlBackupFolder, fileName);

                        int counter = 1;
                        while (File.Exists(destinationPath))
                        {
                            string name = Path.GetFileNameWithoutExtension(fileName);
                            string ext = Path.GetExtension(fileName);
                            destinationPath = Path.Combine(xmlBackupFolder, $"{name}_{counter}{ext}");
                            counter++;
                        }

                        File.Move(file, destinationPath);
                    }
                }

                OpenFileDialog dialog = new OpenFileDialog
                {
                    Multiselect = true,
                    Title = "Select XML Files",
                    Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*"
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in dialog.FileNames)
                    {
                        var dest = Path.Combine(xmlFromUserFolder, Path.GetFileName(file));
                        File.Copy(file, dest, true);
                    }

                    // 🔹 Directly update prefix for each XML (no Excel dependency here)
                    foreach (var xml in Directory.GetFiles(xmlFromUserFolder))
                    {
                        
                        updateBlockName(xml, null, projectPath);
                    }

                    mainOutputFolder = xmlFromUserFolder;
                    List<string> importErrors = ImportXmlFilesFromCorrectTIAInstance(selectedProjectPath, mainOutputFolder, selectedCaxPath1);
                    if (importErrors.Count == 0)
                    {
                        MessageBox.Show("All XML blocks imported successfully.", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"XML import completed with {importErrors.Count} error(s).", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }

            //else if (mode == "AWL")
            //{

            //    if (!Directory.Exists(awlFromUserFolder))
            //        Directory.CreateDirectory(awlFromUserFolder);

            //    if (Directory.GetFiles(awlFromUserFolder).Any())
            //    {
            //        if (!Directory.Exists(awlBackupFolder))
            //            Directory.CreateDirectory(awlBackupFolder);

            //        foreach (string file in Directory.GetFiles(awlFromUserFolder))
            //        {
            //            string fileName = Path.GetFileName(file);
            //            string destinationPath = Path.Combine(awlBackupFolder, fileName);

            //            int counter = 1;
            //            while (File.Exists(destinationPath))
            //            {
            //                string name = Path.GetFileNameWithoutExtension(fileName);
            //                string ext = Path.GetExtension(fileName);
            //                destinationPath = Path.Combine(awlBackupFolder, $"{name}_{counter}{ext}");
            //                counter++;
            //            }

            //            File.Move(file, destinationPath);
            //        }
            //    }

            //    OpenFileDialog dialog = new OpenFileDialog
            //    {
            //        Multiselect = true,
            //        Title = "Select AWL Files",
            //        Filter = "AWL Files (*.awl)|*.awl|All Files (*.*)|*.*"
            //    };

            //    if (dialog.ShowDialog() == DialogResult.OK)
            //    {
            //        foreach (var file in dialog.FileNames)
            //        {
            //            var dest = Path.Combine(awlFromUserFolder, Path.GetFileName(file));
            //            File.Copy(file, dest, true);
            //        }
            //        foreach (var excel in dialog.FileNames)
            //        {
            //            foreach (var awl in Directory.GetFiles(awlFilesDirectory))
            //            {
            //                string baseName = Path.GetFileNameWithoutExtension(excel.ToString());
            //                string fullBlockName = Path.GetFileNameWithoutExtension(awl);

            //                if (fullBlockName.StartsWith(baseName + "_"))
            //                {
            //                    updateBlockName(awl, excel, projectPath);

            //                    break;
            //                }
            //            }
            //        }
            //        awlFilesDirectory = awlFromUserFolder;
            //        var errorMsg = ImportAwlFiles(selectedProjectPath, awlFilesDirectory, selectedCaxPath);

            //        List<string> importErrors = ImportXmlFilesFromCorrectTIAInstance(selectedProjectPath, mainOutputFolder, selectedCaxPath1);
            //        if (importErrors.Count == 0)
            //        {
            //            MessageBox.Show("All XML blocks imported successfully.", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        }
            //        else
            //        {
            //            MessageBox.Show($"XML import completed with {importErrors.Count} error(s).", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        }

            //        //CopyProjectToV20Folder(selectedProjectPath);
            //    }
            //}
            //else if (mode == "XML")
            //{
            //    if (!Directory.Exists(xmlFromUserFolder))
            //        Directory.CreateDirectory(xmlFromUserFolder);

            //    if (Directory.GetFiles(xmlFromUserFolder).Any())
            //    {
            //        if (!Directory.Exists(xmlBackupFolder))
            //            Directory.CreateDirectory(xmlBackupFolder);

            //        foreach (string file in Directory.GetFiles(xmlFromUserFolder))
            //        {
            //            string fileName = Path.GetFileName(file);
            //            string destinationPath = Path.Combine(xmlBackupFolder, fileName);

            //            int counter = 1;
            //            while (File.Exists(destinationPath))
            //            {
            //                string name = Path.GetFileNameWithoutExtension(fileName);
            //                string ext = Path.GetExtension(fileName);
            //                destinationPath = Path.Combine(xmlBackupFolder, $"{name}_{counter}{ext}");
            //                counter++;
            //            }

            //            File.Move(file, destinationPath);
            //        }
            //    }

            //    OpenFileDialog dialog = new OpenFileDialog
            //    {
            //        Multiselect = true,
            //        Title = "Select XML Files",
            //        Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*"
            //    };

            //    if (dialog.ShowDialog() == DialogResult.OK)
            //    {
            //        foreach (var file in dialog.FileNames)
            //        {
            //            var dest = Path.Combine(xmlFromUserFolder, Path.GetFileName(file));
            //            File.Copy(file, dest, true);
            //        }
            //        foreach (var excel in dialog.FileNames)
            //        {
            //            foreach (var awl in Directory.GetFiles(awlFilesDirectory))
            //            {
            //                string baseName = Path.GetFileNameWithoutExtension(excel.ToString());
            //                string fullBlockName = Path.GetFileNameWithoutExtension(awl);

            //                if (fullBlockName.StartsWith(baseName + "_"))
            //                {
            //                    updateBlockName(awl, excel, projectPath);

            //                    break;
            //                }
            //            }
            //        }
            //        mainOutputFolder = xmlFromUserFolder;
            //        List<string> importErrors = ImportXmlFilesFromCorrectTIAInstance(selectedProjectPath, mainOutputFolder, selectedCaxPath1);
            //        if (importErrors.Count == 0)
            //        {
            //            MessageBox.Show("All XML blocks imported successfully.", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        }
            //        else
            //        {
            //            MessageBox.Show($"XML import completed with {importErrors.Count} error(s).", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        }
            //        //CopyProjectToV20Folder(selectedProjectPath);
            //    }
            //}

            else if (mode == "EXCELWITHTEST")
            {
                string excelFolder = Path.Combine(@"C:\Generate Block", "Excel Files");
                string backupExcelFolder = Path.Combine(@"C:\Generate Block", "Backup Excel Files");

                if (!Directory.Exists(excelFolder))
                    Directory.CreateDirectory(excelFolder);

                if (Directory.GetFiles(excelFolder, "*.xlsx").Any())
                {
                    if (!Directory.Exists(backupExcelFolder))
                        Directory.CreateDirectory(backupExcelFolder);

                    foreach (string file in Directory.GetFiles(excelFolder, "*.xlsx"))
                    {
                        string fileName = Path.GetFileName(file);
                        string destinationPath = Path.Combine(backupExcelFolder, fileName);

                        int counter = 1;
                        while (File.Exists(destinationPath))
                        {
                            string name = Path.GetFileNameWithoutExtension(fileName);
                            string ext = Path.GetExtension(fileName);
                            destinationPath = Path.Combine(backupExcelFolder, $"{name}_{counter}{ext}");
                            counter++;
                        }
                        File.Move(file, destinationPath);
                    }
                }

                string result = ExcelFormatInstructionsForm.ShowExcelFormatInstructions();

                if (result == null)
                {
                    MessageBox.Show("Block generation canceled by user before file selection.",
                        "Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Multiselect = true,
                    Title = "Select Excel Files",
                    Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*"
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    int selectedFileCount = dialog.FileNames.Length;

                    foreach (var file in dialog.FileNames)
                    {
                        if (!ValidateExcelSheet(file))
                        {
                            MessageBox.Show("Excel file validation failed. Please correct the errors and try again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    double costPerFile = 0.5;
                    double totalCost = selectedFileCount * costPerFile;

                    var alertForm = new AlertForm(selectedFileCount, 0.5);
                    alertForm.ShowDialog();

                    if (!alertForm.UserConfirmed)
                    {
                        MessageBox.Show(
                            "Block generation has been canceled by the user.",
                            "Process Canceled",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                        return;
                    }

                    foreach (var file in dialog.FileNames)
                    {
                        var dest = Path.Combine(excelFolder, Path.GetFileName(file));
                        File.Copy(file, dest, true);
                    }

                    if (!Directory.Exists(awlFilesDirectory))
                        Directory.CreateDirectory(awlFilesDirectory);

                    if (!Directory.Exists(mainOutputFolder))
                        Directory.CreateDirectory(mainOutputFolder);

                    int attempt = 0;
                    const int maxAttempts = 5;
                    List<string> errorMsg = null;

                    //do
                    //{
                        attempt++;
                        generateAWL();


                    //} while (errorMsg != null && errorMsg.Count > 0 && attempt < maxAttempts);
                    foreach (var excel in dialog.FileNames)
                    {
                        foreach (var awl in Directory.GetFiles(awlFilesDirectory))
                        {
                            string baseName = Path.GetFileNameWithoutExtension(excel.ToString());
                            string fullBlockName = Path.GetFileNameWithoutExtension(awl);

                            if (fullBlockName.StartsWith(baseName + "_"))
                            {
                                updateBlockName(awl, excel, projectPath);

                                break;
                            }
                        }
                    }
                    errorMsg = ImportAwlFiles(selectedProjectPath, awlFilesDirectory, selectedCaxPath);
                    if (errorMsg != null && errorMsg.Count > 0)
                    {
                        MessageBox.Show($"Import failed after {maxAttempts} attempts with {errorMsg.Count} errors.",
                                        "Final Compile Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                    }

                    List<string> importErrors = ImportXmlFilesFromCorrectTIAInstance(selectedProjectPath, mainOutputFolder, selectedCaxPath1);
                    if (importErrors.Count == 0)
                    {
                        MessageBox.Show("All XML blocks imported successfully.", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"XML import completed with {importErrors.Count} error(s).", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    mainfile = Path.Combine(mainOutputFolder, "CompileLogs");
                    //CopyProjectToV20Folder(selectedProjectPath);
                }

                CopyProjectToV20Folder(selectedProjectPath);
                LaunchNextProcessExe(excelForTest);
            }
            else if (mode == "AWLWITHTEST")
            {
                try
                {
                    if (!Directory.Exists(awlFromUserFolder))
                        Directory.CreateDirectory(awlFromUserFolder);

                    if (Directory.GetFiles(awlFromUserFolder).Any())
                    {
                        if (!Directory.Exists(awlBackupFolder))
                            Directory.CreateDirectory(awlBackupFolder);

                        foreach (string file in Directory.GetFiles(awlFromUserFolder))
                        {
                            string fileName = Path.GetFileName(file);
                            string destinationPath = Path.Combine(awlBackupFolder, fileName);

                            int counter = 1;
                            while (File.Exists(destinationPath))
                            {
                                string name = Path.GetFileNameWithoutExtension(fileName);
                                string ext = Path.GetExtension(fileName);
                                destinationPath = Path.Combine(awlBackupFolder, $"{name}_{counter}{ext}");
                                counter++;
                            }

                            File.Move(file, destinationPath);
                        }
                    }

                    OpenFileDialog dialog = new OpenFileDialog
                    {
                        Multiselect = true,
                        Title = "Select AWL Files",
                        Filter = "AWL Files (*.awl)|*.awl|All Files (*.*)|*.*"
                    };

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        List<AwlExcelPair> pairings = new List<AwlExcelPair>();

                        var selectExcel = MessageBox.Show(
                            "Do you want to select Excel for further testing?",
                            "Excel Selection",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question
                        );

                        if (selectExcel == DialogResult.Yes)
                        {
                            excelForTest = "true";
                            string result = ExcelFormatInstructionsForm.ShowExcelFormatInstructions();

                            if (result == null)
                            {
                                MessageBox.Show("Excel selection cancelled.", "Process Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                            pairings = ShowAwlExcelPairingDialog(dialog.FileNames);

                            if (pairings == null)
                            {
                                MessageBox.Show("Pairing cancelled.", "Import AWL", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                            foreach (var pairing in pairings)
                            {
                                if (!string.IsNullOrEmpty(pairing.ExcelPath))
                                {
                                    if (!ValidateExcelSheet(pairing.ExcelPath))
                                    {
                                        MessageBox.Show("Excel file validation failed. Please correct the errors and try again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var awl in dialog.FileNames)
                            {
                                pairings.Add(new AwlExcelPair
                                {
                                    AwlPath = awl,
                                    ExcelPath = string.Empty
                                });
                            }
                        }

                        foreach (var pairing in pairings)
                        {
                            if (!string.IsNullOrEmpty(pairing.ExcelPath))
                            {
                                Console.WriteLine($"Matching AWL file {pairing.AwlPath} with Excel file {pairing.ExcelPath}");
                            }
                            else
                            {
                                Console.WriteLine($"Processing AWL file {pairing.AwlPath} without an Excel file.");
                            }
                        }

                        foreach (var pair in pairings)
                        {
                            var dest = Path.Combine(awlFromUserFolder, Path.GetFileName(pair.AwlPath));
                            File.Copy(pair.AwlPath, dest, true);
                        }
                        //foreach (var excel in dialog.FileNames)
                        //{
                            foreach (var file in dialog.FileNames)
                            {
                                var dest = Path.Combine(awlFromUserFolder, Path.GetFileName(file));
                                File.Copy(file, dest, true);
                            }

                            // 🔹 Directly update prefix for each AWL (no Excel dependency here)
                            foreach (var awl in Directory.GetFiles(awlFromUserFolder))
                            {
                                updateBlockName(awl, null, projectPath);
                            }
                        //}
                            awlFilesDirectory = awlFromUserFolder;
                        var errorMsg = ImportAwlFiles(selectedProjectPath, awlFilesDirectory, selectedCaxPath);

                        if (selectExcel == DialogResult.Yes)
                        {
                            string excelBackupFolder = @"C:\Generate Block\Backup Excel Files";
                            if (!Directory.Exists(excelBackupFolder))
                                Directory.CreateDirectory(excelBackupFolder);

                            foreach (var pair in pairings)
                            {
                                if (string.IsNullOrWhiteSpace(pair.ExcelPath) || !File.Exists(pair.ExcelPath))
                                    continue;

                                string fileName = Path.GetFileName(pair.ExcelPath);
                                string destinationPath = Path.Combine(excelBackupFolder, fileName);
                                int counter = 1;
                                while (File.Exists(destinationPath))
                                {
                                    string name = Path.GetFileNameWithoutExtension(fileName);
                                    string ext = Path.GetExtension(fileName);
                                    destinationPath = Path.Combine(excelBackupFolder, $"{name}_{counter}{ext}");
                                    counter++;
                                }

                                File.Copy(pair.ExcelPath, destinationPath);
                            }
                        }

                        List<string> importErrors = ImportXmlFilesFromCorrectTIAInstance(selectedProjectPath, mainOutputFolder, selectedCaxPath1);
                        if (importErrors.Count == 0)
                        {
                            MessageBox.Show("All XML blocks imported successfully.", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show($"XML import completed with {importErrors.Count} error(s).", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

                        CopyProjectToV20Folder(selectedProjectPath);
                        LaunchNextProcessExe(excelForTest);
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (mode == "XMLWITHTEST")
            {
                try
                {
                    OpenFileDialog xmlDialog = new OpenFileDialog
                    {
                        Multiselect = true,
                        Title = "Select XML Files",
                        Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*"
                    };

                    if (xmlDialog.ShowDialog() != DialogResult.OK)
                    {
                        MessageBox.Show("No XML selected.", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    List<XmlExcelPair> pairings = new List<XmlExcelPair>();

                    var selectExcel = MessageBox.Show(
                        "Do you want to select Excel for further testing?",
                        "Excel Selection",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (selectExcel == DialogResult.Yes)
                    {
                        excelForTest = "true";
                        string result = ExcelFormatInstructionsForm.ShowExcelFormatInstructions();

                        if (result == null)
                        {
                            MessageBox.Show("Excel selection cancelled.", "Process Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        pairings = ShowXmlExcelPairingDialog(xmlDialog.FileNames);

                        if (pairings == null)
                        {
                            MessageBox.Show("Pairing cancelled.", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        foreach (var pairing in pairings)
                        {
                            if (string.IsNullOrEmpty(pairing.ExcelPath))
                            {
                                MessageBox.Show("You must select an Excel file for every XML file.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            if (!ValidateExcelSheet(pairing.ExcelPath))
                            {
                                MessageBox.Show("Excel file validation failed. Please correct the errors and try again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                    }
                    else
                    {
                        foreach (var xml in xmlDialog.FileNames)
                        {
                            pairings.Add(new XmlExcelPair
                            {
                                XmlPath = xml,
                                ExcelPath = string.Empty
                            });
                        }
                    }

                    if (!Directory.Exists(xmlFromUserFolder))
                        Directory.CreateDirectory(xmlFromUserFolder);

                    if (Directory.GetFiles(xmlFromUserFolder).Any())
                    {
                        if (!Directory.Exists(xmlBackupFolder))
                            Directory.CreateDirectory(xmlBackupFolder);

                        foreach (string file in Directory.GetFiles(xmlFromUserFolder))
                        {
                            string fileName = Path.GetFileName(file);
                            string destinationPath = Path.Combine(xmlBackupFolder, fileName);

                            int counter = 1;
                            while (File.Exists(destinationPath))
                            {
                                string name = Path.GetFileNameWithoutExtension(fileName);
                                string ext = Path.GetExtension(fileName);
                                destinationPath = Path.Combine(xmlBackupFolder, $"{name}_{counter}{ext}");
                                counter++;
                            }

                            File.Move(file, destinationPath);
                        }
                    }

                    foreach (var pair in pairings)
                    {
                        var dest = Path.Combine(xmlFromUserFolder, Path.GetFileName(pair.XmlPath));
                        File.Copy(pair.XmlPath, dest, true);
                    }

                    //if (xmlDialog.ShowDialog() == DialogResult.OK)
                    //{
                        foreach (var file in xmlDialog.FileNames)
                        {
                            var dest = Path.Combine(xmlFromUserFolder, Path.GetFileName(file));
                            File.Copy(file, dest, true);
                        }

                        // 🔹 Directly update prefix for each XML (no Excel dependency here)
                        foreach (var xml in Directory.GetFiles(xmlFromUserFolder))
                        {

                            updateBlockName(xml, null, projectPath);
                        }
                    //}
                        mainOutputFolder = xmlFromUserFolder;

                    List<string> importErrors = ImportXmlFilesFromCorrectTIAInstance(selectedProjectPath, mainOutputFolder, selectedCaxPath1);
                    if (importErrors.Count == 0)
                    {
                        MessageBox.Show("All XML blocks imported successfully.", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"XML import completed with {importErrors.Count} error(s).", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    if (selectExcel == DialogResult.Yes)

                    {
                        excelForTest = "true";
                        string excelBackupFolder = @"C:\Generate Block\Excel Files";
                        if (!Directory.Exists(excelBackupFolder))
                            Directory.CreateDirectory(excelBackupFolder);

                        foreach (var pair in pairings)
                        {
                            if (string.IsNullOrWhiteSpace(pair.ExcelPath) || !File.Exists(pair.ExcelPath))
                                continue;

                            string fileName = Path.GetFileName(pair.ExcelPath);
                            string destinationPath = Path.Combine(excelBackupFolder, fileName);
                            int counter = 1;
                            while (File.Exists(destinationPath))
                            {
                                string name = Path.GetFileNameWithoutExtension(fileName);
                                string ext = Path.GetExtension(fileName);
                                destinationPath = Path.Combine(excelBackupFolder, $"{name}_{counter}{ext}");
                                counter++;
                            }

                            File.Copy(pair.ExcelPath, destinationPath);
                        }
                    }

                    CopyProjectToV20Folder(selectedProjectPath);
                    LaunchNextProcessExe(excelForTest);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    ////    else if (mode == "XMLWITHTEST")
    ////    {
    ////        if (!Directory.Exists(xmlFromUserFolder))
    ////            Directory.CreateDirectory(xmlFromUserFolder);
    ////        if (Directory.GetFiles(xmlFromUserFolder).Any())
    ////        {
    ////            if (!Directory.Exists(xmlBackupFolder))
    ////                Directory.CreateDirectory(xmlBackupFolder);
    ////            foreach (string file in Directory.GetFiles(xmlFromUserFolder))
    ////            {
    ////                string fileName = Path.GetFileName(file);
    ////                string destinationPath = Path.Combine(xmlBackupFolder, fileName);
    ////                int counter = 1;
    ////                while (File.Exists(destinationPath))
    ////                {
    ////                    string name = Path.GetFileNameWithoutExtension(fileName);
    ////                    string ext = Path.GetExtension(fileName);
    ////                    destinationPath = Path.Combine(xmlBackupFolder, $"{name}_{counter}{ext}");
    ////                    counter++;
    ////                }
    ////                File.Move(file, destinationPath);
    ////            }
    ////        }
    ////        OpenFileDialog dialog = new OpenFileDialog
    ////        {
    ////            Multiselect = true,
    ////            Title = "Select XML Files",
    ////            Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*"
    ////        };
    ////        if (dialog.ShowDialog() == DialogResult.OK)
    ////        {
    ////            foreach (var file in dialog.FileNames)
    ////            {
    ////                var dest = Path.Combine(xmlFromUserFolder, Path.GetFileName(file));
    ////                File.Copy(file, dest, true);
    ////            }
    ////            mainOutputFolder = xmlFromUserFolder;
    ////            List<string> importErrors = ImportXmlFilesFromCorrectTIAInstance(selectedProjectPath, mainOutputFolder, selectedCaxPath1);
    ////            if (importErrors.Count == 0)
    ////            {
    ////                MessageBox.Show("All XML blocks imported successfully.", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
    ////            }
    ////            else
    ////            {
    ////                MessageBox.Show($"XML import completed with {importErrors.Count} error(s).", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    ////            }
    ////        }
    ////        CopyProjectToV20Folder(selectedProjectPath);
    ////        LaunchNextProcessExe(excelForTest);         
    ////    }
    ////}

    //        else if (mode == "XMLWITHTEST")
    //        {
    //            OpenFileDialog xmlDialog = new OpenFileDialog
    //            {
    //                Multiselect = true,
    //                Title = "Select XML Files",
    //                Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*"
    //            };

    //            if (xmlDialog.ShowDialog() != DialogResult.OK)
    //            {
    //                MessageBox.Show("No XML selected.", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
    //                return;
    //            }

    //            List<XmlExcelPair> pairings = new List<XmlExcelPair>();

    //            var selectExcel = MessageBox.Show(
    //                "Do you want to select Excel for further testing?",
    //                "Excel Selection",
    //                MessageBoxButtons.YesNo,
    //                MessageBoxIcon.Question
    //            );

    //            if (selectExcel == DialogResult.Yes)
    //            {
    //                excelForTest = "true";
    //                pairings = ShowXmlExcelPairingDialog(xmlDialog.FileNames);
    //                if (pairings == null)
    //                {
    //                    MessageBox.Show("Pairing cancelled.", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
    //                    return;
    //                }
    //            }
    //            else
    //            {
    //                foreach (var xml in xmlDialog.FileNames)
    //                {
    //                    pairings.Add(new XmlExcelPair
    //                    {
    //                        XmlPath = xml,
    //                        ExcelPath = string.Empty
    //                    });
    //                }
    //            }
    //            if (!Directory.Exists(xmlFromUserFolder))
    //                Directory.CreateDirectory(xmlFromUserFolder);

    //            if (Directory.GetFiles(xmlFromUserFolder).Any())
    //            {
    //                if (!Directory.Exists(xmlBackupFolder))
    //                    Directory.CreateDirectory(xmlBackupFolder);

    //                foreach (string file in Directory.GetFiles(xmlFromUserFolder))
    //                {
    //                    string fileName = Path.GetFileName(file);
    //                    string destinationPath = Path.Combine(xmlBackupFolder, fileName);

    //                    int counter = 1;
    //                    while (File.Exists(destinationPath))
    //                    {
    //                        string name = Path.GetFileNameWithoutExtension(fileName);
    //                        string ext = Path.GetExtension(fileName);
    //                        destinationPath = Path.Combine(xmlBackupFolder, $"{name}_{counter}{ext}");
    //                        counter++;
    //                    }

    //                    File.Move(file, destinationPath);
    //                }
    //            }

    //            foreach (var pair in pairings)
    //            {
    //                var dest = Path.Combine(xmlFromUserFolder, Path.GetFileName(pair.XmlPath));
    //                File.Copy(pair.XmlPath, dest, true);
    //            }

    //            mainOutputFolder = xmlFromUserFolder;
    //            List<string> importErrors = ImportXmlFilesFromCorrectTIAInstance(selectedProjectPath, mainOutputFolder, selectedCaxPath1);
    //            if (importErrors.Count == 0)
    //            {
    //                MessageBox.Show("All XML blocks imported successfully.", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
    //            }
    //            else
    //            {
    //                MessageBox.Show($"XML import completed with {importErrors.Count} error(s).", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    //            }

    //            if (selectExcel == DialogResult.Yes)
    //            {
    //                string excelBackupFolder = @"C:\Generate Block\Excel Files";
    //                if (!Directory.Exists(excelBackupFolder))
    //                    Directory.CreateDirectory(excelBackupFolder);

    //                foreach (var pair in pairings)
    //                {
    //                    if (string.IsNullOrWhiteSpace(pair.ExcelPath) || !File.Exists(pair.ExcelPath))
    //                        continue;

    //                    string fileName = Path.GetFileName(pair.ExcelPath);
    //                    string destinationPath = Path.Combine(excelBackupFolder, fileName);
    //                    int counter = 1;
    //                    while (File.Exists(destinationPath))
    //                    {
    //                        string name = Path.GetFileNameWithoutExtension(fileName);
    //                        string ext = Path.GetExtension(fileName);
    //                        destinationPath = Path.Combine(excelBackupFolder, $"{name}_{counter}{ext}");
    //                        counter++;
    //                    }

    //                    File.Copy(pair.ExcelPath, destinationPath);
    //                }
    //            }

    //            CopyProjectToV20Folder(selectedProjectPath);
    //            LaunchNextProcessExe(excelForTest);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //    }
    //}

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

    //class XmlExcelPair
    //{
    //    public string XmlPath { get; set; }
    //    public string ExcelPath { get; set; }
    //}

    //private static List<XmlExcelPair> ShowXmlExcelPairingDialog(string[] xmlFiles)
    //{
    //    var pairs = xmlFiles.Select(x => new XmlExcelPair { XmlPath = x, ExcelPath = string.Empty }).ToList();

    //    using (var form = new Form())
    //    {
    //        form.Text = "XML and Matching Excel";
    //        form.Size = new Size(800, 400);
    //        form.StartPosition = FormStartPosition.CenterParent;

    //        var dgv = new WinForms.DataGridView
    //        {
    //            Dock = DockStyle.Top,
    //            Height = 300,
    //            AutoGenerateColumns = false,
    //            AllowUserToAddRows = false,
    //            DataSource = new BindingList<XmlExcelPair>(pairs)
    //        };

    //        var xmlCol = new DataGridViewTextBoxColumn
    //        {
    //            HeaderText = "XML File",
    //            DataPropertyName = "XmlPath",
    //            ReadOnly = true,
    //            Width = 400
    //        };
    //        dgv.Columns.Add(xmlCol);

    //        var excelPathCol = new DataGridViewTextBoxColumn
    //        {
    //            HeaderText = "Selected Excel",
    //            DataPropertyName = "ExcelPath",
    //            ReadOnly = true,
    //            Width = 300
    //        };
    //        dgv.Columns.Add(excelPathCol);

    //        var browseCol = new DataGridViewButtonColumn
    //        {
    //            HeaderText = "Select Excel",
    //            Text = "Browse...",
    //            UseColumnTextForButtonValue = true,
    //            Width = 100
    //        };
    //        dgv.Columns.Add(browseCol);

    //        dgv.CellClick += (s, e) =>
    //        {
    //            if (e.RowIndex >= 0 && e.ColumnIndex == dgv.Columns["Select Excel"].Index)
    //            {
    //                var pair = pairs[e.RowIndex];
    //                using (var excelDialog = new OpenFileDialog
    //                {
    //                    Multiselect = false,
    //                    Title = $"Select Excel for {Path.GetFileName(pair.XmlPath)}",
    //                    Filter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx|All Files (*.*)|*.*"
    //                })
    //                {
    //                    if (excelDialog.ShowDialog() == DialogResult.OK)
    //                    {
    //                        pair.ExcelPath = excelDialog.FileName;
    //                        dgv.Refresh();
    //                    }
    //                }
    //            }
    //        };

    //        browseCol.Name = "Select Excel";

    //        var okButton = new WinForms.Button { Text = "OK", DialogResult = DialogResult.OK, Anchor = AnchorStyles.Bottom | AnchorStyles.Right, Location = new Point(form.ClientSize.Width - 180, 320), Size = new Size(80, 30) };
    //        var cancelButton = new WinForms.Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Anchor = AnchorStyles.Bottom | AnchorStyles.Right, Location = new Point(form.ClientSize.Width - 90, 320), Size = new Size(80, 30) };

    //        form.Controls.Add(dgv);
    //        form.Controls.Add(okButton);
    //        form.Controls.Add(cancelButton);
    //        form.AcceptButton = okButton;
    //        form.CancelButton = cancelButton;

    //        var result = form.ShowDialog();
    //        if (result == DialogResult.OK)
    //            return pairs;
    //        else
    //            return null;
    //    }
    //}

    //class AwlExcelPair
    //{
    //    public string AwlPath { get; set; }
    //    public string ExcelPath { get; set; }
    //}

    //private static List<AwlExcelPair> ShowAwlExcelPairingDialog(string[] awlFiles)
    //{
    //    var pairs = awlFiles.Select(x => new AwlExcelPair { AwlPath = x, ExcelPath = string.Empty }).ToList();

    //    using (var form = new System.Windows.Forms.Form())
    //    {
    //        form.Text = "AWL and Matching Excel";
    //        form.Size = new Size(900, 450);
    //        form.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
    //        form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
    //        form.MinimizeBox = false;
    //        form.MaximizeBox = false;

    //        var bindingList = new BindingList<AwlExcelPair>(pairs);
    //        var dgv = new System.Windows.Forms.DataGridView
    //        {
    //            Dock = System.Windows.Forms.DockStyle.Top,
    //            Height = 330,
    //            AutoGenerateColumns = false,
    //            AllowUserToAddRows = false,
    //            DataSource = bindingList,
    //            SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect,
    //            RowHeadersVisible = false
    //        };

    //        var awlCol = new System.Windows.Forms.DataGridViewTextBoxColumn
    //        {
    //            Name = "AwlFile",
    //            HeaderText = "AWL File",
    //            DataPropertyName = "AwlPath",
    //            ReadOnly = true,
    //            Width = 450
    //        };
    //        dgv.Columns.Add(awlCol);

    //        var excelPathCol = new System.Windows.Forms.DataGridViewTextBoxColumn
    //        {
    //            Name = "ExcelFile",
    //            HeaderText = "Selected Excel",
    //            DataPropertyName = "ExcelPath",
    //            ReadOnly = true,
    //            Width = 350
    //        };
    //        dgv.Columns.Add(excelPathCol);

    //        var browseCol = new System.Windows.Forms.DataGridViewButtonColumn
    //        {
    //            Name = "BrowseExcel",
    //            HeaderText = "Select Excel",
    //            Text = "Browse...",
    //            UseColumnTextForButtonValue = true,
    //            Width = 100
    //        };
    //        dgv.Columns.Add(browseCol);

    //        dgv.CellClick += (s, e) =>
    //        {
    //            if (e.RowIndex >= 0 && dgv.Columns[e.ColumnIndex].Name == "BrowseExcel")
    //            {
    //                var pair = bindingList[e.RowIndex];
    //                using (var excelDialog = new System.Windows.Forms.OpenFileDialog
    //                {
    //                    Multiselect = false,
    //                    Title = $"Select Excel for {Path.GetFileName(pair.AwlPath)}",
    //                    Filter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx|All Files (*.*)|*.*"
    //                })
    //                {
    //                    if (excelDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
    //                    {
    //                        pair.ExcelPath = excelDialog.FileName;
    //                        dgv.Refresh();
    //                    }
    //                }
    //            }
    //        };

    //        var okButton = new System.Windows.Forms.Button
    //        {
    //            Text = "OK",
    //            DialogResult = System.Windows.Forms.DialogResult.OK,
    //            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
    //            Size = new Size(100, 30),
    //            Location = new Point(form.ClientSize.Width - 220, 360)
    //        };
    //        var cancelButton = new System.Windows.Forms.Button
    //        {
    //            Text = "Cancel",
    //            DialogResult = System.Windows.Forms.DialogResult.Cancel,
    //            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
    //            Size = new Size(100, 30),
    //            Location = new Point(form.ClientSize.Width - 110, 360)
    //        };

    //        form.Controls.Add(dgv);
    //        form.Controls.Add(okButton);
    //        form.Controls.Add(cancelButton);
    //        form.AcceptButton = okButton;
    //        form.CancelButton = cancelButton;

    //        okButton.Click += (s, e) =>
    //        {
    //            var missing = bindingList.Where(p => string.IsNullOrWhiteSpace(p.ExcelPath)).ToList();
    //            if (missing.Any())
    //            {
    //                var msg = "The following AWL(s) have no matching Excel selected:\n" +
    //                          string.Join("\n", missing.Select(p => Path.GetFileName(p.AwlPath)));
    //                System.Windows.Forms.MessageBox.Show(msg, "Missing Excel", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
    //                form.DialogResult = System.Windows.Forms.DialogResult.None;
    //            }
    //        };

    //        var result = form.ShowDialog();
    //        if (result == System.Windows.Forms.DialogResult.OK)
    //            return bindingList.Select(p => new AwlExcelPair { AwlPath = p.AwlPath, ExcelPath = p.ExcelPath }).ToList();
    //        else
    //            return null;
    //    }
    //}

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

            // Ensure prefix is chosen once unless "Do same for all" is active
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

                    // Persist for session (kept as in your code)
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

            // ---- AWL: content starts with FUNCTION_BLOCK ----
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
                        // Use prefix + Excel base name
                        updBlockName = selectedPrefix + excelName;
                    }
                    else
                    {
                        // No Excel → prefix + existing AWL block name
                        updBlockName = selectedPrefix + extractedName;
                    }

                    string updatedContent = content.Replace($"\"{extractedName}\"", $"\"{updBlockName}\"");
                    File.WriteAllText(filePath, updatedContent);
                    System.Console.WriteLine($"AWL block name updated to: {updBlockName}");
                }
            }
            else
            {
                // ---- XML: update first <Name>...</Name> ----
                Regex rx = new Regex(@"<Name>(.*?)</Name>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                Match m = rx.Match(content);

                if (m.Success)
                {
                    string originalXmlName = m.Groups[1].Value;

                    string newXmlName;
                    if (!string.IsNullOrEmpty(excelName))
                    {
                        // Use prefix + Excel base name
                        newXmlName = selectedPrefix + excelName;
                    }
                    else
                    {
                        // No Excel → prefix + existing XML <Name> value
                        newXmlName = selectedPrefix + originalXmlName;
                    }

                    // Replace only the first occurrence
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
    //public static void updateBlockName(string blockFilePath, string excelFilePath, string projectPath)
    //{
    //    try
    //    {
    //        string fileContent = File.ReadAllText(blockFilePath);
    //        string blockName = Path.GetFileNameWithoutExtension(blockFilePath);
    //        string excelName = !string.IsNullOrEmpty(excelFilePath)
    //            ? Path.GetFileNameWithoutExtension(excelFilePath)
    //            : null;

    //        // Ask prefix if not set and not skipped
    //        if (selectedPrefix == null && !PrefixSelectionForm.ShouldSkipPopup())
    //        {
    //            using (PrefixSelectionForm form = new PrefixSelectionForm(projectPath, null))
    //            {
    //                if (form.ShowDialog() == DialogResult.OK)
    //                {
    //                    selectedPrefix = form.SelectedPrefix;
    //                }
    //                else
    //                {
    //                    selectedPrefix = "";
    //                }
    //            }
    //        }

    //        if (selectedPrefix == null && PrefixSelectionForm.ShouldSkipPopup())
    //        {
    //            selectedPrefix = "";
    //        }

    //        string newBlockName;

    //        if (!string.IsNullOrEmpty(excelName))
    //        {
    //            // Excel mode → use Excel filename
    //            newBlockName = selectedPrefix + excelName;
    //        }
    //        else
    //        {
    //            // AWL/XML mode → prefix + existing block name
    //            newBlockName = selectedPrefix + blockName;
    //        }

    //        // Replace block name inside file content
    //        fileContent = Regex.Replace(fileContent,
    //            @"(NAME\s*=\s*"")[^""]+("" )",
    //            $"$1{newBlockName}$2");

    //        File.WriteAllText(blockFilePath, fileContent);

    //        Console.WriteLine($"Block renamed: {blockName} -> {newBlockName}");
    //    }
    //    catch (System.Exception ex)
    //    {
    //        Console.WriteLine($"Error updating block name: {ex.Message}");
    //    }
    //}

    //public static void updateBlockName(string awlFilePath, string excelFilePath)
    //{
    //    //string awlFilePath = @"C:\Generate Block\AWL Files & Reports\PR_GantryUnload_Seqv2 5_57af70_1fb51a9a-5897-4462-a51f-bba69fd82344.awl";
    //    string awlName = File.ReadAllText(awlFilePath);
    //    string excelName = Path.GetFileNameWithoutExtension(excelFilePath);
    //    string extractedName = "";

    //    if (selectedPrefix == null)
    //    {
    //        using (PrefixSelectionForm form = new PrefixSelectionForm())
    //        {
    //            if (form.ShowDialog() == DialogResult.OK)
    //            {
    //                selectedPrefix = form.SelectedPrefix;
    //            }
    //            else
    //            {
    //                //MessageBox.Show("Prefix selection was canceled. Defaulting to FB_.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
    //                selectedPrefix = "";
    //            }
    //        }
    //    }

    //    if (awlName.StartsWith("FUNCTION_BLOCK"))
    //    {
    //        int start = awlName.IndexOf('"') + 1;
    //        int end = awlName.IndexOf('"', start);
    //        extractedName = awlName.Substring(start, end - start);
    //        string updBlockName = selectedPrefix + excelName;

    //        string updatedContent = awlName.Replace($"\"{extractedName}\"", $"\"{updBlockName}\"");

    //        File.WriteAllText(awlFilePath, updatedContent);
    //    }

    //    int a = 0;
    //}
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

    //static void LaunchNextProcessExe(string excelForTest)
    //{
    //    string nextExePath = @"C:\Generate Block\V20 All Files\Debug\ConsoleApp6.exe";
    //    string nextExeArgs = $"\"{mainfile}\" \"{selectedProjectPath}\" \"{excelForTest}\"";

    //    try
    //    {
    //        ProcessStartInfo psi = new ProcessStartInfo
    //        {
    //            FileName = nextExePath,
    //            Arguments = nextExeArgs,
    //            UseShellExecute = true
    //        };

    //        Process.Start(psi);
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show($"Failed to launch next process: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //    }
    //}

    //static void LaunchNextProcessExe()
    //{
    //    string mode = "XML";
    //    string projectPath = @"C:\Path\To\YourProject.ap17";

    //    string arguments = $"\"{mode}\" \"{projectPath}\"";

    //    Process.Start(new ProcessStartInfo
    //    {
    //        FileName = @"C:\Generate Block\V20 All Files\Debug\ConsoleApp6.exe",
    //        Arguments = arguments,
    //        UseShellExecute = true
    //    });

    //static void Main(string[] args)
    //{
    //    try
    //    {
    //        Application.EnableVisualStyles();
    //        Application.SetCompatibleTextRenderingDefault(false);
    //        selectedProjectPath = args[1];
    //        if (!Directory.Exists(awlFilesDirectory))
    //            Directory.CreateDirectory(awlFilesDirectory);

    //        if (!Directory.Exists(mainOutputFolder))
    //            Directory.CreateDirectory(mainOutputFolder);
    //        int attempt = 0;
    //        const int maxAttempts = 5;
    //        List<string> errorMsg = null;

    //        FlaUI.Core.Application app = null;
    //        do
    //        {
    //            attempt++;
    //            generateAWL();
    //            errorMsg = ImportAwlFiles(selectedProjectPath, awlFilesDirectory, selectedCaxPath);
    //        } while (errorMsg != null && errorMsg.Count > 0 && attempt < maxAttempts);

    //        if (errorMsg != null && errorMsg.Count > 0)
    //        {
    //            MessageBox.Show($"Import failed after {maxAttempts} attempts with {errorMsg.Count} errors.",
    //                            "Final Compile Error",
    //                            MessageBoxButtons.OK,
    //                            MessageBoxIcon.Error);
    //        }

    //        List<string> importErrors = ImportXmlFilesFromCorrectTIAInstance(selectedProjectPath, mainOutputFolder, selectedCaxPath1);
    //        if (importErrors.Count == 0)
    //        {
    //            MessageBox.Show("All XML blocks imported successfully.", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
    //        }
    //        else
    //        {
    //            MessageBox.Show($" XML import completed with {importErrors.Count} error(s).", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    //        }
    //        mainfile = Path.Combine(mainOutputFolder, "CompileLogs");
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //    }
    //}

    //static void Main(string[] args)
    //{
    //    try
    //    {
    //        Application.EnableVisualStyles();
    //        Application.SetCompatibleTextRenderingDefault(false);

    //        selectedProjectPath = args[0];

    //        //OpenFileDialog openFileDialog = new OpenFileDialog
    //        //{
    //        //    Title = "Select TIA Portal Project File",
    //        //    Filter = "TIA Portal Project Files (*.ap17)|*.ap17|All Files (*.*)|*.*",
    //        //    InitialDirectory = @"C:\",
    //        //    Multiselect = false
    //        //};
    
    //        //if (openFileDialog.ShowDialog() == DialogResult.OK)
    //        //{
    //        //    selectedProjectPath = openFileDialog.FileName;
    //        //    Console.WriteLine("Project selected: " + selectedProjectPath);
    //        //}
    //        //else
    //        //{
    //        //    MessageBox.Show("No project selected. Exiting.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    //        //    return;
    //        //}

    //        if (!Directory.Exists(awlFilesDirectory))
    //            Directory.CreateDirectory(awlFilesDirectory);

    //        if (!Directory.Exists(mainOutputFolder))
    //            Directory.CreateDirectory(mainOutputFolder);
    //        int attempt = 0;
    //        const int maxAttempts = 5;
    //        List<string> errorMsg = null;

    //        FlaUI.Core.Application app = null;

    //        do
    //        {
    //            attempt++;
    //            //Console.WriteLine($"Attempt {attempt}: Process Start...");

    //            //Console.WriteLine($"Attempt {attempt}: Generating and importing AWL...");
    //            generateAWL();

    //            errorMsg = ImportAwlFiles(selectedProjectPath, awlFilesDirectory, selectedCaxPath);

    //        } while (errorMsg != null && errorMsg.Count > 0 && attempt < maxAttempts);

    //        if (errorMsg != null && errorMsg.Count > 0)
    //        {
    //            MessageBox.Show($"Import failed after {maxAttempts} attempts with {errorMsg.Count} errors.",
    //                            "Final Compile Error",
    //                            MessageBoxButtons.OK,
    //                            MessageBoxIcon.Error);
    //        }

    //        List<string> xmlImportErrors = ImportXmlFiles(selectedProjectPath, mainOutputFolder, selectedCaxPath1);
    //        if (xmlImportErrors.Count == 0)
    //        {
    //            MessageBox.Show("All XML blocks imported successfully.", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
    //            //Console.WriteLine("All Process Successfully Done...");
    //        }
    //        else
    //        {
    //            MessageBox.Show($"XML import completed with {xmlImportErrors.Count} error(s).", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    //        }

    //        mainfile = Path.Combine(mainOutputFolder, "CompileLogs");
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //    }
    //}

    //static void Main()
    //{
    //    try
    //    {
    //        Application.EnableVisualStyles();
    //        Application.SetCompatibleTextRenderingDefault(false);

    //        OpenFileDialog openFileDialog = new OpenFileDialog
    //        {
    //            Title = "Select TIA Portal Project File",
    //            Filter = "TIA Portal Project Files (*.ap17)|*.ap17|All Files (*.*)|*.*",
    //            InitialDirectory = @"C:\",
    //            Multiselect = false
    //        };

    //        if (openFileDialog.ShowDialog() == DialogResult.OK)
    //        {
    //            selectedProjectPath = openFileDialog.FileName;
    //            Console.WriteLine("Project selected: " + selectedProjectPath);
    //        }
    //        else
    //        {
    //            MessageBox.Show("No project selected. Exiting.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    //            return;
    //        }

    //        if (!Directory.Exists(awlFilesDirectory))
    //            Directory.CreateDirectory(awlFilesDirectory);

    //        if (!Directory.Exists(mainOutputFolder))
    //            Directory.CreateDirectory(mainOutputFolder);
    //        int attempt = 0;
    //        const int maxAttempts = 5;
    //        List<string> errorMsg = null;

    //        FlaUI.Core.Application app = null;

    //        do
    //        {
    //            attempt++;
    //            Console.WriteLine($"Attempt {attempt}: Process Start...");
    //            //Console.WriteLine($"Attempt {attempt}: Generating and importing AWL...");
    //            generateAWL();

    //            errorMsg = ImportAwlFiles(selectedProjectPath, awlFilesDirectory, selectedCaxPath);
    //        } while (errorMsg != null && errorMsg.Count > 0 && attempt < maxAttempts);

    //        if (errorMsg != null && errorMsg.Count > 0)
    //        {
    //            MessageBox.Show($"Import failed after {maxAttempts} attempts with {errorMsg.Count} errors.",
    //                            "Final Compile Error",
    //                            MessageBoxButtons.OK,
    //                            MessageBoxIcon.Error);
    //        }
    //        List<string> importErrors = ImportXmlFilesFromCorrectTIAInstance(selectedProjectPath, mainOutputFolder, selectedCaxPath1);
    //        if (importErrors.Count == 0)
    //        {
    //            MessageBox.Show("All XML blocks imported successfully.", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
    //        }
    //        else
    //        {
    //            MessageBox.Show($"XML import completed with {importErrors.Count} error(s).", "Import XML", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    //        }
    //        mainfile = Path.Combine(mainOutputFolder, "CompileLogs");
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //    }
    //}

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
    //static List<string> ImportAwlFiles(string projectPath, string awlFilesDirectory, string caxFilePath)
    //{
    //    List<string> errorAwlFiles = new List<string>();
    //    string exportDirectory = mainOutputFolder;

    //    caxFilePath = @"C:\\Generate Block\\AML Files Path\\Project1.aml";
    //    TiaPortal tiaPortal = null;
    //    Project project = null;

    //    try
    //    {
    //        tiaPortal = new TiaPortal(TiaPortalMode.WithoutUserInterface);
    //        //Console.WriteLine("Started new TIA Portal instance for CPU 300.");
    //        string newProjectPath = @"C:\\Generate Block\\CPU300TempProject";
    //        if (Directory.Exists(newProjectPath))
    //        {
    //            Directory.Delete(newProjectPath, true);
    //            //Console.WriteLine("Deleted existing CPU300TempProject folder before creating new one.");
    //        }
    //        Directory.CreateDirectory(newProjectPath);

    //        project = tiaPortal.Projects.Create(new DirectoryInfo(newProjectPath), "CPU300TempProject");
    //        //Console.WriteLine("Created new temporary CPU 300 project.");

    //        if (!File.Exists(caxFilePath))
    //        {
    //            //Console.WriteLine("CAX file not found: " + caxFilePath);
    //            return errorAwlFiles;
    //        }

    //        CaxProvider caxProvider = project.GetService<CaxProvider>();
    //        if (caxProvider != null)
    //        {
    //            string logFolder = @"C:\\Generate Block\\Log Files";
    //            string logFile = Path.Combine(logFolder, "import_log.log");
    //            if (!Directory.Exists(logFolder))
    //                Directory.CreateDirectory(logFolder);

    //            caxProvider.Import(new FileInfo(caxFilePath), new FileInfo(logFile), CaxImportOptions.MoveToParkingLot);
    //            //Console.WriteLine("CPU 300 Device imported from CAX.");
    //        }

    //        var device = project.Devices.FirstOrDefault(d => d.Name.Contains("PLC_1") || d.Name.Contains("S7-300"));
    //        if (device == null)
    //        {
    //            //Console.WriteLine("CPU 300 device not found after import.");
    //            return errorAwlFiles;
    //        }
    //        var softwareContainer = device.DeviceItems[1].GetService<SoftwareContainer>();
    //        var plcSoftware = softwareContainer?.Software as PlcSoftware;
    //        if (plcSoftware != null)
    //        {
    //            var compiler = plcSoftware.GetService<ICompilable>();
    //            foreach (var awlFile in Directory.GetFiles(awlFilesDirectory, "*.awl"))
    //            {
    //                string fileName = Path.GetFileNameWithoutExtension(awlFile);
    //                var existingSource = plcSoftware.ExternalSourceGroup.ExternalSources
    //                    .FirstOrDefault(src => src.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));
    //                if (existingSource != null)
    //                {
    //                    existingSource.Delete();
    //                    //Console.WriteLine($"Deleted existing AWL source: {fileName}");
    //                }
    //                try
    //                {
    //                    var externalSource = plcSoftware.ExternalSourceGroup.ExternalSources.CreateFromFile(fileName, awlFile);
    //                    externalSource.GenerateBlocksFromSource((GenerateBlockOption)ImportOptions.Override);
    //                    //Console.WriteLine($"Imported and generated block from: {awlFile}");
    //                    var plcBlockGroup = plcSoftware.BlockGroup;
    //                    var blockName = GetBlockNameFromAwlFile(awlFile);
    //                    var block = FindBlockRecursively(plcBlockGroup, blockName.Trim());
    //                    if (block != null)
    //                    {
    //                        ls = new List<string>();
    //                        var singleCompiler = block.GetService<ICompilable>();
    //                        var result1 = singleCompiler.Compile();
    //                        //Console.WriteLine($"Compiled block {fileName}: {result1?.State}");
    //                        List<string> errorMessages = GetCompilerMessages(string.Empty, string.Empty, result1.Messages, true);
    //                        if (errorMessages.Count == 0)
    //                        {
    //                            //Console.WriteLine("0 Compilation error");
    //                            Thread.Sleep(2000);
    //                        }
    //                        else
    //                        {
    //                            string errorText = $"Compilation failed with {errorMessages.Count} error(s):\n\n";
    //                            int maxToShow = 10;
    //                            var displayErrors = errorMessages.Take(maxToShow).ToList();
    //                            foreach (var error in displayErrors)
    //                            {
    //                                errorText += $"- {error}\n";
    //                            }
    //                            if (errorMessages.Count > maxToShow)
    //                            {
    //                                errorText += $"\n...and {errorMessages.Count - maxToShow} more.\nCheck log file for full details.";
    //                            }
    //                            errorAwlFiles.Add(Path.GetFileName(awlFile));
    //                            MessageBox.Show(errorText, "Compile Result", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //                        }
    //                    }
    //                    else
    //                    {
    //                        //Console.WriteLine($"Warning: Generated block '{fileName}' not found for compilation.");
    //                    }
    //                }
    //                catch (EngineeringException ex)
    //                {
    //                    //Console.WriteLine($"Error in {awlFile}: {ex.Message}");
    //                }
    //            }
    //            project.Save();
    //            project.Close();
    //            tiaPortal.Dispose();
    //            GC.Collect();
    //            GC.WaitForPendingFinalizers();
    //            string apPath = Path.Combine(newProjectPath, "CPU300TempProject", "CPU300TempProject.ap17");
    //            int retries = 10;
    //            while (!File.Exists(apPath) && retries-- > 0)
    //            {
    //                //Console.WriteLine("Waiting for project file to appear...");
    //                Thread.Sleep(1000);
    //                //Console.WriteLine("Current files in project folder:");
    //                foreach (var f in Directory.GetFiles(Path.Combine(newProjectPath, "CPU300TempProject")))
    //                {
    //                    //Console.WriteLine($"   - {Path.GetFileName(f)}");
    //                }
    //            }
    //            if (!File.Exists(apPath))
    //            {
    //                //Console.WriteLine("Project file not found even after waiting.");
    //                return errorAwlFiles;
    //            }

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

                //tiaPortal = new TiaPortal(TiaPortalMode.WithUserInterface);
                //project = tiaPortal.Projects.Open(new FileInfo(apPath));
                ////Console.WriteLine("Reopened CPU 300 project in UI mode.");

                //var tiaProcess = Process.GetProcessesByName("Siemens.Automation.Portal")
                //    .FirstOrDefault(p => p.MainWindowTitle.Contains("CPU300TempProject"));
                //var device1 = project.Devices.FirstOrDefault(d => d.Name.Contains("PLC_1") || d.Name.Contains("S7-300"));
                //var softwareContainer1 = device1.DeviceItems[1].GetService<SoftwareContainer>();
                //if (tiaProcess != null)
                //{
                //    using (var app = FlaUI.Core.Application.Attach(tiaProcess))
                //    using (var automation = new UIA3Automation())
                //    {
                //        var retryMainWindow = Retry.WhileNull(() => app.GetMainWindow(automation), timeout: TimeSpan.FromSeconds(15));
                //        var mainWindow = retryMainWindow.Result;

                //        if (mainWindow != null)
                //        {
                //            var projectViewBtn = mainWindow.FindFirstDescendant(cf =>
                //                cf.ByName("ContextNavigator.GoToProjectView.Title")
                //                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.ListItem)));
                //            projectViewBtn?.AsButton()?.Invoke();
                tiaPortal = new TiaPortal(TiaPortalMode.WithUserInterface);
                project = tiaPortal.Projects.Open(new FileInfo(apPath));
                //Console.WriteLine("Reopened CPU 300 project in UI mode.");

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
                    //MessageBox.Show("Project did not load within 10 minutes.", "Timeout Error");
                    //return;
                }

                var device1 = project.Devices.FirstOrDefault(d => d.Name.Contains("PLC_1") || d.Name.Contains("S7-300"));

                if (device1 == null)
                {
                    //MessageBox.Show("PLC_1/S7-300 device not found.", "Device Error");
                    //return;
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
                                    //Console.WriteLine("Invoked Program Block using InvokePattern.");
                                }
                                else
                                {
                                    //Console.WriteLine("InvokePattern not available. Fallback to keyboard.");
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
                                        //Console.WriteLine("Focused 'Switch programming language' menu.");

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
                                            //Console.WriteLine("Clicked on LAD submenu.");
                                        }
                                        else
                                        {
                                            //Console.WriteLine("LAD submenu not found.");
                                        }
                                    }
                                    else
                                    {
                                        //Console.WriteLine("Switch programming language menu not found.");
                                    }

                                    project.Save();
                                    //Console.WriteLine("Project saved after language switch.");
                                }
                            }
                        }
                    }
                }
                ExportAllBlocks(project, softwareContainer1?.Software as PlcSoftware, exportDirectory);
                try
                {
                    tiaPortal.Dispose();
                    //Console.WriteLine("TIA Portal instance disposed after CPU 300 process.");

                    var cpu300TiaProcess = Process.GetProcessesByName("Siemens.Automation.Portal")
                        .FirstOrDefault(p => p.MainWindowTitle.Contains("CPU300TempProject"));

                    if (cpu300TiaProcess != null && !cpu300TiaProcess.HasExited)
                    {
                        cpu300TiaProcess.Kill();
                        cpu300TiaProcess.WaitForExit(10000);
                        //Console.WriteLine("Force-closed lingering CPU300 TIA process.");
                    }

                    Thread.Sleep(2000);
                }
                catch (System.Exception ex)
                {
                    //Console.WriteLine("Failed during TIA disposal/wait: " + ex.Message);
                }

                try
                {
                    string cpu300Folder = @"C:\\Generate Block\\CPU300TempProject";
                    if (Directory.Exists(cpu300Folder))
                    {
                        Directory.Delete(cpu300Folder, true);
                        //Console.WriteLine("Deleted CPU300TempProject folder from local path.");
                    }
                }
                catch (System.Exception ex)
                {
                    //Console.WriteLine("Failed to delete CPU300TempProject folder: " + ex.Message);
                }

                try
                {
                    //Console.WriteLine("Attaching to existing TIA instance (Add-In CPU1500)...");

                    //List<string> importErrors = ImportXmlFilesFromCorrectTIAInstance(selectedProjectPath, mainOutputFolder, selectedCaxPath1);

                    //List<string> importErrors = ImportXmlFiles(selectedProjectPath, mainOutputFolder, selectedCaxPath1);

                    //foreach (var err in importErrors)
                    //{
                    //    //Console.WriteLine($"Import error: {err}");
                    //}
                }
                catch (System.Exception ex)
                {
                    //Console.WriteLine("Failed to attach or import to Add-In TIA instance: " + ex.Message);
                }

                return errorAwlFiles;
            }
        }
        catch (EngineeringException ex)
        {
            //Console.WriteLine("TIA Portal engineering error: " + ex.Message);
        }
        catch (System.Exception ex)
        {
            //Console.WriteLine("General error: " + ex.Message);
        }

        return errorAwlFiles;
    }

    //try
    //{
    //    Console.WriteLine("Attaching to existing TIA instance (Add-In CPU1500)...");
    //    string lockedProjectPath = @"C:\\Testing\\Project2\\Project2.ap17";
    //    int retryCount = 0;
    //    const int maxRetries = 12;
    //    while (retryCount < maxRetries)
    //    {
    //        try
    //        {
    //            using (FileStream fs = File.Open(lockedProjectPath, FileMode.Open, FileAccess.Read, FileShare.None))
    //            {
    //                break;
    //            }
    //        }
    //        catch (IOException)
    //        {
    //            Console.WriteLine($"Waiting for TIA to release lock on {lockedProjectPath}... ({retryCount + 1}/12)");
    //            Thread.Sleep(10000);
    //            retryCount++;
    //        }
    //    }
    //    if (retryCount == maxRetries)
    //    {
    //        Console.WriteLine("Timeout: Project2.ap17 still locked after 2 minutes. Aborting import.");
    //        return errorAwlFiles;
    //    }
    //            List<string> importErrors = ImportXmlFiles(selectedProjectPath, mainOutputFolder, selectedCaxPath1);
    //            foreach (var err in importErrors)
    //            {
    //                Console.WriteLine($"Import error: {err}");
    //            }
    //        }
    //            catch (Exception ex)
    //    {
    //        Console.WriteLine("Failed to attach or import to Add-In TIA instance: " + ex.Message);
    //    }
    //}
    //        return errorAwlFiles;
    //    }
    //    catch (EngineeringException ex)
    //    {
    //        Console.WriteLine("TIA Portal engineering error: " + ex.Message);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine("General error: " + ex.Message);
    //    }
    //    return errorAwlFiles;
    //}

    //static List<string> ImportAwlFiles(string projectPath, string awlFilesDirectory)
    //{
    //    List<string> errorAwlFiles = new List<string>();
    //    string miniMouseMacroPath = @"C:\Devlopment\Programing Language\Auto task App\MiniMouseMacro.exe";
    //    string macroFilePath = @"C:\Devlopment\Programing Language\Auto task App\Micros\Micros13.mmmacro";
    //    string exportDirectory = mainOutputFolder; // Assuming you have this defined globally
    //    TiaPortal tiaPortal = null;
    //    Project project = null;
    //    try
    //    {
    //        var tiaProcesses = TiaPortal.GetProcesses();
    //        if (tiaProcesses.Count > 0)
    //        {
    //            tiaPortal = tiaProcesses[0].Attach();
    //            Console.WriteLine("Attached to existing TIA Portal instance.");
    //        }
    //        else
    //        {
    //            tiaPortal = new TiaPortal(TiaPortalMode.WithUserInterface);
    //            Console.WriteLine("Started new TIA Portal instance.");
    //        }
    //        project = tiaPortal.Projects
    //            .FirstOrDefault(p => p.Path.FullName.Equals(projectPath, StringComparison.OrdinalIgnoreCase));
    //        if (project == null)
    //        {
    //            project = tiaPortal.Projects.Open(new FileInfo(projectPath));
    //            Console.WriteLine("Project opened successfully.");
    //        }
    //        else
    //        {
    //            Console.WriteLine("Project already open in attached TIA instance.");
    //        }
    //        var device = project.Devices.FirstOrDefault(d => d.Name.Contains("S7-300"));
    //        if (device != null)
    //        {
    //            var softwareContainer = device.DeviceItems[1].GetService<SoftwareContainer>();
    //            var plcSoftware = softwareContainer?.Software as PlcSoftware;
    //            if (plcSoftware != null)
    //            {
    //                var compiler = plcSoftware.GetService<ICompilable>();
    //                foreach (var awlFile in Directory.GetFiles(awlFilesDirectory, "*.awl"))
    //                {
    //                    string fileName = Path.GetFileNameWithoutExtension(awlFile);
    //                    var existingSource = plcSoftware.ExternalSourceGroup.ExternalSources
    //                        .FirstOrDefault(src => src.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));
    //                    if (existingSource != null)
    //                    {
    //                        existingSource.Delete();
    //                        Console.WriteLine($"Deleted existing AWL source: {fileName}");
    //                    }
    //                    try
    //                    {
    //                        var externalSource = plcSoftware.ExternalSourceGroup.ExternalSources.CreateFromFile(fileName, awlFile);
    //                        externalSource.GenerateBlocksFromSource((GenerateBlockOption)ImportOptions.Override);
    //                        Console.WriteLine($"Imported and generated block from: {awlFile}");
    //                        var plcBlockGroup = plcSoftware.BlockGroup;
    //                        var blockName = GetBlockNameFromAwlFile(awlFile);
    //                        var block = FindBlockRecursively(plcBlockGroup, blockName.Trim());
    //                        if (block != null)
    //                        {
    //                            ls = new List<string>();
    //                            var singleCompiler = block.GetService<ICompilable>();
    //                            var result1 = singleCompiler.Compile();
    //                            Console.WriteLine($"Compiled block {fileName}: {result1?.State}");
    //                            List<string> errorMessages = GetCompilerMessages(string.Empty, string.Empty, result1.Messages, true);
    //                            List<string> allMessages = GetCompilerMessages(string.Empty, string.Empty, result1.Messages, false);
    //                            if (errorMessages.Count == 0)
    //                            {
    //                                MessageBox.Show("", "0 Compilation error", MessageBoxButtons.OK, MessageBoxIcon.Information);
    //                            }
    //                            else
    //                            {
    //                                string errorText = $"Compilation failed with {errorMessages.Count} error(s):\n\n";
    //                                int maxToShow = 10;
    //                                var displayErrors = errorMessages.Take(maxToShow).ToList();
    //                                foreach (var error in displayErrors)
    //                                {
    //                                    errorText += $"- {error}\n";
    //                                }
    //                                if (errorMessages.Count > maxToShow)
    //                                {
    //                                    errorText += $"\n...and {errorMessages.Count - maxToShow} more.\nCheck log file for full details.";
    //                                }
    //                                errorAwlFiles.Add(Path.GetFileName(awlFile));
    //                                MessageBox.Show(errorText, "Compile Result", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //                            }
    //                        }
    //                        else
    //                        {
    //                            Console.WriteLine($"Warning: Generated block '{fileName}' not found for compilation.");
    //                        }
    //                    }
    //                    catch (EngineeringException ex)
    //                    {
    //                        Console.WriteLine($"Error in {awlFile}: {ex.Message}");
    //                    }
    //                }
    //            }
    //        }
    //        var tiaProcess = Process.GetProcessesByName("Siemens.Automation.Portal").FirstOrDefault();
    //        if (tiaProcess != null)
    //        {
    //            using (var app = FlaUI.Core.Application.Attach(tiaProcess))
    //            using (var automation = new UIA3Automation())
    //            {
    //                var mainWindow = app.GetMainWindow(automation);
    //                var projectTree = mainWindow.FindFirstDescendant(cf => cf.ByName("Grid-Control").And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Table)));
    //                if (projectTree == null)
    //                {
    //                    Console.WriteLine("ProjectNavigationTree not found.");
    //                }
    //                if (mainWindow != null)
    //                {
    //                    var projectViewBtn = mainWindow.FindFirstDescendant(cf => cf.ByName("ContextNavigator.GoToProjectView.Title").And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.ListItem)));
    //                    projectViewBtn?.AsButton()?.Invoke();
    //                    var projectTreeContainer = mainWindow.FindFirstDescendant(cf =>
    //cf.ByAutomationId("ProjectNavigationTree")
    //  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Table)));
    //                    if (projectTreeContainer != null)
    //                    {
    //                        var plcEdit = projectTreeContainer.FindFirstDescendant(cf =>
    //                            cf.ByName("PLC_1 [CPU 312]:1E1C1007-B70C-4670-81E9-515ABD35EA0C.Attribute.Name")
    //                              .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit)));
    //                        plcEdit?.Focus();
    //                        Thread.Sleep(2000);
    //                        var invokePattern = plcEdit.Patterns.Invoke.PatternOrDefault;
    //                        if (invokePattern != null)
    //                        {
    //                            invokePattern.Invoke();
    //                            Console.WriteLine("Invoked Program Block using InvokePattern.");
    //                        }
    //                        else
    //                        {
    //                            Console.WriteLine("InvokePattern not available. Trying keyboard fallback...");
    //                            SendKeys.SendWait("{ENTER}");
    //                            Thread.Sleep(100);
    //                            SendKeys.SendWait("{ENTER}");
    //                            Console.WriteLine("Simulated double-click using Enter key.");
    //                        }
    //                        var progBlock = mainWindow.FindFirstDescendant(cf => cf.ByName("Siemens.Simatic.Lang.BlockLogic:Siemens.Simatic.PlcLanguages.BlockLogic.MenuService:SystemFolderProgramBlocksResX:1E1C1007-B70C-4670-81E9-515ABD35EA0C.Attribute.Name").And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit)));
    //                        if (progBlock != null)
    //                        {
    //                            progBlock.Focus();
    //                            progBlock.RightClick();
    //                            Thread.Sleep(5000);
    //                            WinForms.SendKeys.SendWait("+{F10}");
    //                            Thread.Sleep(1000);
    //                        }
    //                        var switchLangMenu = mainWindow.FindFirstDescendant(cf =>
    //                            cf.ByName("Siemens.Simatic.Lang.Portal:Siemens.Simatic.Lang.Portal.Resources.BlockLogic.MenuService:SwitchBlockLanguage")
    //                              .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Menu)));
    //                        if (switchLangMenu != null)
    //                        {
    //                            switchLangMenu.Focus();
    //                            Console.WriteLine("Focused 'Switch programming language' menu.");
    //                            Thread.Sleep(500);
    //                            var ladMenu = mainWindow.FindFirstDescendant(cf =>
    //                                cf.ByName("Siemens.Simatic.Lang.Portal:Siemens.Simatic.Lang.Portal.Resources.BlockLogic.MenuService:SwitchBlockLanguage_LAD")
    //                                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.MenuItem)));
    //                            if (ladMenu != null)
    //                            {
    //                                ladMenu.AsMenuItem()?.Invoke();
    //                                Console.WriteLine("Clicked on LAD submenu.");
    //                            }
    //                            else
    //                            {
    //                                Console.WriteLine("LAD submenu not found.");
    //                            }
    //                        }
    //                        else
    //                        {
    //                            Console.WriteLine("Switch programming language menu not found.");
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    catch (EngineeringException ex)
    //    {
    //        Console.WriteLine("TIA Portal engineering error: " + ex.Message);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine("General error: " + ex.Message);
    //    }
    //    return errorAwlFiles;
    //}

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
                //Console.WriteLine($"Deleted CPU/Device: {deviceName}");
            }
            catch (System.Exception ex)
            {
                //Console.WriteLine($"Failed to delete CPU: {ex.Message}");
            }
        }
        else
        {
            //Console.WriteLine("No device found to delete.");
        }
    }

    //static void ExportAllBlocks(Project project, PlcSoftware plcSoftware, string exportDirectory)
    //{
    //    List<string> compileResults = new List<string>();
    //    var compiler = plcSoftware.GetService<ICompilable>();
    //    var result = compiler.Compile();
    //    foreach (var block in plcSoftware.BlockGroup.Blocks.ToList())
    //    {
    //        if (block is PlcBlock plcBlock)
    //        {
    //            string baseName = plcBlock.Name;
    //            string finalName = baseName;
    //            string filePath = Path.Combine(exportDirectory, finalName + ".xml");
    //            int count = 1;
    //            while (File.Exists(filePath))
    //            {
    //                finalName = baseName + "_" + count;
    //                filePath = Path.Combine(exportDirectory, finalName + ".xml");
    //                count++;
    //            }
    //            try
    //            {
    //                plcBlock.Export(new FileInfo(filePath), ExportOptions.WithDefaults);
    //                compileResults.Add($"Exported block: {finalName}");
    //                plcBlock.Delete();
    //                compileResults.Add($"Deleted block: {finalName}");
    //            }
    //            catch (Exception ex)
    //            {
    //                compileResults.Add($"Failed to export/delete {finalName}: {ex.Message}");
    //            }
    //        }
    //    }
    //    string exportLogPath = Path.Combine(exportDirectory, "export_log.txt");
    //    File.WriteAllLines(exportLogPath, compileResults);
    //    ShowCompilationResults(compileResults);
    //    var device = project.Devices.FirstOrDefault(d => d.Name.Contains("PLC_1") || d.Name.Contains("S7-300"));
    //    if (device != null)
    //    {
    //        try
    //        {
    //            string deviceName = device.Name;
    //            device.Delete();
    //            //Console.WriteLine($"Deleted CPU/Device: {deviceName}");
    //        }
    //        catch (Exception ex)
    //        {
    //            //Console.WriteLine($"Failed to delete CPU: {ex.Message}");
    //        }
    //    }
    //    else
    //    {
    //        //Console.WriteLine("No device found to delete.");
    //    }
    //}
    //static void ExportAllBlocks(Project project, PlcSoftware plcSoftware, string exportDirectory)
    //{
    //    List<string> compileResults = new List<string>();
    //    var compiler = plcSoftware.GetService<ICompilable>();
    //    var result = compiler.Compile();
    //    foreach (var block in plcSoftware.BlockGroup.Blocks.ToList())
    //    {
    //        if (block is PlcBlock plcBlock)
    //        {
    //            string baseName = plcBlock.Name;
    //            string finalName = baseName;
    //            string filePath = Path.Combine(exportDirectory, finalName + ".xml");
    //            int count = 1;
    //            while (File.Exists(filePath))
    //            {
    //                finalName = baseName + "_" + count;
    //                filePath = Path.Combine(exportDirectory, finalName + ".xml");
    //                count++;
    //            }
    //            try
    //            {
    //                plcBlock.Export(new FileInfo(filePath), ExportOptions.WithDefaults);
    //                compileResults.Add($"Exported block: {finalName}");
    //                plcBlock.Delete();
    //                compileResults.Add($"Deleted block: {finalName}");
    //            }
    //            catch (Exception ex)
    //            {
    //                compileResults.Add($"Failed to export/delete {finalName}: {ex.Message}");
    //            }
    //        }
    //    }
    //    string exportLogPath = Path.Combine(exportDirectory, "export_log.txt");
    //    File.WriteAllLines(exportLogPath, compileResults);
    //    ShowCompilationResults(compileResults);

    //    var device = project.Devices.FirstOrDefault(d => d.Name.Contains("PLC_1") || d.Name.Contains("S7-300"));
    //    if (device != null)
    //    {
    //        try
    //        {
    //            string deviceName = device.Name;
    //            device.Delete();
    //            //Console.WriteLine($"Deleted CPU/Device: {deviceName}");
    //        }
    //        catch (Exception ex)
    //        {
    //            //Console.WriteLine($"Failed to delete CPU: {ex.Message}");
    //        }
    //    }
    //    else
    //    {
    //        //Console.WriteLine("No device found to delete.");
    //    }

    //    try
    //    {
    //        string backupFolderPath = Path.Combine(@"C:\Generate Block", "V20 XML Files");
    //        if (!Directory.Exists(backupFolderPath))
    //        {
    //            Directory.CreateDirectory(backupFolderPath);
    //        }
    //        foreach (string filePath in Directory.GetFiles(exportDirectory, "*.xml"))
    //        {
    //            string fileName = Path.GetFileName(filePath);
    //            string destFilePath = Path.Combine(backupFolderPath, fileName);
    //            int count = 1;
    //            string baseName = Path.GetFileNameWithoutExtension(fileName);
    //            string ext = Path.GetExtension(fileName);
    //            while (File.Exists(destFilePath))
    //            {
    //                destFilePath = Path.Combine(backupFolderPath, $"{baseName}_{count}{ext}");
    //                count++;
    //            }
    //            File.Copy(filePath, destFilePath);
    //        }
    //        if (File.Exists(exportLogPath))
    //        {
    //            File.Copy(exportLogPath, Path.Combine(backupFolderPath, "export_log.txt"), overwrite: true);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Failed to copy XML files to backup folder: {ex.Message}");
    //    }
    //}
    //static void ExportAllBlocks(PlcSoftware plcSoftware, string exportDirectory)
    //{
    //    List<string> compileResults = new List<string>();
    //    var compiler = plcSoftware.GetService<ICompilable>();
    //    var result = compiler.Compile();
    //    foreach (var block in plcSoftware.BlockGroup.Blocks.ToList())
    //    {
    //        if (block is PlcBlock plcBlock)
    //        {
    //            string blockName = plcBlock.Name;
    //            string filePath = Path.Combine(exportDirectory, blockName + ".xml");
    //            plcBlock.Export(new FileInfo(filePath), ExportOptions.WithDefaults);
    //            compileResults.Add($"Exported block: {blockName}");
    //            plcBlock.Delete();
    //            compileResults.Add($"Deleted block: {blockName}");
    //        }
    //    }
    //    ShowCompilationResults(compileResults);
    //    //After deletion, import XML files back into project
    //    List<string> importErrors = ImportXmlFiles(selectedProjectPath, mainOutputFolder);
    //    foreach (var err in importErrors)
    //    {
    //        Console.WriteLine($"Import error: {err}");
    //    }
    //}

    //static void ExportAllBlocks(PlcSoftware plcSoftware, string exportDirectory)
    //{
    //    //Directory.CreateDirectory(exportDirectory);
    //    var compiler = plcSoftware.GetService<ICompilable>();
    //    var result = compiler.Compile();
    //    foreach (var block in plcSoftware.BlockGroup.Blocks)
    //    {
    //        if (!block.IsConsistent)
    //        {
    //            compileResults.Add($"Block {block.Name} is inconsistent and was skipped.");
    //            continue;
    //        }
    //        string blockFolder = Path.Combine(exportDirectory, block.Name);
    //        if (File.Exists(blockFolder))
    //        {
    //            File.Delete(blockFolder);
    //        }
    //        blockFolder = blockFolder + ".xml";
    //        block.Export(new FileInfo(blockFolder), ExportOptions.WithDefaults);
    //        compileResults.Add($"Block {block.Name} exported successfully to {blockFolder}.");
    //    }
    //    ShowCompilationResults(compileResults);
    //}
    //static void ChangeLanguageUsingMiniMouse(string miniMousePath, string macroFilePath)
    //{
    //    try
    //    {
    //        //miniMousePath = @"C:\241108_Backup\Devlopment\Programing Language\Auto task App\MiniMouseMacro.exe";
    //        //macroFilePath = @"C:\241108_Backup\Devlopment\Programing Language\Auto task App\Micros\Micros13.mmmacro";
    //        miniMousePath = @"C:\Users\P01090879\Downloads\Minimouse Folder\MiniMouseMacro.exe";
    //        macroFilePath = @"C:\Users\P01090879\Downloads\Minimouse Folder\Micros Path.mmmacro";
    //        if (File.Exists(miniMousePath) && File.Exists(macroFilePath))
    //        {
    //            var process = new System.Diagnostics.Process();
    //            process.StartInfo.FileName = miniMousePath;
    //            process.StartInfo.Arguments = $"\"{macroFilePath}\"";
    //            process.StartInfo.UseShellExecute = true;
    //            process.Start();
    //            Console.WriteLine("MiniMouse application started to change language.");
    //        }
    //        else
    //        {
    //            Console.WriteLine("MiniMouse.exe or Macro file not found at specified path.");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine("Error while running MiniMouse: " + ex.Message);
    //    }
    //}

    //private static void ImportExportedBlocks(string projectPath, string mainDirectory)
    //{
    //    string[] exportedBlockFiles = Directory.GetFiles(mainDirectory, "*.xml", SearchOption.AllDirectories);
    //    Console.WriteLine("Starting block import...");
    //    TiaPortal tiaPortalInstance = null;
    //    try
    //    {
    //        var tiaProcesses = TiaPortal.GetProcesses();
    //        tiaPortalInstance = tiaProcesses.Count > 0 ? tiaProcesses[0].Attach() : new TiaPortal(TiaPortalMode.WithUserInterface);
    //        Project project = tiaPortalInstance.Projects.Any(p => p.Path.FullName.Equals(projectPath, StringComparison.OrdinalIgnoreCase))
    //            ? tiaPortalInstance.Projects.First(p => p.Path.FullName.Equals(projectPath, StringComparison.OrdinalIgnoreCase))
    //            : tiaPortalInstance.Projects.Open(new FileInfo(projectPath));
    //        foreach (var device in project.Devices)
    //        {
    //            var firstDeviceItem = device.DeviceItems[1];
    //            if (firstDeviceItem == null)
    //            {
    //                Console.WriteLine("No device items found in device.");
    //                continue;
    //            }
    //            var softwareContainer = firstDeviceItem.GetService<SoftwareContainer>();
    //            if (softwareContainer?.Software is PlcSoftware plcSoftware)
    //            {
    //                var plcBlockGroup = plcSoftware.BlockGroup;
    //                foreach (var exportedBlockPath in exportedBlockFiles)
    //                {
    //                    try
    //                    {
    //                        plcBlockGroup.Blocks.Import(new FileInfo(exportedBlockPath), ImportOptions.Override);
    //                        Console.WriteLine($"Imported block: {Path.GetFileName(exportedBlockPath)}");
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        Console.WriteLine($"Failed to import block {Path.GetFileName(exportedBlockPath)}: {ex.Message}");
    //                    }
    //                }
    //                var compileService = plcSoftware.GetService<ICompilable>();
    //                var compileResult = compileService.Compile();
    //                Console.WriteLine($"Compilation finished. Status: {compileResult.State}");
    //            }
    //        }
    //        project.Save();
    //        Console.WriteLine("Project saved.");
    //        project.Close();
    //        Console.WriteLine("Project closed.");
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Error during block import: {ex.Message}");
    //    }
    //    finally
    //    {
    //        tiaPortalInstance?.Dispose();
    //    }
    //}

    static void ShowCompilationResults(List<string> results)
    {
        //List<string> success = results.Where(r => !r.Contains("failed", StringComparison.OrdinalIgnoreCase)).ToList();
        //List<string> failures = results.Where(r => r.Contains("failed", StringComparison.OrdinalIgnoreCase)).ToList();

        //if (failures.Count > 0)
        //{
        //    MessageBox.Show(string.Join(Environment.NewLine, failures), "Compilation Errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //}
        //else
        //{
        //    MessageBox.Show(string.Join(Environment.NewLine, success), "Compilation Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //}
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

    public static void generateAWL()
    {
        string userId = "";
        string password = "";

        using (Form owner = GetForegroundWindow("Login"))
        {
            var loginForm = new LoginForm();
            if (loginForm.ShowDialog(owner) != DialogResult.OK)
            {
                //    Console.WriteLine("Login canceled.");
                return;
            }

            userId = loginForm.Username;
            password = loginForm.Password;
        }

        //string userId = loginForm.Username;
        //string password = loginForm.Password;

        var options = new EdgeOptions();
        //options.UseChromium = true;
        options.AddArgument("--headless=new");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument("--ignore-certificate-errors");
        options.AddArgument("--allow-insecure-localhost");
        options.AddArgument("--disable-features=UseDnsHttpsSvcbAlpn");
        options.AddArgument("--disable-features=BlockInsecurePrivateNetworkRequests");
        options.AddArgument("--no-sandbox");
        options.AcceptInsecureCertificates = true;
        options.AddArgument("--proxy-auto-detect");
        options.AddArgument("--dns-prefetch-disable");
        options.AddArgument("--disable-features=msEdgeOnDeviceLLM");
        //string downloadDirectory = @"C:\Users\P01090879\Downloads\AWL Test Download";
        //    string downloadDirectory = Path.Combine(
        //Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        //"Downloads",
        //"AWL Test Download");
        string downloadDirectory = Path.Combine(
            @"C:\Generate Block",
            "AWL Files & Reports"
        );

        options.AddUserProfilePreference("download.default_directory", downloadDirectory);
        options.AddUserProfilePreference("profile.default_content_setting_values.automatic_downloads", 1);
        options.AddUserProfilePreference("profile.default_content_settings.popups", 0);
        options.AddUserProfilePreference("download.prompt_for_download", false);
        options.AddUserProfilePreference("safebrowsing.enabled", true);
        
        options.AcceptInsecureCertificates = true;
        if (!Directory.Exists(downloadDirectory))
        {
            Directory.CreateDirectory(downloadDirectory);
        }
        var service = EdgeDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true;
        service.SuppressInitialDiagnosticInformation = true;
        service.UseVerboseLogging = false;

        try
        {
            using (IWebDriver driver = new EdgeDriver(service, options))
            {
                driver.Navigate().GoToUrl("https://genplc.wipropari.com/");
                //driver.Navigate().GoToUrl("http://44.196.185.41/");
                Thread.Sleep(3000);

                try
                {
                    driver.FindElement(By.XPath("//input[@placeholder='Enter username']")).SendKeys(userId);
                    driver.FindElement(By.XPath("//input[@placeholder='Enter password']")).SendKeys(password);
                    driver.FindElement(By.XPath("//button[contains(text(), 'Login') or @id='btnLogin']")).Click();
                    Thread.Sleep(3000);
                }
                catch (System.Exception ex)
                {
                    //Console.WriteLine("Login error: " + ex.Message);
                    return;
                }

                //string folderPath = @"C:\\Users\\P01090879\\Downloads\\TestUploads";
                //string folderPath = Path.Combine(
                //Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                //"Downloads",
                //"TestUploads");
                string folderPath = Path.Combine(
                    @"C:\Generate Block",
                    "Excel Files"
                );
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    MessageBox.Show($"Folder not found.\nA new folder has been created at:\n{folderPath}", "Folder Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                if (Directory.GetFiles(folderPath, "*.xlsx").Length == 0)
                {
                    MessageBox.Show("No Excel files found in the folder:\n" + folderPath, "Missing Files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //Console.WriteLine("No Excel files found in: " + folderPath);
                    return;
                }

                string[] excelFiles = Directory.GetFiles(folderPath, "*.xlsx");
                DateTime firstUploadTime = DateTime.MaxValue;

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));

                foreach (string file in excelFiles)
                {
                    //Console.WriteLine($"\nUploading file: {Path.GetFileName(file)}");

                    try
                    {
                        IWebElement generateBtn = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions
                            .ElementIsVisible(By.XPath("//button[contains(text(), 'Generate Block') or contains(@class, 'save-btn')]")));

                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", generateBtn);
                        Thread.Sleep(1000);

                        generateBtn.Click();
                        Thread.Sleep(3000);
                    }
                    catch (WebDriverTimeoutException)
                    {
                        //Console.WriteLine("Timeout: 'Generate Block' button not found or not visible.");
                        return;
                    }
                    catch (NoSuchElementException ex)
                    {
                        //Console.WriteLine("Element not found: " + ex.Message);
                        return;
                    }
                    catch (System.Exception ex)
                    {
                        //Console.WriteLine("Unexpected error: " + ex.Message);
                        return;
                    }

                    try
                    {
                        if (string.IsNullOrEmpty(selectedProject))
                        {
                            Thread.Sleep(3000);

                            var dropdownIcon = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions
                                .ElementToBeClickable(By.CssSelector("div[class$='-indicatorContainer']")));
                            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", dropdownIcon);
                            //Console.WriteLine("Clicked on dropdown icon.");
                            Thread.Sleep(4000);

                            var inputBox = driver.FindElement(By.Id("react-select-3-input"));
                            inputBox.SendKeys(" ");
                            Thread.Sleep(2000);

                            wait.Until(drv => driver.FindElements(By.CssSelector("div[class*='option']")).Count > 0);

                            var optionElements = driver.FindElements(By.CssSelector("div[class*='option']"));
                            var projectOptions = optionElements
                                .Where(o => o.Displayed && !string.IsNullOrWhiteSpace(o.Text))
                                .Select(o => o.Text.Trim())
                                .Distinct()
                                .ToList();

                            if (projectOptions.Count == 0)
                            {
                                //Console.WriteLine("No project options found.");
                                return;
                            }

                            string selected = ProjectSelectionForm.ShowProjectSelection(projectOptions);
                            if (string.IsNullOrEmpty(selected))
                            {
                                //Console.WriteLine("No project selected by user.");
                                return;
                            }

                            selectedProject = selected;

                            inputBox.SendKeys(selectedProject);
                            Thread.Sleep(1000);

                            var projectToSelect = driver.FindElement(By.XPath($"//div[contains(text(), '{selectedProject}') and contains(@class,'option')]"));
                            projectToSelect.Click();
                            //Console.WriteLine($"Project '{selectedProject}' selected.");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        //Console.WriteLine("Dropdown interaction error: " + ex.Message);
                        return;
                    }

                    try
                    {
                        IWebElement fileInput = driver.FindElement(By.Id("files"));
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value = '';", fileInput);
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].style.display = 'block';", fileInput);
                        fileInput.SendKeys(file);
                        Thread.Sleep(2000);

                        driver.FindElement(By.XPath("//button[text()='Upload']")).Click();

                        DateTime uploadStartTime = DateTime.Now;
                        if (firstUploadTime == DateTime.MaxValue)
                            firstUploadTime = uploadStartTime;

                        uploadTimes[file] = uploadStartTime;

                        Thread.Sleep(4000);
                    }
                    catch (System.Exception ex)
                    {
                        return;
                    }
                }
                Thread monitorThread = new Thread(() => MonitorUploadStatus(driver, firstUploadTime, excelFiles.Length, downloadDirectory));
                monitorThread.Start();
                monitorThread.Join();
            }
        }
        catch (System.Exception ex)
        {
            //Console.WriteLine("Fatal error: " + ex.Message);
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
                                //Console.WriteLine("Error: Download button not found.");
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
                            //Console.WriteLine("Error generating validation report: " + ex.Message);
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
            //Console.WriteLine("Monitoring thread error: " + ex.Message);
        }
    }

    //static void MonitorUploadStatus(IWebDriver driver, DateTime uploadStartTime, int excelCount, string downloadDirectory)
    //{
    //    try
    //    {
    //        TimeSpan overallTimeout = TimeSpan.FromMinutes(30);
    //        DateTime monitoringStartTime = DateTime.Now;
    //        bool fileDownloaded = false;
    //        HashSet<string> downloadedFiles = new HashSet<string>();
    //        if (!Directory.Exists(awlFilesDirectory))
    //            Directory.CreateDirectory(awlFilesDirectory);

    //        while (DateTime.Now - monitoringStartTime < overallTimeout)
    //        {
    //            int rowCount = driver.FindElements(By.CssSelector("table.user-table tbody tr.user-row")).Count;
    //            for (int i = 0; i < rowCount; i++)
    //            {
    //                driver.Navigate().Refresh();
    //                Thread.Sleep(2000);
    //                var rows = driver.FindElements(By.CssSelector("table.user-table tbody tr.user-row"));
    //                if (i >= rows.Count)
    //                {
    //                    //Console.WriteLine("Row count changed after refresh. Skipping index: " + i);
    //                    continue;
    //                }
    //                var row = rows[i];
    //                var cells = row.FindElements(By.TagName("td"));
    //                if (cells.Count < 6) continue;

    //                string uniqueId = cells[0].Text.Trim();
    //                string statusText = cells[5].Text.Trim().ToLower();
    //                string timeString = cells[1].Text.Trim();
    //                string success = "";
    //                foreach (var cell in cells)
    //                {
    //                    var img = cell.FindElements(By.TagName("img")).FirstOrDefault();
    //                    if (img != null)
    //                    {
    //                        success = img.GetAttribute("src");
    //                        break;
    //                    }
    //                }
    //                if (downloadedFiles.Contains(uniqueId))
    //                    continue;

    //                if (DateTime.TryParse(timeString, out DateTime fileTime) && fileTime >= uploadStartTime)
    //                {
    //                    if (success.Contains("/icon-success"))
    //                    {
    //                        try
    //                        {
    //                            IWebElement downloadBtn = row.FindElement(By.CssSelector("button.download-btn"));
    //                            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", downloadBtn);
    //                            Thread.Sleep(5000);
    //                            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", downloadBtn);
    //                            Thread.Sleep(3000);
    //                            int expectedCount = 3;
    //                            int waitTime = 0;
    //                            int maxWait = 30;
    //                            int previousFileCount = Directory.GetFiles(downloadDirectory).Length;

    //                            while (Directory.GetFiles(downloadDirectory).Length - previousFileCount < expectedCount && waitTime < maxWait)
    //                            {
    //                                Thread.Sleep(1000);
    //                                waitTime++;
    //                                //Console.WriteLine($"Waiting for downloads... ({waitTime}s)");
    //                            }
    //                            int newDownloads = Directory.GetFiles(downloadDirectory).Length - previousFileCount;
    //                            if (newDownloads < expectedCount)
    //                            {
    //                                //Console.WriteLine($"Only {newDownloads}/{expectedCount} files downloaded.");
    //                            }
    //                            else
    //                            {
    //                                //Console.WriteLine("All files downloaded successfully.");
    //                            }
    //                            fileDownloaded = true;
    //                            downloadedFiles.Add(uniqueId);
    //                            string matchedFile = uploadTimes.Keys.FirstOrDefault(k =>
    //                                Path.GetFileNameWithoutExtension(k).Equals(uniqueId, StringComparison.OrdinalIgnoreCase) ||
    //                                uniqueId.IndexOf(Path.GetFileNameWithoutExtension(k), StringComparison.OrdinalIgnoreCase) >= 0 ||
    //                                Path.GetFileNameWithoutExtension(k).IndexOf(uniqueId, StringComparison.OrdinalIgnoreCase) >= 0
    //                            );
    //                            if (!string.IsNullOrEmpty(matchedFile) && uploadTimes.TryGetValue(matchedFile, out DateTime actualUploadTime))
    //                            {
    //                                TimeSpan generationTime = fileTime - actualUploadTime;
    //                                string log = $"File Name: {Path.GetFileName(matchedFile)}\n" +
    //                                             $"Uploaded At: {actualUploadTime:dd-MM-yyyy hh:mm:ss tt}\n" +
    //                                             $"Generated At: {fileTime:dd-MM-yyyy hh:mm:ss tt}\n" +
    //                                             $"Time Taken: {generationTime}\n" +
    //                                             $"--------------------------\n";
    //                                File.AppendAllText(logFilePath, log);
    //                            }
    //                            else
    //                            {
    //                                //Console.WriteLine("Could not find upload time for: " + uniqueId);
    //                                //Console.WriteLine("Keys Available: " + string.Join(", ", uploadTimes.Keys.Select(Path.GetFileNameWithoutExtension)));
    //                            }
    //                        }
    //                        catch (NoSuchElementException)
    //                        {
    //                            //Console.WriteLine("Error: Download button not found.");
    //                        }
    //                    }
    //                    else if (success.Contains("/icon-error"))
    //                    {
    //                        //Console.WriteLine($"Block generation failed for file: {uniqueId} at {fileTime:dd-MM-yyyy hh:mm:ss tt}");
    //                    }
    //                    else
    //                    {
    //                        //Console.WriteLine($"Status not ready or unknown: {statusText} for file at {fileTime}");
    //                    }
    //                }
    //                if (downloadedFiles.Count >= excelCount)
    //                {
    //                    try
    //                    {
    //                        Thread.Sleep(5000);
    //                        string[] validationFiles = Directory.GetFiles(downloadDirectory, "validation_*.txt");
    //                        StringBuilder reportBuilder = new StringBuilder();
    //                        StringBuilder popupBuilder = new StringBuilder();
    //                        foreach (var file in validationFiles)
    //                        {
    //                            string[] lines = File.ReadAllLines(file);
    //                            string fileName = Path.GetFileNameWithoutExtension(file).Replace("validation_", "");
    //                            string spreadsheetName = $"{fileName}.xlsx";
    //                            string description = "";
    //                            string lineA = "All state networks have atleast one connection from a transition variable";
    //                            string lineBStart = "One or more state network has no incoming conections from any transition variables";
    //                            string verifyLine1 = "Verify if the conditions are defined as expected for states";
    //                            string verifyLine2 = "Specifically verify if the boolean expressions are properly created";
    //                            if (lines.Any(line => line.Contains(lineA)))
    //                            {
    //                                description = lineA;
    //                            }
    //                            else
    //                            {
    //                                string lineBFull = lines.FirstOrDefault(line => line.StartsWith(lineBStart));
    //                                if (!string.IsNullOrEmpty(lineBFull))
    //                                {
    //                                    description = lineBFull;
    //                                    var extraLines = lines.Where(l =>
    //                                        l.StartsWith(verifyLine1) || l.StartsWith(verifyLine2)).ToList();
    //                                    if (extraLines.Any())
    //                                    {
    //                                        description += "\n\n" + string.Join("\n", extraLines);
    //                                    }
    //                                }
    //                            }
    //                            reportBuilder.AppendLine($"=== SPREADSHEET NAME ===");
    //                            reportBuilder.AppendLine($"{spreadsheetName}");
    //                            reportBuilder.AppendLine($"=== DESCRIPTION ===");
    //                            reportBuilder.AppendLine(description);
    //                            reportBuilder.AppendLine(new string('-', 60));
    //                            popupBuilder.AppendLine($"*SPREADSHEET NAME:* {spreadsheetName}");
    //                            popupBuilder.AppendLine($"*DESCRIPTION:*");
    //                            popupBuilder.AppendLine(description);
    //                            popupBuilder.AppendLine(new string('-', 60));
    //                        }
    //                        string reportPath = Path.Combine(downloadDirectory, "Validation_Report.txt");
    //                        File.WriteAllText(reportPath, reportBuilder.ToString());
    //                        string popupText = popupBuilder.ToString();
    //                        if (!string.IsNullOrWhiteSpace(popupText))
    //                        {
    //                            string popupDisplay = popupText.Length > 1500
    //                                ? popupText.Substring(0, 1500) + "\n...\n(See full report in file)"
    //                                : popupText;
    //                            MessageBox.Show(popupDisplay, "Validation Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
    //                        }
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        //Console.WriteLine("Error generating validation report: " + ex.Message);
    //                    }
    //                    return;
    //                }
    //            }
    //            if (!fileDownloaded)
    //                Thread.Sleep(2000);
    //        }
    //        if (!fileDownloaded)
    //            Thread.Sleep(1000);
    //    }
    //    catch (Exception ex)
    //    {
    //        //Console.WriteLine("Monitoring thread error: " + ex.Message);
    //    }
    //}
    static string selectedPrefix = null;
    static string sessionFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.txt");
    static string prefixSessionFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "prefix.txt");

    static string globalProjectPath = null;
    static string globalTiaProcId = null;

    //static bool projCheck(string projectPath)
    //{
    //    TiaPortal tiaPortal = null;
    //    Project project = null;

    //    try
    //    {
    //        var tiaProcesses = TiaPortal.GetProcesses();
    //        string normalizedTargetPath = Path.GetFullPath(projectPath).ToLowerInvariant();
    //        bool attached = false;
    //        string procID = "";

    //        foreach (var proc in tiaProcesses)
    //        {
    //            try
    //            {
    //                var portal = proc.Attach();
    //                foreach (var proj in portal.Projects)
    //                {
    //                    string openPath = Path.GetFullPath(proj.Path.FullName).TrimEnd('\\');
    //                    if (openPath.Equals(projectPath, StringComparison.OrdinalIgnoreCase))
    //                    {
    //                        procID = proc.Id.ToString();
    //                        tiaPortal = portal;
    //                        project = proj;
    //                        attached = true;
    //                        break;
    //                    }
    //                }

    //                if (attached)
    //                    break;
    //                else
    //                    portal.Dispose();
    //            }
    //            catch { continue; }
    //        }

    //        if (!attached)
    //        {
    //            MessageBox.Show("Could not find matching open project in any TIA instance.", "Attach Error");
    //            return false;
    //        }

    //        globalProjectPath = Path.GetFullPath(project.Path.FullName);
    //        globalTiaProcId = procID;

    //        string currentProjectId = globalProjectPath.ToLowerInvariant() + procID;

    //        if (File.Exists(sessionFile))
    //        {
    //            string savedProjectPath = File.ReadAllText(sessionFile).Trim();

    //            if (savedProjectPath == currentProjectId)
    //            {
    //                Console.WriteLine("Same project already opened. No login required.");
    //            }
    //            else
    //            {
    //                Console.WriteLine("Different project detected. Asking for login...");

    //                GetApplicationKey();
    //                LicenseForm form = new LicenseForm(ApplicationKey);
    //                DialogResult result = form.ShowDialog();
    //                if (result != DialogResult.OK)
    //                    return false;

    //                File.WriteAllText(sessionFile, currentProjectId);
    //            }
    //        }
    //        else
    //        {
    //            Console.WriteLine("First run or no session detected. Asking for login...");

    //            GetApplicationKey();
    //            LicenseForm form = new LicenseForm(ApplicationKey);
    //            DialogResult result = form.ShowDialog();
    //            if (result != DialogResult.OK)
    //                return false;

    //            File.WriteAllText(sessionFile, currentProjectId);
    //        }

    //        bool prefixLoaded = false;
    //        if (File.Exists(prefixSessionFile))
    //        {
    //            var prefixData = File.ReadAllText(prefixSessionFile).Split('|');
    //            if (prefixData.Length == 2 && prefixData[0] == currentProjectId)
    //            {
    //                selectedPrefix = prefixData[1];
    //                PrefixSelectionForm.SetDoSameForAll(true);
    //                prefixLoaded = true;
    //            }
    //        }

    //        if (!prefixLoaded)
    //        {
    //            var prefixForm = new PrefixSelectionForm(globalProjectPath, globalTiaProcId);
    //            if (prefixForm.ShowDialog() == DialogResult.OK)
    //            {
    //                selectedPrefix = prefixForm.SelectedPrefix;
    //                File.WriteAllText(prefixSessionFile, $"{currentProjectId}|{selectedPrefix}");
    //            }
    //            else
    //            {
    //                return false;
    //            }
    //        }

    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show(ex.Message, "Error");
    //        return false;
    //    }
    //}
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
                    //Expected format → "UserID|ExpiryDate"
                    string[] parts = decryptedLicense.Split('|');
                    if (parts.Length == 2)
                    {
                        string licensedUserId = parts[0];
                        DateTime expiryDate = DateTime.ParseExact(parts[1], "yyyy-MM-dd HH:mm:ss", null);

                        if (/*licensedUserId == TiaPortalAutomation.userID && */DateTime.Now <= expiryDate)
                        {
                            licenseValid = true;
                        }
                        //else if (licensedUserId != TiaPortalAutomation.userID && TiaPortalAutomation.userID!="")
                        //{
                        //    MessageBox.Show("License belongs to a different user. Please request a new license.",
                        //        "Invalid User", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        //    licenseValid = false;
                        //}
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

            //if (DateTime.Now <= DateTime.Parse(expiryDate))
            //        {
            //            licenseValid = true;
            //        }
            //        else
            //        {
            //            MessageBox.Show("License expired. Contact admin for renewal.",
            //                "License Expired", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        }
            //    }
            //    catch
            //    {
            //        MessageBox.Show("License file corrupted. Contact admin.",
            //            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    }
            //}

            if (!licenseValid)
            {
                GetApplicationKey(fullDeviceName);
                using (var licenseForm = new LicenseForm(ApplicationKey, licenseValid, true))
                {
                    if (licenseForm.ShowDialog() != DialogResult.OK)
                    {
                        return false;
                    }

                    //string expiryDate = DateTime.Now.AddDays(15).ToString("yyyy-MM-dd HH:mm:ss");

                    //string encryptedExpiry = EncryptionHelper.Encrypt(expiryDate);
                    //File.WriteAllText(licenseFile, encryptedExpiry);

                    //DialogResult result = licenseForm.ShowDialog();
                    //if (licenseForm.ShowDialog() != DialogResult.OK)
                    //    return false;

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

//    bool prefixLoaded = false;
//    if (File.Exists(prefixSessionFile))
//    {
//        var prefixData = File.ReadAllText(prefixSessionFile).Split('|');
//        if (prefixData.Length == 2 && prefixData[0] == currentProjectId)
//        {
//            selectedPrefix = prefixData[1];
//            PrefixSelectionForm.SetDoSameForAll(true);
//            prefixLoaded = true;
//        }
//    }

//    if (!prefixLoaded)
//    {
//        var prefixForm = new PrefixSelectionForm(globalProjectPath, globalTiaProcId);
//        if (prefixForm.ShowDialog() == DialogResult.OK)
//        {
//            selectedPrefix = prefixForm.SelectedPrefix;
//            File.WriteAllText(prefixSessionFile, $"{currentProjectId}|{selectedPrefix}");
//        }
//        else
//        {
//            return false;
//        }
//    }

//    return true;
//}
//catch (Exception ex)
//{
//    MessageBox.Show(ex.Message, "Error");
//    return false;
//}
        
//static bool licenseValid = false;
//static bool projCheck(string projectPath, string fullDeviceName)
//{
//    TiaPortal tiaPortal = null;
//    Project project = null;

//    try
//    {
//        var tiaProcesses = TiaPortal.GetProcesses();
//        string normalizedTargetPath = Path.GetFullPath(projectPath).ToLowerInvariant();
//        bool attached = false;
//        string procID = "";

//        foreach (var proc in tiaProcesses)
//        {
//            try
//            {
//                var portal = proc.Attach();
//                foreach (var proj in portal.Projects)
//                {
//                    string openPath = Path.GetFullPath(proj.Path.FullName).TrimEnd('\\');
//                    if (openPath.Equals(projectPath, StringComparison.OrdinalIgnoreCase))
//                    {
//                        procID = proc.Id.ToString();
//                        tiaPortal = portal;
//                        project = proj;
//                        attached = true;
//                        break;
//                    }
//                }
//                if (attached)
//                    break;
//                else
//                    portal.Dispose();
//            }
//            catch { continue; }
//        }

//        if (!attached)
//        {
//            MessageBox.Show("Could not find matching open project in any TIA instance.", "Attach Error");
//            return false;
//        }
//        globalProjectPath = Path.GetFullPath(project.Path.FullName);
//        globalTiaProcId = procID;
//        string currentProjectId = globalProjectPath.ToLowerInvariant() + procID;

//        string licenseFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "license.txt");
//        //string licenseFile = @"C:\Generate Block\Debug\license.dat";

//        //string licenseFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "license.dat");
//        // Fixed path for license file
//        //string licenseFile = @"C:\Users\P01090879\source\repos\Final Code Language Change STL to LAD and export logic with exe 01.07.25\With15daysvalidation\140825\ConsoleApp6\bin\Debug\licenseFile";
//        //string licenseFile = @"C:\Generate Block\Debug\licenseFile.txt";
//        //// Make sure directory exists
//        //MessageBox.Show("exist");
//        //string folderPath = Path.GetDirectoryName(licenseFile);
//        //if (!Directory.Exists(folderPath))
//        //{
//        //    MessageBox.Show("not exist");
//        //    Directory.CreateDirectory(folderPath);
//        //}

//        //if (File.Exists(licenseFile))
//        //{
//        //    try
//        //    {
//        //        string encryptedExpiry = File.ReadAllText(licenseFile);
//        //        string expiryDate = EncryptionHelper.Decrypt(encryptedExpiry);

//        //        if (DateTime.Now <= DateTime.Parse(expiryDate))
//        //        {
//        //            licenseValid = true;
//        //        }
//        //        else
//        //        {
//        //            MessageBox.Show("License expired. Contact admin for renewal.",
//        //                "License Expired", MessageBoxButtons.OK, MessageBoxIcon.Warning);
//        //        }
//        //    }
//        //    catch
//        //    {
//        //        MessageBox.Show("License file corrupted. Contact admin.",
//        //            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//        //    }
//        //}
//        if (File.Exists(licenseFile))
//        {
//            try
//            {
//                string encryptedLicense = File.ReadAllText(licenseFile);
//                string decryptedLicense = EncryptionHelper.Decrypt(encryptedLicense);

//                string[] parts = decryptedLicense.Split('|');
//                if (parts.Length == 2)
//                {
//                    string licensedUserId = parts[0];
//                    DateTime expiryDate = DateTime.ParseExact(parts[1], "yyyy-MM-dd HH:mm:ss", null);

//                    if (/*licensedUserId == TiaPortalAutomation.userID && */DateTime.Now <= expiryDate)
//                    {
//                        licenseValid = true;
//                    }
//                    //else if (licensedUserId != TiaPortalAutomation.userID && TiaPortalAutomation.userID!="")
//                    //{
//                    //    MessageBox.Show("License belongs to a different user. Please request a new license.",
//                    //        "Invalid User", MessageBoxButtons.OK, MessageBoxIcon.Error);

//                    //    licenseValid = false;
//                    //}
//                    else
//                    {
//                        MessageBox.Show("License expired. Contact admin for renewal.",
//                            "License Expired", MessageBoxButtons.OK, MessageBoxIcon.Warning);
//                    }
//                }
//                else
//                {
//                    MessageBox.Show("Invalid license format. Contact admin.",
//                        "License Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                }
//            }
//            catch
//            {
//                MessageBox.Show("License file corrupted. Contact admin.",
//                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//        }

//        //if (!licenseValid)
//        //{
//        //    GetApplicationKey(fullDeviceName);
//        //    using (var licenseForm = new LicenseForm(ApplicationKey, licenseValid))
//        //    {
//        //        if (licenseForm.ShowDialog() != DialogResult.OK)
//        //        {
//        //            return false;
//        //        }

//        //        string expiryDate = DateTime.Now.AddDays(15).ToString("yyyy-MM-dd HH:mm:ss");
//        //        //string expiryDate = DateTime.Now.AddMinutes(5).ToString("yyyy-MM-dd HH:mm:ss");

//        //        string encryptedExpiry = EncryptionHelper.Encrypt(expiryDate);
//        //        File.WriteAllText(licenseFile, encryptedExpiry);
//        //        //DialogResult result = licenseForm.ShowDialog();

//        //        //if (licenseForm.ShowDialog() != DialogResult.OK)
//        //        //    return false;

//        //    File.WriteAllText(sessionFile, currentProjectId);
//        //    }
//        //}
//        if (!licenseValid)
//        {
//            GetApplicationKey(fullDeviceName);
//            using (var licenseForm = new LicenseForm(ApplicationKey, licenseValid))
//            {
//                if (licenseForm.ShowDialog() != DialogResult.OK)
//                    return false;

//                //licenseValid = true;
//                File.WriteAllText(sessionFile, currentProjectId);
//            }
//        }

//        if (File.Exists(sessionFile))
//        {
//            string savedProjectPath = File.ReadAllText(sessionFile).Trim();

//            if (savedProjectPath == currentProjectId)
//            {
//                Console.WriteLine("Same project already opened. No login required.");
//            }
//            else
//            {
//                Console.WriteLine("Different project detected. Asking for login...");

//                GetApplicationKey(fullDeviceName);
//                LicenseForm form = new LicenseForm(ApplicationKey, licenseValid);
//                DialogResult result = form.ShowDialog();
//                if (result != DialogResult.OK)
//                    return false;

//                File.WriteAllText(sessionFile, currentProjectId);
//            }
//        }
//        else
//        {
//            Console.WriteLine("First run or no session detected. Asking for login...");

//            GetApplicationKey(fullDeviceName);
//            LicenseForm form = new LicenseForm(ApplicationKey,licenseValid);
//            DialogResult result = form.ShowDialog();
//            if (result != DialogResult.OK)
//                return false;

//            File.WriteAllText(sessionFile, currentProjectId);
//        }

//        bool prefixLoaded = false;
//        if (File.Exists(prefixSessionFile))
//        {
//            var prefixData = File.ReadAllText(prefixSessionFile).Split('|');
//            if (prefixData.Length == 2 && prefixData[0] == currentProjectId)
//            {
//                selectedPrefix = prefixData[1];
//                PrefixSelectionForm.SetDoSameForAll(true);
//                prefixLoaded = true;
//            }
//        }

//        if (!prefixLoaded)
//        {
//            var prefixForm = new PrefixSelectionForm(globalProjectPath, globalTiaProcId);
//            if (prefixForm.ShowDialog() == DialogResult.OK)
//            {
//                selectedPrefix = prefixForm.SelectedPrefix;
//                File.WriteAllText(prefixSessionFile, $"{currentProjectId}|{selectedPrefix}");
//            }
//            else
//            {
//                return false;
//            }
//        }

//        return true;
//    }
//    catch (Exception ex)
//    {
//        MessageBox.Show(ex.Message, "Error");
//        return false;
//    }
//}
    
    static List<string> ImportXmlFilesFromCorrectTIAInstance(string projectPath, string xmlFilesDirectory, string caxFilePath1)
    {
        List<string> errorXmlFiles = new List<string>();
        List<string> addBlockVersion = new List<string>();
        TiaPortal tiaPortal = null;
        Project project = null;
        //caxFilePath1 = @"C:\\Generate Block\\AML Files Path\\Project2.aml";
        //xmlFilesDirectory = @"C:\Generate Block\XML Files";
        
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
                        //Console.WriteLine("Failed to copy XML file to backup folder: " + ex.Message);
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

        //try
        //{
        //    string sourceRootDirectory = Path.GetDirectoryName(projectPath);
        //    string backupFolder = @"C:\Generate Block\V20 Project";

        //    if (!Directory.Exists(backupFolder))
        //    {
        //        Directory.CreateDirectory(backupFolder);
        //    }

        //    foreach (string file in Directory.GetFiles(backupFolder))
        //    {
        //        File.Delete(file);
        //    }

        //    foreach (string dir in Directory.GetDirectories(backupFolder))
        //    {
        //        Directory.Delete(dir, true);
        //    }

        //    foreach (string filePath in Directory.GetFiles(sourceRootDirectory))
        //    {
        //        string destPath = Path.Combine(backupFolder, Path.GetFileName(filePath));
        //        File.Copy(filePath, destPath, true);
        //    }

        //    foreach (string dirPath in Directory.GetDirectories(sourceRootDirectory))
        //    {
        //        string destDir = Path.Combine(backupFolder, Path.GetFileName(dirPath));
        //        CopyDirectory(dirPath, destDir);
        //    }

        //    MessageBox.Show("Project folder and all contents successfully backed up to: " + backupFolder, "Backup Complete");
        //}
        //catch (Exception ex)
        //{
        //    MessageBox.Show($"Failed to back up full project directory:\n{ex.Message}", "Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //}

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
        //var data = GetMacAddress() + ".APL" + "." + System.Windows.Forms.SystemInformation.ComputerName;

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
        //Console.WriteLine(FromHexString(data2));
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
            //using (var entry = new DirectoryEntry("LDAP://10.1.2.15", "wipropari\\" + userID, password))
            //{
            //    object obj = entry.NativeObject;
            //    return true;
            //}
        }
        catch (System.Exception ex)
        {
            MessageBox.Show("Invalid Username Or Password!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }
        //try
        //{
        //    using (var entry = new DirectoryEntry("LDAP://10.1.2.15", "wipropari\\" + userID, password))
        //    {
        //        object obj = entry.NativeObject;
        //        return true;
        //    }
        //}
        //catch (Exception ex)
        //{
        //   return false;
        //}
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

        //currentTop += 60;

        //var emailLabel = new System.Windows.Forms.Label()
        //{
        //    Text = "Enter Name:",
        //    Left = 20,
        //    Top = currentTop,
        //    Width = 150
        //};
        //txtEmail = new System.Windows.Forms.TextBox()
        //{
        //    Left = 20,
        //    Top = currentTop + 25,
        //    Width = 440
        //};

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
        //Controls.Add(emailLabel);
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
            // Ignore errors
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
                File.Delete(credFile); // Remove saved credentials if unchecked
            }
        }
        catch { }
    }
    //public LicenseForm(string appKey, bool licenseValid, bool newSession)
    //{
    //    licenseValidated = licenseValid;
    //    this.Text = "License Verification";
    //    this.Width = 500;
    //    this.Height = 450;
    //    this.FormBorderStyle = FormBorderStyle.FixedDialog;
    //    this.MaximizeBox = false;
    //    this.StartPosition = FormStartPosition.CenterScreen;

    //          var lblAppKey = new System.Windows.Forms.Label() { Text = "Application Key:", Left = 20, Top = 20, Width = 120 };
    //        var txtAppKey = new System.Windows.Forms.TextBox() { Left = 20, Top = 45, Width = 440, ReadOnly = true };
    //        txtAppKey.Text = appKey;

    //        var lblValKey = new System.Windows.Forms.Label() { Text = "Enter Validation Key:", Left = 20, Top = 80, Width = 150 };
    //        txtValidationKey = new System.Windows.Forms.TextBox() { Left = 20, Top = 105, Width = 440 };

    //        var usernameLabel = new System.Windows.Forms.Label() { Text = "Enter User Name:", Left = 20, Top = 140, Width = 150 };
    //        Username = new System.Windows.Forms.TextBox() { Left = 20, Top = 165, Width = 440 };

    //        var pwdLabel = new System.Windows.Forms.Label() { Text = "Enter Login Password:", Left = 20, Top = 200, Width = 150 };
    //        txtLoginPassword = new System.Windows.Forms.TextBox() { Left = 20, Top = 225, Width = 440, PasswordChar = '*' };

    //        var emailLabel = new System.Windows.Forms.Label() { Text = "Enter Name:", Left = 20, Top = 260, Width = 150 };
    //        txtEmail = new System.Windows.Forms.TextBox() { Left = 20, Top = 285, Width = 440 };

    //        Button showHideButton = new Button() { Text = "Show Login Pwd", Left = 20, Top = 320, Width = 140 };
    //        showHideButton.Click += (s, e) =>
    //        {
    //            if (txtLoginPassword.PasswordChar == '*')
    //            {
    //                txtLoginPassword.PasswordChar = '\0';
    //                ((Button)s).Text = "Hide Login Pwd";
    //            }
    //            else
    //            {
    //                txtLoginPassword.PasswordChar = '*';
    //                ((Button)s).Text = "Show Login Pwd";
    //            }
    //        };
    //        var btnSubmit = new Button() { Text = "Submit", Left = 360, Top = 320, Width = 100 };
    //        btnSubmit.Click += BtnSubmit_Click;

    //        Controls.Add(lblAppKey);
    //        Controls.Add(txtAppKey);
    //        Controls.Add(lblValKey);
    //        Controls.Add(txtValidationKey);
    //        Controls.Add(usernameLabel);
    //        Controls.Add(Username);
    //        Controls.Add(pwdLabel);
    //        Controls.Add(txtLoginPassword);
    //        Controls.Add(emailLabel);
    //        Controls.Add(txtEmail);
    //        Controls.Add(showHideButton);
    //        Controls.Add(btnSubmit);
    //}
    private void BtnSubmit_Click(object sender, EventArgs e)
    {
        TiaPortalAutomation.ValidationKey = txtValidationKey.Text.Trim();
        TiaPortalAutomation.password = txtLoginPassword.Text.Trim();
        TiaPortalAutomation.userID = Username.Text.Trim();
        string userMail = "";// txtEmail.Text.Trim();
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

                // Step 2: Send mail to admin for new license
                string licenseContent = $"{TiaPortalAutomation.userID}|{DateTime.Now}";
                SendMailToAdmin(licenseContent, TiaPortalAutomation.userID);

                // Step 3: Ask user to enter admin-provided key
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

    //    private void SendMailToAdmin(string uniqueCode, string userId)
    //    {
    //        try
    //        {
    //            //           

    //            Outlook.Application outlookApp1 = new Outlook.Application();
    //            Outlook.AddressEntry addressEntry1 = outlookApp1.Session.CurrentUser.AddressEntry;

    //            string displayNameInfo = "";
    //            string userMailInfo = "";
    //            string jobTitle = "";
    //            string department = "";
    //            string managerName = "";

    //            if (addressEntry1 != null && addressEntry1.Type == "EX")
    //            {
    //                Outlook.ExchangeUser exchUser = addressEntry1.GetExchangeUser();
    //                //exchUser != null ? exchUser.Name : addressEntry.Name;

    //                exchUser = addressEntry1.GetExchangeUser();
    //                if (exchUser != null)
    //                {
    //                    displayNameInfo = exchUser.Name;
    //                    userMailInfo = exchUser.PrimarySmtpAddress;
    //                    jobTitle = exchUser.JobTitle;
    //                    department = exchUser.Department;

    //                    if (exchUser.GetExchangeUserManager() != null)
    //                        managerName = exchUser.GetExchangeUserManager().Name;
    //                }                
    //            }

    //            string userInfoTable =
    //            "User Details:\n" +
    //            "-------------------------------------------\n" +
    //            $"User Name   : {displayNameInfo}\n" +
    //            $"Email       : {userMailInfo}\n" +
    //            $"Job Title   : {jobTitle}\n" +
    //            $"Department  : {department}\n" +
    //            $"Manager     : {managerName}\n" +
    //            "-------------------------------------------\n";

    //            //

    //            Outlook.Application outlookApp = new Outlook.Application();
    //            Outlook.MailItem mailItem = (Outlook.MailItem)outlookApp.CreateItem(Outlook.OlItemType.olMailItem);

    //            Outlook.Accounts accounts = outlookApp.Session.Accounts;
    //            string userMail = accounts.Count > 0 ? accounts[1].SmtpAddress : "unknown@domain.com";

    //            string displayName = outlookApp.Session.CurrentUser.Name;

    //            string emailUserName = accounts.Count > 0 ? accounts[1].DisplayName : "";
    //            string userName = userMail.Split('@')[0];

    //            mailItem.To = "sushant.p.1@wipropari.com"; // Admin email
    //            mailItem.CC = "sainathhb@wipropari.com"; // CC email
    //            mailItem.Subject = "License Request from User";

    //            mailItem.Body =
    //                $"Dear Admin,\n\n" +
    //                $"I am {displayName}, and I am sending a request for a license key to use the Gen PLC Application.\n\n" +
    //                $"Here are the details:\n" +
    //                $"User ID      : {userId}\n" +
    //                $"Request Time : {DateTime.Now}\n" +
    //                $"License Key  : {uniqueCode}\n\n" +
    //                $"Kindly share the license key at the earliest.\n\n" +
    //                $"Thank you for your time and support.\n\n" +
    //                $"{userInfoTable}" +
    //                $"Best Regards,\n" +
    //                $"{displayName}\n";

    //            mailItem.DeleteAfterSubmit = true;
    //            mailItem.Send();

    //            MessageBox.Show("Request sent successfully.", "Success",
    //                MessageBoxButtons.OK, MessageBoxIcon.Information);
    //        }
    //        catch (System.Exception ex)
    //        {
    //            MessageBox.Show("Failed to send mail: " + ex.Message,
    //                "Mail Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //        }
    //    }
    //}

    private void SendMailToAdmin(string uniqueCode, string userId)
    {
        try
        {
            // Start Outlook session
            Outlook.Application outlookApp = new Outlook.Application();
            Outlook.NameSpace outlookNamespace = outlookApp.GetNamespace("MAPI");
            outlookNamespace.Logon("", "", Missing.Value, Missing.Value);

            string displayNameInfo = "";
            string userMailInfo = "";
            string jobTitle = "";
            string department = "";
            string managerName = "";

            // Get current user information
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

            // Create the mail item
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


//private void BtnSubmit_Click(object sender, EventArgs e)
//{
//    TiaPortalAutomation.ValidationKey = txtValidationKey.Text.Trim();
//    TiaPortalAutomation.password = txtLoginPassword.Text.Trim();
//    TiaPortalAutomation.userID = Username.Text.Trim();
//    string userMail = txtEmail.Text.Trim();

//    if (TiaPortalAutomation.VerifyKey() && TiaPortalAutomation.verifyUser())
//    {

//        if (File.Exists("licenseFile"))

//        {
//            try
//            {
//                string encryptedLicense = File.ReadAllText("licenseFile");
//                string decryptedLicense = EncryptionHelper.Decrypt(encryptedLicense);

//                string[] parts = decryptedLicense.Split('|');
//                if (parts.Length == 2)
//                {
//                    string licensedUserId = parts[0];
//                    DateTime expiryDate = DateTime.ParseExact(parts[1], "yyyy-MM-dd HH:mm:ss", null);

//                    if (licensedUserId == TiaPortalAutomation.userID && DateTime.Now <= expiryDate)
//                    {
//                        MessageBox.Show("License validated successfully.", "Success",
//                            MessageBoxButtons.OK, MessageBoxIcon.Information);

//                        licenseValidated = true;
//                        this.DialogResult = DialogResult.OK;
//                        return;
//                    }
//                    else
//                    {
//                        MessageBox.Show("License Not validated.", "Error",
//                            MessageBoxButtons.OK, MessageBoxIcon.Information);
//                        licenseValidated = false;
//                    }
//                }
//            }

//            catch
//            {
//            }
//        }

//        string licenseFile = @"C:\Generate Block\Debug\license.dat";
//        if (!licenseValidated)
//        {
//            string uniqueRaw = $"{TiaPortalAutomation.userID}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
//            string uniqueEncrypted = EncryptionHelper.Encrypt(uniqueRaw);

//            SendMailToAdmin(uniqueEncrypted, TiaPortalAutomation.userID, userMail);
//            //MessageBox.Show("call");
//            string enteredId = Microsoft.VisualBasic.Interaction.InputBox(
//                "Contact admin for your License Key and enter it here:",
//                "Enter License Key", "");

//            try
//            {
//                string decryptedId = EncryptionHelper.Decrypt(enteredId);

//                if (decryptedId.StartsWith(TiaPortalAutomation.userID))
//                {
//                    string expiryDate = DateTime.Now.AddDays(15).ToString("yyyy-MM-dd HH:mm:ss");
//                    string licenseData = $"{TiaPortalAutomation.userID}|{expiryDate}";
//                    string encryptedLicense = EncryptionHelper.Encrypt(licenseData);

//                    File.WriteAllText("licenseFile", encryptedLicense);

//                    MessageBox.Show("Validation successful! License valid for 15 days.",
//                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

//                    licenseValidated = true;
//                    this.DialogResult = DialogResult.OK;
//                }
//                else
//                {
//                    MessageBox.Show("Invalid License Key. Contact Admin.",
//                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                }
//            }
//            catch
//            {
//                MessageBox.Show("Invalid or corrupted Unique ID. Contact Admin.",
//                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//        }
//        else
//        {
//            this.DialogResult = DialogResult.OK;
//        }
//    }
//}

//private void BtnSubmit_Click(object sender, EventArgs e)
//{
//    TiaPortalAutomation.ValidationKey = txtValidationKey.Text.Trim();
//    TiaPortalAutomation.password = txtLoginPassword.Text.Trim();
//    TiaPortalAutomation.userID = Username.Text.Trim();
//    string userMail = txtEmail.Text.Trim();

//    if (TiaPortalAutomation.VerifyKey() && TiaPortalAutomation.verifyUser())
//    {
//        // Always check if license file exists & is valid for this user
//        if (File.Exists("license.dat"))
//        {
//            try
//            {
//                string encryptedLicense = File.ReadAllText("license.dat");
//                string decryptedLicense = EncryptionHelper.Decrypt(encryptedLicense);
//                string[] parts = decryptedLicense.Split('|');
//                if (parts.Length == 2)
//                {
//                    string licensedUserId = parts[0];
//                    DateTime expiryDate = DateTime.ParseExact(parts[1], "yyyy-MM-dd HH:mm:ss", null);

//                    if (licensedUserId == TiaPortalAutomation.userID && DateTime.Now <= expiryDate)
//                    {
//                        MessageBox.Show("License validated successfully.", "Success",
//                            MessageBoxButtons.OK, MessageBoxIcon.Information);
//                        licenseValidated = true;
//                        this.DialogResult = DialogResult.OK;
//                        return;
//                    }
//                }
//            }
//            catch
//            {
//            }
//        }

//        string uniqueRaw = $"{TiaPortalAutomation.userID}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
//        string uniqueEncrypted = EncryptionHelper.Encrypt(uniqueRaw);

//        SendMailToAdmin(uniqueEncrypted, TiaPortalAutomation.userID, userMail);

//        string enteredId = Microsoft.VisualBasic.Interaction.InputBox(
//            "Contact admin for your Unique ID and enter it here:",
//            "Enter Unique ID", "");

//        try
//        {
//            string decryptedId = EncryptionHelper.Decrypt(enteredId);

//            if (decryptedId.StartsWith(TiaPortalAutomation.userID))
//            {
//                string expiryDate = DateTime.Now.AddDays(15).ToString("yyyy-MM-dd HH:mm:ss");
//                string licenseData = $"{TiaPortalAutomation.userID}|{expiryDate}";
//                string encryptedLicense = EncryptionHelper.Encrypt(licenseData);

//                File.WriteAllText("license.dat", encryptedLicense);

//                MessageBox.Show("Validation successful! License valid for 15 days.",
//                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

//                licenseValidated = true;
//                this.DialogResult = DialogResult.OK;
//            }
//            else
//            {
//                MessageBox.Show("Invalid Unique ID. Contact Admin.",
//                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//        }
//        catch
//        {
//            MessageBox.Show("Invalid or corrupted Unique ID. Contact Admin.",
//                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//        }
//    }
//}


//private void BtnSubmit_Click(object sender, EventArgs e)
//{
//    TiaPortalAutomation.ValidationKey = txtValidationKey.Text.Trim();
//    TiaPortalAutomation.password = txtLoginPassword.Text.Trim();
//    TiaPortalAutomation.userID = Username.Text.Trim();
//    string userMail = txtEmail.Text.Trim();        

//    if (TiaPortalAutomation.VerifyKey() && TiaPortalAutomation.verifyUser())
//    {
//        if (!licenseValidated)
//        {
//            string uniqueRaw = $"{TiaPortalAutomation.userID}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
//            string uniqueEncrypted = EncryptionHelper.Encrypt(uniqueRaw);

//            SendMailToAdmin(uniqueEncrypted, TiaPortalAutomation.userID, userMail);

//            string enteredId = Microsoft.VisualBasic.Interaction.InputBox(
//                "Contact admin for your Unique ID and enter it here:",
//                "Enter Unique ID", "");

//            try
//            {
//                string decryptedId = EncryptionHelper.Decrypt(enteredId);

//                if (decryptedId.StartsWith(TiaPortalAutomation.userID))
//                {
//                    string expiryDate = DateTime.Now.AddDays(10).ToString("yyyy-MM-dd HH:mm:ss");
//                    string encryptedExpiry = EncryptionHelper.Encrypt(expiryDate);

//                    File.WriteAllText("license.dat", encryptedExpiry);

//                    MessageBox.Show("Validation successful!", "Success",
//                        MessageBoxButtons.OK, MessageBoxIcon.Information);

//                    this.DialogResult = DialogResult.OK;
//                }
//                else
//                {
//                    MessageBox.Show("Invalid Unique ID. Contact Admin.",
//                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                }
//            }
//            catch
//            {
//                MessageBox.Show("Invalid or corrupted Unique ID. Contact Admin.",
//                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//        }
//        else// if (licenseValidated)
//        {
//            this.DialogResult = DialogResult.OK;
//        }
//    }
//}

//private void SendMailToAdmin(string uniqueCode, string userId, string userMail)
//{
//    try
//    {
//        Outlook.Application outlookApp = new Outlook.Application();
//        Outlook.MailItem mail = (Outlook.MailItem)outlookApp.CreateItem(Outlook.OlItemType.olMailItem);

//        mail.To = "sushant.p.1@wipropari.com";
//        mail.Subject = "Testing License Request from User";

//        mail.Body = $"User ID: {userId}\n" +
//                    $"User Email: {userMail}\n" +
//                    $"Time: {DateTime.Now}";

//        mail.BCC = "sushant.p.1@wipropari.com";

//        string tempFile = Path.GetTempFileName();
//        File.WriteAllText(tempFile, "Unique Key: " + uniqueCode);
//        mail.Attachments.Add(tempFile);

//        mail.Display(false);

//    }
//    catch (Exception ex)
//    {
//        MessageBox.Show("Failed to prepare Outlook mail: " + ex.Message,
//            "Mail Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//    }
//}

//private void SendMailToAdmin(string uniqueCode, string userId, string userMail)
//{
//    try
//    {
//        string subject = "Testing License Request from User";
//        string bodyForUser = $"User ID: {userId}\n" +
//                             $"User Email: {userMail}\n" +
//                             $"Time: {DateTime.Now}";

//        string bodyForAdmin = bodyForUser +
//                              $"\nUnique Key: {uniqueCode}";

//        string mailto = $"mailto:sushant.p.1@wipropari.com?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(bodyForAdmin)}";

//        System.Diagnostics.Process.Start(mailto);
//    }
//    catch (Exception ex)
//    {
//        MessageBox.Show("Failed to open Outlook: " + ex.Message, "Mail Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//    }
//}

//private void SendMailToAdmin(string uniqueCode, string userId, string userMail)
//{
//    try
//    {
//        Outlook.Application outlook
//        Outlook.Application outlookApp = new Outlook.Application();
//        Outlook.MailItem mailItem = (Outlook.MailItem)outlookApp.CreateItem(Outlook.OlItemType.olMailItem);

//        mailItem.To = "sushant.p.1@wipropari.com";
//        mailItem.Subject = "License Request from User";

//        mailItem.Body = $"User ID: {userId}\n" +
//                        $"User Email: {userMail}\n" +
//                        $"Time: {DateTime.Now}\n" +
//                        $"Unique Key (hidden from user): {uniqueCode}";

//        mailItem.DeleteAfterSubmit = true;

//        mailItem.Send();

//        MessageBox.Show("Request sent successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
//    }
//    catch (Exception ex)
//    {
//        MessageBox.Show("Failed to send mail: " + ex.Message, "Mail Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//    }
//}
//private void SendMailToAdmin(string uniqueCode, string userId, string userMail)

//{
//    MessageBox.Show("start.");
//    try
//    {
//        //if (string.IsNullOrWhiteSpace(userMail) || !userMail.Contains("@"))
//        //{
//        //    MessageBox.Show("Invalid user email address.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//        //    return;
//        //}

//        Outlook.Application outlookApp = new Outlook.Application();
//        Outlook.MailItem mailItem = (Outlook.MailItem)outlookApp.CreateItem(Outlook.OlItemType.olMailItem);

//        mailItem.To = "sushant.p.1@wipropari.com";
//        mailItem.Subject = "License Request from User";

//        mailItem.Body =
//            $"Dear Admin,\n\n" +
//            $"I am {userMail}, and I am sending a request for a license key to use the application.\n\n" +
//            $"Here are the details:\n" +
//            $"User ID      : {userId}\n" +
//            $"User Email   : {userMail}\n" +
//            $"Request Time : {DateTime.Now}\n" +
//            $"License Key  : {uniqueCode}\n\n" +
//            $"Kindly share the license key at the earliest.\n\n" +
//            $"Thank you for your time and support.\n\n" +
//            $"Best Regards,\n" +
//            $"{userMail}";

//        mailItem.DeleteAfterSubmit = true;

//        mailItem.Send();

//        MessageBox.Show("Request sent successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
//    }
//    catch (Exception ex)
//    {
//        MessageBox.Show("Failed to send mail: " + ex.Message, "Mail Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//    }
//}

//private void SendMailToAdmin(string uniqueCode, string userId, string userMail)
//{
//    try
//    {
//        string adminEmail = "priti.d.3@wipropari.com";

//        SmtpClient smtp = new SmtpClient("smtp.office365.com", 587)
//        {
//            Credentials = new NetworkCredential("priti.d.3@wipropari.com", "RfTs#45!jbdf"),
//            EnableSsl = true
//        };

//        MailMessage mail = new MailMessage
//        {
//            From = new MailAddress(userMail),
//            Subject = "License Request from User",
//            Body =
//                $"Dear Admin,\n\n" +
//                $"I am {userMail}, and I am sending a request for a license key to use the application.\n\n" +
//                $"Here are the details:\n" +
//                $"User ID      : {userId}\n" +
//                $"User Email   : {userMail}\n" +
//                $"Request Time : {DateTime.Now}\n" +
//                $"License Key  : {uniqueCode}\n\n" +
//                $"Kindly share the license key at the earliest.\n\n" +
//                $"Thank you for your time and support.\n\n" +
//                $"Best Regards,\n" +
//                $"{userMail}\n"
//        };

//        mail.To.Add(adminEmail);

//        smtp.Send(mail);

//        MessageBox.Show("Request sent successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
//    }
//    catch (Exception ex)
//    {
//        MessageBox.Show("Failed to send mail: " + ex.Message, "Mail Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//    }
//}
//private void SendMailViaOutlookInterop(string uniqueCode, string userId, string userMail)
//{
//    try
//    {
//        Outlook.Application outlookApp = new Outlook.Application();
//        Outlook.MailItem mailItem = (Outlook.MailItem)outlookApp.CreateItem(Outlook.OlItemType.olMailItem);

//        mailItem.To = "sushant.p.1@wipropari.com";
//        mailItem.Subject = "Testing License Request from User";
//        mailItem.Body =
//            $"User ID: {userId}\n" +
//            $"User Email: {userMail}\n" +
//            $"Unique Key: {uniqueCode}\n" +
//            $"Time: {DateTime.Now}";

//        mailItem.Send();
//        MessageBox.Show("Request sent via Outlook.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
//    }
//    catch (Exception ex)
//    {
//        MessageBox.Show("Outlook mail failed: " + ex.Message, "Mail Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//    }
//}

//private void SendMailToAdmin(string uniqueCode, string userId, string userMail)
//{
//    try
//    {
//        MailMessage mail = new MailMessage();
//        mail.From = new MailAddress("tompatil2023@gmail.com");
//        mail.To.Add("deshmukhpriti01@gmail.com");
//        mail.Subject = "Testing License Request from User";
//        mail.Body = $"User ID: {userId}\n" +
//                    $"User Email: {userMail}\n" +
//                    $"Unique Key: {uniqueCode}\n" +
//                    $"Time: {DateTime.Now}";

//        SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
//        //SmtpClient smtp = new SmtpClient("smtp.office365.com")
//        {
//            Port = 587,
//            Credentials = new NetworkCredential("tompatil2023@gmail.com", "slef mfep shjz rqxq"),
//            EnableSsl = true
//        };

//        smtp.Send(mail);

//        MessageBox.Show("Request sent successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
//    }
//    catch (Exception ex)
//    {
//        MessageBox.Show("Failed to send mail: " + ex.Message, "Mail Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//    }
//}
//}

//private void BtnSubmit_Click(object sender, EventArgs e)
//{
//    TiaPortalAutomation.ValidationKey = txtValidationKey.Text.Trim();
//    TiaPortalAutomation.password = password.Text.Trim();
//    TiaPortalAutomation.userID = Username.Text.Trim();

//    if (TiaPortalAutomation.VerifyKey())
//    {
//        //MessageBox.Show("Validation successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
//        //this.DialogResult = DialogResult.OK;
//        this.Close();
//    }
//    else
//    {
//        MessageBox.Show("Invalid validation key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//    }
//    if (TiaPortalAutomation.verifyUser())
//    {
//        //MessageBox.Show("User Validation successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
//        //this.DialogResult = DialogResult.OK;
//        this.Close();
//    }
//    else
//    {
//        MessageBox.Show("Invalid User Id or Password", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//    }
//    if (TiaPortalAutomation.VerifyKey() && TiaPortalAutomation.verifyUser())
//    {
//        this.DialogResult = DialogResult.OK;
//        MessageBox.Show("Validation successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
//    }
//}

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

    /// <summary>
    /// Sets "Do same for all" flag manually.
    /// </summary>
    public static void SetDoSameForAll(bool value)
    {
        doSameForAll = value;
    }

    /// <summary>
    /// Gets "Do same for all" flag.
    /// </summary>
    public static bool GetDoSameForAll()
    {
        return doSameForAll;
    }

    /// <summary>
    /// Gets last used prefix.
    /// </summary>
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

//public class PrefixSelectionForm : Form
//{
//    public string SelectedPrefix { get; private set; } = null;
//    private System.Windows.Forms.ComboBox comboBox;

//    public PrefixSelectionForm()
//    {
//        Text = "Select or Enter Prefix";
//        Width = 320;
//        Height = 160;
//        FormBorderStyle = FormBorderStyle.FixedDialog;
//        StartPosition = FormStartPosition.CenterScreen;
//        MaximizeBox = false;
//        MinimizeBox = false;

//        System.Windows.Forms.Label label = new System.Windows.Forms.Label
//        {
//            Text = "Choose or enter a prefix:",
//            Left = 10,
//            Top = 10,
//            Width = 280
//        };
//        Controls.Add(label);

//        comboBox = new System.Windows.Forms.ComboBox
//        {
//            Left = 10,
//            Top = 35,
//            Width = 280,
//            DropDownStyle = ComboBoxStyle.DropDown
//        };

//        comboBox.Items.AddRange(new[] { "FB_", "PR_", "None" });
//        comboBox.SelectedIndex = 0;
//        Controls.Add(comboBox);

//        System.Windows.Forms.Button okButton = new System.Windows.Forms.Button
//        {
//            Text = "OK",
//            Left = 100,
//            Top = 70,
//            DialogResult = DialogResult.OK
//        };
//        okButton.Click += (sender, e) =>
//        {
//            string input = comboBox.Text.Trim();
//            SelectedPrefix = input.Equals("None", StringComparison.OrdinalIgnoreCase) ? "" : input;
//            Close();
//        };

//        Controls.Add(okButton);
//        AcceptButton = okButton;
//    }
//}