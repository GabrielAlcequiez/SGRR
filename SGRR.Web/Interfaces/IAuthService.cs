using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;

namespace SGRR.Web.Interfaces;

public interface IAuthService
{
    User Register(string name, string lastName, string email, string password, Roles role);
    Task<User> RegisterProprietaryAsync(
        string name,
        string lastName,
        string email,
        string password,
        string restaurantName,
        string specialty,
        string location,
        string? description);
    User? Login(string email, string password);
}
