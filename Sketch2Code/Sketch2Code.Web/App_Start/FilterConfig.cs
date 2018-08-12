using System.Web;
using System.Web.Mvc;

namespace Sketch2Code.Web{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ErrorHandler.AiHandleErrorAttribute());
            if(!HttpContext.Current.IsDebuggingEnabled)
                filters.Add(new RequireHttpsAttribute(true));
        }
    }
}