//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ProjectWiseMailApp
//{
//    internal class MainForm
//    {
//    }
//}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace ProjectWiseMailApp
{
    public class MainForm : Form
    {
        private TextBox txtExcelPath;
        private Button btnBrowse;
        private Button btnLoad;
        private Button btnCreateDrafts;
        private Button btnRefreshAccounts;
        private ComboBox cmbSenderAccount;
        private CheckBox chkOnlyToday;
        private DataGridView gridPreview;
        private Label lblStatus;
        private DateTimePicker dtFrom;
        private DateTimePicker dtTo;
        private Label lblFrom;
        private Label lblTo;
        private List<ProjectMailGroup> _mailGroups = new List<ProjectMailGroup>();
        private List<OutlookAccountInfo> _outlookAccounts = new List<OutlookAccountInfo>();

        private readonly List<string> _baseCcList = new List<string>
        {
            
   "sandipc@wipropari.com",
   "sainathhb@wipropari.com",
   "harshal.g@wipropari.com",
   "vinodpn@wipropari.com",
   "avinashc@wipropari.com",
   "sandeeppa@wipropari.com",
   "prasad.w@wipropari.com"

        };

        public MainForm()
        {
            InitializeUi();
        }

        private void InitializeUi()
        {
            Text = "Project Wise Outlook Mail Generator";
            Width = 1300;
            Height = 750;
            StartPosition = FormStartPosition.CenterScreen;

            Label lblFile = new Label();
            lblFile.Text = "Excel File:";
            lblFile.Left = 15;
            lblFile.Top = 18;
            lblFile.Width = 70;

            txtExcelPath = new TextBox();
            txtExcelPath.Left = 90;
            txtExcelPath.Top = 14;
            txtExcelPath.Width = 400;

            btnBrowse = new Button();
            btnBrowse.Text = "Browse";
            btnBrowse.Left = 500;
            btnBrowse.Top = 12;
            btnBrowse.Width = 60;
            btnBrowse.Click += BtnBrowse_Click;

            lblFrom = new Label();
            lblFrom.Text = "From:";
            lblFrom.Left = 600;
            lblFrom.Top = 14;
            lblFrom.Width = 40;

            dtFrom = new DateTimePicker();
            dtFrom.Left = 650;
            dtFrom.Top = 14;
            dtFrom.Width = 130;
            dtFrom.Format = DateTimePickerFormat.Custom;
            dtFrom.CustomFormat = "dd-MMM-yyyy";
            dtFrom.Value = DateTime.Today;

            lblTo = new Label();
            lblTo.Text = "To:";
            lblTo.Left = 800;
            lblTo.Top = 14;
            lblTo.Width = 25;

            dtTo = new DateTimePicker();
            dtTo.Left = 825;
            dtTo.Top = 14;
            dtTo.Width = 130;
            dtTo.Format = DateTimePickerFormat.Custom;
            dtTo.CustomFormat = "dd-MMM-yyyy";
            dtTo.Value = DateTime.Today;

            chkOnlyToday = new CheckBox();
            chkOnlyToday.Text = "Only today's records";
            chkOnlyToday.Left = 1000;
            chkOnlyToday.Top = 14;
            chkOnlyToday.Width = 150;
            chkOnlyToday.Checked = true;
            chkOnlyToday.CheckedChanged += ChkOnlyToday_CheckedChanged;

            btnLoad = new Button();
            btnLoad.Text = "Load Excel";
            btnLoad.Left = 1150;
            btnLoad.Top = 12;
            btnLoad.Width = 100;
            btnLoad.Click += BtnLoad_Click;

            Label lblSender = new Label();
            lblSender.Text = "Send From:";
            lblSender.Left = 15;
            lblSender.Top = 55;
            lblSender.Width = 75;

            cmbSenderAccount = new ComboBox();
            cmbSenderAccount.Left = 90;
            cmbSenderAccount.Top = 50;
            cmbSenderAccount.Width = 420;
            cmbSenderAccount.DropDownStyle = ComboBoxStyle.DropDownList;

            btnRefreshAccounts = new Button();
            btnRefreshAccounts.Text = "Refresh Outlook Accounts";
            btnRefreshAccounts.Left = 550;
            btnRefreshAccounts.Top = 47;
            btnRefreshAccounts.Width = 220;
            btnRefreshAccounts.Click += BtnRefreshAccounts_Click;

            //chkOnlyToday = new CheckBox();
            //chkOnlyToday.Text = "Only today's records";
            //chkOnlyToday.Left = 530;
            //chkOnlyToday.Top = 53;
            //chkOnlyToday.Width = 160;
            //chkOnlyToday.Checked = true;


            btnCreateDrafts = new Button();
            btnCreateDrafts.Text = "Create Outlook Drafts";
            btnCreateDrafts.Left = 900;
            //btnCreateDrafts.Top = 85;
            btnCreateDrafts.Top = 47;
            btnCreateDrafts.Width = 190;
            btnCreateDrafts.Height = 30;
            btnCreateDrafts.Click += BtnCreateDrafts_Click;

            //btnCreateDrafts = new Button();
            //btnCreateDrafts.Text = "Create Outlook Drafts";
            //btnCreateDrafts.Left = 710;
            //btnCreateDrafts.Top = 47;
            //btnCreateDrafts.Width = 220;
            //btnCreateDrafts.Height = 30;
            //btnCreateDrafts.Click += BtnCreateDrafts_Click;

            lblStatus = new Label();
            lblStatus.Left = 15;
            lblStatus.Top = 90;
            lblStatus.Width = 1200;
            lblStatus.Height = 25;
            lblStatus.Text = "Status: Ready";

            gridPreview = new DataGridView();
            gridPreview.Left = 15;
            gridPreview.Top = 120;
            gridPreview.Width = 1240;
            gridPreview.Height = 560;
            gridPreview.ReadOnly = true;
            gridPreview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridPreview.AllowUserToAddRows = false;
            gridPreview.AllowUserToDeleteRows = false;

            Controls.Add(lblFile);
            Controls.Add(txtExcelPath);
            Controls.Add(btnBrowse);
            Controls.Add(btnLoad);
            Controls.Add(btnRefreshAccounts);
            Controls.Add(lblSender);
            Controls.Add(cmbSenderAccount);
            Controls.Add(chkOnlyToday);
            Controls.Add(btnCreateDrafts);
            Controls.Add(lblStatus);
            Controls.Add(gridPreview);
            Controls.Add(lblFile);
            Controls.Add(txtExcelPath);
            Controls.Add(btnBrowse);
            Controls.Add(btnLoad);
            Controls.Add(btnRefreshAccounts);
            Controls.Add(lblSender);
            Controls.Add(cmbSenderAccount);
            Controls.Add(lblFrom);
            Controls.Add(dtFrom);
            Controls.Add(lblTo);
            Controls.Add(dtTo);
            Controls.Add(chkOnlyToday);
            Controls.Add(btnCreateDrafts);
            Controls.Add(lblStatus);
            Controls.Add(gridPreview);
            Load += MainForm_Load;

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                RefreshOutlookAccounts();
                ChkOnlyToday_CheckedChanged(null, null);
            }
            catch (Exception ex)
            {

                MessageBox.Show("Outlook account load failed: " + ex.Message);
            
            }
        }
        //private void ChkOnlyToday_CheckedChanged(object sender, EventArgs e)
        //{
        //    bool enableDateRange = !chkOnlyToday.Checked;

        //    dtFrom.Enabled = enableDateRange;
        //    dtTo.Enabled = enableDateRange;
        //    lblFrom.Enabled = enableDateRange;
        //    lblTo.Enabled = enableDateRange;
        //}
        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Excel Files (*.xlsx;*.xlsm)|*.xlsx;*.xlsm|All Files (*.*)|*.*";
            ofd.Title = "Select Excel File";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtExcelPath.Text = ofd.FileName;
            }

            ofd.Dispose();
        }

        private void BtnRefreshAccounts_Click(object sender, EventArgs e)
        {
            RefreshOutlookAccounts();
        }

      


        ////////private void BtnLoad_Click(object sender, EventArgs e)
        ////////{
        ////////    try
        ////////    {

        ////////        List<ExcelRowData> rows = ExcelReader.ReadRows(txtExcelPath.Text);

        ////////        if (chkOnlyToday.Checked)
        ////////        {
        ////////            DateTime today = DateTime.Today;
        ////////            rows = rows.Where(r => r.Date.Date == today).ToList();
        ////////        }
        ////////        else
        ////////        {
        ////////            DateTime fromDate = dtFrom.Value.Date;
        ////////            DateTime toDate = dtTo.Value.Date;

        ////////            if (fromDate > toDate)
        ////////            {
        ////////                MessageBox.Show("'From' date cannot be greater than 'To' date.");
        ////////                return;
        ////////            }

        ////////            rows = rows.Where(r => r.Date.Date >= fromDate && r.Date.Date <= toDate).ToList();
        ////////        }

        ////////        _mailGroups = MailGroupingService.BuildGroups(rows);

        ////////        gridPreview.DataSource = BuildPreviewTable(_mailGroups);
        ////////        lblStatus.Text = "Status: Loaded " + _mailGroups.Count + " mail group(s), total rows = " + _mailGroups.Sum(x => x.Rows.Count);
        ////////        //if (string.IsNullOrWhiteSpace(txtExcelPath.Text))
        ////////        //{
        ////////        //    MessageBox.Show("Please select Excel file.");
        ////////        //    return;
        ////////        //}

        ////////        //if (!System.IO.File.Exists(txtExcelPath.Text))
        ////////        //{
        ////////        //    MessageBox.Show("Excel file not found.");
        ////////        //    return;
        ////////        //}

        ////////        //List<ExcelRowData> rows = ExcelReader.ReadRows(txtExcelPath.Text);

        ////////        //if (chkOnlyToday.Checked)
        ////////        //{
        ////////        //    DateTime today = DateTime.Today;
        ////////        //    rows = rows.Where(r => r.Date.Date == today).ToList();
        ////////        //}

        ////////        //_mailGroups = MailGroupingService.BuildGroups(rows);

        ////////        //gridPreview.DataSource = BuildPreviewTable(_mailGroups);
        ////////        //lblStatus.Text = "Status: Loaded " + _mailGroups.Count + " mail group(s), total rows = " + _mailGroups.Sum(x => x.Rows.Count);
        ////////    }
        ////////    catch (Exception ex)
        ////////    {
        ////////        MessageBox.Show("Load failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        ////////    }
        ////////}

        private async void BtnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtExcelPath.Text))
                {
                    MessageBox.Show("Please select Excel file.");
                    return;
                }

                string filePath = txtExcelPath.Text.Trim();

                if (!System.IO.File.Exists(filePath))
                {
                    MessageBox.Show("Excel file not found.");
                    return;
                }

                DateTime fromDate = dtFrom.Value.Date;
                DateTime toDate = dtTo.Value.Date;
                bool onlyToday = chkOnlyToday.Checked;

                if (!onlyToday && fromDate > toDate)
                {
                    MessageBox.Show("'From' date cannot be greater than 'To' date.");
                    return;
                }

                lblStatus.Text = "Status: Loading Excel...";
                btnLoad.Enabled = false;
                btnCreateDrafts.Enabled = false;
                btnBrowse.Enabled = false;
                btnRefreshAccounts.Enabled = false;
                Cursor = Cursors.WaitCursor;

                List<ExcelRowData> rows = await System.Threading.Tasks.Task.Run(delegate
                {
                    return ExcelReader.ReadRows(filePath, onlyToday, fromDate, toDate);
                });

                _mailGroups = MailGroupingService.BuildGroups(rows);

                gridPreview.DataSource = null;
                gridPreview.DataSource = BuildPreviewTable(_mailGroups);

                if (rows.Count == 0)
                {
                    lblStatus.Text = "Status: No records found for selected date filter.";
                    MessageBox.Show("No records found for selected date filter.");
                }
                else
                {
                    lblStatus.Text = "Status: Loaded " + _mailGroups.Count + " mail group(s), total rows = " + rows.Count;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Load failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Status: Load failed.";
            }
            finally
            {
                btnLoad.Enabled = true;
                btnCreateDrafts.Enabled = true;
                btnBrowse.Enabled = true;
                btnRefreshAccounts.Enabled = true;
                Cursor = Cursors.Default;
            }
        }


        private void SetWeekRangeMondayToFriday()
        {
            DateTime today = DateTime.Today;

            // Calculate Monday
            int diff = today.DayOfWeek - DayOfWeek.Monday;

            if (diff < 0)
                diff += 7;

            DateTime monday = today.AddDays(-diff);

            // Friday = Monday + 4 days
            DateTime friday = monday.AddDays(4);

            dtFrom.Value = monday;
            dtTo.Value = friday;
        }
        private void ChkOnlyToday_CheckedChanged(object sender, EventArgs e)
        {
            if (chkOnlyToday.Checked)
            {
                // Only today
                dtFrom.Value = DateTime.Today;
                dtTo.Value = DateTime.Today;
            }
            else
            {
                // Monday to Friday
                SetWeekRangeMondayToFriday();
            }

            // Enable/Disable controls
            bool enableDateRange = !chkOnlyToday.Checked;

            dtFrom.Enabled = enableDateRange;
            dtTo.Enabled = enableDateRange;
            lblFrom.Enabled = enableDateRange;
            lblTo.Enabled = enableDateRange;
        }


        //////private async void BtnLoad_Click(object sender, EventArgs e)
        //////{
        //////    try
        //////    {
        //////        if (string.IsNullOrWhiteSpace(txtExcelPath.Text))
        //////        {
        //////            MessageBox.Show("Please select Excel file.");
        //////            return;
        //////        }

        //////        string filePath = txtExcelPath.Text.Trim();

        //////        if (!System.IO.File.Exists(filePath))
        //////        {
        //////            MessageBox.Show("Excel file not found.");
        //////            return;
        //////        }

        //////        DateTime fromDate = dtFrom.Value.Date;
        //////        DateTime toDate = dtTo.Value.Date;
        //////        bool onlyToday = chkOnlyToday.Checked;

        //////        if (!onlyToday && fromDate > toDate)
        //////        {
        //////            MessageBox.Show("'From' date cannot be greater than 'To' date.");
        //////            return;
        //////        }

        //////        lblStatus.Text = "Status: Loading Excel...";
        //////        btnLoad.Enabled = false;
        //////        btnCreateDrafts.Enabled = false;
        //////        btnBrowse.Enabled = false;
        //////        btnRefreshAccounts.Enabled = false;
        //////        Cursor = Cursors.WaitCursor;

        //////        List<ExcelRowData> rows = await System.Threading.Tasks.Task.Run(delegate
        //////        {
        //////            return ExcelReader.ReadRows(filePath, onlyToday, fromDate, toDate);
        //////        });

        //////        _mailGroups = MailGroupingService.BuildGroups(rows);

        //////        gridPreview.DataSource = null;
        //////        gridPreview.DataSource = BuildPreviewTable(_mailGroups);

        //////        if (rows.Count == 0)
        //////        {
        //////            lblStatus.Text = "Status: No records found for selected date filter.";
        //////            MessageBox.Show("No records found for selected date filter.");
        //////        }
        //////        else
        //////        {
        //////            lblStatus.Text = "Status: Loaded " + _mailGroups.Count + " mail group(s), total rows = " + rows.Count;
        //////        }
        //////    }
        //////    catch (Exception ex)
        //////    {
        //////        MessageBox.Show("Load failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //////        lblStatus.Text = "Status: Load failed.";
        //////    }
        //////    finally
        //////    {
        //////        btnLoad.Enabled = true;
        //////        btnCreateDrafts.Enabled = true;
        //////        btnBrowse.Enabled = true;
        //////        btnRefreshAccounts.Enabled = true;
        //////        Cursor = Cursors.Default;
        //////    }
        //////}

        //private async void BtnLoad_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(txtExcelPath.Text))
        //        {
        //            MessageBox.Show("Please select Excel file.");
        //            return;
        //        }

        //        if (!System.IO.File.Exists(txtExcelPath.Text))
        //        {
        //            MessageBox.Show("Excel file not found.");
        //            return;
        //        }

        //        DateTime fromDate = dtFrom.Value.Date;
        //        DateTime toDate = dtTo.Value.Date;

        //        if (!chkOnlyToday.Checked && fromDate > toDate)
        //        {
        //            MessageBox.Show("'From' date cannot be greater than 'To' date.");
        //            return;
        //        }

        //        lblStatus.Text = "Status: Loading Excel...";
        //        btnLoad.Enabled = false;
        //        btnCreateDrafts.Enabled = false;
        //        Cursor = Cursors.WaitCursor;

        //        List<ExcelRowData> rows = await System.Threading.Tasks.Task.Run(() =>
        //            ExcelReader.ReadRows(
        //                txtExcelPath.Text,
        //                chkOnlyToday.Checked,
        //                fromDate,
        //                toDate
        //            )
        //        );

        //        _mailGroups = MailGroupingService.BuildGroups(rows);

        //        gridPreview.DataSource = BuildPreviewTable(_mailGroups);
        //        lblStatus.Text = "Status: Loaded " + _mailGroups.Count + " mail group(s), total rows = " + _mailGroups.Sum(x => x.Rows.Count);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Load failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //    finally
        //    {
        //        btnLoad.Enabled = true;
        //        btnCreateDrafts.Enabled = true;
        //        Cursor = Cursors.Default;
        //    }
        //}
        private void BtnCreateDrafts_Click(object sender, EventArgs e)
        {
            try
            {
                if (_mailGroups == null || _mailGroups.Count == 0)
                {
                    MessageBox.Show("No mail groups available. Please load Excel first.");
                    return;
                }

                OutlookAccountInfo selectedAccount = cmbSenderAccount.SelectedItem as OutlookAccountInfo;
                if (selectedAccount == null)
                {
                    MessageBox.Show("Please select Outlook sender account.");
                    return;
                }

                int created = 0;

                Outlook.Application outlookApp = OutlookHelper.GetOutlookApplication();

                foreach (ProjectMailGroup group in _mailGroups)
                {
                    string cc = BuildCcList(selectedAccount.SmtpAddress);
                    //string htmlBody = HtmlMailBuilder.BuildBody(group.ProjectName, group.Rows);

                    Outlook.MailItem mail = (Outlook.MailItem)outlookApp.CreateItem(Outlook.OlItemType.olMailItem);

                    Outlook.Account sendAccount = OutlookHelper.FindAccountBySmtp(outlookApp, selectedAccount.SmtpAddress);
                    if (sendAccount != null)
                    {
                        mail.SendUsingAccount = sendAccount;
                    }

                    mail.Subject = "Details of components checked-in";
                    ////mail.To = group.ToEmail;
                    mail.To = string.Join(";", group.ToEmails.ToArray());
                    mail.CC = cc;
                    //mail.HTMLBody = htmlBody;
                    //mail.Display();
                    //mail.Save();
                    //mail.Display(true);

                    string htmlBody = HtmlMailBuilder.BuildBody(group.ProjectName, group.Rows);

                    // Step 1: open mail FIRST → Outlook inserts Sign1 automatically
                    mail.Display(false);

                    // Step 2: capture signature (Sign1)
                    string signature = mail.HTMLBody;

                    // Step 3: combine your content + signature
                    //mail.HTMLBody = htmlBody + "<br/><br/>" + signature;
                    mail.HTMLBody = htmlBody + signature;

                    // Step 4: show to user
                    mail.Save();
                    mail.Display(true);

                    // Uncomment below for auto-send later
                    // mail.Send();

                    created++;
                }

                lblStatus.Text = "Status: " + created + " Outlook draft mail(s) created.";
                MessageBox.Show(created + " draft mail(s) created in Outlook.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Draft creation failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshOutlookAccounts()
        {
            _outlookAccounts = OutlookHelper.GetAvailableAccounts();

            cmbSenderAccount.DataSource = null;
            cmbSenderAccount.DataSource = _outlookAccounts;
            cmbSenderAccount.DisplayMember = "DisplayText";
            cmbSenderAccount.ValueMember = "SmtpAddress";

            if (_outlookAccounts.Count > 0)
                cmbSenderAccount.SelectedIndex = 0;

            lblStatus.Text = "Status: Found " + _outlookAccounts.Count + " Outlook account(s).";
        }

        private string BuildCcList(string senderEmail)
        {
            return string.Join(";",
                _baseCcList
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Where(x => !string.Equals(x, senderEmail, StringComparison.OrdinalIgnoreCase))
                    .ToArray());
        }
        private DataTable BuildPreviewTable(List<ProjectMailGroup> groups)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Project Name");
            dt.Columns.Add("To Emails");
            dt.Columns.Add("Record Count");
            dt.Columns.Add("First Date");
            dt.Columns.Add("Last Date");
            dt.Columns.Add("Sample Component Path");

            foreach (ProjectMailGroup g in groups)
            {
                ExcelRowData first = g.Rows.OrderBy(x => x.Date).ThenBy(x => x.TimeText).FirstOrDefault();
                ExcelRowData last = g.Rows.OrderByDescending(x => x.Date).ThenByDescending(x => x.TimeText).FirstOrDefault();

                dt.Rows.Add(
                    g.ProjectName,
                    string.Join(";", g.ToEmails.ToArray()),
                    g.Rows.Count,
                    first != null ? first.Date.ToString("yyyy-MM-dd") : "",
                    last != null ? last.Date.ToString("yyyy-MM-dd") : "",
                    first != null ? first.ComponentPath : ""
                );
            }

            return dt;
        }
    }
}

////private DataTable BuildPreviewTable(List<ProjectMailGroup> groups)
////{
////    DataTable dt = new DataTable();
////    dt.Columns.Add("Project Name");
////    dt.Columns.Add("To Email");
////    dt.Columns.Add("Record Count");
////    dt.Columns.Add("First Date");
////    dt.Columns.Add("Last Date");
////    dt.Columns.Add("Sample Component Path");

////    foreach (ProjectMailGroup g in groups)
////    {
////        ExcelRowData first = g.Rows.OrderBy(x => x.Date).ThenBy(x => x.TimeText).FirstOrDefault();
////        ExcelRowData last = g.Rows.OrderByDescending(x => x.Date).ThenByDescending(x => x.TimeText).FirstOrDefault();

////        dt.Rows.Add(
////            g.ProjectName,
////            g.ToEmail,
////            g.Rows.Count,
////            first != null ? first.Date.ToString("yyyy-MM-dd") : "",
////            last != null ? last.Date.ToString("yyyy-MM-dd") : "",
////            first != null ? first.ComponentPath : ""
////        );
////    }

////    return dt;
////}
