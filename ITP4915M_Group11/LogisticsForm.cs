using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class LogisticsForm : Form
    {
        // ==========================================
        // 🔒 Database Configuration
        // ==========================================
        private readonly string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // ==========================================
        // 🎨 Modern UI Element Variables
        // ==========================================
        private TextBox txtDispatchID, txtDriverName;
        private ComboBox cboMethod;
        private DateTimePicker dtpEstDelivery;
        private DataGridView dgvPendingOrders;
        private Button btnGenerateDeliveryNote;

        public LogisticsForm()
        {
            InitializeComponent();
            InitializePremiumModernUI(); // Initialize Pure English Modern UI
            SetupDispatchControls();     // Initialize ComboBox Options
            RefreshPendingOrdersGrid();  // Load Pending Packed Orders
        }

        #region 🎨 Dynamic Premium English UI Construction
        private void InitializePremiumModernUI()
        {
            // 1. Clear old control leftovers to prevent overlapping
            this.Controls.Clear();

            // 2. Main Window Settings
            this.Text = "Premium Living Furniture - Delivery Logistics Control";
            this.Size = new Size(1180, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // 3. Left Sidebar Navigation Panel
            Panel pnlSidebar = new Panel
            {
                Width = 260,
                Dock = DockStyle.Left,
                BackColor = Color.FromArgb(15, 23, 42)
            };

            Label lblLogo = new Label
            {
                Text = "Premium Living\nFurniture",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 25),
                Size = new Size(220, 60),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlSidebar.Controls.Add(lblLogo);

            string[] menuItems = {
                "🛒 Sales Order Mgmt",
                "🚚 Delivery Logistics",
                "🛋️ Product Maintenance",
                "👔 HR / Staff Mgmt",
                "📦 Goods Received (GRN)",
                "🏭 Material Requests",
                "📊 Procurement Control",
                "🔧 Customer Support",
                "🚪 Logout System"
            };

            int btnTop = 110;
            foreach (string item in menuItems)
            {
                Button btnMenu = new Button
                {
                    Text = "  " + item,
                    Top = btnTop,
                    Left = 12,
                    Size = new Size(236, 48),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand
                };
                btnMenu.FlatAppearance.BorderSize = 0;

                // Highlight current module; Logout shown as danger red
                if (item.Contains("Delivery Logistics"))
                {
                    btnMenu.BackColor = Color.FromArgb(37, 99, 235);
                    btnMenu.ForeColor = Color.White;
                }
                else if (item.Contains("Logout"))
                {
                    btnMenu.BackColor = Color.FromArgb(239, 68, 68);
                    btnMenu.ForeColor = Color.White;
                    btnMenu.MouseEnter += (s, e) => { btnMenu.BackColor = Color.FromArgb(220, 38, 38); };
                    btnMenu.MouseLeave += (s, e) => { btnMenu.BackColor = Color.FromArgb(239, 68, 68); };
                }
                else
                {
                    btnMenu.BackColor = Color.Transparent;
                    btnMenu.ForeColor = Color.FromArgb(148, 163, 184);
                    btnMenu.MouseEnter += (s, e) => { btnMenu.BackColor = Color.FromArgb(51, 65, 85); btnMenu.ForeColor = Color.White; };
                    btnMenu.MouseLeave += (s, e) => { btnMenu.BackColor = Color.Transparent; btnMenu.ForeColor = Color.FromArgb(148, 163, 184); };
                }

                // Sidebar Navigation Routing
                btnMenu.Click += (s, e) => {
                    Form targetForm = null;
                    try
                    {
                        if (item.Contains("Sales Order Mgmt")) targetForm = new OrderManagementForm();
                        else if (item.Contains("Product Maintenance")) targetForm = new ProductManagement();
                        else if (item.Contains("Staff Mgmt")) targetForm = new EmployeeManagement();
                        else if (item.Contains("Received")) targetForm = new GoodsReceivedForm();
                        else if (item.Contains("Material Requests")) targetForm = new RawMaterialRequestForm();
                        else if (item.Contains("Procurement")) targetForm = new ProcurementForm();
                        else if (item.Contains("Support")) targetForm = new AfterServiceForm();
                        else if (item.Contains("Logout")) { Application.Restart(); return; }

                        if (targetForm != null && !(targetForm is LogisticsForm))
                        {
                            this.Hide();
                            targetForm.FormClosed += (senderForm, args) => this.Show();
                            targetForm.Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Navigation error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                pnlSidebar.Controls.Add(btnMenu);
                btnTop += 55;
            }
            this.Controls.Add(pnlSidebar);

            // 4. Right Main Workspace Panel
            Panel pnlMain = new Panel
            {
                Location = new Point(260, 0),
                Size = new Size(900, 750)
            };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label
            {
                Text = "Delivery Logistics Management Center",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(30, 20),
                AutoSize = true
            };
            pnlMain.Controls.Add(lblHeader);

            // 5. Dispatch Input Card Panel
            Panel pnlCard = new Panel
            {
                Location = new Point(30, 85),
                Size = new Size(420, 600),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlCard);

            Label lblCardTitle = new Label
            {
                Text = "📋 Dispatch Assignment",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 65;

            // Delivery Note ID (Auto-generated)
            txtDispatchID = CreateStyledTextBox(pnlCard, ref startY, "Delivery Note ID (DN ID):", true);

            // Driver Name Label & TextBox
            txtDriverName = CreateStyledTextBox(pnlCard, ref startY, "Driver Name *:", false);

            // Delivery Method ComboBox
            Label lblMethod = new Label { Text = "Delivery Method:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboMethod = new ComboBox { Location = new Point(20, startY + 22), Width = 375, Font = new Font("Segoe UI", 10.5F), DropDownStyle = ComboBoxStyle.DropDownList };
            pnlCard.Controls.Add(lblMethod);
            pnlCard.Controls.Add(cboMethod);
            startY += 67;

            // Est. Delivery Date Picker
            Label lblDate = new Label { Text = "Est. Delivery Date:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            dtpEstDelivery = new DateTimePicker { Location = new Point(20, startY + 22), Width = 375, Font = new Font("Segoe UI", 10.5F), Format = DateTimePickerFormat.Short };
            pnlCard.Controls.Add(lblDate);
            pnlCard.Controls.Add(dtpEstDelivery);
            startY += 85;

            // Action Button
            btnGenerateDeliveryNote = new Button
            {
                Text = "🚚 Generate Delivery Note",
                Location = new Point(20, startY),
                Size = new Size(375, 48),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnGenerateDeliveryNote.FlatAppearance.BorderSize = 0;
            btnGenerateDeliveryNote.Click += btnGenerateDeliveryNote_Click;
            pnlCard.Controls.Add(btnGenerateDeliveryNote);

            // 6. Data Grid View Panel (Right-side list)
            Label lblGridTitle = new Label
            {
                Text = "📦 Pending Packed Orders",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(480, 85),
                AutoSize = true
            };
            pnlMain.Controls.Add(lblGridTitle);

            dgvPendingOrders = new DataGridView
            {
                Location = new Point(480, 125),
                Size = new Size(390, 560),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                GridColor = Color.FromArgb(241, 245, 249)
            };

            // Modern DataGridView Styling
            dgvPendingOrders.EnableHeadersVisualStyles = false;
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvPendingOrders.ColumnHeadersHeight = 38;
            dgvPendingOrders.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvPendingOrders.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
            dgvPendingOrders.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            dgvPendingOrders.SelectionChanged += dgvPendingOrders_SelectionChanged;
            pnlMain.Controls.Add(dgvPendingOrders);
        }

        private TextBox CreateStyledTextBox(Panel container, ref int topY, string labelText, bool readOnly)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(20, topY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(20, topY + 22), Width = 375, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            if (readOnly)
            {
                txt.ReadOnly = true;
                txt.BackColor = Color.FromArgb(241, 245, 249);
            }
            container.Controls.Add(lbl);
            container.Controls.Add(txt);
            topY += 65;
            return txt;
        }
        #endregion

        #region 📦 Core Logistics Functional Logic
        private void SetupDispatchControls()
        {
            cboMethod.Items.Clear();
            cboMethod.Items.Add("Heavy Truck");
            cboMethod.Items.Add("Light Van");
            cboMethod.Items.Add("Motorcycle Express");
            cboMethod.Items.Add("Self-Pickup");
            cboMethod.SelectedIndex = 0;

            dtpEstDelivery.MinDate = DateTime.Today;
        }

        public void RefreshPendingOrdersGrid()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT OrderID, CustomerID, OrderDate, Status FROM orders WHERE Status = 'Packed';";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvPendingOrders.DataSource = dt;
                    }

                    // Pure English Header Customization
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
            if (dgvPendingOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a pending order from the list first!",
                                "Prompt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDriverName.Text))
            {
                MessageBox.Show("Please specify the driver's name!",
                                "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Secure retrieval using database raw columns
            string selectedOrderID = dgvPendingOrders.SelectedRows[0].Cells["OrderID"].Value.ToString();
            string deliveryNoteID = txtDispatchID.Text;
            DateTime estDeliveryDate = dtpEstDelivery.Value;
            string deliveryMethod = cboMethod.Text;
            string driverName = txtDriverName.Text.Trim();

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
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
                                        $"The system has automatically outputted the Delivery Note and Receipt Note.",
                                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        txtDriverName.Clear();
                        RefreshPendingOrdersGrid();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Transaction execution failed:\n" + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion
    }
}