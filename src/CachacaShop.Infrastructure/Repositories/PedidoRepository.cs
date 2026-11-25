using CachacaShop.Domain;
using CachacaShop.Domain.Consultas;
using CachacaShop.Domain.Pedidos;
using CachacaShop.Infrastructure.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CachacaShop.Infrastructure.Repositories;

public class PedidoRepository(MongoContext ctx) : MongoRepository<Pedido>(ctx.Pedidos), IPedidoRepository
{
    private static readonly FilterDefinitionBuilder<Pedido> F = Builders<Pedido>.Filter;

    protected override SortDefinition<Pedido> Ordenacao => Builders<Pedido>.Sort.Descending(p => p.Data);

    public Task<bool> ExisteDoClienteAsync(string clienteId) =>
        Colecao.Find(p => p.ClienteId == clienteId).AnyAsync();

    public Task<bool> ExisteComCachacaAsync(string cachacaId) =>
        Colecao.Find(F.Eq("itens.cachacaId", ParseId(cachacaId))).AnyAsync();

    /// <summary>db.pedidos.find({ clienteId, status, data: { $gte: de, $lt: ate } }).sort({ data: -1 })</summary>
    public Task<List<Pedido>> BuscarPorClienteAsync(string clienteId, StatusPedido? status, DateTime? de,
        DateTime? ate)
    {
        var filtro = F.Eq(p => p.ClienteId, clienteId);
        if (status.HasValue)
            filtro &= F.Eq(p => p.Status, status.Value);
        if (de.HasValue)
            filtro &= F.Gte(p => p.Data, DateTime.SpecifyKind(de.Value.Date, DateTimeKind.Utc));
        if (ate.HasValue)
            filtro &= F.Lt(p => p.Data, DateTime.SpecifyKind(ate.Value.Date.AddDays(1), DateTimeKind.Utc));

        return Colecao.Find(filtro).SortByDescending(p => p.Data).ToListAsync();
    }

    /// <summary>Ranking de vendas: $match -> $unwind itens -> $group por cachaça -> $sort -> $limit.</summary>
    public async Task<List<CachacaMaisVendida>> MaisVendidasAsync(int top)
    {
        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument("status",
                new BsonDocument("$ne", nameof(StatusPedido.Cancelado)))),
            new BsonDocument("$unwind", "$itens"),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$itens.cachacaId" },
                { "nome", new BsonDocument("$first", "$itens.nomeCachaca") },
                { "quantidade", new BsonDocument("$sum", "$itens.quantidade") },
                { "faturamento", new BsonDocument("$sum", "$itens.subtotal") }
            }),
            new BsonDocument("$sort", new BsonDocument { { "quantidade", -1 }, { "faturamento", -1 } }),
            new BsonDocument("$limit", top)
        };

        var docs = await Colecao.Aggregate<BsonDocument>(pipeline).ToListAsync();
        return docs.Select(d => new CachacaMaisVendida(d["_id"].ToString()!, d["nome"].AsString,
            d["quantidade"].ToInt32(), d["faturamento"].AsDecimal)).ToList();
    }

    /// <summary>Faturamento por mês: pedidos pagos/enviados/entregues agrupados com $month.</summary>
    public async Task<List<FaturamentoMensal>> FaturamentoMensalAsync(int ano)
    {
        var inicio = new DateTime(ano, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var statusFaturados = new BsonArray
        {
            nameof(StatusPedido.Pago), nameof(StatusPedido.Enviado), nameof(StatusPedido.Entregue)
        };

        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                { "status", new BsonDocument("$in", statusFaturados) },
                { "data", new BsonDocument { { "$gte", inicio }, { "$lt", inicio.AddYears(1) } } }
            }),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", new BsonDocument("$month", "$data") },
                { "pedidos", new BsonDocument("$sum", 1) },
                { "faturamento", new BsonDocument("$sum", "$total") },
                { "ticketMedio", new BsonDocument("$avg", "$total") }
            }),
            new BsonDocument("$sort", new BsonDocument("_id", 1))
        };

        var docs = await Colecao.Aggregate<BsonDocument>(pipeline).ToListAsync();
        return docs.Select(d => new FaturamentoMensal(ano, d["_id"].ToInt32(), d["pedidos"].ToInt32(),
            d["faturamento"].AsDecimal, decimal.Round(d["ticketMedio"].AsDecimal, 2))).ToList();
    }
}
