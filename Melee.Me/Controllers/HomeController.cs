using System;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using LinqToTwitter;
using Melee.Me.Models;

namespace Melee.Me.Controllers
{
    public class HomeController : Controller
    {
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
            var mm = new MeleeModel();

            try
            {

                IOAuthCredentials credentials = new SessionStateCredentials();
                var auth = GetAuthorizer();


                auth.CompleteAuthorization(Request.Url);

                if (!auth.IsAuthorized)
                {
                    var specialUri = new Uri("/Home/AppAuthorizationConfirmation");
                    return auth.BeginAuthorization(specialUri);
                }

                TwitterContext twitterCtx = new TwitterContext(auth);


                var tUser =
                    (from user in twitterCtx.User
                     where user.Type == UserType.Lookup &&
                           user.UserID == auth.Credentials.UserId
                     select user).FirstOrDefault();



                if (tUser != null)
                {

                    UserModel challenger = UserModel.AddUser(tUser.Identifier.UserID, auth.Credentials.AccessToken);
                    Friend competitor = FriendSelector(twitterCtx, tUser.UserID);

                    var challengerModel = new UserModel()
                        {
                            ImageUrl = tUser.ProfileImageUrl,
                            ScreenName = tUser.Identifier.ScreenName,
                            AccessToken = auth.Credentials.AccessToken,
                            TwitterUserId = tUser.Identifier.UserID,
                            Stats = challenger.Stats
                        };


                    var opponentModel = new UserModel()
                        {
                            ImageUrl = competitor.ImageUrl,
                            ScreenName = competitor.ScreenName,
                            TwitterUserId = competitor.TwitterUserId,
                            Stats = MeleeStatisticsModel.GetMeleeStats(tUser.Identifier.UserID, Types.UserType.Opponent)

                        };

                    mm = new MeleeModel()
                        {
                            Challenger = challengerModel,
                            Competitor = opponentModel
                        };
                }
            }
            catch (TwitterQueryException tqEx)
            {
                return View("Error");
            }


            return View("SocialSignInConfirmation", mm);
        }

        public Friend FriendSelector(TwitterContext twitterCtx, string tUserId)
        {
            Friend competitor = new Friend();

            int random = 0;

            var friendList =
                (from friend in twitterCtx.SocialGraph
                 where friend.Type == SocialGraphType.Friends &&
                       friend.UserID == Convert.ToUInt64(tUserId)
                 select friend)
                 .SingleOrDefault();

            random = new Random().Next(friendList.IDs.Count);
            var tUser =
                (from user in twitterCtx.User
                 where user.Type == UserType.Lookup &&
                       user.UserID == friendList.IDs[random]
                 select user).FirstOrDefault();

            competitor.TwitterUserId = tUser.UserID;
            competitor.ScreenName = tUser.Name;
            competitor.ImageUrl = tUser.ProfileImageUrl;



            return competitor;
        }

        public ActionResult GetCompetitor(string TwitterUserId)
        {

            IOAuthCredentials credentials = new SessionStateCredentials();
            MvcAuthorizer auth = GetAuthorizer();
            TwitterContext twitterCtx;


            auth.CompleteAuthorization(Request.Url);

            if (!auth.IsAuthorized)
            {
                Uri specialUri = new Uri("/Home/AppAuthorizationConfirmation");
                return auth.BeginAuthorization(specialUri);
            }

            twitterCtx = new TwitterContext(auth);
            Friend competitor = FriendSelector(twitterCtx, TwitterUserId);

            var opponentModel = new UserModel()
            {
                ImageUrl = competitor.ImageUrl,
                ScreenName = competitor.ScreenName,
                TwitterUserId = competitor.TwitterUserId,
                Stats = MeleeStatisticsModel.GetMeleeStats(TwitterUserId, Types.UserType.Opponent)
            };

            return Json(opponentModel);
        }

        public MvcAuthorizer GetAuthorizer()
        {
            IOAuthCredentials credentials = new SessionStateCredentials();
            MvcAuthorizer auth;

            if (credentials.ConsumerKey == null || credentials.ConsumerSecret == null)
            {
                credentials.ConsumerKey = "u2ULchA68sGq111YWL2foA";
                credentials.ConsumerSecret = "JcXAVK1GHaFMXtRLZASIviwDhQvtOLliaMKYfO0rY";
            }

            auth = new MvcAuthorizer
            {
                Credentials = credentials
            };

            return auth;
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
