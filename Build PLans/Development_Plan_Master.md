# IronCore Gym SaaS — Master Development Plan
**Last Updated:** May 11, 2026

---

## STACK OVERVIEW

| Layer | Technology |
|-------|-----------|
| Frontend | React 18 + TypeScript, Vite, React Router DOM (lazy routes) |
| State | Zustand (auth, theme) + TanStack React Query (server state) |
| HTTP | Axios — Bearer token + X-Tenant-Id interceptors |
| Forms | React Hook Form + Zod |
| UI | Custom component library, Framer Motion, Tailwind CSS, CSS design tokens |
| Toast | Sonner |
| Charts | Recharts |
| Backend | .NET 9 / ASP.NET Core Web API |
| ORM | Entity Framework Core 9 + PostgreSQL (Npgsql) |
| Auth | JWT Bearer (HMACSHA256, 8h expiry) |
| Email | Brevo transactional email API |
| Uploads | Cloudinary (CloudName: djjjwupdi) |
| Pattern | Clean Architecture: Domain → Application → Infrastructure/Persistence → API |
| Multi-tenancy | Shared DB, row-level isolation via EF global query filters |

**Key Ports:**
- Frontend dev: http://localhost:5173
- Backend API: https://localhost:44304/api
- Database: localhost:5432 / GymSaaSDb / postgres

---

## INFRASTRUCTURE & CROSS-CUTTING CONCERNS

### Multi-Tenancy
- Every entity inherits `BaseTenantEntity` (Guid Id, Guid TenantId, DateTime CreatedAt, DateTime? UpdatedAt)
- EF Core global query filters on ALL entities (`WHERE TenantId = current`)
- `TenantResolverMiddleware` extracts TenantId from JWT claim → scoped `ITenantProvider`
- `DbContext.SaveChangesAsync` auto-stamps TenantId, CreatedAt, UpdatedAt
- Frontend injects `X-Tenant-Id` header on every Axios request

### Middleware Pipeline (in order)
1. `GlobalExceptionMiddleware` — unhandled exceptions → 500 ApiResponse
2. `RequestLoggingMiddleware` — method, path, status, duration
3. CORS — localhost:5173, localhost:3000
4. JWT Authentication + Role Authorization
5. `TenantResolverMiddleware`

### API Response Pattern
All endpoints return `ApiResponse<T>` with `Success`, `Message`, `Data?`
Static factories: `ApiResponse.Ok(data)`, `ApiResponse.Fail(message)`

### Repository Pattern
Generic `IRepository<T>` + `GenericRepository<T>` used across all entities.
DI registered per entity type in `Program.cs`.

### JWT Tokens
- Claims: UserId, TenantId, Role, Email
- Expiry: 8 hours | Issuer: "GymSaaS" | Audience: "GymSaaSClients"
- Algorithm: HMACSHA256
- ⚠️ Secret in appsettings.json (move to env var for production)

### Frontend Auth
- `useAuthStore` (Zustand, persisted to localStorage `gym-app-auth`)
- Stores: user, token, isAuthenticated
- Methods: login(), logout(), updateUser(), hasRole(), tenantId()
- `AuthGuard` → redirects to /login if not authenticated
- `SetupGuard` → Owner + !setupCompleted → /setup
- Response interceptor: 401 → clear localStorage + redirect /login

---

## ROUTE STRUCTURE

```
/                          LandingPage (public)
/login                     LoginPage
/register                  RegisterPage
/forgot-password           ForgotPasswordPage
/reset-password            ResetPasswordPage

AuthGuard:
  /setup                   SetupWizardPage (Owner + !setupCompleted only)

  SetupGuard → AppLayout:
    /dashboard             DashboardPage
    /members               MembersListPage
    /memberships           MembershipsPage
    /payments              PaymentsPage
    /attendance            AttendancePage
    /reports               ReportsPage
    /services              ServicesHubPage
    /services/classes      ClassesPage
    /services/personal-trainers  PersonalTrainersServicePage
    /trainers              TrainersPage
    /settings              SettingsPage
    /admin/gyms            SuperAdmin only
```

