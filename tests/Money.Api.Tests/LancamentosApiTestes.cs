// <copyright file="LancamentosApiTestes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using System.Net;
using System.Net.Http.Json;
using Money.Api.Comum;
using Money.Domain.Contas;
using Money.Services.Contas;
using Money.Services.Lancamentos;

namespace Money.Api.Tests;

/// <summary>Testes da API de lançamentos de ponta a ponta: HTTP, serviços e SQLite temporário.</summary>
public sealed class LancamentosApiTestes : TesteDeApi
{
    private static readonly DateOnly Dia = new(2026, 11, 22);

    /// <summary>Gasto, recebimento e transferência aparecem na movimentação e no saldo do gerenciador.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task Lancar_GastoRecebimentoETransferencia_RefleteNaMovimentacaoENoGerenciador()
    {
        var carteira = await this.CriarContaAsync("Carteira", 100m);
        var cofre = await this.CriarContaAsync("Cofre", 0m);

        await this.LancarAsync(Gasto(carteira, 30m));
        await this.LancarAsync(Gasto(carteira, 50m) with { Natureza = NaturezaLancamento.Recebimento, NomeFavorecido = "Salário", NomeCategoria = "Renda" });
        await this.LancarAsync(Transferencia(carteira, cofre, 40m));

        var movimentacao = await this.Cliente.GetFromJsonAsync<MovimentacaoDto>($"/api/contas/{carteira}/lancamentos", Json);
        Assert.NotNull(movimentacao);
        Assert.Equal([70m, 120m, 80m], movimentacao.Lancamentos.Select(l => l.SaldoApos));
        Assert.Equal("Cofre", movimentacao.Lancamentos[2].NomeOutraConta);

        var gerenciador = await this.Cliente.GetFromJsonAsync<GerenciadorContasDto>("/api/contas", Json);
        Assert.NotNull(gerenciador);
        Assert.Equal(80m, gerenciador.Contas.Single(c => c.Nome == "Carteira").Saldo);
        Assert.Equal(40m, gerenciador.Contas.Single(c => c.Nome == "Cofre").Saldo);
        Assert.Equal(120m, Assert.Single(gerenciador.Totais).Total);
    }

    /// <summary>Sem confirmação, o gasto que deixa a conta negativa volta sem gravar; confirmado, grava.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task Lancar_SaldoNegativo_PedeConfirmacaoAntesDeGravar()
    {
        var carteira = await this.CriarContaAsync("Carteira", 20m);

        var semConfirmar = await this.LancarAsync(Gasto(carteira, 50m));
        Assert.False(semConfirmar.Gravado);
        Assert.Equal(-30m, Assert.Single(semConfirmar.SaldosNegativos).SaldoFinal);
        Assert.Equal(20m, await this.SaldoAsync(carteira));

        var confirmado = await this.LancarAsync(Gasto(carteira, 50m) with { ConfirmarSaldoNegativo = true });
        Assert.True(confirmado.Gravado);
        Assert.Equal(-30m, await this.SaldoAsync(carteira));
    }

    /// <summary>Montante negativo volta como 400 com a mensagem para exibir.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task Lancar_MontanteNegativo_Devolve400ComMensagem()
    {
        var carteira = await this.CriarContaAsync("Carteira", 20m);

        var resposta = await this.Cliente.PostAsJsonAsync("/api/lancamentos", Gasto(carteira, -10m), Json);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var erro = await resposta.Content.ReadFromJsonAsync<ErroDto>(Json);
        Assert.Equal("O montante deve ser maior que zero.", erro?.Mensagem);
    }

    /// <summary>Excluir uma ponta da transferência a tira das duas contas.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task Excluir_Transferencia_SaiDasDuasContas()
    {
        var carteira = await this.CriarContaAsync("Carteira", 100m);
        var cofre = await this.CriarContaAsync("Cofre", 0m);
        await this.LancarAsync(Transferencia(carteira, cofre, 40m));
        var noCofre = await this.Cliente.GetFromJsonAsync<MovimentacaoDto>($"/api/contas/{cofre}/lancamentos", Json);

        (await this.Cliente.DeleteAsync($"/api/lancamentos/{noCofre?.Lancamentos.Single().Id}")).EnsureSuccessStatusCode();

        Assert.Equal(100m, await this.SaldoAsync(carteira));
        Assert.Equal(0m, await this.SaldoAsync(cofre));
        var naCarteira = await this.Cliente.GetFromJsonAsync<MovimentacaoDto>($"/api/contas/{carteira}/lancamentos", Json);
        Assert.NotNull(naCarteira);
        Assert.Empty(naCarteira.Lancamentos);
    }

    private static SalvarLancamentoDto Gasto(Guid conta, decimal valor) =>
        new(NaturezaLancamento.Gasto, conta, null, Dia, valor, "Padaria", "Alimentação", null, null, false);

    private static SalvarLancamentoDto Transferencia(Guid de, Guid para, decimal valor) =>
        new(NaturezaLancamento.Transferencia, de, para, Dia, valor, "Transferência", null, null, null, false);

    private async Task<Guid> CriarContaAsync(string nome, decimal saldo)
    {
        var dados = new SalvarContaDto { Tipo = TipoConta.Dinheiro, Nome = nome, SaldoInformado = saldo };
        var resposta = await this.Cliente.PostAsJsonAsync("/api/contas", dados, Json);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<ContaDto>(Json))?.Id ?? throw new InvalidOperationException("Conta sem Id.");
    }

    private async Task<ResultadoLancamentoDto> LancarAsync(SalvarLancamentoDto dados)
    {
        var resposta = await this.Cliente.PostAsJsonAsync("/api/lancamentos", dados, Json);
        resposta.EnsureSuccessStatusCode();
        return await resposta.Content.ReadFromJsonAsync<ResultadoLancamentoDto>(Json)
            ?? throw new InvalidOperationException("Resposta vazia.");
    }

    private async Task<decimal> SaldoAsync(Guid conta)
    {
        return (await this.Cliente.GetFromJsonAsync<ContaDto>($"/api/contas/{conta}", Json))?.Saldo
            ?? throw new InvalidOperationException("Conta não encontrada.");
    }
}
