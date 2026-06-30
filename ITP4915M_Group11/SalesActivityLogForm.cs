using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ITP4915M_Group11
{
    public partial class SalesActivityLogForm : Form
    {
        // 🔒 Database Configuration
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // UI Elements
        private DataGridView dgvLogs;

        public SalesActivityLogForm()
        {
            SetupUI();
            LoadData();
        }

        #region 🎨 UI Setup
        private void SetupUI()
        {
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;

            // 1. Header Panel
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White
            };
            pnlHeader.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlHeader.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

            Label lblTitle = new Label
            {
                Text = "📜 Sales Activity Log",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(30, 25),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);
            this.Controls.Add(pnlHeader);

            // 2. DataGrid Container Panel (For Box Shadow / Border effect)
            Panel pnlGrid = new Panel
            {
                Location = new Point(30, 110),
                Width = this.Width - 60,
                Height = this.Height - 140,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.White
            };
            pnlGrid.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlGrid.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

            // 3. Modern DataGridView
            dgvLogs = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(pnlGrid.Width - 40, pnlGrid.Height - 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(241, 245, 249)
            };

            // Grid Styling
            dgvLogs.EnableHeadersVisualStyles = false;
            dgvLogs.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235); // Royal Blue
            dgvLogs.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvLogs.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvLogs.ColumnHeadersHeight = 40;
            dgvLogs.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            dgvLogs.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 231, 255); // Light Blue hover
            dgvLogs.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
            dgvLogs.RowTemplate.Height = 35;

            pnlGrid.Controls.Add(dgvLogs);
            this.Controls.Add(pnlGrid);
        }
        #endregion

        #region 💽 Data Loading
        private void LoadData()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // 加入 OrderDate 令紀錄更加完整
                    string query = @"
                        SELECT 
                            OrderID AS 'Order Ref', 
                            CustomerID AS 'Customer', 
                            TotalAmount AS 'Amount (HK$)', 
                            Status, 
                            OrderDate AS 'Date' 
                        FROM orders 
                        ORDER BY OrderDate DESC";

                    using (MySqlDataAdapter da = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvLogs.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load sales activity logs:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion
    }
}