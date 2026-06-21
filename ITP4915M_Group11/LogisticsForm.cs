using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;

namespace ITP4915M_Group11
{
    public partial class LogisticsForm : Form
    {
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";
        private string currentStaffID;

        private DataGridView dgvPendingOrders;
        private TextBox txtOrderID, txtCustomerID, txtDeliveryAddress, txtCurrentStatus;
        private ComboBox cboDeliveryStaff;
        private DateTimePicker dtpScheduleDate;
        private Button btnAssignDelivery, btnUpdateStatus, btnGenerateNote, btnClearFields;

        public LogisticsForm()
        {
            this.currentStaffID = string.IsNullOrEmpty(UserSession.LoggedInStaffID) ? "S001" : UserSession.LoggedInStaffID;

            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                InitializeVibrantModernUI();
                LoadDeliveryStaff();
                RefreshPendingOrdersGrid();
            }
        }

        #region 🎨 Dynamic VIBRANT Modern UI Construction
        private void InitializeVibrantModernUI()
        {
            this.Controls.Clear();
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.FormBorderStyle = FormBorderStyle.None;

            TableLayoutPanel mainTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent };
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.Controls.Add(mainTable);

            Panel pnlHeader = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            pnlHeader.Paint += (s, e) => {
                using (LinearGradientBrush brush = new LinearGradientBrush(pnlHeader.ClientRectangle, Color.FromArgb(79, 70, 229), Color.FromArgb(217, 70, 239), 45F))
                {
                    e.Graphics.FillRectangle(brush, pnlHeader.ClientRectangle);
                }
            };

            Label lblModuleTitle = new Label { Text = "⚡ Logistics Dispatch & Delivery Management", Font = new Font("Segoe UI Black", 18F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Transparent, Location = new Point(20, 18), AutoSize = true };
            pnlHeader.Controls.Add(lblModuleTitle);
            mainTable.Controls.Add(pnlHeader, 0, 0);

            TableLayoutPanel contentTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = new Padding(20) };
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainTable.Controls.Add(contentTable, 0, 1);

