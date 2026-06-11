using System;
using System.Linq;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    public static class NavigationHelper
    {
        public static void GoToMainDashboard(Form current)
        {
            if (current == null) return;

            // If a MainDashboard instance already exists, show it; otherwise create a new one
            var existing = Application.OpenForms.OfType<MainDashboard>().FirstOrDefault();
            if (existing != null)
            {
                existing.Show();
                existing.BringToFront();
            }
            else
            {
                var dash = new MainDashboard();
                dash.Show();
            }

            // Close the current form
            try { current.Close(); } catch { current.Hide(); }
        }
    }
}
