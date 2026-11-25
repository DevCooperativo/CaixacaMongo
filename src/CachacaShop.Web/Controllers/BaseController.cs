using CachacaShop.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace CachacaShop.Web.Controllers;

public abstract class BaseController : Controller
{
    protected void Sucesso(string mensagem) => TempData["Sucesso"] = mensagem;
    protected void Erro(string mensagem) => TempData["Erro"] = mensagem;

    /// <summary>Executa uma ação que pode violar regras de domínio e redireciona exibindo o resultado.</summary>
    protected async Task<IActionResult> ExecutarERedirecionar(Func<Task> acao, string mensagemSucesso,
        string action = "Index", object? rotas = null)
    {
        try
        {
            await acao();
            Sucesso(mensagemSucesso);
        }
        catch (DomainException ex)
        {
            Erro(ex.Message);
        }
        return RedirectToAction(action, rotas);
    }
}
