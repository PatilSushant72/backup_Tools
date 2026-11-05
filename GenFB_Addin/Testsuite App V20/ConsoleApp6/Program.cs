using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using FlaUI.UIA3;
using Microsoft.Office.Interop.Excel;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Siemens.Engineering;
using Siemens.Engineering;
using Siemens.Engineering;
using Siemens.Engineering;
using Siemens.Engineering.Cax;
using Siemens.Engineering.Compiler;
using Siemens.Engineering.Compiler;  // For CompilerResult, CompilerMessageType
using Siemens.Engineering.Connection;
using Siemens.Engineering.Download;
using Siemens.Engineering.Download.Configurations;
using Siemens.Engineering.HmiUnified.HmiTags;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.HW.Utilities; // for GetAvailableDevices
using Siemens.Engineering.Online;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.ExternalSources;
using Siemens.Engineering.TestSuite;
using Siemens.Engineering.TestSuite.ApplicationTest;
using Siemens.Simatic.Simulation.Runtime;
using Siemens.Simatic.Simulation.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.IO;
using System.Linq;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static TiaPortalAutomation;
using Application = System.Windows.Forms.Application;
using Button = System.Windows.Forms.Button;
using FlaUIButton = FlaUI.Core.AutomationElements.Button;
using FlaUITextBox = FlaUI.Core.AutomationElements.TextBox;
using Font = System.Drawing.Font;
using Label = System.Windows.Forms.Label;
using Point = System.Drawing.Point;
using Range = Microsoft.Office.Interop.Excel.Range;
using WinForms = System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.IO.Packaging;

class TiaPortalAutomation
{
    static List<string> compileResults = new List<string>();
    static List<string> humanLoopMessages = new List<string>();
    static Dictionary<string, DateTime> uploadTimes = new Dictionary<string, DateTime>();

