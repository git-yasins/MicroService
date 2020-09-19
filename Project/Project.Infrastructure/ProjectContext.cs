using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Project.Domain.SeedWork;
using Project.Infrastructure.EntityConfigration;
using ProjectModel = Project.Domain.AggregatesModel.Project;
namespace Project.Infrastructure
{
    public class ProjectContext : DbContext, IUnitOfWork {
        private readonly IMediator mediator;
        public DbSet<ProjectModel> Projects { get; set; }
        /// <summary>
        /// 初始EF上下文
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public ProjectContext (DbContextOptions<ProjectContext> options, IMediator mediator) : base (options) {
            this.mediator = mediator;
        }

        /// <summary>
        /// 项目数据持久化
        /// </summary>
        /// <param name="cancellationToken">取消任务</param>
        /// <returns></returns>
        public async Task<bool> SaveEntitiesAsync (CancellationToken cancellationToken = default (CancellationToken)) {
            await mediator.DispatchDomainEventAsync (this);
            await base.SaveChangesAsync ();
            return true;
        }

        protected override void OnModelCreating (ModelBuilder modelBuilder) {
            modelBuilder.ApplyConfiguration (new ProjectEntityConfiguration ());
            modelBuilder.ApplyConfiguration (new ProjectContributorEntityConfiguration ());
            modelBuilder.ApplyConfiguration (new ProjectPropertyEntityConfiguration ());
            modelBuilder.ApplyConfiguration (new ProjectViewerEntityConfiguration ());
            modelBuilder.ApplyConfiguration (new ProjectVisibleRuleEntityConfiguration ());
        }
    }
}