            Panel pnlInputs = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, AutoScroll = true, Padding = new Padding(20) };
            pnlInputs.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlInputs.ClientRectangle, Color.FromArgb(229, 231, 235), ButtonBorderStyle.Solid);
            contentTable.Controls.Add(pnlInputs, 0, 0);

            dgvPendingOrders = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, Font = new Font("Segoe UI", 10F), ReadOnly = true, EnableHeadersVisualStyles = false };
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(14, 165, 233);
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPendingOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvPendingOrders.ColumnHeadersHeight = 40;
            dgvPendingOrders.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);
            dgvPendingOrders.SelectionChanged += dgvPendingOrders_SelectionChanged;

            Panel gridContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1), BackColor = Color.FromArgb(229, 231, 235) };
            gridContainer.Controls.Add(dgvPendingOrders);
            contentTable.Controls.Add(gridContainer, 2, 0);

            int currentY = 15;
            int inputWidth = 300;

            Label lblOrderID = new Label { Text = "Target Order ID", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(107, 114, 128), Location = new Point(15, currentY), AutoSize = true };
            txtOrderID = new TextBox { Location = new Point(15, currentY + 25), Size = new Size(inputWidth, 30), Font = new Font("Segoe UI", 10F), ReadOnly = true, BackColor = Color.FromArgb(243, 244, 246) };
            pnlInputs.Controls.Add(lblOrderID); pnlInputs.Controls.Add(txtOrderID);
            currentY += 65;

            Label lblCust = new Label { Text = "Customer ID", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(107, 114, 128), Location = new Point(15, currentY), AutoSize = true };
            txtCustomerID = new TextBox { Location = new Point(15, currentY + 25), Size = new Size(inputWidth, 30), Font = new Font("Segoe UI", 10F), ReadOnly = true, BackColor = Color.White };
            pnlInputs.Controls.Add(lblCust); pnlInputs.Controls.Add(txtCustomerID);
            currentY += 65;

            Label lblAddress = new Label { Text = "Destination Address", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(107, 114, 128), Location = new Point(15, currentY), AutoSize = true };
            txtDeliveryAddress = new TextBox { Location = new Point(15, currentY + 25), Size = new Size(inputWidth, 55), Font = new Font("Segoe UI", 10F), Multiline = true, ReadOnly = true, BackColor = Color.White };
            pnlInputs.Controls.Add(lblAddress); pnlInputs.Controls.Add(txtDeliveryAddress);
            currentY += 95;

            Label lblStatus = new Label { Text = "Current State", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(107, 114, 128), Location = new Point(15, currentY), AutoSize = true };
            txtCurrentStatus = new TextBox { Location = new Point(15, currentY + 25), Size = new Size(inputWidth, 30), Font = new Font("Segoe UI", 10F), ReadOnly = true, BackColor = Color.FromArgb(243, 244, 246) };
            pnlInputs.Controls.Add(lblStatus); pnlInputs.Controls.Add(txtCurrentStatus);
            currentY += 65;

            Label lblStaff = new Label { Text = "Assign Delivery Team *", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(107, 114, 128), Location = new Point(15, currentY), AutoSize = true };
            cboDeliveryStaff = new ComboBox { Location = new Point(15, currentY + 25), Size = new Size(inputWidth, 30), Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };
            pnlInputs.Controls.Add(lblStaff); pnlInputs.Controls.Add(cboDeliveryStaff);
            currentY += 65;

            Label lblDate = new Label { Text = "Scheduled Date *", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(107, 114, 128), Location = new Point(15, currentY), AutoSize = true };
            dtpScheduleDate = new DateTimePicker { Location = new Point(15, currentY + 25), Size = new Size(inputWidth, 30), Font = new Font("Segoe UI", 10F), Format = DateTimePickerFormat.Short };
            pnlInputs.Controls.Add(lblDate); pnlInputs.Controls.Add(dtpScheduleDate);
            currentY += 75;

            btnAssignDelivery = new Button { Text = "🚀 Dispatch Order", Location = new Point(15, currentY), Size = new Size(140, 40), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(245, 158, 11), Cursor = Cursors.Hand };
            btnAssignDelivery.FlatAppearance.BorderSize = 0;
            btnAssignDelivery.Click += btnAssignDelivery_Click;

            btnUpdateStatus = new Button { Text = "✅ Mark Delivered", Location = new Point(175, currentY), Size = new Size(140, 40), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(16, 185, 129), Cursor = Cursors.Hand };
            btnUpdateStatus.FlatAppearance.BorderSize = 0;
            btnUpdateStatus.Click += btnUpdateStatus_Click;
            currentY += 50;

            btnGenerateNote = new Button { Text = "📄 Preview Note & Reply Slip", Location = new Point(15, currentY), Size = new Size(300, 45), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(236, 72, 153), Cursor = Cursors.Hand };
            btnGenerateNote.FlatAppearance.BorderSize = 0;
            btnGenerateNote.Click += btnGenerateNote_Click;
            currentY += 55;

            btnClearFields = new Button { Text = "🔄 Reset Form", Location = new Point(15, currentY), Size = new Size(300, 40), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), BackColor = Color.FromArgb(226, 232, 240), Cursor = Cursors.Hand };
            btnClearFields.FlatAppearance.BorderSize = 0;
            btnClearFields.Click += (s, e) => ClearLogisticsFields();

            pnlInputs.Controls.Add(btnAssignDelivery);
            pnlInputs.Controls.Add(btnUpdateStatus);
            pnlInputs.Controls.Add(btnGenerateNote);
            pnlInputs.Controls.Add(btnClearFields);
        }
        #endregion

        #region ⚙️ Operational Logic
        private void LoadDeliveryStaff()
        {
            cboDeliveryStaff.Items.Clear();
            cboDeliveryStaff.Items.Add("Team A - Fast Track");
            cboDeliveryStaff.Items.Add("Team B - Heavy Goods");
            cboDeliveryStaff.Items.Add("Team C - Weekend Special");
            cboDeliveryStaff.Items.Add("Outsource - SF Express Elite");
        }

        private void LogisticsForm_Load(object sender, EventArgs e) { }

        private void RefreshPendingOrdersGrid()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT OrderID AS 'Order ID', CustomerID AS 'Customer', Status, OrderDate AS 'Date' FROM orders WHERE Status != 'Delivery Completed' ORDER BY OrderDate DESC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvPendingOrders.DataSource = dt;
                    }
                }
                catch (Exception) { }
            }
        }

        private void dgvPendingOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPendingOrders.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvPendingOrders.SelectedRows[0];
                txtOrderID.Text = row.Cells["Order ID"].Value?.ToString() ?? "";
                txtCustomerID.Text = row.Cells["Customer"].Value?.ToString() ?? "";
                txtCurrentStatus.Text = row.Cells["Status"].Value?.ToString() ?? "";
                txtDeliveryAddress.Text = "Standard Registered Address (Please verify with client)";
            }
        }

        private void btnAssignDelivery_Click(object sender, EventArgs e)
        {
            string orderID = txtOrderID.Text.Trim();
            if (string.IsNullOrEmpty(orderID) || cboDeliveryStaff.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an Order and assign a Delivery Team first.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string updateSql = "UPDATE orders SET Status = 'Dispatched' WHERE OrderID = @OrderID";
                    using (MySqlCommand cmd = new MySqlCommand(updateSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderID);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show($"Order [{orderID}] has been successfully dispatched.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearLogisticsFields();
                        RefreshPendingOrdersGrid();
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            string orderID = txtOrderID.Text.Trim();
            if (string.IsNullOrEmpty(orderID)) return;

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string updateSql = "UPDATE orders SET Status = 'Delivery Completed' WHERE OrderID = @OrderID";
                    using (MySqlCommand cmd = new MySqlCommand(updateSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderID);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show($"Order [{orderID}] marked as delivered.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearLogisticsFields();
                        RefreshPendingOrdersGrid();
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void btnGenerateNote_Click(object sender, EventArgs e)
        {
            string orderID = txtOrderID.Text.Trim();
            if (string.IsNullOrEmpty(orderID))
            {
                MessageBox.Show("Please select an active order from the dashboard to display preview.", "System Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string deliveryStaff = cboDeliveryStaff.SelectedIndex != -1 ? cboDeliveryStaff.Text : "Unassigned Team";
            string scheduleDate = dtpScheduleDate.Value.ToString("MMMM dd, yyyy");
            string customerID = txtCustomerID.Text.Trim();
            string address = txtDeliveryAddress.Text.Trim();

            Form previewForm = new Form { Text = "Interactive Document Hub", Size = new Size(650, 850), StartPosition = FormStartPosition.CenterParent, BackColor = Color.FromArgb(243, 244, 246), FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false };

            Panel pnlPreviewHeader = new Panel { Dock = DockStyle.Top, Height = 60 };
            pnlPreviewHeader.Paint += (s, ev) => {
                using (LinearGradientBrush brush = new LinearGradientBrush(pnlPreviewHeader.ClientRectangle, Color.FromArgb(236, 72, 153), Color.FromArgb(245, 158, 11), 0F))
                {
                    ev.Graphics.FillRectangle(brush, pnlPreviewHeader.ClientRectangle);
                }
            };
            Label lblTitle = new Label { Text = "📄 DIGITAL MANIFEST PREVIEW", Font = new Font("Segoe UI Black", 14F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Transparent, Location = new Point(20, 15), AutoSize = true };
            pnlPreviewHeader.Controls.Add(lblTitle);
            previewForm.Controls.Add(pnlPreviewHeader);

            Panel scrollContainer = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(30, 20, 30, 100) };
            previewForm.Controls.Add(scrollContainer);
            scrollContainer.BringToFront();

            Panel docSheet = new Panel { Width = 550, Height = 650, Location = new Point(35, 20), BackColor = Color.White };
            scrollContainer.Controls.Add(docSheet);

            Panel docTopBanner = new Panel { Dock = DockStyle.Top, Height = 10, BackColor = Color.FromArgb(14, 165, 233) };
            docSheet.Controls.Add(docTopBanner);

            Label lblBrand = new Label { Text = "PREMIUM LIVING FURNITURE", Font = new Font("Segoe UI Black", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(30, 58, 138), Location = new Point(30, 35), AutoSize = true };
            Label lblDocType1 = new Label { Text = "OFFICIAL DELIVERY NOTE", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(14, 165, 233), Location = new Point(30, 65), AutoSize = true };
            docSheet.Controls.Add(lblBrand); docSheet.Controls.Add(lblDocType1);

            int currentMetaY = 110;
            string[] metadataLabels = { "Order Ref:", "Customer ID:", "Target Address:", "Scheduled For:", "Logistics Team:" };
            string[] metadataValues = { orderID, customerID, address, scheduleDate, deliveryStaff };

            for (int i = 0; i < metadataLabels.Length; i++)
            {
                Label lblMeta = new Label { Text = metadataLabels[i], Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(107, 114, 128), Location = new Point(30, currentMetaY), AutoSize = true };
                Label lblVal = new Label { Text = metadataValues[i], Font = new Font("Segoe UI", 10F, FontStyle.Regular), ForeColor = Color.FromArgb(17, 24, 39), Location = new Point(180, currentMetaY), AutoSize = true };
                docSheet.Controls.Add(lblMeta); docSheet.Controls.Add(lblVal);
                currentMetaY += 32;
            }

            Panel pnlDivider = new Panel { Location = new Point(30, 290), Size = new Size(490, 2), BackColor = Color.FromArgb(229, 231, 235) };
            docSheet.Controls.Add(pnlDivider);

            Panel pnlReplySlipCard = new Panel { Location = new Point(30, 320), Size = new Size(490, 300), BackColor = Color.FromArgb(254, 252, 232), Padding = new Padding(20) };
            pnlReplySlipCard.Paint += (s, paintArgs) => ControlPaint.DrawBorder(paintArgs.Graphics, pnlReplySlipCard.ClientRectangle, Color.FromArgb(253, 224, 71), ButtonBorderStyle.Solid);
            docSheet.Controls.Add(pnlReplySlipCard);

            Label lblSlipTitle = new Label { Text = "📌 CUSTOMER REPLY SLIP", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(161, 98, 7), Location = new Point(20, 20), AutoSize = true };
            Label lblSlipDesc = new Label { Text = $"I hereby confirm receipt of items for Order [{orderID}] in pristine condition.", Font = new Font("Segoe UI", 10F), ForeColor = Color.FromArgb(113, 63, 18), Location = new Point(20, 50), Size = new Size(450, 45), AutoSize = false };
            pnlReplySlipCard.Controls.Add(lblSlipTitle); pnlReplySlipCard.Controls.Add(lblSlipDesc);

            int sigY = 120;
            string[] sigFields = { "Customer Signature", "Date of Receipt", "Delivery Operator Sign" };
            for (int i = 0; i < sigFields.Length; i++)
            {
                Label lblLine = new Label { Text = "_________________________________________", Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(202, 138, 4), Location = new Point(20, sigY), AutoSize = true };
                Label lblField = new Label { Text = sigFields[i], Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(161, 98, 7), Location = new Point(20, sigY + 20), AutoSize = true };
                pnlReplySlipCard.Controls.Add(lblLine); pnlReplySlipCard.Controls.Add(lblField);
                sigY += 55;
            }

            Panel pnlActionDock = new Panel { Size = new Size(650, 80), Location = new Point(0, 730), BackColor = Color.White, BorderStyle = BorderStyle.None };
            previewForm.Controls.Add(pnlActionDock);
            pnlActionDock.BringToFront();

            Panel pnlTopLine = new Panel { Size = new Size(650, 1), Location = new Point(0, 0), BackColor = Color.FromArgb(226, 232, 240) };
            pnlActionDock.Controls.Add(pnlTopLine);

            Button btnExport = new Button { Text = "💾 Export & Preview Document", Location = new Point(30, 18), Size = new Size(390, 45), BackColor = Color.FromArgb(14, 165, 233), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnExport.FlatAppearance.BorderSize = 0;

            Button btnCancel = new Button { Text = "Close", Location = new Point(440, 18), Size = new Size(160, 45), BackColor = Color.FromArgb(243, 244, 246), ForeColor = Color.FromArgb(75, 85, 99), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCancel.FlatAppearance.BorderSize = 0;

            // 🛠️ 核心修正：升級為高品質 HTML 導出引擎
            btnExport.Click += (src, args) =>
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Modern Web Document (*.html)|*.html|Plain Text File (*.txt)|*.txt";
                    sfd.FileName = $"DeliveryManifest_{orderID}";
                    sfd.Title = "Save Document Structure";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            string fileContent = "";
                            if (sfd.FileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                            {
                                fileContent = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Delivery Note - {orderID}</title>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; margin: 40px; color: #1e293b; background-color: #f8fafc; }}
        .container {{ max-width: 650px; background: white; padding: 40px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.05); border-top: 8px solid #0ea5e9; margin: 0 auto; }}
        h1 {{ color: #1e3a8a; margin: 0 0 5px 0; font-size: 24px; font-weight: 800; }}
        .subtitle {{ color: #0ea5e9; font-weight: bold; margin-bottom: 30px; letter-spacing: 1px; }}
        .meta-table {{ width: 100%; border-collapse: collapse; margin-bottom: 40px; }}
        .meta-table td {{ padding: 12px 0; border-bottom: 1px solid #e2e8f0; font-size: 15px; }}
        .meta-label {{ font-weight: bold; color: #64748b; width: 35%; }}
        .meta-value {{ color: #0f172a; }}
        .reply-slip {{ background-color: #fef08a; border: 2px dashed #ca8a04; padding: 25px; border-radius: 6px; margin-top: 30px; }}
        .slip-title {{ color: #a16207; font-weight: bold; font-size: 16px; margin-bottom: 10px; }}
        .sig-container {{ margin-top: 50px; display: flex; justify-content: space-between; }}
        .sig-box {{ width: 45%; border-top: 1px solid #ca8a04; text-align: center; padding-top: 8px; font-size: 13px; color: #a16207; font-weight: bold; margin-top: 40px; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>PREMIUM LIVING FURNITURE</h1>
        <div class='subtitle'>OFFICIAL DELIVERY NOTE</div>
        
        <table class='meta-table'>
            <tr><td class='meta-label'>Order Reference:</td><td class='meta-value'><b>{orderID}</b></td></tr>
            <tr><td class='meta-label'>Customer ID:</td><td class='meta-value'>{customerID}</td></tr>
            <tr><td class='meta-label'>Destination Address:</td><td class='meta-value'>{address}</td></tr>
            <tr><td class='meta-label'>Scheduled Date:</td><td class='meta-value'>{scheduleDate}</td></tr>
            <tr><td class='meta-label'>Assigned Logistics Team:</td><td class='meta-value'>{deliveryStaff}</td></tr>
        </table>

        <div class='reply-slip'>
            <div class='slip-title'>📌 CUSTOMER REPLY SLIP & RECEIPT</div>
            <p style='color: #713f12; font-size: 14px;'>I hereby confirm receipt of items for Order [{orderID}] in pristine condition without any structural damages.</p>
            
            <div class='sig-container'>
                <div class='sig-box'>Customer Signature / Date</div>
                <div class='sig-box'>Delivery Operator Signature</div>
            </div>
        </div>
    </div>
</body>
</html>";
                            }
                            else
                            {
                                fileContent = $@"PREMIUM LIVING FURNITURE\r\nOFFICIAL DELIVERY NOTE\r\n--------------------------------------\r\nOrder Ref: {orderID}\r\nCustomer: {customerID}\r\nAddress: {address}\r\nDate: {scheduleDate}\r\nTeam: {deliveryStaff}\r\n--------------------------------------\r\nStatus: Received in good condition.";
                            }

                            File.WriteAllText(sfd.FileName, fileContent, System.Text.Encoding.UTF8);
                            MessageBox.Show($"File successfully generated and saved at:\n{sfd.FileName}", "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // 🛠️ 核心修正：自動呼叫預設瀏覽器秒開網頁，讓用戶隨時 Ctrl+P 轉印成真 PDF
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                            previewForm.Close();
                        }
                        catch (Exception ex) { MessageBox.Show($"File export error:\n{ex.Message}", "IO Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                    }
                }
            };

            btnCancel.Click += (src, args) => previewForm.Close();
            pnlActionDock.Controls.Add(btnExport);
            pnlActionDock.Controls.Add(btnCancel);
            previewForm.ShowDialog();
        }

        private void ClearLogisticsFields()
        {
            txtOrderID.Clear();
            txtCustomerID.Clear();
            txtDeliveryAddress.Clear();
            txtCurrentStatus.Clear();
            if (cboDeliveryStaff.Items.Count > 0) cboDeliveryStaff.SelectedIndex = -1;
            dtpScheduleDate.Value = DateTime.Now;
            dgvPendingOrders.ClearSelection();
        }
        #endregion
    }
}