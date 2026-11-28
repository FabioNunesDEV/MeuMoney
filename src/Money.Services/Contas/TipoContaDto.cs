// <copyright file="TipoContaDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Contas;

namespace Money.Services.Contas;

/// <summary>Tipo de conta com o texto exibido na escolha.</summary>
/// <param name="Tipo">Tipo da conta.</param>
/// <param name="Nome">Nome exibido.</param>
/// <param name="Descricao">Descrição curta do uso do tipo.</param>
public sealed record TipoContaDto(TipoConta Tipo, string Nome, string Descricao)
{
    /// <summary>Obtém um valor que indica se o tipo tem saldo mínimo, para a tela mostrar o campo.</summary>
    public bool TemSaldoMinimo => this.Tipo.TemSaldoMinimo();

    /// <summary>Obtém um valor que indica se o assistente de nova conta pergunta o número da conta.</summary>
    public bool PedeNumeroNaCriacao => this.Tipo.PedeNumeroNaCriacao();
}
