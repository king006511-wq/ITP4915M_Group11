using System;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public class InventoryInboundForm : Form
    {
        private DataGridView dgvInbound;

        public InventoryInboundForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Inventory Inbound";
            this.Size = new System.Drawing.Size(900, 600);

            dgvInbound = new DataGridView();
            dgvInbound.Dock = DockStyle.Fill;
            this.Controls.Add(dgvInbound);
        }

        // TODO: Implement inbound goods recording and stock tracking
    }
}
