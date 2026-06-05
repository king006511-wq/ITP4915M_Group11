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
        // 🔒 Database Configuration (資料庫連線)
        // ==========================================
        private readonly string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";
        private decimal currentUnitPrice = 0;

        // ==========================================
        // 🎨 Modern UI Element Variables
        // ==========================================
        private TextBox txtOrderID, txtCustomerID, txtStaffID, txtQty, txtUnitPrice, txtSubtotal;
        private ComboBox cboProducts;
        private DataGridView dgvOrders;
        private Button btnSubmitOrder;

        public OrderManagementForm()
        {
            InitializeComponent();
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
            // 1. Main Window Settings
            this.Text = "Premium Living Furniture - Sales Order Management";
            this.Size = new Size(1180, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            // 鎖定視窗大小，防止放大縮細搞亂排版
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // 2. Left Sidebar Navigation Panel
            Panel pnlSidebar = new Panel
            {
                Width = 260,
                Dock = DockStyle.Left,
                BackColor = Color.FromArgb(15, 23, 42)
            };

            Label lblLogo = new Label
            {
                Text = "Premium Living\nFurniture",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 25),
                Size = new Size(220, 60),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlSidebar.Controls.Add(lblLogo);

            // ENGLISH Sidebar Items List
            string[] menuItems = {
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
                Button btnMenu = new Button
                {
                    Text = "  " + item,
                    Top = btnTop,
                    Left = 12,
                    Size = new Size(236, 48),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand
                };
                btnMenu.FlatAppearance.BorderSize = 0;

                if (item.Contains("Sales Order Mgmt"))
                {
                    btnMenu.BackColor = Color.FromArgb(37, 99, 235);
                    btnMenu.ForeColor = Color.White;
                }
                else if (item.Contains("Logout"))
                {
                    btnMenu.BackColor = Color.FromArgb(239, 68, 68);
                    btnMenu.ForeColor = Color.White;
                    btnMenu.MouseEnter += (s, e) => { btnMenu.BackColor = Color.FromArgb(220, 38, 38); };
                    btnMenu.MouseLeave += (s, e) => { btnMenu.BackColor = Color.FromArgb(239, 68, 68); };
                }
                else
                {
                    btnMenu.BackColor = Color.Transparent;
                    btnMenu.ForeColor = Color.FromArgb(148, 163, 184);
                    btnMenu.MouseEnter += (s, e) => { btnMenu.BackColor = Color.FromArgb(51, 65, 85); btnMenu.ForeColor = Color.White; };
                    btnMenu.MouseLeave += (s, e) => { btnMenu.BackColor = Color.Transparent; btnMenu.ForeColor = Color.FromArgb(148, 163, 184); };
                }

                btnMenu.Click += (s, e) => {
                    Form targetForm = null;
                    try
                    {
                        if (item.Contains("Logistics")) targetForm = new LogisticsForm();
                        else if (item.Contains("Product Maintenance")) targetForm = new ProductManagement();
                        else if (item.Contains("Staff Mgmt")) targetForm = new EmployeeManagement();
                        else if (item.Contains("Received")) targetForm = new GoodsReceivedForm();
                        else if (item.Contains("Material Requests")) targetForm = new RawMaterialRequestForm();
                        else if (item.Contains("Procurement")) targetForm = new ProcurementForm();
                        else if (item.Contains("Support")) targetForm = new AfterServiceForm();
                        else if (item.Contains("Logout")) { Application.Restart(); return; }

                        if (targetForm != null && !(targetForm is OrderManagementForm))
                        {
                            this.Hide();
                            targetForm.FormClosed += (senderForm, args) => this.Show();
                            targetForm.Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Form navigation error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                pnlSidebar.Controls.Add(btnMenu);
                btnTop += 55;
            }
            this.Controls.Add(pnlSidebar);

            // 3. Right Main Workspace Panel
            Panel pnlMain = new Panel
            {
                Location = new Point(260, 0),
                Size = new Size(900, 750)
            };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label
            {
                Text = "Order Processing Dashboard",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(30, 20),
                AutoSize = true
            };
            pnlMain.Controls.Add(lblHeader);

            // 4. Input Card Panel (左側落單卡片)
            Panel pnlCard = new Panel
            {
                Location = new Point(30, 85),
                Size = new Size(420, 600),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlCard);

            Label lblCardTitle = new Label
            {
                Text = "📝 Create New Sales Order",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlCard.Controls.Add(lblCardTitle);

            // 🌟 配合 CXXX 和 SXXX 的格式更新標籤文字
            string[] labels = {
                "Order ID (Auto-generated):",
                "Customer ID (e.g., C001) *:",
                "Staff ID (e.g., S001) *:",
                "Select Product *:",
                "Unit Price (HKD):",
                "Quantity *:",
                "Subtotal (HKD):"
            };

            int startY = 55;

            txtOrderID = CreateStyledTextBox(pnlCard, ref startY, labels[0], true);
            txtCustomerID = CreateStyledTextBox(pnlCard, ref startY, labels[1], false);
            txtStaffID = CreateStyledTextBox(pnlCard, ref startY, labels[2], false);

            Label lblCbo = new Label { Text = labels[3], Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboProducts = new ComboBox { Location = new Point(20, startY + 25), Width = 375, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F), BackColor = Color.White };
            cboProducts.SelectedIndexChanged += cboProducts_SelectedIndexChanged;
            pnlCard.Controls.Add(lblCbo); pnlCard.Controls.Add(cboProducts);
            startY += 65;

            txtUnitPrice = CreateStyledTextBox(pnlCard, ref startY, labels[4], true);
            txtQty = CreateStyledTextBox(pnlCard, ref startY, labels[5], false);
            txtQty.TextChanged += txtQty_TextChanged;
            txtSubtotal = CreateStyledTextBox(pnlCard, ref startY, labels[6], true);
            txtSubtotal.ForeColor = Color.FromArgb(220, 38, 38);
            txtSubtotal.Font = new Font("Segoe UI", 12F, FontStyle.Bold);

            // 綠色確定按鈕
            btnSubmitOrder = new Button
            {
                Text = "🚀 Confirm and Submit Order",
                Location = new Point(20, startY + 15),
                Size = new Size(375, 52),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSubmitOrder.FlatAppearance.BorderSize = 0;
            btnSubmitOrder.Click += btnCreateOrder_Click;
            pnlCard.Controls.Add(btnSubmitOrder);

            // 5. Data View Panel (右側歷史紀錄)
            Label lblGridTitle = new Label
            {
                Text = "📊 Order History (Premium Living)",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(480, 85),
                AutoSize = true
            };
            pnlMain.Controls.Add(lblGridTitle);

            dgvOrders = new DataGridView
            {
                Location = new Point(480, 125),
                Size = new Size(390, 560),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                GridColor = Color.FromArgb(241, 245, 249)
            };

            dgvOrders.EnableHeadersVisualStyles = false;
            dgvOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvOrders.ColumnHeadersHeight = 38;
            dgvOrders.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvOrders.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
            dgvOrders.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            // 加入雙擊功能
            dgvOrders.CellDoubleClick += dgvOrders_CellDoubleClick;

            pnlMain.Controls.Add(dgvOrders);
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

        #region 🖱️ Grid Interactions (右邊表格點擊功能)
        private void dgvOrders_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvOrders.Rows[e.RowIndex];
                string selectedOrderID = row.Cells["Order ID"].Value.ToString();

                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        string query = @"
                            SELECT o.OrderID, o.CustomerID, o.StaffID, l.PartID, l.Quantity 
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

                                    // 自動選取對應的產品
                                    foreach (ProductItem item in cboProducts.Items)
                                    {
                                        if (item.ID == partID)
                                        {
                                            cboProducts.SelectedItem = item;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading order details: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        #endregion

        #region 💾 Core English-Only Data Logic 
        // 🌟 修正：完美對接資料庫的 SO2026-XXXX 流水號生成
        private void GenerateOrderID()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string currentYear = DateTime.Now.ToString("yyyy");
                    string prefix = "SO" + currentYear + "-"; // "SO2026-"

                    // 找出目前資料庫中今年最大的編號
                    string query = "SELECT OrderID FROM orders WHERE OrderID LIKE @Prefix ORDER BY OrderID DESC LIMIT 1";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Prefix", prefix + "%");
                        object result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            string lastID = result.ToString(); // 例如 "SO2026-0004"
                            string seqStr = lastID.Replace(prefix, ""); // 提取出 "0004"
                            if (int.TryParse(seqStr, out int seq))
                            {
                                // 序號自動加 1 并補齊 4 位數
                                txtOrderID.Text = prefix + (seq + 1).ToString("D4");
                                return;
                            }
                        }

                        // 如果今年完全沒有單，預設由 0001 開始
                        txtOrderID.Text = prefix + "0001";
                    }
                }
                catch (Exception)
                {
                    // 萬一連唔到資料庫的備用安全格式
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
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load product list: " + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    // 驗證客戶
                    string checkCustSql = "SELECT COUNT(*) FROM customer WHERE CustomerID = @CustomerID";
                    using (MySqlCommand checkCustCmd = new MySqlCommand(checkCustSql, conn))
                    {
                        checkCustCmd.Parameters.AddWithValue("@CustomerID", customerID);
                        if (Convert.ToInt32(checkCustCmd.ExecuteScalar()) == 0)
                        {
                            MessageBox.Show($"Customer ID '{customerID}' does not exist! Please check and try again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // 驗證員工
                    string checkStaffSql = "SELECT COUNT(*) FROM staff WHERE StaffID = @StaffID";
                    using (MySqlCommand checkStaffCmd = new MySqlCommand(checkStaffSql, conn))
                    {
                        checkStaffCmd.Parameters.AddWithValue("@StaffID", staffID);
                        if (Convert.ToInt32(checkStaffCmd.ExecuteScalar()) == 0)
                        {
                            MessageBox.Show($"Staff ID '{staffID}' does not exist! Please check and try again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // 驗證庫存
                    string checkStockSql = "SELECT StockLevel FROM product_part WHERE PartID = @PartID";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkStockSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@PartID", partID);
                        int currentStock = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (currentStock < qty)
                        {
                            MessageBox.Show($"Insufficient stock! Current stock level is: {currentStock}. Order cannot be submitted.", "Inventory Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // 開啟安全交易機制 (Transaction)
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            string insertOrderSql = "INSERT INTO orders (OrderID, CustomerID, StaffID, TotalAmount, Status) VALUES (@OrderID, @CustomerID, @StaffID, @Total, 'Pending')";
                            using (MySqlCommand cmdOrder = new MySqlCommand(insertOrderSql, conn, trans))
                            {
                                cmdOrder.Parameters.AddWithValue("@OrderID", orderID);
                                cmdOrder.Parameters.AddWithValue("@CustomerID", customerID);
                                cmdOrder.Parameters.AddWithValue("@StaffID", staffID);
                                cmdOrder.Parameters.AddWithValue("@Total", subtotal);
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
                            MessageBox.Show($"Sales Order [{orderID}] successfully created!\nInventory has been real-time deducted.", "Order Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            ClearFields();
                            GenerateOrderID();
                            RefreshOrdersGrid();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw new Exception("Transaction execution failed. Data rolled back. Root Cause: " + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    string query = @"SELECT o.OrderID AS 'Order ID', o.CustomerID AS 'Customer ID', 
                                            o.OrderDate AS 'Order Date', o.TotalAmount AS 'Total HKD', o.Status AS 'Status' 
                                     FROM orders o ORDER BY o.OrderDate DESC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvOrders.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Grid Refresh Error: " + ex.Message);
                }
            }
        }

        private void ClearFields()
        {
            txtCustomerID.Clear();
            txtStaffID.Clear();
            txtQty.Clear();
            txtUnitPrice.Clear();
            txtSubtotal.Clear();
            cboProducts.SelectedIndex = -1;
        }
        #endregion
    }
}