////using System;
////using System.Collections.Generic;
////using System.Linq;
////using System.Text;
////using System.Threading.Tasks;
////using System.Windows;
////using System.Windows.Controls;
////using System.Windows.Data;
////using System.Windows.Documents;
////using System.Windows.Input;
////using System.Windows.Media;
////using System.Windows.Media.Imaging;
////using System.Windows.Shapes;

////namespace FolderScan
////{
////    /// <summary>
////    /// Interaction logic for ProjectSelectionWindow.xaml
////    /// </summary>
////    public partial class ProjectSelectionWindow : Window
////    {
////        public ProjectSelectionWindow()
////        {
////            InitializeComponent();
////        }
////    }
////}
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Forms;

//public partial class ProjectSelectionWindow : Window
//{
//    public int? SelectedCount { get; private set; }
//    public bool IsAllSelected { get; private set; }

//    public ProjectSelectionWindow()
//    {
//        InitializeComponent();
//    }

//    private void Ok_Click(object sender, RoutedEventArgs e)
//    {
//        if (cmbCount.SelectedItem != null)
//        {
//            string value = (cmbCount.SelectedItem as ComboBoxItem).Content.ToString();

//            if (value == "ALL")
//            {
//                IsAllSelected = true;
//                DialogResult = true;
//                return;
//            }

//            SelectedCount = int.Parse(value);
//            DialogResult = true;
//            return;
//        }

//        if (int.TryParse(txtManual.Text, out int manualValue) && manualValue > 0)
//        {
//            SelectedCount = manualValue;
//            DialogResult = true;
//            return;
//        }

//        MessageBox.Show("Please select or enter a valid number.");
//    }

//    private void Cancel_Click(object sender, RoutedEventArgs e)
//    {
//        DialogResult = false;
//    }
//}
