// <copyright file="ContaServico.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;
using Money.Domain.Contas;
using Money.Services.Abstracoes;
using Money.Services.Comum;
using Money.Services.Lancamentos;

namespace Money.Services.Contas;

/// <summary>Casos de uso das contas: gerenciador, cadastro e encerramento. Conta nunca é excluída.</summary>
/// <param name="contas">Repositório de contas.</param>
/// <param name="instituicoes">Repositório de instituições financeiras.</param>
/// <param name="grupos">Repositório de grupos de contas.</param>
/// <param name="lancamentos">Repositório de lançamentos, para o saldo e a trava da moeda.</param>
/// <param name="unidadeDeTrabalho">Grava as alterações.</param>
public sealed class ContaServico(
    IRepositorio<Conta> contas,
    IRepositorio<InstituicaoFinanceira> instituicoes,
    IRepositorio<GrupoConta> grupos,
    ILancamentoRepositorio lancamentos,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    /// <summary>Lista os tipos de conta com nome e descrição.</summary>
    /// <returns>Os sete tipos, na ordem de exibição.</returns>
    public IReadOnlyList<TipoContaDto> ListarTipos()
    {
        return TiposConta.Todos;
    }

    /// <summary>Lista as moedas aceitas.</summary>
    /// <returns>As moedas do catálogo.</returns>
    public IReadOnlyList<MoedaDto> ListarMoedas()
    {
        return CatalogoMoedas.Todas.Select(ParaDto).ToList();
    }

    /// <summary>Monta o gerenciador: contas ordenadas por nome e total por moeda das abertas.</summary>
    /// <param name="incluirEncerradas">Inclui as contas encerradas na lista (nunca no total).</param>
    /// <param name="cancellationToken">Cancela a consulta.</param>
    /// <returns>Contas e totais.</returns>
    public async Task<GerenciadorContasDto> ObterGerenciadorAsync(bool incluirEncerradas, CancellationToken cancellationToken)
    {
        var dados = await this.CarregarAsync(cancellationToken);
        var lista = dados.Contas
            .Where(c => incluirEncerradas || !c.Encerrada)
            .OrderBy(c => c.Nome, StringComparer.InvariantCultureIgnoreCase)
            .Select(dados.ParaDto)
            .ToList();
        var totais = dados.Contas
            .Where(c => !c.Encerrada)
            .GroupBy(c => c.CodigoMoeda)
            .OrderBy(g => g.Key == CatalogoMoedas.CodigoPadrao ? 0 : 1)
            .ThenBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => new TotalMoedaDto(ParaDto(CatalogoMoedas.Obter(g.Key)), g.Sum(dados.Saldo)))
            .ToList();
        return new GerenciadorContasDto(lista, totais, dados.Contas.Count > 0);
    }

    /// <summary>Busca uma conta.</summary>
    /// <param name="id">Id da conta.</param>
    /// <param name="cancellationToken">Cancela a consulta.</param>
    /// <returns>A conta.</returns>
    public async Task<ContaDto> ObterAsync(Guid id, CancellationToken cancellationToken)
    {
        var dados = await this.CarregarAsync(cancellationToken);
        return dados.ParaDto(ObterDaLista(dados.Contas, id));
    }

    /// <summary>Lista as contas que podem ser caixa de um investimento na moeda indicada.</summary>
    /// <param name="codigoMoeda">Moeda do investimento.</param>
    /// <param name="cancellationToken">Cancela a consulta.</param>
    /// <returns>Contas abertas Corrente, Poupança ou Dinheiro na moeda, ordenadas por nome.</returns>
    public async Task<IReadOnlyList<ContaDto>> ListarCaixasCompativeisAsync(string codigoMoeda, CancellationToken cancellationToken)
    {
        var codigo = CatalogoMoedas.Obter(codigoMoeda).Codigo;
        var dados = await this.CarregarAsync(cancellationToken);
        return dados.Contas
            .Where(c => !c.Encerrada && c.Tipo.PodeSerCaixa() && c.CodigoMoeda == codigo)
            .OrderBy(c => c.Nome, StringComparer.InvariantCultureIgnoreCase)
            .Select(dados.ParaDto)
            .ToList();
    }

    /// <summary>
    /// Confere, antes de criar a conta, se o nome pode ser usado: as mesmas regras de nome da criação,
    /// contando as contas encerradas.
    /// </summary>
    /// <param name="nome">Nome pretendido.</param>
    /// <param name="cancellationToken">Cancela a consulta.</param>
    /// <returns>Tarefa concluída se o nome for aceito; recusa com <see cref="RegraNegocioException"/>.</returns>
    public async Task ConferirNomeAsync(string? nome, CancellationToken cancellationToken)
    {
        var limpo = EntidadeNomeada.ValidarNome(nome);
        Nomes.GarantirUnico(await contas.ListarAsync(cancellationToken), limpo, null, "uma conta");
    }

    /// <summary>Cria uma conta e, se pedido, a conta caixa do investimento.</summary>
    /// <param name="dados">Dados do formulário.</param>
    /// <param name="cancellationToken">Cancela a gravação.</param>
    /// <returns>A conta criada.</returns>
    public async Task<ContaDto> CriarAsync(SalvarContaDto dados, CancellationToken cancellationToken)
    {
        var todas = (await contas.ListarAsync(cancellationToken)).ToList();
        Nomes.GarantirUnico(todas, dados.Nome, null, "uma conta");

        var conta = new Conta(dados.Tipo, dados.Nome, dados.CodigoMoeda, dados.SaldoInformado);
        await this.AplicarCamposAsync(conta, dados, cancellationToken);
        contas.Adicionar(conta);
        todas.Add(conta);
        this.AplicarCaixa(conta, dados, todas);

        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);
        return await this.ObterAsync(conta.Id, cancellationToken);
    }

    /// <summary>Altera uma conta. O tipo não muda; a moeda não muda se a conta estiver vinculada.</summary>
    /// <param name="id">Id da conta.</param>
    /// <param name="dados">Dados do formulário.</param>
    /// <param name="cancellationToken">Cancela a gravação.</param>
    /// <returns>A conta alterada.</returns>
    public async Task<ContaDto> AtualizarAsync(Guid id, SalvarContaDto dados, CancellationToken cancellationToken)
    {
        var todas = (await contas.ListarAsync(cancellationToken)).ToList();
        var conta = ObterDaLista(todas, id);
        if (dados.Tipo != conta.Tipo)
        {
            throw new RegraNegocioException("O tipo da conta não pode ser alterado.");
        }

        Nomes.GarantirUnico(todas, dados.Nome, id, "uma conta");
        var investimento = todas.FirstOrDefault(c => c.ContaCaixaId == id);
        var trocaMoeda = CatalogoMoedas.Obter(dados.CodigoMoeda).Codigo != conta.CodigoMoeda;
        if (investimento is not null && trocaMoeda)
        {
            throw new RegraNegocioException(
                $"Esta conta é caixa do investimento '{investimento.Nome}'; desvincule antes de trocar a moeda.");
        }

        if (trocaMoeda && (await lancamentos.ListarDaContaAsync(id, cancellationToken)).Count > 0)
        {
            throw new RegraNegocioException("A moeda não muda depois do primeiro lançamento da conta.");
        }

        conta.Renomear(dados.Nome);

        // O vínculo é refeito depois da moeda, que só pode mudar sem caixa vinculada.
        conta.DesvincularCaixa();
        conta.DefinirMoedaESaldo(dados.CodigoMoeda, dados.SaldoInformado);
        await this.AplicarCamposAsync(conta, dados, cancellationToken);
        this.AplicarCaixa(conta, dados, todas);

        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);
        return await this.ObterAsync(id, cancellationToken);
    }

    /// <summary>Marca ou desmarca a conta como favorita.</summary>
    /// <param name="id">Id da conta.</param>
    /// <param name="favorita">Novo estado.</param>
    /// <param name="cancellationToken">Cancela a gravação.</param>
    /// <returns>Tarefa concluída após gravar.</returns>
    public async Task DefinirFavoritaAsync(Guid id, bool favorita, CancellationToken cancellationToken)
    {
        var conta = await this.ObterEntidadeAsync(id, cancellationToken);
        conta.DefinirFavorita(favorita);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);
    }

    /// <summary>Encerra a conta. Caixa de investimento aberto não pode ser encerrada.</summary>
    /// <param name="id">Id da conta.</param>
    /// <param name="cancellationToken">Cancela a gravação.</param>
    /// <returns>Tarefa concluída após gravar.</returns>
    public async Task EncerrarAsync(Guid id, CancellationToken cancellationToken)
    {
        var todas = await contas.ListarAsync(cancellationToken);
        var conta = ObterDaLista(todas, id);
        var investimento = todas.FirstOrDefault(c => c.ContaCaixaId == id && !c.Encerrada);
        if (investimento is not null)
        {
            throw new RegraNegocioException(
                $"Esta conta é caixa do investimento '{investimento.Nome}'; encerre o investimento antes.");
        }

        conta.Encerrar();
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);
    }

    /// <summary>Reabre uma conta encerrada.</summary>
    /// <param name="id">Id da conta.</param>
    /// <param name="cancellationToken">Cancela a gravação.</param>
    /// <returns>Tarefa concluída após gravar.</returns>
    public async Task ReabrirAsync(Guid id, CancellationToken cancellationToken)
    {
        var conta = await this.ObterEntidadeAsync(id, cancellationToken);
        conta.Reabrir();
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);
    }

    /// <summary>Converte a moeda do catálogo no DTO.</summary>
    /// <param name="moeda">Moeda do catálogo.</param>
    /// <returns>O DTO da moeda.</returns>
    internal static MoedaDto ParaDto(Moeda moeda)
    {
        return new MoedaDto(moeda.Codigo, moeda.Nome, moeda.Simbolo, moeda.CasasDecimais);
    }

    private static Conta ObterDaLista(IEnumerable<Conta> todas, Guid id)
    {
        return todas.FirstOrDefault(c => c.Id == id)
            ?? throw new RegraNegocioException("Conta não encontrada.");
    }

    private async Task AplicarCamposAsync(Conta conta, SalvarContaDto dados, CancellationToken cancellationToken)
    {
        conta.DefinirInstituicao(await Nomes.ResolverPorNomeAsync(
            instituicoes, dados.NomeInstituicao, n => new InstituicaoFinanceira(n), cancellationToken));
        conta.DefinirGrupo(await Nomes.ResolverPorNomeAsync(
            grupos, dados.NomeGrupo, n => new GrupoConta(n), cancellationToken));
        conta.DefinirFavorita(dados.Favorita);
        conta.DefinirComplementos(dados.NumeroConta, dados.Abreviacao, dados.Comentario);

        // Depois da moeda, que já foi definida: as casas decimais são as da moeda nova.
        conta.DefinirSaldoMinimo(dados.SaldoMinimo);
    }

    private void AplicarCaixa(Conta conta, SalvarContaDto dados, List<Conta> todas)
    {
        switch (dados.ModoCaixa)
        {
            case ModoContaCaixa.Nenhuma:
                conta.DesvincularCaixa();
                break;
            case ModoContaCaixa.Existente:
                var caixa = todas.FirstOrDefault(c => c.Id == dados.ContaCaixaId)
                    ?? throw new RegraNegocioException("Escolha a conta caixa.");
                conta.VincularCaixa(caixa);
                break;
            case ModoContaCaixa.Nova:
                if (conta.Tipo != TipoConta.Investimento)
                {
                    throw new RegraNegocioException("Só conta de investimento tem conta caixa.");
                }

                var nome = string.IsNullOrWhiteSpace(dados.NomeCaixaNova) ? $"{conta.Nome} (Caixa)" : dados.NomeCaixaNova;
                Nomes.GarantirUnico(todas, nome, null, "uma conta");
                var nova = new Conta(TipoConta.Dinheiro, nome, conta.CodigoMoeda, dados.SaldoCaixaNova);
                nova.DefinirInstituicao(conta.InstituicaoFinanceiraId);
                contas.Adicionar(nova);
                todas.Add(nova);
                conta.VincularCaixa(nova);
                break;
            default:
                throw new RegraNegocioException("Modo de conta caixa inválido.");
        }
    }

    private async Task<Conta> ObterEntidadeAsync(Guid id, CancellationToken cancellationToken)
    {
        return await contas.ObterPorIdAsync(id, cancellationToken)
            ?? throw new RegraNegocioException("Conta não encontrada.");
    }

    private async Task<DadosContas> CarregarAsync(CancellationToken cancellationToken)
    {
        return new DadosContas(
            await contas.ListarAsync(cancellationToken),
            await instituicoes.ListarAsync(cancellationToken),
            await grupos.ListarAsync(cancellationToken),
            await lancamentos.SomarEfeitosPorContaAsync(cancellationToken));
    }

    /// <summary>Contas com os cadastros relacionados, para montar os DTOs sem consultas extras.</summary>
    /// <param name="Contas">Todas as contas.</param>
    /// <param name="Instituicoes">Todas as instituições.</param>
    /// <param name="Grupos">Todos os grupos.</param>
    /// <param name="EfeitosLancamentos">Soma dos lançamentos de cada conta, por Id.</param>
    private sealed record DadosContas(
        IReadOnlyList<Conta> Contas,
        IReadOnlyList<InstituicaoFinanceira> Instituicoes,
        IReadOnlyList<GrupoConta> Grupos,
        IReadOnlyDictionary<Guid, decimal> EfeitosLancamentos)
    {
        /// <summary>Saldo exibido: o saldo inicial mais os lançamentos não nulos.</summary>
        /// <param name="conta">Conta.</param>
        /// <returns>O saldo da conta.</returns>
        public decimal Saldo(Conta conta) => conta.SaldoInicial + this.EfeitosLancamentos.GetValueOrDefault(conta.Id);

        /// <summary>Monta o DTO da conta com os nomes relacionados.</summary>
        /// <param name="conta">Conta a converter.</param>
        /// <returns>O DTO.</returns>
        public ContaDto ParaDto(Conta conta)
        {
            return new ContaDto(
                conta.Id,
                conta.Nome,
                conta.Tipo,
                TiposConta.Obter(conta.Tipo).Nome,
                ContaServico.ParaDto(CatalogoMoedas.Obter(conta.CodigoMoeda)),
                this.Saldo(conta),
                conta.SaldoInformado,
                conta.InstituicaoFinanceiraId,
                this.Instituicoes.FirstOrDefault(i => i.Id == conta.InstituicaoFinanceiraId)?.Nome,
                conta.GrupoContaId,
                this.Grupos.FirstOrDefault(g => g.Id == conta.GrupoContaId)?.Nome,
                conta.ContaCaixaId,
                this.Contas.FirstOrDefault(c => c.Id == conta.ContaCaixaId)?.Nome,
                conta.Favorita,
                conta.Encerrada,
                conta.NumeroConta,
                conta.Abreviacao,
                conta.Comentario,
                conta.SaldoMinimo);
        }
    }
}
