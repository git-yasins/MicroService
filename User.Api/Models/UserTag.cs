using System;

namespace User.API.Models
{
    /// <summary>
    /// 用户标签
    /// </summary>
    public class UserTag
    {
        public int UserId { get; set; }
        public string Tag { get; set; }
        public DateTime CreateTime { get; set; }
    }
}