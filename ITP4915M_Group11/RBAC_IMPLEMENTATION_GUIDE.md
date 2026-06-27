# 角色基礎訪問控制 (RBAC) 實現指南

## 概述
此實現提供了一個完整的角色基礎訪問控制系統，根據員工的職位/角色隱藏或顯示側邊菜單按鈕。只有具有相應權限的員工才能看到和訪問相應的功能模塊。

## 架構

### 核心組件

#### 1. **AuthorizationHelper.cs**
包含以下關鍵部分：

- **UserRoleEnum**: 定義所有可能的用戶角色
  - Manager (經理)
  - Administrator (管理員)
  - SalesRepresentative (銷售代表)
  - LogisticsDriver (物流司機)
  - WarehouseSpecialist (倉庫專家)
  - ProcurementOfficer (採購員)
  - SystemManager (系統經理)
  - Staff (職員)

- **MenuPermissions Dictionary**: 定義每個菜單按鈕允許訪問的角色
  ```csharp
  { "CUSTOMER_MGMT", new HashSet<UserRoleEnum> { 
	  UserRoleEnum.Manager, 
	  UserRoleEnum.Administrator, 
	  UserRoleEnum.SalesRepresentative,
	  UserRoleEnum.Staff
  }}
  ```

- **HasMenuPermission(string menuId)**: 檢查當前用戶是否有權訪問指定菜單
  ```csharp
  if (AuthorizationHelper.HasMenuPermission("CUSTOMER_MGMT"))
  {
	  // 顯示客戶管理按鈕
  }
  ```

#### 2. **UserSession.cs**
存儲當前登入用戶的信息：
- LoggedInStaffID: 員工 ID
- LoggedInStaffName: 員工名稱
- LoggedInDepartment: 部門
- LoggedInStaffRole: 職位/角色
- LoggedInStaffRoleEnum: 角色枚舉

#### 3. **MainDashboard.cs**
主儀表板，動態生成菜單：

- **SetupNavigationMenu()**: 根據用戶權限設置菜單項
  - 檢查用戶是否有權訪問"核心模塊"部分
  - 檢查用戶是否有權訪問"內部營運"部分
  - 只顯示有相應權限的按鈕

- **AddNavButton(string text, Type formType, string actionType, string menuId)**: 
  - 檢查菜單權限
  - 如果沒有權限，直接返回不添加按鈕

## 菜單權限配置

### 當前配置

| 菜單項 | 菜單ID | 允許的角色 |
|--------|--------|----------|
| Home Dashboard | HOME | 所有角色 |
| Customer Mgmt | CUSTOMER_MGMT | Manager, Administrator, SalesRepresentative, Staff |
| Sales Order Mgmt | SALES_ORDER | Manager, Administrator, SalesRepresentative, Staff |
| Delivery Logistics | DELIVERY_LOGISTICS | Manager, Administrator, LogisticsDriver, Staff |
| Goods Received | GOODS_RECEIVED | Manager, Administrator, WarehouseSpecialist, Staff |
| Product Maintenance | PRODUCT_MAINTENANCE | Manager, Administrator, SystemManager |
| Material Requests | MATERIAL_REQUESTS | Manager, Administrator, WarehouseSpecialist, Staff |
| Procurement Control | PROCUREMENT_CONTROL | Manager, Administrator, ProcurementOfficer, Staff |
| HR / Staff Mgmt | HR_STAFF_MGMT | Manager, Administrator, SystemManager |
| Customer Support | CUSTOMER_SUPPORT | Manager, Administrator, SalesRepresentative, Staff |

## 修改權限規則

### 添加新的菜單項權限

1. 在 AuthorizationHelper.cs 中的 MenuPermissions 字典中添加新項：

```csharp
private static readonly Dictionary<string, HashSet<UserRoleEnum>> MenuPermissions = new Dictionary<string, HashSet<UserRoleEnum>>
{
	// ... 現有項 ...

	{ "NEW_MODULE", new HashSet<UserRoleEnum> { 
		UserRoleEnum.Manager, 
		UserRoleEnum.Administrator
	}}
};
```

2. 在 MainDashboard.cs 的 SetupNavigationMenu() 中使用新的菜單 ID：

```csharp
AddNavButton("📊 New Module", typeof(NewModuleForm), "FORM", "NEW_MODULE");
```

### 為現有菜單添加新角色

在 MenuPermissions 字典中的相應菜單項中添加新角色：

```csharp
{ "CUSTOMER_MGMT", new HashSet<UserRoleEnum> { 
	UserRoleEnum.Manager, 
	UserRoleEnum.Administrator, 
	UserRoleEnum.SalesRepresentative,
	UserRoleEnum.Staff,
	UserRoleEnum.LogisticsDriver  // 新添加的角色
}}
```

## 權限檢查流程

