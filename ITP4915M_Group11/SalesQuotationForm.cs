using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class SalesQuotationForm : Form
    {
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        private DataGridView dgvPendingOrders;
        private DataGridView dgvOrderDetails;
        private Button btnApprove, btnReject;
        private Label lblSelectedOrder;

        public SalesQuotationForm()
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                InitializeApprovalUI();
                LoadPendingOrders();
            }
        }

        private void InitializeApprovalUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living - Order Approval & Stock Allocation";
            this.BackColor = Color.FromArgb(241, 245, 249);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(20);

            Label lblHeader = new Label { Text = "🔒 Order Approval & Stock Allocation", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblHeader);

            Label lblSub = new Label { Text = "Review pending orders from Sales. Approve to deduct inventory and pass to Logistics.", Font = new Font("Segoe UI", 10F), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(24, 60), AutoSize = true };
            this.Controls.Add(lblSub);

            Label lblGridTitle = new Label { Text = "⏳ Orders Awaiting Approval", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(20, 100), AutoSize = true };
            this.Controls.Add(lblGridTitle);

            dgvPendingOrders = new DataGridView { Location = new Point(20, 130), Size = new Size(450, 550), BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(139, 92, 246);
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPendingOrders.SelectionChanged += DgvPendingOrders_SelectionChanged;
            this.Controls.Add(dgvPendingOrders);

            lblSelectedOrder = new Label { Text = "Selected Order: None", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(500, 100), AutoSize = true };
            this.Controls.Add(lblSelectedOrder);

            dgvOrderDetails = new DataGridView { Location = new Point(500, 130), Size = new Size(600, 480), BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvOrderDetails.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(71, 85, 105);
            dgvOrderDetails.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvOrderDetails.CellFormatting += DgvOrderDetails_CellFormatting;
            this.Controls.Add(dgvOrderDetails);

            btnApprove = new Button { Text = "✅ Approve & Deduct Stock", Location = new Point(500, 630), Size = new Size(300, 50), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand, Enabled = false };
            btnApprove.FlatAppearance.BorderSize = 0;
            btnApprove.Click += BtnApprove_Click;
            this.Controls.Add(btnApprove);

            btnReject = new Button { Text = "❌ Reject Order", Location = new Point(810, 630), Size = new Size(180, 50), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand, Enabled = false };
            btnReject.FlatAppearance.BorderSize = 0;
            btnReject.Click += BtnReject_Click;
            this.Controls.Add(btnReject);
        }

        private void LoadPendingOrders()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // 🌟 只 Load Awaiting Approval 嘅訂單 (包含 -D 同 -P)
                    string sql = "SELECT OrderID, CustomerID, TotalAmount, Status, OrderDate FROM orders WHERE Status LIKE 'Awaiting Approval%' ORDER BY OrderDate ASC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn))
                    {
                        DataTable dt = new DataTable(); adapter.Fill(dt); dgvPendingOrders.DataSource = dt;
                    }
                }
                catch (Exception) { }
            }
        }

        private void DgvPendingOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPendingOrders.SelectedRows.Count > 0)
            {
                string orderID = dgvPendingOrders.SelectedRows[0].Cells["OrderID"].Value.ToString();
                lblSelectedOrder.Text = $"Selected Order: {orderID} (Stock Check)";
                btnApprove.Enabled = true; btnReject.Enabled = true;

                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        string query = @"SELECT l.ProductID AS 'Product ID', p.ProductName AS 'Name', l.Quantity AS 'Required Qty', p.StockLevel AS 'Current Stock',
                                         CASE WHEN p.StockLevel >= l.Quantity THEN 'OK' ELSE 'SHORTAGE' END AS 'Stock Status'
                                         FROM order_lineitem l JOIN product p ON l.ProductID = p.ProductID WHERE l.OrderID = @OID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@OID", orderID);
                            using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                            {
                                DataTable dt = new DataTable(); adapter.Fill(dt); dgvOrderDetails.DataSource = dt;
                                foreach (DataRow row in dt.Rows)
                                {
                                    if (row["Stock Status"].ToString() == "SHORTAGE") { btnApprove.Enabled = false; break; }
                                }
                            }
                        }
                    }
                    catch (Exception) { }
                }
            }
            else { lblSelectedOrder.Text = "Selected Order: None"; dgvOrderDetails.DataSource = null; btnApprove.Enabled = false; btnReject.Enabled = false; }
        }

        private void DgvOrderDetails_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvOrderDetails.Columns[e.ColumnIndex].Name == "Stock Status" && e.Value != null)
            {
                if (e.Value.ToString() == "SHORTAGE") { e.CellStyle.BackColor = Color.Crimson; e.CellStyle.ForeColor = Color.White; e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold); }
                else { e.CellStyle.ForeColor = Color.MediumSeaGreen; e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold); }
            }
        }

        private void BtnApprove_Click(object sender, EventArgs e)
        {
            if (dgvPendingOrders.SelectedRows.Count == 0) return;
            string orderID = dgvPendingOrders.SelectedRows[0].Cells["OrderID"].Value.ToString();

            // 🌟 智能判斷：係要交畀車隊送貨定係舖頭自取
            string currentStatus = dgvPendingOrders.SelectedRows[0].Cells["Status"].Value.ToString();
            string nextStatus = currentStatus.Contains("-D") ? "Ready for Dispatch" : "Ready for Pickup";

            if (MessageBox.Show($"Approve Order [{orderID}]?\nThis will deduct stock permanently and set status to '{nextStatus}'.", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // 扣減庫存
                            using (MySqlCommand cmdCheck = new MySqlCommand("SELECT ProductID, Quantity FROM order_lineitem WHERE OrderID = @OID", conn, trans))
                            {
                                cmdCheck.Parameters.AddWithValue("@OID", orderID);
                                using (MySqlDataReader r = cmdCheck.ExecuteReader())
                                {
                                    System.Collections.Generic.List<Tuple<string, int>> items = new System.Collections.Generic.List<Tuple<string, int>>();
                                    while (r.Read()) items.Add(new Tuple<string, int>(r["ProductID"].ToString(), Convert.ToInt32(r["Quantity"])));
                                    r.Close();

                                    foreach (var item in items)
                                    {
                                        using (MySqlCommand cmdDeduct = new MySqlCommand("UPDATE product SET StockLevel = StockLevel - @Qty WHERE ProductID = @PID", conn, trans))
                                        {
                                            cmdDeduct.Parameters.AddWithValue("@Qty", item.Item2);
                                            cmdDeduct.Parameters.AddWithValue("@PID", item.Item1);
                                            cmdDeduct.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }

                            // 更新狀態
                            using (MySqlCommand cmdStatus = new MySqlCommand("UPDATE orders SET Status = @nextStatus WHERE OrderID = @OID", conn, trans))
                            {
                                cmdStatus.Parameters.AddWithValue("@nextStatus", nextStatus);
                                cmdStatus.Parameters.AddWithValue("@OID", orderID);
                                cmdStatus.ExecuteNonQuery();
                            }
                            trans.Commit();
                            MessageBox.Show($"Order [{orderID}] Approved! Stock deducted.", "Success");
                            LoadPendingOrders();
                        }
                        catch (Exception ex) { trans.Rollback(); throw ex; }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Approval failed: " + ex.Message); }
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SalesQuotationForm
            // 
            this.ClientSize = new System.Drawing.Size(278, 244);
            this.Name = "SalesQuotationForm";
            this.Load += new System.EventHandler(this.SalesQuotationForm_Load);
            this.ResumeLayout(false);

        }

        private void SalesQuotationForm_Load(object sender, EventArgs e)
        {

        }

        private void BtnReject_Click(object sender, EventArgs e)
        {
            if (dgvPendingOrders.SelectedRows.Count == 0) return;
            string orderID = dgvPendingOrders.SelectedRows[0].Cells["OrderID"].Value.ToString();

            if (MessageBox.Show($"Reject Order [{orderID}]?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("UPDATE orders SET Status = 'Rejected' WHERE OrderID = @OID", conn))
                    {
                        cmd.Parameters.AddWithValue("@OID", orderID); cmd.ExecuteNonQuery();
                    }
                    LoadPendingOrders();
                }
            }
        }
    }
}