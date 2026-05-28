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
            this.pnlMainContainer = new System.Windows.Forms.Panel();
            this.txtComplaintID = new System.Windows.Forms.TextBox();
            this.txtCustomerID = new System.Windows.Forms.TextBox();
            this.txtOrderID = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.cboStatus = new System.Windows.Forms.ComboBox();
            this.btnSubmitComplaint = new System.Windows.Forms.Button();
            this.dgvComplaints = new System.Windows.Forms.DataGridView();
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.btnLogout = new System.Windows.Forms.Button();
            this.btnAfterService = new System.Windows.Forms.Button();
            this.btnProcurement = new System.Windows.Forms.Button();
            this.btnMaterialRequest = new System.Windows.Forms.Button();
            this.btnGoodsReceived = new System.Windows.Forms.Button();
            this.btnStaff = new System.Windows.Forms.Button();
            this.btnProduct = new System.Windows.Forms.Button();
            this.btnOrder = new System.Windows.Forms.Button();
            this.btnOrderProcessing = new System.Windows.Forms.Button();
            this.lblWelcome = new System.Windows.Forms.Label();
            this.pnlMainContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvComplaints)).BeginInit();
            this.pnlSidebar.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlMainContainer
            // 
            this.pnlMainContainer.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlMainContainer.Controls.Add(this.txtComplaintID);
            this.pnlMainContainer.Controls.Add(this.txtCustomerID);
            this.pnlMainContainer.Controls.Add(this.txtOrderID);
            this.pnlMainContainer.Controls.Add(this.txtDescription);
            this.pnlMainContainer.Controls.Add(this.cboStatus);
            this.pnlMainContainer.Controls.Add(this.btnSubmitComplaint);
            this.pnlMainContainer.Controls.Add(this.dgvComplaints);
            this.pnlMainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMainContainer.Location = new System.Drawing.Point(220, 0);
            this.pnlMainContainer.Name = "pnlMainContainer";
            this.pnlMainContainer.Size = new System.Drawing.Size(684, 511);
            this.pnlMainContainer.TabIndex = 7;
            // 
            // txtComplaintID
            // 
            this.txtComplaintID.Location = new System.Drawing.Point(67, 70);
            this.txtComplaintID.Name = "txtComplaintID";
            this.txtComplaintID.Size = new System.Drawing.Size(240, 22);
            this.txtComplaintID.TabIndex = 7;
            // 
            // txtCustomerID
            // 
            this.txtCustomerID.Location = new System.Drawing.Point(67, 98);
            this.txtCustomerID.Name = "txtCustomerID";
            this.txtCustomerID.Size = new System.Drawing.Size(240, 22);
            this.txtCustomerID.TabIndex = 8;
            // 
            // txtOrderID
            // 
            this.txtOrderID.Location = new System.Drawing.Point(67, 126);
            this.txtOrderID.Name = "txtOrderID";
            this.txtOrderID.Size = new System.Drawing.Size(240, 22);
            this.txtOrderID.TabIndex = 9;
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(67, 154);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(240, 80);
            this.txtDescription.TabIndex = 10;
            // 
            // cboStatus
            // 
            this.cboStatus.Location = new System.Drawing.Point(67, 242);
            this.cboStatus.Name = "cboStatus";
            this.cboStatus.Size = new System.Drawing.Size(160, 20);
            this.cboStatus.TabIndex = 11;
            // 
            // btnSubmitComplaint
            // 
            this.btnSubmitComplaint.Location = new System.Drawing.Point(67, 272);
            this.btnSubmitComplaint.Name = "btnSubmitComplaint";
            this.btnSubmitComplaint.Size = new System.Drawing.Size(120, 30);
            this.btnSubmitComplaint.TabIndex = 12;
            this.btnSubmitComplaint.Text = "送出/更新投訴";
            // 
            // dgvComplaints
            // 
            this.dgvComplaints.Location = new System.Drawing.Point(325, 70);
            this.dgvComplaints.Name = "dgvComplaints";
            this.dgvComplaints.ReadOnly = true;
            this.dgvComplaints.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvComplaints.Size = new System.Drawing.Size(320, 200);
            this.dgvComplaints.TabIndex = 13;
            // 
            // pnlSidebar
            // 
            this.pnlSidebar.BackColor = System.Drawing.Color.DimGray;
            this.pnlSidebar.Controls.Add(this.btnLogout);
            this.pnlSidebar.Controls.Add(this.btnAfterService);
            this.pnlSidebar.Controls.Add(this.btnProcurement);
            this.pnlSidebar.Controls.Add(this.btnMaterialRequest);
            this.pnlSidebar.Controls.Add(this.btnGoodsReceived);
            this.pnlSidebar.Controls.Add(this.btnStaff);
            this.pnlSidebar.Controls.Add(this.btnProduct);
            this.pnlSidebar.Controls.Add(this.btnOrder);
            this.pnlSidebar.Controls.Add(this.btnOrderProcessing);
            this.pnlSidebar.Controls.Add(this.lblWelcome);
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Location = new System.Drawing.Point(0, 0);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(220, 511);
            this.pnlSidebar.TabIndex = 8;
            // 
            // btnLogout
            // 
            this.btnLogout.BackColor = System.Drawing.Color.Maroon;
            this.btnLogout.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnLogout.FlatAppearance.BorderSize = 0;
            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.ForeColor = System.Drawing.Color.White;
            this.btnLogout.Location = new System.Drawing.Point(0, 461);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(220, 50);
            this.btnLogout.TabIndex = 9;
            this.btnLogout.Text = "登出";
            this.btnLogout.UseVisualStyleBackColor = false;
            // 
            // btnAfterService
            // 
            this.btnAfterService.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnAfterService.FlatAppearance.BorderSize = 0;
            this.btnAfterService.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAfterService.ForeColor = System.Drawing.Color.White;
            this.btnAfterService.Location = new System.Drawing.Point(0, 385);
            this.btnAfterService.Name = "btnAfterService";
            this.btnAfterService.Size = new System.Drawing.Size(220, 45);
            this.btnAfterService.TabIndex = 7;
            this.btnAfterService.Text = "   ☎️ 客戶售後服務";
            this.btnAfterService.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAfterService.UseVisualStyleBackColor = true;
            // 
            // btnProcurement
            // 
            this.btnProcurement.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnProcurement.FlatAppearance.BorderSize = 0;
            this.btnProcurement.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProcurement.ForeColor = System.Drawing.Color.White;
            this.btnProcurement.Location = new System.Drawing.Point(0, 340);
            this.btnProcurement.Name = "btnProcurement";
            this.btnProcurement.Size = new System.Drawing.Size(220, 45);
            this.btnProcurement.TabIndex = 6;
            this.btnProcurement.Text = "   📄 採購控制台";
            this.btnProcurement.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnProcurement.UseVisualStyleBackColor = true;
            // 
            // btnMaterialRequest
            // 
            this.btnMaterialRequest.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnMaterialRequest.FlatAppearance.BorderSize = 0;
            this.btnMaterialRequest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMaterialRequest.ForeColor = System.Drawing.Color.White;
            this.btnMaterialRequest.Location = new System.Drawing.Point(0, 295);
            this.btnMaterialRequest.Name = "btnMaterialRequest";
            this.btnMaterialRequest.Size = new System.Drawing.Size(220, 45);
            this.btnMaterialRequest.TabIndex = 5;
            this.btnMaterialRequest.Text = "   ⚠️ 工廠物料請求";
            this.btnMaterialRequest.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMaterialRequest.UseVisualStyleBackColor = true;
            // 
            // btnGoodsReceived
            // 
            this.btnGoodsReceived.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnGoodsReceived.FlatAppearance.BorderSize = 0;
            this.btnGoodsReceived.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGoodsReceived.ForeColor = System.Drawing.Color.White;
            this.btnGoodsReceived.Location = new System.Drawing.Point(0, 250);
            this.btnGoodsReceived.Name = "btnGoodsReceived";
            this.btnGoodsReceived.Size = new System.Drawing.Size(220, 45);
            this.btnGoodsReceived.TabIndex = 4;
            this.btnGoodsReceived.Text = "   📥 倉儲收貨入庫";
            this.btnGoodsReceived.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnGoodsReceived.UseVisualStyleBackColor = true;
            // 
            // btnStaff
            // 
            this.btnStaff.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnStaff.FlatAppearance.BorderSize = 0;
            this.btnStaff.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStaff.ForeColor = System.Drawing.Color.White;
            this.btnStaff.Location = new System.Drawing.Point(0, 205);
            this.btnStaff.Name = "btnStaff";
            this.btnStaff.Size = new System.Drawing.Size(220, 45);
            this.btnStaff.TabIndex = 8;
            this.btnStaff.Text = "   👥 系統員工管理";
            this.btnStaff.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStaff.UseVisualStyleBackColor = true;
            // 
            // btnProduct
            // 
            this.btnProduct.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnProduct.FlatAppearance.BorderSize = 0;
            this.btnProduct.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProduct.ForeColor = System.Drawing.Color.White;
            this.btnProduct.Location = new System.Drawing.Point(0, 160);
            this.btnProduct.Name = "btnProduct";
            this.btnProduct.Size = new System.Drawing.Size(220, 45);
            this.btnProduct.TabIndex = 3;
            this.btnProduct.Text = "   📦 產品零件維護";
            this.btnProduct.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnProduct.UseVisualStyleBackColor = true;
            // 
            // btnOrder
            // 
            this.btnOrder.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnOrder.FlatAppearance.BorderSize = 0;
            this.btnOrder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOrder.ForeColor = System.Drawing.Color.White;
            this.btnOrder.Location = new System.Drawing.Point(0, 115);
            this.btnOrder.Name = "btnOrder";
            this.btnOrder.Size = new System.Drawing.Size(220, 45);
            this.btnOrder.TabIndex = 2;
            this.btnOrder.Text = "   🚚 物流送貨處理";
            this.btnOrder.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOrder.UseVisualStyleBackColor = true;
            // 
            // btnOrderProcessing
            // 
            this.btnOrderProcessing.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnOrderProcessing.FlatAppearance.BorderSize = 0;
            this.btnOrderProcessing.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOrderProcessing.ForeColor = System.Drawing.Color.White;
            this.btnOrderProcessing.Location = new System.Drawing.Point(0, 70);
            this.btnOrderProcessing.Name = "btnOrderProcessing";
            this.btnOrderProcessing.Size = new System.Drawing.Size(220, 45);
            this.btnOrderProcessing.TabIndex = 1;
            this.btnOrderProcessing.Text = "   🛒 銷售落單管理";
            this.btnOrderProcessing.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOrderProcessing.UseVisualStyleBackColor = true;
            // 
            // lblWelcome
            // 
            this.lblWelcome.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblWelcome.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblWelcome.Location = new System.Drawing.Point(0, 0);
            this.lblWelcome.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.Size = new System.Drawing.Size(220, 70);
            this.lblWelcome.TabIndex = 0;
            this.lblWelcome.Text = "歡迎回來，使用者";
            this.lblWelcome.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // AfterServiceForm
            // 
            this.ClientSize = new System.Drawing.Size(904, 511);
            this.Controls.Add(this.pnlMainContainer);
            this.Controls.Add(this.pnlSidebar);
            this.Name = "AfterServiceForm";
            this.Text = "After Service";
            this.Load += new System.EventHandler(this.AfterServiceForm_Load);
            this.pnlMainContainer.ResumeLayout(false);
            this.pnlMainContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvComplaints)).EndInit();
            this.pnlSidebar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private Panel pnlMainContainer;
        private TextBox txtComplaintID;
        private TextBox txtCustomerID;
        private TextBox txtOrderID;
        private TextBox txtDescription;
        private ComboBox cboStatus;
        private Button btnSubmitComplaint;
        private DataGridView dgvComplaints;
        private Panel pnlSidebar;
        private Button btnLogout;
        private Button btnAfterService;
        private Button btnProcurement;
        private Button btnMaterialRequest;
        private Button btnGoodsReceived;
        private Button btnStaff;
        private Button btnProduct;
        private Button btnOrder;
        private Button btnOrderProcessing;
        private Label lblWelcome;
    }
}
