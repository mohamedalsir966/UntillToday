using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Extensions.Authentication
{
    public class GraphTokenAcquisitionFilter : IAsyncActionFilter
    {
        private readonly AuthenticationOptions authenticationOptions;
        public GraphTokenAcquisitionFilter(AuthenticationOptions authenticationOptions) : base()
        {
            this.authenticationOptions = authenticationOptions;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var assertion = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var httpClient = new HttpClient();

            var requestUrl = $"https://login.microsoftonline.com/{authenticationOptions.AzureAdTenantId}/oauth2/v2.0/token";
            var formVariables = new Dictionary<string, string>
            {
                { "grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer" },
                { "client_id", authenticationOptions.AzureAdClientId },
                { "client_secret", authenticationOptions.AzureAdClientSecret },
                { "assertion", assertion },
                { "scope", "https://graph.microsoft.com/.default" },
                { "requested_token_use", "on_behalf_of" }
            };

            var content = new FormUrlEncodedContent(formVariables);
            var response = await httpClient.PostAsync(requestUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(tokenResponse.Error);
            }

            context.HttpContext.Request.Headers.Add("GraphToken", tokenResponse.AccessToken);
            await next.Invoke();
        }
    }

    struct TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("error_description")]
        public string Error { get; set; }
    }
}
