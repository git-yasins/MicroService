using System;
using System.Threading;
using System.Threading.Tasks;

namespace Project.Domain.SeedWork {
    /// <summary>
    /// 持久化
    /// </summary>
    public interface IUnitOfWork : IDisposable {
        Task<int> SaveChangesAsync (CancellationToken cancellation = default (CancellationToken));
        Task<bool> SaveEntitiesAsync (CancellationToken cancellationToken = default (CancellationToken));
    }
}