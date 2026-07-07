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
        private ComboBox cboDestination; // 🌟 送貨目的地選擇
        private DataGridView dgvHistory, dgvDetails, dgvCart;
        private Button btnAddItem, btnRemoveItem, btnSubmitRequest, btnClear;
        private Label lblTotalAmountDisplay;
        private DataTable cartTable;
        private TextBox txtSearchReq;

        // 👨‍💼 權限與地區動態變數
        private bool isAdminOrManager = false;
        private string staffRegion = "Hong Kong";
        private string staffWarehouseID = "W001";

        public RawMaterialRequestForm() : this(UserSession.LoggedInStaffID ?? "S001") { }

        public RawMaterialRequestForm(string loggedInStaffID)
        {
            this.currentStaffID = loggedInStaffID;
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                DetermineUserAccessLevel(); // 🛡️ 查核權限與城市
                InitializePremiumModernUI();
                EnforceSecurityGatekeeper();
            }
        }

        #region 🔒 權限與地區動態映射
        private void DetermineUserAccessLevel()
        {
            string currentRole = UserSession.LoggedInStaffRole;
            isAdminOrManager = !string.IsNullOrEmpty(currentRole) &&
                               (currentRole.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                                currentRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase));

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT Region FROM staff WHERE StaffID = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", currentStaffID);
                        object res = cmd.ExecuteScalar();
                        if (res != null && res != DBNull.Value) staffRegion = res.ToString();
                    }
                }
                catch { staffRegion = "Hong Kong"; }
            }
            staffWarehouseID = GetWarehouseIDFromRegion(staffRegion);
        }

        private string GetWarehouseIDFromRegion(string region)
        {
            if (string.IsNullOrEmpty(region)) return "W001";
            if (region.IndexOf("Tokyo", StringComparison.OrdinalIgnoreCase) >= 0) return "W002";
            if (region.IndexOf("Singapore", StringComparison.OrdinalIgnoreCase) >= 0) return "W003";
            if (region.IndexOf("New York", StringComparison.OrdinalIgnoreCase) >= 0 || region.IndexOf("NY", StringComparison.OrdinalIgnoreCase) >= 0) return "W004";
            if (region.IndexOf("London", StringComparison.OrdinalIgnoreCase) >= 0) return "W005";
            return "W001";
        }

        private void EnforceSecurityGatekeeper()
        {
            bool isAuthorized = AuthorizationHelper.HasMenuPermission("MATERIAL_REQUESTS");
            if (!isAuthorized)
            {
                var currentRole = string.IsNullOrEmpty(UserSession.LoggedInStaffRole) ? "Guest" : UserSession.LoggedInStaffRole;
                MessageBox.Show($"[SECURITY ALERT] Access Denied!\n\nYour account role \"{currentRole}\" is not authorized to create Material Reorder Cards.", "System Security Guard", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.Shown += (s, e) => this.Close();
            }
        }
        #endregion

        private void RawMaterialRequestForm_Load(object sender, EventArgs e)
        {
            GenerateRequestBatchID();
            LoadMaterialsToCombo();
            RefreshHistoryGrid();
        }

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
            rightTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            rightTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            rightTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            rightTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            Panel pnlHistoryHeader = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            string gridTitleStr = isAdminOrManager ? "📊 Request Headers (All Cities)" : $"📊 Request Headers ({staffRegion})";
            Label lblGridTitle = new Label { Text = gridTitleStr, Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true, Location = new Point(0, 12) };
            pnlHistoryHeader.Controls.Add(lblGridTitle);

            Label lblSearch = new Label { Text = "🔍 Search:", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), AutoSize = true, Location = new Point(320, 15) };
            txtSearchReq = new TextBox { Location = new Point(400, 12), Width = 180, Font = new Font("Segoe UI", 10F), BorderStyle = BorderStyle.FixedSingle };
            txtSearchReq.TextChanged += TxtSearchReq_TextChanged;
            pnlHistoryHeader.Controls.Add(lblSearch); pnlHistoryHeader.Controls.Add(txtSearchReq);
            rightTable.Controls.Add(pnlHistoryHeader, 0, 0);

            dgvHistory = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, GridColor = Color.FromArgb(241, 245, 249), EnableHeadersVisualStyles = false };
            dgvHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235); dgvHistory.ColumnHeadersDefaultCellStyle.ForeColor = Color.White; dgvHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold); dgvHistory.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular); dgvHistory.ColumnHeadersHeight = 35;
            dgvHistory.CellFormatting += dgvHistory_CellFormatting; dgvHistory.SelectionChanged += DgvHistory_SelectionChanged;
            rightTable.Controls.Add(dgvHistory, 0, 1);

            Label lblDetailsTitle = new Label { Text = "🔍 Request Line Items", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true, Dock = DockStyle.Bottom };
            rightTable.Controls.Add(lblDetailsTitle, 0, 2);

            dgvDetails = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, GridColor = Color.FromArgb(241, 245, 249), EnableHeadersVisualStyles = false };
            dgvDetails.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(71, 85, 105); dgvDetails.ColumnHeadersDefaultCellStyle.ForeColor = Color.White; dgvDetails.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold); dgvDetails.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular); dgvDetails.ColumnHeadersHeight = 35;
            rightTable.Controls.Add(dgvDetails, 0, 3);
            contentTable.Controls.Add(rightTable, 2, 0);

            // --- Builder Fields ---
            Label lblCardTitle = new Label { Text = "📝 Request Builder", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(20, 15), AutoSize = true };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 50;
            txtRequestID = CreateStyledTextBox(pnlCard, ref startY, "Request ID (Auto):", true, 450, 20);

            // 🌟 目的地選擇 (Warehouse)
            Label lblDest = new Label { Text = "Destination Warehouse *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(14, 165, 233) };
            cboDestination = new ComboBox { Location = new Point(20, startY + 25), Width = 450, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F), BackColor = Color.White };
            cboDestination.Items.AddRange(new string[] { "W001 - Hong Kong", "W002 - Tokyo", "W003 - Singapore", "W004 - New York", "W005 - London" });
            pnlCard.Controls.Add(lblDest); pnlCard.Controls.Add(cboDestination);
            startY += 65;

            // 🌟 權限鎖定 UI
            if (!isAdminOrManager)
            {
                cboDestination.Enabled = false; // 鎖死唔畀揀
                for (int i = 0; i < cboDestination.Items.Count; i++)
                {
                    if (cboDestination.Items[i].ToString().StartsWith(staffWarehouseID))
                    {
                        cboDestination.SelectedIndex = i;
                        break;
                    }
                }
            }

            Label lblMat = new Label { Text = "Select Material *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboMaterials = new ComboBox { Location = new Point(20, startY + 25), Width = 450, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F), BackColor = Color.White };
            cboMaterials.SelectedIndexChanged += cboMaterials_SelectedIndexChanged;
            pnlCard.Controls.Add(lblMat); pnlCard.Controls.Add(cboMaterials);
            startY += 65;

            int tempY = startY;
            txtUnitPrice = CreateStyledTextBox(pnlCard, ref tempY, "Ref. Unit Cost ($):", true, 210, 20);
            tempY = startY;
            txtQty = CreateStyledTextBox(pnlCard, ref tempY, "Quantity *:", false, 210, 260);
            startY = tempY;

            btnAddItem = new Button { Text = "➕ Add Item", Location = new Point(20, startY), Size = new Size(210, 35), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnRemoveItem = new Button { Text = "❌ Remove", Location = new Point(260, startY), Size = new Size(210, 35), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAddItem.Click += BtnAddItem_Click; btnRemoveItem.Click += BtnRemoveItem_Click;
            pnlCard.Controls.Add(btnAddItem); pnlCard.Controls.Add(btnRemoveItem);
            startY += 60;

            Label lblCartGridTitle = new Label { Text = "📦 Staging Cart", Location = new Point(20, startY), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true };
            pnlCard.Controls.Add(lblCartGridTitle);
            startY += 30;

            cartTable = new DataTable();
            cartTable.Columns.Add("MaterialID", typeof(string)); cartTable.Columns.Add("Material", typeof(string)); cartTable.Columns.Add("Qty", typeof(int)); cartTable.Columns.Add("Unit Price", typeof(decimal)); cartTable.Columns.Add("Subtotal", typeof(decimal));

            dgvCart = new DataGridView { Location = new Point(20, startY), Size = new Size(450, 130), DataSource = cartTable, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, EnableHeadersVisualStyles = false };
            dgvCart.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(71, 85, 105); dgvCart.ColumnHeadersDefaultCellStyle.ForeColor = Color.White; dgvCart.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold); dgvCart.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            pnlCard.Controls.Add(dgvCart);
            startY += 145;

            lblTotalAmountDisplay = new Label { Text = "Total Est. Cost: $0.00", Location = new Point(20, startY), Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(220, 38, 38), AutoSize = true };
            pnlCard.Controls.Add(lblTotalAmountDisplay);
            startY += 40;

            btnSubmitRequest = new Button { Text = "🚀 Submit Request", Location = new Point(20, startY), Size = new Size(210, 42), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClear = new Button { Text = "🧹 Clear Everything", Location = new Point(260, startY), Size = new Size(210, 42), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSubmitRequest.Click += BtnSubmitRequest_Click; btnClear.Click += (s, e) => ClearFields();
            pnlCard.Controls.Add(btnSubmitRequest); pnlCard.Controls.Add(btnClear);
        }

        private TextBox CreateStyledTextBox(Panel container, ref int topY, string labelText, bool readOnly, int width, int offsetX = 20)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(offsetX, topY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(offsetX, topY + 25), Width = width, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            if (readOnly) { txt.ReadOnly = true; txt.BackColor = Color.FromArgb(241, 245, 249); }
            container.Controls.Add(lbl); container.Controls.Add(txt);
            topY += 65; return txt;
        }

        private void dgvHistory_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvHistory.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status == "Pending Approval" || status == "Pending") { e.CellStyle.ForeColor = Color.DarkOrange; e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold); }
                else if (status == "Approved") { e.CellStyle.ForeColor = Color.MediumSeaGreen; e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold); }
                else if (status == "Rejected" || status == "Cancelled") { e.CellStyle.ForeColor = Color.Crimson; e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Strikeout | FontStyle.Bold); }
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
                    using (MySqlCommand cmd = new MySqlCommand("SELECT MaterialID, MaterialName, StandardCost FROM raw_material", conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<MaterialItem> list = new List<MaterialItem>();
                        while (reader.Read())
                            list.Add(new MaterialItem { ID = reader["MaterialID"].ToString(), Name = reader["MaterialName"].ToString(), StandardCost = reader["StandardCost"] != DBNull.Value ? Convert.ToDecimal(reader["StandardCost"]) : 0 });
                        cboMaterials.DataSource = list; cboMaterials.DisplayMember = "Name"; cboMaterials.ValueMember = "ID"; cboMaterials.SelectedIndex = -1;
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

        private void TxtSearchReq_TextChanged(object sender, EventArgs e)
        {
            if (dgvHistory.DataSource is DataTable dt)
            {
                string keyword = txtSearchReq.Text.Trim().Replace("'", "''");
                dt.DefaultView.RowFilter = string.IsNullOrWhiteSpace(keyword) ? "" : $"[Request ID] LIKE '%{keyword}%' OR Status LIKE '%{keyword}%' OR Destination LIKE '%{keyword}%'";
            }
        }
        #endregion

        #region ➕ Add/Remove Cart Items
        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            if (cboMaterials.SelectedItem == null || !int.TryParse(txtQty.Text.Trim(), out int qty) || qty <= 0)
            {
                MessageBox.Show("Please select a Material and enter a valid quantity.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
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
                    itemExists = true; break;
                }
            }
            if (!itemExists) cartTable.Rows.Add(mat.ID, mat.Name, qty, currentUnitCost, subtotal);

            UpdateGlobalTotal(); txtQty.Clear(); cboMaterials.SelectedIndex = -1; txtUnitPrice.Clear(); currentUnitCost = 0;
        }

        private void BtnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvCart.SelectedRows) if (!row.IsNewRow) dgvCart.Rows.Remove(row);
                UpdateGlobalTotal();
            }
        }

        private void UpdateGlobalTotal()
        {
            globalTotal = 0;
            foreach (DataRow row in cartTable.Rows) globalTotal += Convert.ToDecimal(row["Subtotal"]);
            lblTotalAmountDisplay.Text = $"Total Est. Cost: ${globalTotal:N2}";
        }
        #endregion

        #region 🚀 Submit Transaction
        private void BtnSubmitRequest_Click(object sender, EventArgs e)
        {
            if (cartTable.Rows.Count == 0 || cboDestination.SelectedIndex == -1)
            {
                MessageBox.Show("Cart is empty or destination not selected.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }

            string destWH = cboDestination.SelectedItem.ToString().Substring(0, 4);

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
                            string query = @"INSERT INTO reorder_card (ReOrderCardID, MaterialID, RequestedQty, Status, TriggerDate, WarehouseID) 
                                             VALUES (@rcID, @matID, @qty, 'Pending Approval', NOW(), @whID)";

                            using (MySqlCommand cmd = new MySqlCommand(query, conn, trans))
                            {
                                foreach (DataRow row in cartTable.Rows)
                                {
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("@rcID", requestID);
                                    cmd.Parameters.AddWithValue("@matID", row["MaterialID"]);
                                    cmd.Parameters.AddWithValue("@qty", row["Qty"]);
                                    cmd.Parameters.AddWithValue("@whID", destWH);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            trans.Commit();
                            MessageBox.Show($"Successfully submitted {cartTable.Rows.Count} material items under Request [{requestID}] for {destWH}!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields(); RefreshHistoryGrid();
                        }
                        catch (Exception) { trans.Rollback(); throw; }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Submission failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }
        #endregion

        #region 🔄 Refresh & History Filtering
        private void RefreshHistoryGrid()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // 🌟 權限過濾：如果唔係 Admin，就只睇到自己倉庫發出嘅請求
                    string query = @"SELECT r.ReOrderCardID AS 'Request ID', 
                                            COALESCE(w.City, r.WarehouseID, 'Hong Kong') AS 'Destination',
                                            COUNT(r.MaterialID) AS 'Items Included', 
                                            SUM(r.RequestedQty) AS 'Total Qty', 
                                            r.Status, 
                                            MAX(r.TriggerDate) AS 'Date' 
                                     FROM reorder_card r 
                                     LEFT JOIN warehouse w ON r.WarehouseID = w.WarehouseID ";

                    if (!isAdminOrManager) query += " WHERE r.WarehouseID = @whID ";
                    query += " GROUP BY r.ReOrderCardID, w.City, r.WarehouseID, r.Status ORDER BY Date DESC LIMIT 20";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        if (!isAdminOrManager) cmd.Parameters.AddWithValue("@whID", staffWarehouseID);
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable(); adapter.Fill(dt); dgvHistory.DataSource = dt;
                        }
                    }
                }
                catch (Exception ex) { dgvHistory.DataSource = new DataTable(); }
            }
        }

        private void DgvHistory_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvHistory.SelectedRows.Count > 0 && dgvHistory.SelectedRows[0].Cells["Request ID"].Value != null)
            {
                string reqID = dgvHistory.SelectedRows[0].Cells["Request ID"].Value.ToString();
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        string query = @"SELECT r.MaterialID AS 'Material ID', m.MaterialName AS 'Material Name', r.RequestedQty AS 'Qty' 
                                         FROM reorder_card r JOIN raw_material m ON r.MaterialID = m.MaterialID 
                                         WHERE r.ReOrderCardID = @id";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", reqID);
                            using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                            {
                                DataTable dt = new DataTable(); adapter.Fill(dt); dgvDetails.DataSource = dt;
                            }
                        }
                    }
                    catch { dgvDetails.DataSource = null; }
                }
            }
            else dgvDetails.DataSource = null;
        }

        private void GenerateRequestBatchID() { txtRequestID.Text = "RQ" + DateTime.Now.ToString("yyMMddHHmmss"); }

        private void ClearFields()
        {
            cartTable.Clear(); UpdateGlobalTotal();
            if (txtSearchReq != null) txtSearchReq.Clear();
            cboMaterials.SelectedIndex = -1;

            // 🌟 如果係 Admin 可以 Clear 目的地，如果唔係就繼續鎖死
            if (isAdminOrManager) cboDestination.SelectedIndex = -1;

            txtQty.Clear(); txtUnitPrice.Clear(); GenerateRequestBatchID(); dgvHistory.ClearSelection();
        }
        #endregion
    }
}