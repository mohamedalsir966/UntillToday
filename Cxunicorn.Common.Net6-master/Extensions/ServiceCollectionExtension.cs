using Azure;
using Azure.Core;
using Azure.Data.Tables;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage;
using Azure.Storage.Blobs;
using Cxunicorn.Common.Extensions.Authentication;
using Cxunicorn.Common.Middlewares.Logging.Logger;
using Cxunicorn.Common.Middlewares.Logging.Logger.LoggingRepository;
using Cxunicorn.Common.Middlewares.Logging.RequestLogging;
//using Cxunicorn.Common.Middlewares.Logging.RequestLogging.Logger;
using Cxunicorn.Common.Middlewares.Logging.RequestLogging.RequestLoggingRepository;
using Cxunicorn.Common.Middlewares.TeamsActivityHandler;
using Cxunicorn.Common.Middlewares.TeamsActivityHandler.Users;
using Cxunicorn.Common.Services.BotNotification;
using Cxunicorn.Common.Services.Graph;
using Cxunicorn.Common.Services.Queue;
using Cxunicorn.Common.Services.Tables;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using System.Net;

namespace Cxunicorn.Common.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddBlobClient(this IServiceCollection services)
        {
            services.AddSingleton(sp =>
            {
                var repositoryOptions = sp.GetRequiredService<IOptions<RepositoryOptions>>().Value;

                if(repositoryOptions == null)
                    throw new NullReferenceException(nameof(repositoryOptions));

                if (repositoryOptions.IsManagedIdentityAuthentication.GetValueOrDefault())
                {
                    return new BlobServiceClient(
                       new Uri($"https://{repositoryOptions.StorageAccountName}.blob.core.windows.net"),
                       new DefaultAzureCredential());
                }
                else if(repositoryOptions.IsKeyVaultAuthentication.GetValueOrDefault())
                {
                    var secretClient = sp.GetRequiredService<SecretClient>();
                    if (secretClient == null)
                        throw new NullReferenceException(nameof(secretClient));

                    if(string.IsNullOrWhiteSpace(repositoryOptions.ConnectionStringSecretName))
                        throw new NullReferenceException(nameof(repositoryOptions.ConnectionStringSecretName));

                    KeyVaultSecret secret = secretClient.GetSecret(repositoryOptions.ConnectionStringSecretName);

                    if(string.IsNullOrWhiteSpace(secret.Value))
                        throw new NullReferenceException(nameof(secret.Value));

                    return new BlobServiceClient(secret.Value);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(repositoryOptions.ConnectionString))
                        throw new NullReferenceException(nameof(repositoryOptions.ConnectionString));

                    return new BlobServiceClient(repositoryOptions.ConnectionString);
                }
            });
        }

        public static void AddTableClient(this IServiceCollection services)
        {
            services.AddSingleton(sp =>
            {
                var repositoryOptions = sp.GetRequiredService<IOptions<RepositoryOptions>>().Value;

                if (repositoryOptions == null)
                    throw new NullReferenceException(nameof(repositoryOptions));

                if (repositoryOptions.IsManagedIdentityAuthentication.GetValueOrDefault())
                {
                    return new TableServiceClient(
                        new Uri($"https://{repositoryOptions.StorageAccountName}.table.core.windows.net/"), 
                        new DefaultAzureCredential());
                }
                else if (repositoryOptions.IsKeyVaultAuthentication.GetValueOrDefault())
                {
                    var secretClient = sp.GetRequiredService<SecretClient>();
                    if (secretClient == null)
                        throw new NullReferenceException(nameof(secretClient));

                    if (string.IsNullOrWhiteSpace(repositoryOptions.ConnectionStringSecretName))
                        throw new NullReferenceException(nameof(repositoryOptions.ConnectionStringSecretName));

                    KeyVaultSecret secret = secretClient.GetSecret(repositoryOptions.ConnectionStringSecretName);

                    if (string.IsNullOrWhiteSpace(secret.Value))
                        throw new NullReferenceException(nameof(secret.Value));

                    return new TableServiceClient(secret.Value);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(repositoryOptions.ConnectionString))
                        throw new NullReferenceException(nameof(repositoryOptions.ConnectionString));

                    return new TableServiceClient(repositoryOptions.ConnectionString);
                }
            });
        }

        public static void AddServiceBusClient(this IServiceCollection services)
        {
            ServiceBusClientOptions options = new();

            // configure retries
            options.RetryOptions.MaxRetries = 5;// default is 3
            options.RetryOptions.Mode = ServiceBusRetryMode.Exponential; // default is fixed retry policy
            options.RetryOptions.Delay = TimeSpan.FromSeconds(1); // default is 0.8s

            services.AddSingleton(sp =>
            {
                var queueOptions = sp.GetRequiredService<IOptions<QueueOptions>>().Value;
                if(queueOptions == null)
                    throw new NullReferenceException(nameof(queueOptions));

                if (queueOptions.IsManagedIdentityAuthentication.GetValueOrDefault())
                {
                    return new ServiceBusClient(
                            $"{queueOptions.ServiceBusNamespace}.servicebus.windows.net",
                            new DefaultAzureCredential(),
                            options);
                }
                else if (queueOptions.IsKeyVaultAuthentication.GetValueOrDefault())
                {
                    var secretClient = sp.GetRequiredService<SecretClient>();
                    if (secretClient == null)
                        throw new NullReferenceException(nameof(secretClient));

                    if (string.IsNullOrWhiteSpace(queueOptions.ConnectionStringSecretName))
                        throw new NullReferenceException(nameof(queueOptions.ConnectionStringSecretName));

                    KeyVaultSecret secret = secretClient.GetSecret(queueOptions.ConnectionStringSecretName);

                    if (string.IsNullOrWhiteSpace(secret.Value))
                        throw new NullReferenceException(nameof(secret.Value));

                    return new ServiceBusClient(secret.Value, options);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(queueOptions.ConnectionString))
                        throw new NullReferenceException(nameof(queueOptions.ConnectionString));

                    return new ServiceBusClient(queueOptions.ConnectionString, options);
                }
            });
        }

        public static void AddNotificationService(this IServiceCollection services)
        {
            services.AddSingleton<INotificationService>(sp =>
            {
                var botIOptions = sp.GetRequiredService<IOptions<BotOptions>>();
                var botAdapter = sp.GetRequiredService<IBotFrameworkHttpAdapter>();
                var logger = sp.GetRequiredService<ILogger<NotificationService>>();
                var botOptions = botIOptions.Value;

                if (botOptions == null)
                    throw new NullReferenceException(nameof(botOptions));

                if (botOptions.IsKeyVaultAuthentication.GetValueOrDefault())
                {
                    var secretClient = sp.GetRequiredService<SecretClient>();
                    if (secretClient == null)
                        throw new NullReferenceException(nameof(secretClient));

                    if (string.IsNullOrWhiteSpace(botOptions.MicrosoftAppSecretVaultSecretName))
                        throw new NullReferenceException(nameof(botOptions.MicrosoftAppSecretVaultSecretName));

                    KeyVaultSecret secret = secretClient.GetSecret(botOptions.MicrosoftAppSecretVaultSecretName);

                    if (string.IsNullOrWhiteSpace(secret.Value))
                        throw new NullReferenceException(nameof(secret.Value));

                    botOptions.MicrosoftAppSecret = secret.Value;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(botOptions.MicrosoftAppSecret))
                        throw new NullReferenceException(nameof(botOptions.MicrosoftAppSecret));
                }

                return new NotificationService(botAdapter, botIOptions, logger);
            });
        }

        public static void AddGraphService(this IServiceCollection services, bool addApplicationClient, bool? addDeligatedClient = false)
        {
            services.AddScoped<IGraphService>(provider =>
            {
                var options = new TokenCredentialOptions
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                };

                var adOptions = provider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
                if (adOptions == null)
                    throw new NullReferenceException(nameof(adOptions));

                if (addApplicationClient && addDeligatedClient.GetValueOrDefault())
                {
                    var clientSecretCredential = new ClientSecretCredential(
                            adOptions.AzureAdTenantId, adOptions.AzureAdClientId, adOptions.AzureAdClientSecret, options);

                    var scopes = new[] { "https://graph.microsoft.com/.default" };
                    var res = new GraphServiceClient(clientSecretCredential, scopes);

                    if (adOptions.GetServiceAccountPasswordFromKeyVault.GetValueOrDefault())
                    {
                        var secretClient = provider.GetRequiredService<SecretClient>();
                        if (secretClient == null)
                            throw new NullReferenceException(nameof(secretClient));

                        if (string.IsNullOrWhiteSpace(adOptions.ServiceAccountPasswordSecretName))
                            throw new NullReferenceException(nameof(adOptions.ServiceAccountPasswordSecretName));

                        KeyVaultSecret secret = secretClient.GetSecret(adOptions.ServiceAccountPasswordSecretName);

                        if (string.IsNullOrWhiteSpace(secret.Value))
                            throw new NullReferenceException(nameof(secret.Value));

                        adOptions.ServiceAccountPassword = secret.Value;
                    }

                    var deligatedTokenTask = GraphService.CreateDeligatedTokenAsync(adOptions);
                    deligatedTokenTask.Wait();

                    return new GraphService(new GraphServiceClient(clientSecretCredential, scopes), deligatedTokenTask.Result);
                }
                else if (addApplicationClient && !addDeligatedClient.GetValueOrDefault())
                {
                    var clientSecretCredential = new ClientSecretCredential(
                        adOptions.AzureAdTenantId, adOptions.AzureAdClientId, adOptions.AzureAdClientSecret, options);

                    var scopes = new[] { "https://graph.microsoft.com/.default" };
                    var res = new GraphServiceClient(clientSecretCredential, scopes);

                    return new GraphService(new GraphServiceClient(clientSecretCredential, scopes));
                }
                else if (!addApplicationClient && addDeligatedClient.GetValueOrDefault())
                {
                    if (adOptions.GetServiceAccountPasswordFromKeyVault.GetValueOrDefault())
                    {
                        var secretClient = provider.GetRequiredService<SecretClient>();
                        if (secretClient == null)
                            throw new NullReferenceException(nameof(secretClient));

                        if (string.IsNullOrWhiteSpace(adOptions.ServiceAccountPasswordSecretName))
                            throw new NullReferenceException(nameof(adOptions.ServiceAccountPasswordSecretName));

                        KeyVaultSecret secret = secretClient.GetSecret(adOptions.ServiceAccountPasswordSecretName);

                        if (string.IsNullOrWhiteSpace(secret.Value))
                            throw new NullReferenceException(nameof(secret.Value));

                        adOptions.ServiceAccountPassword = secret.Value;
                    }

                    var deligatedTokenTask = GraphService.CreateDeligatedTokenAsync(adOptions);
                    deligatedTokenTask.Wait();

                    return new GraphService(null, deligatedTokenTask.Result);
                }
                else throw new InvalidOperationException();
            });
        }

        public static IApplicationBuilder UseTableStorageRequestLogging(this IApplicationBuilder app) =>
            app.UseMiddleware<RequestLoggingMiddleware>();

        public static void AddRequestLogging(this IServiceCollection services) =>
            services.AddSingleton<IRequestLoggingRepository, RequestLoggingRepository>();

        public static void AddTableStorageLogging(this IServiceCollection services) =>
            services.AddSingleton<ILoggingRepository, LoggingRepository>();

        public static void AddTeamsConversationReferenceStore(this IServiceCollection services) =>
            services.AddSingleton<IUsersDataRepository, UsersDataRepository>();

        public static void UseTableStorageLogging(this IApplicationBuilder app)
        {
            var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
            var loggingRepository = app.ApplicationServices.GetRequiredService<ILoggingRepository>();
            loggerFactory.AddProvider(new AzureTableLoggerProvider(loggingRepository));
        }
        public static void HandleGlobalExceptions(this IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = Azure.Core.ContentType.ApplicationJson.ToString();

                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    var loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
                    var logger = loggerFactory?.CreateLogger(nameof(Program));
                    logger?.LogError($"{contextFeature.Error}");

                    await context.Response.WriteAsync((
                        context?.Response?.StatusCode, contextFeature?.Error?.Message
                    ).ToString());
                }
            });
        }
    }
}
