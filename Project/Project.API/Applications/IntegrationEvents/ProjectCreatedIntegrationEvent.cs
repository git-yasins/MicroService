using System;

namespace Project.API.Applications.IntegrationEvents {
    /// <summary>
    /// 创建项目集成事件
    /// </summary>
    public class ProjectCreatedIntegrationEvent {
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedTime { get; set; }
        /// <summary>
        /// 项目LOGO
        /// </summary>
        /// <value></value>
        public string ProjectAvatar { get; set; }
        public string Company { get; set; }
        /// <summary>
        /// 项目介绍
        /// </summary>
        /// <value></value>
        public string Introduction { get; set; }
        public string Tags { get; set; }
        /// <summary>
        /// 融资阶段
        /// </summary>
        /// <value></value>
        public string FinStage { get; set; }
    }
}