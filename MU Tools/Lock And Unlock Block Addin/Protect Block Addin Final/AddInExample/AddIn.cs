//// This response combines and finalizes the structure you're building for dynamic submenu control
//// using ApplicationKey and ValidationKey architecture
//// No manual paths, and all 6 submenus are permission-controlled.

//// Required Steps:
//// 1. Admin PC uses `ValidationManager` tool to generate a JSON (permitted menus)
//// 2. This JSON (encrypted or plain) is transferred to client PC
//// 3. Launcher installer reads JSON -> unlocks menus accordingly
//// 4. `AddIn.cs` filters visible menu items based on permissions

//// Assumptions:
//// - Permission file: `permissions.json`
//// - Stored in `%AppData%\TIAValidator\permissions.json`
//// - Contains structure:
////   {
////      "validationKey": "XXXX-XXXX-XXXX",
////      "permissions": ["EXCEL", "AWL", "XML"]
////   }

//// Now we structure all pieces below:

//// ========================== AddIn.cs ==============================
//using Siemens.Engineering;
//using Siemens.Engineering.AddIn.Menu;
//using System.Linq;
//using System.Windows.Forms;
//using Siemens.Engineering.SW.Blocks;
//using System;
//using System.Diagnostics;
//using System.Collections.Generic;

//namespace AddInExample
//{
//    public class AddIn : ContextMenuAddIn
//    {
//        private readonly TiaPortal _tiaPortal;
//        private readonly HashSet<string> _permissions;

//        public AddIn(TiaPortal tiaPortal) : base("Generate Block")
//        {
//            _tiaPortal = tiaPortal;
//            _permissions = Util.LoadPermissions(); // Reads from JSON in admin-controlled folder
//        }

//        protected override void BuildContextMenuItems(ContextMenuAddInRoot addInRootSubmenu)
//        {
//            // Always available block info
//            addInRootSubmenu.Items.AddActionItem<PlcBlock>(
//                "Selection info",
//                new ActionItem<PlcBlock>.OnClickDelegate(methodToExecuteOnClick),
//                DisplayStatus);

//            // Conditionally visible menu items
//            AddMenuItem(addInRootSubmenu, "EXCEL", runExcelProcess);
//            AddMenuItem(addInRootSubmenu, "AWL", runAwlProcess);
//            AddMenuItem(addInRootSubmenu, "XML", runXmlProcess);
//            AddMenuItem(addInRootSubmenu, "EXCELWITHTEST", runExcelWithTestProcess);
//            AddMenuItem(addInRootSubmenu, "AWLWITHTEST", runAwlWithTestProcess);
//            AddMenuItem(addInRootSubmenu, "XMLWITHTEST", runXmlWithTestProcess);
//        }

//        private void AddMenuItem(ContextMenuAddInRoot root, string title, Action<MenuSelectionProvider<IEngineeringObject>> callback)
//        {
//            if (_permissions.Contains(title))
//            {
//                root.Items.AddActionItem<IEngineeringObject>(
//                    title,
//                    new ActionItem<IEngineeringObject>.OnClickDelegate(callback));
//            }
//        }

//        private void methodToExecuteOnClick(MenuSelectionProvider<PlcBlock> menuSelectionProvider)
//        {
//            var selectedBlocks = menuSelectionProvider.GetSelection<PlcBlock>();
//            if (selectedBlocks.Any())
//            {
//                Util.ShowBlockInfo(selectedBlocks.First());
//            }
//            else
//            {
//                MessageBox.Show("No block selected.");
//            }
//        }

//        private MenuStatus DisplayStatus(MenuSelectionProvider<PlcBlock> menuSelectionProvider) => MenuStatus.Enabled;

