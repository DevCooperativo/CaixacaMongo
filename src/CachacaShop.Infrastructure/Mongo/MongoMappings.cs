using CachacaShop.Domain.Cachacas;
using CachacaShop.Domain.Clientes;
using CachacaShop.Domain.Common;
using CachacaShop.Domain.Pedidos;
using CachacaShop.Domain.Produtores;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

namespace CachacaShop.Infrastructure.Mongo;

/// <summary>
/// Mapeia as classes de domínio para BSON sem usar atributos do MongoDB no domínio
/// (o domínio permanece livre de dependências de infraestrutura).
/// </summary>
public static class MongoMappings
{
    private static readonly Lock Trava = new();
    private static bool _registrado;

    public static void Registrar()
    {
        lock (Trava)
        {
            if (_registrado) return;
            _registrado = true;

            var convencoes = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new EnumRepresentationConvention(BsonType.String),
                new IgnoreExtraElementsConvention(true)
            };
            ConventionRegistry.Register("CachacaShop", convencoes,
                t => t.Namespace?.StartsWith("CachacaShop.Domain") == true);

            // Valores monetários gravados como Decimal128 (precisão exata).
            BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));
            BsonSerializer.RegisterSerializer(new DateTimeSerializer(DateTimeKind.Utc));

            var objectIdComoString = new StringSerializer(BsonType.ObjectId);

            BsonClassMap.RegisterClassMap<Entity>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(e => e.Id)
                    .SetIdGenerator(StringObjectIdGenerator.Instance)
                    .SetSerializer(objectIdComoString);
            });

            BsonClassMap.RegisterClassMap<Produtor>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<Endereco>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<Cliente>(cm => cm.AutoMap());

            BsonClassMap.RegisterClassMap<Cachaca>(cm =>
            {
                cm.AutoMap();
                // Referência ao produtor gravada como ObjectId (permite $lookup com produtores._id).
                cm.MapMember(c => c.ProdutorId).SetSerializer(objectIdComoString);
            });

            BsonClassMap.RegisterClassMap<ItemPedido>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(i => i.CachacaId).SetSerializer(objectIdComoString);
            });

            BsonClassMap.RegisterClassMap<Pedido>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(p => p.ClienteId).SetSerializer(objectIdComoString);
                // Itens embutidos no documento do pedido (campo privado _itens -> "itens").
                cm.MapField("_itens").SetElementName("itens");
            });
        }
    }
}
