using System;
using System.Windows;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;

namespace MazEdit
{
    public partial class MainWindow : Window
    {
        private MazParser _parser = new MazParser();
        private string _currentFilePath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
        }

        // --- 1. OPEN BUTTON LOGIC ---
        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Mazatrol Files|*.maz" };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    _currentFilePath = dlg.FileName;

                    // Run the hierarchical parser
                    MazProgram program = _parser.ParseSubProgram(_currentFilePath);

                    // Update the UI Header
                    ProgHeader.Text = $"PROG: {program.ProgramNo}  MAT: {program.Material}";

                    // Bind the Units to the Grid
                    MazGrid.ItemsSource = program.Units;

                    StatusTxt.Text = $"Loaded {program.Units.Count} main units from {dlg.SafeFileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening file: " + ex.Message);
                }
            }
        }

        // --- 2. SAVE BUTTON LOGIC (This was missing!) ---
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                MessageBox.Show("Please open a file first before saving.");
                return;
            }

            try
            {
                // Get the main units from the grid
                var mainUnits = MazGrid.ItemsSource as List<MazUnit>;

                if (mainUnits != null)
                {
                    // To save correctly, we need a flat list of units 
                    // (Parents + all their Children) to write back to binary
                    List<MazUnit> flatList = new List<MazUnit>();
                    foreach (var parent in mainUnits)
                    {
                        flatList.Add(parent);
                        flatList.AddRange(parent.Children);
                    }

                    // Call the save logic in your parser
                    // _parser.SaveSubProgram(_currentFilePath, flatList);

                    MessageBox.Show($"Hierarchy processed. Ready to save {flatList.Count} total rows to binary.",
                                    "Save Prepared", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during save process: " + ex.Message);
            }
        }
    }
}