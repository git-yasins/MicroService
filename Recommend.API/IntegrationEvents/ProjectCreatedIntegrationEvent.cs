using System;

namespace Recommend.API.IntegrationEvents {
    /// <summary>
    /// 订阅项目集成事件
    /// </summary>
    public class ProjectCreatedIntegrationEvent {
        /// <summary>
        /// 项目ID
        /// </summary>
        /// <value></value>
        public int ProjectId { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        /// <value></value>
        public int UserId { get; set; }
        /// <summary>
        /// 项目创建时间
        /// </summary>
        /// <value></value>
        public DateTime CreatedTime { get; set; }
        /// <summary>
        /// 项目LOGO
        /// </summary>
        /// <value></value>
        public string ProjectAvatar { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        /// <value></value>
        public string Company { get; set; }
        /// <summary>
        /// 项目介绍
        /// </summary>
        /// <value></value>
        public string Introduction { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        /// <value></value>
        public string Tags { get; set; }
        /// <summary>
        /// 融资阶段
        /// </summary>
        /// <value></value>
        public string FinStage { get; set; }
    }
}