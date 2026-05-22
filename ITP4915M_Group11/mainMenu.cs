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

            // 接收登入成功的資料
            currentStaffName = name;
            currentStaffRole = role;

            // 1. 設定頂部歡迎字句
            lblWelcome.Text = $"歡迎回來，{currentStaffName}！ (權限: {currentStaffRole})";

            // 2. 權限控制：如果不是 Manager，就隱藏「員工管理」按鈕
            if (currentStaffRole != "Manager")
            {
                btnStaff.Visible = false;
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            // 視窗載入時的邏輯（可留空）
        }

        // =========================================================
        // 📦 各模組按鈕點擊事件（導向各個 Form）
        // =========================================================

        // 1. 產品/零件管理 (已有 Form)
        private void btnProduct_Click(object sender, EventArgs e)
        {
            ProductManagement productForm = new ProductManagement();
            productForm.ShowDialog();
        }

        // btnOrderProcessing 的事件處理（由 Designer 呼叫）
        private void btnOrderProcessing_Click(object sender, EventArgs e)
        {
            // 打開訂單管理畫面
            OrderManagementForm orderForm = new OrderManagementForm();
            orderForm.ShowDialog();
        }

        // 2. 物流與送貨單處理 (已有 Form)
        private void btnOrder_Click(object sender, EventArgs e)
        {
            // 注意：看你的介面命名，btnOrder 如果是用來開物流的：
            LogisticsForm logisticsForm = new LogisticsForm();
            logisticsForm.ShowDialog();
        }

        // 3. 員工管理 (必須補上 EmployeeManagement.cs 否則這裡會報錯)
        private void btnStaff_Click(object sender, EventArgs e)
        {
            // 解開註解前，請先在專案中建立 EmployeeManagement 表單
            // EmployeeManagement staffForm = new EmployeeManagement();
            // staffForm.ShowDialog();

            MessageBox.Show("員工管理模組建置中...", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // 4. 登出
        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("確定要登出嗎？", "登出確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Login loginForm = new Login();
                loginForm.Show();
                this.Close(); // 關閉主選單，回到登入畫面
            }
        }
    }
}