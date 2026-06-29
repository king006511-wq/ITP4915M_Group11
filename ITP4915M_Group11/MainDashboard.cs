using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class MainDashboard : Form
    {
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // ==========================================
        // 🔒 全域容器架構變數
        // ==========================================
        private Form activeForm = null;
        private Panel pnlContent;
        private Panel pnlLeftNav;
        private FlowLayoutPanel flpNavMenu;

        public MainDashboard()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                InitializePremiumContainerUI();
            }
        }

        #region 🎨 真正的現代化視窗佈局 (動態適應 + 安全尺寸防護)
        private void InitializePremiumContainerUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Enterprise ERP Dashboard";

            this.Size = new Size(1300, 850);
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // 左側導航欄
            pnlLeftNav = new Panel
            {
                Dock = DockStyle.Left,
                Width = 260,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(0, 20, 0, 0)
            };

            // 企業商標 Logo
            Label lblLogo = new Label
            {
                Text = "PREMIUM\nLIVING",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Black", 18F, FontStyle.Bold),
                AutoSize = false,
                Width = 260,
                Height = 80,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top
            };
            pnlLeftNav.Controls.Add(lblLogo);

            // 導航按鈕流式佈局容器
            flpNavMenu = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(10, 20, 10, 10)
            };
            pnlLeftNav.Controls.Add(flpNavMenu);

            // 右側主內容容器
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(243, 244, 246),
                AutoScroll = true
            };

            // 監聽容器縮放事件，確保內嵌子表單動態 RWD 適應尺寸
            pnlContent.Resize += PnlContent_Resize;

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlLeftNav);

            SetupNavigationMenu();
            ShowHomeDashboard(); // 預設顯示主戰情室
        }

        private void SetupNavigationMenu()
        {
            AddNavHeader("📊 System Dashboards");
            AddNavButton("🏠 Home Dashboard", null, "HOME", "HOME");

            // 🌟 權限控管組合：判斷是否顯示核心業務模組
            if (AuthorizationHelper.HasMenuPermission("CUSTOMER_MGMT") ||
                AuthorizationHelper.HasMenuPermission("SALES_QUOTATION") ||
                AuthorizationHelper.HasMenuPermission("SALES_ORDER") ||
                AuthorizationHelper.HasMenuPermission("DELIVERY_LOGISTICS") ||
                AuthorizationHelper.HasMenuPermission("GOODS_RECEIVED") ||
                AuthorizationHelper.HasMenuPermission("PRODUCT_MAINTENANCE") ||
                AuthorizationHelper.HasMenuPermission("PRODUCT_CREATION_BOM") ||
                AuthorizationHelper.HasMenuPermission("PRODUCT_MANUFACTURING"))
            {
                AddNavHeader("💼 Core Modules");
                AddNavButton("👥 Customer Mgmt", typeof(CustomerManagement), "FORM", "CUSTOMER_MGMT");
                AddNavButton("📄 Sales Quotation", typeof(SalesQuotationForm), "FORM", "SALES_QUOTATION");
                AddNavButton("🛒 Sales Order Mgmt", typeof(OrderManagementForm), "FORM", "SALES_ORDER");
                AddNavButton("🚚 Delivery Logistics", typeof(LogisticsForm), "FORM", "DELIVERY_LOGISTICS");
                AddNavButton("📦 Goods Received (GRN)", typeof(GoodsReceivedForm), "FORM", "GOODS_RECEIVED");
                AddNavButton("🛋️ Product Maintenance", typeof(ProductManagement), "FORM", "PRODUCT_MAINTENANCE");

                // 新產品研發與 BOM 模組
                AddNavButton("✨ New Product R&D", typeof(ProductCreationBOMForm), "FORM", "PRODUCT_CREATION_BOM");
                AddNavButton("🛠️ Product Manufacturing", typeof(ProductManufacturingForm), "FORM", "PRODUCT_MANUFACTURING");
            }

            // 🌟 修正：加入 SUPPLIER_MATERIAL_CREATION 到內部營運權限檢查組入面
            if (AuthorizationHelper.HasMenuPermission("MATERIAL_REQUESTS") ||
                AuthorizationHelper.HasMenuPermission("PROCUREMENT_CONTROL") ||
                AuthorizationHelper.HasMenuPermission("SUPPLIER_MATERIAL_CREATION") || // 👈 加咗呢行判斷
                AuthorizationHelper.HasMenuPermission("HR_STAFF_MGMT") ||
                AuthorizationHelper.HasMenuPermission("CUSTOMER_SUPPORT"))
            {
                AddNavHeader("⚙️ Internal Ops");
                AddNavButton("🏭 Material Requests", typeof(RawMaterialRequestForm), "FORM", "MATERIAL_REQUESTS");
                AddNavButton("📈 Procurement Control", typeof(ProcurementForm), "FORM", "PROCUREMENT_CONTROL");

                // 🌟 新增：真正將「供應商與物料建立」按鈕放落選單到！
                AddNavButton("🤝 Supplier & Material", typeof(SupplierAndMaterialCreationForm), "FORM", "SUPPLIER_MATERIAL_CREATION");

                AddNavButton("👔 HR / Staff Mgmt", typeof(EmployeeManagement), "FORM", "HR_STAFF_MGMT");
                AddNavButton("📞 Customer Support", typeof(AfterServiceForm), "FORM", "CUSTOMER_SUPPORT");
            }

            AddNavHeader("");
            AddNavButton("🚪 Logout System", null, "LOGOUT", "LOGOUT");
        }

        private void AddNavHeader(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                flpNavMenu.Controls.Add(new Panel { Size = new Size(220, 20), BackColor = Color.Transparent });
                return;
            }
            Label lblHeader = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Size = new Size(220, 25),
                Margin = new Padding(5, 15, 5, 5),
                TextAlign = ContentAlignment.BottomLeft
            };
            flpNavMenu.Controls.Add(lblHeader);
        }

        private void AddNavButton(string text, Type formType, string actionType, string menuId = "")
        {
            if (actionType == "FORM" && !AuthorizationHelper.HasMenuPermission(menuId)) return;

            Button btn = new Button
            {
                Text = "  " + text,
                Width = 220,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                ForeColor = actionType == "LOGOUT" ? Color.FromArgb(239, 68, 68) : Color.FromArgb(226, 232, 240),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand,
                Margin = new Padding(5, 2, 5, 2),
                Tag = menuId
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);

            btn.Click += (s, e) =>
            {
                if (actionType == "HOME") ShowHomeDashboard();
                else if (actionType == "LOGOUT") this.Close();
                else LoadModule(formType);
            };
            flpNavMenu.Controls.Add(btn);
        }
        #endregion

        #region 🏠 Enterprise Home Dashboard (Analytics & Self-Service)
        private void ShowHomeDashboard()
        {
            if (activeForm != null) { activeForm.Close(); activeForm = null; }
            pnlContent.Controls.Clear();

            string staffID = UserSession.LoggedInStaffID ?? "Guest";
            string role = UserSession.LoggedInStaffRole ?? "Unknown Role";

            // 1. Header 歡迎區區塊
            Label lblTitle = new Label { Text = $"Welcome back, {staffID} ✨", Font = new Font("Segoe UI Black", 22F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(40, 30), AutoSize = true };
            Label lblSubTitle = new Label { Text = $"Role: {role} | Premium Living Enterprise Control Center", Font = new Font("Segoe UI", 11F), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(43, 75), AutoSize = true };
            pnlContent.Controls.Add(lblTitle); pnlContent.Controls.Add(lblSubTitle);

            // 自主變更密碼按鈕
            Button btnChangePwd = new Button { Text = "🔑 Change My Password", Location = new Point(pnlContent.Width - 300, 40), Size = new Size(220, 42), BackColor = Color.FromArgb(79, 70, 229), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnChangePwd.FlatAppearance.BorderSize = 0;
            btnChangePwd.Click += BtnChangeMyPwd_Click;
            pnlContent.Controls.Add(btnChangePwd);

            string todayOrders = "0", pendingApprove = "0", pendingDispatch = "0", lowStock = "0";
            DataTable dtRecentOrders = new DataTable();
            DataTable dtLowStock = new DataTable();

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM orders WHERE DATE(OrderDate) = CURDATE()", conn)) todayOrders = cmd.ExecuteScalar()?.ToString() ?? "0";
                    using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM orders WHERE Status LIKE 'Awaiting Approval%'", conn)) pendingApprove = cmd.ExecuteScalar()?.ToString() ?? "0";
                    using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM orders WHERE Status = 'Ready for Dispatch'", conn)) pendingDispatch = cmd.ExecuteScalar()?.ToString() ?? "0";
                    using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM product WHERE StockLevel <= 10", conn)) lowStock = cmd.ExecuteScalar()?.ToString() ?? "0";

                    using (MySqlDataAdapter da = new MySqlDataAdapter("SELECT OrderID AS 'Order Ref', CustomerID AS 'Customer', TotalAmount AS 'Amount', Status FROM orders ORDER BY OrderDate DESC LIMIT 8", conn)) da.Fill(dtRecentOrders);
                    using (MySqlDataAdapter da = new MySqlDataAdapter("SELECT ProductID AS 'Product ID', ProductName AS 'Item Name', StockLevel AS 'Stock Left' FROM product WHERE StockLevel <= 10 ORDER BY StockLevel ASC LIMIT 8", conn)) da.Fill(dtLowStock);
                }
                catch (Exception) { }
            }

            // 2. 數據統計卡片區塊
            FlowLayoutPanel flpStats = new FlowLayoutPanel { Location = new Point(40, 120), Size = new Size(pnlContent.Width - 80, 130), FlowDirection = FlowDirection.LeftToRight, WrapContents = true, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            pnlContent.Controls.Add(flpStats);

            flpStats.Controls.Add(CreateStatCard("TODAY'S ORDERS", todayOrders, "Active", Color.FromArgb(14, 165, 233)));
            flpStats.Controls.Add(CreateStatCard("PENDING APPROVAL", pendingApprove, "Requires Action", Color.FromArgb(245, 158, 11)));
            flpStats.Controls.Add(CreateStatCard("READY FOR DISPATCH", pendingDispatch, "Logistics Queue", Color.FromArgb(139, 92, 246)));
            flpStats.Controls.Add(CreateStatCard("LOW STOCK ALERTS", lowStock, "Procurement Needed", Color.FromArgb(239, 68, 68)));

            // 3. 戰情分析網格
            TableLayoutPanel tlpGrids = new TableLayoutPanel { Location = new Point(40, 270), Size = new Size(pnlContent.Width - 80, 450), ColumnCount = 2, RowCount = 1, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            tlpGrids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tlpGrids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            pnlContent.Controls.Add(tlpGrids);

            tlpGrids.Controls.Add(CreateGridPanel("📈 Recent Sales Activity", dtRecentOrders, Color.FromArgb(37, 99, 235)), 0, 0);
            tlpGrids.Controls.Add(CreateGridPanel("⚠️ Low Stock Warning", dtLowStock, Color.FromArgb(220, 38, 38)), 1, 0);
        }

        private Panel CreateStatCard(string title, string value, string subtitle, Color themeColor)
        {
            Panel card = new Panel { Size = new Size(250, 110), BackColor = Color.White, Margin = new Padding(0, 0, 20, 20) };
            card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            Panel topBar = new Panel { Dock = DockStyle.Top, Height = 4, BackColor = themeColor };
            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(15, 15), AutoSize = true };
            Label lblValue = new Label { Text = value, Font = new Font("Segoe UI Black", 26F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(12, 32), AutoSize = true };
            Label lblSub = new Label { Text = subtitle, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = themeColor, Location = new Point(15, 82), AutoSize = true };

            card.Controls.Add(topBar); card.Controls.Add(lblTitle); card.Controls.Add(lblValue); card.Controls.Add(lblSub);
            return card;
        }

        private Panel CreateGridPanel(string title, DataTable data, Color headerColor)
        {
            Panel pnl = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, 20, 0) };
            pnl.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnl.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(15, 15), AutoSize = true };
            pnl.Controls.Add(lblTitle);

            DataGridView dgv = new DataGridView
            {
                Location = new Point(15, 55),
                Size = new Size(pnl.Width - 30, pnl.Height - 70),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                DataSource = data,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(241, 245, 249)
            };
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = headerColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 35;
            pnl.Controls.Add(dgv);

            return pnl;
        }

        private void BtnChangeMyPwd_Click(object sender, EventArgs e)
        {
            string loggedInID = UserSession.LoggedInStaffID ?? "S001";

            Form pwdForm = new Form
            {
                Text = "Account Security",
                Size = new Size(400, 380),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            Label lblTitle = new Label { Text = $"Update Password for {loggedInID}", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(20, 20), AutoSize = true };
            pwdForm.Controls.Add(lblTitle);

            Label lblOld = new Label { Text = "Current Password:", Location = new Point(20, 70), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
            TextBox txtOld = new TextBox { Location = new Point(20, 95), Width = 340, PasswordChar = '●', Font = new Font("Segoe UI", 10.5F) };
            pwdForm.Controls.Add(lblOld); pwdForm.Controls.Add(txtOld);

            Label lblNew = new Label { Text = "New Password:", Location = new Point(20, 140), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
            TextBox txtNew = new TextBox { Location = new Point(20, 165), Width = 340, PasswordChar = '●', Font = new Font("Segoe UI", 10.5F) };
            pwdForm.Controls.Add(lblNew); pwdForm.Controls.Add(txtNew);

            Label lblConfirm = new Label { Text = "Confirm New Password:", Location = new Point(20, 210), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
            TextBox txtConfirm = new TextBox { Location = new Point(20, 235), Width = 340, PasswordChar = '●', Font = new Font("Segoe UI", 10.5F) };
            pwdForm.Controls.Add(lblConfirm); pwdForm.Controls.Add(txtConfirm);

            Button btnSubmit = new Button { Text = "Update Password", Location = new Point(20, 285), Size = new Size(340, 40), BackColor = Color.FromArgb(79, 70, 229), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSubmit.FlatAppearance.BorderSize = 0;
            pwdForm.Controls.Add(btnSubmit);

            btnSubmit.Click += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(txtOld.Text) || string.IsNullOrWhiteSpace(txtNew.Text) || string.IsNullOrWhiteSpace(txtConfirm.Text))
                {
                    MessageBox.Show("Please fill in all password fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
                }
                if (txtNew.Text != txtConfirm.Text)
                {
                    MessageBox.Show("New passwords do not match!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
                }

                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlCommand checkCmd = new MySqlCommand("SELECT COUNT(*) FROM staff WHERE StaffID = @id AND Password = SHA2(@oldPwd, 256)", conn))
                        {
                            checkCmd.Parameters.AddWithValue("@id", loggedInID);
                            checkCmd.Parameters.AddWithValue("@oldPwd", txtOld.Text);
                            if (Convert.ToInt32(checkCmd.ExecuteScalar()) == 0)
                            {
                                MessageBox.Show("Incorrect Current Password. Access Denied.", "Security Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                        using (MySqlCommand updateCmd = new MySqlCommand("UPDATE staff SET Password = SHA2(@newPwd, 256) WHERE StaffID = @id", conn))
                        {
                            updateCmd.Parameters.AddWithValue("@id", loggedInID);
                            updateCmd.Parameters.AddWithValue("@newPwd", txtNew.Text);
                            updateCmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Your password has been changed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        pwdForm.Close();
                    }
                    catch (Exception ex) { MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            };
            pwdForm.ShowDialog();
        }
        #endregion

        #region 🚀 模組動態加載引擎與全域佈局修復
        private void LoadModule(Type formType)
        {
            try
            {
                if (formType == null) return;
                if (activeForm != null && activeForm.GetType() == formType) return;
                if (activeForm != null) { activeForm.Close(); activeForm.Dispose(); }

                Form targetForm = (Form)Activator.CreateInstance(formType);

                if (targetForm is ProductCreationBOMForm bomForm)
                {
                    bomForm.OnNavigationBack = () =>
                    {
                        LoadModule(typeof(ProductManagement));
                    };
                }

                targetForm.TopLevel = false;
                targetForm.FormBorderStyle = FormBorderStyle.None;
                targetForm.Dock = DockStyle.None;
                targetForm.Location = new Point(0, 0);
                targetForm.BackColor = Color.White;

                targetForm.Size = new Size(
                    Math.Max(pnlContent.Width, 1150),
                    Math.Max(pnlContent.Height, 800)
                );

                pnlContent.Controls.Clear();
                pnlContent.Controls.Add(targetForm);

                GlobalOptimizeChildForm(targetForm);

                targetForm.Show();
                activeForm = targetForm;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load module: {formType?.Name}\n{ex.Message}", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PnlContent_Resize(object sender, EventArgs e)
        {
            if (activeForm != null)
            {
                activeForm.Size = new Size(
                    Math.Max(pnlContent.Width, 1150),
                    Math.Max(pnlContent.Height, 800)
                );
            }
        }

        private void GlobalOptimizeChildForm(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is DataGridView dgv)
                {
                    dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                    dgv.BackgroundColor = Color.White;
                    dgv.BorderStyle = BorderStyle.None;
                }
                if (ctrl is Button btn && (btn.Text.Contains("Go Back") || btn.Text.Contains("Back Home") || btn.Text.Contains("⬅")))
                {
                    btn.Visible = false;
                }
                if (ctrl.HasChildren)
                {
                    GlobalOptimizeChildForm(ctrl);
                }
            }
        }
        #endregion

        private void MainDashboard_Load(object sender, EventArgs e)
        {
        }
    }
}