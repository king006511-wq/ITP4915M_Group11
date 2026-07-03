using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public class ProductItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
    public class OrderCustomerItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }

    public partial class OrderManagementForm : BaseForm
    {
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";
        private decimal currentUnitPrice = 0;
        private string currentStaffID;
        private TextBox txtOrderID, txtStaffUIID, txtQty, txtUnitPrice;
        private ComboBox cboProducts, cboCustomers;
        private CheckBox chkRequireDelivery;
        private DateTimePicker dtpDeliveryDate;
        private DataGridView dgvOrders;
        private Button btnSubmitOrder, btnUpdateOrder, btnClear, btnCreateQuotation, btnCompletePickup;
        private DataTable cartTable;
        private DataGridView dgvCart;
        private Label lblTotalAmountDisplay;
        private decimal globalOrderTotal = 0;
        private Button btnAddItem, btnRemoveItem;

        // 🔍 新增：搜尋功能變數
        private TextBox txtSearch;

        public OrderManagementForm() : this(UserSession.LoggedInStaffID ?? "S001") { }

        public OrderManagementForm(string loggedInStaffID)
        {
            this.currentStaffID = loggedInStaffID;
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                InitializePremiumModernUI();
            }
        }

        private void OrderManagementForm_Load(object sender, EventArgs e)
        {
            ValidateUserSession();
            this.currentStaffID = UserSession.LoggedInStaffID ?? "S001";
            GenerateOrderID();
            LoadCustomersToCombo();
            LoadProductsToCombo();
            RefreshOrdersGrid();
        }

        private void ValidateUserSession()
        {
            if (string.IsNullOrEmpty(UserSession.LoggedInStaffID))
            {
                MessageBox.Show("Security Violation: Access denied. Active user session is required.", "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Enabled = false;
                this.BeginInvoke(new Action(() => this.Close()));
            }
        }

        private void HandleSecureException(Exception ex, string operationContext)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "security_audit.log");
                File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SECURITY-INFO] Context: {operationContext} | Details: {ex}{Environment.NewLine}");
            }
            catch { }
            MessageBox.Show($"A system safety restriction or database error occurred during [{operationContext}].\n\nAll details have been securely filed in the audit log. Please contact the administrator.", "System Protection Framework", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void InitializePremiumModernUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Sales Front-End";
            this.BackColor = Color.FromArgb(241, 245, 249);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Load += OrderManagementForm_Load;

            TableLayoutPanel mainTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent };
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.Controls.Add(mainTable);

            Panel pnlHeader = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            Label lblHeader = new Label { Text = "Sales Order Drafting (Front-End)", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(20, 20), AutoSize = true };
            pnlHeader.Controls.Add(lblHeader);
            Label lblStaff = new Label { Text = $"👤 Sales Rep: {currentStaffID}", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(13, 148, 136), Location = new Point(480, 26), AutoSize = true };
            pnlHeader.Controls.Add(lblStaff);
            mainTable.Controls.Add(pnlHeader, 0, 0);

            TableLayoutPanel contentTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = new Padding(20, 0, 20, 20) };
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 510F));
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainTable.Controls.Add(contentTable, 0, 1);

            Panel pnlCard = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, AutoScroll = true };
            pnlCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            contentTable.Controls.Add(pnlCard, 0, 0);

            // -------------------------------------------------------------
            // 📊 右側網格面板 (加入搜尋欄)
            // -------------------------------------------------------------
            TableLayoutPanel rightTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Margin = new Padding(0) };
            rightTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F)); // 標題
            rightTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F)); // 搜尋區塊
            rightTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // DataGridView

            Label lblGridTitle = new Label { Text = "📊 Order Status Tracking", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true, Dock = DockStyle.Bottom };
            rightTable.Controls.Add(lblGridTitle, 0, 0);

            // 🔍 搜尋區塊 UI (修正：改用 FlowLayoutPanel 自動防撞位)
            FlowLayoutPanel pnlSearch = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Padding = new Padding(0, 5, 0, 0),
                WrapContents = false
            };
            Label lblSearch = new Label
            {
                Text = "🔍 Search (Order / Customer):",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                AutoSize = true,
                Margin = new Padding(0, 3, 10, 0) // 右邊預留 10px 空間
            };
            txtSearch = new TextBox
            {
                Width = 250,
                Font = new Font("Segoe UI", 10.5F),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += txtSearch_TextChanged;

            pnlSearch.Controls.Add(lblSearch);
            pnlSearch.Controls.Add(txtSearch);
            rightTable.Controls.Add(pnlSearch, 0, 1);

            dgvOrders = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvOrders.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            dgvOrders.SelectionChanged += dgvOrders_SelectionChanged;
            rightTable.Controls.Add(dgvOrders, 0, 2);

            contentTable.Controls.Add(rightTable, 2, 0);

            // -------------------------------------------------------------
            // 📝 左側表單面板 UI
            // -------------------------------------------------------------
            Label lblCardTitle = new Label { Text = "📝 Draft New Customer Order", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(20, 15), AutoSize = true };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 50;
            txtOrderID = CreateStyledTextBox(pnlCard, ref startY, "Order ID (Auto):", true, 210);
            startY -= 65;

            Label lblCust = new Label { Text = "Select Customer *:", Location = new Point(240, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboCustomers = new ComboBox { Location = new Point(240, startY + 25), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F), BackColor = Color.White };
            pnlCard.Controls.Add(lblCust);
            pnlCard.Controls.Add(cboCustomers);
            startY += 65;

            txtStaffUIID = CreateStyledTextBox(pnlCard, ref startY, "Staff ID:", true, 210);
            txtStaffUIID.Text = currentStaffID;
            startY -= 65;

            Label lblCbo = new Label { Text = "Select Product:", Location = new Point(240, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboProducts = new ComboBox { Location = new Point(240, startY + 25), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F), BackColor = Color.White };
            cboProducts.SelectedIndexChanged += cboProducts_SelectedIndexChanged;
            pnlCard.Controls.Add(lblCbo);
            pnlCard.Controls.Add(cboProducts);
            startY += 65;

            txtUnitPrice = CreateStyledTextBox(pnlCard, ref startY, "Unit Price ($):", true, 130);
            startY -= 65;
            txtQty = CreateStyledTextBox(pnlCard, ref startY, "Qty:", false, 80, 160);
            startY -= 65;

            btnAddItem = new Button { Text = "➕ Add Item", Location = new Point(255, startY + 23), Size = new Size(100, 32), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnAddItem.Click += BtnAddItem_Click;
            pnlCard.Controls.Add(btnAddItem);

            btnRemoveItem = new Button { Text = "❌ Remove", Location = new Point(365, startY + 23), Size = new Size(105, 32), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnRemoveItem.Click += BtnRemoveItem_Click;
            pnlCard.Controls.Add(btnRemoveItem);
            startY += 70;

            Label lblCartGridTitle = new Label { Text = "📦 Order Cart (Requires Approval Later)", Location = new Point(20, startY), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true };
            pnlCard.Controls.Add(lblCartGridTitle);
            startY += 25;

            cartTable = new DataTable();
            cartTable.Columns.Add("ProductID", typeof(string));
            cartTable.Columns.Add("Product Name", typeof(string));
            cartTable.Columns.Add("Qty", typeof(int));
            cartTable.Columns.Add("Unit Price", typeof(decimal));
            cartTable.Columns.Add("Subtotal", typeof(decimal));

            dgvCart = new DataGridView { Location = new Point(20, startY), Size = new Size(450, 160), DataSource = cartTable, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvCart.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(71, 85, 105);
            dgvCart.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCart.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvCart.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            pnlCard.Controls.Add(dgvCart);
            startY += 175;

            lblTotalAmountDisplay = new Label { Text = "Total Bill: $0.00", Location = new Point(20, startY), Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(220, 38, 38), AutoSize = true };
            pnlCard.Controls.Add(lblTotalAmountDisplay);
            startY += 40;

            chkRequireDelivery = new CheckBox { Text = "🚚 Require Delivery Service (Logistics)", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), ForeColor = Color.FromArgb(234, 88, 12), Checked = true };
            pnlCard.Controls.Add(chkRequireDelivery);
            startY += 30;

            dtpDeliveryDate = new DateTimePicker { Location = new Point(45, startY), Width = 250, Font = new Font("Segoe UI", 10F), Format = DateTimePickerFormat.Long, MinDate = DateTime.Now.AddDays(3) };
            pnlCard.Controls.Add(dtpDeliveryDate);
            startY += 40;

            chkRequireDelivery.CheckedChanged += (s, e) => { dtpDeliveryDate.Visible = chkRequireDelivery.Checked; };

            btnSubmitOrder = new Button { Text = "✅ Submit", Location = new Point(20, startY), Size = new Size(110, 42), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            btnSubmitOrder.Click += btnCreateOrder_Click;
            pnlCard.Controls.Add(btnSubmitOrder);

            btnUpdateOrder = new Button { Text = "✏️ Update", Location = new Point(140, startY), Size = new Size(100, 42), BackColor = Color.FromArgb(245, 158, 11), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            btnUpdateOrder.Click += btnUpdateOrder_Click;
            pnlCard.Controls.Add(btnUpdateOrder);

            btnCompletePickup = new Button { Text = "🛍️ Pickup Done", Location = new Point(250, startY), Size = new Size(130, 42), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            btnCompletePickup.Click += btnCompletePickup_Click;
            pnlCard.Controls.Add(btnCompletePickup);

            btnClear = new Button { Text = "🆕 New", Location = new Point(390, startY), Size = new Size(80, 42), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            btnClear.Click += (s, e) => ClearFields();
            pnlCard.Controls.Add(btnClear);
            startY += 55;

            btnCreateQuotation = new Button { Text = "📄 Export Document to Show Customer", Location = new Point(20, startY), Size = new Size(450, 42), BackColor = Color.FromArgb(124, 58, 237), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold) };
            btnCreateQuotation.Click += BtnCreateQuotation_Click;
            pnlCard.Controls.Add(btnCreateQuotation);
        }

        private TextBox CreateStyledTextBox(Panel container, ref int topY, string labelText, bool readOnly, int width, int offsetX = 20)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(offsetX, topY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(offsetX, topY + 25), Width = width, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            if (readOnly)
            {
                txt.ReadOnly = true;
                txt.BackColor = Color.FromArgb(241, 245, 249);
            }
            container.Controls.Add(lbl);
            container.Controls.Add(txt);
            topY += 65;
            return txt;
        }

        // -------------------------------------------------------------
        // 🔍 實時搜尋過濾邏輯 (修正：簡化 RowFilter)
        // -------------------------------------------------------------
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (dgvOrders.DataSource is DataTable dt)
            {
                // 進行防彈處理，過濾特殊字元避免 DataView RowFilter Crash
                string keyword = txtSearch.Text.Trim().Replace("'", "''")
                                         .Replace("[", "[[]")
                                         .Replace("]", "[]]")
                                         .Replace("*", "[*]")
                                         .Replace("%", "[%]");

                if (string.IsNullOrEmpty(keyword))
                {
                    dt.DefaultView.RowFilter = string.Empty;
                }
                else
                {
                    // 修正：直接用 LIKE 搜尋，避免 Convert 報錯
                    dt.DefaultView.RowFilter = $"[Order ID] LIKE '%{keyword}%' OR [Customer ID] LIKE '%{keyword}%'";
                }
            }
        }

        private void LoadCustomersToCombo()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT CustomerID,Name FROM customer", conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<OrderCustomerItem> list = new List<OrderCustomerItem>();
                        while (reader.Read())
                            list.Add(new OrderCustomerItem { ID = reader["CustomerID"].ToString(), Name = $"{reader["CustomerID"]} - {reader["Name"]}" });
                        cboCustomers.DataSource = list;
                        cboCustomers.DisplayMember = "Name";
                        cboCustomers.ValueMember = "ID";
                        cboCustomers.SelectedIndex = -1;
                    }
                }
                catch (Exception ex)
                {
                    HandleSecureException(ex, "Load Customers List");
                }
            }
        }

        private void LoadProductsToCombo()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT ProductID,ProductName,RetailPrice FROM product", conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<ProductItem> list = new List<ProductItem>();
                        while (reader.Read())
                            list.Add(new ProductItem { ID = reader["ProductID"].ToString(), Name = reader["ProductName"].ToString(), Price = Convert.ToDecimal(reader["RetailPrice"]) });
                        cboProducts.DataSource = list;
                        cboProducts.DisplayMember = "Name";
                        cboProducts.ValueMember = "ID";
                        cboProducts.SelectedIndex = -1;
                    }
                }
                catch (Exception ex)
                {
                    HandleSecureException(ex, "Load Products List");
                }
            }
        }

        private void cboProducts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboProducts.SelectedItem != null)
            {
                currentUnitPrice = ((ProductItem)cboProducts.SelectedItem).Price;
                txtUnitPrice.Text = currentUnitPrice.ToString("F2");
            }
        }

        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            if (cboProducts.SelectedItem == null)
            {
                MessageBox.Show("Select product.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txtQty.Text.Trim(), out int qty) || qty <= 0)
            {
                MessageBox.Show("Invalid quantity value.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (qty > 9999)
            {
                MessageBox.Show("Quantity exceeds allowable systemic boundary limits per item (Max 9,999).", "Security Guardrail", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }

            var prod = (ProductItem)cboProducts.SelectedItem;
            bool exists = false;
            foreach (DataRow row in cartTable.Rows)
            {
                if (row["ProductID"].ToString() == prod.ID)
                {
                    int totalQty = Convert.ToInt32(row["Qty"]) + qty;
                    if (totalQty > 99999)
                    {
                        MessageBox.Show("Accumulated quantity limits exceeded for this line item.", "Security Guardrail", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return;
                    }
                    row["Qty"] = totalQty;
                    row["Subtotal"] = Convert.ToDecimal(row["Qty"]) * prod.Price;
                    exists = true;
                    break;
                }
            }
            if (!exists)
                cartTable.Rows.Add(prod.ID, prod.Name, qty, prod.Price, prod.Price * qty);

            UpdateGlobalOrderTotal();
            txtQty.Clear();
        }

        private void BtnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvCart.SelectedRows)
                {
                    if (!row.IsNewRow) dgvCart.Rows.Remove(row);
                }
                UpdateGlobalOrderTotal();
            }
        }

        private void UpdateGlobalOrderTotal()
        {
            globalOrderTotal = 0;
            foreach (DataRow row in cartTable.Rows)
                globalOrderTotal += Convert.ToDecimal(row["Subtotal"]);
            lblTotalAmountDisplay.Text = $"Total Bill: ${globalOrderTotal:N2}";
        }

        private void dgvOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvOrders.CurrentRow == null || dgvOrders.CurrentRow.Index < 0) return;
            string selectedOrderID = dgvOrders.CurrentRow.Cells["Order ID"].Value.ToString();
            cartTable.Clear();

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT CustomerID,StaffID,Status,DeliveryDate FROM orders WHERE OrderID=@OID", conn))
                    {
                        cmd.Parameters.AddWithValue("@OID", selectedOrderID);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtOrderID.Text = selectedOrderID;
                                txtStaffUIID.Text = reader["StaffID"].ToString();
                                string cID = reader["CustomerID"].ToString();
                                foreach (OrderCustomerItem item in cboCustomers.Items)
                                {
                                    if (item.ID == cID)
                                    {
                                        cboCustomers.SelectedItem = item;
                                        break;
                                    }
                                }
                                string currentStatus = reader["Status"].ToString();
                                chkRequireDelivery.Checked = currentStatus.Contains("-D") || currentStatus.Contains("Dispatch") || currentStatus.Contains("Deliver");
                                try
                                {
                                    if (reader["DeliveryDate"] != DBNull.Value)
                                    {
                                        DateTime delDate = Convert.ToDateTime(reader["DeliveryDate"]);
                                        if (delDate >= dtpDeliveryDate.MinDate) dtpDeliveryDate.Value = delDate;
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    using (MySqlCommand cmdLines = new MySqlCommand("SELECT l.ProductID,p.ProductName,l.Quantity,l.UnitPrice FROM order_lineitem l JOIN product p ON l.ProductID=p.ProductID WHERE l.OrderID=@OID", conn))
                    {
                        cmdLines.Parameters.AddWithValue("@OID", selectedOrderID);
                        using (MySqlDataReader reader = cmdLines.ExecuteReader())
                        {
                            while (reader.Read())
                                cartTable.Rows.Add(reader["ProductID"].ToString(), reader["ProductName"].ToString(), Convert.ToInt32(reader["Quantity"]), Convert.ToDecimal(reader["UnitPrice"]), Convert.ToInt32(reader["Quantity"]) * Convert.ToDecimal(reader["UnitPrice"]));
                        }
                    }
                    UpdateGlobalOrderTotal();
                }
                catch (Exception ex)
                {
                    HandleSecureException(ex, "Select/Retrieve Sales Order Details");
                }
            }
        }

        private void GenerateOrderID()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string prefix = "SO" + DateTime.Now.ToString("yyyy") + "-";
                    using (MySqlCommand cmd = new MySqlCommand("SELECT OrderID FROM orders WHERE OrderID LIKE @Prefix ORDER BY OrderID DESC LIMIT 1", conn))
                    {
                        cmd.Parameters.AddWithValue("@Prefix", prefix + "%");
                        object res = cmd.ExecuteScalar();
                        if (res != null && res != DBNull.Value)
                        {
                            if (int.TryParse(res.ToString().Replace(prefix, ""), out int seq))
                            {
                                txtOrderID.Text = prefix + (seq + 1).ToString("D4");
                                return;
                            }
                        }
                    }
                    txtOrderID.Text = prefix + "0001";
                }
                catch (Exception ex)
                {
                    HandleSecureException(ex, "Automatic Sequence Order Generation");
                    txtOrderID.Text = "SO" + DateTime.Now.ToString("yyyyMMddHHmm");
                }
            }
        }

        private void btnCreateOrder_Click(object sender, EventArgs e)
        {
            if (cboCustomers.SelectedItem == null || cartTable.Rows.Count == 0)
            {
                MessageBox.Show("Customer/Cart cannot be empty.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string orderID = txtOrderID.Text.Trim();
            string orderStatus = chkRequireDelivery.Checked ? "Awaiting Approval-D" : "Awaiting Approval-P";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand checkCmd = new MySqlCommand("SELECT COUNT(*) FROM orders WHERE OrderID=@OID", conn))
                    {
                        checkCmd.Parameters.AddWithValue("@OID", orderID);
                        if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                        {
                            MessageBox.Show("Order already exists. Click Update if changes are needed.", "Data Conflict", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            using (MySqlCommand cmdOrder = new MySqlCommand("INSERT INTO orders (OrderID,CustomerID,StaffID,TotalAmount,Status,OrderDate,DeliveryDate) VALUES (@OID,@CID,@SID,@Total,@Status,NOW(),@DelDate)", conn, trans))
                            {
                                cmdOrder.Parameters.AddWithValue("@OID", orderID);
                                cmdOrder.Parameters.AddWithValue("@CID", ((OrderCustomerItem)cboCustomers.SelectedItem).ID);
                                cmdOrder.Parameters.AddWithValue("@SID", currentStaffID);
                                cmdOrder.Parameters.AddWithValue("@Total", globalOrderTotal);
                                cmdOrder.Parameters.AddWithValue("@Status", orderStatus);
                                cmdOrder.Parameters.AddWithValue("@DelDate", chkRequireDelivery.Checked ? (object)dtpDeliveryDate.Value.Date : DBNull.Value);
                                cmdOrder.ExecuteNonQuery();
                            }
                            using (MySqlCommand cmdLine = new MySqlCommand("INSERT INTO order_lineitem (OrderID,ProductID,Quantity,UnitPrice) VALUES (@OID,@ProductID,@Qty,@Price)", conn, trans))
                            {
                                foreach (DataRow row in cartTable.Rows)
                                {
                                    cmdLine.Parameters.Clear();
                                    cmdLine.Parameters.AddWithValue("@OID", orderID);
                                    cmdLine.Parameters.AddWithValue("@ProductID", row["ProductID"].ToString());
                                    cmdLine.Parameters.AddWithValue("@Qty", Convert.ToInt32(row["Qty"]));
                                    cmdLine.Parameters.AddWithValue("@Price", Convert.ToDecimal(row["Unit Price"]));
                                    cmdLine.ExecuteNonQuery();
                                }
                            }
                            trans.Commit();
                            MessageBox.Show($"Order drafted and sent for Approval!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            RefreshOrdersGrid();
                        }
                        catch (Exception innerEx)
                        {
                            trans.Rollback();
                            throw innerEx;
                        }
                    }
                }
                catch (Exception ex)
                {
                    HandleSecureException(ex, "Execute Create Order Transaction");
                }
            }
        }

        private void btnUpdateOrder_Click(object sender, EventArgs e)
        {
            string orderID = txtOrderID.Text.Trim();
            if (string.IsNullOrEmpty(orderID) || cartTable.Rows.Count == 0 || cboCustomers.SelectedItem == null) return;

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string currentStatus = "";
                    using (MySqlCommand statusCmd = new MySqlCommand("SELECT Status FROM orders WHERE OrderID=@OID", conn))
                    {
                        statusCmd.Parameters.AddWithValue("@OID", orderID);
                        var res = statusCmd.ExecuteScalar();
                        if (res == null)
                        {
                            MessageBox.Show("This is a NEW order ID. Click 'Create Order'.", "Invalid operation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        currentStatus = res.ToString();
                    }
                    if (!currentStatus.StartsWith("Awaiting Approval") && currentStatus != "Rejected")
                    {
                        MessageBox.Show("🔒 This order has already been Approved, Processed, or Completed!\n\nYou cannot modify a locked order.", "Order System Locked", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }

                    string orderStatus = chkRequireDelivery.Checked ? "Awaiting Approval-D" : "Awaiting Approval-P";
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            using (MySqlCommand cmdDel = new MySqlCommand("DELETE FROM order_lineitem WHERE OrderID=@OID", conn, trans))
                            {
                                cmdDel.Parameters.AddWithValue("@OID", orderID);
                                cmdDel.ExecuteNonQuery();
                            }
                            using (MySqlCommand cmdLine = new MySqlCommand("INSERT INTO order_lineitem (OrderID,ProductID,Quantity,UnitPrice) VALUES (@OID,@ProductID,@Qty,@Price)", conn, trans))
                            {
                                foreach (DataRow row in cartTable.Rows)
                                {
                                    cmdLine.Parameters.Clear();
                                    cmdLine.Parameters.AddWithValue("@OID", orderID);
                                    cmdLine.Parameters.AddWithValue("@ProductID", row["ProductID"].ToString());
                                    cmdLine.Parameters.AddWithValue("@Qty", Convert.ToInt32(row["Qty"]));
                                    cmdLine.Parameters.AddWithValue("@Price", Convert.ToDecimal(row["Unit Price"]));
                                    cmdLine.ExecuteNonQuery();
                                }
                            }
                            using (MySqlCommand cmdOrder = new MySqlCommand("UPDATE orders SET CustomerID=@CID,TotalAmount=@Total,Status=@Status,DeliveryDate=@DelDate WHERE OrderID=@OID", conn, trans))
                            {
                                cmdOrder.Parameters.AddWithValue("@CID", ((OrderCustomerItem)cboCustomers.SelectedItem).ID);
                                cmdOrder.Parameters.AddWithValue("@Total", globalOrderTotal);
                                cmdOrder.Parameters.AddWithValue("@Status", orderStatus);
                                cmdOrder.Parameters.AddWithValue("@DelDate", chkRequireDelivery.Checked ? (object)dtpDeliveryDate.Value.Date : DBNull.Value);
                                cmdOrder.Parameters.AddWithValue("@OID", orderID);
                                cmdOrder.ExecuteNonQuery();
                            }
                            trans.Commit();
                            MessageBox.Show($"Order Updated Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            RefreshOrdersGrid();
                        }
                        catch (Exception innerEx)
                        {
                            trans.Rollback();
                            throw innerEx;
                        }
                    }
                }
                catch (Exception ex)
                {
                    HandleSecureException(ex, "Execute Update Order Transaction");
                }
            }
        }

        private void btnCompletePickup_Click(object sender, EventArgs e)
        {
            string orderID = txtOrderID.Text.Trim();
            if (string.IsNullOrEmpty(orderID) || cartTable.Rows.Count == 0) return;
            if (chkRequireDelivery.Checked)
            {
                MessageBox.Show("This is a Delivery order. It cannot be marked as Pickup Completed here (hand over to Logistics).", "Invalid Action", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult res = MessageBox.Show($"Is the customer currently picking up the items?\n\nAre you sure you want to mark order [{orderID}] as 'Pickup Completed'?", "Confirm Completion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("UPDATE orders SET Status='Pickup Completed' WHERE OrderID=@OID", conn))
                        {
                            cmd.Parameters.AddWithValue("@OID", orderID);
                            cmd.ExecuteNonQuery();
                        }
                        MessageBox.Show("Order successfully marked as Pickup Completed! 🛍️", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearFields();
                        RefreshOrdersGrid();
                    }
                    catch (Exception ex)
                    {
                        HandleSecureException(ex, "Update Pickup Completion Status");
                    }
                }
            }
        }

        private void BtnCreateQuotation_Click(object sender, EventArgs e)
        {
            if (cartTable.Rows.Count == 0) return;
            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "HTML Web Document (*.html)|*.html", FileName = $"Order_{txtOrderID.Text}.html" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string safeOrderID = WebUtility.HtmlEncode(txtOrderID.Text);
                        string safeCustomer = cboCustomers.SelectedItem != null ? WebUtility.HtmlEncode(cboCustomers.Text) : "";
                        string safeDate = WebUtility.HtmlEncode(DateTime.Now.ToString("yyyy-MM-dd"));
                        string safeGrandTotal = WebUtility.HtmlEncode(globalOrderTotal.ToString("N2"));

                        string html = $@"<!DOCTYPE html><html><head><meta charset='utf-8'><title>Order Document - {safeOrderID}</title><style>body{{font-family:'Segoe UI',Arial;margin:40px;background:#fff;}}.container{{max-width:800px;margin:0 auto;}}h1{{color:#1e3a8a;}}.items-table{{width:100%;border-collapse:collapse;margin-top:30px;}}.items-table th{{background:#f1f5f9;padding:10px;text-align:left;}}.items-table td{{padding:10px;border-bottom:1px solid #ddd;}}</style></head><body><div class='container'><h1>PREMIUM LIVING</h1><h3>ORDER DOCUMENT</h3><p><b>Order Ref:</b> {safeOrderID}<br><b>Customer:</b> {safeCustomer}<br><b>Date:</b> {safeDate}</p><table class='items-table'><tr><th>Product Name</th><th>Qty</th><th>Unit Price</th><th>Subtotal</th></tr>";

                        foreach (DataRow row in cartTable.Rows)
                        {
                            string safeProdName = WebUtility.HtmlEncode(row["Product Name"].ToString());
                            string safeQty = WebUtility.HtmlEncode(row["Qty"].ToString());
                            string safePrice = WebUtility.HtmlEncode(row["Unit Price"].ToString());
                            string safeSub = WebUtility.HtmlEncode(row["Subtotal"].ToString());
                            html += $"<tr><td>{safeProdName}</td><td>{safeQty}</td><td>${safePrice}</td><td>${safeSub}</td></tr>";
                        }

                        html += $@"</table><h2 style='text-align:right;color:#dc2626;'>Grand Total: ${safeGrandTotal}</h2><div style='margin-top:100px;display:flex;justify-content:space-between;'><div style='width:40%;border-top:2px solid #000;padding-top:10px;'>Customer Signature</div><div style='width:40%;border-top:2px solid #000;padding-top:10px;'>Company Representative</div></div></div></body></html>";

                        File.WriteAllText(sfd.FileName, html, System.Text.Encoding.UTF8);
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        HandleSecureException(ex, "Generate Safe Export HTML Document");
                    }
                }
            }
        }

        private void RefreshOrdersGrid()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT OrderID AS 'Order ID',CustomerID AS 'Customer ID',TotalAmount AS 'Total Bill',Status FROM orders ORDER BY OrderDate DESC", conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvOrders.DataSource = dt;

                        // 確保如果搜尋欄有字，重新整理時都會套用過濾
                        txtSearch_TextChanged(null, null);
                    }
                }
                catch (Exception ex)
                {
                    HandleSecureException(ex, "Refresh Orders List Grid");
                }
            }
        }

        private void ClearFields()
        {
            dgvOrders.ClearSelection();
            if (cboCustomers.Items.Count > 0) cboCustomers.SelectedIndex = -1;
            if (cboProducts.Items.Count > 0) cboProducts.SelectedIndex = -1;
            txtQty.Clear();
            txtUnitPrice.Clear();
            txtSearch.Clear(); // 🧹 清除搜尋字眼
            cartTable.Clear();
            chkRequireDelivery.Checked = true;
            dtpDeliveryDate.Value = dtpDeliveryDate.MinDate;
            UpdateGlobalOrderTotal();
            GenerateOrderID();
        }
    }
}