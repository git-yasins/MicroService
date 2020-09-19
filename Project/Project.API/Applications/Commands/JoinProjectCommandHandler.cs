using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Project.Domain.AggregatesModel;

namespace Project.API.Applications.Commands {
    /// <summary>
    /// 加入项目
    /// </summary>
    public class JoinProjectCommandHandler : IRequestHandler<JoinProjectCommand, int> {
        private readonly IProjectRepository projectRepository;

        public JoinProjectCommandHandler (IProjectRepository projectRepository) {
            this.projectRepository = projectRepository;
        }

        public async Task<int> Handle (JoinProjectCommand request, CancellationToken cancellationToken) {
            var project = await projectRepository.GetAsync (request.ProjectContributor.ProjectId);
            if (project == null) {
                throw new Domain.Exceptions.ProjectDomainException ($"project not found:{request.ProjectContributor.ProjectId}");
            }
            //增加参与者
            project.AddContributor (request.ProjectContributor);
            await projectRepository.UnitOfWork.SaveEntitiesAsync ();
            return request.ProjectContributor.ProjectId;
        }
    }
}