//        // Submenu runners
//        private void runExcelProcess(MenuSelectionProvider<IEngineeringObject> sel) => Run("EXCEL", sel);
//        private void runAwlProcess(MenuSelectionProvider<IEngineeringObject> sel) => Run("AWL", sel);
//        private void runXmlProcess(MenuSelectionProvider<IEngineeringObject> sel) => Run("XML", sel);
//        private void runExcelWithTestProcess(MenuSelectionProvider<IEngineeringObject> sel) => Run("EXCELWITHTEST", sel);
//        private void runAwlWithTestProcess(MenuSelectionProvider<IEngineeringObject> sel) => Run("AWLWITHTEST", sel);
//        private void runXmlWithTestProcess(MenuSelectionProvider<IEngineeringObject> sel) => Run("XMLWITHTEST", sel);

//        private void Run(string type, MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
//        {
//            var tiaProcess = _tiaPortal?.GetCurrentProcess();
//            if (tiaProcess?.ProjectPath != null)
//            {
//                string projectPath = tiaProcess.ProjectPath.FullName;
//                Util.RunExternalProcess(type, projectPath);
//            }
//            else
//            {
//                MessageBox.Show("Could not retrieve TIA project path.");
//            }
//        }
//    }
//}







using Siemens.Engineering;
using Siemens.Engineering.AddIn.Menu;
using System.Linq;
using System.Windows.Forms;
using Siemens.Engineering.SW.Blocks;
using System;
using System.Diagnostics;
using System.IO;

namespace LockUnlockBlock
{
    public class AddIn : ContextMenuAddIn
    {
        private readonly TiaPortal _tiaPortal;

        public AddIn(TiaPortal tiaPortal) : base("LOCK AND UNLOCK")
        {
            _tiaPortal = tiaPortal;
            //Its recommended to keep the constructor clean. TIA Portal has a very short timeout for loading add-ins.
        }

        protected override void BuildContextMenuItems(ContextMenuAddInRoot addInRootSubmenu)
        {
            //addInRootSubmenu.Items.AddActionItem<PlcBlock>("Selection info", methodToExecuteOnClick, DisplayStatus);

            // ✅ Replaced old "Start process" with: For EXCEL
            addInRootSubmenu.Items.AddActionItem<IEngineeringObject>("LOCK", runProtectProcess);
            addInRootSubmenu.Items.AddActionItem<IEngineeringObject>("UNLOCK", rununprotectlProcess);
            addInRootSubmenu.Items.AddActionItem<IEngineeringObject>("PROTECT_EXISTING", rununprotectagainlProcess);
            //addInRootSubmenu.Items.AddActionItem<IEngineeringObject>("ProtectExisting", runexistingprotectlProcess);
            // ✅ Added new items: For AWL and For XML
            //addInRootSubmenu.Items.AddActionItem<IEngineeringObject>("AWL", runAwlProcess);
            //addInRootSubmenu.Items.AddActionItem<IEngineeringObject>("XML", runXmlProcess);
            //addInRootSubmenu.Items.AddActionItem<IEngineeringObject>("EXCEL WITH TEST", runExcelWithTestProcess);
            //addInRootSubmenu.Items.AddActionItem<IEngineeringObject>("AWL WITH TEST", runAwlWithTestProcess);
            //addInRootSubmenu.Items.AddActionItem<IEngineeringObject>("XML WITH TEST", runXmlWithTestProcess);


            // 🔴 Previous code for "Start process" is commented but retained for reference
            /*
            addInRootSubmenu.Items.AddActionItem<IEngineeringObject>("Start process", runProcessOnClick);
            */
        }

        private void methodToExecuteOnClick(MenuSelectionProvider<PlcBlock> menuSelectionProvider)
        {
            var selectedBlocks = menuSelectionProvider.GetSelection<PlcBlock>();
            Util.ShowBlockInfo(selectedBlocks.First());
        }

        private MenuStatus DisplayStatus(MenuSelectionProvider<PlcBlock> menuSelectionProvider)
        {
            return MenuStatus.Enabled;
        }

