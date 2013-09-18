﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;
using Facebook;
using Melee.Me.Models;

namespace Melee.Me.Controllers
{
    public class ConnectionsController : Controller
    {
        //
        // GET: /Connections/

        public ActionResult Index()
        {
            var meleeUser = null as UserModel;
            if (Session["challenger"] != null)
            {
                meleeUser = (UserModel) Session["challenger"];
                IEnumerable<ConnectionModel> connections = ConnectionModel.GetUserConnections(meleeUser.UserId);
            }

            return View("Connections", meleeUser);
        }

        public ActionResult Facebook()
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
                scope = "email" // Add other permissions as needed
            });

            return Redirect(loginUrl.AbsoluteUri);
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
            ConnectionModel.AddConnection(10, "Facebook", accessToken);

            if (Session["challenger"] != null)
            {
                meleeUser = (UserModel)Session["challenger"];
            }
            return RedirectToAction("MyProfile", "Home", meleeUser);
        }
    }
}
