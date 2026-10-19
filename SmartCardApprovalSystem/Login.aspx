<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <title>系統登入 - 智慧卡申請與四級職官章簽核系統</title>
    <style>
        * { box-sizing: border-box; font-family: "Microsoft JhengHei", sans-serif; }
        body {
            margin: 0; min-height: 100vh;
            display: flex; align-items: center; justify-content: center;
            background: radial-gradient(circle at 20% 20%, #1e3a8a 0%, transparent 45%),
                        radial-gradient(circle at 80% 80%, #312e81 0%, transparent 45%),
                        linear-gradient(135deg, #090d16 0%, #0f172a 100%);
            color: #f8fafc;
        }
        .login-glass {
            width: 100%; max-width: 500px;
            background: rgba(255, 255, 255, 0.08);
            backdrop-filter: blur(18px);
            border: 1px solid rgba(255, 255, 255, 0.2);
            border-radius: 20px;
            padding: 36px;
            box-shadow: 0 20px 50px rgba(0, 0, 0, 0.5);
        }
        .title {
            font-size: 1.45rem; font-weight: 700; text-align: center;
            margin-bottom: 8px;
            background: linear-gradient(90deg, #38bdf8, #818cf8);
            -webkit-background-clip: text; -webkit-text-fill-color: transparent;
        }
        .subtitle { text-align: center; color: #94a3b8; font-size: 0.88rem; margin-bottom: 26px; }
        .form-group { margin-bottom: 18px; }
        .form-label { display: block; font-size: 0.9rem; margin-bottom: 6px; color: #cbd5e1; }
        .glass-input {
            width: 100%; padding: 12px 14px; border-radius: 10px;
            background: rgba(15, 23, 42, 0.7); border: 1px solid rgba(255, 255, 255, 0.2);
            color: #fff; font-size: 0.96rem; outline: none;
        }
        .btn-login {
            width: 100%; padding: 13px; border: none; border-radius: 12px;
            background: linear-gradient(135deg, #0ea5e9 0%, #6366f1 100%);
            color: #fff; font-size: 1rem; font-weight: 700; cursor: pointer;
            box-shadow: 0 6px 20px rgba(56, 189, 248, 0.35);
            transition: transform 0.2s;
        }
        .btn-login:hover { transform: translateY(-2px); }
        .quick-box {
            margin-top: 26px; padding-top: 18px;
            border-top: 1px solid rgba(255, 255, 255, 0.14);
        }
        .quick-grid {
            display: grid; grid-template-columns: 1fr 1fr; gap: 8px; margin-top: 10px;
        }
        .btn-quick {
            padding: 8px 10px; border-radius: 8px; font-size: 0.8rem;
            background: rgba(255, 255, 255, 0.08); border: 1px solid rgba(255, 255, 255, 0.2);
            color: #e2e8f0; cursor: pointer; text-align: left;
            transition: background 0.2s, border-color 0.2s;
        }
        .btn-quick:hover { background: rgba(56, 189, 248, 0.2); border-color: #38bdf8; }
        .alert-err {
            background: rgba(244, 63, 94, 0.2); border: 1px solid #f43f5e;
            color: #fecdd3; padding: 10px 14px; border-radius: 10px; margin-bottom: 16px; font-size: 0.88rem;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-glass">
            <div class="title">🛡️ 智慧卡申請與四級職官章簽核系統</div>
            <div class="subtitle">ASP.NET Web Forms + 純 C# 動態篆刻與 PDF 簽核引擎</div>

            <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="alert-err">
                <asp:Label ID="lblError" runat="server"></asp:Label>
            </asp:Panel>

            <div class="form-group">
                <label class="form-label">使用者工號 (EmployeeNo)</label>
                <asp:TextBox ID="txtEmpNo" runat="server" CssClass="glass-input" placeholder="例如：EMP001 或 REV001"></asp:TextBox>
            </div>

            <div class="form-group">
                <label class="form-label">登入密碼 (Password)</label>
                <asp:TextBox ID="txtPassword" runat="server" CssClass="glass-input" TextMode="Password" placeholder="預設密碼皆為 123456"></asp:TextBox>
            </div>

            <asp:Button ID="btnLogin" runat="server" Text="🔐 登入系統 (依 RoleLevel 自動分流)" CssClass="btn-login" OnClick="btnLogin_Click" />

            <div class="quick-box">
                <div style="font-size: 0.82rem; color: #94a3b8;">⚡ 測試快速登入 (點擊直接以該層級身分進入系統)：</div>
                <div class="quick-grid">
                    <asp:Button ID="btnQuickApplicant" runat="server" Text="👤 申請人：林申請 (Role=0)" CssClass="btn-quick" CommandArgument="EMP001" OnClick="btnQuickLogin_Click" style="grid-column: span 2; text-align:center; font-weight:700; color:#38bdf8;" />
                    <asp:Button ID="btnQuickRev1" runat="server" Text="① 一級直屬主管：陳初審" CssClass="btn-quick" CommandArgument="REV001" OnClick="btnQuickLogin_Click" />
                    <asp:Button ID="btnQuickRev2" runat="server" Text="② 二級部門主管：張複審" CssClass="btn-quick" CommandArgument="REV002" OnClick="btnQuickLogin_Click" />
                    <asp:Button ID="btnQuickRev3" runat="server" Text="③ 三級資安管制：王資安" CssClass="btn-quick" CommandArgument="REV003" OnClick="btnQuickLogin_Click" />
                    <asp:Button ID="btnQuickRev4" runat="server" Text="④ 四級製卡中心：趙製卡" CssClass="btn-quick" CommandArgument="REV004" OnClick="btnQuickLogin_Click" />
                </div>
            </div>
        </div>
    </form>
</body>
</html>
