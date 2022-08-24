using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Services.Queue
{
    public class QueueOptions
    {
        public string ServiceBusNamespace { get; set; } = string.Empty;
        public string? ConnectionString { get; set; }
        public bool? IsManagedIdentityAuthentication { get; set; }
        public bool? IsKeyVaultAuthentication { get; set; }
        public string? ConnectionStringSecretName { get; set; }
    }
}
