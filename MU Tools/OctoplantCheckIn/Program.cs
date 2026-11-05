//using System;
//using System.IO;
//using System.Text;
//using System.Linq;
//using System.Threading;
//using System.Diagnostics;
//using System.Windows.Forms;
//using Action = System.Action;
//using System.Collections.Generic;
//using System.Security.Cryptography;
//using System.Text.RegularExpressions;
//using Label = System.Windows.Forms.Label;
//using Button = System.Windows.Forms.Button;
//using TextBox = System.Windows.Forms.TextBox;
//using CheckBox = System.Windows.Forms.CheckBox;
//using Application = System.Windows.Forms.Application;
//using System.Drawing;
//using System.Linq;

//class TiaPortalAutomation
//{
//    static System.Timers.Timer timer;
//    static List<OctoplantUserData> allUsers;

//    static string currentDir = Directory.GetCurrentDirectory();
//    static string logData = Path.Combine(currentDir, "LogData.txt");

//    // Use a fixed, known-writable location
//    private static readonly string ConfigFolder = @"C:\TIA_Auto_Config";
//    private static readonly string configFile = Path.Combine(ConfigFolder, "ConfigData.txt");

//    // File operation lock (fixes your error)
//    private static readonly object _fileLock = new object();

//    private static readonly string key = "MySecretKey12345";

//    public static string EncryptString(string plainText)
//    {
//        using (Aes aes = Aes.Create())
//        {
//            aes.Key = Encoding.UTF8.GetBytes(key);
//            aes.IV = new byte[16];
//            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
//            using (var ms = new MemoryStream())
//            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
//            using (var sw = new StreamWriter(cs))
//            {
//                sw.Write(plainText);
//                sw.Close();
//                return Convert.ToBase64String(ms.ToArray());
//            }
//        }
//    }

//    public static string DecryptString(string cipherText)
//    {
//        try
//        {
//            using (Aes aes = Aes.Create())
//            {
//                aes.Key = Encoding.UTF8.GetBytes(key);
//                aes.IV = new byte[16];
//                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
//                using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
//                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
//                using (var sr = new StreamReader(cs))
//                {
//                    return sr.ReadToEnd();
//                }
//            }
//        }
//        catch
//        {
//            return cipherText;
//        }
//    }

//    public static string Encrypt(string plainText) => EncryptString(plainText);
//    public static string Decrypt(string cipherText) => DecryptString(cipherText);

//    public static void SaveAllData(List<OctoplantUserData> allUsers)
//    {
//        if (!Directory.Exists(ConfigFolder))
//            Directory.CreateDirectory(ConfigFolder);

//        var sb = new StringBuilder();
//        foreach (var user in allUsers)
//        {
//            var encPwd = EncryptString(user.OctoplantPassword);
//            sb.AppendLine(
//                $"USER|{user.ServerUrl}|{user.ProjectName}|{user.ComponentForCheckIn}|{user.Interval}|{user.OctoplantProgramFiles}|{user.OctoplantClientArchive}|{user.OctoplantDomain}|{user.OctoplantUser}|{encPwd}|{user.DBFolder}|{user.Compare}|{user.ReleaseAfterCheckIn}"
//            );
//        }

//        File.WriteAllText(configFile, sb.ToString());
//        Console.WriteLine($"All user data saved to: {configFile}");
//    }

//    public static List<OctoplantUserData> LoadAllData()
//    {
//        var list = new List<OctoplantUserData>();
//        if (!File.Exists(configFile)) return list;

//        foreach (var line in File.ReadAllLines(configFile))
//        {
//            if (string.IsNullOrWhiteSpace(line)) continue;
//            var parts = line.Split('|');
//            if (parts.Length >= 12)
//            {
//                list.Add(new OctoplantUserData
//                {
//                    ServerUrl = parts[1],
//                    ProjectName = parts[2],
//                    ComponentForCheckIn = parts[3],
//                    Interval = parts[4],
//                    OctoplantProgramFiles = parts[5],
//                    OctoplantClientArchive = parts[6],
//                    OctoplantDomain = parts[7],
//                    OctoplantUser = parts[8],
//                    OctoplantPassword = DecryptString(parts[9]),
//                    DBFolder = parts[10],
//                    Compare = bool.TryParse(parts[11], out var cmp) && cmp,
//                    ReleaseAfterCheckIn = parts.Length >= 13 && bool.TryParse(parts[12], out var rel) && rel
//                });
//            }
//        }

//        Console.WriteLine("All user data loaded from ConfigData.txt");
//        return list;
//    }

//    [STAThread]
//    static void Main(string[] args)
//    {
//        Application.EnableVisualStyles();
//        Application.SetCompatibleTextRenderingDefault(false);

//        try
//        {
//            if (!Directory.Exists(ConfigFolder))
//                Directory.CreateDirectory(ConfigFolder);

//            string userDataFile = Path.Combine(currentDir, "UserData.txt");
//            if (!File.Exists(userDataFile))
//                File.WriteAllText(userDataFile, string.Empty);

//            allUsers = LoadAllData();

//            if (allUsers.Count > 0)
//            {
//                Console.WriteLine($"Loaded {allUsers.Count} user(s) from ConfigData.txt.");
//            }
//            else
//            {
//                Console.WriteLine("⚠No saved user configuration found.");
//            }

//            Application.Run(new MainForm());
//        }
//        catch (Exception ex)
//        {
//            MessageBox.Show("An unexpected error occurred during startup:\n" + ex.Message,
//                            "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//        }
//    }


//    public static void RunAutomationCycle(List<OctoplantUserData> allUsers)
//    {
//        allUsers = LoadAllData();

//        if (allUsers.Count == 0)
//        {
//            Console.WriteLine("No users found in local ConfigData.txt.");
//            return;
//        }

//        foreach (var user in allUsers)
//        {
//            Console.WriteLine($"Starting cycle for project: {user.ProjectName} ({DateTime.Now})");
//            File.AppendAllText(logData,
//                $"{DateTime.Now} : Project Name : {user.ProjectName}\n Server URL : {user.ServerUrl}\n User Name : {user.OctoplantUser}\n");

//            string version = "";
//            string change = "";

//            try
//            {
//                string workingDirectory = @"C:\Program Files\Siemens\Automation\Project Server\Bin";
//                string command = "pspt";
//                string arguments = "checkin show --server " + user.ServerUrl + " --project " + user.ProjectName;

//                var psi = new ProcessStartInfo
//                {
//                    FileName = "cmd.exe",
//                    Arguments = $"/c {command} {arguments}",
//                    WorkingDirectory = workingDirectory,
//                    RedirectStandardOutput = true,
//                    RedirectStandardError = true,
//                    UseShellExecute = false,
//                    CreateNoWindow = true
//                };

//                using (var process = new Process())
//                {
//                    process.StartInfo = psi;
//                    process.Start();

//                    string output = process.StandardOutput.ReadToEnd();
//                    string error = process.StandardError.ReadToEnd();

//                    process.WaitForExit();

//                    if (!string.IsNullOrEmpty(error))
//                    {
//                        Console.WriteLine("----- ERROR -----");
//                        Console.WriteLine(error);
//                        File.AppendAllText(logData, DateTime.Now + " : " + error + "\n");
//                    }

//                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
//                                      .Select(l => l.TrimEnd())
//                                      .ToList();

//                    var dataLines = lines
//                        .Where(line => !line.StartsWith("_") &&
//                                       !line.StartsWith("Workspace", StringComparison.OrdinalIgnoreCase) &&
//                                       !string.IsNullOrWhiteSpace(line))
//                        .ToList();

//                    if (dataLines.Count > 0)
//                    {
//                        var datalineLast = dataLines.Last();
//                        var parts = datalineLast.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

//                        if (parts.Length > 1) version = parts[1];
//                        if (parts.Length > 4) change = parts[4];

//                        var lastDataLine = lines.LastOrDefault(l => Regex.IsMatch(l.TrimStart(), @"^\d+"));
//                        if (!string.IsNullOrEmpty(lastDataLine))
//                        {
//                            var match = Regex.Match(lastDataLine, @"\d{2}[:\-]\d{2}[:\-]\d{4}\s+\d{2}:\d{2}:\d{2}\s+(.*)$");
//                            if (match.Success) change = match.Groups[1].Value.Trim();
//                        }

//                        File.AppendAllText(logData, DateTime.Now + " : " + datalineLast + "\n");
//                    }
//                }

//                try
//                {
//                    string fullPath1 = Path.Combine(user.DBFolder, user.ProjectName);
//                    string workingDir = @"C:\Program Files\Siemens\Automation\Project Server\Bin";
//                    string exportPath = user.DBFolder;
//                    string command1 = $"pspt project export --server {user.ServerUrl} --project {user.ProjectName} --path \"{exportPath}\"";
//                    string cmdArgs = $"/K cd /d \"{workingDir}\" && {command1}";

//                    if (Directory.Exists(fullPath1))
//                    {
//                        Directory.Delete(fullPath1, true);
//                        Console.WriteLine($"Folder deleted successfully: {fullPath1}");
//                    }

//                    var psi1 = new ProcessStartInfo
//                    {
//                        FileName = "cmd.exe",
//                        Arguments = cmdArgs,
//                        UseShellExecute = false,
//                        Verb = "runas",
//                        CreateNoWindow = true,
//                        RedirectStandardInput = true,
//                        RedirectStandardOutput = true,
//                        RedirectStandardError = true
//                    };

//                    using (var process = new Process())
//                    {
//                        process.StartInfo = psi1;
//                        process.Start();
//                        Thread.Sleep(10000);

//                        try
//                        {
//                            using (var sw = process.StandardInput)
//                            {
//                                if (sw.BaseStream.CanWrite)
//                                {
//                                    sw.WriteLine($"cd /d \"{workingDir}\"");
//                                    sw.WriteLine(command1);
//                                    Thread.Sleep(2000);
//                                    sw.WriteLine("y");
//                                    sw.WriteLine("exit");
//                                }
//                            }
//                        }
//                        catch {}

//                        string output = process.StandardOutput.ReadToEnd();
//                        string error = process.StandardError.ReadToEnd();
//                        process.WaitForExit();

//                        if (!string.IsNullOrEmpty(error))
//                        {
//                            Console.WriteLine("=== Error ===");
//                            Console.WriteLine(error);
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine("Error launching CMD: " + ex.Message);
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Exception: {ex.Message}");
//            }

//            Thread.Sleep(10000);
//            string fullPath = Path.Combine(user.DBFolder, user.ProjectName);

//            if (Directory.Exists(fullPath))
//            {
//                string component = (user.ComponentForCheckIn ?? "").TrimStart('\\', '/');
//                string localOctoplantPath = Path.Combine(@"C:\vdClientArchive", component);

//                try
//                {
//                    CopyDirectoryIncludingRoot(fullPath, localOctoplantPath);
//                }
//                catch {}

//                Thread.Sleep(10000);
//                try
//                {
//                    string comment = $"The Imported revision was {version} from server {user.ServerUrl} and project {user.ProjectName}.";
//                    string changes = string.IsNullOrWhiteSpace(change) ? $"Revision {version}" : $"Revision {version} : {change}";

//                    string plainPassword = user.OctoplantPassword; // already decrypted by LoadAllData()

//                    string iniFile = $@"[User]
//Account={user.OctoplantUser}
//Domain={user.OctoplantDomain}
//Password={plainPassword}
//[Component]
//Dir={user.ComponentForCheckIn}
//[Version]
//Enabled=Y
//WithoutComparison=N
//Comment={comment}
//ChangeReason={changes}";

//                    string iniPath = Path.Combine(currentDir, "AutoCheckIn.ini");
//                    File.WriteAllText(iniPath, iniFile);

//                    using (var process = new Process())
//                    {
//                        process.StartInfo.FileName = @"C:\Program Files (x86)\vdogClient\VDogAutoCheckIn.exe";
//                        process.StartInfo.Arguments = "/at:C /rd:C:\\vdClientArchive /CFile:" + iniPath;
//                        process.StartInfo.UseShellExecute = false;
//                        process.StartInfo.RedirectStandardOutput = true;
//                        process.StartInfo.RedirectStandardError = true;
//                        process.StartInfo.CreateNoWindow = true;

//                        process.Start();
//                        Thread.Sleep(10000);

//                        string output = process.StandardOutput.ReadToEnd();
//                        string error = process.StandardError.ReadToEnd();
//                        process.WaitForExit();

//                        if (!string.IsNullOrEmpty(error))
//                        {
//                            MessageBox.Show("Error: " + error, "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

//                            string checkInFolder = Path.Combine(currentDir, "CheckIn_Error");
//                            if (!Directory.Exists(checkInFolder))
//                                Directory.CreateDirectory(checkInFolder);

//                            string errorFilePath = Path.Combine(checkInFolder, $"Error_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
//                            File.WriteAllText(errorFilePath, error);
//                            File.AppendAllText(logData, DateTime.Now + " : " + error + "\n");
//                        }

//                        // ---------- AutoCheckOut ----------
//                        string exePath = @"C:\Program Files (x86)\vdogClient\VDogAutoCheckOut.exe";

//                        // FIXED: Use the DECRYPTED password in CLI args
//                        string arguments = @"/RD:""C:\vdClientArchive"" " +
//                                           @"/AT:C " +
//                                           $@"/DirR:{user.ComponentForCheckIn} " +
//                                           $@"/Account:{user.OctoplantUser} " +
//                                           $@"/Domain:{user.OctoplantDomain} " +
//                                           $@"/Password:{plainPassword} " +
//                                           @"/LockState:LockForOtherUsers " +
//                                           $@"/Comment:""Component_Is_Multiuser_Project""";

//                        var psi = new ProcessStartInfo
//                        {
//                            FileName = exePath,
//                            Arguments = arguments,
//                            UseShellExecute = false,
//                            RedirectStandardOutput = true,
//                            RedirectStandardError = true,
//                            CreateNoWindow = true
//                        };

//                        using (var process1 = new Process())
//                        {
//                            process1.StartInfo = psi;
//                            process1.Start();

//                            string output1 = process1.StandardOutput.ReadToEnd();
//                            string error1 = process1.StandardError.ReadToEnd();

//                            process1.WaitForExit();

//                            Console.WriteLine("Output:\n" + output1);
//                            Console.WriteLine("Error:\n" + error1);
//                        }

//                        MessageBox.Show("Execution successful!\n" + output, "Success");
//                        Thread.Sleep(2000);

