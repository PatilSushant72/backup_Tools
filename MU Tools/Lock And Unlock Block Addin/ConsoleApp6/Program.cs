//////////using FlaUI.Core;
//////////using FlaUI.Core.AutomationElements;
//////////using FlaUI.Core.Definitions;
//////////using FlaUI.Core.Input;
//////////using FlaUI.Core.Tools;
//////////using FlaUI.Core.WindowsAPI;
//////////using FlaUI.UIA3;
//////////using System;
//////////using System.Diagnostics;
//////////using System.Linq;
//////////using System.Threading;
//////////using System.Windows.Forms;
//////////using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
////////////using static TiaPortalAutomation;
//////////using Application = System.Windows.Forms.Application;
//////////using Button = System.Windows.Forms.Button;
//////////using NetworkInterface = System.Net.NetworkInformation.NetworkInterface;
//////////namespace ProtectViaUI
//////////{
//////////    public static class BlockProtectionHelper
//////////    {
//////////        // ------------------------------
//////////        // ENTRY POINT (Console App)
//////////        // ------------------------------


//////////        [STAThread]   // IMPORTANT for TIA Add-In launching EXE
//////////        static void Main(string[] args)
//////////        {
//////////            Application.EnableVisualStyles();
//////////            Application.SetCompatibleTextRenderingDefault(false);

//////////            try
//////////            {
//////////                MessageBox.Show(args[1]);
//////////                // Attach to TIA Portal process
//////////                var tiaProcess = Process.GetProcessesByName("Siemens.Automation.Portal")
//////////                                        .FirstOrDefault(p => p.MainWindowTitle.Contains(args[1]));

//////////                if (tiaProcess == null)
//////////                {
//////////                    MessageBox.Show("TIA Portal process not found.");
//////////                    return;
//////////                }

//////////                using (var app = FlaUI.Core.Application.Attach(tiaProcess))
//////////                using (var automation = new UIA3Automation())
//////////                {
//////////                    var mainWindow = app.GetMainWindow(automation);

//////////                    if (mainWindow == null)
//////////                    {
//////////                        MessageBox.Show("Main TIA window not found.");
//////////                        return;
//////////                    }

//////////                    if (args[0] == "ProtectBlock")
//////////                        BlockProtectionHelper.ApplyProtection(mainWindow, automation);
//////////                    else if (args[0] == "Unprotect")
//////////                        BlockProtectionHelper.UnprotectBlock(mainWindow, automation);
//////////                    else if (args[0] == "ProtectExisting")
//////////                        BlockProtectionHelper.protectExisting(mainWindow, automation);
//////////                }

//////////                //MessageBox.Show("Protection applied successfully!");
//////////            }
//////////            catch (Exception ex)
//////////            {
//////////                MessageBox.Show("Error:\n" + ex.Message);
//////////            }
//////////        }


//////////        //static void Main(string[] args)
//////////        //{
//////////        //    // 1. Attach to TIA Portal process
//////////        //    var tiaProcess = Process.GetProcessesByName("Siemens.Automation.Portal")
//////////        //                            .FirstOrDefault(p => p.MainWindowTitle.Contains("Project2"));


//////////        //    if (tiaProcess == null)
//////////        //    {
//////////        //        Console.WriteLine("TIA Portal not found.");
//////////        //        return;
//////////        //    }

//////////        //    // 2. Attach FlaUI to TIA Portal window
//////////        //    using (var app = FlaUI.Core.Application.Attach(tiaProcess))
//////////        //    using (var automation = new UIA3Automation())
//////////        //    {
//////////        //        var mainWindow = app.GetMainWindow(automation);

//////////        //        if (mainWindow == null)
//////////        //        {
//////////        //            Console.WriteLine("Main TIA window not found.");
//////////        //            return;
//////////        //        }

//////////        //        // 3. CALL ApplyProtection CORRECTLY
//////////        //        ApplyProtection(mainWindow, automation);

//////////        //        Console.WriteLine("Protection applied successfully!");
//////////        //    }
//////////        //}
//////////        public static void protectExisting(Window mainWindow, UIA3Automation automation)
//////////        {
//////////            const string password = "12345";

//////////            mainWindow.Focus();
//////////            Thread.Sleep(500);

//////////            SendKeys.SendWait("%{ENTER}");
//////////            Thread.Sleep(1500);

//////////            var desktop = automation.GetDesktop();
//////////            var propsWindow = desktop.FindFirstDescendant(cf =>
//////////                cf.ByName("Protection")
//////////                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text)));

//////////            propsWindow.Focus();
//////////            propsWindow.Click();

//////////            var propsWindow2 = desktop.FindFirstDescendant(cf =>
//////////           cf.ByName("checkBoxWriteProtection")
//////////             .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.CheckBox)));
//////////            propsWindow2.Click();

//////////            Thread.Sleep(5000);
//////////            var pwdBox = Retry.WhileNull(
//////////         () => automation.GetDesktop().FindFirstDescendant(cf =>
//////////             cf.ByControlType(ControlType.Edit)
//////////               .And(cf.ByName("AccessPasswordText"))),
//////////         TimeSpan.FromSeconds(5)
//////////     ).Result?.AsTextBox();


//////////            //newPwdBox.Enter(password);
//////////            Thread.Sleep(50);

//////////            // 5. Click OK button
//////////            //var desktop = automation.GetDesktop();
//////////            var okBtn = desktop.FindFirstDescendant(cf =>
//////////                cf.ByName("m_OKButton.Text")
//////////                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//////////            //if (switchLangMenu != null)
//////////            //{
//////////            //propsWindow.Focus();
//////////            okBtn.Click();

//////////            AutomationElement tooltip;

//////////            do
//////////            {
//////////                tooltip = desktop.FindFirstDescendant(cf =>
//////////                    cf.ByName("AccessPasswordTextRolloutTip")
//////////                      .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.ToolTip)));


//////////                if (tooltip != null)
//////////                {
//////////                    //MessageBox.Show("Out0");
//////////                    // Password wrong
//////////                    MessageBox.Show("Password is wrong. Please enter correct password.");

//////////                    break;

//////////                    Thread.Sleep(4000); // Wait for user to type again

//////////                    // Click OK again after user enters new password
//////////                    okBtn?.Click();
//////////                    Thread.Sleep(300);
//////////                }
//////////                else
//////////                {
//////////                    //MessageBox.Show("Out1");
//////////                    break;
//////////                }

//////////            } while (tooltip != null);

//////////            if (tooltip != null)
//////////                return;


//////////            var okBtn4 = desktop.FindFirstDescendant(cf =>
//////////             cf.ByName("m_OKButton.Text")
//////////               .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//////////            //if (switchLangMenu != null)
//////////            //{
//////////            //propsWindow.Focus();
//////////            okBtn4.Click();
//////////            // Step 8 – Final OK
//////////            //propsWindow.FindFirstDescendant(cf => cf.ByName("OK"))
//////////            //           ?.AsButton()?.Invoke();

//////////            Thread.Sleep(50);
//////////            MessageBox.Show("Protection applied successfully!");
//////////        }

//////////        public static void UnprotectBlock(Window mainWindow, UIA3Automation automation)
//////////        {
//////////            mainWindow.Focus();
//////////            Thread.Sleep(500);

//////////            SendKeys.SendWait("%{ENTER}");
//////////            Thread.Sleep(1500);

//////////            var desktop = automation.GetDesktop();

//////////            // Open protection window
//////////            var propsWindow = desktop.FindFirstDescendant(cf =>
//////////                cf.ByName("Protection")
//////////                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text)));

//////////            propsWindow?.Click();

//////////            // Disable write protection
//////////            var checkBox = desktop.FindFirstDescendant(cf =>
//////////                cf.ByName("checkBoxWriteProtection")
//////////                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.CheckBox)));

//////////            checkBox?.Click();

//////////            // Wait for user to type password manually
//////////            Thread.Sleep(4000);

//////////            // Locate password box
//////////            var pwdBox = Retry.WhileNull(
//////////                () => automation.GetDesktop().FindFirstDescendant(cf =>
//////////                    cf.ByControlType(ControlType.Edit)
//////////                      .And(cf.ByName("AccessPasswordText"))),
//////////                TimeSpan.FromSeconds(5)
//////////            ).Result?.AsTextBox();

//////////            Thread.Sleep(100);

//////////            // First OK Button click
//////////            var okBtn = desktop.FindFirstDescendant(cf =>
//////////                cf.ByName("m_OKButton.Text")
//////////                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//////////            okBtn?.Click();
//////////            Thread.Sleep(300);

//////////            // ---------- Tooltip check loop ----------
//////////            AutomationElement tooltip;
//////////            do
//////////            {
//////////                tooltip = desktop.FindFirstDescendant(cf =>
//////////                    cf.ByName("AccessPasswordTextRolloutTip")
//////////                      .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.ToolTip)));

//////////                if (tooltip != null)
//////////                {
//////////                    //MessageBox.Show("Out0");
//////////                    // Password wrong
//////////                    MessageBox.Show("Password is wrong. Please enter correct password.");

//////////                    break;

//////////                    Thread.Sleep(4000); // Wait for user to type again

//////////                    // Click OK again after user enters new password
//////////                    okBtn?.Click();
//////////                    Thread.Sleep(300);
//////////                }
//////////                else
//////////                {
//////////                    //MessageBox.Show("Out1");
//////////                    break;
//////////                }

//////////            } while (tooltip != null);

//////////            if (tooltip != null)
//////////                return;
//////////            // -----------------------------------------
//////////            MessageBox.Show("Out2");
//////////            // Tooltip gone → password correct → final OK click
//////////            var okBtn2 = desktop.FindFirstDescendant(cf =>
//////////                cf.ByName("m_OKButton.Text")
//////////                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//////////            okBtn2?.Click();
//////////            MessageBox.Show("Protection applied successfully!");
//////////        }

//////////        // ------------------------------
//////////        // APPLY BLOCK PROTECTION SEQUENCE
//////////        // ------------------------------
//////////        public static void ApplyProtection(Window mainWindow, UIA3Automation automation)
//////////        {
//////////            const string password = "12345";

//////////            //// Step 1 – ALT+ENTER
//////////            //Keyboard.Press(VirtualKeyShort.ALT);
//////////            //Keyboard.Press(VirtualKeyShort.ENTER);
//////////            //Keyboard.Release(VirtualKeyShort.ENTER);
//////////            //Keyboard.Release(VirtualKeyShort.ALT);
//////////            //Thread.Sleep(1000);
//////////            // =============================
//////////            // FIXED: Open block Properties
//////////            // =============================
//////////            mainWindow.Focus();
//////////            Thread.Sleep(500);

//////////            // Click selected block in tree
//////////            //var selectedBlock = mainWindow.FindFirstDescendant(cf =>
//////////            //    cf.ByControlType(ControlType.DataItem))
//////////            //    ?.AsAutomationElement();

//////////            //selectedBlock?.Click();
//////////            //Thread.Sleep(500);

//////////            // Send ALT + ENTER (Open Properties)
//////////            SendKeys.SendWait("%{ENTER}");
//////////            Thread.Sleep(1500);

//////////            // Step 2 – Find Properties window
//////////            //var propsWindow = Retry.WhileNull(
//////////            //    () => automation.GetDesktop().FindFirstDescendant(cf =>
//////////            //        cf.ByControlType(ControlType.Window)
//////////            //          .And(cf.ByName("General"))),
//////////            //    TimeSpan.FromSeconds(5)
//////////            //).Result?.AsWindow();

//////////            //if (propsWindow == null)
//////////            //    throw new Exception("Properties popup not found.");
//////////            var desktop = automation.GetDesktop();
//////////            var propsWindow = desktop.FindFirstDescendant(cf =>
//////////                cf.ByName("Protection")
//////////                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text)));

//////////            //if (switchLangMenu != null)
//////////            //{
//////////            propsWindow.Focus();
//////////            propsWindow.Click();
//////////            //    //Console.WriteLine("Focused 'Switch programming language' menu.");
//////////            //    // Step 3 – Protection tab
//////////            //    propsWindow.FindFirstDescendant(cf => cf.ByName("Protection"))
//////////            //           ?.AsTabItem()?.Select();
//////////            //Thread.Sleep(700);

//////////            // Step 4 – Define password
//////////            var propsWindow1 = desktop.FindFirstDescendant(cf =>
//////////               cf.ByName("buttonWriteProtection")
//////////                 .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));
//////////            //propsWindow.FindFirstDescendant(cf => cf.ByName("button WriteProtection"))
//////////            //           .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text)));
//////////            propsWindow1.Focus();
//////////            propsWindow1.Click();

//////////            // Step 5 – Enter password popup #1
//////////            EnterPassword(automation, password);

//////////            // Step 6 – Tick write protection checkbox
//////////            //propsWindow.FindFirstDescendant(cf => cf.ByAutomationId("checkBoxWriteProtection"))
//////////            //           ?.AsCheckBox()?.Click();

//////////        }
//////////        // ------------------------------
//////////        // ENTER PASSWORD IN POPUP
//////////        // ------------------------------
//////////        private static void EnterPassword(UIA3Automation automation, string pwd)
//////////        {
//////////            // 1. Find the password popup window (ANY window containing password fields)
//////////            var popup = Retry.WhileNull(
//////////                () => automation.GetDesktop().FindFirstDescendant(cf =>
//////////                    cf.ByControlType(ControlType.TitleBar)
//////////                      .And(cf.ByName("TitleBar")
//////////                           )),
//////////                TimeSpan.FromSeconds(5)
//////////            ).Result?.AsWindow();

//////////            if (popup == null)
//////////                throw new Exception("Password popup not found.");

//////////            Thread.Sleep(300);

//////////            // 2. Find New Password textbox using AutomationId
//////////            //var newPwdBox = popup.WhileNull(
//////////            //    () => automation.GetDesktop().FindFirstDescendant(cf =>
//////////            //        cf.ByControlType(ControlType.Edit)
//////////            //          .And(cf.ByName("NewPasswordText")));
//////////            //var newPwdBox = Retry.WhileNull(
//////////            //     () => automation.GetDesktop().FindFirstDescendant(cf =>
//////////            //         cf.ByControlType(ControlType.TitleBar)
//////////            //           .And(cf.ByName("NewPasswordText")
//////////            //                )));

//////////            //var confirmPwdBox = Retry.WhileNull(
//////////            //    () => automation.GetDesktop().FindFirstDescendant(cf =>
//////////            //        cf.ByControlType(ControlType.TitleBar)
//////////            //          .And(cf.ByName("ConfirmPasswordText")
//////////            //               )));

//////////            //// 3. Find Confirm Password textbox using AutomationId
//////////            ////var confirmPwdBox = popup.FindFirstDescendant(cf =>
//////////            ////    cf.ByAutomationId("ConfirmPasswordText"))?.AsTextBox();

//////////            //if (newPwdBox == null || confirmPwdBox == null)
//////////            //    throw new Exception("Password textboxes not found in popup.");

//////////            //// 4. Enter password in both fields
//////////            //newPwdBox.Enter(pwd);
//////////            //Thread.Sleep(200);

//////////            //confirmPwdBox.Enter(pwd);
//////////            //Thread.Sleep(200);
//////////            // 1. Locate NEW PASSWORD textbox
//////////            //var newPwdBox = Retry.WhileNull(
//////////            //    () => automation.GetDesktop().FindFirstDescendant(cf =>
//////////            //        cf.ByControlType(ControlType.Edit)
//////////            //          .And(cf.ByName("NewPasswordText"))),


//////////            //    TimeSpan.FromSeconds(5)
//////////            //).Result?.AsTextBox();

//////////            Thread.Sleep(4000);
//////////            var newpwdBox = Retry.WhileNull(
//////////         () => automation.GetDesktop().FindFirstDescendant(cf =>
//////////             cf.ByControlType(ControlType.Edit)
//////////               .And(cf.ByName("NewPasswordText"))),
//////////         TimeSpan.FromSeconds(5)
//////////     ).Result?.AsTextBox();


//////////            //newPwdBox.Focus();
//////////            //newPwdBox.Click();

//////////            // 2. Locate CONFIRM PASSWORD textbox
//////////            //var confirmPwdBox = Retry.WhileNull(
//////////            //    () => automation.GetDesktop().FindFirstDescendant(cf =>
//////////            //        cf.ByControlType(ControlType.Edit)
//////////            //          .And(cf.ByName("ConfirmPasswordText"))),
//////////            //    TimeSpan.FromSeconds(5)
//////////            //).Result?.AsTextBox();

//////////            Thread.Sleep(4000);
//////////            var confpwdBox = Retry.WhileNull(
//////////         () => automation.GetDesktop().FindFirstDescendant(cf =>
//////////             cf.ByControlType(ControlType.Edit)
//////////               .And(cf.ByName("ConfirmPasswordText"))),
//////////         TimeSpan.FromSeconds(5)
//////////     ).Result?.AsTextBox();

