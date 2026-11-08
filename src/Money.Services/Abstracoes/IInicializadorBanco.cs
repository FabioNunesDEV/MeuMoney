namespace Money.Services.Abstracoes;

/// <summary>Prepara o banco para uso. Chamado na inicialização do aplicativo.</summary>
public interface IInicializadorBanco
{
    /// <summary>Cria o banco, se não existir, e aplica as migrations pendentes.</summary>
    /// <param name="cancellationToken">Cancela a inicialização.</param>
    Task InicializarAsync(CancellationToken cancellationToken = default);
}
