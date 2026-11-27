using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using RockwellConverterForm = SiemensToRockwellWinForms.ConverterForm;
using BeckhoffConverterForm = SiemensToBackhoff.MainForm;

namespace PLCConversionSuite;

/// <summary>
/// Central application shell. The original conversion engines and their conversion
/// algorithms remain in their respective folders; this form only routes user input.
/// </summary>
public sealed class UnifiedMainForm : Form
{
    private readonly Color _canvas = Color.FromArgb(15, 23, 42);
    private readonly Color _panel = Color.FromArgb(24, 35, 58);
    private readonly Color _panelAlt = Color.FromArgb(30, 43, 69);
    private readonly Color _accent = Color.FromArgb(38, 132, 255);
    private readonly Color _text = Color.FromArgb(232, 239, 250);
    private readonly Color _muted = Color.FromArgb(151, 166, 190);

    private readonly ComboBox _source = new();
    private readonly ComboBox _target = new();
    private readonly ComboBox _mode = new();
    private readonly TextBox _filePath = new();
    private readonly TextBox _outputPath = new();
    private readonly Label _fileName = new();
    private readonly Label _fileFormat = new();
    private readonly Label _fileSize = new();
    private readonly Label _uploadStatus = new();
    private readonly Label _routeHint = new();
    private readonly Label _status = new();
    private readonly Button _convert = new();
    private readonly Panel _content = new();
    private Control? _dashboardBody;
    private Control? _workspaceBody;
    private readonly TextBox _bnrLog = new();
    private readonly Label _pageTitle = new();

    private Form? _activeChild;
    private string? _selectedFile;

