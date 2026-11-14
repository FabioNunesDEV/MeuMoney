// <copyright file="Entidade.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Domain.Comum;

/// <summary>Base das entidades do domínio. O Id é gerado na criação (GUID v7, ordenável por tempo).</summary>
public abstract class Entidade
{
    /// <summary>Obtém ou define o identificador da entidade.</summary>
    public Guid Id { get; protected set; } = Guid.CreateVersion7();
}
