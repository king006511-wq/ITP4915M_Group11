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

        // 🔒 XAMPP Database Connection String Configuration
        private readonly string connectionString = "Server=localhost;Database=premium_living_db;Uid=root;Pwd=;port=3306;SslMode=Disabled;";

        public ProductManagement()
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                InitializePremiumModernUI();
                LoadDatabaseData();
            }
        }

        #region 🎨 Premium Unified Modern UI Construction Engine
        private void InitializePremiumModernUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Product Maintenance & Catalog Control";
            this.Size = new Size(1180, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // 1. Left Sidebar Navigation Container
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

                if (item.Contains("Product Maintenance"))
                {
                    btnMenu.BackColor = Color.FromArgb(37, 99, 235); btnMenu.ForeColor = Color.White;
                }
                else if (item.Contains("Logout"))
                {
                    btnMenu.BackColor = Color.FromArgb(239, 68, 68); btnMenu.ForeColor = Color.White;
                    btnMenu.MouseEnter += (s, e) => { btnMenu.BackColor = Color.FromArgb(220, 38, 38); };
                    btnMenu.MouseLeave += (s, e) => { btnMenu.BackColor = Color.FromArgb(239, 68, 68); };
                }
                else
                {
                    btnMenu.BackColor = Color.Transparent; btnMenu.ForeColor = Color.FromArgb(148, 163, 184);
                    btnMenu.MouseEnter += (s, e) => { btnMenu.BackColor = Color.FromArgb(51, 65, 85); btnMenu.ForeColor = Color.White; };
                    btnMenu.MouseLeave += (s, e) => { btnMenu.BackColor = Color.Transparent; btnMenu.ForeColor = Color.FromArgb(148, 163, 184); };
                }

                btnMenu.Click += (s, e) => {
                    Form targetForm = null;
                    try
                    {
                        if (item.Contains("Sales Order")) targetForm = new OrderManagementForm();
                        else if (item.Contains("Logistics")) targetForm = new LogisticsForm();
                        else if (item.Contains("Product")) targetForm = new ProductManagement();
                        else if (item.Contains("HR")) targetForm = new EmployeeManagement();
                        else if (item.Contains("GRN")) targetForm = new GoodsReceivedForm();
                        else if (item.Contains("Material")) targetForm = new RawMaterialRequestForm();
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
                    catch (Exception ex) { MessageBox.Show("Navigation error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                };
                pnlSidebar.Controls.Add(btnMenu);
                btnTop += 55;
            }
            this.Controls.Add(pnlSidebar);

            // 2. Right Workspace Controller Panel Area
            Panel pnlMain = new Panel { Location = new Point(260, 0), Size = new Size(900, 750) };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label { Text = "Product Inventory Maintenance System", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(30, 20), AutoSize = true };
            pnlMain.Controls.Add(lblHeader);

            // Back Home button
            Button btnBackHome = new Button { Text = "🏠 Back Home", Size = new Size(120, 34), Location = new Point(740, 22), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnBackHome.FlatAppearance.BorderSize = 0;
            btnBackHome.Click += (s, e) => { NavigationHelper.GoToMainDashboard(this); };
            pnlMain.Controls.Add(btnBackHome);

            // 3. Input Details Dashboard Card
            Panel pnlCard = new Panel { Location = new Point(30, 85), Size = new Size(380, 600), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlCard);

            Label lblCardTitle = new Label { Text = "📦 Product Management Details", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(20, 15), AutoSize = true };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 60;
            txtPartID = CreateStyledTextBox(pnlCard, ref startY, "Part ID *:", false);
            txtProductName = CreateStyledTextBox(pnlCard, ref startY, "Product Name *:", false);
            txtStockLevel = CreateStyledTextBox(pnlCard, ref startY, "Stock Level:", false);
            txtDefaultPrice = CreateStyledTextBox(pnlCard, ref startY, "Default Price (HKD):", false);

            Label lblDesc = new Label { Text = "🔍 Live Form Search Filter keyword:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235) };
            txtDescription = new TextBox { Location = new Point(20, startY + 22), Width = 335, Height = 70, Multiline = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            txtDescription.TextChanged += txtDescription_TextChanged; // Wire real-time list filter handler
            pnlCard.Controls.Add(lblDesc);
            pnlCard.Controls.Add(txtDescription);
            startY += 105;

            // Action Buttons Construction
            Button btnAdd = new Button { Text = "➕ Add", Location = new Point(20, startY), Size = new Size(160, 42), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            Button btnUpdate = new Button { Text = "💾 Update", Location = new Point(195, startY), Size = new Size(160, 42), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            Button btnDelete = new Button { Text = "🗑️ Delete", Location = new Point(20, startY + 50), Size = new Size(160, 42), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            Button btnClear = new Button { Text = "🧹 Clear Forms", Location = new Point(195, startY + 50), Size = new Size(160, 42), BackColor = Color.FromArgb(71, 85, 105), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };

            foreach (var b in new Button[] { btnAdd, btnUpdate, btnDelete, btnClear }) b.FlatAppearance.BorderSize = 0;
            pnlCard.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });

            // Wire action behaviors
            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnClear.Click += (s, e) => ClearFields();

            // 4. Clean Unified Data Grid Component
            Label lblGridTitle = new Label { Text = "📋 Real-Time Product Catalog Records", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(440, 85), AutoSize = true };
            pnlMain.Controls.Add(lblGridTitle);

            dgvProductCatalog = new DataGridView { Location = new Point(440, 125), Size = new Size(430, 560), BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvProductCatalog.EnableHeadersVisualStyles = false;
            dgvProductCatalog.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvProductCatalog.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProductCatalog.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvProductCatalog.ColumnHeadersHeight = 38;
            dgvProductCatalog.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvProductCatalog.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);

            dgvProductCatalog.SelectionChanged += dgvProductCatalog_SelectionChanged;
            pnlMain.Controls.Add(dgvProductCatalog);
        }

        private TextBox CreateStyledTextBox(Panel container, ref int topY, string labelText, bool readOnly)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(20, topY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(20, topY + 22), Width = 335, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            if (readOnly) { txt.ReadOnly = true; txt.BackColor = Color.FromArgb(241, 245, 249); }
            container.Controls.Add(lbl); container.Controls.Add(txt);
            topY += 65;
            return txt;
        }
        #endregion

        #region 📦 Business Management Logic Functions
        private void LoadDatabaseData()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT PartID AS 'Part ID', PartName AS 'Name', StockLevel AS 'Stock', DefaultPrice AS 'Price HKD' FROM product_part";
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

        private void txtDescription_TextChanged(object sender, EventArgs e)
        {
            if (dgvProductCatalog.DataSource is DataTable dt)
            {
                string keyword = txtDescription.Text.Trim().Replace("'", "''");
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    dt.DefaultView.RowFilter = "";
                }
                else
                {
                    dt.DefaultView.RowFilter = string.Format("[Part ID] LIKE '%{0}%' OR [Name] LIKE '%{0}%'", keyword);
                }
            }
        }

        private void dgvProductCatalog_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProductCatalog.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvProductCatalog.SelectedRows[0];
                txtPartID.Text = row.Cells["Part ID"].Value?.ToString() ?? "";
                txtProductName.Text = row.Cells["Name"].Value?.ToString() ?? "";
                txtStockLevel.Text = row.Cells["Stock"].Value?.ToString() ?? "";
                txtDefaultPrice.Text = row.Cells["Price HKD"].Value?.ToString() ?? "";
                txtPartID.ReadOnly = true;
                txtPartID.BackColor = Color.FromArgb(241, 245, 249);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPartID.Text) || string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Please fill in Part ID and Product Name fields!", "Missing Required Fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO product_part (PartID, PartName, StockLevel, DefaultPrice) VALUES (@partID, @name, @stock, @price)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@partID", txtPartID.Text.Trim());
                        cmd.Parameters.AddWithValue("@name", txtProductName.Text.Trim());
                        cmd.Parameters.AddWithValue("@stock", string.IsNullOrEmpty(txtStockLevel.Text) ? 0 : Convert.ToInt32(txtStockLevel.Text.Trim()));
                        cmd.Parameters.AddWithValue("@price", string.IsNullOrEmpty(txtDefaultPrice.Text) ? 0 : Convert.ToDecimal(txtDefaultPrice.Text.Trim()));
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Product was added successfully!", "Transaction Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDatabaseData();
                    ClearFields();
                }
            }
            catch (Exception ex) { MessageBox.Show("Database Failure: \n" + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPartID.Text))
            {
                MessageBox.Show("Please click on an active catalog item to initiate modification cycles.", "System Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE product_part SET PartName=@name, StockLevel=@stock, DefaultPrice=@price WHERE PartID=@partID";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@partID", txtPartID.Text.Trim());
                        cmd.Parameters.AddWithValue("@name", txtProductName.Text.Trim());
                        cmd.Parameters.AddWithValue("@stock", string.IsNullOrEmpty(txtStockLevel.Text) ? 0 : Convert.ToInt32(txtStockLevel.Text.Trim()));
                        cmd.Parameters.AddWithValue("@price", string.IsNullOrEmpty(txtDefaultPrice.Text) ? 0 : Convert.ToDecimal(txtDefaultPrice.Text.Trim()));

                        int rowCount = cmd.ExecuteNonQuery();
                        if (rowCount > 0)
                        {
                            MessageBox.Show("Inventory product info has updated correctly!", "Transaction Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadDatabaseData();
                            ClearFields();
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Update Operation Failure:\n" + ex.Message, "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPartID.Text))
            {
                MessageBox.Show("Select an active catalog item from the history panel to drop.", "Deletion Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult confirm = MessageBox.Show($"Are you sure you want to permanently erase catalog item record ID [{txtPartID.Text}]?", "Confirm Erase Sequence", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.No) return;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM product_part WHERE PartID=@partID";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@partID", txtPartID.Text.Trim());
                        int dynamicRows = cmd.ExecuteNonQuery();
                        if (dynamicRows > 0)
                        {
                            MessageBox.Show("Product was dropped successfully!", "Record Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadDatabaseData();
                            ClearFields();
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Critical deletion database block caught: " + ex.Message, "Error Processing Operation", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void ClearFields()
        {
            txtPartID.Clear();
            txtProductName.Clear();
            txtStockLevel.Clear();
            txtDefaultPrice.Clear();
            txtDescription.Clear();
            txtPartID.ReadOnly = false;
            txtPartID.BackColor = Color.White;
            dgvProductCatalog.ClearSelection();
        }
        #endregion

        private void ProductManagement_Load(object sender, EventArgs e)
        {

        }
    }
}