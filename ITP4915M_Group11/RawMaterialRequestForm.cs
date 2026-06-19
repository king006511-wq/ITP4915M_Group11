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
        private readonly string connString = UserSession.ConnString;

        // ==========================================
        // 🎨 Modern UI Element Variables
        // ==========================================
        private TextBox txtCardID, txtPartID, txtQty;
        private DataGridView dgvRequests;
        private Button btnSubmit, btnClear;

        public RawMaterialRequestForm()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                InitializePremiumModernUI();
                GenerateNewCardID(); // 自動生成 RC011 呢類 ID
                LoadRequests();
            }
        }

        // Designer 會註冊到 Load 事件，提供一個實作以避免 Designer 引用錯誤
        private void RawMaterialRequestForm_Load(object sender, EventArgs e)
        {
            // 權限保護：僅 Manager / Administrator / ProcurementOfficer 可使用物料請求功能
            if (!AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.ProcurementOfficer))
            {
                MessageBox.Show("您沒有權限提交物料補貨申請。若需要操作，請洽管理員。", "存取被拒", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.BeginInvoke(new MethodInvoker(this.Close));
                return;
            }

            // 確保在 Load 階段已產生 ID 與載入資料（若尚未執行）
            try { GenerateNewCardID(); } catch { }
            try { LoadRequests(); } catch { }
        }

        private void RawMaterialRequestForm_Shown(object sender, EventArgs e)
        {
            // 請求物料一般不應由所有角色提交，限制給 Manager / ProcurementOfficer
            if (!AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.ProcurementOfficer))
            {
                MessageBox.Show("您沒有權限提交物料補貨申請。若需要操作，請洽管理員。", "存取被拒", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.BeginInvoke(new MethodInvoker(this.Close));
            }
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
                "🏠 Back Home",
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
                Button btnMenu = new Button { Text = "  " + item, Top = btnTop, Left = 12, Size = new Size(236, 48), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand };
                btnMenu.FlatAppearance.BorderSize = 0;

                if (item.Contains("Material Requests"))
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
                        if (item.Contains("Back Home")) targetForm = new MainDashboard();
                        else if (item.Contains("Sales Order")) targetForm = new OrderManagementForm();
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
                pnlSidebar.Controls.Add(btnMenu);
                btnTop += 55;
            }
            this.Controls.Add(pnlSidebar);

            // 2. Right Main Workspace
            Panel pnlMain = new Panel { Location = new Point(260, 0), Size = new Size(900, 750) };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label { Text = "Factory Material Reorder Center", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(30, 20), AutoSize = true };
            pnlMain.Controls.Add(lblHeader);

            // Go Back button (returns to previous view)
            Button btnBackHome = new Button { Text = "🔙 Go Back", Size = new Size(120, 34), Location = new Point(740, 22), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand, Visible = true };
            btnBackHome.FlatAppearance.BorderSize = 0;
            btnBackHome.Click += (s, e) => { try { this.Close(); } catch { this.Hide(); } };
            pnlMain.Controls.Add(btnBackHome);

            // 3. Input Form Card
            Panel pnlCard = new Panel { Location = new Point(30, 85), Size = new Size(380, 600), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlCard);

            Label lblCardTitle = new Label { Text = "📝 Create Replenishment Request", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(20, 15), AutoSize = true };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 60;
            txtCardID = CreateStyledTextBox(pnlCard, ref startY, "Reorder Card ID (Auto-Generated):", true);
            txtPartID = CreateStyledTextBox(pnlCard, ref startY, "Part / Material ID (e.g., P001) *:", false);
            txtQty = CreateStyledTextBox(pnlCard, ref startY, "Requested Quantity (Numeric) *:", false);

            // Buttons Array
            btnSubmit = new Button { Text = "📤 Submit Request", Location = new Point(20, startY + 20), Size = new Size(160, 42), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += btnSubmitRequest_Click;
            pnlCard.Controls.Add(btnSubmit);

            btnClear = new Button { Text = "✨ Add New Data", Location = new Point(195, startY + 20), Size = new Size(160, 42), BackColor = Color.FromArgb(71, 85, 105), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
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

        // ✨ 修復問題 1: 自動順序生成 RC011、RC012...
        private void GenerateNewCardID()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // 去 Database 搵返現存最大嗰個 RC ID (排除帶有 '-' 符號嘅舊錯 record)
                    string query = "SELECT ReOrderCardID FROM reorder_card WHERE ReOrderCardID NOT LIKE '%-%' ORDER BY ReOrderCardID DESC LIMIT 1";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != null && result.ToString().StartsWith("RC"))
                        {
                            string lastId = result.ToString(); // 抽到例如 "RC010"
                            int number = int.Parse(lastId.Substring(2)); // 將 "010" 變做數字 10
                            txtCardID.Text = "RC" + (number + 1).ToString("D3"); // 加 1 變成 11，再補零變成 "RC011"
                        }
                        else
                        {
                            txtCardID.Text = "RC001"; // 如果 Table 係空，就由 RC001 開始
                        }
                    }
                }
                catch (Exception)
                {
                    txtCardID.Text = "RC001"; // Error fallback
                }
            }
        }

        private void LoadRequests()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT ReOrderCardID, PartID, TriggerDate, RequestedQty, Status FROM reorder_card ORDER BY TriggerDate DESC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvRequests.DataSource = dt;
                    }

                    if (dgvRequests.Columns.Contains("ReOrderCardID")) dgvRequests.Columns["ReOrderCardID"].HeaderText = "Reorder ID";
                    if (dgvRequests.Columns.Contains("PartID")) dgvRequests.Columns["PartID"].HeaderText = "Target Part ID";
                    if (dgvRequests.Columns.Contains("TriggerDate")) { dgvRequests.Columns["TriggerDate"].HeaderText = "Requested On"; dgvRequests.Columns["TriggerDate"].DefaultCellStyle.Format = "yyyy-MM-dd"; }
                    if (dgvRequests.Columns.Contains("RequestedQty")) dgvRequests.Columns["RequestedQty"].HeaderText = "Qty";
                    if (dgvRequests.Columns.Contains("Status")) dgvRequests.Columns["Status"].HeaderText = "Current Status";
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Load Error: " + ex.Message); }
            }
        }

        private void btnSubmitRequest_Click(object sender, EventArgs e)
        {
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
                    // ✨ 修復問題 2: 喺 SQL 加入咗 TriggerDate，並將佢寫入 DataBase
                    string sql = "INSERT INTO reorder_card (ReOrderCardID, PartID, TriggerDate, RequestedQty, Status) VALUES (@RCID, @PartID, @Date, @Qty, 'Pending')";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@RCID", txtCardID.Text.Trim());
                        cmd.Parameters.AddWithValue("@PartID", txtPartID.Text.Trim());
                        cmd.Parameters.AddWithValue("@Date", DateTime.Now); // 將當時嘅系統時間塞入去
                        cmd.Parameters.AddWithValue("@Qty", qty);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Material replenishment request successfully dispatched!", "Request Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                    LoadRequests();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Submission failed. Please verify if the provided Part ID strictly exists.\n\nError: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvRequests_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvRequests.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvRequests.SelectedRows[0];
                txtCardID.Text = row.Cells["ReOrderCardID"].Value?.ToString() ?? "";
                txtPartID.Text = row.Cells["PartID"].Value?.ToString() ?? "";
                txtQty.Text = row.Cells["RequestedQty"].Value?.ToString() ?? "";

                btnSubmit.Enabled = false;
                btnSubmit.BackColor = Color.LightGray;
            }
        }

        private void ClearFields()
        {
            txtPartID.Clear();
            txtQty.Clear();
            dgvRequests.ClearSelection();
            GenerateNewCardID(); // 每次 Clear 完再重新計過下一個 ID 係咩

            btnSubmit.Enabled = true;
            btnSubmit.BackColor = Color.FromArgb(16, 185, 129);
        }
        #endregion
    }
}