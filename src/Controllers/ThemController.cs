using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using DA09_QLSK.Models;

namespace DA09_QLSK.Controllers
{
    public class ThemController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThemController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult ThemSK()
        {
            // 1. Tự sinh Mã sự kiện 
            var lastSK = _context.SuKien.OrderByDescending(s => s.MaSK).FirstOrDefault();
            string newId = lastSK != null ? "SK" + (int.Parse(lastSK.MaSK.Substring(2)) + 1).ToString("D3") : "SK001";
            ViewBag.NewMaSK = newId;

            // 2. Tự sinh qr_code_token
            ViewBag.QRCodeToken = Guid.NewGuid().ToString();

            // 3. Lấy dữ liệu loại sự kiện cho dropdown (Dùng tên cột 'TenLoai' theo ảnh DB)
            ViewBag.LoaiSKList = new SelectList(_context.PhanLoaiSK, "MaLoaiSK", "TenLoai");

            return View("~/Views/Admin/ThemSK.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemSK(SuKien model)
        {
            if (ModelState.IsValid)
            {
                _context.SuKien.Add(model);
                _context.SaveChanges();
                // Quay lại trang quản lý sau khi thêm thành công
                return RedirectToAction("QuanLySK", "QuanLySK");
            }

            // Nạp lại dữ liệu nếu có lỗi để tránh lỗi NullReferenceException
            ViewBag.LoaiSKList = new SelectList(_context.PhanLoaiSK, "MaLoaiSK", "TenLoai", model.MaLoaiSK);
            return View("~/Views/Admin/ThemSK.cshtml", model);
        }
    }
}