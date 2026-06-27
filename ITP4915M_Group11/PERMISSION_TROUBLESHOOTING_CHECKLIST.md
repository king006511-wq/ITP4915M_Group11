# Material Requests 權限問題快速檢查表

## ❌ 錯誤症狀
您看到了：`[SECURITY ALERT] Access Denied! Only Procurement Officers and Management can submit material replenishment requests.`

## ✅ 快速診斷步驟

### 第 1 步：檢查 UserSession 設置
在登錄後或進入表單前，檢查：

```csharp
// 添加到主儀表板或任意地方
string debugMsg = $"角色: {UserSession.LoggedInStaffRole ?? "(未設置)"}\n" +
				 $"部門: {UserSession.LoggedInDepartment ?? "(未設置)"}\n" +
				 $"職員 ID: {UserSession.LoggedInStaffID ?? "(未設置)"}";
MessageBox.Show(debugMsg, "角色診斷");
```

**預期結果：** 角色應該顯示 "Manager", "Procurement Officer" 或其他有效角色

### 第 2 步：檢查數據庫

運行此 SQL 查詢（替換 S001 為實際員工 ID）：

```sql
SELECT StaffID, StaffName, Position, Department FROM staff WHERE StaffID = 'S001';
```

**預期結果：** Position 欄位應該完全符合以下之一：
- `Manager`
- `Administrator`  
- `Procurement Officer`
- `Warehouse Specialist`
- `Sales Representative`
- `Logistics Driver`
- `System Manager`
- `Staff`

### 第 3 步：檢查角色解析

在 AuthorizationHelper.cs 中，確認 ParseRole() 正確識別角色：

```csharp
// 臨時測試代碼
var role = AuthorizationHelper.ParseRole("Procurement Officer");
// 應該返回: UserRoleEnum.ProcurementOfficer (不是 Unknown)
```

## 🔧 常見修復

### 修復 1️⃣：職位拼寫錯誤
**症狀：** 數據庫中職位為 "procurement officer"（小寫）

**解決方案：** 更新數據庫
```sql
UPDATE staff SET Position = 'Procurement Officer' WHERE Position = 'procurement officer';
```

### 修復 2️⃣：角色未在登錄時設置
**症狀：** UserSession.LoggedInStaffRole 為 null 或空字符串

**解決方案：** 在 Login.cs 確保設置
```csharp
// 登錄成功後
UserSession.LoggedInStaffRole = staffRow["Position"].ToString().Trim();
```

### 修復 3️⃣：職位名稱完全不同
**症狀：** 數據庫使用 "採購" 或 "Buyer"，而非 "Procurement Officer"

**解決方案：** 在 AuthorizationHelper.ParseRole() 添加映射
```csharp
public static UserRoleEnum ParseRole(string roleString)
{
	if (string.IsNullOrWhiteSpace(roleString)) return UserRoleEnum.Unknown;
	string r = roleString.Trim().ToLowerInvariant();
	// ... 現有代碼 ...
	if (r == "採購" || r == "buyer") return UserRoleEnum.ProcurementOfficer;  // 添加
	return UserRoleEnum.Unknown;
}
```

## 📋 權限配置檢查

### 菜單級別權限
檢查 AuthorizationHelper.cs 中的 MenuPermissions：

```csharp
// MATERIAL_REQUESTS 應該包含您的角色
{ "MATERIAL_REQUESTS", new HashSet<UserRoleEnum> { 
	UserRoleEnum.Manager,                    // ✓
	UserRoleEnum.Administrator,              // ✓
	UserRoleEnum.WarehouseSpecialist,        // ✓
	UserRoleEnum.ProcurementOfficer,         // ✗ 目前不包括
	UserRoleEnum.Staff                       // ✓
}}
```

**解決方案（如需要）：** 添加 ProcurementOfficer
```csharp
{ "MATERIAL_REQUESTS", new HashSet<UserRoleEnum> { 
	UserRoleEnum.Manager,
	UserRoleEnum.Administrator,
	UserRoleEnum.WarehouseSpecialist,
	UserRoleEnum.ProcurementOfficer,         // 添加這一行
	UserRoleEnum.Staff
}}
```

### 提交按鈕權限
檢查 RawMaterialRequestForm.cs 中的 btnSubmit_Click 權限：

```csharp
// 目前只允許這些角色提交
bool canSubmit = currentRoleEnum == AuthorizationHelper.UserRoleEnum.Manager ||
			   currentRoleEnum == AuthorizationHelper.UserRoleEnum.Administrator ||
			   currentRoleEnum == AuthorizationHelper.UserRoleEnum.ProcurementOfficer;
```

## 🎯 最可能的原因排序

1. **[80% 概率]** 數據庫中的職位字段拼寫不正確或大小寫不符
2. **[15% 概率]** 登錄時未正確設置 UserSession.LoggedInStaffRole
3. **[5% 概率]** 菜單或提交權限配置不允許該角色

## ✨ 驗證修復成功

修復後，執行此測試：

### 以 Procurement Officer 身份登錄
1. ✅ 菜單應該顯示 "Material Requests"（或根據配置不顯示，但可以直接訪問）
2. ✅ 進入 Material Replenishment 表單
3. ✅ 應該看到「限制通知」對話框或提交按鈕啟用
4. ✅ 填寫表單並提交應該成功
5. ✅ 新申請應該出現在「Ongoing Reorder Requests」表格中

### 如果仍然看到 "Access Denied"
檢查：
- [ ] 用戶角色是否正確（運行診斷代碼）
- [ ] 數據庫中職位是否正確拼寫
- [ ] 是否需要重新啟動應用程序
- [ ] 是否需要清除舊的編譯緩存（Clean Solution -> Rebuild）

## 📞 聯繫支持

如果問題仍未解決：
1. 運行上述診斷步驟 1-3
2. 記錄輸出結果
3. 檢查數據庫 SQL 查詢結果
4. 檢查是否需要重新編譯或重啟應用

---
**保存此文件備用** - 當發現類似權限問題時可快速參考

