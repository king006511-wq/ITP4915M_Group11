using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ITP4915M_Group11
{
    public partial class ProductManagement : Form
    {
        // 統一資料庫連線字串
        private string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        public ProductManagement()
        {
            InitializeComponent();
        }

        // =========================================================
        // 🔄 核心數據載入邏輯
        // =========================================================
        private void LoadProductData()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT PartID AS '產品編號', Name AS '產品名稱', Description AS '產品描述', StockLevel AS '庫存量', DefaultPrice AS '標準售價' FROM product_part";
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
                    MessageBox.Show("無法載入產品資料，請確保 XAMPP 已開啟：\n" + ex.Message, "系統錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // =========================================================
        // 👻 終極收網防線：包攬所有可能被 Designer 殘留記錄嘅事件名稱
        // =========================================================

        // 1. 視窗載入事件 (攔截所有 Load 變體)
        private void Form4_Load(object sender, EventArgs e) { LoadProductData(); }
        private void Form4_Load_1(object sender, EventArgs e) { LoadProductData(); }

        // 2. 新增產品按鈕 (攔截所有 Add 變體)
        private void button1_Click(object sender, EventArgs e) { ExecuteAddProduct(); }
        private void btnAddProduct_Click(object sender, EventArgs e) { ExecuteAddProduct(); }

        // 3. 修改產品按鈕 (攔截所有 Update 變體)
        private void button2_Click(object sender, EventArgs e) { ExecuteUpdateProduct(); }
        private void btnUpdateProduct_Click(object sender, EventArgs e) { ExecuteUpdateProduct(); }

        // 4. 刪除產品按鈕 (攔截所有 Delete 變體)
        private void button3_Click(object sender, EventArgs e) { ExecuteDeleteProduct(); }
        private void btnDeleteProduct_Click(object sender, EventArgs e) { ExecuteDeleteProduct(); }

        // 5. 大表格點擊事件 (攔截所有 Grid 變體，防止報錯找不到方法)
        private void dgvProducts_CellContentClick(object sender, DataGridViewCellEventArgs e) { HandleGridClick(e.RowIndex); }
        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e) { HandleGridClick(e.RowIndex); }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { HandleGridClick(e.RowIndex); }


        // =========================================================
        // 🛠️ 真正執行核心 CRUD 運作的功能實體
        // =========================================================

        // 【新增執行】
        private void ExecuteAddProduct()
        {
            if (string.IsNullOrWhiteSpace(txtPartID.Text) || string.IsNullOrWhiteSpace(txtPartName.Text))
            {
                MessageBox.Show("產品編號同名稱唔可以空白！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                            MessageBox.Show("🎉 產品新增成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            LoadProductData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("新增產品失敗，產品編號可能重複：\n" + ex.Message, "SQL 錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 【修改執行】
        private void ExecuteUpdateProduct()
        {
            if (string.IsNullOrWhiteSpace(txtPartID.Text) || string.IsNullOrWhiteSpace(txtPartName.Text))
            {
                MessageBox.Show("唔該先喺上方表格點選你想修改嘅產品！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                            MessageBox.Show("✏️ 產品資料更新成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            LoadProductData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("更新產品失敗：\n" + ex.Message, "SQL 錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 【刪除執行】
        private void ExecuteDeleteProduct()
        {
            if (string.IsNullOrWhiteSpace(txtPartID.Text))
            {
                MessageBox.Show("唔該先喺上方表格選擇你想刪除嘅產品！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show($"你確定要刪除產品編號 [{txtPartID.Text}] 嗎？", "確認刪除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

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
                                MessageBox.Show("🗑️ 產品已成功移除！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearFields();
                                LoadProductData();
                            }
                            else
                            {
                                MessageBox.Show("❌ 刪除失敗！資料庫找不到該產品編號。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("資料庫執行刪除時出錯！" + ex.Message, "SQL 錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // 【大表格點擊回填數據邏輯】
        private void HandleGridClick(int rowIndex)
        {
            if (rowIndex >= 0 && dgvProducts != null)
            {
                try
                {
                    DataGridViewRow row = dgvProducts.Rows[rowIndex];
                    txtPartID.Text = row.Cells["產品編號"].Value?.ToString();
                    txtPartName.Text = row.Cells["產品名稱"].Value?.ToString();
                    txtDescription.Text = row.Cells["產品描述"].Value?.ToString();
                    txtStockLevel.Text = row.Cells["庫存量"].Value?.ToString();
                    txtDefaultPrice.Text = row.Cells["標準售價"].Value?.ToString();

                    txtPartID.ReadOnly = true; // 修改模式下鎖定 Primary Key
                }
                catch { }
            }
        }

        // 清空所有輸入框
        private void ClearFields()
        {
            txtPartID.Clear();
            txtPartName.Clear();
            txtDescription.Clear();
            txtStockLevel.Clear();
            txtDefaultPrice.Clear();
            txtPartID.ReadOnly = false;
        }

        // 雙重保險線：如果 Designer 沒有成功綁定表格點擊，用 Code 強制綁定
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