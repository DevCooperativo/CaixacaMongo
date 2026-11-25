using System.ComponentModel.DataAnnotations;
using CachacaShop.Domain.Cachacas;
using CachacaShop.Domain.Clientes;
using CachacaShop.Domain.Produtores;
using CachacaShop.Service;

namespace CachacaShop.Web.Models;

public class ProdutorForm
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "Informe o nome."), StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a cidade."), StringLength(80)]
    public string Cidade { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a UF.")]
    public string Estado { get; set; } = "MG";

    [Display(Name = "Ano de fundação"), Range(1500, 2100, ErrorMessage = "Ano inválido.")]
    public int AnoFundacao { get; set; } = DateTime.Today.Year;

    [Display(Name = "Descrição"), StringLength(500)]
    public string? Descricao { get; set; }

    public ProdutorDados ParaDados() => new(Nome, Cidade, Estado, AnoFundacao, Descricao);

    public static ProdutorForm De(Produtor p) => new()
    {
        Id = p.Id, Nome = p.Nome, Cidade = p.Cidade, Estado = p.Estado,
        AnoFundacao = p.AnoFundacao, Descricao = p.Descricao
    };
}

public class CachacaForm
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "Informe o nome."), StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "Produtor"), Required(ErrorMessage = "Selecione o produtor.")]
    public string ProdutorId { get; set; } = string.Empty;

    public TipoCachaca Tipo { get; set; }

    public Madeira Madeira { get; set; }

    [Display(Name = "Envelhecimento (meses)"), Range(0, 600)]
    public int MesesEnvelhecimento { get; set; }

    [Display(Name = "Teor alcoólico (%)"), Range(38, 48, ErrorMessage = "Entre 38% e 48%.")]
    public decimal TeorAlcoolico { get; set; } = 40;

    [Display(Name = "Volume (ml)"), Range(50, 5000)]
    public int VolumeMl { get; set; } = 700;

    [Display(Name = "Preço (R$)"), Range(0.01, 100000, ErrorMessage = "Preço inválido.")]
    public decimal Preco { get; set; }

    [Display(Name = "Estoque inicial"), Range(0, 100000)]
    public int Estoque { get; set; }

    [Display(Name = "Descrição"), StringLength(500)]
    public string? Descricao { get; set; }

    public CachacaDados ParaDados() => new(Nome, ProdutorId, Tipo, Madeira, MesesEnvelhecimento, TeorAlcoolico,
        VolumeMl, Preco, Estoque, Descricao);

    public static CachacaForm De(Cachaca c) => new()
    {
        Id = c.Id, Nome = c.Nome, ProdutorId = c.ProdutorId, Tipo = c.Tipo, Madeira = c.Madeira,
        MesesEnvelhecimento = c.MesesEnvelhecimento, TeorAlcoolico = c.TeorAlcoolico, VolumeMl = c.VolumeMl,
        Preco = c.Preco, Estoque = c.Estoque, Descricao = c.Descricao
    };
}

public class ClienteForm
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "Informe o nome."), StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "E-mail"), Required(ErrorMessage = "Informe o e-mail."), EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Data de nascimento"), DataType(DataType.Date)]
    public DateTime DataNascimento { get; set; } = DateTime.Today.AddYears(-25);

    [Required(ErrorMessage = "Informe o logradouro.")]
    public string Logradouro { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a cidade.")]
    public string Cidade { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a UF.")]
    public string Estado { get; set; } = "ES";

    [Display(Name = "CEP"), Required(ErrorMessage = "Informe o CEP.")]
    public string Cep { get; set; } = string.Empty;

    public ClienteDados ParaDados() => new(Nome, Email, DataNascimento, Logradouro, Cidade, Estado, Cep);

    public static ClienteForm De(Cliente c) => new()
    {
        Id = c.Id, Nome = c.Nome, Email = c.Email, DataNascimento = c.DataNascimento.Date,
        Logradouro = c.Endereco.Logradouro, Cidade = c.Endereco.Cidade, Estado = c.Endereco.Estado, Cep = c.Endereco.Cep
    };
}

public class PedidoForm
{
    public const int Linhas = 5;

    [Display(Name = "Cliente"), Required(ErrorMessage = "Selecione o cliente.")]
    public string ClienteId { get; set; } = string.Empty;

    public List<ItemForm> Itens { get; set; } = Enumerable.Range(0, Linhas).Select(_ => new ItemForm()).ToList();

    public NovoPedidoDados ParaDados() =>
        new(ClienteId, Itens.Select(i => new ItemPedidoDados(i.CachacaId ?? string.Empty, i.Quantidade)).ToList());
}

public class ItemForm
{
    public string? CachacaId { get; set; }

    [Range(0, 1000)]
    public int Quantidade { get; set; }
}
