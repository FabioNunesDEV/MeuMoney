// <copyright file="IInicializadorBanco.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Services.Abstracoes;

/// <summary>Prepara o banco para uso. Chamado na inicialização do aplicativo.</summary>
public interface IInicializadorBanco
{
    /// <summary>Cria o banco, se não existir, e aplica as migrations pendentes.</summary>
    /// <param name="cancellationToken">Cancela a inicialização.</param>
    /// <returns>Tarefa concluída quando o banco estiver pronto.</returns>
    Task InicializarAsync(CancellationToken cancellationToken = default);
}
