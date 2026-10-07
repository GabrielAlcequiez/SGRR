using SGRR.Data.Entities;
using SGRR.Data.Common.Enums;

namespace SGRR.Web.Interfaces;

public interface IAdminService
{
    IEnumerable<User> GetAllUsers(User requestor);
    void ToggleUserStatus(Guid targetUserId, bool isActive, User requestor);
}