using MediatR;
namespace Project.Domain.Events {
    /// <summary>
    /// 创建项目事件
    /// </summary>
    public class ProjectCreatedEvent : INotification {
        public Project.Domain.AggregatesModel.Project Project { get; set; }
    }
}