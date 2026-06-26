using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DA09_QLSK.Models
{
    public class ThongKe_Report
    {
        public string TenSK { get; set; }
        public string TenKhoa { get; set; }
        public int SoLuongDK { get; set; }
        public int? SoLuongGioiHan { get; set; }
        public string TrangThai { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public TimeSpan? Start_time { get; set; }
        public TimeSpan? End_time { get; set; }
        public string TenLoai { get; set; }
    }
}