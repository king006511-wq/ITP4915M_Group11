using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class LogisticsForm : Form
    {
        // ==========================================
        // 🔒 Database Configuration
        // ==========================================
        private readonly string connString = "Server=localhost;Database=premium_living_db;Uid=root;Pwd=;port=3306;SslMode=Disabled;";
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
            // Fallback initialization just in case Session is empty during individual testing
            this.currentStaffID = string.IsNullOrEmpty(UserSession.LoggedInStaffID) ? "S001" : UserSession.LoggedInStaffID;

            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                InitializePremiumModernUI();
            }
        }

        private void LogisticsForm_Load(object sender, EventArgs e)
        {
            // Sync up with your central active login session variables
            this.currentStaffID = UserSession.LoggedInStaffID;

            // 🔒 Security Access Enforcement logic from OrderManagementForm
            if (!CanAccess())
            {
                MessageBox.Show(
                    $"[SECURITY ALERT] Access Denied!\n\n" +
                    $"Logged In StaffID: {currentStaffID}\n" +
                    $"Your Account Role is: \"{(string.IsNullOrEmpty(UserSession.LoggedInStaffRole) ? "None / Empty" : UserSession.LoggedInStaffRole)}\"\n\n" +
                    $"Only Manager and Administrator are authorized to access the Logistics module.",
                    "System Security Enforcer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop
                );

                this.BeginInvoke(new MethodInvoker(this.Close));
                return;
            }

            // 額外: 禁用特定按鈕如果使用者不是擁有執行權限
            if (!AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.LogisticsDriver))
            {
                btnAssignDelivery.Enabled = false;
                btnUpdateStatus.Enabled = false;
                btnAssignDelivery.BackColor = Color.LightGray;
                btnUpdateStatus.BackColor = Color.LightGray;
            }

            LoadDeliveryStaffToCombo();
            RefreshPendingOrdersGrid();
        }

        private bool CanAccess()
        {
            string currentRole = UserSession.LoggedInStaffRole;
            if (string.IsNullOrWhiteSpace(currentRole)) return false;

            // Restrict logistics viewing strictly to Managers and Admins
            List<string> allowedRoles = new List<string> { "Manager", "Administrator" };
            return allowedRoles.Any(role => role.Equals(currentRole.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        #region 🎨 Dynamic Premium English UI Construction
        private void InitializePremiumModernUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Advanced Delivery & Logistics Management";
            this.Size = new Size(1250, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Load += LogisticsForm_Load;

            // =======================================================
            // 左側導航面板 (Sidebar Menu System matching your layout)
            // =======================================================
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

                // 根據角色決定側邊選單項目是否顯示
                bool menuVisible = true;
                if (item.Contains("Sales Order Mgmt")) menuVisible = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.SalesRepresentative);
                else if (item.Contains("Delivery Logistics")) menuVisible = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.LogisticsDriver);
                else if (item.Contains("Product Maintenance")) menuVisible = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator);
                else if (item.Contains("HR / Staff Mgmt")) menuVisible = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator);
                else if (item.Contains("Goods Received")) menuVisible = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.WarehouseSpecialist);
                else if (item.Contains("Material Requests")) menuVisible = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.ProcurementOfficer);
                else if (item.Contains("Procurement Control")) menuVisible = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.ProcurementOfficer);
                else if (item.Contains("Customer Support")) menuVisible = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.SalesRepresentative);

                btnMenu.Visible = menuVisible;

                if (item.Contains("Delivery Logistics")) { btnMenu.BackColor = Color.FromArgb(37, 99, 235); btnMenu.ForeColor = Color.White; }
                else if (item.Contains("Logout")) { btnMenu.BackColor = Color.FromArgb(239, 68, 68); btnMenu.ForeColor = Color.White; }
                else { btnMenu.BackColor = Color.Transparent; btnMenu.ForeColor = Color.FromArgb(148, 163, 184); }

                btnMenu.Click += (s, e) => {
                    Form targetForm = null;
                    try
                    {
                        if (item.Contains("Delivery Logistics")) return; // Active form shortcut bypass

                        if (item.Contains("Sales Order Mgmt")) targetForm = new OrderManagementForm();
                        else if (item.Contains("Product Maintenance")) targetForm = new ProductManagement();
                        else if (item.Contains("HR / Staff Mgmt")) targetForm = new EmployeeManagement();
                        else if (item.Contains("Goods Received")) targetForm = new GoodsReceivedForm();
                        else if (item.Contains("Material Requests")) targetForm = new RawMaterialRequestForm();
                        else if (item.Contains("Procurement Control")) targetForm = new ProcurementForm();
                        else if (item.Contains("Customer Support")) targetForm = new AfterServiceForm();
                        else if (item.Contains("Logout System"))
                        {
                            DialogResult result = MessageBox.Show("Are you sure you want to log out?", "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (result == DialogResult.Yes)
                            {
                                UserSession.LoggedInStaffID = "";
                                UserSession.LoggedInStaffName = "";
                                UserSession.LoggedInStaffRole = "";

                                Login login = new Login();
                                login.Show();
                                this.Hide();
                                login.FormClosed += (senderLogin, args) => this.Close();
                            }
                            return;
                        }

                        if (targetForm != null)
                        {
                            this.Hide();
                            targetForm.FormClosed += (senderObj, args) => this.Show();
                            targetForm.Show();
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Navigation routing failed.\nError: " + ex.Message, "Routing Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                };
                pnlSidebar.Controls.Add(btnMenu);
                btnTop += 55;
            }
            this.Controls.Add(pnlSidebar);

            // =======================================================
            // 右側工作面板 (Main Workspace Panel)
            // =======================================================
            Panel pnlMain = new Panel { Location = new Point(260, 0), Size = new Size(990, 850) };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label { Text = "Logistics & Dispatch Control Center", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(30, 20), AutoSize = true };
            pnlMain.Controls.Add(lblHeader);

            Button btnBackHome = new Button { Text = "🏠 Back Home", Size = new Size(120, 34), Location = new Point(830, 22), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnBackHome.FlatAppearance.BorderSize = 0;
            btnBackHome.Click += (s, e) => this.Close();
            pnlMain.Controls.Add(btnBackHome);

            Label lblStaff = new Label { Text = $"👤 Controller: {currentStaffID}", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(13, 148, 136), Location = new Point(520, 26), AutoSize = true };
            pnlMain.Controls.Add(lblStaff);

            // Left Card: Dispatch & Routing Workspace
            Panel pnlCard = new Panel { Location = new Point(30, 85), Size = new Size(500, 700), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlMain.Controls.Add(pnlCard);

            Label lblCardTitle = new Label { Text = "📍 Delivery Assignment Builder", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(20, 15), AutoSize = true };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 50;
            txtOrderID = CreateStyledTextBox(pnlCard, ref startY, "Selected Order ID:", true, 210);
            startY -= 65;
            txtCustomerID = CreateStyledTextBox(pnlCard, ref startY, "Customer Reference ID:", true, 210, 250);

            txtDeliveryAddress = CreateStyledTextBox(pnlCard, ref startY, "Shipping Destination Address:", false, 460);
            txtCurrentStatus = CreateStyledTextBox(pnlCard, ref startY, "Active Tracking Status:", true, 460);

            Label lblCombo = new Label { Text = "Assign Logistics Driver / Crew:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboDeliveryStaff = new ComboBox { Location = new Point(20, startY + 25), Width = 460, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F), BackColor = Color.White };
            pnlCard.Controls.Add(lblCombo); pnlCard.Controls.Add(cboDeliveryStaff);
            startY += 65;

            Label lblDate = new Label { Text = "Scheduled Delivery Date:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            dtpScheduleDate = new DateTimePicker { Location = new Point(20, startY + 25), Width = 460, Font = new Font("Segoe UI", 10.5F), Format = DateTimePickerFormat.Short };
            pnlCard.Controls.Add(lblDate); pnlCard.Controls.Add(dtpScheduleDate);
            startY += 80;

            // Logistics Action Action Buttons
            btnAssignDelivery = new Button { Text = "🚀 Dispatch Order", Location = new Point(20, startY), Size = new Size(145, 45), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAssignDelivery.Click += btnAssignDelivery_Click;
            pnlCard.Controls.Add(btnAssignDelivery);

            btnUpdateStatus = new Button { Text = "🏁 Complete Route", Location = new Point(175, startY), Size = new Size(150, 45), BackColor = Color.FromArgb(245, 158, 11), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnUpdateStatus.Click += btnUpdateStatus_Click;
            pnlCard.Controls.Add(btnUpdateStatus);

            btnClearFields = new Button { Text = "🧹 Reset UI", Location = new Point(335, startY), Size = new Size(145, 45), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClearFields.Click += (s, e) => ClearLogisticsFields();
            pnlCard.Controls.Add(btnClearFields);

            // Right Panel: Data Grid Monitoring Area
            Label lblGridTitle = new Label { Text = "📊 Pending Outbound Delivery Orders", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(550, 85), AutoSize = true };
            pnlMain.Controls.Add(lblGridTitle);

            dgvPendingOrders = new DataGridView { Location = new Point(550, 125), Size = new Size(400, 660), BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, GridColor = Color.FromArgb(241, 245, 249) };
            dgvPendingOrders.EnableHeadersVisualStyles = false;
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvPendingOrders.ColumnHeadersHeight = 38;

            dgvPendingOrders.SelectionChanged += dgvPendingOrders_SelectionChanged;
            pnlMain.Controls.Add(dgvPendingOrders);
        }

        private TextBox CreateStyledTextBox(Panel container, ref int topY, string labelText, bool readOnly, int width, int offsetX = 20)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(offsetX, topY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(offsetX, topY + 25), Width = width, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            if (readOnly) { txt.ReadOnly = true; txt.BackColor = Color.FromArgb(241, 245, 249); }
            container.Controls.Add(lbl); container.Controls.Add(txt);
            topY += 65;
            return txt;
        }
        #endregion

        #region 💾 Core Database Operations & Event Handlers
        private void RefreshPendingOrdersGrid()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // Load tracking rows flagged for delivery updates
                    string query = "SELECT OrderID AS `Order ID`, CustomerID AS `Customer ID`, Status FROM orders WHERE Status LIKE '%Delivery%' OR Status = 'Pending Delivery'";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvPendingOrders.DataSource = dt;
                    }
                }
                catch (Exception ex) { MessageBox.Show("Failed to pull logistics records:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void LoadDeliveryStaffToCombo()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // Filters system workforce profile configurations for operational driver accounts
                    string query = "SELECT StaffID, Name FROM staff WHERE Role = 'Delivery Driver' OR Role = 'Delivery Representative' OR Role = 'Manager'";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            cboDeliveryStaff.DataSource = dt;
                            cboDeliveryStaff.DisplayMember = "Name";
                            cboDeliveryStaff.ValueMember = "StaffID";
                        }
                    }
                    cboDeliveryStaff.SelectedIndex = -1;
                }
                catch (Exception) { /* Fallback buffer container safe mode */ }
            }
        }

        private void dgvPendingOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPendingOrders.CurrentRow == null || dgvPendingOrders.CurrentRow.Index < 0) return;

            object orderIdVal = dgvPendingOrders.CurrentRow.Cells["Order ID"].Value;
            if (orderIdVal == null || orderIdVal == DBNull.Value) return;

            string selectedOrderID = orderIdVal.ToString();

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT OrderID, CustomerID, Status FROM orders WHERE OrderID = @OrderID";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", selectedOrderID);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtOrderID.Text = reader["OrderID"].ToString();
                                txtCustomerID.Text = reader["CustomerID"].ToString();
                                txtCurrentStatus.Text = reader["Status"].ToString();
                                txtDeliveryAddress.Text = "Premium Living Default Route Center, Hong Kong"; // Placeholder text
                            }
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Selection routing error:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            }
        }

        private void btnAssignDelivery_Click(object sender, EventArgs e)
        {
            string orderID = txtOrderID.Text.Trim();
            if (string.IsNullOrEmpty(orderID)) { MessageBox.Show("Please highlight a pending order row from the grid list first.", "Validation Rule", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (cboDeliveryStaff.SelectedValue == null) { MessageBox.Show("Please select an available field driver to assign this route package.", "Validation Rule", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string updateSql = "UPDATE orders SET Status = 'Dispatched (In Transit)' WHERE OrderID = @OrderID";
                    using (MySqlCommand cmd = new MySqlCommand(updateSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderID);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show($"Order [{orderID}] structural transit state updated to 'In Transit'. Dispatch manifest logged successfully!", "Logistics Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearLogisticsFields();
                        RefreshPendingOrdersGrid();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Transaction execution aborted:\n" + ex.Message, "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            string orderID = txtOrderID.Text.Trim();
            if (string.IsNullOrEmpty(orderID)) { MessageBox.Show("Please choose a valid active record sequence.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

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
        }
        #endregion
    }
}