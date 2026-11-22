// <copyright file="FavorecidoConfiguracao.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Money.Domain.Comum;
using Money.Domain.Lancamentos;

namespace Money.Data.Lancamentos;

/// <summary>Mapeamento do favorecido para a tabela Favorecidos.</summary>
internal sealed class FavorecidoConfiguracao : IEntityTypeConfiguration<Favorecido>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Favorecido> builder)
    {
        builder.ToTable("Favorecidos");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Nome).IsRequired().HasMaxLength(EntidadeNomeada.TamanhoMaximoNome);
    }
}
