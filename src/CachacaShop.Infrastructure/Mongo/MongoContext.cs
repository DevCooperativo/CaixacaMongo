using CachacaShop.Domain.Cachacas;
using CachacaShop.Domain.Clientes;
using CachacaShop.Domain.Pedidos;
using CachacaShop.Domain.Produtores;
using MongoDB.Driver;

namespace CachacaShop.Infrastructure.Mongo;

public class MongoContext
{
    public const string ColecaoProdutores = "produtores";
    public const string ColecaoCachacas = "cachacas";
    public const string ColecaoClientes = "clientes";
    public const string ColecaoPedidos = "pedidos";

    public IMongoDatabase Database { get; }

    public MongoContext(string connectionString, string databaseName)
    {
        MongoMappings.Registrar();
        Database = new MongoClient(connectionString).GetDatabase(databaseName);
    }

    public IMongoCollection<Produtor> Produtores => Database.GetCollection<Produtor>(ColecaoProdutores);
    public IMongoCollection<Cachaca> Cachacas => Database.GetCollection<Cachaca>(ColecaoCachacas);
    public IMongoCollection<Cliente> Clientes => Database.GetCollection<Cliente>(ColecaoClientes);
    public IMongoCollection<Pedido> Pedidos => Database.GetCollection<Pedido>(ColecaoPedidos);

    public async Task CriarIndicesAsync()
    {
        await Cachacas.Indexes.CreateManyAsync(
        [
            new CreateIndexModel<Cachaca>(Builders<Cachaca>.IndexKeys.Ascending(c => c.ProdutorId)),
            new CreateIndexModel<Cachaca>(Builders<Cachaca>.IndexKeys.Ascending(c => c.Tipo).Ascending(c => c.Preco)),
            new CreateIndexModel<Cachaca>(Builders<Cachaca>.IndexKeys.Ascending(c => c.Estoque))
        ]);

        await Clientes.Indexes.CreateOneAsync(new CreateIndexModel<Cliente>(
            Builders<Cliente>.IndexKeys.Ascending(c => c.Email), new CreateIndexOptions { Unique = true }));

        await Pedidos.Indexes.CreateManyAsync(
        [
            new CreateIndexModel<Pedido>(Builders<Pedido>.IndexKeys.Ascending(p => p.ClienteId).Descending(p => p.Data)),
            new CreateIndexModel<Pedido>(Builders<Pedido>.IndexKeys.Ascending("itens.cachacaId")),
            new CreateIndexModel<Pedido>(Builders<Pedido>.IndexKeys.Ascending(p => p.Status).Ascending(p => p.Data))
        ]);
    }
}
