using CachacaShop.Domain.Common;

namespace CachacaShop.Domain.Produtores;

/// <summary>Alambique / produtor artesanal que fabrica as cachaças.</summary>
public class Produtor : Entity
{
    public string Nome { get; private set; } = default!;
    public string Cidade { get; private set; } = default!;
    public string Estado { get; private set; } = default!;
    public int AnoFundacao { get; private set; }
    public string? Descricao { get; private set; }

    protected Produtor() { }

    public Produtor(string nome, string cidade, string estado, int anoFundacao, string? descricao)
    {
        Atualizar(nome, cidade, estado, anoFundacao, descricao);
    }

    public void Atualizar(string nome, string cidade, string estado, int anoFundacao, string? descricao)
    {
        DomainException.Quando(string.IsNullOrWhiteSpace(nome), "O nome do produtor é obrigatório.");
        DomainException.Quando(string.IsNullOrWhiteSpace(cidade), "A cidade do produtor é obrigatória.");
        DomainException.Quando(anoFundacao < 1500 || anoFundacao > DateTime.Today.Year,
            "Ano de fundação inválido.");

        Nome = nome.Trim();
        Cidade = cidade.Trim();
        Estado = Uf.Normalizar(estado);
        AnoFundacao = anoFundacao;
        Descricao = descricao?.Trim();
    }

    public int AnosDeTradicao => DateTime.Today.Year - AnoFundacao;
}
