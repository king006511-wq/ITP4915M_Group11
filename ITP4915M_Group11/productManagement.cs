using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ITP4915M_Group11
{
    public partial class ProductManagement : Form
    {
        // 變數名稱更新：對應全新 Product 表
        private TextBox txtProductID;
        private TextBox txtProductName;
        private TextBox txtDescription;
        private TextBox txtStockLevel;
        private TextBox txtRetailPrice;
        private DataGridView dgvProductCatalog;

        // 🔒 Centralized Database Connection String (UNTOUCHED)
        private readonly string connectionString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        public ProductManagement()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                InitializePremiumModernUI();
                LoadDatabaseData();
            }
        }

        #region 🔒 System Security Gatekeeper Enforcement
        private void ProductManagement_Load(object sender, EventArgs e)
        {
            // 🎯 Check if the user has Manager or Administrator permissions
            string currentRole = UserSession.LoggedInStaffRole;
            string currentStaffID = UserSession.LoggedInStaffID;

            bool isAuthorized = !string.IsNullOrEmpty(currentRole) &&
                                (currentRole.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                                 currentRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase));

            if (!isAuthorized)
            {
                MessageBox.Show(
                    $"[SECURITY ALERT] Access Denied!\n\n" +
                    $"Logged In Staff ID: {(string.IsNullOrEmpty(currentStaffID) ? "Unknown" : currentStaffID)}\n" +
                    $"Your Account Role is: \"{(string.IsNullOrEmpty(currentRole) ? "None" : currentRole)}\"\n\n" +
                    $"Only a Manager or Administrator is authorized to access Product Maintenance settings.",
                    "System Security Guard",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop
                );

                this.Shown += (s2, e2) => this.Close();
                return;
            }

            bool canEdit = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator);
            foreach (Control c in this.Controls)
            {
                if (c is Button b && (b.Text.Contains("Add") || b.Text.Contains("Update") || b.Text.Contains("Delete")))
                {
                    b.Enabled = canEdit;
                    b.BackColor = canEdit ? b.BackColor : Color.LightGray;
                }
            }
        }
        #endregion

        #region 🎨 Premium Unified Modern UI Construction Engine
        private void InitializePremiumModernUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Product Maintenance & Catalog Control";
            this.Size = new Size(1180, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            this.Load += ProductManagement_Load;

            // Workspace Controller Panel Area
            Panel pnlMain = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label { Text = "Finished Goods Inventory Maintenance", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(30, 20), AutoSize = true };
            pnlMain.Controls.Add(lblHeader);

            Button btnBackHome = new Button { Text = "🔙 Go Back", Size = new Size(120, 34), Location = new Point(1015, 22), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand, Visible = true };
            btnBackHome.FlatAppearance.BorderSize = 0;
            btnBackHome.Click += (s, e) => { try { this.Close(); } catch { this.Hide(); } };
            pnlMain.Controls.Add(btnBackHome);

            // Input Details Dashboard Card
            Panel pnlCard = new Panel { Location = new Point(30, 85), Size = new Size(380, 600), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlCard);

            Label lblCardTitle = new Label { Text = "📦 Finished Product Details", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(20, 15), AutoSize = true };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 60;
            txtProductID = CreateStyledTextBox(pnlCard, ref startY, "Product ID *:", false);
            txtProductName = CreateStyledTextBox(pnlCard, ref startY, "Product Name *:", false);
            txtStockLevel = CreateStyledTextBox(pnlCard, ref startY, "Stock Level:", false);
            txtRetailPrice = CreateStyledTextBox(pnlCard, ref startY, "Retail Price (HKD):", false);

            Label lblDesc = new Label { Text = "🔍 Live Form Search Filter keyword:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235) };
            txtDescription = new TextBox { Location = new Point(20, startY + 22), Width = 335, Height = 70, Multiline = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            txtDescription.TextChanged += txtDescription_TextChanged;
            pnlCard.Controls.Add(lblDesc);
            pnlCard.Controls.Add(txtDescription);
            startY += 105;

            // Action Buttons
            Button btnAdd = new Button { Text = "➕ Add", Location = new Point(20, startY), Size = new Size(160, 42), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            Button btnUpdate = new Button { Text = "💾 Update", Location = new Point(195, startY), Size = new Size(160, 42), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            Button btnDelete = new Button { Text = "🗑️ Delete", Location = new Point(20, startY + 50), Size = new Size(160, 42), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            Button btnClear = new Button { Text = "🧹 Clear Forms", Location = new Point(195, startY + 50), Size = new Size(160, 42), BackColor = Color.FromArgb(71, 85, 105), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };

            foreach (var b in new Button[] { btnAdd, btnUpdate, btnDelete, btnClear }) b.FlatAppearance.BorderSize = 0;
            pnlCard.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });

            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnClear.Click += (s, e) => ClearFields();

            // 4. Data Grid Component (拉闊填滿右側剩餘空白)
            Label lblGridTitle = new Label { Text = "📋 Real-Time Product Catalog Records", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(440, 85), AutoSize = true };
            pnlMain.Controls.Add(lblGridTitle);

            dgvProductCatalog = new DataGridView
            {
                Location = new Point(440, 125),
                Size = new Size(700, 560), // 🚀 將表格寬度從 430 加闊至 700 填滿畫面
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvProductCatalog.EnableHeadersVisualStyles = false;
            dgvProductCatalog.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvProductCatalog.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProductCatalog.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvProductCatalog.ColumnHeadersHeight = 38;
            dgvProductCatalog.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvProductCatalog.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);

            dgvProductCatalog.SelectionChanged += dgvProductCatalog_SelectionChanged;
            pnlMain.Controls.Add(dgvProductCatalog);
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
        #endregion

        #region 📦 Business Management Logic Functions (已更新至全新 product 表)
        private void LoadDatabaseData()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // 🚀 從全新的 product 表讀取資料
                    string query = "SELECT ProductID AS 'Product ID', ProductName AS 'Name', StockLevel AS 'Stock', RetailPrice AS 'Price HKD' FROM product";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvProductCatalog.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                DataTable errorDt = new DataTable();
                errorDt.Columns.Add("System Status");
                errorDt.Rows.Add("Database Error: " + ex.Message);
                dgvProductCatalog.DataSource = errorDt;
            }
        }

        private void txtDescription_TextChanged(object sender, EventArgs e)
        {
            if (dgvProductCatalog.DataSource is DataTable dt)
            {
                string keyword = txtDescription.Text.Trim().Replace("'", "''");
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    dt.DefaultView.RowFilter = "";
                }
                else
                {
                    dt.DefaultView.RowFilter = string.Format("[Product ID] LIKE '%{0}%' OR [Name] LIKE '%{0}%'", keyword);
                }
            }
        }

        private void dgvProductCatalog_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProductCatalog.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvProductCatalog.SelectedRows[0];
                txtProductID.Text = row.Cells["Product ID"].Value?.ToString() ?? "";
                txtProductName.Text = row.Cells["Name"].Value?.ToString() ?? "";
                txtStockLevel.Text = row.Cells["Stock"].Value?.ToString() ?? "";
                txtRetailPrice.Text = row.Cells["Price HKD"].Value?.ToString() ?? "";
                txtProductID.ReadOnly = true;
                txtProductID.BackColor = Color.FromArgb(241, 245, 249);
            }
            else
            {
                ClearFields();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductID.Text) || string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Please fill in Product ID and Product Name fields!", "Missing Required Fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // 🚀 寫入全新的 product 表
                    string query = "INSERT INTO product (ProductID, ProductName, StockLevel, RetailPrice) VALUES (@id, @name, @stock, @price)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtProductID.Text.Trim());
                        cmd.Parameters.AddWithValue("@name", txtProductName.Text.Trim());
                        cmd.Parameters.AddWithValue("@stock", string.IsNullOrEmpty(txtStockLevel.Text) ? 0 : Convert.ToInt32(txtStockLevel.Text.Trim()));
                        cmd.Parameters.AddWithValue("@price", string.IsNullOrEmpty(txtRetailPrice.Text) ? 0 : Convert.ToDecimal(txtRetailPrice.Text.Trim()));
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Product was added successfully!", "Transaction Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDatabaseData();
                    ClearFields();
                }
            }
            catch (Exception ex) { MessageBox.Show("Database Failure: \n" + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductID.Text))
            {
                MessageBox.Show("Please click on an active catalog item to initiate modification cycles.", "System Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // 🚀 更新全新的 product 表
                    string query = "UPDATE product SET ProductName=@name, StockLevel=@stock, RetailPrice=@price WHERE ProductID=@id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtProductID.Text.Trim());
                        cmd.Parameters.AddWithValue("@name", txtProductName.Text.Trim());
                        cmd.Parameters.AddWithValue("@stock", string.IsNullOrEmpty(txtStockLevel.Text) ? 0 : Convert.ToInt32(txtStockLevel.Text.Trim()));
                        cmd.Parameters.AddWithValue("@price", string.IsNullOrEmpty(txtRetailPrice.Text) ? 0 : Convert.ToDecimal(txtRetailPrice.Text.Trim()));

                        int rowCount = cmd.ExecuteNonQuery();
                        if (rowCount > 0)
                        {
                            MessageBox.Show("Inventory product info has updated correctly!", "Transaction Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadDatabaseData();
                            ClearFields();
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Update Operation Failure:\n" + ex.Message, "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductID.Text))
            {
                MessageBox.Show("Select an active catalog item from the history panel to drop.", "Deletion Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult confirm = MessageBox.Show($"Are you sure you want to permanently erase catalog item record ID [{txtProductID.Text}]?", "Confirm Erase Sequence", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.No) return;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // 🚀 從全新的 product 表刪除
                    string query = "DELETE FROM product WHERE ProductID=@id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtProductID.Text.Trim());
                        int dynamicRows = cmd.ExecuteNonQuery();
                        if (dynamicRows > 0)
                        {
                            MessageBox.Show("Product was dropped successfully!", "Record Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadDatabaseData();
                            ClearFields();
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Critical deletion database block caught: " + ex.Message, "Error Processing Operation", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void ClearFields()
        {
            txtProductID.Clear();
            txtProductName.Clear();
            txtStockLevel.Clear();
            txtRetailPrice.Clear();
            txtDescription.Clear();
            txtProductID.ReadOnly = false;
            txtProductID.BackColor = Color.White;
            dgvProductCatalog.ClearSelection();
        }
        #endregion
    }
}