using System.Threading.Tasks;
using System.Threading;
using Contact.API.Data;
using Contact.API.IntegrationEvents.Event;
using DotNetCore.CAP;

namespace Contact.API.IntegrationEvents {
    public class UserProfileChangedEventHandler : ICapSubscribe {
        private readonly IContactRepository contactRepository;
        public UserProfileChangedEventHandler (IContactRepository contactRepository) {
            this.contactRepository = contactRepository;
        }

        /// <summary>
        /// 接收CAP EventBus user.api发送过来的数据
        /// </summary>
        [CapSubscribe ("finbook.user_api.user_profile_changed")]
        public async Task UpdateContactInfo (UserProfileChangedEvent @event) {
            var token = new CancellationToken ();
            await contactRepository.UpdateContactInfoAsync (new Dtos.UserIdentity {
                Title = @event.Avatar,
                    Company = @event.Company,
                    Name = @event.Name,
                    UserId = @event.UserId,
                    Avatar = @event.Avatar
            }, token);
        }
    }
}