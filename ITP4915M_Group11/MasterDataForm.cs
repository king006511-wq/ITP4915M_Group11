using System;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public class MasterDataForm : Form
    {
        private DataGridView dgvEntities;

        public MasterDataForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Master Data Management";
            this.Size = new System.Drawing.Size(900, 600);

            dgvEntities = new DataGridView();
            dgvEntities.Dock = DockStyle.Fill;
            this.Controls.Add(dgvEntities);
        }

        // TODO: Implement supplier and restaurant entity maintenance screens
    }
}
