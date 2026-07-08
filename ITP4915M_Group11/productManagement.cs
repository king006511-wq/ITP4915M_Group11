using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ITP4915M_Group11
{
    public partial class ProductManagement : BaseForm
    {
        private TextBox txtProductID, txtProductName, txtRetailPrice, txtSearch;
        private Label lblRetailPrice;

        // 🌍 各地區獨立庫存 TextBoxes
        private TextBox txtStock_HK, txtStock_Tokyo, txtStock_Singapore, txtStock_NY, txtStock_London;
        private Label lblStock_HK, lblStock_Tokyo, lblStock_Singapore, lblStock_NY, lblStock_London;

        // 🌟 新增：專屬警界線 Textbox
        private TextBox txtAlertLimit;

        // 🌟 折扣與還原按鈕
        private TextBox txtDiscount;
        private Button btnApplyDiscount, btnRestorePrice;

        private DataGridView dgvProductCatalog;

        private Button btnBackHome, btnViewPhoto, btnUploadPhoto, btnUpdate, btnDelete, btnClear;

        // 🔒 Centralized Database Connection String
        private readonly string connectionString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // 👨‍💼 權限與地區變數
        private bool isAdminOrManager = false;
        private string staffRegion = "Hong Kong"; // 預設

        public ProductManagement()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                EnsureProductSchema(); // 🌟 啟動時自動升級 Database，加入 OriginalPrice 同 AlertThreshold
                ThemeManager.ApplyTheme(this);
                DetermineUserAccessLevel();
                InitializePremiumModernUI();
                ApplyButtonColors();
                LoadDatabaseData();

                this.VisibleChanged += (s, e) => {
                    if (this.Visible) LoadDatabaseData();
                };
            }
        }

        // 🌟 自動為 product 表加入原價與警界線欄位，防呆機制
        private void EnsureProductSchema()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    try { new MySqlCommand("ALTER TABLE product ADD COLUMN OriginalPrice DECIMAL(10,2) DEFAULT 0.00;", conn).ExecuteNonQuery(); } catch { }
                    try { new MySqlCommand("ALTER TABLE product ADD COLUMN AlertThreshold INT DEFAULT 20;", conn).ExecuteNonQuery(); } catch { }

                    // 自動將未設定過嘅原價初始化為目前的 RetailPrice
                    try { new MySqlCommand("UPDATE product SET OriginalPrice = RetailPrice WHERE OriginalPrice = 0.00 OR OriginalPrice IS NULL;", conn).ExecuteNonQuery(); } catch { }
                }
                catch { }
            }
        }

        #region 🔒 System Security & Role Management
        private void DetermineUserAccessLevel()
        {
            string currentRole = UserSession.LoggedInStaffRole;
            isAdminOrManager = !string.IsNullOrEmpty(currentRole) &&
                               (currentRole.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                                currentRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase));

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT Region FROM staff WHERE StaffID = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", UserSession.LoggedInStaffID ?? "");
                        object res = cmd.ExecuteScalar();
                        if (res != null && res != DBNull.Value) staffRegion = res.ToString();
                    }
                }
                catch { }
            }
        }

        private void ProductManagement_Load(object sender, EventArgs e)
        {
            string currentRole = UserSession.LoggedInStaffRole;
            bool isAuthorized = isAdminOrManager || (!string.IsNullOrEmpty(currentRole) && currentRole.Equals("Warehouse Specialist", StringComparison.OrdinalIgnoreCase));

            if (!isAuthorized)
            {
                MessageBox.Show($"[SECURITY ALERT] Access Denied!\n\nYour Role is not authorized to manage Inventory.", "System Security Guard", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.Shown += (s2, e2) => this.Close();
                return;
            }

            foreach (Control c in this.Controls)
            {
                if (c is Button b && (b.Text.Contains("Update") || b.Text.Contains("Delete") || b.Text.Contains("Upload")))
                {
                    b.Enabled = isAdminOrManager || currentRole.Equals("Warehouse Specialist", StringComparison.OrdinalIgnoreCase);
                    b.BackColor = b.Enabled ? b.BackColor : Color.LightGray;
                }
            }

            if (btnApplyDiscount != null)
            {
                btnApplyDiscount.Enabled = isAdminOrManager;
                btnApplyDiscount.BackColor = isAdminOrManager ? Color.FromArgb(16, 185, 129) : Color.LightGray;
                btnRestorePrice.Enabled = isAdminOrManager;
                btnRestorePrice.BackColor = isAdminOrManager ? Color.FromArgb(37, 99, 235) : Color.LightGray;
            }
        }
        #endregion

        #region 🎨 Premium Unified Modern UI Construction Engine
        private void InitializePremiumModernUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Product Maintenance & Catalog Control";
            this.Size = new Size(1280, 880);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            this.Load += ProductManagement_Load;

            Panel pnlMain = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label { Text = "Finished Goods Inventory Maintenance", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(30, 20), AutoSize = true };
            pnlMain.Controls.Add(lblHeader);

            btnBackHome = new Button { Text = "🔙 Go Back", Size = new Size(120, 34), Location = new Point(1115, 22), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnBackHome.FlatAppearance.BorderSize = 0;
            btnBackHome.Click += (s, e) => { try { this.Close(); } catch { this.Hide(); } };
            pnlMain.Controls.Add(btnBackHome);

            Panel pnlCard = new Panel { Location = new Point(30, 85), Size = new Size(460, 740), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, AutoScroll = true }; // 🌟 開啟自動捲動
            pnlCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlCard);

            Label lblCardTitle = new Label { Text = isAdminOrManager ? "📦 Global Product Details" : $"📦 Product Details ({staffRegion})", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(79, 70, 229), Location = new Point(25, 15), AutoSize = true };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 60;
            txtProductID = CreateStyledTextBox(pnlCard, ref startY, "Product ID *:", false, out _);
            txtProductName = CreateStyledTextBox(pnlCard, ref startY, "Product Name *:", false, out _);

            txtStock_HK = CreateStyledTextBox(pnlCard, ref startY, "Hong Kong:", false, out lblStock_HK);
            txtStock_Tokyo = CreateStyledTextBox(pnlCard, ref startY, "Tokyo:", false, out lblStock_Tokyo);
            txtStock_Singapore = CreateStyledTextBox(pnlCard, ref startY, "Singapore:", false, out lblStock_Singapore);
            txtStock_NY = CreateStyledTextBox(pnlCard, ref startY, "New York:", false, out lblStock_NY);
            txtStock_London = CreateStyledTextBox(pnlCard, ref startY, "London:", false, out lblStock_London);

            txtRetailPrice = CreateStyledTextBox(pnlCard, ref startY, "Current Retail Price (HKD):", false, out lblRetailPrice);

            // 🌟 獨立專屬警界線輸入框
            txtAlertLimit = CreateStyledTextBox(pnlCard, ref startY, "🚨 Stock Alert Threshold (Min 20):", false, out _);

            ArrangeDynamicUI(pnlCard);

            // ============================================
            // 🌟 右方面板：大量折扣與還原控制區
            // ============================================
            Panel pnlRightTop = new Panel { Location = new Point(510, 85), Size = new Size(730, 100), BackColor = Color.Transparent };
            pnlMain.Controls.Add(pnlRightTop);

            Label lblGridTitle = new Label { Text = "📋 Real-Time Product Catalog Records", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(0, 0), AutoSize = true };
            pnlRightTop.Controls.Add(lblGridTitle);

            Label lblSearch = new Label { Text = "🔍 Search:", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(79, 70, 229), Location = new Point(380, 3), AutoSize = true };
            pnlRightTop.Controls.Add(lblSearch);

            txtSearch = new TextBox { Location = new Point(460, 0), Width = 270, Font = new Font("Segoe UI", 10F), BorderStyle = BorderStyle.FixedSingle };
            txtSearch.TextChanged += txtSearch_TextChanged;
            pnlRightTop.Controls.Add(txtSearch);

            Label lblWarningLegend = new Label { Text = "🚨 Alert: Rows in RED indicate Low Stock (Below the item's custom Alert Limit)", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(220, 38, 38), Location = new Point(0, 35), AutoSize = true };
            pnlRightTop.Controls.Add(lblWarningLegend);

            // 折扣輸入與按鈕
            Label lblDisc = new Label { Text = "🏷️ Batch Discount (%):", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(16, 185, 129), Location = new Point(0, 70), AutoSize = true };
            pnlRightTop.Controls.Add(lblDisc);

            txtDiscount = new TextBox { Location = new Point(160, 67), Width = 50, Font = new Font("Segoe UI", 10F), BorderStyle = BorderStyle.FixedSingle, Text = "10" };
            pnlRightTop.Controls.Add(txtDiscount);

            btnApplyDiscount = new Button { Text = "Apply % Discount", Location = new Point(220, 65), Size = new Size(150, 30), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnApplyDiscount.FlatAppearance.BorderSize = 0;
            btnApplyDiscount.Click += BtnApplyDiscount_Click;
            pnlRightTop.Controls.Add(btnApplyDiscount);

            // 🌟 還原原價按鈕
            btnRestorePrice = new Button { Text = "🔄 Revert to Original Price", Location = new Point(380, 65), Size = new Size(200, 30), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnRestorePrice.FlatAppearance.BorderSize = 0;
            btnRestorePrice.Click += BtnRestorePrice_Click;
            pnlRightTop.Controls.Add(btnRestorePrice);

            dgvProductCatalog = new DataGridView
            {
                Location = new Point(510, 195),
                Size = new Size(730, 630),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvProductCatalog.EnableHeadersVisualStyles = false;
            dgvProductCatalog.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(79, 70, 229);
            dgvProductCatalog.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProductCatalog.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvProductCatalog.ColumnHeadersHeight = 38;
            dgvProductCatalog.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvProductCatalog.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);

            dgvProductCatalog.SelectionChanged += dgvProductCatalog_SelectionChanged;
            dgvProductCatalog.CellFormatting += dgvProductCatalog_CellFormatting;

            ThemeManager.StyleDataGrid(dgvProductCatalog);
            pnlMain.Controls.Add(dgvProductCatalog);
        }

        private TextBox CreateStyledTextBox(Panel container, ref int topY, string labelText, bool readOnly, out Label createdLabel)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(25, topY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(25, topY + 25), Width = 390, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            if (readOnly) { txt.ReadOnly = true; txt.BackColor = Color.FromArgb(241, 245, 249); }
            container.Controls.Add(lbl); container.Controls.Add(txt);
            topY += 65;
            createdLabel = lbl;
            return txt;
        }

        private void ArrangeDynamicUI(Panel pnlCard)
        {
            if (!isAdminOrManager)
            {
                txtStock_HK.Visible = lblStock_HK.Visible = false;
                txtStock_Tokyo.Visible = lblStock_Tokyo.Visible = false;
                txtStock_Singapore.Visible = lblStock_Singapore.Visible = false;
                txtStock_NY.Visible = lblStock_NY.Visible = false;
                txtStock_London.Visible = lblStock_London.Visible = false;

                TextBox activeTxt = null; Label activeLbl = null;
                switch (staffRegion)
                {
                    case "Tokyo": activeTxt = txtStock_Tokyo; activeLbl = lblStock_Tokyo; break;
                    case "Singapore": activeTxt = txtStock_Singapore; activeLbl = lblStock_Singapore; break;
                    case "New York": activeTxt = txtStock_NY; activeLbl = lblStock_NY; break;
                    case "London": activeTxt = txtStock_London; activeLbl = lblStock_London; break;
                    default: activeTxt = txtStock_HK; activeLbl = lblStock_HK; break;
                }

                activeTxt.Visible = activeLbl.Visible = true;
                activeLbl.Location = new Point(25, 190);
                activeTxt.Location = new Point(25, 215);

                lblRetailPrice.Location = new Point(25, 255);
                txtRetailPrice.Location = new Point(25, 280);

                // 警界線輸入框推上少少
                pnlCard.Controls.OfType<Label>().FirstOrDefault(l => l.Text.Contains("Alert Threshold")).Location = new Point(25, 320);
                txtAlertLimit.Location = new Point(25, 345);
            }

            int buttonStartY = isAdminOrManager ? 640 : 410;

            btnViewPhoto = new Button { Text = "🖼️ View Photo", Location = new Point(25, buttonStartY), Size = new Size(190, 42), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnUploadPhoto = new Button { Text = "📂 Upload Photo", Location = new Point(225, buttonStartY), Size = new Size(190, 42), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnUpdate = new Button { Text = "💾 Update Record", Location = new Point(25, buttonStartY + 50), Size = new Size(190, 42), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnDelete = new Button { Text = "🗑️ Delete Item", Location = new Point(225, buttonStartY + 50), Size = new Size(190, 42), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClear = new Button { Text = "🧹 Clear Forms", Location = new Point(25, buttonStartY + 100), Size = new Size(390, 42), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };

            foreach (var b in new Button[] { btnViewPhoto, btnUploadPhoto, btnUpdate, btnDelete, btnClear }) b.FlatAppearance.BorderSize = 0;
            pnlCard.Controls.AddRange(new Control[] { btnViewPhoto, btnUploadPhoto, btnUpdate, btnDelete, btnClear });

            btnViewPhoto.Click += btnViewPhoto_Click;
            btnUploadPhoto.Click += btnUploadPhoto_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnClear.Click += (s, e) => ClearFields();
        }

        private void ApplyButtonColors()
        {
            if (btnBackHome != null) { btnBackHome.BackColor = Color.FromArgb(99, 102, 241); btnBackHome.ForeColor = Color.White; }
            if (btnViewPhoto != null) { btnViewPhoto.BackColor = Color.FromArgb(14, 165, 233); btnViewPhoto.ForeColor = Color.White; }
            if (btnUploadPhoto != null) { btnUploadPhoto.BackColor = Color.FromArgb(245, 158, 11); btnUploadPhoto.ForeColor = Color.White; }
            if (btnUpdate != null) { btnUpdate.BackColor = Color.FromArgb(34, 197, 94); btnUpdate.ForeColor = Color.White; }
            if (btnDelete != null) { btnDelete.BackColor = Color.FromArgb(239, 68, 68); btnDelete.ForeColor = Color.White; }
            if (btnClear != null) { btnClear.BackColor = Color.FromArgb(100, 116, 139); btnClear.ForeColor = Color.White; }
        }
        #endregion

        #region 📦 Business Management Logic Functions

        private string GetDBStockColumnName(string region)
        {
            switch (region)
            {
                case "Tokyo": return "Stock_Tokyo";
                case "Singapore": return "Stock_Singapore";
                case "New York": return "Stock_NY";
                case "London": return "Stock_London";
                default: return "Stock_HK";
            }
        }

        public void LoadDatabaseData()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query;

                    // 🌟 載入資料時加入 OriginalPrice 同 AlertThreshold
                    if (isAdminOrManager)
                    {
                        query = "SELECT ProductID, ProductName AS 'Name', Stock_HK AS 'Hong Kong', Stock_Tokyo AS 'Tokyo', Stock_Singapore AS 'Singapore', Stock_NY AS 'New York', Stock_London AS 'London', RetailPrice AS 'Current Price', OriginalPrice AS 'Orig. Price', AlertThreshold AS 'Alert Limit' FROM product";
                    }
                    else
                    {
                        string targetStockCol = GetDBStockColumnName(staffRegion);
                        query = $"SELECT ProductID, ProductName AS 'Name', {targetStockCol} AS '{staffRegion}', RetailPrice AS 'Current Price', OriginalPrice AS 'Orig. Price', AlertThreshold AS 'Alert Limit' FROM product";
                    }

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvProductCatalog.DataSource = dt;
                        ApplySearchFilter();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Load Error: " + ex.Message);
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e) => ApplySearchFilter();

        private void ApplySearchFilter()
        {
            if (dgvProductCatalog.DataSource is DataTable dt)
            {
                string keyword = txtSearch.Text.Trim().Replace("'", "''");
                dt.DefaultView.RowFilter = string.IsNullOrWhiteSpace(keyword) ? "" : string.Format("ProductID LIKE '%{0}%' OR Name LIKE '%{0}%'", keyword);
            }
        }

        // 🌟 獨立警界線判斷 (逐行讀取 Alert Limit)
        private void dgvProductCatalog_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            string[] cities = { "Hong Kong", "Tokyo", "Singapore", "New York", "London" };
            if (e.RowIndex >= 0 && cities.Contains(dgvProductCatalog.Columns[e.ColumnIndex].Name))
            {
                var cell = dgvProductCatalog.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (cell.Value != null && int.TryParse(cell.Value.ToString(), out int qty))
                {
                    int threshold = 20;
                    var thresholdCell = dgvProductCatalog.Rows[e.RowIndex].Cells["Alert Limit"];
                    if (thresholdCell != null && thresholdCell.Value != DBNull.Value)
                    {
                        if (int.TryParse(thresholdCell.Value.ToString(), out int parsedThreshold))
                            threshold = Math.Max(20, parsedThreshold); // 最低底線保留 20
                    }

                    if (qty < threshold)
                    {
                        e.CellStyle.BackColor = Color.FromArgb(254, 226, 226);
                        e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                        e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                    }
                }
            }
        }

        private void dgvProductCatalog_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProductCatalog.SelectedRows.Count > 0 && dgvProductCatalog.Columns.Contains("ProductID"))
            {
                DataGridViewRow row = dgvProductCatalog.SelectedRows[0];
                txtProductID.Text = row.Cells["ProductID"].Value?.ToString() ?? "";
                txtProductName.Text = row.Cells["Name"].Value?.ToString() ?? "";
                txtRetailPrice.Text = row.Cells["Current Price"].Value?.ToString() ?? "";
                txtAlertLimit.Text = row.Cells["Alert Limit"].Value?.ToString() ?? "20"; // 🌟 載入該產品的警界線

                if (isAdminOrManager)
                {
                    txtStock_HK.Text = row.Cells["Hong Kong"].Value?.ToString() ?? "0";
                    txtStock_Tokyo.Text = row.Cells["Tokyo"].Value?.ToString() ?? "0";
                    txtStock_Singapore.Text = row.Cells["Singapore"].Value?.ToString() ?? "0";
                    txtStock_NY.Text = row.Cells["New York"].Value?.ToString() ?? "0";
                    txtStock_London.Text = row.Cells["London"].Value?.ToString() ?? "0";
                }
                else
                {
                    string stockValue = row.Cells[staffRegion].Value?.ToString() ?? "0";
                    switch (staffRegion)
                    {
                        case "Tokyo": txtStock_Tokyo.Text = stockValue; break;
                        case "Singapore": txtStock_Singapore.Text = stockValue; break;
                        case "New York": txtStock_NY.Text = stockValue; break;
                        case "London": txtStock_London.Text = stockValue; break;
                        default: txtStock_HK.Text = stockValue; break;
                    }
                }

                txtProductID.ReadOnly = true;
                txtProductID.BackColor = Color.FromArgb(241, 245, 249);
            }
        }

        // 🌟 批次折扣 (基於原價 OriginalPrice 進行折扣，防止越折越平)
        private void BtnApplyDiscount_Click(object sender, EventArgs e)
        {
            if (dgvProductCatalog.SelectedRows.Count == 0) return;

            if (!decimal.TryParse(txtDiscount.Text.Trim(), out decimal discount) || discount <= 0 || discount >= 100)
            {
                MessageBox.Show("Please enter a valid discount percentage (1 - 99).", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult res = MessageBox.Show($"Apply a {discount}% discount to {dgvProductCatalog.SelectedRows.Count} selected product(s)?\n\n(Discount is calculated based on each item's Original Price.)", "Confirm Mass Discount", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (res == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlTransaction trans = conn.BeginTransaction())
                        {
                            try
                            {
                                foreach (DataGridViewRow row in dgvProductCatalog.SelectedRows)
                                {
                                    string pID = row.Cells["ProductID"].Value.ToString();

                                    // 永遠用原價做計算基準
                                    decimal origPrice = Convert.ToDecimal(row.Cells["Orig. Price"].Value);
                                    if (origPrice <= 0) origPrice = Convert.ToDecimal(row.Cells["Current Price"].Value);

                                    decimal newPrice = origPrice * (1m - (discount / 100m));

                                    using (MySqlCommand cmd = new MySqlCommand("UPDATE product SET RetailPrice = @p WHERE ProductID = @id", conn, trans))
                                    {
                                        cmd.Parameters.AddWithValue("@p", newPrice);
                                        cmd.Parameters.AddWithValue("@id", pID);
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                trans.Commit();
                                MessageBox.Show("Bulk discount applied successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadDatabaseData();
                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                throw ex;
                            }
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Error applying discount: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            }
        }

        // 🌟 新增：一鍵還原所有選中產品為原價
        private void BtnRestorePrice_Click(object sender, EventArgs e)
        {
            if (dgvProductCatalog.SelectedRows.Count == 0) return;

            DialogResult res = MessageBox.Show($"Revert {dgvProductCatalog.SelectedRows.Count} selected product(s) back to their Original Retail Price?", "Confirm Price Restoration", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (res == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlTransaction trans = conn.BeginTransaction())
                        {
                            try
                            {
                                foreach (DataGridViewRow row in dgvProductCatalog.SelectedRows)
                                {
                                    string pID = row.Cells["ProductID"].Value.ToString();
                                    using (MySqlCommand cmd = new MySqlCommand("UPDATE product SET RetailPrice = OriginalPrice WHERE ProductID = @id AND OriginalPrice > 0", conn, trans))
                                    {
                                        cmd.Parameters.AddWithValue("@id", pID);
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                trans.Commit();
                                MessageBox.Show("Prices restored successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadDatabaseData();
                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                throw ex;
                            }
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Error restoring prices: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductID.Text))
            {
                MessageBox.Show("Please click on an active catalog item first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query;
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conn;

                    // 🌟 Update 查詢加入 AlertThreshold 儲存
                    if (isAdminOrManager)
                    {
                        query = "UPDATE product SET ProductName=@name, Stock_HK=@shk, Stock_Tokyo=@stk, Stock_Singapore=@ssg, Stock_NY=@sny, Stock_London=@sln, RetailPrice=@price, AlertThreshold=@alert WHERE ProductID=@id";
                        cmd.Parameters.AddWithValue("@shk", string.IsNullOrEmpty(txtStock_HK.Text) ? 0 : Convert.ToInt32(txtStock_HK.Text));
                        cmd.Parameters.AddWithValue("@stk", string.IsNullOrEmpty(txtStock_Tokyo.Text) ? 0 : Convert.ToInt32(txtStock_Tokyo.Text));
                        cmd.Parameters.AddWithValue("@ssg", string.IsNullOrEmpty(txtStock_Singapore.Text) ? 0 : Convert.ToInt32(txtStock_Singapore.Text));
                        cmd.Parameters.AddWithValue("@sny", string.IsNullOrEmpty(txtStock_NY.Text) ? 0 : Convert.ToInt32(txtStock_NY.Text));
                        cmd.Parameters.AddWithValue("@sln", string.IsNullOrEmpty(txtStock_London.Text) ? 0 : Convert.ToInt32(txtStock_London.Text));
                    }
                    else
                    {
                        string targetStockCol = GetDBStockColumnName(staffRegion);
                        query = $"UPDATE product SET ProductName=@name, {targetStockCol}=@stock, RetailPrice=@price, AlertThreshold=@alert WHERE ProductID=@id";

                        TextBox activeTxt = txtStock_HK;
                        if (staffRegion == "Tokyo") activeTxt = txtStock_Tokyo;
                        else if (staffRegion == "Singapore") activeTxt = txtStock_Singapore;
                        else if (staffRegion == "New York") activeTxt = txtStock_NY;
                        else if (staffRegion == "London") activeTxt = txtStock_London;

                        cmd.Parameters.AddWithValue("@stock", string.IsNullOrEmpty(activeTxt.Text) ? 0 : Convert.ToInt32(activeTxt.Text));
                    }

                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@id", txtProductID.Text.Trim());
                    cmd.Parameters.AddWithValue("@name", txtProductName.Text.Trim());
                    cmd.Parameters.AddWithValue("@price", string.IsNullOrEmpty(txtRetailPrice.Text) ? 0 : Convert.ToDecimal(txtRetailPrice.Text.Trim()));
                    cmd.Parameters.AddWithValue("@alert", string.IsNullOrEmpty(txtAlertLimit.Text) ? 20 : Convert.ToInt32(txtAlertLimit.Text.Trim()));

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("Inventory product info and Alert Limit updated successfully!", "Transaction Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDatabaseData();
                        ClearFields();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Update Failure: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnUploadPhoto_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductID.Text)) { MessageBox.Show("Select a product first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            string targetFolder = Path.Combine(Application.StartupPath, "ProductImages");
            if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        foreach (string oldFile in Directory.GetFiles(targetFolder, $"{txtProductID.Text.Trim()}.*")) File.Delete(oldFile);
                        string newPath = Path.Combine(targetFolder, $"{txtProductID.Text.Trim()}{Path.GetExtension(ofd.FileName)}");
                        File.Copy(ofd.FileName, newPath);
                        MessageBox.Show("Image uploaded successfully!", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Upload Failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            }
        }

        private void btnViewPhoto_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductID.Text)) return;
            string folderPath = Path.Combine(Application.StartupPath, "ProductImages");
            using (Form f = new Form { Text = $"Photo - {txtProductName.Text}", Size = new Size(500, 550), StartPosition = FormStartPosition.CenterParent })
            {
                PictureBox pb = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
                string[] files = Directory.Exists(folderPath) ? Directory.GetFiles(folderPath, $"{txtProductID.Text.Trim()}.*") : new string[0];
                if (files.Length > 0) { pb.Image = Image.FromStream(new MemoryStream(File.ReadAllBytes(files[0]))); }
                else { pb.Controls.Add(new Label { Text = "🚫 No Photo Assigned", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter }); }
                f.Controls.Add(pb);
                f.ShowDialog();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductID.Text)) return;
            if (MessageBox.Show("Delete this item?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("DELETE FROM product WHERE ProductID=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtProductID.Text.Trim());
                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            LoadDatabaseData();
                            ClearFields();
                            MessageBox.Show("Item Deleted.");
                        }
                    }
                }
            }
        }

        private void ClearFields()
        {
            txtProductID.Clear(); txtProductName.Clear(); txtRetailPrice.Clear(); txtAlertLimit.Clear();
            txtStock_HK.Clear(); txtStock_Tokyo.Clear(); txtStock_Singapore.Clear(); txtStock_NY.Clear(); txtStock_London.Clear();
            txtProductID.ReadOnly = false; txtProductID.BackColor = Color.White;
            dgvProductCatalog.ClearSelection();
        }
        #endregion
    }
}