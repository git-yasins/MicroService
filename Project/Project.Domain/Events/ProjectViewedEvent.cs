using MediatR;
using Project.Domain.AggregatesModel;

namespace Project.Domain.Events {
    public class ProjectViewedEvent : INotification {

        public string Company { get; set; }
        public string Introduction { get; set; }
        public ProjectViewer ProjectViewer { get; set; }
    }
}