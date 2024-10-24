using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace iX1LogAnalyser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string ResultPath { get; set; }

        public MainWindow()
        {
            Title = "iX1 Log Analyzer"; 
            InitializeComponent();
            Text.Text = "No file selected";
            Text2.Text = "No file selected"; 
        }

        private void Search(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".txt";
            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result.HasValue && result.Value)
            {
                // Open document 
                string filename = dlg.FileName;
                
                Text.Text = filename;
                Text2.Text = filename;
                
            }
        }

        private void CreateTOD()
        {
            if (Text.Text == "No file selected" || !Text.Text.Contains(".ctl"))
                Result.Text = "PLEASE SELECT A CTL FILE";
            else
            {
                string? path = Path.GetDirectoryName(Text.Text);
                string? name = Path.GetFileNameWithoutExtension(Text.Text);
                path = Path.Combine(path, name + "-tod.csv");
                var retry = path.Replace("-tod", "-tod-retries");
                
                var CSV = new CSVCreator();
                string result = CSV.CreateTODFile(Text.Text, path);
                if (result == "")
                {
                    result = CSV.CreateRetriesTOD(Text.Text, retry);
                }
                if (Result.Text == "")
                    Result.Text = result == "" ? $"Successfully generated TODs and Retries at {path} and {retry}{Environment.NewLine}" : $"{result}{Environment.NewLine}";
                else
                    Result.Text += result == "" ? $"Successfully generated TODs and Retries at {path} and {retry}{Environment.NewLine}" : $"{result}{Environment.NewLine}";
                string directory = @Path.GetDirectoryName(path).Trim() ?? "";
                ResultPath = directory;
                OpenFolder.IsEnabled = true;
            }
        }

        private void CreateLog()
        {
            if (Text2.Text == "No file selected" || !Text2.Text.Contains(".ctl"))
                Result.Text = "PLEASE SELECT A CTL FILE";
            else
            {
                string? path = Path.GetDirectoryName(Text2.Text);
                string? name = Path.GetFileNameWithoutExtension(Text2.Text);
                path = Path.Combine(path, name + "-log.csv");
                var CSV = new CSVCreator();
                string result = CSV.CreateLogBreak(Text2.Text, path);
                if (Result.Text == "")
                    Result.Text = result == "" ? $"Successfully generated Line Breaks/Drops at {path}{Environment.NewLine}" : $"{result}{Environment.NewLine}";
                else
                    Result.Text += result == "" ? $"Successfully generated Line Breaks/Drops at {path}{Environment.NewLine}" : $"{result}{Environment.NewLine}";
                string directory = @Path.GetDirectoryName(path).Trim() ?? "";
                ResultPath = directory;
                OpenFolder.IsEnabled = true;
            }
        }

        private void SortLog()
        {
            if (Text2.Text == "No file selected" || !Text2.Text.Contains(".txt"))
                Result.Text = "PLEASE SELECT A TXT (SystemDebug) FILE";
            else
            {
                string? path = Path.GetDirectoryName(Text2.Text);
                string? name = Path.GetFileNameWithoutExtension(Text2.Text);
                path = Path.Combine(path, name + "-sorted.txt");
                var Txt = new TextCreator();
                string result = Txt.SortLineInstance(Text2.Text, path);
                string path2 = Path.Combine(Path.GetDirectoryName(Text2.Text), name + "-sorted.csv");
                if (result == "")
                {
                    var Csv = new CSVCreator();
                    result = Csv.CreateSort(path, path2);
                }
                if (Result.Text == "")
                    Result.Text = result == "" ? $"Successfully generated Text at {path} and CSV at {path2}{Environment.NewLine}" : $"{result}{Environment.NewLine}";
                else
                    Result.Text += result == "" ? $"Successfully generated Text at {path} and CSV at {path2}{Environment.NewLine}" : $"{result}{Environment.NewLine}";
                string directory = @Path.GetDirectoryName(path).Trim() ?? "";
                ResultPath = directory;
                OpenFolder.IsEnabled = true;
            }
        }

        private void GenerateReportDebug(object sender, RoutedEventArgs e)
        {
            CreateTOD();
            CreateLog();
        }

        private void GenerateReportSystem(object sender, RoutedEventArgs e)
        {
            SortLog();
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Text.Text = "No file selected";
            Text2.Text = "No file selected";
            Result.Text = "";
            ResultPath = "";
            OpenFolder.IsEnabled = false;
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (ResultPath == "")
                return;

            if (Directory.Exists(ResultPath))
            {
                var startInfo = new ProcessStartInfo
                {
                    Arguments = $"\"{ResultPath}\"",
                    FileName = "explorer.exe"
                };
                Process.Start(startInfo);
            }
        }
    }
}
