// <copyright file="IUnidadeDeTrabalho.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Services.Abstracoes;

/// <summary>Confirma as alterações pendentes na persistência, sem expor o provedor de banco.</summary>
public interface IUnidadeDeTrabalho
{
    /// <summary>Grava no banco tudo o que foi alterado desde a última gravação.</summary>
    /// <param name="cancellationToken">Cancela a gravação.</param>
    /// <returns>Quantidade de registros gravados.</returns>
    Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
