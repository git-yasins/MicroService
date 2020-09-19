using System;

namespace Project.API.Applications.IntegrationEvents
{
    /// <summary>
    /// 创建项目集成事件
    /// </summary>
    public class ProjectCreatedIntegrationEvent
    {
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}