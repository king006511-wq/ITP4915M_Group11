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
        private Button btnLogin;
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

        #region 🎨 網頁級動態 UI 構建 (Web-like Dynamic UI)
        private void InitializeAnimatedPremiumUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Enterprise ERP Login";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // 開啟雙重緩衝，防止動畫閃爍 (Anti-Flicker)
            this.DoubleBuffered = true;

            // ==========================================
            // 💳 置中懸浮登入卡片 (Login Card)
            // ==========================================
            pnlCard = new Panel
            {
                Size = new Size(440, 480),
                BackColor = Color.White,
                Padding = new Padding(40)
            };
            // 將卡片置中
            pnlCard.Location = new Point((this.Width - pnlCard.Width) / 2, (this.Height - pnlCard.Height) / 2 - 20);
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

            // 按鈕 Hover 動畫效果
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(30, 41, 59); // Slate 800
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(15, 23, 42); // Slate 900
            btnLogin.Click += btnLogin_Click;
            pnlCard.Controls.Add(btnLogin);

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

        #region 🌌 日夜交替動畫引擎 (Sky Cycle Animation Engine)
        private void StartBackgroundAnimation()
        {
            animTimer = new Timer { Interval = 30 }; // ~33 FPS
            animTimer.Tick += (s, e) =>
            {
                timeOffset += 0.015f; // 控制日夜交替速度
                if (timeOffset > Math.PI * 2) timeOffset -= (float)(Math.PI * 2);
                this.Invalidate(); // 觸發 Form 的 OnPaint 重繪背景
            };
            animTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. 計算日夜比例 (0 = 日, 1 = 夜)
            float ratio = (float)(Math.Sin(timeOffset) + 1) / 2f;

            // 提取網頁版嘅日夜顏色
            Color dayColor = Color.FromArgb(243, 244, 246);   // Slate 50 (白晝)
            Color nightColor = Color.FromArgb(15, 23, 42);    // Slate 900 (黑夜)

            // 平滑過渡 RGB 值
            int r = (int)(dayColor.R + (nightColor.R - dayColor.R) * ratio);
            int gCol = (int)(dayColor.G + (nightColor.G - dayColor.G) * ratio);
            int b = (int)(dayColor.B + (nightColor.B - dayColor.B) * ratio);

            Color currentSky = Color.FromArgb(r, gCol, b);
            g.Clear(currentSky); // 繪製背景

            // 2. 準備繪製日月光暈
            int centerX = this.Width / 2;
            int centerY = this.Height + 100; // 圓心在視窗底部下方，形成拱形軌跡
            int orbitRadius = this.Width / 2 + 100;

            // ☀️ 繪製太陽 (Sun Glow)
            float sunAngle = timeOffset;
            int sunX = centerX - (int)(Math.Cos(sunAngle) * orbitRadius);
            int sunY = centerY - (int)(Math.Sin(sunAngle) * orbitRadius);

            // 太陽在夜晚會變透明
            int sunAlpha = (int)(180 * (1 - ratio));
            if (sunAlpha > 0)
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(sunX - 250, sunY - 250, 500, 500);
                    using (PathGradientBrush pgb = new PathGradientBrush(path))
                    {
                        pgb.CenterColor = Color.FromArgb(sunAlpha, 245, 158, 11); // Amber
                        pgb.SurroundColors = new Color[] { Color.Transparent };
                        g.FillPath(pgb, path);
                    }
                }
            }

            // 🌙 繪製月亮 (Moon Glow) - 在太陽對面 (+180度/PI)
            float moonAngle = timeOffset + (float)Math.PI;
            int moonX = centerX - (int)(Math.Cos(moonAngle) * orbitRadius);
            int moonY = centerY - (int)(Math.Sin(moonAngle) * orbitRadius);

            // 月亮在白天會變透明
            int moonAlpha = (int)(180 * ratio);
            if (moonAlpha > 0)
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(moonX - 200, moonY - 200, 400, 400);
                    using (PathGradientBrush pgb = new PathGradientBrush(path))
                    {
                        pgb.CenterColor = Color.FromArgb(moonAlpha, 186, 230, 253); // Light Blue
                        pgb.SurroundColors = new Color[] { Color.Transparent };
                        g.FillPath(pgb, path);
                    }
                }
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {

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

                    // 使用 SHA2(256) 比對已雜湊的密碼
                    string loginQuery = "SELECT StaffID, Name, Role FROM staff WHERE StaffID = @user AND Password = SHA2(@pass,256)";

                    using (MySqlCommand cmd = new MySqlCommand(loginQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", inputUser);
                        cmd.Parameters.AddWithValue("@pass", inputPass);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // 💾 儲存 Session
                                UserSession.LoggedInStaffID = reader["StaffID"].ToString();
                                UserSession.LoggedInStaffName = reader["Name"] != DBNull.Value ? reader["Name"].ToString() : "Unknown";
                                UserSession.LoggedInStaffRole = reader["Role"] != DBNull.Value ? reader["Role"].ToString().Trim() : "";
                                UserSession.LoginTime = DateTime.Now;
                                UserSession.LoggedInDepartment = "General";

                                // 停止動畫節省資源
                                if (animTimer != null) animTimer.Stop();

                                MessageBox.Show($"Authentication Success!\nWelcome back, {UserSession.LoggedInStaffName} ({UserSession.LoggedInStaffRole}).", "Access Granted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // 跳轉至主控制面板
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