namespace CachacaShop.Domain.Common;

public static class Uf
{
    public static readonly string[] Todas =
    [
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA",
        "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    ];

    public static string Normalizar(string? uf)
    {
        var valor = (uf ?? string.Empty).Trim().ToUpperInvariant();
        DomainException.Quando(!Todas.Contains(valor), $"UF inválida: '{uf}'.");
        return valor;
    }
}
