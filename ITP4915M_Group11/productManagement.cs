using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ITP4915M_Group11
{
    public partial class ProductManagement : BaseForm
    {
        private TextBox txtProductID;
        private TextBox txtProductName;
        private TextBox txtSearch;
        private TextBox txtStockLevel;
        private TextBox txtRetailPrice;
        private DataGridView dgvProductCatalog;

        // 宣告 Button 為類別變數，方便後續強制上色 (防止被 ThemeManager 覆寫)
        private Button btnBackHome;
        private Button btnViewPhoto;
        private Button btnUploadPhoto;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;

        // 🔒 Centralized Database Connection String
        private readonly string connectionString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        public ProductManagement()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                // 先建立 UI，再套用主題
                InitializePremiumModernUI();

                // 強制套用專屬按鈕顏色，蓋過 ThemeManager 的單一設定
                ApplyButtonColors();

                LoadDatabaseData();
            }
        }

        #region 🎨 強制按鈕上色 (防止 ThemeManager 覆寫)
        private void ApplyButtonColors()
        {
            if (btnBackHome != null) { btnBackHome.BackColor = Color.FromArgb(99, 102, 241); btnBackHome.ForeColor = Color.White; }
            if (btnViewPhoto != null) { btnViewPhoto.BackColor = Color.FromArgb(14, 165, 233); btnViewPhoto.ForeColor = Color.White; }
            if (btnUploadPhoto != null) { btnUploadPhoto.BackColor = Color.FromArgb(245, 158, 11); btnUploadPhoto.ForeColor = Color.White; }
            if (btnUpdate != null) { btnUpdate.BackColor = Color.FromArgb(34, 197, 94); btnUpdate.ForeColor = Color.White; }
            if (btnDelete != null) { btnDelete.BackColor = Color.FromArgb(239, 68, 68); btnDelete.ForeColor = Color.White; }
            if (btnClear != null) { btnClear.BackColor = Color.FromArgb(100, 116, 139); btnClear.ForeColor = Color.White; }
        }
        #endregion

        #region 🔒 System Security Gatekeeper Enforcement
        private void ProductManagement_Load(object sender, EventArgs e)
        {
            string currentRole = UserSession.LoggedInStaffRole;
            string currentStaffID = UserSession.LoggedInStaffID;

            // 允許 Warehouse Specialist 檢視此頁面，但只有 Manager/Administrator 可以編輯
            bool isAuthorized = !string.IsNullOrEmpty(currentRole) &&
                                (currentRole.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                                 currentRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase) ||
                                 currentRole.Equals("Warehouse Specialist", StringComparison.OrdinalIgnoreCase));

            if (!isAuthorized)
            {
                MessageBox.Show(
                    $"[SECURITY ALERT] Access Denied!\n\n" +
                    $"Logged In Staff ID: {(string.IsNullOrEmpty(currentStaffID) ? "Unknown" : currentStaffID)}\n" +
                    $"Your Account Role is: \"{(string.IsNullOrEmpty(currentRole) ? "None" : currentRole)}\"\n\n" +
                    $"Only a Manager or Administrator is authorized to access Product Maintenance settings.",
                    "System Security Guard",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop
                );

                this.Shown += (s2, e2) => this.Close();
                return;
            }

            bool canEdit = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator);
            foreach (Control c in this.Controls)
            {
                if (c is Button b && (b.Text.Contains("Update") || b.Text.Contains("Delete") || b.Text.Contains("Upload")))
                {
                    b.Enabled = canEdit;
                    // 如果冇權限，先變灰；有權限就保持現有顏色
                    b.BackColor = canEdit ? b.BackColor : Color.LightGray;
                }
            }
        }
        #endregion

        #region 🎨 Premium Unified Modern UI Construction Engine
        private void InitializePremiumModernUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Product Maintenance & Catalog Control";
            this.Size = new Size(1180, 780);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            this.Load += ProductManagement_Load;

            // Workspace Controller Panel Area
            Panel pnlMain = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            this.Controls.Add(pnlMain);

            Label lblHeader = new Label { Text = "Finished Goods Inventory Maintenance", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(30, 20), AutoSize = true };
            pnlMain.Controls.Add(lblHeader);

            // 🎨 按鈕 1: Go Back
            btnBackHome = new Button { Text = "🔙 Go Back", Size = new Size(120, 34), Location = new Point(1015, 22), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand, Visible = true };
            btnBackHome.FlatAppearance.BorderSize = 0;
            btnBackHome.Click += (s, e) => { try { this.Close(); } catch { this.Hide(); } };
            pnlMain.Controls.Add(btnBackHome);

            // Input Details Dashboard Card
            Panel pnlCard = new Panel { Location = new Point(30, 85), Size = new Size(380, 630), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlCard);

            Label lblCardTitle = new Label { Text = "📦 Finished Product Details", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(79, 70, 229), Location = new Point(20, 15), AutoSize = true };
            pnlCard.Controls.Add(lblCardTitle);

            int startY = 60;
            txtProductID = CreateStyledTextBox(pnlCard, ref startY, "Product ID *:", false);
            txtProductName = CreateStyledTextBox(pnlCard, ref startY, "Product Name *:", false);
            txtStockLevel = CreateStyledTextBox(pnlCard, ref startY, "Stock Level:", false);
            txtRetailPrice = CreateStyledTextBox(pnlCard, ref startY, "Retail Price (HKD):", false);

            // 微調按鈕間距
            startY += 15;

            // 🎨 初始化各種操作按鈕 (已向上移緊湊排版)
            btnViewPhoto = new Button { Text = "🖼️ View Photo", Location = new Point(20, startY), Size = new Size(160, 42), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnUploadPhoto = new Button { Text = "📂 Upload Photo", Location = new Point(195, startY), Size = new Size(160, 42), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnUpdate = new Button { Text = "💾 Update", Location = new Point(20, startY + 50), Size = new Size(160, 42), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnDelete = new Button { Text = "🗑️ Delete", Location = new Point(195, startY + 50), Size = new Size(160, 42), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClear = new Button { Text = "🧹 Clear Forms", Location = new Point(20, startY + 100), Size = new Size(335, 42), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };

            foreach (var b in new Button[] { btnViewPhoto, btnUploadPhoto, btnUpdate, btnDelete, btnClear }) b.FlatAppearance.BorderSize = 0;
            pnlCard.Controls.AddRange(new Control[] { btnViewPhoto, btnUploadPhoto, btnUpdate, btnDelete, btnClear });

            btnViewPhoto.Click += btnViewPhoto_Click;
            btnUploadPhoto.Click += btnUploadPhoto_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;

            btnClear.Click += (s, e) => {
                ClearFields();
            };

            // Data Grid Component
            Label lblGridTitle = new Label { Text = "📋 Real-Time Product Catalog Records", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(440, 85), AutoSize = true };
            pnlMain.Controls.Add(lblGridTitle);

            // 🔍 搜尋功能組件：✅ 已移上一行與標題並排，解決撞位問題
            Label lblSearch = new Label { Text = "🔍 Search:", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(79, 70, 229), Location = new Point(865, 88), AutoSize = true };
            pnlMain.Controls.Add(lblSearch);

            txtSearch = new TextBox { Location = new Point(950, 85), Width = 190, Font = new Font("Segoe UI", 10F), BorderStyle = BorderStyle.FixedSingle };
            txtSearch.TextChanged += txtSearch_TextChanged;
            pnlMain.Controls.Add(txtSearch);

            // 🚨 庫存警告標籤：✅ 獨佔一行，不會再與 Search 重疊
            Label lblWarningLegend = new Label { Text = "🚨 Alert: Rows highlighted in RED indicate Low Stock (Below 20 units)", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(220, 38, 38), Location = new Point(440, 118), AutoSize = true };
            pnlMain.Controls.Add(lblWarningLegend);

            // DataGridView 設置
            dgvProductCatalog = new DataGridView
            {
                Location = new Point(440, 145),
                Size = new Size(700, 570),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvProductCatalog.EnableHeadersVisualStyles = false;
            dgvProductCatalog.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(79, 70, 229);
            dgvProductCatalog.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProductCatalog.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvProductCatalog.ColumnHeadersHeight = 38;
            dgvProductCatalog.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvProductCatalog.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);

            dgvProductCatalog.SelectionChanged += dgvProductCatalog_SelectionChanged;
            dgvProductCatalog.CellFormatting += dgvProductCatalog_CellFormatting;

            ThemeManager.StyleDataGrid(dgvProductCatalog);
            pnlMain.Controls.Add(dgvProductCatalog);
        }

        private TextBox CreateStyledTextBox(Panel container, ref int topY, string labelText, bool readOnly)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(20, topY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(20, topY + 22), Width = 335, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };
            if (readOnly) { txt.ReadOnly = true; txt.BackColor = Color.FromArgb(241, 245, 249); }
            container.Controls.Add(lbl); container.Controls.Add(txt);
            topY += 65;
            return txt;
        }
        #endregion

        #region 📦 Business Management Logic Functions

        private void ApplySearchFilter()
        {
            if (dgvProductCatalog.DataSource is DataTable dt)
            {
                string keyword = txtSearch.Text.Trim().Replace("'", "''");
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    dt.DefaultView.RowFilter = "";
                }
                else
                {
                    dt.DefaultView.RowFilter = string.Format("[Product ID] LIKE '%{0}%' OR [Name] LIKE '%{0}%'", keyword);
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplySearchFilter();
        }

        private void LoadDatabaseData()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT ProductID AS 'Product ID', ProductName AS 'Name', StockLevel AS 'Stock', RetailPrice AS 'Price HKD' FROM product";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvProductCatalog.DataSource = dt;

                        ApplySearchFilter();
                    }
                }
            }
            catch (Exception ex)
            {
                DataTable errorDt = new DataTable();
                errorDt.Columns.Add("System Status");
                errorDt.Rows.Add("Database Error: " + ex.Message);
                dgvProductCatalog.DataSource = errorDt;
            }
        }

        private void dgvProductCatalog_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvProductCatalog.Columns.Contains("Stock"))
            {
                var stockCell = dgvProductCatalog.Rows[e.RowIndex].Cells["Stock"];
                if (stockCell.Value != null && int.TryParse(stockCell.Value.ToString(), out int stockQty))
                {
                    if (stockQty < 20)
                    {
                        e.CellStyle.BackColor = Color.FromArgb(254, 226, 226);
                        e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);

                        if (dgvProductCatalog.Columns[e.ColumnIndex].Name == "Stock")
                        {
                            e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                        }
                    }
                }
            }
        }

        private void dgvProductCatalog_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProductCatalog.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvProductCatalog.SelectedRows[0];
                txtProductID.Text = row.Cells["Product ID"].Value?.ToString() ?? "";
                txtProductName.Text = row.Cells["Name"].Value?.ToString() ?? "";
                txtStockLevel.Text = row.Cells["Stock"].Value?.ToString() ?? "";
                txtRetailPrice.Text = row.Cells["Price HKD"].Value?.ToString() ?? "";
                txtProductID.ReadOnly = true;
                txtProductID.BackColor = Color.FromArgb(241, 245, 249);
            }
            else
            {
                ClearFields();
            }
        }

        private void btnUploadPhoto_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductID.Text))
            {
                MessageBox.Show("Please select a product first to assign a photo to it.", "No Product Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string productID = txtProductID.Text.Trim();
            string targetFolder = Path.Combine(Application.StartupPath, "ProductImages");

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = $"Select Image for {productID}";
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.webp;*.bmp";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string[] oldFiles = Directory.GetFiles(targetFolder, $"{productID}.*");
                        foreach (string oldFile in oldFiles)
                        {
                            File.Delete(oldFile);
                        }

                        string extension = Path.GetExtension(ofd.FileName);
                        string newFilePath = Path.Combine(targetFolder, $"{productID}{extension}");

                        File.Copy(ofd.FileName, newFilePath);

                        MessageBox.Show($"Success! The image has been automatically saved and renamed to {productID}{extension}.", "Upload Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error processing the image:\n" + ex.Message, "Upload Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnViewPhoto_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductID.Text))
            {
                MessageBox.Show("Please select a product from the catalog to view its photo.", "No Product Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string productID = txtProductID.Text.Trim();
            string productName = txtProductName.Text.Trim();
            string folderPath = Path.Combine(Application.StartupPath, "ProductImages");

            using (Form photoForm = new Form())
            {
                photoForm.Text = $"Product Photo - {productName} ({productID})";
                photoForm.Size = new Size(500, 550);
                photoForm.StartPosition = FormStartPosition.CenterParent;
                photoForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                photoForm.MaximizeBox = false;
                photoForm.MinimizeBox = false;
                photoForm.BackColor = Color.White;

                PictureBox pb = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.FromArgb(249, 250, 251)
                };

                bool isImageLoaded = false;

                if (Directory.Exists(folderPath))
                {
                    string[] matchingFiles = Directory.GetFiles(folderPath, $"{productID}.*");

                    if (matchingFiles.Length > 0)
                    {
                        try
                        {
                            byte[] bytes = File.ReadAllBytes(matchingFiles[0]);
                            using (MemoryStream ms = new MemoryStream(bytes))
                            {
                                pb.Image = Image.FromStream(ms);
                                isImageLoaded = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error loading image file:\n" + ex.Message, "Image Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }

                if (!isImageLoaded)
                {
                    Label lblNoImage = new Label
                    {
                        Text = $"🚫 No Photo Assigned\n\nPlease click 'Upload Photo' to add an image for this product.",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(100, 116, 139)
                    };
                    pb.Controls.Add(lblNoImage);
                }

                photoForm.Controls.Add(pb);
                photoForm.ShowDialog();
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductID.Text))
            {
                MessageBox.Show("Please click on an active catalog item to initiate modification cycles.", "System Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE product SET ProductName=@name, StockLevel=@stock, RetailPrice=@price WHERE ProductID=@id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtProductID.Text.Trim());
                        cmd.Parameters.AddWithValue("@name", txtProductName.Text.Trim());
                        cmd.Parameters.AddWithValue("@stock", string.IsNullOrEmpty(txtStockLevel.Text) ? 0 : Convert.ToInt32(txtStockLevel.Text.Trim()));
                        cmd.Parameters.AddWithValue("@price", string.IsNullOrEmpty(txtRetailPrice.Text) ? 0 : Convert.ToDecimal(txtRetailPrice.Text.Trim()));

                        int rowCount = cmd.ExecuteNonQuery();
                        if (rowCount > 0)
                        {
                            MessageBox.Show("Inventory product info has updated correctly!", "Transaction Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadDatabaseData();
                            ClearFields();
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Update Operation Failure:\n" + ex.Message, "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductID.Text))
            {
                MessageBox.Show("Select an active catalog item from the history panel to drop.", "Deletion Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult confirm = MessageBox.Show($"Are you sure you want to permanently erase catalog item record ID [{txtProductID.Text}]?", "Confirm Erase Sequence", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.No) return;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM product WHERE ProductID=@id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtProductID.Text.Trim());
                        int dynamicRows = cmd.ExecuteNonQuery();
                        if (dynamicRows > 0)
                        {
                            string targetFolder = Path.Combine(Application.StartupPath, "ProductImages");
                            if (Directory.Exists(targetFolder))
                            {
                                string[] oldFiles = Directory.GetFiles(targetFolder, $"{txtProductID.Text.Trim()}.*");
                                foreach (string oldFile in oldFiles) File.Delete(oldFile);
                            }

                            MessageBox.Show("Product was dropped successfully!", "Record Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadDatabaseData();
                            ClearFields();
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Critical deletion database block caught: " + ex.Message, "Error Processing Operation", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void ClearFields()
        {
            txtProductID.Clear();
            txtProductName.Clear();
            txtStockLevel.Clear();
            txtRetailPrice.Clear();
            txtProductID.ReadOnly = false;
            txtProductID.BackColor = Color.White;
            dgvProductCatalog.ClearSelection();
        }
        #endregion
    }
}