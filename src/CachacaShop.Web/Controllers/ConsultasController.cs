using CachacaShop.Domain.Consultas;
using CachacaShop.Service;
using CachacaShop.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CachacaShop.Web.Controllers;

/// <summary>As buscas exigidas pelo trabalho. Todas usam GET com os filtros na query string.</summary>
public class ConsultasController(ConsultaService consultas, ProdutorService produtores, ClienteService clientes)
    : BaseController
{
    public IActionResult Index() => View();

    public async Task<IActionResult> Catalogo(CatalogoViewModel vm)
    {
        vm.Resultado = await consultas.CatalogoAsync(new FiltroCatalogo(vm.Nome, vm.Tipo, vm.Madeira,
            vm.PrecoMinimo, vm.PrecoMaximo, vm.SomenteDisponiveis));
        vm.Produtores = (await produtores.ListarAsync()).ToDictionary(p => p.Id, p => p.Nome);
        return View(vm);
    }

    public async Task<IActionResult> PorEstado(PorEstadoViewModel vm)
    {
        ViewBag.Estados = (await produtores.ListarAsync()).Select(p => p.Estado).Distinct().Order()
            .Select(e => new SelectListItem(e, e, e == vm.Estado)).ToList();
        if (!string.IsNullOrWhiteSpace(vm.Estado))
            vm.Resultado = await consultas.PorEstadoAsync(vm.Estado);
        return View(vm);
    }

    public async Task<IActionResult> EstoqueBaixo(EstoqueBaixoViewModel vm)
    {
        vm.Resultado = await consultas.EstoqueBaixoAsync(vm.Limite);
        return View(vm);
    }

    public async Task<IActionResult> PedidosCliente(PedidosClienteViewModel vm)
    {
        ViewBag.Clientes = new SelectList(await clientes.ListarAsync(), "Id", "Nome", vm.ClienteId);
        if (!string.IsNullOrEmpty(vm.ClienteId))
            vm.Resultado = await consultas.PedidosDoClienteAsync(vm.ClienteId, vm.Status, vm.De, vm.Ate);
        return View(vm);
    }

    public async Task<IActionResult> MaisVendidas(MaisVendidasViewModel vm)
    {
        vm.Resultado = await consultas.MaisVendidasAsync(Math.Clamp(vm.Top, 1, 50));
        return View(vm);
    }

    public async Task<IActionResult> Faturamento(FaturamentoViewModel vm)
    {
        vm.Resultado = await consultas.FaturamentoMensalAsync(vm.Ano);
        return View(vm);
    }
}
