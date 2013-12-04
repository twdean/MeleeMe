using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Melee.Me
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "MyProfile",
                url: "Home/MyProfile/{twitterUserId}",
                defaults: new { controller = "Home", action = "MyProfile", twitterUserId = "" }
            );

            routes.MapRoute(
                name: "Facebook",
                url: "Connections/Facebook/{twitterUserId}",
                defaults: new { controller = "Connections", action = "Facebook", twitterUserId = "" }
            );

            routes.MapRoute(
                name: "Google",
                url: "Connections/Google/{twitterUserId}",
                defaults: new { controller = "Connections", action = "Google", twitterUserId = "" }
            );


            routes.MapRoute(
            name: "MeleeMe",
            url: "Melee/MeleeMe/{twitterId}",
            defaults: new { controller = "Melee", action = "MeleeMe", twitterId = "" }
            );

            routes.MapRoute(
            name: "Login",
            url: "Home/Login/{twitterUserId}",
            defaults: new { controller = "Home", action = "Login", twitterUserId = "" }
            );


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

        }
    }
}