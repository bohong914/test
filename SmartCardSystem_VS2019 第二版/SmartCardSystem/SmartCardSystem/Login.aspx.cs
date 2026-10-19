using System;
using System.Configuration;
using System.Data.SqlClient;

namespace SmartCardSystem
{
    public partial class Login : System.Web.UI.Page
    {
        private string connStr = ConfigurationManager.ConnectionStrings["SmartCardDBConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Session.Clear();
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string userId = txtUserID.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "請輸入帳號與密碼！";
                return;
            }

            string sql = "SELECT UserID, FullName, Department, RoleLevel FROM Users WHERE UserID = @uid AND PasswordHash = @pwd";

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@pwd", password);

                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        int roleLevel = Convert.ToInt32(dr["RoleLevel"]);
                        Session["UserID"] = dr["UserID"].ToString();
                        Session["FullName"] = dr["FullName"].ToString();
                        Session["Department"] = dr["Department"].ToString();
                        Session["RoleLevel"] = roleLevel;

                        // 根據 RoleLevel 自動導向：0 為一般申請人，1~4 為各層級審核官
                        if (roleLevel == 0)
                        {
                            Response.Redirect("ApplyCard.aspx");
                        }
                        else if (roleLevel >= 1 && roleLevel <= 4)
                        {
                            Response.Redirect("ReviewPanel.aspx");
                        }
                    }
                    else
                    {
                        lblError.Text = "帳號或密碼錯誤！";
                    }
                }
            }
        }
    }
}
