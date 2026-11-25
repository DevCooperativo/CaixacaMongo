using CachacaShop.Domain;
using CachacaShop.Domain.Common;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CachacaShop.Infrastructure.Repositories;

public abstract class MongoRepository<T>(IMongoCollection<T> colecao) : IRepository<T> where T : Entity
{
    protected IMongoCollection<T> Colecao { get; } = colecao;

    protected virtual SortDefinition<T> Ordenacao => Builders<T>.Sort.Ascending(e => e.Id);

    public Task<List<T>> ListarAsync() => Colecao.Find(FilterDefinition<T>.Empty).Sort(Ordenacao).ToListAsync();

    public async Task<T?> ObterAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return null;
        return await Colecao.Find(e => e.Id == id).FirstOrDefaultAsync();
    }

    public Task InserirAsync(T entidade) => Colecao.InsertOneAsync(entidade);

    public Task AtualizarAsync(T entidade) => Colecao.ReplaceOneAsync(e => e.Id == entidade.Id, entidade);

    public Task RemoverAsync(string id) => Colecao.DeleteOneAsync(e => e.Id == id);

    protected static ObjectId ParseId(string id) =>
        ObjectId.TryParse(id, out var objectId) ? objectId : ObjectId.Empty;
}
