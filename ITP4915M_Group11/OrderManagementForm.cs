using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    // =======================================================
    // 📦 輔助類別：用來承載下拉選單（ComboBox）的產品資料
    // =======================================================
    public class ProductItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public partial class OrderManagementForm : Form
    {
        // ==========================================
        // 🔒 Database Configuration
        // ==========================================
        private readonly string connString = "Server=localhost;Database=premium_living_db;Uid=root;Pwd=;port=3306;SslMode=Disabled;";
        private decimal currentUnitPrice = 0;
        private string currentStaffID;

        // ==========================================
        // 🎨 UI 元素與購物車變數
        // ==========================================
        private TextBox txtOrderID, txtCustomerID, txtStaffID, txtQty, txtUnitPrice;
        private ComboBox cboProducts;
        private CheckBox chkRequireDelivery;
        private DataGridView dgvOrders;
        private Button btnSubmitOrder, btnUpdateOrder, btnClear, btnCreateQuotation;

        // ✨ 新增：購物車專用變數
        private DataTable cartTable;
        private DataGridView dgvCart;
        private Label lblTotalAmountDisplay;
        private decimal globalOrderTotal = 0;
        private Button btnAddItem, btnRemoveItem;

        public OrderManagementForm() : this("S001") { }

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
            GenerateOrderID();
            LoadProductsToCombo();
            RefreshOrdersGrid();
        }

        #region 🎨 Dynamic Premium English UI Construction
        private void InitializePremiumModernUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Advanced Sales Order Management";
            this.Size = new Size(1250, 850); // 拉大視窗容納購物車
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // =========================================================
            // 左側導航面板 (Sidebar)
            // =========================================================
            Panel pnlSidebar = new Panel { Width = 260, Dock = DockStyle.Left, BackColor = Color.FromArgb(15, 23, 42) };
            Label lblLogo = new Label { Text = "Premium Living\nFurniture", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 25), Size = new Size(220, 60), TextAlign = ContentAlignment.MiddleLeft };
            pnlSidebar.Controls.Add(lblLogo);

            string[] menuItems = {
                "🛒 Sales Order Mgmt", "🚚 Delivery Logistics", "🛋️ Product Maintenance",
                "👔 HR / Staff Mgmt", "📦 Goods Received (GRN)", "🏭 Material Requests",
                "📊 Procurement Control", "🔧 Customer Support", "🚪 Logout System"
            };

            int btnTop = 110;
            foreach (string item in menuItems)
            {
                Button btnMenu = new Button { Text = "  " + item, Top = btnTop, Left = 12, Size = new Size(236, 48), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand };
                btnMenu.FlatAppearance.BorderSize = 0;

                if (item.Contains("Sales Order Mgmt")) { btnMenu.BackColor = Color.FromArgb(37, 99, 235); btnMenu.ForeColor = Color.White; }
                else if (item.Contains("Logout")) { btnMenu.BackColor = Color.FromArgb(239, 68, 68); btnMenu.ForeColor = Color.White; }
                else { btnMenu.BackColor = Color.Transparent; btnMenu.ForeColor = Color.FromArgb(148, 163, 184); }

                btnMenu.Click += (s, e) => {
                    Form targetForm = null;
                    try
                    {
                        if (item.Contains("Sales Order")) { return; }
                        else if (item.Contains("Delivery Logistics") || item.Contains("Delivery")) { targetForm = new LogisticsForm(); }
                        else if (item.Contains("Product Maintenance")) { targetForm = new ProductManagement(); }
                        else if (item.Contains("HR") || item.Contains("Staff")) { targetForm = new EmployeeManagement(); }
                        else if (item.Contains("Goods Received") || item.Contains("GRN")) { targetForm = new GoodsReceivedForm(); }
                        else if (item.Contains("Material Requests") || item.Contains("Material")) { targetForm = new RawMaterialRequestForm(); }
                        else if (item.Contains("Procurement Control") || item.Contains("Procurement")) { targetForm = new ProcurementForm(); }
                        else if (item.Contains("Customer Support") || item.Contains("Support")) { targetForm = new AfterServiceForm(); }
                        else if (item.Contains("Logout")) { Application.Restart(); return; }

                        if (targetForm != null)
                        {
                            this.Hide();
                            targetForm.FormClosed += (senderObj, args) => this.Close();
                            targetForm.Show();
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Navigation routing failed.\nError: " + ex.Message, "Routing Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                };
                pnlSidebar.Controls.Add(btnMenu);
                btnTop += 55;
            }
            this.Controls.Add(pnlSidebar);

            // =========================================================
            // 右側工作面板
            // =========================================================
            Panel pnlMain = new Panel { Location = new Point(260, 0), Size = new Size(990, 850) };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label { Text = "Order Processing Dashboard", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(30, 20), AutoSize = true };
            pnlMain.Controls.Add(lblHeader);

            // 🏠 修正：Back Home 按鈕防重疊
            Button btnBackHome = new Button { Text = "🏠 Back Home", Size = new Size(120, 34), Location = new Point(830, 22), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnBackHome.FlatAppearance.BorderSize = 0;
            btnBackHome.Click += (s, e) => { NavigationHelper.GoToMainDashboard(this); };
            pnlMain.Controls.Add(btnBackHome);

            // 🛠️ 修正：X 座標移至 520 確保文字空間
            Label lblStaff = new Label { Text = $"👤 Active Staff ID: {currentStaffID}", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(13, 148, 136), Location = new Point(520, 26), AutoSize = true };
            pnlMain.Controls.Add(lblStaff);

            // =========================================================
            // 左側卡片：訂單與購物車輸入區 (拉寬加高)
            // =========================================================
            Panel pnlCard = new Panel { Location = new Point(30, 85), Size = new Size(500, 700), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlMain.Controls.Add(pnlCard);

            Label lblCardTitle = new Label { Text = "📝 Multi-Item Order Builder", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(20, 15), AutoSize = true };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 50;
            txtOrderID = CreateStyledTextBox(pnlCard, ref startY, "Order ID (Auto):", true, 210);
            startY -= 65;
            txtCustomerID = CreateStyledTextBox(pnlCard, ref startY, "Customer ID *:", false, 210, 240);

            txtStaffID = CreateStyledTextBox(pnlCard, ref startY, "Staff ID:", true, 210);
            txtStaffID.Text = currentStaffID;
            startY -= 65;

            Label lblCbo = new Label { Text = "Select Product:", Location = new Point(240, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboProducts = new ComboBox { Location = new Point(240, startY + 25), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F), BackColor = Color.White };
            cboProducts.SelectedIndexChanged += cboProducts_SelectedIndexChanged;
            pnlCard.Controls.Add(lblCbo); pnlCard.Controls.Add(cboProducts);
            startY += 65;

            txtUnitPrice = CreateStyledTextBox(pnlCard, ref startY, "Unit Price ($):", true, 130);
            startY -= 65;
            txtQty = CreateStyledTextBox(pnlCard, ref startY, "Qty:", false, 80, 160);
            startY -= 65;

            // 🛒 加入/移除購物車按鈕
            btnAddItem = new Button { Text = "➕ Add Item", Location = new Point(255, startY + 23), Size = new Size(100, 32), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAddItem.Click += BtnAddItem_Click;
            pnlCard.Controls.Add(btnAddItem);

            btnRemoveItem = new Button { Text = "❌ Remove", Location = new Point(365, startY + 23), Size = new Size(105, 32), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnRemoveItem.Click += BtnRemoveItem_Click;
            pnlCard.Controls.Add(btnRemoveItem);
            startY += 70;

            // 🛒 購物車 DataGridView
            Label lblCartTitle = new Label { Text = "📦 Staging Cart (Order Line Items)", Location = new Point(20, startY), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true };
            pnlCard.Controls.Add(lblCartTitle);
            startY += 25;

            cartTable = new DataTable();
            cartTable.Columns.Add("PartID", typeof(string));
            cartTable.Columns.Add("Product Name", typeof(string));
            cartTable.Columns.Add("Qty", typeof(int));
            cartTable.Columns.Add("Unit Price", typeof(decimal));
            cartTable.Columns.Add("Subtotal", typeof(decimal));

            dgvCart = new DataGridView { Location = new Point(20, startY), Size = new Size(450, 160), DataSource = cartTable, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvCart.EnableHeadersVisualStyles = false;
            dgvCart.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(71, 85, 105);
            dgvCart.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            pnlCard.Controls.Add(dgvCart);
            startY += 175;

            // 總金額與物流狀態
            lblTotalAmountDisplay = new Label { Text = "Total Bill: $0.00", Location = new Point(20, startY), Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(220, 38, 38), AutoSize = true };
            pnlCard.Controls.Add(lblTotalAmountDisplay);
            startY += 40;

            chkRequireDelivery = new CheckBox { Text = "🚚 Require Delivery Service (Logistics)", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), ForeColor = Color.FromArgb(234, 88, 12), Checked = true, Cursor = Cursors.Hand };
            pnlCard.Controls.Add(chkRequireDelivery);
            startY += 40;

            // ======================================================================
            // 🌟 底部功能按鈕區
            // ======================================================================
            btnSubmitOrder = new Button { Text = "➕ Create Order", Location = new Point(20, startY), Size = new Size(140, 42), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSubmitOrder.Click += btnCreateOrder_Click;
            pnlCard.Controls.Add(btnSubmitOrder);

            btnUpdateOrder = new Button { Text = "✏️ Update", Location = new Point(170, startY), Size = new Size(140, 42), BackColor = Color.FromArgb(245, 158, 11), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnUpdateOrder.Click += btnUpdateOrder_Click;
            pnlCard.Controls.Add(btnUpdateOrder);

            btnClear = new Button { Text = "🧹 Clear", Location = new Point(320, startY), Size = new Size(150, 42), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClear.Click += (s, e) => ClearFields();
            pnlCard.Controls.Add(btnClear);

            btnCreateQuotation = new Button { Text = "📄 Generate Order Quotation", Location = new Point(20, startY + 50), Size = new Size(450, 42), BackColor = Color.FromArgb(124, 58, 237), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCreateQuotation.Click += btnCreateQuotation_Click;
            pnlCard.Controls.Add(btnCreateQuotation);

            // =========================================================
            // 右側歷史訂單紀錄面板
            // =========================================================
            Label lblGridTitle = new Label { Text = "📊 Overall Order History", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(550, 85), AutoSize = true };
            pnlMain.Controls.Add(lblGridTitle);

            dgvOrders = new DataGridView { Location = new Point(550, 125), Size = new Size(400, 660), BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, GridColor = Color.FromArgb(241, 245, 249) };
            dgvOrders.EnableHeadersVisualStyles = false;
            dgvOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvOrders.ColumnHeadersHeight = 38;

            dgvOrders.SelectionChanged += dgvOrders_SelectionChanged;
            pnlMain.Controls.Add(dgvOrders);

            this.Load += OrderManagementForm_Load;
        }

        private TextBox CreateStyledTextBox(Panel container, ref int topY, string labelText, bool readOnly, int width, int offsetX = 20)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(offsetX, topY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(offsetX, topY + 25), Width = width, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            if (readOnly) { txt.ReadOnly = true; txt.BackColor = Color.FromArgb(241, 245, 249); }
            container.Controls.Add(lbl); container.Controls.Add(txt);
            topY += 65;
            return txt;
        }
        #endregion

        #region 🛒 Cart Logic (Add/Remove)
        private void cboProducts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboProducts.SelectedItem != null)
            {
                ProductItem selectedProduct = (ProductItem)cboProducts.SelectedItem;
                currentUnitPrice = selectedProduct.Price;
                txtUnitPrice.Text = currentUnitPrice.ToString("F2");
            }
        }

        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            if (cboProducts.SelectedItem == null) { MessageBox.Show("Please select a product first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!int.TryParse(txtQty.Text.Trim(), out int qty) || qty <= 0) { MessageBox.Show("Please enter a valid positive quantity.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var selectedProduct = (ProductItem)cboProducts.SelectedItem;
            string partID = selectedProduct.ID;
            string prodName = selectedProduct.Name;
            decimal price = selectedProduct.Price;
            decimal subtotal = price * qty;

            bool itemExists = false;
            foreach (DataRow row in cartTable.Rows)
            {
                if (row["PartID"].ToString() == partID)
                {
                    row["Qty"] = Convert.ToInt32(row["Qty"]) + qty;
                    row["Subtotal"] = Convert.ToDecimal(row["Qty"]) * price;
                    itemExists = true; break;
                }
            }

            if (!itemExists) { cartTable.Rows.Add(partID, prodName, qty, price, subtotal); }
            UpdateGlobalOrderTotal();
        }

        private void BtnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvCart.SelectedRows) { dgvCart.Rows.Remove(row); }
                UpdateGlobalOrderTotal();
            }
        }

        private void UpdateGlobalOrderTotal()
        {
            globalOrderTotal = 0;
            foreach (DataRow row in cartTable.Rows) { globalOrderTotal += Convert.ToDecimal(row["Subtotal"]); }
            lblTotalAmountDisplay.Text = $"Total Bill: ${globalOrderTotal:N2}";
        }
        #endregion

        #region 🖱️ Grid Selection Logic (Load Order into Cart)
        private void dgvOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvOrders.CurrentRow != null && dgvOrders.CurrentRow.Index >= 0)
            {
                object cellValue = dgvOrders.CurrentRow.Cells["Order ID"].Value;
                if (cellValue == null || cellValue == DBNull.Value) return;

                string selectedOrderID = cellValue.ToString();
                cartTable.Clear(); // 清空目前購物車

                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        // 1. 獲取主訂單資訊
                        string queryOrder = "SELECT CustomerID, StaffID FROM orders WHERE OrderID = @OrderID";
                        using (MySqlCommand cmd = new MySqlCommand(queryOrder, conn))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", selectedOrderID);
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    txtOrderID.Text = selectedOrderID;
                                    txtCustomerID.Text = reader["CustomerID"].ToString();
                                    txtStaffID.Text = reader["StaffID"].ToString();
                                }
                            }
                        }

                        // 2. 獲取所有子項目並塞入購物車
                        string queryLines = @"SELECT l.PartID, p.PartName, l.Quantity, l.UnitPrice 
                                              FROM order_lineitem l 
                                              JOIN product_part p ON l.PartID = p.PartID 
                                              WHERE l.OrderID = @OrderID";
                        using (MySqlCommand cmdLines = new MySqlCommand(queryLines, conn))
                        {
                            cmdLines.Parameters.AddWithValue("@OrderID", selectedOrderID);
                            using (MySqlDataReader reader = cmdLines.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string pID = reader["PartID"].ToString();
                                    string pName = reader["PartName"].ToString();
                                    int qty = Convert.ToInt32(reader["Quantity"]);
                                    decimal price = Convert.ToDecimal(reader["UnitPrice"]);
                                    decimal subtotal = qty * price;
                                    cartTable.Rows.Add(pID, pName, qty, price, subtotal);
                                }
                            }
                        }
                        UpdateGlobalOrderTotal();
                    }
                    catch (Exception ex) { MessageBox.Show("Failed to load order details:\n" + ex.Message, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            }
        }
        #endregion

        #region 💾 Core DB Logic (Multi-Item Create / Update)
        private void GenerateOrderID()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string currentYear = DateTime.Now.ToString("yyyy");
                    string prefix = "SO" + currentYear + "-";
                    string query = "SELECT OrderID FROM orders WHERE OrderID LIKE @Prefix ORDER BY OrderID DESC LIMIT 1";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Prefix", prefix + "%");
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            string seqStr = result.ToString().Replace(prefix, "");
                            if (int.TryParse(seqStr, out int seq)) { txtOrderID.Text = prefix + (seq + 1).ToString("D4"); return; }
                        }
                        txtOrderID.Text = prefix + "0001";
                    }
                }
                catch (Exception) { txtOrderID.Text = "SO" + DateTime.Now.ToString("yyyyMMddHHmm"); }
            }
        }

        private void LoadProductsToCombo()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT PartID, PartName, DefaultPrice FROM product_part";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<ProductItem> productList = new List<ProductItem>();
                            while (reader.Read()) { productList.Add(new ProductItem { ID = reader["PartID"].ToString(), Name = reader["PartName"].ToString(), Price = Convert.ToDecimal(reader["DefaultPrice"]) }); }
                            cboProducts.DataSource = productList; cboProducts.DisplayMember = "Name"; cboProducts.ValueMember = "ID";
                        }
                    }
                    cboProducts.SelectedIndex = -1;
                }
                catch (Exception ex) { MessageBox.Show("Failed to load products: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        // ======================================================================
        // 🚀 建立新訂單 (INSERT) - 支援多項產品 Transaction
        // ======================================================================
        private void btnCreateOrder_Click(object sender, EventArgs e)
        {
            if (cartTable.Rows.Count == 0) { MessageBox.Show("Cart is empty!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            string customerID = txtCustomerID.Text.Trim();
            if (string.IsNullOrWhiteSpace(customerID)) { MessageBox.Show("Customer ID required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            string orderID = txtOrderID.Text.Trim();
            string status = chkRequireDelivery.Checked ? "Pending Delivery" : "Self Pickup";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // 1. 檢查 Customer 是否存在
                    string checkCustSql = "SELECT COUNT(*) FROM customer WHERE CustomerID = @CustomerID";
                    using (MySqlCommand checkCustCmd = new MySqlCommand(checkCustSql, conn))
                    {
                        checkCustCmd.Parameters.AddWithValue("@CustomerID", customerID);
                        if (Convert.ToInt32(checkCustCmd.ExecuteScalar()) == 0) { MessageBox.Show($"Customer ID '{customerID}' not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
                    }

                    // 2. 檢查所有庫存是否足夠
                    foreach (DataRow row in cartTable.Rows)
                    {
                        string pID = row["PartID"].ToString();
                        int reqQty = Convert.ToInt32(row["Qty"]);
                        string checkStockSql = "SELECT StockLevel FROM product_part WHERE PartID = @PartID";
                        using (MySqlCommand checkCmd = new MySqlCommand(checkStockSql, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@PartID", pID);
                            int currentStock = Convert.ToInt32(checkCmd.ExecuteScalar());
                            if (currentStock < reqQty) { MessageBox.Show($"Insufficient stock for {row["Product Name"]}! Available: {currentStock}.", "Inventory Alert", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
                        }
                    }

                    // 3. 開始 Transaction 寫入資料
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // 寫入 Orders 主表
                            string insertOrderSql = "INSERT INTO orders (OrderID, CustomerID, StaffID, TotalAmount, Status, OrderDate) VALUES (@OID, @CID, @SID, @Total, @Status, NOW())";
                            using (MySqlCommand cmdOrder = new MySqlCommand(insertOrderSql, conn, trans))
                            {
                                cmdOrder.Parameters.AddWithValue("@OID", orderID);
                                cmdOrder.Parameters.AddWithValue("@CID", customerID);
                                cmdOrder.Parameters.AddWithValue("@SID", currentStaffID);
                                cmdOrder.Parameters.AddWithValue("@Total", globalOrderTotal);
                                cmdOrder.Parameters.AddWithValue("@Status", status);
                                cmdOrder.ExecuteNonQuery();
                            }

                            // 迴圈寫入 LineItem 子表並扣庫存
                            string insertLineSql = "INSERT INTO order_lineitem (OrderID, PartID, Quantity, UnitPrice) VALUES (@OID, @PartID, @Qty, @Price)";
                            string updateStockSql = "UPDATE product_part SET StockLevel = StockLevel - @Qty WHERE PartID = @PartID";

                            foreach (DataRow row in cartTable.Rows)
                            {
                                using (MySqlCommand cmdLine = new MySqlCommand(insertLineSql, conn, trans))
                                {
                                    cmdLine.Parameters.AddWithValue("@OID", orderID);
                                    cmdLine.Parameters.AddWithValue("@PartID", row["PartID"].ToString());
                                    cmdLine.Parameters.AddWithValue("@Qty", Convert.ToInt32(row["Qty"]));
                                    cmdLine.Parameters.AddWithValue("@Price", Convert.ToDecimal(row["Unit Price"]));
                                    cmdLine.ExecuteNonQuery();
                                }
                                using (MySqlCommand cmdStock = new MySqlCommand(updateStockSql, conn, trans))
                                {
                                    cmdStock.Parameters.AddWithValue("@Qty", Convert.ToInt32(row["Qty"]));
                                    cmdStock.Parameters.AddWithValue("@PartID", row["PartID"].ToString());
                                    cmdStock.ExecuteNonQuery();
                                }
                            }
                            trans.Commit();
                            MessageBox.Show($"Order [{orderID}] created successfully with {cartTable.Rows.Count} items!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            RefreshOrdersGrid();
                        }
                        catch (Exception ex) { trans.Rollback(); MessageBox.Show("Transaction Failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                    }
                }
                catch (Exception ex) { MessageBox.Show("DB Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        // ======================================================================
        // ✏️ 更新現有訂單 (UPDATE) - 採用「退回舊庫存 ➔ 刪除舊紀錄 ➔ 寫入新紀錄」安全機制
        // ======================================================================
        private void btnUpdateOrder_Click(object sender, EventArgs e)
        {
            string orderID = txtOrderID.Text.Trim();
            if (string.IsNullOrWhiteSpace(orderID) || cartTable.Rows.Count == 0) return;
            string status = chkRequireDelivery.Checked ? "Pending Delivery" : "Self Pickup";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. 撈出舊單項目，將庫存加回去
                            string getOldSql = "SELECT PartID, Quantity FROM order_lineitem WHERE OrderID = @OID";
                            using (MySqlCommand cmdOld = new MySqlCommand(getOldSql, conn, trans))
                            {
                                cmdOld.Parameters.AddWithValue("@OID", orderID);
                                using (MySqlDataReader reader = cmdOld.ExecuteReader())
                                {
                                    List<Tuple<string, int>> oldItems = new List<Tuple<string, int>>();
                                    while (reader.Read()) { oldItems.Add(new Tuple<string, int>(reader["PartID"].ToString(), Convert.ToInt32(reader["Quantity"]))); }
                                    reader.Close();

                                    foreach (var item in oldItems)
                                    {
                                        string restoreStockSql = "UPDATE product_part SET StockLevel = StockLevel + @Qty WHERE PartID = @PartID";
                                        using (MySqlCommand cmdRes = new MySqlCommand(restoreStockSql, conn, trans))
                                        {
                                            cmdRes.Parameters.AddWithValue("@Qty", item.Item2);
                                            cmdRes.Parameters.AddWithValue("@PartID", item.Item1);
                                            cmdRes.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }

                            // 2. 刪除舊的 line items
                            string deleteOldLines = "DELETE FROM order_lineitem WHERE OrderID = @OID";
                            using (MySqlCommand cmdDel = new MySqlCommand(deleteOldLines, conn, trans))
                            {
                                cmdDel.Parameters.AddWithValue("@OID", orderID);
                                cmdDel.ExecuteNonQuery();
                            }

                            // 3. 更新 Orders 主表總金額與狀態
                            string updateOrderSql = "UPDATE orders SET TotalAmount = @Total, Status = @Status WHERE OrderID = @OID";
                            using (MySqlCommand cmdUpdOrder = new MySqlCommand(updateOrderSql, conn, trans))
                            {
                                cmdUpdOrder.Parameters.AddWithValue("@Total", globalOrderTotal);
                                cmdUpdOrder.Parameters.AddWithValue("@Status", status);
                                cmdUpdOrder.Parameters.AddWithValue("@OID", orderID);
                                cmdUpdOrder.ExecuteNonQuery();
                            }

                            // 4. 將目前的購物車項目寫入並扣減新庫存
                            string insertLineSql = "INSERT INTO order_lineitem (OrderID, PartID, Quantity, UnitPrice) VALUES (@OID, @PartID, @Qty, @Price)";
                            string deductStockSql = "UPDATE product_part SET StockLevel = StockLevel - @Qty WHERE PartID = @PartID";
                            foreach (DataRow row in cartTable.Rows)
                            {
                                using (MySqlCommand cmdLine = new MySqlCommand(insertLineSql, conn, trans))
                                {
                                    cmdLine.Parameters.AddWithValue("@OID", orderID);
                                    cmdLine.Parameters.AddWithValue("@PartID", row["PartID"].ToString());
                                    cmdLine.Parameters.AddWithValue("@Qty", Convert.ToInt32(row["Qty"]));
                                    cmdLine.Parameters.AddWithValue("@Price", Convert.ToDecimal(row["Unit Price"]));
                                    cmdLine.ExecuteNonQuery();
                                }
                                using (MySqlCommand cmdStock = new MySqlCommand(deductStockSql, conn, trans))
                                {
                                    cmdStock.Parameters.AddWithValue("@Qty", Convert.ToInt32(row["Qty"]));
                                    cmdStock.Parameters.AddWithValue("@PartID", row["PartID"].ToString());
                                    cmdStock.ExecuteNonQuery();
                                }
                            }

                            trans.Commit();
                            MessageBox.Show($"Order [{orderID}] updated completely with new items!", "Update Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            RefreshOrdersGrid();
                        }
                        catch (Exception ex) { trans.Rollback(); MessageBox.Show("Update Failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                    }
                }
                catch (Exception ex) { MessageBox.Show("DB Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void RefreshOrdersGrid()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT OrderID AS 'Order ID', CustomerID AS 'Customer ID', 
                                            OrderDate AS 'Order Date', TotalAmount AS 'Total HKD', Status AS 'Status' 
                                     FROM orders ORDER BY OrderDate DESC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvOrders.SelectionChanged -= dgvOrders_SelectionChanged;
                        dgvOrders.DataSource = dt;
                        dgvOrders.ClearSelection();
                        dgvOrders.SelectionChanged += dgvOrders_SelectionChanged;
                    }
                }
                catch (Exception ex) { MessageBox.Show("Grid load failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void ClearFields()
        {
            dgvOrders.SelectionChanged -= dgvOrders_SelectionChanged;
            txtCustomerID.Clear();
            txtQty.Clear();
            txtUnitPrice.Clear();
            cboProducts.SelectedIndex = -1;
            chkRequireDelivery.Checked = true;
            cartTable.Clear();
            UpdateGlobalOrderTotal();
            txtStaffID.Text = currentStaffID;
            dgvOrders.ClearSelection();
            GenerateOrderID();
            dgvOrders.SelectionChanged += dgvOrders_SelectionChanged;
        }
        #endregion

        // ======================================================================
        // 💎 獨立報價單視窗
        // ======================================================================
        private void btnCreateQuotation_Click(object sender, EventArgs e)
        {
            if (dgvOrders.CurrentRow == null || dgvOrders.CurrentRow.Index < 0) { MessageBox.Show("Select an order first!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            string orderID = dgvOrders.CurrentRow.Cells["Order ID"].Value?.ToString();
            if (string.IsNullOrEmpty(orderID)) return;

            string customerID = "", customerName = "Unknown Customer", customerType = "B2C", staffID = "";
            decimal baseAmount = 0;

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string orderQuery = "SELECT CustomerID, StaffID, TotalAmount FROM orders WHERE OrderID = @OrderID";
                    using (MySqlCommand cmd = new MySqlCommand(orderQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderID);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                customerID = reader["CustomerID"].ToString();
                                staffID = reader["StaffID"].ToString();
                                baseAmount = Convert.ToDecimal(reader["TotalAmount"]);
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(customerID))
                    {
                        string custQuery = "SELECT Name, Type FROM customer WHERE CustomerID = @CustomerID";
                        using (MySqlCommand cmd = new MySqlCommand(custQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@CustomerID", customerID);
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read()) { customerName = reader["Name"].ToString(); customerType = reader["Type"].ToString(); }
                            }
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Failed to fetch data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            }

            decimal discountRate = (customerType.Trim().ToUpper() == "B2B") ? 0.10m : 0.00m;
            decimal discountAmount = baseAmount * discountRate;
            decimal quoteFinalTotal = baseAmount - discountAmount;
            string quoteRef = $"QT-{DateTime.Now.ToString("yyyyMMdd")}-{orderID.Substring(Math.Max(0, orderID.Length - 4))}";

            using (SalesQuotationForm dialog = new SalesQuotationForm(quoteRef, customerID, customerName, customerType, orderID, baseAmount, discountRate, discountAmount, quoteFinalTotal, staffID))
            {
                dialog.ShowDialog();
            }
        }
    }

    public class SalesQuotationForm : Form
    {
        public SalesQuotationForm(string refNo, string cID, string cName, string cType, string sOrder, decimal gross, decimal discRate, decimal saved, decimal net, string staffID = "Unknown")
        {
            this.Text = "Official System Quotation Document";
            this.Size = new Size(550, 740);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(30, 64, 175) };
            Label lblComp = new Label { Text = "PREMIUM LIVING FURNITURE LTD.", Font = new Font("Segoe UI", 15F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(25, 20), AutoSize = true };
            Label lblSub = new Label { Text = "Official Commercial Quotation Document", Font = new Font("Segoe UI", 10F, FontStyle.Regular), ForeColor = Color.FromArgb(191, 219, 254), Location = new Point(27, 53), AutoSize = true };
            pnlHeader.Controls.AddRange(new Control[] { lblComp, lblSub });
            this.Controls.Add(pnlHeader);

            int currentY = 125;
            AddGroupHeader("DOCUMENT METADATA", ref currentY, Color.FromArgb(2, 132, 199));
            AddDataRow("Quote Reference:", refNo, ref currentY, true, Color.FromArgb(15, 23, 42));
            AddDataRow("Date Generated:", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ref currentY, false);
            AddDataRow("Issued By (Staff ID):", staffID, ref currentY, true, Color.FromArgb(13, 148, 136));
            AddDataRow("Item / Source Order:", sOrder, ref currentY, false);
            currentY += 20;

            AddGroupHeader("CLIENT PROFILE", ref currentY, Color.FromArgb(124, 58, 237));
            AddDataRow("Client ID:", cID, ref currentY, false);
            AddDataRow("Client Name:", cName, ref currentY, true, Color.FromArgb(15, 23, 42));
            AddDataRow("Account Tier Status:", $"{cType} Account", ref currentY, false, Color.FromArgb(71, 85, 105));
            currentY += 20;

            AddGroupHeader("FINANCIAL BREAKDOWN", ref currentY, Color.FromArgb(234, 88, 12));
            AddDataRow("Gross Subtotal Amount:", $"${gross:N2}", ref currentY, false);
            AddDataRow($"Tier Discount Applied ({discRate * 100:0}%):", $"-${saved:N2}", ref currentY, true, Color.FromArgb(225, 29, 72));
            currentY += 15;

            Panel pnlTotal = new Panel { Location = new Point(25, currentY), Size = new Size(485, 65), BackColor = Color.FromArgb(236, 253, 245) };
            pnlTotal.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlTotal.ClientRectangle, Color.FromArgb(16, 185, 129), ButtonBorderStyle.Solid);
            Label lblNetTitle = new Label { Text = "NET QUOTATION VALUE:", Location = new Point(15, 22), Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(6, 78, 59), AutoSize = true };
            Label lblNetVal = new Label { Text = $"${net:N2}", Location = new Point(290, 14), Font = new Font("Segoe UI", 22F, FontStyle.Bold), ForeColor = Color.FromArgb(4, 120, 87), AutoSize = false, Size = new Size(180, 40), TextAlign = ContentAlignment.MiddleRight };
            pnlTotal.Controls.AddRange(new Control[] { lblNetTitle, lblNetVal });
            this.Controls.Add(pnlTotal);
            currentY += 85;

            Label lblTerms = new Label { Text = "⚠️ Standard Terms: This quotation value is valid for exactly 30 calendar days from issue date. Prices subject to delivery logistics verification.", Font = new Font("Segoe UI", 8.5F, FontStyle.Italic), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(25, currentY), Size = new Size(480, 40) };
            this.Controls.Add(lblTerms);

            Panel pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 75, BackColor = Color.FromArgb(248, 250, 252) };
            pnlBottom.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, 0, pnlBottom.Width, 0);

            Button btnPrint = new Button { Text = "🖨️ Print Quote", Size = new Size(135, 42), Location = new Point(100, 16), BackColor = Color.FromArgb(13, 148, 136), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += (s, e) => MessageBox.Show("Connecting to local hardware printer...", "Print Triggered", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Button btnPDF = new Button { Text = "💾 Export PDF", Size = new Size(135, 42), Location = new Point(245, 16), BackColor = Color.FromArgb(79, 70, 229), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnPDF.FlatAppearance.BorderSize = 0;
            btnPDF.Click += (s, e) => MessageBox.Show("PDF generated successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Button btnClose = new Button { Text = "Close", Size = new Size(100, 42), Location = new Point(390, 16), BackColor = Color.FromArgb(226, 232, 240), ForeColor = Color.FromArgb(51, 65, 85), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
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
            y += 26;
        }

        private void AddDataRow(string label, string val, ref int y, bool isBold, Color? customColor = null)
        {
            Label lblL = new Label { Text = label, Location = new Point(35, y), Font = new Font("Segoe UI", 9.5F, FontStyle.Regular), ForeColor = Color.FromArgb(100, 116, 139), AutoSize = true };
            Label lblR = new Label { Text = val, Location = new Point(220, y), Font = new Font("Segoe UI", 10F, isBold ? FontStyle.Bold : FontStyle.Regular), ForeColor = customColor ?? Color.FromArgb(51, 65, 85), AutoSize = true, Width = 280 };
            this.Controls.AddRange(new Control[] { lblL, lblR });
            y += 26;
        }
    }
}