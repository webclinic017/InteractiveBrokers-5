using InteractiveBrokers.Models;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InteractiveBrokers
{
	public partial class IBClient
	{

		private readonly ILogger _logger;
		private static string s_redirectUrl;
		private static string s_clientId;
		private static string s_clientSecret;
		private static bool s_isSandbox;
		private object _requestToken;
		private object _requestSecret;
		private readonly HttpClient _httpClient;

		public IBClient(
			HttpClient httpClient,
			ILogger logger = null,
			string clientId = null,
			string clientSecret = null,
			string redirectUrl = null,
			bool? isSandbox = null)
		// https://us.InteractiveBrokers.com/e/t/etws/authorize

		{
			_httpClient = httpClient;
			_logger = logger;
			s_clientId = clientId ?? s_clientId;
			s_isSandbox = isSandbox ?? s_isSandbox;
			s_clientSecret = clientSecret ?? s_clientSecret;
			s_redirectUrl = redirectUrl ?? s_redirectUrl;
			httpClient.BaseAddress = new Uri(
				s_isSandbox ?
				@"https://www.interactivebrokers.com/ptradingapi/v1/" :
				"https://www.interactivebrokers.com/tradingapi/v1/");
		}

		static IBClient()
		{
			var options = ((JsonSerializerOptions)typeof(JsonSerializerOptions).GetField("s_defaultOptions", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null));
			options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
			options.PropertyNameCaseInsensitive = true;
			options.IgnoreNullValues = true;
			options.Converters.Add(new JsonStringEnumConverter());
		}

		public static void Configure(string clientId, string clientSecret, string redirectUrl, bool isSandbox = false)
		{
			if (clientId == null) return;
			s_clientId = clientId;
			s_clientSecret = clientSecret;
			s_redirectUrl = redirectUrl;
			s_isSandbox = isSandbox;
		}

		public async Task<AccessToken> GetAccessToken(string codeOrToken, string verificationCode, AuthorizationType authorizationType = AuthorizationType.AuthorizationCode)
		{
			if (authorizationType == AuthorizationType.AuthorizationCode)
			{
				if (codeOrToken.Contains("%")) codeOrToken = System.Web.HttpUtility.UrlDecode(codeOrToken);
				return await MakeCallAsync<AccessToken, object>("oauth/access_token", codeOrToken, verificationCode, null, HttpMethod.Post).ConfigureAwait(false);
			}

			return await MakeCallAsync<AccessToken, object>("oauth/renew_access_token", null, null, HttpMethod.Post).ConfigureAwait(false);
		}

		public static BigInteger DhRandom = new BigInteger(200, new SecureRandom());

		public async Task<LiveSessionTokenResponse> GetLiveSessionToken(string codeOrToken, string verificationCode)
		{
			return await MakeCallAsync<LiveSessionTokenResponse, object>("oauth/live_session_token", codeOrToken, verificationCode, null, HttpMethod.Post).ConfigureAwait(false);
		}


		public async Task<OAuthResult> GetOAuthRedirectUrlAsync()
		{
			var response = await MakeInteractiveBrokersRequest<object>("oauth/request_token", null, null, null, method: HttpMethod.Post).ConfigureAwait(false);
			var result = await JsonSerializer.DeserializeAsync<RequestTokenResponse>(await response.ReadAsStreamAsync().ConfigureAwait(false)).ConfigureAwait(false);
			return new OAuthResult
			{
				RequestToken = result.OauthToken,
				Url = $@"http://www.interactivebrokers.com/authorize?oauth_token={result.OauthToken}"
			};
		}
	}
}
