<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SmartCardSystem.Login" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>系統登入 - 智慧卡申請與四級簽核系統</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" />
</head>
<body class="bg-light">
    <form id="form1" runat="server" class="container mt-5">
        <div class="row justify-content-center">
            <div class="col-md-5">
                <div class="card shadow">
                    <div class="card-header bg-primary text-white text-center">
                        <h4 class="mb-0">智慧卡申請與簽核系統</h4>
                    </div>
                    <div class="card-body p-4">
                        <div class="mb-3">
                            <label class="form-label fw-bold">帳號 (UserID)</label>
                            <asp:TextBox ID="txtUserID" runat="server" CssClass="form-control" placeholder="例如: user01 或 rev1"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-bold">密碼 (Password)</label>
                            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="預設皆為 1234"></asp:TextBox>
                        </div>
                        <div class="d-grid mb-3">
                            <asp:Button ID="btnLogin" runat="server" Text="登入系統" CssClass="btn btn-primary btn-lg" OnClick="btnLogin_Click" />
                        </div>
                        <asp:Label ID="lblError" runat="server" CssClass="text-danger fw-bold d-block text-center"></asp:Label>
                        <hr />
                        <div class="small text-muted">
                            <strong>快速測試帳號 (密碼皆為 <code>1234</code>)：</strong>
                            <ul class="mb-0 mt-1">
                                <li><code>user01</code>：王小明 (一般申請人 - 自動導向申請/重填頁)</li>
                                <li><code>rev1</code>：陳組長 (第一級：直屬主管初審)</li>
                                <li><code>rev2</code>：林處長 (第二級：部門主管複審)</li>
                                <li><code>rev3</code>：張資安官 (第三級：資安背景審查)</li>
                                <li><code>rev4</code>：趙總務 (第四級：製卡配號核定)</li>
                            </ul>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
