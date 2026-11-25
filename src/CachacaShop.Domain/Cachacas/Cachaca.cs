using CachacaShop.Domain.Common;

namespace CachacaShop.Domain.Cachacas;

/// <summary>Produto vendido no e-commerce.</summary>
public class Cachaca : Entity
{
    public string Nome { get; private set; } = default!;
    public string ProdutorId { get; private set; } = default!;
    public TipoCachaca Tipo { get; private set; }
    public Madeira Madeira { get; private set; }
    public int MesesEnvelhecimento { get; private set; }
    public decimal TeorAlcoolico { get; private set; }
    public int VolumeMl { get; private set; }
    public decimal Preco { get; private set; }
    public int Estoque { get; private set; }
    public string? Descricao { get; private set; }
    public DateTime CadastradaEm { get; private set; }

    protected Cachaca() { }

    public Cachaca(string nome, string produtorId, TipoCachaca tipo, Madeira madeira, int mesesEnvelhecimento,
        decimal teorAlcoolico, int volumeMl, decimal preco, int estoque, string? descricao)
    {
        DomainException.Quando(estoque < 0, "O estoque inicial não pode ser negativo.");
        Atualizar(nome, produtorId, tipo, madeira, mesesEnvelhecimento, teorAlcoolico, volumeMl, preco, descricao);
        Estoque = estoque;
        CadastradaEm = DateTime.UtcNow;
    }

    public void Atualizar(string nome, string produtorId, TipoCachaca tipo, Madeira madeira, int mesesEnvelhecimento,
        decimal teorAlcoolico, int volumeMl, decimal preco, string? descricao)
    {
        DomainException.Quando(string.IsNullOrWhiteSpace(nome), "O nome da cachaça é obrigatório.");
        DomainException.Quando(string.IsNullOrWhiteSpace(produtorId), "A cachaça deve pertencer a um produtor.");
        // Pela legislação brasileira, a cachaça tem graduação alcoólica entre 38% e 48% v/v.
        DomainException.Quando(teorAlcoolico < 38 || teorAlcoolico > 48,
            "O teor alcoólico de uma cachaça deve estar entre 38% e 48%.");
        DomainException.Quando(volumeMl <= 0, "O volume deve ser maior que zero.");
        DomainException.Quando(mesesEnvelhecimento < 0, "Meses de envelhecimento não pode ser negativo.");
        DomainException.Quando(
            tipo is TipoCachaca.Ouro or TipoCachaca.Envelhecida or TipoCachaca.Premium or TipoCachaca.ExtraPremium
            && madeira == Madeira.Nenhuma,
            "Cachaças ouro/envelhecidas devem informar a madeira.");
        DomainException.Quando(tipo == TipoCachaca.Premium && mesesEnvelhecimento < 12,
            "Cachaça premium exige no mínimo 12 meses de envelhecimento.");
        DomainException.Quando(tipo == TipoCachaca.ExtraPremium && mesesEnvelhecimento < 36,
            "Cachaça extra premium exige no mínimo 36 meses de envelhecimento.");

        Nome = nome.Trim();
        ProdutorId = produtorId;
        Tipo = tipo;
        Madeira = madeira;
        MesesEnvelhecimento = mesesEnvelhecimento;
        TeorAlcoolico = teorAlcoolico;
        VolumeMl = volumeMl;
        Descricao = descricao?.Trim();
        AlterarPreco(preco);
    }

    public void AlterarPreco(decimal novoPreco)
    {
        DomainException.Quando(novoPreco <= 0, "O preço deve ser maior que zero.");
        Preco = decimal.Round(novoPreco, 2);
    }

    public void BaixarEstoque(int quantidade)
    {
        DomainException.Quando(quantidade <= 0, "Quantidade inválida.");
        DomainException.Quando(quantidade > Estoque,
            $"Estoque insuficiente para '{Nome}'. Disponível: {Estoque}, solicitado: {quantidade}.");
        Estoque -= quantidade;
    }

    public void ReporEstoque(int quantidade)
    {
        DomainException.Quando(quantidade <= 0, "Quantidade inválida.");
        Estoque += quantidade;
    }

    public bool Disponivel => Estoque > 0;
}
