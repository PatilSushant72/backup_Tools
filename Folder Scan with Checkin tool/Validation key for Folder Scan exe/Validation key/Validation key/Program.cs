//using System;
//using System.Text;

//namespace Validation_key
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            Console.WriteLine("Enter your Application Key :");
//            string applicationKey = Console.ReadLine()?.Trim();

//            if (string.IsNullOrWhiteSpace(applicationKey))
//            {
//                Console.WriteLine("Invalid Application Key entered.");
//                return;
//            }

//            try
//            {
//                applicationKey = applicationKey.ToUpperInvariant();

//                // Decode Application Key
//                string rawData = FromHexString(applicationKey.Replace("-", ""));
//                var parts = rawData.Split('.');

//                if (parts.Length < 6 || parts[1] != "APL" || !rawData.Contains("wipropari"))
//                {
//                    Console.WriteLine("Application key is not valid.");
//                    return;
//                }

//                // Generate NEW validation key (+15 days)
//                string validationKey = GenerateValidationKey(parts);

//                Console.WriteLine("\nApplication Key : " + applicationKey);
//                Console.WriteLine("Validation Key  : " + validationKey);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Error processing Application Key : " + ex.Message);
//            }

//            Console.WriteLine("\nPress any key to exit...");
//            Console.ReadKey();
//        }

//        // ================= VALIDATION KEY =================
//        static string GenerateValidationKey(string[] appParts)
//        {
//            string mac = appParts[0];
//            string device = appParts[2];
//            string pc = appParts[3];

//            // ADMIN decides validity
//            DateTime expiry = DateTime.Now.AddDays(15);
//            string expiryDateTime = expiry.ToString("yyyyMMddHHmmss");

//            string validationData =
//                $"{mac}.VAL.{device}.PC.{expiryDateTime}.wipropari";

//            return AddDash(ToHexString(validationData));
//        }

//        // ================= HELPERS =================
//        public static string ToHexString(string str)
//        {
//            StringBuilder sb = new StringBuilder();
//            byte[] bytes = Encoding.UTF8.GetBytes(str);

//            foreach (byte b in bytes)
//            {
//                sb.Append(b.ToString("X2"));
//            }

//            return sb.ToString();
//        }

//        public static string FromHexString(string hexString)
//        {
//            byte[] bytes = new byte[hexString.Length / 2];

//            for (int i = 0; i < bytes.Length; i++)
//            {
//                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
//            }

//            return Encoding.UTF8.GetString(bytes);
//        }

//        public static string AddDash(string str)
//        {
//            StringBuilder sb = new StringBuilder();

//            for (int i = 0; i < str.Length; i++)
//            {
//                if (i % 4 == 0 && i != 0)
//                    sb.Append('-');

//                sb.Append(str[i]);
//            }

//            return sb.ToString();
//        }
//    }
//}
using System;
using System.Text;

namespace ValidationKeyGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter your Application Key :");
            string applicationKey = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(applicationKey))
            {
                Console.WriteLine("Invalid Application Key entered.");
                return;
            }

            try
            {
                applicationKey = applicationKey.ToUpperInvariant();

                // Decode Application Key
                string rawData = FromHexString(applicationKey.Replace("-", ""));
                var parts = rawData.Split('.');

                // Expected:
                // MAC.APL.PC.DEVICE.PC.REQUESTTIME.wipropari
                if (parts.Length < 7 || parts[1] != "APL" || !rawData.EndsWith("wipropari", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Application key is not valid.");
                    return;
                }

                // Generate Validation Key (+15 days)
                string validationKey = GenerateValidationKey(parts);

                Console.WriteLine("\nApplication Key : " + applicationKey);
                Console.WriteLine("Validation Key  : " + validationKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing Application Key : " + ex.Message);
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        // ================= VALIDATION KEY =================
        static string GenerateValidationKey(string[] appParts)
        {
            // From Application Key
            string mac = appParts[0];
            string pc = appParts[2];
            string device = appParts[3];

            // ADMIN decides validity
            DateTime expiry = DateTime.Now.AddHours(9);
            string expiryDateTime = expiry.ToString("yyyyMMddHHmmss");

            // MAC.VAL.PC.DEVICE.PC.EXPIRY.wipropari
            string validationData =
                $"{mac}.VAL.{pc}.{device}.PC.{expiryDateTime}.wipropari";

            return AddDash(ToHexString(validationData));
        }

        // ================= HELPERS =================
        public static string ToHexString(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] bytes = Encoding.UTF8.GetBytes(str);

            foreach (byte b in bytes)
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public static string FromHexString(string hexString)
        {
            byte[] bytes = new byte[hexString.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);

            return Encoding.UTF8.GetString(bytes);
        }

        public static string AddDash(string str)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {
                if (i % 4 == 0 && i != 0)
                    sb.Append('-');

                sb.Append(str[i]);
            }

            return sb.ToString();
        }
    }
}

