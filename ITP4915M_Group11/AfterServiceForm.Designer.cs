using System;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    partial class AfterServiceForm
    {
        private System.ComponentModel.IContainer components = null;

        private TextBox txtComplaintID;
        private TextBox txtCustomerID;
        private TextBox txtOrderID;
        private TextBox txtDescription;
        private ComboBox cboStatus;
        private Button btnSubmitComplaint;
        private DataGridView dgvComplaints;

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
            this.txtComplaintID = new System.Windows.Forms.TextBox();
            this.txtCustomerID = new System.Windows.Forms.TextBox();
            this.txtOrderID = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.cboStatus = new System.Windows.Forms.ComboBox();
            this.btnSubmitComplaint = new System.Windows.Forms.Button();
            this.dgvComplaints = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvComplaints)).BeginInit();
            this.SuspendLayout();
            // 
            // txtComplaintID
            // 
            this.txtComplaintID.Location = new System.Drawing.Point(12, 12);
            this.txtComplaintID.Name = "txtComplaintID";
            this.txtComplaintID.Size = new System.Drawing.Size(240, 22);
            this.txtComplaintID.TabIndex = 0;
            // 
            // txtCustomerID
            // 
            this.txtCustomerID.Location = new System.Drawing.Point(12, 40);
            this.txtCustomerID.Name = "txtCustomerID";
            this.txtCustomerID.Size = new System.Drawing.Size(240, 22);
            this.txtCustomerID.TabIndex = 1;
            // 
            // txtOrderID
            // 
            this.txtOrderID.Location = new System.Drawing.Point(12, 68);
            this.txtOrderID.Name = "txtOrderID";
            this.txtOrderID.Size = new System.Drawing.Size(240, 22);
            this.txtOrderID.TabIndex = 2;
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(12, 96);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(240, 80);
            this.txtDescription.TabIndex = 3;
            // 
            // cboStatus
            // 
            this.cboStatus.Location = new System.Drawing.Point(12, 184);
            this.cboStatus.Name = "cboStatus";
            this.cboStatus.Size = new System.Drawing.Size(160, 20);
            this.cboStatus.TabIndex = 4;
            // 
            // btnSubmitComplaint
            // 
            this.btnSubmitComplaint.Location = new System.Drawing.Point(12, 214);
            this.btnSubmitComplaint.Name = "btnSubmitComplaint";
            this.btnSubmitComplaint.Size = new System.Drawing.Size(120, 30);
            this.btnSubmitComplaint.TabIndex = 5;
            this.btnSubmitComplaint.Text = "送出/更新投訴";
            this.btnSubmitComplaint.Click += new System.EventHandler(this.btnSubmitComplaint_Click);
            // 
            // dgvComplaints
            // 
            this.dgvComplaints.Location = new System.Drawing.Point(270, 12);
            this.dgvComplaints.Name = "dgvComplaints";
            this.dgvComplaints.ReadOnly = true;
            this.dgvComplaints.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvComplaints.Size = new System.Drawing.Size(320, 200);
            this.dgvComplaints.TabIndex = 6;
            this.dgvComplaints.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvComplaints_CellClick);
            // 
            // AfterServiceForm
            // 
            this.ClientSize = new System.Drawing.Size(904, 511);
            this.Controls.Add(this.txtComplaintID);
            this.Controls.Add(this.txtCustomerID);
            this.Controls.Add(this.txtOrderID);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.cboStatus);
            this.Controls.Add(this.btnSubmitComplaint);
            this.Controls.Add(this.dgvComplaints);
            this.Name = "AfterServiceForm";
            this.Text = "After Service";
            this.Load += new System.EventHandler(this.AfterServiceForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvComplaints)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
