using ClassLibrary.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Persistence
{
    public partial class SchoolContext : DbContext
    {
        public SchoolContext()
        {
        }

        public SchoolContext(DbContextOptions<SchoolContext> options)
            : base(options)
        {
        }

        public DbSet<StudentResult> StudentResults { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(GetConnectionString());

        private string GetConnectionString()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true, true).Build();
            return configuration["ConnectionStrings:DefaultConnection"];
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StudentResult>(entity =>
            {
                entity.ToTable("StudentResults");

                entity.HasKey(e => e.Sbd);

                //entity.Property(e => e.Sbd)
                //    .HasColumnName("Sbd")
                //    .ValueGeneratedNever();

                //entity.Property(e => e.Toan)
                //    .HasColumnName("Toan")
                //    .HasColumnType("decimal(5, 2)");

                //entity.Property(e => e.NguVan)
                //    .HasColumnName("NguVan")
                //    .HasColumnType("decimal(5, 2)");

                //entity.Property(e => e.NgoaiNgu)
                //    .HasColumnName("NgoaiNgu")
                //    .HasColumnType("decimal(5, 2)");

                //entity.Property(e => e.VatLi)
                //    .HasColumnName("VatLi")
                //    .HasColumnType("decimal(5, 2)");

                //entity.Property(e => e.HoaHoc)
                //    .HasColumnName("HoaHoc")
                //    .HasColumnType("decimal(5, 2)");

                //entity.Property(e => e.SinhHoc)
                //    .HasColumnName("SinhHoc")
                //    .HasColumnType("decimal(5, 2)");

                //entity.Property(e => e.LichSu)
                //    .HasColumnName("LichSu")
                //    .HasColumnType("decimal(5, 2)");

                //entity.Property(e => e.DiaLi)
                //    .HasColumnName("DiaLi")
                //    .HasColumnType("decimal(5, 2)");

                //entity.Property(e => e.Gdcd)
                //    .HasColumnName("Gdcd")
                //    .HasColumnType("decimal(5, 2)");

                //entity.Property(e => e.MaNgoaiNgu)
                //    .HasColumnName("MaNgoaiNgu")
                //    .HasMaxLength(10);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
