using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Notifications.Helpers.Notification
{
    public interface INotificationDataHelper
    {
        Task<bool> SendNotification(ConversationReference conversationReference);
    }
}
