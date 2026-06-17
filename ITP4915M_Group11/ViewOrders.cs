using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class ViewOrders : Form
    {
        public ViewOrders()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
            }
        }

        private void ViewOrders_Shown(object sender, EventArgs e)
        {
            // Try add a Back Home button at runtime if the form has a panel named pnlMain
            var pnl = this.Controls.OfType<Panel>().FirstOrDefault(p => p.Name == "pnlMain" || p.Location == new System.Drawing.Point(260, 0));
            if (pnl != null)
            {
                Button btnBackHome = new Button { Text = "🏠 Back Home", Size = new System.Drawing.Size(120, 34), Location = new System.Drawing.Point(740, 22), BackColor = System.Drawing.Color.FromArgb(37, 99, 235), ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat, Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold), Cursor = Cursors.Hand };
                btnBackHome.FlatAppearance.BorderSize = 0;
                btnBackHome.Click += (s, ev) => { NavigationHelper.GoToMainDashboard(this); };
                pnl.Controls.Add(btnBackHome);
            }

            // 限制檢視訂單的權限（示範）: 預設允許所有人查看，但若需限制可在此調整
            // 例: 只有 Manager / Administrator / SalesRepresentative 可查看完整訂單
            bool canView = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.SalesRepresentative);
            if (!canView)
            {
                // 若不允許檢視，顯示警告並關閉
                MessageBox.Show("您沒有權限檢視完整訂單資料。如需協助請聯絡管理員。", "存取被拒", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.BeginInvoke(new MethodInvoker(this.Close));
            }
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void dateTimePicker3_ValueChanged(object sender, EventArgs e)
        {

        }

        private void dgvPOItems_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
