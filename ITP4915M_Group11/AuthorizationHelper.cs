using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ITP4915M_Group11
{
    /// <summary>
    /// 集中授權輔助工具：定義常用角色字串並提供檢查與強制方法
    /// </summary>
    public static class AuthorizationHelper
    {
        // 🌟 向後兼容：舊代碼需要用到的 Roles 字串定義
        public static class Roles
        {
            public const string Manager = "Manager";
            public const string Administrator = "Administrator";
            public const string Sales = "Sales";
            public const string SalesRepresentative = "Sales Representative";
            public const string LogisticsDriver = "Logistics Driver";
            public const string WarehouseSpecialist = "Warehouse Specialist";
            public const string ProcurementOfficer = "Procurement Officer";
            public const string SystemManager = "System Manager";
            public const string Staff = "Staff";
        }

        // 角色型別定義 (新版嚴謹寫法)
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

        // 由 enum 轉換回資料庫定義之字串
        public static string RoleToDbString(UserRoleEnum role)
        {
            switch (role)
            {
                case UserRoleEnum.Manager: return Roles.Manager;
                case UserRoleEnum.Administrator: return Roles.Administrator;
                case UserRoleEnum.SalesRepresentative: return Roles.Sales;
                case UserRoleEnum.LogisticsDriver: return Roles.LogisticsDriver;
                case UserRoleEnum.WarehouseSpecialist: return Roles.WarehouseSpecialist;
                case UserRoleEnum.ProcurementOfficer: return Roles.ProcurementOfficer;
                case UserRoleEnum.SystemManager: return Roles.SystemManager;
                case UserRoleEnum.Staff: return Roles.Staff;
                default: return "Unknown";
            }
        }

        // 🌟 權限對照字典 (集中管理哪一個 MenuID 允許哪些 Role 進入)
        private static readonly Dictionary<string, List<UserRoleEnum>> MenuPermissions = new Dictionary<string, List<UserRoleEnum>>
        {
            { "CUSTOMER_MGMT", new List<UserRoleEnum> { UserRoleEnum.Manager, UserRoleEnum.Administrator, UserRoleEnum.SalesRepresentative } },
            { "SALES_QUOTATION", new List<UserRoleEnum> { UserRoleEnum.Manager, UserRoleEnum.Administrator, UserRoleEnum.SalesRepresentative } },
            { "SALES_ORDER", new List<UserRoleEnum> { UserRoleEnum.Manager, UserRoleEnum.Administrator, UserRoleEnum.SalesRepresentative } },
            { "DELIVERY_LOGISTICS", new List<UserRoleEnum> { UserRoleEnum.Manager, UserRoleEnum.Administrator, UserRoleEnum.LogisticsDriver, UserRoleEnum.WarehouseSpecialist } },
            { "GOODS_RECEIVED", new List<UserRoleEnum> { UserRoleEnum.Manager, UserRoleEnum.Administrator, UserRoleEnum.WarehouseSpecialist } },
            { "PRODUCT_MAINTENANCE", new List<UserRoleEnum> { UserRoleEnum.Manager, UserRoleEnum.Administrator, UserRoleEnum.WarehouseSpecialist } },
            
            // 🌟 新產品研發與 BOM 設定的選單權限
            { "PRODUCT_CREATION_BOM", new List<UserRoleEnum> { UserRoleEnum.Manager, UserRoleEnum.Administrator, UserRoleEnum.WarehouseSpecialist } },
            { "PRODUCT_MANUFACTURING", new List<UserRoleEnum> { UserRoleEnum.Manager, UserRoleEnum.Administrator, UserRoleEnum.WarehouseSpecialist } },
            
            // ✨ 新增：供應商與原材料建立的權限 (開放畀經理、管理員同埋採購員)
            { "SUPPLIER_MATERIAL_CREATION", new List<UserRoleEnum> { UserRoleEnum.Manager, UserRoleEnum.Administrator, UserRoleEnum.ProcurementOfficer } },

            { "MATERIAL_REQUESTS", new List<UserRoleEnum> { UserRoleEnum.Manager, UserRoleEnum.Administrator, UserRoleEnum.WarehouseSpecialist, UserRoleEnum.ProcurementOfficer } },
            { "PROCUREMENT_CONTROL", new List<UserRoleEnum> { UserRoleEnum.Manager, UserRoleEnum.Administrator, UserRoleEnum.ProcurementOfficer } },
            { "HR_STAFF_MGMT", new List<UserRoleEnum> { UserRoleEnum.Manager, UserRoleEnum.Administrator } },
            { "CUSTOMER_SUPPORT", new List<UserRoleEnum> { UserRoleEnum.Manager, UserRoleEnum.Administrator, UserRoleEnum.SalesRepresentative } }
        };

        /// <summary>
        /// 檢查當前登入用戶是否有權限訪問特定選單
        /// </summary>
        public static bool HasMenuPermission(string menuId)
        {
            if (string.IsNullOrWhiteSpace(menuId)) return false;

            // 獲取當前用戶的角色
            var currentRole = ParseRole(UserSession.LoggedInStaffRole);
            if (currentRole == UserRoleEnum.Unknown) return false;

            // 檢查菜單是否在權限字典中
            if (!MenuPermissions.ContainsKey(menuId))
            {
                return false;
            }

            // 檢查當前用戶的角色是否在允許列表中
            return MenuPermissions[menuId].Contains(currentRole);
        }

        /// <summary>
        /// 根據菜單 ID 獲取允許訪問的角色清單（用於診斷/調試）
        /// </summary>
        public static HashSet<UserRoleEnum> GetMenuAllowedRoles(string menuId)
        {
            if (MenuPermissions.ContainsKey(menuId))
            {
                return new HashSet<UserRoleEnum>(MenuPermissions[menuId]);
            }
            return new HashSet<UserRoleEnum>();
        }

        /// <summary>
        /// 在表單啟動時強制檢查，若不符則顯示訊息並關閉表單
        /// </summary>
        public static void EnforceRole(Form f, params string[] roles)
        {
            if (!IsInRole(roles))
            {
                MessageBox.Show("Access Denied: insufficient privileges.", "Authorization", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                f.Load += (s, e) => f.Close();
            }
        }

        /// <summary>
        /// 檢查當前用戶是否屬於指定角色清單中的任何一個（寬鬆比對）
        /// </summary>
        public static bool IsInRole(params string[] roles)
        {
            string current = UserSession.LoggedInStaffRole;
            if (string.IsNullOrWhiteSpace(current)) return false;

            foreach (string r in roles)
            {
                if (string.IsNullOrWhiteSpace(r)) continue;
                if (current.Trim().Equals(r.Trim(), StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 🌟 【修復補回方法】檢查當前用戶是否屬於指定的 UserRoleEnum 角色清單之一
        /// 解決 'AuthorizationHelper' 未包含 'IsInRoleEnum' 的定義 報錯
        /// </summary>
        public static bool IsInRoleEnum(params UserRoleEnum[] roles)
        {
            var currentRole = ParseRole(UserSession.LoggedInStaffRole);
            if (currentRole == UserRoleEnum.Unknown) return false;

            foreach (var r in roles)
            {
                if (currentRole == r)
                    return true;
            }
            return false;
        }
    }
}