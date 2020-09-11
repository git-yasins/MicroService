using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contact.API.Dtos;

namespace Contact.API.Data {
    public interface IContactRepository {
        /// <summary>
        /// 更新联系人信息
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        Task<bool> UpdateContactInfoAsync (UserIdentity userInfo, CancellationToken cancellationToken);

        /// <summary>
        /// 添加联系人信息
        /// </summary> 
        /// <param name="userId">用户ID</param>
        /// <param name="contactInfo">联系人信息</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> AddContactAsync (int userId, UserIdentity contactInfo, CancellationToken cancellationToken);
        /// <summary>
        /// 获取联系人列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<Models.Contact>> GetContactsAsync (int userId, CancellationToken cancellationToken);
        /// <summary>
        /// 更新好友列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="contactId">好友ID</param>
        /// <param name="tags">标签列表</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> TagContactAsync (int userId, int contactId, List<string> tags, CancellationToken cancellationToken);
    }
}