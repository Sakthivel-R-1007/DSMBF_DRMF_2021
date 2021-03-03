using DSMBF_DRMF.App_Start;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace DSMBF_DRMF
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            UnityConfig.RegisterComponents();
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_End()
        {

        }

        void Application_BeginRequest(Object source, EventArgs e)
        {
            HttpApplication app = (HttpApplication)source;
            HttpContext context = app.Context;
            Application["ApplicationPath"] = FirstRequestInitialisation.Initialise(context);

            //if (!HttpContext.Current.Request.Url.OriginalString.Contains("https"))
            //    Response.Redirect(Application["ApplicationPath"].ToString());


        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current.User != null)
            {
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    if (HttpContext.Current.User.Identity is FormsIdentity)
                    {
                        FormsIdentity identity = (FormsIdentity)HttpContext.Current.User.Identity;
                        FormsAuthenticationTicket ticket = identity.Ticket;
                        string[] roles = ticket.UserData.Split(',');
                        HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(identity, roles);
                    }
                }
            }
        }

        class FirstRequestInitialisation
        {
            private static string host = null;
            private static Object s_lock = new Object();
            public static string Initialise(HttpContext context)
            {
                if (string.IsNullOrEmpty(host))
                {
                    lock (s_lock)
                    {
                        if (string.IsNullOrEmpty(host))
                        {
                            Uri uri = HttpContext.Current.Request.Url;

                            //Local
                             host = uri.Scheme + Uri.SchemeDelimiter + uri.Host + ":" + uri.Port;

                            //Staging
                            //host = "http" + Uri.SchemeDelimiter + uri.Host;

                            //Live
                            //host = "https" + Uri.SchemeDelimiter + uri.Host;

                        }
                    }
                }
                return host;
            }
        }
    }
}
