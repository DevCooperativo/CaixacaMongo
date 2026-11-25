using CachacaShop.Domain;
using CachacaShop.Domain.Common;
using CachacaShop.Domain.Pedidos;

namespace CachacaShop.Service;

public class PedidoService(IPedidoRepository pedidos, IClienteRepository clientes, ICachacaRepository cachacas)
{
    public Task<List<Pedido>> ListarAsync() => pedidos.ListarAsync();

    public Task<Pedido?> ObterAsync(string id) => pedidos.ObterAsync(id);

    public async Task<Pedido> CriarAsync(NovoPedidoDados dados)
    {
        var itens = dados.Itens.Where(i => !string.IsNullOrEmpty(i.CachacaId) && i.Quantidade > 0).ToList();
        DomainException.Quando(itens.Count == 0, "Informe ao menos uma cachaça no pedido.");

        var cliente = await clientes.ObterAsync(dados.ClienteId)
                      ?? throw new DomainException("Cliente não encontrado.");
        var produtos = (await cachacas.ObterPorIdsAsync(itens.Select(i => i.CachacaId)))
            .ToDictionary(c => c.Id);

        var pedido = new Pedido(cliente);
        foreach (var item in itens)
        {
            if (!produtos.TryGetValue(item.CachacaId, out var cachaca))
                throw new DomainException("Cachaça não encontrada.");
            pedido.AdicionarItem(cachaca, item.Quantidade); // regra de estoque fica no domínio
        }

        foreach (var cachaca in produtos.Values)
            await cachacas.AtualizarAsync(cachaca);
        await pedidos.InserirAsync(pedido);
        return pedido;
    }

    public async Task ExecutarAcaoAsync(string id, AcaoPedido acao)
    {
        var pedido = await pedidos.ObterAsync(id) ?? throw new DomainException("Pedido não encontrado.");
        switch (acao)
        {
            case AcaoPedido.Pagar: pedido.Pagar(); break;
            case AcaoPedido.Enviar: pedido.Enviar(); break;
            case AcaoPedido.Entregar: pedido.Entregar(); break;
            case AcaoPedido.Cancelar:
                var itens = pedido.Cancelar();
                var produtos = await cachacas.ObterPorIdsAsync(itens.Select(i => i.CachacaId));
                foreach (var cachaca in produtos)
                {
                    cachaca.ReporEstoque(itens.Where(i => i.CachacaId == cachaca.Id).Sum(i => i.Quantidade));
                    await cachacas.AtualizarAsync(cachaca);
                }
                break;
        }
        await pedidos.AtualizarAsync(pedido);
    }

    public async Task RemoverAsync(string id)
    {
        var pedido = await pedidos.ObterAsync(id) ?? throw new DomainException("Pedido não encontrado.");
        DomainException.Quando(!pedido.PodeSerExcluido, "Apenas pedidos cancelados podem ser excluídos.");
        await pedidos.RemoverAsync(id);
    }
}
