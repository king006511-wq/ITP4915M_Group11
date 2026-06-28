using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class MainDashboard : Form
    {
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

            // 全螢幕啟動
            this.Size = new Size(1300, 850);
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(243, 244, 246); // 現代背景灰 Slate 50
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // 1. 現代深色導覽列
            pnlLeftNav = new Panel
            {
                Dock = DockStyle.Left,
                Width = 260,
                BackColor = Color.FromArgb(15, 23, 42), // Slate 900
                Padding = new Padding(0, 20, 0, 0)
            };

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

            flpNavMenu = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(10, 20, 10, 10)
            };
            pnlLeftNav.Controls.Add(flpNavMenu);

            // 2. 右側動態內容區
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(243, 244, 246),
                AutoScroll = true
            };

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlLeftNav);

            SetupNavigationMenu();
            ShowHomeDashboard();
        }

        private void SetupNavigationMenu()
        {
            // ==== 系統與戰情室 ====
            AddNavHeader("📊 System Dashboards");
            AddNavButton("🏠 Home Dashboard", null, "HOME", "HOME");

            // ==== 核心業務模組 ====
            if (AuthorizationHelper.HasMenuPermission("CUSTOMER_MGMT") ||
                AuthorizationHelper.HasMenuPermission("SALES_QUOTATION") || // 🌟 加入報價單權限檢查
                AuthorizationHelper.HasMenuPermission("SALES_ORDER") ||
                AuthorizationHelper.HasMenuPermission("DELIVERY_LOGISTICS") ||
                AuthorizationHelper.HasMenuPermission("GOODS_RECEIVED") ||
                AuthorizationHelper.HasMenuPermission("PRODUCT_MAINTENANCE") ||
                AuthorizationHelper.HasMenuPermission("PRODUCT_MANUFACTURING"))
            {
                AddNavHeader("💼 Core Modules");
                AddNavButton("👥 Customer Mgmt", typeof(CustomerManagement), "FORM", "CUSTOMER_MGMT");

                // 🌟 加入 Sales Quotation 報價單按鈕 (放喺 Sales Order 上面最順理成章)
                AddNavButton("📄 Sales Quotation", typeof(SalesQuotationForm), "FORM", "SALES_QUOTATION");

                AddNavButton("🛒 Sales Order Mgmt", typeof(OrderManagementForm), "FORM", "SALES_ORDER");
                AddNavButton("🚚 Delivery Logistics", typeof(LogisticsForm), "FORM", "DELIVERY_LOGISTICS");
                AddNavButton("📦 Goods Received (GRN)", typeof(GoodsReceivedForm), "FORM", "GOODS_RECEIVED");
                AddNavButton("🛋️ Product Maintenance", typeof(ProductManagement), "FORM", "PRODUCT_MAINTENANCE");
                AddNavButton("🛠️ Product Manufacturing", typeof(ProductManufacturingForm), "FORM", "PRODUCT_MANUFACTURING");
            }

            // ==== 內部營運控制 ====
            // ... (下面維持不變)

            // ==== 內部營運控制 ====
            if (AuthorizationHelper.HasMenuPermission("MATERIAL_REQUESTS") ||
                AuthorizationHelper.HasMenuPermission("PROCUREMENT_CONTROL") ||
                AuthorizationHelper.HasMenuPermission("HR_STAFF_MGMT") ||
                AuthorizationHelper.HasMenuPermission("CUSTOMER_SUPPORT"))
            {
                AddNavHeader("⚙️ Internal Ops");
                AddNavButton("🏭 Material Requests", typeof(RawMaterialRequestForm), "FORM", "MATERIAL_REQUESTS");
                AddNavButton("📈 Procurement Control", typeof(ProcurementForm), "FORM", "PROCUREMENT_CONTROL");
                AddNavButton("👔 HR / Staff Mgmt", typeof(EmployeeManagement), "FORM", "HR_STAFF_MGMT");
                AddNavButton("📞 Customer Support", typeof(AfterServiceForm), "FORM", "CUSTOMER_SUPPORT");
            }

            // ==== 系統操作 ====
            AddNavHeader(""); // 分隔用空白列
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
            // 如果是表單按鈕且沒有權限，則直接返回不添加
            if (actionType == "FORM" && !AuthorizationHelper.HasMenuPermission(menuId))
            {
                return;
            }

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
                Tag = menuId  // 儲存菜單 ID 供參考
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

        #region 🏠 Home Dashboard
        private void ShowHomeDashboard()
        {
            if (activeForm != null) { activeForm.Close(); activeForm = null; }
            pnlContent.Controls.Clear();

            Label lblTitle = new Label { Text = "Welcome back, Operator ✨", Font = new Font("Segoe UI Black", 22F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(40, 30), AutoSize = true };
            Label lblSubTitle = new Label { Text = "Premium Living Enterprise ERP Control System.", Font = new Font("Segoe UI", 11F), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(43, 75), AutoSize = true };
            pnlContent.Controls.Add(lblTitle); pnlContent.Controls.Add(lblSubTitle);

            FlowLayoutPanel flpStats = new FlowLayoutPanel { Location = new Point(40, 120), Size = new Size(pnlContent.Width - 80, 130), FlowDirection = FlowDirection.LeftToRight, WrapContents = true, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            pnlContent.Controls.Add(flpStats);

            flpStats.Controls.Add(CreateStatCard("TODAY'S ORDERS", "24", "+12%", Color.FromArgb(14, 165, 233)));
            flpStats.Controls.Add(CreateStatCard("PENDING DISPATCH", "12", "-2", Color.FromArgb(245, 158, 11)));
            flpStats.Controls.Add(CreateStatCard("ACTIVE TICKETS", "5", "Action Req.", Color.FromArgb(239, 68, 68)));
            flpStats.Controls.Add(CreateStatCard("MONTHLY REVENUE", "$142.5K", "+8.4%", Color.FromArgb(16, 185, 129)));
        }

        private Panel CreateStatCard(string title, string value, string trend, Color themeColor)
        {
            Panel card = new Panel { Size = new Size(220, 110), BackColor = Color.White, Margin = new Padding(0, 0, 20, 20) };
            card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            Panel topBar = new Panel { Dock = DockStyle.Top, Height = 4, BackColor = themeColor };
            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(15, 15), AutoSize = true };
            Label lblValue = new Label { Text = value, Font = new Font("Segoe UI Black", 24F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(12, 35), AutoSize = true };
            Label lblTrend = new Label { Text = trend, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = themeColor, Location = new Point(15, 80), AutoSize = true };
            card.Controls.Add(topBar); card.Controls.Add(lblTitle); card.Controls.Add(lblValue); card.Controls.Add(lblTrend);
            return card;
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
                targetForm.TopLevel = false;
                targetForm.FormBorderStyle = FormBorderStyle.None;

                // 佈局設定
                targetForm.Dock = DockStyle.None;
                targetForm.Location = new Point(0, 0);
                targetForm.BackColor = Color.White;

                // 安全範圍限制：畫面大就跟著放大，畫面小就守住底線
                targetForm.Size = new Size(
                    Math.Max(pnlContent.Width, 1150),
                    Math.Max(pnlContent.Height, 800)
                );

                pnlContent.Controls.Clear();
                pnlContent.Controls.Add(targetForm);

                // 🌟 神奇攔截點：在這裡強制從外面幫子表單做整形，唔使改子表單 code
                GlobalOptimizeChildForm(targetForm);

                targetForm.Show();
                activeForm = targetForm;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load module: {formType?.Name}\n{ex.Message}", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 🛠️ 遞迴掃描子表單所有控制項，強制優化 DataGridView 顯示並隱藏舊版返回按鈕
        /// </summary>
        private void GlobalOptimizeChildForm(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                // 1. 解決 DataGridView 擠在左邊留白的問題
                if (ctrl is DataGridView dgv)
                {
                    // 強制欄位等比例拉伸填滿表格內部空白
                    dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // 強制表格本身跟著外框放大縮小
                    dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

                    dgv.BackgroundColor = Color.White;
                    dgv.BorderStyle = BorderStyle.None;
                }

                // 2. 隱藏多餘的 Go Back / 箭頭 返回按鈕
                if (ctrl is Button btn && (btn.Text.Contains("Go Back") || btn.Text.Contains("Back Home") || btn.Text.Contains("⬅")))
                {
                    btn.Visible = false;
                }

                // 3. 繼續往內層尋找
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