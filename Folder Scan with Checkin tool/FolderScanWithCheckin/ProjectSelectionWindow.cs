using System.Windows;
using System.Windows.Controls;

namespace FolderScan   // 🔴 MUST MATCH YOUR PROJECT NAMESPACE
{
    public partial class ProjectSelectionWindow : Window

    {
        public int? SelectedCount { get; private set; }
        public bool IsAllSelected { get; private set; }

        ComboBox cmbCount;

        public ProjectSelectionWindow()
        {
            Title = "Project Selection";
            Width = 360;
            Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;

            Grid grid = new Grid { Margin = new Thickness(10) };

            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            // LABEL
            grid.Children.Add(new TextBlock
            {
                Text = "How many projects do you want to check-in?",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            });

            // SINGLE COMBOBOX (Editable = Infinite numbers)
            cmbCount = new ComboBox
            {
                Height = 30,
                IsEditable = true   // 🔥 KEY POINT
            };

            // Optional starter values
            cmbCount.Items.Add("1");
            cmbCount.Items.Add("2");
            cmbCount.Items.Add("3");
            cmbCount.Items.Add("4");
            cmbCount.Items.Add("5");
            cmbCount.Items.Add("ALL");

            Grid.SetRow(cmbCount, 1);
            grid.Children.Add(cmbCount);

            // BUTTONS
            StackPanel panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 15, 0, 0)
            };

            Button ok = new Button { Content = "OK", Width = 80, Margin = new Thickness(0, 0, 10, 0) };
            ok.Click += Ok_Click;

            Button cancel = new Button { Content = "Cancel", Width = 80 };
            cancel.Click += (s, e) => Close();

            panel.Children.Add(ok);
            panel.Children.Add(cancel);

            Grid.SetRow(panel, 2);
            grid.Children.Add(panel);

            Content = grid;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            string value = cmbCount.Text?.Trim();

            if (string.IsNullOrWhiteSpace(value))
            {
                System.Windows.MessageBox.Show("Please select or enter a value.");
                return;
            }

            if (value.Equals("ALL", System.StringComparison.OrdinalIgnoreCase))
            {
                IsAllSelected = true;
                DialogResult = true;
                return;
            }

            if (int.TryParse(value, out int count) && count > 0)
            {
                SelectedCount = count;
                DialogResult = true;
                return;
            }

            System.Windows.MessageBox.Show(
                "Please enter a valid number or select ALL.",
                "Validation",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}
