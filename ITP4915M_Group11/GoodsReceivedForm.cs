using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class GoodsReceivedForm : Form
    {
        // ==========================================
        // 🔒 Database Configuration
        // ==========================================
        private readonly string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // ==========================================
        // 🎨 Modern UI Element Variables
        // ==========================================
        private TextBox txtGRNID, txtPOID, txtPartID, txtQty, txtStaffResource;
        private DataGridView dgvPOItems;
        private Button btnConfirmReceive;

        public GoodsReceivedForm()
        {
            InitializeComponent();
            InitializePremiumModernUI(); // Initialize Pure English Dynamic UI
            GenerateGRNID();             // Generate unique runtime sequence ID
            LoadActivePurchaseOrders();  // Ingest pending PO records

            // 確保 Form_Load 事件有綁定
            this.Load += GoodsReceivedForm_Load;
        }

        #region 🎨 Dynamic Premium English UI Construction
        private void InitializePremiumModernUI()
        {
            // 1. Clear old control leftovers to prevent overlapping or designer mismatches
            this.Controls.Clear();

            // 2. Main Window Settings
            this.Text = "Premium Living Furniture - Goods Received Note (GRN) Control";
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

                // Highlight current Goods Received (GRN) workspace; Logout shown as danger red
                if (item.Contains("Goods Received (GRN)"))
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

                // Cross-Module Sidebar Routing Execution
                btnMenu.Click += (s, e) => {
                    Form targetForm = null;
                    try
                    {
                        if (item.Contains("Sales Order Mgmt")) targetForm = new OrderManagementForm();
                        else if (item.Contains("Delivery Logistics")) targetForm = new LogisticsForm();
                        else if (item.Contains("Product Maintenance")) targetForm = new ProductManagement();
                        else if (item.Contains("Staff Mgmt")) targetForm = new EmployeeManagement();
                        else if (item.Contains("Material Requests")) targetForm = new RawMaterialRequestForm();
                        else if (item.Contains("Procurement")) targetForm = new ProcurementForm();
                        else if (item.Contains("Support")) targetForm = new AfterServiceForm();
                        else if (item.Contains("Logout")) { Application.Restart(); return; }

                        if (targetForm != null && !(targetForm is GoodsReceivedForm))
                        {
                            this.Hide();
                            targetForm.FormClosed += (senderForm, args) => this.Show();
                            targetForm.Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Navigation routing failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                pnlSidebar.Controls.Add(btnMenu);
                btnTop += 55;
            }
            this.Controls.Add(pnlSidebar);

            // 4. Right Workspace Body
            Panel pnlMain = new Panel
            {
                Location = new Point(260, 0),
                Size = new Size(900, 750)
            };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label
            {
                Text = "Warehouse Stock Ingestion & GRN Center",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(30, 20),
                AutoSize = true
            };
            pnlMain.Controls.Add(lblHeader);

            // 5. GRN Ingestion Processing Panel (Left Input Box Card)
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
                Text = "📋 Stock Ingestion Parameters",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 65;

            // Form Input Blocks
            txtGRNID = CreateStyledTextBox(pnlCard, ref startY, "Goods Received ID (GRN ID):", true);
            txtPOID = CreateStyledTextBox(pnlCard, ref startY, "Selected Purchase Order ID (PO ID):", true);
            txtPartID = CreateStyledTextBox(pnlCard, ref startY, "Part ID / Code:", true);
            txtQty = CreateStyledTextBox(pnlCard, ref startY, "Ingestion Quantity (Qty):", false);
            txtStaffResource = CreateStyledTextBox(pnlCard, ref startY, "Warehouse Staff ID *:", false);

            startY += 10;

            // Action Button
            btnConfirmReceive = new Button
            {
                Text = "📥 Confirm Ingestion & Add Stock",
                Location = new Point(20, startY),
                Size = new Size(375, 48),
                BackColor = Color.FromArgb(16, 185, 129), // Bright success emerald green
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnConfirmReceive.FlatAppearance.BorderSize = 0;
            btnConfirmReceive.Click += btnConfirmReceive_Click;
            pnlCard.Controls.Add(btnConfirmReceive);

            // 6. Data Grid View Panel (Right-side pending selection list)
            Label lblGridTitle = new Label
            {
                Text = "📦 Pending Active Purchase Orders",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(480, 85),
                AutoSize = true
            };
            pnlMain.Controls.Add(lblGridTitle);

            dgvPOItems = new DataGridView
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

            // Modern Interactive DataGridView Layout Specifications
            dgvPOItems.EnableHeadersVisualStyles = false;
            dgvPOItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvPOItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPOItems.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvPOItems.ColumnHeadersHeight = 38;
            dgvPOItems.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvPOItems.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
            dgvPOItems.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            // Switch event mapping to modern row selection change for high accuracy mapping
            dgvPOItems.SelectionChanged += dgvPOItems_SelectionChanged;

            pnlMain.Controls.Add(dgvPOItems);
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

        // ⭐⭐⭐ 自動填寫 User Session 嘅代碼 ⭐⭐⭐
        private void GoodsReceivedForm_Load(object sender, EventArgs e)
        {
            // 檢查系統係咪已經有登入紀錄
            if (UserSession.IsLoggedIn)
            {
                // 將儲存咗嘅 ID 放入 txtStaffResource
                txtStaffResource.Text = UserSession.LoggedInStaffID;

                // 鎖死個 TextBox，防止使用者亂改搞到 Database Error
                txtStaffResource.ReadOnly = true;
                txtStaffResource.BackColor = Color.FromArgb(241, 245, 249); // 轉灰底色
            }
            else
            {
                // 如果你測試緊未寫好 Login，可以畀個假 ID 頂住先，唔會阻住你試 Run
                // txtStaffResource.Text = "S001";
            }
        }
        #endregion

        #region 📦 Core Warehouse Stock Ingestion Logic
        private void GenerateGRNID()
        {
            txtGRNID.Text = "GRN-" + DateTime.Now.ToString("yyyyMM") + "-" + DateTime.Now.ToString("ddHHmmss");
        }

        private void LoadActivePurchaseOrders()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // Refactored with clean English column aliases matching structural schemas
                    string query = @"SELECT po.PO_ID AS 'PO_ID', po.SupplierID AS 'Supplier_ID', 
                                            li.PartID AS 'Part_ID', p.Name AS 'Part_Name', 
                                            li.Quantity AS 'Quantity', po.Status AS 'Status'
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

                    // Explicitly define English headers inside visual UI grid
                    if (dgvPOItems.Columns.Contains("PO_ID")) dgvPOItems.Columns["PO_ID"].HeaderText = "PO ID";
                    if (dgvPOItems.Columns.Contains("Supplier_ID")) dgvPOItems.Columns["Supplier_ID"].HeaderText = "Supplier ID";
                    if (dgvPOItems.Columns.Contains("Part_ID")) dgvPOItems.Columns["Part_ID"].HeaderText = "Part ID";
                    if (dgvPOItems.Columns.Contains("Part_Name")) dgvPOItems.Columns["Part_Name"].HeaderText = "Part Name";
                    if (dgvPOItems.Columns.Contains("Quantity")) dgvPOItems.Columns["Quantity"].HeaderText = "Order Qty";
                    if (dgvPOItems.Columns.Contains("Status")) dgvPOItems.Columns["Status"].HeaderText = "Status";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load active purchase orders:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvPOItems_SelectionChanged(object sender, EventArgs e)
        {
            // Safeguard active selections and load parameters securely into the staging panel
            if (dgvPOItems.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvPOItems.SelectedRows[0];
                txtPOID.Text = row.Cells["PO_ID"].Value?.ToString() ?? "";
                txtPartID.Text = row.Cells["Part_ID"].Value?.ToString() ?? "";
                txtQty.Text = row.Cells["Quantity"].Value?.ToString() ?? "";
            }
            else
            {
                txtPOID.Clear();
                txtPartID.Clear();
                txtQty.Clear();
            }
        }

        private void btnConfirmReceive_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPOID.Text) || string.IsNullOrWhiteSpace(txtPartID.Text) || string.IsNullOrWhiteSpace(txtStaffResource.Text))
            {
                MessageBox.Show("Please fill in the Warehouse Staff ID and select an active Purchase Order item from the list!",
                                "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtQty.Text.Trim(), out int qty) || qty <= 0)
            {
                MessageBox.Show("Please specify a valid positive integer quantity for stock ingestion!",
                                "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                            // Step 1: Commit record parameter into Goods Received Note ledger
                            string insertGrnSql = "INSERT INTO grn (GRN_ID, PO_ID, StaffID, ReceivedDate) VALUES (@GRN_ID, @PO_ID, @StaffID, NOW())";
                            using (MySqlCommand cmdGrn = new MySqlCommand(insertGrnSql, conn, trans))
                            {
                                cmdGrn.Parameters.AddWithValue("@GRN_ID", grnID);
                                cmdGrn.Parameters.AddWithValue("@PO_ID", poID);
                                cmdGrn.Parameters.AddWithValue("@StaffID", staffID);
                                cmdGrn.ExecuteNonQuery();
                            }

                            // Step 2: Increment inventory reserves inside core components tables
                            string addStockSql = "UPDATE product_part SET StockLevel = StockLevel + @Qty WHERE PartID = @PartID";
                            using (MySqlCommand cmdStock = new MySqlCommand(addStockSql, conn, trans))
                            {
                                cmdStock.Parameters.AddWithValue("@Qty", qty);
                                cmdStock.Parameters.AddWithValue("@PartID", partID);
                                cmdStock.ExecuteNonQuery();
                            }

                            // Step 3: Flag Purchase Order reference log tracking as 'Received'
                            string updatePOSql = "UPDATE purchase_order SET Status = 'Received' WHERE PO_ID = @PO_ID";
                            using (MySqlCommand cmdPO = new MySqlCommand(updatePOSql, conn, trans))
                            {
                                cmdPO.Parameters.AddWithValue("@PO_ID", poID);
                                cmdPO.ExecuteNonQuery();
                            }

                            trans.Commit();
                            MessageBox.Show($"Inventory ingestion transaction committed successfully!\n\n" +
                                            $"GRN ID: {grnID}\n" +
                                            $"Part ID [{partID}] stock level successfully increased by {qty} units.",
                                            "Ingestion Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Reset view component states
                            txtPOID.Clear();
                            txtPartID.Clear();
                            txtQty.Clear();
                            GenerateGRNID();
                            LoadActivePurchaseOrders();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback(); // Execute safe operational state recovery rollback
                            throw new Exception("Stock ingestion transaction aborted. Control rolled back safely. Details:\n" + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Transaction Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion
    }
}