using System;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public static class ThemeManager
    {
        // 主題色彩
        public static Color PrimaryBackground = Color.FromArgb(249, 250, 251);
        public static Color CardBackground = Color.White;
        public static Color PrimaryDark = Color.FromArgb(15, 23, 42);
        public static Color MutedText = Color.FromArgb(100, 116, 139);

        // 主色
        public static Color Accent = ColorTranslator.FromHtml("#2563EB"); // Tailwind blue-600
        public static Color AccentStrong = ColorTranslator.FromHtml("#1E40AF");

        // 🎨 補返呢度：保留原本嘅狀態顏色定義，等其他 form 可以繼續用
        public static Color Success = Color.FromArgb(16, 185, 129);
        public static Color Warning = Color.FromArgb(245, 158, 11);
        public static Color Danger = Color.FromArgb(239, 68, 68);

        public static Color BorderColor = Color.FromArgb(226, 232, 240);
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
                        c.BackColor = CardBackground;
                    }
                    else if (c is Label)
                    {
                        c.ForeColor = PrimaryDark;
                    }
                    else if (c is Button btn)
                    {
                        // 乾淨俐落：只處理預設灰色或冇色嘅掣。如果你有自訂色，佢直接放過！
                        if (btn.BackColor == SystemColors.Control || btn.BackColor == Color.Empty || btn.BackColor == Color.Transparent)
                        {
                            btn.BackColor = Accent;
                        }

                        // 統一字體同框線，唔加任何滑鼠動畫，防止消失 bug
                        btn.ForeColor = Color.White;
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderColor = BorderColor;
                        btn.Font = new Font(DefaultFont.FontFamily, 10.5F, FontStyle.Bold);
                    }
                    else if (c is TextBox tb)
                    {
                        tb.BackColor = Color.White;
                        tb.ForeColor = PrimaryDark;
                        tb.BorderStyle = BorderStyle.FixedSingle;
                        tb.Font = new Font(DefaultFont.FontFamily, 11F, FontStyle.Regular);
                    }
                    else if (c is DataGridView dgv)
                    {
                        dgv.BackgroundColor = CardBackground;
                        dgv.EnableHeadersVisualStyles = false;
                        dgv.ColumnHeadersDefaultCellStyle.BackColor = Accent;
                        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                        dgv.ColumnHeadersDefaultCellStyle.Font = new Font(DefaultFont.FontFamily, 10F, FontStyle.Bold);
                        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
                        dgv.DefaultCellStyle.SelectionForeColor = PrimaryDark;
                        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
                        dgv.RowTemplate.Height = 32;
                    }
                    else if (c is ComboBox cb)
                    {
                        cb.BackColor = Color.White;
                        cb.ForeColor = PrimaryDark;
                        cb.Font = new Font(DefaultFont.FontFamily, 10.5F, FontStyle.Regular);
                    }
                    else if (c is RichTextBox rtb)
                    {
                        rtb.BackColor = Color.White;
                        rtb.ForeColor = PrimaryDark;
                        rtb.BorderStyle = BorderStyle.FixedSingle;
                    }
                    else if (c is PictureBox pb)
                    {
                        pb.BackColor = PrimaryBackground;
                        pb.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    else if (c is RadioButton rb)
                    {
                        rb.ForeColor = PrimaryDark;
                    }
                    else if (c is CheckBox chk)
                    {
                        chk.ForeColor = PrimaryDark;
                    }
                    else if (c is LinkLabel ll)
                    {
                        ll.LinkColor = Accent;
                        ll.ActiveLinkColor = AccentStrong;
                    }
                    else if (c is GroupBox gb)
                    {
                        gb.ForeColor = PrimaryDark;
                        gb.BackColor = CardBackground;
                    }
                }
                catch { }

                // 如果有子控制項，遞迴
                if (c.HasChildren)
                    ApplyToControls(c.Controls);
            }
        }

        // 公開方法：統一 DataGridView 樣式
        public static void StyleDataGrid(DataGridView dgv)
        {
            if (dgv == null) return;

            try
            {
                dgv.SuspendLayout();
                dgv.BackgroundColor = CardBackground;
                dgv.BorderStyle = BorderStyle.None;
                dgv.GridColor = Color.FromArgb(241, 245, 249);
                dgv.EnableHeadersVisualStyles = false;

                dgv.ColumnHeadersDefaultCellStyle.BackColor = Accent;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgv.ColumnHeadersDefaultCellStyle.Font = new Font(DefaultFont.FontFamily, 10F, FontStyle.Bold);
                dgv.ColumnHeadersHeight = 36;

                dgv.RowTemplate.Height = 30;
                dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
                dgv.DefaultCellStyle.SelectionForeColor = PrimaryDark;
                dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

                // 微調欄位樣式
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    col.DividerWidth = 0;
                }
            }
            catch { }
            finally { try { dgv.ResumeLayout(); } catch { } }
        }
    }
}