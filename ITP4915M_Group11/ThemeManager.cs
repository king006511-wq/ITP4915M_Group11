using System;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public static class ThemeManager
    {
        // 主題色彩
        // Design tokens (exact hex -> FromArgb)
        public static Color PrimaryBackground = Color.FromArgb(0xF9, 0xFA, 0xFB); // #F9FAFB
        public static Color CardBackground = Color.FromArgb(0xFF, 0xFF, 0xFF); // #FFFFFF
        public static Color PrimaryDark = Color.FromArgb(0x0F, 0x17, 0x2A); // #0F172A
        public static Color MutedText = Color.FromArgb(0x64, 0x74, 0x8B); // #64748B

        // 主色
        public static Color Accent = Color.FromArgb(0x25, 0x63, 0xEB); // #2563EB (Royal Blue)
        public static Color SecondaryAccent = Color.FromArgb(0x0E, 0xA5, 0xE9); // #0EA5E9 (Teal Blue)
        public static Color AccentStrong = Color.FromArgb(0x1E, 0x40, 0xAF); // #1E40AF

        // 🎨 補返呢度：保留原本嘅狀態顏色定義，等其他 form 可以繼續用
        // Semantic
        public static Color Success = Color.FromArgb(0x10, 0xB9, 0x81); // #10B981
        public static Color Warning = Color.FromArgb(0xF5, 0x9E, 0x0B); // #F59E0B
        public static Color Danger = Color.FromArgb(0xEF, 0x44, 0x44); // #EF4444

        public static Color BorderColor = Color.FromArgb(0xE2, 0xE8, 0xF0); // #E2E8F0
        public static Font DefaultFont = new Font("Segoe UI", 10.5F, FontStyle.Regular);

        // Layout tokens
        public const int LeftCardMinWidth = 400;
        public const int LeftCardMaxWidth = 420;
        public const int GlobalMargin = 20;
        public const int PrimaryButtonHeight = 42; // between 42-45
        public const int SecondaryButtonHeight = 40; // between 40-42
        public const int DataGridRowHeight = 32; // between 32-36
        public static Color ReadOnlyFieldBg = Color.FromArgb(0xF1, 0xF5, 0xF9); // #F1F5F9
        public static Color GridSelectionBg = Color.FromArgb(0xDB, 0xE2, 0xFE); // #DBE2FE
        public static Color StockAlertBg = Color.FromArgb(0xFE, 0xE2, 0xE2); // #FEE2E2
        public static Color StockAlertText = Color.FromArgb(0xB9, 0x1C, 0x1C); // #B91C1C

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

            // 自動修正字級以符合設計規範
            AutoFixFontSizes(form);
        }

        // 自動調整字級，將 Label 與 DataGrid 等控制項套用指定階層字級
        public static void AutoFixFontSizes(Control parent)
        {
            if (parent == null) return;

            try
            {
                foreach (Control c in parent.Controls)
                {
                    if (c is Label lbl)
                    {
                        float size = lbl.Font.Size;
                        bool bold = lbl.Font.Style.HasFlag(FontStyle.Bold);

                        // Module Title: >=18 -> 20 or 18 Bold
                        if (size >= 20)
                        {
                            lbl.Font = new Font(ContainsCJK(lbl.Text) ? "Microsoft JhengHei" : "Segoe UI", 20F, FontStyle.Bold);
                            lbl.ForeColor = PrimaryDark;
                        }
                        else if (size >= 16 || (bold && size >= 14))
                        {
                            // Card / Section Headers
                            lbl.Font = new Font(ContainsCJK(lbl.Text) ? "Microsoft JhengHei" : "Segoe UI", 14F, FontStyle.Bold);
                            lbl.ForeColor = Accent;
                        }
                        else if (bold)
                        {
                            // Field headers / small bold
                            lbl.Font = new Font(ContainsCJK(lbl.Text) ? "Microsoft JhengHei" : "Segoe UI", 11F, FontStyle.Bold);
                        }
                        else
                        {
                            // Standard text
                            if (lbl.Font.Style.HasFlag(FontStyle.Italic))
                                lbl.Font = new Font(ContainsCJK(lbl.Text) ? "Microsoft JhengHei" : "Segoe UI", 9.5F, FontStyle.Italic);
                            else
                                lbl.Font = new Font(ContainsCJK(lbl.Text) ? "Microsoft JhengHei" : "Segoe UI", 10.5F, FontStyle.Regular);
                        }
                    }
                    else if (c is DataGridView dgv)
                    {
                        try
                        {
                            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                            dgv.RowTemplate.Height = Math.Max(DataGridRowHeight, dgv.RowTemplate.Height);
                        }
                        catch { }
                    }

                    if (c.HasChildren) AutoFixFontSizes(c);
                }
            }
            catch { }
        }

        private static void InventoryCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                var dgv = sender as DataGridView;
                if (dgv == null) return;
                int row = e.RowIndex;
                if (row < 0 || row >= dgv.Rows.Count) return;
                var r = dgv.Rows[row];
                object stockObj = null; object reorderObj = null;
                if (dgv.Columns.Contains("Stock")) stockObj = r.Cells["Stock"].Value;
                if (dgv.Columns.Contains("ReorderLevel")) reorderObj = r.Cells["ReorderLevel"].Value;
                if (stockObj == null || reorderObj == null) return;
                if (!decimal.TryParse(stockObj.ToString(), out decimal stockVal)) return;
                if (!decimal.TryParse(reorderObj.ToString(), out decimal reorderVal)) return;
                if (stockVal < reorderVal)
                {
                    r.DefaultCellStyle.BackColor = StockAlertBg;
                    r.DefaultCellStyle.ForeColor = StockAlertText;
                    r.DefaultCellStyle.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
            }
            catch { }
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
                    else if (c is Label lbl)
                    {
                        lbl.ForeColor = PrimaryDark;
                        // enforce Chinese font for labels that contain CJK characters
                        if (ContainsCJK(lbl.Text)) lbl.Font = new Font("Microsoft JhengHei", lbl.Font.Size, lbl.Font.Style);
                    }
                    else if (c is Button btn)
                    {
                        // enforce flat style and borderless
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderSize = 0;
                        btn.FlatAppearance.BorderColor = BorderColor;
                        btn.Font = new Font(DefaultFont.FontFamily, 10.5F, FontStyle.Bold);

                        // enforce default colors only if not explicitly set to custom non-transparent
                        if (btn.BackColor == SystemColors.Control || btn.BackColor == Color.Empty || btn.BackColor == Color.Transparent)
                            btn.BackColor = Accent;
                        btn.ForeColor = Color.White;

                        // enforce height constraints
                        if (btn.Height < PrimaryButtonHeight) btn.Height = PrimaryButtonHeight;

                        // RBAC defensiveness: if disabled, force LightGray background and muted text
                        if (!btn.Enabled)
                        {
                            btn.BackColor = Color.LightGray;
                            btn.ForeColor = MutedText;
                        }
                    }
                    else if (c is TextBox tb)
                    {
                        tb.BorderStyle = BorderStyle.FixedSingle;
                        tb.ForeColor = PrimaryDark;
                        tb.Font = new Font(DefaultFont.FontFamily, 10.5F, FontStyle.Regular);
                        if (tb.ReadOnly)
                        {
                            tb.BackColor = ReadOnlyFieldBg;
                            tb.ForeColor = MutedText;
                        }
                        else
                        {
                            tb.BackColor = CardBackground;
                        }
                    }
                    else if (c is DataGridView dgv)
                    {
                        dgv.SuspendLayout();
                        dgv.BackgroundColor = CardBackground;
                        dgv.BorderStyle = BorderStyle.None;
                        dgv.GridColor = BorderColor;
                        dgv.EnableHeadersVisualStyles = false;

                        dgv.ColumnHeadersDefaultCellStyle.BackColor = Accent;
                        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                        dgv.ColumnHeadersDefaultCellStyle.Font = new Font(DefaultFont.FontFamily, 11F, FontStyle.Bold);
                        dgv.ColumnHeadersHeight = 36;

                        dgv.RowTemplate.Height = DataGridRowHeight;
                        dgv.DefaultCellStyle.SelectionBackColor = GridSelectionBg;
                        dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(0x1E, 0x29, 0x3B); // #1E293B
                        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
                        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                        dgv.MultiSelect = false;

                        // attach inventory low-stock formatter if relevant columns exist
                        try
                        {
                            if (dgv.Columns.Contains("Stock") && dgv.Columns.Contains("ReorderLevel"))
                            {
                                dgv.CellFormatting -= InventoryCellFormatting;
                                dgv.CellFormatting += InventoryCellFormatting;
                            }
                        }
                        catch { }
                        try { dgv.ResumeLayout(); } catch { }
                    }
                    else if (c is ComboBox cb)
                    {
                        cb.BackColor = CardBackground;
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

                dgv.RowTemplate.Height = DataGridRowHeight;
                dgv.DefaultCellStyle.SelectionBackColor = GridSelectionBg;
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

        private static bool ContainsCJK(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (char ch in text)
            {
                // CJK Unified Ideographs: U+4E00 - U+9FFF
                if (ch >= 0x4E00 && ch <= 0x9FFF) return true;
                // Hiragana / Katakana ranges
                if (ch >= 0x3040 && ch <= 0x30FF) return true;
                // Hangul Syllables
                if (ch >= 0xAC00 && ch <= 0xD7AF) return true;
            }
            return false;
        }
    }
}