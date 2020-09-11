using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Contact.API.Models {
    /// <summary>
    /// 添加联系人请求
    /// </summary>
    [BsonIgnoreExtraElements]
    public class ContactApplyRequest {
        public int Id { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        /// <value></value>
        public int UserId { get; set; }

        #region 申请人信息
        /// <summary>
        /// 申请人姓名
        /// </summary>
        /// <value></value>
        public string Name { get; set; }
        public string Company { get; set; }
        /// <summary>
        /// 工作职位
        /// </summary>
        /// <value></value>
        public string Title { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        /// <value></value>
        public string Avatar { get; set; }
        /// <summary>
        /// 申请人
        /// </summary>
        /// <value></value>
        public int ApplierId { get; set; }
        /// <summary>
        /// 是否通过 0:未通过 1通过
        /// </summary>
        /// <value></value>
        public int Approvaled { get; set; }
        /// <summary>
        /// 处理时间
        /// </summary>
        /// <value></value>
        public DateTime HandledTime { get; set; }
        /// <summary>
        /// 申请时间
        /// </summary>
        /// <value></value>
        public DateTime ApplyTime { get; set; }
        #endregion
    }
}