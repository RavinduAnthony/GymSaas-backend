## **Forgot Password — Build Plan**

### **Phase 1 — Backend: OTP Entity + Migration**

**1a. New entity PasswordResetOtp**

- File: GymSaaS.Domain/Entities/PasswordResetOtp.cs

- Fields: Id (Guid), Email (string), Code (string), ExpiresAt (DateTime
  UTC), IsUsed (bool)

- Does **not** inherit BaseTenantEntity — OTP lookup is email-only, not
  tenant-scoped

**1b. Register in GymDbContext**

- Add DbSet\<PasswordResetOtp\> PasswordResetOtps

- No query filter

**1c. Migration AddPasswordResetOtp**

- dotnet ef migrations add AddPasswordResetOtp → dotnet ef database
  update

### **Phase 2 — Backend: DTOs**

**File: GymSaaS.Application/DTOs/Auth/ForgotPasswordDtos.cs**

ForgotPasswordRequestDto { Email, NewPassword, ConfirmPassword }\
ResetPasswordWithOtpDto { Email, Otp, NewPassword }\
VerifyOtpResponseDto { IsValid bool }

### **Phase 3 — Backend: Email — OTP Template**

**3a. Add to IEmailService:**

Task SendOtpAsync(string toEmail, string recipientName, string otp);

**3b. Implement in BrevoEmailService:**

- Sends styled 6-digit OTP email with "valid for 1 minute" warning

### **Phase 4 — Backend: Auth Service Methods**

**Add to IAuthService:**

Task\<ApiResponse\<string\>\>
RequestPasswordResetAsync(ForgotPasswordRequestDto dto);\
Task\<ApiResponse\<string\>\>
ResetPasswordWithOtpAsync(ResetPasswordWithOtpDto dto);

**Implement in AuthService** (inject IEmailService + GymDbContext):

- RequestPasswordResetAsync:

  - Find user by email (IgnoreQueryFilters)

  - If not found → return generic success (no enumeration)

  - Invalidate any existing unused OTPs for this email

  - Generate cryptographically random 6-digit code

  - Save PasswordResetOtp with ExpiresAt = UtcNow + 1 minute

  - Call emailService.SendOtpAsync(...)

  - Return success

- ResetPasswordWithOtpAsync:

  - Find latest unused OTP for Email where Code == dto.Otp

  - If not found → "Invalid OTP"

  - If ExpiresAt \< UtcNow → "OTP has expired"

  - Mark OTP as IsUsed = true

  - Find user by email; update PasswordHash

  - Return success

### **Phase 5 — Backend: Controller Endpoints**

**Add to AuthController:**

POST /api/auth/forgot-password → RequestPasswordResetAsync\
POST /api/auth/reset-password-otp → ResetPasswordWithOtpAsync

### **Phase 6 — Frontend: API Layer**

**File: src/features/auth/api/auth-api.ts** (new)

forgotPassword(email, newPassword, confirmPassword) // POST
/auth/forgot-password\
resetPasswordWithOtp(email, otp, newPassword) // POST
/auth/reset-password-otp

### **Phase 7 — Frontend: Forgot Password Page**

**File: src/features/auth/pages/forgot-password-page.tsx**

Two-step UI inside a single page (no route change between steps):

**Step 1 — "Request OTP"**

- Email input

- New Password input (with show/hide toggle)

- Confirm Password input (with show/hide toggle)

- Submit button → calls forgotPassword() → transitions to step 2

**Step 2 — "Enter OTP"**

- Display: "OTP sent to {email}"

- 6 individual digit input boxes (auto-focus advances, backspace
  retreats)

- Countdown timer 0:59 → 0:00 (live, 1-second interval)

- When timer hits 0: timer hides, **"Resend OTP"** button appears (calls
  forgotPassword() again, restarts timer)

- Submit button → calls resetPasswordWithOtp() → shows success toast →
  redirect to /login

### **Phase 8 — Frontend: Router + Login Link**

**Update router.tsx:**

- Add public route /forgot-password → ForgotPasswordPage

**Update login-page.tsx:**

- Change \<a href="#"\>Forgot password?\</a\> → \<Link
  to="/forgot-password"\>Forgot password?\</Link\>

### **Phase 9 — Build Verification**

\# Backend\
cd "d:\Gym Application\GymSaaS.Backend"\
dotnet build GymSaaS.sln --nologo\
\
\# Frontend\
cd "d:\Gym Application\Gym App"\
npx tsc --noEmit

### **File Change Summary**

| **File** | **Action** |
|:---|:---|
| Domain/Entities/PasswordResetOtp.cs | New |
| Persistence/GymDbContext.cs | Add DbSet |
| Persistence/Migrations/AddPasswordResetOtp | New (generated) |
| Application/DTOs/Auth/ForgotPasswordDtos.cs | New |
| Application/Interfaces/IEmailService.cs | Add SendOtpAsync |
| Infrastructure/Services/BrevoEmailService.cs | Implement SendOtpAsync |
| Application/Interfaces/IAuthService.cs | Add 2 methods |
| Application/Services/AuthService.cs | Implement 2 methods |
| API/Controllers/AuthController.cs | Add 2 endpoints |
| src/features/auth/api/auth-api.ts | New |
| src/features/auth/pages/forgot-password-page.tsx | New |
| src/app/router.tsx | Add /forgot-password route |
| src/features/auth/pages/login-page.tsx | Update link |
