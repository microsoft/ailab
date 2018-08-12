using System;
using System.Web.Mvc;
using Microsoft.ApplicationInsights;

namespace Sketch2Code.Web.ErrorHandler
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class AiHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext != null && filterContext.HttpContext != null && filterContext.Exception != null)
            {
                var ai = new TelemetryClient();
                ai.TrackException(filterContext.Exception);
            }
            base.OnException(filterContext);
        }
    }
}