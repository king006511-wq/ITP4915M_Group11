using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class CustomerManagement : Form
    {
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";
        
        private TextBox txtCustomerID, txtName, txtPhone, txtAddress, txtSearch;
        private ComboBox cboType;
        private DataGridView dgvCustomers;

        public CustomerManagement()
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                InitializeUI();
                LoadData();
            }
        }

        private void InitializeUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living - Customer Master Data";
            this.Size = new Size(1180, 750);
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Font = new Font("Segoe UI", 10F);

            AuthorizationHelper.EnforceRole(this, "Manager", "Administrator", "Sales Representative", "Sales");

            Label lblHeader = new Label { Text = "👥 Customer Master Data Maintenance", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(30, 20), AutoSize = true };
            this.Controls.Add(lblHeader);

            // Left Input Panel
            Panel pnlCard = new Panel { Location = new Point(30, 85), Size = new Size(380, 600), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(pnlCard);

            int startY = 20;
            txtCustomerID = CreateInput(pnlCard, ref startY, "Customer ID *:");
            txtName = CreateInput(pnlCard, ref startY, "Customer Name *:");
            
            Label lblType = new Label { Text = "Customer Type:", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            cboType = new ComboBox { Location = new Point(20, startY + 22), Width = 335, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F) };
            cboType.Items.AddRange(new string[] { "B2B", "B2C" });
            pnlCard.Controls.Add(lblType); pnlCard.Controls.Add(cboType);
            startY += 65;

            txtPhone = CreateInput(pnlCard, ref startY, "Phone Number:");
            txtAddress = CreateInput(pnlCard, ref startY, "Delivery Address:");

            Button btnAdd = new Button { Text = "➕ Add", Location = new Point(20, startY), Size = new Size(160, 40), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            Button btnUpdate = new Button { Text = "💾 Update", Location = new Point(195, startY), Size = new Size(160, 40), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            Button btnDelete = new Button { Text = "🗑️ Delete", Location = new Point(20, startY + 50), Size = new Size(160, 40), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            Button btnClear = new Button { Text = "🧹 Clear", Location = new Point(195, startY + 50), Size = new Size(160, 40), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            
            btnAdd.Click += BtnAdd_Click; btnUpdate.Click += BtnUpdate_Click; btnDelete.Click += BtnDelete_Click; btnClear.Click += (s, e) => ClearForm();
            pnlCard.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });

            // Right Grid Panel
            txtSearch = new TextBox { Location = new Point(440, 85), Width = 300, Font = new Font("Segoe UI", 11F), Text = "Search by ID or Name..." };
            txtSearch.GotFocus += (s, e) => { if (txtSearch.Text == "Search by ID or Name...") txtSearch.Text = ""; };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            this.Controls.Add(txtSearch);

            dgvCustomers = new DataGridView { Location = new Point(440, 125), Size = new Size(700, 560), BackgroundColor = Color.White, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvCustomers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgvCustomers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCustomers.SelectionChanged += DgvCustomers_SelectionChanged;
            this.Controls.Add(dgvCustomers);
        }

        private TextBox CreateInput(Panel container, ref int y, string label)
        {
            Label lbl = new Label { Text = label, Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(20, y + 22), Width = 335, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            container.Controls.Add(lbl); container.Controls.Add(txt);
            y += 65; return txt;
        }

        private void LoadData()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlDataAdapter da = new MySqlDataAdapter("SELECT CustomerID, Name, Type, Phone, Address FROM customer", conn))
                    {
                        DataTable dt = new DataTable(); da.Fill(dt);
                        dgvCustomers.DataSource = dt;
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error loading data: " + ex.Message); }
            }
        }

        private void DgvCustomers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count > 0)
            {
                DataGridViewRow r = dgvCustomers.SelectedRows[0];
                txtCustomerID.Text = r.Cells["CustomerID"].Value.ToString();
                txtName.Text = r.Cells["Name"].Value.ToString();
                cboType.Text = r.Cells["Type"].Value.ToString();
                txtPhone.Text = r.Cells["Phone"].Value.ToString();
                txtAddress.Text = r.Cells["Address"].Value.ToString();
                txtCustomerID.ReadOnly = true;
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (dgvCustomers.DataSource is DataTable dt && txtSearch.Text != "Search by ID or Name...")
            {
                string kw = txtSearch.Text.Trim().Replace("'", "''");
                dt.DefaultView.RowFilter = $"CustomerID LIKE '%{kw}%' OR Name LIKE '%{kw}%'";
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCustomerID.Text) || string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("ID and Name are required!"); return; }
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO customer (CustomerID, Name, Type, Phone, Address) VALUES (@id, @n, @t, @p, @a)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtCustomerID.Text.Trim()); cmd.Parameters.AddWithValue("@n", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@t", cboType.Text); cmd.Parameters.AddWithValue("@p", txtPhone.Text.Trim()); cmd.Parameters.AddWithValue("@a", txtAddress.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Customer added."); LoadData(); ClearForm();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCustomerID.Text)) return;
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string sql = "UPDATE customer SET Name=@n, Type=@t, Phone=@p, Address=@a WHERE CustomerID=@id";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtCustomerID.Text.Trim()); cmd.Parameters.AddWithValue("@n", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@t", cboType.Text); cmd.Parameters.AddWithValue("@p", txtPhone.Text.Trim()); cmd.Parameters.AddWithValue("@a", txtAddress.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Customer updated."); LoadData();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCustomerID.Text)) return;
            if (MessageBox.Show("Delete Customer?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("DELETE FROM customer WHERE CustomerID=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", txtCustomerID.Text.Trim());
                            cmd.ExecuteNonQuery();
                        }
                        LoadData(); ClearForm();
                    }
                    catch (Exception ex) { MessageBox.Show("Cannot delete customer. They might have active orders."); }
                }
            }
        }

        private void ClearForm()
        {
            txtCustomerID.Clear(); txtName.Clear(); txtPhone.Clear(); txtAddress.Clear(); cboType.SelectedIndex = -1;
            txtCustomerID.ReadOnly = false; dgvCustomers.ClearSelection();
        }
    }
}