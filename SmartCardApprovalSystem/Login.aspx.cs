using System;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

public partial class Login : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        DbHelper.EnsureDatabaseInitialized();
    }

    protected void btnLogin_Click(object sender, EventArgs e)
    {
        AuthenticateUser(txtEmpNo.Text.Trim(), txtPassword.Text.Trim());
    }

    protected void btnQuickLogin_Click(object sender, EventArgs e)
    {
        Button btn = (Button)sender;
        AuthenticateUser(btn.CommandArgument, "123456");
    }

    private void AuthenticateUser(string empNo, string password)
    {
        using (SqlConnection conn = new SqlConnection(DbHelper.ConnectionString))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT * FROM dbo.Users WHERE EmployeeNo = @EmpNo AND PasswordHash = @Pwd", conn))
        {
            cmd.Parameters.AddWithValue("@EmpNo", empNo);
            cmd.Parameters.AddWithValue("@Pwd", password);
            conn.Open();

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int roleLevel = Convert.ToInt32(dr["RoleLevel"]);
                    Session["UserID"] = dr["UserID"];
                    Session["EmployeeNo"] = dr["EmployeeNo"].ToString();
                    Session["FullName"] = dr["FullName"].ToString();
                    Session["Department"] = dr["Department"].ToString();
                    Session["JobTitle"] = dr["JobTitle"].ToString();
                    Session["RoleLevel"] = roleLevel;

                    // 依 RoleLevel 自動分流導向
                    if (roleLevel == 0)
                        Response.Redirect("ApplicantDashboard.aspx");
                    else
                        Response.Redirect("ReviewerWorkbench.aspx");
                }
                else
                {
                    pnlError.Visible = true;
                    lblError.Text = "帳號或密碼錯誤！請確認工號與密碼（測試密碼皆為 123456）。";
                }
            }
        }
    }
}
