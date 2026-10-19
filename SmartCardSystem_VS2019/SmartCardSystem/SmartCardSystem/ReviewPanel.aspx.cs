using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SmartCardSystem
{
    public partial class ReviewPanel : System.Web.UI.Page
    {
        private string connStr = ConfigurationManager.ConnectionStrings["SmartCardDBConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 驗證 Session 是否已登入且為第 1~4 級審核官
            if (Session["UserID"] == null || Session["RoleLevel"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            int roleLevel = Convert.ToInt32(Session["RoleLevel"]);
            if (roleLevel < 1 || roleLevel > 4)
            {
                Response.Redirect("ApplyCard.aspx");
                return;
            }

            if (!IsPostBack)
            {
                lblReviewerInfo.Text = $"當前登入：{Session["FullName"]} (第 {roleLevel} 級審核權限)";
                LoadPendingApplications();
                LoadApprovalLogs();
            }
        }

        private void LoadPendingApplications()
        {
            int currentLevel = Convert.ToInt32(Session["RoleLevel"]);
            UpdateStandardBanner(currentLevel);

            // 根據 Session["RoleLevel"] 只撈取 CurrentStatus 等於該審核官層級的申請單
            string sql = "SELECT * FROM CardApplications WHERE CurrentStatus = @Level ORDER BY ApplyDate ASC";
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
            {
                da.SelectCommand.Parameters.AddWithValue("@Level", currentLevel);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvApplications.DataSource = dt;
                gvApplications.DataBind();
            }
        }

        private void LoadApprovalLogs()
        {
            string sql = "SELECT TOP 20 * FROM ApprovalLogs ORDER BY LogID DESC";
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvLogs.DataSource = dt;
                gvLogs.DataBind();
            }
        }

        private void UpdateStandardBanner(int level)
        {
            switch (level)
            {
                case 1: lblStandardInfo.Text = "【第一級審核標準 (直屬主管)】確認申請人確實隸屬於該單位、證件照片清晰符合規定，且具備日常通行或業務申請需求。"; break;
                case 2: lblStandardInfo.Text = "【第二級審核標準 (部門主管)】確認申請之卡片種類與權限等級符合部門安全規範，無越權申請。"; break;
                case 3: lblStandardInfo.Text = "【第三級審核標準 (資安管制官)】進行資安查核，確認申請人無違規紀錄，核准高敏區域或特殊晶片功能授權。"; break;
                case 4: lblStandardInfo.Text = "【第四級審核標準 (製卡中心)】確認實體卡片庫存、印製照片並寫入晶片內碼，請務必於下方填入「配發之實體卡號」以完成發卡結案。"; break;
            }
        }

        protected void gvApplications_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Approve" && e.CommandName != "Reject") return;

            int rowIndex = Convert.ToInt32(e.CommandArgument);
            int appId = Convert.ToInt32(gvApplications.DataKeys[rowIndex].Value);
            GridViewRow row = gvApplications.Rows[rowIndex];

            string comment = ((TextBox)row.FindControl("txtComment")).Text.Trim();
            string cardNumber = ((TextBox)row.FindControl("txtCardNumber")).Text.Trim();
            int currentLevel = Convert.ToInt32(Session["RoleLevel"]);
            bool isApprove = (e.CommandName == "Approve");

            if (string.IsNullOrEmpty(comment))
            {
                lblStatusMsg.Text = "錯誤：請務必填寫審核意見！";
                lblStatusMsg.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (isApprove && currentLevel == 4 && string.IsNullOrEmpty(cardNumber))
            {
                lblStatusMsg.Text = "錯誤：第四級製卡核定必須輸入「配發實體卡號」！";
                lblStatusMsg.ForeColor = System.Drawing.Color.Red;
                return;
            }

            // 計算下一階段狀態碼：核准則 +1 (1->2->3->4->5結案)，退回則直接設為 -1 (退回申請人重填)
            int nextStatus = isApprove ? (currentLevel + 1) : -1;
            string rejectNote = isApprove ? null : $"[第{currentLevel}級退回] {comment}";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    // 1. 更新主表狀態
                    string updateSql = @"UPDATE CardApplications 
                                         SET CurrentStatus = @NextStatus, 
                                             RejectReason = @RejectReason,
                                             CardNumber = CASE WHEN @Level = 4 AND @IsApprove = 1 THEN @CardNo ELSE CardNumber END,
                                             LastModified = GETDATE() 
                                         WHERE AppID = @AppID AND CurrentStatus = @Level";

                    using (SqlCommand cmdUpdate = new SqlCommand(updateSql, conn, trans))
                    {
                        cmdUpdate.Parameters.AddWithValue("@NextStatus", nextStatus);
                        cmdUpdate.Parameters.AddWithValue("@RejectReason", (object)rejectNote ?? DBNull.Value);
                        cmdUpdate.Parameters.AddWithValue("@Level", currentLevel);
                        cmdUpdate.Parameters.AddWithValue("@IsApprove", isApprove ? 1 : 0);
                        cmdUpdate.Parameters.AddWithValue("@CardNo", string.IsNullOrEmpty(cardNumber) ? (object)DBNull.Value : cardNumber);
                        cmdUpdate.Parameters.AddWithValue("@AppID", appId);

                        int affected = cmdUpdate.ExecuteNonQuery();
                        if (affected == 0) throw new Exception("該申請單狀態已變更，請重新整理頁面。");
                    }

                    // 2. 寫入簽核歷程紀錄表
                    string logSql = @"INSERT INTO ApprovalLogs 
                                      (AppID, ApprovalLevel, ReviewerID, ReviewerName, ActionResult, Comment) 
                                      VALUES (@AppID, @Level, @ReviewerID, @ReviewerName, @Result, @Comment)";

                    using (SqlCommand cmdLog = new SqlCommand(logSql, conn, trans))
                    {
                        cmdLog.Parameters.AddWithValue("@AppID", appId);
                        cmdLog.Parameters.AddWithValue("@Level", currentLevel);
                        cmdLog.Parameters.AddWithValue("@ReviewerID", Session["UserID"].ToString());
                        cmdLog.Parameters.AddWithValue("@ReviewerName", Session["FullName"].ToString());
                        cmdLog.Parameters.AddWithValue("@Result", isApprove ? "Approved" : "Rejected");
                        cmdLog.Parameters.AddWithValue("@Comment", comment);
                        cmdLog.ExecuteNonQuery();
                    }

                    trans.Commit();
                    lblStatusMsg.Text = isApprove
                        ? (nextStatus == 5 ? $"單號 #{appId} 已完成第四級最終核定並配發卡號 ({cardNumber})！" : $"單號 #{appId} 已核准！流轉至第 {nextStatus} 級審核。")
                        : $"單號 #{appId} 已直接退回給申請人重填！";
                    lblStatusMsg.ForeColor = System.Drawing.Color.Green;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    lblStatusMsg.Text = "簽核失敗：" + ex.Message;
                    lblStatusMsg.ForeColor = System.Drawing.Color.Red;
                }
            }

            LoadPendingApplications();
            LoadApprovalLogs();
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("Login.aspx");
        }
    }
}
