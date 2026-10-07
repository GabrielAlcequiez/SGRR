using SGRR.Data.Entities;

namespace SGRR.Data.Interfaces;

public interface IUserRepository
{
    void Add(User user);
    void Update(User user);
    IEnumerable<User> GetAll();
    User GetById(Guid id);
}