using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class CustomerManagement : BaseForm
    {
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        private TextBox txtCustomerID, txtName, txtPhone, txtAddress, txtSearch;
        private ComboBox cboType;
        private DataGridView dgvCustomers, dgvRecentOrders;
        private Label lblTotalOrders, lblTotalSpent;

        public CustomerManagement()
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                InitializeEnterpriseCRM_UI();
                LoadData();
                GenerateCustomerID();
            }
        }

        #region 🎨 企業級 CRM 現代化介面構建
        private void InitializeEnterpriseCRM_UI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living - Enterprise CRM & Customer 360";
            this.Size = new Size(1180, 750);
            this.BackColor = ThemeManager.PrimaryBackground;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Font = ThemeManager.DefaultFont;

            AuthorizationHelper.EnforceRole(this, "Manager", "Administrator", "Sales Representative", "Sales", "Customer Support");

            // --- Header ---
            Label lblHeader = new Label { Text = "👥 Enterprise CRM & Customer 360", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(30, 20), AutoSize = true };
            this.Controls.Add(lblHeader);

            // --- Left Panel: Customer Profile & Metrics ---
            Panel pnlCard = new Panel { Location = new Point(30, 85), Size = new Size(380, 620), BackColor = ThemeManager.CardBackground, BorderStyle = BorderStyle.FixedSingle };
            pnlCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlCard.ClientRectangle, ThemeManager.BorderColor, ButtonBorderStyle.Solid);
            this.Controls.Add(pnlCard);

            Label lblCardTitle = new Label { Text = "📝 Customer Profile", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(20, 15), AutoSize = true };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 55;
            txtCustomerID = CreateInput(pnlCard, ref startY, "Customer ID (Auto):", true);
            txtName = CreateInput(pnlCard, ref startY, "Customer Name *:", false);

            Label lblType = new Label { Text = "Account Type *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboType = new ComboBox { Location = new Point(20, startY + 22), Width = 335, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F) };
            cboType.Items.AddRange(new string[] { "B2B (Corporate)", "B2C (Retail)" });
            pnlCard.Controls.Add(lblType); pnlCard.Controls.Add(cboType);
            startY += 65;

            txtPhone = CreateInput(pnlCard, ref startY, "Contact Number:", false);
            txtAddress = CreateInput(pnlCard, ref startY, "Billing/Delivery Address:", false);

            // 🌟 企業級 Metrics (Customer Lifetime Value)
            Panel pnlMetrics = new Panel { Location = new Point(20, startY), Size = new Size(335, 75), BackColor = Color.FromArgb(241, 245, 249), BorderStyle = BorderStyle.None };
            lblTotalOrders = new Label { Text = "Lifetime Orders: 0", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(10, 15), AutoSize = true };
            lblTotalSpent = new Label { Text = "Total Spent: $0.00", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(16, 185, 129), Location = new Point(10, 40), AutoSize = true };
            pnlMetrics.Controls.Add(lblTotalOrders); pnlMetrics.Controls.Add(lblTotalSpent);
            pnlCard.Controls.Add(pnlMetrics);
            startY += 95;

            // Buttons
            Button btnAdd = new Button { Text = "➕ Create", Location = new Point(20, startY), Size = new Size(160, 40), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            Button btnUpdate = new Button { Text = "💾 Update", Location = new Point(195, startY), Size = new Size(160, 40), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            Button btnDelete = new Button { Text = "🗑️ Delete", Location = new Point(20, startY + 50), Size = new Size(160, 40), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            Button btnClear = new Button { Text = "🧹 Clear", Location = new Point(195, startY + 50), Size = new Size(160, 40), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };

            foreach (Button b in new Button[] { btnAdd, btnUpdate, btnDelete, btnClear }) b.FlatAppearance.BorderSize = 0;

            btnAdd.Click += BtnAdd_Click; btnUpdate.Click += BtnUpdate_Click; btnDelete.Click += BtnDelete_Click; btnClear.Click += (s, e) => ClearForm();
            pnlCard.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });

            // --- Right Panel: Master-Detail Grid ---
            txtSearch = new TextBox { Location = new Point(440, 85), Width = 350, Font = new Font("Segoe UI", 11F), BorderStyle = BorderStyle.FixedSingle, ForeColor = Color.Gray, Text = "Search by ID, Name or Phone..." };
            txtSearch.GotFocus += (s, e) => { if (txtSearch.Text == "Search by ID, Name or Phone...") { txtSearch.Text = ""; txtSearch.ForeColor = Color.Black; } };
            txtSearch.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) { txtSearch.Text = "Search by ID, Name or Phone..."; txtSearch.ForeColor = Color.Gray; } };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            this.Controls.Add(txtSearch);

            // Customers Grid
            dgvCustomers = new DataGridView { Location = new Point(440, 125), Size = new Size(700, 320), BackgroundColor = Color.White, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BorderStyle = BorderStyle.None };
            dgvCustomers.EnableHeadersVisualStyles = false;
            dgvCustomers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvCustomers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCustomers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvCustomers.ColumnHeadersHeight = 35;
            dgvCustomers.SelectionChanged += DgvCustomers_SelectionChanged;
            this.Controls.Add(dgvCustomers);

            // 🌟 Recent Orders Grid (Customer 360 Support View)
            Label lblOrders = new Label { Text = "📦 Selected Customer's Recent Orders", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(440, 460), AutoSize = true };
            this.Controls.Add(lblOrders);

            dgvRecentOrders = new DataGridView { Location = new Point(440, 495), Size = new Size(700, 210), BackgroundColor = Color.White, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BorderStyle = BorderStyle.None };
            dgvRecentOrders.EnableHeadersVisualStyles = false;
            dgvRecentOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(14, 165, 233); // 淺藍色區分
            dgvRecentOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRecentOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvRecentOrders.ColumnHeadersHeight = 30;
            this.Controls.Add(dgvRecentOrders);
        }

        private TextBox CreateInput(Panel container, ref int y, string label, bool readOnly)
        {
            Label lbl = new Label { Text = label, Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(20, y + 22), Width = 335, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            if (readOnly) { txt.ReadOnly = true; txt.BackColor = Color.FromArgb(241, 245, 249); }
            container.Controls.Add(lbl); container.Controls.Add(txt);
            y += 65; return txt;
        }
        #endregion

        #region 💾 資料庫邏輯與智能生成
        private void GenerateCustomerID()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT CustomerID FROM customer ORDER BY CustomerID DESC LIMIT 1";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            string lastID = result.ToString();
                            string seqStr = lastID.Replace("C", "");
                            if (int.TryParse(seqStr, out int seq))
                            {
                                txtCustomerID.Text = "C" + (seq + 1).ToString("D3");
                                return;
                            }
                        }
                    }
                }
                catch (Exception) { }
            }
            txtCustomerID.Text = "C001";
        }

        private void LoadData()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlDataAdapter da = new MySqlDataAdapter("SELECT CustomerID, Name, Type, Phone, Address FROM customer ORDER BY CustomerID ASC", conn))
                    {
                        DataTable dt = new DataTable(); da.Fill(dt);
                        dgvCustomers.DataSource = dt;
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error loading data: " + ex.Message); }
            }
        }

        // 🌟 當選擇客戶時，自動拉取該客戶嘅 360 度訂單資訊
        private void DgvCustomers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count > 0)
            {
                DataGridViewRow r = dgvCustomers.SelectedRows[0];
                string cid = r.Cells["CustomerID"].Value.ToString();

                txtCustomerID.Text = cid;
                txtName.Text = r.Cells["Name"].Value.ToString();
                cboType.Text = r.Cells["Type"].Value.ToString() == "B2B" ? "B2B (Corporate)" : (r.Cells["Type"].Value.ToString() == "B2C" ? "B2C (Retail)" : r.Cells["Type"].Value.ToString());
                txtPhone.Text = r.Cells["Phone"].Value.ToString();
                txtAddress.Text = r.Cells["Address"].Value.ToString();

                LoadCustomer360Data(cid);
            }
        }

        private void CustomerManagement_Load(object sender, EventArgs e)
        {

        }

        private void LoadCustomer360Data(string customerID)
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // 1. 抓取客戶總訂單量與總消費金額
                    string metricsQuery = "SELECT COUNT(OrderID) AS TotalCount, IFNULL(SUM(TotalAmount), 0) AS TotalSpent FROM orders WHERE CustomerID = @CID AND Status NOT IN ('Rejected', 'Cancelled')";
                    using (MySqlCommand cmd = new MySqlCommand(metricsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@CID", customerID);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lblTotalOrders.Text = $"Lifetime Orders: {reader["TotalCount"]}";
                                lblTotalSpent.Text = $"Total Spent: $ {Convert.ToDecimal(reader["TotalSpent"]):N2}";
                            }
                        }
                    }

                    // 2. 抓取客戶歷史訂單列表
                    string ordersQuery = "SELECT OrderID AS 'Order Ref', OrderDate AS 'Date', TotalAmount AS 'Amount', Status FROM orders WHERE CustomerID = @CID ORDER BY OrderDate DESC LIMIT 10";
                    using (MySqlDataAdapter da = new MySqlDataAdapter(ordersQuery, conn))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@CID", customerID);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvRecentOrders.DataSource = dt;
                    }
                }
                catch (Exception) { dgvRecentOrders.DataSource = null; }
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (dgvCustomers.DataSource is DataTable dt && txtSearch.Text != "Search by ID, Name or Phone...")
            {
                string kw = txtSearch.Text.Trim().Replace("'", "''");
                if (string.IsNullOrWhiteSpace(kw)) dt.DefaultView.RowFilter = "";
                else dt.DefaultView.RowFilter = $"CustomerID LIKE '%{kw}%' OR Name LIKE '%{kw}%' OR Phone LIKE '%{kw}%'";
            }
        }
        #endregion

        #region 📝 CRUD Operations
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCustomerID.Text) || string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("ID and Name are required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            string dbType = cboType.Text.Contains("B2B") ? "B2B" : "B2C";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO customer (CustomerID, Name, Type, Phone, Address) VALUES (@id, @n, @t, @p, @a)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtCustomerID.Text.Trim());
                        cmd.Parameters.AddWithValue("@n", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@t", dbType);
                        cmd.Parameters.AddWithValue("@p", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@a", txtAddress.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("New customer profile created successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData(); ClearForm();
                }
                catch (Exception ex) { MessageBox.Show("Database Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCustomerID.Text)) return;
            string dbType = cboType.Text.Contains("B2B") ? "B2B" : "B2C";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string sql = "UPDATE customer SET Name=@n, Type=@t, Phone=@p, Address=@a WHERE CustomerID=@id";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtCustomerID.Text.Trim());
                        cmd.Parameters.AddWithValue("@n", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@t", dbType);
                        cmd.Parameters.AddWithValue("@p", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@a", txtAddress.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Customer profile updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                }
                catch (Exception ex) { MessageBox.Show("Database Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCustomerID.Text)) return;
            if (MessageBox.Show($"Are you sure you want to permanently delete customer [{txtCustomerID.Text}]?", "Confirm Erase", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("DELETE FROM customer WHERE CustomerID=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", txtCustomerID.Text.Trim());
                            cmd.ExecuteNonQuery();
                        }
                        LoadData(); ClearForm();
                    }
                    catch (Exception) { MessageBox.Show("Cannot delete customer. They have existing sales orders linked to their account.", "Foreign Key Constraint", MessageBoxButtons.OK, MessageBoxIcon.Stop); }
                }
            }
        }

        private void ClearForm()
        {
            txtName.Clear(); txtPhone.Clear(); txtAddress.Clear(); cboType.SelectedIndex = -1;
            lblTotalOrders.Text = "Lifetime Orders: 0";
            lblTotalSpent.Text = "Total Spent: $0.00";
            dgvRecentOrders.DataSource = null;
            dgvCustomers.ClearSelection();
            GenerateCustomerID(); // 自動生成新 ID
        }
        #endregion
    }
}