using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Services.BotNotification
{
    public class NotificationService : INotificationService
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly BotOptions _options;
        private readonly ILogger<NotificationService> _logger;
        public enum NotificationActionType { Send, Update, Delete };
        public NotificationService(IBotFrameworkHttpAdapter adapter, 
            IOptions<BotOptions> options,
            ILogger<NotificationService> logger)
        {
            this._adapter = adapter;
            this._options = options.Value;
            this._logger = logger;

            if (string.IsNullOrWhiteSpace(_options.MicrosoftAppId))
                throw new ArgumentException(nameof(_options.MicrosoftAppId));

            if (string.IsNullOrWhiteSpace(_options.MicrosoftAppSecret))
                throw new ArgumentException(nameof(_options.MicrosoftAppSecret));
        }

        private ConversationReference GetConversationReference(string serviceUrl, string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId))
                throw new ArgumentException($"'{nameof(conversationId)}' cannot be null or empty", nameof(conversationId));

            if (string.IsNullOrEmpty(serviceUrl))
                throw new ArgumentException($"'{nameof(serviceUrl)}' cannot be null or empty", nameof(serviceUrl));

            return new ConversationReference
            {
                ServiceUrl = serviceUrl,
                Conversation = new ConversationAccount
                {
                    Id = conversationId,
                },
            };
        }

        public async Task<SendMessageResponse> SendAsync(string serviceUrl, 
            string conversationId, 
            NotificationActionType notificationActionType,
            IMessageActivity? message = null,
            string? activityIdForDelete = null)
        {
            if(notificationActionType == NotificationActionType.Update)
            {
                if(string.IsNullOrWhiteSpace(message?.Id))
                    throw new NullReferenceException(nameof(message.Id));
            }

            if (notificationActionType == NotificationActionType.Delete)
            {
                if (string.IsNullOrWhiteSpace(activityIdForDelete))
                    throw new NullReferenceException(nameof(activityIdForDelete));
            }
            var response = new SendMessageResponse
            {
                TotalNumberOfSendThrottles = 0,
                AllSendStatusCodes = string.Empty,
            };

            await ((BotAdapter)this._adapter).ContinueConversationAsync(
                botId: _options.MicrosoftAppId,
                reference: GetConversationReference(serviceUrl,conversationId),
                callback: async (turnContext, cancellationToken) =>
                {
                    try
                    {
                        ResourceResponse res = new();

                        switch(notificationActionType)
                        {
                            case NotificationActionType.Update:
                                res = await GetRetryPolicy().ExecuteAsync(async () => await turnContext.UpdateActivityAsync(message));
                                break;
                            case NotificationActionType.Delete:
                                await GetRetryPolicy().ExecuteAsync(async () => await turnContext.DeleteActivityAsync(activityIdForDelete));
                                break;
                            case NotificationActionType.Send:
                                res = await GetRetryPolicy().ExecuteAsync(async () => await turnContext.SendActivityAsync(message));
                                break;
                            default:
                                throw new InvalidOperationException(nameof(notificationActionType));
                        }
 
                        // Success.
                        response.ResultType = SendMessageResult.Succeeded;
                        response.StatusCode = (int)HttpStatusCode.Created;
                        response.AllSendStatusCodes += $"{(int)HttpStatusCode.Created},";
                        response.ActivityId = res.Id;
                    }
                    catch (ErrorResponseException e)
                    {
                        var errorMessage = $"{e.GetType()}: {e.Message}";
                        _logger.LogError(e, $"Failed to {Convert.ToString(notificationActionType)} message. Exception message: {errorMessage}");

                        response.StatusCode = (int)e.Response.StatusCode;
                        response.AllSendStatusCodes += $"{(int)e.Response.StatusCode},";
                        response.ErrorMessage = e.Response.Content;
                        response.ActivityId = null;
                        switch (e.Response.StatusCode)
                        {
                            case HttpStatusCode.TooManyRequests:
                                response.ResultType = SendMessageResult.Throttled;
                                response.TotalNumberOfSendThrottles = _options.MaxAttempts;
                                break;

                            case HttpStatusCode.NotFound:
                                response.ResultType = SendMessageResult.RecipientNotFound;
                                break;

                            default:
                                response.ResultType = SendMessageResult.Failed;
                                break;
                        }
                    }
                },
                cancellationToken: CancellationToken.None);

            return response;
        }
       

        private AsyncRetryPolicy GetRetryPolicy()
        {
            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: _options.MaxAttempts);
            return Policy
                .Handle<ErrorResponseException>(e =>
                {
                    var errorMessage = $"{e.GetType()}: {e.Message}";
                    _logger.LogError(e, $"Exception thrown: {errorMessage}");

                    // Handle throttling and internal server errors.
                    var statusCode = e.Response.StatusCode;
                    return statusCode == HttpStatusCode.TooManyRequests || ((int)statusCode >= 500 && (int)statusCode < 600);
                })
                .WaitAndRetryAsync(delay);
        }
    }
}
