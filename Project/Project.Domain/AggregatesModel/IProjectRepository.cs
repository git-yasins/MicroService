using System.Threading.Tasks;
using Project.Domain.SeedWork;

namespace Project.Domain.AggregatesModel {
    /// <summary>
    /// 项目聚合的操作
    /// </summary>
    public interface IProjectRepository : IRepository<Project> {
        Project Add (Project project);
        void Update (Project project);
        Task<Project> GetAsync (int id);
    }
}