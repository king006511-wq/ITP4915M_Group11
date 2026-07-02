using System;
using System.Drawing;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public class BaseForm : Form
    {
        protected int LeftCardFixedWidth = 410;

        public BaseForm()
        {
            // Apply global theme safely (skip at design-time)
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                this.BackColor = ThemeManager.PrimaryBackground;
                this.Font = ThemeManager.DefaultFont;
            }

            // Ensure we always recalc layout explicitly
            this.SizeChanged += (s, e) => { try { RecalculateTwoColumnLayout(); } catch { } };
            this.Layout += (s, e) => { try { RecalculateTwoColumnLayout(); } catch { } };
        }

        // Child forms may override this to perform math-based layout updates
        protected virtual void RecalculateTwoColumnLayout()
        {
            // Default implementation does nothing. Derived forms should implement explicit math layout.
        }

        // Helper to position two panels using explicit math offsets following the project's UI/UX standard
        protected void LayoutTwoColumnPanels(Panel leftCard, Control rightContent, int leftWidth = -1)
        {
            if (leftCard == null || rightContent == null) return;
            if (leftWidth > 0) LeftCardFixedWidth = leftWidth;

            this.SuspendLayout();
            leftCard.Size = new Size(LeftCardFixedWidth, Math.Max(200, this.ClientSize.Height - 40));
            leftCard.Location = new Point(25, 20);
            leftCard.BackColor = ThemeManager.CardBackground;

            int rightStartX = leftCard.Right + 30;
            int rightWidth = Math.Max(200, this.ClientSize.Width - rightStartX - 25);
            rightContent.Location = new Point(rightStartX, 20);
            rightContent.Size = new Size(rightWidth, Math.Max(200, this.ClientSize.Height - 40));

            this.ResumeLayout(false);
        }
    }
}
