using Microsoft.AspNetCore.Mvc;
using DA09_QLSK.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace DA09_QLSK.Controllers
{
    public class MySKController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MySKController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action này phải trùng với tên file View (MySK.cshtml)
        public IActionResult MySK()
        {
            string maSV = HttpContext.Session.GetString("MaSV");

            if (string.IsNullOrEmpty(maSV))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem sự kiện của bạn.";
                return RedirectToAction("DangNhap", "DangNhap");
            }

            // Truy vấn lấy sự kiện đã đăng ký
            var danhSachSuKien = (from dk in _context.DangKy
                                  join sk in _context.SuKien on dk.MaSK equals sk.MaSK
                                  where dk.MaSV == maSV
                                  select sk).ToList();

            // Chỉ cần return View(data) - Hệ thống sẽ tự tìm trong Views/MySK/MySK.cshtml
            // Nếu bạn để trong Views/SuKien/, hãy dùng: return View("~/Views/SuKien/MySK.cshtml", danhSachSuKien);
            return View("~/Views/SuKien/MySK.cshtml", danhSachSuKien);
        }
    }
}