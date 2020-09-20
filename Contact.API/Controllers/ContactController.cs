using System;
using System.Threading;
using System.Threading.Tasks;
using Contact.API.Data;
using Contact.API.Models;
using Contact.API.Service;
using Contact.API.ViewModels;
using Microsoft.AspNetCore.Mvc;
namespace Contact.API.Controllers {
    [Route ("api/contacts")]
    public class ContactController : BaseController {
        private readonly IContactApplyRequestRepository _contactApplyRequestRepository;
        private readonly IUserService _userService;
        private readonly IContactRepository _contactRepository;

        public ContactController (
            IContactApplyRequestRepository contactApplyRequestRepository,
            IUserService userService,
            IContactRepository contactRepository) {
            _contactApplyRequestRepository = contactApplyRequestRepository;
            _userService = userService;
            _contactRepository = contactRepository;
        }

        [HttpGet]
        [Route ("")]
        public async Task<IActionResult> Get (CancellationToken cancellation) {
            return Ok (await _contactRepository.GetContactsAsync (UserIdentity.UserId, cancellation));
        }

        /// <summary>
        /// 根据指定用户ID查询用户联系人
        /// </summary>
        /// <param name="userId">ID</param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        [HttpGet]
        [Route ("{userId}")]
        public async Task<IActionResult> Get (int userId, CancellationToken cancellation) {
            return Ok (await _contactRepository.GetContactsAsync (userId, cancellation));
        }

        [HttpPut]
        [Route ("tag")]
        public async Task<IActionResult> TagContact ([FromBody] TagContactInputViewModel viewModel, CancellationToken cancellationToken) {
            var result = await _contactRepository.TagContactAsync (UserIdentity.UserId, viewModel.ContactId, viewModel.Tags, cancellationToken);
            if (!result) {
                return BadRequest ();
            }
            return Ok (result);
        }

        /// <summary>
        /// 获取好友申请列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route ("apply-requests")]
        public async Task<IActionResult> GetApplyRequest (CancellationToken cancellationToken) {
            var request = await _contactApplyRequestRepository.GetRequestListAsync (UserIdentity.UserId, cancellationToken);
            return Ok (request);
        }
        /// <summary>
        /// 添加好友请求
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route ("apply-requests/{userId}")]
        public async Task<IActionResult> AddApplyRequest (int userId, CancellationToken cancellationToken) {
            var baseUserInfo = await _userService.GetBaseUserInfoAsync (UserIdentity.UserId);
            if (baseUserInfo == null) {
                throw new Exception ("用户参数错误");
            }

            var result = await _contactApplyRequestRepository.AddRequestAsync (new ContactApplyRequest {
                UserId = userId,
                    ApplierId = UserIdentity.UserId,
                    Name = baseUserInfo.Name,
                    Company = baseUserInfo.Company,
                    ApplyTime = DateTime.Now,
                    Title = baseUserInfo.Title,
                    Avatar = baseUserInfo.Avatar
            }, cancellationToken);

            if (!result) {
                return BadRequest ();
            }

            return Ok (baseUserInfo);
        }
        /// <summary>
        /// 通过好友申请列表
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route ("apply-requests/{applierId}")]
        public async Task<IActionResult> ApprovalApplyRequest (int applierId, CancellationToken cancellationToken) {
            var result = await _contactApplyRequestRepository.ApprovalAsync (UserIdentity.UserId, applierId, cancellationToken);
            if (!result) {
                return BadRequest ();
            }
            //申请人用户信息
            var applierInfo = await _userService.GetBaseUserInfoAsync (applierId);
            //当前用户信息
            var userInfo = await _userService.GetBaseUserInfoAsync (UserIdentity.UserId);
            //添加好友为双向添加
            await _contactRepository.AddContactAsync (UserIdentity.UserId, applierInfo, cancellationToken);
            await _contactRepository.AddContactAsync (applierId, userInfo, cancellationToken);
            return Ok ();
        }
    }
}