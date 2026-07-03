using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class EmployeeManagement : Form
    {
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        private TextBox txtStaffID, txtName;
        private ComboBox cboRole;
        private DataGridView dgvStaff;

        // ⭐ 新增 Search TextBox
        private TextBox txtSearch;

        public EmployeeManagement()
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                InitializeUI();
                LoadData();
                ClearForm(); // 載入後自動進入「新增」模式並獲取最新 ID
            }
        }

        private void InitializeUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living - Staff Master Data";
            this.Size = new Size(1180, 750);
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Font = new Font("Segoe UI", 10F);

            // 權限檢查：只有 Manager 同 Admin 睇到呢版
            AuthorizationHelper.EnforceRole(this, "Manager", "Administrator");

            Label lblHeader = new Label { Text = "👔 Staff Master Data Maintenance", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(30, 20), AutoSize = true };
            this.Controls.Add(lblHeader);

            Panel pnlCard = new Panel { Location = new Point(30, 85), Size = new Size(380, 560), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(pnlCard);

            int startY = 20;
            // 🌟 將 Staff ID 標籤改為提示由系統自動生成
            txtStaffID = CreateInput(pnlCard, ref startY, "Staff ID (Auto-Generated / Default Password):");
            txtStaffID.ReadOnly = true; // 🌟 鎖定輸入框，唔畀手打
            txtStaffID.BackColor = Color.FromArgb(241, 245, 249); // 轉為灰色背景提示無法編輯

            txtName = CreateInput(pnlCard, ref startY, "Staff Name *:");

            Label lblRole = new Label { Text = "System Role *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboRole = new ComboBox { Location = new Point(20, startY + 22), Width = 335, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F) };
            cboRole.Items.AddRange(new string[] { "Administrator", "Sales Representative", "Logistics Driver", "Warehouse Specialist", "Procurement Officer" });
            pnlCard.Controls.Add(lblRole); pnlCard.Controls.Add(cboRole);
            startY += 75;

            // 基本 CRUD 按鈕
            Button btnAdd = new Button { Text = "➕ Add", Location = new Point(20, startY), Size = new Size(160, 40), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            Button btnUpdate = new Button { Text = "💾 Update", Location = new Point(195, startY), Size = new Size(160, 40), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            Button btnDelete = new Button { Text = "🗑️ Delete", Location = new Point(20, startY + 50), Size = new Size(160, 40), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            Button btnClear = new Button { Text = "🧹 Clear", Location = new Point(195, startY + 50), Size = new Size(160, 40), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };

            // 新增「重設密碼」按鈕
            Button btnResetPwd = new Button { Text = "🔑 Reset Password to Default", Location = new Point(20, startY + 100), Size = new Size(335, 40), BackColor = Color.FromArgb(245, 158, 11), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };

            // 移除邊框
            foreach (Button b in new Button[] { btnAdd, btnUpdate, btnDelete, btnClear, btnResetPwd }) b.FlatAppearance.BorderSize = 0;

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += (s, e) => ClearForm();
            btnResetPwd.Click += BtnResetPwd_Click;

            pnlCard.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear, btnResetPwd });

            // ⭐ 補返 Search Box UI (放喺 DataGridView 上面)
            Label lblSearch = new Label { Text = "🔍 Live Search (ID / Name / Role):", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(440, 52), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(680, 48), Width = 460, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            txtSearch.TextChanged += TxtSearch_TextChanged; // 綁定即時搜尋 Event
            this.Controls.Add(lblSearch);
            this.Controls.Add(txtSearch);

            dgvStaff = new DataGridView { Location = new Point(440, 85), Size = new Size(700, 560), BackgroundColor = Color.White, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvStaff.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvStaff.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvStaff.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold); // 標題粗體
            dgvStaff.RowsDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular);      // 🌟 資料列強制轉為正常字體

            dgvStaff.SelectionChanged += DgvStaff_SelectionChanged;
            this.Controls.Add(dgvStaff);
        }

        // ⭐ 新增 Search Event 邏輯
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (dgvStaff.DataSource is DataTable dt)
            {
                // 用 Replace 防止用家入單引號 (') 導致 SQL Syntax Error
                string keyword = txtSearch.Text.Trim().Replace("'", "''");

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    dt.DefaultView.RowFilter = "";
                }
                else
                {
                    // 可以同時 Search StaffID, Name 同 Role
                    dt.DefaultView.RowFilter = $"[StaffID] LIKE '%{keyword}%' OR [Name] LIKE '%{keyword}%' OR [Role] LIKE '%{keyword}%'";
                }
            }
        }

        private TextBox CreateInput(Panel container, ref int y, string label)
        {
            Label lbl = new Label { Text = label, Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(20, y + 22), Width = 335, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            container.Controls.Add(lbl); container.Controls.Add(txt);
            y += 65; return txt;
        }

        // 🌟 核心：從資料庫獲取下一個自動遞增的 StaffID
        private string GetNextStaffID(MySqlConnection conn)
        {
            string query = "SELECT StaffID FROM staff WHERE StaffID LIKE 'S%' ORDER BY StaffID DESC LIMIT 1";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    string maxId = result.ToString();
                    // 假設格式係 S001, S002... 提取後面的數字並加 1
                    if (maxId.Length >= 2 && int.TryParse(maxId.Substring(1), out int currentInt))
                    {
                        return "S" + (currentInt + 1).ToString("D3");
                    }
                }
            }
            return "S001"; // 如果資料庫無任何記錄，預設由 S001 開始
        }

        // 🌟 更新 UI 顯示下一個 ID
        private void GenerateNextStaffIDUI()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    txtStaffID.Text = GetNextStaffID(conn);
                }
                catch
                {
                    txtStaffID.Text = "S001";
                }
            }
        }

        private void LoadData()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlDataAdapter da = new MySqlDataAdapter("SELECT StaffID, Name, Role FROM staff", conn))
                    {
                        DataTable dt = new DataTable(); da.Fill(dt);
                        dgvStaff.DataSource = dt;

                        // ⭐ 重置/套用搜尋字眼（確保 Reload 資料後唔會卡住）
                        if (txtSearch != null && !string.IsNullOrWhiteSpace(txtSearch.Text))
                        {
                            TxtSearch_TextChanged(null, null);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error loading data: " + ex.Message); }
            }
        }

        private void DgvStaff_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStaff.SelectedRows.Count > 0)
            {
                DataGridViewRow r = dgvStaff.SelectedRows[0];
                txtStaffID.Text = r.Cells["StaffID"].Value.ToString();
                txtName.Text = r.Cells["Name"].Value.ToString();
                cboRole.Text = r.Cells["Role"].Value.ToString();
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || cboRole.SelectedIndex == -1)
            {
                MessageBox.Show("Name and Role are required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    // 🌟 即時重新獲取最新嘅 Staff ID，避免多人同時操作時撞 ID
                    string newStaffID = GetNextStaffID(conn);
                    string defaultPassword = newStaffID;

                    string sql = "INSERT INTO staff (StaffID, Name, Password, Role) VALUES (@id, @n, SHA2(@pwd, 256), @r)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", newStaffID);
                        cmd.Parameters.AddWithValue("@n", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@pwd", defaultPassword);
                        cmd.Parameters.AddWithValue("@r", cboRole.Text);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show($"Staff member added successfully!\n\nAllocated Staff ID: {newStaffID}\nThe default password is set to their Staff ID: [{defaultPassword}]", "Account Created", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadData();
                    ClearForm(); // 新增完自動清空並準備下一個 ID
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStaffID.Text)) return;
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string sql = "UPDATE staff SET Name=@n, Role=@r WHERE StaffID=@id";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtStaffID.Text.Trim());
                        cmd.Parameters.AddWithValue("@n", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@r", cboRole.Text);
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Staff information updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); LoadData();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void BtnResetPwd_Click(object sender, EventArgs e)
        {
            string targetStaff = txtStaffID.Text.Trim();
            if (string.IsNullOrWhiteSpace(targetStaff))
            {
                MessageBox.Show("Please select a staff member from the list first.", "Action Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show($"Are you sure you want to reset the password for [{targetStaff}]?\n\nTheir password will be reset to their Staff ID.", "Confirm Password Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        string sql = "UPDATE staff SET Password = SHA2(@pwd, 256) WHERE StaffID = @id";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", targetStaff);
                            cmd.Parameters.AddWithValue("@pwd", targetStaff); // Password equals StaffID
                            cmd.ExecuteNonQuery();
                        }
                        MessageBox.Show($"Password for [{targetStaff}] has been successfully reset to default.", "Password Reset", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex) { MessageBox.Show("Error resetting password: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStaffID.Text)) return;
            if (txtStaffID.Text == UserSession.LoggedInStaffID) { MessageBox.Show("You cannot delete yourself!"); return; }

            if (MessageBox.Show("Delete this staff member?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("DELETE FROM staff WHERE StaffID=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", txtStaffID.Text.Trim());
                            cmd.ExecuteNonQuery();
                        }
                        LoadData(); ClearForm();
                    }
                    catch (Exception) { MessageBox.Show("Cannot delete staff. They might be linked to existing orders or records."); }
                }
            }
        }

        private void EmployeeManagement_Load(object sender, EventArgs e)
        {

        }

        private void ClearForm()
        {
            txtName.Clear();
            cboRole.SelectedIndex = -1;
            dgvStaff.ClearSelection();

            // 🌟 清空表單後，自動生成並顯示下一個可以用的 StaffID
            GenerateNextStaffIDUI();
        }
    }
}