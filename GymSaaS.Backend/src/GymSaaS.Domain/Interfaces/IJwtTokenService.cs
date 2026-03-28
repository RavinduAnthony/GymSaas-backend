using GymSaaS.Domain.Entities;

namespace GymSaaS.Domain.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
