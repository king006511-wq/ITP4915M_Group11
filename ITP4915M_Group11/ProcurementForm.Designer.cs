using System;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    partial class ProcurementForm
    {
        private System.ComponentModel.IContainer components = null;

        private TextBox txtPOID;
        private TextBox txtSupplierID;
        private TextBox txtStaffID;
        private TextBox txtRCID;
        private TextBox txtPartID;
        private TextBox txtQty;
        private TextBox txtPrice;
        private Button btnCreatePO;
        private DataGridView dgvPendingRC;

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
            this.txtPOID = new System.Windows.Forms.TextBox();
            this.txtSupplierID = new System.Windows.Forms.TextBox();
            this.txtStaffID = new System.Windows.Forms.TextBox();
            this.txtRCID = new System.Windows.Forms.TextBox();
            this.txtPartID = new System.Windows.Forms.TextBox();
            this.txtQty = new System.Windows.Forms.TextBox();
            this.txtPrice = new System.Windows.Forms.TextBox();
            this.btnCreatePO = new System.Windows.Forms.Button();
            this.dgvPendingRC = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendingRC)).BeginInit();
            this.SuspendLayout();
            // 
            // txtPOID
            // 
            this.txtPOID.Location = new System.Drawing.Point(12, 12);
            this.txtPOID.Name = "txtPOID";
            this.txtPOID.Size = new System.Drawing.Size(240, 22);
            this.txtPOID.TabIndex = 0;
            // 
            // txtSupplierID
            // 
            this.txtSupplierID.Location = new System.Drawing.Point(12, 40);
            this.txtSupplierID.Name = "txtSupplierID";
            this.txtSupplierID.Size = new System.Drawing.Size(240, 22);
            this.txtSupplierID.TabIndex = 1;
            // 
            // txtStaffID
            // 
            this.txtStaffID.Location = new System.Drawing.Point(12, 68);
            this.txtStaffID.Name = "txtStaffID";
            this.txtStaffID.Size = new System.Drawing.Size(240, 22);
            this.txtStaffID.TabIndex = 2;
            // 
            // txtRCID
            // 
            this.txtRCID.Location = new System.Drawing.Point(12, 96);
            this.txtRCID.Name = "txtRCID";
            this.txtRCID.Size = new System.Drawing.Size(240, 22);
            this.txtRCID.TabIndex = 3;
            // 
            // txtPartID
            // 
            this.txtPartID.Location = new System.Drawing.Point(12, 124);
            this.txtPartID.Name = "txtPartID";
            this.txtPartID.Size = new System.Drawing.Size(240, 22);
            this.txtPartID.TabIndex = 4;
            // 
            // txtQty
            // 
            this.txtQty.Location = new System.Drawing.Point(12, 152);
            this.txtQty.Name = "txtQty";
            this.txtQty.Size = new System.Drawing.Size(100, 22);
            this.txtQty.TabIndex = 5;
            // 
            // txtPrice
            // 
            this.txtPrice.Location = new System.Drawing.Point(12, 180);
            this.txtPrice.Name = "txtPrice";
            this.txtPrice.Size = new System.Drawing.Size(100, 22);
            this.txtPrice.TabIndex = 6;
            // 
            // btnCreatePO
            // 
            this.btnCreatePO.Location = new System.Drawing.Point(12, 210);
            this.btnCreatePO.Name = "btnCreatePO";
            this.btnCreatePO.Size = new System.Drawing.Size(120, 30);
            this.btnCreatePO.TabIndex = 7;
            this.btnCreatePO.Text = "建立採購單";
            this.btnCreatePO.Click += new System.EventHandler(this.btnCreatePO_Click);
            // 
            // dgvPendingRC
            // 
            this.dgvPendingRC.Location = new System.Drawing.Point(270, 12);
            this.dgvPendingRC.Name = "dgvPendingRC";
            this.dgvPendingRC.ReadOnly = true;
            this.dgvPendingRC.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPendingRC.Size = new System.Drawing.Size(320, 200);
            this.dgvPendingRC.TabIndex = 8;
            this.dgvPendingRC.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvPendingRC_CellClick);
            // 
            // ProcurementForm
            // 
            this.ClientSize = new System.Drawing.Size(904, 511);
            this.Controls.Add(this.txtPOID);
            this.Controls.Add(this.txtSupplierID);
            this.Controls.Add(this.txtStaffID);
            this.Controls.Add(this.txtRCID);
            this.Controls.Add(this.txtPartID);
            this.Controls.Add(this.txtQty);
            this.Controls.Add(this.txtPrice);
            this.Controls.Add(this.btnCreatePO);
            this.Controls.Add(this.dgvPendingRC);
            this.Name = "ProcurementForm";
            this.Text = "Procurement";
            this.Load += new System.EventHandler(this.ProcurementForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendingRC)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
