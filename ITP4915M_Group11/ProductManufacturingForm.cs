using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public class ManufactureProductItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }

    public partial class ProductManufacturingForm : BaseForm
    {
        // ==========================================
        // 🔒 Database Configuration & Settings
        // ==========================================
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // 🚨 庫存紅字警告線
        private readonly int STOCK_WARNING_THRESHOLD = 50;

        // ==========================================
        // 🎨 UI 變數 
        // ==========================================
        private ComboBox custom_cmbRegion; // 🌟 新增：城市選擇下拉選單
        private ComboBox custom_cmbProduct;
        private TextBox custom_txtQty;
        private DataGridView custom_dgvBOMRequirements;
        private DataGridView custom_dgvRawMaterialStock;
        private Button custom_btnCalculate, custom_btnManufacture, custom_btnClear;

        // 🔍 搜尋區塊 UI 變數
        private TextBox custom_txtSearch;
        private Label custom_lblSearch;

        // 👨‍💼 權限與地區動態變數
        private bool isAdminOrManager = false;
        private string staffRegion = "Hong Kong"; // 預設城市名稱
        private string staffWarehouseID = "W001";  // 預設對應倉庫 ID

        public ProductManufacturingForm()
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                DetermineUserAccessLevel();  // 🛡️ 步驟 1：查核員工權限與負責城市，並映射倉庫 ID
                SetupCustomSleekUI();
                EnforceSecurityGatekeeper(); // 🛡️ 步驟 2：執行介面進入守衛
                LoadManufacturableProducts();
                LoadRawMaterialInventory();

                this.SizeChanged += ProductManufacturingForm_SizeChanged;
                this.Layout += (s, e) => RecalculateDynamicLayout();
            }
        }

        #region 🔒 System Security Gatekeeper Enforcement & Role Detection
        private void DetermineUserAccessLevel()
        {
            string currentRole = UserSession.LoggedInStaffRole;

            // 判斷是否為高級管理層 (Admin/Manager)
            isAdminOrManager = !string.IsNullOrEmpty(currentRole) &&
                               (currentRole.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                                currentRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase));

            // 查詢當前登入員工所負責的城市 Region
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT Region FROM staff WHERE StaffID = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", UserSession.LoggedInStaffID ?? "");
                        object res = cmd.ExecuteScalar();
                        if (res != null && res != DBNull.Value)
                        {
                            staffRegion = res.ToString();
                        }
                    }
                }
                catch { staffRegion = "Hong Kong"; }
            }

            // 將文字城市動態映射到資料庫真實的 WarehouseID
            staffWarehouseID = GetWarehouseIDFromRegion(staffRegion);
        }

        private void EnforceSecurityGatekeeper()
        {
            string currentRole = UserSession.LoggedInStaffRole ?? "";
            string currentStaffID = UserSession.LoggedInStaffID;

            // 🌟 增強容錯率：只要職位名稱含有 Factory、Warehouse、Manager、Admin 關鍵字就放行
            bool isAuthorized = isAdminOrManager ||
                                currentRole.IndexOf("Factory", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                currentRole.IndexOf("Warehouse", StringComparison.OrdinalIgnoreCase) >= 0;

            if (!isAuthorized)
            {
                MessageBox.Show(
                    $"[SECURITY ALERT] Access Denied!\n\n" +
                    $"Logged In Staff ID: {(string.IsNullOrEmpty(currentStaffID) ? "Unknown" : currentStaffID)}\n" +
                    $"Your Account Role is: \"{currentRole}\"\n\n" +
                    $"You are not authorized to access the Manufacturing Module.",
                    "System Security Guard",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop
                );

                this.Shown += (s, e) => this.Close();
            }
        }

        private string GetWarehouseIDFromRegion(string region)
        {
            if (string.IsNullOrEmpty(region)) return "W001";
            if (region.IndexOf("Tokyo", StringComparison.OrdinalIgnoreCase) >= 0) return "W002";
            if (region.IndexOf("Singapore", StringComparison.OrdinalIgnoreCase) >= 0) return "W003";
            if (region.IndexOf("New York", StringComparison.OrdinalIgnoreCase) >= 0 || region.IndexOf("NY", StringComparison.OrdinalIgnoreCase) >= 0) return "W004";
            if (region.IndexOf("London", StringComparison.OrdinalIgnoreCase) >= 0) return "W005";
            return "W001"; // 預設為香港 W001
        }

        // 🌟 新增 Helper：將地區名轉換為 product 資料表入面的庫存欄位名稱
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
        #endregion

        #region 🎨 精緻手動算繪排版
        private void SetupCustomSleekUI()
        {
            this.Controls.Clear();
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopLevel = false;
            this.Dock = DockStyle.Fill;

            // --- 左側面板 (操作區) ---
            Panel pnlLeftCard = new Panel { Name = "pnlLeftCard", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, AutoScroll = true };
            pnlLeftCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlLeftCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            this.Controls.Add(pnlLeftCard);

            string titleText = isAdminOrManager ? "⚙️ Global Product Processing" : $"⚙️ Product Processing ({staffRegion} - {staffWarehouseID})";
            Label lblCardTitle = new Label { Name = "lblCardTitle", Text = titleText, Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(79, 70, 229), Location = new Point(22, 18), AutoSize = true };
            pnlLeftCard.Controls.Add(lblCardTitle);

            int startY = 65;
            int inputWidth = 350;

            // 🌟 1. Region Selection (城市選擇下拉選單)
            Label lblRegion = new Label { Text = "Target Factory Location *:", Location = new Point(22, startY), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            custom_cmbRegion = new ComboBox { Location = new Point(22, startY + 24), Width = inputWidth, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11F), BackColor = Color.White };
            custom_cmbRegion.Items.AddRange(new string[] { "Hong Kong", "Tokyo", "Singapore", "New York", "London" });

            // 預設選擇 HR 登錄的城市
            if (custom_cmbRegion.Items.Contains(staffRegion))
                custom_cmbRegion.SelectedItem = staffRegion;
            else
                custom_cmbRegion.SelectedIndex = 0;

            // 如果不是 Admin / Manager，鎖定不能更改地區
            custom_cmbRegion.Enabled = isAdminOrManager;
            custom_cmbRegion.SelectedIndexChanged += custom_cmbRegion_SelectedIndexChanged;

            pnlLeftCard.Controls.Add(lblRegion);
            pnlLeftCard.Controls.Add(custom_cmbRegion);
            startY += 75;

            // 🌟 2. Product Selection
            Label lblProd = new Label { Text = "Select Target Product *:", Location = new Point(22, startY), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            custom_cmbProduct = new ComboBox { Location = new Point(22, startY + 24), Width = inputWidth, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11F), BackColor = Color.White };
            custom_cmbProduct.SelectedIndexChanged += custom_cmbProduct_SelectedIndexChanged;
            pnlLeftCard.Controls.Add(lblProd);
            pnlLeftCard.Controls.Add(custom_cmbProduct);
            startY += 75;

            // 🌟 3. Quantity Input
            Label lblQty = new Label { Text = "Production Quantity *:", Location = new Point(22, startY), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            custom_txtQty = new TextBox { Location = new Point(22, startY + 24), Width = inputWidth, Font = new Font("Segoe UI", 11F), BorderStyle = BorderStyle.FixedSingle };
            pnlLeftCard.Controls.Add(lblQty);
            pnlLeftCard.Controls.Add(custom_txtQty);
            startY += 65;

            // 🌟 4. Buttons
            custom_btnCalculate = new Button { Text = "🧮 Check BOM & Stock", Location = new Point(22, startY), Size = new Size(350, 40), BackColor = Color.FromArgb(245, 158, 11), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand };
            custom_btnCalculate.FlatAppearance.BorderSize = 0;
            custom_btnCalculate.Click += btnCalculate_Click;
            pnlLeftCard.Controls.Add(custom_btnCalculate);
            startY += 55;

            // 🌟 5. BOM Requirements Grid
            Label lblBOM = new Label { Text = "📋 Required Raw Materials (BOM):", Location = new Point(22, startY), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            pnlLeftCard.Controls.Add(lblBOM);
            startY += 25;

            custom_dgvBOMRequirements = new DataGridView { Location = new Point(22, startY), Size = new Size(350, 150), BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, EnableHeadersVisualStyles = false };
            custom_dgvBOMRequirements.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(71, 85, 105);
            custom_dgvBOMRequirements.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            pnlLeftCard.Controls.Add(custom_dgvBOMRequirements);
            startY += 165;

            custom_btnManufacture = new Button { Text = "🔨 Start Processing (Deduct Stock)", Location = new Point(22, startY), Size = new Size(350, 42), BackColor = Color.LightGray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand, Enabled = false };
            custom_btnManufacture.FlatAppearance.BorderSize = 0;
            custom_btnManufacture.Click += btnManufacture_Click;
            pnlLeftCard.Controls.Add(custom_btnManufacture);
            startY += 50;

            custom_btnClear = new Button { Text = "🔄 Reset", Location = new Point(22, startY), Size = new Size(350, 40), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            custom_btnClear.FlatAppearance.BorderSize = 0;
            custom_btnClear.Click += (s, e) => ClearForm();
            pnlLeftCard.Controls.Add(custom_btnClear);

            // --- 右側面板 (庫存監控區) ---
            string gridTitleText = isAdminOrManager ? "📦 Global Warehouse Inventory Monitor" : $"📦 Warehouse Inventory Monitor ({staffRegion})";
            Label lblGridTitle = new Label { Name = "lblGridTitle", Text = gridTitleText, Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true };
            this.Controls.Add(lblGridTitle);

            // 🔍 搜尋標籤與輸入框
            custom_lblSearch = new Label { Name = "lblSearch", Text = "🔍 Search Material:", Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), AutoSize = true };
            custom_txtSearch = new TextBox { Name = "txtSearch", Width = 300, Font = new Font("Segoe UI", 11F), BorderStyle = BorderStyle.FixedSingle };
            custom_txtSearch.TextChanged += custom_txtSearch_TextChanged;

            this.Controls.Add(custom_lblSearch);
            this.Controls.Add(custom_txtSearch);

            custom_dgvRawMaterialStock = new DataGridView
            {
                Name = "dgvStock",
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                ScrollBars = ScrollBars.Both,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };
            custom_dgvRawMaterialStock.DefaultCellStyle.Padding = new Padding(8);
            custom_dgvRawMaterialStock.DefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            custom_dgvRawMaterialStock.RowTemplate.Height = 36;
            custom_dgvRawMaterialStock.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            custom_dgvRawMaterialStock.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            custom_dgvRawMaterialStock.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            custom_dgvRawMaterialStock.ColumnHeadersHeight = 42;

            custom_dgvRawMaterialStock.CellFormatting += custom_dgvRawMaterialStock_CellFormatting;
            this.Controls.Add(custom_dgvRawMaterialStock);
        }

        private void ProductManufacturingForm_SizeChanged(object sender, EventArgs e) { RecalculateDynamicLayout(); }

        private void RecalculateDynamicLayout()
        {
            if (this.Width < 200 || this.Height < 200) return;
            this.SuspendLayout();

            Panel pnlLeft = (Panel)this.Controls["pnlLeftCard"];
            pnlLeft.Location = new Point(20, 20);
            pnlLeft.Size = new Size(400, this.Height - 40);

            int rightStartX = pnlLeft.Right + 20;
            int rightWidth = this.Width - rightStartX - 20;

            if (rightWidth > 100)
            {
                this.Controls["lblGridTitle"].Location = new Point(rightStartX, 20);
                custom_lblSearch.Location = new Point(rightStartX, 58);
                custom_txtSearch.Location = new Point(custom_lblSearch.Right + 10, 55);

                DataGridView dgvStock = (DataGridView)this.Controls["dgvStock"];
                dgvStock.Location = new Point(rightStartX, 95);
                dgvStock.Size = new Size(rightWidth, this.Height - 115);
            }
            this.ResumeLayout(false);
        }

        private void custom_dgvRawMaterialStock_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            string[] alertTargetColumns = { "Hong Kong", "Tokyo", "Singapore", "New York", "London", "Local Stock" };
            string columnName = custom_dgvRawMaterialStock.Columns[e.ColumnIndex].Name;

            if (e.RowIndex >= 0 && alertTargetColumns.Contains(columnName) && e.Value != null)
            {
                if (int.TryParse(e.Value.ToString(), out int stock) && stock < STOCK_WARNING_THRESHOLD)
                {
                    e.CellStyle.BackColor = Color.FromArgb(254, 226, 226);
                    e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                    e.CellStyle.Font = new Font(custom_dgvRawMaterialStock.Font, FontStyle.Bold);
                }
            }
        }
        #endregion

        #region 🔄 新增：動態切換城市事件
        private void custom_cmbRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (custom_cmbRegion.SelectedItem != null)
            {
                // 更新當前選擇的地區與倉庫 ID
                staffRegion = custom_cmbRegion.SelectedItem.ToString();
                staffWarehouseID = GetWarehouseIDFromRegion(staffRegion);

                // 更新左方標題
                Panel pnlLeft = (Panel)this.Controls["pnlLeftCard"];
                Label lblCardTitle = (Label)pnlLeft.Controls["lblCardTitle"];
                if (lblCardTitle != null)
                {
                    lblCardTitle.Text = isAdminOrManager ? "⚙️ Global Product Processing" : $"⚙️ Product Processing ({staffRegion} - {staffWarehouseID})";
                }

                // 更新右方庫存標題
                Label lblGridTitle = (Label)this.Controls["lblGridTitle"];
                if (lblGridTitle != null)
                {
                    lblGridTitle.Text = isAdminOrManager ? "📦 Global Warehouse Inventory Monitor" : $"📦 Warehouse Inventory Monitor ({staffRegion})";
                }

                // 重新載入最新選擇城市的原材料庫存
                LoadRawMaterialInventory();
                ClearForm();
            }
        }
        #endregion

        #region 🔍 搜尋邏輯
        private void custom_txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (custom_dgvRawMaterialStock.DataSource is DataTable dt)
            {
                string keyword = custom_txtSearch.Text.Trim().Replace("'", "''")
                                               .Replace("[", "[[]")
                                               .Replace("]", "[]]")
                                               .Replace("*", "[*]")
                                               .Replace("%", "[%]");

                if (string.IsNullOrEmpty(keyword))
                {
                    dt.DefaultView.RowFilter = string.Empty;
                }
                else
                {
                    dt.DefaultView.RowFilter = $"Convert([Material ID], 'System.String') LIKE '%{keyword}%' OR [Material Name] LIKE '%{keyword}%'";
                }
            }
        }
        #endregion

        #region 💾 資料庫載入
        private void LoadManufacturableProducts()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT DISTINCT p.ProductID, p.ProductName 
                        FROM product p
                        JOIN bill_of_materials bom ON p.ProductID = bom.ProductID";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<ManufactureProductItem> list = new List<ManufactureProductItem>();
                        while (reader.Read())
                        {
                            list.Add(new ManufactureProductItem
                            {
                                ID = reader["ProductID"].ToString(),
                                Name = reader["ProductName"].ToString()
                            });
                        }
                        custom_cmbProduct.DataSource = list;
                        custom_cmbProduct.DisplayMember = "Name";
                        custom_cmbProduct.ValueMember = "ID";
                        custom_cmbProduct.SelectedIndex = -1;
                    }
                }
                catch (Exception ex) { MessageBox.Show("Failed to load products: " + ex.Message); }
            }
        }

        private void LoadRawMaterialInventory()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query;

                    if (isAdminOrManager)
                    {
                        query = @"
                            SELECT r.MaterialID AS 'Material ID', r.MaterialName AS 'Material Name',
                                   MAX(CASE WHEN i.WarehouseID = 'W001' THEN i.StockLevel ELSE 0 END) AS 'Hong Kong',
                                   MAX(CASE WHEN i.WarehouseID = 'W002' THEN i.StockLevel ELSE 0 END) AS 'Tokyo',
                                   MAX(CASE WHEN i.WarehouseID = 'W003' THEN i.StockLevel ELSE 0 END) AS 'Singapore',
                                   MAX(CASE WHEN i.WarehouseID = 'W004' THEN i.StockLevel ELSE 0 END) AS 'New York',
                                   MAX(CASE WHEN i.WarehouseID = 'W005' THEN i.StockLevel ELSE 0 END) AS 'London'
                            FROM raw_material r
                            LEFT JOIN inventory i ON r.MaterialID = i.MaterialID
                            GROUP BY r.MaterialID, r.MaterialName
                            ORDER BY r.MaterialID ASC";
                    }
                    else
                    {
                        query = @"
                            SELECT r.MaterialID AS 'Material ID', r.MaterialName AS 'Material Name',
                                   COALESCE(i.StockLevel, 0) AS 'Local Stock'
                            FROM raw_material r
                            LEFT JOIN inventory i ON r.MaterialID = i.MaterialID AND i.WarehouseID = @whID
                            ORDER BY r.MaterialID ASC";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        if (!isAdminOrManager)
                        {
                            cmd.Parameters.AddWithValue("@whID", staffWarehouseID);
                        }

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            custom_dgvRawMaterialStock.DataSource = dt;
                            custom_txtSearch_TextChanged(null, null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DataTable errorDt = new DataTable();
                    errorDt.Columns.Add("System Status");
                    errorDt.Rows.Add("Database Error: " + ex.Message);
                    custom_dgvRawMaterialStock.DataSource = errorDt;
                }
            }
        }

        private void custom_cmbProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            custom_dgvBOMRequirements.DataSource = null;
            custom_btnManufacture.Enabled = false;
            custom_btnManufacture.BackColor = Color.LightGray;
        }
        #endregion

        #region 🧮 庫存運算與 Transaction 生產
        private void btnCalculate_Click(object sender, EventArgs e)
        {
            if (custom_cmbProduct.SelectedItem == null)
            {
                MessageBox.Show("Please select a Product to manufacture.", "Missing Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(custom_txtQty.Text.Trim(), out int qty) || qty <= 0)
            {
                MessageBox.Show("Please enter a valid positive quantity.", "Invalid Qty", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string productID = ((ManufactureProductItem)custom_cmbProduct.SelectedItem).ID;

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            b.MaterialID AS 'Mat ID',
                            rm.MaterialName AS 'Material Name',
                            (b.QuantityRequired * @qty) AS 'Total Required',
                            COALESCE(i.StockLevel, 0) AS 'Stock Available',
                            CASE WHEN COALESCE(i.StockLevel, 0) >= (b.QuantityRequired * @qty) THEN 'OK' ELSE 'SHORTAGE' END AS 'Status'
                        FROM bill_of_materials b
                        JOIN raw_material rm ON b.MaterialID = rm.MaterialID
                        LEFT JOIN inventory i ON rm.MaterialID = i.MaterialID AND i.WarehouseID = @whID
                        WHERE b.ProductID = @prodID";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@qty", qty);
                        cmd.Parameters.AddWithValue("@whID", staffWarehouseID); // 使用目前選取的城市對應倉庫
                        cmd.Parameters.AddWithValue("@prodID", productID);

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            custom_dgvBOMRequirements.DataSource = dt;

                            bool canManufacture = true;
                            foreach (DataRow row in dt.Rows)
                            {
                                if (row["Status"].ToString() == "SHORTAGE")
                                {
                                    canManufacture = false;
                                    break;
                                }
                            }

                            if (canManufacture)
                            {
                                custom_btnManufacture.Enabled = true;
                                custom_btnManufacture.BackColor = Color.FromArgb(16, 185, 129); // 綠色開燈
                                MessageBox.Show($"[{staffRegion} Warehouse - {staffWarehouseID}] Stock is sufficient. Ready to assemble.", "Check Passed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                custom_btnManufacture.Enabled = false;
                                custom_btnManufacture.BackColor = Color.LightGray;
                                MessageBox.Show($"INSUFFICIENT STOCK IN {staffRegion.ToUpper()} WAREHOUSE!\nCannot start assembly process.", "Shortage Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error calculating BOM: " + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnManufacture_Click(object sender, EventArgs e)
        {
            string productID = ((ManufactureProductItem)custom_cmbProduct.SelectedItem).ID;
            int qty = Convert.ToInt32(custom_txtQty.Text.Trim());

            DialogResult dialogResult = MessageBox.Show($"Confirm assembly of {qty} units of [{productID}] for [{staffRegion}] branch?\n\nThis will deduct raw materials from local warehouse ({staffWarehouseID}).", "Confirm Production", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlTransaction trans = conn.BeginTransaction())
                        {
                            try
                            {
                                // 1. 扣減特定倉庫在 `inventory` 表中的原材料存量
                                string deductSql = @"
                                    UPDATE inventory i
                                    JOIN bill_of_materials b ON i.MaterialID = b.MaterialID
                                    SET i.StockLevel = i.StockLevel - (b.QuantityRequired * @qty)
                                    WHERE b.ProductID = @prodID AND i.WarehouseID = @whID";

                                using (MySqlCommand cmd = new MySqlCommand(deductSql, conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@qty", qty);
                                    cmd.Parameters.AddWithValue("@prodID", productID);
                                    cmd.Parameters.AddWithValue("@whID", staffWarehouseID);
                                    cmd.ExecuteNonQuery();
                                }

                                // 🌟🌟🌟 核心修復：增加對應城市的製成品庫存數量 (依據各別城市專屬欄位，取代舊有的 StockLevel)
                                string targetStockCol = GetDBStockColumnName(staffRegion);
                                string addSql = $"UPDATE product SET {targetStockCol} = {targetStockCol} + @qty WHERE ProductID = @prodID";

                                using (MySqlCommand cmd = new MySqlCommand(addSql, conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@qty", qty);
                                    cmd.Parameters.AddWithValue("@prodID", productID);
                                    cmd.ExecuteNonQuery();
                                }

                                trans.Commit();
                                MessageBox.Show($"Manufacturing assembly process for [{staffRegion}] completed successfully!\nWarehouse stock has been updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                ClearForm();
                                LoadRawMaterialInventory(); // 即時重整右側庫存數據
                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                throw new Exception("Transaction failed. Rollback executed. Reason: " + ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to process manufacturing:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ClearForm()
        {
            custom_cmbProduct.SelectedIndex = -1;
            custom_txtQty.Clear();
            custom_dgvBOMRequirements.DataSource = null;
            custom_btnManufacture.Enabled = false;
            custom_btnManufacture.BackColor = Color.LightGray;
            custom_txtSearch.Clear();
        }
        #endregion
    }
}