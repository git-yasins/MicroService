using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using User.API.Dtos;
namespace User.API.Controllers
{
    public class BaseController:ControllerBase
    {
        protected UserIdentity UserIdentity {
            get {
                var identity = new UserIdentity ();
                identity.UserId = Convert.ToInt32 (User.Claims.FirstOrDefault (x => x.Type == "sub").Value);
                identity.Name = User.Claims.FirstOrDefault (x => x.Type == "name").Value;
                identity.Avatar = User.Claims.FirstOrDefault (x => x.Type == "avatar").Value;
                identity.Title = User.Claims.FirstOrDefault (x => x.Type == "title").Value;
                identity.Company = User.Claims.FirstOrDefault (x => x.Type == "company").Value;
                return identity;
            }
        }
    }
}