using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System; 
using DA09_QLSK.Models;

namespace DA09_QLSK.Controllers
{
    public class QuanLySKController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuanLySKController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult QuanLySK(int page = 1)
        {
            int pageSize = 10;

            // 1. Khởi tạo query và lọc bỏ các sự kiện "Đã xóa" ngay từ đầu
            var query = _context.SuKien
                                .Where(s => s.TrangThai != "Đã xóa")
                                .OrderByDescending(s => s.start_date);

            // 2. Lấy tổng số item sau khi đã lọc
            int totalItems = query.Count();

            // 3. Cắt dữ liệu (phân trang) trên query đã lọc
            var dsSuKien = query.Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            // 4. Truyền giá trị vào ViewBag
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View("~/Views/Admin/QuanLySK.cshtml", dsSuKien);
        }
    }
}