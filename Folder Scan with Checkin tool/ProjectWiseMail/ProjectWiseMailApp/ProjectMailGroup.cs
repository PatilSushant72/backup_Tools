//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ProjectWiseMailApp
//{
//    internal class ProjectMailGroup
//    {
//    }
//}
////using System.Collections.Generic;

////namespace ProjectWiseMailApp
////{
////    public class ProjectMailGroup
////    {
////        public string ProjectName { get; set; }
////        public string ToEmail { get; set; }
////        public List<ExcelRowData> Rows { get; set; }

////        public ProjectMailGroup()
////        {
////            ProjectName = "";
////            ToEmail = "";
////            Rows = new List<ExcelRowData>();
////        }
////    }
////}
using System.Collections.Generic;

namespace ProjectWiseMailApp
{
    public class ProjectMailGroup
    {
        public string ProjectName { get; set; }
        public List<string> ToEmails { get; set; }
        public List<ExcelRowData> Rows { get; set; }

        public ProjectMailGroup()
        {
            ProjectName = "";
            ToEmails = new List<string>();
            Rows = new List<ExcelRowData>();
        }
    }
}