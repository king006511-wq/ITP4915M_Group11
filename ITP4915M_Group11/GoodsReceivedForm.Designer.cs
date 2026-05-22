using System;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    partial class GoodsReceivedForm
    {
        /// <summary>
        /// 設計工具產生的元件
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // UI controls required by the code
        private TextBox txtGRNID;
        private TextBox txtPOID;
        private TextBox txtPartID;
        private TextBox txtQty;
        private TextBox txtStaffResource;
        private DataGridView dgvPOItems;
        private Button btnConfirmReceive;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 初始化表單元件。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.txtGRNID = new System.Windows.Forms.TextBox();
            this.txtPOID = new System.Windows.Forms.TextBox();
            this.txtPartID = new System.Windows.Forms.TextBox();
            this.txtQty = new System.Windows.Forms.TextBox();
            this.txtStaffResource = new System.Windows.Forms.TextBox();
            this.dgvPOItems = new System.Windows.Forms.DataGridView();
            this.btnConfirmReceive = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPOItems)).BeginInit();
            this.SuspendLayout();

            // txtGRNID
            this.txtGRNID.Location = new System.Drawing.Point(12, 12);
            this.txtGRNID.Name = "txtGRNID";
            this.txtGRNID.Size = new System.Drawing.Size(240, 22);

            // txtPOID
            this.txtPOID.Location = new System.Drawing.Point(12, 40);
            this.txtPOID.Name = "txtPOID";
            this.txtPOID.Size = new System.Drawing.Size(240, 22);

            // txtPartID
            this.txtPartID.Location = new System.Drawing.Point(12, 68);
            this.txtPartID.Name = "txtPartID";
            this.txtPartID.Size = new System.Drawing.Size(240, 22);

            // txtQty
            this.txtQty.Location = new System.Drawing.Point(12, 96);
            this.txtQty.Name = "txtQty";
            this.txtQty.Size = new System.Drawing.Size(100, 22);

            // txtStaffResource
            this.txtStaffResource.Location = new System.Drawing.Point(12, 124);
            this.txtStaffResource.Name = "txtStaffResource";
            this.txtStaffResource.Size = new System.Drawing.Size(240, 22);

            // dgvPOItems
            this.dgvPOItems.Location = new System.Drawing.Point(270, 12);
            this.dgvPOItems.Name = "dgvPOItems";
            this.dgvPOItems.Size = new System.Drawing.Size(600, 300);
            this.dgvPOItems.AllowUserToAddRows = false;
            this.dgvPOItems.AllowUserToDeleteRows = false;
            this.dgvPOItems.ReadOnly = true;
            this.dgvPOItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvPOItems.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvPOItems_CellClick);

            // btnConfirmReceive
            this.btnConfirmReceive.Location = new System.Drawing.Point(12, 160);
            this.btnConfirmReceive.Name = "btnConfirmReceive";
            this.btnConfirmReceive.Size = new System.Drawing.Size(120, 30);
            this.btnConfirmReceive.Text = "確認收貨";
            this.btnConfirmReceive.UseVisualStyleBackColor = true;
            this.btnConfirmReceive.Click += new System.EventHandler(this.btnConfirmReceive_Click);

            // GoodsReceivedForm
            this.ClientSize = new System.Drawing.Size(900, 330);
            this.Controls.Add(this.txtGRNID);
            this.Controls.Add(this.txtPOID);
            this.Controls.Add(this.txtPartID);
            this.Controls.Add(this.txtQty);
            this.Controls.Add(this.txtStaffResource);
            this.Controls.Add(this.dgvPOItems);
            this.Controls.Add(this.btnConfirmReceive);
            this.Name = "GoodsReceivedForm";
            this.Text = "Goods Received";
            this.Load += new System.EventHandler(this.GoodsReceivedForm_Load);

            ((System.ComponentModel.ISupportInitialize)(this.dgvPOItems)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
