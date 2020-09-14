using System.Net;
using System.Security.AccessControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using User.API.Data;
using User.API.Dtos;
using User.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace User.API.Controllers {
    //[ApiController]
    //[Authorize]
    [Route ("api/users")]
    public class UserController : BaseController {
        private UserContext _userContext;
        private ILogger<UserController> _logger;
        private readonly ICapPublisher _capPublisher;

        public UserController (UserContext userContext, ILogger<UserController> logger, ICapPublisher capPublisher) {
            _userContext = userContext;
            _logger = logger;
            _capPublisher = capPublisher;
        }
        /// <summary>
        /// 发送用户变更消息
        /// </summary>
        /// <param name="appUser">用户数据</param>
        private void RaiseUserProfileChangedEvent (AppUser user) {
            //判断数据是否更改
            if (_userContext.Entry (user).Property (nameof (user.Name)).IsModified ||
                _userContext.Entry (user).Property (nameof (user.Title)).IsModified ||
                _userContext.Entry (user).Property (nameof (user.Company)).IsModified ||
                _userContext.Entry (user).Property (nameof (user.Avatar)).IsModified) {
                //发布消息到rabbitMQ
                _capPublisher.Publish ("finbook.user_api.user_profile_changed", new UserIdentity {
                    UserId = user.Id,
                        Name = user.Name,
                        Title = user.Title,
                        Avatar = user.Avatar,
                        Company = user.Company
                });
            }
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
        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="patch">局部更新JsonPatchDocument</param>
        /// <returns></returns>
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

            //处理事务
            using (var transaction = _userContext.Database.BeginTransaction ()) {
               //发布用户变更消息
                RaiseUserProfileChangedEvent (user);
              
                _userContext.Users.Update (user);
                _userContext.SaveChanges ();

                transaction.Commit ();
            }
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
                await _userContext.SaveChangesAsync ();
            }

            return Ok (new { user.Id, user.Name, user.Company, user.Title, user.Avatar });
        }
        /// <summary>
        /// 获取用户标签
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route ("tags")]
        public async Task<IActionResult> GetUserTags () {
            return Ok (await _userContext.UserTags.Where (u => u.UserId == UserIdentity.UserId).ToListAsync ());
        }
        /// <summary>
        /// 根据用户手机查找用户资料
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpPost]
        [Route ("search")]
        public async Task<IActionResult> Search (string phone) {
            return Ok (await _userContext.Users.Include (u => u.Properties).SingleOrDefaultAsync (u => u.Id == UserIdentity.UserId && u.Phone == phone));
        }
        /// <summary>
        /// 更新用户标签
        /// </summary>
        /// <param name="tags">用户标签列表</param>
        /// <returns></returns>
        [HttpPut]
        [Route ("tags")]
        public async Task<IActionResult> UpdateUserTags ([FromBody] List<string> tags) {
            var originTags = await _userContext.UserTags.Where (x => x.UserId == UserIdentity.UserId).ToListAsync ();
            var newTags = tags.Except (originTags.Select (c => c.Tag));

            await _userContext.UserTags.AddRangeAsync (
                newTags.Select (t => new Models.UserTag {
                    CreateTime = DateTime.Now,
                        UserId = UserIdentity.UserId,
                        Tag = t
                })
            );

            await _userContext.SaveChangesAsync ();
            return Ok ();
        }

        [HttpGet]
        [Route ("baseinfo/{userId}")]
        public async Task<IActionResult> GetBaseInfo (int userId) {
            //检查用户是否好友关系

            var user = await _userContext.Users.SingleOrDefaultAsync (u => u.Id == userId);
            if (user == null) {
                return NotFound ();
            }

            return Ok (new {
                userId = user.Id,
                    user.Name,
                    user.Company,
                    user.Title,
                    user.Avatar
            });
        }

    }
}