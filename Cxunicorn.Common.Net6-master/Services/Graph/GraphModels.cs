using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Services.Graph
{
    public class TokenResponse
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("scope")]
        public string Scope { get; set; }
        [JsonProperty("expires_in")]
        public int Expires { get; set; }
        [JsonProperty("ext_expires_in")]
        public int ExtExpires { get; set; }
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

    }
}
