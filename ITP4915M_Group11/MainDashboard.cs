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
        private Form activeForm = null;
        private Panel pnlContent;
        private Panel pnlLeftNav;
        private FlowLayoutPanel flpNavMenu;
        private Timer clockTimer;

        public MainDashboard()
        {
            InitializePremiumContainerUI();
        }

        private void InitializePremiumContainerUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Enterprise ERP Dashboard";
            this.Size = new Size(1300, 850);
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.FormBorderStyle = FormBorderStyle.Sizable;

            pnlLeftNav = new Panel { Dock = DockStyle.Left, Width = 260, BackColor = Color.FromArgb(15, 23, 42), Padding = new Padding(0, 20, 0, 0) };
            Label lblLogo = new Label { Text = "PREMIUM\nLIVING", ForeColor = Color.White, Font = new Font("Segoe UI Black", 18F, FontStyle.Bold), AutoSize = false, Width = 260, Height = 80, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Top };
            pnlLeftNav.Controls.Add(lblLogo);

            flpNavMenu = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(10, 20, 0, 20) };
            pnlLeftNav.Controls.Add(flpNavMenu);

            pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(243, 244, 246), AutoScroll = true };
            pnlContent.Resize += PnlContent_Resize;
            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlLeftNav);
            pnlContent.BringToFront();

            SetupNavigationMenu();
            ShowHomeDashboard();
        }

        private void SetupNavigationMenu()
        {
            flpNavMenu.Controls.Clear();
            AddNavHeader("📊 System Dashboards");
            AddNavButton("🏠 Home Dashboard", null, "HOME", "HOME");

            if (AuthorizationHelper.HasMenuPermission("STAT_REPORT"))
            {
                AddNavButton("📈 Statistical Report", typeof(StatisticalReportForm), "FORM", "STAT_REPORT");
            }

            if (AuthorizationHelper.HasMenuPermission("CUSTOMER_MGMT") || AuthorizationHelper.HasMenuPermission("SALES_QUOTATION") || AuthorizationHelper.HasMenuPermission("SALES_ORDER") || AuthorizationHelper.HasMenuPermission("DELIVERY_LOGISTICS") || AuthorizationHelper.HasMenuPermission("GOODS_RECEIVED") || AuthorizationHelper.HasMenuPermission("PRODUCT_MAINTENANCE") || AuthorizationHelper.HasMenuPermission("PRODUCT_CREATION_BOM") || AuthorizationHelper.HasMenuPermission("PRODUCT_MANUFACTURING"))
            {
                AddNavHeader("💼 Core Modules");
                AddNavButton("👥 Customer Mgmt", typeof(CustomerManagement), "FORM", "CUSTOMER_MGMT");
                AddNavButton("📄 Sales Quotation", typeof(SalesQuotationForm), "FORM", "SALES_QUOTATION");
                AddNavButton("🛒 Sales Order Mgmt", typeof(OrderManagementForm), "FORM", "SALES_ORDER");
                AddNavButton("🚚 Delivery Logistics", typeof(LogisticsForm), "FORM", "DELIVERY_LOGISTICS");
                AddNavButton("📦 Goods Received (GRN)", typeof(GoodsReceivedForm), "FORM", "GOODS_RECEIVED");
                AddNavButton("🛋️ Product Maintenance", typeof(ProductManagement), "FORM", "PRODUCT_MAINTENANCE");
                AddNavButton("✨ New Product R&D", typeof(ProductCreationBOMForm), "FORM", "PRODUCT_CREATION_BOM");
                AddNavButton("🛠️ Product Manufacturing", typeof(ProductManufacturingForm), "FORM", "PRODUCT_MANUFACTURING");
            }

            if (AuthorizationHelper.HasMenuPermission("MATERIAL_REQUESTS") || AuthorizationHelper.HasMenuPermission("PROCUREMENT_CONTROL") || AuthorizationHelper.HasMenuPermission("SUPPLIER_MATERIAL_CREATION") || AuthorizationHelper.HasMenuPermission("RAW_MATERIAL_MGMT") || AuthorizationHelper.HasMenuPermission("HR_STAFF_MGMT") || AuthorizationHelper.HasMenuPermission("CUSTOMER_SUPPORT"))
            {
                AddNavHeader("⚙️ Internal Ops");
                AddNavButton("🏭 Material Requests", typeof(RawMaterialRequestForm), "FORM", "MATERIAL_REQUESTS");
                AddNavButton("📈 Procurement Control", typeof(ProcurementForm), "FORM", "PROCUREMENT_CONTROL");
                AddNavButton("🤝 Supplier & Material", typeof(SupplierAndMaterialCreationForm), "FORM", "SUPPLIER_MATERIAL_CREATION");

                AddNavButton("🪵 Material Manager", typeof(RawMaterialManagementForm), "FORM", "RAW_MATERIAL_MGMT");

                AddNavButton("👔 HR / Staff Mgmt", typeof(EmployeeManagement), "FORM", "HR_STAFF_MGMT");
                AddNavButton("📞 Customer Support", typeof(AfterServiceForm), "FORM", "CUSTOMER_SUPPORT");
                AddNavButton("📜 Sales Activity Log", typeof(SalesActivityLogForm), "FORM", "CUSTOMER_SUPPORT");
            }

            AddNavHeader("");
            AddNavButton("🚪 Logout System", null, "LOGOUT", "LOGOUT");
        }

        private void AddNavHeader(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                flpNavMenu.Controls.Add(new Panel { Size = new Size(230, 20), BackColor = Color.Transparent });
                return;
            }
            Label lblHeader = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = ThemeManager.MutedText,
                Size = new Size(230, 25),
                Margin = new Padding(10, 15, 0, 5),
                TextAlign = ContentAlignment.BottomLeft
            };
            flpNavMenu.Controls.Add(lblHeader);
        }

        private void AddNavButton(string text, Type formType, string actionType, string menuId = "")
        {
            if (actionType == "FORM" && !AuthorizationHelper.HasMenuPermission(menuId)) return;

            Button btn = new Button
            {
                Text = " " + text,
                Width = 230,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                ForeColor = actionType == "LOGOUT" ? ThemeManager.Danger : Color.FromArgb(226, 232, 240),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 2, 0, 2),
                Padding = new Padding(10, 0, 0, 0),
                Tag = menuId
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(37, 43, 66);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 36, 55);
            btn.Click += (s, e) =>
            {
                if (actionType == "HOME") ShowHomeDashboard();
                else if (actionType == "LOGOUT") { this.Hide(); new Login().Show(); }
                else LoadModule(formType);
            };
            flpNavMenu.Controls.Add(btn);
        }

        private void ShowHomeDashboard()
        {
            if (activeForm != null) { activeForm.Close(); activeForm = null; }
            if (clockTimer != null) { clockTimer.Stop(); clockTimer.Dispose(); }
            pnlContent.Controls.Clear();
            string staffID = UserSession.LoggedInStaffID ?? "Guest";
            string role = UserSession.LoggedInStaffRole ?? "Unknown Role";
            Label lblTitle = new Label { Text = $"Welcome back, {staffID} ✨", Font = new Font("Segoe UI Black", 22F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(40, 30), AutoSize = true };
            Label lblSubTitle = new Label { Text = $"Role: {role} | Premium Living ERP Workspace", Font = new Font("Segoe UI", 11F), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(43, 75), AutoSize = true };
            pnlContent.Controls.Add(lblTitle);
            pnlContent.Controls.Add(lblSubTitle);
            Button btnChangePwd = new Button { Text = "🔑 Account Security", Location = new Point(pnlContent.Width - 260, 40), Size = new Size(180, 42), BackColor = Color.FromArgb(79, 70, 229), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnChangePwd.FlatAppearance.BorderSize = 0;
            btnChangePwd.Click += BtnChangeMyPwd_Click;
            pnlContent.Controls.Add(btnChangePwd);
            var currentEnum = AuthorizationHelper.ParseRole(role);
            bool isManagement = (currentEnum == AuthorizationHelper.UserRoleEnum.Manager || currentEnum == AuthorizationHelper.UserRoleEnum.Administrator || currentEnum == AuthorizationHelper.UserRoleEnum.SystemManager);
            if (isManagement) { RenderManagerDashboardMetrics(); } else { RenderGeneralStaffDashboard(); }
        }

        private void RenderManagerDashboardMetrics()
        {
            int todayOrders = 0, pendingApprove = 0, processingOrders = 0, pendingDispatch = 0, lowStockCount = 0;
            DataTable dtCombinedLowStock = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM orders WHERE DATE(OrderDate)=CURDATE()", conn)) todayOrders = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                    using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM orders WHERE Status LIKE 'Awaiting Approval%'", conn)) pendingApprove = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                    using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM orders WHERE Status LIKE 'Processing%'", conn)) processingOrders = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                    using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM orders WHERE Status='Ready for Dispatch'", conn)) pendingDispatch = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                    int pLow = Convert.ToInt32(new MySqlCommand("SELECT COUNT(*) FROM product WHERE StockLevel<=10", conn).ExecuteScalar() ?? 0);
                    int mLow = Convert.ToInt32(new MySqlCommand("SELECT COUNT(*) FROM raw_material WHERE StockLevel<=ReorderLevel", conn).ExecuteScalar() ?? 0);
                    lowStockCount = pLow + mLow;
                    string unionSql = @"SELECT ProductID AS 'Item ID',ProductName AS 'Item Name',StockLevel AS 'Qty Left','Product' AS 'Category' FROM product WHERE StockLevel<=10 UNION ALL SELECT MaterialID AS 'Item ID',MaterialName AS 'Item Name',StockLevel AS 'Qty Left','Raw Material' AS 'Category' FROM raw_material WHERE StockLevel<=ReorderLevel ORDER BY `Qty Left` ASC LIMIT 8";
                    using (MySqlDataAdapter da = new MySqlDataAdapter(unionSql, conn)) { da.Fill(dtCombinedLowStock); }
                }
                catch { }
            }
            FlowLayoutPanel flpStats = new FlowLayoutPanel { Location = new Point(40, 120), Size = new Size(pnlContent.Width - 80, 130), FlowDirection = FlowDirection.LeftToRight, WrapContents = true, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            pnlContent.Controls.Add(flpStats);
            flpStats.Controls.Add(CreateStatCard("TODAY'S ORDERS", todayOrders.ToString(), "Active", Color.FromArgb(14, 165, 233)));
            flpStats.Controls.Add(CreateStatCard("PENDING APPROVAL", pendingApprove.ToString(), "Requires Action", Color.FromArgb(245, 158, 11)));
            flpStats.Controls.Add(CreateStatCard("READY FOR DISPATCH", pendingDispatch.ToString(), "Logistics Queue", Color.FromArgb(139, 92, 246)));
            flpStats.Controls.Add(CreateStatCard("SYSTEM LOW STOCK", lowStockCount.ToString(), "Procurement Needed", Color.FromArgb(239, 68, 68)));
            TableLayoutPanel tlpGrids = new TableLayoutPanel { Location = new Point(40, 270), Size = new Size(pnlContent.Width - 80, 450), ColumnCount = 2, RowCount = 1, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            tlpGrids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            tlpGrids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            pnlContent.Controls.Add(tlpGrids);
            tlpGrids.Controls.Add(CreateOrderPulseGraphicPanel(pendingApprove, processingOrders, pendingDispatch), 0, 0);
            tlpGrids.Controls.Add(CreateGridPanel("⚠️ Global Inventory Shortage Alerts", dtCombinedLowStock, Color.FromArgb(220, 38, 38)), 1, 0);
        }

        private Panel CreateOrderPulseGraphicPanel(int pending, int processing, int dispatch)
        {
            Panel pnl = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, 20, 0) };
            pnl.Paint += (s, e) =>
            {
                Graphics g = e.Graphics; g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                ControlPaint.DrawBorder(g, pnl.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

                // 💡 已經將所有 Brushes.GetInstance() 改為 ThemeBrushes.GetInstance()
                g.DrawString("📈 Order Fulfillment Pulse", new Font("Segoe UI", 13F, FontStyle.Bold), ThemeBrushes.GetInstance().GetColorBrush(Color.FromArgb(15, 23, 42)), 20, 20);
                g.DrawLine(new Pen(Color.FromArgb(241, 245, 249), 2), 20, 60, pnl.Width - 20, 60);
                int maxVal = Math.Max(10, Math.Max(pending, Math.Max(processing, dispatch)));
                int barWidth = pnl.Width - 200; if (barWidth < 100) barWidth = 100;

                g.DrawString("Awaiting Approval", new Font("Segoe UI", 10F, FontStyle.Bold), ThemeBrushes.GetInstance().GetColorBrush(Color.FromArgb(100, 116, 139)), 30, 90);
                g.FillRectangle(new SolidBrush(Color.FromArgb(241, 245, 249)), 30, 120, barWidth, 24);
                int w1 = (int)(((double)pending / maxVal) * barWidth); g.FillRectangle(new SolidBrush(Color.FromArgb(245, 158, 11)), 30, 120, w1, 24);
                g.DrawString(pending.ToString() + " Orders", new Font("Segoe UI", 10.5F, FontStyle.Bold), ThemeBrushes.GetInstance().GetColorBrush(Color.FromArgb(15, 23, 42)), barWidth + 45, 120);

                g.DrawString("Production / Processing", new Font("Segoe UI", 10F, FontStyle.Bold), ThemeBrushes.GetInstance().GetColorBrush(Color.FromArgb(100, 116, 139)), 30, 180);
                g.FillRectangle(new SolidBrush(Color.FromArgb(241, 245, 249)), 30, 210, barWidth, 24);
                int w2 = (int)(((double)processing / maxVal) * barWidth); g.FillRectangle(new SolidBrush(Color.FromArgb(59, 130, 246)), 30, 210, w2, 24);
                g.DrawString(processing.ToString() + " Orders", new Font("Segoe UI", 10.5F, FontStyle.Bold), ThemeBrushes.GetInstance().GetColorBrush(Color.FromArgb(15, 23, 42)), barWidth + 45, 210);

                g.DrawString("Ready for Dispatch (Logistics)", new Font("Segoe UI", 10F, FontStyle.Bold), ThemeBrushes.GetInstance().GetColorBrush(Color.FromArgb(100, 116, 139)), 30, 270);
                g.FillRectangle(new SolidBrush(Color.FromArgb(241, 245, 249)), 30, 300, barWidth, 24);
                int w3 = (int)(((double)dispatch / maxVal) * barWidth); g.FillRectangle(new SolidBrush(Color.FromArgb(16, 185, 129)), 30, 300, w3, 24);
                g.DrawString(dispatch.ToString() + " Orders", new Font("Segoe UI", 10.5F, FontStyle.Bold), ThemeBrushes.GetInstance().GetColorBrush(Color.FromArgb(15, 23, 42)), barWidth + 45, 300);

                g.DrawString("💡 System automatically updates order pipeline metrics in real-time.", new Font("Segoe UI", 9F, FontStyle.Italic), ThemeBrushes.GetInstance().GetColorBrush(Color.FromArgb(148, 163, 184)), 30, 380);
            };
            return pnl;
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
            DataGridView dgv = new DataGridView { Location = new Point(15, 55), Size = new Size(pnl.Width - 30, pnl.Height - 70), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, DataSource = data, AllowUserToAddRows = false, ReadOnly = true, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, GridColor = Color.FromArgb(241, 245, 249) };
            dgv.EnableHeadersVisualStyles = false; dgv.ColumnHeadersDefaultCellStyle.BackColor = headerColor; dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White; dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold); dgv.ColumnHeadersHeight = 35;
            pnl.Controls.Add(dgv);
            return pnl;
        }

        private void RenderGeneralStaffDashboard()
        {
            Panel pnlWorkspace = new Panel { Location = new Point(40, 120), Size = new Size(pnlContent.Width - 80, 400), BackColor = Color.White, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            pnlWorkspace.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlWorkspace.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlContent.Controls.Add(pnlWorkspace);
            Label lblTime = new Label { Text = "00:00:00", Font = new Font("Segoe UI Semibold", 44F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(40, 40), AutoSize = true };
            Label lblDate = new Label { Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy"), Font = new Font("Segoe UI", 12.5F), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(48, 145), AutoSize = true };
            pnlWorkspace.Controls.Add(lblTime); pnlWorkspace.Controls.Add(lblDate);
            clockTimer = new Timer { Interval = 1000 }; clockTimer.Tick += (s, e) => lblTime.Text = DateTime.Now.ToString("HH:mm:ss"); clockTimer.Start();
            Label lblGuideTitle = new Label { Text = "✨ Premium Living Team Workspace", Font = new Font("Segoe UI", 15F, FontStyle.Bold), ForeColor = Color.FromArgb(79, 70, 229), Location = new Point(450, 50), AutoSize = true };
            Label lblGuide1 = new Label { Text = "☀️ Welcome! Wishing you a smooth, productive, and wonderful shift today!", Font = new Font("Segoe UI", 11.5F), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(450, 105), AutoSize = true };
            Label lblGuide2 = new Label { Text = "🚚 Logistics & Drivers Reminder: Safety first! Please stay alert and drive safely.", Font = new Font("Segoe UI", 11.5F), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(450, 150), AutoSize = true };
            Label lblGuide3 = new Label { Text = "🤝 Missing module access? Contact your Manager or IT Support. We are here to help!", Font = new Font("Segoe UI", 11.5F), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(450, 195), AutoSize = true };
            pnlWorkspace.Controls.Add(lblGuideTitle); pnlWorkspace.Controls.Add(lblGuide1); pnlWorkspace.Controls.Add(lblGuide2); pnlWorkspace.Controls.Add(lblGuide3);
            Panel pnlDeco = new Panel { Dock = DockStyle.Bottom, Height = 6, BackColor = Color.FromArgb(79, 70, 229) }; pnlWorkspace.Controls.Add(pnlDeco);
        }

        private void BtnChangeMyPwd_Click(object sender, EventArgs e)
        {
            string loggedInID = UserSession.LoggedInStaffID ?? "S001";
            Form pwdForm = new Form { Text = "Account Security", Size = new Size(400, 380), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, BackColor = Color.White };
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
            btnSubmit.FlatAppearance.BorderSize = 0; pwdForm.Controls.Add(btnSubmit);
            btnSubmit.Click += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(txtOld.Text) || string.IsNullOrWhiteSpace(txtNew.Text) || string.IsNullOrWhiteSpace(txtConfirm.Text)) { MessageBox.Show("Please fill in all password fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (txtNew.Text != txtConfirm.Text) { MessageBox.Show("New passwords do not match!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlCommand checkCmd = new MySqlCommand("SELECT COUNT(*) FROM staff WHERE StaffID=@id AND Password=SHA2(@oldPwd,256)", conn))
                        {
                            checkCmd.Parameters.AddWithValue("@id", loggedInID); checkCmd.Parameters.AddWithValue("@oldPwd", txtOld.Text);
                            if (Convert.ToInt32(checkCmd.ExecuteScalar()) == 0) { MessageBox.Show("Incorrect Current Password. Access Denied.", "Security Alert", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
                        }
                        using (MySqlCommand updateCmd = new MySqlCommand("UPDATE staff SET Password=SHA2(@newPwd,256) WHERE StaffID=@id", conn))
                        {
                            updateCmd.Parameters.AddWithValue("@id", loggedInID); updateCmd.Parameters.AddWithValue("@newPwd", txtNew.Text); updateCmd.ExecuteNonQuery();
                        }
                        MessageBox.Show("Your password has been changed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); pwdForm.Close();
                    }
                    catch (Exception ex) { MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            };
            pwdForm.ShowDialog();
        }

        private void LoadModule(Type formType)
        {
            try
            {
                if (formType == null) return;
                if (activeForm != null && activeForm.GetType() == formType) return;
                if (activeForm != null) { activeForm.Close(); activeForm.Dispose(); }
                Form targetForm = (Form)Activator.CreateInstance(formType);
                if (targetForm is ProductCreationBOMForm bomForm) { bomForm.OnNavigationBack = () => LoadModule(typeof(ProductManagement)); }
                targetForm.TopLevel = false; targetForm.FormBorderStyle = FormBorderStyle.None; targetForm.Dock = DockStyle.None;
                targetForm.Location = new Point(0, 0); targetForm.BackColor = Color.White;
                targetForm.Size = new Size(Math.Max(pnlContent.Width, 1150), Math.Max(pnlContent.Height, 800));
                pnlContent.Controls.Clear(); pnlContent.Controls.Add(targetForm); GlobalOptimizeChildForm(targetForm);
                targetForm.Show(); activeForm = targetForm;
            }
            catch (Exception ex) { MessageBox.Show($"Failed to load module: {formType?.Name}\n{ex.Message}", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void PnlContent_Resize(object sender, EventArgs e)
        {
            if (activeForm != null) { activeForm.Size = new Size(Math.Max(pnlContent.Width, 1150), Math.Max(pnlContent.Height, 800)); }
        }

        private void MainDashboard_Load(object sender, EventArgs e)
        {
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
    }

    // 💡 類別改名為 ThemeBrushes 防止與 System.Drawing.Brushes 衝突
    public class ThemeBrushes
    {
        private static ThemeBrushes _instance;
        private Dictionary<Color, SolidBrush> _cache = new Dictionary<Color, SolidBrush>();
        public static ThemeBrushes GetInstance() { if (_instance == null) _instance = new ThemeBrushes(); return _instance; }
        public SolidBrush GetColorBrush(Color color) { if (!_cache.ContainsKey(color)) _cache[color] = new SolidBrush(color); return _cache[color]; }
    }
}