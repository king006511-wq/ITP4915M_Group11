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
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";
        private DataTable originalComplaintsTable = new DataTable();
        private TextBox txtComplaintID, txtDetails, txtRefundAmount, txtSearchHistory;
        private ComboBox cboCustomer, cboOrderID, cboStatus, cboRequestType;
        private DataGridView dgvComplaints, dgvOrderItems;
        private Button btnSubmit, btnClear;
        private Label lblOrderSummary, lblGridTitle, lblItemsTitle;
        private Panel pnlLeftCard;
        private bool isPopulatingData = false;

        // 定義你指定嘅極出彩高對比度顏色
        private readonly Color colorRoyalBlue = Color.FromArgb(37, 99, 235);  // #2563EB 皇家藍 (主表格)
        private readonly Color colorTealBlue = Color.FromArgb(14, 165, 233);  // #0EA5E9 湖水藍 (子表格)

        public AfterServiceForm()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                InitializeEnterpriseHelpdeskUI();
                SetupDropdowns();
                LoadCustomers();
                EnsureComplaintTableExists();
                GenerateNewTicketID();
                LoadComplaints();
                this.SizeChanged += AfterServiceForm_SizeChanged;
                this.Layout += (s, e) => RecalculateCustomLayout();
            }
        }

        private void InitializeEnterpriseHelpdeskUI()
        {
            this.Controls.Clear();
            this.BackColor = ThemeManager.PrimaryBackground; // #F9FAFB 極淺灰
            this.Font = ThemeManager.DefaultFont;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopLevel = false;
            this.Dock = DockStyle.Fill;

            pnlLeftCard = new Panel { BackColor = ThemeManager.CardBackground, BorderStyle = BorderStyle.None, AutoScroll = true };
            pnlLeftCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlLeftCard.ClientRectangle, ThemeManager.BorderColor, ButtonBorderStyle.Solid);
            this.Controls.Add(pnlLeftCard);

            Label lblCardTitle = new Label { Text = "🎧 After-Sales Service Center", Font = new Font("Microsoft JhengHei", 15F, FontStyle.Bold), ForeColor = ThemeManager.AccentStrong, Location = new Point(25, 25), AutoSize = true };
            pnlLeftCard.Controls.Add(lblCardTitle);

            int startY = 80; int inputWidth = 350;
            txtComplaintID = CreateStyledTextBox(pnlLeftCard, ref startY, "🎫 Ticket ID:", true, inputWidth);
            startY += 70;

            Label lblCustomer = new Label { Text = "👤 Customer ID *:", Location = new Point(25, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboCustomer = new ComboBox { Location = new Point(25, startY + 25), Width = inputWidth, Font = new Font("Segoe UI", 10.5F), DropDownStyle = ComboBoxStyle.DropDownList };
            cboCustomer.SelectedIndexChanged += CboCustomer_SelectedIndexChanged;
            pnlLeftCard.Controls.Add(lblCustomer); pnlLeftCard.Controls.Add(cboCustomer);
            startY += 70;

            Label lblOrder = new Label { Text = "📦 Order ID *:", Location = new Point(25, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboOrderID = new ComboBox { Location = new Point(25, startY + 25), Width = inputWidth, Font = new Font("Segoe UI", 10.5F), DropDownStyle = ComboBoxStyle.DropDownList };
            cboOrderID.SelectedIndexChanged += CboOrderID_SelectedIndexChanged;
            pnlLeftCard.Controls.Add(lblOrder); pnlLeftCard.Controls.Add(cboOrderID);
            startY += 70;

            Label lblReqType = new Label { Text = "🏷️ Request Type *:", Location = new Point(25, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboRequestType = new ComboBox { Location = new Point(25, startY + 25), Width = inputWidth, Font = new Font("Segoe UI", 10.5F), DropDownStyle = ComboBoxStyle.DropDownList };
            cboRequestType.SelectedIndexChanged += CboRequestType_SelectedIndexChanged;
            pnlLeftCard.Controls.Add(lblReqType); pnlLeftCard.Controls.Add(cboRequestType);
            startY += 70;

            Label lblDetails = new Label { Text = "📝 Description / Remarks:", Location = new Point(25, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtDetails = new TextBox { Location = new Point(25, startY + 25), Width = inputWidth, Height = 85, Multiline = true, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            pnlLeftCard.Controls.Add(lblDetails); pnlLeftCard.Controls.Add(txtDetails);
            startY += 120;

            Label lblRefund = new Label { Text = "💰 Refund Amount (HKD):", Location = new Point(25, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(234, 88, 12) }; // #EA580C 橙色
            txtRefundAmount = new TextBox { Location = new Point(25, startY + 25), Width = inputWidth, Font = new Font("Segoe UI", 11F, FontStyle.Bold), BorderStyle = BorderStyle.FixedSingle, Enabled = false, Text = "0.00", BackColor = Color.FromArgb(241, 245, 249) };
            pnlLeftCard.Controls.Add(lblRefund); pnlLeftCard.Controls.Add(txtRefundAmount);
            startY += 70;

            Label lblStatus = new Label { Text = "📌 Status *:", Location = new Point(25, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboStatus = new ComboBox { Location = new Point(25, startY + 25), Width = inputWidth, Font = new Font("Segoe UI", 10.5F), DropDownStyle = ComboBoxStyle.DropDownList };
            pnlLeftCard.Controls.Add(lblStatus); pnlLeftCard.Controls.Add(cboStatus);
            startY += 85;

            btnSubmit = new Button { Text = "💾 Save Record", Location = new Point(25, startY), Size = new Size(165, 45), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClear = new Button { Text = "✨ Clear Fields", Location = new Point(210, startY), Size = new Size(165, 45), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSubmit.FlatAppearance.BorderSize = 0; btnClear.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += btnSubmitComplaint_Click; btnClear.Click += (s, e) => ClearFields();
            pnlLeftCard.Controls.Add(btnSubmit); pnlLeftCard.Controls.Add(btnClear);

            lblGridTitle = new Label { Text = "📂 Service Ticket History", Font = new Font("Microsoft JhengHei", 20F, FontStyle.Bold), ForeColor = ThemeManager.PrimaryDark, AutoSize = true };
            this.Controls.Add(lblGridTitle);

            txtSearchHistory = new TextBox { Width = 320, Font = new Font("Microsoft JhengHei", 10.5F), BorderStyle = BorderStyle.FixedSingle, ForeColor = Color.Gray, Text = "🔍 Search Ticket ID or Order ID..." };
            txtSearchHistory.GotFocus += (s, e) => { if (txtSearchHistory.Text == "🔍 Search Ticket ID or Order ID...") { txtSearchHistory.Text = ""; txtSearchHistory.ForeColor = Color.Black; } };
            txtSearchHistory.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearchHistory.Text)) { txtSearchHistory.Text = "🔍 Search Ticket ID or Order ID..."; txtSearchHistory.ForeColor = Color.Gray; } };
            txtSearchHistory.TextChanged += txtSearchHistory_TextChanged;
            this.Controls.Add(txtSearchHistory);

            dgvComplaints = CreateCleanGrid();
            dgvComplaints.SelectionChanged += dgvComplaints_SelectionChanged;
            dgvComplaints.CellFormatting += DgvComplaints_CellFormatting;
            this.Controls.Add(dgvComplaints);
            StyleGridHeader(dgvComplaints, colorRoyalBlue); // 初始化皇家藍樣式

            lblItemsTitle = new Label { Text = "🛍️ Order Item Details", Font = new Font("Microsoft JhengHei", 20F, FontStyle.Bold), ForeColor = ThemeManager.PrimaryDark, AutoSize = true };
            this.Controls.Add(lblItemsTitle);

            lblOrderSummary = new Label { Text = "💳 Total Paid: $0.00", Font = new Font("Microsoft JhengHei", 11F, FontStyle.Bold), ForeColor = ThemeManager.Success, AutoSize = true };
            this.Controls.Add(lblOrderSummary);

            dgvOrderItems = CreateCleanGrid();
            this.Controls.Add(dgvOrderItems);
            StyleGridHeader(dgvOrderItems, colorTealBlue); // 初始化湖水藍樣式
        }

        private DataGridView CreateCleanGrid()
        {
            DataGridView dgv = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                GridColor = Color.FromArgb(226, 232, 240),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgv.DefaultCellStyle.SelectionForeColor = ThemeManager.PrimaryDark;
            dgv.DefaultCellStyle.Font = new Font("Microsoft JhengHei", 9.5F);
            return dgv;
        }

        // 🛠️ 核心修正：強制覆蓋作業系統硬解樣式嘅鋼鐵級函數
        private void StyleGridHeader(DataGridView dgv, Color headerColor)
        {
            dgv.EnableHeadersVisualStyles = false; // 關閉預設樣式

            // 設定背景色與「選取時的背景色」（防止點擊/滑鼠懸停時打回原形）
            dgv.ColumnHeadersDefaultCellStyle.BackColor = headerColor;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerColor;

            // 設定高對比純白文字
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;

            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 40;

            // 刷新表格渲染
            dgv.Refresh();
        }

        private void RecalculateCustomLayout()
        {
            if (this.Width < 200 || this.Height < 200) return;
            this.SuspendLayout();
            pnlLeftCard.Location = new Point(25, 25);
            pnlLeftCard.Size = new Size(410, this.Height - 50);
            int rightStartX = pnlLeftCard.Right + 30;
            int rightWidth = this.Width - rightStartX - 25;
            if (rightWidth > 100)
            {
                lblGridTitle.Location = new Point(rightStartX, 25);
                txtSearchHistory.Location = new Point(this.Width - txtSearchHistory.Width - 25, 30);
                int availableHeight = this.Height - 140;
                int topGridHeight = (int)(availableHeight * 0.55);
                int botGridHeight = availableHeight - topGridHeight;
                dgvComplaints.Location = new Point(rightStartX, 75);
                dgvComplaints.Size = new Size(rightWidth, topGridHeight);
                int bottomSectionY = dgvComplaints.Bottom + 20;
                lblItemsTitle.Location = new Point(rightStartX, bottomSectionY);
                lblOrderSummary.Location = new Point(this.Width - lblOrderSummary.Width - 25, bottomSectionY + 10);
                dgvOrderItems.Location = new Point(rightStartX, bottomSectionY + 45);
                dgvOrderItems.Size = new Size(rightWidth, botGridHeight);
            }
            this.ResumeLayout(false);
        }

        private TextBox CreateStyledTextBox(Panel container, ref int topY, string labelText, bool readOnly, int width)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(25, topY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(25, topY + 25), Width = width, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            if (readOnly) { txt.ReadOnly = true; txt.BackColor = Color.FromArgb(241, 245, 249); }
            container.Controls.Add(lbl); container.Controls.Add(txt);
            return txt;
        }

        private void CboRequestType_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isRefund = cboRequestType.Text.Contains("Refund") || cboRequestType.Text.Contains("Return");
            txtRefundAmount.Enabled = isRefund;
            txtRefundAmount.BackColor = isRefund ? Color.White : Color.FromArgb(241, 245, 249);
            if (!isRefund) txtRefundAmount.Text = "0.00";
        }

        private void SetupDropdowns()
        {
            cboRequestType.Items.AddRange(new[] { "General Complaint", "Return & Refund", "Product Replacement", "Partial Refund" });
            cboStatus.Items.AddRange(new[] { "Pending Investigation", "In Progress", "Resolved", "Refunded", "Closed" });
        }

        private void GenerateNewTicketID() => txtComplaintID.Text = "TKT-" + DateTime.Now.ToString("yyyyMMdd-HHmm");

        private void LoadCustomers()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT CustomerID FROM orders ORDER BY CustomerID", conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        cboCustomer.Items.Clear();
                        while (reader.Read()) cboCustomer.Items.Add(reader["CustomerID"].ToString());
                    }
                }
                catch (Exception ex) { MessageBox.Show("Failed to load customers: " + ex.Message); }
            }
        }

        private void CboCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isPopulatingData) return;
            cboOrderID.Items.Clear();
            dgvOrderItems.DataSource = null;
            lblOrderSummary.Text = "💳 Total Paid: $0.00";
            if (cboCustomer.SelectedItem == null) return;
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT OrderID FROM orders WHERE CustomerID = @CID ORDER BY OrderID DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@CID", cboCustomer.SelectedItem);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                            while (reader.Read()) cboOrderID.Items.Add(reader["OrderID"].ToString());
                    }
                }
                catch { }
            }
        }

        private void CboOrderID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isPopulatingData) return;
            if (cboOrderID.SelectedItem != null) LoadOrderContextItems(cboOrderID.SelectedItem.ToString());
            else { dgvOrderItems.DataSource = null; lblOrderSummary.Text = "💳 Total Paid: $0.00"; }
        }

        private void AfterServiceForm_Load_1(object sender, EventArgs e)
        {

        }

        private void LoadOrderContextItems(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId)) { dgvOrderItems.DataSource = null; lblOrderSummary.Text = "💳 Total Paid: $0.00"; return; }
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    object total = new MySqlCommand("SELECT TotalAmount FROM orders WHERE OrderID = @OID", conn) { Parameters = { new MySqlParameter("@OID", orderId) } }.ExecuteScalar();
                    if (total != null && total != DBNull.Value) lblOrderSummary.Text = $"💳 Total Paid: ${Convert.ToDecimal(total):N2}";

                    string query = @"SELECT p.ProductID AS 'Product ID', p.ProductName AS 'Product Name', l.Quantity AS 'Qty', l.UnitPrice AS 'Unit Price ($)', (l.Quantity * l.UnitPrice) AS 'Subtotal ($)' FROM order_lineitem l JOIN product p ON l.ProductID = p.ProductID WHERE l.OrderID = @OID";
                    using (MySqlDataAdapter da = new MySqlDataAdapter(query, conn))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@OID", orderId);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvOrderItems.DataSource = dt;
                    }

                    // ⚠️ 重點：數據加載完畢後，再次補刷 Header 樣式，免被 DataBinding 洗走
                    StyleGridHeader(dgvOrderItems, colorTealBlue);
                }
                catch { dgvOrderItems.DataSource = null; }
            }
        }

        private void EnsureComplaintTableExists()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    new MySqlCommand(@"CREATE TABLE IF NOT EXISTS `complaint` (`ComplaintID` varchar(30) NOT NULL, `CustomerID` varchar(15) NOT NULL, `OrderID` varchar(20) DEFAULT NULL, `Date` datetime NOT NULL, `Status` varchar(30) NOT NULL, PRIMARY KEY (`ComplaintID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;", conn).ExecuteNonQuery();
                    try { new MySqlCommand("ALTER TABLE `complaint` ADD COLUMN `RequestType` varchar(50) DEFAULT 'General Complaint';", conn).ExecuteNonQuery(); } catch { }
                    try { new MySqlCommand("ALTER TABLE `complaint` ADD COLUMN `Description` text;", conn).ExecuteNonQuery(); } catch { }
                    try { new MySqlCommand("ALTER TABLE `complaint` ADD COLUMN `RefundAmount` decimal(10,2) DEFAULT 0.00;", conn).ExecuteNonQuery(); } catch { }
                    try { new MySqlCommand("ALTER TABLE `complaint` ADD COLUMN `IsStockReturned` TINYINT(1) DEFAULT 0;", conn).ExecuteNonQuery(); } catch { }
                }
                catch { }
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

                    // ⚠️ 重點：數據加載完畢後，再次補刷 Header 樣式，免被 DataBinding 洗走
                    StyleGridHeader(dgvComplaints, colorRoyalBlue);
                }
                catch (Exception ex) { MessageBox.Show("Database Load Error: " + ex.Message); }
            }
        }

        private void DgvComplaints_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvComplaints.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string s = e.Value.ToString();
                if (s.Contains("Pending") || s.Contains("In Progress")) e.CellStyle.ForeColor = Color.FromArgb(234, 88, 12);
                else if (s == "Resolved" || s == "Refunded") e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129);
                e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
            }
        }

        private void txtSearchHistory_TextChanged(object sender, EventArgs e)
        {
            if (dgvComplaints.DataSource is DataTable dt && txtSearchHistory.Text != "🔍 Search Ticket ID or Order ID...")
            {
                string k = txtSearchHistory.Text.Trim().Replace("'", "''");
                dt.DefaultView.RowFilter = string.IsNullOrWhiteSpace(k) ? "" : $"[Ticket ID] LIKE '%{k}%' OR [Order Ref] LIKE '%{k}%'";
            }
        }

        private void dgvComplaints_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvComplaints.SelectedRows.Count == 0) return;
            isPopulatingData = true;
            var row = dgvComplaints.SelectedRows[0];
            txtComplaintID.Text = row.Cells["Ticket ID"].Value?.ToString() ?? "";
            string custID = row.Cells["Customer"].Value?.ToString() ?? "";
            if (cboCustomer.Items.Contains(custID)) cboCustomer.SelectedItem = custID;

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
                            using (MySqlDataReader r = cmd.ExecuteReader())
                                while (r.Read()) cboOrderID.Items.Add(r["OrderID"].ToString());
                        }
                    }
                    catch { }
                }
            }

            string orderRef = row.Cells["Order Ref"].Value?.ToString() ?? "";
            if (cboOrderID.Items.Contains(orderRef)) cboOrderID.SelectedItem = orderRef;

            cboRequestType.Text = row.Cells["Type"].Value?.ToString()?.Trim() ?? "General Complaint";
            txtRefundAmount.Text = row.Cells["Refund HKD"].Value?.ToString() ?? "0.00";
            txtDetails.Text = row.Cells["Description"].Value?.ToString() ?? "";
            string status = row.Cells["Status"].Value?.ToString();
            if (cboStatus.Items.Contains(status)) cboStatus.Text = status;

            isPopulatingData = false;
            LoadOrderContextItems(orderRef);
        }

        private void btnSubmitComplaint_Click(object sender, EventArgs e)
        {
            string ticketID = txtComplaintID.Text.Trim();
            string customerID = cboCustomer.Text.Trim();
            string orderID = cboOrderID.Text.Trim();
            string reqType = cboRequestType.Text;
            string status = cboStatus.Text;
            decimal.TryParse(txtRefundAmount.Text.Replace("$", "").Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal refundAmt);

            if (string.IsNullOrWhiteSpace(customerID) || string.IsNullOrWhiteSpace(status) || string.IsNullOrWhiteSpace(reqType))
            {
                MessageBox.Show("Please fill in all required fields.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    bool isStockAlreadyReturned = false;
                    using (MySqlCommand check = new MySqlCommand("SELECT IsStockReturned FROM complaint WHERE ComplaintID = @CID", conn))
                    {
                        check.Parameters.AddWithValue("@CID", ticketID);
                        object res = check.ExecuteScalar();
                        if (res != null && res != DBNull.Value) isStockAlreadyReturned = Convert.ToBoolean(res);
                    }

                    bool shouldReturnStock = (!isStockAlreadyReturned && reqType == "Return & Refund" && (status == "Refunded" || status == "Resolved") && !string.IsNullOrWhiteSpace(orderID));

                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            if (shouldReturnStock)
                            {
                                using (MySqlCommand fetch = new MySqlCommand("SELECT ProductID, Quantity FROM order_lineitem WHERE OrderID = @OID", conn, trans))
                                {
                                    fetch.Parameters.AddWithValue("@OID", orderID);
                                    using (MySqlDataReader reader = fetch.ExecuteReader())
                                    {
                                        List<Tuple<string, int>> items = new List<Tuple<string, int>>();
                                        while (reader.Read()) items.Add(new Tuple<string, int>(reader["ProductID"].ToString(), Convert.ToInt32(reader["Quantity"])));
                                        reader.Close();
                                        foreach (var item in items)
                                        {
                                            using (MySqlCommand upd = new MySqlCommand("UPDATE product SET StockLevel = StockLevel + @qty WHERE ProductID = @pid", conn, trans))
                                            {
                                                upd.Parameters.AddWithValue("@qty", item.Item2);
                                                upd.Parameters.AddWithValue("@pid", item.Item1);
                                                upd.ExecuteNonQuery();
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
                            string msg = "Support Ticket successfully saved!";
                            if (shouldReturnStock) msg += "\n\n📦 Inventory rollback successful.";
                            MessageBox.Show(msg, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            ClearFields();
                            LoadComplaints();
                        }
                        catch { trans.Rollback(); throw; }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Failed to save ticket:\n" + ex.Message); }
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
            lblOrderSummary.Text = "💳 Total Paid: $0.00";
            GenerateNewTicketID();
            isPopulatingData = false;
        }

        private void AfterServiceForm_SizeChanged(object sender, EventArgs e) => RecalculateCustomLayout();
        private void AfterServiceForm_Load(object sender, EventArgs e) { }
    }
}