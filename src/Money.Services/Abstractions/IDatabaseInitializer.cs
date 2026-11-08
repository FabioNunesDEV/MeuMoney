namespace Money.Services.Abstractions;

/// <summary>Prepara o banco para uso (cria e aplica migrations). Chamado na inicialização do aplicativo.</summary>
public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
