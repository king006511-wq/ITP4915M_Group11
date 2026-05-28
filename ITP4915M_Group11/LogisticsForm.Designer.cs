namespace ITP4915M_Group11
{
    partial class LogisticsForm
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlMainContainer = new System.Windows.Forms.Panel();
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
            this.btnGenerateDeliveryNote = new System.Windows.Forms.Button();
            this.grpDispatchConfig = new System.Windows.Forms.GroupBox();
            this.lblDispatchID = new System.Windows.Forms.Label();
            this.txtDispatchID = new System.Windows.Forms.TextBox();
            this.lblEstDelivery = new System.Windows.Forms.Label();
            this.dtpEstDelivery = new System.Windows.Forms.DateTimePicker();
            this.lblMethod = new System.Windows.Forms.Label();
            this.cboMethod = new System.Windows.Forms.ComboBox();
            this.lblDriverName = new System.Windows.Forms.Label();
            this.txtDriverName = new System.Windows.Forms.TextBox();
            this.grpPendingOrders = new System.Windows.Forms.GroupBox();
            this.dgvPendingOrders = new System.Windows.Forms.DataGridView();
            this.pnlMainContainer.SuspendLayout();
            this.pnlSidebar.SuspendLayout();
            this.grpDispatchConfig.SuspendLayout();
            this.grpPendingOrders.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendingOrders)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlMainContainer
            // 
            this.pnlMainContainer.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlMainContainer.Controls.Add(this.btnGenerateDeliveryNote);
            this.pnlMainContainer.Controls.Add(this.grpDispatchConfig);
            this.pnlMainContainer.Controls.Add(this.grpPendingOrders);
            this.pnlMainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMainContainer.Location = new System.Drawing.Point(220, 0);
            this.pnlMainContainer.Name = "pnlMainContainer";
            this.pnlMainContainer.Size = new System.Drawing.Size(684, 511);
            this.pnlMainContainer.TabIndex = 3;
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
            this.pnlSidebar.TabIndex = 4;
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
            // btnGenerateDeliveryNote
            // 
            this.btnGenerateDeliveryNote.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.btnGenerateDeliveryNote.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerateDeliveryNote.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnGenerateDeliveryNote.ForeColor = System.Drawing.Color.White;
            this.btnGenerateDeliveryNote.Location = new System.Drawing.Point(368, 422);
            this.btnGenerateDeliveryNote.Margin = new System.Windows.Forms.Padding(2);
            this.btnGenerateDeliveryNote.Name = "btnGenerateDeliveryNote";
            this.btnGenerateDeliveryNote.Size = new System.Drawing.Size(292, 42);
            this.btnGenerateDeliveryNote.TabIndex = 5;
            this.btnGenerateDeliveryNote.Text = "Generate Delivery Note && Reply Slip";
            this.btnGenerateDeliveryNote.UseVisualStyleBackColor = false;
            // 
            // grpDispatchConfig
            // 
            this.grpDispatchConfig.Controls.Add(this.lblDispatchID);
            this.grpDispatchConfig.Controls.Add(this.txtDispatchID);
            this.grpDispatchConfig.Controls.Add(this.lblEstDelivery);
            this.grpDispatchConfig.Controls.Add(this.dtpEstDelivery);
            this.grpDispatchConfig.Controls.Add(this.lblMethod);
            this.grpDispatchConfig.Controls.Add(this.cboMethod);
            this.grpDispatchConfig.Controls.Add(this.lblDriverName);
            this.grpDispatchConfig.Controls.Add(this.txtDriverName);
            this.grpDispatchConfig.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpDispatchConfig.Location = new System.Drawing.Point(8, 278);
            this.grpDispatchConfig.Name = "grpDispatchConfig";
            this.grpDispatchConfig.Size = new System.Drawing.Size(669, 128);
            this.grpDispatchConfig.TabIndex = 4;
            this.grpDispatchConfig.TabStop = false;
            this.grpDispatchConfig.Text = "2. 發貨參數設定 (Dispatch Configuration)";
            // 
            // lblDispatchID
            // 
            this.lblDispatchID.Location = new System.Drawing.Point(15, 34);
            this.lblDispatchID.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDispatchID.Name = "lblDispatchID";
            this.lblDispatchID.Size = new System.Drawing.Size(90, 20);
            this.lblDispatchID.TabIndex = 0;
            this.lblDispatchID.Text = "Dispatch ID:";
            // 
            // txtDispatchID
            // 
            this.txtDispatchID.Location = new System.Drawing.Point(146, 31);
            this.txtDispatchID.Margin = new System.Windows.Forms.Padding(2);
            this.txtDispatchID.Name = "txtDispatchID";
            this.txtDispatchID.ReadOnly = true;
            this.txtDispatchID.Size = new System.Drawing.Size(188, 25);
            this.txtDispatchID.TabIndex = 1;
            // 
            // lblEstDelivery
            // 
            this.lblEstDelivery.Location = new System.Drawing.Point(355, 34);
            this.lblEstDelivery.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblEstDelivery.Name = "lblEstDelivery";
            this.lblEstDelivery.Size = new System.Drawing.Size(71, 20);
            this.lblEstDelivery.TabIndex = 2;
            this.lblEstDelivery.Text = "Est Date:";
            // 
            // dtpEstDelivery
            // 
            this.dtpEstDelivery.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpEstDelivery.Location = new System.Drawing.Point(471, 31);
            this.dtpEstDelivery.Margin = new System.Windows.Forms.Padding(2);
            this.dtpEstDelivery.Name = "dtpEstDelivery";
            this.dtpEstDelivery.Size = new System.Drawing.Size(181, 25);
            this.dtpEstDelivery.TabIndex = 3;
            // 
            // lblMethod
            // 
            this.lblMethod.Location = new System.Drawing.Point(15, 82);
            this.lblMethod.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMethod.Name = "lblMethod";
            this.lblMethod.Size = new System.Drawing.Size(127, 20);
            this.lblMethod.TabIndex = 4;
            this.lblMethod.Text = "Delivery Method:";
            // 
            // cboMethod
            // 
            this.cboMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMethod.Location = new System.Drawing.Point(146, 79);
            this.cboMethod.Margin = new System.Windows.Forms.Padding(2);
            this.cboMethod.Name = "cboMethod";
            this.cboMethod.Size = new System.Drawing.Size(188, 25);
            this.cboMethod.TabIndex = 5;
            // 
            // lblDriverName
            // 
            this.lblDriverName.Location = new System.Drawing.Point(355, 82);
            this.lblDriverName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDriverName.Name = "lblDriverName";
            this.lblDriverName.Size = new System.Drawing.Size(112, 20);
            this.lblDriverName.TabIndex = 6;
            this.lblDriverName.Text = "Driver Name:";
            // 
            // txtDriverName
            // 
            this.txtDriverName.Location = new System.Drawing.Point(471, 79);
            this.txtDriverName.Margin = new System.Windows.Forms.Padding(2);
            this.txtDriverName.Name = "txtDriverName";
            this.txtDriverName.Size = new System.Drawing.Size(181, 25);
            this.txtDriverName.TabIndex = 7;
            // 
            // grpPendingOrders
            // 
            this.grpPendingOrders.Controls.Add(this.dgvPendingOrders);
            this.grpPendingOrders.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpPendingOrders.Location = new System.Drawing.Point(8, 46);
            this.grpPendingOrders.Name = "grpPendingOrders";
            this.grpPendingOrders.Size = new System.Drawing.Size(669, 220);
            this.grpPendingOrders.TabIndex = 3;
            this.grpPendingOrders.TabStop = false;
            this.grpPendingOrders.Text = "1. 待發貨訂單列表 (Pending Orders Ready for Dispatch)";
            // 
            // dgvPendingOrders
            // 
            this.dgvPendingOrders.AllowUserToAddRows = false;
            this.dgvPendingOrders.AllowUserToDeleteRows = false;
            this.dgvPendingOrders.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPendingOrders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPendingOrders.Location = new System.Drawing.Point(9, 25);
            this.dgvPendingOrders.Margin = new System.Windows.Forms.Padding(2);
            this.dgvPendingOrders.MultiSelect = false;
            this.dgvPendingOrders.Name = "dgvPendingOrders";
            this.dgvPendingOrders.ReadOnly = true;
            this.dgvPendingOrders.RowHeadersWidth = 51;
            this.dgvPendingOrders.RowTemplate.Height = 27;
            this.dgvPendingOrders.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPendingOrders.Size = new System.Drawing.Size(651, 182);
            this.dgvPendingOrders.TabIndex = 0;
            // 
            // LogisticsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(904, 511);
            this.Controls.Add(this.pnlMainContainer);
            this.Controls.Add(this.pnlSidebar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.Name = "LogisticsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "物流管理 - 生成發貨單 (Logistics Processing System)";
            this.Load += new System.EventHandler(this.LogisticsForm_Load);
            this.pnlMainContainer.ResumeLayout(false);
            this.pnlSidebar.ResumeLayout(false);
            this.grpDispatchConfig.ResumeLayout(false);
            this.grpDispatchConfig.PerformLayout();
            this.grpPendingOrders.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendingOrders)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlMainContainer;
        private System.Windows.Forms.Button btnGenerateDeliveryNote;
        private System.Windows.Forms.GroupBox grpDispatchConfig;
        private System.Windows.Forms.Label lblDispatchID;
        private System.Windows.Forms.TextBox txtDispatchID;
        private System.Windows.Forms.Label lblEstDelivery;
        private System.Windows.Forms.DateTimePicker dtpEstDelivery;
        private System.Windows.Forms.Label lblMethod;
        private System.Windows.Forms.ComboBox cboMethod;
        private System.Windows.Forms.Label lblDriverName;
        private System.Windows.Forms.TextBox txtDriverName;
        private System.Windows.Forms.GroupBox grpPendingOrders;
        private System.Windows.Forms.DataGridView dgvPendingOrders;
        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Button btnAfterService;
        private System.Windows.Forms.Button btnProcurement;
        private System.Windows.Forms.Button btnMaterialRequest;
        private System.Windows.Forms.Button btnGoodsReceived;
        private System.Windows.Forms.Button btnStaff;
        private System.Windows.Forms.Button btnProduct;
        private System.Windows.Forms.Button btnOrder;
        private System.Windows.Forms.Button btnOrderProcessing;
        private System.Windows.Forms.Label lblWelcome;
    }
}