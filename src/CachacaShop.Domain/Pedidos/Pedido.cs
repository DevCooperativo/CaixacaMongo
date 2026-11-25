using CachacaShop.Domain.Cachacas;
using CachacaShop.Domain.Clientes;
using CachacaShop.Domain.Common;

namespace CachacaShop.Domain.Pedidos;

public class Pedido : Entity
{
    public const decimal ValorFreteGratis = 300m;
    public const decimal FretePadrao = 25m;

    // Não é readonly para que o driver do MongoDB consiga preenchê-lo ao desserializar.
    private List<ItemPedido> _itens = [];

    public string ClienteId { get; private set; } = default!;
    public string NomeCliente { get; private set; } = default!;
    public string EstadoEntrega { get; private set; } = default!;
    public DateTime Data { get; private set; }
    public StatusPedido Status { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal Frete { get; private set; }
    public decimal Total { get; private set; }
    public IReadOnlyCollection<ItemPedido> Itens => _itens.AsReadOnly();

    protected Pedido() { }

    public Pedido(Cliente cliente, DateTime? data = null)
    {
        DomainException.Quando(cliente is null, "O pedido deve ter um cliente.");
        ClienteId = cliente!.Id;
        NomeCliente = cliente.Nome;
        EstadoEntrega = cliente.Endereco.Estado;
        Data = data ?? DateTime.UtcNow;
        Status = StatusPedido.Pendente;
    }

    /// <summary>Adiciona uma cachaça ao pedido e já baixa o estoque do produto.</summary>
    public void AdicionarItem(Cachaca cachaca, int quantidade)
    {
        GarantirStatus(StatusPedido.Pendente, "Só é possível alterar itens de pedidos pendentes.");
        cachaca.BaixarEstoque(quantidade);

        var existente = _itens.FirstOrDefault(i => i.CachacaId == cachaca.Id);
        if (existente is null)
            _itens.Add(new ItemPedido(cachaca.Id, cachaca.Nome, cachaca.Preco, quantidade));
        else
            existente.Somar(quantidade);

        RecalcularTotais();
    }

    public void Pagar()
    {
        GarantirStatus(StatusPedido.Pendente, "Somente pedidos pendentes podem ser pagos.");
        DomainException.Quando(_itens.Count == 0, "Não é possível pagar um pedido sem itens.");
        Status = StatusPedido.Pago;
    }

    public void Enviar()
    {
        GarantirStatus(StatusPedido.Pago, "Somente pedidos pagos podem ser enviados.");
        Status = StatusPedido.Enviado;
    }

    public void Entregar()
    {
        GarantirStatus(StatusPedido.Enviado, "Somente pedidos enviados podem ser entregues.");
        Status = StatusPedido.Entregue;
    }

    /// <summary>Cancela o pedido e devolve os itens, para que o estoque seja reposto.</summary>
    public IReadOnlyCollection<ItemPedido> Cancelar()
    {
        DomainException.Quando(Status is StatusPedido.Enviado or StatusPedido.Entregue,
            "Pedidos enviados ou entregues não podem ser cancelados.");
        DomainException.Quando(Status == StatusPedido.Cancelado, "O pedido já está cancelado.");
        Status = StatusPedido.Cancelado;
        return Itens;
    }

    /// <summary>Só pedidos cancelados podem ser excluídos (os demais são histórico de venda).</summary>
    public bool PodeSerExcluido => Status == StatusPedido.Cancelado;

    public int QuantidadeGarrafas => _itens.Sum(i => i.Quantidade);

    private void RecalcularTotais()
    {
        Subtotal = _itens.Sum(i => i.Subtotal);
        Frete = Subtotal >= ValorFreteGratis ? 0 : FretePadrao;
        Total = Subtotal + Frete;
    }

    private void GarantirStatus(StatusPedido esperado, string mensagem) =>
        DomainException.Quando(Status != esperado, mensagem);
}
