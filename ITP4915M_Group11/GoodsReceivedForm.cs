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
        private readonly string connString = UserSession.ConnString;

        // ==========================================
        // 🎨 Modern UI Element Variables
        // ==========================================
        private TextBox txtGRNID, txtPOID, txtPartID, txtQty, txtStaffResource;
        private DataGridView dgvPOItems;
        private Button btnConfirmReceive;

        public GoodsReceivedForm()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                InitializePremiumModernUI(); // Initialize Pure English Dynamic UI
                GenerateGRNID();             // Generate unique runtime sequence ID
                LoadActivePurchaseOrders();  // Ingest pending PO records

                // 確保 Form_Load 事件有綁定
                this.Load += GoodsReceivedForm_Load;
                this.Shown += GoodsReceivedForm_Shown;
            }
        }

        private void GoodsReceivedForm_Shown(object sender, EventArgs e)
        {
            // 顯示時強制授權檢查（保險）
            AuthorizationHelper.EnforceRole(this, AuthorizationHelper.Roles.Manager, AuthorizationHelper.Roles.Administrator, AuthorizationHelper.Roles.Warehouse);
        }

        #region 🎨 Dynamic Premium English UI Construction
        private void InitializePremiumModernUI()
        {
            // 1. Clear old control leftovers to prevent overlapping or designer mismatches
            this.Controls.Clear();

            // 2. Main Window Settings
            this.Text = "Premium Living Furniture - Goods Received Note (GRN) Control";
            this.Size = new Size(920, 750); // Adjusted size to remove sidebar footprint
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // ==============================================================
            // 🛑 Left Sidebar Navigation Panel code has been completely REMOVED here.
            // ==============================================================

            // 4. Right Workspace Body (Shifted to Point(0, 0))
            Panel pnlMain = new Panel
            {
                Location = new Point(0, 0),
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

            // Go Back button (top-right) - returns to previous page by closing current form
            Button btnBackHome = new Button { Text = "🔙 Go Back", Size = new Size(120, 34), Location = new Point(740, 22), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand, Visible = true };
            btnBackHome.FlatAppearance.BorderSize = 0;
            btnBackHome.Click += (s, e) => { try { this.Close(); } catch { this.Hide(); } };
            pnlMain.Controls.Add(btnBackHome);

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
            // Database constraint: GRN_ID is varchar(20). Keep string length under 20 characters.
            txtGRNID.Text = "GRN" + DateTime.Now.ToString("ddHHmmss");
        }

        private void LoadActivePurchaseOrders()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // IMPROVED: Adjusted table name to 'purchase_order' and matched descriptive text mapping to 'PartName'
                    string query = @"SELECT po.PO_ID AS 'PO_ID', po.SupplierID AS 'Supplier_ID', 
                                            li.PartID AS 'Part_ID', p.PartName AS 'Part_Name', 
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
            // 再次在執行關鍵動作前檢查角色
            if (!AuthorizationHelper.IsInRole(AuthorizationHelper.Roles.Manager, AuthorizationHelper.Roles.Administrator, AuthorizationHelper.Roles.Warehouse))
            {
                MessageBox.Show("Access Denied: insufficient privileges to confirm goods received.", "Authorization", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

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
                            // IMPROVED: Target table correctly updated to 'goods_received_note'
                            string insertGrnSql = "INSERT INTO goods_received_note (GRN_ID, PO_ID, StaffID, ReceivedDate) VALUES (@GRN_ID, @PO_ID, @StaffID, NOW())";
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
                            // IMPROVED: Target table updated to 'purchase_order' to align with the active database system
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