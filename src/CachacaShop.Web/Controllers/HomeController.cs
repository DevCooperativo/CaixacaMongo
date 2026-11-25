using System.Diagnostics;
using CachacaShop.Service;
using CachacaShop.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace CachacaShop.Web.Controllers;

public class HomeController(ConsultaService consultas) : Controller
{
    public async Task<IActionResult> Index()
    {
        var vitrine = await consultas.CatalogoAsync(new(SomenteDisponiveis: true));
        return View(vitrine.OrderByDescending(c => c.Preco).Take(6).ToList());
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
