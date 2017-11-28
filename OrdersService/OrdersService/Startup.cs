using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Web.Http;
using System.Web.Http.Cors;
using IdentityServer3.AccessTokenValidation;
using Microsoft.Owin.Cors;
using OrdersService.AuthZ;
using OrdersService.Logging;
using OrdersService.Properties;
using Owin;
using Swashbuckle.Application;

namespace OrdersService
{
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {   
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            app.Use(typeof(SignalRAuthorizationMiddleware));

            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = Settings.Default.IdSrvBaseUrl,
                RequiredScopes = new List<string> { "ordersapi" }
            });

            app.UseCors(CorsOptions.AllowAll);

            var config = new HttpConfiguration();
            config.MessageHandlers.Add(new MessageLoggingHandler());
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));

            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{action}");

            config.EnableSwagger(c => c.SingleApiVersion("v1", "Orders Service API"))
                  .EnableSwaggerUi();

            config.Filters.Add(new AuthorizeAttribute());

            config.EnsureInitialized();

            app.UseWebApi(config);
            app.MapSignalR();
        }
    }
}
