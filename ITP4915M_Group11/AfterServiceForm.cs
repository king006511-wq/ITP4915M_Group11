using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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
        private TextBox txtComplaintID, txtSearchHistory;
        private TextBox txtDetails, txtRefundAmount;
        // 🌟 更新：將 Customer 同 Order 轉為 ComboBox
        private ComboBox cboCustomer, cboOrderID, cboStatus, cboRequestType;
        private DataGridView dgvComplaints, dgvOrderItems;
        private Button btnSubmit, btnClear;
        private Label lblOrderSummary;

        private Panel pnlLeftCard;
        private Label lblGridTitle, lblItemsTitle;

        // 防止 DataGridView 選擇事件觸發不必要嘅載入
        private bool isPopulatingData = false;

        public AfterServiceForm()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                InitializeEnterpriseHelpdeskUI();
                SetupDropdowns();
                LoadCustomers(); // 🌟 啟動時先載入所有有訂單嘅客戶
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

            // 🌟 1. Customer ID 下拉選單
            Label lblCustomer = new Label { Text = "Select Customer ID *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboCustomer = new ComboBox { Location = new Point(20, startY + 22), Width = inputWidth, Font = new Font("Segoe UI", 10.5F), DropDownStyle = ComboBoxStyle.DropDownList };
            cboCustomer.SelectedIndexChanged += CboCustomer_SelectedIndexChanged;
            pnlLeftCard.Controls.Add(lblCustomer); pnlLeftCard.Controls.Add(cboCustomer);
            startY += 65;

            // 🌟 2. Order ID 下拉選單 (根據 Customer 變動)
            Label lblOrder = new Label { Text = "Related Order ID *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboOrderID = new ComboBox { Location = new Point(20, startY + 22), Width = inputWidth, Font = new Font("Segoe UI", 10.5F), DropDownStyle = ComboBoxStyle.DropDownList };
            cboOrderID.SelectedIndexChanged += CboOrderID_SelectedIndexChanged;
            pnlLeftCard.Controls.Add(lblOrder); pnlLeftCard.Controls.Add(cboOrderID);
            startY += 65;

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

            lblItemsTitle = new Label { Text = "🛍️ Purchased Items Context", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true };
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
                int topGridHeight = (int)(availableHeight * 0.55);
                int botGridHeight = availableHeight - topGridHeight;

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

        #region 💾 聯動選單及資料載入邏輯
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

        // 🌟 載入所有有訂單嘅 Customer
        private void LoadCustomers()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT DISTINCT CustomerID FROM orders ORDER BY CustomerID";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        cboCustomer.Items.Clear();
                        while (reader.Read())
                        {
                            cboCustomer.Items.Add(reader["CustomerID"].ToString());
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Failed to load customers: " + ex.Message); }
            }
        }

        // 🌟 Customer 改變時，聯動尋找屬於佢嘅 Orders
        private void CboCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isPopulatingData) return;

            cboOrderID.Items.Clear();
            cboOrderID.Text = "";
            dgvOrderItems.DataSource = null;
            lblOrderSummary.Text = "Total Amount Paid: $0.00";

            if (cboCustomer.SelectedItem == null) return;

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT OrderID FROM orders WHERE CustomerID = @CID ORDER BY OrderID DESC";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CID", cboCustomer.SelectedItem.ToString());
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cboOrderID.Items.Add(reader["OrderID"].ToString());
                            }
                        }
                    }
                }
                catch (Exception ex) { /* 避免干擾 UX，可根據需要 Log Error */ }
            }
        }

        // 🌟 Order 改變時，自動載入購物清單
        private void CboOrderID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isPopulatingData) return;

            if (cboOrderID.SelectedItem != null)
            {
                LoadOrderContextItems(cboOrderID.SelectedItem.ToString());
            }
            else
            {
                dgvOrderItems.DataSource = null;
                lblOrderSummary.Text = "Total Amount Paid: $0.00";
            }
        }

        private void AfterServiceForm_Load(object sender, EventArgs e)
        {

        }

        private void LoadOrderContextItems(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId)) { dgvOrderItems.DataSource = null; lblOrderSummary.Text = "Total Amount Paid: $0.00"; return; }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmdSum = new MySqlCommand("SELECT TotalAmount FROM orders WHERE OrderID = @OID", conn);
                    cmdSum.Parameters.AddWithValue("@OID", orderId);
                    object totalObj = cmdSum.ExecuteScalar();
                    if (totalObj != null && totalObj != DBNull.Value) lblOrderSummary.Text = $"Total Amount Paid: ${Convert.ToDecimal(totalObj):N2}";

                    string query = @"SELECT p.ProductID AS 'Product ID', p.ProductName AS 'Product Name', 
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
        #endregion

        #region 💾 核心庫存及投訴邏輯
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
                    try { new MySqlCommand("ALTER TABLE `complaint` ADD COLUMN `IsStockReturned` TINYINT(1) DEFAULT 0;", conn).ExecuteNonQuery(); } catch { }
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

        private void DgvComplaints_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvComplaints.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status.Contains("Pending") || status.Contains("In Progress"))
                {
                    e.CellStyle.ForeColor = Color.FromArgb(234, 88, 12);
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
                else if (status == "Resolved" || status == "Refunded")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129);
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
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
                isPopulatingData = true; // 🌟 鎖住聯動防干擾

                DataGridViewRow row = dgvComplaints.SelectedRows[0];
                txtComplaintID.Text = row.Cells["Ticket ID"].Value?.ToString() ?? "";

                string custID = row.Cells["Customer"].Value?.ToString() ?? "";
                if (cboCustomer.Items.Contains(custID)) cboCustomer.SelectedItem = custID;

                // 補返載入呢個 Customer 嘅 Order 選擇
                cboOrderID.Items.Clear();
                if (!string.IsNullOrWhiteSpace(custID))
                {
                    using (MySqlConnection conn = new MySqlConnection(connString))
                    {
                        try
                        {
                            conn.Open();
                            using (MySqlCommand cmd = new MySqlCommand("SELECT OrderID FROM orders WHERE CustomerID = @CID ORDER BY OrderID DESC", conn))
                            {
                                cmd.Parameters.AddWithValue("@CID", custID);
                                using (MySqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read()) cboOrderID.Items.Add(reader["OrderID"].ToString());
                                }
                            }
                        }
                        catch { }
                    }
                }

                string orderRef = row.Cells["Order Ref"].Value?.ToString() ?? "";
                if (cboOrderID.Items.Contains(orderRef)) cboOrderID.SelectedItem = orderRef;

                string reqType = row.Cells["Type"].Value?.ToString()?.Trim() ?? "General Complaint";
                cboRequestType.Text = reqType;

                txtRefundAmount.Text = row.Cells["Refund HKD"].Value?.ToString() ?? "0.00";
                txtDetails.Text = row.Cells["Description"].Value?.ToString() ?? "";

                string status = row.Cells["Status"].Value?.ToString();
                if (cboStatus.Items.Contains(status)) cboStatus.Text = status;

                isPopulatingData = false; // 解除鎖定
                LoadOrderContextItems(orderRef);
            }
        }

        private void btnSubmitComplaint_Click(object sender, EventArgs e)
        {
            string ticketID = txtComplaintID.Text.Trim();
            string customerID = cboCustomer.Text.Trim(); // 🌟 轉用 ComboBox
            string orderID = cboOrderID.Text.Trim();     // 🌟 轉用 ComboBox
            string reqType = cboRequestType.Text;
            string status = cboStatus.Text;

            string cleanAmt = txtRefundAmount.Text.Replace("$", "").Trim();
            decimal.TryParse(cleanAmt, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal refundAmt);

            if (string.IsNullOrWhiteSpace(customerID) || string.IsNullOrWhiteSpace(status) || string.IsNullOrWhiteSpace(reqType))
            {
                MessageBox.Show("Please complete the required fields (Customer ID, Request Type, Status).", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    bool isStockAlreadyReturned = false;
                    using (MySqlCommand checkCmd = new MySqlCommand("SELECT IsStockReturned FROM complaint WHERE ComplaintID = @CID", conn))
                    {
                        checkCmd.Parameters.AddWithValue("@CID", ticketID);
                        object res = checkCmd.ExecuteScalar();
                        if (res != null && res != DBNull.Value) isStockAlreadyReturned = Convert.ToBoolean(res);
                    }

                    bool shouldReturnStock = (!isStockAlreadyReturned && reqType == "Return & Refund" && (status == "Refunded" || status == "Resolved") && !string.IsNullOrWhiteSpace(orderID));

                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            if (shouldReturnStock)
                            {
                                string fetchItemsSql = "SELECT ProductID, Quantity FROM order_lineitem WHERE OrderID = @OID";
                                using (MySqlCommand fetchCmd = new MySqlCommand(fetchItemsSql, conn, trans))
                                {
                                    fetchCmd.Parameters.AddWithValue("@OID", orderID);
                                    using (MySqlDataReader reader = fetchCmd.ExecuteReader())
                                    {
                                        List<Tuple<string, int>> itemsToReturn = new List<Tuple<string, int>>();
                                        while (reader.Read())
                                        {
                                            itemsToReturn.Add(new Tuple<string, int>(reader["ProductID"].ToString(), Convert.ToInt32(reader["Quantity"])));
                                        }
                                        reader.Close();

                                        foreach (var item in itemsToReturn)
                                        {
                                            using (MySqlCommand updateStockCmd = new MySqlCommand("UPDATE product SET StockLevel = StockLevel + @qty WHERE ProductID = @pid", conn, trans))
                                            {
                                                updateStockCmd.Parameters.AddWithValue("@qty", item.Item2);
                                                updateStockCmd.Parameters.AddWithValue("@pid", item.Item1);
                                                updateStockCmd.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                }
                                isStockAlreadyReturned = true;
                            }

                            string sql = @"INSERT INTO complaint (ComplaintID, CustomerID, OrderID, RequestType, Description, RefundAmount, Date, Status, IsStockReturned) 
                                           VALUES (@CID, @CustID, @OID, @ReqType, @Desc, @RefundAmt, NOW(), @Status, @IsReturned)
                                           ON DUPLICATE KEY UPDATE Status = @Status, OrderID = @OID, RequestType = @ReqType, Description = @Desc, RefundAmount = @RefundAmt, IsStockReturned = @IsReturned;";

                            using (MySqlCommand cmd = new MySqlCommand(sql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@CID", ticketID);
                                cmd.Parameters.AddWithValue("@CustID", customerID);
                                cmd.Parameters.AddWithValue("@OID", string.IsNullOrWhiteSpace(orderID) ? (object)DBNull.Value : orderID);
                                cmd.Parameters.AddWithValue("@ReqType", reqType);
                                cmd.Parameters.AddWithValue("@Desc", txtDetails.Text.Trim());
                                cmd.Parameters.AddWithValue("@RefundAmt", refundAmt);
                                cmd.Parameters.AddWithValue("@Status", status);
                                cmd.Parameters.AddWithValue("@IsReturned", isStockAlreadyReturned ? 1 : 0);
                                cmd.ExecuteNonQuery();
                            }

                            trans.Commit();

                            string msg = "Support Ticket has been successfully saved!";
                            if (shouldReturnStock) msg += "\n\n📦 System Alert: Inventory rollback successful. Returned items have been added back to stock.";
                            MessageBox.Show(msg, "Transaction Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            ClearFields();
                            LoadComplaints();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw ex;
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Failed to save the ticket:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void ClearFields()
        {
            isPopulatingData = true;
            cboCustomer.SelectedIndex = -1;
            cboOrderID.Items.Clear();
            cboRequestType.SelectedIndex = -1;
            cboStatus.SelectedIndex = -1;
            txtDetails.Clear();
            txtRefundAmount.Text = "0.00";
            txtRefundAmount.Enabled = false;
            txtRefundAmount.BackColor = Color.FromArgb(241, 245, 249);
            dgvComplaints.ClearSelection();
            dgvOrderItems.DataSource = null;
            lblOrderSummary.Text = "Total Amount Paid: $0.00";
            GenerateNewTicketID();
            isPopulatingData = false;
        }
        #endregion
    }
}