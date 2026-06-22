using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class AfterServiceForm : Form
    {
        // ==========================================
        // 🔒 Database Configuration
        // ==========================================
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";
        private DataTable originalComplaintsTable = new DataTable();

        // ==========================================
        // 🎨 UI Controls
        // ==========================================
        private TextBox txtComplaintID, txtCustomerID, txtOrderID, txtSearchHistory;
        private TextBox txtDetails, txtRefundAmount;
        private ComboBox cboStatus, cboRequestType;
        private DataGridView dgvComplaints;
        private Button btnSubmit, btnClear;

        // 核心卡片容器
        private Panel pnlLeftCard;
        private Label lblGridTitle;

        public AfterServiceForm()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                InitializeBulletproofUI(); // 🚀 啟用全新「動態座標公式」排版
                SetupDropdowns();
                EnsureComplaintTableExists();
                GenerateNewTicketID();
                LoadComplaints();

                // 🌟 核心防禦：綁定 Layout 同 SizeChanged，無論點放大縮小都由程式碼手動計算像素大細
                this.SizeChanged += AfterServiceForm_SizeChanged;
                this.Layout += (s, e) => RecalculateCustomLayout();
            }
        }

        #region 🎨 暴力手動算繪排版 (徹底消滅白布遮擋、表格隱形 Bug)
        private void InitializeBulletproofUI()
        {
            this.Controls.Clear();
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopLevel = false;
            this.Dock = DockStyle.Fill; // 填滿 Main Form 嘅容器 Panel

            // =========================================================
            // 【左側】獨立資料輸入白底卡片面板
            // =========================================================
            pnlLeftCard = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };
            pnlLeftCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlLeftCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            this.Controls.Add(pnlLeftCard);

            Label lblCardTitle = new Label { Text = "📝 Support Ticket Details", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(79, 70, 229), Location = new Point(20, 20), AutoSize = true };
            pnlLeftCard.Controls.Add(lblCardTitle);

            int startY = 70;
            int inputWidth = 330; // 固定輸入框闊度

            txtComplaintID = CreateStyledTextBox(pnlLeftCard, ref startY, "Ticket / Complaint ID (Auto):", true, inputWidth);
            txtCustomerID = CreateStyledTextBox(pnlLeftCard, ref startY, "Customer ID *:", false, inputWidth);
            txtOrderID = CreateStyledTextBox(pnlLeftCard, ref startY, "Related Order ID (Optional):", false, inputWidth);

            Label lblReqType = new Label { Text = "Request Type *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboRequestType = new ComboBox { Location = new Point(20, startY + 22), Width = inputWidth, Font = new Font("Segoe UI", 10.5F), DropDownStyle = ComboBoxStyle.DropDownList };
            cboRequestType.SelectedIndexChanged += CboRequestType_SelectedIndexChanged;
            pnlLeftCard.Controls.Add(lblReqType); pnlLeftCard.Controls.Add(cboRequestType);
            startY += 70;

            Label lblDetails = new Label { Text = "Complaint Details / Records:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtDetails = new TextBox { Location = new Point(20, startY + 22), Width = inputWidth, Height = 80, Multiline = true, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            pnlLeftCard.Controls.Add(lblDetails); pnlLeftCard.Controls.Add(txtDetails);
            startY += 115;

            Label lblRefund = new Label { Text = "Refund Amount ($):", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtRefundAmount = new TextBox { Location = new Point(20, startY + 22), Width = inputWidth, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle, Enabled = false, Text = "0.00", BackColor = Color.FromArgb(241, 245, 249) };
            pnlLeftCard.Controls.Add(lblRefund); pnlLeftCard.Controls.Add(txtRefundAmount);
            startY += 70;

            Label lblStatus = new Label { Text = "Resolution Status *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboStatus = new ComboBox { Location = new Point(20, startY + 22), Width = inputWidth, Font = new Font("Segoe UI", 10.5F), DropDownStyle = ComboBoxStyle.DropDownList };
            pnlLeftCard.Controls.Add(lblStatus); pnlLeftCard.Controls.Add(cboStatus);
            startY += 70;

            int btnWidth = 160;
            btnSubmit = new Button { Text = "💾 Save Ticket", Location = new Point(20, startY), Size = new Size(btnWidth, 40), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClear = new Button { Text = "✨ Reset", Location = new Point(190, startY), Size = new Size(btnWidth, 40), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSubmit.FlatAppearance.BorderSize = 0; btnClear.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += btnSubmitComplaint_Click; btnClear.Click += (s, e) => ClearFields();
            pnlLeftCard.Controls.Add(btnSubmit); pnlLeftCard.Controls.Add(btnClear);

            // =========================================================
            // 【右側】元件直接置頂（不嵌套 Panel，防止 layout 崩塌）
            // =========================================================
            lblGridTitle = new Label { Text = "📂 Support Tickets History", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true };
            this.Controls.Add(lblGridTitle);

            txtSearchHistory = new TextBox { Width = 260, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            txtSearchHistory.Text = "Type ID to search...";
            txtSearchHistory.ForeColor = Color.Gray;
            txtSearchHistory.GotFocus += (s, e) => { if (txtSearchHistory.Text == "Type ID to search...") { txtSearchHistory.Text = ""; txtSearchHistory.ForeColor = Color.Black; } };
            txtSearchHistory.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearchHistory.Text)) { txtSearchHistory.Text = "Type ID to search..."; txtSearchHistory.ForeColor = Color.Gray; } };
            txtSearchHistory.TextChanged += txtSearchHistory_TextChanged;
            this.Controls.Add(txtSearchHistory);

            // 數據網格表格直接掛載到表單，保證享有第一級別尺寸調整權限
            dgvComplaints = new DataGridView
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
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                EnableHeadersVisualStyles = false
            };

            dgvComplaints.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvComplaints.DefaultCellStyle.Padding = new Padding(10);
            dgvComplaints.DefaultCellStyle.Font = new Font("Segoe UI", 10F);

            dgvComplaints.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(79, 70, 229);
            dgvComplaints.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvComplaints.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvComplaints.ColumnHeadersDefaultCellStyle.Padding = new Padding(10);
            dgvComplaints.ColumnHeadersHeight = 45;

            dgvComplaints.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvComplaints.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);

            dgvComplaints.SelectionChanged += dgvComplaints_SelectionChanged;
            this.Controls.Add(dgvComplaints);
        }

        private void AfterServiceForm_SizeChanged(object sender, EventArgs e)
        {
            RecalculateCustomLayout();
        }

        /// <summary>
        /// 🛠️ 暴力手動佈局計算法：避開所有 WinForms 內置 Docking 漏洞，強行用數學擴展所有容器
        /// </summary>
        private void RecalculateCustomLayout()
        {
            if (this.Width < 200 || this.Height < 200) return;

            this.SuspendLayout();

            // 1. 強指定左側表單位置與高度
            pnlLeftCard.Location = new Point(20, 20);
            pnlLeftCard.Size = new Size(380, this.Height - 40);

            // 2. 計算右側可用範圍起始點
            int rightStartX = pnlLeftCard.Right + 20;
            int rightWidth = this.Width - rightStartX - 20;

            if (rightWidth > 100)
            {
                // 3. 強行鎖死右側標題與搜尋欄
                lblGridTitle.Location = new Point(rightStartX, 22);
                txtSearchHistory.Location = new Point(this.Width - txtSearchHistory.Width - 25, 22);

                // 4. 🔥 核心：用絕對數學計算指定 DataGridView 寬高，逼迫佢食盡賸餘右邊全部畫面！
                dgvComplaints.Location = new Point(rightStartX, 65);
                dgvComplaints.Size = new Size(rightWidth, this.Height - 85);

                // 5. 每次大細變更，都通知 DataGridView 重新刷滿每一列
                if (dgvComplaints.Columns.Count > 0)
                {
                    dgvComplaints.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                    dgvComplaints.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }

            this.ResumeLayout(false);
        }

        private TextBox CreateStyledTextBox(Panel container, ref int topY, string labelText, bool readOnly, int width)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(20, topY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(20, topY + 22), Width = width, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            if (readOnly) { txt.ReadOnly = true; txt.BackColor = Color.FromArgb(241, 245, 249); }
            container.Controls.Add(lbl); container.Controls.Add(txt);
            topY += 70;
            return txt;
        }

        private void CboRequestType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.Equals(cboRequestType.Text.Trim(), "Refund", StringComparison.OrdinalIgnoreCase))
            {
                txtRefundAmount.Enabled = true;
                txtRefundAmount.BackColor = Color.White;
            }
            else
            {
                txtRefundAmount.Enabled = false;
                txtRefundAmount.BackColor = Color.FromArgb(241, 245, 249);
                txtRefundAmount.Text = "0.00";
            }
        }
        #endregion

        #region 💾 資料庫商業邏輯層
        private void SetupDropdowns()
        {
            cboRequestType.Items.Clear();
            cboRequestType.Items.AddRange(new string[] { "General Complaint", "Return", "Replacement", "Refund" });

            cboStatus.Items.Clear();
            cboStatus.Items.AddRange(new string[] { "Pending", "In Progress", "Resolved", "Refunded", "Closed" });
        }

        private void GenerateNewTicketID()
        {
            txtComplaintID.Text = "COMP-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }

        private void EnsureComplaintTableExists()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS `complaint` (
                          `ComplaintID` varchar(30) NOT NULL,
                          `CustomerID` varchar(15) NOT NULL,
                          `OrderID` varchar(20) DEFAULT NULL,
                          `Date` datetime NOT NULL,
                          `Status` varchar(20) NOT NULL,
                          PRIMARY KEY (`ComplaintID`)
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
                    using (MySqlCommand cmd = new MySqlCommand(createTableQuery, conn)) { cmd.ExecuteNonQuery(); }

                    try { new MySqlCommand("ALTER TABLE `complaint` ADD COLUMN `RequestType` varchar(50) DEFAULT 'General Complaint';", conn).ExecuteNonQuery(); } catch { }
                    try { new MySqlCommand("ALTER TABLE `complaint` ADD COLUMN `Description` text;", conn).ExecuteNonQuery(); } catch { }
                    try { new MySqlCommand("ALTER TABLE `complaint` ADD COLUMN `RefundAmount` decimal(10,2) DEFAULT 0.00;", conn).ExecuteNonQuery(); } catch { }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to verify system tables:\n" + ex.Message, "Database Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void LoadComplaints()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT ComplaintID, CustomerID, OrderID, RequestType, RefundAmount, Date, Status, Description FROM complaint ORDER BY Date DESC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        originalComplaintsTable = new DataTable();
                        adapter.Fill(originalComplaintsTable);

                        dgvComplaints.DataSource = originalComplaintsTable;
                    }

                    if (dgvComplaints.Columns.Contains("Description")) dgvComplaints.Columns["Description"].Visible = false;

                    // 防禦性定義每一列嘅標頭與基本寬度大細
                    SetColumnStyle("ComplaintID", "Ticket ID", 150);
                    SetColumnStyle("CustomerID", "Customer ID", 110);
                    SetColumnStyle("OrderID", "Order ID", 110);
                    SetColumnStyle("RequestType", "Request Type", 130);
                    SetColumnStyle("Status", "Status", 100);

                    if (dgvComplaints.Columns.Contains("RefundAmount"))
                    {
                        dgvComplaints.Columns["RefundAmount"].HeaderText = "Refund ($)";
                        dgvComplaints.Columns["RefundAmount"].DefaultCellStyle.Format = "F2";
                        dgvComplaints.Columns["RefundAmount"].MinimumWidth = 100;
                    }

                    if (dgvComplaints.Columns.Contains("Date"))
                    {
                        dgvComplaints.Columns["Date"].HeaderText = "Created Date";
                        dgvComplaints.Columns["Date"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
                        dgvComplaints.Columns["Date"].MinimumWidth = 140;
                    }

                    RecalculateCustomLayout();
                    dgvComplaints.ClearSelection();
                }
                catch (Exception ex) { MessageBox.Show("Database Load Error: " + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void SetColumnStyle(string colName, string headerText, int minWidth)
        {
            if (dgvComplaints.Columns.Contains(colName))
            {
                dgvComplaints.Columns[colName].HeaderText = headerText;
                dgvComplaints.Columns[colName].MinimumWidth = minWidth;
            }
        }

        private void txtSearchHistory_TextChanged(object sender, EventArgs e)
        {
            if (dgvComplaints.DataSource is DataTable dt && txtSearchHistory.Text != "Type ID to search...")
            {
                string keyword = txtSearchHistory.Text.Trim().Replace("'", "''");
                if (string.IsNullOrWhiteSpace(keyword))
                    dt.DefaultView.RowFilter = "";
                else
                    dt.DefaultView.RowFilter = string.Format("ComplaintID LIKE '%{0}%' OR CustomerID LIKE '%{0}%' OR OrderID LIKE '%{0}%'", keyword);
            }
        }

        private void dgvComplaints_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvComplaints.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvComplaints.SelectedRows[0];
                txtComplaintID.Text = row.Cells["ComplaintID"].Value?.ToString() ?? "";
                txtCustomerID.Text = row.Cells["CustomerID"].Value?.ToString() ?? "";
                txtOrderID.Text = row.Cells["OrderID"].Value?.ToString() ?? "";

                string reqType = row.Cells["RequestType"].Value?.ToString()?.Trim() ?? "General Complaint";
                cboRequestType.Text = reqType;

                txtRefundAmount.Text = row.Cells["RefundAmount"].Value?.ToString() ?? "0.00";
                txtDetails.Text = row.Cells["Description"].Value?.ToString() ?? "";

                string status = row.Cells["Status"].Value?.ToString();
                if (cboStatus.Items.Contains(status)) cboStatus.Text = status;
            }
        }

        private void btnSubmitComplaint_Click(object sender, EventArgs e)
        {
            string customerID = txtCustomerID.Text.Trim();
            string orderID = txtOrderID.Text.Trim();

            string cleanAmt = txtRefundAmount.Text.Replace("$", "").Trim();
            decimal.TryParse(cleanAmt, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal refundAmt);

            if (string.IsNullOrWhiteSpace(customerID) || string.IsNullOrWhiteSpace(cboStatus.Text) || string.IsNullOrWhiteSpace(cboRequestType.Text))
            {
                MessageBox.Show("Please provide Customer ID, Request Type, and Resolution Status!", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    string sql = @"INSERT INTO complaint (ComplaintID, CustomerID, OrderID, RequestType, Description, RefundAmount, Date, Status) 
                                   VALUES (@CID, @CustID, @OID, @ReqType, @Desc, @RefundAmt, NOW(), @Status)
                                   ON DUPLICATE KEY UPDATE Status = @Status, OrderID = @OID, RequestType = @ReqType, Description = @Desc, RefundAmount = @RefundAmt;";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CID", txtComplaintID.Text.Trim());
                        cmd.Parameters.AddWithValue("@CustID", customerID);
                        cmd.Parameters.AddWithValue("@OID", string.IsNullOrWhiteSpace(orderID) ? (object)DBNull.Value : orderID);
                        cmd.Parameters.AddWithValue("@ReqType", cboRequestType.Text);
                        cmd.Parameters.AddWithValue("@Desc", txtDetails.Text.Trim());
                        cmd.Parameters.AddWithValue("@RefundAmt", refundAmt);
                        cmd.Parameters.AddWithValue("@Status", cboStatus.Text);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Support Ticket has been successfully saved/updated!", "Transaction Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                    LoadComplaints();
                }
                catch (Exception ex) { MessageBox.Show("Failed to save the ticket:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void ClearFields()
        {
            cboRequestType.SelectedIndex = -1;
            cboStatus.SelectedIndex = -1;
            txtCustomerID.Clear();
            txtOrderID.Clear();
            txtDetails.Clear();
            txtRefundAmount.Text = "0.00";
            txtRefundAmount.Enabled = false;
            txtRefundAmount.BackColor = Color.FromArgb(241, 245, 249);
            dgvComplaints.ClearSelection();
            GenerateNewTicketID();
        }
        #endregion
    }
}