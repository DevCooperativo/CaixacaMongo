namespace CachacaShop.Domain.Common;

public class DomainException(string message) : Exception(message)
{
    public static void Quando(bool condicao, string mensagem)
    {
        if (condicao) throw new DomainException(mensagem);
    }
}
