using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.IO;
using System.Web.Routing;
using System.Data.SqlClient;
using System.Data;


namespace DSMBF_DRMF
{
    public class ErrorLoggerAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            LogError(filterContext);
            string message = string.Empty;

            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                message = filterContext.Exception.Message;
                filterContext.HttpContext.Response.StatusCode = 500;
                var json = new JsonResult { Data = message };
                json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                filterContext.Result = json;
            }
            else

            {
                ViewResult vr = new ViewResult();
                vr.ViewName = "~/Views/Shared/Error.cshtml";
                vr.ViewData.Add("Error", filterContext.Exception);
                filterContext.Result = vr;
            }
            if (filterContext.ExceptionHandled)
            {
                return;
            }
            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
        }

        public void LogError(ExceptionContext filterContext)
        {
            if (filterContext != null && filterContext.Exception != null)
            {
                string controller = filterContext.RouteData.Values["controller"].ToString();
                string action = filterContext.RouteData.Values["action"].ToString();
                string loggerName = string.Format("{0}Controller.{1}", controller, action);
                log4net.LogManager.GetLogger(loggerName).Error(string.Empty, filterContext.Exception);
            }
        }
    }
}