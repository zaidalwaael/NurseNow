namespace NurseNow.Application.Abstractions;

/// <summary>Commits tracked changes across the repositories in the current request scope.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    /// <summary>Starts a serializable transaction. Save changes before explicitly committing.</summary>
    Task<IApplicationTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

/// <summary>Disposing without CommitAsync rolls back the transaction.</summary>
public interface IApplicationTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
}
