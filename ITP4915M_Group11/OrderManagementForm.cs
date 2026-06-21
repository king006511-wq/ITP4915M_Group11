using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public class ProductItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public partial class OrderManagementForm : Form
    {
        private readonly string connString = UserSession.ConnString;
        private decimal currentUnitPrice = 0;
        private string currentStaffID;
        private TextBox txtOrderID, txtCustomerID, txtStaffUIID, txtQty, txtUnitPrice;
        private ComboBox cboProducts;
        private CheckBox chkRequireDelivery;
        private DataGridView dgvOrders;
        private Button btnSubmitOrder, btnUpdateOrder, btnClear, btnCreateQuotation;
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
                InitializePremiumModernUI(); // 🛠️ Loaded with strict TableLayoutPanel Grid to prevent ALL overlaps
            }
        }

        private void OrderManagementForm_Load(object sender, EventArgs e)
        {
            this.currentStaffID = UserSession.LoggedInStaffID;
            if (!CanAccess())
            {
                MessageBox.Show("[SECURITY ALERT] Access Denied!\n\nLogged In StaffID: " + currentStaffID + "\nYour Account Role is: \"" + (string.IsNullOrEmpty(UserSession.LoggedInStaffRole) ? "None / Empty" : UserSession.LoggedInStaffRole) + "\"\n\nOnly Manager, Administrator and Sales Representative are authorized to access this module.", "System Security Enforcer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.BeginInvoke(new MethodInvoker(this.Close));
                return;
            }
            GenerateOrderID();
            LoadProductsToCombo();
            RefreshOrdersGrid();
        }

        private bool EnsureCanCreateOrder()
        {
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

        #region 🎨 Dynamic Premium English UI Construction (Strict Table Grid)
        private void InitializePremiumModernUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Advanced Sales Order Management";
            this.BackColor = Color.FromArgb(241, 245, 249);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Load += OrderManagementForm_Load;

            // 🌟 1. Master Table Layout (Prevents overlapping permanently)
            TableLayoutPanel mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F)); // Header strictly 70px tall
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Content takes exact remaining height
            this.Controls.Add(mainTable);

            // 🌟 2. Header Panel
            Panel pnlHeader = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            Label lblHeader = new Label
            {
                Text = "Order Processing Dashboard",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(20, 20),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblHeader);

            Label lblStaff = new Label
            {
                Text = $"👤 Active Staff ID: {currentStaffID}",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(13, 148, 136),
                Location = new Point(480, 26),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblStaff);
            mainTable.Controls.Add(pnlHeader, 0, 0);

            // 🌟 3. Content Table (Left Inputs + Middle Spacer + Right Grid)
            TableLayoutPanel contentTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(20, 0, 20, 20) // Outer bounds padding
            };
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 510F)); // Left Panel rigidly 510px wide
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));  // Spacer rigidly 20px wide
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // Right Grid exactly fills the rest
            mainTable.Controls.Add(contentTable, 0, 1);

            // 🌟 4. Left Panel: Inputs (With AutoScroll to prevent cutoff)
            Panel pnlCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true // Enables scrolling if the laptop screen is too small vertically
            };
            pnlCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            contentTable.Controls.Add(pnlCard, 0, 0);

            // 🌟 5. Right Panel: Title + Grid
            TableLayoutPanel rightTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0)
            };
            rightTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            rightTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Label lblGridTitle = new Label
            {
                Text = "📊 Overall Order History",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize = true,
                Dock = DockStyle.Bottom
            };
            rightTable.Controls.Add(lblGridTitle, 0, 0);

            dgvOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
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
            dgvOrders.SelectionChanged += dgvOrders_SelectionChanged;
            rightTable.Controls.Add(dgvOrders, 0, 1);

            contentTable.Controls.Add(rightTable, 2, 0);

            // ====================================================
            // 📝 Populate Left Input Panel (Preserved layout math)
            // ====================================================
            Label lblCardTitle = new Label
            {
                Text = "📝 Multi-Item Order Builder",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 50;
            txtOrderID = CreateStyledTextBox(pnlCard, ref startY, "Order ID (Auto):", true, 210);
            startY -= 65;
            txtCustomerID = CreateStyledTextBox(pnlCard, ref startY, "Customer ID *:", false, 210, 240);

            txtStaffUIID = CreateStyledTextBox(pnlCard, ref startY, "Staff ID:", true, 210);
            txtStaffUIID.Text = currentStaffID;
            startY -= 65;

            Label lblCbo = new Label
            {
                Text = "Select Product:",
                Location = new Point(240, startY),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105)
            };
            cboProducts = new ComboBox
            {
                Location = new Point(240, startY + 25),
                Width = 230,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10.5F),
                BackColor = Color.White
            };
            cboProducts.SelectedIndexChanged += cboProducts_SelectedIndexChanged;
            pnlCard.Controls.Add(lblCbo);
            pnlCard.Controls.Add(cboProducts);
            startY += 65;

            txtUnitPrice = CreateStyledTextBox(pnlCard, ref startY, "Unit Price ($):", true, 130);
            startY -= 65;
            txtQty = CreateStyledTextBox(pnlCard, ref startY, "Qty:", false, 80, 160);
            startY -= 65;

            btnAddItem = new Button
            {
                Text = "➕ Add Item",
                Location = new Point(255, startY + 23),
                Size = new Size(100, 32),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddItem.Click += BtnAddItem_Click;
            pnlCard.Controls.Add(btnAddItem);

            btnRemoveItem = new Button
            {
                Text = "❌ Remove",
                Location = new Point(365, startY + 23),
                Size = new Size(105, 32),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRemoveItem.Click += BtnRemoveItem_Click;
            pnlCard.Controls.Add(btnRemoveItem);
            startY += 70;

            Label lblCartGridTitle = new Label
            {
                Text = "📦 Staging Cart (Order Line Items)",
                Location = new Point(20, startY),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize = true
            };
            pnlCard.Controls.Add(lblCartGridTitle);
            startY += 25;

            cartTable = new DataTable();
            cartTable.Columns.Add("PartID", typeof(string));
            cartTable.Columns.Add("Product Name", typeof(string));
            cartTable.Columns.Add("Qty", typeof(int));
            cartTable.Columns.Add("Unit Price", typeof(decimal));
            cartTable.Columns.Add("Subtotal", typeof(decimal));

            dgvCart = new DataGridView
            {
                Location = new Point(20, startY),
                Size = new Size(450, 160),
                DataSource = cartTable,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvCart.EnableHeadersVisualStyles = false;
            dgvCart.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(71, 85, 105);
            dgvCart.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            pnlCard.Controls.Add(dgvCart);
            startY += 175;

            lblTotalAmountDisplay = new Label
            {
                Text = "Total Bill: $0.00",
                Location = new Point(20, startY),
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 38, 38),
                AutoSize = true
            };
            pnlCard.Controls.Add(lblTotalAmountDisplay);
            startY += 40;

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

            btnSubmitOrder = new Button
            {
                Text = "➕ Create Order",
                Location = new Point(20, startY),
                Size = new Size(140, 42),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSubmitOrder.Click += btnCreateOrder_Click;
            pnlCard.Controls.Add(btnSubmitOrder);

            btnUpdateOrder = new Button
            {
                Text = "✏️ Update",
                Location = new Point(170, startY),
                Size = new Size(140, 42),
                BackColor = Color.FromArgb(245, 158, 11),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnUpdateOrder.Click += (s, e) => { /* 實作程式碼 */ };
            pnlCard.Controls.Add(btnUpdateOrder);

            btnClear = new Button
            {
                Text = "🧹 Clear",
                Location = new Point(320, startY),
                Size = new Size(150, 42),
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClear.Click += (s, e) => ClearFields();
            pnlCard.Controls.Add(btnClear);

            btnCreateQuotation = new Button
            {
                Text = "📄 Generate Order Quotation",
                Location = new Point(20, startY + 50),
                Size = new Size(450, 42),
                BackColor = Color.FromArgb(124, 58, 237),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCreateQuotation.Click += (s, e) => { /* 實作程式碼 */ };
            pnlCard.Controls.Add(btnCreateQuotation);
        }

        private TextBox CreateStyledTextBox(Panel container, ref int topY, string labelText, bool readOnly, int width, int offsetX = 20)
        {
            Label lbl = new Label
            {
                Text = labelText,
                Location = new Point(offsetX, topY),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105)
            };
            TextBox txt = new TextBox
            {
                Location = new Point(offsetX, topY + 25),
                Width = width,
                Font = new Font("Segoe UI", 10.5F),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
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

        #region Operational Logic & Core Engines
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
            if (cboProducts.SelectedItem == null)
            {
                MessageBox.Show("Please select a product first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txtQty.Text.Trim(), out int qty) || qty <= 0)
            {
                MessageBox.Show("Please enter a valid positive quantity.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
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
                    itemExists = true;
                    break;
                }
            }
            if (!itemExists)
            {
                cartTable.Rows.Add(partID, prodName, qty, price, subtotal);
            }
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
            foreach (DataRow row in cartTable.Rows)
            {
                globalOrderTotal += Convert.ToDecimal(row["Subtotal"]);
            }
            lblTotalAmountDisplay.Text = $"Total Bill: ${globalOrderTotal:N2}";
        }

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
                    string queryLines = @"SELECT l.PartID, p.PartName, l.Quantity, l.UnitPrice FROM order_lineitem l JOIN product_part p ON l.PartID = p.PartID WHERE l.OrderID = @OrderID";
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
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load order details:\n" + ex.Message, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                            if (int.TryParse(seqStr, out int seq))
                            {
                                txtOrderID.Text = prefix + (seq + 1).ToString("D4");
                                return;
                            }
                        }
                        txtOrderID.Text = prefix + "0001";
                    }
                }
                catch (Exception)
                {
                    txtOrderID.Text = "SO" + DateTime.Now.ToString("yyyyMMddHHmm");
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
                    MessageBox.Show("Failed to load products: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCreateOrder_Click(object sender, EventArgs e)
        {
            if (!EnsureCanCreateOrder()) return;
            if (cartTable.Rows.Count == 0)
            {
                MessageBox.Show("Cart is empty!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string customerID = txtCustomerID.Text.Trim();
            string orderID = txtOrderID.Text.Trim();
            if (string.IsNullOrWhiteSpace(customerID))
            {
                MessageBox.Show("Customer ID required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
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
                        if (Convert.ToInt32(checkCustCmd.ExecuteScalar()) == 0)
                        {
                            MessageBox.Show($"Customer ID '{customerID}' not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
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
                            if (currentStock < reqQty)
                            {
                                MessageBox.Show($"Insufficient stock for {row["Product Name"]}! Available: {currentStock}.", "Inventory Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
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
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to create order:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    string query = "SELECT OrderID AS 'Order ID', CustomerID AS 'Customer ID', TotalAmount AS 'Total Bill', Status FROM orders ORDER BY OrderDate DESC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvOrders.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to refresh logs: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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