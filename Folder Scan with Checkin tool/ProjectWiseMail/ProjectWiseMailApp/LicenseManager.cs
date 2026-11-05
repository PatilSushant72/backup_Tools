using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows;

using System.Windows.Controls;
using MessageBox = System.Windows.MessageBox;
using Window = System.Windows.Window;
using Thickness = System.Windows.Thickness;
using Button = System.Windows.Controls.Button;
using System.Windows.Controls;
using TextBox = System.Windows.Controls.TextBox;
//using MessageBox = System.Windows.MessageBox;
//using Window = System.Windows.Window;
//using Thickness = System.Windows.Thickness;
//using Button = System.Windows.Controls.Button;
//{
//    internal class LicenseManager
//    {
//    }
//}
[Serializable]
public class KeyData
{
    public string ApplicationKey = "";
    public string ValidationKey = "";
    public DateTime RegisterDate = DateTime.Now;
}

//public class LicenseManager


public class LicenseManager
{
    private static readonly string DatFile =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data.dat");

    public static string ApplicationKey = "";
    public static string ValidationKey = "";

    // =====================================================
    // ENTRY METHOD (CALLED FROM ADD-IN / EXE)
    // =====================================================
    public bool EnsureLicense()
    {
        ReadData();

        // 🔹 If no ApplicationKey → generate
        if (string.IsNullOrEmpty(ApplicationKey))
        {
            CreateNewLicense();
        }

        // 🔹 If ValidationKey missing or invalid → show dialog
        if (string.IsNullOrEmpty(ValidationKey) || !VerifyKey())
        {
            var lf = new LicenseForm(ApplicationKey);
            bool? result = lf.ShowDialog();

            if (result == true)
            {
                ValidationKey = lf.EnteredValidationKey;

                if (!VerifyKey())
                {
                    System.Windows.MessageBox.Show(
                        "Invalid Validation Key.",
                        "License Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                SaveKeyData();
            }
            else
            {
                return false; // user closed dialog
            }
        }

        return true;
    }

    // =====================================================
    // READ / CREATE LICENSE
    // =====================================================
    private void ReadData()
    {
        if (!File.Exists(DatFile))
        {
            CreateNewLicense();
            return;
        }

        try
        {
            var data = Deserialize<KeyData>(DatFile);
            ApplicationKey = data?.ApplicationKey ?? "";
            ValidationKey = data?.ValidationKey ?? "";

            if (!VerifyKey())
            {
                MessageBox.Show(
                    "License is invalid or expired.\nA new Application Key has been generated.\n\nPlease send it to Admin.",
                    "License Reset",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                CreateNewLicense();
            }
        }
        catch
        {
            MessageBox.Show("License file corrupted. Creating new license.");
            CreateNewLicense();
        }
    }

    //private void CreateNewLicense()
    //{
    //    GenerateApplicationKey(Environment.MachineName + ".DEVICE");
    //    ValidationKey = "";
    //    SaveKeyData();
    //}
    private void CreateNewLicense()
    {
        // Generate new Application Key
        GenerateApplicationKey(Environment.MachineName + ".DEVICE");

        // Clear validation key
        ValidationKey = "";

        // Save new license info
        SaveKeyData();

        var result = MessageBox.Show(
            $"New Application Key Generated:\n\n{ApplicationKey}\n\nSend this key to Admin for validation.",
            "Application Key",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );

        // ✅ Send mail AFTER clicking OK
        if (result == MessageBoxResult.OK)
        {
            SendApplicationKeyMail(ApplicationKey);
        }
    }
    // =====================================================
    // APPLICATION KEY
    // =====================================================
    private void GenerateApplicationKey(string deviceName)
    {
        string mac = GetMacAddress();
        string pc = Environment.MachineName;
        string device = deviceName.Split('.')[1];
        string time = DateTime.Now.ToString("yyyyMMddHHmmss");
        string app = "PMG";
        // FORMAT:
        // MAC.APL.PC.APP.DEVICE.PC.TIME.wipropari
        string raw = $"{mac}.APL.{pc}.{app}.{device}.PC.{time}.wipropari";

        ApplicationKey = AddDash(ToHexString(raw));
    }

    // =====================================================
    // VALIDATION KEY VERIFY (MATCHES pasted.txt)
    // =====================================================
    public bool VerifyKey()
    {
        if (string.IsNullOrWhiteSpace(ValidationKey))
            return false;

        string raw = FromHexString(ValidationKey.Replace("-", ""));
        string[] parts = raw.Split('.');

        // Expected:
        // MAC.VAL.PC.APP.DEVICE.PC.EXPIRY.wipropari
        if (parts.Length < 8 || !parts[1].Equals("VAL", StringComparison.OrdinalIgnoreCase))
            return false;

        string keyMac = parts[0];
        string keyPc = parts[2];
        string expiryPart = parts[6];
        string app = parts[3];
        // 🔐 Machine binding
        if (!keyMac.Equals(GetMacAddress(), StringComparison.OrdinalIgnoreCase) ||
            !keyPc.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(
                "License is not valid for this machine.",
                "License Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        // ⏰ Expiry check
        if (!DateTime.TryParseExact(
                expiryPart,
                "yyyyMMddHHmmss",
                null,
                System.Globalization.DateTimeStyles.None,
                out DateTime expiry))
            return false;

        if (DateTime.Now > expiry)
        {
            MessageBox.Show(
                "License expired. Contact administrator.",
                "License Expired",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        return true;
    }
    private void SendApplicationKeyMail(string applicationKey)
    {
        try
        {
            Microsoft.Office.Interop.Outlook.Application outlook = new Microsoft.Office.Interop.Outlook.Application();
            Microsoft.Office.Interop.Outlook.MailItem mail =
                (Microsoft.Office.Interop.Outlook.MailItem)outlook.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
            string senderName = Environment.UserName;
            // TO
            mail.To = "priti.d.3@wipropari.com; sushant.p.1@wipropari.com";

            // CC
            mail.CC = "sainathhb@wipropari.com;";

            mail.Subject = "Request for Validation Key of ProjectWise Mail Generator Application";

            mail.Body =
                "Hello Sir," + Environment.NewLine + Environment.NewLine +
                "Kindly provide the validation key for the following application key:" +
                Environment.NewLine + Environment.NewLine +
                applicationKey + Environment.NewLine + Environment.NewLine +
                "Thanks & regards," + Environment.NewLine +
                senderName;

            // ✅ Opens Outlook mail window
            //mail.Display();

            // 🔴 If you want silent auto-send instead:
            mail.Send();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Failed to send email.\n\n" + ex.Message,
                "Mail Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }
    // =====================================================
    // STORAGE
    // =====================================================
    private void SaveKeyData()
    {
        Serialize(DatFile, new KeyData
        {
            ApplicationKey = ApplicationKey,
            ValidationKey = ValidationKey,
            RegisterDate = DateTime.Now
        });
    }

    private static void Serialize(string path, object data)
    {
        using (var fs = new FileStream(path, FileMode.Create))
            new BinaryFormatter().Serialize(fs, data);
    }

    private static T Deserialize<T>(string path) where T : class
    {
        using (var fs = new FileStream(path, FileMode.Open))
            return new BinaryFormatter().Deserialize(fs) as T;
    }

    // =====================================================
    // HELPERS
    // =====================================================
    private static string GetMacAddress()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .FirstOrDefault(n =>
                n.OperationalStatus == OperationalStatus.Up &&
                n.Description.ToLower().Contains("realtek"))
            ?.GetPhysicalAddress().ToString() ?? "UNKNOWN";
    }

    private static string ToHexString(string str)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in Encoding.UTF8.GetBytes(str))
            sb.Append(b.ToString("X2"));
        return sb.ToString();
    }

    private static string FromHexString(string hex)
    {
        byte[] bytes = Enumerable.Range(0, hex.Length / 2)
            .Select(i => Convert.ToByte(hex.Substring(i * 2, 2), 16))
            .ToArray();
        return Encoding.UTF8.GetString(bytes);
    }

    private static string AddDash(string str)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < str.Length; i++)
        {
            if (i > 0 && i % 4 == 0)
                sb.Append('-');
            sb.Append(str[i]);
        }
        return sb.ToString();
    }
}
public class LicenseForm : Window
{
    public string EnteredValidationKey { get; private set; } = "";
    private TextBox txtValidation;


    public LicenseForm(string applicationKey)
    {
        Title = "Key Validation";
        Width = 520;
        Height = 260;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        ResizeMode = ResizeMode.NoResize;

        Grid grid = new Grid
        {
            Margin = new Thickness(20)
        };

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Application Key
        grid.Children.Add(new TextBlock
        {
            Text = "Application Key",
            FontWeight = FontWeights.Bold
        });

        TextBox txtAppKey = new TextBox
        {
            Text = applicationKey,
            IsReadOnly = true,
            Margin = new Thickness(0, 5, 0, 15)
        };
        Grid.SetRow(txtAppKey, 1);
        grid.Children.Add(txtAppKey);

        // Validation Key
        grid.Children.Add(new TextBlock
        {
            Text = "Validation Key",
            Margin = new Thickness(0, 10, 0, 0),
            FontWeight = FontWeights.Bold
        });
        //Grid.SetRow(grid.Children[^1], 2);
        UIElement last = grid.Children[grid.Children.Count - 1];
        Grid.SetRow(last, 2);

        txtValidation = new TextBox
        {
            Margin = new Thickness(0, 5, 0, 15)
        };
        Grid.SetRow(txtValidation, 3);
        grid.Children.Add(txtValidation);

        Button btnRegister = new Button
        {
            Content = "Register",
            Width = 100,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        btnRegister.Click += BtnRegister_Click;
        Grid.SetRow(btnRegister, 4);

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.Children.Add(btnRegister);

        Content = grid;
    }

    private void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        EnteredValidationKey = txtValidation.Text.Trim();

        if (string.IsNullOrEmpty(EnteredValidationKey))
        {
            MessageBox.Show(
                "Please enter a Validation Key.",
                "Validation Required",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }

        DialogResult = true;   // ✅ WPF
        Close();
    }
}
