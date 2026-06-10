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
        private TextBox txtComplaintID, txtCustomerID, txtOrderID, txtDescription;
        private ComboBox cboStatus;
        private DataGridView dgvComplaints;
        private Button btnSubmit, btnClear;

        public AfterServiceForm()
        {
            InitializeComponent();
            InitializePremiumModernUI();
            SetupDropdowns();
            EnsureComplaintTableExists(); // 🛠️ Automatically creates the missing schema from the SQL dump
            GenerateNewTicketID();
            LoadComplaints();
        }

        #region 🎨 Dynamic Premium English UI Construction
        private void InitializePremiumModernUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Customer Support & After-Sales Service";
            this.Size = new Size(1180, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // 1. Left Sidebar Navigation
            Panel pnlSidebar = new Panel { Width = 260, Dock = DockStyle.Left, BackColor = Color.FromArgb(15, 23, 42) };
            Label lblLogo = new Label { Text = "Premium Living\nFurniture", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 25), Size = new Size(220, 60), TextAlign = ContentAlignment.MiddleLeft };
            pnlSidebar.Controls.Add(lblLogo);

            string[] menuItems = {
                "🛒 Sales Order Mgmt", "🚚 Delivery Logistics", "🛋️ Product Maintenance",
                "👔 HR / Staff Mgmt", "📦 Goods Received (GRN)", "🏭 Material Requests",
                "📊 Procurement Control", "🔧 Customer Support", "🚪 Logout System"
            };

            int btnTop = 110;
            foreach (string item in menuItems)
            {
                Button btnMenu = new Button { Text = "  " + item, Top = btnTop, Left = 12, Size = new Size(236, 48), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand };
                btnMenu.FlatAppearance.BorderSize = 0;

                if (item.Contains("Customer Support"))
                {
                    btnMenu.BackColor = Color.FromArgb(37, 99, 235); btnMenu.ForeColor = Color.White;
                }
                else if (item.Contains("Logout"))
                {
                    btnMenu.BackColor = Color.FromArgb(239, 68, 68); btnMenu.ForeColor = Color.White;
                    btnMenu.MouseEnter += (s, e) => { btnMenu.BackColor = Color.FromArgb(220, 38, 38); };
                    btnMenu.MouseLeave += (s, e) => { btnMenu.BackColor = Color.FromArgb(239, 68, 68); };
                }
                else
                {
                    btnMenu.BackColor = Color.Transparent; btnMenu.ForeColor = Color.FromArgb(148, 163, 184);
                    btnMenu.MouseEnter += (s, e) => { btnMenu.BackColor = Color.FromArgb(51, 65, 85); btnMenu.ForeColor = Color.White; };
                    btnMenu.MouseLeave += (s, e) => { btnMenu.BackColor = Color.Transparent; btnMenu.ForeColor = Color.FromArgb(148, 163, 184); };
                }

                btnMenu.Click += (s, e) => {
                    Form targetForm = null;
                    try
                    {
                        if (item.Contains("Sales Order")) targetForm = new OrderManagementForm();
                        else if (item.Contains("Logistics")) targetForm = new LogisticsForm();
                        else if (item.Contains("Product")) targetForm = new ProductManagement();
                        else if (item.Contains("HR")) targetForm = new EmployeeManagement();
                        else if (item.Contains("GRN")) targetForm = new GoodsReceivedForm();
                        else if (item.Contains("Material")) targetForm = new RawMaterialRequestForm();
                        else if (item.Contains("Procurement")) targetForm = new ProcurementForm();
                        else if (item.Contains("Logout")) { Application.Restart(); return; }

                        if (targetForm != null && !(targetForm is AfterServiceForm))
                        {
                            this.Hide();
                            targetForm.FormClosed += (senderForm, args) => this.Show();
                            targetForm.Show();
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Navigation error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                };
                pnlSidebar.Controls.Add(btnMenu);
                btnTop += 55;
            }
            this.Controls.Add(pnlSidebar);

            // 2. Right Main Workspace
            Panel pnlMain = new Panel { Location = new Point(260, 0), Size = new Size(900, 750) };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label { Text = "Customer Support & Ticketing System", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(30, 20), AutoSize = true };
            pnlMain.Controls.Add(lblHeader);

            // 3. Input Form Card
            Panel pnlCard = new Panel { Location = new Point(30, 85), Size = new Size(380, 600), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlCard);

            Label lblCardTitle = new Label { Text = "📝 Support Ticket Details", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(20, 15), AutoSize = true };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 60;
            txtComplaintID = CreateStyledTextBox(pnlCard, ref startY, "Ticket / Complaint ID (Auto-Generated):", true);
            txtCustomerID = CreateStyledTextBox(pnlCard, ref startY, "Customer ID *:", false);
            txtOrderID = CreateStyledTextBox(pnlCard, ref startY, "Related Order ID (Optional):", false);

            Label lblStatus = new Label { Text = "Resolution Status *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboStatus = new ComboBox { Location = new Point(20, startY + 22), Width = 335, Font = new Font("Segoe UI", 10.5F), DropDownStyle = ComboBoxStyle.DropDownList };
            pnlCard.Controls.Add(lblStatus);
            pnlCard.Controls.Add(cboStatus);
            startY += 75;

            // Description Box transformed into an active Real-time History Filter
            Label lblDesc = new Label { Text = "🔍 Live Filter History (Type ID to search):", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235) };
            txtDescription = new TextBox { Location = new Point(20, startY + 22), Width = 335, Height = 90, Multiline = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            txtDescription.TextChanged += txtDescription_TextChanged; // Real-time search connection
            pnlCard.Controls.Add(lblDesc);
            pnlCard.Controls.Add(txtDescription);
            startY += 125;

            // Buttons
            btnSubmit = new Button { Text = "💾 Save / Update Ticket", Location = new Point(20, startY), Size = new Size(160, 42), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += btnSubmitComplaint_Click;
            pnlCard.Controls.Add(btnSubmit);

            btnClear = new Button { Text = "✨ Add New Data", Location = new Point(195, startY), Size = new Size(160, 42), BackColor = Color.FromArgb(71, 85, 105), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, e) => ClearFields();
            pnlCard.Controls.Add(btnClear);

            // 4. Data Grid
            Label lblGridTitle = new Label { Text = "📂 Support Tickets History", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(440, 85), AutoSize = true };
            pnlMain.Controls.Add(lblGridTitle);

            dgvComplaints = new DataGridView { Location = new Point(440, 125), Size = new Size(430, 560), BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
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
            Label lbl = new Label { Text = labelText, Location = Point.Empty, AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            lbl.Location = new Point(20, topY);
            TextBox txt = new TextBox { Location = new Point(20, topY + 22), Width = 335, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            if (readOnly) { txt.ReadOnly = true; txt.BackColor = Color.FromArgb(241, 245, 249); }
            container.Controls.Add(lbl); container.Controls.Add(txt);
            topY += 65;
            return txt;
        }
        #endregion

        #region 📦 Core Logic
        private void SetupDropdowns()
        {
            cboStatus.Items.Clear();
            cboStatus.Items.AddRange(new string[] { "Pending", "In Progress", "Resolved", "Refunded", "Closed" });
        }

        private void GenerateNewTicketID()
        {
            txtComplaintID.Text = "COMP-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }

        // 🛠️ Structural Integrity Protection: Builds missing schema layout automatically
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
                    using (MySqlCommand cmd = new MySqlCommand(createTableQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
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
                    string query = "SELECT ComplaintID, CustomerID, OrderID, Date, Status FROM complaint ORDER BY Date DESC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        originalComplaintsTable = new DataTable();
                        adapter.Fill(originalComplaintsTable);
                        dgvComplaints.DataSource = originalComplaintsTable;
                    }

                    // English Header Mapping
                    if (dgvComplaints.Columns.Contains("ComplaintID")) dgvComplaints.Columns["ComplaintID"].HeaderText = "Ticket ID";
                    if (dgvComplaints.Columns.Contains("CustomerID")) dgvComplaints.Columns["CustomerID"].HeaderText = "Customer ID";
                    if (dgvComplaints.Columns.Contains("OrderID")) dgvComplaints.Columns["OrderID"].HeaderText = "Order ID";
                    if (dgvComplaints.Columns.Contains("Date")) dgvComplaints.Columns["Date"].HeaderText = "Created Date";
                    if (dgvComplaints.Columns.Contains("Status")) dgvComplaints.Columns["Status"].HeaderText = "Status";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database Load Error: " + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AfterServiceForm_Load(object sender, EventArgs e)
        {

        }

        private void txtDescription_TextChanged(object sender, EventArgs e)
        {
            if (dgvComplaints.DataSource is DataTable dt)
            {
                string keyword = txtDescription.Text.Trim().Replace("'", "''");
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    dt.DefaultView.RowFilter = "";
                }
                else
                {
                    dt.DefaultView.RowFilter = string.Format("ComplaintID LIKE '%{0}%' OR CustomerID LIKE '%{0}%' OR OrderID LIKE '%{0}%'", keyword);
                }
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

                string status = row.Cells["Status"].Value?.ToString();
                if (cboStatus.Items.Contains(status)) cboStatus.Text = status;
            }
        }

        private void btnSubmitComplaint_Click(object sender, EventArgs e)
        {
            string customerID = txtCustomerID.Text.Trim();
            string orderID = txtOrderID.Text.Trim();

            if (string.IsNullOrWhiteSpace(customerID) || string.IsNullOrWhiteSpace(cboStatus.Text))
            {
                MessageBox.Show("Please provide at least the Customer ID and Resolution Status!", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    // Verification Check: Customer presence inside base schema records
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

                    // Verification Check: Order reference presence consistency if typed
                    if (!string.IsNullOrWhiteSpace(orderID))
                    {
                        string checkOrderSql = "SELECT COUNT(*) FROM orders WHERE OrderID = @OrderID";
                        using (MySqlCommand cmdCheckOrder = new MySqlCommand(checkOrderSql, conn))
                        {
                            cmdCheckOrder.Parameters.AddWithValue("@OrderID", orderID);
                            if (Convert.ToInt32(cmdCheckOrder.ExecuteScalar()) == 0)
                            {
                                MessageBox.Show($"Associated Order ID '{orderID}' does not exist inside our records! Leave blank or provide a valid code mapping.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                    }

                    string sql = @"INSERT INTO complaint (ComplaintID, CustomerID, OrderID, Date, Status) 
                                   VALUES (@CID, @CustID, @OID, NOW(), @Status)
                                   ON DUPLICATE KEY UPDATE Status = @Status, OrderID = @OID;";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CID", txtComplaintID.Text.Trim());
                        cmd.Parameters.AddWithValue("@CustID", customerID);
                        cmd.Parameters.AddWithValue("@OID", string.IsNullOrWhiteSpace(orderID) ? (object)DBNull.Value : orderID);
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
            txtCustomerID.Clear();
            txtOrderID.Clear();
            txtDescription.Clear();
            cboStatus.SelectedIndex = -1;
            dgvComplaints.ClearSelection();
            GenerateNewTicketID();
        }
        #endregion
    }
}