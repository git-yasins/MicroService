using Project.Domain.AggregatesModel;

namespace Project.API.Applications.IntegrationEvents {
    /// <summary>
    /// 查看项目的集成事件
    /// </summary>
    public class ProjectViewedIntegrationEvent {
        public string Company { get; set; }
        public string Introduction { get; set; }
        public ProjectViewer Viewer { get; set; }
    }
}