namespace ITP4915M_Group11
{
    partial class ProductManagement
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblWelcome = new System.Windows.Forms.Label();
            this.btnOrderProcessing = new System.Windows.Forms.Button();
            this.btnOrder = new System.Windows.Forms.Button();
            this.btnProduct = new System.Windows.Forms.Button();
            this.btnStaff = new System.Windows.Forms.Button();
            this.btnGoodsReceived = new System.Windows.Forms.Button();
            this.btnMaterialRequest = new System.Windows.Forms.Button();
            this.btnProcurement = new System.Windows.Forms.Button();
            this.btnAfterService = new System.Windows.Forms.Button();
            this.btnLogout = new System.Windows.Forms.Button();
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.dgvProducts = new System.Windows.Forms.DataGridView();
            this.txtPartID = new System.Windows.Forms.TextBox();
            this.txtPartName = new System.Windows.Forms.TextBox();
            this.txtStockLevel = new System.Windows.Forms.TextBox();
            this.txtDefaultPrice = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.pnlMainContainer = new System.Windows.Forms.Panel();
            this.pnlSidebar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProducts)).BeginInit();
            this.pnlMainContainer.SuspendLayout();
            this.SuspendLayout();
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
            this.pnlSidebar.TabIndex = 10;
            // 
            // dgvProducts
            // 
            this.dgvProducts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProducts.Location = new System.Drawing.Point(174, 34);
            this.dgvProducts.Margin = new System.Windows.Forms.Padding(2);
            this.dgvProducts.Name = "dgvProducts";
            this.dgvProducts.RowHeadersWidth = 62;
            this.dgvProducts.RowTemplate.Height = 31;
            this.dgvProducts.Size = new System.Drawing.Size(271, 111);
            this.dgvProducts.TabIndex = 9;
            // 
            // txtPartID
            // 
            this.txtPartID.Location = new System.Drawing.Point(190, 177);
            this.txtPartID.Margin = new System.Windows.Forms.Padding(2);
            this.txtPartID.Name = "txtPartID";
            this.txtPartID.Size = new System.Drawing.Size(68, 22);
            this.txtPartID.TabIndex = 10;
            // 
            // txtPartName
            // 
            this.txtPartName.Location = new System.Drawing.Point(190, 217);
            this.txtPartName.Margin = new System.Windows.Forms.Padding(2);
            this.txtPartName.Name = "txtPartName";
            this.txtPartName.Size = new System.Drawing.Size(68, 22);
            this.txtPartName.TabIndex = 11;
            // 
            // txtStockLevel
            // 
            this.txtStockLevel.Location = new System.Drawing.Point(190, 259);
            this.txtStockLevel.Margin = new System.Windows.Forms.Padding(2);
            this.txtStockLevel.Name = "txtStockLevel";
            this.txtStockLevel.Size = new System.Drawing.Size(68, 22);
            this.txtStockLevel.TabIndex = 12;
            // 
            // txtDefaultPrice
            // 
            this.txtDefaultPrice.Location = new System.Drawing.Point(190, 295);
            this.txtDefaultPrice.Margin = new System.Windows.Forms.Padding(2);
            this.txtDefaultPrice.Name = "txtDefaultPrice";
            this.txtDefaultPrice.Size = new System.Drawing.Size(68, 22);
            this.txtDefaultPrice.TabIndex = 13;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(347, 177);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(109, 37);
            this.button1.TabIndex = 14;
            this.button1.Text = "Add Product";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(347, 243);
            this.button2.Margin = new System.Windows.Forms.Padding(2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(109, 35);
            this.button2.TabIndex = 15;
            this.button2.Text = "Update Product";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(347, 307);
            this.button3.Margin = new System.Windows.Forms.Padding(2);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(109, 35);
            this.button3.TabIndex = 16;
            this.button3.Text = "Delete Product";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(190, 342);
            this.txtDescription.Margin = new System.Windows.Forms.Padding(2);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(68, 22);
            this.txtDescription.TabIndex = 17;
            // 
            // pnlMainContainer
            // 
            this.pnlMainContainer.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlMainContainer.Controls.Add(this.txtDescription);
            this.pnlMainContainer.Controls.Add(this.button3);
            this.pnlMainContainer.Controls.Add(this.button2);
            this.pnlMainContainer.Controls.Add(this.button1);
            this.pnlMainContainer.Controls.Add(this.txtDefaultPrice);
            this.pnlMainContainer.Controls.Add(this.txtStockLevel);
            this.pnlMainContainer.Controls.Add(this.txtPartName);
            this.pnlMainContainer.Controls.Add(this.txtPartID);
            this.pnlMainContainer.Controls.Add(this.dgvProducts);
            this.pnlMainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMainContainer.Location = new System.Drawing.Point(220, 0);
            this.pnlMainContainer.Name = "pnlMainContainer";
            this.pnlMainContainer.Size = new System.Drawing.Size(684, 511);
            this.pnlMainContainer.TabIndex = 9;
            // 
            // ProductManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(904, 511);
            this.Controls.Add(this.pnlMainContainer);
            this.Controls.Add(this.pnlSidebar);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ProductManagement";
            this.Text = "ProductManagement";
            this.Load += new System.EventHandler(this.Form4_Load_1);
            this.pnlSidebar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvProducts)).EndInit();
            this.pnlMainContainer.ResumeLayout(false);
            this.pnlMainContainer.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblWelcome;
        private System.Windows.Forms.Button btnOrderProcessing;
        private System.Windows.Forms.Button btnOrder;
        private System.Windows.Forms.Button btnProduct;
        private System.Windows.Forms.Button btnStaff;
        private System.Windows.Forms.Button btnGoodsReceived;
        private System.Windows.Forms.Button btnMaterialRequest;
        private System.Windows.Forms.Button btnProcurement;
        private System.Windows.Forms.Button btnAfterService;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.DataGridView dgvProducts;
        private System.Windows.Forms.TextBox txtPartID;
        private System.Windows.Forms.TextBox txtPartName;
        private System.Windows.Forms.TextBox txtStockLevel;
        private System.Windows.Forms.TextBox txtDefaultPrice;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Panel pnlMainContainer;
    }
}