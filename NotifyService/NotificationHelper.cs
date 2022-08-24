using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotifyService
{
    public class NotificationHelper : INotificationHelper
    {
        //private readonly IBotFrameworkHttpAdapter _adapter;
        //private readonly string _appId;
        //private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        public Task QueueUpcomingShiftStartNotifications()
        {
            throw new NotImplementedException();
        }
    }
}
