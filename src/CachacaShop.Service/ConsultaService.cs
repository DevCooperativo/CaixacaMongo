using CachacaShop.Domain;
using CachacaShop.Domain.Cachacas;
using CachacaShop.Domain.Consultas;
using CachacaShop.Domain.Pedidos;

namespace CachacaShop.Service;

/// <summary>Buscas de negócio exigidas pelo trabalho (recuperação de informações).</summary>
public class ConsultaService(ICachacaRepository cachacas, IPedidoRepository pedidos)
{
    /// <summary>1. Catálogo: nome (texto parcial), tipo, madeira, faixa de preço e disponibilidade.</summary>
    public Task<List<Cachaca>> CatalogoAsync(FiltroCatalogo filtro) => cachacas.BuscarCatalogoAsync(filtro);

    /// <summary>2. Cachaças produzidas em um estado (junção cachacas x produtores com $lookup).</summary>
    public Task<List<CachacaPorEstado>> PorEstadoAsync(string estado) =>
        cachacas.BuscarPorEstadoDoProdutorAsync(estado);

    /// <summary>3. Produtos com estoque baixo (para reposição).</summary>
    public Task<List<Cachaca>> EstoqueBaixoAsync(int limite) => cachacas.BuscarEstoqueBaixoAsync(limite);

    /// <summary>4. Histórico de pedidos de um cliente, por status e período.</summary>
    public Task<List<Pedido>> PedidosDoClienteAsync(string clienteId, StatusPedido? status, DateTime? de,
        DateTime? ate) => pedidos.BuscarPorClienteAsync(clienteId, status, de, ate);

    /// <summary>5. Ranking das cachaças mais vendidas (agregação com $unwind/$group).</summary>
    public Task<List<CachacaMaisVendida>> MaisVendidasAsync(int top) => pedidos.MaisVendidasAsync(top);

    /// <summary>6. Faturamento mês a mês de um ano (agregação por data).</summary>
    public Task<List<FaturamentoMensal>> FaturamentoMensalAsync(int ano) => pedidos.FaturamentoMensalAsync(ano);
}
