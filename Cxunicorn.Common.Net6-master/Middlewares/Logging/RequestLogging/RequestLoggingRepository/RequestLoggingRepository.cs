using Azure.Data.Tables;
using Cxunicorn.Common.Services.Tables;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Middlewares.Logging.RequestLogging.RequestLoggingRepository
{
    public class RequestLoggingRepository : BaseRepository<RequestLogEntity>, IRequestLoggingRepository
    {
        public RequestLoggingRepository(ILogger<RequestLoggingRepository> logger,
                       TableServiceClient tableServiceClient)
                       : base(logger, tableServiceClient, RequestLoggingTableName.TableName, RequestLoggingTableName.PartitionKey)
        {
        }
    }
}
