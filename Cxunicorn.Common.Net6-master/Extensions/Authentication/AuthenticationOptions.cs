namespace Cxunicorn.Common.Extensions.Authentication
{
    public class AuthenticationOptions
    {
        public string AzureAdInstance { get; set; } = string.Empty;
        public string AzureAdTenantId { get; set; } = string.Empty;
        public string AzureAdClientId { get; set; } = string.Empty;
        public string AzureAdApplicationIdUri { get; set; } = string.Empty;
        public string AzureAdValidIssuers { get; set; } = string.Empty;
        public string? AzureAdClientSecret { get; set; } = string.Empty;
        public string TeamsAppId { get; set; } = string.Empty;
        public bool? GetAzureAdClientSecretFromKeyVault { get; set; }
        public string? AzureAdClientSecretName { get; set; }

        public string? ServiceAccountEmail { get; set; }
        public string? ServiceAccountPassword { get; set; }
        public bool? GetServiceAccountPasswordFromKeyVault { get; set; }
        public string? ServiceAccountPasswordSecretName { get; set; }
    }
}
