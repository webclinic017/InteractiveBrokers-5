using System.Text.Json.Serialization;

namespace InteractiveBrokers.Models
{
	public class LiveSessionTokenResponse
	{
		[JsonPropertyName("diffie_hellman_response")]
		public string DiffieHelman { get; set; }
		[JsonPropertyName("live_session_token_signature")]
		public string LikeSessionTokenSignature { get; set; }
	}
}
