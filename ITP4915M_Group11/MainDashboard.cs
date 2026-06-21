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
        private FlowLayoutPanel flpNavMenu; // 🛠️ 改用自動排版選單，防止項目重疊或消失

        public MainDashboard()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                InitializePremiumContainerUI();
            }
        }

        #region 🎨 全螢幕響應式視窗與導覽架構初始化
        private void InitializePremiumContainerUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Enterprise ERP Dashboard";

            // 🛠️ 聽從指示：直接預設全螢幕最大化，完美解決解析度不夠、內容擠在中間或切一半的問題
            this.Size = new Size(1300, 850);
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(243, 244, 246); // Slate 50
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;

            // 1. 左側深色工作導覽列
            pnlLeftNav = new Panel
            {
                Dock = DockStyle.Left,
                Width = 260,
                BackColor = Color.FromArgb(15, 23, 42) // Slate 900
            };
            this.Controls.Add(pnlLeftNav);

            // 企業 Logo 區塊
            Panel pnlLogo = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(15, 23, 42) };
            Label lblLogo = new Label
            {
                Text = "PREMIUM\nLIVING",
                Font = new Font("Segoe UI Black", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = true
            };
            pnlLogo.Controls.Add(lblLogo);
            pnlLeftNav.Controls.Add(pnlLogo);

            // 2. 右側主工作視窗 (徹底修正：開啟滾動條、滿版配置，絕不切半)
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(243, 244, 246),
                AutoScroll = true // 💡 當子表單資料輸入區過大時，自動生成滾動條，確保欄位看得到、點得到
            };
            this.Controls.Add(pnlContent);

            // 3. 自動流式選單容器 (防漏件、防切碎防線)
            flpNavMenu = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true, // 選單項目太多時自動允許滾動
                Padding = new Padding(10, 10, 10, 10),
                BackColor = Color.FromArgb(15, 23, 42)
            };
            pnlLeftNav.Controls.Add(flpNavMenu);
            flpNavMenu.BringToFront();

            // 4. 載入完整的系統清單與首頁
            BuildNavigationMenu();
            ShowHomeDashboard();
        }

        private void BuildNavigationMenu()
        {
            flpNavMenu.Controls.Clear();

            // 🌟 一個都不少！完美接回你所有的原始表單，並保留新加入的統計報表功能
            var menuItems = new List<(string DisplayName, Type FormType, string ActionType)>
            {
                // ---- 分類頁籤 1：系統戰情室 ----
                ("📊 System Dashboards", null, "HEADER"),
                ("🏠 Home Dashboard", null, "HOME"),
                ("📈 Statistical Reports", null, "STATS"),

                // ---- 業務模組 ----
                ("👥 Customer Mgmt", typeof(CustomerManagement), "FORM"),
                ("🛒 Sales Order Mgmt", typeof(OrderManagementForm), "FORM"),
                ("🚚 Delivery Logistics", typeof(LogisticsForm), "FORM"),
                ("📦 Goods Received (GRN)", typeof(GoodsReceivedForm), "FORM"),
                ("🛋️ Product Maintenance", typeof(ProductManagement), "FORM"),

                // ---- 分類頁籤 2：內部營運控制 ----
                ("⚙️ Internal Ops", null, "HEADER"),
                ("🏭 Material Requests", typeof(RawMaterialRequestForm), "FORM"),
                ("📈 Procurement Control", typeof(ProcurementForm), "FORM"),
                ("👔 HR / Staff Mgmt", typeof(EmployeeManagement), "FORM"),
                ("📞 Customer Support", typeof(AfterServiceForm), "FORM")
            };

            foreach (var item in menuItems)
            {
                if (item.ActionType == "HEADER")
                {
                    // 渲染群組標題 (例如：System Dashboards / Internal Ops)
                    Label lblHeader = new Label
                    {
                        Text = item.DisplayName,
                        Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(100, 116, 139), // Slate 400
                        Size = new Size(220, 25),
                        Margin = new Padding(5, 15, 5, 5),
                        TextAlign = ContentAlignment.BottomLeft
                    };
                    flpNavMenu.Controls.Add(lblHeader);
                }
                else
                {
                    // 渲染功能按鈕
                    Button btnNav = new Button
                    {
                        Text = "  " + item.DisplayName,
                        Size = new Size(220, 40),
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(226, 232, 240),
                        BackColor = Color.Transparent,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Margin = new Padding(5, 3, 5, 3),
                        Cursor = Cursors.Hand
                    };
                    btnNav.FlatAppearance.BorderSize = 0;
                    btnNav.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);

                    // 綁定動態點擊導航事件
                    btnNav.Click += (s, e) =>
                    {
                        if (item.ActionType == "HOME") ShowHomeDashboard();
                        else if (item.ActionType == "STATS") ShowStatisticalReports();
                        else ExecuteSecureNavigation(item.FormType);
                    };

                    flpNavMenu.Controls.Add(btnNav);
                }
            }

            // 安全登出按鈕
            Button btnLogout = new Button { Text = "  🚪 Logout System", Size = new Size(220, 40), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(239, 68, 68), BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(5, 30, 5, 5), Cursor = Cursors.Hand };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => { this.Close(); };
            flpNavMenu.Controls.Add(btnLogout);
        }
        #endregion

        #region 🏠 Home Dashboard (首頁戰情快捷中心)
        private void ShowHomeDashboard()
        {
            if (activeForm != null) { activeForm.Close(); activeForm = null; }
            pnlContent.Controls.Clear();

            // 標題歡迎文字
            Label lblTitle = new Label { Text = "Welcome back, Operator ✨", Font = new Font("Segoe UI Black", 22F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(40, 30), AutoSize = true };
            Label lblSubTitle = new Label { Text = "Premium Living furniture enterprise main command bridge plant.", Font = new Font("Segoe UI", 11F), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(43, 75), AutoSize = true };
            pnlContent.Controls.Add(lblTitle); pnlContent.Controls.Add(lblSubTitle);

            // 1. KPI 數據看板列 (寬度自適應)
            FlowLayoutPanel flpStats = new FlowLayoutPanel { Location = new Point(40, 120), Size = new Size(pnlContent.Width - 80, 130), FlowDirection = FlowDirection.LeftToRight, WrapContents = true, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            pnlContent.Controls.Add(flpStats);

            flpStats.Controls.Add(CreateStatCard("TODAY'S ORDERS", "24", "+12%", Color.FromArgb(14, 165, 233)));
            flpStats.Controls.Add(CreateStatCard("PENDING DISPATCH", "12", "-2", Color.FromArgb(245, 158, 11)));
            flpStats.Controls.Add(CreateStatCard("ACTIVE TICKETS", "5", "Action Req.", Color.FromArgb(239, 68, 68)));
            flpStats.Controls.Add(CreateStatCard("MONTHLY REVENUE", "$142.5K", "+8.4%", Color.FromArgb(16, 185, 129)));

            // 2. 常用模組大按鈕快捷區
            Label lblQuickAccess = new Label { Text = "Core Operations Fast-Lane", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(40, 280), AutoSize = true };
            pnlContent.Controls.Add(lblQuickAccess);

            FlowLayoutPanel flpModules = new FlowLayoutPanel { Location = new Point(40, 325), Size = new Size(pnlContent.Width - 80, pnlContent.Height - 380), FlowDirection = FlowDirection.LeftToRight, WrapContents = true, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            pnlContent.Controls.Add(flpModules);

            flpModules.Controls.Add(CreateQuickAccessBtn("👥", "Customer Management", typeof(CustomerManagement)));
            flpModules.Controls.Add(CreateQuickAccessBtn("🛒", "Sales Order Processing", typeof(OrderManagementForm)));
            flpModules.Controls.Add(CreateQuickAccessBtn("🚚", "Logistics & Fleet", typeof(LogisticsForm)));
            flpModules.Controls.Add(CreateQuickAccessBtn("📦", "Goods Received (GRN)", typeof(GoodsReceivedForm)));
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

        private Button CreateQuickAccessBtn(string emoji, string title, Type targetForm)
        {
            Button btn = new Button { Text = $"{emoji}\n\n{title}", Size = new Size(200, 130), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Margin = new Padding(0, 0, 20, 20), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            btn.FlatAppearance.BorderSize = 2;
            btn.Click += (s, e) => ExecuteSecureNavigation(targetForm);
            return btn;
        }
        #endregion

        #region 📊 Statistical Reports (高級統計圖表專區)
        private void ShowStatisticalReports()
        {
            if (activeForm != null) { activeForm.Close(); activeForm = null; }
            pnlContent.Controls.Clear();

            Label lblTitle = new Label { Text = "📊 Enterprise Analytics & Operational Reports", Font = new Font("Segoe UI Black", 22F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(40, 30), AutoSize = true };
            pnlContent.Controls.Add(lblTitle);

            // 用 TableLayoutPanel 平分畫面，確保不縮在中間
            TableLayoutPanel tlpCharts = new TableLayoutPanel { Location = new Point(40, 100), Size = new Size(pnlContent.Width - 80, 360), ColumnCount = 2, RowCount = 1, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            tlpCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlContent.Controls.Add(tlpCharts);

            // 圖表 1：銷售分析
            Panel pnlSalesChart = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, 15, 0) };
            pnlSalesChart.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlSalesChart.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            tlpCharts.Controls.Add(pnlSalesChart, 0, 0);

            Label lblSalesTitle = new Label { Text = "Q2 Revenue Matrix Performance ($K)", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(20, 20), AutoSize = true };
            pnlSalesChart.Controls.Add(lblSalesTitle);

            string[] months = { "April", "May", "June (MTD)" }; int[] values = { 130, 195, 155 }; int chartY = 80;
            for (int i = 0; i < months.Length; i++)
            {
                Label lblM = new Label { Text = months[i], Location = new Point(20, chartY), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.DimGray, AutoSize = true };
                Panel pnlBarBg = new Panel { Location = new Point(130, chartY), Size = new Size(220, 22), BackColor = Color.FromArgb(241, 245, 249) };
                Panel pnlBar = new Panel { Location = new Point(130, chartY), Size = new Size(values[i], 22), BackColor = Color.FromArgb(99, 102, 241) };
                Label lblVal = new Label { Text = $"${values[i]}K", Location = new Point(365, chartY), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), AutoSize = true };
                pnlSalesChart.Controls.Add(lblM); pnlSalesChart.Controls.Add(pnlBar); pnlSalesChart.Controls.Add(pnlBarBg); pnlSalesChart.Controls.Add(lblVal);
                pnlBar.BringToFront(); chartY += 65;
            }

            // 圖表 2：售後服務結案進度
            Panel pnlTickets = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(15, 0, 0, 0) };
            pnlTickets.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlTickets.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            tlpCharts.Controls.Add(pnlTickets, 1, 0);

            Label lblTickTitle = new Label { Text = "Customer Support Resolution Health", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(20, 20), AutoSize = true };
            pnlTickets.Controls.Add(lblTickTitle);

            string[] statuses = { "Closed & Resolved", "In Pipeline Processing", "Awaiting Escalation" }; int[] percentages = { 80, 14, 6 };
            Color[] colors = { Color.FromArgb(16, 185, 129), Color.FromArgb(245, 158, 11), Color.FromArgb(239, 68, 68) }; int tickY = 80;
            for (int i = 0; i < statuses.Length; i++)
            {
                Label lblS = new Label { Text = statuses[i], Location = new Point(20, tickY), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), AutoSize = true };
                Label lblP = new Label { Text = $"{percentages[i]}%", Location = new Point(360, tickY), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = colors[i], AutoSize = true };
                Panel pnlBarBg = new Panel { Location = new Point(20, tickY + 25), Size = new Size(380, 12), BackColor = Color.FromArgb(226, 232, 240) };
                Panel pnlBar = new Panel { Location = new Point(20, tickY + 25), Size = new Size(380 * percentages[i] / 100, 12), BackColor = colors[i] };
                pnlTickets.Controls.Add(lblS); pnlTickets.Controls.Add(lblP); pnlTickets.Controls.Add(pnlBar); pnlTickets.Controls.Add(pnlBarBg);
                pnlBar.BringToFront(); tickY += 75;
            }
        }
        #endregion

        #region 🚀 核心動態導航渲染引擎 (100% 覆蓋填滿，不切邊)
        private void ExecuteSecureNavigation(Type formType)
        {
            if (formType == null) return;

            if (activeForm != null)
            {
                activeForm.Close();
                activeForm.Dispose();
            }

            try
            {
                Form targetForm = (Form)Activator.CreateInstance(formType);
                targetForm.TopLevel = false;
                targetForm.FormBorderStyle = FormBorderStyle.None;

                // 🛠️ 關鍵修復：強制讓子表單完全拉伸填滿，並且啟用內置滾動機制防止輸入欄位被擠壓切半
                targetForm.Dock = DockStyle.Fill;
                targetForm.AutoScroll = true;

                pnlContent.Controls.Clear();
                pnlContent.Controls.Add(targetForm);
                targetForm.Show();
                activeForm = targetForm;

                // 自動巡檢隱藏子表單內多餘的舊返回按鈕
                HideInternalReturnButtons(targetForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load standard module: {formType.Name}\n{ex.Message}", "Navigation System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HideInternalReturnButtons(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Button btn && (btn.Text.Contains("Go Back") || btn.Text.Contains("Back Home")))
                {
                    btn.Visible = false;
                }
                if (ctrl.HasChildren)
                {
                    HideInternalReturnButtons(ctrl);
                }
            }
        }
        #endregion
    }
}