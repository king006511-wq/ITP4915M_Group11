using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;

// This is the project namespace (ITP4915M_Group11)
namespace ITP4915M_Group11
{
    // This partial class contains the Login form logic
    public partial class Login : Form
    {
        // Constructor for Login form (do not remove)
        public Login()
        {
            InitializeComponent();
        }

        // Login button logic
        private void btnLogin_Click(object sender, EventArgs e)
        {
            // Ensure input fields are not empty
            if (string.IsNullOrEmpty(txtUser.Text) || string.IsNullOrEmpty(txtPass.Text))
            {
                MessageBox.Show("Please enter Staff ID and password!", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Use the connection string that you successfully tested earlier
            string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

            // Use 'using' to ensure the connection is automatically closed after use, preventing resource locking
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    // SQL uses actual column names (StaffID and Password)
                    // SELECT Name, Role retrieves the staff name and role on successful login
                    string query = "SELECT Name, Role FROM staff WHERE StaffID = @user AND Password = @pass";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // Use secure parameters (Parameters) to prevent SQL Injection attacks; your instructor will definitely give extra credit
                        cmd.Parameters.AddWithValue("@user", txtUser.Text.Trim());
                        cmd.Parameters.AddWithValue("@pass", txtPass.Text.Trim());

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Successfully retrieved data, meaning the Staff ID and password are completely correct!
                                string staffName = reader["Name"].ToString();
                                string staffRole = reader["Role"].ToString();

                                MessageBox.Show($"Welcome back, {staffName}!\nYour role: {staffRole}",
                                                "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // --- Key modification: pass staffName and staffRole into the constructor! ---
                                MainMenu mainMenu = new MainMenu(staffName, staffRole);
                                mainMenu.Show();
                                this.Hide();
                            }
                            else
                            {
                                // No matching data found, meaning the input is incorrect
                                MessageBox.Show("Incorrect Staff ID or password, please try again!", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("System connection error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void txtUser_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
