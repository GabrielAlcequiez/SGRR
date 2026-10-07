using SGRR.Data.Entities;

namespace SGRR.Data.Interfaces;

public interface IAuthRepository
{
    User? Register(string name, string lastName, string email, string password);
    User Login(string email, string password);
}