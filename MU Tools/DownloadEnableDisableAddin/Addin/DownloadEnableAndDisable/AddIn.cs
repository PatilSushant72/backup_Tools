using Siemens.Engineering;
using Siemens.Engineering.AddIn.Menu;
using System.Linq;
using System.Windows.Forms;
using Siemens.Engineering.SW.Blocks;
using System;
using System.Diagnostics;
using Siemens.Engineering.HW;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NetworkInterface = System.Net.NetworkInformation.NetworkInterface;
using System.Text;
using System.Drawing;

namespace DownloadEnableAndDisable
{
    /// <summary>
    /// This class contains your AddIn.
    /// Its providing entries for the context menu in TIA Portal
    /// Your AddIn can contain sub menus and one or multiple functions a user can call
    /// </summary>
    public class AddIn : ContextMenuAddIn
    {
        //Private variable referencing the TIA Portal object to get additional information for your add-in if necessary
        private readonly TiaPortal _tiaPortal;

        /// <summary>
        /// The string pasted to the base constructor defines the Label of the context menu entry on root level
        /// </summary>
        /// <param name="tiaPortal"></param>
        public AddIn(TiaPortal tiaPortal) : base("Download Enable And Disable")
        {
            _tiaPortal = tiaPortal;
            //Its recommended to keep the contructor clean. TIA Portal has a very short timeout for loading add-ins.
        }


        /// <summary>
        /// In this method you can define the menu entries with the methods to be called.
        /// The hierarchie of the AddIn with sub menus can be defined.
        /// </summary>
        /// <param name="addInRootSubmenu">Add-In are in the context menu</param>
        /// 


        protected override void BuildContextMenuItems(ContextMenuAddInRoot addInRootSubmenu)
        {
            //addInRootSubmenu.Items.AddActionItem<PlcBlock>("Selection info", methodToExecuteOnClick, DisplayStatus);
            //                                      |             |                     |                  |-> Optional: Method to update state of entry e.g. disable entry, set state of checkbox, ...
            //                                      |             |                     |
            //                                      |             |                     |-> Method which will be executed when the user clicks on the context entry
            //                                      |             |
            //                                      |             |-> Label which will be shown in the context menu
            //                                      |
            //                                      |-> Openness-type where the item should be shown

            addInRootSubmenu.Items.AddActionItem<IEngineeringObject>("Download Enable", runProcessOnClickEnable);
            addInRootSubmenu.Items.AddActionItem<IEngineeringObject>("Download Disable", runProcessOnClickDisable);

            //Examples for Submenu and other entry types
            //Submenu settingsSubmenu = addInRootSubmenu.Items.AddSubmenu("Settings");
            //settingsSubmenu.Items.AddActionItemWithCheckBox<IEngineeringObject>("Check Box", MethodToTriggerOnClick, MethodSetAndEvaluateCheckboxState);
            //settingsSubmenu.Items.AddActionItemWithRadioButton<IEngineeringObject>("Radio Button 1", MethodToTriggerOnClick, MethodSetAndEvaluateRadioButtonState);
            //settingsSubmenu.Items.AddActionItemWithRadioButton<IEngineeringObject>("Radio Button 2", MethodToTriggerOnClick, MethodSetAndEvaluateRadioButtonState);
        }
        //protected override void BuildContextMenuItems(ContextMenuAddInRoot addInRootSubmenu)
        //{
        //    addInRootSubmenu.Items.AddActionItem<IEngineeringObject>(
        //        "Grant Access",
        //        runProcessOnClick,
        //        GrantAccessMenuStatus
        //    );
        //}
        //    private MenuStatus GrantAccessMenuStatus(
        //MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
        //    {
        //        if (IsGrantAccessAlreadyDone())
        //            return MenuStatus.Disabled;

        //        return MenuStatus.Enabled;
        //    }
        private static readonly string FlagFilePath =
    Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GrantDownloadRights",
        "grant_access_done.flag"
    );

        private static bool IsGrantAccessAlreadyDone()
        {
            return File.Exists(FlagFilePath);
        }

