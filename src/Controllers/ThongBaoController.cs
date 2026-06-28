
using System;
using Microsoft.AspNetCore.Mvc;
using DA09_QLSK.Models;
using DA09_QLSK.Services;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ThongBaoController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly EmailService _emailService;

    public ThongBaoController(ApplicationDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // Hàm gọi thủ công từ View hoặc khi tạo sự kiện mới (chưa làm)
   
    /// //////////////  HÀM BÁM NÚT GỬI VỚI ALL EMAIL  ///////////
  
    
      [HttpPost]
      public IActionResult GuiThongBaoTuDong(int notification_id)
      {
          // 1. Lấy thông báo
          var notify = _context.Notifications.FirstOrDefault(n => n.notification_id == notification_id);
          if (notify == null) return NotFound("Thông báo không tồn tại!");

          // 2. Lấy danh sách ALL email

           var allEmails = _context.Accounts
              .Where(a => !string.IsNullOrEmpty(a.email))
              .Select(a => a.email)
              .Distinct()
              .ToList();

          if (!allEmails.Any())
          {
              TempData["ErrorMessage"] = "Không tìm thấy email nào!";
              return RedirectToAction("Index");
          }

          try
          {
              // Gửi email theo cách đồng bộ (khớp với service bạn đang có)
              foreach (var email in allEmails)
              {
                  _emailService.SendEmail(email, notify.title, notify.content);
                  System.Diagnostics.Debug.WriteLine($"Đã gửi thành công email tới: {email}");
              }

              // 3. Ghi thời gian và lưu lại
              notify.sent_at = DateTime.Now;
              _context.SaveChanges();

              TempData["SuccessMessage"] = $"Đã gửi thành công cho {allEmails.Count} emails!";
          }
          catch (Exception ex)
          {
              TempData["ErrorMessage"] = "Lỗi gửi: " + ex.Message;
          }


          return RedirectToAction("Index");
      }


    /* ---- Hàm kiểm tra chỉ gửi với 1 mail/////
     
     [HttpPost] 
     public IActionResult GuiThongBaoTuDong(int notification_id)
     {
         // 1. Tìm thông báo
         var notify = _context.Notifications.FirstOrDefault(n => n.notification_id == notification_id);
         if (notify == null) return NotFound("Thông báo không tồn tại!");

         // 2. CHỈ GỬI CHO EMAIL CỦA BẠN ĐỂ KIỂM TRA
         // Thay 'email_cua_ban@gmail.com' bằng email thật của bạn
         string myEmail = "lehuong11@yopmail.com";

         try
         {
             // Gửi thử nghiệm
             _emailService.SendEmail(myEmail, "TEST THÔNG BÁO: " + notify.title, notify.content);

             // 3. Ghi lại thời gian gửi (chỉ cập nhật khi test thành công)
             notify.sent_at = DateTime.Now;
             _context.SaveChanges();

             TempData["SuccessMessage"] = "Đã gửi email test thành công tới: " + myEmail;
         }
         catch (Exception ex)
         {
             TempData["ErrorMessage"] = "Lỗi khi gửi test: " + ex.Message;
         }

         return RedirectToAction("Index");
     }
     /// HẾT HÀM KIỂM TRA GỬI VỚI 1 EMAIL ////////////// */


    // Hàm Index để lấy dữ liệu từ DB truyền sang View.
    public IActionResult Index()
    {
        // Kiểm tra kỹ tên DbSet trong context của bạn là 'SuKien'
        ViewBag.SuKienList = _context.SuKien?.ToList() ?? new List<SuKien>();

        var list = _context.Notifications
            .Include(n => n.SuKien) // EF Core sẽ tự hiểu quan hệ qua ForeignKey MaSK
            .OrderByDescending(n => n.notification_id)
            .ToList();
       
        return View("~/Views/Admin/ThongBao.cshtml", list);
    }


    // Action để nhận dữ liệu từ Form
   
    [HttpPost]
    public IActionResult ThemThongBao(string maSK, string title, string content, DateTime? scheduledDate, TimeSpan? scheduledTime)
    {
        var newNotify = new Notification
        {
            MaSK = maSK,
            title = title,
            content = content,
         //   sent_at = DateTime.Now,
            ScheduledDate = scheduledDate,
            ScheduledTime = scheduledTime
        };

        _context.Notifications.Add(newNotify);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }
}