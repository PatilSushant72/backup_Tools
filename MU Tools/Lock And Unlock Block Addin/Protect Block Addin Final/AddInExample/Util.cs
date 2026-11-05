////using Newtonsoft.Json;
//using Siemens.Engineering.SW.Blocks;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Reflection;
//using System.Text.Json;
//using System.Windows.Forms;

//namespace AddInExample
//{
//    internal static class Util
//    {
//        public static HashSet<string> LoadPermissions()
//        {
//            try
//            {
//                // Get the folder where the current assembly is located
//                string executingFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

//                // Build the full path to user_permissions.json in the same folder
//                string filePath = Path.Combine(executingFolder, "user_permissions.json");

//                if (!File.Exists(filePath))
//                    return new HashSet<string>();

//                // Read and deserialize
//                string json = File.ReadAllText(filePath);
//                var data = JsonSerializer.Deserialize<PermissionFile>(json);

//                return new HashSet<string>(data?.Permissions ?? new List<string>());
//            }
//            catch (Exception ex)
//            {
//                // Optional: log the error to diagnose problems
//                Console.WriteLine($"Error loading permissions: {ex.Message}");
//                return new HashSet<string>();
//            }
//        }


//        public static void RunExternalProcess(string submenu, string projectPath)
//        {
//            string exePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ConsoleLauncher.exe");

//            if (!File.Exists(exePath))
//            {
//                MessageBox.Show($"Required executable not found:\n{exePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                return;
//            }

//            ProcessStartInfo startInfo = new ProcessStartInfo()
//            {
//                FileName = exePath,
//                Arguments = $"\"{submenu}\" \"{projectPath}\"",
//                UseShellExecute = false,
//                CreateNoWindow = true
//            };

//            Process cliProcess = new Process
//            {
//                EnableRaisingEvents = true,
//                StartInfo = startInfo
//            };

//            cliProcess.Start();
//        }
//        //internal static void RunExternalProcess(string handoverData, string projectPath)
//        //{
//        //    string exePath = @"C:\Generate Block\Debug\ConsoleApp6.exe";

//        //    ProcessStartInfo startInfo = new ProcessStartInfo()
//        //    {
//        //        FileName = exePath,
//        //        Arguments = $"\"{handoverData}\" \"{projectPath}\"",  // ✅ Correct arguments format
//        //        UseShellExecute = false,
//        //        CreateNoWindow = true  // ✅ Hides console window
//        //    };

//        //    Process cliProcess = new Process
//        //    {
//        //        EnableRaisingEvents = true,
//        //        StartInfo = startInfo
//        //    };

//        //    cliProcess.Start();
//        //}

//        internal static void ShowBlockInfo(PlcBlock block)
//        {
//            string title = "AddIn - Programblock information";
//            string content = $"Name: {block.Name}\nNumber: {block.Number}\nIs compiled: {block.IsConsistent}";
//            using (Form owner = GetForegroundWindow())
//            {
//                MessageBox.Show(owner, content, title);
//            }
//        }

//        private static Form GetForegroundWindow(string title = null)
//        {
//            Form form = new Form { Opacity = 0, ShowIcon = false, Text = title };
//            form.Show();
//            form.TopMost = true;
//            form.Activate();
//            form.TopMost = false;
//            return form;
//        }

//        private class PermissionFile
//        {
//            public string ValidationKey { get; set; }
//            public List<string> Permissions { get; set; }
//        }
//    }
//}






using Siemens.Engineering.AddIn.Utilities;
using Siemens.Engineering.SW.Blocks;
using System;
using System.Windows.Forms;

namespace LockUnlockBlock
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
        //internal static void RunExternalProcess(string handoverData, string projectPath)
        //{
        //    string exePath = @"C:\Generate Block\Debug\ConsoleApp6.exe";


        //    string demoText = "You passed the following arguments: \"" + handoverData + "\" - This command line was started from your TIA Portal AddIn as an additional process.";
        //    ProcessStartInfo startInfo = new ProcessStartInfo()
        //    {
        //        FileName = exePath,
        //        //Arguments = "/k echo " + demoText,
        //        Arguments = $"/C \"\"{exePath}\" \"{projectPath}\" \"",
        //        UseShellExecute = false,
        //        CreateNoWindow = true
        //        //"C:\Users\Wpuser\Downloads\Final Code Language Change STL to LAD and export logic with exe 01.07.25\ConsoleApp6 - Copy\bin\Debug\ConsoleApp6.exe"
        //    };

        //    Process cliProcess = new Process
        //    {
        //        EnableRaisingEvents = true,
        //        StartInfo = startInfo,
        //    };

        //    cliProcess.Start();
        //}

        internal static void RunExternalProcess(string handoverData, string projectPath)
        {
            //MessageBox.Show("0");
            string exePath = @"C:\Protect Block\Debug\ConsoleApp6.exe";

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = exePath,
                Arguments = $"\"{handoverData}\" \"{projectPath}\"",  // ✅ Correct arguments format
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
    }
}


//using System.Collections.Generic;
//using System.IO;
//using System.Text.Json;
//using System.Diagnostics;
//using System;
//using System.Windows.Forms;
//using Siemens.Engineering.SW.Blocks;

//namespace AddInExample
//{
//    internal static class Util
//    {
//        private static Dictionary<string, bool> _permissions;

//        public static void LoadPermissions()
//        {
//            try
//            {
//                string exeFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
//                string filePath = Path.Combine(exeFolder ?? "", "permissions.json");

//                if (File.Exists(filePath))
//                {
//                    string json = File.ReadAllText(filePath);
//                    _permissions = JsonSerializer.Deserialize<Dictionary<string, bool>>(json);
//                }
//                else
//                {
//                    // Default permissions if no file found — all enabled
//                    _permissions = new Dictionary<string, bool>
//                    {
//                        { "EXCEL", true },
//                        { "AWL", true },
//                        { "XML", true },
//                        { "EXCEL WITH TEST", true },
//                        { "AWL WITH TEST", true },
//                        { "XML WITH TEST", true }
//                    };
//                }
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine("Failed to load permissions.json: " + ex.Message);
//                _permissions = new Dictionary<string, bool>(); // Default deny all if error
//            }
//        }

//        public static bool HasPermission(string menuName)
//        {
//            if (_permissions == null)
//                LoadPermissions();

//            return _permissions.TryGetValue(menuName, out bool allowed) && allowed;
//        }

//        internal static void RunExternalProcess(string handoverData, string projectPath)
//        {
//            string exePath = @"C:\Generate Block\Debug\ConsoleApp6.exe";

//            ProcessStartInfo startInfo = new ProcessStartInfo()
//            {
//                FileName = exePath,
//                Arguments = $"\"{handoverData}\" \"{projectPath}\"",
//                UseShellExecute = false,
//                CreateNoWindow = true
//            };

//            Process cliProcess = new Process
//            {
//                EnableRaisingEvents = true,
//                StartInfo = startInfo
//            };

//            cliProcess.Start();
//        }

//        private static Form GetForegroundWindow(string title = null)
//        {
//            Form form = new Form { Opacity = 0, ShowIcon = false, Text = title };
//            form.Show();
//            form.TopMost = true;
//            form.Activate();
//            form.TopMost = false;
//            return form;
//        }

//        internal static void ShowBlockInfo(PlcBlock block)
//        {
//            string title = "AddIn - Programblock information";
//            string content = "Name: " + block.Name + "\nNumber: " + block.Number + "\nIs compiled: " + block.IsConsistent;
//            using (Form owner = GetForegroundWindow())
//            {
//                MessageBox.Show(owner, content, title);
//            }
//        }
//    }
//}
