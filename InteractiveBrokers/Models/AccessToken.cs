using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InteractiveBrokers.Models
{
    public class AccessToken
    {
		[JsonPropertyName("oauth_token")]
		public string Token { get; set; }
		[JsonPropertyName("oauth_token_secret")]
		public string Secret { get; set; }

		[JsonPropertyName("is_paper")]
		public bool IsPaper { get; set; }

	}
}
