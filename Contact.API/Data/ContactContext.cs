using System.Collections.Generic;
using Contact.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Contact.API.Data {
    public class ContactContext {
        private IMongoDatabase _database;
        private IMongoCollection<ContactBook> _collection;
        private AppSettings _appSettings;

        public ContactContext (IOptions<AppSettings> settings) {
            _appSettings = settings.Value;
            var client = new MongoClient (_appSettings.MongoConnectionString);
            if (client != null) {
                _database = client.GetDatabase (_appSettings.ContactDatabaseName);
            }
        }
        /// <summary>
        /// 用户通讯录
        /// </summary>
        /// <value></value>
        public IMongoCollection<ContactBook> ContactBooks {
            get {
                CheckAndCreateCollection ("ContactBooks");
                return _database.GetCollection<ContactBook> ("ContactBooks");
            }
        }
        /// <summary>
        /// 好友申请请求记录
        /// </summary>
        /// <value></value>
        public IMongoCollection<ContactApplyRequest> ContactApplyRequests {
            get {
                CheckAndCreateCollection ("ContactApplyRequests");
                return _database.GetCollection<ContactApplyRequest> ("ContactApplyRequests");
            }
        }
        
        /// <summary>
        /// 遍历检查mongoDB collection 指定列表是否存在,否则创建
        /// </summary>
        /// <param name="collectionName">列表名称</param>
        private void CheckAndCreateCollection (string collectionName) {
            var collectionList = _database.ListCollections ().ToList ();
            var collectionNames = new List<string> ();

            collectionList.ForEach (document => collectionNames.Add (document["name"].AsString));
            if (!collectionNames.Contains (collectionName)) {
                _database.CreateCollection (collectionName);
            }
        }
    }
}