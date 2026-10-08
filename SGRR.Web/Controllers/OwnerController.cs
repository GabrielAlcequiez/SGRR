using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;

namespace SGRR.Web.Controllers;

public abstract class OwnerController : Controller
{
    private readonly IUserRepository _userRepository;
    protected User CurrentOwner { get; private set; } = null!;

    protected OwnerController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
        {
            context.Result = RedirectToAction("Login", "Auth");
            return;
        }

        var user = _userRepository.GetById(userId);
        if (user is null)
        {
            context.Result = RedirectToAction("Login", "Auth");
            return;
        }
        if (!user.IsActive || user.Role != Roles.Propietary)
        {
            context.Result = StatusCode(StatusCodes.Status403Forbidden);
            return;
        }

        CurrentOwner = user;
        base.OnActionExecuting(context);
    }
}
