using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI.WebControls;

namespace SmartCardSystem
{
    public partial class ApplyCard : System.Web.UI.Page
    {
        private string connStr = ConfigurationManager.ConnectionStrings["SmartCardDBConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 驗證 Session 是否已登入
            if (Session["UserID"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                txtApplicantID.Text = Session["UserID"].ToString();
                txtApplicantName.Text = Session["FullName"].ToString();
                txtDepartment.Text = Session["Department"].ToString();
                lblUserInfo.Text = $"登入者：{Session["FullName"]} ({Session["Department"]}) - 角色：一般申請人";

                LoadMyApplications();
            }
        }

        private void LoadMyApplications()
        {
            string sql = "SELECT * FROM CardApplications WHERE ApplicantID = @uid ORDER BY AppID DESC";
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
            {
                da.SelectCommand.Parameters.AddWithValue("@uid", Session["UserID"].ToString());
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvMyApps.DataSource = dt;
                gvMyApps.DataBind();
            }
        }

        // 狀態標籤格式化
        protected string GetStatusBadge(int status, string rejectReason, string cardNumber)
        {
            switch (status)
            {
                case 1: return "<span class='badge bg-info text-dark'>待第一級審核 (直屬主管)</span>";
                case 2: return "<span class='badge bg-primary'>待第二級審核 (部門主管)</span>";
                case 3: return "<span class='badge bg-warning text-dark'>待第三級審核 (資安管制官)</span>";
                case 4: return "<span class='badge bg-secondary'>待第四級審核 (製卡中心)</span>";
                case 5: return $"<span class='badge bg-success'>已核准發卡 (卡號: {cardNumber})</span>";
                case -1: return $"<span class='badge bg-danger'>已退回重填</span><div class='small text-danger mt-1'>退回原因：{rejectReason}</div>";
                default: return "<span class='badge bg-light text-dark'>未知</span>";
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtApplyReason.Text))
            {
                lblMessage.Text = "請填寫申請事由！";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            bool isReEdit = !string.IsNullOrEmpty(hfEditingAppID.Value);
            string savedPhotoVirtualPath = hfExistingPhoto.Value;

            // 檢查大頭照上傳
            if (fuPhoto.HasFile)
            {
                string ext = Path.GetExtension(fuPhoto.FileName).ToLower();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                {
                    lblMessage.Text = "照片格式錯誤！僅允許上傳 .jpg, .jpeg 或 .png 檔案。";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                string folderPath = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string newFileName = $"{Session["UserID"]}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                string fullSavePath = Path.Combine(folderPath, newFileName);
                fuPhoto.SaveAs(fullSavePath);

                savedPhotoVirtualPath = "~/Uploads/" + newFileName;
            }
            else if (!isReEdit)
            {
                lblMessage.Text = "新申請單請務必上傳「大頭照 / 證件照」！";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                if (!isReEdit)
                {
                    // 新增申請單 -> 進入狀態 1 (待第一級審核)
                    string sql = @"INSERT INTO CardApplications 
                                   (ApplicantID, ApplicantName, Department, CardType, ApplyReason, PhotoPath, CurrentStatus) 
                                   VALUES (@ApplicantID, @ApplicantName, @Department, @CardType, @ApplyReason, @PhotoPath, 1)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ApplicantID", txtApplicantID.Text);
                        cmd.Parameters.AddWithValue("@ApplicantName", txtApplicantName.Text);
                        cmd.Parameters.AddWithValue("@Department", txtDepartment.Text);
                        cmd.Parameters.AddWithValue("@CardType", ddlCardType.SelectedValue);
                        cmd.Parameters.AddWithValue("@ApplyReason", txtApplyReason.Text.Trim());
                        cmd.Parameters.AddWithValue("@PhotoPath", savedPhotoVirtualPath);
                        cmd.ExecuteNonQuery();
                    }
                    lblMessage.Text = "申請單已成功送出！目前狀態：待第一級（直屬主管）審核。";
                }
                else
                {
                    // 退回重填 -> 更新原申請單並將狀態重設為 1 (從第一級重新逐級審核)
                    string updateSql = @"UPDATE CardApplications 
                                         SET CardType = @CardType, 
                                             ApplyReason = @ApplyReason, 
                                             PhotoPath = @PhotoPath, 
                                             CurrentStatus = 1, 
                                             RejectReason = NULL,
                                             LastModified = GETDATE()
                                         WHERE AppID = @AppID AND ApplicantID = @ApplicantID";
                    using (SqlCommand cmd = new SqlCommand(updateSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CardType", ddlCardType.SelectedValue);
                        cmd.Parameters.AddWithValue("@ApplyReason", txtApplyReason.Text.Trim());
                        cmd.Parameters.AddWithValue("@PhotoPath", savedPhotoVirtualPath);
                        cmd.Parameters.AddWithValue("@AppID", int.Parse(hfEditingAppID.Value));
                        cmd.Parameters.AddWithValue("@ApplicantID", txtApplicantID.Text);
                        cmd.ExecuteNonQuery();
                    }
                    lblMessage.Text = $"單號 #{hfEditingAppID.Value} 已完成修正並重新送交第一級審核！";
                    ResetFormMode();
                }
            }

            lblMessage.ForeColor = System.Drawing.Color.Green;
            txtApplyReason.Text = "";
            LoadMyApplications();
        }

        // 點擊「修改重填」載入被退回的申請單資料
        protected void gvMyApps_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditRejected")
            {
                int appId = Convert.ToInt32(e.CommandArgument);
                string sql = "SELECT * FROM CardApplications WHERE AppID = @AppID AND ApplicantID = @uid";
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@AppID", appId);
                    cmd.Parameters.AddWithValue("@uid", Session["UserID"].ToString());
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            hfEditingAppID.Value = appId.ToString();
                            hfExistingPhoto.Value = dr["PhotoPath"].ToString();
                            ddlCardType.SelectedValue = dr["CardType"].ToString();
                            txtApplyReason.Text = dr["ApplyReason"].ToString();

                            if (!string.IsNullOrEmpty(hfExistingPhoto.Value))
                            {
                                imgPreview.ImageUrl = hfExistingPhoto.Value;
                                imgPreview.Visible = true;
                            }

                            lblFormTitle.Text = $"修正退回申請單 (單號 #{appId})";
                            btnSubmit.Text = "確認修正並重新送審 (送交第一級)";
                            btnCancelEdit.Visible = true;
                            lblMessage.Text = "";
                        }
                    }
                }
            }
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            ResetFormMode();
            txtApplyReason.Text = "";
            lblMessage.Text = "";
        }

        private void ResetFormMode()
        {
            hfEditingAppID.Value = "";
            hfExistingPhoto.Value = "";
            imgPreview.Visible = false;
            lblFormTitle.Text = "新增智慧卡申請單";
            btnSubmit.Text = "送出申請 (送交第一級審核)";
            btnCancelEdit.Visible = false;
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("Login.aspx");
        }
    }
}
