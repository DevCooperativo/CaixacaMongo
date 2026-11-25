namespace CachacaShop.Domain.Common;

public abstract class Entity
{
    public string Id { get; protected set; } = default!;
}
