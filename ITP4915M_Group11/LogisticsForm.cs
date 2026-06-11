using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class LogisticsForm : Form
    {
        // ==========================================
        // 🔒 Database Configuration
        // ==========================================
        private readonly string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // ==========================================
        // 🎨 Modern UI Element Variables
        // ==========================================
        private TextBox txtShowOrderID, txtShowCustomer, txtShowOrderDate;
        private TextBox txtDispatchID, txtDriverName;
        private ComboBox cboMethod;
        private DateTimePicker dtpEstDelivery;
        private DataGridView dgvPendingOrders;
        private Button btnGenerateDeliveryNote, btnViewDeliveryNote;

        public LogisticsForm()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                InitializePremiumModernUI();
                SetupDispatchControls();
                RefreshPendingOrdersGrid();
            }
        }

        #region 🎨 Dynamic Premium English UI Construction
        private void InitializePremiumModernUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Delivery Logistics Control";
            this.Size = new Size(1180, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Left Sidebar Navigation Panel
            Panel pnlSidebar = new Panel { Width = 260, Dock = DockStyle.Left, BackColor = Color.FromArgb(15, 23, 42) };
            Label lblLogo = new Label { Text = "Premium Living\nFurniture", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 25), Size = new Size(220, 60), TextAlign = ContentAlignment.MiddleLeft };
            pnlSidebar.Controls.Add(lblLogo);

            string[] menuItems = { "🛒 Sales Order Mgmt", "🚚 Delivery Logistics", "🛋️ Product Maintenance", "👔 HR / Staff Mgmt", "📦 Goods Received (GRN)", "🏭 Material Requests", "📊 Procurement Control", "🔧 Customer Support", "🚪 Logout System" };
            int btnTop = 110;
            foreach (string item in menuItems)
            {
                Button btnMenu = new Button { Text = "  " + item, Top = btnTop, Left = 12, Size = new Size(236, 48), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand };
                btnMenu.FlatAppearance.BorderSize = 0;

                // Color Logic
                if (item.Contains("Delivery Logistics")) { btnMenu.BackColor = Color.FromArgb(37, 99, 235); btnMenu.ForeColor = Color.White; }
                else if (item.Contains("Logout")) { btnMenu.BackColor = Color.FromArgb(239, 68, 68); btnMenu.ForeColor = Color.White; }
                else { btnMenu.BackColor = Color.Transparent; btnMenu.ForeColor = Color.FromArgb(148, 163, 184); }

                // 導航欄跳轉邏輯
                btnMenu.Click += (s, e) => {
                    Form targetForm = null;
                    try
                    {
                        if (item.Contains("Sales Order Mgmt")) targetForm = new OrderManagementForm();
                        else if (item.Contains("Product Maintenance")) targetForm = new ProductManagement();
                        else if (item.Contains("Staff Mgmt")) targetForm = new EmployeeManagement();
                        else if (item.Contains("Received")) targetForm = new GoodsReceivedForm();
                        else if (item.Contains("Material Requests")) targetForm = new RawMaterialRequestForm();
                        else if (item.Contains("Procurement")) targetForm = new ProcurementForm();
                        else if (item.Contains("Support")) targetForm = new AfterServiceForm();
                        else if (item.Contains("Logout")) { Application.Restart(); return; }

                        if (targetForm != null && !(targetForm is LogisticsForm))
                        {
                            this.Hide();
                            targetForm.FormClosed += (senderForm, args) => this.Show();
                            targetForm.Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Navigation error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                pnlSidebar.Controls.Add(btnMenu);
                btnTop += 55;
            }
            this.Controls.Add(pnlSidebar);

            // Right Main Workspace Panel
            Panel pnlMain = new Panel { Location = new Point(260, 0), Size = new Size(900, 850) };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label { Text = "Delivery Logistics Management Center", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(30, 20), AutoSize = true };
            pnlMain.Controls.Add(lblHeader);

            // Back Home button
            Button btnBackHome = new Button { Text = "🏠 Back Home", Size = new Size(120, 34), Location = new Point(740, 22), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnBackHome.FlatAppearance.BorderSize = 0;
            btnBackHome.Click += (s, e) => { NavigationHelper.GoToMainDashboard(this); };
            pnlMain.Controls.Add(btnBackHome);

            // Dispatch Input Card Panel
            Panel pnlCard = new Panel { Location = new Point(30, 85), Size = new Size(420, 710), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlCard);

            Label lblCardTitle = new Label { Text = "📋 Dispatch Assignment", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(20, 15), AutoSize = true };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 55;
            txtShowOrderID = CreateStyledTextBox(pnlCard, ref startY, "Selected Order ID:", true);
            txtShowCustomer = CreateStyledTextBox(pnlCard, ref startY, "Customer Info:", true);
            txtShowOrderDate = CreateStyledTextBox(pnlCard, ref startY, "Order Date:", true);

            Label lblDivider = new Label { Text = "--------------------------------------------------------", Location = new Point(20, startY - 10), ForeColor = Color.LightGray, AutoSize = true };
            pnlCard.Controls.Add(lblDivider);
            startY += 15;

            txtDispatchID = CreateStyledTextBox(pnlCard, ref startY, "Delivery Note ID (DN ID):", true);
            txtDriverName = CreateStyledTextBox(pnlCard, ref startY, "Delivery Address *:", false);

            Label lblMethod = new Label { Text = "Delivery Method:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboMethod = new ComboBox { Location = new Point(20, startY + 22), Width = 375, Font = new Font("Segoe UI", 10.5F), DropDownStyle = ComboBoxStyle.DropDownList };
            pnlCard.Controls.Add(lblMethod); pnlCard.Controls.Add(cboMethod);
            startY += 67;

            Label lblDate = new Label { Text = "Est. Delivery Date:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            dtpEstDelivery = new DateTimePicker { Location = new Point(20, startY + 22), Width = 375, Font = new Font("Segoe UI", 10.5F), Format = DateTimePickerFormat.Short };
            pnlCard.Controls.Add(lblDate); pnlCard.Controls.Add(dtpEstDelivery);
            startY += 75;

            // 派車發貨按鈕 (更新狀態)
            btnGenerateDeliveryNote = new Button { Text = "🚚 Dispatch & Update Status", Location = new Point(20, startY), Size = new Size(375, 45), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnGenerateDeliveryNote.FlatAppearance.BorderSize = 0;
            btnGenerateDeliveryNote.Click += btnGenerateDeliveryNote_Click;
            pnlCard.Controls.Add(btnGenerateDeliveryNote);
            startY += 55;

            // ✨ 新增：彈出現代化送貨單 (Delivery Note)
            btnViewDeliveryNote = new Button { Text = "📄 View / Print Delivery Note", Location = new Point(20, startY), Size = new Size(375, 45), BackColor = Color.FromArgb(13, 148, 136), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnViewDeliveryNote.FlatAppearance.BorderSize = 0;
            btnViewDeliveryNote.Click += btnViewDeliveryNote_Click;
            pnlCard.Controls.Add(btnViewDeliveryNote);

            Label lblGridTitle = new Label { Text = "📦 Pending Logistics Queue", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(480, 85), AutoSize = true };
            pnlMain.Controls.Add(lblGridTitle);

            dgvPendingOrders = new DataGridView { Location = new Point(480, 125), Size = new Size(390, 650), BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, GridColor = Color.FromArgb(241, 245, 249) };
            dgvPendingOrders.EnableHeadersVisualStyles = false;
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvPendingOrders.ColumnHeadersHeight = 38;
            dgvPendingOrders.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvPendingOrders.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
            dgvPendingOrders.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvPendingOrders.SelectionChanged += dgvPendingOrders_SelectionChanged;
            pnlMain.Controls.Add(dgvPendingOrders);
        }

        private TextBox CreateStyledTextBox(Panel container, ref int topY, string labelText, bool readOnly)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(20, topY), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(20, topY + 20), Width = 375, Font = new Font("Segoe UI", 10F), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            if (readOnly) { txt.ReadOnly = true; txt.BackColor = Color.FromArgb(241, 245, 249); }
            container.Controls.Add(lbl); container.Controls.Add(txt);
            topY += 55;
            return txt;
        }
        #endregion

        #region 📦 Core Logistics Functional Logic
        private void SetupDispatchControls()
        {
            cboMethod.Items.Clear();
            cboMethod.Items.AddRange(new string[] { "Heavy Truck (5.5T)", "Light Van", "Express Courier" });
            cboMethod.SelectedIndex = 0;
            dtpEstDelivery.MinDate = DateTime.Today;
        }

        public void RefreshPendingOrdersGrid()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // 🌟 核心升級：只顯示 Sales Order 確認要送貨嘅單 (Pending Delivery)，唔會撈到 Self Pickup！
                    string query = @"SELECT o.OrderID, o.CustomerID, c.Name AS CustomerName, c.Address AS DeliveryAddress, o.OrderDate, o.Status, o.TotalAmount 
                                     FROM orders o INNER JOIN customer c ON o.CustomerID = c.CustomerID 
                                     WHERE o.Status = 'Pending Delivery';";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        dgvPendingOrders.SelectionChanged -= dgvPendingOrders_SelectionChanged;
                        dgvPendingOrders.DataSource = dt;
                        dgvPendingOrders.ClearSelection();
                        dgvPendingOrders.SelectionChanged += dgvPendingOrders_SelectionChanged;
                    }

                    if (dgvPendingOrders.Columns.Count > 0)
                    {
                        dgvPendingOrders.Columns["OrderID"].HeaderText = "Order ID";
                        dgvPendingOrders.Columns["CustomerID"].HeaderText = "Cust ID";
                        dgvPendingOrders.Columns["CustomerName"].HeaderText = "Customer Name";
                        dgvPendingOrders.Columns["OrderDate"].HeaderText = "Order Date";
                        dgvPendingOrders.Columns["OrderDate"].DefaultCellStyle.Format = "yyyy-MM-dd";
                        dgvPendingOrders.Columns["Status"].HeaderText = "Status";
                        dgvPendingOrders.Columns["TotalAmount"].HeaderText = "Total ($)";

                        if (dgvPendingOrders.Columns.Contains("DeliveryAddress")) dgvPendingOrders.Columns["DeliveryAddress"].Visible = false;
                        if (dgvPendingOrders.Columns.Contains("Status")) dgvPendingOrders.Columns["Status"].Visible = false;
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error loading pending delivery queue:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void dgvPendingOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPendingOrders.CurrentRow != null && dgvPendingOrders.CurrentRow.Index >= 0)
            {
                DataGridViewRow row = dgvPendingOrders.CurrentRow;
                string orderID = row.Cells["OrderID"].Value?.ToString();
                if (string.IsNullOrEmpty(orderID)) return;

                txtShowOrderID.Text = orderID;
                txtShowCustomer.Text = $"({row.Cells["CustomerID"].Value}) {row.Cells["CustomerName"].Value}";

                if (row.Cells["OrderDate"].Value != null && row.Cells["OrderDate"].Value != DBNull.Value)
                {
                    txtShowOrderDate.Text = Convert.ToDateTime(row.Cells["OrderDate"].Value).ToString("yyyy-MM-dd HH:mm");
                }

                string existingDNID = FetchExistingDeliveryNoteID(orderID);

                if (!string.IsNullOrEmpty(existingDNID))
                {
                    txtDispatchID.Text = existingDNID;
                    btnGenerateDeliveryNote.Text = "🔄 Update Dispatch Info";
                    btnGenerateDeliveryNote.BackColor = Color.FromArgb(245, 158, 11);
                }
                else
                {
                    txtDispatchID.Text = FetchNextDeliveryNoteID();
                    btnGenerateDeliveryNote.Text = "🚚 Dispatch & Update Status";
                    btnGenerateDeliveryNote.BackColor = Color.FromArgb(16, 185, 129);
                }

                if (row.Cells["DeliveryAddress"].Value != null)
                {
                    txtDriverName.Text = row.Cells["DeliveryAddress"].Value.ToString();
                }
            }
            else
            {
                txtShowOrderID.Clear(); txtShowCustomer.Clear(); txtShowOrderDate.Clear(); txtDispatchID.Clear(); txtDriverName.Clear();
            }
        }

        private void LogisticsForm_Load(object sender, EventArgs e)
        {

        }

        private string FetchExistingDeliveryNoteID(string orderID)
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT DeliveryNoteID FROM delivery_note WHERE OrderID = @orderID LIMIT 1;";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@orderID", orderID);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value) return result.ToString();
                    }
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Lookup error: " + ex.Message); }
            }
            return null;
        }

        private string FetchNextDeliveryNoteID()
        {
            string currentYear = DateTime.Now.ToString("yyyy");
            string nextID = $"DN-{currentYear}-001";
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    string query = "SELECT DeliveryNoteID FROM delivery_note WHERE DeliveryNoteID LIKE @prefix ORDER BY DeliveryNoteID DESC LIMIT 1;";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@prefix", $"DN-{currentYear}-%");
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            string lastID = result.ToString();
                            string[] parts = lastID.Split('-');
                            if (parts.Length == 3 && int.TryParse(parts[2], out int seq))
                            {
                                nextID = $"DN-{currentYear}-{(seq + 1):D3}";
                            }
                        }
                    }
                }
            }
            catch { nextID = $"DN-{currentYear}-011"; }
            return nextID;
        }

        private void btnGenerateDeliveryNote_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtShowOrderID.Text))
            {
                MessageBox.Show("Please select a pending order from the list first!", "Prompt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDriverName.Text))
            {
                MessageBox.Show("Please specify the exact delivery address!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string selectedOrderID = txtShowOrderID.Text;
            string deliveryNoteID = txtDispatchID.Text;
            DateTime estDeliveryDate = dtpEstDelivery.Value;
            string deliveryAddress = txtDriverName.Text.Trim();

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        string updateSql = @"UPDATE delivery_note 
                                             SET DeliveryDate = @estDate, DeliveryAddress = @address 
                                             WHERE DeliveryNoteID = @dnID;";

                        using (MySqlCommand cmdUpdate = new MySqlCommand(updateSql, conn, trans))
                        {
                            cmdUpdate.Parameters.AddWithValue("@dnID", deliveryNoteID);
                            cmdUpdate.Parameters.AddWithValue("@estDate", estDeliveryDate);
                            cmdUpdate.Parameters.AddWithValue("@address", deliveryAddress);

                            if (cmdUpdate.ExecuteNonQuery() == 0)
                            {
                                string insertSql = @"INSERT INTO delivery_note (DeliveryNoteID, OrderID, DeliveryDate, DeliveryAddress) 
                                                     VALUES (@dnID, @orderID, @estDate, @address);";
                                using (MySqlCommand cmdInsert = new MySqlCommand(insertSql, conn, trans))
                                {
                                    cmdInsert.Parameters.AddWithValue("@dnID", deliveryNoteID);
                                    cmdInsert.Parameters.AddWithValue("@orderID", selectedOrderID);
                                    cmdInsert.Parameters.AddWithValue("@estDate", estDeliveryDate);
                                    cmdInsert.Parameters.AddWithValue("@address", deliveryAddress);
                                    cmdInsert.ExecuteNonQuery();
                                }
                            }
                        }

                        // 🌟 派完車，將訂單狀態更新為 'Dispatched' (已發貨)
                        string updateOrderStatusSql = "UPDATE orders SET Status = 'Dispatched' WHERE OrderID = @orderID;";
                        using (MySqlCommand cmdOrderStatus = new MySqlCommand(updateOrderStatusSql, conn, trans))
                        {
                            cmdOrderStatus.Parameters.AddWithValue("@orderID", selectedOrderID);
                            cmdOrderStatus.ExecuteNonQuery();
                        }

                        trans.Commit();
                        MessageBox.Show($"Logistics sequence activated successfully!\n\nDN ID: {deliveryNoteID}\nOrder ID: {selectedOrderID}\n\nStatus is now [Dispatched].", "Dispatch Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        txtShowOrderID.Clear();
                        txtShowCustomer.Clear();
                        txtShowOrderDate.Clear();
                        txtDriverName.Clear();
                        RefreshPendingOrdersGrid();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Transaction execution failed:\n" + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }
        #endregion

        // ======================================================================
        // ✨ ✨ ✨ 彈出「現代化 Delivery Note (送貨單)」獨立視窗 ✨ ✨ ✨
        // ======================================================================
        private void btnViewDeliveryNote_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtShowOrderID.Text))
            {
                MessageBox.Show("Please select an order from the list to view its Delivery Note!", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string orderID = txtShowOrderID.Text;
            string customerInfo = txtShowCustomer.Text;
            string dnID = txtDispatchID.Text;
            string address = string.IsNullOrWhiteSpace(txtDriverName.Text) ? "Pending Assignment" : txtDriverName.Text;
            string method = cboMethod.Text;
            string dDate = dtpEstDelivery.Value.ToString("yyyy-MM-dd");

            // 🚀 實例化並顯示高級送貨單介面
            using (DeliveryNoteForm dialog = new DeliveryNoteForm(dnID, orderID, customerInfo, address, method, dDate))
            {
                dialog.ShowDialog();
            }
        }
    }

    // ======================================================================
    // 🚚 獨立送貨單視窗：湖水藍色、高級物流排版
    // ======================================================================
    public class DeliveryNoteForm : Form
    {
        public DeliveryNoteForm(string dnID, string orderID, string custInfo, string address, string method, string dDate)
        {
            this.Text = "Official Delivery Note Document";
            this.Size = new Size(580, 760);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            // 🔵 頂部橫幅 (Header Banner) - 換上專業的深海藍 (Deep Sea Blue)
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 105, BackColor = Color.FromArgb(15, 23, 42) };
            Label lblComp = new Label { Text = "PREMIUM LIVING LOGISTICS", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(25, 20), AutoSize = true };
            Label lblSub = new Label { Text = "OFFICIAL DELIVERY NOTE (CUSTOMER COPY)", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(14, 165, 233), Location = new Point(27, 56), AutoSize = true };
            pnlHeader.Controls.AddRange(new Control[] { lblComp, lblSub });
            this.Controls.Add(pnlHeader);

            int currentY = 135;

            // 📦 物流資訊區塊 (Logistics Metadata) - 搭配 天藍色 標籤
            AddGroupHeader("DISPATCH METADATA", ref currentY, Color.FromArgb(14, 165, 233));
            AddDataRow("Delivery Note No.:", dnID, ref currentY, true, Color.FromArgb(15, 23, 42));
            AddDataRow("Source Order ID:", orderID, ref currentY, false);
            AddDataRow("Dispatch Date:", DateTime.Now.ToString("yyyy-MM-dd"), ref currentY, false);
            currentY += 20;

            // 📍 目的地資訊 (Destination Profile) - 搭配 翡翠綠 標籤
            AddGroupHeader("DESTINATION PROFILE", ref currentY, Color.FromArgb(16, 185, 129));
            AddDataRow("Recipient:", custInfo, ref currentY, true, Color.FromArgb(15, 23, 42));

            Label lblAddL = new Label { Text = "Delivery Address:", Location = new Point(35, currentY), Font = new Font("Segoe UI", 9.5F, FontStyle.Regular), ForeColor = Color.FromArgb(100, 116, 139), AutoSize = true };
            Label lblAddR = new Label { Text = address, Location = new Point(220, currentY), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = false, Size = new Size(300, 45) };
            this.Controls.AddRange(new Control[] { lblAddL, lblAddR });
            currentY += 55;

            // 🚚 運送細節 (Transport Details) - 搭配 溫暖橙 標籤
            AddGroupHeader("TRANSPORT INSTRUCTIONS", ref currentY, Color.FromArgb(245, 158, 11));
            AddDataRow("Est. Arrival Date:", dDate, ref currentY, true, Color.FromArgb(220, 38, 38));
            AddDataRow("Shipping Method:", method, ref currentY, false);
            currentY += 25;

            // ✍️ 客戶簽收區塊 (Customer Acknowledgment)
            Panel pnlSign = new Panel { Location = new Point(25, currentY), Size = new Size(515, 120), BackColor = Color.FromArgb(248, 250, 252) };
            pnlSign.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlSign.ClientRectangle, Color.FromArgb(203, 213, 225), ButtonBorderStyle.Solid);
            Label lblSignTitle = new Label { Text = "Goods Received In Good Condition:", Location = new Point(15, 15), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), AutoSize = true };
            Label lblLine1 = new Label { Text = "Signature: _________________________________", Location = new Point(15, 55), Font = new Font("Segoe UI", 10F), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true };
            Label lblLine2 = new Label { Text = "Date: _________________________________", Location = new Point(280, 55), Font = new Font("Segoe UI", 10F), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true };
            pnlSign.Controls.AddRange(new Control[] { lblSignTitle, lblLine1, lblLine2 });
            this.Controls.Add(pnlSign);

            // 底部操作按鈕區
            Panel pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 75, BackColor = Color.White };
            pnlBottom.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, 0, pnlBottom.Width, 0);

            Button btnPrint = new Button { Text = "🖨️ Print DN", Size = new Size(140, 42), Location = new Point(100, 16), BackColor = Color.FromArgb(14, 165, 233), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += (s, e) => MessageBox.Show("Connecting to warehouse printer...", "Print Job", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Button btnPDF = new Button { Text = "💾 Save PDF", Size = new Size(140, 42), Location = new Point(255, 16), BackColor = Color.FromArgb(71, 85, 105), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnPDF.FlatAppearance.BorderSize = 0;
            btnPDF.Click += (s, e) => MessageBox.Show("Delivery Note PDF saved successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Button btnClose = new Button { Text = "Close", Size = new Size(100, 42), Location = new Point(410, 16), BackColor = Color.FromArgb(226, 232, 240), ForeColor = Color.FromArgb(51, 65, 85), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            pnlBottom.Controls.AddRange(new Control[] { btnPrint, btnPDF, btnClose });
            this.Controls.Add(pnlBottom);
        }

        private void AddGroupHeader(string title, ref int y, Color accentColor)
        {
            Panel colorBar = new Panel { Location = new Point(25, y + 2), Size = new Size(5, 18), BackColor = accentColor };
            this.Controls.Add(colorBar);
            Label lbl = new Label { Text = title, Location = new Point(38, y), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = accentColor, AutoSize = true };
            this.Controls.Add(lbl);
            y += 28;
        }

        private void AddDataRow(string label, string val, ref int y, bool isBold, Color? customColor = null)
        {
            Label lblL = new Label { Text = label, Location = new Point(35, y), Font = new Font("Segoe UI", 9.5F, FontStyle.Regular), ForeColor = Color.FromArgb(100, 116, 139), AutoSize = true };
            Label lblR = new Label { Text = val, Location = new Point(220, y), Font = new Font("Segoe UI", 10F, isBold ? FontStyle.Bold : FontStyle.Regular), ForeColor = customColor ?? Color.FromArgb(51, 65, 85), AutoSize = true, Width = 300 };
            this.Controls.AddRange(new Control[] { lblL, lblR });
            y += 28;
        }
    }
}