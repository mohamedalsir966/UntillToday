using Azure.Data.Tables;
using Cxunicorn.Common.Services.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Middlewares.Logging.RequestLogging.RequestLoggingRepository
{
    public class RequestLogEntity : BaseEntity, ITableEntity
    {
        public RequestLogEntity() : base(RequestLoggingTableName.PartitionKey)
        {
        }
        public string RequestMethod { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public DateTime RequestedOn { get; set; }
        public string RequestorIP { get; set; } = string.Empty;
        public string RequestUrl { get; set; } = string.Empty;
        public bool IsHttps { get; set; }
        public string QueryString { get; set; } = string.Empty;
        public string? RequestorId { get; set; }
        public int StatusCode { get; set; }
        public DateTime RespondedOn { get; set; }
        public string? RequestBody { get; set; } = string.Empty;
    }
}
