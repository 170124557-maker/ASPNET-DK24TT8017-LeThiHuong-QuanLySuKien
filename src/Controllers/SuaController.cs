using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using DA09_QLSK.Models;

namespace DA09_QLSK.Controllers
{
    public class SuaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Hiển thị form sửa
        public IActionResult SuaSK(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var suKien = _context.SuKien.FirstOrDefault(s => s.MaSK == id);
            if (suKien == null) return NotFound();

            // NOTE: Nạp danh sách loại sự kiện cho dropdown
            ViewBag.LoaiSKList = _context.PhanLoaiSK.ToList();
            // Lấy toàn bộ danh sách Khoa để hiển thị dropdown
            ViewBag.KhoaList = _context.Khoa.ToList();

            // Lấy Khoa hiện tại của sự kiện 
            ViewBag.SelectedKhoa = _context.Khoa_SuKien
                .FirstOrDefault(ks => ks.MaSK == id)?.MaKhoa;

            return View("~/Views/Admin/SuaSK.cshtml", suKien);
        }

   //     ==============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaSK(SuKien model)
        {
            if (ModelState.IsValid)
            {
                var suKien = _context.SuKien.Find(model.MaSK);
                if (suKien == null) return NotFound();

                // 1. Cập nhật thông tin cơ bản
                suKien.TenSK = model.TenSK;
                // Cập nhật các trường thông tin
               
                suKien.MotaSK = model.MotaSK;
                suKien.start_date = model.start_date;
                suKien.end_date = model.end_date;
                suKien.Start_time = model.Start_time; 
                suKien.End_time = model.End_time;     
                suKien.DiaDiem = model.DiaDiem;
                suKien.TrangThai = model.TrangThai;
                suKien.MaLoaiSK = model.MaLoaiSK;    
                suKien.max_participants = model.max_participants;

              
                // 2. Cập nhật bảng trung gian Khoa_SuKien
                if (!string.IsNullOrEmpty(model.MaKhoa))
                {
                    var existingLink = _context.Khoa_SuKien.FirstOrDefault(ks => ks.MaSK == model.MaSK);

                    if (existingLink != null)
                    {
                        // NẾU ĐÃ CÓ KHOA CŨ: Xóa bản ghi cũ và thêm bản ghi mới
                        // Vì MaKhoa là một phần của khóa, không thể sửa trực tiếp
                        _context.Khoa_SuKien.Remove(existingLink);
                        _context.Khoa_SuKien.Add(new Khoa_SuKien { MaSK = model.MaSK, MaKhoa = model.MaKhoa });
                    }
                    else
                    {
                        // NẾU CHƯA CÓ: Chỉ cần thêm mới
                        _context.Khoa_SuKien.Add(new Khoa_SuKien { MaSK = model.MaSK, MaKhoa = model.MaKhoa });
                    }
                }
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật sự kiện thành công!";
                return RedirectToAction("QuanLySK", "QuanLySK"); 


            }

            // Nạp lại dữ liệu nếu lỗi
            ViewBag.LoaiSKList = _context.PhanLoaiSK.ToList();
            ViewBag.KhoaList = _context.Khoa.ToList();
            return View("~/Views/Admin/SuaSK.cshtml", model);
        }



        /*    ==============



            // POST: Lưu thay đổi
            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult SuaSK(SuKien model)
            {
                if (ModelState.IsValid)
                {
                    var suKien = _context.SuKien.Find(model.MaSK);
                    suKien.MaKhoa = model.MaKhoa;
                    if (suKien == null) return NotFound();

                    // Cập nhật các trường thông tin
                    suKien.TenSK = model.TenSK;
                    suKien.MotaSK = model.MotaSK;
                    suKien.start_date = model.start_date;
                    suKien.end_date = model.end_date;
                    suKien.Start_time = model.Start_time; // Cập nhật mới
                    suKien.End_time = model.End_time;     // Cập nhật mới
                    suKien.DiaDiem = model.DiaDiem;
                    suKien.TrangThai = model.TrangThai;
                    suKien.MaLoaiSK = model.MaLoaiSK;     // Cập nhật mới
                    suKien.max_participants = model.max_participants; // Cập nhật mới

                    // Lưu ý: qr_code_token không được cập nhật vì giữ nguyên giá trị cũ

                    _context.SaveChanges();
                    return RedirectToAction("QuanLySK", "QuanLySK");
                }

                // NOTE: Nếu lưu thất bại (ModelState không hợp lệ), 
                // phải nạp lại danh sách để View không bị lỗi NullReferenceException
                ViewBag.LoaiSKList = _context.PhanLoaiSK.ToList();

                return View("~/Views/Admin/SuaSK.cshtml", model);
            }
        */

        // Nút Xem chi tiết Sự kiện
        public IActionResult ChiTietSK(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            // Lấy sự kiện
            var suKien = _context.SuKien.FirstOrDefault(s => s.MaSK == id);
            if (suKien == null) return NotFound();

            // Truy vấn lấy tên Khoa từ bảng trung gian
            // Sử dụng Join hoặc FirstOrDefault để lấy tên khoa
            var tenKhoa = _context.Khoa_SuKien
                .Where(ks => ks.MaSK == id)
                .Join(_context.Khoa,
                      ks => ks.MaKhoa,
                      k => k.MaKhoa,
                      (ks, k) => k.TenKhoa)
                .FirstOrDefault();

            // Truyền tên khoa qua ViewBag
            ViewBag.TenKhoa = tenKhoa ?? "Chưa có đơn vị tổ chức";

            return View("~/Views/Admin/Admin_ChiTietSK.cshtml", suKien);
        }
    }

}