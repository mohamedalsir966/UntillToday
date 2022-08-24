using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Middlewares.Logging.RequestLogging.RequestLoggingRepository
{
    public class RequestLoggingTableName
    {
        public const string TableName = "RequestLogs";
        public const string PartitionKey = "RequestLogs";
    }
}
