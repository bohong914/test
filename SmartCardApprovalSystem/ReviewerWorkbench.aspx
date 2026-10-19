<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ReviewerWorkbench.aspx.cs" Inherits="ReviewerWorkbench" MaintainScrollPositionOnPostBack="true" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <title>後台四級職官章審核工作台 - 智慧卡簽核系統</title>
    <style>
        :root {
            --glass-bg: rgba(255, 255, 255, 0.08);
            --glass-border: rgba(255, 255, 255, 0.18);
            --text-main: #f8fafc;
            --text-muted: #94a3b8;
        }
        * { box-sizing: border-box; font-family: "Microsoft JhengHei", sans-serif; }
        body {
            margin: 0; min-height: 100vh;
            background: radial-gradient(circle at 80% 15%, #312e81 0%, transparent 40%),
                        radial-gradient(circle at 15% 85%, #1e3a8a 0%, transparent 45%),
                        linear-gradient(135deg, #090d16 0%, #0f172a 100%);
            color: var(--text-main); padding-bottom: 50px;
        }
        .top-navbar {
            position: sticky; top: 0; z-index: 100;
            display: flex; justify-content: space-between; align-items: center;
            padding: 14px 32px; background: rgba(15, 23, 42, 0.85);
            backdrop-filter: blur(18px); border-bottom: 1px solid var(--glass-border);
        }
        .brand-title {
            font-size: 1.2rem; font-weight: 700;
            background: linear-gradient(90deg, #f43f5e, #fb923c, #38bdf8);
            -webkit-background-clip: text; -webkit-text-fill-color: transparent;
        }
        .role-pill {
            padding: 5px 14px; border-radius: 999px; font-size: 0.82rem; font-weight: 700;
            background: rgba(244, 63, 94, 0.2); border: 1px solid rgba(244, 63, 94, 0.5); color: #fda4af;
        }
        .main-container { max-width: 1420px; margin: 26px auto; padding: 0 24px; }
        .workbench-grid { display: grid; grid-template-columns: 0.95fr 1.05fr; gap: 24px; margin-bottom: 26px; }
        @media (max-width: 1080px) { .workbench-grid { grid-template-columns: 1fr; } }
        .glass-card {
            background: var(--glass-bg); backdrop-filter: blur(16px);
            border: 1px solid var(--glass-border); border-radius: 18px; padding: 24px;
            box-shadow: 0 10px 35px rgba(0, 0, 0, 0.38);
        }
        .card-header {
            display: flex; justify-content: space-between; align-items: center;
            border-bottom: 1px solid rgba(255, 255, 255, 0.12); padding-bottom: 12px; margin-bottom: 18px;
        }
        .card-title { margin: 0; font-size: 1.14rem; font-weight: 700; }
        .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 14px; margin-bottom: 14px; }
        .form-label { display: block; font-size: 0.86rem; color: #cbd5e1; margin-bottom: 6px; font-weight: 600; }
        .glass-input {
            width: 100%; padding: 10px 12px; border-radius: 10px;
            background: rgba(15, 23, 42, 0.7); border: 1px solid rgba(255, 255, 255, 0.2); color: #fff;
        }
        /* 職官章蓋下動畫 (@keyframes stampDrop) */
        @keyframes stampDrop {
            0%   { transform: scale(2.8) rotate(-14deg); opacity: 0; filter: blur(6px); }
            70%  { transform: scale(0.92) rotate(-3deg); opacity: 1; filter: blur(0px); }
            85%  { transform: scale(1.05) rotate(-4deg); }
            100% { transform: scale(1) rotate(-3.5deg); opacity: 0.95; }
        }
        .stamp-preview-stage {
            background: #f8fafc; border-radius: 12px; padding: 16px;
            display: flex; align-items: center; justify-content: space-around;
            border: 2px dashed #cbd5e1; margin-top: 12px; min-height: 175px;
        }
        .stamp-animate {
            width: 145px; height: 145px; object-fit: contain;
            animation: stampDrop 0.42s cubic-bezier(0.22, 1, 0.36, 1) forwards;
        }
        .btn-gradient {
            padding: 11px 18px; border: none; border-radius: 10px; font-size: 0.9rem; font-weight: 700;
            color: #fff; cursor: pointer; text-decoration: none; display: inline-block;
        }
        .btn-vermilion { background: linear-gradient(135deg, #ef4444 0%, #b91c1c 100%); box-shadow: 0 6px 18px rgba(239, 68, 68, 0.35); }
        .btn-approve { background: linear-gradient(135deg, #10b981 0%, #059669 100%); }
        .btn-outline { background: rgba(255, 255, 255, 0.08); border: 1px solid rgba(255, 255, 255, 0.25); color: #e2e8f0; }
        .btn-small { padding: 6px 12px; font-size: 0.8rem; border-radius: 8px; }
        .queue-table { width: 100%; border-collapse: collapse; margin-top: 10px; }
        .queue-table th { text-align: left; padding: 12px; background: rgba(15, 23, 42, 0.75); color: var(--text-muted); font-size: 0.84rem; }
        .queue-table td { padding: 14px 12px; border-bottom: 1px solid rgba(255, 255, 255, 0.08); font-size: 0.92rem; }
        .decision-box {
            background: rgba(15, 23, 42, 0.65); border: 1px solid rgba(56, 189, 248, 0.4);
            border-radius: 14px; padding: 18px; margin-top: 16px;
        }
        .status-alert { padding: 12px 16px; border-radius: 10px; margin-bottom: 18px; font-weight: 600; display: block; }
        .alert-success { background: rgba(16, 185, 129, 0.2); border: 1px solid #10b981; color: #a7f3d0; }
        .alert-error   { background: rgba(244, 63, 94, 0.2); border: 1px solid #f43f5e; color: #fecdd3; }
        .alert-info    { background: rgba(56, 189, 248, 0.2); border: 1px solid #38bdf8; color: #bae6fd; }
        .pdf-preview-container { width: 100%; height: 620px; border-radius: 12px; overflow: hidden; border: 1px solid rgba(255, 255, 255, 0.16); background: rgba(9, 13, 22, 0.7); }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <header class="top-navbar">
            <div class="brand-title">🏛️ 後台四級職官章簽核工作台</div>
            <div style="display:flex; align-items:center; gap:14px;">
                <asp:Label ID="lblRoleLevelBadge" runat="server" CssClass="role-pill"></asp:Label>
                <asp:Label ID="lblReviewerInfo" runat="server"></asp:Label>
                <asp:LinkButton ID="btnLogout" runat="server" CssClass="btn-gradient btn-outline btn-small" OnClick="btnLogout_Click">切換審核官 / 登出</asp:LinkButton>
            </div>
        </header>

        <div class="main-container">
            <asp:Panel ID="pnlMessage" runat="server" Visible="false">
                <asp:Label ID="lblMessage" runat="server"></asp:Label>
            </asp:Panel>

            <div class="workbench-grid">
                <!-- 左欄：1. 個人職官章設定與落章動畫 + 2. 本級待審清單與極簡簽核 -->
                <div>
                    <!-- 個人職官章設定卡片 -->
                    <div class="glass-card" style="margin-bottom: 24px;">
                        <div class="card-header">
                            <h2 class="card-title">💮 個人職官章設定與即時落章動畫預覽</h2>
                        </div>

                        <div class="form-row">
                            <div>
                                <label class="form-label">職官章頂部頭銜</label>
                                <asp:TextBox ID="txtStampTopTitle" runat="server" CssClass="glass-input"></asp:TextBox>
                            </div>
                            <div>
                                <label class="form-label">職官章中央印文</label>
                                <asp:TextBox ID="txtStampCenterText" runat="server" CssClass="glass-input"></asp:TextBox>
                            </div>
                        </div>

                        <div class="form-row">
                            <div>
                                <label class="form-label">印鑑樣式選擇</label>
                                <asp:DropDownList ID="ddlStampShape" runat="server" CssClass="glass-input">
                                    <asp:ListItem Value="Square">傳統方章 (朱紅雙框篆刻)</asp:ListItem>
                                    <asp:ListItem Value="Round">圓形職官章 (朱紅同心圓章)</asp:ListItem>
                                    <asp:ListItem Value="Custom">自訂上傳透明印鑑圖檔 (.png)</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div>
                                <label class="form-label">上傳自訂透明印鑑 (選填 .png)</label>
                                <asp:FileUpload ID="fuCustomStamp" runat="server" CssClass="glass-input" accept=".png" />
                            </div>
                        </div>

                        <asp:Button ID="btnSavePreviewStamp" runat="server" Text="💥 儲存印鑑設定並觸發「落章動畫」預覽" CssClass="btn-gradient btn-vermilion" Style="width:100%;" OnClick="btnSavePreviewStamp_Click" />

                        <div class="stamp-preview-stage">
                            <div style="color: #334155; font-size: 0.88rem; max-width: 210px;">
                                <strong>即時落章預覽紙面</strong><br/>
                                審核通過時，系統將自動以此朱紅職官章與當日日期戳記壓印至 PDF 對應層級欄位。
                            </div>
                            <asp:Image ID="imgStampAnimatePreview" runat="server" CssClass="stamp-animate" />
                        </div>
                    </div>

                    <!-- 待審核清單與極簡化簽核作業 -->
                    <div class="glass-card">
                        <div class="card-header">
                            <h2 class="card-title">📋 本層級待審核申請單 (嚴格權限控管)</h2>
                            <asp:Label ID="lblLevel4Notice" runat="server" Visible="false" Style="font-size:0.8rem; color:#6ee7b7; font-weight:700;" Text="★ 製卡中心視角：僅顯示已蓋滿前三級職官章之完稿單"></asp:Label>
                        </div>

                        <asp:GridView ID="gvPendingQueue" runat="server" AutoGenerateColumns="False" CssClass="queue-table" GridLines="None" EmptyDataText="🎉 目前您的審核層級沒有待審核的申請單！" OnRowCommand="gvPendingQueue_RowCommand">
                            <Columns>
                                <asp:TemplateField HeaderText="單號">
                                    <ItemTemplate><strong style="color:#38bdf8;">#<%# Eval("AppID") %></strong></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="申請人 / 部門">
                                    <ItemTemplate>
                                        <div><%# Eval("FullName") %> (<%# Eval("EmployeeNo") %>)</div>
                                        <div style="font-size:0.78rem; color:#94a3b8;"><%# Eval("Department") %></div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="CardCategory" HeaderText="卡片類別" />
                                <asp:BoundField DataField="ApplyReasons" HeaderText="申請事由" />
                                <asp:TemplateField HeaderText="檢視與簽核">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnSelectApp" runat="server" CommandName="SelectApp" CommandArgument='<%# Eval("AppID") %>' CssClass="btn-gradient btn-outline btn-small">🔍 載入預覽與簽核</asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>

                        <!-- 極簡化審核作業區 (免填事由，僅勾選通過落章或退回重填) -->
                        <asp:Panel ID="pnlDecisionBox" runat="server" CssClass="decision-box" Visible="false">
                            <asp:HiddenField ID="hfSelectedAppId" runat="server" Value="0" />
                            <div style="font-weight:700; color:#38bdf8; margin-bottom:12px;">
                                正在簽核申請單號：#<asp:Literal ID="litSelectedAppId" runat="server"></asp:Literal>
                                （申請人：<asp:Literal ID="litSelectedApplicant" runat="server"></asp:Literal>）
                            </div>

                            <!-- 第四級：總務製卡中心專屬卡號輸入欄 -->
                            <asp:Panel ID="pnlLevel4CardIssue" runat="server" Visible="false" Style="margin-bottom:14px; padding:12px; background:rgba(16,185,129,0.15); border:1px solid #10b981; border-radius:10px;">
                                <label class="form-label" style="color:#a7f3d0;">💳 第四級（總務製卡中心）核定必填：配發實體卡號 *</label>
                                <asp:TextBox ID="txtIssuedCardNo" runat="server" CssClass="glass-input" placeholder="例如：SC-2026-880192"></asp:TextBox>
                            </asp:Panel>

                            <div style="margin-bottom: 14px;">
                                <label class="form-label">極簡化審核裁決 (免填寫任何意見或事由)：</label>
                                <asp:RadioButtonList ID="rblDecision" runat="server" RepeatDirection="Horizontal" Style="width:100%; font-weight:700;">
                                    <asp:ListItem Value="Approve" Selected="True">✅ 審核通過 (自動落章並流轉下一級)</asp:ListItem>
                                    <asp:ListItem Value="Reject">❌ 不通過 (直接退回申請人重填並清除舊章)</asp:ListItem>
                                </asp:RadioButtonList>
                            </div>

                            <asp:Button ID="btnExecuteDecision" runat="server" Text="🔏 執行審核裁決並更新 PDF 職官章" CssClass="btn-gradient btn-approve" Style="width:100%;" OnClick="btnExecuteDecision_Click" />
                        </asp:Panel>
                    </div>
                </div>

                <!-- 右欄：申請單 PDF 與已蓋職官章即時預覽 -->
                <div class="glass-card">
                    <div class="card-header">
                        <h2 class="card-title">📄 申請單 A4 PDF 與職官章落章即時預覽</h2>
                        <asp:HyperLink ID="hlOpenPdf" runat="server" CssClass="btn-gradient btn-outline btn-small" Target="_blank" Visible="false">⬇️ 新分頁開啟 PDF</asp:HyperLink>
                    </div>
                    <div class="pdf-preview-container">
                        <asp:Panel ID="pnlEmptyPdf" runat="server" Style="height:100%; display:flex; align-items:center; justify-content:center; color:#94a3b8; padding:30px; text-align:center;">
                            請從左側「本層級待審核申請單」點擊「載入預覽與簽核」，即可於此處檢視包含前三級已蓋職官章之 A4 PDF 申請單。
                        </asp:Panel>
                        <iframe id="iframeReviewerPdf" runat="server" style="width:100%; height:100%; border:none;" visible="false"></iframe>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
