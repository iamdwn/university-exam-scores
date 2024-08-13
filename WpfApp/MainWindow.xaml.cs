using ClassLibrary.Entities;
using ClassLibrary.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms.Design;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        private string selectedFilePath;
        private DataReader dataReader;
        private DataImporter dataImporter;
        private List<StudentResult> csvData;
        private double elapsedTime;
        private int rows;

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
                //csvData = dataReader.ReadCsvFile(selectedFilePath);
            }
        }

        private async void ImportFile_Click(object sender, RoutedEventArgs e)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            ////if (csvData != null && csvData.Count > 0)
            //var stopwatch = new Stopwatch();
            //stopwatch.Start();

            //if (!string.IsNullOrEmpty(FilePathTextBox.Text))
            //{
            //    //var (rows, elapsedTime, linesInserted) = await dataImporter.ImportStudentResultsAsync(FilePathTextBox.Text);
            //    var (rows, linesInserted) =  dataImporter.ImportData(csvData);


            //    this.Dispatcher.Invoke(() =>
            //    {
            //        stopwatch.Stop();
            //        dtgResult.ItemsSource = rows;
            //        ElapsedTimeLabel.Content = $"Elapsed Time: {stopwatch}s";
            //        LinesInsertedLabel.Content = $"Lines Inserted: {linesInserted}  ";
            //    });

            //    MessageBox.Show("Imported successful !", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            //}
            //else
            //{
            //    MessageBox.Show("Please select a CSV file first.", "No File Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            //}

            var queue = new BlockingCollection<StudentResult>();
            string filePath = FilePathTextBox.Text;

            var reader = new DataReader(queue);
            var importer = new DataImporter(queue);

            Task readTask = Task.Run(() => reader.ReadFromFile(filePath));
            Task importTask = Task.Run(() => rows = importer.ImportToDatabase());

            Task.WaitAll(readTask, importTask);

            stopwatch.Stop();

            ElapsedTimeLabel.Content = $"Elapsed Time: {stopwatch.ElapsedMilliseconds / 1000.0}s";
            LinesInsertedLabel.Content = $"Lines Inserted: {rows}";

            MessageBox.Show("Imported successful !", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ClearData_Click(object sender, RoutedEventArgs e)
        {   
            dtgResult.ItemsSource = null;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            using (var context = new SchoolContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                await context.Database.ExecuteSqlRawAsync("DELETE FROM StudentResults");

                context.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            stopwatch.Stop();

            ElapsedTimeLabel.Content = $"Elapsed Time: {stopwatch.ElapsedMilliseconds / 1000.0}s";
            LinesInsertedLabel.Content = "Lines Inserted: 0";

            MessageBox.Show("Cleared successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
