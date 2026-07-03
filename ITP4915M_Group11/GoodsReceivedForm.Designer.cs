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
            this.SuspendLayout();
            // 
            // GoodsReceivedForm
            // 
            this.ClientSize = new System.Drawing.Size(904, 511);
            this.Name = "GoodsReceivedForm";
            this.Text = "Goods Received";
            this.ResumeLayout(false);

        }
    }
}
