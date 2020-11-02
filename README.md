Three files need to be in working directoy

private_signature.pem
private_encryption.pem
dhparam.pem

With these provide your key.

Call 
IBClient.Configure("{key}", "", "{redirect}", false); //used for httpclient IOC
var client = new IBClient(new System.Net.Http.HttpClient()); //This also takes params


var redirect = await client.GetOAuthRedirectUrlAsync();
var accessToken = await client.GetAccessToken("", "", AuthorizationType.AuthorizationCode);
finally call
var liveSessionToken = await client.GetLiveSessionToken("", "");
This token will then be passed to subsequent calls.