using SGRR.Data.Entities;
using SGRR.Data.Interfaces;

namespace SGRR.Data.Repositories;

public class AuthRepository : IAuthRepository
{
    public User? Register(string name, string lastName, string email, string password)
    {
        throw new NotImplementedException();
    }

    public User Login(string email, string password)
    {
        throw new NotImplementedException();
    }
}