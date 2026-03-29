using GymSaaS.Application.DTOs.Packages;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Shared;

namespace GymSaaS.Application.Services;

public class PackageService
{
    private readonly IRepository<MembershipPackage> _packageRepo;
    private readonly ITenantProvider _tenantProvider;

    public PackageService(IRepository<MembershipPackage> packageRepo, ITenantProvider tenantProvider)
    {
        _packageRepo = packageRepo;
        _tenantProvider = tenantProvider;
    }

    public async Task<ApiResponse<IEnumerable<PackageResponseDto>>> GetAllAsync()
    {
        try
        {
            var packages = await _packageRepo.GetAllAsync();
            return ApiResponse<IEnumerable<PackageResponseDto>>.Ok(packages.Select(MapToDto));
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<PackageResponseDto>>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PackageResponseDto>> CreateAsync(CreatePackageDto dto)
    {
        try
        {
            var package = new MembershipPackage
            {
                TenantId = _tenantProvider.TenantId,
                Name = dto.Name,
                Duration = dto.Duration,
                Price = dto.Price,
                Branch = dto.Branch,
                Description = dto.Description,
                MaxVisits = dto.MaxVisits,
                TrainerIncluded = dto.TrainerIncluded,
                FreezeDays = dto.FreezeDays,
                DiscountAllowed = dto.DiscountAllowed,
                BillingFrequency = dto.BillingFrequency,
            };

            await _packageRepo.AddAsync(package);
            await _packageRepo.SaveChangesAsync();

            return ApiResponse<PackageResponseDto>.Ok(MapToDto(package), "Package created.");
        }
        catch (Exception ex)
        {
            return ApiResponse<PackageResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PackageResponseDto>> UpdateAsync(Guid id, UpdatePackageDto dto)
    {
        try
        {
            var package = await _packageRepo.GetByIdAsync(id);
            if (package == null) return ApiResponse<PackageResponseDto>.Fail("Package not found.");

            package.Name = dto.Name;
            package.Duration = dto.Duration;
            package.Price = dto.Price;
            package.Branch = dto.Branch;
            package.Status = dto.Status;
            package.Description = dto.Description;
            package.MaxVisits = dto.MaxVisits;
            package.TrainerIncluded = dto.TrainerIncluded;
            package.FreezeDays = dto.FreezeDays;
            package.DiscountAllowed = dto.DiscountAllowed;
            package.BillingFrequency = dto.BillingFrequency;

            _packageRepo.Update(package);
            await _packageRepo.SaveChangesAsync();

            return ApiResponse<PackageResponseDto>.Ok(MapToDto(package), "Package updated.");
        }
        catch (Exception ex)
        {
            return ApiResponse<PackageResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeleteAsync(Guid id)
    {
        try
        {
            var package = await _packageRepo.GetByIdAsync(id);
            if (package == null) return ApiResponse.Fail("Package not found.");

            _packageRepo.Delete(package);
            await _packageRepo.SaveChangesAsync();

            return ApiResponse.Ok("Package deleted.");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"An error occurred: {ex.Message}");
        }
    }

    private static PackageResponseDto MapToDto(MembershipPackage p) => new()
    {
        Id = p.Id,
        TenantId = p.TenantId,
        Name = p.Name,
        Duration = p.Duration,
        Price = p.Price,
        Branch = p.Branch,
        Status = p.Status,
        Description = p.Description,
        MaxVisits = p.MaxVisits,
        TrainerIncluded = p.TrainerIncluded,
        FreezeDays = p.FreezeDays,
        DiscountAllowed = p.DiscountAllowed,
        BillingFrequency = p.BillingFrequency,
        CreatedAt = p.CreatedAt,
    };
}
