using System;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    partial class ProcurementForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ProcurementForm
            // 
            this.ClientSize = new System.Drawing.Size(904, 511);
            this.Name = "ProcurementForm";
            this.Text = "Procurement";
            this.Load += new System.EventHandler(this.ProcurementForm_Load);
            this.ResumeLayout(false);

        }
    }
}
