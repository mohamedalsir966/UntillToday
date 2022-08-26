using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.BotDB
{
    public class Conversation
    {
        public string? isGroup { get; set; }
        public string? conversationType { get; set; }
        public string? id { get; set; }
        public string? name { get; set; }
        public string? aadObjectId { get; set; }
        public string? role { get; set; }
        public string? tenantId { get; set; }
    }
    
}
