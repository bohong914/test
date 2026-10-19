<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ApplicantDashboard.aspx.cs" Inherits="ApplicantDashboard" MaintainScrollPositionOnPostBack="true" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <title>智慧卡申請與進度查詢 - 智慧卡四級職官章簽核系統</title>
    <style>
        :root {
            --glass-bg: rgba(255, 255, 255, 0.08);
            --glass-border: rgba(255, 255, 255, 0.18);
            --text-main: #f8fafc;
            --text-muted: #94a3b8;
            --accent-blue: #38bdf8;
        }
        * { box-sizing: border-box; font-family: "Microsoft JhengHei", sans-serif; }
        body {
            margin: 0; min-height: 100vh;
            background: radial-gradient(circle at 15% 20%, #1e3a8a 0%, transparent 40%),
                        radial-gradient(circle at 85% 80%, #312e81 0%, transparent 45%),
                        linear-gradient(135deg, #090d16 0%, #0f172a 60%, #1e293b 100%);
            color: var(--text-main); padding-bottom: 50px;
        }
        .top-navbar {
            position: sticky; top: 0; z-index: 100;
            display: flex; justify-content: space-between; align-items: center;
            padding: 14px 32px; background: rgba(15, 23, 42, 0.8);
            backdrop-filter: blur(18px); border-bottom: 1px solid var(--glass-border);
        }
        .brand-title {
            font-size: 1.2rem; font-weight: 700;
            background: linear-gradient(90deg, #38bdf8, #818cf8);
            -webkit-background-clip: text; -webkit-text-fill-color: transparent;
        }
        .role-pill {
            padding: 5px 14px; border-radius: 999px; font-size: 0.82rem; font-weight: 600;
            background: rgba(56, 189, 248, 0.16); border: 1px solid rgba(56, 189, 248, 0.45); color: var(--accent-blue);
        }
        .main-container { max-width: 1400px; margin: 26px auto; padding: 0 24px; }
        .workspace-grid { display: grid; grid-template-columns: 1.05fr 0.95fr; gap: 24px; margin-bottom: 28px; }
        @media (max-width: 1080px) { .workspace-grid { grid-template-columns: 1fr; } }
        .glass-card {
            background: var(--glass-bg); backdrop-filter: blur(16px);
            border: 1px solid var(--glass-border); border-radius: 18px; padding: 24px;
            box-shadow: 0 10px 35px rgba(0, 0, 0, 0.35);
            transition: transform 0.25s ease, box-shadow 0.25s ease;
        }
        .glass-card:hover { transform: translateY(-3px); box-shadow: 0 14px 42px rgba(0, 0, 0, 0.45); }
        .card-header {
            display: flex; justify-content: space-between; align-items: center;
            border-bottom: 1px solid rgba(255, 255, 255, 0.12); padding-bottom: 12px; margin-bottom: 18px;
        }
        .card-title { margin: 0; font-size: 1.15rem; font-weight: 700; }
        .applicant-info-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; margin-bottom: 18px; }
        .info-box { background: rgba(15, 23, 42, 0.55); border: 1px solid rgba(255, 255, 255, 0.1); border-radius: 10px; padding: 10px 14px; }
        .info-label { font-size: 0.76rem; color: var(--text-muted); display: block; }
        .info-value { font-size: 0.96rem; font-weight: 700; color: var(--accent-blue); }
        .form-group { margin-bottom: 18px; }
        .form-label { display: block; font-size: 0.9rem; font-weight: 600; margin-bottom: 8px; }
        .glass-select, .glass-file {
            width: 100%; padding: 10px 12px; border-radius: 10px;
            background: rgba(15, 23, 42, 0.7); border: 1px solid rgba(255, 255, 255, 0.2); color: #fff;
        }
        .reason-checkbox-table { width: 100%; border-collapse: separate; border-spacing: 8px; }
        .reason-checkbox-table td {
            background: rgba(15, 23, 42, 0.55); border: 1px solid rgba(255, 255, 255, 0.14);
            border-radius: 8px; padding: 8px 10px;
        }
        .photo-upload-row { display: flex; align-items: center; gap: 16px; }
        .photo-thumb-box {
            width: 80px; height: 100px; border-radius: 8px; border: 1px dashed rgba(255, 255, 255, 0.3);
            background: rgba(15, 23, 42, 0.55); display: flex; align-items: center; justify-content: center; overflow: hidden;
        }
        .photo-thumb-box img { width: 100%; height: 100%; object-fit: cover; }
        .btn-action-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin-top: 20px; }
        .btn-gradient {
            padding: 12px 18px; border: none; border-radius: 10px; font-size: 0.92rem; font-weight: 700;
            color: #fff; cursor: pointer; text-decoration: none; display: inline-block; text-align: center;
        }
        .btn-preview { background: linear-gradient(135deg, #0ea5e9 0%, #6366f1 100%); }
        .btn-submit { background: linear-gradient(135deg, #10b981 0%, #059669 100%); }
        .btn-submit:disabled { background: rgba(148, 163, 184, 0.2); color: rgba(255, 255, 255, 0.4); cursor: not-allowed; }
        .btn-warn { background: linear-gradient(135deg, #f43f5e 0%, #e11d48 100%); }
        .btn-outline { background: rgba(255, 255, 255, 0.08); border: 1px solid rgba(255, 255, 255, 0.25); color: #e2e8f0; }
        .btn-small { padding: 6px 12px; font-size: 0.8rem; border-radius: 8px; }
        .pdf-preview-container {
            width: 100%; height: 530px; border-radius: 12px; overflow: hidden;
            border: 1px solid rgba(255, 255, 255, 0.16); background: rgba(9, 13, 22, 0.7);
        }
        .pdf-iframe { width: 100%; height: 100%; border: none; }
        .empty-preview-box { height: 100%; display: flex; flex-direction: column; align-items: center; justify-content: center; color: var(--text-muted); padding: 24px; text-align: center; }
        .status-alert { padding: 12px 16px; border-radius: 10px; margin-bottom: 18px; font-weight: 600; display: block; }
        .alert-success { background: rgba(16, 185, 129, 0.2); border: 1px solid #10b981; color: #a7f3d0; }
        .alert-error   { background: rgba(244, 63, 94, 0.2); border: 1px solid #f43f5e; color: #fecdd3; }
        .alert-info    { background: rgba(56, 189, 248, 0.2); border: 1px solid #38bdf8; color: #bae6fd; }
        .revision-banner {
            background: rgba(244, 63, 94, 0.18); border: 1px solid rgba(244, 63, 94, 0.55);
            border-radius: 10px; padding: 10px 14px; margin-bottom: 16px;
            display: flex; justify-content: space-between; align-items: center; color: #fecdd3; font-size: 0.88rem;
        }
        .progress-table { width: 100%; border-collapse: collapse; }
        .progress-table th { text-align: left; padding: 12px; background: rgba(15, 23, 42, 0.7); color: var(--text-muted); font-size: 0.84rem; }
        .progress-table td { padding: 12px; border-bottom: 1px solid rgba(255, 255, 255, 0.08); font-size: 0.9rem; }
        .stage-badge { display: inline-block; padding: 4px 10px; border-radius: 999px; font-size: 0.78rem; font-weight: 700; }
        .stage-reviewing { background: rgba(56, 189, 248, 0.18); border: 1px solid #38bdf8; color: #7dd3fc; }
        .stage-completed { background: rgba(16, 185, 129, 0.2); border: 1px solid #10b981; color: #6ee7b7; }
        .stage-returned  { background: rgba(244, 63, 94, 0.22); border: 1px solid #f43f5e; color: #fda4af; }
        .stamp-steps { display: flex; gap: 6px; margin-top: 6px; flex-wrap: wrap; }
        .step-dot { font-size: 0.72rem; padding: 2px 8px; border-radius: 6px; background: rgba(255, 255, 255, 0.07); color: #64748b; }
        .step-dot.stamped { background: rgba(220, 38, 38, 0.25); border: 1px solid rgba(248, 113, 113, 0.65); color: #fca5a5; font-weight: 700; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <header class="top-navbar">
            <div class="brand-title">🛡️ 智慧卡申請與四級職官章簽核系統</div>
            <div style="display:flex; align-items:center; gap:14px;">
                <span class="role-pill">申請人專區 (RoleLevel = 0)</span>
                <asp:Label ID="lblNavUserInfo" runat="server"></asp:Label>
                <asp:LinkButton ID="btnLogout" runat="server" CssClass="btn-gradient btn-outline btn-small" OnClick="btnLogout_Click">切換帳號 / 登出</asp:LinkButton>
            </div>
        </header>

        <div class="main-container">
            <asp:Panel ID="pnlMessage" runat="server" Visible="false">
                <asp:Label ID="lblMessage" runat="server"></asp:Label>
            </asp:Panel>

            <div class="workspace-grid">
                <div class="glass-card">
                    <div class="card-header">
                        <h2 class="card-title">📝 智慧卡申請表單填寫</h2>
                        <asp:Label ID="lblFormModeBadge" runat="server" CssClass="role-pill" Text="新案申請"></asp:Label>
                    </div>

                    <asp:Panel ID="pnlRevisionBanner" runat="server" CssClass="revision-banner" Visible="false">
                        <div>
                            <strong>⚠️ 退回重填修改模式：</strong>單號 <strong>#<asp:Literal ID="litRevisionAppId" runat="server"></asp:Literal></strong>，送出後將清除舊章並由第一級重新審核。
                        </div>
                        <asp:LinkButton ID="btnCancelRevision" runat="server" CssClass="btn-gradient btn-outline btn-small" OnClick="btnCancelRevision_Click">取消重填</asp:LinkButton>
                    </asp:Panel>

                    <asp:HiddenField ID="hfEditingAppId" runat="server" Value="0" />
                    <asp:HiddenField ID="hfSavedPhotoPath" runat="server" Value="" />
                    <asp:HiddenField ID="hfPreviewPdfPath" runat="server" Value="" />

                    <div class="applicant-info-grid">
                        <div class="info-box"><span class="info-label">申請人工號</span><asp:Label ID="lblEmpNo" runat="server" CssClass="info-value"></asp:Label></div>
                        <div class="info-box"><span class="info-label">申請人姓名</span><asp:Label ID="lblFullName" runat="server" CssClass="info-value"></asp:Label></div>
                        <div class="info-box"><span class="info-label">所屬部門 / 職稱</span><asp:Label ID="lblDeptJob" runat="server" CssClass="info-value"></asp:Label></div>
                    </div>

                    <div class="form-group">
                        <label class="form-label">1. 選擇申請卡片類別 *</label>
                        <asp:DropDownList ID="ddlCardCategory" runat="server" CssClass="glass-select">
                            <asp:ListItem Value="" Text="-- 請選擇智慧卡類別 --"></asp:ListItem>
                            <asp:ListItem Value="一般通行卡" Text="一般通行卡 (常規門禁與差勤刷卡)"></asp:ListItem>
                            <asp:ListItem Value="機房特權卡" Text="機房特權卡 (含核心主機房 24H 授權)"></asp:ListItem>
                            <asp:ListItem Value="臨時訪客卡" Text="臨時訪客卡 (外包駐點與短期專案人員)"></asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="form-group">
                        <label class="form-label">2. 申請事由 (可複選) *</label>
                        <asp:CheckBoxList ID="cblApplyReasons" runat="server" RepeatColumns="3" RepeatDirection="Horizontal" CssClass="reason-checkbox-table">
                            <asp:ListItem Value="新進人員">新進人員</asp:ListItem>
                            <asp:ListItem Value="卡片遺失">卡片遺失</asp:ListItem>
                            <asp:ListItem Value="卡片毀損">卡片毀損</asp:ListItem>
                            <asp:ListItem Value="職務調動">職務調動</asp:ListItem>
                            <asp:ListItem Value="權限變更">權限變更</asp:ListItem>
                            <asp:ListItem Value="屆期換發">屆期換發</asp:ListItem>
                        </asp:CheckBoxList>
                    </div>

                    <div class="form-group">
                        <label class="form-label">3. 上傳大頭照 / 證件照 (.jpg, .png；若未選擇將自動產生範例證件照)</label>
                        <div class="photo-upload-row">
                            <div class="photo-thumb-box">
                                <asp:Image ID="imgPhotoPreview" runat="server" Visible="false" />
                                <asp:Label ID="lblNoPhoto" runat="server" Text="尚無照片" Style="font-size:0.75rem; color:#64748b;"></asp:Label>
                            </div>
                            <div style="flex: 1;">
                                <asp:FileUpload ID="fuPhoto" runat="server" CssClass="glass-file" accept=".jpg,.jpeg,.png" />
                            </div>
                        </div>
                    </div>

                    <div class="btn-action-row">
                        <asp:Button ID="btnGeneratePreview" runat="server" Text="🔍 步驟一：生成 PDF 預覽" CssClass="btn-gradient btn-preview" OnClick="btnGeneratePreview_Click" />
                        <asp:Button ID="btnConfirmSubmit" runat="server" Text="🚀 步驟二：確認無誤送出審核" CssClass="btn-gradient btn-submit" Enabled="false" OnClick="btnConfirmSubmit_Click" />
                    </div>
                </div>

                <div class="glass-card">
                    <div class="card-header">
                        <h2 class="card-title">📄 A4 正式 PDF 申請單預覽區</h2>
                        <asp:HyperLink ID="hlDownloadPdf" runat="server" CssClass="btn-gradient btn-outline btn-small" Target="_blank" Visible="false">⬇️ 新分頁開啟 / 下載 PDF</asp:HyperLink>
                    </div>
                    <div class="pdf-preview-container">
                        <asp:Panel ID="pnlEmptyPreview" runat="server" CssClass="empty-preview-box">
                            <div style="font-size: 2.6rem; margin-bottom: 10px;">📑</div>
                            <div>請先於左側填寫表單並點擊<strong>「步驟一：生成 PDF 預覽」</strong>，<br/>或點擊下方進度表的<strong>「調閱最新職官章 PDF」</strong>。</div>
                        </asp:Panel>
                        <iframe id="iframePdfPreview" runat="server" class="pdf-iframe" visible="false"></iframe>
                    </div>
                </div>
            </div>

            <div class="glass-card">
                <div class="card-header">
                    <h2 class="card-title">📊 我的申請進度與四級職官章簽核追蹤</h2>
                    <asp:Button ID="btnRefreshList" runat="server" Text="🔄 重新整理進度" CssClass="btn-gradient btn-outline btn-small" OnClick="btnRefreshList_Click" />
                </div>

                <asp:GridView ID="gvApplications" runat="server" AutoGenerateColumns="False" CssClass="progress-table" GridLines="None" EmptyDataText="目前尚無任何智慧卡申請紀錄。" OnRowCommand="gvApplications_RowCommand">
                    <Columns>
                        <asp:TemplateField HeaderText="單號 / 更新時間">
                            <ItemTemplate>
                                <div style="font-weight: 700; color: #38bdf8;">#<%# Eval("AppID") %></div>
                                <div style="font-size: 0.78rem; color: #94a3b8;"><%# Eval("UpdatedAt", "{0:yyyy-MM-dd HH:mm}") %></div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="CardCategory" HeaderText="卡片類別" />
                        <asp:BoundField DataField="ApplyReasons" HeaderText="申請事由" />
                        <asp:TemplateField HeaderText="目前簽核層級與四級職官章狀態">
                            <ItemTemplate>
                                <div><%# RenderStageBadge(Convert.ToInt32(Eval("CurrentStage")), Eval("IssuedCardNo")) %></div>
                                <div class="stamp-steps">
                                    <span class='step-dot <%# Eval("Level1StampTime") != DBNull.Value ? "stamped" : "" %>'>一級：直屬主管</span>
                                    <span class='step-dot <%# Eval("Level2StampTime") != DBNull.Value ? "stamped" : "" %>'>二級：部門主管</span>
                                    <span class='step-dot <%# Eval("Level3StampTime") != DBNull.Value ? "stamped" : "" %>'>三級：資安管制</span>
                                    <span class='step-dot <%# Eval("Level4StampTime") != DBNull.Value ? "stamped" : "" %>'>四級：製卡中心</span>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="操作">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnViewStampedPdf" runat="server" CommandName="ViewPdf" CommandArgument='<%# Eval("AppID") %>' CssClass="btn-gradient btn-outline btn-small">👁️ 調閱最新職官章 PDF</asp:LinkButton>
                                <asp:LinkButton ID="btnLoadRevision" runat="server" CommandName="LoadRevision" CommandArgument='<%# Eval("AppID") %>' CssClass="btn-gradient btn-warn btn-small" Visible='<%# Convert.ToInt32(Eval("CurrentStage")) == -1 %>'>✏️ 退回重填</asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </form>
</body>
</html>
