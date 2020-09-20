namespace Recommend.API.IntegrationEvents {
    /// <summary>
    /// 参与项目的集成事件
    /// </summary>
    public class ProjectJoinedIntegrationEvent {
        public string Company { get; set; }
        public string Introduction { get; set; }
        public string Avatar { get; set; }
        //public ProjectContributor MyProperty { get; set; }
    }
}