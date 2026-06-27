
using Microsoft.AspNetCore.Mvc;
using DA09_QLSK.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;


namespace DA09_QLSK.Controllers
{
    public class DangKyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DangKyController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult DangKyThamGia(string maSK)
        {
            // 1. Kiểm tra đăng nhập
            string maSV = HttpContext.Session.GetString("MaSV");

            if (string.IsNullOrEmpty(maSV))
            {
                TempData["ErrorMessage"] = "Bạn phải đăng nhập trước khi đăng ký Sự Kiện";
                return RedirectToAction("ChiTietSK", "Home", new { id = maSK });
            }

            // 2. Kiểm tra lại điều kiện sự kiện (bảo mật phía server)
            var suKien = _context.SuKien.Find(maSK);
            int daDangKy = _context.DangKy.Count(d => d.MaSK == maSK);

            if (suKien == null || suKien.TrangThai != "Chưa diễn ra" || daDangKy >= suKien.max_participants)
            {
                TempData["ErrorMessage"] = "Sự kiện không khả dụng hoặc đã đủ số lượng.";
                return RedirectToAction("ChiTietSK", "Home", new { id = maSK });
            }

            // 3. Kiểm tra xem sinh viên đã đăng ký chưa
            bool daTonTai = _context.DangKy.Any(d => d.MaSK == maSK && d.MaSV == maSV);
            if (daTonTai)
            {
                TempData["ErrorMessage"] = "Bạn đã đăng ký sự kiện này rồi!";
                return RedirectToAction("ChiTietSK", "Home", new { id = maSK });
            }

            // 4. Lưu vào bảng DangKy
            var dk = new DangKy
            {
                MaSV = maSV,
                MaSK = maSK,
                registration_date = DateTime.Now,
                is_attended = false // Mặc định chưa điểm danh
            };

            _context.DangKy.Add(dk);
            _context.SaveChanges();

            TempData["Message"] = "Đăng ký tham gia sự kiện thành công!";

            return RedirectToAction("ChiTietSK", "Home", new { id = maSK });
        }

        // --- ACTION MỚI: HIỂN THỊ DANH SÁCH SINH VIÊN ĐĂNG KÝ ---

        public IActionResult DanhSachDK(string maSK, int page = 1)
        {
            var suKien = _context.SuKien.FirstOrDefault(s => s.MaSK == maSK);
            ViewBag.TenSK = suKien != null ? suKien.TenSK : "Không xác định";
            ViewBag.MaSK = maSK;

            int pageSize = 10;

            // Lấy query dữ liệu
            var query = _context.DangKy
                .Where(dk => dk.MaSK == maSK)
                .Join(_context.SinhVien, dk => dk.MaSV, sv => sv.MaSV, (dk, sv) => new { dk, sv })
                .Join(_context.Khoa, combined => combined.sv.MaKhoa, k => k.MaKhoa, (combined, k) => new DangKyViewModel
                {
                    MaSV = combined.sv.MaSV,
                    TenSV = combined.sv.TenSV,
                    TenKhoa = k.TenKhoa,
                    NgayDK = combined.dk.registration_date
                })
                .OrderBy(x => x.NgayDK);

            // Tính tổng số trang
            int totalItems = query.Count();
            var pagedData = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.MaSK = maSK;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(pagedData);
        }
    }

}