using System;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class MainDashboard : Form // 如果你個檔案叫 Form1.cs，請將呢度改為 Form1
    {
        public MainDashboard()
        {
            InitializeComponent();
            InitializePremiumModernUI(); // 啟動全英文現代化主頁
        }

        private void InitializePremiumModernUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Enterprise Management System";
            this.Size = new Size(1180, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // 1. 頂部深色 Header
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(15, 23, 42)
            };

            Label lblLogo = new Label
            {
                Text = "Premium Living Enterprise Management System",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 22),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblLogo);

            // 登出按鈕
            Button btnLogout = new Button
            {
                Text = "🚪 Logout",
                Location = new Point(1020, 20),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => { Application.Exit(); };
            pnlHeader.Controls.Add(btnLogout);
            this.Controls.Add(pnlHeader);

            // 2. 8 大模組名稱 (完美對應我哋之前整嘅 Sidebar)
            string[] modules = {
                "🛒 Sales Order Mgmt", "🚚 Delivery Logistics",
                "🛋️ Product Maintenance", "👔 HR / Staff Mgmt",
                "📦 Goods Received (GRN)", "🏭 Material Requests",
                "📊 Procurement Control", "🔧 Customer Support"
            };

            // 3. 用迴圈動態生成 8 張現代化大卡片按鈕 (2行 x 4列 排版)
            int startX = 55;
            int startY = 160;
            int col = 0;

            foreach (string mod in modules)
            {
                Button btnModule = new Button
                {
                    Text = mod,
                    Location = new Point(startX + (col * 265), startY),
                    Size = new Size(235, 140),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(15, 23, 42),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                // 卡片邊框設計
                btnModule.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
                btnModule.FlatAppearance.BorderSize = 2;

                // 滑鼠懸浮特效 (Hover Effect)
                btnModule.MouseEnter += (s, e) => { btnModule.BackColor = Color.FromArgb(241, 245, 249); btnModule.FlatAppearance.BorderColor = Color.FromArgb(37, 99, 235); };
                btnModule.MouseLeave += (s, e) => { btnModule.BackColor = Color.White; btnModule.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240); };

                // 🔗 點擊跳轉邏輯 (已經包埋 Goods Received 啦！)
                btnModule.Click += (s, e) => {
                    Form target = null;
                    try
                    {
                        if (mod.Contains("Sales Order")) target = new OrderManagementForm();
                        else if (mod.Contains("Logistics")) target = new LogisticsForm();
                        else if (mod.Contains("Product")) target = new ProductManagement();
                        else if (mod.Contains("HR")) target = new EmployeeManagement();
                        else if (mod.Contains("Goods Received")) target = new GoodsReceivedForm();
                        else if (mod.Contains("Material")) target = new RawMaterialRequestForm();
                        else if (mod.Contains("Procurement")) target = new ProcurementForm();
                        else if (mod.Contains("Support")) target = new AfterServiceForm();

                        if (target != null)
                        {
                            this.Hide();
                            target.FormClosed += (senderForm, args) => this.Show();
                            target.Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Navigation error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                this.Controls.Add(btnModule);

                // 計算下一粒掣嘅位置
                col++;
                if (col == 4)
                {
                    col = 0;
                    startY += 180; // 換行
                }
            }
        }
    }
}