//////////            //if (string.IsNullOrEmpty(newpwdBox.Text) || string.IsNullOrEmpty(confpwdBox.Text))
//////////            //    throw new Exception("Password textboxes not found in popup.");

//////////            // 3. Enter password in both fields
//////////            //newPwdBox.Enter(pwd);
//////////            //Thread.Sleep(50);

//////////            //confirmPwdBox.Enter(pwd);
//////////            Thread.Sleep(50);

//////////            // 5. Click OK button
//////////            var desktop = automation.GetDesktop();
//////////            var okBtn = desktop.FindFirstDescendant(cf =>
//////////                cf.ByName("m_OKButton.Text")
//////////                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//////////            //if (switchLangMenu != null)
//////////            //{
//////////            //propsWindow.Focus();
//////////            okBtn.Click();
//////////            //var okBtn = popup.FindFirstDescendant(cf =>
//////////            //    cf.ByControlType(ControlType.Button)
//////////            //      .And(cf.ByName("OK")))?.AsButton();

//////////            //okBtn?.Invoke();

//////////            //Thread.Sleep(600);
//////////            var propsWindow2 = desktop.FindFirstDescendant(cf =>
//////////            cf.ByName("checkBoxWriteProtection")
//////////              .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.CheckBox)));
//////////            propsWindow2.Click();
//////////            //Thread.Sleep(700);

//////////            // Step 7 – Enter password popup #2
//////////            //  var confirmPwdBox3 = Retry.WhileNull(
//////////            //    () => automation.GetDesktop().FindFirstDescendant(cf =>
//////////            //        cf.ByControlType(ControlType.Edit)
//////////            //          .And(cf.ByName("AccessPasswordText"))),

//////////            //    TimeSpan.FromSeconds(5)
//////////            //).Result?.AsTextBox();

//////////            //  //EnterPassword(automation, password);
//////////            //  confirmPwdBox3.Enter(pwd);
//////////            Thread.Sleep(4000);
//////////            var confirmPwdBox3 = Retry.WhileNull(
//////////         () => automation.GetDesktop().FindFirstDescendant(cf =>
//////////             cf.ByControlType(ControlType.Edit)
//////////               .And(cf.ByName("AccessPasswordText"))),
//////////         TimeSpan.FromSeconds(5)
//////////     ).Result?.AsTextBox();

//////////            Thread.Sleep(50);
//////////            var okBtn2 = desktop.FindFirstDescendant(cf =>
//////////               cf.ByName("m_OKButton.Text")
//////////                 .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//////////            //if (switchLangMenu != null)
//////////            //{
//////////            //propsWindow.Focus();
//////////            okBtn2.Click();

//////////            var okBtn4 = desktop.FindFirstDescendant(cf =>
//////////             cf.ByName("m_OKButton.Text")
//////////               .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//////////            //if (switchLangMenu != null)
//////////            //{
//////////            //propsWindow.Focus();
//////////            okBtn4.Click();
//////////            // Step 8 – Final OK
//////////            //propsWindow.FindFirstDescendant(cf => cf.ByName("OK"))
//////////            //           ?.AsButton()?.Invoke();

//////////            Thread.Sleep(50);
//////////            MessageBox.Show("Protection applied successfully!");
//////////        }
//////////    }

//////////}

//////////////////////////////using System;
//////////////////////////////using System.Diagnostics;
//////////////////////////////using System.IO;
//////////////////////////////using System.IO.Compression;
//////////////////////////////using System.Linq;
//////////////////////////////using System.Security.Cryptography;
//////////////////////////////using System.Text;
//////////////////////////////using System.Threading;
//////////////////////////////using System.Windows.Forms;
//////////////////////////////using System.Xml.Linq;
//////////////////////////////using OpenQA.Selenium;
//////////////////////////////using OpenQA.Selenium.Edge;
//////////////////////////////using OpenQA.Selenium.Support.UI;


////////////////////////////////Final Working Code 
//////////////////////////////using FlaUI.Core.Tools;
//////////////////////////////using FlaUI.UIA3;
//////////////////////////////using System;
//////////////////////////////using System.Runtime.InteropServices;
//////////////////////////////using System.Windows.Forms;
//////////////////////////////using FlaUI.Core;
////////////////////////////////using FlaUI.Core.AutomationElements;
//////////////////////////////using FlaUI.Core.Definitions;
//////////////////////////////using FlaUI.Core.Input;
//////////////////////////////using FlaUI.Core.Tools;
//////////////////////////////using FlaUI.UIA3;
//////////////////////////////using System;
//////////////////////////////using System.Diagnostics;
//////////////////////////////using System.Linq;
//////////////////////////////using System.Threading;
//////////////////////////////using System.Windows.Forms;
//////////////////////////////using System.Threading;
//////////////////////////////using System.Windows.Forms;
//////////////////////////////using Application = System.Windows.Forms.Application;
////////////////////////////////public static class WindowHider
//////////////////////////////namespace OneDriveOctoplantAutomator
//////////////////////////////{
//////////////////////////////    internal static class Program
//////////////////////////////    {
//////////////////////////////        // Public settings accessible from LicenseForm
//////////////////////////////        public static string ApplicationKey { get; private set; } = "";
//////////////////////////////        public static string ValidationKey { get; set; } = "";
//////////////////////////////        public static string Username { get; set; } = "";
//////////////////////////////        public static string Password { get; set; } = "";

//////////////////////////////        // Where to store remembered credentials
//////////////////////////////        private static readonly string CredFilePath = Path.Combine(
//////////////////////////////            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
//////////////////////////////            "OneDriveOctoplantAutomator", "credentials.dat");

//////////////////////////////        [STAThread]
//////////////////////////////        static void Main()
//////////////////////////////        {
//////////////////////////////            Application.EnableVisualStyles();
//////////////////////////////            Application.SetCompatibleTextRenderingDefault(false);

//////////////////////////////            // Generate application key (simple deterministic key using MAC + machine name)
//////////////////////////////            ApplicationKey = GenerateApplicationKey();

//////////////////////////////            // Show license/login form first (only AppKey + ValidationKey + Login)
//////////////////////////////            using (var form = new LicenseForm(ApplicationKey))
//////////////////////////////            {
//////////////////////////////                var result = form.ShowDialog();
//////////////////////////////                if (result != DialogResult.OK)
//////////////////////////////                {
//////////////////////////////                    MessageBox.Show("Validation failed or cancelled. Application will exit.", "Stopped",
//////////////////////////////                        MessageBoxButtons.OK, MessageBoxIcon.Information);
//////////////////////////////                    return;
//////////////////////////////                }
//////////////////////////////            }

//////////////////////////////            // After license/login OK, run automation
//////////////////////////////            try
//////////////////////////////            {
//////////////////////////////                RunAutomation();
//////////////////////////////            }
//////////////////////////////            catch (Exception ex)
//////////////////////////////            {
//////////////////////////////                MessageBox.Show("Fatal error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//////////////////////////////            }
//////////////////////////////        }

//////////////////////////////        #region Application Key Generation

//////////////////////////////        private static string GenerateApplicationKey()
//////////////////////////////        {
//////////////////////////////            // Build a stable unique string: first available MAC + machine name
//////////////////////////////            string mac = GetMacAddress() ?? Guid.NewGuid().ToString("N");
//////////////////////////////            string name = Environment.MachineName;
//////////////////////////////            string raw = $"{mac}.APL.{name}";
//////////////////////////////            string hex = ToHexString(raw);
//////////////////////////////            return InsertDashes(hex, 8);
//////////////////////////////        }

//////////////////////////////        private static string GetMacAddress()
//////////////////////////////        {
//////////////////////////////            try
//////////////////////////////            {
//////////////////////////////                var nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
//////////////////////////////                foreach (var nic in nics)
//////////////////////////////                {
//////////////////////////////                    var pa = nic.GetPhysicalAddress();
//////////////////////////////                    if (pa != null && pa.ToString().Length >= 6)
//////////////////////////////                    {
//////////////////////////////                        if (!nic.Description.ToLower().Contains("virtual") && !nic.Description.ToLower().Contains("loopback"))
//////////////////////////////                        {
//////////////////////////////                            return pa.ToString();
//////////////////////////////                        }
//////////////////////////////                    }
//////////////////////////////                }
//////////////////////////////                return nics.FirstOrDefault()?.GetPhysicalAddress().ToString();
//////////////////////////////            }
//////////////////////////////            catch
//////////////////////////////            {
//////////////////////////////                return null;
//////////////////////////////            }
//////////////////////////////        }

//////////////////////////////        private static string ToHexString(string input)
//////////////////////////////        {
//////////////////////////////            var bytes = Encoding.UTF8.GetBytes(input);
//////////////////////////////            var sb = new StringBuilder(bytes.Length * 2);
//////////////////////////////            foreach (var b in bytes) sb.Append(b.ToString("X2"));
//////////////////////////////            return sb.ToString();
//////////////////////////////        }

//////////////////////////////        private static string InsertDashes(string s, int every)
//////////////////////////////        {
//////////////////////////////            var sb = new StringBuilder();
//////////////////////////////            for (int i = 0; i < s.Length; i++)
//////////////////////////////            {
//////////////////////////////                if (i > 0 && i % every == 0) sb.Append('-');
//////////////////////////////                sb.Append(s[i]);
//////////////////////////////            }
//////////////////////////////            return sb.ToString();
//////////////////////////////        }

//////////////////////////////        #endregion

//////////////////////////////        #region Credential Storage (DPAPI)

//////////////////////////////        private static void SaveCredentialsToDisk(string user, string pwd)
//////////////////////////////        {
//////////////////////////////            try
//////////////////////////////            {
//////////////////////////////                var plaintext = $"{user}|{pwd}";
//////////////////////////////                var bytes = Encoding.UTF8.GetBytes(plaintext);
//////////////////////////////                var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
//////////////////////////////                Directory.CreateDirectory(Path.GetDirectoryName(CredFilePath));
//////////////////////////////                File.WriteAllBytes(CredFilePath, encrypted);
//////////////////////////////            }
//////////////////////////////            catch (Exception ex)
//////////////////////////////            {
//////////////////////////////                Console.WriteLine("Failed to save credentials: " + ex.Message);
//////////////////////////////            }
//////////////////////////////        }

//////////////////////////////        private static bool TryLoadCredentials(out string user, out string pwd)
//////////////////////////////        {
//////////////////////////////            user = null; pwd = null;
//////////////////////////////            try
//////////////////////////////            {
//////////////////////////////                if (!File.Exists(CredFilePath)) return false;
//////////////////////////////                var encrypted = File.ReadAllBytes(CredFilePath);
//////////////////////////////                var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
//////////////////////////////                var plain = Encoding.UTF8.GetString(decrypted);
//////////////////////////////                var parts = plain.Split('|');
//////////////////////////////                if (parts.Length == 2)
//////////////////////////////                {
//////////////////////////////                    user = parts[0];
//////////////////////////////                    pwd = parts[1];
//////////////////////////////                    return true;
//////////////////////////////                }
//////////////////////////////                return false;
//////////////////////////////            }
//////////////////////////////            catch
//////////////////////////////            {
//////////////////////////////                return false;
//////////////////////////////            }
//////////////////////////////        }

//////////////////////////////        #endregion

//////////////////////////////        #region Automation Runner

//////////////////////////////        public static void RunAutomation()
//////////////////////////////        {
//////////////////////////////            // Configuration - adjust these URLs/paths as needed
//////////////////////////////            string folderUrl = "https://parirobotics-my.sharepoint.com/:f:/g/personal/priti_d_3_wipropari_com/Erg__9H--6dOmdd1Q6nh5X8Bub1Vx4aqyHhWf5tQt5Wzvg?e=ySlYX1";
//////////////////////////////            string downloadDir = @"C:\OneDriveDownload";
//////////////////////////////            string extractDir = @"C:\OneDriveDownload\TempExtract";

//////////////////////////////            Console.WriteLine("==== Automation started ====");
//////////////////////////////            Console.WriteLine($"OneDrive URL: {folderUrl}");
//////////////////////////////            Console.WriteLine($"Download dir: {downloadDir}");
//////////////////////////////            Console.WriteLine($"Extract dir: {extractDir}");

//////////////////////////////            // 1) Clean download folder
//////////////////////////////            SafeDeleteDirectoryContents(downloadDir);
//////////////////////////////            Directory.CreateDirectory(downloadDir);

//////////////////////////////            // 2) Open OneDrive in Edge and click Download
//////////////////////////////            var edgeOptions = new EdgeOptions();
//////////////////////////////            edgeOptions.AddArgument("--start-maximized");
//////////////////////////////            edgeOptions.AddUserProfilePreference("download.prompt_for_download", false);
//////////////////////////////            edgeOptions.AddUserProfilePreference("download.default_directory", downloadDir);

//////////////////////////////            using (var driver = new EdgeDriver(edgeOptions))
//////////////////////////////            {
//////////////////////////////                driver.Navigate().GoToUrl(folderUrl);

//////////////////////////////                // wait until body present
//////////////////////////////                var loadWait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
//////////////////////////////                loadWait.Until(d => d.FindElement(By.TagName("body")));

//////////////////////////////                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
//////////////////////////////                IWebElement downloadBtn = wait.Until(drv =>
//////////////////////////////                {
//////////////////////////////                    try
//////////////////////////////                    {
//////////////////////////////                        var btn = drv.FindElement(By.CssSelector("button[data-id='download']"));
//////////////////////////////                        if (btn != null) return btn;
//////////////////////////////                    }
//////////////////////////////                    catch { }

//////////////////////////////                    try
//////////////////////////////                    {
//////////////////////////////                        var icon = drv.FindElement(By.CssSelector("i[data-icon-name='Download']"));
//////////////////////////////                        if (icon != null) return icon;
//////////////////////////////                    }
//////////////////////////////                    catch { }

//////////////////////////////                    return null;
//////////////////////////////                });

//////////////////////////////                if (downloadBtn == null)
//////////////////////////////                    throw new Exception("Download button not found on OneDrive page.");

//////////////////////////////                try
//////////////////////////////                {
//////////////////////////////                    downloadBtn.Click();
//////////////////////////////                }
//////////////////////////////                catch
//////////////////////////////                {
//////////////////////////////                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", downloadBtn);
//////////////////////////////                }

//////////////////////////////                Console.WriteLine("Download initiated, waiting for ZIP to complete...");
//////////////////////////////                WaitForDownload(downloadDir, TimeSpan.FromMinutes(5));
//////////////////////////////                Console.WriteLine("Download finished.");
//////////////////////////////            }

//////////////////////////////            //// 3) Extract zip (clean extract folder first)
//////////////////////////////            //string zipFile = Directory.GetFiles(downloadDir, "*.zip").OrderByDescending(f => File.GetLastWriteTime(f)).FirstOrDefault();
//////////////////////////////            //if (zipFile == null) throw new Exception("No zip downloaded.");
//////////////////////////////            //SafeDeleteDirectoryContents(extractDir);
//////////////////////////////            //Directory.CreateDirectory(extractDir);
//////////////////////////////            //Console.WriteLine("Extracting: " + zipFile);
//////////////////////////////            //ZipFile.ExtractToDirectory(zipFile, extractDir, true);
//////////////////////////////            //File.Delete(zipFile);
//////////////////////////////            // 3) Extract zip (clean extract folder first)
//////////////////////////////            string zipFile = Directory.GetFiles(downloadDir, "*.zip")
//////////////////////////////                                      .OrderByDescending(f => File.GetLastWriteTime(f))
//////////////////////////////                                      .FirstOrDefault();

//////////////////////////////            if (zipFile == null)
//////////////////////////////                throw new Exception("No zip downloaded.");

//////////////////////////////            SafeDeleteDirectoryContents(extractDir);
//////////////////////////////            Directory.CreateDirectory(extractDir);

//////////////////////////////            Console.WriteLine("Extracting: " + zipFile);

//////////////////////////////            // Manual extract with overwrite support (required in .NET Framework 4.8)
//////////////////////////////            using (ZipArchive archive = ZipFile.OpenRead(zipFile))
//////////////////////////////            {
//////////////////////////////                foreach (ZipArchiveEntry entry in archive.Entries)
//////////////////////////////                {
//////////////////////////////                    string fullPath = Path.Combine(extractDir, entry.FullName);
//////////////////////////////                    string directory = Path.GetDirectoryName(fullPath);

//////////////////////////////                    if (!Directory.Exists(directory))
//////////////////////////////                        Directory.CreateDirectory(directory);

//////////////////////////////                    if (entry.FullName.EndsWith("/")) // Directory inside ZIP
//////////////////////////////                        continue;

//////////////////////////////                    entry.ExtractToFile(fullPath, true); // Overwrite = true
//////////////////////////////                }
//////////////////////////////            }

//////////////////////////////            File.Delete(zipFile);

//////////////////////////////            // 4) Find all projects (.vdog-sdn files)
//////////////////////////////            var vdogFiles = Directory.GetFiles(extractDir, "*.vdog-sdn", SearchOption.AllDirectories);
//////////////////////////////            if (vdogFiles.Length == 0)
//////////////////////////////            {
//////////////////////////////                Console.WriteLine("No .vdog-sdn files found. Nothing to process.");
//////////////////////////////                return;
//////////////////////////////            }