    public UnifiedMainForm()
    {
        Text = "PLC Forge | Unified Conversion Studio";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1080, 720);
        Size = new Size(1420, 900);
        BackColor = _canvas;
        Font = new Font("Segoe UI", 9.5F);
        BuildShell();
        UpdateTargets();
    }

    private void BuildShell()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = _canvas, Margin = Padding.Empty };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 248));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var sidebar = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(10, 17, 32), Padding = new Padding(22, 24, 18, 20) };
        root.Controls.Add(sidebar, 0, 0);

        var brand = new Label { Text = "PLC  FORGE", ForeColor = _text, Font = new Font("Segoe UI", 17, FontStyle.Bold), AutoSize = false, Height = 38, Dock = DockStyle.Top };
        sidebar.Controls.Add(brand);
        var brandSub = new Label { Text = "CONVERSION STUDIO", ForeColor = _muted, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Height = 28, Dock = DockStyle.Top };
        sidebar.Controls.Add(brandSub);

        var nav = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 160, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(0, 24, 0, 0), BackColor = Color.Transparent };
        sidebar.Controls.Add(nav);
        nav.Controls.Add(NavButton("▦   Overview", true, ShowDashboard));
        nav.Controls.Add(NavButton("⇄   Conversion workspace", false, ShowWorkspacePage));

        var sidebarBottom = new Panel { Dock = DockStyle.Bottom, Height = 118, BackColor = Color.Transparent };
        sidebar.Controls.Add(sidebarBottom);
        sidebarBottom.Controls.Add(new Label { Text = "SUPPORTED ENGINE ROUTES", ForeColor = _muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), Dock = DockStyle.Top, Height = 26 });
        sidebarBottom.Controls.Add(new Label { Text = "SIEMENS  →  ROCKWELL\nSIEMENS  →  BECKHOFF\nSIEMENS  →  B&R", ForeColor = _text, Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill, Padding = new Padding(0, 5, 0, 0) });

        _content.Dock = DockStyle.Fill;
        _content.Padding = new Padding(30, 24, 30, 24);
        _content.BackColor = _canvas;
        root.Controls.Add(_content, 1, 0);

        var contentLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = _canvas };
        contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        contentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        _content.Controls.Add(contentLayout);

        var header = new Panel { Dock = DockStyle.Fill, BackColor = _canvas };
        _pageTitle.Text = "Unified conversion workspace";
        _pageTitle.ForeColor = _text;
        _pageTitle.Font = new Font("Segoe UI", 20, FontStyle.Bold);
        _pageTitle.Dock = DockStyle.Top;
        _pageTitle.Height = 38;
        header.Controls.Add(_pageTitle);
        header.Controls.Add(new Label { Text = "One interface. Dedicated conversion engines. Original conversion behaviour retained.", ForeColor = _muted, Dock = DockStyle.Bottom, Height = 26 });
        contentLayout.Controls.Add(header, 0, 0);

        var body = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = _canvas, Padding = new Padding(0, 8, 0, 8) };
        _dashboardBody = body;
        contentLayout.Controls.Add(body, 0, 1);
        BuildDashboard(body);

        _status.Text = "●  Ready · Select a source, target and XML file to begin";
        _status.ForeColor = _muted;
        _status.Dock = DockStyle.Fill;
        _status.TextAlign = ContentAlignment.MiddleLeft;
        contentLayout.Controls.Add(_status, 0, 2);
    }

    private Button NavButton(string text, bool selected, Action onClick)
    {
        return new Button
        {
            Text = text, Width = 205, Height = 42, TextAlign = ContentAlignment.MiddleLeft,
            FlatStyle = FlatStyle.Flat, BackColor = selected ? _panelAlt : Color.FromArgb(10, 17, 32),
            ForeColor = selected ? Color.White : _muted, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8), Padding = new Padding(12, 0, 0, 0), Cursor = Cursors.Hand
        }.Also(b => { b.FlatAppearance.BorderSize = 0; b.Click += (_, _) => onClick(); });
    }

    private void BuildDashboard(Control parent)
    {
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, RowCount = 4, BackColor = _canvas };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 106));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 252));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 194));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        parent.Controls.Add(layout);

        var hero = Card();
        hero.Padding = new Padding(22, 16, 22, 16);
        hero.Controls.Add(new Label { Text = "PLC BLOCK CONVERSION", ForeColor = Color.FromArgb(111, 178, 255), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Dock = DockStyle.Top, Height = 22 });
        hero.Controls.Add(new Label { Text = "Move PLC logic between engineering platforms", ForeColor = _text, Font = new Font("Segoe UI", 17, FontStyle.Bold), Dock = DockStyle.Top, Height = 34 });
        hero.Controls.Add(new Label { Text = "Choose a supported route and conversion mode. The selected engine performs the conversion using its existing implementation.", ForeColor = _muted, Dock = DockStyle.Fill });
        layout.Controls.Add(hero, 0, 0);

        var route = Card();
        route.Padding = new Padding(22, 18, 22, 16);
        var routeGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 3, BackColor = Color.Transparent };
        routeGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        routeGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        routeGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        routeGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        routeGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        routeGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        route.Controls.Add(routeGrid);
        routeGrid.Controls.Add(FieldLabel("SOURCE PLC PLATFORM"), 0, 0);
        routeGrid.Controls.Add(FieldLabel("TARGET PLC PLATFORM"), 1, 0);
        routeGrid.Controls.Add(FieldLabel("CONVERSION TYPE"), 2, 0);
        ConfigureCombo(_source, new[] { "Siemens", "Rockwell", "B&R", "Beckhoff" });
        ConfigureCombo(_target, Array.Empty<string>());
        ConfigureCombo(_mode, Array.Empty<string>());
        _source.SelectedIndexChanged += (_, _) => UpdateTargets();
        _target.SelectedIndexChanged += (_, _) => UpdateModes();
        routeGrid.Controls.Add(_source, 0, 1);
        routeGrid.Controls.Add(_target, 1, 1);
        routeGrid.Controls.Add(_mode, 2, 1);
        _routeHint.Text = "Supported routes are enabled automatically when a source platform is selected.";
        _routeHint.ForeColor = _muted;
        _routeHint.Dock = DockStyle.Fill;
        _routeHint.TextAlign = ContentAlignment.MiddleLeft;
        routeGrid.Controls.Add(_routeHint, 0, 2);
        routeGrid.SetColumnSpan(_routeHint, 3);
        layout.Controls.Add(route, 0, 1);

        var files = Card();
        files.Padding = new Padding(22, 16, 22, 16);
        var fileLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, BackColor = Color.Transparent };
        fileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));
        fileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        fileLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        fileLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        fileLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        fileLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        files.Controls.Add(fileLayout);
        fileLayout.Controls.Add(FieldLabel("SOURCE FILE"), 0, 0);
        fileLayout.Controls.Add(FieldLabel("OUTPUT LOCATION"), 1, 0);
        _filePath.ReadOnly = true;
        StyleTextBox(_filePath);
        _outputPath.Text = Path.Combine(AppContext.BaseDirectory, "Output");
        StyleTextBox(_outputPath);
        fileLayout.Controls.Add(_filePath, 0, 1);
        var outputPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
        outputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        outputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
        outputPanel.Controls.Add(_outputPath, 0, 0);
        var outputBrowse = ActionButton("Browse", false);
        outputBrowse.Dock = DockStyle.Fill;
        outputBrowse.Width = 92;
        outputBrowse.Click += (_, _) => BrowseOutput();
        outputPanel.Controls.Add(outputBrowse, 1, 0);
        fileLayout.Controls.Add(outputPanel, 1, 1);

        var fileMeta = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent };
        _fileName.Text = "No file selected"; _fileName.ForeColor = _text;
        _fileFormat.Text = "FORMAT —"; _fileFormat.ForeColor = _muted;
        _fileSize.Text = "SIZE —"; _fileSize.ForeColor = _muted;
        foreach (var l in new[] { _fileName, _fileFormat, _fileSize }) { l.AutoSize = true; l.Margin = new Padding(0, 4, 24, 0); }
        fileMeta.Controls.Add(_fileName); fileMeta.Controls.Add(_fileFormat); fileMeta.Controls.Add(_fileSize);
        fileLayout.Controls.Add(fileMeta, 0, 2);
        fileLayout.SetColumnSpan(fileMeta, 2);
        _uploadStatus.Text = "Upload status: waiting for file"; _uploadStatus.ForeColor = _muted; _uploadStatus.Dock = DockStyle.Fill;
        fileLayout.Controls.Add(_uploadStatus, 0, 3);
        fileLayout.SetColumnSpan(_uploadStatus, 2);
        layout.Controls.Add(files, 0, 2);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(0, 14, 0, 0), BackColor = Color.Transparent };
        var browse = ActionButton("＋   Browse source file", false);
        browse.Width = 190;
        browse.Click += (_, _) => BrowseInput();
        _convert.Text = "Start conversion   →";
        _convert.Width = 190; _convert.Height = 44; _convert.FlatStyle = FlatStyle.Flat; _convert.FlatAppearance.BorderSize = 0;
        _convert.BackColor = _accent; _convert.ForeColor = Color.White; _convert.Font = new Font("Segoe UI", 10, FontStyle.Bold); _convert.Cursor = Cursors.Hand;
        _convert.Click += (_, _) => StartConversion();
        actions.Controls.Add(browse); actions.Controls.Add(_convert);
        layout.Controls.Add(actions, 0, 3);
    }

    private Panel Card() => new() { Dock = DockStyle.Fill, BackColor = _panel, Margin = new Padding(0, 0, 0, 12) };

    private Label FieldLabel(string text) => new() { Text = text, ForeColor = _muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };

    private void ConfigureCombo(ComboBox combo, string[] items)
    {
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.FlatStyle = FlatStyle.Flat;
        combo.BackColor = _panelAlt;
        combo.ForeColor = _text;
        combo.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        combo.Dock = DockStyle.Fill;
        combo.Items.Clear();
        combo.Items.AddRange(items);
        if (combo.Items.Count > 0) combo.SelectedIndex = 0;
    }

    private void StyleTextBox(TextBox box)
    {
        box.Dock = DockStyle.Fill;
        box.BorderStyle = BorderStyle.FixedSingle;
        box.BackColor = _panelAlt;
        box.ForeColor = _text;
        box.Font = new Font("Segoe UI", 9.5F);
        box.Margin = new Padding(0, 2, 10, 2);
    }

    private Button ActionButton(string text, bool primary)
    {
        var b = new Button { Text = text, Height = 42, Width = 130, FlatStyle = FlatStyle.Flat, BackColor = primary ? _accent : _panelAlt, ForeColor = _text, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(0, 0, 12, 0) };
        b.FlatAppearance.BorderSize = 0;
        return b;
    }

    private void UpdateTargets()
    {
        string source = _source.SelectedItem?.ToString() ?? "Siemens";
        string[] targets = source == "Siemens" ? new[] { "Rockwell", "Beckhoff", "B&R" } : Array.Empty<string>();
        ConfigureCombo(_target, targets);
        UpdateModes();
        _routeHint.Text = targets.Length == 0
            ? $"No conversion engine is currently supplied for {source} as the source platform. Existing engines support Siemens as the source."
            : "Available conversion routes are based on the three supplied applications.";
    }

    private void UpdateModes()
    {
        string target = _target.SelectedItem?.ToString() ?? "";
        string[] modes = target switch
        {
            "Rockwell" => new[] { "PLC Block (FB / FC)", "UDT / Data Type" },
            "Beckhoff" => new[] { "PLC Block (FB / FC / OB)", "UDT / Data Type" },
            "B&R" => new[] { "Ladder Diagram (.ld) + Function Block (.fun)" },
            _ => Array.Empty<string>()
        };
        ConfigureCombo(_mode, modes);
        _convert.Enabled = modes.Length > 0;
        if (target == "Rockwell")
            _routeHint.Text = "Rockwell retains its original output paths under the application Output folder.";
        else if (target == "Beckhoff" || target == "B&R")
            _routeHint.Text = "Generated files are written to the output folder selected below.";
    }

    private void BrowseInput()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select Siemens TIA Portal XML export",
            Filter = "Siemens XML files (*.xml)|*.xml|All files (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        _selectedFile = dialog.FileName;
        _filePath.Text = _selectedFile;
        var info = new FileInfo(_selectedFile);
        _fileName.Text = info.Name;
        _fileFormat.Text = "FORMAT  " + info.Extension.ToUpperInvariant();
        _fileSize.Text = "SIZE  " + FormatSize(info.Length);
        _uploadStatus.Text = "Upload status: file selected and ready for processing";
        _uploadStatus.ForeColor = Color.FromArgb(91, 205, 155);
        _status.Text = "●  Source file loaded · " + info.Name;
    }

    private void BrowseOutput()
    {
        using var dialog = new FolderBrowserDialog { Description = "Select the output folder", SelectedPath = Directory.Exists(_outputPath.Text) ? _outputPath.Text : AppContext.BaseDirectory };
        if (dialog.ShowDialog(this) == DialogResult.OK) _outputPath.Text = dialog.SelectedPath;
    }

    private static string FormatSize(long bytes) => bytes < 1024 ? bytes + " B" : bytes < 1024 * 1024 ? (bytes / 1024d).ToString("0.0") + " KB" : (bytes / 1024d / 1024d).ToString("0.0") + " MB";

    private void StartConversion()
    {
        if (string.IsNullOrWhiteSpace(_selectedFile) || !File.Exists(_selectedFile))
        {
            MessageBox.Show(this, "Select a valid source XML file before starting the conversion.", "Source file required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_source.SelectedItem?.ToString() != "Siemens")
        {
            MessageBox.Show(this, "The supplied conversion engines currently support Siemens as the source platform.", "Route not supported", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        string target = _target.SelectedItem?.ToString() ?? "";
        string mode = _mode.SelectedItem?.ToString() ?? "";
        string output = string.IsNullOrWhiteSpace(_outputPath.Text) ? Path.Combine(AppContext.BaseDirectory, "Output") : _outputPath.Text;
        Directory.CreateDirectory(output);
        _convert.Enabled = false;
        try
        {
            if (target == "Rockwell")
            {
                ShowRockwellWorkspace();
                var form = (RockwellConverterForm)_activeChild!;
                if (mode.StartsWith("UDT", StringComparison.OrdinalIgnoreCase)) form.ConvertUdtFileFromHost(_selectedFile);
                else form.ConvertFileFromHost(_selectedFile);
            }
            else if (target == "Beckhoff")
            {
                ShowBeckhoffWorkspace();
                var form = (BeckhoffConverterForm)_activeChild!;
                string outputFile = Path.Combine(output, Path.GetFileNameWithoutExtension(_selectedFile) + (mode.StartsWith("UDT", StringComparison.OrdinalIgnoreCase) ? "_Beckhoff_UDT.xml" : "_Beckhoff.xml"));
                form.ConvertFileFromHost(_selectedFile, outputFile, mode.StartsWith("UDT", StringComparison.OrdinalIgnoreCase));
            }
            else if (target == "B&R")
            {
                ShowBnrWorkspace();
                _bnrLog.Clear();
                _bnrLog.AppendText("B&R Automation Studio conversion started\r\n");
                _bnrLog.AppendText("Input: " + _selectedFile + "\r\n\r\n");
                var files = BnrXmlToLdEngine.ConvertFileFromHost(_selectedFile, output);
                _bnrLog.AppendText(files.Count == 0 ? "No output files were generated. Check that the XML contains a supported Siemens FB/FC export." : "Conversion completed. Generated files:\r\n" + string.Join("\r\n", files));
                _status.Text = "●  B&R conversion completed · " + files.Count + " output file(s)";
            }
        }
        catch (Exception ex)
        {
            _status.Text = "●  Conversion failed · Review the error details";
            MessageBox.Show(this, ex.ToString(), "Conversion failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _convert.Enabled = true;
        }
    }

    private void ShowRockwellWorkspace()
    {
        if (_activeChild is RockwellConverterForm) { ShowWorkspacePage(); return; }
        var form = new RockwellConverterForm();
        HostForm(form, "Siemens → Rockwell");
    }

    private void ShowBeckhoffWorkspace()
    {
        if (_activeChild is BeckhoffConverterForm) { ShowWorkspacePage(); return; }
        var form = new BeckhoffConverterForm();
        HostForm(form, "Siemens → Beckhoff");
    }

    private void ShowBnrWorkspace()
    {
        DisposeChild();
        _pageTitle.Text = "Siemens → B&R Automation Studio";
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = _canvas, Padding = new Padding(0, 8, 0, 0) };
        var card = Card(); card.Padding = new Padding(22);
        card.Controls.Add(new Label { Text = "B&R XML → Ladder Diagram", ForeColor = _text, Font = new Font("Segoe UI", 15, FontStyle.Bold), Dock = DockStyle.Top, Height = 38 });
        card.Controls.Add(new Label { Text = "Original rung parsing, layout and Automation Studio XML generation are executed by the supplied B&R conversion engine.", ForeColor = _muted, Dock = DockStyle.Top, Height = 36 });
        _bnrLog.Multiline = true; _bnrLog.ReadOnly = true; _bnrLog.ScrollBars = ScrollBars.Both; _bnrLog.Dock = DockStyle.Fill; _bnrLog.BackColor = Color.FromArgb(11, 18, 32); _bnrLog.ForeColor = Color.FromArgb(180, 218, 255); _bnrLog.Font = new Font("Consolas", 10); _bnrLog.BorderStyle = BorderStyle.None;
        card.Controls.Add(_bnrLog);
        panel.Controls.Add(card);
        _workspaceBody = panel;
        ReplaceBody(panel);
    }

    private void HostForm(Form form, string title)
    {
        DisposeChild();
        _activeChild = form;
        form.TopLevel = false;
        form.FormBorderStyle = FormBorderStyle.None;
        form.Dock = DockStyle.Fill;
        ApplyEmbeddedTheme(form);
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = _canvas, Padding = new Padding(0, 8, 0, 0) };
        panel.Controls.Add(form);
        _workspaceBody = panel;
        ReplaceBody(panel);
        form.Show();
        _pageTitle.Text = title;
    }

    private void ShowWorkspacePage()
    {
        if (_workspaceBody == null) return;
        _pageTitle.Text = _activeChild is RockwellConverterForm ? "Siemens → Rockwell" : _activeChild is BeckhoffConverterForm ? "Siemens → Beckhoff" : "Conversion workspace";
        ReplaceBody(_workspaceBody);
        _activeChild?.BringToFront();
    }

    private void ReplaceBody(Control control)
    {
        var contentLayout = _content.Controls.OfType<TableLayoutPanel>().First();
        var oldBody = contentLayout.GetControlFromPosition(0, 1);
        if (oldBody != null) contentLayout.Controls.Remove(oldBody);
        control.Dock = DockStyle.Fill;
        contentLayout.Controls.Add(control, 0, 1);
    }

    private void ShowDashboard()
    {
        DisposeChild();
        _workspaceBody = null;
        _pageTitle.Text = "Unified conversion workspace";
        if (_dashboardBody != null) ReplaceBody(_dashboardBody);
    }

    private void DisposeChild()
    {
        if (_activeChild == null) return;
        try { _activeChild.Close(); _activeChild.Dispose(); } catch { }
        _activeChild = null;
        _workspaceBody = null;
    }

    private void ApplyEmbeddedTheme(Control parent)
    {
        foreach (Control c in parent.Controls)
        {
            c.Font = new Font("Segoe UI", c.Font.Size < 8 ? 8 : c.Font.Size, c.Font.Style);
            if (c is Button b)
            {
                b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderColor = Color.FromArgb(63, 83, 116);
                b.BackColor = _panelAlt; b.ForeColor = _text; b.Cursor = Cursors.Hand;
            }
            else if (c is TextBox t)
            {
                t.BackColor = Color.FromArgb(13, 22, 39); t.ForeColor = _text; t.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (c is Label l)
            {
                l.ForeColor = _text;
                if (l.BackColor == SystemColors.Control) l.BackColor = Color.Transparent;
            }
            else if (c is TabPage page) { page.BackColor = _panel; page.ForeColor = _text; }
            else if (c is TabControl tabs) tabs.BackColor = _panel;
            else if (c is Panel) c.BackColor = _panel;
            ApplyEmbeddedTheme(c);
        }
        parent.BackColor = _panel;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        DisposeChild();
        base.OnFormClosed(e);
    }
}

internal static class ControlExtensions
{
    public static T Also<T>(this T value, Action<T> action) { action(value); return value; }
}
