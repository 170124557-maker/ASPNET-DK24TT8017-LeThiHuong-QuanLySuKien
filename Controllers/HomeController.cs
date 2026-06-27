using Microsoft.AspNetCore.Mvc;
using DA09_QLSK.Models;
using System.Diagnostics;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DA09_QLSK.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();
        public IActionResult ThongBao() => View();
        public IActionResult SuKienCuaToi() => View();

        // ============================================================
        // HÀM DUY NHẤT: Index_SuKien (Đã gộp tất cả logic lọc)
        // ============================================================
        public IActionResult Index_SuKien(string trangThai, string khoa, string loai, DateTime? tuNgay, DateTime? denNgay, int page = 1)
        {
            int pageSize = 10;
            var query = _context.SuKien.AsQueryable();

            // 1. Áp dụng tất cả bộ lọc
            if (!string.IsNullOrEmpty(trangThai)) query = query.Where(s => s.TrangThai == trangThai);
            if (!string.IsNullOrEmpty(khoa)) query = query.Where(s => _context.Khoa_SuKien.Any(ks => ks.MaSK == s.MaSK && ks.MaKhoa == khoa));
            if (!string.IsNullOrEmpty(loai)) query = query.Where(s => s.TenSK == loai);
            if (tuNgay.HasValue) query = query.Where(s => s.start_date >= tuNgay);
            if (denNgay.HasValue) query = query.Where(s => s.end_date <= denNgay);

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var danhSachSuKien = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // 2. Tạo một từ điển (Dictionary) để tra cứu tên loại nhanh chóng
            // Key: MaLoaiSK, Value: TenLoai
            var loaiDict = _context.PhanLoaiSK.ToDictionary(pl => pl.MaLoaiSK, pl => pl.TenLoai);
         
            // 1. Tạo danh sách đếm số người đã đăng ký để truyền qua View
            // Key: MaSK, Value: Số lượng đã đăng ký
            var countDict = _context.DangKy
                .AsEnumerable()
                .GroupBy(dk => dk.MaSK)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.CountDict = countDict; // Truyền vào ViewBag

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.KhoaList = _context.Khoa.ToList();
            ViewBag.LoaiList = _context.PhanLoaiSK.ToList();
            ViewBag.LoaiDict = loaiDict; // Truyền từ điển sang View

            // Lưu lại các giá trị lọc
            ViewBag.TrangThai = trangThai;
            ViewBag.Khoa = khoa;
            ViewBag.Loai = loai;
            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;

            return View("~/Views/SuKien/Index_SuKien.cshtml", danhSachSuKien);
        }

        // ============================================================
        // TÌM KIẾM CHUNG 
        // ============================================================
        public IActionResult TimKiemChung(string tuKhoa)
        {
            ViewBag.TuKhoaDaNhap = tuKhoa;
            if (string.IsNullOrEmpty(tuKhoa)) return View("~/Views/TimKiem/TimKiemChung.cshtml", new List<dynamic>());

            string tuKhoaGoc = tuKhoa.Trim().ToLower();
            string tuKhoaKhongDau = LoaiBoDauTiengViet(tuKhoaGoc);

            var queryRaw = (from sk in _context.SuKien
                            join pl in _context.PhanLoaiSK on sk.MaLoaiSK equals pl.MaLoaiSK into plGroup
                            from pl in plGroup.DefaultIfEmpty()
                            join ks in _context.Khoa_SuKien on sk.MaSK equals ks.MaSK into ksGroup
                            from ks in ksGroup.DefaultIfEmpty()
                            join k in _context.Khoa on ks.MaKhoa equals k.MaKhoa into kGroup
                            from k in kGroup.DefaultIfEmpty()
                            select new { sk.MaSK, sk.TenSK, MaKhoa = k != null ? k.MaKhoa : "", TenKhoa = k != null ? k.TenKhoa : "Tất cả các Khoa", TenLoaiSK = pl != null ? pl.TenLoai : "Chưa phân loại", sk.start_date, sk.Start_time, sk.end_date, sk.End_time, sk.DiaDiem, sk.TrangThai, sk.max_participants }).AsQueryable();

            var danhSachTuDB = queryRaw.ToList();
            var ketQuaLoc = danhSachTuDB.Where(x =>
                (x.TenKhoa != null && x.TenKhoa.ToLower().Contains(tuKhoaGoc)) || (x.TenKhoa != null && LoaiBoDauTiengViet(x.TenKhoa.ToLower()).Contains(tuKhoaKhongDau)) ||
                (x.MaKhoa != null && x.MaKhoa.ToLower().Contains(tuKhoaGoc)) ||
                (x.TenLoaiSK != null && x.TenLoaiSK.ToLower().Contains(tuKhoaGoc)) || (x.TenLoaiSK != null && LoaiBoDauTiengViet(x.TenLoaiSK.ToLower()).Contains(tuKhoaKhongDau)) ||
                (x.TenSK != null && x.TenSK.ToLower().Contains(tuKhoaGoc)) || (x.TenSK != null && LoaiBoDauTiengViet(x.TenSK.ToLower()).Contains(tuKhoaKhongDau))
            ).Select(x => {
                System.Dynamic.ExpandoObject item = new System.Dynamic.ExpandoObject();
                var dict = (IDictionary<string, object>)item;
                dict["MaSK"] = x.MaSK; dict["TenSK"] = x.TenSK; dict["TenKhoa"] = x.TenKhoa; dict["TenLoaiSK"] = x.TenLoaiSK;
                dict["StartDateStr"] = x.start_date.HasValue ? (x.Start_time.HasValue ? $"{x.start_date.Value:dd/MM/yyyy} {x.Start_time.Value:hh\\:mm}" : x.start_date.Value.ToString("dd/MM/yyyy")) : "Chưa cập nhật";
                dict["EndDateStr"] = x.end_date.HasValue ? (x.End_time.HasValue ? $"{x.end_date.Value:dd/MM/yyyy} {x.End_time.Value:hh\\:mm}" : x.end_date.Value.ToString("dd/MM/yyyy")) : "Chưa cập nhật";
                dict["DiaDiem"] = x.DiaDiem; dict["MaxParticipants"] = x.max_participants; dict["TrangThai"] = x.TrangThai;
                dict["SoNguoiDaDangKy"] = _context.DangKy.Count(dk => dk.MaSK == x.MaSK);
                return item;
            }).ToList();

            return View("~/Views/TimKiem/TimKiemChung.cshtml", ketQuaLoc);
        }

        public IActionResult TatCaSuKien()
        {
            // Logic TatCaSuKien đã được tối ưu trong TimKiemChung 
            return RedirectToAction("TimKiemChung", new { tuKhoa = "" });
        }

        public IActionResult ChiTietSK(string id)
        {
            // 1. Kiểm tra ID có bị null hay không
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Mã sự kiện không tồn tại.");
            }

            // 2. Tìm sự kiện
            var suKien = _context.SuKien.FirstOrDefault(s => s.MaSK == id);

            // 3. Nếu không tìm thấy sự kiện, trả về lỗi 404
            if (suKien == null)
            {
                return NotFound("Không tìm thấy sự kiện này.");
            }
            // Lấy thông tin sự kiện
                  
            var tenKhoa = _context.Khoa_SuKien
                .Where(ks => ks.MaSK == id)
                .Join(_context.Khoa, //
                      ks => ks.MaKhoa,
                      k => k.MaKhoa,
                      (ks, k) => k.TenKhoa)
                .FirstOrDefault();

            ViewBag.TenDonVi = tenKhoa ?? "Chưa có";

            // 4. Đếm số lượng người đã đăng ký
            ViewBag.SoLuongDangKy = _context.DangKy.Count(d => d.MaSK == id);

                  // 5. Trả về View cùng với model suKien
            return View("~/Views/SuKien/ChiTietSK.cshtml", suKien);
        }

        private string LoaiBoDauTiengViet(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string arr1 = "áàảãạâấầẩẫậăắằẳẵặéèẻẽẹêếềểễệíìỉĩịóòỏõọôốồổỗộơớờởỡợúùủũụưứừửữựýỳỷỹỵđ";
            string arr2 = "aaaaaaaaaaaaaaaaaeeeeeeeeeeeiiiiiooooooooooooooooouuuuuuuuuuuuyyyyyd";
            for (int i = 0; i < arr1.Length; i++) text = text.Replace(arr1[i], arr2[i]);
            return text;
        }

        public IActionResult Login() => View();
        public IActionResult Register() => View();
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}