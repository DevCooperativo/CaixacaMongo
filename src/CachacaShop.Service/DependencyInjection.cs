using Microsoft.Extensions.DependencyInjection;

namespace CachacaShop.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ProdutorService>();
        services.AddScoped<CachacaService>();
        services.AddScoped<ClienteService>();
        services.AddScoped<PedidoService>();
        services.AddScoped<ConsultaService>();
        return services;
    }
}
