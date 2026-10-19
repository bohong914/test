<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReviewPanel.aspx.cs" Inherits="SmartCardSystem.ReviewPanel" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>後台四級簽核系統</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" />
</head>
<body class="bg-light">
    <form id="form1" runat="server" class="container mt-4 mb-5">
        <div class="card shadow-sm mb-4">
            <div class="card-header bg-dark text-white d-flex justify-content-between align-items-center p-3">
                <h4 class="mb-0">智慧卡四層級審核後台</h4>
                <div>
                    <asp:Label ID="lblReviewerInfo" runat="server" CssClass="me-3 fw-bold text-warning"></asp:Label>
                    <asp:Button ID="btnLogout" runat="server" Text="登出切換帳號" CssClass="btn btn-sm btn-outline-light" OnClick="btnLogout_Click" />
                </div>
            </div>
            <div class="card-body">
                <!-- 顯示當前審核官層級的審核標準 -->
                <div class="alert alert-info border-start border-4 border-info">
                    <asp:Label ID="lblStandardInfo" runat="server" CssClass="fw-bold"></asp:Label>
                </div>

                <h5 class="mt-3 mb-2">待您簽核的申請單列表</h5>
                <asp:GridView ID="gvApplications" runat="server" CssClass="table table-bordered table-hover align-middle" AutoGenerateColumns="False" DataKeyNames="AppID" OnRowCommand="gvApplications_RowCommand" EmptyDataText="目前沒有等待您這個層級審核的申請單。">
                    <Columns>
                        <asp:BoundField DataField="AppID" HeaderText="單號" />
                        <asp:TemplateField HeaderText="證件照">
                            <ItemTemplate>
                                <asp:Image ID="imgApplicant" runat="server" ImageUrl='<%# Eval("PhotoPath") %>' Width="70px" CssClass="img-thumbnail" Visible='<%# !string.IsNullOrEmpty(Eval("PhotoPath").ToString()) %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="ApplicantName" HeaderText="申請人" />
                        <asp:BoundField DataField="Department" HeaderText="部門" />
                        <asp:BoundField DataField="CardType" HeaderText="卡片類別" />
                        <asp:BoundField DataField="ApplyReason" HeaderText="申請事由" />
                        <asp:BoundField DataField="ApplyDate" HeaderText="申請時間" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                        <asp:TemplateField HeaderText="簽核作業 (退回將直接退給申請人重填)">
                            <ItemTemplate>
                                <asp:TextBox ID="txtComment" runat="server" CssClass="form-control mb-1" placeholder="請輸入審核意見 (必填)"></asp:TextBox>
                                <asp:TextBox ID="txtCardNumber" runat="server" CssClass="form-control mb-1" placeholder="配發實體卡號 (第四級必填)" Visible='<%# Convert.ToInt32(Session["RoleLevel"]) == 4 %>'></asp:TextBox>
                                <div class="mt-1">
                                    <asp:Button ID="btnApprove" runat="server" CommandName="Approve" CommandArgument='<%# Container.DataItemIndex %>' Text="核准通過" CssClass="btn btn-sm btn-success me-2" />
                                    <asp:Button ID="btnReject" runat="server" CommandName="Reject" CommandArgument='<%# Container.DataItemIndex %>' Text="退回申請人重填" CssClass="btn btn-sm btn-danger" />
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>

                <asp:Label ID="lblStatusMsg" runat="server" CssClass="fw-bold d-block mt-2"></asp:Label>

                <hr class="my-4" />
                <h5 class="mb-2">歷史簽核紀錄 (ApprovalLogs)</h5>
                <asp:GridView ID="gvLogs" runat="server" CssClass="table table-sm table-striped table-bordered" AutoGenerateColumns="False" EmptyDataText="尚無簽核紀錄。">
                    <Columns>
                        <asp:BoundField DataField="LogID" HeaderText="紀錄編號" />
                        <asp:BoundField DataField="AppID" HeaderText="申請單號" />
                        <asp:BoundField DataField="ApprovalLevel" HeaderText="審核層級" />
                        <asp:BoundField DataField="ReviewerName" HeaderText="審核官" />
                        <asp:BoundField DataField="ActionResult" HeaderText="結果" />
                        <asp:BoundField DataField="Comment" HeaderText="審核意見" />
                        <asp:BoundField DataField="ReviewDate" HeaderText="簽核時間" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </form>
</body>
</html>