//////////////////////////////            Console.WriteLine($"Found {vdogFiles.Length} project(s).");

//////////////////////////////            // 5) Process each project one by one
//////////////////////////////            foreach (var vdogFile in vdogFiles)
//////////////////////////////            {
//////////////////////////////                try
//////////////////////////////                {
//////////////////////////////                    ProcessSingleProject(vdogFile);
//////////////////////////////                }
//////////////////////////////                catch (Exception ex)
//////////////////////////////                {
//////////////////////////////                    Console.WriteLine($"Error processing {vdogFile}: {ex.Message}");
//////////////////////////////                }
//////////////////////////////            }

//////////////////////////////            Console.WriteLine("==== Automation finished ====");
//////////////////////////////        }

//////////////////////////////        private static void ProcessSingleProject(string vdogFile)
//////////////////////////////        {
//////////////////////////////            Console.WriteLine("\n-------------------------------------------");
//////////////////////////////            Console.WriteLine("Processing project: " + vdogFile);
//////////////////////////////            Console.WriteLine("-------------------------------------------");

//////////////////////////////            // 1) Read DirR
//////////////////////////////            XDocument xml = XDocument.Load(vdogFile);
//////////////////////////////            string dirRValue = xml.Descendants("DirR").FirstOrDefault()?.Value;
//////////////////////////////            if (string.IsNullOrEmpty(dirRValue))
//////////////////////////////                throw new Exception("<DirR> not found in " + vdogFile);

//////////////////////////////            // e.g. "\DevRnDProj\Proj2024_25\OctoplantFnTest"
//////////////////////////////            string cleanedDirR = dirRValue.TrimStart('\\', '/'); // "DevRnDProj\Proj2024_25\OctoplantFnTest"
//////////////////////////////            string octoFolderPath = Path.Combine(@"C:\vdClientArchive", cleanedDirR);

//////////////////////////////            Console.WriteLine("Target Octoplant folder: " + octoFolderPath);

//////////////////////////////            // 2) Run checkout
//////////////////////////////            string checkoutExe = @"C:\Program Files (x86)\vdogClient\VDogAutoCheckOut.exe";
//////////////////////////////            string checkoutArgs = "/rd:C:\\vdClientArchive " +
//////////////////////////////                                  $"\"/dirR:{dirRValue}\" " +
//////////////////////////////                                  "/Account:wipropari\\ " +
//////////////////////////////                                  "/Password: " +
//////////////////////////////                                  "/WithBackups:N " +
//////////////////////////////                                  "/NumberOfArchives:1";

//////////////////////////////            Console.WriteLine("Running checkout CLI...");
//////////////////////////////            var (cout, cerr, code) = RunCmd(checkoutExe, checkoutArgs, TimeSpan.FromMinutes(3));
//////////////////////////////            Console.WriteLine(cout);
//////////////////////////////            if (!string.IsNullOrEmpty(cerr)) Console.WriteLine("ERR: " + cerr);

//////////////////////////////            // 3) Ensure checkout folder exists (checkout should create it)
//////////////////////////////            if (!Directory.Exists(octoFolderPath))
//////////////////////////////            {
//////////////////////////////                Console.WriteLine("Warning: Octoplant checkout folder not found. Creating it.");
//////////////////////////////                Directory.CreateDirectory(octoFolderPath);
//////////////////////////////            }

//////////////////////////////            // 4) Clean contents of octoFolderPath but DO NOT delete the folder itself
//////////////////////////////            Console.WriteLine("Cleaning contents of octoplant folder...");
//////////////////////////////            SafeDeleteDirectoryContents(octoFolderPath);

//////////////////////////////            // 5) Move project files (project root is folder that contains vdogFile)
//////////////////////////////            string projectRoot = Path.GetDirectoryName(vdogFile);
//////////////////////////////            if (string.IsNullOrEmpty(projectRoot)) throw new Exception("Project root not found for " + vdogFile);

//////////////////////////////            Console.WriteLine($"Moving files from {projectRoot} -> {octoFolderPath}");

//////////////////////////////            // Move directories
//////////////////////////////            foreach (var dir in Directory.GetDirectories(projectRoot))
//////////////////////////////            {
//////////////////////////////                string dest = Path.Combine(octoFolderPath, Path.GetFileName(dir));
//////////////////////////////                if (Directory.Exists(dest)) Directory.Delete(dest, true);
//////////////////////////////                Directory.Move(dir, dest);
//////////////////////////////            }

//////////////////////////////            // Move files
//////////////////////////////            foreach (var f in Directory.GetFiles(projectRoot))
//////////////////////////////            {
//////////////////////////////                string dest = Path.Combine(octoFolderPath, Path.GetFileName(f));
//////////////////////////////                if (File.Exists(dest)) File.Delete(dest);
//////////////////////////////                File.Move(f, dest);
//////////////////////////////            }

//////////////////////////////            Console.WriteLine("Files moved.");

//////////////////////////////            // 6) Run check-in
//////////////////////////////            string vdogFileName = Path.GetFileName(vdogFile);
//////////////////////////////            string checkinExe = @"C:\Program Files (x86)\vdogClient\VDogAutoCheckIn.exe";
//////////////////////////////            string checkinArgs =
//////////////////////////////                $"/RD:C:\\vdClientArchive\\{cleanedDirR} " +
//////////////////////////////                "/AT:C " +
//////////////////////////////                $"/CFile:\"C:\\vdClientArchive\\{cleanedDirR}\\{vdogFileName}\" " +
//////////////////////////////                $"/Account:{Program.Username} " +
//////////////////////////////                "/Domain:wipropari " +
//////////////////////////////                "/Password:";

//////////////////////////////            Console.WriteLine("Running check-in CLI...");
//////////////////////////////            var (cinOut, cinErr, cinCode) = RunCmd(checkinExe, checkinArgs, TimeSpan.FromMinutes(3));
//////////////////////////////            Console.WriteLine(cinOut);
//////////////////////////////            if (!string.IsNullOrEmpty(cinErr)) Console.WriteLine("ERR: " + cinErr);

//////////////////////////////            // 7) Delete working folder after successful check-in (if exists)
//////////////////////////////            try
//////////////////////////////            {
//////////////////////////////                if (Directory.Exists(octoFolderPath))
//////////////////////////////                {
//////////////////////////////                    Console.WriteLine("Deleting working folder after check-in...");
//////////////////////////////                    Directory.Delete(octoFolderPath, true);
//////////////////////////////                    Console.WriteLine("Working folder deleted.");
//////////////////////////////                }
//////////////////////////////            }
//////////////////////////////            catch (Exception ex)
//////////////////////////////            {
//////////////////////////////                Console.WriteLine("Failed to delete working folder: " + ex.Message);
//////////////////////////////            }
//////////////////////////////        }

//////////////////////////////        #endregion

//////////////////////////////        #region Utilities

//////////////////////////////        private static void SafeDeleteDirectoryContents(string path)
//////////////////////////////        {
//////////////////////////////            try
//////////////////////////////            {
//////////////////////////////                if (!Directory.Exists(path)) return;

//////////////////////////////                // delete subdirectories
//////////////////////////////                foreach (var dir in Directory.GetDirectories(path))
//////////////////////////////                {
//////////////////////////////                    try { Directory.Delete(dir, true); }
//////////////////////////////                    catch { /* ignore individual failures */ }
//////////////////////////////                }

//////////////////////////////                // delete files
//////////////////////////////                foreach (var file in Directory.GetFiles(path))
//////////////////////////////                {
//////////////////////////////                    try { File.Delete(file); }
//////////////////////////////                    catch { /* ignore */ }
//////////////////////////////                }
//////////////////////////////            }
//////////////////////////////            catch (Exception ex)
//////////////////////////////            {
//////////////////////////////                Console.WriteLine("Safe delete contents error: " + ex.Message);
//////////////////////////////            }
//////////////////////////////        }

//////////////////////////////        /// <summary>
//////////////////////////////        /// Wait for a .zip file to appear and finish downloading (no .crdownload and stable size)
//////////////////////////////        /// </summary>
//////////////////////////////        private static void WaitForDownload(string folderPath, TimeSpan timeout)
//////////////////////////////        {
//////////////////////////////            var sw = Stopwatch.StartNew();
//////////////////////////////            long lastSize = -1;
//////////////////////////////            string zip = null;
//////////////////////////////            while (sw.Elapsed < timeout)
//////////////////////////////            {
//////////////////////////////                try
//////////////////////////////                {
//////////////////////////////                    var cr = Directory.GetFiles(folderPath, "*.crdownload").FirstOrDefault();
//////////////////////////////                    zip = Directory.GetFiles(folderPath, "*.zip").OrderByDescending(f => File.GetLastWriteTime(f)).FirstOrDefault();

//////////////////////////////                    if (!string.IsNullOrEmpty(cr))
//////////////////////////////                    {
//////////////////////////////                        Thread.Sleep(1000);
//////////////////////////////                        continue;
//////////////////////////////                    }

//////////////////////////////                    if (zip == null)
//////////////////////////////                    {
//////////////////////////////                        Thread.Sleep(1000);
//////////////////////////////                        continue;
//////////////////////////////                    }

//////////////////////////////                    long size = new FileInfo(zip).Length;
//////////////////////////////                    if (size == lastSize && size > 0)
//////////////////////////////                    {
//////////////////////////////                        // stable size – assume finished
//////////////////////////////                        return;
//////////////////////////////                    }

//////////////////////////////                    lastSize = size;
//////////////////////////////                }
//////////////////////////////                catch
//////////////////////////////                {
//////////////////////////////                    // ignore
//////////////////////////////                }
//////////////////////////////                Thread.Sleep(1000);
//////////////////////////////            }

//////////////////////////////            throw new TimeoutException("Timeout waiting for OneDrive ZIP download to finish.");
//////////////////////////////        }

//////////////////////////////        /// <summary>
//////////////////////////////        /// Run external exe and capture output. Returns (stdout, stderr, exitCode).
//////////////////////////////        /// </summary>
//////////////////////////////        private static (string stdout, string stderr, int exitCode) RunCmd(string exePath, string args, TimeSpan timeout)
//////////////////////////////        {
//////////////////////////////            try
//////////////////////////////            {
//////////////////////////////                var psi = new ProcessStartInfo
//////////////////////////////                {
//////////////////////////////                    FileName = exePath,
//////////////////////////////                    Arguments = args,
//////////////////////////////                    UseShellExecute = false,
//////////////////////////////                    RedirectStandardOutput = true,
//////////////////////////////                    RedirectStandardError = true,
//////////////////////////////                    CreateNoWindow = true
//////////////////////////////                };

//////////////////////////////                using (var proc = Process.Start(psi))
//////////////////////////////                {
//////////////////////////////                    if (proc == null) return ("", "Failed to start process", -1);

//////////////////////////////                    var outputReader = proc.StandardOutput;
//////////////////////////////                    var errorReader = proc.StandardError;

//////////////////////////////                    // Wait with timeout
//////////////////////////////                    if (!proc.WaitForExit((int)timeout.TotalMilliseconds))
//////////////////////////////                    {
//////////////////////////////                        try { proc.Kill(); } catch { }
//////////////////////////////                        return ("", "Process timed out", -2);
//////////////////////////////                    }

//////////////////////////////                    string outText = outputReader.ReadToEnd();
//////////////////////////////                    string errText = errorReader.ReadToEnd();
//////////////////////////////                    int code = proc.ExitCode;
//////////////////////////////                    return (outText, errText, code);
//////////////////////////////                }
//////////////////////////////            }
//////////////////////////////            catch (Exception ex)
//////////////////////////////            {
//////////////////////////////                return ("", "RunCmd exception: " + ex.Message, -1);
//////////////////////////////            }
//////////////////////////////        }

//////////////////////////////        #endregion

//////////////////////////////        #region LicenseForm (WinForms)

//////////////////////////////        private class LicenseForm : Form
//////////////////////////////        {
//////////////////////////////            private readonly TextBox txtAppKey;
//////////////////////////////            private readonly TextBox txtValidationKey;
//////////////////////////////            private readonly TextBox txtUser;
//////////////////////////////            private readonly TextBox txtPassword;
//////////////////////////////            private readonly CheckBox chkRemember;

//////////////////////////////            public LicenseForm(string appKey)
//////////////////////////////            {
//////////////////////////////                Text = "Application Validation";
//////////////////////////////                Width = 520;
//////////////////////////////                Height = 350;
//////////////////////////////                FormBorderStyle = FormBorderStyle.FixedDialog;
//////////////////////////////                MaximizeBox = false;
//////////////////////////////                StartPosition = FormStartPosition.CenterScreen;

//////////////////////////////                int top = 12;
//////////////////////////////                var lbl = new Label() { Text = "Application Key (share this with admin):", Left = 12, Top = top, Width = 400 };
//////////////////////////////                Controls.Add(lbl);

//////////////////////////////                txtAppKey = new TextBox() { Left = 12, Top = top + 22, Width = 480, ReadOnly = true };
//////////////////////////////                txtAppKey.Text = appKey;
//////////////////////////////                Controls.Add(txtAppKey);

//////////////////////////////                top += 62;
//////////////////////////////                var lblVal = new Label() { Text = "Validation Key:", Left = 12, Top = top, Width = 120 };
//////////////////////////////                Controls.Add(lblVal);
//////////////////////////////                txtValidationKey = new TextBox() { Left = 12, Top = top + 22, Width = 480 };
//////////////////////////////                Controls.Add(txtValidationKey);

//////////////////////////////                top += 62;
//////////////////////////////                var lblUser = new Label() { Text = "Username:", Left = 12, Top = top, Width = 120 };
//////////////////////////////                Controls.Add(lblUser);
//////////////////////////////                txtUser = new TextBox() { Left = 12, Top = top + 22, Width = 300 };
//////////////////////////////                Controls.Add(txtUser);

//////////////////////////////                var lblPwd = new Label() { Text = "Password:", Left = 12, Top = top + 52, Width = 120 };
//////////////////////////////                Controls.Add(lblPwd);
//////////////////////////////                txtPassword = new TextBox() { Left = 12, Top = top + 74, Width = 300, PasswordChar = '*' };
//////////////////////////////                Controls.Add(txtPassword);

//////////////////////////////                chkRemember = new CheckBox() { Left = 330, Top = top + 22, Width = 180, Text = "Remember Username & Password" };
//////////////////////////////                Controls.Add(chkRemember);

//////////////////////////////                // Load saved credentials if present
//////////////////////////////                if (TryLoadCredentials(out string savedUser, out string savedPwd))
//////////////////////////////                {
//////////////////////////////                    txtUser.Text = savedUser;
//////////////////////////////                    txtPassword.Text = savedPwd;
//////////////////////////////                    chkRemember.Checked = true;
//////////////////////////////                }

//////////////////////////////                var btnSubmit = new Button() { Left = 360, Top = top + 120, Width = 130, Text = "Validate & Run" };
//////////////////////////////                btnSubmit.Click += BtnSubmit_Click;
//////////////////////////////                Controls.Add(btnSubmit);

//////////////////////////////                var btnCancel = new Button() { Left = 220, Top = top + 120, Width = 130, Text = "Cancel" };
//////////////////////////////                btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
//////////////////////////////                Controls.Add(btnCancel);
//////////////////////////////            }

//////////////////////////////            private void BtnSubmit_Click(object sender, EventArgs e)
//////////////////////////////            {
//////////////////////////////                // Read fields into Program static props
//////////////////////////////                Program.ValidationKey = txtValidationKey.Text.Trim();
//////////////////////////////                Program.Username = txtUser.Text.Trim();
//////////////////////////////                Program.Password = txtPassword.Text;

//////////////////////////////                // Basic validation: entered ValidationKey must equal ApplicationKey
//////////////////////////////                if (string.IsNullOrEmpty(Program.ValidationKey))
//////////////////////////////                {
//////////////////////////////                    MessageBox.Show("Please enter Validation Key.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
//////////////////////////////                    return;
//////////////////////////////                }

//////////////////////////////                if (Program.ValidationKey != Program.ApplicationKey)
//////////////////////////////                {
//////////////////////////////                    MessageBox.Show("Validation Key does not match Application Key. Contact admin.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
//////////////////////////////                    return;
//////////////////////////////                }

//////////////////////////////                // Optionally check username/password presence
//////////////////////////////                if (string.IsNullOrEmpty(Program.Username) || string.IsNullOrEmpty(Program.Password))
//////////////////////////////                {
//////////////////////////////                    MessageBox.Show("Please enter username and password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
//////////////////////////////                    return;
//////////////////////////////                }

//////////////////////////////                // Save credentials if requested
//////////////////////////////                if (chkRemember.Checked)
//////////////////////////////                {
//////////////////////////////                    SaveCredentialsToDisk(Program.Username, Program.Password);
//////////////////////////////                }
//////////////////////////////                else
//////////////////////////////                {
//////////////////////////////                    try { if (File.Exists(CredFilePath)) File.Delete(CredFilePath); } catch { }
//////////////////////////////                }

