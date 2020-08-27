namespace User.API.Models
{
    /// <summary>
    /// 用户属性
    /// </summary>
    public class UserProperty
    {
        public int AppUserId { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
    }
}