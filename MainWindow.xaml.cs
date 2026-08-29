using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace MazEdit
{
    public partial class MainWindow : Window
    {
        private readonly MazParser _parser = new();
        private string _currentFilePath = string.Empty;
        private MazProgram? _currentProgram;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Mazatrol Files|*.maz" };

            if (dlg.ShowDialog() != true)
                return;

            try
            {
                _currentFilePath = dlg.FileName;
                _currentProgram = _parser.ParseSubProgram(_currentFilePath);

                ProgHeader.Text = $"MAT: {_currentProgram.Material}  INITIAL-Z: {_currentProgram.InitialZ}";
                MazGrid.ItemsSource = _currentProgram.Units;
                ExportDumpBtn.IsEnabled = true;
                StatusTxt.Text = $"Loaded {_currentProgram.Units.Count} units from {Path.GetFileName(_currentFilePath)}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}", "Open Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportDumpBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath) || _currentProgram is null)
                return;

            var dlg = new SaveFileDialog
            {
                Filter = "Text report|*.txt",
                FileName = Path.ChangeExtension(Path.GetFileName(_currentFilePath), ".txt"),
                InitialDirectory = Path.GetDirectoryName(_currentFilePath)
            };

            if (dlg.ShowDialog() != true)
                return;

            try
            {
                MazDump.WriteReport(_currentFilePath, _currentProgram, dlg.FileName);
                StatusTxt.Text = $"Dump saved to {Path.GetFileName(dlg.FileName)}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting dump: {ex.Message}", "Export Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
