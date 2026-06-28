using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class Login : Form
    {
        // ==========================================
        // 🔒 Database Configuration
        // ==========================================
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // ==========================================
        // 🎨 Modern UI Element Variables
        // ==========================================
        private TextBox txtUser, txtPass;
        private Button btnLogin, btnExit, btnMinimize;
        private Panel pnlCard;

        // ⏳ 動畫引擎變數
        private Timer animTimer;
        private float timeOffset = 0f;

        public Login()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                InitializeAnimatedPremiumUI();
                StartBackgroundAnimation();
            }
        }

        #region 🎨 網頁級動態全螢幕 UI 構建
        private void InitializeAnimatedPremiumUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Enterprise ERP Login";

            // 🌟 核心修改：改為電影感無邊框全螢幕 (Borderless Fullscreen)
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true; // 開啟雙重緩衝避免閃爍

            // 💳 懸浮登入卡片 (Login Card)
            pnlCard = new Panel
            {
                Size = new Size(440, 480),
                BackColor = Color.White,
                Padding = new Padding(40)
            };
            this.Controls.Add(pnlCard);

            // 頂部琥珀金飾條
            Panel pnlTopLine = new Panel { Dock = DockStyle.Top, Height = 6, BackColor = Color.FromArgb(245, 158, 11) };
            pnlCard.Controls.Add(pnlTopLine);

            int currentY = 40;

            // 品牌 Logo 與標題
            Label lblBrandLogo = new Label { Text = "🛋️", Font = new Font("Segoe UI", 36F), Location = new Point(0, currentY), Size = new Size(440, 60), TextAlign = ContentAlignment.MiddleCenter };
            currentY += 65;
            Label lblHeader = new Label { Text = "System Login", Font = new Font("Segoe UI", 22F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(0, currentY), Size = new Size(440, 45), TextAlign = ContentAlignment.MiddleCenter };
            currentY += 45;
            Label lblSubHeader = new Label { Text = "Premium Living Furniture Co. Ltd.", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(0, currentY), Size = new Size(440, 20), TextAlign = ContentAlignment.MiddleCenter };
            currentY += 50;

            pnlCard.Controls.Add(lblBrandLogo);
            pnlCard.Controls.Add(lblHeader);
            pnlCard.Controls.Add(lblSubHeader);

            // 帳號輸入框
            Label lblUser = new Label { Text = "USERNAME / STAFF ID", Location = new Point(45, currentY), AutoSize = true, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtUser = new TextBox { Location = new Point(45, currentY + 20), Width = 350, Font = new Font("Segoe UI", 12F), BorderStyle = BorderStyle.FixedSingle, ForeColor = Color.Gray, Text = "Enter Username or ID..." };
            SetupPlaceholder(txtUser, "Enter Username or ID...", false);
            pnlCard.Controls.Add(lblUser); pnlCard.Controls.Add(txtUser);
            currentY += 75;

            // 密碼輸入框
            Label lblPass = new Label { Text = "PASSWORD", Location = new Point(45, currentY), AutoSize = true, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtPass = new TextBox { Location = new Point(45, currentY + 20), Width = 350, Font = new Font("Segoe UI", 12F), BorderStyle = BorderStyle.FixedSingle, ForeColor = Color.Gray, Text = "Enter Password..." };
            SetupPlaceholder(txtPass, "Enter Password...", true);
            pnlCard.Controls.Add(lblPass); pnlCard.Controls.Add(txtPass);
            currentY += 75;

            // 登入按鈕
            btnLogin = new Button { Text = "Sign In / 登入", Location = new Point(45, currentY), Size = new Size(350, 48), BackColor = Color.FromArgb(15, 23, 42), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(30, 41, 59);
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(15, 23, 42);
            btnLogin.Click += btnLogin_Click;
            pnlCard.Controls.Add(btnLogin);

            // 🛠️ 建立右上角全螢幕專用「最小化」與「關閉」按鈕
            btnMinimize = new Button { Text = "—", Font = new Font("Segoe UI", 9F, FontStyle.Bold), Size = new Size(40, 40), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, BackColor = Color.Transparent };
            btnMinimize.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnMinimize);

            btnExit = new Button { Text = "✕", Font = new Font("Segoe UI", 11F, FontStyle.Bold), Size = new Size(40, 40), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, BackColor = Color.Transparent };
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Click += (s, e) => Application.Exit();
            this.Controls.Add(btnExit);

            // 🔄 智能 Resize 引擎：無論任何解析度，永遠即時重新導向計算「絕對置中」
            this.Resize += (s, e) => {
                if (pnlCard != null) pnlCard.Location = new Point((this.Width - pnlCard.Width) / 2, (this.Height - pnlCard.Height) / 2);
                if (btnExit != null) btnExit.Location = new Point(this.Width - 45, 10);
                if (btnMinimize != null) btnMinimize.Location = new Point(this.Width - 85, 10);
            };

            this.AcceptButton = btnLogin;
        }

        private void SetupPlaceholder(TextBox txt, string placeholder, bool isPassword)
        {
            txt.GotFocus += (s, e) => {
                if (txt.Text == placeholder)
                {
                    txt.Text = "";
                    txt.ForeColor = Color.FromArgb(15, 23, 42);
                    if (isPassword) txt.PasswordChar = '●';
                }
            };
            txt.LostFocus += (s, e) => {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    if (isPassword) txt.PasswordChar = '\0';
                    txt.Text = placeholder;
                    txt.ForeColor = Color.Gray;
                }
            };
        }
        #endregion

        #region 🌌 日夜交替全螢幕重繪引擎
        private void StartBackgroundAnimation()
        {
            animTimer = new Timer { Interval = 30 };
            animTimer.Tick += (s, e) =>
            {
                timeOffset += 0.012f; // 稍微放慢一點點，大螢幕滑動更流暢優雅
                if (timeOffset > Math.PI * 2) timeOffset -= (float)(Math.PI * 2);
                this.Invalidate();
            };
            animTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float ratio = (float)(Math.Sin(timeOffset) + 1) / 2f;

            Color dayColor = Color.FromArgb(243, 244, 246);
            Color nightColor = Color.FromArgb(15, 23, 42);

            int r = (int)(dayColor.R + (nightColor.R - dayColor.R) * ratio);
            int gCol = (int)(dayColor.G + (nightColor.G - dayColor.G) * ratio);
            int b = (int)(dayColor.B + (nightColor.B - dayColor.B) * ratio);

            Color currentSky = Color.FromArgb(r, gCol, b);
            g.Clear(currentSky);

            // 🌟 智能動態反轉控制掣顏色，避免按鈕喺日頭/黑夜被背景「食咗」
            Color controlColor = ratio > 0.5f ? Color.FromArgb(203, 213, 225) : Color.FromArgb(71, 85, 105);
            if (btnExit != null) btnExit.ForeColor = controlColor;
            if (btnMinimize != null) btnMinimize.ForeColor = controlColor;

            // 2. 針對全螢幕優化的日月軌跡計算
            int centerX = this.Width / 2;
            int centerY = this.Height + 200; // 降低圓心，擴大拱形拋物線
            int orbitRadius = this.Width / 2 + 150;

            // ☀️ 繪製太陽 (Sun Glow)
            float sunAngle = timeOffset;
            int sunX = centerX - (int)(Math.Cos(sunAngle) * orbitRadius);
            int sunY = centerY - (int)(Math.Sin(sunAngle) * orbitRadius);

            int sunAlpha = (int)(180 * (1 - ratio));
            if (sunAlpha > 0)
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(sunX - 350, sunY - 350, 700, 700); // 放大光暈範圍至 700px 迎合大螢幕
                    using (PathGradientBrush pgb = new PathGradientBrush(path))
                    {
                        pgb.CenterColor = Color.FromArgb(sunAlpha, 245, 158, 11);
                        pgb.SurroundColors = new Color[] { Color.Transparent };
                        g.FillPath(pgb, path);
                    }
                }
            }

            // 🌙 繪製月亮 (Moon Glow)
            float moonAngle = timeOffset + (float)Math.PI;
            int moonX = centerX - (int)(Math.Cos(moonAngle) * orbitRadius);
            int moonY = centerY - (int)(Math.Sin(moonAngle) * orbitRadius);

            int moonAlpha = (int)(180 * ratio);
            if (moonAlpha > 0)
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(moonX - 300, moonY - 300, 600, 600); // 放大光暈範圍至 600px
                    using (PathGradientBrush pgb = new PathGradientBrush(path))
                    {
                        pgb.CenterColor = Color.FromArgb(moonAlpha, 186, 230, 253);
                        pgb.SurroundColors = new Color[] { Color.Transparent };
                        g.FillPath(pgb, path);
                    }
                }
            }
        }
        #endregion

        // ==========================================
        // 🚀 Core Enterprise Security Authentication
        // ==========================================
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string inputUser = txtUser.Text.Trim();
            string inputPass = txtPass.Text.Trim();

            if (string.IsNullOrEmpty(inputUser) || inputUser == "Enter Username or ID..." ||
                string.IsNullOrEmpty(inputPass) || inputPass == "Enter Password...")
            {
                MessageBox.Show("Security Notice:\nPlease enter both your corporate Staff ID and Password to proceed.", "Authorization Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string loginQuery = "SELECT StaffID, Name, Role FROM staff WHERE StaffID = @user AND Password = SHA2(@pass,256)";

                    using (MySqlCommand cmd = new MySqlCommand(loginQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", inputUser);
                        cmd.Parameters.AddWithValue("@pass", inputPass);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                UserSession.LoggedInStaffID = reader["StaffID"].ToString();
                                UserSession.LoggedInStaffName = reader["Name"] != DBNull.Value ? reader["Name"].ToString() : "Unknown";
                                UserSession.LoggedInStaffRole = reader["Role"] != DBNull.Value ? reader["Role"].ToString().Trim() : "";
                                UserSession.LoginTime = DateTime.Now;
                                UserSession.LoggedInDepartment = "General";

                                if (animTimer != null) animTimer.Stop();

                                MessageBox.Show($"Authentication Success!\nWelcome back, {UserSession.LoggedInStaffName} ({UserSession.LoggedInStaffRole}).", "Access Granted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                MainDashboard dashboard = new MainDashboard();
                                dashboard.FormClosed += (s, args) => this.Close();
                                dashboard.Show();
                                this.Hide();
                            }
                            else
                            {
                                MessageBox.Show("Access Denied:\nInvalid Staff ID or Password. Please try again.", "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                txtPass.Clear();
                                if (txtPass.Text == "") { txtPass.Text = "Enter Password..."; txtPass.ForeColor = Color.Gray; txtPass.PasswordChar = '\0'; }
                                txtUser.Focus();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database Connectivity Error:\nUnable to reach the authentication server.\n\nLogs: " + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}