using CachacaShop.Domain.Cachacas;
using CachacaShop.Domain.Clientes;
using CachacaShop.Domain.Pedidos;
using CachacaShop.Domain.Produtores;
using CachacaShop.Infrastructure.Mongo;
using MongoDB.Driver;

namespace CachacaShop.Infrastructure.Seed;

/// <summary>Popula o banco com dados de exemplo na primeira execução (banco vazio).</summary>
public class DataSeeder(MongoContext ctx)
{
    public async Task ExecutarAsync()
    {
        await ctx.CriarIndicesAsync();
        if (await ctx.Produtores.Find(FilterDefinition<Produtor>.Empty).AnyAsync()) return;

        // ---------- Produtores ----------
        var serraVerde = new Produtor("Alambique Serra Verde", "Salinas", "MG", 1948,
            "Alambique familiar no norte de Minas, referência em cachaças envelhecidas em bálsamo.");
        var vale = new Produtor("Engenho Vale do Sol", "Paraty", "RJ", 1975,
            "Engenho histórico do litoral fluminense, produção em alambique de cobre.");
        var chapada = new Produtor("Destilaria Chapada Dourada", "Abaíra", "BA", 1992,
            "Cachaças da Chapada Diamantina com cana orgânica.");
        var colonial = new Produtor("Alambique Colonial", "Luiz Alves", "SC", 1960,
            "Tradição de imigrantes italianos no Vale do Itajaí.");
        var morro = new Produtor("Engenho Morro Alto", "Morretes", "PR", 2005,
            "Pequeno produtor do litoral paranaense, especialista em amburana.");
        Produtor[] produtores = [serraVerde, vale, chapada, colonial, morro];
        await ctx.Produtores.InsertManyAsync(produtores);

        // ---------- Cachaças ----------
        Cachaca C(string nome, Produtor p, TipoCachaca tipo, Madeira madeira, int meses, decimal teor, int ml,
            decimal preco, int estoque, string descricao) =>
            new(nome, p.Id, tipo, madeira, meses, teor, ml, preco, estoque, descricao);

        var cachacas = new[]
        {
            C("Serra Verde Prata", serraVerde, TipoCachaca.Prata, Madeira.Nenhuma, 0, 42, 700, 59.90m, 80,
                "Descansada em inox, leve e frutada."),
            C("Serra Verde Bálsamo 2 anos", serraVerde, TipoCachaca.Premium, Madeira.Balsamo, 24, 43, 700, 139.90m, 40,
                "Notas de anis e especiarias."),
            C("Serra Verde Reserva 6 anos", serraVerde, TipoCachaca.ExtraPremium, Madeira.Carvalho, 72, 44, 700,
                389.00m, 12, "Edição limitada, carvalho francês."),
            C("Vale do Sol Tradicional", vale, TipoCachaca.Prata, Madeira.Nenhuma, 0, 40, 600, 49.90m, 100,
                "Clássica para caipirinha."),
            C("Vale do Sol Ouro", vale, TipoCachaca.Ouro, Madeira.Jequitiba, 6, 40, 600, 69.90m, 6,
                "Armazenada em jequitibá-rosa."),
            C("Vale do Sol Gabriela", vale, TipoCachaca.Envelhecida, Madeira.Carvalho, 12, 39, 600, 79.90m, 30,
                "Com cravo e canela, tradição de Paraty."),
            C("Chapada Orgânica Prata", chapada, TipoCachaca.Prata, Madeira.Nenhuma, 0, 41, 750, 64.90m, 50,
                "Cana orgânica, fermentação natural."),
            C("Chapada Umburana", chapada, TipoCachaca.Envelhecida, Madeira.Amburana, 18, 42, 750, 119.90m, 4,
                "Aroma de baunilha e cumaru."),
            C("Colonial Castanheira", colonial, TipoCachaca.Envelhecida, Madeira.Castanheira, 12, 40, 700, 89.90m, 25,
                "Madeira nativa da Amazônia, final amendoado."),
            C("Colonial Ipê Premium", colonial, TipoCachaca.Premium, Madeira.Ipe, 30, 41, 700, 159.90m, 15,
                "Encorpada, taninos marcantes."),
            C("Morro Alto Amburana", morro, TipoCachaca.Envelhecida, Madeira.Amburana, 12, 40, 700, 74.90m, 35,
                "Doce e aromática."),
            C("Morro Alto Extra Premium 4 anos", morro, TipoCachaca.ExtraPremium, Madeira.Amburana, 48, 42, 500,
                249.90m, 3, "Barril único, numerada.")
        };
        await ctx.Cachacas.InsertManyAsync(cachacas);

        // ---------- Clientes ----------
        var clientes = new[]
        {
            new Cliente("Ana Souza", "ana.souza@email.com", new DateTime(1990, 3, 15),
                new Endereco("Rua das Flores, 120", "Vitória", "ES", "29050000")),
            new Cliente("Bruno Lima", "bruno.lima@email.com", new DateTime(1985, 7, 2),
                new Endereco("Av. Paulista, 1500", "São Paulo", "SP", "01310100")),
            new Cliente("Carla Mendes", "carla.mendes@email.com", new DateTime(1978, 11, 23),
                new Endereco("Rua da Bahia, 900", "Belo Horizonte", "MG", "30160011")),
            new Cliente("Diego Rocha", "diego.rocha@email.com", new DateTime(2000, 1, 30),
                new Endereco("Rua XV de Novembro, 50", "Curitiba", "PR", "80020310")),
            new Cliente("Elisa Martins", "elisa.martins@email.com", new DateTime(1995, 5, 8),
                new Endereco("Av. Atlântica, 300", "Rio de Janeiro", "RJ", "22010000"))
        };
        await ctx.Clientes.InsertManyAsync(clientes);

        // ---------- Pedidos (distribuídos ao longo do ano) ----------
        var ano = DateTime.UtcNow.Year;
        var pedidos = new List<Pedido>();

        Pedido P(Cliente cliente, int mes, int dia, StatusPedido status, params (Cachaca c, int qtd)[] itens)
        {
            var pedido = new Pedido(cliente, new DateTime(ano, mes, dia, 14, 0, 0, DateTimeKind.Utc));
            foreach (var (c, qtd) in itens) pedido.AdicionarItem(c, qtd);
            if (status is StatusPedido.Pago or StatusPedido.Enviado or StatusPedido.Entregue) pedido.Pagar();
            if (status is StatusPedido.Enviado or StatusPedido.Entregue) pedido.Enviar();
            if (status is StatusPedido.Entregue) pedido.Entregar();
            if (status is StatusPedido.Cancelado)
                foreach (var item in pedido.Cancelar())
                    cachacas.First(c => c.Id == item.CachacaId).ReporEstoque(item.Quantidade);
            pedidos.Add(pedido);
            return pedido;
        }

        var mesAtual = Math.Min(DateTime.UtcNow.Month, 12);
        int Mes(int m) => Math.Min(m, mesAtual);

        P(clientes[0], Mes(1), 10, StatusPedido.Entregue, (cachacas[0], 2), (cachacas[1], 1));
        P(clientes[1], Mes(1), 22, StatusPedido.Entregue, (cachacas[3], 6));
        P(clientes[2], Mes(2), 5, StatusPedido.Entregue, (cachacas[2], 1), (cachacas[7], 1));
        P(clientes[3], Mes(3), 18, StatusPedido.Entregue, (cachacas[10], 3), (cachacas[6], 2));
        P(clientes[4], Mes(4), 2, StatusPedido.Entregue, (cachacas[5], 2), (cachacas[3], 2));
        P(clientes[0], Mes(5), 14, StatusPedido.Entregue, (cachacas[9], 1), (cachacas[8], 2));
        P(clientes[1], Mes(6), 9, StatusPedido.Cancelado, (cachacas[2], 2));
        P(clientes[2], Mes(7), 21, StatusPedido.Entregue, (cachacas[1], 3), (cachacas[0], 4));
        P(clientes[3], Mes(8), 30, StatusPedido.Entregue, (cachacas[11], 1));
        P(clientes[4], Mes(9), 12, StatusPedido.Enviado, (cachacas[3], 4), (cachacas[10], 2));
        P(clientes[0], Mes(10), 3, StatusPedido.Pago, (cachacas[6], 3));
        P(clientes[1], Mes(11), 15, StatusPedido.Pago, (cachacas[4], 2), (cachacas[5], 1));
        P(clientes[2], Mes(11), 20, StatusPedido.Pendente, (cachacas[0], 1));

        await ctx.Pedidos.InsertManyAsync(pedidos);
        // Persiste o estoque já baixado pelos pedidos.
        foreach (var cachaca in cachacas)
            await ctx.Cachacas.ReplaceOneAsync(c => c.Id == cachaca.Id, cachaca);
    }
}
