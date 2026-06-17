using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
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
        private TextBox txtOrderID, txtCustomerID, txtStaffUIID, txtQty, txtUnitPrice;
        private ComboBox cboProducts;
        private CheckBox chkRequireDelivery;
        private DataGridView dgvOrders;
        private Button btnSubmitOrder, btnUpdateOrder, btnClear, btnCreateQuotation;

        // ✨ 購物車專用變數
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
                // ThemeManager.ApplyTheme(this); // 💡 如果有 ThemeManager 請取消註解
                InitializePremiumModernUI();
            }
        }

        private void OrderManagementForm_Load(object sender, EventArgs e)
        {
            this.currentStaffID = UserSession.LoggedInStaffID;

            if (!CanAccess())
            {
                MessageBox.Show(
                    $"[SECURITY ALERT] Access Denied!\n\n" +
                    $"Logged In StaffID: {currentStaffID}\n" +
                    $"Your Account Role is: \"{(string.IsNullOrEmpty(UserSession.LoggedInStaffRole) ? "None / Empty" : UserSession.LoggedInStaffRole)}\"\n\n" +
                    $"Only Manager, Administrator and Sales Representative are authorized to access this module.",
                    "System Security Enforcer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop
                );

                this.BeginInvoke(new MethodInvoker(this.Close));
                return;
            }

            GenerateOrderID();
            LoadProductsToCombo();
            RefreshOrdersGrid();
        }

        // 方法級別的授權檢查示例：在建立訂單前確認權限（若需要更嚴格控制）
        private bool EnsureCanCreateOrder()
        {
            // 假設建立訂單僅限於 Sales, Manager, Administrator
            if (!AuthorizationHelper.IsInRole(AuthorizationHelper.Roles.Sales, AuthorizationHelper.Roles.Manager, AuthorizationHelper.Roles.Administrator))
            {
                MessageBox.Show("Access Denied: insufficient privileges to create orders.", "Authorization", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }
            return true;
        }

        private bool CanAccess()
        {
            string currentRole = UserSession.LoggedInStaffRole;
            if (string.IsNullOrWhiteSpace(currentRole)) return false;

            List<string> allowedRoles = new List<string> { "Manager", "Administrator", "Sales Representative" };
            return allowedRoles.Any(role => role.Equals(currentRole.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        #region 🎨 Dynamic Premium English UI Construction
        private void InitializePremiumModernUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Advanced Sales Order Management";
            this.Size = new Size(1250, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Load += OrderManagementForm_Load;

            // 左側導航面板 (Sidebar)
            Panel pnlSidebar = new Panel { Width = 260, Dock = DockStyle.Left, BackColor = Color.FromArgb(15, 23, 42) };
            Label lblLogo = new Label { Text = "Premium Living\nFurniture", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 25), Size = new Size(220, 60), TextAlign = ContentAlignment.MiddleLeft };
            pnlSidebar.Controls.Add(lblLogo);

            string[] menuItems = {
                "🏠 Back Home",
                "🛒 Sales Order Mgmt",
                "🚚 Delivery Logistics",
                "🛋️ Product Maintenance",
                "👔 HR / Staff Mgmt",
                "📦 Goods Received (GRN)",
                "🏭 Material Requests",
                "📊 Procurement Control",
                "🔧 Customer Support",
                "🚪 Logout System"
            };

            int btnTop = 110;
            foreach (string item in menuItems)
            {
                Button btnMenu = new Button { Text = "  " + item, Top = btnTop, Left = 12, Size = new Size(236, 48), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand };
                btnMenu.FlatAppearance.BorderSize = 0;

                // 根據當前使用者角色決定此側邊選單項目是否顯示
                bool menuVisible = true;
                if (item.Contains("Sales Order Mgmt")) menuVisible = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.SalesRepresentative);
                else if (item.Contains("Delivery Logistics")) menuVisible = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.LogisticsDriver);
                else if (item.Contains("Product Maintenance")) menuVisible = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator);
                else if (item.Contains("HR / Staff Mgmt")) menuVisible = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator);
                else if (item.Contains("Goods Received")) menuVisible = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.WarehouseSpecialist);
                else if (item.Contains("Material Requests")) menuVisible = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.ProcurementOfficer);
                else if (item.Contains("Procurement Control")) menuVisible = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.ProcurementOfficer);
                else if (item.Contains("Customer Support")) menuVisible = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.SalesRepresentative);

                btnMenu.Visible = menuVisible;

                if (item.Contains("Sales Order Mgmt")) { btnMenu.BackColor = Color.FromArgb(37, 99, 235); btnMenu.ForeColor = Color.White; }
                else if (item.Contains("Logout")) { btnMenu.BackColor = Color.FromArgb(239, 68, 68); btnMenu.ForeColor = Color.White; }
                else { btnMenu.BackColor = Color.Transparent; btnMenu.ForeColor = Color.FromArgb(148, 163, 184); }

                // 🎯 修正後的 Click 事件路由邏輯
                btnMenu.Click += (s, e) => {
                    Form targetForm = null;
                    try
                    {
                        if (item.Contains("Back Home")) targetForm = new MainDashboard();
                        else if (item.Contains("Sales Order Mgmt")) targetForm = new OrderManagementForm();
                        else if (item.Contains("Delivery Logistics")) { targetForm = new LogisticsForm(); }
                        else if (item.Contains("Product Maintenance")) { targetForm = new ProductManagement(); }
                        else if (item.Contains("HR / Staff Mgmt")) { targetForm = new EmployeeManagement(); }
                        else if (item.Contains("Goods Received")) { targetForm = new GoodsReceivedForm(); }
                        else if (item.Contains("Material Requests")) { targetForm = new RawMaterialRequestForm(); }
                        else if (item.Contains("Procurement Control")) { targetForm = new ProcurementForm(); }
                        else if (item.Contains("Customer Support")) { targetForm = new AfterServiceForm(); }
                        else if (item.Contains("Logout System"))
                        {
                            DialogResult result = MessageBox.Show("Are you sure you want to log out?", "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (result == DialogResult.Yes)
                            {
                                // 清除 Session 確保安全
                                UserSession.LoggedInStaffID = "";
                                UserSession.LoggedInStaffName = "";
                                UserSession.LoggedInStaffRole = "";

                                Login login = new Login();
                                login.Show();
                                this.Hide();
                                login.FormClosed += (senderLogin, args) => this.Close();
                            }
                            return;
                        }

                        // 執行表單切換跳轉
                        if (targetForm != null)
                        {
                            this.Hide();
                            targetForm.FormClosed += (senderObj, args) => { this.Show(); };
                            targetForm.Show();
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Navigation routing failed.\nError: " + ex.Message, "Routing Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                };
                pnlSidebar.Controls.Add(btnMenu);
                btnTop += 55;
            }
            this.Controls.Add(pnlSidebar);

            // 右側工作面板
            Panel pnlMain = new Panel { Location = new Point(260, 0), Size = new Size(990, 850) };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label { Text = "Order Processing Dashboard", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(30, 20), AutoSize = true };
            pnlMain.Controls.Add(lblHeader);

            Button btnBackHome = new Button { Text = "\uD83D\uDD19 Go Back", Size = new Size(120, 34), Location = new Point(830, 22), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnBackHome.FlatAppearance.BorderSize = 0;
            btnBackHome.Click += (s, e) => {
                this.Close(); // 關閉目前視窗，會觸發 MainDashboard 的 target.FormClosed += ... => this.Show(); 重新顯示主頁面
            };
            pnlMain.Controls.Add(btnBackHome);

            Label lblStaff = new Label { Text = $"👤 Active Staff ID: {currentStaffID}", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(13, 148, 136), Location = new Point(520, 26), AutoSize = true };
            pnlMain.Controls.Add(lblStaff);

            // 左側卡片：訂單與購物車輸入區
            Panel pnlCard = new Panel { Location = new Point(30, 85), Size = new Size(500, 700), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlMain.Controls.Add(pnlCard);

            Label lblCardTitle = new Label { Text = "📝 Multi-Item Order Builder", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(20, 15), AutoSize = true };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 50;
            txtOrderID = CreateStyledTextBox(pnlCard, ref startY, "Order ID (Auto):", true, 210);
            startY -= 65;
            txtCustomerID = CreateStyledTextBox(pnlCard, ref startY, "Customer ID *:", false, 210, 240);

            txtStaffUIID = CreateStyledTextBox(pnlCard, ref startY, "Staff ID:", true, 210);
            txtStaffUIID.Text = currentStaffID;
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
            Label lblCartGridTitle = new Label { Text = "📦 Staging Cart (Order Line Items)", Location = new Point(20, startY), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true };
            pnlCard.Controls.Add(lblCartGridTitle);
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

            // 底部功能按鈕區
            btnSubmitOrder = new Button { Text = "➕ Create Order", Location = new Point(20, startY), Size = new Size(140, 42), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSubmitOrder.Click += btnCreateOrder_Click;
            pnlCard.Controls.Add(btnSubmitOrder);

            btnUpdateOrder = new Button { Text = "✏️ Update", Location = new Point(170, startY), Size = new Size(140, 42), BackColor = Color.FromArgb(245, 158, 11), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnUpdateOrder.Click += (s, e) => { /* 實作程式碼 */ };
            pnlCard.Controls.Add(btnUpdateOrder);

            btnClear = new Button { Text = "🧹 Clear", Location = new Point(320, startY), Size = new Size(150, 42), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClear.Click += (s, e) => ClearFields();
            pnlCard.Controls.Add(btnClear);

            btnCreateQuotation = new Button { Text = "📄 Generate Order Quotation", Location = new Point(20, startY + 50), Size = new Size(450, 42), BackColor = Color.FromArgb(124, 58, 237), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCreateQuotation.Click += (s, e) => { /* 實作程式碼 */ };
            pnlCard.Controls.Add(btnCreateQuotation);

            // 右側歷史訂單紀錄面板
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
            foreach (DataRow row in cartTable.Rows) { globalOrderTotal += Convert.ToDecimal(row["Subtotal"]); }
            lblTotalAmountDisplay.Text = $"Total Bill: ${globalOrderTotal:N2}";
        }
        #endregion

        #region 🖱️ Grid Selection Logic (Load Order into Cart)
        private void dgvOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvOrders.CurrentRow == null || dgvOrders.CurrentRow.Index < 0) return;

            object cellValue = dgvOrders.CurrentRow.Cells["Order ID"].Value;
            if (cellValue == null || cellValue == DBNull.Value) return;

            string selectedOrderID = cellValue.ToString();
            cartTable.Clear();

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
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
                                txtStaffUIID.Text = reader["StaffID"].ToString();
                            }
                        }
                    }

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

        private void btnCreateOrder_Click(object sender, EventArgs e)
        {
            if (!EnsureCanCreateOrder()) return;

            if (cartTable.Rows.Count == 0) { MessageBox.Show("Cart is empty!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            string customerID = txtCustomerID.Text.Trim();
            string orderID = txtOrderID.Text.Trim();
            if (string.IsNullOrWhiteSpace(customerID)) { MessageBox.Show("Customer ID required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            string orderStatus = chkRequireDelivery.Checked ? "Pending Delivery" : "Self Pickup";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string checkCustSql = "SELECT COUNT(*) FROM customer WHERE CustomerID = @CustomerID";
                    using (MySqlCommand checkCustCmd = new MySqlCommand(checkCustSql, conn))
                    {
                        checkCustCmd.Parameters.AddWithValue("@CustomerID", customerID);
                        if (Convert.ToInt32(checkCustCmd.ExecuteScalar()) == 0) { MessageBox.Show($"Customer ID '{customerID}' not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
                    }

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

                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            string insertOrderSql = "INSERT INTO orders (OrderID, CustomerID, StaffID, TotalAmount, Status, OrderDate) VALUES (@OID, @CID, @SID, @Total, @Status, NOW())";
                            using (MySqlCommand cmdOrder = new MySqlCommand(insertOrderSql, conn, trans))
                            {
                                cmdOrder.Parameters.AddWithValue("@OID", orderID);
                                cmdOrder.Parameters.AddWithValue("@CID", customerID);
                                cmdOrder.Parameters.AddWithValue("@SID", currentStaffID);
                                cmdOrder.Parameters.AddWithValue("@Total", globalOrderTotal);
                                cmdOrder.Parameters.AddWithValue("@Status", orderStatus);
                                cmdOrder.ExecuteNonQuery();
                            }

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
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw ex;
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Failed to create order:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void RefreshOrdersGrid()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT OrderID AS 'Order ID', CustomerID AS 'Customer ID', TotalAmount AS 'Total Bill', Status FROM orders ORDER BY OrderDate DESC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvOrders.DataSource = dt;
                    }
                }
                catch (Exception ex) { MessageBox.Show("Failed to refresh logs: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void ClearFields()
        {
            txtCustomerID.Clear();
            txtQty.Clear();
            cartTable.Clear();
            UpdateGlobalOrderTotal();
            GenerateOrderID();
            if (cboProducts.Items.Count > 0) cboProducts.SelectedIndex = -1;
        }
        #endregion
    }
}