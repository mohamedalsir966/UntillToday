using Domain;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public interface ILogsRepository
    {
       Task<List<ShiftEntityLog>> GetLogsQueries();
       Task <string> UpdateLogsCommand(List<ShiftEntityLog> items);
        Task<ConversationReference> GetConversationReference();
        Task<ConversationReference> AddConversationReferenceAsync(ConversationReference conversationReference);
    }
}
