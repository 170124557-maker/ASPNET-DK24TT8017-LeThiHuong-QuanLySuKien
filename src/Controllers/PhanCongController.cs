using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DA09_QLSK.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;


namespace DA09_QLSK.Controllers
{
    public class PhanCongController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PhanCongController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action mở trang danh sách các sự kiện để phân công
        public IActionResult Admin_PhanCong()
        {
            // Lấy danh sách sự kiện kèm theo thông tin loại sự kiện
            var dsSuKien = _context.SuKien.Include(s => s.PhanLoaiSK).ToList();
            return View("~/Views/Admin/Admin_PhanCong.cshtml", dsSuKien);
        }


        [HttpGet]
         public IActionResult ChiTietPhanCong(string id)
        {
            var suKien = _context.SuKien.Find(id);
            if (suKien == null) return NotFound();

            // 1. Lấy danh sách tài khoản đã đăng ký sự kiện 
            var dsDaDangKy = _context.DangKy
                .Where(d => d.MaSK == id)
                .Select(d => d.MaSV) // Dùng Account_id để khớp với SinhVien
                .ToList();

            // 2. Nạp dữ liệu vào ViewBag.SinhVienList
            // Dùng .ToList() để thực thi truy vấn ngay, tránh lỗi null reference khi binding
            // Kiểm tra danh sách sinh viên:
            var sinhVienList = _context.SinhVien
                .Where(s => dsDaDangKy.Contains(s.MaSV))
                .ToList();

            ViewBag.SinhVienList = new SelectList(sinhVienList, "Account_id", "TenSV");

            // 3. Lấy dữ liệu phân công
            var list = _context.PhanCong
                .Where(pc => pc.MaSK == id)
                .ToList()
                .Select(pc => new PhanCongViewModel
                {
                    PhanCong = pc,
                    TenSV = _context.SinhVien.FirstOrDefault(s => s.Account_id == pc.Account_id)?.TenSV ?? "N/A"
                }).ToList();

            ViewBag.SuKien = suKien;
            return View("~/Views/Admin/Admin_ChiTietPC.cshtml", list);
        }

        [HttpPost]
        public IActionResult LuuPhanCong(PhanCong model)
        {
            model.ThoiGianPhanCong = DateTime.Now;
            _context.PhanCong.Add(model);
            _context.SaveChanges();

            // Lấy thông tin Tên SV để trả về hiển thị
            var tenSV = _context.SinhVien.FirstOrDefault(s => s.Account_id == model.Account_id)?.TenSV;

            // Trả về dữ liệu để JS cập nhật giao diện
            return Json(new
            {
                success = true,
                tenSV = tenSV,
                nhiemVu = model.NhiemVu,
                thoiGian = model.ThoiGianPhanCong?.ToString("dd/MM/yyyy")
            });
        }

        [HttpPost]
        public IActionResult XoaPhanCong(int maPC) // Tên tham số phải khớp với dữ liệu gửi từ JS
        {
            var pc = _context.PhanCong.FirstOrDefault(p => p.MaPC == maPC); // Dùng FirstOrDefault an toàn hơn
            if (pc != null)
            {
                _context.PhanCong.Remove(pc);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            // Nếu không tìm thấy, trả về success: false để JS hiện "Lỗi khi xóa!"
            return Json(new { success = false });
            
        }

        public IActionResult LayDanhSachPhanCong(string maSK)
        {
           // public IActionResult LayDanhSachJson(string maSK)
            //{
                var list = _context.PhanCong.Where(pc => pc.MaSK == maSK)
                    .Select(pc => new {
                        maPC = pc.MaPC,
                        tenSV = _context.SinhVien.FirstOrDefault(s => s.Account_id == pc.Account_id).TenSV,
                        nhiemVu = pc.NhiemVu,
                        thoiGian = pc.ThoiGianPhanCong.Value.ToString("dd/MM/yyyy HH:mm")
                    }).ToList();

                return Json(list);
            }
        }
    
}