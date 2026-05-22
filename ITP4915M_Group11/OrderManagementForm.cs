using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class OrderManagementForm : Form
    {
        private readonly string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";
        private decimal currentUnitPrice = 0;

        public OrderManagementForm()
        {
            InitializeComponent();
        }

        private void OrderManagementForm_Load(object sender, EventArgs e)
        {
            GenerateOrderID();
            LoadProductsToCombo();
            RefreshOrdersGrid();
        }

        // 自動生成流水號預填 (格式: ORD-YYYYMMDD-時分秒)
        private void GenerateOrderID()
        {
            txtOrderID.Text = "ORD-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }

        // 載入產品至下拉選單
        private void LoadProductsToCombo()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT PartID, Name, DefaultPrice FROM product_part";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            cboProducts.Items.Clear();
                            while (reader.Read())
                            {
                                // 將產品打包成一個匿名物件放進 ComboBox
                                cboProducts.Items.Add(new
                                {
                                    ID = reader["PartID"].ToString(),
                                    Name = reader["Name"].ToString(),
                                    Price = Convert.ToDecimal(reader["DefaultPrice"])
                                });
                            }
                        }
                    }
                    cboProducts.DisplayMember = "Name"; // 畫面上顯示名稱
                }
                catch (Exception ex)
                {
                    MessageBox.Show("載入產品清單失敗: " + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 當使用者切換產品時，自動帶出單價與計算總價
        private void cboProducts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboProducts.SelectedItem != null)
            {
                dynamic selectedProduct = cboProducts.SelectedItem;
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

        // =========================================================
        // 💾 核心功能：建立新訂單 (包含扣庫存的事務處理)
        // =========================================================
        private void btnCreateOrder_Click(object sender, EventArgs e)
        {
            // 1. 防呆基礎驗證
            if (string.IsNullOrWhiteSpace(txtCustomerID.Text) || string.IsNullOrWhiteSpace(txtStaffID.Text))
            {
                MessageBox.Show("請填寫客戶編號與員工編號！", "驗證失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cboProducts.SelectedItem == null)
            {
                MessageBox.Show("請選擇欲訂購的產品！", "驗證失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txtQty.Text.Trim(), out int qty) || qty <= 0)
            {
                MessageBox.Show("請輸入正確的購買數量(必須大於 0)！", "驗證失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dynamic product = cboProducts.SelectedItem;
            string partID = product.ID;
            string orderID = txtOrderID.Text.Trim();
            string customerID = txtCustomerID.Text.Trim();
            string staffID = txtStaffID.Text.Trim();
            decimal subtotal = qty * currentUnitPrice;

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    // 2. 庫存足夠性檢查 (防禦性編程)
                    string checkStockSql = "SELECT StockLevel FROM product_part WHERE PartID = @PartID";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkStockSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@PartID", partID);
                        int currentStock = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (currentStock < qty)
                        {
                            MessageBox.Show($"庫存不足！當前庫存僅剩: {currentStock} 件，無法落單。", "庫存警報", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // 3. 開始執行 Transaction 交易機制
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // 步驟 A: 寫入 orders 主表
                            string insertOrderSql = "INSERT INTO orders (OrderID, CustomerID, StaffID, TotalAmount, Status) VALUES (@OrderID, @CustomerID, @StaffID, @Total, 'Pending')";
                            using (MySqlCommand cmdOrder = new MySqlCommand(insertOrderSql, conn, trans))
                            {
                                cmdOrder.Parameters.AddWithValue("@OrderID", orderID);
                                cmdOrder.Parameters.AddWithValue("@CustomerID", customerID);
                                cmdOrder.Parameters.AddWithValue("@StaffID", staffID);
                                cmdOrder.Parameters.AddWithValue("@Total", subtotal);
                                cmdOrder.ExecuteNonQuery();
                            }

                            // 步驟 B: 寫入 order_lineitem 明細表
                            string insertLineSql = "INSERT INTO order_lineitem (OrderID, PartID, Quantity, Subtotal) VALUES (@OrderID, @PartID, @Qty, @Subtotal)";
                            using (MySqlCommand cmdLine = new MySqlCommand(insertLineSql, conn, trans))
                            {
                                cmdLine.Parameters.AddWithValue("@OrderID", orderID);
                                cmdLine.Parameters.AddWithValue("@PartID", partID);
                                cmdLine.Parameters.AddWithValue("@Qty", qty);
                                cmdLine.Parameters.AddWithValue("@Subtotal", subtotal);
                                cmdLine.ExecuteNonQuery();
                            }

                            // 步驟 C: 更新 product_part 扣減庫存 (銷貨扣庫)
                            string updateStockSql = "UPDATE product_part SET StockLevel = StockLevel - @Qty WHERE PartID = @PartID";
                            using (MySqlCommand cmdStock = new MySqlCommand(updateStockSql, conn, trans))
                            {
                                cmdStock.Parameters.AddWithValue("@Qty", qty);
                                cmdStock.Parameters.AddWithValue("@PartID", partID);
                                cmdStock.ExecuteNonQuery();
                            }

                            // 雙向確認成功，正式提交資料庫
                            trans.Commit();
                            MessageBox.Show($"銷售訂單 [{orderID}] 建立成功！\n庫存已即時扣除。", "落單成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // 重新整理 UI
                            ClearFields();
                            GenerateOrderID();
                            RefreshOrdersGrid();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback(); // 資料中途出錯，全部回復原狀（防髒資料）
                            throw new Exception("交易執行失敗，已全數復原。原委：" + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "系統錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    string query = @"SELECT o.OrderID AS '訂單編號', o.CustomerID AS '客戶編號', 
                                            o.OrderDate AS '下單日期', o.TotalAmount AS '總金額', o.Status AS '狀態' 
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
                    System.Diagnostics.Debug.WriteLine("載入訂單列表失敗: " + ex.Message);
                }
            }
        }

        private void ClearFields()
        {
            txtCustomerID.Clear();
            txtQty.Clear();
            txtSubtotal.Clear();
            cboProducts.SelectedIndex = -1;
        }
    }
}