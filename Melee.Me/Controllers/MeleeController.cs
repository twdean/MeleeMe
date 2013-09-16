using System;
using System.Linq;
using System.Web.Mvc;
using LinqToTwitter;
using Melee.Me.Models;

namespace Melee.Me.Controllers
{
    public class MeleeController : Controller
    {
        //
        // GET: /Melee/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MeleeMe(string challenger, string opponent)
        {
            TwitterContext twitterCtx = GetTwitterContext();

            UserModel result = GetMeleeData(twitterCtx, challenger, opponent);

            if (result == null)
            {
                return View("Error");
            }

            return View("Melee", result);
        }

        public UserModel GetMeleeData(TwitterContext twitterCtx, string challenger, string opponent)
        {
            var meleeWinner = null as UserModel;
            var meleeLoser = null as UserModel;

            try
            {
                var challengerTotal = 0;
                var opponentTotal = 0;

                var challengerResult = new MeleeResultModel();
                var opponentResult = new MeleeResultModel();

                try
                {
                    challengerResult.FriendScore = GetTwitterFollowerFriendCount(twitterCtx, challenger, SocialGraphType.Friends);
                    challengerResult.FollowerScore = GetTwitterFollowerFriendCount(twitterCtx, challenger, SocialGraphType.Followers);
                    challengerResult.PostScore = GetStatusUpdateCount(twitterCtx, challenger);
                    challengerResult.LikeFavouriteScore = GetLikesFavouritesCount(twitterCtx, challenger);

                    opponentResult.FriendScore = GetTwitterFollowerFriendCount(twitterCtx, opponent, SocialGraphType.Friends);
                    opponentResult.FollowerScore = GetTwitterFollowerFriendCount(twitterCtx, opponent, SocialGraphType.Followers);
                    opponentResult.PostScore = GetStatusUpdateCount(twitterCtx, opponent);
                    opponentResult.LikeFavouriteScore = GetLikesFavouritesCount(twitterCtx, opponent);

                    //DateTime cTweet = GetLatestTweet(twitterCtx, challenger);
                    //DateTime oTweet = GetLatestTweet(twitterCtx, opponent);

                }
                catch (TwitterQueryException tqEx)
                {
                    if (tqEx.ErrorCode == 88)
                    {

                    }
                }


                if (CalcMeleeResults(challengerResult) > CalcMeleeResults(opponentResult))
                {
                    meleeWinner = (UserModel) Session["challenger"];
                    meleeLoser = (UserModel)Session["competitor"];
                }
                else
                {
                    meleeWinner = (UserModel)Session["competitor"];
                    meleeLoser = (UserModel)Session["challenger"];
                }

                MeleeModel.AddMelee(challenger, opponent, meleeWinner.TwitterUserId, meleeLoser.TwitterUserId);
            }
            catch (TwitterQueryException tqEx)
            {
                if (tqEx.ErrorCode == 88)
                {

                }
            }

            return meleeWinner;
        }

        public DateTime GetLatestTweet(TwitterContext twitterCtx, string twitterUserId)
        {
            var statusTweets =
                from tweet in twitterCtx.Status
                where tweet.Type == StatusType.User
                      && tweet.UserID == twitterUserId
                select tweet;

            Status lastTweet = statusTweets.FirstOrDefault();

            return lastTweet.CreatedAt;
        }

        private int GetTwitterFollowerFriendCount(TwitterContext twitterCtx, string twitterUserId, SocialGraphType graphType)
        {
            var list =
                                (from friend in twitterCtx.SocialGraph
                                 where friend.Type == graphType &&
                                       friend.UserID == Convert.ToUInt64(twitterUserId)
                                 select friend)
                                 .SingleOrDefault();

            return list.IDs.Count;
        }

        private int GetStatusUpdateCount(TwitterContext twitterCtx, string twitterUserId)
        {
            var tUser =
                    (from user in twitterCtx.User
                     where user.Type == UserType.Lookup &&
                     user.UserID == twitterUserId
                     select user).FirstOrDefault();

            return tUser.StatusesCount;

        }

        private int GetLikesFavouritesCount(TwitterContext twitterCtx, string twitterUserId)
        {
            var tUser =
                        (from user in twitterCtx.User
                         where user.Type == UserType.Lookup &&
                         user.UserID == twitterUserId
                         select user).FirstOrDefault();

            return tUser.FavoritesCount;

        }

        private double CalcMeleeResults(MeleeResultModel results)
        {
            var totalResults = (1 * results.FollowerScore) + (.5 * results.FriendScore) + (.25 * results.PostScore);
            return totalResults;
        }

        public TwitterContext GetTwitterContext()
        {
            TwitterContext twitterCtx = null;
            try
            {
                IOAuthCredentials credentials = new SessionStateCredentials();
                MvcAuthorizer auth = GetAuthorizer();



                auth.CompleteAuthorization(Request.Url);

                twitterCtx = new TwitterContext(auth);
            }
            catch (TwitterQueryException tqEx)
            {
                var msg = tqEx.ToString();
            }
            return twitterCtx;
        }

        public MvcAuthorizer GetAuthorizer()
        {
            IOAuthCredentials credentials = new SessionStateCredentials();

            if (credentials.ConsumerKey == null || credentials.ConsumerSecret == null)
            {
                credentials.ConsumerKey = "u2ULchA68sGq111YWL2foA";
                credentials.ConsumerSecret = "JcXAVK1GHaFMXtRLZASIviwDhQvtOLliaMKYfO0rY";
            }

            var auth = new MvcAuthorizer
                {
                    Credentials = credentials
                };

            return auth;
        }
    }
}
