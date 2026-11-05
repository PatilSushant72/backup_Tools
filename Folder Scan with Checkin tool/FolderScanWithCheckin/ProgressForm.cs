//using System.Drawing;
//using System.Windows.Forms;

//public class ProgressForm : Form
//{
//    private Label lbl;

//    public ProgressForm(string message)
//    {
//        Text = "Processing";
//        Width = 420;
//        Height = 150;
//        StartPosition = FormStartPosition.CenterScreen;
//        FormBorderStyle = FormBorderStyle.FixedDialog;
//        ControlBox = false;
//        TopMost = true;

//        lbl = new Label
//        {
//            Dock = DockStyle.Fill,
//            TextAlign = ContentAlignment.MiddleCenter,
//            Font = new Font("Segoe UI", 10, FontStyle.Bold),
//            Text = message
//        };

//        Controls.Add(lbl);
//    }
//}
using System;
using System.Drawing;
using System.Windows.Forms;

public class ProgressForm : Form
{
    Label lblHeader;
    TextBox txtStatus;

    public ProgressForm()
    {
        Text = "Octoplant Check-In";
        Width = 600;
        Height = 300;
        StartPosition = FormStartPosition.CenterScreen;

        FormBorderStyle = FormBorderStyle.Sizable;
        MinimizeBox = true;
        MaximizeBox = false;

        lblHeader = new Label
        {
            Text = "Octoplant Check-In is in Progress.\r\nPlease don’t close this window.",
            Dock = DockStyle.Top,
            Height = 60,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        txtStatus = new TextBox
        {
            Multiline = true,
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical,
            ReadOnly = true
        };

        Controls.Add(txtStatus);
        Controls.Add(lblHeader);

        FormClosing += ProgressForm_FormClosing;
    }

    private void ProgressForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            var result = MessageBox.Show(
                "Do you really want to close?",
                "Confirm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.No)
                e.Cancel = true;
        }
    }

    public void UpdateStatus(string component, string project, string status)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => UpdateStatus(component, project, status)));
            return;
        }

        txtStatus.AppendText(
            $"[{DateTime.Now:HH:mm:ss}] " +
            $"Component: {component} | " +
            $"Project: {project} | " +
            $"{status}\r\n"
        );
    }

    public void SafeClose()
    {
        if (InvokeRequired)
            Invoke(new Action(Close));
        else
            Close();
    }
}
