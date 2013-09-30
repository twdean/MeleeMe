using System;
using System.Linq;
using System.Web.Mvc;
using LinqToTwitter;
using Melee.Me.Infrastructure;
using Melee.Me.Infrastructure.Repository;
using Melee.Me.Models;

namespace Melee.Me.Controllers
{
    public class MeleeController : Controller
    {
        private readonly MeleeRepository _repository;

        public MeleeController()
        {
            _repository = new MeleeRepository();
        }


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
                    challengerResult.RecentConnectionScore = GetLatestTweet(twitterCtx, challenger);

                    opponentResult.FriendScore = GetTwitterFollowerFriendCount(twitterCtx, opponent, SocialGraphType.Friends);
                    opponentResult.FollowerScore = GetTwitterFollowerFriendCount(twitterCtx, opponent, SocialGraphType.Followers);
                    opponentResult.PostScore = GetStatusUpdateCount(twitterCtx, opponent);
                    opponentResult.LikeFavouriteScore = GetLikesFavouritesCount(twitterCtx, opponent);
                    opponentResult.RecentConnectionScore = GetLatestTweet(twitterCtx, opponent);
                }
                catch (TwitterQueryException tqEx)
                {
                    if (tqEx.ErrorCode == 88)
                    {

                    }
                }


                if (CalcMeleeResults(challengerResult) > CalcMeleeResults(opponentResult))
                {
                    meleeWinner = (UserModel)Session["challenger"];
                    meleeLoser = (UserModel)Session["competitor"];
                }
                else
                {
                    meleeWinner = (UserModel)Session["competitor"];
                    meleeLoser = (UserModel)Session["challenger"];
                }

                _repository.Add(challenger, opponent, meleeWinner.TwitterUserId, meleeLoser.TwitterUserId);

                Session["challenger"] = null;
                Session["competitor"] = null;
            }
            catch (TwitterQueryException tqEx)
            {
                if (tqEx.ErrorCode == 88)
                {

                }
            }

            return meleeWinner;
        }

        public double GetLatestTweet(TwitterContext twitterCtx, string twitterUserId)
        {
            var statusTweets =
                from tweet in twitterCtx.Status
                where tweet.Type == StatusType.User
                      && tweet.UserID == twitterUserId
                select tweet;

            Status lastTweet = statusTweets.FirstOrDefault();

            //Todays date - that date? = N / 100-n = score
            var today = DateTime.Now;
            var lastTweetDate = lastTweet.CreatedAt;
            var dateVariance = (today - lastTweetDate).TotalDays;

            var tweetScore = dateVariance / (100 - dateVariance);

            return tweetScore;
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
            var totalResults = (1 * results.FollowerScore) +
                               (0.5 * results.FriendScore) +
                               (0.25 * results.PostScore) +
                               (results.RecentConnectionScore) +
                               (3 * results.LikeFavouriteScore);

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
