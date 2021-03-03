using System.Web.Mvc;

namespace DSMBF_DRMF.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {

            context.MapRoute(
       "Admin_Index",
       "Admin/Home/Index",
       new { controller = "Home", action = "Index" },
       new[] { "DSMBF_DRMF.Areas.Admin.Controllers" }
   );

            //InvoicePeriod
            context.MapRoute(
                 "Admin_Report_InvoicePeriod",
                 "Admin/Report/InvoicePeriod",
                 new { controller = "Report", action = "InvoicePeriod" },
                 new[] { "DSMBF_DRMF.Areas.Admin.Controllers" }
            );

            context.MapRoute(
   "Distributor_Index",
   "Admin/Distributor/Index",
   new { controller = "Distributor", action = "Index" },
   new[] { "DSMBF_DRMF.Areas.Admin.Controllers" }
);

            context.MapRoute(
"Distributor_View",
"Admin/Distributor/View/{EncryptedId}",
new { controller = "Distributor", action = "View" },
new[] { "DSMBF_DRMF.Areas.Admin.Controllers" }
);

            context.MapRoute(
"DistributorsBudget",
"Admin/Distributor/DistributorsBudget/{EncryptedId}",
new { controller = "Distributor", action = "DistributorsBudget" },
new[] { "DSMBF_DRMF.Areas.Admin.Controllers" }
);
            context.MapRoute(
            "PartialDistributor_Index",
            "Admin/Distributor/PartialIndex/{PageIndex}",
            new { action = "PartialIndex", controller = "Distributor" },
            new[] { "DSMBF_DRMF.Areas.Admin.Controllers" }
        );

            context.MapRoute(
          "PartialClaim_Index",
          "Admin/Claim/PartialIndex/{PageIndex}",
          new { action = "PartialIndex", controller = "Claim" },
          new[] { "DSMBF_DRMF.Areas.Admin.Controllers" }
      );

            context.MapRoute(
              "Claim_Index",
              "Admin/Claim/Index",
              new { controller = "Claim", action = "Index" },
              new[] { "DSMBF_DRMF.Areas.Admin.Controllers" }
            );

            context.MapRoute(
              "Claim_Edit",
              "Admin/Claim/Edit/{EncDetail}",
              new { controller = "Claim", action = "Edit" },
              new[] { "DSMBF_DRMF.Areas.Admin.Controllers" }
            );

            context.MapRoute(
              "Claim_View",
              "Admin/Claim/View/{EncDetail}",
              new { controller = "Claim", action = "view" },
              new[] { "DSMBF_DRMF.Areas.Admin.Controllers" }
            );

            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}