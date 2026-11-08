namespace Money.Services.Abstractions;

/// <summary>Confirma as alterações pendentes na persistência, sem expor o provedor de banco.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
