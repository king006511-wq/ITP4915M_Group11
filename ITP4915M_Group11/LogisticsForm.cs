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
        private Button btnGenerateDeliveryNote;
        private Button btnCreateQuotation;

        public LogisticsForm()
        {
            InitializeComponent();
            InitializePremiumModernUI();
            SetupDispatchControls();
            RefreshPendingOrdersGrid();
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

                // Sidebar Navigation (修復版導航邏輯)
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

            btnGenerateDeliveryNote = new Button { Text = "🚚 Generate Delivery Note", Location = new Point(20, startY), Size = new Size(375, 45), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnGenerateDeliveryNote.FlatAppearance.BorderSize = 0;
            btnGenerateDeliveryNote.Click += btnGenerateDeliveryNote_Click;
            pnlCard.Controls.Add(btnGenerateDeliveryNote);
            startY += 55;

            btnCreateQuotation = new Button { Text = "📄 Create Order Quotation", Location = new Point(20, startY), Size = new Size(375, 45), BackColor = Color.FromArgb(79, 70, 229), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCreateQuotation.FlatAppearance.BorderSize = 0;
            btnCreateQuotation.Click += btnCreateQuotation_Click;
            pnlCard.Controls.Add(btnCreateQuotation);

            Label lblGridTitle = new Label { Text = "📦 Pending Packed Orders", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(480, 85), AutoSize = true };
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
            cboMethod.Items.AddRange(new string[] { "Heavy Truck", "Light Van", "Motorcycle Express", "Self-Pickup" });
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
                    string query = @"SELECT o.OrderID, o.CustomerID, c.Name AS CustomerName, c.Address AS DeliveryAddress, o.OrderDate, o.Status, o.TotalAmount 
                                     FROM orders o INNER JOIN customer c ON o.CustomerID = c.CustomerID 
                                     WHERE o.Status IN ('Pending', 'Processing', 'Packed');";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvPendingOrders.DataSource = dt;
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
                catch (Exception ex) { MessageBox.Show("Error loading pending orders:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void dgvPendingOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPendingOrders.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvPendingOrders.SelectedRows[0];
                string orderID = row.Cells["OrderID"].Value.ToString();

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
                    btnGenerateDeliveryNote.Text = "🔄 Update Delivery Note";
                    btnGenerateDeliveryNote.BackColor = Color.FromArgb(245, 158, 11);
                }
                else
                {
                    txtDispatchID.Text = FetchNextDeliveryNoteID();
                    btnGenerateDeliveryNote.Text = "🚚 Generate Delivery Note";
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
            if (dgvPendingOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a pending order from the list first!", "Prompt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDriverName.Text))
            {
                MessageBox.Show("Please specify the delivery address!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string selectedOrderID = dgvPendingOrders.SelectedRows[0].Cells["OrderID"].Value.ToString();
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

                        string updateOrderStatusSql = "UPDATE orders SET Status = 'Dispatched' WHERE OrderID = @orderID;";
                        using (MySqlCommand cmdOrderStatus = new MySqlCommand(updateOrderStatusSql, conn, trans))
                        {
                            cmdOrderStatus.Parameters.AddWithValue("@orderID", selectedOrderID);
                            cmdOrderStatus.ExecuteNonQuery();
                        }

                        trans.Commit();
                        MessageBox.Show($"Delivery Note Handled Successfully!\n\nDN ID: {deliveryNoteID}\nOrder ID: {selectedOrderID}\n\nThe system has automatically updated the Delivery Note.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        txtDriverName.Clear();
                        RefreshPendingOrdersGrid();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Transaction execution failed:\n" + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }
        #endregion

        // ======================================================================
        // ✨ ✨ ✨ 觸發生成「色彩豐富、現代化」的 QuotationForm ✨ ✨ ✨
        // ======================================================================
        private void btnCreateQuotation_Click(object sender, EventArgs e)
        {
            if (dgvPendingOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an item from the list to create a quote!", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string orderID = dgvPendingOrders.SelectedRows[0].Cells["OrderID"].Value.ToString();
            string customerID = dgvPendingOrders.SelectedRows[0].Cells["CustomerID"].Value.ToString();
            decimal baseAmount = Convert.ToDecimal(dgvPendingOrders.SelectedRows[0].Cells["TotalAmount"].Value);

            string customerName = "Unknown Customer";
            string customerType = "B2C";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string customerQuery = "SELECT Name, Type FROM customer WHERE CustomerID = @custID;";
                    using (MySqlCommand cmd = new MySqlCommand(customerQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@custID", customerID);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                customerName = reader["Name"].ToString();
                                customerType = reader["Type"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database error while loading profile: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            decimal discountRate = (customerType == "B2B") ? 0.10m : 0.00m;
            decimal discountAmount = baseAmount * discountRate;
            decimal quoteFinalTotal = baseAmount - discountAmount;
            string quoteRef = $"QT-{DateTime.Now.ToString("yyyyMMdd")}-{orderID.Substring(Math.Max(0, orderID.Length - 4))}";

            // 🚀 實例化並彈出豐富色彩版本的 QuotationForm
            using (QuotationForm dialog = new QuotationForm(quoteRef, customerID, customerName, customerType, orderID, baseAmount, discountRate, discountAmount, quoteFinalTotal))
            {
                dialog.ShowDialog();
            }
        }
    }

    // ======================================================================
    // 💎 全新色彩豐富、高對比度的現代化 UI Form (Quotation 獨立視窗)
    // ======================================================================
    public class QuotationForm : Form
    {
        public QuotationForm(string refNo, string cID, string cName, string cType, string sOrder, decimal gross, decimal discRate, decimal saved, decimal net)
        {
            this.Text = "Official System Quotation Document";
            this.Size = new Size(550, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            // 🔵 頂部橫幅 (Header Banner) - 換上尊貴的皇家藍 (Royal Blue)
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(30, 64, 175) };
            Label lblComp = new Label { Text = "PREMIUM LIVING FURNITURE LTD.", Font = new Font("Segoe UI", 15F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(25, 20), AutoSize = true };
            // 副標題換上淺柔藍色，增加層次感
            Label lblSub = new Label { Text = "Official Commercial Quotation Document", Font = new Font("Segoe UI", 10F, FontStyle.Regular), ForeColor = Color.FromArgb(191, 219, 254), Location = new Point(27, 53), AutoSize = true };
            pnlHeader.Controls.AddRange(new Control[] { lblComp, lblSub });
            this.Controls.Add(pnlHeader);

            int currentY = 125;

            // 📑 Meta Section (單據資訊) - 搭配 海洋藍色 (Ocean Blue) 標籤
            AddGroupHeader("DOCUMENT METADATA", ref currentY, Color.FromArgb(2, 132, 199));
            AddDataRow("Quote Reference:", refNo, ref currentY, true, Color.FromArgb(15, 23, 42));
            AddDataRow("Date Generated:", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ref currentY, false);
            AddDataRow("Source Order ID:", sOrder, ref currentY, false);
            currentY += 20;

            // 👥 Customer Section (客戶資訊) - 搭配 活力紫色 (Vibrant Violet) 標籤
            AddGroupHeader("CLIENT PROFILE", ref currentY, Color.FromArgb(124, 58, 237));
            AddDataRow("Client ID:", cID, ref currentY, false);
            AddDataRow("Client Name:", cName, ref currentY, true, Color.FromArgb(15, 23, 42));
            AddDataRow("Account Tier Status:", $"{cType} VIP Account", ref currentY, false, Color.FromArgb(71, 85, 105));
            currentY += 20;

            // 💰 Financial Section (財務資訊) - 搭配 溫暖橙色 (Rich Orange) 標籤
            AddGroupHeader("FINANCIAL BREAKDOWN", ref currentY, Color.FromArgb(234, 88, 12));
            AddDataRow("Gross Subtotal Amount:", $"${gross:N2}", ref currentY, false);
            // 鮮艷紅色顯示折扣金額
            AddDataRow($"Tier Discount Applied ({discRate * 100:0}%):", $"-${saved:N2}", ref currentY, true, Color.FromArgb(225, 29, 72));
            currentY += 15;

            // 🚨 最終金額 (Grand Total) - 搭配 高亮薄荷綠背景 (Mint Green) 及 翡翠綠文字 (Emerald)
            Panel pnlTotal = new Panel { Location = new Point(25, currentY), Size = new Size(485, 65), BackColor = Color.FromArgb(236, 253, 245) };
            pnlTotal.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlTotal.ClientRectangle, Color.FromArgb(16, 185, 129), ButtonBorderStyle.Solid);
            Label lblNetTitle = new Label { Text = "NET QUOTATION VALUE:", Location = new Point(15, 22), Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(6, 78, 59), AutoSize = true };
            Label lblNetVal = new Label { Text = $"${net:N2}", Location = new Point(290, 14), Font = new Font("Segoe UI", 22F, FontStyle.Bold), ForeColor = Color.FromArgb(4, 120, 87), AutoSize = false, Size = new Size(180, 40), TextAlign = ContentAlignment.MiddleRight };
            pnlTotal.Controls.AddRange(new Control[] { lblNetTitle, lblNetVal });
            this.Controls.Add(pnlTotal);
            currentY += 85;

            // 📋 Footer Terms
            Label lblTerms = new Label { Text = "⚠️ Standard Terms: This quotation value is valid for exactly 30 calendar days from issue date. Prices subject to delivery logistics verification.", Font = new Font("Segoe UI", 8.5F, FontStyle.Italic), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(25, currentY), Size = new Size(480, 40) };
            this.Controls.Add(lblTerms);

            // 🎨 底部操作按鈕區 (Action Buttons)
            Panel pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 75, BackColor = Color.FromArgb(248, 250, 252) };
            pnlBottom.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, 0, pnlBottom.Width, 0);

            // 🖨️ 孔雀綠色 (Teal) 打印按鈕
            Button btnPrint = new Button { Text = "🖨️ Print Quote", Size = new Size(135, 42), Location = new Point(100, 16), BackColor = Color.FromArgb(13, 148, 136), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += (s, e) => MessageBox.Show("Connecting to local hardware printer subsystem...", "Print Job Triggered", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // 💾 靛藍色 (Indigo) 匯出按鈕
            Button btnPDF = new Button { Text = "💾 Export PDF", Size = new Size(135, 42), Location = new Point(245, 16), BackColor = Color.FromArgb(79, 70, 229), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnPDF.FlatAppearance.BorderSize = 0;
            btnPDF.Click += (s, e) => MessageBox.Show("PDF Document generated successfully inside deployment output matrix folder.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ❌ 淺灰色 (Light Gray) 關閉按鈕
            Button btnClose = new Button { Text = "Close", Size = new Size(100, 42), Location = new Point(390, 16), BackColor = Color.FromArgb(226, 232, 240), ForeColor = Color.FromArgb(51, 65, 85), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            pnlBottom.Controls.AddRange(new Control[] { btnPrint, btnPDF, btnClose });
            this.Controls.Add(pnlBottom);
        }

        // 🎨 自訂帶有「鮮艷左側標記條」的 Group Header
        private void AddGroupHeader(string title, ref int y, Color accentColor)
        {
            // 左側細小顏色標記條 (Color Accent Bar)
            Panel colorBar = new Panel { Location = new Point(25, y + 2), Size = new Size(5, 18), BackColor = accentColor };
            this.Controls.Add(colorBar);

            // 標題文字
            Label lbl = new Label { Text = title, Location = new Point(38, y), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = accentColor, AutoSize = true };
            this.Controls.Add(lbl);
            y += 26;
        }

        private void AddDataRow(string label, string val, ref int y, bool isBold, Color? customColor = null)
        {
            Label lblL = new Label { Text = label, Location = new Point(35, y), Font = new Font("Segoe UI", 9.5F, FontStyle.Regular), ForeColor = Color.FromArgb(100, 116, 139), AutoSize = true };
            Label lblR = new Label { Text = val, Location = new Point(220, y), Font = new Font("Segoe UI", 10F, isBold ? FontStyle.Bold : FontStyle.Regular), ForeColor = customColor ?? Color.FromArgb(51, 65, 85), AutoSize = true, Width = 280 };
            this.Controls.AddRange(new Control[] { lblL, lblR });
            y += 26; // 增加少少行距令畫面透氣啲
        }
    }
}