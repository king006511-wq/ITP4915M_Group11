using System;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class Form2 : Form
    {
        private string currentStaffName;
        private string currentStaffRole;

        public Form2(string name, string role)
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

        // 登出按鈕嘅點擊事件
        private void btnLogout_Click(object sender, EventArgs e)
        {
            // 彈出確認視窗
            DialogResult result = MessageBox.Show("確定要登出嗎？", "登出", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                // 打開返 Login (登入介面)
                Login loginForm = new Login();
                loginForm.Show();

                // 關閉目前嘅 Form2
                this.Close();
            }
        }

        // 點擊「員工及系統設定」按鈕
        private void btnStaff_Click(object sender, EventArgs e)
        {
            Form3 staffForm = new Form3(); // 打開員工管理 Form3
            staffForm.ShowDialog();        // ShowDialog 代表打開新視窗嗰陣，鎖住舊選單視窗
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        // =========================================================
        // 🎯 點擊「產品管理」按鈕，全自動連接到啱啱搞掂嘅 Form4
        // =========================================================
        private void btnProduct_Click(object sender, EventArgs e)
        {
            Form4 productForm = new Form4(); // 宣告並建立產品管理 Form4 視窗物件
            productForm.ShowDialog();        // 用 ShowDialog() 打開，鎖住 Form2 主選單，防止用家重複開幾十個 Form4
        }
    }
}