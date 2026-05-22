using System;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class mainMenu : Form
    {
        private string currentStaffName;
        private string currentStaffRole;

        public mainMenu(string name, string role)
        {
            InitializeComponent();

            // 接收資料
            currentStaffName = name;
            currentStaffRole = role;

            // 1. 設定頂部歡迎字句
            lblWelcome.Text = $"歡迎回來，{currentStaffName}！ (權限: {currentStaffRole})";

            // 2. 權限控制：如果唔係 Manager，就收埋「員工及系統設定」個掣
            if (currentStaffRole != "Manager")
            {
                btnStaff.Visible = false;
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("確定要登出嗎？", "登出", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Login loginForm = new Login();
                loginForm.Show();
                this.Close();
            }
        }

        private void btnStaff_Click(object sender, EventArgs e)
        {
            employeeManagement staffForm = new employeeManagement();
            staffForm.ShowDialog();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void btnProduct_Click(object sender, EventArgs e)
        {
            Form4 productForm = new Form4();
            productForm.ShowDialog();
        }
    }
}
