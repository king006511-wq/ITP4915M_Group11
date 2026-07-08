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
    public partial class StatisticalReportForm : BaseForm
    {
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // UI 元件
        private Label lblTotalSales, lblTotalInventoryValue, lblPendingOrders;
        private Button btnRefresh;
        private ComboBox cmbCity;
        private Label lblChartTitle;

        // 圖表畫布與數據暫存區
        private Panel pnlSalesChart;
        private List<Tuple<string, decimal>> salesChartData = new List<Tuple<string, decimal>>();

        public StatisticalReportForm()
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
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
            this.BackColor = ThemeManager.PrimaryBackground;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;

            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 75, BackColor = Color.White };
            pnlHeader.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240), 1), 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);

            Label lblTitle = new Label { Text = "📈 Executive Statistical Dashboard", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(25, 22), AutoSize = true };
            pnlHeader.Controls.Add(lblTitle);

            cmbCity = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new Size(160, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand
            };
            cmbCity.Items.AddRange(new string[] { "All Cities", "Hong Kong", "Tokyo", "Singapore", "New York", "London" });
            cmbCity.SelectedIndex = 0;
            cmbCity.Location = new Point(pnlHeader.Width - 310, 20);
            cmbCity.SelectedIndexChanged += (s, e) => LoadAllStatistics();
            pnlHeader.Controls.Add(cmbCity);

            btnRefresh = new Button { Text = "🔄 Refresh Data", Size = new Size(130, 36), Anchor = AnchorStyles.Top | AnchorStyles.Right, BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnRefresh.Location = new Point(pnlHeader.Width - 140, 20);
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadAllStatistics();
            pnlHeader.Controls.Add(btnRefresh);
            this.Controls.Add(pnlHeader);

            Panel pnlMainContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };
            this.Controls.Add(pnlMainContainer);
            pnlMainContainer.BringToFront();

            TableLayoutPanel tlpKPIs = new TableLayoutPanel { Dock = DockStyle.Top, Height = 115, ColumnCount = 3, RowCount = 1 };
            tlpKPIs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            tlpKPIs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            tlpKPIs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            pnlMainContainer.Controls.Add(tlpKPIs);

            Panel card1 = CreateKPICard("Total Revenue (Active Orders)", "HKD $0.00", Color.FromArgb(16, 185, 129), out lblTotalSales);
            // 🌟 標題已升級，包含 Raw Material 與 Finished Goods
            Panel card2 = CreateKPICard("Total Asset Value (Raw Mat. + Finished Goods)", "HKD $0.00", Color.FromArgb(37, 99, 235), out lblTotalInventoryValue);
            Panel card3 = CreateKPICard("Pending Sales Orders", "0 Orders", Color.FromArgb(245, 158, 11), out lblPendingOrders);

            card1.Margin = new Padding(0, 0, 12, 15);
            card2.Margin = new Padding(12, 0, 12, 15);
            card3.Margin = new Padding(12, 0, 0, 15);

            tlpKPIs.Controls.Add(card1, 0, 0);
            tlpKPIs.Controls.Add(card2, 1, 0);
            tlpKPIs.Controls.Add(card3, 2, 0);

            Panel pnlChartContainer = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 10, 0, 0), Padding = new Padding(20) };
            pnlChartContainer.Paint += (s, e) => DrawContainerBorder(e.Graphics, pnlChartContainer.ClientRectangle);

            lblChartTitle = new Label { Text = "📊 6-Month Sales Trend", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(51, 65, 85), Dock = DockStyle.Top, Height = 30 };
            pnlChartContainer.Controls.Add(lblChartTitle);

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
            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Dock = DockStyle.Top, AutoSize = true };
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

        #region 📈 真實 SQL 聚合引擎 (真正運算系統庫存與訂單狀態)
        private string GetWarehouseIDFromRegion(string region)
        {
            if (region == "Tokyo") return "W002";
            if (region == "Singapore") return "W003";
            if (region == "New York") return "W004";
            if (region == "London") return "W005";
            return "W001";
        }

        private string GetProductStockColumn(string region)
        {
            if (region == "Tokyo") return "Stock_Tokyo";
            if (region == "Singapore") return "Stock_Singapore";
            if (region == "New York") return "Stock_NY";
            if (region == "London") return "Stock_London";
            return "Stock_HK";
        }

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
                    string selectedCity = cmbCity.SelectedItem?.ToString() ?? "All Cities";

                    // 🌟 1. 總收益 KPI (根據訂單狀態中包含的 [City] 進行過濾，解決 Customer Region 唔準嘅問題)
                    string revQuery = "SELECT SUM(TotalAmount) FROM orders WHERE Status NOT LIKE '%Cancelled%' AND Status NOT LIKE '%Rejected%'";
                    if (cmbCity.SelectedIndex > 0) revQuery += " AND Status LIKE @CitySuffix";

                    using (MySqlCommand cmdRev = new MySqlCommand(revQuery, conn))
                    {
                        if (cmbCity.SelectedIndex > 0) cmdRev.Parameters.AddWithValue("@CitySuffix", "%[" + selectedCity + "]%");
                        object revRes = cmdRev.ExecuteScalar();
                        decimal totalRev = (revRes != DBNull.Value && revRes != null) ? Convert.ToDecimal(revRes) : 0;
                        lblTotalSales.Text = $"HKD {totalRev:N2}";
                    }

                    // 🌟 2. 真實資產總值 (原材料總值 + 製成品總值)
                    decimal totalAsset = 0;
                    string rmQuery = "SELECT SUM(i.StockLevel * r.StandardCost) FROM inventory i JOIN raw_material r ON i.MaterialID = r.MaterialID";
                    string prodQuery = "SELECT SUM((Stock_HK + Stock_Tokyo + Stock_Singapore + Stock_NY + Stock_London) * RetailPrice) FROM product";

                    if (cmbCity.SelectedIndex > 0)
                    {
                        rmQuery += " WHERE i.WarehouseID = @WH";
                        string col = GetProductStockColumn(selectedCity);
                        prodQuery = $"SELECT SUM({col} * RetailPrice) FROM product";
                    }

                    using (MySqlCommand cmdRM = new MySqlCommand(rmQuery, conn))
                    {
                        if (cmbCity.SelectedIndex > 0) cmdRM.Parameters.AddWithValue("@WH", GetWarehouseIDFromRegion(selectedCity));
                        object resRM = cmdRM.ExecuteScalar();
                        if (resRM != DBNull.Value && resRM != null) totalAsset += Convert.ToDecimal(resRM);
                    }

                    using (MySqlCommand cmdProd = new MySqlCommand(prodQuery, conn))
                    {
                        object resProd = cmdProd.ExecuteScalar();
                        if (resProd != DBNull.Value && resProd != null) totalAsset += Convert.ToDecimal(resProd);
                    }
                    lblTotalInventoryValue.Text = $"HKD {totalAsset:N2}";

                    // 🌟 3. 待處理訂單數 KPI
                    string pendingQuery = "SELECT COUNT(*) FROM orders WHERE Status LIKE '%Awaiting Approval%'";
                    if (cmbCity.SelectedIndex > 0) pendingQuery += " AND Status LIKE @CitySuffix";

                    using (MySqlCommand cmdPending = new MySqlCommand(pendingQuery, conn))
                    {
                        if (cmbCity.SelectedIndex > 0) cmdPending.Parameters.AddWithValue("@CitySuffix", "%[" + selectedCity + "]%");
                        lblPendingOrders.Text = $"{Convert.ToInt32(cmdPending.ExecuteScalar())} Orders";
                    }
                }
                catch (Exception ex) { MessageBox.Show("KPI Error: " + ex.Message); }
            }
        }

        private void LoadSalesChartData()
        {
            salesChartData.Clear();
            string selectedCity = cmbCity.SelectedItem?.ToString() ?? "All Cities";
            lblChartTitle.Text = $"📊 6-Month Sales Trend ({selectedCity})";

            // 🌟 核心升級：強制建立最近 6 個月的骨架，防止某個月冇生意導致斷層
            List<Tuple<string, int, int>> last6Months = new List<Tuple<string, int, int>>();
            for (int i = 5; i >= 0; i--)
            {
                DateTime d = DateTime.Now.AddMonths(-i);
                last6Months.Add(new Tuple<string, int, int>(d.ToString("MMM yyyy"), d.Year, d.Month));
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT YEAR(OrderDate) as Y, MONTH(OrderDate) as M, SUM(TotalAmount) AS MonthlyRevenue 
                        FROM orders 
                        WHERE Status NOT LIKE '%Cancelled%' AND Status NOT LIKE '%Rejected%' ";

                    if (cmbCity.SelectedIndex > 0) query += " AND Status LIKE @CitySuffix ";
                    query += " GROUP BY YEAR(OrderDate), MONTH(OrderDate)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        if (cmbCity.SelectedIndex > 0) cmd.Parameters.AddWithValue("@CitySuffix", "%[" + selectedCity + "]%");

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            // 映射數據入骨架
                            foreach (var month in last6Months)
                            {
                                decimal monthTotal = 0;
                                foreach (DataRow row in dt.Rows)
                                {
                                    int rowY = Convert.ToInt32(row["Y"]);
                                    int rowM = Convert.ToInt32(row["M"]);
                                    if (rowY == month.Item2 && rowM == month.Item3)
                                    {
                                        monthTotal = row["MonthlyRevenue"] != DBNull.Value ? Convert.ToDecimal(row["MonthlyRevenue"]) : 0m;
                                        break;
                                    }
                                }
                                salesChartData.Add(new Tuple<string, decimal>(month.Item1, monthTotal));
                            }
                        }
                    }
                    pnlSalesChart.Invalidate();
                }
                catch (Exception ex) { MessageBox.Show("Chart Data Error: " + ex.Message); }
            }
        }

        private void StatisticalReportForm_Load(object sender, EventArgs e) { }
        #endregion

        #region 🖌️ GDI+ 高級手繪圖表核心引擎 (Premium Native Line Chart Rendering)
        private void PnlSalesChart_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int width = pnlSalesChart.Width;
            int height = pnlSalesChart.Height;

            int paddingLeft = 60;
            int paddingRight = 45;
            int paddingTop = 35;
            int paddingBottom = 40;

            if (salesChartData == null || salesChartData.Count == 0) return;

            decimal maxVal = salesChartData.Max(d => d.Item2);
            if (maxVal == 0) maxVal = 1;

            int chartWidth = width - paddingLeft - paddingRight;
            int chartHeight = height - paddingTop - paddingBottom;

            // ─── Step 1: 繪製背景網格線 ───
            using (Pen gridPen = new Pen(Color.FromArgb(241, 245, 249), 1) { DashStyle = DashStyle.Dash })
            using (Font axisFont = new Font("Segoe UI", 8F))
            using (SolidBrush axisBrush = new SolidBrush(Color.FromArgb(148, 163, 184)))
            {
                for (int j = 0; j <= 4; j++)
                {
                    int gridY = height - paddingBottom - (int)(chartHeight * (j / 4.0));
                    g.DrawLine(gridPen, paddingLeft - 5, gridY, width - paddingRight, gridY);

                    decimal gridVal = maxVal * (j / 4.0m);
                    string yLabel = gridVal >= 1000 ? $"${gridVal / 1000:0.#}k" : $"${gridVal:0}";
                    g.DrawString(yLabel, axisFont, axisBrush, paddingLeft - g.MeasureString(yLabel, axisFont).Width - 8, gridY - 7);
                }
            }

            int numPoints = salesChartData.Count;
            PointF[] points = new PointF[numPoints];

            for (int i = 0; i < numPoints; i++)
            {
                decimal value = salesChartData[i].Item2;
                int nodeHeight = (int)((double)value / (double)maxVal * chartHeight);

                float x = (numPoints > 1)
                    ? paddingLeft + (i * ((float)chartWidth / (numPoints - 1)))
                    : paddingLeft + (chartWidth / 2f);

                float y = height - paddingBottom - nodeHeight;
                points[i] = new PointF(x, y);
            }

            // ─── Step 3: 繪製半透明下落陰影區域 (Area Shading) ───
            if (numPoints > 1)
            {
                using (GraphicsPath areaPath = new GraphicsPath())
                {
                    areaPath.AddLine(points[0].X, height - paddingBottom, points[0].X, points[0].Y);
                    for (int i = 1; i < numPoints; i++)
                    {
                        areaPath.AddLine(points[i - 1].X, points[i - 1].Y, points[i].X, points[i].Y);
                    }
                    areaPath.AddLine(points[numPoints - 1].X, points[numPoints - 1].Y, points[numPoints - 1].X, height - paddingBottom);
                    areaPath.CloseFigure();

                    using (LinearGradientBrush areaBrush = new LinearGradientBrush(
                        new RectangleF(paddingLeft, paddingTop, chartWidth, chartHeight),
                        Color.FromArgb(45, 37, 99, 235),
                        Color.FromArgb(0, 37, 99, 235),
                        LinearGradientMode.Vertical))
                    {
                        g.FillPath(areaBrush, areaPath);
                    }
                }
            }

            // ─── Step 4: 繪製主折線趨勢線 ───
            using (Pen linePen = new Pen(Color.FromArgb(37, 99, 235), 3))
            {
                linePen.LineJoin = LineJoin.Round;
                if (numPoints > 1) g.DrawLines(linePen, points);
            }

            // ─── Step 5: 繪製節點圓圈、金額與 X 軸時間標籤 ───
            for (int i = 0; i < numPoints; i++)
            {
                float x = points[i].X;
                float y = points[i].Y;
                string monthLabel = salesChartData[i].Item1;
                decimal value = salesChartData[i].Item2;

                int radius = 5;
                g.FillEllipse(Brushes.White, x - radius, y - radius, radius * 2, radius * 2);
                using (Pen circlePen = new Pen(Color.FromArgb(37, 99, 235), 2))
                {
                    g.DrawEllipse(circlePen, x - radius, y - radius, radius * 2, radius * 2);
                }

                using (Font valFont = new Font("Segoe UI", 8.5F, FontStyle.Bold))
                using (SolidBrush valBrush = new SolidBrush(Color.FromArgb(30, 41, 59)))
                {
                    string valStr = value >= 1000 ? $"${value / 1000:0.##}k" : $"${value:0}";
                    SizeF textSize = g.MeasureString(valStr, valFont);
                    g.DrawString(valStr, valFont, valBrush, x - (textSize.Width / 2), y - 18);
                }

                using (Font lblFont = new Font("Segoe UI", 8.5F, FontStyle.Regular))
                using (SolidBrush lblBrush = new SolidBrush(Color.FromArgb(100, 116, 139)))
                {
                    SizeF textSize = g.MeasureString(monthLabel, lblFont);
                    g.DrawString(monthLabel, lblFont, lblBrush, x - (textSize.Width / 2), height - paddingBottom + 10);
                }
            }

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