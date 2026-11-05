//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FolderScan
//{
//    internal class ExcelHelper
//    {
//    }
//}
////////using ExcelDataReader;
////////using System.Data;
////////using System.IO;
////////using System.Windows;

////////namespace FolderScan
////////{
////////    public static class ExcelHelper
////////    {
////////        public static DataTable ReadExcel(string filePath, string sheetName)
////////        {
////////            if (!File.Exists(filePath))
////////            {
////////                MessageBox.Show(
////////                    $"Excel file not found:\n\n{filePath}",
////////                    "File Not Found",
////////                    MessageBoxButton.OK,
////////                    MessageBoxImage.Error
////////                );

////////            }
////////            // REQUIRED for ExcelDataReader
////////            System.Text.Encoding.RegisterProvider(
////////                System.Text.CodePagesEncodingProvider.Instance);

////////            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))

////////            using (var reader = ExcelReaderFactory.CreateReader(stream))
////////            {
////////                var ds = reader.AsDataSet(new ExcelDataSetConfiguration
////////                {
////////                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
////////                    {
////////                        UseHeaderRow = true
////////                    }
////////                });

////////                return ds.Tables[sheetName];
////////            }
////////        }
////////    }
////////}

using ExcelDataReader;
using System;
using System.Data;
using System.IO;
using System.Windows;

namespace FolderScan
{
    public static class ExcelHelper
    {
        private static DataTable FixVersionColumn(DataTable table)
        {
            if (table == null || !table.Columns.Contains("Version"))
                return table;

            // If Version is already string, no need to change
            if (table.Columns["Version"].DataType == typeof(string))
                return table;

            DataTable newTable = table.Clone();

            // 🔹 Force Version column to string
            newTable.Columns["Version"].DataType = typeof(string);

            foreach (DataRow row in table.Rows)
            {
                DataRow newRow = newTable.NewRow();

                foreach (DataColumn col in table.Columns)
                {
                    if (col.ColumnName == "Version")
                    {
                        newRow[col.ColumnName] =
                            row[col] == DBNull.Value ? string.Empty : row[col].ToString();
                    }
                    else
                    {
                        newRow[col.ColumnName] = row[col];
                    }
                }

                newTable.Rows.Add(newRow);
            }

            return newTable;
        }

        // 🔹 EXISTING METHOD (UNCHANGED)
        public static DataTable ReadExcel(string filePath, string sheetName)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show(
                    $"Excel file not found:\n\n{filePath}",
                    "File Not Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                return null;
            }

            System.Text.Encoding.RegisterProvider(
                System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var ds = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                });
                return FixVersionColumn(ds.Tables[sheetName]);

                //return ds.Tables[sheetName];
            }
        }

        // 🔹 NEW METHOD: READ MULTIPLE SHEETS & MERGE
        public static DataTable ReadExcelMultipleSheets(
            string filePath,
            params string[] sheetNames)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show(
                    $"Excel file not found:\n\n{filePath}",
                    "File Not Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return null;
            }

            System.Text.Encoding.RegisterProvider(
                System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var ds = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                });

                DataTable mergedTable = null;

                foreach (var sheet in sheetNames)
                {
                    if (!ds.Tables.Contains(sheet))
                        continue;

                    var table = ds.Tables[sheet];

                    if (mergedTable == null)
                    {
                        //mergedTable = table.Clone(); // copy structure
                        mergedTable = FixVersionColumn(table).Clone();

                    }

                    //foreach (DataRow row in table.Rows)
                    //{
                    //    mergedTable.ImportRow(row);
                    //}
                    var fixedTable = FixVersionColumn(table);

                    foreach (DataRow row in fixedTable.Rows)
                    {
                        mergedTable.Rows.Add(row.ItemArray);
                    }

                }

                return mergedTable;
            }
        }
    }
}
