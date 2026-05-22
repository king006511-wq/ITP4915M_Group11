using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Data;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class AfterServiceForm : Form
    {
        private readonly string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        public AfterServiceForm()
        {
            InitializeComponent();
        }

        private void AfterServiceForm_Load(object sender, EventArgs e)
        {
            txtComplaintID.Text = "COMP-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            cboStatus.Items.Clear();
            cboStatus.Items.AddRange(new object[] { "Pending", "Resolved", "Refunded" });
            cboStatus.SelectedIndex = 0;
            LoadComplaints();
        }

        private void LoadComplaints()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT ComplaintID, CustomerID, OrderID, Description, ResolutionStatus FROM complaint ORDER BY ComplaintDate DESC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvComplaints.DataSource = dt;
                    }
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            }
        }

        private void btnSubmitComplaint_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCustomerID.Text) || string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("請至少輸入客戶編號與投訴案由內容！");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string sql = @"INSERT INTO complaint (ComplaintID, CustomerID, OrderID, ComplaintDate, Description, ResolutionStatus) 
                                   VALUES (@CID, @CustID, @OID, NOW(), @Desc, @Status)
                                   ON DUPLICATE KEY UPDATE ResolutionStatus = @Status, Description = @Desc;";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CID", txtComplaintID.Text.Trim());
                        cmd.Parameters.AddWithValue("@CustID", txtCustomerID.Text.Trim());
                        cmd.Parameters.AddWithValue("@OID", string.IsNullOrWhiteSpace(txtOrderID.Text) ? (object)DBNull.Value : txtOrderID.Text.Trim());
                        cmd.Parameters.AddWithValue("@Desc", txtDescription.Text.Trim());
                        cmd.Parameters.AddWithValue("@Status", cboStatus.Text);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("售後服務案卷更新/新增成功！");
                    txtCustomerID.Clear(); txtOrderID.Clear(); txtDescription.Clear();
                    txtComplaintID.Text = "COMP-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
                    cboStatus.SelectedIndex = 0;
                    LoadComplaints();
                }
                catch (Exception ex) { MessageBox.Show("儲存失敗: " + ex.Message); }
            }
        }

        private void dgvComplaints_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtComplaintID.Text = dgvComplaints.Rows[e.RowIndex].Cells["ComplaintID"].Value?.ToString();
                txtCustomerID.Text = dgvComplaints.Rows[e.RowIndex].Cells["CustomerID"].Value?.ToString();
                txtOrderID.Text = dgvComplaints.Rows[e.RowIndex].Cells["OrderID"].Value?.ToString();
                txtDescription.Text = dgvComplaints.Rows[e.RowIndex].Cells["Description"].Value?.ToString();
                cboStatus.Text = dgvComplaints.Rows[e.RowIndex].Cells["ResolutionStatus"].Value?.ToString();
            }
        }
    }
}