//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ProjectWiseMailApp
//{
//    internal class OutlookHelper
//    {
//    }
//}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace ProjectWiseMailApp
{
    public static class OutlookHelper
    {
        public static Outlook.Application GetOutlookApplication()
        {
            try
            {
                return (Outlook.Application)Marshal.GetActiveObject("Outlook.Application");
            }
            catch
            {
                return new Outlook.Application();
            }
        }

        public static List<OutlookAccountInfo> GetAvailableAccounts()
        {
            List<OutlookAccountInfo> result = new List<OutlookAccountInfo>();

            Outlook.Application app = GetOutlookApplication();
            Outlook.NameSpace ns = app.Session;

            foreach (Outlook.Account account in ns.Accounts)
            {
                string smtp = GetAccountSmtpAddress(account);
                if (!string.IsNullOrWhiteSpace(smtp))
                {
                    OutlookAccountInfo info = new OutlookAccountInfo();
                    info.DisplayName = string.IsNullOrWhiteSpace(account.DisplayName) ? smtp : account.DisplayName;
                    info.SmtpAddress = smtp;
                    result.Add(info);
                }
            }

            return result
                .GroupBy(x => x.SmtpAddress, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(x => x.SmtpAddress)
                .ToList();
        }

        public static Outlook.Account FindAccountBySmtp(Outlook.Application app, string smtpAddress)
        {
            foreach (Outlook.Account account in app.Session.Accounts)
            {
                string smtp = GetAccountSmtpAddress(account);
                if (string.Equals(smtp, smtpAddress, StringComparison.OrdinalIgnoreCase))
                    return account;
            }

            return null;
        }

        public static string GetAccountSmtpAddress(Outlook.Account account)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(account.SmtpAddress))
                    return account.SmtpAddress.Trim();
            }
            catch
            {
            }

            try
            {
                Outlook.AddressEntry addressEntry = account.CurrentUser.AddressEntry;
                if (addressEntry != null)
                {
                    const string PR_SMTP_ADDRESS = "http://schemas.microsoft.com/mapi/proptag/0x39FE001E";
                    object smtpObj = addressEntry.PropertyAccessor.GetProperty(PR_SMTP_ADDRESS);
                    return smtpObj != null ? smtpObj.ToString().Trim() : "";
                }
            }
            catch
            {
            }

            return "";
        }
    }
}