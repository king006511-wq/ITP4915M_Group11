using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class StatisticalReportForm : Form
    {
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // UI 元件
        private Label lblTotalSales, lblTotalInventoryValue, lblPendingOrders;
        private Button btnRefresh;

        // 圖表畫布與數據暫存區
        private Panel pnlSalesChart;
        private List<Tuple<string, decimal>> salesChartData = new List<Tuple<string, decimal>>();

        public StatisticalReportForm()
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                SetupPremiumDashboardUI();
                EnforceSecurityGatekeeper();
                LoadAllStatistics();
            }
        }

        #region 🛡️ System Security (權限攔截)
        private void EnforceSecurityGatekeeper()
        {
            string currentRole = UserSession.LoggedInStaffRole;
            bool isAuthorized = !string.IsNullOrEmpty(currentRole) &&
                                (currentRole.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                                 currentRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase) ||
                                 currentRole.Equals("System Manager", StringComparison.OrdinalIgnoreCase));

            if (!isAuthorized)
            {
                MessageBox.Show("[SECURITY ALERT] Access Denied!\n\nOnly Management level staff can view Financial and Statistical Reports.", "Security Enforcer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.BeginInvoke(new MethodInvoker(this.Close));
            }
        }
        #endregion

        #region 🎨 Modernized Responsive UI Setup
        private void SetupPremiumDashboardUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living - Executive Statistical Dashboard";
            this.BackColor = Color.FromArgb(248, 250, 252); // 採用現代輕奢 Slate 50 背景色
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;

            // ─── 1. HEADER PANEL ───
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 75, BackColor = Color.White };
            pnlHeader.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240), 1), 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);

            Label lblTitle = new Label { Text = "📈 Executive Statistical Dashboard", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(25, 22), AutoSize = true };
            pnlHeader.Controls.Add(lblTitle);

            btnRefresh = new Button { Text = "🔄 Refresh Data", Size = new Size(130, 36), Anchor = AnchorStyles.Top | AnchorStyles.Right, BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnRefresh.Location = new Point(pnlHeader.Width - 160, 20);
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadAllStatistics();
            pnlHeader.Controls.Add(btnRefresh);
            this.Controls.Add(pnlHeader);

            // ─── 主容器 (包含 KPI 與 下方圖表) ───
            Panel pnlMainContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };
            this.Controls.Add(pnlMainContainer);
            pnlMainContainer.BringToFront();

            // ─── 2. RESPONSIVE KPI CARDS ───
            TableLayoutPanel tlpKPIs = new TableLayoutPanel { Dock = DockStyle.Top, Height = 115, ColumnCount = 3, RowCount = 1 };
            tlpKPIs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            tlpKPIs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            tlpKPIs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            pnlMainContainer.Controls.Add(tlpKPIs);

            Panel card1 = CreateKPICard("Total Revenue (Active Orders)", "HKD $0.00", Color.FromArgb(16, 185, 129), out lblTotalSales);
            Panel card2 = CreateKPICard("Total Raw Material Asset Value", "HKD $0.00", Color.FromArgb(37, 99, 235), out lblTotalInventoryValue);
            Panel card3 = CreateKPICard("Pending Sales Orders", "0 Orders", Color.FromArgb(245, 158, 11), out lblPendingOrders);

            card1.Margin = new Padding(0, 0, 12, 15);
            card2.Margin = new Padding(12, 0, 12, 15);
            card3.Margin = new Padding(12, 0, 0, 15);

            tlpKPIs.Controls.Add(card1, 0, 0);
            tlpKPIs.Controls.Add(card2, 1, 0);
            tlpKPIs.Controls.Add(card3, 2, 0);

            // ─── 3. BOTTOM CONTENT (全闊度圖表) ───
            Panel pnlChartContainer = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 10, 0, 0), Padding = new Padding(20) };
            pnlChartContainer.Paint += (s, e) => DrawContainerBorder(e.Graphics, pnlChartContainer.ClientRectangle);

            Label lblChartTitle = new Label { Text = "📊 6-Month Sales Trend", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(51, 65, 85), Dock = DockStyle.Top, Height = 30 };
            pnlChartContainer.Controls.Add(lblChartTitle);

            // 核心：自適應畫布 Panel，填滿剩下的空間
            pnlSalesChart = new Panel { Dock = DockStyle.Fill };
            pnlSalesChart.Paint += PnlSalesChart_Paint;
            pnlChartContainer.Controls.Add(pnlSalesChart);
            pnlSalesChart.BringToFront();

            pnlMainContainer.Controls.Add(pnlChartContainer);
            pnlChartContainer.BringToFront();
        }

        private Panel CreateKPICard(string title, string defaultVal, Color themeColor, out Label valueLabel)
        {
            Panel card = new Panel { BackColor = Color.White, Dock = DockStyle.Fill, Padding = new Padding(15) };
            card.Paint += (s, e) => DrawContainerBorder(e.Graphics, card.ClientRectangle);

            Panel colorStrip = new Panel { BackColor = themeColor, Width = 5, Dock = DockStyle.Left };
            card.Controls.Add(colorStrip);

            Panel pnlText = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15, 5, 0, 0) };
            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Dock = DockStyle.Top, AutoSize = true };
            valueLabel = new Label { Text = defaultVal, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Dock = DockStyle.Bottom, AutoSize = true };

            pnlText.Controls.Add(lblTitle);
            pnlText.Controls.Add(valueLabel);
            card.Controls.Add(pnlText);
            pnlText.BringToFront();

            return card;
        }

        private void DrawContainerBorder(Graphics g, Rectangle rect)
        {
            using (Pen borderPen = new Pen(Color.FromArgb(226, 232, 240), 1))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawRectangle(borderPen, 0, 0, rect.Width - 1, rect.Height - 1);
            }
        }
        #endregion

        #region 📈 SQL Data Aggregation (數據聚合)
        private void LoadAllStatistics()
        {
            LoadKPIData();
            LoadSalesChartData();
        }

        private void LoadKPIData()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmdRev = new MySqlCommand("SELECT SUM(TotalAmount) FROM orders WHERE Status NOT IN ('Cancelled', 'Rejected')", conn);
                    object revRes = cmdRev.ExecuteScalar();
                    decimal totalRev = (revRes != DBNull.Value && revRes != null) ? Convert.ToDecimal(revRes) : 0;
                    lblTotalSales.Text = $"HKD {totalRev:N2}";

                    MySqlCommand cmdAsset = new MySqlCommand("SELECT SUM(StockLevel * StandardCost) FROM raw_material", conn);
                    object assetRes = cmdAsset.ExecuteScalar();
                    decimal totalAsset = (assetRes != DBNull.Value && assetRes != null) ? Convert.ToDecimal(assetRes) : 0;
                    lblTotalInventoryValue.Text = $"HKD {totalAsset:N2}";

                    MySqlCommand cmdPending = new MySqlCommand("SELECT COUNT(*) FROM orders WHERE Status LIKE 'Awaiting Approval%'", conn);
                    lblPendingOrders.Text = $"{Convert.ToInt32(cmdPending.ExecuteScalar())} Orders";
                }
                catch (Exception ex) { MessageBox.Show("KPI Error: " + ex.Message); }
            }
        }

        private void LoadSalesChartData()
        {
            salesChartData.Clear();
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT DATE_FORMAT(OrderDate, '%b %Y') AS MonthName, SUM(TotalAmount) AS MonthlyRevenue 
                        FROM orders 
                        WHERE Status NOT IN ('Cancelled', 'Rejected') 
                        GROUP BY YEAR(OrderDate), MONTH(OrderDate) 
                        ORDER BY YEAR(OrderDate) DESC, MONTH(OrderDate) DESC 
                        LIMIT 6";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = dt.Rows.Count - 1; i >= 0; i--)
                        {
                            DataRow row = dt.Rows[i];
                            salesChartData.Add(new Tuple<string, decimal>(row["MonthName"].ToString(), Convert.ToDecimal(row["MonthlyRevenue"])));
                        }
                    }
                    pnlSalesChart.Invalidate(); // 觸發重繪
                }
                catch (Exception ex) { MessageBox.Show("Chart Data Error: " + ex.Message); }
            }
        }

        private void StatisticalReportForm_Load(object sender, EventArgs e) { }
        #endregion

        #region 🖌️ GDI+ 高級手繪圖表核心引擎 (Premium Native Rendering)
        private void PnlSalesChart_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int width = pnlSalesChart.Width;
            int height = pnlSalesChart.Height;

            // 給予四周適當邊距留白
            int paddingLeft = 55;
            int paddingRight = 35;
            int paddingTop = 35;
            int paddingBottom = 40;

            if (salesChartData == null || salesChartData.Count == 0)
            {
                using (Font noDataFont = new Font("Segoe UI", 11F, FontStyle.Italic))
                {
                    string noDataText = "No sales data available";
                    SizeF size = g.MeasureString(noDataText, noDataFont);
                    g.DrawString(noDataText, noDataFont, Brushes.Gray, (width - size.Width) / 2, (height - size.Height) / 2);
                }
                return;
            }

            decimal maxVal = salesChartData.Max(d => d.Item2);
            if (maxVal == 0) maxVal = 1;

            int chartWidth = width - paddingLeft - paddingRight;
            int chartHeight = height - paddingTop - paddingBottom;

            // ─── Step 1: 繪製現代背景網格線 (4條橫向虛線) ───
            using (Pen gridPen = new Pen(Color.FromArgb(241, 245, 249), 1) { DashStyle = DashStyle.Dash })
            using (Font axisFont = new Font("Segoe UI", 8F))
            using (SolidBrush axisBrush = new SolidBrush(Color.FromArgb(148, 163, 184)))
            {
                for (int j = 0; j <= 4; j++)
                {
                    int gridY = height - paddingBottom - (int)(chartHeight * (j / 4.0));
                    g.DrawLine(gridPen, paddingLeft - 5, gridY, width - paddingRight, gridY);

                    // 繪製 Y 軸數值標籤 (例如 $20k)
                    decimal gridVal = maxVal * (j / 4.0m);
                    string yLabel = gridVal >= 1000 ? $"${gridVal / 1000:0.#}k" : $"${gridVal:0}";
                    g.DrawString(yLabel, axisFont, axisBrush, paddingLeft - g.MeasureString(yLabel, axisFont).Width - 8, gridY - 7);
                }
            }

            // ─── Step 2: 計算柱體尺寸 (自動按比例伸展) ───
            int spacing = chartWidth / (salesChartData.Count * 2); // 根據畫面闊度自動調整間距
            if (spacing > 60) spacing = 60; // 限制最大間距，避免太散

            int barWidth = (chartWidth - (spacing * (salesChartData.Count - 1))) / salesChartData.Count;
            if (barWidth > 120) barWidth = 120; // 限制最大柱體闊度，避免太過粗壯
            if (barWidth < 12) barWidth = 12;

            // 計算整體偏移，令圖表置中
            int totalChartContentWidth = (salesChartData.Count * barWidth) + ((salesChartData.Count - 1) * spacing);
            int startX = paddingLeft + ((chartWidth - totalChartContentWidth) / 2);

            // ─── Step 3: 循環繪製高品質柱狀圖 ───
            for (int i = 0; i < salesChartData.Count; i++)
            {
                string monthLabel = salesChartData[i].Item1;
                decimal value = salesChartData[i].Item2;

                int barHeight = (int)((double)value / (double)maxVal * chartHeight);
                if (barHeight < 3) barHeight = 3; // 確保微小數值依然隱約可見

                int x = startX + (i * (barWidth + spacing));
                int y = height - paddingBottom - barHeight;

                // 使用美觀的藍青漸變色 (Gradients)
                Rectangle barRect = new Rectangle(x, y, barWidth, barHeight);
                using (LinearGradientBrush gradientBrush = new LinearGradientBrush(barRect, Color.FromArgb(37, 99, 235), Color.FromArgb(96, 165, 250), LinearGradientMode.Vertical))
                {
                    // 使用 GraphicsPath 為柱體頂部加上圓角
                    int cornerRadius = Math.Min(8, barWidth / 2);
                    using (GraphicsPath postPath = new GraphicsPath())
                    {
                        postPath.AddArc(x, y, cornerRadius * 2, cornerRadius * 2, 180, 90);
                        postPath.AddArc(x + barWidth - (cornerRadius * 2), y, cornerRadius * 2, cornerRadius * 2, 270, 90);
                        postPath.AddLine(x + barWidth, y + cornerRadius, x + barWidth, y + barHeight);
                        postPath.AddLine(x + barWidth, y + barHeight, x, y + barHeight);
                        postPath.CloseFigure();

                        g.FillPath(gradientBrush, postPath);
                    }
                }

                // ─── Step 4: 繪製柱頂金額數字 ───
                using (Font valFont = new Font("Segoe UI", 8.5F, FontStyle.Bold))
                using (SolidBrush valBrush = new SolidBrush(Color.FromArgb(30, 41, 59)))
                {
                    string valStr = value >= 1000 ? $"${value / 1000:0.##}k" : $"${value:0}";
                    SizeF textSize = g.MeasureString(valStr, valFont);
                    g.DrawString(valStr, valFont, valBrush, x + (barWidth / 2) - (textSize.Width / 2), y - 18);
                }

                // ─── Step 5: 繪製 X 軸月份標籤 ───
                using (Font lblFont = new Font("Segoe UI", 8.5F, FontStyle.Regular))
                using (SolidBrush lblBrush = new SolidBrush(Color.FromArgb(100, 116, 139)))
                {
                    SizeF textSize = g.MeasureString(monthLabel, lblFont);
                    g.DrawString(monthLabel, lblFont, lblBrush, x + (barWidth / 2) - (textSize.Width / 2), height - paddingBottom + 10);
                }
            }

            // 繪製 X 軸基準線
            using (Pen axisLinePen = new Pen(Color.FromArgb(226, 232, 240), 1.5f))
            {
                g.DrawLine(axisLinePen, paddingLeft - 5, height - paddingBottom, width - paddingRight, height - paddingBottom);
            }
        }
        #endregion

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(950, 600);
            this.Name = "StatisticalReportForm";
            this.Load += new System.EventHandler(this.StatisticalReportForm_Load);
            this.ResumeLayout(false);
        }
    }
}