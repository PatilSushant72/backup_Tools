using System;
using System.IO;
using Tecnomatix.Engineering;
using System.Collections;
public class MainScript
{
    public static void Main(string[] args) { }

    // Method to append a CSV line (quotes ensure separate columns)
    private static void WriteCsvLine(StreamWriter writer, string deviceName, string type, string name)
    {
        writer.WriteLine("\"" + deviceName + "\",\"" + type + "\",\"" + name + "\"");
    }

    // Write poses to CSV
    public static void WritePosesToCsv(ITxDevice device, StreamWriter writer)
    {
        if (device == null) return;

        TxObjectList poses = device.PoseList;
        for (int i = 0; i < poses.Count; i++)
        {
            var poseObj = poses[i];
            string poseName = "<unknown>";
            try
            {
                poseName = poseObj.GetType().GetProperty("Name").GetValue(poseObj, null).ToString();
            }
            catch { }

            WriteCsvLine(writer, device.Name, "Pose", poseName);
        }
    }

    // Write driving joints to CSV
    public static void WriteDrivingJointsToCsv(ITxDevice device, StreamWriter writer)
    {
        if (device == null) return;

        TxObjectList joints = device.DrivingJoints;
        for (int i = 0; i < joints.Count; i++)
        {
            string jointName = "<unknown>";
            try
            {
                jointName = joints[i].GetType().GetProperty("Name").GetValue(joints[i], null).ToString();
            }
            catch { }

            WriteCsvLine(writer, device.Name, "DrivingJoint", jointName);
        }
    }

    // Main method to export all devices to CSV
    public static void MainWithOutput(ref StringWriter output)
    {
        try
        {
            string filePath = @"C:\Users\b6b8b2c7\Desktop\DeviceReport.csv";

            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                // Write header
                WriteCsvLine(writer, "Device", "Type", "Name");

                // Get all devices
                TxObjectList devices = TxApplication.ActiveDocument.PhysicalRoot.GetAllDescendants(new TxTypeFilter(typeof(ITxDevice)));

                if (devices.Count == 0)
                {
                    output.WriteLine("No devices found in the project.");
                    return;
                }

                // Iterate through devices
                for (int i = 0; i < devices.Count; i++)
                {
                    ITxDevice device = devices[i] as ITxDevice;
                    if (device == null) continue;

                    // Write poses
                    WritePosesToCsv(device, writer);

                    // Write driving joints
                    WriteDrivingJointsToCsv(device, writer);
                }
            }

            //Clipboard.SetText(clipboardText);
            output.WriteLine("CSV file created successfully at: " + filePath);

            ArrayList allRows = new ArrayList();

            try
            {
                using (StreamReader reader = new StreamReader("C:\\Users\\b6b8b2c7\\Desktop\\DeviceReport.csv"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Parse CSV line manually
                        ArrayList rowValues = ParseCsvLine(line);
                        allRows.Add(rowValues);
                    }
                }
            }
            catch (Exception ex)
            {
                output.WriteLine(ex);
                //MessageBox.Show("Error reading CSV file: " + ex.Message);
            }
            foreach (var row in allRows)
            {
                output.WriteLine(row);
            }
            CopyCsvArrayListToClipboard(allRows);
        }
        catch (Exception ex)
        {
            output.WriteLine("Error creating CSV file: " + ex.Message);
        }
    }
    private static ArrayList ParseCsvLine(string line)
    {
        ArrayList result = new ArrayList();
        bool inQuotes = false;
        string current = "";

        foreach (char c in line)
        {
            if (c == '\"')
            {
                inQuotes = !inQuotes; // toggle quote mode
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.Trim());
                current = "";
            }
            else
            {
                current += c;
            }
        }

        result.Add(current.Trim());
        return result;
    }

    public static void CopyCsvArrayListToClipboard(ArrayList allRows)
    {
        StringBuilder sb = new StringBuilder();

        foreach (ArrayList row in allRows)
        {
            for (int i = 0; i < row.Count; i++)
            {
                // Replace modern ?. and ?? with a safe null-check
                object obj = row[i];
                string cell = obj == null ? "" : obj.ToString();

                // Escape quotes and wrap field if it contains comma, quote or newline
                if (cell.IndexOfAny(new char[] { ',', '"', '\n', '\r' }) >= 0)
                {
                    cell = "\"" + cell.Replace("\"", "\"\"") + "\"";
                }

                sb.Append(cell);

                if (i < row.Count - 1)
                    sb.Append(","); // comma between columns
            }

            sb.AppendLine(); // new line for next row
        }

        string clipboardText = sb.ToString();

        try
        {
            Clipboard.SetText(clipboardText);
        }
        catch (Exception ex)
        {
        }
    }
}
