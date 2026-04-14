using BCrypt.Net;
using GymSaaS.Application.DTOs.Users;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Enums;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Shared;
using Microsoft.EntityFrameworkCore;

namespace GymSaaS.Application.Services;

public class UserService
{
    private readonly IRepository<User> _repo;
    private readonly ITenantProvider _tenantProvider;

    public UserService(IRepository<User> repo, ITenantProvider tenantProvider)
    {
        _repo = repo;
        _tenantProvider = tenantProvider;
    }

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

    public async Task<ApiResponse<UserResponseDto>> CreateAsync(CreateUserDto dto)
    {
        // Prevent duplicate emails within the tenant
        var exists = await _repo.AsQueryable()
            .AnyAsync(u => u.Email == dto.Email);
        if (exists) return ApiResponse<UserResponseDto>.Fail("A user with that email already exists.");

        if (!Enum.TryParse<UserRole>(dto.Role, out var role))
            return ApiResponse<UserResponseDto>.Fail($"Invalid role '{dto.Role}'. Must be Owner, Manager, Receptionist, or Trainer.");

        var user = new User
        {
            TenantId    = _tenantProvider.TenantId,
            FirstName   = dto.FirstName,
            LastName    = dto.LastName,
            Email       = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role        = role,
            CustomRole  = string.IsNullOrWhiteSpace(dto.CustomRole) ? null : dto.CustomRole,
            IsActive    = true,
        };

        await _repo.AddAsync(user);
        await _repo.SaveChangesAsync();
        return ApiResponse<UserResponseDto>.Ok(MapToDto(user));
    }

    public async Task<ApiResponse<UserResponseDto>> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) return ApiResponse<UserResponseDto>.Fail("User not found.");

        if (!Enum.TryParse<UserRole>(dto.Role, out var role))
            return ApiResponse<UserResponseDto>.Fail($"Invalid role '{dto.Role}'.");

        user.FirstName  = dto.FirstName;
        user.LastName   = dto.LastName;
        user.Email      = dto.Email;
        user.Role       = role;
        user.CustomRole = string.IsNullOrWhiteSpace(dto.CustomRole) ? null : dto.CustomRole;
        user.IsActive   = dto.IsActive;
        user.UpdatedAt  = DateTime.UtcNow;

        _repo.Update(user);
        await _repo.SaveChangesAsync();
        return ApiResponse<UserResponseDto>.Ok(MapToDto(user));
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

    private static UserResponseDto MapToDto(User u) => new()
    {
        Id         = u.Id,
        FirstName  = u.FirstName,
        LastName   = u.LastName,
        Email      = u.Email,
        Role       = u.Role.ToString(),
        CustomRole = u.CustomRole,
        IsActive   = u.IsActive,
        CreatedAt  = u.CreatedAt,
    };
}
