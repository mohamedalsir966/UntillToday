using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Cxunicorn.Common.Services.BotNotification.NotificationService;

namespace Cxunicorn.Common.Services.BotNotification
{
    public interface INotificationService
    {
        Task<SendMessageResponse> SendAsync(string serviceUrl,
                    string conversationId,
                    NotificationActionType notificationActionType,
                    IMessageActivity? message = null,
                    string? activityIdForDelete = null);
    }
}