//////////////////////////////                // All good
//////////////////////////////                DialogResult = DialogResult.OK;
//////////////////////////////                Close();
//////////////////////////////            }
//////////////////////////////        }

//////////////////////////////        #endregion
//////////////////////////////    }
//////////////////////////////}

using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using System;
using System.Diagnostics;
using System;
using System.Diagnostics;
using System.IO;               // ✅ Required for Path.GetFileName
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using FlaUI.Core;
using FlaUI.UIA3;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
//using static TiaPortalAutomation;
using Application = System.Windows.Forms.Application;
using Button = System.Windows.Forms.Button;
using NetworkInterface = System.Net.NetworkInformation.NetworkInterface;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Text;
namespace ProtectViaUI
{
    public static class BlockProtectionHelper
    {
        // ------------------------------
        // ENTRY POINT (Console App)
        // ------------------------------
        // Hide & Minimize API
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_MINIMIZE = 2;

        public static string UserPassword = "12345";
        private static string _applicationKey;
        private static string DatFile = @"C:\DownloadAddin\Debug\Data.dat";
        private static string _validationKey;

        private static bool? _dialogResult;
        private static bool _canCloseDialog;

        private static bool _isRegister;

        public static string ApplicationKey = "";

        /// <summary>
        /// Selected version of the openness api
        /// </summary>
        public static string ValidationKey = "";
        public class KeyData
        {
            public string ApplicationKey { get; set; } = "";
            public string ValidationKey { get; set; } = "";
            public DateTime RegisterDate { get; set; } = DateTime.Now;
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
        public static void ReadData()
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
        public class LicenseForm : Form
        {
            public string EnteredValidationKey = "";

            private static System.Windows.Forms.TextBox txtAppKey;
            private static System.Windows.Forms.TextBox txtValidation;
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
                System.Windows.Forms.Label lblAppKey = new System.Windows.Forms.Label()
                {
                    Text = "Application Key",
                    Left = 20,
                    Top = 30,
                    Width = 120,
                    Font = new Font("Segoe UI", 10)
                };

                txtAppKey = new System.Windows.Forms.TextBox()
                {
                    Left = 150,
                    Top = 25,
                    Width = 300,
                    ReadOnly = true,
                    Text = applicationKey
                };

                // Validation Key Label
                System.Windows.Forms.Label lblValidation = new System.Windows.Forms.Label()
                {
                    Text = "Validation Key",
                    Left = 20,
                    Top = 80,
                    Width = 120,
                    Font = new Font("Segoe UI", 10)
                };

                txtValidation = new System.Windows.Forms.TextBox()
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
        private static void GetApplicationKey(string fullDeviseName)
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
        [STAThread]
        static void Main(string[] args)
        {
            ShowWindow(Process.GetCurrentProcess().MainWindowHandle, SW_HIDE);
            //try
            //{

            //    Application.EnableVisualStyles();
            //    Application.SetCompatibleTextRenderingDefault(false);
            //    ReadData();

            //    // If no application key exists → generate
            //    if (string.IsNullOrEmpty(ApplicationKey))
            //    {
            //        string deviceName = Environment.MachineName + ".PC";
            //        GetApplicationKey(deviceName);
            //    }

            //    // If no validation key OR wrong validation key → show license form
            //    if (string.IsNullOrEmpty(ValidationKey) || !VerifyKey())
            //    {
            //        LicenseForm lf = new LicenseForm(ApplicationKey);

            //        if (lf.ShowDialog() == DialogResult.OK)
            //        {
            //            ValidationKey = lf.EnteredValidationKey;

            //            if (!VerifyKey())
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
            //    return;
            //}
           
            try
            {
                if (args.Length < 2)
                {
                    MessageBox.Show("Invalid arguments received by EXE.");
                    return;
                }

                string action =args[0];
                string projectPath = args[1];

                // Extract file name only → example: Project2.ap17
                string projectFileName = Path.GetFileName(projectPath);

                // --------------------------------------------
                // 1) FIND TIA PROCESS BY PROJECT FILE NAME
                // --------------------------------------------
                var tiaProcess = Process.GetProcessesByName("Siemens.Automation.Portal")
                    .FirstOrDefault(p => p.MainWindowTitle.Contains(projectFileName));

                if (tiaProcess == null)
                {
                    MessageBox.Show(
                        "Could not find TIA Portal window.\n" +
                        "Searching for: " + projectFileName
                    );
                    return;
                }

                // Minimize TIA before UI Automation
                ShowWindow(tiaProcess.MainWindowHandle, SW_MINIMIZE);
                Thread.Sleep(800);

                using (var app = FlaUI.Core.Application.Attach(tiaProcess))
                //using (var automation = new UIA3Automation())
                //{
                //    var mainWindow = app.GetMainWindow(automation);

                //    if (mainWindow == null)
                //    {
                //        MessageBox.Show("Main TIA window not found.");
                //        return;
                //    }

                //    // Run protection logic
                //    BlockProtectionHelper.ApplyProtection(mainWindow, automation);
                //}

                using (var automation = new UIA3Automation())
                {
                    var mainWindow = app.GetMainWindow(automation);

                    if (args[0] == "LOCK")
                        ApplyProtection(mainWindow, automation);

                    else if (args[0] == "UNLOCK")
                        UnprotectBlock(mainWindow, automation);

                    else if (args[0] == "PROTECT_EXISTING")
                        ProtectExisting(mainWindow, automation);
                }
                //ShowWindow(tiaProcess.MainWindowHandle, 9); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:\n" + ex.Message);
            }
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

        //static void Main(string[] args)
        //{
        //    // Hide this EXE window completely
        //    var exeHandle = Process.GetCurrentProcess().MainWindowHandle;
        //    ShowWindow(exeHandle, SW_HIDE);
        //    // Hide this EXE

        //    //Application.EnableVisualStyles();
        //    //Application.SetCompatibleTextRenderingDefault(false);

        //    Application.EnableVisualStyles();
        //    Application.SetCompatibleTextRenderingDefault(false);

        //    try
        //    {
        //        // Attach to TIA Portal process
        //        var tiaProcess = Process.GetProcessesByName("Siemens.Automation.Portal")
        //                                .FirstOrDefault(p => p.MainWindowTitle.Contains("Project2"));

        //        if (tiaProcess == null)
        //        {
        //            MessageBox.Show("TIA Portal process not found.");
        //            return;
        //        }
        //        // Minimize TIA Portal BEFORE automation runs

        //        ShowWindow(tiaProcess.MainWindowHandle, SW_MINIMIZE);
        //        Thread.Sleep(800);
        //        using (var app = FlaUI.Core.Application.Attach(tiaProcess))
        //        using (var automation = new UIA3Automation())
        //        {
        //            var mainWindow = app.GetMainWindow(automation);

        //            if (mainWindow == null)
        //            {
        //                MessageBox.Show("Main TIA window not found.");
        //                return;
        //            }

        //            // Run your protection logic
        //            BlockProtectionHelper.ApplyProtection(mainWindow, automation);
        //        }

        //        MessageBox.Show("Protection applied successfully!");
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error:\n" + ex.Message);
        //    }
        //}


        //static void Main(string[] args)
        //{
        //    // 1. Attach to TIA Portal process
        //    var tiaProcess = Process.GetProcessesByName("Siemens.Automation.Portal")
        //                            .FirstOrDefault(p => p.MainWindowTitle.Contains("Project2"));


        //    if (tiaProcess == null)
        //    {
        //        Console.WriteLine("TIA Portal not found.");
        //        return;
        //    }

        //    // 2. Attach FlaUI to TIA Portal window
        //    using (var app = FlaUI.Core.Application.Attach(tiaProcess))
        //    using (var automation = new UIA3Automation())
        //    {
        //        var mainWindow = app.GetMainWindow(automation);

        //        if (mainWindow == null)
        //        {
        //            Console.WriteLine("Main TIA window not found.");
        //            return;
        //        }

        //        // 3. CALL ApplyProtection CORRECTLY
        //        ApplyProtection(mainWindow, automation);

        //        Console.WriteLine("Protection applied successfully!");
        //    }
        //}
        public static void ApplyProtection(Window mainWindow, UIA3Automation automation)
        {
            try
            {
                mainWindow.Focus();
                SendKeys.SendWait("%{ENTER}");

                var desktop = automation.GetDesktop();

                desktop.FindFirstDescendant(cf =>
                    cf.ByName("Protection")
                      .And(cf.ByControlType(ControlType.Text)))
                ?.Click();

                desktop.FindFirstDescendant(cf =>
                    cf.ByName("buttonWriteProtection")
                      .And(cf.ByControlType(ControlType.Button)))
                ?.AsButton().Click();

                EnterPassword(automation, UserPassword);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" +
                 "LOCK is not applied due to the above error." + "\n" +
                 "Please check the Protect_Existing option.");
            }

        }

        // =====================================================
        // UNLOCK BLOCK (Remove protection)
        // =====================================================
        public static void UnprotectBlock(Window mainWindow, UIA3Automation automation)
        {
            try
            {

                mainWindow.Focus();
                SendKeys.SendWait("%{ENTER}");

                var desktop = automation.GetDesktop();

                desktop.FindFirstDescendant(cf =>
                    cf.ByName("Protection")
                      .And(cf.ByControlType(ControlType.Text)))
                ?.Click();

                desktop.FindFirstDescendant(cf =>
                    cf.ByName("checkBoxWriteProtection")
                      .And(cf.ByControlType(ControlType.CheckBox)))
                ?.AsCheckBox().Click();

                var pwdBox = Retry.WhileNull(
                    () => desktop.FindFirstDescendant(cf =>
                        cf.ByControlType(ControlType.Edit)
                          .And(cf.ByName("AccessPasswordText"))),
                    TimeSpan.FromSeconds(3)
                ).Result?.AsTextBox();

                pwdBox.Enter(UserPassword);

                desktop.FindFirstDescendant(cf =>
                    cf.ByName("m_OKButton.Text")
                      .And(cf.ByControlType(ControlType.Button)))
                ?.AsButton().Click();

                desktop.FindFirstDescendant(cf =>
                    cf.ByName("m_OKButton.Text")
                      .And(cf.ByControlType(ControlType.Button)))
                ?.AsButton().Click();

                MessageBox.Show("Block Unlock successful!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Block Is not Lock");
            }
        }

        // =====================================================
        // ENTER PASSWORD POPUP
        // =====================================================
        private static void EnterPassword(UIA3Automation automation, string pwd)
        {
            var desktop = automation.GetDesktop();

            var newBox = Retry.WhileNull(
                () => desktop.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.Edit)
                      .And(cf.ByName("NewPasswordText"))),
                TimeSpan.FromSeconds(2)
            ).Result?.AsTextBox();

            var confBox = Retry.WhileNull(
                () => desktop.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.Edit)
                      .And(cf.ByName("ConfirmPasswordText"))),
                TimeSpan.FromSeconds(2)
            ).Result?.AsTextBox();

            newBox.Enter(pwd);
            confBox.Enter(pwd);

            desktop.FindFirstDescendant(cf =>
                cf.ByName("m_OKButton.Text")
                  .And(cf.ByControlType(ControlType.Button)))
            ?.AsButton().Click();

            desktop.FindFirstDescendant(cf =>
                cf.ByName("checkBoxWriteProtection")
                  .And(cf.ByControlType(ControlType.CheckBox)))
            ?.Click();

            // Popup #2 (Access password)
            var accessBox = Retry.WhileNull(
                () => desktop.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.Edit)
                      .And(cf.ByName("AccessPasswordText"))),
                TimeSpan.FromSeconds(3)
            ).Result?.AsTextBox();

            accessBox.Enter(pwd);

            desktop.FindFirstDescendant(cf =>
                cf.ByName("m_OKButton.Text")
                  .And(cf.ByControlType(ControlType.Button)))
            ?.AsButton().Click();

            desktop.FindFirstDescendant(cf =>
                cf.ByName("m_OKButton.Text")
                  .And(cf.ByControlType(ControlType.Button)))
            ?.AsButton().Click();

            MessageBox.Show("Protection applied successfully!");
        }
        public static void ProtectExisting(Window mainWindow, UIA3Automation automation)
        {
            try
            {
                const string password = "12345";

                mainWindow.Focus();
                SendKeys.SendWait("%{ENTER}");

                var desktop = automation.GetDesktop();

                var tab = desktop.FindFirstDescendant(cf =>
                    cf.ByName("Protection").And(cf.ByControlType(ControlType.Text)));
                tab?.Click();

                // Checkbox enable
                var checkBox = desktop.FindFirstDescendant(cf =>
                    cf.ByName("checkBoxWriteProtection")
                      .And(cf.ByControlType(ControlType.CheckBox)))?.AsCheckBox();

                checkBox?.Click();

                // Password box
                var pwdBox = Retry.WhileNull(
                    () => automation.GetDesktop().FindFirstDescendant(cf =>
                        cf.ByControlType(ControlType.Edit).And(cf.ByName("AccessPasswordText"))),
                    TimeSpan.FromSeconds(5)).Result?.AsTextBox();

                pwdBox.Enter(password);

                // OK #1
                var ok1 = desktop.FindFirstDescendant(cf =>
                    cf.ByName("m_OKButton.Text").And(cf.ByControlType(ControlType.Button)));
                ok1?.Click();

                // OK #2
                var ok2 = Retry.WhileNull(
                    () => desktop.FindFirstDescendant(cf =>
                        cf.ByName("m_OKButton.Text").And(cf.ByControlType(ControlType.Button))),
                    TimeSpan.FromSeconds(3)).Result?.AsButton();
                ok2?.Click();

                MessageBox.Show("Existing block protection applied.");
            }
            catch(Exception ex)
            {
                MessageBox.Show("Block Is not Locked Previously"+"\n"+"Check with LOCK Option");
            }
        }

    }
}





////////////// Final Working Code 
////////////using FlaUI.Core.Tools;
////////////using FlaUI.UIA3;
////////////using System;
////////////using System.Runtime.InteropServices;
////////////using System.Windows.Forms;
////////////using FlaUI.Core;
////////////using FlaUI.Core.AutomationElements;
////////////using FlaUI.Core.Definitions;
////////////using FlaUI.Core.Input;
////////////using FlaUI.Core.Tools;
////////////using FlaUI.UIA3;
////////////using System;
////////////using System.Diagnostics;
////////////using System.Linq;
////////////using System.Threading;
////////////using System.Windows.Forms;
////////////using System.Threading;
////////////using System.Windows.Forms;
////////////using Application = System.Windows.Forms.Application;
////////////public static class WindowHider
////////////{
////////////    [DllImport("user32.dll")]
////////////    private static extern bool SetWindowPos(
////////////        IntPtr hWnd, IntPtr hWndInsertAfter,
////////////        int X, int Y, int cx, int cy, uint uFlags);

////////////    private const uint SWP_NOSIZE = 0x0001;
////////////    private const uint SWP_SHOWWINDOW = 0x0040;

////////////    // Move TIA Portal window off-screen (fully hidden)
////////////    public static void HideWindow(IntPtr handle)
////////////    {
////////////        SetWindowPos(handle, IntPtr.Zero,
////////////            -4000, -4000, 0, 0,
////////////            SWP_NOSIZE | SWP_SHOWWINDOW);
////////////    }

////////////    // Restore TIA Portal window position
////////////    public static void ShowWindow(IntPtr handle)
////////////    {
////////////        SetWindowPos(handle, IntPtr.Zero,
////////////            100, 100, 1200, 800,
////////////            SWP_SHOWWINDOW);
////////////    }
////////////}


////////////namespace ProtectViaUI
////////////{
////////////    public static class BlockProtectionHelper
////////////    {
////////////        public static string UserPassword = "123456";

////////////        // =====================================================
////////////        // ENTRY POINT
////////////        // =====================================================
////////////        [STAThread]
////////////        static void Main(string[] args)
////////////        {
////////////            Application.EnableVisualStyles();
////////////            Application.SetCompatibleTextRenderingDefault(false);

////////////            try
////////////            {
////////////                string action = args[0];
////////////                string projectTitle = args[1];

////////////                var tiaProcess = Process.GetProcessesByName("Siemens.Automation.Portal")
////////////                                       .FirstOrDefault(p => p.MainWindowTitle.Contains(projectTitle));

////////////                if (tiaProcess == null)
////////////                {
////////////                    MessageBox.Show("TIA Portal process not found.");
////////////                    return;
////////////                }

////////////                using (var app = FlaUI.Core.Application.Attach(tiaProcess))
////////////                using (var automation = new UIA3Automation())
////////////                {
////////////                    var mainWindow = app.GetMainWindow(automation);

////////////                    //if (args[0] == "LOCK")
////////////                    //    ProtectExisting(mainWindow, automation);

////////////                    //else if (args[0] == "UNLOCK")
////////////                    //    UnprotectBlock(mainWindow, automation);

