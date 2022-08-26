using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.BotDB
{
    public class Root
    {
        [Key]
        public string? activityId { get; set; }
        public User? user { get; set; }
        public string? userId { get; set; }
        public Bot? bot { get; set; }
        public string? botId { get; set; }
        public Conversation? conversation { get; set; }
        public string? conversationId { get; set; }
        public string? channelId { get; set; }
        public string? locale { get; set; }
        public string? serviceUrl { get; set; }
    }
}
