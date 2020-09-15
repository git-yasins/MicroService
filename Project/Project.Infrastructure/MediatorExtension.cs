using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using MediatR;
using Project.Domain.SeedWork;

namespace Project.Infrastructure {
    /// <summary>
    /// 领域事件消息发布
    /// </summary>
    public static class MediatorExtension {
        public static async Task DispatchDomainEventAsync (this IMediator mediator, ProjectContext ctx) {
            //EF绑定领域事件集合对象，通过绑定跟踪变更
            var domainEntites = ctx.ChangeTracker.Entries<Entity> ().Where (x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Count () > 0);
            //获取EF的领域消息发布事件列表
            var domainEvents = domainEntites.SelectMany (x => x.Entity.DomainEvents).ToList ();
            //清除所有事件
            domainEntites.ToList ().ForEach (entity => entity.Entity.ClearDomainEvents ());
            //mediator发布事件
            var tasks = domainEvents.Select (async (domainEvent) => {
                await mediator.Publish (domainEvent);
            });
            //等待TASK集合列表的事件全部完成
            await Task.WhenAll (tasks);
        }
    }
}