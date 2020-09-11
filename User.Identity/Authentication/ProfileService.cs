using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace User.Identity.Authentication {
    /// <summary>
    /// 获取用户Claims,传入JWT字符
    /// </summary>
    public class ProfileService : IProfileService {
        public Task GetProfileDataAsync (ProfileDataRequestContext context) {
            var subject = context.Subject??throw new ArgumentNullException (nameof (context));
            var subjectId = subject.Claims.Where (x => x.Type == "sub").FirstOrDefault ().Value;

            if (!int.TryParse (subjectId, out int intUserId)) {
                throw new ArgumentException ("Invalid subject identifier");
            }

            context.IssuedClaims = context.Subject.Claims.ToList ();
            return Task.CompletedTask;
        }

        public Task IsActiveAsync (IsActiveContext context) {
            var subject = context.Subject??throw new ArgumentNullException (nameof (context));
            var subjectId = subject.Claims.Where (x => x.Type == "sub").FirstOrDefault ().Value;
            context.IsActive = int.TryParse (subjectId, out int intUserId);

            return Task.CompletedTask;
        }
    }
}