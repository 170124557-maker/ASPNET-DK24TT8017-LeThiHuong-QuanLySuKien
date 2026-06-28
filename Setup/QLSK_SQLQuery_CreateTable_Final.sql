-- 1. Tạo Database
CREATE DATABASE DA09_QLSK;
GO
USE DA09_QLSK;
GO

-- 2. Tạo các bảng 
CREATE TABLE Accounts (
    account_id varchar(30) PRIMARY KEY,
    account_name nvarchar(100) NOT NULL,
    email varchar(100) NOT NULL,
    Password varchar(50) NOT NULL,
    Role varchar(20) NOT NULL,
    ThoiGianTao datetime NULL
);

CREATE TABLE Khoa (
    MaKhoa varchar(20) PRIMARY KEY,
    TenKhoa nvarchar(100) NOT NULL
);

CREATE TABLE PhanLoaiSK (
    MaLoaiSK varchar(30) PRIMARY KEY,
    TenLoai nvarchar(100) NOT NULL,
    MoTaLoaiSK nvarchar(500) NULL
);

CREATE TABLE SinhVien (
    MaSV varchar(30) PRIMARY KEY,
    TenSV nvarchar(100) NOT NULL,
    MaKhoa varchar(20) NOT NULL,
    account_id varchar(30) NOT NULL,
    CONSTRAINT FK_SinhVien_Khoa FOREIGN KEY (MaKhoa) REFERENCES Khoa(MaKhoa),
    CONSTRAINT FK_SinhVien_Accounts FOREIGN KEY (account_id) REFERENCES Accounts(account_id)
);

CREATE TABLE SuKien (
    MaSK varchar(30) PRIMARY KEY,
    TenSK nvarchar(200) NOT NULL,
    MotaSK nvarchar(500) NULL,
    MaLoaiSK varchar(30) NOT NULL,
    start_date date NOT NULL,
    end_date date NOT NULL,
    DiaDiem nvarchar(300) NOT NULL,
    max_participants int NULL,
    qr_code_token varchar(300) NULL,
    TrangThai nvarchar(100) NULL,
    Start_time time(7) NULL,
    End_time time(7) NULL,
    SoNgayNhac int NULL,
    CONSTRAINT FK_SuKien_PhanLoai FOREIGN KEY (MaLoaiSK) REFERENCES PhanLoaiSK(MaLoaiSK)
);

CREATE TABLE Khoa_SuKien (
    MaKhoa varchar(20),
    MaSK varchar(30),
    PRIMARY KEY (MaKhoa, MaSK),
    CONSTRAINT FK_KS_Khoa FOREIGN KEY (MaKhoa) REFERENCES Khoa(MaKhoa),
    CONSTRAINT FK_KS_SuKien FOREIGN KEY (MaSK) REFERENCES SuKien(MaSK)
);

CREATE TABLE DangKy (
    MaDK int IDENTITY(1,1) PRIMARY KEY,
    MaSV varchar(30) NOT NULL,
    MaSK varchar(30) NOT NULL,
    registration_date datetime NULL,
    is_attended bit NOT NULL,
    attended_at datetime NULL,
    CONSTRAINT FK_DangKy_SinhVien FOREIGN KEY (MaSV) REFERENCES SinhVien(MaSV),
    CONSTRAINT FK_DangKy_SuKien FOREIGN KEY (MaSK) REFERENCES SuKien(MaSK)
);

CREATE TABLE Notifications (
    notification_id int IDENTITY(1,1) PRIMARY KEY,
    MaSK varchar(30) NOT NULL,
    title nvarchar(300) NOT NULL,
    content nvarchar(400) NULL,
    sent_at datetime NULL,
    ScheduledDate datetime NULL,
    ScheduledTime time(7) NULL,
    CONSTRAINT FK_Noti_SuKien FOREIGN KEY (MaSK) REFERENCES SuKien(MaSK)
);

CREATE TABLE PhanCong (
    MaPC int IDENTITY(1,1) PRIMARY KEY,
    MaSK varchar(30) NOT NULL,
    account_id varchar(30) NOT NULL,
    NhiemVu nvarchar(200) NULL,
    ThoiGianPhanCong datetime NULL,
    TrangThaiPC nvarchar(100) NULL,
    CONSTRAINT FK_PC_SuKien FOREIGN KEY (MaSK) REFERENCES SuKien(MaSK),
    CONSTRAINT FK_PC_Accounts FOREIGN KEY (account_id) REFERENCES Accounts(account_id)
);