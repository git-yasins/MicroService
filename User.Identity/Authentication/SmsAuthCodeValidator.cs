using System;
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
            _authCodeService = authCodeService??throw new ArgumentNullException (nameof (authCodeService));
            _userService = userService??throw new ArgumentNullException (nameof (userService));
        }

        public async Task ValidateAsync (ExtensionGrantValidationContext context) {
            var phone = context.Request.Raw["phone"];
            var code = context.Request.Raw["auth_code"];
            var errorValidationResult = new GrantValidationResult (TokenRequestErrors.InvalidGrant);

            if (string.IsNullOrWhiteSpace (phone) || string.IsNullOrEmpty (code)) {
                context.Result = errorValidationResult;
            }

            // 检查状态码
            if (! _authCodeService.Validate (phone, code)) {
                context.Result = errorValidationResult;
                return;
            }

            // 完成用户注册
            var userId = await _userService.CheckOrCreate (phone);
            if (userId <= 0) {
                context.Result = errorValidationResult;
                return;
            }

            context.Result = new GrantValidationResult (userId.ToString (), GrantType);
        }
    }
}