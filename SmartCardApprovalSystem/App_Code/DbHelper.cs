using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

public static class DbHelper
{
    private static bool _initialized = false;
    private static readonly object _lock = new object();

    public static string ConnectionString
    {
        get
        {
            var setting = ConfigurationManager.ConnectionStrings["SmartCardDB"];
            return setting != null
                ? setting.ConnectionString
                : @"Server=(localdb)\MSSQLLocalDB;Database=SmartCardDB;Trusted_Connection=True;MultipleActiveResultSets=True;";
        }
    }

    /// <summary>
    /// 首次執行時自動檢查並建立 SmartCardDB 與三張資料表及預設帳號
    /// </summary>
    public static void EnsureDatabaseInitialized()
    {
        if (_initialized) return;
        lock (_lock)
        {
            if (_initialized) return;
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);
                string targetDb = builder.InitialCatalog;
                builder.InitialCatalog = "master";

                using (SqlConnection masterConn = new SqlConnection(builder.ConnectionString))
                {
                    masterConn.Open();
                    string checkDbSql = string.Format("IF DB_ID('{0}') IS NULL CREATE DATABASE [{0}];", targetDb);
                    using (SqlCommand cmd = new SqlCommand(checkDbSql, masterConn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string initTablesSql = @"
                        IF OBJECT_ID('dbo.Users', 'U') IS NULL
                        BEGIN
                            CREATE TABLE dbo.Users (
                                UserID          INT IDENTITY(1,1) PRIMARY KEY,
                                EmployeeNo      NVARCHAR(20)  NOT NULL UNIQUE,
                                PasswordHash    NVARCHAR(100) NOT NULL,
                                FullName        NVARCHAR(50)  NOT NULL,
                                Department      NVARCHAR(50)  NOT NULL,
                                JobTitle        NVARCHAR(50)  NOT NULL,
                                RoleLevel       INT           NOT NULL,
                                StampShape      NVARCHAR(20)  DEFAULT 'Square',
                                StampTopTitle   NVARCHAR(30)  NULL,
                                StampCenterText NVARCHAR(30)  NULL,
                                CustomStampPath NVARCHAR(255) NULL,
                                CreatedAt       DATETIME      DEFAULT GETDATE()
                            );

                            INSERT INTO dbo.Users (EmployeeNo, PasswordHash, FullName, Department, JobTitle, RoleLevel, StampShape, StampTopTitle, StampCenterText)
                            VALUES 
                            ('EMP001', '123456', N'林申請', N'系統工程處',   N'工程師',     0, 'Square', NULL,          NULL),
                            ('REV001', '123456', N'陳初審', N'系統工程處',   N'直屬組長',   1, 'Square', N'系統工程處',   N'組長陳初審章'),
                            ('REV002', '123456', N'張複審', N'系統工程處',   N'部門處長',   2, 'Square', N'系統工程處',   N'處長張複審章'),
                            ('REV003', '123456', N'王資安', N'資安管制室',   N'資安管制官', 3, 'Round',  N'資安管制室',   N'資安官王資安'),
                            ('REV004', '123456', N'趙製卡', N'總務製卡中心', N'核定發卡官', 4, 'Round',  N'總務製卡中心', N'製卡中心核定章');
                        END

                        IF OBJECT_ID('dbo.CardApplications', 'U') IS NULL
                        BEGIN
                            CREATE TABLE dbo.CardApplications (
                                AppID           INT IDENTITY(1,1) PRIMARY KEY,
                                ApplicantID     INT           NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserID),
                                CardCategory    NVARCHAR(50)  NOT NULL,
                                ApplyReasons    NVARCHAR(200) NOT NULL,
                                PhotoPath       NVARCHAR(255) NOT NULL,
                                PdfPath         NVARCHAR(255) NOT NULL,
                                CurrentStage    INT           NOT NULL DEFAULT 1,
                                IssuedCardNo    NVARCHAR(50)  NULL,
                                Level1StampPath NVARCHAR(255) NULL,
                                Level2StampPath NVARCHAR(255) NULL,
                                Level3StampPath NVARCHAR(255) NULL,
                                Level4StampPath NVARCHAR(255) NULL,
                                Level1StampTime DATETIME      NULL,
                                Level2StampTime DATETIME      NULL,
                                Level3StampTime DATETIME      NULL,
                                Level4StampTime DATETIME      NULL,
                                CreatedAt       DATETIME      DEFAULT GETDATE(),
                                UpdatedAt       DATETIME      DEFAULT GETDATE()
                            );
                        END

                        IF OBJECT_ID('dbo.ApprovalLogs', 'U') IS NULL
                        BEGIN
                            CREATE TABLE dbo.ApprovalLogs (
                                LogID           INT IDENTITY(1,1) PRIMARY KEY,
                                AppID           INT           NOT NULL FOREIGN KEY REFERENCES dbo.CardApplications(AppID),
                                ReviewerID      INT           NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserID),
                                RoleLevel       INT           NOT NULL,
                                ActionResult    NVARCHAR(20)  NOT NULL,
                                StampSnapshot   NVARCHAR(255) NULL,
                                ActionTime      DATETIME      DEFAULT GETDATE()
                            );
                        END";

                    using (SqlCommand cmd = new SqlCommand(initTablesSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                _initialized = true;
            }
            catch
            {
                // 若使用者更改了 Web.config 指向外部資料庫，則略過 master 自動建庫
            }
        }
    }
}