        private static void MarkGrantAccessAsDone()
        {
            var dir = Path.GetDirectoryName(FlagFilePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(FlagFilePath, DateTime.Now.ToString());
        }


        /// <summary>
        /// Click delegate for a menu entry with your specified target type
        /// This method contains your actual AddIn logic that is executed
        /// </summary>
        /// <param name="menuSelectionProvider">All selected elements</param>
        private void methodToExecuteOnClick(MenuSelectionProvider<PlcBlock> menuSelectionProvider)
        {
            var selectedBlocks1 = menuSelectionProvider.GetSelection<DeviceItem>();
            var selectedBlocks = menuSelectionProvider.GetSelection<PlcBlock>();
            Util.ShowBlockInfo(selectedBlocks.First());
        }
        private bool IsConsoleAppRunning()
        {
            // Do NOT include .exe in the process name
            return Process.GetProcessesByName("GrantAccessEXE").Any();
        }
        /// <summary>
        /// This Method is setting the state of a menu entry when the menu is displayed.
        /// Possible values for MenuStatus are:
        /// - Enabled
        /// - Disabled
        /// - Hidden
        /// </summary>
        /// <param name="menuSelectionProvider">All selected elements</param>
        /// <returns></returns>
        private MenuStatus DisplayStatus(MenuSelectionProvider<PlcBlock> menuSelectionProvider)
        {
            return MenuStatus.Enabled;
        }
        private MenuStatus GrantAccessMenuStatus(
    MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
        {
            if (IsConsoleAppRunning())
                return MenuStatus.Disabled;

            return MenuStatus.Enabled;
        }
        /// <summary>
        /// Demo to show trigger of an external process.
        /// This menu entry is attached to all selectable objects in the scope of the AddInProvider since the target type is IEngineeringObject 
        /// </summary>
        /// <param name="menuSelectionProvider">Selected elements</param>

        private void runProcessOnClickEnable(MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
        {
            //if (IsGrantAccessAlreadyDone())
            //{
            //    MessageBox.Show(
            //        "Grant Access has already been executed.",
            //        "Information",
            //        MessageBoxButtons.OK,
            //        MessageBoxIcon.Information);
            //    return;
            //}
            //Util.RunExternalProcess1("");
            string tiaProjectName = _tiaPortal.Projects?.FirstOrDefault()?.Name ?? "Unknown Project";
            
            string tiaProjectPath = _tiaPortal.Projects?.FirstOrDefault()?.Path?.FullName ?? "";


            if(string.IsNullOrEmpty(tiaProjectPath))
            {
                var serverproj = _tiaPortal.LocalSessions.FirstOrDefault();
                var version = serverproj.Project.Version;
                //MessageBox.Show(serverproj.Project.Path.FullName);
                string fullName = serverproj.Project.Path.FullName;
                string version1 = Path.GetExtension(fullName);
                //MessageBox.Show(version1);
                string passversion = "";
                if (version1 == ".amc20")
                {
                    passversion = "20";
                }
                else if (version1 == ".amc19")
                {
                    passversion = "19";
                }
                else if (version1 == ".amc18")
                {
                    passversion = "18";
                }
                //MessageBox.Show(tiaProjectPath);
                var projectPath = _tiaPortal.GetCurrentProcess().ProjectPath;
                //MessageBox.Show(tiaProjectPath);
                //MessageBox.Show(tiaProjectName);
                //MessageBox.Show(projectPath.FullName);
                var selectedElement = menuSelectionProvider.GetSelection<IEngineeringObject>().First();
                //MessageBox.Show();
                var data = selectedElement.GetAttribute("Name").ToString();
                //MessageBox.Show(data);
                //MessageBox.Show("1");
                //MessageBox.Show(tiaProjectName);
            
                Util.RunExternalProcess(data,_tiaPortal,menuSelectionProvider, projectPath.FullName, passversion);
            }
        }
        private void runProcessOnClickDisable(MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
        {
            string tiaProjectName = _tiaPortal.Projects?.FirstOrDefault()?.Name ?? "Unknown Project";

            string tiaProjectPath = _tiaPortal.Projects?.FirstOrDefault()?.Path?.FullName ?? "";
            var projectPath = _tiaPortal.GetCurrentProcess().ProjectPath;
            //MessageBox.Show(tiaProjectPath);
            //MessageBox.Show(tiaProjectName);
            //MessageBox.Show(projectPath.FullName);
            var selectedElement = menuSelectionProvider.GetSelection<IEngineeringObject>().First();
            //MessageBox.Show();
            var data = selectedElement.GetAttribute("Name").ToString();
            //MessageBox.Show(data);
            //MessageBox.Show("1");
            //MessageBox.Show(tiaProjectName);
            Util.downloadDisable(data, _tiaPortal, menuSelectionProvider, projectPath.FullName);
        }
    }
}
