using CachacaShop.Domain.Common;
using CachacaShop.Service;
using CachacaShop.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace CachacaShop.Web.Controllers;

public class ClientesController(ClienteService service) : BaseController
{
    public async Task<IActionResult> Index() => View(await service.ListarAsync());

    public IActionResult Create() => View("Form", new ClienteForm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClienteForm form)
    {
        if (!ModelState.IsValid) return View("Form", form);
        try
        {
            await service.CriarAsync(form.ParaDados());
            Sucesso("Cliente cadastrado com sucesso.");
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Form", form);
        }
    }

    public async Task<IActionResult> Edit(string id)
    {
        var cliente = await service.ObterAsync(id);
        return cliente is null ? NotFound() : View("Form", ClienteForm.De(cliente));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, ClienteForm form)
    {
        if (!ModelState.IsValid) return View("Form", form);
        try
        {
            await service.AtualizarAsync(id, form.ParaDados());
            Sucesso("Cliente atualizado com sucesso.");
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Form", form);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Delete(string id) =>
        ExecutarERedirecionar(() => service.RemoverAsync(id), "Cliente removido.");
}
