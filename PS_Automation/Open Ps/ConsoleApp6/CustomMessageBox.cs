using System;
using System.Drawing;
using System.Windows.Forms;

public class CustomMessageBox : Form
{
    private RichTextBox richTextBox;
    private Button okButton;

    public CustomMessageBox(string successMessages, string errorMessages)
    {
        // Set up the form properties
        this.Text = "Compilation Results";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        // Create and configure RichTextBox
        richTextBox = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BackColor = Color.White,
            BorderStyle = BorderStyle.None,
            SelectionColor = Color.Black
        };

        // Append success and error messages
        AppendMessages(successMessages, Color.Black);
        AppendMessages(errorMessages, Color.Red);

        // Create and configure OK button
        okButton = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Dock = DockStyle.Bottom
        };
        okButton.Click += (sender, e) => this.Close();

        // Add controls to the form
        this.Controls.Add(richTextBox);
        this.Controls.Add(okButton);
        this.Width = 500;
        this.Height = 300;
    }

    private void AppendMessages(string messages, Color color)
    {
        if (!string.IsNullOrWhiteSpace(messages))
        {
            richTextBox.SelectionStart = richTextBox.TextLength;
            richTextBox.SelectionLength = 0;
            richTextBox.SelectionColor = color;
            richTextBox.AppendText(messages + Environment.NewLine);
            richTextBox.SelectionColor = richTextBox.ForeColor; // Reset to default color
        }
    }
}
