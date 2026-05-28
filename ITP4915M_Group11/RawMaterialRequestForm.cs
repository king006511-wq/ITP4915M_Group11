using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class RawMaterialRequestForm : Form
    {
        // ==========================================
        // 🔒 Database Configuration
        // ==========================================
        private readonly string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // ==========================================
        // 🎨 Modern UI Element Variables
        // ==========================================
        private TextBox txtCardID, txtPartID, txtQty;
        private DataGridView dgvRequests;
        private Button btnSubmit, btnClear;

        public RawMaterialRequestForm()
        {
            InitializeComponent();
            InitializePremiumModernUI();
            GenerateNewCardID();
            LoadRequests();
        }

        #region 🎨 Dynamic Premium English UI Construction
        private void InitializePremiumModernUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Factory Material Replenishment";
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

                // Highlight current active module
                if (item.Contains("Material Requests"))
                {
                    btnMenu.BackColor = Color.FromArgb(37, 99, 235); btnMenu.ForeColor = Color.White;
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
                        else if (item.Contains("Procurement")) targetForm = new ProcurementForm();
                        else if (item.Contains("Support")) targetForm = new AfterServiceForm();
                        else if (item.Contains("Logout")) { Application.Restart(); return; }

                        if (targetForm != null && !(targetForm is RawMaterialRequestForm))
                        {
                            this.Hide();
                            targetForm.FormClosed += (senderForm, args) => this.Show();
                            targetForm.Show();
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Navigation error: " + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                };
                pnlSidebar.Controls.Add(btnMenu);
                btnTop += 55;
            }
            this.Controls.Add(pnlSidebar);

            // 2. Right Main Workspace
            Panel pnlMain = new Panel { Location = new Point(260, 0), Size = new Size(900, 750) };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label { Text = "Factory Material Reorder Center", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(30, 20), AutoSize = true };
            pnlMain.Controls.Add(lblHeader);

            // 3. Input Form Card
            Panel pnlCard = new Panel { Location = new Point(30, 85), Size = new Size(380, 600), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlCard);

            Label lblCardTitle = new Label { Text = "📝 Create Replenishment Request", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(20, 15), AutoSize = true };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 60;
            txtCardID = CreateStyledTextBox(pnlCard, ref startY, "Reorder Card ID (Auto-Generated):", true);
            txtPartID = CreateStyledTextBox(pnlCard, ref startY, "Part / Material ID (e.g., PART-A01) *:", false);
            txtQty = CreateStyledTextBox(pnlCard, ref startY, "Requested Quantity (Numeric) *:", false);

            // Buttons Array
            btnSubmit = new Button { Text = "📤 Submit Request", Location = new Point(20, startY + 20), Size = new Size(160, 42), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += btnSubmitRequest_Click;
            pnlCard.Controls.Add(btnSubmit);

            btnClear = new Button { Text = "✨ Clear & New", Location = new Point(195, startY + 20), Size = new Size(160, 42), BackColor = Color.FromArgb(71, 85, 105), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, e) => ClearFields();
            pnlCard.Controls.Add(btnClear);

            // 4. Data Grid Component (History View)
            Label lblGridTitle = new Label { Text = "📋 Factory Reorder Ledger", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(440, 85), AutoSize = true };
            pnlMain.Controls.Add(lblGridTitle);

            dgvRequests = new DataGridView { Location = new Point(440, 125), Size = new Size(430, 560), BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvRequests.EnableHeadersVisualStyles = false;
            dgvRequests.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvRequests.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRequests.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvRequests.ColumnHeadersHeight = 38;
            dgvRequests.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvRequests.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);

            // Replaces CellContentClick with robust full-row SelectionChanged
            dgvRequests.SelectionChanged += dgvRequests_SelectionChanged;
            pnlMain.Controls.Add(dgvRequests);
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
        #endregion

        #region 📦 Core Application Logic
        private void GenerateNewCardID()
        {
            // Generates a unique RC (Reorder Card) timestamped identifier
            txtCardID.Text = "RC-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }

        private void LoadRequests()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // Fetch raw data without alias to avoid matching issues later, we format Headers in C#
                    string query = "SELECT ReOrderCardID, PartID, TriggerDate, RequestedQty, Status FROM reorder_card ORDER BY TriggerDate DESC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvRequests.DataSource = dt;
                    }

                    // Dynamically map Database Columns to English UI Headers
                    if (dgvRequests.Columns.Contains("ReOrderCardID")) dgvRequests.Columns["ReOrderCardID"].HeaderText = "Reorder ID";
                    if (dgvRequests.Columns.Contains("PartID")) dgvRequests.Columns["PartID"].HeaderText = "Target Part ID";
                    if (dgvRequests.Columns.Contains("TriggerDate")) { dgvRequests.Columns["TriggerDate"].HeaderText = "Requested On"; dgvRequests.Columns["TriggerDate"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm"; }
                    if (dgvRequests.Columns.Contains("RequestedQty")) dgvRequests.Columns["RequestedQty"].HeaderText = "Qty";
                    if (dgvRequests.Columns.Contains("Status")) dgvRequests.Columns["Status"].HeaderText = "Current Status";
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Load Error: " + ex.Message); }
            }
        }

        private void btnSubmitRequest_Click(object sender, EventArgs e)
        {
            // Input Validation Shield
            if (string.IsNullOrWhiteSpace(txtPartID.Text) || !int.TryParse(txtQty.Text.Trim(), out int qty) || qty <= 0)
            {
                MessageBox.Show("Please enter a valid Part ID and ensure the Request Quantity is a number greater than 0!", "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // Status automatically starts as 'Pending' for Procurement Department to review
                    string sql = "INSERT INTO reorder_card (ReOrderCardID, PartID, RequestedQty, Status) VALUES (@RCID, @PartID, @Qty, 'Pending')";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@RCID", txtCardID.Text.Trim());
                        cmd.Parameters.AddWithValue("@PartID", txtPartID.Text.Trim());
                        cmd.Parameters.AddWithValue("@Qty", qty);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Material replenishment request successfully dispatched to the Procurement Division!", "Request Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                    LoadRequests();
                }
                catch (Exception ex)
                {
                    // Usually occurs due to Foreign Key constraint if the PartID doesn't exist in the database
                    MessageBox.Show("Submission failed. Please verify if the provided Part ID strictly exists in the master catalog.\n\nSystem Notice: " + ex.Message, "Database Constraint Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvRequests_SelectionChanged(object sender, EventArgs e)
        {
            // Allows user to click on a past request to view its details in the left card (Read-only observation)
            if (dgvRequests.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvRequests.SelectedRows[0];
                txtCardID.Text = row.Cells["ReOrderCardID"].Value?.ToString() ?? "";
                txtPartID.Text = row.Cells["PartID"].Value?.ToString() ?? "";
                txtQty.Text = row.Cells["RequestedQty"].Value?.ToString() ?? "";

                // Disable submit button when viewing past records to prevent duplicate ID crashes
                btnSubmit.Enabled = false;
                btnSubmit.BackColor = Color.LightGray;
            }
        }

        private void ClearFields()
        {
            // Resets the interface for a brand new request
            txtPartID.Clear();
            txtQty.Clear();
            dgvRequests.ClearSelection();
            GenerateNewCardID();

            // Re-enable submit capabilities
            btnSubmit.Enabled = true;
            btnSubmit.BackColor = Color.FromArgb(16, 185, 129); // Restore Emerald Green
        }
        #endregion
    }
}