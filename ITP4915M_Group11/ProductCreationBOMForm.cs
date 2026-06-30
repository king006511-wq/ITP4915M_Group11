using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ITP4915M_Group11
{
    public class MaterialComboItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }

    public class ProductComboItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string DisplayText => $"[{ID}] {Name}";
    }

    public partial class ProductCreationBOMForm : Form
    {
        // 🔗 Callback delegate to notify parent form to switch views
        public Action OnNavigationBack { get; set; }

        // 🔒 Central Database Connection String
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // 💾 Temporary cart for the BOM recipe
        private DataTable dtBOMCart;

        // 🎨 UI Controls
        private RadioButton rbNewProduct, rbUpdateProduct;
        private TextBox txtProductID, txtProductName, txtRetailPrice, txtMaterialQty;
        private ComboBox cmbExistingProducts, cmbRawMaterial;
        private DataGridView dgvBOMCart;
        private Button btnAddMaterial, btnRemoveMaterial, btnSaveProduct;
        private Label lblProductIDTitle;

        public ProductCreationBOMForm()
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                InitializeComponentDataCart();
                SetupPremiumCreationUI();
                LoadRawMaterialsToCombo();

                // Set initial state: locked and blank until a mode is selected
                SetFormState(FormMode.None);
            }
        }

        private void InitializeComponentDataCart()
        {
            dtBOMCart = new DataTable();
            dtBOMCart.Columns.Add("Material ID", typeof(string));
            dtBOMCart.Columns.Add("Material Name", typeof(string));
            dtBOMCart.Columns.Add("Qty Required", typeof(decimal));
        }

        private enum FormMode { None, New, Update }

        #region 🎨 Premium Unified Modern UI Construction
        private void SetupPremiumCreationUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Product R&D & BOM Design";
            this.Size = new Size(1150, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            Panel pnlMain = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(25) };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label { Text = "✨ Product Development & BOM Setup", Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(25, 20), AutoSize = true };
            pnlMain.Controls.Add(lblHeader);

            // --- 1. Left Panel: Product Profile & Mode Selection ---
            Panel pnlLeftCard = new Panel { Location = new Point(25, 75), Size = new Size(420, 580), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlLeftCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlLeftCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlLeftCard);

            Label lblSec1 = new Label { Text = "📦 Product Profile & Identity", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(20, 15), AutoSize = true };
            pnlLeftCard.Controls.Add(lblSec1);

            // Mode Selection Radio Buttons
            rbNewProduct = new RadioButton { Text = "🆕 Create New Product", Location = new Point(20, 50), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            rbUpdateProduct = new RadioButton { Text = "✏️ Update Existing Product", Location = new Point(200, 50), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            pnlLeftCard.Controls.Add(rbNewProduct);
            pnlLeftCard.Controls.Add(rbUpdateProduct);

            rbNewProduct.CheckedChanged += Mode_CheckedChanged;
            rbUpdateProduct.CheckedChanged += Mode_CheckedChanged;

            Label lblLine = new Label { Text = "──────────────────────────────────────────", Location = new Point(20, 80), AutoSize = true, ForeColor = Color.FromArgb(241, 245, 249) };
            pnlLeftCard.Controls.Add(lblLine);

            int startY = 100;

            // Dynamic Product ID Area
            lblProductIDTitle = new Label { Text = "Product ID:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            pnlLeftCard.Controls.Add(lblProductIDTitle);

            txtProductID = new TextBox { Location = new Point(20, startY + 22), Width = 375, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle, ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249) };
            pnlLeftCard.Controls.Add(txtProductID);

            cmbExistingProducts = new ComboBox { Location = new Point(20, startY + 22), Width = 375, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F), Visible = false };
            cmbExistingProducts.SelectedIndexChanged += cmbExistingProducts_SelectedIndexChanged;
            pnlLeftCard.Controls.Add(cmbExistingProducts);

            startY += 65;

            txtProductName = CreateInputField(pnlLeftCard, ref startY, "Product Name *:");
            txtRetailPrice = CreateInputField(pnlLeftCard, ref startY, "Retail Price (HKD) *:");

            // --- 2. Right Panel: BOM Recipe Setup ---
            Panel pnlRightCard = new Panel { Location = new Point(470, 75), Size = new Size(640, 580), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlRightCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlRightCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlRightCard);

            Label lblSec2 = new Label { Text = "🛠️ Bill of Materials (BOM) Formulation", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(79, 70, 229), Location = new Point(20, 15), AutoSize = true };
            pnlRightCard.Controls.Add(lblSec2);

            Label lblMat = new Label { Text = "Select Raw Material Component:", Location = new Point(20, 55), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cmbRawMaterial = new ComboBox { Location = new Point(20, 77), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F) };
            pnlRightCard.Controls.Add(lblMat); pnlRightCard.Controls.Add(cmbRawMaterial);

            Label lblMatQty = new Label { Text = "Quantity Required per Unit:", Location = new Point(320, 55), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtMaterialQty = new TextBox { Location = new Point(320, 77), Width = 120, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            pnlRightCard.Controls.Add(lblMatQty); pnlRightCard.Controls.Add(txtMaterialQty);

            btnAddMaterial = new Button { Text = "➕ Add to Formula", Location = new Point(460, 74), Size = new Size(160, 32), BackColor = Color.FromArgb(79, 70, 229), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAddMaterial.FlatAppearance.BorderSize = 0;
            btnAddMaterial.Click += btnAddMaterial_Click;
            pnlRightCard.Controls.Add(btnAddMaterial);

            Label lblCartTitle = new Label { Text = "📋 Structured Product Ingredients (Recipe Preview):", Location = new Point(20, 125), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            pnlRightCard.Controls.Add(lblCartTitle);

            dgvBOMCart = new DataGridView
            {
                Location = new Point(20, 150),
                Size = new Size(600, 340),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };
            dgvBOMCart.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgvBOMCart.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvBOMCart.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvBOMCart.DataSource = dtBOMCart;
            pnlRightCard.Controls.Add(dgvBOMCart);

            btnRemoveMaterial = new Button { Text = "🗑️ Remove Component", Location = new Point(20, 500), Size = new Size(180, 32), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnRemoveMaterial.FlatAppearance.BorderSize = 0;
            btnRemoveMaterial.Click += btnRemoveMaterial_Click;
            pnlRightCard.Controls.Add(btnRemoveMaterial);

            // --- 3. Bottom Global Action Button ---
            btnSaveProduct = new Button { Text = "💾 Save & Deploy Product", Location = new Point(880, 665), Size = new Size(230, 42), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSaveProduct.FlatAppearance.BorderSize = 0;
            btnSaveProduct.Click += btnSaveProduct_Click;
            pnlMain.Controls.Add(btnSaveProduct);
        }

        private TextBox CreateInputField(Panel container, ref int topY, string labelText)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(20, topY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(20, topY + 22), Width = 375, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            container.Controls.Add(lbl); container.Controls.Add(txt);
            topY += 65;
            return txt;
        }
        #endregion

        #region 🔄 Dynamic State & Data Loading

        private void SetFormState(FormMode mode)
        {
            // 🌟 核心修正：避免切換狀態時引發 ComboBox 的事件連鎖反應
            cmbExistingProducts.SelectedIndexChanged -= cmbExistingProducts_SelectedIndexChanged;

            txtProductID.Clear();
            txtProductName.Clear();
            txtRetailPrice.Clear();
            txtMaterialQty.Clear();
            cmbRawMaterial.SelectedIndex = -1;
            dtBOMCart.Rows.Clear();

            if (mode == FormMode.None)
            {
                txtProductName.Enabled = false;
                txtRetailPrice.Enabled = false;
                cmbRawMaterial.Enabled = false;
                txtMaterialQty.Enabled = false;
                btnAddMaterial.Enabled = false;
                btnRemoveMaterial.Enabled = false;
                btnSaveProduct.Enabled = false;

                txtProductID.Visible = true;
                cmbExistingProducts.Visible = false;
                lblProductIDTitle.Text = "Product ID:";
            }
            else if (mode == FormMode.New)
            {
                txtProductName.Enabled = true;
                txtRetailPrice.Enabled = true;
                cmbRawMaterial.Enabled = true;
                txtMaterialQty.Enabled = true;
                btnAddMaterial.Enabled = true;
                btnRemoveMaterial.Enabled = true;
                btnSaveProduct.Enabled = true;

                txtProductID.Visible = true;
                cmbExistingProducts.Visible = false;
                lblProductIDTitle.Text = "New Product ID (Auto-Generated):";

                GenerateNextProductID();
                btnSaveProduct.Text = "💾 Deploy New Product";
            }
            else if (mode == FormMode.Update)
            {
                txtProductName.Enabled = false;
                txtRetailPrice.Enabled = false;
                cmbRawMaterial.Enabled = false;
                txtMaterialQty.Enabled = false;
                btnAddMaterial.Enabled = false;
                btnRemoveMaterial.Enabled = false;
                btnSaveProduct.Enabled = false;

                txtProductID.Visible = false;
                cmbExistingProducts.Visible = true;

                // 先加載資料源，再安全歸零
                LoadExistingProductsToCombo();
                cmbExistingProducts.SelectedIndex = -1;

                lblProductIDTitle.Text = "Select Existing Product to Update:";
                btnSaveProduct.Text = "💾 Update Product & BOM";
            }

            // 重新綁定事件監聽
            cmbExistingProducts.SelectedIndexChanged += cmbExistingProducts_SelectedIndexChanged;
        }

        private void Mode_CheckedChanged(object sender, EventArgs e)
        {
            if (rbNewProduct.Checked) SetFormState(FormMode.New);
            else if (rbUpdateProduct.Checked) SetFormState(FormMode.Update);
        }

        private void LoadExistingProductsToCombo()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT ProductID, ProductName FROM product ORDER BY ProductID ASC";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<ProductComboItem> items = new List<ProductComboItem>();
                        while (reader.Read())
                        {
                            items.Add(new ProductComboItem
                            {
                                ID = reader["ProductID"].ToString(),
                                Name = reader["ProductName"].ToString()
                            });
                        }
                        cmbExistingProducts.DataSource = items;
                        cmbExistingProducts.DisplayMember = "DisplayText";
                        cmbExistingProducts.ValueMember = "ID";
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error loading products: " + ex.Message); }
            }
        }

        private void cmbExistingProducts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rbUpdateProduct.Checked && cmbExistingProducts.SelectedItem is ProductComboItem selectedProduct)
            {
                txtProductName.Enabled = true;
                txtRetailPrice.Enabled = true;
                cmbRawMaterial.Enabled = true;
                txtMaterialQty.Enabled = true;
                btnAddMaterial.Enabled = true;
                btnRemoveMaterial.Enabled = true;
                btnSaveProduct.Enabled = true;

                LoadProductDetails(selectedProduct.ID);
            }
        }

        private void LoadProductDetails(string productID)
        {
            if (string.IsNullOrEmpty(productID)) return;

            dtBOMCart.Rows.Clear();
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // 1. Load basic info
                    string infoQuery = "SELECT ProductName, RetailPrice FROM product WHERE ProductID = @id";
                    using (MySqlCommand cmdInfo = new MySqlCommand(infoQuery, conn))
                    {
                        cmdInfo.Parameters.AddWithValue("@id", productID);
                        using (MySqlDataReader reader = cmdInfo.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtProductName.Text = reader["ProductName"].ToString();
                                txtRetailPrice.Text = reader["RetailPrice"].ToString();
                            }
                        }
                    }

                    // 2. Load BOM into Cart
                    string bomQuery = @"
                        SELECT b.MaterialID, r.MaterialName, b.QuantityRequired 
                        FROM bill_of_materials b 
                        INNER JOIN raw_material r ON b.MaterialID = r.MaterialID 
                        WHERE b.ProductID = @id";

                    using (MySqlCommand cmdBom = new MySqlCommand(bomQuery, conn))
                    {
                        // 🌟 核心修正：補返之前漏咗嘅參數綁定，徹底清除 Fatal Error
                        cmdBom.Parameters.AddWithValue("@id", productID);

                        using (MySqlDataReader reader = cmdBom.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string mID = reader["MaterialID"].ToString();
                                string mName = $"[{mID}] {reader["MaterialName"]}";
                                decimal qty = Convert.ToDecimal(reader["QuantityRequired"]);
                                dtBOMCart.Rows.Add(mID, mName, qty);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading product details: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void GenerateNextProductID()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT ProductID FROM product ORDER BY ProductID DESC LIMIT 1";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            string lastID = result.ToString();
                            if (lastID.StartsWith("P") && lastID.Length > 1)
                            {
                                string numPart = lastID.Substring(1);
                                if (int.TryParse(numPart, out int currentNum))
                                {
                                    int nextNum = currentNum + 1;
                                    txtProductID.Text = "P" + nextNum.ToString("D3");
                                    return;
                                }
                            }
                        }
                        txtProductID.Text = "P001";
                    }
                }
                catch { txtProductID.Text = "PERR"; }
            }
        }

        private void LoadRawMaterialsToCombo()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT rm.MaterialID, rm.MaterialName, COUNT(sm.SupplierID) AS SuppCount 
                        FROM raw_material rm 
                        LEFT JOIN supplier_material sm ON rm.MaterialID = sm.MaterialID 
                        GROUP BY rm.MaterialID, rm.MaterialName 
                        ORDER BY rm.MaterialID ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<MaterialComboItem> items = new List<MaterialComboItem>();
                        while (reader.Read())
                        {
                            int suppCount = Convert.ToInt32(reader["SuppCount"]);
                            string warningLabel = suppCount == 0 ? " (⚠️ No Supplier Assigned)" : "";

                            items.Add(new MaterialComboItem
                            {
                                ID = reader["MaterialID"].ToString(),
                                Name = $"[{reader["MaterialID"]}] {reader["MaterialName"]}{warningLabel}"
                            });
                        }
                        cmbRawMaterial.DataSource = items;
                        cmbRawMaterial.DisplayMember = "Name";
                        cmbRawMaterial.ValueMember = "ID";
                        cmbRawMaterial.SelectedIndex = -1;
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error fetching components: " + ex.Message); }
            }
        }
        #endregion

        #region ⚡ R&D Interactive Logic (BOM Cart)

        private void btnAddMaterial_Click(object sender, EventArgs e)
        {
            if (cmbRawMaterial.SelectedItem == null)
            {
                MessageBox.Show("Please select a valid raw material component.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🌟 優化：加上語系安全解析
            if (!decimal.TryParse(txtMaterialQty.Text.Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal qtyRequired) || qtyRequired <= 0)
            {
                MessageBox.Show("Please specify a logical required quantity (greater than 0).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MaterialComboItem selectedMaterial = (MaterialComboItem)cmbRawMaterial.SelectedItem;

            foreach (DataRow row in dtBOMCart.Rows)
            {
                if (row["Material ID"].ToString() == selectedMaterial.ID)
                {
                    row["Qty Required"] = Convert.ToDecimal(row["Qty Required"]) + qtyRequired;
                    txtMaterialQty.Clear();
                    cmbRawMaterial.SelectedIndex = -1;
                    return;
                }
            }

            dtBOMCart.Rows.Add(selectedMaterial.ID, selectedMaterial.Name, qtyRequired);
            txtMaterialQty.Clear();
            cmbRawMaterial.SelectedIndex = -1;
        }

        private void btnRemoveMaterial_Click(object sender, EventArgs e)
        {
            if (dgvBOMCart.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow r in dgvBOMCart.SelectedRows)
                {
                    dgvBOMCart.Rows.Remove(r);
                }
            }
            else
            {
                MessageBox.Show("Please pick an active ingredient row to extract.", "Operation Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnSaveProduct_Click(object sender, EventArgs e)
        {
            string targetProductID = rbNewProduct.Checked ? txtProductID.Text.Trim() : ((ProductComboItem)cmbExistingProducts.SelectedItem)?.ID;
            string prodName = txtProductName.Text.Trim();
            string priceStr = txtRetailPrice.Text.Trim();

            if (string.IsNullOrWhiteSpace(targetProductID) || targetProductID == "PERR")
            {
                MessageBox.Show("Invalid Product ID.", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(prodName) || string.IsNullOrWhiteSpace(priceStr))
            {
                MessageBox.Show("Product Name and Retail Price are mandatory fields!", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🌟 優化：加入 InvariantCulture 確保小數點解析安全
            if (!decimal.TryParse(priceStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal retailPrice) || retailPrice < 0)
            {
                MessageBox.Show("Retail Price must be a valid non-negative number.", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            if (rbNewProduct.Checked)
                            {
                                string checkSql = "SELECT COUNT(*) FROM product WHERE ProductID = @id";
                                using (MySqlCommand checkCmd = new MySqlCommand(checkSql, conn, trans))
                                {
                                    checkCmd.Parameters.AddWithValue("@id", targetProductID);
                                    if ((long)checkCmd.ExecuteScalar() > 0)
                                        throw new Exception($"Product Code ID [{targetProductID}] already registered!");
                                }

                                string insertProdSql = "INSERT INTO product (ProductID, ProductName, StockLevel, RetailPrice) VALUES (@id, @name, 0, @price)";
                                using (MySqlCommand cmdProd = new MySqlCommand(insertProdSql, conn, trans))
                                {
                                    cmdProd.Parameters.AddWithValue("@id", targetProductID);
                                    cmdProd.Parameters.AddWithValue("@name", prodName);
                                    cmdProd.Parameters.AddWithValue("@price", retailPrice);
                                    cmdProd.ExecuteNonQuery();
                                }
                            }
                            else if (rbUpdateProduct.Checked)
                            {
                                string updateProdSql = "UPDATE product SET ProductName = @name, RetailPrice = @price WHERE ProductID = @id";
                                using (MySqlCommand cmdProd = new MySqlCommand(updateProdSql, conn, trans))
                                {
                                    cmdProd.Parameters.AddWithValue("@id", targetProductID);
                                    cmdProd.Parameters.AddWithValue("@name", prodName);
                                    cmdProd.Parameters.AddWithValue("@price", retailPrice);
                                    cmdProd.ExecuteNonQuery();
                                }

                                string deleteBOMSql = "DELETE FROM bill_of_materials WHERE ProductID = @id";
                                using (MySqlCommand cmdDelete = new MySqlCommand(deleteBOMSql, conn, trans))
                                {
                                    cmdDelete.Parameters.AddWithValue("@id", targetProductID);
                                    cmdDelete.ExecuteNonQuery();
                                }
                            }

                            // 🌟 核心優化：將 Command 宣告移出 Loop 外，重複利用 Parameter
                            string insertBOMSql = "INSERT INTO bill_of_materials (ProductID, MaterialID, QuantityRequired) VALUES (@pID, @mID, @qty)";
                            using (MySqlCommand cmdBOM = new MySqlCommand(insertBOMSql, conn, trans))
                            {
                                // 先建立好參數結構（指定資料型態）
                                cmdBOM.Parameters.Add("@pID", MySqlDbType.VarChar);
                                cmdBOM.Parameters.Add("@mID", MySqlDbType.VarChar);
                                cmdBOM.Parameters.Add("@qty", MySqlDbType.Decimal);

                                foreach (DataRow row in dtBOMCart.Rows)
                                {
                                    // 在 Loop 內單純更換數值，大幅減少記憶體開銷與提升執行速度
                                    cmdBOM.Parameters["@pID"].Value = targetProductID;
                                    cmdBOM.Parameters["@mID"].Value = row["Material ID"].ToString();
                                    cmdBOM.Parameters["@qty"].Value = Convert.ToDecimal(row["Qty Required"]);

                                    cmdBOM.ExecuteNonQuery();
                                }
                            }

                            trans.Commit();

                            string successMsg = dtBOMCart.Rows.Count == 0
                                ? $"Success! Product [{targetProductID} - {prodName}] saved without a BOM recipe. You can update it later."
                                : $"Success! Product [{targetProductID} - {prodName}] saved with its complete engineering BOM recipe.";

                            MessageBox.Show(successMsg, "Transaction Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            if (OnNavigationBack != null) OnNavigationBack.Invoke();
                            else { this.DialogResult = DialogResult.OK; this.Close(); }
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
                    MessageBox.Show("Critical Transaction Blockage: \n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion
    }
}