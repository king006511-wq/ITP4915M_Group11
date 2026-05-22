using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class RawMaterialRequestForm : Form
    {
        private readonly string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        public RawMaterialRequestForm()
        {
            InitializeComponent();
        }

        private void RawMaterialRequestForm_Load(object sender, EventArgs e)
        {
            txtCardID.Text = "RC-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            LoadRequests();
        }

        private void LoadRequests()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT ReOrderCardID AS '請求單號', PartID AS '產品編號', TriggerDate AS '申請時間', RequestedQty AS '請求數量', Status AS '狀態' FROM reorder_card ORDER BY TriggerDate DESC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvRequests.DataSource = dt;
                    }
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            }
        }

        private void btnSubmitRequest_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPartID.Text) || !int.TryParse(txtQty.Text.Trim(), out int qty) || qty <= 0)
            {
                MessageBox.Show("請輸入正確的產品編號與大於 0 的數量！");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO reorder_card (ReOrderCardID, PartID, RequestedQty, Status) VALUES (@RCID, @PartID, @Qty, 'Pending')";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@RCID", txtCardID.Text.Trim());
                        cmd.Parameters.AddWithValue("@PartID", txtPartID.Text.Trim());
                        cmd.Parameters.AddWithValue("@Qty", qty);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("物料補貨請求已成功送出至採購部門！");
                    txtPartID.Clear();
                    txtQty.Clear();
                    txtCardID.Text = "RC-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
                    LoadRequests();
                }
                catch (Exception ex) { MessageBox.Show("提交失敗，請檢查產品編號是否正確存在。"); }
            }
        }
    }
}