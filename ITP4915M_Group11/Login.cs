using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class Login : Form
    {
        // ==========================================
        // 🔒 Database Configuration
        // ==========================================
        private readonly string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // ==========================================
        // 🎨 Modern UI Element Variables
        // ==========================================
        private TextBox txtUser, txtPass;
        private Button btnLogin;

        public Login()
        {
            InitializeComponent();
            InitializePremiumSplitUI(); // ⬅️ 一鍵動態渲染現代化雙色登入介面
        }

        #region 🎨 Dynamic Premium English UI Construction
        private void InitializePremiumSplitUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Enterprise ERP Login";
            this.Size = new Size(780, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251); // Soft grey-white background
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // 1. Left Corporate Brand Panel (Dark Slate / Deep Navy)
            Panel pnlBrand = new Panel { Width = 320, Dock = DockStyle.Left, BackColor = Color.FromArgb(15, 23, 42) };

            Label lblBrandLogo = new Label { Text = "🛋️", Font = new Font("Segoe UI", 48F), ForeColor = Color.White, Location = new Point(0, 110), Size = new Size(320, 80), TextAlign = ContentAlignment.MiddleCenter };
            Label lblBrandName = new Label { Text = "Premium Living\nFurniture", Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(0, 200), Size = new Size(320, 70), TextAlign = ContentAlignment.MiddleCenter };
            Label lblBrandSub = new Label { Text = "Enterprise Resource Planning System v1.0", Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(0, 400), Size = new Size(320, 30), TextAlign = ContentAlignment.MiddleCenter };

            pnlBrand.Controls.Add(lblBrandLogo);
            pnlBrand.Controls.Add(lblBrandName);
            pnlBrand.Controls.Add(lblBrandSub);
            this.Controls.Add(pnlBrand);

            // 2. Right Workspace Panel (Form Wrapper)
            Panel pnlMain = new Panel { Location = new Point(320, 0), Size = new Size(460, 480), BackColor = Color.Transparent };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label { Text = "Account Login", Font = new Font("Segoe UI", 22F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(50, 50), AutoSize = true };
            Label lblSubHeader = new Label { Text = "Please sign in with your corporate staff credentials.", Font = new Font("Segoe UI", 9.5F), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(54, 100), AutoSize = true };
            pnlMain.Controls.Add(lblHeader);
            pnlMain.Controls.Add(lblSubHeader);

            int currentY = 145;

            // Staff ID Input Block
            Label lblUser = new Label { Text = "Staff ID / Username *", Location = new Point(50, currentY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtUser = new TextBox { Location = new Point(50, currentY + 22), Width = 350, Font = new Font("Segoe UI", 13F), BorderStyle = BorderStyle.FixedSingle, ForeColor = Color.Gray, Text = "Enter Staff ID..." };
            SetupPlaceholder(txtUser, "Enter Staff ID...", false);
            pnlMain.Controls.Add(lblUser);
            pnlMain.Controls.Add(txtUser);

            currentY += 75;

            // Password Input Block
            Label lblPass = new Label { Text = "Password *", Location = new Point(50, currentY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtPass = new TextBox { Location = new Point(50, currentY + 22), Width = 350, Font = new Font("Segoe UI", 13F), BorderStyle = BorderStyle.FixedSingle, ForeColor = Color.Gray, Text = "Enter Password..." };
            SetupPlaceholder(txtPass, "Enter Password...", true);
            pnlMain.Controls.Add(lblPass);
            pnlMain.Controls.Add(txtPass);

            currentY += 85;

            // Premium Login Button
            btnLogin = new Button { Text = "Secure Sign In", Location = new Point(50, currentY), Size = new Size(350, 46), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnLogin.FlatAppearance.BorderSize = 0;

            // Hover Effect Animation
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(29, 78, 216); // Darker blue on hover
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(37, 99, 235); // Original Royal Blue
            btnLogin.Click += btnLogin_Click;
            pnlMain.Controls.Add(btnLogin);

            // Accept Enter Key to instantly submit the login form
            this.AcceptButton = btnLogin;
        }

        private void SetupPlaceholder(TextBox txt, string placeholder, bool isPassword)
        {
            txt.GotFocus += (s, e) => {
                if (txt.Text == placeholder)
                {
                    txt.Text = "";
                    txt.ForeColor = Color.FromArgb(15, 23, 42); // Deep dark text
                    if (isPassword) txt.PasswordChar = '●';
                }
            };
            txt.LostFocus += (s, e) => {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    if (isPassword) txt.PasswordChar = '\0'; // Turn off dots for placeholder text
                    txt.Text = placeholder;
                    txt.ForeColor = Color.Gray;
                }
            };
        }
        #endregion

        // ==========================================
        // 🚀 Core Enterprise Security Authentication
        // ==========================================
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string inputUser = txtUser.Text.Trim();
            string inputPass = txtPass.Text.Trim();

            // Intercept placeholders or empty clicks
            if (string.IsNullOrEmpty(inputUser) || inputUser == "Enter Staff ID..." ||
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

                    // Querying database to match valid staff credentials
                    string loginQuery = "SELECT COUNT(*) FROM staff WHERE StaffID = @user AND Password = @pass";

                    using (MySqlCommand cmd = new MySqlCommand(loginQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", inputUser);
                        cmd.Parameters.AddWithValue("@pass", inputPass);

                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        if (count > 0)
                        {
                            MessageBox.Show("Authentication Success!\nWelcome back to the Premium Living ERP system.", "Access Granted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Seamlessly swap forms
                            MainDashboard dashboard = new MainDashboard();
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
                catch (Exception ex)
                {
                    MessageBox.Show("Database Connectivity Error:\nUnable to reach the authentication server. Please contact IT Support.\n\nLogs: " + ex.Message, "System Infrastructure Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}