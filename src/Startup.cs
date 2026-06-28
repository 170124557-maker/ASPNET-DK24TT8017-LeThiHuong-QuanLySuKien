using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using DA09_QLSK.Models;
using System;
// THÊM THƯ VIỆN HANGFIRE
using Hangfire;

namespace DA09_QLSK
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // 1. Đăng ký kết nối SQL Server
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // 2. Kích hoạt dịch vụ Session
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // 3. THÊM DÒNG NÀY: Để có thể gọi Session trong các View (.cshtml)
            services.AddHttpContextAccessor();

            // 4. Đăng ký dịch vụ hỗ trợ mô hình MVC
            services.AddControllersWithViews();

            // 5. Đăng ký sử dụng dịch vụ email
            services.AddTransient<DA09_QLSK.Services.EmailService>();

            // 6. CẤU HÌNH HANGFIRE: Dùng chính SQL Server làm kho lưu trữ lịch
            services.AddHangfire(x => x.UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection")));
            services.AddHangfireServer();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IRecurringJobManager recurringJob)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // 5. Kích hoạt Session (Phải nằm giữa UseRouting và UseEndpoints)
            app.UseSession();

            // 7. KÍCH HOẠT HANGFIRE DASHBOARD (Truy cập tại /hangfire)
            app.UseHangfireDashboard();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            // 8. ĐỊNH NGHĨA TÁC VỤ TỰ ĐỘNG: Chạy kiểm tra nhắc lịch mỗi ngày lúc 8:00 sáng
            // Lưu ý: Bạn cần tạo hàm 'CheckAndSendReminders' trong một Service hoặc Controller để logic gửi email chạy tại đây
            // recurringJob.AddOrUpdate("NhacLichHangNgay", () => Console.WriteLine("Hangfire đang chạy kiểm tra lịch..."), Cron.Daily(8, 0));
        }

    }
}