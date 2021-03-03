using System.Web.Mvc;
using System.Web.Routing;

namespace DSMBF_DRMF
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
          "Forgotpassword",
          "Forgotpassword",
          new { Controller = "Login", action = "Forgotpassword" },
           namespaces: new[] { "DSMBF_DRMF.Controllers" }
      );
            routes.MapRoute(
             name: "ResetPassword",
             url: "ResetPassword/{EncDetail}",
             defaults: new { controller = "Login", action = "ResetPassword" },
             namespaces: new[] { "DSMBF_DRMF.Controllers" }
           );

            routes.MapRoute(
            name: "Login",
            url: "Login",
            defaults: new { controller = "Login", action = "Index" },
            namespaces: new[] { "DSMBF_DRMF.Controllers" }
            );
            routes.MapRoute(
           name: "Logout",
           url: "Logout",
           defaults: new { controller = "Login", action = "Logout" },
           namespaces: new[] { "DSMBF_DRMF.Controllers" }
           );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Login", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
