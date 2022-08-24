using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Services.Tables
{
    public class BaseEntity : ITableEntity
    {
        private string _partitionKey;
        public BaseEntity(string partitionKey)
        {
            _partitionKey = partitionKey;
        }

        private string _rowKey = Guid.NewGuid().ToString();
        public string PartitionKey
        {
            get => _partitionKey;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    _partitionKey = value;
            }
        }
        public string RowKey
        {
            get => _rowKey;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    _rowKey = value;
            }
        }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
