<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ApplyCard.aspx.cs" Inherits="SmartCardSystem.ApplyCard" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>智慧卡線上申請與進度查詢</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" />
</head>
<body class="bg-light">
    <form id="form1" runat="server" class="container mt-4 mb-5">
        <!-- 頂部導覽列 -->
        <div class="d-flex justify-content-between align-items-center bg-primary text-white p-3 rounded mb-4 shadow-sm">
            <h4 class="mb-0">智慧卡申請端系統</h4>
            <div>
                <asp:Label ID="lblUserInfo" runat="server" CssClass="me-3 fw-bold"></asp:Label>
                <asp:Button ID="btnLogout" runat="server" Text="登出" CssClass="btn btn-sm btn-light" OnClick="btnLogout_Click" CausesValidation="false" />
            </div>
        </div>

        <div class="row">
            <!-- 左側：申請/重填表單 -->
            <div class="col-md-5">
                <div class="card shadow-sm mb-4">
                    <div class="card-header bg-secondary text-white">
                        <h5 class="mb-0"><asp:Label ID="lblFormTitle" runat="server" Text="新增智慧卡申請單"></asp:Label></h5>
                    </div>
                    <div class="card-body">
                        <!-- 隱藏欄位：紀錄如果是被退回重填的單號 AppID -->
                        <asp:HiddenField ID="hfEditingAppID" runat="server" Value="" />
                        <asp:HiddenField ID="hfExistingPhoto" runat="server" Value="" />

                        <div class="mb-3">
                            <label class="form-label">申請人編號</label>
                            <asp:TextBox ID="txtApplicantID" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">申請人姓名</label>
                            <asp:TextBox ID="txtApplicantName" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">所屬單位/部門</label>
                            <asp:TextBox ID="txtDepartment" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-bold">申請卡片類別</label>
                            <asp:DropDownList ID="ddlCardType" runat="server" CssClass="form-select">
                                <asp:ListItem Value="一般通行卡">一般通行卡</asp:ListItem>
                                <asp:ListItem Value="機房特權卡">機房特權卡</asp:ListItem>
                                <asp:ListItem Value="臨時訪客卡">臨時訪客卡</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-bold">上傳大頭照 / 證件照 (.jpg, .png)</label>
                            <asp:FileUpload ID="fuPhoto" runat="server" CssClass="form-control" />
                            <div class="form-text">請上傳清晰正面證件照以供製卡中心印製實體卡。</div>
                            <asp:Image ID="imgPreview" runat="server" CssClass="img-thumbnail mt-2" Width="100px" Visible="false" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-bold">申請事由與需求說明</label>
                            <asp:TextBox ID="txtApplyReason" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="d-flex align-items-center">
                            <asp:Button ID="btnSubmit" runat="server" Text="送出申請 (送交第一級審核)" CssClass="btn btn-success me-2" OnClick="btnSubmit_Click" />
                            <asp:Button ID="btnCancelEdit" runat="server" Text="取消重填" CssClass="btn btn-outline-secondary" OnClick="btnCancelEdit_Click" Visible="false" />
                        </div>
                        <asp:Label ID="lblMessage" runat="server" CssClass="d-block mt-3 fw-bold"></asp:Label>
                    </div>
                </div>
            </div>

            <!-- 右側：我的申請紀錄與四級審核進度 (含退回重填按鈕) -->
            <div class="col-md-7">
                <div class="card shadow-sm">
                    <div class="card-header bg-dark text-white">
                        <h5 class="mb-0">我的申請單與四級簽核進度</h5>
                    </div>
                    <div class="card-body">
                        <asp:GridView ID="gvMyApps" runat="server" CssClass="table table-bordered table-hover align-middle" AutoGenerateColumns="False" DataKeyNames="AppID" OnRowCommand="gvMyApps_RowCommand" EmptyDataText="您目前尚無任何申請紀錄。">
                            <Columns>
                                <asp:BoundField DataField="AppID" HeaderText="單號" />
                                <asp:TemplateField HeaderText="證件照">
                                    <ItemTemplate>
                                        <asp:Image ID="imgCard" runat="server" ImageUrl='<%# Eval("PhotoPath") %>' Width="55px" CssClass="img-thumbnail" Visible='<%# !string.IsNullOrEmpty(Eval("PhotoPath").ToString()) %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="CardType" HeaderText="卡片類別" />
                                <asp:TemplateField HeaderText="當前簽核狀態">
                                    <ItemTemplate>
                                        <%# GetStatusBadge(Convert.ToInt32(Eval("CurrentStatus")), Eval("RejectReason").ToString(), Eval("CardNumber").ToString()) %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="LastModified" HeaderText="最後更新" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                                <asp:TemplateField HeaderText="操作">
                                    <ItemTemplate>
                                        <!-- 當狀態為 -1 (退回申請人) 時，顯示「修改重送」按鈕 -->
                                        <asp:Button ID="btnResubmit" runat="server" CommandName="EditRejected" CommandArgument='<%# Eval("AppID") %>' Text="修改重填" CssClass="btn btn-sm btn-warning" Visible='<%# Convert.ToInt32(Eval("CurrentStatus")) == -1 %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
