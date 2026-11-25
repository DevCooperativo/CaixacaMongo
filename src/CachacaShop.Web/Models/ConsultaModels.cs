using CachacaShop.Domain.Cachacas;
using CachacaShop.Domain.Consultas;
using CachacaShop.Domain.Pedidos;

namespace CachacaShop.Web.Models;

public class CatalogoViewModel
{
    public string? Nome { get; set; }
    public TipoCachaca? Tipo { get; set; }
    public Madeira? Madeira { get; set; }
    public decimal? PrecoMinimo { get; set; }
    public decimal? PrecoMaximo { get; set; }
    public bool SomenteDisponiveis { get; set; }
    public List<Cachaca> Resultado { get; set; } = [];
    public Dictionary<string, string> Produtores { get; set; } = [];
}

public class PorEstadoViewModel
{
    public string? Estado { get; set; }
    public List<CachacaPorEstado> Resultado { get; set; } = [];
}

public class EstoqueBaixoViewModel
{
    public int Limite { get; set; } = 10;
    public List<Cachaca> Resultado { get; set; } = [];
}

public class PedidosClienteViewModel
{
    public string? ClienteId { get; set; }
    public StatusPedido? Status { get; set; }
    public DateTime? De { get; set; }
    public DateTime? Ate { get; set; }
    public List<Pedido>? Resultado { get; set; }
}

public class MaisVendidasViewModel
{
    public int Top { get; set; } = 5;
    public List<CachacaMaisVendida> Resultado { get; set; } = [];
}

public class FaturamentoViewModel
{
    public int Ano { get; set; } = DateTime.Today.Year;
    public List<FaturamentoMensal> Resultado { get; set; } = [];
}
