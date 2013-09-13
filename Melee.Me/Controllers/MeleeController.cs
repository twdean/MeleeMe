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
                DateTime cTweet = GetLatestTweet(twitterCtx, challenger);
                DateTime oTweet = GetLatestTweet(twitterCtx, opponent);
                string winner;
                string loser;

                if (cTweet > oTweet)
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
                var message = tqEx.ToString();
            }
            catch (Exception ex)
            {
                var exMsg = ex.ToString();
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

            //List<Status> tweets = new List<Status>();
            //   statusTweets.ToList().ForEach(
            //    tweets.Add);

            return lastTweet.CreatedAt;

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
