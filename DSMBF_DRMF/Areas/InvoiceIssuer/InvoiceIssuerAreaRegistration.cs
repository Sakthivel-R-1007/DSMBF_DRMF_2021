using System.Web.Mvc;

namespace DSMBF_DRMF.Areas.InvoiceIssuer
{
    public class InvoiceIssuerAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "InvoiceIssuer";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {

            context.MapRoute(
            "InvoiceIssuer_Index",
            "InvoiceIssuer/Invoice/Index",
            new { controller = "Invoice", action = "Index" },
            new[] { "DSMBF_DRMF.Areas.InvoiceIssuer.Controllers" }
        );
            context.MapRoute(
                "InvoiceIssuer_default",
                "InvoiceIssuer/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}