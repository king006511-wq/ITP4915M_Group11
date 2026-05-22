using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class GoodsReceivedForm : Form
    {
        private readonly string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        public GoodsReceivedForm()
        {
            InitializeComponent();
        }

        private void GoodsReceivedForm_Load(object sender, EventArgs e)
        {
            GenerateGRNID();
            LoadActivePurchaseOrders();
        }

        private void GenerateGRNID()
        {
            txtGRNID.Text = "GRN-" + DateTime.Now.ToString("yyyyMM") + "-" + DateTime.Now.ToString("ddHHmmss");
        }

        // 載入未處理入庫的採購單
        private void LoadActivePurchaseOrders()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // 撈出所有目前非 'Received' 完成狀態的採購單明細
                    string query = @"SELECT po.PO_ID AS '採購單號', po.SupplierID AS '供應商', 
                                            li.PartID AS '零件編號', p.Name AS '零件名稱', 
                                            li.Quantity AS '採購數量', po.Status AS '狀態'
                                     FROM purchase_order po
                                     JOIN po_lineitem li ON po.PO_ID = li.PO_ID
                                     JOIN product_part p ON li.PartID = p.PartID
                                     WHERE po.Status != 'Received';";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvPOItems.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("載入採購單失敗: " + ex.Message, "系統錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 當點擊表格中的某一筆採購明細，自動回填到收貨確認區
        private void dgvPOItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvPOItems.Rows[e.RowIndex];
                txtPOID.Text = row.Cells["採購單號"].Value?.ToString() ?? "";
                txtPartID.Text = row.Cells["零件編號"].Value?.ToString() ?? "";
                txtQty.Text = row.Cells["採購數量"].Value?.ToString() ?? "";
            }
        }

        // =========================================================
        // 📥 核心功能：收貨入庫處理 (產生 GRN + 增加產品庫存)
        // =========================================================
        private void btnConfirmReceive_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPOID.Text) || string.IsNullOrWhiteSpace(txtPartID.Text) || string.IsNullOrWhiteSpace(txtStaffResource.Text))
            {
                MessageBox.Show("請填寫收貨倉管員編號，並在大表格中選取一筆欲點收的採購項目！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtQty.Text.Trim(), out int qty) || qty <= 0)
            {
                MessageBox.Show("進貨數量不正確！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string grnID = txtGRNID.Text.Trim();
            string poID = txtPOID.Text.Trim();
            string partID = txtPartID.Text.Trim();
            string staffID = txtStaffResource.Text.Trim();

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // 步驟 1: 寫入 Goods Received Note 主檔 (grn 表)
                            string insertGrnSql = "INSERT INTO grn (GRN_ID, PO_ID, StaffID, ReceivedDate) VALUES (@GRN_ID, @PO_ID, @StaffID, NOW())";
                            using (MySqlCommand cmdGrn = new MySqlCommand(insertGrnSql, conn, trans))
                            {
                                cmdGrn.Parameters.AddWithValue("@GRN_ID", grnID);
                                cmdGrn.Parameters.AddWithValue("@PO_ID", poID);
                                cmdGrn.Parameters.AddWithValue("@StaffID", staffID);
                                cmdGrn.ExecuteNonQuery();
                            }

                            // 步驟 2: 把收進來的貨，【補回加進去】庫存表 (product_part.StockLevel)
                            string addStockSql = "UPDATE product_part SET StockLevel = StockLevel + @Qty WHERE PartID = @PartID";
                            using (MySqlCommand cmdStock = new MySqlCommand(addStockSql, conn, trans))
                            {
                                cmdStock.Parameters.AddWithValue("@Qty", qty);
                                cmdStock.Parameters.AddWithValue("@PartID", partID);
                                cmdStock.ExecuteNonQuery();
                            }

                            // 步驟 3: 更新該採購單狀態為 'Received' 代表已經收貨結案
                            string updatePOSql = "UPDATE purchase_order SET Status = 'Received' WHERE PO_ID = @PO_ID";
                            using (MySqlCommand cmdPO = new MySqlCommand(updatePOSql, conn, trans))
                            {
                                cmdPO.Parameters.AddWithValue("@PO_ID", poID);
                                cmdPO.ExecuteNonQuery();
                            }

                            trans.Commit();
                            MessageBox.Show($"收貨單庫存增補成功！\n收貨單號: {grnID}\n產品 [{partID}] 庫存已成功追加 {qty} 件。", "入庫成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // 清空與重製 UI
                            txtPOID.Clear();
                            txtPartID.Clear();
                            txtQty.Clear();
                            GenerateGRNID();
                            LoadActivePurchaseOrders();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw new Exception("入庫程序出錯，已啟動回滾機制。詳情: " + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "資料庫錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}