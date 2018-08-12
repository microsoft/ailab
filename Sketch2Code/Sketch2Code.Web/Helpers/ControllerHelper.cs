using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sketch2Code.Web.Helpers
{
    public static class ControllerHelper
    {
        public static string RenderViewToString<T>(this Controller controller, string viewPath, T model)
        {
            using (var writer = new StringWriter())
            {
                var view = new RazorView(controller.ControllerContext, viewPath, "/Views/Shared/_FlatLayout.cshtml", false, null);
                var vdd = new ViewDataDictionary<T>(model);
                var viewCxt = new ViewContext(controller.ControllerContext, view, vdd,
                                            new TempDataDictionary(), writer);
                viewCxt.View.Render(viewCxt, writer);
                return writer.ToString();
            }
        }

        public static string RenderViewToString2<T>(this Controller controller, string viewPath, T model)
        {
            using (var writer = new StringWriter())
            {
                var view = new RazorView(controller.ControllerContext, viewPath, null, false, null);
                var vdd = new ViewDataDictionary<T>(model);
                var viewCxt = new ViewContext(controller.ControllerContext, view, vdd,
                                            new TempDataDictionary(), writer);
                viewCxt.View.Render(viewCxt, writer);
                return writer.ToString();
            }
        }
    }
}