////////////                    //else if (args[0] == "PROTECT_EXISTING")
////////////                    //    ProtectExisting(mainWindow, automation);
////////////                    if (args[0] == "ProtectBlock")
////////////                        BlockProtectionHelper.ApplyProtection(mainWindow, automation);
////////////                    else if (args[0] == "Unprotect")
////////////                        BlockProtectionHelper.UnprotectBlock(mainWindow, automation);
////////////                    else if (args[0] == "ProtectExisting")
////////////                        BlockProtectionHelper.ProtectExisting(mainWindow, automation);
////////////                }

////////////            }
////////////            catch (Exception ex)
////////////            {
////////////                MessageBox.Show("Error:\n" + ex.Message);
////////////            }
////////////        }

////////////        // =====================================================
////////////        // LOCK BLOCK (Apply Protection)
////////////        // =====================================================
////////////        public static void ApplyProtection(Window mainWindow, UIA3Automation automation)
////////////        {
////////////            mainWindow.Focus();
////////////            SendKeys.SendWait("%{ENTER}");

////////////            var desktop = automation.GetDesktop();

////////////            desktop.FindFirstDescendant(cf =>
////////////                cf.ByName("Protection")
////////////                  .And(cf.ByControlType(ControlType.Text)))
////////////            ?.Click();

////////////            desktop.FindFirstDescendant(cf =>
////////////                cf.ByName("buttonWriteProtection")
////////////                  .And(cf.ByControlType(ControlType.Button)))
////////////            ?.AsButton().Click();

////////////            EnterPassword(automation, UserPassword);
////////////        }

////////////        // =====================================================
////////////        // UNLOCK BLOCK (Remove protection)
////////////        // =====================================================
////////////        public static void UnprotectBlock(Window mainWindow, UIA3Automation automation)
////////////        {
////////////            mainWindow.Focus();
////////////            SendKeys.SendWait("%{ENTER}");

////////////            var desktop = automation.GetDesktop();

////////////            desktop.FindFirstDescendant(cf =>
////////////                cf.ByName("Protection")
////////////                  .And(cf.ByControlType(ControlType.Text)))
////////////            ?.Click();

////////////            desktop.FindFirstDescendant(cf =>
////////////                cf.ByName("checkBoxWriteProtection")
////////////                  .And(cf.ByControlType(ControlType.CheckBox)))
////////////            ?.AsCheckBox().Click();

////////////            var pwdBox = Retry.WhileNull(
////////////                () => desktop.FindFirstDescendant(cf =>
////////////                    cf.ByControlType(ControlType.Edit)
////////////                      .And(cf.ByName("AccessPasswordText"))),
////////////                TimeSpan.FromSeconds(3)
////////////            ).Result?.AsTextBox();

////////////            pwdBox.Enter(UserPassword);

////////////            desktop.FindFirstDescendant(cf =>
////////////                cf.ByName("m_OKButton.Text")
////////////                  .And(cf.ByControlType(ControlType.Button)))
////////////            ?.AsButton().Click();

////////////            desktop.FindFirstDescendant(cf =>
////////////                cf.ByName("m_OKButton.Text")
////////////                  .And(cf.ByControlType(ControlType.Button)))
////////////            ?.AsButton().Click();

////////////            //MessageBox.Show("Unlock successful!");
////////////        }

////////////        // =====================================================
////////////        // ENTER PASSWORD POPUP
////////////        // =====================================================
////////////        private static void EnterPassword(UIA3Automation automation, string pwd)
////////////        {
////////////            var desktop = automation.GetDesktop();

////////////            var newBox = Retry.WhileNull(
////////////                () => desktop.FindFirstDescendant(cf =>
////////////                    cf.ByControlType(ControlType.Edit)
////////////                      .And(cf.ByName("NewPasswordText"))),
////////////                TimeSpan.FromSeconds(2)
////////////            ).Result?.AsTextBox();

////////////            var confBox = Retry.WhileNull(
////////////                () => desktop.FindFirstDescendant(cf =>
////////////                    cf.ByControlType(ControlType.Edit)
////////////                      .And(cf.ByName("ConfirmPasswordText"))),
////////////                TimeSpan.FromSeconds(2)
////////////            ).Result?.AsTextBox();

////////////            newBox.Enter(pwd);
////////////            confBox.Enter(pwd);

////////////            desktop.FindFirstDescendant(cf =>
////////////                cf.ByName("m_OKButton.Text")
////////////                  .And(cf.ByControlType(ControlType.Button)))
////////////            ?.AsButton().Click();

////////////            desktop.FindFirstDescendant(cf =>
////////////                cf.ByName("checkBoxWriteProtection")
////////////                  .And(cf.ByControlType(ControlType.CheckBox)))
////////////            ?.Click();

////////////            // Popup #2 (Access password)
////////////            var accessBox = Retry.WhileNull(
////////////                () => desktop.FindFirstDescendant(cf =>
////////////                    cf.ByControlType(ControlType.Edit)
////////////                      .And(cf.ByName("AccessPasswordText"))),
////////////                TimeSpan.FromSeconds(3)
////////////            ).Result?.AsTextBox();

////////////            accessBox.Enter(pwd);

////////////            desktop.FindFirstDescendant(cf =>
////////////                cf.ByName("m_OKButton.Text")
////////////                  .And(cf.ByControlType(ControlType.Button)))
////////////            ?.AsButton().Click();

////////////            desktop.FindFirstDescendant(cf =>
////////////                cf.ByName("m_OKButton.Text")
////////////                  .And(cf.ByControlType(ControlType.Button)))
////////////            ?.AsButton().Click();

////////////            //MessageBox.Show("Protection applied successfully!");
////////////        }
////////////        public static void ProtectExisting(Window mainWindow, UIA3Automation automation)
////////////        {
////////////            const string password = "123456";

////////////            mainWindow.Focus();
////////////            SendKeys.SendWait("%{ENTER}");

////////////            var desktop = automation.GetDesktop();

////////////            var tab = desktop.FindFirstDescendant(cf =>
////////////                cf.ByName("Protection").And(cf.ByControlType(ControlType.Text)));
////////////            tab?.Click();

////////////            // Checkbox enable
////////////            var checkBox = desktop.FindFirstDescendant(cf =>
////////////                cf.ByName("checkBoxWriteProtection")
////////////                  .And(cf.ByControlType(ControlType.CheckBox)))?.AsCheckBox();

////////////            checkBox?.Click();

////////////            // Password box
////////////            var pwdBox = Retry.WhileNull(
////////////                () => automation.GetDesktop().FindFirstDescendant(cf =>
////////////                    cf.ByControlType(ControlType.Edit).And(cf.ByName("AccessPasswordText"))),
////////////                TimeSpan.FromSeconds(5)).Result?.AsTextBox();

////////////            pwdBox.Enter(password);

////////////            // OK #1
////////////            var ok1 = desktop.FindFirstDescendant(cf =>
////////////                cf.ByName("m_OKButton.Text").And(cf.ByControlType(ControlType.Button)));
////////////            ok1?.Click();

////////////            // OK #2
////////////            var ok2 = Retry.WhileNull(
////////////                () => desktop.FindFirstDescendant(cf =>
////////////                    cf.ByName("m_OKButton.Text").And(cf.ByControlType(ControlType.Button))),
////////////                TimeSpan.FromSeconds(3)).Result?.AsButton();
////////////            ok2?.Click();

////////////            MessageBox.Show("Existing block protection applied.");
////////////        }

////////////    }
////////////}

























//using FlaUI.Core;
//using FlaUI.Core.AutomationElements;
//using FlaUI.Core.Definitions;
//using FlaUI.Core.Input;
//using FlaUI.Core.Tools;
//using FlaUI.UIA3;
//using System;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading;
//using System.Windows.Forms;
//using Application = System.Windows.Forms.Application;

//namespace ProtectViaUI
//{
//    public static class BlockProtectionHelper
//    {
//        // GLOBAL PASSWORD (entered only once)
//        public static string UserPassword = "";



//        //////[STAThread]
//        //////static void Main(string[] args)
//        //////{
//        //////    Application.EnableVisualStyles();
//        //////    Application.SetCompatibleTextRenderingDefault(false);

//        //////    try
//        //////    {
//        //////        // -----------------------------------------
//        //////        // SAFE DEFAULT VALUES FOR VISUAL STUDIO RUN
//        //////        // -----------------------------------------
//        //////        string action = "LOCK";   // default
//        //////        string projectTitle = "Project2";  // part of window title, not path

//        //////        // -------------------------------------------------------
//        //////        // IF REAL ARGUMENTS CAME FROM ADD-IN, THEN USE THEM
//        //////        // args[0] = LOCK or UNLOCK
//        //////        // args[1] = project window title (NOT full path)
//        //////        // -------------------------------------------------------
//        //////        if (args.Length >= 1) action = args[0];
//        //////        if (args.Length >= 2) projectTitle = args[1];

//        //////        // -------------------------------------------------------
//        //////        // TIA PROCESS MATCHES BY WINDOW TITLE, NOT FILE PATH
//        //////        // -------------------------------------------------------
//        //////        var tiaProcess = Process.GetProcessesByName("Siemens.Automation.Portal")
//        //////                                 .FirstOrDefault(p => p.MainWindowTitle.Contains(projectTitle));

//        //////        if (tiaProcess == null)
//        //////        {
//        //////            MessageBox.Show("TIA Portal process not found.");
//        //////            return;
//        //////        }

//        //////        using (var app = FlaUI.Core.Application.Attach(tiaProcess))
//        //////        using (var automation = new UIA3Automation())
//        //////        {
//        //////            var mainWindow = app.GetMainWindow(automation);

//        //////            if (mainWindow == null)
//        //////            {
//        //////                MessageBox.Show("Main TIA window not found.");
//        //////                return;
//        //////            }

//        //////            // -----------------------------------------
//        //////            // ONLY TWO OPTIONS : LOCK / UNLOCK
//        //////            // -----------------------------------------
//        //////            if (action == "LOCK")
//        //////            {
//        //////                if (IsBlockAlreadyProtected(mainWindow, automation))
//        //////                {
//        //////                    BlockProtectionHelper.ApplyProtection(mainWindow, automation);
//        //////                }
//        //////                else
//        //////                {
//        //////                    BlockProtectionHelper.protectExisting(mainWindow, automation);
//        //////                }
//        //////            }
//        //////            else if (action == "UNLOCK")
//        //////            {
//        //////                BlockProtectionHelper.UnprotectBlock(mainWindow, automation);
//        //////            }
//        //////        }
//        //////    }
//        //////    catch (Exception ex)
//        //////    {
//        //////        MessageBox.Show("Error:\n" + ex.Message);
//        //////    }
//        //////}

//        // ======================================================
//        // MAIN ENTRY – PASSWORD ASKED ONCE HERE
//        // ======================================================
//        [STAThread]
//        static void Main(string[] args)
//        {
//            Application.EnableVisualStyles();
//            Application.SetCompatibleTextRenderingDefault(false);

//            try
//            {
//                // Ask password ONLY ONCE at start
//                //if (string.IsNullOrWhiteSpace(UserPassword))
//                //{
//                //    UserPassword = Microsoft.VisualBasic.Interaction.InputBox(
//                //        "Enter block password (used for LOCK and UNLOCK):",
//                //        "TIA Password",
//                //        ""
//                //    );

//                //    if (string.IsNullOrWhiteSpace(UserPassword))
//                //    {
//                //        MessageBox.Show("Password required to continue.");
//                //        return;
//                //    }
//                //}

//                // args[0] = LOCK/UNLOCK
//                // args[1] = TIA project window title
//                var tiaProcess = Process.GetProcessesByName("Siemens.Automation.Portal")
//                                        .FirstOrDefault(p => p.MainWindowTitle.Contains(args[1]));

//                if (tiaProcess == null)
//                {
//                    MessageBox.Show("TIA Portal process not found.");
//                    return;
//                }

//                using (var app = FlaUI.Core.Application.Attach(tiaProcess))
//                using (var automation = new UIA3Automation())
//                {
//                    var mainWindow = app.GetMainWindow(automation);
//                    if (mainWindow == null)
//                    {
//                        MessageBox.Show("Main TIA window not found.");
//                        return;
//                    }
//                    if (args[0] == "LOCK")
//                        //{
//                        //    if (IsBlockAlreadyProtected(mainWindow, automation))
//                        //    {
//                        // Block already protected → apply EXISTING logic
//                        BlockProtectionHelper.protectExisting(mainWindow, automation);
//                    //}
//                    //else
//                    //{
//                    //    // Block not protected → normal first-time protection
//                    //    BlockProtectionHelper.protectExisting(mainWindow, automation);
//                    //}
//                    //}
//                    else if (args[0] == "UNLOCK")
//                    {
//                        BlockProtectionHelper.ApplyProtection(mainWindow, automation);
//                    }
//                    //if (args[0] == "LOCK")
//                    //{
//                    //    if (IsBlockAlreadyProtected(mainWindow, automation))
//                    //        ApplyProtection(mainWindow, automation);
//                    //    else
//                    //        protectExisting(mainWindow, automation);
//                    //}
//                    //else if (args[0] == "UNLOCK")
//                    //{
//                    //    UnprotectBlock(mainWindow, automation);
//                    //}
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Error:\n" + ex.Message);
//            }
//        }

//        // ====================================================================
//        // CHECK IF BLOCK ALREADY PROTECTED
//        // ====================================================================
//        //////    private static bool IsBlockAlreadyProtected(Window mainWindow, UIA3Automation automation)
//        //////    {
//        //////        mainWindow.Focus();
//        //////        SendKeys.SendWait("%{ENTER}");

//        //////        var desktop = automation.GetDesktop();

//        //////        var protectionTab = desktop.FindFirstDescendant(cf =>
//        //////            cf.ByName("Protection").And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text)));
//        //////        protectionTab?.Click();

//        //////        //var writeProtectionCheckbox = desktop.FindAllDescendants(cf =>
//        //////        //    cf.ByName("checkBoxWriteProtection").And(cf.ByControlType(ControlType.CheckBox)))
//        //////        //    .Select(e => e.AsCheckBox())
//        //////        //    .FirstOrDefault();
//        //////        var writeProtectionCheckbox = desktop.FindAllDescendants(cf =>
//        //////    cf.ByName("checkBoxWriteProtection")
//        //////      .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.CheckBox)))
//        //////.Select(e => e.AsCheckBox())
//        //////.FirstOrDefault(cb =>
//        //////    cb?.Patterns?.LegacyIAccessible?.Pattern?.DefaultAction == "Check");


//        //////        if (writeProtectionCheckbox == null)
//        //////            return true; // assume not protected

//        //////        return writeProtectionCheckbox.IsChecked ?? false;
//        //////    }
//        private static bool? _legacyCheckCache = null;

//        private static bool IsBlockAlreadyProtected(Window mainWindow, UIA3Automation automation)
//        {
//            mainWindow.Focus();
//            SendKeys.SendWait("%{ENTER}");

//            var protectionTab = mainWindow.FindFirstDescendant(cf =>
//                cf.ByName("Protection").And(cf.ByControlType(ControlType.Text)));
//            protectionTab?.Click();

//            var element = mainWindow.FindFirstDescendant(cf =>
//                cf.ByName("checkBoxWriteProtection")
//                  .And(cf.ByControlType(ControlType.CheckBox)));

//            if (element == null)
//                return true;

//            var cb = element.AsCheckBox();

//            // ⭐ FAST PATH: Use cached LegacyAccessible result
//            if (_legacyCheckCache.HasValue)
//            {
//                // If cached result says this is not the right checkbox
//                if (_legacyCheckCache.Value == false)
//                    return true;

//                // Cached result says it's valid checkbox → return IsChecked
//                return cb.IsChecked ?? false;
//            }

//            // ⭐ SLOW PATH: FIRST evaluation, with 1-second timeout
//            string defaultAction = null;

//            var task = System.Threading.Tasks.Task.Run(() =>
//            {
//                try
//                {
//                    defaultAction = cb.Patterns.LegacyIAccessible.Pattern.DefaultAction;
//                }
//                catch { }
//            });

//            if (!task.Wait(TimeSpan.FromSeconds(1)))
//            {
//                // Timed-out → assume "not valid checkbox" AND cache that
//                _legacyCheckCache = false;
//                return true;
//            }

//            // Store whether the checkbox is the right one
//            bool isValidCheckbox = (defaultAction == "Check");

//            // ⭐ CACHE RESULT (never evaluated again)
//            _legacyCheckCache = isValidCheckbox;

//            if (!isValidCheckbox)
//                return true;

//            // Now return actual protection state
//            return cb.IsChecked ?? false;
//        }

//        ////////private static bool IsBlockAlreadyProtected(Window mainWindow, UIA3Automation automation)
//        ////////{
//        ////////    mainWindow.Focus();
//        ////////    SendKeys.SendWait("%{ENTER}");

