using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ITP4915M_Group11
{
    public class SupplierComboItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Contact { get; set; } // 🌟 暫存聯絡資料供畫面連動顯示

        // 🌟 UX 優化：如果是未指定，顯示提示字；否則「只顯示 ID」
        public string DisplayText => ID == "UNASSIGNED" ? "--- Unassigned ---" : ID;
    }

    public partial class SupplierAndMaterialCreationForm : Form
    {
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        private string editingSupplierID = null;

        private RadioButton rbExistingSupplier, rbNewSupplier;
        private ComboBox cmbExistingSuppliers;
        private TextBox txtNewSuppName, txtNewSuppContact;
        private TextBox txtMatName, txtMatPrice, txtMatReorderLevel;
        private Button btnSave;

        private DataGridView dgvSuppliers;
        private Button btnDeleteSupplier;
        private Button btnUpdateSupplier;

        public SupplierAndMaterialCreationForm()
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                SetupPremiumUI();
                LoadExistingSuppliers();
                RefreshSupplierGrid();
            }
        }

        #region 🎨 Premium Modern UI Setup
        private void SetupPremiumUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Material & Procurement Hub";
            this.Size = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            Panel pnlMain = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(25) };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label { Text = "🏭 Material Procurement & Supplier Management Hub", Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(25, 15), AutoSize = true };
            pnlMain.Controls.Add(lblHeader);

            Panel pnlSupplier = new Panel { Location = new Point(25, 70), Size = new Size(420, 240), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlSupplier.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlSupplier.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlSupplier);

            Label lblSupplierTitle = new Label { Text = "🏢 Step 1: Supplier Assignment / Editor", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(15, 12), AutoSize = true };
            pnlSupplier.Controls.Add(lblSupplierTitle);

            rbExistingSupplier = new RadioButton { Text = "Link to Existing Supplier", Location = new Point(15, 42), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Checked = true, Cursor = Cursors.Hand };
            rbNewSupplier = new RadioButton { Text = "Register New Supplier", Location = new Point(15, 68), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            pnlSupplier.Controls.Add(rbExistingSupplier);
            pnlSupplier.Controls.Add(rbNewSupplier);

            rbExistingSupplier.CheckedChanged += SupplierMode_CheckedChanged;
            rbNewSupplier.CheckedChanged += SupplierMode_CheckedChanged;

            Label lblSelectSupp = new Label { Text = "Select Supplier ID:", Location = new Point(220, 42), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cmbExistingSuppliers = new ComboBox { Location = new Point(220, 62), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F) };

            // 🌟 綁定下拉選單變更事件，動態更新下方 TextBox 顯示公司全名
            cmbExistingSuppliers.SelectedIndexChanged += CmbExistingSuppliers_SelectedIndexChanged;

            pnlSupplier.Controls.Add(lblSelectSupp); pnlSupplier.Controls.Add(cmbExistingSuppliers);

            Label lblLine = new Label { Text = "──────────────────────────────────────────", Location = new Point(15, 95), AutoSize = true, ForeColor = Color.FromArgb(241, 245, 249) };
            pnlSupplier.Controls.Add(lblLine);

            Label lblNewSupp = new Label { Text = "Supplier Name * (New / View):", Location = new Point(15, 120), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtNewSuppName = new TextBox { Location = new Point(15, 140), Width = 180, Font = new Font("Segoe UI", 9.5F), BorderStyle = BorderStyle.FixedSingle };

            Label lblNewContact = new Label { Text = "Contact Info (Phone/Email):", Location = new Point(220, 120), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtNewSuppContact = new TextBox { Location = new Point(220, 140), Width = 180, Font = new Font("Segoe UI", 9.5F), BorderStyle = BorderStyle.FixedSingle };

            pnlSupplier.Controls.Add(lblNewSupp); pnlSupplier.Controls.Add(txtNewSuppName);
            pnlSupplier.Controls.Add(lblNewContact); pnlSupplier.Controls.Add(txtNewSuppContact);

            Panel pnlMaterial = new Panel { Location = new Point(25, 325), Size = new Size(420, 260), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlMaterial.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlMaterial.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlMaterial);

            Label lblMaterialTitle = new Label { Text = "📦 Step 2: Raw Material Details & Action", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(13, 148, 136), Location = new Point(15, 12), AutoSize = true };
            pnlMaterial.Controls.Add(lblMaterialTitle);

            Label lblAutoID = new Label { Text = "💡 IDs (e.g. V001, RM001) will be auto-generated sequentially.", Location = new Point(15, 35), AutoSize = true, Font = new Font("Segoe UI", 8.5F, FontStyle.Italic), ForeColor = Color.Gray };
            pnlMaterial.Controls.Add(lblAutoID);

            int matY = 55;
            txtMatName = CreateInputField(pnlMaterial, ref matY, "Raw Material Name * (e.g., Timber Wood):");
            txtMatPrice = CreateInputField(pnlMaterial, ref matY, "Standard Unit Cost ($) *:");
            txtMatReorderLevel = CreateInputField(pnlMaterial, ref matY, "Safety Stock Alert Level:");
            txtMatReorderLevel.Text = "20";

            btnSave = new Button { Text = "💾 Process & Save Data", Location = new Point(220, 195), Size = new Size(180, 42), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += btnSave_Click;
            pnlMaterial.Controls.Add(btnSave);

            Panel pnlRight = new Panel { Location = new Point(465, 70), Size = new Size(590, 515), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlRight.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlRight.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlRight);

            Label lblGridTitle = new Label { Text = "🔍 Live Monitor: Suppliers & Provided Raw Materials", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(15, 12), AutoSize = true };
            pnlRight.Controls.Add(lblGridTitle);

            dgvSuppliers = new DataGridView
            {
                Location = new Point(15, 45),
                Size = new Size(560, 405),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                RowHeadersVisible = false,
                GridColor = Color.FromArgb(226, 232, 240),
                EnableHeadersVisualStyles = false
            };

            dgvSuppliers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgvSuppliers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvSuppliers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvSuppliers.ColumnHeadersHeight = 40;

            dgvSuppliers.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvSuppliers.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            dgvSuppliers.DefaultCellStyle.Padding = new Padding(5);
            dgvSuppliers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            pnlRight.Controls.Add(dgvSuppliers);

            btnUpdateSupplier = new Button
            {
                Text = "✏️ Update Selected Supplier",
                Location = new Point(15, 460),
                Size = new Size(220, 40),
                BackColor = Color.FromArgb(245, 158, 11),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnUpdateSupplier.FlatAppearance.BorderSize = 0;
            btnUpdateSupplier.Click += btnUpdateSupplier_Click;
            pnlRight.Controls.Add(btnUpdateSupplier);

            btnDeleteSupplier = new Button
            {
                Text = "🗑️ Deactivate Selected Supplier",
                Location = new Point(355, 460),
                Size = new Size(220, 40),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDeleteSupplier.FlatAppearance.BorderSize = 0;
            btnDeleteSupplier.Click += btnDeleteSupplier_Click;
            pnlRight.Controls.Add(btnDeleteSupplier);

            SupplierMode_CheckedChanged(null, null);
        }

        private TextBox CreateInputField(Panel container, ref int topY, string labelText)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(15, topY), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(15, topY + 20), Width = 180, Font = new Font("Segoe UI", 9.5F), BorderStyle = BorderStyle.FixedSingle };
            container.Controls.Add(lbl); container.Controls.Add(txt);
            topY += 55;
            return txt;
        }

        // 🌟 動態更新：當 Combobox 選擇改變時，自動將資料填入下方的唯讀 TextBox
        private void CmbExistingSuppliers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rbExistingSupplier.Checked && cmbExistingSuppliers.SelectedItem is SupplierComboItem item)
            {
                txtNewSuppName.Text = item.Name;
                txtNewSuppContact.Text = item.Contact;
            }
        }

        private void SupplierMode_CheckedChanged(object sender, EventArgs e)
        {
            if (rbExistingSupplier.Checked)
            {
                cmbExistingSuppliers.Enabled = true;
                txtNewSuppName.ReadOnly = true;     // 改為唯讀，防止修改
                txtNewSuppContact.ReadOnly = true;  // 改為唯讀，防止修改
                txtNewSuppName.BackColor = Color.FromArgb(241, 245, 249);
                txtNewSuppContact.BackColor = Color.FromArgb(241, 245, 249);

                // 強制觸發一次事件來刷新欄位
                CmbExistingSuppliers_SelectedIndexChanged(null, null);
            }
            else
            {
                cmbExistingSuppliers.Enabled = false;
                cmbExistingSuppliers.SelectedIndex = -1;
                txtNewSuppName.ReadOnly = false;
                txtNewSuppContact.ReadOnly = false;
                txtNewSuppName.BackColor = Color.White;
                txtNewSuppContact.BackColor = Color.White;
                txtNewSuppName.Clear();
                txtNewSuppContact.Clear();
            }
        }
        #endregion

        #region 💾 Database Core Processing Mechanics

        private void LoadExistingSuppliers()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    // 🌟 讀取時加入 ContactInfo 供前端顯示用
                    string query = "SELECT SupplierID, SupplierName, ContactInfo FROM supplier WHERE IsActive = 1 ORDER BY SupplierID ASC";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<SupplierComboItem> list = new List<SupplierComboItem>();

                        // 🌟 方案 A 核心：加入「不指定供應商」選項
                        list.Add(new SupplierComboItem
                        {
                            ID = "UNASSIGNED",
                            Name = "(Pending Procurement Sourcing)",
                            Contact = "-"
                        });

                        while (reader.Read())
                        {
                            list.Add(new SupplierComboItem
                            {
                                ID = reader["SupplierID"].ToString(),
                                Name = reader["SupplierName"].ToString(),
                                Contact = reader["ContactInfo"].ToString()
                            });
                        }
                        cmbExistingSuppliers.DataSource = list;
                        cmbExistingSuppliers.DisplayMember = "DisplayText"; // 綁定顯示屬性 (只顯示 ID)
                        cmbExistingSuppliers.ValueMember = "ID";
                        cmbExistingSuppliers.SelectedIndex = 0; // 預設選中 UNASSIGNED
                    }
                }
                catch (Exception ex) { MessageBox.Show("Failed to load existing suppliers: " + ex.Message); }
            }
        }

        private void RefreshSupplierGrid()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            s.SupplierID AS `ID`, 
                            s.SupplierName AS `Supplier Name`, 
                            IF(s.IsActive=1, 'Active', '🔴 Deactivated') AS `Status`,
                            s.ContactInfo AS `Contact Reference`,
                            IFNULL(GROUP_CONCAT(rm.MaterialName SEPARATOR ',\n'), '(No Materials Linked)') AS `Provided Materials`
                        FROM supplier s
                        LEFT JOIN supplier_material sm ON s.SupplierID = sm.SupplierID
                        LEFT JOIN raw_material rm ON sm.MaterialID = rm.MaterialID
                        GROUP BY s.SupplierID
                        ORDER BY s.IsActive DESC, s.SupplierID DESC";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvSuppliers.DataSource = dt;

                        if (dgvSuppliers.Columns.Count > 0)
                        {
                            dgvSuppliers.Columns["ID"].FillWeight = 10;
                            dgvSuppliers.Columns["Supplier Name"].FillWeight = 20;
                            dgvSuppliers.Columns["Status"].FillWeight = 15;
                            dgvSuppliers.Columns["Contact Reference"].FillWeight = 20;
                            dgvSuppliers.Columns["Provided Materials"].FillWeight = 35;
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Failed to monitor supplier database: " + ex.Message); }
            }
        }

        private void btnUpdateSupplier_Click(object sender, EventArgs e)
        {
            if (editingSupplierID == null)
            {
                if (dgvSuppliers.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a supplier row from the live monitor table first.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string targetID = dgvSuppliers.SelectedRows[0].Cells["ID"].Value.ToString();
                string oldName = dgvSuppliers.SelectedRows[0].Cells["Supplier Name"].Value.ToString();
                string oldContact = dgvSuppliers.SelectedRows[0].Cells["Contact Reference"].Value.ToString();

                editingSupplierID = targetID;
                rbNewSupplier.Checked = true;
                rbExistingSupplier.Enabled = false;
                rbNewSupplier.Enabled = false;
                btnSave.Enabled = false;

                txtNewSuppName.Text = oldName;
                txtNewSuppContact.Text = oldContact;
                txtNewSuppName.ReadOnly = false;
                txtNewSuppContact.ReadOnly = false;
                txtNewSuppName.BackColor = Color.White;
                txtNewSuppContact.BackColor = Color.White;

                btnUpdateSupplier.Text = "💾 Save Supplier Changes";
                btnUpdateSupplier.BackColor = Color.FromArgb(37, 99, 235);
                btnDeleteSupplier.Text = "❌ Cancel Editing";
                btnDeleteSupplier.BackColor = Color.FromArgb(100, 116, 139);

                txtNewSuppName.Focus();
                MessageBox.Show($"Supplier [{targetID}] details loaded!\n\nPlease modify the Name or Contact Info in Step 1, then click 'Save Supplier Changes' to write back.", "Data Loaded into Workspace", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                string updatedName = txtNewSuppName.Text.Trim();
                string updatedContact = txtNewSuppContact.Text.Trim();

                if (string.IsNullOrWhiteSpace(updatedName))
                {
                    MessageBox.Show("Supplier Name cannot be blank.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult confirm = MessageBox.Show(
                    $"Are you sure you want to update Supplier [{editingSupplierID}]?\n\n🔸 [New Name]: {updatedName}\n🔸 [New Contact]: {updatedContact}",
                    "Confirm Data Amendment",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (confirm == DialogResult.Yes)
                {
                    using (MySqlConnection conn = new MySqlConnection(connString))
                    {
                        try
                        {
                            conn.Open();
                            string updateSql = "UPDATE supplier SET SupplierName = @name, ContactInfo = @contact WHERE SupplierID = @id";
                            using (MySqlCommand cmd = new MySqlCommand(updateSql, conn))
                            {
                                cmd.Parameters.AddWithValue("@name", updatedName);
                                cmd.Parameters.AddWithValue("@contact", updatedContact);
                                cmd.Parameters.AddWithValue("@id", editingSupplierID);
                                cmd.ExecuteNonQuery();
                            }

                            MessageBox.Show($"Supplier [{editingSupplierID}] has been updated successfully!", "Update Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ResetFormEditContext();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to update database record: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void btnDeleteSupplier_Click(object sender, EventArgs e)
        {
            if (editingSupplierID != null)
            {
                ResetFormEditContext();
                return;
            }

            if (dgvSuppliers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a supplier from the view table.", "Operation Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedSuppID = dgvSuppliers.SelectedRows[0].Cells["ID"].Value.ToString();
            string selectedSuppName = dgvSuppliers.SelectedRows[0].Cells["Supplier Name"].Value.ToString();

            DialogResult result = MessageBox.Show(
                $"Are you sure you want to DEACTIVATE supplier:\n\n[{selectedSuppName}] (ID: {selectedSuppID})?\n\nHistorical records will be preserved for auditing, but it will be hidden from new procurement dropdowns.",
                "Supplier Status Change",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        string updateQuery = "UPDATE supplier SET IsActive = 0 WHERE SupplierID = @id";
                        using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", selectedSuppID);
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show($"Supplier [{selectedSuppName}] has been deactivated successfully.", "Status Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadExistingSuppliers();
                        RefreshSupplierGrid();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error updating supplier status:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ResetFormEditContext()
        {
            editingSupplierID = null;

            txtNewSuppName.Clear();
            txtNewSuppContact.Clear();

            rbExistingSupplier.Enabled = true;
            rbNewSupplier.Enabled = true;
            btnSave.Enabled = true;
            rbExistingSupplier.Checked = true;

            btnUpdateSupplier.Text = "✏️ Update Selected Supplier";
            btnUpdateSupplier.BackColor = Color.FromArgb(245, 158, 11);
            btnDeleteSupplier.Text = "🗑️ Deactivate Selected Supplier";
            btnDeleteSupplier.BackColor = Color.FromArgb(239, 68, 68);

            LoadExistingSuppliers();
            RefreshSupplierGrid();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (rbExistingSupplier.Checked && cmbExistingSuppliers.SelectedItem == null)
            {
                MessageBox.Show("Please select an existing supplier from the dropdown list.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string finalSuppName = txtNewSuppName.Text.Trim();
            if (rbNewSupplier.Checked && string.IsNullOrWhiteSpace(finalSuppName))
            {
                MessageBox.Show("Please enter the New Supplier's Company Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string matName = txtMatName.Text.Trim();
            string priceStr = txtMatPrice.Text.Trim();
            string reorderStr = txtMatReorderLevel.Text.Trim();

            if (string.IsNullOrWhiteSpace(matName) || string.IsNullOrWhiteSpace(priceStr))
            {
                MessageBox.Show("Material Name and Unit Cost are mandatory fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(priceStr, out decimal price) || price < 0)
            {
                MessageBox.Show("Please enter a valid positive number for Unit Cost.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int reorderLvl = 20;
            if (!string.IsNullOrWhiteSpace(reorderStr) && (!int.TryParse(reorderStr, out reorderLvl) || reorderLvl < 0))
            {
                MessageBox.Show("Safety Stock Alert Level must be a valid positive integer.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string finalSuppID = "";
            string finalMatID = "";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // 🌟 判斷是否為「不指定供應商」
                            bool isUnassigned = rbExistingSupplier.Checked && ((SupplierComboItem)cmbExistingSuppliers.SelectedItem).ID == "UNASSIGNED";

                            // 1. 如果有指定供應商，處理供應商邏輯
                            if (!isUnassigned)
                            {
                                if (rbExistingSupplier.Checked)
                                {
                                    finalSuppID = ((SupplierComboItem)cmbExistingSuppliers.SelectedItem).ID;
                                }
                                else
                                {
                                    string getSuppIdQuery = "SELECT SupplierID FROM supplier WHERE SupplierID LIKE 'V___' ORDER BY SupplierID DESC LIMIT 1";
                                    using (MySqlCommand cmdMaxSupp = new MySqlCommand(getSuppIdQuery, conn, trans))
                                    {
                                        object result = cmdMaxSupp.ExecuteScalar();
                                        if (result != null && result != DBNull.Value)
                                        {
                                            string maxId = result.ToString();
                                            if (maxId.Length == 4 && int.TryParse(maxId.Substring(1), out int currentInt))
                                                finalSuppID = "V" + (currentInt + 1).ToString("D3");
                                            else
                                                finalSuppID = "V001";
                                        }
                                        else { finalSuppID = "V001"; }
                                    }

                                    string insertSuppSql = "INSERT INTO supplier (SupplierID, SupplierName, ContactInfo) VALUES (@id, @name, @contact)";
                                    using (MySqlCommand cmdInsSupp = new MySqlCommand(insertSuppSql, conn, trans))
                                    {
                                        cmdInsSupp.Parameters.AddWithValue("@id", finalSuppID);
                                        cmdInsSupp.Parameters.AddWithValue("@name", finalSuppName);
                                        cmdInsSupp.Parameters.AddWithValue("@contact", txtNewSuppContact.Text.Trim());
                                        cmdInsSupp.ExecuteNonQuery();
                                    }
                                }
                            }

                            // 2. 必定生成原材料 ID 並寫入 raw_material (不受供應商影響)
                            string getMatIdQuery = "SELECT MaterialID FROM raw_material WHERE MaterialID LIKE 'RM___' ORDER BY MaterialID DESC LIMIT 1";
                            using (MySqlCommand cmdMaxMat = new MySqlCommand(getMatIdQuery, conn, trans))
                            {
                                object matResult = cmdMaxMat.ExecuteScalar();
                                if (matResult != null && matResult != DBNull.Value)
                                {
                                    string maxMatId = matResult.ToString();
                                    if (maxMatId.Length == 5 && int.TryParse(maxMatId.Substring(2), out int currentMatInt))
                                        finalMatID = "RM" + (currentMatInt + 1).ToString("D3");
                                    else
                                        finalMatID = "RM001";
                                }
                                else { finalMatID = "RM001"; }
                            }

                            string insertMatSql = "INSERT INTO raw_material (MaterialID, MaterialName, StandardCost, StockLevel, ReorderLevel) VALUES (@id, @name, @cost, 0, @reorder)";
                            using (MySqlCommand cmdInsMat = new MySqlCommand(insertMatSql, conn, trans))
                            {
                                cmdInsMat.Parameters.AddWithValue("@id", finalMatID);
                                cmdInsMat.Parameters.AddWithValue("@name", matName);
                                cmdInsMat.Parameters.AddWithValue("@cost", price);
                                cmdInsMat.Parameters.AddWithValue("@reorder", reorderLvl);
                                cmdInsMat.ExecuteNonQuery();
                            }

                            // 3. 只有當有指派供應商時，才寫入關聯表 supplier_material
                            if (!isUnassigned)
                            {
                                string insertLinkSql = "INSERT INTO supplier_material (SupplierID, MaterialID) VALUES (@suppID, @matID)";
                                using (MySqlCommand cmdLink = new MySqlCommand(insertLinkSql, conn, trans))
                                {
                                    cmdLink.Parameters.AddWithValue("@suppID", finalSuppID);
                                    cmdLink.Parameters.AddWithValue("@matID", finalMatID);
                                    cmdLink.ExecuteNonQuery();
                                }
                            }

                            trans.Commit();

                            // 🌟 依據不同情況顯示對應成功訊息 (方案 A 軟性提示)
                            string successMsg;
                            if (isUnassigned)
                            {
                                successMsg = $"Material [{matName}] (ID: {finalMatID}) created successfully!\n\n⚠️ Note: No supplier is linked yet. You can still use it in Product BOM, but please assign a supplier before procurement.";
                                MessageBox.Show(successMsg, "Created Without Supplier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else if (rbNewSupplier.Checked)
                            {
                                successMsg = $"Success! New Supplier [{finalSuppName}] (ID: {finalSuppID}) and Material [{matName}] (ID: {finalMatID}) registered.";
                                MessageBox.Show(successMsg, "Transaction Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                successMsg = $"Success! Material [{matName}] (ID: {finalMatID}) successfully linked to existing supplier ({finalSuppID}).";
                                MessageBox.Show(successMsg, "Transaction Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            LoadExistingSuppliers();
                            RefreshSupplierGrid();

                            txtMatName.Clear();
                            txtMatPrice.Clear();
                            txtMatReorderLevel.Text = "20";
                            if (rbNewSupplier.Checked)
                            {
                                txtNewSuppName.Clear();
                                txtNewSuppContact.Clear();
                            }
                        }
                        catch (Exception innerEx)
                        {
                            trans.Rollback();
                            throw innerEx;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Transaction Failed to Save:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion
    }
}