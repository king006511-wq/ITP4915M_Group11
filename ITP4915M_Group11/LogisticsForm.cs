using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

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
            AuthorizationHelper.EnforceRole(this, AuthorizationHelper.Roles.Administrator, AuthorizationHelper.Roles.LogisticsDriver, AuthorizationHelper.Roles.WarehouseSpecialist);
            this.currentStaffID = string.IsNullOrEmpty(UserSession.LoggedInStaffID) ? "S001" : UserSession.LoggedInStaffID;

            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                InitializePremiumModernUI();
                LoadDeliveryStaff();
                RefreshPendingOrdersGrid();

                bool isLogistics = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.LogisticsDriver);
                bool isAdminOrManager = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Administrator);

                btnAssignDelivery.Enabled = isLogistics || isAdminOrManager;
                btnUpdateStatus.Enabled = isLogistics || isAdminOrManager;
                btnGenerateNote.Enabled = isLogistics || isAdminOrManager;
            }
        }

        private void InitializePremiumModernUI()
        {
            this.Controls.Clear();
            this.BackColor = Color.FromArgb(241, 245, 249);
            this.FormBorderStyle = FormBorderStyle.None;

            TableLayoutPanel mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.Controls.Add(mainTable);

            Panel pnlHeader = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            Label lblModuleTitle = new Label
            {
                Text = "Logistics Dispatch & Delivery Management",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(20, 20),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblModuleTitle);
            mainTable.Controls.Add(pnlHeader, 0, 0);

            TableLayoutPanel contentTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(20, 0, 20, 20)
            };
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainTable.Controls.Add(contentTable, 0, 1);

            Panel pnlInputs = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, AutoScroll = true, Padding = new Padding(20) };
            pnlInputs.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlInputs.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            contentTable.Controls.Add(pnlInputs, 0, 0);

            dgvPendingOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9.5F),
                ReadOnly = true
            };
            dgvPendingOrders.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvPendingOrders.SelectionChanged += dgvPendingOrders_SelectionChanged;
            contentTable.Controls.Add(dgvPendingOrders, 2, 0);

            int currentY = 15;
            int inputWidth = 300;

            Label lblOrderID = new Label { Text = "Target Order ID", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(15, currentY), AutoSize = true };
            txtOrderID = new TextBox { Location = new Point(15, currentY + 25), Size = new Size(inputWidth, 30), Font = new Font("Segoe UI", 10F), ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249) };
            pnlInputs.Controls.Add(lblOrderID);
            pnlInputs.Controls.Add(txtOrderID);
            currentY += 65;

            Label lblCust = new Label { Text = "Customer ID", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(15, currentY), AutoSize = true };
            txtCustomerID = new TextBox { Location = new Point(15, currentY + 25), Size = new Size(inputWidth, 30), Font = new Font("Segoe UI", 10F), ReadOnly = true, BackColor = Color.White };
            pnlInputs.Controls.Add(lblCust);
            pnlInputs.Controls.Add(txtCustomerID);
            currentY += 65;

            Label lblAddress = new Label { Text = "Destination Address", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(15, currentY), AutoSize = true };
            txtDeliveryAddress = new TextBox { Location = new Point(15, currentY + 25), Size = new Size(inputWidth, 55), Font = new Font("Segoe UI", 10F), Multiline = true, ReadOnly = true, BackColor = Color.White };
            pnlInputs.Controls.Add(lblAddress);
            pnlInputs.Controls.Add(txtDeliveryAddress);
            currentY += 95;

            Label lblStatus = new Label { Text = "Current State", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(15, currentY), AutoSize = true };
            txtCurrentStatus = new TextBox { Location = new Point(15, currentY + 25), Size = new Size(inputWidth, 30), Font = new Font("Segoe UI", 10F), ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249) };
            pnlInputs.Controls.Add(lblStatus);
            pnlInputs.Controls.Add(txtCurrentStatus);
            currentY += 65;

            Label lblStaff = new Label { Text = "Assign Delivery Team *", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(15, currentY), AutoSize = true };
            cboDeliveryStaff = new ComboBox { Location = new Point(15, currentY + 25), Size = new Size(inputWidth, 30), Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };
            pnlInputs.Controls.Add(lblStaff);
            pnlInputs.Controls.Add(cboDeliveryStaff);
            currentY += 65;

            Label lblDate = new Label { Text = "Scheduled Date *", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(15, currentY), AutoSize = true };

            // 【修改位置：加入 MinDate = DateTime.Today 限制使用者無法選擇過去的日期】
            dtpScheduleDate = new DateTimePicker
            {
                Location = new Point(15, currentY + 25),
                Size = new Size(inputWidth, 30),
                Font = new Font("Segoe UI", 10F),
                Format = DateTimePickerFormat.Short,
                MinDate = DateTime.Today
            };
            pnlInputs.Controls.Add(lblDate);
            pnlInputs.Controls.Add(dtpScheduleDate);
            currentY += 75;

            btnAssignDelivery = new Button { Text = "Dispatch Order", Location = new Point(15, currentY), Size = new Size(140, 35), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(14, 165, 233), Cursor = Cursors.Hand };
            btnAssignDelivery.FlatAppearance.BorderSize = 0;
            btnAssignDelivery.Click += btnAssignDelivery_Click;

            btnUpdateStatus = new Button { Text = "Mark Delivered", Location = new Point(175, currentY), Size = new Size(140, 35), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(16, 185, 129), Cursor = Cursors.Hand };
            btnUpdateStatus.FlatAppearance.BorderSize = 0;
            btnUpdateStatus.Click += btnUpdateStatus_Click;
            currentY += 45;

            btnGenerateNote = new Button { Text = "✨ Generate Note & Reply Slip", Location = new Point(15, currentY), Size = new Size(300, 40), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(79, 70, 229), Cursor = Cursors.Hand };
            btnGenerateNote.FlatAppearance.BorderSize = 0;
            btnGenerateNote.Click += btnGenerateNote_Click;
            currentY += 50;

            btnClearFields = new Button { Text = "Reset Form", Location = new Point(15, currentY), Size = new Size(300, 35), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), BackColor = Color.FromArgb(226, 232, 240), Cursor = Cursors.Hand };
            btnClearFields.FlatAppearance.BorderSize = 0;
            btnClearFields.Click += (s, e) => ClearLogisticsFields();

            pnlInputs.Controls.Add(btnAssignDelivery);
            pnlInputs.Controls.Add(btnUpdateStatus);
            pnlInputs.Controls.Add(btnGenerateNote);
            pnlInputs.Controls.Add(btnClearFields);
        }

        private void LoadDeliveryStaff()
        {
            cboDeliveryStaff.Items.Clear();
            cboDeliveryStaff.Items.Add("Team A - John Doe");
            cboDeliveryStaff.Items.Add("Team B - Michael Smith");
            cboDeliveryStaff.Items.Add("Team C - David Wong");
            cboDeliveryStaff.Items.Add("Outsource - SF Express");
        }

        private void RefreshPendingOrdersGrid()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query;
                    var role = UserSession.LoggedInStaffRoleEnum;

                    if (role == AuthorizationHelper.UserRoleEnum.LogisticsDriver)
                    {
                        query = "SELECT OrderID AS 'Order ID', CustomerID AS 'Customer', Status, OrderDate AS 'Date' FROM orders WHERE Status='Ready for Dispatch' ORDER BY OrderDate DESC";
                    }
                    else
                    {
                        query = "SELECT OrderID AS 'Order ID', CustomerID AS 'Customer', Status, OrderDate AS 'Date' FROM orders WHERE Status IN ('Ready for Dispatch','Dispatched') ORDER BY OrderDate DESC";
                    }

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvPendingOrders.DataSource = dt;
                    }
                }
                catch { }
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

                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connString))
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("SELECT Address FROM customer WHERE CustomerID=@CID", conn))
                        {
                            cmd.Parameters.AddWithValue("@CID", txtCustomerID.Text);
                            var addr = cmd.ExecuteScalar();
                            txtDeliveryAddress.Text = (addr != null && addr != DBNull.Value) ? addr.ToString() : "Standard Registered Address (Please verify with client)";
                        }
                    }
                }
                catch
                {
                    txtDeliveryAddress.Text = "Standard Registered Address (Please verify with client)";
                }
            }
        }

        private void btnAssignDelivery_Click(object sender, EventArgs e)
        {
            string orderID = txtOrderID.Text.Trim();
            string address = txtDeliveryAddress.Text.Trim();

            if (string.IsNullOrEmpty(orderID) || cboDeliveryStaff.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an Order and assign a Delivery Team first.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (txtCurrentStatus.Text == "Dispatched")
            {
                MessageBox.Show("This order has already been dispatched!", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            string updateSql = "UPDATE orders SET Status='Dispatched' WHERE OrderID=@OrderID";
                            using (MySqlCommand cmd = new MySqlCommand(updateSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", orderID);
                                cmd.ExecuteNonQuery();
                            }

                            string dnID = "DN-" + DateTime.Now.ToString("yyyyMMdd-HHmm");
                            string insertDnSql = "INSERT INTO delivery_note (DeliveryNoteID, OrderID, DeliveryDate, DeliveryAddress) VALUES (@dnID, @oID, @dDate, @addr)";
                            using (MySqlCommand cmd = new MySqlCommand(insertDnSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@dnID", dnID);
                                cmd.Parameters.AddWithValue("@oID", orderID);
                                cmd.Parameters.AddWithValue("@dDate", dtpScheduleDate.Value);
                                cmd.Parameters.AddWithValue("@addr", address);
                                cmd.ExecuteNonQuery();
                            }

                            trans.Commit();
                            MessageBox.Show($"Order [{orderID}] has been successfully dispatched!\nDelivery Note: {dnID}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearLogisticsFields();
                            RefreshPendingOrdersGrid();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            MessageBox.Show("Dispatch failed (Database Error): " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            string orderID = txtOrderID.Text.Trim();
            if (string.IsNullOrEmpty(orderID)) return;

            if (txtCurrentStatus.Text != "Dispatched")
            {
                MessageBox.Show("Only 'Dispatched' orders can be marked as Delivery Completed.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string updateSql = "UPDATE orders SET Status='Delivery Completed' WHERE OrderID=@OrderID";
                    using (MySqlCommand cmd = new MySqlCommand(updateSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderID);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show($"Order [{orderID}] marked as delivered.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearLogisticsFields();
                        RefreshPendingOrdersGrid();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

            Form previewForm = new Form
            {
                Text = "Premium Invoice & Slip Hub",
                Size = new Size(620, 800),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(248, 250, 252),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Panel scrollContainer = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(25, 15, 25, 90) };
            previewForm.Controls.Add(scrollContainer);

            Panel docSheet = new Panel { Width = 540, Height = 640, Location = new Point(25, 20), BackColor = Color.White, Padding = new Padding(30) };
            scrollContainer.Controls.Add(docSheet);

            Label lblBrand = new Label { Text = "PREMIUM LIVING FURNITURE", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(79, 70, 229), Location = new Point(30, 30), AutoSize = true };
            Label lblDocType1 = new Label { Text = "DELIVERY NOTE", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(30, 60), AutoSize = true };
            docSheet.Controls.Add(lblBrand);
            docSheet.Controls.Add(lblDocType1);

            int currentMetaY = 100;
            string[] metadataLabels = { "Order Identifier:", "Client Code:", "Target Address:", "Execution Date:", "Logistic Operator:" };
            string[] metadataValues = { orderID, customerID, address, scheduleDate, deliveryStaff };

            for (int i = 0; i < metadataLabels.Length; i++)
            {
                Label lblMeta = new Label { Text = metadataLabels[i], Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(30, currentMetaY), AutoSize = true };
                Label lblVal = new Label { Text = metadataValues[i], Font = new Font("Segoe UI", 9.5F, FontStyle.Regular), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(170, currentMetaY), AutoSize = true };
                docSheet.Controls.Add(lblMeta);
                docSheet.Controls.Add(lblVal);
                currentMetaY += 28;
            }

            Panel pnlDivider = new Panel { Location = new Point(30, 260), Size = new Size(480, 2), BackColor = Color.FromArgb(226, 232, 240) };
            docSheet.Controls.Add(pnlDivider);

            Panel pnlReplySlipCard = new Panel { Location = new Point(30, 285), Size = new Size(480, 320), BackColor = Color.FromArgb(241, 245, 249), Padding = new Padding(20) };
            docSheet.Controls.Add(pnlReplySlipCard);

            Label lblSlipTitle = new Label { Text = "CUSTOMER REPLY SLIP (GOODS CONFIRMATION)", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(20, 20), AutoSize = true };
            Label lblSlipDesc = new Label { Text = $"I hereby acknowledge that all package contents tied to Order ID [{orderID}] have arrived intact without visible structural defects.", Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(20, 50), Size = new Size(440, 45), AutoSize = false };
            pnlReplySlipCard.Controls.Add(lblSlipTitle);
            pnlReplySlipCard.Controls.Add(lblSlipDesc);

            int sigY = 130;
            string[] sigFields = { "Customer Signature", "Authorized Date", "Carrier Validation" };
            for (int i = 0; i < sigFields.Length; i++)
            {
                Label lblLine = new Label { Text = "_____________________________________", Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(203, 213, 225), Location = new Point(20, sigY), AutoSize = true };
                Label lblField = new Label { Text = sigFields[i], Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(20, sigY + 20), AutoSize = true };
                pnlReplySlipCard.Controls.Add(lblLine);
                pnlReplySlipCard.Controls.Add(lblField);
                sigY += 60;
            }

            Panel pnlActionDock = new Panel { Size = new Size(620, 80), Location = new Point(0, 685), BackColor = Color.White, BorderStyle = BorderStyle.None };
            previewForm.Controls.Add(pnlActionDock);
            pnlActionDock.BringToFront();

            Panel pnlTopLine = new Panel { Size = new Size(620, 1), Location = new Point(0, 0), BackColor = Color.FromArgb(226, 232, 240) };
            pnlActionDock.Controls.Add(pnlTopLine);

            Button btnExport = new Button { Text = "🖨️ Export / Save as Document", Location = new Point(25, 18), Size = new Size(380, 44), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnExport.FlatAppearance.BorderSize = 0;

            Button btnCancel = new Button { Text = "Dismiss", Location = new Point(425, 18), Size = new Size(150, 44), BackColor = Color.FromArgb(241, 245, 249), ForeColor = Color.FromArgb(100, 116, 139), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCancel.FlatAppearance.BorderSize = 0;

            btnExport.Click += (src, args) =>
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Web Document for Printing (*.html)|*.html|Standard Text File (*.txt)|*.txt";
                    sfd.FileName = $"DeliveryManifest_{orderID}";
                    sfd.Title = "Choose System Directory Location to Save File";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            string fileContent = "";
                            if (sfd.FileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                            {
                                fileContent = $@"<!DOCTYPE html><html><head><meta charset='utf-8'><title>Delivery Note - {orderID}</title><style>body{{font-family:'Segoe UI',Arial,sans-serif;margin:40px;color:#1e293b;background-color:#f8fafc;}}.container{{max-width:650px;background:white;padding:40px;border-radius:8px;box-shadow:0 4px 6px rgba(0,0,0,0.05);border-top:8px solid #4f46e5;margin:0 auto;}}h1{{color:#1e3a8a;margin:0 0 5px 0;font-size:24px;font-weight:800;}}.subtitle{{color:#4f46e5;font-weight:bold;margin-bottom:30px;letter-spacing:1px;}}.meta-table{{width:100%;border-collapse:collapse;margin-bottom:40px;}}.meta-table td{{padding:12px 0;border-bottom:1px solid #e2e8f0;font-size:15px;}}.meta-label{{font-weight:bold;color:#64748b;width:35%;}}.meta-value{{color:#0f172a;}}.reply-slip{{background-color:#f8fafc;border:2px dashed #94a3b8;padding:25px;border-radius:6px;margin-top:30px;}}.slip-title{{color:#0f172a;font-weight:bold;font-size:16px;margin-bottom:10px;}}.sig-container{{margin-top:50px;display:flex;justify-content:space-between;}}.sig-box{{width:45%;border-top:1px solid #94a3b8;text-align:center;padding-top:8px;font-size:13px;color:#475569;font-weight:bold;margin-top:40px;}}</style></head><body><div class='container'><h1>PREMIUM LIVING FURNITURE</h1><div class='subtitle'>OFFICIAL DELIVERY NOTE</div><table class='meta-table'><tr><td class='meta-label'>Order Reference:</td><td class='meta-value'><b>{orderID}</b></td></tr><tr><td class='meta-label'>Customer ID:</td><td class='meta-value'>{customerID}</td></tr><tr><td class='meta-label'>Destination Address:</td><td class='meta-value'>{address}</td></tr><tr><td class='meta-label'>Scheduled Date:</td><td class='meta-value'>{scheduleDate}</td></tr><tr><td class='meta-label'>Assigned Logistics Team:</td><td class='meta-value'>{deliveryStaff}</td></tr></table><div class='reply-slip'><div class='slip-title'>📌 CUSTOMER REPLY SLIP & RECEIPT</div><p style='color:#475569;font-size:14px;'>I hereby confirm receipt of items for Order [{orderID}] in pristine condition without any structural damages.</p><div class='sig-container'><div class='sig-box'>Customer Signature / Date</div><div class='sig-box'>Delivery Operator Signature</div></div></div></div></body></html>";
                            }
                            else
                            {
                                fileContent = $"PREMIUM LIVING FURNITURE\n----------------------------------------------------\nDELIVERY NOTE DATA SUMMARY\n----------------------------------------------------\nOrder Ref: {orderID}\nCustomer Ref: {customerID}\nTarget Date: {scheduleDate}\nLogistics: {deliveryStaff}\nDestination: {address}\n----------------------------------------------------\nCUSTOMER REPLY MANIFEST\n----------------------------------------------------\nStatus: Received in good condition.\nCustomer Sign: ___________________________ \nDate Signed: ___________________________";
                            }

                            File.WriteAllText(sfd.FileName, fileContent, System.Text.Encoding.UTF8);
                            MessageBox.Show($"File successfully pipeline-routed and stored at:\n{sfd.FileName}", "System Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                            previewForm.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"File pipeline exception:\n{ex.Message}", "IO Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
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

        private void LogisticsForm_Load(object sender, EventArgs e)
        {
        }
    }
}