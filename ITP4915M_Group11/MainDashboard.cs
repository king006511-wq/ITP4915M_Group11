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
        private Form activeForm = null;      // 記錄目前右側正在顯示的子視窗
        private Panel pnlContent;            // 右側的主要內容容器面版
        private Panel pnlLeftNav;            // 左側的導覽列面版

        public MainDashboard()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                // 初始化全域單一主控台容器架構
                InitializePremiumContainerUI();
            }
        }

        /// <summary>
        /// 🎨 頂級企業級容器 UI 渲染
        /// </summary>
        private void InitializePremiumContainerUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Enterprise ERP Dashboard";
            this.Size = new Size(1280, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // 1. 頂部深色 Header 
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(15, 23, 42)
            };

            Label lblLogo = new Label
            {
                Text = "PREMIUM LIVING ENTERPRISE ERP SYSTEM",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            pnlHeader.Controls.Add(lblLogo);
            this.Controls.Add(pnlHeader);

            // 2. 左側：統一導覽列
            pnlLeftNav = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = Color.FromArgb(30, 41, 59)
            };
            this.Controls.Add(pnlLeftNav);

            // 3. 右側：全域內容容器 (✨ 加上 AutoScroll 防止內容被裁切看不見)
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(241, 245, 249),
                AutoScroll = true
            };
            this.Controls.Add(pnlContent);

            // 確保 Z 軸排版順序正確，右側容器不會鑽進導覽列底部
            pnlHeader.SendToBack();
            pnlLeftNav.SendToBack();
            pnlContent.BringToFront();

            // 4. 動態生成左側導覽按鈕
            BuildNavigationMenu();

            // 預設載入首頁歡迎訊息
            ShowWelcomeMessage();
        }

        /// <summary>
        /// 🛠️ 集中管理左側導覽選單 (✨ 已補齊所有遺失的模組)
        /// </summary>
        /// <summary>
        /// 🛠️ 集中管理左側導覽選單 (✨ 已補齊所有遺失的模組，包含 CustomerManagement)
        /// </summary>
        private void BuildNavigationMenu()
        {
            // 完整對齊你系統的所有模組，並確保正確的 Form 類別名稱
            var modules = new List<(string DisplayName, Type FormType)>
    {
        ("📊 System Dashboards", null), // 分隔標籤
        ("👥 Customer Mgmt", typeof(CustomerManagement)), // 🌟 ADDED: Customer Management module
        ("🛒 Sales Order Mgmt", typeof(OrderManagementForm)),
        ("🚚 Delivery Logistics", typeof(LogisticsForm)),
        ("📦 Goods Received (GRN)", typeof(GoodsReceivedForm)),
        ("🛋️ Product Maintenance", typeof(ProductManagement)),

        ("⚙️ Internal Ops", null), // 分隔標籤
        ("🏭 Material Requests", typeof(RawMaterialRequestForm)),
        ("📈 Procurement Control", typeof(ProcurementForm)),
        ("👔 HR / Staff Mgmt", typeof(EmployeeManagement)),
        ("📞 Customer Support", typeof(AfterServiceForm))
    };

            int currentY = 20;

            foreach (var mod in modules)
            {
                // 如果 FormType 為 null，代表它是純文字分組標籤
                if (mod.FormType == null)
                {
                    Label lblSection = new Label
                    {
                        Text = mod.DisplayName,
                        Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(148, 163, 184),
                        Location = new Point(15, currentY),
                        AutoSize = true
                    };
                    pnlLeftNav.Controls.Add(lblSection);
                    currentY += 30;
                    continue;
                }

                // 建立現代化扁平式導覽按鈕
                Button btnNav = new Button
                {
                    Text = "  " + mod.DisplayName,
                    Location = new Point(10, currentY),
                    Size = new Size(220, 42),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(226, 232, 240),
                    BackColor = Color.FromArgb(30, 41, 59),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand
                };
                btnNav.FlatAppearance.BorderSize = 0;
                btnNav.FlatAppearance.MouseOverBackColor = Color.FromArgb(71, 85, 105);

                // 綁定點擊事件
                btnNav.Click += (sender, e) =>
                {
                    ExecuteSecureNavigation(mod.FormType);
                };

                pnlLeftNav.Controls.Add(btnNav);
                currentY += 48;
            }

            // 最下方的系統登出按鈕
            Button btnLogout = new Button
            {
                Text = "  🚪 Logout System",
                Location = new Point(10, 650),
                Size = new Size(220, 42),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(239, 68, 68),
                BackColor = Color.FromArgb(30, 41, 59),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => { this.Close(); };
            pnlLeftNav.Controls.Add(btnLogout);
        }

        /// <summary>
        /// 🔒 安全導覽核心：檢查權限，通過後將子表單嵌入右側 Panel
        /// </summary>
        private void ExecuteSecureNavigation(Type formType)
        {
            bool allowed = false;
            string name = formType.Name;

            // 依據模組分配角色權限 (防呆機制)
            if (name == nameof(EmployeeManagement))
                allowed = AuthorizationHelper.IsInRole(AuthorizationHelper.Roles.Manager, AuthorizationHelper.Roles.Administrator);
            else if (name == nameof(CustomerManagement) || name == nameof(OrderManagementForm) || name == nameof(AfterServiceForm))
                allowed = AuthorizationHelper.IsInRole(AuthorizationHelper.Roles.Manager, AuthorizationHelper.Roles.Administrator, AuthorizationHelper.Roles.Sales, AuthorizationHelper.Roles.Warehouse);
            else if (name == nameof(RawMaterialRequestForm) || name == nameof(ProcurementForm))
                allowed = AuthorizationHelper.IsInRole(AuthorizationHelper.Roles.Manager, AuthorizationHelper.Roles.Administrator, AuthorizationHelper.Roles.Procurement);
            else if (name == nameof(ProductManagement) || name == nameof(GoodsReceivedForm))
                allowed = AuthorizationHelper.IsInRole(AuthorizationHelper.Roles.Manager, AuthorizationHelper.Roles.Administrator, AuthorizationHelper.Roles.Warehouse);
            else if (name == nameof(LogisticsForm))
                allowed = AuthorizationHelper.IsInRole(AuthorizationHelper.Roles.Manager, AuthorizationHelper.Roles.Administrator, AuthorizationHelper.Roles.Logistics);
            else
                allowed = true; // 預設放行

            if (!allowed)
            {
                MessageBox.Show("Access Denied: Your assigned corporate role possesses insufficient authorization to load this module.", "System Security Gatekeeper", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            // 權限通過，載入表單
            try
            {
                Form targetForm = (Form)Activator.CreateInstance(formType);
                LoadSubForm(targetForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Infrastructure loading failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// ✨ 核心嵌入方法：剝除視窗外殼並動態載入至右側容器 Panel
        /// </summary>
        private void LoadSubForm(Form childForm)
        {
            if (activeForm != null)
            {
                activeForm.Close(); // 關閉上一個畫面，釋放記憶體與連線
            }

            activeForm = childForm;

            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            // 🪄 【全自動版面校正魔法】：自動把跑掉的表單拉正！
            AdjustChildFormLayout(childForm);

            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(childForm);
            pnlContent.Tag = childForm;

            childForm.BringToFront();
            childForm.Show();
        }

        /// <summary>
        /// 🪄 解決子表單偏右、有空白、或超過邊界的自動修正演算法
        /// </summary>
        private void AdjustChildFormLayout(Form childForm)
        {
            childForm.BackColor = Color.FromArgb(249, 250, 251);

            foreach (Control ctrl in childForm.Controls)
            {
                // 如果子表單的容器(pnlMain)被設定了偏右的座標(例如 X=260)
                if (ctrl is Panel pnl && (pnl.Location.X > 0 || pnl.Name.Contains("Main")))
                {
                    // 強制把它吸附到左上角 (0, 0)，填滿剩餘空間，解決右邊被裁切的問題！
                    pnl.Location = new Point(0, 0);
                    pnl.Size = new Size(1000, 750); // 設定一個安全的顯示寬度

                    // 順便尋找並隱藏原本子表單內手動寫的「Go Back」按鈕，因為左側導覽列已經夠用了
                    foreach (Control innerCtrl in pnl.Controls)
                    {
                        if (innerCtrl is Button btn && (btn.Text.Contains("Go Back") || btn.Text.Contains("Back Home")))
                        {
                            btn.Visible = false;
                        }
                    }
                }

                // 隱藏最外層的返回按鈕
                if (ctrl is Button outerBtn && (outerBtn.Text.Contains("Go Back") || outerBtn.Text.Contains("Back Home")))
                {
                    outerBtn.Visible = false;
                }
            }
        }

        /// <summary>
        /// 預設的主畫面歡迎底圖
        /// </summary>
        private void ShowWelcomeMessage()
        {
            pnlContent.Controls.Clear();
            Label lblWelcomeMessage = new Label
            {
                Text = "Welcome to Premium Living ERP Dashboard.\n\nPlease select an operational module from the left matrix navigation menu.",
                Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            pnlContent.Controls.Add(lblWelcomeMessage);
        }

        private void MainDashboard_Load(object sender, EventArgs e)
        {

        }
    }
}