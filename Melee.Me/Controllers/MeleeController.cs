using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

            try
            {
                string winner;
                string loser;

                var challengerTotal = 0;
                var opponentTotal = 0;

                var challengerResult = new MeleeResultModel();
                var opponentResult = new MeleeResultModel();

                challengerResult.FriendScore = GetTwitterFollowerFriendCount(twitterCtx, challenger, SocialGraphType.Friends);
                challengerResult.FollowerScore = GetTwitterFollowerFriendCount(twitterCtx, challenger, SocialGraphType.Followers);
                challengerResult.PostScore = GetStatusUpdateCount(twitterCtx, challenger);

                opponentResult.FriendScore = GetTwitterFollowerFriendCount(twitterCtx, opponent, SocialGraphType.Friends);
                opponentResult.FollowerScore = GetTwitterFollowerFriendCount(twitterCtx, opponent, SocialGraphType.Followers);
                opponentResult.PostScore = GetStatusUpdateCount(twitterCtx, opponent);



                DateTime cTweet = GetLatestTweet(twitterCtx, challenger);
                DateTime oTweet = GetLatestTweet(twitterCtx, opponent);

                if (CalcMeleeResults(challengerResult) > CalcMeleeResults(opponentResult))
                {
                    winner = challenger;
                    loser = opponent;
                }
                else
                {
                    winner = opponent;
                    loser = challenger;
                }

                MeleeModel.AddMelee(challenger, opponent, winner, loser);


                var tUser =
                    (from user in twitterCtx.User
                     where user.Type == UserType.Lookup &&
                           user.UserID == winner
                     select user).FirstOrDefault();

                meleeWinner = new UserModel()
                    {
                        ImageUrl = tUser.ProfileImageUrl,
                        ScreenName = tUser.Identifier.ScreenName,
                        TwitterUserId = tUser.Identifier.UserID,
                    };

            }
            catch (TwitterQueryException tqEx)
            {
                if (tqEx.ErrorCode == 88)
                {

                }
            }
            catch (Exception ex)
            {

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

        private int CalcMeleeResults(MeleeResultModel results)
        {
            var totalResults = results.FriendScore + results.FollowerScore;
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
}
