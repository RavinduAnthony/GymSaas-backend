using GymSaaS.Application.DTOs.Roles;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Shared;
using Microsoft.EntityFrameworkCore;

namespace GymSaaS.Application.Services;

public class RoleService
{
    private readonly IRepository<AppRole> _roleRepo;
    private readonly IRepository<RolePermission> _permRepo;
    private readonly ITenantProvider _tenantProvider;

    public RoleService(
        IRepository<AppRole> roleRepo,
        IRepository<RolePermission> permRepo,
        ITenantProvider tenantProvider)
    {
        _roleRepo   = roleRepo;
        _permRepo   = permRepo;
        _tenantProvider = tenantProvider;
    }

    // ── Roles ─────────────────────────────────────────────────

    public async Task<ApiResponse<IEnumerable<AppRoleResponseDto>>> GetAllAsync()
    {
        var roles = await _roleRepo.AsQueryable()
            .OrderBy(r => r.IsSystem ? 0 : 1).ThenBy(r => r.Name)
            .ToListAsync();
        return ApiResponse<IEnumerable<AppRoleResponseDto>>.Ok(roles.Select(MapRoleToDto));
    }

    public async Task<ApiResponse<AppRoleResponseDto>> CreateAsync(CreateRoleDto dto)
    {
        var exists = await _roleRepo.AsQueryable()
            .AnyAsync(r => r.Name == dto.Name);
        if (exists) return ApiResponse<AppRoleResponseDto>.Fail($"A role named '{dto.Name}' already exists.");

        var role = new AppRole
        {
            TenantId = _tenantProvider.TenantId,
            Name     = dto.Name,
            IsSystem = false,
            BaseRole = null,
            Color    = dto.Color,
        };

        await _roleRepo.AddAsync(role);
        await _roleRepo.SaveChangesAsync();

        // New custom roles start with no permissions — admin sets L1-L5 per section
        await SeedPermissionsAsync(dto.Name, Array.Empty<string>());

        return ApiResponse<AppRoleResponseDto>.Ok(MapRoleToDto(role));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(string roleName)
    {
        var role = await _roleRepo.AsQueryable()
            .FirstOrDefaultAsync(r => r.Name == roleName);
        if (role == null) return ApiResponse<bool>.Fail("Role not found.");
        if (role.IsSystem) return ApiResponse<bool>.Fail("System roles cannot be deleted.");

        _roleRepo.Delete(role);

        // Remove all permission rows for this role
        var perms = await _permRepo.AsQueryable()
            .Where(p => p.RoleName == roleName).ToListAsync();
        foreach (var p in perms) _permRepo.Delete(p);

        await _roleRepo.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true);
    }

    // ── Permissions ───────────────────────────────────────────

    public async Task<ApiResponse<IEnumerable<RolePermissionDto>>> GetPermissionsAsync(string roleName)
    {
        var stored = await _permRepo.AsQueryable()
            .Where(p => p.RoleName == roleName)
            .ToListAsync();

        // Return full matrix (all keys) with stored value or false
        var result = UiPermissionKeys.All.Select(key =>
        {
            var row = stored.FirstOrDefault(p => p.PermissionKey == key);
            return new RolePermissionDto
            {
                PermissionKey = key,
                IsAllowed     = row?.IsAllowed ?? false,
            };
        });

        return ApiResponse<IEnumerable<RolePermissionDto>>.Ok(result);
    }

    public async Task<ApiResponse<bool>> UpdatePermissionsAsync(string roleName, UpdateRolePermissionsDto dto)
    {
        var tenantId = _tenantProvider.TenantId;

        // Delete existing rows for this role then bulk-insert
        var existing = await _permRepo.AsQueryable()
            .Where(p => p.RoleName == roleName).ToListAsync();
        foreach (var e in existing) _permRepo.Delete(e);
        await _permRepo.SaveChangesAsync();

        foreach (var item in dto.Permissions)
        {
            await _permRepo.AddAsync(new RolePermission
            {
                TenantId      = tenantId,
                RoleName      = roleName,
                PermissionKey = item.PermissionKey,
                IsAllowed     = item.IsAllowed,
            });
        }
        await _permRepo.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true);
    }

    /// <summary>Returns all permission keys that are IsAllowed=true for a given role.</summary>
    public async Task<ApiResponse<IEnumerable<string>>> GetMyPermissionsAsync(string roleName)
    {
        var perms = await _permRepo.AsQueryable()
            .Where(p => p.RoleName == roleName && p.IsAllowed)
            .Select(p => p.PermissionKey)
            .ToListAsync();
        return ApiResponse<IEnumerable<string>>.Ok(perms);
    }

    // ── Seeding helper (called from AuthService on new tenant) ─

    public async Task SeedSystemRolesAsync(Guid tenantId)
    {
        var systemRoles = new[] { "Owner", "Manager", "Receptionist", "Trainer" };
        foreach (var name in systemRoles)
        {
            var role = new AppRole
            {
                TenantId = tenantId,
                Name     = name,
                IsSystem = true,
                BaseRole = null,
                Color    = name switch
                {
                    "Owner"        => "#F59E0B",
                    "Manager"      => "#6366F1",
                    "Receptionist" => "#10B981",
                    "Trainer"      => "#3B82F6",
                    _              => "#6B7280",
                },
            };
            await _roleRepo.AddAsync(role);
        }
        await _roleRepo.SaveChangesAsync();

        // Seed default permissions for each system role
        foreach (var name in systemRoles)
            await SeedPermissionsAsync(name, UiPermissionKeys.DefaultsFor(name), tenantId);
    }

    private async Task SeedPermissionsAsync(string roleName, IReadOnlyList<string> allowedKeys, Guid? overrideTenantId = null)
    {
        var tenantId = overrideTenantId ?? _tenantProvider.TenantId;
        foreach (var key in UiPermissionKeys.All)
        {
            await _permRepo.AddAsync(new RolePermission
            {
                TenantId      = tenantId,
                RoleName      = roleName,
                PermissionKey = key,
                IsAllowed     = allowedKeys.Contains(key),
            });
        }
        await _permRepo.SaveChangesAsync();
    }

    private static AppRoleResponseDto MapRoleToDto(AppRole r) => new()
    {
        Id       = r.Id,
        Name     = r.Name,
        IsSystem = r.IsSystem,
        BaseRole = r.BaseRole,
        Color    = r.Color,
    };
}
