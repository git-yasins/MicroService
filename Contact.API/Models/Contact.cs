using System.Collections.Generic;

namespace Contact.API.Models {
    /// <summary>
    /// 联系人
    /// </summary>
    public class Contact {
        public Contact () {
            Tags = new List<string> ();
        }
        public int UserId { get; set; }
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
        /// 用户标签
        /// </summary>
        /// <value></value>
        public List<string> Tags { get; set; }
    }
}