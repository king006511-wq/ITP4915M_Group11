using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class SalesActivityLogForm : Form
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
            this.BackColor = Color.FromArgb(243, 244, 246);
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

        // 🔍 動態搜尋邏輯 (符合 "Search Order" 功課要求)
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

        #region 🚫 Cancel Order Logic (符合 "Cancel Order" 功課要求)
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

            // 防呆：防止重複取消或取消已完成的訂單
            if (currentStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) ||
                currentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
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
                        string updateSql = "UPDATE orders SET Status = 'Cancelled' WHERE OrderID = @id";
                        using (MySqlCommand cmd = new MySqlCommand(updateSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", orderID);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show($"Order [{orderID}] has been successfully cancelled.", "Order Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadData(); // 重新載入數據，刷新 Grid 狀態
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

                if (status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = Color.Crimson;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Strikeout | FontStyle.Bold);
                }
                else if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = Color.MediumSeaGreen;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
                else if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase) || status.Equals("Processing", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = Color.DarkOrange;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
            }
        }
        #endregion
    }
}