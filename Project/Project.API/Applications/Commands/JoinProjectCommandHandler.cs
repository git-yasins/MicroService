using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Project.Domain.AggregatesModel;

namespace Project.API.Applications.Commands {
    /// <summary>
    /// 增加领域事件
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

            //参与者和当前项目所属用户相同时，不能参与该项目
            if (project.UserId == request.ProjectContributor.UserId) {
                throw new Domain.Exceptions.ProjectDomainException ($"不能参与自己创建的项目:{request.ProjectContributor.ProjectId}");
            }

            //增加参与者
            project.AddContributor (request.ProjectContributor);
            await projectRepository.UnitOfWork.SaveEntitiesAsync ();
            return request.ProjectContributor.ProjectId;
        }
    }
}