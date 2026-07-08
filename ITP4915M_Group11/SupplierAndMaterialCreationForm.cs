using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public class SupplierComboItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Address { get; set; }
        public string DisplayText => ID == "UNASSIGNED" ? "--- Unassigned ---" : ID;
    }

    public partial class SupplierAndMaterialCreationForm : BaseForm
    {
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";
        private string editingSupplierID = null;

        private RadioButton rbExistingSupplier, rbNewSupplier;
        private ComboBox cmbExistingSuppliers;
        private TextBox txtNewSuppName, txtNewSuppContact, txtNewSuppAddress;

        private RadioButton rbNewMaterial, rbExistingMaterial;
        private ComboBox cmbExistingMaterials;
        private Label lblMatName, lblMatPrice, lblMatReorder;
        private TextBox txtMatName, txtMatPrice, txtMatReorderLevel;
        private Button btnSave;
        private DataGridView dgvSuppliers;
        private Button btnDeleteSupplier;
        private Button btnUpdateSupplier;
        private TextBox txtSearch;

        public SupplierAndMaterialCreationForm()
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                EnsureSupplierAddressColumn();
                ThemeManager.ApplyTheme(this);
                SetupPremiumUI();
                LoadExistingSuppliers();
                RefreshSupplierGrid();
            }
        }

        private void EnsureSupplierAddressColumn()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    new MySqlCommand("ALTER TABLE supplier ADD COLUMN Address VARCHAR(255) DEFAULT '';", conn).ExecuteNonQuery();
                }
                catch { }
            }
        }

        private void SetupPremiumUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Material & Procurement Hub";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ThemeManager.PrimaryBackground;
            this.Font = ThemeManager.DefaultFont;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            Panel pnlMain = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(25) };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label { Text = "🏭 Material Procurement & Supplier Management Hub", Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(25, 15), AutoSize = true };
            pnlMain.Controls.Add(lblHeader);

            Panel pnlSupplier = new Panel { Location = new Point(25, 70), Size = new Size(420, 300), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlSupplier.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlSupplier.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlSupplier);

            Label lblSupplierTitle = new Label { Text = "🏢 Step 1: Supplier Assignment / Editor", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(15, 12), AutoSize = true };
            pnlSupplier.Controls.Add(lblSupplierTitle);

            rbExistingSupplier = new RadioButton { Text = "Link to Existing Supplier", Location = new Point(15, 42), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Checked = true, Cursor = Cursors.Hand };
            rbNewSupplier = new RadioButton { Text = "Register New Supplier", Location = new Point(15, 68), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            pnlSupplier.Controls.Add(rbExistingSupplier); pnlSupplier.Controls.Add(rbNewSupplier);

            rbExistingSupplier.CheckedChanged += SupplierMode_CheckedChanged;
            rbNewSupplier.CheckedChanged += SupplierMode_CheckedChanged;

            Label lblSelectSupp = new Label { Text = "Select Supplier ID:", Location = new Point(220, 42), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cmbExistingSuppliers = new ComboBox { Location = new Point(220, 62), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F) };
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

            Label lblNewAddress = new Label { Text = "Company Address:", Location = new Point(15, 180), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtNewSuppAddress = new TextBox { Location = new Point(15, 200), Width = 385, Height = 55, Multiline = true, Font = new Font("Segoe UI", 9.5F), BorderStyle = BorderStyle.FixedSingle };
            pnlSupplier.Controls.Add(lblNewAddress); pnlSupplier.Controls.Add(txtNewSuppAddress);

            Panel pnlMaterial = new Panel { Location = new Point(25, 385), Size = new Size(420, 310), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlMaterial.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlMaterial.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlMaterial);

            Label lblMaterialTitle = new Label { Text = "📦 Step 2: Raw Material Details & Action", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(13, 148, 136), Location = new Point(15, 12), AutoSize = true };
            pnlMaterial.Controls.Add(lblMaterialTitle);

            Label lblAutoID = new Label { Text = "💡 If you need to edit an existing material, please use the Material Manager.", Location = new Point(15, 35), AutoSize = true, Font = new Font("Segoe UI", 8.5F, FontStyle.Italic), ForeColor = Color.Gray };
            pnlMaterial.Controls.Add(lblAutoID);

            rbNewMaterial = new RadioButton { Text = "Create New Material", Location = new Point(15, 60), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Checked = true, Cursor = Cursors.Hand };
            rbExistingMaterial = new RadioButton { Text = "Link Existing Material", Location = new Point(200, 60), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            pnlMaterial.Controls.Add(rbNewMaterial); pnlMaterial.Controls.Add(rbExistingMaterial);

            rbNewMaterial.CheckedChanged += MaterialMode_CheckedChanged;
            rbExistingMaterial.CheckedChanged += MaterialMode_CheckedChanged;

            cmbExistingMaterials = new ComboBox { Location = new Point(15, 90), Width = 385, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F), Visible = false };
            pnlMaterial.Controls.Add(cmbExistingMaterials);

            lblMatName = new Label { Text = "Raw Material Name *:", Location = new Point(15, 90), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtMatName = new TextBox { Location = new Point(15, 110), Width = 385, Font = new Font("Segoe UI", 9.5F), BorderStyle = BorderStyle.FixedSingle };
            pnlMaterial.Controls.Add(lblMatName); pnlMaterial.Controls.Add(txtMatName);

            lblMatPrice = new Label { Text = "Standard Unit Cost ($) *:", Location = new Point(15, 150), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtMatPrice = new TextBox { Location = new Point(15, 170), Width = 180, Font = new Font("Segoe UI", 9.5F), BorderStyle = BorderStyle.FixedSingle };
            pnlMaterial.Controls.Add(lblMatPrice); pnlMaterial.Controls.Add(txtMatPrice);

            lblMatReorder = new Label { Text = "Safety Stock Alert Level:", Location = new Point(220, 150), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtMatReorderLevel = new TextBox { Location = new Point(220, 170), Width = 180, Font = new Font("Segoe UI", 9.5F), BorderStyle = BorderStyle.FixedSingle };
            txtMatReorderLevel.Text = "20";
            pnlMaterial.Controls.Add(lblMatReorder); pnlMaterial.Controls.Add(txtMatReorderLevel);

            btnSave = new Button { Text = "💾 Process & Save Data", Location = new Point(220, 240), Size = new Size(180, 42), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += btnSave_Click;
            pnlMaterial.Controls.Add(btnSave);

            Panel pnlRight = new Panel { Location = new Point(465, 70), Size = new Size(590, 625), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlRight.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlRight.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlRight);

            Label lblGridTitle = new Label { Text = "🔍 Monitor: Suppliers & Materials", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(15, 12), AutoSize = true };
            pnlRight.Controls.Add(lblGridTitle);

            Label lblSearch = new Label { Text = "Search:", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(310, 15), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(370, 12), Width = 205, Font = new Font("Segoe UI", 9.5F), BorderStyle = BorderStyle.FixedSingle };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            pnlRight.Controls.Add(lblSearch); pnlRight.Controls.Add(txtSearch);

            dgvSuppliers = new DataGridView
            {
                Location = new Point(15, 45),
                Size = new Size(560, 500),
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

            btnUpdateSupplier = new Button { Text = "✏️ Update Selected Supplier", Location = new Point(15, 560), Size = new Size(220, 40), BackColor = Color.FromArgb(245, 158, 11), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnUpdateSupplier.FlatAppearance.BorderSize = 0;
            btnUpdateSupplier.Click += btnUpdateSupplier_Click;
            pnlRight.Controls.Add(btnUpdateSupplier);

            btnDeleteSupplier = new Button { Text = "🗑️ Deactivate Selected Supplier", Location = new Point(355, 560), Size = new Size(220, 40), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnDeleteSupplier.FlatAppearance.BorderSize = 0;
            btnDeleteSupplier.Click += btnDeleteSupplier_Click;
            pnlRight.Controls.Add(btnDeleteSupplier);

            SupplierMode_CheckedChanged(null, null);
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (dgvSuppliers.DataSource is DataTable dt)
            {
                string keyword = txtSearch.Text.Trim().Replace("'", "''");
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    dt.DefaultView.RowFilter = "";
                }
                else
                {
                    dt.DefaultView.RowFilter = $"[ID] LIKE '%{keyword}%' OR [Supplier Name] LIKE '%{keyword}%' OR [Contact Reference] LIKE '%{keyword}%' OR [Company Address] LIKE '%{keyword}%' OR [Provided Materials] LIKE '%{keyword}%'";
                }
            }
        }

        private void CmbExistingSuppliers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rbExistingSupplier.Checked && cmbExistingSuppliers.SelectedItem is SupplierComboItem item)
            {
                txtNewSuppName.Text = item.Name;
                txtNewSuppContact.Text = item.Contact;
                txtNewSuppAddress.Text = item.Address;
            }
        }

        private void SupplierMode_CheckedChanged(object sender, EventArgs e)
        {
            if (rbExistingSupplier.Checked)
            {
                cmbExistingSuppliers.Enabled = true;
                txtNewSuppName.ReadOnly = true;
                txtNewSuppContact.ReadOnly = true;
                txtNewSuppAddress.ReadOnly = true;
                txtNewSuppName.BackColor = Color.FromArgb(241, 245, 249);
                txtNewSuppContact.BackColor = Color.FromArgb(241, 245, 249);
                txtNewSuppAddress.BackColor = Color.FromArgb(241, 245, 249);
                CmbExistingSuppliers_SelectedIndexChanged(null, null);
            }
            else
            {
                cmbExistingSuppliers.Enabled = false;
                cmbExistingSuppliers.SelectedIndex = -1;
                txtNewSuppName.ReadOnly = false;
                txtNewSuppContact.ReadOnly = false;
                txtNewSuppAddress.ReadOnly = false;
                txtNewSuppName.BackColor = Color.White;
                txtNewSuppContact.BackColor = Color.White;
                txtNewSuppAddress.BackColor = Color.White;
                txtNewSuppName.Clear();
                txtNewSuppContact.Clear();
                txtNewSuppAddress.Clear();
            }
        }

        private void MaterialMode_CheckedChanged(object sender, EventArgs e)
        {
            bool isNew = rbNewMaterial.Checked;
            lblMatName.Visible = isNew;
            txtMatName.Visible = isNew;
            lblMatPrice.Visible = isNew;
            txtMatPrice.Visible = isNew;
            lblMatReorder.Visible = isNew;
            txtMatReorderLevel.Visible = isNew;
            cmbExistingMaterials.Visible = !isNew;

            if (!isNew)
            {
                LoadExistingMaterials();
                btnSave.Text = "🔗 Link Material to Supplier";
                btnSave.BackColor = Color.FromArgb(37, 99, 235);
            }
            else
            {
                btnSave.Text = "💾 Process & Save Data";
                btnSave.BackColor = Color.FromArgb(16, 185, 129);
                txtMatName.Clear();
                txtMatPrice.Clear();
                txtMatReorderLevel.Text = "20";
            }
        }

        private void LoadExistingSuppliers()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT SupplierID, SupplierName, ContactInfo, Address FROM supplier WHERE IsActive=1 ORDER BY SupplierID ASC";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<SupplierComboItem> list = new List<SupplierComboItem>();
                        list.Add(new SupplierComboItem { ID = "UNASSIGNED", Name = "(Pending Procurement Sourcing)", Contact = "-", Address = "-" });
                        while (reader.Read())
                        {
                            list.Add(new SupplierComboItem
                            {
                                ID = reader["SupplierID"].ToString(),
                                Name = reader["SupplierName"].ToString(),
                                Contact = reader["ContactInfo"].ToString(),
                                Address = reader["Address"]?.ToString() ?? ""
                            });
                        }
                        cmbExistingSuppliers.DataSource = list;
                        cmbExistingSuppliers.DisplayMember = "DisplayText";
                        cmbExistingSuppliers.ValueMember = "ID";
                        cmbExistingSuppliers.SelectedIndex = 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load existing suppliers: " + ex.Message);
                }
            }
        }

        private void LoadExistingMaterials()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MaterialID, MaterialName FROM raw_material ORDER BY MaterialID ASC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dt.Columns.Add("Display", typeof(string), "MaterialID + ' - ' + MaterialName");
                        cmbExistingMaterials.DataSource = dt;
                        cmbExistingMaterials.DisplayMember = "Display";
                        cmbExistingMaterials.ValueMember = "MaterialID";
                    }
                }
                catch { }
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
                        SELECT s.SupplierID AS `ID`,
                               s.SupplierName AS `Supplier Name`,
                               IF(s.IsActive=1,'Active','🔴 Deactivated') AS `Status`,
                               s.ContactInfo AS `Contact Reference`,
                               s.Address AS `Company Address`,
                               IFNULL(GROUP_CONCAT(CONCAT(rm.MaterialName,' (',rm.MaterialID,')') SEPARATOR ',\n'),'(No Materials Linked)') AS `Provided Materials` 
                        FROM supplier s 
                        LEFT JOIN supplier_material sm ON s.SupplierID=sm.SupplierID 
                        LEFT JOIN raw_material rm ON sm.MaterialID=rm.MaterialID 
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
                            dgvSuppliers.Columns["Supplier Name"].FillWeight = 15;
                            dgvSuppliers.Columns["Status"].FillWeight = 10;
                            dgvSuppliers.Columns["Contact Reference"].FillWeight = 15;
                            dgvSuppliers.Columns["Company Address"].FillWeight = 20;
                            dgvSuppliers.Columns["Provided Materials"].FillWeight = 30;
                        }

                        if (txtSearch != null && !string.IsNullOrWhiteSpace(txtSearch.Text))
                        {
                            TxtSearch_TextChanged(null, null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to monitor supplier database: " + ex.Message);
                }
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
                string oldAddress = dgvSuppliers.SelectedRows[0].Cells["Company Address"].Value?.ToString() ?? "";

                editingSupplierID = targetID;
                rbNewSupplier.Checked = true;
                rbExistingSupplier.Enabled = false;
                rbNewSupplier.Enabled = false;
                pnlMaterial_SetEnabled(false);

                txtNewSuppName.Text = oldName;
                txtNewSuppContact.Text = oldContact;
                txtNewSuppAddress.Text = oldAddress;

                txtNewSuppName.ReadOnly = false;
                txtNewSuppContact.ReadOnly = false;
                txtNewSuppAddress.ReadOnly = false;
                txtNewSuppName.BackColor = Color.White;
                txtNewSuppContact.BackColor = Color.White;
                txtNewSuppAddress.BackColor = Color.White;

                btnUpdateSupplier.Text = "💾 Save Supplier Changes";
                btnUpdateSupplier.BackColor = Color.FromArgb(37, 99, 235);
                btnDeleteSupplier.Text = "❌ Cancel Editing";
                btnDeleteSupplier.BackColor = Color.FromArgb(100, 116, 139);

                txtNewSuppName.Focus();
            }
            else
            {
                string updatedName = txtNewSuppName.Text.Trim();
                string updatedContact = txtNewSuppContact.Text.Trim();
                string updatedAddress = txtNewSuppAddress.Text.Trim();

                if (string.IsNullOrWhiteSpace(updatedName))
                {
                    MessageBox.Show("Supplier Name cannot be blank.");
                    return;
                }

                DialogResult confirm = MessageBox.Show($"Are you sure you want to update Supplier [{editingSupplierID}]?\n\n🔸 [New Name]: {updatedName}\n🔸 [New Contact]: {updatedContact}\n🔸 [New Address]: {updatedAddress}", "Confirm Data Amendment", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    using (MySqlConnection conn = new MySqlConnection(connString))
                    {
                        try
                        {
                            conn.Open();
                            string updateSql = "UPDATE supplier SET SupplierName=@name, ContactInfo=@contact, Address=@address WHERE SupplierID=@id";
                            using (MySqlCommand cmd = new MySqlCommand(updateSql, conn))
                            {
                                cmd.Parameters.AddWithValue("@name", updatedName);
                                cmd.Parameters.AddWithValue("@contact", updatedContact);
                                cmd.Parameters.AddWithValue("@address", updatedAddress);
                                cmd.Parameters.AddWithValue("@id", editingSupplierID);
                                cmd.ExecuteNonQuery();
                            }
                            MessageBox.Show($"Supplier [{editingSupplierID}] has been updated successfully!");
                            ResetFormEditContext();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to update database record: " + ex.Message);
                        }
                    }
                }
            }
        }

        private void pnlMaterial_SetEnabled(bool enabled)
        {
            rbNewMaterial.Enabled = enabled;
            rbExistingMaterial.Enabled = enabled;
            cmbExistingMaterials.Enabled = enabled;
            txtMatName.Enabled = enabled;
            txtMatPrice.Enabled = enabled;
            txtMatReorderLevel.Enabled = enabled;
            btnSave.Enabled = enabled;
        }

        // 🌟🌟🌟 升級版 Deactivate Supplier (自動檢查並刪除未使用之 Raw Material)
        private void btnDeleteSupplier_Click(object sender, EventArgs e)
        {
            if (editingSupplierID != null)
            {
                ResetFormEditContext();
                return;
            }

            if (dgvSuppliers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a supplier from the view table.");
                return;
            }

            string selectedSuppID = dgvSuppliers.SelectedRows[0].Cells["ID"].Value.ToString();
            string selectedSuppName = dgvSuppliers.SelectedRows[0].Cells["Supplier Name"].Value.ToString();

            DialogResult result = MessageBox.Show($"Are you sure you want to DEACTIVATE supplier:\n\n[{selectedSuppName}] (ID: {selectedSuppID})?\n\nSystem will also automatically purge any linked Raw Materials that have NEVER been used in any orders.", "Supplier Status Change", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlTransaction trans = conn.BeginTransaction())
                        {
                            try
                            {
                                // 1️⃣ 停用 Supplier
                                string updateQuery = "UPDATE supplier SET IsActive=0 WHERE SupplierID=@id";
                                using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@id", selectedSuppID);
                                    cmd.ExecuteNonQuery();
                                }

                                // 2️⃣ 搵出呢個供應商提供嘅所有原材料
                                List<string> linkedMaterials = new List<string>();
                                string getMatsQuery = "SELECT MaterialID FROM supplier_material WHERE SupplierID = @id";
                                using (MySqlCommand cmdGetMats = new MySqlCommand(getMatsQuery, conn, trans))
                                {
                                    cmdGetMats.Parameters.AddWithValue("@id", selectedSuppID);
                                    using (MySqlDataReader reader = cmdGetMats.ExecuteReader())
                                    {
                                        while (reader.Read()) linkedMaterials.Add(reader["MaterialID"].ToString());
                                    }
                                }

                                // 3️⃣ 檢查邊啲 Material 從來冇出現過喺訂單入面
                                int deletedMatsCount = 0;
                                foreach (string matID in linkedMaterials)
                                {
                                    bool isUsedInOrders = false;

                                    // (根據你個 DB 結構，可能有 order_lineitem / bill_of_materials 等表，我會用 bill_of_materials 檢查是否被產品依賴)
                                    string checkUsageQuery = "SELECT COUNT(*) FROM bill_of_materials WHERE MaterialID = @matID";
                                    using (MySqlCommand cmdCheck = new MySqlCommand(checkUsageQuery, conn, trans))
                                    {
                                        cmdCheck.Parameters.AddWithValue("@matID", matID);
                                        if (Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0)
                                        {
                                            isUsedInOrders = true;
                                        }
                                    }

                                    // 4️⃣ 如果完全冇用過，直接從 raw_material 刪除 (supplier_material 的 ON DELETE CASCADE 會自動清除關聯)
                                    if (!isUsedInOrders)
                                    {
                                        string deleteMatQuery = "DELETE FROM raw_material WHERE MaterialID = @matID";
                                        using (MySqlCommand cmdDel = new MySqlCommand(deleteMatQuery, conn, trans))
                                        {
                                            cmdDel.Parameters.AddWithValue("@matID", matID);
                                            cmdDel.ExecuteNonQuery();
                                            deletedMatsCount++;
                                        }
                                    }
                                }

                                trans.Commit();

                                string successMsg = $"Supplier [{selectedSuppName}] has been deactivated successfully.";
                                if (deletedMatsCount > 0)
                                {
                                    successMsg += $"\n\n🗑️ Purged {deletedMatsCount} unused Raw Material(s) associated with this supplier.";
                                }
                                MessageBox.Show(successMsg, "Deactivation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                LoadExistingSuppliers();
                                RefreshSupplierGrid();
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
                        MessageBox.Show("Error updating supplier status:\n" + ex.Message);
                    }
                }
            }
        }

        private void ResetFormEditContext()
        {
            editingSupplierID = null;
            txtNewSuppName.Clear();
            txtNewSuppContact.Clear();
            txtNewSuppAddress.Clear();
            rbExistingSupplier.Enabled = true;
            rbNewSupplier.Enabled = true;
            pnlMaterial_SetEnabled(true);

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
                MessageBox.Show("Please select an existing supplier.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string finalSuppName = txtNewSuppName.Text.Trim();
            if (rbNewSupplier.Checked && string.IsNullOrWhiteSpace(finalSuppName))
            {
                MessageBox.Show("Please enter the New Supplier's Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isUnassigned = rbExistingSupplier.Checked && ((SupplierComboItem)cmbExistingSuppliers.SelectedItem).ID == "UNASSIGNED";

            if (rbExistingMaterial.Checked && cmbExistingMaterials.SelectedValue == null)
            {
                MessageBox.Show("Please select an existing material to link.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (rbExistingMaterial.Checked && isUnassigned)
            {
                MessageBox.Show("You cannot link a material to 'UNASSIGNED'. Please select a valid supplier.", "Logical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string matNameStr = txtMatName.Text.Trim();
            string priceStr = txtMatPrice.Text.Trim();
            string reorderStr = txtMatReorderLevel.Text.Trim();

            if (rbNewMaterial.Checked)
            {
                if (string.IsNullOrWhiteSpace(matNameStr))
                {
                    MessageBox.Show("Material Name is mandatory.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(priceStr))
                {
                    MessageBox.Show("Unit Cost is a mandatory field.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!decimal.TryParse(priceStr, out decimal dummyPrice) || dummyPrice < 0)
                {
                    MessageBox.Show("Please enter a valid positive number for Unit Cost.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!string.IsNullOrWhiteSpace(reorderStr) && (!int.TryParse(reorderStr, out int dummyReorder) || dummyReorder < 0))
                {
                    MessageBox.Show("Safety Stock must be a positive integer.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
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
                            // 1️⃣ 處理 Supplier (如果是新增)
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
                                            else finalSuppID = "V001";
                                        }
                                        else finalSuppID = "V001";
                                    }

                                    string insertSuppSql = "INSERT INTO supplier (SupplierID, SupplierName, ContactInfo, Address) VALUES (@id, @name, @contact, @address)";
                                    using (MySqlCommand cmdInsSupp = new MySqlCommand(insertSuppSql, conn, trans))
                                    {
                                        cmdInsSupp.Parameters.AddWithValue("@id", finalSuppID);
                                        cmdInsSupp.Parameters.AddWithValue("@name", finalSuppName);
                                        cmdInsSupp.Parameters.AddWithValue("@contact", txtNewSuppContact.Text.Trim());
                                        cmdInsSupp.Parameters.AddWithValue("@address", txtNewSuppAddress.Text.Trim());
                                        cmdInsSupp.ExecuteNonQuery();
                                    }
                                }
                            }

                            // 2️⃣ 處理 Material
                            if (rbExistingMaterial.Checked)
                            {
                                finalMatID = cmbExistingMaterials.SelectedValue.ToString();
                                string checkLinkSql = "SELECT COUNT(*) FROM supplier_material WHERE SupplierID=@s AND MaterialID=@m";
                                using (MySqlCommand cmdCheck = new MySqlCommand(checkLinkSql, conn, trans))
                                {
                                    cmdCheck.Parameters.AddWithValue("@s", finalSuppID);
                                    cmdCheck.Parameters.AddWithValue("@m", finalMatID);
                                    if (Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0)
                                        throw new Exception($"Material [{finalMatID}] is already linked to Supplier [{finalSuppID}].");
                                }
                            }
                            else
                            {
                                string checkNameSql = "SELECT MaterialID FROM raw_material WHERE LOWER(MaterialName)=LOWER(@name) LIMIT 1";
                                using (MySqlCommand cmdCheckName = new MySqlCommand(checkNameSql, conn, trans))
                                {
                                    cmdCheckName.Parameters.AddWithValue("@name", matNameStr);
                                    object res = cmdCheckName.ExecuteScalar();
                                    if (res != null)
                                        throw new Exception($"Material Name '{matNameStr}' already exists (ID: {res.ToString()}).\n\nPlease use 'Link Existing Material' instead.");
                                }

                                string getMatIdQuery = "SELECT MaterialID FROM raw_material WHERE MaterialID LIKE 'RM___' ORDER BY MaterialID DESC LIMIT 1";
                                using (MySqlCommand cmdMaxMat = new MySqlCommand(getMatIdQuery, conn, trans))
                                {
                                    object matResult = cmdMaxMat.ExecuteScalar();
                                    if (matResult != null && matResult != DBNull.Value)
                                    {
                                        string maxMatId = matResult.ToString();
                                        if (maxMatId.Length == 5 && int.TryParse(maxMatId.Substring(2), out int currentMatInt))
                                            finalMatID = "RM" + (currentMatInt + 1).ToString("D3");
                                        else finalMatID = "RM001";
                                    }
                                    else finalMatID = "RM001";
                                }

                                decimal finalPrice = decimal.Parse(priceStr);
                                int finalReorder = string.IsNullOrWhiteSpace(reorderStr) ? 20 : int.Parse(reorderStr);

                                string insertMatSql = "INSERT INTO raw_material (MaterialID, MaterialName, StandardCost, StockLevel, ReorderLevel) VALUES (@id, @name, @cost, 0, @reorder)";
                                using (MySqlCommand cmdInsMat = new MySqlCommand(insertMatSql, conn, trans))
                                {
                                    cmdInsMat.Parameters.AddWithValue("@id", finalMatID);
                                    cmdInsMat.Parameters.AddWithValue("@name", matNameStr);
                                    cmdInsMat.Parameters.AddWithValue("@cost", finalPrice);
                                    cmdInsMat.Parameters.AddWithValue("@reorder", finalReorder);
                                    cmdInsMat.ExecuteNonQuery();
                                }
                            }

                            // 3️⃣ 連結 Supplier 與 Material
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

                            if (rbExistingMaterial.Checked)
                                MessageBox.Show($"Success! Existing Material [{finalMatID}] has been linked to Supplier [{finalSuppID}].", "Link Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            else if (isUnassigned)
                                MessageBox.Show($"Material (ID: {finalMatID}) created successfully!\n\n⚠️ Note: No supplier is linked yet.", "Created Without Supplier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            else if (rbNewSupplier.Checked)
                                MessageBox.Show($"Success! New Supplier [{finalSuppName}] and Material [{finalMatID}] registered.", "Transaction Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            else
                                MessageBox.Show($"Success! Material [{finalMatID}] successfully linked to existing supplier ({finalSuppID}).", "Transaction Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LoadExistingSuppliers();
                            RefreshSupplierGrid();

                            txtMatName.Clear();
                            txtMatPrice.Clear();
                            txtMatReorderLevel.Text = "20";
                            if (rbNewSupplier.Checked)
                            {
                                txtNewSuppName.Clear();
                                txtNewSuppContact.Clear();
                                txtNewSuppAddress.Clear();
                            }
                            if (rbExistingMaterial.Checked) LoadExistingMaterials();
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
                    MessageBox.Show("Transaction Failed:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}