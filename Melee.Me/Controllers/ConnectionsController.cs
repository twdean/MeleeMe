using System;
using System.Collections.Generic;
using System.Configuration;
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

        public ConnectionsController()
        {
            _repository = new ConnectionRepository();
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
                        redirect_uri = RedirectUri.AbsoluteUri,
                        response_type = "code",
                        scope = "email,user_status, user_photos, read_stream, read_insights, user_online_presence"
                        // Add other permissions as needed
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

        private Uri RedirectUri
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
                redirect_uri = RedirectUri.AbsoluteUri,
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
