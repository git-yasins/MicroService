using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Project.Domain.AggregatesModel;

namespace Project.API.Applications.Commands {
    /// <summary>
    /// 查看项目
    /// </summary>
    public class ViewProjectCommandHandler : IRequestHandler<ViewProjectCommand, int> {
        private readonly IProjectRepository projectRepository;

        public ViewProjectCommandHandler (IProjectRepository projectRepository) {
            this.projectRepository = projectRepository;
        }

        public async Task<int> Handle (ViewProjectCommand request, CancellationToken cancellationToken) {
            var project = await projectRepository.GetAsync (request.ProjectId);
            if (project == null) {
                throw new Domain.Exceptions.ProjectDomainException ($"project not found:{request.ProjectId}");
            }
            //加入查看者
            project.AddViewer (request.UserId, request.UserName, request.Avatar);
            await projectRepository.UnitOfWork.SaveEntitiesAsync ();
            return request.ProjectId;
        }
    }
}