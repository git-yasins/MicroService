using MediatR;
using Project.Domain.AggregatesModel;

namespace Project.API.Applications.Commands {
    public class JoinProjectCommand : IRequest<int> {
      public ProjectContributor ProjectContributor { get; set; }
    }
}