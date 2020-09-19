using System;

namespace Project.Domain.AggregatesModel
{
    /// <summary>
    /// 项目展示
    /// </summary>
    public class ProjectViewer
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Avatar { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}