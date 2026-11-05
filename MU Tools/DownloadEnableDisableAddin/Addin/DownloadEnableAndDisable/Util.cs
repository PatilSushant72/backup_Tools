using Siemens.Engineering;
using Siemens.Engineering.AddIn.Menu;
using Siemens.Engineering.AddIn.Utilities;
using Siemens.Engineering.AddIn.VersionControl;
using Siemens.Engineering.Connection;
using Siemens.Engineering.Download;
using Siemens.Engineering.Download.Configurations;
using Siemens.Engineering.HW;
using Siemens.Engineering.Multiuser;
using Siemens.Engineering.Online;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.Umac;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
//using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DownloadEnableAndDisable
{
    /// <summary>
    /// This class represents the actual logic to be executed by the AddIn.
    /// </summary>
    internal static class Util
    {
        /// <summary>
        /// Starts an external process and hands over data as arguments
        /// </summary>
        /// <param name="handoverData">data you want to use in an external application</param>
        //internal static void RunExternalProcess(string handoverData)
        //{
        //    string exePath = "";
        //    //MessageBox.Show("P1");
        //    ProcessStartInfo startInfo = new ProcessStartInfo()
        //    {
        //        FileName = exePath,
        //        UseShellExecute = false,
        //        CreateNoWindow = false // set true if you want to hide window
        //    };

        //    Process cliProcess = new Process
        //    {
        //        EnableRaisingEvents = true,
        //        StartInfo = startInfo
        //    };

        //    cliProcess.Start();
        //    //RunAutomationCycle();
        //}
        //internal static string RunExternalProcess1(string path)
        //{
        //    MessageBox.Show("1");
        //    string exePath = @"C:\DownloadAddin\New folder\Debug\ConsoleApp6.exe";//@"C:\Generate Block\Debug\ConsoleApp6.exe";
        //    try
        //    {

        //        ProcessStartInfo startInfo = new ProcessStartInfo()
        //        {
        //            FileName = exePath, // ✅ Correct arguments format
        //            UseShellExecute = false,
        //            CreateNoWindow = true  // ✅ Hides console window
        //        };

        //        Process cliProcess = new Process
        //        {
        //            EnableRaisingEvents = true,
        //            StartInfo = startInfo
        //        };

        //        cliProcess.Start();
        //        return "";
        //    }
        //    catch(Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //    return "";
        //}
        internal static string RunExternalProcess(string batFilePath, TiaPortal portal, MenuSelectionProvider<IEngineeringObject> menuSelectionProvider,string projPath, string version)
        {
            //MessageBox.Show("2");
            try
            {
                //LocalSession multiuserLocalSession = portal.LocalSessions[0];
                //Markings markings = multiuserLocalSession.MarkingService.GetMarkings();
                //MarkingComposition allMarkings = markings.AllMarkings;

                //List<string> markingObj = new List<string>();
                //foreach (Marking marking in allMarkings)
                //{
                //    IEngineeringObject markedObject = marking.MarkedObject;
                //    MarkState markState = marking.MarkState;
                //    var att = markedObject.GetAttribute("Name");
                //    markingObj.Add(att.ToString());
                //}
                //string message = "Please Check In Project First";
                ////            string message = markingObj.Any()
                ////? string.Join(Environment.NewLine, markingObj)
                ////: "No marked objects found.";
                //if (markingObj.Count() > 0)
                //{
                //    MessageBox.Show(
                //        message,
                //        "Marked Object",
                //        MessageBoxButtons.OK,
                //        MessageBoxIcon.Information
                //    );
                //    return message;
                //}
                //MessageBox.Show("StartExe");
                //MessageBox.Show(version);
                if (version == "18")
                {
                    //MessageBox.Show(version);

                    string exePath = @"C:\Project\DownloadGrantAccess\DownloadEnableEXE\bin\Debug\GrantAccessEXE.exe";//@"C:\GrantAccess\Debug\GrantAccessEXE.exe";//@"C:\Project\DownloadGrantAccess\New folder (2)\GrantAccessEXE\bin\Debug\GrantAccessEXE.exe";//@"C:\Project\MultiUser_ProjDownload_AddIn\New folder (2)\ConsoleApp6 - Copy (2) - Copy\bin\Debug\ConsoleApp6.exe"; //@"C:\Project\MultiUser_ProjDownload_AddIn\ConsoleApp6 - Copy (2) - Copy\bin\Debug\ConsoleApp6.exe";//@"C:\DownloadAddin\Debug\ConsoleApp6.exe";//@"C:\Generate Block\Debug\ConsoleApp6.exe";
                                                                                                                      //"C:\Project\MultiUser_ProjDownload_AddIn\ConsoleApp6 - Copy (2) - Copy\bin\Debug\ConsoleApp6.exe"
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        FileName = exePath,
                        Arguments = $"\"{projPath}\" \"{""}\"",  // ✅ Correct arguments format
                        UseShellExecute = false,
                        CreateNoWindow = true  // ✅ Hides console window
                    };

                    Process cliProcess = new Process
                    {
                        EnableRaisingEvents = true,
                        StartInfo = startInfo
                    };

                    cliProcess.Start();
                }
                else if (version == "19")
                {
                    //MessageBox.Show("Version " + version);
                    string exePath = @"C:\DownloadAccessAddin\Debug\Debug_19\GrantAccessEXE_V19.exe";//@"C:\GrantAccess\Debug\GrantAccessEXE.exe";//@"C:\Project\DownloadGrantAccess\New folder (2)\GrantAccessEXE\bin\Debug\GrantAccessEXE.exe";//@"C:\Project\MultiUser_ProjDownload_AddIn\New folder (2)\ConsoleApp6 - Copy (2) - Copy\bin\Debug\ConsoleApp6.exe"; //@"C:\Project\MultiUser_ProjDownload_AddIn\ConsoleApp6 - Copy (2) - Copy\bin\Debug\ConsoleApp6.exe";//@"C:\DownloadAddin\Debug\ConsoleApp6.exe";//@"C:\Generate Block\Debug\ConsoleApp6.exe";
                                                                                                                      //"C:\Project\MultiUser_ProjDownload_AddIn\ConsoleApp6 - Copy (2) - Copy\bin\Debug\ConsoleApp6.exe"
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        FileName = exePath,
                        Arguments = $"\"{projPath}\" \"{""}\"",  // ✅ Correct arguments format
                        UseShellExecute = false,
                        CreateNoWindow = true  // ✅ Hides console window
                    };

                    Process cliProcess = new Process
                    {
                        EnableRaisingEvents = true,
                        StartInfo = startInfo
                    };

                    cliProcess.Start();
                }
                else if(version == "20")
                {
                    string exePath = @"C:\DownloadAccessAddin\Debug\Debug_V20\GrantAccessEXE_V20.exe";//@"C:\GrantAccess\Debug\GrantAccessEXE.exe";//@"C:\Project\DownloadGrantAccess\New folder (2)\GrantAccessEXE\bin\Debug\GrantAccessEXE.exe";//@"C:\Project\MultiUser_ProjDownload_AddIn\New folder (2)\ConsoleApp6 - Copy (2) - Copy\bin\Debug\ConsoleApp6.exe"; //@"C:\Project\MultiUser_ProjDownload_AddIn\ConsoleApp6 - Copy (2) - Copy\bin\Debug\ConsoleApp6.exe";//@"C:\DownloadAddin\Debug\ConsoleApp6.exe";//@"C:\Generate Block\Debug\ConsoleApp6.exe";
                    //MessageBox.Show(version);                                                                          //"C:\Project\MultiUser_ProjDownload_AddIn\ConsoleApp6 - Copy (2) - Copy\bin\Debug\ConsoleApp6.exe"
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        FileName = exePath,
                        Arguments = $"\"{projPath}\" \"{""}\"",  // ✅ Correct arguments format
                        UseShellExecute = false,
                        CreateNoWindow = true  // ✅ Hides console window
                    };

                    Process cliProcess = new Process
                    {
                        EnableRaisingEvents = true,
                        StartInfo = startInfo
                    };

                    cliProcess.Start();
                }
                //var selectedElement = menuSelectionProvider.GetSelection<IEngineeringObject>().First();


                //var device1 = multiuserLocalSession.Project.Devices.First();
                //var deviceItem1 = device1.DeviceItems
                //         .FirstOrDefault(item => item.Classification == DeviceItemClassifications.CPU);


                //MessageBox.Show(device1.Name);
                //if (deviceItem1 == null)
                //{
                //    MessageBox.Show("Null1");
                //}

                //DownloadProvider downloadProvider1 = deviceItem1.GetService<DownloadProvider>();
                //if (downloadProvider1 == null)
                //{
                //    MessageBox.Show("Null3");
                //}

                //var onlineProvider2 = deviceItem1.GetService<OnlineProvider>();
                //if (onlineProvider2 == null)
                //{
                //    MessageBox.Show("Null2");
                //}
                //var configuration2 = onlineProvider2.Configuration;
                //try
                //{
                //    if (!configuration2.EnableLegacyCommunication) configuration2.EnableLegacyCommunication = true;
                //}
                //catch(Exception ex)
                //{
                //    MessageBox.Show("Error4 : " + ex.Message);
                //}
                //try
                //{

                //    if (downloadProvider1 != null)
                //    {
                //        ConnectionConfiguration configuration = downloadProvider1.Configuration;

                //        ConfigurationMode configurationMode = configuration.Modes.Find("PN/IE");

                //        ConfigurationPcInterface pcInterface = configurationMode.PcInterfaces.Find("PLCSIM", 1);
                //        foreach (var pcInterface1 in configurationMode.PcInterfaces)
                //        {
                //            Console.WriteLine($"Name: {pcInterface1.Name}, Slot: {pcInterface1.Number}");
                //        }
                //        IConfiguration targetConfiguration = pcInterface.TargetInterfaces[0];

                //        ConfigurationTargetInterface targetConfigurationIF = targetConfiguration as ConfigurationTargetInterface;

                //        ConfigurationAddressComposition downloadAddresses = targetConfigurationIF.Addresses;
                //        try
                //        {
                //            ConfigurationAddress onlineAddress = downloadAddresses.Create("192.168.0.1");
                //        }
                //        catch(Exception ex)
                //        {
                //            MessageBox.Show("Error6 : "+ ex.Message);
                //        }
                //        DownloadConfigurationDelegate preDownloadDelegate = PreConfigureDownload;

                //        DownloadConfigurationDelegate postDownloadDelegate = PostConfigureDownload;

                //        //DownloadOptions loadOption = DownloadOptions.Hardware | DownloadOptions.Software;
                //        DownloadOptions loadOption = DownloadOptions.SoftwareOnlyChanges;
                //        try
                //        {
                //            DownloadResult result = downloadProvider1.Download(targetConfiguration, preDownloadDelegate, postDownloadDelegate, loadOption);

                //            MessageBox.Show("Download Done");
                //        }
                //        catch (Exception ex)
                //        {
                //            MessageBox.Show("Error 1: " + ex.Message);
                //        }
                //    }
                //}
                //catch(Exception ex1)
                //{
                //    MessageBox.Show("Error2 :" + ex1.Message);
                //}

                //MessageBox.Show(multiuserLocalSession.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error 3" + ex.Message);
            }
                return "";
        }
        internal static string downloadDisable(string batFilePath, TiaPortal portal, MenuSelectionProvider<IEngineeringObject> menuSelectionProvider, string projPath)
        {
            //MessageBox.Show("Enter");
            try
            {

                string exePath = @"C:\Project\DownloadGrantAccess\DownloadEnableEXE\bin\Debug\GrantAccessEXE.exe";
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = exePath,
                    Arguments = $"\"{"0"}\" \"{""}\"",  // ✅ Correct arguments format
                    UseShellExecute = false,
                    CreateNoWindow = true  // ✅ Hides console window
                };

                Process cliProcess = new Process
                {
                    EnableRaisingEvents = true,
                    StartInfo = startInfo
                };

                cliProcess.Start();
                //string exePath = @"C:\Project\DownloadGrantAccess\New folder (2)\GrantAccessEXE\bin\Debug\GrantAccessEXE.exe";//@"C:\GrantAccess\Debug\GrantAccessEXE.exe";//@"C:\Project\DownloadGrantAccess\New folder (2)\GrantAccessEXE\bin\Debug\GrantAccessEXE.exe";//@"C:\Project\MultiUser_ProjDownload_AddIn\New folder (2)\ConsoleApp6 - Copy (2) - Copy\bin\Debug\ConsoleApp6.exe"; //@"C:\Project\MultiUser_ProjDownload_AddIn\ConsoleApp6 - Copy (2) - Copy\bin\Debug\ConsoleApp6.exe";//@"C:\DownloadAddin\Debug\ConsoleApp6.exe";//@"C:\Generate Block\Debug\ConsoleApp6.exe";
                //"C:\Project\MultiUser_ProjDownload_AddIn\ConsoleApp6 - Copy (2) - Copy\bin\Debug\ConsoleApp6.exe"
                //string processName = Path.GetFileNameWithoutExtension(exePath);
                //string exePath = @"C:\Project\DownloadGrantAccess\DownloadEnableEXE\bin\Debug\GrantAccessEXE.exe";
                //string processName = Path.GetFileNameWithoutExtension(exePath);

                //foreach (var proc in System.Diagnostics.Process.GetProcessesByName(processName))
                //{
                //    try
                //    {
                //        if (proc.MainModule.FileName.Equals(exePath, StringComparison.OrdinalIgnoreCase))
                //        {
                //            proc.Kill();
                //            proc.WaitForExit();
                //        }
                //    }
                //    catch(Exception ex)
                //    {
                //        MessageBox.Show("Error Close Exe " + ex.Message);
                //        // Access denied – ignore
                //    }
                //}
                //ProcessStartInfo startInfo = new ProcessStartInfo()
                //{
                //    FileName = exePath,
                //    Arguments = $"\"{projPath}\" \"{""}\"",  // ✅ Correct arguments format
                //    UseShellExecute = false,
                //    CreateNoWindow = true  // ✅ Hides console window
                //};

                //Process cliProcess = new Process
                //{
                //    EnableRaisingEvents = true,
                //    StartInfo = startInfo
                //};

                //cliProcess.Start();

            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error 3" + ex.Message);
            }
            return "";
        }
        private static void PreConfigureDownload(DownloadConfiguration downloadConfiguration)
        {
            StopModules stopModules = downloadConfiguration as StopModules;
            if (stopModules != null)
            {
                stopModules.CurrentSelection = StopModulesSelections.StopAll; // This selection will set PLC into "Stop" mode
                return;
            }
            AlarmTextLibrariesDownload alarmTextLibraries = downloadConfiguration as AlarmTextLibrariesDownload;
            if (alarmTextLibraries != null)
            {
                alarmTextLibraries.CurrentSelection = AlarmTextLibrariesDownloadSelections.ConsistentDownload;
                return;
            }
            BlockBindingPassword blockBindingPassword = downloadConfiguration as BlockBindingPassword;
            if (blockBindingPassword != null)
            {
                //SecureString password = ;
                //blockBindingPassword.SetPassword(password);
                return;
            }
            CheckBeforeDownload checkBeforeDownload = downloadConfiguration as CheckBeforeDownload;
            if (checkBeforeDownload != null)
            {
                checkBeforeDownload.Checked = true;
                return;
            }
            ConsistentBlocksDownload consistentBlocksDownload = downloadConfiguration as ConsistentBlocksDownload;
            if (consistentBlocksDownload != null)
            {
                consistentBlocksDownload.CurrentSelection = ConsistentBlocksDownloadSelections.ConsistentDownload;
                return;
            }
            //ModuleWriteAccessPassword moduleWriteAccessPassword = downloadConfiguration as ModuleWriteAccessPassword;
            //if (moduleWriteAccessPassword != null)
            //{
            //    SecureString password = '121233';// Get PLC protection level password from a secure location
            //    moduleWriteAccessPassword.SetPassword(password);
            //    return;
            //}
            throw new NotSupportedException(); // Exception thrown in the delagate will cancel download
        }
        private static void PostConfigureDownload(DownloadConfiguration downloadConfiguration)
        {
            StartModules startModules = downloadConfiguration as StartModules;
            if (startModules != null)
            {
                startModules.CurrentSelection = StartModulesSelections.StartModule; // Sets PLC in "Run" mode 
            }
        }
        /// <summary>
        /// Workaround for Add-In windows to be shown in foreground of TIA Portal
        /// Creates new invisible form window on the top of the screen
        /// </summary>
        /// <param name="title">Title of the window (optional)</param>
        /// <returns>Newly created form window on top of all other windows</returns>
        private static Form GetForegroundWindow(string title = null)
        {
            Form form = new Form { Opacity = 0, ShowIcon = false, Text = title };
            form.Show();
            form.TopMost = true;
            form.Activate();
            form.TopMost = false;
            return form;
        }

        /// <summary>
        /// Shows some information of a selected plc block in a message box
        /// </summary>
        /// <param name="block">selected block</param>
        internal static void ShowBlockInfo(PlcBlock block)
        {
            string title = "AddIn - Programblock information";
            string content = "Name: " + block.Name + "\nNumber: " + block.Number + "\nIs compiled: " + block.IsConsistent;
            using (Form owner = GetForegroundWindow())
            {
                MessageBox.Show(owner, content, title);
            }
        }


        static string SafeGetValue(List<string> lines, int index, string defaultValue = "")
        {
            if (index >= lines.Count) return defaultValue;

            var parts = lines[index].Split(',');
            if (parts.Length < 2) return defaultValue;

            string value = parts[1].Trim();
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }
        
        private static string Encrypt(string plainText)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(bytes);
        }

        private static string Decrypt(string encryptedText)
        {
            byte[] bytes = Convert.FromBase64String(encryptedText);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        // 🧩 Simple popup input dialog
        private static (string, string) PromptUserCredentials(string existingUser = "")
        {
            Form prompt = new Form()
            {
                Width = 350,
                Height = 220,
                Text = "Enter Octoplant Credentials",
                StartPosition = FormStartPosition.CenterScreen
            };

            Label lblUser = new Label() { Left = 20, Top = 20, Text = "Username:", Width = 280 };
            TextBox txtUser = new TextBox() { Left = 20, Top = 45, Width = 280, Text = existingUser };

            Label lblPass = new Label() { Left = 20, Top = 85, Text = "Password:", Width = 280 };
            TextBox txtPass = new TextBox() { Left = 20, Top = 110, Width = 280, UseSystemPasswordChar = true };

            Button ok = new Button() { Text = "OK", Left = 120, Width = 80, Top = 150, DialogResult = DialogResult.OK };

            prompt.Controls.Add(lblUser);
            prompt.Controls.Add(txtUser);
            prompt.Controls.Add(lblPass);
            prompt.Controls.Add(txtPass);
            prompt.Controls.Add(ok);
            prompt.AcceptButton = ok;

            if (prompt.ShowDialog() == DialogResult.OK)
                return (txtUser.Text, txtPass.Text);
            else
                return ("", "");
        }
    }
}
