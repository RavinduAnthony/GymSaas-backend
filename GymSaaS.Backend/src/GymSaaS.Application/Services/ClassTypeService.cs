using GymSaaS.Application.DTOs.ClassTypes;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Shared;

namespace GymSaaS.Application.Services;

public class ClassTypeService
{
    private readonly IRepository<ClassType> _classTypeRepo;
    private readonly IRepository<GymClass> _gymClassRepo;
    private readonly ITenantProvider _tenantProvider;

    public ClassTypeService(
        IRepository<ClassType> classTypeRepo,
        IRepository<GymClass> gymClassRepo,
        ITenantProvider tenantProvider)
    {
        _classTypeRepo = classTypeRepo;
        _gymClassRepo = gymClassRepo;
        _tenantProvider = tenantProvider;
    }

    public async Task<ApiResponse<IEnumerable<ClassTypeResponseDto>>> GetAllAsync()
    {
        try
        {
            var types = await _classTypeRepo.GetAllAsync();
            return ApiResponse<IEnumerable<ClassTypeResponseDto>>.Ok(types.OrderBy(t => t.Name).Select(MapToDto));
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<ClassTypeResponseDto>>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ClassTypeResponseDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var classType = await _classTypeRepo.GetByIdAsync(id);
            if (classType == null) return ApiResponse<ClassTypeResponseDto>.Fail("Class type not found.");
            return ApiResponse<ClassTypeResponseDto>.Ok(MapToDto(classType));
        }
        catch (Exception ex)
        {
            return ApiResponse<ClassTypeResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ClassTypeResponseDto>> CreateAsync(CreateClassTypeDto dto)
    {
        try
        {
            var existing = await _classTypeRepo.FindAsync(t => t.Name.ToLower() == dto.Name.ToLower());
            if (existing.Any())
                return ApiResponse<ClassTypeResponseDto>.Fail($"A class type named '{dto.Name}' already exists.");

            var classType = new ClassType
            {
                TenantId = _tenantProvider.TenantId,
                Name = dto.Name,
                Description = dto.Description,
            };

            await _classTypeRepo.AddAsync(classType);
            await _classTypeRepo.SaveChangesAsync();

            return ApiResponse<ClassTypeResponseDto>.Ok(MapToDto(classType), "Class type created.");
        }
        catch (Exception ex)
        {
            return ApiResponse<ClassTypeResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ClassTypeResponseDto>> UpdateAsync(Guid id, UpdateClassTypeDto dto)
    {
        try
        {
            var classType = await _classTypeRepo.GetByIdAsync(id);
            if (classType == null) return ApiResponse<ClassTypeResponseDto>.Fail("Class type not found.");

            var duplicate = await _classTypeRepo.FindAsync(t => t.Name.ToLower() == dto.Name.ToLower() && t.Id != id);
            if (duplicate.Any())
                return ApiResponse<ClassTypeResponseDto>.Fail($"A class type named '{dto.Name}' already exists.");

            classType.Name = dto.Name;
            classType.Description = dto.Description;
            classType.IsActive = dto.IsActive;
            classType.UpdatedAt = DateTime.UtcNow;

            _classTypeRepo.Update(classType);
            await _classTypeRepo.SaveChangesAsync();

            return ApiResponse<ClassTypeResponseDto>.Ok(MapToDto(classType), "Class type updated.");
        }
        catch (Exception ex)
        {
            return ApiResponse<ClassTypeResponseDto>.Fail($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeleteAsync(Guid id)
    {
        try
        {
            var classType = await _classTypeRepo.GetByIdAsync(id);
            if (classType == null) return ApiResponse.Fail("Class type not found.");

            var assignedClasses = await _gymClassRepo.FindAsync(c => c.Category == classType.Name);
            if (assignedClasses.Any())
                return ApiResponse.Fail($"Cannot delete '{classType.Name}' — it is used by {assignedClasses.Count()} class(es). Update those classes first.");

            _classTypeRepo.Delete(classType);
            await _classTypeRepo.SaveChangesAsync();

            return ApiResponse.Ok("Class type deleted.");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"An error occurred: {ex.Message}");
        }
    }

    private static ClassTypeResponseDto MapToDto(ClassType ct) => new()
    {
        Id = ct.Id,
        TenantId = ct.TenantId,
        Name = ct.Name,
        Description = ct.Description,
        IsActive = ct.IsActive,
        CreatedAt = ct.CreatedAt,
    };
}
