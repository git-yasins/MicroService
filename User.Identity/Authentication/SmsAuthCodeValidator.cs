using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using User.Identity.Services;
namespace User.Identity.Authentication {
    public class SmsAuthCodeValidator : IExtensionGrantValidator {
        public string GrantType => "sms_auth_code";
        private readonly IAuthCodeService _authCodeService;
        private readonly IUserService _userService;

        public SmsAuthCodeValidator (IAuthCodeService authCodeService, IUserService userService) {
            _authCodeService = authCodeService;
            _userService = userService;
        }

        public async Task ValidateAsync (ExtensionGrantValidationContext context) {
            var phone = context.Request.Raw["phone"];
            var code = context.Request.Raw["auth_code"];
            var errorValidationResult = new GrantValidationResult (TokenRequestErrors.InvalidGrant);

            if (string.IsNullOrWhiteSpace (phone) || string.IsNullOrEmpty (code)) {
                context.Result = errorValidationResult;
            }

            // 检查状态码
            if (!_authCodeService.Validate (phone, code)) {
                context.Result = errorValidationResult;
                return;
            }

            // 完成用户注册
            var userInfo = await _userService.CheckOrCreate (phone);
            if (userInfo == null) {
                context.Result = errorValidationResult;
                return;
            }

            //新增claims,将User.Api的Claims 传送给Contact.API
            var claims = new Claim[] {
                new Claim ("name", userInfo.Name ?? string.Empty),
                new Claim ("title", userInfo.Title ?? string.Empty),
                new Claim ("avatar", userInfo.Avatar ?? string.Empty),
                new Claim ("company", userInfo.Company ?? string.Empty),
            };

            context.Result = new GrantValidationResult (userInfo.Id.ToString (), GrantType, claims);
        }
    }
}