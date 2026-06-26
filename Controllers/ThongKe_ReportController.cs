using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using System;
using DA09_QLSK.Models;
using OfficeOpenXml;

namespace DA09_QLSK.Controllers
{
    public class ThongKe_ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThongKe_ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ThongKe(string khoa, string trangThai, string loaiSK, int page = 1)
          
        {
            int pageSize = 10;

            // 1. Lấy danh sách Khoa
            ViewBag.KhoaList = _context.Khoa.ToList();
            ViewBag.LoaiSKList = _context.PhanLoaiSK.ToList();

            // 2. Lấy danh sách Trạng thái ĐỘC NHẤT (Distinct) từ DB
            ViewBag.TrangThaiList = _context.SuKien
                                           .Where(s => s.TrangThai != null)
                                           .Select(s => s.TrangThai)
                                           .Distinct()
                                           .ToList();

            // 3. Xử lý logic lọc 
            var dsSuKien = _context.SuKien.AsQueryable();

            if (!string.IsNullOrEmpty(khoa))
                dsSuKien = dsSuKien.Where(s => _context.Khoa_SuKien.Any(ks => ks.MaSK == s.MaSK && ks.MaKhoa == khoa));

            if (!string.IsNullOrEmpty(trangThai))
                dsSuKien = dsSuKien.Where(s => s.TrangThai == trangThai);

            // Lọc theo Lĩnh vực (dựa trên MaLoaiSK trong SuKien)
            if (!string.IsNullOrEmpty(loaiSK))
                dsSuKien = dsSuKien.Where(s => s.MaLoaiSK == loaiSK);

          
            // 3. Map dữ liệu sang ViewModel
            var fullList = dsSuKien.Select(s => new ThongKe_Report
            {
                TenSK = s.TenSK,
                start_date = s.start_date,
                end_date = s.end_date,
                Start_time = s.Start_time,
                End_time = s.End_time,
                // Lấy Tên Khoa
                TenKhoa = (from ks in _context.Khoa_SuKien
                           join k in _context.Khoa on ks.MaKhoa equals k.MaKhoa
                           where ks.MaSK == s.MaSK
                           select k.TenKhoa).FirstOrDefault() ?? "Chưa rõ",
                SoLuongDK = _context.DangKy.Count(d => d.MaSK == s.MaSK),
                SoLuongGioiHan = s.max_participants,
                TrangThai = s.TrangThai,
                // Lây tên Loại SK

                TenLoai = _context.PhanLoaiSK
                      .Where(l => l.MaLoaiSK == s.MaLoaiSK)
                      .Select(l => l.TenLoai)
                      .FirstOrDefault() ?? "Chưa phân loại"
            }).ToList();

            // 4. Phân trang
            int totalItems = fullList.Count;
            var pagedList = fullList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            ViewBag.SelectedKhoa = khoa;
            ViewBag.SelectedTrangThai = trangThai;
            ViewBag.SelectedLoaiSK = loaiSK;     // Lưu lại giá trị đã chọn để dropdown giữ nguyên trạng thái

            return View("~/Views/Admin/ThongKe_Report.cshtml", pagedList);
        }



        // Action mẫu cho nút Export (Bạn có thể bổ sung logic xuất file sau)
        public IActionResult ExportExcel(string khoa, string trangThai, string loaiSK)
        {
            // 1. Lấy dữ liệu giống như khi hiển thị bảng (lấy tất cả, không phân trang)
            var dsSuKien = _context.SuKien.AsQueryable();
           
            // Áp dụng các bộ lọc tương tự như hàm ThongKe
            if (!string.IsNullOrEmpty(khoa))
                dsSuKien = dsSuKien.Where(s => _context.Khoa_SuKien.Any(ks => ks.MaSK == s.MaSK && ks.MaKhoa == khoa));

            if (!string.IsNullOrEmpty(trangThai))
                dsSuKien = dsSuKien.Where(s => s.TrangThai == trangThai);

            if (!string.IsNullOrEmpty(loaiSK))
                dsSuKien = dsSuKien.Where(s => s.MaLoaiSK == loaiSK);

            if (!string.IsNullOrEmpty(khoa))
            {
                dsSuKien = dsSuKien.Where(s => _context.Khoa_SuKien.Any(ks => ks.MaSK == s.MaSK && ks.MaKhoa == khoa));
            }

            var list = dsSuKien.Select(s => new ThongKe_Report
            {
                TenSK = s.TenSK,
                start_date = s.start_date,
                end_date = s.end_date,
                Start_time = s.Start_time,
                End_time = s.End_time,
                TenKhoa = (from ks in _context.Khoa_SuKien
                           join k in _context.Khoa on ks.MaKhoa equals k.MaKhoa
                           where ks.MaSK == s.MaSK
                           select k.TenKhoa).FirstOrDefault() ?? "Chưa rõ",
                SoLuongDK = _context.DangKy.Count(d => d.MaSK == s.MaSK),
                SoLuongGioiHan = s.max_participants ?? 0,
                TrangThai = s.TrangThai ?? "Chưa rõ"
            }).ToList();

            // 2. Tạo nội dung file CSV
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("STT,Tên Sự Kiện, Khoa,Số Lượng Đã Đăng Ký,Giới Hạn Đăng Ký,Trạng Thái,Ngày Bắt Đầu,Giờ Bắt Đầu,Ngày Kết Thúc,Giờ Kết Thúc");

            // 2. Chèn dữ liệu vào các cột tương ứng
            int stt = 1;
            foreach (var item in list)
            {
                // Định dạng ngày giờ cho chuẩn Excel
                string startD = item.start_date?.ToString("dd/MM/yyyy") ?? "";
                string startT = item.Start_time?.ToString(@"hh\:mm") ?? "";
                string endD = item.end_date?.ToString("dd/MM/yyyy") ?? "";
                string endT = item.End_time?.ToString(@"hh\:mm") ?? "";

                builder.AppendLine($"{stt++},{item.TenSK},{item.TenKhoa},{item.SoLuongDK},{item.SoLuongGioiHan},{item.TrangThai},{startD},{startT},{endD},{endT}");
            }

            // 3. Trả về file tải xuống
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
            string fileName = $"DanhSachSuKien_{DateTime.Now:yyyyMMddHHmmss}.csv";
            return File(fileBytes, "text/csv", fileName);
        }
    }
    
}