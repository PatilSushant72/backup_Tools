//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ProjectWiseMailApp
//{
//    internal class ExcelReader
//    {
//    }
//}
////using System;
////using System.Collections.Generic;
////using System.Globalization;
////using ClosedXML.Excel;

////namespace ProjectWiseMailApp
////{
////    public static class ExcelReader
////    {
////        public static List<ExcelRowData> ReadRows(string filePath)
////        {
////            List<ExcelRowData> result = new List<ExcelRowData>();

////            XLWorkbook workbook = new XLWorkbook(filePath);
////            try
////            {
////                IXLWorksheet worksheet = workbook.Worksheet(1);
////                IXLRange usedRange = worksheet.RangeUsed();

////                if (usedRange == null)
////                    throw new Exception("Worksheet is empty.");

////                int firstRow = usedRange.FirstRow().RowNumber();
////                int lastRow = usedRange.LastRow().RowNumber();

////                // Fixed columns based on your Excel layout
////                int dateCol = 1;
////                int timeCol = 2;
////                int versionCol = 3;
////                int componentPathCol = 4;
////                int componentNameCol = 5;
////                int emailCol = 6;

////                // Optional: validate first row looks like header
////                string h1 = GetCellText(worksheet, firstRow, dateCol).ToLowerInvariant();
////                string h2 = GetCellText(worksheet, firstRow, timeCol).ToLowerInvariant();
////                string h3 = GetCellText(worksheet, firstRow, versionCol).ToLowerInvariant();
////                string h4 = GetCellText(worksheet, firstRow, componentPathCol).ToLowerInvariant();
////                string h5 = GetCellText(worksheet, firstRow, componentNameCol).ToLowerInvariant();
////                string h6 = GetCellText(worksheet, firstRow, emailCol).ToLowerInvariant();

////                if (!(h1.Contains("date") &&
////                      h2.Contains("time") &&
////                      h3.Contains("version") &&
////                      h4.Contains("component") &&
////                      h5.Contains("component") &&
////                      h6.Contains("email")))
////                {
////                    throw new Exception(
////                        "Excel first row is not matching expected columns.\n" +
////                        "Expected: Date | Time | Version | Component Path | Component Name | Email\n" +
////                        "Found: " + h1 + " | " + h2 + " | " + h3 + " | " + h4 + " | " + h5 + " | " + h6);
////                }

////                for (int row = firstRow + 1; row <= lastRow; row++)
////                {
////                    string dateText = GetCellText(worksheet, row, dateCol);
////                    string timeText = GetCellText(worksheet, row, timeCol);
////                    string version = GetCellText(worksheet, row, versionCol);
////                    string componentPath = GetCellText(worksheet, row, componentPathCol);
////                    string componentName = GetCellText(worksheet, row, componentNameCol);
////                    string email = GetCellText(worksheet, row, emailCol);

////                    bool isBlank =
////                        string.IsNullOrWhiteSpace(dateText) &&
////                        string.IsNullOrWhiteSpace(timeText) &&
////                        string.IsNullOrWhiteSpace(version) &&
////                        string.IsNullOrWhiteSpace(componentPath) &&
////                        string.IsNullOrWhiteSpace(componentName) &&
////                        string.IsNullOrWhiteSpace(email);

////                    if (isBlank)
////                        continue;

////                    if (string.IsNullOrWhiteSpace(componentPath) || string.IsNullOrWhiteSpace(componentName))
////                        continue;

////                    if (string.IsNullOrWhiteSpace(email))
////                        continue;

////                    ExcelRowData item = new ExcelRowData();
////                    item.Date = ParseExcelDate(dateText);
////                    item.TimeText = timeText;
////                    item.Version = version;
////                    item.ComponentPath = CleanPath(componentPath);
////                    item.ComponentName = componentName;
////                    item.ToEmail = email.Trim();

////                    result.Add(item);
////                }
////            }
////            finally
////            {
////                workbook.Dispose();
////            }

////            return result;
////        }

////        private static string GetCellText(IXLWorksheet worksheet, int row, int col)
////        {
////            string text = worksheet.Cell(row, col).GetFormattedString();

////            if (string.IsNullOrWhiteSpace(text))
////                text = worksheet.Cell(row, col).GetString();

////            if (text == null)
////                return "";

////            return text.Trim();
////        }

////        private static string CleanPath(string value)
////        {
////            if (string.IsNullOrWhiteSpace(value))
////                return "";

