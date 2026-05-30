using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CampusKart.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string receiverEmail, string messageJson)
        {
            if (!string.IsNullOrEmpty(receiverEmail))
            {
                // Broadcast to the recipient's private WebSocket group in real time!
                await Clients.Group(receiverEmail.ToLower().Trim()).SendAsync("ReceiveMessage", messageJson);
            }
        }

        public async Task JoinChatGroup(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                // Add this connection to the user's private group (named after their Email address)
                await Groups.AddToGroupAsync(Context.ConnectionId, email.ToLower().Trim());
            }
        }
    }
}
