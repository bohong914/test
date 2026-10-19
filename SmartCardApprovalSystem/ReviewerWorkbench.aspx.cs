using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI.WebControls;

public partial class ReviewerWorkbench : System.Web.UI.Page
{
    private int CurrentRoleLevel
    {
        get { return Convert.ToInt32(Session["RoleLevel"]); }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        DbHelper.EnsureDatabaseInitialized();
        if (Session["UserID"] == null)
        {
            Response.Redirect("Login.aspx");
            return;
        }
        if (CurrentRoleLevel <= 0)
        {
            Response.Redirect("ApplicantDashboard.aspx");
            return;
        }

        if (!IsPostBack)
        {
            BindReviewerHeaderAndStampSettings();
            BindPendingQueue();
        }
    }

    private void BindReviewerHeaderAndStampSettings()
    {
        string[] stageNames = { "", "第一級：直屬主管初審", "第二級：部門主管複審", "第三級：資安管制官安全審查", "第四級：總務製卡中心核定發卡" };
        lblRoleLevelBadge.Text = stageNames[CurrentRoleLevel] + " (RoleLevel = " + CurrentRoleLevel + ")";
        lblReviewerInfo.Text = string.Format("{0} / {1} ({2})", Session["Department"], Session["FullName"], Session["JobTitle"]);
        lblLevel4Notice.Visible = (CurrentRoleLevel == 4);

        using (SqlConnection conn = new SqlConnection(DbHelper.ConnectionString))
        using (SqlCommand cmd = new SqlCommand("SELECT * FROM dbo.Users WHERE UserID = @UID", conn))
        {
            cmd.Parameters.AddWithValue("@UID", Convert.ToInt32(Session["UserID"]));
            conn.Open();
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    txtStampTopTitle.Text = Convert.ToString(dr["StampTopTitle"]);
                    txtStampCenterText.Text = Convert.ToString(dr["StampCenterText"]);
                    string shape = Convert.ToString(dr["StampShape"]);
                    if (ddlStampShape.Items.FindByValue(shape) != null)
                        ddlStampShape.SelectedValue = shape;
                }
            }
        }

        RenderCurrentReviewerStampPreview();
    }

    protected void btnSavePreviewStamp_Click(object sender, EventArgs e)
    {
        string customVirtualPath = null;
        if (fuCustomStamp.HasFile)
        {
            string folder = Server.MapPath("~/Uploads/CustomStamps/");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string fileName = string.Format("CUSTOM_STAMP_{0}_{1:yyyyMMddHHmmss}.png", Session["EmployeeNo"], DateTime.Now);
            fuCustomStamp.SaveAs(Path.Combine(folder, fileName));
            customVirtualPath = "~/Uploads/CustomStamps/" + fileName;
            ddlStampShape.SelectedValue = "Custom";
        }

        using (SqlConnection conn = new SqlConnection(DbHelper.ConnectionString))
        {
            conn.Open();
            string sql = @"
                UPDATE dbo.Users
                SET StampShape = @Shape,
                    StampTopTitle = @TopTitle,
                    StampCenterText = @CenterText,
                    CustomStampPath = ISNULL(@CustomPath, CustomStampPath)
                WHERE UserID = @UID";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Shape", ddlStampShape.SelectedValue);
                cmd.Parameters.AddWithValue("@TopTitle", txtStampTopTitle.Text.Trim());
                cmd.Parameters.AddWithValue("@CenterText", txtStampCenterText.Text.Trim());
                cmd.Parameters.AddWithValue("@CustomPath", (object)customVirtualPath ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UID", Convert.ToInt32(Session["UserID"]));
                cmd.ExecuteNonQuery();
            }
        }

        RenderCurrentReviewerStampPreview();
        ShowMessage("💮 個人職官章設定已儲存！下方已觸發即時「落章動畫」預覽。", "info");
    }

    private string RenderCurrentReviewerStampPreview()
    {
        string customPath = null;
        using (SqlConnection conn = new SqlConnection(DbHelper.ConnectionString))
        using (SqlCommand cmd = new SqlCommand("SELECT CustomStampPath FROM dbo.Users WHERE UserID = @UID", conn))
        {
            cmd.Parameters.AddWithValue("@UID", Convert.ToInt32(Session["UserID"]));
            conn.Open();
            object val = cmd.ExecuteScalar();
            if (val != null && val != DBNull.Value) customPath = Convert.ToString(val);
        }

        string stampFolder = Server.MapPath("~/Uploads/CustomStamps/");
        if (!Directory.Exists(stampFolder)) Directory.CreateDirectory(stampFolder);

        string stampFileName = string.Format("ACTIVE_STAMP_LEVEL{0}_{1}.png", CurrentRoleLevel, Session["EmployeeNo"]);
        string stampPhysicalPath = Path.Combine(stampFolder, stampFileName);
        string stampVirtualPath = "~/Uploads/CustomStamps/" + stampFileName;

        StampService.GenerateStampPng(
            outputPngPath: stampPhysicalPath,
            shape: ddlStampShape.SelectedValue,
            topTitle: txtStampTopTitle.Text.Trim(),
            centerText: txtStampCenterText.Text.Trim(),
            customStampPhysicalPath: !string.IsNullOrEmpty(customPath) ? Server.MapPath(customPath) : null,
            stampDate: DateTime.Now
        );

        imgStampAnimatePreview.ImageUrl = ResolveUrl(stampVirtualPath) + "?t=" + DateTime.Now.Ticks;
        return stampVirtualPath;
    }

    private void BindPendingQueue()
    {
        // 嚴格控管：審核官只能看到處於自己層級 (CurrentStage == RoleLevel) 的待審單
        string sql = @"
            SELECT a.*, u.EmployeeNo, u.FullName, u.Department, u.JobTitle
            FROM dbo.CardApplications a
            INNER JOIN dbo.Users u ON a.ApplicantID = u.UserID
            WHERE a.CurrentStage = @RoleLevel
            ORDER BY a.UpdatedAt ASC, a.AppID ASC";

        using (SqlConnection conn = new SqlConnection(DbHelper.ConnectionString))
        using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
        {
            da.SelectCommand.Parameters.AddWithValue("@RoleLevel", CurrentRoleLevel);
            DataTable dt = new DataTable();
            da.Fill(dt);
            gvPendingQueue.DataSource = dt;
            gvPendingQueue.DataBind();
        }
    }

    protected void gvPendingQueue_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != "SelectApp") return;
        int appId = Convert.ToInt32(e.CommandArgument);

        string sql = @"
            SELECT a.*, u.EmployeeNo, u.FullName, u.Department
            FROM dbo.CardApplications a
            INNER JOIN dbo.Users u ON a.ApplicantID = u.UserID
            WHERE a.AppID = @AppID AND a.CurrentStage = @RoleLevel";

        using (SqlConnection conn = new SqlConnection(DbHelper.ConnectionString))
        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@AppID", appId);
            cmd.Parameters.AddWithValue("@RoleLevel", CurrentRoleLevel);
            conn.Open();
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    hfSelectedAppId.Value = appId.ToString();
                    litSelectedAppId.Text = appId.ToString();
                    litSelectedApplicant.Text = string.Format("{0} ({1} - {2})", dr["FullName"], dr["Department"], dr["EmployeeNo"]);
                    pnlDecisionBox.Visible = true;
                    pnlLevel4CardIssue.Visible = (CurrentRoleLevel == 4);

                    if (CurrentRoleLevel == 4 && string.IsNullOrEmpty(txtIssuedCardNo.Text))
                    {
                        txtIssuedCardNo.Text = string.Format("SC-{0:yyyyMMdd}-{1:D4}", DateTime.Now, appId);
                    }

                    DisplayPdf(Convert.ToString(dr["PdfPath"]));
                }
            }
        }
    }

    protected void btnExecuteDecision_Click(object sender, EventArgs e)
    {
        int appId = Convert.ToInt32(hfSelectedAppId.Value);
        if (appId <= 0) return;

        bool isApproved = (rblDecision.SelectedValue == "Approve");
        string issuedCardNo = null;

        if (isApproved && CurrentRoleLevel == 4)
        {
            issuedCardNo = txtIssuedCardNo.Text.Trim();
            if (string.IsNullOrEmpty(issuedCardNo))
            {
                ShowMessage("⚠️ 第四級製卡中心核定通過時，必須輸入「配發實體卡號」！", "error");
                return;
            }
        }

        // 先產生本級審核官的最新職官章圖檔
        string currentStampVirtualPath = RenderCurrentReviewerStampPreview();

        using (SqlConnection conn = new SqlConnection(DbHelper.ConnectionString))
        {
            conn.Open();

            // 讀取申請單完整資訊以重新合成 A4 PDF
            string selectSql = @"
                SELECT a.*, u.EmployeeNo, u.FullName, u.Department, u.JobTitle
                FROM dbo.CardApplications a
                INNER JOIN dbo.Users u ON a.ApplicantID = u.UserID
                WHERE a.AppID = @AppID AND a.CurrentStage = @RoleLevel";

            string empNo = "", fullName = "", dept = "", jobTitle = "", cardCategory = "", applyReasons = "", photoPath = "";
            string[] stampVirtualPaths = new string[4];

            using (SqlCommand cmd = new SqlCommand(selectSql, conn))
            {
                cmd.Parameters.AddWithValue("@AppID", appId);
                cmd.Parameters.AddWithValue("@RoleLevel", CurrentRoleLevel);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.Read())
                    {
                        ShowMessage("找不到該筆待審單或已被處理。", "error");
                        return;
                    }
                    empNo = Convert.ToString(dr["EmployeeNo"]);
                    fullName = Convert.ToString(dr["FullName"]);
                    dept = Convert.ToString(dr["Department"]);
                    jobTitle = Convert.ToString(dr["JobTitle"]);
                    cardCategory = Convert.ToString(dr["CardCategory"]);
                    applyReasons = Convert.ToString(dr["ApplyReasons"]);
                    photoPath = Convert.ToString(dr["PhotoPath"]);

                    stampVirtualPaths[0] = dr["Level1StampPath"] != DBNull.Value ? Convert.ToString(dr["Level1StampPath"]) : null;
                    stampVirtualPaths[1] = dr["Level2StampPath"] != DBNull.Value ? Convert.ToString(dr["Level2StampPath"]) : null;
                    stampVirtualPaths[2] = dr["Level3StampPath"] != DBNull.Value ? Convert.ToString(dr["Level3StampPath"]) : null;
                    stampVirtualPaths[3] = dr["Level4StampPath"] != DBNull.Value ? Convert.ToString(dr["Level4StampPath"]) : null;
                }
            }

            string pdfFolder = Server.MapPath("~/Uploads/GeneratedPdfs/");
            string newPdfName = string.Format("APP_{0}_STAGE{1}_{2:yyyyMMddHHmmss}.pdf", appId, isApproved ? CurrentRoleLevel.ToString() : "RETURNED", DateTime.Now);
            string newPdfPhysicalPath = Path.Combine(pdfFolder, newPdfName);
            string newPdfVirtualPath = "~/Uploads/GeneratedPdfs/" + newPdfName;

            if (isApproved)
            {
                // 壓印本級職官章
                stampVirtualPaths[CurrentRoleLevel - 1] = currentStampVirtualPath;
                string[] physicalStamps = new string[4];
                for (int i = 0; i < 4; i++)
                {
                    if (!string.IsNullOrEmpty(stampVirtualPaths[i]))
                        physicalStamps[i] = Server.MapPath(stampVirtualPaths[i]);
                }

                PdfService.GenerateApplicationPdf(
                    outputPdfPath: newPdfPhysicalPath,
                    empNo: empNo,
                    fullName: fullName,
                    department: dept,
                    jobTitle: jobTitle,
                    cardCategory: cardCategory,
                    applyReasons: applyReasons,
                    photoPhysicalPath: Server.MapPath(photoPath),
                    stampPaths: physicalStamps,
                    issuedCardNo: issuedCardNo,
                    appId: appId
                );

                int nextStage = CurrentRoleLevel + 1; // 若第4級通過則變為 5 (已結案)
                string stampColPath = string.Format("Level{0}StampPath", CurrentRoleLevel);
                string stampColTime = string.Format("Level{0}StampTime", CurrentRoleLevel);

                string updateSql = string.Format(@"
                    UPDATE dbo.CardApplications
                    SET CurrentStage = @NextStage,
                        PdfPath = @PdfPath,
                        IssuedCardNo = ISNULL(@CardNo, IssuedCardNo),
                        {0} = @StampPath,
                        {1} = GETDATE(),
                        UpdatedAt = GETDATE()
                    WHERE AppID = @AppID", stampColPath, stampColTime);

                using (SqlCommand cmd = new SqlCommand(updateSql, conn))
                {
                    cmd.Parameters.AddWithValue("@NextStage", nextStage);
                    cmd.Parameters.AddWithValue("@PdfPath", newPdfVirtualPath);
                    cmd.Parameters.AddWithValue("@CardNo", (object)issuedCardNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@StampPath", currentStampVirtualPath);
                    cmd.Parameters.AddWithValue("@AppID", appId);
                    cmd.ExecuteNonQuery();
                }

                LogApproval(conn, appId, "Approved", currentStampVirtualPath);
                DisplayPdf(newPdfVirtualPath);

                if (CurrentRoleLevel == 4)
                    ShowMessage(string.Format("🎉 單號 #{0} 已蓋下第四顆職官章並配發實體卡號【{1}】，全案正式結案！", appId, issuedCardNo), "success");
                else
                    ShowMessage(string.Format("✅ 單號 #{0} 審核通過！已自動將您的職官章壓印至 PDF 並流轉至第 {1} 級審核。", appId, nextStage), "success");
            }
            else
            {
                // 退回機制：清除所有先前已蓋的職官章，重新產生無章 PDF 並設為 CurrentStage = -1
                PdfService.GenerateApplicationPdf(
                    outputPdfPath: newPdfPhysicalPath,
                    empNo: empNo,
                    fullName: fullName,
                    department: dept,
                    jobTitle: jobTitle,
                    cardCategory: cardCategory,
                    applyReasons: applyReasons,
                    photoPhysicalPath: Server.MapPath(photoPath),
                    stampPaths: null,
                    issuedCardNo: null,
                    appId: appId
                );

                string rejectSql = @"
                    UPDATE dbo.CardApplications
                    SET CurrentStage = -1,
                        PdfPath = @PdfPath,
                        IssuedCardNo = NULL,
                        Level1StampPath = NULL, Level2StampPath = NULL, Level3StampPath = NULL, Level4StampPath = NULL,
                        Level1StampTime = NULL, Level2StampTime = NULL, Level3StampTime = NULL, Level4StampTime = NULL,
                        UpdatedAt = GETDATE()
                    WHERE AppID = @AppID";

                using (SqlCommand cmd = new SqlCommand(rejectSql, conn))
                {
                    cmd.Parameters.AddWithValue("@PdfPath", newPdfVirtualPath);
                    cmd.Parameters.AddWithValue("@AppID", appId);
                    cmd.ExecuteNonQuery();
                }

                LogApproval(conn, appId, "Rejected", null);
                DisplayPdf(newPdfVirtualPath);
                ShowMessage(string.Format("❌ 單號 #{0} 已退回給申請人重填，並已自動清除先前已蓋之所有職官章。", appId), "info");
            }
        }

        pnlDecisionBox.Visible = false;
        BindPendingQueue();
    }

    private void LogApproval(SqlConnection conn, int appId, string result, string stampSnapshot)
    {
        string sql = @"
            INSERT INTO dbo.ApprovalLogs (AppID, ReviewerID, RoleLevel, ActionResult, StampSnapshot)
            VALUES (@AppID, @ReviewerID, @RoleLevel, @Result, @Stamp)";
        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@AppID", appId);
            cmd.Parameters.AddWithValue("@ReviewerID", Convert.ToInt32(Session["UserID"]));
            cmd.Parameters.AddWithValue("@RoleLevel", CurrentRoleLevel);
            cmd.Parameters.AddWithValue("@Result", result);
            cmd.Parameters.AddWithValue("@Stamp", (object)stampSnapshot ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
    }

    private void DisplayPdf(string virtualPdfPath)
    {
        string url = ResolveUrl(virtualPdfPath) + "?t=" + DateTime.Now.Ticks;
        pnlEmptyPdf.Visible = false;
        iframeReviewerPdf.Visible = true;
        iframeReviewerPdf.Attributes["src"] = url;
        hlOpenPdf.NavigateUrl = url;
        hlOpenPdf.Visible = true;
    }

    protected void btnLogout_Click(object sender, EventArgs e)
    {
        Session.Clear();
        Response.Redirect("Login.aspx");
    }

    private void ShowMessage(string msg, string type)
    {
        pnlMessage.Visible = true;
        lblMessage.CssClass = "status-alert alert-" + type;
        lblMessage.Text = msg;
    }
}
