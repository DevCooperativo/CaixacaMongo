using CachacaShop.Domain.Cachacas;

namespace CachacaShop.Domain.Consultas;

public record FiltroCatalogo(
    string? Nome = null,
    TipoCachaca? Tipo = null,
    Madeira? Madeira = null,
    decimal? PrecoMinimo = null,
    decimal? PrecoMaximo = null,
    bool SomenteDisponiveis = false);

public record CachacaPorEstado(string Estado, string Produtor, string Cidade, string Cachaca,
    TipoCachaca Tipo, decimal Preco, int Estoque);

public record CachacaMaisVendida(string CachacaId, string Nome, int QuantidadeVendida, decimal Faturamento);

public record FaturamentoMensal(int Ano, int Mes, int QuantidadePedidos, decimal Faturamento, decimal TicketMedio);
