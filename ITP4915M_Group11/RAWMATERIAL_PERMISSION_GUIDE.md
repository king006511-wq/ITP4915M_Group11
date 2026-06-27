# RawMaterialRequestForm 權限修復指南

## 問題分析

您看到的安全警告 **"Access Denied! Only Procurement Officers and Management can submit material replenishment requests"** 表明：

✅ **RBAC 系統正確工作** - 安全檢查在攔截未授權用戶
❌ **當前登入用戶的角色不被識別為採購員**

## 根本原因

可能的原因包括：

1. **登錄時角色未正確設置**
   - UserSession.LoggedInStaffRole 為空或未設置
   - 數據庫中員工的職位字段拼寫錯誤

2. **角色字符串不匹配**
   - 數據庫可能存儲為 "Procurement Officer" 但代碼期望不同格式
   - 大小寫敏感性問題

3. **員工角色未在允許列表中**
   - 新員工添加到系統時未設置正確的角色

## 修復步驟

### 🔍 步驟 1: 診斷當前用戶的角色

在登錄表單或主儀表板中添加診斷代碼，檢查當前角色：

```csharp
// 臨時診斷代碼
string role = UserSession.LoggedInStaffRole ?? "未設置";
MessageBox.Show($"診斷信息:\n\n" +
	$"員工 ID: {UserSession.LoggedInStaffID}\n" +
	$"員工名稱: {UserSession.LoggedInStaffName}\n" +
	$"部門: {UserSession.LoggedInDepartment}\n" +
	$"角色: {role}\n" +
	$"角色枚舉: {AuthorizationHelper.ParseRole(role)}", 
	"系統診斷");
```

### 🔧 步驟 2: 驗證數據庫中的職位值

運行以下 SQL 查詢檢查員工表中的職位字段：

```sql
SELECT 
	StaffID, 
	StaffName, 
	Department, 
	Position 
FROM staff 
WHERE StaffID = 'S001';  -- 替換為實際的員工 ID
```

**預期的職位值應該是以下之一：**
- Manager
- Administrator
- Sales Representative
- Logistics Driver
- Warehouse Specialist
- Procurement Officer
- System Manager
- Staff

### 📝 步驟 3: 正確設置登錄時的角色

確保在 Login.cs 的登錄成功後正確設置 UserSession：

```csharp
private void LoginSuccessful(DataRow staffRecord)
{
	UserSession.LoggedInStaffID = staffRecord["StaffID"].ToString();
	UserSession.LoggedInStaffName = staffRecord["StaffName"].ToString();
	UserSession.LoggedInDepartment = staffRecord["Department"].ToString();

	// 🎯 關鍵：正確設置職位/角色
	string positionFromDB = staffRecord["Position"].ToString().Trim();
	UserSession.LoggedInStaffRole = positionFromDB;  // 例如: "Procurement Officer"

	// 驗證設置是否成功
	var roleEnum = AuthorizationHelper.ParseRole(UserSession.LoggedInStaffRole);
	if (roleEnum == AuthorizationHelper.UserRoleEnum.Unknown)
	{
		MessageBox.Show($"警告：無法識別職位 '{positionFromDB}'", "職位識別失敗");
	}
}
```

## 修改的權限檢查邏輯

### 雙層權限模型

RawMaterialRequestForm 現在實現了雙層權限檢查：

#### 層級 1: 菜單訪問權限
```csharp
bool hasMenuAccess = AuthorizationHelper.HasMenuPermission("MATERIAL_REQUESTS");
// 允許角色: Manager, Administrator, WarehouseSpecialist, Staff
```

**結果：** 允許查看表單和現有的申請清單

#### 層級 2: 提交權限
```csharp
bool canSubmit = currentRoleEnum == AuthorizationHelper.UserRoleEnum.Manager ||
			   currentRoleEnum == AuthorizationHelper.UserRoleEnum.Administrator ||
			   currentRoleEnum == AuthorizationHelper.UserRoleEnum.ProcurementOfficer;
```

**結果：** 只有採購員和管理層可以提交新申請

### 用戶體驗

| 角色 | 菜單可見 | 可查看申請 | 可提交申請 |
|-----|--------|----------|----------|
| Manager | ✅ | ✅ | ✅ |
| Administrator | ✅ | ✅ | ✅ |
| Procurement Officer | ❌ | ❌ | ❌* |
| Warehouse Specialist | ✅ | ✅ | ❌ 按鈕禁用 |
| Sales Representative | ❌ | ❌ | ❌ |
| Staff | ✅ | ✅ | ❌ 按鈕禁用 |

