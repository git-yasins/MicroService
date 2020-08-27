using Microsoft.AspNetCore.Mvc;
using User.API.Dtos;
namespace User.API.Controllers
{
    public class BaseController:ControllerBase
    {
        public UserIdentity UserIdentity => new UserIdentity { UserId = 1, Name = "yasin00" };
    }
}