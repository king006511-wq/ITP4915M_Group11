using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class ProcurementForm : Form
    {
        // ==========================================
        // 🔒 Database Configuration
        // ==========================================
        private readonly string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // ==========================================
        // 🎨 Modern UI Element Variables
        // ==========================================
        private TextBox txtPOID, txtRCID, txtPartID, txtQty, txtSupplierID, txtStaffID, txtPrice;
        private DataGridView dgvPendingRC;
        private Button btnCreatePO;

        public ProcurementForm()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                InitializePremiumModernUI(); // 載入全英文高級採購 UI
            }
        }

        private void ProcurementForm_Load(object sender, EventArgs e)
        {
            ResetPurchaseOrderID();
            LoadPendingRequests();
        }

        #region 🎨 Dynamic Premium English UI Construction
        private void InitializePremiumModernUI()
        {
            // 1. Main Window Settings
            this.Text = "Premium Living Furniture - Procurement Control Center";
            this.Size = new Size(1180, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // ==============================================================
            // 🛑 左側導覽列 (Left Sidebar) 已經根據要求完全移除！
            // ==============================================================

            // 3. Right Main Workspace Panel (改為靠左填滿，完美適應新主控台)
            Panel pnlMain = new Panel
            {
                Dock = DockStyle.Fill, // ⬅️ 讓工作區自動填滿，不寫死座標，就不會跟左側選單重疊
                BackColor = Color.Transparent
            };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label
            {
                Text = "Procurement & Purchase Order Management",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(30, 20),
                AutoSize = true
            };
            pnlMain.Controls.Add(lblHeader);

            // Go Back button (visible, returns to previous page)
            Button btnBackHome = new Button { Text = "🔙 Go Back", Size = new Size(120, 34), Location = new Point(740, 22), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand, Visible = true };
            btnBackHome.FlatAppearance.BorderSize = 0;
            btnBackHome.Click += (s, e) => { try { this.Close(); } catch { this.Hide(); } };
            pnlMain.Controls.Add(btnBackHome);

            // 4. Input Card Panel (左側建立採購單卡片)
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
                Text = "📝 Generate Purchase Order (PO)",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlCard.Controls.Add(lblCardTitle);

            // 🌟 全英文欄位標籤與配置
            int startY = 55;
            txtPOID = CreateStyledTextBox(pnlCard, ref startY, "Purchase Order ID (Auto):", true);
            txtRCID = CreateStyledTextBox(pnlCard, ref startY, "Linked Re-Order Card ID:", true);
            txtPartID = CreateStyledTextBox(pnlCard, ref startY, "Part / Material ID:", true);
            txtQty = CreateStyledTextBox(pnlCard, ref startY, "Purchase Quantity:", true);
            txtSupplierID = CreateStyledTextBox(pnlCard, ref startY, "Supplier ID *:", false);
            txtStaffID = CreateStyledTextBox(pnlCard, ref startY, "Staff ID (Buyer) *:", false);
            txtPrice = CreateStyledTextBox(pnlCard, ref startY, "Negotiated Unit Price (HKD) *:", false);

            // 綠色確定建立 PO 按鈕
            btnCreatePO = new Button
            {
                Text = "🚀 Issue Official Purchase Order",
                Location = new Point(20, startY + 10),
                Size = new Size(375, 52),
                BackColor = Color.FromArgb(16, 185, 129), // 翡翠綠
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCreatePO.FlatAppearance.BorderSize = 0;
            btnCreatePO.Click += btnCreatePO_Click;
            pnlCard.Controls.Add(btnCreatePO);

            // 5. Data View Panel (右側待處理補貨請求)
            Label lblGridTitle = new Label
            {
                Text = "📋 Pending Re-Order Requests (Cards)",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(480, 85),
                AutoSize = true
            };
            pnlMain.Controls.Add(lblGridTitle);

            dgvPendingRC = new DataGridView
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

            dgvPendingRC.EnableHeadersVisualStyles = false;
            dgvPendingRC.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvPendingRC.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPendingRC.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvPendingRC.ColumnHeadersHeight = 38;
            dgvPendingRC.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvPendingRC.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
            dgvPendingRC.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            // 綁定點擊事件
            dgvPendingRC.CellClick += dgvPendingRC_CellClick;

            pnlMain.Controls.Add(dgvPendingRC);
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
            topY += 60; // 緊湊間距防止切掣
            return txt;
        }
        #endregion

        #region 💾 Core English-Only Data Logic
        private void ResetPurchaseOrderID()
        {
            txtPOID.Text = "PO-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }

        private void LoadPendingRequests()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // 🌟 欄位標頭全英文化
                    string query = "SELECT ReOrderCardID AS 'Card ID', PartID AS 'Part ID', RequestedQty AS 'Requested Qty' FROM reorder_card WHERE Status = 'Pending'";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvPendingRC.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Load Requests Error: " + ex.Message);
                }
            }
        }

        private void dgvPendingRC_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // 對應英文化後的 DataGridView 欄位名稱
                txtRCID.Text = dgvPendingRC.Rows[e.RowIndex].Cells["Card ID"].Value?.ToString();
                txtPartID.Text = dgvPendingRC.Rows[e.RowIndex].Cells["Part ID"].Value?.ToString();
                txtQty.Text = dgvPendingRC.Rows[e.RowIndex].Cells["Requested Qty"].Value?.ToString();
            }
        }

        private void btnCreatePO_Click(object sender, EventArgs e)
        {
            // 權限檢查
            if (!AuthorizationHelper.IsInRole(AuthorizationHelper.Roles.Procurement, AuthorizationHelper.Roles.Manager, AuthorizationHelper.Roles.Administrator))
            {
                MessageBox.Show("Access Denied: insufficient privileges to create purchase orders.", "Authorization", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSupplierID.Text) || string.IsNullOrWhiteSpace(txtStaffID.Text) || !decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Please fill in Supplier ID, Staff ID, and a valid Unit Price!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🌟 將檢查與寫入合併在同一個 using 連線區塊中
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    // 1. 先用同一條連線檢查 Staff ID
                    string staffCheckSql = "SELECT COUNT(1) FROM staff WHERE StaffID = @StaffID";
                    using (MySqlCommand cmdCheck = new MySqlCommand(staffCheckSql, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@StaffID", txtStaffID.Text.Trim());
                        if (Convert.ToInt32(cmdCheck.ExecuteScalar()) == 0)
                        {
                            MessageBox.Show("Specified Staff ID does not exist in staff records. Please verify the Staff ID.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // 2. 檢查通過，直接開啟 Transaction 寫入
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            string poSql = "INSERT INTO purchase_order (PO_ID, SupplierID, StaffID, ReOrderCardID, PODate, Status) VALUES (@PO, @Sup, @Staff, @RC, NOW(), 'Ordered')";
                            using (MySqlCommand cmd = new MySqlCommand(poSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@PO", txtPOID.Text.Trim());
                                cmd.Parameters.AddWithValue("@Sup", txtSupplierID.Text.Trim());
                                cmd.Parameters.AddWithValue("@Staff", txtStaffID.Text.Trim());
                                cmd.Parameters.AddWithValue("@RC", string.IsNullOrWhiteSpace(txtRCID.Text) ? (object)DBNull.Value : txtRCID.Text.Trim());
                                cmd.ExecuteNonQuery();
                            }

                            string lineSql = "INSERT INTO po_lineitem (PO_ID, PartID, Quantity, UnitPrice) VALUES (@PO, @Part, @Qty, @Price)";
                            using (MySqlCommand cmd = new MySqlCommand(lineSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@PO", txtPOID.Text.Trim());
                                cmd.Parameters.AddWithValue("@Part", txtPartID.Text.Trim());
                                cmd.Parameters.AddWithValue("@Qty", Convert.ToInt32(txtQty.Text));
                                cmd.Parameters.AddWithValue("@Price", price);
                                cmd.ExecuteNonQuery();
                            }

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
                            MessageBox.Show($"Purchase Order [{txtPOID.Text}] successfully issued to Supplier!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            txtRCID.Clear(); txtPartID.Clear(); txtQty.Clear(); txtPrice.Clear(); txtSupplierID.Clear();
                            ResetPurchaseOrderID();
                            LoadPendingRequests();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw new Exception("Transaction failed, database rolled back. Reason: " + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to create Purchase Order: " + ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion
    }
}