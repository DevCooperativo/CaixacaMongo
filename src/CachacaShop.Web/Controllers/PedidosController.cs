using CachacaShop.Domain.Common;
using CachacaShop.Service;
using CachacaShop.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CachacaShop.Web.Controllers;

public class PedidosController(PedidoService service, ClienteService clientes, CachacaService cachacas)
    : BaseController
{
    public async Task<IActionResult> Index() => View(await service.ListarAsync());

    public async Task<IActionResult> Details(string id)
    {
        var pedido = await service.ObterAsync(id);
        return pedido is null ? NotFound() : View(pedido);
    }

    public async Task<IActionResult> Create() => await Formulario(new PedidoForm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PedidoForm form)
    {
        if (!ModelState.IsValid) return await Formulario(form);
        try
        {
            var pedido = await service.CriarAsync(form.ParaDados());
            Sucesso("Pedido criado com sucesso.");
            return RedirectToAction(nameof(Details), new { id = pedido.Id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return await Formulario(form);
        }
    }

    /// <summary>Atualização do pedido: avança o status (pagar, enviar, entregar) ou cancela.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Acao(string id, AcaoPedido acao) =>
        ExecutarERedirecionar(() => service.ExecutarAcaoAsync(id, acao), "Pedido atualizado.",
            nameof(Details), new { id });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await service.RemoverAsync(id);
            Sucesso("Pedido excluído.");
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            Erro(ex.Message);
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    private async Task<IActionResult> Formulario(PedidoForm form)
    {
        ViewBag.Clientes = new SelectList(await clientes.ListarAsync(), "Id", "Nome", form.ClienteId);
        ViewBag.Cachacas = (await cachacas.ListarAsync()).Where(c => c.Disponivel).ToList();
        return View("Create", form);
    }
}