//        ////////    // 🔥 Search only inside the TIA window (SUPER FAST)
//        ////////    var protectionTab = mainWindow.FindFirstDescendant(cf =>
//        ////////        cf.ByName("Protection")
//        ////////          .And(cf.ByControlType(ControlType.Text)));
//        ////////    protectionTab?.Click();

//        ////////    // 🔥 Only search inside this window, not entire desktop
//        ////////    //var writeProtectionCheckbox = mainWindow.FindAllDescendants(cf =>
//        ////////    //        cf.ByName("checkBoxWriteProtection")
//        ////////    //          .And(cf.ByControlType(ControlType.CheckBox)))
//        ////////    //    .Select(e => e.AsCheckBox())
//        ////////    //    .FirstOrDefault(cb =>
//        ////////    //        cb?.Patterns?.LegacyIAccessible?.Pattern?.DefaultAction == "Check");
//        ////////    // Step 1: Find checkbox quickly
//        ////////    var element = mainWindow.FindFirstDescendant(cf =>
//        ////////        cf.ByName("checkBoxWriteProtection")
//        ////////          .And(cf.ByControlType(ControlType.CheckBox)));

//        ////////    if (element == null)
//        ////////        return true;

//        ////////    // Step 2: Apply your LegacyIAccessible filter on ONE element
//        ////////    var cb = element.AsCheckBox();

//        ////////    if (cb?.Patterns?.LegacyIAccessible?.Pattern?.DefaultAction != "Check")
//        ////////        return true;

//        ////////    return cb.IsChecked ?? false;


//        ////////    //if (writeProtectionCheckbox == null)
//        ////////    //    return true;

//        ////////    //return writeProtectionCheckbox.IsChecked ?? false;
//        ////////}

//        // ====================================================================
//        // FIRST-TIME PROTECTION
//        // ====================================================================
//        public static void protectExisting(Window mainWindow, UIA3Automation automation)
//        {
//            const string password = "Priti@123456";
//            mainWindow.Focus();
//            SendKeys.SendWait("%{ENTER}");

//            var desktop = automation.GetDesktop();

//            var propsWindow = desktop.FindFirstDescendant(cf =>
//                cf.ByName("Protection").And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text)));
//            propsWindow?.Click();


//            var propsWindow2 = desktop.FindFirstDescendant(cf =>
//                cf.ByName("checkBoxWriteProtection")
//                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.CheckBox)));
//            propsWindow2.Click();

//            var pwdBox = Retry.WhileNull(
//                () => automation.GetDesktop().FindFirstDescendant(cf =>
//                    cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit)
//                      .And(cf.ByName("AccessPasswordText"))),
//                TimeSpan.FromSeconds(5)
//            ).Result?.AsTextBox();

//            pwdBox.Enter(password); // Auto fill

//            var okBtn = desktop.FindFirstDescendant(cf =>
//                cf.ByName("m_OKButton.Text").And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));
//            okBtn.Click();

//            //var tooltip = desktop.FindFirstDescendant(cf =>
//            //    cf.ByName("AccessPasswordTextRolloutTip")
//            //      .And(cf.ByControlType(ControlType.ToolTip)));

//            //if (tooltip != null)
//            //{
//            //    MessageBox.Show("Password incorrect.");
//            //    return;
//            //}

//            var okBtn4 = desktop.FindFirstDescendant(cf =>
//                cf.ByName("m_OKButton.Text").And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//            okBtn4.Click();
//            MessageBox.Show("Protection applied successfully!");
//        }

//        // ====================================================================
//        // UNLOCK BLOCK (USE SAVED PASSWORD)
//        // ====================================================================
//        public static void UnprotectBlock(Window mainWindow, UIA3Automation automation)
//        {
//            const string password = "123456";
//            mainWindow.Focus();
//            SendKeys.SendWait("%{ENTER}");

//            var desktop = automation.GetDesktop();

//            var propsWindow = desktop.FindFirstDescendant(cf =>
//                cf.ByName("Protection").And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text)));
//            propsWindow?.Click();

//            var checkBox = desktop.FindFirstDescendant(cf =>
//                cf.ByName("checkBoxWriteProtection")
//                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.CheckBox)))
//                ?.AsCheckBox();
//            checkBox?.Click();

//            var pwdBox8 = Retry.WhileNull(
//                () => automation.GetDesktop().FindFirstDescendant(cf =>
//                    cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit).And(cf.ByName("AccessPasswordText"))),
//                TimeSpan.FromSeconds(5)
//            ).Result?.AsTextBox();

//            pwdBox8.Enter(password);

//            // FIRST OK CLICK
//            var okBtn = desktop.FindFirstDescendant(cf =>
//                cf.ByName("m_OKButton.Text").And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));
//            okBtn.Click();

//            //// WAIT FOR TOOLTIP TO DISAPPEAR
//            //Retry.While(
//            //    () => desktop.FindFirstDescendant(cf =>
//            //        cf.ByName("AccessPasswordTextRolloutTip")
//            //          .And(cf.ByControlType(ControlType.ToolTip))) != null,
//            //    result => result == true,
//            //    TimeSpan.FromSeconds(1)
//            //);

//            // ⭐⭐⭐ SECRET FIX – WAIT UNTIL UI STOPS CHANGING ⭐⭐⭐
//            //AutomationElement secondOk = null;

//            //Retry.While(
//            //    () =>
//            //    {
//            //        secondOk = desktop.FindFirstDescendant(cf =>
//            //            cf.ByName("m_OKButton.Text")
//            //              .And(cf.ByControlType(ControlType.Button)));

//            //        // KEEP WAITING WHILE BUTTON DOES NOT EXIST OR IS DISABLED
//            //        return secondOk == null || !secondOk.AsButton().IsEnabled;
//            //    },
//            //    result => result == true,
//            //    TimeSpan.FromSeconds(1)
//            //);

//            //// INSTANT CLICK (UI is stable now)
//            //secondOk?.AsButton().Click();
//            var okBtn4 = desktop.FindFirstDescendant(cf =>
//            cf.ByName("m_OKButton.Text")
//              .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//            //if (switchLangMenu != null)
//            //{
//            //propsWindow.Focus();
//            okBtn4.Click();
//            MessageBox.Show("Unlock successfully!");
//        }


//        public static void ApplyProtection(Window mainWindow, UIA3Automation automation)
//        {
//            const string password = "123456";

//            //// Step 1 – ALT+ENTER
//            //Keyboard.Press(VirtualKeyShort.ALT);
//            //Keyboard.Press(VirtualKeyShort.ENTER);
//            //Keyboard.Release(VirtualKeyShort.ENTER);
//            //Keyboard.Release(VirtualKeyShort.ALT);
//            //Thread.Sleep(1000);
//            // =============================
//            // FIXED: Open block Properties
//            // =============================
//            //Form coverForm = new Form();
//            //coverForm.FormBorderStyle = FormBorderStyle.None;
//            //coverForm.BackColor = System.Drawing.Color.Black;
//            //coverForm.Opacity = 0.01;  // Almost invisible
//            //coverForm.WindowState = FormWindowState.Maximized;
//            //coverForm.TopMost = true;
//            //coverForm.Show();

//            mainWindow.Focus();
//            TimeSpan.FromSeconds(100);

//            // Click selected block in tree
//            //var selectedBlock = mainWindow.FindFirstDescendant(cf =>
//            //    cf.ByControlType(ControlType.DataItem))
//            //    ?.AsAutomationElement();

//            //selectedBlock?.Click();
//            //Thread.Sleep(500);

//            // Send ALT + ENTER (Open Properties)
//            SendKeys.SendWait("%{ENTER}");
//            TimeSpan.FromSeconds(50);

//            // Step 2 – Find Properties window
//            //var propsWindow = Retry.WhileNull(
//            //    () => automation.GetDesktop().FindFirstDescendant(cf =>
//            //        cf.ByControlType(ControlType.Window)
//            //          .And(cf.ByName("General"))),
//            //    TimeSpan.FromSeconds(5)
//            //).Result?.AsWindow();

//            //if (propsWindow == null)
//            //    throw new Exception("Properties popup not found.");
//            var desktop = automation.GetDesktop();
//            var propsWindow = desktop.FindFirstDescendant(cf =>
//                cf.ByName("Protection")
//                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text)));

//            //if (switchLangMenu != null)
//            //{
//            propsWindow.Focus();
//            propsWindow.Click();
//            //    //Console.WriteLine("Focused 'Switch programming language' menu.");
//            //    // Step 3 – Protection tab
//            //    propsWindow.FindFirstDescendant(cf => cf.ByName("Protection"))
//            //           ?.AsTabItem()?.Select();
//            //Thread.Sleep(700);

//            // Step 4 – Define password
//            var propsWindow1 = desktop.FindFirstDescendant(cf =>
//               cf.ByName("buttonWriteProtection")
//                 .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));
//            //propsWindow.FindFirstDescendant(cf => cf.ByName("button WriteProtection"))
//            //           .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text)));
//            propsWindow1.Focus();
//            propsWindow1.Click();

//            // Step 5 – Enter password popup #1
//            EnterPassword(automation, password);

//            // Step 6 – Tick write protection checkbox
//            //propsWindow.FindFirstDescendant(cf => cf.ByAutomationId("checkBoxWriteProtection"))
//            //           ?.AsCheckBox()?.Click();
//            //coverForm.Close();
//        }

//        // ------------------------------
//        // ENTER PASSWORD IN POPUP
//        // ------------------------------
//        private static void EnterPassword(UIA3Automation automation, string pwd)
//        {
//            // 1. Find the password popup window (ANY window containing password fields)
//            var popup = Retry.WhileNull(
//                () => automation.GetDesktop().FindFirstDescendant(cf =>
//                    cf.ByControlType(ControlType.TitleBar)
//                      .And(cf.ByName("TitleBar")
//                           )),
//                TimeSpan.FromSeconds(5)
//            ).Result?.AsWindow();

//            if (popup == null)
//                throw new Exception("Password popup not found.");

//            Thread.Sleep(100);

//            // 2. Find New Password textbox using AutomationId
//            //var newPwdBox = popup.WhileNull(
//            //    () => automation.GetDesktop().FindFirstDescendant(cf =>
//            //        cf.ByControlType(ControlType.Edit)
//            //          .And(cf.ByName("NewPasswordText")));
//            //var newPwdBox = Retry.WhileNull(
//            //     () => automation.GetDesktop().FindFirstDescendant(cf =>
//            //         cf.ByControlType(ControlType.TitleBar)
//            //           .And(cf.ByName("NewPasswordText")
//            //                )));

//            //var confirmPwdBox = Retry.WhileNull(
//            //    () => automation.GetDesktop().FindFirstDescendant(cf =>
//            //        cf.ByControlType(ControlType.TitleBar)
//            //          .And(cf.ByName("ConfirmPasswordText")
//            //               )));

//            //// 3. Find Confirm Password textbox using AutomationId
//            ////var confirmPwdBox = popup.FindFirstDescendant(cf =>
//            ////    cf.ByAutomationId("ConfirmPasswordText"))?.AsTextBox();

//            //if (newPwdBox == null || confirmPwdBox == null)
//            //    throw new Exception("Password textboxes not found in popup.");

//            //// 4. Enter password in both fields
//            //newPwdBox.Enter(pwd);
//            //Thread.Sleep(200);

//            //confirmPwdBox.Enter(pwd);
//            //Thread.Sleep(200);
//            // 1. Locate NEW PASSWORD textbox
//            var newPwdBox = Retry.WhileNull(
//                () => automation.GetDesktop().FindFirstDescendant(cf =>
//                    cf.ByControlType(ControlType.Edit)
//                      .And(cf.ByName("NewPasswordText"))),


//                TimeSpan.FromSeconds(1)
//            ).Result?.AsTextBox();
//            //newPwdBox.Focus();
//            //newPwdBox.Click();

//            // 2. Locate CONFIRM PASSWORD textbox
//            var confirmPwdBox = Retry.WhileNull(
//                () => automation.GetDesktop().FindFirstDescendant(cf =>
//                    cf.ByControlType(ControlType.Edit)
//                      .And(cf.ByName("ConfirmPasswordText"))),
//                TimeSpan.FromSeconds(5)
//            ).Result?.AsTextBox();

//            if (newPwdBox == null || confirmPwdBox == null)
//                throw new Exception("Password textboxes not found in popup.");

//            // 3. Enter password in both fields
//            newPwdBox.Enter(pwd);
//            //Thread.Sleep(50);

//            confirmPwdBox.Enter(pwd);
//            //Thread.Sleep(50);

//            // 5. Click OK button
//            var desktop = automation.GetDesktop();
//            var okBtn = desktop.FindFirstDescendant(cf =>
//                cf.ByName("m_OKButton.Text")
//                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//            //if (switchLangMenu != null)
//            //{
//            //propsWindow.Focus();
//            okBtn.Click();
//            //var okBtn = popup.FindFirstDescendant(cf =>
//            //    cf.ByControlType(ControlType.Button)
//            //      .And(cf.ByName("OK")))?.AsButton();

//            //okBtn?.Invoke();

//            //Thread.Sleep(600);
//            var propsWindow2 = desktop.FindFirstDescendant(cf =>
//            cf.ByName("checkBoxWriteProtection")
//              .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.CheckBox)));
//            propsWindow2.Click();
//            //Thread.Sleep(700);

//            // Step 7 – Enter password popup #2
//            var confirmPwdBox3 = Retry.WhileNull(
//              () => automation.GetDesktop().FindFirstDescendant(cf =>
//                  cf.ByControlType(ControlType.Edit)
//                    .And(cf.ByName("AccessPasswordText"))),

//              TimeSpan.FromSeconds(5)
//          ).Result?.AsTextBox();

//            //EnterPassword(automation, password);
//            confirmPwdBox3.Enter(pwd);
//            //Thread.Sleep(50);
//            var okBtn2 = desktop.FindFirstDescendant(cf =>
//               cf.ByName("m_OKButton.Text")
//                 .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//            //if (switchLangMenu != null)
//            //{
//            //propsWindow.Focus();
//            okBtn2.Click();

//            var okBtn4 = desktop.FindFirstDescendant(cf =>
//             cf.ByName("m_OKButton.Text")
//               .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//            //if (switchLangMenu != null)
//            //{
//            //propsWindow.Focus();
//            okBtn4.Click();
//            // Step 8 – Final OK
//            //propsWindow.FindFirstDescendant(cf => cf.ByName("OK"))
//            //           ?.AsButton()?.Invoke();

//            Thread.Sleep(50);
//        }
//    }

//}




// ====================================================================
// APPLY PROTECTION (IF ALREADY PROTECTED ONCE)
// ====================================================================
//////////////        public static void ApplyProtection(Window mainWindow, UIA3Automation automation)
//////////////        {
//////////////            string password = UserPassword;
//////////////            var desktop = automation.GetDesktop();

//////////////            var propsWindow1 = desktop.FindFirstDescendant(cf =>
//////////////                cf.ByName("buttonWriteProtection")
//////////////                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));
//////////////            propsWindow1.Click();

//////////////            EnterPassword(automation, password);
//////////////        }

//////////////        // ====================================================================
//////////////        // ENTER PASSWORD POPUP – AUTO-FILL PASSWORD
//////////////        // ====================================================================
//////////////        private static void EnterPassword(UIA3Automation automation, string pwd)
//////////////        {
//////////////            var popup = Retry.WhileNull(
//////////////                () => automation.GetDesktop().FindFirstDescendant(cf =>
//////////////                    cf.ByControlType(FlaUI.Core.Definitions.ControlType.TitleBar)
//////////////                      .And(cf.ByName("TitleBar"))),
//////////////                TimeSpan.FromSeconds(5)
//////////////            ).Result?.AsWindow();

//////////////            var newPwdBox = Retry.WhileNull(
//////////////                () => automation.GetDesktop().FindFirstDescendant(cf =>
//////////////                    cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit)
//////////////                      .And(cf.ByName("NewPasswordText"))),
//////////////                TimeSpan.FromSeconds(5)
//////////////            ).Result?.AsTextBox();

//////////////            var confirmPwdBox = Retry.WhileNull(
//////////////                () => automation.GetDesktop().FindFirstDescendant(cf =>
//////////////                    cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit)
//////////////                      .And(cf.ByName("ConfirmPasswordText"))),
//////////////                TimeSpan.FromSeconds(5)
//////////////            ).Result?.AsTextBox();

//////////////            newPwdBox.Enter(UserPassword);
//////////////            confirmPwdBox.Enter(UserPassword);

//////////////            var desktop = automation.GetDesktop();
//////////////            var okBtn = desktop.FindFirstDescendant(cf =>
//////////////                cf.ByName("m_OKButton.Text")
//////////////                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//////////////            okBtn.Click();



//////////////            var propsWindow2 = desktop.FindFirstDescendant(cf =>
//////////////cf.ByName("checkBoxWriteProtection")
//////////////  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.CheckBox)));
//////////////            propsWindow2.Click();


