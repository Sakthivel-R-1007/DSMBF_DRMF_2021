using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace DSMBF_DRMF.Helpers
{
    public class EmailTemplate
    {
        public static string RenderViewToString<T>(ControllerBase controller, string viewName, T model)
        {
            using (var writer = new StringWriter())
            {
                ViewEngineResult result = ViewEngines
                          .Engines
                          .FindView(controller.ControllerContext,
                                    viewName, null);

                var viewPath = ((RazorView)result.View);
                var view = new RazorView(controller.ControllerContext, viewPath.ViewPath, null, false, null);
                var vdd = new ViewDataDictionary<T>(model);
                var viewCxt = new ViewContext(
                                    controller.ControllerContext,
                                    view,
                                    vdd,
                                    new TempDataDictionary(), writer);
                viewCxt.View.Render(viewCxt, writer);
                return writer.ToString();
            }
        }

    }
}