---

## FEATURES BUILT

---

### 1. GYM REGISTRATION / TENANT ONBOARDING ✅

**Backend:** `POST /api/tenants`
- Creates `Tenant` record (GymName, SubDomain auto-generated as `gymname-{4hex}`, OwnerName, Email, Phone, IsActive)
- Creates `User` record for Owner (BCrypt password hash, Role = Owner)

**Frontend:** `/register`
- Fields: GymName, OwnerName, Email, Password, Phone, Country, Timezone
- On success → redirect to `/login`

---

### 2. AUTHENTICATION ✅

**Backend:** `POST /api/auth/login`
- Finds User by email (bypasses tenant filter)
- Verifies password, issues JWT with UserId, TenantId, Role, Email
- ⚠️ Password verification skipped in dev mode

**Frontend:** `/login`
- React Hook Form + Zod validation
- Persists token + user to `useAuthStore` (localStorage)

---

### 3. FORGOT PASSWORD FLOW ✅

**Backend:**
- `POST /api/auth/forgot-password` — generates 6-digit OTP, stores `PasswordResetOtp` entity (Email, OTP hash, ExpiresAt 15min, IsUsed), sends email via Brevo API
- `POST /api/auth/verify-otp` — validates OTP, returns short-lived reset token
- `POST /api/auth/reset-password` — validates reset token, updates User.PasswordHash, marks OTP used

**Frontend:** `/forgot-password` + `/reset-password`
- Multi-step form: enter email → enter OTP → enter new password

---

### 4. SETUP WIZARD ✅

**Route:** `/setup` — 4-step wizard (Owner only, blocked by SetupGuard)

- Step 1 — Gym Profile: Name, address, logo upload (Cloudinary)
- Step 2 — Membership Plans: Create initial packages
- Step 3 — Add Staff: Invite manager/receptionist accounts
- Step 4 — Payment Settings: Configure payment defaults
- On completion → setupCompleted = true → redirect to /dashboard

---

### 5. DASHBOARD ✅

