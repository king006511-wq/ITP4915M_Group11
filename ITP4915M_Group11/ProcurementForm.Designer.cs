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
            this.components = new System.ComponentModel.Container();
            this.txtPOID = new TextBox();
            this.txtSupplierID = new TextBox();
            this.txtStaffID = new TextBox();
            this.txtRCID = new TextBox();
            this.txtPartID = new TextBox();
            this.txtQty = new TextBox();
            this.txtPrice = new TextBox();
            this.btnCreatePO = new Button();
            this.dgvPendingRC = new DataGridView();

            ((System.ComponentModel.ISupportInitialize)(this.dgvPendingRC)).BeginInit();
            this.SuspendLayout();

            // txtPOID
            this.txtPOID.Location = new System.Drawing.Point(12, 12);
            this.txtPOID.Name = "txtPOID";
            this.txtPOID.Size = new System.Drawing.Size(240, 22);

            // txtSupplierID
            this.txtSupplierID.Location = new System.Drawing.Point(12, 40);
            this.txtSupplierID.Name = "txtSupplierID";
            this.txtSupplierID.Size = new System.Drawing.Size(240, 22);

            // txtStaffID
            this.txtStaffID.Location = new System.Drawing.Point(12, 68);
            this.txtStaffID.Name = "txtStaffID";
            this.txtStaffID.Size = new System.Drawing.Size(240, 22);

            // txtRCID
            this.txtRCID.Location = new System.Drawing.Point(12, 96);
            this.txtRCID.Name = "txtRCID";
            this.txtRCID.Size = new System.Drawing.Size(240, 22);

            // txtPartID
            this.txtPartID.Location = new System.Drawing.Point(12, 124);
            this.txtPartID.Name = "txtPartID";
            this.txtPartID.Size = new System.Drawing.Size(240, 22);

            // txtQty
            this.txtQty.Location = new System.Drawing.Point(12, 152);
            this.txtQty.Name = "txtQty";
            this.txtQty.Size = new System.Drawing.Size(100, 22);

            // txtPrice
            this.txtPrice.Location = new System.Drawing.Point(12, 180);
            this.txtPrice.Name = "txtPrice";
            this.txtPrice.Size = new System.Drawing.Size(100, 22);

            // btnCreatePO
            this.btnCreatePO.Location = new System.Drawing.Point(12, 210);
            this.btnCreatePO.Name = "btnCreatePO";
            this.btnCreatePO.Size = new System.Drawing.Size(120, 30);
            this.btnCreatePO.Text = "建立採購單";
            this.btnCreatePO.Click += new EventHandler(this.btnCreatePO_Click);

            // dgvPendingRC
            this.dgvPendingRC.Location = new System.Drawing.Point(270, 12);
            this.dgvPendingRC.Name = "dgvPendingRC";
            this.dgvPendingRC.Size = new System.Drawing.Size(600, 320);
            this.dgvPendingRC.ReadOnly = true;
            this.dgvPendingRC.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvPendingRC.CellClick += new DataGridViewCellEventHandler(this.dgvPendingRC_CellClick);

            // ProcurementForm
            this.ClientSize = new System.Drawing.Size(900, 360);
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
            this.Load += new EventHandler(this.ProcurementForm_Load);

            ((System.ComponentModel.ISupportInitialize)(this.dgvPendingRC)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
