# Alambique Online — E-commerce de cachaças artesanais

Trabalho 6 de Tópicos Especiais em Banco de Dados: integração do MongoDB com uma aplicação simples.

Aplicação **ASP.NET Core 10 MVC** monolítica, com arquitetura em camadas e domínio rico, usando o **MongoDB 8**
(executado via Docker Compose).

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/), com o Docker Compose

## Como executar

```bash
# 1. Subir o MongoDB (porta 27017) e o Mongo Express (http://localhost:8081)
docker compose up -d

# 2. Executar a aplicação
dotnet run --project src/CachacaShop.Web --launch-profile http
```

Acesse **http://localhost:5062**.

Na primeira execução, com o banco vazio, a aplicação cria os índices e popula o banco com dados de exemplo
(5 produtores, 12 cachaças, 5 clientes e 13 pedidos). Classe responsável: `DataSeeder`.

| Item               | Valor                                                     |
|--------------------|-----------------------------------------------------------|
| Banco              | `cachacashop`                                             |
| Usuário / senha    | `admin` / `admin123`                                      |
| Connection string  | `mongodb://admin:admin123@localhost:27017/?authSource=admin` (em `appsettings.json`) |
| Interface do banco | http://localhost:8081 (Mongo Express)                     |

### Restaurar o banco a partir do backup (opcional)

A pasta `backup/` contém um dump do banco (`cachacashop.archive.gz`) e é montada no container em `/backup`.

```bash
# restaurar (apaga e recria as coleções)
docker exec cachacashop-mongo mongorestore --uri "mongodb://admin:admin123@localhost:27017/?authSource=admin" --archive=/backup/cachacashop.archive.gz --gzip --drop

# gerar um novo backup
docker exec cachacashop-mongo mongodump --uri "mongodb://admin:admin123@localhost:27017/?authSource=admin" --db cachacashop --archive=/backup/cachacashop.archive.gz --gzip
```

### Zerar o ambiente

```bash
docker compose down -v   # remove os containers e o volume de dados
```

Na próxima execução da aplicação, os dados de exemplo são recriados.

## Arquitetura

```
src/
├── CachacaShop.Domain          Entidades ricas, regras de negócio e interfaces de repositório (sem dependências)
├── CachacaShop.Service         Casos de uso (orquestram domínio e repositórios)
├── CachacaShop.Infrastructure  MongoDB: mapeamento BSON, repositórios, índices e seed
└── CachacaShop.Web             ASP.NET Core MVC (controllers, view models e views)
```

- O domínio **não referencia o MongoDB**: o mapeamento é feito por `BsonClassMap` em
  `Infrastructure/Mongo/MongoMappings.cs`.
- As entidades têm setters privados e só mudam de estado por métodos que validam as regras de negócio.

### Regras de negócio (domínio rico)

| Entidade | Regras |
|----------|--------|
| `Cachaca` | Teor alcoólico entre 38% e 48% (legislação brasileira). Tipos Ouro, Envelhecida, Premium e Extra Premium exigem madeira. Premium exige ≥ 12 meses de envelhecimento e Extra Premium ≥ 36. Preço > 0. `BaixarEstoque` não permite estoque negativo. |
| `Cliente` | Idade mínima de 18 anos (venda de bebida alcoólica). E-mail válido e único. Endereço como *value object* (UF e CEP validados). |
| `Pedido` | Adicionar item baixa o estoque da cachaça. Guarda um *snapshot* do nome e do preço. Frete de R$ 25,00, grátis a partir de R$ 300,00. Fluxo de status `Pendente → Pago → Enviado → Entregue`. O cancelamento devolve o estoque e não é permitido após o envio. Só pedidos cancelados podem ser excluídos. |
| `Produtor` | Não pode ser removido se tiver cachaças cadastradas. |

## Modelagem do banco (4 coleções)

```js
// produtores
{ _id: ObjectId, nome, cidade, estado: "MG", anoFundacao: 1948, descricao }

// cachacas: referência ao produtor
{ _id: ObjectId, nome, produtorId: ObjectId, tipo: "Premium", madeira: "Balsamo",
  mesesEnvelhecimento: 24, teorAlcoolico: Decimal128, volumeMl: 700, preco: Decimal128,
  estoque: 40, descricao, cadastradaEm: ISODate }

// clientes: endereço embutido
{ _id: ObjectId, nome, email, dataNascimento: ISODate,
  endereco: { logradouro, cidade, estado, cep }, cadastradoEm: ISODate }

// pedidos: referência ao cliente + itens embutidos (snapshot de nome/preço)
{ _id: ObjectId, clienteId: ObjectId, nomeCliente, estadoEntrega, data: ISODate, status: "Pago",
  subtotal: Decimal128, frete: Decimal128, total: Decimal128,
  itens: [ { cachacaId: ObjectId, nomeCachaca, precoUnitario: Decimal128, quantidade, subtotal } ] }
```

Índices: `cachacas.produtorId`, `cachacas.{tipo, preco}`, `cachacas.estoque`, `clientes.email` (único),
`pedidos.{clienteId, data}`, `pedidos.itens.cachacaId` e `pedidos.{status, data}`.

## CRUD

| Coleção    | Inserir | Listar | Atualizar | Remover |
|------------|:-------:|:------:|:---------:|:-------:|
| produtores | ✔ | ✔ | ✔ (editar) | ✔ |
| cachacas   | ✔ | ✔ | ✔ (editar, repor estoque) | ✔ |
| clientes   | ✔ | ✔ | ✔ (editar) | ✔ |
| pedidos    | ✔ | ✔ | ✔ (pagar, enviar, entregar, cancelar) | ✔ |

## Buscas (menu "Consultas")

| # | Busca | Operações MongoDB | Implementação |
|---|-------|-------------------|---------------|
| 1 | Catálogo por nome (parcial), tipo, madeira, faixa de preço e disponibilidade | `find` com `$regex` (case-insensitive), `$eq`, `$gte`/`$lte`, `$gt`, `sort` | `CachacaRepository.BuscarCatalogoAsync` |
| 2 | Cachaças por estado (UF) do produtor | `aggregate`: `$lookup` (cachacas × produtores), `$unwind`, `$match`, `$sort`, `$project` | `CachacaRepository.BuscarPorEstadoDoProdutorAsync` |
| 3 | Produtos com estoque baixo | `find` com `$lte` e ordenação por estoque | `CachacaRepository.BuscarEstoqueBaixoAsync` |
| 4 | Pedidos de um cliente por status e período | `find` em `clienteId`, `status` e intervalo de `data` | `PedidoRepository.BuscarPorClienteAsync` |
| 5 | Ranking das cachaças mais vendidas | `aggregate`: `$match`, `$unwind` itens, `$group` + `$sum`, `$sort`, `$limit` | `PedidoRepository.MaisVendidasAsync` |
| 6 | Faturamento mensal (pedidos, total e ticket médio) | `aggregate`: `$match` (`$in` status + data), `$group` por `$month`, `$sum`, `$avg` | `PedidoRepository.FaturamentoMensalAsync` |
