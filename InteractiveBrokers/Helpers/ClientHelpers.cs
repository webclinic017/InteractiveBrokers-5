using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InteractiveBrokers.Helpers
{
	public static class ClientHelpers
	{
		private static readonly Random s_random = new Random();
		private static readonly DateTime s_epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		private static readonly ConcurrentDictionary<string, (string, string)> s_accessTokens = new ConcurrentDictionary<string, (string, string)>();

		public static int GetTimestamp()
		{
			var epochUtc = s_epoch;
			return (int)((DateTime.UtcNow - epochUtc).TotalSeconds);
		}

		public static string GetNonce()
		{
			return s_random.Next(123400, 9999999).ToString();
		}

		public static async Task<Dictionary<string, string>> GetDefaultRequestParameters(string verificationCode, string url, string clientId, string clientSecret, Func<string, Task<string>> func = null)
		{
			var nonce = GetNonce();
			var timestamp = GetTimestamp();

			var data = new Dictionary<string, string>
			{
				{ "oauth_consumer_key", clientId },
				{ "oauth_timestamp", timestamp.ToString() },
				{ "oauth_nonce", nonce },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_version", "1.0" }
			};

			if (s_accessTokens.ContainsKey(verificationCode))
			{
				data.Add("oauth_token", s_accessTokens[verificationCode].Item1);
			}
			else
			{
				data.Add("oauth_token", await func(verificationCode));
			}

			data.Add("oauth_signature", GetSignatureBaseString(url, data));

			return data;
		}

		public static AuthenticationHeaderValue GetDefaultAuthHeader(IDictionary<string, string> data, bool addRealm = true)
		{
			return new AuthenticationHeaderValue(
				"OAuth", (addRealm ? @"realm=""limited_poa""," : string.Empty) + GenerateOAuthHeader(data)
			);
		}

		private static string GenerateOAuthHeader(IDictionary<string, string> data)
		{
			return string.Join(
				",",
				data
					.OrderBy(x => x.Key)
					.Where(kvp => kvp.Key.StartsWith("oauth_") || kvp.Key.StartsWith("diffie") || kvp.Key.StartsWith("realm"))
					.Select(kvp => string.Format("{0}=\"{1}\"", kvp.Key, Uri.EscapeDataString(kvp.Value)))
			);
		}

		private static bool ValidateTokenAndSecret(string requestToken, string requestSecret)
		{
			if (!string.IsNullOrEmpty(requestToken) && !string.IsNullOrEmpty(requestSecret))
				return true;

			return false;
		}

		public static async Task<IDictionary<string, string>> ToKeyValueAsync(this object objectToConvert)
		{
			if (objectToConvert == null)
			{
				return null;
			}
			if (objectToConvert is string)
			{
				return new Dictionary<string, string>
				{
					{ "", objectToConvert.ToString() }
				};
			}

			var doc = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(objectToConvert));
			var dic = new Dictionary<string, string>();
			var obj = doc.RootElement.EnumerateObject();
			if (!obj.Any())
			{
				dic.Add("", obj.ToString());
			}

			void Parse(JsonElement element, string prefix = null)
			{
				if (doc.RootElement.ValueKind == JsonValueKind.Object)
				{
					foreach (var property in doc.RootElement.EnumerateObject())
					{
						if (property.Value.ValueKind == JsonValueKind.Array)
						{
							foreach (var (index, jsonProperty) in doc.RootElement.EnumerateArray().Select((item, i) => (i, item)))
							{
								if (jsonProperty.ValueKind == JsonValueKind.Object)
								{
									Parse(jsonProperty, $"{property}[{index}]");
									continue;
								}

								dic.Add($"{property}[{index}]", jsonProperty.GetRawText().Trim('"'));

							}
							continue;
						}
						dic.Add(prefix != null ? $"${prefix}[${property.Name}]" : property.Name, property.Value.GetRawText().Trim('"'));
					}
				}
			}

			Parse(doc.RootElement);

			return dic;
		}

		public static string GetSignatureBaseString(string url, IDictionary<string, string> data, string secret = null, string sigMethod = "RSA-SHA256", string httpMethod = "POST")
		{

			using var reader = File.OpenText(@"private_signature.pem");
			var keyPair = (AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();
			var sigString = string.Join(
			"&",
			data
				.OrderBy(x => x.Key.StartsWith("oauth") ? "zzz" + x.Key : x.Key)
				.Select(kvp => string.Format("{0}={1}", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value)))
		);

			string superSecretString = null;
			if (secret != null && sigMethod.StartsWith("RSA"))
			{
				var secretBytes = Convert.FromBase64String(secret);
				using var encryptionPem = File.OpenText(@"private_encryption.pem");
				var encryptionKeyPair = (AsymmetricCipherKeyPair)new PemReader(encryptionPem).ReadObject();
				IAsymmetricBlockCipher e = new RsaEngine();
				e = new Pkcs1Encoding(e);
				e.Init(false, encryptionKeyPair.Private);
				var result = e.ProcessBlock(secretBytes, 0, secretBytes.Length);
				superSecretString = BitConverter.ToString(result).Replace("-", "").ToLower();
			}

			var urlSplit = url.Split("?");


			var fullSigData = string.Format(
				"{0}&{1}&{2}",
				(superSecretString != null ? superSecretString.ToString() : "") + httpMethod,
				Uri.EscapeDataString(urlSplit.FirstOrDefault()),
				Uri.EscapeDataString((urlSplit.Length == 2 ? urlSplit.LastOrDefault() + "&" : "") + sigString)
			);
			var bytes = Encoding.UTF8.GetBytes(fullSigData.ToString());



			if (sigMethod.StartsWith("RSA"))
			{
				var sha256 = new Sha256Digest();
				var rsa = new RsaDigestSigner(sha256);
				rsa.Init(true, keyPair.Private);
				rsa.BlockUpdate(bytes, 0, bytes.Length);
				return Convert.ToBase64String(rsa.GenerateSignature());
			}
			else
			{
				var h = new HMACSHA256(Convert.FromBase64String(secret));
				var hashedBytes = h.ComputeHash(bytes);
				var s = Convert.ToBase64String(hashedBytes);
				return s;

				//var sha256 = new Sha256Digest();
				//var hmac = new HMac(new Sha256Digest());
				//hmac.Init(new KeyParameter(Convert.FromBase64String("YBWbLw+9RYP2nWrPQHxHZkBb1aM=")));
				//var encodedBytes = Encoding.UTF8.GetBytes(@"GET&http%3A%2F%2Flocalhost%3A12345%2Ftradingapi%2Fv1%2Fmarketdata%2Fsnapshot&conid%3D8314%26oauth consumer key%3DTESTCONS%26oauth nonce%3Daecef17086308940e861%26oauth signature method%3DHMAC-SHA256%26oauth timestamp%3D1473795686%26oauth token%3D6f531f8fd316915af53f");
				//byte[] result = new byte[hmac.GetMacSize()];
				//hmac.BlockUpdate(encodedBytes, 0, encodedBytes.Length);
				//hmac.DoFinal(result, 0);
				//return Convert.ToBase64String(result);
			}
		}


	}
}
