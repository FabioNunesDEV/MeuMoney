// <copyright file="ContasApiTestes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using System.Net;
using System.Net.Http.Json;
using Money.Api.Comum;
using Money.Domain.Contas;
using Money.Services.Contas;

namespace Money.Api.Tests;

/// <summary>Testes da API de contas de ponta a ponta: HTTP, serviços e SQLite temporário.</summary>
public sealed class ContasApiTestes : TesteDeApi
{
    /// <summary>As opções trazem os sete tipos, com o enum como texto, e as moedas.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ObterOpcoes_DevolveTiposComoTextoEMoedas()
    {
        var texto = await this.Cliente.GetStringAsync("/api/contas/opcoes");

        Assert.Contains("\"tipo\":\"CartaoCredito\"", texto);
        Assert.Contains("\"codigo\":\"USD\"", texto);
    }

    /// <summary>Conta criada pela API aparece no gerenciador com o total da moeda.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriarConta_ApareceNoGerenciadorComTotal()
    {
        var resposta = await this.Cliente.PostAsJsonAsync("/api/contas", Dados("Conta principal", saldo: 100.5m), Json);
        resposta.EnsureSuccessStatusCode();

        var gerenciador = await this.Cliente.GetFromJsonAsync<GerenciadorContasDto>("/api/contas", Json);

        Assert.NotNull(gerenciador);
        var conta = Assert.Single(gerenciador.Contas);
        Assert.Equal("Conta principal", conta.Nome);
        Assert.Equal(100.5m, Assert.Single(gerenciador.Totais).Total);

        var sozinha = await this.Cliente.GetFromJsonAsync<ContaDto>($"/api/contas/{conta.Id}", Json);
        Assert.Equal(conta, sozinha);
    }

    /// <summary>Regra violada volta como 400 com a mensagem para exibir.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriarConta_NomeRepetido_Devolve400ComMensagem()
    {
        (await this.Cliente.PostAsJsonAsync("/api/contas", Dados("Conta"), Json)).EnsureSuccessStatusCode();

        var resposta = await this.Cliente.PostAsJsonAsync("/api/contas", Dados("conta"), Json);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var erro = await resposta.Content.ReadFromJsonAsync<ErroDto>(Json);
        Assert.Contains("Já existe", erro?.Mensagem);
    }

    /// <summary>Favoritar e encerrar pela API mudam o que o gerenciador devolve; excluir conta não existe.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task FavoritarEncerrar_RefletemNoGerenciador_EContaNaoSeExclui()
    {
        var criada = await (await this.Cliente.PostAsJsonAsync("/api/contas", Dados("Conta"), Json))
            .Content.ReadFromJsonAsync<ContaDto>(Json);
        Assert.NotNull(criada);

        (await this.Cliente.PutAsJsonAsync($"/api/contas/{criada.Id}/favorita", new { favorita = true }, Json)).EnsureSuccessStatusCode();
        var comFavorita = await this.Cliente.GetFromJsonAsync<GerenciadorContasDto>("/api/contas", Json);
        Assert.True(comFavorita?.Contas.Single().Favorita);

        (await this.Cliente.PostAsync($"/api/contas/{criada.Id}/encerrar", null)).EnsureSuccessStatusCode();
        var semEncerradas = await this.Cliente.GetFromJsonAsync<GerenciadorContasDto>("/api/contas", Json);
        var comEncerradas = await this.Cliente.GetFromJsonAsync<GerenciadorContasDto>("/api/contas?encerradas=true", Json);
        Assert.Empty(semEncerradas?.Contas ?? []);
        Assert.Single(comEncerradas?.Contas ?? []);

        var exclusao = await this.Cliente.DeleteAsync($"/api/contas/{criada.Id}");
        var depois = await this.Cliente.GetFromJsonAsync<GerenciadorContasDto>("/api/contas?encerradas=true", Json);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, exclusao.StatusCode);
        Assert.True(depois?.ExistemContas);
    }

    /// <summary>
    /// Alterar a conta grava número, abreviação, comentário e saldo mínimo, e a leitura os devolve; saldo
    /// mínimo em cartão de crédito volta como 400 com a mensagem.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task AlterarConta_Complementos_GravaEDevolve()
    {
        var criada = await (await this.Cliente.PostAsJsonAsync("/api/contas", Dados("Conta"), Json))
            .Content.ReadFromJsonAsync<ContaDto>(Json);
        Assert.NotNull(criada);
        var alteracao = Dados("Conta") with { NumeroConta = "1234-5", Abreviacao = "Cc", Comentario = "Salário", SaldoMinimo = -500m };

        (await this.Cliente.PutAsJsonAsync($"/api/contas/{criada.Id}", alteracao, Json)).EnsureSuccessStatusCode();
        var lida = await this.Cliente.GetFromJsonAsync<ContaDto>($"/api/contas/{criada.Id}", Json);

        Assert.NotNull(lida);
        Assert.Equal("1234-5", lida.NumeroConta);
        Assert.Equal("Cc", lida.Abreviacao);
        Assert.Equal("Salário", lida.Comentario);
        Assert.Equal(-500m, lida.SaldoMinimo);

        var cartao = new SalvarContaDto { Tipo = TipoConta.CartaoCredito, Nome = "Cartão", SaldoMinimo = 100m };
        var recusa = await this.Cliente.PostAsJsonAsync("/api/contas", cartao, Json);
        Assert.Equal(HttpStatusCode.BadRequest, recusa.StatusCode);
        Assert.Equal("Este tipo de conta não tem saldo mínimo.", (await recusa.Content.ReadFromJsonAsync<ErroDto>(Json))?.Mensagem);

        var opcoes = await this.Cliente.GetStringAsync("/api/contas/opcoes");
        Assert.Contains("\"temSaldoMinimo\":true", opcoes);
        Assert.Contains("\"temSaldoMinimo\":false", opcoes);
    }

    /// <summary>Instituição criada com a conta é listada, renomeada e não excluída enquanto em uso.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task Instituicoes_ListarRenomearEBloquearExclusaoEmUso()
    {
        (await this.Cliente.PostAsJsonAsync("/api/contas", Dados("Conta") with { NomeInstituicao = "Banco A" }, Json)).EnsureSuccessStatusCode();
        var instituicao = Assert.Single(await this.Cliente.GetFromJsonAsync<List<CadastroSimplesDto>>("/api/instituicoes", Json) ?? []);

        (await this.Cliente.PutAsJsonAsync($"/api/instituicoes/{instituicao.Id}", new { nome = "Banco A S.A." }, Json)).EnsureSuccessStatusCode();
        var exclusao = await this.Cliente.DeleteAsync($"/api/instituicoes/{instituicao.Id}");

        var gerenciador = await this.Cliente.GetFromJsonAsync<GerenciadorContasDto>("/api/contas", Json);
        Assert.Equal("Banco A S.A.", gerenciador?.Contas.Single().NomeInstituicao);
        Assert.Equal(HttpStatusCode.BadRequest, exclusao.StatusCode);
    }

    private static SalvarContaDto Dados(string nome, decimal saldo = 0m)
    {
        return new SalvarContaDto { Tipo = TipoConta.Corrente, Nome = nome, SaldoInformado = saldo };
    }
}
