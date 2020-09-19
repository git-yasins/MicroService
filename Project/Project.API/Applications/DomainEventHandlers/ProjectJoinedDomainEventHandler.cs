using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using MediatR;
using Project.API.Applications.IntegrationEvents;
using Project.Domain.Events;
namespace Project.API.Applications.DomainEventHandlers {
    /// <summary>
    /// 参与项目的领域事件接收
    /// </summary>
    public class ProjectJoinedDomainEventHandler : INotificationHandler<ProjectJoinedEvent> {
        private readonly ICapPublisher capPublisher;

        public ProjectJoinedDomainEventHandler (ICapPublisher capPublisher) {
            this.capPublisher = capPublisher;
        }
        public Task Handle (ProjectJoinedEvent notification, CancellationToken cancellationToken) {
            var @event = new ProjectJoinedIntegrationEvent {
                Introduction = notification.Introduction,
                Avatar = notification.Avatar,
                MyProperty = notification.Contributor
            };

            capPublisher.Publish ("finbook.projectapi.projectJoined", @event);
            return Task.CompletedTask;
        }
    }
}