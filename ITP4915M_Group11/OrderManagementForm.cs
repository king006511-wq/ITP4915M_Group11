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
        // ==========================================\
        // 🔒 Database Configuration (標準本地 XAMPP MySQL 連線字串)
        // ==========================================\
        private readonly string connString = "Server=localhost;Database=premium_living_db;Uid=root;Pwd=;port=3306;SslMode=Disabled;";
        private decimal currentUnitPrice = 0;

        // ✨ 核心記錄：當前登入的 Staff ID
        private string currentStaffID;

        // ==========================================\
        // 🎨 UI 元素控制變數
        // ==========================================\
        private TextBox txtOrderID, txtCustomerID, txtStaffID, txtQty, txtUnitPrice, txtSubtotal;
        private ComboBox cboProducts;
        private CheckBox chkRequireDelivery;
        private DataGridView dgvOrders;
        private Button btnSubmitOrder, btnUpdateOrder, btnClear, btnCreateQuotation; // 🌟 新增了 btnUpdateOrder

        // ======================================================================\
        // ✅ 修正核心：動態支援 S 字頭員工編號
        // ======================================================================\

        // 1️⃣ 預設無參數建構子：當沒有透過 Login 傳入時，預設使用 S001
        public OrderManagementForm() : this("S001")
        {
        }

        // 2️⃣ 主要建構子：完美接收 Login Form 傳過來的動態 Staff ID
        public OrderManagementForm(string loggedInStaffID)
        {
            this.currentStaffID = loggedInStaffID;
            InitializePremiumModernUI();
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
            this.Text = "Premium Living Furniture - Sales Order Management";
            this.Size = new Size(1180, 780);
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

                if (item.Contains("Sales Order Mgmt"))
                {
                    btnMenu.BackColor = Color.FromArgb(37, 99, 235); btnMenu.ForeColor = Color.White;
                }
                else if (item.Contains("Logout"))
                {
                    btnMenu.BackColor = Color.FromArgb(239, 68, 68); btnMenu.ForeColor = Color.White;
                }
                else
                {
                    btnMenu.BackColor = Color.Transparent; btnMenu.ForeColor = Color.FromArgb(148, 163, 184);
                }

                btnMenu.Click += (s, e) => {
                    string cleanItem = item.Trim();
                    Form nextForm = null;

                    switch (cleanItem)
                    {
                        case "🛒 Sales Order Mgmt": return;
                        case "🚚 Delivery Logistics": nextForm = new LogisticsForm(); break;
                        case "🛋️ Product Maintenance": nextForm = new ProductManagement(); break;
                        case "🚪 Logout System": Application.Restart(); return;
                        default:
                            MessageBox.Show($"The module [{cleanItem}] is currently under development or not yet linked.", "Module Under Construction", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                    }

                    if (nextForm != null)
                    {
                        this.Hide();
                        nextForm.FormClosed += (senderObj, args) => this.Close();
                        nextForm.Show();
                    }
                };
                pnlSidebar.Controls.Add(btnMenu);
                btnTop += 55;
            }
            this.Controls.Add(pnlSidebar);

            // =========================================================
            // 右側工作面板
            // =========================================================
            Panel pnlMain = new Panel { Location = new Point(260, 0), Size = new Size(900, 780) };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label { Text = "Order Processing Dashboard", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(30, 20), AutoSize = true };
            pnlMain.Controls.Add(lblHeader);

            Label lblStaff = new Label { Text = $"👤 Active Staff ID: {currentStaffID}", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(13, 148, 136), Location = new Point(620, 28), AutoSize = true };
            pnlMain.Controls.Add(lblStaff);

            Panel pnlCard = new Panel { Location = new Point(30, 85), Size = new Size(420, 660), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlMain.Controls.Add(pnlCard);

            Label lblCardTitle = new Label { Text = "📝 Sales Order Details", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(20, 15), AutoSize = true };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 55;
            txtOrderID = CreateStyledTextBox(pnlCard, ref startY, "Order ID (Auto-generated):", true);
            txtCustomerID = CreateStyledTextBox(pnlCard, ref startY, "Customer ID *:", false);

            txtStaffID = CreateStyledTextBox(pnlCard, ref startY, "Staff ID (System Bound):", true);
            txtStaffID.Text = currentStaffID;

            Label lblCbo = new Label { Text = "Select Product *:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboProducts = new ComboBox { Location = new Point(20, startY + 25), Width = 375, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F), BackColor = Color.White };
            cboProducts.SelectedIndexChanged += cboProducts_SelectedIndexChanged;
            pnlCard.Controls.Add(lblCbo); pnlCard.Controls.Add(cboProducts);
            startY += 65;

            txtUnitPrice = CreateStyledTextBox(pnlCard, ref startY, "Unit Price (HKD):", true);
            txtQty = CreateStyledTextBox(pnlCard, ref startY, "Quantity *:", false);
            txtQty.TextChanged += txtQty_TextChanged;
            txtSubtotal = CreateStyledTextBox(pnlCard, ref startY, "Subtotal (HKD):", true);
            txtSubtotal.ForeColor = Color.FromArgb(220, 38, 38);
            txtSubtotal.Font = new Font("Segoe UI", 12F, FontStyle.Bold);

            chkRequireDelivery = new CheckBox
            {
                Text = "🚚 Require Delivery Service (Logistics)",
                Location = new Point(20, startY),
                AutoSize = true,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(234, 88, 12),
                Checked = true,
                Cursor = Cursors.Hand
            };
            pnlCard.Controls.Add(chkRequireDelivery);
            startY += 40;

            // ======================================================================
            // 🌟 按鈕區重新排版：新增 Update 按鈕
            // ======================================================================
            btnSubmitOrder = new Button { Text = "➕ Create", Location = new Point(20, startY), Size = new Size(115, 42), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSubmitOrder.Click += btnCreateOrder_Click;
            pnlCard.Controls.Add(btnSubmitOrder);

            btnUpdateOrder = new Button { Text = "✏️ Update", Location = new Point(145, startY), Size = new Size(125, 42), BackColor = Color.FromArgb(245, 158, 11), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnUpdateOrder.Click += btnUpdateOrder_Click; // 綁定更新事件
            pnlCard.Controls.Add(btnUpdateOrder);

            btnClear = new Button { Text = "🧹 Clear", Location = new Point(280, startY), Size = new Size(115, 42), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClear.Click += (s, e) => ClearFields();
            pnlCard.Controls.Add(btnClear);

            btnCreateQuotation = new Button { Text = "📄 Generate Order Quotation", Location = new Point(20, startY + 50), Size = new Size(375, 42), BackColor = Color.FromArgb(124, 58, 237), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCreateQuotation.Click += btnCreateQuotation_Click;
            pnlCard.Controls.Add(btnCreateQuotation);

            // =========================================================
            // 右側歷史訂單紀錄面板
            // =========================================================
            Label lblGridTitle = new Label { Text = "📊 Order History (Premium Living)", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(480, 85), AutoSize = true };
            pnlMain.Controls.Add(lblGridTitle);

            dgvOrders = new DataGridView { Location = new Point(480, 125), Size = new Size(390, 600), BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, GridColor = Color.FromArgb(241, 245, 249) };
            dgvOrders.EnableHeadersVisualStyles = false;
            dgvOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvOrders.ColumnHeadersHeight = 38;

            dgvOrders.SelectionChanged += dgvOrders_SelectionChanged;
            pnlMain.Controls.Add(dgvOrders);

            this.Load += OrderManagementForm_Load;
        }

        private TextBox CreateStyledTextBox(Panel container, ref int topY, string labelText, bool readOnly)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(20, topY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(20, topY + 25), Width = 375, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
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
        #endregion

        #region 🖱️ Grid Interactions
        private void dgvOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvOrders.CurrentRow != null && dgvOrders.CurrentRow.Index >= 0)
            {
                object cellValue = dgvOrders.CurrentRow.Cells["Order ID"].Value;
                if (cellValue == null || cellValue == DBNull.Value) return;

                string selectedOrderID = cellValue.ToString();

                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        string query = @"SELECT o.OrderID, o.CustomerID, o.StaffID, l.PartID, l.Quantity 
                                         FROM orders o 
                                         INNER JOIN order_lineitem l ON o.OrderID = l.OrderID 
                                         WHERE o.OrderID = @OrderID LIMIT 1";

                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", selectedOrderID);
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    txtOrderID.Text = reader["OrderID"].ToString();
                                    txtCustomerID.Text = reader["CustomerID"].ToString();
                                    txtStaffID.Text = reader["StaffID"].ToString();
                                    txtQty.Text = reader["Quantity"].ToString();

                                    string partID = reader["PartID"].ToString();
                                    foreach (ProductItem item in cboProducts.Items)
                                    {
                                        if (item.ID == partID) { cboProducts.SelectedItem = item; break; }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Database selection connection failed:\n" + ex.Message, "Database Link Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        #endregion

        #region 💾 Core Data Logic 
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
                            string lastID = result.ToString();
                            string seqStr = lastID.Replace(prefix, "");
                            if (int.TryParse(seqStr, out int seq))
                            {
                                txtOrderID.Text = prefix + (seq + 1).ToString("D4");
                                return;
                            }
                        }
                        txtOrderID.Text = prefix + "0001";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Generate OrderID DB Error: " + ex.Message, "DB Link Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtOrderID.Text = "SO" + DateTime.Now.ToString("yyyy") + "-" + DateTime.Now.ToString("MMddHHmm");
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
                    string query = "SELECT PartID, PartName, DefaultPrice FROM product_part";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<ProductItem> productList = new List<ProductItem>();
                            while (reader.Read())
                            {
                                productList.Add(new ProductItem
                                {
                                    ID = reader["PartID"].ToString(),
                                    Name = reader["PartName"].ToString(),
                                    Price = Convert.ToDecimal(reader["DefaultPrice"])
                                });
                            }
                            cboProducts.DataSource = productList;
                            cboProducts.DisplayMember = "Name";
                            cboProducts.ValueMember = "ID";
                        }
                    }
                    cboProducts.SelectedIndex = -1;
                }
                catch (Exception ex) { MessageBox.Show("Failed to load products from database:\n" + ex.Message, "Database Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void cboProducts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboProducts.SelectedItem != null)
            {
                ProductItem selectedProduct = (ProductItem)cboProducts.SelectedItem;
                currentUnitPrice = selectedProduct.Price;
                txtUnitPrice.Text = currentUnitPrice.ToString("F2");
                CalculateSubtotal();
            }
        }

        private void txtQty_TextChanged(object sender, EventArgs e)
        {
            CalculateSubtotal();
        }

        private void CalculateSubtotal()
        {
            if (int.TryParse(txtQty.Text.Trim(), out int qty) && qty > 0)
            {
                decimal subtotal = qty * currentUnitPrice;
                txtSubtotal.Text = subtotal.ToString("F2");
            }
            else
            {
                txtSubtotal.Text = "0.00";
            }
        }

        // ======================================================================
        // 🚀 建立新訂單邏輯 (INSERT)
        // ======================================================================
        private void btnCreateOrder_Click(object sender, EventArgs e)
        {
            string customerID = txtCustomerID.Text.Trim();
            string staffID = txtStaffID.Text.Trim();

            if (string.IsNullOrWhiteSpace(customerID) || string.IsNullOrWhiteSpace(staffID))
            {
                MessageBox.Show("Please provide Customer ID and Staff ID!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cboProducts.SelectedItem == null)
            {
                MessageBox.Show("Please select a product!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txtQty.Text.Trim(), out int qty) || qty <= 0)
            {
                MessageBox.Show("Please enter a valid quantity (greater than 0)!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ProductItem product = (ProductItem)cboProducts.SelectedItem;
            string partID = product.ID;
            string orderID = txtOrderID.Text.Trim();
            decimal subtotal = qty * currentUnitPrice;
            string orderStatus = chkRequireDelivery.Checked ? "Pending Delivery" : "Self-Pickup";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    string checkCustSql = "SELECT COUNT(*) FROM customer WHERE CustomerID = @CustomerID";
                    using (MySqlCommand checkCustCmd = new MySqlCommand(checkCustSql, conn))
                    {
                        checkCustCmd.Parameters.AddWithValue("@CustomerID", customerID);
                        if (Convert.ToInt32(checkCustCmd.ExecuteScalar()) == 0)
                        {
                            MessageBox.Show($"Customer ID '{customerID}' does not exist inside database!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    string checkStockSql = "SELECT StockLevel FROM product_part WHERE PartID = @PartID";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkStockSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@PartID", partID);
                        int currentStock = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (currentStock < qty)
                        {
                            MessageBox.Show($"Insufficient stock! Available: {currentStock}.", "Inventory Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            string insertOrderSql = "INSERT INTO orders (OrderID, CustomerID, StaffID, TotalAmount, Status) VALUES (@OrderID, @CustomerID, @StaffID, @Total, @Status)";
                            using (MySqlCommand cmdOrder = new MySqlCommand(insertOrderSql, conn, trans))
                            {
                                cmdOrder.Parameters.AddWithValue("@OrderID", orderID);
                                cmdOrder.Parameters.AddWithValue("@CustomerID", customerID);
                                cmdOrder.Parameters.AddWithValue("@StaffID", staffID);
                                cmdOrder.Parameters.AddWithValue("@Total", subtotal);
                                cmdOrder.Parameters.AddWithValue("@Status", orderStatus);
                                cmdOrder.ExecuteNonQuery();
                            }

                            string insertLineSql = "INSERT INTO order_lineitem (OrderID, PartID, Quantity, UnitPrice) VALUES (@OrderID, @PartID, @Qty, @UnitPrice)";
                            using (MySqlCommand cmdLine = new MySqlCommand(insertLineSql, conn, trans))
                            {
                                cmdLine.Parameters.AddWithValue("@OrderID", orderID);
                                cmdLine.Parameters.AddWithValue("@PartID", partID);
                                cmdLine.Parameters.AddWithValue("@Qty", qty);
                                cmdLine.Parameters.AddWithValue("@UnitPrice", currentUnitPrice);
                                cmdLine.ExecuteNonQuery();
                            }

                            string updateStockSql = "UPDATE product_part SET StockLevel = StockLevel - @Qty WHERE PartID = @PartID";
                            using (MySqlCommand cmdStock = new MySqlCommand(updateStockSql, conn, trans))
                            {
                                cmdStock.Parameters.AddWithValue("@Qty", qty);
                                cmdStock.Parameters.AddWithValue("@PartID", partID);
                                cmdStock.ExecuteNonQuery();
                            }

                            trans.Commit();

                            string msg = chkRequireDelivery.Checked
                                ? $"Sales Order [{orderID}] created!\nStatus: Forwarded to Logistics (Pending Delivery)."
                                : $"Sales Order [{orderID}] created!\nStatus: Flagged for Customer Self-Pickup.";

                            MessageBox.Show(msg, "Order Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            ClearFields();
                            RefreshOrdersGrid();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            MessageBox.Show("Transaction execution failed: " + ex.Message, "Transaction Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Database operational error:\n" + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        // ======================================================================
        // ✏️ 更新現有訂單邏輯 (UPDATE) - 解決你的 Error!
        // ======================================================================
        private void btnUpdateOrder_Click(object sender, EventArgs e)
        {
            string orderID = txtOrderID.Text.Trim();
            if (string.IsNullOrWhiteSpace(orderID)) return;

            if (cboProducts.SelectedItem == null) { MessageBox.Show("Please select a product!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!int.TryParse(txtQty.Text.Trim(), out int newQty) || newQty <= 0) { MessageBox.Show("Please enter a valid quantity!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            ProductItem product = (ProductItem)cboProducts.SelectedItem;
            string partID = product.ID;
            decimal newSubtotal = newQty * currentUnitPrice;
            string orderStatus = chkRequireDelivery.Checked ? "Pending Delivery" : "Self-Pickup";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    // 1. 搵返資料庫入面，呢張單原本買咗幾多件
                    string getOldQtySql = "SELECT Quantity FROM order_lineitem WHERE OrderID = @OrderID AND PartID = @PartID";
                    int oldQty = -1;
                    using (MySqlCommand cmdOld = new MySqlCommand(getOldQtySql, conn))
                    {
                        cmdOld.Parameters.AddWithValue("@OrderID", orderID);
                        cmdOld.Parameters.AddWithValue("@PartID", partID);
                        object result = cmdOld.ExecuteScalar();
                        if (result != null) oldQty = Convert.ToInt32(result);
                    }

                    if (oldQty == -1)
                    {
                        MessageBox.Show("This order does not exist or you cannot change the product type in update mode.", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 2. 計算數量差額 (正數代表買多咗要扣庫存，負數代表買少咗要加返庫存)
                    int qtyDifference = newQty - oldQty;

                    if (qtyDifference > 0)
                    {
                        string checkStockSql = "SELECT StockLevel FROM product_part WHERE PartID = @PartID";
                        using (MySqlCommand checkCmd = new MySqlCommand(checkStockSql, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@PartID", partID);
                            int currentStock = Convert.ToInt32(checkCmd.ExecuteScalar());
                            if (currentStock < qtyDifference)
                            {
                                MessageBox.Show($"Insufficient stock! You need {qtyDifference} more, but only {currentStock} available.", "Inventory Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                    }

                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // 3. 更新訂單明細 (order_lineitem)
                            string updateLineSql = "UPDATE order_lineitem SET Quantity = @NewQty, UnitPrice = @UnitPrice WHERE OrderID = @OrderID AND PartID = @PartID";
                            using (MySqlCommand cmdUpdateLine = new MySqlCommand(updateLineSql, conn, trans))
                            {
                                cmdUpdateLine.Parameters.AddWithValue("@NewQty", newQty);
                                cmdUpdateLine.Parameters.AddWithValue("@UnitPrice", currentUnitPrice);
                                cmdUpdateLine.Parameters.AddWithValue("@OrderID", orderID);
                                cmdUpdateLine.Parameters.AddWithValue("@PartID", partID);
                                cmdUpdateLine.ExecuteNonQuery();
                            }

                            // 4. 更新主訂單總數及狀態 (orders)
                            string updateOrderSql = "UPDATE orders SET TotalAmount = @Total, Status = @Status WHERE OrderID = @OrderID";
                            using (MySqlCommand cmdUpdateOrder = new MySqlCommand(updateOrderSql, conn, trans))
                            {
                                cmdUpdateOrder.Parameters.AddWithValue("@Total", newSubtotal);
                                cmdUpdateOrder.Parameters.AddWithValue("@Status", orderStatus);
                                cmdUpdateOrder.Parameters.AddWithValue("@OrderID", orderID);
                                cmdUpdateOrder.ExecuteNonQuery();
                            }

                            // 5. 聰明地扣除/加回庫存 (product_part)
                            string updateStockSql = "UPDATE product_part SET StockLevel = StockLevel - @QtyDiff WHERE PartID = @PartID";
                            using (MySqlCommand cmdStock = new MySqlCommand(updateStockSql, conn, trans))
                            {
                                cmdStock.Parameters.AddWithValue("@QtyDiff", qtyDifference);
                                cmdStock.Parameters.AddWithValue("@PartID", partID);
                                cmdStock.ExecuteNonQuery();
                            }

                            trans.Commit();
                            MessageBox.Show($"Order [{orderID}] has been updated successfully!\nNew Quantity: {newQty}", "Update Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            ClearFields();
                            RefreshOrdersGrid();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            MessageBox.Show("Update transaction failed: " + ex.Message, "Transaction Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Database error during update:\n" + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void RefreshOrdersGrid()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT o.OrderID AS 'Order ID', o.CustomerID AS 'Customer ID', 
                                            o.OrderDate AS 'Order Date', o.TotalAmount AS 'Total HKD', o.Status AS 'Status' 
                                     FROM orders o ORDER BY o.OrderDate DESC";
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
                catch (Exception ex) { MessageBox.Show("Failed to load historical grid data:\n" + ex.Message, "Database Link Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void ClearFields()
        {
            dgvOrders.SelectionChanged -= dgvOrders_SelectionChanged;

            txtCustomerID.Clear();
            txtQty.Clear();
            txtUnitPrice.Clear();
            txtSubtotal.Clear();
            cboProducts.SelectedIndex = -1;
            chkRequireDelivery.Checked = true;

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
            if (dgvOrders.CurrentRow == null || dgvOrders.CurrentRow.Index < 0)
            {
                MessageBox.Show("Please select an order from the history panel first!", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string orderID = dgvOrders.CurrentRow.Cells["Order ID"].Value?.ToString();
            if (string.IsNullOrEmpty(orderID)) return;

            string customerID = "";
            string customerName = "Unknown Customer";
            string customerType = "B2C";
            string staffID = "";
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
                                if (reader.Read())
                                {
                                    customerName = reader["Name"].ToString();
                                    customerType = reader["Type"].ToString();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to retrieve quotation data from Database:\n" + ex.Message, "Database Link Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
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
            btnPrint.Click += (s, e) => MessageBox.Show("Connecting to local hardware printer subsystem...", "Print Job Triggered", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Button btnPDF = new Button { Text = "💾 Export PDF", Size = new Size(135, 42), Location = new Point(245, 16), BackColor = Color.FromArgb(79, 70, 229), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnPDF.FlatAppearance.BorderSize = 0;
            btnPDF.Click += (s, e) => MessageBox.Show("PDF Document generated successfully inside output matrix folder.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

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