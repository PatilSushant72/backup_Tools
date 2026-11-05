//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ProjectWiseMailApp
//{
//    internal class HtmlMailBuilder
//    {
//    }
//}
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ProjectWiseMailApp
{
    public static class HtmlMailBuilder
    {
        public static string BuildBody(string projectName, List<ExcelRowData> rows)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<html><body style='font-family:Calibri;font-size:11pt;'>");
            sb.Append("<p>Team,</p>");

            sb.Append("<p>");
            sb.Append("Following table lists details of components checked-in today, in Octoplant from respective folder of cloud service (OneDrive/Dropbox).");
            sb.Append("<br/>");
            sb.Append("You may forward this email to concerned site person, for information.");
            sb.Append("</p>");

            sb.Append("<p><b>Project:</b> " + Html(projectName) + "</p>");

            sb.Append("<table border='1' cellspacing='0' cellpadding='5' style='border-collapse:collapse;'>");
            sb.Append("<thead style='font-weight:bold;background-color:#D9EAF7;'>");
            sb.Append("<tr>");
            sb.Append("<th>Date</th>");
            sb.Append("<th>Time</th>");
            sb.Append("<th>Version</th>");
            sb.Append("<th>Component Path</th>");
            sb.Append("<th>Component Name</th>");
            sb.Append("</tr>");
            sb.Append("</thead>");
            sb.Append("<tbody>");

            foreach (ExcelRowData row in rows)
            {
                sb.Append("<tr>");
                sb.Append("<td>" + Html(row.DateText) + "</td>");
                //sb.Append("<td>" + Html(row.Date == System.DateTime.MinValue ? "" : row.Date.ToString("yyyy-MM-dd")) + "</td>");
                sb.Append("<td>" + Html(row.TimeText) + "</td>");
                sb.Append("<td>" + Html(row.Version) + "</td>");
                sb.Append("<td>" + Html(row.ComponentPath) + "</td>");
                sb.Append("<td>" + Html(row.ComponentName) + "</td>");
                sb.Append("</tr>");
            }

            sb.Append("</tbody>");
            sb.Append("</table>");
            //sb.Append("<br/>");
            //sb.Append("<p>Regards,<br/>System Generated Draft</p>");
            sb.Append("</body></html>");

            return sb.ToString();
        }

        private static string Html(string value)
        {
            return WebUtility.HtmlEncode(value ?? "");
        }
    }
}