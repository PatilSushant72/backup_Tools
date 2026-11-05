//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ProjectWiseMailApp
//{
//    internal class OutlookAccountInfo
//    {
//    }
//}
namespace ProjectWiseMailApp
{
    public class OutlookAccountInfo
    {
        public string DisplayName { get; set; }
        public string SmtpAddress { get; set; }

        public OutlookAccountInfo()
        {
            DisplayName = "";
            SmtpAddress = "";
        }

        public string DisplayText
        {
            get { return DisplayName + " (" + SmtpAddress + ")"; }
        }
    }
}