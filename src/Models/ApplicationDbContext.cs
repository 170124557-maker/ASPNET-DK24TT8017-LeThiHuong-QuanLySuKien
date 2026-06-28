using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DA09_QLSK.Models
{
   
    // CẤU HÌNH TRẠM KẾT NỐI DATABASE
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<SuKien> SuKien { get; set; }
        public DbSet<Khoa> Khoa { get; set; }
        public DbSet<DangKy> DangKy { get; set; }
        public DbSet<Khoa_SuKien> Khoa_SuKien { get; set; }
        public DbSet<PhanLoaiSK> PhanLoaiSK { get; set; }
        public DbSet<SinhVien> SinhVien { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<PhanCong> PhanCong { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình Khóa phức hợp cho bảng trung gian
            modelBuilder.Entity<Khoa_SuKien>()
                .HasKey(ks => new { ks.MaKhoa, ks.MaSK });

            // Định danh chính xác tên bảng dưới SQL Server
            modelBuilder.Entity<SuKien>().ToTable("SuKien");
            modelBuilder.Entity<Khoa>().ToTable("Khoa");
            modelBuilder.Entity<Khoa_SuKien>().ToTable("Khoa_SuKien");
            modelBuilder.Entity<DangKy>().ToTable("DangKy");
            modelBuilder.Entity<PhanLoaiSK>().ToTable("PhanLoaiSK");
            modelBuilder.Entity<SinhVien>().ToTable("SinhVien");
            modelBuilder.Entity<Account>().ToTable("Accounts");
            modelBuilder.Entity<PhanCong>().ToTable("PhanCong");
            modelBuilder.Entity<Notification>().ToTable("Notifications");
        }
    }

    // THỰC THỂ CÁC BẢNG DỮ LIỆU

    // dùng Data Annotation tạo giới hạn cho nhập tối đa bao nhiêu ký tự
    //ví dụ dùng Data Annotation:  [StringLength(200, ErrorMessage = "Tên sự kiện tối đa 200 ký tự")]
    // public string TenSK { get; set; }

    //  [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
    //   public string MoTaSK { get; set; }
    // đã tạo xong
    public class SuKien
    {
        [Key]
        public string MaSK { get; set; }
        [StringLength(200, ErrorMessage = "Tên sự kiện tối đa 200 ký tự")]
        public string TenSK { get; set; }
        [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
        public string MotaSK { get; set; }
        public string MaLoaiSK { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public TimeSpan? Start_time { get; set; }
        public TimeSpan? End_time { get; set; }
        public string DiaDiem { get; set; }
        public int? max_participants { get; set; }
        public string TrangThai { get; set; }
        public string qr_code_token { get; set; } // Đường dẫn đến file ảnh QR
        [ForeignKey("MaLoaiSK")]
        public virtual PhanLoaiSK PhanLoaiSK { get; set; }
        [NotMapped] // Quan trọng: Báo EF bỏ qua trường này khi ánh xạ xuống bảng SuKien
        public string MaKhoa { get; set; }
        public int? SoNgayNhac { get; set; }
    }

    public class Khoa
    {
        [Key]
        public string MaKhoa { get; set; }
        public string TenKhoa { get; set; }
    }

    public class Khoa_SuKien
    {
        public string MaKhoa { get; set; }
        public string MaSK { get; set; }
    }

    public class DangKy
    {
        [Key]
        public int MaDK { get; set; }
        public string MaSV { get; set; }
        public string MaSK { get; set; }
        public DateTime? registration_date { get; set; }
        public bool is_attended { get; set; }
        public DateTime? attended_at { get; set; }
    }

   // [Table("SinhVien")]
    public class SinhVien
    {
        internal string account_id;

        [Key]
        public string MaSV { get; set; }
        public string TenSV { get; set; }
        public string MaKhoa { get; set; }
        public string Account_id { get; set; }
    }
    public class Account
    {
        [Key]
        public string Account_id { get; set; }
        public string account_name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string role { get; set; }
        public DateTime? ThoiGianTao { get; set; }

    }

    public class PhanLoaiSK
    {
        [Key]
        public string MaLoaiSK { get; set; }
        public string TenLoai { get; set; }
        public string MoTaLoaiSK { get; set; }
    }

    public class PhanCong
    {
        [Key]
        public int MaPC { get; set; }
        public string MaSK { get; set; }

        [Column("account_id")]
        public string Account_id { get; set; }

        public string NhiemVu { get; set; }
        public DateTime? ThoiGianPhanCong { get; set; }
        public string TrangThaiPC { get; set; }

        // Sử dụng Account_id để thiết lập quan hệ thay cho MaSV
        [ForeignKey("Account_id")]
        public virtual SinhVien SinhVien { get; set; }
    
    }
    public class PhanCongViewModel
    {
        public PhanCong PhanCong { get; set; }
        public string TenSV { get; set; }
    }

    // VIEWMODEL CHUYỂN ĐỔI DỮ LIỆU ĐỂ HIỂN THỊ RA BẢNG GIAO DIỆN
    public class TimKiemViewModel
    {
        public string MaSK { get; set; }
        public string TenSK { get; set; }
        public string TenKhoa { get; set; }
        public string TenLoaiSK { get; set; }
        public DateTime? StartDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string DiaDiem { get; set; }
        public int? MaxParticipants { get; set; }
        public string TrangThai { get; set; }
        public int SoNguoiDaDangKy { get; set; }

        public string StartDateStr { get; set; }
        public string EndDateStr { get; set; }
    }

   /* public class Notification
    {
        public int notification_id { get; set; }
        public string MaSK { get; set; }

        public string? tite { get; set; }
        public string? content { get; set; }
        public DateTime? sent_at { get; set; }
    }*/
}