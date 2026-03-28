using GymSaaS.Application.DTOs.Tenant;
using GymSaaS.Application.Interfaces;
using GymSaaS.Domain.Entities;
using GymSaaS.Persistence;
using BCrypt.Net;

namespace GymSaaS.Application.Services;

public class TenantService : ITenantService
{
    private readonly GymDbContext _context;

    public TenantService(GymDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> RegisterTenantAsync(RegisterTenantRequest request)
    {
        try
        {
            // 1. Create the new Tenant (Gym)
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                GymName = request.GymName,
                OwnerName = request.OwnerName,
                Email = request.OwnerEmail,
                Phone = request.PhoneNumber,
                SubDomain = request.GymName.ToLower().Replace(" ", "-") + "-" + Guid.NewGuid().ToString().Substring(0, 4),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tenants.Add(tenant);

            // 2. Create the Owner User
            var user = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                FirstName = request.OwnerName.Split(" ").First(),
                LastName = request.OwnerName.Split(" ").Length > 1 ? string.Join(" ", request.OwnerName.Split(" ").Skip(1)) : "",
                Email = request.OwnerEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = Domain.Enums.UserRole.Owner,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            // 3. Save to database
            await _context.SaveChangesAsync();

            return tenant.Id;
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while creating the tenant: {ex.Message}", ex);
        }
    }
}
