using CachacaShop.Domain.Common;

namespace CachacaShop.Domain.Clientes;

/// <summary>Value object embutido no documento do cliente.</summary>
public class Endereco
{
    public string Logradouro { get; private set; } = default!;
    public string Cidade { get; private set; } = default!;
    public string Estado { get; private set; } = default!;
    public string Cep { get; private set; } = default!;

    protected Endereco() { }

    public Endereco(string logradouro, string cidade, string estado, string cep)
    {
        DomainException.Quando(string.IsNullOrWhiteSpace(logradouro), "O logradouro é obrigatório.");
        DomainException.Quando(string.IsNullOrWhiteSpace(cidade), "A cidade é obrigatória.");
        var digitos = new string((cep ?? string.Empty).Where(char.IsDigit).ToArray());
        DomainException.Quando(digitos.Length != 8, "CEP inválido.");

        Logradouro = logradouro.Trim();
        Cidade = cidade.Trim();
        Estado = Uf.Normalizar(estado);
        Cep = $"{digitos[..5]}-{digitos[5..]}";
    }

    public override string ToString() => $"{Logradouro} - {Cidade}/{Estado} - CEP {Cep}";
}
