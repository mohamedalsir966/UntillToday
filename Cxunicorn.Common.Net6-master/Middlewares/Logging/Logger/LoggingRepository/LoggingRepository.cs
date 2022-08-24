using Azure.Data.Tables;
using Cxunicorn.Common.Services.Tables;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Middlewares.Logging.Logger.LoggingRepository
{
    public class LoggingRepository : BaseRepository<LoggingEntity>, ILoggingRepository
    {
        public LoggingRepository(ILogger<LoggingRepository> logger,
                       TableServiceClient tableServiceClient)
                       : base(logger, tableServiceClient, LoggingTableName.TableName, LoggingTableName.PartitionKey)
        {
        }
    }
}
