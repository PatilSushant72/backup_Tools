using System.Windows.Media;

namespace FolderScan
{
    public class PipelineProjectStatusRow
    {
        public string ProjectName { get; set; }

        // numeric values
        public int Received { get; set; }
        public int Total { get; set; }

        // 🔹 Used by DataGrid & Excel
        public string ReceivedText
        {
            get { return Received + "/" + Total; }
        }

        public double Percentage
        {
            get
            {
                if (Total == 0) return 0;
                return (Received * 100.0) / Total;
            }
        }

        // 🔹 Used for dashboard coloring
        public string CompletionText
        {
            get { return Percentage.ToString("0.#") + "%"; }
        }

        public Brush CompletionBrush
        {
            get
            {
                if (Percentage <= 10)
                    return Brushes.IndianRed;
                if (Percentage < 100)
                    return Brushes.Khaki;
                return Brushes.LightGreen;
            }
        }
    }

}
