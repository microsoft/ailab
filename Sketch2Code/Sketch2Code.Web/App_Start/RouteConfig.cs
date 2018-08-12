using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Sketch2Code.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "Detail",
                url: "details/{id}",
                defaults: new { controller = "app", action="details" ,id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ready-to-start",
                url: "ready-to-start",
                defaults: new { controller = "app", action = "Step2", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "upload",
                url: "upload",
                defaults: new { controller = "app", action = "Upload", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "work-in-progress",
                url: "work-in-progress",
                defaults: new { controller = "app", action = "Step3", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "finished",
                url: "finished/{id}",
                defaults: new { controller = "app", action = "Step4", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "generated-html",
                url: "generated-html/{id}",
                defaults: new { controller = "app", action = "Step5", id = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "camera",
               url: "take-snapshot",
               defaults: new { controller = "app", action = "TakeSnapshot", id = UrlParameter.Optional }
           );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "app", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
