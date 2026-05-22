using System;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public class OrderProcessingForm : Form
    {
        private DataGridView dgvOrders;

        public OrderProcessingForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Order Processing";
            this.Size = new System.Drawing.Size(900, 600);

            dgvOrders = new DataGridView();
            dgvOrders.Dock = DockStyle.Fill;
            this.Controls.Add(dgvOrders);
        }

        // TODO: Implement Create, Edit, View, Cancel, Search operations using the provided DB schema
    }
}
