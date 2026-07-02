using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public class MaterialItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public decimal StandardCost { get; set; }
    }

    public partial class RawMaterialRequestForm : BaseForm
    {
        // ==========================================
        // 🔒 Database Configuration & Configuration
        // ==========================================
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";
        private string currentStaffID;
        private decimal currentUnitCost = 0;
        private decimal globalTotal = 0;

        private TextBox txtRequestID, txtUnitPrice, txtQty;
        private ComboBox cboMaterials;
        private DataGridView dgvHistory, dgvDetails, dgvCart;
        private Button btnAddItem, btnRemoveItem, btnSubmitRequest, btnClear;
        private Label lblTotalAmountDisplay;
        private DataTable cartTable;

        // 🌟 新增：用於搜尋的 TextBox
        private TextBox txtSearchReq;

        public RawMaterialRequestForm() : this(UserSession.LoggedInStaffID ?? "S001") { }

        public RawMaterialRequestForm(string loggedInStaffID)
        {
            this.currentStaffID = loggedInStaffID;
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                InitializePremiumModernUI();
                EnforceSecurityGatekeeper(); // 🛡️ 執行安全權限檢查
            }
        }

        private void RawMaterialRequestForm_Load(object sender, EventArgs e)
        {
            GenerateRequestBatchID();
            LoadMaterialsToCombo();
            RefreshHistoryGrid();
        }

        #region 🔒 System Security Gatekeeper Enforcement
        private void EnforceSecurityGatekeeper()
        {
            // 使用集中化的授權輔助工具來決定是否允許開啟此表單
            // 這樣可確保角色清單（例如 Warehouse Specialist）由 AuthorizationHelper 統一管理
            bool isAuthorized = AuthorizationHelper.HasMenuPermission("MATERIAL_REQUESTS");

            if (!isAuthorized)
            {
                var currentRole = string.IsNullOrEmpty(UserSession.LoggedInStaffRole) ? "Guest" : UserSession.LoggedInStaffRole;
                MessageBox.Show(
                    $"[SECURITY ALERT] Access Denied!\n\nYour account role \"{currentRole}\" is not authorized to create Material Reorder Cards.",
                    "System Security Guard",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop
                );

                this.Shown += (s, e) => this.Close();
            }
        }
        #endregion

        #region 🎨 Premium Modern UI Setup
        private void InitializePremiumModernUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Raw Material Purchase Request";
            this.BackColor = Color.FromArgb(241, 245, 249);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Load += RawMaterialRequestForm_Load;

            TableLayoutPanel mainTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent };
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.Controls.Add(mainTable);

            // --- Header ---
            Panel pnlHeader = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            Label lblHeader = new Label { Text = "Internal Material Request", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(20, 20), AutoSize = true };
            pnlHeader.Controls.Add(lblHeader);
            Label lblStaff = new Label { Text = $"👤 Active Staff ID: {currentStaffID}", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(13, 148, 136), Location = new Point(480, 26), AutoSize = true };
            pnlHeader.Controls.Add(lblStaff);
            mainTable.Controls.Add(pnlHeader, 0, 0);

            TableLayoutPanel contentTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = new Padding(20, 0, 20, 20) };
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 510F));
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainTable.Controls.Add(contentTable, 0, 1);

            // --- Left Card ---
            Panel pnlCard = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, AutoScroll = true };
            pnlCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            contentTable.Controls.Add(pnlCard, 0, 0);

            // Right Table Master-Detail
            TableLayoutPanel rightTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Margin = new Padding(0) };
            rightTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F)); // 將 Header 稍微加高容納 Search Box
            rightTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            rightTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            rightTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            // 🌟 修改：右側上半部 Header (加入 Search Box)
            Panel pnlHistoryHeader = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            Label lblGridTitle = new Label { Text = "📊 Request Headers", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true, Location = new Point(0, 12) };
            pnlHistoryHeader.Controls.Add(lblGridTitle);

            Label lblSearch = new Label { Text = "🔍 Search:", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), AutoSize = true, Location = new Point(220, 15) };

            // 🌟 修正：向右移至 X=330 避免重疊
            txtSearchReq = new TextBox { Location = new Point(330, 12), Width = 180, Font = new Font("Segoe UI", 10F), BorderStyle = BorderStyle.FixedSingle };
            txtSearchReq.TextChanged += TxtSearchReq_TextChanged; // 綁定搜尋事件
            pnlHistoryHeader.Controls.Add(lblSearch);
            pnlHistoryHeader.Controls.Add(txtSearchReq);
            rightTable.Controls.Add(pnlHistoryHeader, 0, 0);

            dgvHistory = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, GridColor = Color.FromArgb(241, 245, 249) };
            dgvHistory.EnableHeadersVisualStyles = false;
            dgvHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvHistory.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold); // 標題粗體
            dgvHistory.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular); // 🌟 修正：資料正常字體
            dgvHistory.ColumnHeadersHeight = 35;
            dgvHistory.CellFormatting += dgvHistory_CellFormatting;
            dgvHistory.SelectionChanged += DgvHistory_SelectionChanged;
            rightTable.Controls.Add(dgvHistory, 0, 1);

            Label lblDetailsTitle = new Label { Text = "🔍 Request Line Items", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true, Dock = DockStyle.Bottom };
            rightTable.Controls.Add(lblDetailsTitle, 0, 2);

            dgvDetails = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, GridColor = Color.FromArgb(241, 245, 249) };
            dgvDetails.EnableHeadersVisualStyles = false;
            dgvDetails.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(71, 85, 105);
            dgvDetails.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvDetails.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold); // 標題粗體
            dgvDetails.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular); // 🌟 修正：資料正常字體
            dgvDetails.ColumnHeadersHeight = 35;
            rightTable.Controls.Add(dgvDetails, 0, 3);

            contentTable.Controls.Add(rightTable, 2, 0);

            // --- Builder Fields ---
            Label lblCardTitle = new Label { Text = "📝 Request Builder", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(20, 15), AutoSize = true };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 50;
            txtRequestID = CreateStyledTextBox(pnlCard, ref startY, "Request ID (Auto):", true, 450, 20);

            Label lblMat = new Label { Text = "Select Material *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboMaterials = new ComboBox { Location = new Point(20, startY + 25), Width = 450, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F), BackColor = Color.White };
            cboMaterials.SelectedIndexChanged += cboMaterials_SelectedIndexChanged;
            pnlCard.Controls.Add(lblMat);
            pnlCard.Controls.Add(cboMaterials);
            startY += 65;

            int tempY = startY;
            txtUnitPrice = CreateStyledTextBox(pnlCard, ref tempY, "Ref. Unit Cost ($):", true, 210, 20);

            tempY = startY;
            txtQty = CreateStyledTextBox(pnlCard, ref tempY, "Quantity *:", false, 210, 260);
            startY = tempY;

            btnAddItem = new Button { Text = "➕ Add Item", Location = new Point(20, startY), Size = new Size(210, 35), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAddItem.Click += BtnAddItem_Click;
            pnlCard.Controls.Add(btnAddItem);

            btnRemoveItem = new Button { Text = "❌ Remove", Location = new Point(260, startY), Size = new Size(210, 35), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnRemoveItem.Click += BtnRemoveItem_Click;
            pnlCard.Controls.Add(btnRemoveItem);
            startY += 60;

            Label lblCartGridTitle = new Label { Text = "📦 Staging Cart", Location = new Point(20, startY), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true };
            pnlCard.Controls.Add(lblCartGridTitle);
            startY += 30;

            cartTable = new DataTable();
            cartTable.Columns.Add("MaterialID", typeof(string));
            cartTable.Columns.Add("Material", typeof(string));
            cartTable.Columns.Add("Qty", typeof(int));
            cartTable.Columns.Add("Unit Price", typeof(decimal));
            cartTable.Columns.Add("Subtotal", typeof(decimal));

            dgvCart = new DataGridView { Location = new Point(20, startY), Size = new Size(450, 160), DataSource = cartTable, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvCart.EnableHeadersVisualStyles = false;
            dgvCart.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(71, 85, 105);
            dgvCart.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCart.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold); // 標題粗體
            dgvCart.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular); // 🌟 修正：資料正常字體
            pnlCard.Controls.Add(dgvCart);
            startY += 175;

            lblTotalAmountDisplay = new Label { Text = "Total Est. Cost: $0.00", Location = new Point(20, startY), Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(220, 38, 38), AutoSize = true };
            pnlCard.Controls.Add(lblTotalAmountDisplay);
            startY += 40;

            btnSubmitRequest = new Button { Text = "🚀 Submit Request", Location = new Point(20, startY), Size = new Size(210, 42), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSubmitRequest.Click += BtnSubmitRequest_Click;
            pnlCard.Controls.Add(btnSubmitRequest);

            btnClear = new Button { Text = "🧹 Clear Everything", Location = new Point(260, startY), Size = new Size(210, 42), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClear.Click += (s, e) => ClearFields();
            pnlCard.Controls.Add(btnClear);
        }

        private TextBox CreateStyledTextBox(Panel container, ref int topY, string labelText, bool readOnly, int width, int offsetX = 20)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(offsetX, topY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(offsetX, topY + 25), Width = width, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            if (readOnly) { txt.ReadOnly = true; txt.BackColor = Color.FromArgb(241, 245, 249); }
            container.Controls.Add(lbl);
            container.Controls.Add(txt);
            topY += 65;
            return txt;
        }

        private void dgvHistory_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvHistory.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status == "Pending Approval" || status == "Pending")
                {
                    e.CellStyle.ForeColor = Color.DarkOrange;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
                else if (status == "Approved")
                {
                    e.CellStyle.ForeColor = Color.MediumSeaGreen;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
                else if (status == "Rejected" || status == "Cancelled")
                {
                    e.CellStyle.ForeColor = Color.Crimson;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Strikeout | FontStyle.Bold);
                }
            }
        }
        #endregion

        #region 🔄 Dynamic Data Loading
        private void LoadMaterialsToCombo()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MaterialID, MaterialName, StandardCost FROM raw_material";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<MaterialItem> list = new List<MaterialItem>();
                        while (reader.Read())
                        {
                            list.Add(new MaterialItem
                            {
                                ID = reader["MaterialID"].ToString(),
                                Name = reader["MaterialName"].ToString(),
                                StandardCost = reader["StandardCost"] != DBNull.Value ? Convert.ToDecimal(reader["StandardCost"]) : 0
                            });
                        }
                        cboMaterials.DataSource = list;
                        cboMaterials.DisplayMember = "Name";
                        cboMaterials.ValueMember = "ID";
                        cboMaterials.SelectedIndex = -1;
                    }
                }
                catch (Exception ex) { MessageBox.Show("Failed to load Materials: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void cboMaterials_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboMaterials.SelectedItem != null)
            {
                MaterialItem mat = (MaterialItem)cboMaterials.SelectedItem;
                currentUnitCost = mat.StandardCost;
                txtUnitPrice.Text = currentUnitCost.ToString("F2");
            }
        }

        // 🌟 新增：搜尋過濾器邏輯
        private void TxtSearchReq_TextChanged(object sender, EventArgs e)
        {
            if (dgvHistory.DataSource is DataTable dt)
            {
                string keyword = txtSearchReq.Text.Trim().Replace("'", "''");
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    dt.DefaultView.RowFilter = "";
                }
                else
                {
                    // 根據載入時的欄位名稱 (Request ID, Status) 進行實時搜尋
                    dt.DefaultView.RowFilter = $"[Request ID] LIKE '%{keyword}%' OR Status LIKE '%{keyword}%'";
                }
            }
        }
        #endregion

        #region ➕ Add/Remove Cart Items
        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            if (cboMaterials.SelectedItem == null)
            {
                MessageBox.Show("Please select a Material.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtQty.Text.Trim(), out int qty) || qty <= 0)
            {
                MessageBox.Show("Please enter a valid positive quantity.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MaterialItem mat = (MaterialItem)cboMaterials.SelectedItem;
            decimal subtotal = currentUnitCost * qty;

            bool itemExists = false;
            foreach (DataRow row in cartTable.Rows)
            {
                if (row["MaterialID"].ToString() == mat.ID)
                {
                    row["Qty"] = Convert.ToInt32(row["Qty"]) + qty;
                    row["Subtotal"] = Convert.ToDecimal(row["Qty"]) * currentUnitCost;
                    itemExists = true;
                    break;
                }
            }

            if (!itemExists)
            {
                cartTable.Rows.Add(mat.ID, mat.Name, qty, currentUnitCost, subtotal);
            }

            UpdateGlobalTotal();
            txtQty.Clear();
            cboMaterials.SelectedIndex = -1;
            txtUnitPrice.Clear();
            currentUnitCost = 0;
        }

        private void BtnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvCart.SelectedRows)
                {
                    if (!row.IsNewRow) dgvCart.Rows.Remove(row);
                }
                UpdateGlobalTotal();
            }
        }

        private void UpdateGlobalTotal()
        {
            globalTotal = 0;
            foreach (DataRow row in cartTable.Rows)
            {
                globalTotal += Convert.ToDecimal(row["Subtotal"]);
            }
            lblTotalAmountDisplay.Text = $"Total Est. Cost: ${globalTotal:N2}";
        }
        #endregion

        #region 🚀 Submit Transaction (One-To-Many)
        private void BtnSubmitRequest_Click(object sender, EventArgs e)
        {
            if (cartTable.Rows.Count == 0)
            {
                MessageBox.Show("Staging Cart is empty! Please add materials first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            string requestID = txtRequestID.Text.Trim();

                            string query = @"INSERT INTO reorder_card 
                                             (ReOrderCardID, MaterialID, RequestedQty, Status, TriggerDate) 
                                             VALUES (@rcID, @matID, @qty, 'Pending Approval', NOW())";

                            using (MySqlCommand cmd = new MySqlCommand(query, conn, trans))
                            {
                                foreach (DataRow row in cartTable.Rows)
                                {
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("@rcID", requestID);
                                    cmd.Parameters.AddWithValue("@matID", row["MaterialID"]);
                                    cmd.Parameters.AddWithValue("@qty", row["Qty"]);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            trans.Commit();
                            MessageBox.Show($"Successfully submitted {cartTable.Rows.Count} material items under Request [{requestID}]!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            ClearFields();
                            RefreshHistoryGrid();
                        }
                        catch (Exception)
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Submission failed:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region 🔄 Refresh & Master-Detail Sync
        private void RefreshHistoryGrid()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT ReOrderCardID AS 'Request ID', 
                                            COUNT(MaterialID) AS 'Items Included', 
                                            SUM(RequestedQty) AS 'Total Qty', 
                                            Status, 
                                            MAX(TriggerDate) AS 'Date' 
                                     FROM reorder_card 
                                     GROUP BY ReOrderCardID, Status 
                                     ORDER BY Date DESC LIMIT 20";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvHistory.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    DataTable errDt = new DataTable();
                    errDt.Columns.Add("System Error");
                    errDt.Rows.Add("Failed to refresh: " + ex.Message);
                    dgvHistory.DataSource = errDt;
                }
            }
        }

        private void DgvHistory_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvHistory.SelectedRows.Count > 0 && dgvHistory.SelectedRows[0].Cells["Request ID"].Value != null)
            {
                string reqID = dgvHistory.SelectedRows[0].Cells["Request ID"].Value.ToString();

                if (reqID.StartsWith("Failed to refresh")) { dgvDetails.DataSource = null; return; }

                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        string query = @"SELECT r.MaterialID AS 'Material ID', m.MaterialName AS 'Material Name', r.RequestedQty AS 'Qty' 
                                         FROM reorder_card r 
                                         JOIN raw_material m ON r.MaterialID = m.MaterialID 
                                         WHERE r.ReOrderCardID = @id";

                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", reqID);
                            using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                            {
                                DataTable dt = new DataTable();
                                adapter.Fill(dt);
                                dgvDetails.DataSource = dt;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DataTable errDt = new DataTable();
                        errDt.Columns.Add("Line Items Error");
                        errDt.Rows.Add(ex.Message);
                        dgvDetails.DataSource = errDt;
                    }
                }
            }
            else
            {
                dgvDetails.DataSource = null;
            }
        }

        private void GenerateRequestBatchID()
        {
            txtRequestID.Text = "RQ" + DateTime.Now.ToString("yyMMddHHmmss");
        }

        private void ClearFields()
        {
            cartTable.Clear();
            UpdateGlobalTotal();
            if (txtSearchReq != null) txtSearchReq.Clear(); // 🌟 清除搜尋框
            cboMaterials.SelectedIndex = -1;
            txtQty.Clear();
            txtUnitPrice.Clear();
            GenerateRequestBatchID();
            dgvHistory.ClearSelection();
        }
        #endregion
    }
}