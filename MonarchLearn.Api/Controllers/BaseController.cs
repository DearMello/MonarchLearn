using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.Extensions;

namespace MonarchLearn.Api.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected int CurrentUserId => User.GetUserId();
        protected bool IsAdmin => User.IsInRole("Admin");
    }
}
