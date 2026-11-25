using CachacaShop.Domain.Cachacas;
using CachacaShop.Domain.Clientes;
using CachacaShop.Domain.Consultas;
using CachacaShop.Domain.Pedidos;
using CachacaShop.Domain.Produtores;

namespace CachacaShop.Domain;

public interface IRepository<T>
{
    Task<List<T>> ListarAsync();
    Task<T?> ObterAsync(string id);
    Task InserirAsync(T entidade);
    Task AtualizarAsync(T entidade);
    Task RemoverAsync(string id);
}

public interface IProdutorRepository : IRepository<Produtor>;

public interface ICachacaRepository : IRepository<Cachaca>
{
    Task<List<Cachaca>> ObterPorIdsAsync(IEnumerable<string> ids);
    Task<bool> ExisteDoProdutorAsync(string produtorId);
    Task<List<Cachaca>> BuscarCatalogoAsync(FiltroCatalogo filtro);
    Task<List<CachacaPorEstado>> BuscarPorEstadoDoProdutorAsync(string estado);
    Task<List<Cachaca>> BuscarEstoqueBaixoAsync(int limite);
}

public interface IClienteRepository : IRepository<Cliente>
{
    Task<bool> EmailEmUsoAsync(string email, string? ignorarId = null);
}

public interface IPedidoRepository : IRepository<Pedido>
{
    Task<bool> ExisteDoClienteAsync(string clienteId);
    Task<bool> ExisteComCachacaAsync(string cachacaId);
    Task<List<Pedido>> BuscarPorClienteAsync(string clienteId, StatusPedido? status, DateTime? de, DateTime? ate);
    Task<List<CachacaMaisVendida>> MaisVendidasAsync(int top);
    Task<List<FaturamentoMensal>> FaturamentoMensalAsync(int ano);
}
