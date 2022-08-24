using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Cxunicorn.Common.Services.Secret;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;

namespace Cxunicorn.Common.Extensions.Authentication
{
    public static class AuthenticationServiceCollectionExtensions
    {
        public static void AddAuthentication(
            this IServiceCollection services,
            IConfiguration configuration,
            bool addSecret = false)
        {
            RegisterAuthenticationServices(services, configuration, addSecret);
            RegisterAuthorizationPolicy(services);
        }

        private static void RegisterAuthenticationServices(
            IServiceCollection services,
            IConfiguration configuration,
            bool addSecret = false)
        {
            var authenticationOptions = new AuthenticationOptions
            {
                AzureAdInstance = configuration.GetValue<string>("AzureAd:Instance"),
                AzureAdTenantId = configuration.GetValue<string>("AzureAd:TenantId"),
                AzureAdClientId = configuration.GetValue<string>("AzureAd:ClientId"),
                AzureAdApplicationIdUri = configuration.GetValue<string>("AzureAd:ValidIssuers"),
                AzureAdValidIssuers = configuration.GetValue<string>("AzureAd:ApplicationIdUri"),
                AzureAdClientSecretName = configuration.GetValue<string>("AzureAd:SecretName"),
                GetAzureAdClientSecretFromKeyVault = configuration.GetValue("AzureAd:GetAzureAdClientSecretFromKeyVault", true),
                TeamsAppId = configuration.GetValue<string>("AzureAD:TeamsAppId"),
                // add service account
                ServiceAccountEmail = configuration.GetValue<string>("ServiceAccount:ServiceAccountEmail"),
                ServiceAccountPassword = configuration.GetValue<string>("ServiceAccount:ServiceAccountPassword"),
                ServiceAccountPasswordSecretName = configuration.GetValue<string>("ServiceAccount:SecretName"),
                GetServiceAccountPasswordFromKeyVault = configuration.GetValue<bool>("ServiceAccount:GetServiceAccountPasswordFromKeyVault")

            };

            if (addSecret)
            {
                var secretOptions = new SecretOptions
                {
                    KeyVaultName = configuration.GetValue<string>("KeyVaultName"),
                };

                var secretClient = AddAndGetSecretClient(services, secretOptions);
                if (authenticationOptions.GetAzureAdClientSecretFromKeyVault.GetValueOrDefault())
                {
                    KeyVaultSecret secret = secretClient.GetSecret(authenticationOptions.AzureAdClientSecretName);
                    configuration["AzureAd:ClientSecret"] = secret.Value;
                }
            }

            authenticationOptions.AzureAdClientSecret = configuration.GetValue<string>("AzureAd:ClientSecret");

            services.AddOptions<AuthenticationOptions>()
               .Configure<IConfiguration>((ao, configuration) =>
               {
                   ao.AzureAdInstance = authenticationOptions.AzureAdInstance;
                   ao.AzureAdTenantId = authenticationOptions.AzureAdTenantId;
                   ao.AzureAdClientId = authenticationOptions.AzureAdClientId;
                   ao.AzureAdApplicationIdUri = authenticationOptions.AzureAdApplicationIdUri;
                   ao.AzureAdValidIssuers = authenticationOptions.AzureAdValidIssuers;
                   ao.AzureAdClientSecretName = authenticationOptions.AzureAdClientSecretName;
                   ao.GetAzureAdClientSecretFromKeyVault = authenticationOptions.GetAzureAdClientSecretFromKeyVault;
                   ao.TeamsAppId = authenticationOptions.TeamsAppId;
                   ao.AzureAdClientSecret = authenticationOptions.AzureAdClientSecret;
                   ao.ServiceAccountEmail = authenticationOptions.ServiceAccountEmail;
                   ao.ServiceAccountPassword = authenticationOptions.ServiceAccountPassword;
                   ao.ServiceAccountPasswordSecretName = authenticationOptions.ServiceAccountPasswordSecretName;
                   ao.GetServiceAccountPasswordFromKeyVault = authenticationOptions.GetServiceAccountPasswordFromKeyVault;
               });

            ValidateAuthenticationOptions(authenticationOptions);
#pragma warning disable CS0618 // Type or member is obsolete
            var azureADOptions = new AzureADOptions
            {
                Instance = authenticationOptions.AzureAdInstance,
                TenantId = authenticationOptions.AzureAdTenantId,
                ClientId = authenticationOptions.AzureAdClientId,
            };
#pragma warning restore CS0618 // Type or member is obsolete
            RegisterAuthenticationServicesWithSecret(services, configuration, authenticationOptions, azureADOptions);
        }

