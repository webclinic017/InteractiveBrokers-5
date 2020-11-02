using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InteractiveBrokers.Models
{
    public class RequestTokenResponse
    {
		[JsonPropertyName("oauth_token")]
        public string OauthToken { get; set; }
    }
}
