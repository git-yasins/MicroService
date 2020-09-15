namespace Project.Domain.SeedWork {
    /// <summary>
    /// 项目仓储接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T> where T : IAggregateRoot {
        IUnitOfWork UnitOfWork { get; }
    }
}