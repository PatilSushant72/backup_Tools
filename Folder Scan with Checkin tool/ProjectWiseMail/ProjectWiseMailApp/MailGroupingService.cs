//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ProjectWiseMailApp
//{
//    internal class MailGroupingService
//    {
//    }
//}
////using System;
////using System.Collections.Generic;
////using System.Linq;

////namespace ProjectWiseMailApp
////{
////    public static class MailGroupingService
////    {
////        public static List<ProjectMailGroup> BuildGroups(List<ExcelRowData> rows)
////        {
////            List<ExcelRowData> updatedRows = rows.Select(delegate (ExcelRowData r)
////            {
////                r.ProjectName = ExtractProjectName(r.ComponentPath);
////                return r;
////            }).ToList();

////            return updatedRows
////                .GroupBy(r => new
////                {
////                    ProjectName = Normalize(r.ProjectName),
////                    ToEmail = Normalize(r.ToEmail)
////                })
////                .Select(g => new ProjectMailGroup
////                {
////                    ProjectName = g.First().ProjectName,
////                    ToEmail = g.First().ToEmail,
////                    Rows = g.OrderBy(x => x.Date)
////                            .ThenBy(x => x.TimeText)
////                            .ThenBy(x => x.ComponentPath)
////                            .ToList()
////                })
////                .OrderBy(x => x.ProjectName)
////                .ThenBy(x => x.ToEmail)
////                .ToList();
////        }

////        public static string ExtractProjectName(string componentPath)
////        {
////            if (string.IsNullOrWhiteSpace(componentPath))
////                return string.Empty;

////            string cleanedPath = componentPath.Trim();

////            if (cleanedPath.StartsWith("\""))
////                cleanedPath = cleanedPath.Substring(1);

////            if (cleanedPath.EndsWith("\""))
////                cleanedPath = cleanedPath.Substring(0, cleanedPath.Length - 1);

////            string[] parts = cleanedPath.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);

////            // After first "\" means second part
////            if (parts.Length >= 2)
////                return parts[1].Trim();

////            return cleanedPath;
////        }

////        private static string Normalize(string value)
////        {
////            if (value == null)
////                return "";

////            return value.Trim().ToLowerInvariant();
////        }
////    }
////}
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectWiseMailApp
{
    public static class MailGroupingService
    {
        public static List<ProjectMailGroup> BuildGroups(List<ExcelRowData> rows)
        {
            List<ExcelRowData> updatedRows = rows.Select(delegate (ExcelRowData r)
            {
                r.ProjectName = ExtractProjectName(r.ComponentPath);
                return r;
            }).ToList();

            return updatedRows
                .GroupBy(r => Normalize(r.ProjectName))
                .Select(g => new ProjectMailGroup
                {
                    ProjectName = g.First().ProjectName,
                    ToEmails = g.Select(x => x.ToEmail)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Select(x => x.Trim())
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .OrderBy(x => x)
                                .ToList(),
                    Rows = g.OrderBy(x => x.Date)
                            .ThenBy(x => x.TimeText)
                            .ThenBy(x => x.ComponentPath)
                            .ToList()
                })
                .OrderBy(x => x.ProjectName)
                .ToList();
        }

        public static string ExtractProjectName(string componentPath)
        {
            if (string.IsNullOrWhiteSpace(componentPath))
                return string.Empty;

            string cleanedPath = componentPath.Trim();

            if (cleanedPath.StartsWith("\""))
                cleanedPath = cleanedPath.Substring(1);

            if (cleanedPath.EndsWith("\""))
                cleanedPath = cleanedPath.Substring(0, cleanedPath.Length - 1);

            string[] parts = cleanedPath.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);

            // After first "\" means second part
            if (parts.Length >= 2)
                return parts[1].Trim();

            return cleanedPath;
        }

        private static string Normalize(string value)
        {
            if (value == null)
                return "";

            return value.Trim().ToLowerInvariant();
        }
    }
}