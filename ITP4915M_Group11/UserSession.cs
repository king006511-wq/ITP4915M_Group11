using System;

namespace ITP4915M_Group11
{
    /// <summary>
    /// 全局靜態類別 (Static Class)，用於儲存整個系統運行期間的登入者狀態 (Session)
    /// </summary>
    public static class UserSession
    {
        // ==========================================
        // 👤 登入用戶屬性 (Properties)
        // ==========================================

        /// <summary>
        /// 當前登入員工的 ID (例如: "S001")
        /// </summary>
        public static string LoggedInStaffID { get; set; }

        /// <summary>
        /// 當前登入員工的名稱 (例如: "Chan Tai Man")
        /// </summary>
        public static string LoggedInStaffName { get; set; }

        /// <summary>
        /// 當前登入員工的部門 (例如: "IT", "Sales", "Warehouse")
        /// </summary>
        public static string LoggedInDepartment { get; set; }

        /// <summary>
        /// 🎯 當前登入員工的職位角色 (例如: "Manager", "Administrator", "Sales Representative")
        /// 用於極度精準的頁面權限鎖定 (RBAC 角色存取控制)
        /// </summary>
        public static string LoggedInStaffRole { get; set; }

        /// <summary>
        /// 紀錄員工登入系統的準確時間
        /// </summary>
        public static DateTime LoginTime { get; set; }

        // ==========================================
        // 🛠️ 系統操作方法 (Methods)
        // ==========================================

        /// <summary>
        /// 判斷當前是否有用戶登入中
        /// 只要 LoggedInStaffID 唔係空，就代表已登入
        /// </summary>
        public static bool IsLoggedIn
        {
            get { return !string.IsNullOrEmpty(LoggedInStaffID); }
        }

        /// <summary>
        /// 登出時呼叫此方法，清空所有 Session 資料，防止下一個使用者睇到舊資料
        /// </summary>
        public static void ClearSession()
        {
            LoggedInStaffID = null;
            LoggedInStaffName = null;
            LoggedInDepartment = null;
            LoggedInStaffRole = null; // ⬅️ 登出時同步清空角色資訊
            LoginTime = DateTime.MinValue;
        }
    }
}