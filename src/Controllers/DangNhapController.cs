using Microsoft.AspNetCore.Mvc;
using DA09_QLSK.Models;
using System.Linq;
using Microsoft.AspNetCore.Http; // Bắt buộc để dùng Session

namespace DA09_QLSK.Controllers
{
    public class DangNhapController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DangNhapController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult DangNhap()
        {
            return View();
        }
        [HttpPost]
        public IActionResult DangNhap(string Email, string Password)
        {
            // Kiểm tra tài khoản trong bảng Accounts
            var account = _context.Accounts.FirstOrDefault(a => a.email == Email && a.password == Password);

            if (account != null)
            {
                // Lưu thông tin account cơ bản
                HttpContext.Session.SetString("AccountID", account.Account_id);
                HttpContext.Session.SetString("HoTenSV", account.account_name);
                HttpContext.Session.SetString("Role", account.role);

                // QUAN TRỌNG: Lấy MaSV từ bảng SinhVien dựa trên account_id
                var sinhVien = _context.SinhVien.FirstOrDefault(s => s.Account_id == account.Account_id);
                if (sinhVien != null)
                {
                    // Lưu MaSV vào Session để trang ChiTietSK.cshtml đọc được
                    HttpContext.Session.SetString("MaSV", sinhVien.MaSV);
                }

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Email hoặc mật khẩu không chính xác!";
            return View();
        }
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("DangNhap");
        }
    }
}