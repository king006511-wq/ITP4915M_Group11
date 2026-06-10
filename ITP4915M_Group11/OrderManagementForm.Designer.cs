using System;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    partial class OrderManagementForm
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
            // OrderManagementForm
            // 
            this.ClientSize = new System.Drawing.Size(904, 511);
            this.Name = "OrderManagementForm";
            this.Text = "Order Management";
            this.ResumeLayout(false);

        }
    }
}
