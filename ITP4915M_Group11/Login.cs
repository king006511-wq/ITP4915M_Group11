using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;

// 呢個係你個 Project 嘅名 (見到你右上角叫 ITP4915M_Group11)
namespace ITP4915M_Group11
{
    // 呢個就係包住你所有 Code 嗰間「屋」
    public partial class Login : Form
    {
        // 呢個係 Login 嘅建構子，用嚟載入你畫嘅 UI (千祈唔可以刪除)
        public Login()
        {
            InitializeComponent();
        }

        // 呢度開始就係你啱啱寫嘅 Login Button Code
        private void btnLogin_Click(object sender, EventArgs e)
        {
            // 確保輸入框唔係留空
            if (string.IsNullOrEmpty(txtUser.Text) || string.IsNullOrEmpty(txtPass.Text))
            {
                MessageBox.Show("請輸入 Staff ID 和密碼！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 使用你之前測試成功嘅連線字串
            string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

            // 使用 using 確保連線用完會自動關閉，唔會霸住資源
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    // 根據你 phpMyAdmin 嘅真實欄位名（StaffID 同 Password）黎寫 SQL
                    // SELECT Name, Role 代表如果登入成功，順便攞埋佢個名同職位出嚟
                    string query = "SELECT Name, Role FROM staff WHERE StaffID = @user AND Password = @pass";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // 使用安全參數 (Parameters)，防止被黑客用 SQL Injection 破解，阿 Sir 睇到實加分
                        cmd.Parameters.AddWithValue("@user", txtUser.Text.Trim());
                        cmd.Parameters.AddWithValue("@pass", txtPass.Text.Trim());

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // 成功讀取到資料，代表帳號密碼完全正確！
                                string staffName = reader["Name"].ToString();
                                string staffRole = reader["Role"].ToString();

                                MessageBox.Show($"歡迎回來，{staffName}！\n您的權限級別為：{staffRole}",
                                                "登入成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // --- 重點修改：將 staffName 同 staffRole 放入括號帶過去！ ---
                                MainMenu mainMenu = new MainMenu(staffName, staffRole);
                                mainMenu.Show();
                                this.Hide();
                            }
                            else
                            {
                                // 搵唔到對應嘅資料，代表入錯嘢
                                MessageBox.Show("Staff ID 或密碼錯誤，請重新輸入！", "登入失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("系統連線出錯：" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void txtUser_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
