using System;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using System.Web.Security;

using LinqToTwitter;
using Melee.Me.Models;

namespace Melee.Me.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserModel _userModel;

        public HomeController()
        {
            _userModel = new UserModel();
        }

        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ExternalLogin(string returnUrl)
        {
            MvcAuthorizer auth = GetAuthorizer();
            var twitterReturnUrl = ConfigurationManager.AppSettings["TwitterAutorizationReturnUrl"];


            auth.CompleteAuthorization(Request.Url);

            if (!auth.IsAuthorized)
            {
                Uri specialUri = new Uri(twitterReturnUrl);
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

                var auth = GetAuthorizer();

                auth.CompleteAuthorization(Request.Url);

                if (!auth.IsAuthorized)
                {
                    var specialUri = new Uri("/Home/AppAuthorizationConfirmation");
                    return auth.BeginAuthorization(specialUri);
                }

                var twitterCtx = new TwitterContext(auth);

                challengerModel = GetChallenger(twitterCtx, auth, auth.Credentials.UserId);
                competitorModel = GetCompetitor(twitterCtx, auth.Credentials.UserId);

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

            return View("SocialSignInConfirmation", mm);
        }

        [Authorize]
        public ActionResult MyProfile(string twitterUserId)
        {
            var auth = GetAuthorizer();

            auth.CompleteAuthorization(Request.Url);

            if (!auth.IsAuthorized)
            {
                var specialUri = new Uri("/Home/MyProfile");
                return auth.BeginAuthorization(specialUri);
            }

            var twitterCtx = new TwitterContext(auth);

            UserModel mUser = GetChallenger(twitterCtx, auth, auth.Credentials.UserId);

            return View(mUser);
        }

        [Authorize]
        public ActionResult History(string twitterUserId)
        {
            UserModel mUser = _userModel.Get(twitterUserId);


            return View(mUser);
        }

        [AllowAnonymous]
        public ActionResult GetNewCompetitor(string twitterUserId)
        {
            MvcAuthorizer auth = GetAuthorizer();
            TwitterContext twitterCtx;

            auth.CompleteAuthorization(Request.Url);

            if (!auth.IsAuthorized)
            {
                Uri specialUri = new Uri("/Home/AppAuthorizationConfirmation");
                return auth.BeginAuthorization(specialUri);
            }

            twitterCtx = new TwitterContext(auth);
            UserModel competitor = FriendSelector(twitterCtx, twitterUserId);

            if (Session["competitor"] != null)
            {
                Session["competitor"] = competitor;
            }
            else
            {
                Session.Add("competitor", competitor);
            }

            return Json(competitor);
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

                competitor = new UserModel
                    {
                        TwitterUserId = tUser.UserID,
                        ScreenName = tUser.Name,
                        ImageUrl = tUser.ProfileImageUrl,
                        Stats = new MeleeStatisticsModel()

                    };
            }
            catch (TwitterQueryException tqEx)
            {
                if (tqEx.ErrorCode == 88)
                {

                }
            }

            return competitor;
        }

        private MvcAuthorizer GetAuthorizer()
        {
            var twitterKey = ConfigurationManager.AppSettings["TwitterConsumerKey"];
            var twitterSecret = ConfigurationManager.AppSettings["TwitterConsumerSecret"];

            IOAuthCredentials credentials = new SessionStateCredentials();

            if (credentials.ConsumerKey == null || credentials.ConsumerSecret == null)
            {
                credentials.ConsumerKey = twitterKey;
                credentials.ConsumerSecret = twitterSecret;
            }

            var auth = new MvcAuthorizer
                {
                    Credentials = credentials
                };

            return auth;
        }

        private UserModel GetChallenger(TwitterContext twitterCtx, MvcAuthorizer auth, string twitterUserId)
        {
            var challenger = null as UserModel;

            if (Session["challenger"] != null)
            {
                challenger = (UserModel)Session["challenger"];
            }
            else
            {
                var tUser =
                    (from user in twitterCtx.User
                     where user.Type == UserType.Lookup &&
                           user.UserID == twitterUserId
                     select user).FirstOrDefault();

                if (tUser != null)
                {
                    challenger = _userModel.Add(tUser.Identifier.UserID, auth.Credentials.AccessToken);
                    challenger.ImageUrl = tUser.ProfileImageUrl;
                    challenger.ScreenName = tUser.Identifier.ScreenName;
                }

                Session.Add("challenger", challenger);
            }

            return challenger;
        }

        private UserModel GetCompetitor(TwitterContext twitterCtx, string twitterUserId)
        {
            UserModel competitor;

            if (Session["competitor"] != null)
            {
                competitor = (UserModel)Session["competitor"];
            }
            else
            {
                competitor = FriendSelector(twitterCtx, twitterUserId);
                competitor.Stats = MeleeStatisticsModel.GetMeleeStats(twitterUserId, Types.UserType.Opponent);


                Session.Add("competitor", competitor);
            }

            return competitor;
        }

        private void SetAuthCookie(string username)
        {
            FormsAuthentication.SetAuthCookie(username, false);
        }

    }

    public class Friend
    {
        public string TwitterUserId { get; set; }
        public string ScreenName { get; set; }
        public string ImageUrl { get; set; }

        public Friend()
        {

        }

        public Friend(string sName, string friendId, string imageUrl)
        {
            TwitterUserId = friendId;
            ScreenName = sName;
            ImageUrl = imageUrl;

        }
    }
}