//                        if (Directory.Exists(fullPath))
//                        {
//                            Directory.Delete(fullPath, true);
//                            Console.WriteLine($"Folder deleted successfully: {fullPath}");
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show("Error executing command: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                }
//            }

//            Console.WriteLine($"Cycle completed for {user.ProjectName} at {DateTime.Now}\n");
//        }
//    }

//    // ---------- Helpers ----------
//    static void CopyDirectoryIncludingRoot(string sourceDir, string destinationDir)
//    {
//        string folderName = Path.GetFileName(sourceDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
//        string destDir = Path.Combine(destinationDir, folderName);
//        CopyDirectory(sourceDir, destDir);
//    }

//    static void CopyDirectory(string sourceDir, string destinationDir)
//    {
//        if (!Directory.Exists(destinationDir))
//            Directory.CreateDirectory(destinationDir);

//        foreach (string file in Directory.GetFiles(sourceDir))
//            File.Copy(file, Path.Combine(destinationDir, Path.GetFileName(file)), true);

//        foreach (string subDir in Directory.GetDirectories(sourceDir))
//            CopyDirectory(subDir, Path.Combine(destinationDir, Path.GetFileName(subDir)));
//    }
//    // ---- add this inside TiaPortalAutomation ----
//    public static void AddServer(string serverName, string serverUrl)
//    {
//        if (string.IsNullOrWhiteSpace(serverName))
//            throw new ArgumentException("Server name is required.", nameof(serverName));
//        if (string.IsNullOrWhiteSpace(serverUrl))
//            throw new ArgumentException("Server URL is required.", nameof(serverUrl));

//        InitializeFiles();

//        string newLine = $"{serverName}|{serverUrl}";

//        lock (_fileLock)
//        {
//            var lines = File.ReadAllLines(ServerListFile).ToList();

//            // does a server with this name already exist?
//            int idx = lines.FindIndex(l =>
//                !string.IsNullOrWhiteSpace(l) &&
//                l.StartsWith(serverName + "|", StringComparison.OrdinalIgnoreCase));

//            if (idx >= 0)
//            {
//                // update URL (no duplicate names)
//                lines[idx] = newLine;
//                File.WriteAllLines(ServerListFile, lines);
//            }
//            else
//            {
//                // append new (no overwrite)
//                File.AppendAllText(ServerListFile, newLine + Environment.NewLine);
//            }
//        }
//    }

//    // ===== New file paths =====
//    public static readonly string ServerListFile = Path.Combine(ConfigFolder, "ServerList.txt");
//    public static readonly string GeneralConfigFile = Path.Combine(ConfigFolder, "GeneralConfig.txt");

//    // ===== Ensure folders/files exist =====
//    private static void InitializeFiles()
//    {
//        if (!Directory.Exists(ConfigFolder))
//            Directory.CreateDirectory(ConfigFolder);

//        // Touch files so downstream code can Append/Read safely
//        if (!File.Exists(configFile)) File.WriteAllText(configFile, string.Empty);
//        if (!File.Exists(ServerListFile)) File.WriteAllText(ServerListFile, string.Empty);
//        if (!File.Exists(GeneralConfigFile)) File.WriteAllText(GeneralConfigFile, string.Empty);
//    }

//    // ---------- GENERAL CONFIG (User-only data) ----------
//    public static void AppendGeneralConfig(GeneralConfig config)
//    {
//        InitializeFiles();

//        var line =
//            $"USERCFG|{config.OctoplantUser}|" +
//            $"{Encrypt(config.OctoplantPassword)}|" +
//            $"{config.Domain}|" +
//            $"{config.OctoplantProgram}|" +
//            $"{config.OctoplantClientArchive}|" +
//            $"{config.DBFolder}|" +
//            $"{config.Interval}|" +
//            $"{config.Compare}|" +
//            $"{config.ReleaseAfter}";

//        File.AppendAllText(GeneralConfigFile, line + Environment.NewLine);
//    }

//    public static List<GeneralConfig> LoadGeneralConfigs()
//    {
//        InitializeFiles();

//        var list = new List<GeneralConfig>();
//        foreach (var raw in File.ReadAllLines(GeneralConfigFile))
//        {
//            if (string.IsNullOrWhiteSpace(raw)) continue;
//            if (!raw.StartsWith("USERCFG|")) continue;

//            var parts = raw.Split('|');
//            if (parts.Length < 10) continue;

//            list.Add(new GeneralConfig
//            {
//                OctoplantUser = parts[1],
//                OctoplantPassword = Decrypt(parts[2]),
//                Domain = parts[3],
//                OctoplantProgram = parts[4],
//                OctoplantClientArchive = parts[5],
//                DBFolder = parts[6],
//                Interval = parts[7],
//                Compare = parts[8],
//                ReleaseAfter = parts[9]
//            });
//        }
//        return list;
//    }

//    // Legacy read/write (kept as-is; fixed a wrong bool index in loader)
//    static void SaveUsersToTextFile(List<OctoplantUserData> users, string filePath)
//    {
//        try
//        {
//            using (var writer = new StreamWriter(filePath, false))
//            {
//                foreach (var user in users)
//                {
//                    writer.WriteLine($"{user.ServerUrl}|{user.ProjectName}|{user.ComponentForCheckIn}|{user.Interval}|{user.OctoplantProgramFiles}|{user.OctoplantClientArchive}|{user.OctoplantDomain}|{user.OctoplantUser}|{user.OctoplantPassword}|{user.DBFolder}|{user.Compare}");
//                }
//            }
//            Console.WriteLine("User data saved to " + Path.GetFullPath(filePath));
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine("Error saving user data: " + ex.Message);
//        }
//    }

//    static List<OctoplantUserData> LoadUsersFromTextFile(string filePath)
//    {
//        var users = new List<OctoplantUserData>();
//        try
//        {
//            if (!File.Exists(filePath))
//                return users;

//            foreach (var line in File.ReadAllLines(filePath))
//            {
//                if (string.IsNullOrWhiteSpace(line)) continue;
//                var parts = line.Split('|');
//                if (parts.Length >= 11)
//                {
//                    users.Add(new OctoplantUserData
//                    {
//                        ServerUrl = parts[0],
//                        ProjectName = parts[1],
//                        ComponentForCheckIn = parts[2],
//                        Interval = parts[3],
//                        OctoplantProgramFiles = parts[4],
//                        OctoplantClientArchive = parts[5],
//                        OctoplantDomain = parts[6],
//                        OctoplantUser = parts[7],
//                        OctoplantPassword = parts[8], // legacy likely already encrypted/plain
//                        DBFolder = parts[9],
//                        Compare = bool.TryParse(parts[10], out var cmp) && cmp // fixed index
//                    });
//                }
//            }
//            Console.WriteLine("Loaded user data from " + Path.GetFullPath(filePath));
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine("Error loading user data: " + ex.Message);
//        }
//        return users;
//    }
//}


//public class ProjectInfo
//{
//    public string ProjectName { get; set; }
//    public string Component { get; set; }
//}

//public class ServerInfo
//{
//    public string ServerName { get; set; }
//    public string ServerUrl { get; set; }
//    public List<ProjectInfo> Projects { get; set; } = new List<ProjectInfo>();
//}

//public class GeneralConfig
//{
//    public string Version { get; set; }
//    public string Interval { get; set; }
//    public string OctoplantUser { get; set; }
//    public string OctoplantPassword { get; set; }
//    public string Domain { get; set; }
//    public string OctoplantProgram { get; set; }           // Added
//    public string OctoplantClientArchive { get; set; }     // Added
//    public string DBFolder { get; set; }                   // Added
//    public string Compare { get; set; }                    // Added
//    public string ReleaseAfter { get; set; }               // Added

//    public List<ServerInfo> Servers { get; set; } = new List<ServerInfo>();
//}


//public class MainForm : Form
//{
//    private Panel leftPanel;
//    private Panel mainPanel;
//    private Button btnGeneralConfig;
//    private Button btnGetConfig;
//    private Button btnAddServer;
//    private Button btnModifyServer;
//    private Button btnDeleteServer;
//    private Button btnAddProject;
//    private Button btnModifyProject;  // New button
//    private Button btnDeleteProject;  // New button
//    private Button btnStartService, btnStopService;
//    private System.Timers.Timer automationTimer;
//    private bool serviceRunning = false;

//    private TextBox txtOutput;

//    // In-memory server list
//    public static Dictionary<string, string> serverList = new Dictionary<string, string>();
//    public static Dictionary<string, List<(string ProjectName, string Component)>> projectList = new Dictionary<string, List<(string, string)>>(); // Now public static
//    public static List<GeneralConfig> generalConfigs = new List<GeneralConfig>(); // Now public static

//    public MainForm()
//    {
//        this.Text = "TIA Portal Project Server Importer Configurator";
//        this.Size = new Size(1000, 600);
//        this.StartPosition = FormStartPosition.CenterScreen;

//        // Left panel
//        leftPanel = new Panel()
//        {
//            Dock = DockStyle.Left,
//            Width = 200,
//            BackColor = Color.LightGray
//        };

//        // Main area
//        mainPanel = new Panel()
//        {
//            Dock = DockStyle.Fill,
//            BackColor = Color.White
//        };

//        // --- Buttons on the left ---
//        btnGetConfig = new Button()
//        {
//            Text = "Get Config",
//            Width = 160,
//            Height = 35,
//            Top = 20,
//            Left = 20
//        };
//        btnGetConfig.Click += BtnGetConfig_Click;

//        btnGeneralConfig = new Button()
//        {
//            Text = "General Config",
//            Width = 160,
//            Height = 35,
//            Top = 70,
//            Left = 20
//        };
//        //btnGeneralConfig.Click += (s, e) =>
//        //{
//        //    using (var form = new GeneralConfigForm())
//        //    {
//        //        if (form.ShowDialog() == DialogResult.OK)
//        //        {
//        //            var cfg = new GeneralConfig
//        //            {
//        //                Version = form.Version ?? "",
//        //                Interval = form.Interval ?? "",
//        //                OctoplantUser = form.OctoplantUser ?? "",
//        //                OctoplantPassword = form.OctoplantPassword ?? "",
//        //            };
//        //            generalConfigs.Add(cfg);

//        //            // Immediately persist config to local file
//        //            var users = generalConfigs.Select(gc => new OctoplantUserData
//        //            {
//        //                ServerUrl = "https://placeholder.local",
//        //                ProjectName = "DefaultProject",
//        //                ComponentForCheckIn = "DefaultComponent",
//        //                Interval = gc.Interval,
//        //                OctoplantProgramFiles = gc.OctoplantProgram,
//        //                OctoplantClientArchive = gc.OctoplantClientArchive,
//        //                OctoplantDomain = gc.Domain,
//        //                OctoplantUser = gc.OctoplantUser,
//        //                OctoplantPassword = TiaPortalAutomation.Encrypt(gc.OctoplantPassword),
//        //                DBFolder = gc.DBFolder,
//        //                Compare = gc.Compare?.ToLower() == "true",
//        //                ReleaseAfterCheckIn = gc.ReleaseAfter?.ToLower() == "true"
//        //            }).ToList();

//        //            TiaPortalAutomation.SaveAllData(users);

//        //            MessageBox.Show($"General Config saved for user '{cfg.OctoplantUser}'");

//        //        }
//        btnGeneralConfig.Click += (s, e) =>
//        {
//            using (var form = new GeneralConfigForm())
//            {
//                if (form.ShowDialog() == DialogResult.OK)
//                {
//                    var cfg = new GeneralConfig
//                    {
//                        Version = form.Version ?? "",
//                        Interval = form.Interval ?? "",
//                        OctoplantUser = form.OctoplantUser ?? "",
//                        OctoplantPassword = form.OctoplantPassword ?? "",
//                        Domain = form.OctoplantDomain ?? "",
//                        OctoplantProgram = form.OctoplantProgram ?? "",
//                        OctoplantClientArchive = form.OctoplantClientArchive ?? "",
//                        DBFolder = form.DBFolder ?? "",
//                        Compare = form.Compare ?? "",
//                        ReleaseAfter = form.ReleaseAfter ?? ""
//                    };

//                    // Keep in memory
//                    generalConfigs.Add(cfg);

//                    // Persist to GeneralConfig.txt
//                    TiaPortalAutomation.AppendGeneralConfig(cfg);

//                    MessageBox.Show($"General Config saved for user '{cfg.OctoplantUser}'",
//                        "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
//                }

//    }
//};

//        //btnAddServer = new Button()
//        //{
//        //    Text = "Add Server",
//        //    Width = 160,
//        //    Height = 35,
//        //    Top = 120,
//        //    Left = 20
//        //};
//        //btnAddServer.Click += (s, e) => new AddServerForm(generalConfigs, serverList, projectList).ShowDialog();
//        btnAddServer = new Button()
//        {
//            Text = "Add Server",
//            Width = 160,
//            Height = 35,
//            Top = 120,
//            Left = 20
//        };

//        // Updated to match the new constructor (no generalConfigs needed)
//        btnAddServer.Click += (s, e) => new AddServerForm(serverList, projectList).ShowDialog();

//        btnModifyServer = new Button()
//        {
//            Text = "Modify Server",
//            Width = 160,
//            Height = 35,
//            Top = 170,
//            Left = 20
//        };
//        btnModifyServer.Click += (s, e) => new ModifyServerForm(serverList).ShowDialog();

//        btnDeleteServer = new Button()
//        {
//            Text = "Delete Server",
//            Width = 160,
//            Height = 35,
//            Top = 220,
//            Left = 20
//        };
//        btnDeleteServer.Click += (s, e) => new DeleteServerForm(serverList).ShowDialog();

//        btnAddProject = new Button()
//        {
//            Text = "Add Project",
//            Width = 160,
//            Height = 35,
//            Top = 270,
//            Left = 20
//        };
//        btnAddProject.Click += (s, e) => new AddProjectForm(serverList, projectList, generalConfigs).ShowDialog();

//        // NEW BUTTON: Modify Project
//        btnModifyProject = new Button()
//        {
//            Text = "Modify Project",
//            Width = 160,
//            Height = 35,
//            Top = 320,
//            Left = 20
//        };
//        btnModifyProject.Click += (s, e) => new ModifyProjectForm(projectList).ShowDialog();

//        // NEW BUTTON: Delete Project
//        btnDeleteProject = new Button()
//        {
//            Text = "Delete Project",
//            Width = 160,
//            Height = 35,
//            Top = 370,
//            Left = 20
//        };
//        btnDeleteProject.Click += (s, e) => new DeleteProjectForm(projectList).ShowDialog();

//        // --- Output textbox ---
//        txtOutput = new TextBox()
//        {
//            Multiline = true,
//            Dock = DockStyle.Fill,
//            ScrollBars = ScrollBars.Vertical,
//            ReadOnly = true,
//            Font = new Font("Consolas", 11)
//        };

//        leftPanel.Controls.AddRange(new Control[]
//        {
//            btnGetConfig,
//            btnGeneralConfig,
//            btnAddServer,
//            btnModifyServer,
//            btnDeleteServer,
//            btnAddProject,
//            btnModifyProject,
//            btnDeleteProject
//        });
//        btnStartService = new Button()
//        {
//            Text = "Start Service",
//            Width = 160,
//            Height = 35,
//            Top = 420,
//            Left = 20
//        };
//        btnStartService.Click += BtnStartService_Click;

//        btnStopService = new Button()
//        {
//            Text = "Stop Service",
//            Width = 160,
//            Height = 35,
//            Top = 470,
//            Left = 20,
//            Enabled = false
//        };
//        btnStopService.Click += BtnStopService_Click;

//        leftPanel.Controls.AddRange(new Control[]
//        {
//    btnStartService,
//    btnStopService
//        });

//        mainPanel.Controls.Add(txtOutput);
//        this.Controls.Add(mainPanel);
//        this.Controls.Add(leftPanel);
//    }
//    private void BtnStartService_Click(object sender, EventArgs e)
//    {
//        if (serviceRunning) return;

//        var users = TiaPortalAutomation.LoadAllData();
//        if (users.Count == 0)
//        {
//            MessageBox.Show("No users found in ConfigData.txt", "Info");
//            return;
//        }

//        DisableAllButtons();
//        serviceRunning = true;
//        btnStopService.Enabled = true;

//        MessageBox.Show("Service started. Running automation cycle...");

//        TiaPortalAutomation.RunAutomationCycle(users);

//        int interval = int.TryParse(users.First().Interval, out int parsed) ? parsed : 5;
//        automationTimer = new System.Timers.Timer(interval * 60 * 1000);
//        automationTimer.Elapsed += (s, ev) => TiaPortalAutomation.RunAutomationCycle(users);
//        automationTimer.Start();
//    }

//    private void BtnStopService_Click(object sender, EventArgs e)
//    {
//        if (!serviceRunning) return;

//        automationTimer?.Stop();
//        automationTimer?.Dispose();
//        EnableAllButtons();
//        serviceRunning = false;

//        MessageBox.Show("Service stopped.");
//    }

//    private void DisableAllButtons()
//    {
//        foreach (Control c in leftPanel.Controls)
//            if (c is Button btn && btn != btnStopService)
//                btn.Enabled = false;
//    }

//    private void EnableAllButtons()
//    {
//        foreach (Control c in leftPanel.Controls)
//            if (c is Button btn)
//                btn.Enabled = true;
//    }

//    //private void BtnGetConfig_Click(object sender, EventArgs e)
//    //{
//    //    txtOutput.Clear();

//    //    txtOutput.Text =
//    //        "Version: 1" + Environment.NewLine +
//    //        "Interval: 5" + Environment.NewLine +
//    //        "Octoplant User: s03007223" + Environment.NewLine +
//    //        "--------------------------------" + Environment.NewLine +
//    //        "Server List:" + Environment.NewLine;

//    //    if (serverList.Count == 0)
//    //    {
//    //        txtOutput.AppendText("   (No servers added)" + Environment.NewLine);
//    //    }
//    //    else
//    //    {
//    //        foreach (var s in serverList)
//    //            txtOutput.AppendText($"   {s.Key}  ->  {s.Value}{Environment.NewLine}");
//    //    }
//    //}



//    private void BtnGetConfig_Click(object sender, EventArgs e)
//    {
//        txtOutput.Clear();

//        // If no configs exist
//        if (generalConfigs == null || generalConfigs.Count == 0)
//        {
//            txtOutput.Text = "No general configuration data available!" + Environment.NewLine;
//            return;
//        }

//        int index = 1;
//        foreach (var cfg in generalConfigs)
//        {
//            txtOutput.AppendText($"========== Configuration {index++} ==========" + Environment.NewLine);
//            txtOutput.AppendText($"Version: {cfg.Version}" + Environment.NewLine);
//            txtOutput.AppendText($"Interval: {cfg.Interval}" + Environment.NewLine);
//            txtOutput.AppendText($"Octoplant User: {cfg.OctoplantUser}" + Environment.NewLine);
//            txtOutput.AppendText($"Password: {cfg.OctoplantPassword}" + Environment.NewLine);
//            txtOutput.AppendText("--------------------------------" + Environment.NewLine);

//            // --- Server List Section ---
//            txtOutput.AppendText("Server List:" + Environment.NewLine);
//            if (serverList == null || serverList.Count == 0)
//            {
//                txtOutput.AppendText("   (No servers added)" + Environment.NewLine);
//            }
//            else
//            {
//                foreach (var s in serverList)
//                {
//                    bool check = false;
//                    if (cfg.OctoplantUser == s.Key.Split(',')[1].Trim())
//                    {
//                        check = true;
//                        txtOutput.AppendText($"   {s.Key}  ->  {s.Value}{Environment.NewLine}");
//                    }
//                    if (check == true)
//                    {

//                    foreach (var s1 in projectList)
//                    {
//                        // Match by server name part (before comma)
//                        if (s.Key.Split(',')[0].Trim() == s1.Key.Split(',')[0].Trim())
//                        {
//                            txtOutput.AppendText($"   🔹 Projects for {s1.Key}:{Environment.NewLine}");

//                            // Loop through all projects for that server
//                            foreach (var proj in s1.Value)
//                            {
//                                txtOutput.AppendText($"      → {proj.ProjectName} (Component: {proj.Component}){Environment.NewLine}");
//                            }
//                        }
//                    }

//                    }
//                }                
//            }
//            txtOutput.AppendText(Environment.NewLine);
//        }
//    }
//}

//public class AddProjectForm : Form
//{
//    private ComboBox cmbUser, cmbServer;
//    private TextBox txtProjectName, txtComponent;
//    private Button btnOK, btnCancel;

//    private Dictionary<string, string> servers;
//    private Dictionary<string, List<(string ProjectName, string Component)>> project;
//    private List<GeneralConfig> generalConfigs;

//    public string SelectedUser { get; private set; }
//    public string SelectedServer { get; private set; }
//    public string ProjectName { get; private set; }
//    public string Component { get; private set; }

//    public AddProjectForm(Dictionary<string, string> serverList,
//                          Dictionary<string, List<(string ProjectName, string Component)>> projectList,
//                          List<GeneralConfig> generalConfigs)
//    {
//        this.servers = serverList ?? new Dictionary<string, string>();
//        this.project = projectList ?? new Dictionary<string, List<(string ProjectName, string Component)>>();
//        this.generalConfigs = generalConfigs ?? new List<GeneralConfig>();

//        this.Text = "Append Project";
//        this.Size = new Size(400, 320);
//        this.StartPosition = FormStartPosition.CenterParent;
//        this.FormBorderStyle = FormBorderStyle.FixedDialog;
//        this.MaximizeBox = false;

//        // --- User Dropdown ---
//        Label lblUser = new Label()
//        {
//            Text = "Select Octoplant User:",
//            Left = 20,
//            Top = 20,
//            Width = 150
//        };

//        cmbUser = new ComboBox()
//        {
//            Left = 180,
//            Top = 20,
//            Width = 180,
//            DropDownStyle = ComboBoxStyle.DropDownList
//        };

//        if (generalConfigs.Count > 0)
//            cmbUser.Items.AddRange(generalConfigs.Select(x => x.OctoplantUser).ToArray());
//        else
//            cmbUser.Items.Add("(No users available)");

//        cmbUser.SelectedIndexChanged += CmbUser_SelectedIndexChanged;

//        // --- Server Dropdown ---
//        Label lblServer = new Label()
//        {
//            Text = "Select Server:",
//            Left = 20,
//            Top = 70,
//            Width = 150
//        };

//        cmbServer = new ComboBox()
//        {
//            Left = 180,
//            Top = 70,
//            Width = 180,
//            DropDownStyle = ComboBoxStyle.DropDownList
//        };

//        // Load all servers regardless of user
//        if (servers.Count > 0)
//            cmbServer.Items.AddRange(servers.Keys.ToArray());
//        else
//            cmbServer.Items.Add("(No servers available)");

//        // --- Project Name ---
//        Label lblProj = new Label()
//        {
//            Text = "Project Name:",
//            Left = 20,
//            Top = 120,
//            Width = 120
//        };

//        txtProjectName = new TextBox()
//        {
//            Left = 180,
//            Top = 120,
//            Width = 180
//        };

//        // --- Component ---
//        Label lblComp = new Label()
//        {
//            Text = "Component:",
//            Left = 20,
//            Top = 160,
//            Width = 120
//        };

//        txtComponent = new TextBox()
//        {
//            Left = 180,
//            Top = 160,
//            Width = 180
//        };

//        // --- Cancel ---
//        btnCancel = new Button()
//        {
//            Text = "Cancel",
//            Left = 80,
//            Top = 220,
//            Width = 100
//        };
//        btnCancel.Click += (s, e) => this.Close();

//        // --- OK ---
//        btnOK = new Button()
//        {
//            Text = "OK",
//            Left = 200,
//            Top = 220,
//            Width = 100,
//            Enabled = false
//        };
//        btnOK.Click += BtnOK_Click;

//        txtProjectName.TextChanged += ValidateInput;
//        txtComponent.TextChanged += ValidateInput;
//        cmbServer.SelectedIndexChanged += ValidateInput;

//        Controls.AddRange(new Control[]
//        {
//            lblUser, cmbUser,
//            lblServer, cmbServer,
//            lblProj, txtProjectName,
//            lblComp, txtComponent,
//            btnCancel, btnOK
//        });
//    }

//    private void CmbUser_SelectedIndexChanged(object sender, EventArgs e)
//    {
//        SelectedUser = cmbUser.SelectedItem?.ToString();
//        // No filtering — servers stay visible for all users
//    }

//    private void ValidateInput(object sender, EventArgs e)
//    {
//        btnOK.Enabled = (cmbServer.SelectedIndex >= 0 &&
//                         !string.IsNullOrWhiteSpace(txtProjectName.Text) &&
//                         !string.IsNullOrWhiteSpace(txtComponent.Text));
//    }

//    private void BtnOK_Click(object sender, EventArgs e)
//    {
//        SelectedServer = cmbServer.SelectedItem?.ToString();
//        ProjectName = txtProjectName.Text.Trim();
//        Component = txtComponent.Text.Trim();

//        if (string.IsNullOrEmpty(SelectedServer) || string.IsNullOrEmpty(ProjectName))
//        {
//            MessageBox.Show("Please fill all details.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
//            return;
//        }

//        // Ensure project list entry exists
//        if (!project.ContainsKey(SelectedServer.Split(',')[0]))
//            project[SelectedServer.Split(',')[0]] = new List<(string ProjectName, string Component)>();

//        // Add project entry
//        project[SelectedServer.Split(',')[0]].Add((ProjectName, Component));

//        MessageBox.Show(
//            $"Project '{ProjectName}' added to server '{SelectedServer}' successfully!",
//            "Success",
//            MessageBoxButtons.OK,
//            MessageBoxIcon.Information);
//        // Persist project data along with all general configs
//        TiaPortalAutomation.SaveAllData(
//            MainForm.generalConfigs.Select(cfg => new OctoplantUserData
//            {
//                ServerUrl = servers[SelectedServer],
//                ProjectName = ProjectName,
//                ComponentForCheckIn = Component,
//                Interval = cfg.Interval,
//                OctoplantProgramFiles = cfg.OctoplantProgram,
//                OctoplantClientArchive = cfg.OctoplantClientArchive,
//                OctoplantDomain = cfg.Domain,
//                OctoplantUser = cfg.OctoplantUser,
//                OctoplantPassword = TiaPortalAutomation.Encrypt(cfg.OctoplantPassword),
//                DBFolder = cfg.DBFolder,
//                Compare = cfg.Compare?.ToLower() == "true",
//                ReleaseAfterCheckIn = cfg.ReleaseAfter?.ToLower() == "true"
//            }).ToList()
//        );

//        this.DialogResult = DialogResult.OK;
//        this.Close();
//    }
//}


//#region Add Server Form

//public class AddServerForm : Form
//{
//    private TextBox txtServerName, txtServerUrl;
//    private Button btnOK, btnCancel;

//    private Dictionary<string, string> servers;
//    private Dictionary<string, List<(string ProjectName, string Component)>> project;

//    public string ServerName { get; private set; }
//    public string ServerUrl { get; private set; }

//    public AddServerForm(Dictionary<string, string> serverList, Dictionary<string, List<(string ProjectName, string Component)>> projectList)
//    {
//        this.servers = serverList ?? new Dictionary<string, string>();
//        this.project = projectList ?? new Dictionary<string, List<(string ProjectName, string Component)>>();

//        this.Text = "Add Server";
//        this.Size = new Size(380, 250);
//        this.StartPosition = FormStartPosition.CenterParent;
//        this.FormBorderStyle = FormBorderStyle.FixedDialog;
//        this.MaximizeBox = false;

//        // --- Server Name ---
//        Label lblServerName = new Label()
//        {
//            Text = "Server Name:",
//            Left = 20,
//            Top = 30,
//            Width = 120
//        };

//        txtServerName = new TextBox()
//        {
//            Left = 150,
//            Top = 25,
//            Width = 190
//        };

//        // --- Server URL ---
//        Label lblServerUrl = new Label()
//        {
//            Text = "Server URL:",
//            Left = 20,
//            Top = 75,
//            Width = 120
//        };

//        txtServerUrl = new TextBox()
//        {
//            Left = 150,
//            Top = 70,
//            Width = 190
//        };

//        // --- Cancel Button ---
//        btnCancel = new Button()
//        {
//            Text = "Cancel",
//            Left = 70,
//            Top = 130,
//            Width = 100
//        };
//        btnCancel.Click += (s, e) => this.Close();

//        // --- OK Button ---
//        btnOK = new Button()
//        {
//            Text = "Add Server",
//            Left = 200,
//            Top = 130,
//            Width = 100,
//            Enabled = false
//        };
//        btnOK.Click += BtnOK_Click;

//        // --- Validation Events ---
//        txtServerName.TextChanged += ValidateInput;
//        txtServerUrl.TextChanged += ValidateInput;

//        Controls.AddRange(new Control[]
//        {
//            lblServerName, txtServerName,
//            lblServerUrl, txtServerUrl,
//            btnCancel, btnOK
//        });
//    }

//    private void ValidateInput(object sender, EventArgs e)
//    {
//        btnOK.Enabled = !string.IsNullOrWhiteSpace(txtServerName.Text) &&
//                        !string.IsNullOrWhiteSpace(txtServerUrl.Text);
//    }

//    private void BtnOK_Click(object sender, EventArgs e)
//    {
//        ServerName = txtServerName.Text.Trim();
//        ServerUrl = txtServerUrl.Text.Trim();

//        if (servers.ContainsKey(ServerName))
//        {
//            MessageBox.Show("Server name already exists!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
//            return;
//        }

//        // Add to in-memory list
//        servers[ServerName] = ServerUrl;
//        if (!project.ContainsKey(ServerName))
//            project[ServerName] = new List<(string ProjectName, string Component)>();

//        // Persist server to ServerList.txt
//        try
//        {
//            TiaPortalAutomation.AddServer(ServerName, ServerUrl);
//            MessageBox.Show(
//                $"Server '{ServerName}' added successfully and saved to ServerList.txt!",
//                "Success",
//                MessageBoxButtons.OK,
//                MessageBoxIcon.Information);
//        }
//        catch (Exception ex)
//        {
//            MessageBox.Show("Failed to save server.\n\n" + ex.Message,
//                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//        }

//        this.DialogResult = DialogResult.OK;
//        this.Close();
//    }
//}


//#endregion

//#region Modify Server Form
//public class ModifyServerForm : Form
//{
//    private ComboBox cmbServers;
//    private TextBox txtName, txtUrl;
//    private Button btnUpdate;
//    private Dictionary<string, string> servers;

//    public ModifyServerForm(Dictionary<string, string> serverList)
//    {
//        servers = serverList;
//        this.Text = "Modify Server";
//        this.Size = new Size(420, 300);
//        this.StartPosition = FormStartPosition.CenterParent;

//        Label lblSelect = new Label() { Text = "Select Server:", Left = 30, Top = 30, Width = 100 };
//        cmbServers = new ComboBox() { Left = 140, Top = 30, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
//        cmbServers.Items.AddRange(servers.Keys.ToArray());
//        cmbServers.SelectedIndexChanged += (s, e) =>
//        {
//            if (cmbServers.SelectedItem != null)
//            {
//                string selected = cmbServers.SelectedItem.ToString();
//                txtName.Text = selected;
//                txtUrl.Text = servers[selected];
//            }
//        };

//        Label lblName = new Label() { Text = "New Name:", Left = 30, Top = 90, Width = 100 };
//        txtName = new TextBox() { Left = 140, Top = 90, Width = 220 };

//        Label lblUrl = new Label() { Text = "New URL:", Left = 30, Top = 140, Width = 100 };
//        txtUrl = new TextBox() { Left = 140, Top = 140, Width = 220 };

//        btnUpdate = new Button() { Text = "Update", Left = 140, Top = 190, Width = 100 };
//        btnUpdate.Click += (s, e) =>
//        {
//            if (cmbServers.SelectedItem == null)
//            {
//                MessageBox.Show("Please select a server.");
//                return;
//            }

//            string oldName = cmbServers.SelectedItem.ToString();
//            string newName = string.IsNullOrWhiteSpace(txtName.Text) ? oldName : txtName.Text;
//            string newUrl = string.IsNullOrWhiteSpace(txtUrl.Text) ? servers[oldName] : txtUrl.Text;

//            servers.Remove(oldName);
//            servers[newName] = newUrl;

//            MessageBox.Show("Server updated successfully!");
//            this.Close();
//        };

//        Controls.AddRange(new Control[] { lblSelect, cmbServers, lblName, txtName, lblUrl, txtUrl, btnUpdate });
//    }
//}
//#endregion

//#region Delete Server Form
//public class DeleteServerForm : Form
//{
//    private ComboBox cmbServers;
//    private Button btnDelete;
//    private Dictionary<string, string> servers;

//    public DeleteServerForm(Dictionary<string, string> serverList)
//    {
//        servers = serverList;
//        this.Text = "Delete Server";
//        this.Size = new Size(400, 200);
//        this.StartPosition = FormStartPosition.CenterParent;

//        Label lblSelect = new Label() { Text = "Select Server:", Left = 30, Top = 30, Width = 100 };
//        cmbServers = new ComboBox() { Left = 140, Top = 30, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
//        cmbServers.Items.AddRange(servers.Keys.ToArray());

//        btnDelete = new Button() { Text = "Delete", Left = 140, Top = 80, Width = 100 };
//        btnDelete.Click += (s, e) =>
//        {
//            if (cmbServers.SelectedItem == null)
//            {
//                MessageBox.Show("Please select a server to delete.");
//                return;
//            }

//            string selected = cmbServers.SelectedItem.ToString();
//            servers.Remove(selected);
//            MessageBox.Show("Server deleted successfully!");
//            this.Close();
//        };

//        Controls.AddRange(new Control[] { lblSelect, cmbServers, btnDelete });
//    }
//}
//#endregion

//#region General Config Form
//public class GeneralConfigForm : Form
//{
//    // Public properties to access entered values
//    public string Version { get; private set; }
//    public string Interval { get; private set; }
//    public string OctoplantProgram { get; private set; }
//    public string OctoplantClientArchive { get; private set; }
//    public string OctoplantUser { get; private set; }
//    public string OctoplantPassword { get; private set; }
//    public string DBFolder { get; private set; }
//    public string Compare { get; private set; }
//    public string ReleaseAfter { get; private set; }
//    public string OctoplantDomain { get; private set; }

//    // TextBoxes
//    private TextBox txtVersion, txtInterval, txtProgram, txtClientArchive,
//                    txtUser, txtPassword, txtDBFolder, txtCompare, txtReleaseAfter, txtDomain;

//    public GeneralConfigForm()
//    {
//        this.Text = "General Configuration";
//        this.Size = new Size(400, 500);
//        this.StartPosition = FormStartPosition.CenterParent;
//        this.AutoScroll = true;

//        int top = 20;
//        int labelWidth = 150;
//        int boxWidth = 200;
//        int gap = 40;

//        // Helper function
//        Label AddLabel(string text, int y)
//        {
//            var lbl = new Label() { Text = text, Left = 20, Top = y + 5, Width = labelWidth };
//            this.Controls.Add(lbl);
//            return lbl;
//        }

//        TextBox AddTextBox(int y, string defaultValue = "")
//        {
//            var txt = new TextBox() { Left = 180, Top = y, Width = boxWidth, Text = defaultValue };
//            this.Controls.Add(txt);
//            return txt;
//        }

//        // Add all input fields
//        AddLabel("Version:", top);
//        txtVersion = AddTextBox(top, "1");
//        top += gap;

//        AddLabel("Interval:", top);
//        txtInterval = AddTextBox(top, "5");
//        top += gap;

//        AddLabel("Octoplant Program Folder:", top);
//        txtProgram = AddTextBox(top, @"C:\Program Files (x86)\vdogClient");
//        top += gap;

//        AddLabel("Octoplant Client Archive:", top);
//        txtClientArchive = AddTextBox(top, @"C:\vdClientArchive");
//        top += gap;

//        AddLabel("Octoplant User:", top);
//        txtUser = AddTextBox(top, "s03007223");
//        top += gap;

//        AddLabel("Octoplant Password:", top);
//        txtPassword = AddTextBox(top);
//        txtPassword.PasswordChar = '●';
//        top += gap;

//        AddLabel("DB Folder:", top);
//        txtDBFolder = AddTextBox(top, @"C:\DBFolder");
//        top += gap;

//        AddLabel("Compare:", top);
//        txtCompare = AddTextBox(top, "true");
//        top += gap;

//        AddLabel("Release After:", top);
//        txtReleaseAfter = AddTextBox(top, "false");
//        top += gap;

//        AddLabel("Octoplant Domain:", top);
//        txtDomain = AddTextBox(top, "wipropari");
//        top += gap + 20;

//        // OK & Cancel buttons
//        Button btnOK = new Button() { Text = "Ok", Left = 180, Top = top, Width = 80 };
//        btnOK.Click += BtnOK_Click;

//        Button btnCancel = new Button() { Text = "Cancel", Left = 270, Top = top, Width = 80 };
//        btnCancel.Click += (s, e) => this.Close();

//        Controls.AddRange(new Control[] { btnOK, btnCancel });
//    }

//    private void BtnOK_Click(object sender, EventArgs e)
//    {
//        Version = txtVersion.Text;
//        Interval = txtInterval.Text;
//        OctoplantProgram = txtProgram.Text;
//        OctoplantClientArchive = txtClientArchive.Text;
//        OctoplantUser = txtUser.Text;
//        OctoplantPassword = txtPassword.Text;
//        DBFolder = txtDBFolder.Text;
//        Compare = txtCompare.Text;
//        ReleaseAfter = txtReleaseAfter.Text;
//        OctoplantDomain = txtDomain.Text;

//        this.DialogResult = DialogResult.OK;
//        this.Close();
//    }
//}
//#endregion
//public class GeneralConfigData
//{
//    public string Version { get; set; }
//    public string Interval { get; set; }
//    public string Component { get; set; }
//    public string ProgramFile { get; set; }
//    public string ClientArchive { get; set; }
//    public string Domain { get; set; }
//    public string User { get; set; }
//    public string Password { get; set; }
//    public string DBFolder { get; set; }
//}
//public partial class ControlPopupForm : Form
//{
//    public event Action OnStopClicked;
//    public event Action OnStartNewClicked;

//    public ControlPopupForm()
//    {
//        //InitializeComponent();
//        this.Text = "Automation Control";
//        this.FormBorderStyle = FormBorderStyle.FixedDialog;
//        this.StartPosition = FormStartPosition.CenterScreen;
//        this.TopMost = true;
//        this.MaximizeBox = false;
//        this.MinimizeBox = false;
//        this.Width = 300;
//        this.Height = 150;

//        Button btnStop = new Button()
//        {
//            Text = "Stop",
//            Width = 100,
//            Height = 40,
//            Left = 30,
//            Top = 40
//        };
//        btnStop.Click += (s, e) => OnStopClicked?.Invoke();

//        Button btnStartNew = new Button()
//        {
//            Text = "Start New",
//            Width = 100,
//            Height = 40,
//            Left = 150,
//            Top = 40
//        };
//        btnStartNew.Click += (s, e) => OnStartNewClicked?.Invoke();

//        Controls.Add(btnStop);
//        Controls.Add(btnStartNew);
//    }
//}
//public class OctoplantInputForm : Form
//{
//    // Final list that will store all user entries
//    public List<OctoplantUserData> UsersData { get; private set; } = new List<OctoplantUserData>();

//    // Input fields
//    TextBox txtServerUrl, txtProjectName, txtComponent;
//    TextBox txtInterval, txtProgramFile, txtClientArchive, txtDomain, txtUser, txtPassword, txtDBFolder;
//    CheckBox chkCompare, chkReleaseAfterCheckIn;
//    Button btnBrowseProgram, btnBrowseArchive, btnBrowseDB, btnAddUser, btnSubmit, btnCancel;

//    public OctoplantInputForm()
//    {
//        Text = "Octoplant Configuration";
//        Width = 520;
//        Height = 630;
//        StartPosition = FormStartPosition.CenterScreen;
//        FormBorderStyle = FormBorderStyle.FixedDialog;
//        MaximizeBox = false;

//        int leftLabel = 20, leftInput = 180, labelWidth = 150, inputWidth = 220;
//        int top = 20, gap = 40;

//        // ---- Server URL ----
//        Label lblServerUrl = new Label { Text = "Server URL:", Top = top, Left = leftLabel, Width = labelWidth };
//        txtServerUrl = new TextBox { Top = top, Left = leftInput, Width = inputWidth };
//        top += gap;

//        // ---- Project Name ----
//        Label lblProjectName = new Label { Text = "Project Name:", Top = top, Left = leftLabel, Width = labelWidth };
//        txtProjectName = new TextBox { Top = top, Left = leftInput, Width = inputWidth };
//        top += gap;

//        // ---- Component for Check-In ----
//        Label lblComponent = new Label { Text = "Component for Check-In:", Top = top, Left = leftLabel, Width = labelWidth };
//        txtComponent = new TextBox { Top = top, Left = leftInput, Width = inputWidth };
//        top += gap;

//        // ---- Interval ----
//        Label lblInterval = new Label { Text = "Interval:", Top = top, Left = leftLabel, Width = labelWidth };
//        txtInterval = new TextBox { Top = top, Left = leftInput, Width = inputWidth };
//        top += gap;

//        // ---- Octoplant Program Folder ----
//        Label lblProgramFile = new Label { Text = "Octoplant Program Folder:", Top = top, Left = leftLabel, Width = labelWidth };
//        txtProgramFile = new TextBox { Top = top, Left = leftInput, Width = inputWidth };
//        btnBrowseProgram = new Button { Text = "...", Top = top, Left = leftInput + inputWidth + 10, Width = 30 };
//        btnBrowseProgram.Click += (s, e) => BrowseFolder(txtProgramFile);
//        top += gap;

//        // ---- Octoplant Client Archive ----
//        Label lblClientArchive = new Label { Text = "Octoplant Client Archive:", Top = top, Left = leftLabel, Width = labelWidth };
//        txtClientArchive = new TextBox { Top = top, Left = leftInput, Width = inputWidth };
//        btnBrowseArchive = new Button { Text = "...", Top = top, Left = leftInput + inputWidth + 10, Width = 30 };
//        btnBrowseArchive.Click += (s, e) => BrowseFolder(txtClientArchive);
//        top += gap;

//        // ---- Domain ----
//        Label lblDomain = new Label { Text = "Octoplant Domain:", Top = top, Left = leftLabel, Width = labelWidth };
//        txtDomain = new TextBox { Top = top, Left = leftInput, Width = inputWidth };
//        top += gap;

//        // ---- User ----
//        Label lblUser = new Label { Text = "Octoplant User:", Top = top, Left = leftLabel, Width = labelWidth };
//        txtUser = new TextBox { Top = top, Left = leftInput, Width = inputWidth };
//        top += gap;

//        // ---- Password ----
//        Label lblPassword = new Label { Text = "Octoplant Password:", Top = top, Left = leftLabel, Width = labelWidth };
//        txtPassword = new TextBox { Top = top, Left = leftInput, Width = inputWidth, UseSystemPasswordChar = true };
//        top += gap;

//        // ---- DB Folder ----
//        Label lblDBFolder = new Label { Text = "DB Folder:", Top = top, Left = leftLabel, Width = labelWidth };
//        txtDBFolder = new TextBox { Top = top, Left = leftInput, Width = inputWidth };
//        btnBrowseDB = new Button { Text = "...", Top = top, Left = leftInput + inputWidth + 10, Width = 30 };
//        btnBrowseDB.Click += (s, e) => BrowseFolder(txtDBFolder);
//        top += gap;

//        // ---- Checkboxes ----
//        chkCompare = new CheckBox { Text = "Compare", Top = top, Left = leftInput, Width = 120 };
//        top += 30;
//        chkReleaseAfterCheckIn = new CheckBox { Text = "Release After Check-In", Top = top, Left = leftInput, Width = 200 };
//        top += 50;

//        // ---- Buttons ----
//        btnAddUser = new Button { Text = "Add User", Top = top, Left = leftInput - 80, Width = 100 };
//        btnSubmit = new Button { Text = "Submit All", Top = top, Left = leftInput + 30, Width = 100, DialogResult = DialogResult.OK };
//        btnCancel = new Button { Text = "Cancel", Top = top, Left = leftInput + 150, Width = 100, DialogResult = DialogResult.Cancel };

//        btnAddUser.Click += BtnAddUser_Click;
//        btnSubmit.Click += BtnSubmit_Click;

//        Controls.AddRange(new Control[]
//        {
//            lblServerUrl, txtServerUrl,
//            lblProjectName, txtProjectName,
//            lblComponent, txtComponent,
//            lblInterval, txtInterval,
//            lblProgramFile, txtProgramFile, btnBrowseProgram,
//            lblClientArchive, txtClientArchive, btnBrowseArchive,
//            lblDomain, txtDomain,
//            lblUser, txtUser,
//            lblPassword, txtPassword,
//            lblDBFolder, txtDBFolder, btnBrowseDB,
//            chkCompare, chkReleaseAfterCheckIn,
//            btnAddUser, btnSubmit, btnCancel
//        });
//    }

//    private void BrowseFolder(TextBox target)
//    {
//        using (var fbd = new FolderBrowserDialog())
//        {
//            if (fbd.ShowDialog() == DialogResult.OK)
//                target.Text = fbd.SelectedPath;
//        }
//    }

//    private void BtnAddUser_Click(object sender, EventArgs e)
//    {
//        if (string.IsNullOrWhiteSpace(txtUser.Text))
//        {
//            MessageBox.Show("Please enter a username before adding.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
//            return;
//        }

//        UsersData.Add(GetCurrentUserData());
//        MessageBox.Show($"User '{txtUser.Text}' added successfully!", "Added", MessageBoxButtons.OK, MessageBoxIcon.Information);

//        // clear only user-specific fields for next entry
//        txtDomain.Clear();
//        txtUser.Clear();
//        txtPassword.Clear();
//        chkCompare.Checked = false;
//        chkReleaseAfterCheckIn.Checked = false;
//    }

//    private void BtnSubmit_Click(object sender, EventArgs e)
//    {
//        // Add the last entry if user forgot to click "Add User"
//        if (!string.IsNullOrWhiteSpace(txtUser.Text))
//            UsersData.Add(GetCurrentUserData());

//        //if (UsersData.Count == 0)
//        //{
//        //    MessageBox.Show("Please add at least one user before submitting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//        //    this.DialogResult = DialogResult.None;
//        //}
//    }
//    private static readonly string key = "MySecretKey12345";
//    public static string Encrypt(string plainText)
//    {
//        using (Aes aes = Aes.Create())
//        {
//            aes.Key = Encoding.UTF8.GetBytes(key);
//            aes.IV = new byte[16]; // 16-byte zero IV (for simplicity)

//            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
//            using (var ms = new MemoryStream())
//            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
//            using (var sw = new StreamWriter(cs))
//            {
//                sw.Write(plainText);
//                sw.Close();
//                return Convert.ToBase64String(ms.ToArray());
//            }
//        }
//    }
//    private OctoplantUserData GetCurrentUserData()
//    {
//        return new OctoplantUserData
//        {
//            ServerUrl = txtServerUrl.Text,
//            ProjectName = txtProjectName.Text,
//            ComponentForCheckIn = txtComponent.Text,
//            Interval = txtInterval.Text,
//            OctoplantProgramFiles = txtProgramFile.Text,
//            OctoplantClientArchive = txtClientArchive.Text,
//            OctoplantDomain = txtDomain.Text,
//            OctoplantUser = txtUser.Text,
//            // 🔒 Encrypt password before storing
//            OctoplantPassword = Encrypt(txtPassword.Text),
//            DBFolder = txtDBFolder.Text,
//            Compare = chkCompare.Checked,
//            ReleaseAfterCheckIn = chkReleaseAfterCheckIn.Checked
//        };
//    }
//}

//public class OctoplantUserData
//{
//    public string ServerUrl { get; set; }
//    public string ProjectName { get; set; }
//    public string ComponentForCheckIn { get; set; }
//    public string Interval { get; set; }
//    public string OctoplantProgramFiles { get; set; }
//    public string OctoplantClientArchive { get; set; }
//    public string OctoplantDomain { get; set; }
//    public string OctoplantUser { get; set; }
//    public string OctoplantPassword { get; set; }
//    public string DBFolder { get; set; }
//    public bool Compare { get; set; }
//    public bool ReleaseAfterCheckIn { get; set; }
//}
//public class DeleteProjectForm : Form
//{
//    private ComboBox cmbServer, cmbProject;
//    private Button btnDelete;
//    private Dictionary<string, List<(string ProjectName, string Component)>> projectList;

//    public DeleteProjectForm(Dictionary<string, List<(string ProjectName, string Component)>> projects)
//    {
//        projectList = projects;
//        this.Text = "Delete Project";
//        this.Size = new Size(400, 250);
//        this.StartPosition = FormStartPosition.CenterParent;

//        Label lblServer = new Label() { Text = "Select Server:", Left = 30, Top = 30, Width = 120 };
//        cmbServer = new ComboBox() { Left = 160, Top = 30, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
//        cmbServer.Items.AddRange(projectList.Keys.ToArray());
//        cmbServer.SelectedIndexChanged += (s, e) =>
//        {
//            cmbProject.Items.Clear();
//            if (cmbServer.SelectedItem == null) return;
//            var serverName = cmbServer.SelectedItem.ToString();
//            foreach (var proj in projectList[serverName])
//                cmbProject.Items.Add(proj.ProjectName);
//        };

//        Label lblProject = new Label() { Text = "Select Project:", Left = 30, Top = 80, Width = 120 };
//        cmbProject = new ComboBox() { Left = 160, Top = 80, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

//        btnDelete = new Button() { Text = "Delete", Left = 160, Top = 130, Width = 100 };
//        btnDelete.Click += (s, e) =>
//        {
//            if (cmbServer.SelectedItem == null || cmbProject.SelectedItem == null)
//            {
//                MessageBox.Show("Please select a project to delete.", "Error");
//                return;
//            }

//            string serverName = cmbServer.SelectedItem.ToString();
//            string projectName = cmbProject.SelectedItem.ToString();

//            var projList = projectList[serverName];
//            projList.RemoveAll(p => p.ProjectName == projectName);
//            MessageBox.Show($"Project '{projectName}' deleted from server '{serverName}'.", "Deleted");

//            cmbProject.Items.Remove(projectName);
//        };

//        Controls.AddRange(new Control[] { lblServer, cmbServer, lblProject, cmbProject, btnDelete });
//    }
//}
//public class ModifyProjectForm : Form
//{
//    private ComboBox cmbServer, cmbProject;
//    private TextBox txtNewName, txtNewComponent;
//    private Button btnUpdate;
//    private Dictionary<string, List<(string ProjectName, string Component)>> projectList;

//    public ModifyProjectForm(Dictionary<string, List<(string ProjectName, string Component)>> projects)
//    {
//        projectList = projects;
//        this.Text = "Modify Project";
//        this.Size = new Size(400, 300);
//        this.StartPosition = FormStartPosition.CenterParent;

//        Label lblServer = new Label() { Text = "Select Server:", Left = 30, Top = 30, Width = 120 };
//        cmbServer = new ComboBox() { Left = 160, Top = 30, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
//        cmbServer.Items.AddRange(projectList.Keys.ToArray());
//        cmbServer.SelectedIndexChanged += (s, e) =>
//        {
//            cmbProject.Items.Clear();
//            if (cmbServer.SelectedItem == null) return;
//            var serverName = cmbServer.SelectedItem.ToString();
//            foreach (var proj in projectList[serverName])
//                cmbProject.Items.Add(proj.ProjectName);
//        };

//        Label lblProject = new Label() { Text = "Select Project:", Left = 30, Top = 70, Width = 120 };
//        cmbProject = new ComboBox() { Left = 160, Top = 70, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

//        Label lblNewName = new Label() { Text = "New Name:", Left = 30, Top = 110, Width = 120 };
//        txtNewName = new TextBox() { Left = 160, Top = 110, Width = 200 };

//        Label lblNewComponent = new Label() { Text = "New Component:", Left = 30, Top = 150, Width = 120 };
//        txtNewComponent = new TextBox() { Left = 160, Top = 150, Width = 200 };

//        btnUpdate = new Button() { Text = "Update", Left = 160, Top = 200, Width = 100 };
//        btnUpdate.Click += (s, e) =>
//        {
//            if (cmbServer.SelectedItem == null || cmbProject.SelectedItem == null)
//            {
//                MessageBox.Show("Please select server and project.", "Error");
//                return;
//            }

//            string serverName = cmbServer.SelectedItem.ToString();
//            string projectName = cmbProject.SelectedItem.ToString();

//            // Renamed variable to avoid conflict
//            var projectEntries = projectList[serverName];
//            int index = projectEntries.FindIndex(p => p.ProjectName == projectName);

//            if (index >= 0)
//            {
//                var newName = string.IsNullOrWhiteSpace(txtNewName.Text) ? projectName : txtNewName.Text;
//                var newComp = string.IsNullOrWhiteSpace(txtNewComponent.Text) ? projectEntries[index].Component : txtNewComponent.Text;

//                projectEntries[index] = (newName, newComp);
//                MessageBox.Show("Project updated successfully!", "Success");
//            }
//        };

//        Controls.AddRange(new Control[] { lblServer, cmbServer, lblProject, cmbProject, lblNewName, txtNewName, lblNewComponent, txtNewComponent, btnUpdate });
//    }
//}


////                foreach (var user in allUsers)
////                {
////                    string version = "";
////                    string change = "";
////                    try
////                    {
////                        string workingDirectory = @"C:\Program Files\Siemens\Automation\Project Server\Bin";
////                        string command = "pspt";
////                        string arguments = "checkin show --server " + user.ServerUrl + " --project " + user.ProjectName;

////                        ProcessStartInfo psi = new ProcessStartInfo
////                        {
////                            FileName = "cmd.exe",
////                            Arguments = $"/c {command} {arguments}",
////                            WorkingDirectory = workingDirectory,
////                            RedirectStandardOutput = true,
////                            RedirectStandardError = true,
////                            UseShellExecute = false,
////                            CreateNoWindow = true
////                        };

////                        using (Process process = new Process())
////                        {
////                            process.StartInfo = psi;
////                            process.Start();
////                            string output = process.StandardOutput.ReadToEnd();
////                            string error = process.StandardError.ReadToEnd();

////                            process.WaitForExit();

////                            Console.WriteLine("----- OUTPUT -----");
////                            Console.WriteLine(output);

////                            if (!string.IsNullOrEmpty(error))
////                            {
////                                Console.WriteLine("----- ERROR -----");
////                                Console.WriteLine(error);
////                            }
////                            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
////                          .Select(l => l.TrimEnd())
////                          .ToList();

////                            var dataLines = lines
////                                .Where(line => !line.StartsWith("_") && !line.StartsWith("Workspace") && !string.IsNullOrWhiteSpace(line))
////                                .ToList();

////                            var datalineLast = dataLines.Last();
////                            string[] parts = datalineLast.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

////                            string value = parts[1];
////                            version = value;
////                            change = parts[4];
////                        }

////                        try
////                        {
////                            // Siemens Project Server Bin directory
////                            string workingDir = @"C:\Program Files\Siemens\Automation\Project Server\Bin";

////                            // Path to export project
////                            string exportPath = user.DBFolder;

////                            // Command
////                            string command1 = $"pspt project export --server {user.ServerUrl} --project {user.ProjectName} --path \"{exportPath}\"";

////                            // Full command chain: change directory and then run your command
////                            string cmdArgs = $"/K cd /d \"{workingDir}\" && {command1}";

////                            // Create process
////                            ProcessStartInfo psi1 = new ProcessStartInfo
////                            {
////                                FileName = "cmd.exe",
////                                Arguments = cmdArgs,
////                                //WorkingDirectory = workingDir,
////                                //RedirectStandardInput = true,
////                                //RedirectStandardOutput = true,
////                                //RedirectStandardError = true,
////                                UseShellExecute = true, // flase
////                                Verb = "runas" // optional: run as admin
////                                               //CreateNoWindow = false, // show console window
////                            };
////                            Thread.Sleep(200);
////                            using (Process process = new Process())
////                            {
////                                process.StartInfo = psi1;
////                                process.Start();
////                                Thread.Sleep(10000);
////                                try
////                                {
////                                    // Send commands to CMD
////                                    using (StreamWriter sw = process.StandardInput)
////                                    {
////                                        if (sw.BaseStream.CanWrite)
////                                        {
////                                            // Change directory (safety)
////                                            sw.WriteLine($"cd /d \"{workingDir}\"");

////                                            // Run the export command
////                                            sw.WriteLine(command1);

////                                            // Wait briefly, then auto-confirm 'y'
////                                            System.Threading.Thread.Sleep(2000);
////                                            sw.WriteLine("y");

////                                            // Optional: exit CMD when done
////                                            sw.WriteLine("exit");
////                                        }
////                                    }
////                                }
////                                catch
////                                {

////                                }
////                                // Show command output (optional)
////                                string output = process.StandardOutput.ReadToEnd();
////                                string error = process.StandardError.ReadToEnd();
////                                process.WaitForExit();

////                                Console.WriteLine("=== Output ===");
////                                Console.WriteLine(output);

////                                if (!string.IsNullOrEmpty(error))
////                                {
////                                    Console.WriteLine("=== Error ===");
////                                    Console.WriteLine(error);
////                                }
////                            }
////                        }
////                        catch (System.Exception ex)
////                        {
////                            Console.WriteLine("Error launching CMD: " + ex.Message);
////                        }

////                    }
////                    catch (System.Exception ex)
////                    {
////                        Console.WriteLine($"Exception: {ex.Message}");
////                    }
////                    Thread.Sleep(10000);
////                    string fullPath = Path.Combine(user.DBFolder, user.ProjectName);

////                    if (Directory.Exists(fullPath))
////                    {
////                        //C:\Program Files (x86)\vdogClient\VDogAutoCheckIn.exe /at:c "/rd:{vdClientArchive}" [/account:{User}] [/domain:{domain}] [/password:{password}] /CFile:d:{configuration file}
////                        string component = user.ComponentForCheckIn;
////                        component = component.TrimStart('\\', '/');

////                        // Combine properly
////                        string localOctoplantPath = Path.Combine(@"C:\vdClientArchive", component);
////                        try
////                        {
////                            CopyDirectoryIncludingRoot(fullPath, localOctoplantPath);
////                        }
////                        catch
////                        {

////                        }
////                        Thread.Sleep(10000);
////                        try
////                        {
////                            string comment = "The Imported revision was " +version+ " from server " + user.ServerUrl +" and project"+ user.ProjectName +".";
////                            string changes = "";
////                            if (change!="")
////                            {
////                                changes = "Revision " + version +" : "+change;
////                            }
////                            else
////                            {
////                                changes = "Revision " + version;
////                            }

////                            string iniFile = $@"[User]
////Account={user.OctoplantUser}
////Domain={user.OctoplantDomain}
////Password={user.OctoplantPassword}
////[Component]
////Dir={user.ComponentForCheckIn}
////[Version]
////Enabled=Y
////WithoutComparison=N
////Comment={comment}
////ChangeReason={changes}";


////                            string currentDir = Directory.GetCurrentDirectory();
////                            string iniPath = Path.Combine(currentDir, "AutoCheckIn.ini");

////                            File.WriteAllText(iniPath, string.Empty);

////                            File.WriteAllText(iniPath, iniFile);

////                            using (Process process = new Process())
////                            {
////                                process.StartInfo.FileName = @"C:\Program Files (x86)\vdogClient\VDogAutoCheckIn.exe";
////                                process.StartInfo.Arguments = "/at:C /rd:C:\\vdClientArchive /CFile:" + iniPath;

////                                process.StartInfo.UseShellExecute = false;
////                                process.StartInfo.RedirectStandardOutput = true;
////                                process.StartInfo.RedirectStandardError = true;
////                                process.StartInfo.CreateNoWindow = true;

////                                process.Start();
////                                Thread.Sleep(10000);

////                                string output = process.StandardOutput.ReadToEnd();
////                                string error = process.StandardError.ReadToEnd();
////                                process.WaitForExit();

////                                if (!string.IsNullOrEmpty(error))
////                                {
////                                    MessageBox.Show("Error: " + error, "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

////                                    string checkInFolder = Path.Combine(currentDir, "CheckIn_Error");

////                                    if (!Directory.Exists(checkInFolder))
////                                        Directory.CreateDirectory(checkInFolder);

////                                    // Generate file name with timestamp
////                                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
////                                    string errorFileName = $"Error_{timestamp}.txt";

////                                    // Full file path
////                                    string errorFilePath = Path.Combine(checkInFolder, errorFileName);

////                                    // Write error details into file
////                                    File.WriteAllText(errorFilePath, error);

////                                    // Optional: show path in console/log
////                                    Console.WriteLine($"Error log saved: {errorFilePath}");
////                                }
////                                MessageBox.Show("Execution successful!\n" + output, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
////                                string folderPath = fullPath;

////                                if (Directory.Exists(folderPath))
////                                {
////                                    Directory.Delete(folderPath, true);
////                                    Console.WriteLine($"Folder deleted successfully: {folderPath}");
////                                }
////                                else
////                                {
////                                    Console.WriteLine("Folder not found: " + folderPath);
////                                }
////                            }
////                        }
////                        catch (System.Exception ex)
////                        {
////                            MessageBox.Show("Error executing command: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
////                        }
////                    }
////                }



////public class AddServerForm : Form
////{
////    private TextBox txtName, txtUrl;
////    private Button btnAdd;
////    private Dictionary<string, string> servers;

////    public AddServerForm(List<GeneralConfig> generalConfigs,Dictionary<string, string> serverList)
////    {
////        servers = serverList;
////        this.Text = "Add Server";
////        this.Size = new Size(400, 230);
////        this.StartPosition = FormStartPosition.CenterParent;

////        Label lblName = new Label() { Text = "Server Name:", Left = 30, Top = 30, Width = 100 };
////        Label lblUrl = new Label() { Text = "Server URL:", Left = 30, Top = 80, Width = 100 };

////        txtName = new TextBox() { Left = 140, Top = 30, Width = 200 };
////        txtUrl = new TextBox() { Left = 140, Top = 80, Width = 200 };

////        btnAdd = new Button() { Text = "Add", Left = 140, Top = 130, Width = 100 };
////        btnAdd.Click += (s, e) =>
////        {
////            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtUrl.Text))
////            {
////                MessageBox.Show("Please enter both Name and URL.");
////                return;
////            }

////            if (servers.ContainsKey(txtName.Text))
////            {
////                MessageBox.Show("Server name already exists!");
////                return;
////            }

////            servers[txtName.Text] = txtUrl.Text;
////            MessageBox.Show("Server added successfully!");
////            this.Close();
////        };

////        Controls.AddRange(new Control[] { lblName, txtName, lblUrl, txtUrl, btnAdd });
////    }
////}
////public class AddProjectForm : Form
////{
////    private ComboBox cmbServer;
////    private TextBox txtProjectName, txtComponent;
////    private Button btnOK, btnCancel;

////    private Dictionary<string, string> servers;
////    private Dictionary<string, List<(string ProjectName, string Component)>> project;
////    public string SelectedServer { get; private set; }
////    public string ProjectName { get; private set; }
////    public string Component { get; private set; }

////    public AddProjectForm(Dictionary<string, string> serverList, Dictionary<string, List<(string ProjectName, string Component)>> projectList, List<GeneralConfig> generalConfigs)
////    {
////        servers = serverList;
////        project = projectList;

////        this.Text = "Append Project";
////        this.Size = new Size(360, 280);
////        this.StartPosition = FormStartPosition.CenterParent;
////        this.FormBorderStyle = FormBorderStyle.FixedDialog;
////        this.MaximizeBox = false;

////        Label lblChoose = new Label()
////        {
////            Text = "Choose a server to add a project:",
////            Left = 20,
////            Top = 20,
////            Width = 300
////        };

////        cmbServer = new ComboBox()
////        {
////            Left = 20,
////            Top = 50,
////            Width = 300,
////            DropDownStyle = ComboBoxStyle.DropDownList
////        };

////        if (servers.Count > 0)
////            cmbServer.Items.AddRange(new List<string>(servers.Keys).ToArray());
////        else
////            cmbServer.Items.Add("(No servers available)");

////        Label lblProj = new Label()
////        {
////            Text = "Project Name:",
////            Left = 20,
////            Top = 100,
////            Width = 120
////        };

////        txtProjectName = new TextBox()
////        {
////            Left = 140,
////            Top = 100,
////            Width = 180
////        };

////        Label lblComp = new Label()
////        {
////            Text = "Component:",
////            Left = 20,
////            Top = 140,
////            Width = 120
////        };

////        txtComponent = new TextBox()
////        {
////            Left = 140,
////            Top = 140,
////            Width = 180
////        };

////        btnCancel = new Button()
////        {
////            Text = "Cancel",
////            Left = 60,
////            Top = 190,
////            Width = 100
////        };
////        btnCancel.Click += (s, e) => this.Close();

////        btnOK = new Button()
////        {
////            Text = "OK",
////            Left = 180,
////            Top = 190,
////            Width = 100,
////            Enabled = false
////        };
////        btnOK.Click += BtnOK_Click;

////        // Enable OK only when valid data is entered
////        txtProjectName.TextChanged += ValidateInput;
////        txtComponent.TextChanged += ValidateInput;
////        cmbServer.SelectedIndexChanged += ValidateInput;

////        Controls.AddRange(new Control[]
////        {
////            lblChoose, cmbServer,
////            lblProj, txtProjectName,
////            lblComp, txtComponent,
////            btnCancel, btnOK
////        });
////    }

////    private void ValidateInput(object sender, EventArgs e)
////    {
////        btnOK.Enabled = (cmbServer.SelectedIndex >= 0 &&
////                         !string.IsNullOrWhiteSpace(txtProjectName.Text) &&
////                         !string.IsNullOrWhiteSpace(txtComponent.Text));
////    }

////    private void BtnOK_Click(object sender, EventArgs e)
////    {
////        SelectedServer = cmbServer.SelectedItem?.ToString();
////        ProjectName = txtProjectName.Text;
////        Component = txtComponent.Text;

////        foreach (var ser in project.Keys)
////        {
////            if (SelectedServer.Split(',')[0] == ser)
////            {
////                project[ser].Add((ProjectName, Component));
////            }
////        }
////        MessageBox.Show(
////            $"Project '{ProjectName}' added to server '{SelectedServer}' successfully!",
////            "Success",
////            MessageBoxButtons.OK,
////            MessageBoxIcon.Information);



////        this.DialogResult = DialogResult.OK;
////        this.Close();
////    }
////}

//// ============================================================
////  General Configuration Form
//// ============================================================
////public class GeneralConfigForm : Form
////{
////    TextBox txtVersion, txtInterval, txtComponent;
////    TextBox txtProgramFile, txtClientArchive, txtDomain, txtUser, txtPassword, txtDBFolder;
////    Button btnBrowseProgram, btnBrowseArchive, btnBrowseDB;

////    public GeneralConfigData ConfigData { get; private set; }

////    public GeneralConfigForm()
////    {
////        this.Text = "General Configuration";
////        this.Size = new Size(650, 600);
////        this.StartPosition = FormStartPosition.CenterParent;
////        this.AutoScroll = true;

////        int top = 20;
////        int leftLabel = 30;
////        int leftInput = 220;
////        int labelWidth = 180;
////        int inputWidth = 300;
////        int gap = 40;

////        // ---- Version ----
////        Label lblVersion = new Label { Text = "Version:", Top = top, Left = leftLabel, Width = labelWidth };
////        txtVersion = new TextBox { Top = top, Left = leftInput, Width = inputWidth, Text = "1" };
////        top += gap;

////        // ---- Interval ----
////        Label lblInterval = new Label { Text = "Interval:", Top = top, Left = leftLabel, Width = labelWidth };
////        txtInterval = new TextBox { Top = top, Left = leftInput, Width = inputWidth, Text = "5" };
////        top += gap;

////        // ---- Component ----
////        Label lblComponent = new Label { Text = "Component for Check-In:", Top = top, Left = leftLabel, Width = labelWidth };
////        txtComponent = new TextBox { Top = top, Left = leftInput, Width = inputWidth };
////        top += gap;

////        // ---- Octoplant Program Folder ----
////        Label lblProgramFile = new Label { Text = "Octoplant Program Folder:", Top = top, Left = leftLabel, Width = labelWidth };
////        txtProgramFile = new TextBox { Top = top, Left = leftInput, Width = inputWidth };
////        btnBrowseProgram = new Button { Text = "...", Top = top, Left = leftInput + inputWidth + 10, Width = 30 };
////        btnBrowseProgram.Click += (s, e) => BrowseFolder(txtProgramFile);
////        top += gap;

////        // ---- Octoplant Client Archive ----
////        Label lblClientArchive = new Label { Text = "Octoplant Client Archive:", Top = top, Left = leftLabel, Width = labelWidth };
////        txtClientArchive = new TextBox { Top = top, Left = leftInput, Width = inputWidth };
////        btnBrowseArchive = new Button { Text = "...", Top = top, Left = leftInput + inputWidth + 10, Width = 30 };
////        btnBrowseArchive.Click += (s, e) => BrowseFolder(txtClientArchive);
////        top += gap;

////        // ---- Octoplant Domain ----
////        Label lblDomain = new Label { Text = "Octoplant Domain:", Top = top, Left = leftLabel, Width = labelWidth };
////        txtDomain = new TextBox { Top = top, Left = leftInput, Width = inputWidth };
////        top += gap;

////        // ---- Octoplant User ----
////        Label lblUser = new Label { Text = "Octoplant User:", Top = top, Left = leftLabel, Width = labelWidth };
////        txtUser = new TextBox { Top = top, Left = leftInput, Width = inputWidth };
////        top += gap;

////        // ---- Octoplant Password ----
////        Label lblPassword = new Label { Text = "Octoplant Password:", Top = top, Left = leftLabel, Width = labelWidth };
////        txtPassword = new TextBox { Top = top, Left = leftInput, Width = inputWidth, UseSystemPasswordChar = true };
////        top += gap;

////        // ---- DB Folder ----
////        Label lblDBFolder = new Label { Text = "DB Folder:", Top = top, Left = leftLabel, Width = labelWidth };
////        txtDBFolder = new TextBox { Top = top, Left = leftInput, Width = inputWidth };
////        btnBrowseDB = new Button { Text = "...", Top = top, Left = leftInput + inputWidth + 10, Width = 30 };
////        btnBrowseDB.Click += (s, e) => BrowseFolder(txtDBFolder);
////        top += gap;

////        // ---- Buttons ----
////        Button btnOk = new Button() { Text = "OK", Top = top + 20, Left = leftInput - 40, Width = 100 };
////        Button btnCancel = new Button() { Text = "Cancel", Top = top + 20, Left = leftInput + 80, Width = 100 };

////        btnOk.Click += BtnOk_Click;
////        btnCancel.Click += (s, e) => this.Close();

////        this.Controls.AddRange(new Control[]
////        {
////            lblVersion, txtVersion,
////            lblInterval, txtInterval,
////            lblComponent, txtComponent,
////            lblProgramFile, txtProgramFile, btnBrowseProgram,
////            lblClientArchive, txtClientArchive, btnBrowseArchive,
////            lblDomain, txtDomain,
////            lblUser, txtUser,
////            lblPassword, txtPassword,
////            lblDBFolder, txtDBFolder, btnBrowseDB,
////            btnOk, btnCancel
////        });
////    }

////    private void BtnOk_Click(object sender, EventArgs e)
////    {
////        ConfigData = new GeneralConfigData
////        {
////            Version = txtVersion.Text,
////            Interval = txtInterval.Text,
////            Component = txtComponent.Text,
////            ProgramFile = txtProgramFile.Text,
////            ClientArchive = txtClientArchive.Text,
////            Domain = txtDomain.Text,
////            User = txtUser.Text,
////            Password = txtPassword.Text,
////            DBFolder = txtDBFolder.Text
////        };

////        this.DialogResult = DialogResult.OK;
////        this.Close();
////    }

////    public void LoadExistingConfig(GeneralConfigData data)
////    {
////        if (data == null) return;
////        txtVersion.Text = data.Version;
////        txtInterval.Text = data.Interval;
////        txtComponent.Text = data.Component;
////        txtProgramFile.Text = data.ProgramFile;
////        txtClientArchive.Text = data.ClientArchive;
////        txtDomain.Text = data.Domain;
////        txtUser.Text = data.User;
////        txtPassword.Text = data.Password;
////        txtDBFolder.Text = data.DBFolder;
////    }

////    private void BrowseFolder(TextBox target)
////    {
////        using (var fbd = new FolderBrowserDialog())
////        {
////            if (fbd.ShowDialog() == DialogResult.OK)
////                target.Text = fbd.SelectedPath;
////        }
////    }
////}

//// ============================================================
////  Config Data Class
//// ============================================================

////public class AddServerForm : Form
////{
////    private ComboBox cmbOctoplantUser;
////    private TextBox txtServerName, txtServerUrl;
////    private Button btnOK, btnCancel;

////    private Dictionary<string, string> servers;
////    private Dictionary<string, List<(string ProjectName, string Component)>> project;
////    private List<GeneralConfig> generalConfigs;

////    public string SelectedUser { get; private set; }
////    public string ServerName { get; private set; }
////    public string ServerUrl { get; private set; }
////    //Dictionary<string, List<(string ProjectName, string Component)>> project = new Dictionary<string, List<(string, string)>>();
////    public AddServerForm(List<GeneralConfig> generalConfigs, Dictionary<string, string> serverList, Dictionary<string, List<(string ProjectName, string Component)>> projectList)
////    {
////        this.generalConfigs = generalConfigs ?? new List<GeneralConfig>();
////        this.servers = serverList ?? new Dictionary<string, string>();
////        this.project = projectList ?? new Dictionary<string, List<(string ProjectName, string Component)>>();

////        this.Text = "Add Server";
////        this.Size = new Size(380, 300);
////        this.StartPosition = FormStartPosition.CenterParent;
////        this.FormBorderStyle = FormBorderStyle.FixedDialog;
////        this.MaximizeBox = false;

////        // --- Octoplant User Selection ---
////        Label lblUser = new Label()
////        {
////            Text = "Select Octoplant User:",
////            Left = 20,
////            Top = 20,
////            Width = 200
////        };

////        cmbOctoplantUser = new ComboBox()
////        {
////            Left = 20,
////            Top = 45,
////            Width = 320,
////            DropDownStyle = ComboBoxStyle.DropDownList
////        };

////        if (generalConfigs.Count > 0)
////            cmbOctoplantUser.Items.AddRange(generalConfigs.Select(x => x.OctoplantUser).ToArray());
////        else
////            cmbOctoplantUser.Items.Add("(No users available)");

////        // --- Server Name ---
////        Label lblServerName = new Label()
////        {
////            Text = "Server Name:",
////            Left = 20,
////            Top = 90,
////            Width = 120
////        };

////        txtServerName = new TextBox()
////        {
////            Left = 150,
////            Top = 85,
////            Width = 190
////        };

////        // --- Server URL ---
////        Label lblServerUrl = new Label()
////        {
////            Text = "Server URL:",
////            Left = 20,
////            Top = 130,
////            Width = 120
////        };

////        txtServerUrl = new TextBox()
////        {
////            Left = 150,
////            Top = 125,
////            Width = 190
////        };

////        // --- Cancel Button ---
////        btnCancel = new Button()
////        {
////            Text = "Cancel",
////            Left = 70,
////            Top = 190,
////            Width = 100
////        };
////        btnCancel.Click += (s, e) => this.Close();

////        // --- OK Button ---
////        btnOK = new Button()
////        {
////            Text = "Add Server",
////            Left = 200,
////            Top = 190,
////            Width = 100,
////            Enabled = false
////        };
////        btnOK.Click += BtnOK_Click;

////        // --- Validation Events ---
////        txtServerName.TextChanged += ValidateInput;
////        txtServerUrl.TextChanged += ValidateInput;
////        cmbOctoplantUser.SelectedIndexChanged += ValidateInput;

////        Controls.AddRange(new Control[]
////        {
////            lblUser, cmbOctoplantUser,
////            lblServerName, txtServerName,
////            lblServerUrl, txtServerUrl,
////            btnCancel, btnOK
////        });
////    }

////    private void ValidateInput(object sender, EventArgs e)
////    {
////        btnOK.Enabled = (cmbOctoplantUser.SelectedIndex >= 0 &&
////                         !string.IsNullOrWhiteSpace(txtServerName.Text) &&
////                         !string.IsNullOrWhiteSpace(txtServerUrl.Text));
////    }

////    private void BtnOK_Click(object sender, EventArgs e)
////    {
////        SelectedUser = cmbOctoplantUser.SelectedItem?.ToString();
////        ServerName = txtServerName.Text.Trim();
////        ServerUrl = txtServerUrl.Text.Trim();

////        if (servers.ContainsKey(ServerName))
////        {
////            MessageBox.Show("Server name already exists!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
////            return;
////        }

////        servers[ServerName + "," + SelectedUser] = ServerUrl;
////        project[ServerName] = new List<(string ProjectName, string Component)>();

////        MessageBox.Show(
////            $"Server '{ServerName}' added for Octoplant user '{SelectedUser}' successfully!",
////            "Success",
////            MessageBoxButtons.OK,
////            MessageBoxIcon.Information);

////        this.DialogResult = DialogResult.OK;
////        this.Close();
////    }
////}
////public class AddProjectForm : Form
////{
////    private ComboBox cmbUser, cmbServer;
////    private TextBox txtProjectName, txtComponent;
////    private Button btnOK, btnCancel;

////    private Dictionary<string, string> servers;
////    private Dictionary<string, List<(string ProjectName, string Component)>> project;
////    private List<GeneralConfig> generalConfigs;

////    public string SelectedUser { get; private set; }
////    public string SelectedServer { get; private set; }
////    public string ProjectName { get; private set; }
////    public string Component { get; private set; }

////    public AddProjectForm(Dictionary<string, string> serverList,
////                          Dictionary<string, List<(string ProjectName, string Component)>> projectList,
////                          List<GeneralConfig> generalConfigs)
////    {
////        this.servers = serverList ?? new Dictionary<string, string>();
////        this.project = projectList ?? new Dictionary<string, List<(string ProjectName, string Component)>>();
////        this.generalConfigs = generalConfigs ?? new List<GeneralConfig>();

////        this.Text = "Append Project";
////        this.Size = new Size(400, 320);
////        this.StartPosition = FormStartPosition.CenterParent;
////        this.FormBorderStyle = FormBorderStyle.FixedDialog;
////        this.MaximizeBox = false;

////        // --- User Dropdown ---
////        Label lblUser = new Label()
////        {
////            Text = "Select Octoplant User:",
////            Left = 20,
////            Top = 20,
////            Width = 150
////        };

////        cmbUser = new ComboBox()
////        {
////            Left = 180,
////            Top = 20,
////            Width = 180,
////            DropDownStyle = ComboBoxStyle.DropDownList
////        };

////        if (generalConfigs.Count > 0)
////            cmbUser.Items.AddRange(generalConfigs.Select(x => x.OctoplantUser).ToArray());
////        else
////            cmbUser.Items.Add("(No users available)");

////        cmbUser.SelectedIndexChanged += CmbUser_SelectedIndexChanged;

////        // --- Server Dropdown ---
////        Label lblServer = new Label()
////        {
////            Text = "Select Server:",
////            Left = 20,
////            Top = 70,
////            Width = 150
////        };

////        cmbServer = new ComboBox()
////        {
////            Left = 180,
////            Top = 70,
////            Width = 180,
////            DropDownStyle = ComboBoxStyle.DropDownList
////        };

////        // --- Project Name ---
////        Label lblProj = new Label()
////        {
////            Text = "Project Name:",
////            Left = 20,
////            Top = 120,
////            Width = 120
////        };

////        txtProjectName = new TextBox()
////        {
////            Left = 180,
////            Top = 120,
////            Width = 180
////        };

////        // --- Component ---
////        Label lblComp = new Label()
////        {
////            Text = "Component:",
////            Left = 20,
////            Top = 160,
////            Width = 120
////        };

////        txtComponent = new TextBox()
////        {
////            Left = 180,
////            Top = 160,
////            Width = 180
////        };

////        // --- Cancel ---
////        btnCancel = new Button()
////        {
////            Text = "Cancel",
////            Left = 80,
////            Top = 220,
////            Width = 100
////        };
////        btnCancel.Click += (s, e) => this.Close();

////        // --- OK ---
////        btnOK = new Button()
////        {
////            Text = "OK",
////            Left = 200,
////            Top = 220,
////            Width = 100,
////            Enabled = false
////        };
////        btnOK.Click += BtnOK_Click;

////        txtProjectName.TextChanged += ValidateInput;
////        txtComponent.TextChanged += ValidateInput;
////        cmbServer.SelectedIndexChanged += ValidateInput;

////        Controls.AddRange(new Control[]
////        {
////            lblUser, cmbUser,
////            lblServer, cmbServer,
////            lblProj, txtProjectName,
////            lblComp, txtComponent,
////            btnCancel, btnOK
////        });
////    }

////    private void CmbUser_SelectedIndexChanged(object sender, EventArgs e)
////    {
////        cmbServer.Items.Clear();

////        SelectedUser = cmbUser.SelectedItem?.ToString();
////        if (string.IsNullOrEmpty(SelectedUser)) return;

////        // Filter servers for selected user
////        var userServers = servers
////            .Where(s => s.Key.Split(',').Length > 1 && s.Key.Split(',')[1] == SelectedUser)
////            .ToDictionary(s => s.Key, s => s.Value);

////        if (userServers.Count > 0)
////            cmbServer.Items.AddRange(userServers.Keys.ToArray());
////        else
////            cmbServer.Items.Add("(No servers for this user)");
////    }

////    private void ValidateInput(object sender, EventArgs e)
////    {
////        btnOK.Enabled = (cmbServer.SelectedIndex >= 0 &&
////                         !string.IsNullOrWhiteSpace(txtProjectName.Text) &&
////                         !string.IsNullOrWhiteSpace(txtComponent.Text));
////    }

////    private void BtnOK_Click(object sender, EventArgs e)
////    {
////        SelectedServer = cmbServer.SelectedItem?.ToString();
////        ProjectName = txtProjectName.Text.Trim();
////        Component = txtComponent.Text.Trim();

////        if (string.IsNullOrEmpty(SelectedServer) || string.IsNullOrEmpty(ProjectName))
////        {
////            MessageBox.Show("Please fill all details.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
////            return;
////        }

////        // Ensure project list entry exists
////        if (!project.ContainsKey(SelectedServer.Split(',')[0]))
////            project[SelectedServer.Split(',')[0]] = new List<(string ProjectName, string Component)>();

////        // Add project entry
////        project[SelectedServer.Split(',')[0]].Add((ProjectName, Component));

////        MessageBox.Show(
////            $"Project '{ProjectName}' added to server '{SelectedServer}' successfully!",
////            "Success",
////            MessageBoxButtons.OK,
////            MessageBoxIcon.Information);

////        this.DialogResult = DialogResult.OK;
////        this.Close();
////    }
////}

////public class MainForm : Form
////{
////    private Panel leftPanel;
////    private Panel mainPanel;
////    private Button btnGeneralConfig;
////    private Button btnGetConfig;
////    private Button btnAddServer;
////    private Button btnModifyServer;
////    private Button btnDeleteServer;
////    private Button btnAddProject;
////    private TextBox txtOutput;

////    // In-memory server list
////    private static Dictionary<string, string> serverList = new Dictionary<string, string>(); 
////    private static Dictionary<string, List<(string ProjectName, string Component)>> projectList = new Dictionary<string, List<(string, string)>>();
////    private static List<GeneralConfig> generalConfigs = new List<GeneralConfig>();
////    public MainForm()
////    {
////        this.Text = "TIA Portal Project Server Importer Configurator";
////        this.Size = new Size(1000, 600);
////        this.StartPosition = FormStartPosition.CenterScreen;

////        // Left panel
////        leftPanel = new Panel()
////        {
////            Dock = DockStyle.Left,
////            Width = 200,
////            BackColor = Color.LightGray
////        };

////        // Main area
////        mainPanel = new Panel()
////        {
////            Dock = DockStyle.Fill,
////            BackColor = Color.White
////        };

////        // --- Buttons on the left ---
////        btnGetConfig = new Button()
////        {
////            Text = "Get Config",
////            Width = 160,
////            Height = 35,
////            Top = 20,
////            Left = 20
////        };
////        btnGetConfig.Click += BtnGetConfig_Click;

////        btnGeneralConfig = new Button()
////        {
////            Text = "General Config",
////            Width = 160,
////            Height = 35,
////            Top = 70,
////            Left = 20
////        };
////        btnGeneralConfig.Click += (s, e) =>
////        {
////            using (var form = new GeneralConfigForm())
////            {
////                if (form.ShowDialog() == DialogResult.OK)
////                {
////                    var cfg = new GeneralConfig
////                    {
////                        Version = form.Version ?? "",
////                        Interval = form.Interval ?? "",
////                        OctoplantUser = form.OctoplantUser ?? "",
////                        OctoplantPassword = form.OctoplantPassword ?? "",
////                    };
////                    generalConfigs.Add(cfg);
////                    MessageBox.Show($"General Config added for user '{cfg.OctoplantUser}'");
////                }
////            }
////        };

////        btnAddServer = new Button()
////        {
////            Text = "Add Server",
////            Width = 160,
////            Height = 35,
////            Top = 120,
////            Left = 20
////        };
////        btnAddServer.Click += (s, e) => new AddServerForm(generalConfigs,serverList,projectList).ShowDialog();

////        btnModifyServer = new Button()
////        {
////            Text = "Modify Server",
////            Width = 160,
////            Height = 35,
////            Top = 170,
////            Left = 20
////        };
////        btnModifyServer.Click += (s, e) => new ModifyServerForm(serverList).ShowDialog();

////        btnDeleteServer = new Button()
////        {
////            Text = "Delete Server",
////            Width = 160,
////            Height = 35,
////            Top = 220,
////            Left = 20
////        };
////        btnDeleteServer.Click += (s, e) => new DeleteServerForm(serverList).ShowDialog();

////        btnAddProject = new Button()
////        {
////            Text = "Add Project",
////            Width = 160,
////            Height = 35,
////            Top = 270,
////            Left = 20
////        };
////        btnAddProject.Click += (s, e) => new AddProjectForm(serverList,projectList,generalConfigs).ShowDialog();

////        // --- Output textbox ---
////        txtOutput = new TextBox()
////        {
////            Multiline = true,
////            Dock = DockStyle.Fill,
////            ScrollBars = ScrollBars.Vertical,
////            ReadOnly = true,
////            Font = new Font("Consolas", 11)
////        };

////        // Add controls
////        leftPanel.Controls.AddRange(new Control[]
////        {
////            btnGetConfig,
////            btnGeneralConfig,
////            btnAddServer,
////            btnModifyServer,
////            btnDeleteServer,
////            btnAddProject
////        });

////        mainPanel.Controls.Add(txtOutput);
////        this.Controls.Add(mainPanel);
////        this.Controls.Add(leftPanel);
////    }





////class TiaPortalAutomation
////{
////    static System.Timers.Timer timer;
////    static List<OctoplantUserData> allUsers;

////    static string currentDir = Directory.GetCurrentDirectory();
////    static string logData = Path.Combine(currentDir, "LogData.txt");
////    // Local file for persistence
////    private static string configFile = Path.Combine(Directory.GetCurrentDirectory(), "ConfigData.txt");

////    // Encrypt string (AES)
////    public static string EncryptString(string plainText)
////    {
////        using (Aes aes = Aes.Create())
////        {
////            aes.Key = Encoding.UTF8.GetBytes(key);
////            aes.IV = new byte[16];
////            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
////            using (var ms = new MemoryStream())
////            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
////            using (var sw = new StreamWriter(cs))
////            {
////                sw.Write(plainText);
////                sw.Close();
////                return Convert.ToBase64String(ms.ToArray());
////            }
////        }
////    }

////    // Decrypt string
////    public static string DecryptString(string cipherText)
////    {
////        try
////        {
////            using (Aes aes = Aes.Create())
////            {
////                aes.Key = Encoding.UTF8.GetBytes(key);
////                aes.IV = new byte[16];
////                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
////                using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
////                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
////                using (var sr = new StreamReader(cs))
////                {
////                    return sr.ReadToEnd();
////                }
////            }
////        }
////        catch { return cipherText; }
////    }

////    // Save all user/server/project info to text file
////    public static void SaveAllData(List<OctoplantUserData> allUsers)
////    {
////        StringBuilder sb = new StringBuilder();

////        foreach (var user in allUsers)
////        {
////            sb.AppendLine($"USER|{user.ServerUrl}|{user.ProjectName}|{user.ComponentForCheckIn}|{user.Interval}|{user.OctoplantProgramFiles}|{user.OctoplantClientArchive}|{user.OctoplantDomain}|{user.OctoplantUser}|{EncryptString(user.OctoplantPassword)}|{user.DBFolder}|{user.Compare}|{user.ReleaseAfterCheckIn}");
////        }

////        File.WriteAllText(configFile, sb.ToString());
////        Console.WriteLine("All user data saved to ConfigData.txt");
////    }

////    // Load data from file
////    public static List<OctoplantUserData> LoadAllData()
////    {
////        var list = new List<OctoplantUserData>();
////        if (!File.Exists(configFile)) return list;

////        foreach (var line in File.ReadAllLines(configFile))
////        {
////            if (string.IsNullOrWhiteSpace(line)) continue;
////            var parts = line.Split('|');
////            if (parts.Length >= 12)
////            {
////                list.Add(new OctoplantUserData
////                {
////                    ServerUrl = parts[1],
////                    ProjectName = parts[2],
////                    ComponentForCheckIn = parts[3],
////                    Interval = parts[4],
////                    OctoplantProgramFiles = parts[5],
////                    OctoplantClientArchive = parts[6],
////                    OctoplantDomain = parts[7],
////                    OctoplantUser = parts[8],
////                    OctoplantPassword = DecryptString(parts[9]),
////                    DBFolder = parts[10],
////                    Compare = bool.TryParse(parts[11], out bool cmp) && cmp,
////                    ReleaseAfterCheckIn = parts.Length >= 13 && bool.TryParse(parts[12], out bool rel) && rel
////                });
////            }
////        }
////        Console.WriteLine("All user data loaded from ConfigData.txt");
////        return list;
////    }

////    [STAThread]
////    static void Main(string[] args)
////    {
////        System.Windows.Forms.Application.EnableVisualStyles();
////        System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

////        Application.Run(new MainForm());

////        string userDataFile = Path.Combine(currentDir, "UserData.txt");

////        if (!File.Exists(userDataFile))
////            File.Create(userDataFile);

////        //allUsers = LoadUsersFromTextFile(userDataFile);
////        // Try loading from ConfigData.txt first, fallback to old file if empty
////        allUsers = LoadAllData();
////        if (allUsers.Count == 0)
////            allUsers = LoadUsersFromTextFile(userDataFile);

////        bool restart = false;

////        do
////        {
////            restart = false;
////            using (OctoplantInputForm form = new OctoplantInputForm())
////            {
////                if (form.ShowDialog() == DialogResult.OK)
////                {
////                    if (allUsers.Count() > 0)
////                        allUsers.AddRange(form.UsersData);
////                    else if(allUsers.Count()==0)
////                        allUsers = form.UsersData;

////                    File.WriteAllText(userDataFile, string.Empty);

////                    SaveUsersToTextFile(allUsers, userDataFile);
////                    // NEW: also save encrypted persistent config
////                    SaveAllData(allUsers);

////                    foreach (var user in allUsers)
////                    {
////                        Console.WriteLine($"User: {user.OctoplantUser}");
////                        Console.WriteLine($"Domain: {user.OctoplantDomain}");
////                        Console.WriteLine($"Server: {user.ServerUrl}");
////                        Console.WriteLine($"Compare: {user.Compare}");
////                        Console.WriteLine("--------------------------------");
////                    }

////                    int number = int.Parse(allUsers[0].Interval);
////                    int intervalMinutes = number;
////                    if (int.TryParse(allUsers.FirstOrDefault()?.Interval, out int parsed))
////                        intervalMinutes = parsed;

////                    RunAutomationCycle(allUsers);

////                    System.Timers.Timer timer = new System.Timers.Timer(intervalMinutes * 60 * 1000);
////                    timer.Elapsed += (s, e) =>
////                    {
////                        Console.WriteLine($"Running next cycle at {DateTime.Now}");
////                        RunAutomationCycle(allUsers);
////                    };
////                    timer.AutoReset = true;
////                    timer.Start();

////                    ControlPopupForm popup = new ControlPopupForm();

////                    popup.OnStopClicked += () =>
////                    {
////                        timer.Stop();
////                        timer.Dispose();
////                        MessageBox.Show("Automation stopped.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
////                        Application.Exit();
////                    };

////                    popup.OnStartNewClicked += () =>
////                    {
////                        timer.Stop();
////                        timer.Dispose();
////                        popup.Close();
////                        restart = true;
////                        MessageBox.Show("Restarting automation...", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
////                    };

////                    Application.Run(popup);
////                }

////            }
////        }while(restart);
////    }
////    public static void RunAutomationCycle(List<OctoplantUserData> allUsers)
////    {
////        // Always reload from file to ensure latest
////        allUsers = LoadAllData();

////        if (allUsers.Count == 0)
////        {
////            Console.WriteLine("No users found in local ConfigData.txt.");
////            return;
////        }

////        foreach (var user in allUsers)
////        {
////            Console.WriteLine($"Starting cycle for project: {user.ProjectName} ({DateTime.Now})");
////            //File.AppendAllText(logData, $"[{DateTime.Now}] Running project: {user.ProjectName} on {user.ServerUrl}\n");

////            //Console.WriteLine($"Starting cycle for project: {user.ProjectName} ({DateTime.Now})");

////            //File.AppendAllText(logData, "----------------------------------------------------------------------------------------\n");
////            File.AppendAllText(logData, DateTime.Now +" : Project Name : "+ user.ProjectName + " \n Server URL : " + user.ServerUrl + " \n User Name : "+ user.OctoplantUser + "\n");

////            //user.ServerUrl = "https://nb012026:8735/";
////            string version = "";
////            string change = "";
////            try
////            {
////                string workingDirectory = @"C:\Program Files\Siemens\Automation\Project Server\Bin";

////                string command = "pspt";
////                string arguments = "checkin show --server " + user.ServerUrl + " --project " + user.ProjectName;

////                ProcessStartInfo psi = new ProcessStartInfo
////                {
////                    FileName = "cmd.exe",
////                    Arguments = $"/c {command} {arguments}",
////                    WorkingDirectory = workingDirectory,
////                    RedirectStandardOutput = true,
////                    RedirectStandardError = true,
////                    UseShellExecute = false,
////                    CreateNoWindow = true
////                };

////                using (Process process = new Process())
////                {
////                    process.StartInfo = psi;
////                    process.Start();

////                    string output = process.StandardOutput.ReadToEnd();
////                    string error = process.StandardError.ReadToEnd();

////                    process.WaitForExit();

////                    Console.WriteLine("----- OUTPUT -----");
////                    Console.WriteLine(output);

////                    if (!string.IsNullOrEmpty(error))
////                    {
////                        Console.WriteLine("----- ERROR -----");
////                        Console.WriteLine(error);
////                        File.AppendAllText(logData, DateTime.Now + " : " + error + "\n");
////                    }

////                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
////                    .Select(l => l.TrimEnd())
////                    .ToList();

////                    var dataLines = lines
////                        .Where(line => !line.StartsWith("_") && !line.StartsWith("Workspace") && !string.IsNullOrWhiteSpace(line))
////                        .ToList();

////                    var datalineLast = dataLines.Last();
////                    string[] parts = datalineLast.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

////                    string value = parts[1];
////                    version = value;
////                    change = parts[4];

////                    var lines1 = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

////                    // Find the last line that looks like a data row (starts with spaces and a number)
////                    string lastDataLine = lines1.LastOrDefault(line1 => Regex.IsMatch(line1.TrimStart(), @"^\d+"));

////                    if (lastDataLine != null)
////                    {
////                        // Match the last "Comment" part (everything after the time)
////                        Match match = Regex.Match(lastDataLine, @"\d{2}[:\-]\d{2}[:\-]\d{4}\s+\d{2}:\d{2}:\d{2}\s+(.*)$");

////                        string comment = match.Success ? match.Groups[1].Value.Trim() : "No comment found";
////                        change = comment;
////                    }

////                    File.AppendAllText(logData, DateTime.Now + " : " + datalineLast + "\n");
////                }

////                try
////                {

////                    string fullPath1 = Path.Combine(user.DBFolder, user.ProjectName); 

////                    string workingDir = @"C:\Program Files\Siemens\Automation\Project Server\Bin";

////                    string exportPath = user.DBFolder;

////                    string command1 = $"pspt project export --server {user.ServerUrl} --project {user.ProjectName} --path \"{exportPath}\"";

////                    string cmdArgs = $"/K cd /d \"{workingDir}\" && {command1}";

////                    string folderPath1 = fullPath1;

////                    if (Directory.Exists(folderPath1))
////                    {
////                        Directory.Delete(folderPath1, true);
////                        Console.WriteLine($"Folder deleted successfully: {folderPath1}");
////                    }
////                    else
////                    {
////                        Console.WriteLine("Folder not found: " + folderPath1);
////                    }

////                    ProcessStartInfo psi1 = new ProcessStartInfo
////                    {
////                        FileName = "cmd.exe",
////                        Arguments = cmdArgs,
////                        //WorkingDirectory = workingDir,
////                        //RedirectStandardInput = true,
////                        //RedirectStandardOutput = true,
////                        //RedirectStandardError = true,
////                        UseShellExecute = false,
////                        Verb = "runas", // optional: run as admin
////                        CreateNoWindow = true,       //CreateNoWindow = false, // show console window
////                    };
////                    Thread.Sleep(200);
////                    using (Process process = new Process())
////                    {
////                        process.StartInfo = psi1;
////                        process.Start();
////                        Thread.Sleep(10000);
////                        try
////                        {
////                            using (StreamWriter sw = process.StandardInput)
////                            {
////                                if (sw.BaseStream.CanWrite)
////                                {
////                                    sw.WriteLine($"cd /d \"{workingDir}\"");

////                                    sw.WriteLine(command1);

////                                    System.Threading.Thread.Sleep(2000);
////                                    sw.WriteLine("y");

////                                    sw.WriteLine("exit");
////                                }
////                            }
////                        }
////                        catch
////                        {

////                        }
////                        string output = process.StandardOutput.ReadToEnd();
////                        string error = process.StandardError.ReadToEnd();
////                        process.WaitForExit();

////                        Console.WriteLine("=== Output ===");
////                        Console.WriteLine(output);

////                        if (!string.IsNullOrEmpty(error))
////                        {
////                            Console.WriteLine("=== Error ===");
////                            Console.WriteLine(error);
////                        }
////                    }
////                }
////                catch (System.Exception ex)
////                {
////                    Console.WriteLine("Error launching CMD: " + ex.Message);
////                }
////            }
////            catch (System.Exception ex)
////            {
////                Console.WriteLine($"Exception: {ex.Message}");
////            }
////            Thread.Sleep(10000);
////            string fullPath = Path.Combine(user.DBFolder, user.ProjectName);

////            if (Directory.Exists(fullPath))
////            {
////                //C:\Program Files (x86)\vdogClient\VDogAutoCheckIn.exe /at:c "/rd:{vdClientArchive}" [/account:{User}] [/domain:{domain}] [/password:{password}] /CFile:d:{configuration file}
////                string component = user.ComponentForCheckIn;
////                component = component.TrimStart('\\', '/');

////                string localOctoplantPath = Path.Combine(@"C:\vdClientArchive", component);
////                try
////                {
////                    CopyDirectoryIncludingRoot(fullPath, localOctoplantPath);                    
////                }
////                catch
////                {

////                }
////                Thread.Sleep(10000);
////                try
////                {
////                    string comment = "The Imported revision was " + version + " from server " + user.ServerUrl + " and project " + user.ProjectName + ".";
////                    string changes = "";
////                    if (change != "")
////                    {
////                        changes = "Revision " + version + " : " + change;
////                    }
////                    else
////                    {
////                        changes = "Revision " + version;
////                    }
////                    string plainPassword = Decrypt(user.OctoplantPassword);
////                    string iniFile = $@"[User]
////Account={user.OctoplantUser}
////Domain={user.OctoplantDomain}
////Password={plainPassword}
////[Component]
////Dir={user.ComponentForCheckIn}
////[Version]
////Enabled=Y
////WithoutComparison=N
////Comment={comment}
////ChangeReason={changes}";

////                    string currentDir = Directory.GetCurrentDirectory();
////                    string iniPath = Path.Combine(currentDir, "AutoCheckIn.ini");

////                    File.WriteAllText(iniPath, string.Empty);

////                    File.WriteAllText(iniPath, iniFile);

////                    using (Process process = new Process())
////                    {
////                        process.StartInfo.FileName = @"C:\Program Files (x86)\vdogClient\VDogAutoCheckIn.exe";
////                        process.StartInfo.Arguments = "/at:C /rd:C:\\vdClientArchive /CFile:" + iniPath;

////                        process.StartInfo.UseShellExecute = false;
////                        process.StartInfo.RedirectStandardOutput = true;
////                        process.StartInfo.RedirectStandardError = true;
////                        process.StartInfo.CreateNoWindow = true;

////                        process.Start();
////                        Thread.Sleep(10000);

////                        string output = process.StandardOutput.ReadToEnd();
////                        string error = process.StandardError.ReadToEnd();
////                        process.WaitForExit();

////                        if (!string.IsNullOrEmpty(error))
////                        {
////                            MessageBox.Show("Error: " + error, "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

////                            string checkInFolder = Path.Combine(currentDir, "CheckIn_Error");

////                            if (!Directory.Exists(checkInFolder))
////                                Directory.CreateDirectory(checkInFolder);

////                            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
////                            string errorFileName = $"Error_{timestamp}.txt";

////                            string errorFilePath = Path.Combine(checkInFolder, errorFileName);

////                            File.WriteAllText(errorFilePath, error);
////                            File.AppendAllText(logData, DateTime.Now + " : " + error + "\n");

////                            Console.WriteLine($"Error log saved: {errorFilePath}");
////                        }

////                        //"C:\Program Files (x86)\vdogClient\VDogAutoCheckOut.exe" / RD:"C:\vdClientArchive" / AT:C / DirR:"\DevRnDProj\Proj2024_25\SIMIT_Proj\demoproj" / Account:d03007591 / Domain:wipropari / Password:Gayatri@1834 / LockState:LockForOtherUsers / comment:Added_Project

////                        string exePath = @"C:\Program Files (x86)\vdogClient\VDogAutoCheckOut.exe";

////                        string arguments = @"/RD:""C:\vdClientArchive"" " +
////                                           @"/AT:C " +
////                                           $@"/DirR:{user.ComponentForCheckIn} " +
////                                           $@"/Account:{user.OctoplantUser} " +
////                                           $@"/Domain:{user.OctoplantDomain} " +
////                                           $@"/Password:{user.OctoplantPassword} " +
////                                           @"/LockState:LockForOtherUsers " +
////                                           $@"/Comment:""Component_Is_Multiuser_Project""";

////                        ProcessStartInfo psi = new ProcessStartInfo
////                        {
////                            FileName = exePath,
////                            Arguments = arguments,
////                            UseShellExecute = false,
////                            RedirectStandardOutput = true,
////                            RedirectStandardError = true,
////                            CreateNoWindow = true
////                        };

////                        using (Process process1 = new Process())
////                        {
////                            process1.StartInfo = psi;
////                            process1.Start();

////                            string output1 = process1.StandardOutput.ReadToEnd();
////                            string error1 = process1.StandardError.ReadToEnd();

////                            process1.WaitForExit();

////                            Console.WriteLine("Output:\n" + output1);
////                            Console.WriteLine("Error:\n" + error1);
////                        }

////                        MessageBox.Show("Execution successful!\n" + output, "Success");
////                        Thread.Sleep(2000);

////                        string folderPath = fullPath;

////                        if (Directory.Exists(folderPath))
////                        {
////                            Directory.Delete(folderPath, true);
////                            Console.WriteLine($"Folder deleted successfully: {folderPath}");
////                        }
////                        else
////                        {
////                            Console.WriteLine("Folder not found: " + folderPath);
////                        }
////                    }
////                }
////                catch (System.Exception ex)
////                {
////                    MessageBox.Show("Error executing command: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
////                }
////            }

////            Console.WriteLine($"Cycle completed for {user.ProjectName} at {DateTime.Now}\n");
////        }
////    }

////    public static string Decrypt(string cipherText)
////    {
////        using (Aes aes = Aes.Create())
////        {
////            aes.Key = Encoding.UTF8.GetBytes(key);
////            aes.IV = new byte[16]; // must match Encrypt

////            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
////            using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
////            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
////            using (var sr = new StreamReader(cs))
////            {
////                return sr.ReadToEnd();
////            }
////        }
////    }


////    private static readonly string key = "MySecretKey12345";
////    public static string Encrypt(string plainText)
////    {
////        using (Aes aes = Aes.Create())
////        {
////            aes.Key = Encoding.UTF8.GetBytes(key);
////            aes.IV = new byte[16]; // 16-byte zero IV (for simplicity)

////            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
////            using (var ms = new MemoryStream())
////            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
////            using (var sw = new StreamWriter(cs))
////            {
////                sw.Write(plainText);
////                sw.Close();
////                return Convert.ToBase64String(ms.ToArray());
////            }
////        }
////    }
////    static void CopyDirectoryIncludingRoot(string sourceDir, string destinationDir)
////    {
////        string folderName = Path.GetFileName(sourceDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

////        string destDir = Path.Combine(destinationDir, folderName);

////        CopyDirectory(sourceDir, destDir);
////    }

////    static void CopyDirectory(string sourceDir, string destinationDir)    
////    {
////        if (!Directory.Exists(destinationDir))
////        {
////            Directory.CreateDirectory(destinationDir);
////        }

////        foreach (string file in Directory.GetFiles(sourceDir))
////        {
////            string fileName = Path.GetFileName(file);
////            string destFile = Path.Combine(destinationDir, fileName);
////            File.Copy(file, destFile, true);
////        }

////        foreach (string subDir in Directory.GetDirectories(sourceDir))
////        {
////            string subDirName = Path.GetFileName(subDir);
////            string destSubDir = Path.Combine(destinationDir, subDirName);
////            CopyDirectory(subDir, destSubDir);
////        }
////    }
////    static void SaveUsersToTextFile(List<OctoplantUserData> users, string filePath)
////    {
////        try
////        {
////            using (StreamWriter writer = new StreamWriter(filePath, false))
////            {
////                foreach (var user in users)
////                {
////                    writer.WriteLine($"{user.ServerUrl}|{user.ProjectName}|{user.ComponentForCheckIn}|{user.Interval}|{user.OctoplantProgramFiles}|{user.OctoplantClientArchive}|{user.OctoplantDomain}|{user.OctoplantUser}|{user.OctoplantPassword}|{user.DBFolder}|{user.Compare}");
////                }
////            }
////            Console.WriteLine("User data saved to " + Path.GetFullPath(filePath));
////        }
////        catch (System.Exception ex)
////        {
////            Console.WriteLine("Error saving user data: " + ex.Message);
////        }
////    }

////    static List<OctoplantUserData> LoadUsersFromTextFile(string filePath)
////    {
////        var users = new List<OctoplantUserData>();

////        try
////        {
////            if (!File.Exists(filePath))
////                return users; // no file yet

////            foreach (var line in File.ReadAllLines(filePath))
////            {
////                if (string.IsNullOrWhiteSpace(line)) continue;

////                var parts = line.Split('|');
////                if (parts.Length >= 5)
////                {
////                    users.Add(new OctoplantUserData
////                    {
////                        ServerUrl = parts[0],
////                        ProjectName = parts[1],
////                        ComponentForCheckIn = parts[2],
////                        Interval = parts[3],
////                        OctoplantProgramFiles = parts[4],
////                        OctoplantClientArchive = parts[5],
////                        OctoplantDomain = parts[6],
////                        OctoplantUser = parts[7],
////                        OctoplantPassword = parts[8],
////                        DBFolder = parts[9],
////                        Compare = bool.TryParse(parts[3], out bool cmp) ? cmp : false
////                    });
////                }
////            }
////            Console.WriteLine("Loaded user data from " + Path.GetFullPath(filePath));
////        }
////        catch (System.Exception ex)
////        {
////            Console.WriteLine("Error loading user data: " + ex.Message);
////        }

////        return users;
////    }

////    public static void testData()
////    {
////        StringBuilder sb = new StringBuilder();

////        for (int i = 5; i > 0; i--)
////        {
////            for (int j = i; j>0; j--)
////            {
////                sb.Append("* ");
////            }
////            sb.AppendLine("\n");
////        }

////        MessageBox.Show(sb.ToString(), "Pattern Output", MessageBoxButtons.OK, MessageBoxIcon.Information);
////    }
////}



////private void BtnGetConfig_Click(object sender, EventArgs e)
////{
////    if (currentConfig == null)
////    {
////        txtOutput.Text = "No configuration found.\nPlease click 'General Config' to set it first.";
////        return;
////    }

////    txtOutput.Text =
////        $"Version: {currentConfig.Version}"+ Environment.NewLine +
////        $"Interval: {currentConfig.Interval}" + Environment.NewLine +
////        $"Component: {currentConfig.Component}" + Environment.NewLine +
////        $"Octoplant Program Folder: {currentConfig.ProgramFile}" + Environment.NewLine +
////        $"Octoplant Client Archive: {currentConfig.ClientArchive}" + Environment.NewLine +
////        $"Octoplant Domain: {currentConfig.Domain}" + Environment.NewLine +
////        $"Octoplant User: {currentConfig.User}" + Environment.NewLine +
////        $"Octoplant Password: {currentConfig.Password}" + Environment.NewLine +
////        $"DB Folder: {currentConfig.DBFolder}";
////}



using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Action = System.Action;
using Application = System.Windows.Forms.Application;
using Button = System.Windows.Forms.Button;
using CheckBox = System.Windows.Forms.CheckBox;
using Label = System.Windows.Forms.Label;
using TextBox = System.Windows.Forms.TextBox;

// =============================
// ========== MODELS ===========
// =============================

public class ProjectInfo
{
    public string ProjectName { get; set; }
    public string Component { get; set; }
}

public class ServerInfo
{
    public string ServerName { get; set; }
    public string ServerUrl { get; set; }
    public List<ProjectInfo> Projects { get; set; } = new List<ProjectInfo>();
}

public class GeneralConfig
{
    public string Version { get; set; }
    public string Interval { get; set; }
    public string OctoplantUser { get; set; }
    public string OctoplantPassword { get; set; }
    public string Domain { get; set; }
    public string OctoplantProgram { get; set; }
    public string OctoplantClientArchive { get; set; }
    public string DBFolder { get; set; }
    public string Compare { get; set; }
    public string ReleaseAfter { get; set; }

    // Add this missing property
    public List<ServerInfo> Servers { get; set; } = new List<ServerInfo>();
}


public class OctoplantUserData
{
    public string ServerUrl { get; set; }
    public string ProjectName { get; set; }
    public string ComponentForCheckIn { get; set; }
    public string Interval { get; set; }
    public string OctoplantProgramFiles { get; set; }
    public string OctoplantClientArchive { get; set; }
    public string OctoplantDomain { get; set; }
    public string OctoplantUser { get; set; }
    public string OctoplantPassword { get; set; } // plain in memory (decrypted)
    public string DBFolder { get; set; }
    public bool Compare { get; set; }
    public bool ReleaseAfterCheckIn { get; set; }
    public string ManagerID { get; set; }
    public string ManagerPassword { get; set; }

}

// Root JSON file structure
public class UserProjectMapping
{
    public string OctoplantUser { get; set; }
    public string ServerName { get; set; }
    public string ServerUrl { get; set; }
    public string ProjectName { get; set; }
    public string Component { get; set; }
    public string managerID { get; set; }
    public string managerPassword { get; set; }
}

public class RootConfigJson
{
    public List<GeneralConfigRecord> GeneralConfigs { get; set; } = new List<GeneralConfigRecord>();
    public List<ServerInfo> Servers { get; set; } = new List<ServerInfo>();
    public List<UserProjectMapping> UserProjects { get; set; } = new List<UserProjectMapping>();
}


// Stored record for GeneralConfig with encrypted password
public class GeneralConfigRecord
{
    public string Version { get; set; }
    public string Interval { get; set; }
    public string OctoplantUser { get; set; }
    public string EncryptedPassword { get; set; }
    public string Domain { get; set; }
    public string OctoplantProgram { get; set; }
    public string OctoplantClientArchive { get; set; }
    public string DBFolder { get; set; }
    public string Compare { get; set; }
    public string ReleaseAfter { get; set; }
}

// ===========================================
// ========== CENTRAL JSON STORAGE ============
// ===========================================



public static class TiaPortalJsonStorage
{
    private static readonly object _lock = new object();
    public static readonly string ConfigFolder = @"C:\CheckINExe";
    public static readonly string ConfigJsonFile = Path.Combine(ConfigFolder, "ConfigData.json");

    // Encryption key (same as before)
    private static readonly string key = "MySecretKey12345";

    // Use only Newtonsoft.Json
    private static readonly JsonSerializerSettings _jsonOptions = new JsonSerializerSettings
    {
        Formatting = Newtonsoft.Json.Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Include
    };

    // ------------------ AES HELPERS ------------------
    public static string EncryptString(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = new byte[16];
            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
                sw.Flush();
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    public static string DecryptString(string cipherText)
    {
        try
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = new byte[16];
                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
        catch
        {
            return cipherText; // already plain
        }
    }

    // ------------------ ROOT MODEL ------------------
    public class RootConfigJson
    {
        public List<GeneralConfigRecord> GeneralConfigs { get; set; } = new List<GeneralConfigRecord>();
        public List<ServerInfo> Servers { get; set; } = new List<ServerInfo>();

        // Link each project to a specific user + server
        public List<UserProjectMapping> UserProjects { get; set; } = new List<UserProjectMapping>();
    }

    // ------------------ CORE LOAD/SAVE ------------------
    private static RootConfigJson Load()
    {
        lock (_lock)
        {
            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);

            if (!File.Exists(ConfigJsonFile))
            {
                var fresh = new RootConfigJson();
                File.WriteAllText(ConfigJsonFile, JsonConvert.SerializeObject(fresh, _jsonOptions));
                return fresh;
            }

            var json = File.ReadAllText(ConfigJsonFile);
            if (string.IsNullOrWhiteSpace(json))
            {
                var fresh = new RootConfigJson();
                File.WriteAllText(ConfigJsonFile, JsonConvert.SerializeObject(fresh, _jsonOptions));
                return fresh;
            }

            var data = JsonConvert.DeserializeObject<RootConfigJson>(json, _jsonOptions);
            return data ?? new RootConfigJson();
        }
    }

    private static void Save(RootConfigJson root)
    {
        lock (_lock)
        {
            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);

            string json = JsonConvert.SerializeObject(root, _jsonOptions);
            File.WriteAllText(ConfigJsonFile, json);
        }
    }

    // Exposed for direct use (used by AddProjectForm)
    public static void SaveAll(List<GeneralConfig> configs)
    {
        var root = Load();
        root.GeneralConfigs = configs.Select(c => new GeneralConfigRecord
        {
            Version = c.Version,
            Interval = c.Interval,
            OctoplantUser = c.OctoplantUser,
            EncryptedPassword = EncryptString(c.OctoplantPassword ?? ""),
            Domain = c.Domain,
            OctoplantProgram = c.OctoplantProgram,
            OctoplantClientArchive = c.OctoplantClientArchive,
            DBFolder = c.DBFolder,
            Compare = c.Compare,
            ReleaseAfter = c.ReleaseAfter
        }).ToList();

        Save(root);
    }
    public static void RenameServer(string oldName, string newName, string newUrl)
    {
        var root = Load();

        // Update or create server record
        var existingServer = root.Servers.FirstOrDefault(s =>
            s.ServerName.Equals(oldName, StringComparison.OrdinalIgnoreCase));

        if (existingServer != null)
        {
            existingServer.ServerName = newName;
            existingServer.ServerUrl = newUrl;
        }
        else
        {
            // If somehow not found, add it
            root.Servers.Add(new ServerInfo
            {
                ServerName = newName,
                ServerUrl = newUrl,
                Projects = new List<ProjectInfo>()
            });
        }

        // Update all linked UserProjects safely — DO NOT DELETE
        foreach (var up in root.UserProjects)
        {
            if (up.ServerName.Equals(oldName, StringComparison.OrdinalIgnoreCase))
            {
                up.ServerName = newName;
                up.ServerUrl = newUrl;
            }
        }

        // Also fix any projects linked under Servers
        foreach (var server in root.Servers)
        {
            if (server.ServerName.Equals(newName, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var project in server.Projects)
                {
                    // Ensure internal project structure remains correct
                    project.ProjectName = project.ProjectName?.Trim();
                    project.Component = project.Component?.Trim();
                }
            }
        }

        Save(root);
    }
    public static RootConfigJson LoadFullJson()
    {
        try
        {
            if (!File.Exists(ConfigJsonFile))
                return new RootConfigJson();

            string json = File.ReadAllText(ConfigJsonFile);
            var root = Newtonsoft.Json.JsonConvert.DeserializeObject<RootConfigJson>(json);
            return root ?? new RootConfigJson();
        }
        catch
        {
            return new RootConfigJson();
        }
    }

    public static void AddOrUpdateServer(string serverName, string serverUrl)
    {
        var root = Load();

        var server = root.Servers.FirstOrDefault(s =>
            s.ServerName.Equals(serverName, StringComparison.OrdinalIgnoreCase));

        if (server == null)
        {
            // Add new server entry
            root.Servers.Add(new ServerInfo
            {
                ServerName = serverName,
                ServerUrl = serverUrl,
                Projects = new List<ProjectInfo>()
            });
        }
        else
        {
            // Update only URL
            server.ServerUrl = serverUrl;
        }

        // Propagate updated URL to all related user-project mappings
        foreach (var up in root.UserProjects)
        {
            if (up.ServerName.Equals(serverName, StringComparison.OrdinalIgnoreCase))
            {
                up.ServerUrl = serverUrl;
            }
        }

        Save(root);
    }


    // ------------------ CRUD (GENERAL CONFIG) ------------------
    public static List<GeneralConfig> GetAllGeneralConfigs()
    {
        var root = Load();
        return root.GeneralConfigs.Select(r => new GeneralConfig
        {
            Version = r.Version,
            Interval = r.Interval,
            OctoplantUser = r.OctoplantUser,
            OctoplantPassword = DecryptString(r.EncryptedPassword),
            Domain = r.Domain,
            OctoplantProgram = r.OctoplantProgram,
            OctoplantClientArchive = r.OctoplantClientArchive,
            DBFolder = r.DBFolder,
            Compare = r.Compare,
            ReleaseAfter = r.ReleaseAfter
        }).ToList();
    }

    // ------------------ CRUD (SERVERS & PROJECT CATALOG) ------------------
    public static Dictionary<string, string> GetAllServersAsDictionary()
    {
        var root = Load();
        return root.Servers.ToDictionary(s => s.ServerName, s => s.ServerUrl, StringComparer.OrdinalIgnoreCase);
    }

    //public static Dictionary<string, List<(string ProjectName, string Component)>> GetProjectsByServer()
    //{
    //    var root = Load();
    //    var dict = new Dictionary<string, List<(string, string)>>(StringComparer.OrdinalIgnoreCase);
    //    foreach (var s in root.Servers)
    //        dict[s.ServerName] = s.Projects.Select(p => (p.ProjectName, p.Component)).ToList();
    //    return dict;
    //}
    public static Dictionary<string, List<(string ProjectName, string Component)>> GetProjectsByServer()
    {
        var dict = new Dictionary<string, List<(string ProjectName, string Component)>>(StringComparer.OrdinalIgnoreCase);

        var root = LoadFullJson();
        if (root?.UserProjects == null || root.UserProjects.Count == 0)
            return dict;

        // 🔹 Group projects by ServerName
        foreach (var project in root.UserProjects)
        {
            if (string.IsNullOrWhiteSpace(project.ServerName) || string.IsNullOrWhiteSpace(project.ProjectName))
                continue;

            if (!dict.ContainsKey(project.ServerName))
                dict[project.ServerName] = new List<(string, string)>();

            dict[project.ServerName].Add((project.ProjectName, project.Component));
        }

        return dict;
    }

    public static void AddOrUpdateGeneralConfig(GeneralConfig cfg)
    {
        var root = Load();
        var existing = root.GeneralConfigs.FirstOrDefault(x =>
            string.Equals(x.OctoplantUser, cfg.OctoplantUser, StringComparison.OrdinalIgnoreCase));

        if (existing == null)
        {
            root.GeneralConfigs.Add(new GeneralConfigRecord
            {
                Version = cfg.Version ?? "",
                Interval = cfg.Interval ?? "",
                OctoplantUser = cfg.OctoplantUser ?? "",
                EncryptedPassword = EncryptString(cfg.OctoplantPassword ?? ""),
                Domain = cfg.Domain ?? "",
                OctoplantProgram = cfg.OctoplantProgram ?? "",
                OctoplantClientArchive = cfg.OctoplantClientArchive ?? "",
                DBFolder = cfg.DBFolder ?? "",
                Compare = cfg.Compare ?? "",
                ReleaseAfter = cfg.ReleaseAfter ?? ""
            });
        }
        else
        {
            existing.Version = cfg.Version ?? "";
            existing.Interval = cfg.Interval ?? "";
            existing.EncryptedPassword = EncryptString(cfg.OctoplantPassword ?? "");
            existing.Domain = cfg.Domain ?? "";
            existing.OctoplantProgram = cfg.OctoplantProgram ?? "";
            existing.OctoplantClientArchive = cfg.OctoplantClientArchive ?? "";
            existing.DBFolder = cfg.DBFolder ?? "";
            existing.Compare = cfg.Compare ?? "";
            existing.ReleaseAfter = cfg.ReleaseAfter ?? "";
        }

        Save(root);
    }

   
    public static void DeleteServer(string serverName)
    {
        var root = Load();

        root.Servers.RemoveAll(x => x.ServerName.Equals(serverName, StringComparison.OrdinalIgnoreCase));
        // 🔁 also delete user-project mappings for that server
        root.UserProjects.RemoveAll(x => x.ServerName.Equals(serverName, StringComparison.OrdinalIgnoreCase));

        Save(root);
    }

    // ------------------ USER-PROJECT RELATION ------------------
    public static void AddUserProject(string octoplantUser, string serverName, string projectName, string component, string ManagerID, string ManagerPassword)
    {
        var root = Load();

        // Find server URL once
        var server = root.Servers.FirstOrDefault(s =>
            s.ServerName.Equals(serverName, StringComparison.OrdinalIgnoreCase));
        string serverUrl = server?.ServerUrl ?? "";

        // Remove old record if same user + server + project exists
        root.UserProjects.RemoveAll(x =>
            x.OctoplantUser.Equals(octoplantUser, StringComparison.OrdinalIgnoreCase) &&
            x.ServerName.Equals(serverName, StringComparison.OrdinalIgnoreCase) &&
            x.ProjectName.Equals(projectName, StringComparison.OrdinalIgnoreCase) &&
        x.managerID.Equals(ManagerID, StringComparison.OrdinalIgnoreCase) &&
        x.managerPassword.Equals(ManagerPassword, StringComparison.OrdinalIgnoreCase));

        // Add new record
        root.UserProjects.Add(new UserProjectMapping
        {
            OctoplantUser = octoplantUser,
            ServerName = serverName,
            ServerUrl = serverUrl,
            ProjectName = projectName,
            Component = component,
            managerID =  ManagerID,
            managerPassword = ManagerPassword
        });

        Save(root);
    }

    // ------------------ OPTIONAL: server catalog projects ------------------
    public static void AddOrUpdateProject(string serverName, ProjectInfo project)
    {
        var root = Load();
        var s = root.Servers.FirstOrDefault(x => x.ServerName.Equals(serverName, StringComparison.OrdinalIgnoreCase));

        if (s == null)
        {
            s = new ServerInfo { ServerName = serverName, ServerUrl = "", Projects = new List<ProjectInfo>() };
            root.Servers.Add(s);
        }

        var p = s.Projects.FirstOrDefault(x =>
            x.ProjectName.Equals(project.ProjectName, StringComparison.OrdinalIgnoreCase));

        if (p == null)
        {
            s.Projects.Add(new ProjectInfo { ProjectName = project.ProjectName, Component = project.Component });
        }
        else
        {
            p.Component = project.Component;
        }

        Save(root);
    }

    public static void DeleteProject(string serverName, string projectName)
    {
        var root = Load();
        var s = root.Servers.FirstOrDefault(x => x.ServerName.Equals(serverName, StringComparison.OrdinalIgnoreCase));
        if (s != null)
        {
            s.Projects.RemoveAll(x => x.ProjectName.Equals(projectName, StringComparison.OrdinalIgnoreCase));
        }

        // Also remove any user-project mappings that reference this project on this server
        root.UserProjects.RemoveAll(x =>
            x.ServerName.Equals(serverName, StringComparison.OrdinalIgnoreCase) &&
            x.ProjectName.Equals(projectName, StringComparison.OrdinalIgnoreCase));

        Save(root);
    }

    // ------------------ FLATTEN (user-dependent) ------------------
    public static List<OctoplantUserData> GetFlattenedUsers()
    {
        var root = Load();
        var list = new List<OctoplantUserData>();

        foreach (var g in root.GeneralConfigs)
        {
            string plainPwd = DecryptString(g.EncryptedPassword ?? "");

            // Only the projects belonging to this user
            var userProjects = root.UserProjects
                .Where(p => p.OctoplantUser.Equals(g.OctoplantUser, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var p in userProjects)
            {
                list.Add(new OctoplantUserData
                {
                    ServerUrl = p.ServerUrl,
                    ProjectName = p.ProjectName,
                    ComponentForCheckIn = p.Component,
                    Interval = g.Interval ?? "5",
                    OctoplantProgramFiles = g.OctoplantProgram ?? "",
                    OctoplantClientArchive = g.OctoplantClientArchive ?? "",
                    OctoplantDomain = g.Domain ?? "",
                    OctoplantUser = g.OctoplantUser ?? "",
                    OctoplantPassword = plainPwd,
                    DBFolder = g.DBFolder ?? "",
                    Compare = string.Equals(g.Compare, "true", StringComparison.OrdinalIgnoreCase),
                    //ReleaseAfterCheckIn = string.Equals(g.ReleaseAfter, "true", StringComparison.OrdinalIgnoreCase),
                    ManagerID = p.managerID,
                    ManagerPassword = p.managerPassword
                });
            }
        }

        return list;
    }
}

// ===============================================
// ========== AUTOMATION + ENCRYPT WRAPPER =======
// ===============================================

class TiaPortalAutomation
{

    static System.Timers.Timer timer;
    static List<OctoplantUserData> allUsers;

    static string currentDir = Directory.GetCurrentDirectory();
    static string logData = Path.Combine(currentDir, "LogData.txt");
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

    public static readonly string ConfigFolder = TiaPortalJsonStorage.ConfigFolder;

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
    [STAThread]
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
    static void Main(string[] args)
    {
        //Application.EnableVisualStyles();
        //Application.SetCompatibleTextRenderingDefault(false);
        
        //ReadData();

        //// If no application key exists → generate
        //if (string.IsNullOrEmpty(ApplicationKey))
        //{
        //    string deviceName = Environment.MachineName + ".PC";
        //    GetApplicationKey(deviceName);
        //}

        //// If no validation key OR wrong validation key → show license form
        //if (string.IsNullOrEmpty(ValidationKey) || !VerifyKey())
        //{
        //    LicenseForm lf = new LicenseForm(ApplicationKey);

        //    if (lf.ShowDialog() == DialogResult.OK)
        //    {
        //        ValidationKey = lf.EnteredValidationKey;

        //        if (!VerifyKey())
        //        {
        //            MessageBox.Show("Invalid Validation Key.");
        //            return;
        //        }

        //        // Save Data.dat
        //        SaveKeyData();
        //    }
        //    else
        //    {
        //        return; // user closed window
        //    }
        //}
        try
        {
            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);

            // Load JSON to memory models for UI
            MainForm.generalConfigs = TiaPortalJsonStorage.GetAllGeneralConfigs();
            MainForm.serverList = TiaPortalJsonStorage.GetAllServersAsDictionary();
            MainForm.projectList = TiaPortalJsonStorage.GetProjectsByServer();

            // Flattened list for automation
            allUsers = TiaPortalJsonStorage.GetFlattenedUsers();

            if (allUsers.Count > 0)
                Console.WriteLine($"Loaded {allUsers.Count} entries from JSON.");
            else
                Console.WriteLine("No saved configuration found in JSON.");

            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show("An unexpected error occurred during startup:\n" + ex.Message,
                            "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static void updateDat(string userName, string userpassword, string exportPath, string server, string projName)
    {
        string batFilePath = @"C:\VCI_GIT_RnD\ExportProject.bat";
        string username = userName;
        string password = userpassword;
        username = userName;//"wipropari\\" + userName; 
        // Step 1: Read the BAT file
        string[] lines = File.ReadAllLines(batFilePath);

        bool foundUser = false;
        bool foundPass = false;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].TrimStart();

            if (line.StartsWith("set \"USERNAME=", StringComparison.OrdinalIgnoreCase))
            {
                lines[i] = $"set \"USERNAME={username}\"";
                foundUser = true;
            }
            else if (line.StartsWith("set \"PASSWORD=", StringComparison.OrdinalIgnoreCase))
            {
                lines[i] = $"set \"PASSWORD={password}\"";
                foundPass = true;
            }
            else if (line.StartsWith("set \"TARGET=", StringComparison.OrdinalIgnoreCase))
            {
                lines[i] = $"set \"TARGET={exportPath}\"";
                foundPass = true;
            }
            else if (line.StartsWith("set \"SERVER=", StringComparison.OrdinalIgnoreCase))
            {
                lines[i] = $"set \"SERVER={server}\"";
                foundPass = true;
            }
            else if (line.StartsWith("set \"PROJECT=", StringComparison.OrdinalIgnoreCase))
            {
                lines[i] = $"set \"PROJECT={projName}\"";
                foundPass = true;
            }
        }

        // Step 2: Write back the modified or new variables
        StreamWriter sw = new StreamWriter(batFilePath);
        for (int i = 0; i < lines.Length; i++)
        {
            sw.WriteLine(lines[i]);
        }

        if (!foundUser)
            sw.WriteLine($"set \"USERNAME={username}\"");
        if (!foundPass)
            sw.WriteLine($"set \"PASSWORD={password}\"");

        sw.Close();
    }
    private static string RunProcess1(string batpath)
    {
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();
        string output = "";
        string error = "";
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = batpath;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;

            Process proc = new Process();
            proc.StartInfo = psi;

            proc.Start();

            output = proc.StandardOutput.ReadToEnd();
            error = proc.StandardError.ReadToEnd();

            proc.WaitForExit();
            proc.Close();

            // Step 4: Display results
            Console.WriteLine("OUTPUT:");
            Console.WriteLine(output);

            Console.WriteLine("ERROR:");
            Console.WriteLine(error);
            return output;
        }
        catch
        {
            return "Error : " + error;
        }


    }
    // ---------- Main Automation Loop ----------
    public static void RunAutomationCycle(List<OctoplantUserData> _)
    {
        // Always reload fresh snapshot from JSON
        allUsers = _;// TiaPortalJsonStorage.GetFlattenedUsers();

        if (allUsers.Count == 0)
        {
            Console.WriteLine("No users found in JSON Config.");
            return;
        }

        foreach (var user in allUsers)
        {
            Console.WriteLine($"Starting cycle for project: {user.ProjectName} ({DateTime.Now})");
            File.AppendAllText(logData,
                $"{DateTime.Now} : Project Name : {user.ProjectName}\n Server URL : {user.ServerUrl}\n User Name : {user.OctoplantUser}\n");

            string version = "";
            string change = "";

            try
            {
                updateDat(user.ManagerID, user.ManagerPassword, user.DBFolder, user.ServerUrl, user.ProjectName);

                string workingDirectory = @"C:\Program Files\Siemens\Automation\Project Server\Bin";
                string command = "pspt";
                string arguments = "checkin show --server " + user.ServerUrl + " --project " + user.ProjectName;

                string userValidateOutput1 = RunProcess1(@"C:\VCI_GIT_RnD\ExportProject.bat");
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command} {arguments}",
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process())
                {
                    process.StartInfo = psi;
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("----- ERROR -----");
                        Console.WriteLine(error);
                        File.AppendAllText(logData, DateTime.Now + " : " + error + "\n");
                    }

                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(l => l.TrimEnd())
                                      .ToList();

                    var dataLines = lines
                        .Where(line => !line.StartsWith("_") &&
                                       !line.StartsWith("Workspace", StringComparison.OrdinalIgnoreCase) &&
                                       !string.IsNullOrWhiteSpace(line))
                        .ToList();

                    if (dataLines.Count > 0)
                    {
                        var datalineLast = dataLines.Last();
                        var parts = datalineLast.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length > 1) version = parts[1];
                        if (parts.Length > 4) change = parts[4];

                        // robust comment extraction (last row with leading number)
                        var lastDataLine = lines.LastOrDefault(l => Regex.IsMatch(l.TrimStart(), @"^\d+"));
                        if (!string.IsNullOrEmpty(lastDataLine))
                        {
                            var match = Regex.Match(lastDataLine, @"\d{2}[:\-]\d{2}[:\-]\d{4}\s+\d{2}:\d{2}:\d{2}\s+(.*)$");
                            if (match.Success) change = match.Groups[1].Value.Trim();
                        }

                        File.AppendAllText(logData, DateTime.Now + " : " + datalineLast + "\n");
                    }
                }

                //// ------- Export from server -------
                //try
                //{
                //    string fullPath1 = Path.Combine(user.DBFolder, user.ProjectName);
                //    string workingDir = @"C:\Program Files\Siemens\Automation\Project Server\Bin";
                //    string exportPath = user.DBFolder;
                //    string command1 = $"pspt project export --server {user.ServerUrl} --project {user.ProjectName} --path \"{exportPath}\" -v {version}";
                //    string cmdArgs = $"/K cd /d \"{workingDir}\" && {command1}";

                //    if (Directory.Exists(fullPath1))
                //    {
                //        Directory.Delete(fullPath1, true);
                //        Console.WriteLine($"Folder deleted successfully: {fullPath1}");
                //    }

                //    var psi1 = new ProcessStartInfo
                //    {
                //        FileName = "cmd.exe",
                //        Arguments = cmdArgs,
                //        UseShellExecute = false,
                //        Verb = "runas",
                //        CreateNoWindow = true,
                //        RedirectStandardInput = true,
                //        RedirectStandardOutput = true,
                //        RedirectStandardError = true
                //    };

                //    using (var process = new Process())
                //    {
                //        process.StartInfo = psi1;
                //        process.Start();
                //        Thread.Sleep(10000);

                //        try
                //        {
                //            using (var sw = process.StandardInput)
                //            {
                //                if (sw.BaseStream.CanWrite)
                //                {
                //                    sw.WriteLine($"cd /d \"{workingDir}\"");
                //                    sw.WriteLine(command1);
                //                    Thread.Sleep(2000);
                //                    sw.WriteLine("y");
                //                    sw.WriteLine("exit");
                //                }
                //            }
                //        }
                //        catch { /* ignore */ }

                //        string output = process.StandardOutput.ReadToEnd();
                //        string error = process.StandardError.ReadToEnd();
                //        process.WaitForExit();

                //        if (!string.IsNullOrEmpty(error))
                //        {
                //            Console.WriteLine("=== Error ===");
                //            Console.WriteLine(error);
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine("Error launching CMD: " + ex.Message);
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }

            Thread.Sleep(5000);
            string fullPath = Path.Combine(user.DBFolder, user.ProjectName);

            if (Directory.Exists(fullPath))
            {
                string component = (user.ComponentForCheckIn ?? "").TrimStart('\\', '/');
                string localOctoplantPath = Path.Combine(@"C:\vdClientArchive", component);

                try
                {
                    CopyDirectoryIncludingRoot(fullPath, localOctoplantPath);
                }
                catch { /* ignore */ }

                Thread.Sleep(5000);

                string comment = $"The Imported revision was {version} and project {user.ProjectName}";
                string changes = string.IsNullOrWhiteSpace(change) ? $"Revision {version}" : $"Revision {version} : {change}";
                try
                {
                    //pspt project exportHistory -s https://nb012026:8735 -p MultiUser_Testing -t "C:\Users\wpuser\OneDrive - Wipro PARI\Desktop\MultiUser\history_MultiUser.xml" -v 79 -f


                    string fileDirectory = currentDir + "\\historyXML\\history_MultiUser.xml";
                    string workingDirectory = @"C:\Program Files\Siemens\Automation\Project Server\Bin";
                    string command = "pspt";
                    string arguments = "project exportHistory -s " + user.ServerUrl + " -p " + user.ProjectName + " -t " + fileDirectory + " -v " + version + " -f";

                    var psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c {command} {arguments}",
                        WorkingDirectory = workingDirectory,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = new Process())
                    {
                        process.StartInfo = psi;
                        process.Start();

                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        if (string.IsNullOrEmpty(error))
                        {
                            XDocument doc = XDocument.Load(fileDirectory);

                            var revisions = doc
                                .Descendants()
                                .Where(x => x.Name.LocalName.Equals("Revision", StringComparison.OrdinalIgnoreCase))
                                .Select(r => new
                                {
                                    RevisionNumber = ((string)r.Attribute("number")) ?? "N/A",
                                    WindowsUser = ((string)r.Attribute("windowsUser")) ?? "N/A",
                                    ServerUser = ((string)r.Attribute("serverUser")) ?? "N/A",
                                    ComputerName = ((string)r.Attribute("computerName")) ?? "N/A",
                                    Comment = ((string)r.Attribute("comment")) ?? "N/A"
                                })
                                .ToList();
                            if (revisions.Count != 0)
                            {
                                comment = "The Imported revision was " + revisions[0].RevisionNumber +" and project MultiUser_Testing" + ", Changed By : " + revisions[0].WindowsUser + " Changed From Server : " + user.ServerUrl + " Changed From Device : " + revisions[0].ComputerName;
                                changes =  "Revision "+ revisions[0].RevisionNumber + " : "+ revisions[0].Comment;
                            }
                        }
                        else
                        {

                        }
                        process.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error When Access Server Info : " + ex.Message);
                    return;
                }


                try
                {
                    // ---------- Build INI and Run AutoCheckIn ----------
                    //string comment = $"The Imported revision was {version} from server {user.ServerUrl} and project {user.ProjectName}.";
                    //string changes = string.IsNullOrWhiteSpace(change) ? $"Revision {version}" : $"Revision {version} : {change}";

                    string plainPassword = user.OctoplantPassword; // already decrypted
                    string iniFile = $@"[User]
Account={user.OctoplantUser}
Domain={user.OctoplantDomain}
Password={plainPassword}
[Component]
Dir={user.ComponentForCheckIn}
[Version]
Enabled=Y
WithoutComparison=N
Comment={comment}
ChangeReason={changes}";
                    /*string iniFile = $@"[User]
Account={user.OctoplantUser}
Domain={user.OctoplantDomain}
Password={plainPassword}
[Component]
Dir={user.ComponentForCheckIn}
[Version]
Enabled=Y
WithoutComparison=N
Comment={comment}
ChangeReason={changes}";*/

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
                        Thread.Sleep(10000);

                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (!string.IsNullOrEmpty(error))
                        {
                            MessageBox.Show("Error: " + error, "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            string checkInFolder = Path.Combine(currentDir, "CheckIn_Error");
                            if (!Directory.Exists(checkInFolder))
                                Directory.CreateDirectory(checkInFolder);

                            string errorFilePath = Path.Combine(checkInFolder, $"Error_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                            File.WriteAllText(errorFilePath, error);
                            File.AppendAllText(logData, DateTime.Now + " : " + error + "\n");
                        }

                        // ---------- AutoCheckOut ----------
                        string exePath = @"C:\Program Files (x86)\vdogClient\VDogAutoCheckOut.exe";

                        string arguments = @"/RD:""C:\vdClientArchive"" " +
                                           @"/AT:C " +
                                           $@"/DirR:{user.ComponentForCheckIn} " +
                                           $@"/Account:{user.OctoplantUser} " +
                                           $@"/Domain:{user.OctoplantDomain} " +
                                           $@"/Password:{plainPassword} " +
                                           @"/LockState:LockForOtherUsers " +
                                           $@"/Comment:""Component_Is_Multiuser_Project""";

                        var psi = new ProcessStartInfo
                        {
                            FileName = exePath,
                            Arguments = arguments,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        };

                        using (var process1 = new Process())
                        {
                            process1.StartInfo = psi;
                            process1.Start();

                            string output1 = process1.StandardOutput.ReadToEnd();
                            string error1 = process1.StandardError.ReadToEnd();

                            process1.WaitForExit();

                            Console.WriteLine("Output:\n" + output1);
                            Console.WriteLine("Error:\n" + error1);
                        }

                        MessageBox.Show("Execution successful!\n" + output, "Success");
                        Thread.Sleep(2000);

                        if (Directory.Exists(fullPath))
                        {
                            Directory.Delete(fullPath, true);
                            Console.WriteLine($"Folder deleted successfully: {fullPath}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error executing command: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            Console.WriteLine($"Cycle completed for {user.ProjectName} at {DateTime.Now}\n");
        }
    }

    // ---------- Helpers ----------
    static void CopyDirectoryIncludingRoot(string sourceDir, string destinationDir)
    {
        string folderName = Path.GetFileName(sourceDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        string destDir = Path.Combine(destinationDir, folderName);
        CopyDirectory(sourceDir, destDir);
    }

    static void CopyDirectory(string sourceDir, string destinationDir)
    {
        if (!Directory.Exists(destinationDir))
            Directory.CreateDirectory(destinationDir);

        foreach (string file in Directory.GetFiles(sourceDir))
            File.Copy(file, Path.Combine(destinationDir, Path.GetFileName(file)), true);

        foreach (string subDir in Directory.GetDirectories(sourceDir))
            CopyDirectory(subDir, Path.Combine(destinationDir, Path.GetFileName(subDir)));
    }
}

// ===============================================
// ============== WINDOWS FORMS UI ===============
// ===============================================

public class MainForm : Form
{
    private Panel leftPanel;
    private Panel mainPanel;
    private Button btnGeneralConfig;
    private Button btnModifyGeneralConfig;
    private Button btnDeleteGeneralConfig;
    private Button btnGetConfig;
    private Button btnAddServer;
    private Button btnModifyServer;
    private Button btnDeleteServer;
    private Button btnAddProject;
    private Button btnModifyProject;
    private Button btnDeleteProject;
    private Button btnStartService, btnStopService;
    private System.Timers.Timer automationTimer;
    private bool serviceRunning = false;

    private TextBox txtOutput;

    // Shared in-memory mirrors (populated from JSON at startup / after writes)
    public static Dictionary<string, string> serverList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public static Dictionary<string, List<(string ProjectName, string Component)>> projectList = new Dictionary<string, List<(string, string)>>(StringComparer.OrdinalIgnoreCase);
    public static List<GeneralConfig> generalConfigs = new List<GeneralConfig>();

    public MainForm()
    {
        this.Text = "TIA Portal Project Server Importer Configurator";
        this.Size = new Size(1000, 650);
        this.StartPosition = FormStartPosition.CenterScreen;

        // Left panel
        leftPanel = new Panel()
        {
            Dock = DockStyle.Left,
            Width = 200,
            BackColor = Color.LightGray
        };

        // Main area
        mainPanel = new Panel()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };

        // --- Buttons on the left ---
        // --- Buttons on the left ---
        int top = 20;
        int gap = 50;

        btnGetConfig = new Button() { Text = "Get Config", Width = 160, Height = 35, Top = top, Left = 20 };
        btnGetConfig.Click += BtnGetConfig_Click;
        top += gap;

        btnGeneralConfig = new Button() { Text = "General Config", Width = 160, Height = 35, Top = top, Left = 20 };
        btnGeneralConfig.Click += (s, e) => new GeneralConfigForm().ShowDialog();
        top += gap;

        btnModifyGeneralConfig = new Button() { Text = "Modify General Config", Width = 160, Height = 35, Top = top, Left = 20 };
        btnModifyGeneralConfig.Click += (s, e) => new ModifyGeneralConfigForm().ShowDialog();
        top += gap;

        btnDeleteGeneralConfig = new Button() { Text = "Delete General Config", Width = 160, Height = 35, Top = top, Left = 20 };
        btnDeleteGeneralConfig.Click += (s, e) => new DeleteGeneralConfigForm().ShowDialog();
        top += gap;

        btnGeneralConfig = new Button()
        {
            Text = "General Config",
            Width = 160,
            Height = 35,
            Top = 70,
            Left = 20
        };
        btnGeneralConfig.Click += (s, e) =>
        {
            using (var form = new GeneralConfigForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var cfg = new GeneralConfig
                    {
                        Version = form.Version ?? "",
                        Interval = form.Interval ?? "",
                        OctoplantUser = form.OctoplantUser ?? "",
                        OctoplantPassword = form.OctoplantPassword ?? "",
                        Domain = form.OctoplantDomain ?? "",
                        OctoplantProgram = form.OctoplantProgram ?? "",
                        OctoplantClientArchive = form.OctoplantClientArchive ?? "",
                        DBFolder = form.DBFolder ?? "",
                        Compare = form.Compare ?? "",
                        //ReleaseAfter = form.ReleaseAfter ?? ""
                    };

                    // Save to JSON
                    TiaPortalJsonStorage.AddOrUpdateGeneralConfig(cfg);

                    // Refresh in-memory mirrors
                    generalConfigs = TiaPortalJsonStorage.GetAllGeneralConfigs();

                    MessageBox.Show($"General Config saved for user '{cfg.OctoplantUser}'",
                        "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        };

        // 🛠 Modify General Config Button
        btnModifyGeneralConfig = new Button()
        {
            Text = "Modify General Config",
            Width = 160,
            Height = 35,
            Top = 120,
            Left = 20
        };
        btnModifyGeneralConfig.Click += (s, e) =>
        {
            using (var modifyForm = new ModifyGeneralConfigForm())
            {
                modifyForm.ShowDialog();
            }
        };

        // ❌ Delete General Config Button
        btnDeleteGeneralConfig = new Button()
        {
            Text = "Delete General Config",
            Width = 160,
            Height = 35,
            Top = 170,
            Left = 20
        };
        btnDeleteGeneralConfig.Click += (s, e) =>
        {
            using (var deleteForm = new DeleteGeneralConfigForm())
            {
                deleteForm.ShowDialog();
            }
        };

        btnAddServer = new Button() { Text = "Add Server", Width = 160, Height = 35, Top = top, Left = 20 };
        btnAddServer.Click += (s, e) => new AddServerForm(serverList, projectList).ShowDialog();
        top += gap;

        btnModifyServer = new Button() { Text = "Modify Server", Width = 160, Height = 35, Top = top, Left = 20 };
        btnModifyServer.Click += (s, e) => new ModifyServerForm(serverList).ShowDialog();
        top += gap;

        btnDeleteServer = new Button() { Text = "Delete Server", Width = 160, Height = 35, Top = top, Left = 20 };
        btnDeleteServer.Click += (s, e) => new DeleteServerForm(serverList).ShowDialog();
        top += gap;

        btnAddProject = new Button() { Text = "Add Project", Width = 160, Height = 35, Top = top, Left = 20 };
        btnAddProject.Click += (s, e) => new AddProjectForm(serverList, projectList, generalConfigs).ShowDialog();
        top += gap;

        btnModifyProject = new Button() { Text = "Modify Project", Width = 160, Height = 35, Top = top, Left = 20 };
        btnModifyProject.Click += (s, e) => new ModifyProjectForm(projectList).ShowDialog();
        top += gap;

        btnDeleteProject = new Button() { Text = "Delete Project", Width = 160, Height = 35, Top = top, Left = 20 };
        btnDeleteProject.Click += (s, e) => new DeleteProjectForm(projectList).ShowDialog();
        top += gap;

        btnStartService = new Button() { Text = "Start Service", Width = 160, Height = 35, Top = top, Left = 20 };
        btnStartService.Click += BtnStartService_Click;
        top += gap;

        

        btnStopService = new Button() { Text = "Stop Service", Width = 160, Height = 35, Top = top, Left = 20, Enabled = false };
        btnStopService.Click += BtnStopService_Click;

        // Add after all are created                
        leftPanel.Controls.AddRange(new Control[]   
        {                                           
btnGetConfig,                                       
btnGeneralConfig,                                   
btnModifyGeneralConfig,                             
btnDeleteGeneralConfig,                             
btnAddServer,                                       
btnModifyServer,                                    
btnDeleteServer,                                    
btnAddProject,                                      
btnModifyProject,                                   
btnDeleteProject,                                   
btnStartService,                                    
btnStopService                                      
        });                                         


        btnStartService = new Button()
        {
            Text = "Start Service",
            Width = 160,
            Height = 35,
            Top = 420,
            Left = 20
        };
        btnStartService.Click += BtnStartService_Click;

        //btnStopService = new Button()
        //{
        //    Text = "Stop Service",
        //    Width = 160,
        //    Height = 35,
        //    Top = 470,
        //    Left = 20,
        //    Enabled = false
        //};
        //btnStopService.Click += BtnStopService_Click;

       // leftPanel.Controls.AddRange(new Control[] { btnStartService, btnStopService });

        mainPanel.Controls.Add(txtOutput);
        this.Controls.Add(mainPanel);
        this.Controls.Add(leftPanel);
    }

    private void BtnStartService_Click(object sender, EventArgs e)
    {
        if (serviceRunning) return;

        // Load all users from JSON
        var users = TiaPortalJsonStorage.GetFlattenedUsers();

        // Filter only those who have valid mapping (user + server + project + component)
        users = users
            .Where(u =>
                !string.IsNullOrWhiteSpace(u.OctoplantUser) &&
                !string.IsNullOrWhiteSpace(u.ServerUrl) &&
                !string.IsNullOrWhiteSpace(u.ProjectName) &&
                !string.IsNullOrWhiteSpace(u.ComponentForCheckIn))
            .ToList();

        if (users.Count == 0)
        {
            MessageBox.Show("No valid user-server-project mappings found in ConfigData.json.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Disable all configuration buttons while service is running
        DisableAllButtons();
        serviceRunning = true;
        btnStopService.Enabled = true;

        MessageBox.Show($"Service started.\nRunning automation for {users.Count} valid configurations...", "Automation");

        // Run first cycle immediately
        TiaPortalAutomation.RunAutomationCycle(users);
        string intervalstr = users[0].Interval;
        int intervalnum = int.Parse(intervalstr);
        // Set automation interval (default = 5 min if missing)
        int interval = int.TryParse(users.FirstOrDefault()?.Interval, out int parsed) ? parsed : intervalnum;

        automationTimer = new System.Timers.Timer(interval * 60 * 1000);
        automationTimer.Elapsed += (s2, ev) =>
        {
            try
            {
                // Re-fetch users dynamically each cycle (in case JSON changes)
                var freshUsers = TiaPortalJsonStorage.GetFlattenedUsers()
                    .Where(u =>
                        !string.IsNullOrWhiteSpace(u.OctoplantUser) &&
                        !string.IsNullOrWhiteSpace(u.ServerUrl) &&
                        !string.IsNullOrWhiteSpace(u.ProjectName) &&
                        !string.IsNullOrWhiteSpace(u.ComponentForCheckIn))
                    .ToList();

                if (freshUsers.Count > 0)
                    TiaPortalAutomation.RunAutomationCycle(freshUsers);
                else
                    Console.WriteLine("Skipped cycle — no valid entries found in JSON.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Automation cycle failed: {ex.Message}");
            }
        };

        automationTimer.Start();
    }


    private void BtnStopService_Click(object sender, EventArgs e)
    {
        if (!serviceRunning) return;

        automationTimer?.Stop();
        automationTimer?.Dispose();
        EnableAllButtons();
        serviceRunning = false;

        MessageBox.Show("Service stopped.");
    }

    private void DisableAllButtons()
    {
        foreach (Control c in leftPanel.Controls)
        {
            if (c is Button btn && btn != btnStopService)
            {
                btn.Enabled = false;
            }
            else if (c is Button btn1 && btn1 == btnStopService)
            {
                btn1.Enabled = true;
            }
        }
    }

    private void EnableAllButtons()
    {
        foreach (Control c in leftPanel.Controls)
            if (c is Button btn)
                btn.Enabled = true;
    }

    private void BtnGetConfig_Click(object sender, EventArgs e)
    {
        // Ensure txtOutput exists before accessing it
        if (txtOutput == null)
        {
            txtOutput = new TextBox()
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new Font("Consolas", 11)
            };
            mainPanel.Controls.Clear();
            mainPanel.Controls.Add(txtOutput);
        }

        //Refresh mirrors from JSON
        generalConfigs = TiaPortalJsonStorage.GetAllGeneralConfigs();
        serverList = TiaPortalJsonStorage.GetAllServersAsDictionary();
        projectList = TiaPortalJsonStorage.GetProjectsByServer();

        // Clear old content
        txtOutput.Clear();

        //Display JSON info
        if (generalConfigs == null || generalConfigs.Count == 0)
        {
            txtOutput.Text = "No general configuration data available!" + Environment.NewLine;
            return;
        }


        int index = 1;
        foreach (var cfg in generalConfigs)
        {
            txtOutput.AppendText($"========== Configuration {index++} ==========" + Environment.NewLine);
            txtOutput.AppendText($"Version: {cfg.Version}" + Environment.NewLine);
            txtOutput.AppendText($"Interval (In Minute): {cfg.Interval}" + Environment.NewLine);
            txtOutput.AppendText($"Octoplant User: {cfg.OctoplantUser}" + Environment.NewLine);
            //txtOutput.AppendText($"Password: {new string('•', Math.Max(4, Math.Min(12, (cfg.OctoplantPassword ?? "").Length)))}" + Environment.NewLine);
            txtOutput.AppendText($"Domain: {cfg.Domain}" + Environment.NewLine);
            txtOutput.AppendText($"Program: {cfg.OctoplantProgram}" + Environment.NewLine);
            txtOutput.AppendText($"ClientArchive: {cfg.OctoplantClientArchive}" + Environment.NewLine);
            txtOutput.AppendText($"DBFolder: {cfg.DBFolder}" + Environment.NewLine);
            txtOutput.AppendText($"Compare: {cfg.Compare}" + Environment.NewLine);
            //txtOutput.AppendText($"ReleaseAfter: {cfg.ReleaseAfter}" + Environment.NewLine);
            txtOutput.AppendText("--------------------------------" + Environment.NewLine);
        }

        txtOutput.AppendText("Server List:" + Environment.NewLine);
        if (serverList == null || serverList.Count == 0)
        {
            txtOutput.AppendText("   (No servers added)" + Environment.NewLine);
        }
        else
        {
            foreach (var s in serverList)
                txtOutput.AppendText($"   {s.Key}  ->  {s.Value}{Environment.NewLine}");
        }

        if (projectList?.Count > 0)
        {
            txtOutput.AppendText(Environment.NewLine + "Projects by Server:" + Environment.NewLine);
            foreach (var kv in projectList)
            {
                txtOutput.AppendText($"   🔹 {kv.Key}:{Environment.NewLine}");
                foreach (var proj in kv.Value)
                {
                    txtOutput.AppendText($"      → {proj.ProjectName} (Component: {proj.Component}){Environment.NewLine}");
                }
            }
        }

       // txtOutput.AppendText(Environment.NewLine + $"JSON File: {TiaPortalJsonStorage.ConfigJsonFile}" + Environment.NewLine);
    }
}

// ================================
// ======== FORMS (CRUD) ==========
// ================================

public class AddProjectForm : Form
{
    private ComboBox cmbUser, cmbServer;
    private TextBox txtProjectName, txtComponent, txtmanagerID, txtPassword;
    private Button btnOK, btnCancel;

    private Dictionary<string, string> servers;
    private Dictionary<string, List<(string ProjectName, string Component)>> project;
    private List<GeneralConfig> generalConfigs;

    public string SelectedUser { get; private set; }
    public string SelectedServer { get; private set; }
    public string ProjectName { get; private set; }
    public string Component { get; private set; }
    public string ManagerID { get; private set; }
    public string ManagerPassword { get; private set; }

    //public AddProjectForm(
    //    Dictionary<string, string> serverList,
    //    Dictionary<string, List<(string ProjectName, string Component)>> projectList,
    //    List<GeneralConfig> generalConfigs)
    //{
    //    this.servers = serverList ?? new Dictionary<string, string>();
    //    this.project = projectList ?? new Dictionary<string, List<(string ProjectName, string Component)>>();
    //    this.generalConfigs = generalConfigs ?? new List<GeneralConfig>();

    //    this.Text = "Append Project";
    //    this.Size = new Size(400, 420);
    //    this.StartPosition = FormStartPosition.CenterParent;
    //    this.FormBorderStyle = FormBorderStyle.FixedDialog;
    //    this.MaximizeBox = false;

    //    Label lblUser = new Label() { Text = "Select Octoplant User:", Left = 20, Top = 20, Width = 150 };
    //    cmbUser = new ComboBox() { Left = 180, Top = 20, Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
    //    if (generalConfigs.Count > 0) cmbUser.Items.AddRange(generalConfigs.Select(x => x.OctoplantUser).ToArray());
    //    else cmbUser.Items.Add("(No users available)");
    //    cmbUser.SelectedIndexChanged += (s, e) => { SelectedUser = cmbUser.SelectedItem?.ToString(); };

    //    Label lblServer = new Label() { Text = "Select Server:", Left = 20, Top = 70, Width = 150 };
    //    cmbServer = new ComboBox() { Left = 180, Top = 70, Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
    //    if (servers.Count > 0) cmbServer.Items.AddRange(servers.Keys.ToArray());
    //    else cmbServer.Items.Add("(No servers available)");

    //    Label lblProj = new Label() { Text = "Project Name:", Left = 20, Top = 120, Width = 120 };
    //    txtProjectName = new TextBox() { Left = 180, Top = 120, Width = 180 };

    //    Label lblComp = new Label() { Text = "Component:", Left = 20, Top = 160, Width = 120 };
    //    txtComponent = new TextBox() { Left = 180, Top = 160, Width = 180 };

    //    Label managerID = new Label() { Text = "Manager ID:", Left = 20, Top = 180, Width = 120 };
    //    txtmanagerID = new TextBox() { Left = 180, Top = 120, Width = 180 };

    //    btnCancel = new Button() { Text = "Cancel", Left = 80, Top = 220, Width = 100 };
    //    btnCancel.Click += (s, e) => this.Close();

    //    Label lblPassword = new Label() { Text = "Password:", Left = 20, Top = 200, Width = 120 };
    //    txtPassword = new TextBox()
    //    {
    //        Left = 120,
    //        Top = 12,
    //        Width = 180,
    //        UseSystemPasswordChar = true
    //    };

    //    btnOK = new Button() { Text = "OK", Left = 200, Top = 220, Width = 100, Enabled = false };
    //    btnOK.Click += BtnOK_Click;

    //    txtProjectName.TextChanged += ValidateInput;
    //    txtComponent.TextChanged += ValidateInput;
    //    cmbServer.SelectedIndexChanged += ValidateInput;

    //    Controls.AddRange(new Control[] { lblUser, cmbUser, lblServer, cmbServer, lblProj, txtProjectName, lblComp, txtComponent, managerID, txtmanagerID,lblPassword, txtPassword, btnCancel, btnOK });
    //}
    public AddProjectForm(
    Dictionary<string, string> serverList,
    Dictionary<string, List<(string ProjectName, string Component)>> projectList,
    List<GeneralConfig> generalConfigs)
    {
        this.servers = serverList ?? new Dictionary<string, string>();
        this.project = projectList ?? new Dictionary<string, List<(string ProjectName, string Component)>>();
        this.generalConfigs = generalConfigs ?? new List<GeneralConfig>();

        this.Text = "Append Project";
        this.Size = new Size(420, 420);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        int labelLeft = 20;
        int controlLeft = 180;
        int top = 20;
        int spacing = 40;

        // User
        Label lblUser = new Label() { Text = "Select Octoplant User:", Left = labelLeft, Top = top, Width = 150 };
        cmbUser = new ComboBox()
        {
            Left = controlLeft,
            Top = top,
            Width = 180,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        if (generalConfigs.Count > 0)
            cmbUser.Items.AddRange(generalConfigs.Select(x => x.OctoplantUser).ToArray());
        else
            cmbUser.Items.Add("(No users available)");

        cmbUser.SelectedIndexChanged += (s, e) =>
            SelectedUser = cmbUser.SelectedItem?.ToString();

        // Server
        top += spacing;
        Label lblServer = new Label() { Text = "Select Server:", Left = labelLeft, Top = top, Width = 150 };
        cmbServer = new ComboBox()
        {
            Left = controlLeft,
            Top = top,
            Width = 180,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        if (servers.Count > 0)
            cmbServer.Items.AddRange(servers.Keys.ToArray());
        else
            cmbServer.Items.Add("(No servers available)");

        // Project Name
        top += spacing;
        Label lblProj = new Label() { Text = "Project Name:", Left = labelLeft, Top = top, Width = 150 };
        txtProjectName = new TextBox() { Left = controlLeft, Top = top, Width = 180 };

        // Component
        top += spacing;
        Label lblComp = new Label() { Text = "Component:", Left = labelLeft, Top = top, Width = 150 };
        txtComponent = new TextBox() { Left = controlLeft, Top = top, Width = 180 };

        // Manager ID
        top += spacing;
        Label lblManagerID = new Label() { Text = "Manager ID:", Left = labelLeft, Top = top, Width = 150 };
        txtmanagerID = new TextBox() { Left = controlLeft, Top = top, Width = 180 };

        // Password
        top += spacing;
        Label lblPassword = new Label() { Text = "Password:", Left = labelLeft, Top = top, Width = 150 };
        txtPassword = new TextBox()
        {
            Left = controlLeft,
            Top = top,
            Width = 180,
            UseSystemPasswordChar = true
        };

        // Buttons
        btnCancel = new Button()
        {
            Text = "Cancel",
            Left = 90,
            Top = top + 60,
            Width = 100
        };
        btnCancel.Click += (s, e) => this.Close();

        btnOK = new Button()
        {
            Text = "OK",
            Left = 210,
            Top = top + 60,
            Width = 100,
            Enabled = false
        };
        btnOK.Click += BtnOK_Click;

        txtProjectName.TextChanged += ValidateInput;
        txtComponent.TextChanged += ValidateInput;
        cmbServer.SelectedIndexChanged += ValidateInput;
        txtmanagerID.TextChanged += ValidateInput;
        txtPassword.TextChanged += ValidateInput;
        Controls.AddRange(new Control[]
        {
        lblUser, cmbUser,
        lblServer, cmbServer,
        lblProj, txtProjectName,
        lblComp, txtComponent,
        lblManagerID, txtmanagerID,
        lblPassword, txtPassword,
        btnCancel, btnOK
        });
    }

    private void ValidateInput(object sender, EventArgs e)
    {
        btnOK.Enabled = (cmbServer.SelectedIndex >= 0 &&
                         !string.IsNullOrWhiteSpace(txtProjectName.Text) &&
                         !string.IsNullOrWhiteSpace(txtComponent.Text));
    }

    private void BtnOK_Click(object sender, EventArgs e)
    {
        SelectedUser = cmbUser.SelectedItem?.ToString();
        SelectedServer = cmbServer.SelectedItem?.ToString();
        ProjectName = txtProjectName.Text.Trim();
        Component = txtComponent.Text.Trim();
        ManagerID = txtmanagerID.Text.Trim();
        ManagerPassword = txtPassword.Text.Trim();
        if (string.IsNullOrEmpty(SelectedUser) || string.IsNullOrEmpty(SelectedServer) ||
            string.IsNullOrEmpty(ProjectName) || string.IsNullOrEmpty(Component) || string.IsNullOrEmpty(ManagerID) || string.IsNullOrEmpty(ManagerPassword))
        {
            MessageBox.Show("Please fill all fields.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Call JSON-based method to add user–server–project relationship
        TiaPortalJsonStorage.AddUserProject(SelectedUser, SelectedServer, ProjectName, Component, ManagerID, ManagerPassword);

        MessageBox.Show(
            $"Project '{ProjectName}' added for user '{SelectedUser}' on server '{SelectedServer}'.",
            "Success",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        this.DialogResult = DialogResult.OK;
        this.Close();
    }


}



    public class AddServerForm : Form
    {
        private TextBox txtServerName, txtServerUrl;
        private Button btnOK, btnCancel;

        private Dictionary<string, string> servers;
        private Dictionary<string, List<(string ProjectName, string Component)>> project;

        public string ServerName { get; private set; }
        public string ServerUrl { get; private set; }

        public AddServerForm(Dictionary<string, string> serverList, Dictionary<string, List<(string ProjectName, string Component)>> projectList)
        {
            this.servers = serverList ?? new Dictionary<string, string>();
            this.project = projectList ?? new Dictionary<string, List<(string ProjectName, string Component)>>();

            this.Text = "Add Server";
            this.Size = new Size(380, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblServerName = new Label() { Text = "Server Name:", Left = 20, Top = 30, Width = 120 };
            txtServerName = new TextBox() { Left = 150, Top = 25, Width = 190 };

            Label lblServerUrl = new Label() { Text = "Server URL:", Left = 20, Top = 75, Width = 120 };
            txtServerUrl = new TextBox() { Left = 150, Top = 70, Width = 190 };

            btnCancel = new Button() { Text = "Cancel", Left = 70, Top = 130, Width = 100 };
            btnCancel.Click += (s, e) => this.Close();

            btnOK = new Button() { Text = "Add Server", Left = 200, Top = 130, Width = 100, Enabled = false };
            btnOK.Click += BtnOK_Click;

            txtServerName.TextChanged += ValidateInput;
            txtServerUrl.TextChanged += ValidateInput;

            Controls.AddRange(new Control[] { lblServerName, txtServerName, lblServerUrl, txtServerUrl, btnCancel, btnOK });
        }

        private void ValidateInput(object sender, EventArgs e)
        {
            btnOK.Enabled = !string.IsNullOrWhiteSpace(txtServerName.Text) && !string.IsNullOrWhiteSpace(txtServerUrl.Text);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            ServerName = txtServerName.Text.Trim();
            ServerUrl = txtServerUrl.Text.Trim();

            // Validate duplicate Server Name
            if (servers.ContainsKey(ServerName))
            {
                MessageBox.Show($"Server name '{ServerName}' already exists!", "Duplicate Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate duplicate Server URL
            if (servers.Values.Any(url => string.Equals(url, ServerUrl, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"Server URL '{ServerUrl}' is already assigned to another server!", "Duplicate URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Add new server to JSON
            TiaPortalJsonStorage.AddOrUpdateServer(ServerName, ServerUrl);
            MainForm.serverList = TiaPortalJsonStorage.GetAllServersAsDictionary();

            if (!project.ContainsKey(ServerName))
                project[ServerName] = new List<(string ProjectName, string Component)>();

            MessageBox.Show($"Server '{ServerName}' added successfully and saved to JSON!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }


public class ModifyServerForm : Form
{
    private ComboBox cmbServers;
    private TextBox txtName, txtUrl;
    private Button btnUpdate;
    private Dictionary<string, string> servers;

    public ModifyServerForm(Dictionary<string, string> serverList)
    {
        servers = serverList;
        this.Text = "Modify Server";
        this.Size = new Size(420, 300);
        this.StartPosition = FormStartPosition.CenterParent;

        Label lblSelect = new Label() { Text = "Select Server:", Left = 30, Top = 30, Width = 100 };
        cmbServers = new ComboBox() { Left = 140, Top = 30, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbServers.Items.AddRange(servers.Keys.ToArray());
        cmbServers.SelectedIndexChanged += (s, e) =>
        {
            if (cmbServers.SelectedItem != null)
            {
                string selected = cmbServers.SelectedItem.ToString();
                txtName.Text = selected;
                txtUrl.Text = servers[selected];
            }
        };

        Label lblName = new Label() { Text = "New Name:", Left = 30, Top = 90, Width = 100 };
        txtName = new TextBox() { Left = 140, Top = 90, Width = 220 };

        Label lblUrl = new Label() { Text = "New URL:", Left = 30, Top = 140, Width = 100 };
        txtUrl = new TextBox() { Left = 140, Top = 140, Width = 220 };

        btnUpdate = new Button() { Text = "Update", Left = 140, Top = 190, Width = 100 };
        btnUpdate.Click += BtnUpdate_Click;

        Controls.AddRange(new Control[] { lblSelect, cmbServers, lblName, txtName, lblUrl, txtUrl, btnUpdate });
    }

    private void BtnUpdate_Click(object sender, EventArgs e)
    {
        if (cmbServers.SelectedItem == null)
        {
            MessageBox.Show("Please select a server to modify.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string oldName = cmbServers.SelectedItem.ToString();
        string newName = string.IsNullOrWhiteSpace(txtName.Text) ? oldName : txtName.Text.Trim();
        string newUrl = string.IsNullOrWhiteSpace(txtUrl.Text) ? servers[oldName] : txtUrl.Text.Trim();

        // Prevent renaming to an existing name (different from old)
        if (!oldName.Equals(newName, StringComparison.OrdinalIgnoreCase) && servers.ContainsKey(newName))
        {
            MessageBox.Show($"Server name '{newName}' already exists!", "Duplicate Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Prevent setting URL that belongs to another server
        if (servers.Any(s => !s.Key.Equals(oldName, StringComparison.OrdinalIgnoreCase)
                          && string.Equals(s.Value, newUrl, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show($"Server URL '{newUrl}' is already assigned to another server!", "Duplicate URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Proceed with rename or update
        if (!oldName.Equals(newName, StringComparison.OrdinalIgnoreCase))
        {
            TiaPortalJsonStorage.RenameServer(oldName, newName, newUrl);
        }
        else
        {
            TiaPortalJsonStorage.AddOrUpdateServer(oldName, newUrl);
        }

        // Refresh lists
        MainForm.serverList = TiaPortalJsonStorage.GetAllServersAsDictionary();
        MainForm.projectList = TiaPortalJsonStorage.GetProjectsByServer();

        MessageBox.Show("Server updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.Close();
    }
}

public class DeleteServerForm : Form
{
    private ComboBox cmbServers;
    private Button btnDelete;
    private Dictionary<string, string> servers;

    public DeleteServerForm(Dictionary<string, string> serverList)
    {
        servers = serverList;
        this.Text = "Delete Server";
        this.Size = new Size(400, 200);
        this.StartPosition = FormStartPosition.CenterParent;

        Label lblSelect = new Label() { Text = "Select Server:", Left = 30, Top = 30, Width = 100 };
        cmbServers = new ComboBox() { Left = 140, Top = 30, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbServers.Items.AddRange(servers.Keys.ToArray());

        btnDelete = new Button() { Text = "Delete", Left = 140, Top = 80, Width = 100 };
        btnDelete.Click += (s, e) =>
        {
            if (cmbServers.SelectedItem == null)
            {
                MessageBox.Show("Please select a server to delete.");
                return;
            }

            string selected = cmbServers.SelectedItem.ToString();
            TiaPortalJsonStorage.DeleteServer(selected);

            // Refresh mirrors
            MainForm.serverList = TiaPortalJsonStorage.GetAllServersAsDictionary();
            MainForm.projectList = TiaPortalJsonStorage.GetProjectsByServer();

            MessageBox.Show("Server deleted successfully!");
            this.Close();
        };

        Controls.AddRange(new Control[] { lblSelect, cmbServers, btnDelete });
    }
}


    public class GeneralConfigForm : Form
    {
        private TextBox txtVersion, txtInterval, txtProgram, txtClientArchive, txtUser,
                        txtPassword, txtDBFolder, txtCompare, txtReleaseAfter, txtDomain;

        // Public properties for parent form access
        public string Version { get; private set; }
        public string Interval { get; private set; }
        public string OctoplantProgram { get; private set; }
        public string OctoplantClientArchive { get; private set; }
        public string OctoplantUser { get; private set; }
        public string OctoplantPassword { get; private set; }
        public string DBFolder { get; private set; }
        public string Compare { get; private set; }
        public string ReleaseAfter { get; private set; }
        public string OctoplantDomain { get; private set; }

        public GeneralConfigForm()
        {
            this.Text = "General Configuration";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.AutoScroll = true;

            int top = 20;
            int labelWidth = 150;
            int boxWidth = 200;
            int gap = 40;

            Label AddLabel(string text, int y)
            {
                var lbl = new Label()
                {
                    Text = text,
                    Left = 20,
                    Top = y + 5,
                    Width = labelWidth
                };
                this.Controls.Add(lbl);
                return lbl;
            }

            TextBox AddTextBox(int y, string defaultValue = "")
            {
                var txt = new TextBox()
                {
                    Left = 180,
                    Top = y,
                    Width = boxWidth,
                    Text = defaultValue
                };
                this.Controls.Add(txt);
                return txt;
            }

            AddLabel("Version:", top); txtVersion = AddTextBox(top, "1"); top += gap;
            AddLabel("Interval (In Minute):", top); txtInterval = AddTextBox(top, "5"); top += gap;
            AddLabel("Octoplant Program Folder:", top); txtProgram = AddTextBox(top, @"C:\Program Files (x86)\vdogClient"); top += gap;
            AddLabel("Octoplant Client Archive:", top); txtClientArchive = AddTextBox(top, @"C:\vdClientArchive"); top += gap;
            AddLabel("Octoplant User:", top); txtUser = AddTextBox(top, "s03007223"); top += gap;
            AddLabel("Octoplant Password:", top); txtPassword = AddTextBox(top); txtPassword.PasswordChar = '●'; top += gap;
            AddLabel("DB Folder:", top); txtDBFolder = AddTextBox(top, @"C:\DBFolder"); top += gap;
            AddLabel("Compare:", top); txtCompare = AddTextBox(top, "true"); top += gap;
            //AddLabel("Release After:", top); txtReleaseAfter = AddTextBox(top, "false"); top += gap;
            AddLabel("Octoplant Domain:", top); txtDomain = AddTextBox(top, "wipropari"); top += gap + 20;

            Button btnOK = new Button() { Text = "OK", Left = 180, Top = top, Width = 80 };
            btnOK.Click += BtnOK_Click;
            Button btnCancel = new Button() { Text = "Cancel", Left = 270, Top = top, Width = 80 };
            btnCancel.Click += (s, e) => this.Close();

            Controls.AddRange(new Control[] { btnOK, btnCancel });
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(txtUser.Text))
            {
                MessageBox.Show("Please enter Octoplant User.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter Octoplant Password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check for duplicate user in JSON
            var existingConfigs = TiaPortalJsonStorage.GetAllGeneralConfigs();
            bool userExists = existingConfigs.Any(c =>
                string.Equals(c.OctoplantUser, txtUser.Text.Trim(), StringComparison.OrdinalIgnoreCase));

            if (userExists)
            {
                MessageBox.Show($"User '{txtUser.Text}' already exists in configuration!", "Duplicate User", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Assign all fields
            Version = txtVersion.Text.Trim();
            Interval = txtInterval.Text.Trim();
            OctoplantProgram = txtProgram.Text.Trim();
            OctoplantClientArchive = txtClientArchive.Text.Trim();
            OctoplantUser = txtUser.Text.Trim();
            OctoplantPassword = txtPassword.Text.Trim();
            DBFolder = txtDBFolder.Text.Trim();
            Compare = txtCompare.Text.Trim();
            //ReleaseAfter = txtReleaseAfter.Text.Trim();
            OctoplantDomain = txtDomain.Text.Trim();

            // Close form successfully
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
}


public class ModifyGeneralConfigForm : Form
{
    private ComboBox cmbUser;
    private TextBox txtVersion, txtInterval, txtProgram, txtClientArchive,
                    txtPasswordOld, txtPasswordNew, txtDBFolder, txtCompare, txtReleaseAfter, txtDomain;
    private Button btnUpdate, btnCancel, btnChangePassword, btnToggleOldPwd, btnToggleNewPwd;
    private bool passwordChangeEnabled = false;
    private bool oldPwdVisible = false, newPwdVisible = false;

    private List<GeneralConfig> existingConfigs;

    public ModifyGeneralConfigForm()
    {
        this.Text = "Modify General Configuration";
        this.Size = new Size(430, 640);
        this.StartPosition = FormStartPosition.CenterParent;
        this.AutoScroll = true;

        int top = 20;
        int labelWidth = 180;
        int boxWidth = 200;
        int gap = 40;

        existingConfigs = TiaPortalJsonStorage.GetAllGeneralConfigs();

        // User Dropdown
        Label lblSelectUser = new Label() { Text = "Select Octoplant User:", Left = 20, Top = top + 5, Width = labelWidth };
        cmbUser = new ComboBox() { Left = 210, Top = top, Width = boxWidth, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbUser.Items.AddRange(existingConfigs.Select(c => c.OctoplantUser).ToArray());
        cmbUser.SelectedIndexChanged += CmbUser_SelectedIndexChanged;
        this.Controls.AddRange(new Control[] { lblSelectUser, cmbUser });

        top += gap;

        Label AddLabel(string text, int y)
        {
            var lbl = new Label() { Text = text, Left = 20, Top = y + 5, Width = labelWidth };
            this.Controls.Add(lbl);
            return lbl;
        }

        TextBox AddTextBox(int y)
        {
            var txt = new TextBox() { Left = 210, Top = y, Width = boxWidth };
            this.Controls.Add(txt);
            return txt;
        }

        AddLabel("Version:", top); txtVersion = AddTextBox(top); top += gap;
        AddLabel("Interval (In Minute):", top); txtInterval = AddTextBox(top); top += gap;
        AddLabel("Octoplant Program Folder:", top); txtProgram = AddTextBox(top); top += gap;
        AddLabel("Octoplant Client Archive:", top); txtClientArchive = AddTextBox(top); top += gap;
        AddLabel("Octoplant Domain:", top); txtDomain = AddTextBox(top); top += gap;
        AddLabel("DB Folder:", top); txtDBFolder = AddTextBox(top); top += gap;
        AddLabel("Compare (true/false):", top); txtCompare = AddTextBox(top); top += gap;
        //AddLabel("Release After (true/false):", top); txtReleaseAfter = AddTextBox(top); top += gap;
       // txtReleaseAfter.Text = "true";
        
        // Old Password
        // Old Password
        AddLabel("Password:", top);
        txtPasswordOld = new TextBox()
        {
            Left = 210,
            Top = top,
            Width = boxWidth - 30,
            PasswordChar = '●'
        };
        btnToggleOldPwd = new Button()
        {
            Text = "👁️",
            Left = 210 + boxWidth - 25,
            Top = top - 1,
            Width = 25,
            Height = 23
        };
        btnToggleOldPwd.Click += (s, e) =>
        {
            oldPwdVisible = !oldPwdVisible;
            txtPasswordOld.PasswordChar = oldPwdVisible ? '\0' : '●';
        };
        this.Controls.AddRange(new Control[] { txtPasswordOld, btnToggleOldPwd });
        top += gap;

        // New Password (fully hidden initially)
        Label lblNewPassword = new Label()
        {
            Text = "New Password:",
            Left = 20,
            Top = top + 5,
            Width = labelWidth,
            Visible = false
        };
        txtPasswordNew = new TextBox()
        {
            Left = 210,
            Top = top,
            Width = boxWidth - 30,
            PasswordChar = '●',
            Visible = false
        };
        btnToggleNewPwd = new Button()
        {
            Text = "👁️",
            Left = 210 + boxWidth - 25,
            Top = top - 1,
            Width = 25,
            Height = 23,
            Visible = false
        };
        btnToggleNewPwd.Click += (s, e) =>
        {
            newPwdVisible = !newPwdVisible;
            txtPasswordNew.PasswordChar = newPwdVisible ? '\0' : '●';
        };

        this.Controls.AddRange(new Control[] { lblNewPassword, txtPasswordNew, btnToggleNewPwd });
        top += gap;

        // Change Password toggle button
        btnChangePassword = new Button()
        {
            Text = "Change Password",
            Left = 210,
            Top = top - 10,
            Width = 150
        };
        btnChangePassword.Click += (s, e) =>
        {
            passwordChangeEnabled = !passwordChangeEnabled;
            lblNewPassword.Visible = passwordChangeEnabled;
            txtPasswordNew.Visible = passwordChangeEnabled;
            btnToggleNewPwd.Visible = passwordChangeEnabled;
            btnChangePassword.Text = passwordChangeEnabled ? "Cancel Change" : "Change Password";
        };
        this.Controls.Add(btnChangePassword);


        top += 50;

        // Update and Cancel buttons
        btnUpdate = new Button() { Text = "Update", Left = 210, Top = top, Width = 80 };
        btnUpdate.Click += BtnUpdate_Click;

        btnCancel = new Button() { Text = "Cancel", Left = 310, Top = top, Width = 80 };
        btnCancel.Click += (s, e) => this.Close();

        this.Controls.AddRange(new Control[] { btnUpdate, btnCancel });
    }

    private void CmbUser_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cmbUser.SelectedItem == null) return;
        string selectedUser = cmbUser.SelectedItem.ToString();

        var cfg = existingConfigs.FirstOrDefault(c => c.OctoplantUser.Equals(selectedUser, StringComparison.OrdinalIgnoreCase));
        if (cfg != null)
        {
            txtVersion.Text = cfg.Version;
            txtInterval.Text = cfg.Interval;
            txtProgram.Text = cfg.OctoplantProgram;
            txtClientArchive.Text = cfg.OctoplantClientArchive;
            txtDomain.Text = cfg.Domain;
            txtDBFolder.Text = cfg.DBFolder;
            txtCompare.Text = cfg.Compare;
            //txtReleaseAfter.Text = "false";// cfg.ReleaseAfter;

            txtPasswordOld.Clear();
            txtPasswordNew.Clear();
            txtPasswordNew.Visible = false;
            btnToggleNewPwd.Visible = false;
            passwordChangeEnabled = false;
            btnChangePassword.Text = "Change Password";
        }
    }

    private void BtnUpdate_Click(object sender, EventArgs e)
    {
        if (cmbUser.SelectedItem == null)
        {
            MessageBox.Show("Please select a user to modify.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string selectedUser = cmbUser.SelectedItem.ToString();
        var userConfig = existingConfigs.FirstOrDefault(c => c.OctoplantUser.Equals(selectedUser, StringComparison.OrdinalIgnoreCase));

        if (userConfig == null)
        {
            MessageBox.Show("Invalid user selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Require old password for all updates
        if (string.IsNullOrWhiteSpace(txtPasswordOld.Text))
        {
            MessageBox.Show("Please enter your password to apply changes.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (txtPasswordOld.Text != userConfig.OctoplantPassword)
        {
            MessageBox.Show("Old password is incorrect.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Password change logic (only if enabled)
        string newPassword = userConfig.OctoplantPassword;
        if (passwordChangeEnabled)
        {
            if (string.IsNullOrWhiteSpace(txtPasswordNew.Text))
            {
                MessageBox.Show("Please enter a new password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            newPassword = txtPasswordNew.Text.Trim();
        }

        // Update other fields
        userConfig.Version = txtVersion.Text.Trim();
        userConfig.Interval = txtInterval.Text.Trim();
        userConfig.OctoplantProgram = txtProgram.Text.Trim();
        userConfig.OctoplantClientArchive = txtClientArchive.Text.Trim();
        userConfig.Domain = txtDomain.Text.Trim();
        userConfig.DBFolder = txtDBFolder.Text.Trim();
        userConfig.Compare = txtCompare.Text.Trim();
        userConfig.ReleaseAfter = txtReleaseAfter.Text.Trim();
        userConfig.OctoplantPassword = newPassword;

        TiaPortalJsonStorage.AddOrUpdateGeneralConfig(userConfig);

        MessageBox.Show($"Configuration updated successfully for user '{selectedUser}'.", "Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.Close();
    }
}


public partial class ControlPopupForm : Form
{
    public event Action OnStopClicked;
    public event Action OnStartNewClicked;

    public ControlPopupForm()
    {
        this.Text = "Automation Control";
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.TopMost = true;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Width = 300;
        this.Height = 150;

        Button btnStop = new Button() { Text = "Stop", Width = 100, Height = 40, Left = 30, Top = 40 };
        btnStop.Click += (s, e) => OnStopClicked?.Invoke();

        Button btnStartNew = new Button() { Text = "Start New", Width = 100, Height = 40, Left = 150, Top = 40 };
        btnStartNew.Click += (s, e) => OnStartNewClicked?.Invoke();

        Controls.Add(btnStop);
        Controls.Add(btnStartNew);
    }
}
public class DeleteProjectForm : Form
{
    private ComboBox cmbUser, cmbServer, cmbProject;
    private Button btnDelete;
    private List<GeneralConfig> generalConfigs;
    private List<UserProjectMapping> userProjects;

    public DeleteProjectForm(Dictionary<string, List<(string ProjectName, string Component)>> _)
    {
        this.Text = "Delete Project (User-based)";
        this.Size = new Size(420, 300);
        this.StartPosition = FormStartPosition.CenterParent;

        // Load from JSON
        generalConfigs = TiaPortalJsonStorage.GetAllGeneralConfigs();
        userProjects = LoadUserProjects();

        // --- User selection ---
        Label lblUser = new Label() { Text = "Select User:", Left = 30, Top = 30, Width = 120 };
        cmbUser = new ComboBox()
        {
            Left = 160,
            Top = 30,
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbUser.Items.AddRange(generalConfigs.Select(g => g.OctoplantUser).ToArray());
        cmbUser.SelectedIndexChanged += CmbUser_SelectedIndexChanged;

        // --- Server selection ---
        Label lblServer = new Label() { Text = "Select Server:", Left = 30, Top = 70, Width = 120 };
        cmbServer = new ComboBox()
        {
            Left = 160,
            Top = 70,
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbServer.SelectedIndexChanged += CmbServer_SelectedIndexChanged;

        // --- Project selection ---
        Label lblProject = new Label() { Text = "Select Project:", Left = 30, Top = 110, Width = 120 };
        cmbProject = new ComboBox()
        {
            Left = 160,
            Top = 110,
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        // --- Delete button ---
        btnDelete = new Button() { Text = "Delete", Left = 160, Top = 160, Width = 100 };
        btnDelete.Click += BtnDelete_Click;

        Controls.AddRange(new Control[] { lblUser, cmbUser, lblServer, cmbServer, lblProject, cmbProject, btnDelete });
    }

    // Load UserProjects directly from JSON
    private List<UserProjectMapping> LoadUserProjects()
    {
        var loadMethod = typeof(TiaPortalJsonStorage)
            .GetMethod("Load", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var root = loadMethod?.Invoke(null, null) as dynamic;
        return root?.UserProjects != null
            ? ((IEnumerable<UserProjectMapping>)root.UserProjects).ToList()
            : new List<UserProjectMapping>();
    }

    private void CmbUser_SelectedIndexChanged(object sender, EventArgs e)
    {
        cmbServer.Items.Clear();
        cmbProject.Items.Clear();

        if (cmbUser.SelectedItem == null) return;

        string selectedUser = cmbUser.SelectedItem.ToString();

        // Get servers for this user only
        var serversForUser = userProjects
            .Where(up => up.OctoplantUser.Equals(selectedUser, StringComparison.OrdinalIgnoreCase))
            .Select(up => up.ServerName)
            .Distinct()
            .ToList();

        cmbServer.Items.AddRange(serversForUser.ToArray());
    }

    private void CmbServer_SelectedIndexChanged(object sender, EventArgs e)
    {
        cmbProject.Items.Clear();

        if (cmbUser.SelectedItem == null || cmbServer.SelectedItem == null) return;

        string selectedUser = cmbUser.SelectedItem.ToString();
        string selectedServer = cmbServer.SelectedItem.ToString();

        // Get projects for this user + server only
        var projects = userProjects
            .Where(up =>
                up.OctoplantUser.Equals(selectedUser, StringComparison.OrdinalIgnoreCase) &&
                up.ServerName.Equals(selectedServer, StringComparison.OrdinalIgnoreCase))
            .Select(up => up.ProjectName)
            .Distinct()
            .ToList();

        cmbProject.Items.AddRange(projects.ToArray());
    }

    private void BtnDelete_Click(object sender, EventArgs e)
    {
        if (cmbUser.SelectedItem == null || cmbServer.SelectedItem == null || cmbProject.SelectedItem == null)
        {
            MessageBox.Show("Please select a user, server, and project to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        string user = cmbUser.SelectedItem.ToString();
        string serverName = cmbServer.SelectedItem.ToString();
        string projectName = cmbProject.SelectedItem.ToString();

        // Load full root config
        var loadMethod = typeof(TiaPortalJsonStorage)
            .GetMethod("Load", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var root = loadMethod?.Invoke(null, null) as dynamic;
        if (root == null)
        {
            MessageBox.Show("Failed to load configuration JSON.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Remove from UserProjects only for this user + server + project
        ((List<UserProjectMapping>)root.UserProjects).RemoveAll(x =>
            x.OctoplantUser.Equals(user, StringComparison.OrdinalIgnoreCase) &&
            x.ServerName.Equals(serverName, StringComparison.OrdinalIgnoreCase) &&
            x.ProjectName.Equals(projectName, StringComparison.OrdinalIgnoreCase));

        // Remove from server project catalog
        TiaPortalJsonStorage.DeleteProject(serverName, projectName);

        // Save updated JSON correctly
        var saveMethod = typeof(TiaPortalJsonStorage)
            .GetMethod("Save", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        saveMethod?.Invoke(null, new object[] { root });

        // Refresh memory mirrors
        MainForm.projectList = TiaPortalJsonStorage.GetProjectsByServer();

        MessageBox.Show($"Project '{projectName}' deleted for user '{user}' on server '{serverName}'.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);

        // Remove from UI
        cmbProject.Items.Remove(projectName);
    }
}

public class ModifyProjectForm : Form
{
    private ComboBox cmbUser, cmbServer, cmbProject;
    private TextBox txtNewName, txtNewComponent;
    private Button btnUpdate;
    private List<GeneralConfig> generalConfigs;
    private List<UserProjectMapping> userProjects;

    public ModifyProjectForm(Dictionary<string, List<(string ProjectName, string Component)>> _)
    {
        this.Text = "Modify Project (User-based)";
        this.Size = new Size(450, 360);
        this.StartPosition = FormStartPosition.CenterParent;

        // Load from JSON directly
        generalConfigs = TiaPortalJsonStorage.GetAllGeneralConfigs();
        userProjects = LoadUserProjects();

        // --- User selection ---
        Label lblUser = new Label() { Text = "Select User:", Left = 30, Top = 30, Width = 120 };
        cmbUser = new ComboBox()
        {
            Left = 160,
            Top = 30,
            Width = 220,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbUser.Items.AddRange(generalConfigs.Select(g => g.OctoplantUser).ToArray());
        cmbUser.SelectedIndexChanged += CmbUser_SelectedIndexChanged;

        // --- Server selection ---
        Label lblServer = new Label() { Text = "Select Server:", Left = 30, Top = 70, Width = 120 };
        cmbServer = new ComboBox()
        {
            Left = 160,
            Top = 70,
            Width = 220,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbServer.SelectedIndexChanged += CmbServer_SelectedIndexChanged;

        // --- Project selection ---
        Label lblProject = new Label() { Text = "Select Project:", Left = 30, Top = 110, Width = 120 };
        cmbProject = new ComboBox()
        {
            Left = 160,
            Top = 110,
            Width = 220,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbProject.SelectedIndexChanged += CmbProject_SelectedIndexChanged;

        // --- Editable fields ---
        Label lblNewName = new Label() { Text = "New Project Name:", Left = 30, Top = 150, Width = 120 };
        txtNewName = new TextBox() { Left = 160, Top = 150, Width = 220 };

        Label lblNewComponent = new Label() { Text = "New Component:", Left = 30, Top = 190, Width = 120 };
        txtNewComponent = new TextBox() { Left = 160, Top = 190, Width = 220 };

        // --- Update Button ---
        btnUpdate = new Button() { Text = "Update", Left = 160, Top = 240, Width = 100 };
        btnUpdate.Click += BtnUpdate_Click;

        Controls.AddRange(new Control[]
        {
            lblUser, cmbUser,
            lblServer, cmbServer,
            lblProject, cmbProject,
            lblNewName, txtNewName,
            lblNewComponent, txtNewComponent,
            btnUpdate
        });
    }

    // Load UserProjects directly from JSON
    private List<UserProjectMapping> LoadUserProjects()
    {
        var loadMethod = typeof(TiaPortalJsonStorage)
            .GetMethod("Load", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var root = loadMethod?.Invoke(null, null) as dynamic;
        return root?.UserProjects != null
            ? ((IEnumerable<UserProjectMapping>)root.UserProjects).ToList()
            : new List<UserProjectMapping>();
    }

    private void CmbUser_SelectedIndexChanged(object sender, EventArgs e)
    {
        cmbServer.Items.Clear();
        cmbProject.Items.Clear();
        txtNewName.Clear();
        txtNewComponent.Clear();

        if (cmbUser.SelectedItem == null) return;

        string selectedUser = cmbUser.SelectedItem.ToString();
        var serversForUser = userProjects
            .Where(up => up.OctoplantUser.Equals(selectedUser, StringComparison.OrdinalIgnoreCase))
            .Select(up => up.ServerName)
            .Distinct()
            .ToList();

        cmbServer.Items.AddRange(serversForUser.ToArray());
    }

    private void CmbServer_SelectedIndexChanged(object sender, EventArgs e)
    {
        cmbProject.Items.Clear();
        txtNewName.Clear();
        txtNewComponent.Clear();

        if (cmbUser.SelectedItem == null || cmbServer.SelectedItem == null) return;

        string selectedUser = cmbUser.SelectedItem.ToString();
        string selectedServer = cmbServer.SelectedItem.ToString();

        var projectsForUserServer = userProjects
            .Where(up =>
                up.OctoplantUser.Equals(selectedUser, StringComparison.OrdinalIgnoreCase) &&
                up.ServerName.Equals(selectedServer, StringComparison.OrdinalIgnoreCase))
            .Select(up => up.ProjectName)
            .Distinct()
            .ToList();

        cmbProject.Items.AddRange(projectsForUserServer.ToArray());
    }

    private void CmbProject_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cmbUser.SelectedItem == null || cmbServer.SelectedItem == null || cmbProject.SelectedItem == null) return;

        string user = cmbUser.SelectedItem.ToString();
        string server = cmbServer.SelectedItem.ToString();
        string project = cmbProject.SelectedItem.ToString();

        var entry = userProjects.FirstOrDefault(up =>
            up.OctoplantUser.Equals(user, StringComparison.OrdinalIgnoreCase) &&
            up.ServerName.Equals(server, StringComparison.OrdinalIgnoreCase) &&
            up.ProjectName.Equals(project, StringComparison.OrdinalIgnoreCase));

        if (entry != null)
        {
            txtNewName.Text = entry.ProjectName;
            txtNewComponent.Text = entry.Component;
        }
    }

    private void BtnUpdate_Click(object sender, EventArgs e)
    {
        if (cmbUser.SelectedItem == null || cmbServer.SelectedItem == null || cmbProject.SelectedItem == null)
        {
            MessageBox.Show("Please select User, Server, and Project.", "Error");
            return;
        }

        string user = cmbUser.SelectedItem.ToString();
        string server = cmbServer.SelectedItem.ToString();
        string oldProjectName = cmbProject.SelectedItem.ToString();
        string newProjectName = string.IsNullOrWhiteSpace(txtNewName.Text) ? oldProjectName : txtNewName.Text.Trim();
        string newComponent = string.IsNullOrWhiteSpace(txtNewComponent.Text) ? "" : txtNewComponent.Text.Trim();

        // Load actual root object (correct type)
        var loadMethod = typeof(TiaPortalJsonStorage)
            .GetMethod("Load", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var root = loadMethod?.Invoke(null, null) as dynamic;
        if (root == null)
        {
            MessageBox.Show("Failed to load configuration JSON.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Update in UserProjects (user-based)
        var existing = ((List<UserProjectMapping>)root.UserProjects).FirstOrDefault(up =>
            up.OctoplantUser.Equals(user, StringComparison.OrdinalIgnoreCase) &&
            up.ServerName.Equals(server, StringComparison.OrdinalIgnoreCase) &&
            up.ProjectName.Equals(oldProjectName, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            existing.ProjectName = newProjectName;
            existing.Component = newComponent;
        }

        // Also update in Servers list (Projects)
        TiaPortalJsonStorage.DeleteProject(server, oldProjectName);
        TiaPortalJsonStorage.AddOrUpdateProject(server, new ProjectInfo
        {
            ProjectName = newProjectName,
            Component = newComponent
        });

        // Save back correctly using real type
        var saveMethod = typeof(TiaPortalJsonStorage)
            .GetMethod("Save", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        saveMethod?.Invoke(null, new object[] { root });

        MessageBox.Show($"Updated project '{newProjectName}' for user '{user}' on server '{server}'.",
            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

        this.Close();
    }
}
public class DeleteGeneralConfigForm : Form
{
    private ComboBox cmbUser;
    private Button btnDelete, btnCancel;
    private List<GeneralConfig> generalConfigs;

    public DeleteGeneralConfigForm()
    {
        this.Text = "Delete General Config";
        this.Size = new Size(400, 220);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        generalConfigs = TiaPortalJsonStorage.GetAllGeneralConfigs();

        Label lblUser = new Label
        {
            Text = "Select Octoplant User:",
            Left = 30,
            Top = 40,
            Width = 150
        };

        cmbUser = new ComboBox
        {
            Left = 190,
            Top = 35,
            Width = 160,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbUser.Items.AddRange(generalConfigs.Select(c => c.OctoplantUser).ToArray());

        btnDelete = new Button { Text = "Delete", Left = 190, Top = 90, Width = 80 };
        btnDelete.Click += BtnDelete_Click;

        btnCancel = new Button { Text = "Cancel", Left = 280, Top = 90, Width = 80 };
        btnCancel.Click += (s, e) => this.Close();

        Controls.AddRange(new Control[] { lblUser, cmbUser, btnDelete, btnCancel });
    }

    private void BtnDelete_Click(object sender, EventArgs e)
    {
        if (cmbUser.SelectedItem == null)
        {
            MessageBox.Show("Please select a user to delete.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string user = cmbUser.SelectedItem.ToString();

        var result = MessageBox.Show(
            $"Are you sure you want to delete all data for user '{user}'?\nThis will also remove all linked UserProjects.",
            "Confirm Deletion",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes) return;

        try
        {
            // Step 1: Define paths
            string configFile = TiaPortalJsonStorage.ConfigJsonFile;
            string configDir = Path.GetDirectoryName(configFile) ?? AppDomain.CurrentDomain.BaseDirectory;
            string backupFile = Path.Combine(configDir, "BackupConfigData.json");

            // Step 2: Load JSON
            var root = TiaPortalJsonStorage.LoadFullJson();
            if (root == null)
            {
                MessageBox.Show("Failed to load JSON configuration file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Step 3: Build backup slice for this user
            var userBackup = new TiaPortalJsonStorage.RootConfigJson
            {
                GeneralConfigs = root.GeneralConfigs
                    .Where(g => g.OctoplantUser.Equals(user, StringComparison.OrdinalIgnoreCase))
                    .ToList(),
                UserProjects = root.UserProjects
                    .Where(p => p.OctoplantUser.Equals(user, StringComparison.OrdinalIgnoreCase))
                    .ToList(),
                Servers = new List<ServerInfo>()
            };

            // Step 4: Merge into backup file
            TiaPortalJsonStorage.RootConfigJson backupRoot;

            if (File.Exists(backupFile))
            {
                try
                {
                    var existingBackupJson = File.ReadAllText(backupFile);
                    backupRoot = JsonConvert.DeserializeObject<TiaPortalJsonStorage.RootConfigJson>(existingBackupJson)
                                 ?? new TiaPortalJsonStorage.RootConfigJson();
                }
                catch
                {
                    backupRoot = new TiaPortalJsonStorage.RootConfigJson();
                }
            }
            else
            {
                backupRoot = new TiaPortalJsonStorage.RootConfigJson();
            }

            // Merge without duplicates
            foreach (var g in userBackup.GeneralConfigs)
            {
                if (!backupRoot.GeneralConfigs.Any(x =>
                    x.OctoplantUser.Equals(g.OctoplantUser, StringComparison.OrdinalIgnoreCase)))
                {
                    backupRoot.GeneralConfigs.Add(g);
                }
            }

            foreach (var p in userBackup.UserProjects)
            {
                bool exists = backupRoot.UserProjects.Any(x =>
                    x.OctoplantUser.Equals(p.OctoplantUser, StringComparison.OrdinalIgnoreCase) &&
                    x.ServerName.Equals(p.ServerName, StringComparison.OrdinalIgnoreCase) &&
                    x.ProjectName.Equals(p.ProjectName, StringComparison.OrdinalIgnoreCase));

                if (!exists)
                    backupRoot.UserProjects.Add(p);
            }

            // Step 5: Save merged backup
            File.WriteAllText(backupFile, JsonConvert.SerializeObject(backupRoot, Newtonsoft.Json.Formatting.Indented));

            // Step 6: Remove user data from main config
            int beforeUsers = root.GeneralConfigs.Count;
            int beforeProjects = root.UserProjects.Count;

            root.GeneralConfigs.RemoveAll(g => g.OctoplantUser.Equals(user, StringComparison.OrdinalIgnoreCase));
            root.UserProjects.RemoveAll(p => p.OctoplantUser.Equals(user, StringComparison.OrdinalIgnoreCase));

            // Save updated main file (use fully qualified Formatting)
            File.WriteAllText(configFile, JsonConvert.SerializeObject(root, Newtonsoft.Json.Formatting.Indented));

            // Step 7: Refresh main UI data
            MainForm.generalConfigs = TiaPortalJsonStorage.GetAllGeneralConfigs();
            MainForm.serverList = TiaPortalJsonStorage.GetAllServersAsDictionary();
            MainForm.projectList = TiaPortalJsonStorage.GetProjectsByServer();

            int removedUsers = beforeUsers - root.GeneralConfigs.Count;
            int removedProjects = beforeProjects - root.UserProjects.Count;

            MessageBox.Show(
                $"User '{user}' deleted successfully.\n" +
                $"Merged backup saved to:\n{backupFile}\n\n" +
                $"Removed {removedUsers} GeneralConfig entry and {removedProjects} linked UserProjects.",
                "Deleted",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error while deleting configuration: " + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
