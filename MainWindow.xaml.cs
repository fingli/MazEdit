using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace MazEdit
{
    public partial class MainWindow : Window
    {
        private readonly MazParser _parser = new();
        private string _currentFilePath = string.Empty;

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
                MazProgram program = _parser.ParseSubProgram(_currentFilePath);

                ProgHeader.Text = $"PROG: {program.ProgramNo}  MAT: {program.Material}";
                MazGrid.ItemsSource = program.Units;
                StatusTxt.Text = $"Loaded {program.Units.Count} units from {Path.GetFileName(_currentFilePath)}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}", "Open Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
