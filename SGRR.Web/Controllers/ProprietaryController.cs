using Microsoft.AspNetCore.Mvc;
using SGRR.Data.Interfaces;

namespace SGRR.Web.Controllers;

public class ProprietaryController : OwnerController
{
    public ProprietaryController(IUserRepository userRepository) : base(userRepository)
    {
    }

    [HttpGet]
    public IActionResult Dashboard() => View(CurrentOwner.Restaurant);
}
