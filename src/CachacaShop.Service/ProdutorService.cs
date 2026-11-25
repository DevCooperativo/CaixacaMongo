using CachacaShop.Domain;
using CachacaShop.Domain.Common;
using CachacaShop.Domain.Produtores;

namespace CachacaShop.Service;

public class ProdutorService(IProdutorRepository produtores, ICachacaRepository cachacas)
{
    public Task<List<Produtor>> ListarAsync() => produtores.ListarAsync();

    public Task<Produtor?> ObterAsync(string id) => produtores.ObterAsync(id);

    public async Task<Produtor> CriarAsync(ProdutorDados dados)
    {
        var produtor = new Produtor(dados.Nome, dados.Cidade, dados.Estado, dados.AnoFundacao, dados.Descricao);
        await produtores.InserirAsync(produtor);
        return produtor;
    }

    public async Task AtualizarAsync(string id, ProdutorDados dados)
    {
        var produtor = await produtores.ObterAsync(id) ?? throw new DomainException("Produtor não encontrado.");
        produtor.Atualizar(dados.Nome, dados.Cidade, dados.Estado, dados.AnoFundacao, dados.Descricao);
        await produtores.AtualizarAsync(produtor);
    }

    public async Task RemoverAsync(string id)
    {
        DomainException.Quando(await cachacas.ExisteDoProdutorAsync(id),
            "Não é possível remover um produtor que possui cachaças cadastradas.");
        await produtores.RemoverAsync(id);
    }
}
