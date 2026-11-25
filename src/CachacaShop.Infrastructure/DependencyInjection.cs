using CachacaShop.Domain;
using CachacaShop.Infrastructure.Mongo;
using CachacaShop.Infrastructure.Repositories;
using CachacaShop.Infrastructure.Seed;
using Microsoft.Extensions.DependencyInjection;

namespace CachacaShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString,
        string databaseName)
    {
        services.AddSingleton(new MongoContext(connectionString, databaseName));
        services.AddScoped<IProdutorRepository, ProdutorRepository>();
        services.AddScoped<ICachacaRepository, CachacaRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<DataSeeder>();
        return services;
    }
}
