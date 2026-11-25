using System.Globalization;
using CachacaShop.Domain.Cachacas;
using CachacaShop.Domain.Common;
using CachacaShop.Domain.Pedidos;
using CachacaShop.Service;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CachacaShop.Web.Helpers;

/// <summary>Formatação em pt-BR e rótulos amigáveis para os enums do domínio.</summary>
public static class Fmt
{
    private static readonly CultureInfo Br = new("pt-BR");

    public static string Moeda(decimal valor) => valor.ToString("C", Br);
    public static string Data(DateTime data) => data.ToLocalTime().ToString("dd/MM/yyyy", Br);
    public static string DataHora(DateTime data) => data.ToLocalTime().ToString("dd/MM/yyyy HH:mm", Br);
    public static string Percentual(decimal valor) => valor.ToString("0.#", Br) + "%";
    public static string MesAno(int ano, int mes) => new DateTime(ano, mes, 1).ToString("MMMM/yyyy", Br);

    public static string Rotulo(TipoCachaca tipo) => tipo switch
    {
        TipoCachaca.ExtraPremium => "Extra Premium",
        _ => tipo.ToString()
    };

    public static string Rotulo(Madeira madeira) => madeira switch
    {
        Madeira.Nenhuma => "Sem madeira (inox)",
        Madeira.Jequitiba => "Jequitibá",
        Madeira.Balsamo => "Bálsamo",
        Madeira.Ipe => "Ipê",
        _ => madeira.ToString()
    };

    public static string Rotulo(StatusPedido status) => status.ToString();

    public static string Rotulo(AcaoPedido acao) => acao switch
    {
        AcaoPedido.Pagar => "Confirmar pagamento",
        AcaoPedido.Enviar => "Marcar como enviado",
        AcaoPedido.Entregar => "Marcar como entregue",
        _ => "Cancelar pedido"
    };

    public static string CorStatus(StatusPedido status) => status switch
    {
        StatusPedido.Pendente => "warning",
        StatusPedido.Pago => "info",
        StatusPedido.Enviado => "primary",
        StatusPedido.Entregue => "success",
        _ => "secondary"
    };

    public static IEnumerable<SelectListItem> Tipos() =>
        Enum.GetValues<TipoCachaca>().Select(t => new SelectListItem(Rotulo(t), t.ToString()));

    public static IEnumerable<SelectListItem> Madeiras() =>
        Enum.GetValues<Madeira>().Select(m => new SelectListItem(Rotulo(m), m.ToString()));

    public static IEnumerable<SelectListItem> Status() =>
        Enum.GetValues<StatusPedido>().Select(s => new SelectListItem(Rotulo(s), s.ToString()));

    public static IEnumerable<SelectListItem> Ufs() => Uf.Todas.Select(u => new SelectListItem(u, u));
}
