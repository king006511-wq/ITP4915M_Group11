using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public class POSupplierItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }

    public partial class ProcurementForm : Form
    {
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        private TextBox custom_txtPOID, custom_txtRCID, custom_txtMaterialID, custom_txtQty, custom_txtPrice, custom_txtStaffID;
        private ComboBox custom_cmbSupplier;
        private DataGridView custom_dgvPendingRC;
        private Button custom_btnCreatePO, custom_btnReject, custom_btnClear;

        private Panel custom_pnlLeftCard;
        private Label custom_lblGridTitle;

        public ProcurementForm()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                SetupCustomSleekUI();
                ResetPurchaseOrderID();
                LoadPendingRequests();

                this.Load += ProcurementForm_Load;
                this.SizeChanged += ProcurementForm_SizeChanged;
                this.Layout += (s, e) => RecalculateDynamicLayout();
            }
        }

        #region 🔒 權限驗證
        private void ProcurementForm_Load(object sender, EventArgs e)
        {
            custom_txtStaffID.Text = UserSession.LoggedInStaffID ?? "S001";

            string currentRole = UserSession.LoggedInStaffRole;
            bool isAuthorized = !string.IsNullOrEmpty(currentRole) &&
                                (currentRole.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                                 currentRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase) ||
                                 currentRole.Equals("Procurement Officer", StringComparison.OrdinalIgnoreCase));

            if (!isAuthorized)
            {
                MessageBox.Show("[SECURITY ALERT] Access Denied!\n\nOnly Procurement Officers and Management can issue Purchase Orders.", "System Security Enforcer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.BeginInvoke(new MethodInvoker(this.Close));
            }
        }
        #endregion

        #region 🎨 精緻手動算繪排版
        private void SetupCustomSleekUI()
        {
            this.Controls.Clear();
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopLevel = false;
            this.Dock = DockStyle.Fill;

            custom_pnlLeftCard = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, AutoScroll = true };
            custom_pnlLeftCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, custom_pnlLeftCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            this.Controls.Add(custom_pnlLeftCard);

            Label lblCardTitle = new Label { Text = "🛒 Approve & Issue PO", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(79, 70, 229), Location = new Point(22, 18), AutoSize = true };
            custom_pnlLeftCard.Controls.Add(lblCardTitle);

            int startY = 65;
            int inputWidth = 350;

            custom_txtPOID = CreateCustomTextBox(custom_pnlLeftCard, ref startY, "New PO ID (Auto):", true, inputWidth);
            custom_txtRCID = CreateCustomTextBox(custom_pnlLeftCard, ref startY, "Ref. Request ID:", true, inputWidth);
            custom_txtMaterialID = CreateCustomTextBox(custom_pnlLeftCard, ref startY, "Raw Material ID:", true, inputWidth);
            custom_txtQty = CreateCustomTextBox(custom_pnlLeftCard, ref startY, "Approved Quantity *:", false, inputWidth);
            custom_txtPrice = CreateCustomTextBox(custom_pnlLeftCard, ref startY, "Unit Cost Price ($) *:", false, inputWidth);

            Label lblSup = new Label { Text = "Select Supplier (Vendor) *:", Location = new Point(22, startY), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            custom_cmbSupplier = new ComboBox { Location = new Point(22, startY + 24), Width = inputWidth, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11F), BackColor = Color.White };
            custom_pnlLeftCard.Controls.Add(lblSup);
            custom_pnlLeftCard.Controls.Add(custom_cmbSupplier);
            startY += 75;

            custom_txtStaffID = CreateCustomTextBox(custom_pnlLeftCard, ref startY, "Processed By (Staff ID):", true, inputWidth);

            custom_btnCreatePO = new Button { Text = "✅ Approve & PO", Location = new Point(22, startY + 10), Size = new Size(130, 42), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            custom_btnReject = new Button { Text = "❌ Reject", Location = new Point(160, startY + 10), Size = new Size(100, 42), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            custom_btnClear = new Button { Text = "🔄 Clear", Location = new Point(270, startY + 10), Size = new Size(100, 42), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };

            custom_btnCreatePO.FlatAppearance.BorderSize = 0; custom_btnReject.FlatAppearance.BorderSize = 0; custom_btnClear.FlatAppearance.BorderSize = 0;

            custom_btnCreatePO.Click += btnCreatePO_Click;
            custom_btnReject.Click += btnReject_Click;
            custom_btnClear.Click += (s, e) => ClearCustomFields();

            custom_pnlLeftCard.Controls.Add(custom_btnCreatePO);
            custom_pnlLeftCard.Controls.Add(custom_btnReject);
            custom_pnlLeftCard.Controls.Add(custom_btnClear);

            custom_lblGridTitle = new Label { Text = "⏳ Pending Replenishment Requests", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), AutoSize = true };
            this.Controls.Add(custom_lblGridTitle);

            custom_dgvPendingRC = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                ScrollBars = ScrollBars.Both,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                EnableHeadersVisualStyles = false
            };

            custom_dgvPendingRC.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            custom_dgvPendingRC.DefaultCellStyle.Padding = new Padding(8);
            custom_dgvPendingRC.DefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            custom_dgvPendingRC.RowTemplate.Height = 36;
            custom_dgvPendingRC.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            custom_dgvPendingRC.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            custom_dgvPendingRC.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            custom_dgvPendingRC.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            custom_dgvPendingRC.ColumnHeadersDefaultCellStyle.Padding = new Padding(8);
            custom_dgvPendingRC.ColumnHeadersHeight = 42;
            custom_dgvPendingRC.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            custom_dgvPendingRC.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);

            custom_dgvPendingRC.SelectionChanged += dgvPendingRC_SelectionChanged;
            this.Controls.Add(custom_dgvPendingRC);
        }

        private TextBox CreateCustomTextBox(Panel container, ref int topY, string labelText, bool readOnly, int width)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(22, topY), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(22, topY + 24), Width = width, Font = new Font("Segoe UI", 11F), BorderStyle = BorderStyle.FixedSingle };
            if (readOnly) { txt.ReadOnly = true; txt.BackColor = Color.FromArgb(241, 245, 249); }
            container.Controls.Add(lbl); container.Controls.Add(txt);
            topY += 75;
            return txt;
        }

        private void ProcurementForm_SizeChanged(object sender, EventArgs e) { RecalculateDynamicLayout(); }

        private void RecalculateDynamicLayout()
        {
            if (this.Width < 200 || this.Height < 200) return;
            this.SuspendLayout();

            custom_pnlLeftCard.Location = new Point(20, 20);
            custom_pnlLeftCard.Size = new Size(400, this.Height - 40);

            int rightStartX = custom_pnlLeftCard.Right + 20;
            int rightWidth = this.Width - rightStartX - 20;

            if (rightWidth > 100)
            {
                custom_lblGridTitle.Location = new Point(rightStartX, 20);
                custom_dgvPendingRC.Location = new Point(rightStartX, 55);
                custom_dgvPendingRC.Size = new Size(rightWidth, this.Height - 75);

                if (custom_dgvPendingRC.Columns.Count > 0)
                {
                    custom_dgvPendingRC.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                    custom_dgvPendingRC.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
            this.ResumeLayout(false);
        }
        #endregion

        #region 💾 資料庫連線與核心邏輯
        private void LoadSuppliersForMaterial(string materialID)
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT s.SupplierID, s.SupplierName 
                        FROM supplier s
                        JOIN supplier_material sm ON s.SupplierID = sm.SupplierID
                        WHERE sm.MaterialID = @matID";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@matID", materialID);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<POSupplierItem> list = new List<POSupplierItem>();
                            while (reader.Read())
                            {
                                list.Add(new POSupplierItem
                                {
                                    ID = reader["SupplierID"].ToString(),
                                    Name = reader["SupplierName"].ToString()
                                });
                            }

                            custom_cmbSupplier.DataSource = list;
                            custom_cmbSupplier.DisplayMember = "Name";
                            custom_cmbSupplier.ValueMember = "ID";
                            custom_cmbSupplier.SelectedIndex = -1;
                        }
                    }
                }
                catch (Exception) { }
            }
        }

        // 🌟 核心優化：智能讀取資料庫最大單號，避免重複
        private void ResetPurchaseOrderID()
        {
            string prefix = "PO" + DateTime.Now.ToString("yyyyMMdd") + "-";
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT PO_ID FROM purchase_order WHERE PO_ID LIKE @prefix ORDER BY PO_ID DESC LIMIT 1";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@prefix", prefix + "%");
                        object result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            string lastID = result.ToString();
                            string seqStr = lastID.Replace(prefix, "");
                            if (int.TryParse(seqStr, out int seq))
                            {
                                custom_txtPOID.Text = prefix + (seq + 1).ToString("D3");
                                return;
                            }
                        }
                    }
                }
                catch (Exception) { /* Fail silently, use fallback */ }
            }
            // 如果今日仲未有單，就由 001 開始
            custom_txtPOID.Text = prefix + "001";
        }

        private void LoadPendingRequests()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            r.ReOrderCardID AS 'Request ID', 
                            r.MaterialID AS 'Material ID', 
                            m.MaterialName AS 'Material Name', 
                            r.RequestedQty AS 'Qty', 
                            r.TriggerDate AS 'Date' 
                        FROM reorder_card r
                        JOIN raw_material m ON r.MaterialID = m.MaterialID
                        WHERE r.Status = 'Pending Approval'
                        ORDER BY r.TriggerDate ASC";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        custom_dgvPendingRC.DataSource = null;
                        custom_dgvPendingRC.DataSource = dt;

                        if (custom_dgvPendingRC.Columns.Contains("Date"))
                        {
                            custom_dgvPendingRC.Columns["Date"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
                        }
                        RecalculateDynamicLayout();
                        custom_dgvPendingRC.ClearSelection();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load Pending Requests:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvPendingRC_SelectionChanged(object sender, EventArgs e)
        {
            if (custom_dgvPendingRC.SelectedRows.Count > 0)
            {
                DataGridViewRow row = custom_dgvPendingRC.SelectedRows[0];
                custom_txtRCID.Text = row.Cells["Request ID"].Value?.ToString() ?? "";
                string matID = row.Cells["Material ID"].Value?.ToString() ?? "";
                custom_txtMaterialID.Text = matID;
                custom_txtQty.Text = row.Cells["Qty"].Value?.ToString() ?? "";

                custom_cmbSupplier.DataSource = null;

                if (!string.IsNullOrEmpty(matID))
                {
                    LoadSuppliersForMaterial(matID);

                    using (MySqlConnection conn = new MySqlConnection(connString))
                    {
                        try
                        {
                            conn.Open();
                            MySqlCommand cmd = new MySqlCommand("SELECT StandardCost FROM raw_material WHERE MaterialID = @mat", conn);
                            cmd.Parameters.AddWithValue("@mat", matID);
                            object result = cmd.ExecuteScalar();

                            if (result != null && result != DBNull.Value)
                                custom_txtPrice.Text = Convert.ToDecimal(result).ToString("F2");
                            else
                                custom_txtPrice.Text = "0.00";
                        }
                        catch (Exception) { custom_txtPrice.Text = ""; }
                    }
                }
            }
        }

        private void btnReject_Click(object sender, EventArgs e)
        {
            string rcID = custom_txtRCID.Text.Trim();
            if (string.IsNullOrWhiteSpace(rcID))
            {
                MessageBox.Show("Please select a pending request to reject.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult dialogResult = MessageBox.Show($"Are you sure you want to REJECT the material request [{rcID}]?\n\nThis will notify the staff and remove it from pending.", "Confirm Rejection", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        string rcSql = "UPDATE reorder_card SET Status = 'Rejected' WHERE ReOrderCardID = @RC";
                        using (MySqlCommand cmd = new MySqlCommand(rcSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@RC", rcID);
                            cmd.ExecuteNonQuery();
                        }
                        MessageBox.Show($"Request [{rcID}] has been rejected.", "Rejected", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        ClearCustomFields();
                        LoadPendingRequests();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to reject request:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnCreatePO_Click(object sender, EventArgs e)
        {
            string poID = custom_txtPOID.Text.Trim();
            string rcID = custom_txtRCID.Text.Trim();
            string matID = custom_txtMaterialID.Text.Trim();
            string qtyStr = custom_txtQty.Text.Trim();
            string priceStr = custom_txtPrice.Text.Trim();
            string staffID = custom_txtStaffID.Text.Trim();

            if (string.IsNullOrWhiteSpace(rcID) || string.IsNullOrWhiteSpace(matID))
            {
                MessageBox.Show("Please select a pending request from the list.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (custom_cmbSupplier.SelectedItem == null)
            {
                MessageBox.Show("Please select a Supplier for this Purchase Order.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string supID = ((POSupplierItem)custom_cmbSupplier.SelectedItem).ID;

            if (!int.TryParse(qtyStr, out int qty) || qty <= 0 || !decimal.TryParse(priceStr, out decimal price) || price <= 0)
            {
                MessageBox.Show("Please enter valid positive numbers for Quantity and Unit Price.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            string poSql = "INSERT INTO purchase_order (PO_ID, SupplierID, ReOrderCardID, PODate, Status, StaffID) VALUES (@PO, @Sup, @RC, NOW(), 'Ordered', @Staff)";
                            using (MySqlCommand cmd = new MySqlCommand(poSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@PO", poID);
                                cmd.Parameters.AddWithValue("@Sup", supID);
                                cmd.Parameters.AddWithValue("@RC", rcID);
                                cmd.Parameters.AddWithValue("@Staff", staffID);
                                cmd.ExecuteNonQuery();
                            }

                            string polSql = "INSERT INTO po_lineitem (PO_ID, MaterialID, Quantity, UnitPrice) VALUES (@PO, @Mat, @Qty, @Price)";
                            using (MySqlCommand cmd = new MySqlCommand(polSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@PO", poID);
                                cmd.Parameters.AddWithValue("@Mat", matID);
                                cmd.Parameters.AddWithValue("@Qty", qty);
                                cmd.Parameters.AddWithValue("@Price", price);
                                cmd.ExecuteNonQuery();
                            }

                            string rcSql = "UPDATE reorder_card SET Status = 'Approved' WHERE ReOrderCardID = @RC";
                            using (MySqlCommand cmd = new MySqlCommand(rcSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@RC", rcID);
                                cmd.ExecuteNonQuery();
                            }

                            trans.Commit();
                            MessageBox.Show($"Purchase Order [{poID}] successfully issued to Supplier!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            ClearCustomFields();
                            ResetPurchaseOrderID(); // 成功後立即刷新 ID
                            LoadPendingRequests();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw new Exception("Transaction failed. Reason: " + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to create PO:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearCustomFields()
        {
            custom_txtRCID.Clear();
            custom_txtMaterialID.Clear();
            custom_txtQty.Clear();
            custom_txtPrice.Clear();
            custom_cmbSupplier.DataSource = null;
            custom_dgvPendingRC.ClearSelection();
            ResetPurchaseOrderID(); // Clear 嗰陣都重新拎一次最新 ID 保平安
        }
        #endregion
    }
}