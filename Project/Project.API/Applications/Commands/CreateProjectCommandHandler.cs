using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Project.Domain.AggregatesModel;
using ProjectModel = Project.Domain.AggregatesModel.Project;
namespace Project.API.Applications.Commands {
    /// <summary>
    /// 创建项目
    /// </summary>
    public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommands, ProjectModel> {
        private readonly IProjectRepository projectRepository;

        public CreateProjectCommandHandler (IProjectRepository projectRepository) {
            this.projectRepository = projectRepository;
        }
        public async Task<ProjectModel> Handle (CreateProjectCommands request, CancellationToken cancellationToken) {
            ProjectModel project = projectRepository.Add (request.Project);
            await projectRepository.UnitOfWork.SaveEntitiesAsync ();
            return project;
        }
    }
}