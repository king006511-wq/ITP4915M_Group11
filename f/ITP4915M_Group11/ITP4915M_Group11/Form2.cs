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

        // 登出按鈕嘅點擊事件 (雙擊 btnLogout 就會自動產生呢個 block)
        private void btnLogout_Click(object sender, EventArgs e)
        {
            // 彈出確認視窗
            DialogResult result = MessageBox.Show("確定要登出嗎？", "登出", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                // 打開返 Form1 (登入介面)
                Form1 loginForm = new Form1();
                loginForm.Show();

                // 關閉目前嘅 Form2
                this.Close();
            }
        }

        private void btnStaff_Click(object sender, EventArgs e)
        {
            Form3 staffForm = new Form3(); // 如果你改咗名，記得將 Form3 換成你個名
            staffForm.ShowDialog();        // ShowDialog 代表打開新視窗嗰陣，鎖住舊視窗
        }
    }
}