////            string cleaned = value.Trim();

////            if (cleaned.StartsWith("\""))
////                cleaned = cleaned.Substring(1);

////            if (cleaned.EndsWith("\""))
////                cleaned = cleaned.Substring(0, cleaned.Length - 1);

////            return cleaned.Trim();
////        }

////        private static DateTime ParseExcelDate(string text)
////        {
////            if (string.IsNullOrWhiteSpace(text))
////                return DateTime.MinValue;

////            DateTime dt;
////            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
////                return dt.Date;

////            if (DateTime.TryParse(text, out dt))
////                return dt.Date;

////            double oa;
////            if (double.TryParse(text, out oa))
////                return DateTime.FromOADate(oa).Date;

////            return DateTime.MinValue;
////        }
////    }
////}

////////////using System;
////////////using System.Collections.Generic;
////////////using System.Globalization;
////////////using System.Linq;
////////////using ClosedXML.Excel;

////////////namespace ProjectWiseMailApp
////////////{
////////////    public static class ExcelReader
////////////    {
////////////        public static List<ExcelRowData> ReadRows(string filePath, bool onlyToday, DateTime fromDate, DateTime toDate)
////////////        //public static List<ExcelRowData> ReadRows(string filePath)
////////////        {
////////////            List<ExcelRowData> result = new List<ExcelRowData>();

////////////            XLWorkbook workbook = new XLWorkbook(filePath);
////////////            try
////////////            {
////////////                List<IXLWorksheet> targetSheets = workbook.Worksheets
////////////                    .Where(ws =>
////////////                       ws.Name.Equals("P1Proj", StringComparison.OrdinalIgnoreCase) ||
////////////                       ws.Name.Equals("P2Proj", StringComparison.OrdinalIgnoreCase))
////////////                    .ToList();

////////////                if (targetSheets.Count == 0)
////////////                    throw new Exception("No sheets found with name starting from P1Proj or P2Proj.");

////////////                foreach (IXLWorksheet worksheet in targetSheets)
////////////                {
////////////                    ReadWorksheetRows(worksheet, result);
////////////                }
////////////            }
////////////            finally
////////////            {
////////////                workbook.Dispose();
////////////            }

////////////            return result;
////////////        }

////////////        private static void ReadWorksheetRows(IXLWorksheet worksheet, List<ExcelRowData> result)
////////////        {
////////////            IXLRange usedRange = worksheet.RangeUsed();

////////////            if (usedRange == null)
////////////                return;

////////////            int firstRow = usedRange.FirstRow().RowNumber();
////////////            int lastRow = usedRange.LastRow().RowNumber();

////////////            // Fixed columns based on your Excel layout
////////////            int dateCol = 1;
////////////            int timeCol = 2;
////////////            int versionCol = 3;
////////////            int componentPathCol = 4;
////////////            int componentNameCol = 5;
////////////            int projNameChangeCol = 6;
////////////            int logicChangeCol = 7;
////////////            int emailCol = 8;

////////////            string h1 = GetCellText(worksheet, firstRow, dateCol).ToLowerInvariant();
////////////            string h2 = GetCellText(worksheet, firstRow, timeCol).ToLowerInvariant();
////////////            string h3 = GetCellText(worksheet, firstRow, versionCol).ToLowerInvariant();
////////////            string h4 = GetCellText(worksheet, firstRow, componentPathCol).ToLowerInvariant();
////////////            string h5 = GetCellText(worksheet, firstRow, componentNameCol).ToLowerInvariant();
////////////            string h6 = GetCellText(worksheet, firstRow, projNameChangeCol).ToLowerInvariant();
////////////            string h7 = GetCellText(worksheet, firstRow, logicChangeCol).ToLowerInvariant();
////////////            string h8 = GetCellText(worksheet, firstRow, emailCol).ToLowerInvariant();

////////////            if (!(h1.Contains("date") &&
////////////                  h2.Contains("time") &&
////////////                  h3.Contains("version") &&
////////////                  h4.Contains("component") &&
////////////                  h5.Contains("component") &&
////////////                  h6.Contains("proj") &&
////////////                  h7.Contains("logic") &&
////////////                  h8.Contains("email")))
////////////            {
////////////                throw new Exception(
////////////                    "Sheet '" + worksheet.Name + "' first row is not matching expected columns.\n" +
////////////                    "Expected: Date | Time | Version | Component Path | Component Name | ProjNameChange | LogicChange | Email\n" +
////////////                    "Found: " + h1 + " | " + h2 + " | " + h3 + " | " + h4 + " | " + h5 + " | " + h6 + " | " + h7 + " | " + h8);
////////////            }

