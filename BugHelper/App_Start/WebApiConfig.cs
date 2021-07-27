using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace BugHelper.App_Start
{
        public static class WebApiConfig
        {
            public static void Register(HttpConfiguration config)
            {
                // TODO: Add any additional configuration code.

                // Web API routes
                config.MapHttpAttributeRoutes();

                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{action}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );

            // WebAPI when dealing with JSON & JavaScript!
            // Setup json serialization to serialize classes to camel (std. Json format)
            config.Formatters.Remove(config.Formatters.XmlFormatter);
                var formatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
                formatter.SerializerSettings.ContractResolver =
                    new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            }
        }
}