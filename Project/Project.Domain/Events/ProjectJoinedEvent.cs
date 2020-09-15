using MediatR;
using Project.Domain.AggregatesModel;
namespace Project.Domain.Events {
    /// <summary>
    /// 项目跟进者事件
    /// </summary>
    public class ProjectJoinedEvent : INotification {
        public ProjectContributor Contributor { get; set; }
    }
}