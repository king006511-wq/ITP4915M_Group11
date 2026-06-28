using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class EmployeeManagement : Form
    {
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        private TextBox txtStaffID, txtName, txtEmail;
        private ComboBox cboRole;
        private DataGridView dgvStaff;

        public EmployeeManagement()
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                InitializeUI();
                LoadData();
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

            // 只有 Manager 同 Admin 可以管理員工
            AuthorizationHelper.EnforceRole(this, "Manager", "Administrator");

            Label lblHeader = new Label { Text = "👔 Staff Master Data Maintenance", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(30, 20), AutoSize = true };
            this.Controls.Add(lblHeader);

            Panel pnlCard = new Panel { Location = new Point(30, 85), Size = new Size(380, 500), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(pnlCard);

            int startY = 20;
            txtStaffID = CreateInput(pnlCard, ref startY, "Staff ID *:");
            txtName = CreateInput(pnlCard, ref startY, "Staff Name *:");
            txtEmail = CreateInput(pnlCard, ref startY, "Staff Email (Gmail) *:");

            // Role Combo
            Label lblRole = new Label { Text = "System Role *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboRole = new ComboBox { Location = new Point(20, startY + 22), Width = 335, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F) };
            cboRole.Items.AddRange(new string[] { "Manager", "Administrator", "Sales Representative", "Logistics Driver", "Warehouse Specialist", "Procurement Officer", "System Manager" });
            pnlCard.Controls.Add(lblRole); pnlCard.Controls.Add(cboRole);
            startY += 75;

            Button btnAdd = new Button { Text = "➕ Add", Location = new Point(20, startY), Size = new Size(160, 40), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            Button btnUpdate = new Button { Text = "💾 Update", Location = new Point(195, startY), Size = new Size(160, 40), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            Button btnDelete = new Button { Text = "🗑️ Delete", Location = new Point(20, startY + 50), Size = new Size(160, 40), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            Button btnClear = new Button { Text = "🧹 Clear", Location = new Point(195, startY + 50), Size = new Size(160, 40), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };

            btnAdd.Click += BtnAdd_Click; btnUpdate.Click += BtnUpdate_Click; btnDelete.Click += BtnDelete_Click; btnClear.Click += (s, e) => ClearForm();
            pnlCard.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });

            dgvStaff = new DataGridView { Location = new Point(440, 85), Size = new Size(700, 600), BackgroundColor = Color.White, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvStaff.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvStaff.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvStaff.SelectionChanged += DgvStaff_SelectionChanged;
            this.Controls.Add(dgvStaff);
        }

        private TextBox CreateInput(Panel container, ref int y, string label)
        {
            Label lbl = new Label { Text = label, Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(20, y + 22), Width = 335, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            container.Controls.Add(lbl); container.Controls.Add(txt);
            y += 65; return txt;
        }

        private void LoadData()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // 加入 Email 顯示
                    using (MySqlDataAdapter da = new MySqlDataAdapter("SELECT StaffID, Name, Email, Role FROM staff", conn))
                    {
                        DataTable dt = new DataTable(); da.Fill(dt);
                        dgvStaff.DataSource = dt;
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
                // 讀取 Email
                txtEmail.Text = r.Cells["Email"].Value != DBNull.Value ? r.Cells["Email"].Value.ToString() : "";
                cboRole.Text = r.Cells["Role"].Value.ToString();
                txtStaffID.ReadOnly = true;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStaffID.Text) || string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text) || cboRole.SelectedIndex == -1)
            {
                MessageBox.Show("ID, Name, Email, and Role are required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🌟 隨機生成 6 位數字密碼
            Random rnd = new Random();
            string tempPassword = rnd.Next(100000, 999999).ToString();

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // 寫入 DB 並將密碼進行 SHA256 加密，加入 Email
                    string sql = "INSERT INTO staff (StaffID, Name, Email, Password, Role) VALUES (@id, @n, @email, SHA2(@pwd, 256), @r)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtStaffID.Text.Trim());
                        cmd.Parameters.AddWithValue("@n", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@pwd", tempPassword);
                        cmd.Parameters.AddWithValue("@r", cboRole.Text);
                        cmd.ExecuteNonQuery();
                    }

                    // 寄出 Email 通知員工
                    bool emailSent = SendWelcomeEmail(txtEmail.Text.Trim(), txtStaffID.Text.Trim(), txtName.Text.Trim(), tempPassword);

                    if (emailSent)
                    {
                        MessageBox.Show($"Staff member added successfully!\n\nA system generated password has been sent to {txtEmail.Text}.", "Account Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Staff added, but failed to send email. Their temporary password is: {tempPassword}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    LoadData(); ClearForm();
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
                    // 🌟 杜絕修改密碼，只能修改 Name, Email, Role
                    string sql = "UPDATE staff SET Name=@n, Email=@email, Role=@r WHERE StaffID=@id";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtStaffID.Text.Trim());
                        cmd.Parameters.AddWithValue("@n", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@r", cboRole.Text);
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Staff information updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); LoadData();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
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

        private void ClearForm()
        {
            txtStaffID.Clear(); txtName.Clear(); txtEmail.Clear(); cboRole.SelectedIndex = -1;
            txtStaffID.ReadOnly = false; dgvStaff.ClearSelection();
        }

        // ====================================================================
        // 📧 透過 Gmail SMTP 發送歡迎信 (請設定你的真實帳號與應用程式密碼)
        // ====================================================================
        private bool SendWelcomeEmail(string toEmail, string staffId, string staffName, string tempPwd)
        {
            try
            {
                // 請將下面嘅 Email 同 App Password 換做你申請咗嘅資料
                string fromEmail = "your_company_erp@gmail.com";
                string appPassword = "abcd efgh ijkl mnop"; // Gmail 應用程式密碼 (16位字母)

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromEmail, "Premium Living ERP Admin");
                mail.To.Add(toEmail);
                mail.Subject = "Welcome to Premium Living! Your ERP Account Details";

                mail.Body = $@"
Hello {staffName},

Welcome to the Premium Living team! 

Your ERP system account has been successfully created.
Please find your login details below:

Staff ID: {staffId}
Temporary Password: {tempPwd}

⚠️ SECURITY NOTICE:
Please log in to the system as soon as possible and change your password via the Home Dashboard.

Best Regards,
HR & IT Administration
Premium Living
                ";

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(fromEmail, appPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
                return true;
            }
            catch (Exception)
            {
                // 如果 SMTP 設定未搞掂，Catch 住個 error 確保系統唔會彈 app
                return false;
            }
        }
    }
}