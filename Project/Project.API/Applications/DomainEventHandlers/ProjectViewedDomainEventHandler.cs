using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using MediatR;
using Project.API.Applications.IntegrationEvents;
using Project.Domain.Events;
namespace Project.API.Applications.DomainEventHandlers {
    /// <summary>
    /// 查看项目的领域事件接收
    /// </summary>
    public class ProjectViewedDomainEventHandler : INotificationHandler<ProjectViewedEvent> {
        private readonly ICapPublisher capPublisher;

        public ProjectViewedDomainEventHandler (ICapPublisher capPublisher) {
            this.capPublisher = capPublisher;
        }
        public Task Handle (ProjectViewedEvent notification, CancellationToken cancellationToken) {
            var @event = new ProjectViewedIntegrationEvent {
                Introduction = notification.Introduction,
                Viewer = notification.ProjectViewer,
                Company = notification.Company
            };

            capPublisher.Publish ("finbook.projectapi.projectviewed", @event);
            return Task.CompletedTask;
        }
    }
}