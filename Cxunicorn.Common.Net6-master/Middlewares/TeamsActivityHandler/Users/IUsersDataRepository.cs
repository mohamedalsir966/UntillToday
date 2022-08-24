using Cxunicorn.Common.Services.Tables;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Middlewares.TeamsActivityHandler.Users
{
    public interface IUsersDataRepository : IBaseRepository<UsersDataEntity>
    {
        Task AddorUpdateConversationRefrenceAsync(ConversationReference reference, TeamsChannelAccount member);
        Task DeleteConversationRefrenceAsync(ConversationReference reference, TeamsChannelAccount member);
        Task<UsersDataEntity> GetConversationRefrenceAsync(string upn);
    }
}
