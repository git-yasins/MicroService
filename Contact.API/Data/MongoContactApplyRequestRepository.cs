using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contact.API.Models;
using MongoDB.Driver;

namespace Contact.API.Data {
    /// <summary>
    /// 好友申请
    /// </summary>
    public class MongoContactApplyRequestRepository : IContactApplyRequestRepository {
        private readonly ContactContext _contacContext;
        public MongoContactApplyRequestRepository (ContactContext contacContext) {
            _contacContext = contacContext;
        }

        /// <summary>
        /// 添加好友申请
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> AddRequestAsync (ContactApplyRequest request, CancellationToken cancellationToken) {
            var filter = Builders<ContactApplyRequest>.Filter.Where (r => r.UserId == request.UserId && r.ApplierId == request.ApplierId);

            if ((await _contacContext.ContactApplyRequests.CountDocumentsAsync (filter)) > 0) {
                //存在则更新
                var update = Builders<ContactApplyRequest>.Update.Set (r => r.ApplyTime, DateTime.Now);
                var result = await _contacContext.ContactApplyRequests.UpdateOneAsync (filter, update, null, cancellationToken);
                return result.MatchedCount == result.ModifiedCount && result.MatchedCount == 1;
            }
            //否则新增
            await _contacContext.ContactApplyRequests.InsertOneAsync (request, null, cancellationToken);
            return true;
        }

        /// <summary>
        /// 通过好友申请
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="applierId">申请人ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> ApprovalAsync (int userId, int applierId, CancellationToken cancellationToken) {
            var filter = Builders<ContactApplyRequest>.Filter.Where (r => r.UserId == userId && r.ApplierId == applierId);
            var update = Builders<ContactApplyRequest>.Update.Set (r => r.Approvaled, 1).Set (r => r.HandledTime, DateTime.Now);
            var result = await _contacContext.ContactApplyRequests.UpdateOneAsync (filter, update, null, cancellationToken);
            return result.MatchedCount == result.ModifiedCount && result.MatchedCount == 1;
        }

        /// <summary>
        /// 获取好友申请列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<ContactApplyRequest>> GetRequestListAsync (int userId, CancellationToken cancellationToken) {
            return (await _contacContext.ContactApplyRequests.FindAsync (c => c.UserId == userId)).ToList (cancellationToken);
        }
    }
}