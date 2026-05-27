using System;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    partial class RawMaterialRequestForm
    {
        private System.ComponentModel.IContainer components = null;

        private TextBox txtCardID;
        private TextBox txtPartID;
        private TextBox txtQty;
        private Button btnSubmitRequest;
        private DataGridView dgvRequests;

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
            this.txtCardID = new System.Windows.Forms.TextBox();
            this.txtPartID = new System.Windows.Forms.TextBox();
            this.txtQty = new System.Windows.Forms.TextBox();
            this.btnSubmitRequest = new System.Windows.Forms.Button();
            this.dgvRequests = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRequests)).BeginInit();
            this.SuspendLayout();
            // 
            // txtCardID
            // 
            this.txtCardID.Location = new System.Drawing.Point(12, 12);
            this.txtCardID.Name = "txtCardID";
            this.txtCardID.Size = new System.Drawing.Size(240, 22);
            this.txtCardID.TabIndex = 0;
            // 
            // txtPartID
            // 
            this.txtPartID.Location = new System.Drawing.Point(12, 40);
            this.txtPartID.Name = "txtPartID";
            this.txtPartID.Size = new System.Drawing.Size(240, 22);
            this.txtPartID.TabIndex = 1;
            // 
            // txtQty
            // 
            this.txtQty.Location = new System.Drawing.Point(12, 68);
            this.txtQty.Name = "txtQty";
            this.txtQty.Size = new System.Drawing.Size(100, 22);
            this.txtQty.TabIndex = 2;
            // 
            // btnSubmitRequest
            // 
            this.btnSubmitRequest.Location = new System.Drawing.Point(12, 96);
            this.btnSubmitRequest.Name = "btnSubmitRequest";
            this.btnSubmitRequest.Size = new System.Drawing.Size(120, 30);
            this.btnSubmitRequest.TabIndex = 3;
            this.btnSubmitRequest.Text = "送出補貨申請";
            this.btnSubmitRequest.Click += new System.EventHandler(this.btnSubmitRequest_Click);
            // 
            // dgvRequests
            // 
            this.dgvRequests.Location = new System.Drawing.Point(270, 12);
            this.dgvRequests.Name = "dgvRequests";
            this.dgvRequests.ReadOnly = true;
            this.dgvRequests.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRequests.Size = new System.Drawing.Size(600, 320);
            this.dgvRequests.TabIndex = 4;
            // 
            // RawMaterialRequestForm
            // 
            this.ClientSize = new System.Drawing.Size(904, 511);
            this.Controls.Add(this.txtCardID);
            this.Controls.Add(this.txtPartID);
            this.Controls.Add(this.txtQty);
            this.Controls.Add(this.btnSubmitRequest);
            this.Controls.Add(this.dgvRequests);
            this.Name = "RawMaterialRequestForm";
            this.Text = "Raw Material Request";
            this.Load += new System.EventHandler(this.RawMaterialRequestForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRequests)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
