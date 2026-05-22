using System;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class MainMenu : Form
    {
        private string currentStaffName;
        private string currentStaffRole;

        public MainMenu(string name, string role)
        {
            InitializeComponent();

        // Receive data
            currentStaffName = name;
            currentStaffRole = role;

            // 1. Set welcome message
            lblWelcome.Text = $"Welcome back, {currentStaffName}! (Role: {currentStaffRole})";

            // 2. Permission control: hide staff settings button if not a Manager
            if (currentStaffRole != "Manager")
            {
                btnStaff.Visible = false;
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to logout?", "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Login loginForm = new Login();
                loginForm.Show();
                this.Close();
            }
        }

        private void btnStaff_Click(object sender, EventArgs e)
        {
            EmployeeManagement staffForm = new EmployeeManagement();
            staffForm.ShowDialog();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void btnProduct_Click(object sender, EventArgs e)
        {
            ProductManagement productForm = new ProductManagement();
            productForm.ShowDialog();
        }
    }
}
