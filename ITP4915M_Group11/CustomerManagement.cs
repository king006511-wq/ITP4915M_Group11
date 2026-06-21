using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class CustomerManagement : Form
    {
        // ==========================================
        // 🔒 資料庫配置
        // ==========================================
        private readonly string connString = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

        // ==========================================
        // 🎨 現代化 UI 元件變數
        // ==========================================
        private TextBox txtCustomerID, txtName, txtPhone, txtAddress, txtSearch;
        private ComboBox cboType;
        private DataGridView dgvCustomer;
        private Button btnAdd, btnUpdate, btnDelete, btnReset;

        public CustomerManagement()
        {
            InitializeComponent();
            InitializePremiumCustomerUI(); // 動態渲染國際企業級 UI
            SetupTypeComboBox();          // 設定 B2B/B2C 下拉選單
            LoadCustomerData();           // 載入客戶清單
            GenerateNextCustomerID();     // 自動生成下一個客戶 ID
        }

        #region 🎨 動態企業級 UI 渲染 (Fixed Layout & Overlaps)
        #region 🎨 動態企業級 UI 渲染 (Bulletproof Docking Layout)
        private void InitializePremiumCustomerUI()
        {
            this.Text = "Premium Living Furniture - Client Relationship Management (CRM)";
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.None; // Seamless embedding

            // 1. 頂部深色導覽列 (Dock = Top, Fixed Height)
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(15, 23, 42)
            };
            Label lblTitle = new Label
            {
                Text = "GLOBAL CLIENT REGISTRY METRICS",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 16),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblTitle);
            this.Controls.Add(headerPanel);

            // 2. 主容器 (Fills the rest of the window, acts as a safe zone)
            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20) // Adds a 20px gap around all edges
            };
            this.Controls.Add(contentPanel);
            contentPanel.BringToFront(); // Ensures it sits below the header

            // ==========================================
            // 3. 左側：資料輸入面板 (Dock = Left, Fixed Width)
            // ==========================================
            Panel inputPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 340,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblSection = new Label { Text = "Client Profile Master", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(30, 41, 59), Location = new Point(15, 15), AutoSize = true };
            inputPanel.Controls.Add(lblSection);

            string[] labels = { "Customer ID *", "Client Name *", "Account Type *", "Contact Phone *", "Billing Address" };
            int startY = 55;

            txtCustomerID = new TextBox { Location = new Point(15, startY + 20), Size = new Size(300, 25), ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249) };
            txtName = new TextBox { Location = new Point(15, startY + 80), Size = new Size(300, 25) };
            cboType = new ComboBox { Location = new Point(15, startY + 140), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            txtPhone = new TextBox { Location = new Point(15, startY + 200), Size = new Size(300, 25) };
            txtAddress = new TextBox { Location = new Point(15, startY + 260), Size = new Size(300, 60), Multiline = true };

            for (int i = 0; i < labels.Length; i++)
            {
                inputPanel.Controls.Add(new Label { Text = labels[i], Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(15, startY + (i * 60)), AutoSize = true });
            }

            inputPanel.Controls.Add(txtCustomerID);
            inputPanel.Controls.Add(txtName);
            inputPanel.Controls.Add(cboType);
            inputPanel.Controls.Add(txtPhone);
            inputPanel.Controls.Add(txtAddress);

            btnAdd = CreateStyledButton("Add Client", Color.FromArgb(14, 165, 233), Color.White, new Point(15, 395), new Size(145, 32));
            btnUpdate = CreateStyledButton("Update Profile", Color.FromArgb(245, 158, 11), Color.White, new Point(170, 395), new Size(145, 32));
            btnDelete = CreateStyledButton("Purge Record", Color.FromArgb(239, 68, 68), Color.White, new Point(15, 435), new Size(145, 32));
            btnReset = CreateStyledButton("Reset Workspace", Color.FromArgb(100, 116, 139), Color.White, new Point(170, 435), new Size(145, 32));

            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnReset.Click += (s, e) => ClearWorkspace();

            inputPanel.Controls.Add(btnAdd);
            inputPanel.Controls.Add(btnUpdate);
            inputPanel.Controls.Add(btnDelete);
            inputPanel.Controls.Add(btnReset);

            // ==========================================
            // 4. 右側：搜尋與資料表格面板 (Dock = Fill, Flexible)
            // ==========================================
            Panel rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 0, 0, 0) // Creates a 20px gap from the Left Panel
            };

            // 搜尋列區塊 (Dock = Top inside Right Panel)
            Panel searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40
            };
            Label lblSearch = new Label { Text = "🔍 Global Query Filter :", Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(0, 5), AutoSize = true };
            txtSearch = new TextBox
            {
                Location = new Point(150, 2),
                Size = new Size(300, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            txtSearch.TextChanged += txtSearch_TextChanged;
            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);

            // 資料表格 (Dock = Fill inside Right Panel)
            dgvCustomer = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvCustomer.CellClick += dgvCustomer_CellClick;

            // Build Right Panel (Order matters here for Docking to work properly)
            rightPanel.Controls.Add(dgvCustomer);  // Add Grid First (Fill)
            rightPanel.Controls.Add(searchPanel);  // Add Search Second (Top)

            // Build Content Panel (Order matters: Fill goes first, then Left)
            contentPanel.Controls.Add(rightPanel);
            contentPanel.Controls.Add(inputPanel);
        }
        #endregion

        private Button CreateStyledButton(string text, Color backColor, Color foreColor, Point location, Size size)
        {
            Button btn = new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = foreColor,
                Location = location,
                Size = size,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void SetupTypeComboBox()
        {
            cboType.Items.Clear();
            cboType.Items.Add("B2B");
            cboType.Items.Add("B2C");
            cboType.SelectedIndex = 0;
        }
        #endregion

        #region 💾 資料庫商業邏輯 (CRUD Framework)

        private void LoadCustomerData(string filter = "")
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    string query = "SELECT CustomerID, Name, Type, Phone, Address FROM customer WHERE 1=1";
                    if (!string.IsNullOrEmpty(filter))
                    {
                        query += " AND (CustomerID LIKE @filter OR Name LIKE @filter OR Phone LIKE @filter)";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(filter))
                            cmd.Parameters.AddWithValue("@filter", "%" + filter + "%");

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dgvCustomer.DataSource = dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to connect database system:\n" + ex.Message, "Infrastructure Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CustomerManagement_Load(object sender, EventArgs e)
        {

        }

        private void GenerateNextCustomerID()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    string query = "SELECT CustomerID FROM customer ORDER BY CustomerID DESC LIMIT 1";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            string lastID = result.ToString(); // 例如 "C010"
                            if (lastID.StartsWith("C") && int.TryParse(lastID.Substring(1), out int num))
                            {
                                txtCustomerID.Text = "C" + (num + 1).ToString("D3");
                                return;
                            }
                        }
                        txtCustomerID.Text = "C001";
                    }
                }
            }
            catch { txtCustomerID.Text = "C001"; }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text.Trim()) || string.IsNullOrEmpty(txtPhone.Text.Trim()))
            {
                MessageBox.Show("Mandatory validation fault:\nClient Name and Phone fields cannot be blank.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    string query = "INSERT INTO customer (CustomerID, Name, Type, Phone, Address) VALUES (@id, @name, @type, @phone, @address)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtCustomerID.Text.Trim());
                        cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@type", cboType.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@address", txtAddress.Text.Trim());

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("New client organization synchronized successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearWorkspace();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Transaction execution aborted:\n" + ex.Message, "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                MessageBox.Show("Client Name is required for core updates.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    string query = "UPDATE customer SET Name = @name, Type = @type, Phone = @phone, Address = @address WHERE CustomerID = @id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", txtCustomerID.Text.Trim());
                        cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@type", cboType.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@address", txtAddress.Text.Trim());

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            MessageBox.Show("Client profile alterations updated globally.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearWorkspace();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update execution rejected:\n" + ex.Message, "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCustomerID.Text)) return;

            DialogResult confirm = MessageBox.Show($"Are you sure you want to permanently delete client {txtCustomerID.Text}?\nThis may violate data links if they have active orders.", "Critical Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connString))
                    {
                        conn.Open();
                        string query = "DELETE FROM customer WHERE CustomerID = @id";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", txtCustomerID.Text.Trim());
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Client master records purged from the database maps.", "Purge Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearWorkspace();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Purge refused due to relational integrity constraint (Active orders exist).\nDetails: " + ex.Message, "Integrity Check Blocked", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvCustomer_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvCustomer.Rows[e.RowIndex];
                txtCustomerID.Text = row.Cells["CustomerID"].Value.ToString();
                txtName.Text = row.Cells["Name"].Value.ToString();
                cboType.SelectedItem = row.Cells["Type"].Value.ToString();
                txtPhone.Text = row.Cells["Phone"].Value.ToString();
                txtAddress.Text = row.Cells["Address"].Value.ToString();

                // 選取狀態下，新增按鈕反灰鎖定，避免編號覆寫
                btnAdd.Enabled = false;
                btnAdd.BackColor = Color.LightGray;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadCustomerData(txtSearch.Text.Trim());
        }

        private void ClearWorkspace()
        {
            txtName.Clear();
            txtPhone.Clear();
            txtAddress.Clear();
            txtSearch.Clear();
            if (cboType.Items.Count > 0) cboType.SelectedIndex = 0;

            btnAdd.Enabled = true;
            btnAdd.BackColor = Color.FromArgb(14, 165, 233);

            LoadCustomerData();
            GenerateNextCustomerID(); // 重新整理並指派全新編號
        }
        #endregion
    }
}