        // 🔴 Old process logic (commented)
        /*
        private void runProcessOnClick(MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
        {
            var selectedElement = menuSelectionProvider.GetSelection<IEngineeringObject>().First();
            var projectPath = _tiaPortal.GetCurrentProcess().ProjectPath;
            var data = selectedElement.GetAttribute("Name").ToString();

            Util.RunExternalProcess(data, projectPath.ToString());
        }
        */

        // ✅ EXCEL process
        private void runProtectProcess(MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
        {
            //MessageBox.Show("1");
            var selected = menuSelectionProvider.GetSelection<IEngineeringObject>().First();
            var projectPath = _tiaPortal.GetCurrentProcess().ProjectPath;
            string projName = Path.GetFileNameWithoutExtension(projectPath.FullName);
            //MessageBox.Show(projName);
            Util.RunExternalProcess("LOCK", projName);
        }
        private void rununprotectlProcess(MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
        {
            //MessageBox.Show("1");
            var selected = menuSelectionProvider.GetSelection<IEngineeringObject>().First();
            var projectPath = _tiaPortal.GetCurrentProcess().ProjectPath;
            string projName = Path.GetFileNameWithoutExtension(projectPath.FullName);
            //MessageBox.Show(projName);
            Util.RunExternalProcess("UNLOCK", projName);
        }
        private void rununprotectagainlProcess(MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
        {
            //MessageBox.Show("1");
            var selected = menuSelectionProvider.GetSelection<IEngineeringObject>().First();
            var projectPath = _tiaPortal.GetCurrentProcess().ProjectPath;
            string projName = Path.GetFileNameWithoutExtension(projectPath.FullName);
            //MessageBox.Show(projName);
            Util.RunExternalProcess("PROTECT_EXISTING", projName);
        }

        //private void runexistingprotectlProcess(MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
        //{
        //    //MessageBox.Show("1");
        //    var selected = menuSelectionProvider.GetSelection<IEngineeringObject>().First();
        //    var projectPath = _tiaPortal.GetCurrentProcess().ProjectPath;
        //    string projName = Path.GetFileNameWithoutExtension(projectPath.FullName);
        //    //MessageBox.Show(projName);
        //    Util.RunExternalProcess("ProtectExisting", projName);
        //}

        //// ✅ AWL process
        //private void runAwlProcess(MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
        //{
        //    var selected = menuSelectionProvider.GetSelection<IEngineeringObject>().First();
        //    var projectPath = _tiaPortal.GetCurrentProcess().ProjectPath;
        //    Util.RunExternalProcess("AWL", projectPath.ToString());
        //}

        //// ✅ XML process
        //private void runXmlProcess(MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
        //{
        //    var selected = menuSelectionProvider.GetSelection<IEngineeringObject>().First();
        //    var projectPath = _tiaPortal.GetCurrentProcess().ProjectPath;
        //    Util.RunExternalProcess("XML", projectPath.ToString());
        //}
        //// EXCEL WITH TEST Process
        //private void runExcelWithTestProcess(MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
        //{
        //    var selected = menuSelectionProvider.GetSelection<IEngineeringObject>().First();
        //    var projectPath = _tiaPortal.GetCurrentProcess().ProjectPath;
        //    Util.RunExternalProcess("EXCELWITHTEST", projectPath.ToString());
        //}
        //private void runAwlWithTestProcess(MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
        //{
        //    var selected = menuSelectionProvider.GetSelection<IEngineeringObject>().First();
        //    var projectPath = _tiaPortal.GetCurrentProcess().ProjectPath;
        //    Util.RunExternalProcess("AWLWITHTEST", projectPath.ToString());
        //}
        //private void runXmlWithTestProcess(MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
        //{
        //    var selected = menuSelectionProvider.GetSelection<IEngineeringObject>().First();
        //    var projectPath = _tiaPortal.GetCurrentProcess().ProjectPath;
        //    Util.RunExternalProcess("XMLWITHTEST", projectPath.ToString());
        //}
    }
}


//using Siemens.Engineering;
//using Siemens.Engineering.AddIn.Menu;
//using System.Linq;
//using Siemens.Engineering.SW.Blocks;
//using System;

//namespace AddInExample
//{
//    public class AddIn : ContextMenuAddIn
//    {
//        private readonly TiaPortal _tiaPortal;

//        public AddIn(TiaPortal tiaPortal) : base("Generate Block")
//        {
//            _tiaPortal = tiaPortal;
//            Util.LoadPermissions();
//        }

//        protected override void BuildContextMenuItems(ContextMenuAddInRoot addInRootSubmenu)
//        {
//            addInRootSubmenu.Items.AddActionItem<PlcBlock>("Selection info", methodToExecuteOnClick, DisplayStatus);

//            if (Util.HasPermission("EXCEL"))
//                addInRootSubmenu.Items.AddActionItem<IEngineeringObject>("EXCEL", runExcelProcess);

//            if (Util.HasPermission("AWL"))
//                addInRootSubmenu.Items.AddActionItem<IEngineeringObject>("AWL", runAwlProcess);

//            if (Util.HasPermission("XML"))
//                addInRootSubmenu.Items.AddActionItem<IEngineeringObject>("XML", runXmlProcess);

//            if (Util.HasPermission("EXCEL WITH TEST"))
//                addInRootSubmenu.Items.AddActionItem<IEngineeringObject>("EXCEL WITH TEST", runExcelWithTestProcess);

//            if (Util.HasPermission("AWL WITH TEST"))
//                addInRootSubmenu.Items.AddActionItem<IEngineeringObject>("AWL WITH TEST", runAwlWithTestProcess);

//            if (Util.HasPermission("XML WITH TEST"))
//                addInRootSubmenu.Items.AddActionItem<IEngineeringObject>("XML WITH TEST", runXmlWithTestProcess);
//        }

//        private void methodToExecuteOnClick(MenuSelectionProvider<PlcBlock> menuSelectionProvider)
//        {
//            var selectedBlocks = menuSelectionProvider.GetSelection<PlcBlock>();
//            if (selectedBlocks.Any())
//                Util.ShowBlockInfo(selectedBlocks.First());
//        }

//        private MenuStatus DisplayStatus(MenuSelectionProvider<PlcBlock> menuSelectionProvider)
//        {
//            return MenuStatus.Enabled;
//        }

//        private void runExcelProcess(MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
//        {
//            var projectPath = _tiaPortal.GetCurrentProcess()?.ProjectPath;
//            if (projectPath == null) return;

//            Util.RunExternalProcess("EXCEL", projectPath.ToString());
//        }

//        private void runAwlProcess(MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
//        {
//            var projectPath = _tiaPortal.GetCurrentProcess()?.ProjectPath;
//            if (projectPath == null) return;

//            Util.RunExternalProcess("AWL", projectPath.ToString());
//        }

//        private void runXmlProcess(MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
//        {
//            var projectPath = _tiaPortal.GetCurrentProcess()?.ProjectPath;
//            if (projectPath == null) return;

//            Util.RunExternalProcess("XML", projectPath.ToString());
//        }

//        private void runExcelWithTestProcess(MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
//        {
//            var projectPath = _tiaPortal.GetCurrentProcess()?.ProjectPath;
//            if (projectPath == null) return;

//            Util.RunExternalProcess("EXCEL WITH TEST", projectPath.ToString());
//        }

//        private void runAwlWithTestProcess(MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
//        {
//            var projectPath = _tiaPortal.GetCurrentProcess()?.ProjectPath;
//            if (projectPath == null) return;

//            Util.RunExternalProcess("AWL WITH TEST", projectPath.ToString());
//        }

//        private void runXmlWithTestProcess(MenuSelectionProvider<IEngineeringObject> menuSelectionProvider)
//        {
//            var projectPath = _tiaPortal.GetCurrentProcess()?.ProjectPath;
//            if (projectPath == null) return;

//            Util.RunExternalProcess("XML WITH TEST", projectPath.ToString());
//        }
//    }
//}

