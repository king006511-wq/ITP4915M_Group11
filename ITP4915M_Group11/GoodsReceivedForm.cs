using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class GoodsReceivedForm : BaseForm
    {
        // ==========================================
        // 🔒 Database Configuration
        // ==========================================
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // ==========================================
        // 🎨 UI Element Variables
        // ==========================================
        private TextBox txtGRNID, txtPOID, txtMaterialID, txtQty, txtStaffResource, txtTargetWarehouse;
        private DataGridView dgvPOItems;
        private Button btnConfirmReceive, btnClear;

        // 核心佈局控制元件
        private Panel pnlLeftCard;
        private Label lblGridTitle;

        // 🔍 新增搜尋控制項 (統一為 Live Search)
        private TextBox txtSearch;
        private Label lblSearch;

        // 👨‍💼 權限與地區動態變數
        private bool isAdminOrManager = false;
        private string staffRegion = "Hong Kong"; // 預設城市名稱
        private string staffWarehouseID = "W001";  // 預設對應倉庫 ID

        public GoodsReceivedForm()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                DetermineUserAccessLevel();   // 🛡️ 步驟 1：查核員工權限與負責城市，並映射倉庫 ID
                InitializeSleekModernUI();    // 🚀 啟動精緻收身版排版
                GenerateGRNID();
                LoadActivePurchaseOrders();  // 📥 根據權限載入 PO 數據

                this.Load += GoodsReceivedForm_Load;

                // 🌟 核心防禦：綁定視窗縮放事件，強行用數學公式完美控制畫面
                this.SizeChanged += GoodsReceivedForm_SizeChanged;
                this.Layout += (s, e) => RecalculateCustomLayout();
            }
        }

        #region 🔒 權限驗證 與 地區動態映射
        private void DetermineUserAccessLevel()
        {
            string currentRole = UserSession.LoggedInStaffRole;

            // 判斷是否為進階管理層 (Admin/Manager)
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

            // 將文字城市動態映射到你資料庫真實的 WarehouseID (W001 - W005)
            staffWarehouseID = GetWarehouseIDFromRegion(staffRegion);
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

        private void GoodsReceivedForm_Load(object sender, EventArgs e)
        {
            txtStaffResource.Text = UserSession.LoggedInStaffID ?? "S001";

            string currentRole = UserSession.LoggedInStaffRole;
            bool isAuthorized = !string.IsNullOrEmpty(currentRole) &&
                                (isAdminOrManager ||
                                 currentRole.Equals("Warehouse Specialist", StringComparison.OrdinalIgnoreCase) ||
                                 currentRole.Equals("Procurement Officer", StringComparison.OrdinalIgnoreCase));

            if (!isAuthorized)
            {
                MessageBox.Show("[SECURITY ALERT] Access Denied!\n\nOnly Warehouse Specialists and Management can process Goods Received Notes.", "System Security Enforcer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.BeginInvoke(new MethodInvoker(this.Close));
            }
        }
        #endregion

        #region 🎨 精緻手動算繪排版 (告別臃腫，變回精準現代商務風)
        private void InitializeSleekModernUI()
        {
            this.Controls.Clear();
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopLevel = false;
            this.Dock = DockStyle.Fill;

            // =========================================================
            // 【左側】收貨單輸入卡片 (寬度由 420 縮窄至 400)
            // =========================================================
            pnlLeftCard = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };
            pnlLeftCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlLeftCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            this.Controls.Add(pnlLeftCard);

            // 根據權限動態更改標題
            string leftTitle = isAdminOrManager ? "📦 Goods Receiving (Global GRN)" : $"📦 Goods Receiving ({staffRegion} - {staffWarehouseID})";
            Label lblCardTitle = new Label { Text = leftTitle, Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(79, 70, 229), Location = new Point(22, 18), AutoSize = true };
            pnlLeftCard.Controls.Add(lblCardTitle);

            int startY = 65;
            int inputWidth = 350;

            txtGRNID = CreateStyledTextBox(pnlLeftCard, ref startY, "GRN Document ID:", true, inputWidth);
            txtPOID = CreateStyledTextBox(pnlLeftCard, ref startY, "Purchase Order ID:", true, inputWidth);
            txtMaterialID = CreateStyledTextBox(pnlLeftCard, ref startY, "Raw Material ID:", true, inputWidth);
            txtQty = CreateStyledTextBox(pnlLeftCard, ref startY, "Received Quantity:", true, inputWidth);
            txtTargetWarehouse = CreateStyledTextBox(pnlLeftCard, ref startY, "Destination Warehouse:", true, inputWidth);
            txtStaffResource = CreateStyledTextBox(pnlLeftCard, ref startY, "Processed By (Staff ID):", true, inputWidth);

            int btnWidth = 170;
            btnConfirmReceive = new Button { Text = "✅ Confirm Receive", Location = new Point(22, startY + 10), Size = new Size(btnWidth, 42), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClear = new Button { Text = "🔄 Clear Selection", Location = new Point(202, startY + 10), Size = new Size(btnWidth, 42), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnConfirmReceive.FlatAppearance.BorderSize = 0; btnClear.FlatAppearance.BorderSize = 0;

            btnConfirmReceive.Click += btnConfirmReceive_Click;
            btnClear.Click += (s, e) => ClearFields();

            pnlLeftCard.Controls.Add(btnConfirmReceive); pnlLeftCard.Controls.Add(btnClear);

            // =========================================================
            // 【右側】數據表格與標題 (精緻商務化) + Live Search 功能
            // =========================================================
            string rightTitle = isAdminOrManager ? "📥 Pending Purchase Orders (All Cities)" : $"📥 Pending Purchase Orders ({staffRegion} Branch)";
            lblGridTitle = new Label { Text = rightTitle, Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true };
            this.Controls.Add(lblGridTitle);

            // --- 🔍 統一 Live Search UI ---
            lblSearch = new Label { Text = "🔍 Live Search (PO / Material / Destination):", Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), AutoSize = true };
            txtSearch = new TextBox { Width = 350, Font = new Font("Segoe UI", 11F), BorderStyle = BorderStyle.FixedSingle };
            txtSearch.TextChanged += TxtSearch_TextChanged; // 綁定即時搜尋事件

            this.Controls.Add(lblSearch);
            this.Controls.Add(txtSearch);

            dgvPOItems = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                ScrollBars = ScrollBars.Both,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                EnableHeadersVisualStyles = false
            };

            dgvPOItems.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvPOItems.DefaultCellStyle.Padding = new Padding(8);
            dgvPOItems.DefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            dgvPOItems.RowTemplate.Height = 36;

            dgvPOItems.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvPOItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvPOItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPOItems.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            dgvPOItems.ColumnHeadersDefaultCellStyle.Padding = new Padding(8);
            dgvPOItems.ColumnHeadersHeight = 42;

            dgvPOItems.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvPOItems.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);

            dgvPOItems.SelectionChanged += dgvPOItems_SelectionChanged;
            this.Controls.Add(dgvPOItems);
        }

        private TextBox CreateStyledTextBox(Panel container, ref int topY, string labelText, bool readOnly, int width)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(22, topY), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(22, topY + 24), Width = width, Font = new Font("Segoe UI", 11F), BorderStyle = BorderStyle.FixedSingle };
            if (readOnly) { txt.ReadOnly = true; txt.BackColor = Color.FromArgb(241, 245, 249); }
            container.Controls.Add(lbl); container.Controls.Add(txt);
            topY += 75;
            return txt;
        }

        private void GoodsReceivedForm_SizeChanged(object sender, EventArgs e) { RecalculateCustomLayout(); }

        private void RecalculateCustomLayout()
        {
            if (this.Width < 200 || this.Height < 200) return;
            this.SuspendLayout();

            pnlLeftCard.Location = new Point(20, 20);
            pnlLeftCard.Size = new Size(400, this.Height - 40);

            int rightStartX = pnlLeftCard.Right + 20;
            int rightWidth = this.Width - rightStartX - 20;

            if (rightWidth > 100)
            {
                lblGridTitle.Location = new Point(rightStartX, 20);

                if (txtSearch != null && lblSearch != null)
                {
                    int searchY = 60;
                    lblSearch.Location = new Point(rightStartX, searchY + 3);
                    txtSearch.Location = new Point(lblSearch.Right + 10, searchY);
                }

                int dgvStartY = 100;
                dgvPOItems.Location = new Point(rightStartX, dgvStartY);
                dgvPOItems.Size = new Size(rightWidth, this.Height - dgvStartY - 20);

                if (dgvPOItems.Columns.Count > 0)
                {
                    dgvPOItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                    dgvPOItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
            this.ResumeLayout(false);
        }
        #endregion

        #region 💾 資料庫連線與核心邏輯 (權限過濾與分倉庫存寫入)
        private void GenerateGRNID()
        {
            txtGRNID.Text = "GRN" + DateTime.Now.ToString("MMddHHmmss");
        }

        // 🌟 核心修改：結合權限動態過濾
        private void LoadActivePurchaseOrders()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query;

                    // 🌟 如果是 Admin 顯示全球訂單與目的地城市；如果是一般專員，使用 WHERE 鎖死所屬倉庫
                    if (isAdminOrManager)
                    {
                        query = @"
                            SELECT 
                                po.PO_ID AS 'PO Number', 
                                pol.MaterialID AS 'Material ID', 
                                rm.MaterialName AS 'Material Name', 
                                pol.Quantity AS 'Order Qty', 
                                po.WarehouseID AS 'Warehouse ID',
                                COALESCE(w.City, 'Unassigned') AS 'Destination',
                                po.Status 
                            FROM purchase_order po
                            JOIN po_lineitem pol ON po.PO_ID = pol.PO_ID
                            JOIN raw_material rm ON pol.MaterialID = rm.MaterialID
                            LEFT JOIN warehouse w ON po.WarehouseID = w.WarehouseID
                            WHERE po.Status != 'Received'
                            ORDER BY po.PODate DESC";
                    }
                    else
                    {
                        query = @"
                            SELECT 
                                po.PO_ID AS 'PO Number', 
                                pol.MaterialID AS 'Material ID', 
                                rm.MaterialName AS 'Material Name', 
                                pol.Quantity AS 'Order Qty', 
                                po.WarehouseID AS 'Warehouse ID',
                                w.City AS 'Destination',
                                po.Status 
                            FROM purchase_order po
                            JOIN po_lineitem pol ON po.PO_ID = pol.PO_ID
                            JOIN raw_material rm ON pol.MaterialID = rm.MaterialID
                            LEFT JOIN warehouse w ON po.WarehouseID = w.WarehouseID
                            WHERE po.Status != 'Received' AND po.WarehouseID = @whID
                            ORDER BY po.PODate DESC";
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

                            dgvPOItems.DataSource = null;
                            dgvPOItems.DataSource = dt;

                            foreach (DataGridViewColumn col in dgvPOItems.Columns)
                            {
                                col.MinimumWidth = 100;
                            }
                            if (dgvPOItems.Columns.Contains("Material Name")) dgvPOItems.Columns["Material Name"].MinimumWidth = 150;

                            if (txtSearch != null && !string.IsNullOrWhiteSpace(txtSearch.Text))
                            {
                                TxtSearch_TextChanged(null, null);
                            }

                            RecalculateCustomLayout();
                            dgvPOItems.ClearSelection();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load PO data:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 🌟 Live Search 同步增強過濾
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (dgvPOItems.DataSource is DataTable dt)
            {
                string keyword = txtSearch.Text.Trim().Replace("'", "''");

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    dt.DefaultView.RowFilter = "";
                }
                else
                {
                    dt.DefaultView.RowFilter = $"[PO Number] LIKE '%{keyword}%' OR [Material ID] LIKE '%{keyword}%' OR [Material Name] LIKE '%{keyword}%' OR [Destination] LIKE '%{keyword}%'";
                }
            }
        }

        private void dgvPOItems_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPOItems.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvPOItems.SelectedRows[0];
                txtPOID.Text = row.Cells["PO Number"].Value?.ToString() ?? "";
                txtMaterialID.Text = row.Cells["Material ID"].Value?.ToString() ?? "";
                txtQty.Text = row.Cells["Order Qty"].Value?.ToString() ?? "";

                // 🌟 同步顯示這筆訂單的目的地倉庫 (例如: W001 - Hong Kong)
                string whID = row.Cells["Warehouse ID"].Value?.ToString() ?? "";
                string destCity = row.Cells["Destination"].Value?.ToString() ?? "";
                txtTargetWarehouse.Text = $"{whID} - {destCity}";
            }
        }

        private void btnConfirmReceive_Click(object sender, EventArgs e)
        {
            string poID = txtPOID.Text.Trim();
            string materialID = txtMaterialID.Text.Trim();
            string qtyStr = txtQty.Text.Trim();
            string grnID = txtGRNID.Text.Trim();
            string staffID = txtStaffResource.Text.Trim();

            if (string.IsNullOrWhiteSpace(poID) || string.IsNullOrWhiteSpace(materialID) || dgvPOItems.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a pending Purchase Order from the grid first.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🌟 獲取選取訂單實際歸屬的 WarehouseID 
            string targetWHID = dgvPOItems.SelectedRows[0].Cells["Warehouse ID"].Value.ToString();
            string targetCity = dgvPOItems.SelectedRows[0].Cells["Destination"].Value.ToString();

            if (!int.TryParse(qtyStr, out int qty) || qty <= 0)
            {
                MessageBox.Show("Invalid quantity format detected in PO details.", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Are you sure you want to receive this shipment?\n\n" +
                $"PO Number: {poID}\n" +
                $"Material ID: {materialID}\n" +
                $"Quantity: {qty}\n" +
                $"Target Warehouse: [{targetWHID} - {targetCity}]\n\n" +
                $"This will permanently increase the stock level for {targetCity.ToUpper()} warehouse.",
                "Confirm Receiving", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.No) return;

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. 寫入收貨單紀錄
                            string queryGRN = "INSERT INTO goods_received_note (GRN_ID, PO_ID, StaffID, ReceivedDate) VALUES (@grnID, @poID, @staffID, NOW())";
                            using (MySqlCommand cmdGRN = new MySqlCommand(queryGRN, conn, trans))
                            {
                                cmdGRN.Parameters.AddWithValue("@grnID", grnID);
                                cmdGRN.Parameters.AddWithValue("@poID", poID);
                                cmdGRN.Parameters.AddWithValue("@staffID", staffID);
                                cmdGRN.ExecuteNonQuery();
                            }

                            // 2. 🌟 核心修正：將貨物數量增加到該訂單指定的城市「地區庫存關聯表 (inventory)」內！
                            string queryStock = @"
                                INSERT INTO inventory (WarehouseID, MaterialID, StockLevel) 
                                VALUES (@whID, @matID, @qty)
                                ON DUPLICATE KEY UPDATE StockLevel = StockLevel + @qty";

                            using (MySqlCommand cmdStock = new MySqlCommand(queryStock, conn, trans))
                            {
                                cmdStock.Parameters.AddWithValue("@qty", qty);
                                cmdStock.Parameters.AddWithValue("@matID", materialID);
                                cmdStock.Parameters.AddWithValue("@whID", targetWHID);
                                cmdStock.ExecuteNonQuery();
                            }

                            // 3. 更新採購單狀態為已收貨
                            string queryPO = "UPDATE purchase_order SET Status = 'Received' WHERE PO_ID = @poID";
                            using (MySqlCommand cmdPO = new MySqlCommand(queryPO, conn, trans))
                            {
                                cmdPO.Parameters.AddWithValue("@poID", poID);
                                cmdPO.ExecuteNonQuery();
                            }

                            trans.Commit();
                            MessageBox.Show($"Inventory ingestion transaction committed successfully!\n\nGRN ID: {grnID}\nMaterial ID [{materialID}] has been safely added into [{targetCity}] warehouse stock (+{qty} units).", "Ingestion Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            ClearFields();
                            LoadActivePurchaseOrders(); // 即時重整右側列表
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw new Exception("Stock ingestion transaction aborted. Control rolled back safely.\nDetails: " + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Transaction Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearFields()
        {
            txtPOID.Clear();
            txtMaterialID.Clear();
            txtQty.Clear();
            txtTargetWarehouse.Clear();
            dgvPOItems.ClearSelection();
            GenerateGRNID();
        }
        #endregion
    }
}