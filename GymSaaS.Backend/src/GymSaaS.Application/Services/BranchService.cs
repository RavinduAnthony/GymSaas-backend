using GymSaaS.Application.DTOs.Branches;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Shared;

namespace GymSaaS.Application.Services;

public class BranchService
{
    private readonly IRepository<Branch> _branchRepo;
    private readonly ITenantProvider _tenantProvider;

    public BranchService(IRepository<Branch> branchRepo, ITenantProvider tenantProvider)
    {
        _branchRepo = branchRepo;
        _tenantProvider = tenantProvider;
    }

    public async Task<ApiResponse<IEnumerable<BranchResponseDto>>> GetAllAsync()
    {
        try
        {
            var branches = await _branchRepo.GetAllAsync();
            return ApiResponse<IEnumerable<BranchResponseDto>>.Ok(branches.Select(MapToDto));
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<BranchResponseDto>>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BranchResponseDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var branch = await _branchRepo.GetByIdAsync(id);
            if (branch == null) return ApiResponse<BranchResponseDto>.Fail("Branch not found.");
            return ApiResponse<BranchResponseDto>.Ok(MapToDto(branch));
        }
        catch (Exception ex)
        {
            return ApiResponse<BranchResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BranchResponseDto>> CreateAsync(CreateBranchDto dto)
    {
        try
        {
            var branch = new Branch
            {
                TenantId = _tenantProvider.TenantId,
                Name = dto.Name,
                Address = dto.Address,
                Phone = dto.Phone,
            };

            await _branchRepo.AddAsync(branch);
            await _branchRepo.SaveChangesAsync();

            return ApiResponse<BranchResponseDto>.Ok(MapToDto(branch), "Branch created.");
        }
        catch (Exception ex)
        {
            return ApiResponse<BranchResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BranchResponseDto>> UpdateAsync(Guid id, UpdateBranchDto dto)
    {
        try
        {
            var branch = await _branchRepo.GetByIdAsync(id);
            if (branch == null) return ApiResponse<BranchResponseDto>.Fail("Branch not found.");

            branch.Name = dto.Name;
            branch.Address = dto.Address;
            branch.Phone = dto.Phone;
            branch.IsActive = dto.IsActive;

            _branchRepo.Update(branch);
            await _branchRepo.SaveChangesAsync();

            return ApiResponse<BranchResponseDto>.Ok(MapToDto(branch), "Branch updated.");
        }
        catch (Exception ex)
        {
            return ApiResponse<BranchResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeleteAsync(Guid id)
    {
        try
        {
            var branch = await _branchRepo.GetByIdAsync(id);
            if (branch == null) return ApiResponse.Fail("Branch not found.");

            _branchRepo.Delete(branch);
            await _branchRepo.SaveChangesAsync();

            return ApiResponse.Ok("Branch deleted.");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"An error occurred: {ex.Message}");
        }
    }

    private static BranchResponseDto MapToDto(Branch b) => new()
    {
        Id = b.Id,
        Name = b.Name,
        Address = b.Address,
        Phone = b.Phone,
        IsActive = b.IsActive,
        CreatedAt = b.CreatedAt,
    };
}