////////////            for (int row = firstRow + 1; row <= lastRow; row++)
////////////            {
////////////                string dateText = GetCellText(worksheet, row, dateCol);
////////////                string timeText = GetCellText(worksheet, row, timeCol);
////////////                string version = GetCellText(worksheet, row, versionCol);
////////////                string componentPath = GetCellText(worksheet, row, componentPathCol);
////////////                string componentName = GetCellText(worksheet, row, componentNameCol);
////////////                string projNameChange = GetCellText(worksheet, row, projNameChangeCol);
////////////                string logicChange = GetCellText(worksheet, row, logicChangeCol);
////////////                string email = GetCellText(worksheet, row, emailCol);

////////////                bool isBlank =
////////////                    string.IsNullOrWhiteSpace(dateText) &&
////////////                    string.IsNullOrWhiteSpace(timeText) &&
////////////                    string.IsNullOrWhiteSpace(version) &&
////////////                    string.IsNullOrWhiteSpace(componentPath) &&
////////////                    string.IsNullOrWhiteSpace(componentName) &&
////////////                    string.IsNullOrWhiteSpace(projNameChange) &&
////////////                    string.IsNullOrWhiteSpace(logicChange) &&
////////////                    string.IsNullOrWhiteSpace(email);

////////////                if (isBlank)
////////////                    continue;

////////////                if (string.IsNullOrWhiteSpace(componentPath) || string.IsNullOrWhiteSpace(componentName))
////////////                    continue;

////////////                if (string.IsNullOrWhiteSpace(email))
////////////                    continue;

////////////                ExcelRowData item = new ExcelRowData();
////////////                item.Date = ParseExcelDate(dateText);
////////////                item.TimeText = timeText;
////////////                item.Version = version;
////////////                item.ComponentPath = CleanPath(componentPath);
////////////                item.ComponentName = componentName;
////////////                item.ToEmail = email.Trim();

////////////                // If you added these properties in ExcelRowData, uncomment:
////////////                // item.ProjNameChange = projNameChange;
////////////                // item.LogicChange = logicChange;

////////////                result.Add(item);
////////////            }
////////////        }

////////////        private static string GetCellText(IXLWorksheet worksheet, int row, int col)
////////////        {
////////////            string text = worksheet.Cell(row, col).GetFormattedString();

////////////            if (string.IsNullOrWhiteSpace(text))
////////////                text = worksheet.Cell(row, col).GetString();

////////////            if (text == null)
////////////                return "";

////////////            return text.Trim();
////////////        }

////////////        private static string CleanPath(string value)
////////////        {
////////////            if (string.IsNullOrWhiteSpace(value))
////////////                return "";

////////////            string cleaned = value.Trim();

////////////            if (cleaned.StartsWith("\""))
////////////                cleaned = cleaned.Substring(1);

////////////            if (cleaned.EndsWith("\""))
////////////                cleaned = cleaned.Substring(0, cleaned.Length - 1);

////////////            return cleaned.Trim();
////////////        }

////////////        private static DateTime ParseExcelDate(string text)
////////////        {
////////////            if (string.IsNullOrWhiteSpace(text))
////////////                return DateTime.MinValue;

////////////            DateTime dt;
////////////            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
////////////                return dt.Date;

////////////            if (DateTime.TryParse(text, out dt))
////////////                return dt.Date;

////////////            double oa;
////////////            if (double.TryParse(text, out oa))
////////////                return DateTime.FromOADate(oa).Date;

////////////            return DateTime.MinValue;
////////////        }
////////////    }
////////////}





//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using ClosedXML.Excel;

//namespace ProjectWiseMailApp
//{
//    public static class ExcelReader
//    {
//        public static List<ExcelRowData> ReadRows(string filePath, bool onlyToday, DateTime fromDate, DateTime toDate)
//        {
//            List<ExcelRowData> result = new List<ExcelRowData>();

