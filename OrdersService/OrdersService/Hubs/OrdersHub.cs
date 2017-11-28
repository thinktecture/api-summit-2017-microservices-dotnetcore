using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace OrdersService.Hubs
{
    [Authorize]
    public class OrdersHub : Hub
    {
        public override Task OnConnected()
        {
            var identity = Context.User.Identity as ClaimsIdentity;
            var subjectId = identity?.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            Groups.Add(Context.ConnectionId, subjectId);

            return base.OnConnected();
        }
    }
}
