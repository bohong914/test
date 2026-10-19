using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI.WebControls;

public partial class ApplicantDashboard : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        DbHelper.EnsureDatabaseInitialized();
        if (Session["UserID"] == null)
        {
            Response.Redirect("Login.aspx");
            return;
        }
        if (Convert.ToInt32(Session["RoleLevel"]) > 0)
        {
            Response.Redirect("ReviewerWorkbench.aspx");
            return;
        }

        if (!IsPostBack)
        {
            lblEmpNo.Text = Convert.ToString(Session["EmployeeNo"]);
            lblFullName.Text = Convert.ToString(Session["FullName"]);
            lblDeptJob.Text = string.Format("{0} / {1}", Session["Department"], Session["JobTitle"]);
            lblNavUserInfo.Text = string.Format("{0} ({1})", Session["FullName"], Session["EmployeeNo"]);
            BindApplicationProgressGrid();
        }
    }

    protected void btnGeneratePreview_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(ddlCardCategory.SelectedValue))
        {
            ShowMessage("請選擇「申請卡片類別」！", "error");
            return;
        }

        List<string> selectedReasons = new List<string>();
        foreach (ListItem item in cblApplyReasons.Items)
        {
            if (item.Selected) selectedReasons.Add(item.Value);
        }
        if (selectedReasons.Count == 0)
        {
            ShowMessage("請至少勾選一項「申請事由」！", "error");
            return;
        }

        string photoVirtualPath = hfSavedPhotoPath.Value;
        string photoFolder = Server.MapPath("~/Uploads/Photos/");
        if (!Directory.Exists(photoFolder)) Directory.CreateDirectory(photoFolder);

        if (fuPhoto.HasFile)
        {
            string ext = Path.GetExtension(fuPhoto.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
            {
                ShowMessage("證件照僅支援 .jpg 或 .png 格式！", "error");
                return;
            }
            string fileName = string.Format("PHOTO_{0}_{1:yyyyMMddHHmmss}{2}", Session["EmployeeNo"], DateTime.Now, ext);
            fuPhoto.SaveAs(Path.Combine(photoFolder, fileName));
            photoVirtualPath = "~/Uploads/Photos/" + fileName;
            hfSavedPhotoPath.Value = photoVirtualPath;
        }
        else if (string.IsNullOrEmpty(photoVirtualPath))
        {
            // 自動生成範例證件照方便快速測試
            string defaultFile = string.Format("DEFAULT_{0}.jpg", Session["EmployeeNo"]);
            StampService.EnsureDefaultAvatarExists(Path.Combine(photoFolder, defaultFile), lblFullName.Text);
            photoVirtualPath = "~/Uploads/Photos/" + defaultFile;
            hfSavedPhotoPath.Value = photoVirtualPath;
        }

        imgPhotoPreview.ImageUrl = ResolveUrl(photoVirtualPath);
        imgPhotoPreview.Visible = true;
        lblNoPhoto.Visible = false;

        string pdfFolder = Server.MapPath("~/Uploads/GeneratedPdfs/");
        if (!Directory.Exists(pdfFolder)) Directory.CreateDirectory(pdfFolder);

        string pdfFileName = string.Format("APP_PREVIEW_{0}_{1:yyyyMMddHHmmss}.pdf", Session["EmployeeNo"], DateTime.Now);
        string pdfPhysicalPath = Path.Combine(pdfFolder, pdfFileName);
        string pdfVirtualPath = "~/Uploads/GeneratedPdfs/" + pdfFileName;

        PdfService.GenerateApplicationPdf(
            outputPdfPath: pdfPhysicalPath,
            empNo: lblEmpNo.Text,
            fullName: lblFullName.Text,
            department: Convert.ToString(Session["Department"]),
            jobTitle: Convert.ToString(Session["JobTitle"]),
            cardCategory: ddlCardCategory.SelectedValue,
            applyReasons: string.Join("、", selectedReasons),
            photoPhysicalPath: Server.MapPath(photoVirtualPath),
            stampPaths: null,
            issuedCardNo: null,
            appId: Convert.ToInt32(hfEditingAppId.Value)
        );

        hfPreviewPdfPath.Value = pdfVirtualPath;
        DisplayPdfInPreviewPane(pdfVirtualPath);
        btnConfirmSubmit.Enabled = true;
        ShowMessage("✅ A4 PDF 申請單預覽已生成！請於右側確認無誤後，點擊「步驟二：確認無誤送出審核」。", "info");
    }

    protected void btnConfirmSubmit_Click(object sender, EventArgs e)
    {
        List<string> selectedReasons = new List<string>();
        foreach (ListItem item in cblApplyReasons.Items)
        {
            if (item.Selected) selectedReasons.Add(item.Value);
        }
        string reasonsJoined = string.Join("、", selectedReasons);
        int userId = Convert.ToInt32(Session["UserID"]);
        int editingAppId = Convert.ToInt32(hfEditingAppId.Value);

        using (SqlConnection conn = new SqlConnection(DbHelper.ConnectionString))
        {
            conn.Open();
            if (editingAppId == 0)
            {
                string sql = @"
                    INSERT INTO dbo.CardApplications (ApplicantID, CardCategory, ApplyReasons, PhotoPath, PdfPath, CurrentStage)
                    VALUES (@ApplicantID, @CardCategory, @ApplyReasons, @PhotoPath, @PdfPath, 1);";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ApplicantID", userId);
                    cmd.Parameters.AddWithValue("@CardCategory", ddlCardCategory.SelectedValue);
                    cmd.Parameters.AddWithValue("@ApplyReasons", reasonsJoined);
                    cmd.Parameters.AddWithValue("@PhotoPath", hfSavedPhotoPath.Value);
                    cmd.Parameters.AddWithValue("@PdfPath", hfPreviewPdfPath.Value);
                    cmd.ExecuteNonQuery();
                }
                ShowMessage("🚀 申請單已正式送出！目前已進入「第一級：直屬主管初審」。", "success");
            }
            else
            {
                string sql = @"
                    UPDATE dbo.CardApplications
                    SET CardCategory = @CardCategory, ApplyReasons = @ApplyReasons,
                        PhotoPath = @PhotoPath, PdfPath = @PdfPath,
                        CurrentStage = 1, IssuedCardNo = NULL,
                        Level1StampPath = NULL, Level2StampPath = NULL, Level3StampPath = NULL, Level4StampPath = NULL,
                        Level1StampTime = NULL, Level2StampTime = NULL, Level3StampTime = NULL, Level4StampTime = NULL,
                        UpdatedAt = GETDATE()
                    WHERE AppID = @AppID AND ApplicantID = @ApplicantID;";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CardCategory", ddlCardCategory.SelectedValue);
                    cmd.Parameters.AddWithValue("@ApplyReasons", reasonsJoined);
                    cmd.Parameters.AddWithValue("@PhotoPath", hfSavedPhotoPath.Value);
                    cmd.Parameters.AddWithValue("@PdfPath", hfPreviewPdfPath.Value);
                    cmd.Parameters.AddWithValue("@AppID", editingAppId);
                    cmd.Parameters.AddWithValue("@ApplicantID", userId);
                    cmd.ExecuteNonQuery();
                }
                ShowMessage(string.Format("🚀 退回單號 #{0} 已修改並重新送出，由「第一級：直屬主管初審」重新開始！", editingAppId), "success");
            }
        }

        ResetForm();
        BindApplicationProgressGrid();
    }

    private void BindApplicationProgressGrid()
    {
        using (SqlConnection conn = new SqlConnection(DbHelper.ConnectionString))
        using (SqlDataAdapter da = new SqlDataAdapter(
            "SELECT * FROM dbo.CardApplications WHERE ApplicantID = @UID ORDER BY UpdatedAt DESC, AppID DESC", conn))
        {
            da.SelectCommand.Parameters.AddWithValue("@UID", Convert.ToInt32(Session["UserID"]));
            DataTable dt = new DataTable();
            da.Fill(dt);
            gvApplications.DataSource = dt;
            gvApplications.DataBind();
        }
    }

    protected void gvApplications_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int appId = Convert.ToInt32(e.CommandArgument);
        using (SqlConnection conn = new SqlConnection(DbHelper.ConnectionString))
        using (SqlCommand cmd = new SqlCommand("SELECT * FROM dbo.CardApplications WHERE AppID = @ID", conn))
        {
            cmd.Parameters.AddWithValue("@ID", appId);
            conn.Open();
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (!dr.Read()) return;
                string pdfPath = Convert.ToString(dr["PdfPath"]);
                if (e.CommandName == "ViewPdf")
                {
                    DisplayPdfInPreviewPane(pdfPath);
                    ShowMessage("正在右側預覽單號 #" + appId + " 之最新 PDF 表單。", "info");
                }
                else if (e.CommandName == "LoadRevision")
                {
                    hfEditingAppId.Value = appId.ToString();
                    litRevisionAppId.Text = appId.ToString();
                    pnlRevisionBanner.Visible = true;
                    lblFormModeBadge.Text = "退回重填 (#" + appId + ")";
                    ddlCardCategory.SelectedValue = Convert.ToString(dr["CardCategory"]);

                    cblApplyReasons.ClearSelection();
                    string[] reasons = Convert.ToString(dr["ApplyReasons"]).Split(new char[] { ',', '、' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string r in reasons)
                    {
                        ListItem li = cblApplyReasons.Items.FindByValue(r.Trim());
                        if (li != null) li.Selected = true;
                    }

                    hfSavedPhotoPath.Value = Convert.ToString(dr["PhotoPath"]);
                    imgPhotoPreview.ImageUrl = ResolveUrl(hfSavedPhotoPath.Value);
                    imgPhotoPreview.Visible = true;
                    lblNoPhoto.Visible = false;
                    btnConfirmSubmit.Enabled = false;
                    DisplayPdfInPreviewPane(pdfPath);
                    ShowMessage("已載入單號 #" + appId + " 原表單資料，修改後請重新點擊「步驟一：生成 PDF 預覽」。", "info");
                }
            }
        }
    }

    protected void btnCancelRevision_Click(object sender, EventArgs e) { ResetForm(); }
    protected void btnRefreshList_Click(object sender, EventArgs e) { BindApplicationProgressGrid(); }
    protected void btnLogout_Click(object sender, EventArgs e) { Session.Clear(); Response.Redirect("Login.aspx"); }

    private void DisplayPdfInPreviewPane(string virtualPdfPath)
    {
        string url = ResolveUrl(virtualPdfPath) + "?t=" + DateTime.Now.Ticks;
        pnlEmptyPreview.Visible = false;
        iframePdfPreview.Visible = true;
        iframePdfPreview.Attributes["src"] = url;
        hlDownloadPdf.NavigateUrl = url;
        hlDownloadPdf.Visible = true;
    }

    private void ResetForm()
    {
        hfEditingAppId.Value = "0";
        hfSavedPhotoPath.Value = "";
        hfPreviewPdfPath.Value = "";
        pnlRevisionBanner.Visible = false;
        lblFormModeBadge.Text = "新案申請";
        ddlCardCategory.SelectedIndex = 0;
        cblApplyReasons.ClearSelection();
        btnConfirmSubmit.Enabled = false;
    }

    protected string RenderStageBadge(int stage, object cardNo)
    {
        switch (stage)
        {
            case 1: return "<span class='stage-badge stage-reviewing'>⏳ 第一級：待直屬主管初審</span>";
            case 2: return "<span class='stage-badge stage-reviewing'>⏳ 第二級：待部門主管複審</span>";
            case 3: return "<span class='stage-badge stage-reviewing'>🛡️ 第三級：待資安管制官審查</span>";
            case 4: return "<span class='stage-badge stage-reviewing'>🖨️ 第四級：待總務製卡中心核定</span>";
            case 5: return "<span class='stage-badge stage-completed'>✅ 已核發卡號：" + Server.HtmlEncode(Convert.ToString(cardNo)) + "</span>";
            case -1: return "<span class='stage-badge stage-returned'>❌ 退回重填 (已清除舊職官章)</span>";
            default: return "";
        }
    }

    private void ShowMessage(string msg, string type)
    {
        pnlMessage.Visible = true;
        lblMessage.CssClass = "status-alert alert-" + type;
        lblMessage.Text = msg;
    }
}
