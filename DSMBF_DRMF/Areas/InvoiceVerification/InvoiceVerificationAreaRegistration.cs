using System.Web.Mvc;

namespace DSMBF_DRMF.Areas.InvoiceVerification
{
    public class InvoiceVerificationAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "InvoiceVerification";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
         "InvoiceVerification_Index",
         "InvoiceVerification/Verification/Index",
         new { controller = "Verification", action = "Index" },
         new[] { "DSMBF_DRMF.Areas.InvoiceVerification.Controllers" }
     );

            context.MapRoute(
                "InvoiceVerification_default",
                "InvoiceVerification/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}