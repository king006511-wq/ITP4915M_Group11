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

    public partial class ProductCreationBOMForm : Form
    {
        // 🔗 新增 Callback 委派，用來通知母視窗切換畫面，防止白面
        public Action OnNavigationBack { get; set; }

        // 🔒 中央資料庫連線字串
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // 💾 暫存目前新產品的 BOM 配方清單 (購物車機制)
        private DataTable dtBOMCart;

        // 🎨 UI 控制項 (移除了 PictureBox, btnBrowseImage, btnCancel)
        private TextBox txtProductID, txtProductName, txtRetailPrice, txtMaterialQty;
        private ComboBox cmbRawMaterial;
        private DataGridView dgvBOMCart;
        private Button btnAddMaterial, btnRemoveMaterial, btnSaveProduct;

        public ProductCreationBOMForm()
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                InitializeComponentDataCart();
                SetupPremiumCreationUI();
                LoadRawMaterialsToCombo();
                GenerateNextProductID(); // ⚡ 載入時自動計算下一個 Product ID
            }
        }

        // 初始化 BOM 暫存配方表結構
        private void InitializeComponentDataCart()
        {
            dtBOMCart = new DataTable();
            dtBOMCart.Columns.Add("Material ID", typeof(string));
            dtBOMCart.Columns.Add("Material Name", typeof(string));
            dtBOMCart.Columns.Add("Qty Required", typeof(decimal));
        }

        #region 🎨 Premium Unified Modern UI Construction
        private void SetupPremiumCreationUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - New Product R&D & BOM Design";
            this.Size = new Size(1150, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // 主工作區面板
            Panel pnlMain = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(25) };
            this.Controls.Add(pnlMain);

            // 標題
            Label lblHeader = new Label { Text = "✨ New Product Development & BOM Setup", Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(25, 20), AutoSize = true };
            pnlMain.Controls.Add(lblHeader);

            // --- 1. 左側面板：產品基本資訊 (Card 1) ---
            Panel pnlLeftCard = new Panel { Location = new Point(25, 75), Size = new Size(420, 580), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlLeftCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlLeftCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlLeftCard);

            Label lblSec1 = new Label { Text = "📦 Product Profile & Identity", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(20, 15), AutoSize = true };
            pnlLeftCard.Controls.Add(lblSec1);

            int startY = 55;
            // ⚡ 將 Product ID 改為唯讀，防止人手輸入
            txtProductID = CreateInputField(pnlLeftCard, ref startY, "New Product ID (Auto-Generated):");
            txtProductID.ReadOnly = true;
            txtProductID.BackColor = Color.FromArgb(241, 245, 249); // 灰色背景提示唯讀

            txtProductName = CreateInputField(pnlLeftCard, ref startY, "Product Name *:");
            txtRetailPrice = CreateInputField(pnlLeftCard, ref startY, "Retail Price (HKD) *:");

            // --- 2. 右側面板：BOM 配方設定與購物車 (Card 2) ---
            Panel pnlRightCard = new Panel { Location = new Point(470, 75), Size = new Size(640, 580), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlRightCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlRightCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlRightCard);

            Label lblSec2 = new Label { Text = "🛠️ Bill of Materials (BOM) Formulation", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(79, 70, 229), Location = new Point(20, 15), AutoSize = true };
            pnlRightCard.Controls.Add(lblSec2);

            // 原材料選擇 Combo
            Label lblMat = new Label { Text = "Select Raw Material Component:", Location = new Point(20, 55), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cmbRawMaterial = new ComboBox { Location = new Point(20, 77), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F) };
            pnlRightCard.Controls.Add(lblMat); pnlRightCard.Controls.Add(cmbRawMaterial);

            // 原材料數量 TextBox
            Label lblMatQty = new Label { Text = "Quantity Required per Unit:", Location = new Point(320, 55), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtMaterialQty = new TextBox { Location = new Point(320, 77), Width = 120, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            pnlRightCard.Controls.Add(lblMatQty); pnlRightCard.Controls.Add(txtMaterialQty);

            // 新增至配方清單按鈕
            btnAddMaterial = new Button { Text = "➕ Add to Formula", Location = new Point(460, 74), Size = new Size(160, 32), BackColor = Color.FromArgb(79, 70, 229), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAddMaterial.FlatAppearance.BorderSize = 0;
            btnAddMaterial.Click += btnAddMaterial_Click;
            pnlRightCard.Controls.Add(btnAddMaterial);

            // 配方清單 DataGridView
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

            // 移除選定配方按鈕
            btnRemoveMaterial = new Button { Text = "🗑️ Remove Component", Location = new Point(20, 500), Size = new Size(180, 32), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnRemoveMaterial.FlatAppearance.BorderSize = 0;
            btnRemoveMaterial.Click += btnRemoveMaterial_Click;
            pnlRightCard.Controls.Add(btnRemoveMaterial);

            // --- 3. 底部全域控制按鈕 ---
            btnSaveProduct = new Button { Text = "💾 Deploy Product & Lock BOM", Location = new Point(880, 665), Size = new Size(230, 42), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand };
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

        #region 💾 資料庫讀取 & ID 自動生成邏輯

        // ⚡ 核心演算法：自動向 DB 查詢目前最大 ID 並加 1 (支援 P001 格式)
        private void GenerateNextProductID()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // 撈取排序後最後一個 ProductID
                    string query = "SELECT ProductID FROM product ORDER BY ProductID DESC LIMIT 1";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            string lastID = result.ToString(); // 例如 "P005"
                            if (lastID.StartsWith("P") && lastID.Length > 1)
                            {
                                string numPart = lastID.Substring(1); // 拆出 "005"
                                if (int.TryParse(numPart, out int currentNum))
                                {
                                    int nextNum = currentNum + 1; // 加 1 變成 6
                                    // 格式化回 P 加三位數，如 P006
                                    txtProductID.Text = "P" + nextNum.ToString("D3");
                                    return;
                                }
                            }
                        }

                        // 如果資料庫是空的，預設給予第一筆 ID
                        txtProductID.Text = "P001";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error generating automatic Product ID: " + ex.Message, "System Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtProductID.Text = "PERR"; // 發生異常時的錯誤標記
                }
            }
        }

        private void LoadRawMaterialsToCombo()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MaterialID, MaterialName FROM raw_material ORDER BY MaterialID ASC";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<MaterialComboItem> items = new List<MaterialComboItem>();
                        while (reader.Read())
                        {
                            items.Add(new MaterialComboItem
                            {
                                ID = reader["MaterialID"].ToString(),
                                Name = $"[{reader["MaterialID"]}] {reader["MaterialName"]}"
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

        #region ⚡ 研發核心互動邏輯 (配方購物車)

        private void btnAddMaterial_Click(object sender, EventArgs e)
        {
            if (cmbRawMaterial.SelectedItem == null)
            {
                MessageBox.Show("Please select a valid raw material component.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!decimal.TryParse(txtMaterialQty.Text.Trim(), out decimal qtyRequired) || qtyRequired <= 0)
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
            // A. 前端數據驗證 (移除了對 prodID 留白的驗證，因為是自動生成)
            string prodID = txtProductID.Text.Trim();
            string prodName = txtProductName.Text.Trim();
            string priceStr = txtRetailPrice.Text.Trim();

            if (prodID == "PERR" || string.IsNullOrWhiteSpace(prodID))
            {
                MessageBox.Show("System failed to secure a safe Product ID sequence. Please reopening this form.", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(prodName) || string.IsNullOrWhiteSpace(priceStr))
            {
                MessageBox.Show("Product Name and Retail Price are mandatory fields!", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(priceStr, out decimal retailPrice) || retailPrice < 0)
            {
                MessageBox.Show("Retail Price must be a valid non-negative number.", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dtBOMCart.Rows.Count == 0)
            {
                MessageBox.Show("An item cannot exist without a structure! Please attach at least 1 raw material component to construct its BOM.", "Engineering Discrepancy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // B. 開啟資料庫交易 (Transaction) 確保雙方完整性
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. 再次防禦性檢查 ProductID 是否重複
                            string checkSql = "SELECT COUNT(*) FROM product WHERE ProductID = @id";
                            using (MySqlCommand checkCmd = new MySqlCommand(checkSql, conn, trans))
                            {
                                checkCmd.Parameters.AddWithValue("@id", prodID);
                                long exists = (long)checkCmd.ExecuteScalar();
                                if (exists > 0)
                                {
                                    throw new Exception($"Product Code ID [{prodID}] already registered within systemic directory database! Overlapping rejected.");
                                }
                            }

                            // 2. 寫入 product 表 (不處理圖片)
                            string insertProdSql = "INSERT INTO product (ProductID, ProductName, StockLevel, RetailPrice) VALUES (@id, @name, 0, @price)";
                            using (MySqlCommand cmdProd = new MySqlCommand(insertProdSql, conn, trans))
                            {
                                cmdProd.Parameters.AddWithValue("@id", prodID);
                                cmdProd.Parameters.AddWithValue("@name", prodName);
                                cmdProd.Parameters.AddWithValue("@price", retailPrice);
                                cmdProd.ExecuteNonQuery();
                            }

                            // 3. 循環寫入 bill_of_materials 表 (BOM 結構)
                            string insertBOMSql = "INSERT INTO bill_of_materials (ProductID, MaterialID, QuantityRequired) VALUES (@pID, @mID, @qty)";
                            foreach (DataRow row in dtBOMCart.Rows)
                            {
                                using (MySqlCommand cmdBOM = new MySqlCommand(insertBOMSql, conn, trans))
                                {
                                    cmdBOM.Parameters.AddWithValue("@pID", prodID);
                                    cmdBOM.Parameters.AddWithValue("@mID", row["Material ID"].ToString());
                                    cmdBOM.Parameters.AddWithValue("@qty", Convert.ToDecimal(row["Qty Required"]));
                                    cmdBOM.ExecuteNonQuery();
                                }
                            }

                            // 4. 提交
                            trans.Commit();
                            MessageBox.Show($"Success! Product [{prodID} - {prodName}] successfully established with its complete engineering BOM recipe.", "R&D Deployment Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // 觸發回傳事件通知主畫面換頁
                            if (OnNavigationBack != null)
                            {
                                OnNavigationBack.Invoke();
                            }
                            else
                            {
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                            }
                        }
                        catch (Exception innerEx)
                        {
                            trans.Rollback(); // 任何一步死機就全盤倒帶
                            throw innerEx;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Critical Transaction Blockage: \n" + ex.Message, "Database Operations Aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion
    }
}