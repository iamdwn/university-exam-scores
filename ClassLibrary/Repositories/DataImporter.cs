using ClassLibrary.Entities;
using ClassLibrary.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

public class DataImporter
{
    private readonly BlockingCollection<StudentResult> _queue;
    private string _connectionString = "Server=(local); Database=StudentUniversityResults; Uid=sa; Pwd=12345;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;";
    private readonly IConfiguration _configuration;
    private int rows;

    public DataImporter(BlockingCollection<StudentResult> queue, IConfiguration configuration)
    {
        _queue = queue;
        _configuration = configuration;
    }

    public DataImporter(BlockingCollection<StudentResult> queue)
    {
        _queue = queue;
    }

    //public (List<StudentResult> rows, int linesInserted) ImportData(List<StudentResult> data)
    //{
    //    var stopwatch = new Stopwatch();
    //    stopwatch.Start();
    //    var headers = data[0];
    //    var rows = data.Skip(0).ToList();

    //    stopwatch.Stop();

    //    SaveToDatabase(rows);

    //    return (rows, rows.Count);
    //}

    //private void SaveToDatabase(List<StudentResult> data)
    //{
    //    using (var context = new SchoolContext())   
    //    {
    //        context.StudentResults.AddRange(data);
    //        context.SaveChanges();
    //    }
    //}

    public int ImportToDatabase()
    {
        //_connectionString = _configuration.GetConnectionString("DefaultConnection");
        List<StudentResult> batch = new List<StudentResult>();


        foreach (var item in _queue.GetConsumingEnumerable())
        {
            batch.Add(item);

            if (batch.Count >= 1000)
            {
                rows += BulkInsert(batch);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            rows += BulkInsert(batch);
        }

        return rows;
    }

    private int BulkInsert(List<StudentResult> studentResults)
    {
        //_connectionString = _configuration.GetConnectionString("DefaultConnection");
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
            {
                bulkCopy.DestinationTableName = "StudentResults";

                bulkCopy.ColumnMappings.Add("Sbd", "sbd");
                bulkCopy.ColumnMappings.Add("Toan", "toan");
                bulkCopy.ColumnMappings.Add("NguVan", "ngu_van");
                bulkCopy.ColumnMappings.Add("NgoaiNgu", "ngoai_ngu");
                bulkCopy.ColumnMappings.Add("VatLi", "vat_li");
                bulkCopy.ColumnMappings.Add("HoaHoc", "hoa_hoc");
                bulkCopy.ColumnMappings.Add("SinhHoc", "sinh_hoc");
                bulkCopy.ColumnMappings.Add("LichSu", "lich_su");
                bulkCopy.ColumnMappings.Add("DiaLi", "dia_li");
                bulkCopy.ColumnMappings.Add("Gdcd", "gdcd");
                bulkCopy.ColumnMappings.Add("MaNgoaiNgu", "ma_ngoai_ngu");

                var dataTable = ToDataTable(studentResults);
                bulkCopy.WriteToServer(dataTable);

                return dataTable.Rows.Count;
            }
        }
    }

    private DataTable ToDataTable(List<StudentResult> items)
    {
        var dataTable = new DataTable(typeof(StudentResult).Name);

        foreach (var prop in typeof(StudentResult).GetProperties())
        {
            dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        }

        foreach (var item in items)
        {
            var values = new object[typeof(StudentResult).GetProperties().Length];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = typeof(StudentResult).GetProperties()[i].GetValue(item, null);
            }

            dataTable.Rows.Add(values);
        }

        return dataTable;
    }
}
