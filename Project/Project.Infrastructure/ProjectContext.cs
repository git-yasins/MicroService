using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Project.Domain.SeedWork;

namespace Project.Infrastructure {
    public class ProjectContext : DbContext, IUnitOfWork {
        /// <summary>
        /// 初始EF上下文
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public ProjectContext (DbContextOptions<ProjectContext> options) : base (options) { }
        /// <summary>
        /// 项目数据持久化
        /// </summary>
        /// <param name="cancellationToken">取消任务</param>
        /// <returns></returns>
        public Task<bool> SaveEntitiesAsync (CancellationToken cancellationToken = default) {
            throw new System.NotImplementedException ();
        }
    }
}