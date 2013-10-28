using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using Facebook;
using Melee.Me.Infrastructure;
using Melee.Me.Infrastructure.Repository;
using Melee.Me.Models;
using Newtonsoft.Json.Linq;

namespace Melee.Me.Controllers
{
    public class ConnectionsController : Controller
    {
        private readonly IConnectionRepository _repository;

        public ConnectionsController()
        {
            _repository = new ConnectionRepository();
        }

        private Uri FacebookRedirectUri
        {
            get
            {
                var uriBuilder = new UriBuilder(Request.Url)
                    {
                        Query = null,
                        Fragment = null,
                        Path = Url.Action("FacebookCallback")
                    };
                return uriBuilder.Uri;
            }
        }

        private Uri GoogleRedirectUri
        {
            get
            {
                var uriBuilder = new UriBuilder(Request.Url)
                    {
                        Query = null,
                        Fragment = null,
                        Path = Url.Action("GoogleCallback")
                    };
                return uriBuilder.Uri;
            }
        }

        private Uri LinkedInRedirectUri
        {
            get
            {
                var uriBuilder = new UriBuilder(Request.Url)
                    {
                        Query = null,
                        Fragment = null,
                        Path = Url.Action("LinkedInCallback")
                    };
                return uriBuilder.Uri;
            }
        }

        public ActionResult LinkedIn()
        {
            var meleeUser = (UserModel)Session["challenger"];
            if (meleeUser.Connections.All(c => c.ConnectionName != "LinkedIn"))
            {
                var uri = GetServiceLoginUrl(LinkedInRedirectUri);

                return Redirect(uri.AbsoluteUri);
            }
            else
            {
                _repository.Delete(meleeUser.UserId,
                                   meleeUser.Connections.First(c => c.ConnectionName == "LinkedIn").ConnectionId);
                meleeUser.Connections.Remove(meleeUser.Connections.First(c => c.ConnectionName == "LinkedIn"));
            }

            return View("../Home/MyProfile", meleeUser);
        }

        public ActionResult Google()
        {
            var meleeUser = (UserModel)Session["challenger"];
            if (meleeUser.Connections.All(c => c.ConnectionName != "Google"))
            {
                var uri = GetServiceLoginUrl(GoogleRedirectUri);

                return Redirect(uri.AbsoluteUri);
            }
            else
            {
                _repository.Delete(meleeUser.UserId,
                                   meleeUser.Connections.First(c => c.ConnectionName == "Google").ConnectionId);
                meleeUser.Connections.Remove(meleeUser.Connections.First(c => c.ConnectionName == "Google"));
            }

            return View("../Home/MyProfile", meleeUser);
        }

        public ActionResult Facebook()
        {
            var meleeUser = (UserModel)Session["challenger"];
            if (meleeUser.Connections.All(c => c.ConnectionName != "Facebook"))
            {
                var facebookKey = ConfigurationManager.AppSettings["FacebookKey"];
                var facebookSecret = ConfigurationManager.AppSettings["FacebookSecret"];


                var fb = new FacebookClient();
                var loginUrl = fb.GetLoginUrl(new
                {
                    client_id = facebookKey,
                    client_secret = facebookSecret,
                    redirect_uri = FacebookRedirectUri.AbsoluteUri,
                    response_type = "code",
                    scope = "email,user_status, user_photos, read_stream, read_insights, user_online_presence"
                });

                return Redirect(loginUrl.AbsoluteUri);
            }
            else
            {
                _repository.Delete(meleeUser.UserId,
                                   meleeUser.Connections.First(c => c.ConnectionName == "Facebook").ConnectionId);
                meleeUser.Connections.Remove(meleeUser.Connections.First(c => c.ConnectionName == "Facebook"));
            }

            return View("../Home/MyProfile", meleeUser);
        }

