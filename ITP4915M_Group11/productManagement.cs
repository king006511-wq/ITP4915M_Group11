using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class ProductManagement : Form
    {
        // ==========================================
        // 🔒 Database Configuration
        // ==========================================
        private readonly string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // ==========================================
        // 🎨 Modern UI Element Variables
        // ==========================================
        private TextBox txtPartID, txtPartName, txtDescription, txtStockLevel, txtDefaultPrice;
        private DataGridView dgvProducts;
        private Button btnAdd, btnUpdate, btnDelete, btnClear;

        public ProductManagement()
        {
            InitializeComponent();
            InitializePremiumModernUI();
            LoadProductData();
        }

        #region 🎨 Dynamic Premium English UI Construction
        private void InitializePremiumModernUI()
        {
            // 1. Main Window Settings
            this.Text = "Premium Living Furniture - Product Maintenance";
            this.Size = new Size(1180, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
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

                // 🌟 高亮當前頁面
                if (item.Contains("Product Maintenance"))
                {
                    btnMenu.BackColor = Color.FromArgb(37, 99, 235);
                    btnMenu.ForeColor = Color.White;
                }
                else
                {
                    btnMenu.BackColor = Color.Transparent;
                    btnMenu.ForeColor = Color.FromArgb(148, 163, 184);
                    btnMenu.MouseEnter += (s, e) => { btnMenu.BackColor = Color.FromArgb(51, 65, 85); btnMenu.ForeColor = Color.White; };
                    btnMenu.MouseLeave += (s, e) => { btnMenu.BackColor = Color.Transparent; btnMenu.ForeColor = Color.FromArgb(148, 163, 184); };
                }

                // 🔗 Sidebar 頁面跳轉 (已確認用大寫 Class Name)
                btnMenu.Click += (s, e) => {
                    Form targetForm = null;
                    try
                    {
                        if (item.Contains("Sales Order Mgmt")) targetForm = new OrderManagementForm();
                        else if (item.Contains("Logistics")) targetForm = new LogisticsForm();
                        else if (item.Contains("Staff Mgmt")) targetForm = new EmployeeManagement(); // 大寫 E
                        else if (item.Contains("Received")) targetForm = new GoodsReceivedForm();
                        else if (item.Contains("Material Requests")) targetForm = new RawMaterialRequestForm();
                        else if (item.Contains("Procurement")) targetForm = new ProcurementForm();
                        else if (item.Contains("Support")) targetForm = new AfterServiceForm();
                        else if (item.Contains("Logout")) { Application.Restart(); return; }

                        if (targetForm != null && !(targetForm is ProductManagement))
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

            // 3. Right Main Workspace Panel
            Panel pnlMain = new Panel
            {
                Location = new Point(260, 0),
                Size = new Size(900, 750)
            };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label
            {
                Text = "Product Inventory Maintenance",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(30, 20),
                AutoSize = true
            };
            pnlMain.Controls.Add(lblHeader);

            // 4. Input Card Panel (左側輸入卡片)
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
                Text = "📦 Product Details",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 55;
            txtPartID = CreateStyledTextBox(pnlCard, ref startY, "Part ID *:", false);
            txtPartName = CreateStyledTextBox(pnlCard, ref startY, "Product Name *:", false);
            txtDescription = CreateStyledTextBox(pnlCard, ref startY, "Description:", false);
            txtStockLevel = CreateStyledTextBox(pnlCard, ref startY, "Stock Level:", false);
            txtDefaultPrice = CreateStyledTextBox(pnlCard, ref startY, "Default Price (HKD):", false);

            // 🌟 排列 CRUD 按鈕 (2x2 Grid 設計)
            int btnY = startY + 5;
            btnAdd = CreateActionButton(pnlCard, "➕ Add", Color.FromArgb(16, 185, 129), new Point(20, btnY));
            btnUpdate = CreateActionButton(pnlCard, "💾 Update", Color.FromArgb(37, 99, 235), new Point(210, btnY));
            btnDelete = CreateActionButton(pnlCard, "🗑️ Delete", Color.FromArgb(239, 68, 68), new Point(20, btnY + 55));
            btnClear = CreateActionButton(pnlCard, "🧹 Clear", Color.FromArgb(100, 116, 139), new Point(210, btnY + 55));

            btnAdd.Click += (s, e) => ExecuteAddProduct();
            btnUpdate.Click += (s, e) => ExecuteUpdateProduct();
            btnDelete.Click += (s, e) => ExecuteDeleteProduct();
            btnClear.Click += (s, e) => ClearFields();

            // 5. Data View Panel (右側 DataGridView)
            Label lblGridTitle = new Label
            {
                Text = "📋 Product Catalog",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(480, 85),
                AutoSize = true
            };
            pnlMain.Controls.Add(lblGridTitle);

            dgvProducts = new DataGridView
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

            dgvProducts.EnableHeadersVisualStyles = false;
            dgvProducts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProducts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvProducts.ColumnHeadersHeight = 38;
            dgvProducts.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvProducts.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
            dgvProducts.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            dgvProducts.CellClick += (s, e) => HandleGridClick(e.RowIndex);

            pnlMain.Controls.Add(dgvProducts);
        }

        private TextBox CreateStyledTextBox(Panel container, ref int topY, string labelText, bool readOnly)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(20, topY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(20, topY + 22), Width = 375, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            if (readOnly)
            {
                txt.ReadOnly = true;
                txt.BackColor = Color.FromArgb(241, 245, 249);
            }
            container.Controls.Add(lbl);
            container.Controls.Add(txt);
            topY += 62;
            return txt;
        }

        private Button CreateActionButton(Panel container, string text, Color bgColor, Point loc)
        {
            Button btn = new Button
            {
                Text = text,
                Location = loc,
                Size = new Size(185, 45),
                BackColor = bgColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            container.Controls.Add(btn);
            return btn;
        }
        #endregion

        #region 💾 Core CRUD Data Logic
        private void LoadProductData()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT PartID AS 'Part ID', Name AS 'Name', Description AS 'Description', StockLevel AS 'Stock', DefaultPrice AS 'Price HKD' FROM product_part";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            if (dgvProducts != null) dgvProducts.DataSource = dt;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to load product data, please ensure DB is running:\n" + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExecuteAddProduct()
        {
            if (string.IsNullOrWhiteSpace(txtPartID.Text) || string.IsNullOrWhiteSpace(txtPartName.Text))
            {
                MessageBox.Show("Part ID and Name cannot be empty!", "Validation Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = "INSERT INTO product_part (PartID, Name, Description, StockLevel, DefaultPrice) VALUES (@id, @name, @desc, @stock, @price)";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtPartID.Text.Trim());
                        cmd.Parameters.AddWithValue("@name", txtPartName.Text.Trim());
                        cmd.Parameters.AddWithValue("@desc", txtDescription.Text.Trim());
                        cmd.Parameters.AddWithValue("@stock", string.IsNullOrWhiteSpace(txtStockLevel.Text) ? 0 : Convert.ToInt32(txtStockLevel.Text));
                        cmd.Parameters.AddWithValue("@price", string.IsNullOrWhiteSpace(txtDefaultPrice.Text) ? 0.00m : Convert.ToDecimal(txtDefaultPrice.Text));

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            MessageBox.Show("Product added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            LoadProductData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to add product (Duplicate PartID?):\n" + ex.Message, "SQL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExecuteUpdateProduct()
        {
            if (string.IsNullOrWhiteSpace(txtPartID.Text) || string.IsNullOrWhiteSpace(txtPartName.Text))
            {
                MessageBox.Show("Please select a product from the table to edit!", "Validation Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = "UPDATE product_part SET Name = @name, Description = @desc, StockLevel = @stock, DefaultPrice = @price WHERE PartID = @id";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtPartID.Text.Trim());
                        cmd.Parameters.AddWithValue("@name", txtPartName.Text.Trim());
                        cmd.Parameters.AddWithValue("@desc", txtDescription.Text.Trim());
                        cmd.Parameters.AddWithValue("@stock", string.IsNullOrWhiteSpace(txtStockLevel.Text) ? 0 : Convert.ToInt32(txtStockLevel.Text));
                        cmd.Parameters.AddWithValue("@price", string.IsNullOrWhiteSpace(txtDefaultPrice.Text) ? 0.00m : Convert.ToDecimal(txtDefaultPrice.Text));

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            MessageBox.Show("Product updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            LoadProductData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update product:\n" + ex.Message, "SQL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExecuteDeleteProduct()
        {
            if (string.IsNullOrWhiteSpace(txtPartID.Text))
            {
                MessageBox.Show("Please select the product you want to delete from the table!", "Validation Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show($"Are you sure you want to delete product [{txtPartID.Text}]?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                string query = "DELETE FROM product_part WHERE PartID = @id";
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", txtPartID.Text.Trim());
                            int rows = cmd.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                MessageBox.Show("Product removed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearFields();
                                LoadProductData();
                            }
                            else
                            {
                                MessageBox.Show("Delete failed! Product ID not found in database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error executing delete (May have foreign key constraints):\n" + ex.Message, "SQL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void HandleGridClick(int rowIndex)
        {
            if (rowIndex >= 0 && dgvProducts != null)
            {
                try
                {
                    DataGridViewRow row = dgvProducts.Rows[rowIndex];
                    txtPartID.Text = row.Cells["Part ID"].Value?.ToString();
                    txtPartName.Text = row.Cells["Name"].Value?.ToString();
                    txtDescription.Text = row.Cells["Description"].Value?.ToString();
                    txtStockLevel.Text = row.Cells["Stock"].Value?.ToString();
                    txtDefaultPrice.Text = row.Cells["Price HKD"].Value?.ToString();

                    // 鎖定 Primary Key 防手殘改錯
                    txtPartID.ReadOnly = true;
                    txtPartID.BackColor = Color.FromArgb(241, 245, 249);
                }
                catch { }
            }
        }

        private void ClearFields()
        {
            txtPartID.Clear();
            txtPartName.Clear();
            txtDescription.Clear();
            txtStockLevel.Clear();
            txtDefaultPrice.Clear();

            // 解除鎖定
            txtPartID.ReadOnly = false;
            txtPartID.BackColor = Color.White;
        }
        #endregion
    }
}