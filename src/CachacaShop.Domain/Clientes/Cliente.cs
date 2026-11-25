using System.Net.Mail;
using CachacaShop.Domain.Common;

namespace CachacaShop.Domain.Clientes;

public class Cliente : Entity
{
    public const int IdadeMinima = 18;

    public string Nome { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public DateTime DataNascimento { get; private set; }
    public Endereco Endereco { get; private set; } = default!;
    public DateTime CadastradoEm { get; private set; }

    protected Cliente() { }

    public Cliente(string nome, string email, DateTime dataNascimento, Endereco endereco)
    {
        Atualizar(nome, email, dataNascimento, endereco);
        CadastradoEm = DateTime.UtcNow;
    }

    public void Atualizar(string nome, string email, DateTime dataNascimento, Endereco endereco)
    {
        DomainException.Quando(string.IsNullOrWhiteSpace(nome), "O nome do cliente é obrigatório.");
        DomainException.Quando(!MailAddress.TryCreate(email, out _), "E-mail inválido.");
        // A venda de bebidas alcoólicas é proibida para menores de 18 anos.
        DomainException.Quando(CalcularIdade(dataNascimento, DateTime.Today) < IdadeMinima,
            $"O cliente deve ter pelo menos {IdadeMinima} anos para comprar bebidas alcoólicas.");

        Nome = nome.Trim();
        Email = email.Trim().ToLowerInvariant();
        DataNascimento = DateTime.SpecifyKind(dataNascimento.Date, DateTimeKind.Utc);
        Endereco = endereco ?? throw new DomainException("O endereço é obrigatório.");
    }

    public int Idade => CalcularIdade(DataNascimento, DateTime.Today);

    private static int CalcularIdade(DateTime nascimento, DateTime hoje)
    {
        var idade = hoje.Year - nascimento.Year;
        if (nascimento.Date > hoje.AddYears(-idade)) idade--;
        return idade;
    }
}
