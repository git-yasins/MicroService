using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using User.API.Data;
using User.API.Dtos;
using User.API.Models;

namespace User.API.Controllers {
    //[ApiController]
    [Route ("api/users")]
    public class UserController : BaseController {
        private UserContext _userContext;
        private ILogger<UserController> _logger;
        public UserController (UserContext userContext, ILogger<UserController> logger) {
            _userContext = userContext;
            _logger = logger;
        }

        //[Route("")]
        [HttpGet]
        public async Task<IActionResult> Get () {
            var user = await _userContext.Users.AsNoTracking ()
                .Include (x => x.Properties)
                .SingleOrDefaultAsync (x => x.Name == UserIdentity.Name);
            if (user == null) {
                throw new UserOperationException ($"用户ID查询不存在{UserIdentity.Name}");
            }
            return Ok (user);

        }

        [Route ("")]
        [HttpPatch]
        public async Task<IActionResult> Path ([FromBody] JsonPatchDocument<AppUser> patch) {
            var user = await _userContext.Users.SingleOrDefaultAsync (x => x.Id == UserIdentity.UserId);
            patch.ApplyTo (user, ModelState);

            // if (!TryValidateModel (user)) {
            //     return ValidationProblem (ModelState);
            // }

            foreach (var property in user.Properties) {
                _userContext.Entry (property).State = EntityState.Detached;
            }

            var originProperties = await _userContext.UserPropertys.AsNoTracking ().Where (x => x.AppUserId == UserIdentity.UserId).ToListAsync ();
            var allProperties = originProperties.Union (user.Properties).Distinct ();

            //从原始记录过滤掉匹配项
            var removeProperties = originProperties.Except (user.Properties);

            var newProperties = allProperties.Except (originProperties);

            foreach (var property in originProperties) {
                _userContext.Remove (property);
            }

            //新增修改的项
            foreach (var property in newProperties) {
                _userContext.Add (property);
            }
            _userContext.Users.Update (user);
            _userContext.SaveChanges ();
            System.Console.WriteLine (user.Name);
            return Ok (user);
        }

        // [Route ("")]
        // [HttpPatch]
        // public async Task<IActionResult> Path ([FromBody] JsonPatchDocument<AppUser> patch) {

        //     var user = await _userContext.Users.SingleOrDefaultAsync (x => x.Id == UserIdentity.UserId);
        //     patch.ApplyTo (user, ModelState);

        //     if (!TryValidateModel (user)) {
        //         return ValidationProblem (ModelState);
        //     }

        //     var originProperties = await _userContext.UserPropertys.AsNoTracking ().Where (x => x.AppUserId == UserIdentity.UserId).ToListAsync ();
        //     var newProperties = user.Properties.Except (originProperties);

        //     foreach (var property in originProperties) {
        //         _userContext.Remove (property);
        //     }

        //     //新增修改的项
        //     foreach (var property in newProperties) {
        //         _userContext.Add (property);
        //     }
        //     _userContext.Users.Update (user);
        //     _userContext.SaveChanges ();
        //     return Ok (user);
        // }

        /// <summary>
        /// 重写验证Model 加入控制器
        /// </summary>
        /// <param name="modelStateDictionary"></param>
        /// <returns></returns>
        public override ActionResult ValidationProblem (Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelStateDictionary) {
            var options = HttpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>> ();
            return (ActionResult) options.Value.InvalidModelStateResponseFactory (ControllerContext);
        }

        [Route ("check-or-create")]
        [HttpPost]
        public async Task<IActionResult> CheckOrCreate (string phone) {
            var user = await _userContext.Users.SingleOrDefaultAsync (x => x.Phone == phone);
            if (user == null) {
                user = new AppUser { Phone = phone };
                _userContext.Users.Add (user);
                _userContext.SaveChanges();
            }
            return Ok (user.Id);
        }
    }
}