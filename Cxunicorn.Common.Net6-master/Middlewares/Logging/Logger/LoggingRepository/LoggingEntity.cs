using Azure.Data.Tables;
using Cxunicorn.Common.Services.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Middlewares.Logging.Logger.LoggingRepository
{
    public class LoggingEntity : BaseEntity, ITableEntity
    {
        public LoggingEntity() : base(LoggingTableName.PartitionKey)
        {
        }
        public string LogLevel { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
