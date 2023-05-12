using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyWeb.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {

            System.Diagnostics.Debug.WriteLine(Context.User.Identity.Name);
            if (Clients.User(user) == null)
            {
                Debug.WriteLine("null");

            }
            else
            {
                Debug.WriteLine(Clients.User(user).ToString());
            }
            await Clients.User(user).SendAsync("ReceiveMessage", message);
        }
    }
    public class UserNameProvider : IUserIdProvider
    {
        public virtual string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.Name)?.Value;
        }
    }
}
