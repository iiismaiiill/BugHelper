using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;

using Newtonsoft.Json;


namespace BugHelper
{
    public class FacebookClient : OAuth2Client
    {
        /// <summary>
        /// The app id.
        /// </summary>
        private readonly string _appId;

        /// <summary>
        /// The app secret.
        /// </summary>
        private readonly string _appSecret;

        /// <summary>
        /// The requested scopes.
        /// </summary>
        private readonly string[] _requestedScopes;

        /// <summary>
        /// The requested fields to return from the user profile.
        /// </summary>
        private readonly string[] _userProfileFields;

        /// <summary>
        /// The authorization endpoint.
        /// </summary>
        private readonly string _authorizationEndpoint;

        /// <summary>
        /// The token endpoint.
        /// </summary>
        private readonly string _tokenEndpoint;

        /// <summary>
        /// The user info endpoint.
        /// </summary>
        private readonly string _userInfoEndpoint;


        /// <summary>
        /// Creates a new Facebook OAuth2 client, requesting the default "email" scope.
        /// </summary>
        /// <param name="appId">The Facebook App Id</param>
        /// <param name="appSecret">The Facebook App Secret</param>
        /// <param name="facebookVersion">Version of the Facebook graph api being accessed. This should come in the format of "vX.Y" (i.e. "v3.2"). Default is "v3.2"</param>
        public FacebookClient(string appId, string appSecret, string facebookVersion = "v3.2") : this(appId, appSecret, new[] { "email" }, new[] { "email", "name" }, facebookVersion) { }


        /// <summary>
        /// Creates a new Facebook OAuth2 client.
        /// </summary>
        /// <param name="appId">The Facebook App Id</param>
        /// <param name="appSecret">The Facebook App Secret</param>
        /// <param name="requestedScopes">One or more requested scopes, passed without the base URI.</param>
        /// <param name="userProfileFields">One or more requested default user profile fields from facebook. These are limited to "first_name", "last_name", "middle_name", "name", "name_format", "picture", "short_name"</param>
        /// <param name="facebookVersion">Version of the Facebook graph api being accessed. This should come in the format of "vX.Y" (i.e. "v3.2"). Default is "v3.2"</param>
        public FacebookClient(string appId, string appSecret, string[] requestedScopes, string[] userProfileFields, string facebookVersion = "v3.2") : base("facebook")
        {
            if (string.IsNullOrWhiteSpace(appId))
                throw new ArgumentNullException(nameof(appId));

            if (string.IsNullOrWhiteSpace(appSecret))
                throw new ArgumentNullException(nameof(appSecret));

            if (requestedScopes == null)
                throw new ArgumentNullException(nameof(requestedScopes));

            if (requestedScopes.Length == 0)
                throw new ArgumentException("One or more scopes must be requested.", nameof(requestedScopes));

            _appId = appId;
            _appSecret = appSecret;
            _requestedScopes = requestedScopes;
            _userProfileFields = userProfileFields ?? new string[0];
            var facebookVersion1 = facebookVersion;

            _authorizationEndpoint = $"https://www.facebook.com/{facebookVersion1}/dialog/oauth";
            _tokenEndpoint = $"https://graph.facebook.com/{facebookVersion1}/oauth/access_token";
            _userInfoEndpoint = $"https://graph.facebook.com/{facebookVersion1}/me";
        }


        public override void RequestAuthentication(HttpContextBase context, Uri returnUrl)
        {
            var redirectUrl = GetServiceLoginUrl(returnUrl).AbsoluteUri;
            context.Response.Redirect(redirectUrl, endResponse: true);
        }


        public new AuthenticationResult VerifyAuthentication(HttpContextBase context)
        {
            throw new NoNullAllowedException();
        }


