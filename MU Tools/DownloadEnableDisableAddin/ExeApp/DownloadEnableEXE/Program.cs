using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Siemens.Engineering;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.Multiuser;
using Siemens.Engineering.Umac;
using static System.Net.Mime.MediaTypeNames;

namespace GrantAccessEXE
{
    class Program
    {
        // =============================
        // CONFIGURATION (FROM ARGS)
        // =============================
        //static string TiaVersion;
        //static string ServerName;
        //static string Username;
        //static string Password;
        //static string UmacConnected;
        //static string UmacDisconnected;
        //static int IntervalSeconds = 5;
        static string TiaVersion = "18.0";
        static string ServerName = "nb012026";
        static string Username = "Manager";
        static string Password = "Manager@123";
        static string UmacConnected = @"C:\Project\MultiUser_ProjDownload_AddIn\ServerConnected.json";
        static string UmacDisconnected = @"C:\Project\MultiUser_ProjDownload_AddIn\ServerDisConnected.json";
        static int IntervalSeconds = 5;

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

        public static string ApplicationKeyGenerate(string data)
        {
            var hexData = ToHexString(data);
            return AddDash(hexData);
        }
        public void ReadData()
        {
            KeyData data = null;

            if (File.Exists(DatFile))
            {
                try
                {
                    data = (KeyData)Serialization.DeSerialize<KeyData>(DatFile);
                }
                catch
                {
                    MessageBox.Show("License file corrupted. Creating new license.");
                    data = new KeyData();
                    SaveKeyData();
                }
            }
            else
            {
                // Create new file if missing
                data = new KeyData();
                SaveKeyData();
            }
            data = new KeyData();
            // Apply values
            ApplicationKey = data.ApplicationKey ?? "";
            ValidationKey = data.ValidationKey ?? "";

            // Check expiry (2 years)
            var expiry = data.RegisterDate.AddDays(15);
            if (DateTime.Now >= expiry)
            {
                MessageBox.Show("Your licence expired.\nPlease contact the administrator.");
                ValidationKey = "";
            }
        }
        private void GetApplicationKey(string fullDeviseName)
        {
            string mac = GetMacAddress();
            string device = fullDeviseName.Split('.')[1];
            string pc = SystemInformation.ComputerName;

            string requestDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

            string data =
                $"{mac}.APL.{device}.PC.{requestDateTime}.wipropari";

            ApplicationKey = AddDash(ToHexString(data));
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
        private string _applicationKey;
        private static string DatFile = @"C:\GrantAccess\Debug\Data.dat";
        private string _validationKey;

        private bool? _dialogResult;
        private bool _canCloseDialog;

        private bool _isRegister;

        public static string ApplicationKey = "";
        public static string ValidationKey = "";
        public static string FromHexString(string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return Encoding.Default.GetString(bytes);
        }
        public bool VerifyKey()
        {
            if (string.IsNullOrEmpty(ValidationKey))
                return false;

            string raw = FromHexString(ValidationKey.Replace("-", ""));
            var parts = raw.Split('.');

            if (parts.Length < 6 || parts[1] != "VAL")
                return false;

            if (!DateTime.TryParseExact(
                    parts[4],
                    "yyyyMMddHHmmss",
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime expiry))
                return false;

            if (DateTime.Now > expiry)
            {
                MessageBox.Show("License expired. Contact administrator.");
                return false;
            }

            return true;
        }
        public class LicenseForm : Form
        {
            public string EnteredValidationKey = "";

            private TextBox txtAppKey;
            private TextBox txtValidation;
            private Button btnRegister;

            public LicenseForm(string applicationKey)
            {
                Width = 500;
                Height = 250;
                Text = "Key Validation";
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                StartPosition = FormStartPosition.CenterScreen;

                // Application Key Label
                Label lblAppKey = new Label()
                {
                    Text = "Application Key",
                    Left = 20,
                    Top = 30,
                    Width = 120,
                    Font = new Font("Segoe UI", 10)
                };

                txtAppKey = new TextBox()
                {
                    Left = 150,
                    Top = 25,
                    Width = 300,
                    ReadOnly = true,
                    Text = applicationKey
                };

                // Validation Key Label
                Label lblValidation = new Label()
                {
                    Text = "Validation Key",
                    Left = 20,
                    Top = 80,
                    Width = 120,
                    Font = new Font("Segoe UI", 10)
                };

                txtValidation = new TextBox()
                {
                    Left = 150,
                    Top = 75,
                    Width = 300
                };

                // Register Button
                btnRegister = new Button()
                {
                    Text = "Register",
                    Left = 200,
                    Top = 130,
                    Width = 100,
                    Enabled = true
                };
                btnRegister.Click += BtnRegister_Click;

                Controls.Add(lblAppKey);
                Controls.Add(txtAppKey);
                Controls.Add(lblValidation);
                Controls.Add(txtValidation);
                Controls.Add(btnRegister);
            }

            private void BtnRegister_Click(object sender, EventArgs e)
            {
                EnteredValidationKey = txtValidation.Text.Trim();

                if (string.IsNullOrEmpty(EnteredValidationKey))
                {
                    MessageBox.Show("Please enter a Validation Key.", "Validation Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            }
        }
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
        public class Serialization
        {
            public static bool Serialize<T>(string FileName, T cls)
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream file = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None);
                    using (file)
                    {
                        bf.Serialize(file, cls);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }

            }

            public static object DeSerialize<T>(string FileName)
            {
                try
                {

                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream file = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.None);
                    using (file)
                    {

                        return bf.Deserialize(file);
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }

            }
        }
        private static void KillPreviousInstance()
        {
            Process current = Process.GetCurrentProcess();

            var oldProcesses = Process.GetProcessesByName(current.ProcessName)
                                      .Where(p => p.Id != current.Id);

            foreach (var proc in oldProcesses)
            {
                try
                {
                    proc.Kill();
                    proc.WaitForExit();
                }
                catch
                {
                    // ignore access denied / already exited
                }
            }
        }
        public static ProjectServer ShowProjectServerSelection(TiaPortal portal)
        {
            ProjectServer selectedServer = null;

            using (Form form = new Form())
            {
                form.Text = "Select Project Server";
                form.Width = 400;
                form.Height = 180;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;

                Label lblTitle = new Label
                {
                    Text = "Select Project Server:",
                    Left = 20,
                    Top = 20,
                    AutoSize = true
                };

                ComboBox cmbServers = new ComboBox
                {
                    Left = 20,
                    Top = 45,
                    Width = 340,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };

                Button btnOk = new Button
                {
                    Text = "OK",
                    Left = 260,
                    Top = 90,
                    Width = 100
                };

                var servers = portal.ProjectServers.ToList();

                if (servers.Count == 0)
                {
                    MessageBox.Show("No Project Servers found.", "Error");
                    return null;
                }

                cmbServers.DataSource = servers;
                cmbServers.DisplayMember = "ServerName";
                //cmbServers.SelectedIndex = 0;

                btnOk.Click += (s, e) =>
                {
                    selectedServer = cmbServers.SelectedItem as ProjectServer;
                    form.DialogResult = DialogResult.OK;
                    form.Close();
                };

                form.Controls.Add(lblTitle);
                form.Controls.Add(cmbServers);
                form.Controls.Add(btnOk);

                form.ShowDialog();
            }

            return selectedServer;
        }
        static void Main(string[] args)
        {
            KillPreviousInstance();
            //try
            //{
            //    Application.EnableVisualStyles();
            //    Application.SetCompatibleTextRenderingDefault(false);
            //    Program p = new Program();
            //    p.ReadData();

            //    // If no application key exists → generate
            //    if (string.IsNullOrEmpty(ApplicationKey))
            //    {
            //        //MessageBox.Show("3");
            //        string deviceName = Environment.MachineName + ".PC";
            //        p.GetApplicationKey(deviceName);
            //    }

            //    // If no validation key OR wrong validation key → show license form
            //    if (string.IsNullOrEmpty(ValidationKey) || !p.VerifyKey())
            //    {
            //        LicenseForm lf = new LicenseForm(ApplicationKey);

            //        if (lf.ShowDialog() == DialogResult.OK)
            //        {
            //            ValidationKey = lf.EnteredValidationKey;

            //            if (!p.VerifyKey())
            //            {
            //                MessageBox.Show("Invalid Validation Key.");
            //                return;
            //            }

            //            // Save Data.dat
            //            SaveKeyData();
            //        }
            //        else
            //        {
            //            return; // user closed window
            //        }
            //    }
            //    //MessageBox.Show("4");
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
            ParseArguments(args);

            Console.WriteLine("🔍 TIA Project Server UMAC Watchdog starting...");

            // =============================
            // ATTACH TO TIA PORTAL
            // =============================
            TiaPortal portal = AttachTiaPortal(TiaVersion);
            if (portal == null)
            {
                Console.WriteLine("❌ No running TIA Portal session found.");
                return;
            }

            LocalSession project = portal.LocalSessions.FirstOrDefault();
            var ps = portal.ProjectServers;
            

            var projectServers = portal.ProjectServers.ToList();
            if (project == null)
            {
                MessageBox.Show("❌ No active TIA project session found. Aborting.");
                return;
            }
            ProjectServer server = ShowProjectServerSelection(portal);
            //TiaPortal newprotal = new TiaPortal(TiaPortalMode.WithUserInterface);
            //newprotal.Authentication += TiaPortal_Authentication;

            //Project umacProject = newprotal.Projects.Open(new FileInfo(@"C:\Project\MultiUser_ProjDownload_AddIn\Test_ProjV1\Project1\Project1.ap18"));
            //
            Console.WriteLine("✅ TIA Portal session attached");
            string a = "1";
            if (args[0] == "0")
            {
                MessageBox.Show("Download rights removed, please relogin");
                //MessageBox.Show(args[0]);
                stopprocess(project);
                
            }
            // =============================
            // WATCHDOG LOOP
            // =============================
            //string lastState = null;
            //int count = 0;
            //try
            //{
            //    while (true)
            //    {
            //        count++;
            //        //ServerName = "parisrv18";
            //        //MessageBox.Show(count.ToString());
            //        string state = GetProjectServerState(portal, ServerName);

            //        string selectedUmac =
            //            state == "CONNECTED" ? UmacConnected : UmacDisconnected;
            //        addcoustomRights(project);
            //                if(count == 1)
            //                {
            //                    MessageBox.Show("Download Access Granted Please Relogin");
            //                }
            //        if (state != lastState)
            //        {
            //            //if (state == "CONNECTED")
            //            //{
            //                Console.WriteLine($"🔄 State change → {state}");

            //            //}
            //            if (state == "DISCONNECTED")
            //            {
            //                //removeCustomRight(project);
            //                MessageBox.Show("Server Is Diconnected");
            //                //return;
            //            }
            //                // -----------------------------
            //            // PLACEHOLDER: Encrypt UMAC
            //            // -----------------------------
            //            EncryptUmacConfig(selectedUmac, Password);

            //            // -----------------------------
            //            // PLACEHOLDER: Import UMAC
            //            // -----------------------------
            //            //int rc = ImportUmacConfig(project, selectedUmac, Password);

            //            //Console.WriteLine($"✅ UMAC applied ({state}), return code = {rc}");
            //            lastState = state;
            //        }
            //        else
            //        {
            //            Console.WriteLine($"⏸ No change ({state})");
            //        }

            //        Thread.Sleep(5 * 1000);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("\n🛑 Watchdog stopped manually. " + ex.Message);
            //}
            string lastState = null;
            int count = 0;

            while (true)
            {
                try
                {
                    count++;
                    ServerName = server.ServerName;
                    string state = GetProjectServerState(portal, ServerName);

                    string selectedUmac =
                        state == "CONNECTED" ? UmacConnected : UmacDisconnected;
                    project = portal.LocalSessions.FirstOrDefault();
                                        
                    if (lastState == "DISCONNECTED" && state == "CONNECTED")
                    {
                        MessageBox.Show("Server Connected, Download Rights Provided, Please Relogin");
                    }
                    if (state == "CONNECTED")
                    {
                        if (count == 1)
                        {
                            MessageBox.Show("Download Access Granted Please Relogin");
                        }
                        addcoustomRights(project);
                    }
                    if (state != lastState)
                    {
                        Console.WriteLine($"🔄 State change → {state}");
                        
                        if (state == "DISCONNECTED")
                        {
                            MessageBox.Show("Server Is Disconnected");
                            removeCustomRight(project);
                        }

                        EncryptUmacConfig(selectedUmac, Password);
                        lastState = state;
                    }
                    else
                    {
                        Console.WriteLine($"⏸ No change ({state})");
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("The object is no longer available."))
                    {
                        portal = AttachTiaPortal(TiaVersion);
                        Console.WriteLine("🔁 Re-attached to TIA Portal");
                    }
                    else
                    {
                        Console.WriteLine("⚠ Error: " + ex.Message);
                    }
                }

                Thread.Sleep(5 * 1000);
            }
        }

        private static void TiaPortal_Authentication(object sender, AuthenticationEventArgs e)
        {
            e.AuthenticationTypeProvider = AuthenticationTypeProvider.Credentials;
            e.UmacCredentials.Type = UmacUserType.Project;
            e.UmacCredentials.Name = "Contributor";
            e.UmacCredentials.SetPassword(GetSecureString("Contributor@123"));
        }
        private static SecureString GetSecureString(string password)
        {
            SecureString secureString = new SecureString();
            foreach (char c in password)
            {
                secureString.AppendChar(c);
            }
            return secureString;
        }
        static void stopprocess(LocalSession project)
        {
            UmacConfigurator umacConfiguratorService = project.Project.GetService<UmacConfigurator>();
            var projectUsers = umacConfiguratorService.ProjectUsers;

            var projectUser = projectUsers.Find("Manager");

            foreach (var v1 in projectUsers)
            {

            }
            //projectUser.Deactivate();
            var customRoles = umacConfiguratorService.CustomRoles;
            CustomRole customRole = customRoles.Find("Contributor");

            var engineeringFunctionRights = umacConfiguratorService.EngineeringFunctionRights;
            EngineeringFunctionRight downloadOtherDeviceengineeringFunctionRight = engineeringFunctionRights.Find("DownloadToOtherDevices");
            EngineeringFunctionRight downloadPLCengineeringFunctionRight = engineeringFunctionRights.Find("DownloadPLC");
            EngineeringFunctionRight downloadHMIDeviceengineeringFunctionRight = engineeringFunctionRights.Find("Download");
            EngineeringFunctionRight downloadToDrivesengineeringFunctionRight = engineeringFunctionRights.Find("DownloadDriveFunctionRight");
            try
            {

                customRole.AssignedEngineeringRights.Remove(downloadOtherDeviceengineeringFunctionRight);
            }
            catch
            {

            }
            try
            {
                customRole.AssignedEngineeringRights.Remove(downloadPLCengineeringFunctionRight);
            }
            catch
            {

            }
            try
            {
                customRole.AssignedEngineeringRights.Remove(downloadHMIDeviceengineeringFunctionRight);
            }
            catch
            {

            }
            try
            {
                customRole.AssignedEngineeringRights.Remove(downloadToDrivesengineeringFunctionRight);
            }
            catch
            {

            }
            project.Save();

            Process[] processes = Process.GetProcessesByName("GrantAccessEXE");

            foreach (Process proc in processes)
            {
                try
                {
                    if (!proc.HasExited)
                    {
                        proc.Kill();          // Force close
                        proc.WaitForExit();   // Wait until fully closed
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Close exe " + ex.Message);
                }
            }
            //projectUser.Activate();
            //foreach (var sr in customRoles)
            //{
            //    string systemRoleName = sr.Name;
            //}

            //var engineeringFunctionRights = umacConfiguratorService.EngineeringFunctionRights;
            //EngineeringFunctionRight engineeringFunctionRight = engineeringFunctionRights.Find("DownloadToOtherDevices");


            //var engineeringFunctionRights = customRole.AssignedEngineeringRights;
            //var engineeringFunctionRightss = umacConfiguratorService.EngineeringFunctionRights;
        }
        static void addcoustomRights(LocalSession project)
        {
            UmacConfigurator umacConfiguratorService = project.Project.GetService<UmacConfigurator>();
            var projectUsers = umacConfiguratorService.ProjectUsers;

            var projectUser = projectUsers.Find("Manager");

            foreach (var v1 in projectUsers)
            {

            }
            //projectUser.Deactivate();
            var customRoles = umacConfiguratorService.CustomRoles;
            CustomRole customRole = customRoles.Find("Contributor");

            var engineeringFunctionRights = umacConfiguratorService.EngineeringFunctionRights;
            EngineeringFunctionRight downloadOtherDeviceengineeringFunctionRight = engineeringFunctionRights.Find("DownloadToOtherDevices");
            EngineeringFunctionRight downloadPLCengineeringFunctionRight = engineeringFunctionRights.Find("DownloadPLC");
            EngineeringFunctionRight downloadHMIDeviceengineeringFunctionRight = engineeringFunctionRights.Find("Download");
            EngineeringFunctionRight downloadToDrivesengineeringFunctionRight = engineeringFunctionRights.Find("DownloadDriveFunctionRight");
            try
            {

                customRole.AssignedEngineeringRights.Add(downloadOtherDeviceengineeringFunctionRight);
            }
            catch
            {

            }
            try
            {
                customRole.AssignedEngineeringRights.Add(downloadPLCengineeringFunctionRight);
            }
            catch
            {

            }
            try
            {
                customRole.AssignedEngineeringRights.Add(downloadHMIDeviceengineeringFunctionRight);
            }
            catch
            {

            }
            try
            {
                customRole.AssignedEngineeringRights.Add(downloadToDrivesengineeringFunctionRight);
            }
            catch
            {

            }
            //projectUser.Activate();
            //foreach (var sr in customRoles)
            //{
            //    string systemRoleName = sr.Name;
            //}

            //var engineeringFunctionRights = umacConfiguratorService.EngineeringFunctionRights;
            //EngineeringFunctionRight engineeringFunctionRight = engineeringFunctionRights.Find("DownloadToOtherDevices");


            //var engineeringFunctionRights = customRole.AssignedEngineeringRights;
            //var engineeringFunctionRightss = umacConfiguratorService.EngineeringFunctionRights;
        }
        static void removeCustomRight(LocalSession project)
        {
            UmacConfigurator umacConfiguratorService = project.Project.GetService<UmacConfigurator>();
            var projectUsers = umacConfiguratorService.ProjectUsers;
            
            var projectUser = projectUsers.Find("Manager");

            foreach (var v1 in projectUsers)
            {

            }
            //projectUser.Deactivate();
            var customRoles = umacConfiguratorService.CustomRoles;
            CustomRole customRole = customRoles.Find("Contributor");

            var engineeringFunctionRights = umacConfiguratorService.EngineeringFunctionRights;
            EngineeringFunctionRight downloadOtherDeviceengineeringFunctionRight = engineeringFunctionRights.Find("DownloadToOtherDevices");
            EngineeringFunctionRight downloadPLCengineeringFunctionRight = engineeringFunctionRights.Find("DownloadPLC");
            EngineeringFunctionRight downloadHMIDeviceengineeringFunctionRight = engineeringFunctionRights.Find("Download");
            EngineeringFunctionRight downloadToDrivesengineeringFunctionRight = engineeringFunctionRights.Find("DownloadDriveFunctionRight");
            try
            {

                customRole.AssignedEngineeringRights.Remove(downloadOtherDeviceengineeringFunctionRight);
            }
            catch
            {

            }
            try
            {
                customRole.AssignedEngineeringRights.Remove(downloadPLCengineeringFunctionRight);
            }
            catch
            {

            }
            try
            {
                customRole.AssignedEngineeringRights.Remove(downloadHMIDeviceengineeringFunctionRight);
            }
            catch
            {

            }
            try
            {
                customRole.AssignedEngineeringRights.Remove(downloadToDrivesengineeringFunctionRight);
            }
            catch
            {

            }
            project.Save();
            //project.Close();
            //projectUser.Activate();
            //foreach (var sr in customRoles)
            //{
            //    string systemRoleName = sr.Name;
            //}

            //var engineeringFunctionRights = umacConfiguratorService.EngineeringFunctionRights;
            //EngineeringFunctionRight engineeringFunctionRight = engineeringFunctionRights.Find("DownloadToOtherDevices");


            //var engineeringFunctionRights = customRole.AssignedEngineeringRights;
            //var engineeringFunctionRightss = umacConfiguratorService.EngineeringFunctionRights;
        }
        // =============================
        // ARGUMENT PARSING
        // =============================
        static void ParseArguments(string[] args)
        {
            //foreach (string arg in args)
            //{
            //    string[] kv = arg.Split('=');
            //    if (kv.Length != 2) continue;

            //    switch (kv[0].ToLower())
            //    {
            //        case "--tia_version": TiaVersion = kv[1]; break;
            //        case "--server": ServerName = kv[1]; break;
            //        case "--username": Username = kv[1]; break;
            //        case "--password": Password = kv[1]; break;
            //        case "--umac_connected": UmacConnected = kv[1]; break;
            //        case "--umac_disconnected": UmacDisconnected = kv[1]; break;
            //        case "--interval": IntervalSeconds = int.Parse(kv[1]); break;
            //    }
            //}

            //if (string.IsNullOrEmpty(ServerName))
            //    throw new ArgumentException("❌ --server is required");
        }

        // =============================
        // TIA PORTAL ATTACH
        // =============================
        static TiaPortal AttachTiaPortal(string version)
        {
            //TiaPortal portal1 = new TiaPortal();
            foreach (var proc in TiaPortal.GetProcesses())
            {
                try
                {
                    return proc.Attach();
                }
                catch
                {
                    // ignore and continue
                }
            }
            return null;
        }

        // =============================
        // PROJECT SERVER STATE CHECK
        // =============================
        static string GetProjectServerState(TiaPortal portal, string serverName)
        {
            try
            {
                var projectServer = portal.ProjectServers
                    .FirstOrDefault(s => s.ServerName.Equals(serverName, StringComparison.OrdinalIgnoreCase));

                if (projectServer == null)
                    return "DISCONNECTED";

                // Force server access
                projectServer.GetServerProjects();

                return "CONNECTED";
            }
            catch (EngineeringException ex)
            {
                string msg = ex.Message.ToLower();

                if (msg.Contains("login") || msg.Contains("authentication"))
                {
                    Console.WriteLine("ℹ️ Server reachable (authentication required) → treating as CONNECTED");
                    return "CONNECTED";
                }

                if (msg.Contains("cannot be reached"))
                {
                    Console.WriteLine("❌ Server not reachable");
                    return "DISCONNECTED";
                }

                Console.WriteLine($"⚠️ Unknown Openness error → treating as DISCONNECTED\n{ex.Message}");
                return "DISCONNECTED";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Unexpected error → DISCONNECTED\n{ex.Message}");
                return "DISCONNECTED";
            }
        }

        // =============================
        // PLACEHOLDERS (UMAC)
        // =============================

        static void EncryptUmacConfig(string umacFilePath, string secret)
        {
            // ❗ Python-only functionality in Siemens
            // Implement using:
            // 1) External Python call
            // 2) Siemens internal tools
            // 3) CLI wrapper
            Console.WriteLine($"🔐 Encrypting UMAC: {umacFilePath}");
        }

        static int ImportUmacConfig(Project project, string umacFilePath, string secret)
        {
            // ❗ Python-only functionality in Siemens
            Console.WriteLine($"📥 Importing UMAC into project: {umacFilePath}");
            return 0; // success
        }
    }
}