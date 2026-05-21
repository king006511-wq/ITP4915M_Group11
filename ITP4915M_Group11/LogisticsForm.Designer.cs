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
            this.grpPendingOrders.Location = new System.Drawing.Point(12, 12);
            this.grpPendingOrders.Name = "grpPendingOrders";
            this.grpPendingOrders.Size = new System.Drawing.Size(772, 220);
            this.grpPendingOrders.TabIndex = 0;
            this.grpPendingOrders.TabStop = false;
            this.grpPendingOrders.Text = "1. Pending Orders Ready for Dispatch";
            // 
            // dgvPendingOrders
            // 
            this.dgvPendingOrders.AllowUserToAddRows = false;
            this.dgvPendingOrders.AllowUserToDeleteRows = false;
            this.dgvPendingOrders.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPendingOrders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPendingOrders.Location = new System.Drawing.Point(9, 25);
            this.dgvPendingOrders.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dgvPendingOrders.MultiSelect = false;
            this.dgvPendingOrders.Name = "dgvPendingOrders";
            this.dgvPendingOrders.ReadOnly = true;
            this.dgvPendingOrders.RowHeadersWidth = 51;
            this.dgvPendingOrders.RowTemplate.Height = 27;
            this.dgvPendingOrders.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPendingOrders.Size = new System.Drawing.Size(754, 182);
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
            this.grpDispatchConfig.Location = new System.Drawing.Point(12, 244);
            this.grpDispatchConfig.Name = "grpDispatchConfig";
            this.grpDispatchConfig.Size = new System.Drawing.Size(772, 128);
            this.grpDispatchConfig.TabIndex = 1;
            this.grpDispatchConfig.TabStop = false;
            this.grpDispatchConfig.Text = "2. Dispatch Configuration";
            // 
            // lblDispatchID
            // 
            this.lblDispatchID.Location = new System.Drawing.Point(15, 34);
            this.lblDispatchID.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDispatchID.Name = "lblDispatchID";
            this.lblDispatchID.Size = new System.Drawing.Size(165, 20);
            this.lblDispatchID.TabIndex = 0;
            this.lblDispatchID.Text = "Dispatch ID:";
            // 
            // txtDispatchID
            // 
            this.txtDispatchID.Location = new System.Drawing.Point(184, 31);
            this.txtDispatchID.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtDispatchID.Name = "txtDispatchID";
            this.txtDispatchID.ReadOnly = true;
            this.txtDispatchID.Size = new System.Drawing.Size(188, 25);
            this.txtDispatchID.TabIndex = 1;
            // 
            // lblEstDelivery
            // 
            this.lblEstDelivery.Location = new System.Drawing.Point(405, 34);
            this.lblEstDelivery.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblEstDelivery.Name = "lblEstDelivery";
            this.lblEstDelivery.Size = new System.Drawing.Size(165, 20);
            this.lblEstDelivery.TabIndex = 2;
            this.lblEstDelivery.Text = "Est Date:";
            // 
            // dtpEstDelivery
            // 
            this.dtpEstDelivery.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpEstDelivery.Location = new System.Drawing.Point(574, 31);
            this.dtpEstDelivery.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dtpEstDelivery.Name = "dtpEstDelivery";
            this.dtpEstDelivery.Size = new System.Drawing.Size(181, 25);
            this.dtpEstDelivery.TabIndex = 3;
            // 
            // lblMethod
            // 
            this.lblMethod.Location = new System.Drawing.Point(15, 82);
            this.lblMethod.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMethod.Name = "lblMethod";
            this.lblMethod.Size = new System.Drawing.Size(165, 20);
            this.lblMethod.TabIndex = 4;
            this.lblMethod.Text = "Delivery Method:";
            // 
            // cboMethod
            // 
            this.cboMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMethod.Location = new System.Drawing.Point(184, 79);
            this.cboMethod.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cboMethod.Name = "cboMethod";
            this.cboMethod.Size = new System.Drawing.Size(188, 25);
            this.cboMethod.TabIndex = 5;
            // 
            // lblDriverName
            // 
            this.lblDriverName.Location = new System.Drawing.Point(405, 82);
            this.lblDriverName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDriverName.Name = "lblDriverName";
            this.lblDriverName.Size = new System.Drawing.Size(165, 20);
            this.lblDriverName.TabIndex = 6;
            this.lblDriverName.Text = "Driver Name:";
            // 
            // txtDriverName
            // 
            this.txtDriverName.Location = new System.Drawing.Point(574, 79);
            this.txtDriverName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtDriverName.Name = "txtDriverName";
            this.txtDriverName.Size = new System.Drawing.Size(181, 25);
            this.txtDriverName.TabIndex = 7;
            // 
            // btnGenerateDeliveryNote
            // 
            this.btnGenerateDeliveryNote.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.btnGenerateDeliveryNote.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerateDeliveryNote.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnGenerateDeliveryNote.ForeColor = System.Drawing.Color.White;
            this.btnGenerateDeliveryNote.Location = new System.Drawing.Point(372, 388);
            this.btnGenerateDeliveryNote.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnGenerateDeliveryNote.Name = "btnGenerateDeliveryNote";
            this.btnGenerateDeliveryNote.Size = new System.Drawing.Size(412, 42);
            this.btnGenerateDeliveryNote.TabIndex = 2;
            this.btnGenerateDeliveryNote.Text = "Generate Delivery Note && Reply Slip";
            this.btnGenerateDeliveryNote.UseVisualStyleBackColor = false;
            this.btnGenerateDeliveryNote.Click += new System.EventHandler(this.btnGenerateDeliveryNote_Click);
            // 
            // LogisticsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(796, 442);
            this.Controls.Add(this.btnGenerateDeliveryNote);
            this.Controls.Add(this.grpDispatchConfig);
            this.Controls.Add(this.grpPendingOrders);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.Name = "LogisticsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Logistics Processing System";
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