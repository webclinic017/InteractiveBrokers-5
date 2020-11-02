

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
        public static byte[] DiffieBytes { get; private set; }

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
            DiffieBytes ??= File.ReadAllBytes(@"dhparam.pem");
            Prime ??= new Org.BouncyCastle.Math.BigInteger(1, DiffieBytes);
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
