using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Services.BotNotification
{
    public class BotOptions
    {
        public string? MicrosoftAppId { get; set; } = "d472c8f6-2966-4468-b6ab-9cd62b16dd9e";
        public string? MicrosoftAppSecret { get; set; } = "2488Q~UA7QwevmaF62VZ300oV-F-vjwgIgfkxdkO";
        public string? TenantId { get; set; }
        public bool? IsKeyVaultAuthentication { get; set; }
        public string? MicrosoftAppSecretVaultSecretName { get; set; }
        public int MaxAttempts { get; set; } = 3;
    }
}
