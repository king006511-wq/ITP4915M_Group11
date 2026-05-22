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
            this.components = new System.ComponentModel.Container();
            this.txtComplaintID = new TextBox();
            this.txtCustomerID = new TextBox();
            this.txtOrderID = new TextBox();
            this.txtDescription = new TextBox();
            this.cboStatus = new ComboBox();
            this.btnSubmitComplaint = new Button();
            this.dgvComplaints = new DataGridView();

            ((System.ComponentModel.ISupportInitialize)(this.dgvComplaints)).BeginInit();
            this.SuspendLayout();

            // txtComplaintID
            this.txtComplaintID.Location = new System.Drawing.Point(12, 12);
            this.txtComplaintID.Name = "txtComplaintID";
            this.txtComplaintID.Size = new System.Drawing.Size(240, 22);

            // txtCustomerID
            this.txtCustomerID.Location = new System.Drawing.Point(12, 40);
            this.txtCustomerID.Name = "txtCustomerID";
            this.txtCustomerID.Size = new System.Drawing.Size(240, 22);

            // txtOrderID
            this.txtOrderID.Location = new System.Drawing.Point(12, 68);
            this.txtOrderID.Name = "txtOrderID";
            this.txtOrderID.Size = new System.Drawing.Size(240, 22);

            // txtDescription
            this.txtDescription.Location = new System.Drawing.Point(12, 96);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(240, 80);
            this.txtDescription.Multiline = true;

            // cboStatus
            this.cboStatus.Location = new System.Drawing.Point(12, 184);
            this.cboStatus.Name = "cboStatus";
            this.cboStatus.Size = new System.Drawing.Size(160, 24);

            // btnSubmitComplaint
            this.btnSubmitComplaint.Location = new System.Drawing.Point(12, 214);
            this.btnSubmitComplaint.Name = "btnSubmitComplaint";
            this.btnSubmitComplaint.Size = new System.Drawing.Size(120, 30);
            this.btnSubmitComplaint.Text = "送出/更新投訴";
            this.btnSubmitComplaint.Click += new EventHandler(this.btnSubmitComplaint_Click);

            // dgvComplaints
            this.dgvComplaints.Location = new System.Drawing.Point(270, 12);
            this.dgvComplaints.Name = "dgvComplaints";
            this.dgvComplaints.Size = new System.Drawing.Size(600, 320);
            this.dgvComplaints.ReadOnly = true;
            this.dgvComplaints.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvComplaints.CellClick += new DataGridViewCellEventHandler(this.dgvComplaints_CellClick);

            // AfterServiceForm
            this.ClientSize = new System.Drawing.Size(900, 360);
            this.Controls.Add(this.txtComplaintID);
            this.Controls.Add(this.txtCustomerID);
            this.Controls.Add(this.txtOrderID);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.cboStatus);
            this.Controls.Add(this.btnSubmitComplaint);
            this.Controls.Add(this.dgvComplaints);
            this.Name = "AfterServiceForm";
            this.Text = "After Service";
            this.Load += new EventHandler(this.AfterServiceForm_Load);

            ((System.ComponentModel.ISupportInitialize)(this.dgvComplaints)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
