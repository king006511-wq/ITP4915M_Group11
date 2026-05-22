using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ITP4915M_Group11
{
    public partial class employeeManagement : Form
    {
        // 統一放連線字串
        private string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        public employeeManagement()
        {
            InitializeComponent();
        }

        // 一打開視窗就會自動行呢度
        private void Form3_Load(object sender, EventArgs e)
        {
            if (cboRole.Items.Count > 0)
            {
                cboRole.SelectedIndex = 0;
            }
            LoadStaffData();
        }

        // 載入數據功能
        private void LoadStaffData()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT StaffID AS '員工編號', Name AS '姓名', Password AS '密碼', Role AS '職位' FROM staff";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dgvStaff.DataSource = dt;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("載入員工資料失敗：" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 新增員工功能
        private void btnAddStaff_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStaffID.Text) || string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("唔該填晒所有欄位！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = "INSERT INTO staff (StaffID, Name, Password, Role) VALUES (@id, @name, @pass, @role)";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtStaffID.Text.Trim());
                        cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@pass", txtPassword.Text);
                        cmd.Parameters.AddWithValue("@role", cboRole.Text);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("成功新增員工！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            LoadStaffData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("新增失敗：\n" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 修改資料功能
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStaffID.Text) || string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("唔該先喺表格點選要修改嘅員工，並填妥資料！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = "UPDATE staff SET Name = @name, Password = @pass, Role = @role WHERE StaffID = @id";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtStaffID.Text.Trim());
                        cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@pass", txtPassword.Text);
                        cmd.Parameters.AddWithValue("@role", cboRole.Text);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("員工資料修改成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            LoadStaffData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("修改失敗：\n" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 【大結局功能】撳「刪除員工」掣會行呢度
        private void btnDelete_Click(object sender, EventArgs e)
        {
            // 1. 防呆：確保選取咗人先可以刪除
            if (string.IsNullOrWhiteSpace(txtStaffID.Text))
            {
                MessageBox.Show("唔該先喺表格點選你想刪除嘅員工！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. 彈出雙重確認視窗，安全第一
            DialogResult result = MessageBox.Show(
                $"你確定要永久刪除員工 [{txtName.Text}] (編號: {txtStaffID.Text}) 嗎？\n呢個操作無法復原！",
                "警告",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            // 如果用戶選擇「是 (Yes)」，先會真正去 Database 執行刪除
            if (result == DialogResult.Yes)
            {
                // 3. 寫 DELETE SQL 語句
                string query = "DELETE FROM staff WHERE StaffID = @id";

                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", txtStaffID.Text.Trim());

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("員工已成功刪除！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearFields();   // 清空輸入框
                                LoadStaffData(); // 即時更新大表格
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("刪除失敗：\n" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // 點擊表格行，自動填入資料
        private void dgvStaff_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvStaff.Rows[e.RowIndex];
                txtStaffID.Text = row.Cells["員工編號"].Value.ToString();
                txtName.Text = row.Cells["姓名"].Value.ToString();
                txtPassword.Text = row.Cells["密碼"].Value.ToString();
                cboRole.Text = row.Cells["職位"].Value.ToString();

                txtStaffID.ReadOnly = true; // 修改/刪除時，員工編號設為唯讀
            }
        }

        // 清空欄位小功能
        private void ClearFields()
        {
            txtStaffID.Clear();
            txtName.Clear();
            txtPassword.Clear();
            cboRole.SelectedIndex = 0;
            txtStaffID.ReadOnly = false;
        }
    }
}
