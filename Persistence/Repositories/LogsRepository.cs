using Domain;
using Domain.BotDB;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class LogsRepository : ILogsRepository
    {
        private readonly ApplicationDbContext _context;
        public LogsRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<ShiftEntityLog>> GetLogsQueries()
        {
            
            var logs= await _context.ShiftEntityLog.Where(x=>x.Status == 0).ToListAsync();
            return logs;
        }

        public async Task <string> UpdateLogsCommand(List<ShiftEntityLog> items)
        {
            foreach (var item in items)
            {
                item.Status = Status.Sent;
                await _context.SaveChangesAsync();
            }
            return "Data Updated";
        }
        public async Task<ConversationReference> GetConversationReference()
        {
            try
            {
                var root = _context.Roots.Include(c => c.user).Include(c => c.conversation).Include(c => c.bot).FirstOrDefault();

                var rootJson = JsonConvert.SerializeObject(root);

                var conversationRef = JsonConvert.DeserializeObject<ConversationReference>(rootJson);
                return conversationRef;
            }
            catch (Exception ex)
            {
                Console.Write("Error info:" + ex.Message);
                return default;
            }
           
        }

        public Task<ConversationReference> AddConversationReferenceAsync(ConversationReference  conversationReference)
        {
            var conversationRef = JsonConvert.SerializeObject(conversationReference);

            var roots = JsonConvert.DeserializeObject<Root>(conversationRef);

            roots.botId = conversationReference.Bot.Id;
            roots.conversationId = conversationReference.Conversation.Id;
            roots.userId = conversationReference.User.Id;
             _context.AddAsync(roots);
            _context.SaveChanges();
            return default;
        }
    }
}
