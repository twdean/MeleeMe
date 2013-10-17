using System;
using System.Linq;
using LinqToTwitter;
using Melee.Me.Models;

namespace Melee.Me.Infrastructure.Connection
{
    public class TwitterConnection : IConnection
    {
        public TwitterContext TwitterCtx { get; set; }

        public TwitterConnection()
        {
            MvcAuthorizer auth = GetAuthorizer();
            TwitterCtx = new TwitterContext(auth);
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

        public double GetScore(UserModel meleeUser)
        {
            double score =  (1 * GetTwitterFollowerCount(meleeUser));
            score += (0.5 * GetTwitterFriendCount(meleeUser));
            score += (0.25 * GetStatusUpdateCount(meleeUser));
            score += (3 * GetFavouritesCount(meleeUser));
            score += (1 * GetLatestTweet(meleeUser));

            return score;
        }

        private int GetTwitterFollowerCount(UserModel meleeUser)
        {
            var list =
                                (from friend in TwitterCtx.SocialGraph
                                 where friend.Type == SocialGraphType.Followers &&
                                       friend.UserID == Convert.ToUInt64(meleeUser.TwitterUserId)
                                 select friend)
                                 .SingleOrDefault();

            return list != null ? list.IDs.Count : 0;             
        }

        private int GetTwitterFriendCount(UserModel meleeUser)
        {
            var list =
                                (from friend in TwitterCtx.SocialGraph
                                 where friend.Type == SocialGraphType.Friends &&
                                       friend.UserID == Convert.ToUInt64(meleeUser.TwitterUserId)
                                 select friend)
                                 .SingleOrDefault();

            return list != null ? list.IDs.Count : 0;
        }

        private int GetStatusUpdateCount(UserModel meleeUser)
        {
            var tUser =
                    (from user in TwitterCtx.User
                     where user.Type == UserType.Lookup &&
                     user.UserID == meleeUser.TwitterUserId
                     select user).FirstOrDefault();

            int retVal = tUser != null ? tUser.StatusesCount : 0;

            return retVal;
        }

        private int GetFavouritesCount(UserModel meleeUser)
        {
            var tUser =
                        (from user in TwitterCtx.User
                         where user.Type == UserType.Lookup &&
                         user.UserID == meleeUser.TwitterUserId
                         select user).FirstOrDefault();

            return tUser != null ? tUser.FavoritesCount : 0;

        }

        private double GetLatestTweet(UserModel meleeUser)
        {
            var statusTweets =
                from tweet in TwitterCtx.Status
                where tweet.Type == StatusType.User
                      && tweet.UserID == meleeUser.TwitterUserId
                select tweet;

            Status lastTweet = statusTweets.FirstOrDefault();

            //Todays date - that date? = N / 100-n = score
            var today = DateTime.Now;
            var lastTweetDate = lastTweet.CreatedAt;
            var dateVariance = (today - lastTweetDate).TotalDays;

            var tweetScore = dateVariance / (100 - dateVariance);

            return tweetScore;
        }
    }
}