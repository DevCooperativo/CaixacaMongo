using CachacaShop.Domain.Common;

namespace CachacaShop.Domain.Pedidos;

public enum StatusPedido
{
    Pendente,
    Pago,
    Enviado,
    Entregue,
    Cancelado
}

/// <summary>
/// Item embutido no documento do pedido. Guarda um "snapshot" do nome e do preço
/// da cachaça no momento da compra (alterações futuras no produto não afetam o pedido).
/// </summary>
public class ItemPedido
{
    public string CachacaId { get; private set; } = default!;
    public string NomeCachaca { get; private set; } = default!;
    public decimal PrecoUnitario { get; private set; }
    public int Quantidade { get; private set; }
    public decimal Subtotal { get; private set; }

    protected ItemPedido() { }

    internal ItemPedido(string cachacaId, string nomeCachaca, decimal precoUnitario, int quantidade)
    {
        DomainException.Quando(quantidade <= 0, "A quantidade do item deve ser maior que zero.");
        CachacaId = cachacaId;
        NomeCachaca = nomeCachaca;
        PrecoUnitario = precoUnitario;
        Quantidade = quantidade;
        Subtotal = precoUnitario * quantidade;
    }

    internal void Somar(int quantidade)
    {
        DomainException.Quando(quantidade <= 0, "A quantidade do item deve ser maior que zero.");
        Quantidade += quantidade;
        Subtotal = PrecoUnitario * Quantidade;
    }
}
