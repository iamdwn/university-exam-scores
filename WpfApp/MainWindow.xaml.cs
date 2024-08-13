using ClassLibrary.Entities;
using ClassLibrary.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Windows;
using System.Windows.Forms.Design;
using System.Windows.Threading;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _statusTimer;
        private Stopwatch _stopwatch;
        private int _statusDotCount;
        private string selectedFilePath;
        private DataReader dataReader;
        private DataImporter dataImporter;
        private List<StudentResult> csvData;
        private double elapsedTime;
        private int rows;
        private List<StudentResult> results;
        private int _currentPage = 1;
        private const int PageSize = 1000;
        private ObservableCollection<StudentResult> _pagedResults = new ObservableCollection<StudentResult>();

        public MainWindow()
        {
            InitializeComponent();
            _statusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000)
            };
            _statusTimer.Tick += StatusTimer_Tick;
        }

        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            _statusDotCount = (_statusDotCount + 1) % 4;
            StatusTextBlock.Text = $"Executing {new string('.', _statusDotCount)}";
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
            _stopwatch = Stopwatch.StartNew();
            _statusDotCount = 0;
            _statusTimer.Start();

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

            Task<List<StudentResult>> readTask = Task.Run(() => results = reader.ReadFromFile(filePath));
            Task<int> importTask = Task.Run(() => rows = importer.ImportToDatabase());

            //Task.WaitAll(readTask, importTask);
            var result = await readTask;
            var row = await importTask;

            _stopwatch.Stop();
            _statusTimer.Stop();

            StatusTextBlock.Text = "Executed";
            ElapsedTimeLabel.Content = $"Elapsed Time: {_stopwatch.ElapsedMilliseconds / 1000.0}s";
            LinesInsertedLabel.Content = $"Lines Inserted: {rows}";

            dtgResult.ItemsSource = results;

            _currentPage = 1;
            UpdatePage();

            MessageBox.Show("Imported successful !", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ClearData_Click(object sender, RoutedEventArgs e)
        {
            dtgResult.ItemsSource = null;

            _stopwatch = Stopwatch.StartNew();
            _statusDotCount = 0;
            _statusTimer.Start();

            using (var context = new SchoolContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                await context.Database.ExecuteSqlRawAsync("DELETE FROM StudentResults");

                context.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            _stopwatch.Stop();
            _statusTimer.Stop();

            StatusTextBlock.Text = "Executed";
            ElapsedTimeLabel.Content = $"Elapsed Time: {_stopwatch.ElapsedMilliseconds / 1000.0}s";
            LinesInsertedLabel.Content = "Lines Inserted: 0";


            MessageBox.Show("Cleared successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdatePage()
        {
            int startIndex = (_currentPage - 1) * PageSize;
            _pagedResults.Clear();

            var currentPageResults = results.Skip(startIndex).Take(PageSize);
            foreach (var result in currentPageResults)
            {
                _pagedResults.Add(result);
            }

            int totalPages = (int)Math.Ceiling((double)results.Count / PageSize);
            PageInfoLabel.Content = $"Page {_currentPage}/{totalPages}";

            dtgResult.ItemsSource = _pagedResults;
        }


        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdatePage();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (results == null || _currentPage == results.Count) return;
            int totalPages = (int)Math.Ceiling((double)results.Count / PageSize);
            if (_currentPage < totalPages)
            {
                _currentPage++;
                UpdatePage();
            }
        }

        public async Task<List<StudentGroupMathScore>> CalculateAverageMathScore()
        {
            using (var context = new SchoolContext())
            {
                var studentResults = await context.StudentResults.ToListAsync();

                var groupedResults = studentResults
                    .GroupBy(sr =>
                        {
                        if (sr.Sbd.ToString().Length == 8)
                        {
                            return sr.Sbd.ToString().Substring(0, 2);
                        }
                        else if (sr.Sbd.ToString().Length == 7)
                        {
                            return sr.Sbd.ToString().Substring(0, 1); 
                        }
                        else
                        {
                            return null; 
                        }
                    })
                    .Where(group => group.Key != null) 
                    .Select(group => new StudentGroupMathScore
                    {
                        GroupKey = GetProvince(group.Key),
                        AverageMathScore = Math.Round((decimal)group.Average(sr => sr.Toan), 2)
                    })
                    .OrderByDescending(group => group.AverageMathScore)
                    .ToList();

                return groupedResults;
            }
        }



        public class StudentGroupMathScore
        {
            public string GroupKey { get; set; }
            public decimal? AverageMathScore { get; set; }
        }

        public string GetProvince(string provinceCode)
        {
            //string provinceCode;
            //if (sbd.ToString().Length == 7) provinceCode = sbd.ToString().Substring(0, 1);
            //if (sbd.ToString().Length == 8) provinceCode = sbd.ToString().Substring(0, 2);

            var provinces = new Dictionary<string, string>
            {
                {"1", "Hà Nội"},
                {"2", "Hà Giang"},
                {"3", "Cao Bằng"},
                {"4", "Bắc Kạn"},
                {"5", "Tuyên Quang"},
                {"6", "Lào Cai"},
                {"7", "Điện Biên"},
                {"8", "Lai Châu"},
                {"9", "Sơn La"},
                {"10", "Yên Bái"},
                {"11", "Hòa Bình"},
                {"12", "Thái Nguyên"},
                {"13", "Lạng Sơn"},
                {"14", "Quảng Ninh"},
                {"15", "Bắc Giang"},
                {"16", "Bắc Ninh"},
                {"17", "Vĩnh Phúc"},
                {"18", "Hải Dương"},
                {"19", "Hải Phòng"},
                {"20", "Hưng Yên"},
                {"21", "Thái Bình"},
                {"22", "Hà Nam"},
                {"23", "Nam Định"},
                {"24", "Ninh Bình"},
                {"25", "Thanh Hóa"},
                {"26", "Nghệ An"},
                {"27", "Hà Tĩnh"},
                {"28", "Quảng Bình"},
                {"29", "Quảng Trị"},
                {"30", "Thừa Thiên Huế"},
                {"31", "Đà Nẵng"},
                {"32", "Quảng Nam"},
                {"33", "Quảng Ngãi"},
                {"34", "Bình Định"},
                {"35", "Phú Yên"},
                {"36", "Khánh Hòa"},
                {"37", "Ninh Thuận"},
                {"38", "Bình Thuận"},
                {"39", "Kon Tum"},
                {"40", "Gia Lai"},
                {"41", "Đắk Lắk"},
                {"42", "Đắk Nông"},
                {"43", "Lâm Đồng"},
                {"44", "Bình Phước"},
                {"45", "Tây Ninh"},
                {"46", "Bình Dương"},
                {"47", "Đồng Nai"},
                {"48", "Bà Rịa-Vũng Tàu"},
                {"49", "Long An"},
                {"50", "Tiền Giang"},
                {"51", "Bến Tre"},
                {"52", "Trà Vinh"},
                {"53", "Vĩnh Long"},
                {"54", "Đồng Tháp"},
                {"55", "An Giang"},
                {"56", "Kiên Giang"},
                {"57", "Cần Thơ"},
                {"58", "Hậu Giang"},
                {"59", "Sóc Trăng"},
                {"60", "Bạc Liêu"},
                {"61", "Cà Mau"},
                {"62", "Hà Tây"},
                {"63", "Thành phố Hồ Chí Minh"}
            };

            if (provinces.TryGetValue(provinceCode.ToString(), out var provinceName))
            {
                return provinceName;
            }
            else
            {
                return "Not Found";
            }
        }

        private async void CalculateAndDisplayMathScores_Click(object sender, RoutedEventArgs e)
        {
            var groupedScores = await CalculateAverageMathScore();

            dtgResult.ItemsSource = groupedScores;

            MessageBox.Show("Average math scores calculated successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void CalculateHighestA0Nationwide_Click(object sender, RoutedEventArgs e)
        {
            _stopwatch = Stopwatch.StartNew();
            _statusDotCount = 0;
            _statusTimer.Start();
            using (var context = new SchoolContext())
            {
                var studentResults = await context.StudentResults.ToListAsync();

                var groupedResults = studentResults
                    .GroupBy(sr =>
                    {
                        if (sr.Sbd.ToString().Length == 8)
                        {
                            return sr.Sbd.ToString().Substring(0, 2);
                        }
                        else if (sr.Sbd.ToString().Length == 7)
                        {
                            return sr.Sbd.ToString().Substring(0, 1);
                        }
                        else
                        {
                            return null;
                        }
                    })
                    .Where(group => group.Key != null)
                    .Select(group => new
                    {
                        GroupKey = GetProvince(group.Key),
                        HighestA0Score = group.Max(sr => sr.Toan + sr.VatLi + sr.HoaHoc),
                        SBD = group.OrderByDescending(sr => sr.Toan + sr.VatLi + sr.HoaHoc).First().Sbd
                    })
                    .OrderByDescending(group => group.HighestA0Score)
                    .FirstOrDefault();

                _stopwatch.Stop();
                _statusTimer.Stop();

                StatusTextBlock.Text = "Executed";
                ElapsedTimeLabel.Content = $"Elapsed Time: {_stopwatch.ElapsedMilliseconds / 1000.0}s";

                if (groupedResults != null)
                {
                    dtgResult.ItemsSource = new[] { groupedResults };
                    MessageBox.Show($"Highest A0 Score Nationwide calculated successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No data available.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async void CalculateHighestA0ByGroup_Click(object sender, RoutedEventArgs e)
        {
            _stopwatch = Stopwatch.StartNew();
            _statusDotCount = 0;
            _statusTimer.Start();
            using (var context = new SchoolContext())
            {
                var studentResults = await context.StudentResults.ToListAsync();

                var groupedResults = studentResults
                    .GroupBy(sr =>
                    {
                        if (sr.Sbd.ToString().Length == 8)
                        {
                            return sr.Sbd.ToString().Substring(0, 2); 
                        }
                        else if (sr.Sbd.ToString().Length == 7)
                        {
                            return sr.Sbd.ToString().Substring(0, 1); 
                        }
                        else
                        {
                            return null; 
                        }
                    })
                    .Where(group => group.Key != null)
                    .Select(group => new
                    {
                        GroupKey = GetProvince(group.Key),
                        HighestA0Score = group.Max(sr => sr.Toan + sr.VatLi + sr.HoaHoc),
                        SBD = group.OrderByDescending(sr => sr.Toan + sr.VatLi + sr.HoaHoc).First().Sbd
                    })
                    .OrderByDescending(group => group.HighestA0Score)
                    .ToList();

                //sw.Stop();
                _stopwatch.Stop();
                _statusTimer.Stop();

                StatusTextBlock.Text = "Executed";
                ElapsedTimeLabel.Content = $"Elapsed Time: {_stopwatch.ElapsedMilliseconds / 1000.0}s";

                if (groupedResults.Any())
                {
                    dtgResult.ItemsSource = groupedResults;
                    MessageBox.Show("Highest A0 Scores by Group calculated successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No data available.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

    }
}
