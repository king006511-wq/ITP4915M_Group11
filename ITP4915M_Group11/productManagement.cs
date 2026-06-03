using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ITP4915M_Group11
{
    public partial class ProductManagement : Form
    {
        private TextBox txtPartID;
        private TextBox txtProductName;
        private TextBox txtDescription;
        private TextBox txtStockLevel;
        private TextBox txtDefaultPrice;
        private DataGridView dgvProductCatalog;

        // 📝 XAMPP Database Connection String
        private string connectionString = "Server=localhost;Database=premium_living_db;Uid=root;Pwd=;";

        public ProductManagement()
        {
            this.Text = "Premium Living Furniture - Product Maintenance";
            this.Size = new Size(1300, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);

            InitializeUniversalSidebar();
            InitializeProductFormLayout();

            // Load data automatically when the form is initialized
            LoadDatabaseData();
        }

        private void InitializeUniversalSidebar()
        {
            Panel pnlSidebar = new Panel { Width = 260, Dock = DockStyle.Left, BackColor = Color.FromArgb(17, 24, 39), Name = "pnlSidebar" };
            this.Controls.Add(pnlSidebar);

            Label lblTitle = new Label { Text = "Premium Living\nFurniture", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 25), Size = new Size(220, 60), TextAlign = ContentAlignment.MiddleLeft };
            pnlSidebar.Controls.Add(lblTitle);

            string[] menuItems = { "Sales Order Mgmt", "Delivery Logistics", "Product Maintenance", "HR / Staff Mgmt", "Goods Received (GRN)", "Material Requests", "Procurement Control", "Customer Support", "Logout System" };
            int btnTop = 110;
            foreach (string item in menuItems)
            {
                Button btnMenu = new Button { Text = item, Size = new Size(260, 50), Location = new Point(0, btnTop), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Regular), ForeColor = Color.FromArgb(156, 163, 175), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(25, 0, 0, 0), Cursor = Cursors.Hand };
                btnMenu.FlatAppearance.BorderSize = 0;
                btnMenu.FlatAppearance.MouseOverBackColor = Color.FromArgb(31, 41, 55);


                if (item.Contains("Product Maintenance"))
                {
                    btnMenu.BackColor = Color.FromArgb(37, 99, 235);
                    btnMenu.ForeColor = Color.White;
                    btnMenu.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
                else if (item.Contains("Logout"))
                {
                    btnMenu.BackColor = Color.FromArgb(239, 68, 68);
                    btnMenu.ForeColor = Color.White;
                    btnMenu.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 38, 38);
                }

                btnMenu.Click += (s, e) => {
                    Form targetForm = null;
                    try
                    {
                        if (item.Contains("Sales Order Mgmt")) targetForm = new OrderManagementForm();
                        else if (item.Contains("Logistics")) targetForm = new LogisticsForm();
                        else if (item.Contains("Product Maintenance")) targetForm = new ProductManagement();
                        else if (item.Contains("Staff Mgmt")) targetForm = new EmployeeManagement();
                        else if (item.Contains("Received")) targetForm = new GoodsReceivedForm();
                        else if (item.Contains("Material Requests")) targetForm = new RawMaterialRequestForm();
                        else if (item.Contains("Procurement")) targetForm = new ProcurementForm();
                        else if (item.Contains("Support")) targetForm = new AfterServiceForm();
                        else if (item.Contains("Logout")) { Application.Restart(); return; }

                        if (targetForm != null)
                        {
                            if (targetForm.GetType() == this.GetType()) { targetForm.Dispose(); return; }
                            this.Hide();
                            targetForm.FormClosed += (sf, args) => this.Show();
                            targetForm.Show();
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Navigation Error: " + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                };
                pnlSidebar.Controls.Add(btnMenu);
                btnTop += 52;
            }
        }

        private void InitializeProductFormLayout()
        {
            Label lblMainTitle = new Label { Text = "Product Inventory Maintenance", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.FromArgb(17, 24, 39), Location = new Point(290, 25), Size = new Size(500, 45) };
            this.Controls.Add(lblMainTitle);

            Panel pnlDetails = new Panel { Location = new Point(290, 90), Size = new Size(450, 680), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(pnlDetails);

            Label lblSection1 = new Label { Text = "📦 Product Details", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(20, 15), Size = new Size(200, 25) };
            pnlDetails.Controls.Add(lblSection1);

            int startY = 60;
            txtPartID = CreateFormInput(pnlDetails, "Part ID *:", startY);
            txtProductName = CreateFormInput(pnlDetails, "Product Name *:", startY += 75);
            txtDescription = CreateFormInput(pnlDetails, "Description:", startY += 75);
            txtStockLevel = CreateFormInput(pnlDetails, "Stock Level:", startY += 75);
            txtDefaultPrice = CreateFormInput(pnlDetails, "Default Price (HKD):", startY += 75);

            Button btnAdd = new Button { Text = "➕ Add", Location = new Point(20, 460), Size = new Size(195, 45), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            Button btnUpdate = new Button { Text = "💾 Update", Location = new Point(230, 460), Size = new Size(195, 45), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            Button btnDelete = new Button { Text = "🗑️ Delete", Location = new Point(20, 520), Size = new Size(195, 45), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            Button btnClear = new Button { Text = "🧹 Clear", Location = new Point(230, 520), Size = new Size(195, 45), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            foreach (var b in new Button[] { btnAdd, btnUpdate, btnDelete, btnClear }) b.FlatAppearance.BorderSize = 0;
            pnlDetails.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });

            // ==========================================
            // ➕ ADD Button Logic
            // ==========================================
            btnAdd.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtPartID.Text) || string.IsNullOrWhiteSpace(txtProductName.Text))
                {
                    MessageBox.Show("Please fill in Part ID and Product Name!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "INSERT INTO product_part (PartID, Name, Description, StockLevel, DefaultPrice) VALUES (@partID, @name, @desc, @stock, @price)";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@partID", txtPartID.Text);
                            cmd.Parameters.AddWithValue("@name", txtProductName.Text);
                            cmd.Parameters.AddWithValue("@desc", txtDescription.Text);
                            cmd.Parameters.AddWithValue("@stock", string.IsNullOrEmpty(txtStockLevel.Text) ? 0 : Convert.ToInt32(txtStockLevel.Text));
                            cmd.Parameters.AddWithValue("@price", string.IsNullOrEmpty(txtDefaultPrice.Text) ? 0 : Convert.ToDecimal(txtDefaultPrice.Text));
                            cmd.ExecuteNonQuery();
                        }
                        MessageBox.Show("Product added successfully!", "System Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDatabaseData();
                        btnClear.PerformClick(); // Clear fields after adding
                    }
                }
                catch (Exception ex) { MessageBox.Show("Database Error (Possible duplicate Part ID): \n" + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            // ==========================================
            // 💾 UPDATE Button Logic
            // ==========================================
            btnUpdate.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtPartID.Text))
                {
                    MessageBox.Show("Please double-click a product from the catalog on the right to update!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "UPDATE product_part SET Name=@name, Description=@desc, StockLevel=@stock, DefaultPrice=@price WHERE PartID=@partID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@partID", txtPartID.Text);
                            cmd.Parameters.AddWithValue("@name", txtProductName.Text);
                            cmd.Parameters.AddWithValue("@desc", txtDescription.Text);
                            cmd.Parameters.AddWithValue("@stock", string.IsNullOrEmpty(txtStockLevel.Text) ? 0 : Convert.ToInt32(txtStockLevel.Text));
                            cmd.Parameters.AddWithValue("@price", string.IsNullOrEmpty(txtDefaultPrice.Text) ? 0 : Convert.ToDecimal(txtDefaultPrice.Text));

                            int rows = cmd.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                MessageBox.Show("Product data updated successfully!", "System Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadDatabaseData();
                                btnClear.PerformClick();
                            }
                            else
                            {
                                MessageBox.Show("Product not found. Update failed.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Update Error: " + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            // ==========================================
            // 🗑️ DELETE Button Logic
            // ==========================================
            btnDelete.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtPartID.Text))
                {
                    MessageBox.Show("Please double-click a product from the catalog on the right to delete!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DialogResult confirm = MessageBox.Show($"Are you sure you want to permanently delete product [{txtPartID.Text}]?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.No) return;

                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "DELETE FROM product_part WHERE PartID=@partID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@partID", txtPartID.Text);
                            int rows = cmd.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                MessageBox.Show("Product deleted successfully!", "System Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadDatabaseData();
                                btnClear.PerformClick();
                            }
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Delete Error: " + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            // 🧹 CLEAR Button Logic
            btnClear.Click += (s, e) => {
                txtPartID.Clear(); txtProductName.Clear(); txtDescription.Clear(); txtStockLevel.Clear(); txtDefaultPrice.Clear();
                txtPartID.ReadOnly = false; // Unlock Part ID field for creating new products
            };

            // Right Catalog Panel
            Panel pnlCatalog = new Panel { Location = new Point(760, 90), Size = new Size(490, 680), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(pnlCatalog);

            Label lblSection2 = new Label { Text = "📋 Product Catalog", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(17, 24, 39), Location = new Point(15, 15), Size = new Size(200, 25) };
            pnlCatalog.Controls.Add(lblSection2);

            dgvProductCatalog = new DataGridView { Location = new Point(15, 55), Size = new Size(460, 600), BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, ReadOnly = true };
            pnlCatalog.Controls.Add(dgvProductCatalog);

            // =========================================================
            // 🖱️ Double-Click Event: Populate textboxes from grid
            // =========================================================
            dgvProductCatalog.CellDoubleClick += (s, e) => {
                if (e.RowIndex >= 0)
                { // Ensure the clicked row is a valid data row, not the header
                    DataGridViewRow row = dgvProductCatalog.Rows[e.RowIndex];

                    // Extract data from the selected row and write to textboxes
                    txtPartID.Text = row.Cells["Part ID"].Value?.ToString();
                    txtProductName.Text = row.Cells["Name"].Value?.ToString();
                    txtDescription.Text = row.Cells["Description"].Value?.ToString();
                    txtStockLevel.Text = row.Cells["Stock"].Value?.ToString();
                    txtDefaultPrice.Text = row.Cells["Price HKD"].Value?.ToString();

                    // 🔒 Security mechanism: Make Part ID read-only when editing an existing product
                    txtPartID.ReadOnly = true;
                }
            };
        }

        private TextBox CreateFormInput(Panel parent, string labelText, int topY)
        {
            Label lbl = new Label { Text = labelText, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(55, 65, 81), Location = new Point(20, topY), Size = new Size(200, 20) };
            TextBox txt = new TextBox { Location = new Point(20, topY + 22), Size = new Size(405, 25), Font = new Font("Segoe UI", 10) };
            parent.Controls.Add(lbl); parent.Controls.Add(txt);
            return txt;
        }

        // ==========================================
        // 🔥 Database Load: Fetch records from product_part table
        // ==========================================
        private void LoadDatabaseData()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT PartID AS 'Part ID', Name AS 'Name', Description AS 'Description', StockLevel AS 'Stock', DefaultPrice AS 'Price HKD' FROM product_part";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvProductCatalog.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                DataTable errorDt = new DataTable();
                errorDt.Columns.Add("System Status");
                errorDt.Rows.Add("Database Error: " + ex.Message);
                dgvProductCatalog.DataSource = errorDt;
            }
        }
    }
}