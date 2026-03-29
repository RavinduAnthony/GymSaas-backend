namespace GymSaaS.Application.Interfaces;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType, string tenantId, string category);
    Task DeleteImageAsync(string tenantId, string category);
}
