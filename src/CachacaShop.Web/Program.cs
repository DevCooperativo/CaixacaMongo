using System.Globalization;
using CachacaShop.Infrastructure;
using CachacaShop.Infrastructure.Seed;
using CachacaShop.Service;

// Binding de números/datas com cultura invariante ("59.90"); a exibição usa pt-BR (ver Helpers/Fmt).
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("MongoDb")!,
    builder.Configuration["MongoDb:Database"] ?? "cachacashop");
builder.Services.AddServices();

var app = builder.Build();

// Cria índices e popula o banco com dados de exemplo (somente se estiver vazio).
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<DataSeeder>().ExecutarAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
