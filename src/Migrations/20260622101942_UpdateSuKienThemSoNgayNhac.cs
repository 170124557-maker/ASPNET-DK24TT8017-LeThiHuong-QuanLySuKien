using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DA09_QLSK.Migrations
{
    public partial class UpdateSuKienThemSoNgayNhac : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Account_id = table.Column<string>(nullable: false),
                    account_name = table.Column<string>(nullable: true),
                    email = table.Column<string>(nullable: true),
                    password = table.Column<string>(nullable: true),
                    role = table.Column<string>(nullable: true),
                    ThoiGianTao = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Account_id);
                });

            migrationBuilder.CreateTable(
                name: "DangKy",
                columns: table => new
                {
                    MaDK = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaSV = table.Column<string>(nullable: true),
                    MaSK = table.Column<string>(nullable: true),
                    registration_date = table.Column<DateTime>(nullable: true),
                    is_attended = table.Column<bool>(nullable: false),
                    attended_at = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKy", x => x.MaDK);
                });

            migrationBuilder.CreateTable(
                name: "Khoa",
                columns: table => new
                {
                    MaKhoa = table.Column<string>(nullable: false),
                    TenKhoa = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Khoa", x => x.MaKhoa);
                });

            migrationBuilder.CreateTable(
                name: "Khoa_SuKien",
                columns: table => new
                {
                    MaKhoa = table.Column<string>(nullable: false),
                    MaSK = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Khoa_SuKien", x => new { x.MaKhoa, x.MaSK });
                });

            migrationBuilder.CreateTable(
                name: "PhanLoaiSK",
                columns: table => new
                {
                    MaLoaiSK = table.Column<string>(nullable: false),
                    TenLoai = table.Column<string>(nullable: true),
                    MoTaLoaiSK = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanLoaiSK", x => x.MaLoaiSK);
                });

            migrationBuilder.CreateTable(
                name: "SinhVien",
                columns: table => new
                {
                    MaSV = table.Column<string>(nullable: false),
                    TenSV = table.Column<string>(nullable: true),
                    MaKhoa = table.Column<string>(nullable: true),
                    Account_id = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SinhVien", x => x.MaSV);
                });

            migrationBuilder.CreateTable(
                name: "SuKien",
                columns: table => new
                {
                    MaSK = table.Column<string>(nullable: false),
                    TenSK = table.Column<string>(maxLength: 200, nullable: true),
                    MotaSK = table.Column<string>(maxLength: 500, nullable: true),
                    MaLoaiSK = table.Column<string>(nullable: true),
                    start_date = table.Column<DateTime>(nullable: true),
                    end_date = table.Column<DateTime>(nullable: true),
                    Start_time = table.Column<TimeSpan>(nullable: true),
                    End_time = table.Column<TimeSpan>(nullable: true),
                    DiaDiem = table.Column<string>(nullable: true),
                    max_participants = table.Column<int>(nullable: true),
                    TrangThai = table.Column<string>(nullable: true),
                    qr_code_token = table.Column<string>(nullable: true),
                    SoNgayNhac = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuKien", x => x.MaSK);
                    table.ForeignKey(
                        name: "FK_SuKien_PhanLoaiSK_MaLoaiSK",
                        column: x => x.MaLoaiSK,
                        principalTable: "PhanLoaiSK",
                        principalColumn: "MaLoaiSK",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhanCong",
                columns: table => new
                {
                    MaPC = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaSK = table.Column<string>(nullable: true),
                    account_id = table.Column<string>(nullable: true),
                    NhiemVu = table.Column<string>(nullable: true),
                    ThoiGianPhanCong = table.Column<DateTime>(nullable: true),
                    TrangThaiPC = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanCong", x => x.MaPC);
                    table.ForeignKey(
                        name: "FK_PhanCong_SinhVien_account_id",
                        column: x => x.account_id,
                        principalTable: "SinhVien",
                        principalColumn: "MaSV",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    notification_id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaSK = table.Column<string>(nullable: false),
                    title = table.Column<string>(nullable: false),
                    content = table.Column<string>(nullable: true),
                    sent_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.notification_id);
                    table.ForeignKey(
                        name: "FK_Notifications_SuKien_MaSK",
                        column: x => x.MaSK,
                        principalTable: "SuKien",
                        principalColumn: "MaSK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_MaSK",
                table: "Notifications",
                column: "MaSK");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCong_account_id",
                table: "PhanCong",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_SuKien_MaLoaiSK",
                table: "SuKien",
                column: "MaLoaiSK");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "DangKy");

            migrationBuilder.DropTable(
                name: "Khoa");

            migrationBuilder.DropTable(
                name: "Khoa_SuKien");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PhanCong");

            migrationBuilder.DropTable(
                name: "SuKien");

            migrationBuilder.DropTable(
                name: "SinhVien");

            migrationBuilder.DropTable(
                name: "PhanLoaiSK");
        }
    }
}
