using System;

namespace CampusKart.Data
{
    public class ChatNotificationService
    {
        public event Action<string, string>? OnMessageNotification;

        public void NotifyMessageSent(string senderEmail, string receiverEmail)
        {
            OnMessageNotification?.Invoke(senderEmail, receiverEmail);
        }
    }
}
