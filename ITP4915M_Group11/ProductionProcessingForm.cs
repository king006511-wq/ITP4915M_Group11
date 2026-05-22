using System;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public class ProductionProcessingForm : Form
    {
        private DataGridView dgvReorder;

        public ProductionProcessingForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Production Processing";
            this.Size = new System.Drawing.Size(900, 600);

            dgvReorder = new DataGridView();
            dgvReorder.Dock = DockStyle.Fill;
            this.Controls.Add(dgvReorder);
        }

        // TODO: Implement creation and search of raw material requirements
    }
}
