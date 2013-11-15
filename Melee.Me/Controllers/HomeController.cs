using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using System.Web.Security;
using System.Collections.Generic;

using LinqToTwitter;
using Melee.Me.Infrastructure.Repository;
using Melee.Me.Models;

namespace Melee.Me.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserRepository _repository;

        public HomeController()
        {
            _repository = new MeleeUserRepository();
        }

        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ExternalLogin(string returnUrl)
        {
            MvcAuthorizer auth = GetAuthorizer(null);
            var twitterReturnUrl = ConfigurationManager.AppSettings["TwitterAutorizationReturnUrl"];


            auth.CompleteAuthorization(Request.Url);

            if (!auth.IsAuthorized)
            {
                var specialUri = new Uri(twitterReturnUrl);
                return auth.BeginAuthorization(specialUri);
            }

            return AppAuthorizationConfirmation(null);
        }

        [AllowAnonymous]
        public ActionResult AppAuthorizationConfirmation(string returnUrl)
        {
            MeleeModel mm;
            UserModel challengerModel;
            UserModel competitorModel;

            try
            {

                var auth = GetAuthorizer(null);

                auth.CompleteAuthorization(Request.Url);

                if (!auth.IsAuthorized)
                {
                    var specialUri = new Uri("/Home/AppAuthorizationConfirmation");
                    return auth.BeginAuthorization(specialUri);
                }

                var twitterCtx = new TwitterContext(auth);

                challengerModel = GetCurrentUser(twitterCtx, auth, auth.Credentials.UserId);
                competitorModel = new UserModel
                    {
                        ImageUrl = "/Content/images/default_user.png",
                        ScreenName = String.Empty,
                        TwitterUserId = String.Empty
                    };

                mm = new MeleeModel
                    {
                        Challenger = challengerModel,
                        Competitor = competitorModel
                    };
            }
            catch (TwitterQueryException tqEx)
            {
                return View("Error");
            }

            SetAuthCookie(challengerModel.TwitterUserId);

            TempData["MeleeModel"] = mm;
            return RedirectToAction("Login");
        }

        //TODO:  Add internal user attribute
        public ActionResult Login()
        {
            var mm = TempData["MeleeModel"] as MeleeModel;
            return View("SocialSignInConfirmation", mm);
        }

        [Authorize]
        public ActionResult MyProfile(string twitterUserId)
        {
            UserModel mUser = null;
            MvcAuthorizer auth = null;

            try
            {
                auth = GetAuthorizer(twitterUserId);

                auth.CompleteAuthorization(Request.Url);

                if (!auth.IsAuthorized)
                {
                    var specialUri = new Uri("/Home/MyProfile");
                    return auth.BeginAuthorization(specialUri);
                }

                var twitterCtx = new TwitterContext(auth);

                mUser = GetCurrentUser(twitterCtx, auth, twitterUserId);

            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = ex.ToString() + " twitterUserId: " + twitterUserId;
            }


            return View(mUser);
        }

        protected UserModel UserModel
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        [Authorize]
        public ActionResult Draggable()
        {
            return View("MyProfile");
        }

        [Authorize]
        public ActionResult History(string twitterUserId)
        {
            UserModel mUser = _repository.Get(twitterUserId);

            return View(mUser);
        }

        [Authorize]
        public ActionResult Challenge(string twitterUserId)
        {
            //pull back a list of melee users with their stats.
            MvcAuthorizer auth = GetAuthorizer(twitterUserId);
            var twitterCtx = new TwitterContext(auth);

            var listOfFriends = FriendList(twitterCtx, twitterUserId);



            return listOfFriends;
        }

        [Authorize]
        public ActionResult GetNewCompetitor(string twitterUserId)
        {
            MvcAuthorizer auth = GetAuthorizer(twitterUserId);

            auth.CompleteAuthorization(Request.Url);

            if (!auth.IsAuthorized)
            {
                var specialUri = new Uri("/Home/AppAuthorizationConfirmation");
                return auth.BeginAuthorization(specialUri);
            }

            var twitterCtx = new TwitterContext(auth);
            var competitor = FriendSelector(twitterCtx, twitterUserId);

            return Json(competitor);
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }

        private JsonResult FriendList(TwitterContext twitterCtx, string tUserId)
        {
            var fList = string.Empty;
            var listCollection = new Collection<string>();
            var friendCounter = 0;
            var userCollection = new Collection<User>();

            try
            {
                var friendList =
                    (from friend in twitterCtx.SocialGraph
                     where friend.Type == SocialGraphType.Friends &&
                           friend.UserID == Convert.ToUInt64(tUserId)
                     select friend)
                     .SingleOrDefault();

                var followerList =
                    (from follower in twitterCtx.SocialGraph
                     where follower.Type == SocialGraphType.Followers &&
                           follower.UserID == Convert.ToUInt64(tUserId)
                     select follower)
                     .SingleOrDefault();

                if (friendList != null)
                {
                    foreach (var id in friendList.IDs)
                    {
                        if (friendCounter == 100)
                        {
                            listCollection.Add(fList.Trim(','));
                            fList = string.Empty;
                            friendCounter = 0;
                        }

                        fList += "," + id;
                        friendCounter++;
                    }

                    foreach (var s in listCollection)
                    {
                        var myFriends =
                            (from user in twitterCtx.User
                             where user.Type == UserType.Lookup &&
                                   user.UserID == s
                             select user).ToList();

                        foreach (var f in myFriends)
                        {
                            userCollection.Add(f);
                        }
                    }

                }



                return Json(userCollection);
            }
            catch (TwitterQueryException tqEx)
            {
                if (tqEx.ErrorCode == 88)
                {

                }
            }

            return null;
        }


        private UserModel FriendSelector(TwitterContext twitterCtx, string tUserId)
        {
            var competitor = null as UserModel;

            try
            {
                var friendList =
                    (from friend in twitterCtx.SocialGraph
                     where friend.Type == SocialGraphType.Friends &&
                           friend.UserID == Convert.ToUInt64(tUserId)
                     select friend)
                     .SingleOrDefault();

                int random = new Random().Next(friendList.IDs.Count);
                var tUser =
                    (from user in twitterCtx.User
                     where user.Type == UserType.Lookup &&
                           user.UserID == friendList.IDs[random]
                     select user).FirstOrDefault();

                var meleeOponent = _repository.Get(tUser.UserID);

                if (meleeOponent != null)
                {
                    competitor = meleeOponent;
                }
                else
                {
                    competitor = new UserModel
                    {
                        TwitterUserId = tUser.UserID,
                        ScreenName = tUser.Name,
                        ImageUrl = tUser.ProfileImageUrl,
                        Stats = new MeleeStatisticsModel()
                    };
                }
            }
            catch (TwitterQueryException tqEx)
            {
                if (tqEx.ErrorCode == 88)
                {

                }
            }

            return competitor;
        }

        private MvcAuthorizer GetAuthorizer(string twitterUserId)
        {
            var twitterKey = ConfigurationManager.AppSettings["TwitterConsumerKey"];
            var twitterSecret = ConfigurationManager.AppSettings["TwitterConsumerSecret"];

            IOAuthCredentials credentials = new InMemoryCredentials();

            if (credentials.ConsumerKey == null || credentials.ConsumerSecret == null)
            {
                credentials.ConsumerKey = twitterKey;
                credentials.ConsumerSecret = twitterSecret;
            }

            if (!string.IsNullOrEmpty(twitterUserId))
            {
                var challengerModel = _repository.Get(twitterUserId);
                credentials.AccessToken =
                    challengerModel.Connections.Single(x => x.ConnectionName == "Twitter").AccessToken;
                credentials.OAuthToken =
                    challengerModel.Connections.Single(x => x.ConnectionName == "Twitter").OAuthToken;
            }


            var auth = new MvcAuthorizer
                {
                    Credentials = credentials
                };

            return auth;
        }

        private UserModel GetCurrentUser(TwitterContext twitterCtx, MvcAuthorizer auth, string twitterUserId)
        {
            var challenger = _repository.Get(twitterUserId);

            if (challenger == null)
            {
                var tUser =
                    (from user in twitterCtx.User
                     where user.Type == UserType.Lookup &&
                           user.UserID == twitterUserId
                     select user).FirstOrDefault();

                if (tUser != null)
                {
                    challenger = _repository.Add(tUser.Identifier.UserID, tUser.ProfileImageUrl, tUser.Identifier.ScreenName, auth.Credentials.AccessToken, auth.Credentials.OAuthToken);
                }
            }

            return challenger;
        }

        //private UserModel GetCompetitor(TwitterContext twitterCtx, string twitterUserId)
        //{
        //    UserModel competitor;

        //    if (Session["competitor"] != null)
        //    {
        //        competitor = (UserModel)Session["competitor"];
        //    }
        //    else
        //    {
        //        competitor = FriendSelector(twitterCtx, twitterUserId);
        //        competitor.Stats = MeleeRepository.GetMeleeStats(twitterUserId, Types.UserType.Opponent);


        //        Session.Add("competitor", competitor);
        //    }

        //    return competitor;
        //}

        private void SetAuthCookie(string username)
        {
            FormsAuthentication.SetAuthCookie(username, false);
        }

    }
}
