using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GymSaaS.Application.Interfaces;
using GymSaaS.Infrastructure.Cloudinary;
using Microsoft.Extensions.Options;

namespace GymSaaS.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly global::CloudinaryDotNet.Cloudinary _cloudinary;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/png", "image/webp"
    };

    private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB

    public CloudinaryService(IOptions<CloudinarySettings> settings)
    {
        var s = settings.Value;
        var account = new Account(s.CloudName, s.ApiKey, s.ApiSecret);
        _cloudinary = new global::CloudinaryDotNet.Cloudinary(account) { Api = { Secure = true } };
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType, string tenantId, string category, string? uniqueId = null)
    {
        if (fileStream == null || fileStream.Length == 0)
            throw new ArgumentException("No file provided.");

        if (fileStream.Length > MaxFileSizeBytes)
            throw new ArgumentException("File size exceeds the 2 MB limit.");

        if (!AllowedContentTypes.Contains(contentType))
            throw new ArgumentException("Only JPEG, PNG, or WebP images are allowed.");

        var publicId = uniqueId != null ? $"{category}-{uniqueId}" : $"{category}-logo";

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = $"gym-saas/{tenantId}/{category}",
            Overwrite = uniqueId == null,
            UniqueFilename = false,
            PublicId = publicId
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new Exception($"Cloudinary upload failed: {result.Error.Message}");

        return result.SecureUrl.ToString();
    }

    public async Task DeleteImageAsync(string tenantId, string category)
    {
        // PublicId is the full path without the cloud name
        var publicId = $"gym-saas/{tenantId}/{category}/{category}-logo";
        var deleteParams = new DeletionParams(publicId) { ResourceType = ResourceType.Image };
        var result = await _cloudinary.DestroyAsync(deleteParams);

        if (result.Error != null)
            throw new Exception($"Cloudinary delete failed: {result.Error.Message}");
    }

    public async Task DeleteImageByUrlAsync(string photoUrl)
    {
        // Extract public ID from a Cloudinary URL.
        // URL format: https://res.cloudinary.com/{cloud}/image/upload/v{ver}/{public_id}.{ext}
        var marker = "/upload/";
        var idx = photoUrl.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) throw new ArgumentException("Not a valid Cloudinary URL.");

        var afterUpload = photoUrl[(idx + marker.Length)..];

        // Strip optional version segment like "v1234567890/"
        if (afterUpload.StartsWith('v') && afterUpload.IndexOf('/') is int slashIdx && slashIdx > 1
            && int.TryParse(afterUpload[1..slashIdx], out _))
        {
            afterUpload = afterUpload[(slashIdx + 1)..];
        }

        // Strip file extension
        var dotIdx = afterUpload.LastIndexOf('.');
        var publicId = dotIdx >= 0 ? afterUpload[..dotIdx] : afterUpload;

        var deleteParams = new DeletionParams(publicId) { ResourceType = ResourceType.Image };
        var result = await _cloudinary.DestroyAsync(deleteParams);

        if (result.Error != null)
            throw new Exception($"Cloudinary delete failed: {result.Error.Message}");
    }
}
