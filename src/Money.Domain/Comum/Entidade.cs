namespace Money.Domain.Comum;

/// <summary>Base das entidades do domínio. O Id é gerado na criação (GUID v7, ordenável por tempo).</summary>
public abstract class Entidade
{
    public Guid Id { get; protected set; } = Guid.CreateVersion7();
}
