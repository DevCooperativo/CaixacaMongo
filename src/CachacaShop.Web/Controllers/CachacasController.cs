using CachacaShop.Domain.Common;
using CachacaShop.Service;
using CachacaShop.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CachacaShop.Web.Controllers;

public class CachacasController(CachacaService service, ProdutorService produtores) : BaseController
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Produtores = (await produtores.ListarAsync()).ToDictionary(p => p.Id, p => p.Nome);
        return View(await service.ListarAsync());
    }

    public async Task<IActionResult> Create() => await Formulario(new CachacaForm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CachacaForm form)
    {
        if (!ModelState.IsValid) return await Formulario(form);
        try
        {
            await service.CriarAsync(form.ParaDados());
            Sucesso("Cachaça cadastrada com sucesso.");
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return await Formulario(form);
        }
    }

    public async Task<IActionResult> Edit(string id)
    {
        var cachaca = await service.ObterAsync(id);
        return cachaca is null ? NotFound() : await Formulario(CachacaForm.De(cachaca));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, CachacaForm form)
    {
        if (!ModelState.IsValid) return await Formulario(form);
        try
        {
            await service.AtualizarAsync(id, form.ParaDados());
            Sucesso("Cachaça atualizada com sucesso.");
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return await Formulario(form);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> ReporEstoque(string id, int quantidade) =>
        ExecutarERedirecionar(() => service.ReporEstoqueAsync(id, quantidade), "Estoque reposto.");

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Delete(string id) =>
        ExecutarERedirecionar(() => service.RemoverAsync(id), "Cachaça removida.");

    private async Task<IActionResult> Formulario(CachacaForm form)
    {
        ViewBag.Produtores = new SelectList(await produtores.ListarAsync(), "Id", "Nome", form.ProdutorId);
        return View("Form", form);
    }
}
