using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class LogisticsForm : Form
    {
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";
        private string currentStaffID;
        private string currentStaffRegion = "Hong Kong"; // 預設地區

        private DataGridView dgvPendingOrders;
        private TextBox txtOrderID, txtCustomerID, txtDeliveryAddress, txtCurrentStatus;
        private ComboBox cboDeliveryStaff;
        private DateTimePicker dtpScheduleDate;
        private TextBox txtSearch;

        public LogisticsForm()
        {
            AuthorizationHelper.EnforceRole(this, AuthorizationHelper.Roles.Administrator, AuthorizationHelper.Roles.Manager, AuthorizationHelper.Roles.LogisticsDriver, AuthorizationHelper.Roles.WarehouseSpecialist);
            this.currentStaffID = string.IsNullOrEmpty(UserSession.LoggedInStaffID) ? "S001" : UserSession.LoggedInStaffID;

            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                DetermineUserRegion();
                InitializeUI();
                LoadData();
                ClearForm();
            }
        }

        // 🌟 解析訂單所屬城市
        private string ExtractRegion(string status)
        {
            if (string.IsNullOrEmpty(status)) return "Hong Kong";
            string lowerStatus = status.ToLower();

            if (lowerStatus.Contains("tokyo")) return "Tokyo";
            if (lowerStatus.Contains("singapore")) return "Singapore";
            if (lowerStatus.Contains("new york") || lowerStatus.Contains("ny")) return "New York";
            if (lowerStatus.Contains("london")) return "London";

            return "Hong Kong"; // 預設為香港
        }

        // 🌟 獲取登入員工所屬地區
        private void DetermineUserRegion()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT Region FROM staff WHERE StaffID = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", currentStaffID);
                        object res = cmd.ExecuteScalar();
                        if (res != null && res != DBNull.Value)
                        {
                            currentStaffRegion = res.ToString();
                        }
                    }
                }
                catch { currentStaffRegion = "Hong Kong"; }
            }
        }

        private void InitializeUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living - Logistics Management";
            this.Size = new Size(1180, 750);
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Font = new Font("Segoe UI", 10F);

            bool isAdmin = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Administrator);
            string headerText = isAdmin ? "🚚 Global Logistics Dispatch Management" : $"🚚 Logistics Dispatch ({currentStaffRegion})";

            Label lblHeader = new Label { Text = headerText, Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(30, 20), AutoSize = true };
            this.Controls.Add(lblHeader);

            Panel pnlCard = new Panel { Location = new Point(30, 85), Size = new Size(380, 600), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(pnlCard);

            int startY = 20;

            txtOrderID = CreateReadOnlyInput(pnlCard, ref startY, "Target Order ID:");
            txtCustomerID = CreateReadOnlyInput(pnlCard, ref startY, "Customer ID:");

            Label lblAddress = new Label { Text = "Destination Address:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtDeliveryAddress = new TextBox { Location = new Point(20, startY + 22), Width = 335, Height = 60, Multiline = true, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle, ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249) };
            pnlCard.Controls.Add(lblAddress); pnlCard.Controls.Add(txtDeliveryAddress);
            startY += 95;

            txtCurrentStatus = CreateReadOnlyInput(pnlCard, ref startY, "Current State:");

            Label lblStaff = new Label { Text = "Assign Delivery Team *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboDeliveryStaff = new ComboBox { Location = new Point(20, startY + 22), Width = 335, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F) };
            pnlCard.Controls.Add(lblStaff); pnlCard.Controls.Add(cboDeliveryStaff);
            startY += 70;

            Label lblDate = new Label { Text = "Scheduled Date *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            dtpScheduleDate = new DateTimePicker { Location = new Point(20, startY + 22), Width = 335, Font = new Font("Segoe UI", 10.5F), Format = DateTimePickerFormat.Short, MinDate = DateTime.Today };
            pnlCard.Controls.Add(lblDate); pnlCard.Controls.Add(dtpScheduleDate);
            startY += 70;

            bool actionEnabled = true;

            Button btnAssignDelivery = new Button { Text = "📦 Dispatch Order", Location = new Point(20, startY), Size = new Size(160, 40), BackColor = Color.FromArgb(14, 165, 233), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Enabled = actionEnabled, Cursor = Cursors.Hand };
            Button btnUpdateStatus = new Button { Text = "✅ Mark Delivered", Location = new Point(195, startY), Size = new Size(160, 40), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Enabled = actionEnabled, Cursor = Cursors.Hand };
            Button btnGenerateNote = new Button { Text = "📄 Generate Note & Reply Slip", Location = new Point(20, startY + 50), Size = new Size(335, 40), BackColor = Color.FromArgb(79, 70, 229), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Enabled = actionEnabled, Cursor = Cursors.Hand };
            Button btnClearFields = new Button { Text = "🧹 Reset Form", Location = new Point(20, startY + 100), Size = new Size(335, 40), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };

            foreach (Button b in new Button[] { btnAssignDelivery, btnUpdateStatus, btnGenerateNote, btnClearFields }) b.FlatAppearance.BorderSize = 0;

            btnAssignDelivery.Click += btnAssignDelivery_Click;
            btnUpdateStatus.Click += btnUpdateStatus_Click;
            btnGenerateNote.Click += btnGenerateNote_Click;
            btnClearFields.Click += (s, e) => ClearForm();

            pnlCard.Controls.AddRange(new Control[] { btnAssignDelivery, btnUpdateStatus, btnGenerateNote, btnClearFields });

            Label lblSearch = new Label { Text = "🔍 Live Search (Order / Customer):", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(440, 52), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(710, 48), Width = 430, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            this.Controls.Add(lblSearch);
            this.Controls.Add(txtSearch);

            dgvPendingOrders = new DataGridView { Location = new Point(440, 85), Size = new Size(700, 600), BackgroundColor = Color.White, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvPendingOrders.RowsDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            // 🌟 格式化日期欄位，確保無論係乜嘢日期格式都可以靚靚地顯示出嚟
            dgvPendingOrders.CellFormatting += (s, e) => {
                if (dgvPendingOrders.Columns[e.ColumnIndex].Name == "Delivery Date" && e.Value != null)
                {
                    if (DateTime.TryParse(e.Value.ToString(), out DateTime date))
                        e.Value = date.ToString("yyyy-MM-dd");
                    e.FormattingApplied = true;
                }
            };

            dgvPendingOrders.SelectionChanged += dgvPendingOrders_SelectionChanged;
            this.Controls.Add(dgvPendingOrders);
        }

        private TextBox CreateReadOnlyInput(Panel container, ref int y, string label)
        {
            Label lbl = new Label { Text = label, Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(20, y + 22), Width = 335, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle, ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249) };
            container.Controls.Add(lbl); container.Controls.Add(txt);
            y += 65; return txt;
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (dgvPendingOrders.DataSource is DataTable dt)
            {
                string keyword = txtSearch.Text.Trim().Replace("'", "''");
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    dt.DefaultView.RowFilter = "";
                }
                else
                {
                    dt.DefaultView.RowFilter = $"[Order ID] LIKE '%{keyword}%' OR [Customer] LIKE '%{keyword}%'";
                }
            }
        }

        // 🌟 新邏輯：根據「訂單嘅城市」讀取嗰個城市專屬嘅物流司機名單
        private void LoadDeliveryStaff(string targetRegion)
        {
            cboDeliveryStaff.Items.Clear();
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT StaffID, Name FROM staff WHERE Role = 'Logistics Driver' AND Region = @Region";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Region", targetRegion);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cboDeliveryStaff.Items.Add($"{reader["StaffID"]} - {reader["Name"]}");
                            }
                        }
                    }
                    // 加入全球通用外判物流
                    cboDeliveryStaff.Items.Add("Outsource - DHL Global Forwarding");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading driver list: " + ex.Message);
                }
            }
        }

        private void LoadData()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    bool isAdmin = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Administrator) || AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager);
                    string query;

                    // 🌟 核心修正：將 'OrderDate' 改為 'DeliveryDate'，並用 COALESCE 防止 NULL
                    if (isAdmin)
                    {
                        // Admin / Manager 睇晒全世界所有 Dispatch 訂單
                        query = @"SELECT OrderID AS 'Order ID', CustomerID AS 'Customer', Status, 
                                  COALESCE(DeliveryDate, OrderDate) AS 'Delivery Date' 
                                  FROM orders 
                                  WHERE Status LIKE 'Ready for Dispatch%' OR Status LIKE 'Dispatched%' 
                                  ORDER BY COALESCE(DeliveryDate, OrderDate) DESC";
                    }
                    else
                    {
                        // 普通員工只睇自己城市嘅訂單
                        query = @"SELECT OrderID AS 'Order ID', CustomerID AS 'Customer', Status, 
                                  COALESCE(DeliveryDate, OrderDate) AS 'Delivery Date' 
                                  FROM orders 
                                  WHERE (Status = @StatusDispatch OR Status = @StatusDispatched) 
                                  ORDER BY COALESCE(DeliveryDate, OrderDate) DESC";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        if (!isAdmin)
                        {
                            cmd.Parameters.AddWithValue("@StatusDispatch", $"Ready for Dispatch [{currentStaffRegion}]");
                            cmd.Parameters.AddWithValue("@StatusDispatched", $"Dispatched [{currentStaffRegion}]");
                        }

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dgvPendingOrders.DataSource = dt;

                            if (txtSearch != null && !string.IsNullOrWhiteSpace(txtSearch.Text))
                            {
                                TxtSearch_TextChanged(null, null);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading orders: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvPendingOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPendingOrders.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvPendingOrders.SelectedRows[0];
                txtOrderID.Text = row.Cells["Order ID"].Value?.ToString() ?? "";
                txtCustomerID.Text = row.Cells["Customer"].Value?.ToString() ?? "";
                txtCurrentStatus.Text = row.Cells["Status"].Value?.ToString() ?? "";

                // 🌟 當你點擊一張訂單，系統會解析訂單所在城市，並刷新 ComboBox 嘅司機名單！
                string orderRegion = ExtractRegion(txtCurrentStatus.Text);
                LoadDeliveryStaff(orderRegion);

                // 同步將左邊卡片嘅 Schedule Date 設定為表格上顯示嘅送貨日期
                if (row.Cells["Delivery Date"].Value != null && DateTime.TryParse(row.Cells["Delivery Date"].Value.ToString(), out DateTime parsedDate))
                {
                    // 防止設定咗以前嘅日期導致 Error，保證最小為今日
                    if (parsedDate < DateTime.Today) dtpScheduleDate.Value = DateTime.Today;
                    else dtpScheduleDate.Value = parsedDate;
                }

                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connString))
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("SELECT Address FROM customer WHERE CustomerID=@CID", conn))
                        {
                            cmd.Parameters.AddWithValue("@CID", txtCustomerID.Text);
                            var addr = cmd.ExecuteScalar();
                            txtDeliveryAddress.Text = (addr != null && addr != DBNull.Value) ? addr.ToString() : "Standard Registered Address (Please verify with client)";
                        }
                    }
                }
                catch
                {
                    txtDeliveryAddress.Text = "Standard Registered Address (Please verify with client)";
                }
            }
        }

        private void btnAssignDelivery_Click(object sender, EventArgs e)
        {
            string orderID = txtOrderID.Text.Trim();
            string address = txtDeliveryAddress.Text.Trim();
            string oldStatus = txtCurrentStatus.Text.Trim();

            if (string.IsNullOrEmpty(orderID) || cboDeliveryStaff.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an Order and assign a Delivery Team first.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (oldStatus.StartsWith("Dispatched"))
            {
                MessageBox.Show("This order has already been dispatched!", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string orderRegion = ExtractRegion(oldStatus);
            string newStatus = $"Dispatched [{orderRegion}]";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // 更新訂單狀態時，一併更新真實出貨日期 (以防有臨時更改)
                            string updateSql = "UPDATE orders SET Status=@NewStatus, DeliveryDate=@dDate WHERE OrderID=@OrderID";
                            using (MySqlCommand cmd = new MySqlCommand(updateSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@NewStatus", newStatus);
                                cmd.Parameters.AddWithValue("@dDate", dtpScheduleDate.Value);
                                cmd.Parameters.AddWithValue("@OrderID", orderID);
                                cmd.ExecuteNonQuery();
                            }

                            string dnID = "DN-" + DateTime.Now.ToString("yyyyMMdd-HHmm");
                            string insertDnSql = "INSERT INTO delivery_note (DeliveryNoteID, OrderID, DeliveryDate, DeliveryAddress) VALUES (@dnID, @oID, @dDate, @addr)";

                            using (MySqlCommand cmd = new MySqlCommand(insertDnSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@dnID", dnID);
                                cmd.Parameters.AddWithValue("@oID", orderID);
                                cmd.Parameters.AddWithValue("@dDate", dtpScheduleDate.Value);
                                cmd.Parameters.AddWithValue("@addr", address);
                                cmd.ExecuteNonQuery();
                            }

                            trans.Commit();
                            MessageBox.Show($"Order [{orderID}] has been successfully dispatched!\nDelivery Note: {dnID}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LoadData();
                            ClearForm();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            MessageBox.Show("Dispatch failed (Database Error): " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LogisticsForm_Load(object sender, EventArgs e)
        {

        }

        private void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            string orderID = txtOrderID.Text.Trim();
            if (string.IsNullOrEmpty(orderID)) return;

            string oldStatus = txtCurrentStatus.Text.Trim();

            if (!oldStatus.StartsWith("Dispatched"))
            {
                MessageBox.Show("Only 'Dispatched' orders can be marked as Delivery Completed.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string orderRegion = ExtractRegion(oldStatus);
            string finalStatus = $"Delivery Completed [{orderRegion}]";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string updateSql = "UPDATE orders SET Status=@FinalStatus WHERE OrderID=@OrderID";
                    using (MySqlCommand cmd = new MySqlCommand(updateSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@FinalStatus", finalStatus);
                        cmd.Parameters.AddWithValue("@OrderID", orderID);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show($"Order [{orderID}] marked as delivered.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadData();
                        ClearForm();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnGenerateNote_Click(object sender, EventArgs e)
        {
            string orderID = txtOrderID.Text.Trim();
            if (string.IsNullOrEmpty(orderID))
            {
                MessageBox.Show("Please select an active order from the dashboard to display preview.", "System Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string deliveryStaff = cboDeliveryStaff.SelectedIndex != -1 ? cboDeliveryStaff.Text : "Unassigned Team";
            string scheduleDate = dtpScheduleDate.Value.ToString("MMMM dd, yyyy");
            string customerID = txtCustomerID.Text.Trim();
            string address = txtDeliveryAddress.Text.Trim();

            Form previewForm = new Form { Text = "Premium Invoice & Slip Hub", Size = new Size(620, 800), StartPosition = FormStartPosition.CenterParent, BackColor = Color.FromArgb(248, 250, 252), FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false };
            Panel scrollContainer = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(25, 15, 25, 90) };
            previewForm.Controls.Add(scrollContainer);

            Panel docSheet = new Panel { Width = 540, Height = 640, Location = new Point(25, 20), BackColor = Color.White, Padding = new Padding(30) };
            scrollContainer.Controls.Add(docSheet);

            Label lblBrand = new Label { Text = "PREMIUM LIVING FURNITURE", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(79, 70, 229), Location = new Point(30, 30), AutoSize = true };
            Label lblDocType1 = new Label { Text = "DELIVERY NOTE", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(30, 60), AutoSize = true };
            docSheet.Controls.Add(lblBrand); docSheet.Controls.Add(lblDocType1);

            int currentMetaY = 100;
            string[] metadataLabels = { "Order Identifier:", "Client Code:", "Target Address:", "Execution Date:", "Logistic Operator:" };
            string[] metadataValues = { orderID, customerID, address, scheduleDate, deliveryStaff };

            for (int i = 0; i < metadataLabels.Length; i++)
            {
                Label lblMeta = new Label { Text = metadataLabels[i], Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(30, currentMetaY), AutoSize = true };
                Label lblVal = new Label { Text = metadataValues[i], Font = new Font("Segoe UI", 9.5F, FontStyle.Regular), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(170, currentMetaY), AutoSize = true };
                docSheet.Controls.Add(lblMeta); docSheet.Controls.Add(lblVal);
                currentMetaY += 28;
            }

            Panel pnlDivider = new Panel { Location = new Point(30, 260), Size = new Size(480, 2), BackColor = Color.FromArgb(226, 232, 240) };
            docSheet.Controls.Add(pnlDivider);

            Panel pnlReplySlipCard = new Panel { Location = new Point(30, 285), Size = new Size(480, 320), BackColor = Color.FromArgb(241, 245, 249), Padding = new Padding(20) };
            docSheet.Controls.Add(pnlReplySlipCard);

            Label lblSlipTitle = new Label { Text = "CUSTOMER REPLY SLIP (GOODS CONFIRMATION)", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(20, 20), AutoSize = true };
            Label lblSlipDesc = new Label { Text = $"I hereby acknowledge that all package contents tied to Order ID [{orderID}] have arrived intact without visible structural defects.", Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(20, 50), Size = new Size(440, 45), AutoSize = false };
            pnlReplySlipCard.Controls.Add(lblSlipTitle); pnlReplySlipCard.Controls.Add(lblSlipDesc);

            int sigY = 130;
            string[] sigFields = { "Customer Signature", "Authorized Date", "Carrier Validation" };
            for (int i = 0; i < sigFields.Length; i++)
            {
                Label lblLine = new Label { Text = "_____________________________________", Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(203, 213, 225), Location = new Point(20, sigY), AutoSize = true };
                Label lblField = new Label { Text = sigFields[i], Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(20, sigY + 20), AutoSize = true };
                pnlReplySlipCard.Controls.Add(lblLine); pnlReplySlipCard.Controls.Add(lblField);
                sigY += 60;
            }

            Panel pnlActionDock = new Panel { Size = new Size(620, 80), Location = new Point(0, 685), BackColor = Color.White, BorderStyle = BorderStyle.None };
            previewForm.Controls.Add(pnlActionDock); pnlActionDock.BringToFront();
            Panel pnlTopLine = new Panel { Size = new Size(620, 1), Location = new Point(0, 0), BackColor = Color.FromArgb(226, 232, 240) };
            pnlActionDock.Controls.Add(pnlTopLine);

            Button btnExport = new Button { Text = "🖨️ Export / Save as Document", Location = new Point(25, 18), Size = new Size(380, 44), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnExport.FlatAppearance.BorderSize = 0;
            Button btnCancel = new Button { Text = "Dismiss", Location = new Point(425, 18), Size = new Size(150, 44), BackColor = Color.FromArgb(241, 245, 249), ForeColor = Color.FromArgb(100, 116, 139), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCancel.FlatAppearance.BorderSize = 0;

            btnExport.Click += (src, args) =>
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Web Document for Printing (*.html)|*.html|Standard Text File (*.txt)|*.txt";
                    sfd.FileName = $"DeliveryManifest_{orderID}";
                    sfd.Title = "Choose System Directory Location to Save File";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            string fileContent = "";
                            if (sfd.FileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                            {
                                fileContent = $@"<!DOCTYPE html><html><head><meta charset='utf-8'><title>Delivery Note - {orderID}</title><style>body{{font-family:'Segoe UI',Arial,sans-serif;margin:40px;color:#1e293b;background-color:#f8fafc;}}.container{{max-width:650px;background:white;padding:40px;border-radius:8px;box-shadow:0 4px 6px rgba(0,0,0,0.05);border-top:8px solid #4f46e5;margin:0 auto;}}h1{{color:#1e3a8a;margin:0 0 5px 0;font-size:24px;font-weight:800;}}.subtitle{{color:#4f46e5;font-weight:bold;margin-bottom:30px;letter-spacing:1px;}}.meta-table{{width:100%;border-collapse:collapse;margin-bottom:40px;}}.meta-table td{{padding:12px 0;border-bottom:1px solid #e2e8f0;font-size:15px;}}.meta-label{{font-weight:bold;color:#64748b;width:35%;}}.meta-value{{color:#0f172a;}}.reply-slip{{background-color:#f8fafc;border:2px dashed #94a3b8;padding:25px;border-radius:6px;margin-top:30px;}}.slip-title{{color:#0f172a;font-weight:bold;font-size:16px;margin-bottom:10px;}}.sig-container{{margin-top:50px;display:flex;justify-content:space-between;}}.sig-box{{width:45%;border-top:1px solid #94a3b8;text-align:center;padding-top:8px;font-size:13px;color:#475569;font-weight:bold;margin-top:40px;}}</style></head><body><div class='container'><h1>PREMIUM LIVING FURNITURE</h1><div class='subtitle'>OFFICIAL DELIVERY NOTE</div><table class='meta-table'><tr><td class='meta-label'>Order Reference:</td><td class='meta-value'><b>{orderID}</b></td></tr><tr><td class='meta-label'>Customer ID:</td><td class='meta-value'>{customerID}</td></tr><tr><td class='meta-label'>Destination Address:</td><td class='meta-value'>{address}</td></tr><tr><td class='meta-label'>Scheduled Date:</td><td class='meta-value'>{scheduleDate}</td></tr><tr><td class='meta-label'>Assigned Logistics Team:</td><td class='meta-value'>{deliveryStaff}</td></tr></table><div class='reply-slip'><div class='slip-title'>📌 CUSTOMER REPLY SLIP & RECEIPT</div><p style='color:#475569;font-size:14px;'>I hereby confirm receipt of items for Order [{orderID}] in pristine condition without any structural damages.</p><div class='sig-container'><div class='sig-box'>Customer Signature / Date</div><div class='sig-box'>Delivery Operator Signature</div></div></div></div></body></html>";
                            }
                            else
                            {
                                fileContent = $"PREMIUM LIVING FURNITURE\n----------------------------------------------------\nDELIVERY NOTE DATA SUMMARY\n----------------------------------------------------\nOrder Ref: {orderID}\nCustomer Ref: {customerID}\nTarget Date: {scheduleDate}\nLogistics: {deliveryStaff}\nDestination: {address}\n----------------------------------------------------\nCUSTOMER REPLY MANIFEST\n----------------------------------------------------\nStatus: Received in good condition.\nCustomer Sign: ___________________________ \nDate Signed: ___________________________";
                            }

                            File.WriteAllText(sfd.FileName, fileContent, System.Text.Encoding.UTF8);
                            MessageBox.Show($"File successfully pipeline-routed and stored at:\n{sfd.FileName}", "System Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });

                            previewForm.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"File pipeline exception:\n{ex.Message}", "IO Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            };

            btnCancel.Click += (src, args) => previewForm.Close();
            pnlActionDock.Controls.Add(btnExport); pnlActionDock.Controls.Add(btnCancel);
            previewForm.ShowDialog();
        }

        private void ClearForm()
        {
            txtOrderID.Clear();
            txtCustomerID.Clear();
            txtDeliveryAddress.Clear();
            txtCurrentStatus.Clear();
            if (cboDeliveryStaff.Items.Count > 0) cboDeliveryStaff.Items.Clear(); // 🌟 清空下拉選單

            dtpScheduleDate.Value = DateTime.Today;

            dgvPendingOrders.ClearSelection();
        }
    }
}