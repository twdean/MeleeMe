using System;
using System.Linq;
using LinqToTwitter;
using Melee.Me.Models;

namespace Melee.Me.Infrastructure.Connection
{
    public class TwitterConnection : IConnection
    {
        public TwitterContext TwitterCtx { get; set; }
        public MvcAuthorizer Authorizer { get; set; }

        public MvcAuthorizer GetAuthorizer(UserModel meleeUser)
        {
            IOAuthCredentials credentials = new InMemoryCredentials();

            if (credentials.ConsumerKey == null || credentials.ConsumerSecret == null)
            {
                credentials.ConsumerKey = "u2ULchA68sGq111YWL2foA";
                credentials.ConsumerSecret = "JcXAVK1GHaFMXtRLZASIviwDhQvtOLliaMKYfO0rY";
            }

            if (meleeUser != null)
            {
                credentials.AccessToken = meleeUser.Connections.Single(x => x.ConnectionName == "Twitter").AccessToken;
                credentials.OAuthToken = meleeUser.Connections.Single(x => x.ConnectionName == "Twitter").OAuthToken;
            }

            var auth = new MvcAuthorizer
            {
                Credentials = credentials
            };

            return auth;
        }

        public double GetScore(UserModel meleeUser)
        {
            Authorizer = GetAuthorizer(meleeUser);
            TwitterCtx = new TwitterContext(Authorizer);

            double score = (1 * GetTwitterFollowerCount(meleeUser.TwitterUserId));
            score += (0.5 * GetTwitterFriendCount(meleeUser.TwitterUserId));
            score += (0.25 * GetStatusUpdateCount(meleeUser.TwitterUserId));
            score += (3 * GetFavouritesCount(meleeUser.TwitterUserId));
            score += (1 * GetLatestTweet(meleeUser.TwitterUserId));

            return score;
        }

        public double GetScore(string twitterId)
        {
            Authorizer = GetAuthorizer(null);
            TwitterCtx = new TwitterContext(Authorizer);

            double score = (1 * GetTwitterFollowerCount(twitterId));
            score += (0.5 * GetTwitterFriendCount(twitterId));
            score += (0.25 * GetStatusUpdateCount(twitterId));
            score += (3 * GetFavouritesCount(twitterId));
            score += (1 * GetLatestTweet(twitterId));

            return score;
        }


        private int GetTwitterFollowerCount(string twitterId)
        {
            var list =
                                (from friend in TwitterCtx.SocialGraph
                                 where friend.Type == SocialGraphType.Followers &&
                                       friend.UserID == Convert.ToUInt64(twitterId)
                                 select friend)
                                 .SingleOrDefault();

            return list != null ? list.IDs.Count : 0;
        }

        private int GetTwitterFriendCount(string twitterId)
        {
            var list =
                                (from friend in TwitterCtx.SocialGraph
                                 where friend.Type == SocialGraphType.Friends &&
                                       friend.UserID == Convert.ToUInt64(twitterId)
                                 select friend)
                                 .SingleOrDefault();

            return list != null ? list.IDs.Count : 0;
        }

        private int GetStatusUpdateCount(string twitterId)
        {
            var tUser =
                    (from user in TwitterCtx.User
                     where user.Type == UserType.Lookup &&
                     user.UserID == twitterId
                     select user).FirstOrDefault();

            int retVal = tUser != null ? tUser.StatusesCount : 0;

            return retVal;
        }

        private int GetFavouritesCount(string twitterId)
        {
            var tUser =
                        (from user in TwitterCtx.User
                         where user.Type == UserType.Lookup &&
                         user.UserID == twitterId
                         select user).FirstOrDefault();

            return tUser != null ? tUser.FavoritesCount : 0;

        }

        private double GetLatestTweet(string twitterId)
        {
            var statusTweets =
                from tweet in TwitterCtx.Status
                where tweet.Type == StatusType.User
                      && tweet.UserID == twitterId
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