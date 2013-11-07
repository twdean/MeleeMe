using System;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using System.Web.Security;

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
            var auth = GetAuthorizer(twitterUserId);

            auth.CompleteAuthorization(Request.Url);

            if (!auth.IsAuthorized)
            {
                var specialUri = new Uri("/Home/MyProfile");
                return auth.BeginAuthorization(specialUri);
            }

            var twitterCtx = new TwitterContext(auth);

            UserModel mUser = GetCurrentUser(twitterCtx, auth, twitterUserId);

            return View(mUser);
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
        public ActionResult Challenge()
        {
            //pull back a list of melee users with their stats.
            return null;
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
