using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SpraySite
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapHubs();

            routes.MapRoute(
                name: "View Spray",
                url: "Spray/{id}",
                defaults: new { controller = "Spray", action = "ViewSpray", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Old Spray",
                url: "Gallery/View/{id}",
                defaults: new { controller = "Gallery", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}