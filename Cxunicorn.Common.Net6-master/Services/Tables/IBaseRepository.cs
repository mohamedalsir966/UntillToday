using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Services.Tables
{
    public interface IBaseRepository<T> where T : ITableEntity, new()
    {
        public TableClient TableClient { get; }

        public string PartitionKey { get; }
        Task AddEntities(IEnumerable<T> entities);
        Task DeleteEntities(IEnumerable<T> entities);
        Task UpdateEntities(IEnumerable<T> entities);
    }
}