//            XLWorkbook workbook = new XLWorkbook(filePath);
//            try
//            {
//                List<IXLWorksheet> targetSheets = workbook.Worksheets
//                    .Where(ws =>
//                        ws.Name.Equals("P1Proj", StringComparison.OrdinalIgnoreCase) ||
//                        ws.Name.Equals("P2Proj", StringComparison.OrdinalIgnoreCase))
//                    .ToList();

//                if (targetSheets.Count == 0)
//                    throw new Exception("No sheets found with name P1Proj or P2Proj.");

//                foreach (IXLWorksheet worksheet in targetSheets)
//                {
//                    ReadWorksheetRows(worksheet, result, onlyToday, fromDate, toDate);
//                }
//            }
//            finally
//            {
//                workbook.Dispose();
//            }

//            return result;
//        }

//        private static void ReadWorksheetRows(IXLWorksheet worksheet, List<ExcelRowData> result, bool onlyToday, DateTime fromDate, DateTime toDate)
//        {
//            IXLRange usedRange = worksheet.RangeUsed();

//            if (usedRange == null)
//                return;

//            int firstRow = usedRange.FirstRow().RowNumber();
//            int lastRow = usedRange.LastRow().RowNumber();

//            int dateCol = 1;
//            int timeCol = 2;
//            int versionCol = 3;
//            int componentPathCol = 4;
//            int componentNameCol = 5;
//            int projNameChangeCol = 6;
//            int logicChangeCol = 7;
//            int emailCol = 8;

//            string h1 = GetCellText(worksheet, firstRow, dateCol).ToLowerInvariant();
//            string h2 = GetCellText(worksheet, firstRow, timeCol).ToLowerInvariant();
//            string h3 = GetCellText(worksheet, firstRow, versionCol).ToLowerInvariant();
//            string h4 = GetCellText(worksheet, firstRow, componentPathCol).ToLowerInvariant();
//            string h5 = GetCellText(worksheet, firstRow, componentNameCol).ToLowerInvariant();
//            string h6 = GetCellText(worksheet, firstRow, projNameChangeCol).ToLowerInvariant();
//            string h7 = GetCellText(worksheet, firstRow, logicChangeCol).ToLowerInvariant();
//            string h8 = GetCellText(worksheet, firstRow, emailCol).ToLowerInvariant();

//            if (!(h1.Contains("date") &&
//                  h2.Contains("time") &&
//                  h3.Contains("version") &&
//                  h4.Contains("component") &&
//                  h5.Contains("component") &&
//                  h6.Contains("proj") &&
//                  h7.Contains("logic") &&
//                  h8.Contains("email")))
//            {
//                throw new Exception(
//                    "Sheet '" + worksheet.Name + "' first row is not matching expected columns.\n" +
//                    "Expected: Date | Time | Version | Component Path | Component Name | ProjNameChange | LogicChange | Email\n" +
//                    "Found: " + h1 + " | " + h2 + " | " + h3 + " | " + h4 + " | " + h5 + " | " + h6 + " | " + h7 + " | " + h8);
//            }

//            for (int row = firstRow + 1; row <= lastRow; row++)
//            {
//                string dateText = GetCellText(worksheet, row, dateCol);
//                string timeText = GetCellText(worksheet, row, timeCol);
//                string version = GetCellText(worksheet, row, versionCol);
//                string componentPath = GetCellText(worksheet, row, componentPathCol);
//                string componentName = GetCellText(worksheet, row, componentNameCol);
//                string projNameChange = GetCellText(worksheet, row, projNameChangeCol);
//                string logicChange = GetCellText(worksheet, row, logicChangeCol);
//                string email = GetCellText(worksheet, row, emailCol);

//                bool isBlank =
//                    string.IsNullOrWhiteSpace(dateText) &&
//                    string.IsNullOrWhiteSpace(timeText) &&
//                    string.IsNullOrWhiteSpace(version) &&
//                    string.IsNullOrWhiteSpace(componentPath) &&
//                    string.IsNullOrWhiteSpace(componentName) &&
//                    string.IsNullOrWhiteSpace(projNameChange) &&
//                    string.IsNullOrWhiteSpace(logicChange) &&
//                    string.IsNullOrWhiteSpace(email);

//                if (isBlank)
//                    continue;

//                if (string.IsNullOrWhiteSpace(componentPath) || string.IsNullOrWhiteSpace(componentName))
//                    continue;

//                if (string.IsNullOrWhiteSpace(email))
//                    continue;

//                DateTime parsedDate = ParseExcelDate(dateText);
//                if (parsedDate == DateTime.MinValue)
//                    continue;

