using System.Security.Claims;
using VehicleParts.Domain.Models;

namespace VehicleParts.Application.Interfaces.IServices;

public interface IAuthService
{
    string GenerateToken(User user, IList<string> userRoles);
}