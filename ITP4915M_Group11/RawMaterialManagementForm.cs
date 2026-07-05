using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class RawMaterialManagementForm : BaseForm
    {
        // 🔒 Database Connection
        private readonly string connString = UserSession.ConnString ?? "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // 🎨 UI 控件
        private DataGridView dgvMaterials;
        private TextBox txtSearch;

        // 編輯區控件
        private TextBox txtMatID;
        private TextBox txtMatName;
        private TextBox txtMatPrice;
        private TextBox txtMatReorderLevel;
        private Button btnUpdate;
        private Button btnClear;

        public RawMaterialManagementForm()
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                ThemeManager.ApplyTheme(this);
                SetupPremiumUI();
                LoadRawMaterials();
            }
        }

        #region 🎨 Premium UI Setup
        private void SetupPremiumUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Raw Material Manager";
            this.Size = new Size(1100, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ThemeManager.PrimaryBackground;
            this.Font = ThemeManager.DefaultFont;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            Panel pnlMain = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(25) };
            this.Controls.Add(pnlMain);

            // Header
            Label lblHeader = new Label { Text = "🪵 Raw Material Database Manager", Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(25, 15), AutoSize = true };
            Label lblSub = new Label { Text = "Update material specifications, base costs, and safety stock levels.", Font = new Font("Segoe UI", 9.5F), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(28, 50), AutoSize = true };
            pnlMain.Controls.Add(lblHeader);
            pnlMain.Controls.Add(lblSub);

            // ================= LEFT PANEL: Monitor & Search =================
            Panel pnlLeft = new Panel { Location = new Point(25, 85), Size = new Size(650, 530), BackColor = ThemeManager.CardBackground, BorderStyle = BorderStyle.FixedSingle };
            pnlLeft.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlLeft.ClientRectangle, ThemeManager.BorderColor, ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlLeft);

            Label lblSearch = new Label { Text = "🔍 Search by ID or Name:", Location = new Point(15, 15), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            txtSearch = new TextBox { Location = new Point(200, 13), Width = 430, Font = new Font("Segoe UI", 10F), BorderStyle = BorderStyle.FixedSingle };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            pnlLeft.Controls.Add(lblSearch);
            pnlLeft.Controls.Add(txtSearch);

            dgvMaterials = new DataGridView
            {
                Location = new Point(15, 55),
                Size = new Size(615, 455),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                GridColor = Color.FromArgb(226, 232, 240),
                EnableHeadersVisualStyles = false
            };
            dgvMaterials.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(79, 70, 229); // Indigo
            dgvMaterials.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvMaterials.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvMaterials.ColumnHeadersHeight = 35;
            dgvMaterials.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvMaterials.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
            dgvMaterials.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvMaterials.SelectionChanged += DgvMaterials_SelectionChanged;
            pnlLeft.Controls.Add(dgvMaterials);

            // ================= RIGHT PANEL: Editor =================
            Panel pnlRight = new Panel { Location = new Point(695, 85), Size = new Size(365, 530), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlRight.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlRight.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            pnlMain.Controls.Add(pnlRight);

            Label lblEditorTitle = new Label { Text = "✏️ Material Details Editor", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(13, 148, 136), Location = new Point(20, 15), AutoSize = true };
            pnlRight.Controls.Add(lblEditorTitle);

            int startY = 60;
            txtMatID = CreateInputField(pnlRight, ref startY, "Material ID (Read-Only):", true);
            txtMatName = CreateInputField(pnlRight, ref startY, "Material Name *:", false);
            txtMatPrice = CreateInputField(pnlRight, ref startY, "Standard Base Cost ($) *:", false);
            txtMatReorderLevel = CreateInputField(pnlRight, ref startY, "Safety Stock Level *:", false);

            btnUpdate = new Button { Text = "💾 Update Material", Location = new Point(20, startY + 20), Size = new Size(320, 45), BackColor = Color.FromArgb(34, 197, 94), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.Click += BtnUpdate_Click;
            pnlRight.Controls.Add(btnUpdate);

            btnClear = new Button { Text = "🧹 Clear Selection", Location = new Point(20, startY + 75), Size = new Size(320, 40), BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, e) => ClearEditor();
            pnlRight.Controls.Add(btnClear);
        }

        private TextBox CreateInputField(Panel container, ref int topY, string labelText, bool readOnly)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(20, topY), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            TextBox txt = new TextBox { Location = new Point(20, topY + 22), Width = 320, Font = new Font("Segoe UI", 10.5F), BorderStyle = BorderStyle.FixedSingle };

            if (readOnly)
            {
                txt.ReadOnly = true;
                txt.BackColor = Color.FromArgb(241, 245, 249);
                txt.ForeColor = Color.Gray;
            }

            container.Controls.Add(lbl);
            container.Controls.Add(txt);
            topY += 65;
            return txt;
        }
        #endregion

        #region 💽 Data Binding & Logic
        private void LoadRawMaterials()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT MaterialID AS 'ID', MaterialName AS 'Material Name', 
                                            StandardCost AS 'Base Cost ($)', StockLevel AS 'Current Stock', 
                                            ReorderLevel AS 'Reorder Lvl' 
                                     FROM raw_material ORDER BY MaterialID ASC";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvMaterials.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load materials database:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (dgvMaterials.DataSource is DataTable dt)
            {
                string keyword = txtSearch.Text.Trim().Replace("'", "''");
                if (string.IsNullOrWhiteSpace(keyword))
                    dt.DefaultView.RowFilter = "";
                else
                    dt.DefaultView.RowFilter = string.Format("ID LIKE '%{0}%' OR [Material Name] LIKE '%{0}%'", keyword);
            }
        }

        private void DgvMaterials_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvMaterials.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvMaterials.SelectedRows[0];
                txtMatID.Text = row.Cells["ID"].Value?.ToString() ?? "";
                txtMatName.Text = row.Cells["Material Name"].Value?.ToString() ?? "";
                txtMatPrice.Text = row.Cells["Base Cost ($)"].Value?.ToString() ?? "";
                txtMatReorderLevel.Text = row.Cells["Reorder Lvl"].Value?.ToString() ?? "";
            }
        }

        private void ClearEditor()
        {
            dgvMaterials.ClearSelection();
            txtMatID.Clear();
            txtMatName.Clear();
            txtMatPrice.Clear();
            txtMatReorderLevel.Clear();
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMatID.Text))
            {
                MessageBox.Show("Please select a material from the list on the left to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string matId = txtMatID.Text.Trim();
            string matName = txtMatName.Text.Trim();
            string priceStr = txtMatPrice.Text.Trim();
            string reorderStr = txtMatReorderLevel.Text.Trim();

            // 🛑 防呆驗證
            if (string.IsNullOrWhiteSpace(matName)) { MessageBox.Show("Material Name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!decimal.TryParse(priceStr, out decimal finalPrice) || finalPrice < 0) { MessageBox.Show("Please enter a valid positive number for Base Cost.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!int.TryParse(reorderStr, out int finalReorder) || finalReorder < 0) { MessageBox.Show("Safety Stock Level must be a positive integer.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            DialogResult confirm = MessageBox.Show($"Are you sure you want to update Material [{matId}]?\n\nNew Name: {matName}\nNew Cost: ${finalPrice}\nNew Safety Stock: {finalReorder}", "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string updateSql = "UPDATE raw_material SET MaterialName=@name, StandardCost=@cost, ReorderLevel=@reorder WHERE MaterialID=@id";
                    using (MySqlCommand cmd = new MySqlCommand(updateSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", matName);
                        cmd.Parameters.AddWithValue("@cost", finalPrice);
                        cmd.Parameters.AddWithValue("@reorder", finalReorder);
                        cmd.Parameters.AddWithValue("@id", matId);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            MessageBox.Show($"Material [{matId}] has been successfully updated!", "Update Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // 記住原本選取嘅 ID，等 Refresh 完可以重新 highlight 返
                            string savedID = matId;
                            LoadRawMaterials();

                            // 嘗試重新 highlight 剛剛改完嗰行
                            foreach (DataGridViewRow row in dgvMaterials.Rows)
                            {
                                if (row.Cells["ID"].Value.ToString() == savedID)
                                {
                                    row.Selected = true;
                                    dgvMaterials.FirstDisplayedScrollingRowIndex = row.Index;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update material:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // RawMaterialManagementForm
            // 
            this.ClientSize = new System.Drawing.Size(278, 244);
            this.Name = "RawMaterialManagementForm";
            this.Load += new System.EventHandler(this.RawMaterialManagementForm_Load);
            this.ResumeLayout(false);

        }

        private void RawMaterialManagementForm_Load(object sender, EventArgs e)
        {

        }
    }
}