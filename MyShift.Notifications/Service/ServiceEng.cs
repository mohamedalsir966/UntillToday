using AdaptiveCards;
using Azure.Storage.Queues;
using Cxunicorn.Common.Middlewares.TeamsActivityHandler.Users;
using Cxunicorn.Common.Services.BotNotification;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MyShift.Notifications.Entitys;
using MyShift.Notifications.Helpers.Notification;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static Cxunicorn.Common.Services.BotNotification.NotificationService;

namespace MyShift.Notifications.Service
{
    public class ServiceEng : IService
    {
        private readonly ILogsRepository _logsRepository;
        private readonly INotificationDataHelper _notificationDataHelper;
        public ServiceEng( ILogsRepository logsRepository , INotificationDataHelper notificationDataHelper)
        {
            _logsRepository = logsRepository;
            _notificationDataHelper = notificationDataHelper;
        }
        public async Task<string> GetDataToNotifiy()
        {
            var dataTobeNotifyed = await _logsRepository.GetLogsQueries();

            foreach (var item in dataTobeNotifyed)
            {
                //we need to get it by the user how ??
                var conversationReference = await _logsRepository.GetConversationReference();
                //her we just need to send it now data back??
                var result = await _notificationDataHelper.SendNotification(conversationReference);
            }

           // var updateData = await _logsRepository.UpdateLogsCommand(dataTobeNotifyed);
           
            var queueMessage = JsonConvert.SerializeObject(dataTobeNotifyed);
            return queueMessage;
        }

    }
}
