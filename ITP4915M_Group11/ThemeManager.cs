using System;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public static class ThemeManager
    {
        // 主題色彩
        public static Color PrimaryBackground = Color.FromArgb(249, 250, 251);
        public static Color PrimaryDark = Color.FromArgb(15, 23, 42);
        public static Color Accent = Color.FromArgb(37, 99, 235);
        public static Font DefaultFont = new Font("Segoe UI", 10F, FontStyle.Regular);

        public static void ApplyTheme(Form form)
        {
            if (form == null) return;

            // 避免在設計器模式下執行
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                return;

            form.BackColor = PrimaryBackground;
            form.Font = DefaultFont;

            // 對所有 child controls 遞迴套用樣式
            ApplyToControls(form.Controls);
        }

        private static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control c in controls)
            {
                try
                {
                    if (c is Panel)
                    {
                        c.BackColor = Color.Transparent;
                    }
                    else if (c is Label)
                    {
                        c.ForeColor = PrimaryDark;
                    }
                    else if (c is Button btn)
                    {
                        btn.BackColor = Color.White;
                        btn.ForeColor = PrimaryDark;
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
                        btn.Font = new Font(DefaultFont.FontFamily, 10.5F, FontStyle.Bold);
                    }
                    else if (c is TextBox tb)
                    {
                        tb.BackColor = Color.White;
                        tb.ForeColor = Color.FromArgb(15, 23, 42);
                        tb.BorderStyle = BorderStyle.FixedSingle;
                        tb.Font = new Font(DefaultFont.FontFamily, 11F, FontStyle.Regular);
                    }
                    else if (c is DataGridView dgv)
                    {
                        dgv.BackgroundColor = Color.White;
                        dgv.EnableHeadersVisualStyles = false;
                        dgv.ColumnHeadersDefaultCellStyle.BackColor = PrimaryDark;
                        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                        dgv.DefaultCellStyle.SelectionBackColor = Accent;
                    }
                }
                catch { }

                // 如果有子控制項，遞迴
                if (c.HasChildren)
                    ApplyToControls(c.Controls);
            }
        }
    }
}
