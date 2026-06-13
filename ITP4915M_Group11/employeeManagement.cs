using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class EmployeeManagement : Form
    {
        // ==========================================
        // 🔒 Database Configuration
        // ==========================================
        private readonly string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // ==========================================
        // 🎨 Modern UI Element Variables
        // ==========================================
        private TextBox txtStaffID, txtName, txtPassword;
        private ComboBox cboRole;
        private DataGridView dgvStaff;
        private Button btnAddStaff, btnUpdate, btnDelete, btnReset;

        public EmployeeManagement()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                InitializePremiumModernUI(); // Initialize Pure English Dynamic UI
                SetupRoleControls();         // Ingest Role options into ComboBox
                LoadStaffData();             // Fetch staff list from Database
            }
        }

        #region 🔒 System Security Gatekeeper Enforcement
        private void EmployeeManagement_Load(object sender, EventArgs e)
        {
            // 🎯 Check if the user has Manager or Administrator permissions
            string currentRole = UserSession.LoggedInStaffRole;
            string currentStaffID = UserSession.LoggedInStaffID;

            bool isAuthorized = !string.IsNullOrEmpty(currentRole) &&
                                (currentRole.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                                 currentRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase));

            if (!isAuthorized)
            {
                MessageBox.Show(
                    $"[SECURITY ALERT] Access Denied!\n\n" +
                    $"Logged In Staff ID: {(string.IsNullOrEmpty(currentStaffID) ? "Unknown" : currentStaffID)}\n" +
                    $"Your Account Role is: \"{(string.IsNullOrEmpty(currentRole) ? "None" : currentRole)}\"\n\n" +
                    $"Only a Manager or Administrator is authorized to access Employee Management profiles.",
                    "System Security Guard",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop
                );

                // Gracefully abort and force close the form context before it finishes rendering
                this.BeginInvoke(new MethodInvoker(this.Close));
            }
        }
        #endregion

        #region 🎨 Dynamic Premium English UI Construction
        private void InitializePremiumModernUI()
        {
            // 1. Clear old control leftovers to prevent overlapping or designer mismatches
            this.Controls.Clear();

            // 2. Main Window Settings
            this.Text = "Premium Living Furniture - HR & Staff Resource Control Center";
            this.Size = new Size(1180, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Wire up the load event handler to trigger security checking routines
            this.Load += EmployeeManagement_Load;

            // 3. Left Sidebar Navigation Panel
            Panel pnlSidebar = new Panel
            {
                Width = 260,
                Dock = DockStyle.Left,
                BackColor = Color.FromArgb(15, 23, 42)
            };

            Label lblLogo = new Label
            {
                Text = "Premium Living\nFurniture",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 25),
                Size = new Size(220, 60),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlSidebar.Controls.Add(lblLogo);

            string[] menuItems = {
                "🛒 Sales Order Mgmt",
                "🚚 Delivery Logistics",
                "🛋️ Product Maintenance",
                "👔 HR / Staff Mgmt",
                "📦 Goods Received (GRN)",
                "🏭 Material Requests",
                "📊 Procurement Control",
                "🔧 Customer Support",
                "🚪 Logout System"
            };

            int btnTop = 110;
            foreach (string item in menuItems)
            {
                Button btnMenu = new Button
                {
                    Text = "  " + item,
                    Top = btnTop,
                    Left = 12,
                    Size = new Size(236, 48),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand
                };
                btnMenu.FlatAppearance.BorderSize = 0;

                // Highlight current HR / Staff Management workspace; Logout shown as danger red
                if (item.Contains("HR / Staff Mgmt"))
                {
                    btnMenu.BackColor = Color.FromArgb(37, 99, 235);
                    btnMenu.ForeColor = Color.White;
                }
                // Check specifically for Logout System
                else if (item.Contains("Logout System"))
                {
                    btnMenu.BackColor = Color.FromArgb(239, 68, 68);
                    btnMenu.ForeColor = Color.White;
                    btnMenu.MouseEnter += (s, e) => { btnMenu.BackColor = Color.FromArgb(220, 38, 38); };
                    btnMenu.MouseLeave += (s, e) => { btnMenu.BackColor = Color.FromArgb(239, 68, 68); };
                }
                else
                {
                    btnMenu.BackColor = Color.Transparent;
                    btnMenu.ForeColor = Color.FromArgb(148, 163, 184);
                    btnMenu.MouseEnter += (s, e) => { btnMenu.BackColor = Color.FromArgb(51, 65, 85); btnMenu.ForeColor = Color.White; };
                    btnMenu.MouseLeave += (s, e) => { btnMenu.BackColor = Color.Transparent; btnMenu.ForeColor = Color.FromArgb(148, 163, 184); };
                }

                // Module Router Linkage
                btnMenu.Click += (s, e) => {
                    Form targetForm = null;
                    try
                    {
                        if (item.Contains("Sales Order Mgmt")) targetForm = new OrderManagementForm();
                        else if (item.Contains("Delivery Logistics")) targetForm = new LogisticsForm();
                        else if (item.Contains("Product Maintenance")) targetForm = new ProductManagement();
                        else if (item.Contains("Goods Received (GRN)")) targetForm = new GoodsReceivedForm();
                        else if (item.Contains("Material Requests")) targetForm = new RawMaterialRequestForm();
                        else if (item.Contains("Procurement")) targetForm = new ProcurementForm();
                        else if (item.Contains("Support")) targetForm = new AfterServiceForm();
                        else if (item.Contains("Logout System")) { Application.Restart(); return; }

                        if (targetForm != null && !(targetForm is EmployeeManagement))
                        {
                            this.Hide();
                            targetForm.FormClosed += (senderForm, args) => this.Show();
                            targetForm.Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Navigation routing error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                pnlSidebar.Controls.Add(btnMenu);
                btnTop += 55;
            }
            this.Controls.Add(pnlSidebar);

            // 4. Right Main Workspace
            Panel pnlMain = new Panel
            {
                Location = new Point(260, 0),
                Size = new Size(900, 750)
            };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label
            {
                Text = "HR & Employee Resource Management Center",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(30, 20),
                AutoSize = true
            };
            pnlMain.Controls.Add(lblHeader);

            // Back Home button
            Button btnBackHome = new Button { Text = "🏠 Back Home", Size = new Size(120, 34), Location = new Point(740, 22), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnBackHome.FlatAppearance.BorderSize = 0;
            btnBackHome.Click += (s, e) => {
                try
                {
                    // Attempt to locate and show an existing main dashboard form if available
                    foreach (Form f in Application.OpenForms)
                    {
                        if (f.GetType().Name == "MainDashboard")
                        {
                            this.Hide();
                            f.Show();
                            return;
                        }
                    }
                    // Fallback: restart the application to simulate returning to home
                    Application.Restart();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Navigation error: " + ex.Message, "Navigation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            pnlMain.Controls.Add(btnBackHome);

            // 5. Input Parameter Card Panel (Left Side Container)
            Panel pnlCard = new Panel
            {
                Location = new Point(30, 85),
                Size = new Size(420, 620),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlCard);

            Label lblCardTitle = new Label
            {
                Text = "📋 Staff Profile Parameters",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 65;

            // Form Input Layouts
            txtStaffID = CreateStyledTextBox(pnlCard, ref startY, "Staff ID / System Username *:", false);
            txtName = CreateStyledTextBox(pnlCard, ref startY, "Full Employee Name *:", false);
            txtPassword = CreateStyledTextBox(pnlCard, ref startY, "Account Access Password *:", false);

            // Dropdown Role Picker Spec
            Label lblRole = new Label { Text = "Corporate Authorization Role *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboRole = new ComboBox { Location = new Point(20, startY + 22), Width = 375, Font = new Font("Segoe UI", 10.5F), DropDownStyle = ComboBoxStyle.DropDownList };
            pnlCard.Controls.Add(lblRole);
            pnlCard.Controls.Add(cboRole);
            startY += 85;

            // 6. Styled Action CRUD Button Cluster
            btnAddStaff = new Button
            {
                Text = "➕ Add New Employee",
                Location = new Point(20, startY),
                Size = new Size(375, 42),
                BackColor = Color.FromArgb(16, 185, 129), // Emerald Green
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddStaff.FlatAppearance.BorderSize = 0;
            btnAddStaff.Click += btnAddStaff_Click;
            pnlCard.Controls.Add(btnAddStaff);
            startY += 52;

            btnUpdate = new Button
            {
                Text = "🔄 Update Staff Records",
                Location = new Point(20, startY),
                Size = new Size(375, 42),
                BackColor = Color.FromArgb(245, 158, 11), // Amber Orange
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.Click += btnUpdate_Click;
            pnlCard.Controls.Add(btnUpdate);
            startY += 52;

            btnDelete = new Button
            {
                Text = "🗑️ Terminate & Delete Account",
                Location = new Point(20, startY),
                Size = new Size(375, 42),
                BackColor = Color.FromArgb(239, 68, 68), // Rose Red
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += btnDelete_Click;
            pnlCard.Controls.Add(btnDelete);
            startY += 52;

            // ✨ Reset / Clear Button Layout
            btnReset = new Button
            {
                Text = "🧹 Clear / Reset Fields",
                Location = new Point(20, startY),
                Size = new Size(375, 42),
                BackColor = Color.FromArgb(100, 116, 139), // Slate Gray
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Click += (s, e) => {
                dgvStaff.ClearSelection();
                ClearFields();
            };
            pnlCard.Controls.Add(btnReset);

            // 7. Grid View Panel Container (Right Side List View)
            Label lblGridTitle = new Label
            {
                Text = "👥 Corporate Staff Directory Ledger",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(480, 85),
                AutoSize = true
            };
            pnlMain.Controls.Add(lblGridTitle);

            dgvStaff = new DataGridView
            {
                Location = new Point(480, 125),
                Size = new Size(390, 580),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                GridColor = Color.FromArgb(241, 245, 249)
            };

            // Modern Grid Specifications Setup
            dgvStaff.EnableHeadersVisualStyles = false;
            dgvStaff.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvStaff.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvStaff.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvStaff.ColumnHeadersHeight = 38;
            dgvStaff.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvStaff.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
            dgvStaff.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            // Row Selection Tracker Linkage
            dgvStaff.SelectionChanged += dgvStaff_SelectionChanged;

            pnlMain.Controls.Add(dgvStaff);
        }

        private TextBox CreateStyledTextBox(Panel container, ref int topY, string labelText, bool readOnly)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(20, topY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(20, topY + 22), Width = 375, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            if (readOnly)
            {
                txt.ReadOnly = true;
                txt.BackColor = Color.FromArgb(241, 245, 249);
            }
            container.Controls.Add(lbl);
            container.Controls.Add(txt);
            topY += 65;
            return txt;
        }
        #endregion

        #region 📦 Core Operational HR Logic Backends
        private void SetupRoleControls()
        {
            cboRole.Items.Clear();
            cboRole.Items.Add("Manager");
            cboRole.Items.Add("System Manager");
            cboRole.Items.Add("Administrator");
            cboRole.Items.Add("Sales Representative");
            cboRole.Items.Add("Logistics Driver");
            cboRole.Items.Add("Warehouse Specialist");
            cboRole.Items.Add("Procurement Officer");
            cboRole.Items.Add("Factory Operator");
            if (cboRole.Items.Count > 0) cboRole.SelectedIndex = 0;
        }

        private void LoadStaffData()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT StaffID, Name, Password, Role FROM staff";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvStaff.DataSource = dt;
                    }

                    if (dgvStaff.Columns.Contains("Password"))
                        dgvStaff.Columns["Password"].Visible = false;

                    if (dgvStaff.Columns.Contains("StaffID")) dgvStaff.Columns["StaffID"].HeaderText = "Staff ID";
                    if (dgvStaff.Columns.Contains("Name")) dgvStaff.Columns["Name"].HeaderText = "Employee Name";
                    if (dgvStaff.Columns.Contains("Role")) dgvStaff.Columns["Role"].HeaderText = "Role Title";

                    dgvStaff.ClearSelection();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load staff data:\n" + ex.Message);
                }
            }
        }

        private void dgvStaff_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStaff.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvStaff.SelectedRows[0];
                txtStaffID.Text = row.Cells["StaffID"].Value?.ToString() ?? "";
                txtName.Text = row.Cells["Name"].Value?.ToString() ?? "";
                txtPassword.Clear(); // Clear field to indicate password typing for edits
                cboRole.Text = row.Cells["Role"].Value?.ToString() ?? "";

                txtStaffID.ReadOnly = true;
                txtStaffID.BackColor = Color.FromArgb(241, 245, 249);
            }
            else
            {
                ClearFields();
            }
        }

        private void btnAddStaff_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStaffID.Text) || string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please fill out all mandatory profile parameters input blocks fields first!", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = "INSERT INTO staff (StaffID, Name, Password, Role) VALUES (@id, @name, @pass, @role)";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtStaffID.Text.Trim());
                        cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@pass", txtPassword.Text);
                        cmd.Parameters.AddWithValue("@role", cboRole.Text);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Successfully provisioned new personnel access profile accounts record!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            LoadStaffData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Profile provisioning architecture initialization failed:\n" + ex.Message, "Insertion Aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStaffID.Text) || string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please select a registered employee line record ledger entry to manipulate payload updates parameters!", "Validation Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = "UPDATE staff SET Name = @name, Password = @pass, Role = @role WHERE StaffID = @id";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtStaffID.Text.Trim());
                        cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@pass", txtPassword.Text);
                        cmd.Parameters.AddWithValue("@role", cboRole.Text);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Employee credentials parameter updated successfully!", "Commit Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            LoadStaffData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Profile configuration adjustment aborted:\n" + ex.Message, "Transaction Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStaffID.Text))
            {
                MessageBox.Show("Please isolate a target staff row inside the directory registry ledger list map before staging deletion commands!", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Are you absolutely certain you want to permanently revoke system privileges and purge profile record for [{txtName.Text}] (ID: {txtStaffID.Text})?\n\nThis security clearance lifecycle execution cannot be reversed!",
                "Security Warning - Critical Cleansing Command",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                string query = "DELETE FROM staff WHERE StaffID = @id";

                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", txtStaffID.Text.Trim());

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Staff authentication profiles wiped from central records ledger database maps.", "Purge Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearFields();
                                LoadStaffData();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Structural security lifecycle purge execution failed:\n" + ex.Message, "Purge Refused", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ClearFields()
        {
            txtStaffID.Clear();
            txtName.Clear();
            txtPassword.Clear();

            if (cboRole.Items.Count > 0)
                cboRole.SelectedIndex = 0;

            // Unlock Staff ID input box field and revert color schema back to baseline
            txtStaffID.ReadOnly = false;
            txtStaffID.BackColor = Color.White;
        }
        #endregion
    }
}