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
            this.components = new System.ComponentModel.Container();
            this.txtCardID = new TextBox();
            this.txtPartID = new TextBox();
            this.txtQty = new TextBox();
            this.btnSubmitRequest = new Button();
            this.dgvRequests = new DataGridView();

            ((System.ComponentModel.ISupportInitialize)(this.dgvRequests)).BeginInit();
            this.SuspendLayout();

            // txtCardID
            this.txtCardID.Location = new System.Drawing.Point(12, 12);
            this.txtCardID.Name = "txtCardID";
            this.txtCardID.Size = new System.Drawing.Size(240, 22);

            // txtPartID
            this.txtPartID.Location = new System.Drawing.Point(12, 40);
            this.txtPartID.Name = "txtPartID";
            this.txtPartID.Size = new System.Drawing.Size(240, 22);

            // txtQty
            this.txtQty.Location = new System.Drawing.Point(12, 68);
            this.txtQty.Name = "txtQty";
            this.txtQty.Size = new System.Drawing.Size(100, 22);

            // btnSubmitRequest
            this.btnSubmitRequest.Location = new System.Drawing.Point(12, 96);
            this.btnSubmitRequest.Name = "btnSubmitRequest";
            this.btnSubmitRequest.Size = new System.Drawing.Size(120, 30);
            this.btnSubmitRequest.Text = "送出補貨申請";
            this.btnSubmitRequest.Click += new EventHandler(this.btnSubmitRequest_Click);

            // dgvRequests
            this.dgvRequests.Location = new System.Drawing.Point(270, 12);
            this.dgvRequests.Name = "dgvRequests";
            this.dgvRequests.Size = new System.Drawing.Size(600, 320);
            this.dgvRequests.ReadOnly = true;
            this.dgvRequests.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // RawMaterialRequestForm
            this.ClientSize = new System.Drawing.Size(900, 360);
            this.Controls.Add(this.txtCardID);
            this.Controls.Add(this.txtPartID);
            this.Controls.Add(this.txtQty);
            this.Controls.Add(this.btnSubmitRequest);
            this.Controls.Add(this.dgvRequests);
            this.Name = "RawMaterialRequestForm";
            this.Text = "Raw Material Request";
            this.Load += new EventHandler(this.RawMaterialRequestForm_Load);

            ((System.ComponentModel.ISupportInitialize)(this.dgvRequests)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
