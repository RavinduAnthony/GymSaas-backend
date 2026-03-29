namespace GymSaaS.Application.DTOs.Settings;

public record WorkingHoursResponseDto(Guid Id, string Day, string OpenTime, string CloseTime, bool IsClosed);

public record UpsertWorkingHoursItemDto(string Day, string OpenTime, string CloseTime, bool IsClosed);
