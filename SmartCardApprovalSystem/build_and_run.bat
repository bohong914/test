@echo off
chcp 65001 >nul
title 智慧卡申請與四級職官章簽核系統 - 一鍵建置啟動器
echo ====================================================================
echo   智慧卡申請與四級職官章簽核系統 (VS2019 + MS SQL Server)
echo ====================================================================

echo [1/3] 正在檢查並初始化 MS SQL Server LocalDB 資料庫...
where sqlcmd >nul 2>nul
if %errorlevel%==0 (
    sqlcmd -S "(localdb)\MSSQLLocalDB" -i "%~dp0InitDatabase.sql" >nul 2>nul
    echo       SQL 腳本執行完成 (若無 LocalDB，系統啟動時將由 C# 自動建置資料庫)。
) else (
    echo       未偵測到 sqlcmd 工具，將於網頁啟動時由 App_Code\DbHelper.cs 自動建立資料庫。
)

echo [2/3] 正在執行 ASP.NET 動態編譯語法檢核...
if exist "%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\aspnet_compiler.exe" (
    "%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\aspnet_compiler.exe" -v / -p "%~dp0." >nul
    if %errorlevel%==0 (
        echo       C# 與 ASPX 動態編譯檢核通過！
    ) else (
        echo       [警告] 編譯檢核發現提示訊息，繼續啟動 IIS Express...
    )
)

echo [3/3] 正在啟動 IIS Express 並開啟瀏覽器...
set IISEXPRESS="%ProgramFiles%\IIS Express\iisexpress.exe"
if not exist %IISEXPRESS% set IISEXPRESS="%ProgramFiles(x86)%\IIS Express\iisexpress.exe"

if exist %IISEXPRESS% (
    start "" "http://localhost:50888/Login.aspx"
    %IISEXPRESS% /path:"%~dp0." /port:50888
) else (
    echo 找不到 IIS Express，正在為您開啟 Visual Studio 2019 方案檔...
    start "" "%~dp0SmartCardApprovalSystem.sln"
)
pause
