using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class CustomerManagement : Form
    {
        // ==========================================
        // 🔒 資料庫配置
        // ==========================================
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // ==========================================
        // 🎨 現代化多彩 UI 元件變數
        // ==========================================
        private TextBox txtCustomerID, txtName, txtPhone, txtAddress, txtSearch;
        private ComboBox cboType;
        private DataGridView dgvCustomer;
        private Button btnAdd, btnUpdate, btnDelete, btnReset;

        public CustomerManagement()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                InitializePerfectMatchUI();
                SetupTypeComboBox();
                LoadCustomerData();
                GenerateNextCustomerID();
            }
        }

        #region 🎨 絕對座標佈局 (修正空白與資料顯示)
        private void InitializePerfectMatchUI()
        {
            this.Controls.Clear();
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(1180, 800);

            // =========================================================
            // 🛡️ 核心修復 1：消除中間多餘空白 (將 X 座標由 260 改為 20)
            // =========================================================
            Panel pnlMain = new Panel
            {
                Location = new Point(20, 0),  // ⬅️ 移去左邊
                Size = new Size(1120, 750)    // ⬅️ 擴大整個面板寬度
            };
            this.Controls.Add(pnlMain);

            // =========================================================
            // 【中間】資料輸入面板 (Inputting Data)
            // =========================================================
            Panel pnlCard = new Panel
            {
                Location = new Point(20, 50),
                Size = new Size(380, 650),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlCard);

            Label lblCardTitle = new Label { Text = "📝 Client Profile Entry", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(79, 70, 229), Location = new Point(20, 15), AutoSize = true };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 60;
            txtCustomerID = CreateStyledTextBox(pnlCard, ref startY, "Customer ID (Auto):", true);
            txtName = CreateStyledTextBox(pnlCard, ref startY, "Client Full Name *:", false);

            Label lblType = new Label { Text = "Account Type *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboType = new ComboBox { Location = new Point(20, startY + 22), Width = 335, Font = new Font("Segoe UI", 10.5F), DropDownStyle = ComboBoxStyle.DropDownList };
            pnlCard.Controls.Add(lblType); pnlCard.Controls.Add(cboType);
            startY += 65;

            txtPhone = CreateStyledTextBox(pnlCard, ref startY, "Contact Phone *:", false);

            Label lblAddr = new Label { Text = "Billing / Delivery Address:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtAddress = new TextBox { Location = new Point(20, startY + 22), Width = 335, Height = 75, Multiline = true, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            pnlCard.Controls.Add(lblAddr); pnlCard.Controls.Add(txtAddress);
            startY += 110;

            btnAdd = new Button { Text = "➕ Add Client", Location = new Point(20, startY), Size = new Size(160, 40), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnUpdate = new Button { Text = "💾 Update", Location = new Point(195, startY), Size = new Size(160, 40), BackColor = Color.FromArgb(59, 130, 246), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };

            startY += 50;
            btnDelete = new Button { Text = "🗑️ Delete", Location = new Point(20, startY), Size = new Size(160, 40), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnReset = new Button { Text = "🔄 Reset", Location = new Point(195, startY), Size = new Size(160, 40), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };

            btnAdd.FlatAppearance.BorderSize = 0; btnUpdate.FlatAppearance.BorderSize = 0; btnDelete.FlatAppearance.BorderSize = 0; btnReset.FlatAppearance.BorderSize = 0;
            btnAdd.Click += btnAdd_Click; btnUpdate.Click += btnUpdate_Click; btnDelete.Click += btnDelete_Click; btnReset.Click += (s, e) => ClearWorkspace();

            pnlCard.Controls.Add(btnAdd); pnlCard.Controls.Add(btnUpdate); pnlCard.Controls.Add(btnDelete); pnlCard.Controls.Add(btnReset);

            // =========================================================
            // 【右側】數據表格與搜尋 (Displaying Data)
            // =========================================================
            Label lblGridTitle = new Label { Text = "📂 Customer Records", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(430, 50), AutoSize = true };
            pnlMain.Controls.Add(lblGridTitle);

            txtSearch = new TextBox { Location = new Point(850, 50), Size = new Size(240, 25), Font = new Font("Segoe UI", 10F) }; // 向右靠
            txtSearch.Text = "Search Name / Phone...";
            txtSearch.ForeColor = Color.Gray;
            txtSearch.GotFocus += (s, e) => { if (txtSearch.Text == "Search Name / Phone...") { txtSearch.Text = ""; txtSearch.ForeColor = Color.Black; } };
            txtSearch.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) { txtSearch.Text = "Search Name / Phone..."; txtSearch.ForeColor = Color.Gray; } };
            txtSearch.TextChanged += txtSearch_TextChanged;
            pnlMain.Controls.Add(txtSearch);

            // 數據網格
            dgvCustomer = new DataGridView
            {
                Location = new Point(430, 90),
                Size = new Size(660, 610), // ⬅️ 擴闊表格，用盡右側空間
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,

                // 🛡️ 核心修復 2：強制資料欄自動填滿表格寬度，解決資料截斷問題
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
            };

            // 允許文字自動換行
            dgvCustomer.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            dgvCustomer.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(79, 70, 229);
            dgvCustomer.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCustomer.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvCustomer.ColumnHeadersHeight = 38;
            dgvCustomer.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvCustomer.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);

            dgvCustomer.CellClick += dgvCustomer_CellClick;
            pnlMain.Controls.Add(dgvCustomer);
        }

        private TextBox CreateStyledTextBox(Panel container, ref int topY, string labelText, bool readOnly)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(20, topY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(20, topY + 22), Width = 335, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            if (readOnly) { txt.ReadOnly = true; txt.BackColor = Color.FromArgb(241, 245, 249); }
            container.Controls.Add(lbl); container.Controls.Add(txt);
            topY += 65;
            return txt;
        }

        private void SetupTypeComboBox()
        {
            cboType.Items.Clear();
            cboType.Items.Add("B2B (Corporate)");
            cboType.Items.Add("B2C (Retail)");
            if (cboType.Items.Count > 0) cboType.SelectedIndex = 0;
        }
        #endregion

        #region 💾 資料庫核心商業邏輯
        private void LoadCustomerData(string filter = "")
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    string query = "SELECT CustomerID, Name, Type, Phone, Address FROM customer WHERE 1=1";
                    if (!string.IsNullOrEmpty(filter) && filter != "Search Name / Phone...")
                    {
                        query += " AND (CustomerID LIKE @filter OR Name LIKE @filter OR Phone LIKE @filter)";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(filter) && filter != "Search Name / Phone...")
                            cmd.Parameters.AddWithValue("@filter", "%" + filter + "%");

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dgvCustomer.DataSource = dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load customer list:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateNextCustomerID()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    string query = "SELECT CustomerID FROM customer ORDER BY CustomerID DESC LIMIT 1";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            string lastID = result.ToString();
                            if (lastID.StartsWith("C") && int.TryParse(lastID.Substring(1), out int num))
                            {
                                txtCustomerID.Text = "C" + (num + 1).ToString("D3");
                                return;
                            }
                        }
                        txtCustomerID.Text = "C001";
                    }
                }
            }
            catch (Exception)
            {
                txtCustomerID.Text = "C001";
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text.Trim()) || string.IsNullOrEmpty(txtPhone.Text.Trim()))
            {
                MessageBox.Show("Mandatory validation fault:\nClient Name and Phone fields cannot be blank.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    string query = "INSERT INTO customer (CustomerID, Name, Type, Phone, Address) VALUES (@id, @name, @type, @phone, @address)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtCustomerID.Text.Trim());
                        cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@type", cboType.Text);
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@address", txtAddress.Text.Trim());

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("New client organization synchronized successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearWorkspace();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Transaction execution aborted:\n" + ex.Message, "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                MessageBox.Show("Client Name is required for core updates.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    string query = "UPDATE customer SET Name = @name, Type = @type, Phone = @phone, Address = @address WHERE CustomerID = @id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtCustomerID.Text.Trim());
                        cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@type", cboType.Text);
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@address", txtAddress.Text.Trim());

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            MessageBox.Show("Client profile alterations updated globally.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearWorkspace();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update execution rejected:\n" + ex.Message, "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCustomerID.Text)) return;

            DialogResult confirm = MessageBox.Show($"Are you sure you want to permanently delete client {txtCustomerID.Text}?\nThis may violate data links if they have active orders.", "Critical Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connString))
                    {
                        conn.Open();
                        string query = "DELETE FROM customer WHERE CustomerID = @id";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", txtCustomerID.Text.Trim());
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Client master records purged from the database maps.", "Purge Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearWorkspace();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Purge refused due to relational integrity constraint (Active orders exist).\nDetails: " + ex.Message, "Integrity Check Blocked", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvCustomer_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvCustomer.Rows[e.RowIndex];
                txtCustomerID.Text = row.Cells["CustomerID"].Value?.ToString();
                txtName.Text = row.Cells["Name"].Value?.ToString();

                string typeVal = row.Cells["Type"].Value?.ToString();
                if (cboType.Items.Contains(typeVal)) cboType.SelectedItem = typeVal;

                txtPhone.Text = row.Cells["Phone"].Value?.ToString();
                txtAddress.Text = row.Cells["Address"].Value?.ToString();

                // 編輯模式下，鎖定 Add 按鈕並轉為灰色
                btnAdd.Enabled = false;
                btnAdd.BackColor = Color.FromArgb(203, 213, 225);
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (txtSearch.Text != "Search Name / Phone...")
            {
                LoadCustomerData(txtSearch.Text.Trim());
            }
        }

        private void ClearWorkspace()
        {
            txtName.Clear();
            txtPhone.Clear();
            txtAddress.Clear();
            txtSearch.Clear();
            if (cboType.Items.Count > 0) cboType.SelectedIndex = 0;

            // 恢復 Add 按鈕為翠綠色
            btnAdd.Enabled = true;
            btnAdd.BackColor = Color.FromArgb(16, 185, 129);

            LoadCustomerData();
            GenerateNextCustomerID();
        }
        #endregion
    }
}