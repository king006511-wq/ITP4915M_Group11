using System;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public class RawMaterialManagementForm : Form
    {
        private DataGridView dgvPurchases;

        public RawMaterialManagementForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Raw Material Management";
            this.Size = new System.Drawing.Size(900, 600);

            dgvPurchases = new DataGridView();
            dgvPurchases.Dock = DockStyle.Fill;
            this.Controls.Add(dgvPurchases);
        }

        // TODO: Implement create, edit, search, and list of procurement activities
    }
}
