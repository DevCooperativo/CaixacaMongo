using System.Text.RegularExpressions;
using CachacaShop.Domain;
using CachacaShop.Domain.Cachacas;
using CachacaShop.Domain.Consultas;
using CachacaShop.Infrastructure.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CachacaShop.Infrastructure.Repositories;

public class CachacaRepository(MongoContext ctx) : MongoRepository<Cachaca>(ctx.Cachacas), ICachacaRepository
{
    private static readonly FilterDefinitionBuilder<Cachaca> F = Builders<Cachaca>.Filter;

    protected override SortDefinition<Cachaca> Ordenacao => Builders<Cachaca>.Sort.Ascending(c => c.Nome);

    public Task<List<Cachaca>> ObterPorIdsAsync(IEnumerable<string> ids) =>
        Colecao.Find(F.In(c => c.Id, ids.Distinct())).ToListAsync();

    public Task<bool> ExisteDoProdutorAsync(string produtorId) =>
        Colecao.Find(c => c.ProdutorId == produtorId).AnyAsync();

    /// <summary>
    /// db.cachacas.find({ nome: /texto/i, tipo: "...", madeira: "...", preco: { $gte: min, $lte: max }, estoque: { $gt: 0 } })
    ///            .sort({ preco: 1 })
    /// </summary>
    public Task<List<Cachaca>> BuscarCatalogoAsync(FiltroCatalogo filtro)
    {
        var filtros = new List<FilterDefinition<Cachaca>>();

        if (!string.IsNullOrWhiteSpace(filtro.Nome))
            filtros.Add(F.Regex(c => c.Nome, new BsonRegularExpression(Regex.Escape(filtro.Nome.Trim()), "i")));
        if (filtro.Tipo.HasValue)
            filtros.Add(F.Eq(c => c.Tipo, filtro.Tipo.Value));
        if (filtro.Madeira.HasValue)
            filtros.Add(F.Eq(c => c.Madeira, filtro.Madeira.Value));
        if (filtro.PrecoMinimo.HasValue)
            filtros.Add(F.Gte(c => c.Preco, filtro.PrecoMinimo.Value));
        if (filtro.PrecoMaximo.HasValue)
            filtros.Add(F.Lte(c => c.Preco, filtro.PrecoMaximo.Value));
        if (filtro.SomenteDisponiveis)
            filtros.Add(F.Gt(c => c.Estoque, 0));

        var where = filtros.Count > 0 ? F.And(filtros) : F.Empty;
        return Colecao.Find(where).SortBy(c => c.Preco).ToListAsync();
    }

    /// <summary>Junção entre cachacas e produtores usando $lookup.</summary>
    public async Task<List<CachacaPorEstado>> BuscarPorEstadoDoProdutorAsync(string estado)
    {
        var pipeline = new[]
        {
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", MongoContext.ColecaoProdutores },
                { "localField", "produtorId" },
                { "foreignField", "_id" },
                { "as", "produtor" }
            }),
            new BsonDocument("$unwind", "$produtor"),
            new BsonDocument("$match", new BsonDocument("produtor.estado", estado.Trim().ToUpperInvariant())),
            new BsonDocument("$sort", new BsonDocument { { "produtor.nome", 1 }, { "preco", 1 } }),
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 0 },
                { "estado", "$produtor.estado" },
                { "produtor", "$produtor.nome" },
                { "cidade", "$produtor.cidade" },
                { "nome", 1 }, { "tipo", 1 }, { "preco", 1 }, { "estoque", 1 }
            })
        };

        var docs = await Colecao.Aggregate<BsonDocument>(pipeline).ToListAsync();
        return docs.Select(d => new CachacaPorEstado(
            d["estado"].AsString, d["produtor"].AsString, d["cidade"].AsString, d["nome"].AsString,
            Enum.Parse<TipoCachaca>(d["tipo"].AsString), d["preco"].AsDecimal, d["estoque"].AsInt32)).ToList();
    }

    /// <summary>db.cachacas.find({ estoque: { $lte: limite } }).sort({ estoque: 1 })</summary>
    public Task<List<Cachaca>> BuscarEstoqueBaixoAsync(int limite) =>
        Colecao.Find(F.Lte(c => c.Estoque, limite)).SortBy(c => c.Estoque).ThenBy(c => c.Nome).ToListAsync();
}