1. **登錄後**: UserSession 設置當前用戶的角色
   ```csharp
   UserSession.LoggedInStaffRole = "Manager";
   ```

2. **菜單加載**: SetupNavigationMenu() 被調用
   - 循環遍歷每個菜單項
   - 調用 AuthorizationHelper.HasMenuPermission(menuId)

3. **權限檢查**: HasMenuPermission() 方法
   - 獲取當前用戶角色
   - 查找菜單 ID 對應的允許角色
   - 比較當前角色是否在允許列表中
   - 返回 true/false

4. **按鈕顯示**:
   - 如果 HasMenuPermission 返回 true，按鈕被添加到菜單
   - 如果返回 false，按鈕不被添加

## 測試指南

### 測試不同角色

1. **Manager 角色測試**:
   - 登錄為 Manager
   - 應該看到所有菜單項（Home Dashboard 除外所有項都應顯示）

2. **SalesRepresentative 角色測試**:
   - 登錄為 SalesRepresentative
   - 應該看到：Customer Mgmt, Sales Order Mgmt, Customer Support
   - 不應該看到：Product Maintenance, HR / Staff Mgmt, Material Requests 等

3. **LogisticsDriver 角色測試**:
   - 登錄為 LogisticsDriver
   - 應該看到：Delivery Logistics
   - 不應該看到其他核心模塊

4. **WarehouseSpecialist 角色測試**:
   - 登錄為 WarehouseSpecialist
   - 應該看到：Goods Received, Material Requests
   - 不應該看到：Customer Mgmt, Sales Order Mgmt 等

## 故障排除

### 菜單項不出現

1. 檢查 UserSession.LoggedInStaffRole 是否正確設置
2. 檢查 MenuPermissions 字典中是否有該菜單 ID
3. 檢查該菜單 ID 對應的角色集合中是否包含當前用戶角色
4. 驗證角色字符串是否與 UserRoleEnum 正確對應

### 不應該看到的菜單項仍然出現

1. 檢查 SetupNavigationMenu() 中是否調用了權限檢查
2. 確認 AddNavButton() 的最後一個參數（menuId）正確傳遞
3. 驗證 MenuPermissions 字典中該菜單 ID 的角色配置

## 安全最佳實踐

1. **驗證**: 即使按鈕被隱藏，仍然應在對應表單的 Load 事件中驗證用戶權限
   ```csharp
   private void CustomerManagement_Load(object sender, EventArgs e)
   {
	   AuthorizationHelper.EnforceRole(this, 
		   AuthorizationHelper.Roles.Manager,
		   AuthorizationHelper.Roles.Administrator,
		   AuthorizationHelper.Roles.Sales
	   );
	   // ... 加載數據
   }
   ```

2. **多層防護**: 
   - UI 層隱藏不允許的按鈕（用戶體驗）
   - 業務邏輯層驗證用戶權限（安全性）
   - 數據庫層驗證用戶權限（數據保護）

3. **審計日誌**: 考慮記錄所有權限檢查失敗的情況，以便審計和安全監控

## 相關文件

- `AuthorizationHelper.cs` - 權限檢查邏輯
- `UserSession.cs` - 用戶會話管理
- `MainDashboard.cs` - 菜單 UI 實現
- `Login.cs` - 登錄流程（應設置 UserSession）

## 示例代碼

### 在其他表單中使用權限檢查

```csharp
public partial class CustomerManagement : Form
{
	private void CustomerManagement_Load(object sender, EventArgs e)
	{
		// 強制檢查：只有這些角色可以訪問此表單
		AuthorizationHelper.EnforceRole(this, 
			AuthorizationHelper.Roles.Manager,
			AuthorizationHelper.Roles.Administrator,
			AuthorizationHelper.Roles.Sales
		);

		// 或者使用枚舉方式：
		// AuthorizationHelper.EnforceRole(this,
		//     AuthorizationHelper.UserRoleEnum.Manager,
		//     AuthorizationHelper.UserRoleEnum.Administrator,
		//     AuthorizationHelper.UserRoleEnum.SalesRepresentative
		// );
	}
}
```

### 條件顯示 UI 元素

```csharp
if (AuthorizationHelper.HasMenuPermission("CUSTOMER_MGMT"))
{
	btnCustomerMgmt.Visible = true;
}
else
{
	btnCustomerMgmt.Visible = false;
}
```

## 總結

此 RBAC 系統提供了：
- ✅ 根據角色動態隱藏/顯示菜單項
- ✅ 集中式權限管理（易於擴展和修改）
- ✅ 清晰的代碼結構和文檔
- ✅ 安全的多層防護
- ✅ 靈活的角色定義和權限配置

確保在登錄流程中正確設置 UserSession 的角色信息，系統就能自動按權限控制菜單顯示。
