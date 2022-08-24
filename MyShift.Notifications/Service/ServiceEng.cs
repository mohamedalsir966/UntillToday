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
        private readonly IConfiguration _configuration;
        private readonly ILogsRepository _logsRepository;
        private readonly INotificationService _notificationService;
        private readonly IUsersDataRepository _usersDataRepository;
        public ServiceEng(IConfiguration configuration, ILogsRepository logsRepository ,INotificationService notificationService,IUsersDataRepository usersDataRepository)
        {
            _configuration = configuration;
            _logsRepository = logsRepository;
            _notificationService = notificationService;
            _usersDataRepository = usersDataRepository;
        }
        public async Task<string> GetDataToNotifiy()
        {
            var dataTobeNotifyed = await _logsRepository.GetLogsQueries();
            foreach (var item in dataTobeNotifyed)
            {
                // samble card
                var attachment = getcard("item=>what will be in the card");
                //get the usier to fitch the data to get the ConversationId and ServiceUrl
                var user = await _usersDataRepository.GetConversationRefrenceAsync("item.id=>what is Upn is it the id in the Q");
                //send the notification.
                await _notificationService.SendAsync(user.ServiceUrl, user.ConversationId, 0, MessageFactory.Attachment(attachment));

                var updateData = await _logsRepository.UpdateLogsCommand(dataTobeNotifyed);
            }

            var queueMessage = JsonConvert.SerializeObject(dataTobeNotifyed);
            return queueMessage;
        }
        public Attachment getcard(string shift)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "Proactive Hello",
                Size = AdaptiveTextSize.ExtraLarge
            });

            card.Body.Add(new AdaptiveImage()
            {
                Url = new Uri("http://adaptivecards.io/content/cats/1.png")
            });

            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
            return attachment;

        }


    }
}