//////////////            //            var newPwdBox = Retry.WhileNull(
//////////////            //    () => automation.GetDesktop().FindFirstDescendant(cf =>
//////////////            //        cf.ByControlType(ControlType.Edit)
//////////////            //          .And(cf.ByName("AccessPasswordText"))),
//////////////            //    TimeSpan.FromSeconds(5)
//////////////            //).Result?.AsTextBox();

//////////////            //            newPwdBox.Enter(password);


//////////////            var pwdBox = Retry.WhileNull(
//////////////         () => automation.GetDesktop().FindFirstDescendant(cf =>
//////////////             cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit)
//////////////               .And(cf.ByName("AccessPasswordText"))),
//////////////         TimeSpan.FromSeconds(5)
//////////////     ).Result?.AsTextBox();
//////////////            pwdBox.Enter(UserPassword);

//////////////            var okBtn3 = desktop.FindFirstDescendant(cf =>
//////////////                cf.ByName("m_OKButton.Text")
//////////////                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//////////////            //if (switchLangMenu != null)
//////////////            //{
//////////////            //propsWindow.Focus();
//////////////            okBtn3.Click();

//////////////            //var tooltip = desktop.FindFirstDescendant(cf =>
//////////////            //    cf.ByName("AccessPasswordTextRolloutTip")
//////////////            //    .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.ToolTip)));


//////////////        //    var okBtn2 = desktop.FindFirstDescendant(cf =>
//////////////        //        cf.ByName("m_OKButton.Text")
//////////////        //          .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//////////////        //    //if (switchLangMenu != null)
//////////////        //    //{
//////////////        //    //propsWindow.Focus();
//////////////        //    okBtn2.Click();
//////////////        //}



//////////////        var okBtn2 = desktop.FindFirstDescendant(cf =>
//////////////                cf.ByName("m_OKButton.Text")
//////////////                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//////////////            //if (switchLangMenu != null)
//////////////            //{
//////////////            //propsWindow.Focus();
//////////////            okBtn2.Click();
//////////////            MessageBox.Show("Lock successfully!");
//////////////        }


//////////////    }
//////////////    }
























//using FlaUI.Core;
//using FlaUI.Core.AutomationElements;
//using FlaUI.Core.Definitions;
//using FlaUI.Core.Input;
//using FlaUI.Core.Tools;
//using FlaUI.Core.WindowsAPI;
//using FlaUI.UIA3;
//using System;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading;
//using System.Windows.Forms;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
////using static TiaPortalAutomation;
//using Application = System.Windows.Forms.Application;
//using Button = System.Windows.Forms.Button;
//using NetworkInterface = System.Net.NetworkInformation.NetworkInterface;
//namespace ProtectViaUI
//{
//    public static class BlockProtectionHelper
//    {
//        // this is for tia addin 
//        [STAThread]
//        static void Main(string[] args)
//        {
//            Application.EnableVisualStyles();
//            Application.SetCompatibleTextRenderingDefault(false);

//            try
//            {
//                // args[0] = LOCK or UNLOCK
//                // args[1] = project window title

//                var tiaProcess = Process.GetProcessesByName("Siemens.Automation.Portal")
//                                        .FirstOrDefault(p => p.MainWindowTitle.Contains(args[1]));

//                if (tiaProcess == null)
//                {
//                    MessageBox.Show("TIA Portal process not found.");
//                    return;
//                }

//                using (var app = FlaUI.Core.Application.Attach(tiaProcess))
//                using (var automation = new UIA3Automation())
//                {
//                    var mainWindow = app.GetMainWindow(automation);

//                    if (mainWindow == null)
//                    {
//                        MessageBox.Show("Main TIA window not found.");
//                        return;
//                    }

//                    // ------------ ONLY TWO OPTIONS ------------
//                    if (args[0] == "LOCK")
//                    {
//                        if (IsBlockAlreadyProtected(mainWindow, automation))
//                        {
//                            // Block already protected → apply EXISTING logic
//                            BlockProtectionHelper.ApplyProtection(mainWindow, automation);
//                        }
//                        else
//                        {
//                            // Block not protected → normal first-time protection
//                            BlockProtectionHelper.protectExisting(mainWindow, automation);
//                        }
//                    }
//                    else if (args[0] == "UNLOCK")
//                    {
//                        BlockProtectionHelper.UnprotectBlock(mainWindow, automation);
//                    }
//                    // -------------------------------------------
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Error:\n" + ex.Message);
//            }
//        }
//        private static bool IsBlockAlreadyProtected(Window mainWindow, UIA3Automation automation)
//        {
//            mainWindow.Focus();
//            //Thread.Sleep(100);

//            // Open block Properties (ALT + ENTER)
//            SendKeys.SendWait("%{ENTER}");
//            //Thread.Sleep(200);

//            var desktop = automation.GetDesktop();

//            // Open the Protection section
//            var protectionTab = desktop.FindFirstDescendant(cf =>
//                cf.ByName("Protection")
//                  .And(cf.ByControlType(ControlType.Text)));
//            protectionTab?.Focus();
//            protectionTab?.Click();
//            //Thread.Sleep(100);

//            // -----------------------------------------------------------
//            // Locate Write Protection checkbox
//            // With extra condition:
//            // LegacyIAccessible.DefaultAction == "Check"
//            // -----------------------------------------------------------
//            var writeProtectionCheckbox = desktop.FindAllDescendants(cf =>
//                    cf.ByName("checkBoxWriteProtection")
//                      .And(cf.ByControlType(ControlType.CheckBox)))
//                .Select(e => e.AsCheckBox())
//                .FirstOrDefault(cb =>
//                    cb?.Patterns?.LegacyIAccessible?.Pattern?.DefaultAction == "Check");

//            if (writeProtectionCheckbox == null)
//            {
//                // Checkbox missing → assume NOT protected
//                return true;
//            }

//            // TRUE = already protected
//            return writeProtectionCheckbox.IsChecked ?? false;
//        }
//        public static void protectExisting(Window mainWindow, UIA3Automation automation)
//        {
//            //const string password = "12345";

//            //mainWindow.Focus();
//            //Thread.Sleep(500);

//            //SendKeys.SendWait("%{ENTER}");
//            //Thread.Sleep(1500);

//            var desktop = automation.GetDesktop();
//            //var propsWindow = desktop.FindFirstDescendant(cf =>
//            //    cf.ByName("Protection")
//            //      .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text)));

//            //propsWindow.Focus();
//            //propsWindow.Click();

//            var propsWindow2 = desktop.FindFirstDescendant(cf =>
//           cf.ByName("checkBoxWriteProtection")
//             .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.CheckBox)));
//            propsWindow2.Click();

//            Thread.Sleep(100);
//            var pwdBox = Retry.WhileNull(
//         () => automation.GetDesktop().FindFirstDescendant(cf =>
//             cf.ByControlType(ControlType.Edit)
//               .And(cf.ByName("AccessPasswordText"))),
//         TimeSpan.FromSeconds(5)
//     ).Result?.AsTextBox();


//            //newPwdBox.Enter(password);
//            Thread.Sleep(5000);

//            // 5. Click OK button
//            //var desktop = automation.GetDesktop();
//            var okBtn = desktop.FindFirstDescendant(cf =>
//                cf.ByName("m_OKButton.Text")
//                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//            //if (switchLangMenu != null)
//            //{
//            //propsWindow.Focus();
//            okBtn.Click();

//            AutomationElement tooltip;

//            do
//            {
//                tooltip = desktop.FindFirstDescendant(cf =>
//                    cf.ByName("AccessPasswordTextRolloutTip")
//                      .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.ToolTip)));


//                if (tooltip != null)
//                {
//                    //MessageBox.Show("Out0");
//                    // Password wrong
//                    MessageBox.Show("Password is wrong. Please enter correct password.");

//                    break;

//                    Thread.Sleep(4000); // Wait for user to type again

//                    // Click OK again after user enters new password
//                    okBtn?.Click();
//                    Thread.Sleep(300);
//                }
//                else
//                {
//                    //MessageBox.Show("Out1");
//                    break;
//                }

//            } while (tooltip != null);

//            if (tooltip != null)
//                return;


//            var okBtn4 = desktop.FindFirstDescendant(cf =>
//             cf.ByName("m_OKButton.Text")
//               .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//            //if (switchLangMenu != null)
//            //{
//            //propsWindow.Focus();
//            okBtn4.Click();
//            // Step 8 – Final OK
//            //propsWindow.FindFirstDescendant(cf => cf.ByName("OK"))
//            //           ?.AsButton()?.Invoke();

//            //Thread.Sleep(50);
//            MessageBox.Show("Protection applied successfully!");
//        }
//        public static void UnprotectBlock(Window mainWindow, UIA3Automation automation)
//        {
//            mainWindow.Focus();
//            SendKeys.SendWait("%{ENTER}");

//            var desktop = automation.GetDesktop();

//            var propsWindow = desktop.FindFirstDescendant(cf =>
//                cf.ByName("Protection")
//                  .And(cf.ByControlType(ControlType.Text)));

//            propsWindow?.Click();

//            // disable protection checkbox
//            var checkBox = desktop.FindFirstDescendant(cf =>
//                cf.ByName("checkBoxWriteProtection")
//                  .And(cf.ByControlType(ControlType.CheckBox)))
//                ?.AsCheckBox();

//            checkBox?.Click();

//            // Wait for password box to appear
//            var pwdBox = Retry.WhileNull(
//                () => automation.GetDesktop().FindFirstDescendant(cf =>
//                    cf.ByControlType(ControlType.Edit)
//                      .And(cf.ByName("AccessPasswordText"))),
//                TimeSpan.FromSeconds(5)
//            ).Result?.AsTextBox();

//            Thread.Sleep(2000);

//            // OK button
//            var okBtn = desktop.FindFirstDescendant(cf =>
//                cf.ByName("m_OKButton.Text")
//                  .And(cf.ByControlType(ControlType.Button)));

//            // ---------------------------------------------
//            // PASSWORD VALIDATION LOOP
//            // ---------------------------------------------
//            while (true)
//            {
//                okBtn?.Click();
//                //Thread.Sleep(100);

//                // Check tooltip → wrong password
//                var tooltip = desktop.FindFirstDescendant(cf =>
//                    cf.ByName("AccessPasswordTextRolloutTip")
//                      .And(cf.ByControlType(ControlType.ToolTip)));

//                if (tooltip != null)
//                {
//                    // Wrong password
//                    MessageBox.Show("Incorrect password. Please enter correct password");

//                    // Wait for user to fix password
//                    bool tooltipStillVisible = true;

//                    while (tooltipStillVisible)
//                    {
//                        //Thread.Sleep(500);

//                        tooltip = desktop.FindFirstDescendant(cf =>
//                            cf.ByName("AccessPasswordTextRolloutTip")
//                              .And(cf.ByControlType(ControlType.ToolTip)));

//                        if (tooltip == null)
//                            tooltipStillVisible = false;
//                    }

//                    // User corrected password → retry loop from beginning
//                    continue;
//                }

//                // NO tooltip = password accepted
//                break;
//            }

//            // Final OK click
//            var okBtn2 = desktop.FindFirstDescendant(cf =>
//                cf.ByName("m_OKButton.Text")
//                  .And(cf.ByControlType(ControlType.Button)));

//            okBtn2?.Click();
//            MessageBox.Show("Unlock successfully!");
//        }
//        public static void ApplyProtection(Window mainWindow, UIA3Automation automation)
//        {
//            const string password = "12345";
//            var desktop = automation.GetDesktop();
//            var propsWindow1 = desktop.FindFirstDescendant(cf =>
//            cf.ByName("buttonWriteProtection")
//              .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));
//            propsWindow1.Focus();
//            propsWindow1.Click();

//            // Step 5 – Enter password popup #1
//            EnterPassword(automation, password);

//            // Step 6 – Tick write protection checkbox
//            //propsWindow.FindFirstDescendant(cf => cf.ByAutomationId("checkBoxWriteProtection"))
//            //           ?.AsCheckBox()?.Click();

//        }
//        private static void EnterPassword(UIA3Automation automation, string pwd)
//        {
//            // 1. Find the password popup window (ANY window containing password fields)
//            var popup = Retry.WhileNull(
//                () => automation.GetDesktop().FindFirstDescendant(cf =>
//                    cf.ByControlType(ControlType.TitleBar)
//                      .And(cf.ByName("TitleBar")
//                           )),
//                TimeSpan.FromSeconds(5)
//            ).Result?.AsWindow();

//            if (popup == null)
//                throw new Exception("Password popup not found.");

//            Thread.Sleep(300);
//            var newpwdBox = Retry.WhileNull(
//      () => automation.GetDesktop().FindFirstDescendant(cf =>
//          cf.ByControlType(ControlType.Edit)
//            .And(cf.ByName("NewPasswordText"))),
//      TimeSpan.FromSeconds(5)
//  ).Result?.AsTextBox();

//            var confpwdBox = Retry.WhileNull(
//         () => automation.GetDesktop().FindFirstDescendant(cf =>
//             cf.ByControlType(ControlType.Edit)
//               .And(cf.ByName("ConfirmPasswordText"))),
//         TimeSpan.FromSeconds(5)
//     ).Result?.AsTextBox();
//            Thread.Sleep(6000);


//            // 5. Click OK button
//            var desktop = automation.GetDesktop();
//            var okBtn = desktop.FindFirstDescendant(cf =>
//                cf.ByName("m_OKButton.Text")
//                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//            //if (switchLangMenu != null)
//            //{
//            //propsWindow.Focus();
//            okBtn.Click();
//            var propsWindow2 = desktop.FindFirstDescendant(cf =>
//        cf.ByName("checkBoxWriteProtection")
//          .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.CheckBox)));
//            propsWindow2.Click();
//            //Thread.Sleep(700);

//            // Step 7 – Enter password popup #2
//            //  var confirmPwdBox3 = Retry.WhileNull(
//            //    () => automation.GetDesktop().FindFirstDescendant(cf =>
//            //        cf.ByControlType(ControlType.Edit)
//            //          .And(cf.ByName("AccessPasswordText"))),

//            //    TimeSpan.FromSeconds(5)
//            //).Result?.AsTextBox();

//            //  //EnterPassword(automation, password);
//            //  confirmPwdBox3.Enter(pwd);
//            //Thread.Sleep(2000);
//            var confirmPwdBox3 = Retry.WhileNull(
//         () => automation.GetDesktop().FindFirstDescendant(cf =>
//             cf.ByControlType(ControlType.Edit)
//               .And(cf.ByName("AccessPasswordText"))),
//         TimeSpan.FromSeconds(5)
//     ).Result?.AsTextBox();

//            Thread.Sleep(6000);
//            var okBtn2 = desktop.FindFirstDescendant(cf =>
//               cf.ByName("m_OKButton.Text")
//                 .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//            //if (switchLangMenu != null)
//            //{
//            //propsWindow.Focus();
//            okBtn2.Click();

//            var okBtn4 = desktop.FindFirstDescendant(cf =>
//             cf.ByName("m_OKButton.Text")
//               .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//            //if (switchLangMenu != null)
//            //{
//            okBtn4.Focus();
//            okBtn4.Click();
//            // Step 8 – Final OK
//            //propsWindow.FindFirstDescendant(cf => cf.ByName("OK"))
//            //           ?.AsButton()?.Invoke();

//            //Thread.Sleep(50);
//            MessageBox.Show("Protection applied successfully!");
//        }
//    }

//}




























// ------------------------------
// ENTRY POINT (Console App)
// ------------------------------


/*[STAThread] */  // IMPORTANT for TIA Add-In launching EXE
                  //static void Main(string[] args)
                  //{
                  //    Application.EnableVisualStyles();
                  //    Application.SetCompatibleTextRenderingDefault(false);

//    try
//    {
//        MessageBox.Show(args[1]);
//        // Attach to TIA Portal process
//        var tiaProcess = Process.GetProcessesByName("Siemens.Automation.Portal")
//                                .FirstOrDefault(p => p.MainWindowTitle.Contains(args[1]));

//        if (tiaProcess == null)
//        {
//            MessageBox.Show("TIA Portal process not found.");
//            return;
//        }

//        using (var app = FlaUI.Core.Application.Attach(tiaProcess))
//        using (var automation = new UIA3Automation())
//        {
//            var mainWindow = app.GetMainWindow(automation);

//            if (mainWindow == null)
//            {
//                MessageBox.Show("Main TIA window not found.");
//                return;
//            }

//            if (args[0] == "LOCK")
//                BlockProtectionHelper.ApplyProtection(mainWindow, automation);
//            else if (args[0] == "UNLOCK")
//                BlockProtectionHelper.UnprotectBlock(mainWindow, automation);
//            else if (args[0] == "ProtectExisting")
//                BlockProtectionHelper.protectExisting(mainWindow, automation);
//        }

//        //MessageBox.Show("Protection applied successfully!");
//    }
//    catch (Exception ex)
//    {
//        MessageBox.Show("Error:\n" + ex.Message);
//    }
//}


////[STAThread]
////static void Main(string[] args)
////{
////    Application.EnableVisualStyles();
////    Application.SetCompatibleTextRenderingDefault(false);

