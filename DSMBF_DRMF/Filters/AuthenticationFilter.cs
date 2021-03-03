using DSMBF_DRMF.Domain;
using System;
using System.Linq;
using System.Web.Mvc;

namespace DSMBF_DRMF.Filters
{
    public class AuthenticationFilterAttribute : ActionFilterAttribute, IActionFilter
    {
        public bool Disable { get; set; }
        public string ControllerName { get; set; }
        public string Type { get; set; }
        public string Language { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.ActionParameters.Count != 0 && filterContext.ActionParameters.Values.Last() != null)
            {
                var sts = filterContext.ActionParameters.Values.Last().ToString();
                if (sts == "True")
                {
                    Disable = Convert.ToBoolean(filterContext.ActionParameters.Values.Last());
                }
            }
            if (Disable) return;
            base.OnActionExecuting(filterContext);
            if (filterContext.HttpContext.Session["UserAccount"] == null)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.HttpContext.Response.StatusCode = 401;
                    filterContext.HttpContext.Response.Flush();
                }
                else
                {
                    filterContext.Result = new RedirectResult("~/Logout");
                }
            }
            else
            {
                UserAccount UA = (UserAccount)filterContext.HttpContext.Session["UserAccount"];
                if (!UA.IsPasswordChanged || !(Type != null && Type.Split(',').Contains(UA.UserRole.Name)))
                {
                    //filterContext.Result = new RedirectResult("~/Logout");
                    if (filterContext.HttpContext.Request.IsAjaxRequest())
                    {
                        filterContext.HttpContext.Response.StatusCode = 401;
                        filterContext.HttpContext.Response.Flush();
                    }
                    else
                    {
                        filterContext.Result = new RedirectResult("~/Logout");
                    }
                }
               
            }


        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Exception != null)
                filterContext.HttpContext.Trace.Write("(Logging Filter)Exception thrown");

            base.OnActionExecuted(filterContext);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);
            dumpData(filterContext, "OnResultExecuted");
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);
            dumpData(filterContext, "OnResultExecuting");
        }

        void dumpData(ControllerContext context, string filter, string actionName = "Login")
        {
        }
    }
}