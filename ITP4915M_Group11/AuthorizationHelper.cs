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

        /// <summary>
        /// 菜單按鈕 ID 到所需角色的映射字典
        /// 按鈕 ID 對應菜單按鈕的文本，所需角色為允許訪問該功能的角色清單
        /// </summary>
        private static readonly Dictionary<string, HashSet<UserRoleEnum>> MenuPermissions = new Dictionary<string, HashSet<UserRoleEnum>>
        {
            // Home Dashboard - 所有角色都可以訪問
            { "HOME", new HashSet<UserRoleEnum> { 
                UserRoleEnum.Manager, 
                UserRoleEnum.Administrator, 
                UserRoleEnum.SalesRepresentative,
                UserRoleEnum.LogisticsDriver,
                UserRoleEnum.WarehouseSpecialist,
                UserRoleEnum.ProcurementOfficer,
                UserRoleEnum.SystemManager,
                UserRoleEnum.Staff
            }},

            // Core Modules
            { "CUSTOMER_MGMT", new HashSet<UserRoleEnum> { 
                UserRoleEnum.Manager, 
                UserRoleEnum.Administrator, 
                UserRoleEnum.SalesRepresentative,
                UserRoleEnum.Staff
            }},

            { "SALES_ORDER", new HashSet<UserRoleEnum> { 
                UserRoleEnum.Manager, 
                UserRoleEnum.Administrator, 
                UserRoleEnum.SalesRepresentative,
                UserRoleEnum.Staff
            }},

            { "DELIVERY_LOGISTICS", new HashSet<UserRoleEnum> { 
                UserRoleEnum.Manager, 
                UserRoleEnum.Administrator, 
                UserRoleEnum.LogisticsDriver,
                UserRoleEnum.Staff
            }},

            { "GOODS_RECEIVED", new HashSet<UserRoleEnum> { 
                UserRoleEnum.Manager, 
                UserRoleEnum.Administrator, 
                UserRoleEnum.WarehouseSpecialist,
                UserRoleEnum.Staff
            }},

            { "PRODUCT_MAINTENANCE", new HashSet<UserRoleEnum> { 
                UserRoleEnum.Manager, 
                UserRoleEnum.Administrator, 
                UserRoleEnum.SystemManager
            }},

            // Internal Ops
            { "MATERIAL_REQUESTS", new HashSet<UserRoleEnum> { 
                UserRoleEnum.Manager, 
                UserRoleEnum.Administrator, 
                UserRoleEnum.WarehouseSpecialist,
                UserRoleEnum.Staff
            }},

            { "PROCUREMENT_CONTROL", new HashSet<UserRoleEnum> { 
                UserRoleEnum.Manager, 
                UserRoleEnum.Administrator, 
                UserRoleEnum.ProcurementOfficer,
                UserRoleEnum.Staff
            }},

            { "HR_STAFF_MGMT", new HashSet<UserRoleEnum> { 
                UserRoleEnum.Manager, 
                UserRoleEnum.Administrator,
                UserRoleEnum.SystemManager
            }},

            { "CUSTOMER_SUPPORT", new HashSet<UserRoleEnum> { 
                UserRoleEnum.Manager, 
                UserRoleEnum.Administrator, 
                UserRoleEnum.SalesRepresentative,
                UserRoleEnum.Staff
            }}
        };

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
        /// 檢查當前用戶是否有權訪問指定的菜單按鈕/功能
        /// </summary>
        /// <param name="menuId">菜單按鈕的 ID（如 "CUSTOMER_MGMT", "HOME"）</param>
        /// <returns>如果當前用戶有權訪問該功能，則返回 true；否則返回 false</returns>
        public static bool HasMenuPermission(string menuId)
        {
            if (string.IsNullOrWhiteSpace(menuId)) return false;

            // 獲取當前用戶的角色
            var currentRole = ParseRole(UserSession.LoggedInStaffRole);
            if (currentRole == UserRoleEnum.Unknown) return false;

            // 檢查菜單是否在權限字典中
            if (!MenuPermissions.ContainsKey(menuId))
            {
                // 如果菜單未定義，預設不允許訪問
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
                MessageBox.Show("Access Denied: insufficient privileges.", "Authorization", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                try { f.BeginInvoke(new MethodInvoker(f.Close)); }
                catch { try { f.Close(); } catch { } }
            }
        }
    }
}
