using System;
using System.ComponentModel.DataAnnotations; // Cần thiết cho [Key]
using System.ComponentModel.DataAnnotations.Schema; // Cần thiết cho [ForeignKey]

namespace DA09_QLSK.Models
{
    public class Notification
    {
        [Key] // Định nghĩa Khóa chính
        public int notification_id { get; set; }

        [Required] // Nên thêm để đảm bảo trường này không được để trống
        public string MaSK { get; set; }

        [Required]
        public string title { get; set; }

        public string content { get; set; }

        public DateTime? sent_at { get; set; }

        // Thuộc tính điều hướng (Navigation Property) để lấy thông tin sự kiện
        [ForeignKey("MaSK")]
        public virtual SuKien SuKien { get; set; }
        public DateTime? ScheduledDate { get; set; } // Ngày gửi
        public TimeSpan? ScheduledTime { get; set; } // Giờ gửi
    }
}