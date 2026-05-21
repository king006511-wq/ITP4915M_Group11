using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class LogisticsForm : Form
    {
        // Database connection string synchronized with your Form1 setup
        private string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        public LogisticsForm()
        {
            InitializeComponent();
        }

        private void LogisticsForm_Load(object sender, EventArgs e)
        {
            SetupDispatchControls();
            RefreshPendingOrdersGrid();
        }

        private void SetupDispatchControls()
        {
            // Populate delivery method drop-down options as expected by local logistics operations
            cboMethod.Items.Clear();
            cboMethod.Items.Add("Heavy Truck");
            cboMethod.Items.Add("Light Van");
            cboMethod.Items.Add("Motorcycle Express");
            cboMethod.Items.Add("Self-Pickup");
            cboMethod.SelectedIndex = 0;

            // Restrict delivery scheduling to current date or future dates to enforce data integrity
            dtpEstDelivery.MinDate = DateTime.Today;
        }

        public void RefreshPendingOrdersGrid()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // Query orders that have been marked as 'Packed' by the warehouse and are ready for delivery assignment
                    string query = "SELECT OrderID, CustomerID, OrderDate, Status FROM orders WHERE Status = 'Packed';";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvPendingOrders.DataSource = dt;
                    }

                    // Apply descriptive bilingual headers onto the dynamically bound schema
                    if (dgvPendingOrders.Columns.Contains("OrderID")) dgvPendingOrders.Columns["OrderID"].HeaderText = "Order ID";
                    if (dgvPendingOrders.Columns.Contains("CustomerID")) dgvPendingOrders.Columns["CustomerID"].HeaderText = "Customer ID";
                    if (dgvPendingOrders.Columns.Contains("OrderDate")) dgvPendingOrders.Columns["OrderDate"].HeaderText = "Order Date";
                    if (dgvPendingOrders.Columns.Contains("Status")) dgvPendingOrders.Columns["Status"].HeaderText = "Status";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading pending orders:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvPendingOrders_SelectionChanged(object sender, EventArgs e)
        {
            // Auto-generate a structured Delivery Note sequence ID based on Section 4.2 of your group's Design Specs
            if (dgvPendingOrders.SelectedRows.Count > 0)
            {
                string trackingSeq = DateTime.Now.ToString("mmssfff");
                txtDispatchID.Text = "DN-" + DateTime.Now.ToString("yyyy") + "-" + trackingSeq.Substring(0, 5);
            }
            else
            {
                txtDispatchID.Clear();
            }
        }

        private void btnGenerateDeliveryNote_Click(object sender, EventArgs e)
        {
            // Test Case Validation: Enforce order row targeting allocation
            if (dgvPendingOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a pending order from the list.",
                                "Prompt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Test Case Validation: Verify that mandatory dispatch parameter logs are filled
            if (string.IsNullOrWhiteSpace(txtDriverName.Text))
            {
                MessageBox.Show("Please specify the driver's name.",
                                "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string selectedOrderID = dgvPendingOrders.SelectedRows[0].Cells["OrderID"].Value.ToString();
            string deliveryNoteID = txtDispatchID.Text;
            DateTime estDeliveryDate = dtpEstDelivery.Value;
            string deliveryMethod = cboMethod.Text;
            string driverName = txtDriverName.Text.Trim();

            // Transactional multi-table persistence implementation block
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        // Action A: Populate the delivery table parameters
                        string insertSql = @"INSERT INTO delivery_note 
                                            (DeliveryNoteID, OrderID, EstDeliveryDate, DeliveryMethod, DriverName, Status) 
                                            VALUES (@dnID, @orderID, @estDate, @method, @driver, 'Dispatched');";

                        using (MySqlCommand cmdInsert = new MySqlCommand(insertSql, conn, trans))
                        {
                            cmdInsert.Parameters.AddWithValue("@dnID", deliveryNoteID);
                            cmdInsert.Parameters.AddWithValue("@orderID", selectedOrderID);
                            cmdInsert.Parameters.AddWithValue("@estDate", estDeliveryDate.ToString("yyyy-MM-dd"));
                            cmdInsert.Parameters.AddWithValue("@method", deliveryMethod);
                            cmdInsert.Parameters.AddWithValue("@driver", driverName);
                            cmdInsert.ExecuteNonQuery();
                        }

                        // Action B: Shift Order Transaction status flags inside core sales records
                        string updateSql = "UPDATE orders SET Status = 'Dispatched' WHERE OrderID = @orderID;";
                        using (MySqlCommand cmdUpdate = new MySqlCommand(updateSql, conn, trans))
                        {
                            cmdUpdate.Parameters.AddWithValue("@orderID", selectedOrderID);
                            cmdUpdate.ExecuteNonQuery();
                        }

                        trans.Commit();

                        MessageBox.Show($"Delivery Note Generated Successfully!\n\n" +
                                        $"DN ID: {deliveryNoteID}\n" +
                                        $"Order ID: {selectedOrderID}\n\n" +
                                        $"\"Delivery note\" and \"receipt\" have been generated automatically.",
                                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh component view states
                        txtDriverName.Clear();
                        RefreshPendingOrdersGrid();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Transaction execution failed:\n" + ex.Message, "System error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}