//                if (onlyToday)
//                {
//                    if (parsedDate.Date != DateTime.Today)
//                        continue;
//                }
//                else
//                {
//                    if (parsedDate.Date < fromDate.Date || parsedDate.Date > toDate.Date)
//                        continue;
//                }

//                ExcelRowData item = new ExcelRowData();
//                item.Date = parsedDate;
//                item.TimeText = timeText;
//                item.Version = version;
//                item.ComponentPath = CleanPath(componentPath);
//                item.ComponentName = componentName;
//                item.ToEmail = email.Trim();

//                // If added in ExcelRowData:
//                // item.ProjNameChange = projNameChange;
//                // item.LogicChange = logicChange;

//                result.Add(item);
//            }
//        }

//        private static string GetCellText(IXLWorksheet worksheet, int row, int col)
//        {
//            string text = worksheet.Cell(row, col).GetFormattedString();

//            if (string.IsNullOrWhiteSpace(text))
//                text = worksheet.Cell(row, col).GetString();

//            if (text == null)
//                return "";

//            return text.Trim();
//        }

//        private static string CleanPath(string value)
//        {
//            if (string.IsNullOrWhiteSpace(value))
//                return "";

//            string cleaned = value.Trim();

//            if (cleaned.StartsWith("\""))
//                cleaned = cleaned.Substring(1);

//            if (cleaned.EndsWith("\""))
//                cleaned = cleaned.Substring(0, cleaned.Length - 1);

//            return cleaned.Trim();
//        }

//        private static DateTime ParseExcelDate(string text)
//        {
//            if (string.IsNullOrWhiteSpace(text))
//                return DateTime.MinValue;

//            DateTime dt;
//            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
//                return dt.Date;

//            if (DateTime.TryParse(text, out dt))
//                return dt.Date;

//            double oa;
//            if (double.TryParse(text, out oa))
//                return DateTime.FromOADate(oa).Date;

//            return DateTime.MinValue;
//        }
//    }
//}





using ExcelDataReader;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;

using OfficeOpenXml;

//using LicenseContext = OfficeOpenXml.LicenseContext;
namespace ProjectWiseMailApp
{
    public static class ExcelReader
    {
     