        public ActionResult FacebookCallback(string code)
        {
            var meleeUser = null as UserModel;

            var facebookKey = ConfigurationManager.AppSettings["FacebookKey"];
            var facebookSecret = ConfigurationManager.AppSettings["FacebookSecret"];

            var fb = new FacebookClient();
            dynamic result = fb.Post("oauth/access_token", new
            {
                client_id = facebookKey,
                client_secret = facebookSecret,
                redirect_uri = FacebookRedirectUri.AbsoluteUri,
                code = code
            });

            var accessToken = result.access_token;

            if (Session["challenger"] != null)
            {
                meleeUser = (UserModel)Session["challenger"];
                meleeUser.Connections.Add(_repository.Add(meleeUser.UserId, "Facebook", accessToken, ""));
                Session["challenger"] = meleeUser;
            }

            return RedirectToAction("MyProfile", "Home", meleeUser);
        }

        public ActionResult LinkedInCallback(string code)
        {
            var meleeUser = (UserModel)Session["challenger"];
            var accessToken = QueryLinkedInAccessToken(LinkedInRedirectUri, code);

            if (Session["challenger"] != null)
            {
                meleeUser = (UserModel)Session["challenger"];
                meleeUser.Connections.Add(_repository.Add(meleeUser.UserId, "LinkedIn", accessToken, ""));
                Session["challenger"] = meleeUser;
            }

            return RedirectToAction("MyProfile", "Home", meleeUser);

        }

        public ActionResult GoogleCallback(string code)
        {
            var meleeUser = (UserModel)Session["challenger"];
            var authorizationResponse = GetAuthorizationResponse(GoogleRedirectUri, code);
            var accessToken = QueryGoogleAccessToken(authorizationResponse);
            var refreshToken = QueryGoogleRefreshToken(authorizationResponse);


            if (Session["challenger"] != null)
            {
                meleeUser = (UserModel)Session["challenger"];
                meleeUser.Connections.Add(_repository.Add(meleeUser.UserId, "Google", accessToken, refreshToken));
                Session["challenger"] = meleeUser;
            }

            return RedirectToAction("MyProfile", "Home", meleeUser);

        }

        private string QueryLinkedInAccessToken(Uri returnUrl, string authorizationCode)
        {
            var googleClientId = ConfigurationManager.AppSettings["GoogleClientId"];
            var googleClientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"];

            const string tokenEndpoint = "https://accounts.google.com/o/oauth2/token";
            var postData = HttpUtility.ParseQueryString(string.Empty);
            postData.Add(new NameValueCollection
                {
                    { "grant_type", "authorization_code" },
                    { "code", authorizationCode },
                    { "client_id", googleClientId },
                    { "client_secret", googleClientSecret },
                    { "redirect_uri", returnUrl.GetLeftPart(UriPartial.Path) },
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
                    var accessToken = json.Value<string>("access_token");
                    return accessToken;
                }
            }
        }

        private Uri GetServiceLoginUrl(Uri returnUrl)
        {
            var googleClientId = ConfigurationManager.AppSettings["GoogleClientId"];

            const string authorizationEndpoint = "https://accounts.google.com/o/oauth2/auth";
            const string profileScope = "https://www.googleapis.com/auth/userinfo.profile";
            const string emailScope = "https://www.googleapis.com/auth/userinfo.email";
            const string gPlusScope = "https://www.googleapis.com/auth/plus.login";

            var scope = profileScope + " " + emailScope + " " + gPlusScope;

            return BuildUri(authorizationEndpoint, new NameValueCollection
                {
                    { "response_type", "code" },
                    { "client_id", googleClientId },
                    { "scope", scope},
                    { "redirect_uri", returnUrl.AbsoluteUri },
                    { "access_type", "offline"},
                    { "state", "" }
                });
        }

        private JObject GetAuthorizationResponse(Uri returnUrl, string authorizationCode)
        {
            var googleClientId = ConfigurationManager.AppSettings["GoogleClientId"];
            var googleClientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"];

            const string tokenEndpoint = "https://accounts.google.com/o/oauth2/token";
            var postData = HttpUtility.ParseQueryString(string.Empty);
            postData.Add(new NameValueCollection
                {
                    { "grant_type", "authorization_code" },
                    { "code", authorizationCode },
                    { "client_id", googleClientId },
                    { "client_secret", googleClientSecret },
                    { "redirect_uri", returnUrl.GetLeftPart(UriPartial.Path) }
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

        //
        // GET: /Connections/



    }
}
