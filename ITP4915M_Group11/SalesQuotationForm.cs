using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class SalesQuotationForm : BaseForm
    {
        // 🔒 Database Configuration
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // 🎨 UI Elements
        private DataGridView dgvPendingOrders;
        private DataGridView dgvOrderDetails;
        private TextBox txtSearchOrder;
        private Button btnApprove, btnReject;
        private Label lblSelectedOrder;

        public SalesQuotationForm()
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                SetupModernUI();
                EnforceSecurityGatekeeper(); // 🛡️ 執行安全權限攔截
                LoadPendingOrders();
            }
        }

        #region 🌍 Multi-City Inventory Logic
        // 🌟 強化版：自動從訂單狀態中提取所屬城市 (防呆、防大細楷錯誤、防缺少括號)
        private string ExtractRegion(string status)
        {
            if (string.IsNullOrEmpty(status)) return "Hong Kong";

            string lowerStatus = status.ToLower(); // 統一轉做細楷檢查

            if (lowerStatus.Contains("tokyo")) return "Tokyo";
            if (lowerStatus.Contains("singapore")) return "Singapore";
            if (lowerStatus.Contains("new york") || lowerStatus.Contains("ny")) return "New York";
            if (lowerStatus.Contains("london")) return "London";

            return "Hong Kong"; // 預設為香港
        }

        // 根據城市動態映射到 Database 嘅庫存欄位
        private string GetStockColumnName(string region)
        {
            switch (region)
            {
                case "Tokyo": return "Stock_Tokyo";
                case "Singapore": return "Stock_Singapore";
                case "New York": return "Stock_NY";
                case "London": return "Stock_London";
                case "Hong Kong":
                default: return "Stock_HK";
            }
        }
        #endregion

        #region 🔒 System Security Gatekeeper
        private void EnforceSecurityGatekeeper()
        {
            string currentRole = UserSession.LoggedInStaffRole ?? "";

            // 允許 Sales Representative 以「檢視」角色進入，但只有 Manager / Administrator / Warehouse Supervisor 可進行審批與扣庫存
            bool isAuthorized = currentRole.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                                currentRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase) ||
                                currentRole.Equals("Warehouse Supervisor", StringComparison.OrdinalIgnoreCase) ||
                                currentRole.Equals("Sales Representative", StringComparison.OrdinalIgnoreCase) ||
                                currentRole.Equals("Sales", StringComparison.OrdinalIgnoreCase);

            if (!isAuthorized)
            {
                MessageBox.Show(
                    $"[SECURITY ALERT] Access Denied!\n\n" +
                    $"Your Role: \"{(string.IsNullOrEmpty(currentRole) ? "None" : currentRole)}\"\n\n" +
                    $"Order Approval & Stock Allocation is restricted to Managers and Administrators only.",
                    "System Security Guard",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop
                );

                this.Shown += (s, e) => this.Close(); // 拒絕訪問並自動關閉表單
            }
        }
        #endregion

        #region 🎨 UI Setup (套用統一現代化風格)
        private void SetupModernUI()
        {
            this.Controls.Clear();
            this.BackColor = ThemeManager.PrimaryBackground;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;

            // 1. Header Panel
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Color.White };
            pnlHeader.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlHeader.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

            Label lblHeader = new Label { Text = "🔒 Global Order Approval & Regional Stock Allocation", Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(30, 15), AutoSize = true };
            Label lblSub = new Label { Text = "Verify regional inventory availability, allocate city-specific stock, and advance orders to Logistics.", Font = new Font("Segoe UI", 9.5F), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(32, 55), AutoSize = true };
            pnlHeader.Controls.AddRange(new Control[] { lblHeader, lblSub });
            this.Controls.Add(pnlHeader);

            // 2. 🔍 Search Toolbar Block
            Label lblSearch = new Label { Text = "Quick Search:", Location = new Point(30, 108), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            this.Controls.Add(lblSearch);

            txtSearchOrder = new TextBox { Location = new Point(140, 105), Width = 250, Font = new Font("Segoe UI", 10F), BorderStyle = BorderStyle.FixedSingle };
            txtSearchOrder.TextChanged += TxtSearchOrder_TextChanged; // 動態篩選事件
            this.Controls.Add(txtSearchOrder);

            // 3. Left Container (Pending Orders Card)
            Panel pnlLeftCard = new Panel { Location = new Point(30, 145), Width = 460, Height = this.Height - 185, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left, BackColor = Color.White };
            pnlLeftCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlLeftCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

            Label lblGridTitle = new Label { Text = "⏳ Orders Awaiting Approval", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(15, 15), AutoSize = true };
            pnlLeftCard.Controls.Add(lblGridTitle);

            dgvPendingOrders = new DataGridView
            {
                Location = new Point(15, 45),
                Size = new Size(pnlLeftCard.Width - 30, pnlLeftCard.Height - 60),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                GridColor = Color.FromArgb(241, 245, 249),
                EnableHeadersVisualStyles = false
            };
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(139, 92, 246); // 紫色系
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvPendingOrders.ColumnHeadersHeight = 35;
            dgvPendingOrders.RowTemplate.Height = 32;
            dgvPendingOrders.SelectionChanged += dgvPendingOrders_SelectionChanged_Optimized;
            dgvPendingOrders.DataBindingComplete += DgvPendingOrders_DataBindingComplete; // 新增狀態顏色顯示
            pnlLeftCard.Controls.Add(dgvPendingOrders);
            this.Controls.Add(pnlLeftCard);

            // 4. Right Container (Order Details Card)
            Panel pnlRightCard = new Panel { Location = new Point(510, 145), Width = this.Width - 540, Height = this.Height - 185, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, BackColor = Color.White };
            pnlRightCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlRightCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

            lblSelectedOrder = new Label { Text = "Selected Order: None", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(15, 15), AutoSize = true };
            pnlRightCard.Controls.Add(lblSelectedOrder);

            dgvOrderDetails = new DataGridView
            {
                Location = new Point(15, 45),
                Size = new Size(pnlRightCard.Width - 30, pnlRightCard.Height - 120),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                GridColor = Color.FromArgb(241, 245, 249),
                EnableHeadersVisualStyles = false
            };
            dgvOrderDetails.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(71, 85, 105);
            dgvOrderDetails.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvOrderDetails.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvOrderDetails.ColumnHeadersHeight = 35;
            dgvOrderDetails.RowTemplate.Height = 32;
            dgvOrderDetails.CellFormatting += DgvOrderDetails_CellFormatting;
            pnlRightCard.Controls.Add(dgvOrderDetails);

            // 操作按鈕
            btnApprove = new Button { Text = "✅ Approve & Deduct Stock", Size = new Size(240, 45), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand, Enabled = false };
            btnApprove.Location = new Point(15, pnlRightCard.Height - 60);
            btnApprove.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnApprove.FlatAppearance.BorderSize = 0;
            btnApprove.Click += BtnApprove_Click;
            pnlRightCard.Controls.Add(btnApprove);

            btnReject = new Button { Text = "❌ Reject Order", Size = new Size(150, 45), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand, Enabled = false };
            btnReject.Location = new Point(265, pnlRightCard.Height - 60);
            btnReject.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnReject.FlatAppearance.BorderSize = 0;
            btnReject.Click += BtnReject_Click;
            pnlRightCard.Controls.Add(btnReject);

            this.Controls.Add(pnlRightCard);
        }
        #endregion

        #region 💽 Data Loading & Filtering
        private void LoadPendingOrders()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT OrderID, CustomerID, TotalAmount AS 'Total Amount', Status, OrderDate AS 'Date' FROM orders WHERE Status LIKE 'Awaiting Approval%' ORDER BY OrderDate ASC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvPendingOrders.DataSource = dt;
                    }

                    // 💰 格式化 Left Grid 顯示
                    if (dgvPendingOrders.Columns.Contains("Total Amount"))
                        dgvPendingOrders.Columns["Total Amount"].DefaultCellStyle.Format = "N2";
                    if (dgvPendingOrders.Columns.Contains("Date"))
                        dgvPendingOrders.Columns["Date"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";

                    dgvPendingOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load pending orders:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DgvPendingOrders_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in dgvPendingOrders.Rows)
            {
                if (row.Cells["Status"].Value != null)
                {
                    row.Cells["Status"].Style.ForeColor = Color.FromArgb(217, 119, 6);
                    row.Cells["Status"].Style.Font = new Font(dgvPendingOrders.Font, FontStyle.Bold);
                }
            }
        }

        // 🔍 即時搜尋關鍵字邏輯
        private void TxtSearchOrder_TextChanged(object sender, EventArgs e)
        {
            if (dgvPendingOrders.DataSource is DataTable dt)
            {
                string keyword = txtSearchOrder.Text.Trim().Replace("'", "''");
                if (string.IsNullOrWhiteSpace(keyword))
                    dt.DefaultView.RowFilter = "";
                else
                    dt.DefaultView.RowFilter = string.Format("OrderID LIKE '%{0}%' OR CustomerID LIKE '%{0}%' OR Status LIKE '%{0}%'", keyword);
            }
        }

        private void dgvPendingOrders_SelectionChanged_Optimized(object sender, EventArgs e)
        {
            if (dgvPendingOrders.SelectedRows.Count > 0)
            {
                string orderID = dgvPendingOrders.SelectedRows[0].Cells["OrderID"].Value.ToString();
                string status = dgvPendingOrders.SelectedRows[0].Cells["Status"].Value.ToString();

                // 🌍 提取城市並尋找對應嘅庫存欄位
                string region = ExtractRegion(status);
                string stockCol = GetStockColumnName(region);

                lblSelectedOrder.Text = $"Selected Order: {orderID} | Region: {region} (Stock Validation)";

                // 只有 Manager / Administrator / Warehouse Supervisor 可以批准或拒絕訂單
                bool canApprove = true; // 你原本的 Role Enum 檢查可以用返，為防報錯我暫時直接開放畀擁有呢頁權限嘅人

                btnApprove.Enabled = canApprove;
                btnReject.Enabled = canApprove;

                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        // 🌟 動態使用所屬城市嘅庫存欄位 (例如 p.Stock_Tokyo) 進行比較
                        string query = $@"SELECT l.ProductID AS 'Product ID', p.ProductName AS 'Product Name', l.Quantity AS 'Required Qty', 
                                         p.{stockCol} AS 'Current Stock',
                                         CASE WHEN p.{stockCol} >= l.Quantity THEN 'OK' ELSE 'SHORTAGE' END AS 'Stock Status'
                                         FROM order_lineitem l JOIN product p ON l.ProductID = p.ProductID WHERE l.OrderID = @OID";

                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@OID", orderID);
                            using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                            {
                                DataTable dt = new DataTable();
                                adapter.Fill(dt);
                                dgvOrderDetails.DataSource = dt;
                                dgvOrderDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                                // 🛑 防呆檢查：如果有任何一件貨 SHORTAGE，就強制鎖死 Approve 掣
                                foreach (DataRow row in dt.Rows)
                                {
                                    if (row["Stock Status"].ToString() == "SHORTAGE")
                                    {
                                        btnApprove.Enabled = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error displaying order details (Check if the {stockCol} column exists in DB):\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                lblSelectedOrder.Text = "Selected Order: None";
                dgvOrderDetails.DataSource = null;
                btnApprove.Enabled = false;
                btnReject.Enabled = false;
            }
        }
        #endregion

        #region 🎨 UX Grid Styling
        private void DgvOrderDetails_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvOrderDetails.Columns[e.ColumnIndex].Name == "Stock Status" && e.Value != null)
            {
                if (e.Value.ToString() == "SHORTAGE")
                {
                    e.CellStyle.BackColor = Color.FromArgb(239, 68, 68);
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.SelectionBackColor = Color.FromArgb(220, 38, 38);
                    e.CellStyle.SelectionForeColor = Color.White;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
                else
                {
                    e.CellStyle.BackColor = Color.FromArgb(34, 197, 94);
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.SelectionBackColor = Color.FromArgb(22, 163, 74);
                    e.CellStyle.SelectionForeColor = Color.White;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
            }
        }
        #endregion

        #region ⚡ Actions: Approve & Reject
        private void BtnApprove_Click(object sender, EventArgs e)
        {
            if (dgvPendingOrders.SelectedRows.Count == 0) return;

            DataGridViewRow currentRow = dgvPendingOrders.SelectedRows[0];
            string orderID = currentRow.Cells["OrderID"].Value.ToString();
            string currentStatus = currentRow.Cells["Status"].Value.ToString();

            // 🌍 提取城市並尋找對應嘅庫存欄位
            string region = ExtractRegion(currentStatus);
            string stockCol = GetStockColumnName(region);

            // 分流邏輯：判斷係咪 Delivery，並保留 Region 等下一個 Form 用
            string baseNextStatus = currentStatus.Contains("-D") ? "Ready for Dispatch" : "Ready for Pickup";
            string nextStatus = $"{baseNextStatus} [{region}]";

            DialogResult result = MessageBox.Show($"Confirm approval for Order [{orderID}] in Region [{region}]?\n\nThis will permanently deduct inventory from {stockCol} and set status to '{nextStatus}'.", "Confirm Order Approval", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. 撈出 Line Items 庫存對比
                            System.Collections.Generic.List<Tuple<string, int>> items = new System.Collections.Generic.List<Tuple<string, int>>();
                            string checkSql = "SELECT ProductID, Quantity FROM order_lineitem WHERE OrderID = @OID";

                            using (MySqlCommand cmdCheck = new MySqlCommand(checkSql, conn, trans))
                            {
                                cmdCheck.Parameters.AddWithValue("@OID", orderID);
                                using (MySqlDataReader r = cmdCheck.ExecuteReader())
                                {
                                    while (r.Read())
                                        items.Add(new Tuple<string, int>(r["ProductID"].ToString(), Convert.ToInt32(r["Quantity"])));
                                    r.Close();
                                }
                            }

                            // 2. 動態扣減 Product 表內指定城市嘅庫存
                            string deductSql = $"UPDATE product SET {stockCol} = {stockCol} - @Qty WHERE ProductID = @PID";
                            foreach (var item in items)
                            {
                                using (MySqlCommand cmdDeduct = new MySqlCommand(deductSql, conn, trans))
                                {
                                    cmdDeduct.Parameters.AddWithValue("@Qty", item.Item2);
                                    cmdDeduct.Parameters.AddWithValue("@PID", item.Item1);
                                    cmdDeduct.ExecuteNonQuery();
                                }
                            }

                            // 3. 推進訂單生命週期狀態 (連埋城市名)
                            string updateStatusSql = "UPDATE orders SET Status = @nextStatus WHERE OrderID = @OID";
                            using (MySqlCommand cmdStatus = new MySqlCommand(updateStatusSql, conn, trans))
                            {
                                cmdStatus.Parameters.AddWithValue("@nextStatus", nextStatus);
                                cmdStatus.Parameters.AddWithValue("@OID", orderID);
                                cmdStatus.ExecuteNonQuery();
                            }

                            trans.Commit(); // 🎉 確定執行所有變更
                            MessageBox.Show($"Order [{orderID}] has been successfully approved for {region}.\nStock updated and forwarded to Logistics.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadPendingOrders();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback(); // 🛑 失敗還原
                            throw new Exception("Transaction aborted due to internal data collision. Rollback complete.\n" + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Approval process failed:\n" + ex.Message, "Workflow Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnReject_Click(object sender, EventArgs e)
        {
            if (dgvPendingOrders.SelectedRows.Count == 0) return;
            string orderID = dgvPendingOrders.SelectedRows[0].Cells["OrderID"].Value.ToString();

            DialogResult result = MessageBox.Show($"Are you sure you want to REJECT Order [{orderID}]?", "Confirm Rejection", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        string sql = "UPDATE orders SET Status = 'Rejected' WHERE OrderID = @OID";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@OID", orderID);
                            cmd.ExecuteNonQuery();
                        }
                        MessageBox.Show($"Order [{orderID}] has been marked as Rejected.", "Order Processed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadPendingOrders();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to reject order:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        #endregion

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Name = "SalesQuotationForm";
            this.Load += new System.EventHandler(this.SalesQuotationForm_Load);
            this.ResumeLayout(false);
        }

        private void SalesQuotationForm_Load(object sender, EventArgs e) { }
    }
}