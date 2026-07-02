using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public class POSupplierItem { public string ID { get; set; } public string Name { get; set; } }

    public partial class ProcurementForm : BaseForm
    {
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";
        private TextBox custom_txtRCID, custom_txtMaterialID, custom_txtMaterialName, custom_txtQty, custom_txtPrice, custom_txtTotalCost, custom_txtStaffID;
        private ComboBox custom_cmbSupplier;
        private DataGridView custom_dgvPendingRC, custom_dgvDetails, custom_dgvPOCart, dgvPOList;
        private TextBox txtPOSearch;
        private DataTable poCartTable;

        // 🌟 補回缺失的按鈕欄位宣告
        private Button custom_btnAddAllocation, custom_btnRemoveAllocation, custom_btnCreatePO, custom_btnReject, custom_btnClear;

        public ProcurementForm()
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                SetupCustomSleekUI();
                LoadPendingRequests();
                LoadPOHistory();
                this.Load += ProcurementForm_Load;
            }
        }

        private void ProcurementForm_Load(object sender, EventArgs e)
        {
            custom_txtStaffID.Text = UserSession.LoggedInStaffID ?? "S001";
            string role = UserSession.LoggedInStaffRole;
            bool authorized = !string.IsNullOrEmpty(role) && (role.Equals("Manager", StringComparison.OrdinalIgnoreCase) || role.Equals("Administrator", StringComparison.OrdinalIgnoreCase) || role.Equals("Procurement Officer", StringComparison.OrdinalIgnoreCase));
            if (!authorized)
            {
                MessageBox.Show("[SECURITY ALERT] Access Denied!\n\nOnly Procurement Officers and Management can issue Purchase Orders.", "System Security Enforcer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.BeginInvoke(new MethodInvoker(this.Close));
            }
        }

        private void SetupCustomSleekUI()
        {
            this.Controls.Clear();
            this.BackColor = ThemeManager.PrimaryBackground;
            this.Font = ThemeManager.DefaultFont;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopLevel = false;
            this.Dock = DockStyle.Fill;

            TableLayoutPanel mainTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.Controls.Add(mainTable);

            Panel pnlHeader = new Panel { Dock = DockStyle.Fill };
            Label lblHeader = new Label { Text = "Procurement & Purchase Orders", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(20, 20), AutoSize = true };
            pnlHeader.Controls.Add(lblHeader);
            mainTable.Controls.Add(pnlHeader, 0, 0);

            TableLayoutPanel contentTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = new Padding(20, 0, 20, 20) };
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 460F));
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainTable.Controls.Add(contentTable, 0, 1);

            // Left Card
            Panel custom_pnlLeftCard = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, AutoScroll = true };
            custom_pnlLeftCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, custom_pnlLeftCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            contentTable.Controls.Add(custom_pnlLeftCard, 0, 0);

            Label lblCardTitle = new Label { Text = "🛒 Approve & Allocate Vendors", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(79, 70, 229), Location = new Point(20, 15), AutoSize = true };
            custom_pnlLeftCard.Controls.Add(lblCardTitle);

            int startY = 55; int inputWidth = 400;
            custom_txtRCID = CreateCustomTextBox(custom_pnlLeftCard, ref startY, "Ref. Request ID (Header):", true, inputWidth);

            Label lblMat = new Label { Text = "Selected Material (Detail):", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            custom_txtMaterialID = new TextBox { Location = new Point(20, startY + 22), Width = 100, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle, ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249) };
            custom_txtMaterialName = new TextBox { Location = new Point(125, startY + 22), Width = 295, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle, ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249) };
            custom_pnlLeftCard.Controls.Add(lblMat); custom_pnlLeftCard.Controls.Add(custom_txtMaterialID); custom_pnlLeftCard.Controls.Add(custom_txtMaterialName);
            startY += 60;

            int tempY = startY;
            custom_txtQty = CreateCustomTextBox(custom_pnlLeftCard, ref tempY, "Approved Qty:", false, 195);
            startY = tempY - 60;
            custom_txtPrice = CreateCustomTextBox(custom_pnlLeftCard, ref startY, "Unit Cost ($):", false, 195, 225);

            Label lblSup = new Label { Text = "Select Capable Vendor (Supplier) *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(234, 88, 12) };
            custom_cmbSupplier = new ComboBox { Location = new Point(20, startY + 22), Width = inputWidth, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F), BackColor = Color.White };
            custom_pnlLeftCard.Controls.Add(lblSup); custom_pnlLeftCard.Controls.Add(custom_cmbSupplier);
            startY += 65;

            custom_btnAddAllocation = new Button { Text = "➕ Add to PO Cart", Location = new Point(20, startY), Size = new Size(220, 35), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            custom_btnRemoveAllocation = new Button { Text = "❌ Remove", Location = new Point(250, startY), Size = new Size(170, 35), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            custom_btnAddAllocation.FlatAppearance.BorderSize = 0; custom_btnRemoveAllocation.FlatAppearance.BorderSize = 0;
            custom_btnAddAllocation.Click += BtnAddAllocation_Click; custom_btnRemoveAllocation.Click += BtnRemoveAllocation_Click;
            custom_pnlLeftCard.Controls.Add(custom_btnAddAllocation); custom_pnlLeftCard.Controls.Add(custom_btnRemoveAllocation);
            startY += 50;

            Label lblCart = new Label { Text = "📦 PO Staging Cart (Will split automatically):", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42) };
            custom_pnlLeftCard.Controls.Add(lblCart); startY += 25;

            poCartTable = new DataTable();
            poCartTable.Columns.Add("Request ID", typeof(string)); // 🌟 新增：用作隱藏記錄來源申請單號，確保精準扣數
            poCartTable.Columns.Add("Mat.ID", typeof(string));
            poCartTable.Columns.Add("Sup.ID", typeof(string));
            poCartTable.Columns.Add("Supplier Name", typeof(string));
            poCartTable.Columns.Add("Qty", typeof(int));
            poCartTable.Columns.Add("Price", typeof(decimal));
            poCartTable.Columns.Add("Subtotal", typeof(decimal));

            custom_dgvPOCart = new DataGridView { Location = new Point(20, startY), Size = new Size(400, 150), DataSource = poCartTable, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            custom_dgvPOCart.EnableHeadersVisualStyles = false;
            custom_dgvPOCart.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(71, 85, 105);
            custom_dgvPOCart.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            custom_dgvPOCart.RowsDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular); // 🌟 修正：轉回普通平時字體
            custom_pnlLeftCard.Controls.Add(custom_dgvPOCart);
            startY += 165;

            custom_txtTotalCost = CreateCustomTextBox(custom_pnlLeftCard, ref startY, "Global Estimated Cost (HKD):", true, inputWidth);
            custom_txtTotalCost.ForeColor = Color.FromArgb(220, 38, 38);
            custom_txtTotalCost.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            custom_txtStaffID = CreateCustomTextBox(custom_pnlLeftCard, ref startY, "Processed By (Staff ID):", true, inputWidth);

            custom_btnCreatePO = new Button { Text = "🚀 Approve & Dispatch POs", Location = new Point(20, startY), Size = new Size(400, 45), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand };
            custom_btnReject = new Button { Text = "❌ Reject Entire Request", Location = new Point(20, startY + 55), Size = new Size(195, 42), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            custom_btnClear = new Button { Text = "🔄 Clear Everything", Location = new Point(225, startY + 55), Size = new Size(195, 42), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            custom_btnCreatePO.FlatAppearance.BorderSize = 0; custom_btnReject.FlatAppearance.BorderSize = 0; custom_btnClear.FlatAppearance.BorderSize = 0;
            custom_btnCreatePO.Click += btnCreatePO_Click; custom_btnReject.Click += btnReject_Click; custom_btnClear.Click += (s, e) => ClearCustomFields();
            custom_pnlLeftCard.Controls.Add(custom_btnCreatePO); custom_pnlLeftCard.Controls.Add(custom_btnReject); custom_pnlLeftCard.Controls.Add(custom_btnClear);

            // Right TabControl
            TabControl tcRight = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold) };

            // Tab 1: Pending Requests
            TabPage tpRequests = new TabPage("📥 Pending Requests");
            TableLayoutPanel rightTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            rightTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            rightTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            rightTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            rightTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tpRequests.Controls.Add(rightTable);

            Label lblGridTitle = new Label { Text = "⏳ Pending Material Requests (Headers)", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true, Dock = DockStyle.Bottom };
            rightTable.Controls.Add(lblGridTitle, 0, 0);
            custom_dgvPendingRC = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            custom_dgvPendingRC.EnableHeadersVisualStyles = false;
            custom_dgvPendingRC.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            custom_dgvPendingRC.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            custom_dgvPendingRC.RowsDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular); // 🌟 修正：轉回普通平時字體
            custom_dgvPendingRC.SelectionChanged += dgvPendingRC_SelectionChanged;
            rightTable.Controls.Add(custom_dgvPendingRC, 0, 1);

            Label lblDetailsTitle = new Label { Text = "🔍 Request Line Items (Click to allocate vendor)", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true, Dock = DockStyle.Bottom };
            rightTable.Controls.Add(lblDetailsTitle, 0, 2);
            custom_dgvDetails = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            custom_dgvDetails.EnableHeadersVisualStyles = false;
            custom_dgvDetails.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(14, 165, 233);
            custom_dgvDetails.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            custom_dgvDetails.RowsDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular); // 🌟 修正：轉回普通平時字體
            custom_dgvDetails.SelectionChanged += dgvDetails_SelectionChanged;
            rightTable.Controls.Add(custom_dgvDetails, 0, 3);

            // Tab 2: PO History
            TabPage tpPOHistory = new TabPage("📜 Purchase Order History");
            Panel pnlPOSearch = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            Label lblSearchPO = new Label { Text = "🔍 Fast Search PO:", Location = new Point(15, 20), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtPOSearch = new TextBox { Location = new Point(150, 18), Width = 300, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            txtPOSearch.TextChanged += TxtPOSearch_TextChanged;
            pnlPOSearch.Controls.Add(lblSearchPO); pnlPOSearch.Controls.Add(txtPOSearch);
            tpPOHistory.Controls.Add(pnlPOSearch);

            dgvPOList = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvPOList.EnableHeadersVisualStyles = false;
            dgvPOList.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(16, 185, 129);
            dgvPOList.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPOList.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvPOList.RowsDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular); // 🌟 修正：轉回普通平時字體
            tpPOHistory.Controls.Add(dgvPOList);

            tcRight.TabPages.Add(tpRequests);
            tcRight.TabPages.Add(tpPOHistory);
            contentTable.Controls.Add(tcRight, 2, 0);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // 🌟 隱藏購物車中的 Request ID 欄位，不破壞畫面整潔
            if (custom_dgvPOCart.Columns.Contains("Request ID"))
            {
                custom_dgvPOCart.Columns["Request ID"].Visible = false;
            }
        }

        private TextBox CreateCustomTextBox(Panel container, ref int topY, string labelText, bool readOnly, int width, int offsetX = 20)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(offsetX, topY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(offsetX, topY + 22), Width = width, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            if (readOnly) { txt.ReadOnly = true; txt.BackColor = Color.FromArgb(241, 245, 249); }
            container.Controls.Add(lbl); container.Controls.Add(txt);
            topY += 60;
            return txt;
        }

        private void LoadSuppliersForMaterial(string materialID)
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT s.SupplierID, s.SupplierName FROM supplier s JOIN supplier_material sm ON s.SupplierID = sm.SupplierID WHERE sm.MaterialID = @matID AND s.IsActive = 1";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@matID", materialID);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<POSupplierItem> list = new List<POSupplierItem>();
                            while (reader.Read()) list.Add(new POSupplierItem { ID = reader["SupplierID"].ToString().Trim(), Name = $"{reader["SupplierID"].ToString().Trim()} - {reader["SupplierName"]}" });
                            custom_cmbSupplier.DataSource = list;
                            custom_cmbSupplier.DisplayMember = "Name";
                            custom_cmbSupplier.ValueMember = "ID";
                            custom_cmbSupplier.SelectedIndex = -1;
                        }
                    }
                }
                catch { }
            }
        }

        private void LoadPendingRequests()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT r.ReOrderCardID AS 'Request ID', COUNT(r.MaterialID) AS 'Total Items', SUM(r.RequestedQty) AS 'Total Qty', MAX(r.TriggerDate) AS 'Submitted Date' FROM reorder_card r WHERE r.Status = 'Pending Approval' GROUP BY r.ReOrderCardID ORDER BY MAX(r.TriggerDate) DESC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        custom_dgvPendingRC.DataSource = dt;
                    }
                }
                catch (Exception ex) { MessageBox.Show("Failed to load Pending Requests:\n" + ex.Message); }
            }
        }

        private void LoadPOHistory()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT po.PO_ID AS 'PO Number', s.SupplierName AS 'Supplier', po.PODate AS 'Date', po.Status FROM purchase_order po JOIN supplier s ON po.SupplierID = s.SupplierID ORDER BY po.PODate DESC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvPOList.DataSource = dt;
                    }
                }
                catch (Exception ex) { MessageBox.Show("Failed to load PO History: " + ex.Message); }
            }
        }

        private void TxtPOSearch_TextChanged(object sender, EventArgs e)
        {
            if (dgvPOList.DataSource is DataTable dt)
            {
                string keyword = txtPOSearch.Text.Trim().Replace("'", "''");
                dt.DefaultView.RowFilter = string.IsNullOrWhiteSpace(keyword) ? "" : $"[PO Number] LIKE '%{keyword}%' OR Supplier LIKE '%{keyword}%' OR Status LIKE '%{keyword}%'";
            }
        }

        private void dgvPendingRC_SelectionChanged(object sender, EventArgs e)
        {
            if (custom_dgvPendingRC.SelectedRows.Count == 0) return;
            string rcID = custom_dgvPendingRC.SelectedRows[0].Cells["Request ID"].Value.ToString();
            custom_txtRCID.Text = rcID;
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT r.MaterialID AS 'Material ID', m.MaterialName AS 'Material Name', r.RequestedQty AS 'Requested Qty' FROM reorder_card r JOIN raw_material m ON r.MaterialID = m.MaterialID WHERE r.ReOrderCardID = @RCID AND r.Status = 'Pending Approval'";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RCID", rcID);
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            custom_dgvDetails.DataSource = dt;
                        }
                    }
                }
                catch { custom_dgvDetails.DataSource = null; }
            }
        }

        private void dgvDetails_SelectionChanged(object sender, EventArgs e)
        {
            if (custom_dgvDetails.SelectedRows.Count == 0) return;
            DataGridViewRow row = custom_dgvDetails.SelectedRows[0];
            string matID = row.Cells["Material ID"].Value?.ToString() ?? "";
            custom_txtMaterialID.Text = matID;
            custom_txtMaterialName.Text = row.Cells["Material Name"].Value?.ToString() ?? "";
            custom_txtQty.Text = row.Cells["Requested Qty"].Value?.ToString() ?? "";
            custom_cmbSupplier.DataSource = null;
            if (!string.IsNullOrEmpty(matID))
            {
                LoadSuppliersForMaterial(matID);
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        object result = new MySqlCommand("SELECT StandardCost FROM raw_material WHERE MaterialID = @mat", conn) { Parameters = { new MySqlParameter("@mat", matID) } }.ExecuteScalar();
                        custom_txtPrice.Text = result != null && result != DBNull.Value ? Convert.ToDecimal(result).ToString("F2") : "0.00";
                    }
                    catch { custom_txtPrice.Text = "0.00"; }
                }
            }
        }

        private void BtnAddAllocation_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(custom_txtMaterialID.Text) || custom_cmbSupplier.SelectedItem == null || !int.TryParse(custom_txtQty.Text, out int qty) || qty <= 0 || !decimal.TryParse(custom_txtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Please select material, vendor and enter valid quantity/price.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var sup = (POSupplierItem)custom_cmbSupplier.SelectedItem;
            string rcID = custom_txtRCID.Text.Trim();
            string matID = custom_txtMaterialID.Text.Trim();
            decimal subtotal = qty * price;
            bool exists = false;

            // 🌟 修正：比對條件加入 Request ID 確保精確度，防止同物料跨單出事
            foreach (DataRow row in poCartTable.Rows)
            {
                if (row["Request ID"].ToString() == rcID && row["Mat.ID"].ToString() == matID && row["Sup.ID"].ToString() == sup.ID)
                {
                    row["Qty"] = Convert.ToInt32(row["Qty"]) + qty;
                    row["Subtotal"] = Convert.ToDecimal(row["Qty"]) * price;
                    exists = true; break;
                }
            }
            if (!exists) poCartTable.Rows.Add(rcID, matID, sup.ID, sup.Name, qty, price, subtotal);
            UpdateGlobalTotal();
            custom_cmbSupplier.SelectedIndex = -1;
            custom_dgvDetails.ClearSelection();
        }

        private void BtnRemoveAllocation_Click(object sender, EventArgs e)
        {
            if (custom_dgvPOCart.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in custom_dgvPOCart.SelectedRows) if (!row.IsNewRow) custom_dgvPOCart.Rows.Remove(row);
                UpdateGlobalTotal();
            }
        }

        private void UpdateGlobalTotal()
        {
            decimal total = poCartTable.Rows.Cast<DataRow>().Sum(row => Convert.ToDecimal(row["Subtotal"]));
            custom_txtTotalCost.Text = $"$ {total:N2}";
        }

        // ==========================================================
        // 🚀 核心優化：嚴格執行按 Supplier 合併出單，絕不重覆拆分
        // ==========================================================
        private void btnCreatePO_Click(object sender, EventArgs e)
        {
            string staffID = custom_txtStaffID.Text.Trim();
            if (poCartTable.Rows.Count == 0)
            {
                MessageBox.Show("Please select a request and add items to cart.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show("Ready to generate Purchase Orders?\nSystem will merge items by vendor into ONE single PO.", "Confirm Dispatch", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // 🌟 關鍵修正：將 GroupBy Key 進行 Trim() 及 ToUpper() 處理，確保同 Supplier 必然入同一個 Group
                            var supplierGroups = poCartTable.AsEnumerable()
                                                             .GroupBy(r => r.Field<string>("Sup.ID").Trim().ToUpper())
                                                             .ToList();

                            string prefix = "PO" + DateTime.Now.ToString("yyyyMMdd") + "-";
                            int nextSeq = 1;

                            // 獲取當天最新的序號
                            using (MySqlCommand seqCmd = new MySqlCommand("SELECT PO_ID FROM purchase_order WHERE PO_ID LIKE @prefix ORDER BY PO_ID DESC LIMIT 1", conn, trans))
                            {
                                seqCmd.Parameters.AddWithValue("@prefix", prefix + "%");
                                object result = seqCmd.ExecuteScalar();
                                if (result != null && result != DBNull.Value)
                                {
                                    string seqStr = result.ToString().Replace(prefix, "");
                                    if (int.TryParse(seqStr, out int seq)) nextSeq = seq + 1;
                                }
                            }

                            string generatedPOs = "";

                            // 迴圈遍歷每一個供應商分組（一個分組代表一張採購單 Header）
                            foreach (var group in supplierGroups)
                            {
                                string supID = group.First().Field<string>("Sup.ID"); // 拎番最原始無事嘅 ID
                                string poID = prefix + nextSeq.ToString("D3");
                                nextSeq++;

                                // 取出這個分組內的第一個 RequestID 作為 Header 關聯（若跨單則動態記錄）
                                string associatedRCID = group.First().Field<string>("Request ID");

                                // 1️⃣ 寫入採購單主檔 (purchase_order) -> 每一間公司嚴格執行只 Insert 一次！
                                string insertPO = "INSERT INTO purchase_order (PO_ID, SupplierID, ReOrderCardID, PODate, Status, StaffID) VALUES (@PO, @Sup, @RC, NOW(), 'Ordered', @Staff)";
                                using (MySqlCommand cmd = new MySqlCommand(insertPO, conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@PO", poID);
                                    cmd.Parameters.AddWithValue("@Sup", supID);
                                    cmd.Parameters.AddWithValue("@RC", associatedRCID);
                                    cmd.Parameters.AddWithValue("@Staff", staffID);
                                    cmd.ExecuteNonQuery();
                                }

                                // 2️⃣ 寫入該供應商名下的所有物料明細 (po_lineitem) -> 放入同一個 PO_ID
                                string insertLine = "INSERT INTO po_lineitem (PO_ID, MaterialID, Quantity, UnitPrice) VALUES (@PO, @Mat, @Qty, @Price)";
                                using (MySqlCommand cmd = new MySqlCommand(insertLine, conn, trans))
                                {
                                    foreach (var row in group)
                                    {
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("@PO", poID);
                                        cmd.Parameters.AddWithValue("@Mat", row["Mat.ID"]);
                                        cmd.Parameters.AddWithValue("@Qty", row["Qty"]);
                                        cmd.Parameters.AddWithValue("@Price", row["Price"]);
                                        cmd.ExecuteNonQuery();
                                    }
                                }

                                // 3️⃣ 精準更新對應內部申請表狀態 (reorder_card) 
                                // 🌟 修正：採用「單號 + 物料」雙條件精準鎖定，防止同張單其他未處理嘅物料被誤點「Approved」而消失
                                string updateRC = "UPDATE reorder_card SET Status = 'Approved' WHERE ReOrderCardID = @RC AND MaterialID = @Mat";
                                using (MySqlCommand cmd = new MySqlCommand(updateRC, conn, trans))
                                {
                                    foreach (var row in group)
                                    {
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("@RC", row["Request ID"]);
                                        cmd.Parameters.AddWithValue("@Mat", row["Mat.ID"]);
                                        cmd.ExecuteNonQuery();
                                    }
                                }

                                generatedPOs += poID + "\n";
                            }

                            trans.Commit();
                            MessageBox.Show($"Success! Generated POs:\n\n{generatedPOs}", "POs Dispatched", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            ClearCustomFields();
                            LoadPendingRequests();
                            LoadPOHistory();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            MessageBox.Show("Transaction failed, rolled back:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void btnReject_Click(object sender, EventArgs e)
        {
            string rcID = custom_txtRCID.Text.Trim();
            if (string.IsNullOrWhiteSpace(rcID)) return;
            if (MessageBox.Show($"Reject entire request [{rcID}]?", "Confirm Rejection", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("UPDATE reorder_card SET Status = 'Rejected' WHERE ReOrderCardID = @RC AND Status = 'Pending Approval'", conn))
                        {
                            cmd.Parameters.AddWithValue("@RC", rcID);
                            cmd.ExecuteNonQuery();
                        }
                        MessageBox.Show($"Request [{rcID}] rejected.", "Rejected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearCustomFields();
                        LoadPendingRequests();
                    }
                    catch (Exception ex) { MessageBox.Show("Failed to reject: " + ex.Message); }
                }
            }
        }

        private void ClearCustomFields()
        {
            custom_txtRCID.Clear();
            custom_txtMaterialID.Clear();
            custom_txtMaterialName.Clear();
            custom_txtQty.Clear();
            custom_txtPrice.Clear();
            poCartTable.Clear();
            UpdateGlobalTotal();
            custom_cmbSupplier.DataSource = null;
            custom_dgvPendingRC.ClearSelection();
            custom_dgvDetails.DataSource = null;
        }
    }
}