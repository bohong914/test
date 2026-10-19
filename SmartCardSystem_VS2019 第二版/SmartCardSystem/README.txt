===================================================================
智慧卡申請與四級簽核系統 (Visual Studio 2019 / ASP.NET Web Forms)
===================================================================

【資料夾結構說明】
1. SmartCardSystem.sln                -> Visual Studio 2019 方案檔 (請直接雙擊開啟)
2. Database/InitDB.sql                -> MS SQL 資料庫建立與測試帳號初始化腳本
3. SmartCardSystem/
   ├── Web.config                     -> 資料庫連線字串與上傳大小設定
   ├── Login.aspx (.cs/.designer)     -> 登入頁 (依 Session["RoleLevel"] 自動判斷導向)
   ├── ApplyCard.aspx (.cs/.designer) -> 申請人頁面 (含 FileUpload 證件照上傳、退回重填與進度追蹤)
   ├── ReviewPanel.aspx (.cs/.designer)-> 後台四級審核頁 (依登入層級過濾待審單、退回直接退給申請人)
   └── Uploads/                       -> 儲存上傳之證件照/大頭照資料夾

【快速啟動步驟】
步驟 1：建立 MS SQL 資料庫
  - 開啟 SQL Server Management Studio (SSMS)
  - 打開並執行 `Database/InitDB.sql`，將會自動建立 `SmartCardDB` 資料庫、3 張資料表與預設測試帳號。

步驟 2：確認資料庫連線字串
  - 用 Visual Studio 2019 開啟 `SmartCardSystem.sln`
  - 打開 `Web.config`，確認 `<connectionStrings>` 中的 `Data Source=localhost;` 是否符合您的 SQL Server 名稱（若使用 SQL Express 請改為 `Data Source=.\SQLEXPRESS;`）。

步驟 3：啟動偵錯 (F5)
  - 將 `Login.aspx` 設為起始頁，按下 F5 即可執行。

【預設測試帳號 (密碼皆為 1234)】
  - 一般申請人：user01 (王小明) / user02 (李小華)
  - 第一級審核 (直屬主管)：rev1
  - 第二級審核 (部門主管)：rev2
  - 第三級審核 (資安管制官)：rev3
  - 第四級審核 (製卡中心)：rev4