        public static List<ExcelRowData> ReadRows(string filePath, bool onlyToday, DateTime fromDate, DateTime toDate)
        {
            //bool p1EmailMissing = false;
            //bool p2EmailMissing = false;
            bool p1EmailFound = false;
            bool p2EmailFound = false;
            List<ExcelRowData> result = new List<ExcelRowData>();

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
            {
                do
                {
                    string sheetName = reader.Name;

                    if (!sheetName.Equals("P1Proj", StringComparison.OrdinalIgnoreCase) &&
                        !sheetName.Equals("P2Proj", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    bool isHeaderRead = false;

                    while (reader.Read())
                    {
                        // Skip completely empty rows
                        if (IsRowEmpty(reader, 8))
                            continue;

                        //if (!isHeaderRead)
                        //{
                        //    isHeaderRead = true;
                        //    continue; // first row = header
                        //}
                        if (!isHeaderRead)
                        {
                            isHeaderRead = true;

                            if (reader.FieldCount < 8)
                            {
                                ShowExcelFormatPopup();
                                return result;
                            }

                            continue;
                        }
                        string dateText = GetDateValue(reader, 0);
                        string timeText = GetTimeValue(reader, 1);
                        string version = GetValue(reader, 2);
                        string componentPath = CleanPath(GetValue(reader, 3));
                        string componentName = GetValue(reader, 4);
                        string projNameChange = GetValue(reader, 5);
                        string logicChange = GetValue(reader, 6);
                        string email = GetValue(reader, 7);

                        if (string.IsNullOrWhiteSpace(componentPath) || string.IsNullOrWhiteSpace(componentName))
                            continue;

                        //if (string.IsNullOrWhiteSpace(email))
                        //    continue;
                        //if (string.IsNullOrWhiteSpace(email))
                        //{
                        //    if (sheetName.Equals("P1Proj", StringComparison.OrdinalIgnoreCase))
                        //        p1EmailMissing = true;

                        //    if (sheetName.Equals("P2Proj", StringComparison.OrdinalIgnoreCase))
                        //        p2EmailMissing = true;

                        //    continue;
                        //}


                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            if (sheetName.Equals("P1Proj", StringComparison.OrdinalIgnoreCase))
                                p1EmailFound = true;

                            if (sheetName.Equals("P2Proj", StringComparison.OrdinalIgnoreCase))
                                p2EmailFound = true;
                        }
                        else
                        {
                            continue;
                        }

                        DateTime parsedDate = ParseExcelDate(reader.GetValue(0), dateText);
                        if (parsedDate == DateTime.MinValue)
                            continue;

                        if (onlyToday)
                        {
                            if (parsedDate.Date != DateTime.Today)
                                continue;
                        }
                        else
                        {
                            if (parsedDate.Date < fromDate.Date || parsedDate.Date > toDate.Date)
                                continue;
                        }

                        ExcelRowData item = new ExcelRowData();
                        item.Date = parsedDate;
                        item.DateText = dateText;
                        item.TimeText = timeText;
                        item.Version = version;
                        item.ComponentPath = componentPath;
                        item.ComponentName = componentName;
                        item.ToEmail = email.Trim();

                        // If you added these properties in ExcelRowData, uncomment:
                        // item.ProjNameChange = projNameChange;
                        // item.LogicChange = logicChange;

                        result.Add(item);
                    }
                }
                while (reader.NextResult());
            }
            //if (p1EmailMissing && p2EmailMissing)
            //{
            //    System.Windows.Forms.MessageBox.Show(
            //        "Email not found in both sheets (P1Proj and P2Proj).",
            //        "Email Missing",
            //        System.Windows.Forms.MessageBoxButtons.OK,
            //        System.Windows.Forms.MessageBoxIcon.Warning);
            //}
            //else if (p1EmailMissing)
            //{
            //    System.Windows.Forms.MessageBox.Show(
            //        "Email not found in P1Proj sheet.",
            //        "Email Missing",
            //        System.Windows.Forms.MessageBoxButtons.OK,
            //        System.Windows.Forms.MessageBoxIcon.Warning);
            //}
            //else if (p2EmailMissing)
            //{
            //    System.Windows.Forms.MessageBox.Show(
            //        "Email not found in P2Proj sheet.",
            //        "Email Missing",
            //        System.Windows.Forms.MessageBoxButtons.OK,
            //        System.Windows.Forms.MessageBoxIcon.Warning);
            //}


            if (!p1EmailFound && !p2EmailFound)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Email not found in both sheets (P1Proj and P2Proj).",
                    "Email Missing",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);
            }
            else if (!p1EmailFound)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Email not found in P1Proj sheet.",
                    "Email Missing",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);
            }
            else if (!p2EmailFound)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Email not found in P2Proj sheet.",
                    "Email Missing",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);
            }
            return result;
        }
        private static void ShowExcelFormatPopup()
        {
            var result = System.Windows.Forms.MessageBox.Show(
                "Excel columns do not match the required format.\n\n" +
                "Required Columns:\n" +
                "1. Date\n" +
                "2. Time\n" +
                "3. Version\n" +
                "4. ComponentPath\n" +
                "5. ComponentName\n" +
                "6. ProjNameChange\n" +
                "7. LogicChange\n" +
                "8. Email\n\n" +
                "Click YES to download Excel format template.",
                "Excel Format Error",
                System.Windows.Forms.MessageBoxButtons.YesNo,
                System.Windows.Forms.MessageBoxIcon.Warning);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                DownloadExcelTemplate();
            }
        }
        //private static void DownloadExcelTemplate()
        //{
        //    try
        //    {
        //        string path = Path.Combine(
        //            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        //            "Excel_Template.csv");

        //        string header =
        //            "Date,Time,Version,ComponentPath,Component Name,ProjNameChange,LogicChange,Email";

        //        File.WriteAllText(path, header);

        //        System.Windows.Forms.MessageBox.Show(
        //            "Excel template downloaded to Desktop.",
        //            "Download Complete",
        //            System.Windows.Forms.MessageBoxButtons.OK,
        //            System.Windows.Forms.MessageBoxIcon.Information);

        //        System.Diagnostics.Process.Start(path);
        //    }
        //    catch
        //    {
        //        System.Windows.Forms.MessageBox.Show(
        //            "Unable to create Excel template.",
        //            "Error",
        //            System.Windows.Forms.MessageBoxButtons.OK,
        //            System.Windows.Forms.MessageBoxIcon.Error);
        //    }
        //}

