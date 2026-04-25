using BCrypt.Net;
using GymSaaS.Application.DTOs.Users;
using GymSaaS.Application.Interfaces;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Enums;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymSaaS.Application.Services;

public class UserService
{
    private readonly IRepository<User> _repo;
    private readonly ITenantProvider _tenantProvider;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserService> _logger;

    public UserService(IRepository<User> repo, ITenantProvider tenantProvider, IEmailService emailService, ILogger<UserService> logger)
    {
        _repo           = repo;
        _tenantProvider = tenantProvider;
        _emailService   = emailService;
        _logger         = logger;
    }

    // ── Helpers ───────────────────────────────────────────────

    /// <summary>Generates a random 10-character alphanumeric one-time password.</summary>
    private static string GenerateOtp()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        var bytes = new byte[10];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    }

    // ── Queries ───────────────────────────────────────────────

    public async Task<ApiResponse<IEnumerable<UserResponseDto>>> GetAllAsync()
    {
        var users = await _repo.AsQueryable()
            .OrderBy(u => u.FirstName)
            .ToListAsync();
        return ApiResponse<IEnumerable<UserResponseDto>>.Ok(users.Select(MapToDto));
    }

    public async Task<ApiResponse<UserResponseDto>> GetByIdAsync(Guid id)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) return ApiResponse<UserResponseDto>.Fail("User not found.");
        return ApiResponse<UserResponseDto>.Ok(MapToDto(user));
    }

    // ── Commands ──────────────────────────────────────────────

    public async Task<ApiResponse<CreateUserResponseDto>> CreateAsync(CreateUserDto dto)
    {
        var exists = await _repo.AsQueryable().AnyAsync(u => u.Email == dto.Email);
        if (exists) return ApiResponse<CreateUserResponseDto>.Fail("A user with that email already exists.");

        var otp = GenerateOtp();

        var user = new User
        {
            TenantId     = _tenantProvider.TenantId,
            FirstName    = dto.FirstName,
            LastName     = dto.LastName,
            MobileNumber = dto.MobileNumber,
            Email        = dto.Email,
            DateOfBirth  = dto.DateOfBirth,
            Gender       = dto.Gender,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(otp),
            // Custom roles from AppRoles are stored in CustomRole; system Role is always Receptionist
            // unless the role string matches a system enum value directly
            Role                = UserRole.Receptionist,
            CustomRole          = string.IsNullOrWhiteSpace(dto.CustomRole) ? null : dto.CustomRole,
            IsActive            = true,
            IsTemporaryPassword = true,
        };

        await _repo.AddAsync(user);
        await _repo.SaveChangesAsync();

        // Send one-time password to the user and record result in response
        bool emailSent = false;
        string? emailError = null;
        try
        {
            await _emailService.SendCredentialsAsync(
                user.Email,
                $"{user.FirstName} {user.LastName}",
                user.Email,
                otp
            );
            emailSent = true;
        }
        catch (Exception ex)
        {
            emailError = ex.Message;
            _logger.LogWarning(ex, "Failed to send credentials email to {Email}. User was still created.", user.Email);
        }

        var responseDto = MapToCreateDto(user, emailSent, emailError);
        return ApiResponse<CreateUserResponseDto>.Ok(responseDto);
    }

    public async Task<ApiResponse<UserResponseDto>> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) return ApiResponse<UserResponseDto>.Fail("User not found.");

        user.FirstName    = dto.FirstName;
        user.LastName     = dto.LastName;
        user.MobileNumber = dto.MobileNumber;
        user.Email        = dto.Email;
        user.DateOfBirth  = dto.DateOfBirth;
        user.Gender       = dto.Gender;
        user.CustomRole   = string.IsNullOrWhiteSpace(dto.CustomRole) ? null : dto.CustomRole;
        user.IsActive     = dto.IsActive;
        user.UpdatedAt    = DateTime.UtcNow;

        _repo.Update(user);
        await _repo.SaveChangesAsync();
        return ApiResponse<UserResponseDto>.Ok(MapToDto(user));
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(Guid id, string newPassword)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) return ApiResponse<bool>.Fail("User not found.");

        user.PasswordHash       = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.IsTemporaryPassword = false;
        user.UpdatedAt           = DateTime.UtcNow;

        _repo.Update(user);
        await _repo.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) return ApiResponse<bool>.Fail("User not found.");
        if (user.Role == UserRole.Owner) return ApiResponse<bool>.Fail("Cannot delete the Owner account.");

        _repo.Delete(user);
        await _repo.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true);
    }

    // ── Mapping ───────────────────────────────────────────────

    private static UserResponseDto MapToDto(User u) => new()
    {
        Id           = u.Id,
        FirstName    = u.FirstName,
        LastName     = u.LastName,
        MobileNumber = u.MobileNumber ?? string.Empty,
        Email        = u.Email,
        DateOfBirth  = u.DateOfBirth,
        Gender       = u.Gender,
        Role         = u.Role.ToString(),
        CustomRole   = u.CustomRole,
        IsActive            = u.IsActive,
        IsTemporaryPassword = u.IsTemporaryPassword,
        CreatedAt           = u.CreatedAt,
    };

    private static CreateUserResponseDto MapToCreateDto(User u, bool emailSent, string? emailError) => new()
    {
        Id           = u.Id,
        FirstName    = u.FirstName,
        LastName     = u.LastName,
        MobileNumber = u.MobileNumber ?? string.Empty,
        Email        = u.Email,
        DateOfBirth  = u.DateOfBirth,
        Gender       = u.Gender,
        Role         = u.Role.ToString(),
        CustomRole   = u.CustomRole,
        IsActive            = u.IsActive,
        IsTemporaryPassword = u.IsTemporaryPassword,
        CreatedAt           = u.CreatedAt,
        EmailSent           = emailSent,
        EmailError          = emailError,
    };
}

