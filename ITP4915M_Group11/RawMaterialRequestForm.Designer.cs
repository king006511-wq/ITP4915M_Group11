using System;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    partial class RawMaterialRequestForm
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
            // RawMaterialRequestForm
            // 
            this.ClientSize = new System.Drawing.Size(904, 511);
            this.Name = "RawMaterialRequestForm";
            this.Text = "Raw Material Request";

            this.ResumeLayout(false);

        }
    }
}
