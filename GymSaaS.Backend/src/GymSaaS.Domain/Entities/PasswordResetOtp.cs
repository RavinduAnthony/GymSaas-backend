namespace GymSaaS.Domain.Entities;

/// <summary>
/// Stores a 6-digit one-time password for password reset flows.
/// Not tenant-scoped — looked up by email alone.
/// </summary>
public class PasswordResetOtp
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>The email address the OTP was issued for.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>6-digit code sent to the user.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>UTC timestamp after which the OTP is invalid (CreatedAt + 1 minute).</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Set to true once the OTP has been successfully verified and used.</summary>
    public bool IsUsed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
