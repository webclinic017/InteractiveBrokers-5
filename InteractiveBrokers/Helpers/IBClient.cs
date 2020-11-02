

using InteractiveBrokers.Helpers;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InteractiveBrokers
{
	public partial class IBClient
	{
		public static Org.BouncyCastle.Math.BigInteger Prime { get; private set; }

		public Task<TReturn> MakeCallAsync<TReturn>(string url, string accountId, string token, string liveSessionToken = null, HttpMethod method = null)
		{
			return MakeCallAsync<TReturn, object>(url, accountId, token, liveSessionToken: liveSessionToken, method: method);
		}

		public async Task MakeCallAsync(string url, string accountId, string token, string liveSessionToken = null, HttpMethod method = null)
		{
			await MakeInteractiveBrokersRequest<object>(url, accountId, token, null, liveSessionToken, method);
		}

		public Task<TReturn> MakeCallAsync<TReturn>(string url, string token, string liveSessionToken = null, HttpMethod method = null)
		{
			return MakeCallAsync<TReturn, object>(url, token, null, method);
		}

		public Task<TReturn> MakeCallAsync<TReturn, TObject>(string url, string token, TObject obj, string liveSessionToken = null, HttpMethod method = null) where TObject : class
		{
			return MakeCallAsync<TReturn, TObject>(url, string.Empty, token, obj, liveSessionToken, method);
		}

		public Task MakeCallAsync<TObject>(string url, TObject obj, string liveSessionToken = null, HttpMethod method = null) where TObject : class
		{
			return MakeInteractiveBrokersRequest<TObject>(url, string.Empty, null, obj, liveSessionToken, method);
		}

		public Task<HttpContent> MakeCallAsync<TObject>(string url, string accountId, string token, TObject obj, string liveSessionToken = null, HttpMethod method = null) where TObject : class
		{
			return MakeInteractiveBrokersRequest<TObject>(url, accountId, token, obj, liveSessionToken, method);
		}


		public async Task<HttpContent> MakeInteractiveBrokersRequest<TObject>(string url, string accountId, string token, TObject obj, string liveSessionToken = null, HttpMethod method = null)
		{
			try
			{
				_logger?.LogInformation($"Sending an request to InteractiveBrokers for account {accountId} {(obj == null ? "" : (Environment.NewLine + JsonSerializer.Serialize(obj)))}");
				var requestMessage = new HttpRequestMessage(method ?? HttpMethod.Post, "")
				{
					Content = obj != null ? new FormUrlEncodedContent(await obj.ToKeyValueAsync()) : null,
					RequestUri = url.StartsWith(_httpClient.BaseAddress.ToString()) ? new Uri(url) : new Uri($"{_httpClient.BaseAddress}{url}")
				};

				var isAuthRequest = url.StartsWith("oauth");

				if (isAuthRequest)
				{
					var isAccessTokenRequest = url.Contains("access_token");
					var isRequestTokenRequest = url.Contains("request_token");
					var isLiveSessionRequest = url.Contains("live_session_token");



					if (isRequestTokenRequest)
					{
						requestMessage.Headers.Authorization = GetRequestTokenHeaders(url);
					}
					else if (isAccessTokenRequest)
					{
						requestMessage.Headers.Authorization = GetAccesstokenHeaders(token, url, obj as string);
					}
					else
					{
						requestMessage.Headers.Authorization = GetLiveSessionHeaders(token, url, obj as string);
					}
					requestMessage.Content = null;
				}
				else
				{
					requestMessage.Headers.Authorization = await GetHeadersAsync(token, url, method, liveSessionToken, obj);
				}

				var result = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
				if (!result.IsSuccessStatusCode)
				{
					throw new InteractiveBrokersException($"Status Code: {result.StatusCode} Response: {await result.Content.ReadAsStringAsync().ConfigureAwait(false)}");
				}
				if (obj is Models.Order)
				{
					var location = result.Headers.Location;
					if (location != null)
					{
						return await MakeInteractiveBrokersRequest<Models.Order>(location.ToString(), accountId, token, null, liveSessionToken, HttpMethod.Get);
					}

					if (_logger?.IsEnabled(LogLevel.Information) ?? false)
					{
						_logger.LogInformation($"Response received successfully from E*Trade with response object {await result.Content.ReadAsStringAsync().ConfigureAwait(false)} ");
					}
				}
				return result.Content;
			}
			catch (Exception ex)
			{
				_logger?.LogError($"Failed to make the request to TD", ex);
				throw;
			}
			finally
			{
				_logger?.LogInformation("Finished ");
			}
		}

		private async Task<AuthenticationHeaderValue> GetHeadersAsync(string token, string url, HttpMethod method, string liveSessionToken, object payload)
		{
			var nonce = ClientHelpers.GetNonce();
			var timestamp = ClientHelpers.GetTimestamp();

			var data = await payload.ToKeyValueAsync() ?? new Dictionary<string, string>();
			data.Add("oauth_consumer_key", s_clientId);
			data.Add("oauth_timestamp", timestamp.ToString());
			data.Add("oauth_nonce", nonce);
			data.Add("oauth_signature_method", "HMAC-SHA256");
			data.Add("oauth_token", token);
			data.Add("oauth_signature", ClientHelpers.GetSignatureBaseString(_httpClient.BaseAddress.AbsoluteUri + url, data, liveSessionToken, sigMethod: "HMAC-SHA256", httpMethod: method.Method));

			return ClientHelpers.GetDefaultAuthHeader(data, true);
		}

		public AuthenticationHeaderValue GetAccesstokenHeaders(string token, string url, string verificationCode)
		{
			var nonce = ClientHelpers.GetNonce();
			var timestamp = ClientHelpers.GetTimestamp();
			var data = new Dictionary<string, string>
			{
				{ "oauth_consumer_key", s_clientId },
				{ "oauth_timestamp", timestamp.ToString() },
				{ "oauth_nonce", nonce },
				{ "oauth_signature_method",  "RSA-SHA256" },
				{ "oauth_token", token },
				{ "oauth_verifier", Uri.EscapeDataString(verificationCode) }
			};


			data.Add("oauth_signature", ClientHelpers.GetSignatureBaseString(_httpClient.BaseAddress.AbsoluteUri + url, data));
			return ClientHelpers.GetDefaultAuthHeader(data, false);
		}

		public AuthenticationHeaderValue GetLiveSessionHeaders(string token, string url, string verificationCode)
		{
			using var reader = File.OpenText(@"dhparam.pem");
			var bytes = new byte[] {
		0xC3, 0x30, 0x59, 0xC2, 0x95, 0x01, 0xDD, 0x9E, 0x61, 0x9E,
		0x89, 0x45, 0xE0, 0x5E, 0xFB, 0xB8, 0xF8, 0x82, 0x29, 0x66,
		0xBF, 0x8A, 0x96, 0x08, 0x33, 0xC6, 0x41, 0xB3, 0x98, 0x42,
		0x9A, 0xA4, 0x69, 0x7B, 0xE6, 0xCD, 0x99, 0x48, 0x81, 0xD3,
		0x18, 0x71, 0xA5, 0x57, 0x9F, 0x6B, 0x4A, 0x3B, 0x6C, 0x78,
		0x62, 0x62, 0x3F, 0x3D, 0x6A, 0x6B, 0xAC, 0xB3, 0x4B, 0x31,
		0x7A, 0xBB, 0xED, 0x28, 0xE5, 0x64, 0x34, 0x8A, 0xD4, 0xF0,
		0x6A, 0xF2, 0xAA, 0x61, 0x38, 0xB3, 0x9D, 0x22, 0xF2, 0x51,
		0x16, 0xF7, 0x55, 0xE8, 0x9D, 0xCB, 0xEE, 0xF3, 0x53, 0x4D,
		0x8F, 0xA4, 0x45, 0x23, 0xE6, 0xAD, 0x87, 0x10, 0x07, 0x9A,
		0xDA, 0x02, 0x73, 0x94, 0x6A, 0xEB, 0x48, 0xB0, 0x13, 0xE7,
		0x02, 0x95, 0x4F, 0xDD, 0xBE, 0xD9, 0x9D, 0xDD, 0xA7, 0x67,
		0x56, 0xD3, 0xE5, 0x4C, 0x3C, 0xE4, 0x7D, 0xAB, 0xBB, 0x61,
		0xFD, 0x5E, 0x96, 0x5B, 0xE9, 0x72, 0xB1, 0x40, 0x7B, 0xC7,
		0xC3, 0xDD, 0xA8, 0x49, 0x01, 0x71, 0xE2, 0x61, 0x2A, 0x28,
		0xC1, 0x9B, 0xB0, 0xD6, 0xCC, 0x7B, 0x96, 0x5E, 0xC5, 0x03,
		0x5D, 0x1A, 0x33, 0x77, 0xAC, 0x50, 0xDB, 0xB2, 0x5D, 0xA5,
		0x42, 0x1C, 0x4F, 0xCF, 0xBA, 0x7D, 0xA1, 0xFE, 0xE7, 0x71,
		0x9A, 0x4F, 0xF4, 0x9E, 0xA9, 0x0F, 0x98, 0x33, 0xCD, 0x32,
		0x7F, 0x18, 0xA4, 0x4C, 0x06, 0x4A, 0xE9, 0x63, 0x6E, 0xE5,
		0xB7, 0x32, 0x7A, 0x37, 0x9C, 0xDB, 0xBC, 0xE3, 0x79, 0x5B,
		0xC6, 0x77, 0x42, 0xCD, 0x57, 0x46, 0xA3, 0x14, 0x9A, 0x31,
		0x10, 0x49, 0x9D, 0x47, 0xDF, 0xCE, 0x0C, 0x24, 0x2A, 0x3C,
		0x22, 0xB0, 0xE9, 0x46, 0x38, 0xF1, 0xFF, 0x4F, 0xA7, 0x12,
		0x86, 0x27, 0xC6, 0x0F, 0x25, 0xE3, 0x51, 0x6C, 0xD0, 0xA1,
		0xCE, 0x7C, 0xD0, 0x0B, 0x35, 0xBB
	};
			Prime = new Org.BouncyCastle.Math.BigInteger(1, bytes);
			var g = new Org.BouncyCastle.Math.BigInteger("2", 10);
			var dh = new DHParameters(Prime, g);

			//var dhPrivateKeyParameters = keyPair.Private as DHPrivateKeyParameters;
			var challenge = dh.G.ModPow(DhRandom, dh.P);
			var diffie_hellman_challenge = BitConverter.ToString(challenge.ToByteArray()).Replace("-", "").ToLower();
			var nonce = ClientHelpers.GetNonce();
			var timestamp = ClientHelpers.GetTimestamp();
			var data = new Dictionary<string, string>
			{
				{ "diffie_hellman_challenge", diffie_hellman_challenge },
				{ "oauth_consumer_key", s_clientId },
				{ "oauth_timestamp", timestamp.ToString() },
				{ "oauth_nonce", nonce },
				{ "oauth_signature_method", "RSA-SHA256" },
				{ "oauth_token", token },
			};


			data.Add("oauth_signature", ClientHelpers.GetSignatureBaseString(_httpClient.BaseAddress.AbsoluteUri + url, data, verificationCode));
			return ClientHelpers.GetDefaultAuthHeader(data, false);
		}

		public AuthenticationHeaderValue GetRequestTokenHeaders(string url)
		{
			var nonce = ClientHelpers.GetNonce();
			var timestamp = ClientHelpers.GetTimestamp();
			var data = new Dictionary<string, string>
			{
				{ "oauth_consumer_key", s_clientId },
				{ "oauth_timestamp", timestamp.ToString() },
				{ "oauth_nonce", nonce },
				{ "oauth_signature_method", "RSA-SHA256" },
				{ "oauth_callback", "oob" },
				{ "oauth_version", "1.0" }
			};
			data.Add("oauth_signature", ClientHelpers.GetSignatureBaseString(_httpClient.BaseAddress.AbsoluteUri + url, data));
			return ClientHelpers.GetDefaultAuthHeader(data);
		}

		public async Task<TReturn> MakeCallAsync<TReturn, TObject>(string url, string accountId, string token, TObject obj = null, string liveSessionToken = null, HttpMethod method = null) where TObject : class
		{
			var content = await this.MakeInteractiveBrokersRequest<TObject>(url, accountId, token, obj, liveSessionToken, method);
			using var result = await content.ReadAsStreamAsync().ConfigureAwait(false);
			return await JsonSerializer.DeserializeAsync<TReturn>(result).ConfigureAwait(false);
		}
	}
}
