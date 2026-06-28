/* GỬI VỚI MAIL CLIENT - server SMTP. 
 * DO Khi gửi từ localhost, nhiều nhà cung cấp SMTP (như Google) sẽ chặn các yêu cầu gửi đi nếu cấu hình không chuẩn.
 * Vì vậy sẽ dùng mailtrap.io để test chức năng này
 * 
using System;
using System.Linq;
using MailKit.Net.Smtp;
using MailKit.Security; // Thêm thư viện để bảo mật kết nối
using MimeKit;
using DA09_QLSK.Models;
using System.Net.Http;

namespace DA09_QLSK.Services
{
    public class EmailService
    {
        private readonly ApplicationDbContext _context;

        public EmailService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Hệ thống Quản lý Sự kiện", "email_cua_ban@gmail.com"));
                message.To.Add(new MailboxAddress("Sinh viên", toEmail));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = body };

                using (var client = new SmtpClient())
                   {
                       // Sử dụng SecureSocketOptions.StartTls để kết nối an toàn hơn
                       client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                       // Lưu ý: Mật khẩu phải là "App Password" (Mật khẩu ứng dụng) từ Google
                       client.Authenticate("email_cua_ban@gmail.com", "mat_khau_ung_dung");

                       client.Send(message);
                       client.Disconnect(true);
                   }
                

               
            }
            catch (Exception ex)
            {
                // Ghi log lỗi để bạn kiểm tra trong cửa sổ Output của Visual Studio
                Console.WriteLine($"Lỗi gửi mail tới {toEmail}: {ex.Message}");
            }
        }

        public void SendFromNotification(string toEmail, string maSK)
        {
            // Kiểm tra kỹ tên DbSet trong ApplicationDbContext của bạn (thường là Notifications có 's')
            var notify = _context.Notifications.FirstOrDefault(n => n.MaSK == maSK);

            if (notify != null)
            {
                SendEmail(toEmail, notify.title, notify.content);
            }
            else
            {
                Console.WriteLine($"Không tìm thấy thông báo cho sự kiện: {maSK}");
            }
        }
    }
}

/////////// đÃ XONG GỬI VỚI MAIL CLIENT///////////*/
///
/// 
////// hÁM SỬ DỤNG KIỂM TRA TẠO VỚI mailtrap.io: ĐÂY LÀ 1 HỘP HỨNG TẤT CẢ CÁC MAIL ĐƯỢC GỬI ĐI TỪ DỰ ÁN

using System;
using System.Linq;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using DA09_QLSK.Models;

namespace DA09_QLSK.Services
{
    public class EmailService
    {
        private readonly ApplicationDbContext _context;

        public EmailService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                // Thay đổi From nếu cần thiết (địa chỉ gửi đi)
                message.From.Add(new MailboxAddress("Hệ thống Quản lý Sự kiện", "noreply@qlsk.com"));
                message.To.Add(new MailboxAddress("Sinh viên", toEmail));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = body };

                using (var client = new SmtpClient())
                {
                    // Cấu hình kết nối tới Mailtrap
                    // Host: smtp.mailtrap.io, Port: 2525
                    client.Connect("smtp.mailtrap.io", 2525, SecureSocketOptions.StartTls);

                    // Thay thế bằng Username và Password từ Mailtrap Inbox của bạn
                    client.Authenticate("ecd738b557828e", "df2c0e524e37ea");

                    client.Send(message);
                    client.Disconnect(true);
                }

                // In ra thông báo thành công vào cửa sổ Output
                System.Diagnostics.Debug.WriteLine($"Đã gửi mail tới {toEmail} thành công qua Mailtrap.");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi để bạn kiểm tra trong cửa sổ Output của Visual Studio
                System.Diagnostics.Debug.WriteLine($"Lỗi gửi mail tới {toEmail}: {ex.Message}");
            }
        }

        public void SendFromNotification(string toEmail, string maSK)
        {
            var notify = _context.Notifications.FirstOrDefault(n => n.MaSK == maSK);

            if (notify != null)
            {
                SendEmail(toEmail, notify.title, notify.content);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Không tìm thấy thông báo cho sự kiện: {maSK}");
            }
        }
    }
}