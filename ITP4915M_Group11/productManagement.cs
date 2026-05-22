using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ITP4915M_Group11
{
    public partial class ProductManagement : Form
    {
        // Centralized database connection string
        private string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        public ProductManagement()
        {
            InitializeComponent();
        }

        // =========================================================
        // Core data loading logic
        // =========================================================
        private void LoadProductData()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT PartID AS 'PartID', Name AS 'Name', Description AS 'Description', StockLevel AS 'StockLevel', DefaultPrice AS 'DefaultPrice' FROM product_part";
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
                    MessageBox.Show("Unable to load product data, please ensure XAMPP is running:\n" + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // =========================================================
        // Catch-all for possible Designer-generated event handler names
        // =========================================================

// 1. Form load events (catch all Load variants)
        private void Form4_Load(object sender, EventArgs e) { LoadProductData(); }
        private void Form4_Load_1(object sender, EventArgs e) { LoadProductData(); }

// 2. Add product buttons (catch all Add variants)
        private void button1_Click(object sender, EventArgs e) { ExecuteAddProduct(); }
        private void btnAddProduct_Click(object sender, EventArgs e) { ExecuteAddProduct(); }

// 3. Update product buttons (catch all Update variants)
        private void button2_Click(object sender, EventArgs e) { ExecuteUpdateProduct(); }
        private void btnUpdateProduct_Click(object sender, EventArgs e) { ExecuteUpdateProduct(); }

// 4. Delete product buttons (catch all Delete variants)
        private void button3_Click(object sender, EventArgs e) { ExecuteDeleteProduct(); }
        private void btnDeleteProduct_Click(object sender, EventArgs e) { ExecuteDeleteProduct(); }

// 5. Main grid click events (catch all Grid variants to avoid missing handler errors)
        private void dgvProducts_CellContentClick(object sender, DataGridViewCellEventArgs e) { HandleGridClick(e.RowIndex); }
        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e) { HandleGridClick(e.RowIndex); }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { HandleGridClick(e.RowIndex); }


        // =========================================================
        // Core CRUD execution methods
        // =========================================================

        // Add product execution
        private void ExecuteAddProduct()
        {
            if (string.IsNullOrWhiteSpace(txtPartID.Text) || string.IsNullOrWhiteSpace(txtPartName.Text))
            {
                MessageBox.Show("Part ID and Name cannot be empty!", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show("Failed to add product, PartID might be duplicated:\n" + ex.Message, "SQL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Update product execution
        private void ExecuteUpdateProduct()
        {
            if (string.IsNullOrWhiteSpace(txtPartID.Text) || string.IsNullOrWhiteSpace(txtPartName.Text))
            {
                MessageBox.Show("Please select a product from the table above to edit!", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        cmd.Parameters.AddWithValue("@stock", Convert.ToInt32(txtStockLevel.Text));
                        cmd.Parameters.AddWithValue("@price", Convert.ToDecimal(txtDefaultPrice.Text));

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

        // Delete product execution
        private void ExecuteDeleteProduct()
        {
            if (string.IsNullOrWhiteSpace(txtPartID.Text))
            {
                MessageBox.Show("Please select the product you want to delete from the table above!", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        MessageBox.Show("Error executing delete in database: " + ex.Message, "SQL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Logic for populating input fields when clicking a row in the main DataGridView
        private void HandleGridClick(int rowIndex)
        {
            if (rowIndex >= 0 && dgvProducts != null)
            {
                try
                {
                    DataGridViewRow row = dgvProducts.Rows[rowIndex];
                    txtPartID.Text = row.Cells["PartID"].Value?.ToString();
                    txtPartName.Text = row.Cells["Name"].Value?.ToString();
                    txtDescription.Text = row.Cells["Description"].Value?.ToString();
                    txtStockLevel.Text = row.Cells["StockLevel"].Value?.ToString();
                    txtDefaultPrice.Text = row.Cells["DefaultPrice"].Value?.ToString();

                    txtPartID.ReadOnly = true; // Lock primary key in edit mode
                }
                catch { }
            }
        }

        // Clear all input fields
        private void ClearFields()
        {
            txtPartID.Clear();
            txtPartName.Clear();
            txtDescription.Clear();
            txtStockLevel.Clear();
            txtDefaultPrice.Clear();
            txtPartID.ReadOnly = false;
        }

        // Safety net: bind grid click handler in code if Designer didn't
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (dgvProducts != null)
            {
                dgvProducts.CellClick += (s, ev) => { HandleGridClick(ev.RowIndex); };
            }
        }
    }
}