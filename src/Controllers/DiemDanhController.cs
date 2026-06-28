
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using QRCoder;
using DA09_QLSK.Models;

public class DiemDanhController : Controller
{
    private readonly ApplicationDbContext _context;
    public DiemDanhController(ApplicationDbContext context) => _context = context;

    // Hiển thị view để sinh viên quét mã
    public IActionResult DiemDanh(string maSK)
    {
        string maSV = HttpContext.Session.GetString("MaSV");
        if (string.IsNullOrEmpty(maSV)) return RedirectToAction("DangNhap", "DangNhap");

        var suKien = _context.SuKien.FirstOrDefault(s => s.MaSK == maSK);
        if (suKien == null) return NotFound();
        return View(suKien);
    }

    // Tạo QR Code lưu URL xác nhận
    public IActionResult TaoQRCode(string maSK)
    {
        string url = Url.Action("DiemDanh", "DiemDanh", new { maSK = maSK }, Request.Scheme);
        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q))
        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        {
            return File(qrCode.GetGraphic(20), "image/png");
        }
    }

    // Logic xử lý khi sinh viên quét mã

    public IActionResult XacNhanDiemDanh(string maSK)
    {
        // 1. Lấy mã sinh viên từ Session (đảm bảo sinh viên đã đăng nhập trên điện thoại)
        string maSV = HttpContext.Session.GetString("MaSV");

        if (string.IsNullOrEmpty(maSV))
        {
            return Content("Vui lòng đăng nhập vào hệ thống trên điện thoại trước khi quét mã!");
        }

        // 2. Tìm bản ghi đăng ký của sinh viên này trong sự kiện đó
        var dk = _context.DangKy.FirstOrDefault(d => d.MaSV == maSV && d.MaSK == maSK);

        if (dk != null)
        {
            // 3. Cập nhật trạng thái
            dk.is_attended = true; // Đánh dấu đã điểm danh
            dk.attended_at = DateTime.Now; // Lưu thời gian điểm danh

            _context.SaveChanges(); // Lưu vào DB

            return Content("Điểm danh thành công! Bạn có thể đóng trang này.");
        }

        return Content("Bạn chưa đăng ký sự kiện này nên không thể điểm danh!");
    }
}

/*

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using QRCoder;
using DA09_QLSK.Models;

public class DiemDanhController : Controller
{
    private readonly ApplicationDbContext _context;
    public DiemDanhController(ApplicationDbContext context) => _context = context;

    // --- HÀM KIỂM TRA THIẾT BỊ (DÙNG CHUNG) ---
    private bool IsMobile()
    {
        var userAgent = Request.Headers["User-Agent"].ToString().ToLower();
        return userAgent.Contains("android") || userAgent.Contains("iphone") || userAgent.Contains("ipad");
    }

    // --- VIEW ĐIỂM DANH (CHỈ CHO PHÉP MỞ TRÊN ĐIỆN THOẠI) ---
    public IActionResult DiemDanh(string maSK)
    {
        // Kiểm tra thiết bị
       // if (!IsMobile())
     //   {
      //      return Content("Vui lòng sử dụng điện thoại để quét mã QR và điểm danh.");
      //  }

        string maSV = HttpContext.Session.GetString("MaSV");
        if (string.IsNullOrEmpty(maSV)) return RedirectToAction("DangNhap", "DangNhap");

        var suKien = _context.SuKien.FirstOrDefault(s => s.MaSK == maSK);
        if (suKien == null) return NotFound();

        return View(suKien);
    }

    // --- TẠO QR CODE (GIỮ NGUYÊN) ---
    public IActionResult TaoQRCode(string maSK)
    {
        string url = Url.Action("DiemDanh", "DiemDanh", new { maSK = maSK }, Request.Scheme);

        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q))
        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        {
            return File(qrCode.GetGraphic(20), "image/png");
        }
    }

    // --- XỬ LÝ ĐIỂM DANH (GỌI KHI SINH VIÊN BẤM XÁC NHẬN) ---
    public IActionResult XacNhanDiemDanh(string maSK)
    {
        // Kiểm tra thiết bị (tránh gian lận giả lập trên máy tính)
        if (!IsMobile())
        {
            return Content("Hành động không hợp lệ! Vui lòng điểm danh trên điện thoại.");
        }

        string maSV = HttpContext.Session.GetString("MaSV");

        if (string.IsNullOrEmpty(maSV))
        {
            return Content("Vui lòng đăng nhập vào hệ thống trên điện thoại trước khi quét mã!");
        }

        var dk = _context.DangKy.FirstOrDefault(d => d.MaSV == maSV && d.MaSK == maSK);

        if (dk != null)
        {
            dk.is_attended = true;
            dk.attended_at = DateTime.Now;

            _context.SaveChanges();

            return Content("Điểm danh thành công! Bạn có thể đóng trang này.");
        }

        return Content("Bạn chưa đăng ký sự kiện này nên không thể điểm danh!");
    }
} */