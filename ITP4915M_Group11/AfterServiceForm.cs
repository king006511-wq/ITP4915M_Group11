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
        private DataGridView dgvComplaints, dgvOrderItems;
        private Button btnSubmit, btnClear, btnVerifyOrder;
        private Label lblOrderSummary;

        // 核心容器
        private Panel pnlLeftCard;
        private Label lblGridTitle, lblItemsTitle;

        public AfterServiceForm()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                InitializeEnterpriseHelpdeskUI();
                SetupDropdowns();
                EnsureComplaintTableExists();
                GenerateNewTicketID();
                LoadComplaints();

                this.SizeChanged += AfterServiceForm_SizeChanged;
                this.Layout += (s, e) => RecalculateCustomLayout();
            }
        }

        #region 🎨 企業級 Helpdesk & RMA 排版
        private void InitializeEnterpriseHelpdeskUI()
        {
            this.Controls.Clear();
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopLevel = false;
            this.Dock = DockStyle.Fill;

            // =========================================================
            // 【左側】Helpdesk Ticket 操作卡片
            // =========================================================
            pnlLeftCard = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };
            pnlLeftCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlLeftCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            this.Controls.Add(pnlLeftCard);

            Label lblCardTitle = new Label { Text = "🎧 Helpdesk Ticket Action", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(79, 70, 229), Location = new Point(20, 20), AutoSize = true };
            pnlLeftCard.Controls.Add(lblCardTitle);

            int startY = 70;
            int inputWidth = 330;

            txtComplaintID = CreateStyledTextBox(pnlLeftCard, ref startY, "Ticket ID (Auto):", true, inputWidth);

            // 🌟 訂單驗證區 (Order Verification)
            Label lblOrder = new Label { Text = "Related Order ID *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtOrderID = new TextBox { Location = new Point(20, startY + 22), Width = 230, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            btnVerifyOrder = new Button { Text = "🔍 Lookup", Location = new Point(260, startY + 21), Size = new Size(90, 28), BackColor = Color.FromArgb(14, 165, 233), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnVerifyOrder.FlatAppearance.BorderSize = 0;
            btnVerifyOrder.Click += BtnVerifyOrder_Click;
            pnlLeftCard.Controls.Add(lblOrder); pnlLeftCard.Controls.Add(txtOrderID); pnlLeftCard.Controls.Add(btnVerifyOrder);
            startY += 65;

            txtCustomerID = CreateStyledTextBox(pnlLeftCard, ref startY, "Verified Customer ID:", true, inputWidth); // 驗證後自動填寫，防止亂打

            Label lblReqType = new Label { Text = "Support Request Type *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboRequestType = new ComboBox { Location = new Point(20, startY + 22), Width = inputWidth, Font = new Font("Segoe UI", 10.5F), DropDownStyle = ComboBoxStyle.DropDownList };
            cboRequestType.SelectedIndexChanged += CboRequestType_SelectedIndexChanged;
            pnlLeftCard.Controls.Add(lblReqType); pnlLeftCard.Controls.Add(cboRequestType);
            startY += 65;

            Label lblDetails = new Label { Text = "Issue Description / Notes:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtDetails = new TextBox { Location = new Point(20, startY + 22), Width = inputWidth, Height = 75, Multiline = true, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            pnlLeftCard.Controls.Add(lblDetails); pnlLeftCard.Controls.Add(txtDetails);
            startY += 110;

            Label lblRefund = new Label { Text = "RMA Refund Amount (HKD):", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(220, 38, 38) };
            txtRefundAmount = new TextBox { Location = new Point(20, startY + 22), Width = inputWidth, Font = new Font("Segoe UI", 11F, FontStyle.Bold), BorderStyle = BorderStyle.FixedSingle, Enabled = false, Text = "0.00", BackColor = Color.FromArgb(241, 245, 249) };
            pnlLeftCard.Controls.Add(lblRefund); pnlLeftCard.Controls.Add(txtRefundAmount);
            startY += 65;

            Label lblStatus = new Label { Text = "Resolution Status *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboStatus = new ComboBox { Location = new Point(20, startY + 22), Width = inputWidth, Font = new Font("Segoe UI", 10.5F), DropDownStyle = ComboBoxStyle.DropDownList };
            pnlLeftCard.Controls.Add(lblStatus); pnlLeftCard.Controls.Add(cboStatus);
            startY += 75;

            int btnWidth = 160;
            btnSubmit = new Button { Text = "💾 Save Ticket", Location = new Point(20, startY), Size = new Size(btnWidth, 42), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClear = new Button { Text = "✨ Clear", Location = new Point(190, startY), Size = new Size(btnWidth, 42), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSubmit.FlatAppearance.BorderSize = 0; btnClear.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += btnSubmitComplaint_Click; btnClear.Click += (s, e) => ClearFields();
            pnlLeftCard.Controls.Add(btnSubmit); pnlLeftCard.Controls.Add(btnClear);

            // =========================================================
            // 【右側】Master-Detail 雙表格設計 (Tickets + Order Context)
            // =========================================================
            lblGridTitle = new Label { Text = "📂 Support Tickets Center", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true };
            this.Controls.Add(lblGridTitle);

            txtSearchHistory = new TextBox { Width = 300, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle, ForeColor = Color.Gray, Text = "Search by Ticket or Order ID..." };
            txtSearchHistory.GotFocus += (s, e) => { if (txtSearchHistory.Text == "Search by Ticket or Order ID...") { txtSearchHistory.Text = ""; txtSearchHistory.ForeColor = Color.Black; } };
            txtSearchHistory.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearchHistory.Text)) { txtSearchHistory.Text = "Search by Ticket or Order ID..."; txtSearchHistory.ForeColor = Color.Gray; } };
            txtSearchHistory.TextChanged += txtSearchHistory_TextChanged;
            this.Controls.Add(txtSearchHistory);

            dgvComplaints = CreateCleanGrid();
            dgvComplaints.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(79, 70, 229);
            dgvComplaints.SelectionChanged += dgvComplaints_SelectionChanged;
            dgvComplaints.CellFormatting += DgvComplaints_CellFormatting;
            this.Controls.Add(dgvComplaints);

            // 🌟 訂單內容子表格 (Order Context)
            lblItemsTitle = new Label { Text = "🛍️ Purchased Items Context (Select a ticket to view)", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true };
            this.Controls.Add(lblItemsTitle);

            lblOrderSummary = new Label { Text = "Total Amount Paid: $0.00", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(16, 185, 129), AutoSize = true };
            this.Controls.Add(lblOrderSummary);

            dgvOrderItems = CreateCleanGrid();
            dgvOrderItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(71, 85, 105);
            this.Controls.Add(dgvOrderItems);
        }

        private DataGridView CreateCleanGrid()
        {
            DataGridView dgv = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 35;
            return dgv;
        }

        private void AfterServiceForm_SizeChanged(object sender, EventArgs e) { RecalculateCustomLayout(); }

        private void RecalculateCustomLayout()
        {
            if (this.Width < 200 || this.Height < 200) return;
            this.SuspendLayout();

            pnlLeftCard.Location = new Point(20, 20);
            pnlLeftCard.Size = new Size(380, this.Height - 40);

            int rightStartX = pnlLeftCard.Right + 20;
            int rightWidth = this.Width - rightStartX - 20;

            if (rightWidth > 100)
            {
                lblGridTitle.Location = new Point(rightStartX, 22);
                txtSearchHistory.Location = new Point(this.Width - txtSearchHistory.Width - 25, 22);

                int availableHeight = this.Height - 110;
                int topGridHeight = (int)(availableHeight * 0.55); // 55% 給 Tickets
                int botGridHeight = availableHeight - topGridHeight; // 45% 給 Items

                dgvComplaints.Location = new Point(rightStartX, 60);
                dgvComplaints.Size = new Size(rightWidth, topGridHeight);

                int bottomSectionY = dgvComplaints.Bottom + 15;
                lblItemsTitle.Location = new Point(rightStartX, bottomSectionY);
                lblOrderSummary.Location = new Point(this.Width - lblOrderSummary.Width - 25, bottomSectionY);

                dgvOrderItems.Location = new Point(rightStartX, bottomSectionY + 30);
                dgvOrderItems.Size = new Size(rightWidth, botGridHeight);
            }
            this.ResumeLayout(false);
        }

        private TextBox CreateStyledTextBox(Panel container, ref int topY, string labelText, bool readOnly, int width)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(20, topY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(20, topY + 22), Width = width, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            if (readOnly) { txt.ReadOnly = true; txt.BackColor = Color.FromArgb(241, 245, 249); }
            container.Controls.Add(lbl); container.Controls.Add(txt);
            topY += 65;
            return txt;
        }

        private void CboRequestType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboRequestType.Text.Contains("Refund") || cboRequestType.Text.Contains("Return"))
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

        #region 💾 企業級智能商業邏輯
        private void SetupDropdowns()
        {
            cboRequestType.Items.Clear();
            cboRequestType.Items.AddRange(new string[] { "General Complaint", "Return & Refund", "Product Replacement", "Partial Refund" });

            cboStatus.Items.Clear();
            cboStatus.Items.AddRange(new string[] { "Pending Investigation", "In Progress", "Resolved", "Refunded", "Closed" });
        }

        private void GenerateNewTicketID()
        {
            txtComplaintID.Text = "TKT-" + DateTime.Now.ToString("yyyyMMdd-HHmm");
        }

        // 🌟 訂單智能驗證：輸入 Order ID，自動去 DB 搵 CustomerID 出嚟，確保單號真實存在！
        private void BtnVerifyOrder_Click(object sender, EventArgs e)
        {
            string searchOID = txtOrderID.Text.Trim();
            if (string.IsNullOrWhiteSpace(searchOID)) { MessageBox.Show("Please enter an Order ID to lookup.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT CustomerID, TotalAmount FROM orders WHERE OrderID = @OID";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@OID", searchOID);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtCustomerID.Text = reader["CustomerID"].ToString();
                                MessageBox.Show($"Order Verified!\nCustomer ID: {txtCustomerID.Text}\nOrder Total: ${Convert.ToDecimal(reader["TotalAmount"]):N2}", "Order Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadOrderContextItems(searchOID); // 即刻顯示買咗咩
                            }
                            else
                            {
                                MessageBox.Show("Order ID not found in the system. Please verify with the customer.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                txtCustomerID.Clear();
                                dgvOrderItems.DataSource = null;
                            }
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Database Error: " + ex.Message); }
            }
        }

        // 🌟 載入該訂單嘅商品明細 (Order Context)
        private void LoadOrderContextItems(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId)) { dgvOrderItems.DataSource = null; lblOrderSummary.Text = "Total Amount Paid: $0.00"; return; }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // 1. 獲取訂單總額
                    MySqlCommand cmdSum = new MySqlCommand("SELECT TotalAmount FROM orders WHERE OrderID = @OID", conn);
                    cmdSum.Parameters.AddWithValue("@OID", orderId);
                    object totalObj = cmdSum.ExecuteScalar();
                    if (totalObj != null && totalObj != DBNull.Value) lblOrderSummary.Text = $"Total Amount Paid: ${Convert.ToDecimal(totalObj):N2}";

                    // 2. 獲取購買明細
                    string query = @"SELECT p.ProductName AS 'Product Name', 
                                            l.Quantity AS 'Qty', 
                                            l.UnitPrice AS 'Unit Price ($)', 
                                            (l.Quantity * l.UnitPrice) AS 'Subtotal ($)' 
                                     FROM order_lineitem l 
                                     JOIN product p ON l.ProductID = p.ProductID 
                                     WHERE l.OrderID = @OID";
                    using (MySqlDataAdapter da = new MySqlDataAdapter(query, conn))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@OID", orderId);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvOrderItems.DataSource = dt;
                        dgvOrderItems.ClearSelection();
                    }
                }
                catch (Exception) { dgvOrderItems.DataSource = null; }
            }
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
                          `Status` varchar(30) NOT NULL,
                          PRIMARY KEY (`ComplaintID`)
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
                    using (MySqlCommand cmd = new MySqlCommand(createTableQuery, conn)) { cmd.ExecuteNonQuery(); }

                    try { new MySqlCommand("ALTER TABLE `complaint` ADD COLUMN `RequestType` varchar(50) DEFAULT 'General Complaint';", conn).ExecuteNonQuery(); } catch { }
                    try { new MySqlCommand("ALTER TABLE `complaint` ADD COLUMN `Description` text;", conn).ExecuteNonQuery(); } catch { }
                    try { new MySqlCommand("ALTER TABLE `complaint` ADD COLUMN `RefundAmount` decimal(10,2) DEFAULT 0.00;", conn).ExecuteNonQuery(); } catch { }
                }
                catch (Exception) { }
            }
        }

        private void LoadComplaints()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT ComplaintID AS 'Ticket ID', OrderID AS 'Order Ref', CustomerID AS 'Customer', RequestType AS 'Type', RefundAmount AS 'Refund HKD', Date, Status, Description FROM complaint ORDER BY Date DESC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        originalComplaintsTable = new DataTable();
                        adapter.Fill(originalComplaintsTable);
                        dgvComplaints.DataSource = originalComplaintsTable;
                    }
                    if (dgvComplaints.Columns.Contains("Description")) dgvComplaints.Columns["Description"].Visible = false;

                    if (dgvComplaints.Columns.Contains("Date")) dgvComplaints.Columns["Date"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
                    if (dgvComplaints.Columns.Contains("Refund HKD")) dgvComplaints.Columns["Refund HKD"].DefaultCellStyle.Format = "N2";

                    dgvComplaints.ClearSelection();
                }
                catch (Exception ex) { MessageBox.Show("Database Load Error: " + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        // 🌟 狀態視覺化 (Status Color Coding)
        private void DgvComplaints_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvComplaints.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status.Contains("Pending"))
                {
                    e.CellStyle.ForeColor = Color.FromArgb(234, 88, 12); // Orange
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
                else if (status == "Resolved" || status == "Refunded")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129); // Green
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
                else if (status == "Closed")
                {
                    e.CellStyle.ForeColor = Color.Gray;
                }
            }
        }

        private void txtSearchHistory_TextChanged(object sender, EventArgs e)
        {
            if (dgvComplaints.DataSource is DataTable dt && txtSearchHistory.Text != "Search by Ticket or Order ID...")
            {
                string keyword = txtSearchHistory.Text.Trim().Replace("'", "''");
                if (string.IsNullOrWhiteSpace(keyword)) dt.DefaultView.RowFilter = "";
                else dt.DefaultView.RowFilter = string.Format("[Ticket ID] LIKE '%{0}%' OR [Order Ref] LIKE '%{0}%'", keyword);
            }
        }

        private void dgvComplaints_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvComplaints.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvComplaints.SelectedRows[0];
                txtComplaintID.Text = row.Cells["Ticket ID"].Value?.ToString() ?? "";
                txtOrderID.Text = row.Cells["Order Ref"].Value?.ToString() ?? "";
                txtCustomerID.Text = row.Cells["Customer"].Value?.ToString() ?? "";

                string reqType = row.Cells["Type"].Value?.ToString()?.Trim() ?? "General Complaint";
                cboRequestType.Text = reqType;

                txtRefundAmount.Text = row.Cells["Refund HKD"].Value?.ToString() ?? "0.00";
                txtDetails.Text = row.Cells["Description"].Value?.ToString() ?? "";

                string status = row.Cells["Status"].Value?.ToString();
                if (cboStatus.Items.Contains(status)) cboStatus.Text = status;

                // 自動載入該 Ticket 對應嘅訂單明細
                LoadOrderContextItems(txtOrderID.Text.Trim());
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
                MessageBox.Show("Please complete the required fields (Customer ID, Request Type, Status). Tip: Use the 'Lookup' button to fetch Customer ID automatically from Order ID.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                    MessageBox.Show("Support Ticket has been successfully saved!", "Transaction Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            dgvOrderItems.DataSource = null;
            lblOrderSummary.Text = "Total Amount Paid: $0.00";
            GenerateNewTicketID();
        }
        #endregion
    }
}