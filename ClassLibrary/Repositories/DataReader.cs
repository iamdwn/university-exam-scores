using ClassLibrary.Entities;
using CsvHelper;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;

public class DataReader
{
    public (List<StudentResult> rows, double elapsedTime) ReadCsvFile(string filePath)
    {
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            csv.Context.RegisterClassMap<StudentResultMap>();

            var records = csv.GetRecords<StudentResult>().ToList();
            stopwatch.Stop();
            return (records, stopwatch.ElapsedMilliseconds / 1000.0);
        }
    }
}
