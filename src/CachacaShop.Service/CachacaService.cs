using CachacaShop.Domain;
using CachacaShop.Domain.Cachacas;
using CachacaShop.Domain.Common;

namespace CachacaShop.Service;

public class CachacaService(ICachacaRepository cachacas, IProdutorRepository produtores, IPedidoRepository pedidos)
{
    public Task<List<Cachaca>> ListarAsync() => cachacas.ListarAsync();

    public Task<Cachaca?> ObterAsync(string id) => cachacas.ObterAsync(id);

    public async Task<Cachaca> CriarAsync(CachacaDados d)
    {
        await GarantirProdutorAsync(d.ProdutorId);
        var cachaca = new Cachaca(d.Nome, d.ProdutorId, d.Tipo, d.Madeira, d.MesesEnvelhecimento,
            d.TeorAlcoolico, d.VolumeMl, d.Preco, d.Estoque, d.Descricao);
        await cachacas.InserirAsync(cachaca);
        return cachaca;
    }

    public async Task AtualizarAsync(string id, CachacaDados d)
    {
        var cachaca = await cachacas.ObterAsync(id) ?? throw new DomainException("Cachaça não encontrada.");
        await GarantirProdutorAsync(d.ProdutorId);
        cachaca.Atualizar(d.Nome, d.ProdutorId, d.Tipo, d.Madeira, d.MesesEnvelhecimento,
            d.TeorAlcoolico, d.VolumeMl, d.Preco, d.Descricao);
        await cachacas.AtualizarAsync(cachaca);
    }

    public async Task ReporEstoqueAsync(string id, int quantidade)
    {
        var cachaca = await cachacas.ObterAsync(id) ?? throw new DomainException("Cachaça não encontrada.");
        cachaca.ReporEstoque(quantidade);
        await cachacas.AtualizarAsync(cachaca);
    }

    public async Task RemoverAsync(string id)
    {
        DomainException.Quando(await pedidos.ExisteComCachacaAsync(id),
            "Não é possível remover uma cachaça que já consta em pedidos.");
        await cachacas.RemoverAsync(id);
    }

    private async Task GarantirProdutorAsync(string produtorId) =>
        DomainException.Quando(await produtores.ObterAsync(produtorId) is null, "Produtor informado não existe.");
}
