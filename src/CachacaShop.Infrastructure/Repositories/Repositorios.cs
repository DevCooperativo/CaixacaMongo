using CachacaShop.Domain;
using CachacaShop.Domain.Clientes;
using CachacaShop.Domain.Produtores;
using CachacaShop.Infrastructure.Mongo;
using MongoDB.Driver;

namespace CachacaShop.Infrastructure.Repositories;

public class ProdutorRepository(MongoContext ctx) : MongoRepository<Produtor>(ctx.Produtores), IProdutorRepository
{
    protected override SortDefinition<Produtor> Ordenacao => Builders<Produtor>.Sort.Ascending(p => p.Nome);
}

public class ClienteRepository(MongoContext ctx) : MongoRepository<Cliente>(ctx.Clientes), IClienteRepository
{
    protected override SortDefinition<Cliente> Ordenacao => Builders<Cliente>.Sort.Ascending(c => c.Nome);

    public Task<bool> EmailEmUsoAsync(string email, string? ignorarId = null)
    {
        var normalizado = (email ?? string.Empty).Trim().ToLowerInvariant();
        var filtro = Builders<Cliente>.Filter.Eq(c => c.Email, normalizado);
        if (!string.IsNullOrEmpty(ignorarId))
            filtro &= Builders<Cliente>.Filter.Ne(c => c.Id, ignorarId);
        return Colecao.Find(filtro).AnyAsync();
    }
}
