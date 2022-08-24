using Azure.Data.Tables;
using Cxunicorn.Common.Services.Tables;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Middlewares.TeamsActivityHandler.Users
{
    public class UsersDataRepository : BaseRepository<UsersDataEntity>, IUsersDataRepository
    {
        public UsersDataRepository(ILogger<UsersDataRepository> logger,
                       TableServiceClient tableServiceClient)
                       : base(logger, tableServiceClient, UsersDataTableName.TableName, UsersDataTableName.PartitionKey)
        {
        }

        public async Task AddorUpdateConversationRefrenceAsync(ConversationReference reference, TeamsChannelAccount member)
        {
            var entity = ConvertConversationReferanceForDB(reference, member);
            await TableClient.UpsertEntityAsync(entity);
        }

        public async Task DeleteConversationRefrenceAsync(ConversationReference reference, TeamsChannelAccount member)
        {
            var conRef = ConvertConversationReferanceForDB(reference, member);
            await TableClient.DeleteEntityAsync(PartitionKey, conRef.Upn);
        }

        public async Task<UsersDataEntity> GetConversationRefrenceAsync(string upn) =>
            await TableClient.GetEntityAsync<UsersDataEntity>(PartitionKey, upn);

        private UsersDataEntity ConvertConversationReferanceForDB(ConversationReference reference, TeamsChannelAccount currentMember)
        {
            return new UsersDataEntity
            {
                Upn = currentMember.UserPrincipalName,
                Name = currentMember.Name,
                aadId = Guid.Parse(currentMember.AadObjectId),
                ConversationId = reference.Conversation.Id,
                RowKey = currentMember.UserPrincipalName,
                ServiceUrl = reference.ServiceUrl,
            };
        }
    }
}
