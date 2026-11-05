//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ProjectWiseMailApp
//{
//    internal class ExcelRowData
//    {
//    }
//}
using System;

namespace ProjectWiseMailApp
{
    public class ExcelRowData
    {
        public DateTime Date { get; set; }
        public string TimeText { get; set; }
        public string Version { get; set; }
        public string ComponentPath { get; set; }
        public string ComponentName { get; set; }
        public string ToEmail { get; set; }
        public string ProjectName { get; set; }
        public string DateText { get; set; }
        public ExcelRowData()
        {
            TimeText = "";
            Version = "";
            ComponentPath = "";
            ComponentName = "";
            ToEmail = "";
            ProjectName = "";
        }
    }
}