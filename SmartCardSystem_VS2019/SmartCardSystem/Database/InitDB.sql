-- =========================================================================
-- 智慧卡申請與四級簽核系統 - MS SQL 資料庫初始化腳本
-- 包含：Users(使用者與審核角色表)、CardApplications(申請主表)、ApprovalLogs(簽核紀錄表)
-- =========================================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SmartCardDB')
BEGIN
    CREATE DATABASE SmartCardDB;
END
GO

USE SmartCardDB;
GO

-- 先移除舊表 (依外鍵順序)
IF OBJECT_ID('ApprovalLogs', 'U') IS NOT NULL DROP TABLE ApprovalLogs;
IF OBJECT_ID('CardApplications', 'U') IS NOT NULL DROP TABLE CardApplications;
IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
GO

-- 1. 使用者與角色權限表
-- RoleLevel 定義：
-- 0 = 一般申請人 (Applicant)
-- 1 = 第一級審核官：直屬主管/導師 (身分與需求初審)
-- 2 = 第二級審核官：部門主管/科長 (權限合規複審)
-- 3 = 第三級審核官：資安管制官 (資安與背景審查)
-- 4 = 第四級審核官：製卡中心/總務 (配發實體卡號與核定)
CREATE TABLE Users (
    UserID NVARCHAR(50) PRIMARY KEY,
    PasswordHash NVARCHAR(100) NOT NULL,     -- 範例使用明碼展示，實務可改為 SHA256
    FullName NVARCHAR(50) NOT NULL,
    Department NVARCHAR(50) NOT NULL,
    RoleLevel INT NOT NULL DEFAULT 0         -- 0:申請人, 1~4:第一至第四級審核官
);

-- 預設塞入測試帳號 (密碼皆為 1234)
INSERT INTO Users (UserID, PasswordHash, FullName, Department, RoleLevel) VALUES
('user01', '1234', N'王小明', N'系統工程處', 0),
('user02', '1234', N'李小華', N'資訊網路組', 0),
('rev1',   '1234', N'陳組長 (一級初審)', N'系統工程處', 1),
('rev2',   '1234', N'林處長 (二級複審)', N'系統工程處', 2),
('rev3',   '1234', N'張資安官 (三級資安)', N'資安管控中心', 3),
('rev4',   '1234', N'趙總務 (四級製卡)', N'總務製卡中心', 4);
GO

-- 2. 智慧卡申請主表
CREATE TABLE CardApplications (
    AppID INT IDENTITY(1,1) PRIMARY KEY,
    ApplicantID NVARCHAR(50) NOT NULL REFERENCES Users(UserID),
    ApplicantName NVARCHAR(50) NOT NULL,
    Department NVARCHAR(50) NOT NULL,
    CardType NVARCHAR(30) NOT NULL,          -- 一般通行卡 / 機房特權卡 / 臨時訪客卡
    ApplyReason NVARCHAR(500) NOT NULL,
    PhotoPath NVARCHAR(255) NULL,            -- 大頭照/證件照相對路徑 (例如 ~/Uploads/xxx.jpg)
    CurrentStatus INT NOT NULL DEFAULT 1,    -- 1:待一級, 2:待二級, 3:待三級, 4:待四級, 5:已核准發卡, -1:退回申請人重填
    RejectReason NVARCHAR(500) NULL,         -- 若被退回，記錄最近一次退回原因與層級
    CardNumber NVARCHAR(50) NULL,            -- 第四級核發之實體卡號
    ApplyDate DATETIME NOT NULL DEFAULT GETDATE(),
    LastModified DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- 3. 四級簽核歷程紀錄表
CREATE TABLE ApprovalLogs (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    AppID INT NOT NULL REFERENCES CardApplications(AppID),
    ApprovalLevel INT NOT NULL,              -- 審核層級 (1~4)
    ReviewerID NVARCHAR(50) NOT NULL REFERENCES Users(UserID),
    ReviewerName NVARCHAR(50) NOT NULL,
    ActionResult NVARCHAR(20) NOT NULL,      -- 'Approved' 或 'Rejected'
    Comment NVARCHAR(500) NULL,
    ReviewDate DATETIME NOT NULL DEFAULT GETDATE()
);
GO
