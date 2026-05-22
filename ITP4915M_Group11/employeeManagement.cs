using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ITP4915M_Group11
{
    public partial class EmployeeManagement : Form
    {
        // Centralized connection string
        private string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        public EmployeeManagement()
        {
            InitializeComponent();
        }

        // Runs when the form loads
        private void Form3_Load(object sender, EventArgs e)
        {
            if (cboRole.Items.Count > 0)
            {
                cboRole.SelectedIndex = 0;
            }
            LoadStaffData();
        }

        // Load staff data
        private void LoadStaffData()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT StaffID AS 'StaffID', Name AS 'Name', Password AS 'Password', Role AS 'Role' FROM staff";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dgvStaff.DataSource = dt;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load staff data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Add new staff
        private void btnAddStaff_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStaffID.Text) || string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please fill out all fields!", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = "INSERT INTO staff (StaffID, Name, Password, Role) VALUES (@id, @name, @pass, @role)";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtStaffID.Text.Trim());
                        cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@pass", txtPassword.Text);
                        cmd.Parameters.AddWithValue("@role", cboRole.Text);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Successfully added staff!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            LoadStaffData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Add failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Update staff
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStaffID.Text) || string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please select a staff from the table and fill in the data first!", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = "UPDATE staff SET Name = @name, Password = @pass, Role = @role WHERE StaffID = @id";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtStaffID.Text.Trim());
                        cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@pass", txtPassword.Text);
                        cmd.Parameters.AddWithValue("@role", cboRole.Text);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Staff data updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            LoadStaffData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Update failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Delete staff
        private void btnDelete_Click(object sender, EventArgs e)
        {
            // 1. Guard clause: ensure a staff is selected
            if (string.IsNullOrWhiteSpace(txtStaffID.Text))
            {
                MessageBox.Show("Please select the staff you want to delete from the table!", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Show double-confirmation dialog
            DialogResult result = MessageBox.Show(
                $"Are you sure you want to permanently delete staff [{txtName.Text}] (ID: {txtStaffID.Text})?\nThis action cannot be undone!",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            // If the user selects "Yes", the deletion will actually be executed in the Database
            if (result == DialogResult.Yes)
            {
                // 3. Write DELETE SQL statement
                string query = "DELETE FROM staff WHERE StaffID = @id";

                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", txtStaffID.Text.Trim());

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Staff deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearFields();   // Clear input fields
                                LoadStaffData(); // Refresh table
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Delete failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Populate fields when clicking a row in the DataGridView
        private void dgvStaff_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvStaff.Rows[e.RowIndex];
                txtStaffID.Text = row.Cells["StaffID"].Value.ToString();
                txtName.Text = row.Cells["Name"].Value.ToString();
                txtPassword.Text = row.Cells["Password"].Value.ToString();
                cboRole.Text = row.Cells["Role"].Value.ToString();

                txtStaffID.ReadOnly = true; // Make StaffID read-only when editing/deleting
            }
        }

        // Clear input fields
        private void ClearFields()
        {
            txtStaffID.Clear();
            txtName.Clear();
            txtPassword.Clear();
            cboRole.SelectedIndex = 0;
            txtStaffID.ReadOnly = false;
        }
    }
}
