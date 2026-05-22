using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class ProcurementForm : Form
    {
        private readonly string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        public ProcurementForm()
        {
            InitializeComponent();
        }

        private void ProcurementForm_Load(object sender, EventArgs e)
        {
            txtPOID.Text = "PO-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            LoadPendingRequests();
        }

        private void LoadPendingRequests()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT ReOrderCardID, PartID, RequestedQty FROM reorder_card WHERE Status = 'Pending'";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvPendingRC.DataSource = dt;
                    }
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            }
        }

        private void dgvPendingRC_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtRCID.Text = dgvPendingRC.Rows[e.RowIndex].Cells["ReOrderCardID"].Value?.ToString();
                txtPartID.Text = dgvPendingRC.Rows[e.RowIndex].Cells["PartID"].Value?.ToString();
                txtQty.Text = dgvPendingRC.Rows[e.RowIndex].Cells["RequestedQty"].Value?.ToString();
            }
        }

        private void btnCreatePO_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSupplierID.Text) || string.IsNullOrWhiteSpace(txtStaffID.Text) || !decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("請完整填寫供應商、員工與正確的採購單價！");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        // 1. 插入採購單主檔
                        string poSql = "INSERT INTO purchase_order (PO_ID, SupplierID, StaffID, ReOrderCardID, PODate, Status) VALUES (@PO, @Sup, @Staff, @RC, NOW(), 'Ordered')";
                        using (MySqlCommand cmd = new MySqlCommand(poSql, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@PO", txtPOID.Text.Trim());
                            cmd.Parameters.AddWithValue("@Sup", txtSupplierID.Text.Trim());
                            cmd.Parameters.AddWithValue("@Staff", txtStaffID.Text.Trim());
                            cmd.Parameters.AddWithValue("@RC", string.IsNullOrWhiteSpace(txtRCID.Text) ? (object)DBNull.Value : txtRCID.Text.Trim());
                            cmd.ExecuteNonQuery();
                        }

                        // 2. 插入採購明細表
                        string lineSql = "INSERT INTO po_lineitem (PO_ID, PartID, Quantity, UnitPrice) VALUES (@PO, @Part, @Qty, @Price)";
                        using (MySqlCommand cmd = new MySqlCommand(lineSql, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@PO", txtPOID.Text.Trim());
                            cmd.Parameters.AddWithValue("@Part", txtPartID.Text.Trim());
                            cmd.Parameters.AddWithValue("@Qty", Convert.ToInt32(txtQty.Text));
                            cmd.Parameters.AddWithValue("@Price", price);
                            cmd.ExecuteNonQuery();
                        }

                        // 3. 更新補貨卡狀態為「已處理 (Approved)」
                        if (!string.IsNullOrWhiteSpace(txtRCID.Text))
                        {
                            string rcSql = "UPDATE reorder_card SET Status = 'Approved' WHERE ReOrderCardID = @RC";
                            using (MySqlCommand cmd = new MySqlCommand(rcSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@RC", txtRCID.Text.Trim());
                                cmd.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                        MessageBox.Show($"向供應商發出的採購單 [{txtPOID.Text}] 建立成功！");

                        txtRCID.Clear(); txtPartID.Clear(); txtQty.Clear(); txtPrice.Clear();
                        txtPOID.Text = "PO-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
                        LoadPendingRequests();
                    }
                }
                catch (Exception ex) { MessageBox.Show("採購單建立失敗: " + ex.Message); }
            }
        }
    }
}