    static string mainOutputFolder = @"C:\Development\ProgrammingLanguage\OutputFiles";
    static string selectedProjectPath = null;
    private static string mainfile;
    static string selectedProject = null;
    static string selectedCaxPath = null;
    static string selectedCaxPath1 = null;
    static string awlFilesDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Downloads",
        "AWL Test Download"
    );
    static string logFilePath = Path.Combine(awlFilesDirectory, "UploadReport.txt");
    public class BlockTTSTMapping
    {
        public string BlockName { get; set; }
        public string TTSTFilePath { get; set; }
        public System.Windows.Forms.CheckBox BlockCheckBox { get; set; }
    }

    public class BlockSelectionForm : Form
    {
        private List<BlockTTSTMapping> blockMappings = new List<BlockTTSTMapping>();
        public List<BlockTTSTMapping> SelectedBlockMappings => blockMappings
            .Where(b => b.BlockCheckBox.Checked && !string.IsNullOrEmpty(b.TTSTFilePath))
            .ToList();

        private Button btnOk;
        private Button btnCancel;

        public BlockSelectionForm(List<string> blockNames)
        {
            this.Text = "Select Blocks and TTST Files";
            this.Width = 700;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                ColumnCount = 2,
                Padding = new Padding(10),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                AutoSize = true
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            foreach (var block in blockNames)
            {
                var mapping = new BlockTTSTMapping { BlockName = block };
                blockMappings.Add(mapping);

                var chk = new System.Windows.Forms.CheckBox
                {
                    Text = block,
                    Width = 450,
                    AutoSize = false,
                    Tag = mapping
                };
                mapping.BlockCheckBox = chk;

                var btnTTST = new Button
                {
                    Text = "➕ TTST",
                    Width = 90,
                    Height = 28,
                    Tag = mapping,
                    BackColor = Color.LightSteelBlue,
                    FlatStyle = FlatStyle.Flat,
                    Margin = new Padding(10, 5, 5, 5)
                };

                btnTTST.Click += (s, e) =>
                {
                    using (var ofd = new OpenFileDialog())
                    {
                        ofd.Title = $"Select TTST for {block}";
                        ofd.Filter = "TTST Files (*.txt)|*.txt|All Files (*.*)|*.*";
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            mapping.TTSTFilePath = ofd.FileName;
                            chk.Checked = true;
                            chk.Text = $"{block}   (TTST Selected)";
                        }
                    }
                };
                
                table.RowCount += 1;
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
                table.Controls.Add(chk, 0, table.RowCount - 1);
                table.Controls.Add(btnTTST, 1, table.RowCount - 1);
            }

            btnOk = new Button
            {
                Text = "✅ OK",
                Width = 100,
                Height = 35,
                BackColor = Color.MediumSeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOk.Click += BtnOk_Click;

            btnCancel = new Button
            {
                Text = "❌ Cancel",
                Width = 100,
                Height = 35,
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            var footer = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(10),
                BackColor = Color.WhiteSmoke
            };

            footer.Controls.Add(btnOk);
            footer.Controls.Add(btnCancel);

            this.Controls.Add(table);
            this.Controls.Add(footer);
        }
        private void BtnOk_Click(object sender, EventArgs e)
        {
            var missing = blockMappings
                .Where(m => m.BlockCheckBox.Checked && string.IsNullOrEmpty(m.TTSTFilePath))
                .Select(m => m.BlockName)
                .ToList();

            if (missing.Count > 0)
            {
                MessageBox.Show("Please select TTST file for:\n\n" + string.Join("\n", missing),
                    "Missing TTST Files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
    static string excelForTest = "";
    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            //xmlPath.Add(@"C:\Users\Wpuser\Downloads\PR_GantryUnload_Seqv25_57af70.xml");
            //xmlPath.Add(@"C:\Users\Wpuser\Downloads\Final\Generate Block\XML Files\PR_PC4005_Operator_DecisionV4_be2ac0_3.xml");                                          
            //xmlPath.Add(@"C:\Users\Wpuser\Downloads\Final\Generate Block\XML Files\S31808022001_Sequence_All_73b5c5_cfdfd9eb-ebd8-4267-a68e-df0ebcea3fa8_7a6274_3.xml");  
            string copyProjPath = "";
            List<string> blockNames = new List<string>();
            excelForTest = "true"; /*args[2];*/
            //updateVersion();
            //blockCalling(xmlPath);

            //createProject(copyProjPath, blockNames);

            if ("XML Testing" == "XML Testing")
            {
                createProject(copyProjPath, blockNames);
            }
            else if ("BLock Testing" == "BLock Testing")
            {
            }
            CleanupUpgradeFolders();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static void CleanupUpgradeFolders()
    {
        string parent = @"C:\Generate Block";
        if (!Directory.Exists(parent))
            return;

        var candidates = Directory.GetDirectories(parent, "V20 Project*",
            SearchOption.TopDirectoryOnly);

        foreach (var dir in candidates)
        {
            try
            {   
                if (string.Equals(Path.GetFullPath(dir).TrimEnd('\\'),
                                  Path.GetFullPath(@"C:\Generate Block\V20 Project").TrimEnd('\\'),
                                  StringComparison.OrdinalIgnoreCase))
                    continue;

                for (int attempt = 1; attempt <= 3; attempt++)
                {
                    try
                    {
                        Directory.Delete(dir, true);
                        Console.WriteLine($"Deleted upgrade/backup folder: {dir}");
                        break;
                    }
                    catch (IOException) when (attempt < 3)
                    {
                        Thread.Sleep(500);
                    }
                    catch (UnauthorizedAccessException) when (attempt < 3)
                    {
                        Thread.Sleep(500);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete '{dir}': {ex.Message}");
            }
        }
    }

    static List<string> tempCsv = new List<string>();
    public static string exportblock(Project proj, string blockname, Device dev)
    {
        string exportPath = "C:\\Generate Block\\V20 All Files\\Export Files";        
        
        if (!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }

        var softwareContainer = dev.DeviceItems[1].GetService<SoftwareContainer>();
        var plcSoftware = softwareContainer?.Software as PlcSoftware;

        var compiler = plcSoftware.GetService<ICompilable>();
        var result = compiler.Compile();

        bool hasErrors = result.Messages.Any(msg =>
            msg != null &&
            msg.ToString().IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0);

        if (hasErrors)
        {
            MessageBox.Show($"Compilation failed. Cannot export block '{blockname}' due to errors.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return "";
        }

        var block = FindBlockRecursively(plcSoftware.BlockGroup, blockname);

        if (block != null)
        {
            try
            {
                string exportFilePath = Path.Combine(exportPath, block.Name + ".xml");

                if (File.Exists(exportFilePath))
                {
                    File.Delete(exportFilePath);
                }
                
                block.Export(new FileInfo(exportFilePath), exportOptions: default);
                return exportPath;
            }
            catch (EngineeringTargetInvocationException ex)
            {
                MessageBox.Show($"Failed to export block '{block.Name}': {ex.Message}", "Export Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            MessageBox.Show($"Block '{blockname}' not found.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        return "";
    }

    static void updateTTST(List<string> blockNames, string dbName)
    {
        //string folderPath = @"C:\Generate Block\V20 All Files\Testing\TTST";
        string folderPath = @"C:\Generate Block\V20 All Files\Testing\TTST\Updated TTST";

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        foreach (string blockName in blockNames)
        {
            string matchedFilePath = Path.Combine(folderPath, blockName + ".TTST");

            if (!File.Exists(matchedFilePath))
                continue;

            string fileContent = File.ReadAllText(matchedFilePath);
            var matches = Regex.Matches(fileContent, @"VAR\s*(.*?)\s*END_VAR", RegexOptions.Singleline);

            foreach (Match block in matches)
            {
                string fullBlock = block.Value;
                string varSection = block.Groups[1].Value;

                var lines = varSection.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                List<string> modifiedLines = new List<string>();

                foreach (var line in lines)
                {
                    if (!line.Trim().StartsWith("iq") && line.Trim() != "")
                    {
                        string trimmedLine = line.Trim().TrimEnd(';');

                        if (!trimmedLine.Contains(":"))
                        {
                            modifiedLines.Add(trimmedLine + ";");
                            continue;
                        }

                        var parts = trimmedLine.Split(':');
                        if (parts.Length != 2)
                        {
                            modifiedLines.Add(trimmedLine + ";");
                            continue;
                        }

                        string lhs = parts[0].Trim();
                        string rhs = parts[1].Trim();
                        string[] rhsParts = rhs.Split('.');

                        if (rhsParts.Length > 1 && rhsParts[0] != dbName)
                        {
                            rhsParts[0] = dbName.Trim();
                            rhs = string.Join(".", rhsParts);
                        }

                        modifiedLines.Add($"{lhs}: {rhs};");
                    }
                    else
                    {
                        modifiedLines.Add(line.Trim());
                    }
                }

                string modifiedBlock = "VAR\n" + string.Join("\n", modifiedLines) + "\nEND_VAR";
                fileContent = fileContent.Replace(fullBlock, modifiedBlock);
            }           
            File.WriteAllText(matchedFilePath, fileContent);
        }
    }

    //static void updateTTST(string blockName, string dbName)
    //{
    //    List<string> uniqueList = new List<string>();
    //    //string[] fileName = Directory.GetFiles(@"C:\Users\Wpuser\Downloads\Final\Generate Block\Testing\TTST","*.TTST");
    //    //string[] fileName = Directory.GetFiles(@"C:\Generate Block\V20 All Files\Testing\TTST\Updated TTST", "*.TTST");
    //    string[] fileName = Directory.GetFiles(@"C:\Generate Block\V20 All Files\Testing\TTST", "*.TTST");
    //    string matchedFile = fileName.FirstOrDefault(file =>
    //Path.GetFileNameWithoutExtension(file).Equals(blockName.Trim(), StringComparison.OrdinalIgnoreCase));
    //    string fileContent = System.IO.File.ReadAllText(matchedFile);
    //    if (matchedFile != null)
    //    {
    //        var matches = Regex.Matches(fileContent, @"VAR\s*(.*?)\s*END_VAR", RegexOptions.Singleline);
    //        var matchesStep = Regex.Matches(fileContent, @"STEP\s*(.*?)\s*END_STEP", RegexOptions.Singleline);
    //        foreach (Match block in matches)
    //        {
    //            string fullBlock = block.Value;
    //            string varSection = block.Groups[1].Value;
    //            var lines = varSection.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
    //            List<string> modifiedLines = new List<string>();
    //            foreach (var line in lines)
    //            {
    //                if (!line.Trim().StartsWith("iq") && line.Trim() != "")
    //                {
    //                    string trimmedLine = line.Trim().TrimEnd(';');
    //                    if (!trimmedLine.Contains(":"))
    //                    {
    //                        modifiedLines.Add(trimmedLine + ";");
    //                        continue;
    //                    }
    //                    var parts = trimmedLine.Split(':');
    //                    if (parts.Length != 2)
    //                    {
    //                        modifiedLines.Add(trimmedLine + ";");
    //                        continue;
    //                    }
    //                    string lhs = parts[0].Trim();
    //                    string rhs = parts[1].Trim();
    //                    string[] rhsParts = rhs.Split('.');
    //                    if (rhsParts.Length > 1 && rhsParts[0] != dbName)
    //                    {
    //                        rhsParts[0] = dbName.Trim();
    //                        rhs = string.Join(".", rhsParts);
    //                    }
    //                    modifiedLines.Add($"{lhs}: {rhs};");
    //                }
    //                else
    //                {
    //                    modifiedLines.Add(line.Trim());
    //                }
    //            }
    //            string modifiedBlock = "VAR\n" + string.Join("\n", modifiedLines) + "\nEND_VAR";
    //            fileContent = fileContent.Replace(fullBlock, modifiedBlock);
    //        }
    //        System.IO.File.WriteAllText(matchedFile, fileContent);
    //    }
    //    else
    //    {
    //    }
    //    System.IO.File.WriteAllText(matchedFile, fileContent);
    //}

    public static void updateVersion()
    {
        //var xmlFile = Directory.GetFiles("C:\\Users\\Wpuser\\Downloads\\Final\\Generate Block\\Generated XML\\DB Xml", "*.xml");        
        var xmlFile = Directory.GetFiles("C:\\Generate Block\\V20 All Files\\DB XML", "*.xml");

        foreach (var xml in xmlFile)
        {
            try
            {
                string blockName = Path.GetFileNameWithoutExtension(xml);

                string xmlContent = File.ReadAllText(xml);
                xmlContent = xmlContent.Replace("V17", "V20");

                if (!xmlContent.Contains("<Namespace />"))
                    xmlContent = Regex.Replace(xmlContent, @"(<Name[^>]*>.*?</Name>)", "$1\n<Namespace />", RegexOptions.Singleline);

                File.WriteAllText(xml, xmlContent);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error executing login command: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    //var tiaPortal = new TiaPortal(TiaPortalMode.WithUserInterface);
    //var project = tiaPortal.Projects.OpenWithUpgrade(new FileInfo(@"C:\Generate Block\V20 Project\Project2.ap17"));

    //var device = project.Devices.FirstOrDefault(d => d.Name.Contains("PLC") || d.Name.Contains("S7-1500"));
    //var softwareContainer = device.DeviceItems[1].GetService<SoftwareContainer>();
    //var plcSoftware = softwareContainer?.Software as PlcSoftware;

    //var allBlocks = plcSoftware?.BlockGroup?.Blocks?.Select(b => b.Name).ToList() ?? new List<string>();

    //Application.EnableVisualStyles();
    //Application.SetCompatibleTextRenderingDefault(false);
    //BlockSelectionForm form = new BlockSelectionForm(allBlocks);

    //List<string> selectedBlocks = new List<string>();
    //if (form.ShowDialog() == DialogResult.OK)
    //{
    //    selectedBlocks = form.SelectedBlocks;
    //}
    //else
    //{
    //    MessageBox.Show("No blocks selected. Operation cancelled.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    //    return;
    //}

    //string exportPath = "";
    //foreach (var block in selectedBlocks)
    //{
    //    exportPath = exportblock(project, block, device);
    //}

    public static async void createProject(string projPath, List<string> initialBlockList)
    {
        string projectFolder = @"C:\Generate Block\V20 Project";

        string projectFile = Directory.GetFiles(projectFolder, "*.ap17")
                                      .OrderByDescending(f => File.GetLastWriteTime(f))
                                      .FirstOrDefault();

        if (string.IsNullOrEmpty(projectFile))
        {
            MessageBox.Show("No .ap17 project file found in the folder.", "Project Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var tiaPortal = new TiaPortal(TiaPortalMode.WithoutUserInterface);
        var project = tiaPortal.Projects.OpenWithUpgrade(new FileInfo(projectFile));

        var device = project.Devices.FirstOrDefault(d => d.Name.Contains("PLC") || d.Name.Contains("S7-1500"));
        var softwareContainer = device.DeviceItems[1].GetService<SoftwareContainer>();
        var plcSoftware = softwareContainer?.Software as PlcSoftware;

        var allBlocks = plcSoftware?.BlockGroup?.Blocks?.Select(b => b.Name).ToList() ?? new List<string>();

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        BlockSelectionForm form = new BlockSelectionForm(allBlocks);

        List<BlockTTSTMapping> selectedBlockMappings = new List<BlockTTSTMapping>();
        if (form.ShowDialog() == DialogResult.OK)
        {
            selectedBlockMappings = form.SelectedBlockMappings;
        }
        else
        {
            MessageBox.Show("No blocks selected. Operation cancelled.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!project.IsSimulationDuringBlockCompilationEnabled)
        {
            project.IsSimulationDuringBlockCompilationEnabled = true;
        }

        project.Save();

        string exportPath = @"C:\Generate Block\V20 All Files\Export Files";
        if (Directory.Exists(exportPath))
        {
            foreach (var file in Directory.GetFiles(exportPath))
            {
                File.Delete(file);
            }
        }

        foreach (var mapping in selectedBlockMappings)
        {
            exportPath = exportblock(project, mapping.BlockName, device);
        }

        string ttstFolder1 = @"C:\Generate Block\V20 All Files\Testing\TTST";
        string ttstFolder2 = @"C:\Generate Block\V20 All Files\Testing\TTST\Updated TTST";
        Directory.CreateDirectory(ttstFolder1);
        Directory.CreateDirectory(ttstFolder2);

        foreach (var mapping in selectedBlockMappings)
        {
            string currentBlockName = mapping.BlockName;
            string originalTTST = mapping.TTSTFilePath;

            var lines = File.ReadAllLines(originalTTST).ToList();
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartsWith("TEST_SET "))
                {
                    lines[i] = $"TEST_SET \"{currentBlockName}\"";
                    break;
                }
            }

            string newFileName = currentBlockName + ".ttst";
            string newPath1 = Path.Combine(ttstFolder1, newFileName);
            string newPath2 = Path.Combine(ttstFolder2, newFileName);

            File.WriteAllLines(newPath1, lines);
            File.WriteAllLines(newPath2, lines);
        }

        blockCalling(exportPath);

        string mainXmlPath = @"C:\Generate Block\V20 All Files\Generated XML\Main.xml";
        File.WriteAllText(mainXmlPath, string.Empty);

        var compiler = plcSoftware.GetService<ICompilable>();
        string dbXmlDirectory = @"C:\Generate Block\V20 All Files\DB XML";
        if (!Directory.Exists(dbXmlDirectory))
        {
            Directory.CreateDirectory(dbXmlDirectory);
        }

        var xmlFiles = Directory.GetFiles(dbXmlDirectory, "*.xml");
        foreach (var xml in xmlFiles)
        {
            try
            {
                plcSoftware.BlockGroup.Blocks.Import(new FileInfo(xml), ImportOptions.Override);
            }
            catch (Exception ex)
            {                
            }
        }

        compiler.Compile();
        project.Save();

        ImportElementAsync(project);
    }


    //public static async void createProject(string projPath, List<string> blockName)
    //{
    //    string projectFolder = @"C:\Generate Block\V20 Project";

    //    string projectFile = Directory.GetFiles(projectFolder, "*.ap17")
    //                                  .OrderByDescending(f => File.GetLastWriteTime(f))
    //                                  .FirstOrDefault();

    //    if (string.IsNullOrEmpty(projectFile))
    //    {
    //        MessageBox.Show("No .ap17 project file found in the folder.", "Project Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //        return;
    //    }

    //    var tiaPortal = new TiaPortal(TiaPortalMode.WithoutUserInterface);
    //    var project = tiaPortal.Projects.OpenWithUpgrade(new FileInfo(projectFile));

    //    var device = project.Devices.FirstOrDefault(d => d.Name.Contains("PLC") || d.Name.Contains("S7-1500"));
    //    var softwareContainer = device.DeviceItems[1].GetService<SoftwareContainer>();
    //    var plcSoftware = softwareContainer?.Software as PlcSoftware;

    //    var allBlocks = plcSoftware?.BlockGroup?.Blocks?.Select(b => b.Name).ToList() ?? new List<string>();

    //    Application.EnableVisualStyles();
    //    Application.SetCompatibleTextRenderingDefault(false);
    //    BlockSelectionForm form = new BlockSelectionForm(allBlocks);

    //    List<string> selectedBlocks = new List<string>();
    //    if (form.ShowDialog() == DialogResult.OK)
    //    {
    //        selectedBlocks = form.SelectedBlocks;
    //    }
    //    else
    //    {
    //        MessageBox.Show("No blocks selected. Operation cancelled.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    //        return;
    //    }

    //    var attributeValue = project.IsSimulationDuringBlockCompilationEnabled;

    //    if (attributeValue == false)
    //        project.IsSimulationDuringBlockCompilationEnabled = true;

    //    project.Save();
    //    string exportPath = @"C:\Generate Block\V20 All Files\Export Files";

    //    foreach(var file in Directory.GetFiles(exportPath))
    //    {
    //        File.Delete(file);
    //    }

    //    foreach (var block in selectedBlocks)
    //    {
    //        exportPath = exportblock(project, block, device);
    //    }

    //    blockCalling(exportPath);
    //    File.WriteAllText("C:\\Generate Block\\V20 All Files\\Generated XML\\Main.xml", string.Empty);

    //    var compiler = plcSoftware.GetService<ICompilable>();
    //    //var xmlFile = Directory.GetFiles("C:\\Users\\Wpuser\\Downloads\\Final\\Generate Block\\Generated XML\\DB Xml", "*.xml");
    //    //var xmlFile = Directory.GetFiles("C:\\Generate Block\\V20 All Files\\DB XML", "*.xml");
    //    string dbXmlDirectory = @"C:\Generate Block\V20 All Files\DB XML";

    //    if (!Directory.Exists(dbXmlDirectory))
    //    {
    //        Directory.CreateDirectory(dbXmlDirectory);
    //    }

    //    var xmlFile = Directory.GetFiles(dbXmlDirectory, "*.xml");

    //    foreach (var xml in xmlFile)
    //    {
    //        try
    //        {
    //            var importedBlocks = plcSoftware.BlockGroup.Blocks.Import(new FileInfo(xml), ImportOptions.Override);
    //        }
    //        catch (Exception ex)
    //        {
    //        }
    //    }

    //    compiler.Compile();
    //    project.Save();
    //    ImportElementAsync(project);
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
    static List<string> ImportAwlFiles(string projectPath, string awlFilesDirectory, string caxFilePath)
    {
        List<string> errorAwlFiles = new List<string>();
        string exportDirectory = mainOutputFolder;
        caxFilePath = @"C:\Users\Wpuser\Downloads\aml file\Project1.aml";
        TiaPortal tiaPortal = null;
        Project project = null;

        try
        {
            var tiaProcesses = TiaPortal.GetProcesses();

            if (tiaProcesses.Count > 0)
            {
                tiaPortal = tiaProcesses[0].Attach();
                Console.WriteLine("Attached to existing TIA Portal instance.");
            }
            else
            {
                tiaPortal = new TiaPortal(TiaPortalMode.WithoutUserInterface);
                Console.WriteLine("Started new TIA Portal instance.");
            }

            project = tiaPortal.Projects
                .FirstOrDefault(p => p.Path.FullName.Equals(projectPath, StringComparison.OrdinalIgnoreCase));

            if (project == null)
            {
                project = tiaPortal.Projects.Open(new FileInfo(projectPath));
                Console.WriteLine("Project opened successfully.");
            }
            else
            {
                Console.WriteLine("Project already open in attached TIA instance.");
            }
            var device = project.Devices.FirstOrDefault(d => d.Name.Contains("PLC_1") || d.Name.Contains("S7-300"));
            if (device == null)
            {
                Console.WriteLine("No device found. Importing from CAX file...");

                if (!File.Exists(caxFilePath))
                {
                    Console.WriteLine("CAX file not found: " + caxFilePath);
                    return errorAwlFiles;
                }

                //var deviceGroup = project.DeviceGroups.FirstOrDefault() ?? project.DeviceGroups.Create("ImportedDevices");
                //deviceGroup.ImportDevices(new FileInfo(caxFilePath));
                CaxProvider caxProvider = project.GetService<CaxProvider>();
                if (caxProvider != null)
                {
                    string logFolder = @"C:\New folder";
                    string logFile = Path.Combine(logFolder, "import_log.log");

                    if (!Directory.Exists(logFolder))
                    {
                        Directory.CreateDirectory(logFolder);
                    }
                    caxProvider.Import(new FileInfo(caxFilePath), new FileInfo(logFile), CaxImportOptions.MoveToParkingLot);
                    Console.WriteLine("Device imported from CAX.");
                }
                device = project.Devices.FirstOrDefault(d => d.Name.Contains("PLC_1") || d.Name.Contains("S7-300"));

                if (device == null)
                {
                    Console.WriteLine("Still no device found after CAX import.");
                    return errorAwlFiles;
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
                            Console.WriteLine($"Deleted existing AWL source: {fileName}");
                        }
                        try
                        {
                            var externalSource = plcSoftware.ExternalSourceGroup.ExternalSources.CreateFromFile(fileName, awlFile);
                            externalSource.GenerateBlocksFromSource((GenerateBlockOption)ImportOptions.Override);
                            Console.WriteLine($"Imported and generated block from: {awlFile}");

                            var plcBlockGroup = plcSoftware.BlockGroup;
                            var blockName = GetBlockNameFromAwlFile(awlFile);
                            var block = FindBlockRecursively(plcBlockGroup, blockName.Trim());
                            if (block != null)
                            {
                                ls = new List<string>();
                                var singleCompiler = block.GetService<ICompilable>();
                                var result1 = singleCompiler.Compile();
                                Console.WriteLine($"Compiled block {fileName}: {result1?.State}");
                                List<string> errorMessages = GetCompilerMessages(string.Empty, string.Empty, result1.Messages, true);
                                List<string> allMessages = GetCompilerMessages(string.Empty, string.Empty, result1.Messages, false);
                                if (errorMessages.Count == 0)
                                {
                                    Console.WriteLine("0 Compilation error");
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
                            else
                            {
                                Console.WriteLine($"Warning: Generated block '{fileName}' not found for compilation.");
                            }
                        }
                        catch (EngineeringException ex)
                        {
                            Console.WriteLine($"Error in {awlFile}: {ex.Message}");
                        }
                    }
                }
            }

            var tiaProcess = Process.GetProcessesByName("Siemens.Automation.Portal").FirstOrDefault();

            if (tiaProcess != null)
            {
                Thread.Sleep(9000);

                using (var app = FlaUI.Core.Application.Attach(tiaProcess))
                using (var automation = new UIA3Automation())
                {
                    var mainWindow = app.GetMainWindow(automation);

                    var projectTree = mainWindow.FindFirstDescendant(cf => cf.ByName("Grid-Control").And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Table)));
                    if (projectTree == null)
                    {
                        Console.WriteLine("ProjectNavigationTree not found.");

                    }
                    if (mainWindow != null)
                    {
                        var projectViewBtn = mainWindow.FindFirstDescendant(cf => cf.ByName("ContextNavigator.GoToProjectView.Title").And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.ListItem)));
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

                            var invokePattern = plcEdit.Patterns.Invoke.PatternOrDefault;
                            if (invokePattern != null)
                            {
                                invokePattern.Invoke();
                                Console.WriteLine("Invoked Program Block using InvokePattern.");
                            }
                            else
                            {
                                Console.WriteLine("InvokePattern not available. Trying keyboard fallback...");
                                SendKeys.SendWait("{ENTER}");
                                Thread.Sleep(100);
                                SendKeys.SendWait("{ENTER}");
                                Console.WriteLine("Simulated double-click using Enter key.");
                            }
                            var progBlock = mainWindow.FindFirstDescendant(cf => cf.ByName("Siemens.Simatic.Lang.BlockLogic:Siemens.Simatic.PlcLanguages.BlockLogic.MenuService:SystemFolderProgramBlocksResX:1E1C1007-B70C-4670-81E9-515ABD35EA0C.Attribute.Name").And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit)));
                            if (progBlock != null)
                            {
                                progBlock.Focus();
                                Thread.Sleep(5000);
                                WinForms.SendKeys.SendWait("+{F10}");
                                Thread.Sleep(1000);
                            }
                            progBlock.Focus();
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
                                Console.WriteLine("Focused 'Switch programming language' menu.");

                                Thread.Sleep(500);

                                var ladMenu = desktop.FindFirstDescendant(cf =>
                                    cf.ByName("Siemens.Simatic.Lang.Portal:Siemens.Simatic.Lang.Portal.Resources.BlockLogic.MenuService:SwitchBlockLanguage_LAD")
                                      .And(cf.ByControlType(FlaUI.Core.Definitions.ControlType.MenuItem)));

                                if (ladMenu != null)
                                {
                                    ladMenu.AsMenuItem()?.Invoke();
                                    Console.WriteLine("Clicked on LAD submenu.");
                                }
                                else
                                {
                                    Console.WriteLine("LAD submenu not found.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Switch programming language menu not found.");
                            }
                            project.Save();
                            Console.WriteLine("Project saved.");
                            var softwareContainer1 = device.DeviceItems[1].GetService<SoftwareContainer>();
                            var plcSoftware1 = softwareContainer1?.Software as PlcSoftware;
                            
                            ExportAllBlocks(project, plcSoftware1, exportDirectory);
                        }
                    }
                }
            }
        }
        catch (EngineeringException ex)
        {
            Console.WriteLine("TIA Portal engineering error: " + ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("General error: " + ex.Message);
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
                catch (Exception ex)
                {
                    compileResults.Add($"Failed to export/delete {finalName}: {ex.Message}");
                }
            }
        }

        string exportLogPath = Path.Combine(exportDirectory, "export_log.txt");
        File.WriteAllLines(exportLogPath, compileResults);

        ShowCompilationResults(compileResults);

        List<string> importErrors = ImportXmlFiles(selectedProjectPath, mainOutputFolder, selectedCaxPath1);

        foreach (var err in importErrors)
        {
            Console.WriteLine($"Import error: {err}");
        }

        var device = project.Devices.FirstOrDefault(d => d.Name.Contains("PLC_1") || d.Name.Contains("S7-300"));
        if (device != null)
        {
            try
            {
                string deviceName = device.Name;
                device.Delete();
                Console.WriteLine($"Deleted CPU/Device: {deviceName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete CPU: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("No device found to delete.");
        }
    }

    static void ShowCompilationResults(List<string> results)
    {
    }
    public static void generateAWL()
    {
        var loginForm = new LoginForm();
        if (loginForm.ShowDialog() != DialogResult.OK)
        {
            Console.WriteLine("Login canceled.");
            return;
        }

        string userId = loginForm.Username;
        string password = loginForm.Password;

        var options = new EdgeOptions();
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
        string downloadDirectory = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    "Downloads",
    "AWL Test Download");

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

        try
        {
            using (IWebDriver driver = new EdgeDriver(options))
            {

                driver.Navigate().GoToUrl("https://genplc.wipropari.com/");
                Thread.Sleep(3000);

                try
                {
                    driver.FindElement(By.XPath("//input[@placeholder='Enter username']")).SendKeys(userId);
                    driver.FindElement(By.XPath("//input[@placeholder='Enter password']")).SendKeys(password);
                    driver.FindElement(By.XPath("//button[contains(text(), 'Login') or @id='btnLogin']")).Click();
                    Thread.Sleep(3000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Login error: " + ex.Message);
                    return;
                }

                string folderPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    "Downloads",
    "TestUploads");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    MessageBox.Show($"Folder not found.\nA new folder has been created at:\n{folderPath}", "Folder Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                if (Directory.GetFiles(folderPath, "*.xlsx").Length == 0)
                {
                    MessageBox.Show("No Excel files found in the folder:\n" + folderPath, "Missing Files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Console.WriteLine("No Excel files found in: " + folderPath);
                    return;
                }

                string[] excelFiles = Directory.GetFiles(folderPath, "*.xlsx");
                DateTime firstUploadTime = DateTime.MaxValue;

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));

                foreach (string file in excelFiles)
                {
                    Console.WriteLine($"\nUploading file: {Path.GetFileName(file)}");

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
                        Console.WriteLine("Timeout: 'Generate Block' button not found or not visible.");
                        return;
                    }
                    catch (NoSuchElementException ex)
                    {
                        Console.WriteLine("Element not found: " + ex.Message);
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unexpected error: " + ex.Message);
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
                            Console.WriteLine("Clicked on dropdown icon.");
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
                                Console.WriteLine("No project options found.");
                                return;
                            }

                            string selected = ProjectSelectionForm.ShowProjectSelection(projectOptions);
                            if (string.IsNullOrEmpty(selected))
                            {
                                Console.WriteLine("No project selected by user.");
                                return;
                            }

                            selectedProject = selected;

                            inputBox.SendKeys(selectedProject);
                            Thread.Sleep(1000);

                            var projectToSelect = driver.FindElement(By.XPath($"//div[contains(text(), '{selectedProject}') and contains(@class,'option')]"));
                            projectToSelect.Click();
                            Console.WriteLine($"Project '{selectedProject}' selected.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Dropdown interaction error: " + ex.Message);
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

                        Console.WriteLine("Uploaded at: " + uploadStartTime);
                        Thread.Sleep(4000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("File upload error: " + ex.Message);
                        return;
                    }
                }
                Thread monitorThread = new Thread(() => MonitorUploadStatus(driver, firstUploadTime, excelFiles.Length, downloadDirectory));
                monitorThread.Start();
                monitorThread.Join();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fatal error: " + ex.Message);
        }
    }
    static void MonitorUploadStatus(IWebDriver driver, DateTime uploadStartTime, int excelCount, string downloadDirectory)
    {
        try
        {
            Console.WriteLine("Monitoring upload status...");

            TimeSpan overallTimeout = TimeSpan.FromMinutes(30);
            DateTime monitoringStartTime = DateTime.Now;
            bool fileDownloaded = false;
            HashSet<string> downloadedFiles = new HashSet<string>();

            if (!Directory.Exists(awlFilesDirectory))
                Directory.CreateDirectory(awlFilesDirectory);

            while (DateTime.Now - monitoringStartTime < overallTimeout)
            {
                int rowCount = driver.FindElements(By.CssSelector("table.user-table tbody tr.user-row")).Count;

                for (int i = 0; i < rowCount; i++)
                {
                    driver.Navigate().Refresh();
                    Thread.Sleep(2000);

                    var rows = driver.FindElements(By.CssSelector("table.user-table tbody tr.user-row"));
                    if (i >= rows.Count)
                    {
                        Console.WriteLine("Row count changed after refresh. Skipping index: " + i);
                        continue;
                    }

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
                            Console.WriteLine($"File uploaded at: {fileTime}");
                            Console.WriteLine("Detected file uploaded after upload button click.");

                            try
                            {
                                IWebElement downloadBtn = row.FindElement(By.CssSelector("button.download-btn"));
                                Console.WriteLine("Clicking download button...");

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
                                    Console.WriteLine($"Waiting for downloads... ({waitTime}s)");
                                }

                                int newDownloads = Directory.GetFiles(downloadDirectory).Length - previousFileCount;
                                if (newDownloads < expectedCount)
                                {
                                    Console.WriteLine($"Only {newDownloads}/{expectedCount} files downloaded.");
                                }
                                else
                                {
                                    Console.WriteLine("All files downloaded successfully.");
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

                                    File.AppendAllText(logFilePath, log);
                                }
                                else
                                {
                                    Console.WriteLine("Could not find upload time for: " + uniqueId);
                                    Console.WriteLine("Keys Available: " + string.Join(", ", uploadTimes.Keys.Select(Path.GetFileNameWithoutExtension)));
                                }
                            }
                            catch (NoSuchElementException)
                            {
                                Console.WriteLine("Error: Download button not found.");
                            }
                        }
                        else if (success.Contains("/icon-error"))
                        {
                            Console.WriteLine($"Block generation failed for file: {uniqueId} at {fileTime:dd-MM-yyyy hh:mm:ss tt}");
                        }
                        else
                        {
                            Console.WriteLine($"Status not ready or unknown: {statusText} for file at {fileTime}");
                        }
                    }

                    if (downloadedFiles.Count >= excelCount)
                    {
                        Console.WriteLine("All uploaded files downloaded successfully.");
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

                            string reportPath = Path.Combine(downloadDirectory, "Validation_Report.txt");
                            File.WriteAllText(reportPath, reportBuilder.ToString());

                            string popupText = popupBuilder.ToString();
                            if (!string.IsNullOrWhiteSpace(popupText))
                            {
                                string popupDisplay = popupText.Length > 1500
                                    ? popupText.Substring(0, 1500) + "\n...\n(See full report in file)"
                                    : popupText;

                                MessageBox.Show(popupDisplay, "Validation Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error generating validation report: " + ex.Message);
                        }
                        return;
                    }
                }

                if (!fileDownloaded)
                    Thread.Sleep(2000);
            }

            if (!fileDownloaded)
                Console.WriteLine("Timeout: No uploaded file matching the criteria appeared.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Monitoring thread error: " + ex.Message);
        }
    }

    static List<string> xmlPath = new List<string>();
    static List<string> ImportXmlFiles(string projectPath, string xmlFilesDirectory, string caxFilePath1)
    {
        List<string> errorXmlFiles = new List<string>();
        TiaPortal tiaPortal = null;
        Project project = null;
        caxFilePath1 = @"C:\\Users\\P01090879\\Downloads\\Project2.aml";

        try
        {
            var tiaProcesses = TiaPortal.GetProcesses();

            if (tiaProcesses.Count > 0)
            {
                tiaPortal = tiaProcesses[0].Attach();
                Console.WriteLine("Attached to existing TIA Portal instance.");
            }
            else
            {
                tiaPortal = new TiaPortal(TiaPortalMode.WithUserInterface);
                Console.WriteLine("Started new TIA Portal instance.");
            }

            project = tiaPortal.Projects
                .FirstOrDefault(p => p.Path.FullName.Equals(projectPath, StringComparison.OrdinalIgnoreCase));

            if (project == null)
            {
                project = tiaPortal.Projects.Open(new FileInfo(projectPath));
                Console.WriteLine("Project opened successfully.");
            }
            else
            {
                Console.WriteLine("Project already open in attached TIA instance.");
            }

            var device = project.Devices.FirstOrDefault(d => d.Name.Contains("PLC_2") || d.Name.Contains("S7-1500"));
            if (device == null)
            {
                Console.WriteLine("No device found. Importing from CAX file...");

                if (!File.Exists(caxFilePath1))
                {
                    Console.WriteLine("CAX file not found: " + caxFilePath1);
                }

                CaxProvider caxProvider = project.GetService<CaxProvider>();
                if (caxProvider != null)
                {
                    string logFolder = @"C:\\New folder";
                    string logFile = Path.Combine(logFolder, "import_log1.log");

                    if (!Directory.Exists(logFolder))
                    {
                        Directory.CreateDirectory(logFolder);
                    }

                    caxProvider.Import(new FileInfo(caxFilePath1), new FileInfo(logFile), CaxImportOptions.MoveToParkingLot);
                    Console.WriteLine("Device imported from CAX.");
                }

                device = project.Devices.FirstOrDefault(d => d.Name.Contains("PLC_2") || d.Name.Contains("S7-1500"));

                if (device == null)
                {
                    Console.WriteLine("Still no device found after CAX import.");
                    return errorXmlFiles;
                }
            }

            var softwareContainer = device.DeviceItems[1].GetService<SoftwareContainer>();
            var plcSoftware = softwareContainer?.Software as PlcSoftware;

            if (plcSoftware != null)
            {
                string importedLogFile = Path.Combine(xmlFilesDirectory, "imported_files.log");
                HashSet<string> alreadyImported = new HashSet<string>();

                if (File.Exists(importedLogFile))
                {
                    alreadyImported = new HashSet<string>(File.ReadAllLines(importedLogFile));
                }

                var xmlFiles = Directory.GetFiles(xmlFilesDirectory, "*.xml")
                                        .Where(f => !alreadyImported.Contains(Path.GetFileName(f)))
                                        .OrderBy(f => File.GetLastWriteTime(f))
                                        .ToList();

                if (xmlFiles.Count == 0)
                {
                    Console.WriteLine("No new XML files to import.");
                    return errorXmlFiles;
                }

                Console.WriteLine($"Found {xmlFiles.Count} new XML file(s) to import.");

                foreach (var xmlFile in xmlFiles)
                {
                    try
                    {
                        var importedBlocks = plcSoftware.BlockGroup.Blocks.Import(new FileInfo(xmlFile), ImportOptions.Override);
                        xmlPath.Add(xmlFile);
                        foreach (PlcBlock importedBlock in importedBlocks)
                        {
                            Console.WriteLine($"Imported XML block: {importedBlock.Name}");

                            var singleCompiler = importedBlock.GetService<ICompilable>();
                            var result1 = singleCompiler.Compile();

                            List<string> errorMessages = GetCompilerMessages("", "", result1.Messages, true);
                            if (errorMessages.Count == 0)
                            {
                                Console.WriteLine("Compilation successful.");
                            }
                            else
                            {
                                string errorText = $"Compilation failed for {importedBlock.Name}:\n" +
                                                   string.Join("\n", errorMessages.Take(10));
                                errorXmlFiles.Add(importedBlock.Name);
                                MessageBox.Show(errorText, "Compile Result", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }

                        File.AppendAllLines(importedLogFile, new[] { Path.GetFileName(xmlFile) });
                    }
                    catch (EngineeringException ex)
                    {
                        Console.WriteLine($"Error importing {Path.GetFileName(xmlFile)}: {ex.Message}");
                        errorXmlFiles.Add(Path.GetFileName(xmlFile));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("General Error: " + ex.Message);
        }

        return errorXmlFiles;
    }

    public static List<List<string>> selectionOption(List<List<string>> options, List<string> Inout, List<List<object>> TTST, Dictionary<string, string> previousSelections, List<List<List<string>>> disc/*,string tatPath*/, List<List<string>> tagAndRangeList)
    {
        List<List<string>> L1 = new List<List<string>>();
        options = options
    .Select(sublist => sublist
        .Where(item => !item.ToLower().Contains("[false]"))
        .ToList())
    .ToList();
        //List<string> reportData = options;
        List<string> selectedValues = new List<string>();
        //List<string> newlist = tagAndRangeList;
        int screenWidth = Screen.PrimaryScreen.Bounds.Width;
        int screenHeight = Screen.PrimaryScreen.Bounds.Height;
        int countfortT = 0;

        Form form = new Form
        {
            Text = "Select Options",
            AutoScroll = true,//
            WindowState = FormWindowState.Maximized,
            FormBorderStyle = FormBorderStyle.None,
            StartPosition = FormStartPosition.CenterScreen
        };

        Panel titleBar = new Panel
        {
            Height = 20,
            Dock = DockStyle.Top,
            BackColor = System.Drawing.Color.Transparent
        };
        form.Controls.Add(titleBar);

        Button closeButton = new Button
        {
            Text = "X",
            ForeColor = System.Drawing.Color.White,
            BackColor = System.Drawing.Color.Red,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(20, 20),
            Dock = DockStyle.Right
        };
        string result = "None";
        closeButton.FlatAppearance.BorderSize = 0;
        closeButton.Click += (s, e) =>
        {
            result = "ClosedByUser";
            form.DialogResult = DialogResult.OK;
            form.Close();
            options.Add(new List<string> { result });
        };
        titleBar.Controls.Add(closeButton);

        Button maximizeButton = new Button
        {
            Text = "🗖",
            ForeColor = System.Drawing.Color.White,
            BackColor = System.Drawing.Color.Gray,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(20, 20),
            Dock = DockStyle.Right
        };
        maximizeButton.FlatAppearance.BorderSize = 0;
        maximizeButton.Click += (s, e) =>
        {
            if (form.WindowState == FormWindowState.Maximized)
                form.WindowState = FormWindowState.Normal;
            else
                form.WindowState = FormWindowState.Maximized;
        };
        titleBar.Controls.Add(maximizeButton);

        Button minimizeButton = new Button
        {
            Text = "―",
            ForeColor = System.Drawing.Color.White,
            BackColor = System.Drawing.Color.Gray,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(20, 20),
            Dock = DockStyle.Right
        };
        minimizeButton.FlatAppearance.BorderSize = 0;
        minimizeButton.Click += (s, e) => form.WindowState = FormWindowState.Minimized;
        titleBar.Controls.Add(minimizeButton);

        bool dragging = false;
        Point dragCursor = Point.Empty;
        Point dragForm = Point.Empty;
        titleBar.MouseDown += (s, e) =>
        {
            dragging = true;
            dragCursor = System.Windows.Forms.Cursor.Position;
            dragForm = form.Location;
        };
        titleBar.MouseMove += (s, e) =>
        {
            if (dragging)
            {
                Point diff = Point.Subtract(System.Windows.Forms.Cursor.Position, new Size(dragCursor));
                form.Location = Point.Add(dragForm, new Size(diff));
            }
        };
        titleBar.MouseUp += (s, e) => dragging = false;

        FlowLayoutPanel mainPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Width = screenWidth,
            Height = screenHeight
        };
        form.Controls.Add(mainPanel);

        FlowLayoutPanel dropdownPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            Padding = new Padding(10),
            Width = screenWidth / 2
        };
        mainPanel.Controls.Add(dropdownPanel);

        FlowLayoutPanel inputsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoScroll = true,//
            WrapContents = false,//
            AutoSize = true,
            Padding = new Padding(10),
            Width = screenWidth / 2
        };
        mainPanel.Controls.Add(inputsPanel);

        Dictionary<System.Windows.Forms.Control, string> inputControls = new Dictionary<System.Windows.Forms.Control, string>();

        void AddInputs(string label, List<List<string>> items)
        {
            Label headerLabel = new Label
            {
                Text = label,
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold),
                Padding = new Padding(5)
            };
            inputsPanel.Controls.Add(headerLabel);


            int row = 1;

            foreach (var item1 in items)
            {
                TableLayoutPanel tableLayout = new TableLayoutPanel
                {
                    RowCount = 1,
                    ColumnCount = 3,
                    AutoSize = true,
                    Padding = new Padding(5),
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
                };

                tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
                tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
                tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

                tableLayout.Controls.Add(new Label { Text = "Tag Name", AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold) }, 0, 0);
                tableLayout.Controls.Add(new Label { Text = "Selection Option", AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold) }, 1, 0);
                tableLayout.Controls.Add(new Label { Text = "Range Info", AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold) }, 2, 0);

                foreach (var item2 in item1)
                {
                    if (string.IsNullOrWhiteSpace(item2) || item2.Contains("Block Name :") || item2.Contains("Block_Name :")) continue;
                    var tagMatch = Regex.Match(item2, @"^([^,]+)");
                    var bracketMatches = Regex.Matches(item2, @"\[(.*?)\]");

                    string tagName = tagMatch.Success ? tagMatch.Groups[1].Value.Trim() : "";
                    string selection = bracketMatches.Count >= 1 ? bracketMatches[0].Groups[1].Value.Trim() : "";
                    if (selection == "" && item2.Contains("Bool"))
                    {
                        selection = "FALSE";
                    }
                    string range = bracketMatches.Count >= 2 ? bracketMatches[1].Groups[1].Value.Trim() : "";
                    var booleanValues = new HashSet<string>(new[] { "TRUE", "FALSE", "ON", "OFF", "Checked", "Unchecked" }, StringComparer.OrdinalIgnoreCase);
                    bool isBoolean = booleanValues.Contains(selection);

                    var tagLabel = new Label
                    {
                        Text = tagName,
                        AutoSize = true,
                        Padding = new Padding(4)
                    };
                    tableLayout.Controls.Add(tagLabel, 0, row);

                    if (isBoolean)
                    {
                        bool isTrue = selection.Equals("TRUE", StringComparison.OrdinalIgnoreCase)
                                   || selection.Equals("ON", StringComparison.OrdinalIgnoreCase)
                                   || selection.Equals("Checked", StringComparison.OrdinalIgnoreCase);

                        Button toggleBtn = new Button
                        {
                            Text = "",
                            BackColor = isTrue ? System.Drawing.Color.LightGreen : System.Drawing.Color.IndianRed,
                            Width = 30,
                            Tag = isTrue
                        };

                        toggleBtn.Click += (sender, e) =>
                        {
                            bool current = (bool)toggleBtn.Tag;
                            current = !current;
                            toggleBtn.Tag = current;
                            toggleBtn.BackColor = current ? System.Drawing.Color.LightGreen : System.Drawing.Color.IndianRed;
                            previousSelections[tagName] = current ? "Checked" : "Unchecked";
                        };

                        inputControls[toggleBtn] = tagName + "," + item1[0].Split(',')[1];
                        tableLayout.Controls.Add(toggleBtn, 1, row);
                    }
                    else
                    {
                        var inputBox = new System.Windows.Forms.TextBox
                        {
                            Width = 100,
                            Text = selection
                        };
                        inputBox.TextChanged += (sender, e) =>
                        {
                            previousSelections[tagName] = inputBox.Text;
                        };
                        inputControls[inputBox] = tagName;
                        tableLayout.Controls.Add(inputBox, 1, row);
                    }
                    string cleanedRange = range.Replace("\"", "")
                                               .Replace("][", ",")
                                               .Replace("/", "")
                                               .Replace("[", "")
                                               .Replace("]", "")
                                               .Trim();

                    var rangeBox = new System.Windows.Forms.TextBox
                    {
                        Text = cleanedRange,
                        Width = 120,
                        ForeColor = System.Drawing.Color.DarkSlateGray
                    };
                    tableLayout.Controls.Add(rangeBox, 2, row);

                    row++;
                }

                Button addButton = new Button
                {
                    Text = "Add Value",
                    AutoSize = true,
                    Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold),
                    Padding = new Padding(5)
                };
                ToolTip toolTip = new ToolTip();
                toolTip.SetToolTip(addButton, item1[0]);
                addButton.Click += (sender, e) =>
                {
                    try
                    {
                        string tip = toolTip.GetToolTip(addButton);

                        L1 = new List<List<string>>();
                        List<string> subList = new List<string>();
                        string newValue = "";
                        string headerValue = "";
                        foreach (var t in inputControls)
                        {
                            if (t.Key.ToString().Contains("System.Windows.Forms.ComboBox") && t.Value == tip.Split(',')[1].Trim())
                            {
                                string val = t.Key.Text;

                                var match = System.Text.RegularExpressions.Regex.Match(val, @"(?:_Trans|Transition|_S)\s*(\d+(_T\d+)?)");
                                if (match.Success)
                                {
                                    string selectedValue = match.Groups[1].Value;

                                    subList.Add(val);
                                    newValue = selectedValue;
                                    headerValue = t.Value;
                                }
                            }
                            foreach (var itam1 in items)
                            {
                                if (item1[0].Split(',')[1].Trim().Contains(headerValue.Trim()) && headerValue != "")
                                {
                                    foreach (var it in item1)
                                    {
                                        if (it != null)
                                        {
                                            if (it.Split(',').Count() > 0 && (!it.Contains("Block Name") || !it.Contains("Block_Name")))
                                            {
                                                if (it.Split(',')[0] == t.Value.Split(',')[0].Trim() && (t.Key.BackColor != System.Drawing.Color.IndianRed && t.Key.BackColor.Name != "Window"))
                                                {
                                                    var itemParts = it.Split(',');
                                                    var item = it.Trim();
                                                    var parts = item.Split(',').Select(p => p.Trim()).ToList();

                                                    var bracketMatches = Regex.Matches(item, @"\[(.*?)\]");

                                                    if (bracketMatches.Count >= 2)
                                                    {
                                                        string tag = parts[0];
                                                        string selection = bracketMatches[0].Groups[1].Value;
                                                        string range = bracketMatches[1].Groups[1].Value;
                                                        var rangeList = range.Split(',').Select(x => x.Trim()).ToList();
                                                        if (rangeList.Contains(newValue.ToString())) continue;
                                                        string updatedRange = AddValueToRange(range, newValue);

                                                        string updatedItem = $"{tag},[{selection}],{updatedRange},Bool";
                                                        item1[item1.IndexOf(it)] = updatedItem;

                                                        for (int i = 0; i < tagAndRangeList.Count; i++)
                                                        {
                                                            for (int j = 0; j < tagAndRangeList[i].Count; j++)
                                                            {
                                                                if (tagAndRangeList[i][0].Split(',')[1].Contains(headerValue))
                                                                {
                                                                    var parts1 = tagAndRangeList[i][j].Split(',');
                                                                    if (parts1[0].Trim() == tag)
                                                                    {
                                                                        tagAndRangeList[i][j] = tag + "," + updatedRange;
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        subList.Add(updatedItem);
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        string tagName = itemParts[0].Trim();
                                                        string type = itemParts.Length > 1 ? itemParts[1].Trim() : "Bool";
                                                        string selection = "TRUE";
                                                        string initialRange = newValue.ToString();

                                                        string updatedItem = $"{tagName},[{selection}],[{initialRange}],{type}";

                                                        int index = item1.IndexOf(it);
                                                        if (index != -1)
                                                            item1[index] = updatedItem;
                                                        subList.Add(updatedItem);
                                                        var t1 = tagAndRangeList;
                                                        bool tagExists = tagAndRangeList.Any(innerList => innerList.Any(itemw => itemw.Split(',')[0].Trim() == tagName));

                                                        if (!tagExists)
                                                        {
                                                            tagAndRangeList.Add(new List<string> { tagName + ",[" + initialRange + "]" });
                                                        }
                                                        break;
                                                    }
                                                }
                                                else if (it.Split(',')[0] == t.Value.Split(',')[0].Trim() && (t.Key.BackColor == System.Drawing.Color.IndianRed && t.Key.BackColor.Name != "Window"))
                                                {
                                                    if (tagAndRangeList.Any(sublist =>
                                                        sublist.Any(entry =>
                                                        entry.StartsWith(it.Split(',')[0] + ",") && entry.Contains(newValue))))
                                                    {
                                                        var item = it.Trim();
                                                        var parts = item.Split(',').Select(p => p.Trim()).ToList();
                                                        var bracketMatches = Regex.Matches(item, @"\[(.*?)\]");
                                                        string range = bracketMatches[1].Groups[1].Value;
                                                        string tag = parts[0];
                                                        string updatedItem = $"{tag},[{"False"}],[{newValue}],Bool";
                                                        subList.Add(updatedItem);
                                                        for (int i = 0; i < tagAndRangeList.Count; i++)
                                                        {
                                                            for (int j = 0; j < tagAndRangeList[i].Count; j++)
                                                            {
                                                                var entry = tagAndRangeList[i][j].Trim();
                                                                var parts2 = entry.Split(',').Select(p => p.Trim()).ToList();

                                                                string tagName = parts2[0];

                                                                var match = Regex.Match(entry, @"\[(.*?)\]");
                                                                if (!match.Success) continue;

                                                                string range1 = match.Groups[1].Value;
                                                                var rangeList = range1.Split(',').Select(x => x.Trim()).ToList();

                                                                if (entry.StartsWith(item.Split(',')[0] + ",") && entry.Contains(newValue))
                                                                {
                                                                    if (rangeList.Count == 1)
                                                                    {
                                                                        tagAndRangeList[i].RemoveAt(j);
                                                                        item1[item1.IndexOf(it)] = updatedItem;
                                                                    }
                                                                    else if (rangeList.Count > 1)
                                                                    {
                                                                        rangeList.Remove(newValue.ToString());
                                                                        string updatedRange = string.Join(",", rangeList);
                                                                        string updatedItem1 = $"{tagName},[True],[{updatedRange}],Bool";
                                                                        tagAndRangeList[i][j] = updatedItem1;
                                                                        item1[item1.IndexOf(it)] = updatedItem1;
                                                                        item1.Add(updatedItem);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (it.Split(',')[0] == t.Value.Trim() && !string.IsNullOrEmpty(t.Key.Text))
                                                {
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            L1.Add(subList);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                };
                inputsPanel.Controls.Add(addButton);
                inputsPanel.Controls.Add(tableLayout);
            }
        }
        string selectedItem1 = "";
        string tatName = "";
        void AddDropdownListsFromTTST(string lable, List<List<object>> TTsT/*, List<string> tagAndRangeList*/)
        {
            int fixedLabelWidth = 200;
            System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
            List<System.Windows.Forms.ComboBox> allComboBoxes = new List<System.Windows.Forms.ComboBox>();

            foreach (var item in TTsT)
            {
                string header = item[0].ToString();
                List<string> data = item[1] as List<string>;
                data.Insert(0, header);
                if (data == null || data.Count == 0) continue;

                FlowLayoutPanel listPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.TopDown,
                    AutoSize = true,
                    Padding = new Padding(10),
                    Dock = DockStyle.Top
                };

                Label categoryLabel = new Label
                {
                    Text = header.Length > 15 ? header.Substring(0, 20) + "" : header,
                    AutoSize = false,
                    Width = fixedLabelWidth,
                    Height = 25,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new System.Drawing.Font("Arial", 10, FontStyle.Bold),
                    ForeColor = System.Drawing.Color.Black,
                    AutoEllipsis = true
                };
                toolTip.SetToolTip(categoryLabel, header);
                listPanel.Controls.Add(categoryLabel);

                System.Windows.Forms.ComboBox comboBox = new System.Windows.Forms.ComboBox
                {
                    Width = 300,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                toolTip.SetToolTip(comboBox, header);
                comboBox.Items.Add("None");
                comboBox.Items.AddRange(data.ToArray());

                if (previousSelections.ContainsKey(header))
                {
                    comboBox.SelectedItem = previousSelections[header];
                }
                else
                {
                    comboBox.SelectedIndex = 0;
                }

                allComboBoxes.Add(comboBox);

                //comboBox.MouseHover += (sender, e) =>
                //{
                //    if (comboBox.SelectedItem != null)
                //        toolTip.SetToolTip(comboBox, comboBox.SelectedItem.ToString());
                //};
                comboBox.SelectedIndexChanged += (sender, e) =>
                {
                    if (comboBox.SelectedIndex <= 0) return;

                    foreach (var cb in allComboBoxes)
                    {
                        //if (cb != comboBox) cb.Enabled = false;
                    }
                    selectedItem1 = comboBox.SelectedItem.ToString();
                    string selectedItem = comboBox.SelectedItem.ToString();
                    var match = Regex.Match(selectedItem, @"(?:_Trans|Transition|_S)\s*(\d+(_T\d+)?)",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    tatName = selectedItem;
                    List<string> foundList = new List<string>();

                    string namebox = comboBox.AccessibilityObject.Name;
                    if (match.Success )
                    {
                        string selectedValue = match.Groups[1].Value;
                        foreach (var innerList in tagAndRangeList)
                        {
                            if (innerList[0].Split(',')[1].Contains(namebox))
                            {
                                foreach (string entry in innerList)
                                {
                                    string[] parts = entry.Split(new[] { ',' }, 2);
                                    if (parts.Length == 2)
                                    {
                                        string tagName = parts[0].Trim();
                                        string rangeText = parts[1].Trim();

                                        var nums = rangeText
                                                    .Trim('[', ']')
                                                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                    .Select(s1 => int.TryParse(s1.Trim(), out int num)
                                                    ? num.ToString() : s1.Trim())
                                                    .ToList();

                                        bool exists = nums.Any(x => x == selectedValue.Trim());
                                        if (exists)
                                        {
                                            foundList.Add(tagName + "," + innerList[0].Split(',')[1]);
                                        }
                                    }
                                }
                            }
                        }
                        var notFoundList = foundList.Where(item1 => inputControls.ContainsValue(item1)).ToList();

                        foreach (var ti in inputControls)
                        {
                            if (!notFoundList.Contains(ti.Value) && (ti.Key.BackColor ==
                            System.Drawing.Color.LightGreen || ti.Key.BackColor == System.Drawing.Color.IndianRed))
                            {
                                ti.Key.BackColor = System.Drawing.Color.IndianRed;
                            }
                            else
                            {
                                if (ti.Key.BackColor == System.Drawing.Color.IndianRed && notFoundList.Contains(ti.Value))
                                {
                                    ti.Key.BackColor = System.Drawing.Color.LightGreen;
                                }
                            }
                        }
                    }
                };
                inputControls.Add(comboBox, header);
                listPanel.Controls.Add(comboBox);
                dropdownPanel.Controls.Add(listPanel);
            }
        }
        void AddListSections(List<List<List<string>>> descriptions)
        {
            HashSet<string> uniqueFirstValues = new HashSet<string>();

            foreach (var flowGroup in descriptions)
            {
                if (flowGroup.Count > 0 && flowGroup[0].Count > 0)
                {
                    uniqueFirstValues.Add(flowGroup[0][0]);
                }
            }
            FlowLayoutPanel outerPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                //AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10),
                BorderStyle = BorderStyle.None
            };
            int fixedLabelWidth = 200;
            System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
            foreach (var t in uniqueFirstValues)
            {
                FlowLayoutPanel listSectionPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true,
                    Padding = new Padding(10),
                    Dock = DockStyle.Right,
                    AutoScroll = true
                };
                System.Windows.Forms.ListBox listBox = new System.Windows.Forms.ListBox
                {
                    Width = screenWidth / 6,
                    Height = screenHeight / 2,
                    ScrollAlwaysVisible = true
                };
                FlowLayoutPanel detailPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true,
                    Padding = new Padding(10),
                    Width = screenWidth / 6,
                    AutoScroll = true
                };
                FlowLayoutPanel statusPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true,
                    Padding = new Padding(10),
                    BackColor = System.Drawing.Color.Black,
                    Location = new Point(listBox.Bottom + 10),
                    AutoScroll = true
                };
                mainPanel.Controls.Add(statusPanel);
                Dictionary<string, List<string>> itemMap = new Dictionary<string, List<string>>();
                HashSet<string> addedLinkedKeys = new HashSet<string>();
                //foreach (var sublist in descriptions)
                //{
                //    if (sublist.Count > 0)
                //    {
                //        string key = sublist[0];
                //        itemMap[key] = sublist.Skip(1).ToList();
                //        //listBox.Items.Add(key);
                //        if (key.Contains("&"))
                //        {
                //            var parts = key.Split('&').Select(p => p.Trim()).ToList();
                //            if (parts.Count > 1)
                //            {
                //                string linkedKey = parts[1];
                //                if (!addedLinkedKeys.Contains(linkedKey))
                //                {
                //                    listBox.Items.Add(key);
                //                    addedLinkedKeys.Add(linkedKey);
                //                }
                //            }
                //        }
                //        else
                //        {
                //            listBox.Items.Add(key);
                //        }
                //    }
                //}
                foreach (var sublist in Discriptions)
                {
                    if (sublist.Count > 1)
                    {
                        if (sublist[0][0].Contains(t))
                        {
                            string key = sublist[0][1];
                            List<string> data = sublist[1];

                            itemMap[key] = data;

                            if (key.Contains("&"))
                            {
                                var parts = key.Split('&').Select(p => p.Trim()).ToList();

                                if (parts.Count > 1)
                                {
                                    string linkedKey = parts[1];

                                    if (!addedLinkedKeys.Contains(linkedKey))
                                    {
                                        listBox.Items.Add(key);
                                        addedLinkedKeys.Add(linkedKey);
                                    }
                                }
                            }
                            else
                            {
                                listBox.Items.Add(key);
                            }
                        }
                    }
                }
                int a = 0;

                listBox.SelectedIndexChanged += (sender, e) =>
                {
                    detailPanel.Controls.Clear();
                    int testedOkCount = 0;
                    int testedNotOkCount = 0;
                    Label testedOkLabel = new Label
                    {
                        AutoSize = true,
                        Font =
                        new System.Drawing.Font("Arial", 9, FontStyle.Bold),
                        ForeColor = System.Drawing.Color.Green
                    };
                    Label testedNotOkLabel = new Label
                    {
                        AutoSize = true,
                        Font =
                        new System.Drawing.Font("Arial", 9, FontStyle.Bold),
                        ForeColor = System.Drawing.Color.Red
                    };
                    if (listBox.SelectedItem != null)
                    {
                        string selectedKey = listBox.SelectedItem.ToString();
                        if (selectedKey.Contains("&"))
                        {
                            var keyParts = selectedKey.Split('&').Select(p => p.Trim()).ToList();
                            string mainLabel = keyParts[0];
                            string linkedKey = keyParts[1];
                            FlowLayoutPanel topDownPanel = new FlowLayoutPanel
                            {
                                FlowDirection = FlowDirection.TopDown,
                                AutoSize = true,
                                WrapContents = false
                            };
                            try
                            {
                                var matchingKeys = itemMap.Keys
                                    .Where(k => k.Contains("&"))
                                    .Where(k =>
                                    {
                                        var parts = k.Split('&').Select(p => p.Trim()).ToList();
                                        return parts.Any(part => part.Equals(linkedKey));
                                    })
                                    .ToList();

                                string firstMatchingKey = matchingKeys.FirstOrDefault();
                                if (!string.IsNullOrEmpty(firstMatchingKey) && itemMap.ContainsKey(firstMatchingKey))
                                {
                                    foreach (var matchingKey in matchingKeys)
                                    {
                                        if (itemMap.ContainsKey(matchingKey))
                                        {
                                            topDownPanel.Controls.Add(CreateDetailTable(matchingKey, itemMap[matchingKey]));
                                            foreach (var item in itemMap[matchingKey])
                                            {
                                                if (item.Contains("Tested Ok")) testedOkCount++;
                                                else if (item.Contains("Tested Not Ok")) testedNotOkCount++;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("");
                            }
                            Panel scrollablePanel = new Panel
                            {
                                AutoScroll = true,
                                Width = screenWidth / 2,
                                Height = screenHeight - 200
                            };
                            scrollablePanel.Controls.Add(topDownPanel);
                            detailPanel.Controls.Add(scrollablePanel);
                        }
                        else
                        {
                            if (itemMap.ContainsKey(selectedKey))
                            {
                                Panel scrollablePanel = new Panel
                                {
                                    AutoScroll = true,
                                    Width = screenWidth / 2,
                                    Height = screenHeight - 200
                                };
                                scrollablePanel.Controls.Add(CreateDetailTable(selectedKey, itemMap[selectedKey]));
                                detailPanel.Controls.Add(scrollablePanel);
                                int length = detailPanel.Height;
                                int breadth = detailPanel.Width;
                                statusPanel.ResetText();
                                int screenHeightdata = Screen.PrimaryScreen.Bounds.Height;
                                FlowLayoutPanel statusPanel1 = new FlowLayoutPanel
                                {
                                    FlowDirection = FlowDirection.LeftToRight,
                                    AutoSize = true,
                                    Padding = new Padding(10),
                                    BackColor = System.Drawing.Color.White,
                                };
                                foreach (var item in itemMap[selectedKey])
                                {
                                    if (item.Contains("Tested Ok"))
                                    {
                                        //testedOkLabel.Text = item;
                                        //statusPanel1.Controls.Add(testedOkLabel);
                                        //string text = testedOkLabel.Text;
                                    }
                                    else if (item.Contains("Tested Not Ok"))
                                    {
                                        //testedNotOkLabel.Text = item;
                                        //statusPanel1.Controls.Add(testedNotOkLabel);
                                        //string text = testedOkLabel.Text;
                                    }
                                }
                                string text1 = testedOkLabel.Text;

                                detailPanel.Controls.Add(statusPanel1);
                                scrollablePanel.Controls.Add(statusPanel1);
                            }
                        }
                    }
                };
                listSectionPanel.Controls.Add(listBox);
                listSectionPanel.Controls.Add(detailPanel);
                outerPanel.Controls.Add(listSectionPanel);
            }
            mainPanel.Controls.Add(outerPanel);
        }

        TableLayoutPanel CreateDetailTable(string title, List<string> items)
        {
            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                Padding = new Padding(5)
            };
            tableLayoutPanel.Controls.Add(new Label
            {
                Text = title,
                Font = new System.Drawing.Font("Arial", 9, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(3)
            }, 0, 0);
            tableLayoutPanel.SetColumnSpan(tableLayoutPanel.Controls[tableLayoutPanel.Controls.Count - 1], 2);

            int row = 1;
            foreach (var item in items)
            {
                string[] parts = item.Split(':');
                if (parts.Length == 2)
                {
                    string param = parts[0].Trim();
                    string value = parts[1].Trim();

                    Label paramLabel = new Label { Text = param, AutoSize = true, Padding = new Padding(3) };
                    Label valueLabel = new Label
                    {
                        Text = value,
                        AutoSize = true,
                        Padding = new Padding(3),
                        ForeColor = value.Contains("Tested Ok") ? System.Drawing.Color.Green :
                                     value.Contains("Tested Not Ok") ? System.Drawing.Color.Red :
                                     System.Drawing.Color.Black
                    };
                    tableLayoutPanel.Controls.Add(paramLabel, 0, row);
                    tableLayoutPanel.Controls.Add(valueLabel, 1, row);
                }
                else
                {
                    Label label = new Label
                    {
                        Text = item,
                        Font = new System.Drawing.Font("Arial", 8, FontStyle.Bold),
                        AutoSize = true,
                        Padding = new Padding(3),
                        ForeColor = item.Contains("Tested Ok") ? System.Drawing.Color.Green :
                                     item.Contains("Tested Not Ok") ? System.Drawing.Color.Red :
                                     System.Drawing.Color.Black
                    };
                    tableLayoutPanel.Controls.Add(label, 0, row);
                    tableLayoutPanel.SetColumnSpan(label, 2);
                }
                row++;
            }
            return tableLayoutPanel;
        }

        AddDropdownListsFromTTST("Select TAT", TTST/*,tagAndRangeList*/);
        AddInputs("Select Input:", options);
        //AddInputs("Select Inout:", Inout);

        foreach (Control ctrl in inputsPanel.Controls)
        {
            string info = $"Type: {ctrl.GetType().Name}, Name: {ctrl.Name}";

            if (ctrl is Label label)
            {
                info += $", Text: {label.Text}";
            }
            else if (ctrl is Button button)
            {
                info += $", Text: {button.Text}";
            }
            else if (ctrl is TableLayoutPanel table)
            {
                info += $", Rows: {table.RowCount}, Columns: {table.ColumnCount}";
            }
            Console.WriteLine(info);
        }

        AddListSections(disc);
        //DisplayDataWithInputs(Inout, mainPanel);  
        Panel bottomPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 60
        };
        Button submitButton = new Button
        {
            Text = "Submit",
            Height = 50,
            Width = 150,
            Left = 10,
            Top = 5
        };

        List<string> formattedInputsList = new List<string>();
        submitButton.Click += (sender, e) =>
        {
            previousSelections.Clear();
            selectedValues.Clear();
            foreach (var control in inputControls)
            {
                string tagName = control.Value;
                string value = "";
                string dataType = "Bool";
                string range = "";
                if (control.Key is Panel statusPanel)
                {
                    value = statusPanel.BackColor == System.Drawing.Color.LightGreen ? "Checked" : "Unchecked";
                    selectedValues.Add("Tag," + tagName + " = " + value);
                    previousSelections[tagName] = value;
                }
                else if (control.Key is Button toggleBtn)
                {
                    bool isOn = (bool)toggleBtn.Tag;
                    value = isOn ? "TRUE" : "FALSE";
                    selectedValues.Add("Tag," + tagName + " = " + value);
                    previousSelections[tagName] = value;
                }
                else if (control.Key is System.Windows.Forms.TextBox textBox && !string.IsNullOrWhiteSpace(textBox.Text))
                {
                    value = textBox.Text.Trim();
                    selectedValues.Add("Tag," + tagName + " = " + value);
                    previousSelections[tagName] = value;
                }
                else if (control.Key is System.Windows.Forms.ComboBox comboBox && comboBox.SelectedItem != null)
                {
                    value = comboBox.SelectedItem.ToString();
                    selectedValues.Add("TTST," + tagName + " = " + value);
                    previousSelections[tagName] = value;
                    tatName = tagName;
                }
                System.Windows.Forms.TextBox rangeBox = FindRangeTextBox(control.Key);
                if (rangeBox != null && !string.IsNullOrWhiteSpace(rangeBox.Text))
                {
                    range = rangeBox.Text.Trim();
                }
                string formattedLine = $"{tagName}, [{value}], [{range}]/, {dataType}";
                formattedInputsList.Add(formattedLine);
            }
            CopyMatchingTatFile(tatName);
            form.DialogResult = DialogResult.OK;
            form.Close();
        };

        Button submitButtonSingleTAT = new Button
        {
            Text = "Single Function Run",
            Height = 50,
            Width = 180,
            Left = submitButton.Right + 20,
            Top = 5
        };

        submitButtonSingleTAT.Click += (sender, e) =>
        {
            previousSelections.Clear();
            selectedValues.Clear();
            string filename = "";

            foreach (var control in inputControls)
            {
                string tagName = control.Value;
                string value = "";
                string dataType = "Bool";
                string range = "";
                if (control.Key is Panel statusPanel)
                {
                    value = statusPanel.BackColor == System.Drawing.Color.LightGreen ? "Checked" : "Unchecked";
                    selectedValues.Add("Tag," + tagName + " = " + value);
                    previousSelections[tagName] = value;
                }
                else if (control.Key is Button toggleBtn)
                {
                    bool isOn = (bool)toggleBtn.Tag;
                    value = isOn ? "TRUE" : "FALSE";
                    selectedValues.Add("Tag," + tagName + " = " + value);
                    previousSelections[tagName] = value;
                }
                else if (control.Key is System.Windows.Forms.TextBox textBox && !string.IsNullOrWhiteSpace(textBox.Text))
                {
                    value = textBox.Text.Trim();
                    selectedValues.Add("Tag," + tagName + " = " + value);
                    previousSelections[tagName] = value;
                }
                else if (control.Key is System.Windows.Forms.ComboBox comboBox && comboBox.SelectedItem != null)
                {
                    value = comboBox.SelectedItem.ToString();
                    selectedValues.Add("TTST," + tagName + " = " + value);
                    previousSelections[tagName] = value;
                    filename = tagName;
                    tatName = value;
                }
                System.Windows.Forms.TextBox rangeBox = FindRangeTextBox(control.Key);
                if (rangeBox != null && !string.IsNullOrWhiteSpace(rangeBox.Text))
                {
                    range = rangeBox.Text.Trim();
                }
                string formattedLine = $"{tagName}, [{value}], [{range}]/, {dataType}";
                formattedInputsList.Add(formattedLine);
            }
            CopyMatchingTatFile(filename);
            singletatUpdate(tatName, filename);
            form.DialogResult = DialogResult.OK;
            form.Close();
        };

        Button generateReportButton = new Button
        {
            Text = "Generate Report",
            Dock = DockStyle.Bottom,
            Height = 50,
            Width = 150,
            Enabled = disc.Count > 0
        };
        generateReportButton.Click += (sender, e) =>
        {
            //generateReport(disc, options);            
        };
        form.Controls.Add(generateReportButton);
        bottomPanel.Controls.Add(submitButton);
        bottomPanel.Controls.Add(submitButtonSingleTAT);
        form.Controls.Add(bottomPanel);
        form.ShowDialog();

        return options;
    }
    static string csvPath = "";
    public static void blockCalling(string exportPath)
    {
        if (string.IsNullOrWhiteSpace(exportPath) || !Directory.Exists(exportPath))
        {
            MessageBox.Show($"Export path is invalid or does not exist:\n{exportPath}", "Path Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var xmlFiles = Directory.GetFiles(exportPath, "*.xml");

        if (xmlFiles.Length == 0)
        {
            MessageBox.Show("No XML files found in export folder.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        foreach (string xmlFile in xmlFiles)
        {
            extractTagData(xmlFile);
        }

        combineMain();
    }

    static Dictionary<string, string> blockDB = new Dictionary<string, string>();
    [STAThread] 
    public static void combineMain()
    {
        //string mainPath = @"C:\Generate Block\V20 All Files\Generated XML\Main.xml";
        string mainPath = @"C:\Generate Block\V20 All Files\Generated XML\Main.xml";

        if (File.Exists(mainPath))
        {
            //MessageBox.Show("File exists. Proceed with connection.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(mainPath));

                string initialContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Root></Root>";
                File.WriteAllText(mainPath, initialContent);

                MessageBox.Show("File did not exist. New XML file created successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating XML file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        string mainDataPath = @"C:\Generate Block\V20 All Files\Generated XML\MainData.xml";
        string xmlContent = File.ReadAllText(mainPath);
        string newXml = File.ReadAllText(mainDataPath);

        string patternToFind = @"</MultilingualText>\s*<SW.Blocks.CompileUnit";
        string replacement = $"</MultilingualText>\n{xmlContent}\n<SW.Blocks.CompileUnit";
        string updatedContent = System.Text.RegularExpressions.Regex.Replace(newXml, patternToFind, replacement);

        string downloadPath = @"C:\Generate Block\V20 All Files\DB XML\" + "Main.xml";

        File.WriteAllText(downloadPath, updatedContent);

        //File.WriteAllText(mainDataPath,"");
        //File.Copy(mainPath,downloadPath);
    }
    public static void extractTagData(string xmlPath)
    {
        XDocument xmlDoc = XDocument.Load(xmlPath);
        XNamespace ns = "http://www.siemens.com/automation/Openness/SW/Interface/v5";
        List<string> inputNames = new List<string>();
        List<string> inputtype = new List<string>();
        List<string> outputNames = new List<string>();
        List<string> outputtype = new List<string>();
        List<string> InoutputNames = new List<string>();
        List<string> Inoutputtype = new List<string>();
        List<string> TagsData = new List<string>();
        var blockName = xmlDoc.Descendants("Name").FirstOrDefault();
        var inputSection = xmlDoc.Descendants(ns + "Section")
            .FirstOrDefault(section => (string)section.Attribute("Name") == "Input");
        var inputOutSection = xmlDoc.Descendants(ns + "Section")
           .FirstOrDefault(section => (string)section.Attribute("Name") == "InOut");
        var outputSection = xmlDoc.Descendants(ns + "Section")
            .FirstOrDefault(section => (string)section.Attribute("Name") == "Output");
        if (inputSection != null)
        {
            foreach (var member in inputSection.Elements(ns + "Member"))
            {
                string memberName = member.Attribute("Name")?.Value;
                string memberType = member.Attribute("Datatype")?.Value;

                if (!string.IsNullOrEmpty(memberName) && !string.IsNullOrEmpty(memberType))
                {
                    inputNames.Add(memberName);
                    inputtype.Add(memberName + "," + memberType);
                    TagsData.Add(memberName + "," + memberType + "," + "Input");
                }
            }
        }

        if (outputSection != null)
        {
            foreach (var member in outputSection.Elements(ns + "Member"))
            {
                string memberName = member.Attribute("Name")?.Value;
                string memberType = member.Attribute("Datatype")?.Value;

                if (!string.IsNullOrEmpty(memberName) && !string.IsNullOrEmpty(memberType))
                {
                    outputNames.Add(memberName);
                    outputtype.Add(memberName + "," + memberType);
                    TagsData.Add(memberName + "," + memberType + "," + "Output");
                }
            }
        }

        if (inputOutSection != null)
        {
            foreach (var member in inputOutSection.Elements(ns + "Member"))
            {
                string memberName = member.Attribute("Name")?.Value;
                string memberType = member.Attribute("Datatype")?.Value;

                if (!string.IsNullOrEmpty(memberName) && !string.IsNullOrEmpty(memberType))
                {
                    InoutputNames.Add(memberName);
                    Inoutputtype.Add(memberName + "," + memberType);
                    TagsData.Add(memberName + "," + memberType + "," + "InOut");
                }
            }
        }

        string finalXMLData = "";
        string xmldataHeader = @"<?xml version=""1.0"" encoding=""utf-8""?>
    <Document>
      <Engineering version=""V20"" />
      <DocumentInfo>
        <Created>2025-07-07T08:55:10.1278921Z</Created>
        <ExportSetting>None</ExportSetting>
        <InstalledProducts>
          <Product>
            <DisplayName>Totally Integrated Automation Portal</DisplayName>
            <DisplayVersion>V20 Update 5</DisplayVersion>
          </Product>
          <OptionPackage>
            <DisplayName>TIA Portal Openness</DisplayName>
            <DisplayVersion>V20 Update 5</DisplayVersion>
          </OptionPackage>
          <OptionPackage>
            <DisplayName>TIA Portal Version Control Interface</DisplayName>
            <DisplayVersion>V20</DisplayVersion>
          </OptionPackage>
          <Product>
            <DisplayName>SINAMICS Startdrive Advanced</DisplayName>
            <DisplayVersion>V20</DisplayVersion>
          </Product>
          <OptionPackage>
            <DisplayName>SINAMICS Startdrive G130, G150, S120, S150, SINAMICS MV, S210</DisplayName>
            <DisplayVersion>V20</DisplayVersion>
          </OptionPackage>
          <OptionPackage>
            <DisplayName>SINAMICS Startdrive G110M, G120, G120C, G120D, G120P, G115D</DisplayName>
            <DisplayVersion>V20</DisplayVersion>
          </OptionPackage>
          <Product>
            <DisplayName>STEP 7 Professional</DisplayName>
            <DisplayVersion>V20 Update 5</DisplayVersion>
          </Product>
          <OptionPackage>
            <DisplayName>STEP 7 Safety</DisplayName>
            <DisplayVersion>V20</DisplayVersion>
          </OptionPackage>
          <Product>
            <DisplayName>WinCC Advanced</DisplayName>
            <DisplayVersion>V20 Update 5</DisplayVersion>
          </Product>
        </InstalledProducts>
      </DocumentInfo>";

        finalXMLData = xmldataHeader + Environment.NewLine;
        string xmlDataName = @"<SW.Blocks.InstanceDB ID=""0"">
        <AttributeList>
          <InstanceOfName>$</InstanceOfName>
          <InstanceOfType>FB</InstanceOfType>
          <Interface><Sections xmlns=""http://www.siemens.com/automation/Openness/SW/Interface/v5"">";

        string updatedXmlDataName = xmlDataName.Replace("$", blockName.Value);

        string startTagType = @"<Section Name=""$"">";

        string endTag = @"</Section>";
        string tag = @"<Member Name=""{0}"" Datatype=""{1}"" Remanence=""NonRetain"" />";
        string updatedTag = "";
        startTagType = @"<Section Name=""Input"">";

        foreach (var tags in inputtype)
        {
            var parts = tags.Split(',');
            if (parts.Length >= 2)
            {
                string name = parts[0].Trim();
                string datatype = parts[1].Trim();
                updatedTag += string.Format(tag, name, datatype) + Environment.NewLine;
                //updatedTag += tag.Replace("$", name).Replace("$$", datatype) + Environment.NewLine;
            }
        }

        string endSection = "</Section>";

        finalXMLData += updatedXmlDataName + Environment.NewLine + startTagType + Environment.NewLine +
                        updatedTag + Environment.NewLine + endSection + Environment.NewLine;
        updatedTag = "";
        startTagType = @"<Section Name=""Output"">";
        foreach (var tags in outputtype)
        {
            var parts = tags.Split(',');
            if (parts.Length >= 2)
            {
                string name = parts[0].Trim();
                string datatype = parts[1].Trim();
                updatedTag += string.Format(tag, name, datatype) + Environment.NewLine;
            }
        }

        finalXMLData += startTagType + Environment.NewLine + updatedTag + Environment.NewLine
                        + endSection + Environment.NewLine;
        updatedTag = "";
        startTagType = @"<Section Name=""InOut"">";
        foreach (var tags in Inoutputtype)
        {
            var parts = tags.Split(',');
            if (parts.Length >= 2)
            {
                string name = parts[0].Trim();
                string datatype = parts[1].Trim();
                updatedTag += string.Format(tag, name, datatype) + Environment.NewLine;
            }
        }

        finalXMLData += startTagType + Environment.NewLine + updatedTag
            + Environment.NewLine + endSection + Environment.NewLine;

        string endXMLTag = @"</Sections></Interface>
      <Name>$</Name>
      <Namespace />
      <Number>1</Number>
      <ProgrammingLanguage>DB</ProgrammingLanguage>
    </AttributeList>
    <ObjectList>
      <MultilingualText ID=""1"" CompositionName=""Comment"">
        <ObjectList>
          <MultilingualTextItem ID=""2"" CompositionName=""Items"">
            <AttributeList>
              <Culture>en-US</Culture>
              <Text />
            </AttributeList>
          </MultilingualTextItem>
        </ObjectList>
      </MultilingualText>
      <MultilingualText ID=""3"" CompositionName=""Title"">
        <ObjectList>
          <MultilingualTextItem ID=""4"" CompositionName=""Items"">
            <AttributeList>
              <Culture>en-US</Culture>
              <Text />
            </AttributeList>
          </MultilingualTextItem>
        </ObjectList>
      </MultilingualText>
    </ObjectList>
  </SW.Blocks.InstanceDB>
</Document>";

        //string updatedEndXMLTag = endXMLTag.Replace("$", Path.GetFileNameWithoutExtension(xmlPath) + "_DB");

        ////string downloadPath = @"C:\Users\Wpuser\Downloads\Final\Generate Block\Generated XML\DB Xml\" + blockName.Value + "_DB.xml";
        //string downloadPath = @"C:\Generate Block\V20 All Files\DB XML\" + blockName.Value + "_DB.xml";


        //finalXMLData += updatedEndXMLTag;

        ////blockName.Add(blockName.Value, blockName.Value + "_DB");

        //File.WriteAllText(downloadPath, finalXMLData);

        //blockDB.Add(blockName.Value, blockName.Value + "_DB");

        //writeCsv(blockName.Value, inputNames, TagsData);

        //mainData(TagsData, blockName.Value + "_DB", blockName.Value);

        ////updateTTST(blockName.Value, blockName.Value + "_DB");
        ////updateTTST(new List<string> { blockName.Value }, blockName.Value + "_DB");
        //updateTTST(new List<string> { blockName.Value }, blockName.Value + "_DB");
        // Replace $ with the block name in the closing tag
        string updatedEndXMLTag = endXMLTag.Replace("$", Path.GetFileNameWithoutExtension(xmlPath) + "_DB");

        string dbXmlDirectory = @"C:\Generate Block\V20 All Files\DB XML";
        Directory.CreateDirectory(dbXmlDirectory); // Ensure folder exists

        string downloadPath = Path.Combine(dbXmlDirectory, blockName.Value + "_DB.xml");

        if (File.Exists(downloadPath))
        {
            File.Delete(downloadPath);
        }

        finalXMLData += updatedEndXMLTag;

        File.WriteAllText(downloadPath, finalXMLData);

        blockDB.Add(blockName.Value, blockName.Value + "_DB");

        writeCsv(blockName.Value, inputNames, TagsData);
        mainData(TagsData, blockName.Value + "_DB", blockName.Value);
        updateTTST(new List<string> { blockName.Value }, blockName.Value + "_DB");
    }

    static int UID = 1111;
    static List<string> Datatype = new List<string>();
    static string csvpath = "";
    static bool isFirstFile = true;

    public static string writeCsv(string blockName, List<string> inputNames, List<string> TagsData)
    {
        //string csvPath = @"C:\Generate Block\V20 All Files\Testing\Testing CSV";
        string csvPath = @"C:\Generate Block\V20 All Files\Testing\Testing CSV";

        if (!Directory.Exists(csvPath))
        {
            Directory.CreateDirectory(csvPath);
        }
        string ttstFolder = @"C:\Generate Block\V20 All Files\Testing\TTST";
        if (!Directory.Exists(ttstFolder))
        {
            Directory.CreateDirectory(ttstFolder);
        }
        
        if (string.IsNullOrEmpty(csvpath))
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            csvpath = Path.Combine(csvPath, $"Data_{timestamp}.csv");
        }

        string[] ttst = Directory.GetFiles(ttstFolder, "*.TTST");

        foreach (var file in ttst)
        {
            if (Path.GetFileNameWithoutExtension(file).Trim() == blockName.Trim())
            {
                List<string> awlData = readAWL(file, blockName);
                List<string> inputData = writeInput(inputNames, awlData);
                List<string> firstItems = TagsData.Select(item => item.Split(',')[0]).ToList();

                try
                {
                    for (int i = 0; i < inputData.Count(); i++)
                    {
                        if (inputData[i].Split(',')[0].Trim() == firstItems[i].Trim())
                            firstItems[i] = inputData[i];
                    }
                    List<string> tempList = new List<string> { "Block_Name : ," + blockName };
                    tempList.AddRange(firstItems);
                    AllDatatypeEntries.Add(tempList);

                    Datatype = firstItems;
                    firstItems = firstItems.Select(s => s.Replace("[", "\"[")).ToList();
                    firstItems = firstItems.Select(s => s.Replace("]", "]\"")).ToList();

                    List<string> linesToWrite = new List<string>();

                    if (isFirstFile)
                    {
                        linesToWrite.Add("Block_Name : ," + blockName);
                        linesToWrite.Add("");
                        linesToWrite.Add("Tag,Value,State Number");
                        isFirstFile = false;
                    }
                    else
                    {
                        linesToWrite.Add("");
                        linesToWrite.Add("Block_Name : ," + blockName);
                        linesToWrite.Add("");
                        linesToWrite.Add("Tag,Value,State Number");
                    }

                    linesToWrite.AddRange(firstItems);

                    File.AppendAllLines(csvpath, linesToWrite);

                    return csvpath;
                }
                catch
                {
                }
            }
        }

        return null;
    }

    //static string csvpath = "";
    //public static string writeCsv(string blockName, List<string> inputNames, List<string> TagsData)
    //{
    //    string csvPath = @"C:\Generate Block\V20 All Files\Testing\Testing CSV";
    //    string ttstFolder = @"C:\Generate Block\V20 All Files\Testing\TTST";

    //    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
    //    string filePath = Path.Combine(csvPath, $"Data_{timestamp}.csv");
    //    csvpath = filePath;
    //    string[] ttst = Directory.GetFiles(ttstFolder, "*.TTST");

    //    foreach (var file in ttst)
    //    {
    //        if (Path.GetFileNameWithoutExtension(file).Trim() == blockName.Trim())
    //        {
    //            List<string> awlData = readAWL(file, blockName);
    //            List<string> inputData = writeInput(inputNames, awlData);
    //            List<string> firstItems = TagsData.Select(item => item.Split(',')[0]).ToList();

    //            try
    //            {
    //                for (int i = 0; i < inputData.Count(); i++)
    //                {
    //                    if (inputData[i].Split(',')[0].Trim() == firstItems[i].Trim())
    //                        firstItems[i] = inputData[i];
    //                }

    //                Datatype = firstItems;
    //                firstItems = firstItems.Select(s => s.Replace("[", "\"[")).ToList();
    //                firstItems = firstItems.Select(s => s.Replace("]", "]\"")).ToList();

    //                firstItems.Insert(0, "Block_Name : ," + blockName);
    //                firstItems.Insert(1, "");
    //                firstItems.Insert(2, "Tag,Value,State Number");

    //                File.WriteAllLines(filePath, firstItems);

    //                return filePath; 
    //            }
    //            catch
    //            {
    //                
    //            }
    //        }
    //    }

    //    return null;
    //}

    public static void mainData(List<string> Tags, string dbName, string fbName)
    {
        int accessUid = 21;
        ++UID;

        string swTag = @"<SW.Blocks.CompileUnit ID=""{0}"" CompositionName=""CompileUnits"">
        <AttributeList>
          <NetworkSource><FlgNet xmlns=""http://www.siemens.com/automation/Openness/SW/NetworkSource/FlgNet/v4"">
        <Parts>";

        string updatedSWTag = string.Format(swTag, ++UID);

        string endParts = @"</CallInfo>
    </Call>
  </Parts>";

        string callInfoTag = @"<Call UId=""2111"">
      <CallInfo Name=""{0}"" BlockType=""FB"">
        <Instance Scope=""GlobalVariable"" UId=""{1}"">
          <Component Name=""{2}"" />
        </Instance>";

        string updatedCallInfoTag = string.Format(callInfoTag, fbName, ++UID, dbName);

        string wireStart = "<Wires>";
        string wireEnd = "</Wires>\r\n</FlgNet></NetworkSource>";

        string accessTag = @"<Access Scope=""GlobalVariable"" UId=""{0}"">
      <Symbol>
        <Component Name=""{1}"" />
        <Component Name=""{2}"" />
      </Symbol>
    </Access>";

        Dictionary<int, string> accessTagList = new Dictionary<int, string>();
        string updatedAccessTag = "";
        foreach (var tag in Tags)
        {
            var parts = tag.Split(',');
            if (parts.Length >= 2)
            {
                string name = parts[0].Trim();
                string datatype = parts[1].Trim();
                updatedAccessTag += string.Format(accessTag, ++accessUid, dbName, name) + Environment.NewLine;
                accessTagList.Add(accessUid, name);
            }
        }

        string parameterTag = @"<Parameter Name=""{0}"" Section=""{1}"" Type=""{2}"" />";
        string updatedParameterTag = "";
        foreach (var tag in Tags)
        {
            var parts = tag.Split(',');
            if (parts.Length >= 2)
            {
                string name = parts[0].Trim();
                string datatype = parts[1].Trim();
                string type = parts[2].Trim();
                updatedParameterTag += string.Format(parameterTag, name, type, datatype) + Environment.NewLine;
            }
        }

        string powerRail = @"<Wire UId=""{0}"">
      <Powerrail />
      <NameCon UId=""2111"" Name=""en"" />
    </Wire>";

        string updatedPowerRail = string.Format(powerRail, ++UID);
        string wireTag = @"<Wire UId=""{0}"">
      <IdentCon UId=""{1}"" />
      <NameCon UId=""2111"" Name=""{2}"" />
    </Wire>";

        string endSWTag = $"<ProgrammingLanguage>LAD</ProgrammingLanguage>\r\n        </AttributeList>\r\n        <ObjectList>\r\n          <MultilingualText ID=\"{++UID}\" CompositionName=\"Comment\">\r\n            <ObjectList>\r\n              <MultilingualTextItem ID=\"{++UID}\" CompositionName=\"Items\">\r\n                <AttributeList>\r\n                  <Culture>en-US</Culture>\r\n                  <Text />\r\n                </AttributeList>\r\n              </MultilingualTextItem>\r\n            </ObjectList>\r\n          </MultilingualText>\r\n          <MultilingualText ID=\"{++UID}\" CompositionName=\"Title\">\r\n            <ObjectList>\r\n              <MultilingualTextItem ID=\"{++UID}\" CompositionName=\"Items\">\r\n                <AttributeList>\r\n                  <Culture>en-US</Culture>\r\n                  <Text />\r\n                </AttributeList>\r\n              </MultilingualTextItem>\r\n            </ObjectList>\r\n          </MultilingualText>\r\n        </ObjectList>\r\n      </SW.Blocks.CompileUnit>";

        string updatedWireTag = "";

        foreach (var tag in accessTagList)
        {
            updatedWireTag += string.Format(wireTag, ++UID, tag.Key, tag.Value) + Environment.NewLine;
        }

        string finalData = updatedSWTag + Environment.NewLine + updatedAccessTag + Environment.NewLine +

            updatedCallInfoTag + Environment.NewLine + updatedParameterTag + Environment.NewLine +
            endParts + Environment.NewLine + wireStart + Environment.NewLine + updatedPowerRail +
            Environment.NewLine + updatedWireTag + wireEnd + endSWTag;

        //string filePath = @"C:\Users\Wpuser\Downloads\Final\Generate Block\Generated XML\MainData.xml";
        //string filePath = @"C:\Generate Block\V20 All Files\Generated XML\Main.xml";
        //string existingContent = File.ReadAllText(filePath);
        //string updatedfinalData = existingContent + Environment.NewLine + finalData;
        //File.WriteAllText(filePath, updatedfinalData);
        string filePath = @"C:\Generate Block\V20 All Files\Generated XML\Main.xml";

        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        string existingContent = File.Exists(filePath) ? File.ReadAllText(filePath) : "";

        string updatedfinalData = existingContent + Environment.NewLine + finalData;

        File.WriteAllText(filePath, updatedfinalData);
    }

    public static List<string> readAWL(string Path, string BlockName)
    {
        List<string> fileData = new List<string>(System.IO.File.ReadLines(Path));
        List<string> outData = new List<string>();
        string extractedNumber = "";
        bool step = false;
        bool endStep = false;
        bool endVar = false;
        for (int i = 0; i < fileData.Count(); i++)
        {
            if (fileData[i].StartsWith("END_VAR"))
            {
                endVar = true;
            }

            if (fileData[i].StartsWith("TEST_CASE"/*"STEP:"*/))
            {
                step = true;
                var match = Regex.Match(fileData[i], @"(?:_Tran|_Trans|Transition|_S[s]?)\s*(\d+(?:_T\d+)?)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    extractedNumber = match.Groups[1].Value;
                }
                string data = fileData[i];
            }
            else if (!fileData[i].StartsWith("END_STEP") || step)
            {
                if (endVar)
                    outData.Add(extractedNumber.ToString() + "," + fileData[i]);
            }
        }
        outData.RemoveAll(item => !item.ToLower().Contains("true"));
        outData.RemoveAll(item => item.IndexOf("ASSERT.Equal", StringComparison.OrdinalIgnoreCase) >= 0);
        outData.RemoveAll(item => item.IndexOf("RUN(CYCLES", StringComparison.OrdinalIgnoreCase) >= 0);
        outData = outData.Select(s => s.Replace(" ", "")).ToList();
        return outData;
    }

    public static List<string> writeInput(List<string> InputData, List<string> Data)
    {
        List<string> input = new List<string>();
        foreach (var t in InputData)
        {
            foreach (var a in Data)
            {
                int count = a.Split(',').Count();
                if (a.Split(',').Count() == 2)
                {
                    string aval = a.Split(',')[1];
                    if (aval != "")
                    {
                        if (aval.Contains(":="))
                        {
                            string actval = aval.Split(':')[0].Trim();
                            if (actval == t)
                            {
                                input.Add(actval + "," + a.Split('=')[1] + "," + a.Split(',')[0]);
                            }
                        }
                    }
                }
            }
        }
        input.RemoveAll(item => item.ToLower().Contains("false"));
        input.RemoveAll(item => !item.ToLower().Contains("true"));
        Dictionary<string, List<string>> groupedData = new Dictionary<string, List<string>>();
        for (int i = 0; i < input.Count; i++)
        {
            input[i] = Regex.Replace(input[i], @"true", "True", RegexOptions.IgnoreCase);
        }
        foreach (var item in input)
        {
            if (item.Contains("iProcessStart"))
                ;
            string[] parts = item.Split(new[] { ',' }, StringSplitOptions.None);
            string key = string.Join(",", parts.Take(parts.Length - 1));
            string number = parts.Last().Trim(';');

            if (!groupedData.ContainsKey(key))
            {
                groupedData[key] = new List<string>();
            }
            groupedData[key].Add(number);
        }
        List<string> result = new List<string>();

        foreach (var entry in groupedData)
        {
            string key = entry.Key;
            string numbers = string.Join(",", entry.Value.Distinct());
            result.Add($"{key},[{numbers}]");
        }
        List<string> datanew = new List<string>();
        foreach (var t in InputData)
        {
            string data = "";
            foreach (var a in result)
            {
                if (t == a.Split(',')[0])
                {
                    data += a + "/";
                }
            }
            int count = data.Split('/').Count() - 1;
            if (count == 1)
            {
                if (data.Contains("False;"))
                    data = data.Replace("False;", "[FALSE]");
                else if (data.Contains("True;"))
                    data = data.Replace("True;", "[TRUE]");
                else if (data.Contains("FALSE;"))
                    data = data.Replace("FALSE;", "[FALSE]");
                datanew.Add(data);
            }
            else
            {
                try
                {
                    string[] arr = data.Split('/');
                    string prefix = string.Empty;
                    string booleans = string.Empty;
                    string values = string.Empty;

                    foreach (var entry in arr)
                    {
                        if (entry != "")
                        {
                            string currentPrefix = entry.Split(',')[0];
                            if (string.IsNullOrEmpty(prefix))
                            {
                                prefix = currentPrefix;
                            }
                            string boolean = entry.Split(',')[1].Split(';')[0].Trim();
                            string numberList = Regex.Match(entry, @"\[(.*?)\]").Groups[1].Value;

                            booleans += $"[{boolean}]";
                            values += $"[{numberList}]";
                        }
                    }
                    string dat = $"{prefix},{"\"" + booleans + "\""},{"\"" + values + "\""}";
                    datanew.Add(dat);
                }
                catch (Exception ex)
                {
                }
            }
        }
        return datanew;
    }
    static List<List<object>> getTat(string[] ttat)
    {
        List<List<object>> Ttst = new List<List<object>>();

        for (int i = 0; i < ttat.Count(); i++)
        {
            List<string> currentCommandList = new List<string>();
            List<object> currentFileList = new List<object> { Path.GetFileNameWithoutExtension(ttat[i]), currentCommandList };

            string input = System.IO.File.ReadAllText(ttat[i]);
            string pattern = @"SEQUENCER\s*(.*?)\s*END_SEQUENCER";
            var matches = Regex.Matches(input, pattern, RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                string stepName = match.Groups[1].Value;
                string stepContent = match.Groups[2].Value;

                string[] lines = stepName.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (!string.IsNullOrEmpty(trimmedLine) && trimmedLine.StartsWith("\"") && trimmedLine.EndsWith("();"))
                    {
                        currentCommandList.Add(trimmedLine);
                    }
                }
            }
            Ttst.Add(currentFileList);
        }
        return Ttst;
    }
    static Project proj = null;
    public static List<string> InoutputNames = new List<string>();
    public static List<List<List<string>>> Discriptions = new List<List<List<string>>>();
    static List<List<string>> AllDatatypeEntries = new List<List<string>>();
    static List<List<string>> dataOkNok = new List<List<string>>();
    public static void ImportElementAsync(Project project)
    {
        try
        {
            dataOkNok = new List<List<string>>();
            //var selectName = (string)SelectedItem.Header;
            //string filePath = @"C:\Users\Wpuser\Downloads\Final\Generate Block\Testing\TTST";//C:\Test Suits\V20_TestSuite";
            //string filePath = @"C:\Generate Block\V20 All Files\Testing";//C:\Test Suits\V20_TestSuite";
            /*        string filePath = @"C:\Generate Block\V20 All Files\Testing\TTST"; */    //C:\Test Suits\V20_TestSuite";
            string filePath = @"C:\Generate Block\V20 All Files\Testing\TTST";

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
       
            string[] tat = Directory.GetFiles(filePath, "*.tat");
            string[] ttst = Directory.GetFiles(filePath, "*.TTST");
            List<List<object>> TTST = getTat(ttst);
            List<List<object>> finalTTST = new List<List<object>>();

            List<List<string>> options = new List<List<string>>();
            List<string> fileOptions = new List<string>();
            foreach (var t in ttst)
            {
                fileOptions.Add(Path.GetFileNameWithoutExtension(t));
            }

            string selectedOption = "";
            bool test = true;
            Dictionary<string, string> previousSelections = new Dictionary<string, string>();
            IList<FileInfo> fileInfos1 = new List<FileInfo>();
            //string[] fil = Directory.GetFiles(@"C:\Users\Wpuser\Downloads\Final\Generate Block\Excel Files", "*.xlsx");
            //string[] fil = Directory.GetFiles(@"C:\Generate Block\V20 All Files\Export Files", "*.xlsx");
            //string[] fil = Directory.GetFiles(@"C:\Generate Block\Backup XML Files V17", "*.xlsx");
            string[] fil = Directory.GetFiles(@"C:\Generate Block\Backup Excel Files", "*.xlsx");
            string validateExcel = "";
            foreach (var t in fil)
            {
                string fileName = Path.GetFileNameWithoutExtension(t);
                if (fileOptions[0].Contains(fileName))
                {
                    validateExcel = t;
                }
            }
            //validateExcel = "PR_TwoPosTwoLayer_Soft2_f5e808e9-9b16-4683-b81c-08f537fd4a6c_V11_1";
            //List<string> Data = new List<string>();
            if (validateExcel != "")
            {
                //Data = ProjectServiceProvider.ReadModuleExcelData(validateExcel);
            }
            List<string> data = Datatype;
            List<string> tagAndRangeList = new List<string>();
            List<List<string>> AllData = AllDatatypeEntries;
            //foreach (var t in TTST)
            //{
            //    if (t[0].ToString() == "PR_GantryUnload_Seqv2 5_dc4239")
            //    {
            //        finalTTST.Add(t);
            //    }
            //}
            
            string ttstFolder = @"C:\Generate Block\V20 All Files\Testing\TTST";
            string[] ttstFiles = Directory.GetFiles(ttstFolder, "*.TTST");

            HashSet<string> ttstFileNamesWithoutExtension = ttstFiles
                .Select(file => Path.GetFileNameWithoutExtension(file))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            finalTTST.Clear();

            foreach (var t in TTST)
            {
                string blockName = t[0].ToString().Trim();
                if (ttstFileNamesWithoutExtension.Contains(blockName))
                {
                    finalTTST.Add(t);
                }
            }
            
            //foreach (var line in data)
            //{
            //    Match match = Regex.Match(line, @"^(.*?),\s*\[.*?\],\s*\[(.*?)\]");

            //    if (match.Success)
            //    {
            //        string tag = match.Groups[1].Value.Trim();
            //        string range = match.Groups[2].Value.Trim();

            //        tagAndRangeList.Add($"{tag}, [{range}]");
            //    }
            //}
            List<List<string>> allTagAndRangeLists = new List<List<string>>();
            foreach (var data1 in AllData)
            {
                foreach (var line in data1)
                {
                    Match match = Regex.Match(line, @"^(.*?),\s*\[.*?\],\s*\[(.*?)\]");

                    if (match.Success)
                    {
                        string tag = match.Groups[1].Value.Trim();
                        string range = match.Groups[2].Value.Trim();

                        tagAndRangeList.Add($"{tag}, [{range}]");
                    }
                    else if (line.Contains("Block_Name :"))
                    {
                        tagAndRangeList.Add(line);
                    }
                }

                allTagAndRangeLists.Add(tagAndRangeList);
                tagAndRangeList = new List<string>();
            }
            //extracIQ(data);
            int count = 0;
            do
            {
                count++;
                List<List<List<string>>> Discription = Discriptions;//List<List<string>> Discription = Discriptions;//opValue(ProjectServiceProvider.returnList(), Data);//ProjectServiceProvider.returnList();

                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i].Contains(",Int") || data[i].Contains(",Real") || data[i].Contains(",Struct") || data[i].Contains(",Time"))
                    {

                    }
                    else if (!data[i].Contains(",Bool"))
                        data[i] = data[i] + ",Bool";
                }
                for (int i = 0; i < AllData.Count; i++)
                {
                    for (int j = 0; j < AllData[i].Count; j++)
                    {
                        if (AllData[i][j].Contains(",Int") || AllData[i][j].Contains(",Real") || AllData[i][j].Contains(",Struct") || AllData[i][j].Contains(",Time"))
                        {

                        }
                        else if (!AllData[i][j].Contains(",Bool"))
                            AllData[i][j] = AllData[i][j] + ",Bool";
                    }
                }
                try
                {
                    if (count != 1)
                    {
                        if (dataOkNok.Count == 0)
                            dataOkNok.Add(new List<string>());
                        //MessageBox.Show("If Match");
                        ShowSelectionPopup();
                        //MessageBox.Show("1");
                        List<List<string>> filteredAllData = AllData
                                .Where(sublist =>
                                                sublist.Count > 0 &&
                                                sublist[0].Split(',').Length > 1 &&
                                                selectedData.Contains(sublist[0].Split(',')[1].Trim())
                                      ).ToList();

                        var filteredFinalTTST = finalTTST
                                                .Where(sublist =>
                                                sublist.Count > 0 &&
                                                selectedData.Contains(sublist[0]?.ToString().Trim())
                                                ).ToList();

                        var filteredDiscription = Discription
                                                    .Where(innerList =>
                                                    innerList.Count > 0 &&
                                                    selectedData.Contains(innerList[0][0].Trim())
                                                    ).ToList();

                        List<List<string>> filteredAllTagAndRangeLists = allTagAndRangeLists
                                                                        .Where(sublist =>
                                                                            sublist.Count > 0 &&
                                                                            sublist[0].Split(',').Length > 1 &&
                                                                            selectedData.Contains(sublist[0].Split(',')[1].Trim())
                                                                        ).ToList();

                        //MessageBox.Show("2");
                        var result = MessageBox.Show(
                            "Do you want to continue with further testing?",
                            "Confirmation",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question
                        );

                        if (result == DialogResult.Yes)
                        {
                            //MessageBox.Show("3");
                            options = selectionOption(filteredAllData, InoutputNames, filteredFinalTTST, previousSelections, filteredDiscription, filteredAllTagAndRangeLists);
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                }

                if (options.Any(inner => inner.Contains("ClosedByUser")))
                {
                    return;
                }

                for (int i = 0; i < options.Count; i++)
                {
                    for (int j = 0; j < options[i].Count; j++)
                    {
                        string entry = options[i][j];

                        if (entry.Contains(",Bool"))
                        {
                            entry = entry.Replace(",Time", "").Trim();
                            entry = entry.Replace(",Struct", "").Trim();
                            entry = entry.Replace(",Bool", "").Trim();
                        }
                        else if (entry.Contains(",Struct"))
                        {
                            entry = entry.Replace(",Struct", "").Trim();
                        }
                        else if (entry.Contains(",Time"))
                        {
                            entry = entry.Replace(",Time", "").Trim();
                        }

                        entry = entry.Replace("[", "\"[").Trim();
                        entry = entry.Replace("]", "]\"").Trim();

                        entry = Regex.Replace(entry, @"\""\[\""(.*?)\""\]\""", "\"[$1]\"");
                        entry = Regex.Replace(entry, "[\"\\\\]+", "\"");
                        entry = entry.TrimEnd('/').Trim();

                        string pattern = @"^(.*?,\""\[TRUE\]\"",\"")(\[+)(.*?)(\]+)\""$";
                        Match match = Regex.Match(entry, pattern);
                        if (match.Success)
                        {
                            string prefix = match.Groups[1].Value;
                            string inner = match.Groups[3].Value;
                            entry = $"{prefix}[{inner}]\"";
                        }

                        options[i][j] = entry;
                    }
                }

                //options.Insert(0, "Block Name :," + "" + ",StateTrace");
                //for (int i = 0; i < options.Count; i++)
                //{
                //    string line = options[i];
                //    line = Regex.Replace(line, @"\""\[\""(.*?)\""\]\""", "\"[$1]\"");
                //    line = Regex.Replace(line, "[\"\\\\]+", "\"");
                //    line = line.TrimEnd('/').Trim();
                //    options[i] = line;
                //    string input = line;
                //    string pattern = @"^(.*?,\""\[TRUE\]\"",\"")(\[+)(.*?)(\]+)\""$";
                //    Match match = Regex.Match(input, pattern);
                //    if (match.Success)
                //    {
                //        string prefix = match.Groups[1].Value;
                //        string bracketsOpen = match.Groups[2].Value;
                //        string inner = match.Groups[3].Value;
                //        string bracketsClose = match.Groups[4].Value;
                //        if (bracketsOpen.Length > 1 && bracketsClose.Length > 1)
                //        {
                //            string result = $"{prefix}[{inner}]\"";
                //            options[i] = result;
                //        }
                //        else
                //        {
                //        }
                //    }
                //}

                //System.IO.File.WriteAllLines(@"C:\Test Suits\Demo.csv", options);
                var lines = new List<string>();

                int listIndex = 1;
                foreach (var sublist in options)
                {
                    lines.AddRange(sublist.Select(item => item));
                    lines.Add("");
                    listIndex++;
                }

                System.IO.File.WriteAllLines(@"C:\Generate Block\V20 All Files\Testing\Demo.csv", lines);

                //count++;
                if (count == 1)
                {
                }
                else
                {
                    count++;
                    //ReadExcelDataV20UI("C:\\Users\\Wpuser\\Downloads\\Final\\Generate Block\\Testing\\TTST\\Updated TTST", @"C:\Test Suits\Demo.csv");
                    ReadExcelDataV20UI("C:\\Generate Block\\V20 All Files\\Testing\\TTST\\Updated TTST", @"C:\Generate Block\V20 All Files\Testing\Demo.csv");
                }
                if (selectedOption == null)
                {
                    test = false;
                }
                else
                {
                    //string[] pathInput = Directory.GetFiles(@"C:\Users\Wpuser\Downloads\Final\Generate Block\Testing\TTST\Updated TTST", "*.TTST");
                    string[] pathInput = Directory.GetFiles(@"C:\Generate Block\V20 All Files\Testing\TTST\Updated TTST", "*.TTST");
                    foreach (var t in pathInput)
                    {
                        if (count == 1 && Path.GetFileNameWithoutExtension(t) == "")
                        {
                            //System.IO.File.Copy(t, @"C:\Test Suits\Input Tat\ModifiedTat\" + "" + ".TTST", overwrite: true);
                        }
                    }
                    //string[] path = Directory.GetFiles(@"C:\Users\Wpuser\Downloads\Final\Generate Block\Testing\TTST\Updated TTST", "*.TTST");
                    //string[] path = Directory.GetFiles(@"C:\Generate Block\V20 All Files\Testing\TTST\Updated TTST", "*.TTST");
                    string updatettstFolder = @"C:\Generate Block\V20 All Files\Testing\TTST\Updated TTST";

                    if (!Directory.Exists(updatettstFolder))
                    {
                        Directory.CreateDirectory(updatettstFolder);
                    }

                    string[] path = Directory.GetFiles(updatettstFolder, "*.TTST");

                    //foreach (var t in path)
                    //{
                    //    fileInfos1 = new List<FileInfo>();
                    //    if (Path.GetFileNameWithoutExtension(t) == "PR_GantryUnload_Seqv25_57af70")
                    //        fileInfos1.Add(new FileInfo(t));
                    //}
                    fileInfos1 = new List<FileInfo>();

                    string[] selectedXmlFiles = Directory.GetFiles(@"C:\Generate Block\V20 All Files\Export Files", "*.xml");
                    HashSet<string> selectedXmlNames = selectedXmlFiles
                        .Select(f => Path.GetFileNameWithoutExtension(f))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    foreach (var t in path)
                    {
                        string ttstName = Path.GetFileNameWithoutExtension(t);

                        if (selectedXmlNames.Contains(ttstName))
                        {
                            fileInfos1.Add(new FileInfo(t));
                        }
                    }

                    //if (fileInfos1.Count == 0)
                    //{
                    //    foreach (var t in path)
                    //    {
                    //        if (Path.GetFileNameWithoutExtension(t) == "PR_GantryUnload_Seqv25_57af70")
                    //        {
                    //            fileInfos1.Add(new FileInfo(t));
                    //            break;
                    //        }
                    //    }
                    //}
                    try
                    {
                        ImportElementAsyncP(fileInfos1, project);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            } while (test);

            countDo = 0;
            count_Excel = 0;
            RepeatCount = 0;
            int a = 0;
            return;
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //IamDone(processInfo);
        }
    }
    static int countDo = 0;
    static int count_Excel = 0;
    static int RepeatCount = 0;

    static int countRepeat = 0;
    //string updatedTtstFolder = @"C:\Generate Block\V20 All Files\Testing\TTST\Updated TTST";
    //string[] allUpdatedTtstFiles = Directory.GetFiles(updatedTtstFolder, "*.TTST");

    //fileInfos1.Clear();

    //foreach (var file in allUpdatedTtstFiles)
    //{
    //    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file).Trim();

    //    foreach (var row in finalTTST)
    //    {
    //        if (row.Count > 0)
    //        {
    //            string blockName = row[0].ToString().Trim();

    //            if (string.Equals(fileNameWithoutExt, blockName, StringComparison.OrdinalIgnoreCase))
    //            {
    //                fileInfos1.Add(new FileInfo(file));
    //                break;
    //            }
    //        }
    //    }
    //}

    //public static void ImportElementAsyncP(IList<FileInfo> importFileInfos, Project project)
    //{
    //    try
    //    {
    //        countRepeat++;
    //        Discriptions = new List<List<List<string>>>();

    //        var projectName = "Proj.";
    //        Project project1 = project;
    //        DeviceComposition devices = project1.Devices;
    //        var Project = project1;
    //        int tatcount = 0;

    //        TestSuiteService testSuite = Project.GetService<TestSuiteService>();
    //        ApplicationTestSystemGroup atfFolder = testSuite.ApplicationTestGroup;
    //        int countAddReport = 0;

    //        if (importFileInfos.Count > 0)
    //        {
    //            TestCaseExecutor applicationTestService = atfFolder.GetService<TestCaseExecutor>();
    //            TestCaseComposition listTestCases = atfFolder.TestCases;

    //            foreach (var file in importFileInfos)
    //            {
    //                try
    //                {
    //                    string ttstData = File.ReadAllText(file.FullName);
    //                    var deviceFinalName = Project.Devices.First();
    //                    var deviceItemFinalName = deviceFinalName.DeviceItems.FirstOrDefault(item => item.Classification == DeviceItemClassifications.CPU);
    //                    string cpuName = deviceItemFinalName.Name;
    //                    string updatedData = Regex.Replace(ttstData, @"SCOPE\s*:\s*""[^""]+""", $"SCOPE : \"{cpuName}\"");
    //                    File.WriteAllText(file.FullName, updatedData);

    //                    atfFolder.LoadFromFile(file, ImportOptions.Override, TSLoadOptions.IgnoreInvalidObject);

    //                    var device = Project.Devices.First();
    //                    string debugInfo = "Available DeviceItems:\n\n";

    //                    foreach (var item in device.DeviceItems)
    //                    {
    //                        debugInfo += $"- Name: {item.Name}, Classification: {item.Classification}\n";
    //                    }

    //                    MessageBox.Show(debugInfo, "Device Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

    //                    var deviceItem = device.DeviceItems
    //                        .FirstOrDefault(item => item.Classification == DeviceItemClassifications.CPU);

    //                    if (deviceItem == null)
    //                    {
    //                        MessageBox.Show("No CPU device found in this TIA project.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //                        return;
    //                    }

    //                    string m_PlcInstanceName = "myplc";
    //                    IInstance m_PlcSimInstance;
    //                    try
    //                    {
    //                        m_PlcSimInstance = SimulationRuntimeManager.RegisterInstance(ECPUType.CPU1500_Unspecified, m_PlcInstanceName);
    //                        m_PlcSimInstance.PowerOn(60000);
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                    }

    //                    DownloadProvider downloadProvider = deviceItem.GetService<DownloadProvider>();
    //                    var onlineProvider1 = deviceItem.GetService<OnlineProvider>();
    //                    var configuration1 = onlineProvider1.Configuration;
    //                    if (!configuration1.EnableLegacyCommunication)
    //                        configuration1.EnableLegacyCommunication = true;

    //                    if (downloadProvider != null)
    //                    {
    //                        ConnectionConfiguration configuration = downloadProvider.Configuration;
    //                        ConfigurationMode configurationMode = configuration.Modes.Find("PN/IE");

    //                        ConfigurationPcInterface pcInterface = configurationMode.PcInterfaces.Find("PLCSIM", 1);
    //                        foreach (var pcInterface1 in configurationMode.PcInterfaces)
    //                        {
    //                            Console.WriteLine($"Name: {pcInterface1.Name}, Slot: {pcInterface1.Number}");
    //                        }

    //                        IConfiguration targetConfiguration = pcInterface.TargetInterfaces[0];
    //                        ConfigurationTargetInterface targetConfigurationIF = targetConfiguration as ConfigurationTargetInterface;
    //                        ConfigurationAddressComposition downloadAddresses = targetConfigurationIF.Addresses;
    //                        ConfigurationAddress onlineAddress = downloadAddresses.Create("192.168.0.1");

    //                        DownloadConfigurationDelegate preDownloadDelegate = PreConfigureDownload;
    //                        DownloadConfigurationDelegate postDownloadDelegate = PostConfigureDownload;

    //                        DownloadOptions loadOption = DownloadOptions.Hardware | DownloadOptions.Software;
    //                        try
    //                        {
    //                            DownloadResult result = downloadProvider.Download(targetConfiguration, preDownloadDelegate, postDownloadDelegate, loadOption);
    //                        }
    //                        catch (Exception ex)
    //                        {
    //                        }
    //                    }
    //                }
    //                catch (Exception ex)
    //                {
    //                    Console.WriteLine($"Error during processing TTST '{file.Name}': {ex.Message}");
    //                }

    //                var deviceFinal = Project.Devices.First();
    //                var deviceItemFinal = deviceFinal.DeviceItems.FirstOrDefault(item => item.Classification == DeviceItemClassifications.CPU);
    //                var softwareContainer = deviceItemFinal?.GetService<SoftwareContainer>();
    //                var twoHandMonitor = softwareContainer?.Software as PlcSoftware;

    //                List<string> allDescriptions = new List<string>();
    //                string lastResultPath = "";
    //                string lastTestSetName = "";

    //                Console.WriteLine("Total TTST files: " + importFileInfos.Count);

    //                string testSetName = Path.GetFileNameWithoutExtension(file.FullName);
    //                Console.WriteLine("Processing TTST: " + testSetName);

    //                ApplicationTestSet verifyMachineFlow = testSuite.ApplicationTestGroup.ApplicationTestSets.Find(testSetName);

    //                if (verifyMachineFlow == null)
    //                {
    //                    Console.WriteLine($"Test Set '{testSetName}' not found in ApplicationTestSets.");
    //                    continue;
    //                }

    //                Console.WriteLine("Running test for: " + testSetName);
    //                TestResults testSetResults = applicationTestService.Run(verifyMachineFlow);

    //                List<string> singleTestDescriptions = new List<string>();
    //                WriteTestResults(testSetResults, singleTestDescriptions);

    //                foreach (var line in singleTestDescriptions)
    //                {
    //                    allDescriptions.Add($"{testSetName}, {line}");
    //                }

    //                lastResultPath = WriteTestResultss(testSetResults);
    //                lastTestSetName = verifyMachineFlow.Name;

    //                Console.WriteLine("All TTSTs completed. Writing combined CSV...");

    //                //string finalCsvPath = @"C:\Generate Block\V20 All Files\Testing\Final_TTST_Log.csv";
    //                string finalCsvPath = @"C:\Generate Block\V20 All Files\Testing\Final_TTST_Log.csv";

    //                string finalCsvFolder = Path.GetDirectoryName(finalCsvPath);
    //                if (!Directory.Exists(finalCsvFolder))
    //                {
    //                    Directory.CreateDirectory(finalCsvFolder);
    //                }
    //                File.WriteAllLines(finalCsvPath, new[] { "TestSetName,LogEntry" }.Concat(allDescriptions));
    //                Console.WriteLine("CSV written to: " + finalCsvPath);

    //                string originalResultCsv = finalCsvPath;
    //                List<string> originalData = new List<string>();
    //                if (File.Exists(originalResultCsv))
    //                    originalData = new List<string>(File.ReadAllLines(originalResultCsv));

    //                List<string> tempData = new List<string>();
    //                List<string> mergedData = new List<string>();

    //                foreach (var desc in allDescriptions)
    //                {
    //                    tempData.Add(desc);
    //                }

    //                bool hasHeader = originalData.Count > 0 && originalData[0].StartsWith("TestSetName");
    //                if (hasHeader)
    //                {
    //                    mergedData.Add(originalData[0]);
    //                    originalData.RemoveAt(0);
    //                }

    //                mergedData.AddRange(originalData);
    //                mergedData.AddRange(tempData);

    //                mergedData = mergedData.Distinct().ToList();

    //                File.WriteAllLines(originalResultCsv, mergedData);
    //                Console.WriteLine("Final result CSV updated with all TTST logs.");

    //                if (!string.IsNullOrEmpty(lastResultPath))
    //                {
    //                    Console.WriteLine("Generating final XML...");
    //                    GenerateXMLSelect(lastResultPath, allDescriptions, "", lastTestSetName, countRepeat);
    //                }
    //                else
    //                {
    //                    Console.WriteLine("No valid TTST test was executed. Skipping XML generation.");
    //                }
    //            }
    //            File.WriteAllLines(csvpath, tempCsv);
    //        }
    //    }
    //    catch (EngineeringException engineeringException)
    //    {
    //        throw;
    //    }
    //    catch (NonRecoverableException nonRecoverableException)
    //    {
    //        throw;
    //    }
    //    catch (Exception ex)
    //    {
    //        throw;
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    public static async Task ImportElementAsyncP(IList<FileInfo> importFileInfos, Project project)
    {
        try
        {
            countRepeat++;
            Discriptions = new List<List<List<string>>>();

            var projectName = "Proj.";
            Project project1 = project;
            DeviceComposition devices = project1.Devices;
            var Project = project1;
            int tatcount = 0;

            TestSuiteService testSuite = Project.GetService<TestSuiteService>();
            ApplicationTestSystemGroup atfFolder = testSuite.ApplicationTestGroup;
            int countAddReport = 0;

            if (importFileInfos.Count > 0)
            {
                TestCaseExecutor applicationTestService = atfFolder.GetService<TestCaseExecutor>();
                TestCaseComposition listTestCases = atfFolder.TestCases;

                foreach (var file in importFileInfos)
                {
                    try
                    {
                        string ttstData = File.ReadAllText(file.FullName);
                        var deviceFinalName = Project.Devices.First();
                        var deviceItemFinalName = deviceFinalName.DeviceItems.FirstOrDefault(item => item.Classification == DeviceItemClassifications.CPU);
                        string cpuName = deviceItemFinalName.Name;
                        //string updatedData = Regex.Replace(ttstData, @"SCOPE\s*:\s*\"[^\"]+\"", $"SCOPE : \"{cpuName}\"");
                        string updatedData = Regex.Replace(ttstData, @"SCOPE\s*:\s*""[^""]+""", $"SCOPE : \"{cpuName}\"");
                        File.WriteAllText(file.FullName, updatedData);

                        atfFolder.LoadFromFile(file, ImportOptions.Override, TSLoadOptions.IgnoreInvalidObject);

                        var device = Project.Devices.First();
                        string debugInfo = "Available DeviceItems:\n\n";

                        foreach (var item in device.DeviceItems)
                        {
                            debugInfo += $"- Name: {item.Name}, Classification: {item.Classification}\n";
                        }

                        MessageBox.Show(debugInfo, "Device Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        var deviceItem = device.DeviceItems.FirstOrDefault(item => item.Classification == DeviceItemClassifications.CPU);

                        if (deviceItem == null)
                        {
                            MessageBox.Show("No CPU device found in this TIA project.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        //string m_PlcInstanceName = "myplc";

                        //try
                        //{
                        //    IInstance m_PlcSimInstance = SimulationRuntimeManager.RegisterInstance(ECPUType.CPU1500_Unspecified, m_PlcInstanceName);

                        //    m_PlcSimInstance.PowerOn(60000);
                           
                        //    Console.WriteLine("PLCSIM instance powered on. Waiting 5 seconds before proceeding...");
                        //    Thread.Sleep(5000);

                        //    Console.WriteLine("Proceeding with PLCSIM assuming it's ready.");
                        //}
                        //catch (Exception ex)
                        //{
                        //    Console.WriteLine("Error initializing PLCSIM: " + ex.Message);
                        //    return;
                        //}

                        string m_PlcInstanceName = "myplc";
                        IInstance m_PlcSimInstance;
                        try
                        {
                            m_PlcSimInstance = SimulationRuntimeManager.RegisterInstance(ECPUType.CPU1500_Unspecified, m_PlcInstanceName);
                            m_PlcSimInstance.PowerOn(60000);
                        }
                        catch (Exception ex)
                        {
                        }
                        DownloadProvider downloadProvider = deviceItem.GetService<DownloadProvider>();
                        var onlineProvider1 = deviceItem.GetService<OnlineProvider>();
                        var configuration1 = onlineProvider1.Configuration;

                        if (!configuration1.EnableLegacyCommunication)
                            configuration1.EnableLegacyCommunication = true;

                        if (downloadProvider != null)
                        {
                            ConnectionConfiguration configuration = downloadProvider.Configuration;
                            ConfigurationMode configurationMode = configuration.Modes.Find("PN/IE");

                            ConfigurationPcInterface pcInterface = configurationMode.PcInterfaces.Find("PLCSIM", 1);
                            foreach (var pcInterface1 in configurationMode.PcInterfaces)
                            {
                                Console.WriteLine($"Name: {pcInterface1.Name}, Slot: {pcInterface1.Number}");
                            }

                            IConfiguration targetConfiguration = pcInterface.TargetInterfaces[0];
                            ConfigurationTargetInterface targetConfigurationIF = targetConfiguration as ConfigurationTargetInterface;
                            ConfigurationAddressComposition downloadAddresses = targetConfigurationIF.Addresses;
                            ConfigurationAddress onlineAddress = downloadAddresses.Create("192.168.0.1");

                            DownloadConfigurationDelegate preDownloadDelegate = PreConfigureDownload;
                            DownloadConfigurationDelegate postDownloadDelegate = PostConfigureDownload;

                            DownloadOptions loadOption = DownloadOptions.Hardware | DownloadOptions.Software;
                            try
                            {
                                DownloadResult result = downloadProvider.Download(targetConfiguration, preDownloadDelegate, postDownloadDelegate, loadOption);
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during processing TTST '{file.Name}': {ex.Message}");
                    }

                    var deviceFinal = Project.Devices.First();
                    var deviceItemFinal = deviceFinal.DeviceItems.FirstOrDefault(item => item.Classification == DeviceItemClassifications.CPU);
                    var softwareContainer = deviceItemFinal?.GetService<SoftwareContainer>();
                    var twoHandMonitor = softwareContainer?.Software as PlcSoftware;

                    List<string> allDescriptions = new List<string>();
                    string lastResultPath = "";
                    string lastTestSetName = "";

                    Console.WriteLine("Total TTST files: " + importFileInfos.Count);
                    string testSetName = Path.GetFileNameWithoutExtension(file.FullName);
                    Console.WriteLine("Processing TTST: " + testSetName);

                    ApplicationTestSet verifyMachineFlow = testSuite.ApplicationTestGroup.ApplicationTestSets.Find(testSetName);

                    if (verifyMachineFlow == null)
                    {
                        Console.WriteLine($"Test Set '{testSetName}' not found in ApplicationTestSets.");
                        continue;
                    }

                    foreach (var ttstName in verifyMachineFlow.Name)
                    {
                        if (ttstName.ToString() == " " && !ttstName.ToString().StartsWith("FB_"))
                        {
                            string selectTTSTFile = ttstName.ToString();    
                        }
                    }
                    TestResults testSetResults = applicationTestService.Run(verifyMachineFlow);                    

                    List<string> singleTestDescriptions = new List<string>();
                    WriteTestResults(testSetResults, singleTestDescriptions);

                    foreach (var line in singleTestDescriptions)
                    {
                        allDescriptions.Add($"{testSetName}, {line}");
                    }

                    lastResultPath = WriteTestResultss(testSetResults);
                    lastTestSetName = verifyMachineFlow.Name;

                    Console.WriteLine("All TTSTs completed. Writing combined CSV...");

                    string finalCsvPath = @"C:\\Generate Block\\V20 All Files\\Testing\\Final_TTST_Log.csv";
                    string finalCsvFolder = Path.GetDirectoryName(finalCsvPath);
                    if (!Directory.Exists(finalCsvFolder))
                    {
                        Directory.CreateDirectory(finalCsvFolder);
                    }
                    File.WriteAllLines(finalCsvPath, new[] { "TestSetName,LogEntry" }.Concat(allDescriptions));
                    Console.WriteLine("CSV written to: " + finalCsvPath);

                    string originalResultCsv = finalCsvPath;
                    List<string> originalData = new List<string>();
                    if (File.Exists(originalResultCsv))
                        originalData = new List<string>(File.ReadAllLines(originalResultCsv));

                    List<string> tempData = new List<string>();
                    List<string> mergedData = new List<string>();

                    foreach (var desc in allDescriptions)
                    {
                        tempData.Add(desc);
                    }

                    bool hasHeader = originalData.Count > 0 && originalData[0].StartsWith("TestSetName");
                    if (hasHeader)
                    {
                        mergedData.Add(originalData[0]);
                        originalData.RemoveAt(0);
                    }

                    mergedData.AddRange(originalData);
                    mergedData.AddRange(tempData);

                    mergedData = mergedData.Distinct().ToList();

                    File.WriteAllLines(originalResultCsv, mergedData);
                    Console.WriteLine("Final result CSV updated with all TTST logs.");

                    if (!string.IsNullOrEmpty(lastResultPath))
                    {
                        Console.WriteLine("Generating final XML...");
                        GenerateXMLSelect(lastResultPath, allDescriptions, "", lastTestSetName, countRepeat);
                    }
                    else
                    {
                        Console.WriteLine("No valid TTST test was executed. Skipping XML generation.");
                    }
                }
                File.WriteAllLines(csvpath, tempCsv);
            }
        }
        catch (EngineeringException engineeringException)
        {
            throw;
        }
        catch (NonRecoverableException nonRecoverableException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw;
        }
        catch
        {
            throw;
        }
    }

    static List<string> selectedData = new List<string>();
    public static void ShowSelectionPopup()
    {
        try
        {
            Form popup = new Form
            {
                Text = "Select 0th Index Items",
                Width = 600,
                Height = 600,
                StartPosition = FormStartPosition.CenterScreen
            };

            FlowLayoutPanel panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(10)
            };

            List<System.Windows.Forms.CheckBox> checkboxes = new List<System.Windows.Forms.CheckBox>();

            for (int i = 0; i < dataOkNok.Count; i++)
            {
                var sublist = dataOkNok[i];

                panel.Controls.Add(new Label
                {
                    Text = $"Block {i + 1}:",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    AutoSize = true,
                    Margin = new Padding(0, 10, 0, 0)
                });

                System.Windows.Forms.CheckBox chk = new System.Windows.Forms.CheckBox
                {
                    Text = sublist[0],
                    Width = 550,
                    Tag = sublist[0]
                };
                checkboxes.Add(chk);
                panel.Controls.Add(chk);

                for (int j = 1; j < sublist.Count; j++)
                {
                    panel.Controls.Add(new Label
                    {
                        Text = "  " + sublist[j],
                        AutoSize = true,
                        ForeColor = Color.Gray,
                        Padding = new Padding(10, 0, 0, 0)
                    });
                }
            }
            
            Button btnConfirm = new Button
            {
                Text = "Confirm Selection",
                Width = 200,
                Height = 40,
                BackColor = Color.LightGreen,
                Margin = new Padding(10)
            };

            btnConfirm.Click += (s, e) =>
            {
                selectedData.Clear();

                foreach (var chk in checkboxes)
                {
                    if (chk.Checked)
                        selectedData.Add(chk.Tag.ToString());
                }

                string result = "You selected:\n\n" + string.Join("\n", selectedData);
                MessageBox.Show(result, "Selected 0th Index Items");
                popup.Close();
            };

            panel.Controls.Add(btnConfirm);
            popup.Controls.Add(panel);
            popup.ShowDialog();
        }
        catch(Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
    //public static void ImportElementAsyncP(IList<FileInfo> importFileInfos, Project project)
    //{
    //    try
    //    {
    //        countRepeat++;

    //        var projectName = "Proj.";
    //        Project project1 = project;
    //        DeviceComposition devices = project1.Devices;
    //        var Project = project1;
    //        int tatcount = 0;

    //        TestSuiteService testSuite = Project.GetService<TestSuiteService>();
    //        ApplicationTestSystemGroup atfFolder = testSuite.ApplicationTestGroup;
    //        int countAddReport = 0;

    //        if (importFileInfos.Count > 0)
    //        {
    //            TestCaseExecutor applicationTestService = atfFolder.GetService<TestCaseExecutor>();
    //            TestCaseComposition listTestCases = atfFolder.TestCases;

    //            foreach (var file in importFileInfos)
    //            {
    //                try
    //                {
    //                    atfFolder.LoadFromFile(file, ImportOptions.Override, TSLoadOptions.IgnoreInvalidObject);
    //                    var device = Project.Devices.First();
    //                    string debugInfo = "Available DeviceItems:\n\n";

    //                    foreach (var item in device.DeviceItems)
    //                    {
    //                        debugInfo += $"- Name: {item.Name}, Classification: {item.Classification}\n";
    //                    }

    //                    MessageBox.Show(debugInfo, "Device Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

    //                    var deviceItem = device.DeviceItems
    //                        .FirstOrDefault(item => item.Classification == DeviceItemClassifications.CPU);

    //                    if (deviceItem == null)
    //                    {
    //                        MessageBox.Show("No CPU device found in this TIA project.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //                        return;
    //                    }
    //                    string m_PlcInstanceName = "myplc";
    //                    IInstance m_PlcSimInstance;
    //                    try
    //                    {
    //                        m_PlcSimInstance = SimulationRuntimeManager.RegisterInstance(ECPUType.CPU1500_Unspecified, m_PlcInstanceName);
    //                        m_PlcSimInstance.PowerOn(60000);
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                    }

    //                    DownloadProvider downloadProvider = deviceItem.GetService<DownloadProvider>();
    //                    var onlineProvider1 = deviceItem.GetService<OnlineProvider>();
    //                    var configuration1 = onlineProvider1.Configuration;
    //                    if (!configuration1.EnableLegacyCommunication)
    //                        configuration1.EnableLegacyCommunication = true;

    //                    if (downloadProvider != null)
    //                    {
    //                        ConnectionConfiguration configuration = downloadProvider.Configuration;
    //                        ConfigurationMode configurationMode = configuration.Modes.Find("PN/IE");

    //                        ConfigurationPcInterface pcInterface = configurationMode.PcInterfaces.Find("PLCSIM", 1);
    //                        foreach (var pcInterface1 in configurationMode.PcInterfaces)
    //                        {
    //                            Console.WriteLine($"Name: {pcInterface1.Name}, Slot: {pcInterface1.Number}");
    //                        }

    //                        IConfiguration targetConfiguration = pcInterface.TargetInterfaces[0];
    //                        ConfigurationTargetInterface targetConfigurationIF = targetConfiguration as ConfigurationTargetInterface;
    //                        ConfigurationAddressComposition downloadAddresses = targetConfigurationIF.Addresses;
    //                        ConfigurationAddress onlineAddress = downloadAddresses.Create("192.168.0.1");

    //                        DownloadConfigurationDelegate preDownloadDelegate = PreConfigureDownload;
    //                        DownloadConfigurationDelegate postDownloadDelegate = PostConfigureDownload;

    //                        DownloadOptions loadOption = DownloadOptions.Hardware | DownloadOptions.Software;
    //                        try
    //                        {
    //                            DownloadResult result = downloadProvider.Download(targetConfiguration, preDownloadDelegate, postDownloadDelegate, loadOption);
    //                        }
    //                        catch (Exception ex)
    //                        {
    //                        }
    //                    }
    //                }
    //                catch (Exception ex)
    //                {
    //                    Console.WriteLine($"Error during processing TTST '{file.Name}': {ex.Message}");
    //                }

    //                var deviceFinal = Project.Devices.First();
    //                var deviceItemFinal = deviceFinal.DeviceItems.FirstOrDefault(item => item.Classification == DeviceItemClassifications.CPU);
    //                var softwareContainer = deviceItemFinal?.GetService<SoftwareContainer>();
    //                var twoHandMonitor = softwareContainer?.Software as PlcSoftware;

    //                List<string> allDescriptions = new List<string>();
    //                string lastResultPath = "";
    //                string lastTestSetName = "";

    //                Console.WriteLine("Total TTST files: " + importFileInfos.Count);

    //                string testSetName = Path.GetFileNameWithoutExtension(file.FullName);
    //                Console.WriteLine("Processing TTST: " + testSetName);

    //                ApplicationTestSet verifyMachineFlow = testSuite.ApplicationTestGroup.ApplicationTestSets.Find(testSetName);

    //                if (verifyMachineFlow == null)
    //                {
    //                    Console.WriteLine($"Test Set '{testSetName}' not found in ApplicationTestSets.");
    //                    continue;
    //                }

    //                Console.WriteLine("Running test for: " + testSetName);
    //                TestResults testSetResults = applicationTestService.Run(verifyMachineFlow);

    //                List<string> singleTestDescriptions = new List<string>();
    //                WriteTestResults(testSetResults, singleTestDescriptions);

    //                foreach (var line in singleTestDescriptions)
    //                {
    //                    allDescriptions.Add($"{testSetName}, {line}");
    //                }
    //                lastResultPath = WriteTestResultss(testSetResults);
    //                lastTestSetName = verifyMachineFlow.Name;

    //                Console.WriteLine("All TTSTs completed. Writing combined CSV...");

    //                string finalCsvPath = @"C:\Generate Block\V20 All Files\Testing\Final_TTST_Log.csv";
    //                File.WriteAllLines(finalCsvPath, new[] { "TestSetName,LogEntry" }.Concat(allDescriptions));

    //                Console.WriteLine("CSV written to: " + finalCsvPath);

    //                if (!string.IsNullOrEmpty(lastResultPath))
    //                {
    //                    Console.WriteLine("Generating final XML...");
    //                    GenerateXMLSelect(lastResultPath, allDescriptions, "", lastTestSetName, countRepeat);
    //                }
    //                else
    //                {
    //                    Console.WriteLine("No valid TTST test was executed. Skipping XML generation.");
    //                }
    //            }
    //        }
    //    }
    //    catch (EngineeringException engineeringException)
    //    {
    //        throw;
    //    }
    //    catch (NonRecoverableException nonRecoverableException)
    //    {
    //        throw;
    //    }
    //    catch (Exception ex)
    //    {
    //        throw;
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    //public static void ImportElementAsyncP(IList<FileInfo> importFileInfos, Project project)
    //{
    //    try
    //    {
    //        countRepeat++;

    //        //var exportWithDefaults = _settingsService.GetExportWithDefaults();
    //        //var exportReadOnly = _settingsService.GetExportReadOnly();
    //        var projectName = "Proj.";
    //        //var proj = CurrentProject;
    //        //var projpath = proj.Path;

    //        Project project1 = project;
    //        //project1 = CurrentProject;
    //        DeviceComposition devices = project1.Devices;

    //        //var tree = GetLogicalTree();
    //        var Project = project1;
    //        int tatcount = 0;
    //        TestSuiteService testSuite = Project.GetService<TestSuiteService>();
    //        ApplicationTestSystemGroup atfFolder = testSuite.ApplicationTestGroup;
    //        int countAddReport = 0;
    //        if (importFileInfos.Count > 0)
    //        {
    //            TestCaseExecutor applicationTestService = testSuite.ApplicationTestGroup.GetService<TestCaseExecutor>();
    //            TestCaseComposition listTestCases = testSuite.ApplicationTestGroup.TestCases;
    //            //IList<IEngineeringObject> testDataCollection = atfFolder.LoadFromFile(importFileInfos[0], ImportOptions.Override, TSLoadOptions.IgnoreInvalidObject);
    //            //foreach (var file in importFileInfos)
    //            //{
    //            //    atfFolder.LoadFromFile(file, ImportOptions.Override, TSLoadOptions.IgnoreInvalidObject);
    //            //}

    //            //                var device = Project.Devices.First();
    //            //                //var deviceItem = device.DeviceItems.First(e => e.Classification == DeviceItemClassifications.CPU && e.Name == "PLC");
    //            //                string debugInfo = "Available DeviceItems:\n\n";

    //            //                foreach (var item in device.DeviceItems)
    //            //                {
    //            //                    debugInfo += $"- Name: {item.Name}, Classification: {item.Classification}\n";
    //            //                }

    //            //                MessageBox.Show(debugInfo, "Device Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

    //            //                var deviceItem = device.DeviceItems
    //            //     .FirstOrDefault(item => item.Classification == DeviceItemClassifications.CPU);

    //            //                if (deviceItem == null)
    //            //                {
    //            //                    MessageBox.Show("No CPU device found in this TIA project.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //            //                    return;
    //            //                }

    //            //                string m_PlcInstanceName = /*"Encoder_testing";//*/"myplc";
    //            //                IInstance m_PlcSimInstance;
    //            //                try
    //            //                {
    //            //                    m_PlcSimInstance = SimulationRuntimeManager.RegisterInstance(ECPUType.CPU1500_Unspecified, m_PlcInstanceName);
    //            //                    m_PlcSimInstance.PowerOn(60000);
    //            //                }
    //            //                catch (Exception ex)
    //            //                {

    //            //                }
    //            //                DownloadProvider downloadProvider = deviceItem.GetService<DownloadProvider>();
    //            //                var onlineProvider1 = deviceItem.GetService<OnlineProvider>();
    //            //                var configuration1 = onlineProvider1.Configuration;
    //            //                if (!configuration1.EnableLegacyCommunication) configuration1.EnableLegacyCommunication = true;

    //            //                if (downloadProvider != null)
    //            //                {
    //            //                    ConnectionConfiguration configuration = downloadProvider.Configuration;

    //            //                    ConfigurationMode configurationMode = configuration.Modes.Find("PN/IE");

    //            //                    ConfigurationPcInterface pcInterface = configurationMode.PcInterfaces.Find("PLCSIM", 1);
    //            //                    foreach (var pcInterface1 in configurationMode.PcInterfaces)
    //            //                    {
    //            //                        Console.WriteLine($"Name: {pcInterface1.Name}, Slot: {pcInterface1.Number}");
    //            //                    }
    //            //                    IConfiguration targetConfiguration = pcInterface.TargetInterfaces[0];

    //            //                    ConfigurationTargetInterface targetConfigurationIF = targetConfiguration as ConfigurationTargetInterface;

    //            //                    ConfigurationAddressComposition downloadAddresses = targetConfigurationIF.Addresses;

    //            //                    ConfigurationAddress onlineAddress = downloadAddresses.Create("192.168.0.1");

    //            //                    DownloadConfigurationDelegate preDownloadDelegate = PreConfigureDownload;

    //            //                    DownloadConfigurationDelegate postDownloadDelegate = PostConfigureDownload;

    //            //                    DownloadOptions loadOption = DownloadOptions.Hardware | DownloadOptions.Software;
    //            //                    try
    //            //                    {
    //            //                        DownloadResult result = downloadProvider.Download(targetConfiguration, preDownloadDelegate, postDownloadDelegate, loadOption);
    //            //                    }
    //            //                    catch (Exception ex)
    //            //                    {

    //            //                    }
    //            //                }
    //            foreach (var file in importFileInfos)
    //            {
    //                try
    //                {
    //                    atfFolder.LoadFromFile(file, ImportOptions.Override, TSLoadOptions.IgnoreInvalidObject);

    //                    var device = Project.Devices.First();
    //                    string debugInfo = "Available DeviceItems:\n\n";

    //                    foreach (var item in device.DeviceItems)
    //                    {
    //                        debugInfo += $"- Name: {item.Name}, Classification: {item.Classification}\n";
    //                    }

    //                    MessageBox.Show(debugInfo, "Device Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

    //                    var deviceItem = device.DeviceItems
    //                        .FirstOrDefault(item => item.Classification == DeviceItemClassifications.CPU);

    //                    if (deviceItem == null)
    //                    {
    //                        MessageBox.Show("No CPU device found in this TIA project.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //                        return;
    //                    }

    //                    string m_PlcInstanceName = "myplc";
    //                    IInstance m_PlcSimInstance;
    //                    try
    //                    {
    //                        m_PlcSimInstance = SimulationRuntimeManager.RegisterInstance(ECPUType.CPU1500_Unspecified, m_PlcInstanceName);
    //                        m_PlcSimInstance.PowerOn(60000);
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                    }

    //                    DownloadProvider downloadProvider = deviceItem.GetService<DownloadProvider>();
    //                    var onlineProvider1 = deviceItem.GetService<OnlineProvider>();
    //                    var configuration1 = onlineProvider1.Configuration;
    //                    if (!configuration1.EnableLegacyCommunication) configuration1.EnableLegacyCommunication = true;

    //                    if (downloadProvider != null)
    //                    {
    //                        ConnectionConfiguration configuration = downloadProvider.Configuration;
    //                        ConfigurationMode configurationMode = configuration.Modes.Find("PN/IE");

    //                        ConfigurationPcInterface pcInterface = configurationMode.PcInterfaces.Find("PLCSIM", 1);
    //                        foreach (var pcInterface1 in configurationMode.PcInterfaces)
    //                        {
    //                            Console.WriteLine($"Name: {pcInterface1.Name}, Slot: {pcInterface1.Number}");
    //                        }

    //                        IConfiguration targetConfiguration = pcInterface.TargetInterfaces[0];
    //                        ConfigurationTargetInterface targetConfigurationIF = targetConfiguration as ConfigurationTargetInterface;
    //                        ConfigurationAddressComposition downloadAddresses = targetConfigurationIF.Addresses;
    //                        ConfigurationAddress onlineAddress = downloadAddresses.Create("192.168.0.1");

    //                        DownloadConfigurationDelegate preDownloadDelegate = PreConfigureDownload;
    //                        DownloadConfigurationDelegate postDownloadDelegate = PostConfigureDownload;

    //                        DownloadOptions loadOption = DownloadOptions.Hardware | DownloadOptions.Software;
    //                        try
    //                        {
    //                            DownloadResult result = downloadProvider.Download(targetConfiguration, preDownloadDelegate, postDownloadDelegate, loadOption);
    //                        }
    //                        catch (Exception ex)
    //                        {
    //                        }
    //                    }

    //                }
    //                catch (Exception ex)
    //                {
    //                    Console.WriteLine($"Error during processing TTST '{file.Name}': {ex.Message}");
    //                }
    //            }

    //            var twoHandMonitor = deviceItem.GetService<SoftwareContainer>().Software as PlcSoftware;
    //            List<string> allDescriptions = new List<string>();
    //            string lastResultPath = "";
    //            string lastTestSetName = "";

    //            Console.WriteLine("Total TTST files: " + importFileInfos.Count);

    //            foreach (var fileInfo in importFileInfos)
    //            {
    //                string testSetName = Path.GetFileNameWithoutExtension(fileInfo.FullName);
    //                Console.WriteLine("Processing TTST: " + testSetName);

    //                ApplicationTestSet verifyMachineFlow = testSuite.ApplicationTestGroup.ApplicationTestSets.Find(testSetName);

    //                if (verifyMachineFlow == null)
    //                {
    //                    Console.WriteLine($"Test Set '{testSetName}' not found in ApplicationTestSets.");
    //                    continue;
    //                }

    //                if (verifyMachineFlow == null)
    //                {
    //                    Console.WriteLine($"Test Set '{testSetName}' not found in ApplicationTestSets.");
    //                    continue;
    //                }

    //                Console.WriteLine("Running test for: " + testSetName);
    //                TestResults testSetResults = applicationTestService.Run(verifyMachineFlow);

    //                List<string> singleTestDescriptions = new List<string>();
    //                WriteTestResults(testSetResults, singleTestDescriptions);

    //                foreach (var line in singleTestDescriptions)
    //                {
    //                    allDescriptions.Add($"{testSetName}, {line}");
    //                }

    //                lastResultPath = WriteTestResultss(testSetResults);
    //                lastTestSetName = verifyMachineFlow.Name;
    //            }

    //            Console.WriteLine("All TTSTs completed. Writing combined CSV...");

    //            string finalCsvPath = @"C:\Generate Block\V20 All Files\Testing\Final_TTST_Log.csv";
    //            File.WriteAllLines(finalCsvPath, new[] { "TestSetName,LogEntry" }.Concat(allDescriptions));

    //            Console.WriteLine("CSV written to: " + finalCsvPath);

    //            if (!string.IsNullOrEmpty(lastResultPath))
    //            {
    //                Console.WriteLine("Generating final XML...");
    //                GenerateXMLSelect(lastResultPath, allDescriptions, "", lastTestSetName, countRepeat);
    //            }
    //            else
    //            {
    //                Console.WriteLine("No valid TTST test was executed. Skipping XML generation.");
    //            }
    //        }
    //    }

    //    //string[] tatFile = Directory.GetFiles(@"C:\Test Suits\Input Tat\ModifiedTat", "*.tat");

    //    catch (EngineeringException engineeringException)
    //    {
    //        throw;
    //        //throw OpennessApiExceptionHandler.CatchedEngineeringException(engineeringException);
    //    }
    //    catch (NonRecoverableException nonRecoverableException)
    //    {
    //        throw;
    //        //throw OpennessApiExceptionHandler.CatchedNonRecoverableException(nonRecoverableException);
    //    }
    //    catch (Exception ex)
    //    {
    //        throw;
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}
    //    var twoHandMonitor = deviceItem.GetService<SoftwareContainer>().Software as PlcSoftware;
    //    string TestSetName = System.IO.Path.GetFileNameWithoutExtension(importFileInfos[0].FullName);
    //    FileInfo info = new FileInfo(TestSetName);
    //    ApplicationTestSet verifyMachineFlow = testSuite.ApplicationTestGroup.ApplicationTestSets.Find(TestSetName);

    //    TestResults TestSetResults1 = applicationTestService.Run(verifyMachineFlow);

    //    List<string> Discription = new List<string>();
    //    WriteTestResults(TestSetResults1, Discription);
    //    string path = WriteTestResultss(TestSetResults1);
    //    GenerateXMLSelect(path, Discription, "", verifyMachineFlow.Name, countRepeat);

    //    string tatpath = importFileInfos[0].FullName;
    //}
    //static List<string> dataOkNok = new List<string>();
    private static void GenerateXMLSelect(string path, List<string> Disc, string path1, string name, int countRepeat)
    {
        try
        {
            //MessageBox.Show("Report Generation Start");
            //name = name.Split('[')[0];
            //System.IO.File.Copy(path1, @"C:\Test Suits\Report Gen\AutoGenXml\" + System.IO.Path.GetFileName(path1), true);
            List<string> Compair = new List<string>();
            //string folderPathCsv = @"C:\Test Suits\Report Gen\AutoGenXml";
            string folderPathCsv = @"C:\Generate Block\V20 All Files\Test Suits\Report Gen\AutoGenXml";
            //string filePath = @"";
            //string[] csvFiles = Directory.GetFiles(folderPathCsv, "*.csv");
            //List<string> tmv = new List<string>();
            //if (csvFiles.Length > 0)
            //{
            //    filePath = csvFiles[0];
            //}

            //List<string[]> records = new List<string[]>();
            List<string> BlockName = new List<string>();
            BlockName.Add(name);

            List<string> TextID = new List<string>();
            //string FIlePath = @"C:\Generate Block\V20 All Files\Downloads\Generated Block\XML Files";
            string FIlePath = @"C:\Generate Block\V20 All Files\Export Files";
            if (!Directory.Exists(FIlePath))
            {
                Directory.CreateDirectory(FIlePath);
            }
            //string FIlePath = @"C:\Users\Wpuser\Downloads\Final\Generate Block\XML Files";
            string[] xmlFiles = Directory.GetFiles(FIlePath, "*.xml");
            List<string> xmlCont = new List<string>();
            List<List<string>> Lbc = new List<List<string>>();

            foreach (var msd in BlockName)
            {
                foreach (var arr in xmlFiles)
                {
                    var nam = System.IO.Path.GetFileNameWithoutExtension(arr);
                    if (nam.Trim() == name.Trim())
                    {
                        string fileContent = File.ReadAllText(arr);
                        string textPattern = @"<Text>(.*?)<\/Text>";
                        string pattern1 = @"<SW.Blocks.CompileUnit.*?</SW.Blocks.CompileUnit>";

                        Regex regexSW = new Regex(pattern1, RegexOptions.Singleline);

                        MatchCollection matchesSW = regexSW.Matches(fileContent);

                        foreach (Match match in matchesSW)
                        {
                            string section = match.Value;
                            string patternWires = @"<Wires>.*?<\/Wires>";
                            Regex regexWire = new Regex(patternWires, RegexOptions.Singleline);
                            MatchCollection matchesWire = regexSW.Matches(section);
                            List<MatchCollection> storedMatchesWire = new List<MatchCollection>();
                            storedMatchesWire.Add(matchesWire);
                            string patternParts = @"<Parts>.*?<\/Parts>";

                            Regex regexParts = new Regex(patternParts, RegexOptions.Singleline);

                            MatchCollection matchesParts = regexParts.Matches(section);
                            MatchCollection matchesPart = regexSW.Matches(section);
                            List<MatchCollection> storedMatchesPart = new List<MatchCollection>();
                            storedMatchesPart.Add(matchesPart);

                            foreach (Match matchP in matchesParts)
                            {
                                string partsContent = matchP.Value;
                                Console.WriteLine("Matched <Parts> content:");
                                string patternMovePart = @"<Part\s+Name=""Move""\s+UId=""(\d+)""\s+DisabledENO=""([^""]+)""[^>]*?>.*?<\/Part>";

                                Regex regexMovePart = new Regex(patternMovePart, RegexOptions.Singleline);
                                MatchCollection matchesMovePart = regexMovePart.Matches(partsContent);
                                foreach (Match matchMovePart in matchesMovePart)
                                {
                                    string movePart = matchMovePart.Value;
                                    string uId = matchMovePart.Groups[1].Value;
                                    string disabledENO = matchMovePart.Groups[2].Value;
                                    if (uId != null)
                                    {
                                        foreach (Match matchW in matchesWire)
                                        {
                                            string wireData = matchW.Value;
                                            string patternLiteralConstant = @"<Access Scope=""LiteralConstant""\s+UId=""(\d+)""[^>]*>.*?<ConstantType>([^<]+)<\/ConstantType>.*?<ConstantValue>([^<]+)<\/ConstantValue>.*?<\/Access>";
                                            Regex regexLiteralConstant = new Regex(patternLiteralConstant, RegexOptions.Singleline);

                                            MatchCollection matchesLiteralConstant = regexLiteralConstant.Matches(wireData);

                                            foreach (Match matchLiteralConstant in matchesLiteralConstant)
                                            {
                                                string literalConstant = matchLiteralConstant.Value;
                                                string constantUId = matchLiteralConstant.Groups[1].Value;
                                                string constantType = matchLiteralConstant.Groups[2].Value;
                                                string constantValue = matchLiteralConstant.Groups[3].Value;
                                                string textPatt = @"<Text>(.*?)<\/Text>";
                                                Regex textRege = new Regex(textPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

                                                MatchCollection textMatche = textRege.Matches(section);

                                                foreach (Match textMatch in textMatche)
                                                {
                                                    string textCont = textMatch.Groups[1].Value;
                                                    string result = textCont.Replace(",", "");
                                                    TextID.Add(constantValue + "," + result + "," + System.IO.Path.GetFileNameWithoutExtension(arr));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            string folderPath = path;

            List<string> lines = new List<string>();

            string data = "";
            data = File.ReadAllText(folderPath);

            string input = data;

            List<string> TagData = new List<string>();
            var regex = new Regex(@"Actual:\s(\d+)", RegexOptions.Singleline);

            foreach (Match match in regex.Matches(input))
            {
                string seq = match.Groups[1].Value;
                string actual = match.Groups[2].Value;
                TagData.Add(seq);//("[" + seq + "]" + "Actual: " + actual);
            }

            List<string> op = new List<string>();
            //if (File.Exists(@"C:\Test Suits\Input File\New Text Document.txt"))
            //    File.Delete(@"C:\Test Suits\Input File\New Text Document.txt");
            //string TxtPath = @"C:\Test Suits\Input File\New Text Document.txt";
            if (File.Exists(@"C:\Generate Block\V20 All Files\Test Suits\Input File\New Text Document.txt"))
                File.Delete(@"C:\Generate Block\V20 All Files\Test Suits\Input File\New Text Document.txt");
            string TxtPath = @"C:\Generate Block\V20 All Files\Test Suits\Input File\New Text Document.txt";
            using (FileStream fs = File.Create(TxtPath))
            {
            }
            File.WriteAllLines(TxtPath, Disc);
            string hpInput = File.ReadAllText(TxtPath);
            //string pattern5 = @"Path:\s*""[^""]*Transition\s*(\d+)""[\s\S]*?Description:\s*Actual:\s*(\S+),\s*Expected:\s*(\S+)(?:,\s*Assert type:\s*(\w+))?";
            string pattern5 = @"(?:(\w+(?:\.\w+)*)(?:\[\d+\])?:\s*Actual:\s*(\S+),\s*E[xX][\s‐\-]*p[\s‐\-]*e[\s‐\-]*c[\s‐\-]*t[\s‐\-]*e[\s‐\-]*d:\s*(\S+)(?:,\s*Assert type:\s*(\w+))?)|(?:\b(MemoryReset):\s*(.+))";
            List<string> outputCSV = new List<string>();
    
            var match5 = Regex.Matches(hpInput, pattern5);
            int flow = 0;
            int lastIndex_i = -1;
            int valcount_i = 0;
            string ap = "";
            try
            {
                foreach (Match mc in match5)
                {
                    string fullMatch = mc.Value;
                    if (fullMatch.Contains("MemoryReset"))
                    {
                        flow++;
                        outputCSV.Add("Output tag Flow : " + flow);
                        op.Add("State traces Flow : " + flow);
                    }
                    string inpat = @"Actual:\s*\S+";
            
                    if (Regex.IsMatch(fullMatch, inpat))
                    {
                        if (fullMatch.Contains("[1]"))
                        {
                            valcount_i++;
                        }
                    }
                    if (fullMatch.Contains("Transition"))
                    {
                        ap = fullMatch.Split(',')[0];
                    }
                    else if (valcount_i != null)
                    {
                        if (fullMatch.Contains("Actual: False") || fullMatch.Contains("Actual: FALSE") || fullMatch.Contains("Actual: True") || fullMatch.Contains("Actual: TRUE") || !fullMatch.Contains("seq["))
                        {
                            if (!fullMatch.Contains("MemoryReset"))
                            {
                                int c = 0;

                                string pattern1 = @"Actual:\s*(\S+)";

                                Match matchese = Regex.Match(fullMatch, pattern1);
                                string tagname = fullMatch.Split('.')[1];
                                int bolc = c + 1;
                                string mismatch = matchese.Groups[1].Value;
                                if (!tagname.Contains("StateNo") && op.Count() > 0)
                                {
                                    if (!tagname.Contains(":"))
                                    {
                                        string lastName = op[op.Count - 1];
                                        outputCSV.Add(name + "," + fullMatch.Split('.')[0] + "." + fullMatch.Split('.')[1].Split(':')[0] + "," + "[" + mismatch.Split(',')[0] + " : " + "]");
                                    }
                                    else
                                    {
                                        string lastName = op[op.Count - 1];
                                        if (!lastName.Contains(','))
                                        {
                                            outputCSV.Add(name + "," + tagname.Split(':')[0] + "," + "[" + mismatch.Split(',')[0] + "]");
                                        }
                                        else
                                            outputCSV.Add(name + "," + tagname.Split(':')[0] + "," + "[" + mismatch.Split(',')[0] + " : " + lastName.Split(',')[2] + "]");
                                    }
                                }
                                //else if (tagname.StartsWith("StateNo"))
                                else if (tagname.Contains("StateNo"))
                                {
                                    op.Add(name + "," + tagname.Split(':')[0] + "," + mismatch.Split(',')[0]);
                                }
                                else
                                {
                                    outputCSV.Add(name + "," + tagname.Split(':')[0] + "," + "[" + mismatch.Split(',')[0] + "]");
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            int lastIndexState = op.FindLastIndex(item => item.Contains("StateNo"));
            if (lastIndexState != null && lastIndexState != -1)
            {
                outputCSV.Add(op[lastIndexState]);
            }
            List<string> updatedList = outputCSV
                                        .Select(tag => tag.Replace("True", "ON ")).ToList();
            List<string> updatedList1 = updatedList
                                        .Select(tag => tag.Replace("False", "OFF")).ToList();
            int a = 0;

            outputCSV = updatedList1;

            List<string> dam = new List<string>();

            //dataOkNok = new List<string>();
            int lastIndex = -1;
            int valcount = 0;
            int tagCount = -1;
            List<string> extdata = new List<string>();
            List<string> prevData = new List<string>();
            string lastval = "";
            int lastIndexof = TagData.FindLastIndex(item => item.StartsWith("[1]"));
            //File.WriteAllLines(@"C:\Test Suits\New Text Document.txt", TagData);
            File.WriteAllLines(@"C:\Generate Block\V20 All Files\Test Suits\New Text Document.txt", TagData);

            foreach (var t in TagData)
            {
                extdata.Add(t + "," + name);
            }

            Dictionary<int, string> stateDictionary = new Dictionary<int, string>();
            List<KeyValuePair<string, string>> Pair = new List<KeyValuePair<string, string>>();
            try
            {
                try
                {
                    for (int i = 0; i < BlockName.Count(); i++)
                    {
                        string blockna = BlockName[i];
                        for (int j = 0; j < extdata.Count(); j++)
                        {
                            for (int k = 0; k < TextID.Count(); k++)
                            {
                                string in1 = extdata[j].Split(',')[1].Trim();
                                string in2 = TextID[k].Split(',')[2].Trim();
                                if (extdata[j].Split(',')[1].Trim() == TextID[k].Split(',')[2].Trim())
                                {
                                    if (extdata[j].Split(',')[0].Trim() == TextID[k].Split(',')[0].Trim())
                                    {
                                        if (TextID[k].Split(',')[1].Trim().Contains("[ST" + extdata[j].Split(',')[0] + "]") || TextID[k].Split(',')[1].ToLower().Trim().Contains("state"))
                                        {
                                            string output = TextID[k].Split(',')[1].Replace("@", "");
                                            Pair.Add(new KeyValuePair<string, string>(extdata[j].Split(',')[0], output + "," + TextID[k].Split(',')[2]));
                                            break;
                                        }
                                        else if (extdata[j].Split(',')[0] == "0" && TextID[k].Split(',')[0] == "0")
                                        {
                                            string output = TextID[k].Split(',')[1].Replace("@", "");
                                            Pair.Add(new KeyValuePair<string, string>(extdata[j].Split(',')[0], output + "," + TextID[k].Split(',')[2]));
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        int a3 = 0;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            Compair = new List<string>();
            for (int t = 0; t < op.Count(); t++)
            {
                try
                {
                    if (op[t].Contains("State traces Flow"))
                        continue;
                    else
                    {
                        string val = op[t].Split(',')[2];
                        foreach (var t1 in TextID)
                        {
                            if (val == t1.Split(',')[0])
                            {
                                op[t] = t1.Split(',')[0] + ", " + t1.Split(',')[1];
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            List<List<string>> Discriptions1 = new List<List<string>>();
            Discriptions1.Add(op);
            Discriptions1.Add(outputCSV);
            if (op.Count() == 0 || op.Count() == 1)
                return;

            List<List<string>> updOpValue1 = new List<List<string>>();
            List<List<string>> updOpValue2 = new List<List<string>>();
            List<List<string>> updOpValue = new List<List<string>>();
            if (Discriptions1.Count > 0)
            {
                List<string> l1 = new List<string>();
                l1 = Discriptions1[0];
                List<string> l2 = new List<string>();
                l2 = Discriptions1[1];
                List<string> currentBlock = null;
                foreach (var t in l1)
                {
                    if (t.Contains("State traces Flow"))
                    {
                        if (currentBlock != null && currentBlock.Count > 0)
                        {
                            updOpValue.Add(currentBlock);
                            if (currentBlock[0].Contains("State traces Flow"))
                            {
                                updOpValue1.Add(currentBlock);
                            }
                            else if (currentBlock[0].Contains("Output tag Flow"))
                            {
                                updOpValue2.Add(currentBlock);
                            }
                        }
                        currentBlock = new List<string>();
                        currentBlock.Add(t);
                        continue;
                    }
                    if (currentBlock != null)
                    {
                        currentBlock.Add(t);
                    }
                }
                if (currentBlock != null && currentBlock.Count > 0)
                {
                    updOpValue.Add(currentBlock);
                    if (currentBlock[0].Contains("State traces Flow"))
                    {
                        updOpValue1.Add(currentBlock);
                    }
                    else if (currentBlock[0].Contains("Output tag Flow"))
                    {
                        updOpValue2.Add(currentBlock);
                    }
                }
                foreach (var t in l2)
                {
                    try
                    {
                        if (t.Contains("Output tag Flow"))
                        {
                            if (currentBlock != null && currentBlock.Count > 0)
                            {
                                updOpValue.Add(currentBlock);
                                if (currentBlock[0].Contains("State traces Flow"))
                                {
                                    updOpValue1.Add(currentBlock);
                                }
                                else if (currentBlock[0].Contains("Output tag Flow"))
                                {
                                    updOpValue2.Add(currentBlock);
                                }
                            }
                            currentBlock = new List<string>();
                            currentBlock.Add(t);
                            continue;
                        }
                        if (currentBlock != null)
                        {
                            string g = t.Replace(":", " ");
                            currentBlock.Add(g.Split(',')[1] + ":" + g.Split(',')[2]);
                        }
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                if (currentBlock != null && currentBlock.Count > 0)
                {
                    updOpValue.Add(currentBlock);
                    if (currentBlock[0].Contains("State traces Flow"))
                    {
                        updOpValue1.Add(currentBlock);
                    }
                    else if (currentBlock[0].Contains("Output tag Flow"))
                    {
                        updOpValue2.Add(currentBlock);
                    }
                }
            }

            int r1 = 0;

            string[] fil = Directory.GetFiles(@"C:\Generate Block\Excel Files", "*.xlsx");
            List<List<string>> toAdd = new List<List<string>>();
            List <string> newEntry=new List<string>();
            Console.WriteLine("Block name to match: " + name);
            //excelForTest = "";
            if (excelForTest == "true")
            {
                foreach (var f in fil)
                {
                    string na = System.IO.Path.GetFileNameWithoutExtension(f);
                    Console.WriteLine("Checking Excel file: " + na);

                    //
                    if (name.StartsWith("FB_", StringComparison.OrdinalIgnoreCase))
                    {
                        name = name.Substring(3);
                        name = Regex.Replace(name, @"_V\d+$", "");
                    }
                    //
                    bool isMatch = false;
                    if (name.Contains(na))
                    {
                        isMatch = true;
                    }
                    //bool isMatch =
                    //    name.Equals(na, StringComparison.OrdinalIgnoreCase) ||
                    //    name.StartsWith(na + "_", StringComparison.OrdinalIgnoreCase);

                    //static List<List<string>> dataOkNok = new List<List<string>>();

                    if (isMatch)
                    {
                        Console.WriteLine("Matched file: " + f);

                        List<string> Dataa = ReadExcelData(f);

                        List<string> DataaOp = ReadExcelDataOp(f);

                        List<string> containsAmpersand = Dataa
                            .Where(item => item.Contains("&"))
                            .ToList();

                        newEntry.Add(BlockName[0]);

                        for (int h = 0; h < updOpValue.Count(); h++)
                        {
                            var t = updOpValue[h];
                            Compair = new List<string>();

                            List<string> compairOp = new List<string>();

                            if (t[0].Contains("State traces Flow"))
                            {
                                for (int i = 1; i < t.Count(); i++)
                                {
                                    Compair.Add(t[i].Split(',')[0]);
                                }

                                if (containsAmpersand.Count > 0)
                                {
                                    foreach (var item in containsAmpersand)
                                    {
                                        var parts = item.Split(',');
                                        if (parts.Length != 2) continue;

                                        bool check = false;
                                        string first = parts[0].Trim();

                                        string[] secondParts = parts[1].Split('&').Select(x => x.Trim()).ToArray();

                                        for (int idx = 0; idx < t.Count; idx++)
                                        {
                                            if (t[idx].Contains(first))
                                            {
                                                check = true;
                                                break;
                                            }
                                        }

                                        bool allPresent = secondParts.All(teItem =>
                                            t.Any(tItem => tItem.StartsWith(teItem + ",")));

                                        if (t.Contains(first) && !t[0].Contains("&"))
                                            t[0] += " &" + first;

                                        if (t.Contains(first) && secondParts.Any(x => t.Contains(x)) && !t[0].Contains("&"))
                                            t[0] += " &" + first;

                                        if (allPresent && check)
                                        {

                                            Console.WriteLine("All parts matched for: " + first);
                                            File.WriteAllLines(@"C:\Generate Block\V20 All Files\Downloads\New Text Document.txt", Dataa);
                                            List<List<string>> fork = checkFork(Dataa, t, containsAmpersand, Compair, secondParts);

                                            updOpValue[h] = fork[0];
                                            Compair = new List<string>();
                                            t = updOpValue[h];

                                            for (int i = 1; i < t.Count(); i++)
                                                Compair.Add(t[i].Split(',')[0]);

                                            for (int k = 1; k < fork.Count; k++)
                                                updOpValue.Insert(h + k, fork[k]);
                                        }
                                    }
                                }

                                List<string> filtered;
                                bool OK = compairReport(Dataa, Compair, out filtered);

                                if (OK)
                                {
                                    if (t.Last().Contains("Tested Ok") || t.Last().Contains("Tested Not Ok"))
                                    {
                                        t[t.Count - 1] = "Tested Ok";
                                        newEntry.Add(t[0] + ", " + "Tested Ok");
                                    }
                                    else
                                    {
                                        newEntry.Add(t[0] + ", " + "Tested Ok");
                                        t.Add("Tested Ok");
                                    }
                                }
                                else
                                {
                                    string sdata = string.Join("", filtered.Select(x => $"[{x}]"));

                                    if (t.Last().Contains("Tested Ok") || t.Last().Contains("Tested Not Ok"))
                                    {
                                        t[t.Count - 1] = "Tested Not Ok," + sdata;
                                        newEntry.Add(t[0] + ", " + "Tested Not Ok," + sdata);
                                    }
                                    else
                                    {
                                        newEntry.Add(t[0] + ", " + "Tested Not Ok," + sdata);
                                        //newEntry.Add("Tested Not Ok," + sdata);
                                        t.Add("Tested Not Ok," + sdata);
                                    }
                                }
                            }

                            else if (t[0].Contains("Output tag Flow"))
                            {
                                List<string> filtered;
                                bool okOP = compairReportOp(DataaOp, t, out filtered);
                                updOpValue[h] = filtered;

                                //if (okOP)
                                //{
                                //    if (t.Last().Contains("Tested Ok") || t.Last().Contains("Tested Not Ok"))
                                //        t[t.Count - 1] = "Tested Ok";

                                //    else
                                //        t.Add("Tested Ok");
                                //}

                                //else
                                //{
                                //    string sdata = string.Join("", filtered.Select(x => $"[{x}]"));
                                //    if (t.Last().Contains("Tested Ok") || t.Last().Contains("Tested Not Ok"))

                                //        t[t.Count - 1] = "Tested Not Ok," + sdata;

                                //    else
                                //    {
                                //        dataOkNok.Add(t[0] + ", " + "Tested Not Ok," + sdata);

                                //        t.Add("Tested Not Ok," + sdata);
                                //    }
                                //}
                            }
                        }
                        //int e = 0;
                        break;
                    }
                }

                dataOkNok.Add(newEntry);

            }
            else
            {
                dataOkNok.Add(new List<string> { name });
            }

            for (int i = 0; i < updOpValue2.Count; i++)
            {
                for (int j = 0; j < updOpValue.Count; j++)
                {
                    if (updOpValue[j][0] == updOpValue2[i][0])
                    {
                        updOpValue2[i] = updOpValue[j];
                    }
                }
            }

            int r = 0;
            //Discriptions = new List<List<List<string>>>();

            int maxCount = Math.Max(updOpValue1.Count, updOpValue2.Count);

            for (int i = 0; i < maxCount; i++)
            {
                List<string> temp = new List<string>();

                if (i < updOpValue1.Count)
                    temp.AddRange(updOpValue1[i]);

                if (i < updOpValue2.Count)
                    temp.AddRange(updOpValue2[i]);

                List<string> firstRow = new List<string> { BlockName[0] };
                firstRow.AddRange(temp); 

                List<List<string>> flowGroup = new List<List<string>>
                {
                    firstRow,
                    new List<string>(temp)
                };

                Discriptions.Add(flowGroup);
            }

            List<string> resultCSV = new List<string>(File.ReadAllLines(csvpath));
            //List<string> resultCSV = new List<string>(File.ReadAllLines(@"C:\Generate Block\V20 All Files\Testing\Testing CSV\Data_20250721_105829.csv"));
            //tempCsv = new List<string>();
            bool checkCSV = false;
            for (int t = 0; t < resultCSV.Count(); t++)
            {
                if ((resultCSV[t].Contains("Block_Name") && resultCSV[t].Split(',')[1].Trim() == BlockName[0]) || checkCSV)
                {
                    checkCSV = true;
                    tempCsv.Add(resultCSV[t]);
                    if (t == resultCSV.Count - 1)
                    {
                        break;
                    }
                    if (resultCSV[t + 1].Contains("Block_Name"))
                    {
                        break;
                    }
                }
            }

            int maxLength = Math.Max(tempCsv.ToList().Count, Math.Max(op.Count, outputCSV.Count));
            try
            {
                for (int j = 0; j < tempCsv.Count(); j++)
                {
                    if (tempCsv[j].Contains("Block_Name") && tempCsv[j].Split(',')[1].Trim() == BlockName[0])
                    {
                        for (int i = 0; i < maxLength; i++)
                        {
                            j++;

                            if (j == 68)
                                ;
                            if (i < outputCSV.Count)
                            {
                                string dataCSV = "";
                                if (outputCSV[i].Contains(","))
                                    dataCSV = outputCSV[i].Split(',')[1] + " : " + outputCSV[i].Split(',')[2];
                                else
                                    dataCSV = outputCSV[i];
                                if ((i < tempCsv.Count() - 1 && i <= j) && j < tempCsv.Count)
                                {
                                    string res = tempCsv[j];
                                    if (!res.Contains(','))
                                    {
                                        res += ",,,";
                                        tempCsv[j] = res + dataCSV;
                                    }
                                    else
                                    {
                                        tempCsv[j] = tempCsv[j] + "," + dataCSV;
                                    }
                                }
                                else
                                    tempCsv.Add(",,," + dataCSV);
                            }
                            if (i < op.Count)
                            {
                                string dataCSV = string.Join(",", dataOkNok.FirstOrDefault(item => item.Any(s => s.Contains(op[i]))) ?? new List<string>());

                                if (j < tempCsv.Count)
                                {
                                    string line = tempCsv[j];
                                    List<string> parts = line.Split(',').ToList();

                                    while (parts.Count < 4)
                                        parts.Add("");

                                    if (!string.IsNullOrWhiteSpace(parts[0]))
                                    {
                                        parts.Add(op[i]); 
                                    }
                                    else
                                    {
                                        parts.Add(op[i]);
                                    }

                                    tempCsv[j] = string.Join(",", parts);
                                }
                                else
                                {
                                    tempCsv.Add(",,,," + op[i]);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("ERROR");
            }
        }
        catch
        {
            MessageBox.Show("ERROR");
        }
    }

    public static bool compairReportOp(List<string> Result, List<string> Result1, out List<string> filteredPairs)
    {
        List<string> filteredPair = new List<string>();
        bool allSame = false;
        filteredPair.Add(Result1[0]);
        foreach (var t in Result)
        {
            foreach (var r in Result1)
            {
                if (t.Split(',')[1].Contains(r.Split(':')[0]) && r.Split(':')[1].Contains(t.Split(',')[0]))
                {
                    if (r.Split(':')[1].Contains(t.Split(',')[0]))
                    {
                        string[] words = t.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);

                        if (words.Count() == 2)
                        {
                            if (r.Split(':')[1].Contains("OFF"))
                            {
                                filteredPair.Add(r + "," + "Tested Not Ok");
                            }
                            else if (r.Split(':')[1].Contains("ON"))
                            {
                                filteredPair.Add(r + "," + "Tested Ok");
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                            filteredPair.Add(r + "," + "Not Sure");
                        }
                    }
                    else
                    {
                        string[] words = t.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);

                        if (words.Count() == 2)
                        {
                            if (r.Split(':')[1].Contains("OFF"))
                            {
                                filteredPair.Add(r + "," + "Tested Not Ok");
                            }
                            else
                            {
                            }
                        }
                        else
                            filteredPair.Add(r + "," + "Not Sure");
                    }
                }
            }
        }

        if (filteredPair.Count() == 1)
        {
            filteredPair.AddRange(Result1.Select(item => item + ",Not Sure"));
        }

        filteredPairs = filteredPair;
        if (filteredPair.Count == 0)
        {
            allSame = true;
        }
        return allSame;
    }

    //File.WriteAllLines(@"C:\Users\Wpuser\Downloads\Final\Generate Block\Testing\Testing CSV\Data_20250711_090017.csv", resultCSV.ToList());
    //File.WriteAllLines(@"C:\Users\Wpuser\Downloads\Final\Generate Block\Testing\Testing CSV\Data_20250718_092937", resultCSV.ToList());
    //File.WriteAllLines(@"C:\Generate Block\V20 All Files\Testing\Testing CSV\Data_20250721_105829.csv", resultCSV.ToList());

    public static List<string> ReadExcelData(string excelPath)
    {
        List<string> excelData = new List<string>();

        var xlApp = new Microsoft.Office.Interop.Excel.Application();
        var xlWorkbook = xlApp.Workbooks.Open(excelPath, 0, false, 5, "", "", true, XlPlatform.xlWindows, "\t", true, false, 0, true, 1, 0);
        xlApp.DisplayAlerts = false;

        Worksheet xlWorksheet = (Worksheet)xlWorkbook.Sheets[1];

        Range xlRange = xlWorksheet.UsedRange;

        for (int row = 1; row <= xlRange.Rows.Count; row++)
        {
            var columnAValue = xlRange.Cells[row, 1].Value2;
            var columnDValue = xlRange.Cells[row, 4].Value2;

            if (columnAValue != null && columnDValue != null)
            {
                string combinedValue = columnAValue.ToString() + "," + columnDValue.ToString();
                excelData.Add(combinedValue);
            }
            else if (columnAValue != null && columnDValue == null)
            {
                string combinedValue = columnAValue.ToString() + "," + " ";
                excelData.Add(combinedValue);
            }
        }

        xlWorkbook.Close(false);
        xlApp.Quit();

        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkbook);
        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorksheet);
        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlRange);

        return excelData;
    }
    public static List<string> ReadExcelDataOp(string excelPath)
    {
        List<string> excelData = new List<string>();

        var xlApp = new Microsoft.Office.Interop.Excel.Application();
        var xlWorkbook = xlApp.Workbooks.Open(excelPath, 0, false, 5, "", "", true, XlPlatform.xlWindows, "\t", true, false, 0, true, 1, 0);
        xlApp.DisplayAlerts = false;

        Worksheet xlWorksheet = (Worksheet)xlWorkbook.Sheets[1];

        Range xlRange = xlWorksheet.UsedRange;

        for (int row = 1; row <= xlRange.Rows.Count; row++)
        {
            var columnAValue = xlRange.Cells[row, 1].Value2;
            var columnDValue = xlRange.Cells[row, 5].Value2;

            if (columnAValue != null && columnDValue != null)
            {
                string combinedValue = columnAValue.ToString() + "," + columnDValue.ToString();
                excelData.Add(combinedValue);
            }
            else if (columnAValue != null && columnDValue == null)
            {
                string combinedValue = columnAValue.ToString() + "," + " ";
                excelData.Add(combinedValue);
            }
        }

        xlWorkbook.Close(false);
        xlApp.Quit();

        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkbook);
        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorksheet);
        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlRange);

        return excelData;
    }
    public static bool compairReport(List<string> Result, List<string> Result1, out List<string> filteredPairs)
    {
        List<string> tmv = new List<string>();
        foreach (var t in Result1)
        {
            if (t != "0")
                tmv.Add(t);
        }
        Result1.RemoveAll(item => item.Trim().Equals("0", StringComparison.OrdinalIgnoreCase));
        List<(string, string)> pairs = new List<(string, string)>();
        HashSet<string> seenPairs = new HashSet<string>();

        for (int i = 0; i < Result1.Count - 1; i++)
        {
            var pair = (Result1[i], Result1[i + 1]);
            string pairKey = $"{pair.Item1},{pair.Item2}";

            if (seenPairs.Contains(pairKey))
                break;

            seenPairs.Add(pairKey);
            pairs.Add(pair);
        }
        if (Result.Contains("&"))
        {
        }
        List<string> result = new List<string>();

        for (int i = 0; i < Result.Count; i++)
        {
            var current = Result[i].Split(',')
                                  .Select(x => x.Trim())
                                  .Where(x => !string.IsNullOrEmpty(x))
                                  .ToList();

            if (current.Count == 2)
            {
                var secondParts = current[1]
                                 .Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(x => x.Trim());
 
                foreach (var part in secondParts)
                {
                    if (!string.IsNullOrEmpty(part))
                        result.Add($"{current[0]},{part}");
                }
            }
            else if (current.Count == 1 && i + 1 < Result.Count)
            {
                var next = Result[i + 1].Split(',')
                                       .Select(x => x.Trim())
                                       .Where(x => !string.IsNullOrEmpty(x))
                                       .ToList();

                if (next.Count >= 1)
                {
                    result.Add($"{current[0]},{next[0]}");
                }
            }
        }

        List<string> toRemove = new List<string>();

        foreach (string pair in seenPairs)
        {
            var parts = pair.Split(',');
            if ((parts.Length == 2 && parts[0].Trim() == parts[1].Trim()) || result.Contains(pair))
            {
                toRemove.Add(pair);
            }
        }

        foreach (var item in toRemove)
        {
            seenPairs.Remove(item);
        }

        filteredPairs = seenPairs.ToList();

        bool allSame = true;
        if (filteredPairs.Count > 0)
        {
            for (int i = 1; i < filteredPairs.Count; i++)
            {
                if (filteredPairs[i] != filteredPairs[0])
                {
                    allSame = false;
                    break;
                }
            }
            for (int i = 0; i < filteredPairs.Count; i++)
            {
                if (filteredPairs[i].Split(',')[0].Trim() != filteredPairs[i].Split(',')[1].Trim())
                {
                    allSame = false;
                    break;
                }
            }
        }
        return allSame;
    }
    public static List<List<string>> checkFork(List<string> Data, List<string> genData, List<string> containAmpersent, List<string> compaire, string[] seconfParts)
    {
        string ampercent = containAmpersent[0].Split(',')[0].Trim();
        string input = genData[0];
        Match match = Regex.Match(input, @"\d+");

        if (match.Success)
        {
            int number = int.Parse(match.Value);
            input = number.ToString();
        }

        List<List<string>> allResults = new List<List<string>>();

        List<(string, string)> pairs = new List<(string, string)>();
        HashSet<string> seenPairs = new HashSet<string>();

        for (int i = 0; i < compaire.Count - 1; i++)
        {
            var pair = (compaire[i], compaire[i + 1]);
            string pairKey = $"{pair.Item1},{pair.Item2}";

            if (seenPairs.Contains(pairKey))
                break;

            seenPairs.Add(pairKey);
            pairs.Add(pair);
        }
        int count = 0;
        foreach (var t in seconfParts)
        {
            count++;
            List<string> forkd = new List<string>();
            forkd.Add("State traces Flow" + input + "_" + count + " &" + ampercent);
            int index = compaire.IndexOf(t);
            for (int i = 0; i < index; i++)
            {
                if (seconfParts.Contains(compaire[i]) && compaire[i] != t)
                    continue;
                forkd.Add(compaire[i]);
            }
            graph = new Dictionary<string, List<string>>();
            visitedGlobal = new HashSet<string>();
            string[] data = Data.ToArray();
            BuildGraph(data.Skip(1));
            List<string> forkdata = new List<string>();
            var path160 = new List<string>();
            visitedGlobal.Clear();
            string end = compaire.Last();
            DFSUntilEnd(t, t, path160, false, end);
            foreach (var step in path160) forkd.Add(step);
            allResults.Add(forkd);
        }

        //File.WriteAllLines(@"C:\Users\Wpuser\Downloads\New Text Document.txt", seenPairs);
        File.WriteAllLines(@"C:\Generate Block\V20 All Files\Downloads\New Text Document.txt", seenPairs);

        foreach (var innerList in allResults)
        {
            for (int i = innerList.Count - 1; i >= 1; i--)
            {
                if (!compaire.Contains(innerList[i]))
                {
                    innerList.RemoveAt(i);
                }
            }
            for (int d = 1; d < innerList.Count(); d++)
            {
                for (int t = 1; t < genData.Count(); t++)
                {
                    if (genData[t].Split(',')[0].Trim() == innerList[d].Trim())
                    {
                        string val = genData[t].Split(',')[1];
                        innerList[d] += "," + val;
                    }
                }
            }
        }
        return allResults;
    }
    public static bool DFSUntilEnd(string current, string target, List<string> path, bool targetReached, string end)
    {
        path.Add(current);
        visitedGlobal.Add(current);

        if (current == target)
            targetReached = true;

        if (targetReached && current == end)
            return true;

        if (graph.ContainsKey(current))
        {
            foreach (var neighbor in graph[current])
            {
                if (!visitedGlobal.Contains(neighbor))
                {
                    if (DFSUntilEnd(neighbor, target, path, targetReached, end))
                        return true;
                }
            }
        }

        path.RemoveAt(path.Count - 1);
        visitedGlobal.Remove(current);
        return false;
    }
    static HashSet<string> visitedGlobal;
    static Dictionary<string, List<string>> graph;
    bool DFS(string current, string target, HashSet<string> visited, List<string> path, List<string> result)
    {
        visited.Add(current);
        path.Add(current);

        if (current == target)
        {
            result.AddRange(path);
            return true;
        }

        if (graph.ContainsKey(current))
        {
            foreach (var neighbor in graph[current])
            {
                if (!visited.Contains(neighbor))
                {
                    if (DFS(neighbor, target, visited, path, result))
                        return true;
                }
            }
        }
        path.RemoveAt(path.Count - 1);
        visited.Remove(current);
        return false;
    }
    public static void BuildGraph(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts.Length != 2) continue;

            string from = parts[0].Trim();
            string[] toList = parts[1].Split('&');

            if (!graph.ContainsKey(from))
                graph[from] = new List<string>();

            foreach (var to in toList)
            {
                var clean = to.Trim();
                if (!string.IsNullOrEmpty(clean))
                    graph[from].Add(clean);
            }
        }
    }
    public static string WriteTestResultss(TestResults result)
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string dateTimeSuffix = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string textFilePath = System.IO.Path.Combine(desktopPath, $"TestResults_{dateTimeSuffix}.txt");
        //textFilePath = @"C:\Test Suits\Report Gen\" + $"TestResults_{dateTimeSuffix}.txt";
        //textFilePath = @"C:\Generate Block\V20 All Files\Test Suits\Report Gen" + $"TestResults_{dateTimeSuffix}.txt";
        string folderPath = @"C:\Generate Block\V20 All Files\Test Suits\Report Gen";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        textFilePath = Path.Combine(folderPath, $"TestResults_{dateTimeSuffix}.txt");
        using (StreamWriter writer = new StreamWriter(textFilePath, append: false))
        {
            writer.WriteLine("State: " + result.State);
            writer.WriteLine("Warning Count: " + result.WarningCount);
            writer.WriteLine("Error Count: " + result.ErrorCount);
            RecursivelyWriteMessages(result.Messages, writer);
        }

        Console.WriteLine("Test results saved to: " + textFilePath);
        //string textFilePathCopy = @"C:\Test Suits\Report Gen\AutoGenXml\" + $"TestResults_{dateTimeSuffix}.txt";
        //string textFilePathCopy = @"C:\Generate Block\V20 All Files\Test Suits\Report Gen\AutoGenXml" + $"TestResults_{dateTimeSuffix}.txt";
        //System.IO.File.Copy(textFilePath, textFilePathCopy, true);
        //return textFilePathCopy;
       
        string copyFolderPath = Path.Combine(folderPath, "AutoGenXml");
        Directory.CreateDirectory(copyFolderPath);

        string textFilePathCopy = Path.Combine(copyFolderPath, $"TestResults_{dateTimeSuffix}.txt");
        File.Copy(textFilePath, textFilePathCopy, true);

        return textFilePathCopy;
    }

    private static void WriteTestResults(TestResults result, List<string> Disc)
    {
        Console.WriteLine("Error Count:" + result.ErrorCount);
        RecursivelyWriteMessages(result.Messages, Disc);
    }
    public static void RecursivelyWriteMessages(TestResultsMessageComposition messages, List<string> Disc, string indent
   = "")
    {
        string test = "";
        indent += "\t";
        foreach (TestResultsMessage message in messages)
        {
            Console.WriteLine(indent + "Path: " + message.Path);
            Console.WriteLine(indent + "DateTime: " + message.DateTime);
            Console.WriteLine(indent + "State: " + message.State);
            Console.WriteLine(indent + "Description: " + message.Description);
            Console.WriteLine(indent + "Warning Count: " + message.WarningCount);
            Console.WriteLine(indent + "Error Count: " + message.ErrorCount);
            if (message.Path != null)
            {
                if (message.Path.Contains("Transition"))
                {
                    string pattern = @"Transition\s+(\d+)";
                    string pat = message.Path;
                    Match match = Regex.Match(pat, pattern);
                    if (match.Success)
                    {
                        test = match.Groups[1].Value;
                    }
                }
            }
            if (message.Messages.Count() != 0)
                ;
            Disc.Add(test + "," + message.Path + ": " + message.Description);
            RecursivelyWriteMessages(message.Messages, Disc, indent);
        }
    }

    private static void RecursivelyWriteMessages(TestResultsMessageComposition messages, StreamWriter writer, string indent = "")
    {
        indent += "\t";
        foreach (TestResultsMessage message in messages)
        {
            writer.WriteLine(indent + "Path: " + message.Path);
            writer.WriteLine(indent + "DateTime: " + message.DateTime);
            writer.WriteLine(indent + "State: " + message.State);
            writer.WriteLine(indent + "Description: " + message.Description);
            writer.WriteLine(indent + "Warning Count: " + message.WarningCount);
            writer.WriteLine(indent + "Error Count: " + message.ErrorCount);
            RecursivelyWriteMessages(message.Messages, writer, indent);
        }
    }
    private static void PostConfigureDownload(DownloadConfiguration downloadConfiguration)
    {
        StartModules startModules = downloadConfiguration as StartModules;
        if (startModules != null)
        {
            startModules.CurrentSelection = StartModulesSelections.StartModule;
        }
    }
    private static void PreConfigureDownload(DownloadConfiguration downloadConfiguration)
    {
        StopModules stopModules = downloadConfiguration as StopModules;
        if (stopModules != null)
        {
            stopModules.CurrentSelection = StopModulesSelections.StopAll;
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
        throw new NotSupportedException();
    }

    static void ReadExcelDataV20UI(string tatFile, string excelFile)
    {
        try
        {
            string[] tatFIle = Directory.GetFiles(tatFile, "*.TTST");
            //string[] RmvtatFIle = Directory.GetFiles(@"C:\Test Suits\Input Tat\ModifiedTat", "*.TTST");
            string[] RmvtatFIle = Directory.GetFiles(@"C:\Generate Block\V20 All Files\Test Suits\Input Tat\ModifiedTat", "*.TTST");
            foreach (var t in RmvtatFIle)
            {
                if (System.IO.File.Exists(t))
                {
                    System.IO.File.Delete(t);
                }
            }
            string excelFilePath = excelFile;
            string sheetName = "Sheet1";
            string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={excelFilePath};Extended Properties=\"Excel 12.0 Xml;HDR=NO;IMEX=1\";";
            List<string> colAdata = new List<string>();
            List<string> colBdata = new List<string>();
            List<string> colCdata = new List<string>();
            //ProjectServiceProvider.CloseExcelProcessByFile(ToolBarViewModel.csvFilePath);
            Thread.Sleep(3000);
            if (!System.IO.File.Exists(excelFilePath))
            {
                return;
            }
            try
            {
                using (StreamReader reader = new StreamReader(excelFilePath))
                {
                    string headerLine = reader.ReadLine();
                    if (headerLine != "")
                    {
                        colAdata.Add(headerLine.Split(',')[0]);
                        colBdata.Add(headerLine.Split(',')[1]);
                        colCdata.Add("StateTrace");
                    }
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (line.Contains("Block_Name"))
                        {
                            colAdata.Add(line.Split(',')[0]);
                            colBdata.Add(line.Split(',')[1]);
                            colCdata.Add("StateTrace");
                            continue;
                        }
                        string input = line;
                        List<string> InData = new List<string>();
                        //string[] values = line.Split(',');
                        int goi = line.Split(',').Count();
                        if (line.Split(',').Count() > 1)
                        {
                            if (line.Split(',')[1].Contains("["))
                            {
                                List<string> resultList = new List<string>();
                                string name = input.Split(',')[0];
                                string boolValuesPart = input.Split(',')[1];
                                //var boolMatches = Regex.Matches(boolValuesPart, @"\[([A-Za-z]+)\]");
                                var boolMatches = Regex.Matches(boolValuesPart, @"\[(\w+)\]");
                                int boolm = boolMatches.Count;
                                List<string> lbt = new List<string>();
                                List<string> num = new List<string>();
                                foreach (Match match in boolMatches)
                                {
                                    lbt.Add(match.Value);
                                }
                                string numberPart = Regex.Match(input, "\"(\\[[^\\]]+\\])\"").Groups[1].Value;
                                string pattern = @"\[(\d+(?:,\d+)*)\]";
                                string[] patt = ParseCsvLine(line);
                                MatchCollection matches = Regex.Matches(patt[2], pattern);
                                int count1 = 1;
                                foreach (Match match in matches)
                                {
                                    if (match.Success)
                                    {
                                        string numbers = match.Groups[1].Value;
                                        num.Add(numbers);
                                    }
                                }
                                for (int ft = 0; ft < num.Count; ft++)
                                {
                                    if (ft == lbt.Count)
                                        break;
                                    if (num[ft].Split(',').Count() > 0)
                                    {
                                        for (int y = 0; y < num[ft].Split(',').Count(); y++)
                                        {
                                            InData.Add(name + "," + num[ft].Split(',')[y] + "," + lbt[ft]);
                                        }
                                    }
                                    else
                                    {
                                        InData.Add(name + "," + num[ft] + "," + lbt[ft]);
                                    }
                                }
                            }
                            if (InData.Count > 0)
                            {
                                for (int i = 0; i < InData.Count; i++)
                                {
                                    colAdata.Add(InData[i].Split(',')[0]);
                                    colBdata.Add(InData[i].Split(',')[2].Split('[')[1].Split(']')[0]);
                                    colCdata.Add(InData[i].Split(',')[1]);
                                }
                            }
                            else
                            {
                                string[] values = ParseCsvLine(line);
                                if (values.Length >= 2)
                                {
                                    var key = values[0]?.Trim();
                                    var value = values[1]?.Trim();
                                    var valueState = values[2]?.Trim();
                                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                                    {
                                        colAdata.Add(key);
                                        colBdata.Add(value);
                                        colCdata.Add(valueState);
                                    }
                                    else
                                    {
                                    }
                                }
                                else
                                {
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }
            List<List<string>> Lbc = new List<List<string>>();
            List<string> result = colBdata.Select(item =>
            {
                if (item.Contains("[") && item.Contains("]"))
                {
                    var unique = item.Split(new[] { '[', ']', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Distinct(StringComparer.OrdinalIgnoreCase)
                                     .ToList();

                    return string.Join("", unique.Select(v => $"[{v}]")) + "";
                }
                else
                {
                    return item;
                }
            }).ToList();
            colBdata = result;
            var cleanedList = colBdata.Select(x => x.Trim('[', ']')).ToList();
            var cleanedLisCt = colCdata.Select(x => x.Trim('[', ']')).ToList();
            colBdata = cleanedList;
            colCdata = cleanedLisCt;
            string step = "";
            for (int c = 0; c < colCdata.Count(); c++)
            {
                foreach (var t in colCdata[c].Split(','))
                {
                    if (colAdata[c].Contains("."))
                    {
                        string rep = colAdata[c];
                        string cleanedString = Regex.Replace(rep, @"[-.]", "");
                        colAdata[c] = cleanedString;
                    }
                    if (colAdata[c].Contains(" "))
                    {
                        string rep = colAdata[c];
                        string cleanedString = Regex.Replace(rep, @"\s", "");
                        colAdata[c] = cleanedString;
                    }
                    if (colAdata[c].Contains("BlockName") || colAdata[c].Contains("Block_Name"))
                    {
                        Lbc.Add(new List<string> { t + "," + colAdata[c] + "," + colBdata[c] });
                    }
                    else
                    {
                        if (Lbc.Count > 0)
                        {
                            Lbc[Lbc.Count - 1].Add(t + "," + colAdata[c] + "," + colBdata[c]);
                        }
                    }
                }
            }
            bool stepCheck = false;
            int count = 0;
            List<string> fileData = new List<string>();
            List<string> subFile = new List<string>();
            if (tatFIle.Count() >= 1)
            {
                foreach (var tat in tatFIle)
                {
                    string file = tat;
                    List<string> Lbcd = new List<string>();
                    fileData = System.IO.File.ReadAllLines(tat).ToList();
                    foreach (var lst in Lbc)
                    {
                        if (lst[0].Contains(Path.GetFileNameWithoutExtension(tat)))
                        {
                            if (lst[0].Contains("d2b62273-f49b-4444-8c79-d6caf922cfb3_PR_GantryUnload_Seqv2"))
                            {

                            }
                            foreach (var lstn in lst)
                            {
                                string fileTTSTName = Path.GetFileNameWithoutExtension(tat);
                                //if (lst[0].Split(',')[2].Trim() == Path.GetFileNameWithoutExtension(tat).Trim())
                                //{
                                //System.IO.File.Copy(tat, @"C:\Test Suits\Input Tat\ModifiedTat\" + "" + ".TTST", overwrite: true);
                                System.IO.File.Copy(tat, @"C:\Generate Block\V20 All Files\Test Suits\Input Tat\ModifiedTat\" + fileTTSTName + ".TTST", overwrite: true);
                                //file = @"C:\Test Suits\Input Tat\ModifiedTat\" + "" + ".TTST";
                                Lbcd.Add(lstn);
                            }
                        }
                    }
                    if (Lbcd.Count > 0)
                    {
                        string fileContaint = System.IO.File.ReadAllText(file);
                        var matches = Regex.Matches(fileContaint, @"TEST_CASE\s*(.*?)\s*END_TEST_CASE", RegexOptions.Singleline);
                        StringBuilder updatedContent = new StringBuilder(fileContaint);

                        foreach (var t in matches)
                        {
                            string text = t.ToString();

                            var match = Regex.Match(t.ToString(), @"TEST_CASE\s+""([^""]+)""");
                            if (match.Success)
                            {
                                string extracted = match.Groups[1].Value;
                                var matchData = Regex.Match(extracted, @"(?:_Trans|_S|Transition)[ _]?(\d+(?:_T\d+)?)", RegexOptions.IgnoreCase);
                                if (matchData.Success)
                                {
                                    var matchDigit = matchData.Groups[1].Value;

                                    foreach (var line in Lbcd)
                                    {
                                        if (line.Contains("110,iAreaClrSensr_DnPosUPL"))
                                            ;
                                        var parts = line.Split(',');
                                        if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
                                        {
                                            if (parts[0] == matchDigit)
                                            {
                                                var matchesStep = Regex.Matches(text, @"STEP:\s*(.*?)\s*END_STEP", RegexOptions.Singleline);
                                                foreach (var st in matchesStep)
                                                {
                                                    string replace = st.ToString();
                                                    string stText = st.ToString();
                                                    if (stText.Contains(parts[1]))
                                                    {
                                                        string variablePattern = $@"\b{Regex.Escape(parts[1])}\b:=[^;\r\n]+;";
                                                        string replacementValue = parts[1] + ":=" + parts[2].Trim() + ";";

                                                        string noSpaces = Regex.Replace(stText, @"\s*:=\s*", ":=");
                                                        string updatedStepText = Regex.Replace(noSpaces, variablePattern, replacementValue);

                                                        string fileDataa = fileContaint;

                                                        if (fileContaint.Contains(stText))
                                                        {
                                                            //
                                                            text = text.Replace(replace, " @@@@");
                                                            text = text.Replace(" @@@@", updatedStepText);
                                                            //
                                                            fileDataa = fileDataa.Replace(replace, " @@@@");
                                                            fileDataa = fileDataa.Replace(" @@@@", updatedStepText);
                                                        }
                                                        fileContaint = fileDataa;
                                                        string upd = updatedContent.ToString();
                                                        string filename = Lbcd[0].Split(',')[2].Trim();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        //string outputPath = $@"C:\Test Suits\Input Tat\ModifiedTat\{fileName}.TTST";
                        string outputPath = $@"C:\Generate Block\V20 All Files\Test Suits\Input Tat\ModifiedTat\{fileName}.TTST";
                        //System.IO.File.WriteAllText(outputPath, fileContaint);
                        System.IO.File.WriteAllText(file, fileContaint);
                        int a = 0;
                    }
                }
            }
            else
            {
                fileData = System.IO.File.ReadAllLines(tatFIle[0]).ToList();
            }
            //System.IO.File.Copy(excelFile, @"C:\Test Suits\Report Gen\AutoGenXml\" + System.IO.Path.GetFileName(excelFile), true);
            System.IO.File.Copy(excelFile, @"C:\Generate Block\V20 All Files\Test Suits\Report Gen\AutoGenXml\" + System.IO.Path.GetFileName(excelFile), true);
        }
        catch (Exception ex)
        {
        }
    }
    static string[] ParseCsvLine(string line)
    {
        List<string> values = new List<string>();
        bool insideQuote = false;
        string currentValue = string.Empty;

        for (int i = 0; i < line.Length; i++)
        {
            char ch = line[i];

            if (ch == '"')
            {
                if (insideQuote && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentValue += '"';
                    i++;
                }
                else
                {
                    insideQuote = !insideQuote;
                }
            }
            else if (ch == ',' && !insideQuote)
            {
                values.Add(currentValue);
                currentValue = string.Empty;
            }
            else
            {
                currentValue += ch;
            }
        }
        values.Add(currentValue);
        return values.ToArray();
    }

    public static List<string> selectionOption(List<string> options, List<string> Inout, List<List<object>> TTST, Dictionary<string, string> previousSelections, List<List<string>> disc/*,string tatPath*/, List<string> tagAndRangeList)
    {
        List<List<string>> L1 = new List<List<string>>();
        options = options.Where(item => !item.ToLower().Contains("[false]")).ToList();

        List<string> reportData = options;
        List<string> selectedValues = new List<string>();
        List<string> newlist = tagAndRangeList;
        int screenWidth = Screen.PrimaryScreen.Bounds.Width;
        int screenHeight = Screen.PrimaryScreen.Bounds.Height;
        int countfortT = 0;

        Form form = new Form
        {
            Text = "Select Options",
            WindowState = FormWindowState.Maximized,
            FormBorderStyle = FormBorderStyle.None,
            StartPosition = FormStartPosition.CenterScreen
        };

        Panel titleBar = new Panel
        {
            Height = 20,
            Dock = DockStyle.Top,
            BackColor = System.Drawing.Color.Transparent
        };
        form.Controls.Add(titleBar);

        Button closeButton = new Button
        {
            Text = "X",
            ForeColor = System.Drawing.Color.White,
            BackColor = System.Drawing.Color.Red,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(20, 20),
            Dock = DockStyle.Right
        };
        string result = "None";
        closeButton.FlatAppearance.BorderSize = 0;
        closeButton.Click += (s, e) =>
        {
            result = "ClosedByUser";
            form.DialogResult = DialogResult.OK; 
            form.Close();
            options.Add(result);
        };
        titleBar.Controls.Add(closeButton);

        Button maximizeButton = new Button
        {
            Text = "🗖",
            ForeColor = System.Drawing.Color.White,
            BackColor = System.Drawing.Color.Gray,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(20, 20),
            Dock = DockStyle.Right
        };
        maximizeButton.FlatAppearance.BorderSize = 0;
        maximizeButton.Click += (s, e) =>
        {
            if (form.WindowState == FormWindowState.Maximized)
                form.WindowState = FormWindowState.Normal;
            else
                form.WindowState = FormWindowState.Maximized;
        };
        titleBar.Controls.Add(maximizeButton);

        Button minimizeButton = new Button
        {
            Text = "―",
            ForeColor = System.Drawing.Color.White,
            BackColor = System.Drawing.Color.Gray,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(20, 20),
            Dock = DockStyle.Right
        };
        minimizeButton.FlatAppearance.BorderSize = 0;
        minimizeButton.Click += (s, e) => form.WindowState = FormWindowState.Minimized;
        titleBar.Controls.Add(minimizeButton);
                
        bool dragging = false;
        Point dragCursor = Point.Empty;
        Point dragForm = Point.Empty;
        titleBar.MouseDown += (s, e) =>
        {
            dragging = true;
            dragCursor = System.Windows.Forms.Cursor.Position;
            dragForm = form.Location;
        };
        titleBar.MouseMove += (s, e) =>
        {
            if (dragging)
            {
                Point diff = Point.Subtract(System.Windows.Forms.Cursor.Position, new Size(dragCursor));
                form.Location = Point.Add(dragForm, new Size(diff));
            }
        };
        titleBar.MouseUp += (s, e) => dragging = false;
        //
        FlowLayoutPanel mainPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Width = screenWidth,
            Height = screenHeight
        };
        form.Controls.Add(mainPanel);

        FlowLayoutPanel dropdownPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            Padding = new Padding(10),
            Width = screenWidth / 2
        };
        mainPanel.Controls.Add(dropdownPanel);

        FlowLayoutPanel inputsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            Padding = new Padding(10),
            Width = screenWidth / 2
        };
        mainPanel.Controls.Add(inputsPanel);

        Dictionary<System.Windows.Forms.Control, string> inputControls = new Dictionary<System.Windows.Forms.Control, string>();
        void AddInputs(string label, List<string> items)
        {
            Label headerLabel = new Label
            {
                Text = label,
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold),
                Padding = new Padding(5)
            };
            inputsPanel.Controls.Add(headerLabel);

            TableLayoutPanel tableLayout = new TableLayoutPanel
            {
                ColumnCount = 3,
                AutoSize = true,
                Padding = new Padding(5),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            tableLayout.Controls.Add(new Label { Text = "Tag Name", AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold) }, 0, 0);
            tableLayout.Controls.Add(new Label { Text = "Selection Option", AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold) }, 1, 0);
            tableLayout.Controls.Add(new Label { Text = "Range Info", AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold) }, 2, 0);

            int row = 1;
            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item) || item.Contains("Block Name :")) continue;
                var tagMatch = Regex.Match(item, @"^([^,]+)");
                var bracketMatches = Regex.Matches(item, @"\[(.*?)\]");

                string tagName = tagMatch.Success ? tagMatch.Groups[1].Value.Trim() : "";
                string selection = bracketMatches.Count >= 1 ? bracketMatches[0].Groups[1].Value.Trim() : "";
                if (selection == "" && item.Contains("Bool"))
                {
                    selection = "FALSE";
                }
                string range = bracketMatches.Count >= 2 ? bracketMatches[1].Groups[1].Value.Trim() : "";
                var booleanValues = new HashSet<string>(new[] { "TRUE", "FALSE", "ON", "OFF", "Checked", "Unchecked" }, StringComparer.OrdinalIgnoreCase);
                bool isBoolean = booleanValues.Contains(selection);

                var tagLabel = new Label
                {
                    Text = tagName,
                    AutoSize = true,
                    Padding = new Padding(4)
                };
                tableLayout.Controls.Add(tagLabel, 0, row);

                if (isBoolean)
                {
                    bool isTrue = selection.Equals("TRUE", StringComparison.OrdinalIgnoreCase)
                               || selection.Equals("ON", StringComparison.OrdinalIgnoreCase)
                               || selection.Equals("Checked", StringComparison.OrdinalIgnoreCase);

                    Button toggleBtn = new Button
                    {
                        Text = "", 
                        BackColor = isTrue ? System.Drawing.Color.LightGreen : System.Drawing.Color.IndianRed,
                        Width = 30,
                        Tag = isTrue
                    };

                    toggleBtn.Click += (sender, e) =>
                    {
                        bool current = (bool)toggleBtn.Tag;
                        current = !current;
                        toggleBtn.Tag = current;
                        toggleBtn.BackColor = current ? System.Drawing.Color.LightGreen : System.Drawing.Color.IndianRed;
                        previousSelections[tagName] = current ? "Checked" : "Unchecked";
                    };

                    inputControls[toggleBtn] = tagName;
                    tableLayout.Controls.Add(toggleBtn, 1, row);
                }
                else
                {
                    var inputBox = new System.Windows.Forms.TextBox
                    {
                        Width = 100,
                        Text = selection
                    };
                    inputBox.TextChanged += (sender, e) =>
                    {
                        previousSelections[tagName] = inputBox.Text;
                    };
                    inputControls[inputBox] = tagName;
                    tableLayout.Controls.Add(inputBox, 1, row);
                }
                string cleanedRange = range.Replace("\"", "")
                                           .Replace("][", ",")
                                           .Replace("/", "")
                                           .Replace("[", "")
                                           .Replace("]", "")
                                           .Trim();

                var rangeBox = new System.Windows.Forms.TextBox
                {
                    Text = cleanedRange,
                    Width = 120,
                    ForeColor = System.Drawing.Color.DarkSlateGray
                };
                tableLayout.Controls.Add(rangeBox, 2, row);

                row++;
            }

            Button addButton = new Button
            {
                Text = "Add Value",
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(5)
            };
            addButton.Click += (sender, e) =>
            {
                try
                {
                    L1 = new List<List<string>>();
                    List<string> subList = new List<string>();
                    string newValue = "";
                    foreach (var t in inputControls)
                    {
                        if (t.Key.ToString().Contains("System.Windows.Forms.ComboBox"))
                        {
                            string val = t.Key.Text;

                            var match = System.Text.RegularExpressions.Regex.Match(val, @"(?:_Trans|Transition|_S)\s*(\d+(_T\d+)?)");
                            if (match.Success)
                            {
                                string selectedValue = match.Groups[1].Value;

                                subList.Add(val);
                                newValue = selectedValue;
                            }
                        }

                        foreach (var it in items)
                        {
                            if (it != null)
                            {
                                if (it.Split(',').Count() > 0 && (!it.Contains("Block Name")))
                                {
                                    if (it.Split(',')[0] == t.Value.Trim() && (t.Key.BackColor != System.Drawing.Color.IndianRed && t.Key.BackColor.Name != "Window"))
                                    {
                                        var itemParts = it.Split(',');
                                        var item = it.Trim();
                                        var parts = item.Split(',').Select(p => p.Trim()).ToList();

                                        var bracketMatches = Regex.Matches(item, @"\[(.*?)\]");

                                        if (bracketMatches.Count >= 2)
                                        {
                                            string tag = parts[0];
                                            string selection = bracketMatches[0].Groups[1].Value;
                                            string range = bracketMatches[1].Groups[1].Value;
                                            var rangeList = range.Split(',').Select(x => x.Trim()).ToList();
                                            if (rangeList.Contains(newValue.ToString())) continue;
                                            string updatedRange = AddValueToRange(range, newValue);

                                            string updatedItem = $"{tag},[{selection}],{updatedRange},Bool";
                                            items[items.IndexOf(it)] = updatedItem;

                                            for (int i = 0; i < tagAndRangeList.Count(); i++)
                                            {
                                                if (tagAndRangeList[i].Split(',')[0].Trim() == tag)
                                                {
                                                    tagAndRangeList[i] = tag + "," + updatedRange;
                                                }
                                            }

                                            subList.Add(updatedItem);
                                            break;
                                        }
                                        else
                                        {
                                            string tagName = itemParts[0].Trim();
                                            string type = itemParts.Length > 1 ? itemParts[1].Trim() : "Bool";
                                            string selection = "TRUE";  // Default boolean selection
                                            string initialRange = newValue.ToString();
                       
                                            string updatedItem = $"{tagName},[{selection}],[{initialRange}],{type}";

                                            int index = items.IndexOf(it);
                                            if (index != -1)
                                                items[index] = updatedItem;
                                            subList.Add(updatedItem);
                                            var t1 = tagAndRangeList;
                                            if (tagAndRangeList.Contains(tagName))
                                                ;
                                            else
                                                tagAndRangeList.Add(tagName + ",[" + initialRange + "]");
                                            break;
                                        }
                                    }
                                    else if (it.Split(',')[0] == t.Value.Trim() && (t.Key.BackColor == System.Drawing.Color.IndianRed && t.Key.BackColor.Name != "Window"))
                                    {
                                        if (tagAndRangeList.Any(entry => entry.StartsWith(it.Split(',')[0] + ",") && entry.Contains(newValue)))
                                        {
                                            var item = it.Trim();
                                            var parts = item.Split(',').Select(p => p.Trim()).ToList();
                                            var bracketMatches = Regex.Matches(item, @"\[(.*?)\]");
                                            string range = bracketMatches[1].Groups[1].Value;
                                            string tag = parts[0];
                                            string updatedItem = $"{tag},[{"False"}],[{newValue}],Bool";
                                            subList.Add(updatedItem);

                                            for (int i = 0; i < tagAndRangeList.Count; i++)
                                            {
                                                var item1 = tagAndRangeList[i].Trim();
                                                var parts1 = item1.Split(',').Select(p => p.Trim()).ToList();
                                                string range1 = bracketMatches[1].Groups[1].Value;
                                                string entry = tagAndRangeList[i];
                                                if (entry.StartsWith(item.Split(',')[0] + ",") && entry.Contains(newValue))
                                                {
                                                    var rangeList = range1.Split(',').Select(x => x.Trim()).ToList();
                                                    if (rangeList.Count() == 1)
                                                    {
                                                        tagAndRangeList.RemoveAt(i);
                                                        items[items.IndexOf(it)] = updatedItem;
                                                    }
                                                    else if (rangeList.Count() > 1)
                                                    {
                                                        rangeList.Remove(newValue.ToString());
                                                        string updatedRange = string.Join(",", rangeList);
                                                        string updatedItem1 = $"{tag},[{"True"}],[{updatedRange}],Bool";
                                                        tagAndRangeList[i] = updatedItem1;
                                                        items[items.IndexOf(it)] = updatedItem1;
                                                        items.Add(updatedItem);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (it.Split(',')[0] == t.Value.Trim() && !string.IsNullOrEmpty(t.Key.Text))
                                    {

                                    }
                                }
                            }
                        }
                    }
                    L1.Add(subList);
                }
                catch (Exception ex)
                {
                }
            };
            inputsPanel.Controls.Add(addButton);
            inputsPanel.Controls.Add(tableLayout);
        }

        string tatName = "";
        void AddDropdownListsFromTTST(string lable, List<List<object>> TTsT/*, List<string> tagAndRangeList*/)
        {
            int fixedLabelWidth = 200;
            System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
            List<System.Windows.Forms.ComboBox> allComboBoxes = new List<System.Windows.Forms.ComboBox>();

            foreach (var item in TTsT)
            {
                string header = item[0].ToString();
                List<string> data = item[1] as List<string>;
                if (data == null || data.Count == 0) continue;

                FlowLayoutPanel listPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.TopDown,
                    AutoSize = true,
                    Padding = new Padding(10),
                    Dock = DockStyle.Top
                };

                Label categoryLabel = new Label
                {
                    Text = header.Length > 15 ? header.Substring(0, 20) + "..." : header,
                    AutoSize = false,
                    Width = fixedLabelWidth,
                    Height = 25,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new System.Drawing.Font("Arial", 10, FontStyle.Bold),
                    ForeColor = System.Drawing.Color.Black,
                    AutoEllipsis = true
                };
                toolTip.SetToolTip(categoryLabel, header);
                listPanel.Controls.Add(categoryLabel);

                System.Windows.Forms.ComboBox comboBox = new System.Windows.Forms.ComboBox
                {
                    Width = 300,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };

                comboBox.Items.Add("None");
                comboBox.Items.AddRange(data.ToArray());

                if (previousSelections.ContainsKey(header))
                {
                    comboBox.SelectedItem = previousSelections[header];
                }
                else
                {
                    comboBox.SelectedIndex = 0;
                }

                allComboBoxes.Add(comboBox);

                comboBox.MouseHover += (sender, e) =>
                {
                    if (comboBox.SelectedItem != null)
                        toolTip.SetToolTip(comboBox, comboBox.SelectedItem.ToString());
                };
                comboBox.SelectedIndexChanged += (sender, e) =>
                {
                    if (comboBox.SelectedIndex <= 0) return;

                    foreach (var cb in allComboBoxes)
                    {
                        if (cb != comboBox) cb.Enabled = false;
                    }

                    string selectedItem = comboBox.SelectedItem.ToString();
                    var match = Regex.Match(selectedItem, @"(?:_Trans|Transition|_S)\s*(\d+(_T\d+)?)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    tatName = selectedItem;
                    List<string> foundList = new List<string>();

                    if (match.Success)
                    {
                        string selectedValue = match.Groups[1].Value;

                        foreach (string entry in tagAndRangeList)
                        {
                            string[] parts = entry.Split(new[] { ',' }, 2);
                            if (parts.Length == 2)
                            {
                                string tagName = parts[0].Trim();
                                string rangeText = parts[1].Trim();

                                var nums = rangeText
                                            .Trim('[', ']')
                                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(s1 => int.TryParse(s1.Trim(), out int num) ? num.ToString() : s1.Trim())
                                            .ToList();
                                bool exists = nums.Any(x => x == selectedValue.Trim());
                                if (exists)
                                {
                                    foundList.Add(tagName);
                                }
                            }
                        }

                        var notFoundList = foundList.Where(item1 => inputControls.ContainsValue(item1)).ToList();

                        foreach (var ti in inputControls)
                        {
                            if (!notFoundList.Contains(ti.Value) && (ti.Key.BackColor == System.Drawing.Color.LightGreen || ti.Key.BackColor == System.Drawing.Color.IndianRed))
                            {
                                ti.Key.BackColor = System.Drawing.Color.IndianRed;
                            }
                            else
                            {
                                if (ti.Key.BackColor == System.Drawing.Color.IndianRed && notFoundList.Contains(ti.Value))
                                {
                                    ti.Key.BackColor = System.Drawing.Color.LightGreen;
                                }
                            }
                        }
                    }
                };
                inputControls.Add(comboBox, header);
                listPanel.Controls.Add(comboBox);
                dropdownPanel.Controls.Add(listPanel);
            }
        }

        void AddListSections(List<List<string>> descriptions)
        {
            int fixedLabelWidth = 200;
            System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();

            FlowLayoutPanel listSectionPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Padding = new Padding(10),
                Dock = DockStyle.Right
            };

            System.Windows.Forms.ListBox listBox = new System.Windows.Forms.ListBox
            {
                Width = screenWidth / 6,
                Height = screenHeight / 2,
                ScrollAlwaysVisible = true
            };

            FlowLayoutPanel detailPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                Padding = new Padding(10),
                Width = screenWidth / 6,
            };
            //
            FlowLayoutPanel statusPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Padding = new Padding(10),
                BackColor = System.Drawing.Color.Black,
                Location = new Point(listBox.Bottom + 10)
            };

            mainPanel.Controls.Add(statusPanel);
            
            Dictionary<string, List<string>> itemMap = new Dictionary<string, List<string>>();
            HashSet<string> addedLinkedKeys = new HashSet<string>();
            foreach (var sublist in descriptions)
            {
                if (sublist.Count > 0)
                {
                    string key = sublist[0];
                    itemMap[key] = sublist.Skip(1).ToList();
                     
                    if (key.Contains("&"))
                    {
                        var parts = key.Split('&').Select(p => p.Trim()).ToList();
                        if (parts.Count > 1)
                        {
                            string linkedKey = parts[1];
                            if (!addedLinkedKeys.Contains(linkedKey))
                            {
                                listBox.Items.Add(key);
                                addedLinkedKeys.Add(linkedKey);
                            }
                        }
                    }
                    else
                    {
                        listBox.Items.Add(key);
                    }
                }
            }
            int a = 0;
            listBox.SelectedIndexChanged += (sender, e) =>
            {
                detailPanel.Controls.Clear();
                int testedOkCount = 0;
                int testedNotOkCount = 0;
                Label testedOkLabel = new Label { AutoSize = true, Font = new System.Drawing.Font("Arial", 9, FontStyle.Bold), ForeColor = System.Drawing.Color.Green };
                Label testedNotOkLabel = new Label { AutoSize = true, Font = new System.Drawing.Font("Arial", 9, FontStyle.Bold), ForeColor = System.Drawing.Color.Red };

                if (listBox.SelectedItem != null)
                {
                    string selectedKey = listBox.SelectedItem.ToString();

                    if (selectedKey.Contains("&"))
                    {
                        var keyParts = selectedKey.Split('&').Select(p => p.Trim()).ToList();

                        string mainLabel = keyParts[0];
                        string linkedKey = keyParts[1];
                        FlowLayoutPanel topDownPanel = new FlowLayoutPanel
                        {
                            FlowDirection = FlowDirection.TopDown,
                            AutoSize = true,
                            WrapContents = false
                        };
                        try
                        {
                            var matchingKeys = itemMap.Keys
                                .Where(k => k.Contains("&"))
                                .Where(k =>
                                {
                                    var parts = k.Split('&').Select(p => p.Trim()).ToList();
                                    return parts.Any(part => part.Equals(linkedKey));
                                })
                                .ToList();

                            string firstMatchingKey = matchingKeys.FirstOrDefault();
                            if (!string.IsNullOrEmpty(firstMatchingKey) && itemMap.ContainsKey(firstMatchingKey))
                            {
                                foreach (var matchingKey in matchingKeys)
                                {
                                    if (itemMap.ContainsKey(matchingKey))
                                    {
                                        topDownPanel.Controls.Add(CreateDetailTable(matchingKey, itemMap[matchingKey]));
                                        foreach (var item in itemMap[matchingKey])
                                        {
                                            if (item.Contains("Tested Ok")) testedOkCount++;
                                            else if (item.Contains("Tested Not Ok")) testedNotOkCount++;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("");
                        }
                        Panel scrollablePanel = new Panel
                        {
                            AutoScroll = true,
                            Width = screenWidth / 2,
                            Height = screenHeight - 200
                        };

                        scrollablePanel.Controls.Add(topDownPanel);
                        detailPanel.Controls.Add(scrollablePanel);
                    }
                    else
                    {
                        if (itemMap.ContainsKey(selectedKey))
                        {
                            Panel scrollablePanel = new Panel
                            {
                                AutoScroll = true,
                                Width = screenWidth / 2,
                                Height = screenHeight - 200
                            };
                            scrollablePanel.Controls.Add(CreateDetailTable(selectedKey, itemMap[selectedKey]));
                            detailPanel.Controls.Add(scrollablePanel);
                            int length = detailPanel.Height;
                            int breadth = detailPanel.Width;
                            statusPanel.ResetText();
                            int screenHeightdata = Screen.PrimaryScreen.Bounds.Height;
                            FlowLayoutPanel statusPanel1 = new FlowLayoutPanel
                            {
                                FlowDirection = FlowDirection.LeftToRight,
                                AutoSize = true,
                                Padding = new Padding(10),
                                BackColor = System.Drawing.Color.White,
                            };
                            foreach (var item in itemMap[selectedKey])
                            {
                                if (item.Contains("Tested Ok"))
                                {
                                    testedOkLabel.Text = item;
                                    statusPanel1.Controls.Add(testedOkLabel);
                                    string text = testedOkLabel.Text;
                                }
                                else if (item.Contains("Tested Not Ok"))
                                {
                                    testedNotOkLabel.Text = item;
                                    statusPanel1.Controls.Add(testedNotOkLabel);
                                    string text = testedOkLabel.Text;
                                }
                            }

                            string text1 = testedOkLabel.Text;

                            detailPanel.Controls.Add(statusPanel1);
                            scrollablePanel.Controls.Add(statusPanel1);
                        }
                    }
                }
            };
            listSectionPanel.Controls.Add(listBox);
            listSectionPanel.Controls.Add(detailPanel);
            mainPanel.Controls.Add(listSectionPanel); 
        }

        TableLayoutPanel CreateDetailTable(string title, List<string> items)
        {
            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                Padding = new Padding(5)
            };
            
            tableLayoutPanel.Controls.Add(new Label
            {
                Text = title,
                Font = new System.Drawing.Font("Arial", 9, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(3)
            }, 0, 0);
            tableLayoutPanel.SetColumnSpan(tableLayoutPanel.Controls[tableLayoutPanel.Controls.Count - 1], 2);

            int row = 1;
            foreach (var item in items)
            {
                string[] parts = item.Split(':');
                if (parts.Length == 2)
                {
                    string param = parts[0].Trim();
                    string value = parts[1].Trim();

                    Label paramLabel = new Label { Text = param, AutoSize = true, Padding = new Padding(3) };
                    Label valueLabel = new Label
                    {
                        Text = value,
                        AutoSize = true,
                        Padding = new Padding(3),
                        ForeColor = value.Contains("Tested Ok") ? System.Drawing.Color.Green :
                                     value.Contains("Tested Not Ok") ? System.Drawing.Color.Red :
                                     System.Drawing.Color.Black
                    };

                    tableLayoutPanel.Controls.Add(paramLabel, 0, row);
                    tableLayoutPanel.Controls.Add(valueLabel, 1, row);
                }
                else
                {
                    Label label = new Label
                    {
                        Text = item,
                        Font = new System.Drawing.Font("Arial", 8, FontStyle.Bold),
                        AutoSize = true,
                        Padding = new Padding(3),
                        ForeColor = item.Contains("Tested Ok") ? System.Drawing.Color.Green :
                                     item.Contains("Tested Not Ok") ? System.Drawing.Color.Red :
                                     System.Drawing.Color.Black
                    };
                    tableLayoutPanel.Controls.Add(label, 0, row);
                    tableLayoutPanel.SetColumnSpan(label, 2);
                }

                row++;
            }

            return tableLayoutPanel;
        }

        AddDropdownListsFromTTST("Select TAT", TTST/*,tagAndRangeList*/);
        AddInputs("Select Input:", options);
        //AddInputs("Select Inout:", Inout);
        AddListSections(disc);
        //DisplayDataWithInputs(Inout, mainPanel);
        Panel bottomPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 60
        };
        Button submitButton = new Button
        {
            Text = "Submit",
            Height = 50,
            Width = 150,
            Left = 10,
            Top = 5
        };
        List<string> formattedInputsList = new List<string>();

        submitButton.Click += (sender, e) =>
        {
            previousSelections.Clear();
            selectedValues.Clear(); 

            foreach (var control in inputControls)
            {
                string tagName = control.Value;
                string value = "";
                string dataType = "Bool";
                string range = "";
                if (control.Key is Panel statusPanel)
                {
                    value = statusPanel.BackColor == System.Drawing.Color.LightGreen ? "Checked" : "Unchecked";
                    selectedValues.Add("Tag," + tagName + " = " + value);
                    previousSelections[tagName] = value;
                }
                else if (control.Key is Button toggleBtn)
                {
                    bool isOn = (bool)toggleBtn.Tag;
                    value = isOn ? "TRUE" : "FALSE";
                    selectedValues.Add("Tag," + tagName + " = " + value);
                    previousSelections[tagName] = value;
                }
                else if (control.Key is System.Windows.Forms.TextBox textBox && !string.IsNullOrWhiteSpace(textBox.Text))
                {
                    value = textBox.Text.Trim();
                    selectedValues.Add("Tag," + tagName + " = " + value);
                    previousSelections[tagName] = value;
                }
                else if (control.Key is System.Windows.Forms.ComboBox comboBox && comboBox.SelectedItem != null)
                {
                    value = comboBox.SelectedItem.ToString();
                    selectedValues.Add("TTST," + tagName + " = " + value);
                    previousSelections[tagName] = value;
                    tatName = tagName;
                }
                System.Windows.Forms.TextBox rangeBox = FindRangeTextBox(control.Key);
                if (rangeBox != null && !string.IsNullOrWhiteSpace(rangeBox.Text))
                {
                    range = rangeBox.Text.Trim();
                }
           
                string formattedLine = $"{tagName}, [{value}], [{range}]/, {dataType}";
                formattedInputsList.Add(formattedLine);
            }

            CopyMatchingTatFile(tatName);
            form.DialogResult = DialogResult.OK;
            form.Close();
        };

        Button submitButtonSingleTAT = new Button
        {
            Text = "Single Function Run",
            Height = 50,
            Width = 180,
            Left = submitButton.Right + 20,
            Top = 5
        };

        submitButtonSingleTAT.Click += (sender, e) =>
        {
            previousSelections.Clear();
            selectedValues.Clear();
            string filename = "";

            foreach (var control in inputControls)
            {
                string tagName = control.Value;
                string value = "";
                string dataType = "Bool";
                string range = "";
                if (control.Key is Panel statusPanel)
                {
                    value = statusPanel.BackColor == System.Drawing.Color.LightGreen ? "Checked" : "Unchecked";
                    selectedValues.Add("Tag," + tagName + " = " + value);
                    previousSelections[tagName] = value;
                }
                else if (control.Key is Button toggleBtn)
                {
                    bool isOn = (bool)toggleBtn.Tag;
                    value = isOn ? "TRUE" : "FALSE";
                    selectedValues.Add("Tag," + tagName + " = " + value);
                    previousSelections[tagName] = value;
                }
                else if (control.Key is System.Windows.Forms.TextBox textBox && !string.IsNullOrWhiteSpace(textBox.Text))
                {
                    value = textBox.Text.Trim();
                    selectedValues.Add("Tag," + tagName + " = " + value);
                    previousSelections[tagName] = value;
                }
                else if (control.Key is System.Windows.Forms.ComboBox comboBox && comboBox.SelectedItem != null)
                {
                    value = comboBox.SelectedItem.ToString();
                    selectedValues.Add("TTST," + tagName + " = " + value);
                    previousSelections[tagName] = value;
                    filename = tagName;
                    tatName = value;
                }

                System.Windows.Forms.TextBox rangeBox = FindRangeTextBox(control.Key);
                if (rangeBox != null && !string.IsNullOrWhiteSpace(rangeBox.Text))
                {
                    range = rangeBox.Text.Trim();
                }
          
                string formattedLine = $"{tagName}, [{value}], [{range}]/, {dataType}";
                formattedInputsList.Add(formattedLine);
            }
            CopyMatchingTatFile(filename);
            singletatUpdate(tatName, filename);
            form.DialogResult = DialogResult.OK;
            form.Close();
        };

        Button generateReportButton = new Button
        {
            Text = "Generate Report",
            Dock = DockStyle.Bottom,
            Height = 50,
            Width = 150,
            Enabled = disc.Count > 0
        };
        generateReportButton.Click += (sender, e) =>
        {
            generateReport(disc, options);
        };
        form.Controls.Add(generateReportButton);
        bottomPanel.Controls.Add(submitButton);
        bottomPanel.Controls.Add(submitButtonSingleTAT);
        form.Controls.Add(bottomPanel);
        form.ShowDialog();
        
        return options;
    }
    public static  System.Windows.Forms.TextBox FindRangeTextBox(System.Windows.Forms.Control inputControl)
    {
        var parent = inputControl.Parent as TableLayoutPanel;
        if (parent == null) return null;
        
        int row = parent.GetRow(inputControl);
        
        foreach (System.Windows.Forms.Control ctrl in parent.Controls)
        {
            if (parent.GetRow(ctrl) == row && parent.GetColumn(ctrl) == 2 && ctrl is System.Windows.Forms.TextBox txt)
            {
                return txt;
            }
        }
        return null;
    }
    public static void generateReport(List<List<string>> Disc, List<string> previousSelections)
    {
        List<List<string>> Discription = Disc;
        for (int i = 0; i < previousSelections.Count; i++)
        {
            string cleaned = Regex.Replace(previousSelections[i], "(\\\\*\")+", "\"");
            previousSelections[i] = cleaned;
        }
        FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
        folderBrowserDialog.Description = "Select a folder to save the report";
        List<string> previousSelectionsItem = new List<string>();
        previousSelectionsItem = previousSelections;
        List<List<string>> resulttrace = new List<List<string>>();
        List<List<string>> resultoptag = new List<List<string>>();
        foreach (var innerList in Discription)
        {
            if (innerList.Any(item => item.IndexOf("State traces Flow", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                resulttrace.Add(innerList);   
            }
            else if (innerList.Any(item => item.IndexOf("Output tag Flow", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                resultoptag.Add(innerList);
            }
        }
        List<List<string>> combinedResult = new List<List<string>>();
        List<List<string>> filtered = resulttrace
            .Where(sublist => sublist.Count > 0 && sublist[0].Contains("&"))
.ToList();
        if (filtered.Count > 0)
        {
            var grouped = new Dictionary<string, List<List<string>>>();
            foreach (var sublist in filtered)
            {
                if (sublist.Count == 0)
                    continue;

                var match = Regex.Match(sublist[0], @"&(\d+)");
                if (!match.Success)
                    continue;

                string key = match.Groups[1].Value;

                if (!grouped.ContainsKey(key))
                    grouped[key] = new List<List<string>>();

                grouped[key].Add(sublist);
            }

            var result = new List<List<string>>();
            foreach (var kvp in grouped)
            {
                string key = kvp.Key;
                var sublists = kvp.Value;
                int maxLength = sublists.Max(l => l.Count);

                var combined = new List<string>();
                for (int i = 0; i < maxLength; i++)
                {
                    var items = sublists
                        .Where(l => l.Count > i)
                        .Select(l => l[i])
                        .ToList();

                    combined.Add(string.Join(",", items));
                }
                result.Add(combined);
            }
            resulttrace.RemoveAll(sublist => sublist.Count > 0 && sublist[0].Contains("&"));
            foreach (var tn in result)
                resulttrace.Add(tn);
        }
        foreach (var result in resulttrace)
        {
            for (int temp = 0; temp < result.Count; temp++)
            {
                if (result[temp].Contains("["))
                {
                    result[temp] = result[temp].Replace("[", "\"[")
                        .Replace("]", "]\"");
                }
            }
        }
        int outerCount = Math.Max(resulttrace.Count, resultoptag.Count);
        
        for (int i = 0; i < outerCount; i++)
        {
            List<string> traceList = i < resulttrace.Count ? resulttrace[i] : new List<string>();
            List<string> optagList = i < resultoptag.Count ? resultoptag[i] : new List<string>();
            
            int innerCount = Math.Max(traceList.Count, optagList.Count);
            List<string> combinedInner = new List<string>();
            
            for (int j = 0; j < innerCount; j++)
            {
                string traceVal = j < traceList.Count ? traceList[j] : "";
                string optagVal = j < optagList.Count ? optagList[j] : "";

                combinedInner.Add($"{traceVal}, {optagVal}");
            }
            combinedResult.Add(combinedInner);
        }
        for (int i = 0; i < previousSelectionsItem.Count; i++)
        {
            previousSelectionsItem[i] = previousSelectionsItem[i].Replace(",Bool", "");
            previousSelectionsItem[i] = previousSelectionsItem[i].Replace(",Real", "");
            previousSelectionsItem[i] = previousSelectionsItem[i].Replace(",Int", "");
        }

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"TestCase_Report_{timestamp}.csv";
        //string fullPath = Path.Combine("C:\\Users\\Wpuser\\Downloads\\", fileName);
        string fullPath = Path.Combine("C:\\Generate Block\\V20 All Files\\Downloads", fileName);

        List<string> lines = new List<string>();

        List<string> flatList = combinedResult.SelectMany(innerList => innerList).ToList();
        int countA = previousSelectionsItem.Count;
        int countB = flatList.Count;
        int maxCount = Math.Max(countA, countB);

        for (int i = 0; i < maxCount; i++)
        {
            if (i < countA && i < countB)
            {
                lines.Add($"{previousSelectionsItem[i]}, {flatList[i]}");
            }
            else if (i < countA)
            {
                lines.Add(previousSelectionsItem[i]);
            }
            else if (i < countB)
            {
                lines.Add("," + flatList[i]);
            }
        }
        using (StreamWriter writer = new StreamWriter(fullPath))
        {
            foreach (var line in previousSelections)
            {
                writer.WriteLine(line);
            }
        }

        List<string> simpleList = combinedResult.SelectMany(innerList => innerList).ToList();

        List<string> updatedLines = new List<string>();
        using (var reader = new StreamReader(fullPath))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                updatedLines.Add(line);
            }
        }

        for (int i = 0; i < simpleList.Count; i++)
        {
            if (updatedLines.Count > i + 1)
            {
                if (!updatedLines[i + 1].Contains(','))
                {
                    updatedLines[i + 1] = updatedLines[i + 1] + ",,";
                }

                updatedLines[i + 1] += "," + simpleList[i];
            }
            else
            {
                updatedLines.Add(",,," + simpleList[i]);
            }
        }

        using (var writer = new StreamWriter(fullPath, false))
        {
            foreach (var line in updatedLines)
            {
                writer.WriteLine(line);
            }
        }
    }
    public static void CopyMatchingTatFile(string tatName)
    {
        ////////string sourceDir = @"C:\Users\Wpuser\Downloads\Final\Generate Block\Testing\TTST";
        ////string sourceDir = @"C:\Generate Block\V20 All Files\Testing\TTST";
        //////string targetDir = @"C:\Users\Wpuser\Downloads\Final\Generate Block\Testing\TTST\Updated TTST"; 
        ////string targetDir = @"C:\Generate Block\V20 All Files\Testing\TTST\Updated TTST";

        ////string[] files = Directory.GetFiles(sourceDir, "*.TTST");
        ////foreach (string file in files)
        ////{
        ////    string name = Path.GetFileNameWithoutExtension(file);

        ////    if (name.Trim() == tatName.Trim())
        ////    {   
        ////        string filename = Path.GetFileName(file);
        ////        string destinationPath = Path.Combine(targetDir, filename);
        ////        System.IO.File.Copy(file, destinationPath, overwrite: true);

        ////        break;
        ////    }
        ////}
    }
    public static void singletatUpdate(string tatName, string fileName)
    {
        string[] ttst = Directory.GetFiles(@"C:\Test Suits\Input Tat\UpdatedTTST", "*.TTST");
        if (ttst.Length == 0) return;
        string filePath = "";
        foreach (var t in ttst)
        {
            if (Path.GetFileNameWithoutExtension(t).Trim() == fileName.Trim())
            {
                filePath = t;
            }
        }
        string input = System.IO.File.ReadAllText(filePath);
        string pattern = @"SEQUENCER\s*(.*?)\s*END_SEQUENCER";
        var matches = Regex.Matches(input, pattern, RegexOptions.Singleline);

        bool updated = false;
        int matchIndex = 0;
        
        foreach (Match match in matches)
        {
            string stepBlock = match.Groups[0].Value;
            string stepContent = match.Groups[1].Value;

            string[] lines = stepContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> processedLines = new List<string>();
            bool tatFound = false;

            foreach (var rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("\"") && line.EndsWith("();"))
                {
                    if (!tatFound && line.Trim() == tatName.Trim())
                    {
                        tatFound = true;
                        processedLines.Add(line);
                    }
                    else if (tatFound)
                    {
                        processedLines.Add("//" + line);
                    }
                    else
                    {
                        if (line.Contains("\"Memory_Reset\"();") || line.Contains("\"MemoryReset\"();"))
                            processedLines.Add(line);
                    }
                }
                else
                {
                    processedLines.Add(line);
                }
            }
            if (tatFound)
            {
                string newSequencerBlock = "SEQUENCER\n" + string.Join("\n", processedLines) + "\nEND_SEQUENCER";
                input = input.Remove(match.Index, match.Length);
                input = input.Insert(match.Index, newSequencerBlock);
                updated = true;
                break;
            }
            matchIndex++;
        }
        if (updated)
        {
            System.IO.File.WriteAllText(filePath, input);
        }
    }
    static string AddValueToRange(string range, string newValue)
    {
        var rangeValues = range.Trim('[', ']').Split(',');

        var rangeList = new List<string>();
        foreach (var value in rangeValues)
        {
            rangeList.Add(value.Trim());
        }

        if (!rangeList.Contains(newValue.Trim()))
        {
            rangeList.Add(newValue.Trim());
        }

        return "[" + string.Join(",", rangeList) + "]";
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

//public class BlockSelectionForm : Form
//{
//    private CheckedListBox checkedListBox;
//    private Button btnOk;
//    private Button btnCancel;
//    private System.Windows.Forms.CheckBox chkSelectAll;
//    public List<string> SelectedBlocks { get; private set; } = new List<string>();
//    public BlockSelectionForm(List<string> blockNames)
//    {
//        this.Text = "Select Blocks to Import";
//        this.Width = 600;
//        this.Height = 550;
//        this.StartPosition = FormStartPosition.CenterScreen;
//        this.Font = new Font("Segoe UI", 10);
//        this.FormBorderStyle = FormBorderStyle.FixedDialog;
//        this.MaximizeBox = false;
//        this.MinimizeBox = false;

//        chkSelectAll = new System.Windows.Forms.CheckBox()
//        {
//            Text = "Select All / Deselect All",
//            AutoSize = true,
//            Font = new Font("Segoe UI", 10, FontStyle.Bold),
//            Padding = new Padding(5),
//            Location = new Point(10, 10)
//        };
//        chkSelectAll.CheckedChanged += ChkSelectAll_CheckedChanged;

//        checkedListBox = new CheckedListBox()
//        {
//            CheckOnClick = true,
//            HorizontalScrollbar = true,
//            BorderStyle = BorderStyle.FixedSingle,
//            Location = new Point(10, 45),
//            Size = new Size(560, 370)
//        };

//        foreach (var name in blockNames)
//        {
//            checkedListBox.Items.Add(name);
//        }

//        btnOk = new Button()
//        {
//            Text = "OK",
//            BackColor = Color.MediumSeaGreen,
//            ForeColor = Color.White,
//            FlatStyle = FlatStyle.Flat,
//            Width = 100,
//            Height = 35
//        };
//        btnOk.FlatAppearance.BorderSize = 0;
//        btnOk.Click += BtnOk_Click;

//        btnCancel = new Button()
//        {
//            Text = "Cancel",
//            BackColor = Color.IndianRed,
//            ForeColor = Color.White,
//            FlatStyle = FlatStyle.Flat,
//            Width = 100,
//            Height = 35
//        };
//        btnCancel.FlatAppearance.BorderSize = 0;
//        btnCancel.Click += (s, e) =>
//        {
//            this.DialogResult = DialogResult.Cancel;
//            this.Close();
//        };

//        FlowLayoutPanel buttonPanel = new FlowLayoutPanel()
//        {
//            FlowDirection = FlowDirection.RightToLeft,
//            Dock = DockStyle.Bottom,
//            Height = 60,
//            Padding = new Padding(10),
//            BackColor = Color.WhiteSmoke
//        };
//        buttonPanel.Controls.Add(btnOk);
//        buttonPanel.Controls.Add(btnCancel);

//        Panel contentPanel = new Panel()
//        {
//            Dock = DockStyle.Fill,
//            Padding = new Padding(10)
//        };
//        contentPanel.Controls.Add(chkSelectAll);
//        contentPanel.Controls.Add(checkedListBox);

//        this.Controls.Add(contentPanel);
//        this.Controls.Add(buttonPanel);
//    }
//    private void ChkSelectAll_CheckedChanged(object sender, EventArgs e)
//    {
//        bool checkAll = chkSelectAll.Checked;
//        for (int i = 0; i < checkedListBox.Items.Count; i++)
//        {
//            checkedListBox.SetItemChecked(i, checkAll);
//        }

//        chkSelectAll.Text = checkAll ? "Deselect All" : "☑ Select All / Deselect All";
//    }
//    private void BtnOk_Click(object sender, EventArgs e)
//    {
//        SelectedBlocks.Clear();
//        foreach (var item in checkedListBox.CheckedItems)
//        {
//            SelectedBlocks.Add(item.ToString());
//        }

//        if (SelectedBlocks.Count == 0)
//        {
//            MessageBox.Show("Please select at least one block.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
//            return;
//        }

//        this.DialogResult = DialogResult.OK;
//        this.Close();
//    }
//}
