# Expenses Tracking — Build Plan

## Overview

Add an **Expenses Settings** tab to the Settings page supporting two expense types:
- **Fixed Expenses** — recurring, defined once, automatically apply every month's revenue
- **Variable Expenses** — one-off, entered for a specific month/year only

Both are managed entirely from the Expenses Settings tab. Fixed expenses can be created, edited, and deleted at any time.

---

## Phase 1 — Backend: Domain Entities

### 1a. `FixedExpense` entity
**File:** `GymSaaS.Domain/Entities/FixedExpense.cs`

```csharp
public class FixedExpense : BaseTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### 1b. `VariableExpense` entity
**File:** `GymSaaS.Domain/Entities/VariableExpense.cs`

```csharp
public class VariableExpense : BaseTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public int Month { get; set; }   // 1–12
    public int Year { get; set; }    // e.g. 2026
}
```

---

## Phase 2 — Backend: DbContext + Migration

### 2a. Register in `GymDbContext.cs`
Add DbSets after existing sets:
```csharp
public DbSet<FixedExpense> FixedExpenses { get; set; }
public DbSet<VariableExpense> VariableExpenses { get; set; }
```

Add global query filters in `OnModelCreating`:
```csharp
modelBuilder.Entity<FixedExpense>()
    .HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);
modelBuilder.Entity<VariableExpense>()
    .HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);
```

### 2b. Migration
```bash
dotnet ef migrations add AddExpenseEntities
dotnet ef database update
```

---

## Phase 3 — Backend: DTOs

**File:** `GymSaaS.Application/DTOs/Expenses/ExpenseDtos.cs`

```csharp
// Fixed Expense
public class CreateFixedExpenseDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}

public class UpdateFixedExpenseDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class FixedExpenseResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// Variable Expense
public class CreateVariableExpenseDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}

public class UpdateVariableExpenseDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}

public class VariableExpenseResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## Phase 4 — Backend: Service Interface + Implementation

### 4a. Interface
**File:** `GymSaaS.Application/Interfaces/IExpenseService.cs`

```csharp
public interface IExpenseService
{
    // Fixed
    Task<ApiResponse<IEnumerable<FixedExpenseResponseDto>>> GetFixedExpensesAsync();
    Task<ApiResponse<FixedExpenseResponseDto>> CreateFixedExpenseAsync(CreateFixedExpenseDto dto);
    Task<ApiResponse<FixedExpenseResponseDto>> UpdateFixedExpenseAsync(Guid id, UpdateFixedExpenseDto dto);
    Task<ApiResponse<string>> DeleteFixedExpenseAsync(Guid id);

    // Variable
    Task<ApiResponse<IEnumerable<VariableExpenseResponseDto>>> GetVariableExpensesAsync(int month, int year);
    Task<ApiResponse<VariableExpenseResponseDto>> CreateVariableExpenseAsync(CreateVariableExpenseDto dto);
    Task<ApiResponse<VariableExpenseResponseDto>> UpdateVariableExpenseAsync(Guid id, UpdateVariableExpenseDto dto);
    Task<ApiResponse<string>> DeleteVariableExpenseAsync(Guid id);
}
```

### 4b. Implementation
**File:** `GymSaaS.Application/Services/ExpenseService.cs`

Inject `GymDbContext` + `ITenantProvider` (following the same pattern as `ServiceSettingService`).

**Fixed expense logic:**
- `GetFixedExpensesAsync` → all fixed expenses, ordered by `CreatedAt`
- `CreateFixedExpenseAsync` → validate Name not empty, Amount ≥ 0; save; return DTO
- `UpdateFixedExpenseAsync` → find by id, update Name/Amount/Description/IsActive; save; return DTO
- `DeleteFixedExpenseAsync` → find by id; remove; save; return Ok

**Variable expense logic:**
- `GetVariableExpensesAsync(month, year)` → filter by Month+Year, ordered by `CreatedAt`
- `CreateVariableExpenseAsync` → validate Month 1–12, Year ≥ 2000, Name not empty; save; return DTO
- `UpdateVariableExpenseAsync` → find by id; update Name/Amount/Description; save; return DTO
- `DeleteVariableExpenseAsync` → find by id; remove; save; return Ok

