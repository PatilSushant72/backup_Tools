using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace Validation_key
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Admin License Utility ===");
                Console.WriteLine("1. Validate Application Key");
                Console.WriteLine("2. Generate New License Key for User (from mailed key)");
                Console.WriteLine("3. Exit");
                Console.Write("Choose Option (1, 2 or 3): ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ProcessApplicationKey();
                        break;

                    case "2":
                        GenerateLicenseKey();
                        break;

                    case "3":
                        Console.WriteLine("\nExiting... Goodbye!");
                        return;

                    default:
                        Console.WriteLine("\nInvalid option. Please choose 1, 2 or 3.");
                        break;
                }

                Console.WriteLine("\nPress any key to return to menu...");
                Console.ReadKey();
            }
        }

        // === Old Logic : Application Key Validation ===
        private static void ProcessApplicationKey()
        {
            Console.WriteLine("\nEnter your Application Key :");
            string applicationkey = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(applicationkey))
            {
                Console.WriteLine("Invalid Application Key entered.");
                return;
            }

            try
            {
                var data = FromHexString(applicationkey.Replace("-", ""));
                var arrData = data.Split('.');
                var returnData = "";

                if (!data.Contains("wipropari"))
                {
                    Console.WriteLine("Generated Application key is not for valid System/PC");
                    return;
                }

                for (int i = 0; i < arrData.Length; i++)
                {
                    if (arrData[i] == "APL")
                        returnData += ToHexString("VAL");
                    else
                    {
                        var strReverse = string.Join("", arrData[i].Reverse().ToList());
                        returnData += ToHexString(strReverse);
                    }

                    if (i != arrData.Length - 1)
                        returnData += ToHexString(".");
                }

                var validationKey = AddDash(returnData);

                Console.WriteLine("\nApplication Key : " + applicationkey);
                Console.WriteLine("Validation Key  : " + validationKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing Application Key : " + ex.Message);
            }
        }

        // === New Logic : Generate License Key for User ===
        private static void GenerateLicenseKey()
        {
            Console.WriteLine("\nEnter the Request Key received from User (format: UserId|RequestTime):");
            string userKey = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(userKey))
            {
                Console.WriteLine("Invalid Key entered.");
                return;
            }

            try
            {
                // Step 1: Parse user request
                string[] parts = userKey.Split('|');
                if (parts.Length < 2)
                {
                    Console.WriteLine("Invalid User Request Format.");
                    return;
                }

                string userId = parts[0];
                string requestTime = parts[1];

                // Step 2: Create license valid for 30 days
                string licenseData = $"{userId}|{DateTime.Now.AddDays(15):yyyy-MM-dd HH:mm:ss}";

                // Step 3: Encrypt license key to send back
                string licenseKey = EncryptionHelper.Encrypt(licenseData);

                Console.WriteLine("\nUser ID : " + userId);
                Console.WriteLine("Generated License Key (Send this to User):\n");
                Console.WriteLine(licenseKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error generating License Key : " + ex.Message);
            }
        }

        // === Utility Methods (Old Validation Key Logic) ===
        public static string FromHexString(string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return Encoding.Default.GetString(bytes);
        }

        public static string ToHexString(string str)
        {
            var sb = new StringBuilder();
            var bytes = Encoding.Default.GetBytes(str);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }

        public static string AddDash(string str)
        {
            var retrunString = "";
            for (int i = 0; i < str.Length; i++)
            {
                if (i % 4 == 0 && i != 0)
                    retrunString += "-" + str[i];
                else
                    retrunString += str[i];
            }
            if (retrunString.EndsWith("-"))
                retrunString = retrunString.Substring(0, retrunString.Length - 1);
            return retrunString;
        }

        // === AES Helper : Common with User Side ===
        public static class EncryptionHelper
        {
            private static readonly string secretKey = "My$ecretK3y12345"; // Must match User side

            public static string Encrypt(string plainText)
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(secretKey);
                    aes.IV = new byte[16]; // fixed IV

                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                        sw.Close();
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }

            public static string Decrypt(string cipherText)
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(secretKey);
                    aes.IV = new byte[16];

                    using (var decryptor = aes.CreateDecryptor())
                    using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}