        private static SecretClient AddAndGetSecretClient(IServiceCollection services, SecretOptions secretOptions)
        {
            if (string.IsNullOrWhiteSpace(secretOptions.KeyVaultName))
                throw new NullReferenceException(nameof(secretOptions.KeyVaultName));

            var client = new SecretClient(
               new Uri($"https://{secretOptions.KeyVaultName}.vault.azure.net/"),
               new DefaultAzureCredential());

            services.AddSingleton(sp => client);
            return client;
        }

        private static void RegisterAuthorizationPolicy(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                var mustContainUpnClaimRequirement = new ValidateIssuerRequirement();

                options.AddPolicy(
                    "ValidateIssuerPolicy",
                    policyBuilder => policyBuilder.AddRequirements(mustContainUpnClaimRequirement));

            });

            services.AddSingleton<IAuthorizationHandler, ValidateIssuerHandler>();
        }

        private static void RegisterAuthenticationServicesWithSecret(
        IServiceCollection services,
        IConfiguration configuration,
        AuthenticationOptions authenticationOptions,
#pragma warning disable CS0618 // Type or member is obsolete
        AzureADOptions azureADOptions)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            services.AddMicrosoftIdentityWebApiAuthentication(configuration)
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddInMemoryTokenCaches();

            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/hubs/user"))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
                options.Authority = $"{azureADOptions.Instance}{azureADOptions.TenantId}/v2.0";
                options.SaveToken = true;
                options.TokenValidationParameters.ValidAudiences = GetValidAudiences(authenticationOptions);
                options.TokenValidationParameters.AudienceValidator = AudienceValidator;
                options.TokenValidationParameters.ValidIssuers = GetValidIssuers(authenticationOptions);
            });
        }

        private static void ValidateAuthenticationOptions(AuthenticationOptions authenticationOptions)
        {
            if (string.IsNullOrWhiteSpace(authenticationOptions?.AzureAdClientId))
            {
                throw new ApplicationException("AzureAd ClientId is missing in the configuration file.");
            }

            if (string.IsNullOrWhiteSpace(authenticationOptions?.AzureAdTenantId))
            {
                throw new ApplicationException("AzureAd TenantId is missing in the configuration file.");
            }

            if (string.IsNullOrWhiteSpace(authenticationOptions?.AzureAdApplicationIdUri))
            {
                throw new ApplicationException("AzureAd ApplicationIdUri is missing in the configuration file.");
            }

            if (string.IsNullOrWhiteSpace(authenticationOptions?.AzureAdValidIssuers))
            {
                throw new ApplicationException("AzureAd ValidIssuers is missing in the configuration file.");
            }
        }

        private static IEnumerable<string> GetValidAudiences(AuthenticationOptions authenticationOptions)
        {
            var validAudiences = new List<string>
            {
                authenticationOptions.AzureAdClientId,
                authenticationOptions.AzureAdApplicationIdUri.ToLower(),
            };

            return validAudiences;
        }

        private static IEnumerable<string> GetValidIssuers(AuthenticationOptions authenticationOptions)
        {
            var tenantId = authenticationOptions.AzureAdTenantId;

            var validIssuers =
                SplitAuthenticationOptionsList(
                    authenticationOptions.AzureAdValidIssuers);

            validIssuers = validIssuers.Select(validIssuer => validIssuer.Replace("TENANT_ID", tenantId));

            return validIssuers;
        }

        private static IEnumerable<string> SplitAuthenticationOptionsList(string stringInAuthenticationOptions)
        {
            var settings = stringInAuthenticationOptions
                ?.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                ?.Select(p => p.Trim());
            if (settings == null)
            {
                throw new ApplicationException($"Invalid list of settings in authentication options.");
            }

            return settings;
        }

        private static bool AudienceValidator(
            IEnumerable<string> tokenAudiences,
            SecurityToken securityToken,
            TokenValidationParameters validationParameters)
        {
            if (tokenAudiences == null || !tokenAudiences.Any())
            {
                throw new ApplicationException("No audience defined in token!");
            }

            var validAudiences = validationParameters.ValidAudiences;
            if (validAudiences == null || !validAudiences.Any())
            {
                throw new ApplicationException("No valid audiences defined in validationParameters!");
            }

            foreach (var tokenAudience in tokenAudiences)
            {
                if (validAudiences.Any(validAudience => validAudience.Equals(tokenAudience, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
