using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mimo.Application.Repositories
{
    public interface IUnitOfWork
    {
        Task<IAsyncDisposable> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
