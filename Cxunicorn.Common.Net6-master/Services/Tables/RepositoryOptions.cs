using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Services.Tables
{
    public class RepositoryOptions
    {
        public string? StorageAccountName { get; set; } = string.Empty;
        public string? ConnectionString { get; set; } = string.Empty;
        public bool? IsManagedIdentityAuthentication { get; set; }
        public bool? IsKeyVaultAuthentication { get; set; }
        public string? ConnectionStringSecretName { get; set; }
    }
}
