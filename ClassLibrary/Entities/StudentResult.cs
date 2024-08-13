using CsvHelper.Configuration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassLibrary.Entities
{
    public class StudentResult
    {
        [Key]
        [Column("sbd")]
        public int Sbd { get; set; }

        [Column("toan")]
        public decimal? Toan { get; set; }

        [Column("ngu_van")]
        public decimal? NguVan { get; set; }

        [Column("ngoai_ngu")]
        public decimal? NgoaiNgu { get; set; }

        [Column("vat_li")]
        public decimal? VatLi { get; set; }

        [Column("hoa_hoc")]
        public decimal? HoaHoc { get; set; }

        [Column("sinh_hoc")]
        public decimal? SinhHoc { get; set; }

        [Column("lich_su")]
        public decimal? LichSu { get; set; }

        [Column("dia_li")]
        public decimal? DiaLi { get; set; }

        [Column("gdcd")]
        public decimal? Gdcd { get; set; }

        [Column("ma_ngoai_ngu")]
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
            Map(m => m.DiaLi).Name("dia_li");
            Map(m => m.Gdcd).Name("gdcd");
            Map(m => m.MaNgoaiNgu).Name("ma_ngoai_ngu");
        }
    }
}
