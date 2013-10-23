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


namespace Melee.Me.Controllers
{
    public class ConnectionsController : Controller
    {
        private readonly IConnectionRepository _repository;
        private Uri FacebookRedirectUri
        {
            get
            {
                var uriBuilder = new UriBuilder(Request.Url);
                uriBuilder.Query = null;
                uriBuilder.Fragment = null;
                uriBuilder.Path = Url.Action("FacebookCallback");
                return uriBuilder.Uri;
            }
        }

        private Uri GoogleRedirectUri
        {
            get
            {
                var uriBuilder = new UriBuilder(Request.Url);
                uriBuilder.Query = null;
                uriBuilder.Fragment = null;
                uriBuilder.Path = Url.Action("GoogleCallback");
                return uriBuilder.Uri;
            }
        }


        public ConnectionsController()
        {
            _repository = new ConnectionRepository();
        }

        public ActionResult Google()
        {
            var uri = GetServiceLoginUrl(GoogleRedirectUri);

            return Redirect(uri.AbsoluteUri);
        }

        public ActionResult GoogleCallback(string code)
        {
            var meleeUser = (UserModel)Session["challenger"];
            var accessToken = QueryAccessToken(GoogleRedirectUri, code);

            if (Session["challenger"] != null)
            {
                meleeUser = (UserModel)Session["challenger"];
                meleeUser.Connections.Add(_repository.Add(meleeUser.UserId, "Google", accessToken));
                Session["challenger"] = meleeUser;
            }

            return RedirectToAction("MyProfile", "Home", meleeUser);

        }

        private Uri GetServiceLoginUrl(Uri returnUrl)
        {
            const string authorizationEndpoint = "https://accounts.google.com/o/oauth2/auth";
            var profileScope = "https://www.googleapis.com/auth/userinfo#profile";
            var emailScope = "https://www.googleapis.com/auth/userinfo#email";
            var scope = profileScope + " " + emailScope;

            return BuildUri(authorizationEndpoint, new NameValueCollection
                {
                    { "response_type", "code" },
                    { "client_id", "894864914110-vv4ahuco0e3dm97njeb752hdc67k0q2f.apps.googleusercontent.com" },
                    { "scope", "email profile"},
                    { "redirect_uri", returnUrl.AbsoluteUri },
                    { "access_type", "offline"},
                    { "state", "" },
                });
        }

        private string QueryAccessToken(Uri returnUrl, string authorizationCode)
        {
            string TokenEndpoint = "https://accounts.google.com/o/oauth2/token";
            var postData = HttpUtility.ParseQueryString(string.Empty);
            postData.Add(new NameValueCollection
                {
                    { "grant_type", "authorization_code" },
                    { "code", authorizationCode },
                    { "client_id", "894864914110-vv4ahuco0e3dm97njeb752hdc67k0q2f.apps.googleusercontent.com" },
                    { "client_secret", "rDMDGAWoYoiWmE9gcYLweKRQ" },
                    { "redirect_uri", returnUrl.GetLeftPart(UriPartial.Path) },
                });

            var webRequest = (HttpWebRequest)WebRequest.Create(TokenEndpoint);

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
                    //var response = reader.ReadToEnd();
                    //var json = JObject.Parse(response);
                    //var accessToken = json.Value<string>("access_token");
                    //return accessToken;

                    return null;
                }
            }
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
                meleeUser.Connections.Add(_repository.Add(meleeUser.UserId, "Facebook", accessToken));
                Session["challenger"] = meleeUser;
            }

            return RedirectToAction("MyProfile", "Home", meleeUser);
        }

    }
}
