using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contact.API.Dtos;
using Contact.API.Models;
using MongoDB.Driver;
namespace Contact.API.Data {
    /// <summary>
    /// MongoDB 通讯录联系人操作
    /// </summary>
    public class MongoContactRepository : IContactRepository {
        private readonly ContactContext _contactContext;
        public MongoContactRepository (ContactContext contactContext) {
            this._contactContext = contactContext;
        }

        /// <summary>
        /// 添加好友到通讯录列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="contactInfo">联系人信息</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> AddContactAsync (int userId, UserIdentity contactInfo, CancellationToken cancellationToken) {

            //检查通讯录是否存在
            if (await _contactContext.ContactBooks.CountDocumentsAsync (x => x.UserId == userId) == 0) {
                //初始通讯录
                await _contactContext.ContactBooks.InsertOneAsync (new ContactBook { UserId = userId });
            }

            var filter = Builders<ContactBook>.Filter.Eq (x => x.UserId, userId);
            var update = Builders<ContactBook>.Update.AddToSet (x => x.Contacts,
                new Models.Contact {
                    UserId = contactInfo.UserId,
                        Avatar = contactInfo.Avatar,
                        Company = contactInfo.Company,
                        Name = contactInfo.Name,
                        Title = contactInfo.Title
                });

            var result = await _contactContext.ContactBooks.UpdateOneAsync (filter, update, null, cancellationToken);
            return result.ModifiedCount == result.MatchedCount && result.ModifiedCount == 1;
        }
        /// <summary>
        /// 根据用户ID获取通讯录联系人列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="cancellationToken">取消操作</param>
        /// <returns>联系人列表</returns>
        public async Task<List<Models.Contact>> GetContactsAsync (int userId, CancellationToken cancellationToken) {
            var contactBook = (await _contactContext.ContactBooks.FindAsync (x => x.UserId == userId)).FirstOrDefault (cancellationToken);
            if (contactBook != null) {
                return contactBook.Contacts;
            }
            return new List<Models.Contact> ();
        }
        /// <summary>
        /// 根据用户ID和联系人ID更新联系人标签
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="contactId">联系人ID</param>
        /// <param name="tags">标签列表</param>
        /// <returns>是否更新成功</returns>
        public async Task<bool> TagContactAsync (int userId, int contactId, List<string> tags, CancellationToken cancellationToken) {
            var filter = Builders<ContactBook>.Filter.And (
                Builders<ContactBook>.Filter.Eq (x => x.UserId, userId),
                Builders<ContactBook>.Filter.Eq ("Contacts.UserId", contactId)
            );

            var update = Builders<ContactBook>.Update.Set ("Contacts.$.Tags", tags);

            var result = await _contactContext.ContactBooks.UpdateOneAsync (filter, update, null, cancellationToken);
            return (result.MatchedCount == result.ModifiedCount && result.ModifiedCount == 1);
        }

        /// <summary>
        /// 更新联系人信息
        /// </summary>
        /// <param name="userInfo">用户信息</param>
        /// <param name="cancellationToken">取消操作的标识</param>
        /// <returns></returns>
        public async Task<bool> UpdateContactInfoAsync (UserIdentity userInfo, CancellationToken cancellationToken) {
            var contactBook = (await _contactContext.ContactBooks.FindAsync (x => x.UserId == userInfo.UserId, null, cancellationToken)).FirstOrDefault (cancellationToken);
            if (contactBook == null) {
                //throw new Exception ($"wrong user id for update contact info userId:{userInfo.UserId}");
                return true;
            }

            var contactIds = contactBook.Contacts.Select (x => x.UserId);

            var filter = Builders<ContactBook>.Filter.And (
                Builders<ContactBook>.Filter.In (x => x.UserId, contactIds), //userId的好友
                Builders<ContactBook>.Filter.ElemMatch (x => x.Contacts, c => c.UserId == userInfo.UserId)
            );

            var update = Builders<ContactBook>.Update
                .Set ("Contacts.$.Name", userInfo.Name)
                .Set ("Contacts.$.Avatar", userInfo.Avatar)
                .Set ("Contacts.$.Company", userInfo.Company)
                .Set ("Contacts.$.Title", userInfo.Title);

            var updateResult = _contactContext.ContactBooks.UpdateMany (filter, update);

            return updateResult.MatchedCount == updateResult.ModifiedCount;
        }
    }
}