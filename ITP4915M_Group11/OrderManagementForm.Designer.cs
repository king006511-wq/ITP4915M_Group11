using System;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    partial class OrderManagementForm
    {
        private System.ComponentModel.IContainer components = null;

        private TextBox txtOrderID;
        private TextBox txtCustomerID;
        private TextBox txtStaffID;
        private TextBox txtUnitPrice;
        private TextBox txtQty;
        private TextBox txtSubtotal;
        private ComboBox cboProducts;
        private DataGridView dgvOrders;
        private Button btnCreateOrder;

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
            this.txtOrderID = new TextBox();
            this.txtCustomerID = new TextBox();
            this.txtStaffID = new TextBox();
            this.txtUnitPrice = new TextBox();
            this.txtQty = new TextBox();
            this.txtSubtotal = new TextBox();
            this.cboProducts = new ComboBox();
            this.dgvOrders = new DataGridView();
            this.btnCreateOrder = new Button();

            ((System.ComponentModel.ISupportInitialize)(this.dgvOrders)).BeginInit();
            this.SuspendLayout();

            // cboProducts
            this.cboProducts.Location = new System.Drawing.Point(12, 40);
            this.cboProducts.Name = "cboProducts";
            this.cboProducts.Size = new System.Drawing.Size(240, 24);
            this.cboProducts.SelectedIndexChanged += new EventHandler(this.cboProducts_SelectedIndexChanged);

            // txtUnitPrice
            this.txtUnitPrice.Location = new System.Drawing.Point(12, 70);
            this.txtUnitPrice.Name = "txtUnitPrice";
            this.txtUnitPrice.Size = new System.Drawing.Size(100, 22);

            // txtQty
            this.txtQty.Location = new System.Drawing.Point(12, 98);
            this.txtQty.Name = "txtQty";
            this.txtQty.Size = new System.Drawing.Size(100, 22);
            this.txtQty.TextChanged += new EventHandler(this.txtQty_TextChanged);

            // txtSubtotal
            this.txtSubtotal.Location = new System.Drawing.Point(12, 126);
            this.txtSubtotal.Name = "txtSubtotal";
            this.txtSubtotal.Size = new System.Drawing.Size(100, 22);

            // txtOrderID
            this.txtOrderID.Location = new System.Drawing.Point(12, 12);
            this.txtOrderID.Name = "txtOrderID";
            this.txtOrderID.Size = new System.Drawing.Size(240, 22);

            // txtCustomerID
            this.txtCustomerID.Location = new System.Drawing.Point(12, 154);
            this.txtCustomerID.Name = "txtCustomerID";
            this.txtCustomerID.Size = new System.Drawing.Size(240, 22);

            // txtStaffID
            this.txtStaffID.Location = new System.Drawing.Point(12, 182);
            this.txtStaffID.Name = "txtStaffID";
            this.txtStaffID.Size = new System.Drawing.Size(240, 22);

            // dgvOrders
            this.dgvOrders.Location = new System.Drawing.Point(270, 12);
            this.dgvOrders.Name = "dgvOrders";
            this.dgvOrders.Size = new System.Drawing.Size(600, 300);

            // btnCreateOrder
            this.btnCreateOrder.Location = new System.Drawing.Point(12, 210);
            this.btnCreateOrder.Name = "btnCreateOrder";
            this.btnCreateOrder.Size = new System.Drawing.Size(120, 30);
            this.btnCreateOrder.Text = "建立訂單";
            this.btnCreateOrder.Click += new EventHandler(this.btnCreateOrder_Click);

            // OrderManagementForm
            this.ClientSize = new System.Drawing.Size(900, 330);
            this.Controls.Add(this.cboProducts);
            this.Controls.Add(this.txtUnitPrice);
            this.Controls.Add(this.txtQty);
            this.Controls.Add(this.txtSubtotal);
            this.Controls.Add(this.txtOrderID);
            this.Controls.Add(this.txtCustomerID);
            this.Controls.Add(this.txtStaffID);
            this.Controls.Add(this.dgvOrders);
            this.Controls.Add(this.btnCreateOrder);
            this.Name = "OrderManagementForm";
            this.Text = "Order Management";
            this.Load += new EventHandler(this.OrderManagementForm_Load);

            ((System.ComponentModel.ISupportInitialize)(this.dgvOrders)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
