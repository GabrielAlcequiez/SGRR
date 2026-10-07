using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;

namespace SGRR.Data.Interfaces;

public interface IAuthRepository
{
    User? Register(string name, string lastName, string email, string password, Roles role);
    User? Login(string email, string password);
}
