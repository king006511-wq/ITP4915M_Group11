using System;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public class AfterSalesServiceForm : Form
    {
        private DataGridView dgvCases;

        public AfterSalesServiceForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "After-sales Service";
            this.Size = new System.Drawing.Size(900, 600);

            dgvCases = new DataGridView();
            dgvCases.Dock = DockStyle.Fill;
            this.Controls.Add(dgvCases);
        }

        // TODO: Implement return, replacement, refund handling and logging
    }
}