**KPI Cards:**
- Total Athletes (active member count)
- Tactical Checks (today's attendance count)
- Core Streak (attendance streak metric)
- Capital Revenue (total payments this month)

**Visualizations:**
- Attendance heatmap (calendar grid, check-in intensity)
- Recharts AreaChart (revenue/attendance over time)

---

### 6. MEMBERS MODULE ✅

**Backend:** `MemberController` + `MemberService`

| Endpoint | Auth |
|----------|------|
| GET /api/member | [Authorize] |
| GET /api/member/{id} | [Authorize] |
| POST /api/member | Owner, Manager, Receptionist |
| PUT /api/member/{id} | Owner, Manager, Receptionist |
| DELETE /api/member/{id} | Owner, Manager |

**Entity fields:** FirstName, LastName, Phone, Email?, Gender, DateOfBirth, JoinDate, Status, MembershipNumber (unique per tenant), Address?, EmergencyContact?, Height?, Weight?, MedicalConditions?, Photo (Cloudinary URL)?, TrainerId? (FK), BranchId? (FK)

**Delete cascades:** Memberships, Payments, Attendances deleted; TrainerId set to null

**Frontend:** `/members`
- CRUD table with search/filter
- `MemberFormDialog` — creates Member + Membership in a single UX flow
- Enrichment: fetches latest membership per member, merges into table row
- Photo display, status badges (Active / Inactive / Suspended)

---

### 7. MEMBERSHIPS & PACKAGES ✅

**Backend — Packages:** `PackageController` + `PackageService`
- GET /api/package, POST, PUT, DELETE (Owner only delete — restricted if active memberships exist)
- Fields: Name, Duration ("X Months"), Price, Branch, Status, MaxVisits?, TrainerIncluded, FreezeDays?, DiscountAllowed

**Backend — Memberships:** `MembershipController` + `MembershipService`
- `GET /api/membership/member/{memberId}` — enrollment history
- `POST /api/membership` — create enrollment (StartDate, EndDate, Price, Discount, PaymentStatus)
- `PUT /api/membership/{id}` — update enrollment
- `POST /api/membership/renew` — extends EndDate, sets PaymentStatus = "Paid", creates Payment record
- `GET /api/membership/payments/{memberId}` — full payment history

**Payment Types (seeded master table):**
- 1 = Registration Fee
- 2 = First Month Payment
- 3 = Monthly Payment (default)

**Frontend:** `/memberships`
- Package catalog CRUD
- Duration mapping: `durationInMonths` number ↔ "X Months" string for API
- Membership renewal dialog (amount, discount, new end date)

---

### 8. TRAINERS MODULE ✅

**Backend:** `TrainerController` + `TrainerService`
- Full CRUD: GET, POST [Owner,Manager], PUT [Owner,Manager], DELETE [Owner,Manager]
- Fields: FirstName, LastName, Phone, Email, Specialization, Status, BranchId?, TrainerTypeId?, DateOfBirth?, Photo?, Certifications (comma-separated), ExperienceYears?, Availability ("Mon,Wed,Fri 09:00-17:00")

**Frontend:** `/trainers`
- `TrainersTable` — filterable table with status badges
- `TrainerFormDialog` — Zod validation, availability day-toggles + time range, certification tag management
- `TrainerDetailsDialog` — full profile view
- `TrainerScheduleDialog` — UI built, backend not yet connected
- Certifications: string[] ↔ comma-separated string at API boundary

---

### 9. BRANCHES ✅

**Backend:** `BranchController` + `BranchService`
- Full CRUD: GET, POST [Owner,Manager], PUT [Owner,Manager], DELETE [Owner,Manager]
- Fields: Name, Address, Phone, IsActive

**Frontend:** Managed inside Settings (Gym Profile tab)
- `useBranches()` React Query hook provides branch data app-wide (used in Member/Trainer/Class forms)

---

### 10. PAYMENTS MODULE ✅

**Backend:** `PaymentsController`
- Full payment history with member details
- `PaymentSchedule` table — installment tracking per membership
- `PaymentType` enforcement (Registration Fee / First Month / Monthly)

**Frontend:** `/payments`
- Payments list with filtering by status, date, member
- Payment receipt/details view
- Linked to membership payment schedules

---

### 11. ATTENDANCE ✅

**Backend:** `Attendance` entity (MemberId, BranchId, CheckInTime, CheckOutTime?)

**Frontend:** `/attendance`
- Check-in / check-out tracking UI

---

### 12. REPORTS ✅

**Frontend:** `/reports`
- Revenue reports
- Member growth charts
- Attendance analytics

---

### 13. SERVICES MODULE ✅

Shared infrastructure: `ServicePaymentSchedule` + `ServicePayment` entities handle billing for all service types.

#### 13a. Classes (Group Classes)

**Backend:** `GymClassController` + `ClassTypeController` + `ClassTimeSlotController`
- `GymClass`: Name, Category, Description, InstructorId (FK→Trainer), BranchId (FK), ClassTypeId (FK), BatchStartDate?, BatchEndDate?, MaxCapacity, DefaultAmount, HourlyRate, Status
- `ClassType`: Name, Description, IsActive (e.g. Yoga, Zumba, Boxing)
- `ClassSchedule`: GymClassId, BranchId, DayOfWeek, StartTime, EndTime

**Frontend:** `/services/classes`
- Class listing with schedules
- Enrollment management
- Payment tracking per class

#### 13b. Personal Training (PT) Sessions

**Backend:** `PtRegistrationController`
- `PtRegistration`: TrainerId, TrainerName, StudentCount, PaymentRatePerStudent, Status
- Linked to `ServicePaymentSchedule` for billing

**Frontend:** `/services/personal-trainers`
- PT registration management
- Per-session payment tracking

#### 13c. Services Hub

**Frontend:** `/services`
- Overview of all active services
- Navigation to Classes and PT pages

**Service Settings:** `ServiceSettingController`
- `ServiceSetting`: ServiceType (enum), DefaultAmount, Notes

---

### 14. SETTINGS MODULE ✅

**Route:** `/settings` — sidebar tab navigation

| Tab | Component | What it does |
|-----|-----------|-------------|
| Gym Profile | `GymProfileSettings` | Edit gym name, address, phone, logo; Branches CRUD |
| Users & Roles | `UsersRolesSettings` | Staff accounts + RBAC permissions |
| Membership Rules | `MembershipRulesSettings` | Auto-renewal, freeze, discount policies |
| Payments | `PaymentSettings` | Payment method config, late fee settings |
| Working Hours | `WorkingHoursSettings` | Per-day open/close times, closed flag |
| Trainer Settings | `TrainerTypeSettings` | TrainerType CRUD (Name, Description, IsActive) |
| Service Settings | `ServiceSettings` | Default amounts per service type |
| Expenses | `ExpensesSettings` | Fixed + Variable expense tracking |

#### Users & Roles Detail
- `UserController` — Create staff (Manager, Receptionist), set temporary passwords, deactivate
- `User` entity: IsTemporaryPassword flag, CustomRole field
- `AppRole` + `RolePermission` entities — RBAC
- RolePermission unique index: (TenantId, RoleName, PermissionKey)
- `UiPermissionKeys` static class defines all permission key strings

#### Working Hours
- `WorkingHours`: Day (Mon–Sun), OpenTime, CloseTime, IsClosed per tenant
- `WorkingHoursController`: GET / POST / PUT /api/workinghours

#### Trainer Types
- `TrainerType`: Name, Description, IsActive
- `TrainerTypeController`: Full CRUD
- Frontend: inline-editable table with Add / Edit / Delete

#### Expenses ← Most recently built (Apr 26, 2026)

**Fixed Expenses** (recurring monthly costs):
- `FixedExpense`: Name, Amount, Description, IsActive
- `GET /api/expense/fixed`, POST, PUT [Owner,Manager], DELETE [Owner,Manager]
- Frontend: inline-editable table; monthly total of active expenses shown

**Variable Expenses** (one-off monthly costs):
- `VariableExpense`: Name, Amount, Description, Month, Year
- `GET /api/expense/variable?month={m}&year={y}`, POST, PUT, DELETE
- Frontend: month/year navigator (← → arrows) to browse periods; inline-editable table per period

---

### 15. PHOTO UPLOADS ✅

**Backend:** `UploadController` — `POST /api/upload`
- Accepts multipart file, uploads to Cloudinary, returns secure URL

Used by: Member photos, Trainer photos, Gym logo

---

## DATABASE MIGRATIONS (chronological)

| # | Date | Migration Name | Added |
|---|------|---------------|-------|
| 1 | Mar 20 | InitialCreate | Tenant, User, Member, Trainer, Membership, Package, Payment, Attendance, Role, AuditLog, TrainerAssignment |
| 2 | Mar 26 | AddBranchEntity | Branch table; BranchId FK on Member & Trainer |
| 3 | Mar 28 | AddLogoUrlToTenant | LogoUrl on Tenant |
| 4 | Mar 28 | AddWorkingHoursTable | WorkingHours table |
| 5 | Mar 29 | AddPaymentScheduleAndEnhancePayment | PaymentSchedule table; PaymentTypeId FK on Payment |
| 6 | Mar 29 | AddPaymentTypeMasterTable | PaymentType seeded x3 |
| 7 | Mar 29 | AddMemberDeletionLog | Deletion audit table |
| 8 | Mar 29 | AddBillingFrequencyToPackage | BillingFrequency on Package |
| 9 | Apr 3 | AddMemberTypeAndPaymentTypes | MemberType enum; payment type seeds |
| 10 | Apr 4 | AddTrainerAge | Age field on Trainer |
| 11 | Apr 4 | AddTrainerTypeEntity | TrainerType table; TrainerTypeId FK on Trainer |
| 12 | Apr 4 | AddServiceSettings | ServiceSetting table |
| 13 | Apr 4 | AddClassTypes | ClassType + GymClass + ClassSchedule tables |
| 14 | Apr 5 | AddMembershipNumber | MembershipNumber (unique per tenant) on Member |
| 15 | Apr 6 | AddHourlyRateAndClassTimeSlots | HourlyRate on GymClass; ClassSchedule table |
| 16 | Apr 7 | MakeClassDatesOptional | BatchStartDate/EndDate nullable on GymClass |
| 17 | Apr 7 | SeparateClassSchedules | ClassSchedules split into own table |
| 18 | Apr 9 | AddPtRegistrations | PtRegistration table |
| 19 | Apr 12 | AddUsersCustomRoleAndRBAC | CustomRole on User; AppRole + RolePermission tables |
| 20 | Apr 14 | AddUserProfileFields | DateOfBirth, Gender, MobileNumber on User |
| 21 | Apr 14 | AddIsTemporaryPassword | IsTemporaryPassword flag on User |
| 22 | Apr 15 | AddServicePayments | ServicePaymentSchedule + ServicePayment tables |
| 23 | Apr 25 | AddPasswordResetOtp | PasswordResetOtp table (email, OTP hash, expiry, isUsed) |
| 24 | Apr 26 | AddExpenseEntities | FixedExpense + VariableExpense tables |

---

## KNOWN ISSUES / TECH DEBT

| # | Issue | Severity |
|---|-------|---------|
| 1 | Password verification SKIPPED in AuthService login (dev shortcut) | High |
| 2 | JWT secret stored in appsettings.json — must move to env var | High |
| 3 | AuthService uses SHA256; TenantService uses BCrypt — inconsistency | Medium |
| 4 | TrainerScheduleDialog UI complete but backend Schedule entity not implemented | Medium |
| 5 | No Unit of Work pattern for complex multi-repo transactions | Medium |
| 6 | Photo upload via Cloudinary — endpoint exists but UI integration incomplete on trainers | Low |
| 7 | No server-side validation service layer (only DTO annotations) | Low |

---

## FEATURE COMPLETION STATUS

| Module | Frontend | Backend | Notes |
|--------|----------|---------|-------|
| Auth / Registration | ✅ | ✅ | |
| Forgot Password | ✅ | ✅ | |
| Setup Wizard | ✅ | Partial | Stats endpoint TBD |
| Dashboard | ✅ | Partial | |
| Members | ✅ | ✅ | |
| Memberships / Packages | ✅ | ✅ | |
| Trainers | ✅ | ✅ | Schedule backend missing |
| Branches | ✅ | ✅ | |
| Payments | ✅ | ✅ | |
| Attendance | ✅ | ✅ | Entity ready |
| Reports | ✅ | Partial | |
| Classes (Group) | ✅ | ✅ | |
| Personal Training | ✅ | ✅ | |
| Services Hub | ✅ | ✅ | |
| Settings — Gym Profile | ✅ | ✅ | |
| Settings — Users & Roles | ✅ | ✅ | |
| Settings — Working Hours | ✅ | ✅ | |
| Settings — Trainer Types | ✅ | ✅ | |
| Settings — Service Settings | ✅ | ✅ | |
| Settings — Expenses | ✅ | ✅ | |
| Photo Uploads | Partial | ✅ | Trainer photo UI incomplete |
