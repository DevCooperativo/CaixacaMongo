using CachacaShop.Domain.Cachacas;
using CachacaShop.Domain.Pedidos;

namespace CachacaShop.Service;

public record ProdutorDados(string Nome, string Cidade, string Estado, int AnoFundacao, string? Descricao);

public record CachacaDados(string Nome, string ProdutorId, TipoCachaca Tipo, Madeira Madeira, int MesesEnvelhecimento,
    decimal TeorAlcoolico, int VolumeMl, decimal Preco, int Estoque, string? Descricao);

public record ClienteDados(string Nome, string Email, DateTime DataNascimento,
    string Logradouro, string Cidade, string Estado, string Cep);

public record ItemPedidoDados(string CachacaId, int Quantidade);

public record NovoPedidoDados(string ClienteId, IReadOnlyCollection<ItemPedidoDados> Itens);

public enum AcaoPedido
{
    Pagar,
    Enviar,
    Entregar,
    Cancelar
}

public static class StatusPedidoExtensions
{
    public static IEnumerable<AcaoPedido> AcoesDisponiveis(this StatusPedido status) => status switch
    {
        StatusPedido.Pendente => [AcaoPedido.Pagar, AcaoPedido.Cancelar],
        StatusPedido.Pago => [AcaoPedido.Enviar, AcaoPedido.Cancelar],
        StatusPedido.Enviado => [AcaoPedido.Entregar],
        _ => []
    };
}
