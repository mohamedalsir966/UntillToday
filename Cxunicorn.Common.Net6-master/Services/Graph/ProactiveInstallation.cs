
using Cxunicorn.Common.Policies;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Services.Graph
{
    internal class ProactiveInstallation
    {
        private readonly GraphServiceClient _serviceClient;
        public ProactiveInstallation(GraphServiceClient graphServiceClient)
        {
            _serviceClient = graphServiceClient;
        }
        public async Task<string?> InstallAppAndGetConversationIdAsync(string appId, Guid userAadId)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new Exception("Invalid AppId.");
            }

            // Install app.
            try
            {
                await this.InstallAppForUserAsync(appId, userAadId);
            }
            catch (ServiceException) { }

            // Get conversation id.
            try
            {
                return await GetChatThreadIdAsync(userAadId, appId);
            }
            catch (ServiceException e)
            {
                throw new Exception(e.Message);
            }
        }
        private async Task<string?> GetChatThreadIdAsync(Guid userId, string appId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            var installationId = await this.GetAppInstallationIdForUserAsync(appId, userId);
            var retryPolicy = PollyPolicy.GetGraphRetryPolicy(5);
            var chat = await retryPolicy.ExecuteAsync(async () => await this._serviceClient.Users[Convert.ToString(userId)]
                .Teamwork
                .InstalledApps[installationId]
                .Chat
                .Request()
                .WithMaxRetry(5)
                .GetAsync());

            return chat?.Id;
        }
        private async Task<string?> GetAppInstallationIdForUserAsync(string appId, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            if (userId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var retryPolicy = PollyPolicy.GetGraphRetryPolicy(5);
            var collection = await retryPolicy.ExecuteAsync(async () =>
                await this._serviceClient.Users[Convert.ToString(userId)]
                    .Teamwork
                    .InstalledApps
                    .Request()
                    .Expand("teamsApp")
                    .Filter($"teamsApp/id eq '{appId}'")
                    .WithMaxRetry(5)
                    .GetAsync());

            return collection?.FirstOrDefault()?.Id;
        }
        private async Task InstallAppForUserAsync(string appId, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            if (userId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var userScopeTeamsAppInstallation = new UserScopeTeamsAppInstallation
            {
                AdditionalData = new Dictionary<string, object>()
                {
                    { "teamsApp@odata.bind", $"https://graph.microsoft.com/beta/appCatalogs/teamsApps/{appId}" },
                },
            };

            var retryPolicy = PollyPolicy.GetGraphRetryPolicy(5);
            await retryPolicy.ExecuteAsync(async () =>
                await this._serviceClient.Users[Convert.ToString(userId)]
                    .Teamwork
                    .InstalledApps
                    .Request()
                    .WithMaxRetry(5)
                    .AddAsync(userScopeTeamsAppInstallation));
        }
    }
}