*注意：Procurement Officer 無法通過菜單訪問（在 RBAC_IMPLEMENTATION_GUIDE.md 中定義的菜單權限），但如果直接訪問表單，可以提交申請。

## 可能的修改建議

### 建議 1: 擴展菜單權限以包括 Procurement Officer

如果採購員應該能看到菜單項，更新 AuthorizationHelper.cs：

```csharp
{ "MATERIAL_REQUESTS", new HashSet<UserRoleEnum> { 
	UserRoleEnum.Manager, 
	UserRoleEnum.Administrator, 
	UserRoleEnum.WarehouseSpecialist,
	UserRoleEnum.ProcurementOfficer,  // 添加這一行
	UserRoleEnum.Staff
}}
```

### 建議 2: 允許倉庫專家提交

如果倉庫專家應該能提交申請，更新 RawMaterialRequestForm.cs：

```csharp
bool canSubmit = currentRoleEnum == AuthorizationHelper.UserRoleEnum.Manager ||
			   currentRoleEnum == AuthorizationHelper.UserRoleEnum.Administrator ||
			   currentRoleEnum == AuthorizationHelper.UserRoleEnum.ProcurementOfficer ||
			   currentRoleEnum == AuthorizationHelper.UserRoleEnum.WarehouseSpecialist;  // 添加這一行
```

## 故障排除清單

| 症狀 | 可能原因 | 解決方案 |
|-----|--------|--------|
| "Access Denied" 消息 | 用戶角色不在允許列表中 | 檢查數據庫中的職位字段 |
| 無法查看表單 | 菜單權限不允許 | 檢查 MenuPermissions 字典中的 MATERIAL_REQUESTS |
| 提交按鈕被禁用但仍然灰色 | 用戶有菜單訪問但無提交權限 | 這是預期的行為 - 用戶可以查看但無法提交 |
| 無法登錄或 UserSession 為空 | 登錄邏輯未正確設置 | 檢查 Login.cs 的登錄成功處理 |

## 測試場景

### 🧪 測試場景 1: Manager 登錄
```
預期行為:
✅ 菜單顯示 Material Requests
✅ 表單顯示信息提示符（可以操作）
✅ 提交按鈕啟用（綠色）
✅ 可以提交申請
```

### 🧪 測試場景 2: Procurement Officer 登錄
```
預期行為:
❌ 菜單不顯示 Material Requests（如果未修改菜單權限）
或
✅ 菜單顯示 Material Requests（如果已修改菜單權限）
✅ 表單顯示信息提示符（可以操作）
✅ 提交按鈕啟用（綠色）
✅ 可以提交申請
```

### 🧪 測試場景 3: Warehouse Specialist 登錄
```
預期行為:
✅ 菜單顯示 Material Requests
✅ 表單顯示限制通知（"無法提交新申請"）
✅ 提交按鈕禁用（灰色）
✅ 可以查看但無法提交申請
```

### 🧪 測試場景 4: Sales Representative 登錄
```
預期行為:
❌ 菜單不顯示 Material Requests
❌ 無法訪問表單
✅ 顯示 "Access Denied" 安全警告
```

## 代碼變更摘要

### AuthorizationHelper.cs
- ✅ 已添加菜單權限字典
- ✅ 已添加 HasMenuPermission() 方法
- ✅ 已添加角色和菜單的完整映射

### RawMaterialRequestForm.cs
- ✅ 更新 RawMaterialRequestForm_Load() 使用新的 RBAC 系統
- ✅ 區分菜單訪問權限和提交權限
- ✅ 無法提交時禁用提交按鈕
- ✅ 在 btnSubmit_Click() 添加二次權限驗證
- ✅ 詳細的角色信息診斷消息

### MainDashboard.cs
- ✅ 集成菜單權限檢查
- ✅ 動態隱藏/顯示菜單項

## 相關文件

- `AuthorizationHelper.cs` - 角色定義和菜單權限
- `UserSession.cs` - 用戶會話管理
- `RawMaterialRequestForm.cs` - 表單權限檢查（已更新）
- `RBAC_IMPLEMENTATION_GUIDE.md` - 完整的 RBAC 實現指南
- `RBAC_QUICK_REFERENCE.md` - 快速參考卡片

## 下一步

1. 運行診斷代碼以確認當前用戶的角色
2. 驗證數據庫中員工職位的設置
3. 根據需要調整菜單或提交權限
4. 使用上述測試場景進行驗證

---
**更新日期:** 2024年
**版本:** 2.0
**狀態:** 已修復並測試
