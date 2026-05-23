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
            this.grpPendingOrders = new System.Windows.Forms.GroupBox();
            this.dgvPendingOrders = new System.Windows.Forms.DataGridView();
            this.grpDispatchConfig = new System.Windows.Forms.GroupBox();
            this.lblDispatchID = new System.Windows.Forms.Label();
            this.txtDispatchID = new System.Windows.Forms.TextBox();
            this.lblEstDelivery = new System.Windows.Forms.Label();
            this.dtpEstDelivery = new System.Windows.Forms.DateTimePicker();
            this.lblMethod = new System.Windows.Forms.Label();
            this.cboMethod = new System.Windows.Forms.ComboBox();
            this.lblDriverName = new System.Windows.Forms.Label();
            this.txtDriverName = new System.Windows.Forms.TextBox();
            this.btnGenerateDeliveryNote = new System.Windows.Forms.Button();
            this.grpPendingOrders.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendingOrders)).BeginInit();
            this.grpDispatchConfig.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpPendingOrders
            // 
            this.grpPendingOrders.Controls.Add(this.dgvPendingOrders);
            this.grpPendingOrders.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpPendingOrders.Location = new System.Drawing.Point(16, 15);
            this.grpPendingOrders.Margin = new System.Windows.Forms.Padding(4);
            this.grpPendingOrders.Name = "grpPendingOrders";
            this.grpPendingOrders.Padding = new System.Windows.Forms.Padding(4);
            this.grpPendingOrders.Size = new System.Drawing.Size(1030, 275);
            this.grpPendingOrders.TabIndex = 0;
            this.grpPendingOrders.TabStop = false;
            this.grpPendingOrders.Text = "1. 待發貨訂單列表 (Pending Orders Ready for Dispatch)";
            // 
            // dgvPendingOrders
            // 
            this.dgvPendingOrders.AllowUserToAddRows = false;
            this.dgvPendingOrders.AllowUserToDeleteRows = false;
            this.dgvPendingOrders.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPendingOrders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPendingOrders.Location = new System.Drawing.Point(12, 31);
            this.dgvPendingOrders.MultiSelect = false;
            this.dgvPendingOrders.Name = "dgvPendingOrders";
            this.dgvPendingOrders.ReadOnly = true;
            this.dgvPendingOrders.RowHeadersWidth = 51;
            this.dgvPendingOrders.RowTemplate.Height = 27;
            this.dgvPendingOrders.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPendingOrders.Size = new System.Drawing.Size(1005, 227);
            this.dgvPendingOrders.TabIndex = 0;
            this.dgvPendingOrders.SelectionChanged += new System.EventHandler(this.dgvPendingOrders_SelectionChanged);
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
            this.grpDispatchConfig.Location = new System.Drawing.Point(16, 305);
            this.grpDispatchConfig.Margin = new System.Windows.Forms.Padding(4);
            this.grpDispatchConfig.Name = "grpDispatchConfig";
            this.grpDispatchConfig.Padding = new System.Windows.Forms.Padding(4);
            this.grpDispatchConfig.Size = new System.Drawing.Size(1030, 160);
            this.grpDispatchConfig.TabIndex = 1;
            this.grpDispatchConfig.TabStop = false;
            this.grpDispatchConfig.Text = "2. 發貨參數設定 (Dispatch Configuration)";
            // 
            // lblDispatchID
            // 
            this.lblDispatchID.Location = new System.Drawing.Point(20, 42);
            this.lblDispatchID.Name = "lblDispatchID";
            this.lblDispatchID.Size = new System.Drawing.Size(220, 25);
            this.lblDispatchID.TabIndex = 0;
            this.lblDispatchID.Text = "發貨單號 (Dispatch ID):";
            // 
            // txtDispatchID
            // 
            this.txtDispatchID.Location = new System.Drawing.Point(245, 39);
            this.txtDispatchID.Name = "txtDispatchID";
            this.txtDispatchID.ReadOnly = true;
            this.txtDispatchID.Size = new System.Drawing.Size(250, 30);
            this.txtDispatchID.TabIndex = 1;
            // 
            // lblEstDelivery
            // 
            this.lblEstDelivery.Location = new System.Drawing.Point(540, 42);
            this.lblEstDelivery.Name = "lblEstDelivery";
            this.lblEstDelivery.Size = new System.Drawing.Size(220, 25);
            this.lblEstDelivery.TabIndex = 2;
            this.lblEstDelivery.Text = "預計送貨日期 (Est Date):";
            // 
            // dtpEstDelivery
            // 
            this.dtpEstDelivery.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpEstDelivery.Location = new System.Drawing.Point(765, 39);
            this.dtpEstDelivery.Name = "dtpEstDelivery";
            this.dtpEstDelivery.Size = new System.Drawing.Size(240, 30);
            this.dtpEstDelivery.TabIndex = 3;
            // 
            // lblMethod
            // 
            this.lblMethod.Location = new System.Drawing.Point(20, 102);
            this.lblMethod.Name = "lblMethod";
            this.lblMethod.Size = new System.Drawing.Size(220, 25);
            this.lblMethod.TabIndex = 4;
            this.lblMethod.Text = "送貨方式 (Delivery Method):";
            // 
            // cboMethod
            // 
            this.cboMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMethod.Location = new System.Drawing.Point(245, 99);
            this.cboMethod.Name = "cboMethod";
            this.cboMethod.Size = new System.Drawing.Size(250, 31);
            this.cboMethod.TabIndex = 5;
            // 
            // lblDriverName
            // 
            this.lblDriverName.Location = new System.Drawing.Point(540, 102);
            this.lblDriverName.Name = "lblDriverName";
            this.lblDriverName.Size = new System.Drawing.Size(220, 25);
            this.lblDriverName.TabIndex = 6;
            this.lblDriverName.Text = "司機姓名 (Driver Name):";
            // 
            // txtDriverName
            // 
            this.txtDriverName.Location = new System.Drawing.Point(765, 99);
            this.txtDriverName.Name = "txtDriverName";
            this.txtDriverName.Size = new System.Drawing.Size(240, 30);
            this.txtDriverName.TabIndex = 7;
            // 
            // btnGenerateDeliveryNote
            // 
            this.btnGenerateDeliveryNote.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.btnGenerateDeliveryNote.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerateDeliveryNote.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnGenerateDeliveryNote.ForeColor = System.Drawing.Color.White;
            this.btnGenerateDeliveryNote.Location = new System.Drawing.Point(496, 485);
            this.btnGenerateDeliveryNote.Name = "btnGenerateDeliveryNote";
            this.btnGenerateDeliveryNote.Size = new System.Drawing.Size(550, 52);
            this.btnGenerateDeliveryNote.TabIndex = 2;
            this.btnGenerateDeliveryNote.Text = "生成發貨單與回條 (Generate Delivery Note && Reply Slip)";
            this.btnGenerateDeliveryNote.UseVisualStyleBackColor = false;
            this.btnGenerateDeliveryNote.Click += new System.EventHandler(this.btnGenerateDeliveryNote_Click);
            // 
            // LogisticsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1062, 553);
            this.Controls.Add(this.btnGenerateDeliveryNote);
            this.Controls.Add(this.grpDispatchConfig);
            this.Controls.Add(this.grpPendingOrders);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "LogisticsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "物流管理 - 生成發貨單 (Logistics Processing System)";
            this.Load += new System.EventHandler(this.LogisticsForm_Load);
            this.grpPendingOrders.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendingOrders)).EndInit();
            this.grpDispatchConfig.ResumeLayout(false);
            this.grpDispatchConfig.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpPendingOrders;
        private System.Windows.Forms.DataGridView dgvPendingOrders;
        private System.Windows.Forms.GroupBox grpDispatchConfig;
        private System.Windows.Forms.Label lblDispatchID;
        private System.Windows.Forms.TextBox txtDispatchID;
        private System.Windows.Forms.Label lblEstDelivery;
        private System.Windows.Forms.DateTimePicker dtpEstDelivery;
        private System.Windows.Forms.Label lblMethod;
        private System.Windows.Forms.ComboBox cboMethod;
        private System.Windows.Forms.Label lblDriverName;
        private System.Windows.Forms.TextBox txtDriverName;
        private System.Windows.Forms.Button btnGenerateDeliveryNote;
    }
}