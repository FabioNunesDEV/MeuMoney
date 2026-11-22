// <copyright file="SalvarContaDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Contas;

namespace Money.Services.Contas;

/// <summary>Dados do formulário de conta, para criar ou alterar.</summary>
public sealed record SalvarContaDto
{
    /// <summary>Obtém ou inicializa o tipo; numa alteração, deve ser o tipo atual.</summary>
    public required TipoConta Tipo { get; init; }

    /// <summary>Obtém ou inicializa o nome da conta.</summary>
    public required string Nome { get; init; }

    /// <summary>Obtém ou inicializa o código ISO 4217 da moeda.</summary>
    public string CodigoMoeda { get; init; } = CatalogoMoedas.CodigoPadrao;

    /// <summary>Obtém ou inicializa o saldo inicial como o usuário digita (dívida positiva).</summary>
    public decimal SaldoInformado { get; init; }

    /// <summary>Obtém ou inicializa o nome da instituição; existente é reaproveitada, nova é criada.</summary>
    public string? NomeInstituicao { get; init; }

    /// <summary>Obtém ou inicializa o nome do grupo; existente é reaproveitado, novo é criado.</summary>
    public string? NomeGrupo { get; init; }

    /// <summary>Obtém ou inicializa um valor que indica se a conta é favorita.</summary>
    public bool Favorita { get; init; }

    /// <summary>Obtém ou inicializa o número da conta, opcional.</summary>
    public string? NumeroConta { get; init; }

    /// <summary>Obtém ou inicializa o nome curto da conta, opcional.</summary>
    public string? Abreviacao { get; init; }

    /// <summary>Obtém ou inicializa o comentário livre, opcional.</summary>
    public string? Comentario { get; init; }

    /// <summary>Obtém ou inicializa o saldo mínimo; só nos tipos que o têm.</summary>
    public decimal? SaldoMinimo { get; init; }

    /// <summary>Obtém ou inicializa o modo da conta caixa; só vale para investimento.</summary>
    public ModoContaCaixa ModoCaixa { get; init; }

    /// <summary>Obtém ou inicializa a conta caixa escolhida, no modo <see cref="ModoContaCaixa.Existente"/>.</summary>
    public Guid? ContaCaixaId { get; init; }

    /// <summary>Obtém ou inicializa o nome da caixa nova; vazio usa "&lt;nome&gt; (Caixa)".</summary>
    public string? NomeCaixaNova { get; init; }

    /// <summary>Obtém ou inicializa o saldo inicial da caixa nova.</summary>
    public decimal SaldoCaixaNova { get; init; }
}
