using System.Web.Http;

namespace OrdersService.Controllers
{
    [RoutePrefix("api/health")]
    public class HealthController : ApiController
    {
        [HttpGet]
        [Route("ping")]
        [AllowAnonymous]
        public string Ping()
        {
            return "OK";
        }
    }
}