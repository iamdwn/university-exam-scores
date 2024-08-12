using ClassLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Windows;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        private string selectedFilePath;
        private DataReader dataReader = new DataReader();
        private DataImporter dataImporter = new DataImporter();
        private List<StudentResult> csvData;
        private double elapsedTime;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".csv",
                Filter = "CSV Files (*.csv)|*.csv"
            };

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                selectedFilePath = dlg.FileName;
                FilePathTextBox.Text = selectedFilePath;
                (csvData, elapsedTime) = dataReader.ReadCsvFile(selectedFilePath);
            }
        }

        private void ImportFile_Click(object sender, RoutedEventArgs e)
        {
            if (csvData != null && csvData.Count > 0)
            {
                var (rows, linesInserted) = dataImporter.ImportData(csvData);

                this.Dispatcher.Invoke(() =>
                {
                    dtgResult.ItemsSource = rows;
                    ElapsedTimeLabel.Content = $"Elapsed Time: {elapsedTime}s";
                    LinesInsertedLabel.Content = $"Lines Inserted: {linesInserted}";
                });
            }
            else
            {
                MessageBox.Show("Please select a CSV file first.", "No File Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
