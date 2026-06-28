using Microsoft.AspNetCore.Mvc;
using System.Linq;
using DA09_QLSK.Models;

namespace DA09_QLSK.Controllers
{
    public class XoaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public XoaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSK(string id)
        {
            // 1. Tìm sự kiện theo ID
            var suKien = _context.SuKien.Find(id);

            // Nếu không tìm thấy, trả về thông báo lỗi
            if (suKien == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sự kiện!" });
            }

            try
            {
                // 2. Xóa trực tiếp bản ghi khỏi database
                _context.SuKien.Remove(suKien);

                // 3. Lưu thay đổi vào database
                _context.SaveChanges();

                // Trả về kết quả thành công cho Ajax
                return Json(new { success = true, message = "Đã xóa sự kiện thành công!" });
            }
            catch (System.Exception ex)
            {
                // Nếu có lỗi hệ thống (ví dụ: lỗi khóa ngoại hoặc database disconnect)
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
    }
}