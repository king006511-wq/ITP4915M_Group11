using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class LogisticsForm : Form
    {
        // ==========================================
        // 🔒 Database Configuration
        // ==========================================
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";
        private string currentStaffID;

        // ==========================================
        // 🎨 UI Elements & Logistics Controls
        // ==========================================
        private DataGridView dgvPendingOrders;
        private TextBox txtOrderID, txtCustomerID, txtDeliveryAddress, txtCurrentStatus;
        private ComboBox cboDeliveryStaff;
        private DateTimePicker dtpScheduleDate;
        private Button btnAssignDelivery, btnUpdateStatus, btnClearFields;

        public LogisticsForm()
        {
            this.currentStaffID = string.IsNullOrEmpty(UserSession.LoggedInStaffID) ? "S001" : UserSession.LoggedInStaffID;

            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                InitializePremiumModernUI(); // 🛠️ Loaded with strict TableLayoutPanel Grid to prevent ALL overlaps
                LoadDeliveryStaff();
                RefreshPendingOrdersGrid();
            }
        }

        #region 🎨 Dynamic Premium English UI Construction (Strict Table Grid)
        private void InitializePremiumModernUI()
        {
            this.Controls.Clear();
            this.BackColor = Color.FromArgb(241, 245, 249);
            this.FormBorderStyle = FormBorderStyle.None;

            // 🌟 1. Master Table Layout (Prevents overlapping permanently)
            TableLayoutPanel mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F)); // Header strictly 70px tall
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Content takes exact remaining height
            this.Controls.Add(mainTable);

            // 🌟 2. Header Panel
            Panel pnlHeader = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            Label lblModuleTitle = new Label
            {
                Text = "Logistics Dispatch & Delivery Management",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(20, 20),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblModuleTitle);
            mainTable.Controls.Add(pnlHeader, 0, 0);

            // 🌟 3. Content Table (Left Inputs + Middle Spacer + Right Grid)
            TableLayoutPanel contentTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(20, 0, 20, 20) // Outer bounds padding
            };
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F)); // Left Panel rigidly 360px wide
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));  // Spacer rigidly 20px wide
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // Right Grid exactly fills the rest
            mainTable.Controls.Add(contentTable, 0, 1);

            // 🌟 4. Left Panel: Inputs (With AutoScroll to prevent cutoff)
            Panel pnlInputs = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true, // Enables scrolling if the laptop screen is too small vertically
                Padding = new Padding(20)
            };
            contentTable.Controls.Add(pnlInputs, 0, 0);

            // 🌟 5. Right Panel: Data Grid
            dgvPendingOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, // Ensures data columns fill nicely
                Font = new Font("Segoe UI", 9.5F),
                ReadOnly = true
            };
            dgvPendingOrders.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvPendingOrders.SelectionChanged += dgvPendingOrders_SelectionChanged;
            contentTable.Controls.Add(dgvPendingOrders, 2, 0);

            // ====================================================
            // 📝 Populate Left Input Panel
            // ====================================================
            int currentY = 15;
            int inputWidth = 300; // Carefully sized to fit within the 360px panel width without clipping

            Label lblOrderID = new Label { Text = "Target Order ID", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(15, currentY), AutoSize = true };
            txtOrderID = new TextBox { Location = new Point(15, currentY + 25), Size = new Size(inputWidth, 30), Font = new Font("Segoe UI", 10F), ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249) };
            pnlInputs.Controls.Add(lblOrderID); pnlInputs.Controls.Add(txtOrderID);
            currentY += 65;

            Label lblCust = new Label { Text = "Customer ID", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(15, currentY), AutoSize = true };
            txtCustomerID = new TextBox { Location = new Point(15, currentY + 25), Size = new Size(inputWidth, 30), Font = new Font("Segoe UI", 10F), ReadOnly = true, BackColor = Color.White };
            pnlInputs.Controls.Add(lblCust); pnlInputs.Controls.Add(txtCustomerID);
            currentY += 65;

            Label lblAddress = new Label { Text = "Destination Address", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(15, currentY), AutoSize = true };
            txtDeliveryAddress = new TextBox { Location = new Point(15, currentY + 25), Size = new Size(inputWidth, 55), Font = new Font("Segoe UI", 10F), Multiline = true, ReadOnly = true, BackColor = Color.White };
            pnlInputs.Controls.Add(lblAddress); pnlInputs.Controls.Add(txtDeliveryAddress);
            currentY += 95;

            Label lblStatus = new Label { Text = "Current State", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(15, currentY), AutoSize = true };
            txtCurrentStatus = new TextBox { Location = new Point(15, currentY + 25), Size = new Size(inputWidth, 30), Font = new Font("Segoe UI", 10F), ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249) };
            pnlInputs.Controls.Add(lblStatus); pnlInputs.Controls.Add(txtCurrentStatus);
            currentY += 65;

            Label lblStaff = new Label { Text = "Assign Delivery Team *", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(15, currentY), AutoSize = true };
            cboDeliveryStaff = new ComboBox { Location = new Point(15, currentY + 25), Size = new Size(inputWidth, 30), Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };
            pnlInputs.Controls.Add(lblStaff); pnlInputs.Controls.Add(cboDeliveryStaff);
            currentY += 65;

            Label lblDate = new Label { Text = "Scheduled Date *", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(15, currentY), AutoSize = true };
            dtpScheduleDate = new DateTimePicker { Location = new Point(15, currentY + 25), Size = new Size(inputWidth, 30), Font = new Font("Segoe UI", 10F), Format = DateTimePickerFormat.Short };
            pnlInputs.Controls.Add(lblDate); pnlInputs.Controls.Add(dtpScheduleDate);
            currentY += 75;

            btnAssignDelivery = new Button { Text = "Dispatch Order", Location = new Point(15, currentY), Size = new Size(140, 35), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(14, 165, 233), Cursor = Cursors.Hand };
            btnAssignDelivery.FlatAppearance.BorderSize = 0;
            btnAssignDelivery.Click += btnAssignDelivery_Click;

            btnUpdateStatus = new Button { Text = "Mark Delivered", Location = new Point(175, currentY), Size = new Size(140, 35), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(16, 185, 129), Cursor = Cursors.Hand };
            btnUpdateStatus.FlatAppearance.BorderSize = 0;
            btnUpdateStatus.Click += btnUpdateStatus_Click;
            currentY += 45;

            btnClearFields = new Button { Text = "Reset Form", Location = new Point(15, currentY), Size = new Size(300, 35), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), BackColor = Color.FromArgb(226, 232, 240), Cursor = Cursors.Hand };
            btnClearFields.FlatAppearance.BorderSize = 0;
            btnClearFields.Click += (s, e) => ClearLogisticsFields();

            pnlInputs.Controls.Add(btnAssignDelivery);
            pnlInputs.Controls.Add(btnUpdateStatus);
            pnlInputs.Controls.Add(btnClearFields);
        }
        #endregion

        #region ⚙️ Operational Logic & Core Engines
        private void LoadDeliveryStaff()
        {
            cboDeliveryStaff.Items.Clear();
            cboDeliveryStaff.Items.Add("Team A - John Doe");
            cboDeliveryStaff.Items.Add("Team B - Michael Smith");
            cboDeliveryStaff.Items.Add("Team C - David Wong");
            cboDeliveryStaff.Items.Add("Outsource - SF Express");
        }

        private void LogisticsForm_Load(object sender, EventArgs e) { }

        private void RefreshPendingOrdersGrid()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT OrderID AS 'Order ID', CustomerID AS 'Customer', Status, OrderDate AS 'Date' FROM orders WHERE Status != 'Delivery Completed' ORDER BY OrderDate DESC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvPendingOrders.DataSource = dt;
                    }
                }
                catch (Exception) { /* Silent fail fallback */ }
            }
        }

        private void dgvPendingOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPendingOrders.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvPendingOrders.SelectedRows[0];
                txtOrderID.Text = row.Cells["Order ID"].Value?.ToString() ?? "";
                txtCustomerID.Text = row.Cells["Customer"].Value?.ToString() ?? "";
                txtCurrentStatus.Text = row.Cells["Status"].Value?.ToString() ?? "";
                txtDeliveryAddress.Text = "Standard Registered Address (Please verify with client)";
            }
        }

        private void btnAssignDelivery_Click(object sender, EventArgs e)
        {
            string orderID = txtOrderID.Text.Trim();
            if (string.IsNullOrEmpty(orderID) || cboDeliveryStaff.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an Order and assign a Delivery Team first.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string updateSql = "UPDATE orders SET Status = 'Dispatched' WHERE OrderID = @OrderID";
                    using (MySqlCommand cmd = new MySqlCommand(updateSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderID);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show($"Order [{orderID}] has been successfully dispatched to {cboDeliveryStaff.Text}.", "Dispatch Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearLogisticsFields();
                        RefreshPendingOrdersGrid();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Database write fault operation failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            string orderID = txtOrderID.Text.Trim();
            if (string.IsNullOrEmpty(orderID))
            {
                MessageBox.Show("Please select an Order to mark as completed.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string updateSql = "UPDATE orders SET Status = 'Delivery Completed' WHERE OrderID = @OrderID";
                    using (MySqlCommand cmd = new MySqlCommand(updateSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderID);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show($"Order [{orderID}] has been finalized as successfully delivered to destination site.", "Delivery Closed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearLogisticsFields();
                        RefreshPendingOrdersGrid();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Database write fault operation failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void ClearLogisticsFields()
        {
            txtOrderID.Clear();
            txtCustomerID.Clear();
            txtDeliveryAddress.Clear();
            txtCurrentStatus.Clear();
            if (cboDeliveryStaff.Items.Count > 0) cboDeliveryStaff.SelectedIndex = -1;
            dtpScheduleDate.Value = DateTime.Now;
            dgvPendingOrders.ClearSelection();
        }
        #endregion
    }
}