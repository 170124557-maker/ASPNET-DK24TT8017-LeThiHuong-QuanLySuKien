using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using DA09_QLSK.Models;

namespace DA09_QLSK.Controllers
{
    public class QuanLyLoaiController : Controller
    {
        private readonly ApplicationDbContext _context;
        public QuanLyLoaiController(ApplicationDbContext context) => _context = context;

        public IActionResult Admin_LoaiSK(int page = 1)
        {
            int pageSize = 10;
            var query = _context.PhanLoaiSK.OrderBy(l => l.MaLoaiSK);

            int totalItems = query.Count();
            var dsLoai = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View("~/Views/Admin/Admin_LoaiSK.cshtml", dsLoai);
        }
        // Thêm vào GET-POST để  hiển thị danh sách sửa và lưu
        [HttpGet]
        public IActionResult Edit(string id)
        {
            var loai = _context.PhanLoaiSK.Find(id);
            if (loai == null) return NotFound();
            return View("~/Views/Admin/Admin_SuaLoai.cshtml", loai);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PhanLoaiSK model)
        {
            if (ModelState.IsValid)
            {
                var loai = _context.PhanLoaiSK.Find(model.MaLoaiSK);
                if (loai == null) return NotFound();

                loai.TenLoai = model.TenLoai;
                loai.MoTaLoaiSK = model.MoTaLoaiSK;

                _context.SaveChanges();
                return RedirectToAction("Admin_LoaiSK");
            }
            return View("~/Views/Admin/Admin_SuaLoai.cshtml", model);
        }

        //==========Đoạn code thêm Loại. Có 2 phương thức GET và POST
        // ==== Thêm 2 Action: một để hiển thị trang thêm mới, hai là để tiếp nhận dữ liệu từ form.

        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Admin_ThemLoai.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PhanLoaiSK model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra mã loại đã tồn tại chưa (tùy chọn)
                var exists = _context.PhanLoaiSK.Any(l => l.MaLoaiSK == model.MaLoaiSK);
                if (exists)
                {
                    ModelState.AddModelError("MaLoaiSK", "Mã loại này đã tồn tại!");
                    return View("~/Views/Admin/Admin_ThemLoai.cshtml", model);
                }

                _context.PhanLoaiSK.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Admin_LoaiSK");
            }
            return View("~/Views/Admin/Admin_ThemLoai.cshtml", model);
        }
        /// =========Xủ lý Xóa Loại=====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteLoai(string id)
        {
            var loai = _context.PhanLoaiSK.Find(id);
            if (loai == null)
                return Json(new { success = false, message = "Không tìm thấy loại sự kiện!" });

            try
            {
                // Kiểm tra xem có sự kiện nào đang dùng loại này không (tránh lỗi khóa ngoại)
                var dangSuDung = _context.SuKien.Any(s => s.MaLoaiSK == id);
                if (dangSuDung)
                {
                    return Json(new { success = false, message = "Không thể xóa vì đang có sự kiện thuộc loại này!" });
                }

                _context.PhanLoaiSK.Remove(loai);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}