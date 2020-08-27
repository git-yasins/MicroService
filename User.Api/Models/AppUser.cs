using System.Collections.Generic;
namespace User.API.Models {
    /// <summary>
    /// 用户信息
    /// </summary>
    public class AppUser {
        public AppUser () {
            Properties = new List<UserProperty> ();
        }
        /// <summary>
        /// 用户ID
        /// </summary>
        /// <value></value>
        public int Id { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        /// <value></value>
        public string Name { get; set; }
        /// <summary>
        /// 公司
        /// </summary>
        /// <value></value>
        public string Company { get; set; }
        /// <summary>
        /// 职位
        /// </summary>
        /// <value></value>
        public string Title { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        /// <value></value>
        public string Phone { get; set; }
        /// <summary>
        /// 头像地址
        /// </summary>
        /// <value></value>
        public string Avatar { get; set; }
        /// <summary>
        /// 性别 1:男 0:女
        /// </summary>
        /// <value></value>
        public string Gender { get; set; }
        /// <summary>
        /// 详细地址
        /// </summary>
        /// <value></value>
        public string Address { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        /// <value></value>
        public string Email { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        /// <value></value>
        public string Tel { get; set; }
        /// <summary>
        /// 省ID
        /// </summary>
        /// <value></value>
        public int ProvinceId { get; set; }
        /// <summary>
        /// 省名称
        /// </summary>
        /// <value></value>
        public string Province { get; set; }
        /// <summary>
        /// 城市名称
        /// </summary>
        /// <value></value>
        public string City { get; set; }
        /// <summary>
        /// 名片地址
        /// </summary>
        /// <value></value>
        public string NameGard { get; set; }
        /// <summary>
        /// 用户属性列表
        /// </summary>
        /// <value></value>
        public List<UserProperty> Properties { get; set; }
    }
}