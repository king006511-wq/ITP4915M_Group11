namespace ITP4915M_Group11
{
    partial class productManagement
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
            this.dgvProducts = new System.Windows.Forms.DataGridView();
            this.txtPartID = new System.Windows.Forms.TextBox();
            this.txtPartName = new System.Windows.Forms.TextBox();
            this.txtStockLevel = new System.Windows.Forms.TextBox();
            this.txtDefaultPrice = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.txtDescription = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProducts)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvProducts
            // 
            this.dgvProducts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProducts.Location = new System.Drawing.Point(12, 12);
            this.dgvProducts.Name = "dgvProducts";
            this.dgvProducts.RowHeadersWidth = 62;
            this.dgvProducts.RowTemplate.Height = 31;
            this.dgvProducts.Size = new System.Drawing.Size(407, 167);
            this.dgvProducts.TabIndex = 0;
            this.dgvProducts.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvProducts_CellContentClick);
            // 
            // txtPartID
            // 
            this.txtPartID.Location = new System.Drawing.Point(86, 223);
            this.txtPartID.Name = "txtPartID";
            this.txtPartID.Size = new System.Drawing.Size(100, 29);
            this.txtPartID.TabIndex = 1;
            // 
            // txtPartName
            // 
            this.txtPartName.Location = new System.Drawing.Point(86, 284);
            this.txtPartName.Name = "txtPartName";
            this.txtPartName.Size = new System.Drawing.Size(100, 29);
            this.txtPartName.TabIndex = 2;
            // 
            // txtStockLevel
            // 
            this.txtStockLevel.Location = new System.Drawing.Point(86, 346);
            this.txtStockLevel.Name = "txtStockLevel";
            this.txtStockLevel.Size = new System.Drawing.Size(100, 29);
            this.txtStockLevel.TabIndex = 3;
            // 
            // txtDefaultPrice
            // 
            this.txtDefaultPrice.Location = new System.Drawing.Point(86, 401);
            this.txtDefaultPrice.Name = "txtDefaultPrice";
            this.txtDefaultPrice.Size = new System.Drawing.Size(100, 29);
            this.txtDefaultPrice.TabIndex = 4;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(321, 223);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(163, 55);
            this.button1.TabIndex = 5;
            this.button1.Text = "Add Product";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnAddProduct_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(321, 323);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(163, 52);
            this.button2.TabIndex = 6;
            this.button2.Text = "Update Product";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.btnUpdateProduct_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(321, 419);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(163, 52);
            this.button3.TabIndex = 7;
            this.button3.Text = "Delete Product";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.btnDeleteProduct_Click);
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(86, 471);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(100, 29);
            this.txtDescription.TabIndex = 8;
            // 
            // productManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(923, 666);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtDefaultPrice);
            this.Controls.Add(this.txtStockLevel);
            this.Controls.Add(this.txtPartName);
            this.Controls.Add(this.txtPartID);
            this.Controls.Add(this.dgvProducts);
            this.Name = "productManagement";
            this.Text = "productManagement";
            this.Load += new System.EventHandler(this.Form4_Load_1);
            ((System.ComponentModel.ISupportInitialize)(this.dgvProducts)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvProducts;
        private System.Windows.Forms.TextBox txtPartID;
        private System.Windows.Forms.TextBox txtPartName;
        private System.Windows.Forms.TextBox txtStockLevel;
        private System.Windows.Forms.TextBox txtDefaultPrice;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox txtDescription;
    }
}