        public override AuthenticationResult VerifyAuthentication(HttpContextBase context, Uri returnPageUrl)
        {
            var code = context.Request.QueryString["code"];
            if (string.IsNullOrEmpty(code))
            {
                return AuthenticationResult.Failed;
            }

            var accessToken = QueryAccessToken(returnPageUrl, code);
            if (accessToken == null)
            {
                return AuthenticationResult.Failed;
            }

            var userData = GetUserData(accessToken);
            if (userData == null)
            {
                return AuthenticationResult.Failed;
            }

            var id = userData["id"];
            string name;

            // Some oAuth providers do not return value for the 'username' attribute. 
            // In that case, try the 'name' attribute. If it's still unavailable, fall back to 'id'
            if (!userData.TryGetValue("username", out name) && !userData.TryGetValue("name", out name))
            {
                name = id;
            }

            // add the access token to the user data dictionary just in case page developers want to use it
            userData["accesstoken"] = accessToken;

            return new AuthenticationResult(
                isSuccessful: true, provider: ProviderName, providerUserId: id, userName: name, extraData: userData);
        }


        public string QueryAccessTokenByCode(Uri returnUrl, string authorizationCode)
        {
            return QueryAccessToken(returnUrl, authorizationCode);
        }


        /// <summary>
        /// Facebook works best when return data be packed into a "state" parameter.
        /// This should be called before verifying the request, so that the url is rewritten to support this.
        /// </summary>
        public static void RewriteRequest()
        {
            var ctx = HttpContext.Current;

            var stateString = HttpUtility.UrlDecode(ctx.Request.QueryString["state"]);
            if (stateString == null || !stateString.Contains("__provider__=facebook"))
                return;

            var q = HttpUtility.ParseQueryString(stateString);
            q.Add(ctx.Request.QueryString);
            q.Remove("state");

            ctx.RewritePath(ctx.Request.Path + "?" + q);
        }


        private static Uri BuildUri(string baseUri, NameValueCollection queryParameters)
        {
            var keyValuePairs = queryParameters.AllKeys.Select(k => HttpUtility.UrlEncode(k) + "=" + HttpUtility.UrlEncode(queryParameters[k]));
            var qs = string.Join("&", keyValuePairs);

            var builder = new UriBuilder(baseUri) { Query = qs };
            return builder.Uri;
        }


        protected override Uri GetServiceLoginUrl(Uri returnUrl)
        {
            var state = string.IsNullOrEmpty(returnUrl.Query) ? string.Empty : returnUrl.Query.Substring(1);

            return BuildUri(_authorizationEndpoint, new NameValueCollection
            {
                {"client_id", _appId},
                {"scope", $"{string.Join(" ", _requestedScopes)}"},
                {"redirect_uri", returnUrl.GetLeftPart(UriPartial.Path)},
                {"state", state}
            });
        }


        protected override IDictionary<string, string> GetUserData(string accessToken)
        {
            var uri = BuildUri(_userInfoEndpoint, new NameValueCollection { { "access_token", accessToken }, { "fields", string.Join(",", _userProfileFields) } });

            var webRequest = (HttpWebRequest)WebRequest.Create(uri);

            using (var webResponse = webRequest.GetResponse())
            using (var stream = webResponse.GetResponseStream())
            {
                if (stream == null)
                    return null;

                using (var textReader = new StreamReader(stream))
                {
                    var json = textReader.ReadToEnd();
                    var extraData = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    var data = extraData.ToDictionary(x => x.Key, x => x.Value.ToString());

                    data.Add("picture", $"https://graph.facebook.com/{data["id"]}/picture");

                    return data;
                }
            }
        }


        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode)
        {
            var uri = BuildUri(_tokenEndpoint, new NameValueCollection
            {
                {"code", authorizationCode},
                {"client_id", _appId},
                {"client_secret", _appSecret},
                {"redirect_uri", returnUrl.GetLeftPart(UriPartial.Path)}
            });

            var webRequest = (HttpWebRequest)WebRequest.Create(uri);
            string accessToken;
            var response = (HttpWebResponse)webRequest.GetResponse();

            // handle response from FB 
            // this will not be a url with params like the first request to get the 'code'
            var rEncoding = Encoding.GetEncoding(response.CharacterSet ?? throw new InvalidOperationException("Response from getting the facebook access token was invalid."));

            using (var sr = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException("Response from getting the facebook access token was invalid"), rEncoding))
            {
                var serializer = new JavaScriptSerializer();
                var jsonObject = serializer.DeserializeObject(sr.ReadToEnd());
                var jConvert = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(jsonObject));

                var deserializedJsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jConvert.ToString());
                accessToken = deserializedJsonObject["access_token"].ToString();
            }

            return accessToken;
        }
    }
}