### 4c. Register in DI
**File:** `GymSaaS.API/Program.cs`

```csharp
builder.Services.AddScoped<IExpenseService, ExpenseService>();
```

---

## Phase 5 — Backend: Controller

**File:** `GymSaaS.API/Controllers/ExpenseController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpenseController : ControllerBase
{
    // Fixed
    GET    /api/expense/fixed                          → GetFixedExpenses
    POST   /api/expense/fixed                          → CreateFixedExpense        [Owner, Manager]
    PUT    /api/expense/fixed/{id}                     → UpdateFixedExpense        [Owner, Manager]
    DELETE /api/expense/fixed/{id}                     → DeleteFixedExpense        [Owner, Manager]

    // Variable
    GET    /api/expense/variable?month={m}&year={y}    → GetVariableExpenses
    POST   /api/expense/variable                       → CreateVariableExpense     [Owner, Manager]
    PUT    /api/expense/variable/{id}                  → UpdateVariableExpense     [Owner, Manager]
    DELETE /api/expense/variable/{id}                  → DeleteVariableExpense     [Owner, Manager]
}
```

All endpoints return `IActionResult` wrapping `ApiResponse<T>`. On failure return `BadRequest(result)`, on success return `Ok(result)`.

---

## Phase 6 — Frontend: API Layer

**File:** `src/features/settings/api/expenses-api.ts`

```ts
export type ExpenseType = 'fixed' | 'variable';

export interface FixedExpense { id, name, amount, description?, isActive, createdAt, updatedAt? }
export interface VariableExpense { id, name, amount, description?, month, year, createdAt }

export interface CreateFixedExpenseDto { name, amount, description? }
export interface UpdateFixedExpenseDto { name, amount, description?, isActive }
export interface CreateVariableExpenseDto { name, amount, description?, month, year }
export interface UpdateVariableExpenseDto { name, amount, description? }

export const expensesApi = {
    // Fixed
    getFixed: () => api.get<{ data: FixedExpense[] }>('/expense/fixed'),
    createFixed: (data: CreateFixedExpenseDto) => api.post('/expense/fixed', data),
    updateFixed: (id: string, data: UpdateFixedExpenseDto) => api.put(`/expense/fixed/${id}`, data),
    deleteFixed: (id: string) => api.delete(`/expense/fixed/${id}`),

    // Variable
    getVariable: (month: number, year: number) =>
        api.get<{ data: VariableExpense[] }>(`/expense/variable?month=${month}&year=${year}`),
    createVariable: (data: CreateVariableExpenseDto) => api.post('/expense/variable', data),
    updateVariable: (id: string, data: UpdateVariableExpenseDto) =>
        api.put(`/expense/variable/${id}`, data),
    deleteVariable: (id: string) => api.delete(`/expense/variable/${id}`),
};
```

---

## Phase 7 — Frontend: React Query Hooks

**File:** `src/hooks/useExpenses.ts`

```ts
// Fixed
useFixedExpenses()                         → queryKey: ['fixedExpenses']
useCreateFixedExpense()                    → invalidates ['fixedExpenses'], toast success/error
useUpdateFixedExpense()                    → invalidates ['fixedExpenses'], toast success/error
useDeleteFixedExpense()                    → invalidates ['fixedExpenses'], toast success/error

// Variable
useVariableExpenses(month: number, year: number) → queryKey: ['variableExpenses', month, year]
useCreateVariableExpense()                 → invalidates ['variableExpenses'], toast success/error
useUpdateVariableExpense()                 → invalidates ['variableExpenses'], toast success/error
useDeleteVariableExpense()                 → invalidates ['variableExpenses'], toast success/error
```

---

## Phase 8 — Frontend: ExpensesSettings Component

**File:** `src/features/settings/components/ExpensesSettings.tsx`

### Layout
Two sections stacked vertically, each in a `Card` with header, matching the pattern of `TrainerTypeSettings.tsx`:

---

### Section 1 — Fixed Expenses

