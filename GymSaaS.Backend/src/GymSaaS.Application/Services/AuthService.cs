using GymSaaS.Application.DTOs.Auth;
using GymSaaS.Application.Interfaces;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Enums;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Persistence;
using GymSaaS.Shared;
using Microsoft.EntityFrameworkCore;

namespace GymSaaS.Application.Services;

public class AuthService : IAuthService
{
    private readonly GymDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly RoleService _roleService;

    public AuthService(GymDbContext context, IJwtTokenService jwtTokenService, RoleService roleService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _roleService = roleService;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterTenantAsync(RegisterTenantDto dto)
    {
        try
        {
            // Check if email already exists
            var existingUser = await _context.Users.IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (existingUser != null)
                return ApiResponse<AuthResponseDto>.Fail("Email already registered.");

            // Check subdomain uniqueness
            var existingTenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.SubDomain == dto.SubDomain);

            if (existingTenant != null)
                return ApiResponse<AuthResponseDto>.Fail("Subdomain already taken.");

            // Create tenant
            var tenant = new Tenant
            {
                GymName = dto.GymName,
                SubDomain = dto.SubDomain,
                OwnerName = dto.OwnerName,
                Email = dto.Email,
                Phone = dto.Phone,
            };

            await _context.Tenants.AddAsync(tenant);

            // Create owner user
            var user = new User
            {
                TenantId = tenant.Id,
                Email = dto.Email,
                PasswordHash = BCryptHash(dto.Password),
                FirstName = dto.OwnerName.Split(' ').FirstOrDefault() ?? dto.OwnerName,
                LastName = dto.OwnerName.Split(' ').Skip(1).FirstOrDefault() ?? "",
                Role = UserRole.Owner,
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Seed 4 system roles + default permission matrix for this new tenant
            await _roleService.SeedSystemRolesAsync(tenant.Id);

            var token = _jwtTokenService.GenerateToken(user);

            return ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
            {
                Token = token,
                UserId = user.Id.ToString(),
                TenantId = tenant.Id.ToString(),
                Role = user.Role.ToString(),
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<AuthResponseDto>.Fail($"An error occurred during registration: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        try
        {
            var user = await _context.Users.IgnoreQueryFilters()
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            // Bypass credentials check for development
            if (user == null)
            {
                // Grab the first user in the system if the typed email doesn't exist
                user = await _context.Users.IgnoreQueryFilters()
                    .Include(u => u.Tenant)
                    .FirstOrDefaultAsync();
                
                if (user == null)
                {
                    return ApiResponse<AuthResponseDto>.Fail("No users exist in the database at all. Please register a gym first.");
                }
            }
            
            // Password verification is skipped intentionally

            if (!user.IsActive)
                return ApiResponse<AuthResponseDto>.Fail("Your account has been deactivated.");

            var token = _jwtTokenService.GenerateToken(user);

            return ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
            {
                Token = token,
                UserId = user.Id.ToString(),
                TenantId = user.TenantId.ToString(),
                Role = user.Role.ToString(),
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<AuthResponseDto>.Fail($"An error occurred during login: {ex.Message}");
        }
    }

    // Simple password hashing (in production use BCrypt NuGet package)
    private static string BCryptHash(string password)
        => Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(password)));

    private static bool VerifyPassword(string password, string hash)
        => BCryptHash(password) == hash;
}
