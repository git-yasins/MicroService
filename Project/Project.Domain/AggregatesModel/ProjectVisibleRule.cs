using Project.Domain.SeedWork;

namespace Project.Domain.AggregatesModel {
    /// <summary>
    /// 项目展示权限
    /// </summary>
    public class ProjectVisibleRule : Entity {
        public int ProjectId { get; set; }
        public bool Visible { get; set; }
        public string Tags { get; set; }
    }
}