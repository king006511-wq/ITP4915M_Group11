using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class SalesActivityLogForm : BaseForm
    {
        // 🔒 Database Configuration
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // 🎨 UI Elements
        private DataGridView dgvLogs;
        private TextBox txtSearch;
        private Button btnCancelOrder;

        public SalesActivityLogForm()
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                SetupUI();
                EnforceSecurityGatekeeper(); // 🛡️ 執行權限攔截
                LoadData();
            }
        }

        #region 🌍 Multi-City Inventory Helpers
        // 🌟 解析訂單所屬城市
        private string ExtractRegion(string status)
        {
            if (string.IsNullOrEmpty(status)) return "Hong Kong";
            string lowerStatus = status.ToLower();

            if (lowerStatus.Contains("tokyo")) return "Tokyo";
            if (lowerStatus.Contains("singapore")) return "Singapore";
            if (lowerStatus.Contains("new york") || lowerStatus.Contains("ny")) return "New York";
            if (lowerStatus.Contains("london")) return "London";

            return "Hong Kong"; // 預設為香港
        }

        // 🌟 根據城市動態映射到 Database 嘅庫存欄位
        private string GetStockColumnName(string region)
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

        #region 🔒 System Security Gatekeeper
        private void EnforceSecurityGatekeeper()
        {
            string currentRole = UserSession.LoggedInStaffRole;

            // 限制只有 Manager, Admin 或者 Sales 可以睇同操作銷售紀錄
            bool isAuthorized = !string.IsNullOrEmpty(currentRole) &&
                                (currentRole.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                                 currentRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase) ||
                                 currentRole.Equals("Sales Representative", StringComparison.OrdinalIgnoreCase) ||
                                 currentRole.Equals("Sales", StringComparison.OrdinalIgnoreCase));

            if (!isAuthorized)
            {
                MessageBox.Show(
                    $"[SECURITY ALERT] Access Denied!\n\n" +
                    $"Your Account Role is: \"{(string.IsNullOrEmpty(currentRole) ? "None" : currentRole)}\"\n\n" +
                    $"Only Sales Representatives or Management can access the Sales Activity Log.",
                    "System Security Guard",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop
                );

                this.Shown += (s, e) => this.Close();
            }
        }
        #endregion

        #region 🎨 UI Setup
        private void SetupUI()
        {
            this.BackColor = ThemeManager.PrimaryBackground;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;

            // 1. Header Panel
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White
            };
            pnlHeader.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlHeader.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

            Label lblTitle = new Label
            {
                Text = "📜 Sales Activity Log & Order Management",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(30, 25),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);
            this.Controls.Add(pnlHeader);

            // 2. 🔍 工具列區塊 (Search & Cancel)
            Label lblSearch = new Label { Text = "Search Order:", Location = new Point(30, 100), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            this.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Location = new Point(140, 98),
                Width = 300,
                Font = new Font("Segoe UI", 10.5F),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += TxtSearch_TextChanged; // 綁定動態搜尋
            this.Controls.Add(txtSearch);

            btnCancelOrder = new Button
            {
                Text = "🚫 Cancel Selected Order",
                Location = new Point(460, 95),
                Size = new Size(200, 32),
                BackColor = Color.FromArgb(239, 68, 68), // Red
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancelOrder.FlatAppearance.BorderSize = 0;
            btnCancelOrder.Click += BtnCancelOrder_Click; // 綁定取消訂單事件
            this.Controls.Add(btnCancelOrder);

            // 3. DataGrid Container Panel
            Panel pnlGrid = new Panel
            {
                Location = new Point(30, 140),
                Width = this.Width - 60,
                Height = this.Height - 170,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.White
            };
            pnlGrid.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlGrid.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

            // 4. Modern DataGridView
            dgvLogs = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(pnlGrid.Width - 40, pnlGrid.Height - 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(241, 245, 249)
            };

            // Grid Styling
            dgvLogs.EnableHeadersVisualStyles = false;
            dgvLogs.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvLogs.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvLogs.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvLogs.ColumnHeadersHeight = 40;
            dgvLogs.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            dgvLogs.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 231, 255);
            dgvLogs.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
            dgvLogs.RowTemplate.Height = 35;

            dgvLogs.CellFormatting += DgvLogs_CellFormatting; // 綁定資料格式化與顏色

            pnlGrid.Controls.Add(dgvLogs);
            this.Controls.Add(pnlGrid);
        }
        #endregion

        #region 💽 Data Loading & Searching
        private void LoadData()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            OrderID AS 'Order Ref', 
                            CustomerID AS 'Customer', 
                            TotalAmount AS 'Amount (HK$)', 
                            Status, 
                            OrderDate AS 'Date' 
                        FROM orders 
                        ORDER BY OrderDate DESC";

                    using (MySqlDataAdapter da = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvLogs.DataSource = dt;
                    }

                    // 💰 設定欄位格式 (數字與日期)
                    if (dgvLogs.Columns.Contains("Amount (HK$)"))
                        dgvLogs.Columns["Amount (HK$)"].DefaultCellStyle.Format = "N2"; // $1,234.50

                    if (dgvLogs.Columns.Contains("Date"))
                        dgvLogs.Columns["Date"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm"; // 2026-06-30 14:30
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load sales activity logs:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 🔍 動態搜尋邏輯
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (dgvLogs.DataSource is DataTable dt)
            {
                string keyword = txtSearch.Text.Trim().Replace("'", "''"); // 防止簡單 SQL 錯誤
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    dt.DefaultView.RowFilter = "";
                }
                else
                {
                    // 支援搜尋訂單號碼、客人 ID、或訂單狀態
                    dt.DefaultView.RowFilter = string.Format("[Order Ref] LIKE '%{0}%' OR [Customer] LIKE '%{0}%' OR Status LIKE '%{0}%'", keyword);
                }
            }
        }
        #endregion

        #region 🚫 Cancel Order Logic (自動還原專屬城市庫存)
        private void BtnCancelOrder_Click(object sender, EventArgs e)
        {
            if (dgvLogs.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an order from the list to cancel.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = dgvLogs.SelectedRows[0];
            string orderID = row.Cells["Order Ref"].Value.ToString();
            string currentStatus = row.Cells["Status"].Value.ToString();

            // 防呆：防止重複取消或取消已完成的訂單 (使用 IndexOf 以防包含城市名)
            if (currentStatus.IndexOf("Cancelled", StringComparison.OrdinalIgnoreCase) >= 0 ||
                currentStatus.IndexOf("Completed", StringComparison.OrdinalIgnoreCase) >= 0 ||
                currentStatus.IndexOf("Delivered", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                MessageBox.Show($"Order [{orderID}] cannot be cancelled because its current status is '{currentStatus}'.", "Invalid Action", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirm = MessageBox.Show($"Are you sure you want to CANCEL Order [{orderID}]?\n\nThis action cannot be undone.", "Confirm Cancellation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
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
                                // 🌟 1. 判斷是否需要退回庫存 
                                // (如果單仲係 Awaiting Approval 或者 Rejected，即係根本未扣過庫存，所以唔使加返去)
                                bool isStockDeducted = currentStatus.IndexOf("Awaiting", StringComparison.OrdinalIgnoreCase) < 0 &&
                                                       currentStatus.IndexOf("Rejected", StringComparison.OrdinalIgnoreCase) < 0;

                                string region = ExtractRegion(currentStatus);
                                string stockCol = GetStockColumnName(region);
                                string newStatus = $"Cancelled [{region}]";

                                // 🌟 2. 更新訂單狀態
                                string updateSql = "UPDATE orders SET Status = @newStatus WHERE OrderID = @id";
                                using (MySqlCommand cmd = new MySqlCommand(updateSql, conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@newStatus", newStatus);
                                    cmd.Parameters.AddWithValue("@id", orderID);
                                    cmd.ExecuteNonQuery();
                                }

                                // 🌟 3. 退回庫存邏輯 (精準退回專屬城市)
                                if (isStockDeducted)
                                {
                                    string getItemsSql = "SELECT ProductID, Quantity FROM order_lineitem WHERE OrderID = @id";
                                    List<Tuple<string, int>> itemsToRestore = new List<Tuple<string, int>>();

                                    using (MySqlCommand cmdItems = new MySqlCommand(getItemsSql, conn, trans))
                                    {
                                        cmdItems.Parameters.AddWithValue("@id", orderID);
                                        using (MySqlDataReader reader = cmdItems.ExecuteReader())
                                        {
                                            while (reader.Read())
                                            {
                                                itemsToRestore.Add(new Tuple<string, int>(reader["ProductID"].ToString(), Convert.ToInt32(reader["Quantity"])));
                                            }
                                        }
                                    }

                                    foreach (var item in itemsToRestore)
                                    {
                                        string restoreSql = $"UPDATE product SET {stockCol} = {stockCol} + @qty WHERE ProductID = @pid";
                                        using (MySqlCommand cmdRestore = new MySqlCommand(restoreSql, conn, trans))
                                        {
                                            cmdRestore.Parameters.AddWithValue("@qty", item.Item2);
                                            cmdRestore.Parameters.AddWithValue("@pid", item.Item1);
                                            cmdRestore.ExecuteNonQuery();
                                        }
                                    }
                                }

                                trans.Commit(); // 確認寫入

                                string msg = $"Order [{orderID}] has been successfully cancelled.";
                                if (isStockDeducted)
                                {
                                    msg += $"\n\n📦 Inventory for [{region}] has been accurately restored.";
                                }

                                MessageBox.Show(msg, "Order Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadData(); // 重新載入數據，刷新 Grid 狀態
                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                throw ex;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to cancel order:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        #endregion

        #region 🎨 UX Enhancement: Status Color Coding
        private void DgvLogs_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvLogs.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();

                // 🌟 修正：因為狀態字串尾部有城市名 (e.g. Cancelled [Hong Kong])，所以要用 IndexOf 取代 Equals
                if (status.IndexOf("Cancelled", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    e.CellStyle.ForeColor = Color.Crimson;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Strikeout | FontStyle.Bold);
                }
                else if (status.IndexOf("Completed", StringComparison.OrdinalIgnoreCase) >= 0 || status.IndexOf("Delivered", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    e.CellStyle.ForeColor = Color.MediumSeaGreen;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
                else if (status.IndexOf("Pending", StringComparison.OrdinalIgnoreCase) >= 0 || status.IndexOf("Awaiting", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    e.CellStyle.ForeColor = Color.DarkOrange;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
            }
        }
        #endregion

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SalesActivityLogForm
            // 
            this.ClientSize = new System.Drawing.Size(278, 244);
            this.Name = "SalesActivityLogForm";
            this.Load += new System.EventHandler(this.SalesActivityLogForm_Load);
            this.ResumeLayout(false);

        }

        private void SalesActivityLogForm_Load(object sender, EventArgs e)
        {

        }
    }
}