private static void DownloadExcelTemplate()
    {
        try
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "Excel_Template.xlsx");

            using (ExcelPackage package = new ExcelPackage())
            {
                string[] headers =
                {
                "Date",
                "Time",
                "Version",
                "ComponentPath",
                "Component Name",
                "ProjNameChange",
                "LogicChange",
                "Email"
            };

                // ---------- P1Proj Sheet ----------
                var sheet1 = package.Workbook.Worksheets.Add("P1Proj");

                for (int i = 0; i < headers.Length; i++)
                {
                    sheet1.Cells[1, i + 1].Value = headers[i];
                    sheet1.Cells[1, i + 1].Style.Font.Bold = true;
                }

                sheet1.Cells.AutoFitColumns();

                // ---------- P2Proj Sheet ----------
                var sheet2 = package.Workbook.Worksheets.Add("P2Proj");

                for (int i = 0; i < headers.Length; i++)
                {
                    sheet2.Cells[1, i + 1].Value = headers[i];
                    sheet2.Cells[1, i + 1].Style.Font.Bold = true;
                }

                sheet2.Cells.AutoFitColumns();

                FileInfo file = new FileInfo(path);
                package.SaveAs(file);
            }

            System.Windows.Forms.MessageBox.Show(
                "Excel template downloaded to Desktop.",
                "Download Complete",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Information);

            System.Diagnostics.Process.Start(path);
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(
                "Unable to create Excel template.\n\n" + ex.Message,
                "Error",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Error);
        }
    }
    private static string GetTimeValue(IExcelDataReader reader, int index)
        {
            object value = reader.GetValue(index);

            if (value == null)
                return "";

            try
            {
                if (value is DateTime)
                    return ((DateTime)value).ToString("h:mm:ss tt");

                double oa;
                if (double.TryParse(value.ToString(), out oa))
                {
                    DateTime dt = DateTime.FromOADate(oa);
                    return dt.ToString("h:mm:ss tt");
                }

                DateTime parsed;
                if (DateTime.TryParse(value.ToString(), out parsed))
                    return parsed.ToString("h:mm:ss tt");

                return value.ToString().Trim();
            }
            catch
            {
                return value.ToString().Trim();
            }
        }
        private static string GetValue(IExcelDataReader reader, int index)
        {
            if (index >= reader.FieldCount)
                return "";

            object value = reader.GetValue(index);
            return value == null ? "" : value.ToString().Trim();
        }


        //private static string GetValue(IExcelDataReader reader, int index)
        //{
        //    object value = reader.GetValue(index);
        //    return value == null ? "" : value.ToString().Trim();
        //}

        private static string GetDateValue(IExcelDataReader reader, int index)
        {
            object value = reader.GetValue(index);

            if (value == null)
                return "";

            try
            {
                if (value is DateTime)
                    return ((DateTime)value).ToString("dd-MMM-yyyy");

                double oa;
                if (double.TryParse(value.ToString(), out oa))
                {
                    DateTime dt = DateTime.FromOADate(oa);
                    return dt.ToString("dd-MMM-yyyy");
                }

                DateTime parsed;
                if (DateTime.TryParse(value.ToString(), out parsed))
                    return parsed.ToString("dd-MMM-yyyy");

                return value.ToString().Trim();
            }
            catch
            {
                return value.ToString().Trim();
            }
        }

        private static bool IsRowEmpty(IExcelDataReader reader, int maxColumns)
        {
            for (int i = 0; i < maxColumns; i++)
            {
                object value = reader.GetValue(i);
                if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                    return false;
            }

            return true;
        }

        private static string CleanPath(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            string cleaned = value.Trim();

            if (cleaned.StartsWith("\""))
                cleaned = cleaned.Substring(1);

            if (cleaned.EndsWith("\""))
                cleaned = cleaned.Substring(0, cleaned.Length - 1);

            return cleaned.Trim();
        }

        private static DateTime ParseExcelDate(object rawValue, string text)
        {
            try
            {
                if (rawValue is DateTime)
                    return ((DateTime)rawValue).Date;

                double oa;
                if (rawValue != null && double.TryParse(rawValue.ToString(), out oa))
                    return DateTime.FromOADate(oa).Date;

                DateTime dt;
                if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                    return dt.Date;

                if (DateTime.TryParse(text, out dt))
                    return dt.Date;

                return DateTime.MinValue;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }
}