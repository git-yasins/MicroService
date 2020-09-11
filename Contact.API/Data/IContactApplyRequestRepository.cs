using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contact.API.Models;

namespace Contact.API.Data {
    /// <summary>
    /// 联系人好友申请接口
    /// </summary>
    public interface IContactApplyRequestRepository {
        /// <summary>
        /// 添加申请好友的请求
        /// </summary>
        /// <param name="request">好友申请信息</param>
        /// <returns></returns>
        Task<bool> AddRequestAsync (ContactApplyRequest request, CancellationToken cancellationToken);
        /// <summary>
        /// 通过好友请求
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="applierId">申请人ID</param>
        /// <returns></returns>
        Task<bool> ApprovalAsync (int userId, int applierId, CancellationToken cancellationToken);
        /// <summary>
        /// 获取好友申请列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<ContactApplyRequest>> GetRequestListAsync (int userId, CancellationToken cancellationToken);
    }
}