using ClassLibrary.Entities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class DataImporter
{
    public (List<StudentResult> rows, int linesInserted) ImportData(List<StudentResult> data)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var headers = data[0];
        var rows = data.Skip(0).ToList();

        stopwatch.Stop();

        SaveToDatabase(rows);

        return (rows, rows.Count);
    }

    private void SaveToDatabase(List<StudentResult> data)
    {

    }
}
