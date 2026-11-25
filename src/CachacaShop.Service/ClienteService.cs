using CachacaShop.Domain;
using CachacaShop.Domain.Clientes;
using CachacaShop.Domain.Common;

namespace CachacaShop.Service;

public class ClienteService(IClienteRepository clientes, IPedidoRepository pedidos)
{
    public Task<List<Cliente>> ListarAsync() => clientes.ListarAsync();

    public Task<Cliente?> ObterAsync(string id) => clientes.ObterAsync(id);

    public async Task<Cliente> CriarAsync(ClienteDados d)
    {
        DomainException.Quando(await clientes.EmailEmUsoAsync(d.Email), "Já existe um cliente com este e-mail.");
        var cliente = new Cliente(d.Nome, d.Email, d.DataNascimento,
            new Endereco(d.Logradouro, d.Cidade, d.Estado, d.Cep));
        await clientes.InserirAsync(cliente);
        return cliente;
    }

    public async Task AtualizarAsync(string id, ClienteDados d)
    {
        var cliente = await clientes.ObterAsync(id) ?? throw new DomainException("Cliente não encontrado.");
        DomainException.Quando(await clientes.EmailEmUsoAsync(d.Email, id), "Já existe um cliente com este e-mail.");
        cliente.Atualizar(d.Nome, d.Email, d.DataNascimento, new Endereco(d.Logradouro, d.Cidade, d.Estado, d.Cep));
        await clientes.AtualizarAsync(cliente);
    }

    public async Task RemoverAsync(string id)
    {
        DomainException.Quando(await pedidos.ExisteDoClienteAsync(id),
            "Não é possível remover um cliente que possui pedidos.");
        await clientes.RemoverAsync(id);
    }
}
