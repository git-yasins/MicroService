using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using MediatR;
using Project.API.Applications.IntegrationEvents;
using Project.Domain.Events;

namespace Project.API.Applications.DomainEventHandlers {
    /// <summary>
    /// 创建项目的领域事件接收
    /// </summary>
    public class ProjectCreatedDomainEventHandler : INotificationHandler<ProjectCreatedEvent> {
        private readonly ICapPublisher capPublisher;

        public ProjectCreatedDomainEventHandler (ICapPublisher capPublisher) {
            this.capPublisher = capPublisher;
        }
        /// <summary>
        /// CAP发送事件到EventBus
        /// </summary>
        /// <param name="notification">领域事件</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Handle (ProjectCreatedEvent notification, CancellationToken cancellationToken) {
            var @event = new ProjectCreatedIntegrationEvent {
                ProjectId = notification.Project.Id,
                CreatedTime = DateTime.Now,
                UserId = notification.Project.UserId
            };

            capPublisher.Publish ("finbook.projectapi.projectcreated", @event);
            return Task.CompletedTask;
        }
    }
}