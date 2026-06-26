using Microsoft.AspNetCore.Mvc;
using DA09_QLSK.Models;
using System.Linq;
using System.Security.Claims; // Quan trọng để lấy thông tin User
using System.Collections.Generic;

namespace DA09_QLSK.Controllers
{
    public class myNotiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public myNotiController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // 1. Lấy account_id từ User đang đăng nhập
            var currentAccountId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentAccountId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["Breadcrumb"] = new List<(string Title, string Url)>
    {
        ("Thông báo của tôi", null)
    };

            // 2. Truy vấn LINQ nối các bảng để lọc theo account_id của Sinh viên
            var list = _context.Notifications
                .Join(_context.SuKien,
                      n => n.MaSK,
                      s => s.MaSK,
                      (n, s) => new { n, s })
                .Join(_context.DangKy,
                      x => x.s.MaSK,
                      d => d.MaSK,
                      (x, d) => new { x.n, d })
                .Join(_context.SinhVien,
                      x => x.d.MaSV,
                      sv => sv.MaSV,
                      (x, sv) => new { x.n, sv })
                .Where(x => x.sv.account_id == currentAccountId && x.n.sent_at != null)
                .Select(x => x.n) // Lấy ra đối tượng Notification
                .OrderByDescending(x => x.sent_at)
                .Distinct() // Dùng Distinct để tránh trùng lặp thông báo nếu sinh viên đăng ký nhiều lần
                .ToList();

            return View("User_ThongBao", list);
        }
    }

}