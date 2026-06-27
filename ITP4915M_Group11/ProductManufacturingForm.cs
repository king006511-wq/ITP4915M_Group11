using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public class ManufactureProductItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }

    public partial class ProductManufacturingForm : Form
    {
        // ==========================================
        // 🔒 Database Configuration & Settings
        // ==========================================
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // 🚨 庫存紅字警告線 (你可以按需要修改呢個數值)
        private readonly int STOCK_WARNING_THRESHOLD = 50;

        // ==========================================
        // 🎨 UI 變數 
        // ==========================================
        private ComboBox custom_cmbProduct;
        private TextBox custom_txtQty;
        private DataGridView custom_dgvBOMRequirements;
        private DataGridView custom_dgvRawMaterialStock;
        private Button custom_btnCalculate, custom_btnManufacture, custom_btnClear;

        public ProductManufacturingForm()
        {
            // InitializeComponent();  // 👈 刪除或註解呢一行！

            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                SetupCustomSleekUI();
                LoadManufacturableProducts();
                LoadRawMaterialInventory();

                this.SizeChanged += ProductManufacturingForm_SizeChanged;
                this.Layout += (s, e) => RecalculateDynamicLayout();
            }
        }

        #region 🎨 精緻手動算繪排版
        private void SetupCustomSleekUI()
        {
            this.Controls.Clear();
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopLevel = false;
            this.Dock = DockStyle.Fill;

            // --- 左側面板 (操作區) ---
            Panel pnlLeftCard = new Panel { Name = "pnlLeftCard", BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, AutoScroll = true };
            pnlLeftCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlLeftCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            this.Controls.Add(pnlLeftCard);

            Label lblCardTitle = new Label { Text = "⚙️ Product Processing & Assembly", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(79, 70, 229), Location = new Point(22, 18), AutoSize = true };
            pnlLeftCard.Controls.Add(lblCardTitle);

            int startY = 65;
            int inputWidth = 350;

            // Product Selection
            Label lblProd = new Label { Text = "Select Target Product *:", Location = new Point(22, startY), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            custom_cmbProduct = new ComboBox { Location = new Point(22, startY + 24), Width = inputWidth, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11F), BackColor = Color.White };
            custom_cmbProduct.SelectedIndexChanged += custom_cmbProduct_SelectedIndexChanged;
            pnlLeftCard.Controls.Add(lblProd);
            pnlLeftCard.Controls.Add(custom_cmbProduct);
            startY += 75;

            // Quantity Input
            Label lblQty = new Label { Text = "Production Quantity *:", Location = new Point(22, startY), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            custom_txtQty = new TextBox { Location = new Point(22, startY + 24), Width = inputWidth, Font = new Font("Segoe UI", 11F), BorderStyle = BorderStyle.FixedSingle };
            pnlLeftCard.Controls.Add(lblQty);
            pnlLeftCard.Controls.Add(custom_txtQty);
            startY += 65;

            // Buttons
            custom_btnCalculate = new Button { Text = "🧮 Check BOM & Stock", Location = new Point(22, startY), Size = new Size(350, 40), BackColor = Color.FromArgb(245, 158, 11), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand };
            custom_btnCalculate.FlatAppearance.BorderSize = 0;
            custom_btnCalculate.Click += btnCalculate_Click;
            pnlLeftCard.Controls.Add(custom_btnCalculate);
            startY += 55;

            // BOM Requirements Grid
            Label lblBOM = new Label { Text = "📋 Required Raw Materials (BOM):", Location = new Point(22, startY), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            pnlLeftCard.Controls.Add(lblBOM);
            startY += 25;

            custom_dgvBOMRequirements = new DataGridView { Location = new Point(22, startY), Size = new Size(350, 200), BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, EnableHeadersVisualStyles = false };
            custom_dgvBOMRequirements.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(71, 85, 105);
            custom_dgvBOMRequirements.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            pnlLeftCard.Controls.Add(custom_dgvBOMRequirements);
            startY += 215;

            custom_btnManufacture = new Button { Text = "🔨 Start Processing (Deduct Stock)", Location = new Point(22, startY), Size = new Size(350, 42), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand, Enabled = false };
            custom_btnManufacture.FlatAppearance.BorderSize = 0;
            custom_btnManufacture.Click += btnManufacture_Click;
            pnlLeftCard.Controls.Add(custom_btnManufacture);
            startY += 50;

            custom_btnClear = new Button { Text = "🔄 Reset", Location = new Point(22, startY), Size = new Size(350, 40), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            custom_btnClear.FlatAppearance.BorderSize = 0;
            custom_btnClear.Click += (s, e) => ClearForm();
            pnlLeftCard.Controls.Add(custom_btnClear);

            // --- 右側面板 (庫存監控區) ---
            Label lblGridTitle = new Label { Name = "lblGridTitle", Text = "📦 Raw Material Inventory Monitor", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true };
            this.Controls.Add(lblGridTitle);

            custom_dgvRawMaterialStock = new DataGridView
            {
                Name = "dgvStock",
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                ScrollBars = ScrollBars.Both,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };
            custom_dgvRawMaterialStock.DefaultCellStyle.Padding = new Padding(8);
            custom_dgvRawMaterialStock.DefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            custom_dgvRawMaterialStock.RowTemplate.Height = 36;
            custom_dgvRawMaterialStock.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            custom_dgvRawMaterialStock.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            custom_dgvRawMaterialStock.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            custom_dgvRawMaterialStock.ColumnHeadersHeight = 42;

            // 🌟 綁定變色警告事件
            custom_dgvRawMaterialStock.CellFormatting += custom_dgvRawMaterialStock_CellFormatting;

            this.Controls.Add(custom_dgvRawMaterialStock);
        }

        private void ProductManufacturingForm_SizeChanged(object sender, EventArgs e) { RecalculateDynamicLayout(); }

        private void RecalculateDynamicLayout()
        {
            if (this.Width < 200 || this.Height < 200) return;
            this.SuspendLayout();

            Panel pnlLeft = (Panel)this.Controls["pnlLeftCard"];
            pnlLeft.Location = new Point(20, 20);
            pnlLeft.Size = new Size(400, this.Height - 40);

            int rightStartX = pnlLeft.Right + 20;
            int rightWidth = this.Width - rightStartX - 20;

            if (rightWidth > 100)
            {
                this.Controls["lblGridTitle"].Location = new Point(rightStartX, 20);
                DataGridView dgvStock = (DataGridView)this.Controls["dgvStock"];
                dgvStock.Location = new Point(rightStartX, 55);
                dgvStock.Size = new Size(rightWidth, this.Height - 75);
            }
            this.ResumeLayout(false);
        }

        // 🌟 庫存低於閾值自動變紅機制
        private void custom_dgvRawMaterialStock_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (custom_dgvRawMaterialStock.Columns[e.ColumnIndex].Name == "Current Stock" && e.Value != null)
            {
                if (int.TryParse(e.Value.ToString(), out int stock))
                {
                    if (stock < STOCK_WARNING_THRESHOLD)
                    {
                        // 整行變成淡紅色，文字變深紅加粗
                        custom_dgvRawMaterialStock.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(254, 226, 226); // Light Red
                        custom_dgvRawMaterialStock.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(185, 28, 28);   // Dark Red
                        custom_dgvRawMaterialStock.Rows[e.RowIndex].DefaultCellStyle.Font = new Font(custom_dgvRawMaterialStock.Font, FontStyle.Bold);
                    }
                }
            }
        }
        #endregion

        #region 💾 資料庫載入邏輯
        private void LoadManufacturableProducts()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // 只載入喺 bill_of_materials 有配方設定嘅 Product
                    string query = @"
                        SELECT DISTINCT p.ProductID, p.ProductName 
                        FROM product p
                        JOIN bill_of_materials bom ON p.ProductID = bom.ProductID";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<ManufactureProductItem> list = new List<ManufactureProductItem>();
                        while (reader.Read())
                        {
                            list.Add(new ManufactureProductItem
                            {
                                ID = reader["ProductID"].ToString(),
                                Name = reader["ProductName"].ToString()
                            });
                        }
                        custom_cmbProduct.DataSource = list;
                        custom_cmbProduct.DisplayMember = "Name";
                        custom_cmbProduct.ValueMember = "ID";
                        custom_cmbProduct.SelectedIndex = -1;
                    }
                }
                catch (Exception ex) { MessageBox.Show("Failed to load products: " + ex.Message); }
            }
        }

        private void LoadRawMaterialInventory()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT MaterialID AS 'Material ID', MaterialName AS 'Material Name', StockLevel AS 'Current Stock' 
                                     FROM raw_material ORDER BY MaterialID ASC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        custom_dgvRawMaterialStock.DataSource = dt;
                    }
                }
                catch (Exception) { /* Fail silently */ }
            }
        }

        private void custom_cmbProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            custom_dgvBOMRequirements.DataSource = null;
            custom_btnManufacture.Enabled = false;
            custom_btnManufacture.BackColor = Color.LightGray;
        }
        #endregion

        #region 🧮 運算與轉換邏輯
        // 計算所需物料並檢查庫存
        private void btnCalculate_Click(object sender, EventArgs e)
        {
            if (custom_cmbProduct.SelectedItem == null)
            {
                MessageBox.Show("Please select a Product to manufacture.", "Missing Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(custom_txtQty.Text.Trim(), out int qty) || qty <= 0)
            {
                MessageBox.Show("Please enter a valid positive quantity.", "Invalid Qty", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string productID = ((ManufactureProductItem)custom_cmbProduct.SelectedItem).ID;

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // 聯合 BOM 同 Raw_Material，一次過計出所需數量同目前庫存
                    string query = @"
                        SELECT 
                            b.MaterialID AS 'Mat ID',
                            rm.MaterialName AS 'Material Name',
                            (b.QuantityRequired * @qty) AS 'Total Required',
                            rm.StockLevel AS 'Stock Available',
                            CASE WHEN rm.StockLevel >= (b.QuantityRequired * @qty) THEN 'OK' ELSE 'SHORTAGE' END AS 'Status'
                        FROM bill_of_materials b
                        JOIN raw_material rm ON b.MaterialID = rm.MaterialID
                        WHERE b.ProductID = @prodID";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@qty", qty);
                        cmd.Parameters.AddWithValue("@prodID", productID);

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            custom_dgvBOMRequirements.DataSource = dt;

                            // 檢查有無任何零件 Shortage (缺貨)
                            bool canManufacture = true;
                            foreach (DataRow row in dt.Rows)
                            {
                                if (row["Status"].ToString() == "SHORTAGE")
                                {
                                    canManufacture = false;
                                    break;
                                }
                            }

                            if (canManufacture)
                            {
                                custom_btnManufacture.Enabled = true;
                                custom_btnManufacture.BackColor = Color.FromArgb(16, 185, 129); // Green
                                MessageBox.Show("Stock is sufficient. You may proceed with manufacturing.", "Check Passed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                custom_btnManufacture.Enabled = false;
                                custom_btnManufacture.BackColor = Color.LightGray;
                                MessageBox.Show("INSUFFICIENT STOCK!\nOne or more raw materials are below the required amount. Please issue a purchase request first.", "Shortage Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error calculating BOM: " + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 正式生產 (扣原材料 + 加製成品)
        private void btnManufacture_Click(object sender, EventArgs e)
        {
            string productID = ((ManufactureProductItem)custom_cmbProduct.SelectedItem).ID;
            int qty = Convert.ToInt32(custom_txtQty.Text.Trim());

            DialogResult dialogResult = MessageBox.Show($"Confirm manufacturing {qty} units of [{productID}]?\n\nThis will deduct the required raw materials and increase the final product stock.", "Confirm Processing", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlTransaction trans = conn.BeginTransaction())
                        {
                            try
                            {
                                // 1. 根據 BOM 扣減 Raw Material 庫存
                                string deductSql = @"
                                    UPDATE raw_material rm
                                    JOIN bill_of_materials b ON rm.MaterialID = b.MaterialID
                                    SET rm.StockLevel = rm.StockLevel - (b.QuantityRequired * @qty)
                                    WHERE b.ProductID = @prodID";

                                using (MySqlCommand cmd = new MySqlCommand(deductSql, conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@qty", qty);
                                    cmd.Parameters.AddWithValue("@prodID", productID);
                                    cmd.ExecuteNonQuery();
                                }

                                // 2. 增加 Product 庫存
                                string addSql = "UPDATE product SET StockLevel = StockLevel + @qty WHERE ProductID = @prodID";
                                using (MySqlCommand cmd = new MySqlCommand(addSql, conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@qty", qty);
                                    cmd.Parameters.AddWithValue("@prodID", productID);
                                    cmd.ExecuteNonQuery();
                                }

                                trans.Commit();
                                MessageBox.Show("Manufacturing process completed successfully!\nInventory has been updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                ClearForm();
                                LoadRawMaterialInventory(); // 刷新庫存表，過低會即時變紅
                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                throw new Exception("Transaction aborted. Reason: " + ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to process manufacturing:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ClearForm()
        {
            custom_cmbProduct.SelectedIndex = -1;
            custom_txtQty.Clear();
            custom_dgvBOMRequirements.DataSource = null;
            custom_btnManufacture.Enabled = false;
            custom_btnManufacture.BackColor = Color.LightGray;
        }
        #endregion
    }
}