**Header:** `TrendingUp` icon + "Fixed Expenses" + badge "Monthly" (green)
**Sub-text:** "These expenses are deducted from every month's revenue automatically."

**Behaviour:**
- Table/list of all fixed expenses:
  - Columns: Name | Amount (LKR x,xxx) | Description | Status (Active/Inactive badge) | Actions
  - Each row has **Edit** (inline edit, same row transforms to input) + **Delete** (confirm dialog) buttons
- **Inline Edit:** clicking Edit turns Name, Amount, Description, and IsActive toggle into editable fields in place; Save / Cancel buttons appear (matching pattern from `TrainerTypeSettings`)
- **Add Fixed Expense** button → `isAddingMode` state reveals a new row at top with: Name input, Amount input, Description input, Add / Cancel
- Validation: Name required, Amount ≥ 0
- Empty state: "No fixed expenses added yet."

---

### Section 2 — Variable Expenses

**Header:** `BarChart3` icon + "Variable Expenses" + badge "Month-specific" (orange)
**Sub-text:** "One-off expenses assigned to a specific month."

**Month/Year selector** (two `CustomSelect` dropdowns side by side):
- Month: Jan–Dec
- Year: current year and 2 years back/forward

**Behaviour:**
- Loads variable expenses for the selected month/year via `useVariableExpenses(month, year)`
- List shows: Name | Amount | Description | Delete button
- **No inline edit** for variable expenses — only create and delete (variable expenses are a record of what happened that month; if wrong, delete and re-add)

  > *Optional nicety: add an Edit button for variable expenses using the same inline pattern. Include `useUpdateVariableExpense` hook just in case.* 
  
- **Add Variable Expense** button → reveals inline form row: Name, Amount, Description (optional), then Add / Cancel
- Validation: Name required, Amount ≥ 0
- Empty state: "No variable expenses for {Month} {Year}."

---

## Phase 9 — Frontend: Wire into Settings Page

**File:** `src/features/settings/pages/settings-page.tsx`

### 9a. Import
```ts
import { ExpensesSettings } from '../components/ExpensesSettings';
```

### 9b. Add to TABS array (after `service-settings`):
```ts
{ id: 'expenses', label: 'Expenses', icon: TrendingUp },
```

### 9c. Add to TAB_CONTENT map:
```ts
'expenses': <ExpensesSettings />,
```

---

## Phase 10 — Build Verification

```bash
# Backend
cd "d:\Gym Application\GymSaaS.Backend"
dotnet build GymSaaS.sln --nologo

# Frontend
cd "d:\Gym Application\Gym App"
npx tsc --noEmit
```

---

## File Change Summary

| File | Action |
|:-----|:-------|
| `Domain/Entities/FixedExpense.cs` | New |
| `Domain/Entities/VariableExpense.cs` | New |
| `Persistence/GymDbContext.cs` | Add 2 DbSets + 2 query filters |
| `Persistence/Migrations/AddExpenseEntities` | New (generated) |
| `Application/DTOs/Expenses/ExpenseDtos.cs` | New |
| `Application/Interfaces/IExpenseService.cs` | New |
| `Application/Services/ExpenseService.cs` | New |
| `API/Controllers/ExpenseController.cs` | New |
| `API/Program.cs` | Register `IExpenseService` → `ExpenseService` |
| `src/features/settings/api/expenses-api.ts` | New |
| `src/hooks/useExpenses.ts` | New |
| `src/features/settings/components/ExpensesSettings.tsx` | New |
| `src/features/settings/pages/settings-page.tsx` | Add Expenses tab |

---

## Key Decisions

| Decision | Rationale |
|:---------|:----------|
| Two separate DB tables (not one table + type enum) | Clean separation; fixed and variable have different fields (Month/Year only on variable) |
| Variable expenses are per-month records, no IsActive | They are historical facts, not standing config; delete-and-recreate if wrong |
| Month/Year stored as int columns on VariableExpense | Simple to filter; avoids timezone ambiguity of DateTime |
| Inline editing for Fixed (not a modal) | Matches the TrainerTypeSettings pattern already in the codebase |
| `GET /expense/variable?month=&year=` query params | Keeps URL clean; server filters at DB level |
