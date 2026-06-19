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
        /// 🎯 當前登入員工的職位角色 (enum 表示)
        /// 同時保留字串欄位以利與資料庫互轉
        /// </summary>
        public static AuthorizationHelper.UserRoleEnum LoggedInStaffRoleEnum { get; set; } = AuthorizationHelper.UserRoleEnum.Unknown;

        /// <summary>
        /// 兼容性字串表示，僅供外部舊代碼使用。設定時會嘗試同步 LoggedInStaffRoleEnum
        /// </summary>
        public static string LoggedInStaffRole
        {
            get => AuthorizationHelper.RoleToDbString(LoggedInStaffRoleEnum);
            set => LoggedInStaffRoleEnum = AuthorizationHelper.ParseRole(value);
        }

        /// <summary>
        /// 紀錄員工登入系統的準確時間
        /// </summary>
        public static DateTime LoginTime { get; set; }

        /// <summary>
        /// 中央化資料庫連線字串，供所有表單統一使用。可在發佈或測試時只修改此處即可。
        /// </summary>
        public static string ConnString { get; set; } = "server=127.0.0.1;database=premium_living_db;user=root;password=;port=3306;SslMode=Disabled;";

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

        public static string CurrentUserRole { get; internal set; }

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