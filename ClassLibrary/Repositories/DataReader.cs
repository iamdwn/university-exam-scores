using ClassLibrary.Entities;
using CsvHelper;
using System.Globalization;
using System.Collections.Concurrent;

public class DataReader
{
    private readonly BlockingCollection<StudentResult> _queue;

    public DataReader(BlockingCollection<StudentResult> queue)
    {
        _queue = queue;
    }

    //    public List<StudentResult> ReadCsvFile(string filePath)
    //    {
    //        using (var reader = new StreamReader(filePath))
    //        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    //        {
    //            csv.Context.RegisterClassMap<StudentResultMap>();

    //            var stopwatch = new Stopwatch();

    //            var records = new List<StudentResult>();
    //            while (csv.Read())
    //            {
    //                records.Add(csv.GetRecord<StudentResult>());
    //            }

    //            stopwatch.Stop();
    //            return (records);
    //        }
    //    }
    //}

    public void ReadFromFile(string filePath)
    {
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<StudentResultMap>();
            var records = csv.GetRecords<StudentResult>();

            foreach (var record in records)
            {
                _queue.Add(record);
            }
        }

        _queue.CompleteAdding();
    }
}
