# RawMaterialRequestForm - English UI Quick Reference

## ✨ Form Overview

### Main Interface
```
┌─────────────────────────────────────────────────────────────────┐
│ Material Replenishment [Form Title]                             │
├─────────────────────┬───────────────────────────────────────────┤
│  INPUT CARD         │  ONGOING REORDER REQUESTS [Grid Title]    │
│  ────────────────   │  ─────────────────────────────────        │
│                     │  ┌───────────────────────────────────────┐│
│  Reorder Card ID:   │  │ Request │ Material │ Material │ Qty  ││
│  [Auto-RC011]       │  │ ID      │ ID       │ Name     │      ││
│                     │  ├───────────────────────────────────────┤│
│  Raw Material ID:   │  │ RC011   │ RM004    │ Ergonomic│ 100  ││
│  [RM004]            │  │ RC010   │ RM010    │ Large Co│ 5    ││
│                     │  │ RC009   │ RM009    │ Wardrobe│ 12   ││
│  Requested Qty:     │  │ ...     │ ...      │ ...     │ ...  ││
│  [100]              │  └───────────────────────────────────────┘│
│                     │                                           │
│  [Dispatch Request] [Clear Form]                               │
└─────────────────────┴───────────────────────────────────────────┘
```

## 🎯 UI Elements (English)

### Form Labels
| Label | Purpose |
|-------|---------|
| Material Replenishment | Card title |
| Reorder Card ID (Auto): | Auto-generated request ID |
| Raw Material ID *: | Material code input |
| Requested Quantity *: | Quantity required |

### Buttons
| Button | Action | Status |
|--------|--------|--------|
| 📤 Dispatch Request | Submit material request | Enabled/Disabled based on role |
| 🔄 Clear Form | Clear all input fields | Always enabled |

### Grid Headers
| Column | Shows |
|--------|-------|
| Request ID | Reorder card ID (e.g., RC011) |
| Material ID | Raw material code (e.g., RM004) |
| Material Name | Name of the material |
| Qty | Requested quantity |
| Status | Request status (Pending, Approved, etc.) |
| Date | Request date and time |

## 🔐 Permission-Based UI Behavior

### Role: Manager / Administrator
✅ **Status:** Full Access
```
[✓] Can view form
[✓] Can view requests
[✓] Can submit requests
[✓] Dispatch button: Enabled (Green)
```

### Role: Procurement Officer
⚠️ **Status:** Partial Access
```
[✓] Can view form (if accessed directly)
[✓] Can view requests
[✓] Can submit requests
[✓] Dispatch button: Enabled (Green)
[!] Menu item not visible (must access directly)
```

### Role: Warehouse Specialist
⚠️ **Status:** View Only
```
[✓] Can view form
[✓] Can view requests
[✗] Cannot submit requests
[!] Dispatch button: Disabled (Gray)
[!] Shows: "Access Level Notice"
```

### Role: Sales Representative
❌ **Status:** No Access
```
[✗] Cannot view form
[✗] Cannot view requests
[✗] Cannot submit requests
[!] Shows: "Access Denied" alert
```

## 💬 User Messages (English)

### Success Message
**When:** Request submitted successfully
```
✅ Request Submitted

Raw Material replenishment request successfully dispatched!
```

### Access Level Notice
**When:** User has view access but not submit permission
```
ℹ️ Access Level Notice

You can view existing material requests, but cannot submit new requests.

Current Role: Warehouse Specialist
Submit Permission Required: Manager, Administrator, or Procurement Officer
```

### Security Alert
**When:** User tries to submit without permission
```
🛑 System Security Enforcer

[SECURITY ALERT] Access Denied!

Only Procurement Officers and Management can submit material replenishment requests.

Current Role: Warehouse Specialist
```

### Validation Messages
| Message | When |
|---------|------|
| "Please fill in Raw Material ID and Quantity." | Empty required fields |
| "Quantity must be a valid positive integer." | Invalid quantity format |
| "Raw Material ID does not exist in the master inventory database." | Material not found |
| "Failed to load requests data:" | Database connection error |
| "Submission failed. Please verify the database connection." | Submit failed |

## 📊 Data Display

### Reorder Card ID Format
- **Auto-generated:** RC + sequential number
- **Example:** RC001, RC002, RC011, etc.

### Date Format
- **Display Format:** YYYY-MM-DD HH:MM
- **Example:** 2026-06-10 13:11

### Status Options
- `Pending` - Awaiting approval
- `Ordered` - Order placed with supplier
- `Approved` - Request approved
- `Completed` - Request completed
- `Cancelled` - Request cancelled

## ⚙️ User Actions

### To Submit a Material Request
1. Enter `Raw Material ID` (e.g., RM004)
2. Enter `Requested Quantity` (e.g., 100)
3. Click `📤 Dispatch Request` button
4. ✅ Confirmation message appears

### To View Requests
1. Scroll through the grid on the right
2. Click a row to select and view details
3. Details populate in the left card

### To Clear Form
1. Click `🔄 Clear Form` button
2. All inputs cleared
3. New Reorder Card ID generated

## 🔄 Permission Check Flow

```
User Opens Form
	↓
Check Menu Access Permission?
	├─ No → "Access Denied" → Close Form ❌
	└─ Yes → Continue
		  ↓
		Check Submit Permission?
			├─ No → Show "Access Level Notice" → Disable Dispatch Button ⚠️
			└─ Yes → Enable all features ✅
				  ↓
				Form Displays Fully Enabled
```

## 🛠️ Troubleshooting (English Messages)

| Issue | English Message | Solution |
|-------|-----------------|----------|
| Cannot see menu | No menu item | Check role permissions |
| Cannot submit | Dispatch button grayed out | Check role (requires Manager/Admin/Procurement) |
| Database error | "Failed to load requests data:" | Check MySQL connection |
| Validation error | "Please fill in Raw Material ID and Quantity." | Complete all required fields |

## 📱 Response Codes

| Code | Meaning | Action |
|------|---------|--------|
| 0 | Success | Request submitted |
| 1 | Invalid Material | Check Material ID exists |
| 2 | Invalid Quantity | Enter positive number |
| 3 | No Permission | Check user role |
| 4 | DB Error | Check connection |

## 🎨 Color Coding

### Buttons
| Color | Status |
|-------|--------|
| 🟢 Green (#10B981) | Enabled, ready to submit |
| 🔘 Gray (#9CA3AF) | Disabled, no permission |

### Messages
| Icon | Meaning |
|------|---------|
| ✅ | Success |
| ⚠️ | Warning/Notice |
| 🛑 | Security Alert |
| ❌ | Error |
| ℹ️ | Information |

---
**Version:** 1.0 (English UI)
**Last Updated:** 2024
**Language:** English (US)
