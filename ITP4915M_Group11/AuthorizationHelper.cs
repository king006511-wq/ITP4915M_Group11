using System;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    /// <summary>
    /// 集中授權輔助工具：定義常用角色字串並提供檢查與強制方法
    /// </summary>
    public static class AuthorizationHelper
    {
        // 新增 enum 定義以便程式內部使用更嚴謹的角色型別
        public enum UserRoleEnum
        {
            Unknown = 0,
            Manager,
            Administrator,
            SalesRepresentative,
            LogisticsDriver,
            WarehouseSpecialist,
            ProcurementOfficer,
            SystemManager,
            Staff
        }

        // 由資料庫字串轉換為 enum（寬鬆比對）
        public static UserRoleEnum ParseRole(string roleString)
        {
            if (string.IsNullOrWhiteSpace(roleString)) return UserRoleEnum.Unknown;
            string r = roleString.Trim().ToLowerInvariant();
            if (r == "manager") return UserRoleEnum.Manager;
            if (r == "administrator" || r == "admin") return UserRoleEnum.Administrator;
            if (r == "sales representative" || r == "sales") return UserRoleEnum.SalesRepresentative;
            if (r == "logistics driver" || r == "delivery driver" || r == "delivery representative") return UserRoleEnum.LogisticsDriver;
            if (r == "warehouse specialist" || r == "warehouse") return UserRoleEnum.WarehouseSpecialist;
            if (r == "procurement officer" || r == "procurement") return UserRoleEnum.ProcurementOfficer;
            if (r == "system manager") return UserRoleEnum.SystemManager;
            if (r == "staff") return UserRoleEnum.Staff;
            return UserRoleEnum.Unknown;
        }

        public static class Roles
        {
            public const string Manager = "Manager";
            public const string Administrator = "Administrator";
            public const string Sales = "Sales Representative";
            public const string Logistics = "Logistics Driver";
            public const string Warehouse = "Warehouse Specialist";
            public const string Procurement = "Procurement Officer";
            public const string SystemManager = "System Manager";
        }

        public static bool IsInRole(params string[] roles)
        {
            string current = UserSession.LoggedInStaffRole;
            if (string.IsNullOrWhiteSpace(current)) return false;
            foreach (var r in roles)
            {
                if (string.Equals(current.Trim(), r, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public static bool IsInRoleEnum(params UserRoleEnum[] roles)
        {
            var current = ParseRole(UserSession.LoggedInStaffRole);
            if (current == UserRoleEnum.Unknown) return false;
            foreach (var r in roles) if (current == r) return true;
            return false;
        }

        // 將 enum 轉為資料庫儲存/顯示用的字串
        public static string RoleToDbString(UserRoleEnum role)
        {
            switch (role)
            {
                case UserRoleEnum.Manager: return "Manager";
                case UserRoleEnum.Administrator: return "Administrator";
                case UserRoleEnum.SalesRepresentative: return "Sales Representative";
                case UserRoleEnum.LogisticsDriver: return "Logistics Driver";
                case UserRoleEnum.WarehouseSpecialist: return "Warehouse Specialist";
                case UserRoleEnum.ProcurementOfficer: return "Procurement Officer";
                case UserRoleEnum.SystemManager: return "System Manager";
                case UserRoleEnum.Staff: return "Staff";
                default: return string.Empty;
            }
        }

        public static bool IsInAnyRole(string[] roles) => IsInRole(roles);

        /// <summary>
        /// 在表單啟動時強制檢查，若不符則顯示訊息並關閉表單
        /// </summary>
        public static void EnforceRole(Form f, params string[] roles)
        {
            if (!IsInRole(roles))
            {
                MessageBox.Show("Access Denied: insufficient privileges.", "Authorization", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                try { f.BeginInvoke(new MethodInvoker(f.Close)); }
                catch { try { f.Close(); } catch { } }
            }
        }
    }
}
