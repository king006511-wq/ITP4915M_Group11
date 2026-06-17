using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public partial class MainDashboard : Form
    {
        public MainDashboard()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                // ThemeManager.ApplyTheme(this); 
                InitializePremiumModernUI();
            }
        }

        private void InitializePremiumModernUI()
        {
            this.Controls.Clear();
            this.Text = "Premium Living Furniture - Enterprise Management System";
            this.Size = new Size(1180, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(249, 250, 251);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // 1. Top Dark Header
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(15, 23, 42)
            };

            Label lblLogo = new Label
            {
                Text = "Premium Living Enterprise Management System",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 22),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblLogo);

            // 🚪 Logout Button
            Button btnLogout = new Button
            {
                Text = "🚪 Logout",
                Location = new Point(1020, 20),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;

            // 🎯 FIXED LOGOUT ROUTER: Restarts the engine back to Login.cs baseline context
            btnLogout.Click += (s, e) => {
                DialogResult result = MessageBox.Show("Are you sure you want to log out and close all active management windows?", "Logout Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    // 1. Instantly wipe current authorization sessions properties
                    if (UserSession.LoggedInStaffID != null) UserSession.LoggedInStaffID = "";
                    if (UserSession.LoggedInStaffName != null) UserSession.LoggedInStaffName = "";
                    if (UserSession.LoggedInStaffRole != null) UserSession.LoggedInStaffRole = "";

                    // 2. Shut down every window and cleanly restart the app back to the startup form
                    Application.Restart();
                }
            };

            // Home Button
            Button btnHome = new Button
            {
                Text = "🏠 Home",
                Location = new Point(900, 20),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnHome.FlatAppearance.BorderSize = 0;
            btnHome.Click += (s, e) => { InitializePremiumModernUI(); };

            pnlHeader.Controls.Add(btnHome);
            pnlHeader.Controls.Add(btnLogout);
            this.Controls.Add(pnlHeader);

            // 2. Core Modules
            string[] modules = {
                "🛒 Sales Order Mgmt", "🚚 Delivery Logistics",
                "🛋️ Product Maintenance", "👔 HR / Staff Mgmt",
                "📦 Goods Received (GRN)", "🏭 Material Requests",
                "📊 Procurement Control", "🔧 Customer Support"
            };

            // 3. Dynamic Card Grid Layout Construction
            int startX = 55;
            int startY = 160;
            int col = 0;

            foreach (string mod in modules)
            {
                Button btnModule = new Button
                {
                    Text = mod,
                    Location = new Point(startX + (col * 265), startY),
                    Size = new Size(235, 140),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(15, 23, 42),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                btnModule.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
                btnModule.FlatAppearance.BorderSize = 2;

                // Hover Effects
                btnModule.MouseEnter += (s, e) => { btnModule.BackColor = Color.FromArgb(241, 245, 249); btnModule.FlatAppearance.BorderColor = Color.FromArgb(37, 99, 235); };
                btnModule.MouseLeave += (s, e) => { btnModule.BackColor = Color.White; btnModule.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240); };

                // 根據角色隱藏或停用按鈕（UI 層）
                var role = AuthorizationHelper.ParseRole(UserSession.LoggedInStaffRole);
                bool canOpen = true;
                if (mod.Contains("HR")) canOpen = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator);
                if (mod.Contains("Sales Order")) canOpen = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.SalesRepresentative);
                if (mod.Contains("Goods Received")) canOpen = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.WarehouseSpecialist);
                if (mod.Contains("Procurement")) canOpen = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.ProcurementOfficer);
                if (mod.Contains("Logistics")) canOpen = AuthorizationHelper.IsInRoleEnum(AuthorizationHelper.UserRoleEnum.Manager, AuthorizationHelper.UserRoleEnum.Administrator, AuthorizationHelper.UserRoleEnum.LogisticsDriver);

                if (!canOpen)
                {
                    btnModule.Enabled = false;
                    btnModule.BackColor = Color.FromArgb(243, 244, 246);
                    btnModule.ForeColor = Color.FromArgb(148, 163, 184);
                    btnModule.Cursor = Cursors.Default;
                }

                // Module Router Mapping Linkage
                btnModule.Click += (s, e) => {
                    Form target = null;
                    try
                    {
                        if (mod.Contains("Sales Order")) target = new OrderManagementForm();
                        else if (mod.Contains("Logistics")) target = new LogisticsForm();
                        else if (mod.Contains("Product")) target = new ProductManagement();
                        else if (mod.Contains("HR")) target = new EmployeeManagement();
                        else if (mod.Contains("Goods Received")) target = new GoodsReceivedForm();
                        else if (mod.Contains("Material")) target = new RawMaterialRequestForm();
                        else if (mod.Contains("Procurement")) target = new ProcurementForm();
                        else if (mod.Contains("Support")) target = new AfterServiceForm();

                        if (target != null)
                        {
                            // 再次在跳轉前做快速授權檢查，避免程式碼直接呼叫而繞過 UI 控制
                            bool allowed = true;
                            string name = target.GetType().Name;
                            if (name == nameof(EmployeeManagement)) allowed = AuthorizationHelper.IsInRole(AuthorizationHelper.Roles.Manager, AuthorizationHelper.Roles.Administrator);
                            if (name == nameof(OrderManagementForm)) allowed = AuthorizationHelper.IsInRole(AuthorizationHelper.Roles.Manager, AuthorizationHelper.Roles.Administrator, AuthorizationHelper.Roles.Sales);
                            if (name == nameof(GoodsReceivedForm)) allowed = AuthorizationHelper.IsInRole(AuthorizationHelper.Roles.Manager, AuthorizationHelper.Roles.Administrator, AuthorizationHelper.Roles.Warehouse);
                            if (name == nameof(ProcurementForm)) allowed = AuthorizationHelper.IsInRole(AuthorizationHelper.Roles.Manager, AuthorizationHelper.Roles.Administrator, AuthorizationHelper.Roles.Procurement);
                            if (name == nameof(AfterServiceForm)) allowed = AuthorizationHelper.IsInRole(AuthorizationHelper.Roles.Manager, AuthorizationHelper.Roles.Administrator, AuthorizationHelper.Roles.Sales);

                            if (!allowed)
                            {
                                MessageBox.Show("Access Denied: your role cannot open this module.", "Authorization", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                return;
                            }

                            this.Hide();
                            target.FormClosed += (senderForm, args) => this.Show();
                            target.Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Navigation error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                this.Controls.Add(btnModule);

                col++;
                if (col == 4)
                {
                    col = 0;
                    startY += 180;
                }
            }
        }
    }
}