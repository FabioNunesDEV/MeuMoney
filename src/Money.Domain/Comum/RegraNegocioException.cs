// <copyright file="RegraNegocioException.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Domain.Comum;

/// <summary>Regra de negócio violada. A mensagem vai ao usuário como está.</summary>
/// <param name="mensagem">Mensagem em pt-BR, pronta para exibir.</param>
public sealed class RegraNegocioException(string mensagem) : Exception(mensagem);