////    try
////    {
////        // -----------------------------------------
////        // SAFE DEFAULT VALUES FOR VISUAL STUDIO RUN
////        // -----------------------------------------
////        string action = "LOCK";   // default
////        string projectTitle = "Project2";  // part of window title, not path

////        // -------------------------------------------------------
////        // IF REAL ARGUMENTS CAME FROM ADD-IN, THEN USE THEM
////        // args[0] = LOCK or UNLOCK
////        // args[1] = project window title (NOT full path)
////        // -------------------------------------------------------
////        if (args.Length >= 1) action = args[0];
////        if (args.Length >= 2) projectTitle = args[1];

////        // -------------------------------------------------------
////        // TIA PROCESS MATCHES BY WINDOW TITLE, NOT FILE PATH
////        // -------------------------------------------------------
////        var tiaProcess = Process.GetProcessesByName("Siemens.Automation.Portal")
////                                 .FirstOrDefault(p => p.MainWindowTitle.Contains(projectTitle));

////        if (tiaProcess == null)
////        {
////            MessageBox.Show("TIA Portal process not found.");
////            return;
////        }

////        using (var app = FlaUI.Core.Application.Attach(tiaProcess))
////        using (var automation = new UIA3Automation())
////        {
////            var mainWindow = app.GetMainWindow(automation);

////            if (mainWindow == null)
////            {
////                MessageBox.Show("Main TIA window not found.");
////                return;
////            }

////            // -----------------------------------------
////            // ONLY TWO OPTIONS : LOCK / UNLOCK
////            // -----------------------------------------
////            if (action == "LOCK")
////            {
////                if (IsBlockAlreadyProtected(mainWindow, automation))
////                {
////                    BlockProtectionHelper.ApplyProtection(mainWindow, automation);
////                }
////                else
////                {
////                    BlockProtectionHelper.protectExisting(mainWindow, automation);
////                }
////            }
////            else if (action == "UNLOCK")
////            {
////                BlockProtectionHelper.UnprotectBlock(mainWindow, automation);
////            }
////        }
////    }
////    catch (Exception ex)
////    {
////        MessageBox.Show("Error:\n" + ex.Message);
////    }
////}

//---------------------------------------------------
//CHECK IF BLOCK IS ALREADY PROTECTED
//---------------------------------------------------
//private static bool IsBlockAlreadyProtected(Window mainWindow, UIA3Automation automation)
//{
//    mainWindow.Focus();
//    Thread.Sleep(300);

//    // Open block Properties (ALT + ENTER)
//    SendKeys.SendWait("%{ENTER}");
//    Thread.Sleep(1200);

//    var desktop = automation.GetDesktop();



























//    // Open the Protection section
//    var protectionTab = desktop.FindFirstDescendant(cf =>
//        cf.ByName("Protection")
//          .And(cf.ByControlType(ControlType.Text)));

//    protectionTab?.Click();
//    Thread.Sleep(400);

//    // Locate the Write Protection checkbox
//    var writeProtectionCheckbox = desktop.FindFirstDescendant(cf =>
//        cf.ByName("checkBoxWriteProtection")
//          .And(cf.ByControlType(ControlType.CheckBox)))
//        ?.AsCheckBox();

//    if (writeProtectionCheckbox == null)
//    {
//        // Checkbox missing → cannot determine → assume not protected
//        return false;
//    }

//    // TRUE = Block already protected
//    return writeProtectionCheckbox.IsChecked ?? false;
//}


//private static bool IsBlockAlreadyProtected(Window mainWindow, UIA3Automation automation)
//{
//    mainWindow.Focus();
//    Thread.Sleep(300);

//    // Open block Properties (ALT + ENTER)
//    SendKeys.SendWait("%{ENTER}");
//    Thread.Sleep(1200);

//    var desktop = automation.GetDesktop();

//    // Open the Protection section
//    var protectionTab = desktop.FindFirstDescendant(cf =>
//        cf.ByName("Protection")
//          .And(cf.ByControlType(ControlType.Text)));

//    protectionTab?.Click();
//    Thread.Sleep(400);

//    // ---------------------------------------------------------
//    // NEW LOGIC:
//    // If "buttonWriteProtection" IS FOUND → NOT protected
//    // If NOT FOUND → ALREADY protected
//    // ---------------------------------------------------------

//    var writeProtectionButton = desktop.FindFirstDescendant(cf =>
//        cf.ByName("buttonWriteProtection")
//          .And(cf.ByControlType(ControlType.Button)));

//    if (writeProtectionButton != null)
//    {
//        // Button exists → block is NOT protected
//        return false;
//    }

//    // Button not found → block IS protected
//    return true;
//}




//public static void UnprotectBlock(Window mainWindow, UIA3Automation automation)
//{
//    mainWindow.Focus();
//    //Thread.Sleep(100);

//    SendKeys.SendWait("%{ENTER}");
//    //Thread.Sleep(100);

//    var desktop = automation.GetDesktop();

//    // Open protection window
//    var propsWindow = desktop.FindFirstDescendant(cf =>
//        cf.ByName("Protection")
//          .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text)));

//    propsWindow?.Click();

//    // Disable write protection
//    var checkBox = desktop.FindFirstDescendant(cf =>
//        cf.ByName("checkBoxWriteProtection")
//          .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.CheckBox)));

//    checkBox?.Click();

//    // Wait for user to type password manually


//    // Locate password box
//    var pwdBox = Retry.WhileNull(
//        () => automation.GetDesktop().FindFirstDescendant(cf =>
//            cf.ByControlType(ControlType.Edit)
//              .And(cf.ByName("AccessPasswordText"))),
//        TimeSpan.FromSeconds(5)
//    ).Result?.AsTextBox();
//    Thread.Sleep(3000);
//    //Thread.Sleep(100);

//    // First OK Button click
//    var okBtn = desktop.FindFirstDescendant(cf =>
//        cf.ByName("m_OKButton.Text")
//          .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//    okBtn?.Click();
//    //Thread.Sleep(100);

//    // ---------- Tooltip check loop ----------
//    AutomationElement tooltip;
//    do
//    {
//        tooltip = desktop.FindFirstDescendant(cf =>
//            cf.ByName("AccessPasswordTextRolloutTip")
//              .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.ToolTip)));

//        if (tooltip != null)
//        {
//            //MessageBox.Show("Out0");
//            // Password wrong
//            MessageBox.Show("Password is wrong. Please enter correct password.");

//            break;

//            Thread.Sleep(2000); // Wait for user to type again

//            // Click OK again after user enters new password
//            okBtn?.Click();
//            Thread.Sleep(300);
//        }
//        else
//        {
//            //MessageBox.Show("Out1");
//            break;
//        }

//    } while (tooltip != null);

//    if(tooltip != null)
//        return;
//    // -----------------------------------------
//    //MessageBox.Show("Out2");
//    // Tooltip gone → password correct → final OK click
//    var okBtn2 = desktop.FindFirstDescendant(cf =>
//        cf.ByName("m_OKButton.Text")
//          .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//    okBtn2?.Click();
//    MessageBox.Show("Unlock successfully!");
//}




//// Step 1 – ALT+ENTER
//Keyboard.Press(VirtualKeyShort.ALT);
//Keyboard.Press(VirtualKeyShort.ENTER);
//Keyboard.Release(VirtualKeyShort.ENTER);
//Keyboard.Release(VirtualKeyShort.ALT);
//Thread.Sleep(1000);
// =============================
// FIXED: Open block Properties
// =============================
//mainWindow.Focus();
//Thread.Sleep(500);

// Click selected block in tree
//var selectedBlock = mainWindow.FindFirstDescendant(cf =>
//    cf.ByControlType(ControlType.DataItem))
//    ?.AsAutomationElement();

//selectedBlock?.Click();
//Thread.Sleep(500);

// Send ALT + ENTER (Open Properties)
//SendKeys.SendWait("%{ENTER}");
//Thread.Sleep(1500);

// Step 2 – Find Properties window
//var propsWindow = Retry.WhileNull(
//    () => automation.GetDesktop().FindFirstDescendant(cf =>
//        cf.ByControlType(ControlType.Window)
//          .And(cf.ByName("General"))),
//    TimeSpan.FromSeconds(5)
//).Result?.AsWindow();

//if (propsWindow == null)
//    throw new Exception("Properties popup not found.");

//var propsWindow = desktop.FindFirstDescendant(cf =>
//    cf.ByName("Protection")
//      .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text)));

////if (switchLangMenu != null)
////{
//propsWindow.Focus();
//propsWindow.Click();
//    //Console.WriteLine("Focused 'Switch programming language' menu.");
//    // Step 3 – Protection tab
//    propsWindow.FindFirstDescendant(cf => cf.ByName("Protection"))
//           ?.AsTabItem()?.Select();
//Thread.Sleep(700);

// Step 4 – Define password

//propsWindow.FindFirstDescendant(cf => cf.ByName("button WriteProtection"))
//           .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text)));

// ------------------------------
// ENTER PASSWORD IN POPUP
// ------------------------------


// 2. Find New Password textbox using AutomationId
//var newPwdBox = popup.WhileNull(
//    () => automation.GetDesktop().FindFirstDescendant(cf =>
//        cf.ByControlType(ControlType.Edit)
//          .And(cf.ByName("NewPasswordText")));
//var newPwdBox = Retry.WhileNull(
//     () => automation.GetDesktop().FindFirstDescendant(cf =>
//         cf.ByControlType(ControlType.TitleBar)
//           .And(cf.ByName("NewPasswordText")
//                )));

//var confirmPwdBox = Retry.WhileNull(
//    () => automation.GetDesktop().FindFirstDescendant(cf =>
//        cf.ByControlType(ControlType.TitleBar)
//          .And(cf.ByName("ConfirmPasswordText")
//               )));

//// 3. Find Confirm Password textbox using AutomationId
////var confirmPwdBox = popup.FindFirstDescendant(cf =>
////    cf.ByAutomationId("ConfirmPasswordText"))?.AsTextBox();

//if (newPwdBox == null || confirmPwdBox == null)
//    throw new Exception("Password textboxes not found in popup.");

//// 4. Enter password in both fields
//newPwdBox.Enter(pwd);
//Thread.Sleep(200);

//confirmPwdBox.Enter(pwd);
//Thread.Sleep(200);
// 1. Locate NEW PASSWORD textbox
//var newPwdBox = Retry.WhileNull(
//    () => automation.GetDesktop().FindFirstDescendant(cf =>
//        cf.ByControlType(ControlType.Edit)
//          .And(cf.ByName("NewPasswordText"))),


//    TimeSpan.FromSeconds(5)
//).Result?.AsTextBox();

//Thread.Sleep(2000);


//newPwdBox.Focus();
//newPwdBox.Click();

// 2. Locate CONFIRM PASSWORD textbox
//var confirmPwdBox = Retry.WhileNull(
//    () => automation.GetDesktop().FindFirstDescendant(cf =>
//        cf.ByControlType(ControlType.Edit)
//          .And(cf.ByName("ConfirmPasswordText"))),
//    TimeSpan.FromSeconds(5)
//).Result?.AsTextBox();



//if (string.IsNullOrEmpty(newpwdBox.Text) || string.IsNullOrEmpty(confpwdBox.Text))
//    throw new Exception("Password textboxes not found in popup.");

// 3. Enter password in both fields
//newPwdBox.Enter(pwd);
//Thread.Sleep(50);

//confirmPwdBox.Enter(pwd);

//var okBtn = popup.FindFirstDescendant(cf =>
//    cf.ByControlType(ControlType.Button)
//      .And(cf.ByName("OK")))?.AsButton();

//okBtn?.Invoke();

//Thread.Sleep(600);



//static void Main(string[] args)
//{
//    // 1. Attach to TIA Portal process
//    var tiaProcess = Process.GetProcessesByName("Siemens.Automation.Portal")
//                            .FirstOrDefault(p => p.MainWindowTitle.Contains("Project2"));


//    if (tiaProcess == null)
//    {
//        Console.WriteLine("TIA Portal not found.");
//        return;
//    }

//    // 2. Attach FlaUI to TIA Portal window
//    using (var app = FlaUI.Core.Application.Attach(tiaProcess))
//    using (var automation = new UIA3Automation())
//    {
//        var mainWindow = app.GetMainWindow(automation);

//        if (mainWindow == null)
//        {
//            Console.WriteLine("Main TIA window not found.");
//            return;
//        }

//        // 3. CALL ApplyProtection CORRECTLY
//        ApplyProtection(mainWindow, automation);

//        Console.WriteLine("Protection applied successfully!");
//    }
//}

//        public static void UnprotectBlock(Window mainWindow, UIA3Automation automation)
//        {
//            const string password = "12345";

//            mainWindow.Focus();
//            Thread.Sleep(500);

//            SendKeys.SendWait("%{ENTER}");
//            Thread.Sleep(1500);

//            var desktop = automation.GetDesktop();
//            var propsWindow = desktop.FindFirstDescendant(cf =>
//                cf.ByName("Protection")
//                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text)));

//            propsWindow.Focus();
//            propsWindow.Click();


//            var propsWindow2 = desktop.FindFirstDescendant(cf =>
//cf.ByName("checkBoxWriteProtection")
//  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.CheckBox)));
//            propsWindow2.Click();


//            //            var newPwdBox = Retry.WhileNull(
//            //    () => automation.GetDesktop().FindFirstDescendant(cf =>
//            //        cf.ByControlType(ControlType.Edit)
//            //          .And(cf.ByName("AccessPasswordText"))),
//            //    TimeSpan.FromSeconds(5)
//            //).Result?.AsTextBox();

//            //            newPwdBox.Enter(password);

//            Thread.Sleep(5000);
//            var pwdBox = Retry.WhileNull(
//         () => automation.GetDesktop().FindFirstDescendant(cf =>
//             cf.ByControlType(ControlType.Edit)
//               .And(cf.ByName("AccessPasswordText"))),
//         TimeSpan.FromSeconds(5)
//     ).Result?.AsTextBox();
//            Thread.Sleep(50);

//            var okBtn = desktop.FindFirstDescendant(cf =>
//                cf.ByName("m_OKButton.Text")
//                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//            //if (switchLangMenu != null)
//            //{
//            //propsWindow.Focus();
//            okBtn.Click();

//            var tooltip = desktop.FindFirstDescendant(cf =>
//                cf.ByName("AccessPasswordTextRolloutTip")
//                .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.ToolTip)));


//            var okBtn2 = desktop.FindFirstDescendant(cf =>
//                cf.ByName("m_OKButton.Text")
//                  .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)));

//            //if (switchLangMenu != null)
//            //{
//            //propsWindow.Focus();
//            okBtn2.Click();
//        }


// ------------------------------
// APPLY BLOCK PROTECTION SEQUENCE
// ------------------------------

//using Siemens.Engineering;
//using Siemens.Engineering.Compiler;
//using Siemens.Engineering.HW.Features;
//using Siemens.Engineering.SW;
//using Siemens.Engineering.SW.Blocks;
//using System;
//using System.Linq;
////using Siemens.Engineering.SW.Blocks.PlcBlockSecurity;

//class BlockProtection
//{
//    static TiaPortal tia;
//    static Project project;

//    static void Main()
//    {
//        string projectPath = @"C:\Projects\My1500Project.ap17";
//        string blockName = "FB1";
//        string password = "12345";

//        OpenProjectHidden(projectPath);

//        LockBlock(blockName, password);
//        //UnlockBlock(blockName, password);

//        project.Save();
//        tia.Dispose();
//    }

//    static void OpenProjectHidden(string projectPath)
//    {
//        tia = new TiaPortal(TiaPortalMode.WithoutUserInterface);
//        project = tia.Projects.Open(projectPath);
//    }

//    static PlcSoftware GetPlcSoftware()
//    {
//        var device = project.Devices
//            .FirstOrDefault(d => d.DeviceItems.Any(i => i is SoftwareContainer));

//        var swContainer = device?.DeviceItems
//            .OfType<SoftwareContainer>()
//            .FirstOrDefault();

//        return swContainer?.Software as PlcSoftware;
//    }

//    static PlcBlock GetBlock(string blockName)
//    {
//        var plcSoftware = GetPlcSoftware();

//        return plcSoftware?.BlockGroup.Blocks
//            .FirstOrDefault(b => b.Name == blockName);
//    }

//    static void LockBlock(string blockName, string password)
//    {
//        var block = GetBlock(blockName);
//        if (block == null) throw new Exception("Block not found.");

//        block.Protection.WriteAccess = AccessLevel.WriteProtected;
//        block.Protection.SetPassword(password);

//        Console.WriteLine($"{blockName} locked successfully.");
//    }

//    static void UnlockBlock(string blockName, string password)
//    {
//        var block = GetBlock(blockName);
//        if (block == null) throw new Exception("Block not found.");

//        block.Protection.ResetPassword(password);
//        block.Protection.WriteAccess = AccessLevel.WriteEnabled;

//        Console.WriteLine($"{blockName} unlocked successfully.");
//    }
//}
