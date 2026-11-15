// <copyright file="ContaConfiguracao.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Money.Domain.Comum;
using Money.Domain.Contas;

namespace Money.Data.Contas;

/// <summary>Mapeamento da conta para a tabela Contas.</summary>
internal sealed class ContaConfiguracao : IEntityTypeConfiguration<Conta>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Conta> builder)
    {
        builder.ToTable("Contas");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.Nome).IsRequired().HasMaxLength(EntidadeNomeada.TamanhoMaximoNome);

        // Texto em vez de número: legível no banco e imune à reordenação do enum.
        builder.Property(c => c.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.CodigoMoeda).IsRequired().HasMaxLength(3);
        builder.Property(c => c.SaldoInicial).HasPrecision(18, 2);

        builder.HasOne<InstituicaoFinanceira>()
            .WithMany()
            .HasForeignKey(c => c.InstituicaoFinanceiraId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GrupoConta>()
            .WithMany()
            .HasForeignKey(c => c.GrupoContaId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<Conta>()
            .WithMany()
            .HasForeignKey(c => c.ContaCaixaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
