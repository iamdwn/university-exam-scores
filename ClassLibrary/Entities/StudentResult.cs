using CsvHelper.Configuration;
using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.Entities
{
    public class StudentResult
    {
        [Key]
        public int Sbd { get; set; }

        public decimal? Toan { get; set; }

        public decimal? NguVan { get; set; }

        public decimal? NgoaiNgu { get; set; }

        public decimal? VatLi { get; set; }

        public decimal? HoaHoc { get; set; }

        public decimal? SinhHoc { get; set; }

        public decimal? LichSu { get; set; }

        public decimal? DiaLi { get; set; }

        public decimal? Gdcd { get; set; }

        public string MaNgoaiNgu { get; set; }
    }

    public class StudentResultMap : ClassMap<StudentResult>
    {
        public StudentResultMap()
        {
            Map(m => m.Sbd).Name("sbd");
            Map(m => m.Toan).Name("toan");
            Map(m => m.NguVan).Name("ngu_van");
            Map(m => m.NgoaiNgu).Name("ngoai_ngu");
            Map(m => m.VatLi).Name("vat_li");
            Map(m => m.HoaHoc).Name("hoa_hoc");
            Map(m => m.SinhHoc).Name("sinh_hoc");
            Map(m => m.LichSu).Name("lich_su");
            Map(m => m.DiaLi).Name("lich_su");
            Map(m => m.Gdcd).Name("gdcd");
            Map(m => m.MaNgoaiNgu).Name("ma_ngoai_ngu");
        }
    }
}
