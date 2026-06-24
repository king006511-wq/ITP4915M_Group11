using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class RawMaterialRequestForm : Form
    {
        // ==========================================
        // 🔒 Database Configuration
        // ==========================================
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // ==========================================
        // 🎨 獨立防撞名 UI 變數 (加咗 custom_ 避免同 Designer 撞)
        // ==========================================
        private TextBox custom_txtCardID, custom_txtMaterialID, custom_txtQty;
        private DataGridView custom_dgvRequests;
        private Button custom_btnSubmit, custom_btnClear;

        // 核心佈局控制元件
        private Panel custom_pnlLeftCard;
        private Label custom_lblGridTitle;

        public RawMaterialRequestForm()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                SetupCustomSleekUI(); // 🚀 啟動防撞名精緻排版 (收身版)
                GenerateNewCardID();
                LoadRequests();

                this.Load += RawMaterialRequestForm_Load;

                // 🌟 核心防禦：綁定視窗縮放事件，強行用數學公式完美控制畫面
                this.SizeChanged += RawMaterialRequestForm_SizeChanged;
                this.Layout += (s, e) => RecalculateDynamicLayout();
            }
        }

        #region 🔒 權限驗證
        private void RawMaterialRequestForm_Load(object sender, EventArgs e)
        {
            string currentRole = UserSession.LoggedInStaffRole;
            bool isAuthorized = !string.IsNullOrEmpty(currentRole) &&
                                (currentRole.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                                 currentRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase) ||
                                 currentRole.Equals("Procurement Officer", StringComparison.OrdinalIgnoreCase));

            if (!isAuthorized)
            {
                MessageBox.Show("[SECURITY ALERT] Access Denied!\n\nOnly Procurement Officers and Management can submit material replenishment requests.", "System Security Enforcer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.BeginInvoke(new MethodInvoker(this.Close));
            }
        }
        #endregion

        #region 🎨 精緻手動算繪排版 (告別臃腫，變回精準現代商務風)
        private void SetupCustomSleekUI()
        {
            this.Controls.Clear();
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular); // 全局字體調整為標準大細
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopLevel = false;
            this.Dock = DockStyle.Fill;

            // =========================================================
            // 【左側】物料請求輸入卡片 (寬度由 420 縮窄至 400)
            // =========================================================
            custom_pnlLeftCard = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };
            custom_pnlLeftCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, custom_pnlLeftCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            this.Controls.Add(custom_pnlLeftCard);

            Label lblCardTitle = new Label { Text = "📋 Material Replenishment", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(79, 70, 229), Location = new Point(22, 18), AutoSize = true };
            custom_pnlLeftCard.Controls.Add(lblCardTitle);

            int startY = 65;
            int inputWidth = 350; // 輸入框微調

            custom_txtCardID = CreateCustomTextBox(custom_pnlLeftCard, ref startY, "Reorder Card ID (Auto):", true, inputWidth);
            custom_txtMaterialID = CreateCustomTextBox(custom_pnlLeftCard, ref startY, "Raw Material ID *:", false, inputWidth);
            custom_txtQty = CreateCustomTextBox(custom_pnlLeftCard, ref startY, "Requested Quantity *:", false, inputWidth);

            int btnWidth = 170;
            custom_btnSubmit = new Button { Text = "📤 Dispatch Request", Location = new Point(22, startY + 10), Size = new Size(btnWidth, 42), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand };
            custom_btnClear = new Button { Text = "🔄 Clear Form", Location = new Point(202, startY + 10), Size = new Size(btnWidth, 42), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand };
            custom_btnSubmit.FlatAppearance.BorderSize = 0; custom_btnClear.FlatAppearance.BorderSize = 0;

            custom_btnSubmit.Click += btnSubmit_Click;
            custom_btnClear.Click += (s, e) => ClearCustomFields();

            custom_pnlLeftCard.Controls.Add(custom_btnSubmit); custom_pnlLeftCard.Controls.Add(custom_btnClear);

            // =========================================================
            // 【右側】數據表格與標題 (精緻商務化)
            // =========================================================
            custom_lblGridTitle = new Label { Text = "📑 Ongoing Reorder Requests", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true };
            this.Controls.Add(custom_lblGridTitle);

            custom_dgvRequests = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                ScrollBars = ScrollBars.Both,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                EnableHeadersVisualStyles = false
            };

            // 📉 縮減大細：打造俐落舒適嘅閱讀感 (行高由 55px 修正為 36px)
            custom_dgvRequests.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            custom_dgvRequests.DefaultCellStyle.Padding = new Padding(8);
            custom_dgvRequests.DefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            custom_dgvRequests.RowTemplate.Height = 36;

            custom_dgvRequests.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            custom_dgvRequests.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            custom_dgvRequests.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            custom_dgvRequests.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            custom_dgvRequests.ColumnHeadersDefaultCellStyle.Padding = new Padding(8);
            custom_dgvRequests.ColumnHeadersHeight = 42;

            custom_dgvRequests.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            custom_dgvRequests.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);

            custom_dgvRequests.SelectionChanged += dgvRequests_SelectionChanged;
            this.Controls.Add(custom_dgvRequests);
        }

        private TextBox CreateCustomTextBox(Panel container, ref int topY, string labelText, bool readOnly, int width)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(22, topY), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(22, topY + 24), Width = width, Font = new Font("Segoe UI", 11F), BorderStyle = BorderStyle.FixedSingle };
            if (readOnly) { txt.ReadOnly = true; txt.BackColor = Color.FromArgb(241, 245, 249); }
            container.Controls.Add(lbl); container.Controls.Add(txt);
            topY += 75; // 元件間距由 85 縮窄到 75，介面更緊湊
            return txt;
        }

        private void RawMaterialRequestForm_SizeChanged(object sender, EventArgs e)
        {
            RecalculateDynamicLayout();
        }

        /// <summary>
        /// 🛠️ 鋼鐵動態佈局：不依賴 Dock，完全用像素公式精確定位
        /// </summary>
        private void RecalculateDynamicLayout()
        {
            if (this.Width < 200 || this.Height < 200) return;

            this.SuspendLayout();

            // 1. 指定左側卡片位置 (固定 400px 闊度)
            custom_pnlLeftCard.Location = new Point(20, 20);
            custom_pnlLeftCard.Size = new Size(400, this.Height - 40);

            // 2. 計算右側對齊錨點
            int rightStartX = custom_pnlLeftCard.Right + 20;
            int rightWidth = this.Width - rightStartX - 20;

            if (rightWidth > 100)
            {
                // 3. 固定標題
                custom_lblGridTitle.Location = new Point(rightStartX, 20);

                // 4. 右側表格動態拉大填滿，精準漂亮
                custom_dgvRequests.Location = new Point(rightStartX, 55);
                custom_dgvRequests.Size = new Size(rightWidth, this.Height - 75);

                // 5. 自動平分欄位
                if (custom_dgvRequests.Columns.Count > 0)
                {
                    custom_dgvRequests.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                    custom_dgvRequests.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }

            this.ResumeLayout(false);
        }
        #endregion

        #region 💾 核心資料庫連線邏輯
        private void GenerateNewCardID()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT ReOrderCardID FROM reorder_card ORDER BY ReOrderCardID DESC LIMIT 1";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            string lastID = result.ToString();
                            if (lastID.StartsWith("RC") && int.TryParse(lastID.Substring(2), out int num))
                            {
                                custom_txtCardID.Text = "RC" + (num + 1).ToString("D3");
                                return;
                            }
                        }
                        custom_txtCardID.Text = "RC001";
                    }
                }
                catch (Exception)
                {
                    custom_txtCardID.Text = "RC" + DateTime.Now.ToString("MMddHHmmss");
                }
            }
        }

        private void LoadRequests()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            r.ReOrderCardID AS 'Request ID', 
                            r.MaterialID AS 'Material ID', 
                            m.MaterialName AS 'Material Name',
                            r.RequestedQty AS 'Qty', 
                            r.Status AS 'Status', 
                            r.TriggerDate AS 'Date' 
                        FROM reorder_card r
                        LEFT JOIN raw_material m ON r.MaterialID = m.MaterialID
                        ORDER BY r.TriggerDate DESC";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        custom_dgvRequests.DataSource = null;
                        custom_dgvRequests.DataSource = dt;

                        // 設定最小防禦欄寬，防止內容擠壓
                        foreach (DataGridViewColumn col in custom_dgvRequests.Columns)
                        {
                            col.MinimumWidth = 100;
                        }

                        if (custom_dgvRequests.Columns.Contains("Request ID")) custom_dgvRequests.Columns["Request ID"].MinimumWidth = 120;
                        if (custom_dgvRequests.Columns.Contains("Material ID")) custom_dgvRequests.Columns["Material ID"].MinimumWidth = 120;
                        if (custom_dgvRequests.Columns.Contains("Material Name")) custom_dgvRequests.Columns["Material Name"].MinimumWidth = 180;
                        if (custom_dgvRequests.Columns.Contains("Date"))
                        {
                            custom_dgvRequests.Columns["Date"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
                            custom_dgvRequests.Columns["Date"].MinimumWidth = 150;
                        }

                        RecalculateDynamicLayout();
                        custom_dgvRequests.ClearSelection();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load requests data:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string cardID = custom_txtCardID.Text.Trim();
            string materialID = custom_txtMaterialID.Text.Trim();
            string qtyStr = custom_txtQty.Text.Trim();

            if (string.IsNullOrWhiteSpace(materialID) || string.IsNullOrWhiteSpace(qtyStr))
            {
                MessageBox.Show("Please fill in Raw Material ID and Quantity.", "Validation Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(qtyStr, out int qty) || qty <= 0)
            {
                MessageBox.Show("Quantity must be a valid positive integer.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string checkSql = "SELECT COUNT(*) FROM raw_material WHERE MaterialID = @matID";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@matID", materialID);
                        int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (exists == 0)
                        {
                            MessageBox.Show($"Raw Material ID '{materialID}' does not exist in the master inventory database. Please verify.", "Invalid Material", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    string query = "INSERT INTO reorder_card (ReOrderCardID, MaterialID, TriggerDate, RequestedQty, Status) VALUES (@cID, @matID, NOW(), @qty, 'Pending')";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@cID", cardID);
                        cmd.Parameters.AddWithValue("@matID", materialID);
                        cmd.Parameters.AddWithValue("@qty", qty);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Raw Material replenishment request successfully dispatched!", "Request Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearCustomFields();
                    LoadRequests();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Submission failed. Please verify the database connection.\n\nError: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvRequests_SelectionChanged(object sender, EventArgs e)
        {
            if (custom_dgvRequests.SelectedRows.Count > 0)
            {
                DataGridViewRow row = custom_dgvRequests.SelectedRows[0];
                custom_txtCardID.Text = row.Cells["Request ID"].Value?.ToString() ?? "";
                custom_txtMaterialID.Text = row.Cells["Material ID"].Value?.ToString() ?? "";
                custom_txtQty.Text = row.Cells["Qty"].Value?.ToString() ?? "";

                custom_btnSubmit.Enabled = false;
                custom_btnSubmit.BackColor = Color.LightGray;
            }
        }

        private void ClearCustomFields()
        {
            custom_txtMaterialID.Clear();
            custom_txtQty.Clear();
            custom_dgvRequests.ClearSelection();
            GenerateNewCardID();

            custom_btnSubmit.Enabled = true;
            custom_btnSubmit.BackColor = Color.FromArgb(16, 185, 129);
        }
        #endregion
    }
}