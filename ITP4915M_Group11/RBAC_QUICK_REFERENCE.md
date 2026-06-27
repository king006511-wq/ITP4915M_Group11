# RBAC 快速參考卡片

## 核心概念
✅ **隱藏按鈕** - 員工只能看到他們有權限的菜單項
✅ **角色檢查** - 根據用戶的職位/角色進行權限驗證
✅ **動態菜單** - 菜單根據登入用戶的角色自動調整

## 角色清單
| 角色代碼 | 角色名稱 | 菜單可見性 |
|---------|--------|----------|
| Manager | 經理 | 所有菜單 |
| Administrator | 管理員 | 所有菜單 |
| SalesRepresentative | 銷售代表 | 客戶、銷售訂單、客支 |
| LogisticsDriver | 物流司機 | 配送物流 |
| WarehouseSpecialist | 倉庫專家 | 入庫單據、物料申請 |
| ProcurementOfficer | 採購員 | 採購控制 |
| SystemManager | 系統經理 | 產品維護、HR管理 |
| Staff | 職員 | 基本功能 |

## 快速配置步驟

### 1️⃣ 設置用戶角色（登錄時）
```csharp
// 在 Login.cs 中
UserSession.LoggedInStaffRole = "Manager";  // 根據數據庫查詢設置
```

### 2️⃣ 添加新菜單按鈕
```csharp
// 在 AuthorizationHelper.cs 中
{ "NEW_MENU_ID", new HashSet<UserRoleEnum> { 
	UserRoleEnum.Manager, 
	UserRoleEnum.Administrator 
}}

// 在 MainDashboard.cs 中
AddNavButton("📊 新菜單", typeof(NewForm), "FORM", "NEW_MENU_ID");
```

### 3️⃣ 修改按鈕權限
```csharp
// 在 MenuPermissions 字典中修改對應菜單項
{ "CUSTOMER_MGMT", new HashSet<UserRoleEnum> { 
	UserRoleEnum.Manager,
	UserRoleEnum.Administrator,
	UserRoleEnum.SalesRepresentative,
	UserRoleEnum.NewRole  // 添加新角色
}}
```

## 關鍵方法

### ✔️ 檢查用戶是否有權訪問菜單
```csharp
if (AuthorizationHelper.HasMenuPermission("CUSTOMER_MGMT"))
{
	// 用戶有權限 - 按鈕將顯示
}
else
{
	// 用戶無權限 - 按鈕不會添加到菜單
}
```

### ✔️ 獲取菜單允許的角色
```csharp
var allowedRoles = AuthorizationHelper.GetMenuAllowedRoles("CUSTOMER_MGMT");
// 返回: { Manager, Administrator, SalesRepresentative, Staff }
```

### ✔️ 檢查用戶角色
```csharp
if (AuthorizationHelper.IsInRole("Manager", "Administrator"))
{
	// 當前用戶是 Manager 或 Administrator
}
```

### ✔️ 在表單中強制執行權限檢查
```csharp
private void CustomerManagement_Load(object sender, EventArgs e)
{
	AuthorizationHelper.EnforceRole(this, 
		AuthorizationHelper.Roles.Manager,
		AuthorizationHelper.Roles.Administrator,
		AuthorizationHelper.Roles.Sales
	);
}
```

## 菜單 ID 清單
```
HOME                    // 首頁儀表板
CUSTOMER_MGMT           // 客戶管理
SALES_ORDER             // 銷售訂單
DELIVERY_LOGISTICS      // 配送物流
GOODS_RECEIVED          // 入庫單據
PRODUCT_MAINTENANCE     // 產品維護
MATERIAL_REQUESTS       // 物料申請
PROCUREMENT_CONTROL     // 採購控制
HR_STAFF_MGMT          // HR/員工管理
CUSTOMER_SUPPORT        // 客戶支持
```

## 測試權限

### 🧪 測試 Manager 角色
- ✅ 應看到所有菜單項
- ✅ 可訪問所有表單

### 🧪 測試 SalesRepresentative 角色
```
應顯示的菜單：
✅ Customer Mgmt
✅ Sales Order Mgmt
✅ Customer Support

不應顯示的菜單：
❌ Product Maintenance
❌ HR / Staff Mgmt
❌ Material Requests
❌ Goods Received
❌ Delivery Logistics
❌ Procurement Control
```

### 🧪 測試 WarehouseSpecialist 角色
```
應顯示的菜單：
✅ Goods Received (GRN)
✅ Material Requests

不應顯示的菜單：
❌ Customer Mgmt
❌ Sales Order Mgmt
❌ 其他菜單項
```

## 故障排除清單

| 問題 | 解決方案 |
|-----|--------|
| 菜單項不顯示 | 檢查 MenuPermissions 中是否包含該菜單 ID |
| 不該顯示的菜單項仍然顯示 | 驗證 SetupNavigationMenu() 中是否調用了權限檢查 |
| 角色不匹配 | 確保 UserSession.LoggedInStaffRole 正確設置 |
| 用戶仍可訪問受限表單 | 在表單 Load 中添加 EnforceRole() 檢查 |

## 文件位置
- 📄 `AuthorizationHelper.cs` - 權限邏輯
- 📄 `MainDashboard.cs` - 菜單實現
- 📄 `UserSession.cs` - 會話管理
- 📚 `RBAC_IMPLEMENTATION_GUIDE.md` - 完整指南

## 常見問題 (FAQ)

**Q: 菜單項被隱藏後，用戶如何直接訪問對應的表單？**
A: 即使菜單按鈕被隱藏，該表單的 Load 事件中也應該有 EnforceRole() 檢查，防止直接訪問。

**Q: 如何添加新的角色？**
A: 
1. 在 UserRoleEnum 中添加新角色
2. 在 ParseRole() 中添加解析規則
3. 在 RoleToDbString() 中添加轉換規則
4. 在 MenuPermissions 中相應位置添加新角色

**Q: 能否動態改變用戶權限而不重新啟動應用？**
A: 可以。修改 UserSession.LoggedInStaffRole 後，調用 MainDashboard 的 InitializePremiumContainerUI() 即可重新加載菜單。

**Q: 如何為不同部門設置不同的菜單？**
A: 可以基於 UserSession.LoggedInDepartment 進行條件檢查，或者創建部門特定的角色映射。

---
最後更新: 2024年
版本: 1.0
