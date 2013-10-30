using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using Melee.Me.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Melee.Me.Infrastructure.Connection
{
    public class GoogleConnection : IConnection
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public double GetScore(UserModel meleeUser)
        {
            AccessToken = meleeUser.Connections.Single(x => x.ConnectionName == "Google").AccessToken;
            RefreshToken = meleeUser.Connections.Single(x => x.ConnectionName == "Google").RefreshToken;


            GetUserData();
            double score = 0.00;

            return score;
        }

        private IDictionary<string, string> GetUserData()
        {
            const string userInfoEndpoint = "https://www.googleapis.com/oauth2/v1/userinfo";

            //https://www.googleapis.com/plus/v1/people/userId

            var uri = BuildUri(userInfoEndpoint, new NameValueCollection { { "access_token", AccessToken } });

            var webRequest = (HttpWebRequest)WebRequest.Create(uri);

            try
            {
                using (var webResponse = webRequest.GetResponse())
                using (var stream = webResponse.GetResponseStream())
                {
                    if (stream == null)
                        return null;

                    using (var textReader = new StreamReader(stream))
                    {
                        var json = textReader.ReadToEnd();
                        var extraData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                        return extraData;
                    }
                }
            }
            catch (Exception)
            {
                var authorizationResponse = GetAuthorizationResponse();
                AccessToken = QueryGoogleAccessToken(authorizationResponse);
                RefreshToken = QueryGoogleRefreshToken(authorizationResponse);

                return GetUserData();
            }

        }

        private JObject GetAuthorizationResponse()
        {
            var googleClientId = ConfigurationManager.AppSettings["GoogleClientId"];
            var googleClientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"];

            const string tokenEndpoint = "https://accounts.google.com/o/oauth2/token";
            var postData = HttpUtility.ParseQueryString(string.Empty);
            postData.Add(new NameValueCollection
                {
                    { "refresh_token", RefreshToken },
                    { "client_id", googleClientId },
                    { "client_secret", googleClientSecret },
                    { "grant_type", "refresh_token" }
                });

            var webRequest = (HttpWebRequest)WebRequest.Create(tokenEndpoint);

            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";

            using (var s = webRequest.GetRequestStream())
            using (var sw = new StreamWriter(s))
                sw.Write(postData.ToString());

            using (var webResponse = webRequest.GetResponse())
            {
                var responseStream = webResponse.GetResponseStream();
                if (responseStream == null)
                    return null;

                using (var reader = new StreamReader(responseStream))
                {
                    var response = reader.ReadToEnd();
                    var json = JObject.Parse(response);
                    return json;
                }
            }
        }

        private string QueryGoogleAccessToken(JObject json)
        {
            var accessToken = json.Value<string>("access_token");
            return accessToken;
        }

        private string QueryGoogleRefreshToken(JObject json)
        {
            var refreshToken = json.Value<string>("refresh_token");
            return refreshToken;
        }



        private static Uri BuildUri(string baseUri, NameValueCollection queryParameters)
        {
            var q = HttpUtility.ParseQueryString(string.Empty);
            q.Add(queryParameters);
            var builder = new UriBuilder(baseUri) { Query = q.ToString() };
            return builder.Uri;
        }



    }
}