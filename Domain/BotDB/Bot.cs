﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.BotDB
{
    public class Bot
    {
        public string? id { get; set; }
        public string? name { get; set; }
        public string? aadObjectId { get; set; }
        public string? role { get; set; }
    }
}
