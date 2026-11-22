// <copyright file="ContaDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Contas;

namespace Money.Services.Contas;

/// <summary>Conta pronta para exibir ou editar.</summary>
/// <param name="Id">Id da conta.</param>
/// <param name="Nome">Nome da conta.</param>
/// <param name="Tipo">Tipo da conta.</param>
/// <param name="NomeTipo">Nome exibido do tipo.</param>
/// <param name="Moeda">Moeda da conta.</param>
/// <param name="Saldo">Saldo com sinal (dívida negativa), para exibir e somar.</param>
/// <param name="SaldoInformado">Saldo como o usuário digita (dívida positiva), para o formulário.</param>
/// <param name="InstituicaoFinanceiraId">Id da instituição, se houver.</param>
/// <param name="NomeInstituicao">Nome da instituição, se houver.</param>
/// <param name="GrupoContaId">Id do grupo, se houver.</param>
/// <param name="NomeGrupo">Nome do grupo, se houver.</param>
/// <param name="ContaCaixaId">Id da conta caixa vinculada, se houver.</param>
/// <param name="NomeContaCaixa">Nome da conta caixa vinculada, se houver.</param>
/// <param name="Favorita">Indica se a conta é favorita.</param>
/// <param name="Encerrada">Indica se a conta está encerrada.</param>
/// <param name="NumeroConta">Número da conta, se informado.</param>
/// <param name="Abreviacao">Nome curto da conta, se informado.</param>
/// <param name="Comentario">Comentário livre, se informado.</param>
/// <param name="SaldoMinimo">Saldo mínimo, se informado.</param>
public sealed record ContaDto(
    Guid Id,
    string Nome,
    TipoConta Tipo,
    string NomeTipo,
    MoedaDto Moeda,
    decimal Saldo,
    decimal SaldoInformado,
    Guid? InstituicaoFinanceiraId,
    string? NomeInstituicao,
    Guid? GrupoContaId,
    string? NomeGrupo,
    Guid? ContaCaixaId,
    string? NomeContaCaixa,
    bool Favorita,
    bool Encerrada,
    string? NumeroConta,
    string? Abreviacao,
    string? Comentario,
    decimal? SaldoMinimo);
