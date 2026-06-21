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
        private readonly string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";
        private DataTable originalComplaintsTable = new DataTable();

        // ==========================================
        // 🎨 Modern UI Element Variables
        // ==========================================
        private TextBox txtComplaintID, txtCustomerID, txtOrderID, txtSearchHistory;
        private TextBox txtDetails, txtRefundAmount;
        private ComboBox cboStatus, cboRequestType;
        private DataGridView dgvComplaints;
        private Button btnSubmit, btnClear;

        public AfterServiceForm()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                InitializePremiumModernUI();
                SetupDropdowns();
                EnsureComplaintTableExists();
                GenerateNewTicketID();
                LoadComplaints();
            }
        }

        #region 🎨 Dynamic Premium English UI Construction
        private void InitializePremiumModernUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Customer Support & After-Sales Service";
            this.Size = new Size(1180, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            Panel pnlMain = new Panel { Location = new Point(260, 0), Size = new Size(900, 750) };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label { Text = "Customer Support & Ticketing System", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(30, 20), AutoSize = true };
            pnlMain.Controls.Add(lblHeader);

            Button btnBackHome = new Button { Text = "⬅ Go Back", Size = new Size(120, 34), Location = new Point(740, 22), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnBackHome.FlatAppearance.BorderSize = 0;
            btnBackHome.Click += (s, e) => { this.Close(); };
            pnlMain.Controls.Add(btnBackHome);

            Panel pnlCard = new Panel { Location = new Point(30, 85), Size = new Size(380, 650), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlCard);

            Label lblCardTitle = new Label { Text = "📝 Support Ticket Details", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(20, 15), AutoSize = true };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 60;
            txtComplaintID = CreateStyledTextBox(pnlCard, ref startY, "Ticket / Complaint ID (Auto-Generated):", true);
            txtCustomerID = CreateStyledTextBox(pnlCard, ref startY, "Customer ID *:", false);
            txtOrderID = CreateStyledTextBox(pnlCard, ref startY, "Related Order ID (Optional):", false);

            Label lblReqType = new Label { Text = "Request Type *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboRequestType = new ComboBox { Location = new Point(20, startY + 22), Width = 335, Font = new Font("Segoe UI", 10.5F), DropDownStyle = ComboBoxStyle.DropDownList };
            cboRequestType.SelectedIndexChanged += CboRequestType_SelectedIndexChanged;
            pnlCard.Controls.Add(lblReqType); pnlCard.Controls.Add(cboRequestType);
            startY += 65;

            Label lblDetails = new Label { Text = "Complaint Details / Records:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtDetails = new TextBox { Location = new Point(20, startY + 22), Width = 335, Height = 60, Multiline = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            pnlCard.Controls.Add(lblDetails); pnlCard.Controls.Add(txtDetails);
            startY += 95;

            Label lblRefund = new Label { Text = "Refund Amount ($):", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtRefundAmount = new TextBox { Location = new Point(20, startY + 22), Width = 335, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle, Enabled = false, Text = "0.00" };
            pnlCard.Controls.Add(lblRefund); pnlCard.Controls.Add(txtRefundAmount);
            startY += 65;

            Label lblStatus = new Label { Text = "Resolution Status *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboStatus = new ComboBox { Location = new Point(20, startY + 22), Width = 335, Font = new Font("Segoe UI", 10.5F), DropDownStyle = ComboBoxStyle.DropDownList };
            pnlCard.Controls.Add(lblStatus); pnlCard.Controls.Add(cboStatus);
            startY += 65;

            btnSubmit = new Button { Text = "💾 Save / Update Ticket", Location = new Point(20, startY), Size = new Size(160, 40), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += btnSubmitComplaint_Click;
            pnlCard.Controls.Add(btnSubmit);

            btnClear = new Button { Text = "✨ Add New Data", Location = new Point(195, startY), Size = new Size(160, 40), BackColor = Color.FromArgb(71, 85, 105), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, e) => ClearFields();
            pnlCard.Controls.Add(btnClear);

            Label lblGridTitle = new Label { Text = "📂 Support Tickets History", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(440, 85), AutoSize = true };
            pnlMain.Controls.Add(lblGridTitle);

            txtSearchHistory = new TextBox { Location = new Point(650, 85), Size = new Size(220, 25), Font = new Font("Segoe UI", 10F) };
            txtSearchHistory.Text = "Type ID to search...";
            txtSearchHistory.ForeColor = Color.Gray;
            txtSearchHistory.GotFocus += (s, e) => { if (txtSearchHistory.Text == "Type ID to search...") { txtSearchHistory.Text = ""; txtSearchHistory.ForeColor = Color.Black; } };
            txtSearchHistory.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearchHistory.Text)) { txtSearchHistory.Text = "Type ID to search..."; txtSearchHistory.ForeColor = Color.Gray; } };
            txtSearchHistory.TextChanged += txtSearchHistory_TextChanged;
            pnlMain.Controls.Add(txtSearchHistory);

            dgvComplaints = new DataGridView { Location = new Point(440, 125), Size = new Size(430, 610), BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvComplaints.EnableHeadersVisualStyles = false;
            dgvComplaints.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvComplaints.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvComplaints.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvComplaints.ColumnHeadersHeight = 38;
            dgvComplaints.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvComplaints.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);

            dgvComplaints.SelectionChanged += dgvComplaints_SelectionChanged;
            pnlMain.Controls.Add(dgvComplaints);
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

        private void CboRequestType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 🛠️ 優化：改為不分大小寫與消除空格的精準比對
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

        #region 📦 Core Logic
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
                          PRIMARY KEY (`ComplaintID`),
                          FOREIGN KEY (`CustomerID`) REFERENCES `customer` (`CustomerID`),
                          FOREIGN KEY (`OrderID`) REFERENCES `orders` (`OrderID`)
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
                    using (MySqlCommand cmd = new MySqlCommand(createTableQuery, conn)) { cmd.ExecuteNonQuery(); }

                    try { new MySqlCommand("ALTER TABLE `complaint` ADD COLUMN `RequestType` varchar(50) DEFAULT 'General Complaint';", conn).ExecuteNonQuery(); } catch { }
                    try { new MySqlCommand("ALTER TABLE `complaint` ADD COLUMN `Description` text;", conn).ExecuteNonQuery(); } catch { }
                    try { new MySqlCommand("ALTER TABLE `complaint` ADD COLUMN `RefundAmount` decimal(10,2) DEFAULT 0.00;", conn).ExecuteNonQuery(); } catch { }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to verify system tables:\n" + ex.Message, "Database Initialization Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    if (dgvComplaints.Columns.Contains("ComplaintID")) dgvComplaints.Columns["ComplaintID"].HeaderText = "Ticket ID";
                    if (dgvComplaints.Columns.Contains("CustomerID")) dgvComplaints.Columns["CustomerID"].HeaderText = "Customer ID";
                    if (dgvComplaints.Columns.Contains("RequestType")) dgvComplaints.Columns["RequestType"].HeaderText = "Type";
                    if (dgvComplaints.Columns.Contains("RefundAmount")) dgvComplaints.Columns["RefundAmount"].HeaderText = "Refund $";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database Load Error: " + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AfterServiceForm_Load(object sender, EventArgs e)
        {
            if (!AuthorizationHelper.IsInRole(AuthorizationHelper.Roles.Manager, AuthorizationHelper.Roles.Administrator, AuthorizationHelper.Roles.Sales))
            {
                MessageBox.Show("您沒有權限存取售後服務模組。", "存取被拒", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.BeginInvoke(new MethodInvoker(this.Close));
                return;
            }

            EnsureComplaintTableExists();
            GenerateNewTicketID();
            LoadComplaints();
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

                // 🛠️ 核心修正：先綁定下拉選單類型 (防範事件在背後偷偷把值清空)
                string reqType = row.Cells["RequestType"].Value?.ToString()?.Trim() ?? "General Complaint";
                cboRequestType.Text = reqType;

                // 🛠️ 核心修正：在下拉選單事件安頓好後，才正式塞入退款數值
                txtRefundAmount.Text = row.Cells["RefundAmount"].Value?.ToString() ?? "0.00";
                txtDetails.Text = row.Cells["Description"].Value?.ToString() ?? "";

                string status = row.Cells["Status"].Value?.ToString();
                if (cboStatus.Items.Contains(status)) cboStatus.Text = status;
            }
        }

        private void btnSubmitComplaint_Click(object sender, EventArgs e)
        {
            string newStatus = cboStatus.Text?.Trim();
            if (!string.IsNullOrEmpty(newStatus) && (newStatus.Equals("Refunded", StringComparison.OrdinalIgnoreCase) || newStatus.Equals("Closed", StringComparison.OrdinalIgnoreCase)))
            {
                if (!AuthorizationHelper.IsInRole(AuthorizationHelper.Roles.Manager, AuthorizationHelper.Roles.Administrator))
                {
                    MessageBox.Show("Access Denied: insufficient privileges to set status to Refund/Close.", "Authorization", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
            }

            string customerID = txtCustomerID.Text.Trim();
            string orderID = txtOrderID.Text.Trim();

            // 🛠️ 核心修正：採用更安全的文化常規解析，防止字串干擾數值
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

                    string checkCustSql = "SELECT COUNT(*) FROM customer WHERE CustomerID = @CustID";
                    using (MySqlCommand cmdCheckCust = new MySqlCommand(checkCustSql, conn))
                    {
                        cmdCheckCust.Parameters.AddWithValue("@CustID", customerID);
                        if (Convert.ToInt32(cmdCheckCust.ExecuteScalar()) == 0)
                        {
                            MessageBox.Show($"Customer ID '{customerID}' does not exist inside our system records!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(orderID))
                    {
                        string checkOrderSql = "SELECT COUNT(*) FROM orders WHERE OrderID = @OrderID";
                        using (MySqlCommand cmdCheckOrder = new MySqlCommand(checkOrderSql, conn))
                        {
                            cmdCheckOrder.Parameters.AddWithValue("@OrderID", orderID);
                            if (Convert.ToInt32(cmdCheckOrder.ExecuteScalar()) == 0)
                            {
                                MessageBox.Show($"Associated Order ID '{orderID}' does not exist inside our records!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                    }

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
            // 🛠️ 核心修正：調整清空順序，先重設下拉選單，最後才塞入預設退款金額，徹底解決歸零蓋屏問題
            cboRequestType.SelectedIndex = -1;
            cboStatus.SelectedIndex = -1;
            txtCustomerID.Clear();
            txtOrderID.Clear();
            txtDetails.Clear();
            txtRefundAmount.Text = "0.00";
            dgvComplaints.ClearSelection();
            GenerateNewTicketID();
        }
        #endregion
    }
}