using System;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    partial class AfterServiceForm
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
            // AfterServiceForm
            // 
            this.ClientSize = new System.Drawing.Size(904, 511);
            this.Name = "AfterServiceForm";
            this.Text = "After Service";
            this.Load += new System.EventHandler(this.AfterServiceForm_Load_1);
            this.ResumeLayout(false);

        }
    }
}
