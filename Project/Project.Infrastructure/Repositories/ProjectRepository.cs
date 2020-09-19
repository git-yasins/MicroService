using System.Threading.Tasks;
using Project.Domain.AggregatesModel;
using Project.Domain.SeedWork;
using ProjectEntity = Project.Domain.AggregatesModel.Project;
using Microsoft.EntityFrameworkCore;

namespace Project.Infrastructure.Repositories {
    /// <summary>
    /// 项目仓储
    /// </summary>
    public class ProjectRepository : IProjectRepository {
        private readonly ProjectContext context;
        public IUnitOfWork UnitOfWork => context;
        public ProjectRepository (ProjectContext context) {
            this.context = context;
        }

        public ProjectEntity Add (ProjectEntity project) {
            if (project.IsTransient ()) {
                return context.Add (project).Entity;
            } else {
                return project;
            }
        }

        public async Task<ProjectEntity> GetAsync (int id) {
            return await context.Projects
                .Include (x => x.Properties)
                .Include (x => x.Viewers)
                .Include (x => x.Contributors)
                .Include (x => x.VisibleRule)
                .SingleOrDefaultAsync ();
        }

        public void Update (ProjectEntity project) {
            context.Update (project);
        }
    }
}