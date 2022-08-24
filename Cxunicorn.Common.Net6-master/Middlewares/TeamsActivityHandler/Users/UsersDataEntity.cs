using Azure.Data.Tables;
using Cxunicorn.Common.Services.Tables;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Middlewares.TeamsActivityHandler.Users
{
    public class UsersDataEntity : BaseEntity, ITableEntity
    {
        public UsersDataEntity() : base(UsersDataTableName.PartitionKey)
        {
        }

        public string Name { get; set; } = string.Empty;
        public string Upn { get; set; } = string.Empty;
        public string AadId
        {
            get => Convert.ToString(aadId) ?? string.Empty;
            set => aadId = Guid.Parse(value);
        }
        public string ConversationId { get; set; } = string.Empty;
        public string ServiceUrl { get; set; } = string.Empty;

        [IgnoreDataMember]
        public Guid aadId { get; set; }
    }
}
