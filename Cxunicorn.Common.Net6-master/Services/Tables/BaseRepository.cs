using Azure;
using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Services.Tables
{
    public class BaseRepository<T> : IBaseRepository<T> where T : ITableEntity, new()
    {
        protected ILogger Logger { get; }
        public TableClient TableClient { get; }
        public string PartitionKey { get; }
        public BaseRepository(
           ILogger logger,
           TableServiceClient tableClient,
           string tableName,
           string partitionKey)
        {
            Logger = logger;
            PartitionKey = partitionKey;
            TableClient = tableClient.GetTableClient(tableName);
            TableClient.CreateIfNotExists();
        }
        public async Task AddEntities(IEnumerable<T> entities)
        {
            if (entities != null && entities.Any())
            {
                var addEntitiesBatch = entities.Select(x => new TableTransactionAction(TableTransactionActionType.Add, x));
                await TableClient.SubmitTransactionAsync(addEntitiesBatch);
            }
        }
        public async Task UpdateEntities(IEnumerable<T> entities)
        {
            if (entities != null && entities.Any())
            {
                var updateEntitiesBatch = entities.Select(x => new TableTransactionAction(TableTransactionActionType.UpsertMerge, x));
                await TableClient.SubmitTransactionAsync(updateEntitiesBatch);
            }
        }
        public async Task DeleteEntities(IEnumerable<T> entities)
        {
            if (entities != null && entities.Any())
            {
                var deleteEntitiesBatch = entities.Select(x => new TableTransactionAction(TableTransactionActionType.Delete, x));
                await TableClient.SubmitTransactionAsync(deleteEntitiesBatch);
            }
        }
    }
}
