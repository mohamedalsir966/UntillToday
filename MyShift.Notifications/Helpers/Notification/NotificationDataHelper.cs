using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Persistence.Repositories;
using Azure.Core;
using Azure;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading;
using Microsoft.Bot.Schema;
using Microsoft.Graph.ExternalConnectors;
using Microsoft.Graph;
using MyShift.Notifications.Helpers.Cards;

namespace MyShift.Notifications.Helpers.Notification
{
    public class NotificationDataHelper : INotificationDataHelper
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly IBot _bot;
        private readonly ILogsRepository _logsRepository;
        public NotificationDataHelper(IBot bot, IBotFrameworkHttpAdapter adapter, IConfiguration configuration, ILogsRepository logsRepository)
        {
            _bot = bot;
            _adapter = adapter;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
            _logsRepository = logsRepository;
        }
        public async Task<bool> SendNotification(ConversationReference conversationReference)
        {   
            await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));

            return true;
        }

        private async Task BotCallback( ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var attachment = MessageFactory.Attachment(UpcomingShiftCard.GetCard());
            await turnContext.SendActivityAsync(attachment);
        }
    }
}
